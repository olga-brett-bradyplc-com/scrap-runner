using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Util;
using BWF.DataServices.Core.Concrete.ChangeSets;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;
using BWF.DataServices.Core.Interfaces;
using BWF.DataServices.Core.Models;
using BWF.DataServices.Domain.Models;
using BWF.DataServices.Metadata.Models;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.DataService.Interfaces;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.DataService.Util;

namespace Brady.ScrapRunner.DataService.ProcessTypes
{
    /// <summary>
    /// Processing for a driver completing an action on a container.  Call this process "withoutrequery".
    /// </summary> 
    /// Note this processes is relatively independent of the "trivial" backing query and results
    /// are simply built up in memory.  As such, make this service call using the form of 
    /// PUT .../{dataServiceName}/{typeName}/{id}/withoutrequery
    /// 
    /// cURL example: 
    ///     PUT https://maunb-stm10.bradyplc.com:7776//api/scraprunner/DriverContainerDoneProcess/001/withoutrequery
    /// Portable Client example: 
    ///     var updateResult = client.UpdateAsync(itemToUpdate, requeryUpdated:false).Result;
    ///  
    /// This mode will prevent the Nancy.DataServiceModule from issuing an automatic re-retrieve via getSingleAsync() 
    /// within the postSingleAsync().  These re-retrieves of a trival query clobber our post-processed ChangeSetResult
    /// in memory.

    [EditAction("DriverContainerDoneProcess")]
    public class DriverContainerDoneProcessRecordType : ChangeableRecordType
        <DriverContainerDoneProcess, string, DriverContainerDoneProcessValidator, DriverContainerDoneProcessDeletionValidator>
    {
        /// <summary>
        /// Mandatory implementation of virtual base class method.
        /// </summary>
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<DriverContainerDoneProcess, DriverContainerDoneProcess>();
        }

        /// <summary>
        /// This is the deprecated signature. 
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="token"></param>
        /// <param name="username"></param>
        /// <param name="changeSet"></param>
        /// <param name="persistChanges"></param>
        /// <returns></returns>
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService, string token, string username,
                        ChangeSet<string, DriverContainerDoneProcess> changeSet, bool persistChanges)
        {
            return ProcessChangeSet(dataService, changeSet, new ProcessChangeSetSettings(token, username, persistChanges));
        }
        /// <summary>
        /// Perform the driver arrive processing.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="changeSet"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService,
                        ChangeSet<string, DriverContainerDoneProcess> changeSet, ProcessChangeSetSettings settings)
        {
            ISession session = null;
            ITransaction transaction = null;

            // If session isn't passed in and changes are being persisted then open a new session
            if (settings.Session == null && settings.PersistChanges)
            {
                var srRepository = (ISRRepository)repository;
                session = srRepository.OpenSession();
                transaction = session.BeginTransaction();
                settings.Session = session;
            }

            // Running the base process changeset first in this case.  This should give up the benefit of validators, 
            // auditing, security, pipelines, etc. 
            ChangeSetResult<string> changeSetResult = base.ProcessChangeSet(dataService, changeSet, settings);

            // If no problems, we are free to process.
            // We only process one driver arrive at a time but in the more general cases we could be processing multiple records.
            // So we loop over the one to many keys in the changeSetResult.SuccessfullyUpdated
            if (!changeSetResult.FailedCreates.Any() && !changeSetResult.FailedUpdates.Any() &&
                !changeSetResult.FailedDeletions.Any())
            {
                foreach (String key in changeSetResult.SuccessfullyUpdated)
                {
                    DataServiceFault fault;
                    string msgKey = key;

                    int containerHistoryInsertCount = 0;

                    var driverContainerDoneProcess = (DriverContainerDoneProcess)changeSetResult.GetSuccessfulUpdateForId(key);
 
                    // TODO:  Determine userCulture and userRoleIds on a per user basis.
                    string userCulture = "en-GB";
                    IEnumerable<long> userRoleIds = Enumerable.Empty<long>().ToList();

                    // It appears, in the general case, I may need to backfill any additional user input values other than driverID.
                    // They will get clobbered by the call to the base process method.
                    DriverContainerDoneProcess backfillDriverContainerDoneProcess;
                    if (changeSet.Update.TryGetValue(key, out backfillDriverContainerDoneProcess))
                    {
                        // Generally use a mapper?  May not always be the best approach.
                        Mapper.Map(backfillDriverContainerDoneProcess, driverContainerDoneProcess);
                    }
                    else
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Unable to process container done for DriverId: " 
                                        + driverContainerDoneProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Validate driver id / Get the EmployeeMaster record
                    var employeeMaster = Common.GetEmployeeDriver(dataService, settings, userCulture, userRoleIds,
                                         driverContainerDoneProcess.EmployeeId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == employeeMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid DriverId: " 
                                        + driverContainerDoneProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Get the Trip record
                    var tripRecord = Common.GetTrip(dataService, settings, userCulture, userRoleIds,
                                     driverContainerDoneProcess.TripNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == tripRecord)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid TripNumber: " 
                                        + driverContainerDoneProcess.TripNumber));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Check if trip is complete
                    if (Common.IsTripComplete(tripRecord))
                    {
                        log.DebugFormat("SRTEST:TripNumber:{0} is Complete. Container done processing ends.",
                                        driverContainerDoneProcess.TripNumber);
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Get a list of all incomplete segments for the trip
                    var tripSegList = Common.GetTripSegmentsIncomplete(dataService, settings, userCulture, userRoleIds,
                                      driverContainerDoneProcess.TripNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Get the current TripSegment record
                    var tripSegmentRecord = (from item in tripSegList
                                             where item.TripSegNumber == driverContainerDoneProcess.TripSegNumber
                                             select item).FirstOrDefault();
                    if (null == tripSegmentRecord)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid TripSegment: " +
                            driverContainerDoneProcess.TripNumber + "-" + driverContainerDoneProcess.TripSegNumber));
                        break;
                    }
                    ////////////////////////////////////////////////
                    //Get a list of all containers for the segment
                    var tripSegContainerList = Common.GetTripSegmentContainers(dataService, settings, userCulture, userRoleIds,
                                               driverContainerDoneProcess.TripNumber, driverContainerDoneProcess.TripSegNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    ////////////////////////////////////////////////
                    // Get the Customer record for the destination cust host code
                    var destCustomerMaster = Common.GetCustomer(dataService, settings, userCulture, userRoleIds,
                                             tripSegmentRecord.TripSegDestCustHostCode, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == destCustomerMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid CustHostCode: "
                                        + tripSegmentRecord.TripSegDestCustHostCode));
                        break;
                    }
                    ////////////////////////////////////////////////
                    //Define the TripSegmentContainer for use later
                    TripSegmentContainer tripSegmentContainerRecord = new TripSegmentContainer();

                    ////////////////////////////////////////////////
                    //Update Container Information
                    var containerMaster = Common.GetContainer(dataService, settings, userCulture, userRoleIds,
                                          driverContainerDoneProcess.ContainerNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == containerMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid ContainerNumber: " 
                                        + driverContainerDoneProcess.ContainerNumber));
                        break;
                    }
                    //Check the QtyInID Flag to see if we have do process the container differently
                    if (containerMaster.ContainerQtyInIDFlag == Constants.Yes)
                    {
                        //ContainerNumber actually containe the quantity of containers and not a real container number
                        //In this case we would want to update the ContainerQuantity and ContainerQuantityHIstory tables
                        //instead of the ContainerMaster and ContainerHistory tables
                        //But only for drops and pickups....
                        var types = new List<string> {BasicTripTypeConstants.DropFull,
                                                  BasicTripTypeConstants.DropEmpty,
                                                  BasicTripTypeConstants.PickupFull,
                                                  BasicTripTypeConstants.DropEmpty};
                        if (types.Contains(tripSegmentRecord.TripSegType))
                        {
                            //TODO: Call a separate routine
                        }
                    }
                    else
                    {
                        //Do not alter these values
                        //ContainerType,ContainerSize,ContainerTerminalId,ContainerRegionId

                        //Based on the segment type, container may or may not still be on the PowerId.
                        if (Common.IsContainerOnPowerId(tripSegmentRecord.TripSegType,
                            driverContainerDoneProcess.SetInYardFlag, driverContainerDoneProcess.ActionType))
                        {
                            containerMaster.ContainerPowerId = driverContainerDoneProcess.PowerId;
                        }
                        else
                        {
                            containerMaster.ContainerPowerId = null;
                        }

                        //Set the status based on the destination customer type
                        if (tripSegmentRecord.TripSegDestCustType == CustomerTypeConstants.Yard)
                        {
                            containerMaster.ContainerStatus = ContainerStatusConstants.Yard;
                        }
                        else
                        {
                            containerMaster.ContainerStatus = ContainerStatusConstants.CustomerSite;
                            containerMaster.ContainerPrevCustHostCode = tripSegmentRecord.TripSegDestCustHostCode;
                        }
                        containerMaster.ContainerCurrentTripNumber = driverContainerDoneProcess.TripNumber;
                        containerMaster.ContainerPrevTripNumber = driverContainerDoneProcess.TripNumber;
                        containerMaster.ContainerCurrentTripSegNumber = driverContainerDoneProcess.TripSegNumber;
                        containerMaster.ContainerCurrentTripSegType = tripSegmentRecord.TripSegType;
                        containerMaster.ContainerLastActionDateTime = driverContainerDoneProcess.ActionDateTime;
                        containerMaster.ContainerCustHostCode = tripSegmentRecord.TripSegDestCustHostCode;
                        containerMaster.ContainerCustType = tripSegmentRecord.TripSegDestCustType;

                        //TODO: Check code. 
                        //Determine when and where this should be set to null
                        containerMaster.ContainerPendingMoveDateTime = null;

                        containerMaster.ContainerContents = driverContainerDoneProcess.ContainerContents;
                        containerMaster.ContainerCommodityCode = driverContainerDoneProcess.CommodityCode;
                        containerMaster.ContainerCommodityDesc = driverContainerDoneProcess.CommodityDesc;
                        containerMaster.ContainerLocation = driverContainerDoneProcess.ContainerLocation;
                        containerMaster.ContainerLevel = driverContainerDoneProcess.ContainerLevel;
                        containerMaster.ContainerLatitude = driverContainerDoneProcess.Latitude;
                        containerMaster.ContainerLongitude = driverContainerDoneProcess.Longitude;

                        containerMaster.ContainerCurrentTerminalId = destCustomerMaster.ServingTerminalId;
                        if (tripSegmentRecord.TripSegType == BasicTripTypeConstants.ReturnYard)
                        {
                            containerMaster.ContainerInboundTerminalId = destCustomerMaster.ServingTerminalId;
                        }

                        /////////////////////////////////////////////////
                        //Set the location warning flag
                        containerMaster.LocationWarningFlag = null;
                        var types = new List<string> {BasicTripTypeConstants.DropFull,
                                                      BasicTripTypeConstants.DropEmpty,
                                                      BasicTripTypeConstants.Unload,
                                                      BasicTripTypeConstants.Load,
                                                      BasicTripTypeConstants.Respot};

                        if (types.Contains(tripSegmentRecord.TripSegType))
                        {
                            //We need to look in the container history to find the previous trip and the last 
                            //status of the container at the end of that trip.
                            //GetContainerPreviousTrip(sContNum, sTripNum, sPrevContainerStatus, sPrevTripNum, sPrevCustHostCode);
                            
                            var containerLastTrip = Common.GetContainerHistoryLastTrip(dataService, settings, userCulture, userRoleIds,
                                          driverContainerDoneProcess.ContainerNumber, out fault);
                            if (null != fault)
                            {
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                                break;
                            }
                            var statuses = new List<string> {ContainerStatusConstants.CustomerSite,
                                                             ContainerStatusConstants.Contractor,
                                                             ContainerStatusConstants.SpecialProject };
                            if (statuses.Contains(containerLastTrip.ContainerStatus))
                            {
                                containerMaster.LocationWarningFlag = Constants.Yes;
                            }
                            else
                            {
                                containerMaster.LocationWarningFlag = Constants.No;
                            }
                        }

                        //Do the update
                        changeSetResult = Common.UpdateContainerMaster(dataService, settings, containerMaster);
                        log.DebugFormat("SRTEST:Saving ContainerMaster Record for ContainerNumber:{0} - Arrive.",
                                        driverContainerDoneProcess.ContainerNumber);
                        if (Common.LogChangeSetFailure(changeSetResult, containerMaster, log))
                        {
                            var s = string.Format("Could not update ContainerMaster for ContainerNumber:{0}.",
                                        driverContainerDoneProcess.ContainerNumber);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }

                        ////////////////////////////////////////////////
                        //Add record to Container History. 
                        if (!Common.InsertContainerHistory(dataService, settings, containerMaster,
                            ++containerHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            log.ErrorFormat("InsertContainerHistory failed: {0} during container done request: {1}", fault.Message,
                                             driverContainerDoneProcess);
                            break;
                        }
                    }

                    ////////////////////////////////////////////////
                    //Update TripSegmentContainer table.
                    if (null != tripSegContainerList && tripSegContainerList.Count() > 0)
                    {
                        //First, try to find a container in the list that matches the container number on the power unit.
                        tripSegmentContainerRecord = (from item in tripSegContainerList
                                                      where item.TripSegContainerNumber == containerMaster.ContainerNumber
                                                      select item).FirstOrDefault();
                        if (null == tripSegmentContainerRecord)
                        {
                            //Otherwise find a container record with no container number
                            tripSegmentContainerRecord = (from item in tripSegContainerList
                                                          where item.TripSegContainerNumber == null
                                                          select item).FirstOrDefault();
                        }
                    }
                    //If still not found, then add a new one
                    if (null == tripSegmentContainerRecord)
                    {
                        //Set up a new record
                        tripSegmentContainerRecord = new TripSegmentContainer();

                        //Look up the last trip segment container for this trip and segment
                        //Use this to calculate the next sequence number.
                        var tripSegmentContainerMax = Common.GetTripSegmentContainerLast(dataService, settings, userCulture, userRoleIds,
                                                      driverContainerDoneProcess.TripNumber, driverContainerDoneProcess.TripSegNumber, out fault);
                        if (null != fault)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        //Determine the next sequence number
                        if (null == tripSegmentContainerMax)
                        {
                            tripSegmentContainerRecord.TripSegContainerSeqNumber = 0;
                        }
                        else
                        {
                            tripSegmentContainerRecord.TripSegContainerSeqNumber = ++tripSegmentContainerMax.TripSegContainerSeqNumber;
                        }
                        tripSegmentContainerRecord.TripNumber = driverContainerDoneProcess.TripNumber;
                        tripSegmentContainerRecord.TripSegNumber = driverContainerDoneProcess.TripSegNumber;
                    }
                    //Container number from the mobile device is required.
                    tripSegmentContainerRecord.TripSegContainerNumber = driverContainerDoneProcess.ContainerNumber;
                    
                    //Action Date/Time from the mobile device is required.
                    tripSegmentContainerRecord.TripSegContainerActionDateTime = driverContainerDoneProcess.ActionDateTime;
                    
                    //Use the type and size from the container master, not the mobile device.
                    tripSegmentContainerRecord.TripSegContainerType = containerMaster.ContainerType;
                    tripSegmentContainerRecord.TripSegContainerSize = containerMaster.ContainerSize;

                    //Comodity code and description are optional, from driver. May have been filled in by dispatch. 
                    if (driverContainerDoneProcess.CommodityCode != null && driverContainerDoneProcess.CommodityDesc != null)
                    {
                        tripSegmentContainerRecord.TripSegContainerCommodityCode = driverContainerDoneProcess.CommodityCode;
                        tripSegmentContainerRecord.TripSegContainerCommodityDesc = driverContainerDoneProcess.CommodityDesc;
                    }

                    //Location is optional, from driver. May have been filled in by dispatch. 
                    if (driverContainerDoneProcess.ContainerLocation != null)
                    {
                        tripSegmentContainerRecord.TripSegContainerLocation = driverContainerDoneProcess.ContainerLocation;
                    }

                    //Level is optional, as determined by a preference.
                    tripSegmentContainerRecord.TripSegContainerLevel = driverContainerDoneProcess.ContainerLevel;
                    
                    //If latitude/longitude not provided from driver, use the lat/lon for the destination customer.
                    if (driverContainerDoneProcess.Latitude == null || driverContainerDoneProcess.Longitude == null)
                    {
                        tripSegmentContainerRecord.TripSegContainerLatitude = destCustomerMaster.CustLatitude;
                        tripSegmentContainerRecord.TripSegContainerLongitude = destCustomerMaster.CustLongitude;
                    }
                    else
                    {
                        tripSegmentContainerRecord.TripSegContainerLatitude = driverContainerDoneProcess.Latitude;
                        tripSegmentContainerRecord.TripSegContainerLongitude = driverContainerDoneProcess.Longitude;
                    }
                    //This indicates whether the container number was scanned or manually (hand-typed) entered.
                    tripSegmentContainerRecord.TripSegContainerEntryMethod = driverContainerDoneProcess.MethodOfEntry;

                     // The Review Flag and Reason contains either R=Review and the ReviewReason
                    // or E=Exception and the Exception Reason
                    if (driverContainerDoneProcess.ActionType == ActionTypeConstants.Exception ||
                        driverContainerDoneProcess.ActionType == ActionTypeConstants.Review)
                    {
                        tripSegmentContainerRecord.TripSegContainerReviewFlag = driverContainerDoneProcess.ActionType;
                        tripSegmentContainerRecord.TripSegContainerReviewReason = driverContainerDoneProcess.ActionCode
                            + "-" + driverContainerDoneProcess.ActionDesc;
                    }
                    else
                    {
                        tripSegmentContainerRecord.TripSegContainerReviewFlag = ActionTypeConstants.None;
                    }

                    // The purpose of this is field to indicate that processing of this container is complete.
                    // We used to remove all trip segment container records that are not complete.
                    // But we are not doing it that way in the new ScrapRunner.
                    tripSegmentContainerRecord.TripSegContainerComplete = Constants.Yes;

                    //For return to yard processing only...
                    if (tripSegmentRecord.TripSegType == BasicTripTypeConstants.ReturnYard)
                    {
                        //Scale reference number can be captured at gross action or at both gross and tare actions.
                        tripSegmentContainerRecord.TripScaleReferenceNumber = driverContainerDoneProcess.ScaleReferenceNumber;
                        
                        //The ContainerLoaded flag is based on ContainerContents
                        if (driverContainerDoneProcess.ContainerContents == ContainerContentsConstants.Loaded)
                        {
                            tripSegmentContainerRecord.TripSegContainerLoaded = Constants.Yes;
                        }
                        else
                        {
                            tripSegmentContainerRecord.TripSegContainerLoaded = Constants.No;
                        }

                        //The TripSegContainerOnTruck is based on the SetInYardFlag flag
                        if (driverContainerDoneProcess.SetInYardFlag == Constants.Yes)
                        {
                            tripSegmentContainerRecord.TripSegContainerOnTruck = Constants.Yes;
                        }

                        //Gross, 2nd gross (optional) and tare times and weights (optional)
                        tripSegmentContainerRecord.WeightGrossDateTime = driverContainerDoneProcess.Gross1ActionDateTime;
                        tripSegmentContainerRecord.TripSegContainerWeightGross = driverContainerDoneProcess.Gross1Weight;
                        tripSegmentContainerRecord.WeightGross2ndDateTime = driverContainerDoneProcess.Gross2ActionDateTime;
                        tripSegmentContainerRecord.TripSegContainerWeightGross2nd = driverContainerDoneProcess.Gross2Weight;
                        tripSegmentContainerRecord.WeightTareDateTime = driverContainerDoneProcess.TareActionDateTime;
                        tripSegmentContainerRecord.TripSegContainerWeightTare = driverContainerDoneProcess.TareWeight;
                    }

                    //Do the update
                    changeSetResult = Common.UpdateTripSegmentContainer(dataService, settings, tripSegmentContainerRecord);
                    log.DebugFormat("SRTEST:Saving TripSegmentContainerRecord for ContainerNumber:{0} - Container Done.",
                                    tripSegmentContainerRecord.TripSegContainerNumber);
                    if (Common.LogChangeSetFailure(changeSetResult, tripSegmentContainerRecord, log))
                    {
                        var s = string.Format("Could not update TripSegmentContainerRecord for ContainerNumber:{0}.",
                                    tripSegmentContainerRecord.TripSegContainerNumber);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }



                }//end of foreach 
            }//end of if (!changeSetResult.Failed...

            // If our local session variable is set then it is our session/txn to deal with
            // otherwise we simply return the result.
            if (session == null)
            {
                return changeSetResult;
            }

            if (changeSetResult.FailedCreates.Any() || changeSetResult.FailedUpdates.Any() ||
                changeSetResult.FailedDeletions.Any())
            {
                transaction.Rollback();
                log.Debug("SRTEST:Transaction Rollback - Container Done");
            }
            else
            {
                transaction.Commit();
                log.Debug("SRTEST:Transaction Committed - Container Done");
                // We need to notify that data has changed for any types we have updated
                // We always need to notify for the current type
                dataService.NotifyOfExternalChangesToData();
            }
            transaction.Dispose();
            session.Dispose();
            settings.Session = null;

            return changeSetResult;
        }
    }
}

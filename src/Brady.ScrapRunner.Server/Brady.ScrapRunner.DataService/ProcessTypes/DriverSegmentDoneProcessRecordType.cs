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
using Brady.ScrapRunner.Domain.Enums;

namespace Brady.ScrapRunner.DataService.ProcessTypes
{
    /// <summary>
    /// Processing for a driver completing a segment.  Call this process "withoutrequery".
    /// </summary> 
    /// Note this processes is relatively independent of the "trivial" backing query and results
    /// are simply built up in memory.  As such, make this service call using the form of 
    /// PUT .../{dataServiceName}/{typeName}/{id}/withoutrequery
    /// 
    /// cURL example: 
    ///     PUT https://maunb-stm10.bradyplc.com:7776//api/scraprunner/DriverSegmentDoneProcess/001/withoutrequery
    /// Portable Client example: 
    ///     var updateResult = client.UpdateAsync(itemToUpdate, requeryUpdated:false).Result;
    ///  
    /// This mode will prevent the Nancy.DataServiceModule from issuing an automatic re-retrieve via getSingleAsync() 
    /// within the postSingleAsync().  These re-retrieves of a trival query clobber our post-processed ChangeSetResult
    /// in memory.

    [EditAction("DriverSegmentDoneProcess")]
    public class DriverSegmentDoneProcessRecordType : ChangeableRecordType
        <DriverSegmentDoneProcess, string, DriverSegmentDoneProcessValidator, DriverSegmentDoneProcessDeletionValidator>
    {
        /// <summary>
        /// Mandatory implementation of virtual base class method.
        /// </summary>
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<DriverSegmentDoneProcess, DriverSegmentDoneProcess>();
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
                        ChangeSet<string, DriverSegmentDoneProcess> changeSet, bool persistChanges)
        {
            return ProcessChangeSet(dataService, changeSet, new ProcessChangeSetSettings(token, username, persistChanges));
        }
        /// <summary>
        /// Perform the driver "segment done" processing.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="changeSet"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService,
                        ChangeSet<string, DriverSegmentDoneProcess> changeSet, ProcessChangeSetSettings settings)
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
            // We only process one record at a time but in the more general cases we could be processing multiple records.
            // So we loop over the one to many keys in the changeSetResult.SuccessfullyUpdated
            if (!changeSetResult.FailedCreates.Any() && !changeSetResult.FailedUpdates.Any() &&
                !changeSetResult.FailedDeletions.Any())
            {
                foreach (String key in changeSetResult.SuccessfullyUpdated)
                {
                    DataServiceFault fault;
                    string msgKey = key;

                    int driverHistoryInsertCount = 0;

                    var driverSegmentDoneProcess = (DriverSegmentDoneProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    // TODO:  Determine userCulture and userRoleIds on a per user basis.
                    string userCulture = "en-GB";
                    IEnumerable<long> userRoleIds = Enumerable.Empty<long>().ToList();

                    // It appears, in the general case, I may need to backfill any additional user input values other than driverID.
                    // They will get clobbered by the call to the base process method.
                    DriverSegmentDoneProcess backfillDriverSegmentDoneProcess;
                    if (changeSet.Update.TryGetValue(key, out backfillDriverSegmentDoneProcess))
                    {
                        // Generally use a mapper?  May not always be the best approach.
                        Mapper.Map(backfillDriverSegmentDoneProcess, driverSegmentDoneProcess);
                    }
                    else
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Unable to process segment done for DriverId: "
                                + driverSegmentDoneProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Validate driver id / Get the EmployeeMaster record
                    var employeeMaster = Common.GetEmployeeDriver(dataService, settings, userCulture, userRoleIds,
                                         driverSegmentDoneProcess.EmployeeId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == employeeMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid DriverId: "
                                        + driverSegmentDoneProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Get the Trip record
                    var currentTrip = Common.GetTrip(dataService, settings, userCulture, userRoleIds,
                                     driverSegmentDoneProcess.TripNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == currentTrip)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid TripNumber: "
                                        + driverSegmentDoneProcess.TripNumber));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Check if trip is complete
                    if (Common.IsTripComplete(currentTrip))
                    {
                        log.DebugFormat("SRTEST:TripNumber:{0} is Complete. Segment done processing ends.",
                                        driverSegmentDoneProcess.TripNumber);
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Get the PowerMaster record
                    var powerMaster = Common.GetPowerUnit(dataService, settings, userCulture, userRoleIds,
                                      driverSegmentDoneProcess.PowerId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == powerMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid PowerId: "
                                        + driverSegmentDoneProcess.PowerId));
                        break;
                    }
                    //The odometer is not used in the current ScrapRunner processing of a segment done.
                    //Do not use the odometer from the driver if it is less than the last recorded 
                    //odometer stored in the PowerMaster.
                    //if (powerMaster.PowerOdometer != null)
                    //{
                    //    if (driverSegmentDoneProcess.Odometer < powerMaster.PowerOdometer)
                    //    {
                    //        driverSegmentDoneProcess.Odometer = (int)powerMaster.PowerOdometer;
                    //    }
                    //}

                    ////////////////////////////////////////////////
                    //Get a list of all segments for the trip
                    var tripSegList = Common.GetTripSegments(dataService, settings, userCulture, userRoleIds,
                                      driverSegmentDoneProcess.TripNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Get the current TripSegment record
                    var currentTripSegment = (from item in tripSegList
                                             where item.TripSegNumber == driverSegmentDoneProcess.TripSegNumber
                                             select item).FirstOrDefault();
                    if (null == currentTripSegment)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid TripSegment: " +
                            driverSegmentDoneProcess.TripNumber + "-" + driverSegmentDoneProcess.TripSegNumber));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Get a list of incomplete containers for the segment
                    var incompleteTripSegContainerList = Common.GetTripSegmentContainersIncomplete(dataService, settings, userCulture, userRoleIds,
                                               driverSegmentDoneProcess.TripNumber, driverSegmentDoneProcess.TripSegNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (incompleteTripSegContainerList != null)
                    {
                        foreach (var incompleteTripSegmentContainer in incompleteTripSegContainerList)
                        {
                            //TODO: Do the delete. DeleteTripSegmentContainer throws an exception
                            //changeSetResult = Common.DeleteTripSegmentContainer(dataService, settings, incompleteTripSegmentContainer);
                            log.DebugFormat("SRTEST:Deleting TripSegmentContainer Record for Trip:{0}-{1} Container:{2}- Segment Done.",
                                            incompleteTripSegmentContainer.TripNumber, incompleteTripSegmentContainer.TripSegNumber,
                                            incompleteTripSegmentContainer.TripSegContainerNumber);
                            if (Common.LogChangeSetFailure(changeSetResult, incompleteTripSegmentContainer, log))
                            {
                                var s = string.Format("Could not update TripSegmentContainer for Trip:{0}-{1} Container:{2}.",
                                        incompleteTripSegmentContainer.TripNumber, incompleteTripSegmentContainer.TripSegNumber,
                                        incompleteTripSegmentContainer.TripSegContainerNumber);
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                break;
                            }
                        }
                    }

                    ////////////////////////////////////////////////
                    //Get a list of all containers for the segment
                    var tripSegContainerList = Common.GetTripSegmentContainers(dataService, settings, userCulture, userRoleIds,
                                               driverSegmentDoneProcess.TripNumber, driverSegmentDoneProcess.TripSegNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    //Delete any containers that have not been completed.
                    //var incompleteTripSegContainerList = (from c in tripSegContainerList
                    //                                     where (c.TripSegContainerComplete == null
                    //                                     || c.TripSegContainerComplete != Constants.Yes)
                    //                                     select c).ToList();

                    ////////////////////////////////////////////////
                    //Include just the complete containers in the list
                    tripSegContainerList = (from c in tripSegContainerList
                                           where c.TripSegContainerComplete == Constants.Yes
                                           select c).ToList();

                    ////////////////////////////////////////////////
                    // Get the Customer record for the destination cust host code
                    var destCustomerMaster = Common.GetCustomer(dataService, settings, userCulture, userRoleIds,
                                     currentTripSegment.TripSegDestCustHostCode, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == destCustomerMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid CustHostCode: "
                                        + currentTripSegment.TripSegDestCustHostCode));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Start Updating...
                    ////////////////////////////////////////////////
                    //Update the TripSegment record.
                    currentTripSegment.TripSegPowerId = driverSegmentDoneProcess.PowerId;
                    currentTripSegment.TripSegPowerAssetNumber = powerMaster.PowerAssetNumber;
                    currentTripSegment.TripSegDriverId = driverSegmentDoneProcess.EmployeeId;
                    currentTripSegment.TripSegDriverName = Common.GetDriverName(employeeMaster);
                    currentTripSegment.TripSegEndLatitude = driverSegmentDoneProcess.Latitude;
                    currentTripSegment.TripSegEndLongitude = driverSegmentDoneProcess.Longitude;

                    //The odometer is not used in the current ScrapRunner processing of a segment done.
                    //currentTripSegment.TripSegOdometerEnd = driverSegmentDoneProcess.Odometer;

                    //Segment End Time will be overwritten when driver goes enroute on next segment
                    currentTripSegment.TripSegEndDateTime = driverSegmentDoneProcess.ActionDateTime;

                    //Stop End Time will be overwritten when driver goes enroute on next segment
                    //Driver has completed segment. Stop time ends.
                    currentTripSegment.TripSegActualStopEndDateTime = driverSegmentDoneProcess.ActionDateTime;
                    //Calculate the stop minutes: StopEndDateTime - StopStartDateTime
                    if (currentTripSegment.TripSegActualStopStartDateTime != null && currentTripSegment.TripSegActualStopEndDateTime != null)
                    {
                        currentTripSegment.TripSegActualStopMinutes = (int)(currentTripSegment.TripSegActualStopEndDateTime.Value.Subtract
                                  (currentTripSegment.TripSegActualStopStartDateTime.Value).TotalMinutes);
                    }
                    else
                    {
                        currentTripSegment.TripSegActualStopMinutes = 0;
                    }

                    //Get the number of containers on this segment
                    currentTripSegment.TripSegContainerQty = tripSegContainerList.Count();

                    currentTripSegment.TripSegDriverGenerated = driverSegmentDoneProcess.DriverGenerated;
                    currentTripSegment.TripSegDriverModified = driverSegmentDoneProcess.DriverModified;

                    //////////////////////////////////////////////////////
                    //Determine the trip segment status
                    //If the ActionType of any container on the segment is R=Review, then the trip seg status is R=Review
                    //If the ActionType of any container on the segment is E=Exception, then the trip seg status is E=Exception
                    //Otherwise the trip seg status is D=Done
                    currentTripSegment.TripSegStatus = TripSegStatusConstants.Done;
                    //If any container is in review, set segment status to R=Review
                    var review = from item in tripSegContainerList
                                 where item.TripSegContainerReviewFlag == ActionTypeConstants.Review
                                 select item;
                    if (review != null && review.Count() > 0)
                    {
                        currentTripSegment.TripSegStatus = TripSegStatusConstants.Review;

                    }
                    else
                    {
                        //If any container is an exception, set segment status to E=Exception
                        var exception = from item in tripSegContainerList
                                        where item.TripSegContainerReviewFlag == ActionTypeConstants.Exception
                                        select item;
                        if (exception != null && exception.Count() > 0)
                        {
                            currentTripSegment.TripSegStatus = TripSegStatusConstants.Exception;

                        }
                    }

                    //Lookup the TripSegStatus Description in the CodeTable TRIPSEGSTATUS 
                    var codeTableTripSegStatus = Common.GetCodeTableEntry(dataService, settings, userCulture, userRoleIds,
                                                 CodeTableNameConstants.TripSegStatus, currentTripSegment.TripSegStatus, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    currentTripSegment.TripSegStatusDesc = codeTableTripSegStatus?.CodeDisp1;

                    //////////////////////////////////////////////////////
                    //Determine if an auto receipt is required by this destination customer
                    currentTripSegment.TripSegSendReceiptFlag = TripSendAutoReceiptValue.NoReceipt;
                    //If the “Send email receipt for all trip types” (CustAutoReceiptAllFlag) is set to Y and “Email” (Customer Receipt Settings) 
                    //is checked and “Email Address for Auto Receipt” is present, then we check if the next segment destination customer 
                    //is the same as the current destination customer. If so, we wait until the next segment is completed.
                    //Otherwise we mark the trip to send the email.
                    if (destCustomerMaster.CustAutoReceiptAllFlag == Constants.Yes)
                    {
                        if (destCustomerMaster.CustAutoRcptSettings.Contains(CustomerAutoReceiptConstants.EmailReceipt) &&
                            destCustomerMaster.CustEMailAddress != null)
                        {
                            string destHostcode = null;
                            foreach (var nextTripSegment in tripSegList)
                            {
                                if (nextTripSegment.TripSegNumber == driverSegmentDoneProcess.TripSegNumber)
                                {
                                    //This is our current destination customer.
                                    destHostcode = nextTripSegment.TripSegDestCustHostCode;
                                }
                                //Look for subsequent segments with the same destination customer.
                                //The first string follows the second string in the sort order.
                                else if (1 == nextTripSegment.TripSegNumber.CompareTo(driverSegmentDoneProcess.TripSegNumber))
                                {
                                    if (destHostcode != nextTripSegment.TripSegDestCustHostCode)
                                    {
                                        //Since destination of the next segment is the not the same as the current segment, send the email.
                                        currentTripSegment.TripSegSendReceiptFlag = TripSendAutoReceiptValue.ReceiptReady;
                                    }
                                }
                            }//end foreach ...
                        }//end if (destCustomerMaster.CustAutoRcptSettings...
                    }//end if (destCustomerMaster.CustAutoReceiptAllFlag...
                    else
                    {
                        //If the “Send email receipt for all trip types” (CustAutoReceiptAllFlag) is not set and the trip segment status is done 
                        //and the destination customer is a S = Supplier and the trip segment type is a load or pickup full, then we check the settings 
                        //for the destination customer.  If the “Email” (Customer Receipt Settings) is checked and “Email Address for Auto Receipt” 
                        //is present, then we mark the trip to send the email.
                        var types = new List<string> {BasicTripTypeConstants.PickupFull,
                                                      BasicTripTypeConstants.Load};
                        if (currentTripSegment.TripSegStatus == TripSegStatusConstants.Done &&
                             types.Contains(currentTripSegment.TripSegType) &&
                             destCustomerMaster.CustAutoRcptSettings.Contains(CustomerAutoReceiptConstants.EmailReceipt) &&
                             destCustomerMaster.CustEMailAddress != null)
                        {
                            currentTripSegment.TripSegSendReceiptFlag = TripSendAutoReceiptValue.ReceiptReady;
                        }
                    }

                    //////////////////////////////////////////////////////
                    //Update TripSegment Primary Container Information from first TripSegmentContainer information. 
                    //Use the list instead of requerying the database. Values are not yet saved.
                    if (tripSegContainerList.Count() > 0)
                    {
                        var firstTripSegmentContainer = tripSegContainerList.First();

                        if (null != firstTripSegmentContainer)
                        {
                            //Only if there is a container number
                            if (null != firstTripSegmentContainer.TripSegContainerNumber)
                            {
                                currentTripSegment.TripSegPrimaryContainerNumber = firstTripSegmentContainer.TripSegContainerNumber;
                                currentTripSegment.TripSegPrimaryContainerType = firstTripSegmentContainer.TripSegContainerType;
                                currentTripSegment.TripSegPrimaryContainerSize = firstTripSegmentContainer.TripSegContainerSize;
                                currentTripSegment.TripSegPrimaryContainerCommodityCode = firstTripSegmentContainer.TripSegContainerCommodityCode;
                                currentTripSegment.TripSegPrimaryContainerCommodityDesc = firstTripSegmentContainer.TripSegContainerCommodityDesc;
                                currentTripSegment.TripSegPrimaryContainerLocation = firstTripSegmentContainer.TripSegContainerLocation;
                            }
                        }
                    }

                    //Do the update
                    changeSetResult = Common.UpdateTripSegment(dataService, settings, currentTripSegment);
                    log.DebugFormat("SRTEST:Saving TripSegment Record for Trip:{0}-{1} - Segment Done.",
                                    currentTripSegment.TripNumber, currentTripSegment.TripSegNumber);
                    if (Common.LogChangeSetFailure(changeSetResult, currentTripSegment, log))
                    {
                        var s = string.Format("Could not update TripSegment for Trip:{0}-{1}.",
                            currentTripSegment.TripNumber, currentTripSegment.TripSegNumber);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }
                    ////////////////////////////////////////////////
                    //Check if next segment is at the same location as current segment
                    //If so, set TripSegStartDateTime for next segment.
                    //Set TripSegActualDriveStartDateTime, TripSegActualDriveEndDateTime,TripSegActualStopStartDateTime for next segment.
                    string hostcode = null;
                    foreach (var nextTripSegment in tripSegList)
                    {
                        if (nextTripSegment.TripSegNumber == driverSegmentDoneProcess.TripSegNumber)
                        {
                            //This is our current destination customer.
                            hostcode = nextTripSegment.TripSegDestCustHostCode;
                        }
                        //Look for subsequent segments with the same destination customer.
                        //The first string follows the second string in the sort order.
                        else if (1 == nextTripSegment.TripSegNumber.CompareTo(driverSegmentDoneProcess.TripSegNumber))
                        {
                            if (hostcode == nextTripSegment.TripSegDestCustHostCode)
                            {
                                //Since destination of the next segment is the same as the current segment, set the times.
                                nextTripSegment.TripSegStartDateTime = driverSegmentDoneProcess.ActionDateTime;
                                nextTripSegment.TripSegActualDriveStartDateTime = driverSegmentDoneProcess.ActionDateTime;
                                nextTripSegment.TripSegActualDriveEndDateTime = driverSegmentDoneProcess.ActionDateTime;
                                nextTripSegment.TripSegActualStopStartDateTime = driverSegmentDoneProcess.ActionDateTime;
                                nextTripSegment.TripSegActualDriveMinutes = 0;

                                //Do the update
                                changeSetResult = Common.UpdateTripSegment(dataService, settings, nextTripSegment);
                                log.DebugFormat("SRTEST:Saving next TripSegment Record for Trip:{0}-{1} - Segment Done.",
                                                nextTripSegment.TripNumber, nextTripSegment.TripSegNumber);
                                if (Common.LogChangeSetFailure(changeSetResult, nextTripSegment, log))
                                {
                                    var s = string.Format("Could not update next TripSegment for Trip:{0}-{1}.",
                                        nextTripSegment.TripNumber, nextTripSegment.TripSegNumber);
                                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                    break;
                                }
                            }
                            else
                            {
                                //Do not look any further. If trip is a PF-RT-DE where PR and DE are at the same site, 
                                //we do not want to set any values on the DE segment.
                                break;
                            }
                        }
                    }//end foreach ...
                     ////////////////////////////////////////////////
                     //Update Trip Record.  Update Primary Container Information if this is the first TripSegment.
                     //Container number in the TripSegmentContainer record should not be null but check anyway.
                    if (currentTripSegment.TripSegNumber == Constants.FirstSegment &&
                        null != currentTripSegment.TripSegPrimaryContainerNumber)
                    {
                        currentTrip.TripPrimaryContainerNumber = currentTripSegment.TripSegPrimaryContainerNumber;
                        currentTrip.TripPrimaryContainerType = currentTripSegment.TripSegPrimaryContainerType;
                        currentTrip.TripPrimaryContainerSize = currentTripSegment.TripSegPrimaryContainerSize;
                        currentTrip.TripPrimaryCommodityCode = currentTripSegment.TripSegPrimaryContainerCommodityCode;
                        currentTrip.TripPrimaryCommodityDesc = currentTripSegment.TripSegPrimaryContainerCommodityDesc;
                        currentTrip.TripPrimaryContainerLocation = currentTripSegment.TripSegPrimaryContainerLocation;
                        currentTrip.TripStartedDateTime = currentTripSegment.TripSegStartDateTime;
                        if (currentTripSegment.TripSegContainerQty > 1)
                        {
                            currentTrip.TripMultContainerFlag = Constants.Yes;
                        }
                        else
                        {
                            currentTrip.TripMultContainerFlag = Constants.No;
                        }
                        currentTrip.TripInProgressFlag = Constants.Yes;

                        //Set the auto email receipt flag in the trip record
                        currentTrip.TripSendReceiptFlag = currentTripSegment.TripSegSendReceiptFlag;

                        //Do the update
                        changeSetResult = Common.UpdateTrip(dataService, settings, currentTrip);
                        log.DebugFormat("SRTEST:Saving Trip Record for Trip:{0} - Segment Done.",
                                        currentTrip.TripNumber);
                        if (Common.LogChangeSetFailure(changeSetResult, currentTrip, log))
                        {
                            var s = string.Format("Could not update Trip for Trip:{0}.",
                                currentTrip.TripNumber);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }
                    }

                    ////////////////////////////////////////////////
                    //Update the PowerMaster table. 
                    //The odometer is not used in the current ScrapRunner processing of a segment done.
                    //powerMaster.PowerOdometer = driverSegmentDoneProcess.Odometer;
                    powerMaster.PowerLastActionDateTime = driverSegmentDoneProcess.ActionDateTime;
                    powerMaster.PowerCurrentTripNumber = driverSegmentDoneProcess.TripNumber;
                    powerMaster.PowerCurrentTripSegNumber = driverSegmentDoneProcess.TripSegNumber;
                    powerMaster.PowerCurrentTripSegType = currentTripSegment.TripSegType;

                    //Set these to the trip destination host code and type.
                    powerMaster.PowerCustHostCode = currentTripSegment.TripSegDestCustHostCode;
                    powerMaster.PowerCustType = currentTripSegment.TripSegDestCustType;

                    //Do the update
                    changeSetResult = Common.UpdatePowerMaster(dataService, settings, powerMaster);
                    log.DebugFormat("SRTEST:Saving PowerMaster Record for PowerId:{0} - Segment Done.",
                                    driverSegmentDoneProcess.PowerId);
                    if (Common.LogChangeSetFailure(changeSetResult, powerMaster, log))
                    {
                        var s = string.Format("Could not update PowerMaster for PowerId:{0}.",
                                              driverSegmentDoneProcess.PowerId);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Update Customer Master with GPS co-ordinates.
                    if (driverSegmentDoneProcess.Latitude != null && driverSegmentDoneProcess.Longitude != null)
                    {
                        if (destCustomerMaster.CustLatitude == null || destCustomerMaster.CustLongitude == null)
                        {
                            destCustomerMaster.CustLatitude = driverSegmentDoneProcess.Latitude;
                            destCustomerMaster.CustLongitude = driverSegmentDoneProcess.Longitude;

                            //Do the update
                            changeSetResult = Common.UpdateCustomerMaster(dataService, settings, destCustomerMaster);
                            log.DebugFormat("SRTEST:Saving CustomerMaster Record for CustHostCode:{0} - Segment Done.",
                                            currentTripSegment.TripSegDestCustHostCode);
                            if (Common.LogChangeSetFailure(changeSetResult, destCustomerMaster, log))
                            {
                                var s = string.Format("Could not update CustomerMaster for CustHostCode:{0}.",
                                                      currentTripSegment.TripSegDestCustHostCode);
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                break;
                            }
                        }
                    }
                    ////////////////////////////////////////////////
                    //Add entry to Event Log – Segment Done. 
                    StringBuilder sbComment = new StringBuilder();
                    sbComment.Append(EventCommentConstants.ReceivedDriverSegDone);
                    sbComment.Append(" HH:");
                    sbComment.Append(driverSegmentDoneProcess.ActionDateTime);
                    sbComment.Append(" Trip:");
                    sbComment.Append(driverSegmentDoneProcess.TripNumber);
                    sbComment.Append("-");
                    sbComment.Append(driverSegmentDoneProcess.TripSegNumber);
                    sbComment.Append(" Drv:");
                    sbComment.Append(driverSegmentDoneProcess.EmployeeId);
                    sbComment.Append(" Pwr:");
                    sbComment.Append(driverSegmentDoneProcess.PowerId);
                    sbComment.Append(" SegType:");
                    sbComment.Append(currentTripSegment.TripSegType);
                    sbComment.Append(" SegStatus:");
                    sbComment.Append(currentTripSegment.TripSegStatus);
                    string comment = sbComment.ToString().Trim();

                    var eventLog = new EventLog()
                    {
                        EventDateTime = driverSegmentDoneProcess.ActionDateTime,
                        EventSeqNo = 0,
                        EventTerminalId = employeeMaster.TerminalId,
                        EventRegionId = employeeMaster.RegionId,
                        //These are not populated for logins in the current system.
                        // EventEmployeeId = driverStatus.EmployeeId,
                        // EventEmployeeName = Common.GetDriverName(employeeMaster),
                        EventTripNumber = driverSegmentDoneProcess.TripNumber,
                        EventProgram = EventProgramConstants.Services,
                        //These are not populated for enroutes in the current system.
                        //EventScreen = null,
                        //EventAction = null,
                        EventComment = comment,
                    };

                    ChangeSetResult<int> eventChangeSetResult;
                    eventChangeSetResult = Common.UpdateEventLog(dataService, settings, eventLog);
                    log.Debug("SRTEST:Saving EventLog Record - Segment Done");
                    //if (Common.LogChangeSetFailure(eventChangeSetResult, eventLog, log))
                    //{
                    //    var s = string.Format("Could not update EventLog for Driver {0} {1}.",
                    //                         driverStatus.EmployeeId, EventCommentConstants.ReceivedDriverLogin);
                    //    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                    //    break;
                    //}

                    //TODO Maybe not.  Check all dates and odometers for accuracy.
                    //Make sure drive and stop start and end date/ times are within the range of the segment start and end date / times.
                    //Make sure delay start and end date/ times are within the range of the segment start and end date / times.
                    //Make sure all state mileage odometers are within the range of the segment start and end odometers.
                    //Recalculate Stop minutes, Drive minutes for segment.

                    //If this was the last segment, proceed to Trip Done Processing.
                    var lastTripSegment = (from item in tripSegList
                                           select item).LastOrDefault();
                    if (null != lastTripSegment)
                    {
                        if (lastTripSegment.TripSegNumber == driverSegmentDoneProcess.TripSegNumber)
                        {
                            if (!MarkTripDone(dataService, settings, changeSetResult, key, userRoleIds, userCulture,
                                                driverSegmentDoneProcess, employeeMaster, powerMaster,
                                                currentTrip, tripSegList, tripSegContainerList))
                            {
                                var s = string.Format("Could not Mark Trip Done for Trip:{0}.",
                                                      currentTrip.TripNumber);
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                break;
                            }
                        }
                    }//end of if (lastTripSegment.TripSegNumber

                    ////////////////////////////////////////////////
                    //Update the DriverStatus table. Do this after the trip might have been marked done
                    var driverStatus = Common.GetDriverStatus(dataService, settings, userCulture, userRoleIds,
                                       driverSegmentDoneProcess.EmployeeId, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (driverStatus == null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid DriverId: "
                                      + driverSegmentDoneProcess.EmployeeId));
                        break;
                    }
                    driverStatus.Status = DriverStatusSRConstants.Done;
                    driverStatus.PrevDriverStatus = driverStatus.Status;
                    driverStatus.TripNumber = driverSegmentDoneProcess.TripNumber;
                    driverStatus.TripSegNumber = driverSegmentDoneProcess.TripSegNumber;
                    driverStatus.TripSegType = currentTripSegment.TripSegType;
                    driverStatus.TripAssignStatus = currentTrip.TripAssignStatus;
                    driverStatus.TripStatus = currentTrip.TripStatus;
                    driverStatus.TripSegStatus = currentTripSegment.TripSegStatus;
                    driverStatus.PowerId = driverSegmentDoneProcess.PowerId;
                    driverStatus.MDTId = driverSegmentDoneProcess.Mdtid;
                    driverStatus.ActionDateTime = driverSegmentDoneProcess.ActionDateTime;

                    //The odometer is not used in the current ScrapRunner processing of a segment done.
                    //Use the last odometer stored in the PowerMaster record
                    driverStatus.Odometer = powerMaster.PowerOdometer;

                    //These are not set on in the segment done processing.
                    //driverStatus.GPSAutoGeneratedFlag = driverSegmentDoneProcess.GPSAutoFlag;
                    //if (null == driverSegmentDoneProcess.Latitude || null == driverSegmentDoneProcess.Longitude)
                    //{
                    //    driverStatus.GPSXmitFlag = Constants.No;
                    //}
                    //else
                    //{
                    //    driverStatus.GPSXmitFlag = Constants.Yes;
                    //}

                    ////////////////////////////////////////////////
                    //Update the driver's cumulative time which is the sum of standard drive and stop minutes for all 
                    //incomplete trip segments.Cannot requery db because changes have not been committed.
                    //Just subtract the std stop and drive times for this completed segment from the existing cumulative time.
                    driverStatus.DriverCumMinutes -= (int)currentTripSegment.TripSegStandardDriveMinutes;
                    driverStatus.DriverCumMinutes -= (int)currentTripSegment.TripSegStandardStopMinutes;

                    ////////////////////////////////////////////////
                    //Add record to the DriverHistory table.  
                    //For a completed trip, we need to log completed trip info to the driver history.
                    //Add it here, after the trip has been marked done and before we change the driver status
                    //info to show his next trip information
                    if (!Common.InsertDriverHistory(dataService, settings, driverStatus, employeeMaster,
                        ++driverHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        log.ErrorFormat("InsertDriverHistory failed: {0} during segment done request: {1}", fault.Message, driverSegmentDoneProcess);
                        break;
                    }

                    //Now make the final changes to the driver status if the trip has been marked done.
                    //We need to be able to display his new status and his next trip information.
                    if (lastTripSegment.TripSegNumber == driverSegmentDoneProcess.TripSegNumber)
                    {
                        //Determine the Next Incomplete Trip For Driver
                        var nextTrip = Common.GetNextTripForDriver(dataService, settings, userCulture, userRoleIds,
                                           driverSegmentDoneProcess.EmployeeId, currentTrip.TripNumber,out fault);

                        if (fault != null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        if (null == nextTrip)
                        {
                            //If he has no more trips, set status to no work, logged in but has no trips.
                            driverStatus.Status = DriverStatusSRConstants.NoWork;
                            driverStatus.PrevDriverStatus = null;
                            //Remove trip info
                            driverStatus.TripNumber = null;
                            driverStatus.TripSegNumber = null;
                            driverStatus.TripSegType = null;
                            driverStatus.TripStatus = null;
                            driverStatus.TripAssignStatus = null;
                        }
                        else
                        {
                            //If he has more trips, set status to available, logged in and has trips.
                            driverStatus.Status = DriverStatusSRConstants.Available;
                            driverStatus.PrevDriverStatus = null;

                            //Set trip info
                            driverStatus.TripNumber = nextTrip.TripNumber;
                            driverStatus.TripStatus = nextTrip.TripStatus;
                            driverStatus.TripAssignStatus = nextTrip.TripAssignStatus;

                            //Get next incomplete trip segment for next trip
                            var nextTripSegment = Common.GetNextIncompleteTripSegment(dataService, settings, userCulture, userRoleIds,
                                                  nextTrip.TripNumber, out fault);
                            if (fault != null)
                            {
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                                break;
                            }
                            if (null != nextTripSegment)
                            {
                                driverStatus.TripSegNumber = nextTripSegment.TripSegNumber;
                                driverStatus.TripSegType = nextTripSegment.TripSegType;
                            }
                        }
                    }

                    //Do the update
                    changeSetResult = Common.UpdateDriverStatus(dataService, settings, driverStatus);
                    log.DebugFormat("SRTEST:Saving DriverStatus Record for DriverId:{0} - Segment Done.",
                                    driverStatus.EmployeeId);
                    if (Common.LogChangeSetFailure(changeSetResult, driverStatus, log))
                    {
                        var s = string.Format("Could not update DriverStatus for DriverId:{0}.",
                            driverStatus.EmployeeId);
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
                log.Debug("SRTEST:Transaction Rollback - Segment Done");
            }
            else
            {
                transaction.Commit();
                log.Debug("SRTEST:Transaction Committed - Segment Done");
                // We need to notify that data has changed for any types we have updated
                // We always need to notify for the current type
                dataService.NotifyOfExternalChangesToData();
            }
            transaction.Dispose();
            session.Dispose();
            settings.Session = null;

            return changeSetResult;
        }
        public bool MarkTripDone(IDataService dataService, ProcessChangeSetSettings settings,
           ChangeSetResult<string> changeSetResult, String key, IEnumerable<long> userRoleIds, string userCulture,
           DriverSegmentDoneProcess driverSegmentDoneProcess, EmployeeMaster employeeMaster, PowerMaster powerMaster, 
           Trip currentTrip, List<TripSegment> tripSegList, List<TripSegmentContainer> tripSegContainerList)
        {
            DataServiceFault fault = null;
            string msgKey = key;
            int containerHistoryInsertCount = 0;

            ////////////////////////////////////////////////
            //TODO: Check for exception type delays
            //If any delay was an exception type, change the status of the segment to E=Exception
            //var driverDelays = Common.GetDriverDelaysException(dataService, settings, userCulture, userRoleIds,
            //                                driverSegmentDoneProcess.EmployeeId, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }

            // Get a list of all containers for the trip from the TripSegmentContainer table
            var tripContainerList = Common.GetTripContainers(dataService, settings, userCulture, userRoleIds,
                                    driverSegmentDoneProcess.TripNumber, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }

            ////////////////////////////////////////////////
            //TODO: Update TripTable - partially complete.

            //////////////////////////////////////////////////////
            //Determine the trip status
            //If any trip segment status is R=Review, then the trip status is R=Review
            //If any trip segment status is E=Exception, then the trip status is E=Exception
            //Otherwise the trip status is D=Done
            currentTrip.TripStatus = TripSegStatusConstants.Done;
            //If any segment status is in review, set trip status to R=Review
            var review = from item in tripSegList
                         where item.TripSegStatus == TripSegStatusConstants.Review
                         select item;
            if (review != null && review.Count() > 0)
            {
                currentTrip.TripStatus = TripStatusConstants.Review;

            }
            else
            {
                //If any segment status is an exception, set trip status to E=Exception
                var exception = from item in tripSegList
                                where item.TripSegStatus == TripSegStatusConstants.Exception
                                select item;
                if (exception != null && exception.Count() > 0)
                {
                    currentTrip.TripStatus = TripStatusConstants.Exception;

                }
            }
            //Lookup the TripStatus Description in the CodeTable TRIPSTATUS 
            var codeTableTripStatus = Common.GetCodeTableEntry(dataService, settings, userCulture, userRoleIds,
                                         CodeTableNameConstants.TripStatus, currentTrip.TripStatus, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            currentTrip.TripStatusDesc = codeTableTripStatus?.CodeDisp1;

            //If trip status is done, set the rseolved flag to Y.
            if(currentTrip.TripStatus == TripStatusConstants.Done)
            {
                currentTrip.TripResolvedFlag = Constants.Yes;
            }
            else
            {
                currentTrip.TripResolvedFlag = Constants.No;

            }
            //Trip Actual Drive Minutes is the sum of the Actual Drive Minutes of all of the segments. 
            currentTrip.TripActualDriveMinutes = (from item in tripSegList
                                  select item.TripSegActualDriveMinutes).Sum();

            //Trip Actual Stop Minutes is the sum of the Actual Stop Minutes of all of the segments. 
            currentTrip.TripActualStopMinutes = (from item in tripSegList
                                                 select item.TripSegActualStopMinutes).Sum();

            //Trip Completed DateTime is the End DateTime of the last segment.
            currentTrip.TripCompletedDateTime = (from item in tripSegList
                                                select item.TripSegEndDateTime).LastOrDefault();

            //Calculate the trip total minutes: TripCompletedDateTime - TripStartedDateTime
            if (currentTrip.TripCompletedDateTime != null && currentTrip.TripStartedDateTime != null)
            {
                currentTrip.TripActualTotalMinutes = (int)(currentTrip.TripCompletedDateTime.Value.Subtract
                          (currentTrip.TripStartedDateTime.Value).TotalMinutes);
            }
            else
            {
                currentTrip.TripActualTotalMinutes = 0;
            }

            //Set the Trip Changed DateTime to the Trip Completed DateTime.
            currentTrip.TripChangedDateTime = currentTrip.TripCompletedDateTime;

            //Set the Trip Changed UserId to the driver id of the last segment.
            currentTrip.TripChangedUserId = (from item in tripSegList
                                            select item.TripSegDriverId).LastOrDefault();

            //Set the Trip Changed UserName to the driver name.
            currentTrip.TripChangedUserName = Common.GetDriverName(employeeMaster);

            //Set the Trip Completed UserId to the Trip Changed UserId.
            currentTrip.TripCompletedUserId = currentTrip.TripChangedUserId;

            //Set the Trip Completed UserName to the Trip Changed UserName.
            currentTrip.TripCompletedUserName = currentTrip.TripChangedUserName;

            //Set the Trip In Progress Flag to N.
            currentTrip.TripInProgressFlag = Constants.No;

            //Set the Trip Done Method to D (Driver).
            currentTrip.TripDoneMethod = TripMethodOfCompletionConstants.Driver;

            //Set the Trip Sequence Number to 32766 to be excluded from resequencing routines.
            currentTrip.TripSequenceNumber = Constants.ExcludeFromSequencing;

            //Set the Trip Power Asset Number to the value in the PowerMaster table.
            currentTrip.TripPowerAssetNumber = powerMaster.PowerAssetNumber;

            //If there is nothing in the TripSpecInstructions field, update it with the
            //first review reason. This is for display on the View Trips screen in ScrapRunner.
            if (currentTrip.TripSpecInstructions == null)
            {
                currentTrip.TripSpecInstructions = (from item in tripContainerList
                                                   where item.TripSegContainerReviewReason !=null
                                                   select item.TripSegContainerReviewReason).FirstOrDefault();

            }

            //TODO: Check if the TripSendFlag in the Trip table should be set to send completed trip information.
            //Set the trip send flag to the appropriate value based on Trip Status.
            if (currentTrip.TripStatus == TripStatusConstants.Done)
            {
                currentTrip.TripSendFlag = TripSendFlagValue.TripDone;
            }
            else if (currentTrip.TripStatus == TripStatusConstants.Exception)
            {
                currentTrip.TripSendFlag = TripSendFlagValue.TripException;
            }
            else if (currentTrip.TripStatus == TripStatusConstants.Review)
            {
                currentTrip.TripSendFlag = TripSendFlagValue.TripInReview;
            }

            //TODO: Check if the TripSendScaleNotificationFlag in the Trip table should be set to send a scale notice.


            //TODO: Check if the trip should be placed in the Error Queue
            //If so,  set the Trip.TripStatus to TS_ERRQ (Q)
            //Set the Trip.TripErrorDesc to EXCESS TIME, EXCESS MILEAGE, INSUFFICIENT TIME, INSUFFICIENT MILEAGE

            //Do the update to the trip table
            changeSetResult = Common.UpdateTrip(dataService, settings, currentTrip);
            log.DebugFormat("SRTEST:Saving Trip Record for Trip:{0} - Segment Done.",
                            currentTrip.TripNumber);
            if (Common.LogChangeSetFailure(changeSetResult, currentTrip, log))
            {
                var s = string.Format("Could not update Trip for Trip:{0}.",
                    currentTrip.TripNumber);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }

            ////////////////////////////////////////////////
            //Update ContainerMaster for each container in the TripSegmentContainer records,/remove the current trip information.      


            // Next pull out a list of distinct container numbers only 
            var containerList = (from item in tripContainerList
                                 select item.TripSegContainerNumber).Distinct().ToList();

            // Get the Container records from the ContainerMaster
            var containersOnTrip = Common.GetContainersForTrip(dataService, settings, userCulture, userRoleIds,
                                   containerList, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            if (null == containersOnTrip)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid PowerId: "
                                + driverSegmentDoneProcess.PowerId));
                return false;
            }
            foreach (var containerMaster in containersOnTrip)
            {
                //Remove current trip information
                containerMaster.ContainerCurrentTripNumber = null;
                containerMaster.ContainerCurrentTripSegNumber = null;
                containerMaster.ContainerCurrentTripSegType = null;

                //This is where the Pending Move DateTime is removed in the current ScrapRunner.
                containerMaster.ContainerPendingMoveDateTime = null;

                //Do the update to the container master table
                changeSetResult = Common.UpdateContainerMaster(dataService, settings, containerMaster);
                log.DebugFormat("SRTEST:Saving Container Record for Container:{0} - Trip Done.",
                                containerMaster.ContainerNumber);
                if (Common.LogChangeSetFailure(changeSetResult, containerMaster, log))
                {
                    var s = string.Format("Could not update Container for Container:{0}.",
                        containerMaster.ContainerNumber);
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                    return false;
                }
                ////////////////////////////////////////////////
                //Add record to Container History. 
                if (!Common.InsertContainerHistory(dataService, settings, containerMaster,
                    ++containerHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                {
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                    log.ErrorFormat("InsertContainerHistory failed: {0} during segment done request: {1}", 
                                     fault.Message, driverSegmentDoneProcess);
                    break;
                }
            }

            ////////////////////////////////////////////////
            //TODO: Add record to DriverEfficiency table.

            ////////////////////////////////////////////////
            //TODO: Add records to History Trip Tables. 

            ////////////////////////////////////////////////
            //TODO: Resequence trips for the driver of the completed trip.

            return true;

        } //end of  MarkTripDone
    }
}

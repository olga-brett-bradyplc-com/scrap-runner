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
                    var tripRecord = Common.GetTrip(dataService, settings, userCulture, userRoleIds,
                                     driverSegmentDoneProcess.TripNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == tripRecord)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid TripNumber: "
                                        + driverSegmentDoneProcess.TripNumber));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Check if trip is complete
                    if (Common.IsTripComplete(tripRecord))
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

                    ////////////////////////////////////////////////
                    //Get a list of all incomplete segments for the trip
                    var tripSegList = Common.GetTripSegmentsIncomplete(dataService, settings, userCulture, userRoleIds,
                                      driverSegmentDoneProcess.TripNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Get the current TripSegment record
                    var tripSegmentRecord = (from item in tripSegList
                                             where item.TripSegNumber == driverSegmentDoneProcess.TripSegNumber
                                             select item).FirstOrDefault();
                    if (null == tripSegmentRecord)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid TripSegment: " +
                            driverSegmentDoneProcess.TripNumber + "-" + driverSegmentDoneProcess.TripSegNumber));
                        break;
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
                    //Start Updating...
                    ////////////////////////////////////////////////
                    //Update the TripSegment record.
                    tripSegmentRecord.TripSegPowerId = driverSegmentDoneProcess.PowerId;
                    tripSegmentRecord.TripSegPowerAssetNumber  = powerMaster.PowerAssetNumber;
                    tripSegmentRecord.TripSegDriverId = driverSegmentDoneProcess.EmployeeId;
                    tripSegmentRecord.TripSegDriverName = Common.GetDriverName(employeeMaster);
                    tripSegmentRecord.TripSegOdometerEnd = driverSegmentDoneProcess.Odometer;
                    tripSegmentRecord.TripSegEndLatitude = driverSegmentDoneProcess.Latitude;
                    tripSegmentRecord.TripSegEndLongitude = driverSegmentDoneProcess.Longitude;

                    //Driver has completed segment. Stop time ends.
                    tripSegmentRecord.TripSegActualStopEndDateTime = driverSegmentDoneProcess.ActionDateTime;
                    //Calculate the stop minutes: StopEndDateTime - StopStartDateTime
                    if (tripSegmentRecord.TripSegActualStopStartDateTime != null && tripSegmentRecord.TripSegActualStopEndDateTime != null)
                    {
                        tripSegmentRecord.TripSegActualStopMinutes = (int)(tripSegmentRecord.TripSegActualStopEndDateTime.Value.Subtract
                                  (tripSegmentRecord.TripSegActualStopStartDateTime.Value).TotalMinutes);
                    }
                    else
                    {
                        tripSegmentRecord.TripSegActualStopMinutes = 0;
                    }

                    //TODO Test. 
                    //If this is not his last segment, do not set Segment end time.  Wait until driver goes enroute on next segment.
                    //If this is the last segment of the trip, set the segment end date/time.
                    var lastTripSegmentRecord = (from item in tripSegList
                                             select item).LastOrDefault();
                    if (null == lastTripSegmentRecord)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid TripNumber: "
                                        + lastTripSegmentRecord.TripNumber));
                        break;
                    }
                    if (lastTripSegmentRecord.TripSegNumber == driverSegmentDoneProcess.TripSegNumber)
                    {
                        tripSegmentRecord.TripSegEndDateTime = driverSegmentDoneProcess.ActionDateTime;
                    }

                    //Get the number of containers on this segment
                    tripSegmentRecord.TripSegContainerQty =  tripSegContainerList.Count();

                    tripSegmentRecord.TripSegDriverGenerated = driverSegmentDoneProcess.DriverGenerated;
                    tripSegmentRecord.TripSegDriverModified = driverSegmentDoneProcess.DriverModified;

                    //////////////////////////////////////////////////////
                    //Determine the trip segment status
                    //If the ActionType of any container on the segment is E=Exception, then the trip seg status is E=Exception
                    //If the ActionType of any container on the segment is R=Review, then the trip seg status is R=Review
                    //Otherwise the trip seg status is D=Done
                    tripSegmentRecord.TripSegStatus = TripSegStatusConstants.Done;
                    //If any container is in review, set segment status to R=Review
                    var review = from item in tripSegContainerList
                                 where item.TripSegContainerReviewFlag == ActionTypeConstants.Review
                                 select item;
                    if (review != null && review.Count()>0)
                    {
                        tripSegmentRecord.TripSegStatus = TripSegStatusConstants.Review;

                    }
                    else
                    {
                        //If any container is an exception, set segment status to E=Exception
                        var exception = from item in tripSegContainerList
                                        where item.TripSegContainerReviewFlag == ActionTypeConstants.Exception
                                        select item;
                        if (exception != null && review.Count() > 0)
                        {
                            tripSegmentRecord.TripSegStatus = TripSegStatusConstants.Exception;

                        }
                    }

                    //Lookup the TripSegStatus Description in the CodeTable TRIPSEGSTATUS 
                    var codeTableTripSegStatus = Common.GetCodeTableEntry(dataService, settings, userCulture, userRoleIds,
                                                 CodeTableNameConstants.TripSegStatus, tripSegmentRecord.TripSegStatus, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    tripSegmentRecord.TripSegStatusDesc = codeTableTripSegStatus?.CodeDisp1;

                    //////////////////////////////////////////////////////
                    //Determine if an auto receipt is required by this destination customer
                    tripSegmentRecord.TripSegSendReceiptFlag = TripSendAutoReceiptValue.NoReceipt;
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
                            foreach (var nextTripSegmentRecord in tripSegList)
                            {
                                if (nextTripSegmentRecord.TripSegNumber == driverSegmentDoneProcess.TripSegNumber)
                                {
                                    //This is our current destination customer.
                                    destHostcode = nextTripSegmentRecord.TripSegDestCustHostCode;
                                }
                                //Look for subsequent segments with the same destination customer.
                                //The first string follows the second string in the sort order.
                                else if (1 == nextTripSegmentRecord.TripSegNumber.CompareTo(driverSegmentDoneProcess.TripSegNumber))
                                {
                                    if (destHostcode != nextTripSegmentRecord.TripSegDestCustHostCode)
                                    {
                                        //Since destination of the next segment is the not the same as the current segment, send the email.
                                        tripSegmentRecord.TripSegSendReceiptFlag = TripSendAutoReceiptValue.ReceiptReady;
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
                        if (tripSegmentRecord.TripSegStatus == TripSegStatusConstants.Done &&
                             types.Contains(tripSegmentRecord.TripSegType) &&
                             destCustomerMaster.CustAutoRcptSettings.Contains(CustomerAutoReceiptConstants.EmailReceipt) &&
                             destCustomerMaster.CustEMailAddress != null)
                        {
                            tripSegmentRecord.TripSegSendReceiptFlag = TripSendAutoReceiptValue.ReceiptReady;
                        }
                    }

                    //////////////////////////////////////////////////////
                    //Update TripSegment Primary Container Information from first TripSegmentContainer information. 
                    //Use the list instead of requerying the database. Values are not yet saved.
                    if (tripSegContainerList.Count() > 0)
                    {
                        var firstTripSegmentContainer = tripSegContainerList.First();

                        //Only if there is a container number
                        if (null != firstTripSegmentContainer.TripSegContainerNumber)
                        {
                            tripSegmentRecord.TripSegPrimaryContainerNumber = firstTripSegmentContainer.TripSegContainerNumber;
                            tripSegmentRecord.TripSegPrimaryContainerType = firstTripSegmentContainer.TripSegContainerType;
                            tripSegmentRecord.TripSegPrimaryContainerSize = firstTripSegmentContainer.TripSegContainerSize;
                            tripSegmentRecord.TripSegPrimaryContainerCommodityCode = firstTripSegmentContainer.TripSegContainerCommodityCode;
                            tripSegmentRecord.TripSegPrimaryContainerCommodityDesc = firstTripSegmentContainer.TripSegContainerCommodityCode;
                            tripSegmentRecord.TripSegPrimaryContainerLocation = firstTripSegmentContainer.TripSegContainerLocation;
                        }
                    }

                    //Do the update
                    changeSetResult = Common.UpdateTripSegment(dataService, settings, tripSegmentRecord);
                    log.DebugFormat("SRTEST:Saving TripSegment Record for Trip:{0}-{1} - Segment Done.",
                                    tripSegmentRecord.TripNumber, tripSegmentRecord.TripSegNumber);
                    if (Common.LogChangeSetFailure(changeSetResult, tripSegmentRecord, log))
                    {
                        var s = string.Format("Could not update TripSegment for Trip:{0}-{1}.",
                            tripSegmentRecord.TripNumber, tripSegmentRecord.TripSegNumber);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }
                    ////////////////////////////////////////////////
                    //Check if next segment is at the same location as current segment
                    //If so, set TripSegStartDateTime for next segment.
                    //Set TripSegActualDriveStartDateTime, TripSegActualDriveEndDateTime,TripSegActualStopStartDateTime for next segment.
                    //TODO test
                    string hostcode = null;
                    foreach (var nextTripSegmentRecord in tripSegList)
                    {
                        if (nextTripSegmentRecord.TripSegNumber == driverSegmentDoneProcess.TripSegNumber)
                        {
                            //This is our current destination customer.
                            hostcode = nextTripSegmentRecord.TripSegDestCustHostCode;
                        }
                        //Look for subsequent segments with the same destination customer.
                        //The first string follows the second string in the sort order.
                        else if (1 == nextTripSegmentRecord.TripSegNumber.CompareTo(driverSegmentDoneProcess.TripSegNumber))
                        {
                            if (hostcode == nextTripSegmentRecord.TripSegDestCustHostCode)
                            {
                                //Since destination of the next segment is the same as the current segment, set the times.
                                nextTripSegmentRecord.TripSegStartDateTime = driverSegmentDoneProcess.ActionDateTime;
                                nextTripSegmentRecord.TripSegActualDriveStartDateTime = driverSegmentDoneProcess.ActionDateTime;
                                nextTripSegmentRecord.TripSegActualDriveEndDateTime = driverSegmentDoneProcess.ActionDateTime;
                                nextTripSegmentRecord.TripSegActualStopStartDateTime = driverSegmentDoneProcess.ActionDateTime;
 
                                //Do the update
                                changeSetResult = Common.UpdateTripSegment(dataService, settings, nextTripSegmentRecord);
                                log.DebugFormat("SRTEST:Saving next TripSegment Record for Trip:{0}-{1} - Segment Done.",
                                                nextTripSegmentRecord.TripNumber, nextTripSegmentRecord.TripSegNumber);
                                if (Common.LogChangeSetFailure(changeSetResult, nextTripSegmentRecord, log))
                                {
                                    var s = string.Format("Could not update next TripSegment for Trip:{0}-{1}.",
                                        nextTripSegmentRecord.TripNumber, nextTripSegmentRecord.TripSegNumber);
                                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                    break;
                                }
                            }
                            else
                            {
                                //Do not look any farther. If trip is a PF-RT-DE where PR and DE are at the same site, 
                                //we do not want to set any values on the DE segment.
                                break;
                            }
                        }
                    }//end foreach ...
                     ////////////////////////////////////////////////
                     //Update Trip Record.  Update Primary Container Information if this is the first TripSegment.
                     //Container number in the TripSegmentContainer record should not be null but check anyway.
                    if (tripSegmentRecord.TripSegNumber == Constants.FirstSegment &&
                        null != tripSegmentRecord.TripSegPrimaryContainerNumber)
                    {
                        tripRecord.TripPrimaryContainerNumber = tripSegmentRecord.TripSegPrimaryContainerNumber;
                        tripRecord.TripPrimaryContainerType = tripSegmentRecord.TripSegPrimaryContainerType;
                        tripRecord.TripPrimaryContainerSize = tripSegmentRecord.TripSegPrimaryContainerSize;
                        tripRecord.TripPrimaryCommodityCode = tripSegmentRecord.TripSegPrimaryContainerCommodityCode;
                        tripRecord.TripPrimaryCommodityDesc = tripSegmentRecord.TripSegPrimaryContainerCommodityDesc;
                        tripRecord.TripPrimaryContainerLocation = tripSegmentRecord.TripSegPrimaryContainerLocation;
                        tripRecord.TripStartedDateTime = tripSegmentRecord.TripSegStartDateTime;
                        if (tripSegmentRecord.TripSegContainerQty > 1)
                        {
                            tripRecord.TripMultContainerFlag = Constants.Yes;
                        }
                        else
                        {
                            tripRecord.TripMultContainerFlag = Constants.No;
                        }
                        tripRecord.TripInProgressFlag = Constants.Yes;

                        //Set the auto email receipt flag in the trip record
                        tripRecord.TripSendReceiptFlag = tripSegmentRecord.TripSegSendReceiptFlag;

                        //Do the update
                        changeSetResult = Common.UpdateTrip(dataService, settings, tripRecord);
                        log.DebugFormat("SRTEST:Saving Trip Record for Trip:{0} - Segment Done.",
                                        tripRecord.TripNumber);
                        if (Common.LogChangeSetFailure(changeSetResult, tripRecord, log))
                        {
                            var s = string.Format("Could not update Trip for Trip:{0}.",
                                tripRecord.TripNumber);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            break;
                        }
                    }
                    ////////////////////////////////////////////////
                    //Update the DriverStatus table. 
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
                    driverStatus.TripSegType = tripSegmentRecord.TripSegType;
                    driverStatus.TripAssignStatus = tripRecord.TripAssignStatus;
                    driverStatus.TripStatus = tripRecord.TripStatus;
                    driverStatus.TripSegStatus = tripSegmentRecord.TripSegStatus;
                    driverStatus.PowerId = driverSegmentDoneProcess.PowerId;
                    driverStatus.MDTId = driverSegmentDoneProcess.Mdtid;
                    driverStatus.ActionDateTime = driverSegmentDoneProcess.ActionDateTime;
                    driverStatus.Odometer = driverSegmentDoneProcess.Odometer;
                    //driverStatus.GPSAutoGeneratedFlag = driverSegmentDoneProcess.GPSAutoFlag;
                    if (null == driverSegmentDoneProcess.Latitude || null == driverSegmentDoneProcess.Longitude)
                    {
                        driverStatus.GPSXmitFlag = Constants.No;
                    }
                    else
                    {
                        driverStatus.GPSXmitFlag = Constants.Yes;
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

                    ////////////////////////////////////////////////
                    //Add record to the DriverHistory table.
                    if (!Common.InsertDriverHistory(dataService, settings, driverStatus, employeeMaster,
                        ++driverHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        log.ErrorFormat("InsertDriverHistory failed: {0} during segment done request: {1}", fault.Message, driverSegmentDoneProcess);
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Update the PowerMaster table. 
    
                    powerMaster.PowerOdometer = driverSegmentDoneProcess.Odometer;
                    powerMaster.PowerLastActionDateTime = driverSegmentDoneProcess.ActionDateTime;
                    powerMaster.PowerCurrentTripNumber = driverSegmentDoneProcess.TripNumber;
                    powerMaster.PowerCurrentTripSegNumber = driverSegmentDoneProcess.TripSegNumber;
                    powerMaster.PowerCurrentTripSegType = tripSegmentRecord.TripSegType;

                    //Set these to the trip destination host code and type.
                    powerMaster.PowerCustHostCode = tripSegmentRecord.TripSegDestCustHostCode;
                    powerMaster.PowerCustType = tripSegmentRecord.TripSegDestCustType;

                    //Do the update
                    changeSetResult = Common.UpdatePowerMaster(dataService, settings, powerMaster);
                    log.DebugFormat("SRTEST:Saving PowerMaster Record for PowerId:{0} - Arrive.",
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
                                            tripSegmentRecord.TripSegDestCustHostCode);
                            if (Common.LogChangeSetFailure(changeSetResult, destCustomerMaster, log))
                            {
                                var s = string.Format("Could not update CustomerMaster for CustHostCode:{0}.",
                                                      tripSegmentRecord.TripSegDestCustHostCode);
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
                    sbComment.Append(tripSegmentRecord.TripSegType);
                    sbComment.Append(" SegStatus:");
                    sbComment.Append(tripSegmentRecord.TripSegStatus);
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

                    //TODO Maybe...
                    //Check all dates and odometers
                    //Make sure drive and stop start and end date/ times are within the range of the segment start and end date / times.
                    //Make sure delay start and end date/ times are within the range of the segment start and end date / times.
                    //Make sure all state mileage odometers are within the range of the segment start and end odometers.
                    //Recalculate Stop minutes, Drive minutes for segment.

                    //TODO
                    //If this was the last segment, proceed to Trip Done Processing.
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
    }
}


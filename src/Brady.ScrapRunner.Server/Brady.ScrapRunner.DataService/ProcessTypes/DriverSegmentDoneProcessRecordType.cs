﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
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
using log4net;

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
        // We hide the base logger deliberately. We name the logger after the domain obejct deliberately. 
        // We want a clean logger name for sensible I/O capture.
        protected new static readonly ILog log = LogManager.GetLogger(typeof(DriverSegmentDoneProcess));

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
            // Capture details of incoming request for logging the INFO level
            var requestRespStrBld = RequestResponseUtil.CaptureRequest(changeSet);
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

                // Determine userCulture and userRoleIds.
                var userCulture = "en-GB";
                var userRoleIds = Enumerable.Empty<long>().ToArray();
                if (null != settings.Username && null != settings.Token)
                {
                    var userCultureDetails = authorisation.GetUserCultureDetailsAsync(settings.Token, settings.Username).Result;
                    userCulture = userCultureDetails.LanguageCulture;
                    userRoleIds = authorisation.GetRoleIdsAsync(settings.Token, settings.Username).Result;
                }

                foreach (String key in changeSetResult.SuccessfullyUpdated)
                {
                    DataServiceFault fault;
                    string msgKey = key;

                    var driverSegmentDoneProcess = (DriverSegmentDoneProcess)changeSetResult.GetSuccessfulUpdateForId(key);

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
                    //Validate the ActionType
                    var actions = new List<string> {TripSegmentActionTypeConstants.Done,
                                                    TripSegmentActionTypeConstants.Canceled,
                                                    TripSegmentActionTypeConstants.Pending};
                    if (!actions.Contains(driverSegmentDoneProcess.ActionType))
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid ActionType: "
                                        + driverSegmentDoneProcess.ActionType));
                        break;
                    }
                    ////////////////////////////////////////////////
                    //DriverSegmentDoneProcess has been called
                    log.DebugFormat("SRTEST:DriverSegmentDoneProcess Called by {0}", key);
                    if (driverSegmentDoneProcess.ActionType == TripSegmentActionTypeConstants.Done)
                    {
                        log.DebugFormat("SRTEST:DriverSegmentDoneProcess:DONE Driver:{0} DT:{1} Trip:{2}-{3} PowerId:{4} Lat:{5} Lon:{6} Host:{7} DrvGen:{8} DrvMod:{9} MDT:{10}",
                                     driverSegmentDoneProcess.EmployeeId, driverSegmentDoneProcess.ActionDateTime,
                                     driverSegmentDoneProcess.TripNumber, driverSegmentDoneProcess.TripSegNumber,
                                     driverSegmentDoneProcess.PowerId, driverSegmentDoneProcess.Latitude,
                                     driverSegmentDoneProcess.Longitude, driverSegmentDoneProcess.DestCustHostCode,
                                     driverSegmentDoneProcess.DriverGenerated, driverSegmentDoneProcess.DriverModified,
                                     driverSegmentDoneProcess.Mdtid);
                    }
                    if (driverSegmentDoneProcess.ActionType == TripSegmentActionTypeConstants.Canceled)
                    {
                        log.DebugFormat("SRTEST:DriverSegmentDoneProcess:CANCELED Driver:{0} DT:{1} Trip:{2}-{3} PowerId:{4} Lat:{5} Lon:{6} Host:{7} DrvGen:{8} DrvMod:{9} MDT:{10}",
                                     driverSegmentDoneProcess.EmployeeId, driverSegmentDoneProcess.ActionDateTime,
                                     driverSegmentDoneProcess.TripNumber, driverSegmentDoneProcess.TripSegNumber,
                                     driverSegmentDoneProcess.PowerId, driverSegmentDoneProcess.Latitude,
                                     driverSegmentDoneProcess.Longitude, driverSegmentDoneProcess.DestCustHostCode,
                                     driverSegmentDoneProcess.DriverGenerated, driverSegmentDoneProcess.DriverModified,
                                     driverSegmentDoneProcess.Mdtid);
                    }
                    if (driverSegmentDoneProcess.ActionType == TripSegmentActionTypeConstants.Pending)
                    {
                        log.DebugFormat("SRTEST:DriverSegmentDoneProcess:PENDING Driver:{0} DT:{1} Trip:{2}-{3} PowerId:{4} Lat:{5} Lon:{6} Host:{7} DrvGen:{8} DrvMod:{9} MDT:{10}",
                                     driverSegmentDoneProcess.EmployeeId, driverSegmentDoneProcess.ActionDateTime,
                                     driverSegmentDoneProcess.TripNumber, driverSegmentDoneProcess.TripSegNumber,
                                     driverSegmentDoneProcess.PowerId, driverSegmentDoneProcess.Latitude,
                                     driverSegmentDoneProcess.Longitude, driverSegmentDoneProcess.DestCustHostCode,
                                     driverSegmentDoneProcess.DriverGenerated, driverSegmentDoneProcess.DriverModified,
                                     driverSegmentDoneProcess.Mdtid);
                        if (driverSegmentDoneProcess.DestCustHostCode == null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DestCustHostCode is required. "
                                    + driverSegmentDoneProcess.TripNumber + "-" + driverSegmentDoneProcess.TripSegNumber));
                            break;
                        }
                        if (driverSegmentDoneProcess.DriverGenerated == null)
                        {
                            driverSegmentDoneProcess.DriverGenerated = Constants.No;
                        }
                        if (driverSegmentDoneProcess.DriverModified == null)
                        {
                            driverSegmentDoneProcess.DriverModified = Constants.No;
                        }
                        if (driverSegmentDoneProcess.DriverGenerated == Constants.No &&
                            driverSegmentDoneProcess.DriverModified == Constants.No)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverGenerated or DriverModified must be Y. "
                                    + driverSegmentDoneProcess.TripNumber + "-" + driverSegmentDoneProcess.TripSegNumber));
                            break;
                        }

                        if (driverSegmentDoneProcess.DriverGenerated == Constants.Yes)
                        {
                            if (driverSegmentDoneProcess.TripSegType == null)
                            {
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("TripSegType is required. "
                                        + driverSegmentDoneProcess.TripNumber + "-" + driverSegmentDoneProcess.TripSegNumber));
                                break;
                            }
                            if (driverSegmentDoneProcess.TripSegType != BasicTripTypeConstants.ReturnYard &&
                                driverSegmentDoneProcess.TripSegType != BasicTripTypeConstants.ReturnYardNC)
                            {
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("TripSegType must be RT or RN. "
                                        + driverSegmentDoneProcess.TripNumber + "-" + driverSegmentDoneProcess.TripSegNumber));
                                break;
                            }
                        }
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
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Segment Done:Invalid DriverId: "
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
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Segment Done:Invalid TripNumber: "
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
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Segment Done:Invalid PowerId: "
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

                    ////////////////////////////////////////////////////////
                    //Do not use the MDTId from the mobile app. Build it using the MDT Prefix (if it exists) plus the employee id.
                    // Lookup Preference: DEFMDTPrefix
                    string prefMdtPrefix = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                                    Constants.SystemTerminalId, PrefSystemConstants.DEFMDTPrefix, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    driverSegmentDoneProcess.Mdtid = prefMdtPrefix + driverSegmentDoneProcess.EmployeeId;

                    ////////////////////////////////////////////////
                    //Get a list of all segments for the trip
                    var tripSegList = Common.GetTripSegmentsForTrip(dataService, settings, userCulture, userRoleIds,
                                      driverSegmentDoneProcess.TripNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    ////////////////////////////////////////////////
                    // Get a list of all containers for the trip from the TripSegmentContainer table
                    var tripContainerList = Common.GetTripContainersForTrip(dataService, settings, userCulture, userRoleIds,
                                        driverSegmentDoneProcess.TripNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Now call the SegmentPending method to process adds and modifies
                    if (driverSegmentDoneProcess.ActionType == TripSegmentActionTypeConstants.Pending)
                    {
                        if (driverSegmentDoneProcess.DriverGenerated == Constants.Yes)
                        {
                            if (!SegmentAdd(dataService, settings, changeSetResult, msgKey, userRoleIds, userCulture,
                                               driverSegmentDoneProcess, employeeMaster, tripSegList, tripContainerList))
                            {
                                break;
                            }
                        }
                        else if (driverSegmentDoneProcess.DriverModified == Constants.Yes)
                        {
                            if (!SegmentModify(dataService, settings, changeSetResult, msgKey, userRoleIds, userCulture,
                                               driverSegmentDoneProcess, employeeMaster, powerMaster,
                                               currentTrip, tripSegList, tripContainerList))
                            {
                                break;
                            }
                        }

                        break;
                    }
                    ////////////////////////////////////////////////
                    // Get the current TripSegment record
                    var currentTripSegment = (from item in tripSegList
                                              where item.TripSegNumber == driverSegmentDoneProcess.TripSegNumber
                                              select item).FirstOrDefault();
                    if (null == currentTripSegment)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Segment Done:Invalid TripSegment: " +
                            driverSegmentDoneProcess.TripNumber + "-" + driverSegmentDoneProcess.TripSegNumber));
                        break;
                    }
                    ////////////////////////////////////////////////
                    //Check if trip segment is complete
                    if (Common.IsTripSegmentComplete(currentTripSegment))
                    {
                        log.DebugFormat("SRTEST:TripSegNumber:{0}-{1} is Complete. Segment done processing ends.",
                                        driverSegmentDoneProcess.TripNumber, driverSegmentDoneProcess.TripSegNumber);
                        break;
                    }
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
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Segment Done:Invalid CustHostCode: "
                                        + currentTripSegment.TripSegDestCustHostCode));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Get a list of all TripReferenceNumber records for the trip
                    var tripReferenceNumberList = Common.GetTripReferenceNumberForTrip(dataService, settings, userCulture, userRoleIds,
                                                  driverSegmentDoneProcess.TripNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    ////////////////////////////////////////////////
                    //Get a list of all TripSegmentMileage records for the entire trip
                    var tripMileageList = Common.GetTripSegmentMileageForTrip(dataService, settings, userCulture, userRoleIds,
                                                 driverSegmentDoneProcess.TripNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    ////////////////////////////////////////////////
                    // Get the driver delays for trip number
                    var tripDelayList = Common.GetDriverDelaysForTrip(dataService, settings, userCulture, userRoleIds,
                                                 driverSegmentDoneProcess.TripNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Get a list of incomplete containers for the segment
                    var incompleteTripSegContainerList = (from item in tripContainerList
                                                          where item.TripSegNumber == currentTripSegment.TripSegNumber
                                                          && item.TripSegContainerComplete != Constants.Yes
                                                          select item).ToList();
                    if (incompleteTripSegContainerList != null)
                    {
                        foreach (var incompleteTripSegmentContainer in incompleteTripSegContainerList)
                        {

                            tripContainerList.Remove(incompleteTripSegmentContainer);

                            //Do the delete. Deleting records with composite keys is now fixed.
                            changeSetResult = Common.DeleteTripSegmentContainer(dataService, settings, incompleteTripSegmentContainer);
                            log.DebugFormat("SRTEST:Deleting TripSegmentContainer Record for Trip:{0}-{1} Container:{2}- Segment Done.",
                                            incompleteTripSegmentContainer.TripNumber, incompleteTripSegmentContainer.TripSegNumber,
                                            incompleteTripSegmentContainer.TripSegContainerNumber);
                            if (Common.LogChangeSetFailure(changeSetResult, incompleteTripSegmentContainer, log))
                            {
                                var s = string.Format("Segment Done:Could not delete TripSegmentContainer for Trip:{0}-{1} Container:{2}.",
                                        incompleteTripSegmentContainer.TripNumber, incompleteTripSegmentContainer.TripSegNumber,
                                        incompleteTripSegmentContainer.TripSegContainerNumber);
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                break;
                            }
                        }
                    }

                    ////////////////////////////////////////////////
                    //Get a list of all containers for the segment. 
                    var tripSegContainerList = (from item in tripContainerList
                                                where item.TripSegNumber == driverSegmentDoneProcess.TripSegNumber
                                                select item).ToList();

                    ////////////////////////////////////////////////
                    //If the ActionType is Canceled then process sement canceled
                    if (driverSegmentDoneProcess.ActionType == TripSegmentActionTypeConstants.Canceled)
                    {
                        if (!SegmentCancel(dataService, settings, changeSetResult, msgKey, userRoleIds, userCulture,
                                            driverSegmentDoneProcess, employeeMaster, powerMaster, destCustomerMaster,
                                            currentTrip, tripSegList, currentTripSegment, tripContainerList, tripReferenceNumberList,
                                            tripMileageList, tripDelayList))
                        {
                            break;
                        }

                    }
                    else
                    {
                        if (!SegmentDone(dataService, settings, changeSetResult, msgKey, userRoleIds, userCulture,
                                            driverSegmentDoneProcess, employeeMaster, powerMaster, destCustomerMaster,
                                            currentTrip, tripSegList, currentTripSegment, tripContainerList, tripReferenceNumberList,
                                            tripMileageList, tripDelayList))
                        {
                            break;
                        }
                    }

                }//end of foreach 
            }//end of if (!changeSetResult.Failed...

            // If our local session variable is set then it is our session/txn to deal with
            // otherwise we simply return the result.
            if (session == null)
            {
                // Capture details of outgoing response too and log at INFO level
                log.Info(RequestResponseUtil.CaptureResponse(changeSetResult, requestRespStrBld));
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

            // Capture details of outgoing response too and log at INFO level
            log.Info(RequestResponseUtil.CaptureResponse(changeSetResult, requestRespStrBld));
            return changeSetResult;
        }

        /// <summary>
        /// Driver canceled segment
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="changeSetResult"></param>
        /// <param name="msgKey"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="userCulture"></param>
        /// <param name="driverSegmentDoneProcess"></param>
        /// <param name="employeeMaster"></param>
        /// <param name="powerMaster"></param>
        /// <param name="destCustomerMaster"></param>
        /// <param name="currentTrip"></param>
        /// <param name="tripSegList"></param>
        /// <param name="currentTripSegment"></param>
        /// <param name="tripContainerList"></param>
        /// <param name="tripReferenceNumberList"></param>
        /// <param name="tripMileageList"></param>
        /// <param name="tripDelayList"></param>
        /// <returns></returns>
        private bool SegmentCancel(IDataService dataService, ProcessChangeSetSettings settings,
           ChangeSetResult<string> changeSetResult, String msgKey, long[] userRoleIds, string userCulture,
           DriverSegmentDoneProcess driverSegmentDoneProcess, EmployeeMaster employeeMaster, PowerMaster powerMaster,
           CustomerMaster destCustomerMaster, Trip currentTrip, List<TripSegment> tripSegList, TripSegment currentTripSegment,
           List<TripSegmentContainer> tripContainerList, List<TripReferenceNumber> tripReferenceNumberList,
           List<TripSegmentMileage> tripMileageList, List<DriverDelay> tripDelayList)
        {
            DataServiceFault fault;

            ////////////////////////////////////////////////
            //Get the DriverStatus record. 
            var driverStatus = Common.GetDriverStatus(dataService, settings, userCulture, userRoleIds,
                               driverSegmentDoneProcess.EmployeeId, out fault);
            if (fault != null)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            if (driverStatus == null)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Segment Cancel:Invalid DriverId: "
                              + driverSegmentDoneProcess.EmployeeId));
                return false;
            }

            var tempContainerList = new List<TripSegmentContainer>();
            tempContainerList.AddRange(tripContainerList);
            foreach (var tripContainer in tempContainerList)
            {
                //Delete the trip segment container records for the current segment as well as any containers on segments 
                //beyond the current one. 
                if ( 0 <= tripContainer.TripSegNumber.CompareTo(currentTripSegment.TripSegNumber))
                {
                    tripContainerList.Remove(tripContainer);

                    //Do the delete. Deleting records with composite keys is now fixed.
                    changeSetResult = Common.DeleteTripSegmentContainer(dataService, settings, tripContainer);
                    log.DebugFormat("SRTEST:Deleting TripSegmentContainer Record for Trip:{0}-{1} Seq:{2} - Segment Canceled.",
                                    tripContainer.TripNumber, tripContainer.TripSegNumber,
                                    tripContainer.TripSegContainerSeqNumber);
                    if (Common.LogChangeSetFailure(changeSetResult, tripContainer, log))
                    {
                        var s = string.Format("DriverSegmentDoneProcess:Could not delete TripSegmentMileage for Trip:{0}-{1} Seq:{2} - Segment Canceled.",
                                tripContainer.TripNumber, tripContainer.TripSegNumber,
                                tripContainer.TripSegContainerSeqNumber);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }
                }
            }

            var tempMileageList = new List<TripSegmentMileage>();
            tempMileageList.AddRange(tripMileageList);
            foreach (var tripMileage in tempMileageList)
            {
                //Delete the trip segment mileage records for the current segment as well as any mileage segments 
                //beyond the current one. 
                if (0 <= tripMileage.TripSegNumber.CompareTo(currentTripSegment.TripSegNumber))
                {
                    tripMileageList.Remove(tripMileage);

                    //Do the delete. Deleting records with composite keys is now fixed.
                    changeSetResult = Common.DeleteTripSegmentMileage(dataService, settings, tripMileage);
                    log.DebugFormat("SRTEST:Deleting TripSegmentMileage Record for Trip:{0}-{1} Seq:{2}- Segment Canceled.",
                                    tripMileage.TripNumber, tripMileage.TripSegNumber,
                                    tripMileage.TripSegMileageSeqNumber);
                    if (Common.LogChangeSetFailure(changeSetResult, tripMileage, log))
                    {
                        var s = string.Format("DriverSegmentDoneProcess:Could not delete TripSegmentMileage for Trip:{0}-{1} Seq:{2} - Segment Canceled.",
                                tripMileage.TripNumber, tripMileage.TripSegNumber,
                                tripMileage.TripSegMileageSeqNumber);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }
                }
            }

            var tempSegList = new List<TripSegment>();
            tempSegList.AddRange(tripSegList);
            foreach (var tripSegment in tempSegList)
            {
                //Delete the trip segment records for the current segment as well as any segments beyond the current one. 
                if (0 <= tripSegment.TripSegNumber.CompareTo(currentTripSegment.TripSegNumber))
                {
                    tripSegList.Remove(tripSegment);

                    ////////////////////////////////////////////////
                    //Update the driver's cumulative time which is the sum of standard drive and stop minutes for all 
                    //incomplete trip segments.Cannot requery db because changes have not been committed.
                    //Just subtract the std stop and drive times for this completed segment from the existing cumulative time.
                    if (null != driverStatus.DriverCumMinutes)
                    {
                        driverStatus.DriverCumMinutes -= currentTripSegment.TripSegStandardDriveMinutes ?? 0;
                        driverStatus.DriverCumMinutes -= currentTripSegment.TripSegStandardStopMinutes ?? 0;
                    }

                    //Do the delete. Deleting records with composite keys is now fixed.
                    changeSetResult = Common.DeleteTripSegment(dataService, settings, tripSegment);
                    log.DebugFormat("SRTEST:Deleting TripSegment Record for Trip:{0}-{1} - Segment Canceled.",
                                    tripSegment.TripNumber, tripSegment.TripSegNumber);
                    if (Common.LogChangeSetFailure(changeSetResult, tripSegment, log))
                    {
                        var s = string.Format("DriverSegmentDoneProcess:Could not delete TripSegment for Trip:{0}-{1} - Segment Canceled.",
                                tripSegment.TripNumber, tripSegment.TripSegNumber);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Add entry to Event Log – Segment Canceled. 
                    StringBuilder sbComment = new StringBuilder();
                    sbComment.Append(EventCommentConstants.ReceivedDriverCancelSeg);
                    sbComment.Append(" HH:");
                    sbComment.Append(driverSegmentDoneProcess.ActionDateTime);
                    sbComment.Append(" Trip:");
                    sbComment.Append(tripSegment.TripNumber);
                    sbComment.Append("-");
                    sbComment.Append(tripSegment.TripSegNumber);
                    sbComment.Append(" Drv:");
                    sbComment.Append(driverSegmentDoneProcess.EmployeeId);
                    sbComment.Append(" Pwr:");
                    sbComment.Append(driverSegmentDoneProcess.PowerId);
                    string comment = sbComment.ToString().Trim();

                    var eventLog = new EventLog()
                    {
                        EventDateTime = driverSegmentDoneProcess.ActionDateTime,
                        EventSeqNo = 0,
                        EventTerminalId = employeeMaster.TerminalId,
                        EventRegionId = employeeMaster.RegionId,
                        //These are not populated in the current system.
                        // EventEmployeeId = driverStatus.EmployeeId,
                        // EventEmployeeName = Common.GetEmployeeName(employeeMaster),
                        EventTripNumber = driverSegmentDoneProcess.TripNumber,
                        EventProgram = EventProgramConstants.Services,
                        //These are not populated in the current system.
                        //EventScreen = null,
                        //EventAction = null,
                        EventComment = comment,
                    };

                    ChangeSetResult<int> eventChangeSetResult = Common.UpdateEventLog(dataService, settings, eventLog);
                    log.Debug("SRTEST:Saving EventLog Record - Segment Canceled");
                    log.DebugFormat("SRTEST:Saving EventLog Record for Trip:{0}-{1} - Segment Canceled.",
                                    currentTripSegment.TripNumber, currentTripSegment.TripSegNumber);
                    //Check for EventLog failure.
                    if (Common.LogChangeSetFailure(eventChangeSetResult, eventLog, log))
                    {
                        var s = string.Format("Segment Canceled:Could not update EventLog for Trip:{0}-{1}.",
                                currentTripSegment.TripNumber, currentTripSegment.TripSegNumber);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        return false;
                    }
                }
            }

            //Since removed the rest of the segments, mark the trip done
            if (!MarkTripDone(dataService, settings, changeSetResult, msgKey, userRoleIds, userCulture,
                                 driverSegmentDoneProcess, employeeMaster, powerMaster, destCustomerMaster,
                                 currentTrip, tripSegList, tripContainerList, tripReferenceNumberList,
                                 tripMileageList, tripDelayList, Constants.No))
            {
                return false;
            }

            ////////////////////////////////////////////////
            //Update the DriverStatus table. Do this after the trip has been marked done
            driverStatus.PowerId = driverSegmentDoneProcess.PowerId;
            driverStatus.MDTId = driverSegmentDoneProcess.Mdtid;
            driverStatus.ActionDateTime = driverSegmentDoneProcess.ActionDateTime;

            //The odometer is not used in the current ScrapRunner processing of a segment done.
            //Use the last odometer stored in the PowerMaster record
            driverStatus.Odometer = powerMaster.PowerOdometer;

            //Determine the Next Incomplete Trip For Driver
            var nextTrip = Common.GetNextTripForDriver(dataService, settings, userCulture, userRoleIds,
                               driverSegmentDoneProcess.EmployeeId, currentTrip.TripNumber, out fault);

            if (fault != null)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
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
                    return false;
                }
                if (null != nextTripSegment)
                {
                    driverStatus.TripSegNumber = nextTripSegment.TripSegNumber;
                    driverStatus.TripSegType = nextTripSegment.TripSegType;
                }
            }
            //Do the update
            changeSetResult = Common.UpdateDriverStatus(dataService, settings, driverStatus);
            log.DebugFormat("SRTEST:Saving DriverStatus Record for DriverId:{0} - Mark Done.",
                            driverStatus.EmployeeId);
            if (Common.LogChangeSetFailure(changeSetResult, driverStatus, log))
            {
                var s = string.Format("Segment Cancel:Could not update DriverStatus for DriverId:{0}.",
                    driverStatus.EmployeeId);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }

            //Do not add to Driver History, not necessary when removing segments

            //Now do the update of the trip table
            changeSetResult = Common.UpdateTrip(dataService, settings, currentTrip);
            log.DebugFormat("SRTEST:Saving Trip Record for Trip:{0} - Segment Cancel.",
                            currentTrip.TripNumber);
            if (Common.LogChangeSetFailure(changeSetResult, currentTrip, log))
            {
                var s = string.Format("Segment Cancel:Could not update Trip for Trip:{0}.",
                    currentTrip.TripNumber);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }

            ////////////////////////////////////////////////
            //Adjust Origins to exclude the canceled segment.
            //Get all of the trips that have been assigned to the driver.
            //First, pending/missed trips dispatched to or acknowledged by the driver.
            //Followed by pending/missed trips assigned to the driver.
            //Followed by future trips assigned or dispatched to the driver.
            var allTripList = Common.GetAllTripsForDriver(dataService, settings, userCulture, userRoleIds,
                              employeeMaster.EmployeeId, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            if (allTripList != null && allTripList.Count() > 0)
            {
                //Don't need to get the last completed trip, since the origin of his current trip did not change.
                //Get all of the segments for all of the driver's trips keeping them in the same order as allTripList.
                var allTripSegmentList = Common.GetAllTripSegmentsForTripList(dataService, settings, userCulture,
                                            userRoleIds, allTripList, out fault);

                //Remove the current trip segment since it has been canceled by the driver.
                allTripSegmentList.Remove(currentTripSegment);
                //Now call the method of set the origins as needed. Only ones where the origins are changed
                //will be updated.
                changeSetResult = Common.SetTripSegmentOrigins(dataService, settings, allTripSegmentList);
            }
            return true;

        }

        /// <summary>
        /// Driver completed segment
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="changeSetResult"></param>
        /// <param name="msgKey"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="userCulture"></param>
        /// <param name="driverSegmentDoneProcess"></param>
        /// <param name="employeeMaster"></param>
        /// <param name="powerMaster"></param>
        /// <param name="destCustomerMaster"></param>
        /// <param name="currentTrip"></param>
        /// <param name="tripSegList"></param>
        /// <param name="currentTripSegment"></param>
        /// <param name="tripContainerList"></param>
        /// <param name="tripReferenceNumberList"></param>
        /// <param name="tripMileageList"></param>
        /// <param name="tripDelayList"></param>
        /// <returns></returns>
        private bool SegmentDone(IDataService dataService, ProcessChangeSetSettings settings,
           ChangeSetResult<string> changeSetResult, String msgKey, long[] userRoleIds, string userCulture,
           DriverSegmentDoneProcess driverSegmentDoneProcess, EmployeeMaster employeeMaster, PowerMaster powerMaster,
           CustomerMaster destCustomerMaster, Trip currentTrip, List<TripSegment> tripSegList, TripSegment currentTripSegment,
           List<TripSegmentContainer> tripContainerList, List<TripReferenceNumber> tripReferenceNumberList,
           List<TripSegmentMileage> tripMileageList, List<DriverDelay> tripDelayList)
        {
            DataServiceFault fault;
            int driverHistoryInsertCount = 0;

            ////////////////////////////////////////////////
            //Get a list of all containers for the segment. 
            var tripSegContainerList = (from item in tripContainerList
                                        where item.TripSegNumber == driverSegmentDoneProcess.TripSegNumber
                                        select item).ToList();
            ////////////////////////////////////////////////
            //Start Updating...
            ////////////////////////////////////////////////

            ////////////////////////////////////////////////
            //Exception Delay Processing begins here
            // Check for any delays for the segment that have been flagged as exceptions 
            //(Y in CodeDisp4 field of the DELAYCODE code table entry). 
            ////////////////////////////////////////////////
            //Only applies to segments that are not RT or RN types.
            if (currentTripSegment.TripSegType != BasicTripTypeConstants.ReturnYard &&
                currentTripSegment.TripSegType != BasicTripTypeConstants.ReturnYardNC)
            {
                // Preferences:  Lookup the yard preference "DEFMarkDelayException".  
                string prefDefMarkDelayException = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                       currentTrip.TripTerminalId, PrefYardConstants.DEFMarkDelayException, out fault);
                if (null != fault)
                {
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                    return false;
                }
                //DEFMarkExceptionDelay Preference must be set to Y
                if (prefDefMarkDelayException == Constants.Yes)
                {
                    //Get a list of codes from the DELAYCODE code table that are flagged as exception delays.
                    var exceptionDelayCodes = Common.GetDelayExceptionCodes(dataService, settings, userCulture,
                                              userRoleIds, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        return false;
                    }
                    //At least one delay code that was flagged an an exception type delay has to be found in the code table
                    if (exceptionDelayCodes != null && exceptionDelayCodes.Count() > 0)
                    {
                        //Look in the list of delays from the trip for the current segment to see 
                        //if any are of the type of delays that are flagged as exception delays
                        var exceptionTripDelay = from delay in tripDelayList
                                                 where delay.TripSegNumber == currentTripSegment.TripSegNumber &&
                                                 exceptionDelayCodes.Contains(delay.DelayCode)
                                                 select delay;

                        //If any delay was an exception type, change the status of the segment to E=Exception
                        if (exceptionTripDelay != null && exceptionTripDelay.Count() > 0)
                        {
                            // Each container is flagged with review flag of E and review reason of ED#-EXCEPTION DELAY
                            foreach (var tripSegmentContainer in tripSegContainerList)
                            {
                                tripSegmentContainer.TripSegContainerReviewFlag = ContainerActionTypeConstants.Exception;
                                tripSegmentContainer.TripSegContainerReviewReason = Constants.ExceptionDelayCode + "=" +
                                                                                    Constants.ExceptionDelayDesc;
                                //Do the update
                                changeSetResult = Common.UpdateTripSegmentContainer(dataService, settings, tripSegmentContainer);
                                log.DebugFormat("SRTEST:Saving TripSegmentContainer for ContainerNumber:{0} - Segment Done.",
                                                tripSegmentContainer.TripSegContainerNumber);
                                if (Common.LogChangeSetFailure(changeSetResult, tripSegmentContainer, log))
                                {
                                    var s = string.Format("Could not update TripSegmentContainer for ContainerNumber:{0}.",
                                                tripSegmentContainer.TripSegContainerNumber);
                                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                    return false;
                                }
                            }
                            // The segment status is set to E=Excepton. 
                            currentTripSegment.TripSegStatus = TripSegStatusConstants.Exception;
                            currentTripSegment.TripSegComments = Constants.ExceptionDelayCode + "=" +
                                                                 Constants.ExceptionDelayDesc;
                        }//if (exceptionTripDelay != null && exceptionTripDelay.Count() > 0)
                    }//if (exceptionDelayCodes != null && exceptionDelayCodes.Count() > 0)
                }//if (prefDefMarkExceptionDelay == Constants.Yes)
            }//if (currentTripSegment.TripSegType != BasicTripTypeConstants.ReturnYard &&

            ////////////////////////////////////////////////
            //Update the current TripSegment record.
            currentTripSegment.TripSegPowerId = driverSegmentDoneProcess.PowerId;
            currentTripSegment.TripSegPowerAssetNumber = powerMaster.PowerAssetNumber;
            currentTripSegment.TripSegDriverId = driverSegmentDoneProcess.EmployeeId;
            currentTripSegment.TripSegDriverName = Common.GetEmployeeName(employeeMaster);
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
                         where item.TripSegContainerReviewFlag == ContainerActionTypeConstants.Review
                         select item;
            if (review != null && review.Count() > 0)
            {
                currentTripSegment.TripSegStatus = TripSegStatusConstants.Review;
                currentTripSegment.TripSegComments = review.FirstOrDefault().TripSegContainerReviewReason;

            }
            else
            {
                //If any container is an exception, set segment status to E=Exception
                var exception = from item in tripSegContainerList
                                where item.TripSegContainerReviewFlag == ContainerActionTypeConstants.Exception
                                select item;
                if (exception != null && exception.Count() > 0)
                {
                    currentTripSegment.TripSegStatus = TripSegStatusConstants.Exception;
                    currentTripSegment.TripSegComments = exception.FirstOrDefault().TripSegContainerReviewReason;

                }
            }
            //Check if the trip should be placed in the Error Queue
            //If so,  set the Trip.TripStatus to TS_ERRQ (Q)
            //Set the Trip.TripErrorDesc to EXCESS TIME, EXCESS MILEAGE, INSUFFICIENT TIME, INSUFFICIENT MILEAGE
            ////////////////////////////////////////////////
            // Preferences:  Lookup the yard preference "DEFUseErrorQ".  
            string DEFUseErrorQ = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                   currentTrip.TripTerminalId, PrefYardConstants.DEFUseErrorQ, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            // If preference DEFUseErrorQ set to Y then use the error queue.
            if (DEFUseErrorQ == Constants.Yes)
            {
                string reasonInErrorQueue;
                string reasonInErrorQueueDetails;

                if (Common.CheckForErrorQueue(dataService,settings,userCulture, userRoleIds, 
                                  currentTrip,currentTripSegment,tripDelayList, 
                                  out reasonInErrorQueue, out reasonInErrorQueueDetails, out fault))
                {
                    currentTripSegment.TripSegErrorDesc = reasonInErrorQueueDetails;
                    if (currentTripSegment.TripSegErrorDesc.Length > MaxLengthConstants.LENTripSegErrorDesc)
                        currentTripSegment.TripSegErrorDesc = currentTripSegment.TripSegErrorDesc.Substring(0, MaxLengthConstants.LENTripSegErrorDesc);
                    currentTripSegment.TripSegStatus = TripSegStatusConstants.ErrorQueue;

                    currentTrip.TripErrorDesc = reasonInErrorQueue;
                    if (currentTrip.TripErrorDesc.Length > MaxLengthConstants.LENTripErrorDesc)
                        currentTrip.TripErrorDesc = currentTrip.TripErrorDesc.Substring(0, MaxLengthConstants.LENTripErrorDesc);
                  }
                }

            //Lookup the TripSegStatus Description in the CodeTable TRIPSEGSTATUS 
            var codeTableTripSegStatus = Common.GetCodeTableEntry(dataService, settings, userCulture, userRoleIds,
                                         CodeTableNameConstants.TripSegStatus, currentTripSegment.TripSegStatus, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
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
            //Do the TripSegment table update
            changeSetResult = Common.UpdateTripSegment(dataService, settings, currentTripSegment);
            log.DebugFormat("SRTEST:Saving TripSegment Record for Trip:{0}-{1} - Segment Done.",
                            currentTripSegment.TripNumber, currentTripSegment.TripSegNumber);
            if (Common.LogChangeSetFailure(changeSetResult, currentTripSegment, log))
            {
                var s = string.Format("Segment Done:Could not update TripSegment for Trip:{0}-{1}.",
                    currentTripSegment.TripNumber, currentTripSegment.TripSegNumber);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }
            ////////////////////////////////////////////////
            //Check if next segment is at the same location as current segment
            //If so, set TripSegStartDateTime for next segment.
            //Set TripSegActualDriveStartDateTime, TripSegActualDriveEndDateTime,TripSegActualStopStartDateTime for next segment.
            string hostcode = null;
            foreach (var tripSegment in tripSegList)
            {
                if (tripSegment.TripSegNumber == driverSegmentDoneProcess.TripSegNumber)
                {
                    //This is our current destination customer.
                    hostcode = tripSegment.TripSegDestCustHostCode;
                }
                //Look for subsequent segments with the same destination customer.
                //The first string follows the second string in the sort order.
                else if (1 == tripSegment.TripSegNumber.CompareTo(driverSegmentDoneProcess.TripSegNumber))
                {
                    if (hostcode == tripSegment.TripSegDestCustHostCode)
                    {
                        //Since destination of the next segment is the same as the current segment, set the times.
                        tripSegment.TripSegStartDateTime = driverSegmentDoneProcess.ActionDateTime;
                        tripSegment.TripSegActualDriveStartDateTime = driverSegmentDoneProcess.ActionDateTime;
                        tripSegment.TripSegActualDriveEndDateTime = driverSegmentDoneProcess.ActionDateTime;
                        tripSegment.TripSegActualStopStartDateTime = driverSegmentDoneProcess.ActionDateTime;
                        tripSegment.TripSegActualDriveMinutes = 0;

                        //Do the update
                        changeSetResult = Common.UpdateTripSegment(dataService, settings, tripSegment);
                        log.DebugFormat("SRTEST:Saving next TripSegment Record for Trip:{0}-{1} - Segment Done.",
                                        tripSegment.TripNumber, tripSegment.TripSegNumber);
                        if (Common.LogChangeSetFailure(changeSetResult, tripSegment, log))
                        {
                            var s = string.Format("Segment Done:Could not update next TripSegment for Trip:{0}-{1}.",
                                tripSegment.TripNumber, tripSegment.TripSegNumber);
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

             ///////////////////////////////////////////////////////////////////
             //If the current trip segment is a LD (Load), UL(Unload), PF(Pickup Full), or PE(Pickup Empty)
             //and the next trip segment is a RT, delete all containers from the next segment
             //and add containers from the current segment to the next segment
             if (currentTripSegment.TripSegType == BasicTripTypeConstants.Load ||
                 currentTripSegment.TripSegType == BasicTripTypeConstants.Unload ||
                 currentTripSegment.TripSegType == BasicTripTypeConstants.PickupFull ||
                 currentTripSegment.TripSegType == BasicTripTypeConstants.PickupEmpty)
            {
                 // Get the next TripSegment record
                int intNo = Int32.Parse(driverSegmentDoneProcess.TripSegNumber);
                intNo++;
                string segNo = intNo.ToString("D2");
                var nextTripSegment = (from item in tripSegList
                                       where item.TripSegNumber == segNo
                                       select item).FirstOrDefault();
                if (null != nextTripSegment)
                {
                    //If the next segment type is a RT (Return to Yard)
                    if (nextTripSegment.TripSegType == BasicTripTypeConstants.ReturnYard)
                    {
                        //Delete any existing trip segment container records for this segment
                        var oldTripSegmentContainerList = Common.GetTripSegmentContainers(dataService, settings, userCulture, userRoleIds,
                                        currentTrip.TripNumber, segNo, out fault);
                        foreach (var oldSegmentContainer in oldTripSegmentContainerList)
                        {
                            //Do the delete. Deleting records with composite keys is now fixed.
                            changeSetResult = Common.DeleteTripSegmentContainer(dataService, settings, oldSegmentContainer);
                            log.DebugFormat("SRTEST:DriverSegmentDoneProcess:Deleting TripSegmentContainer Record for Trip:{0}-{1} Seq:{2}- Done.",
                                            oldSegmentContainer.TripNumber, oldSegmentContainer.TripSegNumber,
                                            oldSegmentContainer.TripSegContainerSeqNumber);
                            if (Common.LogChangeSetFailure(changeSetResult, oldSegmentContainer, log))
                            {
                                var s = string.Format("DriverSegmentDoneProcess:Could not delete TripSegmentContainer for Trip:{0}-{1} Seq:{2}.",
                                        oldSegmentContainer.TripNumber, oldSegmentContainer.TripSegNumber,
                                        oldSegmentContainer.TripSegContainerSeqNumber);
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                break;
                            }
                        }//foreach (var oldSegmentContainer in oldTripSegmentContainerList)

                        //Add containers from the current segment to the next segment.
                        var firstTripSegmentContainer = new TripSegmentContainer();
                        int idx = 0;
                        foreach (var tripSegContainer in tripSegContainerList)
                        {
                            var newSegmentContainer = new TripSegmentContainer();

                            newSegmentContainer.TripNumber = tripSegContainer.TripNumber;
                            newSegmentContainer.TripSegNumber = segNo;
                            newSegmentContainer.TripSegContainerSeqNumber = tripSegContainer.TripSegContainerSeqNumber;
                            newSegmentContainer.TripSegContainerNumber = tripSegContainer.TripSegContainerNumber;
                            newSegmentContainer.TripSegContainerType = tripSegContainer.TripSegContainerType;
                            newSegmentContainer.TripSegContainerSize = tripSegContainer.TripSegContainerSize;
                            newSegmentContainer.TripSegContainerCommodityCode = tripSegContainer.TripSegContainerCommodityCode;
                            newSegmentContainer.TripSegContainerCommodityDesc = tripSegContainer.TripSegContainerCommodityDesc;
                            newSegmentContainer.TripSegContainerLevel = tripSegContainer.TripSegContainerLevel;
                            newSegmentContainer.TripSegContainerActionDateTime = tripSegContainer.TripSegContainerActionDateTime;
                            if (idx == 0)
                            {
                                firstTripSegmentContainer = newSegmentContainer;
                            }
                            idx++;

                            //Do the update for the new trip segment container records
                            changeSetResult = Common.UpdateTripSegmentContainer(dataService, settings, newSegmentContainer);
                            log.DebugFormat("SRTEST:Saving TripSegmentContainer Record for Trip:{0}-{1} Seq:{2}- Done.",
                                            newSegmentContainer.TripNumber, newSegmentContainer.TripSegNumber,
                                            newSegmentContainer.TripSegContainerSeqNumber);
                            if (Common.LogChangeSetFailure(changeSetResult, powerMaster, log))
                            {
                                var s = string.Format("Segment Done:Could not update TripSegmentContainer Record for Trip:{0}-{1} Seq:{2}.",
                                            newSegmentContainer.TripNumber, newSegmentContainer.TripSegNumber,
                                            newSegmentContainer.TripSegContainerSeqNumber);
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                                return false;
                            }
                        }

                        //Now set the primary trip segment information for the next segment
                        nextTripSegment.TripSegPrimaryContainerNumber = firstTripSegmentContainer.TripSegContainerNumber;
                        nextTripSegment.TripSegPrimaryContainerType = firstTripSegmentContainer.TripSegContainerType;
                        nextTripSegment.TripSegPrimaryContainerSize = firstTripSegmentContainer.TripSegContainerSize;
                        nextTripSegment.TripSegPrimaryContainerCommodityCode = firstTripSegmentContainer.TripSegContainerCommodityCode;
                        nextTripSegment.TripSegPrimaryContainerCommodityDesc = firstTripSegmentContainer.TripSegContainerCommodityDesc;
                        nextTripSegment.TripSegPrimaryContainerLocation = firstTripSegmentContainer.TripSegContainerLocation;

                        ////////////////////////////////////////////////
                        //Do the TripSegment table update for the next trip segment
                        changeSetResult = Common.UpdateTripSegment(dataService, settings, nextTripSegment);
                        log.DebugFormat("SRTEST:Saving TripSegment Record for Trip:{0}-{1} - Segment Add.",
                                        nextTripSegment.TripNumber, nextTripSegment.TripSegNumber);
                        if (Common.LogChangeSetFailure(changeSetResult, nextTripSegment, log))
                        {
                            var s = string.Format("Segment Add:Could not update TripSegment for Trip:{0}-{1}.",
                                nextTripSegment.TripNumber, nextTripSegment.TripSegNumber);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            return false;
                        }
                    }//end of if (nextTripSegment.TripSegType == BasicTripTypeConstants.ReturnYard)
                }

            }

            ///////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////
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
            }
            //Set the multiple container flag at the trip level
            if (currentTripSegment.TripSegContainerQty > 1)
            {
                currentTrip.TripMultContainerFlag = Constants.Yes;
            }

            //Set the Trip In Progress flag to Y even though it should already have been set to Y in the enroute process.
            currentTrip.TripInProgressFlag = Constants.Yes;

            //Set the auto email receipt flag in the trip record
            currentTrip.TripSendReceiptFlag = currentTripSegment.TripSegSendReceiptFlag;

            //Do not update trip table yet. May need to make more changes to the record, if this is the last segment.

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
                var s = string.Format("Segment Done:Could not update PowerMaster for PowerId:{0}.",
                                      driverSegmentDoneProcess.PowerId);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }

            ////////////////////////////////////////////////
            //Update Customer Master with GPS co-ordinates.
            if (driverSegmentDoneProcess.Latitude != null && driverSegmentDoneProcess.Longitude != null)
            {
                if (destCustomerMaster.CustLatitude == null || destCustomerMaster.CustLongitude == null)
                {
                    destCustomerMaster.CustLatitude = driverSegmentDoneProcess.Latitude;
                    destCustomerMaster.CustLongitude = driverSegmentDoneProcess.Longitude;
                    destCustomerMaster.CustGeoCoded = Constants.Yes;

                    //Do the update
                    changeSetResult = Common.UpdateCustomerMaster(dataService, settings, destCustomerMaster);
                    log.DebugFormat("SRTEST:Saving CustomerMaster Record for CustHostCode:{0} - Segment Done.",
                                    currentTripSegment.TripSegDestCustHostCode);
                    if (Common.LogChangeSetFailure(changeSetResult, destCustomerMaster, log))
                    {
                        var s = string.Format("Segment Done:Could not update CustomerMaster for CustHostCode:{0}.",
                                              currentTripSegment.TripSegDestCustHostCode);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        return false;
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
                //These are not populated in the current system.
                // EventEmployeeId = driverStatus.EmployeeId,
                // EventEmployeeName = Common.GetEmployeeName(employeeMaster),
                EventTripNumber = driverSegmentDoneProcess.TripNumber,
                EventProgram = EventProgramConstants.Services,
                //These are not populated in the current system.
                //EventScreen = null,
                //EventAction = null,
                EventComment = comment,
            };

            ChangeSetResult<int> eventChangeSetResult;
            eventChangeSetResult = Common.UpdateEventLog(dataService, settings, eventLog);
            log.Debug("SRTEST:Saving EventLog Record - Segment Done");
            log.DebugFormat("SRTEST:Saving EventLog Record for Trip:{0}-{1} - Segment Done.",
                          currentTripSegment.TripNumber, currentTripSegment.TripSegNumber);
            //Check for EventLog failure.
            if (Common.LogChangeSetFailure(eventChangeSetResult, eventLog, log))
            {
                var s = string.Format("Segment Done:Could not update EventLog for Trip:{0}-{1}.",
                        currentTripSegment.TripNumber, currentTripSegment.TripSegNumber);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }

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
                    if (!MarkTripDone(dataService, settings, changeSetResult, msgKey, userRoleIds, userCulture,
                                        driverSegmentDoneProcess, employeeMaster, powerMaster, destCustomerMaster,
                                        currentTrip, tripSegList, tripContainerList, tripReferenceNumberList,
                                        tripMileageList, tripDelayList, DEFUseErrorQ))
                    {
                        return false;
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
                return false;
            }
            if (driverStatus == null)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Segment Done:Invalid DriverId: "
                              + driverSegmentDoneProcess.EmployeeId));
                return false;
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
            //    driverStatus.GPSXmitFlag = Constants.No;
            //else
            //    driverStatus.GPSXmitFlag = Constants.Yes;

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
            if (!Common.InsertDriverHistory(dataService, settings, driverStatus, employeeMaster, currentTripSegment,
                ++driverHistoryInsertCount, userRoleIds, userCulture, log, out fault))
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                log.ErrorFormat("InsertDriverHistory failed: {0} during segment done request: {1}", fault.Message, driverSegmentDoneProcess);
                return false;
            }

            //Now make the final changes to the driver status if the trip has been marked done.
            //We need to be able to display his new status and his next trip information.
            if (lastTripSegment.TripSegNumber == driverSegmentDoneProcess.TripSegNumber)
            {
                //Determine the Next Incomplete Trip For Driver
                var nextTrip = Common.GetNextTripForDriver(dataService, settings, userCulture, userRoleIds,
                                   driverSegmentDoneProcess.EmployeeId, currentTrip.TripNumber, out fault);

                if (fault != null)
                {
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                    return false;
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
                        return false;
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
                var s = string.Format("Segment Done:Could not update DriverStatus for DriverId:{0}.",
                    driverStatus.EmployeeId);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }

            //Now do the update of the trip table
            changeSetResult = Common.UpdateTrip(dataService, settings, currentTrip);
            log.DebugFormat("SRTEST:Saving Trip Record for Trip:{0} - Segment Done.",
                            currentTrip.TripNumber);
            if (Common.LogChangeSetFailure(changeSetResult, currentTrip, log))
            {
                var s = string.Format("Segment Add:Could not update Trip for Trip:{0}.",
                    currentTrip.TripNumber);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }

            return true;
        }

        private bool SegmentAdd(IDataService dataService, ProcessChangeSetSettings settings,
           ChangeSetResult<string> changeSetResult, String msgKey, long[] userRoleIds, string userCulture,
           DriverSegmentDoneProcess driverSegmentDoneProcess, EmployeeMaster employeeMaster,
           List<TripSegment> tripSegList, List<TripSegmentContainer> tripContainerList)
        {
            DataServiceFault fault;
            ////////////////////////////////////////////////
            // Get the Customer record for the destination cust host code
            var destCustomerMaster = Common.GetCustomer(dataService, settings, userCulture, userRoleIds,
                             driverSegmentDoneProcess.DestCustHostCode, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            //End processing if null
            if (null == destCustomerMaster)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Segment Add:Invalid CustHostCode: "
                                + driverSegmentDoneProcess.DestCustHostCode));
                return false;
            }

            /////////////////////////////////////////
            //Get the current trip segment which should be the last segment in the list
            var currentTripSegment = (from item in tripSegList                                   
                                      select item).LastOrDefault();
            //End the process if null
            if (null == currentTripSegment)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Segment Add:Invalid TripSegment: " +
                    driverSegmentDoneProcess.TripNumber + "-" + driverSegmentDoneProcess.TripSegNumber));
                return false;
            }

            //////////////////////////////////////////////////////////////////////////////////////
            //Set the values for the new Segment
            var newTripSegment = new TripSegment();

            newTripSegment.TripNumber = currentTripSegment.TripNumber;
            //Use the new segment number from the mobile app
            newTripSegment.TripSegNumber = driverSegmentDoneProcess.TripSegNumber;

            //////////////////////////////////////////////////////////////////////////////////////
            //Set the new segment status to pending
            newTripSegment.TripSegStatus = TripSegStatusConstants.Pending;
            //Lookup the TripSegStatus Description in the CodeTable TRIPSEGSTATUS 
            var codeTableTripSegStatus = Common.GetCodeTableEntry(dataService, settings, userCulture, userRoleIds,
                                    CodeTableNameConstants.TripSegStatus, TripSegStatusConstants.Pending, out fault);
            if (null != fault)
            {
                return false;
            }
            newTripSegment.TripSegStatusDesc = codeTableTripSegStatus?.CodeDisp1;

            //////////////////////////////////////////////////////////////////////////////////////
            //Use the new segment type from the mobile app
            newTripSegment.TripSegType = driverSegmentDoneProcess.TripSegType;
            //Lookup the Basic Trip Type Description in the TripTypeBasic table 
            var tripTypeBasic = Common.GetTripTypeBasic(dataService, settings, userCulture, userRoleIds,
                                driverSegmentDoneProcess.TripSegType, out fault);
            if (null != fault)
            {
                return false;
            }
            newTripSegment.TripSegTypeDesc = tripTypeBasic?.TripTypeDesc;

            newTripSegment.TripSegDriverId = driverSegmentDoneProcess.EmployeeId;
            newTripSegment.TripSegDriverName = Common.GetEmployeeName(employeeMaster);
            newTripSegment.TripSegPowerId = driverSegmentDoneProcess.PowerId;

            //Set the origin customer information from the destination of the current segment
            newTripSegment.TripSegOrigCustType = currentTripSegment.TripSegDestCustType;
            newTripSegment.TripSegOrigCustTypeDesc = currentTripSegment.TripSegDestCustTypeDesc;
            newTripSegment.TripSegOrigCustHostCode = currentTripSegment.TripSegDestCustHostCode;
            newTripSegment.TripSegOrigCustCode4_4 = currentTripSegment.TripSegDestCustCode4_4;
            newTripSegment.TripSegOrigCustName = currentTripSegment.TripSegDestCustName;
            newTripSegment.TripSegOrigCustAddress1 = currentTripSegment.TripSegDestCustAddress1;
            newTripSegment.TripSegOrigCustAddress2 = currentTripSegment.TripSegDestCustAddress2;
            newTripSegment.TripSegOrigCustCity = currentTripSegment.TripSegDestCustCity;
            newTripSegment.TripSegOrigCustState = currentTripSegment.TripSegDestCustState;
            newTripSegment.TripSegOrigCustZip = currentTripSegment.TripSegDestCustZip;
            newTripSegment.TripSegOrigCustCountry = currentTripSegment.TripSegDestCustCountry;
            newTripSegment.TripSegOrigCustPhone1 = currentTripSegment.TripSegDestCustPhone1;
            newTripSegment.TripSegOrigCustTimeFactor = currentTripSegment.TripSegDestCustTimeFactor;

            //Set the destination customer information from the destination host code supplied by the mobile app
            newTripSegment.TripSegDestCustType = destCustomerMaster.CustType;
            //Look up the description in the CodeTable CUSTOMERTYPE
            var codeTableDestCustomerType = Common.GetCodeTableEntry(dataService, settings, userCulture, userRoleIds,
                                        CodeTableNameConstants.CustomerType, destCustomerMaster.CustType, out fault);
            if (null != fault)
            {
                return false;
            }
            newTripSegment.TripSegDestCustTypeDesc = codeTableDestCustomerType?.CodeDisp1;

            newTripSegment.TripSegDestCustHostCode = destCustomerMaster.CustHostCode;
            newTripSegment.TripSegDestCustCode4_4 = destCustomerMaster.CustCode4_4;
            newTripSegment.TripSegDestCustName = destCustomerMaster.CustName;
            newTripSegment.TripSegDestCustAddress1 = destCustomerMaster.CustAddress1;
            newTripSegment.TripSegDestCustAddress2 = destCustomerMaster.CustAddress2;
            newTripSegment.TripSegDestCustCity = destCustomerMaster.CustCity;
            newTripSegment.TripSegDestCustState = destCustomerMaster.CustState;
            newTripSegment.TripSegDestCustZip = destCustomerMaster.CustZip;
            newTripSegment.TripSegDestCustCountry = destCustomerMaster.CustCountry;
            newTripSegment.TripSegDestCustPhone1 = destCustomerMaster.CustPhone1;
            newTripSegment.TripSegDestCustTimeFactor = destCustomerMaster.CustTimeFactor;

            //Try to find a matching entry in the TripPoints table based on origin host code and destination host code
            var tripPoints = Common.GetTripPoints(dataService, settings, userCulture, userRoleIds,
                             newTripSegment.TripSegOrigCustHostCode, newTripSegment.TripSegDestCustHostCode, out fault);
            if (null != fault)
            {
                return false;
            }
            if (tripPoints == null)
            {
                newTripSegment.TripSegStandardDriveMinutes = 0;
                newTripSegment.TripSegStandardMiles = 0;

                ///////////////////////////////////////////////////////////////
                //Send off a request to maps
                tripPoints = new TripPoints();
                tripPoints.TripPointsHostCode1 = newTripSegment.TripSegOrigCustHostCode;
                tripPoints.TripPointsHostCode2 = newTripSegment.TripSegDestCustHostCode;
                tripPoints.TripPointsSendToMaps = TripSendToMapsValue.Ready;
                tripPoints.ChgDateTime = driverSegmentDoneProcess.ActionDateTime;
                tripPoints.ChgEmployeeId = driverSegmentDoneProcess.EmployeeId;

                //Do the TripPoints table update
                changeSetResult = Common.UpdateTripPoints(dataService, settings, tripPoints);
                log.DebugFormat("SRTEST:Saving TripPoints Record for Trip:{0}-{1} Orig:{2} Dest:{3} - Segment Add.",
                                newTripSegment.TripNumber, newTripSegment.TripSegNumber,
                                newTripSegment.TripSegOrigCustHostCode, newTripSegment.TripSegDestCustHostCode);
                if (Common.LogChangeSetFailure(changeSetResult, newTripSegment, log))
                {
                    var s = string.Format("Segment Add:Could not update TripPoints for Trip:{0}-{1} Orig:{2} Dest:{3}.",
                            newTripSegment.TripNumber, newTripSegment.TripSegNumber,
                            newTripSegment.TripSegOrigCustHostCode, newTripSegment.TripSegDestCustHostCode);
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                    return false;
                }
            }
            else
            {
                newTripSegment.TripSegStandardMiles = tripPoints.TripPointsStandardMiles;
                newTripSegment.TripSegStandardDriveMinutes = tripPoints.TripPointsStandardMinutes;
                //ToDo: Do we recalculate the minutes based on miles, just in case it changed?
                //We do that in the current ScrapRunner
            }

            //ToDo:Calculate the standard stop time.
            newTripSegment.TripSegStandardStopMinutes = 0;

            //Set driver generated flag to Y
            newTripSegment.TripSegDriverGenerated = driverSegmentDoneProcess.DriverGenerated;

            ////////////////////////////////////////////////
            //Get a list of all containers for the segment. 
            var tripSegContainerList = (from item in tripContainerList
                                        where item.TripSegNumber == driverSegmentDoneProcess.TripSegNumber
                                        select item).ToList();

            //Get the number of containers on this segment
            newTripSegment.TripSegContainerQty = tripSegContainerList.Count();

            ////////////////////////////////////////////////
            //Update TripSegment Primary Container Information from first TripSegmentContainer information. 
            if (tripSegContainerList.Count() > 0)
            {
                var firstTripSegmentContainer = tripSegContainerList.First();

                if (null != firstTripSegmentContainer)
                {
                    //Only if there is a container number
                    if (null != firstTripSegmentContainer.TripSegContainerNumber)
                    {
                        newTripSegment.TripSegPrimaryContainerNumber = firstTripSegmentContainer.TripSegContainerNumber;
                        newTripSegment.TripSegPrimaryContainerType = firstTripSegmentContainer.TripSegContainerType;
                        newTripSegment.TripSegPrimaryContainerSize = firstTripSegmentContainer.TripSegContainerSize;
                        newTripSegment.TripSegPrimaryContainerCommodityCode = firstTripSegmentContainer.TripSegContainerCommodityCode;
                        newTripSegment.TripSegPrimaryContainerCommodityDesc = firstTripSegmentContainer.TripSegContainerCommodityDesc;
                        newTripSegment.TripSegPrimaryContainerLocation = firstTripSegmentContainer.TripSegContainerLocation;
                    }
                }
            }

            ////////////////////////////////////////////////
            //Do the TripSegment table update
            changeSetResult = Common.UpdateTripSegment(dataService, settings, newTripSegment);
            log.DebugFormat("SRTEST:Saving TripSegment Record for Trip:{0}-{1} - Segment Add.",
                            newTripSegment.TripNumber, newTripSegment.TripSegNumber);
            if (Common.LogChangeSetFailure(changeSetResult, newTripSegment, log))
            {
                var s = string.Format("Segment Add:Could not update TripSegment for Trip:{0}-{1}.",
                    newTripSegment.TripNumber, newTripSegment.TripSegNumber);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }

            ////////////////////////////////////////////////
            //It seems unecessary to update trip info with primary container info, since this segment is never the first segment.

            ////////////////////////////////////////////////
            //Add entry to Event Log – Driver Added Segment. 
            StringBuilder sbComment = new StringBuilder();
            sbComment.Append(EventCommentConstants.ReceivedDriverAddedSeg);
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
            sbComment.Append(driverSegmentDoneProcess.TripSegType);
            sbComment.Append(" Dest:");
            sbComment.Append(driverSegmentDoneProcess.DestCustHostCode);
            string comment = sbComment.ToString().Trim();

            var eventLog = new EventLog()
            {
                EventDateTime = driverSegmentDoneProcess.ActionDateTime,
                EventSeqNo = 0,
                EventTerminalId = employeeMaster.TerminalId,
                EventRegionId = employeeMaster.RegionId,
                //These are not populated in the current system.
                // EventEmployeeId = driverStatus.EmployeeId,
                // EventEmployeeName = Common.GetEmployeeName(employeeMaster),
                EventTripNumber = driverSegmentDoneProcess.TripNumber,
                EventProgram = EventProgramConstants.Services,
                //These are not populated in the current system.
                //EventScreen = null,
                //EventAction = null,
                EventComment = comment,
            };

            ChangeSetResult<int> eventChangeSetResult;
            eventChangeSetResult = Common.UpdateEventLog(dataService, settings, eventLog);
            log.Debug("SRTEST:Saving EventLog Record - Segment Add");
            log.DebugFormat("SRTEST:Saving EventLog Record for Trip:{0}-{1} - Segment Add.",
                            currentTripSegment.TripNumber, currentTripSegment.TripSegNumber);
            //Check for EventLog failure.
            if (Common.LogChangeSetFailure(eventChangeSetResult, eventLog, log))
            {
                var s = string.Format("Segment Add:Could not update EventLog for Trip:{0}-{1}.",
                        currentTripSegment.TripNumber, currentTripSegment.TripSegNumber);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }

            ////////////////////////////////////////////////
            //Adjust Origins to include the new trip segment.
            //Get all of the trips that have been assigned to the driver.
            //First, pending/missed trips dispatched to or acknowledged by the driver.
            //Followed by pending/missed trips assigned to the driver.
            //Followed by future trips assigned or dispatched to the driver.
            var allTripList = Common.GetAllTripsForDriver(dataService, settings, userCulture, userRoleIds,
                              employeeMaster.EmployeeId, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            if (allTripList != null && allTripList.Count() > 0)
            {
                //Get all of the segments for all of the driver's trips keeping them in the same order as allTripList.
                var allTripSegmentList = Common.GetAllTripSegmentsForTripList(dataService, settings, userCulture,
                                            userRoleIds, allTripList, out fault);
                //Since we added a new segment of the current trip, we need to insert the new trip segment 
                //after the current trip segment.
                int indexCurrentTripSegment = allTripSegmentList.IndexOf(currentTripSegment);
                //Should always be in the list, but just in case...
                if (-1 != indexCurrentTripSegment)
                {
                    allTripSegmentList.Insert(indexCurrentTripSegment+1, newTripSegment);
                }
                //Now call the method of set the origins as needed. Only ones where the origins are changed
                //will be updated.
                changeSetResult = Common.SetTripSegmentOrigins(dataService, settings, allTripSegmentList);
            }

            return true;
        }

        public bool SegmentModify(IDataService dataService, ProcessChangeSetSettings settings,
           ChangeSetResult<string> changeSetResult, String msgKey, long[] userRoleIds, string userCulture,
           DriverSegmentDoneProcess driverSegmentDoneProcess, EmployeeMaster employeeMaster, PowerMaster powerMaster,
           Trip currentTrip, List<TripSegment> tripSegList, List<TripSegmentContainer> tripContainerList)
        {
            DataServiceFault fault;
            ////////////////////////////////////////////////
            // Get the Customer record for the destination cust host code
            var destCustomerMaster = Common.GetCustomer(dataService, settings, userCulture, userRoleIds,
                             driverSegmentDoneProcess.DestCustHostCode, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            if (null == destCustomerMaster)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Segment Modify:Invalid CustHostCode: "
                                + driverSegmentDoneProcess.DestCustHostCode));
                return false;
            }

            ////////////////////////////////////////////////
            // Get the current TripSegment record
            var currentTripSegment = (from item in tripSegList
                                        where item.TripSegNumber == driverSegmentDoneProcess.TripSegNumber
                                        select item).FirstOrDefault();
            if (null == currentTripSegment)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Segment Modify:Invalid TripSegment: " +
                    driverSegmentDoneProcess.TripNumber + "-" + driverSegmentDoneProcess.TripSegNumber));
                return false;
            }

            ////////////////////////////////////////////////
            //Set the destination customer information from the destination host code supplied by the mobile app
            currentTripSegment.TripSegDestCustType = destCustomerMaster.CustType;
            //Look up the description in the CodeTable CUSTOMERTYPE
            var codeTableDestCustomerType = Common.GetCodeTableEntry(dataService, settings, userCulture, userRoleIds,
                                        CodeTableNameConstants.CustomerType, destCustomerMaster.CustType, out fault);
            if (null != fault)
            {
                return false;
            }
            currentTripSegment.TripSegDestCustTypeDesc = codeTableDestCustomerType?.CodeDisp1;

            currentTripSegment.TripSegDestCustHostCode = destCustomerMaster.CustHostCode;
            currentTripSegment.TripSegDestCustCode4_4 = destCustomerMaster.CustCode4_4;
            currentTripSegment.TripSegDestCustName = destCustomerMaster.CustName;
            currentTripSegment.TripSegDestCustAddress1 = destCustomerMaster.CustAddress1;
            currentTripSegment.TripSegDestCustAddress2 = destCustomerMaster.CustAddress2;
            currentTripSegment.TripSegDestCustCity = destCustomerMaster.CustCity;
            currentTripSegment.TripSegDestCustState = destCustomerMaster.CustState;
            currentTripSegment.TripSegDestCustZip = destCustomerMaster.CustZip;
            currentTripSegment.TripSegDestCustCountry = destCustomerMaster.CustCountry;
            currentTripSegment.TripSegDestCustPhone1 = destCustomerMaster.CustPhone1;
            currentTripSegment.TripSegDestCustTimeFactor = destCustomerMaster.CustTimeFactor;

            ////////////////////////////////////////////////
            //ToDo:Calculate the standard drive time and standard stop time and standard miles.
            currentTripSegment.TripSegStandardDriveMinutes = 0;
            currentTripSegment.TripSegStandardStopMinutes = 0;
            currentTripSegment.TripSegStandardMiles = 0;

            //Set driver modified flag to Y
            currentTripSegment.TripSegDriverModified = driverSegmentDoneProcess.DriverModified;

            //Do the TripSegment table update
            changeSetResult = Common.UpdateTripSegment(dataService, settings, currentTripSegment);
            log.DebugFormat("SRTEST:Saving TripSegment Record for Trip:{0}-{1} - Segment Modify.",
                            currentTripSegment.TripNumber, currentTripSegment.TripSegNumber);
            if (Common.LogChangeSetFailure(changeSetResult, currentTripSegment, log))
            {
                var s = string.Format("Segment Modify:Could not update TripSegment for Trip:{0}-{1}.",
                    currentTripSegment.TripNumber, currentTripSegment.TripSegNumber);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }

            ////////////////////////////////////////////////
            //If there is a next segment, modify origin info
            // Get the next TripSegment record
            int intNo = Int32.Parse(driverSegmentDoneProcess.TripSegNumber);
            intNo++;
            string segNo = intNo.ToString("D2");
            var nextTripSegment = (from item in tripSegList
                                      where item.TripSegNumber == segNo
                                      select item).FirstOrDefault();
            if (null != nextTripSegment)
            {
                //Set the origin customer information from the destination of the current segment
                nextTripSegment.TripSegOrigCustType = currentTripSegment.TripSegDestCustType;
                nextTripSegment.TripSegOrigCustTypeDesc = currentTripSegment.TripSegDestCustTypeDesc;
                nextTripSegment.TripSegOrigCustHostCode = currentTripSegment.TripSegDestCustHostCode;
                nextTripSegment.TripSegOrigCustCode4_4 = currentTripSegment.TripSegDestCustCode4_4;
                nextTripSegment.TripSegOrigCustName = currentTripSegment.TripSegDestCustName;
                nextTripSegment.TripSegOrigCustAddress1 = currentTripSegment.TripSegDestCustAddress1;
                nextTripSegment.TripSegOrigCustAddress2 = currentTripSegment.TripSegDestCustAddress2;
                nextTripSegment.TripSegOrigCustCity = currentTripSegment.TripSegDestCustCity;
                nextTripSegment.TripSegOrigCustState = currentTripSegment.TripSegDestCustState;
                nextTripSegment.TripSegOrigCustZip = currentTripSegment.TripSegDestCustZip;
                nextTripSegment.TripSegOrigCustCountry = currentTripSegment.TripSegDestCustCountry;
                nextTripSegment.TripSegOrigCustPhone1 = currentTripSegment.TripSegDestCustPhone1;
                nextTripSegment.TripSegOrigCustTimeFactor = currentTripSegment.TripSegDestCustTimeFactor;

                //Do the TripSegment table update
                changeSetResult = Common.UpdateTripSegment(dataService, settings, nextTripSegment);
                log.DebugFormat("SRTEST:Saving TripSegment Record for Trip:{0}-{1} - Segment Modify.",
                                nextTripSegment.TripNumber, nextTripSegment.TripSegNumber);
                if (Common.LogChangeSetFailure(changeSetResult, nextTripSegment, log))
                {
                    var s = string.Format("Segment Modify:Could not update TripSegment for Trip:{0}-{1}.",
                        nextTripSegment.TripNumber, nextTripSegment.TripSegNumber);
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                    return false;
                }
            }

            ////////////////////////////////////////////////
            //It seems unecessary to update trip info with primary container info, since container info has not changed.

            ////////////////////////////////////////////////
            //Add entry to Event Log – Driver Modified Segment. 
            StringBuilder sbComment = new StringBuilder();
            sbComment.Append(EventCommentConstants.ReceivedDriverModifiedSeg);
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
            sbComment.Append(" Dest:");
            sbComment.Append(driverSegmentDoneProcess.DestCustHostCode);
            string comment = sbComment.ToString().Trim();

            var eventLog = new EventLog()
            {
                EventDateTime = driverSegmentDoneProcess.ActionDateTime,
                EventSeqNo = 0,
                EventTerminalId = employeeMaster.TerminalId,
                EventRegionId = employeeMaster.RegionId,
                //These are not populated in the current system.
                // EventEmployeeId = driverStatus.EmployeeId,
                // EventEmployeeName = Common.GetEmployeeName(employeeMaster),
                EventTripNumber = driverSegmentDoneProcess.TripNumber,
                EventProgram = EventProgramConstants.Services,
                //These are not populated in the current system.
                //EventScreen = null,
                //EventAction = null,
                EventComment = comment,
            };

            ChangeSetResult<int> eventChangeSetResult;
            eventChangeSetResult = Common.UpdateEventLog(dataService, settings, eventLog);
            log.Debug("SRTEST:Saving EventLog Record - Segment Modify");
            log.DebugFormat("SRTEST:Saving EventLog Record for Trip:{0}-{1} - Segment Modify.",
                            currentTripSegment.TripNumber, currentTripSegment.TripSegNumber);
            //Check for EventLog failure.
            if (Common.LogChangeSetFailure(eventChangeSetResult, eventLog, log))
            {
                var s = string.Format("Segment Modify:Could not update EventLog for Trip:{0}-{1}.",
                        currentTripSegment.TripNumber, currentTripSegment.TripSegNumber);
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                return false;
            }

            ////////////////////////////////////////////////
            //Adjust Origins to include the modified trip segment.
            //Get all of the trips that have been assigned to the driver.
            //First, pending/missed trips dispatched to or acknowledged by the driver.
            //Followed by pending/missed trips assigned to the driver.
            //Followed by future trips assigned or dispatched to the driver.
            var allTripList = Common.GetAllTripsForDriver(dataService, settings, userCulture, userRoleIds,
                              employeeMaster.EmployeeId, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            if (allTripList != null && allTripList.Count() > 0)
            {
                //Get all of the segments for all of the driver's trips keeping them in the same order as allTripList.
                var allTripSegmentList = Common.GetAllTripSegmentsForTripList(dataService, settings, userCulture,
                                            userRoleIds, allTripList, out fault);
                //Since we modified the current segment of the current trip, we need to remove and insert the modified trip segment 
                int indexCurrentTripSegment = allTripSegmentList.IndexOf(currentTripSegment);
                //Should always be in the list, but just in case...
                if (-1 != indexCurrentTripSegment)
                {
                    allTripSegmentList.RemoveAt(indexCurrentTripSegment);
                    allTripSegmentList.Insert(indexCurrentTripSegment, currentTripSegment);
                }
                //Now call the method of set the origins as needed. Only ones where the origins are changed
                //will be updated.
                changeSetResult = Common.SetTripSegmentOrigins(dataService, settings, allTripSegmentList);
            }

            return true;
        }//end of public bool SegmentModify

        /// <summary>
        /// Marks an entire trip done.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="changeSetResult"></param>
        /// <param name="msgKey"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="userCulture"></param>
        /// <param name="driverSegmentDoneProcess"></param>
        /// <param name="employeeMaster"></param>
        /// <param name="powerMaster"></param>
        /// <param name="destCustomerMaster"></param>
        /// <param name="currentTrip"></param>
        /// <param name="tripSegList"></param>
        /// <param name="tripContainerList"></param>
        /// <param name="tripReferenceNumberList"></param>
        /// <param name="tripMileageList"></param>
        /// <param name="tripDelayList"></param>
        /// <returns></returns>
        private bool MarkTripDone(IDataService dataService, ProcessChangeSetSettings settings,
           ChangeSetResult<string> changeSetResult, String msgKey, long[] userRoleIds, string userCulture,
           DriverSegmentDoneProcess driverSegmentDoneProcess, EmployeeMaster employeeMaster, PowerMaster powerMaster,
           CustomerMaster destCustomerMaster,Trip currentTrip, List<TripSegment> tripSegList, 
           List<TripSegmentContainer> tripContainerList,List<TripReferenceNumber> tripReferenceNumberList, 
           List<TripSegmentMileage> tripMileageList, List<DriverDelay> tripDelayList, string DEFUseErrorQ)
        {
            DataServiceFault fault = null;
            //int containerHistoryInsertCount = 0;
            int tripHistoryInsertCount = 0;

            ////////////////////////////////////////////////
            //Update TripTable

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
                currentTrip.TripSpecInstructions = review.FirstOrDefault().TripSegComments;

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
                    currentTrip.TripSpecInstructions = exception.FirstOrDefault().TripSegComments;

                }
            }
            //If error queue is being used then...
            //If any segment status is in error queue, set trip status to Q=Error Queue
            if (DEFUseErrorQ == Constants.Yes)
            {
                var errorQ = from item in tripSegList
                             where item.TripSegStatus == TripSegStatusConstants.ErrorQueue
                             select item;
                if (errorQ != null && errorQ.Count() > 0)
                {
                    currentTrip.TripStatus = TripStatusConstants.ErrorQueue;
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
            //Trip Standard Drive Minutes is the sum of the Standard Drive Minutes of all of the segments. 
            currentTrip.TripStandardDriveMinutes = (from item in tripSegList
                                                  select item.TripSegStandardDriveMinutes).Sum();

            //Trip Standard Stop Minutes is the sum of the Standard Stop Minutes of all of the segments. 
            currentTrip.TripStandardStopMinutes = (from item in tripSegList
                                                 select item.TripSegStandardStopMinutes).Sum();

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
            currentTrip.TripChangedUserName = Common.GetEmployeeName(employeeMaster);

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

            ////////////////////////////////////////////////
            // Check if the TripSendFlag in the Trip table should be set to send completed trip information.
            // ScrapRunner sends trips to host only when the trip send flag is set to D=Done.

            ////////////////////////////////////////////////
            //Initially set the send flag not to send completed trip information to the host.
            currentTrip.TripSendFlag = TripSendFlagValue.NotSentToHost;

            ////////////////////////////////////////////////
            // Preferences:  Lookup the yard preference "DEFTHTrip".  
            // If preference DefTHTrip set to Y then set the send flag according to the trip status
            // Otherwise set the flag to indicate that completed trip information will not be sent to the host.
            string prefDefthTrip = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                   currentTrip.TripTerminalId, PrefYardConstants.DEFTHTrip, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            ////////////////////////////////////////////////
            // Preferences:  Lookup the yard preference "DEFSendExceptionTripsToHost".  
            // If preference DefTHTrip set to Y then set the send flag according to the trip status
            // Otherwise set the flag to indicate that completed trip information will not be sent to the host.
            string prefDEFSendExceptionTripsToHost = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                                     currentTrip.TripTerminalId, PrefYardConstants.DEFSendExceptionTripsToHost, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the TripTypeCompTripMsg flag in the TripTypeMasterDesc table 
            var tripTypeMasterDesc = Common.GetTripTypeMasterDesc(dataService, settings, userCulture, userRoleIds,
                                     currentTrip.TripType, out fault);
            if (null != fault)
            {
                return false;
            }
            string tripTypeCompTripMsg = tripTypeMasterDesc?.TripTypeCompTripMsg;

            //First, this flag must be set to Y or nothing will be sent.
            if (prefDefthTrip == Constants.Yes)
            {
                //Next this flag must be set to Y or nothing will be sent
                if (tripTypeCompTripMsg == Constants.Yes)
                {
                    //Set the trip send flag to the appropriate value based on Trip Status.
                    if (currentTrip.TripStatus == TripStatusConstants.Done)
                    {
                        currentTrip.TripSendFlag = TripSendFlagValue.TripDone;
                    }
                    else if (currentTrip.TripStatus == TripStatusConstants.Exception)
                    {
                        currentTrip.TripSendFlag = TripSendFlagValue.TripException;
                        //Check the DEFSendExceptionTripsToHost to see if exception trips should be sent
                        if (prefDEFSendExceptionTripsToHost == Constants.Yes)
                        {
                            //This overrides the Exception setting
                            currentTrip.TripSendFlag = TripSendFlagValue.TripDone;
                        }
                    }
                    else if (currentTrip.TripStatus == TripStatusConstants.Review)
                    {
                        currentTrip.TripSendFlag = TripSendFlagValue.TripInReview;
                    }
                }//end of if (tripTypeCompTripMsg == Constants.Yes)
            }//end of if (prefDEFTHTrip == Constants.Yes)


            ////////////////////////////////////////////////
            //Check if the TripSendScaleNotificationFlag in the Trip table should be set to send a scale notice.
            //If the trip is supposed to generate a scale notice (TripCommodityScaleMsg = Y)
            //and one has not been generated, send it now by setting the TripSendScaleNotificationFlag to 1
            //This would occur for a trip that did not have a return to yard segment because the driver
            //is going to a different customer site to pick up a second container there.

            ////////////////////////////////////////////////
            //Initially set the send flag not to send completed trip information to the host.
            currentTrip.TripSendScaleNotificationFlag = TripSendScaleFlagValue.NoScale;

            ////////////////////////////////////////////////
            // Preferences:  Lookup the yard preference "DEFTHScale".  
            // If preference DEFTHScale set to Y then set the send scale flag.
            string preDefthScale = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                   currentTrip.TripTerminalId, PrefYardConstants.DEFTHScale, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            //First, this flag must be set to Y or nothing will be sent.
            if (preDefthScale == Constants.Yes)
            {
                //Also this flag must be set on the trip record
                if (currentTrip.TripCommodityScaleMsg == Constants.Yes)
                {
                    //If Scale notice has not been sent on the enroute back to the yard, send the scale notice.
                    if (currentTrip.TripSendScaleNotificationFlag == TripSendScaleFlagValue.NoScale)
                    {
                        //Set the scale notice flag to send
                        currentTrip.TripSendScaleNotificationFlag = TripSendScaleFlagValue.ScaleReady;
                        //However...
                        //If trip is an exception, but has a loaded container, send the scale notice.
                        //Otherwise do not send.
                        if (currentTrip.TripStatus == TripStatusConstants.Exception)
                        {
                            string loaded = (from item in tripContainerList
                                         where item.TripSegContainerLoaded == Constants.Yes
                                         select item.TripSegContainerLoaded).FirstOrDefault();

                            if (loaded == null)
                            {
                                currentTrip.TripSendScaleNotificationFlag = TripSendScaleFlagValue.NoScale;
                            }

                        }//end of if (currentTrip.TripStatus == TripStatusConstants.Exception)
                    }//end of if (currentTrip.TripSendScaleNotificationFlag == TripSendScaleFlagValue.NoScale)
                }//end of if (currentTrip.TripCommodityScaleMsg == Constants.Yes)
            }//end of if (preDEFTHScale == Constants.Yes)

            ////////////////////////////////////////////////
            //Update ContainerMaster for each container in the TripSegmentContainer records,
            //remove the current trip information.      

            // From the list of containers used on the trip, pull out a list of distinct container numbers. 
            var distinctTripContainerList = (from item in tripContainerList
                                where item.TripSegContainerNumber != null
                                select item.TripSegContainerNumber).Distinct().ToList();
            if (distinctTripContainerList != null && distinctTripContainerList.Count > 0)
            {
                // Get the Container records from the ContainerMaster
                var containersOnTrip = Common.GetContainersForTrip(dataService, settings, userCulture, userRoleIds,
                                       distinctTripContainerList, out fault);
                if (null != fault)
                {
                    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                    return false;
                }
                if (null != containersOnTrip)
                {
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
                            var s = string.Format("Trip Done:Could not update Container:{0}.",
                                containerMaster.ContainerNumber);
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                            return false;
                        }
                        ////////////////////////////////////////////////
                        //Do not add record to Container History. This adds an extra entry that is not done in the current ScrapRunner.               
                        //if (!Common.InsertContainerHistory(dataService, settings, containerMaster, destCustomerMaster, null,
                        //    ++containerHistoryInsertCount, userRoleIds, userCulture, log, out fault))
                        //{
                        //    changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        //    log.ErrorFormat("Trip Done:InsertContainerHistory failed: {0} during segment done request: {1}",
                        //                     fault.Message, driverSegmentDoneProcess);
                        //    return false;
                        //}
                    }//foreach (var containerMaster in containersOnTrip)
                }// if (null != containersOnTrip)
            }//if (distinctTripContainerList != null)
            ////////////////////////////////////////////////
            //Add record to the DriverEfficiency table.  
            if (!Common.InsertDriverEfficiency(dataService, settings, userRoleIds, userCulture, log,
                     currentTrip, tripSegList, tripDelayList, out fault))
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                log.ErrorFormat("Trip Done:InsertDriverHistory failed: {0} during segment done request: {1}", 
                                 fault.Message, driverSegmentDoneProcess);
                return false;
            }

            ////////////////////////////////////////////////
            //Add records to Trip History tables. 
            string histAction = HistoryActionConstants.DriverDoneTrip;
            if (!Common.InsertTripHistory(dataService, settings, userRoleIds, userCulture, log,
                histAction, currentTrip, tripSegList, tripContainerList, tripReferenceNumberList, tripMileageList,
                ++tripHistoryInsertCount, out fault))
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                log.ErrorFormat("Trip Done:InsertTripHistory failed: {0} during segment done request: {1}",
                                 fault.Message, driverSegmentDoneProcess);
                return false;
            }

            ////////////////////////////////////////////////
            //Resequence trips for the driver of the completed trip.
            //Get all of the trips that have been assigned to the driver.
            //First, pending/missed trips dispatched to or acknowledged by the driver.
            //Followed by pending/missed trips assigned to the driver.
            //Followed by future trips assigned or dispatched to the driver.
            var allTripList = Common.GetAllTripsForDriver(dataService, settings, userCulture, userRoleIds,
                              employeeMaster.EmployeeId, out fault);
            if (null != fault)
            {
                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                return false;
            }
            if (allTripList != null && allTripList.Count() > 0)
            {
                //Remove the current trip in the list since it is now complete.
                allTripList.Remove(currentTrip);

                //Call the resequence method.
                changeSetResult = Common.ResequenceTripsForDriver(dataService, settings, allTripList);
            }
            return true;

        } //end of  MarkTripDone
    }
}

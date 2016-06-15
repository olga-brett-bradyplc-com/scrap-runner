using System;
using System.Linq;
using System.Collections.Generic;
using Brady.ScrapRunner.DataService.RecordTypes;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Core.Concrete.ChangeSets;
using BWF.DataServices.Core.Interfaces;
using BWF.DataServices.Core.Models;
using BWF.DataServices.Domain.Models;
using BWF.DataServices.PortableClients;
using BWF.DataServices.Support.NHibernate.Abstract;
using log4net;
using Brady.ScrapRunner.Domain.Enums;
using BWF.DataServices.Metadata.Fluent.Enums;

namespace Brady.ScrapRunner.DataService.Util
{


    /// <summary>
    /// A collection of common utilty classes.  Typically to support ChangeableRecordType classes and the like.
    /// </summary>
    public class Common
    {

        /// <summary>
        /// Return driver's cumulative time in minutes.
        /// </summary>
        /// <param name="tripSegList"></param>
        /// <param name="currentTripSegment"></param>
        /// <returns>null if tripSegList is null </returns>
        public static int GetDriverCumulativeTime(List<TripSegment> tripSegList,TripSegment currentTripSegment)
        {
            int driverTime = 0;
            if (null != tripSegList)
            {
                driverTime = (int)(from t in tripSegList
                             select (t.TripSegStandardDriveMinutes + t.TripSegStandardStopMinutes)).ToList().Sum();

                if (null != currentTripSegment)
                {
                    driverTime -= (int)currentTripSegment.TripSegStandardDriveMinutes;
                    driverTime -= (int)currentTripSegment.TripSegStandardStopMinutes;
                }
            }
            return driverTime;
        }
        /// <summary>
        /// Return "LastName, FirstName" or as much as possible from the provided EmployeeMaster object.
        /// </summary>
        /// <param name="employeeMaster"></param>
        /// <returns>null if employeeMaster is null or both name components are null</returns>
        public static string GetEmployeeName(EmployeeMaster employeeMaster)
        {
            string driverName = null;
            if (null != employeeMaster)
            {
                if (null != employeeMaster.LastName)
                {
                    driverName = employeeMaster.LastName;
                }
                if (null != employeeMaster.FirstName)
                {
                    driverName += ", " + employeeMaster.FirstName;
                }
            }
            return driverName;
        }
        /// <summary>
        /// Determines if container is still on the truck
        /// Based on the trip segment type and the SetInYardFlag
        /// </summary>
        /// <param name="tripSegType"></param>
        /// <param name="setInYardFlag"></param>
        /// <param name="actionType"></param>
        /// <returns>true if on truck, otherwise false</returns>
        public static bool IsContainerOnPowerId(string tripSegType, string setInYardFlag, string actionType)
        {
            bool onPowerId = true;
            if (tripSegType == BasicTripTypeConstants.ReturnYard)
            {
                if (setInYardFlag == Constants.Yes)
                {
                    onPowerId = false;
                }
            }
            else
            {
                if (tripSegType == BasicTripTypeConstants.PickupEmpty ||
                    tripSegType == BasicTripTypeConstants.PickupFull)
                {
                    if (actionType == ContainerActionTypeConstants.Exception)
                    {
                        //Container was not picked up.
                        onPowerId = false;
                    }
                }
                if (tripSegType == BasicTripTypeConstants.DropEmpty ||
                    tripSegType == BasicTripTypeConstants.DropFull)
                {
                    if (actionType != ContainerActionTypeConstants.Exception)
                    {
                        //Container was dropped.
                        onPowerId = false;
                    }
                }
            }
            return onPowerId;
        }
        /// <summary>
        /// Determines if the segment is loaded.
        /// If any container on the truck is loaded, segment is loaded
        /// Otherwise if the segment is a drop full, scale, or unload, the segment is loaded
        /// </summary>
        /// <param name="tripSegment"></param>
        /// <param name="containersOnPowerId"></param>List<ContainerMaster>
        /// <returns>Y if loaded, N if not</returns>
        public static string GetSegmentLoadedFlag(TripSegment tripSegment, List<ContainerMaster> containersOnPowerId)
        {
            string tripSegLoadedFlag = Constants.No;
            //If any container on the truck is loaded, these miles are loaded miles
            var loaded = from item in containersOnPowerId
                          where item.ContainerContents == ContainerContentsConstants.Loaded
                          select item;
            if (null != loaded && loaded.Count()>0)
            {
                tripSegLoadedFlag = Constants.Yes;
            }
            else
            {
                //Also consider the miles to be loaded miles if the driver is dropping a full container,
                //unloading a container, or driving to an independent scale.
                var types = new List<string> {BasicTripTypeConstants.DropFull,
                                                  BasicTripTypeConstants.Scale,
                                                  BasicTripTypeConstants.Unload };
                if (types.Contains(tripSegment.TripSegType))
                    tripSegLoadedFlag = Constants.Yes;
            }
            return tripSegLoadedFlag;
        }
        /// <summary>
        /// Log an entry into the specified logger if a fault is detected.
        /// </summary>
        /// <param name="query">The originating query.</param>
        /// <param name="fault">The possibly resultant fault.</param>
        /// <param name="log">Referece to the caller's logger</param>
        /// <returns>true if a fault is detected</returns>
        public static bool LogFault(Query query, DataServiceFault fault, ILog log)
        {
            var faultDetected = false;
            if (null != fault)
            {
                faultDetected = true;
                log.ErrorFormat("Fault occured: {0} during login query: {1}", fault.Message, query.CurrentQuery);
            }
            return faultDetected;
        }

        /// <summary>
        /// Log an entry into the specified logger for every detected failure within a changeSetResult.
        /// </summary>
        /// <param name="changeSetResult">Resulting change set from an update</param>
        /// <param name="requestObject">Data that caused the error.  It should have a meaningful ToString() defined.</param>
        /// <param name="log">Referece to the caller's logger</param>
        /// <returns>true if a failure is detected</returns>
        public static bool LogChangeSetFailure(ChangeSetResult<string> changeSetResult, object requestObject, ILog log)
        {
            var errorsDetected = false;
            if (changeSetResult.FailedCreates.Any())
            {
                errorsDetected = true;
                foreach (long key in changeSetResult.FailedCreates.Keys)
                {
                    var failedChange = changeSetResult.GetFailedCreateForRef(key);
                    log.ErrorFormat("ChangeSet create error occured: {0},  Request object: {1}", failedChange, requestObject);
                }
            }
            if (changeSetResult.FailedUpdates.Any())
            {
                errorsDetected = true;
                foreach (string key in changeSetResult.FailedUpdates.Keys)
                {
                    var failedChange = changeSetResult.GetFailedUpdateForId(key);
                    log.ErrorFormat("ChangeSet update error occured: {0}, Request object: {1}", failedChange, requestObject);
                }
            }
            if (changeSetResult.FailedDeletions.Any())
            {
                errorsDetected = true;
                foreach (string key in changeSetResult.FailedDeletions.Keys)
                {
                    var failedChange = changeSetResult.GetFailedDeleteForId(key);
                    log.ErrorFormat("ChangeSet delete error occured: {0}, Request object: {1}", failedChange, requestObject);
                }
            }
            return errorsDetected;
        }
        /// <summary>
        /// Log an entry into the specified logger for every detected failure within a changeSetResult.
        /// </summary>
        /// <param name="changeSetResult">Resulting change set from an update</param>
        /// <param name="requestObject">Data that caused the error.  It should have a meaningful ToString() defined.</param>
        /// <param name="log">Referece to the caller's logger</param>
        /// <returns>true if a failure is detected</returns>
        public static bool LogChangeSetFailure(ChangeSetResult<int> changeSetResult, object requestObject, ILog log)
        {
            var errorsDetected = false;
            if (changeSetResult.FailedCreates.Any())
            {
                errorsDetected = true;
                foreach (long key in changeSetResult.FailedCreates.Keys)
                {
                    var failedChange = changeSetResult.GetFailedCreateForRef(key);
                    log.ErrorFormat("ChangeSet create error occured: {0},  Request object: {1}", failedChange, requestObject);
                }
            }
            if (changeSetResult.FailedUpdates.Any())
            {
                errorsDetected = true;
                foreach (int key in changeSetResult.FailedUpdates.Keys)
                {
                    var failedChange = changeSetResult.GetFailedUpdateForId(key);
                    log.ErrorFormat("ChangeSet update error occured: {0}, Request object: {1}", failedChange, requestObject);
                }
            }
            if (changeSetResult.FailedDeletions.Any())
            {
                errorsDetected = true;
                foreach (int key in changeSetResult.FailedDeletions.Keys)
                {
                    var failedChange = changeSetResult.GetFailedDeleteForId(key);
                    log.ErrorFormat("ChangeSet delete error occured: {0}, Request object: {1}", failedChange, requestObject);
                }
            }
            return errorsDetected;
        }
        ////////////////////////////////////////////////////////////////
        ///TABLE INSERTS
        /// <summary>
        /// Insert a ContainerHistory record.
        ///  Note:  caller must handle faults.  E.G. if( handleFault(changeSetResult, msgKey, fault, driverLoginProcess)) { break; }
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="containerMaster"></param>
        /// <param name="destCustomerMaster"></param>
        /// <param name="prevLastActionDate"></param>
        /// <param name="callCountThisTxn">Start with 1 and increment if multiple inserts are desired.</param>
        /// <param name="userRoleIdsEnumerable"></param>
        /// <param name="userCulture"></param>
        /// <param name="fault"></param>
        /// <returns>true if success</returns>
        public static bool InsertContainerHistory(IDataService dataService, ProcessChangeSetSettings settings,
            ContainerMaster containerMaster,  CustomerMaster destCustomerMaster, DateTime? prevLastActionDateTime, int callCountThisTxn,
            IEnumerable<long> userRoleIdsEnumerable, string userCulture, ILog log, out DataServiceFault fault)
        {
            List<long> userRoleIds = userRoleIdsEnumerable.ToList();

            // Note this is a commited snapshot read, a not dirty value, thus we have to keep do our own bookkeeping
            // to support multiple inserts in one txn: callCountThisTxn

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the last container history record for this container id to get the last sequence number used
            var containerHistoryMax = Common.GetContainerHistoryLast(dataService, settings, userCulture, userRoleIds,
                                  containerMaster.ContainerNumber, out fault);
            if (null != fault)
            {
                return false;
            }
            int containerSeqNo = callCountThisTxn;
            if (containerHistoryMax != null)
            {
                containerSeqNo = containerHistoryMax.ContainerSeqNumber + callCountThisTxn;
            }
            //For testing
            log.Debug("SRTEST:Add ContainerHistory");
            log.DebugFormat("SRTEST:ContainerNumber:{0} TripNumber:{1} Seg:{2} Status:{3} DateTime:{4} Seq#:{5}",
                             containerMaster.ContainerNumber,
                             containerMaster.ContainerCurrentTripNumber,
                             containerMaster.ContainerCurrentTripSegNumber,
                             containerMaster.ContainerStatus,
                             containerMaster.ContainerLastActionDateTime,
                             containerSeqNo);
            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the TerminalName in the TerminalMaster 
            var containerTerminalMaster = Common.GetTerminal(dataService, settings, userCulture, userRoleIds,
                                          containerMaster.ContainerTerminalId, out fault);
            if (null != fault)
            {
                return false;
            }
            string containerTerminalName = containerTerminalMaster?.TerminalName;
            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the CurrentTerminalName in the TerminalMaster 
            var containerCurrentTerminalMaster = Common.GetTerminal(dataService, settings, userCulture, userRoleIds,
                                          containerMaster.ContainerCurrentTerminalId, out fault);
            if (null != fault)
            {
                return false;
            }
            string containerCurrentTerminalName = containerCurrentTerminalMaster?.TerminalName;

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the RegionName in the RegionMaster 
            var containerRegionMaster = Common.GetRegion(dataService, settings, userCulture, userRoleIds,
                                          containerMaster.ContainerRegionId, out fault);
            if (null != fault)
            {
                return false;
            }
            string containerRegionName = containerRegionMaster?.RegionName;

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the ContainerStatus Description in the CodeTable CONTAINERSTATUS 
            var codeTableContainerStatus = Common.GetCodeTableEntry(dataService, settings, userCulture, userRoleIds,
                                    CodeTableNameConstants.ContainerStatus, containerMaster.ContainerStatus, out fault);
            if (null != fault)
            {
                return false;
            }
            string containerStatusDesc = codeTableContainerStatus?.CodeDisp1;

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the ContainerContents Description in the CodeTable CONTENTSTATUS 
            var codeTableContainerContents = Common.GetCodeTableEntry(dataService, settings, userCulture, userRoleIds,
                                    CodeTableNameConstants.ContentStatus, containerMaster.ContainerContents, out fault);
            if (null != fault)
            {
                return false;
            }
            string containerContentsDesc = codeTableContainerContents?.CodeDisp1;

            //////////////////////////////////////////////////////////////////////////////////////
            //If there is a customer type, look up the description in the CodeTable CUSTOMERTYPE
            //Generally, for logins, this information should be null
            var codeTableCustomerType = new CodeTable();
            if (null != containerMaster.ContainerCustType)
            {
                codeTableCustomerType = Common.GetCodeTableEntry(dataService, settings, userCulture, userRoleIds,
                                      CodeTableNameConstants.CustomerType, containerMaster.ContainerCustType, out fault);
                if (null != fault)
                {
                    return false;
                }
            }
            string containerCustTypeDesc = codeTableCustomerType?.CodeDisp1;

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the Basic Trip Type Description in the TripTypeBasic table 
            var tripTypeBasic = Common.GetTripTypeBasic(dataService, settings, userCulture, userRoleIds,
                                containerMaster.ContainerCurrentTripSegType, out fault);
            if (null != fault)
            {
                return false;
            }
            string tripTypeBasicDesc = tripTypeBasic?.TripTypeDesc;

            //Only calculate days at site when a container is picked up at a customer site
            int? daysAtSite = null;
            if (containerMaster.ContainerCurrentTripSegType == BasicTripTypeConstants.PickupFull ||
                containerMaster.ContainerCurrentTripSegType == BasicTripTypeConstants.PickupEmpty)
            {
                //Container has to be on the truck (not an exception)
                if (containerMaster.ContainerPowerId != null)
                {
                    if (prevLastActionDateTime != null && containerMaster.ContainerLastActionDateTime != null)
                    {
                        daysAtSite = (int)(containerMaster.ContainerLastActionDateTime.Value.Subtract
                                          (prevLastActionDateTime.Value).TotalDays);
                    }
                }
            }

            var containerHistory = new ContainerHistory()
            {
                ContainerNumber = containerMaster.ContainerNumber,
                ContainerSeqNumber = containerSeqNo,
                ContainerType = containerMaster.ContainerType,
                ContainerSize = containerMaster.ContainerSize,
                ContainerUnits = containerMaster.ContainerUnits,
                ContainerLength = containerMaster.ContainerLength,
                ContainerCustType = containerMaster.ContainerCustType,
                ContainerCustTypeDesc = containerCustTypeDesc,
                ContainerTerminalId = containerMaster.ContainerTerminalId,
                ContainerTerminalName = containerTerminalName,
                ContainerRegionId = containerMaster.ContainerRegionId,
                ContainerRegionName = containerRegionName,
                ContainerLocation = containerMaster.ContainerLocation,
                ContainerLastActionDateTime = containerMaster.ContainerLastActionDateTime,
                ContainerDaysAtSite = daysAtSite,
                ContainerPendingMoveDateTime = containerMaster.ContainerPendingMoveDateTime,
                ContainerTripNumber = containerMaster.ContainerCurrentTripNumber,
                ContainerTripSegNumber = containerMaster.ContainerCurrentTripSegNumber,
                ContainerTripSegType = containerMaster.ContainerCurrentTripSegType,
                ContainerTripSegTypeDesc = tripTypeBasicDesc,
                ContainerStatus = containerMaster.ContainerStatus,
                ContainerStatusDesc = containerStatusDesc,
                ContainerContents = containerMaster.ContainerContents,
                ContainerContentsDesc = containerContentsDesc,
                ContainerCommodityCode = containerMaster.ContainerCommodityCode,
                ContainerCommodityDesc = containerMaster.ContainerCommodityDesc,
                ContainerComments = containerMaster.ContainerComments,
                ContainerPowerId = containerMaster.ContainerPowerId,
                ContainerShortTerm = containerMaster.ContainerShortTerm,
                ContainerCustHostCode = containerMaster.ContainerCustHostCode,
                ContainerCustName = destCustomerMaster?.CustName,
                ContainerCustAddress1 = destCustomerMaster?.CustAddress1,
                ContainerCustAddress2 = destCustomerMaster?.CustAddress2,
                ContainerCustCity = destCustomerMaster?.CustCity,
                ContainerCustState = destCustomerMaster?.CustState,
                ContainerCustZip = destCustomerMaster?.CustZip,
                ContainerCustCountry = destCustomerMaster?.CustCountry,
                ContainerCustCounty = destCustomerMaster?.CustCounty,
                ContainerCustTownship = destCustomerMaster?.CustTownship,
                ContainerCustPhone1 = destCustomerMaster?.CustPhone1,
                ContainerLevel = containerMaster.ContainerLevel,
                ContainerLatitude = containerMaster.ContainerLatitude,
                ContainerLongitude = containerMaster.ContainerLongitude,
                ContainerNotes = containerMaster.ContainerNotes,
                ContainerCurrentTerminalId = containerMaster.ContainerCurrentTerminalId,
                ContainerCurrentTerminalName = containerCurrentTerminalName,
                ContainerWidth = containerMaster.ContainerWidth,
                ContainerHeight = containerMaster.ContainerHeight

            };

            // Insert ContainerHistory 
            var recordType = (ContainerHistoryRecordType)dataService.RecordTypes.Single(x => x.TypeName == "ContainerHistory");
            var changeSet = (ChangeSet<string, ContainerHistory>)recordType.GetNewChangeSet();
            long recordRef = 1;
            changeSet.AddCreate(recordRef, containerHistory, userRoleIds, userRoleIds);
            log.Debug("SRTEST:Saving ContainerHistory");
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            if (Common.LogChangeSetFailure(changeSetResult, containerHistory, log))
            {
                return false;
            }
            return true;
        }

        ///TABLE INSERTS
        /// <summary>
        /// Insert a ContainerHistory record
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userRoleIdsEnumerable"></param>
        /// <param name="userCulture"></param>
        /// <param name="log"></param>
        /// <param name="containerHistory"></param>
        /// <returns></returns>
        public static bool InsertContainerHistory(IDataService dataService, ProcessChangeSetSettings settings,
        IEnumerable<long> userRoleIdsEnumerable, string userCulture, ILog log, ContainerHistory containerHistory)
        {
            List<long> userRoleIds = userRoleIdsEnumerable.ToList();

            // Insert ContainerHistory 
            var recordType = (ContainerHistoryRecordType)dataService.RecordTypes.Single(x => x.TypeName == "ContainerHistory");
            var changeSet = (ChangeSet<string, ContainerHistory>)recordType.GetNewChangeSet();
            long recordRef = 1;
            changeSet.AddCreate(recordRef, containerHistory, userRoleIds, userRoleIds);
            log.Debug("SRTEST:Saving ContainerHistory");
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            if (Common.LogChangeSetFailure(changeSetResult, containerHistory, log))
            {
                return false;
            }
            return true;
        }
        ///TABLE INSERTS
        /// <summary>
        /// Insert a ContainerMaster record
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userRoleIdsEnumerable"></param>
        /// <param name="userCulture"></param>
        /// <param name="log"></param>
        /// <param name="containerMaster"></param>
        /// <param name="fault"></param>
        /// <returns></returns>
        public static bool InsertContainerMaster(IDataService dataService, ProcessChangeSetSettings settings,
         IEnumerable<long> userRoleIdsEnumerable, string userCulture, ILog log,ContainerMaster containerMaster)
        {
            List<long> userRoleIds = userRoleIdsEnumerable.ToList();

            // Insert containerMaster 
            var recordType = (ContainerMasterRecordType)dataService.RecordTypes.Single(x => x.TypeName == "ContainerMaster");
            var changeSet = (ChangeSet<string, ContainerMaster>)recordType.GetNewChangeSet();
            long recordRef = 1;
            changeSet.AddCreate(recordRef, containerMaster, userRoleIds, userRoleIds);
            log.Debug("SRTEST:Saving ContainerMaster");
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            if (Common.LogChangeSetFailure(changeSetResult, containerMaster, log))
            {
                return false;
            }
            return true;

        }
        ///TABLE INSERTS
        /// <summary>
        /// Insert a DriverDelay record.
        ///  Note:  caller must handle faults.  
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="driverDelay"></param>
        /// <param name="callCountThisTxn">Start with 1 and increment if multiple inserts are desired.</param>
        /// <param name="userRoleIdsEnumerable"></param>
        /// <param name="userCulture"></param>
        /// <param name="fault"></param>
        /// <returns>true if success</returns>
        public static bool InsertDriverDelay(IDataService dataService, ProcessChangeSetSettings settings,
             IEnumerable<long> userRoleIdsEnumerable, string userCulture, ILog log,
             DriverDelay driverDelay, int callCountThisTxn, out DataServiceFault fault)
        {
            List<long> userRoleIds = userRoleIdsEnumerable.ToList();

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the last delay record for this driverid and trip number to get the last sequence number used
            var driverDelayMax = Common.GetDriverDelayLast(dataService, settings, userCulture, userRoleIds,
                                 driverDelay.DriverId, driverDelay.TripNumber, out fault);
            if (null != fault)
            {
                return false;
            }
            driverDelay.DelaySeqNumber = callCountThisTxn;
            if (driverDelayMax != null)
            {
                driverDelay.DelaySeqNumber = driverDelayMax.DelaySeqNumber + callCountThisTxn;
            }

            //For testing
            log.Debug("SRTEST:Add DriverDelay");
            log.DebugFormat("SRTEST:DriverId:{0} TripNumber:{1}-{2} StartDate:{3} DelayCode:{4} Seq#:{5}",
                             driverDelay.DriverId,
                             driverDelay.TripNumber,
                             driverDelay.TripSegNumber,
                             driverDelay.DelayStartDateTime,
                             driverDelay.DelayCode,
                             driverDelay.DelaySeqNumber);


            // Insert DriverDelay 
            var recordType = (DriverDelayRecordType)dataService.RecordTypes.Single(x => x.TypeName == "DriverDelay");
            var changeSet = (ChangeSet<string, DriverDelay>)recordType.GetNewChangeSet();
            long recordRef = 1;
            changeSet.AddCreate(recordRef, driverDelay, userRoleIds, userRoleIds);
            log.Debug("SRTEST:Saving DriverDelay");
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            if (Common.LogChangeSetFailure(changeSetResult, driverDelay, log))
            {
                return false;
            }
            return true;
        }
        ///TABLE INSERTS
        /// <summary>
        /// Insert a Driver Efficiency Record
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userRoleIdsEnumerable"></param>
        /// <param name="userCulture"></param>
        /// <param name="log"></param>
        /// <param name="trip"></param>
        /// <param name="tripSegmentList"></param>
        /// <param name="tripDelayList"></param>
        /// <param name="fault"></param>
        /// <returns></returns>
        public static bool InsertDriverEfficiency(IDataService dataService, ProcessChangeSetSettings settings,
             IEnumerable<long> userRoleIdsEnumerable, string userCulture, ILog log,
              Trip trip, List<TripSegment> tripSegmentList, List<DriverDelay> tripDelayList, out DataServiceFault fault)
        {
            List<long> userRoleIds = userRoleIdsEnumerable.ToList();
            //////////////////////////////////////////////////////////////////////////////////////
            //Try to find a driver efficiency record for this trip. There should not be one
            var driverEfficiency = Common.GetDriverEfficiencyForDriverTrip(dataService, settings, userCulture, userRoleIds,
                                   trip.TripDriverId,trip.TripNumber, out fault);
            if (null != fault)
            {
                return false;
            }
            if (null == driverEfficiency)
            {
                driverEfficiency = new DriverEfficiency();
            }

            driverEfficiency.TripDriverId = trip.TripDriverId;
            driverEfficiency.TripNumber = trip.TripNumber;
            driverEfficiency.TripDriverName = trip.TripDriverName;
            driverEfficiency.TripType = trip.TripType;
            driverEfficiency.TripTypeDesc = trip.TripTypeDesc;
            driverEfficiency.TripCustHostCode = trip.TripCustHostCode;
            driverEfficiency.TripCustName = trip.TripCustName;
            driverEfficiency.TripCustAddress1 = trip.TripCustAddress1;
            driverEfficiency.TripCustAddress2 = trip.TripCustAddress2;
            driverEfficiency.TripCustCity = trip.TripCustCity;
            driverEfficiency.TripCustState = trip.TripCustState;
            driverEfficiency.TripCustZip = trip.TripCustZip;
            driverEfficiency.TripCustCountry = trip.TripCustCountry;
            driverEfficiency.TripTerminalId = trip.TripTerminalId;
            driverEfficiency.TripTerminalName = trip.TripTerminalName;
            driverEfficiency.TripRegionId = trip.TripRegionId;
            driverEfficiency.TripRegionName = trip.TripRegionName;
            driverEfficiency.TripReferenceNumber = trip.TripReferenceNumber;
            driverEfficiency.TripCompletedDateTime = trip.TripCompletedDateTime;

            driverEfficiency.TripPowerId = trip.TripPowerId;

            driverEfficiency.TripOdometerStart = (from tripSegment in tripSegmentList
                    select tripSegment.TripSegOdometerStart).FirstOrDefault();

            driverEfficiency.TripOdometerEnd = (from tripSegment in tripSegmentList
                    select tripSegment.TripSegOdometerEnd).LastOrDefault();

            driverEfficiency.TripActualDriveMinutes = trip.TripActualDriveMinutes;
            driverEfficiency.TripStandardDriveMinutes = trip.TripStandardDriveMinutes;

            //Initialize
            driverEfficiency.TripActualYardMinutes = 0;
            driverEfficiency.TripStandardYardMinutes = 0;
            driverEfficiency.TripActualStopMinutes = 0;
            driverEfficiency.TripStandardStopMinutes = 0;
            driverEfficiency.TripActualTotalMinutes = 0;
            driverEfficiency.TripStandardTotalMinutes = 0;
            driverEfficiency.TripYardDelayMinutes = 0;
            driverEfficiency.TripCustDelayMinutes = 0;
            driverEfficiency.TripLunchBreakDelayMinutes = 0;
            driverEfficiency.TripDelayMinutes = 0;

            foreach (var tripSegment in tripSegmentList)
            {
                if (tripSegment.TripSegDestCustType == CustomerTypeConstants.Yard)
                {
                    driverEfficiency.TripActualYardMinutes += tripSegment.TripSegActualStopMinutes;
                    driverEfficiency.TripStandardYardMinutes += tripSegment.TripSegStandardStopMinutes;
                }
                else
                {
                    driverEfficiency.TripActualStopMinutes += tripSegment.TripSegActualStopMinutes;
                    driverEfficiency.TripStandardStopMinutes += tripSegment.TripSegStandardStopMinutes;
                }
            }

            driverEfficiency.TripActualTotalMinutes = driverEfficiency.TripActualDriveMinutes
                                                      + driverEfficiency.TripActualYardMinutes
                                                      + driverEfficiency.TripActualStopMinutes;
            driverEfficiency.TripStandardTotalMinutes = driverEfficiency.TripStandardDriveMinutes
                                                        + driverEfficiency.TripStandardYardMinutes 
                                                        + driverEfficiency.TripStandardStopMinutes;

            foreach (var tripDelay in tripDelayList)
            {
                if (tripDelay.DelayStartDateTime != null && tripDelay.DelayEndDateTime != null)
                {

                    var codeTableDelayCode = Common.GetCodeTableEntry(dataService, settings, userCulture, userRoleIds,
                        CodeTableNameConstants.DelayCodes, tripDelay.DelayCode, out fault);

                    if (codeTableDelayCode.CodeDisp2 == DelayTypeConstants.Yard)
                    {
                        driverEfficiency.TripYardDelayMinutes += (int)(tripDelay.DelayEndDateTime.Value.Subtract
                                (tripDelay.DelayStartDateTime.Value).TotalMinutes);
                    }
                    else if (codeTableDelayCode.CodeDisp2 == DelayTypeConstants.Customer)
                    {
                        driverEfficiency.TripCustDelayMinutes += (int)(tripDelay.DelayEndDateTime.Value.Subtract
                                (tripDelay.DelayStartDateTime.Value).TotalMinutes);
                    }
                    else if (codeTableDelayCode.CodeDisp2 == DelayTypeConstants.LunchBreak)
                    {
                        driverEfficiency.TripLunchBreakDelayMinutes += (int)(tripDelay.DelayEndDateTime.Value.Subtract
                               (tripDelay.DelayStartDateTime.Value).TotalMinutes);
                    }

                    driverEfficiency.TripDelayMinutes += (int)(tripDelay.DelayEndDateTime.Value.Subtract
                                (tripDelay.DelayStartDateTime.Value).TotalMinutes);
                }//end of if (tripDelay.DelayStartDateTime != null && tripDelay.DelayEndDateTime != null)

            } //end of             foreach (var tripDelay in tripDelayList)

            //////////////////////////////////////////////////////////////////////////////////////
            // Insert DriverEfficiency
            var recordType = (DriverEfficiencyRecordType)dataService.RecordTypes.Single(x => x.TypeName == "DriverEfficiency");
            var changeSet = (ChangeSet<string, DriverEfficiency>)recordType.GetNewChangeSet();
            long recordRef = 1;
            changeSet.AddCreate(recordRef, driverEfficiency, userRoleIds, userRoleIds);
            log.Debug("SRTEST:Saving DriverEfficiency Record");
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            if (Common.LogChangeSetFailure(changeSetResult, driverEfficiency, log))
            {
                return false;
            }

            return true;
        }

        ///TABLE INSERTS
        /// <summary>
        /// Insert a DriverHistory record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="driverStatus"></param>
        /// <param name="employeeMaster"></param>
        /// <param name="currentTripSegment"></param>
        /// <param name="callCountThisTxn">Start with 1 and incremenet if multiple inserts are desired.</param>
        /// <param name="userRoleIdsEnumerable"></param>
        /// <param name="userCulture"></param>
        /// <param name="fault"></param>
        /// <returns>true if success</returns>
        public static bool InsertDriverHistory(IDataService dataService, ProcessChangeSetSettings settings,
            DriverStatus driverStatus, EmployeeMaster employeeMaster, TripSegment currentTripSegment, int callCountThisTxn,
            IEnumerable<long> userRoleIdsEnumerable, string userCulture, ILog log, out DataServiceFault fault)
        {
            List<long> userRoleIds = userRoleIdsEnumerable.ToList();

            // Note this is a commited snapshot read, a not dirty value, thus we have to keep do our own bookkeeping
            // to support multiple inserts in one txn: callCountThisTxn

            //DriverHistory needs a trip number, but if driver is not on a trip we need to create a substitute trip number.
            //Construct a number to use that consists of the driver status + driverid
            //For example the tripnumber for a driver delay would be X802. For a back on duty B802.
            //For a Login L802. For a Logout O802.
            string tripNumber = driverStatus.TripNumber;
            if (null == tripNumber)
            {
                tripNumber = driverStatus.Status + driverStatus.EmployeeId;
            }

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the last driver history record for this trip and driver to get the last sequence number used
            //Pass the constructed trip number to get the last sequence number.
            var driverHistoryMax = Common.GetDriverHistoryLast(dataService, settings, userCulture, userRoleIds,
                                    driverStatus.EmployeeId, tripNumber, out fault);
            if (null != fault)
            {
                return false;
            }
            int driverSeqNo = callCountThisTxn;
            if (driverHistoryMax != null)
            {
                driverSeqNo = driverHistoryMax.DriverSeqNumber + callCountThisTxn;
            }
            //For testing
            log.Debug("SRTEST:Add DriverHistory");
            log.DebugFormat("SRTEST:Driver:{0} TripNumber:{1} Seg:{2} Status:{3} DateTime:{4} Seq#:{5}",
                             driverStatus.EmployeeId,
                             driverStatus.TripNumber,
                             driverStatus.TripSegNumber,
                             driverStatus.Status,
                             driverStatus.ActionDateTime,
                             driverSeqNo);

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the Basic Trip Type Description in the TripTypeBasic table 
            var tripTypeBasic = Common.GetTripTypeBasic(dataService, settings, userCulture, userRoleIds,
                                driverStatus.TripSegType, out fault);
            if (null != fault)
            {
                return false;
            }
            string tripTypeBasicDesc = tripTypeBasic?.TripTypeDesc;

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the TripAssignStatus Description in the CodeTable TRIPASSIGNSTATUS 
            var codeTableTripAssignStatus = Common.GetCodeTableEntry(dataService, settings, userCulture, userRoleIds,
                                    CodeTableNameConstants.TripAssignStatus, driverStatus.TripAssignStatus, out fault);
            if (null != fault)
            {
                return false;
            }
            string tripAssignStatusDesc = codeTableTripAssignStatus?.CodeDisp1;

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the TripStatus Description in the CodeTable TRIPSTATUS 
            var codeTableTripStatus = Common.GetCodeTableEntry(dataService, settings, userCulture, userRoleIds,
                                    CodeTableNameConstants.TripStatus, driverStatus.TripStatus, out fault);
            if (null != fault)
            {
                return false;
            }
            string tripStatusDesc = codeTableTripStatus?.CodeDisp1;

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the TripSegStatus Description in the CodeTable TRIPSEGSTATUS 
            var codeTableTripSegStatus = Common.GetCodeTableEntry(dataService, settings, userCulture, userRoleIds,
                                    CodeTableNameConstants.TripSegStatus, driverStatus.TripSegStatus, out fault);
            if (null != fault)
            {
                return false;
            }
            string tripSegStatusDesc = codeTableTripSegStatus?.CodeDisp1;

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the DriverStatus Description in the CodeTable DRIVERSTATUS 
            var codeTableDriverStatus = Common.GetCodeTableEntry(dataService, settings, userCulture, userRoleIds,
                                    CodeTableNameConstants.DriverStatus, driverStatus.Status, out fault);
            if (null != fault)
            {
                return false;
            }
            string driverStatusDesc = codeTableDriverStatus?.CodeDisp1;

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the TerminalName in the TerminalMaster 
            var driverTerminalMaster = Common.GetTerminal(dataService, settings, userCulture, userRoleIds,
                                          driverStatus.TerminalId, out fault);
            if (null != fault)
            {
                return false;
            }
            string driverTerminalName = driverTerminalMaster?.TerminalName;

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the RegionName in the RegionMaster 
            var driverRegionMaster = Common.GetRegion(dataService, settings, userCulture, userRoleIds,
                                          driverStatus.RegionId, out fault);
            if (null != fault)
            {
                return false;
            }
            string driverRegionName = driverRegionMaster?.RegionName;

            //////////////////////////////////////////////////////////////////////////////////////
            //do not lookup the trip segment information. It has been set up for an update and hangs
            //eventually throwing an exception:
            //Error executing query: NHibernate.Exceptions.GenericADOException: Failed to execute multi criteria
            //Timeout expired.  The timeout period elapsed prior to completion of the operation or the server is not responding. 
            //If there is a trip number and segment number, look up the TripSegment record to get the
            //destination customer information. (Instead of the cutomer master).
            //Unless the driver is logging in in the middle of the trip, we want this to be null
            //var driverTripSegment = new TripSegment();
            //if (null != driverStatus.TripNumber && null != driverStatus.TripSegNumber)
            //{
            //    driverTripSegment = Common.GetTripSegment(dataService, settings, userCulture, userRoleIds,
            //                                  driverStatus.TripNumber, driverStatus.TripSegNumber, out fault);
            //    if (null != fault)
            //    {
            //        return false;
            //    }
            //}

             //////////////////////////////////////////////////////////////////////////////////////
            var driverHistory = new DriverHistory()
            {
                EmployeeId = driverStatus.EmployeeId,
                TripNumber = tripNumber,
                DriverSeqNumber = driverSeqNo,
                TripSegNumber = driverStatus.TripSegNumber,
                TripSegType = driverStatus.TripSegType,
                TripSegTypeDesc  = tripTypeBasicDesc,
                TripAssignStatus  = driverStatus.TripAssignStatus,
                TripAssignStatusDesc  = tripAssignStatusDesc,
                TripStatus  = driverStatus.TripStatus,
                TripStatusDesc  = tripStatusDesc,
                TripSegStatus  = driverStatus.TripSegStatus,
                TripSegStatusDesc = tripSegStatusDesc,
                DriverStatus = driverStatus.Status,
                DriverStatusDesc = driverStatusDesc,
                DriverName = Common.GetEmployeeName(employeeMaster),
                TerminalId = driverStatus.TerminalId,
                TerminalName = driverTerminalName,
                RegionId = driverStatus.RegionId,
                RegionName = driverRegionName,
                PowerId = driverStatus.PowerId,
                // DriverArea =,
                MDTId = driverStatus.MDTId,
                LoginDateTime = driverStatus.LoginDateTime,
                ActionDateTime = driverStatus.ActionDateTime,
                DriverCumMinutes = driverStatus.DriverCumMinutes,
                Odometer = driverStatus.Odometer,
                DestCustType = currentTripSegment.TripSegDestCustType,
                DestCustTypeDesc = currentTripSegment.TripSegDestCustTypeDesc,
                DestCustHostCode = currentTripSegment.TripSegDestCustHostCode,
                DestCustName = currentTripSegment.TripSegDestCustName,
                DestCustAddress1 = currentTripSegment.TripSegDestCustAddress1,
                DestCustAddress2 = currentTripSegment.TripSegDestCustAddress2,
                DestCustCity = currentTripSegment.TripSegDestCustCity,
                DestCustState = currentTripSegment.TripSegDestCustState,
                DestCustZip = currentTripSegment.TripSegDestCustZip,
                DestCustCountry = currentTripSegment.TripSegDestCustCountry,
                GPSAutoGeneratedFlag = driverStatus.GPSAutoGeneratedFlag,
                GPSXmitFlag = driverStatus.GPSXmitFlag,
                MdtVersion = driverStatus.MdtVersion
            };

            //////////////////////////////////////////////////////////////////////////////////////
            // Insert DriverHistory 
            var recordType = (DriverHistoryRecordType)dataService.RecordTypes.Single(x => x.TypeName == "DriverHistory");
            var changeSet = (ChangeSet<string, DriverHistory>)recordType.GetNewChangeSet();
            long recordRef = 1;
            changeSet.AddCreate(recordRef, driverHistory, userRoleIds, userRoleIds);
            log.Debug("SRTEST:Saving DriverHistory Record");
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            if (Common.LogChangeSetFailure(changeSetResult, driverHistory, log))
            {
                return false;
            }
            return true;
        }
        ///TABLE INSERTS
        /// <summary>
        /// Insert a Message Record
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userRoleIdsEnumerable"></param>
        /// <param name="userCulture"></param>
        /// <param name="log"></param>
        /// <param name="message"></param>
        /// <param name="fault"></param>
        /// <returns></returns>
        public static bool InsertMessage(IDataService dataService, ProcessChangeSetSettings settings,
             IEnumerable<long> userRoleIdsEnumerable, string userCulture, ILog log,
             Messages message, out DataServiceFault fault)
        {
            List<long> userRoleIds = userRoleIdsEnumerable.ToList();

            //For testing
            log.Debug("SRTEST:Add Message");
            log.DebugFormat("SRTEST:Sender:{0}-{1} Receiver:{2}-{3} Msg:{4}",
                             message.SenderId,
                             message.SenderName,
                             message.ReceiverId,
                             message.ReceiverName,
                             message.MsgText);


            // Insert Message 
            var recordType = (MessagesRecordType)dataService.RecordTypes.Single(x => x.TypeName == "Messages");
            var changeSet = (ChangeSet<int, Messages>)recordType.GetNewChangeSet();
            long recordRef = 1;
            changeSet.AddCreate(recordRef, message, userRoleIds, userRoleIds);
            log.Debug("SRTEST:Saving Messages");
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            //ToDo: add fault checking
            fault = null;
            if (Common.LogChangeSetFailure(changeSetResult, message, log))
            {
                return false;
            }
            return true;
        }

        ///TABLE INSERTS
        /// <summary>
        /// Insert a PowerFuel record.
        ///  Note:  caller must handle faults.  
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="powerFuel"></param>
        /// <param name="callCountThisTxn">Start with 1 and increment if multiple inserts are desired.</param>
        /// <param name="userRoleIdsEnumerable"></param>
        /// <param name="userCulture"></param>
        /// <param name="fault"></param>
        /// <returns>true if success</returns>
        public static bool InsertPowerFuel(IDataService dataService, ProcessChangeSetSettings settings,
             IEnumerable<long> userRoleIdsEnumerable, string userCulture, ILog log,
             PowerFuel powerFuel, int callCountThisTxn, out DataServiceFault fault)
        {
            List<long> userRoleIds = userRoleIdsEnumerable.ToList();

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the last power record for this powerid and trip number to get the last sequence number used
            var powerFuelMax = Common.GetPowerFuelLast(dataService, settings, userCulture, userRoleIds,
                               powerFuel.PowerId, powerFuel.TripNumber, out fault);
            if (null != fault)
            {
                return false;
            }
            powerFuel.PowerFuelSeqNumber = callCountThisTxn;
            if (powerFuelMax != null)
            {
                powerFuel.PowerFuelSeqNumber = powerFuelMax.PowerFuelSeqNumber + callCountThisTxn;
            }
              
            //For testing
            log.Debug("SRTEST:Add PowerFuel");
            log.DebugFormat("SRTEST:PowerId:{0} TripNumber:{1}-{2} Date:{3} State:{4} Amt:{5} Seq#:{5}",
                             powerFuel.PowerId,
                             powerFuel.TripNumber,
                             powerFuel.TripSegNumber,
                             powerFuel.PowerDateOfFuel,
                             powerFuel.PowerState,
                             powerFuel.PowerGallons,
                             powerFuel.PowerFuelSeqNumber);


            // Insert PowerFuel 
            var recordType = (PowerFuelRecordType)dataService.RecordTypes.Single(x => x.TypeName == "PowerFuel");
            var changeSet = (ChangeSet<string, PowerFuel>)recordType.GetNewChangeSet();
            long recordRef = 1;
            changeSet.AddCreate(recordRef, powerFuel, userRoleIds, userRoleIds);
            log.Debug("SRTEST:Saving PowerFuel");
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            if (Common.LogChangeSetFailure(changeSetResult, powerFuel, log))
            {
                return false;
            }
            return true;
        }
        ///TABLE INSERTS
        /// <summary>
        /// Insert a PowerHistory record.
        ///  Note:  caller must handle faults.  E.G. if( handleFault(changeSetResult, msgKey, fault, driverLoginProcess)) { break; }
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="powerMaster"></param>
        /// <param name="employeeMaster"></param>
        /// <param name="callCountThisTxn">Start with 1 and incremenet if multiple inserts are desired.</param>
        /// <param name="userRoleIdsEnumerable"></param>
        /// <param name="userCulture"></param>
        /// <param name="fault"></param>
        /// <returns>true if success</returns>
        public static bool InsertPowerHistory(IDataService dataService, ProcessChangeSetSettings settings,
            PowerMaster powerMaster, EmployeeMaster employeeMaster, CustomerMaster destCustomerMaster, int callCountThisTxn,
            IEnumerable<long> userRoleIdsEnumerable, string userCulture, ILog log, out DataServiceFault fault)
        {
            List<long> userRoleIds = userRoleIdsEnumerable.ToList();

            // Note this is a commited snapshot read, a not dirty value, thus we have to keep do our own bookkeeping
            // to support multiple inserts in one txn: callCountThisTxn

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the last power history record for this power id to get the last sequence number used
            var powerHistoryMax = Common.GetPowerHistoryLast(dataService, settings, userCulture, userRoleIds,
                                  powerMaster.PowerId, out fault);
            if (null != fault)
            {
                return false;
            }
            int powerSeqNo = callCountThisTxn;
            if (powerHistoryMax != null)
            {
                powerSeqNo = powerHistoryMax.PowerSeqNumber + callCountThisTxn;
            }
            //For testing
            log.Debug("SRTEST:Add PowerHistory");
            log.DebugFormat("SRTEST:Driver:{0} TripNumber:{1} Seg:{2} Status:{3} DateTime:{4} Seq#:{5}",
                             powerMaster.PowerDriverId,
                             powerMaster.PowerCurrentTripNumber,
                             powerMaster.PowerCurrentTripSegNumber,
                             powerMaster.PowerStatus,
                             powerMaster.PowerLastActionDateTime,
                             powerSeqNo);
            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the TerminalName in the TerminalMaster 
            var powerTerminalMaster = Common.GetTerminal(dataService, settings, userCulture, userRoleIds,
                                          powerMaster.PowerTerminalId, out fault);
            if (null != fault)
            {
                return false;
            }
            string powerTerminalName = powerTerminalMaster?.TerminalName;

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the RegionName in the RegionMaster 
            var powerRegionMaster = Common.GetRegion(dataService, settings, userCulture, userRoleIds,
                                          powerMaster.PowerRegionId, out fault);
            if (null != fault)
            {
                return false;
            }
            string powerRegionName = powerRegionMaster?.RegionName;

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the PowerStatus Description in the CodeTable POWERUNITSTATUS 
            var codeTablePowerStatus = Common.GetCodeTableEntry(dataService, settings, userCulture, userRoleIds,
                                    CodeTableNameConstants.PowerUnitStatus, powerMaster.PowerStatus, out fault);
            if (null != fault)
            {
                return false;
            }
            string powerStatusDesc = codeTablePowerStatus?.CodeDisp1;

            //////////////////////////////////////////////////////////////////////////////////////
            //If there is a customer type, look up the description in the CodeTable CUSTOMERTYPE
            //Generally, for logins, this information should be null
            var codeTableCustomerType = new CodeTable();
            if (null != powerMaster.PowerCustType)
            {
                codeTableCustomerType = Common.GetCodeTableEntry(dataService, settings, userCulture, userRoleIds,
                                      CodeTableNameConstants.CustomerType, powerMaster.PowerCustType, out fault);
                if (null != fault)
                {
                    return false;
                }
            }
            string powerCustTypeDesc = codeTableCustomerType?.CodeDisp1;

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the Basic Trip Type Description in the TripTypeBasic table 
            var tripTypeBasic = Common.GetTripTypeBasic(dataService, settings, userCulture, userRoleIds,
                                powerMaster.PowerCurrentTripSegType, out fault);
            if (null != fault)
            {
                return false;
            }
            string tripTypeBasicDesc = tripTypeBasic?.TripTypeDesc;

            var powerHistory = new PowerHistory();
            powerHistory.PowerId = powerMaster.PowerId;
            powerHistory.PowerSeqNumber = powerSeqNo;
            powerHistory.PowerType = powerMaster.PowerType;
            powerHistory.PowerDesc = powerMaster.PowerDesc;
            powerHistory.PowerSize = powerMaster.PowerSize;
            powerHistory.PowerLength = powerMaster.PowerLength;
            powerHistory.PowerTareWeight = powerMaster.PowerTareWeight;
            powerHistory.PowerCustType = powerMaster.PowerCustType;
            powerHistory.PowerCustTypeDesc = powerCustTypeDesc;
            powerHistory.PowerTerminalId = powerMaster.PowerTerminalId;
            powerHistory.PowerTerminalName = powerTerminalName;
            powerHistory.PowerRegionId = powerMaster.PowerRegionId;
            powerHistory.PowerRegionName = powerRegionName;
            powerHistory.PowerLocation = powerMaster.PowerLocation;
            powerHistory.PowerStatus = powerMaster.PowerStatus;
            powerHistory.PowerDateOutOfService = powerMaster.PowerDateOutOfService;
            powerHistory.PowerDateInService = powerMaster.PowerDateInService;
            powerHistory.PowerDriverId = powerMaster.PowerDriverId;
            powerHistory.PowerDriverName = Common.GetEmployeeName(employeeMaster);
            powerHistory.PowerOdometer = powerMaster.PowerOdometer;
            powerHistory.PowerComments = powerMaster.PowerComments;
            powerHistory.MdtId = powerMaster.MdtId;
            powerHistory.PrimaryPowerType = null;
            powerHistory.PowerCustHostCode = powerMaster.PowerCustHostCode;
            powerHistory.PowerLastActionDateTime = powerMaster.PowerLastActionDateTime;
            powerHistory.PowerStatusDesc = powerStatusDesc;
            powerHistory.PowerCurrentTripNumber = powerMaster.PowerCurrentTripNumber;
            powerHistory.PowerCurrentTripSegNumber = powerMaster.PowerCurrentTripSegNumber;
            powerHistory.PowerCurrentTripSegType = powerMaster.PowerCurrentTripSegType;
            powerHistory.PowerCurrentTripSegTypeDesc = tripTypeBasicDesc;
            if (null != destCustomerMaster)
            {
                powerHistory.PowerCustName = destCustomerMaster.CustName;
                powerHistory.PowerCustAddress1 = destCustomerMaster.CustAddress1;
                powerHistory.PowerCustAddress2 = destCustomerMaster.CustAddress2;
                powerHistory.PowerCustCity = destCustomerMaster.CustCity;
                powerHistory.PowerCustState = destCustomerMaster.CustState;
                powerHistory.PowerCustZip = destCustomerMaster.CustZip;
                powerHistory.PowerCustCountry = destCustomerMaster.CustCountry;
                powerHistory.PowerCustCounty = destCustomerMaster.CustCounty;
                powerHistory.PowerCustTownship = destCustomerMaster.CustTownship;
                powerHistory.PowerCustPhone1 = destCustomerMaster.CustPhone1;
            }
            // Insert PowerHistory 
            var recordType = (PowerHistoryRecordType)dataService.RecordTypes.Single(x => x.TypeName == "PowerHistory");
            var changeSet = (ChangeSet<string, PowerHistory>)recordType.GetNewChangeSet();
            long recordRef = 1;
            changeSet.AddCreate(recordRef, powerHistory, userRoleIds, userRoleIds);
            log.Debug("SRTEST:Saving PowerHistory");
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            if (Common.LogChangeSetFailure(changeSetResult, powerHistory, log))
            {
                return false;
            }
            return true;
        }

        ///TABLE INSERTS
        /// <summary>
        /// Add history trip, trip segment, trip segment container, trip reference number, trip segment mileage records
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userRoleIdsEnumerable"></param>
        /// <param name="userCulture"></param>
        /// <param name="log"></param>
        /// <param name="histAction"></param>
        /// <param name="trip"></param>
        /// <param name="tripSegment"></param>
        /// <param name="tripSegmentContainer"></param>
        /// <param name="tripReferenceNumber"></param>
        /// <param name="tripSegmentMileage"></param>
        /// <param name="callCountThisTxn"></param>
        /// <param name="fault"></param>
        /// <returns></returns>
        public static bool InsertTripHistory(IDataService dataService, ProcessChangeSetSettings settings,
             IEnumerable<long> userRoleIdsEnumerable, string userCulture, ILog log,
             string histAction, Trip trip, List<TripSegment> tripSegmentList, List<TripSegmentContainer> tripSegmentContainerList, 
             List<TripReferenceNumber> tripReferenceNumberList, List<TripSegmentMileage> tripSegmentMileageList, 
             int callCountThisTxn, out DataServiceFault fault)
        {
            List<long> userRoleIds = userRoleIdsEnumerable.ToList();

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the last trip history trip record for this trip  to get the last sequence number used
            var histTripMax = Common.GetHistTripLast(dataService, settings, userCulture, userRoleIds,
                               trip.TripNumber,  out fault);
            if (null != fault)
            {
                return false;
            }
            int histSeqNo = callCountThisTxn;
            if (histTripMax != null)
            {
                histSeqNo = histTripMax.HistSeqNo + callCountThisTxn;
            }

            //Set the HistTrip fields
            var histTrip = new HistTrip();
            histTrip.HistSeqNo = histSeqNo;
            histTrip.HistAction = histAction;
            histTrip.TripNumber = trip.TripNumber;
            histTrip.TripStatus = trip.TripStatus;
            histTrip.TripStatusDesc = trip.TripStatusDesc;
            histTrip.TripAssignStatus = trip.TripAssignStatus;
            histTrip.TripAssignStatusDesc = trip.TripAssignStatusDesc;
            histTrip.TripType = trip.TripType;
            histTrip.TripTypeDesc = trip.TripTypeDesc;
            histTrip.TripSequenceNumber = trip.TripSequenceNumber;
            histTrip.TripSendFlag = trip.TripSendFlag;
            histTrip.TripDriverId = trip.TripDriverId;
            histTrip.TripDriverName = trip.TripDriverName;
            histTrip.TripCustHostCode = trip.TripCustHostCode;
            histTrip.TripCustCode4_4 = trip.TripCustCode4_4;
            histTrip.TripCustName = trip.TripCustName;
            histTrip.TripCustAddress1 = trip.TripCustAddress1;
            histTrip.TripCustAddress2 = trip.TripCustAddress2;
            histTrip.TripCustCity = trip.TripCustCity;
            histTrip.TripCustState = trip.TripCustState;
            histTrip.TripCustZip = trip.TripCustZip;
            histTrip.TripCustCountry = trip.TripCustCountry;
            histTrip.TripCustPhone1 = trip.TripCustPhone1;
            histTrip.TripTerminalId = trip.TripTerminalId;
            histTrip.TripTerminalName = trip.TripTerminalName;
            histTrip.TripRegionId = trip.TripRegionId;
            histTrip.TripRegionName = trip.TripRegionName;
            histTrip.TripContactName = trip.TripContactName;
            histTrip.TripSalesman = trip.TripSalesman;
            histTrip.TripCustOpenTime = trip.TripCustOpenTime;
            histTrip.TripCustCloseTime = trip.TripCustCloseTime;
            histTrip.TripReadyDateTime = trip.TripReadyDateTime;
            histTrip.TripEnteredUserId = trip.TripEnteredUserId;
            histTrip.TripEnteredUserName = trip.TripEnteredUserName;
            histTrip.TripEnteredDateTime = trip.TripEnteredDateTime;
            histTrip.TripStandardDriveMinutes = trip.TripStandardDriveMinutes;
            histTrip.TripStandardStopMinutes = trip.TripStandardStopMinutes;
            histTrip.TripActualDriveMinutes = trip.TripActualDriveMinutes;
            histTrip.TripActualStopMinutes = trip.TripActualStopMinutes;
            histTrip.TripContractNumber = trip.TripContractNumber;
            histTrip.TripShipmentNumber = trip.TripShipmentNumber;
            histTrip.TripCommodityPurchase = trip.TripCommodityPurchase;
            histTrip.TripCommoditySale = trip.TripCommoditySale;
            histTrip.TripSpecInstructions = trip.TripSpecInstructions;
            histTrip.TripExpediteFlag = trip.TripExpediteFlag;
            histTrip.TripPrimaryContainerNumber = trip.TripPrimaryContainerNumber;
            histTrip.TripPrimaryContainerType = trip.TripPrimaryContainerType;
            histTrip.TripPrimaryContainerSize = trip.TripPrimaryContainerSize;
            histTrip.TripPrimaryCommodityCode = trip.TripPrimaryCommodityCode;
            histTrip.TripPrimaryCommodityDesc = trip.TripPrimaryCommodityDesc;
            histTrip.TripPrimaryContainerLocation = trip.TripPrimaryContainerLocation;
            histTrip.TripPowerId = trip.TripPowerId;
            histTrip.TripCommodityScaleMsg = trip.TripCommodityScaleMsg;
            histTrip.TripSendReceiptFlag = trip.TripSendReceiptFlag;
            histTrip.TripDriverIdPrev = trip.TripDriverIdPrev;
            histTrip.TripCompletedDateTime = trip.TripCompletedDateTime;
            histTrip.TripSendScaleNotificationFlag = trip.TripSendScaleNotificationFlag;
            histTrip.TripExtendedFlag = trip.TripExtendedFlag;
            histTrip.TripExtendedReason = trip.TripExtendedReason;
            histTrip.TripInProgressFlag = trip.TripInProgressFlag;
            histTrip.TripNightRunFlag = trip.TripNightRunFlag;
            histTrip.TripDoneMethod = trip.TripDoneMethod;
            histTrip.TripCompletedUserId = trip.TripCompletedUserId;
            histTrip.TripCompletedUserName = trip.TripCompletedUserName;
            histTrip.TripReferenceNumber = trip.TripReferenceNumber;
            histTrip.TripChangedDateTime = trip.TripChangedDateTime;
            histTrip.TripChangedUserId = trip.TripChangedUserId;
            histTrip.TripChangedUserName = trip.TripChangedUserName;
            histTrip.TripDirectSegNumber = trip.TripDirectSegNumber;
            histTrip.TripDirectDriveMinutes = trip.TripDirectDriveMinutes;
            histTrip.TripDirectTotalMinutes = trip.TripDirectTotalMinutes;
            histTrip.TripStartedDateTime = trip.TripStartedDateTime;
            histTrip.TripErrorDesc = trip.TripErrorDesc;
            histTrip.TripDriverInstructions = trip.TripDriverInstructions;
            histTrip.TripDispatcherInstructions = trip.TripDispatcherInstructions;
            histTrip.TripScaleReferenceNumber = trip.TripScaleReferenceNumber;
            histTrip.TripMultContainerFlag = trip.TripMultContainerFlag;
            histTrip.TripSendReseqFlag = trip.TripSendReseqFlag;
            histTrip.TripPowerAssetNumber = trip.TripPowerAssetNumber;
            histTrip.TripHaulerHostCode = trip.TripHaulerHostCode;
            histTrip.TripHaulerName = trip.TripHaulerName;
            histTrip.TripHaulerAddress1 = trip.TripHaulerAddress1;
            histTrip.TripHaulerCity = trip.TripHaulerCity;
            histTrip.TripHaulerState = trip.TripHaulerState;
            histTrip.TripHaulerZip = trip.TripHaulerZip;
            histTrip.TripHaulerCountry = trip.TripHaulerCountry;
            histTrip.TripResolvedFlag = trip.TripResolvedFlag;
            histTrip.TripSendScaleDateTime = trip.TripSendScaleDateTime;
            histTrip.TripResendScaleNotificationFlag = trip.TripResendScaleNotificationFlag;
            histTrip.TripResendScaleDateTime = trip.TripResendScaleDateTime;
            histTrip.TripImageFlag = trip.TripImageFlag;
            histTrip.TripScaleTerminalId = trip.TripScaleTerminalId;
            histTrip.TripScaleTerminalName = trip.TripScaleTerminalName;
            histTrip.TripSendScaleTerminalId = trip.TripSendScaleTerminalId;
            histTrip.TripSendScaleTerminalName = trip.TripSendScaleTerminalName;
            histTrip.TripResendScaleTerminalId = trip.TripResendScaleTerminalId;
            histTrip.TripResendScaleTerminalName = trip.TripResendScaleTerminalName;

            // Insert HistTrip 
            var recordTypeHistTrip = (HistTripRecordType)dataService.RecordTypes.Single(x => x.TypeName == "HistTrip");
            var changeSetHistTrip = (ChangeSet<string, HistTrip>)recordTypeHistTrip.GetNewChangeSet();
            long recordRefHistTrip = 1;
            changeSetHistTrip.AddCreate(recordRefHistTrip, histTrip, userRoleIds, userRoleIds);
            log.DebugFormat("SRTEST:Saving HistTrip {0}", histTrip.TripNumber);
            var changeSetResultHistTrip = recordTypeHistTrip.ProcessChangeSet(dataService, changeSetHistTrip, settings);
            if (Common.LogChangeSetFailure(changeSetResultHistTrip, histTrip, log))
            {
                return false;
            }

            //Set the HistTripSegment fields
            foreach (var tripSegment in tripSegmentList)
            {
                var histTripSegment = new HistTripSegment();
                histTripSegment.HistSeqNo = histSeqNo;
                histTripSegment.TripNumber = tripSegment.TripNumber;
                histTripSegment.TripSegNumber = tripSegment.TripSegNumber;
                histTripSegment.TripSegStatus = tripSegment.TripSegStatus;
                histTripSegment.TripSegStatusDesc = tripSegment.TripSegStatusDesc;
                histTripSegment.TripSegType = tripSegment.TripSegType;
                histTripSegment.TripSegTypeDesc = tripSegment.TripSegTypeDesc;
                histTripSegment.TripSegPowerId = tripSegment.TripSegPowerId;
                histTripSegment.TripSegDriverId = tripSegment.TripSegDriverId;
                histTripSegment.TripSegDriverName = tripSegment.TripSegDriverName;
                histTripSegment.TripSegStartDateTime = tripSegment.TripSegStartDateTime;
                histTripSegment.TripSegEndDateTime = tripSegment.TripSegEndDateTime;
                histTripSegment.TripSegStandardDriveMinutes = tripSegment.TripSegStandardDriveMinutes;
                histTripSegment.TripSegStandardStopMinutes = tripSegment.TripSegStandardStopMinutes;
                histTripSegment.TripSegActualDriveMinutes = tripSegment.TripSegActualDriveMinutes;
                histTripSegment.TripSegActualStopMinutes = tripSegment.TripSegActualStopMinutes;
                histTripSegment.TripSegOdometerStart = tripSegment.TripSegOdometerStart;
                histTripSegment.TripSegOdometerEnd = tripSegment.TripSegOdometerEnd;
                histTripSegment.TripSegComments = tripSegment.TripSegComments;
                histTripSegment.TripSegOrigCustType = tripSegment.TripSegOrigCustType;
                histTripSegment.TripSegOrigCustTypeDesc = tripSegment.TripSegOrigCustTypeDesc;
                histTripSegment.TripSegOrigCustHostCode = tripSegment.TripSegOrigCustHostCode;
                histTripSegment.TripSegOrigCustCode4_4 = tripSegment.TripSegOrigCustCode4_4;
                histTripSegment.TripSegOrigCustName = tripSegment.TripSegOrigCustName;
                histTripSegment.TripSegOrigCustAddress1 = tripSegment.TripSegOrigCustAddress1;
                histTripSegment.TripSegOrigCustAddress2 = tripSegment.TripSegOrigCustAddress2;
                histTripSegment.TripSegOrigCustCity = tripSegment.TripSegOrigCustCity;
                histTripSegment.TripSegOrigCustState = tripSegment.TripSegOrigCustState;
                histTripSegment.TripSegOrigCustZip = tripSegment.TripSegOrigCustZip;
                histTripSegment.TripSegOrigCustCountry = tripSegment.TripSegOrigCustCountry;
                histTripSegment.TripSegOrigCustPhone1 = tripSegment.TripSegOrigCustPhone1;
                histTripSegment.TripSegOrigCustTimeFactor = tripSegment.TripSegOrigCustTimeFactor;
                histTripSegment.TripSegDestCustType = tripSegment.TripSegDestCustType;
                histTripSegment.TripSegDestCustTypeDesc = tripSegment.TripSegDestCustTypeDesc;
                histTripSegment.TripSegDestCustHostCode = tripSegment.TripSegDestCustHostCode;
                histTripSegment.TripSegDestCustName = tripSegment.TripSegDestCustName;
                histTripSegment.TripSegDestCustAddress1 = tripSegment.TripSegDestCustAddress1;
                histTripSegment.TripSegDestCustAddress2 = tripSegment.TripSegDestCustAddress2;
                histTripSegment.TripSegDestCustCity = tripSegment.TripSegDestCustCity;
                histTripSegment.TripSegDestCustState = tripSegment.TripSegDestCustState;
                histTripSegment.TripSegDestCustZip = tripSegment.TripSegDestCustZip;
                histTripSegment.TripSegDestCustCountry = tripSegment.TripSegDestCustCountry;
                histTripSegment.TripSegDestCustPhone1 = tripSegment.TripSegDestCustPhone1;
                histTripSegment.TripSegDestCustTimeFactor = tripSegment.TripSegDestCustTimeFactor;
                histTripSegment.TripSegPrimaryContainerNumber = tripSegment.TripSegPrimaryContainerNumber;
                histTripSegment.TripSegPrimaryContainerType = tripSegment.TripSegPrimaryContainerType;
                histTripSegment.TripSegPrimaryContainerSize = tripSegment.TripSegPrimaryContainerSize;
                histTripSegment.TripSegPrimaryContainerCommodityCode = tripSegment.TripSegPrimaryContainerCommodityCode;
                histTripSegment.TripSegPrimaryContainerCommodityDesc = tripSegment.TripSegPrimaryContainerCommodityDesc;
                histTripSegment.TripSegPrimaryContainerLocation = tripSegment.TripSegPrimaryContainerLocation;
                histTripSegment.TripSegActualDriveStartDateTime = tripSegment.TripSegActualDriveStartDateTime;
                histTripSegment.TripSegActualDriveEndDateTime = tripSegment.TripSegActualDriveEndDateTime;
                histTripSegment.TripSegActualStopStartDateTime = tripSegment.TripSegActualStopStartDateTime;
                histTripSegment.TripSegActualStopEndDateTime = tripSegment.TripSegActualStopEndDateTime;
                histTripSegment.TripSegStartLatitude = tripSegment.TripSegStartLatitude;
                histTripSegment.TripSegStartLongitude = tripSegment.TripSegStartLongitude;
                histTripSegment.TripSegEndLatitude = tripSegment.TripSegEndLatitude;
                histTripSegment.TripSegEndLongitude = tripSegment.TripSegEndLongitude;
                histTripSegment.TripSegStandardMiles = tripSegment.TripSegStandardMiles;
                histTripSegment.TripSegErrorDesc = tripSegment.TripSegErrorDesc;
                histTripSegment.TripSegContainerQty = tripSegment.TripSegContainerQty;
                histTripSegment.TripSegDriverGenerated = tripSegment.TripSegDriverGenerated;
                histTripSegment.TripSegDriverModified = tripSegment.TripSegDriverModified;
                histTripSegment.TripSegPowerAssetNumber = tripSegment.TripSegPowerAssetNumber;
                histTripSegment.TripSegExtendedFlag = tripSegment.TripSegExtendedFlag;
                histTripSegment.TripSegSendReceiptFlag = tripSegment.TripSegSendReceiptFlag;

                // Insert HistTripSegment
                var recordTypeHistTripSegment = (HistTripSegmentRecordType)dataService.RecordTypes.Single(x => x.TypeName == "HistTripSegment");
                var changeSetHistTripSegment = (ChangeSet<string, HistTripSegment>)recordTypeHistTripSegment.GetNewChangeSet();
                long recordRefHistTripSegment = 1;
                changeSetHistTripSegment.AddCreate(recordRefHistTripSegment, histTripSegment, userRoleIds, userRoleIds);
                log.DebugFormat("SRTEST:Saving HistTripSegment {0}-{1}", histTripSegment.TripNumber, histTripSegment.TripSegNumber);
                var changeSetResultHistTripSegment = recordTypeHistTripSegment.ProcessChangeSet(dataService, changeSetHistTripSegment, settings);
                if (Common.LogChangeSetFailure(changeSetResultHistTripSegment, histTripSegment, log))
                {
                    return false;
                }
            }

            foreach (var tripSegmentContainer in tripSegmentContainerList)
            {
                //Set the HistTripSegmentContainer fields
                var histTripSegmentContainer = new HistTripSegmentContainer();
                histTripSegmentContainer.HistSeqNo = histSeqNo;
                histTripSegmentContainer.TripNumber = tripSegmentContainer.TripNumber;
                histTripSegmentContainer.TripSegNumber = tripSegmentContainer.TripSegNumber;
                histTripSegmentContainer.TripSegContainerSeqNumber = tripSegmentContainer.TripSegContainerSeqNumber;
                histTripSegmentContainer.TripSegContainerNumber = tripSegmentContainer.TripSegContainerNumber;
                histTripSegmentContainer.TripSegContainerType = tripSegmentContainer.TripSegContainerType;
                histTripSegmentContainer.TripSegContainerSize = tripSegmentContainer.TripSegContainerSize;
                histTripSegmentContainer.TripSegContainerCommodityCode = tripSegmentContainer.TripSegContainerCommodityCode;
                histTripSegmentContainer.TripSegContainerCommodityDesc = tripSegmentContainer.TripSegContainerCommodityDesc;
                histTripSegmentContainer.TripSegContainerLocation = tripSegmentContainer.TripSegContainerLocation;
                histTripSegmentContainer.TripSegContainerShortTerm = tripSegmentContainer.TripSegContainerShortTerm;
                histTripSegmentContainer.TripSegContainerWeightGross = tripSegmentContainer.TripSegContainerWeightGross;
                histTripSegmentContainer.TripSegContainerWeightGross2nd = tripSegmentContainer.TripSegContainerWeightGross2nd;
                histTripSegmentContainer.TripSegContainerWeightTare = tripSegmentContainer.TripSegContainerWeightTare;
                histTripSegmentContainer.TripSegContainerReviewFlag = tripSegmentContainer.TripSegContainerReviewFlag;
                histTripSegmentContainer.TripSegContainerReviewReason = tripSegmentContainer.TripSegContainerReviewReason;
                histTripSegmentContainer.TripSegContainerActionDateTime = tripSegmentContainer.TripSegContainerActionDateTime;
                histTripSegmentContainer.TripSegContainerEntryMethod = tripSegmentContainer.TripSegContainerEntryMethod;
                histTripSegmentContainer.WeightGrossDateTime = tripSegmentContainer.WeightGrossDateTime;
                histTripSegmentContainer.WeightGross2ndDateTime = tripSegmentContainer.WeightGross2ndDateTime;
                histTripSegmentContainer.WeightTareDateTime = tripSegmentContainer.WeightTareDateTime;
                histTripSegmentContainer.TripSegContainerLevel = tripSegmentContainer.TripSegContainerLevel;
                histTripSegmentContainer.TripSegContainerLatitude = tripSegmentContainer.TripSegContainerLatitude;
                histTripSegmentContainer.TripSegContainerLongitude = tripSegmentContainer.TripSegContainerLongitude;
                histTripSegmentContainer.TripSegContainerLoaded = tripSegmentContainer.TripSegContainerLoaded;
                histTripSegmentContainer.TripSegContainerOnTruck = tripSegmentContainer.TripSegContainerOnTruck;
                histTripSegmentContainer.TripScaleReferenceNumber = tripSegmentContainer.TripScaleReferenceNumber;
                histTripSegmentContainer.TripSegContainerSubReason = tripSegmentContainer.TripSegContainerSubReason;
                histTripSegmentContainer.TripSegContainerComment = tripSegmentContainer.TripSegContainerComment;
                histTripSegmentContainer.TripSegContainerComplete = tripSegmentContainer.TripSegContainerComplete;

                // Insert HistTripSegmentContainer
                var recordTypeHistTripSegmentContainer = (HistTripSegmentContainerRecordType)dataService.RecordTypes.Single(x => x.TypeName == "HistTripSegmentContainer");
                var changeSetHistTripSegmentContainer = (ChangeSet<string, HistTripSegmentContainer>)recordTypeHistTripSegmentContainer.GetNewChangeSet();
                long recordRefHistTripSegmentContainer = 1;
                changeSetHistTripSegmentContainer.AddCreate(recordRefHistTripSegmentContainer, histTripSegmentContainer, userRoleIds, userRoleIds);
                log.DebugFormat("SRTEST:Saving HistTripSegmentContainer {0}-{1} {2}", 
                    histTripSegmentContainer.TripNumber, histTripSegmentContainer.TripSegNumber,histTripSegmentContainer.TripSegContainerSeqNumber);
                var changeSetResultHistTripSegmentContainer = recordTypeHistTripSegmentContainer.ProcessChangeSet(dataService, changeSetHistTripSegmentContainer, settings);
                if (Common.LogChangeSetFailure(changeSetResultHistTripSegmentContainer, histTripSegmentContainer, log))
                {
                    return false;
                }
            }

            foreach (var tripReferenceNumber in tripReferenceNumberList)
            {
                //Set the HistTripReferenceNumber fields
                var histTripReferenceNumber = new HistTripReferenceNumber();
                histTripReferenceNumber.HistSeqNo = histSeqNo;
                histTripReferenceNumber.TripNumber = tripReferenceNumber.TripNumber;
                histTripReferenceNumber.TripSeqNumber = tripReferenceNumber.TripSeqNumber;
                histTripReferenceNumber.TripRefNumberDesc = tripReferenceNumber.TripRefNumberDesc;
                histTripReferenceNumber.TripRefNumber = tripReferenceNumber.TripRefNumber;

                // Insert HistTripReferenceNumber
                var recordTypeHistTripReferenceNumber = (HistTripReferenceNumberRecordType)dataService.RecordTypes.Single(x => x.TypeName == "HistTripReferenceNumber");
                var changeSetHistTripReferenceNumber = (ChangeSet<string, HistTripReferenceNumber>)recordTypeHistTripReferenceNumber.GetNewChangeSet();
                long recordRefHistTripReferenceNumber = 1;
                changeSetHistTripReferenceNumber.AddCreate(recordRefHistTripReferenceNumber, histTripReferenceNumber, userRoleIds, userRoleIds);
                log.DebugFormat("SRTEST:Saving HistTripReferenceNumber {0} Seq:{1} Ref#{2}-{3}",
                    histTripReferenceNumber.TripNumber, histTripReferenceNumber.TripSeqNumber, histTripReferenceNumber.TripRefNumberDesc,histTripReferenceNumber.TripRefNumber);
                var changeSetResultHistTripReferenceNumber = recordTypeHistTripReferenceNumber.ProcessChangeSet(dataService, changeSetHistTripReferenceNumber, settings);
                if (Common.LogChangeSetFailure(changeSetResultHistTripReferenceNumber, histTripReferenceNumber, log))
                {
                    return false;
                }
            }

            foreach (var tripSegmentMileage in tripSegmentMileageList)
            {
                //Set the HistTripSegmentMileage fields
                var histTripSegmentMileage = new HistTripSegmentMileage();
                histTripSegmentMileage.HistSeqNo = histSeqNo;
                histTripSegmentMileage.TripNumber = tripSegmentMileage.TripNumber;
                histTripSegmentMileage.TripSegNumber = tripSegmentMileage.TripSegNumber;
                histTripSegmentMileage.TripSegMileageSeqNumber = tripSegmentMileage.TripSegMileageSeqNumber;
                histTripSegmentMileage.TripSegMileageState = tripSegmentMileage.TripSegMileageState;
                histTripSegmentMileage.TripSegMileageCountry = tripSegmentMileage.TripSegMileageCountry;
                histTripSegmentMileage.TripSegMileageOdometerStart = tripSegmentMileage.TripSegMileageOdometerStart;
                histTripSegmentMileage.TripSegMileageOdometerEnd = tripSegmentMileage.TripSegMileageOdometerEnd;
                histTripSegmentMileage.TripSegLoadedFlag = tripSegmentMileage.TripSegLoadedFlag;
                histTripSegmentMileage.TripSegMileagePowerId = tripSegmentMileage.TripSegMileagePowerId;
                histTripSegmentMileage.TripSegMileageDriverId = tripSegmentMileage.TripSegMileageDriverId;
                histTripSegmentMileage.TripSegMileageDriverName = tripSegmentMileage.TripSegMileageDriverName;

                // Insert HistTripSegmentMileage
                var recordTypeHistTripSegmentMileage = (HistTripSegmentMileageRecordType)dataService.RecordTypes.Single(x => x.TypeName == "HistTripSegmentMileage");
                var changeSetHistTripSegmentMileage = (ChangeSet<string, HistTripSegmentMileage>)recordTypeHistTripSegmentMileage.GetNewChangeSet();
                long recordRefHistTripSegmentMileage = 1;
                changeSetHistTripSegmentMileage.AddCreate(recordRefHistTripSegmentMileage, histTripSegmentMileage, userRoleIds, userRoleIds);
                log.DebugFormat("SRTEST:Saving HistTripSegmentMileage {0}-{1} {2}",
                    histTripSegmentMileage.TripNumber, histTripSegmentMileage.TripSegNumber, histTripSegmentMileage.TripSegMileageSeqNumber);
                var changeSetResultHistTripSegmentMileage = recordTypeHistTripSegmentMileage.ProcessChangeSet(dataService, changeSetHistTripSegmentMileage, settings);
                if (Common.LogChangeSetFailure(changeSetResultHistTripSegmentMileage, histTripSegmentMileage, log))
                {
                    return false;
                }
            }

            return true;
        }
        ///TABLE INSERTS
        /// <summary>
        /// Insert a TripSegmentMileage record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userRoleIdsEnumerable"></param>
        /// <param name="userCulture"></param>
        /// <param name="log"></param>
        /// <param name="tripSegmentMileage"></param>
        /// <param name="callCountThisTxn"></param>
        /// <param name="fault"></param>
        /// <returns></returns>
        public static bool InsertTripSegmentMileage(IDataService dataService, ProcessChangeSetSettings settings,
             IEnumerable<long> userRoleIdsEnumerable, string userCulture, ILog log,
             TripSegmentMileage tripSegmentMileage, int callCountThisTxn, out DataServiceFault fault)
        {
            List<long> userRoleIds = userRoleIdsEnumerable.ToList();

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the last trip segment mileage record for this trip and segment number to get the last sequence number used
            var tripSegmentMileageMax = Common.GetTripSegmentMileageLast(dataService, settings, userCulture, userRoleIds,
                                        tripSegmentMileage.TripNumber, tripSegmentMileage.TripSegNumber, out fault);
            if (null != fault)
            {
                return false;
            }
            tripSegmentMileage.TripSegMileageSeqNumber = callCountThisTxn;
            if (tripSegmentMileageMax != null)
            {
                tripSegmentMileage.TripSegMileageSeqNumber = tripSegmentMileageMax.TripSegMileageSeqNumber + callCountThisTxn;
            }

            //For testing
            log.Debug("SRTEST:Add TripSegmentMileage");
            log.DebugFormat("SRTEST:TripNumber:{0} Seg:{1} Start:{2} End:{3} State:{4} Seq#:{5}",
                             tripSegmentMileage.TripNumber,
                             tripSegmentMileage.TripSegNumber,
                             tripSegmentMileage.TripSegMileageOdometerStart,
                             tripSegmentMileage.TripSegMileageOdometerEnd,
                             tripSegmentMileage.TripSegMileageState,
                             tripSegmentMileage.TripSegMileageSeqNumber);


            // Insert TripSegmentMileage 
            var recordType = (TripSegmentMileageRecordType)dataService.RecordTypes.Single(x => x.TypeName == "TripSegmentMileage");
            var changeSet = (ChangeSet<string, TripSegmentMileage>)recordType.GetNewChangeSet();
            long recordRef = 1;
            changeSet.AddCreate(recordRef, tripSegmentMileage, userRoleIds, userRoleIds);
            log.Debug("SRTEST:Saving TripSegmentMileage");
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            if (Common.LogChangeSetFailure(changeSetResult, tripSegmentMileage, log))
            {
                return false;
            }
            return true;

        }

        ///TABLE INSERTS
        /// <summary>
        /// Insert a TripSegmentMileage record.
        ///  Note:  caller must handle faults.  E.G. if( handleFault(changeSetResult, msgKey, fault, driverLoginProcess)) { break; }
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="tripSegment"></param>
        /// <param name="containersOnPowerId"></param>List<ContainerMaster>
        /// <param name="withOdometerEnd"></param>
        /// <param name="callCountThisTxn">Start with 1 and increment if multiple inserts are desired.</param>
        /// <param name="userRoleIdsEnumerable"></param>
        /// <param name="userCulture"></param>
        /// <param name="fault"></param>
        /// <returns>true if success</returns>
        public static bool InsertTripSegmentMileage(IDataService dataService, ProcessChangeSetSettings settings,
             IEnumerable<long> userRoleIdsEnumerable, string userCulture, ILog log, 
             TripSegment tripSegment, List<ContainerMaster> containersOnPowerId,bool withOdometerEnd, int callCountThisTxn,
              out DataServiceFault fault)
        {
            List<long> userRoleIds = userRoleIdsEnumerable.ToList();

            //////////////////////////////////////////////////////////////////////////////////////
            //Lookup the last trip segment mileage record for this trip and segment number to get the last sequence number used
            var tripSegmentMileageMax = Common.GetTripSegmentMileageLast(dataService, settings, userCulture, userRoleIds,
                                  tripSegment.TripNumber, tripSegment.TripSegNumber, out fault);
            if (null != fault)
            {
                return false;
            }
            int tripSegmentMileageSeqNo = callCountThisTxn;
            if (tripSegmentMileageMax != null)
            {
                tripSegmentMileageSeqNo = tripSegmentMileageMax.TripSegMileageSeqNumber + callCountThisTxn;
            }
            var tripSegmentMileage = new TripSegmentMileage();
            tripSegmentMileage.TripNumber = tripSegment.TripNumber;
            tripSegmentMileage.TripSegNumber = tripSegment.TripSegNumber;
            tripSegmentMileage.TripSegMileageSeqNumber = tripSegmentMileageSeqNo;
            tripSegmentMileage.TripSegMileageState = tripSegment.TripSegDestCustState;
            tripSegmentMileage.TripSegMileageCountry = tripSegment.TripSegDestCustCountry;
            tripSegmentMileage.TripSegMileageOdometerStart = tripSegment.TripSegOdometerStart;
            if (withOdometerEnd)
            {
                tripSegmentMileage.TripSegMileageOdometerEnd = tripSegment.TripSegOdometerEnd;
            }
            tripSegmentMileage.TripSegLoadedFlag = Common.GetSegmentLoadedFlag(tripSegment, containersOnPowerId);
            tripSegmentMileage.TripSegMileagePowerId = tripSegment.TripSegPowerId;
            tripSegmentMileage.TripSegMileageDriverId = tripSegment.TripSegDriverId;
            tripSegmentMileage.TripSegMileageDriverName = tripSegment.TripSegDriverName;
            
            //For testing
            log.Debug("SRTEST:Add TripSegmentMileage");
            log.DebugFormat("SRTEST:TripNumber:{0} Seg:{1} Start:{2} End:{3} State:{4} Seq#:{5}",
                             tripSegmentMileage.TripNumber,
                             tripSegmentMileage.TripSegNumber,
                             tripSegmentMileage.TripSegMileageOdometerStart,
                             tripSegmentMileage.TripSegMileageOdometerEnd,
                             tripSegmentMileage.TripSegMileageState,
                             tripSegmentMileageSeqNo);


            // Insert TripSegmentMileage 
            var recordType = (TripSegmentMileageRecordType)dataService.RecordTypes.Single(x => x.TypeName == "TripSegmentMileage");
            var changeSet = (ChangeSet<string, TripSegmentMileage>)recordType.GetNewChangeSet();
            long recordRef = 1;
            changeSet.AddCreate(recordRef, tripSegmentMileage, userRoleIds, userRoleIds);
            log.Debug("SRTEST:Saving TripSegmentMileage");
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            if (Common.LogChangeSetFailure(changeSetResult, tripSegmentMileage, log))
            {
                return false;
            }
            return true;
        }
 
        ///TABLE INSERTS
        /// <summary>
        /// Add/Update an EventLog record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="eventLog"></param>
        /// <returns>The changeSetResult.  Caller must inspect for errors.</returns>
        public static ChangeSetResult<int> UpdateEventLog(IDataService dataService, ProcessChangeSetSettings settings,
                                              EventLog eventLog)
        {
            var recordType = (EventLogRecordType)dataService.RecordTypes.Single(x => x.TypeName == "EventLog");
            var changeSet = (ChangeSet<int, EventLog>)recordType.GetNewChangeSet();
            changeSet.AddUpdate(eventLog.Id, eventLog);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }

        //////////////////////////////////////////////////////////////////////////
        ///TABLE UPDATES
        /// <summary>
        /// Update a ContainerHistory record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="containerHistory"></param>
        /// <returns>The changeSetResult.  Caller must inspect for errors.</returns>
        public static ChangeSetResult<string> UpdateContainerHistory(IDataService dataService, ProcessChangeSetSettings settings,
                                              ContainerHistory containerHistory)
        {
            var recordType = (ContainerHistoryRecordType)dataService.RecordTypes.Single(x => x.TypeName == "ContainerHistory");
            var changeSet = (ChangeSet<string, ContainerHistory>)recordType.GetNewChangeSet();
            changeSet.AddUpdate(containerHistory.Id, containerHistory);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }
        //TABLE UPDATES
        /// <summary>
        /// Update a ContainerMaster record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="containerMaster"></param>
        /// <returns>The changeSetResult.  Caller must inspect for errors.</returns>
        public static ChangeSetResult<string> UpdateContainerMaster(IDataService dataService, ProcessChangeSetSettings settings,
                                              ContainerMaster containerMaster)
        {
            var recordType = (ContainerMasterRecordType)dataService.RecordTypes.Single(x => x.TypeName == "ContainerMaster");
            var changeSet = (ChangeSet<string, ContainerMaster>)recordType.GetNewChangeSet();
            changeSet.AddUpdate(containerMaster.Id, containerMaster);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }
        ///TABLE UPDATES
        /// <summary>
        /// Update a CustomerMaster record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="customerMaster"></param>
        /// <returns>The changeSetResult.  Caller must inspect for errors.</returns>
        public static ChangeSetResult<string> UpdateCustomerMaster(IDataService dataService, ProcessChangeSetSettings settings,
                                              CustomerMaster customerMaster)
        {
            var recordType = (CustomerMasterRecordType)dataService.RecordTypes.Single(x => x.TypeName == "CustomerMaster");
            var changeSet = (ChangeSet<string, CustomerMaster>)recordType.GetNewChangeSet();
            changeSet.AddUpdate(customerMaster.Id, customerMaster);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }
        ///TABLE UPDATES
        /// <summary>
        /// Update a DriverHistory record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="driverHistory"></param>
        /// <returns>The changeSetResult.  Caller must inspect for errors.</returns>
        public static ChangeSetResult<string> UpdateDriverHistory(IDataService dataService, ProcessChangeSetSettings settings,
                                              DriverHistory driverHistory)
        {
            var recordType = (DriverHistoryRecordType)dataService.RecordTypes.Single(x => x.TypeName == "DriverHistory");
            var changeSet = (ChangeSet<string, DriverHistory>)recordType.GetNewChangeSet();
            changeSet.AddUpdate(driverHistory.Id, driverHistory);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }
        ///TABLE UPDATES
        /// <summary>
        /// Updates a Messages record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public static ChangeSetResult<int> UpdateMessages(IDataService dataService, ProcessChangeSetSettings settings,
                                                  Messages messages)
        {
            var recordType = (MessagesRecordType)dataService.RecordTypes.Single(x => x.TypeName == "Messages");
            var changeSet = (ChangeSet<int, Messages>)recordType.GetNewChangeSet();
            changeSet.AddUpdate(messages.Id, messages);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }
        /// <summary>
        /// Update a UpdatePowerFuel record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="powerFuel"></param>
        /// <returns>The changeSetResult.  Caller must inspect for errors.</returns>
        public static ChangeSetResult<string> UpdatePowerFuel(IDataService dataService, ProcessChangeSetSettings settings,
            PowerFuel powerFuel)
        {
            var recordType = (PowerFuelRecordType)dataService.RecordTypes.Single(x => x.TypeName == "PowerFuel");
            var changeSet = (ChangeSet<string, PowerFuel>)recordType.GetNewChangeSet();
            changeSet.AddUpdate(powerFuel.Id, powerFuel);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }
        ///TABLE UPDATES
        /// <summary>
        /// Update a PowerHistory record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="powerHistory"></param>
        /// <returns>The changeSetResult.  Caller must inspect for errors.</returns>
        public static ChangeSetResult<string> UpdatePowerHistory(IDataService dataService, ProcessChangeSetSettings settings,
                                              PowerHistory powerHistory)
        {
            var recordType = (PowerHistoryRecordType)dataService.RecordTypes.Single(x => x.TypeName == "PowerHistory");
            var changeSet = (ChangeSet<string, PowerHistory>)recordType.GetNewChangeSet();
            changeSet.AddUpdate(powerHistory.Id, powerHistory);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }
        ///TABLE UPDATES
        /// <summary>
        /// Update a PowerMaster record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="powerMaster"></param>
        /// <returns>The changeSetResult.  Caller must inspect for errors.</returns>
        public static ChangeSetResult<string> UpdatePowerMaster(IDataService dataService, ProcessChangeSetSettings settings,
                                              PowerMaster powerMaster)
        {
            var recordType = (PowerMasterRecordType)dataService.RecordTypes.Single(x => x.TypeName == "PowerMaster");
            var changeSet = (ChangeSet<string, PowerMaster>)recordType.GetNewChangeSet();
            changeSet.AddUpdate(powerMaster.Id, powerMaster);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }
        /// <summary>
        /// Update a DriverDelay record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="driverDelay"></param>
        /// <returns>The changeSetResult.  Caller must inspect for errors.</returns>
        public static ChangeSetResult<string> UpdateDriverDelay(IDataService dataService, ProcessChangeSetSettings settings,
                                      DriverDelay driverDelay)
        {
            var recordType = (DriverDelayRecordType)dataService.RecordTypes.Single(x => x.TypeName == "DriverDelay");
            var changeSet = (ChangeSet<string, DriverDelay>)recordType.GetNewChangeSet();
            changeSet.AddUpdate(driverDelay.Id, driverDelay);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }

        /// <summary>
        /// Update a DriverStatus record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="driverStatus"></param>
        /// <returns>The changeSetResult.  Caller must inspect for errors.</returns>
        public static ChangeSetResult<string> UpdateDriverStatus(IDataService dataService, ProcessChangeSetSettings settings,
            DriverStatus driverStatus)
        {
            var recordType = (DriverStatusRecordType)dataService.RecordTypes.Single(x => x.TypeName == "DriverStatus");
            var changeSet = (ChangeSet<string, DriverStatus>)recordType.GetNewChangeSet();
            changeSet.AddUpdate(driverStatus.Id, driverStatus);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }
        /// <summary>
        /// Update a Trip record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="trip"></param>
        /// <returns>The changeSetResult.  Caller must inspect for errors.</returns>
        public static ChangeSetResult<string> UpdateTrip(IDataService dataService, ProcessChangeSetSettings settings,
            Trip trip)
        {
            var recordType = (TripRecordType)dataService.RecordTypes.Single(x => x.TypeName == "Trip");
            var changeSet = (ChangeSet<string, Trip>)recordType.GetNewChangeSet();
            changeSet.AddUpdate(trip.Id, trip);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }
        
        /// <summary>
        /// Update a TripSegment record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="tripSegment"></param>
        /// <returns>The changeSetResult.  Caller must inspect for errors.</returns>
        public static ChangeSetResult<string> UpdateTripSegment(IDataService dataService, ProcessChangeSetSettings settings,
            TripSegment tripSegment)
        {
            var recordType = (TripSegmentRecordType)dataService.RecordTypes.Single(x => x.TypeName == "TripSegment");
            var changeSet = (ChangeSet<string, TripSegment>)recordType.GetNewChangeSet();
            changeSet.AddUpdate(tripSegment.Id, tripSegment);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }
        
        /// <summary>
        /// Update a TripSegmentContainer record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="tripSegmentContainer"></param>
        /// <returns>The changeSetResult.  Caller must inspect for errors.</returns>
        public static ChangeSetResult<string> UpdateTripSegmentContainer(IDataService dataService, ProcessChangeSetSettings settings,
            TripSegmentContainer tripSegmentContainer)
        {
            var recordType = (TripSegmentContainerRecordType)dataService.RecordTypes.Single(x => x.TypeName == "TripSegmentContainer");
            var changeSet = (ChangeSet<string, TripSegmentContainer>)recordType.GetNewChangeSet();
            changeSet.AddUpdate(tripSegmentContainer.Id, tripSegmentContainer);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }

        /// <summary>
        /// Update a TripSegmentMileage record using TripSegment information.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="tripSegmentMileage"></param>
        /// <param name="tripSegment"></param>
        /// <param name="containersOnPowerId"></param>
        /// <returns>The changeSetResult.  Caller must inspect for errors.</returns>
        public static ChangeSetResult<string> UpdateTripSegmentMileageFromSegment(IDataService dataService, ProcessChangeSetSettings settings,
            TripSegmentMileage tripSegmentMileage, TripSegment tripSegment, List<ContainerMaster> containersOnPowerId)
        {
            tripSegmentMileage.TripSegMileageOdometerEnd = tripSegment.TripSegOdometerEnd;
            //We don't need to set State and Country. It should have been already set. But just in case....
            if (null == tripSegmentMileage.TripSegMileageState)
                tripSegmentMileage.TripSegMileageState = tripSegment.TripSegDestCustState;
            if (null == tripSegmentMileage.TripSegMileageCountry)
                tripSegmentMileage.TripSegMileageCountry = tripSegment.TripSegDestCustCountry;
            if (null != containersOnPowerId && containersOnPowerId.Count() > 0)
            {
                tripSegmentMileage.TripSegLoadedFlag = Common.GetSegmentLoadedFlag(tripSegment, containersOnPowerId);
            }
            tripSegmentMileage.TripSegMileagePowerId = tripSegment.TripSegPowerId;
            tripSegmentMileage.TripSegMileageDriverId = tripSegment.TripSegDriverId;
            tripSegmentMileage.TripSegMileageDriverName = tripSegment.TripSegDriverName;

            var recordType = (TripSegmentMileageRecordType)dataService.RecordTypes.Single(x => x.TypeName == "TripSegmentMileage");
            var changeSet = (ChangeSet<string, TripSegmentMileage>)recordType.GetNewChangeSet();
            changeSet.AddUpdate(tripSegmentMileage.Id, tripSegmentMileage);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }

        /// <summary>
        /// Update a UpdateTripSegmentMileage record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="tripSegmentMileage"></param>
        /// <returns>The changeSetResult.  Caller must inspect for errors.</returns>
        public static ChangeSetResult<string> UpdateTripSegmentMileage(IDataService dataService, ProcessChangeSetSettings settings,
            TripSegmentMileage tripSegmentMileage)
        {
            var recordType = (TripSegmentMileageRecordType)dataService.RecordTypes.Single(x => x.TypeName == "TripSegmentMileage");
            var changeSet = (ChangeSet<string, TripSegmentMileage>)recordType.GetNewChangeSet();
            changeSet.AddUpdate(tripSegmentMileage.Id, tripSegmentMileage);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }
        //////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Delete a ContainerHistory record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="containerHistory"></param>
        /// <returns>The changeSetResult.  Caller must inspect for errors.</returns>
        public static ChangeSetResult<string> DeleteContainerHistory(IDataService dataService, ProcessChangeSetSettings settings,
                                              ContainerHistory containerHistory)
        {
            var recordType = (ContainerHistoryRecordType)dataService.RecordTypes.Single(x => x.TypeName == "ContainerHistory");
            var changeSet = (ChangeSet<string, ContainerHistory>)recordType.GetNewChangeSet();
            changeSet.AddDelete(containerHistory.Id);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }

         /// <summary>
        /// Delete a ContainerMaster record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="containerMaster"></param>
        /// <returns>The changeSetResult.  Caller must inspect for errors.</returns>
        public static ChangeSetResult<string> DeleteContainerMaster(IDataService dataService, ProcessChangeSetSettings settings,
                                              ContainerMaster containerMaster)
        {
            var recordType = (ContainerMasterRecordType)dataService.RecordTypes.Single(x => x.TypeName == "ContainerMaster");
            var changeSet = (ChangeSet<string, ContainerMaster>)recordType.GetNewChangeSet();
            changeSet.AddDelete(containerMaster.Id);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }

        /// <summary>
        /// Delete a DriverDelay record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="driverDelay"></param>
        /// <returns>The changeSetResult.  Caller must inspect for errors.</returns>
        public static ChangeSetResult<string> DeleteDriverDelay(IDataService dataService, ProcessChangeSetSettings settings,
                                              DriverDelay driverDelay)
        {
            var recordType = (DriverDelayRecordType)dataService.RecordTypes.Single(x => x.TypeName == "DriverDelay");
            var changeSet = (ChangeSet<string, DriverDelay>)recordType.GetNewChangeSet();
            changeSet.AddDelete(driverDelay.Id);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }
        /// <summary>
        /// Delete a TripSegment record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="tripSegment"></param>
        /// <returns>The changeSetResult.  Caller must inspect for errors.</returns>
        public static ChangeSetResult<string> DeleteTripSegment(IDataService dataService, ProcessChangeSetSettings settings,
            TripSegment tripSegment)
        {
            var recordType = (TripSegmentRecordType)dataService.RecordTypes.Single(x => x.TypeName == "TripSegment");
            var changeSet = (ChangeSet<string, TripSegment>)recordType.GetNewChangeSet();
            changeSet.AddDelete(tripSegment.Id);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }
        /// <summary>
        /// Delete a TripSegmentContainer record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="tripSegmentContainer"></param>
        /// <returns>The changeSetResult.  Caller must inspect for errors.</returns>
        public static ChangeSetResult<string> DeleteTripSegmentContainer(IDataService dataService, ProcessChangeSetSettings settings,
            TripSegmentContainer tripSegmentContainer)
        {
            var recordType = (TripSegmentContainerRecordType)dataService.RecordTypes.Single(x => x.TypeName == "TripSegmentContainer");
            var changeSet = (ChangeSet<string, TripSegmentContainer>)recordType.GetNewChangeSet();
            changeSet.AddDelete(tripSegmentContainer.Id);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }
        /// <summary>
        /// Delete a TripSegmentMileage record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="tripSegmentMileage"></param>
        /// <returns>The changeSetResult.  Caller must inspect for errors.</returns>
        public static ChangeSetResult<string> DeleteTripSegmentMileage(IDataService dataService, ProcessChangeSetSettings settings,
            TripSegmentMileage tripSegmentMileage)
        {
            var recordType = (TripSegmentMileageRecordType)dataService.RecordTypes.Single(x => x.TypeName == "TripSegmentMileage");
            var changeSet = (ChangeSet<string, TripSegmentMileage>)recordType.GetNewChangeSet();
            changeSet.AddDelete(tripSegmentMileage.Id);
            var changeSetResult = recordType.ProcessChangeSet(dataService, changeSet, settings);
            return changeSetResult;
        }


        /// AREAMASTER Table queries
        /// <summary>
        ///  Get a list of all terminals for a given area.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="areaId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if areaId is null or if no entries are found</returns>
        public static List<string> GetTerminalsByArea(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string areaId, out DataServiceFault fault)
        {
            fault = null;
            var terminals = new List<string>();
            if (null != areaId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<AreaMaster>()
                    .Filter(y => y.Property(x => x.AreaId).EqualTo(areaId))
                    .OrderBy(x => x.TerminalId)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return terminals;
                }
                var terminalsArea = new List<AreaMaster>();
                terminalsArea = queryResult.Records.Cast<AreaMaster>().ToList();    
                terminals.AddRange(terminalsArea.Select
                    (terminalInstance => terminalInstance.TerminalId));
            }
            return terminals;
        }


        /// CODETABLE Table queries
        /// <summary>
        /// Get a list of all codetable values that are sent to the driver at login.
        /// CONTAINERSIZE,CONTAINERTYPE,DELAYCODES,EXCEPTIONCODES,REASONCODES,CONTAINERLEVEL
        /// Note: CONTAINERLEVEL is optional, based on a  preference:DEFUseContainerLevel
        /// Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="regionId"></param>
        /// <param name="prefDefCountry"></param>
        /// <param name="prefUseContainerLevel"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if no entries are found</returns>
        public static List<CodeTable> GetCodeTablesForDriver(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string regionId, string prefDefCountry, string prefUseContainerLevel, out DataServiceFault fault)
        {
            fault = null;
            var codetables = new List<CodeTable>();
            Query query = new Query
            {
                CurrentQuery = new QueryBuilder<CodeTable>()
                    .Filter(y => y.Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ContainerType)
                    .Or().Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ContainerSize)
                    .Or().Property(x => x.CodeName).EqualTo(CodeTableNameConstants.DelayCodes)
                    .Or().Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ExceptionCodes)
                    .Or().Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ReasonCodes)
                    .Or().Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ContainerLevel)
                    .Or().Property(x => x.CodeName).EqualTo(Constants.StatesPrefix + prefDefCountry))
                    .OrderBy(x => x.CodeName)
                    .OrderBy(x => x.CodeValue)
                    .GetQuery()
            };
            var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
            if (null != fault)
            {
                return codetables;
            }
            codetables = queryResult.Records.Cast<CodeTable>().ToList();

            //Filter the results
            //If a region id is in the CodeDisp5 field, make sure it matches the region id for the user
            //Also for reason codes, do not send the reason code SR#
            var filteredcodetables = 
                from entry in codetables
                where (entry.CodeDisp5 == regionId || entry.CodeDisp5 == null)
                && (entry.CodeValue != Constants.ScaleRefNotAvailable)
                select entry;

            //If the preference to use container level is not set, then remove container levels from the list
            if (prefUseContainerLevel != Constants.Yes ||
                prefUseContainerLevel == null)
            {
                filteredcodetables =
                    from entry in filteredcodetables
                    where (entry.CodeName != CodeTableNameConstants.ContainerLevel)
                    select entry;
            }
            return filteredcodetables.Cast<CodeTable>().ToList();
        }

        /// CODETABLE Table queries
        /// <summary>
        /// Get a list of all CONTAINERLEVEL codetable values.
        /// The preference:DEFUseContainerLevel determines whether container level is used.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if no entries are found</returns>
        public static List<CodeTable> GetContainerLevelCodes(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, out DataServiceFault fault)
        {
            fault = null;
            var containerlevels = new List<CodeTable>();
            Query query = new Query
            {
                CurrentQuery = new QueryBuilder<CodeTable>()
                    .Filter(y => y.Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ContainerLevel))
                    .OrderBy(x => x.CodeValue)
                    .GetQuery()
            };
            var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
            if (null != fault)
            {
                return containerlevels;
            }
            containerlevels = queryResult.Records.Cast<CodeTable>().ToList();
            return containerlevels;
        }

        /// CODETABLE Table queries
        /// <summary>
        /// Get a list of all CONTAINERTYPE codetable values.
        /// RegionId, if present, is stored in CodeDisp5. If null the code is included for all regions.
        /// Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="regionId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if no entries are found</returns>
        public static List<CodeTable> GetContainerTypeCodes(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string regionId, out DataServiceFault fault)
        {
            fault = null;
            var containerTypes = new List<CodeTable>();
            Query query = new Query
            {
                CurrentQuery = new QueryBuilder<CodeTable>()
                    .Filter(y => y.Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ContainerType)
                    .And(x => x.CodeDisp5).EqualTo(regionId)
                    .Or(x => x.CodeDisp5).IsNull())
                    .OrderBy(x => x.CodeValue)
                    .GetQuery()
            };
            var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
            if (null != fault)
            {
                return containerTypes;
            }
            containerTypes = queryResult.Records.Cast<CodeTable>().ToList();
            return containerTypes;
        }

        /// CODETABLE Table queries
        /// <summary>
        /// Get a list of all CONTAINERSIZE codetable values.
        /// Although CONTAINERSIZE contains both type and size, some types that do not have sizes will not be present in the
        /// CONTAINERSIZE table, so both CONTAINERTYPE and CONTAINERSIZE tables need to be sent
        /// RegionId, if present, is stored in CodeDisp5. If null the code is included for all regions.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="regionId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if no entries are found</returns>
        public static List<CodeTable> GetContainerTypeSizeCodes(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string regionId, out DataServiceFault fault)
        {
            fault = null;
            var containerTypeSizes = new List<CodeTable>();
            Query query = new Query
            {
                CurrentQuery = new QueryBuilder<CodeTable>()
                    .Filter(y => y.Property(x => x.CodeName).EqualTo(CodeTableNameConstants.ContainerSize)
                    .And(x => x.CodeDisp5).EqualTo(regionId)
                    .Or(x => x.CodeDisp5).IsNull())
                    .OrderBy(x => x.CodeValue)
                    .GetQuery()
            };
            var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
            if (null != fault)
            {
                return containerTypeSizes;
            }
            containerTypeSizes = queryResult.Records.Cast<CodeTable>().ToList();
            return containerTypeSizes;
        }
        /// CODETABLE Table queries
        /// <summary>
        /// Get a list of all STATES... codetable values for a country.
        /// Codetable is the prefix STATES flollowed by the 3 character country code
        /// Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="regionId"></param>
        /// <param name="country"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if no entries are found</returns>
        public static List<CodeTable> GetStateCodesForCountry(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string regionId, string country, out DataServiceFault fault)
        {
            fault = null;
            var states = new List<CodeTable>();
            Query query = new Query
            {
                CurrentQuery = new QueryBuilder<CodeTable>()
                    .Filter(y => y.Property(x => x.CodeName).EqualTo(Constants.StatesPrefix+country)
                    .And(x => x.CodeDisp5).EqualTo(regionId)
                    .Or(x => x.CodeDisp5).IsNull())
                    .OrderBy(x => x.CodeValue)
                    .GetQuery()
            };
            var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
            if (null != fault)
            {
                return states;
            }
            states = queryResult.Records.Cast<CodeTable>().ToList();
            return states;
        }
        /// CODETABLE Table queries
        /// <summary>
        /// Get a single codetable record.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="codeName"></param>
        /// <param name="codeValue"></param>
        /// <param name="fault"></param>
        /// <returns>An empty CodeTable if no entry is found</returns>
        public static CodeTable GetCodeTableEntry(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string codeName, string codeValue, out DataServiceFault fault)
        {
            fault = null;
            var codeTableEntry = new CodeTable();
            Query query = new Query
            {
                CurrentQuery = new QueryBuilder<CodeTable>()
                    .Filter(y => y.Property(x => x.CodeName).EqualTo(codeName)
                    .And(x => x.CodeValue).EqualTo(codeValue))
                    .GetQuery()
            };
            var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
            if (null != fault)
            {
                return codeTableEntry;
            }
            codeTableEntry = queryResult.Records.Cast<CodeTable>().FirstOrDefault();
            return codeTableEntry;
        }
 
        /// COMMODITYMASTER Table queries
        /// <summary>
        /// Get a list of all commodities with the universal flag set to Y.
        /// These will be sent to the driver at login..
        /// Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if no entries are found</returns>
        public static List<CommodityMaster> GetMasterCommoditiesForDriver(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, out DataServiceFault fault)
        {
            fault = null;
            var commodityTypes = new List<CommodityMaster>();
            Query query = new Query
            {
                CurrentQuery = new QueryBuilder<CommodityMaster>()
                    .Filter(y => y.Property(x => x.InactiveFlag).NotEqualTo(Constants.Yes)
                    .Or(x => x.InactiveFlag).IsNull()
                    .And(x => x.UniversalFlag).EqualTo(Constants.Yes))
                    .OrderBy(x => x.CommodityDesc)
                    .GetQuery()
            };
            var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
            if (null != fault)
            {
                return commodityTypes;
            }
            commodityTypes = queryResult.Records.Cast<CommodityMaster>().ToList();
            return commodityTypes;
        }

        /// CONTAINERCHANGE Table queries
        /// <summary>
        ///  Get a list of all container master updates after a given date time.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="dateTime"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if dateTime is null or no entries are found</returns>
        public static List<ContainerChange> GetContainerChangesAll(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, DateTime? dateTime, out DataServiceFault fault)
        {
            fault = null;
            var containers = new List<ContainerChange>();
            if (null != dateTime)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<ContainerChange>()
                        .Filter(y => y.Property(x => x.ActionDate).GreaterThan(dateTime))
                        .OrderBy(x => x.ContainerNumber)
                        .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return containers;
                }
                containers = queryResult.Records.Cast<ContainerChange>().ToList();
            }
            return containers;
        }

        /// CONTAINERCHANGE Table queries
        /// <summary>
        ///  Get a list of all container master updates after a given date time for a given region.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="dateTime"></param>
        /// <param name="regionId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if dateTime or regionId is null or no entries are found</returns>
        public static List<ContainerChange> GetContainerChangesForRegion(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, DateTime? dateTime, string regionId, out DataServiceFault fault)
        {
            fault = null;
            var containers = new List<ContainerChange>();
            if (null != dateTime && regionId != null)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<ContainerChange>()
                        .Filter(y => y.Property(x => x.ActionDate).GreaterThan(dateTime)
                        .And().Property(x => x.RegionId).EqualTo(regionId))
                        .OrderBy(x => x.ContainerNumber)
                        .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return containers;
                }
                containers = queryResult.Records.Cast<ContainerChange>().ToList();
            }
            return containers;
        }
        /// CONTAINERHISTORY Table queries
        /// <summary>
        ///  Get a list of container history records for a given container
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="containerNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if containerNumber is null or no entries are found</returns>
        public static List<ContainerHistory> GetContainerHistory(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string containerNumber, out DataServiceFault fault)
        {
            fault = null;
            var containers = new List<ContainerHistory>();
            if (null != containerNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<ContainerHistory>()
                    .Filter(y => y.Property(x => x.ContainerNumber).EqualTo(containerNumber))
                    .OrderBy(x => x.ContainerSeqNumber)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return containers;
                }
                containers = queryResult.Records.Cast<ContainerHistory>().ToList();
            }
            return containers;
        }

        /// CONTAINERHISTORY Table  queries
        /// <summary>
        /// Gets a single container history record
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="containerNumber"></param>
        /// <param name="containerSeqNumber"></param>
        /// <param name="fault"></param>
        /// <returns></returns>
        public static ContainerHistory GetContainerHistoryOne(IDataService dataService, ProcessChangeSetSettings settings,
         string userCulture, IEnumerable<long> userRoleIds, string containerNumber, int containerSeqNumber,out DataServiceFault fault)
         {
            fault = null;
            var containerHistory = new ContainerHistory();
            if (null != containerNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<ContainerHistory>()
                             .Filter(t => t.Property(p => p.ContainerNumber).EqualTo(containerNumber)
                             .And().Property(x => x.ContainerSeqNumber).EqualTo(containerSeqNumber))
                             .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                    out fault);
                if (null != fault)
                {
                    return containerHistory;
                }
                containerHistory = (ContainerHistory)queryResult.Records.Cast<ContainerHistory>().FirstOrDefault();
            }
            return containerHistory;
        }

        /// CONTAINERHISTORY Table  queries
        /// <summary>
        ///  Get the last container history record for a given container number
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="containerNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty ContainerHistory if containerNumber is null or no entry is found</returns>
        public static ContainerHistory GetContainerHistoryLast(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string containerNumber, out DataServiceFault fault)
        {
            fault = null;
            var containerHistory = new ContainerHistory();
            if (null != containerNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<ContainerHistory>().Top(1)
                             .Filter(t => t.Property(p => p.ContainerNumber).EqualTo(containerNumber))
                             .OrderBy(p => p.ContainerSeqNumber, Direction.Descending)
                             .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                    out fault);
                if (null != fault)
                {
                    return containerHistory;
                }
                containerHistory = (ContainerHistory)queryResult.Records.Cast<ContainerHistory>().FirstOrDefault();
            }
            return containerHistory;
        }
        /// CONTAINERHISTORY Table  queries
        /// <summary>
        ///  Get the last container history record for a given container number on a trip
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="containerNumber"></param>
        /// <param name="currentTripNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty ContainerHistory if containerNumber is null or no entry is found</returns>
        public static ContainerHistory GetContainerHistoryLastTrip(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string containerNumber, string currentTripNumber, out DataServiceFault fault)
        {
            fault = null;
            var containerHistory = new ContainerHistory();
            if (null != containerNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<ContainerHistory>().Top(1)
                             .Filter(t => t.Property(p => p.ContainerNumber).EqualTo(containerNumber)
                             .And().Property(p => p.ContainerTripNumber).IsNotNull()
                             .And().Property(p => p.ContainerTripNumber).NotEqualTo(currentTripNumber))
                             .OrderBy(p => p.ContainerSeqNumber, Direction.Descending)
                             .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                    out fault);
                if (null != fault)
                {
                    return containerHistory;
                }
                containerHistory = (ContainerHistory)queryResult.Records.Cast<ContainerHistory>().FirstOrDefault();
            }
            return containerHistory;
        }

        /// CONTAINERMASTER Table queries
        /// <summary>
        ///  Get a a container master record for a given container number.
        ///  Caller needs to check if the fault is non-null before using the returned record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="containerNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty ContainerMaster if containerNumber is null or record does not exist for containerNumber</returns>
        public static ContainerMaster GetContainer(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string containerNumber, out DataServiceFault fault)
        {
            fault = null;
            var container = new ContainerMaster();
            if (null != containerNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<ContainerMaster>()
                    .Filter(y => y.Property(x => x.ContainerNumber).EqualTo(containerNumber))
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return container;
                }
                container = (ContainerMaster)queryResult.Records.Cast<ContainerMaster>().FirstOrDefault();
            }
            return container;
        }
        /// CONTAINERMASTER Table queries
        /// <summary>
        ///  Get a a container master record for a given container bar code.
        ///  Caller needs to check if the fault is non-null before using the returned record.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="containerBarCode"></param>
        /// <param name="fault"></param>
        /// <returns>An empty ContainerMaster if containerNumber is null or record does not exist for containerNumber</returns>
        public static ContainerMaster GetContainerByBarCode(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string containerBarCode, out DataServiceFault fault)
        {
            fault = null;
            var container = new ContainerMaster();
            if (null != containerBarCode)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<ContainerMaster>()
                    .Filter(y => y.Property(x => x.ContainerBarCodeNo).EqualTo(containerBarCode))
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return container;
                }
                container = (ContainerMaster)queryResult.Records.Cast<ContainerMaster>().FirstOrDefault();
            }
            return container;
        }
        /// CONTAINERMASTER Table queries
        /// <summary>
        ///  Get a list of all container master records for a given region.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="regionId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if dateTime is null or no entries are found</returns>
        public static List<ContainerMaster> GetContainerMasterForRegion(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string regionId, out DataServiceFault fault)
        {
            fault = null;
            var containers = new List<ContainerMaster>();
            if (null != regionId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<ContainerMaster>()
                        .Filter(y => y.Property(x => x.ContainerRegionId).EqualTo(regionId))
                        .OrderBy(x => x.ContainerNumber)
                        .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return containers;
                }
                containers = queryResult.Records.Cast<ContainerMaster>().ToList();
            }
            return containers;
        }
        /// CONTAINERMASTER Table queries
        /// <summary>
        ///  Get a list of all container master records for a given region.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if dateTime is null or no entries are found</returns>
        public static List<ContainerMaster> GetContainerMasterAll(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, out DataServiceFault fault)
        {
            fault = null;
            var containers = new List<ContainerMaster>();
            Query query = new Query
            {
                CurrentQuery = new QueryBuilder<ContainerMaster>()
                    .OrderBy(x => x.ContainerNumber)
                    .GetQuery()
            };
            var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
            if (null != fault)
            {
                return containers;
            }
            containers = queryResult.Records.Cast<ContainerMaster>().ToList();
            return containers;
        }

        /// CONTAINERMASTER Table queries
        /// <summary>
        ///  Get a list of container master that are on a given power unit..
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="powerId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if powerId is null or no entries are found</returns>
        public static List<ContainerMaster> GetContainersForPowerId(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string powerId, out DataServiceFault fault)
        {
            fault = null;
            var containers = new List<ContainerMaster>();
            if (null != powerId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<ContainerMaster>()
                    .Filter(y => y.Property(x => x.ContainerPowerId).EqualTo(powerId))
                    .OrderBy(x => x.ContainerNumber)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return containers;
                }
                containers = queryResult.Records.Cast<ContainerMaster>().ToList();
            }
            return containers;
        }
        /// CONTAINERMASTER Table queries
        /// <summary>
        ///  Get a list of containers from the container master that are on a given trip number
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="containerNumberList"></param>List of containernumbers
        /// <param name="fault"></param>
        /// <returns>An empty list if powerId is null or no entries are found</returns>
        public static List<ContainerMaster> GetContainersForTrip(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, List<string> containerNumberList, out DataServiceFault fault)
        {
            fault = null;
            var containers = new List<ContainerMaster>();
            if (null != containerNumberList)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<ContainerMaster>()
                    .Filter(y => y.Property(x => x.ContainerNumber).In(containerNumberList.ToArray()))
                    .OrderBy(x => x.ContainerNumber)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return containers;
                }
                containers = queryResult.Records.Cast<ContainerMaster>().ToList();
            }
            return containers;
        }

        /// CUSTOMERDIRECTIONS queries
        /// <summary>
        ///  Get a list of directions for each destination custhostcode to be sent to a given driver.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="custHostCode"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if custHostCode is null or no entries are found</returns>
        public static List<CustomerDirections> GetCustomerDirections(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string custHostCode, out DataServiceFault fault)
        {
            fault = null;
            var custDirections = new List<CustomerDirections>();
            if (null != custHostCode)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<CustomerDirections>()
                    .Filter(y => y.Property(x => x.CustHostCode).EqualTo(custHostCode))
                    .OrderBy(x => x.CustHostCode)
                    .OrderBy(x => x.DirectionsSeqNo)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return custDirections;
                }
                custDirections = queryResult.Records.Cast<CustomerDirections>().ToList();
            }
            return custDirections;
        }
        /// CUSTOMERCOMMODITY queries
        /// <summary>
        ///  Get a list of commodities for each destination custhostcode to be sent to a given driver.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="custHostCode"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if custHostCode is null or no entries are found</returns>
        public static List<CustomerCommodity> GetCustomerCommodities(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string custHostCode, out DataServiceFault fault)
        {
            fault = null;
            var custCommodities = new List<CustomerCommodity>();
            if (null != custHostCode)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<CustomerCommodity>()
                    .Filter(y => y.Property(x => x.CustHostCode).EqualTo(custHostCode))
                    .OrderBy(x => x.CustHostCode)
                    .OrderBy(x => x.CustCommodityDesc)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return custCommodities;
                }
                custCommodities = queryResult.Records.Cast<CustomerCommodity>().ToList();
            }
            return custCommodities;
        }
        /// CUSTOMERLOCATION queries
        /// <summary>
        ///  Get a list of locations for each destination custhostcode to be sent to a given driver.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="custHostCode"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if custHostCode is null or no entries are found</returns>
        public static List<CustomerLocation> GetCustomerLocations(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string custHostCode, out DataServiceFault fault)
        {
            fault = null;
            var custLocations = new List<CustomerLocation>();
            if (null != custHostCode)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<CustomerLocation>()
                    .Filter(y => y.Property(x => x.CustHostCode).EqualTo(custHostCode))
                    .OrderBy(x => x.CustHostCode)
                    .OrderBy(x => x.CustLocation)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return custLocations;
                }
                custLocations = queryResult.Records.Cast<CustomerLocation>().ToList();
            }
            return custLocations;
        }
        /// CUSTOMERMASTER Table queries
        /// <summary>
        ///  Get a customer record from the CustomerMaster
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="custHostCode"></param>
        /// <param name="fault"></param>
        /// <returns>An empty CustomerMaster if custHostCode is null or record does not exist for customer</returns>
        public static CustomerMaster GetCustomer(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string custHostCode, out DataServiceFault fault)
        {
            fault = null;
            var customer = new CustomerMaster();
            if (null != custHostCode)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<CustomerMaster>()
                         .Filter(t => t.Property(p => p.CustHostCode).EqualTo(custHostCode))
                         .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                    out fault);
                if (null != fault)
                {
                    return customer;
                }
                customer = (CustomerMaster)queryResult.Records.Cast<CustomerMaster>().FirstOrDefault();
            }
            return customer;
        }
        /// <summary>
        /// Get customer record from the customer master for a given terminal
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="terminalId"></param>
        /// <param name="fault"></param>
        /// <returns></returns>
        public static CustomerMaster GetCustomerForTerminal(IDataService dataService, ProcessChangeSetSettings settings,
        string userCulture, IEnumerable<long> userRoleIds, string terminalId, out DataServiceFault fault)
        {
            fault = null;

            fault = null;
            var customer = new CustomerMaster();
            if (null != terminalId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<CustomerMaster>()
                          .Filter(t => t.Property(p => p.ServingTerminalId).EqualTo(terminalId)
                          .And().Property(p => p.CustType).EqualTo(CustomerTypeConstants.Yard))
                          .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                    out fault);
                if (null != fault)
                {
                    return customer;
                }
                customer = (CustomerMaster)queryResult.Records.Cast<CustomerMaster>().FirstOrDefault();
            }
            return customer;
        }

        /// DRIVERDELAY Table queries
        /// <summary>
        ///  Get an employee record from the EmployeeMaster
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="driverId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if driverId is null or no entries are found</returns>
        public static List<DriverDelay> GetDriverDelaysOpenEnded(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string driverId, out DataServiceFault fault)
        {
            fault = null;
            var driverDelays = new List<DriverDelay>();
            if (null != driverId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<DriverDelay>()
                        .Filter(t => t.Property(p => p.DriverId).EqualTo(driverId)
                        .And().Property(p => p.DelayEndDateTime).IsNull())
                        .OrderBy(p => p.DelayStartDateTime, Direction.Descending)
                        .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return driverDelays;
                }
                driverDelays = queryResult.Records.Cast<DriverDelay>().ToList();
            }
            return driverDelays;
        }
        /// <summary>
        /// Get driver delays for a given trip number
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripNumber"></param>
        /// <param name="fault"></param>
        /// <returns></returns>
        public static List<DriverDelay> GetDriverDelaysForTrip(IDataService dataService, ProcessChangeSetSettings settings,
               string userCulture, IEnumerable<long> userRoleIds, string tripNumber, out DataServiceFault fault)
        {
            fault = null;
            var driverDelays = new List<DriverDelay>();
            if (null != tripNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<DriverDelay>()
                        .Filter(t => t.Property(p => p.TripNumber).EqualTo(tripNumber))
                        .OrderBy(p => p.TripSegNumber)
                        .OrderBy(p => p.DelaySeqNumber)
                        .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return driverDelays;
                }
                driverDelays = queryResult.Records.Cast<DriverDelay>().ToList();
            }
            return driverDelays;
        }
        /// DriverDelay Table queries
        /// <summary>
        /// Get the last delay record for driverid and tripnumber/driver id
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="driverId"></param>
        /// <param name="tripNumberOrDriverId"></param>
        /// <param name="fault"></param>
        /// <returns></returns>
        public static DriverDelay GetDriverDelayLast(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string driverId, string tripNumberOrDriverId, out DataServiceFault fault)
        {
            //Pass false to GetQuery so that when a # sign is used in the filter, records will be returned.
            //Otherwise no records will be found to match.
            fault = null;
            var driverDelay = new DriverDelay();
            if (null != driverId && null != tripNumberOrDriverId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<DriverDelay>()
                        .Filter(t => t.Property(p => p.DriverId).EqualTo(driverId).And()
                        .Property(p => p.TripNumber).EqualTo(tripNumberOrDriverId))
                        .OrderBy(p => p.DelaySeqNumber, Direction.Descending)
                        .GetQuery(false)
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return driverDelay;
                }
                driverDelay = queryResult.Records.Cast<DriverDelay>().FirstOrDefault();
            }
            return driverDelay;
        }

        ///DRIVEREFFICIENCY table queries
        /// <summary>
        /// Get a driver efficiency record for a given trip
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="driverId"></param>
        /// <param name="tripNumber"></param>
        /// <param name="fault"></param>
        /// <returns></returns>
        public static DriverEfficiency GetDriverEfficiencyForDriverTrip(IDataService dataService, ProcessChangeSetSettings settings,
                            string userCulture, IEnumerable<long> userRoleIds, string driverId, string tripNumber, out DataServiceFault fault)
        {
            fault = null;
            var driverEfficiency = new DriverEfficiency();
            if (null != tripNumber && null != driverId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<DriverEfficiency>()
                    .Filter(y => y.Property(x => x.TripDriverId).EqualTo(driverId)
                    .And(x => x.TripNumber).EqualTo(tripNumber))
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return driverEfficiency;
                }
                driverEfficiency = queryResult.Records.Cast<DriverEfficiency>().FirstOrDefault();
            }
            return driverEfficiency;
        }
        /// DRIVERSTATUS Table  queries
        /// <summary>
        ///  Get an driver status record from the DriverStatus
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="driverId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty DriverStatus if driverId is null or record does not exist for driver</returns>
        public static DriverStatus GetDriverStatus(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string driverId, out DataServiceFault fault)
        {
            fault = null;
            var driver = new DriverStatus();
            if (null != driverId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<DriverStatus>()
                         .Filter(t => t.Property(p => p.EmployeeId).EqualTo(driverId))
                         .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                    out fault);
                if (null != fault)
                {
                    return driver;
                }
                driver = (DriverStatus)queryResult.Records.Cast<DriverStatus>().FirstOrDefault();
            }
            return driver;
        }
        /// DRIVERHISTORY Table queries
        /// <summary>
        ///  Get driver history records for a given trip
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="driverId"></param>
        /// <param name="tripNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if areaId is null or no entries are found</returns>
        public static List<DriverHistory> GetDriverHistoryForTrip(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string driverId, string tripNumber, out DataServiceFault fault)
        {
            fault = null;
            var driverHistory = new List<DriverHistory>();
            if (null != driverId && null != tripNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<DriverHistory>()
                        .Filter(y => y.Property(x => x.EmployeeId).EqualTo(driverId)
                        .And().Property(x => x.TripNumber).EqualTo(tripNumber))
                        .GetQuery()
                };

                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return driverHistory;
                }
                driverHistory = queryResult.Records.Cast<DriverHistory>().ToList();
            }
            return driverHistory;
        }

        /// DRIVERHISTORY Table  queries
        /// <summary>
        ///  Get the last driver history record for a given driver and trip number
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="driverId"></param>
        /// <param name="tripNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty DriverHistory if driverId or tripNumber is null or no entry is found</returns>
        public static DriverHistory GetDriverHistoryLast(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string driverId, string tripNumber, out DataServiceFault fault)
        {
            fault = null;
            var driverHistory = new DriverHistory();
            if (null != driverId && null != tripNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<DriverHistory>().Top(1)
                             .Filter(t => t.Property(p => p.EmployeeId).EqualTo(driverId)
                             .And().Property(p => p.TripNumber).EqualTo(tripNumber))
                             .OrderBy(p => p.DriverSeqNumber, Direction.Descending)
                             .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                    out fault);
                if (null != fault)
                {
                    return driverHistory;
                }
                driverHistory = (DriverHistory)queryResult.Records.Cast<DriverHistory>().FirstOrDefault();
            }
            return driverHistory;
        }

        /// EMPLOYEEMASTER Table queries
        /// <summary>
        ///  Get an employee record from the EmployeeMaster
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="employeeId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty EmployeeMaster if employeeId is null or record does not exist for employee</returns>
        public static EmployeeMaster GetEmployee(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string employeeId, out DataServiceFault fault)
        {
            fault = null;
            EmployeeMaster employee = new EmployeeMaster();
            if (null != employeeId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<EmployeeMaster>()
                         .Filter(t => t.Property(p => p.EmployeeId).EqualTo(employeeId))
                         .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                    out fault);
                if (null != fault)
                {
                    return employee;
                }
                employee = (EmployeeMaster) queryResult.Records.Cast<EmployeeMaster>().FirstOrDefault();
            }
            return employee;
        }
        /// EMPLOYEEMASTER Table queries
        /// <summary>
        ///  Get an employee record from the EmployeeMaster for a driver
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="employeeId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty EmployeeMaster if employeeId is null or record does not exist for driver</returns>
        public static EmployeeMaster GetEmployeeDriver(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string employeeId, out DataServiceFault fault)
        {
            fault = null;
            EmployeeMaster employee = new EmployeeMaster();
            if (null != employeeId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<EmployeeMaster>()
                         .Filter(t => t.Property(p => p.EmployeeId).EqualTo(employeeId)
                        .And().Property(x => x.SecurityLevel).EqualTo(SecurityLevelConstants.Driver))
                         .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                    out fault);
                if (null != fault)
                {
                    return employee;
                }
                employee = (EmployeeMaster)queryResult.Records.Cast<EmployeeMaster>().FirstOrDefault();
            }
            return employee;
        }
        /// EMPLOYEEMASTER Table queries
        /// <summary>
        ///  Get an employee record from the EmployeeMaster for any employee
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="employeeId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty EmployeeMaster if employeeId is null or record does not exist for driver</returns>
        public static EmployeeMaster GetEmployeeMaster(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string employeeId, out DataServiceFault fault)
        {
            fault = null;
            EmployeeMaster employee = new EmployeeMaster();
            if (null != employeeId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<EmployeeMaster>()
                         .Filter(t => t.Property(p => p.EmployeeId).EqualTo(employeeId))
                         .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                    out fault);
                if (null != fault)
                {
                    return employee;
                }
                employee = (EmployeeMaster)queryResult.Records.Cast<EmployeeMaster>().FirstOrDefault();
            }
            return employee;
        }
        /// EMPLOYEEMASTER Table queries
        /// <summary>
        ///  Get a list of users that have access to messaging for the driver's area
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="areaId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if areaId is null or no entries are found</returns>
        public static List<EmployeeMaster> GetDispatcherListForArea(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string areaId, out DataServiceFault fault)
        {
            fault = null;
            var terminalsInArea = new List<string>();
            var users = new List<EmployeeMaster>();
            if (null != areaId)
            {
                terminalsInArea = Common.GetTerminalsByArea
                    (dataService, settings, userCulture, userRoleIds, areaId, out fault);
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<EmployeeMaster>()
                        .Filter(y => y.Property(x => x.SecurityLevel).NotEqualTo(SecurityLevelConstants.Driver)
                        .And().Property(x => x.AllowMessaging).EqualTo(Constants.Yes)
                        .And().Property(x => x.TerminalId).In(terminalsInArea.ToArray()))
                        .OrderBy(x => x.LastName)
                        .OrderBy(x => x.FirstName)
                        .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return users;
                }
                users = queryResult.Records.Cast<EmployeeMaster>().ToList();
            }
            return users;
        }
        /// EMPLOYEEMASTER Table queries
        /// <summary>
        ///  Get a list of users that have access to messaging for the driver's region
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="regionId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if areaId is null or no entries are found</returns>
        public static List<EmployeeMaster> GetDispatcherListForRegion(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string regionId, out DataServiceFault fault)
        {
            fault = null;
            var users = new List<EmployeeMaster>();
            if (null != regionId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<EmployeeMaster>()
                        .Filter(y => y.Property(x => x.SecurityLevel).NotEqualTo(SecurityLevelConstants.Driver)
                        .And().Property(x => x.AllowMessaging).EqualTo(Constants.Yes)
                        .And().Property(x => x.RegionId).EqualTo(regionId))
                        .OrderBy(x => x.LastName)
                        .OrderBy(x => x.FirstName)
                        .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return users;
                }
                users = queryResult.Records.Cast<EmployeeMaster>().ToList();
            }
            return users;
        }
        /// <summary>
        /// Get the last history trip record
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripNumber"></param>
        /// <param name="fault"></param>
        /// <returns></returns>
        public static HistTrip GetHistTripLast(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string tripNumber, out DataServiceFault fault)
        {
            fault = null;
            var histTrip = new HistTrip();
            if (null != tripNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<HistTrip>()
                        .Filter(t => t.Property(p => p.TripNumber).EqualTo(tripNumber))
                        .OrderBy(p => p.HistSeqNo, Direction.Descending)
                        .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return histTrip;
                }
                histTrip = queryResult.Records.Cast<HistTrip>().FirstOrDefault();
            }
            return histTrip;
        }


        /// MESSAGE Table queries
        /// <summary>
        ///  Get messages that need to be sent to the driver
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="employeeId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if areaId is null or no entries are found</returns>
        public static List<Messages> GetMessagesForDriver(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string employeeId, out DataServiceFault fault)
        {
            fault = null;
            var messages = new List<Messages>();
            if (null != employeeId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<Messages>()
                        .Filter(y => y.Property(x => x.ReceiverId).EqualTo(employeeId)
                        .And().Property(x => x.Processed).EqualTo(Constants.No)
                        .And().Property(x => x.DeleteFlag).EqualTo(Constants.No))
                        .OrderBy(x => x.CreateDateTime)
                        .GetQuery()
                };

                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return messages;
                }
                messages = queryResult.Records.Cast<Messages>().ToList();
            }
            return messages;
        }
        /// POWERFUEL Table queries
        /// <summary>
        ///  Get power fuel records for a given trip
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="powerId"></param>
        /// <param name="loginDateTime"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if areaId is null or no entries are found</returns>
        public static List<PowerFuel> GetPowerFuelSinceLogin(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string powerId, DateTime? loginDateTime, out DataServiceFault fault)
        {
            fault = null;
            var powerFuel = new List<PowerFuel>();
            if (null != powerId && null != loginDateTime)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<PowerFuel>()
                        .Filter(y => y.Property(x => x.PowerId).EqualTo(powerId)
                        .And().Property(x => x.PowerDateOfFuel).GreaterThanOrEqualTo(loginDateTime))
                        .GetQuery()
                };

                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return powerFuel;
                }
                powerFuel = queryResult.Records.Cast<PowerFuel>().ToList();
            }
            return powerFuel;
        }
        /// POWERFUEL Table queries
        /// <summary>
        ///  Get power fuel records for a given trip
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="powerId"></param>
        /// <param name="tripNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if areaId is null or no entries are found</returns>
        public static List<PowerFuel> GetPowerFuelForTrip(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string powerId, string tripNumber, out DataServiceFault fault)
        {
            fault = null;
            var powerFuel = new List<PowerFuel>();
            if (null != powerId && null != tripNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<PowerFuel>()
                        .Filter(y => y.Property(x => x.PowerId).EqualTo(powerId)
                        .And().Property(x => x.TripNumber).EqualTo(tripNumber))
                        .GetQuery()
                };

                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return powerFuel;
                }
                powerFuel = queryResult.Records.Cast<PowerFuel>().ToList();
            }
            return powerFuel;
        }
        /// POWERFUEL Table queries
        /// <summary>
        /// Get the last power fuel record for powerid and tripnumber/driver id
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="powerId"></param>
        /// <param name="tripNumberOrEmployeeId"></param>
        /// <param name="fault"></param>
        /// <returns></returns>
        public static PowerFuel GetPowerFuelLast(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string powerId, string tripNumberOrEmployeeId, out DataServiceFault fault)
        {
            fault = null;
            var powerFuel = new PowerFuel();
            if (null != powerId && null != tripNumberOrEmployeeId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<PowerFuel>()
                        .Filter(t => t.Property(p => p.PowerId).EqualTo(powerId).And()
                        .Property(p => p.TripNumber).EqualTo(tripNumberOrEmployeeId))
                        .OrderBy(p => p.PowerFuelSeqNumber, Direction.Descending)
                        .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return powerFuel;
                }
                powerFuel = queryResult.Records.Cast<PowerFuel>().FirstOrDefault();
            }
            return powerFuel;
        }
        /// POWERHISTORY Table queries
        /// <summary>
        ///  Get power history records for a given trip
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="powerId"></param>
        /// <param name="tripNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if areaId is null or no entries are found</returns>
        public static List<PowerHistory> GetPowerHistoryForTrip(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string powerId, string tripNumber, out DataServiceFault fault)
        {
            fault = null;
            var powerHistory = new List<PowerHistory>();
            if (null != powerId && null != tripNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<PowerHistory>()
                        .Filter(y => y.Property(x => x.PowerId).EqualTo(powerId)
                        .And().Property(x => x.PowerCurrentTripNumber).EqualTo(tripNumber))
                        .GetQuery()
                };

                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return powerHistory;
                }
                powerHistory = queryResult.Records.Cast<PowerHistory>().ToList();
            }
            return powerHistory;
        }

        /// POWERHISTORY Table  queries
        /// <summary>
        ///  Get the last power history record for a given power id 
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="powerId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty PowerHistory if powerId is null or no entry is found</returns>
        public static PowerHistory GetPowerHistoryLast(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string powerId, out DataServiceFault fault)
        {
            fault = null;
            var powerHistory = new PowerHistory();
            if (null != powerId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<PowerHistory>().Top(1)
                             .Filter(t => t.Property(p => p.PowerId).EqualTo(powerId))
                             .OrderBy(p => p.PowerSeqNumber, Direction.Descending)
                             .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                    out fault);
                if (null != fault)
                {
                    return powerHistory;
                }
                powerHistory = (PowerHistory)queryResult.Records.Cast<PowerHistory>().FirstOrDefault();
            }
            return powerHistory;
        }
        /// POWERMASTER Table queries
        /// <summary>
        ///  Get a a power master record for a given power unit.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="powerId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty PowerMaster if powerId is null or record does not exist for powerId</returns>
        public static PowerMaster GetPowerUnit(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string powerId, out DataServiceFault fault)
        {
            fault = null;
            var powerunit = new PowerMaster();
            if (null != powerId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<PowerMaster>()
                    .Filter(y => y.Property(x => x.PowerId).EqualTo(powerId))
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return powerunit;
                }
                powerunit = (PowerMaster)queryResult.Records.Cast<PowerMaster>().FirstOrDefault();
            }
            return powerunit;
        }


        /// POWERMASTER Table queries
        /// <summary>
        ///  Get a a power master record for a given power unit and region id.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="powerId"></param>
        /// <param name="regionId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if powerId or regionId is null or record does not exist for powerId and regionId</returns>
        public static PowerMaster GetPowerUnitForRegion(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string powerId, string regionId, out DataServiceFault fault)
        {
            fault = null;
            var powerunit = new PowerMaster();
            if (null != powerId && null != regionId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<PowerMaster>()
                    .Filter(y => y.Property(x => x.PowerId).EqualTo(powerId)
                    .And().Property(x => x.PowerRegionId).EqualTo(regionId))
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return powerunit;
                }
                powerunit = (PowerMaster)queryResult.Records.Cast<PowerMaster>().FirstOrDefault();
            }
            return powerunit;
        }
        /// PREFERENCE Table queries
        /// <summary>
        ///  Get a list of all preferences for a terminalId.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="terminalId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if terminalId is null or no entries are found</returns>
        public static List<Preference> GetPreferences(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string terminalId, out DataServiceFault fault)
        {
            fault = null;
            var preferences = new List<Preference>();
            if (null != terminalId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<Preference>()
                         .Filter(t => t.Property(p => p.TerminalId).EqualTo(terminalId))
                         .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token,
                    out fault);
                if (null != fault)
                {
                    return preferences;
                }
                preferences = queryResult.Records.Cast<Preference>().ToList();
            }
            return preferences;
        }

        /// PREFERENCE Table queries
        /// <summary>
        ///  Get the preference by parameter.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="terminalId"></param>
        /// <param name="parameter"></param>
        /// <param name="fault"></param>
        /// <returns>the parameter value or null if not found</returns>
        public static string GetPreferenceByParameter(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string terminalId, string parameter,  out DataServiceFault fault)
        {
            fault = null;
            string parametervalue = null;

            if (null != parameter)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<Preference>()
                        .Filter(y => y.Property(x => x.TerminalId).EqualTo(terminalId)
                        .And().Property(x => x.Parameter).EqualTo(parameter))
                        .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return parametervalue;
                }
                var preference = (Preference)queryResult.Records.Cast<Preference>().FirstOrDefault();

                if (null != preference)
                {
                    parametervalue = preference.ParameterValue;
                }
            }
            return parametervalue;
        }
        /// REGIONMASTER Table queries
        /// <summary>
        ///  Get a a region master record for a given region.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="regionId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty TerminalMaster if terminalId is null or record does not exist for </returns>
        public static RegionMaster GetRegion(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string regionId, out DataServiceFault fault)
        {
            fault = null;

            var region = new RegionMaster();
            if (null != regionId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<RegionMaster>()
                    .Filter(y => y.Property(x => x.RegionId).EqualTo(regionId))
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return region;
                }
                region = (RegionMaster)queryResult.Records.Cast<RegionMaster>().FirstOrDefault();
            }
            return region;
        }
        /// PREFERENCE Table queries
        /// <summary>
        ///  Get the preferencse by terminal.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="terminalId"></param>
        /// <param name="fault"></param>
        /// <returns>>An empty list if terminalId is null or no entries are found</returns>
        public static List<Preference> GetPreferenceByTerminal(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string terminalId, out DataServiceFault fault)
        { 
            fault = null;
            var preferences = new List<Preference>();

            if (null != terminalId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<Preference>()
                        .Filter(y => y.Property(x => x.TerminalId).EqualTo(terminalId))
                        .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return preferences;
                }

                preferences = queryResult.Records.Cast<Preference>().ToList();
            }
            return preferences;
        }
        /// TERMINALCHANGE Table queries
        /// <summary>
        ///  Get a list of all terminal master updates after a given date time for a given region.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="dateTime"></param>
        /// <param name="regionId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if dateTime or regionId is null or no entries are found</returns>
        public static List<TerminalChange> GetTerminalChangesForRegion(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, DateTime? dateTime, string regionId, out DataServiceFault fault)
        {
            fault = null;
            var terminals = new List<TerminalChange>();
            if (null != dateTime && null != regionId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TerminalChange>()
                    .Filter(y => y.Property(x => x.ChgDateTime).GreaterThan(dateTime)
                    .And().Property(x => x.RegionId).EqualTo(regionId))
                    .OrderBy(x => x.TerminalId)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return terminals;
                }
                terminals = queryResult.Records.Cast<TerminalChange>().ToList();
            }
            return terminals;
        }

        /// TERMINALCHANGE Table queries
        /// <summary>
        ///  Get a list of all terminal master updates after a given date time for a given area.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="dateTime"></param>
        /// <param name="areaId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if dateTime or areaId is null or no entries are found</returns>
        public static List<TerminalChange> GetTerminalChangesForArea(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, DateTime? dateTime, string areaId, out DataServiceFault fault)
        {
            fault = null;
            var terminalsInArea = new List<string>();
            var terminals = new List<TerminalChange>();
            if (null != dateTime && null != areaId)
            {
                terminalsInArea = Common.GetTerminalsByArea
                    (dataService, settings, userCulture, userRoleIds, areaId, out fault);
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TerminalChange>()
                    .Filter(y => y.Property(x => x.ChgDateTime).GreaterThan(dateTime)
                    .And().Property(x => x.TerminalId).In(terminalsInArea.ToArray()))
                    .OrderBy(x => x.TerminalId)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return terminals;
                }
                terminals = queryResult.Records.Cast<TerminalChange>().ToList();
            }
            return terminals;
        }

        /// TERMINALMASTER Table queries
        /// <summary>
        ///  Get a a terminal master record for a given terminal.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="terminalId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty TerminalMaster if terminalId is null or record does not exist for </returns>
        public static TerminalMaster GetTerminal(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string terminalId, out DataServiceFault fault)
        {
            fault = null;

            var terminal = new TerminalMaster();
            if (null != terminalId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TerminalMaster>()
                    .Filter(y => y.Property(x => x.TerminalId).EqualTo(terminalId))
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return terminal;
                }
                terminal = (TerminalMaster)queryResult.Records.Cast<TerminalMaster>().FirstOrDefault();
            }
            return terminal;
        }
        /// TERMINALMASTER Table queries
        /// <summary>
        /// Gets a list of terminals by area
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="areaId"></param>
        /// <param name="fault"></param>
        /// <returns></returns>
        public static List<TerminalMaster> GetTerminalMastersForArea(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string areaId, out DataServiceFault fault)
        {
            fault = null;
            var terminalsInArea = new List<string>();
            var terminals = new List<TerminalMaster>();
            if (null != areaId)
            {
                terminalsInArea = Common.GetTerminalsByArea
                    (dataService, settings, userCulture, userRoleIds, areaId, out fault);
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TerminalMaster>()
                    .Filter(y => y.Property(x => x.TerminalId).In(terminalsInArea.ToArray()))
                    .OrderBy(x => x.TerminalId)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return terminals;
                }
                terminals = queryResult.Records.Cast<TerminalMaster>().ToList();
            }
            return terminals;
        }
        /// TERMINALMASTER Table queries
        /// <summary>
        /// Gets a list of terminals by region
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="regionId"></param>
        /// <param name="fault"></param>
        /// <returns></returns>
        public static List<TerminalMaster> GetTerminalMastersForRegion(IDataService dataService, ProcessChangeSetSettings settings,
            string userCulture, IEnumerable<long> userRoleIds, string regionId, out DataServiceFault fault)
        {
            fault = null;
            var terminals = new List<TerminalMaster>();
            if (null != regionId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TerminalMaster>()
                    .Filter(y => y.Property(x => x.Region).EqualTo(regionId))
                    .OrderBy(x => x.TerminalId)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return terminals;
                }
                terminals = queryResult.Records.Cast<TerminalMaster>().ToList();
            }
            return terminals;
        }
        /// TRIP Table  queries
        /// <summary>
        ///  Get a list of all trips for given driver and powerid.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="driverId"></param>
        /// <param name="powerId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if driverId is null or no entries are found </returns>
        public static List<Trip> GetTripsForDriverAndPowerId(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string driverId, string powerId, out DataServiceFault fault)
        {
            fault = null;
            var trips = new List<Trip>();
            if (null != driverId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<Trip>()
                    .Filter(y => y.Property(x => x.TripDriverId).EqualTo(driverId)
                    .And().Property(x => x.TripStatus).In(TripStatusConstants.Pending, TripStatusConstants.Missed)
                    .And().Property(x => x.TripAssignStatus).In(TripAssignStatusConstants.Dispatched, TripAssignStatusConstants.Acked)
                    .And().Property(x => x.TripSendFlag).In(TripSendFlagValue.Ready, TripSendFlagValue.SentToDriver)
                    .And().Property(x => x.TripPowerId).EqualTo(powerId))
                    .OrderBy(x => x.TripSequenceNumber)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return trips;
                }
                trips = queryResult.Records.Cast<Trip>().ToList();
            }
            return trips;
        }
        /// TRIP Table  queries
        /// <summary>
        ///  Get a list of all trips to be sent to a given driver at login time.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="driverId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if driverId is null or no entries are found </returns>
        public static List<Trip> GetTripsForDriverAtLogin(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string driverId, out DataServiceFault fault)
        {
            fault = null;
            var trips = new List<Trip>();
            if (null != driverId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<Trip>()
                    .Filter(y => y.Property(x => x.TripDriverId).EqualTo(driverId)
                    .And().Property(x => x.TripStatus).In(TripStatusConstants.Pending, TripStatusConstants.Missed)
                    .And().Property(x => x.TripAssignStatus).In(TripAssignStatusConstants.Dispatched, TripAssignStatusConstants.Acked)
                    .And().Property(x => x.TripSendFlag).In(TripSendFlagValue.Ready, TripSendFlagValue.SentToDriver))
                    .OrderBy(x => x.TripSequenceNumber)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return trips;
                }
                trips = queryResult.Records.Cast<Trip>().ToList();
            }
            return trips;
        }
        /// TRIP Table queries
        /// <summary>
        ///  Get a list of all trips to be sent to a given driver whenever a new trip is added or an existing trip is modified.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="driverId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if driverId is null or no entries are found</returns>
        public static List<Trip> GetTripsForDriver(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string driverId, out DataServiceFault fault)
        {
            fault = null;
            var trips = new List<Trip>();
            if (null != driverId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<Trip>()
                    .Filter(y => y.Property(x => x.TripDriverId).EqualTo(driverId)
                    .And().Property(x => x.TripStatus).In(TripStatusConstants.Pending, TripStatusConstants.Missed)
                    .And().Property(x => x.TripAssignStatus).In(TripAssignStatusConstants.Dispatched, TripAssignStatusConstants.Acked)
                    .And().Property(x => x.TripSendFlag).EqualTo(TripSendFlagValue.Ready))
                    .OrderBy(x => x.TripSequenceNumber)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return trips;
                }
                trips = queryResult.Records.Cast<Trip>().ToList();
            }
            return trips;
        }
        /// TRIP Table  queries
        /// <summary>
        ///  Get the next trip record for a given driver id.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="driverId"></param>
        /// <param name="tripNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty Trip record if tripNumber is null or no record is found</returns>
        public static Trip GetNextTripForDriver(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string driverId, string tripNumber,out DataServiceFault fault)
        {
            fault = null;
            var trip = new Trip();
            if (null != driverId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<Trip>()
                    .Filter(y => y.Property(x => x.TripDriverId).EqualTo(driverId)
                    .And().Property(x => x.TripStatus).In(TripStatusConstants.Pending, TripStatusConstants.Missed)
                    .And().Property(x => x.TripAssignStatus).In(TripAssignStatusConstants.Dispatched, TripAssignStatusConstants.Acked)
                    .And().Property(x => x.TripSendFlag).In(TripSendFlagValue.Ready, TripSendFlagValue.SentToDriver)
                    .And().Property(x => x.TripNumber).NotEqualTo(tripNumber))
                    .OrderBy(x => x.TripSequenceNumber)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return trip;
                }
                trip = queryResult.Records.Cast<Trip>().FirstOrDefault();
            }
            return trip;
        }
        /// TRIP Table  queries
        /// <summary>
        ///  Get the trip record for a given trip number.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty Trip record if tripNumber is null or no record is found</returns>
        public static Trip GetTrip(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string tripNumber, out DataServiceFault fault)
        {
            fault = null;
            var trip = new Trip();
            if (null != tripNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<Trip>()
                    .Filter(y => y.Property(x => x.TripNumber).EqualTo(tripNumber))
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return trip;
                }
                trip = queryResult.Records.Cast<Trip>().FirstOrDefault();
            }
            return trip;
        }
        /// <summary>
        /// Checks if the trip has already been completed.
        /// </summary>
        /// <param name="tripRecord"></param>
        /// <returns>true if trip is complete, false if not complete.</returns>
        public static bool IsTripComplete(Trip tripRecord)
        {
            bool response = false;
            if (null != tripRecord)
            {
                var statuses = new List<string> {
                            TripStatusConstants.Done,
                            TripStatusConstants.ErrorQueue,
                            TripStatusConstants.Review,
                            TripStatusConstants.Exception};
                if (statuses.Contains(tripRecord.TripStatus))
                    response = true;
            }
            return response;
        }

        /// TRIPREFERENCE Table queries
        /// <summary>
        ///  Get a list of trip reference numbers to be sent to a given driver for a given trip.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if tripNumber is null or no entries are found</returns>
        public static List<TripReferenceNumber> GetTripReferenceNumberForTrip(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string tripNumber, out DataServiceFault fault)
        {
            fault = null;
            var tripRefNums = new List<TripReferenceNumber>();
            if (null != tripNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TripReferenceNumber>()
                    .Filter(y => y.Property(x => x.TripNumber).EqualTo(tripNumber))
                    .OrderBy(x => x.TripRefNumber)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return tripRefNums;
                }
                tripRefNums = queryResult.Records.Cast<TripReferenceNumber>().ToList();
            }
            return tripRefNums;
        }
        /// TRIPSEGMENT Table  queries
        /// <summary>
        ///  Gets the trip segment record for a given trip and segment.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripNumber"></param>
        /// <param name="tripSegNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty TripSegment if tripNumber or tripSegNumber is null or no record is found</returns>
        public static TripSegment GetTripSegment(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string tripNumber, string tripSegNumber, out DataServiceFault fault)
        {
            fault = null;
            var tripSegment = new TripSegment();
            if (null != tripNumber && null != tripSegNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TripSegment>()
                    .Filter(y => y.Property(x => x.TripNumber).EqualTo(tripNumber)
                    .And().Property(x => x.TripSegNumber).EqualTo(tripSegNumber))
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return tripSegment;
                }
                tripSegment = queryResult.Records.Cast<TripSegment>().FirstOrDefault();
            }
            return tripSegment;
        }
        /// TRIPSEGMENT Table  queries
        /// <summary>
        ///  Gets the trip segment record for a given trip and segment.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripNumber"></param>
        /// <param name="tripSegNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty TripSegment if tripNumber or tripSegNumber is null or no record isfound</returns>
        public static TripSegment GetTripSegmentOpenEnded(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string tripNumber, string tripSegNumber, out DataServiceFault fault)
        {
            fault = null;
            var tripSegment = new TripSegment();
            if (null != tripNumber && null != tripSegNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TripSegment>()
                    .Filter(y => y.Property(x => x.TripNumber).EqualTo(tripNumber)
                    .And().Property(x => x.TripSegNumber).EqualTo(tripSegNumber)
                    .And().Property(p => p.TripSegOdometerStart).NotEqualTo(null)
                    .And().Property(p => p.TripSegOdometerEnd).EqualTo(null))
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return tripSegment;
                }
                tripSegment = queryResult.Records.Cast<TripSegment>().FirstOrDefault();
            }
            return tripSegment;
        }
        /// TRIPSEGMENT Table  queries
        /// <summary>
        ///  Gets the next trip segment for a given trip.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty TripSegment if tripNumber is null or no record is found</returns>
        public static TripSegment GetNextIncompleteTripSegment(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string tripNumber, out DataServiceFault fault)
        {
            fault = null;
            var tripSegment = new TripSegment();
            if (null != tripNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TripSegment>()
                    .Filter(y => y.Property(x => x.TripNumber).EqualTo(tripNumber)
                    .And().Property(x => x.TripSegStatus).In(TripSegStatusConstants.Pending, TripSegStatusConstants.Missed))
                    .OrderBy(x => x.TripSegNumber)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return tripSegment;
                }
                tripSegment = queryResult.Records.Cast<TripSegment>().FirstOrDefault(); 
            }
            return tripSegment;
        }
        /// TRIPSEGMENT queries
        /// <summary>
        ///  Get a list of all trip segments for a given trip.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripList"></param>
        /// <param name="powerId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if tripNumber is null or no entries are found</returns>
        public static List<TripSegment> GetTripSegmentsForDriverAndPowerId(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, List<Trip> tripList,string powerId, out DataServiceFault fault)
        {
            fault = null;
            var tripSegments = new List<TripSegment>();
            if (null != tripList)
            {
                foreach (var trip in tripList)
                {
                    Query query = new Query
                    {
                        CurrentQuery = new QueryBuilder<TripSegment>()
                        .Filter(y => y.Property(x => x.TripNumber).EqualTo(trip.TripNumber)
                        .And().Property(x => x.TripSegPowerId).EqualTo(powerId))
                        .OrderBy(x => x.TripNumber)
                        .OrderBy(x => x.TripSegNumber)
                        .GetQuery()
                    };
                    var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                    if (null != fault)
                    {
                        return tripSegments;
                    }
                    tripSegments.AddRange(queryResult.Records.Cast<TripSegment>().ToList());
                }
            }
            return tripSegments;
        }
        /// TRIPSEGMENT queries
        /// <summary>
        ///  Get a list of all trip segments for a given trip.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if tripNumber is null or no entries are found</returns>
        public static List<TripSegment> GetTripSegmentsForTrip(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string tripNumber, out DataServiceFault fault)
        {
            fault = null;
            var tripSegments = new List<TripSegment>();
            if (null != tripNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TripSegment>()
                    .Filter(y => y.Property(x => x.TripNumber).EqualTo(tripNumber))
                    .OrderBy(x => x.TripSegNumber)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return tripSegments;
                }
                tripSegments = queryResult.Records.Cast<TripSegment>().ToList();
            }
            return tripSegments;
        }
        /// TRIPSEGMENT queries
        /// <summary>
        ///  Get a list of trip segments to be sent to a given driver for a given trip.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if tripNumber is null or no entries are found</returns>
        public static List<TripSegment> GetTripSegmentsIncomplete(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string tripNumber, out DataServiceFault fault)
        {
            fault = null;
            var tripSegments = new List<TripSegment>();
            if (null != tripNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TripSegment>()
                    .Filter(y => y.Property(x => x.TripNumber).EqualTo(tripNumber)
                    .And().Property(x => x.TripSegStatus).In(TripSegStatusConstants.Pending, TripSegStatusConstants.Missed))
                    .OrderBy(x => x.TripSegNumber)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return tripSegments;
                }
                tripSegments = queryResult.Records.Cast<TripSegment>().ToList();
            }
            return tripSegments;
        }
        /// TRIPSEGMENT queries
        /// <summary>
        ///  Get a list of incomplete trip segments for the trips assigned to a driver.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="driverId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if tripNumber is null or no entries are found</returns>
        public static List<TripSegment> GetTripSegmentsIncompleteForDriver(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string driverId, out DataServiceFault fault)
        {
            fault = null;
            var tripSegments = new List<TripSegment>();
            if (null != driverId)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TripSegment>()
                    .Filter(y => y.Property(x => x.TripSegDriverId).EqualTo(driverId)
                    .And().Property(x => x.TripSegStatus).In(TripSegStatusConstants.Pending, TripSegStatusConstants.Missed))
                    .OrderBy(x => x.TripNumber)
                    .OrderBy(x => x.TripSegNumber)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return tripSegments;
                }
                tripSegments = queryResult.Records.Cast<TripSegment>().ToList();
            }
            return tripSegments;
        }

        /// TRIPSEGMENTCONTAINER queries
        /// <summary>
        ///  Get a list of trip containers  for a given trip.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if tripNumber is null or no entries are found</returns>
        public static List<TripSegmentContainer> GetTripContainersForTrip(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string tripNumber, out DataServiceFault fault)
        {
            fault = null;
            var tripContainers = new List<TripSegmentContainer>();
            if (null != tripNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TripSegmentContainer>()
                    .Filter(y => y.Property(x => x.TripNumber).EqualTo(tripNumber))
                    .OrderBy(x => x.TripSegNumber)
                    .OrderBy(x => x.TripSegContainerSeqNumber)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return tripContainers;
                }
                tripContainers = queryResult.Records.Cast<TripSegmentContainer>().ToList();
            }
            return tripContainers;
        }
        /// TRIPSEGMENTCONTAINER queries
        /// <summary>
        ///  Get a list of trip segment containers for a given trip and segment.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripNumber"></param>
        /// <param name="tripSegNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if tripNumber or tripSegNumber is null or no entries are found</returns>
        public static List<TripSegmentContainer> GetTripSegmentContainers(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string tripNumber, string tripSegNumber, out DataServiceFault fault)
        {
            fault = null;
            var tripSegmentContainers = new List<TripSegmentContainer>();
            if (null != tripNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TripSegmentContainer>()
                    .Filter(y => y.Property(x => x.TripNumber).EqualTo(tripNumber)
                    .And().Property(x => x.TripSegNumber).EqualTo(tripSegNumber))
                    .OrderBy(x => x.TripSegContainerSeqNumber)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return tripSegmentContainers;
                }
                tripSegmentContainers = queryResult.Records.Cast<TripSegmentContainer>().ToList();
            }
            return tripSegmentContainers;
        }
        /// TRIPSEGMENTCONTAINER queries
        /// <summary>
        ///  Get a list of incomplete trip segment containers for a given trip and segment.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripNumber"></param>
        /// <param name="tripSegNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if tripNumber or triSegNumber is null or no entries are found</returns>
        public static List<TripSegmentContainer> GetTripSegmentContainersIncomplete(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string tripNumber, string tripSegNumber, out DataServiceFault fault)
        {
            fault = null;
            var tripSegmentContainers = new List<TripSegmentContainer>();
            if (null != tripNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TripSegmentContainer>()
                    .Filter(y => y.Property(x => x.TripNumber).EqualTo(tripNumber)
                    .And().Property(x => x.TripSegNumber).EqualTo(tripSegNumber)
                    .And().Parenthesis(z => z.Property(x => x.TripSegContainerComplete)
                        .NotEqualTo(Constants.Yes).Or(x => x.TripSegContainerComplete).IsNull()))
                    .OrderBy(x => x.TripSegContainerSeqNumber)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return tripSegmentContainers;
                }
                tripSegmentContainers = queryResult.Records.Cast<TripSegmentContainer>().ToList();
            }
            return tripSegmentContainers;
        }
        /// TRIPSEGMENTCONTAINER Table  queries
        /// <summary>
        ///  Gets the trip segment container record for a given trip, segment, and container.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripNumber"></param>
        /// <param name="tripSegNumber"></param>
        /// <param name="tripSegContainerNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty TripSegmentContainer if tripNumber,tripSegNumber or tripSegContainer is null or no record is found</returns>
        public static TripSegmentContainer GetTripSegmentContainer(IDataService dataService, ProcessChangeSetSettings settings,
                      string userCulture, IEnumerable<long> userRoleIds, string tripNumber, string tripSegNumber, 
                      string tripSegContainerNumber, out DataServiceFault fault)
        {
            fault = null;
            var tripSegmentContainer = new TripSegmentContainer();
            if (null != tripNumber && null != tripSegNumber && null != tripSegContainerNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TripSegmentContainer>()
                    .Filter(y => y.Property(x => x.TripNumber).EqualTo(tripNumber)
                    .And().Property(x => x.TripSegNumber).EqualTo(tripSegNumber)
                    .And().Property(p => p.TripSegContainerNumber).EqualTo(tripSegContainerNumber))
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return tripSegmentContainer;
                }
                tripSegmentContainer = queryResult.Records.Cast<TripSegmentContainer>().FirstOrDefault();
            }
            return tripSegmentContainer;
        }
  
        /// TRIPSEGMENTCONTAINER Table  queries
        /// <summary>
        ///  Gets the last trip segment container record for a given trip and segment.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripNumber"></param>
        /// <param name="tripSegNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty TripSegmentContainer if tripNumber or tripSegNumber is null or no record is found</returns>
        public static TripSegmentContainer GetTripSegmentContainerLast(IDataService dataService, ProcessChangeSetSettings settings,
                      string userCulture, IEnumerable<long> userRoleIds, string tripNumber, string tripSegNumber, out DataServiceFault fault)
        {
            fault = null;
            var tripSegmentContainer = new TripSegmentContainer();
            if (null != tripNumber && null != tripSegNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TripSegmentContainer>()
                    .Filter(y => y.Property(x => x.TripNumber).EqualTo(tripNumber)
                    .And().Property(x => x.TripSegNumber).EqualTo(tripSegNumber))
                    .OrderBy(x => x.TripSegContainerSeqNumber,Direction.Descending)
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return tripSegmentContainer;
                }
                tripSegmentContainer = queryResult.Records.Cast<TripSegmentContainer>().FirstOrDefault();
            }
            return tripSegmentContainer;
        }
        /// TRIPSEGMENTMILEAGE Table  queries
        /// <summary>
        ///  Gets the trip segment mileage records for a given trip.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripList"></param>
        /// <param name="powerId"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if tripNumber or tripSegNumber is null or no entries are found</returns>
        public static List<TripSegmentMileage> GetTripSegmentMileageForDriverAndPowerId(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, List<Trip> tripList, string powerId, out DataServiceFault fault)
        {
            fault = null;
            var tripSegmentMileage = new List<TripSegmentMileage>();
            if (null != tripList)
            {
                foreach (var trip in tripList)
                {
                    Query query = new Query
                    {
                        CurrentQuery = new QueryBuilder<TripSegmentMileage>()
                        .Filter(t => t.Property(p => p.TripNumber).EqualTo(trip.TripNumber)
                        .And().Property(x => x.TripSegMileagePowerId).EqualTo(powerId))
                        .OrderBy(x => x.TripNumber)
                        .OrderBy(x => x.TripSegNumber)
                        .OrderBy(p => p.TripSegMileageSeqNumber)
                        .GetQuery()
                    };
                    var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                    if (null != fault)
                    {
                        return tripSegmentMileage;
                    }
                    tripSegmentMileage.AddRange(queryResult.Records.Cast<TripSegmentMileage>().ToList());
                }
            }
            return tripSegmentMileage;
        }
        /// TRIPSEGMENTMILEAGE Table  queries
        /// <summary>
        ///  Gets the trip segment mileage records for a given trip.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if tripNumber or tripSegNumber is null or no entries are found</returns>
        public static List<TripSegmentMileage> GetTripSegmentMileageForTrip(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string tripNumber, out DataServiceFault fault)
        {
            fault = null;
            var tripSegmentMileage = new List<TripSegmentMileage>();
            if (null != tripNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TripSegmentMileage>()
                        .Filter(t => t.Property(p => p.TripNumber).EqualTo(tripNumber))
                        .OrderBy(p => p.TripSegMileageSeqNumber)
                        .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return tripSegmentMileage;
                }
                tripSegmentMileage = queryResult.Records.Cast<TripSegmentMileage>().ToList();
            }
            return tripSegmentMileage;
        }
        /// TRIPSEGMENTMILEAGE Table  queries
        /// <summary>
        ///  Gets the last open-ended trip segment mileage record for a given trip and segment.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripNumber"></param>
        /// <param name="tripSegNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty TripSegment if tripNumber or tripsegnumber is null or no entries are found</returns>
        public static TripSegmentMileage GetTripSegmentMileageLast(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string tripNumber, string tripSegNumber, out DataServiceFault fault)
        {
            fault = null;
            var tripSegmentMileage = new TripSegmentMileage();
            if (null != tripNumber && null != tripSegNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TripSegmentMileage>()
                        .Filter(t => t.Property(p => p.TripNumber).EqualTo(tripNumber).And()
                        .Property(p => p.TripSegNumber).EqualTo(tripSegNumber).And()
                        .Property(p => p.TripSegMileageOdometerStart).NotEqualTo(null))
                        .OrderBy(p => p.TripSegMileageSeqNumber, Direction.Descending)
                        .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return tripSegmentMileage;
                }
                tripSegmentMileage = queryResult.Records.Cast<TripSegmentMileage>().FirstOrDefault();
            }
            return tripSegmentMileage;
        }
        /// TRIPSEGMENTMILEAGE Table  queries
        /// <summary>
        ///  Gets the trip segment mileage records for a given trip and segment.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripNumber"></param>
        /// <param name="tripSegNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty list if tripNumber or tripSegNumber is null or no entries are found</returns>
        public static List<TripSegmentMileage> GetTripSegmentMileage(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string tripNumber, string tripSegNumber, out DataServiceFault fault)
        {
            fault = null;
            var tripSegmentMileage = new List<TripSegmentMileage>();
            if (null != tripNumber && null != tripSegNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TripSegmentMileage>()
                        .Filter(t => t.Property(p => p.TripNumber).EqualTo(tripNumber).And()
                        .Property(p => p.TripSegNumber).EqualTo(tripSegNumber))
                        .OrderBy(p => p.TripSegMileageSeqNumber)
                        .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return tripSegmentMileage;
                }
                tripSegmentMileage = queryResult.Records.Cast<TripSegmentMileage>().ToList();
            }
            return tripSegmentMileage;
        }
        /// TRIPSEGMENTMILEAGE Table  queries
        /// <summary>
        ///  Gets the last open-ended trip segment mileage record for a given trip and segment.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripNumber"></param>
        /// <param name="tripSegNumber"></param>
        /// <param name="fault"></param>
        /// <returns>An empty TripSegment if tripNumber is null or no entries are found</returns>
        public static TripSegmentMileage GetTripSegmentMileageOpenEndedLast(IDataService dataService, ProcessChangeSetSettings settings,
              string userCulture, IEnumerable<long> userRoleIds, string tripNumber, string tripSegNumber, out DataServiceFault fault)
        {
            fault = null;
            var tripSegmentMileage = new TripSegmentMileage();
            if (null != tripNumber && null != tripSegNumber)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TripSegmentMileage>()
                        .Filter(t => t.Property(p => p.TripNumber).EqualTo(tripNumber).And()
                        .Property(p => p.TripSegNumber).EqualTo(tripSegNumber).And()
                        .Property(p => p.TripSegMileageOdometerStart).NotEqualTo(null).And()
                        .Property(p => p.TripSegMileageOdometerEnd).EqualTo(null))
                        .OrderBy(p => p.TripSegMileageSeqNumber, Direction.Descending)
                        .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return tripSegmentMileage;
                }
                tripSegmentMileage = queryResult.Records.Cast<TripSegmentMileage>().FirstOrDefault();
            }
            return tripSegmentMileage;
        }
        /// TRIPTYPEBASIC Table queries
        /// <summary>
        ///  Get a basic trip type record for a given basic trip type.
        ///  Caller needs to check if the fault is non-null before using the returned list.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripTypeCode"></param>
        /// <param name="fault"></param>
        /// <returns>A TripTypeBasic if powerId or regionId is null or record does not exist for powerId and regionId</returns>
        public static TripTypeBasic GetTripTypeBasic(IDataService dataService, ProcessChangeSetSettings settings,
             string userCulture, IEnumerable<long> userRoleIds, string tripTypeCode, out DataServiceFault fault)
        {
            fault = null;
            var tripTypeBasic = new TripTypeBasic();
            if (null != tripTypeCode)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TripTypeBasic>()
                    .Filter(y => y.Property(x => x.TripTypeCode).EqualTo(tripTypeCode))
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return tripTypeBasic;
                }
                tripTypeBasic = (TripTypeBasic)queryResult.Records.Cast<TripTypeBasic>().FirstOrDefault();
            }
            return tripTypeBasic;
        }

        /// <summary>
        /// Get a master desc trip type record for a given master trip type.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="settings"></param>
        /// <param name="userCulture"></param>
        /// <param name="userRoleIds"></param>
        /// <param name="tripTypeCode"></param>
        /// <param name="fault"></param>
        /// <returns></returns>
        public static TripTypeMasterDesc GetTripTypeMasterDesc(IDataService dataService, ProcessChangeSetSettings settings,
                    string userCulture, IEnumerable<long> userRoleIds, string tripTypeCode, out DataServiceFault fault)
        {
            fault = null;
            var tripTypeMasterDesc = new TripTypeMasterDesc();
            if (null != tripTypeCode)
            {
                Query query = new Query
                {
                    CurrentQuery = new QueryBuilder<TripTypeMasterDesc>()
                    .Filter(y => y.Property(x => x.TripTypeCode).EqualTo(tripTypeCode))
                    .GetQuery()
                };
                var queryResult = dataService.Query(query, settings.Username, userRoleIds, userCulture, settings.Token, out fault);
                if (null != fault)
                {
                    return tripTypeMasterDesc;
                }
                tripTypeMasterDesc = (TripTypeMasterDesc)queryResult.Records.Cast<TripTypeMasterDesc>().FirstOrDefault();
            }
            return tripTypeMasterDesc;
        }
    }
}

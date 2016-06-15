using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Get the relevant client terminal changes for the driver based on the last action date.    Call this process "withoutrequery".
    /// </summary>
    /// 
    /// Call this using the form PUT .../{dataServiceName}/{typeName}/{id}/withoutrequery
    /// cURL example: 
    ///     PUT https://maunb-stm10.bradyplc.com:7776//api/scraprunner/TerminalChangeProcess/001/withoutrequery
    /// Portable Client example:
    ///     var updateResult = client.UpdateAsync(itemToUpdate, requeryUpdated:false).Result;
    ///  
    [EditAction("TerminalChangeProcess")]
    public class TerminalChangeProcessRecordType : ChangeableRecordType
            <TerminalChangeProcess, string, TerminalChangeProcessValidator, TerminalChangeProcessDeletionValidator>
    {
        /// <summary>
        /// Mandatory implementation of virtual base class method.
        /// </summary>
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<TerminalChangeProcess, TerminalChangeProcess>();
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
            ChangeSet<string, TerminalChangeProcess> changeSet,
            bool persistChanges)
        {
            return ProcessChangeSet(dataService, changeSet, new ProcessChangeSetSettings(token, username, persistChanges));
        }

        /// <summary>
        /// This is the "real" method implementation.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="changeSet"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService,
            ChangeSet<string, TerminalChangeProcess> changeSet, ProcessChangeSetSettings settings)
        {

            ISession session = null;
            ITransaction transaction = null;

            // If session isn't passed in and changes are being persisted
            // then open a new session
            if (settings.Session == null && settings.PersistChanges)
            {
                var srRepository = (ISRRepository)repository;
                session = srRepository.OpenSession();
                transaction = session.BeginTransaction();
                settings.Session = session;
            }

            // Running the base process changeset first in this case.
            // This should gain the benefit of validators, auditing, security, piplines, etc.
            // However, it looks like we are losing some user input in the base.ProcessChangeSet
            // from a reretrieve or the sparse NHibernate mapping?
            ChangeSetResult<string> changeSetResult = base.ProcessChangeSet(dataService, changeSet, settings);

            // If no problems, we are free to process.
            // We only respond to one request at a time but in the more general cases we could be processing multiple records.
            // So we loop over the one to many keys in the changeSetResult.SuccessfullyUpdated
            if (!changeSetResult.FailedCreates.Any() && !changeSetResult.FailedUpdates.Any() && !changeSetResult.FailedDeletions.Any())
            {
                foreach (String key in changeSetResult.SuccessfullyUpdated)
                {
                    DataServiceFault fault;
                    string msgKey = key;
                    ChangeSetResult<string> scratchChangeSetResult;

                    TerminalChangeProcess terminalsProcess = (TerminalChangeProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    // TODO:  Determine userCulture and userRoleIds on a per user basis.
                    string userCulture = "en-GB";
                    IEnumerable<long> userRoleIds = Enumerable.Empty<long>().ToList();

                    // It appears, in the gernal case, I may need to backfill any additional user input values other than driverID.
                    // They will get clobbered by the call to the base process method.
                    TerminalChangeProcess backfillTerminalChangeProcess;
                    if (changeSet.Update.TryGetValue(key, out backfillTerminalChangeProcess))
                    {
                        // Generally use a mapper?  May not always be the best approach.
                        Mapper.Map(backfillTerminalChangeProcess, terminalsProcess);
                    }
                    else
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Unable to process terminalchange for Driver ID " + terminalsProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Validate driver id / Get the EmployeeMaster record
                    var employeeMaster = Common.GetEmployeeDriver(dataService, settings, userCulture, userRoleIds,
                                                  terminalsProcess.EmployeeId, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (employeeMaster == null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Invalid Driver ID " + terminalsProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    // Lookup Preference: DEFSendOnlyYardsForArea
                    string prefSendOnlyYardsForArea = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                                 employeeMaster.TerminalId, PrefDriverConstants.DEFSendOnlyYardsForArea, out fault);
                    if (fault != null)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }

                    //To contain the list of terminals
                    var terminalChangeList = new List<TerminalChange>();

                    ////////////////////////////////////////////////////////
                    // Lookup terminal changes or entire terminal master
                    if (terminalsProcess.LastTerminalChangeUpdate == null)
                    {
                        // If no date is provided, get the entire Terminal list from the Terminal Master and the details
                        // from the CustomerMaster instead of the TerminalChange table.
                        var terminalMasterList = new List<TerminalMaster>();
                        if (prefSendOnlyYardsForArea == Constants.Yes)
                        {
                            terminalMasterList = Common.GetTerminalMastersForArea(dataService, settings, userCulture, userRoleIds,
                                              employeeMaster.AreaId, out fault);
                        }
                        else
                        {
                            terminalMasterList = Common.GetTerminalMastersForRegion(dataService, settings, userCulture, userRoleIds,
                                              employeeMaster.RegionId, out fault);
                        }
                        if (fault != null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                        //Look up the customer details in the Customer Master
                        foreach (var terminal in terminalMasterList)
                        {
                            var terminalChange = new TerminalChange();
                            var terminalCustomerMaster = new CustomerMaster();

                            terminalChange.TerminalId = terminal.TerminalId;
                            terminalChange.RegionId = terminal.Region;

                            ////////////////////////////////////////////////
                            // Get the Customer record for the each terminal
                            terminalCustomerMaster = Common.GetCustomerForTerminal(dataService, settings, userCulture, userRoleIds,
                                                         terminal.TerminalId, out fault);
                            if (null != fault)
                            {
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                                break;
                            }
                            if (null == terminalCustomerMaster)
                            {
                                changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Segment Done:Invalid Terminal: "
                                                + terminal.TerminalId));
                                break;
                            }

                            terminalChange.CustType = terminalCustomerMaster.CustType;
                            terminalChange.CustHostCode = terminalCustomerMaster.CustHostCode;
                            terminalChange.CustCode4_4 = terminalCustomerMaster.CustCode4_4;
                            terminalChange.CustName = terminalCustomerMaster.CustName;
                            terminalChange.CustAddress1 = terminalCustomerMaster.CustAddress1;
                            terminalChange.CustAddress2 = terminalCustomerMaster.CustAddress2;
                            terminalChange.CustCity = terminalCustomerMaster.CustCity;
                            terminalChange.CustState = terminalCustomerMaster.CustState;
                            terminalChange.CustZip = terminalCustomerMaster.CustZip;
                            terminalChange.CustCountry = terminalCustomerMaster.CustCountry;
                            terminalChange.CustPhone1 = terminalCustomerMaster.CustPhone1;
                            terminalChange.CustContact1 = terminalCustomerMaster.CustContact1;
                            terminalChange.CustOpenTime = terminalCustomerMaster.CustOpenTime;
                            terminalChange.CustCloseTime = terminalCustomerMaster.CustCloseTime;
                            terminalChange.CustLatitude = terminalCustomerMaster.CustLatitude;
                            terminalChange.CustLongitude = terminalCustomerMaster.CustLongitude;
                            terminalChange.CustRadius = terminalCustomerMaster.CustRadius;
                            terminalChange.ChgDateTime = terminalCustomerMaster.ChgDateTime;
                            terminalChange.ChgActionFlag = TerminalChangeConstants.Add;
                            terminalChange.CustDriverInstructions = terminalCustomerMaster.CustDriverInstructions;

                            terminalChangeList.Add(terminalChange);
                        }//end of foreach (var terminal in terminalMasterList)

                    }//end of if (terminalsProcess.LastTerminalChangeUpdate == null)
                    else
                    {
                        //Since the date is provided, get the terminal changes since the last terminal update was provided to this driver.
                        if (prefSendOnlyYardsForArea == Constants.Yes)
                        {
                            terminalChangeList = Common.GetTerminalChangesForArea(dataService, settings, userCulture, userRoleIds,
                              terminalsProcess.LastTerminalChangeUpdate, employeeMaster.AreaId, out fault);
                        }
                        else
                        {
                            terminalChangeList = Common.GetTerminalChangesForRegion(dataService, settings, userCulture, userRoleIds,
                              terminalsProcess.LastTerminalChangeUpdate, employeeMaster.RegionId, out fault);
                        }
                        if (fault != null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                            break;
                        }
                    }//end of else if (terminalsProcess.LastTerminalChangeUpdate == null)

                    // Don't forget to actually backfill the TerminalProcess object contained within 
                    // the ChangeSetResult that exits this method and is returned to the caller.
                    terminalsProcess.Terminals = terminalChangeList;
                    //Set the return values

                    //Now using log4net.ILog implementation to test results of query.
                    log.Debug("SRTEST:TerminalChangeProcess");
                    foreach (var item in terminalChangeList)
                    {
                        log.DebugFormat("SRTEST:TerminalId:{0} RegionId:{1} Yard:{2} {3} ChgDateTime:{4} ChgActionFlag:{5}",
                                        item.TerminalId,
                                        item.RegionId,
                                        item.CustHostCode,
                                        item.CustName,
                                        item.ChgDateTime,
                                        item.ChgActionFlag);
                    }
                   ////////////////////////////////////////////////
                    //Now update the TerminalMasterDateTime in the DriverStatus record
                    //If there is no record yet, I guess we will have to add one.
                    var driverStatus = Common.GetDriverStatus(dataService, settings, userCulture, userRoleIds,
                                                terminalsProcess.EmployeeId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }

                    // If no driver status record is found create one to add
                    if (null == driverStatus)
                    {
                        driverStatus = new DriverStatus()
                        {
                            EmployeeId = terminalsProcess.EmployeeId,
                        };
                    }
                    //Now there is a driverStatus
                    //For testing
                    log.Debug("SRTEST:GetDriverStatus");
                    log.DebugFormat("SRTEST:DriverId:{0} LastTerminalMasterUpdate:{1}",
                                                        driverStatus.EmployeeId,
                                                        driverStatus.TerminalMasterDateTime);

                    driverStatus.TerminalMasterDateTime = DateTime.Now;

                    scratchChangeSetResult = Common.UpdateDriverStatus(dataService, settings, driverStatus);

                    log.DebugFormat("SRTEST:Saving DriverStatus Record: DriverId:{0} TerminalMasterUpdate:{1}",
                                                        driverStatus.EmployeeId,
                                                        driverStatus.TerminalMasterDateTime);

                    if (Common.LogChangeSetFailure(scratchChangeSetResult, driverStatus, log))
                    {
                        var s = string.Format("Could not update DriverStatus for Driver {0}.",
                                                terminalsProcess.EmployeeId);
                        scratchChangeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }
                }//end of foreach (String key in changeSetResult.SuccessfullyUpdated)
            }

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
            }
            else
            {
                transaction.Commit();
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


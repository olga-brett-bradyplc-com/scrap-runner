﻿using System;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class EmployeeMasterMetadata : TypeMetadataProvider<EmployeeMaster>
    {
        public EmployeeMasterMetadata()
        {

            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.EmployeeId)
                .IsId()
                .DisplayName("Employee Id");

            StringProperty(x => x.TerminalId);
            StringProperty(x => x.FirstName);
            StringProperty(x => x.LastName);
            StringProperty(x => x.Address);
            StringProperty(x => x.City);
            StringProperty(x => x.State);
            StringProperty(x => x.Zip);
            StringProperty(x => x.Phone1);
            StringProperty(x => x.Phone2);
            StringProperty(x => x.Password);
            StringProperty(x => x.FileMaintAccess);
            StringProperty(x => x.SecurityLevel);
            StringProperty(x => x.SupervisorId);
            StringProperty(x => x.NickName);
            StringProperty(x => x.EmployeeType);
            StringProperty(x => x.CompanyCode);
            StringProperty(x => x.EmployeeStatus);
            TimeProperty(x => x.LoginDateTime);
            TimeProperty(x => x.AccessDateTime);
            StringProperty(x => x.WorkArea);
            StringProperty(x => x.BillerInitials);
            IntegerProperty(x => x.NumTimesLogin);
            IntegerProperty(x => x.MaxLogins);
            StringProperty(x => x.RouterId);
            StringProperty(x => x.AreaId);
            StringProperty(x => x.RegionId);
            StringProperty(x => x.DefTerminalId);
            StringProperty(x => x.PrevEmployeeId);
            StringProperty(x => x.AllowMessaging);
            StringProperty(x => x.DefLabelPrinter);
            StringProperty(x => x.Country);
            StringProperty(x => x.AllowMapsAccess);
            StringProperty(x => x.DefReadyDateTomorrow);
            StringProperty(x => x.AudibleAlertNewMsg);
            StringProperty(x => x.MapId);
            StringProperty(x => x.ActionFlag);
            StringProperty(x => x.DefReadyDateMonday);
            StringProperty(x => x.DefStartAcctSearchBegName);
            StringProperty(x => x.AllowChangeContNumber);
            StringProperty(x => x.AllowModDoneTrips);
            StringProperty(x => x.AllowCancelDoneTrips);
            StringProperty(x => x.opt);
            IntegerProperty(x => x.SessionID);
            StringProperty(x => x.Router);
            StringProperty(x => x.DisplayReceiptNumber);
            StringProperty(x => x.DisplayScaleReferenceNumber);
            StringProperty(x => x.LoginID);
            StringProperty(x => x.LoginIDPrev);
            DateProperty(x => x.InactiveDate);

            ViewDefaults()
                .Property(x => x.EmployeeId)
                .Property(x => x.TerminalId)
                .Property(x => x.FirstName)
                .Property(x => x.LastName)
                .Property(x => x.Address)
                .Property(x => x.City)
                .Property(x => x.State)
                .Property(x => x.Zip)
                .Property(x => x.Phone1)
                .Property(x => x.Phone2)
                .Property(x => x.Password)
                .Property(x => x.FileMaintAccess)
                .Property(x => x.SecurityLevel)
                .Property(x => x.SupervisorId)
                .Property(x => x.NickName)
                .Property(x => x.EmployeeType)
                .Property(x => x.CompanyCode)
                .Property(x => x.EmployeeStatus)
                .Property(x => x.LoginDateTime)
                .Property(x => x.AccessDateTime)
                .Property(x => x.WorkArea)
                .Property(x => x.BillerInitials)
                .Property(x => x.NumTimesLogin)
                .Property(x => x.MaxLogins)
                .Property(x => x.RouterId)
                .Property(x => x.AreaId)
                .Property(x => x.RegionId)
                .Property(x => x.DefTerminalId)
                .Property(x => x.PrevEmployeeId)
                .Property(x => x.AllowMessaging)
                .Property(x => x.DefLabelPrinter)
                .Property(x => x.Country)
                .Property(x => x.AllowMapsAccess)
                .Property(x => x.DefReadyDateTomorrow)
                .Property(x => x.AudibleAlertNewMsg)
                .Property(x => x.MapId)
                .Property(x => x.ActionFlag)
                .Property(x => x.DefReadyDateMonday)
                .Property(x => x.DefStartAcctSearchBegName)
                .Property(x => x.AllowChangeContNumber)
                .Property(x => x.AllowModDoneTrips)
                .Property(x => x.AllowCancelDoneTrips)
                .Property(x => x.opt)
                .Property(x => x.SessionID)
                .Property(x => x.Router)
                .Property(x => x.DisplayReceiptNumber)
                .Property(x => x.DisplayScaleReferenceNumber)
                .Property(x => x.LoginID)
                .Property(x => x.LoginIDPrev)
                .Property(x => x.InactiveDate)
                .OrderBy(x => x.EmployeeId);
        }
    }
}

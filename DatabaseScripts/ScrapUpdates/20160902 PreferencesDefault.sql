--When set to Y, Users may set individual driver preferences which will be sent to the Android by services.
if not exists (select 1 from [Preferences] where parameter = 'DEFUseDriverPreferences')
INSERT INTO [Preferences] VALUES('0000','DEFUseDriverPreferences','Y','Use Individual Driver Preferences? (Y/N)')
else
update preferences set ParameterValue = 'Y' where parameter = 'DEFUseDriverPreferences'

IF  NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'PreferencesDefault' AND type = 'U')
CREATE TABLE [dbo].PreferencesDefault(
	[Parameter] [varchar](30) NOT NULL,
	[ParameterValue] [varchar](100) NULL,
	[Description] [varchar](100) NULL,
 	[PreferenceType] [char](2) NULL,
 	[PreferenceSeqNo] [smallint] NULL,
CONSTRAINT [PK_Parameter] PRIMARY KEY CLUSTERED 
(
	[Parameter] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO


if EXISTS (SELECT * FROM sys.tables WHERE name = N'EmployeePreferences' AND type = 'U')
if (select count(*) from EmployeePreferences) = 0
drop TABLE EmployeePreferences


IF  NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'EmployeePreferences' AND type = 'U')
CREATE TABLE [dbo].EmployeePreferences(
    [RegionId] [varchar](10) NOT NULL,
	[TerminalId] [varchar](10) NOT NULL,
	[EmployeeId] [varchar](10) NOT NULL,
	[Parameter] [varchar](30) NOT NULL,
	[ParameterValue] [varchar](100) NULL,
	[Description] [varchar](100) NULL,
 	[PreferenceType] [char](2) NULL,
 	[PreferenceSeqNo] [smallint] NULL,
CONSTRAINT [PK_EmployeePreferences] PRIMARY KEY CLUSTERED 
(
	[RegionId] ASC,
	[TerminalId] ASC,
	[EmployeeId] ASC,
	[Parameter] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

--Enroute
--Current Default value:Y
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFEnforceSeqProcess')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFEnforceSeqProcess', 'Y', 'Enforce sequential processing of trips (Y/N):', 'D', 1);

--Stop Preferences
--Current Default value:N
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFShowHostCode')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFShowHostCode', 'N', 'Show Account Host Code When Driver Weighs Gross? (Y/N):', 'D',10);

--Current Default value:5
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFMinAutoTriggerDone')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFMinAutoTriggerDone', '5', 'Minutes to wait before auto trigger Done on Stop Trans screen:', 'D',11);

--Current Default value:N
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFCommodSelection')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFCommodSelection', 'N', 'Driver selects commodity on pick up? (Y/N):', 'D',12);


--Yard Preferences
--Current Default value:N
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFAllowAddRT')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFAllowAddRT', 'Y', 'Allow Driver to Add a Return To Yard Segment? (Y/N):', 'D',20);

--Current Default value:N
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFAllowChangeRT')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFAllowChangeRT', 'Y', 'Allow Driver to Change a Yard? (Y/N):', 'D',21);

--Current Default value:Y
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFPromptRTMsg')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFPromptRTMsg', 'Y', 'Prompt Driver with Return to Yard Msg (Y/N):', 'D',22);

--Current Default value:N
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFDriverWeights')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFDriverWeights', 'N', 'Require Driver To Enter Weights? (Y/N):', 'D',23);

--Auto Delay
--Current Default value:Y
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFUseDrvAutoDelay')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFUseDrvAutoDelay', 'Y', 'Use Driver Auto Delay (Y/N):', 'D',24);

--Current Default value:30
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFDrvAutoDelayGross')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFDrvAutoDelayGross', '30', 'Arrive to gross weight auto delay minutes:', 'D',25);

--Current Default value:30
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFDrvAutoDelayTare')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFDrvAutoDelayTare', '30', 'Gross to tare weight auto delay minutes:', 'D',26);

--Container Related Preferences
--Current Default value:Y
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFOneContPerPowerUnit')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFOneContPerPowerUnit', 'Y', 'Restrict Driver to one container per power unit (Y/N):', 'D',30);

--Current Default value:N
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFPrevChangContID')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFPrevChangContID', 'N', 'Prevent Driver from Changing Container ID (Y/N):', 'D',31);

--Current Default value:2
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFContainerValidationCount')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFContainerValidationCount', '2', '# Times Driver Must Re-enter Container Number:', 'D',32);

--Current Default value:N
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFShowSimilarContainers')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFShowSimilarContainers', 'N', 'Show similar container numbers if match not found? (Y/N):', 'D',33);

--Current Default value:Y
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFUseContainerLevel')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFUseContainerLevel', 'Y', 'Require Driver To Enter Container Level? (Y/N):', 'D',34);

--Current Default value:N
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFAutoDropContainers')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFAutoDropContainers', 'N', 'Auto drop containers not used on Stop Trans screen? (Y/N):', 'D',35);

--Current Default value:N
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFConfirmTruckInvMsg')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFConfirmTruckInvMsg', 'N', 'Confirm Truck Inventory Message? (Y/N):', 'D',36);

--Current Default value:N
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFReqEntryContNumberForNB')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFReqEntryContNumberForNB', 'N', 'Require Driver to Enter Container # for NB# serialized labels? (Y/N):', 'D',37);

--Receipt Preferences
--Current Default value:N
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFDriverReceipt')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFDriverReceipt', 'Y', 'Require Driver to enter Receipt Number (Y/N):', 'D',40);

--Current Default value:N
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFDriverReceiptAllTrips')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFDriverReceiptAllTrips', 'N', 'Require Driver Receipt For All Trip Types? (Y/N):', 'D',41);

--Current Default value: null
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFDriverReceiptMask')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFDriverReceiptMask', NULL, 'Receipt Number Mask:', 'D',42);

--Current Default value:1
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFReceiptValidationCount')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFReceiptValidationCount', '1', '# Times Driver Must Re-enter Receipt Number:', 'D',43);


--Reference Number Preferences
--Current Default value:N
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFDriverReference')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFDriverReference', 'N', 'Require Driver to enter Scale Ref# at Gross? (Y/N):', 'D',50);

--Current Default value:N
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFReqScaleRefNo')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFReqScaleRefNo', 'N', 'Require Driver to enter Scale Ref# at Tare? (Y/N):', 'D',51);

--Current Default value: null
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFDriverReferenceMask')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFDriverReferenceMask', NULL, 'Scale Reference # Mask:', 'D',52);

--Current Default value:N
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFNotAvlScalRefNo')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFNotAvlScalRefNo', 'N', 'Add "Not Available" button to scale ref # screen? (Y/N):', 'D',53);

--Current Default value:1
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFReferenceValidationCount')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFReferenceValidationCount', '1', '# Times Driver Must Re-enter Scale Reference #:', 'D',54);



--Other Preferences
--Current Default value: USA
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFCountry')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFCountry', 'USA', 'Default Country Code:', 'D',60);


--Current Default value:N
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFEnableImageCapture')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFEnableImageCapture', 'N', 'Enable Image Capture? (Y/N):', 'D',63);


--The following preferences will be discontinued in the redevelopment project:
--DEFUseMaterGrading:Use Materials Grading Screen (Y/N) - Could be implemented later.
--DEFUseAutoArrive:Use Auto Arrive Feature (Y/N) - Always use auto arrive.
--DEFContMasterValidation:Container Master validation for manual entry (Y/N) - Always validate.
--DEFContMasterScannedVal:Container Master validation for scanned entry (Y/N) - Always validate.
--DEFDriverEntersGPS:Allow Driver to enter GPS (Y/N) - Always allow.
--DEFDriverAdd:Driver may add container on site (Y/N) - Not used.
--DEFDrvAutoDelayTime:Driver Auto Delay Time (minutes) - replaced with DEFDrvAutoDelayGross and DEFDrvAutoDelayTare
--DEFUseKM - Not used.
--DEFUseLitre - Not used.
/*
--Current Default value:N
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFUseKM')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFUseKM', 'N', 'Use Kilometres? (Y/N):', 'D',61);
--Current Default value:N
if not exists (select 1 from PreferencesDefault where Parameter = 'DEFUseLitre')
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFUseLitre', 'N', 'Use Litres? (Y/N):', 'D',62);--Current Default value:N
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFUseMaterGrading', 'N', 'Use Materials Grading Screen? (Y/N):', 'D');
--Current Default value:Y
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFUseAutoArrive', 'Y', 'Use Auto Arrive Feature (Y/N):', 'D');
--Current Default value:Y
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFContMasterValidate', 'Y', 'Container Master validation for manual entry (Y/N):', 'D');
--Current Default value:Y
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFContMasterScannedVal', 'Y', 'Container Master validation for scanned entry (Y/N):', 'D');
--Current Default value:Y
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFDriverEntersGPS', 'Y', 'Allow Driver to enter GPS (Y/N):', 'D');
--Current Default value:N
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFDriverAdd', 'N', 'Driver may add container on site (Y/N):', 'D');
--Current Default value:30
insert into PreferencesDefault (Parameter, ParameterValue, Description, PreferenceType, PreferenceSeqNo) values
('DEFDrvAutoDelayTime', '30', 'Driver Auto Delay Time (minutes):', 'D');
*/
--select * from PreferencesDefault order by PreferenceSeqNo

/*
select * from preferences where (Parameter = 'DEFAllowAddRT' OR Parameter = 'DEFAllowChangeRT' OR Parameter = 'DEFAutoDropContainers'
OR Parameter = 'DEFCommodSelection' OR Parameter = 'DEFConfirmTruckInvMsg' OR Parameter = 'DEFContainerValidationCount'
OR Parameter = 'DEFCountry' OR Parameter = 'DEFDriverReceipt' OR Parameter = 'DEFDriverReceiptAllTrips'
OR Parameter = 'DEFDriverReceiptMask' OR Parameter = 'DEFDriverReference' OR Parameter = 'DEFDriverReferenceMask'
OR Parameter = 'DEFDriverWeights' OR Parameter = 'DEFEnableImageCapture' OR Parameter = 'DEFEnforceSeqProcess'
OR Parameter = 'DEFMinAutoTriggerDone' OR Parameter = 'DEFNotAvlScalRefNo' OR Parameter = 'DEFOneContPerPowerUnit'
OR Parameter = 'DEFPrevChangContID' OR Parameter = 'DEFPromptRTMsg' OR Parameter = 'DEFReceiptValidationCount'
OR Parameter = 'DEFReferenceValidationCount' OR Parameter = 'DEFReqEntryContNumberForNB' OR Parameter = 'DEFReqScaleRefNo'
OR Parameter = 'DEFShowHostCode' OR Parameter = 'DEFShowSimilarContainers' OR Parameter = 'DEFUseContainerLevel'
OR Parameter = 'DEFUseDrvAutoDelay' OR Parameter = 'DEFUseKM' OR Parameter = 'DEFUseLitre'
OR Parameter = 'DEFDrvAutoDelayGross' OR Parameter = 'DEFDrvAutoDelayTare' ) 
*/
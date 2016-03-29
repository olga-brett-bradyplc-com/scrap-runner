﻿namespace Brady.ScrapRunner.Domain
{
    public static class Constants
    {
        public static readonly string ScrapRunner = "ScrapRunner";
  
        /// <summary>Yes: Y</summary>
        public static readonly string Yes = "Y";
        
        /// <summary>No: N</summary>
        public static readonly string No = "N";

        /// <summary>
        /// REASONCODES SR# is Scale Reference Number
        /// Useage: To prevent this reason code from being sent to the driver
        /// </summary>
        public static readonly string NOTAVLSCALREFNO = "SR#";

        /// <summary>
        /// SYSTEM_TERMINALID "0000" is the TerminalId for system preferences
        /// Useage: To retrieve system preferences
        /// </summary>
        public static readonly string SYSTEM_TERMINALID = "0000";
    }

    public static class BasicTripTypeConstants
    {
        public static readonly string Bobtail = "BT";
        //public static readonly string DeadRun    = "DD";
        public static readonly string DropEmpty = "DE";
        public static readonly string DropFull = "DF";
        public static readonly string Load = "LD";
        public static readonly string PickupEmpty = "PE";
        public static readonly string PickupFull = "PF";
        public static readonly string ReturnYardNC = "RN";
        public static readonly string Respot = "RS";
        public static readonly string ReturnYard = "RT";
        public static readonly string Scale = "SC";
        public static readonly string Unload = "UL";
        public static readonly string YardWork = "YD";
    }

    public static class CodeTableNameConstants
    {
        public static readonly string AccessorialCodes = "ACCESSORIALCODES";
        public static readonly string CannedMessages = "CANNEDMSG";
        public static readonly string ContainerGroup = "CONTAINERGROUP";
        public static readonly string ContainerLevel = "CONTAINERLEVEL";
        public static readonly string ContainerSize = "CONTAINERSIZE";
        public static readonly string ContainerStatus = "CONTAINERSTATUS";
        public static readonly string ContainerType = "CONTAINERTYPE";
        public static readonly string ContentStatus = "CONTENTSSTATUS";
        public static readonly string Control = "CONTROL";
        public static readonly string Countries = "COUNTRIES";
        public static readonly string CustomerType = "CUSTOMERTYPE";
        public static readonly string DelayCodes = "DELAYCODES";
        public static readonly string DriverStatus = "DRIVERSTATUS";
        public static readonly string ExceptionCodes = "EXCEPTIONCODES";
        public static readonly string ExceptionSubCodes = "EXCEPTIONSUBCODES";
        public static readonly string ExtensionCodes = "EXTENSIONCODES";
        public static readonly string PowerUnitStatus = "POWERUNITSTATUS";
        public static readonly string PowerUnitType = "POWERUNITTYPE";
        public static readonly string ReasonCodes = "REASONCODES";
        public static readonly string ReceiptComments = "RECEIPTCOMMENTS";
        public static readonly string RegularRuns = "REGULARRUNS";
        public static readonly string SecurityAccess = "SECURITYACCESS";
        public static readonly string StatesCanada = "STATESCAN";
        public static readonly string StatesMexico = "STATESMEX";
        public static readonly string StatesUSA = "STATESUSA";
        public static readonly string TripAssignStatus = "TRIPASSIGNSTATUS";
        public static readonly string TripSegStatus = "TRIPSEGSTATUS";
        public static readonly string TripStatus = "TRIPSTATUS";
        public static readonly string Version = "VERSION";
    }

    public static class CodeTableTypeConstants
    {
        public static readonly string System = "S";
        public static readonly string User = "U";
    }

    public static class ContainerContentsConstants
    {
        public static readonly string Empty = "E";
        public static readonly string Loaded = "L";
        public static readonly string Preload = "P";
        public static readonly string Unknown = "U";
    }

    public static class ContainerStatusConstants
    {
        public static readonly string Inbound = "I";
        public static readonly string Outbound = "O";
        public static readonly string Yard = "Y";
        public static readonly string Shop = "S";
        public static readonly string CustomerSite = "C";
        public static readonly string Unknown = "U";
        public static readonly string Scale = "W";
        public static readonly string SpecialProject = "P";
        public static readonly string Contractor = "T";
    }

    public static class CustomerTypeConstants
    {
        public static readonly string Supplier    = "S";
        public static readonly string Consumer    = "C";
        public static readonly string Yard        = "Y";
        public static readonly string Scale       = "W";
        public static readonly string RepairShop  = "R";
        public static readonly string Parent      = "P";
        public static readonly string Hauler      = "H";
        public static readonly string Landfill    = "L";
        public static readonly string XferStation = "T";
    }

    public static class DelayTypeConstants
    {
        public static readonly string Customer   = "C";
        public static readonly string Yard       = "Y";
        public static readonly string LunchBreak = "L";
    }

    public static class DriverStatusConstants
    {
        public static readonly string Done = "SD";
        public static readonly string Pending = "P";
        public static readonly string EnRoute = "EN";
        public static readonly string Arrive = "AR";
        public static readonly string Canceled = "XX";
        public static readonly string StateCrossing = "SC";
        public static readonly string Available = "V";
    }

    /// <summary>
    /// The Driver Status internal codes.  Used by the DriverStatus table.
    /// </summary>
    public static class DriverStatusSRConstants
    {
        /// <summary>Has trips, but is not logged in.  Ready: R</summary>
        public static readonly string Ready         = "R";
        /// <summary>LoggedIn: L</summary>
        public static readonly string LoggedIn      = "L";
        /// <summary>LoggedOut: O</summary>
        public static readonly string LoggedOut     = "O";
        /// <summary>EnRoute: E</summary>
        public static readonly string EnRoute       = "E";
        /// <summary>Arrive: A</summary>
        public static readonly string Arrive        = "A";
        /// <summary>Done: D</summary>
        public static readonly string Done          = "D";
        /// <summary>Delay: X</summary>
        public static readonly string Delay         = "X";
        /// <summary>BackOnDuty: B</summary>
        public static readonly string BackOnDuty    = "B";
        /// <summary>Still Logged in, but has no more trips.  Idle: Z</summary>
        public static readonly string Idle          = "Z"; 
        /// <summary>Still Logged in, has completed a trip, has more trips.  Available: V</summary>
        public static readonly string Available     = "V";
        /// <summary>StateCrossing: S</summary>
        public static readonly string StateCrossing = "S";
        /// <summary>Fuel: F</summary>
        public static readonly string Fuel          = "F";
        /// <summary>Connected: C</summary>
        public static readonly string Connected     = "C";
        /// <summary>Disconnected: K</summary>
        public static readonly string Disconnected  = "K";
        /// <summary>Dispatcher logged out.  LoggedOutDisp: T</summary>
        public static readonly string LoggedOutDisp = "T"; 
    }
    /// <summary>
    /// Driver Preference internal codes. From the Preferences table.
    /// </summary>
    public static class PrefDriverConstants
    {
        /// <summary>
        /// The following preferences will be discontinued in the redevelopment project:
        /// DEFUseMaterGrading:Use Materials Grading Screen (Y/N) - Could be implemented later.
        /// DEFUseAutoArrive:Use Auto Arrive Feature (Y/N) - Always use auto arrive.
        /// DEFContMasterValidation:Container Master validation for manual entry (Y/N) - Always validate.
        /// DEFContMasterScannedVal:Container Master validation for scanned entry (Y/N) - Always validate.
        /// DEFDriverEntersGPS:Allow Driver to enter GPS (Y/N) - Always allow.
        /// DEFDriverAdd:Driver may add container on site (Y/N) - Not used.
        /// </summary>

        /// TRIP-RELATED PREFERENCES
        /// <summary>
        /// DEFEnforceSeqProcess:
        /// Enforce sequential processing of trips (Y/N):
        /// When this option is set to “Y”, the driver cannot go out of the sequence displayed in the 
        /// Route Summary screen of the driver computer in completing the trips.
        /// </summary>
        public static readonly string DEFEnforceSeqProcess = "DEFEnforceSeqProcess";

        /// <summary>
        /// DEFMinAutoTriggerDone:
        /// Minutes to wait before auto trigger Done on Stop Trans screen:
        /// Enter the number of minutes driver can be on Stop Transactions screen before “Done” is 
        /// triggered automatically. If no value is entered, “Done” will not be automatically triggered. 
        /// </summary>
        public static readonly string DEFMinAutoTriggerDone = "DEFMinAutoTriggerDone";

        /// <summary>
        /// DEFUseDrvAutoDelay:
        /// Use Driver Auto Delay (Y/N):
        /// When this option is set to “Y”, the handheld will automatically display the “Delay” screen 
        /// when the driver has returned to the yard and no activity has occurred for x number of minutes. 
        /// </summary>
        public static readonly string DEFUseDrvAutoDelay = "DEFUseDrvAutoDelay";

        /// <summary>
        /// DEFDrvAutoDelayTime:
        /// Driver Auto Delay Time (minutes):
        /// The number of minutes after which the handheld will display the “Delay” screen if the 
        /// “Use Driver Auto Delay” option has been set to “Y”. 
        /// </summary>
        public static readonly string DEFDrvAutoDelayTime = "DEFDrvAutoDelayTime";

        /// CONTAINER-RELATED PREFERENCES
        /// <summary>
        /// DEFOneContPerPowerUnit:
        /// Restrict Driver to one container per power unit (Y/N):
        /// When this option is set to “Y”, only one container is permitted on any leg of a trip. 
        /// If a container is loaded, it must be dropped before another container can be loaded.
        /// </summary>
        public static readonly string DEFOneContPerPowerUnit = "DEFOneContPerPowerUnit";

        /// <summary>
        /// DEFConfirmTruckInvMsg:
        /// Confirm Truck Inventory Message?(Y/N):
        /// This preference can only be set to “Y” if ‘Restrict Driver to one container per power unit’ is set to “Y”. 
        /// When this option is set to “Y”, if a container is already loaded when the driver logs in, the following  
        /// message is displayed: Is container {xxxx} being used for the first trip? 
        /// </summary>
        public static readonly string DEFConfirmTruckInvMsg = "DEFConfirmTruckInvMsg";

        /// <summary>
        /// DEFPrevChangContID:
        /// Prevent Driver from Changing Container ID (Y/N):
        /// When this option is set to “Y”, the driver cannot load a container other than the one entered by 
        /// the dispatcher. If no container has been loaded or a container other than the one entered by the 
        /// dispatcher has been loaded and the driver attempts to go enroute, the enroute will be cancelled 
        /// and the driver will be instructed to load the container entered by the dispatcher. 
        /// If the dispatcher did not enter a container, the driver will still not be able to go enroute without 
        /// first loading a container. Though in that case any container can be loaded.
        /// This preference will cause the message: 
        /// "ENROUTE CANCELED You need to load the following container(s) to complete trip:xxxxxx Type, Size"
        /// to display on the handheld before the driver can go enroute on a drop type segment
        /// </summary>
        public static readonly string DEFPrevChangContID = "DEFPrevChangContID";

        /// <summary>
        /// DEFAutoDropContainers:
        /// Auto drop containers not used on Stop Trans screen? (Y/N):
        /// When this option is set to “Y”, any container in power unit inventory not used on 
        /// Stop Transactions screen will be dropped in the yard, empty. 
        /// If ‘Restrict Driver to one container per power unit’ is set to “Y”, this setting cannot be set to “Y”. 
        /// </summary>
        public static readonly string DEFAutoDropContainers = "DEFAutoDropContainers";

        /// <summary>
        /// DEFShowSimilarContainers:
        /// Show similar container numbers if match not found?(Y/N):
        /// When this option is set to “Y”, if a container match is not found for the value entered by the driver, 
        /// a list of similar containers is shown for the driver to choose from. 
        /// </summary>
        public static readonly string DEFShowSimilarContainers = "DEFShowSimilarContainers";

        /// <summary>
        /// DEFUseContainerLevel:
        /// Require Driver To Enter Container Level? (Y/N):
        /// When this option is set to "Y", the driver must select the container fill level. 
        /// The available fill levels are set on "4110 User Defined Code Table Maintenance", CONTAINERLEVEL
        /// </summary>
        public static readonly string DEFUseContainerLevel = "DEFUseContainerLevel";

        /// <summary>
        /// DEFContainerValidationCount:
        /// # Times Driver Must Re-enter Container Number:
        /// If a driver manually enters a container number, this sets the number of times the driver must 
        /// re-enter the container number to validate. Set to 0 to not require reentry. 
        /// </summary>
        public static readonly string DEFContainerValidationCount = "DEFContainerValidationCount";

        /// <summary>
        /// DEFReqEntryContNumberForNB:
        /// Require Driver to Enter Container # for NB# serialized labels?(Y/N):
        /// When this option is set to “Y”, when a driver uses a container with a serialized label and a 
        /// container type of NB#, he will be prompted to enter the correct container number, type and size. 
        /// This will update the container record and will not be required the next time the container is used.
        /// </summary>
        public static readonly string DEFReqEntryContNumberForNB = "DEFReqEntryContNumberForNB";

        /// COMMODITY-RELATED PREFERENCES
        /// <summary>
        /// DEFCommodSelection:
        /// Driver selects commodity on pick up (Y/N):
        /// When this option is set to “Y”, the driver can change the commodity on the 
        /// “Stop Transaction” screen to indicate which commodity was actually loaded or picked up. 
        /// </summary>
        public static readonly string DEFCommodSelection = "DEFCommodSelection";

        /// RECEIPT-RELATED PREFERENCES
        /// <summary>
        /// DEFDriverReceipt:
        /// Require Driver to enter Receipt Number (Y/N):
        /// When this option is set to “Y”, the driver must enter the driver receipt number, either by scanning it or 
        /// entering it manually. This is only required for accounts that have “Driver to Supply” checked on the 2030 
        /// Account Master Maintenance screen. It is for purchases unless “Require Driver Receipt For All Trips?” is 
        /// also set to Y
        /// </summary>
        public static readonly string DEFDriverReceipt = "DEFDriverReceipt";

        /// <summary>
        /// DEFDriverReceiptAllTrips:
        /// Require Driver Receipt For All Trip Types? (Y/N):
        /// Unless this is set to Y receipt entry is only required for trips where material is purchased. 
        /// </summary>
        public static readonly string DEFDriverReceiptAllTrips = "DEFDriverReceiptAllTrips";

        /// <summary>
        /// DEFDriverReceiptMask:
        /// Receipt Number Mask:
        /// The entered receipt number must match this mask. # = numbers @ = alpha characters & = alphanumeric characters
        /// Anything else is taken as a literal character. The input must match that character exactly. 
        /// For example: ####-@@@@& This means that the receipt number must have four numbers, followed by a “dash”, 
        /// followed by four alpha characters, followed by one alphanumeric character. 
        /// </summary>
        public static readonly string DEFDriverReceiptMask = "DEFDriverReceiptMask";

        /// <summary>
        /// DEFReceiptValidationCount:
        /// # Times Driver Must Re-enter Receipt Number:
        /// If a driver manually enters a receipt number, this sets the number of times the driver must 
        /// re-enter the receipt number to validate. Set to 0 to not require re-entry. 
        /// </summary>
        public static readonly string DEFReceiptValidationCount = "DEFReceiptValidationCount";

        /// SCALE REFERENCE NUMBER-RELATED PREFERENCES
        /// <summary>
        /// DEFDriverReference:
        /// Require Driver to enter Scale Ref # at Gross? (Y/N):
        /// When this option is set to "Y", the driver must enter a scale reference number when the gross weight is collected. 
        /// </summary>
        public static readonly string DEFDriverReference = "DEFDriverReference";

        /// <summary>
        /// DEFReqScaleRefNo:
        /// Require Driver to enter Scale Ref# at Tare? (Y/N):
        /// When this option is set to "Y", the driver must enter a scale reference number when the tare weight is collected. 
        /// </summary>
        public static readonly string DEFReqScaleRefNo = "DEFReqScaleRefNo";

        /// <summary>
        /// DEFDriverReferenceMask:
        /// Scale Reference Number mask:
        /// The entered scale reference number must match this mask. # = numbers @ = alpha characters & = alphanumeric 
        /// characters Anything else is taken as a literal character. The input must match that character exactly. 
        /// For example: ####-@@@@& This means that the receipt number must have four numbers, followed by a “dash”, 
        /// followed by four alpha characters, followed by one alphanumeric character.
        /// </summary>
        public static readonly string DEFDriverReferenceMask = "DEFDriverReferenceMask";

        /// <summary>
        /// DEFReferenceValidationCount:
        /// # Times Driver Must Re-enter scale reference Number:
        /// If a driver manually enters a scale reference number, this sets the number of times the driver must 
        /// re-enter the scale reference number to validate. Set to 0 to not require re-entry. 
        /// </summary>
        public static readonly string DEFReferenceValidationCount = "DEFReferenceValidationCount";

        /// <summary>
        /// DEFReqScaleRefNo:
        /// Add "Not Available" button to scale ref # screen?(Y/N):
        /// When this option is set to "Y", the driver can indicate a scale reference number was unavailable and continue 
        /// with the processing of the container. If set to "N", the driver will not be able to proceed without entering a 
        /// scale reference number.
        /// </summary>
        public static readonly string DEFNotAvlScalRefNo = "DEFNotAvlScalRefNo";

        /// <summary>
        /// DEFDriverWeights:
        /// Require Driver To Enter Weights? (Y/N):
        /// This causes an additional entry screen to pop up so the driver can enter scale weights. 
        /// </summary>
        public static readonly string DEFDriverWeights = "DEFDriverWeights";

        /// <summary>
        /// DEFShowHostCode:
        /// Show Account Host Code When Driver Weighs Gross? (Y/N):
        /// This displays the accounts host code in a pop up screen on the Yard/Scale function. 
        /// This is so the driver can give the host code to the scale operator in installations 
        /// where ScrapRunner is not integrated to a scale software system. 
        /// </summary>
        public static readonly string DEFShowHostCode = "DEFShowHostCode";

        /// RETURN TO YARD-RELATED PREFERENCES
        /// <summary>
        /// DEFAllowAddRT:
        /// Allow Driver to Add a Return To Yard Segment? (Y/N):
        /// When this option is set to "Y", the driver can add a Return to Yard segment if all of the containers on the 
        /// Stop Transactions screen have been processed, AND it's last segment of the trip, AND the last segment of the 
        /// trip is not a Return to Yard segment. 
        /// </summary>
        public static readonly string DEFAllowAddRT = "DEFAllowAddRT";

        /// <summary>
        /// DEFAllowAddRT:
        /// Allow Driver to Change a Yard? (Y/N):
        /// When this option is set to "Y", if the driver is on a Return to Yard segment, the yard can be changed. 
        /// </summary>
        public static readonly string DEFAllowChangeRT = "DEFAllowChangeRT";

        /// <summary>
        /// DEFPromptRTMsg:
        /// Prompt Driver with Return to Yard Msg? (Y/N):
        /// When this option is set to "Y", a pop-up will appear on the handheld asking if the driver wants to add 
        /// a Return to Yard segment if all of the containers on the Stop Transactions screen have been processed, 
        /// AND it's last segment of the trip, AND the last segment of the trip is not a Return to Yard segment.
        /// </summary>
        public static readonly string DEFPromptRTMsg = "DEFPromptRTMsg";

        /// MISC PREFERENCES
        /// <summary>
        /// DEFEnableImageCapture:
        /// Enable Image Capture?(Y/N):
        /// When this option is set to “Y”, driver can take a picture while on an active trip. 
        /// </summary>
        public static readonly string DEFEnableImageCapture = "DEFEnableImageCapture";

        /// <summary>
        /// DEFCountry:
        /// Default Country Code:
        /// May not be used.
        /// </summary>
        public static readonly string DEFCountry = "DEFCountry";

        /// <summary>
        /// DEFUseLitre:
        /// Use liters for display purposes(instead of gallons)? (Y/N):
        /// </summary>
        public static readonly string DEFUseLitre = "DEFUseLitre";

        /// <summary>
        /// DEFUseKM:
        /// Use kilometers for display purposes(instead of miles)? (Y/N):
        /// </summary>
        public static readonly string DEFUseKM = "DEFUseKM";

        //ADDITIONAL PREFERENCE FROM TERMINALMASTER
        /// <summary>
        /// TimeZoneFactor:
        /// Time Zone Factor: Difference between the terminal's time zone (for the driver) from the server time zone.
        /// Example: If server is in Eastern Time Zone and driver is based out of a yard in Pacific Time Zone, 
        /// the TimeZoneFactor is -3
        /// </summary>
        public static readonly string TimeZoneFactor = "TimeZoneFactor";

        /// <summary>
        /// DaylightSavings:
        /// Daylight Savings: Y= terminal is in a time zone that observes daylight savings time.
        /// </summary>
        public static readonly string DaylightSavings = "DaylightSavings";

        //OTHER DRIVER PREFERENCES
        //These driver preferences are not sent to the driver, but are used in determining other
        //types of information or validations sent to the driver
        /// <summary>
        /// DEFOdomWarnRange:
        /// Range to Send Odometer Warning: (default is 5 over or below)
        /// If the entered reading is more or less than xx miles/kilometers than the last recorded 
        /// reading, a warning message is displayed on the screen. 
        /// Message is "Warning! Please check odometer and log in again."
        /// </summary>
        public static readonly string DEFOdomWarnRange = "DEFOdomWarnRange";

        /// <summary>
        /// DEFPutTripInReviewNA:
        /// Put trip into review if driver is unable to enter scale ref#? (Y/N) 
        /// When this option is set to “Y”, if the driver is unable to enter the scale reference number 
        /// the trip will be put into review status. Dispatch will enter the scale reference number and 
        /// the status will be updated to completed. 
        /// </summary>
        public static readonly string DEFPutTripInReviewNA = "DEFPutTripInReviewNA";

        /// <summary>
        /// DEFSendOnlyYardsForArea:
        /// Send only yards for driver’s default area to driver? (Y/N) 
        /// When this option is set to “Y”, only yards in the driver’s default area are sent at log in. 
        /// This is used to populate the list of yards if ‘Allow Driver to Add a Return To Yard Segment’ is set to “Y”. 
        /// </summary>
        public static readonly string DEFSendOnlyYardsForArea = "DEFSendOnlyYardsForArea";

        /// <summary>
        /// DEFSendDispatchersForArea:
        /// For messaging, send dispatcher list only for driver’s default area? (Y/N)  
        /// When this option is set to “Y”, only dispatchers in the driver’s default area are sent at log in. 
        /// This is used to populate the list of dispatchers when driver goes to Messages, Send Messages.  
        /// </summary>
        public static readonly string DEFSendDispatchersForArea = "DEFSendDispatchersForArea";
        
    }
    /// <summary>
    /// Driver Preference internal codes. From the Preferences table.
    /// </summary>
    public static class PrefSystemConstants
    {
        /// <summary>
        /// DEFAllowAnyContainer:
        /// Send all containers to driver regardless of company? (Y/N)
        /// When this option is set to “Y”, all container updates will be sent to the driver. 
        /// Otherwise only send containers for the driver's region. 
        /// </summary>
        public static readonly string DEFAllowAnyContainer = "DEFAllowAnyContainer";
    }
    /// <summary>
    /// The Power Status internal codes.  Used by the PowerMaster table.
    /// </summary>
    public static class PowerStatusConstants
    {
        /// <summary>Available: A</summary>
        public static readonly string Available = "A";
        /// <summary>InUse: I</summary>
        public static readonly string InUse     = "I";
        /// <summary>Shop: S</summary>
        public static readonly string Shop      = "S";
        /// <summary>Unknown: U</summary>
        public static readonly string Unknown   = "U";
    }
    /// <summary>
    /// The Security Level internal codes.  Used by the SecurityMaster table.
    /// </summary>
    public static class SecurityLevelConstants
    {
        /// <summary>System Administrator: SA</summary>
        public static readonly string SysAdmin = "SA";
        /// <summary>General Office: GO</summary>
        public static readonly string GenOffice = "GO";
        /// <summary>Dispatcher: DI</summary>
        public static readonly string Dispatcher = "DI";
        /// <summary>Super Dispatcher: SD</summary>
        public static readonly string SuperDispatcher = "SD";
        /// <summary>CallTaker: CT</summary>
        public static readonly string CallTaker = "CT";
        /// <summary>View Only: VO</summary>
        public static readonly string ViewOnly = "VO";
        /// <summary>View & Print: VW</summary>
        public static readonly string ViewPrint = "VW";
        /// <summary>Driver: DR</summary>
        public static readonly string Driver = "DR";
        /// <summary>Container Inventory: CI</summary>
        public static readonly string ContainerInv = "CI";
        /// <summary>Yard Work: YW</summary>
        public static readonly string YardWork = "YW";
        /// <summary>Gate Check: GT</summary>
        public static readonly string GateCheck = "GT";
    }
    public static class TripAssignStatusConstants
    {
        public static readonly string NotDispatched = "N";
        public static readonly string Dispatched    = "D";
        public static readonly string Acked         = "A";
        public static readonly string Canceled      = "X";
    }
    
    //Changed to enum TripSendFlagValue
    //public static class TripSendFlagConstants
    //{
    //    public static readonly int NotReady        = 0;  //Not Ready to be sent to driver
    //    public static readonly int Ready           = 1;  //Ready to be sent to driver
    //    public static readonly int SentToDevice    = 2;  //Sent to driver
    //    public static readonly int NotSentToDevice = 3;  //Unable to be sent to driver
    //    public static readonly int CanceledReady   = 4;  //Canceled trip, ready to be sent to driver
    //    public static readonly int CanceledSent    = 5;  //Canceled trip, sent to driver
    //    public static readonly int TripInReview    = 7;  //Completed trip in review (set down or left on truck full)
    //    public static readonly int TripException   = 8;  //Completed trip as exception (unable to process)
    //    public static readonly int TripDone        = 9;  //Completed trip normal
    //    public static readonly int SentToHost      = 10; //Completed, sent to host accounting system
    //    public static readonly int SentToHostError = 11; //Completed, error in sending to host accounting system
    //    public static readonly int NotSentToHost   = 12; //Completed, not sent to host accounting system
    //}
    
    //public static class TripSendReseqFlagConstants
    //{
    //    public static readonly int NotReseq = 0;     //Not Sequenced
    //    public static readonly int AutoReseq = 1;    //Set when trip is entered or modified
    //    public static readonly int ManualReseq = 2;  //Set when trips are actually resequenced by dispatcher
    //    public static readonly int ReseqSent = 3;    //Set when the Reseq Message is sent
    //}

    public static class TripSegStatusConstants
    {
        public static readonly string Done = "D";
        public static readonly string Pending = "P";
        public static readonly string Canceled = "X";
        public static readonly string Missed = "M";
        public static readonly string Hold = "H";
        public static readonly string Future = "F";
        public static readonly string Review = "R";
        public static readonly string Exception = "E";
        public static readonly string ErrorQueue = "Q";
    }

    /// <summary>
    /// The internal status codes used by the ScrapRunner Trip table.
    /// </summary>
    public static class TripStatusConstants
    {
        /// <summary>Done: D</summary>
        public static readonly string Done = "D";
        /// <summary>Penmding: P</summary>
        public static readonly string Pending = "P";
        /// <summary>Canceled: X</summary>
        public static readonly string Canceled = "X";
        /// <summary>Missed: M</summary>
        public static readonly string Missed = "M";
        /// <summary>Hold: H</summary>
        public static readonly string Hold = "H";
        /// <summary>Future: F</summary>
        public static readonly string Future = "F";
        /// <summary>Review: R</summary>
        public static readonly string Review = "R";
        /// <summary>Exception: E</summary>
        public static readonly string Exception = "E";
        /// <summary>ErrorQueue: Q</summary>
        public static readonly string ErrorQueue = "Q";
    }
}

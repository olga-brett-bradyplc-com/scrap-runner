namespace Brady.ScrapRunner.Domain
{
    public static class Constants
    {
        public static readonly string ScrapRunner = "ScrapRunner";
        public static readonly string Yes = "Y";
        public static readonly string No = "N";
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
    public static class DriverStatusSRConstants
    {
        public static readonly string Ready         = "R"; //Has trips, but is not logged in
        public static readonly string LoggedIn      = "L";
        public static readonly string LoggedOut     = "O";
        public static readonly string EnRoute       = "E";
        public static readonly string Arrive        = "A";
        public static readonly string Done          = "D";
        public static readonly string Delay         = "X";
        public static readonly string BackOnDuty    = "B";
        public static readonly string Idle          = "Z"; //Still Logged in, but has no more trips
        public static readonly string Available     = "V"; //Still Logged in, has completed a trip, has more trips
        public static readonly string StateCrossing = "S";
        public static readonly string Fuel          = "F";
        public static readonly string Connected     = "C";
        public static readonly string Disconnected  = "K";
        public static readonly string LoggedOutDisp = "T"; //Dispatcher logged out
    }

    public static class PowerStatusConstants
    {
        public static readonly string Available = "A";
        public static readonly string InUse     = "I";
        public static readonly string Shop      = "S";
        public static readonly string Unknown   = "U";
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

    public static class TripStatusConstants
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
}

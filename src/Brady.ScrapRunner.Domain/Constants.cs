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
    public static class DelayTypesConstants
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
        public static readonly string Dispatched = "D";
        public static readonly string Acked = "A";
        public static readonly string Canceled = "X";
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

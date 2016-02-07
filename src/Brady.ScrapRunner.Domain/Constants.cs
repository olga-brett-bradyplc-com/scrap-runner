namespace Brady.ScrapRunner.Domain
{
    public static class Constants
    {
        public static readonly string ScrapRunner = "ScrapRunner";

    }

    public static class TripStatusConstants
    {
        public static readonly string Done = "D";
        public static readonly string Pending = "P";
        public static readonly string Canceled = "X";
        public static readonly string Missed = "M";
        public static readonly string Future = "F";
        public static readonly string Review = "R";
        public static readonly string Exception = "E";
        public static readonly string ErrorQueue = "Q";
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
}

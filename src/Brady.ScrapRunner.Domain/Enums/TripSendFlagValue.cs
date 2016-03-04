using BWF.DataServices.Metadata.Enums;
using BWF.Enums.Attributes;
using Newtonsoft.Json;

namespace Brady.ScrapRunner.Domain.Enums
{
    [JsonConverter(typeof(RichEnumConverter))]
    public enum TripSendFlagValue
    {
        /// <summary>
        /// 0 - Not Ready to be sent to driver.
        /// </summary>
        [RichEnum("Not Ready", "NotReady")]
        NotReady = 0,

        /// <summary>
        /// 1 - Ready to be sent to driver.
        /// </summary>
        [RichEnum("Ready", "Ready")]
        Ready = 1,

        /// <summary>
        /// 2 - Sent to driver.
        /// </summary> 
        [RichEnum("Sent To Driver", "SentToDriver")]
        SentToDriver = 2,

        /// <summary>
        /// 3 - Unable to be sent to driver
        /// </summary>
        [RichEnum("Not Sent To Driver", "NotSentToDriver")]
        NotSentToDriver = 3,

        /// <summary>
        /// 4 - Canceled trip, ready to be sent to driver
        /// </summary> 
        [RichEnum("Canceled Ready", "CanceledReady")]
        CanceledReady = 4,

        /// <summary>
        /// 5 - Canceled trip, sent to driver
        /// </summary>
        [RichEnum("Canceled Sent", "CanceledSent")]
        CanceledSent = 5,

        /// <summary>
        /// 7 - Completed trip in review (set down or left on truck full)
        /// </summary>
        [RichEnum("Trip In Review", "TripInReview")]
        TripInReview = 7,

        /// <summary>
        /// 8 - Completed trip as exception (unable to process)
        /// </summary>
        [RichEnum("Trip Exception", "TripException")]
        TripException = 8,

        /// <summary>
        /// 9 - Completed trip normal
        /// </summary>
        [RichEnum("Trip Done", "TripDone")]
        TripDone = 9,

        /// <summary>
        /// 10 - Completed, sent to host accounting system
        /// </summary>
        [RichEnum("Sent To Host", "SentToHost")]
        SentToHost = 10,

        /// <summary>
        /// 11 - Completed, error in sending to host accounting system
        /// </summary>
        [RichEnum("Sent To Host Error", "SentToHostError")]
        SentToHostError = 11,

        /// <summary>
        /// <12 - Completed, not sent to host accounting system
        /// </summary>
        [RichEnum("Not Sent To Host", "NotSentToHost")]
        NotSentToHost = 12
    }
}

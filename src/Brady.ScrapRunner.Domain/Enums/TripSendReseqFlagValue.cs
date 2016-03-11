using BWF.DataServices.Metadata.Enums;
using BWF.Enums.Attributes;
using Newtonsoft.Json;

namespace Brady.ScrapRunner.Domain.Enums
{
    [JsonConverter(typeof(RichEnumConverter))]
    public enum TripSendReseqFlagValue
    {
        /// <summary>
        /// 0 - Not Sequenced.
        /// </summary>
        [RichEnum("Not Sequenced", "NotSeq")]
        NotSeq = 0,

        /// <summary>
        /// 1 - Auto resequenced and ready to be sent to driver.
        ///     Set when trip is entered or modified.
        /// </summary>
        [RichEnum("Auto Resequence", "AutoReseq")]
        AutoReseq = 1,

        /// <summary>
        /// 2 - Manually resequenced and ready to be sent to driver.
        ///     Set when trips are actually resequenced by dispatcher.
        /// </summary> 
        [RichEnum("Manual Resequence", "ManualReseq")]
        ManualReseq = 2,

        /// <summary>
        /// 3 - Set when the Reseq Message is sent
        /// </summary>
        [RichEnum("Resequence Sent", "ReseqSent")]
        ReseqSent = 3
    }
}


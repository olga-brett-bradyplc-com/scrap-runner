using BWF.DataServices.Metadata.Enums;
using BWF.Enums.Attributes;
using Newtonsoft.Json;

namespace Brady.ScrapRunner.Domain.Enums
{
    [JsonConverter(typeof(RichEnumConverter))]
    public enum DriverForceLogoffValue
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
        /// 3 - Acknowledged by driver
        /// </summary>
        [RichEnum("Driver Ack", "DriverAck")]
        DriverAck = 3,
    }
}

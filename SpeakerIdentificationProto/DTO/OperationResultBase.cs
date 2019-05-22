using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpeakerIdentificationProto.DTO
{
    public class OperationResultBase
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public OperationStatus Status { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime LastActionDateTime { get; set; }
        public string Message { get; set; }
    }
}

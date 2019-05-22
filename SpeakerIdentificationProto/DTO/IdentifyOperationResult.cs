using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpeakerIdentificationProto.DTO
{
    public class IdentifyOperationResult : OperationResultBase
    {
        public IdentifyProcessingResult ProcessingResult { get; set; }
    }

    public class IdentifyProcessingResult
    {
        public Guid IdentifiedProfileId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Confidence Confidence { get; set; }
    }
}

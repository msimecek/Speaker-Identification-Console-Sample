using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpeakerIdentificationProto.DTO
{
    public class SpeakerProfile
    {
        public string IdentificationProfileId { get; set; }
        public string Locale { get; set; }
        public float EnrollmentSpeechTime { get; set; }
        public float RemainingEnrollmentSpeechTime { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime LastActionDateTime { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public EnrollmentStatus EnrollmentStatus { get; set; }
    }

}

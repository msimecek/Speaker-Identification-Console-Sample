using System;
using System.Collections.Generic;
using System.Text;

namespace SpeakerIdentificationProto.DTO
{
    public class ResultLocation
    {
        public Uri Uri { get; set; }

        public ResultLocation() { }

        public ResultLocation(string uri)
        {
            Uri = new Uri(uri);
        }
    }
}

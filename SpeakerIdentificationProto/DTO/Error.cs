using System;
using System.Collections.Generic;
using System.Text;

namespace SpeakerIdentificationProto.DTO
{
    public class ErrorResponse
    {
        public Error Error { get; set; }
    }

    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }

    public class SpeechErrorException : Exception
    {
        public string Code { get; set; }

        public SpeechErrorException(Error e): base(e.Message)
        {
            Code = e.Code;
        }

        public SpeechErrorException(string code, string message): base(message)
        {
            Code = code;
        }
    }
}

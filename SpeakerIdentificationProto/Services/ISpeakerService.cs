using SpeakerIdentificationProto.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SpeakerIdentificationProto.Services
{
    interface ISpeakerService
    {
        Task<Guid> CreateSpeakerProfileAsync(string locale, bool wait = true);

        Task<SpeakerProfile> GetSpeakerProfileAsync(Guid speakerProfileId);

        Task CreateEnrollmentAsync(Stream audioStream, Guid speakerProfileId, bool shortAudio = false);

        Task<ResultLocation> IdentifySpeakerAsync(Stream audioStream, Guid[] speakerProfileIds, bool shortAudio = false);

        Task<IdentifyOperationResult> IdentifySpeakerWaitForResultAsync(Stream audioStream, Guid[] speakerProfileIds, bool shortAudio = false);
    }
}

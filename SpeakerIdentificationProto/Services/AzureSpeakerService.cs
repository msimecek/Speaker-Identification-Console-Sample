using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpeakerIdentificationProto.DTO;

namespace SpeakerIdentificationProto.Services
{
    public class AzureSpeakerService
    {
        private HttpClient _hc;

        public AzureSpeakerService(string key, string location = "westus")
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("You have to provide the key parameter.");
            }

            if (string.IsNullOrEmpty(location))
            {
                throw new ArgumentException("You have to provide the location parameter.");
            }

            _hc = new HttpClient();
            _hc.BaseAddress = new Uri($"https://{location}.api.cognitive.microsoft.com/spid/v1.0/");
            _hc.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
        }

        public async Task<Guid> CreateSpeakerProfileAsync(string locale, bool wait = true)
        {
            var content = new StringContent("{'locale': '" + locale + "'}", Encoding.UTF8, "application/json");
            var resp = await _hc.PostAsync("identificationProfiles", content);
            var resBody = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                var response = JsonConvert.DeserializeObject<ErrorResponse>(resBody);
                throw new SpeechErrorException(response.Error);
            }
            
            var id = JsonConvert.DeserializeObject<CreateProfileResponse>(resBody).IdentificationProfileId;

            return id;    
        }

        public async Task<ResultLocation> CreateEnrollmentAsync(Stream audioStream, Guid speakerProfileId, bool shortAudio = false)
        {
            var content = new StreamContent(audioStream);
            var url = $"identificationProfiles/{speakerProfileId.ToString()}/enroll{(shortAudio ? "?shortAudio=true" : "")}";
            var resp = await _hc.PostAsync(url, content);

            if (!resp.IsSuccessStatusCode)
            {
                var response = JsonConvert.DeserializeObject<ErrorResponse>(await resp.Content.ReadAsStringAsync());
                throw new SpeechErrorException(response.Error);
            }

            return new ResultLocation(resp.Headers.GetValues("Operation-Location").FirstOrDefault());
        }

        public async Task<SpeakerProfile> CreateBatchEnrollmentAsync(Stream[] audioStreams, Guid speakerProfileId, bool shortAudio = false)
        {
            foreach (var stream in audioStreams)
            {

            }

            throw new NotImplementedException();
        }

        public async Task<EnrollmentOperationResult> CreateEnrollmentWaitForResultAsync(Stream audioStream, Guid speakerProfileId, bool shortAudio = false)
        {
            var resultLocation = await CreateEnrollmentAsync(audioStream, speakerProfileId, shortAudio);

            EnrollmentOperationResult result;
            do
            {
                result = await GetOperationStatusAsync<EnrollmentOperationResult>(resultLocation.Uri);
                //result = await GetSpeakerProfileAsync(speakerProfileId);
                if (result.Status == OperationStatus.Failed)
                {
                    throw new SpeechErrorException("EnrollmentError", result.Message);
                }
                await Task.Delay(2000);
            }
            while (result.Status != OperationStatus.Succeeded);

            return result;
        }

        public async Task<SpeakerProfile> GetSpeakerProfileAsync(Guid speakerProfileId)
        {
            var resp = await _hc.GetAsync($"identificationProfiles/{speakerProfileId.ToString()}");
            var content = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode && resp.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                throw new SpeechErrorException(JsonConvert.DeserializeObject<ErrorResponse>(content).Error);
            }

            return JsonConvert.DeserializeObject<SpeakerProfile>(content);
        }

        public async Task<IEnumerable<SpeakerProfile>> GetSpeakerProfilesAsync()
        {
            var resp = await _hc.GetAsync($"identificationProfiles/");
            var content = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode && resp.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                throw new SpeechErrorException(JsonConvert.DeserializeObject<ErrorResponse>(content).Error);
            }

            return JsonConvert.DeserializeObject<IEnumerable<SpeakerProfile>>(content);
        }

        public async Task<ResultLocation> IdentifySpeakerAsync(Stream audioStream, Guid[] speakerProfileIds, bool shortAudio = false)
        {
            var content = new StreamContent(audioStream);

            StringBuilder urlBuilder = new StringBuilder();
            urlBuilder.AppendJoin(',', speakerProfileIds);

            if (shortAudio)
            {
                urlBuilder.Append("&shortAudio=true");
            }

            var url = $"identify?identificationProfileIds={urlBuilder.ToString()}";

            var resp = await _hc.PostAsync(url, content);

            if (!resp.IsSuccessStatusCode && resp.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                throw new SpeechErrorException(new Error() { Code = "Undefined", Message = "The speech service didn't return a specific error." });
            }

            return new ResultLocation(resp.Headers.GetValues("Operation-Location").FirstOrDefault());
        }

        public async Task<IdentifyOperationResult> IdentifySpeakerWaitForResultAsync(Stream audioStream, Guid[] speakerProfileIds, bool shortAudio = false)
        {
            var resultLocation = await IdentifySpeakerAsync(audioStream, speakerProfileIds, shortAudio);

            IdentifyOperationResult opResult;
            do
            {
                opResult = await GetOperationStatusAsync<IdentifyOperationResult>(resultLocation.Uri);
                if (opResult.Status == OperationStatus.Failed)
                {
                    throw new SpeechErrorException(new Error() { Code = "IdentificationError", Message = opResult.Message });
                }
                await Task.Delay(2000);
            }
            while (opResult.Status != OperationStatus.Succeeded);

            return opResult;
        }

        public async Task<T> GetOperationStatusAsync<T>(Uri operationLocation)
        {
            var res = await _hc.GetAsync(operationLocation.ToString());

            return JsonConvert.DeserializeObject<T>(await res.Content.ReadAsStringAsync());
        }
    }
}

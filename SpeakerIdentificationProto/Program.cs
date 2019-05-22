using Microsoft.Extensions.Configuration;
using SpeakerIdentificationProto.DTO;
using SpeakerIdentificationProto.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SpeakerIdentificationProto
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Please specify mode with an argument (either 'enroll' or 'identify').");
                return;
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            var speakerService = new AzureSpeakerService(configuration["speakerServiceKey"]);

            // enrollment
            if (args[0].ToLowerInvariant() == "enroll")
            {
                var sourceDir = configuration["enrollmentFilesDir"];
                var files = Directory.EnumerateFiles(sourceDir);
                Guid profileId;

                if (args.Length > 1 && !string.IsNullOrEmpty(args[1]))
                {
                    profileId = Guid.Parse(args[1]);
                    Console.WriteLine($"Using speaker profile {profileId}.");
                }
                else
                {
                    try
                    {
                        Console.WriteLine("Creating speaker profile...");
                        profileId = await speakerService.CreateSpeakerProfileAsync(Locales.EN, false);
                        Console.WriteLine($"Created. Profile ID: {profileId}");
                    }
                    catch (SpeechErrorException ex)
                    {
                        Console.Error.WriteLine("Speech service returned an error: " + ex.Message);
                        return;
                    }
                }


                foreach (string file in files)
                {
                    var fileStream = File.OpenRead(file);

                    try
                    {
                        Console.WriteLine($"Enrolling file {file}...");
                        await speakerService.CreateEnrollmentWaitForResultAsync(fileStream, profileId);
                        Console.WriteLine("Enrolled.");
                    }
                    catch (SpeechErrorException ex)
                    {
                        Console.Error.WriteLine("Speech service returned an error: " + ex.Message);
                    }
                    finally
                    {
                        fileStream.Close();
                    }
                }

                EnrollmentStatus status;
                do
                {
                    status = (await speakerService.GetSpeakerProfileAsync(profileId)).EnrollmentStatus;
                    await Task.Delay(2000);
                }
                while (status != EnrollmentStatus.Enrolled);

                Console.WriteLine("Speaker profile ready.");
            }
            
            // identification - don't use
            if (args[0].ToLowerInvariant() == "identify")
            {
                Console.WriteLine("Warning! Not complete, don't use :)");

                if (args.Length < 2)
                {
                    Console.Error.WriteLine("You have to provide a list of candidate IDs.");
                    return;
                }

                string file = @"";
                var fileStream = File.OpenRead(file);

                //TODO: all of IDs, not just one
                var profile = await speakerService.IdentifySpeakerWaitForResultAsync(fileStream, new Guid[] { Guid.Parse(args[1]) }, true);

                Console.WriteLine("Profile: " + profile.ProcessingResult?.IdentifiedProfileId);
            }

            Console.ReadKey();
        }
    }
}

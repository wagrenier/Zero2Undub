using System;
using System.Diagnostics;
using System.IO;

namespace Zero2UndubProcess
{
    public static class ExternalProcesses
    {
        public static bool MfAudioCompress(Zero2File file, AudioFileInfo audioFile, int newFrequency, string directory)
        {
            if (audioFile.Channel > 2 || audioFile.Channel < 1)
            {
                return false;
            }
            
            var orig = $"{directory}/{file.FileId}.{file.FileExtension}".Replace("/", "\\");
            var dest = $"{directory}/{file.FileId}_compressed.{file.FileExtension}".Replace("/", "\\");
            
            var hasInterleave = audioFile.Channel > 1 ? $"/II{Convert.ToString(audioFile.Interleave, 16)}" : "";
            var hasInterleaveO = audioFile.Channel > 1 ? $"/OI{Convert.ToString(audioFile.Interleave, 16)}" : "";
            
            var args = $"/IF{audioFile.Frequency} /IC{audioFile.Channel} {hasInterleave} /IH{Convert.ToString(audioFile.Offset, 16)} /OTRAWC /OF{newFrequency} /OC{audioFile.Channel} {hasInterleaveO} \"{orig}\" \"{dest}\"";
            
            Console.WriteLine(args);
            
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "MFAudio.exe",
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    Arguments = args
                }
            };

            process.Start();

            if (!process.WaitForExit(10000))
            {
                try
                {
                    process.Kill(true);
                    Console.WriteLine($"Extracting audio file: {file.FileId} took too long, it was killed!");

                    return false;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }

            return true;
        }
        
        public static void PssSwitchAudio(int fileId, string folder)
        {
            folder = folder.Replace("/", "\\");
            PssDemux(fileId, "us", folder);
            PssDemux(fileId, "jp", folder);
            PssMux(fileId, folder);
        }

        public static void PssDemux(int fileId, string region, string folder)
        {
            var args =
                $"D /N \"{folder}\\{fileId}_{region}.PSS\" \"{folder}\\{fileId}_{region}.M2V\" \"{folder}\\{fileId}_{region}.WAV\"";

            var process = new Process
            {
                StartInfo =
                {
                    FileName = "PSS_Plex.exe",
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    Arguments = args
                }
            };

            process.Start();

            if (!process.WaitForExit(10000))
            {
                try
                {
                    process.Kill(true);
                    Console.WriteLine($"Extracting audio file: {fileId} took too long, it was killed!");
                }
                catch (InvalidOperationException)
                {
                }
            }
            
            File.Delete($"{folder}/{fileId}_{region}.PSS");
        }

        public static void PssMux(int fileId, string folder)
        {
            var args = $"M /N \"{folder}\\{fileId}_us.M2V\" \"{folder}\\{fileId}_jp.WAV\" \"{folder}\\{fileId}.PSS\"";

            var process = new Process
            {
                StartInfo =
                {
                    FileName = "PSS_Plex.exe",
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    Arguments = args
                }
            };

            process.Start();

            if (!process.WaitForExit(10000))
            {
                try
                {
                    process.Kill(true);
                    Console.WriteLine($"Extracting video file: {fileId} took too long, it was killed!");
                }
                catch (InvalidOperationException)
                {
                }
            }
                

            File.Delete($"{folder}/{fileId}_us.WAV");
            File.Delete($"{folder}/{fileId}_jp.WAV");
            File.Delete($"{folder}/{fileId}_us.M2V");
            File.Delete($"{folder}/{fileId}_jp.M2V");
        }
    }
}
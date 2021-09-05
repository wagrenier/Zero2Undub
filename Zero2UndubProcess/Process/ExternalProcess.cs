using System;
using System.IO;
using Zero2UndubProcess.Audio;
using Zero2UndubProcess.GameFiles;

namespace Zero2UndubProcess.Process
{
    public static class ExternalProcess
    {
        public static byte[] MfAudioCompress(ZeroFile file, AudioFileInfo audioFile, int newFrequency, string directory)
        {
            if (audioFile.Channel > 2 || audioFile.Channel < 1)
            {
                return null;
            }
            
            var orig = $"{directory}/{file.FileId}.str".Replace("/", "\\");
            var dest = $"{directory}/{file.FileId}_compressed.str".Replace("/", "\\");
            
            var hasInterleave = audioFile.Channel > 1 ? $"/II{Convert.ToString(audioFile.Interleave, 16)}" : "";
            var hasInterleaveO = audioFile.Channel > 1 ? $"/OI{Convert.ToString(audioFile.Interleave, 16)}" : "";
            
            var args = $"/IF{audioFile.Frequency} /IC{audioFile.Channel} {hasInterleave} /IH{Convert.ToString(audioFile.Offset, 16)} /OTRAWC /OF{newFrequency} /OC{audioFile.Channel} {hasInterleaveO} \"{orig}\" \"{dest}\"";

            var process = new System.Diagnostics.Process
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

                    return null;
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            }

            var fileContent = File.ReadAllBytes($"{directory}/{file.FileId}_compressed.str");
            File.Delete($"{directory}/{file.FileId}_compressed.str");

            return fileContent;
        }
    }
}
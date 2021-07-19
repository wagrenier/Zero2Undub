using System;
using System.Diagnostics;

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
    }
}
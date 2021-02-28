using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shared.Configs;
using Shared.Models;

namespace Shared.Archives.v2
{
    public static class SubmissionsArchive
    {
        public static async Task<byte[]> CreateAsync(List<Submission> submissions, IOptions<ApplicationConfig> config)
        {
            await using var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                foreach (var submission in submissions)
                {
                    var sourceFile = submission.User.ContestantId + '-' + submission.Id +
                                     submission.Program.GetSourceFileExtension();
                    var sourceEntry = archive.CreateEntry(sourceFile);
                    await using var sourceStream = sourceEntry.Open();
                    await sourceStream.WriteAsync(Encoding.UTF8.GetBytes(submission.GetInfoCommentsString(config)));
                    await sourceStream.WriteAsync(Convert.FromBase64String(submission.Program.Code));
                    sourceStream.Close();
                }
            }

            // ZipArchive must be disposed before getting bytes of zip file.
            // See https://stackoverflow.com/questions/40175391/invalid-zip-file-after-creating-it
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            return stream.ToArray();
        }
    }
}
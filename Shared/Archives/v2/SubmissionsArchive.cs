using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
                    var comments = submission.GetInfoCommentsString(config);
                    var destFile = submission.User.ContestantId + '-' + submission.Id +
                                   submission.Program.GetSourceFileExtension();
                    var destEntry = archive.CreateEntry(destFile);
                    await using var destStream = destEntry.Open();
                    if (submission.Program.Language != Language.LabArchive)
                    {
                        await destStream.WriteAsync(Encoding.UTF8.GetBytes(comments));
                        await destStream.WriteAsync(Convert.FromBase64String(submission.Program.Code));
                    }
                    else
                    {
                        var sourceFile = Encoding.UTF8.GetString(Convert.FromBase64String(submission.Program.Code));
                        var sourcePath =
                            Path.Combine(config.Value.DataPath, "submissions", submission.Id.ToString(), sourceFile);
                        await using var sourceStream = new FileStream(sourcePath, FileMode.Open);
                        await sourceStream.CopyToAsync(destStream);
                    }
                    destStream.Close();
                }

                var logEntry = archive.CreateEntry("export.log");
                await using var logStream = logEntry.Open();
                foreach (var comments in submissions.Select(submission => submission.GetInfoCommentsString(config)))
                {
                    await logStream.WriteAsync(Encoding.UTF8.GetBytes(comments));
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
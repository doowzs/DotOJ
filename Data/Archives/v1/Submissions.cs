using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Data.Configs;
using Data.Models;
using Microsoft.Extensions.Options;

namespace Data.Archives.v1
{
    public class SubmissionsArchive
    {
        public static async Task<byte[]> CreateAsync(List<Submission> submissions, IOptions<ApplicationConfig> options)
        {
            await using var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                foreach (var submission in submissions)
                {
                    var program = submission.Program;
                    var comment = program.GetSourceFileCommentSign();
                    var builder = new StringBuilder();

                    #region Information about submission

                    builder.AppendLine(comment + $"Submission  #{submission.Id}");
                    builder.AppendLine(comment + $"User ID:    {submission.UserId}");
                    if (submission.User is not null)
                    {
                        var user = submission.User;
                        builder.AppendLine(comment + $"Contestant: {user.ContestantId} ({user.ContestantName})");
                    }

                    builder.AppendLine(comment + $"Verdict:    {submission.Verdict.ToString()}");
                    builder.AppendLine(comment + $"Score:      {submission.Score ?? 0}");
                    builder.AppendLine(comment + $"Submitted:  {submission.CreatedAt} (UTC Time)");
                    if (submission.JudgedAt.HasValue) {
                        builder.AppendLine(comment + $"Judged:     {submission.JudgedAt} by {submission.JudgedBy}");
                    }
                    
                    builder.AppendLine();

                    #endregion
                    
                    var sourceFile = submission.Id + program.GetSourceFileExtension();
                    var sourceEntry = archive.CreateEntry(sourceFile);
                    await using var sourceStream = sourceEntry.Open();
                    await sourceStream.WriteAsync(Encoding.UTF8.GetBytes(builder.ToString()));
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
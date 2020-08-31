using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using IdentityModel;
using Judge1.Data;
using Judge1.Exceptions;
using Judge1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Judge1.Services.Admin
{
    public interface IAdminProblemService
    {
        public Task<PaginatedList<ProblemInfoDto>> GetPaginatedProblemInfosAsync(int? pageIndex);
        public Task<ProblemEditDto> GetProblemEditAsync(int id);
        public Task<ProblemEditDto> CreateProblemAsync(ProblemEditDto dto);
        public Task<ProblemEditDto> UpdateProblemAsync(int id, ProblemEditDto dto);
        public Task DeleteProblemAsync(int id);
        public Task<List<TestCase>> GetProblemTestCasesAsync(int id);
        public Task<List<TestCase>> UpdateProblemTestCasesAsync(int id, IFormFile file);
    }

    public class AdminProblemService : IAdminProblemService
    {
        private const int PageSize = 20;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _manager;
        private readonly IHttpContextAccessor _accessor;
        private readonly IOptions<JudgingConfig> _options;
        private readonly ILogger<AdminProblemService> _logger;

        public AdminProblemService(ApplicationDbContext context, UserManager<ApplicationUser> manager,
            IHttpContextAccessor accessor, IOptions<JudgingConfig> options, ILogger<AdminProblemService> logger)
        {
            _context = context;
            _manager = manager;
            _accessor = accessor;
            _options = options;
            _logger = logger;
        }

        private async Task EnsureProblemExists(int id)
        {
            if (!await _context.Problems.AnyAsync(p => p.Id == id))
            {
                throw new NotFoundException();
            }
        }

        private async Task ValidateProblemEditDtoAsync(ProblemEditDto dto)
        {
            var contest = await _context.Contests.FindAsync(dto.ContestId.GetValueOrDefault());
            if (contest == null)
            {
                throw new ValidationException("Invalid contest ID.");
            }

            if (string.IsNullOrEmpty(dto.Title))
            {
                throw new ValidationException("Invalid problem title.");
            }

            if (string.IsNullOrEmpty(dto.Description))
            {
                throw new ValidationException("Invalid problem description.");
            }

            if (string.IsNullOrEmpty(dto.InputFormat) || string.IsNullOrEmpty(dto.OutputFormat))
            {
                throw new ValidationException("Invalid problem input or output format.");
            }

            if (dto.HasSpecialJudge || dto.HasHacking)
            {
                throw new NotImplementedException("Hacking and Special Judge is not implemented.");
            }

            if (dto.SampleCases.Count == 0)
            {
                throw new ValidationException("At lease one sample case is required.");
            }
        }

        public async Task<PaginatedList<ProblemInfoDto>> GetPaginatedProblemInfosAsync(int? pageIndex)
        {
            return await _context.Problems.PaginateAsync(p => new ProblemInfoDto(p), pageIndex ?? 1, PageSize);
        }

        public async Task<ProblemEditDto> GetProblemEditAsync(int id)
        {
            await EnsureProblemExists(id);
            return new ProblemEditDto(await _context.Problems.FindAsync(id));
        }

        public async Task<ProblemEditDto> CreateProblemAsync(ProblemEditDto dto)
        {
            await ValidateProblemEditDtoAsync(dto);
            var problem = new Problem
            {
                ContestId = dto.ContestId.GetValueOrDefault(),
                Title = dto.Title,
                Description = dto.Description,
                InputFormat = dto.InputFormat,
                OutputFormat = dto.OutputFormat,
                FootNote = dto.FootNote,
                TimeLimit = dto.TimeLimit.GetValueOrDefault(),
                MemoryLimit = dto.MemoryLimit.GetValueOrDefault(),
                HasSpecialJudge = false,
                HasHacking = false,
                SampleCases = dto.SampleCases,
                TestCases = new List<TestCase>()
            };
            await _context.Problems.AddAsync(problem);
            await _context.SaveChangesAsync();
            return new ProblemEditDto(problem);
        }

        public async Task<ProblemEditDto> UpdateProblemAsync(int id, ProblemEditDto dto)
        {
            await EnsureProblemExists(id);
            await ValidateProblemEditDtoAsync(dto);
            var problem = await _context.Problems.FindAsync(id);
            problem.ContestId = dto.ContestId.GetValueOrDefault();
            problem.Title = dto.Title;
            problem.Description = dto.Description;
            problem.InputFormat = dto.InputFormat;
            problem.OutputFormat = dto.OutputFormat;
            problem.FootNote = dto.FootNote;
            problem.TimeLimit = dto.TimeLimit.GetValueOrDefault();
            problem.MemoryLimit = dto.MemoryLimit.GetValueOrDefault();
            problem.HasSpecialJudge = false;
            problem.HasHacking = false;
            problem.SampleCases = dto.SampleCases;
            _context.Problems.Update(problem);
            await _context.SaveChangesAsync();
            return new ProblemEditDto(problem);
        }

        public async Task DeleteProblemAsync(int id)
        {
            await EnsureProblemExists(id);
            var problem = new Problem {Id = id};
            _context.Problems.Attach(problem);
            _context.Problems.Remove(problem);
            await _context.SaveChangesAsync();
        }

        public async Task<List<TestCase>> GetProblemTestCasesAsync(int id)
        {
            await EnsureProblemExists(id);
            return (await _context.Problems.FindAsync(id)).TestCases;
        }

        public async Task<List<TestCase>> UpdateProblemTestCasesAsync(int id, IFormFile file)
        {
            await EnsureProblemExists(id);
            var testCases = new List<TestCase>();

            using (var stream = file.OpenReadStream())
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                var inputs = new HashSet<string>();
                var outputs = new HashSet<string>();
                var path = Path.Combine(_options.Value.DataPath, id.ToString());

                // Traverse all files in zip archive and get filenames.
                foreach (var entry in zip.Entries)
                {
                    var filename = Path.GetFileNameWithoutExtension(entry.FullName);
                    var extension = Path.GetExtension(entry.FullName);
                    if (extension.Equals(".in"))
                    {
                        inputs.Add(filename);
                    }
                    else if (extension.Equals(".out"))
                    {
                        outputs.Add(filename);
                    }
                }

                // Filter all valid test case files.
                foreach (var filename in inputs)
                {
                    if (outputs.Contains(filename))
                    {
                        testCases.Add(new TestCase
                        {
                            Input = filename + ".in",
                            Output = filename + ".out"
                        });
                    }
                    else
                    {
                        inputs.Remove(filename);
                    }
                }

                // Clear current test case folder.
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var dir = new DirectoryInfo(path);
                foreach (var f in dir.EnumerateFiles())
                {
                    f.Delete();
                }

                foreach (var d in dir.EnumerateDirectories())
                {
                    d.Delete(true);
                }

                // Move new test case files into test case folder.
                foreach (var entry in zip.Entries)
                {
                    var filename = Path.GetFileNameWithoutExtension(entry.FullName);
                    var extension = Path.GetExtension(entry.FullName);
                    if (inputs.Contains(filename) && (extension.Equals(".in") || extension.Equals(".out")))
                    {
                        var dest = Path.Combine(path, entry.FullName);
                        await using (var fs = new FileStream(dest, FileMode.Create))
                        {
                            await entry.Open().CopyToAsync(fs);
                        }
                    }
                }
            }

            var problem = await _context.Problems.FindAsync(id);
            problem.TestCases = testCases;
            await _context.SaveChangesAsync();

            return testCases;
        }
    }
}
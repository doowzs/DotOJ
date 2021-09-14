using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Shared.Configs;
using Shared.DTOs;
using Shared.Generics;
using Shared.Models;
using Shared.RabbitMQ;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Server.Exceptions;
using Server.RabbitMQ;
using Server.Services.Singleton;
using Shared.Archives.v2.Problems;

namespace Server.Services.Admin
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
        public Task<ProblemEditDto> ImportProblemAsync(int contestId, IFormFile file);
        public Task<byte[]> ExportProblemAsync(int id);
        public Task<byte[]> ExportProblemSubmissionsAsync(int id, bool all);
        public Task<List<PlagiarismInfoDto>> GetProblemPlagiarismInfosAsync(int id);
        public Task<PlagiarismInfoDto> CheckProblemPlagiarismAsync(int id);
    }

    public class AdminProblemService : LoggableService<AdminProblemService>, IAdminProblemService
    {
        private const int PageSize = 20;
        private readonly IOptions<ApplicationConfig> _options;
        private readonly BackgroundTaskQueue<JobRequestMessage> _queue;
        private readonly ProblemStatisticsService _statistics;

        public AdminProblemService(IServiceProvider provider) : base(provider)
        {
            _options = provider.GetRequiredService<IOptions<ApplicationConfig>>();
            _queue = provider.GetRequiredService<BackgroundTaskQueue<JobRequestMessage>>();
            _statistics = provider.GetRequiredService<ProblemStatisticsService>();
        }

        private async Task EnsureProblemExists(int id)
        {
            if (!await Context.Problems.AnyAsync(p => p.Id == id))
            {
                throw new NotFoundException();
            }
        }

        private async Task ValidateProblemEditDtoAsync(ProblemEditDto dto)
        {
            var contest = await Context.Contests.FindAsync(dto.ContestId.GetValueOrDefault());
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

            if (dto.Type == ProblemType.Ordinary)
            {
                if (string.IsNullOrEmpty(dto.InputFormat) || string.IsNullOrEmpty(dto.OutputFormat))
                {
                    throw new ValidationException("Invalid problem input or output format.");
                }

                if (!dto.TimeLimit.HasValue)
                {
                    throw new ValidationException("Time limit cannot be null.");
                }

                if (!dto.MemoryLimit.HasValue)
                {
                    throw new ValidationException("Memory limit cannot be null.");
                }

                if (dto.HasSpecialJudge && dto.SpecialJudgeProgram == null)
                {
                    throw new ValidationException("Special judge program cannot be null.");
                }

                if (dto.HasHacking)
                {
                    throw new NotImplementedException("Hacking and Special Judge is not implemented.");
                }

                if (dto.SampleCases.Count == 0)
                {
                    throw new ValidationException("At lease one sample case is required.");
                }
            }
        }

        public async Task<PaginatedList<ProblemInfoDto>> GetPaginatedProblemInfosAsync(int? pageIndex)
        {
            var paginated = await Context.Problems
                .OrderByDescending(p => p.Id)
                .PaginateAsync(pageIndex ?? 1, PageSize);
            List<ProblemInfoDto> problemInfos = new();
            foreach (var problem in paginated.Items)
            {
                var statistics = await _statistics.GetStatisticsAsync(problem.Id);
                problemInfos.Add(new ProblemInfoDto(problem, false, false, false, statistics));
            }
            return new PaginatedList<ProblemInfoDto>
                (paginated.TotalItems, paginated.PageIndex, paginated.PageSize, problemInfos);
        }

        public async Task<ProblemEditDto> GetProblemEditAsync(int id)
        {
            await EnsureProblemExists(id);
            return new ProblemEditDto(await Context.Problems.FindAsync(id));
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
                HasSpecialJudge = dto.HasSpecialJudge,
                SpecialJudgeProgram = dto.HasSpecialJudge ? dto.SpecialJudgeProgram : null,
                HasHacking = false,
                SampleCases = dto.SampleCases,
                TestCases = new List<TestCase>()
            };
            await Context.Problems.AddAsync(problem);
            await Context.SaveChangesAsync();

            await LogInformation($"CreateProblem Id={problem.Id} Contest={problem.ContestId} Title={problem.Id} " +
                                 $"HasSpecialJudge={problem.HasSpecialJudge} HasHacking={problem.HasHacking}");
            return new ProblemEditDto(problem);
        }

        public async Task<ProblemEditDto> UpdateProblemAsync(int id, ProblemEditDto dto)
        {
            await EnsureProblemExists(id);
            await ValidateProblemEditDtoAsync(dto);
            var problem = await Context.Problems.FindAsync(id);
            problem.ContestId = dto.ContestId.GetValueOrDefault();
            problem.Title = dto.Title;
            problem.Type = dto.Type;
            problem.Description = dto.Description;
            problem.InputFormat = dto.InputFormat;
            problem.OutputFormat = dto.OutputFormat;
            problem.FootNote = dto.FootNote;
            problem.TimeLimit = dto.TimeLimit.GetValueOrDefault();
            problem.MemoryLimit = dto.MemoryLimit.GetValueOrDefault();
            problem.HasSpecialJudge = dto.HasSpecialJudge;
            problem.SpecialJudgeProgram = dto.HasSpecialJudge ? dto.SpecialJudgeProgram : null;
            problem.HasHacking = false;
            problem.SampleCases = dto.SampleCases;
            Context.Problems.Update(problem);
            await Context.SaveChangesAsync();

            await _statistics.InvalidStatisticsAsync(problem.Id);
            await LogInformation($"UpdateProblem Id={problem.Id} Contest={problem.ContestId} Title={problem.Id} " +
                                 $"HasSpecialJudge={problem.HasSpecialJudge} HasHacking={problem.HasHacking}");
            return new ProblemEditDto(problem);
        }

        public async Task DeleteProblemAsync(int id)
        {
            await EnsureProblemExists(id);
            var problem = new Problem {Id = id};
            Context.Problems.Attach(problem);
            Context.Problems.Remove(problem);
            await Context.SaveChangesAsync();
            await _statistics.InvalidStatisticsAsync(problem.Id);
            await LogInformation($"DeleteProblem Id={problem.Id}");
        }

        public async Task<List<TestCase>> GetProblemTestCasesAsync(int id)
        {
            await EnsureProblemExists(id);
            return (await Context.Problems.FindAsync(id)).TestCases;
        }

        public async Task<List<TestCase>> UpdateProblemTestCasesAsync(int id, IFormFile file)
        {
            await EnsureProblemExists(id);

            var problem = await Context.Problems.FindAsync(id);
            switch (problem.Type)
            {
                case ProblemType.Ordinary:
                    await OrdinaryProblemArchive.ExtractTestCasesAsync(problem, file, _options, prefix: "");
                    break;
                case ProblemType.TestKitLab:
                    await TestKitLabProblemArchive.ExtractTestKitAsync(problem, file, _options, prefix: "");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            await Context.SaveChangesAsync();

            await LogInformation($"UpdateProblemTestCases Id={problem.Id}" +
                                 $" Type={problem.Type} Count={problem.TestCases.Count}");
            return problem.TestCases;
        }

        public async Task<ProblemEditDto> ImportProblemAsync(int contestId, IFormFile file)
        {
            if (!await Context.Contests.AnyAsync(c => c.Id == contestId))
            {
                throw new ValidationException("Invalid Contest ID.");
            }

            Problem problem;
            var type = await ProblemConfig.PeekProblemTypeAsync(file);
            switch (type)
            {
                case ProblemType.Ordinary:
                    problem = await OrdinaryProblemArchive.ParseAsync(contestId, file, _options);
                    await Context.Problems.AddAsync(problem);
                    await Context.SaveChangesAsync();
                    await OrdinaryProblemArchive.ExtractTestCasesAsync(problem, file, _options);
                    await Context.SaveChangesAsync();
                    break;
                case ProblemType.TestKitLab:
                    problem = await TestKitLabProblemArchive.ParseAsync(contestId, file, _options);
                    await Context.Problems.AddAsync(problem);
                    await Context.SaveChangesAsync();
                    await TestKitLabProblemArchive.ExtractTestKitAsync(problem, file, _options);
                    await Context.SaveChangesAsync();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return new ProblemEditDto(problem);
        }

        public async Task<byte[]> ExportProblemAsync(int id)
        {
            await EnsureProblemExists(id);
            var problem = await Context.Problems.FindAsync(id);
            return problem.Type switch
            {
                ProblemType.Ordinary
                    => await OrdinaryProblemArchive.CreateAsync(problem, _options),
                ProblemType.TestKitLab
                    => await TestKitLabProblemArchive.CreateAsync(problem, _options),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public async Task<byte[]> ExportProblemSubmissionsAsync(int id, bool all)
        {
            await EnsureProblemExists(id);
            List<Submission> submissions;
            if (all)
            {
                submissions = await Context.Submissions
                    .Where(s => s.ProblemId == id)
                    .Include(s => s.User)
                    .ToListAsync();
            }
            else
            {
                var submissionIds = await Context.Submissions
                    .Where(s => s.ProblemId == id && s.Verdict == Verdict.Accepted)
                    .GroupBy(s => s.UserId)
                    .Select(g => g.Min(s => s.Id))
                    .ToListAsync();
                submissions = await Context.Submissions
                    .Where(s => submissionIds.Contains(s.Id))
                    .Include(s => s.User)
                    .ToListAsync();
            }
            return await Shared.Archives.v2.SubmissionsArchive.CreateAsync(submissions, _options);
        }

        public async Task<List<PlagiarismInfoDto>> GetProblemPlagiarismInfosAsync(int id)
        {
            await EnsureProblemExists(id);
            return await Context.Plagiarisms
                .Where(p => p.ProblemId == id && !p.Outdated)
                .OrderByDescending(p => p.Id)
                .Select(p => new PlagiarismInfoDto(p))
                .ToListAsync();
        }

        public async Task<PlagiarismInfoDto> CheckProblemPlagiarismAsync(int id)
        {
            await EnsureProblemExists(id);

            var problem = await Context.Problems.FindAsync(id);
            if (problem is null || problem.Type != ProblemType.Ordinary)
            {
                throw new BadHttpRequestException("Plagiarism detection is only available for ordinary problems.");
            }

            var plagiarism = new Plagiarism
            {
                ProblemId = id,
                Results = null,
                Outdated = false
            };
            await Context.Plagiarisms.AddAsync(plagiarism);
            await Context.SaveChangesAsync();

            _queue.EnqueueTask(new JobRequestMessage(JobType.CheckPlagiarism, plagiarism.Id, 1));
            return new PlagiarismInfoDto(plagiarism);
        }
    }
}
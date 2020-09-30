using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Data.Configs;
using Data.DTOs;
using Data.Generics;
using Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebApp.Exceptions;

namespace WebApp.Services.Admin
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
    }

    public class AdminProblemService : LoggableService<AdminProblemService>, IAdminProblemService
    {
        private const int PageSize = 20;

        protected readonly IOptions<ApplicationConfig> Options;

        public AdminProblemService(IServiceProvider provider) : base(provider)
        {
            Options = provider.GetRequiredService<IOptions<ApplicationConfig>>();
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

            if (string.IsNullOrEmpty(dto.InputFormat) || string.IsNullOrEmpty(dto.OutputFormat))
            {
                throw new ValidationException("Invalid problem input or output format.");
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

        public async Task<PaginatedList<ProblemInfoDto>> GetPaginatedProblemInfosAsync(int? pageIndex)
        {
            var problems = await Context.Problems
                .OrderByDescending(p => p.Id)
                .PaginateAsync(pageIndex ?? 1, PageSize);

            var problemInfos = new List<ProblemInfoDto>();
            foreach (var problem in problems.Items)
            {
                var query = Context.Submissions.Where(s => s.ProblemId == problem.Id);
                var acceptedSubmissions = await query.CountAsync(s => s.Verdict == Verdict.Accepted);
                var totalSubmissions = await query.CountAsync();
                problemInfos.Add(new ProblemInfoDto(problem, false, false, acceptedSubmissions, totalSubmissions));
            }

            return new PaginatedList<ProblemInfoDto>
                (problems.TotalItems, problems.PageIndex, problems.PageSize, problemInfos);
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
            await Data.Archives.v1.ProblemArchive.ExtractTestCasesAsync(problem, file, "", Options);
            await Context.SaveChangesAsync();

            await LogInformation($"UpdateProblemTestCases Id={problem.Id} Count={problem.TestCases.Count}");
            return problem.TestCases;
        }

        public async Task<ProblemEditDto> ImportProblemAsync(int contestId, IFormFile file)
        {
            if (!await Context.Contests.AnyAsync(c => c.Id == contestId))
            {
                throw new ValidationException("Invalid Contest ID.");
            }

            var problem = await Data.Archives.v1.ProblemArchive.ParseAsync(contestId, file, Options);
            await Context.Problems.AddRangeAsync(problem);
            await Context.SaveChangesAsync();

            await Data.Archives.v1.ProblemArchive.ExtractTestCasesAsync(problem, file, "tests/", Options);
            await Context.SaveChangesAsync();

            return new ProblemEditDto(problem);
        }

        public async Task<byte[]> ExportProblemAsync(int id)
        {
            await EnsureProblemExists(id);
            var problem = await Context.Problems.FindAsync(id);
            return await Data.Archives.v1.ProblemArchive.CreateAsync(problem, Options);
        }
    }
}
using System;
using Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Worker.Runners.JudgeSubmission.ProblemTypes
{
    public class SpecialJudgeRunner : PlainRunner
    {
        public SpecialJudgeRunner(Contest contest, Problem problem, Submission submission, IServiceProvider provider)
            : base(contest, problem, submission, provider)
        {
            if (problem.SpecialJudgeProgram == null)
            {
                throw new Exception($"Special judge program is null for Problem={problem.Id}.");
            }
        }
    }
}
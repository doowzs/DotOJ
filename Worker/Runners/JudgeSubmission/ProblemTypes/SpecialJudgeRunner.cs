using System;
using Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Worker.Models;

namespace Worker.Runners.JudgeSubmission.ProblemTypes
{
    public class SpecialJudgeRunner : PlainRunner
    {
        public SpecialJudgeRunner
            (Contest contest, Problem problem, Submission submission, Box box, IServiceProvider provider)
            : base(contest, problem, submission, box, provider)
        {
            if (problem.SpecialJudgeProgram == null)
            {
                throw new Exception($"Special judge program is null for Problem={problem.Id}.");
            }
        }
    }
}
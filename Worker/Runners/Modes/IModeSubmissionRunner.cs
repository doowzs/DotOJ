using System.Threading.Tasks;
using Data.Models;
using Worker.Models;

namespace Worker.Runners.Modes
{
    public interface IModeSubmissionRunner
    {
        public Task<Result> Run(Data.Models.Submission submission, Problem problem);
    }
}
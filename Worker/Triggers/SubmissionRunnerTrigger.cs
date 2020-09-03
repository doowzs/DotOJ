using System;
using System.Threading.Tasks;

namespace Worker.Triggers
{
    public class SubmissionRunnerTrigger : TriggerBase<SubmissionRunnerTrigger>
    {
        public SubmissionRunnerTrigger(IServiceProvider provider) : base(provider)
        {
        }

        public override Task CheckAndRunAsync()
        {
            throw new NotImplementedException();
        }
    }
}
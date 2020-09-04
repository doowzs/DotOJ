using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Data.Configs;
using Data.Generics;
using Data.Models;
using IdentityServer4.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Worker.Models;

namespace Worker.Runners.Modes
{
    public class PracticeModeSubmissionRunner : ModeSubmissionRunnerBase<PracticeModeSubmissionRunner>
    {
        public PracticeModeSubmissionRunner(IServiceProvider provider) : base(provider)
        {
        }
    }
}
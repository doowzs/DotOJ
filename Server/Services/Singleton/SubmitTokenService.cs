using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;

namespace Server.Services.Singleton
{
    public class SubmitTokenService
    {
        private readonly ILogger<SubmitTokenService> _logger;
        private readonly AsyncReaderWriterLock _lock = new();
        private readonly Dictionary<string, (string UserId, int ProblemId, DateTime TimeStamp)> _dictionary = new();

        public SubmitTokenService(IServiceProvider provider)
        {
            _logger = provider.GetRequiredService<ILogger<SubmitTokenService>>();
        }

        public async Task ClearOutdatedTokensAsync()
        {
            using var locked = await _lock.ReaderLockAsync();
            var now = DateTime.Now.ToUniversalTime();
            var removals = _dictionary
                .Where(p => p.Value.TimeStamp <= now.AddMinutes(-30))
                .Select(p => p.Key)
                .ToList();
            foreach (var removal in removals)
            {
                _ = _dictionary.Remove(removal);
            }
        }

        public async Task<string> GetOrCreateToken(string userId, int problemId)
        {
            using (var locked = await _lock.ReaderLockAsync())
            {
                throw new NotImplementedException();
            }
        }

        public async Task<(string UserId, int ProblemId)> ConsumeTokenAsync(string token)
        {
            using var locked = await _lock.WriterLockAsync();
            if (_dictionary.TryGetValue(token, out var value) && _dictionary.Remove(token))
            {
                return (value.UserId, value.ProblemId);
            }

            return (null, 0);
        }
    }
}
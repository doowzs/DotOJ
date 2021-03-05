using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Generics;

namespace Server.Services.Singleton
{
    public class TestKitLabSubmitTokenService
    {
        private readonly ILogger<TestKitLabSubmitTokenService> _logger;

        private readonly LruCache<string, (string UserId, int ProblemId)>
            _cache = new(100000, null);

        public TestKitLabSubmitTokenService(IServiceProvider provider)
        {
            _logger = provider.GetRequiredService<ILogger<TestKitLabSubmitTokenService>>();
        }

        private async Task<(bool, string)> TryGetTokenAsync(string userId, int problemId)
        {
            bool Predicate(KeyValuePair<string, (string UserId, int ProblemId)> p) =>
                p.Value.UserId.Equals(userId) && p.Value.ProblemId.Equals(problemId);

            return await _cache.TryFindAsync(Predicate) is (true, var pair) ? (true, pair.Key) : (false, null);
        }

        private async Task<string> GenerateTokenAsync(string userId, int problemId)
        {
            var pool = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvqxyz";
            var random = RandomNumberGenerator.Create();
            var now = DateTime.Now.ToUniversalTime();
            for (int i = 0; i < 10; ++i)
            {
                var builder = new StringBuilder();
                var buffer = new byte[1];
                while (builder.Length < 8)
                {
                    random.GetBytes(buffer);
                    char character = (char) buffer[0];
                    if (pool.Contains(character))
                    {
                        builder.Append(character);
                    }
                }

                var token = builder.ToString();
                if (await _cache.TryAddAsync(token, (userId, problemId)))
                {
                    return token;
                }
            }
            throw new Exception("Cannot create unique token in 10 tries.");
        }

        public async Task<string> GetOrGenerateToken(string userId, int problemId)
        {
            return await TryGetTokenAsync(userId, problemId) is (true, var token)
                ? token
                : await GenerateTokenAsync(userId, problemId);
        }

        public async Task<(string UserId, int ProblemId)> ConsumeTokenAsync(string token)
        {
            return await _cache.TryGetValueAsync(token) is (true, var value)
                ? (value.UserId, value.ProblemId)
                : (null, 0);
        }
    }
}
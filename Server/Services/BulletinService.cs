using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.DTOs;
using Shared.Generics;
using Microsoft.EntityFrameworkCore;

namespace Server.Services
{
    public interface IBulletinService
    {
        public Task<List<BulletinInfoDto>> GetBulletinInfosAsync();
    }

    public class BulletinService : LoggableService<BulletinService>, IBulletinService
    {
        public BulletinService(IServiceProvider provider) : base(provider)
        {
        }

        public async Task<List<BulletinInfoDto>> GetBulletinInfosAsync()
        {
            var now = DateTime.Now.ToUniversalTime();
            return await Context.Bulletins
                .Where(b => (!b.PublishAt.HasValue || now >= b.PublishAt.Value) &&
                            (!b.ExpireAt.HasValue || now <= b.ExpireAt.Value))
                .Select(b => new BulletinInfoDto(b)) // sort by client
                .ToListAsync();
        }
    }
}
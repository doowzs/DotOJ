using System;
using System.Linq;
using System.Threading.Tasks;
using Shared.DTOs;
using Shared.Generics;
using Shared.Models;
using Microsoft.EntityFrameworkCore;
using WebApp.Exceptions;

namespace WebApp.Services.Admin
{
    public interface IAdminBulletinService
    {
        public Task<PaginatedList<BulletinInfoDto>> GetPaginatedBulletinInfosAsync(int? pageIndex);
        public Task<BulletinEditDto> GetBulletinEditAsync(int id);
        public Task<BulletinEditDto> CreateBulletinAsync(BulletinEditDto dto);
        public Task<BulletinEditDto> UpdateBulletinAsync(int id, BulletinEditDto dto);
        public Task DeleteBulletinAsync(int id);
    }

    public class AdminBulletinService : LoggableService<AdminBulletinService>, IAdminBulletinService
    {
        private const int PageSize = 20;

        public AdminBulletinService(IServiceProvider provider) : base(provider)
        {
        }

        private async Task EnsureBulletinExists(int id)
        {
            if (!await Context.Bulletins.AnyAsync(b => b.Id == id))
            {
                throw new NotFoundException();
            }
        }

        public async Task<PaginatedList<BulletinInfoDto>> GetPaginatedBulletinInfosAsync(int? pageIndex)
        {
            return await Context.Bulletins.OrderByDescending(b => b.Id)
                .PaginateAsync(b => new BulletinInfoDto(b), pageIndex ?? 1, PageSize);
        }

        public async Task<BulletinEditDto> GetBulletinEditAsync(int id)
        {
            await EnsureBulletinExists(id);
            return new BulletinEditDto(await Context.Bulletins.FindAsync(id));
        }

        public async Task<BulletinEditDto> CreateBulletinAsync(BulletinEditDto dto)
        {
            var bulletin = new Bulletin
            {
                Weight = dto.Weight.GetValueOrDefault(),
                Content = dto.Content,
                PublishAt = dto.PublishAt,
                ExpireAt = dto.ExpireAt
            };
            await Context.Bulletins.AddAsync(bulletin);
            await Context.SaveChangesAsync();
            return new BulletinEditDto(bulletin);
        }

        public async Task<BulletinEditDto> UpdateBulletinAsync(int id, BulletinEditDto dto)
        {
            await EnsureBulletinExists(id);
            var bulletin = await Context.Bulletins.FindAsync(id);
            bulletin.Weight = dto.Weight.GetValueOrDefault();
            bulletin.Content = dto.Content;
            bulletin.PublishAt = dto.PublishAt;
            bulletin.ExpireAt = dto.ExpireAt;
            Context.Bulletins.Update(bulletin);
            await Context.SaveChangesAsync();
            return new BulletinEditDto(bulletin);
        }

        public async Task DeleteBulletinAsync(int id)
        {
            await EnsureBulletinExists(id);
            var bullet = new Bulletin {Id = id};
            Context.Bulletins.Attach(bullet);
            Context.Bulletins.Remove(bullet);
            await Context.SaveChangesAsync();
        }
    }
}
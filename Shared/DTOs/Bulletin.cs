using System;
using System.ComponentModel.DataAnnotations;
using Shared.Generics;
using Shared.Models;

namespace Shared.DTOs
{
    public class BulletinInfoDto : DtoWithTimestamps
    {
        public int Id { get; }
        public int Weight { get; }
        public string Content { get; }
        public DateTime? PublishAt { get; set; }
        public DateTime? ExpireAt { get; set; }

        public BulletinInfoDto(Bulletin bulletin) : base(bulletin)
        {
            Id = bulletin.Id;
            Weight = bulletin.Weight;
            Content = bulletin.Content;
            PublishAt = bulletin.PublishAt;
            ExpireAt = bulletin.ExpireAt;
        }
    }

    public class BulletinEditDto : DtoWithTimestamps
    {
        public int? Id { get; }
        [Required] public int? Weight { get; set; }
        [Required] public string Content { get; set; }
        public DateTime? PublishAt { get; set; }
        public DateTime? ExpireAt { get; set; }

        public BulletinEditDto()
        {
        }

        public BulletinEditDto(Bulletin bulletin) : base(bulletin)
        {
            Id = bulletin.Id;
            Weight = bulletin.Weight;
            Content = bulletin.Content;
            PublishAt = bulletin.PublishAt;
            ExpireAt = bulletin.ExpireAt;
        }
    }
}
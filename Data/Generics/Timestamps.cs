using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Generics
{
    public abstract class ModelWithTimestamps
    {
        #region Timestamps
        [Required] public DateTime CreatedAt { get; set; }
        [Required] public DateTime UpdatedAt { get; set; }
        
        #endregion
    }

    [NotMapped]
    public abstract class DtoWithTimestamps
    {
        #region Timestamps
        
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; }

        public DtoWithTimestamps()
        {
        }

        public DtoWithTimestamps(ModelWithTimestamps model)
        {
            CreatedAt = model.CreatedAt;
            UpdatedAt = model.UpdatedAt;
        }

        #endregion
    }
}
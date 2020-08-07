using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Judge1.Models
{
    public abstract class ModelWithTimestamps
    {
        #region Timestamps

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }
        
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
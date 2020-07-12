using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Judge1.Models
{
    public enum AssignmentMode
    {
        OiMode = 1,
        CpcMode = 2,
    }

    public class Assignment : ModelWithTimestamps
    {
        public int Id { get; set; }

        #region Assignment Content

        [Required] public string Name { get; set; }
        [Required, Column(TypeName = "text")] public string Description { get; set; }

        [Required] public AssignmentMode Mode { get; set; }
        
        [Required] public DateTime BeginTime { get; set; }
        [Required] public DateTime EndTime { get; set; }

        #endregion

        #region Relationships

        public List<Problem> Problems { get; set; }
        public List<AssignmentNotice> Notices { get; set; }
        public List<AssignmentRegistration> Registrations { get; set; }

        #endregion
    }

    public class AssignmentNotice : ModelWithTimestamps
    {
        public int Id { get; set; }
        
        public int AssignmentId { get; set; }
        public Assignment Assignment { get; set; }
        
        [Required, Column(TypeName = "text")] public string Content { get; set; }
    }

    public class AssignmentRegistration
    {
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }
        
        public int AssignmentId { get; set; }
        public Assignment Assignment { get; set; }
        
        public bool IsParticipant { get; set; }
        public bool IsAssignmentManager { get; set; }
    }
}
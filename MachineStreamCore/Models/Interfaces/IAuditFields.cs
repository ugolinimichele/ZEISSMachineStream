using System;

namespace MachineStreamCore.Models.Interfaces
{
    public interface IAuditFields
    {
        string CreatedBy { get; set; }
        DateTime CreatedAt { get; set; }
        string UpdatedBy { get; set; }
        DateTime UpdatedAt { get; set; }
        bool IsDeleted { get; set; }
    }
}

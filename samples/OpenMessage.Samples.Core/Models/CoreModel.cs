using System;

namespace OpenMessage.Samples.Core.Models
{
    public abstract class CoreModel
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
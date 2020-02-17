using System;

namespace OpenMessage.Samples.Core.Models
{
    public class SimpleModel : CoreModel
    {
        public string Property1 { get; set; } = Guid.NewGuid()
                                                    .ToString("n");

        public string Property2 { get; set; } = Guid.NewGuid()
                                                    .ToString("n");

        public string Property3 { get; set; } = Guid.NewGuid()
                                                    .ToString("n");
    }
}
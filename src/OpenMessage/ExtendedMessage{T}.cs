using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenMessage
{
    public class ExtendedMessage<T> : Message<T>, ISupportProperties, ISupportIdentification
    {
        public IEnumerable<KeyValuePair<string, string>> Properties { get; set; } = Enumerable.Empty<KeyValuePair<string, string>>();
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
    }
}
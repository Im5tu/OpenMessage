using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OpenMessage
{
    public class Batch<T> : ISupportAcknowledgement, IReadOnlyCollection<Message<T>>
    {
        public IReadOnlyCollection<Message<T>> Messages { get; }

        public Batch(IEnumerable<Message<T>> messages)
        {
            Messages = new ReadOnlyCollection<Message<T>>(messages.ToArray());
        }

        public AcknowledgementState AcknowledgementState { get; }

        public Task AcknowledgeAsync(bool positivelyAcknowledge = true)
        {
            var tasks = Messages.OfType<ISupportAcknowledgement>().Select(x => x.AcknowledgeAsync(positivelyAcknowledge));

            return Task.WhenAll(tasks);
        }

        public IEnumerator<Message<T>> GetEnumerator() => Messages.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) Messages).GetEnumerator();

        public int Count => Messages.Count;
    }
}
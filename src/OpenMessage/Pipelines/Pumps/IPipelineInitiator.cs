using System.Threading;
using System.Threading.Tasks;
using OpenMessage.Pipelines.Middleware;

namespace OpenMessage.Pipelines.Pumps
{
    /// <summary>
    /// Pushes a message into the provided pipeline
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPipelineInitiator<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tracer"></param>
        /// <param name="pipeline"></param>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Initiate(Trace.ActivityTracer tracer, PipelineDelegate.SingleMiddleware<T> pipeline, Message<T> message, CancellationToken cancellationToken);
    }
}
using System.Threading.Channels;
using KariyerNet.Application.Interfaces;

namespace KariyerNet.API.BackgroundServices
{
    public class EvaluationQueue : IEvaluationQueue
    {
        private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>();

        public ValueTask EnqueueAsync(Guid applicationId)
        {
            return _channel.Writer.WriteAsync(applicationId);
        }

        public IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken cancellationToken)
        {
            return _channel.Reader.ReadAllAsync(cancellationToken);
        }
    }
}
namespace KariyerNet.Application.Interfaces
{
    public interface IEvaluationQueue
    {
        ValueTask EnqueueAsync(Guid applicationId);
    }
}
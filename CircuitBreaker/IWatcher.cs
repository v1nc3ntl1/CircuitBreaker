using System;

namespace CircuitBreaker
{
    public interface IWatcher
    {
        PipelineData<T> Evaluate<T>(Func<T> requestOperation);
    }
}
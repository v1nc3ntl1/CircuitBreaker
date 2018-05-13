namespace CircuitBreaker
{
    public class PipelineData<T>
    {
        public PipelineData()
        {
        }

        public T Value { get; set; }
        public bool Success { get; set; }
    }
}
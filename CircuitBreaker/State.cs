/// <summary>
/// State of circuit
/// </summary>

namespace CircuitBreaker
{
    public static class State
    {
        public static string Opened = "Opened";
        public static string Closed = "Closed";
        public static string HalfOpened = "HalfOpened";
    }
}

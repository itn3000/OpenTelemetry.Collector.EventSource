using System.Diagnostics.Tracing;

namespace SampleUtil
{
    [EventSource]
    public class SampleEventSource : EventSource
    {
        public const int Event1Id = 1;
        [Event(Event1Id)]
        public void Event1(int x)
        {
            WriteEvent(Event1Id, x);
        }
        private SampleEventSource()
        {
        }
        public static readonly SampleEventSource Log = new SampleEventSource();
    }
}
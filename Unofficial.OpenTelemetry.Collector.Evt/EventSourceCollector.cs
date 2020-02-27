using System;
using System.Diagnostics.Tracing;
using System.Collections.Generic;
using OpenTelemetry.Trace;

namespace Unofficial.OpenTelemetry.Collector.Evt
{
    internal class EventSourceAdapter : EventListener
    {
        Tracer _Tracer;
        Func<EventWrittenEventArgs, Event> _convertFunc;
        public EventSourceAdapter(Tracer tracer) : this(tracer, null)
        {
        }
        public EventSourceAdapter(Tracer tracer, Func<EventWrittenEventArgs, Event> convertFunc)
        {
            _Tracer = tracer;
            _convertFunc = convertFunc;
        }
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (_convertFunc == null)
            {
                _Tracer.CurrentSpan.AddEvent(new Event(eventData.EventName, new Dictionary<string, object>()
                {
                    ["data"] = eventData
                }));
            }
            else
            {
                _Tracer.CurrentSpan.AddEvent(_convertFunc(eventData));
            }
        }
    }
}

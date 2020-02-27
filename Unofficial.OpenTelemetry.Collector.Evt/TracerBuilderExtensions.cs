using System;
using System.Diagnostics.Tracing;
using System.Collections.Generic;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Configuration;
using Unofficial.OpenTelemetry.Collector.Evt;

namespace OpenTelemetry.Trace.Configuration
{
    public static class TracerBuilderExtensions
    {
        public static TracerBuilder UseEventSource(this TracerBuilder builder, EventSource ev, EventLevel level, Func<EventWrittenEventArgs, Event> convertFunc = null)
        {
            return UseEventSource(builder, new KeyValuePair<EventSource, EventLevel>[] { new KeyValuePair<EventSource, EventLevel>(ev, level) }, convertFunc);
        }
        public static TracerBuilder UseEventSource(this TracerBuilder builder, IEnumerable<KeyValuePair<EventSource, EventLevel>> events, Func<EventWrittenEventArgs, Event> convertFunc = null)
        {
            builder.AddCollector(tracer =>
            {
                var collector = new EventSourceAdapter(tracer, convertFunc);
                foreach (var pair in events)
                {
                    collector.EnableEvents(pair.Key, pair.Value);
                }
                return collector;
            });
            return builder;
        }
    }
}
using System;
using System.Diagnostics.Tracing;
using System.Collections.Generic;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Configuration;
using Unofficial.OpenTelemetry.Collector.Evt;

namespace OpenTelemetry.Trace.Configuration
{
    public class EventEnableOption
    {
        public EventLevel Level = EventLevel.LogAlways;
        public EventKeywords Keywords = EventKeywords.All;
        public IDictionary<string, string> EventArguments = null;
    }
    public class EventSourceCollectorOption
    {
        List<(EventSource, EventEnableOption)> _Events = new List<(EventSource, EventEnableOption)>();
        public IEnumerable<(EventSource, EventEnableOption)> Events => _Events;
        public Func<EventWrittenEventArgs, Event> ConvertFunc { get; set; }
        public Func<EventSource, (bool, EventEnableOption)> IsEnableFunc { get; set; }
        public static EventSourceCollectorOption Create()
        {
            return new EventSourceCollectorOption();
        }
        public static EventSourceCollectorOption Create(IEnumerable<EventSource> events)
        {
            var ret = new EventSourceCollectorOption();
            foreach(var ev in events)
            {
                ret.Add(ev, new EventEnableOption());
            }
            return ret;
        }
        public EventSourceCollectorOption Add(EventSource source, EventEnableOption option)
        {
            if(option == null)
            {
                option = new EventEnableOption();
            }
            _Events.Add((source, option));
            return this;
        }
        public EventSourceCollectorOption Add(EventSource source)
        {
            return Add(source, null);
        }
        public EventSourceCollectorOption SetConvertFunc(Func<EventWrittenEventArgs, Event> f)
        {
            ConvertFunc = f;
            return this;
        }
        public EventSourceCollectorOption SetIsEnableFunc(Func<EventSource, (bool, EventEnableOption)> f)
        {
            IsEnableFunc = f;
            return this;
        }
    }
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
        public static TracerBuilder UseEventSource(this TracerBuilder builder, EventSourceCollectorOption option)
        {
            builder.AddCollector(tracer =>
            {
                var collector = new EventSourceAdapter(tracer, option.ConvertFunc, option.IsEnableFunc);
                foreach (var (ev, evoption) in option.Events)
                {
                    collector.Add(ev, evoption);
                }
                return collector;
            });
            return builder;
        }
    }
}
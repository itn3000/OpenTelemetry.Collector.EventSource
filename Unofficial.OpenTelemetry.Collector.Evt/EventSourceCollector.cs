using System;
using System.Diagnostics.Tracing;
using System.Collections.Generic;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Configuration;

namespace Unofficial.OpenTelemetry.Collector.Evt
{
    internal sealed class EventSourceAdapter : EventListener
    {
        Tracer _Tracer;
        Func<EventWrittenEventArgs, Event> _convertFunc;
        Func<EventSource, (bool, EventEnableOption)> _isEnabledFunc;
        List<EventSource> _eventsBeforeInit = new List<EventSource>();
        static readonly Func<EventSource, (bool, EventEnableOption)> _alwaysDisable = (ev) => (false, default);
        public EventSourceAdapter(Tracer tracer) : this(tracer, null)
        {
        }
        public EventSourceAdapter(Tracer tracer, Func<EventWrittenEventArgs, Event> convertFunc) : this(tracer, convertFunc, _alwaysDisable)
        {
        }
        public EventSourceAdapter(Tracer tracer, Func<EventWrittenEventArgs, Event> convertFunc, Func<EventSource, (bool, EventEnableOption)> isEnabled)
        {
            _Tracer = tracer;
            _convertFunc = convertFunc;
            _isEnabledFunc = isEnabled ?? _alwaysDisable;
            // OnSourceCreated may be called before constructor
            foreach (var evsrc in _eventsBeforeInit)
            {
                var (isenabled, opts) = _isEnabledFunc(evsrc);
                if (isenabled)
                {
                    Add(evsrc, opts);
                }
            }
        }
        public void Add(EventSource source, EventEnableOption option)
        {
            this.EnableEvents(source, option.Level, option.Keywords, option.EventArguments);
        }
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            Event ev;
            if (_convertFunc == null)
            {
                ev = new Event(eventData.EventName, new Dictionary<string, object>()
                {
                    ["data"] = eventData
                });
            }
            else
            {
                ev = _convertFunc(eventData);
            }
            WriteEventInternal(ev);
        }
        void WriteEventInternal(Event ev)
        {
            if (_Tracer.CurrentSpan.Context.IsValid)
            {
                _Tracer.CurrentSpan.AddEvent(ev);
            }
            else
            {
                using (_Tracer.StartActiveSpan(ev.Name, out var span))
                {
                    span.AddEvent(ev);
                }
            }
        }
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            base.OnEventSourceCreated(eventSource);
            if (_isEnabledFunc != null)
            {
                var (isEnable, opt) = _isEnabledFunc(eventSource);
                if (isEnable)
                {
                    EnableEvents(eventSource, opt.Level, opt.Keywords, opt.EventArguments);
                }
            }
            else
            {
                // before constructor
                _eventsBeforeInit.Add(eventSource);
            }
        }
    }
}

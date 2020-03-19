# Overview

[![NuGet Badge](https://buildstats.info/nuget/Unofficial.OpenTelemetry.Collector.EventSource?includePreReleases=true)](https://www.nuget.org/packages/Unofficial.OpenTelemetry.Collector.EventSource)

This is the C# library for collecting [EventSource](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.tracing.eventsource) with [opentelemetry](https://opentelemetry.io)

# Usage

## Installing from NuGet

add `Unofficial.OpenTelemetry.Collector.EventSource` to your nuget package reference.

available version is listed in [nuget project page](https://www.nuget.org/packages/Unofficial.OpenTelemetry.Collector.EventSource)

## Initialize

### Add Custom Events

if you want to add your EventSource, you should use `UseEventSource(this TracerBuilder builder, EventSource ev, EventLevel level)` or
`UseEventSource(this TracerBuilder builder, IEnumerable<KeyValuePair<EventSource, EventLevel>> events)`
sample code is here

```csharp
// using System.Diagnostics.Tracing;
// using OpenTelemetry.Trace;
// using OpenTelemetry.Trace.Configuration;
// "YourEventSource.Log" is static readonly instance of your custom EventSource.
using(var factory = TracerFactory.Create(builder => 
{
    builder.UseEventSource(YourEventSource.Log, EventLevel.Always);
    // add your exporter...
}))
{
    // ...
}
```

### Add Framework Events

if you want to add Framework's EventSource(like "System.Runtime", or another EventSource you cannot get instance directly),
set `EventSourceCollectorOption.IsEnableFunc` and pass in `UseEventSource`.
sample code is here

```csharp
// using System.Diagnostics.Tracing;
// using OpenTelemetry.Trace;
// using OpenTelemetry.Trace.Configuration;
// "YourEventSource.Log" is static readonly instance of your custom EventSource.

using(var factory = TracerFactory.Create(builder => 
{
    builder.UseEventSource(EventSourceCollectorOption.Create().SetIsEnableFunc(src =>
    {
        // if System.Threading.Tasks.TplEventSource enabled, stackoverflow error may be occured when creating span
        if(!src.Name != "System.Threading.Tasks.TplEventSource")
        {
            Console.WriteLine($"{src.Name} enabled");
            return (true, new EventEnableOption());
        }
        else
        {
            Console.WriteLine($"{src.Name} ignored");
            return (false, default);
        }
    }));
    // add your exporter...
}))
{
    // ...
}
```

## Using with IServiceCollection

1. define your eventsource, ex: 
2. do `OpenTelemetry.Trace.Configuration.TracerBuilderExtensions.UseEventSource` with your TracerBuilder instance(this is extension method)

* [basic sample is here](sample/BasicSample)
* [events from framework sample is here](sample/SystemEventSample)

# ChangeLog

## 0.2.0

Add `UseEventSource(EventSourceCollectorOption)` for collecting framework events.

## 0.1.0

First release
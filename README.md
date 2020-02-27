# Overview

This is the C# library for collecting [EventSource](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.tracing.eventsource) with [opentelemetry](https://opentelemetry.io)

# Usage

## Installing from NuGet

add `Unofficial.OpenTelemetry.Collector.Evt` to your nuget package reference.

## Using with IServiceCollection

1. define your eventsource, ex: 
2. do `OpenTelemetry.Trace.Configuration.TracerBuilderExtensions.UseEventSource` with your TracerBuilder instance(this is extension method)

[basic sample is here](sample/BasicSample)

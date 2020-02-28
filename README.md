# Overview

[![NuGet Badge](https://buildstats.info/nuget/Unofficial.OpenTelemetry.Collector.EventSource?includePreReleases=true)](https://www.nuget.org/packages/Unofficial.OpenTelemetry.Collector.EventSource)

This is the C# library for collecting [EventSource](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.tracing.eventsource) with [opentelemetry](https://opentelemetry.io)

# Usage

## Installing from NuGet

add `Unofficial.OpenTelemetry.Collector.Evt` to your nuget package reference.

available version is listed in [nuget project page](https://www.nuget.org/packages/Unofficial.OpenTelemetry.Collector.EventSource)

## Using with IServiceCollection

1. define your eventsource, ex: 
2. do `OpenTelemetry.Trace.Configuration.TracerBuilderExtensions.UseEventSource` with your TracerBuilder instance(this is extension method)

[basic sample is here](sample/BasicSample)

using System;
using System.Threading.Tasks;
using System.Diagnostics.Tracing;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Configuration;
using OpenTelemetry.Trace.Export;
using SampleUtil;

namespace NoDISample
{
    class Program
    {
        static void Main(string[] args)
        {
            using(var factory = TracerFactory.Create((builder) => 
                builder.UseEventSource(SampleEventSource.Log, EventLevel.LogAlways)
                    .AddDelegateExporter((dataList, ct) =>
                    {
                        foreach(var data in dataList)
                        {
                            Console.WriteLine($"{data.Name}, {data.ParentSpanId}");
                            foreach(var ev in data.Events)
                            {
                                Console.WriteLine($"  {ev.Name}, {ev.Timestamp}");
                            }
                        }
                        return Task.FromResult(SpanExporter.ExportResult.Success);
                    })
            ))
            {
                var tracer = factory.GetTracer("nodisapmle", "semver:1.0.0");
                using(tracer.StartActiveSpan("tracerspan", out var span))
                {
                    SampleEventSource.Log.Event1(1);
                }
            }
        }
    }
}

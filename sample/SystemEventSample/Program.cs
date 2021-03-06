﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenTelemetry.Trace.Configuration;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Export;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics.Tracing;
using SampleUtil;

namespace SystemEventSample
{
    [EventSource]
    public class MyEventSource : EventSource
    {
        public const int Event1Id = 1;
        [Event(Event1Id)]
        public void Event1(int x)
        {
            WriteEvent(Event1Id, x);
        }
        private MyEventSource()
        {
        }
        public static readonly MyEventSource Log = new MyEventSource();
    }

    class Program : IHostedService
    {
        Tracer _tracer;
        public Program(TracerFactoryBase tracerFactory)
        {
            _tracer = tracerFactory.GetTracer("mytracername", "semver:1.0.0");
        }
        static void ProcessEventData(IEnumerable<SpanData> dataList)
        {
            foreach (var data in dataList)
            {
                Console.WriteLine($"{data.Name}, {data.Kind}, {data.Status}");
                foreach (var ev in data.Events)
                {
                    Console.WriteLine($"  {ev.Name}");
                    foreach (var attr in ev.Attributes)
                    {
                        Console.WriteLine($"    {attr.Key}, {attr.Value}");
                    }
                }
            }
        }
        static void Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddOpenTelemetry(builder =>
                    {
                        builder.UseEventSource(EventSourceCollectorOption.Create()
                            .SetIsEnableFunc((src) =>
                            {
                                if(!src.Name.StartsWith("System.Threading"))
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
                        builder.AddDelegateExporter((dataList, ct) =>
                        {
                            ProcessEventData(dataList);
                            return Task.FromResult(SpanExporter.ExportResult.Success);
                        });
                    });
                    services.AddHostedService<Program>();
                })
                .UseConsoleLifetime()
                .Build();
            host.Run();
        }

        Task _task;
        CancellationTokenSource _cts = new CancellationTokenSource();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _task = Task.Run(() =>
            {
                while (!_cts.Token.WaitHandle.WaitOne(1000))
                {
                    using (_tracer.StartActiveSpan("span1", out var span))
                    {
                        MyEventSource.Log.Event1(1);
                    }
                }
            });
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            await _task.ConfigureAwait(false);
            _cts.Dispose();
        }
    }
}

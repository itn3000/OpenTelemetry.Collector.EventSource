using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenTelemetry.Trace.Configuration;
using OpenTelemetry.Trace.Export;

namespace BasicSample
{
    using OnFinishFunc = Func<CancellationToken, Task>;
    using OnProcessFunc = Func<IEnumerable<SpanData>, CancellationToken, Task<SpanExporter.ExportResult>>;

    class DelegateExporter : SpanExporter
    {
        OnProcessFunc _func;
        OnFinishFunc _finish;
        public DelegateExporter(OnProcessFunc onProcess,
            OnFinishFunc onShutdown)
        {
            _func = onProcess;
            _finish = onShutdown;
        }
        public override Task<ExportResult> ExportAsync(IEnumerable<SpanData> batch, CancellationToken cancellationToken)
        {
            return _func(batch, cancellationToken);
        }

        public override Task ShutdownAsync(CancellationToken cancellationToken)
        {
            return _finish(cancellationToken);
        }
    }
    static class TracerBuilderExtensions
    {
        public static TracerBuilder AddDelegateExporter(this TracerBuilder builder,
            OnProcessFunc onProcess,
            OnFinishFunc onShutdown = null)
        {
            return builder.AddProcessorPipeline((cfg) => 
                cfg.SetExporter(new DelegateExporter(onProcess, onShutdown))
                    .SetExportingProcessor(e => new SimpleSpanProcessor(e)));
        }
    }
}
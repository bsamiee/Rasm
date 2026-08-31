namespace Lab.T01;

internal static class Logging {
    public static void Emit(OrderStatus status, Serilog.Core.ILogEventSink sink) {
        using Serilog.Core.Logger logger = new Serilog.LoggerConfiguration().Destructure.UsingThinktectureRuntimeExtensions().WriteTo.Sink(sink).CreateLogger();
        logger.Information("status {@Status}", status);
    }
}

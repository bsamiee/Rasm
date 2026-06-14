using System.Diagnostics;
using System.Text.Json;
using Rasm.Bridge.Contract;
using Rhino.Display;

namespace Rasm.Bridge.Cargo;

// --- [SERVICES] -----------------------------------------------------------------------------

// Ownership: per-scenario JSONL evidence and adjacent PNG captures. WriteThrough JSONL makes each
// complete line crash-durable, while disk faults degrade to counted spool facts instead of failing
// the scenario.
internal sealed class Spool : IDisposable {
    private const int CaptureDpi = 96;
    private const int FallbackHeight = 768;
    private const int FallbackWidth = 1024;

    private readonly string reportDir;
    private readonly string scenario;
    private readonly FileStream? stream;

    internal Spool(string reportDir, string scenario) {
        // BOUNDARY ADAPTER: an unopened spool keeps the run publish-only and counted as degraded.
        this.reportDir = reportDir;
        this.scenario = scenario;
        try {
            _ = Directory.CreateDirectory(path: reportDir);
            stream = new FileStream(
                path: Path.Combine(path1: reportDir, path2: scenario + ".jsonl"),
                mode: FileMode.Append, access: FileAccess.Write, share: FileShare.Read,
                bufferSize: 1, options: FileOptions.WriteThrough);
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or NotSupportedException) {
            stream = null;
            Failures++;
            Debug.WriteLine(message: $"spool open failed for '{scenario}': {error.Message}");
        }
    }

    internal int Failures { get; private set; }

    internal void Append(BridgeEvent evt) {
        // BOUNDARY ADAPTER: one flushed JSONL line per event is the crash-durability unit.
        try {
            if (stream is { } live) {
                live.Write(buffer: JsonSerializer.SerializeToUtf8Bytes(value: evt, jsonTypeInfo: BridgeJsonContext.Default.BridgeEvent));
                live.WriteByte(value: (byte)'\n');
                live.Flush();
            } else {
                Failures++;
            }
        } catch (Exception error) when (error is IOException or ObjectDisposedException or UnauthorizedAccessException) {
            Failures++;
            Debug.WriteLine(message: $"spool append failed for '{scenario}': {error.Message}");
        }
    }

    internal Fin<BridgeEvent.CaptureCase> Capture(RhinoView view, string? label, bool onFailure) {
        // BOUNDARY ADAPTER: capture the current idle frame beside the spool; the runner stamps it.
        try {
            System.Drawing.Size frame = view.ActiveViewport.Size;
            int width = frame.Width > 0 ? frame.Width : FallbackWidth;
            int height = frame.Height > 0 ? frame.Height : FallbackHeight;
            string path = Path.Combine(path1: reportDir, path2: label is { Length: > 0 } ? $"{scenario}.{label}.png" : scenario + ".png");
            using ViewCaptureSettings settings = new(sourceView: view, mediaSize: new System.Drawing.Size(width: width, height: height), dpi: CaptureDpi);
            using System.Drawing.Bitmap? bitmap = ViewCapture.CaptureToBitmap(settings: settings);
            if (bitmap is null) {
                return Fin.Fail<BridgeEvent.CaptureCase>(error: Error.New(message: "ViewCapture.CaptureToBitmap returned null"));
            }
            bitmap.Save(filename: path, format: System.Drawing.Imaging.ImageFormat.Png);
            return Fin.Succ(value: new BridgeEvent.CaptureCase(Path: path, Width: width, Height: height, OnFailure: onFailure) { Stamp = default });
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException or NotSupportedException) {
            return Fin.Fail<BridgeEvent.CaptureCase>(error: Error.New(message: $"viewport capture failed: {error.Message}"));
        }
    }

    public void Dispose() => stream?.Dispose();
}

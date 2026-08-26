using System.Diagnostics;
using System.Text.Json;
using Rasm.Bridge.Contract;
using Rhino.Display;

namespace Rasm.Bridge.Cargo;

// --- [SERVICES] ------------------------------------------------------------------------

internal sealed class Spool : IDisposable {
    private const int CaptureDpi = 96;
    private const int FallbackWidth = 1024;
    private const int FallbackHeight = 768;

    private readonly string reportDir;
    private readonly string scenario;
    private readonly FileStream? stream;

    internal Spool(string reportDir, string scenario) {
        this.reportDir = reportDir;
        this.scenario = scenario;
        try {
            _ = Directory.CreateDirectory(Path.Combine(reportDir, ReportLayout.EventsDirectory));
            stream = new FileStream(ReportLayout.Spool(reportDir, scenario), FileMode.Append, FileAccess.Write, FileShare.Read, bufferSize: 1, FileOptions.WriteThrough);
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or NotSupportedException) {
            stream = null;
            Failures++;
            Debug.WriteLine($"spool open failed for '{scenario}': {error.Message}");
        }
    }

    internal int Failures { get; private set; }

    internal void Append(BridgeEvent evt) {
        try {
            if (stream is { } live) {
                live.Write(JsonSerializer.SerializeToUtf8Bytes(evt, BridgeJsonContext.Default.BridgeEvent));
                live.WriteByte((byte)'\n');
                live.Flush();
            } else {
                Failures++;
            }
        } catch (Exception error) when (error is IOException or ObjectDisposedException or UnauthorizedAccessException) {
            Failures++;
            Debug.WriteLine($"spool append failed for '{scenario}': {error.Message}");
        }
    }

    internal Fin<BridgeEvent.CaptureCase> Capture(RhinoView view, string label, bool onFailure) {
        try {
            System.Drawing.Size frame = view.ActiveViewport.Size;
            int width = frame.Width > 0 ? frame.Width : FallbackWidth;
            int height = frame.Height > 0 ? frame.Height : FallbackHeight;
            string directory = Path.Combine(reportDir, ReportLayout.CapturesDirectory, scenario);
            _ = Directory.CreateDirectory(directory);
            string stem = Sanitize(label);
            string path = Path.Combine(directory, $"{stem}.png");
            string temp = path + ".tmp";
            using ViewCaptureSettings settings = new(view, new System.Drawing.Size(width, height), CaptureDpi);
            using System.Drawing.Bitmap? bitmap = ViewCapture.CaptureToBitmap(settings);
            if (bitmap is null) {
                return Error.New("ViewCapture.CaptureToBitmap returned null");
            }
            bitmap.Save(temp, System.Drawing.Imaging.ImageFormat.Png);
            File.Move(temp, path, overwrite: true);
            return new BridgeEvent.CaptureCase(
                ArtifactRef.Index(reportDir, path, EvidenceRole.Capture, scenario, onFailure),
                width, height, stem, view.ActiveViewport.Name) { Stamp = default };
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException or NotSupportedException) {
            return Error.New($"viewport capture failed: {error.Message}");
        }
    }

    internal static string Sanitize(string label) {
        Span<char> chars = stackalloc char[label.Length];
        for (int index = 0; index < label.Length; index++) {
            char c = label[index];
            chars[index] = char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.' ? c : '-';
        }
        return new string(chars).Trim('-') is { Length: > 0 } clean ? clean : "capture";
    }

    public void Dispose() => stream?.Dispose();
}

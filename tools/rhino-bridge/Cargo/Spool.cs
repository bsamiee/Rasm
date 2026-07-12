using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using Rasm.Bridge.Contract;
using Rhino.Display;

namespace Rasm.Bridge.Cargo;

// --- [SERVICES] ------------------------------------------------------------------------

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
            _ = Directory.CreateDirectory(path: Path.Combine(path1: reportDir, path2: ReportLayout.EventsDirectory));
            stream = new FileStream(
                path: ReportLayout.Spool(reportDir: reportDir, scenario: scenario),
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
            string captureDir = Path.Combine(path1: reportDir, path2: ReportLayout.CapturesDirectory, path3: scenario);
            _ = Directory.CreateDirectory(path: captureDir);
            string stem = label is { Length: > 0 } ? Sanitize(label) : "failure";
            string path = Path.Combine(path1: captureDir, path2: $"{stem}.png");
            string temp = path + ".tmp";
            using ViewCaptureSettings settings = new(sourceView: view, mediaSize: new System.Drawing.Size(width: width, height: height), dpi: CaptureDpi);
            using System.Drawing.Bitmap? bitmap = ViewCapture.CaptureToBitmap(settings: settings);
            if (bitmap is null) {
                return Fin.Fail<BridgeEvent.CaptureCase>(error: Error.New(message: "ViewCapture.CaptureToBitmap returned null"));
            }
            bitmap.Save(filename: temp, format: System.Drawing.Imaging.ImageFormat.Png);
            File.Move(sourceFileName: temp, destFileName: path, overwrite: true);
            ArtifactRef artifact = IndexFile(
                reportDir: reportDir, path: path, scenario: scenario, role: EvidenceRole.Capture,
                mediaType: "image/png", retention: onFailure ? ArtifactRetentionClass.Forensic : ArtifactRetentionClass.Evidence,
                onFailure: onFailure);
            CaptureArtifact capture = new(
                Artifact: artifact, Width: width, Height: height, OnFailure: onFailure, Label: stem,
                Frame: string.Create(provider: CultureInfo.InvariantCulture, $"{width}x{height}"), Camera: string.Empty, NonBlank: true);
            return Fin.Succ(value: new BridgeEvent.CaptureCase(Path: path, Width: width, Height: height, OnFailure: onFailure) {
                Stamp = default,
                Artifact = artifact,
                Capture = capture,
            });
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException or NotSupportedException) {
            return Fin.Fail<BridgeEvent.CaptureCase>(error: Error.New(message: $"viewport capture failed: {error.Message}"));
        }
    }

    internal static ArtifactRef IndexFile(
        string reportDir, string path, string scenario, EvidenceRole role,
        string mediaType, ArtifactRetentionClass retention, bool onFailure = false) {
        FileInfo info = new(fileName: path);
        string relative = Path.GetRelativePath(relativeTo: reportDir, path: path);
        string id = relative.Replace(oldChar: Path.DirectorySeparatorChar, newChar: '/');
        return new ArtifactRef(
            Id: id, Role: role, RelativePath: id, MediaType: mediaType, Bytes: info.Exists ? info.Length : 0L,
            Hash: Hash(path: path), Retention: retention, Scenario: scenario, OnFailure: onFailure);
    }

    private static ArtifactHash Hash(string path) {
        try {
            return new ArtifactHash(Algorithm: "sha256", Value: Convert.ToHexStringLower(inArray: SHA256.HashData(source: File.ReadAllBytes(path: path))));
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return new ArtifactHash(Algorithm: "sha256", Value: string.Empty);
        }
    }

    private static string Sanitize(string label) {
        Span<char> chars = stackalloc char[label.Length];
        for (int i = 0; i < label.Length; i++) {
            char c = label[index: i];
            chars[index: i] = char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.' ? c : '-';
        }
        return new string(value: chars).Trim(trimChar: '-').ToUpperInvariant() is { Length: > 0 } clean ? clean : "CAPTURE";
    }

    public void Dispose() => stream?.Dispose();
}

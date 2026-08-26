using System.Globalization;
using System.IO.Hashing;
using System.Text;
using System.Text.Json;
using Rasm.Bridge.Contract;

namespace Rasm.Bridge.Supervisor;

// --- [OPERATIONS] ----------------------------------------------------------------------

internal static class Evidence {
    internal static Seq<BridgeEvent> HarvestSpool(string reportDir, string scenario) {
        string path = ReportLayout.Spool(reportDir, scenario);
        try {
            return !File.Exists(path) ? Seq<BridgeEvent>() : toSeq(File.ReadLines(path)).Choose(Decode);
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return Seq<BridgeEvent>();
        }
    }

    internal static (long Count, long LastSequence) SpoolTail(string reportDir) {
        Seq<string> files;
        try {
            string events = Path.Combine(reportDir, ReportLayout.EventsDirectory);
            files = Directory.Exists(events) ? toSeq(Directory.EnumerateFiles(events, "*.jsonl")) : Seq<string>();
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            files = Seq<string>();
        }
        return SpoolTail(files.Bind(path => HarvestSpool(reportDir, Path.GetFileNameWithoutExtension(path))));
    }

    internal static (long Count, long LastSequence) SpoolTail(Seq<BridgeEvent> harvested) {
        Seq<BridgeEvent> evidence = harvested.Filter(static evt => evt.IsEvidence);
        return (evidence.Count, evidence.Fold(0L, static (max, evt) => Math.Max(max, evt.Stamp.Sequence)));
    }

    internal static ArtifactRef[] ArtifactRefs(string reportDir) {
        try {
            return !Directory.Exists(reportDir)
                ? []
                : [.. Directory.EnumerateFiles(reportDir, "*", SearchOption.AllDirectories)
                    .Select(path => Relative(reportDir, path))
                    .Where(IsEvidenceFile)
                    .Order(StringComparer.Ordinal)
                    .Select(relative => ArtifactRef(reportDir, relative))];
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return [];
        }
    }

    internal static string WriteCertificate(string reportDir, SessionEnvelope envelope) {
        string path = ReportLayout.Certificate(reportDir);
        _ = Directory.CreateDirectory(reportDir);
        string temp = Path.Combine(reportDir, $".{Guid.NewGuid():N}.tmp");
        File.WriteAllText(temp, JsonSerializer.Serialize(envelope, BridgeJsonContext.Default.SessionEnvelope));
        File.Move(temp, path, overwrite: true);
        return path;
    }

    internal static Option<CrashFact> IpsDiff(Seq<string> baseline, string diagnosticReportsDirectory, BundleInfo bundle) {
        ArgumentNullException.ThrowIfNull(bundle);
        return toSeq(Reconcile.Reports(diagnosticReportsDirectory, bundle.CrashReportPattern)
                .Filter(path => !baseline.Exists(known => string.Equals(known, path, StringComparison.Ordinal)))
                .OrderByDescending(LastWriteUnixMs))
            .Head.Map(Parse);
    }

    internal static Fin<CargoManifest> Stage(string payloadRoot, Guid sessionId, string reportDir, string stageRoot) {
        try {
            string cargoRoot = Path.Combine(payloadRoot, "cargo");
            string scenariosRoot = Path.Combine(payloadRoot, CargoManifest.ScenariosDirectory);
            if (!File.Exists(Path.Combine(cargoRoot, CargoManifest.AssemblyFile))) {
                return Error.New($"bridge cargo is absent from '{cargoRoot}'");
            }
            Seq<(string Source, string Relative)> files = Slot(cargoRoot, string.Empty) + Slot(scenariosRoot, CargoManifest.ScenariosDirectory);
            if (!files.Exists(static file => file.Relative.StartsWith(CargoManifest.ScenariosDirectory, StringComparison.Ordinal) && file.Relative.EndsWith(".dll", StringComparison.Ordinal))) {
                return Error.New($"no scenario assembly is staged under '{scenariosRoot}'");
            }
            string contentHash = Hash(files);
            string stagePath = Path.Combine(stageRoot, contentHash);
            _ = files.Iter(file => CopyFresh(file.Source, Path.Combine(stagePath, file.Relative)));
            return new CargoManifest(sessionId, reportDir, contentHash, stagePath);
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return Error.New($"bridge payload staging failed: {error.Message}");
        }
    }

    private static Seq<(string Source, string Relative)> Slot(string root, string prefix) =>
        !Directory.Exists(root)
            ? Seq<(string, string)>()
            : toSeq(Directory.EnumerateFiles(root)
                .OrderBy(Path.GetFileName, StringComparer.Ordinal)
                .Select(path => (path, prefix.Length == 0 ? Path.GetFileName(path) : Path.Combine(prefix, Path.GetFileName(path)))));

    private static string Relative(string reportDir, string path) =>
        Path.GetRelativePath(reportDir, path).Replace(Path.DirectorySeparatorChar, '/');

    private static bool IsEvidenceFile(string relative) =>
        !relative.EndsWith(".tmp", StringComparison.Ordinal)
        && relative.Split('/', 2)[0] is ReportLayout.EventsDirectory or ReportLayout.CapturesDirectory or ReportLayout.Gh2Directory or ReportLayout.ScratchDirectory;

    private static ArtifactRef ArtifactRef(string reportDir, string relative) {
        string scenario = relative.Split('/', StringSplitOptions.RemoveEmptyEntries) is [_, string name, ..] ? Path.GetFileNameWithoutExtension(name) : string.Empty;
        return Contract.ArtifactRef.Index(reportDir, Path.Combine(reportDir, relative), Role(relative), scenario,
            relative.Contains("failure", StringComparison.OrdinalIgnoreCase));
    }

    private static EvidenceRole Role(string relative) =>
        relative.Split('/', 2)[0] switch {
            ReportLayout.EventsDirectory => EvidenceRole.Spool,
            ReportLayout.CapturesDirectory => EvidenceRole.Capture,
            ReportLayout.Gh2Directory => EvidenceRole.Gh2CanvasManifest,
            ReportLayout.ScratchDirectory => EvidenceRole.Scratch,
            _ => EvidenceRole.Artifact,
        };

    private static void CopyFresh(string source, string target) {
        _ = Directory.CreateDirectory(Path.GetDirectoryName(target) ?? ".");
        if (!File.Exists(target)) {
            File.Copy(source, target);
        } else if (!File.ReadAllBytes(source).AsSpan().SequenceEqual(File.ReadAllBytes(target))) {
            throw new IOException($"staged file conflict for '{Path.GetFileName(source)}'");
        }
    }

    private static Option<BridgeEvent> Decode(string line) {
        try {
            return Optional(JsonSerializer.Deserialize(line, BridgeJsonContext.Default.BridgeEvent));
        } catch (JsonException) {
            return Option<BridgeEvent>.None;
        }
    }

    private static string Hash(Seq<(string Source, string Relative)> files) {
        XxHash3 hasher = new();
        _ = files.Iter(file => {
            hasher.Append(Encoding.UTF8.GetBytes(file.Relative));
            hasher.Append(File.ReadAllBytes(file.Source));
        });
        return Convert.ToHexStringLower(hasher.GetCurrentHash());
    }

    private static long LastWriteUnixMs(string path) {
        try {
            return new DateTimeOffset(File.GetLastWriteTimeUtc(path)).ToUnixTimeMilliseconds();
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return 0L;
        }
    }

    private static Option<JsonElement> Member(JsonElement root, string name) =>
        root.TryGetProperty(name, out JsonElement member) ? member : Option<JsonElement>.None;

    internal static CrashFact Parse(string path) {
        try {
            string raw = File.ReadAllText(path);
            using JsonDocument body = JsonDocument.Parse(raw[(raw.IndexOf('\n', StringComparison.Ordinal) + 1)..]);
            JsonElement root = body.RootElement;
            int faulting = Member(root, "faultingThread").Case is JsonElement thread && thread.TryGetInt32(out int index) ? index : -1;
            string crashThread = Member(root, "threads").Case is JsonElement threads
                    && threads.ValueKind == JsonValueKind.Array && faulting >= 0 && faulting < threads.GetArrayLength()
                ? ThreadName(threads[faulting], faulting)
                : "unknown";
            string exceptionType = Member(root, "exception").Bind(exception => Member(exception, "type")).Case is JsonElement type
                ? type.GetString() ?? "unknown"
                : "unknown";
            return new CrashFact(path, crashThread, exceptionType, string.Empty);
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException or ArgumentOutOfRangeException) {
            return new CrashFact(path, "unknown", "unknown", string.Empty);
        }
    }

    private static string ThreadName(JsonElement thread, int index) =>
        Member(thread, "name").Case is JsonElement name && name.GetString() is { Length: > 0 } named ? named
            : Member(thread, "queue").Case is JsonElement queue && queue.GetString() is { Length: > 0 } queued ? queued
                : string.Create(CultureInfo.InvariantCulture, $"thread {index}");
}

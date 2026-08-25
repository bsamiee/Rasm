using System.Globalization;
using System.IO.Hashing;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Rasm.Bridge.Contract;

namespace Rasm.Bridge.Supervisor;

// --- [MODELS] --------------------------------------------------------------------------

internal sealed record CrashSummary(string Thread, string ExceptionType, string ReportPath);

// --- [OPERATIONS] ----------------------------------------------------------------------

internal static class Evidence {
    internal static Option<string> GcDump(int pid, string reportDir, TimeSpan deadline) {
        string artifact = Path.Combine(path1: reportDir, path2: string.Create(provider: CultureInfo.InvariantCulture, $"{pid}.gcdump"));
        return Exec.Run(file: "dotnet",
                args: ["tool", "run", "dotnet-gcdump", "--", "collect", "-p", pid.ToString(provider: CultureInfo.InvariantCulture), "-o", artifact],
                deadline: deadline) is Fin<ExecResult>.Succ(ExecResult collected) && collected.ExitCode == 0 && File.Exists(path: artifact)
            ? Some(value: artifact)
            : Option<string>.None;
    }

    internal static Seq<BridgeEvent> HarvestSpool(string reportDir, string scenario) {
        string path = ReportLayout.Spool(reportDir: reportDir, scenario: scenario);
        try {
            return !File.Exists(path: path)
                ? Seq<BridgeEvent>()
                : toSeq(value: File.ReadLines(path: path)).Choose(selector: static line => Decode(line: line));
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return Seq<BridgeEvent>();
        }
    }

    internal static (long Count, long LastSequence) SpoolTail(string reportDir) {
        Seq<string> files;
        try {
            string events = Path.Combine(path1: reportDir, path2: ReportLayout.EventsDirectory);
            files = Directory.Exists(path: events)
                ? toSeq(value: Directory.EnumerateFiles(path: events, searchPattern: "*.jsonl"))
                : Seq<string>();
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            files = Seq<string>();
        }
        Seq<BridgeEvent> harvested = files
            .Bind(f: path => HarvestSpool(reportDir: reportDir, scenario: Path.GetFileNameWithoutExtension(path: path)))
            .Filter(f: static evt => evt is BridgeEvent.FactCase or BridgeEvent.CaptureCase);
        return (harvested.Count, harvested.Map(f: static evt => evt.Stamp.Sequence)
            .Fold(initialState: 0L, f: static (max, observed) => Math.Max(val1: max, val2: observed)));
    }

    internal static ArtifactRef[] ArtifactRefs(string reportDir) {
        try {
            return !Directory.Exists(path: reportDir)
                ? []
                : [.. Directory.EnumerateFiles(path: reportDir, searchPattern: "*", searchOption: SearchOption.AllDirectories)
                .Where(predicate: path => EvidenceFile(reportDir: reportDir, path: path))
                .Order(comparer: StringComparer.Ordinal)
                .Select(selector: path => ArtifactRef(reportDir: reportDir, path: path))];
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return [];
        }
    }

    internal static string WriteCertificate(string reportDir, EvidenceCertificate certificate) {
        string path = ReportLayout.Certificate(reportDir: reportDir);
        _ = Directory.CreateDirectory(path: reportDir);
        string temp = Path.Combine(path1: reportDir, path2: $".{Guid.NewGuid():N}.tmp");
        File.WriteAllText(path: temp, contents: JsonSerializer.Serialize(value: certificate, jsonTypeInfo: BridgeJsonContext.Default.EvidenceCertificate));
        try {
            File.Move(sourceFileName: temp, destFileName: path, overwrite: true);
        } catch (FileNotFoundException) when (File.Exists(path: temp)) {
            File.Copy(sourceFileName: temp, destFileName: path, overwrite: true);
            File.Delete(path: temp);
        }
        return path;
    }

    internal static Seq<string> IpsBaseline(BundleInfo bundle) {
        ArgumentNullException.ThrowIfNull(argument: bundle);
        try {
            return Directory.Exists(path: ReportsDirectory)
                ? toSeq(value: Directory.GetFiles(path: ReportsDirectory, searchPattern: bundle.CrashReportPattern))
                : Seq<string>();
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return Seq<string>();
        }
    }

    internal static Option<CrashSummary> IpsDiff(Seq<string> baseline, BundleInfo bundle) {
        Seq<string> landed = IpsBaseline(bundle: bundle).Filter(f: path => !baseline.Exists(f: known => string.Equals(a: known, b: path, comparisonType: StringComparison.Ordinal)));
        return toSeq(value: landed.OrderByDescending(keySelector: LastWriteUnixMs)).Head.Case is string newest
            ? Some(value: Parse(path: newest))
            : Option<CrashSummary>.None;
    }

    internal static Fin<CargoManifest> Stage(string payloadRoot, Guid sessionId, string reportDir, string refsRoot) {
        try {
            Seq<string> assemblies = Directory.Exists(path: payloadRoot)
                ? toSeq(value: Directory.EnumerateFiles(path: payloadRoot, searchPattern: "*.dll"))
                : Seq<string>();
            if (!assemblies.Exists(f: static path => string.Equals(a: Path.GetFileName(path: path), b: "Rasm.Bridge.Cargo.dll", comparisonType: StringComparison.Ordinal)))
                return Fin.Fail<CargoManifest>(error: Error.New(message: $"bridge cargo is absent from '{payloadRoot}'"));
            string contentHash = Hash(assemblies: toSeq(value: assemblies.OrderBy(keySelector: static path => Path.GetFileName(path: path), comparer: StringComparer.Ordinal)));
            string stagePath = Path.Combine(path1: refsRoot, path2: contentHash);
            _ = Directory.CreateDirectory(path: stagePath);
            _ = assemblies.Iter(f: source => CopyFresh(source: source, stagePath: stagePath));
            return Fin.Succ(value: new CargoManifest(
                SessionId: sessionId, ReportDir: reportDir, ContentHash: contentHash, StagePath: stagePath));
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return Fin.Fail<CargoManifest>(error: Error.New(message: $"bridge payload staging failed: {error.Message}"));
        }
    }

    private static string ReportsDirectory =>
        Path.Combine(path1: Environment.GetFolderPath(folder: Environment.SpecialFolder.UserProfile), path2: "Library", path3: "Logs", path4: "DiagnosticReports");

    private static bool EvidenceFile(string reportDir, string path) {
        string relative = Path.GetRelativePath(relativeTo: reportDir, path: path).Replace(oldChar: Path.DirectorySeparatorChar, newChar: '/');
        if (relative.EndsWith(value: ".tmp", comparisonType: StringComparison.Ordinal) || string.Equals(a: relative, b: ReportLayout.CertificateFile, comparisonType: StringComparison.Ordinal)) {
            return false;
        }
        if (!relative.Contains(value: '/', comparisonType: StringComparison.Ordinal) && relative.EndsWith(value: ".gcdump", comparisonType: StringComparison.OrdinalIgnoreCase)) {
            return true;
        }
        string root = relative.Split(separator: '/', count: 2)[0];
        return root is ReportLayout.EventsDirectory or ReportLayout.CapturesDirectory or ReportLayout.Gh2Directory
            or ReportLayout.ManifestsDirectory or ReportLayout.ScratchDirectory;
    }

    private static ArtifactRef ArtifactRef(string reportDir, string path) {
        string relative = Path.GetRelativePath(relativeTo: reportDir, path: path).Replace(oldChar: Path.DirectorySeparatorChar, newChar: '/');
        string scenario = relative.Split(separator: '/', options: StringSplitOptions.RemoveEmptyEntries) is [_, var name, ..]
            ? Path.GetFileNameWithoutExtension(path: name)
            : string.Empty;
        return new ArtifactRef(
            Id: relative, Role: Role(relative: relative), RelativePath: relative, MediaType: Media(path: path),
            Bytes: Size(path: path), Hash: HashFile(path: path), Retention: Retention(relative: relative),
            Scenario: scenario, OnFailure: relative.Contains(value: "failure", comparisonType: StringComparison.Ordinal));
    }

    private static EvidenceRole Role(string relative) =>
        !relative.Contains(value: '/', comparisonType: StringComparison.Ordinal) && relative.EndsWith(value: ".gcdump", comparisonType: StringComparison.OrdinalIgnoreCase) ? EvidenceRole.Forensic :
        relative.Split(separator: '/', count: 2)[0] switch {
            ReportLayout.EventsDirectory => EvidenceRole.Spool,
            ReportLayout.CapturesDirectory => EvidenceRole.Capture,
            ReportLayout.Gh2Directory => EvidenceRole.Gh2CanvasManifest,
            ReportLayout.ManifestsDirectory => relative.Contains(value: "geometry", comparisonType: StringComparison.Ordinal) ? EvidenceRole.GeometryManifest
                : relative.Contains(value: "viewport", comparisonType: StringComparison.Ordinal) ? EvidenceRole.ViewportManifest
                : EvidenceRole.ObjectManifest,
            ReportLayout.ScratchDirectory => EvidenceRole.Scratch,
            _ => EvidenceRole.Artifact,
        };

    private static ArtifactRetentionClass Retention(string relative) =>
        !relative.Contains(value: '/', comparisonType: StringComparison.Ordinal) && relative.EndsWith(value: ".gcdump", comparisonType: StringComparison.OrdinalIgnoreCase) ? ArtifactRetentionClass.Forensic :
        relative.Split(separator: '/', count: 2)[0] switch {
            ReportLayout.ScratchDirectory => ArtifactRetentionClass.Scratch,
            ReportLayout.CapturesDirectory or ReportLayout.Gh2Directory when relative.Contains(value: "failure", comparisonType: StringComparison.Ordinal) => ArtifactRetentionClass.Forensic,
            _ => ArtifactRetentionClass.Evidence,
        };

    private static string Media(string path) =>
        Path.GetExtension(path: path).ToUpperInvariant() switch {
            ".PNG" => "image/png",
            ".JSON" => "application/json",
            ".JSONL" => "application/x-ndjson",
            ".GCDUMP" => "application/octet-stream",
            _ => "application/octet-stream",
        };

    private static long Size(string path) {
        try {
            return new FileInfo(fileName: path).Length;
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return 0L;
        }
    }

    private static ArtifactHash HashFile(string path) {
        try {
            return new ArtifactHash(Algorithm: "sha256", Value: Convert.ToHexStringLower(inArray: SHA256.HashData(source: File.ReadAllBytes(path: path))));
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return new ArtifactHash(Algorithm: "sha256", Value: string.Empty);
        }
    }

    private static void CopyFresh(string source, string stagePath) {
        Seq<string> sources = string.Equals(a: Path.GetExtension(path: source), b: ".dll", comparisonType: StringComparison.OrdinalIgnoreCase)
            ? toSeq(value: [source, Path.ChangeExtension(path: source, extension: ".deps.json"), Path.ChangeExtension(path: source, extension: ".runtimeconfig.json"), Path.ChangeExtension(path: source, extension: ".pdb"), Path.ChangeExtension(path: source, extension: ".xml")])
            : Seq(source);
        _ = sources.Filter(f: File.Exists).Iter(f: candidate => {
            string target = Path.Combine(path1: stagePath, path2: Path.GetFileName(path: candidate));
            if (!File.Exists(path: target)) {
                File.Copy(sourceFileName: candidate, destFileName: target);
            } else if (!File.ReadAllBytes(path: candidate).SequenceEqual(second: File.ReadAllBytes(path: target))) {
                throw new IOException(message: $"staged file conflict for '{Path.GetFileName(path: candidate)}'");
            }
        });
    }

    private static Option<BridgeEvent> Decode(string line) {
        try {
            return JsonSerializer.Deserialize(json: line, jsonTypeInfo: BridgeJsonContext.Default.BridgeEvent) is { } evt
                ? Some(value: evt)
                : Option<BridgeEvent>.None;
        } catch (JsonException) {
            return Option<BridgeEvent>.None;
        }
    }

    private static string Hash(Seq<string> assemblies) {
        XxHash3 hasher = new();
        _ = assemblies.Iter(f: path => {
            hasher.Append(source: Encoding.UTF8.GetBytes(s: Path.GetFileName(path: path)));
            hasher.Append(source: File.ReadAllBytes(path: path));
        });
        return Convert.ToHexStringLower(inArray: hasher.GetCurrentHash());
    }

    private static long LastWriteUnixMs(string path) {
        try {
            return new DateTimeOffset(dateTime: File.GetLastWriteTimeUtc(path: path)).ToUnixTimeMilliseconds();
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return 0L;
        }
    }

    private static Option<JsonElement> Member(JsonElement root, string name) =>
        root.TryGetProperty(propertyName: name, value: out JsonElement member) ? Some(value: member) : Option<JsonElement>.None;

    private static CrashSummary Parse(string path) {
        try {
            string raw = File.ReadAllText(path: path);
            int split = raw.IndexOf(value: '\n', comparisonType: StringComparison.Ordinal);
            using JsonDocument body = JsonDocument.Parse(json: raw[(split + 1)..]);
            JsonElement root = body.RootElement;
            int faulting = Member(root: root, name: "faultingThread").Case is JsonElement thread && thread.TryGetInt32(value: out int index) ? index : -1;
            string crashThread = Member(root: root, name: "threads").Case is JsonElement threads
                    && threads.ValueKind == JsonValueKind.Array && faulting >= 0 && faulting < threads.GetArrayLength()
                ? ThreadName(thread: threads[index: faulting], index: faulting)
                : "unknown";
            string exceptionType = Member(root: root, name: "exception").Case is JsonElement exception
                    && Member(root: exception, name: "type").Case is JsonElement type
                ? type.GetString() ?? "unknown"
                : "unknown";
            return new CrashSummary(Thread: crashThread, ExceptionType: exceptionType, ReportPath: path);
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException or ArgumentOutOfRangeException) {
            return new CrashSummary(Thread: "unknown", ExceptionType: "unknown", ReportPath: path);
        }
    }

    private static string ThreadName(JsonElement thread, int index) =>
        Member(root: thread, name: "name").Case is JsonElement name && name.GetString() is { Length: > 0 } named ? named
            : Member(root: thread, name: "queue").Case is JsonElement queue && queue.GetString() is { Length: > 0 } queued ? queued
                : string.Create(provider: CultureInfo.InvariantCulture, $"thread {index}");
}

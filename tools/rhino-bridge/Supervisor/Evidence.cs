using System.Globalization;
using System.IO.Hashing;
using System.Text;
using System.Text.Json;
using Rasm.Bridge.Contract;

namespace Rasm.Bridge.Supervisor;

// --- [MODELS] -----------------------------------------------------------------------------

// Ownership: supervisor-private crash summary; unparseable reports degrade to raw paths.
internal sealed record CrashSummary(string Thread, string ExceptionType, string ReportPath);

// Ownership: build-time closure manifest; supervisor reads it without invoking build tooling.
internal sealed record ClosureManifest(string[] Assemblies, Guid[] HostPlugins, HostFingerprint BuiltAgainst, string[] ScenarioAssemblies);

// --- [OPERATIONS] -------------------------------------------------------------------------

// Ownership: workstation-side evidence outside the session fold: staged refs, .ips summaries,
// spool harvest, and unload-leak gcdumps.
internal static class Evidence {
    // Best-effort unload-leak forensics. The collect runs under the forensics deadline; a deadline
    // elapse (Exec kills the child tree and returns Fin.Fail), a non-zero exit, or a missing artifact
    // all project to None, so the caller's after-leak recycle fact stays the host-recycle quit ladder
    // and never blocks or faults on an unavailable dump.
    internal static Option<string> GcDump(int pid, string reportDir, TimeSpan deadline) {
        string artifact = Path.Combine(path1: reportDir, path2: string.Create(provider: CultureInfo.InvariantCulture, $"{pid}.gcdump"));
        return Exec.Run(file: "dotnet",
                args: ["tool", "run", "dotnet-gcdump", "--", "collect", "-p", pid.ToString(provider: CultureInfo.InvariantCulture), "-o", artifact],
                deadline: deadline) is Fin<ExecResult>.Succ(ExecResult collected) && collected.ExitCode == 0 && File.Exists(path: artifact)
            ? Some(value: artifact)
            : Option<string>.None;
    }

    internal static Seq<BridgeEvent> HarvestSpool(string reportDir, string scenario) {
        // BOUNDARY ADAPTER: truncated JSONL tails drop while prior spool facts survive.
        string path = Path.Combine(path1: reportDir, path2: scenario + ".jsonl");
        try {
            return !File.Exists(path: path)
                ? Seq<BridgeEvent>()
                : toSeq(value: File.ReadLines(path: path)).Choose(selector: static line => Decode(line: line));
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return Seq<BridgeEvent>();
        }
    }

    internal static Seq<string> IpsBaseline(BundleInfo bundle) {
        ArgumentNullException.ThrowIfNull(argument: bundle);
        // BOUNDARY ADAPTER: absent or denied crash-report access yields an empty baseline.
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

    internal static Fin<CargoManifest> Stage(string closureManifest, Guid sessionId, string reportDir, string refsRoot) {
        // BOUNDARY ADAPTER: closure read, hash, and copy fail as one typed precondition.
        try {
            ClosureManifest? closure = JsonSerializer.Deserialize(
                json: File.ReadAllText(path: closureManifest), jsonTypeInfo: SupervisorJsonContext.Default.ClosureManifest);
            if (closure is null)
                return Fin.Fail<CargoManifest>(error: Error.New(message: $"closure manifest decoded to null: {closureManifest}"));
            if (closure.Assemblies is not { Length: > 0 })
                return Fin.Fail<CargoManifest>(error: Error.New(message: $"closure manifest contains no assemblies: {closureManifest}"));
            string root = Path.GetDirectoryName(path: Path.GetFullPath(path: closureManifest)) ?? ".";
            Seq<string> assemblies = toSeq(value: closure.Assemblies)
                .Map(f: path => Path.IsPathRooted(path: path) ? path : Path.Combine(path1: root, path2: path));
            Seq<string> missing = assemblies.Filter(f: path => !File.Exists(path: path));
            if (!missing.IsEmpty)
                return Fin.Fail<CargoManifest>(error: Error.New(message: $"closure assemblies absent: {string.Join(separator: ", ", values: missing)}"));
            string contentHash = Hash(assemblies: toSeq(value: assemblies.OrderBy(keySelector: static path => Path.GetFileName(path: path), comparer: StringComparer.Ordinal)));
            string stagePath = Path.Combine(path1: refsRoot, path2: contentHash);
            _ = Directory.CreateDirectory(path: stagePath);
            _ = assemblies.Iter(f: source => CopyFresh(source: source, stagePath: stagePath));
            return Fin.Succ(value: new CargoManifest(
                SessionId: sessionId, ReportDir: reportDir, ContentHash: contentHash, StagePath: stagePath,
                HostPlugins: closure.HostPlugins ?? [], BuiltAgainst: closure.BuiltAgainst,
                ScenarioAssemblies: closure.ScenarioAssemblies ?? []));
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException) {
            return Fin.Fail<CargoManifest>(error: Error.New(message: $"closure staging failed: {error.Message}"));
        }
    }

    private static string ReportsDirectory =>
        Path.Combine(path1: Environment.GetFolderPath(folder: Environment.SpecialFolder.UserProfile), path2: "Library", path3: "Logs", path4: "DiagnosticReports");

    private static void CopyFresh(string source, string stagePath) {
        // Existing staged files under the same hash must be byte-identical.
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

    // macOS .ips files have a summary-header line followed by JSON crash details.
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
            // Preserve the raw report path even when structured forensics degrade.
            return new CrashSummary(Thread: "unknown", ExceptionType: "unknown", ReportPath: path);
        }
    }

    private static string ThreadName(JsonElement thread, int index) =>
        Member(root: thread, name: "name").Case is JsonElement name && name.GetString() is { Length: > 0 } named ? named
            : Member(root: thread, name: "queue").Case is JsonElement queue && queue.GetString() is { Length: > 0 } queued ? queued
                : string.Create(provider: CultureInfo.InvariantCulture, $"thread {index}");
}

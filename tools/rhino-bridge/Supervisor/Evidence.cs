using System.Globalization;
using System.IO.Hashing;
using System.Text;
using System.Text.Json;
using Rasm.Bridge.Contract;

namespace Rasm.Bridge.Supervisor;

// --- [MODELS] -----------------------------------------------------------------------------

// Ownership: supervisor-private parse result feeding the RhinoCrash fault case; unparseable
// reports degrade to raw paths ("unknown" fields carry the forensics-degraded signal at the
// fold), never a throw.
internal sealed record CrashSummary(string Thread, string ExceptionType, string ReportPath);

// Ownership: the decode shape of the build-time `bridge-closure.json` target (assembly list +
// host-plugin GUIDs + built-against fingerprint). The supervisor READS the closure — zero MSBuild
// evaluation, zero `dotnet build` children at invoke time.
internal sealed record ClosureManifest(string[] Assemblies, Guid[] HostPlugins, HostFingerprint BuiltAgainst);

// --- [OPERATIONS] -------------------------------------------------------------------------

// Ownership: every workstation-side evidence producer outside the session fold — content-hash
// staging (XxHash3 into refs/<hash>/), .ips snapshot/diff (RhinoCrashReportFinder port: crash
// thread + exception type from the macOS JSON report body), per-scenario spool harvest, and the
// dotnet-gcdump trigger for the unload-leak proof.
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
        // BOUNDARY ADAPTER — the spool is crash-durable JSONL; a truncated tail line (host died
        // mid-append) decodes to nothing and the facts-to-point-of-death before it survive.
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
        // BOUNDARY ADAPTER — pre-launch snapshot; absence or denial is an empty baseline.
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
        // BOUNDARY ADAPTER — closure read + hash + copy is one IO transaction; any miss is one
        // typed failure naming the missing precondition.
        try {
            ClosureManifest? closure = JsonSerializer.Deserialize(
                json: File.ReadAllText(path: closureManifest), jsonTypeInfo: SupervisorJsonContext.Default.ClosureManifest);
            if (closure is null)
                return Fin.Fail<CargoManifest>(error: Error.New(message: $"closure manifest decoded to null: {closureManifest}"));
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
                HostPlugins: closure.HostPlugins, BuiltAgainst: closure.BuiltAgainst));
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException) {
            return Fin.Fail<CargoManifest>(error: Error.New(message: $"closure staging failed: {error.Message}"));
        }
    }

    private static string ReportsDirectory =>
        Path.Combine(path1: Environment.GetFolderPath(folder: Environment.SpecialFolder.UserProfile), path2: "Library", path3: "Logs", path4: "DiagnosticReports");

    private static void CopyFresh(string source, string stagePath) {
        // Content-addressed: an existing staged file under the same hash is byte-identical.
        string target = Path.Combine(path1: stagePath, path2: Path.GetFileName(path: source));
        if (!File.Exists(path: target))
            File.Copy(sourceFileName: source, destFileName: target);
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

    // RhinoCrashReportFinder port: a macOS .ips is one summary-header line then a JSON body
    // carrying faultingThread + exception/termination taxonomy + per-thread frames.
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
            // forensics-degraded: the raw path still attaches; the fold names the degradation.
            return new CrashSummary(Thread: "unknown", ExceptionType: "unknown", ReportPath: path);
        }
    }

    private static string ThreadName(JsonElement thread, int index) =>
        Member(root: thread, name: "name").Case is JsonElement name && name.GetString() is { Length: > 0 } named ? named
            : Member(root: thread, name: "queue").Case is JsonElement queue && queue.GetString() is { Length: > 0 } queued ? queued
                : string.Create(provider: CultureInfo.InvariantCulture, $"thread {index}");
}

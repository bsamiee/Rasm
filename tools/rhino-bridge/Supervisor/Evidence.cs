using System.Globalization;
using System.IO.Hashing;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Rasm.Bridge.Contract;

namespace Rasm.Bridge.Supervisor;

// --- [MODELS] --------------------------------------------------------------------------

internal sealed record CrashSummary(string Thread, string ExceptionType, string ReportPath);

// Evidence mode rides argv (the single source of truth); the closure carries only reference roots.
internal sealed record ClosureManifest(string[] Assemblies, Guid[] HostPlugins, HostFingerprint BuiltAgainst, string[] ScenarioAssemblies) {
    public ReferenceRoot[] ReferenceRoots { get; init; } = [];
}

// Ownership: the staging result — the wire manifest plus the supervisor-side reference roots that
// never cross into the host.
internal sealed record StagedCargo(CargoManifest Manifest, ReferenceRoot[] ReferenceRoots);

internal readonly record struct ReferenceActual(string Scenario, EvidenceName Name, JsonElement Actual, ReferenceTolerance Tolerance);

// --- [OPERATIONS] ----------------------------------------------------------------------

// Ownership: workstation-side evidence outside the live host session fold. Report paths resolve
// through the Contract ReportLayout owner.
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

    internal static void EnsureReportFiles(string reportDir, Seq<ScenarioReceipt> receipts) {
        string references = Path.Combine(path1: reportDir, path2: ReportLayout.ReferencesDirectory);
        _ = Directory.CreateDirectory(path: references);
        foreach (ScenarioReceipt receipt in receipts) {
            string scenario = SafeScenario(name: receipt.Scenario);
            WriteJson(
                path: Path.Combine(path1: references, path2: $"{scenario}.reference-result.json"),
                value: receipt.ReferenceResults ?? [],
                jsonTypeInfo: BridgeJsonContext.Default.ReferenceEvidenceResultArray);
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

    internal static Fin<StagedCargo> Stage(string closureManifest, Guid sessionId, string reportDir, string refsRoot) {
        try {
            ClosureManifest? closure = JsonSerializer.Deserialize(
                json: File.ReadAllText(path: closureManifest), jsonTypeInfo: SupervisorJsonContext.Default.ClosureManifest);
            if (closure is null)
                return Fin.Fail<StagedCargo>(error: Error.New(message: $"closure manifest decoded to null: {closureManifest}"));
            if (closure.Assemblies is not { Length: > 0 })
                return Fin.Fail<StagedCargo>(error: Error.New(message: $"closure manifest contains no assemblies: {closureManifest}"));
            string root = Path.GetDirectoryName(path: Path.GetFullPath(path: closureManifest)) ?? ".";
            Seq<string> assemblies = toSeq(value: closure.Assemblies)
                .Map(f: path => Path.IsPathRooted(path: path) ? path : Path.Combine(path1: root, path2: path));
            Seq<string> missing = assemblies.Filter(f: path => !File.Exists(path: path));
            if (!missing.IsEmpty)
                return Fin.Fail<StagedCargo>(error: Error.New(message: $"closure assemblies absent: {string.Join(separator: ", ", values: missing)}"));
            string contentHash = Hash(assemblies: toSeq(value: assemblies.OrderBy(keySelector: static path => Path.GetFileName(path: path), comparer: StringComparer.Ordinal)));
            string stagePath = Path.Combine(path1: refsRoot, path2: contentHash);
            _ = Directory.CreateDirectory(path: stagePath);
            _ = assemblies.Iter(f: source => CopyFresh(source: source, stagePath: stagePath));
            return Fin.Succ(value: new StagedCargo(
                Manifest: new CargoManifest(
                    SessionId: sessionId, ReportDir: reportDir, ContentHash: contentHash, StagePath: stagePath,
                    HostPlugins: closure.HostPlugins ?? [], BuiltAgainst: closure.BuiltAgainst,
                    ScenarioAssemblies: closure.ScenarioAssemblies ?? []),
                ReferenceRoots: closure.ReferenceRoots ?? []));
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException) {
            return Fin.Fail<StagedCargo>(error: Error.New(message: $"closure staging failed: {error.Message}"));
        }
    }

    // The honest reference lifecycle: author-mode runs write candidate files beside the scenario
    // owner (reference root when configured), a human review promotes candidate -> reviewed under
    // <root>/<theme>/<method>.reference.json, and a verify run over a root with no reviewed corpus
    // reports Unpromoted rather than a structural failure.
    internal static ReferenceEvidenceResult[] ReferenceResults(
        EvidenceMode mode, ReferenceRoot[] roots, Seq<ScenarioReceipt> receipts, Seq<BridgeEvent> evidence, string reportDir) {
        ArgumentNullException.ThrowIfNull(argument: mode);
        ReferenceActual[] actuals = [.. evidence.Choose(selector: static evt =>
            evt is BridgeEvent.FactCase fact && EvidenceRole.Reference.OwnsFactKey(key: fact.Key)
                ? ReferenceActualOf(fact: fact)
                : Option<ReferenceActual>.None)];
        System.Collections.Generic.HashSet<string> covered = new(collection: actuals.Select(selector: static row => row.Scenario), comparer: StringComparer.Ordinal);
        bool promoted = Promoted(roots: roots);
        IEnumerable<ReferenceEvidenceResult> observed = mode == EvidenceMode.Author
            ? actuals.GroupBy(keySelector: static row => row.Scenario, comparer: StringComparer.Ordinal)
                .SelectMany(selector: group => CandidateResults(roots: roots, reportDir: reportDir, group: group))
            : actuals.Select(selector: row => MatchReference(roots: roots, actual: row, promoted: promoted));
        IEnumerable<ReferenceEvidenceResult> missing = mode == EvidenceMode.Author
            ? []
            : receipts.Filter(f: static row => row.Status == PhaseStatus.Ok)
                .Filter(f: receipt => !covered.Contains(item: receipt.Scenario))
                .Map(f: receipt => MissingReference(roots: roots, receipt: receipt, promoted: promoted));
        return [.. observed.Concat(second: missing).OrderBy(keySelector: static row => row.Scenario, comparer: StringComparer.Ordinal)
            .ThenBy(keySelector: static row => row.Name.Key, comparer: StringComparer.Ordinal)];

        static IEnumerable<ReferenceEvidenceResult> CandidateResults(ReferenceRoot[] roots, string reportDir, IGrouping<string, ReferenceActual> group) {
            string candidate = WriteCandidateReferences(path: CandidatePath(roots: roots, reportDir: reportDir, scenario: group.Key), actuals: [.. group]);
            return group.Select(selector: row => new ReferenceEvidenceResult(
                Name: row.Name, Class: EvidenceClass.CertifiedReference, Admission: ReferenceAdmission.Candidate, Matched: false,
                ReferencePath: candidate, Detail: "reference.candidate:review, set admission to reviewed, and rename to <method>.reference.json", Tolerance: row.Tolerance) {
                Scenario = row.Scenario,
            });
        }

        static ReferenceEvidenceResult MissingReference(ReferenceRoot[] roots, ScenarioReceipt receipt, bool promoted) =>
            new(Name: new EvidenceName(Key: "scenario.reference"), Class: EvidenceClass.CertifiedReference,
                Admission: promoted ? ReferenceAdmission.Missing : ReferenceAdmission.Unpromoted, Matched: false,
                ReferencePath: ReferencePath(roots: roots, scenario: receipt.Scenario),
                Detail: promoted ? "reference.missing:no reference facts emitted" : "reference.unpromoted:no reviewed corpus under the reference root; run --evidence author, review, promote",
                Tolerance: ExactTolerance()) {
                Scenario = receipt.Scenario,
            };
    }

    private static string ReportsDirectory =>
        Path.Combine(path1: Environment.GetFolderPath(folder: Environment.SpecialFolder.UserProfile), path2: "Library", path3: "Logs", path4: "DiagnosticReports");

    private static Option<ReferenceActual> ReferenceActualOf(BridgeEvent.FactCase fact) {
        string scenario = fact.Stamp.Scenario ?? string.Empty;
        if (scenario.Length == 0) {
            return Option<ReferenceActual>.None;
        }
        string key = EvidenceRole.Reference.FactArgument(key: fact.Key);
        JsonElement actual = fact.Value.Clone();
        ReferenceTolerance tolerance = ExactTolerance();
        if (fact.Value.ValueKind == JsonValueKind.Object) {
            if (fact.Value.TryGetProperty(propertyName: "name", value: out JsonElement name)
                && name.GetString() is { Length: > 0 } named) {
                key = named;
            }
            if (fact.Value.TryGetProperty(propertyName: "actual", value: out JsonElement observed)) {
                actual = observed.Clone();
            }
            if (fact.Value.TryGetProperty(propertyName: "tolerance", value: out JsonElement toleranceElement)) {
                try {
                    tolerance = toleranceElement.Deserialize(jsonTypeInfo: BridgeJsonContext.Default.ReferenceTolerance);
                } catch (JsonException) {
                    tolerance = ExactTolerance();
                }
            }
        }
        return Some(value: new ReferenceActual(
            Scenario: scenario, Name: new EvidenceName(Key: key), Actual: actual, Tolerance: tolerance));
    }

    private static ReferenceEvidenceResult MatchReference(ReferenceRoot[] roots, ReferenceActual actual, bool promoted) {
        string path = ReferencePath(roots: roots, scenario: actual.Scenario);
        if (!File.Exists(path: path)) {
            return promoted
                ? Fail(admission: ReferenceAdmission.Missing, detail: "reference.missing")
                : Fail(admission: ReferenceAdmission.Unpromoted, detail: "reference.unpromoted:no reviewed corpus under the reference root; run --evidence author, review, promote");
        }
        Fin<ReferenceEvidence[]> loaded = ReadReferences(path: path);
        if (loaded is Fin<ReferenceEvidence[]>.Fail) {
            return Fail(admission: ReferenceAdmission.Mismatch, detail: "reference.decode.failed");
        }
        ReferenceEvidence[] expectedRows = loaded is Fin<ReferenceEvidence[]>.Succ(ReferenceEvidence[] rows) ? rows : [];
        ReferenceEvidence? expected = expectedRows.FirstOrDefault(predicate: row =>
            string.Equals(a: row.Name.Key, b: actual.Name.Key, comparisonType: StringComparison.Ordinal));
        if (expected is null) {
            return Fail(admission: ReferenceAdmission.Missing, detail: "reference.missing:key");
        }
        if (expected.Admission != ReferenceAdmission.Reviewed) {
            return Fail(admission: ReferenceAdmission.Candidate, detail: "reference.not-reviewed");
        }
        bool matched = Equivalent(expected: expected.Expected, actual: actual.Actual, tolerance: expected.Tolerance);
        return ReferenceResult(
            actual: actual, admission: matched ? ReferenceAdmission.Matched : ReferenceAdmission.Mismatch,
            matched: matched, path: path, detail: matched ? "reference.matched" : "reference.mismatch");

        ReferenceEvidenceResult Fail(ReferenceAdmission admission, string detail) =>
            ReferenceResult(actual: actual, admission: admission, matched: false, path: path, detail: detail);
    }

    private static ReferenceEvidenceResult ReferenceResult(
        ReferenceActual actual, ReferenceAdmission admission, bool matched, string path, string detail) =>
        new(Name: actual.Name, Class: EvidenceClass.CertifiedReference, Admission: admission, Matched: matched,
            ReferencePath: path, Detail: detail, Tolerance: actual.Tolerance) {
            Scenario = actual.Scenario,
        };

    private static string WriteCandidateReferences(string path, ReferenceActual[] actuals) {
        ReferenceEvidence[] rows = [.. actuals.Select(selector: static actual => new ReferenceEvidence(
            Name: actual.Name, Class: EvidenceClass.CertifiedReference, Expected: actual.Actual,
            Tolerance: actual.Tolerance, Admission: ReferenceAdmission.Candidate,
            ReviewedBy: "author", ReviewedAt: DateTimeOffset.UtcNow.ToString(format: "O", formatProvider: CultureInfo.InvariantCulture)))];
        WriteJson(path: path, value: rows, jsonTypeInfo: BridgeJsonContext.Default.ReferenceEvidenceArray);
        return path;
    }

    private static Fin<ReferenceEvidence[]> ReadReferences(string path) {
        try {
            using JsonDocument document = JsonDocument.Parse(json: File.ReadAllText(path: path));
            JsonElement root = document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty(propertyName: "references", value: out JsonElement references)
                    ? references
                    : document.RootElement;
            ReferenceEvidence[]? rows = root.ValueKind == JsonValueKind.Array
                ? JsonSerializer.Deserialize(json: root.GetRawText(), jsonTypeInfo: BridgeJsonContext.Default.ReferenceEvidenceArray)
                : JsonSerializer.Deserialize(json: root.GetRawText(), jsonTypeInfo: BridgeJsonContext.Default.ReferenceEvidence) is { } row ? [row] : null;
            return rows is { }
                ? Fin.Succ(value: rows)
                : Fin.Fail<ReferenceEvidence[]>(error: Error.New(message: "reference decoded to null"));
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException) {
            return Fin.Fail<ReferenceEvidence[]>(error: Error.New(message: error.Message));
        }
    }

    // Root resolution prefers an exact theme row, then the theme-less catch-all row assay ships;
    // the theme-keyed miss over a catch-all root was the structural fault that made verify-mode
    // unsatisfiable regardless of promoted references.
    private static string RootPath(ReferenceRoot[] roots, string theme) {
        string themed = roots.FirstOrDefault(predicate: row =>
            string.Equals(a: row.Theme, b: theme, comparisonType: StringComparison.Ordinal)).Path;
        string catchAll = roots.FirstOrDefault(predicate: static row =>
            row.Theme is null or { Length: 0 } && row.Path is { Length: > 0 }).Path;
        return themed is { Length: > 0 } ? themed : catchAll is { Length: > 0 } ? catchAll : string.Empty;
    }

    private static bool Promoted(ReferenceRoot[] roots) {
        // BOUNDARY ADAPTER: unreadable roots read as unpromoted.
        try {
            return roots.Select(selector: static row => row.Path).Where(predicate: static path => path is { Length: > 0 })
                .Distinct(comparer: StringComparer.Ordinal)
                .Any(predicate: static root => Directory.Exists(path: root)
                    && Directory.EnumerateFiles(path: root, searchPattern: "*.reference.json", searchOption: SearchOption.AllDirectories)
                        .Any(predicate: static file => !file.EndsWith(value: ".candidate.reference.json", comparisonType: StringComparison.Ordinal)));
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return false;
        }
    }

    private static string ReferencePath(ReferenceRoot[] roots, string scenario) {
        (string theme, string method) = ScenarioParts(scenario: scenario);
        string root = RootPath(roots: roots, theme: theme);
        string referenceRoot = root.Length > 0 ? root : Path.Combine(path1: ".", path2: "_references");
        return Path.Combine(path1: referenceRoot, path2: theme, path3: method + ".reference.json");
    }

    private static string CandidatePath(ReferenceRoot[] roots, string reportDir, string scenario) {
        (string theme, string method) = ScenarioParts(scenario: scenario);
        string root = RootPath(roots: roots, theme: theme);
        return root.Length > 0
            ? Path.Combine(path1: root, path2: theme, path3: method + ".candidate.reference.json")
            : Path.Combine(path1: reportDir, path2: ReportLayout.ReferencesDirectory, path3: $"{SafeScenario(name: scenario)}.candidate.reference.json");
    }

    private static (string Theme, string Method) ScenarioParts(string scenario) {
        int split = scenario.IndexOf(value: '.', comparisonType: StringComparison.Ordinal);
        return split <= 0 || split == scenario.Length - 1
            ? (scenario, scenario)
            : (scenario[..split], scenario[(split + 1)..]);
    }

    private static ReferenceTolerance ExactTolerance() =>
        new(Mode: "exact", Absolute: 0.0, Relative: 0.0);

    private static bool Equivalent(JsonElement expected, JsonElement actual, ReferenceTolerance tolerance) {
        return expected.ValueKind == actual.ValueKind && (expected.ValueKind switch {
            JsonValueKind.Number => NumberEquivalent(expected: expected, actual: actual, tolerance: tolerance),
            JsonValueKind.String => string.Equals(a: expected.GetString(), b: actual.GetString(), comparisonType: StringComparison.Ordinal),
            JsonValueKind.True or JsonValueKind.False => expected.GetBoolean() == actual.GetBoolean(),
            JsonValueKind.Null => true,
            JsonValueKind.Array => ArrayEquivalent(expected: expected, actual: actual, tolerance: tolerance),
            JsonValueKind.Object => ObjectEquivalent(expected: expected, actual: actual, tolerance: tolerance),
            _ => string.Equals(a: expected.GetRawText(), b: actual.GetRawText(), comparisonType: StringComparison.Ordinal),
        });
    }

    private static bool NumberEquivalent(JsonElement expected, JsonElement actual, ReferenceTolerance tolerance) {
        if (!expected.TryGetDouble(value: out double left) || !actual.TryGetDouble(value: out double right)) {
            return string.Equals(a: expected.GetRawText(), b: actual.GetRawText(), comparisonType: StringComparison.Ordinal);
        }
        double limit = Math.Abs(value: tolerance.Absolute) + (Math.Abs(value: tolerance.Relative) * Math.Max(val1: Math.Abs(value: left), val2: Math.Abs(value: right)));
        return Math.Abs(value: left - right) <= limit;
    }

    private static bool ArrayEquivalent(JsonElement expected, JsonElement actual, ReferenceTolerance tolerance) {
        JsonElement[] leftRows = [.. expected.EnumerateArray()];
        JsonElement[] rightRows = [.. actual.EnumerateArray()];
        return leftRows.Length == rightRows.Length
            && leftRows.Zip(second: rightRows).All(predicate: pair => Equivalent(expected: pair.First, actual: pair.Second, tolerance: tolerance));
    }

    private static bool ObjectEquivalent(JsonElement expected, JsonElement actual, ReferenceTolerance tolerance) {
        Dictionary<string, JsonElement> left = expected.EnumerateObject().ToDictionary(keySelector: static property => property.Name, elementSelector: static property => property.Value, comparer: StringComparer.Ordinal);
        Dictionary<string, JsonElement> right = actual.EnumerateObject().ToDictionary(keySelector: static property => property.Name, elementSelector: static property => property.Value, comparer: StringComparer.Ordinal);
        return left.Count == right.Count
            && left.All(predicate: row => right.TryGetValue(key: row.Key, value: out JsonElement observed)
                && Equivalent(expected: row.Value, actual: observed, tolerance: tolerance));
    }

    private static string SafeScenario(string name) {
        string safe = new string(value: [.. name.Select(selector: static c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.' ? c : '-')])
            .Trim(trimChar: '-').ToUpperInvariant();
        return safe.Length > 0 ? safe : "SCENARIO";
    }

    private static void WriteJson<T>(string path, T value, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> jsonTypeInfo) {
        _ = Directory.CreateDirectory(path: Path.GetDirectoryName(path: path) ?? ".");
        string temp = path + ".tmp";
        File.WriteAllText(path: temp, contents: JsonSerializer.Serialize(value: value, jsonTypeInfo: jsonTypeInfo));
        File.Move(sourceFileName: temp, destFileName: path, overwrite: true);
    }

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
            or ReportLayout.ManifestsDirectory or ReportLayout.ReferencesDirectory or ReportLayout.ScratchDirectory;
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
            ReportLayout.ReferencesDirectory => EvidenceRole.Reference,
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

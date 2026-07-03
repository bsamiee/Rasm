using System.Collections.Frozen;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Xunit.Sdk;

namespace Rasm.TestKit;

// --- [TYPES] --------------------------------------------------------------------------------
// One call-shape family: each case is a DISTINCT substitution behavior — Canned repeats one value,
// FanOut walks its sequence across successive calls, Factory records its inner label payload-free.
// The generated Switch is the sole dispatch; async seams can a completed task via TValue itself.
[Union]
public abstract partial record Shape<TValue> {
    private Shape() { }
    public sealed record Canned(TValue Value) : Shape<TValue>;
    public sealed record FanOut(Seq<TValue> Values) : Shape<TValue>;
    public sealed record Factory(TValue Value, string InnerLabel = "<factory>.run") : Shape<TValue>;
}

// VariantPayload is the closed raw-or-encoded carrier: raw bytes forward verbatim, an object encodes
// through the writer's JsonTypeInfo — the two ingress modes of one wire payload, never two writers.
[Union]
public abstract partial record VariantPayload {
    private VariantPayload() { }
    public sealed record Raw(ReadOnlyMemory<byte> Bytes) : VariantPayload;
    public sealed record Encoded(object Value) : VariantPayload;
}

// --- [MODELS] -------------------------------------------------------------------------------
// One typed captured invocation: the resolution-site member and the caller-typed payload; a
// Factory substitution records its inner label with no payload, so absence is `None`, never null.
// Equality is reflection-free by design: LanguageExt trait resolution enumerates referenced
// assemblies, which faults in host-aware test processes whose RhinoCommon closure is unstaged.
public readonly record struct SeamCall<TArgs>(string Member, Option<TArgs> Payload) {
    public bool Equals(SeamCall<TArgs> other) =>
        string.Equals(a: Member, b: other.Member, comparisonType: StringComparison.Ordinal)
        && (Payload.Case, other.Payload.Case) switch {
            (null, null) => true,
            (TArgs left, TArgs right) => EqualityComparer<TArgs>.Default.Equals(x: left, y: right),
            _ => false,
        };

    public override int GetHashCode() =>
        HashCode.Combine(
            value1: StringComparer.Ordinal.GetHashCode(obj: Member),
            value2: Payload.Case is TArgs value ? EqualityComparer<TArgs>.Default.GetHashCode(obj: value) : 0);
}

// A delegate-substitution restore scope: Dispose reinstates the prior delegate exactly once, so a
// stack of installs unwinds last-in-first-out.
public readonly record struct SeamRestore(Action Restore) : IDisposable {
    public void Dispose() => Restore();
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
// SeamProbe is the recording substitution host: an Atom call log threads every invocation, and
// Install hands back a LIFO restore so nested seam swaps unwind in reverse bind order.
public sealed class SeamProbe<TArgs> {
    private readonly Atom<Seq<SeamCall<TArgs>>> calls = Atom(Seq<SeamCall<TArgs>>());

    public Seq<SeamCall<TArgs>> Calls => calls.Value;

    public Seq<TArgs> Payloads => calls.Value.Bind(static call => call.Payload.ToSeq());

    // Install binds `member` to the shape at the production resolution site through `bind`,
    // recording each call before yielding the case payload; the returned scope restores the prior
    // delegate, so a stack of installs unwinds last-in-first-out on disposal. FanOut walks its
    // values per call through an install-scoped cursor; exhaustion fails loudly, never recycles.
    public SeamRestore Install<TResult>(string member, Shape<TResult> shape, Func<Func<TArgs, TResult>, Action> bind) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: member);
        ArgumentNullException.ThrowIfNull(argument: shape);
        ArgumentNullException.ThrowIfNull(argument: bind);
        int[] cursor = [0];
        TResult Record(string label, Option<TArgs> payload, TResult value) {
            _ = calls.Swap(log => log.Add(new SeamCall<TArgs>(Member: label, Payload: payload)));
            return value;
        }
        TResult Substitute(TArgs args) => shape.Switch(
            state: (args, member, cursor, Record: (Func<string, Option<TArgs>, TResult, TResult>)Record),
            canned: static (st, s) => st.Record(st.member, Some(value: st.args), s.Value),
            fanOut: static (st, s) => {
                int index = Interlocked.Increment(location: ref st.cursor[0]) - 1;
                return index < s.Values.Count
                    ? st.Record(st.member, Some(value: st.args), s.Values[index])
                    : throw new XunitException($"FanOut seam '{st.member}' exhausted after {s.Values.Count} value(s)");
            },
            factory: static (st, s) => st.Record(s.InnerLabel, Option<TArgs>.None, s.Value));
        return new SeamRestore(Restore: bind(Substitute));
    }
}

// --- [FIXTURE_WRITERS]
// VariantWriter is the table-driven payload-variant writer: a name row and a payload row per
// variant, raw bytes written verbatim and objects encoded once, with absent variants never emitted.
// WriteAll refuses payload/absence rows outside the name table — a row that can never emit is a
// table defect, never silent dead data.
public sealed record VariantWriter<TVariant>(
    DirectoryInfo Directory,
    FrozenDictionary<TVariant, string> Names,
    FrozenDictionary<TVariant, VariantPayload> Payloads,
    JsonTypeInfo Encode,
    FrozenSet<TVariant> Absent)
    where TVariant : notnull {

    public FileInfo Path(TVariant variant) {
        ArgumentNullException.ThrowIfNull(argument: variant);
        string target = System.IO.Path.Combine(path1: Directory.FullName, path2: Names[variant]);
        return (Absent.Contains(variant), Payloads.TryGetValue(variant, out VariantPayload? payload)) switch {
            (true, _) => new FileInfo(target),
            (_, true) => Emit(target: target, raw: payload!.Switch(
                state: Encode,
                raw: static (_, r) => r.Bytes,
                encoded: static (encode, encoded) => JsonSerializer.SerializeToUtf8Bytes(value: encoded.Value, jsonTypeInfo: encode))),
            _ => throw new XunitException($"VariantWriter has no payload for variant '{variant}'"),
        };
    }

    public FrozenDictionary<TVariant, FileInfo> WriteAll() {
        string[] orphans = [.. Payloads.Keys.Concat(Absent)
            .Where(variant => !Names.ContainsKey(variant))
            .Select(selector: static variant => $"{variant}")
            .Order(comparer: StringComparer.Ordinal)];
        return orphans.Length == 0
            ? Names.Keys.ToFrozenDictionary(static variant => variant, Path)
            : throw new XunitException($"VariantWriter rows outside the name table never emit: {string.Join(separator: ", ", value: orphans)}");
    }

    private static FileInfo Emit(string target, ReadOnlyMemory<byte> raw) {
        FileInfo file = new(target);
        file.Directory?.Create();
        using FileStream stream = File.Create(path: target);
        stream.Write(buffer: raw.Span);
        return file;
    }
}

// TmpRoot is the isolated tmp tree plus its injected settings projection: write materializes one
// root-contained relative path, optionally applying a Unix mode, and settings derives once at
// build. A rooted or upward-traversing relative would break isolation silently; the guard is
// separator-anchored so a sibling directory sharing the root's name prefix never slips through.
public sealed record TmpRoot<TSettings>(DirectoryInfo Root, TSettings Settings) {
    public FileInfo Write(string relative, string text = "", Option<UnixFileMode> mode = default) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: relative);
        string anchor = $"{Root.FullName.TrimEnd(Path.DirectorySeparatorChar)}{Path.DirectorySeparatorChar}";
        string target = Path.GetFullPath(path: Path.Combine(path1: Root.FullName, path2: relative));
        _ = target.StartsWith(value: anchor, comparisonType: StringComparison.Ordinal)
            ? target : throw new ArgumentOutOfRangeException(paramName: nameof(relative), actualValue: relative, message: "relative escapes the tmp root");
        FileInfo file = new(target);
        file.Directory?.Create();
        File.WriteAllText(path: file.FullName, contents: text);
        if (!OperatingSystem.IsWindows() && mode.Case is UnixFileMode unix) {
            File.SetUnixFileMode(path: file.FullName, mode: unix);
        }
        return file;
    }
}

// TmpRoot.Of is the settings-deriving admission factory, kept off the generic owner so settings
// derivation infers from the root without spelling the settings type twice.
public static class TmpRoot {
    public static TmpRoot<TSettings> Of<TSettings>(DirectoryInfo root, Func<DirectoryInfo, TSettings> makeSettings) {
        ArgumentNullException.ThrowIfNull(argument: root);
        ArgumentNullException.ThrowIfNull(argument: makeSettings);
        return new TmpRoot<TSettings>(Root: root, Settings: makeSettings(root));
    }
}

// --- [DECODE_ORACLES]
// NdjsonOracle gates the line count before decoding through the contract's JsonTypeInfo<T>:
// shape is asserted before content. One reads the first row; All decodes every gated row into an
// array — assertion material stays reflection-free where LanguageExt trait equality would fault.
// Gate and decode share ONE segmentation walk, so the two derivations can never disagree: a
// doubled trailing newline stays a counted empty segment whose decode fails loudly, never a
// silently dropped row. ExpectLines 0 with All asserts an empty stream.
public sealed record NdjsonOracle<T>(JsonTypeInfo<T> Decoder, int ExpectLines = 1) {
    public T One(ReadOnlySpan<byte> raw) {
        GateLines(raw: raw);
        ReadOnlySpan<byte> rest = raw;
        return Decode(line: NextLine(rest: ref rest));
    }

    public T One(string raw) {
        ArgumentNullException.ThrowIfNull(argument: raw);
        return One(System.Text.Encoding.UTF8.GetBytes(raw));
    }

    public T[] All(ReadOnlySpan<byte> raw) {
        GateLines(raw: raw);
        List<T> rows = new(capacity: ExpectLines);
        ReadOnlySpan<byte> rest = raw;
        while (!rest.IsEmpty) {
            rows.Add(item: Decode(line: NextLine(rest: ref rest)));
        }
        return [.. rows];
    }

    public T[] All(string raw) {
        ArgumentNullException.ThrowIfNull(argument: raw);
        return All(System.Text.Encoding.UTF8.GetBytes(raw));
    }

    private void GateLines(ReadOnlySpan<byte> raw) {
        int lines = 0;
        for (ReadOnlySpan<byte> rest = raw; !rest.IsEmpty; lines++) {
            _ = NextLine(rest: ref rest);
        }
        _ = lines == ExpectLines
            ? lines : throw new XunitException(string.Create(provider: CultureInfo.InvariantCulture, $"expected exactly {ExpectLines} NDJSON line(s), got {lines}"));
    }

    private T Decode(ReadOnlySpan<byte> line) =>
        JsonSerializer.Deserialize(utf8Json: line, jsonTypeInfo: Decoder) ?? throw new XunitException("NDJSON row decoded to null");

    // One segmentation step: the bytes before the next '\n' with one trailing '\r' trimmed; a
    // single final newline terminates the stream instead of opening an empty segment.
    private static ReadOnlySpan<byte> NextLine(ref ReadOnlySpan<byte> rest) {
        int newline = rest.IndexOf((byte)'\n');
        ReadOnlySpan<byte> line = newline < 0 ? rest : rest[..newline];
        rest = newline < 0 ? [] : rest[(newline + 1)..];
        return line.Length > 0 && line[^1] == (byte)'\r' ? line[..^1] : line;
    }
}

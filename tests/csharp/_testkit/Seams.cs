using System.Collections.Frozen;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Xunit.Sdk;

namespace Rasm.TestKit;

// --- [TYPES] --------------------------------------------------------------------------------
// One call-shape family: the case is the substitution behavior, the generated Switch is the sole
// dispatch, and the canned value rides each case rather than a parallel mode flag.
[Union]
public abstract partial record Shape<TValue> {
    private Shape() { }
    public sealed record Sync(TValue Value) : Shape<TValue>;
    public sealed record Async(TValue Value) : Shape<TValue>;
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

    // Install binds `member` to the canned shape at the production resolution site through `bind`,
    // recording each call before yielding the case payload; the returned scope restores the prior
    // delegate, so a stack of installs unwinds last-in-first-out on disposal.
    public SeamRestore Install<TResult>(string member, Shape<TResult> shape, Func<Func<TArgs, TResult>, Action> bind) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: member);
        ArgumentNullException.ThrowIfNull(argument: shape);
        ArgumentNullException.ThrowIfNull(argument: bind);
        TResult Record(string label, Option<TArgs> payload, TResult value) {
            _ = calls.Swap(log => log.Add(new SeamCall<TArgs>(Member: label, Payload: payload)));
            return value;
        }
        TResult Canned(TArgs args) => shape.Switch(
            state: (args, member, Record: (Func<string, Option<TArgs>, TResult, TResult>)Record),
            sync: static (st, s) => st.Record(st.member, Some(value: st.args), s.Value),
            async: static (st, s) => st.Record(st.member, Some(value: st.args), s.Value),
            fanOut: static (st, s) => s.Values is [var head, ..] ? st.Record(st.member, Some(value: st.args), head) : throw new XunitException($"FanOut seam '{st.member}' has no values"),
            factory: static (st, s) => st.Record(s.InnerLabel, Option<TArgs>.None, s.Value));
        return new SeamRestore(Restore: bind(Canned));
    }
}

// --- [FIXTURE_WRITERS]
// VariantWriter is the table-driven payload-variant writer: a name row and a payload row per
// variant, raw bytes written verbatim and objects encoded once, with absent variants never emitted.
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

    public FrozenDictionary<TVariant, FileInfo> WriteAll() =>
        Names.Keys.ToFrozenDictionary(static variant => variant, Path);

    private static FileInfo Emit(string target, ReadOnlyMemory<byte> raw) {
        FileInfo file = new(target);
        file.Directory?.Create();
        using FileStream stream = File.Create(path: target);
        stream.Write(buffer: raw.Span);
        return file;
    }
}

// TmpRoot is the isolated tmp tree plus its injected settings projection: write materializes one
// relative path under the root, optionally applying a Unix mode, and settings derives once at build.
public sealed record TmpRoot<TSettings>(DirectoryInfo Root, TSettings Settings) {
    public FileInfo Write(string relative, string text = "", Option<UnixFileMode> mode = default) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: relative);
        FileInfo file = new(Path.Combine(path1: Root.FullName, path2: relative));
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
// NdjsonOracle gates the line count before decoding the first row through the contract's
// JsonTypeInfo<T>: shape is asserted before content.
public sealed record NdjsonOracle<T>(JsonTypeInfo<T> Decoder, int ExpectLines = 1) {
    public T One(ReadOnlySpan<byte> raw) {
        int lines = CountLines(raw);
        return lines == ExpectLines
            ? JsonSerializer.Deserialize(utf8Json: FirstLine(raw), jsonTypeInfo: Decoder) ?? throw new XunitException("NDJSON row decoded to null")
            : throw new XunitException(string.Create(provider: CultureInfo.InvariantCulture, $"expected exactly {ExpectLines} NDJSON line(s), got {lines}"));
    }

    public T One(string raw) {
        ArgumentNullException.ThrowIfNull(argument: raw);
        return One(System.Text.Encoding.UTF8.GetBytes(raw));
    }

    private static int CountLines(ReadOnlySpan<byte> raw) {
        ReadOnlySpan<byte> trimmed = raw.TrimEnd((byte)'\n').TrimEnd((byte)'\r');
        return trimmed.IsEmpty ? 0 : trimmed.Count((byte)'\n') + 1;
    }

    private static ReadOnlySpan<byte> FirstLine(ReadOnlySpan<byte> raw) {
        int newline = raw.IndexOf((byte)'\n');
        ReadOnlySpan<byte> line = newline < 0 ? raw : raw[..newline];
        return line.TrimEnd((byte)'\r');
    }
}

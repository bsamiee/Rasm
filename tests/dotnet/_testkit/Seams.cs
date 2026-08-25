using System.Collections.Frozen;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Time.Testing;
using Xunit.Sdk;

namespace Rasm.TestKit;

// --- [TYPES] ---------------------------------------------------------------------------
[Union]
public abstract partial record Shape<TValue> {
    private Shape() { }
    public sealed record Canned(TValue Value) : Shape<TValue>;
    public sealed record FanOut(Seq<TValue> Values) : Shape<TValue>;
    public sealed record Factory(TValue Value, string InnerLabel = "<factory>.run") : Shape<TValue>;
}

[Union]
public abstract partial record VariantPayload {
    private VariantPayload() { }
    public sealed record Raw(ReadOnlyMemory<byte> Bytes) : VariantPayload;
    public sealed record Encoded(object Value) : VariantPayload;
}

// --- [MODELS] --------------------------------------------------------------------------
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

public readonly record struct SeamRestore(Action Restore) : IDisposable {
    public void Dispose() => Restore();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class SeamProbe<TArgs> {
    private readonly Atom<Seq<SeamCall<TArgs>>> calls = Atom(Seq<SeamCall<TArgs>>());

    public Seq<SeamCall<TArgs>> Calls => calls.Value;

    public Seq<TArgs> Payloads => calls.Value.Bind(static call => call.Payload.ToSeq());

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

public static class TmpRoot {
    public static TmpRoot<TSettings> Of<TSettings>(DirectoryInfo root, Func<DirectoryInfo, TSettings> makeSettings) {
        ArgumentNullException.ThrowIfNull(argument: root);
        ArgumentNullException.ThrowIfNull(argument: makeSettings);
        return new TmpRoot<TSettings>(Root: root, Settings: makeSettings(root));
    }
}

// --- [DECODE_ORACLES]
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

    private static ReadOnlySpan<byte> NextLine(ref ReadOnlySpan<byte> rest) {
        int newline = rest.IndexOf((byte)'\n');
        ReadOnlySpan<byte> line = newline < 0 ? rest : rest[..newline];
        rest = newline < 0 ? [] : rest[(newline + 1)..];
        return line.Length > 0 && line[^1] == (byte)'\r' ? line[..^1] : line;
    }
}

// --- [CLOCK]
public sealed record ClockMark(string Label, TimeSpan Elapsed);

public sealed class Timeline(DateTimeOffset? start = null) {
    private readonly Atom<Seq<ClockMark>> marks = Atom(Seq<ClockMark>());

    public FakeTimeProvider Clock { get; } = start is DateTimeOffset instant ? new FakeTimeProvider(startDateTime: instant) : new FakeTimeProvider();

    public Seq<ClockMark> Marks => marks.Value;

    public Seq<ClockMark> Advance(TimeSpan delta) {
        int before = marks.Value.Count;
        Clock.Advance(delta: delta);
        return marks.Value.Skip(amount: before);
    }

    public ITimer Probe(string label, TimeSpan due, TimeSpan? period = null) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: label);
        TimeSpan origin = Clock.GetUtcNow() - Clock.Start;
        TimeSpan beat = period ?? TimeSpan.Zero;
        int[] fired = [0];
        return Clock.CreateTimer(
            callback: _ => {
                int ordinal = Interlocked.Increment(location: ref fired[0]) - 1;
                _ = marks.Swap(log => log.Add(new ClockMark(Label: label, Elapsed: origin + due + (beat * ordinal))));
            },
            state: null,
            dueTime: due,
            period: period ?? Timeout.InfiniteTimeSpan);
    }
}

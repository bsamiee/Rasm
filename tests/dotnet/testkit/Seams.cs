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

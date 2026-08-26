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
public readonly record struct ProbeCall<TArgs>(string Member, Option<TArgs> Payload) {
    public bool Equals(ProbeCall<TArgs> other) =>
        string.Equals(Member, other.Member, StringComparison.Ordinal)
        && (Payload.Case, other.Payload.Case) switch {
            (null, null) => true,
            (TArgs left, TArgs right) => EqualityComparer<TArgs>.Default.Equals(left, right),
            _ => false,
        };

    public override int GetHashCode() =>
        HashCode.Combine(StringComparer.Ordinal.GetHashCode(Member), Payload.Case is TArgs value ? EqualityComparer<TArgs>.Default.GetHashCode(value) : 0);
}

public readonly record struct ProbeRestore(Action Restore) : IDisposable {
    public void Dispose() => Restore();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class CallProbe<TArgs> {
    private readonly Atom<Seq<ProbeCall<TArgs>>> calls = Atom(Seq<ProbeCall<TArgs>>());

    public Seq<ProbeCall<TArgs>> Calls => calls.Value;

    public Seq<TArgs> Payloads => calls.Value.Bind(static call => call.Payload.ToSeq());

    public ProbeRestore Install<TResult>(string member, Shape<TResult> shape, Func<Func<TArgs, TResult>, Action> bind) {
        ArgumentException.ThrowIfNullOrWhiteSpace(member);
        ArgumentNullException.ThrowIfNull(shape);
        ArgumentNullException.ThrowIfNull(bind);
        int[] cursor = [0];
        TResult Record(string label, Option<TArgs> payload, TResult value) {
            _ = calls.Swap(log => log.Add(new ProbeCall<TArgs>(label, payload)));
            return value;
        }
        TResult Substitute(TArgs args) => shape.Switch(
            state: (args, member, cursor, Record: (Func<string, Option<TArgs>, TResult, TResult>)Record),
            canned: static (st, s) => st.Record(st.member, st.args, s.Value),
            fanOut: static (st, s) => {
                int index = Interlocked.Increment(ref st.cursor[0]) - 1;
                return index < s.Values.Count
                    ? st.Record(st.member, st.args, s.Values[index])
                    : throw new XunitException($"FanOut double '{st.member}' exhausted after {s.Values.Count} value(s)");
            },
            factory: static (st, s) => st.Record(s.InnerLabel, Option<TArgs>.None, s.Value));
        return new ProbeRestore(bind(Substitute));
    }
}

// --- [CLOCK]
public sealed class Timeline(DateTimeOffset? start = null) {
    public sealed record ClockMark(string Label, TimeSpan Elapsed);

    private readonly Atom<Seq<ClockMark>> marks = Atom(Seq<ClockMark>());

    public FakeTimeProvider Clock { get; } = start is DateTimeOffset instant ? new FakeTimeProvider(instant) : new FakeTimeProvider();

    public Seq<ClockMark> Marks => marks.Value;

    public Seq<ClockMark> Advance(TimeSpan delta) {
        int before = marks.Value.Count;
        Clock.Advance(delta);
        return marks.Value.Skip(before);
    }

    public ITimer Probe(string label, TimeSpan due, TimeSpan? period = null) {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        TimeSpan origin = Clock.GetUtcNow() - Clock.Start;
        TimeSpan beat = period ?? TimeSpan.Zero;
        int[] fired = [0];
        return Clock.CreateTimer(
            _ => {
                int ordinal = Interlocked.Increment(ref fired[0]) - 1;
                _ = marks.Swap(log => log.Add(new ClockMark(label, origin + due + (beat * ordinal))));
            },
            state: null,
            due,
            period ?? Timeout.InfiniteTimeSpan);
    }
}

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Time.Testing;
using Xunit.Sdk;

namespace Rasm.TestSupport;

// --- [TYPES] ---------------------------------------------------------------------------
[Union]
public abstract partial record StubBehavior<TValue> {
    private StubBehavior() { }
    public sealed record Constant(TValue Value) : StubBehavior<TValue>;
    public sealed record Sequence(Seq<TValue> Values) : StubBehavior<TValue>;
    public sealed record Factory(TValue Value, string InnerLabel = "<factory>.run") : StubBehavior<TValue>;
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

public readonly record struct RestoreHandle(Action Restore) : IDisposable {
    public void Dispose() => Restore();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class CallProbe<TArgs> {
    private readonly Atom<Seq<ProbeCall<TArgs>>> calls = Atom(Seq<ProbeCall<TArgs>>());

    public Seq<ProbeCall<TArgs>> Calls => calls.Value;

    public Seq<TArgs> Payloads => calls.Value.Bind(static call => call.Payload.ToSeq());

    public RestoreHandle Attach<TResult>(string member, StubBehavior<TResult> behavior, Func<Func<TArgs, TResult>, Action> bind) {
        ArgumentException.ThrowIfNullOrWhiteSpace(member);
        ArgumentNullException.ThrowIfNull(behavior);
        ArgumentNullException.ThrowIfNull(bind);
        StrongBox<int> cursor = new(0);
        TResult Record(string label, Option<TArgs> payload, TResult value) {
            _ = calls.Swap(log => log.Add(new ProbeCall<TArgs>(label, payload)));
            return value;
        }
        TResult Substitute(TArgs args) => behavior.Switch(
            state: (args, member, cursor, Record: (Func<string, Option<TArgs>, TResult, TResult>)Record),
            constant: static (st, value) => st.Record(st.member, st.args, value.Value),
            sequence: static (st, values) => {
                int index = Interlocked.Increment(ref st.cursor.Value) - 1;
                return index < values.Values.Count
                    ? st.Record(st.member, st.args, values.Values[index])
                    : throw new XunitException($"Sequence stub '{st.member}' exhausted after {values.Values.Count} value(s)");
            },
            factory: static (st, factory) => st.Record(factory.InnerLabel, Option<TArgs>.None, factory.Value));
        return new RestoreHandle(bind(Substitute));
    }
}

// --- [CLOCK]
public sealed class Timeline(DateTimeOffset? start = null) {
    public sealed record TimerEvent(string Label, TimeSpan Elapsed);

    private readonly Atom<Seq<TimerEvent>> events = Atom(Seq<TimerEvent>());

    public FakeTimeProvider Clock { get; } = start is DateTimeOffset instant ? new FakeTimeProvider(instant) : new FakeTimeProvider();

    public Seq<TimerEvent> Events => events.Value;

    public Seq<TimerEvent> Advance(TimeSpan delta) {
        int before = events.Value.Count;
        Clock.Advance(delta);
        return events.Value.Skip(before);
    }

    public ITimer CreateTimer(string label, TimeSpan due, TimeSpan? period = null) {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        TimeSpan origin = Clock.GetUtcNow() - Clock.Start;
        TimeSpan interval = period ?? TimeSpan.Zero;
        StrongBox<int> invocationCount = new(0);
        return Clock.CreateTimer(
            _ => {
                int ordinal = Interlocked.Increment(ref invocationCount.Value) - 1;
                _ = events.Swap(log => log.Add(new TimerEvent(label, origin + due + (interval * ordinal))));
            },
            state: null,
            due,
            period ?? Timeout.InfiniteTimeSpan);
    }
}

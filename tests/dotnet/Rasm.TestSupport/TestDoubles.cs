using Microsoft.Extensions.Time.Testing;
using Xunit.Sdk;

namespace Rasm.TestSupport;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record StubBehavior<TValue> {
    private StubBehavior() { }
    public sealed record Constant(TValue Value) : StubBehavior<TValue>;
    public sealed record Sequence(Seq<TValue> Values) : StubBehavior<TValue>;
}

public readonly record struct SpyCall<TArgs>(string Member, TArgs Arguments);

public readonly record struct RestoreHandle(Action Restore) : IDisposable {
    public void Dispose() => Restore();
}

public sealed record TimerEvent(string Label, TimeSpan Due, TimeSpan Observed);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class CallSpy<TArgs> {
    private readonly Atom<Seq<SpyCall<TArgs>>> calls = Atom(Seq<SpyCall<TArgs>>());

    public Seq<SpyCall<TArgs>> Calls => calls.Value;

    public Seq<TArgs> Arguments => calls.Value.Map(static call => call.Arguments);

    public Func<TArgs, TResult> Stub<TResult>(string member, StubBehavior<TResult> behavior) {
        ArgumentException.ThrowIfNullOrWhiteSpace(member);
        Atom<int> cursor = Atom(0);
        return args => {
            _ = calls.Swap(log => log.Add(new SpyCall<TArgs>(member, args)));
            return behavior.Switch(
                state: (member, cursor),
                constant: static (_, constant) => constant.Value,
                sequence: static (st, sequence) => {
                    int index = st.cursor.Swap(static position => position + 1) - 1;
                    return index < sequence.Values.Count
                        ? sequence.Values[index]
                        : throw new XunitException($"sequence stub '{st.member}' exhausted after {sequence.Values.Count} values");
                });
        };
    }

    public RestoreHandle Attach<TResult>(string member, StubBehavior<TResult> behavior, Func<Func<TArgs, TResult>, Action> bind) =>
        new(bind(Stub(member, behavior)));
}

public sealed class Timeline(DateTimeOffset? start = null) {
    private readonly Atom<Seq<TimerEvent>> events = Atom(Seq<TimerEvent>());

    public FakeTimeProvider Clock { get; } = start is DateTimeOffset instant ? new FakeTimeProvider(instant) : new FakeTimeProvider();

    public Seq<TimerEvent> Events => events.Value;

    public Seq<TimerEvent> Advance(TimeSpan delta) {
        int before = events.Value.Count;
        Clock.Advance(delta);
        return events.Value.Skip(before);
    }

    public ITimer CreateTimer(string label, TimeSpan due, Option<TimeSpan> period = default) {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        TimeSpan origin = Clock.GetUtcNow() - Clock.Start;
        TimeSpan interval = period.IfNone(Timeout.InfiniteTimeSpan);
        Atom<int> firings = Atom(0);
        return Clock.CreateTimer(
            _ => {
                int ordinal = firings.Swap(static count => count + 1) - 1;
                _ = events.Swap(log => log.Add(new TimerEvent(label, origin + due + (interval * ordinal), Clock.GetUtcNow() - Clock.Start)));
            },
            state: null,
            due,
            interval);
    }
}

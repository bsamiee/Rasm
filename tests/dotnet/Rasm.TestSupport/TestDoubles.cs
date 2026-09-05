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
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct SpyCall<TArgs>(string Member, TArgs Arguments);

public readonly record struct RestoreHandle(Action Restore) : IDisposable {
    public void Dispose() => Restore();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class CallSpy<TArgs> {
    private readonly Atom<Seq<SpyCall<TArgs>>> calls = Atom(Seq<SpyCall<TArgs>>());

    public Seq<SpyCall<TArgs>> Calls => calls.Value;

    public Seq<TArgs> Arguments => calls.Value.Map(static call => call.Arguments);

    // The returned function records every call under the member name and answers from the behavior, a sequence that runs out fails the test
    public Func<TArgs, TResult> Stub<TResult>(string member, StubBehavior<TResult> behavior) {
        ArgumentException.ThrowIfNullOrWhiteSpace(member);
        ArgumentNullException.ThrowIfNull(behavior);
        StrongBox<int> cursor = new(0);
        return args => {
            _ = calls.Swap(log => log.Add(new SpyCall<TArgs>(member, args)));
            return behavior.Switch(
                state: (member, cursor),
                constant: static (_, constant) => constant.Value,
                sequence: static (st, sequence) => {
                    int index = Interlocked.Increment(ref st.cursor.Value) - 1;
                    return index < sequence.Values.Count
                        ? sequence.Values[index]
                        : throw new XunitException($"sequence stub '{st.member}' exhausted after {sequence.Values.Count} values");
                });
        };
    }

    // Installs the stub into a mutable hook through bind, and the action bind returns restores the hook when the handle disposes
    public RestoreHandle Attach<TResult>(string member, StubBehavior<TResult> behavior, Func<Func<TArgs, TResult>, Action> bind) {
        ArgumentNullException.ThrowIfNull(bind);
        return new RestoreHandle(bind(Stub(member, behavior)));
    }
}

// --- [CLOCK] ---------------------------------------------------------------------------
public sealed class Timeline(DateTimeOffset? start = null) {
    public sealed record TimerEvent(string Label, TimeSpan Due, TimeSpan Observed);

    private readonly Atom<Seq<TimerEvent>> events = Atom(Seq<TimerEvent>());

    public FakeTimeProvider Clock { get; } = start is DateTimeOffset instant ? new FakeTimeProvider(instant) : new FakeTimeProvider();

    public Seq<TimerEvent> Events => events.Value;

    public Seq<TimerEvent> Advance(TimeSpan delta) {
        int before = events.Value.Count;
        Clock.Advance(delta);
        return events.Value.Skip(before);
    }

    // Due is the schedule time of the firing, and Observed is the clock reading in the callback, which FakeTimeProvider moves to the end of Advance before it fires
    public ITimer CreateTimer(string label, TimeSpan due, TimeSpan? period = null) {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        TimeSpan origin = Clock.GetUtcNow() - Clock.Start;
        TimeSpan interval = period ?? TimeSpan.Zero;
        StrongBox<int> firings = new(0);
        return Clock.CreateTimer(
            _ => {
                int ordinal = Interlocked.Increment(ref firings.Value) - 1;
                _ = events.Swap(log => log.Add(new TimerEvent(label, origin + due + (interval * ordinal), Clock.GetUtcNow() - Clock.Start)));
            },
            state: null,
            due,
            period ?? Timeout.InfiniteTimeSpan);
    }
}

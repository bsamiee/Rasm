using System.Collections.Concurrent;
using Microsoft.Extensions.Time.Testing;

namespace Rasm.TestSupport;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record TimerEvent(string Label, TimeSpan Observed);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Timeline(Option<DateTimeOffset> start = default) {
    private readonly ConcurrentQueue<TimerEvent> events = new();

    public FakeTimeProvider Clock { get; } = start.Match(static instant => new FakeTimeProvider(instant), static () => new FakeTimeProvider());

    public Seq<TimerEvent> Events => toSeq(events.ToArray());

    public Seq<TimerEvent> Advance(TimeSpan delta) {
        int before = events.Count;
        Clock.Advance(delta);
        return toSeq(events.ToArray().Skip(before));
    }

    public ITimer CreateTimer(string label, TimeSpan due, Option<TimeSpan> period = default) =>
        Clock.CreateTimer(_ => events.Enqueue(new TimerEvent(label, Clock.GetUtcNow() - Clock.Start)), state: null, due, period.IfNone(Timeout.InfiniteTimeSpan));
}

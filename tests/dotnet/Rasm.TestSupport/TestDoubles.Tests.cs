using Xunit.Sdk;

namespace Rasm.TestSupport;

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class CallSpyTests {
    [Fact]
    public void ConstantStubRecordsEveryCallInOrder() {
        CallSpy<int> spy = new();
        Func<int, string> lookup = spy.Stub("Lookup", new StubBehavior<string>.Constant("answer"));
        Assert.Equal("answer", lookup(1));
        Assert.Equal("answer", lookup(2));
        Assert.Equal(Seq(1, 2), spy.Arguments);
        Assert.All(spy.Calls, static call => Assert.Equal("Lookup", call.Member));
    }

    [Fact]
    public void SequenceStubAnswersInOrderThenFailsTheTest() {
        CallSpy<Unit> spy = new();
        Func<Unit, int> next = spy.Stub("Next", new StubBehavior<int>.Sequence(Seq(1, 2)));
        Assert.Equal(1, next(unit));
        Assert.Equal(2, next(unit));
        _ = Assert.Throws<XunitException>(() => next(unit));
    }

    [Fact]
    public void AttachRestoresTheHookWhenTheHandleDisposes() {
        CallSpy<int> spy = new();
        Func<int, int> hook = static x => x;
        using (spy.Attach("Hook", new StubBehavior<int>.Constant(7), stub => { Func<int, int> previous = hook; hook = stub; return () => hook = previous; }))
            Assert.Equal(7, hook(1));
        Assert.Equal(1, hook(1));
        Assert.Equal(Seq(1), spy.Arguments);
    }
}

public sealed class TimelineTests {
    [Fact]
    public void PeriodicTimerLogsEachFiringWithItsScheduleAndTheClockReading() {
        Timeline timeline = new();
        using ITimer timer = timeline.CreateTimer("tick", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        Seq<Timeline.TimerEvent> first = timeline.Advance(TimeSpan.FromSeconds(3.5));
        Assert.Equal([TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3)], first.Map(static e => e.Due));
        Assert.All(first, static e => Assert.Equal(TimeSpan.FromSeconds(3.5), e.Observed));
        Assert.Empty(timeline.Advance(TimeSpan.FromSeconds(0.4)));
        Assert.Equal([TimeSpan.FromSeconds(4)], timeline.Advance(TimeSpan.FromSeconds(0.2)).Map(static e => e.Due));
        Assert.All(timeline.Events, static e => Assert.Equal("tick", e.Label));
    }
}

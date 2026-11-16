using Rasm.Imaging.Pixels;
using Rasm.Rhino;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Objects;
using Rhino.Render;

namespace Darkroom.Queue;

// --- [MODELS] --------------------------------------------------------------------------

internal sealed record FrameTiming(UInt128 Entry, SequenceNumber Frame, long Started, long Began, long Saved, long Written, long PeakWorkingSet) {
    public Duration Time(TimeProvider clock) => clock.GetElapsedTime(Started, Written).ToDuration();
}

[SmartEnum<string>]
internal sealed partial class FrameStage {
    public static readonly FrameStage Setup = new("setup", static (timing, clock) => clock.GetElapsedTime(timing.Started, timing.Began).ToDuration());
    public static readonly FrameStage Render = new("render", static (timing, clock) => clock.GetElapsedTime(timing.Began, timing.Saved).ToDuration());
    public static readonly FrameStage Save = new("save", static (timing, clock) => clock.GetElapsedTime(timing.Saved, timing.Written).ToDuration());

    [UseDelegateFromConstructor]
    public partial Duration Of(FrameTiming timing, TimeProvider clock);
}

internal sealed record QueueEstimate(long Completed, long Total, Option<(Duration Remaining, Instant Finish)> Prediction) {

    // --- [ETA]

    public static QueueEstimate Running(RenderBatch batch, QueueProgress record, Instant now) => Of(batch, record, now, Mean);

    public static QueueEstimate Sampled(RenderBatch batch, QueueProgress record, Instant now) => Of(batch, record, now, static times => times.At(1) | times.Head);

    public string Description(DateTimeZone zone) =>
        Prediction.Map(prediction => LocalText.Of("{0:-H:mm:ss} left, finishing {1:g}, {2} of {3} frames rendered", prediction.Remaining, prediction.Finish.InZone(zone).LocalDateTime, Completed, Total).Local)
            .IfNone(() => LocalText.Of("{0} of {1} frames rendered", Completed, Total).Local);

    private static QueueEstimate Of(RenderBatch batch, QueueProgress record, Instant now, Func<Seq<Duration>, Option<Duration>> rate) {
        Seq<(Seq<Duration> Times, long Remaining)> rows = batch.Entries.Map(entry => record.Entries.Find(entry.Key).IfNone(EntryProgress.Empty) switch {
            var held => (held.Times, Remaining: held.Last.Match(Some: done => entry.Frames.Numbers.Count - 1L - (int)done, None: () => entry.Frames.Numbers.Count)),
        }).Strict();
        Option<Duration> mean = Mean(rows.Bind(static held => held.Times));
        return new(record.Completed, batch.Frames, rows.Filter(static row => row.Remaining > 0)
            .Traverse(row => (rate(row.Times) | mean).Map(each => each * row.Remaining)).As()
            .Map(static parts => parts.Fold(Duration.Zero, static (sum, part) => sum + part))
            .Map(remaining => (remaining, now + remaining)));
    }

    private static Option<Duration> Mean(Seq<Duration> times) =>
        times.IsEmpty ? None : Some(times.Fold(Duration.Zero, static (sum, time) => sum + time) / times.Count);

    // --- [MEMORY]

    public static ulong Memory(DocumentStatistics statistics, PixelExtent rendered, Seq<RenderWindow.StandardChannels> components) =>
        statistics.MemoryEstimate + ((ulong)rendered.Width * (ulong)rendered.Height * sizeof(float) * (ulong)components.Count);
}

using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Objects;
using Rasm.Rhino.Render.Scenes;
using Rhino.Render;

namespace Rasm.Rhino.Render.Queue;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FrameMark(ContentKey Entry, SequenceNumber Frame, long Started, long Began, long Saved, long Written, long PeakWorkingSet) {
    public Duration Time(TimeProvider clock) => clock.GetElapsedTime(Started, Saved).ToDuration();
}

[SmartEnum]
public sealed partial class FrameStage {
    public static readonly FrameStage Setup = new(static (mark, clock) => clock.GetElapsedTime(mark.Started, mark.Began).ToDuration());
    public static readonly FrameStage Render = new(static (mark, clock) => clock.GetElapsedTime(mark.Began, mark.Saved).ToDuration());
    public static readonly FrameStage Save = new(static (mark, clock) => clock.GetElapsedTime(mark.Saved, mark.Written).ToDuration());

    [UseDelegateFromConstructor]
    public partial Duration Of(FrameMark mark, TimeProvider clock);
}

public sealed record QueueEstimate(int Landed, int Total, Option<Duration> Remaining, Option<Instant> Finish, Option<FrameMark> Last) {
    // --- [ETA]
    public static QueueEstimate Idle { get; } = new(0, 0, None, None, None);

    public static QueueEstimate Running(RenderBatch queue, QueueProgress record, Option<FrameMark> last, Instant now) =>
        Of(queue, record, last, now, Mean);

    public static QueueEstimate Sampled(RenderBatch queue, QueueProgress record, Instant now) =>
        Of(queue, record, None, now, static times => times.At(1) | times.Head);

    public string Description(DateTimeZone zone) =>
        (Remaining, Finish)
            .Apply((left, at) => RowText.Localize("{0:-H:mm:ss} left, finishing {1:g}, {2} of {3} frames rendered", arguments: [left, at.InZone(zone).LocalDateTime, Landed, Total]))
            .As()
            .IfNone(() => RowText.Localize("{0} of {1} frames rendered", arguments: [Landed, Total]));

    private static QueueEstimate Of(RenderBatch queue, QueueProgress record, Option<FrameMark> last, Instant now, Func<Seq<Duration>, Option<Duration>> rate) =>
        queue.Entries.Map(entry => record.Entries.Find(entry.Key).IfNone(EntryProgress.Empty) switch {
            var held => (held.Times, Remaining: held.Last.Match(Some: done => (int)entry.Frames.Last - (int)done, None: () => entry.Frames.Count)),
        }) switch {
            var rows => rows.Filter(static row => row.Remaining > 0)
                .Traverse(row => (rate(row.Times) | Mean(rows.Bind(static held => held.Times))).Map(each => each * row.Remaining))
                .As()
                .Map(static parts => parts.Fold(Duration.Zero, static (sum, part) => sum + part)) switch {
                    var remaining => new(record.Landed, queue.Frames, remaining, remaining.Map(left => now + left), last),
                },
        };

    private static Option<Duration> Mean(Seq<Duration> times) =>
        times.IsEmpty ? None : Some(times.Fold(Duration.Zero, static (sum, time) => sum + time) / times.Count);

    // --- [MEMORY]
    public static RenderWindow.StandardChannels Channels(RenderChannelsState requested) =>
        (requested.Mode == RenderChannels.Modes.Custom ? toSeq(requested.CustomList) : Seq<Guid>()).Fold(
            RenderWindow.StandardChannels.Red | RenderWindow.StandardChannels.Green | RenderWindow.StandardChannels.Blue | RenderWindow.StandardChannels.Alpha,
            static (held, id) => held | RenderWindow.StandardChannelForGuid(id));

    public static ulong Memory(DocumentStatistics statistics, PixelExtent rendered, RenderWindow.StandardChannels channels) =>
        statistics.MemoryEstimate + ((ulong)rendered.Width * (ulong)rendered.Height * sizeof(float)
            * (ulong)(System.Numerics.BitOperations.PopCount((uint)channels)
                + (3 * System.Numerics.BitOperations.PopCount((uint)(channels & (RenderWindow.StandardChannels.WireframePointsRGBA | RenderWindow.StandardChannels.WireframeIsocurvesRGBA
                    | RenderWindow.StandardChannels.WireframeCurvesRGBA | RenderWindow.StandardChannels.WireframeAnnotationsRGBA))))));
}

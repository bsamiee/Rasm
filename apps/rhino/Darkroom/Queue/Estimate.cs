using System.Numerics;
using Rasm.Imaging.Pixels;
using Rasm.RhinoCommon;
using Rasm.RhinoCommon.Document.Files;
using Rasm.RhinoCommon.Objects;
using Rasm.RhinoCommon.UI;
using Rasm.UI;
using Rasm.UI.Components;
using Rhino.Render;

namespace Darkroom.Queue;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FrameMark(ContentKey Entry, SequenceNumber Frame, long Started, long Began, long Saved, long Written, long PeakWorkingSet) {
    public Duration Time(TimeProvider clock) => clock.GetElapsedTime(Started, Written).ToDuration();
}

[SmartEnum<string>]
public sealed partial class FrameStage {
    public static readonly FrameStage Setup = new("setup", static (mark, clock) => clock.GetElapsedTime(mark.Started, mark.Began).ToDuration());
    public static readonly FrameStage Render = new("render", static (mark, clock) => clock.GetElapsedTime(mark.Began, mark.Saved).ToDuration());
    public static readonly FrameStage Save = new("save", static (mark, clock) => clock.GetElapsedTime(mark.Saved, mark.Written).ToDuration());

    [UseDelegateFromConstructor]
    public partial Duration Of(FrameMark mark, TimeProvider clock);
}

public sealed record QueueEstimate(long Landed, long Total) {
    public Option<Duration> Remaining { get; init; }
    public Option<Instant> Finish { get; init; }
    public Option<FrameMark> Last { get; init; }

    // --- [ETA]
    public static QueueEstimate Idle { get; } = new(0, 0);

    public Content<Unit> Work =>
        Total == 0 ? new Content<Unit>.Empty(nameof(Idle))
        : Landed < Total ? new Content<Unit>.Pending { Work = Progress.Validated(Landed, Total).ToOption() }
        : new Content<Unit>.Ready(unit);

    public static QueueEstimate Running(RenderBatch batch, QueueProgress record, Option<FrameMark> last, Instant now) => Of(batch, record, last, now, Mean);

    public static QueueEstimate Sampled(RenderBatch batch, QueueProgress record, Instant now) => Of(batch, record, None, now, static times => times.At(1) | times.Head);

    public string Description(DateTimeZone zone, IPlugInViews views) =>
        (Remaining, Finish)
            .Apply((left, at) => RowText.Localize("{0:-H:mm:ss} left, finishing {1:g}, {2} of {3} frames rendered", views, arguments: [left, at.InZone(zone).LocalDateTime, Landed, Total]).Local)
            .As()
            .IfNone(() => RowText.Localize("{0} of {1} frames rendered", views, arguments: [Landed, Total]).Local);

    private static QueueEstimate Of(RenderBatch batch, QueueProgress record, Option<FrameMark> last, Instant now, Func<Seq<Duration>, Option<Duration>> rate) =>
        batch.Entries.Map(entry => record.Entries.Find(entry.Key).IfNone(EntryProgress.Empty) switch {
            var held => (held.Times, Remaining: held.Last.Match(Some: done => entry.Frames.Numbers.Count - 1L - (int)done, None: () => entry.Frames.Numbers.Count)),
        }) switch {
            var rows => rows.Filter(static row => row.Remaining > 0)
                .Traverse(row => (rate(row.Times) | Mean(rows.Bind(static held => held.Times))).Map(each => each * row.Remaining))
                .As()
                .Map(static parts => parts.Fold(Duration.Zero, static (sum, part) => sum + part)) switch {
                    var remaining => new(record.Landed, batch.Frames) { Remaining = remaining, Finish = remaining.Map(left => now + left), Last = last },
                },
        };

    private static Option<Duration> Mean(Seq<Duration> times) =>
        times.IsEmpty ? None : Some(times.Fold(Duration.Zero, static (sum, time) => sum + time) / times.Count);

    // --- [MEMORY]
    public static RenderWindow.StandardChannels Channels(RenderChannels requested) =>
        (requested.Mode is RenderChannels.Modes.Custom ? toSeq(requested.CustomList) : Seq<Guid>()).Fold(
            RenderWindow.StandardChannels.Red | RenderWindow.StandardChannels.Green | RenderWindow.StandardChannels.Blue | RenderWindow.StandardChannels.Alpha,
            static (held, id) => held | RenderWindow.StandardChannelForGuid(id));

    public static ulong Memory(DocumentStatistics statistics, PixelExtent rendered, RenderWindow.StandardChannels channels) =>
        statistics.MemoryEstimate + ((ulong)rendered.Width * (ulong)rendered.Height * sizeof(float)
            * (ulong)(BitOperations.PopCount((uint)channels)
                + (3 * BitOperations.PopCount((uint)(channels & (RenderWindow.StandardChannels.WireframePointsRGBA | RenderWindow.StandardChannels.WireframeIsocurvesRGBA
                    | RenderWindow.StandardChannels.WireframeCurvesRGBA | RenderWindow.StandardChannels.WireframeAnnotationsRGBA))))));
}

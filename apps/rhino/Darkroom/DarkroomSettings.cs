using Rasm.Rhino;

using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Queue;
using Rasm.Rhino.Render.Slots;

using System.Runtime.InteropServices;
using Rasm.Imaging.Pixels;
using UnitsNet;
using UnitsNet.Units;

namespace Darkroom;

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class DarkroomSettings {
    // --- [ROWS]
    public static readonly QueueSettingRows Queue = new(
        Retries: new(
            ValueKey.Numeric<RetryCount, int>("QueueRetries", "Retries", "Times a crashed queue reopens Rhino and resumes", QueueSettings.Default.Retries,
                Conversions.Validated<RetryCount, int, InvalidRhinoValue>, static value => value, (RetryCount.MinValue, RetryCount.MaxValue), new() { Form = NumberForm.Field }),
            SettingType.Integer, Applied.Live, [], Hidden: false),
        Delay: new(
            ValueKey.Numeric("QueueRetryDelay", "Retry delay", "Wait before a crashed queue reopens", QueueSettings.Default.Delay,
                Conversions.Validated<RetryDelay, int, InvalidRhinoValue>, static value => value, (RetryDelay.MinValue, RetryDelay.MaxValue), RetryDelay.Presentation),
            SettingType.Integer, Applied.Live, [], Hidden: false),
        SampleEstimate: new(
            ValueKey.Raw("QueueSampleEstimate", "Estimate from first frames", "Renders each entry's first two frames ahead of the rest and posts the finish they estimate", QueueSettings.Default.SampleEstimate),
            SettingType.Bool, Applied.Live, [], Hidden: false),
        KeepAwake: new(
            ValueKey.Raw("KeepAwake", "Keep computer awake", "Holds off system sleep from the first queued render to the last", QueueSettings.Default.KeepAwake),
            SettingType.Bool, Applied.Live, [], Hidden: false),
        ShutDownAfterSuccess: new(
            ValueKey.Raw("ShutDownAfterSuccess", "Shut down when done", "Shuts the computer down 60 seconds after every queued render succeeds", QueueSettings.Default.ShutDownAfterSuccess),
            SettingType.Bool, Applied.Live, [], Hidden: false),
        HeartbeatInterval: new(
            ValueKey.Numeric("QueueHeartbeatInterval", "Heartbeat interval", "Time between queue heartbeat updates", QueueHeartbeat.Default.Interval,
                static seconds => Fin.Succ(Duration.FromSeconds(seconds)), static value => value.TotalSeconds,
                (QueueHeartbeat.Intervals.Low.TotalSeconds, QueueHeartbeat.Intervals.High.TotalSeconds), new() { Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(DurationUnit.Second) }),
            SettingType.Double, Applied.Live, [], Hidden: false),
        HeartbeatMisses: new(
            ValueKey.Numeric("QueueHeartbeatMisses", "Missed heartbeats", "Missed heartbeat intervals before a queue is considered crashed", QueueHeartbeat.Default.Misses,
                Fin.Succ, identity, QueueHeartbeat.MissedIntervals, new() { Form = NumberForm.Field }),
            SettingType.Integer, Applied.Live, [], Hidden: false),
        Server: new(
            ValueKey.Of<NoticeServer, string, InvalidRhinoValue>("NtfyServer", "Server", "ntfy server the render queue posts notices to", NoticeSettings.Default.Server),
            SettingType.String, Applied.Live, [], Hidden: false),
        Topic: new(
            ValueKey.Optional<NoticeTopic, InvalidRhinoValue>("NtfyTopic", "Topic", "ntfy topic the render queue posts notices to, empty for none"),
            SettingType.String, Applied.Live, [], Hidden: false),
        Flamenco: new(
            Manager: fun(static (string text) => Uri.TryCreate(text, UriKind.Absolute, out Uri? uri) && (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.Ordinal) || string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal))
                ? Fin.Succ(uri) : Fin.Fail<Uri>(new InvalidRhinoValue())) switch {
                    var read => new(new ValueKey<Uri, string>("FlamencoManager", "Manager", "Flamenco Manager address", new Uri("http://localhost:8080"),
                        new StateParameter<Uri>.Opaque<Uri>(Lens.identity<Uri>()), read, static value => value.AbsoluteUri, read),
                        SettingType.String, Applied.Live, [], Hidden: false),
                },
            Scripts: fun(static (string text) => Fin.Succ(Conversions.Present(text))) switch {
                var read => new(new ValueKey<Option<string>, string>("FlamencoScripts", "Scripts folder", "Manager scripts folder the install action writes to; empty for none", None,
                    new StateParameter<Option<string>>.Opaque<Option<string>>(Lens.identity<Option<string>>()), read, Conversions.Unset, read),
                    SettingType.String, Applied.Live, [], Hidden: false),
            },
            WorkerPlatform: fun(static (string text) => Conversions.Present(text).Match(
                Some: static held => Fin.Succ(OSPlatform.Create(held)), None: static () => Fin.Fail<OSPlatform>(new InvalidRhinoValue()))) switch {
                    var read => new(new ValueKey<OSPlatform, string>("FlamencoWorkerPlatform", "Worker platform", "Operating system of selected Rhino workers; use a matching worker tag for mixed platforms", Default: OperatingSystem.IsWindows() ? OSPlatform.Windows : OSPlatform.OSX,
                        new StateParameter<OSPlatform>.Opaque<OSPlatform>(Lens.identity<OSPlatform>()), read, static value => value.ToString(), read),
                        SettingType.String, Applied.Live, [], Hidden: false),
                },
            Priority: new(ValueKey.Numeric("FlamencoPriority", "Priority", "Priority of submitted Flamenco jobs", 50,
                Fin.Succ, identity, (1, 100), new() { Form = NumberForm.Field }),
                SettingType.Integer, Applied.Live, [], Hidden: false),
            WorkerTag: fun(static (string text) => Conversions.Present(text).Traverse(StoredText.Parse<Guid>).As()) switch {
                var read => new(new ValueKey<Option<Guid>, string>("FlamencoWorkerTag", "Worker tag", "Tag selecting workers with the chosen platform; empty allows any eligible Rhino worker", None,
                    new StateParameter<Option<Guid>>.Opaque<Option<Guid>>(Lens.identity<Option<Guid>>()), read, static value => Conversions.Unset(value.Map(StoredText.Format)), read),
                    SettingType.String, Applied.Live, [], Hidden: false),
            }));

    public static readonly HistoryLimits DefaultHistory = new(10, Duration.FromSeconds(5));

    public static readonly (PlugInSetting<int, int> Capacity, PlugInSetting<int, int> Window) History = (
        new(
            ValueKey.Numeric("HistorySteps", "Undo steps", "Edits each Darkroom section keeps for undo, redo, and restore; zero disables history", DefaultHistory.Capacity,
                Fin.Succ, identity, (0, int.MaxValue), new() { Form = NumberForm.Field, Soft = Some((1, 100)) }),
            SettingType.Integer, Applied.Live, [], Hidden: false),
        new(
            ValueKey.Numeric("HistoryMergeWindow", "Merge edits within", "Joins edits of one section made within this time of its last edit into one undo step", (int)DefaultHistory.Window.TotalSeconds,
                Fin.Succ, identity, (0, int.MaxValue), new() { Form = NumberForm.Field, Soft = Some((0, 600)), Unit = Quantity.GetUnitInfo(DurationUnit.Second) }),
            SettingType.Integer, Applied.Live, [], Hidden: false));

    public static readonly ComparisonSettingRows Comparison = new(
        PsnrAdvisory: new(
            ValueKey.Numeric<BarLevel, double>("ComparePsnrAdvisory", "PSNR advisory", "PSNR in decibels below which a compared render reads advisory", BarLevel.PsnrAdvisory,
                Conversions.Validated<BarLevel, double, InvalidPixelValue>, static value => value, (BarLevel.MinValue, BarLevel.MaxValue), new() { Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(LevelUnit.Decibel), Decimals = 1 }),
            SettingType.Double, Applied.Live, [], Hidden: false),
        PsnrRefuse: new(
            ValueKey.Numeric<BarLevel, double>("ComparePsnrRefuse", "PSNR refuse", "PSNR in decibels below which a compared render is refused", BarLevel.PsnrRefuse,
                Conversions.Validated<BarLevel, double, InvalidPixelValue>, static value => value, (BarLevel.MinValue, BarLevel.MaxValue), new() { Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(LevelUnit.Decibel), Decimals = 1 }),
            SettingType.Double, Applied.Live, [], Hidden: false),
        NrmseAdvisory: new(
            ValueKey.Numeric<BarLevel, double>("CompareNrmseAdvisory", "NRMSE advisory", "Normalized error above which a compared render reads advisory", BarLevel.NrmseAdvisory,
                Conversions.Validated<BarLevel, double, InvalidPixelValue>, static value => value, (BarLevel.MinValue, BarLevel.MaxValue), new() { Form = NumberForm.Field, Decimals = 2 }),
            SettingType.Double, Applied.Live, [], Hidden: false),
        NrmseRefuse: new(
            ValueKey.Numeric<BarLevel, double>("CompareNrmseRefuse", "NRMSE refuse", "Normalized error above which a compared render is refused", BarLevel.NrmseRefuse,
                Conversions.Validated<BarLevel, double, InvalidPixelValue>, static value => value, (BarLevel.MinValue, BarLevel.MaxValue), new() { Form = NumberForm.Field, Decimals = 2 }),
            SettingType.Double, Applied.Live, [], Hidden: false));

    public static readonly TileLimitRows Tiles = new(
        Batch: new(
            ValueKey.Numeric<TileBatch, int>("PreviewBatch", "Tiles per tick", "Preview tiles formed in one scheduling tick", TileBatch.Default,
                Conversions.Validated<TileBatch, int, InvalidRhinoValue>, static value => value, (TileBatch.MinValue, TileBatch.MaxValue), new() { Form = NumberForm.Field }),
            SettingType.Integer, Applied.Live, [], Hidden: false),
        Budget: new(
            ValueKey.Numeric<TileBudget, int>("PreviewBudget", "Time budget", "Time available to form preview tiles in one scheduling tick", TileBudget.Default,
                Conversions.Validated<TileBudget, int, InvalidRhinoValue>, static value => value, (TileBudget.MinValue, TileBudget.MaxValue), new() { Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(DurationUnit.Millisecond) }),
            SettingType.Integer, Applied.Live, [], Hidden: false),
        Capacity: new(
            ValueKey.Numeric<TileCapacity, int>("PreviewCapacity", "Cached tiles", "Preview tiles retained while they are not visible", TileCapacity.Default,
                Conversions.Validated<TileCapacity, int, InvalidRhinoValue>, static value => value, (TileCapacity.MinValue, TileCapacity.MaxValue), new() { Form = NumberForm.Field }),
            SettingType.Integer, Applied.Live, [], Hidden: false));

    // --- [REGISTRATION]
    public static readonly Seq<PlugInSetting> Settings = Queue.Rows + Seq<PlugInSetting>(History.Capacity, History.Window) + toSeq<PlugInSetting>(Comparison.Rows) + Tiles.Rows;

    public static IO<HistoryLimits> ReadHistory(ISettingReader reader) =>
        (reader.Current(History.Capacity), reader.Current(History.Window))
            .Apply(static (capacity, seconds) => new HistoryLimits(capacity, Duration.FromSeconds(seconds)))
            .As();
}

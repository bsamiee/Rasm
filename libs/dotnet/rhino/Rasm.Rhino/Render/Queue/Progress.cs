using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using NodaTime.Text;
using Rasm.Imaging.Files;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document.Files;

namespace Rasm.Rhino.Render.Queue;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class QueueHeartbeat {
    public Duration Interval { get; }
    public int Misses { get; }

    public static QueueHeartbeat Default { get; } = new(Duration.FromSeconds(3), 10);
    public static (Duration Low, Duration High) Intervals { get; } = (Duration.FromMilliseconds(1), Duration.FromMilliseconds(uint.MaxValue - 1));
    public static (int Low, int High) MissedIntervals { get; } = (2, int.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Duration interval, ref int misses) =>
        validationError = interval >= Intervals.Low && interval <= Intervals.High && misses >= MissedIntervals.Low
            ? null
            : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"progress needs an interval inside [{Intervals.Low}, {Intervals.High}] and at least {MissedIntervals.Low} missed intervals"));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RunEnd {
    public static readonly RunEnd Succeeded = new("succeeded");
    public static readonly RunEnd Stopped = new("stopped");
    public static readonly RunEnd Failed = new("failed");
}

[Union]
public abstract partial record RunState {
    public sealed record Absent : RunState;
    public sealed record Active(int ProcessId, Instant Heard) : RunState;
    public sealed record Ended(RunEnd End) : RunState;
    public sealed record Crashed(Instant Heard) : RunState;
}

public sealed record EntryProgress(Option<SequenceNumber> Last, Seq<Duration> Times, HashMap<Seq<NamePart>, OutputVersion> Versions) {
    public static EntryProgress Empty { get; } = new(None, Seq<Duration>(), HashMap<Seq<NamePart>, OutputVersion>());
}

public sealed record QueueProgress(
    Option<string> Job, int ProcessId, Instant ProcessStart, RetryCount Retries, RetryDelay Delay, Option<RunEnd> Ended, HashMap<ContentKey, EntryProgress> Entries) {
    // --- [FILE]
    public static IO<FileStream> Acquire(OutputPath path, Action<FileStream> retain) =>
        DisposalOps.Acquire(IO.lift(() => File.Open($"{(string)path}.lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)), retain);

    public static IO<Option<QueueProgress>> Read(OutputPath path) =>
        JsonFiles.Read(path, ProgressContext.Default.ProgressRecord, ProgressMapper.FromRecord, "render queue record");

    public static IO<Unit> Write(OutputPath path, QueueProgress progress) =>
        JsonFiles.Write(path, ProgressMapper.ToRecord(progress), ProgressContext.Default.ProgressRecord);

    public static IO<Unit> Touch(OutputPath path, TimeProvider clock) =>
        IO.lift(() => File.SetLastWriteTimeUtc(path, clock.GetUtcNow().UtcDateTime));

    public static IO<QueueProgress> Started(Option<string> job, RenderBatch queue, Option<QueueProgress> resumed, QueueSettings settings) =>
        DisposalOps.Within(Process.GetCurrentProcess, self => IO.lift(() => new QueueProgress(
                job,
                self.Id,
                self.StartTime.ToUniversalTime().ToInstant(),
                resumed.Map(static record => record.Retries).IfNone(settings.Retries),
                settings.Delay,
                None,
                toHashMap(queue.Entries.Map(entry => (entry.Key, resumed.Bind(record => record.Entries.Find(entry.Key)).IfNone(EntryProgress.Empty) with {
                    Versions = toHashMap(entry.Outputs.Choose(static output => output.Name.Version.Taken.Map(version => (output.Name.Scope, version)))),
                }))))));

    // --- [STATE]
    public bool Succeeded => Ended.Exists(static end => end == RunEnd.Succeeded);

    public long Landed => Entries.Values.Fold(0L, static (sum, entry) => sum + entry.Times.Count);

    public Option<Relaunch> Relaunch => Job.Map(job => new Relaunch(job, Delay));

    public static IO<(QueueSettings Settings, Option<QueueProgress> Prior, RunState State)> Observed(QueueContext context) =>
        from settings in context.Settings
        from prior in Read(context.Record)
        from state in prior.Match(
            Some: held => held.Ended.Match(
                Some: static end => IO.pure<RunState>(new RunState.Ended(end)),
                None: () =>
                    from heard in IO.lift(() => File.GetLastWriteTimeUtc(context.Record).ToInstant())
                    from now in IO.lift(context.Sink.Clock.GetCurrentInstant)
                    from live in Live(held)
                    select live && (now - heard).ToInt128Nanoseconds() < settings.Heartbeat.Interval.ToInt128Nanoseconds() * settings.Heartbeat.Misses
                        ? (RunState)new RunState.Active(held.ProcessId, heard)
                        : new RunState.Crashed(heard)),
            None: static () => IO.pure<RunState>(new RunState.Absent()))
        select (settings, prior, state);

    private static IO<bool> Live(QueueProgress record) =>
        DisposalOps.Within(() => Process.GetProcessById(record.ProcessId), process => IO.lift(() => process.StartTime.ToUniversalTime().ToInstant() == record.ProcessStart))
            .Catch(static error => error.HasException<ArgumentException>() || error.HasException<InvalidOperationException>(), static _ => IO.pure(value: false));
}

internal sealed record ProgressRecord(string? Job, int ProcessId, string ProcessStart, int Retries, int Delay, string? Ended, ProgressRecord.Entry[] Entries) {
    internal sealed record Entry(string Key, int? Last, string[] Times, Version[] Versions);
    internal sealed record Version(string[] Scope, int Number);
}

[JsonSerializable(typeof(ProgressRecord))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Strict, WriteIndented = true)]
internal sealed partial class ProgressContext : JsonSerializerContext;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ProgressMapper {
    // --- [OUTBOUND]
    internal static ProgressRecord ToRecord(QueueProgress progress) =>
        new(progress.Job.ValueUnsafe(), progress.ProcessId, InstantPattern.ExtendedIso.Format(progress.ProcessStart), progress.Retries, progress.Delay,
            progress.Ended.Map(static end => end.Key).ValueUnsafe(),
            [.. progress.Entries.AsIterable().Map(static row => new ProgressRecord.Entry(
                row.Key.ToString("x32", CultureInfo.InvariantCulture),
                row.Value.Last.Map(static last => (int)last).ToNullable(),
                [.. row.Value.Times.Map(static time => DurationPattern.Roundtrip.Format(time))],
                [.. row.Value.Versions.AsIterable().Map(static version => new ProgressRecord.Version([.. version.Key.Map(static part => (string)part)], version.Value))]))]);

    // --- [INBOUND]
    internal static Fin<QueueProgress> FromRecord(ProgressRecord record) =>
        (
            Parsed(InstantPattern.ExtendedIso.Parse(record.ProcessStart), record.ProcessStart),
            RetryCount.Validated(record.Retries),
            RetryDelay.Validated(record.Delay),
            Optional(record.Ended).Traverse(static end => RunEnd.Validated(end)).As(),
            toSeq(record.Entries).Traverse(Entry).As()
        )
            .Apply((start, retries, delay, end, entries) => new QueueProgress(Optional(record.Job), record.ProcessId, start, retries, delay, end, toHashMap(entries)))
            .As();

    internal static Fin<T> Parsed<T>(ParseResult<T> parsed, string text) =>
        parsed.Success ? parsed.Value : Unparsed(text);

    private static Error Unparsed(string text) => Errors.ParseError($"Render queue record text {text} is unreadable");

    private static Fin<(ContentKey Key, EntryProgress Progress)> Entry(ProgressRecord.Entry entry) =>
        (
            Invariant.Number<UInt128>(entry.Key, NumberStyles.HexNumber).ToFin(Unparsed(entry.Key)).Bind(ContentKey.Validated),
            Optional(entry.Last).Traverse(static last => SequenceNumber.Validated(last)).As(),
            toSeq(entry.Times).Traverse(static time => Parsed(DurationPattern.Roundtrip.Parse(time), time)).As(),
            toSeq(entry.Versions).Traverse(Version).As()
        ).Apply(static (key, last, times, versions) => (key, new EntryProgress(last, times, toHashMap(versions)))).As();

    private static Fin<(Seq<NamePart> Scope, OutputVersion Version)> Version(ProgressRecord.Version row) =>
        (
            toSeq(row.Scope).Traverse(NamePart.Validated).As(),
            OutputVersion.Validated(row.Number)
        ).Apply(static (scope, version) => (scope, version)).As();
}

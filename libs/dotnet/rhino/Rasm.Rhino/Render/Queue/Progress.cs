using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using NodaTime.Text;
using Rasm.Imaging.Output;
using Rasm.Rhino.Document.Files;

namespace Rasm.Rhino.Render.Queue;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
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
    public static Duration Heartbeat { get; } = Duration.FromSeconds(3);

    public static IO<Option<QueueProgress>> Read(OutputPath path) =>
        IO.lift(() => File.ReadAllBytes(path))
            .Bind(static bytes => IO.lift(() => Missing.Unless(JsonSerializer.Deserialize(bytes, ProgressContext.Default.ProgressRecord), nameof(JsonSerializer.Deserialize))))
            .Catch(static error => error.HasException<JsonException>(), error => IO.fail<ProgressRecord>(new ProgressUnreadable(path, error)))
            .Bind(record => IO.lift(ProgressMapper.FromRecord(record, path)))
            .Map(static progress => Some(progress))
            .Catch(static error => error.HasException<FileNotFoundException>(), static _ => IO.pure(Option<QueueProgress>.None));

    public static IO<Unit> Write(OutputPath path, QueueProgress progress) =>
        $"{(string)path}.partial" switch {
            var partial => IO.lift(() => File.WriteAllBytes(partial, JsonSerializer.SerializeToUtf8Bytes(ProgressMapper.ToRecord(progress), ProgressContext.Default.ProgressRecord)))
                .Bind(_ => IO.lift(() => File.Move(partial, path, overwrite: true))),
        };

    public static IO<Unit> Touch(OutputPath path, TimeProvider clock) =>
        IO.lift(() => File.SetLastWriteTimeUtc(path, clock.GetUtcNow().UtcDateTime));

    public static IO<QueueProgress> Started(Option<string> job, RenderBatch queue, Option<QueueProgress> resumed, QueueSettings settings) =>
        use(Process.GetCurrentProcess).Map(self => new QueueProgress(
                job,
                self.Id,
                self.StartTime.ToUniversalTime().ToInstant(),
                resumed.Map(static record => record.Retries).IfNone(settings.Retries),
                settings.Delay,
                None,
                toHashMap(queue.Entries.Map(entry => (entry.Key, resumed.Bind(record => record.Entries.Find(entry.Key)).IfNone(EntryProgress.Empty) with {
                    Versions = toHashMap(entry.Outputs.Choose(static output => output.Name.Version.Taken.Map(version => (output.Name.Scope, version)))),
                })))))
            .Bracket();

    // --- [STATE]
    public static Duration Stale { get; } = Duration.FromSeconds(30);

    public bool Succeeded => Ended.Exists(static end => end == RunEnd.Succeeded);

    public int Landed => Entries.Values.Fold(0, static (sum, entry) => sum + entry.Times.Count);

    public Option<Relaunch> Relaunch => Job.Map(job => new Relaunch(job, Delay));

    public static IO<RunState> State(Option<QueueProgress> record, OutputPath path, TimeProvider clock) =>
        record.Match(
            Some: held => held.Ended.Match(
                Some: static end => IO.pure<RunState>(new RunState.Ended(end)),
                None: () =>
                    from heard in IO.lift(() => File.GetLastWriteTimeUtc(path).ToInstant())
                    from now in IO.lift(clock.GetCurrentInstant)
                    from live in Live(held)
                    select live && now - heard <= Stale ? (RunState)new RunState.Active(held.ProcessId, heard) : new RunState.Crashed(heard)),
            None: static () => IO.pure<RunState>(new RunState.Absent()));

    private static IO<bool> Live(QueueProgress record) =>
        use(IO.lift(() => Process.GetProcessById(record.ProcessId)))
            .Map(process => process.StartTime.ToUniversalTime().ToInstant() == record.ProcessStart)
            .Bracket()
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
    internal static Fin<QueueProgress> FromRecord(ProgressRecord record, OutputPath path) =>
        (
            Parsed(InstantPattern.ExtendedIso.Parse(record.ProcessStart), record.ProcessStart),
            Conversions.Validated<RetryCount, int, InvalidRhinoValue>(record.Retries).ToValidation(),
            Conversions.Validated<RetryDelay, int, InvalidRhinoValue>(record.Delay).ToValidation(),
            Optional(record.Ended).Traverse(static end => Conversions.Validated<RunEnd, string, InvalidRhinoValue>(end).ToValidation()).As(),
            toSeq(record.Entries).Traverse(Entry).As()
        )
            .Apply((start, retries, delay, end, entries) => new QueueProgress(Optional(record.Job), record.ProcessId, start, retries, delay, end, toHashMap(entries)))
            .As()
            .ToFin()
            .MapFail(error => new ProgressUnreadable(path, error));

    internal static Validation<Error, T> Parsed<T>(ParseResult<T> parsed, string text) =>
        parsed.Success ? parsed.Value : new ProgressTextRefused(text);

    private static Validation<Error, (ContentKey Key, EntryProgress Progress)> Entry(ProgressRecord.Entry entry) =>
        (
            (UInt128.TryParse(entry.Key, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out UInt128 key)
                ? Conversions.Validated<ContentKey, UInt128, InvalidRhinoValue>(key)
                : Fin.Fail<ContentKey>(new ProgressTextRefused(entry.Key))).ToValidation(),
            Optional(entry.Last).Traverse(static last => Conversions.Validated<SequenceNumber, int, InvalidRhinoValue>(last).ToValidation()).As(),
            toSeq(entry.Times).Traverse(static time => Parsed(DurationPattern.Roundtrip.Parse(time), time)).As(),
            toSeq(entry.Versions).Traverse(Version).As()
        ).Apply(static (key, last, times, versions) => (key, new EntryProgress(last, times, toHashMap(versions)))).As();

    private static Validation<Error, (Seq<NamePart> Scope, OutputVersion Version)> Version(ProgressRecord.Version row) =>
        (
            toSeq(row.Scope).Traverse(static part => Conversions.Validated<NamePart, string, InvalidRhinoValue>(part).ToValidation()).As(),
            Conversions.Validated<OutputVersion, int, InvalidRhinoValue>(row.Number).ToValidation()
        ).Apply(static (scope, version) => (scope, version)).As();
}

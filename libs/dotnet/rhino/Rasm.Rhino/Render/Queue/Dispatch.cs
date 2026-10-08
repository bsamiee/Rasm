using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using NodaTime.Text;
using Rasm.Imaging.Output;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Geolocation;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Scenes;
using Rasm.Rhino.Render.Sessions;
using Rhino;
using Rhino.Commands;
using Rhino.PlugIns;
using Rhino.Render;

namespace Rasm.Rhino.Render.Queue;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record QueueTarget {
    public sealed record Local : QueueTarget;
    public sealed record Handoff(Destination Package) : QueueTarget;
    public sealed record JobFile(Destination Package) : QueueTarget;
}

internal sealed partial record QueueJob(int RhinoVersion, string Copy, QueueJob.Entry[] Entries) {
    internal sealed record Entry(
        string Scene,
        ValueSet Values,
        float? Clay,
        RenderSettings.RenderingSources Source,
        string? SpecificViewport,
        string? NamedView,
        string? Snapshot,
        int First,
        int Last,
        int Width,
        int Height,
        double Overscan,
        Region? Region,
        Study? Sun,
        Output[] Outputs);

    internal sealed record Region(double Left, double Top, double Right, double Bottom);

    [Union]
    [JsonPolymorphic]
    [JsonDerivedType(typeof(Day), "day")]
    [JsonDerivedType(typeof(Season), "season")]
    internal abstract partial record Study {
        public sealed record Day(string Date, string From, string Until, int Minutes) : Study;
        public sealed record Season(string From, string Until, string At, int Days) : Study;
    }

    internal sealed record Output(int Frame, ValueSet Target, string[] Scope, int? Version, string[] Parts, string Path);
}

[JsonSerializable(typeof(QueueJob))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Strict, WriteIndented = true, UseStringEnumConverter = true, Converters = [typeof(ValueSetConverter)])]
internal sealed partial class JobContext : JsonSerializerContext;

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
internal sealed partial class PackageFile {
    public static readonly PackageFile Copy = new(".3dm", None, static (doc, _, path, _) =>
        Exchange.WriteFile(doc, path, static options => (options.IncludeBitmapTable, options.IncludeRenderMeshes) = (true, true)));

    public static readonly PackageFile Job = new(".json", None, static (_, queue, path, copy) =>
        IO.lift(() => File.WriteAllBytes(path, JsonSerializer.SerializeToUtf8Bytes(JobMapper.ToJob(queue, copy), JobContext.Default.QueueJob))));

    public static readonly PackageFile Schema = new(".json", Some("schema"), static (_, _, path, _) =>
        IO.lift(() => File.WriteAllText(path, JobContext.Default.QueueJob.GetJsonSchemaAsNode().ToJsonString(JobContext.Default.Options))));

    private readonly string _extension;
    private readonly Option<string> _part;

    [UseDelegateFromConstructor]
    public partial IO<Unit> Write(RhinoDoc doc, RenderBatch queue, OutputPath path, OutputPath copy);

    public Validation<Error, OutputName> Name(NamePart stamp) =>
        (_part.Traverse(static part => Conversions.Validated<NamePart, string, InvalidRhinoValue>(part).ToValidation()).As(),
         Conversions.Validated<FileExtension, string, InvalidRhinoValue>(_extension).ToValidation())
            .Apply((part, extension) => new OutputName(Seq(stamp), new OutputVersioning.Unversioned(), part.ToSeq(), None, extension))
            .As();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class JobMapper {
    // --- [OUTBOUND]
    private const string TargetOwner = nameof(QueueOutput.Target);

    internal static QueueJob ToJob(RenderBatch queue, string copy) =>
        new(RhinoApp.ExeVersion, copy, [.. queue.Entries.Map(static entry => new QueueJob.Entry(
            entry.Scene.Name, entry.Scene.Values, entry.Scene.Clay.Map(static clay => (float)clay).ToNullable(),
            entry.Source.RenderSource, entry.Source.SpecificViewport.ValueUnsafe(), entry.Source.NamedView.ValueUnsafe(), entry.Source.Snapshot.ValueUnsafe(),
            entry.Frames.First, entry.Frames.Last, entry.Extent.Width, entry.Extent.Height, entry.Overscan,
            entry.Region.Map(Region).ValueUnsafe(),
            entry.Sun.Map(Study).ValueUnsafe(),
            [.. entry.Outputs.Map(Output)]))]);

    internal static QueueJob.Region Region(RenderRegion region) => new(region.Left, region.Top, region.Right, region.Bottom);

    internal static QueueJob.Study Study(SunWindow window) =>
        window.Switch<QueueJob.Study>(
            day: static day => new QueueJob.Study.Day(
                LocalDatePattern.Iso.Format(day.Start.Date), LocalTimePattern.ExtendedIso.Format(day.Start.TimeOfDay), LocalTimePattern.ExtendedIso.Format(day.End.TimeOfDay),
                day.MinutesBetweenFrames),
            season: static season => new QueueJob.Study.Season(
                LocalDatePattern.Iso.Format(season.Start.Date), LocalDatePattern.Iso.Format(season.End.Date), LocalTimePattern.ExtendedIso.Format(season.Start.TimeOfDay),
                season.DaysBetweenFrames));

    private static QueueJob.Output Output(QueueOutput output) =>
        new(
            output.Frame,
            new ValueSet(toHashMap(FieldTexts<OutputTarget>.Items.Choose(item => item.Capture(output.Target).Map(text => (new EntryKey(TargetOwner, item.Path), text))))),
            [.. output.Name.Scope.Map(static part => (string)part)],
            output.Name.Version.Taken.Map(static version => (int)version).ToNullable(),
            [.. output.Name.Parts.Map(static part => (string)part)],
            output.Path);

    // --- [INBOUND]
    internal static IO<RenderBatch> FromJob(QueueJob job) =>
        Callbacks.Each(toSeq(job.Entries).Map(static (entry, index) => Entry(entry).MapFail(error => new JobEntryRejected(index, error))))
            .Map(static entries => new RenderBatch(entries));

    internal static Validation<Error, RenderRegion> Region(QueueJob.Region region) =>
        (Fraction(region.Left), Fraction(region.Top), Fraction(region.Right), Fraction(region.Bottom))
            .Apply(static (left, top, right, bottom) => (Left: left, Top: top, Right: right, Bottom: bottom))
            .As()
            .Bind(static edges => (RenderRegion.Validate(edges.Left, edges.Top, edges.Right, edges.Bottom, out RenderRegion? held) is { } error
                ? Fin.Fail<RenderRegion>(error)
                : Fin.Succ(held!)).ToValidation());

    internal static Validation<Error, SunWindow> Window(QueueJob.Study study) =>
        study.Switch(
            day: static day =>
                (Date(day.Date), Time(day.From), Time(day.Until))
                    .Apply(static (date, from, until) => (Date: date, From: from, Until: until))
                    .As()
                    .Bind(span => SunWindow.Day.Create(span.Date, span.From, span.Until, Period.FromMinutes(day.Minutes)).Map(static held => (SunWindow)held).ToValidation()),
            season: static season =>
                (Date(season.From), Date(season.Until), Time(season.At))
                    .Apply(static (from, until, at) => (From: from, Until: until, At: at))
                    .As()
                    .Bind(span => SunWindow.Season.Create(span.From, span.Until, span.At, Period.FromDays(season.Days)).Map(static held => (SunWindow)held).ToValidation()));

    private static IO<QueueEntry<QueueOutput>> Entry(QueueJob.Entry entry) =>
        from columns in IO.lift((
                Optional(entry.Clay).Traverse(static clay => Conversions.Validated<Albedo, float, InvalidRhinoValue>(clay).ToValidation()).As(),
                FrameSpan.Of(entry.First, entry.Last).ToValidation(),
                (PixelExtent.Validate(entry.Width, entry.Height, out PixelExtent sized) is { } refused ? Fin.Fail<PixelExtent>(refused) : Fin.Succ(sized)).ToValidation(),
                Conversions.Validated<Overscan, double, InvalidRhinoValue>(entry.Overscan).ToValidation(),
                Optional(entry.Region).Traverse(Region).As(),
                Optional(entry.Sun).Traverse(Window).As())
            .Apply((clay, frames, extent, overscan, region, sun) => new QueueEntry<QueueJob.Output>(
                new RenderOverride(entry.Scene, entry.Values, clay),
                new RenderSourceState(entry.Source, Optional(entry.SpecificViewport), Optional(entry.NamedView), Optional(entry.Snapshot)),
                frames, extent, overscan, region, sun, toSeq(entry.Outputs)))
            .As()
            .ToFin())
        from outputs in Callbacks.Each(columns.Outputs.Map(output => Output(columns.Frames, output)))
        select columns.Placed(outputs);

    private static IO<QueueOutput> Output(FrameSpan frames, QueueJob.Output output) =>
        from folded in FieldTexts.Folded<OutputTarget>(TargetOwner, path => output.Target.Entries.Find(new EntryKey(TargetOwner, path)))
        from placed in IO.lift((
                (folded.Refused.IsEmpty ? Fin.Succ(folded.Record) : Fin.Fail<OutputTarget>(Error.Many(folded.Refused))).ToValidation(),
                Number(output.Frame),
                toSeq(output.Scope).Traverse(Part).As(),
                toSeq(output.Parts).Traverse(Part).As(),
                Optional(output.Version).Traverse(static version => Conversions.Validated<OutputVersion, int, InvalidRhinoValue>(version).ToValidation()).As(),
                Conversions.Validated<OutputPath, string, InvalidOutput>(output.Path).ToValidation())
            .Apply((target, frame, scope, parts, version, path) => new PlannedOutput(target, scope, parts)
                .Name(frames.Named(frame), version.Match<OutputVersioning>(Some: static held => held, None: static () => new OutputVersioning.Unversioned()))
                .Map(name => new QueueOutput(frame, target, name, path)))
            .As()
            .ToFin()
            .Bind(static named => named))
        select placed;

    private static Validation<Error, LocalDate> Date(string text) => ProgressMapper.Parsed(LocalDatePattern.Iso.Parse(text), text);

    private static Validation<Error, LocalTime> Time(string text) => ProgressMapper.Parsed(LocalTimePattern.ExtendedIso.Parse(text), text);

    private static Validation<Error, SequenceNumber> Number(int number) => Conversions.Validated<SequenceNumber, int, InvalidRhinoValue>(number).ToValidation();

    private static Validation<Error, FrameFraction> Fraction(double edge) => Conversions.Validated<FrameFraction, double, InvalidRhinoValue>(edge).ToValidation();

    private static Validation<Error, NamePart> Part(string text) => Conversions.Validated<NamePart, string, InvalidRhinoValue>(text).ToValidation();
}

public static class Dispatch {
    // --- [TARGETS]
    public static IO<Unit> Send(QueueContext context, RhinoDoc doc, QueuePlan plan, QueueTarget target, LocalDateTime stamp, Option<QueueProgress> resumed) =>
        target.Switch(
            (Context: context, Doc: doc, Plan: plan, Stamp: stamp, Resumed: resumed),
            local: static (state, _) =>
                from queue in Queues.Resolve(state.Doc, state.Plan, state.Resumed)
                from forked in Reported(state.Context, nameof(Send), Queues.Run(state.Context, state.Doc, queue, None, state.Resumed)).Fork()
                select unit,
            handoff: static (state, handoff) =>
                from commands in CommandRegistry.GetCommands(state.Context.PlugIn.Id)
                from job in IO.lift(commands.Find(static command => command is QueueJobCommand).ToFin(new Missing(nameof(QueueJobCommand))))
                from saved in SaveAll
                from paths in Packed(state.Doc, state.Plan, state.Resumed, handoff.Package, state.Stamp)
                from moved in state.Resumed.Traverse(record => QueueProgress.Write(
                    state.Context.Record, record with { Job = Some((string)paths[PackageFile.Job]), Ended = record.Ended | Some(RunEnd.Stopped) })).As()
                from relayed in Relay(paths[PackageFile.Job], job.EnglishName)
                from quit in Quit(state.Context)
                select unit,
            jobFile: static (state, file) => Packed(state.Doc, state.Plan, state.Resumed, file.Package, state.Stamp).Map(static _ => unit));

    private static IO<Unit> Reported(QueueContext context, string member, IO<Unit> run) =>
        run.Catch(error => IO.lift(() => context.Sink.Report(error, typeof(Dispatch), member))).As();

    // --- [PACKAGE]
    private static IO<HashMap<PackageFile, OutputPath>> Packed(RhinoDoc doc, QueuePlan plan, Option<QueueProgress> resumed, Destination package, LocalDateTime stamp) =>
        from queue in Queues.Resolve(doc, plan, resumed)
        from date in IO.lift(Conversions.Validated<NamePart, string, InvalidRhinoValue>(LocalDateTimePattern.ExtendedIso.Format(stamp.With(TimeAdjusters.TruncateToSecond))))
        from names in IO.lift(toSeq(PackageFile.Items).Traverse(file => file.Name(date)).As().ToFin())
        from placed in Destinations.Resolve(doc, package, names)
        let paths = toHashMap(toSeq(PackageFile.Items).Zip(placed, static (file, row) => (file, row.Path)))
        from written in toSeq(PackageFile.Items).TraverseM(file => file.Write(doc, queue, paths[file], paths[PackageFile.Copy])).As()
        select paths;

    // --- [HANDOFF]
    private static IO<Unit> SaveAll =>
        IO.lift(static () => Conversions.Rows(RhinoDoc.OpenDocuments(includeHeadless: false)))
            .Bind(static open => open.TraverseM(Exchange.Save).As())
            .Map(static _ => unit);

    private static IO<Unit> Relay(string job, string command) =>
        IO.lift(() => Launch(job, command))
            .Bind(static start => use(IO.lift(() => Missing.Unless(Process.Start(start), nameof(Process.Start)))).Map(static _ => unit).Bracket());

    private static Fin<ProcessStartInfo> Launch(string job, string command) =>
        OperatingSystem.IsMacOS()
            ? new ProcessStartInfo("/bin/sh") {
                ArgumentList = {
                    "-c", "pid=$1; shift; /usr/bin/caffeinate -w \"$pid\"; exec /usr/bin/open \"$@\"", "sh",
                    Environment.ProcessId.ToString(CultureInfo.InvariantCulture),
                    "-g", "-b", string.Create(CultureInfo.InvariantCulture, $"com.mcneel.rhinoceros.{RhinoApp.ExeVersion}"),
                    "--env", $"{JobVariable}={job}", "--args", "-runscript=_NoEcho", $"_{command}",
                },
                UseShellExecute = false, }
            : Missing.Unless(Environment.ProcessPath, nameof(Environment.ProcessPath)).Map(path => new ProcessStartInfo(path) {
                ArgumentList = { "/nosplash", $"/runscript=_NoEcho _{command}" },
                Environment = { [JobVariable] = job },
                UseShellExecute = false, });

    private static IO<Unit> Quit(QueueContext context) =>
        from flushed in IO.lift(PlugIn.FlushSettingsSavedQueue)
        from exited in IO.lift(static () => RhinoApp.Exit(allowCancel: false))
        from forked in Reported(context, nameof(Quit), IO.yieldFor(QueueProgress.Stale.ToTimeSpan())
                .Bind(static _ => use(Process.GetCurrentProcess).Bind(static self => IO.lift(() => self.Kill())).Bracket()))
            .Fork()
        select unit;

    // --- [JOBS]
    private const string JobVariable = "RASM_RENDER_JOB";

    public static IO<ForkIO<Unit>> Job(QueueContext context) =>
        IO.lift(static () => Conversions.Present(Environment.GetEnvironmentVariable(JobVariable)).ToFin(new JobUnset(JobVariable)))
            .Bind(path => Launched(context, nameof(Job), Run(context, path)).Fork());

    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Relaunched(QueueContext context) =>
        (_, sink) =>
            Reported(context, nameof(Relaunched), Queues.Resumable(context)
                    .Bind(found => found.Traverse(relaunch =>
                        Launched(context, nameof(Relaunched), IO.yieldFor(relaunch.Delay.Span.ToTimeSpan()).Bind(_ => Run(context, relaunch.Job)))).As())
                    .Map(static _ => unit))
                .Fork()
                .Map(fork => (IDisposable)new Disposal<ForkIO<Unit>>(fork, held => Callbacks.Succeeded(held.Cancel, new CallbackSite(sink, typeof(Dispatch), nameof(Relaunched)))));

    private static IO<Unit> Launched(QueueContext context, string member, IO<Unit> run) =>
        Reported(context, member, run).Bind(_ => Quit(context).Post());

    private static IO<Unit> Run(QueueContext context, string path) =>
        from job in Read(path)
        from prior in QueueProgress.Read(context.Record)
        from doc in Documents.WithDocument(new DocumentSource.Opened(job.Copy), IO.pure).Post()
        from ran in Queues.Run(context, doc, job.Queue, Some(path), prior.Filter(record => record.Job == Some(path) && !record.Succeeded))
        select unit;

    private static IO<(string Copy, RenderBatch Queue)> Read(string path) =>
        from existing in IO.lift(() => Exchange.ExistingPath(path))
        from job in IO.lift(() => Missing.Unless(JsonSerializer.Deserialize(File.ReadAllBytes(existing), JobContext.Default.QueueJob), nameof(JsonSerializer.Deserialize)))
            .Catch(static error => error.HasException<JsonException>(), error => IO.fail<QueueJob>(new JobUnreadable(existing, error)))
        from queue in JobMapper.FromJob(job)
        select (job.Copy, queue);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class QueueJobCommand(IPlugInSink sink, Guid id, string englishName) : HostCommand(sink, id, englishName, None) {
    protected override IO<Unit> Run(RhinoDoc doc, RunMode mode, CallbackSite site) =>
        IPlugInRendering.Served(((IPlugInRendering)PlugIn).QueueContext, nameof(IPlugInRendering.QueueContext))
            .Bind(Dispatch.Job)
            .Map(static _ => unit);
}

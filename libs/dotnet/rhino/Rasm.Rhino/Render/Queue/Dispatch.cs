using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using Eto;
using NodaTime.Text;
using Rasm.Imaging.Output;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Geolocation;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Scenes;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Views;
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
    public sealed record Flamenco(Destination Package, Uri Manager, OSPlatform WorkerPlatform, int Priority, Option<Guid> WorkerTag) : QueueTarget;
}

public sealed record FlamencoSettingRows(
    PlugInSetting<Uri, string> Manager,
    PlugInSetting<string, string> Scripts,
    PlugInSetting<bool, bool> ScriptsEnabled,
    PlugInSetting<OSPlatform, string> WorkerPlatform,
    PlugInSetting<int, int> Priority,
    PlugInSetting<Option<Guid>, string> WorkerTag) {
    public Seq<PlugInSetting> Rows => Seq<PlugInSetting>(Manager, Scripts, ScriptsEnabled, WorkerPlatform, Priority, WorkerTag);

    public Seq<Child> Children =>
        RowSource.Keyed(nameof(PlugInSettings), ScriptsEnabled.Value, scope => PlugInSettings.Store(((IPlugInViews)scope.Sink).Settings, ScriptsEnabled)) switch {
            var gate => RowRule.When(gate.Source, identity, gate.Field.Parameter) switch {
                var enabled =>
                    (Seq(Text(Manager, RowRules.Always))
                     + SectionRows.Setting(ScriptsEnabled, RowRules.Always)
                     + Seq(Text(Scripts, new RowRules(Some(enabled), None), Some((FileAction.SelectFolder, Seq<FileTypeRow>()))),
                         RowSource.Keyed(nameof(PlugInSettings), WorkerPlatform.Value, scope => PlugInSettings.Store(((IPlugInViews)scope.Sink).Settings, WorkerPlatform)) switch {
                             var pair => ChoiceRows.Choice(pair.Source, pair.Field, Lens.identity<OSPlatform>(), static _ => IO.pure(
                                 Seq(OSPlatform.OSX, OSPlatform.Windows).Map(static platform => new ChoiceItem<OSPlatform>(platform, platform.ToString(), None))), RowRules.Always),
                         })
                     + SectionRows.Setting(Priority, RowRules.Always)
                     + Seq(Text(WorkerTag, RowRules.Always), SectionRows.Pushed(new CommandRow.Run(
                         new CommandFace(nameof(Dispatch.InstallFlamencoCompiler), RowText.Localize("Install Flamenco job type").Local,
                             RowText.Localize("Write the Rhino job compiler to the configured Manager scripts folder").Local, None, None) { Wording = Wording.Localized },
                         Some(enabled), scope => ((IPlugInViews)scope.Sink).Settings.Current(ScriptsEnabled).Bind(on => on
                             ? ((IPlugInViews)scope.Sink).Settings.Current(Scripts).Bind(Dispatch.InstallFlamencoCompiler)
                             : IO.fail<Unit>(new Missing(ScriptsEnabled.Value.Name)))))))
                        .Map(static row => (Child)row),
            },
        };

    public IO<QueueTarget> Target(ISettingReader reader, Destination package) =>
        (reader.Current(Manager), reader.Current(WorkerPlatform), reader.Current(Priority), reader.Current(WorkerTag))
            .Apply((manager, platform, priority, tag) => (QueueTarget)new QueueTarget.Flamenco(package, manager, platform, priority, tag))
            .As();

    private static ControlRow Text<TValue>(PlugInSetting<TValue, string> row, RowRules rules, Option<(FileAction Action, Seq<FileTypeRow> Types)> browse = default) where TValue : notnull =>
        RowSource.Keyed(nameof(PlugInSettings), row.Value, scope => PlugInSettings.Store(((IPlugInViews)scope.Sink).Settings, row)) switch {
            var pair => TextRows.Typed(pair.Source, pair.Field, row.Value.Text, (_, text) => row.Value.Read(text), new EntryForm.SingleLine(), browse, rules),
        };
}

internal sealed record FlamencoJob(string Name, string Type, string TypeEtag, int Priority, string SubmitterPlatform, FlamencoJob.Parameters Settings, Guid? WorkerTag) {
    internal sealed record Parameters(string Copy, string[] Arguments);
}

internal sealed record FlamencoReceipt(Guid Id);

internal sealed record FlamencoTypes(FlamencoTypes.JobType[] JobTypes) {
    internal sealed record JobType(string Name, string Etag);
}

[JsonSerializable(typeof(FlamencoJob))]
[JsonSerializable(typeof(FlamencoReceipt))]
[JsonSerializable(typeof(FlamencoTypes))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, RespectRequiredConstructorParameters = true, RespectNullableAnnotations = true)]
internal sealed partial class FlamencoContext : JsonSerializerContext;

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
        ValueSet Destination,
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

    internal sealed record Output(int Frame, ValueSet Target, string[] Scope, int? Version, string[] Parts);
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

    public readonly string Extension;
    private readonly Option<string> _part;

    [UseDelegateFromConstructor]
    public partial IO<Unit> Write(RhinoDoc doc, RenderBatch queue, OutputPath path, OutputPath copy);

    public Validation<Error, OutputName> Name(NamePart stamp) =>
        (_part.Traverse(static part => Conversions.Validated<NamePart, string, InvalidRhinoValue>(part).ToValidation()).As(),
         Conversions.Validated<FileExtension, string, InvalidRhinoValue>(Extension).ToValidation())
            .Apply((part, extension) => new OutputName(Seq(stamp), new OutputVersioning.Unversioned(), part.ToSeq(), None, extension))
            .As();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class JobMapper {
    // --- [OUTBOUND]
    private const string TargetOwner = nameof(QueueOutput<>.Target);
    private const string DestinationOwner = nameof(QueueEntry<>.Destination);

    internal static QueueJob ToJob(RenderBatch queue, string copy) =>
        new(RhinoApp.ExeVersion, copy, [.. queue.Entries.Map(static entry => new QueueJob.Entry(
            entry.Scene.Name, entry.Scene.Values, entry.Scene.Clay.Map(static clay => (float)clay).ToNullable(),
            entry.Source.RenderSource, entry.Source.SpecificViewport.ValueUnsafe(), entry.Source.NamedView.ValueUnsafe(), entry.Source.Snapshot.ValueUnsafe(),
            entry.Frames.First, entry.Frames.Last, entry.Extent.Width, entry.Extent.Height, entry.Overscan,
            entry.Region.Map(Region).ValueUnsafe(),
            entry.Sun.Map(Study).ValueUnsafe(),
            new ValueSet(toHashMap(FieldTexts.Items<Destination>().Choose(item => item.Capture(entry.Destination).Map(text => (new EntryKey(DestinationOwner, item.Path), text))))),
            [.. entry.Outputs.Map(output => Output(entry.Frames, output))]))]);

    internal static QueueJob.Region Region(RenderRegion region) => new(region.Left, region.Top, region.Right, region.Bottom);

    internal static QueueJob.Study Study(SunWindow window) =>
        window.Switch<QueueJob.Study>(
            day: static day => new QueueJob.Study.Day(
                LocalDatePattern.Iso.Format(day.Start.Date), LocalTimePattern.ExtendedIso.Format(day.Start.TimeOfDay), LocalTimePattern.ExtendedIso.Format(day.End.TimeOfDay),
                day.MinutesBetweenFrames),
            season: static season => new QueueJob.Study.Season(
                LocalDatePattern.Iso.Format(season.Start.Date), LocalDatePattern.Iso.Format(season.End.Date), LocalTimePattern.ExtendedIso.Format(season.Start.TimeOfDay),
                season.DaysBetweenFrames));

    private static QueueJob.Output Output(FrameSpan frames, QueueOutput<OutputPath> output) =>
        new(
            output.Name.Number.IfNone(frames.First),
            new ValueSet(toHashMap(FieldTexts.Items<OutputTarget>().Choose(item => item.Capture(output.Target).Map(text => (new EntryKey(TargetOwner, item.Path), text))))),
            [.. output.Name.Scope.Map(static part => (string)part)],
            output.Name.Version.Taken.Map(static version => (int)version).ToNullable(),
            [.. output.Name.Parts.Map(static part => (string)part)]);

    // --- [INBOUND]
    internal static IO<RenderBatch> FromJob(RhinoDoc doc, QueueJob job) =>
        Callbacks.Each(toSeq(job.Entries).Map((entry, index) => Entry(doc, entry).MapFail(error => new JobEntryRejected(index, error))))
            .Bind(static entries => IO.lift(QueuePlan.Unique(entries)))
            .Bind(entries => Queues.Placed(doc, entries, overwrite: true));

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

    private static IO<QueueEntry<Unit>> Entry(RhinoDoc doc, QueueJob.Entry entry) =>
        from stored in FieldTexts.Folded(DestinationOwner, path => IO.pure(entry.Destination.Entries.Find(new EntryKey(DestinationOwner, path))), Destination.Default)
        from columns in IO.lift((
                (stored.Refused.IsEmpty ? Destinations.Resolved(stored.Record, Conversions.Present(doc.Path)) : Fin.Fail<Destination>(Error.Many(stored.Refused))).ToValidation(),
                Optional(entry.Clay).Traverse(static clay => Conversions.Validated<Albedo, float, InvalidRhinoValue>(clay).ToValidation()).As(),
                FrameSpan.Of(entry.First, entry.Last).ToValidation(),
                (PixelExtent.Validate(entry.Width, entry.Height, out PixelExtent sized) is { } refused ? Fin.Fail<PixelExtent>(refused) : Fin.Succ(sized)).ToValidation(),
                Conversions.Validated<Overscan, double, InvalidRhinoValue>(entry.Overscan).ToValidation(),
                Optional(entry.Region).Traverse(Region).As(),
                Optional(entry.Sun).Traverse(Window).As())
            .Apply(static (destination, clay, frames, extent, overscan, region, sun) => (Destination: destination, Clay: clay, Frames: frames, Extent: extent, Overscan: overscan, Region: region, Sun: sun))
            .As()
            .ToFin())
        from outputs in Callbacks.Each(toSeq(entry.Outputs).Map(output => Output(columns.Frames, output)))
        select new QueueEntry<Unit>(
            new RenderOverride(entry.Scene, entry.Values, columns.Clay),
            new RenderSourceState(entry.Source, Optional(entry.SpecificViewport), Optional(entry.NamedView), Optional(entry.Snapshot)),
            columns.Frames, columns.Extent, columns.Overscan, columns.Region, columns.Sun, columns.Destination, outputs);

    private static IO<QueueOutput<Unit>> Output(FrameSpan frames, QueueJob.Output output) =>
        from folded in FieldTexts.Folded(TargetOwner, path => IO.pure(output.Target.Entries.Find(new EntryKey(TargetOwner, path))), OutputTarget.Default)
        from placed in IO.lift((
                (folded.Refused.IsEmpty ? Fin.Succ(folded.Record) : Fin.Fail<OutputTarget>(Error.Many(folded.Refused))).ToValidation(),
                Number(output.Frame).Bind(frame => frame >= frames.First && frame <= frames.Last
                    ? Fin.Succ(frame).ToValidation()
                    : Fin.Fail<SequenceNumber>(new JobFrameOutsideSpan(frame, frames.First, frames.Last)).ToValidation()),
                toSeq(output.Scope).Traverse(Part).As(),
                toSeq(output.Parts).Traverse(Part).As(),
                Optional(output.Version).Traverse(static version => Conversions.Validated<OutputVersion, int, InvalidRhinoValue>(version).ToValidation()).As())
            .Apply((target, frame, scope, parts, version) => Conversions.Validated<FileExtension, string, InvalidRhinoValue>(target.FileFormat.Extension)
                .Map(extension => new QueueOutput<Unit>(target, new OutputName(scope,
                    version.Match<OutputVersioning>(Some: static held => held, None: static () => new OutputVersioning.Unversioned()), parts, frames.Named(frame), extension), unit)))
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
    public static IO<Unit> Send(
        QueueContext context,
        DocumentSource source,
        QueuePlan plan,
        QueueTarget target,
        LocalDateTime stamp,
        Option<QueueProgress> resumed,
        Action<IDisposable> retain,
        Option<Func<Preflight, IO<Unit>>> report = default) =>
        Documents.WithDocument(source, doc =>
            from check in Preflight.Check(context, doc, plan, target).OnUiThread()
            from shown in report.Traverse(publish => publish(check).OnUiThread()).As()
            from verdict in IO.lift(check.Verdict.ToFin())
            from sent in target.Switch(
                (Context: context, Doc: doc, Plan: plan, Stamp: stamp, Resumed: resumed),
                local: static (state, _) =>
                    Queues.Resolve(state.Doc, state.Plan, state.Resumed).OnUiThread()
                        .Bind(queue => Queues.Run(state.Context, state.Doc, queue, None, state.Resumed)),
                handoff: static (state, handoff) => Handed(state.Context, state.Doc, state.Plan, state.Stamp, state.Resumed, handoff.Package),
                jobFile: static (state, file) => Packed(state.Doc, state.Plan, state.Resumed, file.Package, state.Stamp).OnUiThread().Map(static _ => unit),
                flamenco: static (state, farm) =>
                    from arguments in IO.lift(FarmArguments(farm.WorkerPlatform, state.Context.FarmScript))
                    from paths in Packed(state.Doc, state.Plan, state.Resumed, farm.Package, state.Stamp).OnUiThread()
                    from submitted in Submitted(farm, paths[PackageFile.Copy], arguments)
                    from noticed in state.Context.Notify(new QueueEvent.Submitted(submitted))
                    select unit)
            select sent) switch {
                var send => target.Map(local: true, handoff: false, jobFile: false, flamenco: false)
                    ? DisposalOps.Fork(Reported(context, nameof(Send), send), retain).Map(static _ => unit)
                    : send,
            };

    private static IO<Unit> Reported(QueueContext context, string member, IO<Unit> run) =>
        DisposalOps.OnFailure(run, error => IO.lift(() => context.Sink.Report(error, typeof(Dispatch), member)));

    // --- [FLAMENCO]
    private const string CompilerFile = "rasm_rhino_render.js";

    public static IO<Unit> InstallFlamencoCompiler(string folder) =>
        IO.lift(() => Exchange.QualifiedPath(folder)).Bind(static path =>
            IO.liftAsync(async environment => {
                Stream source = typeof(Dispatch).Assembly.GetManifestResourceStream($"{typeof(Dispatch).Namespace}.{CompilerFile}")!;
                await using (source.ConfigureAwait(false)) {
                    DirectoryInfo directory = Directory.CreateDirectory(path);
                    FileStream target = File.Create(Path.Combine(directory.FullName, CompilerFile));
                    await using (target.ConfigureAwait(false))
                        await source.CopyToAsync(target, environment.Token).ConfigureAwait(false);
                }
                return unit;
            }));

    private static Fin<string[]> FarmArguments(OSPlatform platform, string script) =>
        platform == OSPlatform.OSX ? Fin.Succ(new[] { "-runscript", script })
            : platform == OSPlatform.Windows ? Fin.Succ(new[] { $"/runscript=\"{script}\"" })
            : Fin.Fail<string[]>(new FarmPlatformUnsupported(platform));

    public static IO<bool> FarmStartup(Seq<string> farmScripts) =>
        IO.lift(() => {
            bool mac = OperatingSystem.IsMacOS();
#if RHINO_MACOS_BINDINGS
            string[] arguments = mac ? Foundation.NSProcessInfo.ProcessInfo.Arguments : Environment.GetCommandLineArgs();
#else
            string[] arguments = Environment.GetCommandLineArgs();
#endif
            return farmScripts.Traverse(script => FarmArguments(mac ? OSPlatform.OSX : OSPlatform.Windows, script)).As()
                .Map(expected => expected.Exists(suffix => arguments.AsSpan().EndsWith(suffix.AsSpan())));
        });

    private static IO<Uri> Submitted(QueueTarget.Flamenco farm, string copy, string[] arguments) =>
        DisposalOps.Within(static () => new HttpClient(), client =>
            from advertised in IO.liftAsync(environment => client.GetFromJsonAsync(new Uri(farm.Manager, "/api/v3/jobs/types"), FlamencoContext.Default.FlamencoTypes, environment.Token))
            from types in IO.lift(Missing.Unless(advertised, nameof(HttpClientJsonExtensions.GetFromJsonAsync)))
            let type = Path.GetFileNameWithoutExtension(CompilerFile).Replace('_', '-')
            from compiled in IO.lift(toSeq(types.JobTypes).Find(job => string.Equals(job.Name, type, StringComparison.Ordinal)).ToFin(new Missing(type)))
            from submitted in IO.liftAsync(async environment => {
                using JsonContent content = JsonContent.Create(new FlamencoJob(
                    Path.GetFileNameWithoutExtension(copy), type, compiled.Etag, farm.Priority,
                    OperatingSystem.IsMacOS() ? "darwin" : "windows", new FlamencoJob.Parameters(copy, arguments), farm.WorkerTag.ToNullable()),
                    FlamencoContext.Default.FlamencoJob);
                using HttpResponseMessage response = await client.PostAsync(new Uri(farm.Manager, "/api/v3/jobs"), content, environment.Token).ConfigureAwait(false);
                return response.StatusCode == HttpStatusCode.OK
                    ? Missing.Unless(await response.Content.ReadFromJsonAsync(FlamencoContext.Default.FlamencoReceipt, environment.Token).ConfigureAwait(false), nameof(HttpContentJsonExtensions.ReadFromJsonAsync))
                        .Map(receipt => new Uri(farm.Manager, $"/jobs/{receipt.Id:D}"))
                    : Fin.Fail<Uri>(new FlamencoRefused(response.StatusCode));
            })
            from received in IO.lift(submitted)
            select received, static client => IO.lift(client.Dispose))
            .Catch(static error => error.HasException<HttpRequestException>(), error => IO.fail<Uri>(new FlamencoUnreached(farm.Manager, error)));

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
    private static IO<Unit> Handed(QueueContext context, RhinoDoc doc, QueuePlan plan, LocalDateTime stamp, Option<QueueProgress> resumed, Destination package) =>
        DisposalOps.Within<IDisposable, Unit>(acquired =>
            from lease in QueueProgress.Acquire(context.Record, acquired)
            from command in context.JobCommand.OnUiThread()
            from saved in SaveAll.OnUiThread()
            from paths in Packed(doc, plan, resumed, package, stamp).OnUiThread()
            from moved in resumed.Traverse(record => QueueProgress.Write(
                context.Record, record with { Job = Some((string)paths[PackageFile.Job]), Ended = record.Ended | Some(RunEnd.Stopped) })).As()
            from relayed in Relay(context, paths[PackageFile.Job], command)
            from quit in Quit.OnUiThread()
            select unit, DisposalOps.Release);

    private static IO<Unit> SaveAll =>
        IO.lift(static () => Conversions.Rows(RhinoDoc.OpenDocuments(includeHeadless: false)))
            .Bind(static open => open.TraverseM(Exchange.Save).As())
            .Map(static _ => unit);

    private static IO<Unit> Relay(QueueContext context, string job, string command) =>
        IO.lift(() => Launch(job, command, JobVariable(context)))
            .Bind(static start => DisposalOps.Within(() => Process.Start(start),
                static child => IO.lift(Missing.Unless(child, nameof(Process.Start))).Map(static _ => unit),
                static child => IO.lift(() => Optional(child).Iter(static held => held.Dispose()))));

    private static Fin<ProcessStartInfo> Launch(string job, string command, string variable) =>
        OperatingSystem.IsMacOS()
            ? new ProcessStartInfo("sh") {
                ArgumentList = {
                    "-c", "pid=$1; shift; caffeinate -w \"$pid\"; exec open \"$@\"", "sh",
                    Environment.ProcessId.ToString(CultureInfo.InvariantCulture),
                    "-g", "-b", string.Create(CultureInfo.InvariantCulture, $"com.mcneel.rhinoceros.{RhinoApp.ExeVersion}"),
                    "--env", $"{variable}={job}", "--args", "-runscript=_NoEcho", $"_{command}",
                },
                UseShellExecute = false, }
            : Missing.Unless(Environment.ProcessPath, nameof(Environment.ProcessPath)).Map(path => new ProcessStartInfo(path) {
                ArgumentList = { "/nosplash", $"/runscript=_NoEcho _{command}" },
                Environment = { [variable] = job },
                UseShellExecute = false, });

    private static IO<Unit> Quit =>
        from flushed in IO.lift(PlugIn.FlushSettingsSavedQueue)
        from exited in IO.lift(static () => RhinoApp.Exit(allowCancel: false))
        select unit;

    // --- [JOBS]
    private static string JobVariable(QueueContext context) => $"RASM_RENDER_JOB_{context.Owner:N}";

    public static IO<ForkIO<Unit>> Job(QueueContext context) =>
        JobVariable(context) switch {
            var variable => IO.lift(() => Conversions.Present(Environment.GetEnvironmentVariable(variable)).ToFin(new JobUnset(variable)))
                .Bind(path => DisposalOps.Fork(Launched(context, nameof(Job), Run(context, path)), context.Retain)),
        };

    public static IO<Unit> FarmJob(QueueContext context, RhinoDoc doc, IO<Unit> close) =>
        IO.lift(parent => {
            IO<Unit> work =
                from path in IO.lift(() => Exchange.QualifiedPath(Path.ChangeExtension(doc.Path, PackageFile.Job.Extension))).OnUiThread()
                from job in Read(path)
                from record in IO.lift(Conversions.Validated<OutputPath, string, InvalidOutput>(Path.ChangeExtension(path, ".progress.json")))
                let worker = context with { Record = record, Settings = context.Settings.Map(static settings => settings with { ShutDownAfterSuccess = false }) }
                from prior in QueueProgress.Read(worker.Record)
                from queue in JobMapper.FromJob(doc, job).OnUiThread()
                from ran in Queues.Run(worker, doc, queue, Some(path), prior.Filter(held => held.Job == Some(path) && !held.Succeeded))
                select unit;
            _ = Task.Run(async () => {
                EnvIO environment = EnvIO.New(token: parent.Token, syncContext: parent.SyncContext);
                Fin<Unit> outcome = await DisposalOps.Within(static () => unit, _ => work, _ => close.OnUiThread())
                    .Try().Run().As().Uninterruptible().RunAsync(environment).ConfigureAwait(false);
                Fin<Unit> result = (outcome.ToValidation(),
                        Try.lift(environment.DisposeResources).ToFin().ToValidation(),
                        Try.lift(environment.DisposeNonResources).ToFin().ToValidation())
                    .Apply(static (_, _, _) => unit).As().ToFin();
                int status = result.Match(Succ: static _ => 0, Fail: static error => {
                    Console.Error.WriteLine(error);
                    return Queues.Stopping(error) ? 2 : 1;
                });
                Environment.Exit(status);
            }, parent.Token);
            return unit;
        });

    public static IO<Unit> Startup(QueueContext context, Action<IDisposable> retain, Seq<string> farmScripts) =>
        FarmStartup(farmScripts).Bind(farm => unless(farm,
            IO.lift(() => Conversions.Present(Environment.GetEnvironmentVariable(JobVariable(context))))
                .Bind(job => job.Match(
                    Some: path => DisposalOps.Fork(Launched(context, nameof(Startup), Run(context, path)), retain).Map(static _ => unit),
                    None: () => Relaunched(context, retain)))).As());

    public static IO<Unit> Relaunched(QueueContext context, Action<IDisposable> retain) =>
        DisposalOps.Fork(DisposalOps.OnFailure(Queues.Resumable(context),
                    error => IO.lift(() => context.Sink.Report(error, typeof(Dispatch), nameof(Relaunched))))
                    .Bind(found => found.Traverse(relaunch =>
                        Launched(context, nameof(Relaunched), IO.yieldFor(relaunch.Delay.Span.ToTimeSpan()).Bind(_ => Run(context, relaunch.Job)))).As())
                    .Map(static _ => unit), retain).Map(static _ => unit);

    private static IO<Unit> Launched(QueueContext context, string member, IO<Unit> run) =>
        DisposalOps.Within(static () => unit, _ => Reported(context, member, run), _ => Reported(context, member, Quit.OnUiThread()));

    private static IO<Unit> Run(QueueContext context, string path) =>
        from job in Read(path)
        from prior in QueueProgress.Read(context.Record)
        from ran in Documents.WithDocument(new DocumentSource.Opened(job.Copy), doc =>
            JobMapper.FromJob(doc, job).OnUiThread()
                .Bind(queue => Queues.Run(context, doc, queue, Some(path), prior.Filter(record => record.Job == Some(path) && !record.Succeeded))))
        select unit;

    private static IO<QueueJob> Read(string path) =>
        from existing in IO.lift(() => Exchange.ExistingPath(path))
        from job in IO.lift(() => Missing.Unless(JsonSerializer.Deserialize(File.ReadAllBytes(existing), JobContext.Default.QueueJob), nameof(JsonSerializer.Deserialize)))
            .Catch(static error => error.HasException<JsonException>(), error => IO.fail<QueueJob>(new JobUnreadable(existing, error)))
        select job;
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class QueueJobCommand(IPlugInSink sink, Guid id, string englishName, IO<QueueContext> context) : HostCommand(sink, id, englishName, None) {
    protected override IO<Unit> Run(RhinoDoc doc, RunMode mode, CallbackSite site) =>
        context.Bind(Dispatch.Job)
            .Map(static _ => unit);
}

public sealed class QueueFarmJobCommand(IPlugInSink sink, Guid id, string englishName, IO<QueueContext> context, IO<Unit> close) : HostCommand(sink, id, englishName, None) {
    protected override IO<Unit> Run(RhinoDoc doc, RunMode mode, CallbackSite site) =>
        mode == RunMode.Scripted
            ? context.Bind(held => Dispatch.FarmJob(held, doc, close))
            : IO.fail<Unit>(new FarmScriptRequired());
}

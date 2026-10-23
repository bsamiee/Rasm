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
using Rasm.Imaging.Files;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
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
    PlugInSetting<Option<string>, string> Scripts,
    PlugInSetting<OSPlatform, string> WorkerPlatform,
    PlugInSetting<int, int> Priority,
    PlugInSetting<Option<Guid>, string> WorkerTag) {
    public static FlamencoSettingRows Default { get; } = new(
        Manager: new(new ValueKey<Uri, string>("FlamencoManager", "Manager", "Flamenco Manager address", new Uri("http://localhost:8080"),
            new StateParameter<Uri>.Opaque<Uri>(Lens.identity<Uri>()), Address, static value => value.AbsoluteUri, Address), SettingType.String),
        Scripts: new(new ValueKey<Option<string>, string>("FlamencoScripts", "Scripts folder", "Manager scripts folder for Rhino job compiler, empty for none", None,
            new StateParameter<Option<string>>.Opaque<Option<string>>(Lens.identity<Option<string>>()), static text => Conversions.Present(text), Conversions.Unset,
            static text => Conversions.Present(text)), SettingType.String),
        WorkerPlatform: new(new ValueKey<OSPlatform, string>("FlamencoWorkerPlatform", "Worker platform", "Rhino worker operating system, mixed farms need matching worker tag",
            Default: OperatingSystem.IsWindows() ? OSPlatform.Windows : OSPlatform.OSX,
            new StateParameter<OSPlatform>.Opaque<OSPlatform>(Lens.identity<OSPlatform>()), Platform, static value => value.ToString(), Platform), SettingType.String),
        Priority: new(ValueKey.Numeric("FlamencoPriority", "Priority", "Priority of submitted Flamenco jobs", 50,
            new NumericDomain<int, int>(Fin.Succ, identity, (1, 100), new() { Form = NumberForm.Field })), SettingType.Integer),
        WorkerTag: new(new ValueKey<Option<Guid>, string>("FlamencoWorkerTag", "Worker tag", "Tag selecting workers of chosen platform, empty for any Rhino worker", None,
            new StateParameter<Option<Guid>>.Opaque<Option<Guid>>(Lens.identity<Option<Guid>>()), Tag, static value => Conversions.Unset(value.Map(StoredText.Format)), Tag), SettingType.String));

    public Seq<PlugInSetting> Rows => [Manager, Scripts, WorkerPlatform, Priority, WorkerTag];

    public Seq<Child> Children =>
        SectionRows.Setting(Scripts) switch {
            var folder => [
                Text(Manager, RowRules.Always),
                Text(Scripts, RowRules.Always, Some((FileAction.SelectFolder, Seq<FileTypeRow>()))),
                SectionRows.Setting(WorkerPlatform) switch {
                    var pair => ChoiceRows.Choice(pair.Source, pair.Field, Lens.identity<OSPlatform>(), static _ => IO.pure(
                        Seq(OSPlatform.OSX, OSPlatform.Windows).Map(static platform => new ChoiceItem<OSPlatform>(platform, platform.ToString(), None))), RowRules.Always),
                },
                .. SectionRows.Setting(Priority, RowRules.Always),
                Text(WorkerTag, RowRules.Always),
                SectionRows.Pushed(new CommandRow.Run(
                    new CommandFace(nameof(Dispatch.InstallFlamencoCompiler), "Install Flamenco job type",
                        "Write Rhino job compiler to Manager scripts folder", None, None) { Wording = Wording.Library },
                    Some(RowRule.When(folder.Source, static held => held.IsSome, folder.Field.Parameter)),
                    scope => IO.lift(() => ((IPlugInViews)scope.Sink).Settings.Current(Scripts).Bind(held => held.ToFin(new Refused(Scripts.Value.Name, Refused.Absent))))
                        .Bind(Dispatch.InstallFlamencoCompiler))),
            ],
        };

    public IO<QueueTarget> Target(ISettingReader reader, Destination package) =>
        IO.lift(() => (reader.Current(Manager), reader.Current(WorkerPlatform), reader.Current(Priority), reader.Current(WorkerTag))
            .Apply((manager, platform, priority, tag) => (QueueTarget)new QueueTarget.Flamenco(package, manager, platform, priority, tag))
            .As());

    private static ControlRow Text<TValue>(PlugInSetting<TValue, string> row, RowRules rules, Option<(FileAction Action, Seq<FileTypeRow> Types)> browse = default) where TValue : notnull =>
        SectionRows.Setting(row) switch {
            var pair => TextRows.Typed(pair.Source, pair.Field, row.Value.Text, (_, text) => row.Value.Read(text), new EntryForm.SingleLine(), browse, rules),
        };

    private static Fin<Uri> Address(string text) =>
        Uri.TryCreate(text, UriKind.Absolute, out Uri? uri) && (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.Ordinal) || string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal))
            ? uri
            : Error.New(ErrorCodes.ValidationFailed, $"{text} is no absolute http or https address");

    private static Fin<OSPlatform> Platform(string text) =>
        Conversions.Present(text).Map(OSPlatform.Create).ToFin(Error.New(ErrorCodes.ValidationFailed, "Worker platform is empty"));

    private static Fin<Option<Guid>> Tag(string text) => Conversions.Present(text).Traverse(StoredText.Parse<Guid>).As();
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
        string Override,
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
[JsonSourceGenerationOptions(JsonSerializerDefaults.Strict, WriteIndented = true, UseStringEnumConverter = true)]
internal sealed partial class JobContext : JsonSerializerContext;

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
internal sealed partial class PackageFile {
    public static readonly PackageFile Copy = new(".3dm", None, static (doc, _, path, _) =>
        Exchange.WriteFile(doc, path, static options => (options.IncludeBitmapTable, options.IncludeRenderMeshes) = (true, true)));

    public static readonly PackageFile Job = new(".json", None, static (_, queue, path, copy) =>
        JsonFiles.Write(path, JobMapper.ToJob(queue, copy), JobContext.Default.QueueJob));

    public static readonly PackageFile Schema = new(".json", Some("schema"), static (_, _, path, _) =>
        IO.lift(static () => JobContext.Default.QueueJob.GetJsonSchemaAsNode().ToJsonString(JobContext.Default.Options)).Bind(text => FilePublication.Write(path, text)));

    public readonly string Extension;
    private readonly Option<string> _part;

    [UseDelegateFromConstructor]
    public partial IO<Unit> Write(RhinoDoc doc, RenderBatch queue, OutputPath path, OutputPath copy);

    public Fin<OutputName> Name(NamePart stamp) =>
        (_part.Traverse(NamePart.Validated).As(), FileExtension.Validated(Extension))
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
            entry.Override.Name, entry.Override.Values, entry.Override.Clay.Map(static clay => (float)clay).ToNullable(),
            entry.Source.RenderSource, entry.Source.SpecificViewport.ValueUnsafe(), entry.Source.NamedView.ValueUnsafe(), entry.Source.Snapshot.ValueUnsafe(),
            entry.Frames.First, entry.Frames.Last, entry.Extent.Width, entry.Extent.Height, entry.Overscan,
            entry.Region.Map(Region).ValueUnsafe(),
            entry.Sun.Map(Study).ValueUnsafe(),
            ValueSet.Fields(DestinationOwner, entry.Destination),
            [.. entry.Outputs.Map(output => Output(entry.Frames, output))]))]);

    private static QueueJob.Region Region(RenderRegion region) => new(region.Left, region.Top, region.Right, region.Bottom);

    private static QueueJob.Study Study(SunWindow window) =>
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
            ValueSet.Fields(TargetOwner, output.Target),
            [.. output.Name.Scope.Map(static part => (string)part)],
            output.Name.Version.Taken.Map(static version => (int)version).ToNullable(),
            [.. output.Name.Parts.Map(static part => (string)part)]);

    // --- [INBOUND]
    internal static IO<RenderBatch> FromJob(RhinoDoc doc, QueueJob job) =>
        Callbacks.Each(toSeq(job.Entries).Map((entry, index) => Entry(doc, entry).MapFail(error =>
                Error.New(string.Create(CultureInfo.InvariantCulture, $"Render queue job entry {index} is invalid"), error))))
            .Bind(static entries => IO.lift(QueuePlan.Unique(entries)))
            .Bind(entries => Queues.Placed(doc, entries, overwrite: true));

    private static Fin<RenderRegion> Region(QueueJob.Region region) =>
        (FrameFraction.Validated(region.Left), FrameFraction.Validated(region.Top), FrameFraction.Validated(region.Right), FrameFraction.Validated(region.Bottom))
            .Apply(static (left, top, right, bottom) => (Left: left, Top: top, Right: right, Bottom: bottom))
            .As()
            .Bind(static edges => ValidationResults.ToFin(RenderRegion.Validate(edges.Left, edges.Top, edges.Right, edges.Bottom, out RenderRegion? held), held));

    private static Fin<SunWindow> Window(QueueJob.Study study) =>
        study.Switch(
            day: static day =>
                (Date(day.Date), Time(day.From), Time(day.Until))
                    .Apply(static (date, from, until) => (Date: date, From: from, Until: until))
                    .As()
                    .Bind(span => SunWindow.Day.Create(span.Date, span.From, span.Until, Period.FromMinutes(day.Minutes)))
                    .Map(static held => (SunWindow)held),
            season: static season =>
                (Date(season.From), Date(season.Until), Time(season.At))
                    .Apply(static (from, until, at) => (From: from, Until: until, At: at))
                    .As()
                    .Bind(span => SunWindow.Season.Create(span.From, span.Until, span.At, Period.FromDays(season.Days)))
                    .Map(static held => (SunWindow)held));

    private static IO<QueueEntry<Unit>> Entry(RhinoDoc doc, QueueJob.Entry entry) =>
        from stored in FieldTexts.Folded(DestinationOwner, entry.Destination.Texts(DestinationOwner), Destination.Default)
        from columns in IO.lift((
                stored.Refused.IsEmpty ? Destinations.Resolved(stored.Record, Conversions.Present(doc.Path)) : Error.Many(stored.Refused),
                Optional(entry.Clay).Traverse(static clay => Albedo.Validated(clay)).As(),
                FrameSpan.Of(entry.First, entry.Last),
                PixelExtent.Validated(entry.Width, entry.Height),
                Overscan.Validated(entry.Overscan),
                Optional(entry.Region).Traverse(Region).As(),
                Optional(entry.Sun).Traverse(Window).As())
            .Apply(static (destination, clay, frames, extent, overscan, region, sun) => (Destination: destination, Clay: clay, Frames: frames, Extent: extent, Overscan: overscan, Region: region, Sun: sun))
            .As())
        from outputs in Callbacks.Each(toSeq(entry.Outputs).Map(output => Output(columns.Frames, output)))
        select new QueueEntry<Unit>(
            new RenderOverride(entry.Override, entry.Values, columns.Clay),
            new RenderSourceState(entry.Source, Optional(entry.SpecificViewport), Optional(entry.NamedView), Optional(entry.Snapshot)),
            columns.Frames, columns.Extent, columns.Overscan, columns.Region, columns.Sun, columns.Destination, outputs);

    private static IO<QueueOutput<Unit>> Output(FrameSpan frames, QueueJob.Output output) =>
        from folded in FieldTexts.Folded(TargetOwner, output.Target.Texts(TargetOwner), OutputTarget.Default)
        from placed in IO.lift((
                folded.Refused.IsEmpty ? Fin.Succ(folded.Record) : Error.Many(folded.Refused),
                SequenceNumber.Validated(output.Frame).Bind(frame => frame >= frames.First && frame <= frames.Last
                    ? Fin.Succ(frame)
                    : Error.New(string.Create(CultureInfo.InvariantCulture, $"Job frame {frame} lies outside entry range {frames.First} to {frames.Last}"))),
                toSeq(output.Scope).Traverse(NamePart.Validated).As(),
                toSeq(output.Parts).Traverse(NamePart.Validated).As(),
                Optional(output.Version).Traverse(static version => OutputVersion.Validated(version)).As())
            .Apply((target, frame, scope, parts, version) => FileExtension.Validated(target.FileFormat.Extension)
                .Map(extension => new QueueOutput<Unit>(target, new OutputName(scope,
                    version.Match<OutputVersioning>(Some: static held => held, None: static () => new OutputVersioning.Unversioned()), parts, frames.Named(frame), extension), unit)))
            .As()
            .Bind(static named => named))
        select placed;

    private static Fin<LocalDate> Date(string text) => ProgressMapper.Parsed(LocalDatePattern.Iso.Parse(text), text);

    private static Fin<LocalTime> Time(string text) => ProgressMapper.Parsed(LocalTimePattern.ExtendedIso.Parse(text), text);
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
        Option<Func<Preflight, IO<Unit>>> report = default) =>
        Documents.WithDocument(source, doc =>
            from check in Preflight.Check(context, doc, plan, target).OnUiThread()
            from shown in report.Traverse(publish => publish(check).OnUiThread()).As()
            from verdict in IO.lift(check.Verdict)
            from sent in target.Switch(
                (Context: context, Doc: doc, Plan: plan, Stamp: stamp, Resumed: resumed),
                local: static (state, _) =>
                    Queues.Resolve(state.Doc, state.Plan, state.Resumed).OnUiThread()
                        .Bind(queue => Queues.Run(state.Context, state.Doc, queue, None, state.Resumed)),
                handoff: static (state, handoff) => Handed(state.Context, state.Doc, state.Plan, state.Stamp, state.Resumed, handoff.Package),
                jobFile: static (state, file) => Packed(state.Doc, state.Plan, state.Resumed, file.Package, state.Stamp).OnUiThread().Map(unit),
                flamenco: static (state, farm) =>
                    from arguments in IO.lift(FarmArguments(farm.WorkerPlatform, state.Context.FarmScript))
                    from paths in Packed(state.Doc, state.Plan, state.Resumed, farm.Package, state.Stamp).OnUiThread()
                    from submitted in Submitted(farm, paths[PackageFile.Copy], arguments)
                    from noticed in state.Context.Notify(new QueueEvent.Submitted(submitted))
                    select unit)
            select sent) switch {
                var send => target.Map(local: true, handoff: false, jobFile: false, flamenco: false)
                    ? DisposalOps.Fork(Reported(context, nameof(Send), send), context.Retain).Map(unit)
                    : send,
            };

    private static IO<T> Reported<T>(QueueContext context, string member, IO<T> run) =>
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
        platform == OSPlatform.OSX ? new[] { "-runscript", script }
            : platform == OSPlatform.Windows ? new[] {
                $"""
                /runscript="{script}"
                """,
            }
            : Error.New($"Rhino farm workers run on macOS or Windows, not {platform}");

    public static IO<bool> FarmStartup(string farmScript) =>
        IO.lift(() => {
            bool mac = OperatingSystem.IsMacOS();
#if !WINDOWS
            string[] arguments = mac ? Foundation.NSProcessInfo.ProcessInfo.Arguments : Environment.GetCommandLineArgs();
#else
            string[] arguments = Environment.GetCommandLineArgs();
#endif
            return FarmArguments(mac ? OSPlatform.OSX : OSPlatform.Windows, farmScript).Map(expected => arguments.AsSpan().EndsWith(expected.AsSpan()));
        });

    private static IO<Uri> Submitted(QueueTarget.Flamenco farm, string copy, string[] arguments) =>
        DisposalOps.Within(static () => new HttpClient(), client =>
            from advertised in IO.liftAsync(environment => client.GetFromJsonAsync(new Uri(farm.Manager, "/api/v3/jobs/types"), FlamencoContext.Default.FlamencoTypes, environment.Token))
            from types in IO.lift(Refused.UnlessFound(advertised, nameof(HttpClientJsonExtensions.GetFromJsonAsync)))
            let type = Path.GetFileNameWithoutExtension(CompilerFile).Replace('_', '-')
            from compiled in IO.lift(toSeq(types.JobTypes).Find(job => string.Equals(job.Name, type, StringComparison.Ordinal)).ToFin(new Refused(type, Refused.Absent)))
            from submitted in IO.liftAsync(async environment => {
                using JsonContent content = JsonContent.Create(new FlamencoJob(
                    Path.GetFileNameWithoutExtension(copy), type, compiled.Etag, farm.Priority,
                    OperatingSystem.IsMacOS() ? "darwin" : "windows", new FlamencoJob.Parameters(copy, arguments), farm.WorkerTag.ToNullable()),
                    FlamencoContext.Default.FlamencoJob);
                using HttpResponseMessage response = await client.PostAsync(new Uri(farm.Manager, "/api/v3/jobs"), content, environment.Token).ConfigureAwait(false);
                return response.StatusCode is HttpStatusCode.OK
                    ? Refused.UnlessFound(await response.Content.ReadFromJsonAsync(FlamencoContext.Default.FlamencoReceipt, environment.Token).ConfigureAwait(false), nameof(HttpContentJsonExtensions.ReadFromJsonAsync))
                        .Map(receipt => new Uri(farm.Manager, $"/jobs/{receipt.Id:D}"))
                    : Error.New($"Flamenco refused render job with {response.StatusCode}");
            })
            from received in IO.lift(submitted)
            select received)
            .Catch(static error => error.HasException<HttpRequestException>(), error => IO.fail<Uri>(Error.New($"Render job post to {farm.Manager} failed", error)));

    // --- [PACKAGE]
    private static IO<HashMap<PackageFile, OutputPath>> Packed(RhinoDoc doc, QueuePlan plan, Option<QueueProgress> resumed, Destination package, LocalDateTime stamp) =>
        from queue in Queues.Resolve(doc, plan, resumed)
        from date in IO.lift(NamePart.Validated(LocalDateTimePattern.ExtendedIso.Format(stamp.With(TimeAdjusters.TruncateToSecond))))
        from names in IO.lift(toSeq(PackageFile.Items).Traverse(file => file.Name(date)).As())
        from placed in Destinations.Resolve(doc, package, names)
        let paths = toHashMap(toSeq(PackageFile.Items).Zip(placed, static (file, row) => (file, row.Path)))
        from written in toSeq(PackageFile.Items).TraverseM(file => file.Write(doc, queue, paths[file], paths[PackageFile.Copy])).As()
        select paths;

    // --- [HANDOFF]
    private static IO<Unit> Handed(QueueContext context, RhinoDoc doc, QueuePlan plan, LocalDateTime stamp, Option<QueueProgress> resumed, Destination package) =>
        DisposalOps.Within<IDisposable, Unit>(acquired =>
            from lease in QueueProgress.Acquire(context.Record, acquired)
            from saved in IO.lift(static () => Conversions.Rows(RhinoDoc.OpenDocuments())).Bind(static open => open.TraverseM(Exchange.Save).As()).OnUiThread()
            from paths in Packed(doc, plan, resumed, package, stamp).OnUiThread()
            from moved in resumed.Traverse(record => QueueProgress.Write(
                context.Record, record with { Job = Some((string)paths[PackageFile.Job]), Ended = record.Ended | Some(RunEnd.Stopped) })).As()
            from relayed in IO.lift(() => Launch(paths[PackageFile.Job], context.JobCommand, JobVariable(context)))
                .Bind(static start => DisposalOps.Found(() => Process.Start(start), nameof(Process.Start), static _ => IO.pure(unit)))
            from quit in Quit.OnUiThread()
            select unit, DisposalOps.Release);

    private static Fin<ProcessStartInfo> Launch(string job, string command, string variable) =>
        OperatingSystem.IsMacOS()
            ? new ProcessStartInfo("sh") {
                ArgumentList = {
                    "-c", """
                        pid=$1; shift; caffeinate -w "$pid"; exec open "$@"
                        """, "sh",
                    Environment.ProcessId.ToString(CultureInfo.InvariantCulture),
                    "-g", "-b", string.Create(CultureInfo.InvariantCulture, $"com.mcneel.rhinoceros.{RhinoApp.ExeVersion}"),
                    "--env", $"{variable}={job}", "--args", "-runscript=_NoEcho", $"_{command}",
                },
                UseShellExecute = false, }
            : Refused.UnlessFound(Environment.ProcessPath, nameof(Environment.ProcessPath)).Map(path => new ProcessStartInfo(path) {
                ArgumentList = { "/nosplash", $"/runscript=_NoEcho _{command}" },
                Environment = { [variable] = job },
                UseShellExecute = false, });

    private static IO<Unit> Quit =>
        from flushed in IO.lift(PlugIn.FlushSettingsSavedQueue)
        from exited in IO.lift(static () => RhinoApp.Exit(allowCancel: false))
        select unit;

    // --- [JOBS]
    private static string JobVariable(QueueContext context) => $"RASM_RENDER_JOB_{context.Sink.PlugIn.Id:N}";

    public static IO<ForkIO<Unit>> Job(QueueContext context) =>
        JobVariable(context) switch {
            var variable => IO.lift(() => Conversions.Present(Environment.GetEnvironmentVariable(variable)).ToFin(Error.New($"Environment variable {variable} is empty or unset")))
                .Bind(path => DisposalOps.Fork(Launched(context, nameof(Job), Run(context, path)), context.Retain)),
        };

    public static IO<Unit> FarmJob(QueueContext context, RhinoDoc doc, IO<Unit> close) =>
        (from code in Farmed(context, doc).Map(static _ => 0).Catch(error => IO.lift(() => context.Sink.Report(error, typeof(Dispatch), nameof(FarmJob))).Map(_ => Queues.Stopping(error) ? 2 : 1)).As()
         from exited in close.OnUiThread().Finally(IO.lift(() => Environment.Exit(code)).OnUiThread())
         select exited).Uninterruptible().Fork().Map(unit);

    private static IO<Unit> Farmed(QueueContext context, RhinoDoc doc) =>
        from path in IO.lift(() => Exchange.QualifiedPath(Path.ChangeExtension(doc.Path, PackageFile.Job.Extension))).OnUiThread()
        from job in Read(path)
        from record in IO.lift(OutputPath.Validated(Path.ChangeExtension(path, ".progress.json")))
        from ran in Ran(context with { Record = record, Settings = context.Settings.Map(static settings => settings with { ShutDownAfterSuccess = false }) }, doc, path, job)
        select ran;

    public static IO<Unit> Relaunched(QueueContext context) =>
        DisposalOps.Fork(Reported(context, nameof(Relaunched), Queues.Resumable(context))
            .Bind(found => found.Traverse(relaunch =>
                Launched(context, nameof(Relaunched), IO.yieldFor(relaunch.Delay.Span.ToTimeSpan()).Bind(_ => Run(context, relaunch.Job)))).As())
            .Map(unit), context.Retain).Map(static _ => unit);

    private static IO<Unit> Launched(QueueContext context, string member, IO<Unit> run) =>
        DisposalOps.Within(static () => unit, _ => Reported(context, member, run), _ => Reported(context, member, Quit.OnUiThread()));

    private static IO<Unit> Run(QueueContext context, string path) =>
        Read(path).Bind(job => Documents.WithDocument(new DocumentSource.Opened(job.Copy), doc => Ran(context, doc, path, job)));

    private static IO<Unit> Ran(QueueContext context, RhinoDoc doc, string path, QueueJob job) =>
        from prior in QueueProgress.Read(context.Record)
        from queue in JobMapper.FromJob(doc, job).OnUiThread()
        from ran in Queues.Run(context, doc, queue, Some(path), prior.Filter(held => held.Job == Some(path) && !held.Succeeded))
        select ran;

    private static IO<QueueJob> Read(string path) =>
        from qualified in IO.lift(() => Exchange.QualifiedPath(path))
        from found in JsonFiles.Read(qualified, JobContext.Default.QueueJob, static job => Fin.Succ(job), "render queue job")
        from job in IO.lift(found.ToFin(new NotFound("render queue job", qualified)))
        select job;
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class QueueJobCommand(IPlugInSink sink, Guid id, string englishName) : HostCommand(sink, id, englishName, None) {
    protected override IO<Unit> Run(RhinoDoc doc, RunMode mode, CallbackSite site) =>
        QueueContext.Current.Of(site.Sink).Bind(Dispatch.Job).Map(unit);
}

public sealed class QueueFarmJobCommand(IPlugInSink sink, Guid id, string englishName) : HostCommand(sink, id, englishName, None) {
    protected override IO<Unit> Run(RhinoDoc doc, RunMode mode, CallbackSite site) =>
        mode is RunMode.Scripted
            ? from context in QueueContext.Current.Of(site.Sink)
              from session in IO.lift(() => PlugInSession.Of(site.Sink))
              from ran in Dispatch.FarmJob(context, doc, session.Close)
              select ran
            : IO.fail<Unit>(Error.New("Farm job command runs only scripted in Rhino farm process"));
}

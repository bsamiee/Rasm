using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using Darkroom.Output;
using NodaTime.Text;
using Rasm.Imaging.Pixels;
using Rasm.Rhino;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rasm.UI;
using Rhino;
using Rhino.Commands;
using Rhino.PlugIns;
using Rhino.Render;

namespace Darkroom.Queue;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class PackageUse {
    public static readonly PackageUse JobFile = new(quits: false);
    public static readonly PackageUse Handoff = new(quits: true);

    public bool Quits { get; }
}

[SmartEnum<string>]
public sealed partial class WorkerPlatform : ICaptioned {
    public static readonly WorkerPlatform MacOs = new("darwin", OSPlatform.OSX, static script => ["-runscript", script]);
    public static readonly WorkerPlatform Windows = new("windows", OSPlatform.Windows, static script => [
        $"""
        /runscript="{script}"
        """,
    ]);

    public OSPlatform Platform { get; }

    public static WorkerPlatform Current => OperatingSystem.IsWindows() ? Windows : MacOs;

    [UseDelegateFromConstructor]
    public partial string[] Arguments(string script);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct JobPriority : IPresented<JobPriority, int> {
    public static JobPriority MinValue { get; } = new(1);
    public static JobPriority MaxValue { get; } = new(100);
    public static Presentation<int> Presentation { get; } = new();

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

public sealed record FlamencoSettings(HttpServer Manager, WorkerPlatform Platform, JobPriority Priority) : IStateRecord<FlamencoSettings, FlamencoParameter> {
    public static FlamencoSettings Default { get; } = new(HttpServer.Create("http://localhost:8080"), WorkerPlatform.Current, JobPriority.Create(50));

    public Option<string> Scripts { get; init; }
    public Option<Guid> WorkerTag { get; init; }
}

[SmartEnum<string>]
public sealed partial class FlamencoParameter : IStateParameter<FlamencoSettings> {
    public static readonly FlamencoParameter Manager = new("manager", new StateParameter<FlamencoSettings>.Keyed<HttpServer, string>(
            Lens<FlamencoSettings, HttpServer>.New(static farm => farm.Manager, static manager => farm => farm with { Manager = manager })),
        new("Manager", "Flamenco Manager address"));
    public static readonly FlamencoParameter Scripts = new("scripts", new StateParameter<FlamencoSettings>.OptionalRaw<string>(
            Lens<FlamencoSettings, Option<string>>.New(static farm => farm.Scripts, static scripts => farm => farm with { Scripts = scripts })),
        new("Scripts folder", "Manager scripts folder the job compiler installs into"));
    public static readonly FlamencoParameter Platform = new("platform", new StateParameter<FlamencoSettings>.Choice<WorkerPlatform>(
            Lens<FlamencoSettings, WorkerPlatform>.New(static farm => farm.Platform, static platform => farm => farm with { Platform = platform })),
        new("Worker platform", "Rhino worker operating system, mixed farms need a matching worker tag"));
    public static readonly FlamencoParameter Priority = new("priority", StateParameter.Bounded(
            Lens<FlamencoSettings, JobPriority>.New(static farm => farm.Priority, static priority => farm => farm with { Priority = priority })),
        new("Priority", "Priority of submitted Flamenco jobs"));
    public static readonly FlamencoParameter WorkerTag = new("worker-tag", new StateParameter<FlamencoSettings>.OptionalRaw<Guid>(
            Lens<FlamencoSettings, Option<Guid>>.New(static farm => farm.WorkerTag, static tag => farm => farm with { WorkerTag = tag })),
        new("Worker tag", "Tag selecting workers of the chosen platform, empty for any Rhino worker"));

    public StateParameter<FlamencoSettings> Kind { get; }

    public ParameterText Text { get; }
}

[Union]
public abstract partial record QueueTarget {
    public static Fin<Destination> Jobs(Destination destination) =>
        NamePart.Validated(nameof(Jobs).ToLowerInvariant()).Map(jobs => destination with {
            Folder = destination.Folder.Switch<NamePart, OutputFolder>(
                jobs,
                absolute: static (part, absolute) => new OutputFolder.Absolute(Path.Join(absolute.Folder, part)),
                besideDocument: static (part, beside) => new OutputFolder.BesideDocument(beside.Segments.Add(part))),
        });

    public sealed record Local : QueueTarget;
    public sealed record Packaged(Destination Package, PackageUse Use) : QueueTarget;
    public sealed record Flamenco(Destination Package, FlamencoSettings Farm) : QueueTarget;
}

public sealed record QueueContext(OutputPath Record, IO<QueueSettings> Settings, IPlugInViews Views, Atom<QueueEstimate> Estimate, Func<QueueEvent, IO<Unit>> Notify, string JobCommand, string FarmScript, Action<IDisposable> Retain) {
    public static PlugInSlot<QueueContext> Current { get; } = new();
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

internal sealed record QueueJob(int RhinoVersion, string Copy, QueueJob.Entry[] Entries) {
    private const string TargetOwner = nameof(QueueOutput<>.Target);
    private const string DestinationOwner = nameof(QueueEntry<>.Destination);

    internal sealed record Entry(
        ValueSet Values, float? Clay, RenderSettings.RenderingSources Source, string? SpecificViewport, string? NamedView, string? Snapshot,
        int First, int Last, int Width, int Height, double Overscan, Region? Region, SunWindow? Sun, ValueSet Destination, Output[] Outputs);

    internal sealed record Region(double Left, double Top, double Right, double Bottom);

    internal sealed record Output(int Frame, ValueSet Target, string[] Scope, int? Version, string[] Parts);

    // --- [OUTBOUND]
    internal static QueueJob Of(RenderBatch batch, string copy) =>
        new(RhinoApp.ExeVersion, copy, [.. batch.Entries.Map(static entry => Entried(entry, entry.Outputs.Map(output => new Output(
            output.Name.Number.IfNone(entry.Frames.First), ValueSet.Fields(TargetOwner, output.Target), [.. output.Name.Scope.Map(static part => (string)part)],
            output.Name.Version.Taken.Map(static version => (int)version).ToNullable(), [.. output.Name.Parts.Map(static part => (string)part)]))))]);

    internal static Entry Identity<TPath>(QueueEntry<TPath> entry) =>
        Entried(entry, entry.Outputs.Map(static output => (output.Target, output.Name.Scope, output.Name.Parts)).Distinct().Map(output => new Output(
            entry.Frames.First, ValueSet.Fields(TargetOwner, output.Target), [.. output.Scope.Map(static part => (string)part)], null, [.. output.Parts.Map(static part => (string)part)])));

    private static Entry Entried<TPath>(QueueEntry<TPath> entry, Seq<Output> outputs) =>
        new(entry.Override.Values, entry.Override.Clay.Map(static clay => (float)clay).ToNullable(),
            entry.Source.RenderSource, entry.Source.SpecificViewport.ValueUnsafe(), entry.Source.NamedView.ValueUnsafe(), entry.Source.Snapshot.ValueUnsafe(),
            entry.Frames.First, entry.Frames.Last, entry.Extent.Width, entry.Extent.Height, entry.Overscan,
            entry.Region.Map(static region => new Region(region.Left, region.Top, region.Right, region.Bottom)).ValueUnsafe(),
            entry.Sun.ValueUnsafe(), ValueSet.Fields(DestinationOwner, entry.Destination), [.. outputs]);

    // --- [INBOUND]
    internal IO<RenderBatch> Batch(RhinoDoc doc) =>
        Callbacks.Each(toSeq(Entries).Map((entry, index) => Planned(doc, entry).MapFail(error =>
                Error.New(string.Create(CultureInfo.InvariantCulture, $"render queue job entry {index} is invalid"), error))))
            .Bind(static entries => IO.lift(Callbacks.Unique(entries, static entry => entry.Key, nameof(QueueJob))))
            .Bind(entries => RenderBatch.Placed(doc, entries, overwrite: true));

    private static IO<QueueEntry<Unit>> Planned(RhinoDoc doc, Entry entry) =>
        from stored in entry.Destination.Recalled(DestinationOwner, Destination.Default)
        from columns in IO.lift((
                Destinations.Resolved(stored, Conversions.Present(doc.Path)),
                Optional(entry.Clay).Traverse(static clay => Albedo.Validated(clay)).As(),
                FrameSpan.Of(entry.First, entry.Last),
                PixelExtent.Validated(entry.Width, entry.Height),
                Overscan.Validated(entry.Overscan),
                Optional(entry.Region).Traverse(static region => RenderRegion.Of(region.Left, region.Top, region.Right, region.Bottom)).As())
            .Apply(static (destination, clay, frames, extent, overscan, region) => (Destination: destination, Clay: clay, Frames: frames, Extent: extent, Overscan: overscan, Region: region))
            .As())
        from outputs in Callbacks.Each(toSeq(entry.Outputs).Map(output => Planned(columns.Frames, output)))
        select new QueueEntry<Unit>(
            new RenderOverride(entry.Values, columns.Clay),
            new RenderSourceState(entry.Source, Optional(entry.SpecificViewport), Optional(entry.NamedView), Optional(entry.Snapshot)),
            columns.Frames, columns.Extent, columns.Overscan, columns.Region, Optional(entry.Sun), columns.Destination, outputs);

    private static IO<QueueOutput<Unit>> Planned(FrameSpan frames, Output output) =>
        from target in output.Target.Recalled(TargetOwner, OutputTarget.Default)
        from placed in IO.lift((
                SequenceNumber.Validated(output.Frame).Bind(frame => frames.Holds(frame)
                    ? Fin.Succ(frame)
                    : Error.New(string.Create(CultureInfo.InvariantCulture, $"job frame {frame} lies outside entry range {frames.First} to {frames.Last}"))),
                toSeq(output.Scope).Traverse(NamePart.Validated).As(),
                toSeq(output.Parts).Traverse(NamePart.Validated).As(),
                Optional(output.Version).Traverse(static version => OutputVersion.Validated(version)).As(),
                FileExtension.Validated(target.FileFormat.Extension))
            .Apply((frame, scope, parts, version, extension) => new QueueOutput<Unit>(target, new OutputName(
                scope, version.Match<OutputVersioning>(Some: static held => new OutputVersioning.Pinned(held), None: static () => new OutputVersioning.Unversioned()),
                parts, frames.Named(frame), extension), unit))
            .As())
        select placed;

    internal static IO<QueueJob> Read(string path) =>
        from found in JsonFiles.Read(path, JobContext.Default.QueueJob, static job => Fin.Succ(job))
        from job in IO.lift(found.ToFin(new FileRefused(path, FileReason.Missing)))
        select job;
}

[JsonSerializable(typeof(QueueJob))]
[JsonSerializable(typeof(QueueJob.Entry))]
[JsonSerializable(typeof(JsonNode))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Strict, WriteIndented = true, UseStringEnumConverter = true)]
internal sealed partial class JobContext : JsonSerializerContext;

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
internal sealed partial class PackageFile {
    public static readonly PackageFile Copy = new(".3dm", None, static (doc, _, path, _) =>
        Exchange.WriteFile(doc, path, static options => (options.IncludeBitmapTable, options.IncludeRenderMeshes) = (true, true)));
    public static readonly PackageFile Job = new(".json", None, static (_, batch, path, copy) =>
        JsonFiles.Write(path, QueueJob.Of(batch, copy), JobContext.Default.QueueJob));
    public static readonly PackageFile Schema = new(".json", Some(nameof(Schema)), static (_, _, path, _) =>
        JsonFiles.Write(path, JobContext.Default.QueueJob.GetJsonSchemaAsNode(), JobContext.Default.JsonNode));

    public string Extension { get; }

    private readonly Option<string> part;

    [UseDelegateFromConstructor]
    public partial IO<Unit> Write(RhinoDoc doc, RenderBatch batch, OutputPath path, OutputPath copy);

    public Fin<OutputName> Name(NamePart stamp) =>
        (part.Traverse(static held => NamePart.Validated(held.ToLowerInvariant())).As(), FileExtension.Validated(Extension))
            .Apply((suffix, extension) => new OutputName(Seq(stamp), new OutputVersioning.Unversioned(), suffix.ToSeq(), None, extension))
            .As();
}

// --- [SERVICES] ------------------------------------------------------------------------
internal static partial class SafeNativeMethods {
    // --- [ARGUMENTS]
    [LibraryImport("/usr/lib/libSystem.B.dylib")]
    [SupportedOSPlatform("macos")]
    private static partial nint _NSGetArgv();

    [LibraryImport("/usr/lib/libSystem.B.dylib")]
    [SupportedOSPlatform("macos")]
    private static partial nint _NSGetArgc();

    [SupportedOSPlatform("macos")]
    internal static string[] Arguments() =>
        [.. Enumerable.Range(0, Marshal.ReadInt32(_NSGetArgc())).Select(static index => Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(Marshal.ReadIntPtr(_NSGetArgv()), index * nint.Size))!)];
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Dispatch {
    private const string CompilerFile = "darkroom_render.js";

    public static Action<ILoggingBuilder> Logging { get; } = static logging => logging.AddConsole(static options => options.LogToStandardErrorThreshold = LogLevel.Error);

    // --- [SEND]
    public static IO<Unit> Send(QueueContext context, RhinoDoc doc, Func<QueuePlan, IO<QueueTarget>> target, bool resume, Option<Func<Preflight, IO<Unit>>> report = default) =>
        from plan in QueuePlan.Read(doc)
        from chosen in target(plan)
        from resumed in resume ? QueueProgress.Read(context.Record).Map(static prior => prior.Filter(static record => !record.Succeeded)) : IO.pure(Option<QueueProgress>.None)
        from stamp in IO.lift(() => context.Views.Clock.ToZonedClock().GetCurrentLocalDateTime())
        from check in Preflight.Check(context, doc, plan, chosen).OnUiThread()
        from shown in report.Traverse(publish => publish(check).OnUiThread()).As()
        from verdict in IO.lift(check.Verdict)
        from sent in chosen.Switch(
            (Context: context, Doc: doc, Plan: plan, Stamp: stamp, Resumed: resumed),
            local: static (state, _) => DisposalOps.Fork(Reported(state.Context, state.Plan.Resolved(state.Doc, state.Resumed).OnUiThread()
                .Bind(batch => QueueRuns.Run(state.Context, state.Doc, batch, None, state.Resumed))), state.Context.Retain).Map(static _ => unit),
            packaged: static (state, packaged) => packaged.Use.Switch(
                (state.Context, state.Doc, state.Plan, state.Stamp, state.Resumed, packaged.Package),
                jobFile: static held => Packed(held.Doc, held.Plan, held.Resumed, held.Package, held.Stamp).OnUiThread().Map(static _ => unit),
                handoff: static held => Handed(held.Context, held.Doc, held.Plan, held.Stamp, held.Resumed, held.Package)),
            flamenco: static (state, farm) =>
                from paths in Packed(state.Doc, state.Plan, state.Resumed, farm.Package, state.Stamp).OnUiThread()
                from submitted in Submitted(farm.Farm, paths[PackageFile.Copy], farm.Farm.Platform.Arguments(state.Context.FarmScript))
                from noticed in state.Context.Notify(new QueueEvent.Submitted(submitted))
                select unit)
        select sent;

    private static IO<T> Reported<T>(QueueContext context, IO<T> run, [CallerMemberName] string member = "") =>
        DisposalOps.OnFailure(run, error => IO.lift(() => context.Views.Report(error, typeof(Dispatch), member)));

    // --- [FLAMENCO]
    public static IO<Unit> InstallCompiler(FlamencoSettings farm) =>
        from folder in IO.lift(farm.Scripts.ToFin(new Refused(nameof(FlamencoSettings.Scripts), Refused.Absent)))
        from path in IO.lift(OutputPath.Of(Path.Join(folder, CompilerFile)))
        from written in DisposalOps.Found(() => typeof(Dispatch).Assembly.GetManifestResourceStream(CompilerFile), nameof(System.Reflection.Assembly.GetManifestResourceStream),
            source => FilePublication.Write(path, overwrite: true, target => IO.liftAsync(async environment => {
                await source.CopyToAsync(target, environment.Token).ConfigureAwait(false);
                return unit;
            })))
        select written;

    public static IO<bool> FarmStartup(string farmScript) =>
        IO.lift(() => (OperatingSystem.IsMacOS() ? SafeNativeMethods.Arguments() : Environment.GetCommandLineArgs()).AsSpan().EndsWith(WorkerPlatform.Current.Arguments(farmScript)));

    private static IO<Uri> Submitted(FlamencoSettings farm, string copy, string[] arguments) =>
        DisposalOps.Within(static () => new HttpClient(), client =>
            from advertised in IO.liftAsync(environment => client.GetFromJsonAsync(new Uri(farm.Manager.Address, "/api/v3/jobs/types"), FlamencoContext.Default.FlamencoTypes, environment.Token))
            from types in IO.lift(Refused.UnlessFound(advertised, nameof(HttpClientJsonExtensions.GetFromJsonAsync)))
            let type = Path.GetFileNameWithoutExtension(CompilerFile).Replace('_', '-')
            from compiled in IO.lift(toSeq(types.JobTypes).Find(job => string.Equals(job.Name, type, StringComparison.Ordinal)).ToFin(new NotFound("Flamenco job type", type)))
            from submitted in IO.liftAsync(async environment => {
                using JsonContent content = JsonContent.Create(new FlamencoJob(
                    Path.GetFileNameWithoutExtension(copy), type, compiled.Etag, farm.Priority, WorkerPlatform.Current.Key,
                    new FlamencoJob.Parameters(copy, arguments), farm.WorkerTag.ToNullable()), FlamencoContext.Default.FlamencoJob);
                using HttpResponseMessage response = await client.PostAsync(new Uri(farm.Manager.Address, "/api/v3/jobs"), content, environment.Token).ConfigureAwait(false);
                return response.StatusCode is HttpStatusCode.OK
                    ? Refused.UnlessFound(await response.Content.ReadFromJsonAsync(FlamencoContext.Default.FlamencoReceipt, environment.Token).ConfigureAwait(false), nameof(HttpContentJsonExtensions.ReadFromJsonAsync))
                        .Map(receipt => new Uri(farm.Manager.Address, $"/jobs/{receipt.Id:D}"))
                    : Error.New($"Flamenco refused render job with {response.StatusCode}");
            })
            from received in IO.lift(submitted)
            select received)
            .Catch(static error => error.HasException<HttpRequestException>(), error => IO.fail<Uri>(Error.New($"render job post to {farm.Manager} failed", error)));

    // --- [PACKAGE]
    private static IO<HashMap<PackageFile, OutputPath>> Packed(RhinoDoc doc, QueuePlan plan, Option<QueueProgress> resumed, Destination package, LocalDateTime stamp) =>
        from batch in plan.Resolved(doc, resumed)
        from date in IO.lift(NamePart.Validated(LocalDateTimePattern.ExtendedIso.Format(stamp.With(TimeAdjusters.TruncateToSecond))))
        from names in IO.lift(toSeq(PackageFile.Items).Traverse(file => file.Name(date)).As())
        from placed in Destinations.Resolve(doc, package, names)
        let paths = toHashMap(toSeq(PackageFile.Items).Zip(placed, static (file, row) => (file, row.Path)))
        from written in toSeq(PackageFile.Items).TraverseM(file => file.Write(doc, batch, paths[file], paths[PackageFile.Copy])).As()
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
                    "-g", "-a", RhinoApp.GetInstallDirectory().FullName,
                    "--env", $"{variable}={job}", "--args", "-runscript=_NoEcho", $"_{command}",
                },
                UseShellExecute = false,
            }
            : Refused.UnlessFound(Environment.ProcessPath, nameof(Environment.ProcessPath)).Map(path => new ProcessStartInfo(path) {
                ArgumentList = { "/nosplash", $"/runscript=_NoEcho _{command}" },
                Environment = { [variable] = job },
                UseShellExecute = false,
            });

    private static IO<Unit> Quit =>
        from flushed in IO.lift(PlugIn.FlushSettingsSavedQueue)
        from exited in IO.lift(static () => RhinoApp.Exit(allowCancel: false))
        select unit;

    // --- [JOBS]
    private static string JobVariable(QueueContext context) => $"RENDER_JOB_{context.Views.PlugIn.Id:N}";

    public static IO<ForkIO<Unit>> Job(QueueContext context) =>
        JobVariable(context) switch {
            var variable => IO.lift(() => Conversions.Present(Environment.GetEnvironmentVariable(variable)).ToFin(new Refused(variable, Refused.Absent)))
                .Bind(path => DisposalOps.Fork(Launched(context, Run(context, path)), context.Retain)),
        };

    public static IO<Unit> FarmJob(QueueContext context, RhinoDoc doc, IO<Unit> close) =>
        (from code in Farmed(context, doc).Map(static _ => 0).Catch(error => IO.lift(() => context.Views.Report(error, typeof(Dispatch), nameof(FarmJob))).Map(_ => QueueRuns.Stopping(error) ? 2 : 1)).As()
         from exited in close.OnUiThread().Finally(IO.lift(() => Environment.Exit(code)).OnUiThread())
         select exited).Uninterruptible().Fork().Map(unit);

    private static IO<Unit> Farmed(QueueContext context, RhinoDoc doc) =>
        from path in IO.lift(() => Conversions.Present(doc.Path).ToFin(new Refused(nameof(RhinoDoc.Path), Refused.Absent)).Bind(saved => InputPath.Of(Path.ChangeExtension(saved, PackageFile.Job.Extension)))).OnUiThread()
        from job in QueueJob.Read(path)
        from record in IO.lift(OutputPath.Of(Path.ChangeExtension(path, ".progress.json")))
        from ran in Ran(context with { Record = record, Settings = context.Settings.Map(static settings => settings with { ShutDownAfterSuccess = false }) }, doc, path, job)
        select ran;

    public static IO<Unit> Relaunched(QueueContext context) =>
        DisposalOps.Fork(Reported(context, QueueRuns.Resumable(context))
            .Bind(found => found.Traverse(relaunch =>
                Launched(context, IO.yieldFor(relaunch.Delay.Span.ToTimeSpan()).Bind(_ => Run(context, relaunch.Job)))).As())
            .Map(unit), context.Retain).Map(static _ => unit);

    private static IO<Unit> Launched(QueueContext context, IO<Unit> run, [CallerMemberName] string member = "") =>
        Reported(context, run, member).Finally(Reported(context, Quit.OnUiThread(), member));

    private static IO<Unit> Run(QueueContext context, string path) =>
        from input in IO.lift(InputPath.Of(path))
        from job in QueueJob.Read(input)
        from opened in IO.lift(InputPath.Of(job.Copy))
        from ran in Documents.WithDocument(new DocumentSource.Opened(opened), doc => Ran(context, doc, input, job))
        select ran;

    private static IO<Unit> Ran(QueueContext context, RhinoDoc doc, string path, QueueJob job) =>
        from prior in QueueProgress.Read(context.Record)
        from batch in job.Batch(doc).OnUiThread()
        from ran in QueueRuns.Run(context, doc, batch, Some(path), prior.Filter(held => held.Job == Some(path) && !held.Succeeded))
        select ran;

    // --- [REGISTRATION]
    public static Func<IPlugInViews, Action<IDisposable>, IO<Unit>> Register { get; } = static (views, retain) =>
        DisposalOps.Acquire(acquired =>
            from record in IO.lift(() => OutputPath.Of(Path.Join(Directory.CreateDirectory(views.PlugIn.SettingsDirectory).FullName, $"{nameof(RenderQueue)}.json")))
            from commands in IO.lift(() => toSeq(views.PlugIn.GetCommands()))
            from named in IO.lift((Named<QueueJobCommand>(commands), Named<QueueFarmJobCommand>(commands)).Apply(static (job, farm) => (Job: job, Farm: $"_-{farm}")).As())
            let settings = QueueSettings.Read(views)
            from sender in NoticeSender.Acquire(settings.Map(static held => held.Notices), views, acquired)
            from estimate in atomIO(QueueEstimate.Idle)
            let context = new QueueContext(record, settings, views, estimate, sender.Notify, named.Job, named.Farm, acquired)
            from published in QueueContext.Current.Publish(views, context)
            from farm in FarmStartup(named.Farm)
            from relaunched in unless(farm, Relaunched(context)).As()
            select unit, retain);

    private static Fin<string> Named<TCommand>(Seq<Command> commands) where TCommand : Command =>
        commands.Find(static command => command is TCommand).Map(static command => command.EnglishName).ToFin(new NotFound("command", typeof(TCommand).Name));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class QueueJobCommand(IPlugInSink sink) : HostCommand(sink, new Guid(0x4a60fdca, 0x6daf, 0x4fef, 0x8d, 0x5e, 0xfd, 0xc7, 0x71, 0x63, 0x9f, 0xee), "DarkroomRenderQueue", None) {
    protected override IO<Unit> Run(RhinoDoc doc, RunMode mode, CallbackSite site) =>
        QueueContext.Current.Of(site.Sink).Bind(Dispatch.Job).Map(unit);
}

public sealed class QueueFarmJobCommand(IPlugInSink sink) : HostCommand(sink, new Guid(0x3857668a, 0x1b07, 0x4ae9, 0x97, 0x13, 0x63, 0x33, 0xb3, 0xca, 0x46, 0xee), "DarkroomRenderFarm", None) {
    protected override IO<Unit> Run(RhinoDoc doc, RunMode mode, CallbackSite site) =>
        mode is RunMode.Scripted
            ? from context in QueueContext.Current.Of(site.Sink)
              from session in IO.lift(() => PlugInSession.Of(site.Sink))
              from ran in Dispatch.FarmJob(context, doc, session.Close)
              select ran
            : IO.fail<Unit>(new Refused(nameof(RunMode.Scripted), Refused.Declined));
}

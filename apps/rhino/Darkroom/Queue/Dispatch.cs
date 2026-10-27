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
using Darkroom.Chain;
using Darkroom.Output;
using NodaTime.Text;
using Rasm.RhinoCommon;
using Rasm.RhinoCommon.Commands;
using Rasm.RhinoCommon.Document.Files;
using Rasm.RhinoCommon.PlugIns;
using Rasm.RhinoCommon.Render.Settings;
using Rasm.RhinoCommon.UI;
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
public sealed partial class WorkerPlatform {
    public static readonly WorkerPlatform MacOs = new("darwin", OSPlatform.OSX, static script => ["-runscript", script]);
    public static readonly WorkerPlatform Windows = new("windows", OSPlatform.Windows, static script => [
        $"""
        /runscript="{script}"
        """,
    ]);

    public OSPlatform Platform { get; }

    public static WorkerPlatform Current() => OperatingSystem.IsWindows() ? Windows : MacOs;

    [UseDelegateFromConstructor]
    public partial string[] Arguments(string script);
}

public sealed record FlamencoSettings(Uri Manager, WorkerPlatform Platform, int Priority) {
    public static FlamencoSettings Default { get; } = new(new Uri("http://localhost:8080"), WorkerPlatform.Current(), Priority: 50);

    public string? Scripts { get; init; }
    public Guid? WorkerTag { get; init; }
}

[Union]
public abstract partial record QueueTarget {
    public static OutputFolder Jobs(OutputFolder folder) =>
        folder.Switch<NamePart, OutputFolder>(
            NamePart.Create("jobs"),
            absolute: static (part, absolute) => new OutputFolder.Absolute(Path.Join(absolute.Folder, part)),
            besideDocument: static (part, beside) => new OutputFolder.BesideDocument(beside.Segments.Add(part)));

    public sealed record Local : QueueTarget;
    public sealed record Packaged(OutputFolder Folder, Stem Stem, PackageUse Use) : QueueTarget;
    public sealed record Flamenco(OutputFolder Folder, Stem Stem, FlamencoSettings Farm) : QueueTarget;
}

public sealed record QueueContext(OutputPath Record, IO<QueueSettings> Settings, IPlugInViews Views, Atom<QueueEstimate> Estimate, Func<QueueEvent, IO<Unit>> Notify, string JobCommand, string FarmScript) {
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
    internal sealed record Entry(QueuedEntry Queued, string Folder, NamePart Stem, bool Overwrite, Output[] Outputs);

    internal sealed record Output(SequenceNumber Frame, OutputTarget Target, NamePart[] Scope, OutputVersion? Version, NamePart[] Parts);

    // --- [OUTBOUND]
    internal static QueueJob Of(RenderBatch batch, string copy) =>
        new(RhinoApp.ExeVersion, copy, [.. batch.Entries.Map(static entry => Entried(entry, entry.Outputs.Map(static output => new Output(
            output.Name.Number.IfNone(FrameSpan.First), output.Target, [.. output.Name.Scope], output.Name.Version.Taken.ValueUnsafe(), [.. output.Name.Parts]))))]);

    internal static Entry Identity<TPath>(QueueEntry<TPath> entry) =>
        Entried(entry, entry.Outputs.Map(static output => (output.Target, output.Name.Scope, output.Name.Parts)).Distinct().Map(static output => new Output(
            FrameSpan.First, output.Target, [.. output.Scope], Version: null, [.. output.Parts])));

    private static Entry Entried<TPath>(QueueEntry<TPath> entry, Seq<Output> outputs) => new(entry.Queued, entry.Folder, entry.Stem, entry.Overwrite, [.. outputs]);

    // --- [INBOUND]
    internal IO<RenderBatch> Batch(RhinoDoc doc) =>
        from settings in RenderSettingsSources.Read(new RenderSettingsSource.Live(doc), static scope => IO.lift(scope.Settings.Duplicate))
        from entries in IO.lift(toSeq(Entries).Map(static (entry, index) => (Entry: entry, Index: index))
            .Traverse(held => Planned(held.Entry, settings).MapFail(error => new DarkroomFault(string.Create(CultureInfo.InvariantCulture, $"render queue job entry {held.Index}"), DarkroomReason.Invalid, error)))
            .As()
            .Bind(static entries => Conversions.Unique(entries, static entry => entry.Key)))
        from batch in RenderBatch.Placed(doc, entries, overwrite: true)
        select batch;

    private static Fin<QueueEntry<Unit>> Planned(Entry entry, RenderSettings settings) =>
        FrameSpan.Of(entry.Queued.Sun) switch {
            var frames => toSeq(entry.Outputs).Traverse(output =>
                    from inside in guard<Error>((int)output.Frame < frames.Numbers.Count, new DarkroomFault(string.Create(CultureInfo.InvariantCulture, $"job frame {output.Frame} of {frames.Numbers.Count}"), DarkroomReason.Beyond)).ToFin()
                    select new QueueOutput<Unit>(output.Target, new OutputName(
                        toSeq(output.Scope),
                        Optional(output.Version).Match<OutputVersioning>(Some: static held => new OutputVersioning.Pinned(held), None: static () => new OutputVersioning.Unversioned()),
                        toSeq(output.Parts), frames.Named(output.Frame), output.Target.FileFormat.Extension), unit))
                .As()
                .Map(outputs => new QueueEntry<Unit>(entry.Queued, new RenderOverride(settings, Optional(entry.Queued.Clay)), entry.Folder, entry.Stem, entry.Overwrite, outputs)),
        };

    internal static IO<QueueJob> Read(string path) =>
        from found in JsonFiles.Read(path, JobContext.Default.QueueJob, static job => Fin.Succ(job))
        from job in IO.lift(found.ToFin(new RhinoFault(path, RhinoReason.Missing)))
        select job;
}

[JsonSerializable(typeof(QueueJob))]
[JsonSerializable(typeof(QueueJob.Entry))]
[JsonSerializable(typeof(JsonNode))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Strict, WriteIndented = true, UseStringEnumConverter = true)]
internal sealed partial class JobContext : JsonSerializerContext;

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
internal sealed partial class PackageFile {
    public static readonly PackageFile Copy = new(FileExtension.Create(".3dm"), None, static (doc, _, path, _) =>
        Exchange.WriteFile(doc, path, static options => (options.IncludeBitmapTable, options.IncludeRenderMeshes) = (true, true)));
    public static readonly PackageFile Job = new(FileExtension.Create(".json"), None, static (_, batch, path, copy) =>
        JsonFiles.Write(path, QueueJob.Of(batch, copy), JobContext.Default.QueueJob));
    public static readonly PackageFile Schema = new(FileExtension.Create(".json"), Some(NamePart.Create(nameof(Schema))), static (_, _, path, _) =>
        JsonFiles.Write(path, JobContext.Default.QueueJob.GetJsonSchemaAsNode(), JobContext.Default.JsonNode));

    public FileExtension Extension { get; }

    private readonly Option<NamePart> part;

    [UseDelegateFromConstructor]
    public partial IO<Unit> Write(RhinoDoc doc, RenderBatch batch, OutputPath path, OutputPath copy);

    public OutputName Name(NamePart stamp) => new(Seq(stamp), new OutputVersioning.Unversioned(), part.ToSeq(), None, Extension);
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
            local: static (state, _) => state.Context.Views.Session.Held(Reported(state.Context, state.Plan.Resolved(state.Doc, state.Resumed).OnUiThread()
                .Bind(batch => QueueRuns.Run(state.Context, state.Doc, batch, None, state.Resumed))).Fork()).Map(static _ => unit),
            packaged: static (state, packaged) => packaged.Use.Switch(
                (state.Context, state.Doc, state.Plan, state.Stamp, state.Resumed, packaged.Folder, packaged.Stem),
                jobFile: static held => Packed(held.Doc, held.Plan, held.Resumed, held.Folder, held.Stem, held.Stamp).OnUiThread().Map(static _ => unit),
                handoff: static held => Handed(held.Context, held.Doc, held.Plan, held.Stamp, held.Resumed, held.Folder, held.Stem)),
            flamenco: static (state, farm) =>
                from paths in Packed(state.Doc, state.Plan, state.Resumed, farm.Folder, farm.Stem, state.Stamp).OnUiThread()
                from submitted in Submitted(farm.Farm, paths[PackageFile.Copy], farm.Farm.Platform.Arguments(state.Context.FarmScript))
                from noticed in state.Context.Notify(new QueueEvent.Submitted(submitted))
                select unit)
        select sent;

    private static IO<T> Reported<T>(QueueContext context, IO<T> run, [CallerMemberName] string member = "") =>
        run.Catch(error => IO.lift(() => PlugInLog.Of(context.Views).Failed(error, typeof(Dispatch), member)) >> IO.fail<T>(error)).As();

    // --- [FLAMENCO]
    public static IO<Unit> InstallCompiler(FlamencoSettings farm) =>
        (from folder in IO.lift(Conversions.Found(farm.Scripts))
         from path in IO.lift(OutputPath.Of(Path.Join(folder, CompilerFile)))
         from source in Products.Made(typeof(Dispatch).Assembly.GetManifestResourceStream(CompilerFile))
         from written in FilePublication.Write(path, overwrite: true, target => IO.liftAsync(async environment => {
             await source.CopyToAsync(target, environment.Token).ConfigureAwait(false);
             return unit;
         }))
         select written).Bracket();

    public static IO<bool> FarmStartup(string farmScript) =>
        IO.lift(() => (OperatingSystem.IsMacOS() ? SafeNativeMethods.Arguments() : Environment.GetCommandLineArgs()).AsSpan().EndsWith(WorkerPlatform.Current().Arguments(farmScript)));

    private static IO<Uri> Submitted(FlamencoSettings farm, string copy, string[] arguments) =>
        (from client in use(static () => new HttpClient())
         from advertised in IO.liftAsync(environment => client.GetFromJsonAsync(new Uri(farm.Manager, "/api/v3/jobs/types"), FlamencoContext.Default.FlamencoTypes, environment.Token))
         from types in IO.lift(Conversions.Found(advertised))
         let type = Path.GetFileNameWithoutExtension(CompilerFile).Replace('_', '-')
         from compiled in IO.lift(toSeq(types.JobTypes).Find(job => string.Equals(job.Name, type, StringComparison.Ordinal)).ToFin(new DarkroomFault($"Flamenco job type {type}", DarkroomReason.Missing)))
         from submitted in IO.liftAsync(async environment => {
             using JsonContent content = JsonContent.Create(new FlamencoJob(
                 Path.GetFileNameWithoutExtension(copy), type, compiled.Etag, farm.Priority, WorkerPlatform.Current().Key,
                 new FlamencoJob.Parameters(copy, arguments), farm.WorkerTag), FlamencoContext.Default.FlamencoJob);
             using HttpResponseMessage response = await client.PostAsync(new Uri(farm.Manager, "/api/v3/jobs"), content, environment.Token).ConfigureAwait(false);
             return response.StatusCode is HttpStatusCode.OK
                 ? Conversions.Found(await response.Content.ReadFromJsonAsync(FlamencoContext.Default.FlamencoReceipt, environment.Token).ConfigureAwait(false))
                     .Map(receipt => new Uri(farm.Manager, $"/jobs/{receipt.Id:D}"))
                 : new DarkroomFault($"Flamenco answering {response.StatusCode}", DarkroomReason.Refused);
         })
         from received in IO.lift(submitted)
         select received).Bracket()
            .Catch(static error => error.HasException<HttpRequestException>(), error => IO.fail<Uri>(new DarkroomFault($"render job post to {farm.Manager}", DarkroomReason.Failed, error)));

    // --- [PACKAGE]
    private static IO<HashMap<PackageFile, OutputPath>> Packed(RhinoDoc doc, QueuePlan plan, Option<QueueProgress> resumed, OutputFolder folder, Stem stem, LocalDateTime stamp) =>
        from batch in plan.Resolved(doc, resumed)
        from date in IO.lift(NamePart.TryCreate(LocalDateTimePattern.ExtendedIso.Format(stamp.With(TimeAdjusters.TruncateToSecond)), out NamePart? part, out RhinoFault? fault) ? Fin.Succ(part) : fault)
        from placed in Destinations.Resolve(doc, folder, stem, overwrite: false, toSeq(PackageFile.Items).Map(file => file.Name(date)))
        let paths = toHashMap(toSeq(PackageFile.Items).Zip(placed, static (file, row) => (file, row.Path)))
        from written in toSeq(PackageFile.Items).TraverseM(file => file.Write(doc, batch, paths[file], paths[PackageFile.Copy])).As()
        select paths;

    // --- [HANDOFF]
    private static IO<Unit> Handed(QueueContext context, RhinoDoc doc, QueuePlan plan, LocalDateTime stamp, Option<QueueProgress> resumed, OutputFolder folder, Stem stem) =>
        (from lease in QueueProgress.Acquire(context.Record)
         from saved in IO.lift(static () => Conversions.Rows(RhinoDoc.OpenDocuments())).Bind(static open => open.TraverseM(Exchange.Save).As()).OnUiThread()
         from paths in Packed(doc, plan, resumed, folder, stem, stamp).OnUiThread()
         from moved in resumed.Traverse(record => QueueProgress.Write(
             context.Record, record with { Job = Some((string)paths[PackageFile.Job]), Ended = record.Ended | Some(RunEnd.Stopped) })).As()
         from start in IO.lift(() => Launch(paths[PackageFile.Job], context.JobCommand, JobVariable(context)))
         from relayed in Products.Made(Process.Start(start))
         from quit in Quit.OnUiThread()
         select unit).Bracket();

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
            : Conversions.Found(Environment.ProcessPath).Map(path => new ProcessStartInfo(path) {
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
            var variable => IO.lift(() => Conversions.Present(Environment.GetEnvironmentVariable(variable)).ToFin(new DarkroomFault(variable, DarkroomReason.Missing)))
                .Bind(path => context.Views.Session.Held(Launched(context, Run(context, path)).Fork())),
        };

    public static IO<Unit> FarmJob(QueueContext context, RhinoDoc doc, IO<Unit> close) =>
        (from code in Reported(context, Farmed(context, doc)).Map(static _ => 0).Catch(static error => IO.pure(QueueRuns.Stopping(error) ? 2 : 1)).As()
         from exited in close.OnUiThread().Finally(IO.lift(() => Environment.Exit(code)).OnUiThread())
         select exited).Uninterruptible().Fork().Map(unit);

    private static IO<Unit> Farmed(QueueContext context, RhinoDoc doc) =>
        from path in IO.lift(() => Conversions.Found(Conversions.Present(doc.Path)).Bind(static saved => InputPath.Of(Path.ChangeExtension(saved, PackageFile.Job.Extension)))).OnUiThread()
        from job in QueueJob.Read(path)
        from record in IO.lift(OutputPath.Of(Path.ChangeExtension(path, ".progress.json")))
        from ran in Ran(context with { Record = record, Settings = context.Settings.Map(static settings => settings with { ShutDownAfterSuccess = false }) }, doc, path, job)
        select ran;

    public static IO<Unit> Relaunched(QueueContext context) =>
        context.Views.Session.Held(Reported(context, QueueRuns.Resumable(context))
            .Bind(found => found.Traverse(relaunch =>
                Launched(context, IO.yieldFor(relaunch.Delay).Bind(_ => Run(context, relaunch.Job)))).As())
            .Map(unit)
            .Fork()).Map(static _ => unit);

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
    public static Func<IPlugInViews, IO<Unit>> Register { get; } = static views =>
        from record in IO.lift(() => OutputPath.Of(Path.Join(Directory.CreateDirectory(views.PlugIn.SettingsDirectory).FullName, $"{nameof(QueueProgress)}.json")))
        from commands in IO.lift(() => toSeq(views.PlugIn.GetCommands()))
        from named in IO.lift((Named<QueueJobCommand>(commands), Named<QueueFarmJobCommand>(commands)).Apply(static (job, farm) => (Job: job, Farm: $"_-{farm}")).As())
        let settings = QueueSettings.Read(views)
        from sender in NoticeSender.Acquire(settings.Map(static held => held.Notices), views)
        from estimate in atomIO(QueueEstimate.Idle)
        let context = new QueueContext(record, settings, views, estimate, sender.Notify, named.Job, named.Farm)
        from published in QueueContext.Current.Publish(views, context)
        from farm in FarmStartup(named.Farm)
        from relaunched in unless(farm, Relaunched(context)).As()
        select unit;

    private static Fin<string> Named<TCommand>(Seq<Command> commands) where TCommand : Command =>
        commands.Find(static command => command is TCommand).Map(static command => command.EnglishName).ToFin(new DarkroomFault($"command {typeof(TCommand).Name}", DarkroomReason.Missing));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class QueueJobCommand() : HostCommand(new Guid(0x4a60fdca, 0x6daf, 0x4fef, 0x8d, 0x5e, 0xfd, 0xc7, 0x71, 0x63, 0x9f, 0xee), "DarkroomRenderQueue", None) {
    protected override IO<Unit> Run(RhinoDoc doc, RunMode mode) =>
        from owner in IO.lift(Conversions.Cast<IPlugInLifetime>(PlugIn))
        from context in QueueContext.Current.Of(owner)
        from job in Dispatch.Job(context)
        select unit;
}

public sealed class QueueFarmJobCommand() : HostCommand(new Guid(0x3857668a, 0x1b07, 0x4ae9, 0x97, 0x13, 0x63, 0x33, 0xb3, 0xca, 0x46, 0xee), "DarkroomRenderFarm", None) {
    protected override IO<Unit> Run(RhinoDoc doc, RunMode mode) =>
        from scripted in IO.lift(RhinoFault.Unless(mode is RunMode.Scripted))
        from owner in IO.lift(Conversions.Cast<IPlugInLifetime>(PlugIn))
        from context in QueueContext.Current.Of(owner)
        from ran in Dispatch.FarmJob(context, doc, owner.Session.Close)
        select ran;
}

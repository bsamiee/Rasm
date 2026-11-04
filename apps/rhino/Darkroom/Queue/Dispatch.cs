using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Darkroom.Chain;
using Darkroom.Output;
using NodaTime.Text;
using Rasm.Rhino;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document.Files;
using Rhino;
using Rhino.Commands;
using Rhino.Input.Custom;
using Rhino.PlugIns;

namespace Darkroom.Queue;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class PackageUse {
    public static readonly PackageUse JobFile = new("JobFile", quits: false);
    public static readonly PackageUse Handoff = new("Handoff", quits: true);

    public bool Quits { get; }
}

[Union]
public abstract partial record QueueTarget {
    private static readonly NamePart JobsPart = NamePart.Create("jobs");

    public string Name => Switch(local: static _ => nameof(Local), packaged: static held => held.Use.Key, flamenco: static _ => nameof(Flamenco));
    public bool RendersHere => this is Local || Quits;
    public bool Quits => this is Packaged { Use.Quits: true };
    public Option<(OutputFolder Folder, Stem Stem)> Package => Switch<Option<(OutputFolder Folder, Stem Stem)>>(
        local: static _ => None,
        packaged: static held => Some((held.Folder, held.Stem)),
        flamenco: static held => Some((held.Folder, held.Stem)));

    public static OutputFolder Jobs(OutputFolder folder) =>
        folder.Switch<OutputFolder>(
            absolute: static absolute => new OutputFolder.Absolute(Path.Join(absolute.Folder, JobsPart)),
            besideDocument: static beside => new OutputFolder.BesideDocument(beside.Segments.Add(JobsPart)));

    public sealed record Local() : QueueTarget;
    public sealed record Packaged(OutputFolder Folder, Stem Stem, PackageUse Use) : QueueTarget;
    public sealed record Flamenco(OutputFolder Folder, Stem Stem, FlamencoSettings Farm) : QueueTarget;
}

public sealed record QueueContext(OutputPath Record, IO<QueueSettings> Settings, TimeProvider Clock, Func<QueueEvent, IO<Unit>> Notify) {
    public static IO<QueueContext> Open =>
        from record in IO.lift(static () => OutputPath.Of(Path.Join(Directory.CreateDirectory(Session.PlugIn.SettingsDirectory).FullName, $"{nameof(QueueProgress)}.json")))
        from sender in NoticeSender.Acquire(QueueSettings.Read.Map(static held => held.Notices), DarkroomPlugIn.Clock)
        select new QueueContext(record, QueueSettings.Read, DarkroomPlugIn.Clock, sender.Notify);
}

public sealed record QueueRequest(Option<float> Clay, double Overscan, RegionFrame Region, bool Resume) {
    public static QueueRequest Default { get; } = new(None, 0d, RegionFrame.Full, Resume: false);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class Dispatch {
    private const string CompilerFile = "darkroom_render.js";
    private const float Albedo = 0.8f;

    private static readonly CommandDefinition<CommandInput.Run> Queued = new("DarkroomQueue", new CommandInput.Run(static (doc, _, _) => Edited(doc)));
    private static readonly CommandDefinition<CommandInput.Run> RenderJob = new("DarkroomRenderJob", new CommandInput.Run(static (_, _, _) => Job));
    private static readonly CommandDefinition<CommandInput.Run> RenderFarm = new("DarkroomRenderFarm", new CommandInput.Run(static (doc, mode, _) => FarmJob(doc, mode)));

    public static Action<ILoggingBuilder> Logging { get; } = static logging => logging.AddConsole(static options => options.LogToStandardErrorThreshold = LogLevel.Error);

    public static Seq<PlugInCommand> Commands { get; } = [Queued, RenderJob, RenderFarm];

    public static IO<Unit> Load =>
        from farm in FarmStartup
        from relaunched in unless(farm, Relaunched).As()
        select unit;

    private static string FarmScript => $"_-{RenderFarm.EnglishName}";

    private static string JobVariable => $"RENDER_JOB_{Session.PlugIn.Id:N}";

    private static string ParentVariable => $"RENDER_PARENT_{Session.PlugIn.Id:N}";

    // --- [EDIT]
    private static IO<Unit> Edited(RhinoDoc doc) =>
        from listed in QueueState.Read(doc).Bind(Shown)
        from book in OutputBook.Read(doc)
        from settings in QueueSettings.Read
        let targets = Seq<QueueTarget>(
            new QueueTarget.Local(),
            new QueueTarget.Packaged(QueueTarget.Jobs(book.Folder), book.Stem, PackageUse.JobFile),
            new QueueTarget.Packaged(QueueTarget.Jobs(book.Folder), book.Stem, PackageUse.Handoff),
            new QueueTarget.Flamenco(QueueTarget.Jobs(book.Folder), book.Stem, settings.Flamenco))
        from send in Getters.Stages<QueueRequest, IO<Unit>>(QueueRequest.Default, (request, kit) =>
            Getters.Choice(doc, new GetterRequest<GetOption, StageStep<QueueRequest, IO<Unit>>>(Queued.EnglishName) {
                Accept = kit.Accept with { Nothing = Some(new NothingEntry<StageStep<QueueRequest, IO<Unit>>>(None, kit.Done(IO.pure(unit)))) },
                Options = [
                    Added("ActiveView", static _ => IO.pure(Seq(QueuePlacement.Active)), doc, request, kit),
                    Added("NamedViews", QueuePlacement.Viewed, doc, request, kit),
                    Added("Snapshots", QueuePlacement.Snapped, doc, request, kit),
                    Added("SunStudy", QueuePlacement.Studied, doc, request, kit),
                    OptionSpecs.Of(OptionType.Toggle(LabelPair.OffOn), request.Clay.IsSome, clay => kit.Advance(request with { Clay = clay ? Some(Albedo) : None }), caption: "Clay"),
                    OptionSpecs.Of(OptionType.Number(0d, 1d), request.Overscan, overscan => kit.Advance(request with { Overscan = overscan })),
                    OptionSpecs.Of(OptionType.List(toSeq(RegionFrame.Items), static frame => frame.Key), request.Region, region => kit.Advance(request with { Region = region })),
                    OptionSpecs.Of(OptionType.Toggle(LabelPair.NoYes), request.Resume, resume => kit.Advance(request with { Resume = resume })),
                    OptionSpecs.Of(OptionType.Integer(1, None), 1, index => new QueueEdit.Remove(HashSet(index - 1)).Committed(doc).Bind(Shown) >> kit.Advance(request), caption: "Remove"),
                    OptionSpecs.Of(OptionType.Integer(1, None), 1, index =>
                        QueueState.Read(doc).Bind(state => state.Entries.At(index - 1).ToFin(new DarkroomFault("render queue entry", DarkroomReason.Missing, index)).Match(
                            Succ: entry => new QueueEdit.Toggle(index - 1, !entry.Renders).Committed(doc).Bind(Shown) >> kit.Advance(request),
                            Fail: kit.Retry)), caption: "Toggle"),
                    OptionSpecs.Of(OptionType.List(targets, static target => target.Name), targets[0], target => kit.Done(Send(doc, target, request.Resume)), caption: "Send"),
                ],
            }))
        from sent in send
        select unit;

    private static OptionSpec<StageStep<QueueRequest, IO<Unit>>> Added(
        string caption, Func<RhinoDoc, IO<Seq<QueuePlacement>>> placed, RhinoDoc doc, QueueRequest request, StageKit<QueueRequest, IO<Unit>> kit) =>
        OptionSpecs.Plain(caption, None, hidden: false, _ =>
            from placements in placed(doc)
            from entries in QueuePlacement.Entries(doc, placements, request)
            from state in new QueueEdit.Add(entries).Committed(doc)
            from shown in Shown(state)
            from step in kit.Advance(request)
            select step);

    private static IO<Unit> Shown(QueueState state) =>
        IO.lift(() => state.Entries.Iter(static (index, entry) => Entry(Session.Logger, index + 1, entry.Label, entry.Renders)));

    // --- [SEND]
    public static IO<Unit> Send(RhinoDoc doc, QueueTarget target, bool resume) {
        return IO.env.Bind(environment => Products.Transfer(
            () => EnvIO.New(token: environment.Token, syncContext: environment.SyncContext),
            scope => (from context in QueueContext.Open
                      from plan in QueuePlan.Read(doc)
                      from resumed in resume ? QueueProgress.Read(context.Record).Map(static prior => prior.Filter(static record => !record.Succeeded)) : IO.pure(Option<QueueProgress>.None)
                      from stamp in IO.lift(() => context.Clock.ToZonedClock().GetCurrentLocalDateTime())
                      from check in Preflight.Check(context, doc, plan, target).OnUiThread()
                      from cautioned in IO.lift(() => check.Cautions.Iter(static caution => Cautioned(Session.Logger, caution.Error.Message)))
                      from estimated in IO.lift(() => check.Memory.Iter(static (index, bytes) => Estimated(Session.Logger, index + 1, bytes)))
                      from verdict in IO.lift(check.Verdict)
                      select target.Switch(
                          (Context: context, Doc: doc, Plan: plan, Stamp: stamp, Resumed: resumed, Scope: scope),
                          local: static (state, _) => IO.lift(() => Session.Start((
                              from batch in state.Plan.Resolved(state.Doc, state.Resumed).OnUiThread()
                              from ran in QueueRuns.Run(state.Context, state.Doc, batch, None, state.Resumed)
                              select ran).Bracket().Scoped(state.Scope.Resources), owned: state.Scope)).Map(unit),
                          packaged: static (state, packaged) => Completed(packaged.Use.Quits
                              ? Handed(state.Context, state.Doc, state.Plan, state.Stamp, state.Resumed, packaged.Folder, packaged.Stem)
                              : Packed(state.Doc, state.Plan, state.Resumed, packaged.Folder, packaged.Stem, state.Stamp).OnUiThread().Map(static _ => unit), state.Scope),
                          flamenco: static (state, farm) => Completed(
                              from installed in farm.Farm.Scripts.Traverse(InstallCompiler).As()
                              from paths in Packed(state.Doc, state.Plan, state.Resumed, farm.Folder, farm.Stem, state.Stamp).OnUiThread()
                              from submitted in Submitted(farm.Farm, paths[PackageFile.Copy], farm.Farm.Platform.Arguments(FarmScript))
                              from noticed in state.Context.Notify(new QueueEvent.Submitted(submitted))
                              select unit, state.Scope))).Scoped(scope).Flatten()).Uninterruptible());

        static IO<Unit> Completed(IO<Unit> send, EnvIO scope) => send.Scoped(scope).Finally(IO.lift(scope.Dispose));
    }

    // --- [FLAMENCO]
    public static IO<Unit> InstallCompiler(string scripts) =>
        (from path in IO.lift(() => OutputPath.Of(Path.Join(scripts, CompilerFile)))
         from source in Products.Of(static () => typeof(Dispatch).Assembly.GetManifestResourceStream(CompilerFile))
         from written in FilePublication.Write(path, overwrite: true, target => IO.liftAsync(environment => source.CopyToAsync(target, environment.Token).ToUnit()))
         select written).Bracket();

    private static IO<bool> FarmStartup =>
        IO.lift(static () => (OperatingSystem.IsMacOS() ? SafeNativeMethods.Arguments() : Environment.GetCommandLineArgs()).AsSpan().EndsWith(WorkerPlatform.Current().Arguments(FarmScript)));

    private static IO<Uri> Submitted(FlamencoSettings farm, string copy, string[] arguments) =>
        (from client in use(static () => new HttpClient())
         from advertised in IO.liftAsync(environment => client.GetFromJsonAsync(new Uri(farm.Manager, "/api/v3/jobs/types"), FlamencoContext.Default.FlamencoTypes, environment.Token))
         from types in IO.lift(Conversions.Required(advertised))
         let type = Path.GetFileNameWithoutExtension(CompilerFile).Replace('_', '-')
         from compiled in IO.lift(toSeq(types.JobTypes).Find(job => string.Equals(job.Name, type, StringComparison.Ordinal)).ToFin(new DarkroomFault((FormattableString)$"Flamenco job type {type}", DarkroomReason.Missing)))
         from submitted in IO.liftAsync(async environment => {
             using JsonContent content = JsonContent.Create(new FlamencoJob(
                 Path.GetFileNameWithoutExtension(copy), type, compiled.Etag, farm.Priority, WorkerPlatform.Current().Key,
                 new FlamencoJob.Parameters(copy, arguments), farm.WorkerTag.ToNullable()), FlamencoContext.Default.FlamencoJob);
             using HttpResponseMessage response = await client.PostAsync(new Uri(farm.Manager, "/api/v3/jobs"), content, environment.Token).ConfigureAwait(false);
             return response.StatusCode is HttpStatusCode.OK
                 ? Conversions.Required(await response.Content.ReadFromJsonAsync(FlamencoContext.Default.FlamencoReceipt, environment.Token).ConfigureAwait(false))
                     .Map(receipt => new Uri(farm.Manager, $"/jobs/{receipt.Id:D}"))
                 : new DarkroomFault((FormattableString)$"Flamenco answering {response.StatusCode}", DarkroomReason.Refused);
         })
         from received in IO.lift(submitted)
         select received).Bracket()
            | @catch(static error => error.HasException<HttpRequestException>(), error => IO.fail<Uri>(new DarkroomFault((FormattableString)$"render job post to {farm.Manager}", DarkroomReason.Failed, Inner: error)));

    // --- [PACKAGE]
    private static IO<HashMap<PackageFile, OutputPath>> Packed(RhinoDoc doc, QueuePlan plan, Option<QueueProgress> resumed, OutputFolder folder, Stem stem, LocalDateTime stamp) =>
        from batch in plan.Resolved(doc, resumed)
        from date in IO.lift(NamePart.Validated(LocalDateTimePattern.ExtendedIso.Format(stamp.With(TimeAdjusters.TruncateToSecond))))
        from placed in Destinations.Resolve(doc, folder, stem, overwrite: false, OutputConvention.Default, toSeq(PackageFile.Items).Map(file => file.Name(date)))
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
         from start in IO.lift(() => Launch(paths[PackageFile.Job]))
         from relayed in Products.Of(() => Process.Start(start))
         from quit in Quit.OnUiThread()
         select unit).Bracket();

    private static Fin<ProcessStartInfo> Launch(string job) =>
        ($"_{RenderJob.EnglishName}", Environment.ProcessId.ToString(CultureInfo.InvariantCulture)) switch {
            var (script, parent) => OperatingSystem.IsMacOS()
                ? new ProcessStartInfo("open", [
                    "-n", "-g", "-a", RhinoApp.GetInstallDirectory().FullName,
                    "--env", $"{JobVariable}={job}", "--env", $"{ParentVariable}={parent}", "--args", .. WorkerPlatform.MacOs.Arguments(script),
                ]) { UseShellExecute = false }
                : Conversions.Required(Environment.ProcessPath).Map(path => new ProcessStartInfo(path, WorkerPlatform.Windows.Arguments(script)) {
                    Environment = { [JobVariable] = job, [ParentVariable] = parent },
                    UseShellExecute = false,
                }),
        };

    private static IO<Unit> Quit =>
        from flushed in IO.lift(PlugIn.FlushSettingsSavedQueue)
        from exited in IO.lift(static () => RhinoApp.Exit(allowCancel: false))
        select unit;

    // --- [JOBS]
    private static IO<Unit> Job =>
        from path in IO.lift(static () => Conversions.Present(Environment.GetEnvironmentVariable(JobVariable)).ToFin(new DarkroomFault(JobVariable, DarkroomReason.Missing)))
        from parent in IO.lift(static () => Conversions.Present(Environment.GetEnvironmentVariable(ParentVariable))
            .Traverse(static text => int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out int id) ? Fin.Succ(id) : new DarkroomFault(text, DarkroomReason.Invalid)).As())
        from started in IO.lift(() => Session.Start(Launched(parent.Traverse(MachinePower.Exited).As().Bind(_ => Run(path)))))
        select unit;

    private static IO<Unit> FarmJob(RhinoDoc doc, RunMode mode) =>
        from scripted in IO.lift(() => DarkroomFault.Unless(mode is RunMode.Scripted, DarkroomReason.Invalid, mode))
        from started in IO.lift(() => Session.Start(
            from code in Farmed(doc).Map(static _ => 0).Catch(static error => IO.lift(() => Session.Failed(error)).Map(_ => QueueRuns.Stopping(error) ? 2 : 1)).As()
            from flushed in IO.lift(PlugIn.FlushSettingsSavedQueue).OnUiThread()
            from exited in IO.lift(() => Environment.Exit(code)).OnUiThread()
            select unit))
        select unit;

    private static IO<Unit> Farmed(RhinoDoc doc) =>
        (from path in IO.lift(() => Conversions.Present(doc.Path).ToFin(new DarkroomFault(doc.Name, DarkroomReason.Unsaved)).Bind(static saved => InputPath.Of(Path.ChangeExtension(saved, PackageFile.Job.Extension)))).OnUiThread()
         from job in QueueJob.Read(path)
         from record in IO.lift(OutputPath.Of(Path.ChangeExtension(path, ".progress.json")))
         from context in QueueContext.Open
         from ran in Ran(context with { Record = record, Settings = context.Settings.Map(static settings => settings with { ShutDownAfterSuccess = false }) }, doc, path, job)
         select ran).Bracket();

    private static IO<Unit> Relaunched =>
        IO.lift(static () => Session.Start((
            from context in QueueContext.Open
            from found in QueueRuns.Resumable(context)
            from ran in found.Traverse(static relaunch => Launched(IO.yieldFor(relaunch.Delay.ToTimeSpan()) >> Run(relaunch.Job))).As()
            select unit).Bracket())).Map(unit);

    private static IO<Unit> Launched(IO<Unit> run) => (Products.Released(Quit.OnUiThread()) >> run).Bracket();

    private static IO<Unit> Run(string path) =>
        (from input in IO.lift(InputPath.Of(path))
         from job in QueueJob.Read(input)
         from opened in IO.lift(InputPath.Of(job.Copy))
         from context in QueueContext.Open
         from ran in Documents.With(new DocumentSource.Opened(opened), doc => Ran(context, doc, input, job))
         select ran).Bracket();

    private static IO<Unit> Ran(QueueContext context, RhinoDoc doc, string path, QueueJob job) =>
        from prior in QueueProgress.Read(context.Record)
        from batch in job.Batch(doc).OnUiThread()
        from ran in QueueRuns.Run(context, doc, batch, Some(path), prior.Filter(held => held.Job == Some(path) && !held.Succeeded))
        select ran;

    // --- [LOGS]
    [LoggerMessage(Level = LogLevel.Warning, Message = "{Caution}")]
    private static partial void Cautioned(ILogger logger, string caution);

    [LoggerMessage(Level = LogLevel.Information, Message = "Render queue entry {Index} needs an estimated {Bytes} bytes of memory")]
    private static partial void Estimated(ILogger logger, int index, ulong bytes);

    [LoggerMessage(Level = LogLevel.Information, Message = "Render queue entry {Index}: {Label}, renders {Renders}")]
    private static partial void Entry(ILogger logger, int index, string label, bool renders);
}

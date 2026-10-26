using Rhino;
using Rhino.Collections;
using Rhino.Commands;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Document.Files;

// --- [TYPES] ---------------------------------------------------------------------------
public enum DocumentPhase { Initializing, Creating, Opening, Ready, Closing, Unavailable }

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record DocumentSource {
    public sealed record Live(RhinoDoc Doc) : DocumentSource;

    public sealed record Active() : DocumentSource;

    public sealed record Keyed(uint Serial) : DocumentSource;

    public sealed record Opened(InputPath Path) : DocumentSource;

    public sealed record WorksessionFile(InputPath Path) : DocumentSource;

    public sealed record Headless(Option<InputPath> Template) : DocumentSource;

    public sealed record Archive(InputPath Path, Option<ArchivableDictionary> Options) : DocumentSource;
}

public sealed record DocumentState(
    uint Serial,
    Option<string> Path,
    DocumentPhase Phase,
    bool IsReadOnly,
    bool IsLocked,
    bool UndoRecordingEnabled,
    bool UndoRecordingIsActive,
    bool UndoActive,
    bool RedoActive,
    bool IsHeadless,
    bool Modified,
    bool InGetPoint,
    int InCommand,
    Option<Guid> ActiveCommandId);

public sealed record WorksessionState(Option<string> FileName, Seq<WorksessionModel> Models);

[Union]
public abstract partial record WorksessionChange {
    public sealed record Attach(Seq<InputPath> Paths) : WorksessionChange;

    public sealed record Detach(Seq<uint> Models) : WorksessionChange;

    public sealed record Update(Option<uint> Model) : WorksessionChange;

    public sealed record SetActiveModel(uint Model) : WorksessionChange;

    public sealed record Save(Option<OutputPath> Path) : WorksessionChange;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class DocumentMapper {
    [MapProperty(nameof(RhinoDoc.RuntimeSerialNumber), nameof(DocumentState.Serial))]
    [MapPropertyFromSource(nameof(DocumentState.Phase), Use = nameof(Phase))]
    [MapPropertyFromSource(nameof(DocumentState.InCommand), Use = nameof(InCommand))]
    public static partial DocumentState ToState(RhinoDoc doc);

    private static DocumentPhase Phase(RhinoDoc doc) =>
        doc switch {
            { IsClosing: true } => DocumentPhase.Closing,
            { IsOpening: true } => DocumentPhase.Opening,
            { IsInitializing: true } => DocumentPhase.Initializing,
            { IsCreating: true } => DocumentPhase.Creating,
            { IsAvailable: true } => DocumentPhase.Ready,
            _ => DocumentPhase.Unavailable,
        };

    private static int InCommand(RhinoDoc doc) => doc.InCommand(bIgnoreScriptRunnerCommands: true);
}

public static class Documents {
    // --- [LEASE]
    public static IO<T> WithDocument<T>(DocumentSource source, Func<RhinoDoc, IO<T>> body) =>
        DisposalOps.Within<RhinoDoc, T>(retain => source.Switch(
            retain,
            live: static (_, live) => IO.pure(live.Doc),
            active: static (_, _) => IO.lift(static () => Refused.UnlessFound(RhinoDoc.ActiveDoc, nameof(RhinoDoc.ActiveDoc))),
            keyed: static (_, keyed) =>
                IO.lift(() => Refused.UnlessFound(RhinoDoc.FromRuntimeSerialNumber(keyed.Serial), nameof(RhinoDoc.FromRuntimeSerialNumber))),
            opened: static (_, opened) =>
                IO.lift(() => Refused.UnlessFound(RhinoDoc.Open(opened.Path, out bool _), nameof(RhinoDoc.Open))),
            worksessionFile: static (_, file) =>
                IO.lift(() => Refused.UnlessFound(Worksession.Open(file.Path), nameof(Worksession.Open))),
            headless: static (own, headless) =>
                headless.Template.Map(static template => (string)template) switch {
                    var template => DisposalOps.Acquire(IO.lift(() => Try.lift(() => Refused.UnlessFound(RhinoDoc.CreateHeadless(Conversions.Unset(template)), nameof(RhinoDoc.CreateHeadless)))
                        .Run()
                        .MapFail(error => error.HasException<ArgumentException>() ? new FileRefused(Conversions.Unset(template), FileReason.Unreadable, error) : error)), own),
                },
            archive: static (own, archive) =>
                DisposalOps.Acquire(IO.lift(() => Refused.UnlessFound(RhinoDoc.OpenHeadless(archive.Path, archive.Options.ValueUnsafe()), nameof(RhinoDoc.OpenHeadless))), own)).OnUiThread().Uninterruptible().Bind(body),
            static held => DisposalOps.Release(held).OnUiThread());

    // --- [WORKSESSION]
    public static IO<WorksessionState> ReadWorksession(RhinoDoc doc, bool checkForUpdates) =>
        IO.lift(() => new WorksessionState(Conversions.Present(doc.Worksession.FileName), Conversions.Rows(doc.Worksession.GetModels(checkForUpdates))));

    public static IO<Option<string>> GetLockInformation(InputPath model) =>
        IO.lift(() => Conversions.Present(Worksession.GetLockInformation(model)));

    public static IO<RhinoDoc> ChangeWorksession(RhinoDoc doc, WorksessionChange change) =>
        change.Switch(
            doc,
            attach: static (target, attach) =>
                IO.lift(() => Callbacks.Each(attach.Paths, path => target.Worksession.Attach(path), nameof(Worksession.Attach)).Map(_ => target)),
            detach: static (target, detach) =>
                IO.lift(() => Callbacks.Each(detach.Models, target.Worksession.Detach, nameof(Worksession.Detach)).Map(_ => target)),
            update: static (target, update) =>
                IO.lift(() => Refused.Unless(target.Worksession.Update(Conversions.Unset(update.Model)), target, nameof(Worksession.Update))),
            setActiveModel: static (target, active) =>
                IO.lift(() => Refused.UnlessFound(target.Worksession.SetActiveModel(active.Model), nameof(Worksession.SetActiveModel))),
            save: static (target, save) =>
                IO.lift(() => save.Path.Match(
                    Some: path => Refused.Unless(target.Worksession.SaveAs(path), target, nameof(Worksession.SaveAs)),
                    None: () => Refused.Unless(target.Worksession.Save(), target, nameof(Worksession.Save)))));

    // --- [SCRIPTS]
    public static Fin<RhinoDoc> Scripted(RhinoDoc doc) =>
        Refused.UnlessFound(RhinoDoc.FromRuntimeSerialNumber(doc.RuntimeSerialNumber), nameof(RhinoDoc.FromRuntimeSerialNumber))
            .Bind(static open => (
                    Command.InCommand() && !Command.InScriptRunnerCommand() ? Error.New("command other than script runner is running") : Fin.Succ(unit),
                    open.IsHeadless ? Error.New($"headless document {open.RuntimeSerialNumber} takes no script input") : Fin.Succ(unit))
                .Apply((_, _) => open)
                .As());

    public static IO<Unit> RunScript(RhinoDoc doc, string script, bool echo, Option<string> display) =>
        IO.lift(() => Scripted(doc).Bind(open => Refused.Unless(RhinoApp.RunScript(open.RuntimeSerialNumber, script, Conversions.Unset(display), echo), nameof(RhinoApp.RunScript))));
}

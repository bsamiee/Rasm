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
        (from doc in source.Switch(
                 live: static live => IO.pure(live.Doc),
                 active: static _ => IO.lift(static () => Conversions.Found(RhinoDoc.ActiveDoc)),
                 keyed: static keyed => IO.lift(() => Conversions.Found(RhinoDoc.FromRuntimeSerialNumber(keyed.Serial))),
                 opened: static opened => IO.lift(() => Conversions.Found(RhinoDoc.Open(opened.Path, out bool _))),
                 worksessionFile: static file => IO.lift(() => Conversions.Found(Worksession.Open(file.Path))),
                 headless: static headless => headless.Template.Map(static template => (string)template) switch {
                     var template => Owned(IO.lift(() => Conversions.Found(RhinoDoc.CreateHeadless(Conversions.Unset(template))))
                         .MapFail(error => error.HasException<ArgumentException>() ? new RhinoFault(Conversions.Unset(template), RhinoReason.Unreadable, Inner: error) : error)),
                 },
                 archive: static archive => Owned(IO.lift(() => Conversions.Found(RhinoDoc.OpenHeadless(archive.Path, archive.Options.ValueUnsafe())))))
             .OnUiThread().Uninterruptible()
         from value in body(doc)
         select value).Bracket();

    private static IO<RhinoDoc> Owned(IO<RhinoDoc> open) =>
        use(open, static held => IO.lift(held.Dispose).OnUiThread());

    // --- [WORKSESSION]
    public static IO<WorksessionState> ReadWorksession(RhinoDoc doc, bool checkForUpdates) =>
        IO.lift(() => new WorksessionState(Conversions.Present(doc.Worksession.FileName), Conversions.Rows(doc.Worksession.GetModels(checkForUpdates))));

    public static IO<Option<string>> GetLockInformation(InputPath model) =>
        IO.lift(() => Conversions.Present(Worksession.GetLockInformation(model)));

    public static IO<RhinoDoc> ChangeWorksession(RhinoDoc doc, WorksessionChange change) =>
        IO.lift(() => change.Switch(
            doc,
            attach: static (target, attach) => Callbacks.Each(attach.Paths, (path, index) => RhinoFault.Unless(target.Worksession.Attach(path), index)).Map(_ => target),
            detach: static (target, detach) => Callbacks.Each(detach.Models, (model, index) => RhinoFault.Unless(target.Worksession.Detach(model), index)).Map(_ => target),
            update: static (target, update) => RhinoFault.Unless(target.Worksession.Update(Conversions.Unset(update.Model))).Map(_ => target),
            setActiveModel: static (target, active) => Conversions.Found(target.Worksession.SetActiveModel(active.Model)),
            save: static (target, save) => save.Path.Match(
                Some: path => RhinoFault.Unless(target.Worksession.SaveAs(path)),
                None: () => RhinoFault.Unless(target.Worksession.Save())).Map(_ => target)));

    // --- [SCRIPTS]
    public static Fin<RhinoDoc> Scripted(RhinoDoc doc) =>
        Conversions.Found(RhinoDoc.FromRuntimeSerialNumber(doc.RuntimeSerialNumber))
            .Bind(static open => (
                    RhinoFault.Unless(!Command.InCommand() || Command.InScriptRunnerCommand()),
                    RhinoFault.Unless(!open.IsHeadless))
                .Apply((_, _) => open)
                .As());

    public static IO<Unit> RunScript(RhinoDoc doc, string script, bool echo, Option<string> display) =>
        IO.lift(() => Scripted(doc).Bind(open => RhinoFault.Unless(RhinoApp.RunScript(open.RuntimeSerialNumber, script, Conversions.Unset(display), echo))));
}

using Rhino;
using Rhino.Collections;
using Rhino.DocObjects;
using Rhino.FileIO;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Document;

// --- [TYPES] ---------------------------------------------------------------------------
public enum DocumentPhase { Ready = 0, Opening = 1, Closing = 2, Initializing = 3, Creating = 4, Unavailable = 5 }

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct DocumentHandle(RhinoDoc Doc, bool Owned);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DocumentSource {
    public sealed record Live(RhinoDoc Doc) : DocumentSource;

    public sealed record Keyed(uint Serial) : DocumentSource;

    public sealed record Opened(string Path) : DocumentSource;

    public sealed record Empty() : DocumentSource;

    public sealed record Template(string Path) : DocumentSource;

    public sealed record Archive(string Path) : DocumentSource;

    public sealed record Configured(string Path, Func<ArchivableDictionary> Options) : DocumentSource;

    public sealed record Active() : DocumentSource;

    public sealed record WorksessionFile(string Path) : DocumentSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FileOp {
    public sealed record Import(string Path, Option<ArchivableDictionary> Options) : FileOp;

    public sealed record ReadFile(string Path, Func<FileReadOptions> Options) : FileOp;

    public sealed record Export(string Path, Option<ArchivableDictionary> Options) : FileOp;

    public sealed record ExportSelected(string Path, Option<ArchivableDictionary> Options) : FileOp;

    public sealed record WriteFile(string Path, Func<FileWriteOptions> Options) : FileOp;

    public sealed record Write3dmFile(string Path, Func<FileWriteOptions> Options) : FileOp;

    public sealed record Save() : FileOp;

    public sealed record SaveAs(string Path) : FileOp;

    public sealed record SaveAsTemplate(string Path) : FileOp;
}

public sealed record DocumentState(
    uint Serial,
    Option<string> Path,
    Option<string> Name,
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

public sealed record WorksessionState(Option<string> FileName, Option<string> Name, Seq<WorksessionModel> Models);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WorksessionChange {
    public sealed record Attach(Seq<string> Paths) : WorksessionChange;

    public sealed record Detach(Seq<uint> Serials) : WorksessionChange;

    public sealed record Update(uint Serial) : WorksessionChange;

    public sealed record UpdateAll() : WorksessionChange;

    public sealed record SetActiveModel(uint Serial) : WorksessionChange;

    public sealed record Save() : WorksessionChange;

    public sealed record SaveAs(string Path) : WorksessionChange;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class DocumentMapper {
    [MapProperty(nameof(RhinoDoc.RuntimeSerialNumber), nameof(DocumentState.Serial))]
    [MapPropertyFromSource(nameof(DocumentState.Phase), Use = nameof(Phase))]
    [MapPropertyFromSource(nameof(DocumentState.InCommand), Use = nameof(InCommand))]
    internal static partial DocumentState ToState(RhinoDoc doc);

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

public static class DocumentHandles {
    // --- [LIFECYCLE]
    public static IO<DocumentHandle> Acquire(DocumentSource source) =>
        source.Switch(
            live: static live => IO.pure(new DocumentHandle(live.Doc, Owned: false)),
            keyed: static keyed => Resolve(keyed.Serial).Map(static doc => new DocumentHandle(doc, Owned: false)),
            opened: static opened => FromPath(opened.Path, static path => RhinoDoc.Open(path, out _), nameof(RhinoDoc.Open), owned: false),
            empty: static _ => IO.lift(static () => Missing.Unless(RhinoDoc.CreateHeadless(""), nameof(RhinoDoc.CreateHeadless)))
                .Map(static doc => new DocumentHandle(doc, Owned: true)),
            template: static template => FromPath(template.Path, RhinoDoc.CreateHeadless, nameof(RhinoDoc.CreateHeadless), owned: true),
            archive: static archive => FromPath(archive.Path, RhinoDoc.OpenHeadless, nameof(RhinoDoc.OpenHeadless), owned: true),
            configured: static configured => FromPath(configured.Path, path => RhinoDoc.OpenHeadless(path, configured.Options()), nameof(RhinoDoc.OpenHeadless), owned: true),
            active: static _ => IO.lift(static () => Missing.Unless(RhinoDoc.ActiveDoc, nameof(RhinoDoc.ActiveDoc)))
                .Map(static doc => new DocumentHandle(doc, Owned: false)),
            worksessionFile: static worksession => FromPath(worksession.Path, Worksession.Open, nameof(Worksession.Open), owned: false));

    public static IO<Unit> Release(DocumentHandle handle) =>
        when(handle.Owned, IO.lift(handle.Doc.Dispose)).As();

    public static IO<TValue> WithDocument<TValue>(DocumentSource source, Func<RhinoDoc, IO<TValue>> body) =>
        Acquire(source).Bracket(Use: handle => body(handle.Doc), Fin: Release);

    private static IO<DocumentHandle> FromPath(string path, Func<string, RhinoDoc?> open, string member, bool owned) =>
        AtPath(path, qualified => Missing.Unless(open(qualified), member)).Map(doc => new DocumentHandle(doc, owned));

    private static IO<T> AtPath<T>(string path, Func<string, Fin<T>> call) =>
        from qualified in Answers.QualifiedPath(path)
        from answer in IO.lift(() => call(qualified)).Catch(static error => error.HasException<ArgumentException>(), static _ => IO.fail<T>(new Invalid(nameof(path))))
        select answer;

    // --- [READS]
    public static IO<Seq<uint>> OpenDocuments(bool includeHeadless) =>
        IO.lift(() => toSeq(RhinoDoc.OpenDocuments(includeHeadless)).Map(static doc => doc.RuntimeSerialNumber).Strict());

    public static IO<RhinoDoc> Resolve(uint serial) =>
        IO.lift(() => Missing.Unless(RhinoDoc.FromRuntimeSerialNumber(serial), nameof(RhinoDoc.FromRuntimeSerialNumber)));

    [UserMapping]
    public static Option<uint> Serial(RhinoDoc? doc) =>
        Optional(doc).Map(static document => document.RuntimeSerialNumber);

    public static IO<DocumentState> Snapshot(RhinoDoc doc) =>
        IO.lift(() => DocumentMapper.ToState(doc));

    // --- [WORKSESSION]
    public static IO<WorksessionState> ReadWorksession(RhinoDoc doc, bool checkForUpdates) =>
        IO.lift(() => new WorksessionState(
            Answers.Present(doc.Worksession.FileName),
            Answers.Present(doc.Worksession.Name),
            toSeq(doc.Worksession.GetModels(checkForUpdates))));

    public static IO<Option<string>> GetLockInformation(string modelPath) =>
        Answers.QualifiedPath(modelPath).Map(static path => Answers.Present(Worksession.GetLockInformation(path)));

    public static IO<RhinoDoc> ChangeWorksession(RhinoDoc doc, WorksessionChange change) =>
        change.Switch(
            doc,
            attach: static (target, attach) =>
                from paths in attach.Paths.TraverseM(Answers.ExistingPath).As()
                from attached in IO.lift(() => Answers.Each(paths, target.Worksession.Attach, nameof(Worksession.Attach)))
                select target,
            detach: static (target, detach) =>
                IO.lift(() => Answers.Each(detach.Serials, target.Worksession.Detach, nameof(Worksession.Detach))).Map(_ => target),
            update: static (target, update) => IO.lift(() => Refused.Unless(target.Worksession.Update(update.Serial), target, nameof(Worksession.Update))),
            updateAll: static (target, _) => IO.lift(() => Refused.Unless(target.Worksession.UpdateAll(), target, nameof(Worksession.UpdateAll))),
            setActiveModel: static (target, active) =>
                IO.lift(() => Missing.Unless(target.Worksession.SetActiveModel(active.Serial), nameof(Worksession.SetActiveModel))),
            save: static (target, _) => IO.lift(() => Refused.Unless(target.Worksession.Save(), target, nameof(Worksession.Save))),
            saveAs: static (target, saveAs) =>
                from path in Answers.QualifiedPath(saveAs.Path)
                from saved in IO.lift(() => Refused.Unless(target.Worksession.SaveAs(path), target, nameof(Worksession.SaveAs)))
                select saved);

    // --- [SCRIPTING]
    public static IO<Unit> Scripted(RhinoDoc doc, string script) =>
        IO.lift(() => Refused.Unless(RhinoApp.RunScript(doc.RuntimeSerialNumber, script, echo: false), nameof(RhinoApp.RunScript)))
            .Catch(static error => error.HasException<ApplicationException>(), static _ => IO.fail<Unit>(new Refused(nameof(RhinoApp.RunScript))));

    // --- [FILES]
    public static IO<Unit> ApplyFileOp(RhinoDoc doc, FileOp op) =>
        op.Switch(
            doc,
            import: static (target, import) =>
                from path in Answers.QualifiedPath(import.Path)
                from imported in IO.lift(() => Refused.Unless(target.Import(path, import.Options.ValueUnsafe()), nameof(RhinoDoc.Import)))
                    .Catch(static error => error.HasException<FileNotFoundException>(), static _ => IO.fail<Unit>(new Missing(nameof(File.Exists))))
                select imported,
            readFile: static (target, read) =>
                from path in Answers.ExistingPath(read.Path)
                from active in IO.lift(() => Invalid.Unless(Serial(RhinoDoc.ActiveDoc) == Some(target.RuntimeSerialNumber), nameof(RhinoDoc.ActiveDoc)))
                from done in DisposalOps.Using(read.Options, options => IO.lift(() => Refused.Unless(RhinoDoc.ReadFile(path, options), nameof(RhinoDoc.ReadFile))))
                select done,
            export: static (target, export) =>
                from path in Answers.QualifiedPath(export.Path)
                from exported in IO.lift(() => Refused.Unless(target.Export(path, export.Options.ValueUnsafe()), nameof(RhinoDoc.Export)))
                select exported,
            exportSelected: static (target, export) =>
                from path in Answers.QualifiedPath(export.Path)
                from exported in IO.lift(() => Refused.Unless(target.ExportSelected(path, export.Options.ValueUnsafe()), nameof(RhinoDoc.ExportSelected)))
                select exported,
            writeFile: static (target, write) =>
                from path in Answers.QualifiedPath(write.Path)
                from written in DisposalOps.Using(write.Options, options => IO.lift(() => Refused.Unless(target.WriteFile(path, options), nameof(RhinoDoc.WriteFile))))
                select written,
            write3dmFile: static (target, write) =>
                from path in Answers.QualifiedPath(write.Path)
                from written in DisposalOps.Using(write.Options, options => IO.lift(() => Refused.Unless(target.Write3dmFile(path, options), nameof(RhinoDoc.Write3dmFile))))
                select written,
            save: static (target, _) => IO.lift(() => Refused.Unless(target.Save(), nameof(RhinoDoc.Save)))
                .Catch(static error => error.HasException<InvalidOperationException>(), static _ => IO.fail<Unit>(new Missing(nameof(RhinoDoc.Path)))),
            saveAs: static (target, saveAs) => AtPath(saveAs.Path, path => Refused.Unless(target.SaveAs(path), nameof(RhinoDoc.SaveAs))),
            saveAsTemplate: static (target, template) => AtPath(template.Path, path => Refused.Unless(target.SaveAsTemplate(path), nameof(RhinoDoc.SaveAsTemplate))));
}

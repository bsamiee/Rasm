using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Views;
using Rhino;
using Rhino.FileIO;
using Rhino.PlugIns;
using Rhino.UI;

namespace Rasm.Rhino.Plugin;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class FileFormats {
    // --- [LIST]
    public static IO<FileTypeList> AddFileTypes<TOptions>(Seq<FileFormat<TOptions>> formats, IPlugInSink sink) where TOptions : class =>
        from list in IO.lift(static () => new FileTypeList())
        from added in IO.lift(() => Callbacks.Each(
            formats,
            format => list.AddFileType(
                RowText.Localize(format.Description, table: Some<object>(sink)).Local,
                format.Extensions.Map(static extension => (string)extension),
                format.Options.IsSome) >= 0,
            nameof(FileTypeList.AddFileType)))
        select list;

    // --- [RUN]
    public static IO<A> Run<TOptions, A>(Seq<FileFormat<TOptions>> formats, int index, string path, RhinoDoc doc, TOptions options, A done, A cancelled, string member) where TOptions : class =>
        (from format in IO.lift(formats.At(index).ToFin(new IndexOutOfRange(member, index, formats.Count)))
         from ran in format.Run(path, doc, options)
         select done)
            .Catch(static error => error.Is(Errors.Cancelled), _ => IO.pure(cancelled));

    // --- [OPTIONS]
    public static IO<Unit> DisplayOptionsDialog<TOptions>(IPlugInViews plugIn, Seq<FileFormat<TOptions>> formats, string description, string extension) where TOptions : class =>
        (from dialog in IO.lift(() => formats
             .Find(format =>
                 string.Equals(RowText.Localize(format.Description, table: Some<object>(plugIn)).Local, description, StringComparison.Ordinal)
                 && string.Equals(((string)format.Extensions.Head)[1..], extension, StringComparison.Ordinal))
             .Bind(static format => format.Options)
             .ToFin(new UnmatchedFormat(description, extension)))
         from shown in Documents.WithDocument(new DocumentSource.Active(), document => Showing.ShowSemiModal(plugIn, dialog, document))
         select unit)
            .Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(unit));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class DefinedPlugIn(PlugInDefinition definition) : PlugIn, IPlugInViews, IPlugInLifetime {
    // --- [LIFECYCLE]
    private readonly PlugInDefinition definition = definition;
    private readonly PlugInLifetime lifetime = new(definition, Seq<View>());

    public sealed override PlugInLoadTime LoadTime => lifetime.LoadTime(base.LoadTime);

    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) {
        (LoadReturnCode code, errorMessage) = lifetime.Load(this, CallbackSite.Of(this));
        return code;
    }

    protected sealed override void CreateCommands() {
        base.CreateCommands();
        lifetime.Register(RegisterCommand, CallbackSite.Of(this));
    }

    protected sealed override void OnShutdown() {
        base.OnShutdown();
        lifetime.Shutdown(CallbackSite.Of(this));
    }

    protected sealed override void ResetMessageBoxes() {
        base.ResetMessageBoxes();
        Callbacks.Run(HostDialogs.ResetMessageBoxes(lifetime.Views, ((IPlugInViews)this).Settings), CallbackSite.Of(this));
    }

    public void Report(Error error, Type owner, string member) => lifetime.Session.Report(error, owner, member);

    public IO<Unit> Close => lifetime.Close;

    // --- [VIEWS]
    public ViewCatalog Views => lifetime.Views;

    public TimeProvider Clock => definition.Clock;

    public AtomHashMap<Guid, ValueHistory> Histories => lifetime.Histories;

    public IO<HistoryLimits> Limits => definition.History(((IPlugInViews)this).Settings);

    SettingsNode IPlugInViews.Settings => new(new SettingsRoot.PlugInNode(Id));

    // --- [IDENTITY]
    public sealed override object? GetPlugInObject() => definition.Published.ValueUnsafe();

    // --- [HELP]
    public sealed override bool AddToHelpMenu => definition.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => Callbacks.Succeeded(definition.Help.Map(static topic => topic.Show), static () => false, CallbackSite.Of(this));

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) {
        base.OptionsDialogPages(pages);
        Callbacks.Run(ViewRegistration.OptionsDialogPages(this, pages), CallbackSite.Of(this));
    }

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) {
        base.DocumentPropertiesDialogPages(doc, pages);
        Callbacks.Run(ViewRegistration.DocumentPropertiesDialogPages(this, doc, pages), CallbackSite.Of(this));
    }

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) {
        base.ObjectPropertiesPages(collection);
        Callbacks.Run(ViewRegistration.ObjectPropertiesPages(this, collection), CallbackSite.Of(this));
    }

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => DocumentArchive.ShouldWrite(definition.DocumentFrames, options, CallbackSite.Of(this));

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) {
        base.WriteDocument(doc, archive, options);
        DocumentArchive.Write(definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));
    }

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) {
        base.ReadDocument(doc, archive, options);
        DocumentArchive.Read(definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));
    }
}

public abstract class DefinedImportPlugIn(PlugInDefinition definition, Seq<FileFormat<FileReadOptions>> formats) : FileImportPlugIn, IPlugInViews, IPlugInLifetime {
    // --- [LIFECYCLE]
    private readonly PlugInDefinition definition = definition;
    private readonly Seq<FileFormat<FileReadOptions>> formats = formats;
    private readonly PlugInLifetime lifetime = new(definition, formats.Choose(static format => format.Options.Map<View>(static dialog => dialog)));

    public sealed override PlugInLoadTime LoadTime => lifetime.LoadTime(base.LoadTime);

    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) {
        (LoadReturnCode code, errorMessage) = lifetime.Load(this, CallbackSite.Of(this));
        return code;
    }

    protected sealed override void CreateCommands() {
        base.CreateCommands();
        lifetime.Register(RegisterCommand, CallbackSite.Of(this));
    }

    protected sealed override void OnShutdown() {
        base.OnShutdown();
        lifetime.Shutdown(CallbackSite.Of(this));
    }

    protected sealed override void ResetMessageBoxes() {
        base.ResetMessageBoxes();
        Callbacks.Run(HostDialogs.ResetMessageBoxes(lifetime.Views, ((IPlugInViews)this).Settings), CallbackSite.Of(this));
    }

    public void Report(Error error, Type owner, string member) => lifetime.Session.Report(error, owner, member);

    public IO<Unit> Close => lifetime.Close;

    // --- [VIEWS]
    public ViewCatalog Views => lifetime.Views;

    public TimeProvider Clock => definition.Clock;

    public AtomHashMap<Guid, ValueHistory> Histories => lifetime.Histories;

    public IO<HistoryLimits> Limits => definition.History(((IPlugInViews)this).Settings);

    SettingsNode IPlugInViews.Settings => new(new SettingsRoot.PlugInNode(Id));

    // --- [IDENTITY]
    public sealed override object? GetPlugInObject() => definition.Published.ValueUnsafe();

    // --- [HELP]
    public sealed override bool AddToHelpMenu => definition.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => Callbacks.Succeeded(definition.Help.Map(static topic => topic.Show), static () => false, CallbackSite.Of(this));

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) {
        base.OptionsDialogPages(pages);
        Callbacks.Run(ViewRegistration.OptionsDialogPages(this, pages), CallbackSite.Of(this));
    }

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) {
        base.DocumentPropertiesDialogPages(doc, pages);
        Callbacks.Run(ViewRegistration.DocumentPropertiesDialogPages(this, doc, pages), CallbackSite.Of(this));
    }

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) {
        base.ObjectPropertiesPages(collection);
        Callbacks.Run(ViewRegistration.ObjectPropertiesPages(this, collection), CallbackSite.Of(this));
    }

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => DocumentArchive.ShouldWrite(definition.DocumentFrames, options, CallbackSite.Of(this));

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) {
        base.WriteDocument(doc, archive, options);
        DocumentArchive.Write(definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));
    }

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) {
        base.ReadDocument(doc, archive, options);
        DocumentArchive.Read(definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));
    }

    // --- [FILES]
    protected sealed override FileTypeList AddFileTypes(FileReadOptions options) =>
        Callbacks.Answer(FileFormats.AddFileTypes(formats, this), static () => new FileTypeList(), CallbackSite.Of(this));

    protected sealed override bool ReadFile(string filename, int index, RhinoDoc? doc, FileReadOptions options) =>
        Callbacks.Answer(
            IO.lift(Missing.Unless(doc, nameof(RhinoDoc.FromRuntimeSerialNumber)))
                .Bind(document => FileFormats.Run(formats, index, filename, document, options, done: true, cancelled: false, nameof(ReadFile))),
            static () => false,
            CallbackSite.Of(this));

    protected sealed override void DisplayOptionsDialog(nint parent, string description, string extension) {
        base.DisplayOptionsDialog(parent, description, extension);
        Callbacks.Run(FileFormats.DisplayOptionsDialog(this, formats, description, extension), CallbackSite.Of(this));
    }
}

public abstract class DefinedExportPlugIn(PlugInDefinition definition, Seq<FileFormat<FileWriteOptions>> formats) : FileExportPlugIn, IPlugInViews, IPlugInLifetime {
    // --- [LIFECYCLE]
    private readonly PlugInDefinition definition = definition;
    private readonly Seq<FileFormat<FileWriteOptions>> formats = formats;
    private readonly PlugInLifetime lifetime = new(definition, formats.Choose(static format => format.Options.Map<View>(static dialog => dialog)));

    public sealed override PlugInLoadTime LoadTime => lifetime.LoadTime(base.LoadTime);

    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) {
        (LoadReturnCode code, errorMessage) = lifetime.Load(this, CallbackSite.Of(this));
        return code;
    }

    protected sealed override void CreateCommands() {
        base.CreateCommands();
        lifetime.Register(RegisterCommand, CallbackSite.Of(this));
    }

    protected sealed override void OnShutdown() {
        base.OnShutdown();
        lifetime.Shutdown(CallbackSite.Of(this));
    }

    protected sealed override void ResetMessageBoxes() {
        base.ResetMessageBoxes();
        Callbacks.Run(HostDialogs.ResetMessageBoxes(lifetime.Views, ((IPlugInViews)this).Settings), CallbackSite.Of(this));
    }

    public void Report(Error error, Type owner, string member) => lifetime.Session.Report(error, owner, member);

    public IO<Unit> Close => lifetime.Close;

    // --- [VIEWS]
    public ViewCatalog Views => lifetime.Views;

    public TimeProvider Clock => definition.Clock;

    public AtomHashMap<Guid, ValueHistory> Histories => lifetime.Histories;

    public IO<HistoryLimits> Limits => definition.History(((IPlugInViews)this).Settings);

    SettingsNode IPlugInViews.Settings => new(new SettingsRoot.PlugInNode(Id));

    // --- [IDENTITY]
    public sealed override object? GetPlugInObject() => definition.Published.ValueUnsafe();

    // --- [HELP]
    public sealed override bool AddToHelpMenu => definition.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => Callbacks.Succeeded(definition.Help.Map(static topic => topic.Show), static () => false, CallbackSite.Of(this));

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) {
        base.OptionsDialogPages(pages);
        Callbacks.Run(ViewRegistration.OptionsDialogPages(this, pages), CallbackSite.Of(this));
    }

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) {
        base.DocumentPropertiesDialogPages(doc, pages);
        Callbacks.Run(ViewRegistration.DocumentPropertiesDialogPages(this, doc, pages), CallbackSite.Of(this));
    }

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) {
        base.ObjectPropertiesPages(collection);
        Callbacks.Run(ViewRegistration.ObjectPropertiesPages(this, collection), CallbackSite.Of(this));
    }

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => DocumentArchive.ShouldWrite(definition.DocumentFrames, options, CallbackSite.Of(this));

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) {
        base.WriteDocument(doc, archive, options);
        DocumentArchive.Write(definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));
    }

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) {
        base.ReadDocument(doc, archive, options);
        DocumentArchive.Read(definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));
    }

    // --- [FILES]
    protected sealed override FileTypeList AddFileTypes(FileWriteOptions options) =>
        Callbacks.Answer(FileFormats.AddFileTypes(formats, this), static () => new FileTypeList(), CallbackSite.Of(this));

    protected sealed override WriteFileResult WriteFile(string filename, int index, RhinoDoc? doc, FileWriteOptions options) =>
        Callbacks.Answer(
            IO.lift(Missing.Unless(doc, nameof(RhinoDoc.FromRuntimeSerialNumber)))
                .Bind(document => FileFormats.Run(formats, index, filename, document, options, WriteFileResult.Success, WriteFileResult.Cancel, nameof(WriteFile))),
            static () => WriteFileResult.Failure,
            CallbackSite.Of(this));

    protected sealed override void DisplayOptionsDialog(nint parent, string description, string extension) {
        base.DisplayOptionsDialog(parent, description, extension);
        Callbacks.Run(FileFormats.DisplayOptionsDialog(this, formats, description, extension), CallbackSite.Of(this));
    }
}

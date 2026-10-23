using Rasm.Rhino.Document.Files;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.FileIO;
using Rhino.PlugIns;
using Rhino.UI;

namespace Rasm.Rhino.PlugIns;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class FileFormats {
    // --- [LIST]
    public static IO<FileTypeList> AddFileTypes<TOptions>(Seq<FileFormat<TOptions>> formats, IPlugInSink sink) where TOptions : class =>
        from list in IO.lift(static () => new FileTypeList())
        from added in IO.lift(() => Callbacks.Each(
            formats,
            format => list.AddFileType(
                RowText.Localize(format.Description, sink).Local,
                format.Extensions.Map(static extension => (string)extension),
                format.Options.IsSome) >= 0,
            nameof(FileTypeList.AddFileType)))
        select list;

    // --- [RUN]
    public static IO<A> Run<TOptions, A>(Seq<FileFormat<TOptions>> formats, int index, string path, RhinoDoc doc, TOptions options, A done, A cancelled, string member) where TOptions : class =>
        (from format in IO.lift(Conversions.Within(index, formats.Count, member).Map(_ => formats[index]))
         from ran in format.Run(path, doc, options)
         select done)
            .Catch(static error => error.Is(Errors.Cancelled), _ => IO.pure(cancelled));

    // --- [OPTIONS]
    public static IO<Unit> DisplayOptionsDialog<TOptions>(IPlugInViews plugIn, Seq<FileFormat<TOptions>> formats, string description, string extension) where TOptions : class =>
        (from dialog in IO.lift(() => formats
             .Find(format =>
                 string.Equals(RowText.Localize(format.Description, plugIn).Local, description, StringComparison.Ordinal)
                 && string.Equals(((string)format.Extensions.Head)[1..], extension, StringComparison.Ordinal))
             .Bind(static format => format.Options)
             .ToFin(new NotFound("file format", $"{description} {extension}")))
         from shown in Documents.WithDocument(new DocumentSource.Active(), document => Showing.ShowSemiModal(plugIn, dialog, document))
         select unit)
            .Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(unit));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class DefinedPlugIn(PlugInDefinition definition) : PlugIn, IDefinedPlugIn {
    // --- [LIFECYCLE]
    private readonly PlugInLifetime lifetime = new(definition, Seq<View>());

    PlugInLifetime IDefinedPlugIn.Lifetime => lifetime;

    public sealed override PlugInLoadTime LoadTime => lifetime.LoadTime(base.LoadTime);

    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) => lifetime.Load(this, GetLicense, CallbackSite.Of(this), ref errorMessage);

    protected sealed override void CreateCommands() {
        base.CreateCommands();
        lifetime.Register(RegisterCommand, CallbackSite.Of(this));
    }

    protected sealed override void OnShutdown() => lifetime.Shutdown(CallbackSite.Of(this));

    protected sealed override void ResetMessageBoxes() => lifetime.ResetMessageBoxes(this, CallbackSite.Of(this));

    // --- [IDENTITY]
    public sealed override object? GetPlugInObject() => lifetime.Definition.Published.ValueUnsafe();

    // --- [HELP]
    public sealed override bool AddToHelpMenu => lifetime.Definition.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => lifetime.DisplayHelp(CallbackSite.Of(this));

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) => Callbacks.Run(ViewRegistration.OptionsDialogPages(this, pages), CallbackSite.Of(this));

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) => Callbacks.Run(ViewRegistration.DocumentPropertiesDialogPages(this, doc, pages), CallbackSite.Of(this));

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) => Callbacks.Run(ViewRegistration.ObjectPropertiesPages(this, collection), CallbackSite.Of(this));

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => DocumentArchive.ShouldWrite(lifetime.Definition.DocumentFrames, options, CallbackSite.Of(this));

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) => DocumentArchive.Write(lifetime.Definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) => DocumentArchive.Read(lifetime.Definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));
}

public abstract class DefinedImportPlugIn(PlugInDefinition definition, Seq<FileFormat<FileReadOptions>> formats) : FileImportPlugIn, IDefinedPlugIn {
    // --- [LIFECYCLE]
    private readonly Seq<FileFormat<FileReadOptions>> formats = formats;
    private readonly PlugInLifetime lifetime = new(definition, formats.Choose(static format => format.Options.Map<View>(static dialog => dialog)));

    PlugInLifetime IDefinedPlugIn.Lifetime => lifetime;

    public sealed override PlugInLoadTime LoadTime => lifetime.LoadTime(base.LoadTime);

    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) => lifetime.Load(this, GetLicense, CallbackSite.Of(this), ref errorMessage);

    protected sealed override void CreateCommands() {
        base.CreateCommands();
        lifetime.Register(RegisterCommand, CallbackSite.Of(this));
    }

    protected sealed override void OnShutdown() => lifetime.Shutdown(CallbackSite.Of(this));

    protected sealed override void ResetMessageBoxes() => lifetime.ResetMessageBoxes(this, CallbackSite.Of(this));

    // --- [IDENTITY]
    public sealed override object? GetPlugInObject() => lifetime.Definition.Published.ValueUnsafe();

    // --- [HELP]
    public sealed override bool AddToHelpMenu => lifetime.Definition.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => lifetime.DisplayHelp(CallbackSite.Of(this));

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) => Callbacks.Run(ViewRegistration.OptionsDialogPages(this, pages), CallbackSite.Of(this));

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) => Callbacks.Run(ViewRegistration.DocumentPropertiesDialogPages(this, doc, pages), CallbackSite.Of(this));

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) => Callbacks.Run(ViewRegistration.ObjectPropertiesPages(this, collection), CallbackSite.Of(this));

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => DocumentArchive.ShouldWrite(lifetime.Definition.DocumentFrames, options, CallbackSite.Of(this));

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) => DocumentArchive.Write(lifetime.Definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) => DocumentArchive.Read(lifetime.Definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));

    // --- [FILES]
    protected sealed override FileTypeList AddFileTypes(FileReadOptions options) =>
        Callbacks.Answer(FileFormats.AddFileTypes(formats, this), static () => new FileTypeList(), CallbackSite.Of(this));

    protected sealed override bool ReadFile(string filename, int index, RhinoDoc? doc, FileReadOptions options) =>
        Callbacks.Answer(
            IO.lift(Refused.UnlessFound(doc, nameof(RhinoDoc.FromRuntimeSerialNumber)))
                .Bind(document => FileFormats.Run(formats, index, filename, document, options, done: true, cancelled: false, nameof(ReadFile))),
            static () => false,
            CallbackSite.Of(this));

    protected sealed override void DisplayOptionsDialog(nint parent, string description, string extension) => Callbacks.Run(FileFormats.DisplayOptionsDialog(this, formats, description, extension), CallbackSite.Of(this));
}

public abstract class DefinedExportPlugIn(PlugInDefinition definition, Seq<FileFormat<FileWriteOptions>> formats) : FileExportPlugIn, IDefinedPlugIn {
    // --- [LIFECYCLE]
    private readonly Seq<FileFormat<FileWriteOptions>> formats = formats;
    private readonly PlugInLifetime lifetime = new(definition, formats.Choose(static format => format.Options.Map<View>(static dialog => dialog)));

    PlugInLifetime IDefinedPlugIn.Lifetime => lifetime;

    public sealed override PlugInLoadTime LoadTime => lifetime.LoadTime(base.LoadTime);

    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) => lifetime.Load(this, GetLicense, CallbackSite.Of(this), ref errorMessage);

    protected sealed override void CreateCommands() {
        base.CreateCommands();
        lifetime.Register(RegisterCommand, CallbackSite.Of(this));
    }

    protected sealed override void OnShutdown() => lifetime.Shutdown(CallbackSite.Of(this));

    protected sealed override void ResetMessageBoxes() => lifetime.ResetMessageBoxes(this, CallbackSite.Of(this));

    // --- [IDENTITY]
    public sealed override object? GetPlugInObject() => lifetime.Definition.Published.ValueUnsafe();

    // --- [HELP]
    public sealed override bool AddToHelpMenu => lifetime.Definition.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => lifetime.DisplayHelp(CallbackSite.Of(this));

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) => Callbacks.Run(ViewRegistration.OptionsDialogPages(this, pages), CallbackSite.Of(this));

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) => Callbacks.Run(ViewRegistration.DocumentPropertiesDialogPages(this, doc, pages), CallbackSite.Of(this));

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) => Callbacks.Run(ViewRegistration.ObjectPropertiesPages(this, collection), CallbackSite.Of(this));

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => DocumentArchive.ShouldWrite(lifetime.Definition.DocumentFrames, options, CallbackSite.Of(this));

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) => DocumentArchive.Write(lifetime.Definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) => DocumentArchive.Read(lifetime.Definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));

    // --- [FILES]
    protected sealed override FileTypeList AddFileTypes(FileWriteOptions options) =>
        Callbacks.Answer(FileFormats.AddFileTypes(formats, this), static () => new FileTypeList(), CallbackSite.Of(this));

    protected sealed override WriteFileResult WriteFile(string filename, int index, RhinoDoc? doc, FileWriteOptions options) =>
        Callbacks.Answer(
            IO.lift(Refused.UnlessFound(doc, nameof(RhinoDoc.FromRuntimeSerialNumber)))
                .Bind(document => FileFormats.Run(formats, index, filename, document, options, WriteFileResult.Success, WriteFileResult.Cancel, nameof(WriteFile))),
            static () => WriteFileResult.Failure,
            CallbackSite.Of(this));

    protected sealed override void DisplayOptionsDialog(nint parent, string description, string extension) => Callbacks.Run(FileFormats.DisplayOptionsDialog(this, formats, description, extension), CallbackSite.Of(this));
}

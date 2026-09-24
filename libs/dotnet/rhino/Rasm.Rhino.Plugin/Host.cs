using System.Drawing;
using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.Collections;
using Rhino.Commands;
using Rhino.FileIO;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.UI;

namespace Rasm.Rhino.Plugin;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LicenseRequest {
    public sealed record ByCapability(LicenseCapabilities Capabilities, Option<string> TextMask) : LicenseRequest;

    public sealed record ByBuild(LicenseBuildType Build) : LicenseRequest;

    public sealed record AskUser(LicenseBuildType Build, bool StandAlone, Option<string> TextMask, Option<object> Parent) : LicenseRequest;
}

public sealed record ArchiveCallbacks(
    ChunkFrame Frame,
    Func<int, int, bool> SupportsVersion,
    Func<FileWriteOptions, IO<bool>> ShouldWrite,
    Func<RhinoDoc, FileWriteOptions, IO<ArchivableDictionary>> Write,
    Func<RhinoDoc, FileReadOptions, ChunkFrame, ArchivableDictionary, IO<Unit>> Read);

public sealed record PlugInCallbacks {
    public Action<Error> Reject { get; init; } = ErrorOps.Report;

    public Option<IO<Unit>> Load { get; init; }

    public Option<IO<Seq<Command>>> Commands { get; init; }

    public Option<IO<Unit>> Shutdown { get; init; }

    public Option<IO<Unit>> ResetMessageBoxes { get; init; }

    public Option<Func<nint, IO<Unit>>> Help { get; init; }

    public Option<object> PlugInObject { get; init; }

    public Option<IO<Seq<OptionsDialogPage>>> OptionsPages { get; init; }

    public Option<Func<RhinoDoc, IO<Seq<OptionsDialogPage>>>> DocumentPages { get; init; }

    public Option<Func<RhinoDoc, IO<Seq<ObjectPropertiesPage>>>> ObjectPages { get; init; }

    public Option<ArchiveCallbacks> Archive { get; init; }
}

public sealed record ImportCallbacks(Func<FileReadOptions, IO<FileTypeList>> AddFileTypes, Func<string, int, RhinoDoc, FileReadOptions, IO<Unit>> ReadFile);

public sealed record ExportCallbacks(Func<FileWriteOptions, IO<FileTypeList>> AddFileTypes, Func<string, int, RhinoDoc, FileWriteOptions, IO<Unit>> WriteFile);

public sealed record RenderCallbacks(Func<RhinoDoc, RunMode, bool, IO<Unit>> Render) {
    public Option<Func<RenderPanels, IO<Unit>>> RegisterRenderPanels { get; init; }

    public Option<Func<RenderTabs, IO<Unit>>> RegisterRenderTabs { get; init; }

    public Option<IO<Seq<RenderContentSerializer>>> RenderContentSerializers { get; init; }
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record CommandRefused(string EnglishName, Guid Id) : Expected("RegisterCommand refused {EnglishName} {Id}", ErrorOps.Code<CommandRefused>()) {
    public static Fin<Unit> Unless(bool registered, Command command) => registered ? unit : new CommandRefused(command.EnglishName, command.Id);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class PlugInOverrides {
    // --- [LIFECYCLE]
    internal static LoadReturnCode OnLoad(PlugInCallbacks callbacks, ref string errorMessage) {
        (LoadReturnCode code, Option<string> message) = callbacks.Load.IfNone(IO.pure(unit)).RunSafe().Match(
            Succ: static _ => (LoadReturnCode.Success, Option<string>.None),
            Fail: static error => error.IsType<Canceled>()
                ? (LoadReturnCode.ErrorNoDialog, Option<string>.None)
                : (LoadReturnCode.ErrorShowDialog, Some(ErrorOps.Localize(error))));
        errorMessage = message.IfNone(errorMessage);
        return code;
    }

    internal static void CreateCommands(PlugInCallbacks callbacks, Func<Command, bool> register) =>
        _ = Answers.Answer(
            from commands in callbacks.Commands.IfNone(IO.pure(Seq<Command>()))
            from registered in IO.lift(() => commands.TraverseM(command => CommandRefused.Unless(register(command), command)).As())
            select registered,
            callbacks.Reject,
            Seq<Unit>());

    internal static void Deliver(PlugInCallbacks callbacks, Option<IO<Unit>> callback) =>
        _ = Answers.Answer(callback, callbacks.Reject, static () => unit);

    // --- [QUERIES]
    internal static bool DisplayHelp(PlugInCallbacks callbacks, nint windowHandle) =>
        Answers.Succeeded(callbacks.Help.Map(help => help(windowHandle)), callbacks.Reject, static () => false);

    // --- [PAGES]
    internal static void OptionsDialogPages(PlugInCallbacks callbacks, List<OptionsDialogPage> pages) =>
        AddPages(callbacks, callbacks.OptionsPages, pages.Add);

    internal static void DocumentPropertiesDialogPages(PlugInCallbacks callbacks, RhinoDoc doc, List<OptionsDialogPage> pages) =>
        AddPages(callbacks, callbacks.DocumentPages.Map(build => build(doc)), pages.Add);

    internal static void ObjectPropertiesPages(PlugInCallbacks callbacks, ObjectPropertiesPageCollection collection) =>
        AddPages(
            callbacks,
            callbacks.ObjectPages.Map(build =>
                from doc in IO.lift(() => Missing.Unless(collection.Document, nameof(ObjectPropertiesPageCollection.Document)))
                from pages in build(doc)
                select pages),
            collection.Add);

    private static void AddPages<TPage>(PlugInCallbacks callbacks, Option<IO<Seq<TPage>>> pages, Action<TPage> add) =>
        Answers.Answer(pages, callbacks.Reject, static () => Seq<TPage>()).Iter(add);

    // --- [ARCHIVE]
    internal static bool ShouldCallWriteDocument(PlugInCallbacks callbacks, FileWriteOptions options) =>
        Answers.Answer(callbacks.Archive.Map(archive => archive.ShouldWrite(options)), callbacks.Reject, static () => false);

    internal static void WriteDocument(PlugInCallbacks callbacks, RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) =>
        callbacks.Archive.Iter(archived => archive.WriteErrorOccured = Answers.Answer(
            archived.Write(doc, options).Bind(dictionary => AttachedData.WriteChunk(archive, archived.Frame, dictionary)).Map(static _ => false),
            callbacks.Reject,
            fallback: true));

    internal static void ReadDocument(PlugInCallbacks callbacks, RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) =>
        _ = Answers.Answer(
            callbacks.Archive.Map(archived => AttachedData.ReadChunk(archive, archived.Frame.TypeCode, archived.SupportsVersion).Bind(read => archived.Read(doc, options, read.Frame, read.Dictionary))),
            callbacks.Reject,
            static () => unit);

    // --- [LICENSE]
    internal static IO<Unit> RequestLicense(
        PlugInCallbacks callbacks,
        LicenseRequest request,
        Func<string, IO<LicenseData>> validate,
        Option<Func<Option<LeaseState>, IO<Option<Icon>>>> leaseChanged,
        (Func<LicenseCapabilities, string?, ValidateProductKeyDelegate, OnLeaseChangedDelegate?, bool> ByCapability,
            Func<LicenseBuildType, ValidateProductKeyDelegate, OnLeaseChangedDelegate?, bool> ByBuild,
            Func<LicenseBuildType, bool, string?, object?, ValidateProductKeyDelegate, OnLeaseChangedDelegate?, bool> AskUser) host) =>
        IO.lift(() => request.Switch(
            (Host: host, Validator: PlugInRegistry.Validator(validate), Handler: leaseChanged.Map(changed => PlugInRegistry.LeaseChangedHandler(changed, callbacks.Reject)).ValueUnsafe()),
            byCapability: static (state, capability) => Refused.Unless(
                state.Host.ByCapability(capability.Capabilities, capability.TextMask.ValueUnsafe(), state.Validator, state.Handler),
                nameof(LicenseRequest.ByCapability)),
            byBuild: static (state, build) => Refused.Unless(state.Host.ByBuild(build.Build, state.Validator, state.Handler), nameof(LicenseRequest.ByBuild)),
            askUser: static (state, askUser) => Refused.Unless(
                state.Host.AskUser(askUser.Build, askUser.StandAlone, askUser.TextMask.ValueUnsafe(), askUser.Parent.ValueUnsafe(), state.Validator, state.Handler),
                nameof(LicenseRequest.AskUser))));

    // --- [FILES]
    internal static FileTypeList AddFileTypes(PlugInCallbacks callbacks, IO<FileTypeList> types) =>
        Answers.Answer(types, callbacks.Reject, new FileTypeList());

    internal static WriteFileResult WriteFile(PlugInCallbacks callbacks, IO<Unit> write) =>
        Answers.Answer(
            write.Map(static _ => WriteFileResult.Success).IfFail(static error => error.IsType<Canceled>() ? IO.pure(WriteFileResult.Cancel) : IO.fail<WriteFileResult>(error)),
            callbacks.Reject,
            WriteFileResult.Failure);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class CallbackPlugIn : PlugIn {
    // --- [CALLBACKS]
    protected abstract PlugInCallbacks Callbacks { get; }

    // --- [LIFECYCLE]
    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) => PlugInOverrides.OnLoad(Callbacks, ref errorMessage);

    protected sealed override void CreateCommands() {
        base.CreateCommands();
        PlugInOverrides.CreateCommands(Callbacks, RegisterCommand);
    }

    protected sealed override void OnShutdown() => PlugInOverrides.Deliver(Callbacks, Callbacks.Shutdown);

    protected sealed override void ResetMessageBoxes() => PlugInOverrides.Deliver(Callbacks, Callbacks.ResetMessageBoxes);

    // --- [QUERIES]
    public sealed override bool AddToHelpMenu => Callbacks.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => PlugInOverrides.DisplayHelp(Callbacks, windowHandle);

    public sealed override object? GetPlugInObject() => Callbacks.PlugInObject.ValueUnsafe();

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) => PlugInOverrides.OptionsDialogPages(Callbacks, pages);

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) => PlugInOverrides.DocumentPropertiesDialogPages(Callbacks, doc, pages);

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) => PlugInOverrides.ObjectPropertiesPages(Callbacks, collection);

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => PlugInOverrides.ShouldCallWriteDocument(Callbacks, options);

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) => PlugInOverrides.WriteDocument(Callbacks, doc, archive, options);

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) => PlugInOverrides.ReadDocument(Callbacks, doc, archive, options);

    // --- [LICENSE]
    protected IO<Unit> RequestLicense(LicenseRequest request, Func<string, IO<LicenseData>> validate, Option<Func<Option<LeaseState>, IO<Option<Icon>>>> leaseChanged) =>
        PlugInOverrides.RequestLicense(Callbacks, request, validate, leaseChanged, (GetLicense, GetLicense, AskUserForLicense));
}

public abstract class CallbackImportPlugIn : FileImportPlugIn {
    // --- [CALLBACKS]
    protected abstract PlugInCallbacks Callbacks { get; }

    protected abstract ImportCallbacks Import { get; }

    // --- [LIFECYCLE]
    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) => PlugInOverrides.OnLoad(Callbacks, ref errorMessage);

    protected sealed override void CreateCommands() {
        base.CreateCommands();
        PlugInOverrides.CreateCommands(Callbacks, RegisterCommand);
    }

    protected sealed override void OnShutdown() => PlugInOverrides.Deliver(Callbacks, Callbacks.Shutdown);

    protected sealed override void ResetMessageBoxes() => PlugInOverrides.Deliver(Callbacks, Callbacks.ResetMessageBoxes);

    // --- [QUERIES]
    public sealed override bool AddToHelpMenu => Callbacks.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => PlugInOverrides.DisplayHelp(Callbacks, windowHandle);

    public sealed override object? GetPlugInObject() => Callbacks.PlugInObject.ValueUnsafe();

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) => PlugInOverrides.OptionsDialogPages(Callbacks, pages);

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) => PlugInOverrides.DocumentPropertiesDialogPages(Callbacks, doc, pages);

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) => PlugInOverrides.ObjectPropertiesPages(Callbacks, collection);

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => PlugInOverrides.ShouldCallWriteDocument(Callbacks, options);

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) => PlugInOverrides.WriteDocument(Callbacks, doc, archive, options);

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) => PlugInOverrides.ReadDocument(Callbacks, doc, archive, options);

    // --- [LICENSE]
    protected IO<Unit> RequestLicense(LicenseRequest request, Func<string, IO<LicenseData>> validate, Option<Func<Option<LeaseState>, IO<Option<Icon>>>> leaseChanged) =>
        PlugInOverrides.RequestLicense(Callbacks, request, validate, leaseChanged, (GetLicense, GetLicense, AskUserForLicense));

    // --- [FILES]
    protected sealed override FileTypeList AddFileTypes(FileReadOptions options) => PlugInOverrides.AddFileTypes(Callbacks, Import.AddFileTypes(options));

    protected sealed override bool ReadFile(string filename, int index, RhinoDoc doc, FileReadOptions options) =>
        Answers.Succeeded(Import.ReadFile(filename, index, doc, options), Callbacks.Reject);
}

public abstract class CallbackExportPlugIn : FileExportPlugIn {
    // --- [CALLBACKS]
    protected abstract PlugInCallbacks Callbacks { get; }

    protected abstract ExportCallbacks Export { get; }

    // --- [LIFECYCLE]
    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) => PlugInOverrides.OnLoad(Callbacks, ref errorMessage);

    protected sealed override void CreateCommands() {
        base.CreateCommands();
        PlugInOverrides.CreateCommands(Callbacks, RegisterCommand);
    }

    protected sealed override void OnShutdown() => PlugInOverrides.Deliver(Callbacks, Callbacks.Shutdown);

    protected sealed override void ResetMessageBoxes() => PlugInOverrides.Deliver(Callbacks, Callbacks.ResetMessageBoxes);

    // --- [QUERIES]
    public sealed override bool AddToHelpMenu => Callbacks.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => PlugInOverrides.DisplayHelp(Callbacks, windowHandle);

    public sealed override object? GetPlugInObject() => Callbacks.PlugInObject.ValueUnsafe();

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) => PlugInOverrides.OptionsDialogPages(Callbacks, pages);

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) => PlugInOverrides.DocumentPropertiesDialogPages(Callbacks, doc, pages);

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) => PlugInOverrides.ObjectPropertiesPages(Callbacks, collection);

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => PlugInOverrides.ShouldCallWriteDocument(Callbacks, options);

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) => PlugInOverrides.WriteDocument(Callbacks, doc, archive, options);

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) => PlugInOverrides.ReadDocument(Callbacks, doc, archive, options);

    // --- [LICENSE]
    protected IO<Unit> RequestLicense(LicenseRequest request, Func<string, IO<LicenseData>> validate, Option<Func<Option<LeaseState>, IO<Option<Icon>>>> leaseChanged) =>
        PlugInOverrides.RequestLicense(Callbacks, request, validate, leaseChanged, (GetLicense, GetLicense, AskUserForLicense));

    // --- [FILES]
    protected sealed override FileTypeList AddFileTypes(FileWriteOptions options) => PlugInOverrides.AddFileTypes(Callbacks, Export.AddFileTypes(options));

    protected sealed override WriteFileResult WriteFile(string filename, int index, RhinoDoc doc, FileWriteOptions options) =>
        PlugInOverrides.WriteFile(Callbacks, Export.WriteFile(filename, index, doc, options));
}

public abstract class CallbackRenderPlugIn : RenderPlugIn {
    // --- [CALLBACKS]
    protected abstract PlugInCallbacks Callbacks { get; }

    protected abstract RenderCallbacks Rendering { get; }

    // --- [LIFECYCLE]
    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) => PlugInOverrides.OnLoad(Callbacks, ref errorMessage);

    protected sealed override void CreateCommands() {
        base.CreateCommands();
        PlugInOverrides.CreateCommands(Callbacks, RegisterCommand);
    }

    protected sealed override void OnShutdown() => PlugInOverrides.Deliver(Callbacks, Callbacks.Shutdown);

    protected sealed override void ResetMessageBoxes() => PlugInOverrides.Deliver(Callbacks, Callbacks.ResetMessageBoxes);

    // --- [QUERIES]
    public sealed override bool AddToHelpMenu => Callbacks.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => PlugInOverrides.DisplayHelp(Callbacks, windowHandle);

    public sealed override object? GetPlugInObject() => Callbacks.PlugInObject.ValueUnsafe();

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) => PlugInOverrides.OptionsDialogPages(Callbacks, pages);

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) => PlugInOverrides.DocumentPropertiesDialogPages(Callbacks, doc, pages);

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) => PlugInOverrides.ObjectPropertiesPages(Callbacks, collection);

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => PlugInOverrides.ShouldCallWriteDocument(Callbacks, options);

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) => PlugInOverrides.WriteDocument(Callbacks, doc, archive, options);

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) => PlugInOverrides.ReadDocument(Callbacks, doc, archive, options);

    // --- [LICENSE]
    protected IO<Unit> RequestLicense(LicenseRequest request, Func<string, IO<LicenseData>> validate, Option<Func<Option<LeaseState>, IO<Option<Icon>>>> leaseChanged) =>
        PlugInOverrides.RequestLicense(Callbacks, request, validate, leaseChanged, (GetLicense, GetLicense, AskUserForLicense));

    // --- [RENDER]
    protected sealed override Result Render(RhinoDoc doc, RunMode mode, bool fastPreview) =>
        Answers.ToResult(Rendering.Render(doc, mode, fastPreview).RunSafe(), Callbacks.Reject);

    protected sealed override void RegisterRenderPanels(RenderPanels panels) => PlugInOverrides.Deliver(Callbacks, Rendering.RegisterRenderPanels.Map(register => register(panels)));

    protected sealed override void RegisterRenderTabs(RenderTabs tabs) => PlugInOverrides.Deliver(Callbacks, Rendering.RegisterRenderTabs.Map(register => register(tabs)));

    protected sealed override IEnumerable<RenderContentSerializer> RenderContentSerializers() =>
        Answers.Answer(Rendering.RenderContentSerializers, Callbacks.Reject, static () => Seq<RenderContentSerializer>());
}

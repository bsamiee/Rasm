using System.Drawing;
using Rasm.Rhino.Document;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.Collections;
using Rhino.Commands;
using Rhino.FileIO;
using Rhino.PlugIns;
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

    public Seq<Func<PlugIn, IO<Unit>>> Registrations { get; init; }

    public Seq<Func<Action<Error>, IO<IDisposable>>> Subscriptions { get; init; }

    public Seq<Command> Commands { get; init; }

    public Seq<IO<OptionsDialogPage>> OptionsPages { get; init; }

    public Seq<Func<RhinoDoc, IO<OptionsDialogPage>>> DocumentPages { get; init; }

    public Seq<Func<RhinoDoc, IO<ObjectPropertiesPage>>> ObjectPages { get; init; }

    public Option<ArchiveCallbacks> Archive { get; init; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class PlugInOverrides {
    // --- [LIFECYCLE]
    internal static LoadReturnCode OnLoad(PlugIn plugIn, PlugInCallbacks callbacks, ref string errorMessage, out Option<IDisposable> subscription) {
        Fin<IDisposable> loaded = callbacks.Registrations.TraverseM(register => register(plugIn)).As()
            .Bind(_ => Events.AttachAll(callbacks.Subscriptions.Map(subscribe => subscribe(callbacks.Reject)), callbacks.Reject))
            .RunSafe();
        subscription = loaded.ToOption();
        (LoadReturnCode code, errorMessage) = loaded.Match(
            Succ: static _ => (LoadReturnCode.Success, ""),
            Fail: static error => (error.IsType<Canceled>() ? LoadReturnCode.ErrorNoDialog : LoadReturnCode.ErrorShowDialog, ErrorOps.Localize(error)));
        return code;
    }

    internal static void CreateCommands(PlugInCallbacks callbacks, Func<Command, bool> register) =>
        _ = callbacks.Commands.Traverse(command => CommandRefused.Unless(register(command), command)).As().IfFail(callbacks.Reject);

    // --- [LICENSING]
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

    // --- [PAGES]
    internal static void OptionsDialogPages(PlugInCallbacks callbacks, List<OptionsDialogPage> pages) =>
        _ = Answered(callbacks.Reject, callbacks.OptionsPages).Iter(pages.Add);

    internal static void DocumentPropertiesDialogPages(PlugInCallbacks callbacks, RhinoDoc doc, List<OptionsDialogPage> pages) =>
        _ = Answered(callbacks.Reject, callbacks.DocumentPages.Map(page => page(doc))).Iter(pages.Add);

    internal static void ObjectPropertiesPages(PlugInCallbacks callbacks, ObjectPropertiesPageCollection collection) =>
        _ = Answered(
                callbacks.Reject,
                from doc in Optional(collection.Document).ToSeq()
                from page in callbacks.ObjectPages
                select page(doc))
            .Iter(collection.Add);

    private static Seq<TValue> Answered<TValue>(Action<Error> reject, Seq<IO<TValue>> rows) =>
        rows.Choose(row => Answers.Answer(row.Map(static value => Some(value)), reject, Option<TValue>.None)).Strict();

    // --- [ARCHIVE]
    internal static bool ShouldCallWriteDocument(PlugInCallbacks callbacks, FileWriteOptions options) =>
        Answers.Answer(callbacks.Archive.Map(archive => archive.ShouldWrite(options)), callbacks.Reject, static () => false);

    internal static void WriteDocument(PlugInCallbacks callbacks, RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) =>
        archive.WriteErrorOccured |= !Answers.Succeeded(
            callbacks.Archive.Map(archived => archived.Write(doc, options).Bind(dictionary => AttachedData.WriteChunk(archive, archived.Frame, dictionary))),
            callbacks.Reject,
            static () => true);

    internal static void ReadDocument(PlugInCallbacks callbacks, RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) =>
        archive.ReadErrorOccured |= !Answers.Succeeded(
            callbacks.Archive.Map(archived => AttachedData.ReadChunk(archive, archived.Frame.TypeCode, archived.SupportsVersion).Bind(read => archived.Read(doc, options, read.Frame, read.Dictionary))),
            callbacks.Reject,
            static () => true);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class CallbackPlugIn(PlugInCallbacks callbacks) : PlugIn {
    // --- [LIFECYCLE]
    private Option<IDisposable> subscription;

    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) => PlugInOverrides.OnLoad(this, callbacks, ref errorMessage, out subscription);

    protected sealed override void CreateCommands() {
        base.CreateCommands();
        PlugInOverrides.CreateCommands(callbacks, RegisterCommand);
    }

    protected sealed override void OnShutdown() => _ = subscription.Iter(static held => held.Dispose());

    // --- [LICENSING]
    protected IO<Unit> RequestLicense(LicenseRequest request, Func<string, IO<LicenseData>> validate, Option<Func<Option<LeaseState>, IO<Option<Icon>>>> leaseChanged) =>
        PlugInOverrides.RequestLicense(callbacks, request, validate, leaseChanged, (GetLicense, GetLicense, AskUserForLicense));

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) => PlugInOverrides.OptionsDialogPages(callbacks, pages);

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) => PlugInOverrides.DocumentPropertiesDialogPages(callbacks, doc, pages);

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) => PlugInOverrides.ObjectPropertiesPages(callbacks, collection);

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => PlugInOverrides.ShouldCallWriteDocument(callbacks, options);

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) => PlugInOverrides.WriteDocument(callbacks, doc, archive, options);

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) => PlugInOverrides.ReadDocument(callbacks, doc, archive, options);
}

public abstract class CallbackImportPlugIn(PlugInCallbacks callbacks) : FileImportPlugIn {
    // --- [LIFECYCLE]
    private Option<IDisposable> subscription;

    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) => PlugInOverrides.OnLoad(this, callbacks, ref errorMessage, out subscription);

    protected sealed override void CreateCommands() {
        base.CreateCommands();
        PlugInOverrides.CreateCommands(callbacks, RegisterCommand);
    }

    protected sealed override void OnShutdown() => _ = subscription.Iter(static held => held.Dispose());

    // --- [LICENSING]
    protected IO<Unit> RequestLicense(LicenseRequest request, Func<string, IO<LicenseData>> validate, Option<Func<Option<LeaseState>, IO<Option<Icon>>>> leaseChanged) =>
        PlugInOverrides.RequestLicense(callbacks, request, validate, leaseChanged, (GetLicense, GetLicense, AskUserForLicense));

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) => PlugInOverrides.OptionsDialogPages(callbacks, pages);

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) => PlugInOverrides.DocumentPropertiesDialogPages(callbacks, doc, pages);

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) => PlugInOverrides.ObjectPropertiesPages(callbacks, collection);

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => PlugInOverrides.ShouldCallWriteDocument(callbacks, options);

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) => PlugInOverrides.WriteDocument(callbacks, doc, archive, options);

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) => PlugInOverrides.ReadDocument(callbacks, doc, archive, options);
}

public abstract class CallbackExportPlugIn(PlugInCallbacks callbacks) : FileExportPlugIn {
    // --- [LIFECYCLE]
    private Option<IDisposable> subscription;

    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) => PlugInOverrides.OnLoad(this, callbacks, ref errorMessage, out subscription);

    protected sealed override void CreateCommands() {
        base.CreateCommands();
        PlugInOverrides.CreateCommands(callbacks, RegisterCommand);
    }

    protected sealed override void OnShutdown() => _ = subscription.Iter(static held => held.Dispose());

    // --- [LICENSING]
    protected IO<Unit> RequestLicense(LicenseRequest request, Func<string, IO<LicenseData>> validate, Option<Func<Option<LeaseState>, IO<Option<Icon>>>> leaseChanged) =>
        PlugInOverrides.RequestLicense(callbacks, request, validate, leaseChanged, (GetLicense, GetLicense, AskUserForLicense));

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) => PlugInOverrides.OptionsDialogPages(callbacks, pages);

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) => PlugInOverrides.DocumentPropertiesDialogPages(callbacks, doc, pages);

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) => PlugInOverrides.ObjectPropertiesPages(callbacks, collection);

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => PlugInOverrides.ShouldCallWriteDocument(callbacks, options);

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) => PlugInOverrides.WriteDocument(callbacks, doc, archive, options);

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) => PlugInOverrides.ReadDocument(callbacks, doc, archive, options);

    // --- [FILES]
    protected abstract IO<Unit> Write(string filename, int index, RhinoDoc doc, FileWriteOptions options);

    protected sealed override WriteFileResult WriteFile(string filename, int index, RhinoDoc doc, FileWriteOptions options) =>
        Answers.Answer(
            Write(filename, index, doc, options)
                .Map(static _ => WriteFileResult.Success)
                .IfFail(static error => error.IsType<Canceled>() ? IO.pure(WriteFileResult.Cancel) : IO.fail<WriteFileResult>(error)),
            callbacks.Reject,
            WriteFileResult.Failure);
}

public abstract class CallbackRenderPlugIn(PlugInCallbacks callbacks) : RenderPlugIn {
    // --- [LIFECYCLE]
    private Option<IDisposable> subscription;

    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) => PlugInOverrides.OnLoad(this, callbacks, ref errorMessage, out subscription);

    protected sealed override void CreateCommands() {
        base.CreateCommands();
        PlugInOverrides.CreateCommands(callbacks, RegisterCommand);
    }

    protected sealed override void OnShutdown() => _ = subscription.Iter(static held => held.Dispose());

    // --- [LICENSING]
    protected IO<Unit> RequestLicense(LicenseRequest request, Func<string, IO<LicenseData>> validate, Option<Func<Option<LeaseState>, IO<Option<Icon>>>> leaseChanged) =>
        PlugInOverrides.RequestLicense(callbacks, request, validate, leaseChanged, (GetLicense, GetLicense, AskUserForLicense));

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) => PlugInOverrides.OptionsDialogPages(callbacks, pages);

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) => PlugInOverrides.DocumentPropertiesDialogPages(callbacks, doc, pages);

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) => PlugInOverrides.ObjectPropertiesPages(callbacks, collection);

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => PlugInOverrides.ShouldCallWriteDocument(callbacks, options);

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) => PlugInOverrides.WriteDocument(callbacks, doc, archive, options);

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) => PlugInOverrides.ReadDocument(callbacks, doc, archive, options);
}

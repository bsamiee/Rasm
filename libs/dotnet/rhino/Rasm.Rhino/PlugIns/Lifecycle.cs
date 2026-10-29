using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Eto;
using Rasm.Rhino.PlugIns.Licenses;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Commands;
using Rhino.FileIO;
using Rhino.PlugIns;
using Rhino.Runtime;

namespace Rasm.Rhino.PlugIns;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class PlugInBody {
    // --- [STATE]
    private readonly PlugInDefinition definition;
    private readonly ViewCatalog catalog;

    public PlugInBody(PlugIn host, PlugInDefinition definition, Seq<View> kindViews) {
        this.definition = definition;
        catalog = new ViewCatalog(definition, definition.Views.Concat(kindViews));
        _ = Session.RunSafe(Session.Bind(host, definition.Logging), CancellationToken.None).ThrowIfFail();
    }

    // --- [IDENTITY]
    public PlugInLoadTime LoadTime =>
        definition.LoadTime.IfNone(() =>
            catalog.Rows.Exists(static view => view is View.Panel) ? PlugInLoadTime.WhenNeededOrTabbedDockBar
            : catalog.Rows.Exists(static view => view is View.Page) ? PlugInLoadTime.WhenNeededOrOptionsDialog
            : PlugInLoadTime.WhenNeeded);

    // --- [LIFECYCLE]
    public LoadReturnCode Load(Func<LicenseCapabilities, string?, ValidateProductKeyDelegate, OnLeaseChangedDelegate, bool> getLicense, ref string errorMessage) {
        (LoadReturnCode code, errorMessage) = Session.RunSafe(
                Loading(getLicense).Scoped(Session.Scope).Map(static _ => (LoadReturnCode.Success, ""))
                    .Catch(static error => IO.lift(() => Session.Failed(error)) >> Session.Close
                        >> IO.pure(Session.Cancellation(error) ? (LoadReturnCode.ErrorNoDialog, "") : (LoadReturnCode.ErrorShowDialog, Session.Messages(error))))
                    .As(),
                CancellationToken.None)
            .IfFail(static error => (LoadReturnCode.ErrorShowDialog, Session.Messages(error)));
        return code;
    }

    private IO<Unit> Loading(Func<LicenseCapabilities, string?, ValidateProductKeyDelegate, OnLeaseChangedDelegate, bool> getLicense) =>
        from initialized in IO.lift(static () => AssemblyLoading.Initialized)
        from licensed in definition.License.TraverseM(license => Licensing.Request(license, getLicense)).As()
        from dependencies in PlugInRegistry.LoadPlugIns(definition.Dependencies)
        from views in ViewCatalog.Load(catalog)
        from rewrites in DocumentArchive.Rewrites
        from endpoints in IO.lift(() => definition.Endpoints.Iter(static endpoint => HostUtils.RegisterComputeEndpoint(endpoint.Path, endpoint.Type)))
        from loaded in definition.Load.TraverseM(identity).As()
        select unit;

    public Unit Commands(Func<Command, bool> register) =>
        Session.Run(Products.Each(definition.Commands.Map((row, index) =>
            from command in IO.lift(row.Realized)
            from registered in IO.lift(() => RhinoFault.Unless(register(command), RhinoReason.Declined, row.EnglishName, index))
            from history in when(row.History.Exists(static history => history.ReplayOnAttributeChange), IO.lift(() => {
                command.HistoryReplayOnObjectAttributeChange = true;
            })).As()
            select unit)));

    // --- [HELP]
    public bool DisplayHelp() => Session.Run(definition.Help.Map(static topic => topic.Show.Map(value: true)).IfNone(IO.pure(value: false)), failed: false);

    // --- [ARCHIVE]
    public bool ShouldCallWriteDocument(FileWriteOptions options) =>
        Session.Run(DocumentArchive.ShouldWrite(definition.Frames, options), failed: false);

    public Unit WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) =>
        Session.Run(DocumentArchive.Write(definition.Frames, doc, archive, options));

    public Unit ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) =>
        Session.Run(DocumentArchive.Read(definition.Frames, doc, archive, options));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class AssemblyLoading {
    // --- [INITIALIZATION]
    public static readonly Unit Initialized = Initialize(typeof(AssemblyLoading).Assembly);

    private static Unit Initialize(Assembly copy) {
        Platform.Instance.LoadAssembly(copy);
        return (from context in Optional(AssemblyLoadContext.GetLoadContext(copy))
                from folder in Optional(Path.GetDirectoryName(copy.Location))
                from executables in Optional(Path.GetDirectoryName(Environment.ProcessPath))
                select (Context: context, Folder: folder, Frameworks: Path.Join(executables, "..", "Frameworks")))
            .Iter(static held => held.Context.ResolvingUnmanagedDll += (importer, name) =>
                string.Equals(Path.GetDirectoryName(importer.Location), held.Folder, StringComparison.Ordinal) ? Library(held.Frameworks, name) : 0);
    }

    // --- [NATIVE_LIBRARIES]
    private static nint Library(string frameworks, string name) {
        string path = Path.Join(frameworks, $"lib{name}.dylib");
        using RiskyAction risky = new(path);
        return NativeLibrary.TryLoad(path, out nint handle) ? handle : 0;
    }
}

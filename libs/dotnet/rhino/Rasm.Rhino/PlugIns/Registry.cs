using Rasm.Rhino.Document.Files;
using Rhino.PlugIns;

namespace Rasm.Rhino.PlugIns;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PlugInRegistry {
    // --- [READS]
    public static IO<Seq<PlugInInfo>> InstalledPlugIns =>
        IO.lift(static () => toSeq(PlugIn.GetInstalledPlugIns().Keys).Strict()).Bind(static ids => ids.TraverseM(static id => Products.Of(() => PlugIn.GetPlugInInfo(id))).As());

    // --- [LOADS]
    public static IO<Guid> LoadPlugIn(InputPath path) =>
        IO.lift(Fin<Guid> () => PlugIn.LoadPlugIn(path, out Guid loaded) switch {
            LoadPlugInResult.Success or LoadPlugInResult.SuccessAlreadyLoaded => loaded,
            var result => new RhinoFault(path, RhinoReason.Unreadable, Inner: Error.New(result.ToString())),
        });

    public static IO<Unit> LoadPlugIns(Seq<Guid> ids) => Products.Each(ids.Map(LoadInstalled)).Map(unit);

    private static IO<Unit> LoadInstalled(Guid id) =>
        from info in Products.Of(() => PlugIn.GetPlugInInfo(id))
        from silent in IO.lift(() => {
            _ = info.IsLoadProtected(out bool loadSilently);
            return RhinoFault.Unless(loadSilently, RhinoReason.Unavailable, id);
        })
        from loaded in when(!info.IsLoaded, Products.Accepted(() => PlugIn.LoadPlugIn(id))).As()
        select unit;

    // --- [PROTECTION]
    public static IO<Unit> SetLoadProtection(Guid id, bool loadSilently) =>
        from info in Products.Of(() => PlugIn.GetPlugInInfo(id))
        from current in IO.lift(() => {
            _ = info.IsLoadProtected(out bool held);
            return held;
        })
        let changed = current != loadSilently
        from loaded in IO.lift(() => RhinoFault.Unless(!changed || !info.IsLoaded, RhinoReason.Unavailable, id))
        from written in when(changed, IO.lift(() => PlugIn.SetLoadProtection(id, loadSilently))).As()
        select unit;
}

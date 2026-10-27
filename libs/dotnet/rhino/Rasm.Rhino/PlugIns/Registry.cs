using Rasm.RhinoCommon.Document.Files;
using Rhino.PlugIns;
using Riok.Mapperly.Abstractions;

namespace Rasm.RhinoCommon.PlugIns;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PlugInState(
    Guid Id,
    Option<string> RegistryPath,
    Option<string> Name,
    Option<string> Description,
    Option<string> FileName,
    Option<string> Version,
    Option<string> Organization,
    Option<string> Address,
    Option<string> Country,
    Option<string> Email,
    Option<string> Phone,
    Option<string> Fax,
    Option<string> WebSite,
    Option<string> UpdateUrl,
    PlugInType PlugInType,
    PlugInLoadTime PlugInLoadTime,
    bool IsLoaded,
    bool ShipsWithRhino,
    bool IsDotNet,
    bool LoadSilently,
    Seq<FileTypeRow> FileTypes);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class RegistryMapper {
    [MapPropertyFromSource(nameof(PlugInState.LoadSilently), Use = nameof(@Exchange.LoadSilently))]
    [MapPropertyFromSource(nameof(PlugInState.FileTypes), Use = nameof(@Exchange.Rows))]
    internal static partial PlugInState ToState(PlugInInfo info);
}

public static class PlugInRegistry {
    // --- [READS]
    public static IO<Option<PlugInState>> ReadPlugIn(Guid id) => IO.lift(() => Optional(PlugIn.GetPlugInInfo(id)).Map(RegistryMapper.ToState));

    public static IO<Seq<PlugInState>> InstalledPlugIns =>
        IO.lift(static () => toSeq(PlugIn.GetInstalledPlugIns().Keys).Traverse(static id => Installed(id).Map(RegistryMapper.ToState)).As());

    private static Fin<PlugInInfo> Installed(Guid id) => Optional(PlugIn.GetPlugInInfo(id)).ToFin(new RhinoFault(id.ToString(), RhinoReason.Missing));

    // --- [LOADS]
    public static IO<Guid> LoadPlugIn(InputPath path) =>
        IO.lift(Fin<Guid> () => PlugIn.LoadPlugIn(path, out Guid loaded) switch {
            LoadPlugInResult.Success or LoadPlugInResult.SuccessAlreadyLoaded => loaded,
            var result => new RhinoFault(path, RhinoReason.Unreadable, Inner: Error.New(result.ToString())),
        });

    public static IO<Unit> LoadPlugIns(Seq<Guid> ids) =>
        IO.lift(() => Callbacks.Each(ids, static (id, _) => LoadInstalled(id)).Map(static _ => unit));

    private static Fin<Unit> LoadInstalled(Guid id) =>
        from info in Installed(id)
        from _ in guard<Error>(Exchange.LoadSilently(info), new RhinoFault(id.ToString(), RhinoReason.Disabled))
        from __ in RhinoFault.Unless(info.IsLoaded || PlugIn.LoadPlugIn(id))
        select unit;

    // --- [PROTECTION]
    public static IO<Unit> SetLoadProtection(Guid id, bool loadSilently) =>
        IO.lift(() =>
                from info in Installed(id)
                let changed = Exchange.LoadSilently(info) != loadSilently
                from _ in guard<Error>(!changed || !info.IsLoaded, new RhinoFault(id.ToString(), RhinoReason.Loaded))
                select changed)
            .Bind(changed => when(changed, IO.lift(() => PlugIn.SetLoadProtection(id, loadSilently))).As());
}

using Rasm.Rhino.Document.Files;
using Rhino.PlugIns;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Plugin.Hosting;

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
    public static IO<Option<PlugInState>> ReadPlugIn(Guid id) => IO.lift(() => State(id));

    public static IO<(Seq<Error> Fails, Seq<PlugInState> Succs)> InstalledPlugIns =>
        IO.lift(static () => toSeq(PlugIn.GetInstalledPlugIns().Keys))
            .Bind(static ids => ids.Map<K<IO, PlugInState>>(static id => IO.lift(() => State(id).ToFin(new NotInstalled(id)))).PartitionFallible().As());

    private static Option<PlugInState> State(Guid id) => Optional(PlugIn.GetPlugInInfo(id)).Map(RegistryMapper.ToState);

    // --- [LOADS]
    public static IO<Guid> LoadPlugIn(string path) =>
        IO.lift(() => Exchange.ExistingPath(path))
            .Bind(static existing => IO.lift(Fin<Guid> () => PlugIn.LoadPlugIn(existing, out Guid loaded) switch {
                LoadPlugInResult.Success or LoadPlugInResult.SuccessAlreadyLoaded => loaded,
                LoadPlugInResult.ErrorUnknown => new LoadRefused(existing),
                LoadPlugInResult.NotRhinoPlugIn => new NotRhinoPlugIn(existing),
            }));

    public static IO<Unit> LoadPlugIns(Seq<Guid> ids) =>
        IO.lift(() => Callbacks.Each(ids, static (id, _) => LoadInstalled(id)).Map(static _ => unit));

    private static Fin<Unit> LoadInstalled(Guid id) =>
        from info in Optional(PlugIn.GetPlugInInfo(id)).ToFin(new NotInstalled(id))
        from _ in guard<Error>(Exchange.LoadSilently(info), new LoadDisabled(id))
        from __ in guard<Error>(info.IsLoaded || PlugIn.LoadPlugIn(id), new LoadFailed(id))
        select unit;

    // --- [PROTECTION]
    public static IO<Unit> SetLoadProtection(Guid id, bool loadSilently) =>
        IO.lift(() =>
                from info in Optional(PlugIn.GetPlugInInfo(id)).ToFin(new NotInstalled(id))
                let changed = Exchange.LoadSilently(info) != loadSilently
                from _ in guard<Error>(!changed || !info.IsLoaded, new RestartRequired(id))
                select changed)
            .Bind(changed => when(changed, IO.lift(() => PlugIn.SetLoadProtection(id, loadSilently))).As());
}

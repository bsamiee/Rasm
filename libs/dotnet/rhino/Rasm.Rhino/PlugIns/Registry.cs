using Rasm.Rhino.Document.Files;
using Rhino.PlugIns;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.PlugIns;

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

    public static IO<(Seq<Error> Fails, Seq<PlugInState> Succs)> InstalledPlugIns =>
        IO.lift(static () => toSeq(PlugIn.GetInstalledPlugIns().Keys))
            .Bind(static ids => ids.Map<K<IO, PlugInState>>(static id => IO.lift(() => Installed(id).Map(RegistryMapper.ToState))).PartitionFallible().As());

    private static Fin<PlugInInfo> Installed(Guid id) => Optional(PlugIn.GetPlugInInfo(id)).ToFin(new NotFound("installed plug-in", $"{id}"));

    // --- [LOADS]
    public static IO<Guid> LoadPlugIn(string path) =>
        IO.lift(() => Exchange.ExistingPath(path))
            .Bind(static existing => IO.lift(Fin<Guid> () => PlugIn.LoadPlugIn(existing, out Guid loaded) switch {
                LoadPlugInResult.Success or LoadPlugInResult.SuccessAlreadyLoaded => loaded,
                var result => new FileRefused(existing, $"load failed with {result}"),
            }));

    public static IO<Unit> LoadPlugIns(Seq<Guid> ids) =>
        IO.lift(() => Callbacks.Each(ids, static (id, _) => LoadInstalled(id)).Map(static _ => unit));

    private static Fin<Unit> LoadInstalled(Guid id) =>
        from info in Installed(id)
        from _ in guard(Exchange.LoadSilently(info), Error.New($"plug-in {id} is disabled"))
        from __ in guard(info.IsLoaded || PlugIn.LoadPlugIn(id), Error.New($"plug-in {id} failed to load"))
        select unit;

    // --- [PROTECTION]
    public static IO<Unit> SetLoadProtection(Guid id, bool loadSilently) =>
        IO.lift(() =>
                from info in Installed(id)
                let changed = Exchange.LoadSilently(info) != loadSilently
                from _ in guard(!changed || !info.IsLoaded, Error.New($"load protection change needs unloaded plug-in {id}"))
                select changed)
            .Bind(changed => when(changed, IO.lift(() => PlugIn.SetLoadProtection(id, loadSilently))).As());
}

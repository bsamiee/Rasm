using Rasm.Rhino.Persistence;
using Rhino.PlugIns;

namespace Rasm.Rhino.Render.Settings;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record CyclesSetting<T>(string Key, bool Document) where T : notnull {
    public IO<Option<T>> Default => Cycles.Node.Bind(node => node.Default<T>(Key));

    public IO<Option<T>> Effective(RenderSettingsSource source) =>
        from stored in Document
            ? RenderSettingsSources.Read(source, scope => IO.lift(() => ArchivableDictionaries.Find<T>(scope.Settings.UserDictionary, Key)))
            : IO.pure(Option<T>.None)
        from value in stored.Match(Some: static held => IO.pure(Some(held)), None: Applied.Bind(applied => applied.Match(Some: static held => IO.pure(Some(held)), None: Default)))
        select value;

    public Preference<T> Stored(RenderSettingsSource source, T fallback) => Preferences.Dictionary(RenderSettingsSources.Dictionary(source), Key, fallback);

    public IO<Preference<T>> Application(T fallback) => Cycles.Node.Map(node => Preferences.Setting(node, Key, fallback));

    private IO<Option<T>> Applied => Cycles.Node.Bind(static node => node.Existing).Map(held => held.Bind(settings => SettingOverloads<T>.Read(settings, Key, [])));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Cycles {
    public static IO<Guid> PlugInId => Installed("Rhino Render");

    public static IO<SettingsNode> Node => Installed("RhinoCycles").Map(static id => new SettingsNode(new SettingsRoot.PlugInNode(id)));

    private static IO<Guid> Installed(string name) => IO.lift(() => Conversions.Present(PlugIn.IdFromName(name)).ToFin(new RhinoFault(name, RhinoReason.Missing)));
}

using Grasshopper2;
using Grasshopper2.Framework;
using Grasshopper2.UI.Icon;
using Rasm.Rhino;
using Rasm.Rhino.PlugIns;

namespace Rasm.Grasshopper;

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class DefinedGrasshopperPlugin : Plugin, IGrasshopperPlugin {
    private readonly GrasshopperPluginDefinition definition;

    protected DefinedGrasshopperPlugin(GrasshopperPluginDefinition definition) {
        this.definition = definition;
        SettingsFile = SettingsFile.InDefaultFolder(Path.GetFileNameWithoutExtension(Location));
        Session = new(GrasshopperStores.Saved(SettingsFile), definition.Logging);
        GrasshopperPlugins.Register(this);
    }

    public SettingsFile SettingsFile { get; }

    public PlugInSession Session { get; }

    public Plugin Plugin => this;

    public sealed override IIcon? Icon => GrasshopperPlugins.Icon(this, CallbackSite.Of(this)).ValueUnsafe();

    protected sealed override void HarvestCustomTypes() => GrasshopperPlugins.Harvest(this, definition, CallbackSite.Of(this));

    public sealed override void OnLoaded() => GrasshopperPlugins.Loaded(this, definition, CallbackSite.Of(this));
}

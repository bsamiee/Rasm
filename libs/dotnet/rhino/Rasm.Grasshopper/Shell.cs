using Grasshopper2;
using Grasshopper2.Framework;
using Grasshopper2.UI.Icon;
using Rasm.RhinoCommon;
using Rasm.RhinoCommon.PlugIns;

namespace Rasm.Grasshopper;

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class DefinedGrasshopperPlugin : Plugin, IGrasshopperPlugin {
    private readonly GrasshopperPluginDefinition definition;

    protected DefinedGrasshopperPlugin(GrasshopperPluginDefinition definition) {
        this.definition = definition;
        SettingsFile = SettingsFile.InDefaultFolder(Path.GetFileNameWithoutExtension(Location));
        Session = new(definition.Logging);
        GrasshopperPlugins.Register(this);
    }

    public SettingsFile SettingsFile { get; }

    public PlugInSession Session { get; }

    public Plugin Plugin => this;

    public sealed override IIcon? Icon => this.Answer(CanvasIcons.PlugIn(this), Option<IIcon>.None).ValueUnsafe();

    protected sealed override void HarvestCustomTypes() => this.Run(GrasshopperPlugins.Harvest(this, definition));

    public sealed override void OnLoaded() => this.Run(GrasshopperPlugins.Loaded(this, definition));
}

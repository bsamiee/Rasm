using Rasm.Imaging.Filters.Optics;
using Rasm.Rhino;
using Rasm.Rhino.Render;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Queue;
using Rasm.Rhino.Render.Scenes;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Slots;
using Rasm.Rhino.UI.Views;

using Rasm.Rhino.Plugin;

using System.Runtime.InteropServices;
using Darkroom.Effects;
using Darkroom.Sections;
using Rhino.PlugIns;

[assembly: Guid("b5bac151-510d-4dc3-9f2d-14526e39d793")]

namespace Darkroom;

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class DarkroomPlugIn() : DefinedRenderingPlugIn(Definition, EffectChain.Kinds) {
    private const string FarmCommandName = "DarkroomRenderFarm";
    private const string FarmScript = $"_-{FarmCommandName}";

    private static readonly PlugInDefinition Definition = new() {
        LoadTime = PlugInLoadTime.AtStartup,
        Settings = DarkroomSettings.Settings,
        History = DarkroomSettings.ReadHistory,
        Commands = [
            static sink => new PanelCommand(sink, new Guid(0xd19b5cfb, 0x5262, 0x4f60, 0x86, 0xeb, 0xdd, 0x8e, 0x34, 0x98, 0x8d, 0xed), DarkroomPanel.Row) {
                Toolbar = Some((Option<Guid>.None, new Guid(0xd0a817a1, 0xdea9, 0x4e03, 0x89, 0xe3, 0x0d, 0x63, 0xd9, 0x9b, 0x5e, 0x51))),
            },
            static sink => new QueueJobCommand(sink, new Guid(0x4a60fdca, 0x6daf, 0x4fef, 0x8d, 0x5e, 0xfd, 0xc7, 0x71, 0x63, 0x9f, 0xee), "DarkroomRenderQueue",
                IPlugInRendering.Served(((IPlugInRendering)sink).QueueContext, nameof(IPlugInRendering.QueueContext))),
            static sink => new QueueFarmJobCommand(sink, new Guid(0x3857668a, 0x1b07, 0x4ae9, 0x97, 0x13, 0x63, 0x33, 0xb3, 0xca, 0x46, 0xee), FarmCommandName,
                IPlugInRendering.Served(((IPlugInRendering)sink).QueueContext, nameof(IPlugInRendering.QueueContext)), ((IPlugInLifetime)sink).Close),
            static sink => new FrameExportCommand(sink, new Guid(0x66a517fb, 0x68c5, 0x42c9, 0x86, 0xe4, 0x30, 0x01, 0x03, 0x63, 0xa4, 0x57), "DarkroomExportStMap",
                static (settings, extent) => EffectRegistry.State<RadialDistortion, LensDistortion>(settings, Some(RadialDistortion.Row.Default))
                    .Map(state => state.Placed.Bake(extent))),
            static sink => new LutExportCommand(sink, new Guid(0x09625191, 0xac85, 0x44eb, 0xa7, 0x67, 0x23, 0x07, 0x6c, 0x4c, 0x64, 0xcf), "DarkroomExportLut"),
        ],
        Views = [DarkroomPanel.Row, DarkroomPage.Row],
        Acquisitions = [
            RenderHistory.Register(RenderSetsSection.Row, static history => [
                SlotComparison.Register(history),
                RenderSets.Register(history, static sets => new RenderSetsSnapshots(sets)),
            ]),
            PreviewTiles.Register(Some(DarkroomSettings.Tiles), static _ => []),
            Queues.Register(DarkroomSettings.Queue, DarkroomPanel.Row, FarmScript, Seq(FarmScript)),
            SceneSources.Register<DitheringState>(static state => state with { Enabled = false }),
        ],
    };
}

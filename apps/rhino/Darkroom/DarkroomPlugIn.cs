using Darkroom.Effects;
using Rasm.Imaging.Filters.Optics;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Presets;
using Rasm.Rhino.Render.Queue;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Rows;
using Rasm.UI;
using Rhino.PlugIns;

[assembly: RuiFile<Darkroom.DarkroomPlugIn>]

namespace Darkroom;

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class DarkroomPlugIn() : DefinedPlugIn(new PlugInDefinition {
    LoadTime = PlugInLoadTime.AtStartup,
    History = HistorySection.Settings.Limits,
    Clock = TimeProvider.System,
    Logging = [Queues.Logging],
    Commands = [
        static sink => new PanelCommand(sink, new Guid(0xd19b5cfb, 0x5262, 0x4f60, 0x86, 0xeb, 0xdd, 0x8e, 0x34, 0x98, 0x8d, 0xed), DarkroomPanel.Row),
        static sink => new QueueJobCommand(sink, new Guid(0x4a60fdca, 0x6daf, 0x4fef, 0x8d, 0x5e, 0xfd, 0xc7, 0x71, 0x63, 0x9f, 0xee), "DarkroomRenderQueue"),
        static sink => new QueueFarmJobCommand(sink, new Guid(0x3857668a, 0x1b07, 0x4ae9, 0x97, 0x13, 0x63, 0x33, 0xb3, 0xca, 0x46, 0xee), "DarkroomRenderFarm"),
        static sink => new FrameExportCommand(sink, new Guid(0x66a517fb, 0x68c5, 0x42c9, 0x86, 0xe4, 0x30, 0x01, 0x03, 0x63, 0xa4, 0x57), ExportStMap.Name,
            static (settings, extent) => EffectRegistry.State<RadialDistortion, LensDistortion>(settings)
                .Map(state => state.Placed.Bake(extent))),
        static sink => new LutExportCommand(sink, new Guid(0x09625191, 0xac85, 0x44eb, 0xa7, 0x67, 0x23, 0x07, 0x6c, 0x4c, 0x64, 0xcf), ExportLut.Name),
    ],
    Views = [DarkroomPanel.Row, DarkroomPage.Row, OutputPage.Row],
    Acquisitions = [
        EffectRegistry.Register,
        CompareSection.Picker.Published,
        Framing.Extent.Attach,
        TargetRows.Shown.Attach,
        NamedEntry.Catalog.Attach,
        RenderHistory.Register(HistorySection.Row, static history => [RenderingComparison.Register(history), .. RenderSnapshots.Clients(history)]),
        PreviewTiles.Register(Some(LooksSection.Settings), static _ => []),
        Queues.Register(RenderQueueSection.Settings, DarkroomPanel.Row),
    ],
}), IRuiSource {
    private static readonly CommandFace ExportLut = new("DarkroomExportLut", "Export LUT...", "Bake the formation and grade chain into a 3D LUT file", None);
    private static readonly CommandFace ExportStMap = new("DarkroomExportStMap", "Export ST Map...", "Write the lens distortion of the active view as an ST map", None);

    public static RuiFile Rui { get; } = new([], [new MenuExtension(["Render", "Render Properties..."], [
        Seq(ExportLut, ExportStMap).Map(static face => (Command<RowRule, Macro>)new Command<RowRule, Macro>.Run(face, None, new Macro(face.Name, []))),
    ])]);
}

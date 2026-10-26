using Darkroom.Chain;
using Darkroom.Output;
using Darkroom.Queue;
using Darkroom.Review;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.PlugIns;
using Rhino.PlugIns;

namespace Darkroom;

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class DarkroomPlugIn() : DefinedPlugIn(new PlugInDefinition {
    LoadTime = PlugInLoadTime.AtStartup,
    History = static settings => PlugInSettings.Record<HistorySettings>(settings, Applied.Live).Read.Map(static held => held.IfNone(HistorySettings.Default).Limits),
    Clock = TimeProvider.System,
    Logging = [Dispatch.Logging],
    Commands = [
        static sink => new QueueJobCommand(sink),
        static sink => new QueueFarmJobCommand(sink),
        static sink => new StMapExportCommand(sink),
        static sink => new LutExportCommand(sink),
    ],
    Acquisitions = [
        RenderRuns.Register,
        ChainSnapshots.Register,
        RenderHistory.Register,
        RenderingComparison.Register,
        PreviewTiles.Register,
        NamedEntry.Catalog.Attach,
        Dispatch.Register,
    ],
});

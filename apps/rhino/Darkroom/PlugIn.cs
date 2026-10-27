using Darkroom.Chain;
using Darkroom.Output;
using Darkroom.Queue;
using Darkroom.Review;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.PlugIns;
using Rhino.PlugIns;

namespace Darkroom;

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class DarkroomPlugIn() : DefinedPlugIn(new PlugInDefinition {
    LoadTime = PlugInLoadTime.AtStartup,
    Clock = TimeProvider.System,
    Logging = [Dispatch.Logging],
    Commands = [
        static () => new QueueJobCommand(),
        static () => new QueueFarmJobCommand(),
        static () => new StMapExportCommand(),
        static () => new LutExportCommand(),
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

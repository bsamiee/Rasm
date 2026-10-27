using Rasm.Rhino.Persistence;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.DocObjects.SnapShots;

namespace Darkroom.Chain;

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class ChainSnapshots {
    private static readonly Guid Client = new(0x6affd2cd, 0x7fc8, 0x41fb, 0x8c, 0x42, 0x79, 0x67, 0x63, 0x65, 0x60, 0xb3);

    public static Func<IPlugInViews, IO<Unit>> Register { get; } = static views =>
        DefinedSnapshotClient.Register(new SnapshotDefinition(
            Client, views.PlugIn.Name, SnapShotsClient.RenderingCategory,
            Document: Some<SnapshotState<RhinoDoc>>(new SnapshotState<RhinoDoc>.Of<ChainState>(
                0x5244_5353u, ArchivableDictionaries.Json<ChainState>(nameof(ChainState), 4), ChainState.Read,
                static (doc, state) => new ChainEdit.Restore(state).Committed(doc).Map(unit)))))(views);
}

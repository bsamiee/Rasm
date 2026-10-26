using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.DocObjects.SnapShots;

namespace Darkroom.Chain;

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class ChainSnapshots {
    private static readonly Guid Client = new("6affd2cd-7fc8-41fb-8c42-7967636560b3");

    public static Func<IPlugInViews, Action<IDisposable>, IO<Unit>> Register { get; } = static (views, retain) =>
        DefinedSnapshotClient.Register(new SnapshotDefinition(
            Client, views.PlugIn.Name, SnapShotsClient.RenderingCategory,
            Document: Some<SnapshotState<RhinoDoc>>(new SnapshotState<RhinoDoc, ValueSet>(
                0x5244_5353u, ValueSet.Codec,
                static doc => ChainState.Read(doc).Map(static state => ValueSet.Fields(ChainState.Owner, state)),
                static (doc, values) => values.Recalled(ChainState.Owner, ChainState.Default).Bind(state => new ChainEdit.Restore(state).Committed(doc)).Map(unit)))))(views, retain);
}

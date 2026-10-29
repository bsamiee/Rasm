using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Objects;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Viewport;

namespace Rasm.Rhino;

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class NamedStates {
    public static Seq<NamedTable> Tables { get; } =
        [LayerStates.Table, NamedViews.Table, ConstructionPlanes.Table, NamedPositions.Table, Snapshots.Table, NamedSelections.Table, Markups.Table];

    public static ReadModel<Seq<NamedEntry>> Catalog { get; } = NamedTable.Catalog(Tables);
}

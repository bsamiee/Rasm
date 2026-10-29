using Rasm.Rhino.Blocks;
using Rhino;
using Rhino.DocObjects;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DocumentStatistics(
    HashMap<ObjectType, int> Kinds,
    HashMap<ActiveSpace, int> Spaces,
    HashMap<ObjectMode, int> Modes,
    HashMap<Guid, int> LayerUsage,
    HashMap<Option<Guid>, int> MaterialUsage,
    HashMap<ObjectMaterialSource, int> MaterialSources,
    ulong MemoryEstimate,
    HashMap<Guid, int> LayerDepths,
    DefinitionGraph DefinitionGraph,
    Option<long> FileLength) {
    // --- [MEASURES]
    public int ObjectCount => Kinds.Values.Fold(0, static (sum, count) => sum + count);

    public int LayerLevelCount => LayerDepths.Values.Fold(0, static (levels, depth) => int.Max(levels, depth + 1));

    public int PlacementCount => DefinitionGraph.Placements.Values.Fold(0, static (sum, placed) => sum + placed.Count);

    // --- [READS]
    public static IO<DocumentStatistics> Read(RhinoDoc doc) =>
        (IO.lift(() => Conversions.Rows(doc.Objects.GetObjectList(new ObjectEnumeratorSettings { HiddenObjects = true, IdefObjects = true, ReferenceObjects = true, IncludeLights = true }))
             .Map(found => (
                 Kind: found.ObjectType, found.Attributes.Space, found.Attributes.Mode,
                 Layer: doc.Layers[found.Attributes.LayerIndex].Id,
                 Material: Optional(found.GetMaterial(frontMaterial: true)).Bind(static material => Conversions.Present(material.RenderMaterialInstanceId)),
                 found.Attributes.MaterialSource, Memory: found.MemoryEstimate())).Strict()),
         IO.lift(() => toHashMap(Conversions.Rows(doc.Layers).Filter(static layer => !layer.IsDeleted)
             .Map(static layer => (layer.Id, Depth: layer.FullPath.Split(ModelComponent.NamePathSeparator).Length - 1)))),
         DefinitionGraph.Read(doc),
         IO.lift(() => Conversions.Present(doc.Path).Map(static path => new FileInfo(path)).Filter(static file => file.Exists).Map(static file => file.Length)))
        .Apply(static (rows, depths, graph, length) => new DocumentStatistics(
            Counted(rows, static row => row.Kind),
            Counted(rows, static row => row.Space),
            Counted(rows, static row => row.Mode),
            Counted(rows, static row => row.Layer),
            Counted(rows, static row => row.Material),
            Counted(rows, static row => row.MaterialSource),
            rows.Fold(0UL, static (sum, row) => sum + row.Memory),
            depths,
            graph,
            length))
        .As();

    private static HashMap<TKey, int> Counted<TRow, TKey>(Seq<TRow> rows, Func<TRow, TKey> key) where TKey : notnull =>
        toHashMap(rows.CountBy(key).Select(static counted => (counted.Key, counted.Value)));
}

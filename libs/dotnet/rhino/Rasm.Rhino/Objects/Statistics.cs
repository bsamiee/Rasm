using Rasm.Rhino.Blocks;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

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
    LayerTree LayerTree,
    DefinitionGraph DefinitionGraph,
    Option<long> FileLength) {
    // --- [MEASURES]
    public int ObjectCount => Kinds.Values.Fold(0, static (sum, count) => sum + count);

    public int LayerLevelCount => LayerTree.Depths.Values.Fold(0, static (levels, depth) => int.Max(levels, depth + 1));

    public int PlacementCount => DefinitionGraph.Placements.Values.Fold(0, static (sum, placed) => sum + placed.Count);

    // --- [READS]
    public static IO<DocumentStatistics> Read(RhinoDoc doc) =>
        (new ObjectTarget.Lookup(static table => table.GetObjectList(StatisticsMapper.Settings(include: true)))
             .Objects(doc)
             .Map(objects => objects.Map(found => (
                 Kind: found.ObjectType, found.Attributes.Space, found.Attributes.Mode,
                 Layer: doc.Layers[found.Attributes.LayerIndex].Id,
                 Material: Optional(found.GetRenderMaterial(frontMaterial: true)?.Id),
                 found.Attributes.MaterialSource, Memory: found.MemoryEstimate())).Strict()),
         Layers.Read(doc, Seq<Guid>()),
         DefinitionGraph.Read(doc),
         IO.lift(() => Conversions.Present(doc.Path).Map(static path => new FileInfo(path)).Filter(static file => file.Exists).Map(static file => file.Length)))
        .Apply(static (rows, tree, graph, length) => new DocumentStatistics(
            LanguageExt.HashMap.createRange(rows.CountBy(static row => row.Kind)),
            LanguageExt.HashMap.createRange(rows.CountBy(static row => row.Space)),
            LanguageExt.HashMap.createRange(rows.CountBy(static row => row.Mode)),
            LanguageExt.HashMap.createRange(rows.CountBy(static row => row.Layer)),
            LanguageExt.HashMap.createRange(rows.CountBy(static row => row.Material)),
            LanguageExt.HashMap.createRange(rows.CountBy(static row => row.MaterialSource)),
            rows.Fold(0UL, static (sum, row) => sum + row.Memory),
            tree,
            graph,
            length))
        .As();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
internal static partial class StatisticsMapper {
    [MapPropertyFromSource(nameof(ObjectEnumeratorSettings.HiddenObjects))]
    [MapPropertyFromSource(nameof(ObjectEnumeratorSettings.IdefObjects))]
    [MapPropertyFromSource(nameof(ObjectEnumeratorSettings.ReferenceObjects))]
    [MapPropertyFromSource(nameof(ObjectEnumeratorSettings.IncludeLights))]
    internal static partial ObjectEnumeratorSettings Settings(bool include);
}

using Rasm.Rhino.Blocks;
using Rasm.Rhino.Document.Tables;
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
    LayerTree LayerTree,
    DefinitionGraph DefinitionGraph,
    Option<long> FileLength) {
    public int ObjectCount => Kinds.Values.Fold(0, static (sum, count) => sum + count);

    public int LayerLevelCount => LayerTree.Depths.Values.Fold(0, static (levels, depth) => int.Max(levels, depth + 1));

    public int PlacementCount => DefinitionGraph.Placements.Values.Fold(0, static (sum, placed) => sum + placed.Count);

    public static IO<DocumentStatistics> Read(RhinoDoc doc) =>
        (new ObjectTarget.Lookup(static table => table.GetObjectList(new ObjectEnumeratorSettings { HiddenObjects = true, IdefObjects = true, ReferenceObjects = true, IncludeLights = true }))
             .Objects(doc)
             .Map(objects => objects.Map(found => Row.Of(doc, found)).Strict()),
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

    private readonly record struct Row(ObjectType Kind, ActiveSpace Space, ObjectMode Mode, Guid Layer, Option<Guid> Material, ObjectMaterialSource MaterialSource, uint Memory) {
        public static Row Of(RhinoDoc doc, RhinoObject found) =>
            new(found.ObjectType, found.Attributes.Space, found.Attributes.Mode, doc.Layers[found.Attributes.LayerIndex].Id,
                Optional(found.GetRenderMaterial(frontMaterial: true)).Map(static material => material.Id), found.Attributes.MaterialSource, found.MemoryEstimate());
    }
}

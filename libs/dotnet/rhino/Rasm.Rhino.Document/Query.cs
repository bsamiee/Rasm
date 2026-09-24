using System.Drawing;
using LanguageExt.UnsafeValueAccess;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Document;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ObjectQuery {
    public bool NormalObjects { get; init; } = true;

    public bool LockedObjects { get; init; } = true;

    public bool HiddenObjects { get; init; }

    public bool IdefObjects { get; init; }

    public bool DeletedObjects { get; init; }

    public bool SubObjectSelected { get; init; }

    public bool ActiveObjects { get; init; } = true;

    public bool ReferenceObjects { get; init; }

    public bool IncludeLights { get; init; }

    public bool IncludeGrips { get; init; }

    public bool IncludePhantoms { get; init; }

    public bool SelectedObjectsFilter { get; init; }

    public bool UseFastSelection { get; init; }

    public bool VisibleFilter { get; init; }

    public ObjectType ObjectTypeFilter { get; init; } = ObjectType.AnyObject;

    public Option<Type> ClassTypeFilter { get; init; }

    public Option<int> LayerIndexFilter { get; init; }

    public Option<int> MaterialIndexFilter { get; init; }

    public Option<string> NameFilter { get; init; }

    public ActiveSpace SpaceFilter { get; init; } = ActiveSpace.None;

    public Option<ViewportTarget> ViewportFilter { get; init; }

    public static ObjectQuery Default { get; } = new();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ObjectPredicate {
    public sealed record DrawColor(Color Value) : ObjectPredicate;

    public sealed record Bounds(BoundingBox Region, bool Contains, double Tolerance) : ObjectPredicate;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ObjectTarget {
    public sealed record Ids(Seq<Guid> Values) : ObjectTarget;

    public sealed record Query(ObjectQuery Spec, Seq<ObjectPredicate> Predicates) : ObjectTarget;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SearchStores {
    public sealed record GeometryOnly() : SearchStores;

    public sealed record AttributesOnly() : SearchStores;

    public sealed record BothStores() : SearchStores;
}

public sealed record UserStringSearch(string KeyPattern, string ValuePattern, bool CaseSensitive, SearchStores Stores);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Queries {
    public static IO<Seq<RhinoObject>> Evaluate(RhinoDoc doc, ObjectTarget target) =>
        target.Switch(
            doc,
            ids: static (document, ids) => ids.Values
                .TraverseM(id => IO.lift(() => Missing.Unless(document.Objects.FindId(id), nameof(ObjectTable.FindId))))
                .As(),
            query: static (document, query) =>
                from settings in Enumerated(document, query.Spec)
                let matches = fun((RhinoObject candidate) => query.Predicates.ForAll(predicate => Matches(document, candidate, predicate)))
                from objects in IO.lift(() => toSeq(document.Objects.GetObjectList(settings)).Filter(matches).Strict())
                select objects);

    public static IO<Seq<RhinoObject>> FindByUserString(RhinoDoc doc, ObjectQuery spec, UserStringSearch search) =>
        from keyed in IO.lift(() => Invalid.Unless(search.KeyPattern.Length > 0, nameof(UserStringSearch.KeyPattern)))
        from settings in Enumerated(doc, spec)
        let stores = search.Stores.Map<(bool Geometry, bool Attributes)>(geometryOnly: (true, false), attributesOnly: (false, true), bothStores: (true, true))
        from found in IO.lift(() => toSeq(doc.Objects.FindByUserString(search.KeyPattern, search.ValuePattern, search.CaseSensitive, stores.Geometry, stores.Attributes, settings)))
        select found;

    public static IO<(TObject Object, TGeometry Geometry)> Resolve<TObject, TGeometry>(RhinoDoc doc, Guid id) where TObject : RhinoObject where TGeometry : GeometryBase =>
        IO.lift(() =>
            from found in Missing.Unless(doc.Objects.FindId(id), nameof(ObjectTable.FindId))
            from narrowed in Optional(found as TObject).ToFin(new WrongGeometry(typeof(TObject), found.ObjectType))
            from geometry in Missing.Unless(found.Geometry, nameof(RhinoObject.Geometry))
            from typed in Optional(geometry as TGeometry).ToFin(new WrongGeometry(typeof(TGeometry), geometry.ObjectType))
            select (narrowed, typed));

    private static IO<ObjectEnumeratorSettings> Enumerated(RhinoDoc doc, ObjectQuery spec) =>
        from validated in IO.lift(() => Validated(spec))
        from viewport in spec.ViewportFilter.Traverse(address => Viewports.ResolveViewport(doc, address).Map(static row => row.Viewport)).As()
        select QueryMapper.Settings(spec, viewport.ValueUnsafe());

    private static Fin<Unit> Validated(ObjectQuery spec) =>
        from states in Invalid.Unless(spec.NormalObjects || spec.LockedObjects || spec.HiddenObjects || spec.IdefObjects || spec.DeletedObjects, nameof(ObjectEnumeratorSettings))
        from origins in Invalid.Unless(spec.ActiveObjects || spec.ReferenceObjects, nameof(ObjectEnumeratorSettings))
        from named in Invalid.Unless(
            spec.NameFilter.ForAll(static text => (text.Length > 0) && !string.Equals(text, "*", StringComparison.Ordinal)),
            nameof(ObjectEnumeratorSettings.NameFilter))
        select unit;

    private static bool Matches(RhinoDoc doc, RhinoObject candidate, ObjectPredicate predicate) =>
        predicate.Switch(
            (Doc: doc, Candidate: candidate),
            drawColor: static (state, draw) => state.Candidate.Attributes.DrawColor(state.Doc).ToArgb() == draw.Value.ToArgb(),
            bounds: static (state, bounds) => Optional(state.Candidate.Geometry)
                .Exists(geometry => Within(bounds.Region, bounds.Tolerance, bounds.Contains, geometry.GetBoundingBox(accurate: true))));

    private static bool Within(BoundingBox region, double tolerance, bool contains, BoundingBox box) {
        BoundingBox inflated = region;
        inflated.Inflate(tolerance);
        return contains ? inflated.Contains(box, strict: false) : BoundingBox.Intersection(inflated, box).IsValid;
    }
}

[Mapper]
internal static partial class QueryMapper {
    [MapperIgnoreSource(nameof(ObjectQuery.ViewportFilter), Justification = "Resolved to the viewportFilter parameter")]
    [MapProperty(nameof(ObjectQuery.LayerIndexFilter), nameof(ObjectEnumeratorSettings.LayerIndexFilter), Use = nameof(LayerIndex))]
    [MapProperty(nameof(ObjectQuery.MaterialIndexFilter), nameof(ObjectEnumeratorSettings.MaterialIndexFilter), Use = nameof(MaterialIndex))]
    internal static partial ObjectEnumeratorSettings Settings(ObjectQuery query, RhinoViewport? viewportFilter);

    private static int LayerIndex(Option<int> index) => index.IfNone(-1);

    private static int MaterialIndex(Option<int> index) => index.IfNone(RhinoMath.UnsetIntIndex);

    [UserMapping]
    private static Type? ClassType(Option<Type> type) => type.ValueUnsafe();

    [UserMapping]
    private static string? Name(Option<string> name) => name.ValueUnsafe();
}

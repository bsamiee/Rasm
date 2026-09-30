using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Document;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ObjectTarget {
    public sealed record Ids(Seq<Guid> Values) : ObjectTarget;

    public sealed record Serials(Seq<uint> Values) : ObjectTarget;

    public sealed record Query(ObjectEnumeratorSettings Settings) : ObjectTarget;
}

public sealed record UserStringSearch(string KeyPattern, string ValuePattern, bool CaseSensitive, bool SearchGeometry, bool SearchAttributes);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Queries {
    public static IO<Seq<RhinoObject>> Evaluate(RhinoDoc doc, ObjectTarget target) =>
        IO.lift(() => target.Switch(
            doc,
            ids: static (document, ids) => Found(ids.Values, document.Objects.FindId, nameof(ObjectTable.FindId)),
            serials: static (document, serials) => Found(serials.Values, document.Objects.Find, nameof(ObjectTable.Find)),
            query: static (document, query) => toSeq(document.Objects.GetObjectList(query.Settings)).Strict()));

    public static IO<Seq<RhinoObject>> FindByUserString(RhinoDoc doc, ObjectEnumeratorSettings settings, UserStringSearch search) =>
        IO.lift(() => toSeq(doc.Objects.FindByUserString(search.KeyPattern, search.ValuePattern, search.CaseSensitive, search.SearchGeometry, search.SearchAttributes, settings)));

    public static IO<(TObject Object, TGeometry Geometry)> Resolve<TObject, TGeometry>(RhinoDoc doc, Guid id) where TObject : RhinoObject where TGeometry : GeometryBase =>
        IO.lift(() =>
            from found in Missing.Unless(doc.Objects.FindId(id), nameof(ObjectTable.FindId))
            from narrowed in Optional(found as TObject).ToFin(new WrongType(typeof(TObject), found.GetType()))
            from geometry in Missing.Unless(found.Geometry, nameof(RhinoObject.Geometry))
            from typed in Optional(geometry as TGeometry).ToFin(new WrongType(typeof(TGeometry), geometry.GetType()))
            select (narrowed, typed));

    private static Fin<Seq<RhinoObject>> Found<TKey>(Seq<TKey> keys, Func<TKey, RhinoObject?> find, string member) =>
        keys.Map((key, index) => Optional(find(key)).ToFin(new InvalidElement(member, index))).Traverse(identity).As();
}

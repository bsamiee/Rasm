using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Document.Tables;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ObjectTarget {
    public sealed record Ids(Seq<Guid> Values) : ObjectTarget;

    public sealed record Serials(Seq<uint> Values) : ObjectTarget;

    public sealed record Lookup(Func<ObjectTable, IEnumerable<RhinoObject>> Find) : ObjectTarget;

    public sealed record Bounded(ObjectEnumeratorSettings Settings, Func<BoundingBox, bool> Predicate) : ObjectTarget;

    public sealed record Window(RhinoViewport Viewport, Seq<Point3d> Region, bool Inside, bool Crossing, ObjectType Filter) : ObjectTarget;

    public IO<Seq<RhinoObject>> Objects(RhinoDoc document) =>
        Switch(
            document,
            ids: static (doc, ids) => IO.lift(() => Located(ids.Values, doc.Objects.FindId, nameof(ObjectTable.FindId))),
            serials: static (doc, serials) => IO.lift(() => Located(serials.Values, doc.Objects.Find, nameof(ObjectTable.Find))),
            lookup: static (doc, lookup) => IO.lift(() => Conversions.Rows(lookup.Find(doc.Objects))),
            bounded: static (doc, bounded) => IO.lift(() =>
                Conversions.Rows(doc.Objects.GetObjectList(bounded.Settings)).Filter(found => RhinoObject.GetTightBoundingBox([found], out BoundingBox box) && bounded.Predicate(box)).Strict()),
            window: static (doc, window) => IO.lift(() =>
                    Conversions.Rows(doc.Objects.GetSelectedObjects(includeLights: true, includeGrips: false)).Map(static found => (Object: found, Persistent: found.IsSelected(checkSubObjects: false) == 2)).Strict())
                .Bracket(
                    Use: selected =>
                        from cleared in IO.lift(() => selected.Iter(static held => held.Object.Select(@on: false, syncHighlight: true)))
                        from picked in IO.lift(() => Conversions.Rows(window.Crossing
                            ? doc.Objects.FindByCrossingWindowRegion(window.Viewport, window.Region, window.Inside, window.Filter)
                            : doc.Objects.FindByWindowRegion(window.Viewport, window.Region, window.Inside, window.Filter)))
                        select picked,
                    Fin: static selected => IO.lift(() => selected.Iter(static held =>
                        held.Object.Select(on: true, syncHighlight: true, persistentSelect: held.Persistent, ignoreGripsState: true, ignoreLayerLocking: true, ignoreLayerVisibility: true)))));

    public static IO<(TObject Object, TGeometry Geometry)> Resolve<TObject, TGeometry>(RhinoDoc document, Guid id) where TObject : RhinoObject where TGeometry : GeometryBase =>
        IO.lift(() =>
            from found in Missing.Unless(document.Objects.FindId(id), nameof(ObjectTable.FindId))
            from typed in WrongType.Unless<TObject>(found)
            from geometry in Missing.Unless(typed.Geometry, nameof(RhinoObject.Geometry))
            from shaped in WrongType.Unless<TGeometry>(geometry)
            select (typed, shaped));

    private static Fin<Seq<RhinoObject>> Located<TKey>(Seq<TKey> keys, Func<TKey, RhinoObject?> find, string member) =>
        Callbacks.Each(keys, Fin<RhinoObject> (key, index) => find(key) is { } found ? found : new RefusedElement(member, index));
}

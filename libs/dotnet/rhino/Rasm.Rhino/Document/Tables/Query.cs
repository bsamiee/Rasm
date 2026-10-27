using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rasm.RhinoCommon.Viewport;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace Rasm.RhinoCommon.Document.Tables;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record NamedId(Guid Id, Option<string> Name);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ObjectTarget {
    public sealed record Ids(Seq<Guid> Values) : ObjectTarget;

    public sealed record Serials(Seq<uint> Values) : ObjectTarget;

    public sealed record Lookup(Func<ObjectTable, IEnumerable<RhinoObject>> Find) : ObjectTarget;

    public sealed record Bounded(ObjectEnumeratorSettings Settings, Func<BoundingBox, bool> Predicate) : ObjectTarget;

    public sealed record Window(ViewportTarget Viewport, Seq<Point3d> Region, bool Inside, bool Crossing, ObjectType Filter) : ObjectTarget;

    public sealed record Named(NamedEntry Entry) : ObjectTarget;

    public IO<Seq<RhinoObject>> Objects(RhinoDoc document) =>
        Switch(
            document,
            ids: static (doc, ids) => IO.lift(() => Located(ids.Values, doc.Objects.FindId)),
            serials: static (doc, serials) => IO.lift(() => Located(serials.Values, doc.Objects.Find)),
            lookup: static (doc, lookup) => IO.lift(() => Conversions.Rows(lookup.Find(doc.Objects))),
            bounded: static (doc, bounded) => IO.lift(() =>
                Conversions.Rows(doc.Objects.GetObjectList(bounded.Settings)).Filter(found => RhinoObject.GetTightBoundingBox([found], out BoundingBox box) && bounded.Predicate(box)).Strict()),
            window: static (doc, window) => Viewports.With(doc, window.Viewport, row => Selection(doc).Bind(selected =>
                (from cleared in IO.lift(() => selected.Iter(static held => held.Object.Select(@on: false)))
                 from picked in IO.lift(() => Conversions.Rows(window.Crossing
                     ? doc.Objects.FindByCrossingWindowRegion(row.Viewport, window.Region, window.Inside, window.Filter)
                     : doc.Objects.FindByWindowRegion(row.Viewport, window.Region, window.Inside, window.Filter)))
                 select picked).Finally(Reselected(selected)))),
            named: static (doc, named) =>
                from members in IO.lift(() => named.Entry.Table.Held(named.Entry.Table.Members))
                from ids in members(doc, named.Entry)
                from found in IO.lift(() => Located(ids, doc.Objects.FindId))
                select found);

    public static IO<(TObject Object, TGeometry Geometry)> Resolve<TObject, TGeometry>(RhinoDoc document, Guid id) where TObject : RhinoObject where TGeometry : GeometryBase =>
        IO.lift(() =>
            from found in Conversions.Found(document.Objects.FindId(id))
            from typed in Conversions.Cast<TObject>(found)
            from geometry in Conversions.Found(typed.Geometry)
            from shaped in Conversions.Cast<TGeometry>(geometry)
            select (typed, shaped));

    internal static IO<Seq<(RhinoObject Object, bool Persistent)>> Selection(RhinoDoc document) =>
        IO.lift(() => Conversions.Rows(document.Objects.GetSelectedObjects(includeLights: true, includeGrips: false))
            .Map(static found => (Object: found, Persistent: found.IsSelected(checkSubObjects: false) is 2)).Strict());

    internal static IO<Unit> Reselected(Seq<(RhinoObject Object, bool Persistent)> selected) =>
        IO.lift(() => selected.Iter(static held =>
            held.Object.Select(on: true, syncHighlight: true, persistentSelect: held.Persistent, ignoreGripsState: true, ignoreLayerLocking: true, ignoreLayerVisibility: true)));

    private static Fin<Seq<RhinoObject>> Located<TKey>(Seq<TKey> keys, Func<TKey, RhinoObject?> find, [CallerArgumentExpression(nameof(find))] string call = "") =>
        Callbacks.Each(keys, (key, index) => Conversions.Found(find(key), index, call));
}

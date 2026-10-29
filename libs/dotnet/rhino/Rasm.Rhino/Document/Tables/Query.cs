using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Document.Tables;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GeometryPair(GeometryBase Geometry, Option<ObjectAttributes> Attributes) {
    public IO<Guid> Add(RhinoDoc doc, Option<HistoryRecord> history, bool reference) =>
        WithAttributes(doc.CreateDefaultAttributes, Attributes, attributes =>
            IO.lift(() => Conversions.Required(Conversions.Present(doc.Objects.Add(Geometry, attributes, history.ValueUnsafe(), reference)))));

    public static IO<T> WithAttributes<T>(Func<ObjectAttributes> defaults, Option<ObjectAttributes> attributes, Func<ObjectAttributes, IO<T>> body) =>
        attributes.Match(Some: body, None: () => use(defaults).Bind(body).Bracket());

    public static IO<T> WithAttributes<T>(Func<ObjectAttributes> defaults, Seq<GeometryPair> members, Func<Seq<GeometryBase>, Seq<ObjectAttributes>, IO<T>> body) =>
        members.FoldBack(
            (Seq<ObjectAttributes> rows) => body(members.Map(static member => member.Geometry), rows),
            (next, member) => rows => WithAttributes(defaults, member.Attributes, value => next(rows.Add(value))))(Seq<ObjectAttributes>());
}

public sealed record PickCapture(
    Guid ObjectId,
    Option<ComponentIndex> GeometryComponentIndex,
    SelectionMethod SelectionMethod,
    Option<Point3d> SelectionPoint,
    Option<double> CurveParameter,
    Option<(double U, double V)> SurfaceParameter,
    Option<Guid> Viewport) {
    public static IO<PickCapture> Of(ObjRef reference) =>
        IO.lift(reference.SelectionMethod).Map(method => new PickCapture(
            reference.ObjectId,
            Conversions.Present(reference.GeometryComponentIndex),
            method,
            Conversions.Present(reference.SelectionPoint()),
            Conversions.Present(reference.CurveParameter(out double t) is not null && method is SelectionMethod.MousePick, t),
            Conversions.Present(reference.SurfaceParameter(out double u, out double v) is not null, (u, v)),
            Conversions.Present(reference.SelectionViewDetailSerialNumber()).Bind(serial => Optional(reference.Document.Objects.Find(serial))).Map(static detail => detail.Id)
            || Optional(reference.SelectionView()).Map(static view => view.MainViewport.Id)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ObjectTarget {
    public sealed record Ids(Seq<Guid> Values) : ObjectTarget;

    public sealed record Serials(Seq<uint> Values) : ObjectTarget;

    public sealed record Lookup(Func<ObjectTable, IEnumerable<RhinoObject>> Find) : ObjectTarget;

    public sealed record Bounded(ObjectEnumeratorSettings Settings, Func<BoundingBox, bool> Predicate) : ObjectTarget;

    public sealed record Window(RhinoViewport Viewport, Seq<Point3d> Region, bool Inside, bool Crossing, ObjectType Filter) : ObjectTarget;

    public sealed record Named(NamedEntry Entry) : ObjectTarget;

    // --- [RESOLUTION]
    public IO<Seq<RhinoObject>> Objects(RhinoDoc document) =>
        Switch(
            document,
            ids: static (doc, ids) => IO.lift(() => Located(ids.Values, doc.Objects.FindId)),
            serials: static (doc, serials) => IO.lift(() => Located(serials.Values, doc.Objects.Find)),
            lookup: static (doc, lookup) => IO.lift(() => Conversions.Rows(lookup.Find(doc.Objects))),
            bounded: static (doc, bounded) => IO.lift(() =>
                Conversions.Rows(doc.Objects.GetObjectList(bounded.Settings)).Filter(found => RhinoObject.GetTightBoundingBox([found], out BoundingBox box) && bounded.Predicate(box)).Strict()),
            window: static (doc, window) => Selection(doc).Bind(selected =>
                (from cleared in IO.lift(() => selected.Iter(static held => held.Object.Select(@on: false)))
                 from picked in IO.lift(() => Conversions.Rows(window.Crossing
                     ? doc.Objects.FindByCrossingWindowRegion(window.Viewport, window.Region, window.Inside, window.Filter)
                     : doc.Objects.FindByWindowRegion(window.Viewport, window.Region, window.Inside, window.Filter)))
                 select picked).Finally(Reselected(selected))),
            named: static (doc, named) =>
                from members in IO.lift(() => named.Entry.Table.Supported(named.Entry.Table.Members))
                from ids in members(doc, named.Entry)
                from found in IO.lift(() => Located(ids, doc.Objects.FindId))
                select found);

    public static IO<(TObject Object, TGeometry Geometry)> Resolve<TObject, TGeometry>(RhinoDoc document, Guid id) where TObject : RhinoObject where TGeometry : GeometryBase =>
        IO.lift(() =>
            from found in Conversions.Required(document.Objects.FindId(id))
            from typed in Conversions.Cast<TObject>(found)
            from geometry in Conversions.Required(typed.Geometry)
            from shaped in Conversions.Cast<TGeometry>(geometry)
            select (typed, shaped));

    // --- [WRITES]
    public IO<Seq<Guid>> Each(RhinoDoc document, Func<RhinoObject, bool> call, [CallerArgumentExpression(nameof(call))] string member = "") =>
        Objects(document).Bind(found => IO.lift(() => Products.Each(found, (target, index) => call(target) ? Fin.Succ(target.Id) : new RhinoFault(member, RhinoReason.Declined, index))));

    // --- [SELECTION]
    internal static IO<Seq<(RhinoObject Object, bool Persistent)>> Selection(RhinoDoc document) =>
        IO.lift(() => Conversions.Rows(document.Objects.GetSelectedObjects(includeLights: true, includeGrips: false))
            .Map(static found => (Object: found, Persistent: found.IsSelected(checkSubObjects: false) is 2)).Strict());

    internal static IO<Unit> Reselected(Seq<(RhinoObject Object, bool Persistent)> selected) =>
        IO.lift(() => selected.Iter(static held =>
            held.Object.Select(on: true, syncHighlight: true, persistentSelect: held.Persistent, ignoreGripsState: true, ignoreLayerLocking: true, ignoreLayerVisibility: true)));

    private static Fin<Seq<RhinoObject>> Located<TKey>(Seq<TKey> keys, Func<TKey, RhinoObject?> find, [CallerArgumentExpression(nameof(find))] string call = "") =>
        Products.Each(keys, (key, index) => Conversions.Required(find(key), index, call));
}

using System.Drawing;
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

    public sealed record Since(uint Serial) : ObjectTarget;

    public sealed record Query(ObjectEnumeratorSettings Settings) : ObjectTarget;

    public sealed record UserString(string KeyPattern, string ValuePattern, bool CaseSensitive, bool SearchGeometry, bool SearchAttributes, ObjectEnumeratorSettings Settings) : ObjectTarget;

    public sealed record DrawColor(Color Color, bool IncludeLights) : ObjectTarget;

    public sealed record Grouped(int GroupIndex) : ObjectTarget;

    public sealed record Contained(ObjectEnumeratorSettings Settings, BoundingBox Region, bool Strict) : ObjectTarget;

    public sealed record Overlapping(ObjectEnumeratorSettings Settings, BoundingBox Region, double Tolerance) : ObjectTarget;

    public sealed record WindowRegion(RhinoViewport Viewport, Seq<Point3d> Region, bool Inside, ObjectType Filter) : ObjectTarget;

    public sealed record CrossingWindowRegion(RhinoViewport Viewport, Seq<Point3d> Region, bool Inside, ObjectType Filter) : ObjectTarget;

    public IO<Seq<RhinoObject>> Objects(RhinoDoc document) =>
        IO.lift(() => document.Objects).Bind(objects => Switch(
            objects,
            ids: static (table, ids) => IO.lift(() => Located(ids.Values, table.FindId, nameof(ObjectTable.FindId))),
            serials: static (table, serials) => IO.lift(() => Located(serials.Values, table.Find, nameof(ObjectTable.Find))),
            since: static (table, since) => IO.lift(() => Conversions.Rows(table.AllObjectsSince(since.Serial))),
            query: static (table, query) => IO.lift(() => Conversions.Rows(table.GetObjectList(query.Settings))),
            userString: static (table, search) => IO.lift(() => Conversions.Rows(table.FindByUserString(search.KeyPattern, search.ValuePattern, search.CaseSensitive, search.SearchGeometry, search.SearchAttributes, search.Settings))),
            drawColor: static (table, drawn) => IO.lift(() => Conversions.Rows(table.FindByDrawColor(drawn.Color, drawn.IncludeLights))),
            grouped: static (table, grouped) => IO.lift(() => Conversions.Rows(table.FindByGroup(grouped.GroupIndex))),
            contained: static (table, contained) => Bounded(table, contained.Settings, box => contained.Region.Contains(box, contained.Strict)),
            overlapping: static (table, overlapping) => Bounded(table, overlapping.Settings, box => !overlapping.Region.IsDisjoint(box, overlapping.Tolerance)),
            windowRegion: static (table, window) => Picked(table, () => table.FindByWindowRegion(window.Viewport, window.Region, window.Inside, window.Filter)),
            crossingWindowRegion: static (table, crossing) => Picked(table, () => table.FindByCrossingWindowRegion(crossing.Viewport, crossing.Region, crossing.Inside, crossing.Filter))));

    public static IO<(TObject Object, TGeometry Geometry)> Resolve<TObject, TGeometry>(RhinoDoc document, Guid id) where TObject : RhinoObject where TGeometry : GeometryBase =>
        IO.lift(() =>
            from found in Missing.Unless(document.Objects.FindId(id), nameof(ObjectTable.FindId))
            from typed in WrongType.Unless<TObject>(found)
            from geometry in Missing.Unless(typed.Geometry, nameof(RhinoObject.Geometry))
            from shaped in WrongType.Unless<TGeometry>(geometry)
            select (typed, shaped));

    private static Fin<Seq<RhinoObject>> Located<TKey>(Seq<TKey> keys, Func<TKey, RhinoObject?> find, string member) =>
        Callbacks.Each(keys, Fin<RhinoObject> (key, index) => find(key) is { } found ? found : new RefusedElement(member, index));

    private static IO<Seq<RhinoObject>> Bounded(ObjectTable table, ObjectEnumeratorSettings settings, Func<BoundingBox, bool> predicate) =>
        IO.lift(() => Conversions.Rows(table.GetObjectList(settings)).Filter(found => RhinoObject.GetTightBoundingBox([found], out BoundingBox box) && predicate(box)).Strict());

    private static IO<Seq<RhinoObject>> Picked(ObjectTable table, Func<RhinoObject[]> pick) =>
        IO.lift(() => Conversions.Rows(table.GetSelectedObjects(includeLights: true, includeGrips: true))
                .Choose(static found => found.IsSelected(checkSubObjects: false) switch {
                    1 => Some((Object: found, Persistent: false)),
                    2 => Some((Object: found, Persistent: true)),
                    _ => None,
                }))
            .Bracket(
                Use: selected => IO.lift(() => selected.Iter(static held => held.Object.Select(on: false, syncHighlight: true))).Bind(_ => IO.lift(() => Conversions.Rows(pick()))),
                Fin: static selected => IO.lift(() => selected.Iter(static held => held.Object.Select(on: true, syncHighlight: true, persistentSelect: held.Persistent, ignoreGripsState: true, ignoreLayerLocking: true, ignoreLayerVisibility: true))));
}

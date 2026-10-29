using Rasm.Rhino.Document.Units;
using Rhino;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Geometry.Intersect;
using Rhino.UI.Gumball;

namespace Rasm.Rhino.Objects;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class ObjectSelection {
    public static readonly ObjectSelection Unselected = new(0, on: false, persistentSelect: false);
    public static readonly ObjectSelection Selected = new(1, on: true, persistentSelect: false);
    public static readonly ObjectSelection Persistent = new(2, on: true, persistentSelect: true);

    public bool On { get; }

    public bool PersistentSelect { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record TransformedObjects(Transform Transform, bool ObjectsWillBeCopied, Seq<Guid> ObjectIds, Seq<Guid> GripOwnerIds, int GripCount, Option<uint> Document) {
    public static TransformedObjects Of(RhinoTransformObjectsEventArgs args) =>
        (Conversions.Rows(args.Objects), Conversions.Rows(args.GripOwners)) switch {
            var (objects, owners) => new(
                args.Transform,
                args.ObjectsWillBeCopied,
                objects.Map(static item => item.Id).Strict(),
                owners.Map(static item => item.Id).Strict(),
                args.GripCount,
                (objects + owners).Head.Bind(static item => Conversions.Serial(item.Document))),
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RhinoObjects {
    // --- [EVENTS]
    public static readonly DocumentEvent<TransformedObjects> Transformed = new(
        "RhinoDoc.BeforeTransformObjects | RhinoDoc.AfterTransformObjects",
        static deliver =>
            from pending in IO.lift(static () => Ref(HashMap<uint, TransformedObjects>()))
            from before in Subscriptions.On<RhinoTransformObjectsEventArgs>(static h => RhinoDoc.BeforeTransformObjects += h, static h => RhinoDoc.BeforeTransformObjects -= h).Attach(args =>
                atomicIO(() => pending.Swap(held => held.AddOrUpdate(args.TransformEventId, TransformedObjects.Of(args)))).Map(unit))
            from after in Subscriptions.On<RhinoAfterTransformObjectsEventArgs>(static h => RhinoDoc.AfterTransformObjects += h, static h => RhinoDoc.AfterTransformObjects -= h).Attach(args =>
                from taken in atomicIO(() => {
                    Option<TransformedObjects> found = pending.Value.Find(args.TransformEventId);
                    pending.Value = pending.Value.Remove(args.TransformEventId);
                    return found;
                })
                from delivered in taken.Traverse(deliver).As()
                select unit)
            select unit,
        static args => args.Document);

    // --- [FACTS]
    extension(RhinoObject o) {
        public ObjectSelection Selection => ObjectSelection.Get(o.IsSelected(checkSubObjects: false));

        public Option<GumballFrame> GumballFrame => Conversions.Present(o.TryGetGumballFrame(out GumballFrame frame), frame);

        public Option<GumballFrame> AlignedGumballFrame => Conversions.Present(o.TryGetGumballFrameForCurrentAlignment(out GumballFrame frame), frame);

        public Option<Transform> DynamicTransform => Conversions.Present(o.GetDynamicTransform(out Transform dragged), dragged);

        public Option<bool> Closed {
            get {
                _ = o.ShortDescriptionWithClosedStatus(prepend: false, plural: false, out int status);
                return Some(status).Filter(static closed => closed is not 0).Map(static closed => closed is 2);
            }
        }

        public IO<string> Description() =>
            (from log in use(static () => new TextLog())
             from described in IO.lift(() => {
                 o.Description(log);
                 return log.ToString();
             })
             select described).Bracket();

        // --- [SUBOBJECTS]
        public IO<Unit> SelectSubObjects(Seq<ComponentIndex> components, ObjectSelection selection, bool syncHighlight) =>
            IO.lift(() => Products.Each(components, (component, index) => ObjectSelection.Get(o.SelectSubObject(component, selection.On, syncHighlight, selection.PersistentSelect)) switch {
                var landed => RhinoFault.Unless(landed == selection, RhinoReason.Declined, landed, index),
            })).Map(unit);

        public IO<Unit> HighlightSubObjects(Seq<ComponentIndex> components, bool highlight) =>
            IO.lift(() => Products.Each(components, (component, index) => RhinoFault.Unless(o.HighlightSubObject(component, highlight) == highlight, RhinoReason.Declined, component, index))).Map(unit);

        // --- [CUTS]
        public IO<Seq<(GeometryBase Geometry, ObjectAttributes Attributes)>> Sections(Tolerances tolerances, Plane plane, string name, Option<double> thickness) =>
            from cut in Products.Of(() => thickness.Match(
                Some: value => (o.CreateSlices(plane, name, value, tolerances.Absolute, out ObjectAttributes[] rows), rows),
                None: () => (o.CreateSections(plane, name, tolerances.Absolute, out ObjectAttributes[] rows), rows)))
            from rows in toSeq(cut.Value).Zip(toSeq(cut.Out)).TraverseM(static pair =>
                from geometry in use(() => pair.First)
                from attributes in use(() => pair.Second)
                select (Geometry: geometry, Attributes: attributes)).As()
            select rows;
    }

    // --- [INTERFERENCES]
    public static IO<Seq<MeshInterference>> Interferences(Seq<RhinoObject> setA, Seq<RhinoObject> setB, double distance, MeshType meshType, MeshingParameters parameters) =>
        IO.lift(() => toSeq(MeshClash.Search(setA, setB, distance, meshType, parameters)).Filter(static found => Conversions.Present(found.IndexA).IsSome).Strict());

    // --- [REPLACEMENT]
    public static Stage<T> Replacement<T>(RhinoDoc doc, Guid id, bool ignoreModes) where T : GeometryBase =>
        Stages.Geometry<T>() switch {
            var geometry => geometry with {
                Commit = (live, staged) =>
                    from valid in geometry.Commit(live, staged)
                    from replaced in Products.Accepted(() => doc.Objects.Replace(id, valid, ignoreModes))
                    select valid,
            },
        };

    public static IO<TResult> ReplaceGeometry<T, TResult>(RhinoDoc doc, Guid id, Func<T, IO<(T Geometry, TResult Result)>> edit) where T : GeometryBase =>
        (from live in IO.lift(() => Conversions.Required(doc.Objects.FindId(id)).Bind(static found => Conversions.Cast<T>(found.Geometry)))
         let stage = Replacement<T>(doc, id, ignoreModes: false)
         from staged in stage.Copy(live)
         from edited in edit(staged)
         from committed in stage.Commit(live, edited.Geometry)
         select edited.Result).Bracket();
}

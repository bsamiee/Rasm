using LanguageExt.ClassInstances;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Events;
using Rasm.Rhino.Modeling;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using UnitsNet;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>(SkipIParsable = true)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LightRole {
    public static readonly LightRole Key = Create("key");
    public static readonly LightRole Fill = Create("fill");
    public static readonly LightRole Rim = Create("rim");
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record RigEmitter {
    public sealed record PointLight() : RigEmitter;

    public sealed record SpotLight(Angle Angle, double HotSpot) : RigEmitter;

    public sealed record DirectionalLight() : RigEmitter;

    public sealed record RectangularLight() : RigEmitter;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record RigColor {
    public sealed record Blackbody(UnitsNet.Temperature Kelvin) : RigColor;

    public sealed record Linear(ColorFilter Filter) : RigColor;
}

public sealed record RigLight(
    LightRole Role, RigEmitter Emitter, Angle Azimuth, Angle Elevation, double Distance,
    Angle Size, double Stops, RigColor Color, Light.Attenuation Falloff, Option<Guid> Light = default) {
    public (Vector3d Toward, Vector3d Across, Vector3d Rise) Axes =>
        (Math.SinCos(Azimuth.Radians.ToDouble()), Math.SinCos(Elevation.Radians.ToDouble())) switch {
            var ((sinA, cosA), (sinE, cosE)) => (new Vector3d(cosE * sinA, sinE, cosE * cosA), new Vector3d(cosA, 0d, -sinA), new Vector3d(-sinE * sinA, cosE, -sinE * cosA)),
        };
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record RigTarget {
    public sealed record Subjects(IterableNE<Guid> Ids) : RigTarget;

    public sealed record Fixed(Point3d Focus, double Radius) : RigTarget;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record RigBasis {
    public sealed record World() : RigBasis;

    public sealed record View(ViewportTarget Target) : RigBasis;

    public sealed record Frame(Guid Object) : RigBasis;
}

public sealed record LightRig(Guid Id, RigTarget Target, RigBasis Basis, double Level, Angle Rotation, Angle Elevation, Seq<RigLight> Lights);

public sealed record RigCommit(Func<LightRole, string> Caption, string Name, RedrawPolicy Redraw);

public sealed record PlacedLight(LightRole Role, LightShape Shape, LightColor Color, double Intensity, Vector3d AttenuationVector) {
    public LightSpec Spec(string name, bool enabled, double watts, double shadow) =>
        new(Conversions.Present(name), enabled, Shape, Color, Intensity, watts, shadow, AttenuationVector);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record RigChange {
    public sealed record Create(int Slot, PlacedLight Placed, Option<Guid> Source) : RigChange;

    public sealed record Place(int Slot, Guid Light, PlacedLight Placed) : RigChange;

    public sealed record Remove(Guid Light) : RigChange;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class LightRigs {
    // --- [KEYS]
    private const string RigKey = "rasm-rig";

    // --- [TARGET]
    public static IO<RigTarget.Fixed> Target(RhinoDoc doc, RigTarget target) =>
        target.Switch(
            doc,
            subjects: static (document, subjects) => from found in new ObjectTarget.Ids(subjects.Ids.AsIterable().ToSeq()).Objects(document)
                                                     let bounds = Measurements.Bounds(found.Map(static held => held.Geometry.GetBoundingBox(accurate: true)))
                                                     from box in IO.lift(bounds.ToFin(new Refused(nameof(GeometryBase.GetBoundingBox), Refused.Absent)))
                                                     select new RigTarget.Fixed(box.Center, box.Diagonal.Length / 2d),
            @fixed: static (_, held) => IO.pure(held));

    public static IO<Plane> Basis(RhinoDoc doc, RigBasis basis, Point3d focus) =>
        basis.Switch(
            (Document: doc, Focus: focus),
            world: static (state, _) => IO.pure(new Plane(state.Focus, Vector3d.XAxis, Vector3d.ZAxis)),
            view: static (state, view) =>
                Viewports.With(state.Document, view.Target, static row => Cameras.Read(row.Viewport))
                    .Map(pose => new Plane(state.Focus, pose.Frame.XAxis, pose.Frame.YAxis)),
            frame: static (state, frame) => IO.lift(() => Refused.UnlessFound(state.Document.Objects.FindId(frame.Object), nameof(ObjectTable.FindId)))
                .Bind(static found => IO.lift(() => Conversions.Present(found.ObjectFrame()).ToFin(new Refused(nameof(RhinoObject.ObjectFrame), Refused.Invalid))))
                .Map(axes => new Plane(state.Focus, axes.XAxis, axes.ZAxis)));

    // --- [PLACEMENT]
    public static Seq<PlacedLight> Placed(LightRig rig, RigTarget.Fixed target, Plane basis, LengthUnit modelUnit) =>
        (Transform.Rotation(rig.Rotation.Radians.ToDouble(), basis.YAxis, basis.Origin)
            * Transform.Rotation(-rig.Elevation.Radians.ToDouble(), basis.XAxis, basis.Origin)
            * Transform.PlaneToPlane(Plane.WorldXY, basis)) switch {
                var placing => rig.Lights.Map(light => Positioned(rig, light, target, placing, modelUnit)).Strict(),
            };

    private static PlacedLight Positioned(LightRig rig, RigLight light, RigTarget.Fixed target, Transform placing, LengthUnit modelUnit) =>
        (Reach: light.Distance * target.Radius, Meters: light.Distance * Quantities.From(target.Radius, modelUnit).Meters.ToDouble(), light.Axes) switch {
            var (reach, meters, axes) => (
                Side: 2d * reach * Math.Tan(light.Size.Radians.ToDouble() / 2d),
                At: placing * new Point3d(reach * axes.Toward),
                Falloff: light.Falloff switch {
                    Light.Attenuation.Constant => (Gain: 1d, Vector: Light.ConstantAttenuationVector),
                    Light.Attenuation.Linear => (Gain: meters, Vector: Light.LinearAttenuationVector),
                    Light.Attenuation.InverseSquared => (Gain: meters * meters, Vector: Light.InverseSquaredAttenuationVector),
                }) switch {
                    var (side, at, falloff) => light.Emitter.Switch<(Point3d At, Vector3d Aim, double Side, Angle Size, double Gain, Vector3d Across, Vector3d Rise), (LightShape Shape, double Gain)>(
                        (at, target.Focus - at, side, light.Size, falloff.Gain, side * (placing * axes.Across), side * (placing * axes.Rise)),
                        pointLight: static (state, _) => (new LightShape.PointLight(state.At, state.Side / 2d, CameraRelative: false), state.Gain),
                        spotLight: static (state, spot) => (new LightShape.SpotLight(state.At, state.Aim, spot.Angle, spot.HotSpot, state.Side / 2d, CameraRelative: false), state.Gain),
                        directionalLight: static (state, _) => (new LightShape.DirectionalLight(state.At, state.Aim, state.Size, CameraRelative: false), 1d),
                        rectangularLight: static (state, _) => (new LightShape.RectangularLight(state.At - (state.Across / 2d) - (state.Rise / 2d), state.Across, state.Rise, state.Aim), state.Gain)) switch {
                            var (shape, gain) => new PlacedLight(
                                light.Role, shape, light.Color.Switch(blackbody: static blackbody => (LightColor)blackbody.Kelvin, linear: static linear => (LightColor)linear.Filter),
                                rig.Level * double.Exp2(light.Stops) * gain, falloff.Vector),
                        },
                },
        };

    // --- [RECONCILE]
    public static IO<Committed<LightRig>> Reconcile(RhinoDoc doc, LightRig rig, RigCommit commit, IPlugInSink sink) =>
        Commits.Commit(doc, RowText.Localize(commit.Name, sink), commit.Redraw,
            from target in Target(doc, rig.Target)
            from basis in Basis(doc, rig.Basis, target.Focus)
            from held in Held(doc, rig.Id)
            from placed in Apply(doc, rig.Id, Plan(rig, held, Placed(rig, target, basis, doc.ModelUnits)), role => RowText.Localize(commit.Caption(role), sink))
            select rig with { Lights = rig.Lights.Map((light, slot) => light with { Light = placed.Find(slot) }).Strict() });

    public static IO<LanguageExt.HashSet<Guid>> Held(RhinoDoc doc, Guid rig) =>
        IO.lift(() => toHashSet(Conversions.Rows(doc.Lights)
            .Filter(light => !light.IsDeleted && UserStrings.Held(light.Attributes.GetUserStrings()).Find(RigKey) == Some(rig.ToString("D")))
            .Map(static light => light.Id)));

    public static Seq<RigChange> Plan(LightRig rig, LanguageExt.HashSet<Guid> held, Seq<PlacedLight> placed) =>
        rig.Lights.Choose(static light => light.Light).Filter(held.Contains) switch {
            var recorded => rig.Lights.Zip(placed).Map((pair, slot) => pair.First.Light.Filter(held.Contains).Match(
                    Some: light => (RigChange)new RigChange.Place(slot, light, pair.Second),
                    None: () => new RigChange.Create(slot, pair.Second, recorded.Head)))
                + toSeq(held - toHashSet(recorded)).Map(static light => (RigChange)new RigChange.Remove(light)),
        };

    private static IO<HashMap<int, Guid>> Apply(RhinoDoc doc, Guid rig, Seq<RigChange> changes, Func<LightRole, string> caption) =>
        from placed in changes.TraverseM(change => change.Switch(
                (Document: doc, Rig: rig, Caption: caption),
                create: static (state, create) => Created(state.Document, state.Rig, create, state.Caption).Map(ids => ids.Head.Map(id => (create.Slot, Light: id, Fresh: create.Source.IsNone))),
                place: static (state, place) => Moved(state.Document, place, state.Caption).Map(_ => Some((place.Slot, place.Light, Fresh: false))),
                remove: static (state, remove) =>
                    Lights.Apply(state.Document, new LightOp.Delete(new ComponentRef<LightObject>.ById(remove.Light), Quiet: true))
                        .Map(static _ => Option<(int Slot, Guid Light, bool Fresh)>.None))).As()
        let rows = placed.Somes()
        let fresh = rows.Filter(static row => row.Fresh).Map(static row => row.Light)
        from grouped in unless(fresh.IsEmpty, Lights.Apply(doc, new LightOp.Grouping(
            fresh.Map<ComponentRef<LightObject>>(static id => new ComponentRef<LightObject>.ById(id)).Strict(), On: true)).Map(unit)).As()
        select toHashMap(rows.Map(static row => (row.Slot, row.Light)));

    private static IO<Seq<Guid>> Created(RhinoDoc doc, Guid rig, RigChange.Create create, Func<LightRole, string> caption) =>
        from source in create.Source.Traverse(member => IO.lift(() => Refused.UnlessFound(doc.Objects.FindId(member), nameof(ObjectTable.FindId)))).As()
        let joined = source.ToSeq().Bind(static found => Seq(
            AttributeEdits.LayerIndex(new LayerRef.Row(new ComponentRef<Layer>.ByIndex(found.Attributes.LayerIndex))),
            AttributeEdits.Groups(new RowsEdit<ComponentRef<Group>, ComponentRef<Group>>.Add(
                Conversions.Rows(found.Attributes.GetGroupList()).Map<ComponentRef<Group>>(static index => new ComponentRef<Group>.ByIndex(index))))))
        let keyed = AttributeEdits.SetUserStrings(HashMap<EqStringOrdinalIgnoreCase, string, Option<string>>((RigKey, Some(rig.ToString("D")))))
        from added in TableOps.WithAttributes(doc.CreateDefaultAttributes, None, attributes => from edited in AttributeOps.Apply(doc, attributes, joined.Add(keyed))
                                                                                               from light in Lights.Apply(doc, new LightOp.Add(
                                                                                                   create.Placed.Spec(caption(create.Placed.Role), enabled: true, watts: 0d, shadow: 1d), Some(attributes)))
                                                                                               select light)
        select added;

    private static IO<Unit> Moved(RhinoDoc doc, RigChange.Place place, Func<LightRole, string> caption) =>
        new ComponentRef<LightObject>.ById(place.Light) switch {
            var address =>
                from held in Lights.Read(doc, address)
                from moved in Lights.Apply(doc, new LightOp.Modify(
                    address, place.Placed.Spec(caption(place.Placed.Role), held.Spec.Enabled, held.Spec.Watts, held.Spec.ShadowIntensity)))
                select unit,
        };

    // --- [FOLLOW]
    public static IO<Unit> Follow(RhinoDoc doc, LightRig rig, RigCommit commit, IPlugInSink sink, Action<IDisposable> retain) =>
        rig.Target.Switch(
            (Document: doc, Rig: rig, Commit: commit, Sink: sink, Retain: retain),
            subjects: static (state, subjects) => from initial in Target(state.Document, subjects)
                                                  from acquisition in IO.lift(() => Watching(state, subjects, initial))
                                                  from followed in acquisition
                                                  select followed,
            @fixed: static (_, _) => IO.pure(unit));

    private static IO<Unit> Watching(
        (RhinoDoc Document, LightRig Rig, RigCommit Commit, IPlugInSink Sink, Action<IDisposable> Retain) state, RigTarget.Subjects subjects, RigTarget.Fixed initial) {
        Atom<(RigTarget.Fixed Target, LightRig Rig)> previous = Atom((initial, state.Rig));
        LanguageExt.HashSet<Guid> ids = toHashSet(subjects.Ids);
        uint serial = state.Document.RuntimeSerialNumber;
        CallbackSite site = new(state.Sink, typeof(LightRigs), nameof(Follow));
        Seq<HostEvent<(Unit, Unit)>> events = Seq(
            EventKind.ReplaceRhinoObject.In(serial).Choose(args => Callbacks.Found(ids.Contains(args.ObjectId), (unit, unit))),
            EventKind.DeleteRhinoObject.In(serial).Choose(args => Callbacks.Found(ids.Contains(args.ObjectId), (unit, unit))),
            EventKind.UndeleteRhinoObject.In(serial).Choose(args => Callbacks.Found(ids.Contains(args.ObjectId), (unit, unit))),
            EventKind.TransformObjects.In(serial).Choose(args => Callbacks.Found(args.ObjectIds.Exists(ids.Contains) || args.GripOwnerIds.Exists(ids.Contains), (unit, unit))));
        IO<Unit> Resync(HashMap<Unit, Unit> _) =>
            from target in Target(state.Document, state.Rig.Target)
            from held in IO.lift(() => previous.Value)
            from changed in when(held.Target != target, Reconcile(state.Document, held.Rig with { Target = target }, state.Commit, state.Sink)
                .Bind(committed => previous.SwapIO(_ => (target, committed.Value with { Target = state.Rig.Target }))).Map(unit)).As()
            select unit;
        return DisposalOps.Acquire(
            owned => from mailbox in Subscriptions.Idle<Unit, Unit>(Resync)(site, owned)
                     from watched in events.TraverseM(row => row.Inline(mailbox, state.Sink, owned)).As()
                     select unit,
            state.Retain);
    }
}

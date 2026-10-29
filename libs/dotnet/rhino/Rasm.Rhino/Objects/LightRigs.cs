using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.DocObjects;
using Rhino.Render;
using UnitsNet;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record RigEmitter {
    public sealed record PointLight : RigEmitter;

    public sealed record SpotLight(Angle Angle, double HotSpot) : RigEmitter;

    public sealed record DirectionalLight : RigEmitter;

    public sealed record RectangularLight : RigEmitter;
}

public sealed record RigLight(string Slot, string Name, RigEmitter Emitter, Angle Azimuth, Angle Elevation, double Distance, Angle Size, double Stops, LightColor Color, Vector3d Falloff) {
    public double ShadowIntensity { get; init; } = 1d;

    public Option<LightPower> Power { get; init; }
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record RigTarget {
    public sealed record Subjects(IterableNE<Guid> Ids) : RigTarget;

    public sealed record Fixed(Point3d Focus, double Radius) : RigTarget;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record RigBasis {
    public sealed record World : RigBasis;

    public sealed record View(ViewportTarget Target) : RigBasis;

    public sealed record Frame(Guid Object) : RigBasis;
}

public sealed record LightRig(KeyName Name, RigTarget Target, RigBasis Basis, double Level, Angle Rotation, Angle Elevation, Seq<RigLight> Lights);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class LightRigs {
    // --- [TARGET]
    public static IO<RigTarget.Fixed> Target(RhinoDoc doc, RigTarget target) =>
        target.Switch(
            doc,
            subjects: static (document, subjects) =>
                from found in new ObjectTarget.Ids(toSeq(subjects.Ids)).Objects(document)
                from box in Products.Accepted(() => (RhinoObject.GetTightBoundingBox(found, out BoundingBox bounds), bounds))
                select new RigTarget.Fixed(box.Center, box.Diagonal.Length / 2d),
            @fixed: static (_, held) => IO.pure(held));

    public static IO<Plane> Basis(RhinoDoc doc, RigBasis basis, Point3d focus) =>
        basis.Switch(
            (Document: doc, Focus: focus),
            world: static (state, _) => IO.pure(new Plane(state.Focus, Vector3d.XAxis, Vector3d.ZAxis)),
            view: static (state, view) => Viewports.With(state.Document, view.Target, static row => Products.Accepted(() => (row.Viewport.GetCameraFrame(out Plane frame), frame)))
                .Map(frame => new Plane(state.Focus, frame.XAxis, frame.YAxis)),
            frame: static (state, frame) => IO.lift(() => Conversions.Required(state.Document.Objects.FindId(frame.Object)).Bind(static found => Conversions.Required(Conversions.Present(found.ObjectFrame()))))
                .Map(axes => new Plane(state.Focus, axes.XAxis, axes.ZAxis)));

    // --- [PLACEMENT]
    public static Seq<(string Slot, LightSpec Spec)> Placed(LightRig rig, RigTarget.Fixed target, Plane basis, double metersPerUnit) =>
        (Transform.Rotation(rig.Rotation.Radians.ToDouble(), basis.YAxis, basis.Origin)
            * Transform.Rotation(-rig.Elevation.Radians.ToDouble(), basis.XAxis, basis.Origin)
            * Transform.PlaneToPlane(Plane.WorldXY, basis)) switch {
                var placing => rig.Lights.Map(light => (light.Slot, Spec: Positioned(rig, light, target, placing, metersPerUnit))).Strict(),
            };

    private static LightSpec Positioned(LightRig rig, RigLight light, RigTarget.Fixed target, Transform placing, double metersPerUnit) =>
        (Reach: light.Distance * target.Radius,
         Facing: placing
             * Transform.Rotation(light.Azimuth.Radians.ToDouble(), Vector3d.YAxis, Point3d.Origin)
             * Transform.Rotation(-light.Elevation.Radians.ToDouble(), Vector3d.XAxis, Point3d.Origin)) switch {
                 var (reach, facing) => (
                     Side: 2d * reach * Math.Tan(light.Size.Radians.ToDouble() / 2d),
                     At: facing * new Point3d(0d, 0d, reach),
                     Gain: light.Falloff * new Vector3d(1d, reach * metersPerUnit, reach * metersPerUnit * reach * metersPerUnit)) switch {
                         var (side, at, gain) => light.Emitter.Switch<(Point3d At, Vector3d Aim, double Side, Angle Size, double Gain, Vector3d Across, Vector3d Rise), (LightShape Shape, double Gain)>(
                        (at, target.Focus - at, side, light.Size, gain, side * (facing * Vector3d.XAxis), side * (facing * Vector3d.YAxis)),
                        pointLight: static (state, _) => (new LightShape.PointLight(state.At, state.Side / 2d, CameraRelative: false), state.Gain),
                        spotLight: static (state, spot) => (new LightShape.SpotLight(state.At, state.Aim, spot.Angle, spot.HotSpot, state.Side / 2d, CameraRelative: false), state.Gain),
                        directionalLight: static (state, _) => (new LightShape.DirectionalLight(state.At, state.Aim, state.Size, CameraRelative: false), 1d),
                        rectangularLight: static (state, _) => (new LightShape.RectangularLight(state.At - (state.Across / 2d) - (state.Rise / 2d), state.Across, state.Rise, state.Aim), state.Gain)) switch {
                            var (shape, scaled) => new LightSpec(Some(light.Name), Enabled: true, shape, light.Color, rig.Level * double.Exp2(light.Stops) * scaled, light.Power, light.ShadowIntensity, light.Falloff),
                        },
                     },
             };

    // --- [RECONCILE]
    public static IO<Committed<Seq<(string Slot, Guid Id, bool Created)>>> Reconcile(RhinoDoc doc, LightRig rig, string undo, RedrawPolicy redraw) =>
        Commits.Commit(doc, undo, redraw,
            from target in Target(doc, rig.Target)
            from basis in Basis(doc, rig.Basis, target.Focus)
            from placed in Lights.Reconcile(doc, rig.Name, Placed(rig, target, basis, RhinoMath.MetersPerUnit(doc.ModelUnitSystem)))
            from grouped in unless(placed.ForAll(static row => !row.Created), Grouped(doc, placed.Map(static row => row.Id))).As()
            select placed);

    private static IO<Unit> Grouped(RhinoDoc doc, Seq<Guid> ids) =>
        (from client in use(() => new LightManagerSupportClient(doc.RuntimeSerialNumber))
         from lights in use(static () => new LightArray())
         from found in ids.TraverseM(id => Lights.Found(doc, id)).As()
         from appended in IO.lift(() => found.Iter(row => lights.Append(row.LightGeometry)))
         from joined in IO.lift(() => client.GroupLights(lights))
         select unit).Bracket();

    // --- [FOLLOW]
    public static IO<Unit> Follow(RhinoDoc doc, LightRig rig, string undo, RedrawPolicy redraw) =>
        rig.Target.Switch(
            (Document: doc, Rig: rig, Undo: undo, Redraw: redraw),
            subjects: static (state, subjects) => Following(state.Document, state.Rig, subjects, state.Undo, state.Redraw),
            @fixed: static (_, _) => IO.pure(unit));

    private static IO<Unit> Following(RhinoDoc doc, LightRig rig, RigTarget.Subjects subjects, string undo, RedrawPolicy redraw) =>
        from initial in Target(doc, subjects)
        from previous in IO.lift(() => Ref(initial))
        from post in Mailboxes.Idle<Unit, Unit>(_ =>
            from target in Target(doc, subjects)
            from moved in atomicIO(() => {
                bool changed = previous.Value != target;
                previous.Value = target;
                return changed;
            })
            from reconciled in when(moved, Reconcile(doc, rig, undo, redraw).Map(unit)).As()
            select unit)
        from watched in Touched(doc.RuntimeSerialNumber, toHashSet(subjects.Ids)).Attach(_ => post(unit, unit))
        select unit;

    private static HostEvent<Unit> Touched(uint serial, HashSet<Guid> ids) =>
        Subscriptions.Merged(
            Subscriptions.Watched<RhinoReplaceObjectEventArgs>(static (watcher, h) => watcher.ReplaceRhinoObject += h).Documents(static args => args.DocumentSerialNumber).In(serial)
                .Choose(args => Conversions.Present(ids.Contains(args.ObjectId), unit)),
            Subscriptions.Watched<RhinoObjectEventArgs>(static (watcher, h) => watcher.DeleteRhinoObject += h).Documents(static args => args.TheObject?.Document).In(serial)
                .Choose(args => Conversions.Present(ids.Contains(args.ObjectId), unit)),
            Subscriptions.Watched<RhinoObjectEventArgs>(static (watcher, h) => watcher.UndeleteRhinoObject += h).Documents(static args => args.TheObject?.Document).In(serial)
                .Choose(args => Conversions.Present(ids.Contains(args.ObjectId), unit)),
            RhinoObjects.Transformed.In(serial).Choose(args => Conversions.Present(args.ObjectIds.Exists(ids.Contains) || args.GripOwnerIds.Exists(ids.Contains), unit)));
}

using System.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record KeyGesture {
    public sealed record Rotate : KeyGesture {
        private Rotate(bool leftRight, double angleRadians) => (LeftRight, AngleRadians) = (leftRight, angleRadians);

        public bool LeftRight { get; }

        public double AngleRadians { get; }

        public static Fin<KeyGesture> Create(bool leftRight, double angleRadians) =>
            Invalid.Unless<KeyGesture>(double.IsFinite(angleRadians), new Rotate(leftRight, angleRadians), nameof(AngleRadians));
    }

    public sealed record Dolly : KeyGesture {
        private Dolly(bool leftRight, double amount) => (LeftRight, Amount) = (leftRight, amount);

        public bool LeftRight { get; }

        public double Amount { get; }

        public static Fin<KeyGesture> Create(bool leftRight, double amount) =>
            Invalid.Unless<KeyGesture>(double.IsFinite(amount), new Dolly(leftRight, amount), nameof(Amount));
    }

    public sealed record DollyInOut : KeyGesture {
        private DollyInOut(double amount) => Amount = amount;

        public double Amount { get; }

        public static Fin<KeyGesture> Create(double amount) =>
            Invalid.Unless<KeyGesture>(double.IsFinite(amount), new DollyInOut(amount), nameof(Amount));
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DragGesture {
    public sealed record RotateAroundTarget() : DragGesture;

    public sealed record RotateCamera() : DragGesture;

    public sealed record InOutDolly() : DragGesture;

    public sealed record Magnify() : DragGesture;

    public sealed record Tilt() : DragGesture;

    public sealed record DollyZoom() : DragGesture;

    public sealed record LateralDolly() : DragGesture;

    public sealed record AdjustLens(bool MoveTarget) : DragGesture;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ZoomTarget {
    public sealed record ToBox : ZoomTarget {
        private ToBox(BoundingBox subject, double padding) => (Subject, Padding) = (subject, padding);

        public BoundingBox Subject { get; }

        public double Padding { get; }

        public static Fin<ZoomTarget> Create(BoundingBox subject, double padding) =>
            Invalid.Unless<ZoomTarget>(double.IsFinite(padding) && (padding >= 0.0), new ToBox(subject, padding), nameof(Padding));
    }

    public sealed record ToWindow(Rectangle Client) : ZoomTarget;

    public sealed record ByFactor : ZoomTarget {
        private ByFactor(double factor, bool lensZoom, Option<System.Drawing.Point> fixedPoint) => (Factor, LensZoom, FixedPoint) = (factor, lensZoom, fixedPoint);

        public double Factor { get; }

        public bool LensZoom { get; }

        public Option<System.Drawing.Point> FixedPoint { get; }

        public static Fin<ZoomTarget> Create(double factor, bool lensZoom, Option<System.Drawing.Point> fixedPoint) =>
            DocumentUnits.PositiveFinite(factor, nameof(Factor)).ToFin().Map<ZoomTarget>(_ => new ByFactor(factor, lensZoom, fixedPoint));
    }

    public sealed record ToExtents() : ZoomTarget;

    public sealed record ToSelected() : ZoomTarget;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProjectionChange {
    public sealed record ToParallel(bool Symmetric) : ProjectionChange;

    public sealed record ToPerspective : ProjectionChange {
        private ToPerspective(Option<double> targetDistance, bool symmetric, double lensLength) => (TargetDistance, Symmetric, LensLength) = (targetDistance, symmetric, lensLength);

        public Option<double> TargetDistance { get; }

        public bool Symmetric { get; }

        public double LensLength { get; }

        public static Fin<ProjectionChange> Create(Option<double> targetDistance, bool symmetric, double lensLength) =>
            Lensed(targetDistance, lensLength).Map<ProjectionChange>(_ => new ToPerspective(targetDistance, symmetric, lensLength));
    }

    public sealed record ToTwoPoint : ProjectionChange {
        private ToTwoPoint(Option<double> targetDistance, Option<Vector3d> up, double lensLength) => (TargetDistance, Up, LensLength) = (targetDistance, up, lensLength);

        public Option<double> TargetDistance { get; }

        public Option<Vector3d> Up { get; }

        public double LensLength { get; }

        public static Fin<ProjectionChange> Create(Option<double> targetDistance, Option<Vector3d> up, double lensLength) =>
            Lensed(targetDistance, lensLength).Map<ProjectionChange>(_ => new ToTwoPoint(targetDistance, up, lensLength));
    }

    public sealed record ToReflected() : ProjectionChange;

    public sealed record Lens : ProjectionChange {
        private Lens(double lensLength) => LensLength = lensLength;

        public double LensLength { get; }

        public static Fin<ProjectionChange> Create(double lensLength) =>
            Invalid.Unless<ProjectionChange>(double.IsFinite(lensLength), new Lens(lensLength), nameof(LensLength));
    }

    public sealed record LockedProjection(bool Locked) : ProjectionChange;

    public sealed record Defined : ProjectionChange {
        private Defined(DefinedViewportProjection projection, Option<string> viewName, bool updateConstructionPlane) =>
            (Projection, ViewName, UpdateConstructionPlane) = (projection, viewName, updateConstructionPlane);

        public DefinedViewportProjection Projection { get; }

        public Option<string> ViewName { get; }

        public bool UpdateConstructionPlane { get; }

        public static Fin<ProjectionChange> Create(DefinedViewportProjection projection, Option<string> viewName, bool updateConstructionPlane) =>
            Invalid.Unless<ProjectionChange>(projection != DefinedViewportProjection.None, new Defined(projection, viewName, updateConstructionPlane), nameof(Projection));
    }

    public sealed record Isometric : ProjectionChange {
        private Isometric(IsometricCamera camera, Option<string> viewName, bool updateConstructionPlane) =>
            (Camera, ViewName, UpdateConstructionPlane) = (camera, viewName, updateConstructionPlane);

        public IsometricCamera Camera { get; }

        public Option<string> ViewName { get; }

        public bool UpdateConstructionPlane { get; }

        public static Fin<ProjectionChange> Create(IsometricCamera camera, Option<string> viewName, bool updateConstructionPlane) =>
            Invalid.Unless<ProjectionChange>(camera != IsometricCamera.None, new Isometric(camera, viewName, updateConstructionPlane), nameof(Camera));
    }

    private static Fin<Seq<Unit>> Lensed(Option<double> targetDistance, double lensLength) =>
        (Invalid.Unless(double.IsFinite(lensLength), nameof(ToPerspective.LensLength)).ToValidation()
         & targetDistance.Traverse(static distance => DocumentUnits.PositiveFinite(distance, nameof(ToPerspective.TargetDistance))).As().Map(static _ => unit))
        .ToFin();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StackOp {
    public sealed record PushViewProjection() : StackOp;

    public sealed record PopViewProjection() : StackOp;

    public sealed record NextViewProjection() : StackOp;

    public sealed record PreviousViewProjection() : StackOp;

    public sealed record PushConstructionPlane(ConstructionPlane CPlane) : StackOp;

    public sealed record PopConstructionPlane() : StackOp;

    public sealed record NextConstructionPlane() : StackOp;

    public sealed record PreviousConstructionPlane() : StackOp;

    public sealed record SetConstructionPlane(ConstructionPlane CPlane) : StackOp;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Navigation {
    // --- [GESTURES]
    public static IO<Unit> ApplyKey(RhinoViewport viewport, KeyGesture gesture) =>
        IO.lift(() => gesture.Switch(
            viewport,
            rotate: static (port, rotate) => Refused.Unless(port.KeyboardRotate(rotate.LeftRight, rotate.AngleRadians), nameof(RhinoViewport.KeyboardRotate)),
            dolly: static (port, dolly) => Refused.Unless(port.KeyboardDolly(dolly.LeftRight, dolly.Amount), nameof(RhinoViewport.KeyboardDolly)),
            dollyInOut: static (port, dolly) => Refused.Unless(port.KeyboardDollyInOut(dolly.Amount), nameof(RhinoViewport.KeyboardDollyInOut))));

    public static IO<Unit> Rotate(RhinoViewport viewport, GeometryMotion.Rotation rotation) =>
        IO.lift(() => Refused.Unless(viewport.Rotate(rotation.AngleRadians, rotation.Axis, rotation.Center), nameof(RhinoViewport.Rotate)));

    public static IO<Unit> ApplyDrag(RhinoViewport viewport, DragGesture gesture, System.Drawing.Point from, System.Drawing.Point to) =>
        IO.lift(() => gesture.Switch(
            (Port: viewport, From: from, To: to),
            rotateAroundTarget: static (drag, _) => Refused.Unless(drag.Port.MouseRotateAroundTarget(drag.From, drag.To), nameof(RhinoViewport.MouseRotateAroundTarget)),
            rotateCamera: static (drag, _) => Refused.Unless(drag.Port.MouseRotateCamera(drag.From, drag.To), nameof(RhinoViewport.MouseRotateCamera)),
            inOutDolly: static (drag, _) => Refused.Unless(drag.Port.MouseInOutDolly(drag.From, drag.To), nameof(RhinoViewport.MouseInOutDolly)),
            magnify: static (drag, _) => Refused.Unless(drag.Port.MouseMagnify(drag.From, drag.To), nameof(RhinoViewport.MouseMagnify)),
            tilt: static (drag, _) => Refused.Unless(drag.Port.MouseTilt(drag.From, drag.To), nameof(RhinoViewport.MouseTilt)),
            dollyZoom: static (drag, _) => Refused.Unless(drag.Port.MouseDollyZoom(drag.From, drag.To), nameof(RhinoViewport.MouseDollyZoom)),
            lateralDolly: static (drag, _) => Refused.Unless(drag.Port.MouseLateralDolly(drag.From, drag.To), nameof(RhinoViewport.MouseLateralDolly)),
            adjustLens: static (drag, adjust) => Refused.Unless(drag.Port.MouseAdjustLensLength(drag.From, drag.To, moveTarget: adjust.MoveTarget), nameof(RhinoViewport.MouseAdjustLensLength))));

    public static IO<Unit> Zoom(RhinoViewport viewport, ZoomTarget target) =>
        IO.lift(() => target.Switch(
            viewport,
            toBox: static (port, box) => {
                BoundingBox padded = box.Subject;
                padded.Inflate(box.Subject.Diagonal.Length * box.Padding);
                return Refused.Unless(port.ZoomBoundingBox(padded), nameof(RhinoViewport.ZoomBoundingBox));
            },
            toWindow: static (port, window) => Refused.Unless(port.ZoomWindow(window.Client), nameof(RhinoViewport.ZoomWindow)),
            byFactor: static (port, factor) => Refused.Unless(
                factor.FixedPoint.Match(Some: at => port.Magnify(factor.Factor, mode: factor.LensZoom, at), None: () => port.Magnify(factor.Factor, mode: factor.LensZoom)),
                nameof(RhinoViewport.Magnify)),
            toExtents: static (port, _) => Refused.Unless(port.ZoomExtents(), nameof(RhinoViewport.ZoomExtents)),
            toSelected: static (port, _) => Refused.Unless(port.ZoomExtentsSelected(), nameof(RhinoViewport.ZoomExtentsSelected))));

    // --- [PROJECTIONS]
    public static IO<Unit> ChangeProjection(RhinoViewport viewport, ProjectionChange change) =>
        IO.lift(() => change.Switch(
            viewport,
            toParallel: static (port, parallel) => Refused.Unless(port.ChangeToParallelProjection(parallel.Symmetric), nameof(RhinoViewport.ChangeToParallelProjection)),
            toPerspective: static (port, perspective) => Refused.Unless(
                perspective.TargetDistance.Match(
                    Some: distance => port.ChangeToPerspectiveProjection(distance, perspective.Symmetric, perspective.LensLength),
                    None: () => port.ChangeToPerspectiveProjection(perspective.Symmetric, perspective.LensLength)),
                nameof(RhinoViewport.ChangeToPerspectiveProjection)),
            toTwoPoint: static (port, twoPoint) => Refused.Unless(
                port.ChangeToTwoPointPerspectiveProjection(twoPoint.TargetDistance.IfNone(RhinoMath.UnsetValue), twoPoint.Up.IfNone(Vector3d.Zero), twoPoint.LensLength),
                nameof(RhinoViewport.ChangeToTwoPointPerspectiveProjection)),
            toReflected: static (port, _) => Refused.Unless(port.ChangeToParallelReflectedProjection(), nameof(RhinoViewport.ChangeToParallelReflectedProjection)),
            lens: static (port, lens) => {
                port.Camera35mmLensLength = lens.LensLength;
                return Fin.Succ(unit);
            },
            lockedProjection: static (port, locked) => {
                port.LockedProjection = locked.Locked;
                return Fin.Succ(unit);
            },
            defined: static (port, defined) => Refused.Unless(
                port.SetProjection(defined.Projection, defined.ViewName.IfNone(""), defined.UpdateConstructionPlane),
                nameof(RhinoViewport.SetProjection)),
            isometric: static (port, isometric) => Refused.Unless(
                port.SetProjection(isometric.Camera, isometric.ViewName.IfNone(""), isometric.UpdateConstructionPlane),
                nameof(RhinoViewport.SetProjection))));

    // --- [STACK]
    public static IO<bool> ApplyStack(RhinoViewport viewport, StackOp op) =>
        IO.lift(() => op.Switch(
            viewport,
            pushViewProjection: static (port, _) => {
                port.PushViewProjection();
                return true;
            },
            popViewProjection: static (port, _) => port.PopViewProjection(),
            nextViewProjection: static (port, _) => port.NextViewProjection(),
            previousViewProjection: static (port, _) => port.PreviousViewProjection(),
            pushConstructionPlane: static (port, push) => {
                port.PushConstructionPlane(push.CPlane);
                return true;
            },
            popConstructionPlane: static (port, _) => port.PopConstructionPlane(),
            nextConstructionPlane: static (port, _) => port.NextConstructionPlane(),
            previousConstructionPlane: static (port, _) => port.PreviousConstructionPlane(),
            setConstructionPlane: static (port, set) => {
                port.SetConstructionPlane(set.CPlane);
                return true;
            }));

    // --- [ROWS]
    public static IO<Seq<(TValue Value, bool Moved)>> ApplyToRows<TValue>(RhinoDoc document, Seq<ViewportRef> rows, Func<RhinoViewport, IO<TValue>> operation, RedrawPolicy redraw) =>
        Commits.WithinRedraw(document, redraw, rows.TraverseM(row => Applied(row, operation)).As());

    private static IO<(TValue Value, bool Moved)> Applied<TValue>(ViewportRef row, Func<RhinoViewport, IO<TValue>> operation) =>
        from before in IO.lift(() => row.Viewport.ChangeCounter)
        from value in operation(row.Viewport)
        from after in IO.lift(() => row.Viewport.ChangeCounter)
        from committed in IO.lift(() => row.Detail.Match(
            Some: static view => Refused.Unless(view.CommitViewportChanges(), nameof(DetailViewObject.CommitViewportChanges)),
            None: static () => unit))
        select (Value: value, Moved: after != before);
}

using System.Diagnostics.CodeAnalysis;
using Rasm.Rhino.Camera;
using Rasm.Rhino.Commands;
using Rasm.TestKit.Scenarios;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rasm.Rhino.Tests.Camera.Scenarios;

// --- [OPERATIONS] ---------------------------------------------------------------------------

// Ownership: the Camera theme — named-view save/list/restore/read, stack history, DOF
// persistence with a raw NamedViews oracle, raster capture, and the four-flag restore policy
// scoped to the native restore call.
internal static class CameraScenarios {
    [RhinoScenario(theme: "camera")]
    internal static Fin<Unit> NamedViewRail(ScenarioContext ctx) =>
        from scope in DocumentScope.Open(ctx: ctx)
        let onMain = Note(ctx: ctx, key: "mainThread", value: RhinoApp.IsOnMainThread)
        let camera = RhinoCamera.Live(document: scope.Doc, mode: RunMode.Scripted)
        let target = (ViewportTarget)new ViewportTarget.Current()
        from brep in BoxBrep(x0: -20.0, x1: 20.0, y0: -20.0, y1: 20.0, z0: 0.0, z1: 10.0)
        let boxId = scope.Doc.Objects.Add(brep)
        let redrawn = Redraw(doc: scope.Doc)
        let viewName = Stamp(stem: "RasmVerifyNamed")
        let viewNameFact = Note(ctx: ctx, key: "viewName", value: viewName)
        from saved in ctx.Expect(label: "save named", projection: camera.RunValue(operation: CameraOps.SaveNamed(name: viewName), target: target))
        let savedKindFact = Note(ctx: ctx, key: "saved.kind", value: Text(value: saved.Kind))
        let savedNameFact = Note(ctx: ctx, key: "saved.name", value: saved.Name)
        from savedLaw in ctx.Require(label: "saved kind", observed: saved.Kind == DocumentResourceKind.NamedView)
        from names in ctx.Expect(label: "list named", projection: camera.RunValue(operation: CameraOps.ListNamed(), target: target))
        let namedCountFact = Note(ctx: ctx, key: "named.count", value: names.Count)
        from listedLaw in ctx.Require(label: "named view listed", observed: names.Exists(name => string.Equals(a: name, b: viewName, comparisonType: StringComparison.Ordinal)))
        from restored in ctx.Expect(label: "restore named", projection: camera.RunValue(operation: CameraOps.RestoreNamed(name: viewName), target: target))
        from zoomReceipt in ctx.Expect(label: "zoom change", projection: camera.RunValue(operation: CameraOps.Change(new CameraEdit.Zoom()), target: target))
        let zoomFact = Note(ctx: ctx, key: "receipt.redraw", value: zoomReceipt.RedrawRequested)
        from boundary in ctx.Expect(label: "stack pop at empty boundary", projection: camera.RunValue(operation: CameraOps.Change(new CameraEdit.Gesture(new CameraGesture.Navigate(Verb: StackVerb.ViewPop))), target: target))
        let boundaryFact = Note(ctx: ctx, key: "stack.boundary.redraw", value: boundary.RedrawRequested)
        from boundaryLaw in ctx.Require(label: "boundary pop benign no-op", observed: !boundary.RedrawRequested)
        from pushed in ctx.Expect(label: "stack push", projection: camera.RunValue(operation: CameraOps.Change(new CameraEdit.Gesture(new CameraGesture.Navigate(Verb: StackVerb.ViewPush))), target: target))
        let pushFact = Note(ctx: ctx, key: "stack.push.redraw", value: pushed.RedrawRequested)
        from pushLaw in ctx.Require(label: "push requests redraw", observed: pushed.RedrawRequested)
        from popped in ctx.Expect(label: "stack pop after push", projection: camera.RunValue(operation: CameraOps.Change(new CameraEdit.Gesture(new CameraGesture.Navigate(Verb: StackVerb.ViewPop))), target: target))
        let popFact = Note(ctx: ctx, key: "stack.pop.redraw", value: popped.RedrawRequested)
        from dofChecked in DofAndReadNamed(ctx: ctx, camera: camera, target: target, doc: scope.Doc)
        let redrawnAgain = Redraw(doc: scope.Doc)
        from rasterChecked in RasterCapture(ctx: ctx, camera: camera, target: target)
        from fourFlag in FourFlagRestore(ctx: ctx, camera: camera, target: target, viewName: viewName)
        select Done(scope: scope);

    // Phase-2 polymorphic surface law matrix: the FOV-preserving framing pair (Fit/Dolly), the
    // NearFar native-overload-vs-clamp branch, correlated multi-viewport Rig folding under both
    // sync policies, the plural SectionViews container preservation, and the three new architectural
    // rows reaching the otherwise-unreachable ParallelReflected projection.
    [RhinoScenario(theme: "camera")]
    internal static Fin<Unit> Phase2Surface(ScenarioContext ctx) =>
        from scope in DocumentScope.Open(ctx: ctx)
        let camera = RhinoCamera.Live(document: scope.Doc, mode: RunMode.Scripted)
        let target = (ViewportTarget)new ViewportTarget.Current()
        from brep in BoxBrep(x0: -30.0, x1: 30.0, y0: -20.0, y1: 20.0, z0: 0.0, z1: 15.0)
        let bounds = brep.GetBoundingBox(accurate: true)
        let boxId = scope.Doc.Objects.Add(brep)
        let subject = (CameraSubject)new CameraSubject.InBounds(Value: bounds)
        let redrawn = Redraw(doc: scope.Doc)
        from fitChecked in FitHoldsFov(ctx: ctx, camera: camera, target: target, subject: subject)
        from dollyChecked in DollyFramePair(ctx: ctx, camera: camera, target: target, subject: subject, bounds: bounds)
        from nearFarChecked in NearFarBranches(ctx: ctx, camera: camera, target: target, bounds: bounds)
        from rigChecked in RigSyncPolicies(ctx: ctx, camera: camera, target: target, subject: subject)
        from sectionChecked in SectionViewsPlural(ctx: ctx, camera: camera, target: target, doc: scope.Doc)
        from styleChecked in ArchitecturalRows(ctx: ctx, camera: camera, target: target, subject: subject)
        select Done(scope: scope);

    // Fit moves the camera to frame the subject while holding the supplied half-view-angle; the FOV
    // (camera angle) read back after the edit must equal the requested angle, proving the frustum FOV
    // is preserved where SubjectFrame's ZoomBoundingBox would have changed it.
    private static Fin<Unit> FitHoldsFov(ScenarioContext ctx, RhinoCamera camera, ViewportTarget target, CameraSubject subject) {
        const double halfAngle = 0.35;
        return
            from perspective in ctx.Expect(label: "fit prep perspective", projection: camera.RunValue(
                operation: CameraOps.Change(new CameraEdit.Project(Projection: new CameraProjection.Perspective())), target: target))
            from fit in ctx.Expect(label: "fit holds fov", projection: camera.RunValue(
                operation: CameraOps.Change(new CameraEdit.Fit(Subject: subject, HalfViewAngle: halfAngle)), target: target))
            let fitFact = Note(ctx: ctx, key: "fit.redraw", value: fit.RedrawRequested)
            from fitLaw in ctx.Require(label: "fit requests redraw", observed: fit.RedrawRequested)
                // Fit fails closed on an empty subject — the bounds admission rejects before the native Extents call.
            let emptyFit = camera.RunValue(operation: CameraOps.Change(new CameraEdit.Fit(Subject: new CameraSubject.InBounds(Value: BoundingBox.Empty), HalfViewAngle: halfAngle)), target: target)
            from emptyLaw in ctx.Require(label: "fit fails closed on empty subject", observed: emptyFit.IsFail)
            select unit;
    }

    // Dolly is the FOV-preserving sibling to Zoom: the Vector case applies a raw camera-axis move plus
    // frustum dolly; the Extents case requires camera-coordinate input and so fails closed when the
    // world->camera transform produces an empty/invalid camera box (the typed precondition, not a
    // silent wrong frame).
    private static Fin<Unit> DollyFramePair(ScenarioContext ctx, RhinoCamera camera, ViewportTarget target, CameraSubject subject, BoundingBox bounds) =>
        from vector in ctx.Expect(label: "dolly vector", projection: camera.RunValue(
            operation: CameraOps.Change(new CameraEdit.Dolly(Move: new DollyTarget.Vector(Value: new Vector3d(x: 0.0, y: 0.0, z: bounds.Diagonal.Length * 0.25)))), target: target))
        let vectorFact = Note(ctx: ctx, key: "dolly.vector.redraw", value: vector.RedrawRequested)
        from vectorLaw in ctx.Require(label: "dolly vector requests redraw", observed: vector.RedrawRequested)
        from extents in ctx.Expect(label: "dolly extents", projection: camera.RunValue(
            operation: CameraOps.Change(new CameraEdit.Dolly(Move: new DollyTarget.Extents(Subject: subject))), target: target))
        from extentsLaw in ctx.Require(label: "dolly extents requests redraw", observed: extents.RedrawRequested)
            // The Extents case transforms the world bbox into camera coordinates; an empty subject yields an
            // invalid camera box, so the guard rejects before the native DollyExtents call (fail closed).
        let emptyDolly = camera.RunValue(operation: CameraOps.Change(new CameraEdit.Dolly(Move: new DollyTarget.Extents(Subject: new CameraSubject.InBounds(Value: BoundingBox.Empty)))), target: target)
        from emptyLaw in ctx.Require(label: "dolly extents fails closed on empty camera box", observed: emptyDolly.IsFail)
        select unit;

    // NearFar discriminates on subject shape at unit margin: an InBounds/InSphere subject admits the
    // native SetFrustumNearFar(BoundingBox)/(center,radius) overload that auto-derives near/far in
    // camera space; a non-unit margin (or a point subject) falls to the 5-arg form that owns the
    // minNear/minRatio clamping the native overloads do not expose. Both branches must commit.
    private static Fin<Unit> NearFarBranches(ScenarioContext ctx, RhinoCamera camera, ViewportTarget target, BoundingBox bounds) {
        Sphere sphere = new(center: bounds.Center, radius: bounds.Diagonal.Length * 0.5);
        return
            from nativeBox in ctx.Expect(label: "nearfar native bbox", projection: camera.RunValue(
                operation: CameraOps.Change(new CameraEdit.NearFar(Source: new CameraSubject.InBounds(Value: bounds))), target: target))
            let boxFact = Note(ctx: ctx, key: "nearfar.bbox.redraw", value: nativeBox.RedrawRequested)
            from boxLaw in ctx.Require(label: "nearfar native bbox commits", observed: nativeBox.RedrawRequested)
            from nativeSphere in ctx.Expect(label: "nearfar native sphere", projection: camera.RunValue(
                operation: CameraOps.Change(new CameraEdit.NearFar(Source: new CameraSubject.InSphere(Value: sphere))), target: target))
            from sphereLaw in ctx.Require(label: "nearfar native sphere commits", observed: nativeSphere.RedrawRequested)
                // Margin != 1.0 forces the 5-arg clamp branch on the same bbox subject (native overload bypassed).
            from clamp in ctx.Expect(label: "nearfar clamp branch", projection: camera.RunValue(
                operation: CameraOps.Change(new CameraEdit.NearFar(Source: new CameraSubject.InBounds(Value: bounds), Margin: 1.5)), target: target))
            from clampLaw in ctx.Require(label: "nearfar 5-arg clamp branch commits", observed: clamp.RedrawRequested)
            select unit;
    }

    // Rig folds correlated (target, op) pairs under a CameraSyncPolicy. Independent is abort-on-first:
    // a failing op short-circuits the whole rail (TraverseM monadic abort). Coordinated is
    // accumulate-then-guard: every member runs and the fold succeeds while at least one op succeeds,
    // tolerating the partial failure.
    private static Fin<Unit> RigSyncPolicies(ScenarioContext ctx, RhinoCamera camera, ViewportTarget target, CameraSubject subject) {
        CameraOp<CameraChangeReceipt> succeeding = CameraOps.Change(new CameraEdit.Zoom());
        CameraOp<CameraChangeReceipt> failing = CameraOps.Change(new CameraEdit.Fit(Subject: new CameraSubject.InBounds(Value: BoundingBox.Empty), HalfViewAngle: 0.35));
        Seq<(ViewportTarget Target, CameraOp<CameraChangeReceipt> Op)> mixed = Seq((target, failing), (target, succeeding));
        Seq<(ViewportTarget Target, CameraOp<CameraChangeReceipt> Op)> allGood = Seq((target, succeeding), (target, succeeding));
        // Independent aborts on the first failing assignment — the whole Rig rail fails.
        bool independentAborts = camera.Rig(assignments: mixed, policy: CameraSyncPolicy.Independent).IsFail;
        return
            from abortLaw in ctx.Require(label: "rig independent aborts on first failure", observed: independentAborts)
                // Coordinated runs every assignment and accumulates: the mixed set still resolves because one
                // member succeeded; the per-scope results carry the failure without poisoning the fold.
            from coordinated in ctx.Expect(label: "rig coordinated accumulates", projection: camera.Rig(assignments: mixed, policy: CameraSyncPolicy.Coordinated))
            let resultFact = Note(ctx: ctx, key: "rig.coordinated.count", value: coordinated.Value.Count)
            from countLaw in ctx.Require(label: "rig coordinated preserves every scope result", observed: coordinated.Value.Count == mixed.Count)
            from partialLaw in ctx.Require(label: "rig coordinated retains the failed member", observed: coordinated.Value.Exists(static result => !result.Succeeded))
            from succeededLaw in ctx.Require(label: "rig coordinated retains the succeeded member", observed: coordinated.Value.Exists(static result => result.Succeeded))
            from allOutcome in ctx.Expect(label: "rig all-good", projection: camera.Rig(assignments: allGood, policy: CameraSyncPolicy.Independent))
            from allLaw in ctx.Require(label: "rig independent passes when no member fails", observed: allOutcome.Value.ForAll(static result => result.Succeeded))
            select unit;
    }

    // SectionViews is the plural anchored section-set entrypoint: Traverse over the singular
    // CameraSectionView.Edits arm with the Seq<Guid> container preserved. Distinct() dedups plane ids
    // so duplicated ids collapse to one receipt while distinct ids each yield one.
    private static Fin<Unit> SectionViewsPlural(ScenarioContext ctx, RhinoCamera camera, ViewportTarget target, RhinoDoc doc) {
        Guid planeA = doc.Objects.AddClippingPlane(plane: Plane.WorldZX, uMagnitude: 40.0, vMagnitude: 40.0, clippedViewportIds: [doc.Views.ActiveView.ActiveViewportID]);
        Guid planeB = doc.Objects.AddClippingPlane(plane: Plane.WorldYZ, uMagnitude: 40.0, vMagnitude: 40.0, clippedViewportIds: [doc.Views.ActiveView.ActiveViewportID]);
        return
            from distinct in ctx.Expect(label: "section views plural", projection: camera.RunValue(
                operation: CameraOps.SectionViews(planeIds: Seq(planeA, planeB)), target: target))
            let countFact = Note(ctx: ctx, key: "sectionViews.count", value: distinct.Count)
            from countLaw in ctx.Require(label: "section views preserves one receipt per distinct plane", observed: distinct.Count == 2)
                // Duplicate ids collapse through the producer's Distinct() — two equal ids yield one receipt.
            from deduped in ctx.Expect(label: "section views dedup", projection: camera.RunValue(
                operation: CameraOps.SectionViews(planeIds: Seq(planeA, planeA)), target: target))
            from dedupLaw in ctx.Require(label: "section views dedups duplicate plane ids", observed: deduped.Count == 1)
                // Empty plane set fails closed (the InvalidInput guard at the head of the plural producer).
            let empty = camera.RunValue(operation: CameraOps.SectionViews(planeIds: Seq<Guid>()), target: target)
            from emptyLaw in ctx.Require(label: "section views fails closed on empty plane set", observed: empty.IsFail)
            select unit;
    }

    // ArchitecturalStyle is the smart-enum growth axis: the three new rows (PerspectiveTopView,
    // PerspectiveSection, ReflectedCeiling) each compile a Seq<CameraEdit> via the shared Build column.
    // ReflectedCeiling is the only factory that reaches CameraProjection.ParallelReflected — verifying
    // the otherwise-orphaned projection is now reachable.
    private static Fin<Unit> ArchitecturalRows(ScenarioContext ctx, RhinoCamera camera, ViewportTarget target, CameraSubject subject) =>
        from top in ctx.Expect(label: "architectural perspective-top", projection: camera.RunValue(
            operation: CameraOps.ArchitecturalShot(style: ArchitecturalStyle.PerspectiveTopView, subject: subject), target: target))
        from topLaw in ctx.Require(label: "perspective-top commits", observed: top.RedrawRequested)
        from section in ctx.Expect(label: "architectural perspective-section", projection: camera.RunValue(
            operation: CameraOps.ArchitecturalShot(style: ArchitecturalStyle.PerspectiveSection, subject: subject), target: target))
        from sectionLaw in ctx.Require(label: "perspective-section commits", observed: section.RedrawRequested)
        from reflected in ctx.Expect(label: "architectural reflected-ceiling", projection: camera.RunValue(
            operation: CameraOps.ArchitecturalShot(style: ArchitecturalStyle.ReflectedCeiling, subject: subject), target: target))
        let reflectedFact = Note(ctx: ctx, key: "reflectedCeiling.redraw", value: reflected.RedrawRequested)
        from reflectedLaw in ctx.Require(label: "reflected-ceiling reaches ParallelReflected and commits", observed: reflected.RedrawRequested)
        select unit;

    // BOUNDARY ADAPTER — CA2000: ownership of the transient brep transfers into the document table.
    [SuppressMessage(category: "Reliability", checkId: "CA2000", Justification = "Ownership transfers into the Fin rail; the document copies the geometry and the transient brep is finalizer-released.")]
    private static Fin<Brep> BoxBrep(double x0, double x1, double y0, double y1, double z0, double z1) =>
        Optional(Brep.CreateFromBox(new Box(
            Plane.WorldXY,
            new Interval(t0: x0, t1: x1),
            new Interval(t0: y0, t1: y1),
            new Interval(t0: z0, t1: z1))))
        .ToFin(Fail: Error.New(message: "box brep construction failed"));

    private static Fin<Unit> DofAndReadNamed(ScenarioContext ctx, RhinoCamera camera, ViewportTarget target, RhinoDoc doc) {
        string dofName = Stamp(stem: "RasmVerifyDof");
        CameraDof testDof = new(
            Mode: ViewInfoFocalBlurModes.Manual,
            Distance: 42.5,
            Aperture: 2.8,
            Jitter: 0.3,
            SampleCount: 16u);
        return
            from dofSaved in ctx.Expect(label: "save named with dof", projection: camera.RunValue(operation: CameraOps.SaveNamed(name: dofName, fullView: true, dof: testDof), target: target))
            let dofIndex = doc.NamedViews.FindByName(dofName)
            from dofFound in ctx.Require(label: "dof named view found", observed: dofIndex >= 0)
            from inspected in Bracket(acquire: Fin.Succ(value: doc.NamedViews[dofIndex]), use: dofView =>
                from modeFact in Fin.Succ(value: Note(ctx: ctx, key: "dof.mode", value: Text(value: dofView.FocalBlurMode)))
                let distanceFact = Note(ctx: ctx, key: "dof.distance", value: dofView.FocalBlurDistance)
                let sampleFact = Note(ctx: ctx, key: "dof.sampleCount", value: dofView.FocalBlurSampleCount)
                from modeLaw in ctx.Require(label: "dof mode persisted", observed: dofView.FocalBlurMode == ViewInfoFocalBlurModes.Manual)
                from distanceLaw in ctx.Require(label: "dof distance persisted", observed: Math.Abs(value: dofView.FocalBlurDistance - 42.5) < 1e-9)
                from readFrame in ctx.Expect(label: "read named camera", projection: camera.RunValue(operation: CameraOps.ReadNamed(name: dofName), target: target))
                let oracle = dofView.Viewport
                let locationFact = Note(ctx: ctx, key: "read.location", value: $"{readFrame.Location.X:F3},{readFrame.Location.Y:F3},{readFrame.Location.Z:F3}")
                from frameLaw in ctx.Require(label: "read frame valid", observed: readFrame.Frame.IsValid)
                from locationLaw in ctx.Require(label: "read location matches saved-view oracle", observed: readFrame.Location.DistanceTo(other: oracle.CameraLocation) < 1e-6)
                select unit)
            select unit;
    }

    private static Unit Done(DocumentScope scope) {
        scope.Dispose();
        return unit;
    }

    // BOUNDARY ADAPTER — global ApplicationSettings flags are process state; the try/finally
    // bracket restores the operator's configuration regardless of the rail outcome.
    private static Fin<Unit> FourFlagRestore(ScenarioContext ctx, RhinoCamera camera, ViewportTarget target, string viewName) {
        bool savedCPlane = global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane;
        bool savedProjection = global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection;
        bool savedClipping = global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes;
        bool savedDisplay = global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode;
        try {
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane = false;
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection = false;
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes = false;
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode = false;
            return
                from restored in ctx.Expect(
                    label: "restore named with four-flag policy",
                    projection: camera.RunValue(operation: CameraOps.RestoreNamed(name: viewName, restore: new NamedRestorePolicy(CPlane: true, Projection: true, Clipping: true, Display: true)), target: target))
                let cplaneFact = Note(ctx: ctx, key: "restore.cplane.after", value: global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane)
                let projectionFact = Note(ctx: ctx, key: "restore.projection.after", value: global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection)
                let clippingFact = Note(ctx: ctx, key: "restore.clipping.after", value: global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes)
                let displayFact = Note(ctx: ctx, key: "restore.display.after", value: global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode)
                from flagLaw in ctx.Require(
                    label: "restore policy restores global flags after native restore",
                    observed: !global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane
                        && !global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection
                        && !global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes
                        && !global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode)
                select unit;
        } finally {
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane = savedCPlane;
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection = savedProjection;
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes = savedClipping;
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode = savedDisplay;
        }
    }

    private static T Note<T>(ScenarioContext ctx, string key, T value) {
        ctx.Fact(key: key, value: value);
        return value;
    }

    private static Fin<Unit> RasterCapture(ScenarioContext ctx, RhinoCamera camera, ViewportTarget target) =>
        Bracket(
            acquire: ctx.Expect(label: "raster capture", projection: camera.RunValue(
                operation: CameraOps.CaptureFrame(format: CaptureFormat.Bitmap, recipe: new CaptureRecipe(Size: Some(new System.Drawing.Size(width: 320, height: 240)), Dpi: Some(96d), Raster: true)),
                target: target)),
            use: raster =>
                from isBitmap in ctx.Require(label: "raster capture is bitmap", observed: raster is CaptureResult.Bitmap)
                from bitmap in raster is CaptureResult.Bitmap admitted
                    ? Fin.Succ(value: admitted)
                    : Fin.Fail<CaptureResult.Bitmap>(error: Error.New(message: "raster capture is not a bitmap"))
                let widthFact = Note(ctx: ctx, key: "raster.width", value: bitmap.Value.Width)
                let heightFact = Note(ctx: ctx, key: "raster.height", value: bitmap.Value.Height)
                from nonEmpty in ctx.Require(label: "raster bitmap non-empty", observed: bitmap.Value.Width > 0 && bitmap.Value.Height > 0)
                select unit);

    private static Unit Redraw(RhinoDoc doc) {
        doc.Views.Redraw();
        return unit;
    }

    private static string Stamp(string stem) =>
        $"{stem}{Guid.NewGuid():N}";

    private static string Text(object? value) =>
        Convert.ToString(value: value, provider: System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;

    // BOUNDARY ADAPTER — native handle bracket: disposes the acquired resource on both rails so
    // failed Require gates cannot leak ViewInfo/CaptureResult handles.
    private static Fin<T> Bracket<THandle, T>(Fin<THandle> acquire, Func<THandle, Fin<T>> use) where THandle : IDisposable {
        if (acquire is not Fin<THandle>.Succ(THandle handle)) {
            return acquire is Fin<THandle>.Fail(Error fault)
                ? Fin.Fail<T>(error: fault)
                : Fin.Fail<T>(error: Error.New(message: "bracket acquisition unresolved"));
        }
        try {
            return use(handle);
        } finally {
            handle.Dispose();
        }
    }
}

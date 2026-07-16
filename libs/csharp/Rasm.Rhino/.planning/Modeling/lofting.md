# [RASM_RHINO_MODELING_LOFTING]

`Rasm.Rhino.Modeling` owns sweep, loft, patch, and developable construction. One `LoftOp` union carries one-rail and two-rail sweep modalities, lofting with rebuild, refit, and trim-tangency seeding, direct and variational patch solvers, developable lofting, and ruling adjustment through `Lofts.Build`. `SweepFrameLaw` carries every roadlike frame preset, `SweepFit` carries the rebuild axis, and `VariationalLaw` carries the complete `Brep.VariationalPatchSettings` policy while `Context` supplies every tolerance. Rails, shapes, and constraints enter as leased `GeometryHandle`s; products leave through owned spine handles, and variational diagnostics land as typed receipt facts.

## [01]-[INDEX]

- [02]-[SWEEP_POLICY]: `SweepFrameLaw`, `SweepFit`, `SweepEnds` — the sweep axes as values and the class-engine modality law.
- [03]-[PATCH_POLICY]: `PatchLaw`, `VariationalLaw`, `LoftFit`, `LoftTangency`, `DevelopableLaw`, `RulingSolve` — the fit and solver policies.
- [04]-[OPERATION_RAIL]: `LoftSlot`, `LoftOp`, and the `Lofts.Build` entry.
- [05]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[SWEEP_POLICY]

- Owner: `SweepFrameLaw` `[Union]` — freeform, roadlike top/front/right presets, and an explicit roadlike direction carry the full `SweepOneRail` frame surface; `SweepFit` `[Union]` — `AsIs`, `Rebuild(Points)`, and `Refit(RefitRail)` carry the host's `SweepRebuild` axis while `Context` supplies refit tolerance; `SweepEnds` — optional start and end caps with `Point3d.Unset` spelled once at the seam.
- Law: parameter-keyed one-rail sweeping routes through `SweepOneRail`; parameterless sweeping routes through the frame-driven static. Engine mode rejects static-only end and segmentation inputs instead of ignoring them.
- Law: `SweepTwoStations.Engine` carries station rows and the engine-only `UseLegacySweeper` host switch; `Partitioned` carries paired rail parameters for `CreateFromSweepInParts`; absence selects the static two-rail form. Each modality rejects knobs its native cannot consume.
- Growth: a new host sweep knob is one field on the owning case rigged in the arm; a new fit behavior is one `SweepFit` case.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SweepFrameLaw {
    private SweepFrameLaw() { }
    public sealed record Freeform : SweepFrameLaw;
    public sealed record RoadlikeTop : SweepFrameLaw;
    public sealed record RoadlikeFront : SweepFrameLaw;
    public sealed record RoadlikeRight : SweepFrameLaw;
    public sealed record RoadlikeDirection(Vector3d Normal) : SweepFrameLaw;

    internal (SweepFrame Frame, Vector3d Normal) Native => Switch(
        freeform: static _ => (SweepFrame.Freeform, Vector3d.Unset),
        roadlikeTop: static _ => (SweepFrame.Roadlike, Vector3d.ZAxis),
        roadlikeFront: static _ => (SweepFrame.Roadlike, Vector3d.YAxis),
        roadlikeRight: static _ => (SweepFrame.Roadlike, Vector3d.XAxis),
        roadlikeDirection: static law => (SweepFrame.Roadlike, law.Normal));

    internal Unit Rig(SweepOneRail engine) => Switch(
        state: engine,
        freeform: static _ => unit,
        roadlikeTop: static sweep => { sweep.SetToRoadlikeTop(); return unit; },
        roadlikeFront: static sweep => { sweep.SetToRoadlikeFront(); return unit; },
        roadlikeRight: static sweep => { sweep.SetToRoadlikeRight(); return unit; },
        roadlikeDirection: static (sweep, law) => { sweep.SetRoadlikeUpDirection(up: law.Normal); return unit; });
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SweepFit {
    private SweepFit() { }
    public sealed record AsIs : SweepFit;
    public sealed record Rebuild(int Points) : SweepFit;
    public sealed record Refit(bool RefitRail = false) : SweepFit;

    internal (SweepRebuild Kind, int Points, double Tolerance, bool RefitRail) Native(Context domain) => Switch(
        state: domain,
        asIs: static _ => (SweepRebuild.None, 0, 0.0, false),
        rebuild: static (_, law) => (SweepRebuild.Rebuild, law.Points, 0.0, false),
        refit: static (model, law) => (SweepRebuild.Refit, 0, model.Absolute.Value, law.RefitRail));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LoftFit {
    private LoftFit() { }
    public sealed record AsIs : LoftFit;
    public sealed record Rebuild(int Points) : LoftFit;
    public sealed record Refit : LoftFit;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SweepTwoStations {
    private SweepTwoStations() { }
    public sealed record Engine(Seq<double> Rail1, Seq<double> Rail2, bool UseLegacySweeper = false) : SweepTwoStations;
    public sealed record Partitioned(Seq<Point2d> RailParameters) : SweepTwoStations;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DevelopableLaw {
    private DevelopableLaw() { }
    public sealed record ByDensity(bool Reverse0 = false, bool Reverse1 = false, int Density = 10) : DevelopableLaw;
    public sealed record ByRulings(Seq<Point2d> FixedRulings) : DevelopableLaw;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RulingSolve {
    private RulingSolve() { }
    public sealed record Local(Interval Domain0, Interval Domain1) : RulingSolve;
    public sealed record MinTwistSecond(Interval Domain1) : RulingSolve;
    public sealed record MinTwistBoth(Interval Domain0, Interval Domain1) : RulingSolve;
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct SweepEnds(Option<Point3d> Start = default, Option<Point3d> End = default) {
    internal Point3d StartOrUnset => Start.IfNone(Point3d.Unset);
    internal Point3d EndOrUnset => End.IfNone(Point3d.Unset);
}
```

## [03]-[PATCH_POLICY]

- Owner: `PatchLaw` — the direct patch solver's complete policy surface: spans, trim, tangency, point spacing, flexibility, surface pull, and four fixed-edge grants; `VariationalLaw` — the complete `Brep.VariationalPatchSettings` surface with context-rigged tolerances; `LoftTangency` — trim-seeded loft ends as one optional record; `RulingSolve` `[Union]` — local, minimum-twist second-parameter, and minimum-twist two-parameter solves.
- Law: constraint continuity rides the row — a variational edge or interior curve enters as `(Handle, Continuity)` and the arm mints `Brep.CurveConstraint` per row inside the borrow, so the constraint carrier never crosses a case payload.
- Law: the solver verdict is evidence — `Brep.VariationalPatchResult` folds to `Text` facts for its warning and error channels and `Flag` facts for the `G0Int`/`G0`/`G1`/`G2` continuity verdicts, so a patch that succeeded with degraded continuity is distinguishable from a clean solve without re-running it.
- Law: an initial surface is leased policy — `VariationalLaw.InitialSurface` and the direct patch's starting surface are optional handles borrowed only for the solve window.

```csharp
// --- [MODELS] -----------------------------------------------------------------------------
public sealed record PatchLaw(
    int USpans,
    int VSpans,
    bool Trim = true,
    bool Tangency = true,
    double PointSpacing = 1.0,
    double Flexibility = 1.0,
    double SurfacePull = 1.0,
    bool FixNorth = false,
    bool FixEast = false,
    bool FixSouth = false,
    bool FixWest = false) {
    internal bool[] FixedEdges => [FixNorth, FixEast, FixSouth, FixWest];
}

public sealed record VariationalLaw(
    RhinoVariationalDomain Domain = RhinoVariationalDomain.Molded,
    int DegreeU = 5,
    int DegreeV = 5,
    int SpanCountU = 1,
    int SpanCountV = 1,
    double Stretching = 0.0,
    double Bending = 0.0,
    double RocBending = 1.0,
    double UVRotation = 0.0,
    int MaxRefinements = 5,
    bool PreserveEdges = false,
    Option<GeometryHandle> InitialSurface = default) {
    internal Fin<Brep.VariationalPatchSettings> Rig(Context domain, Option<Surface> initial, Op key) =>
        key.Catch(() => Fin.Succ(value: new Brep.VariationalPatchSettings {
            Tolerance = domain.Absolute.Value,
            AngleToleranceRadians = domain.Angle.Value,
            InternalTolerance = domain.Absolute.Value,
            CurvatureRelativeTolerance = domain.Fractional,
            CurvatureZeroTolerance = domain.Absolute.Value,
            DegreeU = DegreeU,
            DegreeV = DegreeV,
            SpanCountU = SpanCountU,
            SpanCountV = SpanCountV,
            Domain = Domain,
            Stretching = Stretching,
            Bending = Bending,
            RocBending = RocBending,
            UVRotation = UVRotation,
            MaxRefinements = MaxRefinements,
            InitialSurface = initial.IfNoneUnsafe((Surface?)null),
            PreserveEdges = PreserveEdges,
        }));
}

public sealed record LoftTangency(
    GeometryHandle StartOwner, int StartTrim, bool StartTangent,
    GeometryHandle EndOwner, int EndTrim, bool EndTangent);
```

## [04]-[OPERATION_RAIL]

- Owner: `LoftSlot` `[SmartEnum<int>]` — the consequence vocabulary; `LoftOp` `[Union]` — one-rail sweep, two-rail sweep with station modality, loft, patch, variational patch, developable loft, and ruling adjustment as one verb family; `Lofts` — the one entry folding any operation spread into one `Built<LoftSlot>`.
- Law: the class engine is rigged, consumed, and dropped inside the arm — `SweepOneRail`/`SweepTwoRail` never cross a case payload; the case carries values, the arm constructs the engine, sets its columns from the regime and the case, performs, and owns the products.
- Law: loft tolerance overloads collapse — `Context` supplies angle and refit tolerances, `LoftFit` selects plain, rebuild, or refit construction, and `LoftTangency` selects the trim-seeded form with trims indexed inside the borrows.
- Law: ruling adjustment is evidence — `UntwistRulings` answers the adjusted uv rows as one `UvRows` fact with no geometry product, feeding the `ByRulings` developable case the caller composes next.
- Law: cancellation and progress are boundary payload — the variational case carries its `CancellationToken` and optional `IProgress<double>` as case fields consumed only by the native call, so the rail stays synchronous `Fin` while long solves stay interruptible.
- Growth: a new sweep, loft, or patch modality is one case with its arm; the spine and every consumer read it with zero new surface.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class LoftSlot {
    public static readonly LoftSlot Swept = new(key: 0);
    public static readonly LoftSlot Lofted = new(key: 1);
    public static readonly LoftSlot Patched = new(key: 2);
    public static readonly LoftSlot Solved = new(key: 3);
    public static readonly LoftSlot Developed = new(key: 4);
    public static readonly LoftSlot Rulings = new(key: 5);
    public static readonly LoftSlot Warning = new(key: 6);
    public static readonly LoftSlot Error = new(key: 7);
    public static readonly LoftSlot G0Interior = new(key: 8);
    public static readonly LoftSlot G0 = new(key: 9);
    public static readonly LoftSlot G1 = new(key: 10);
    public static readonly LoftSlot G2 = new(key: 11);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LoftOp {
    private LoftOp() { }
    public sealed record SweepOne(
        GeometryHandle Rail, Seq<GeometryHandle> Shapes, SweepEnds Ends, SweepFrameLaw Frame,
        bool Closed, SweepBlend Blend, SweepMiter Miter, SweepFit Fit,
        bool Segmented = false, Option<Seq<double>> ShapeParams = default) : LoftOp;
    public sealed record SweepTwo(
        GeometryHandle Rail1, GeometryHandle Rail2, Seq<GeometryHandle> Shapes, SweepEnds Ends,
        bool Closed, SweepFit Fit, bool MaintainHeight = false, bool AutoAdjust = true,
        Option<SweepTwoStations> Stations = default) : LoftOp;
    public sealed record Loft(
        Seq<GeometryHandle> Shapes, SweepEnds Ends, LoftType Kind, bool Closed,
        LoftFit Fit, Option<LoftTangency> Tangency = default) : LoftOp;
    public sealed record Patch(Seq<GeometryHandle> Geometry, Option<GeometryHandle> StartingSurface, PatchLaw Law) : LoftOp;
    public sealed record Variational(
        Seq<(GeometryHandle Curve, Continuity Continuity)> Edges,
        Seq<(GeometryHandle Curve, Continuity Continuity)> InternalCurves,
        Seq<Point3d> Points, VariationalLaw Law,
        bool MultiThreaded = true, CancellationToken Cancel = default, Option<IProgress<double>> Progress = default) : LoftOp;
    public sealed record Developable(GeometryHandle Rail0, GeometryHandle Rail1, DevelopableLaw Law) : LoftOp;
    public sealed record SolveRuling(GeometryHandle Rail0, GeometryHandle Rail1, Point2d Seed, RulingSolve Law) : LoftOp;
    public sealed record AdjustRulings(GeometryHandle Rail0, GeometryHandle Rail1, Seq<Point2d> Rulings) : LoftOp;

    internal Fin<Built<LoftSlot>> Apply(Context domain) =>
        Switch(
            context: domain,
            sweepOne: static (model, edit) => {
                Op op = Op.Of(name: nameof(SweepOne));
                return ModelGate.Borrow<Curve, Built<LoftSlot>>(handle: edit.Rail, key: op, body: rail =>
                    ModelGate.BorrowMany<Curve, Built<LoftSlot>>(handles: edit.Shapes, key: op, body: shapes =>
                        edit.ShapeParams.Case switch {
                            Seq<double> stations => (
                                from _ in guard(
                                    stations.Count == shapes.Count && edit.Ends.Start.IsNone && edit.Ends.End.IsNone && !edit.Segmented
                                    && edit.Fit is not SweepFit.Refit { RefitRail: true },
                                    op.InvalidInput())
                                from built in op.Catch(() => {
                                    SweepOneRail engine = new() {
                                        SweepTolerance = model.Absolute.Value,
                                        AngleToleranceRadians = model.Angle.Value,
                                        ClosedSweep = edit.Closed,
                                        GlobalShapeBlending = edit.Blend == SweepBlend.Global,
                                        MiterType = (int)edit.Miter,
                                    };
                                    _ = edit.Frame.Rig(engine: engine);
                                    (SweepRebuild kind, int points, double refit, _) = edit.Fit.Native(domain: model);
                                    return Swept(op, () => kind switch {
                                        SweepRebuild.Rebuild => engine.PerformSweepRebuild(rail, shapes.AsIterable(), stations.AsIterable(), points),
                                        SweepRebuild.Refit => engine.PerformSweepRefit(rail, shapes.AsIterable(), stations.AsIterable(), refit),
                                        _ => engine.PerformSweep(rail, shapes.AsIterable(), stations.AsIterable()),
                                    });
                                })
                                select built),
                            _ when edit.Segmented && edit.Fit is SweepFit.Refit { RefitRail: true } =>
                                Fin.Fail<Built<LoftSlot>>(error: op.InvalidInput()),
                            _ => op.Catch(() => {
                                (SweepFrame frame, Vector3d normal) = edit.Frame.Native;
                                (SweepRebuild kind, int points, double refit, bool refitRail) = edit.Fit.Native(domain: model);
                                return Swept(op, () => edit.Segmented
                                    ? Brep.CreateFromSweepSegmented(
                                        rail: rail, shapes: shapes.AsIterable(), startPoint: edit.Ends.StartOrUnset, endPoint: edit.Ends.EndOrUnset,
                                        frameType: frame, roadlikeNormal: normal, closed: edit.Closed, blendType: edit.Blend, miterType: edit.Miter,
                                        tolerance: model.Absolute.Value, rebuildType: kind, rebuildPointCount: points, refitTolerance: refit)
                                    : Brep.CreateFromSweep(
                                        rail: rail, shapes: shapes.AsIterable(), startPoint: edit.Ends.StartOrUnset, endPoint: edit.Ends.EndOrUnset,
                                        frameType: frame, roadlikeNormal: normal, closed: edit.Closed, blendType: edit.Blend, miterType: edit.Miter,
                                        tolerance: model.Absolute.Value, rebuildType: kind, rebuildPointCount: points, refitTolerance: refit, refitRail: refitRail));
                            }),
                        }));
            },
            sweepTwo: static (model, edit) => {
                Op op = Op.Of(name: nameof(SweepTwo));
                return ModelGate.Borrow<Curve, Built<LoftSlot>>(handle: edit.Rail1, key: op, body: rail1 =>
                    ModelGate.Borrow<Curve, Built<LoftSlot>>(handle: edit.Rail2, key: op, body: rail2 =>
                        ModelGate.BorrowMany<Curve, Built<LoftSlot>>(handles: edit.Shapes, key: op, body: shapes =>
                            edit.Stations.Case switch {
                                SweepTwoStations.Engine stations => (
                                    from _ in guard(
                                        stations.Rail1.Count == shapes.Count && stations.Rail2.Count == shapes.Count
                                        && edit.Ends.Start.IsNone && edit.Ends.End.IsNone
                                        && edit.Fit is not SweepFit.Refit { RefitRail: true },
                                        op.InvalidInput())
                                    from built in op.Catch(() => {
                                        SweepTwoRail engine = new() {
                                            SweepTolerance = model.Absolute.Value,
                                            AngleToleranceRadians = model.Angle.Value,
                                            ClosedSweep = edit.Closed,
                                            MaintainHeight = edit.MaintainHeight,
                                            AutoAdjust = edit.AutoAdjust,
                                            UseLegacySweeper = stations.UseLegacySweeper,
                                        };
                                        (SweepRebuild kind, int points, double refit, _) = edit.Fit.Native(domain: model);
                                        return Swept(op, () => kind switch {
                                            SweepRebuild.Rebuild => engine.PerformSweepRebuild(
                                                rail1, rail2, shapes.AsIterable(), stations.Rail1.AsIterable(), stations.Rail2.AsIterable(), points),
                                            SweepRebuild.Refit => engine.PerformSweepRefit(
                                                rail1, rail2, shapes.AsIterable(), stations.Rail1.AsIterable(), stations.Rail2.AsIterable(), refit),
                                            _ => engine.PerformSweep(
                                                rail1, rail2, shapes.AsIterable(), stations.Rail1.AsIterable(), stations.Rail2.AsIterable()),
                                        });
                                    })
                                    select built),
                                SweepTwoStations.Partitioned stations => (
                                    from _ in guard(
                                        stations.RailParameters.Count == shapes.Count && edit.Ends.Start.IsNone && edit.Ends.End.IsNone
                                        && edit.Fit is SweepFit.AsIs && !edit.MaintainHeight && edit.AutoAdjust,
                                        op.InvalidInput())
                                    from built in op.Catch(() => Swept(op, () => Brep.CreateFromSweepInParts(
                                        rail1: rail1, rail2: rail2, shapes: shapes.AsIterable(),
                                        rail_params: stations.RailParameters.AsIterable(), closed: edit.Closed, tolerance: model.Absolute.Value)))
                                    select built),
                                _ when edit.Fit is SweepFit.Refit { RefitRail: true } =>
                                    Fin.Fail<Built<LoftSlot>>(error: op.InvalidInput()),
                                _ => op.Catch(() => {
                                    (SweepRebuild kind, int points, double refit, _) = edit.Fit.Native(domain: model);
                                    return Swept(op, () => Brep.CreateFromSweep(
                                        rail1: rail1, rail2: rail2, shapes: shapes.AsIterable(),
                                        start: edit.Ends.StartOrUnset, end: edit.Ends.EndOrUnset, closed: edit.Closed,
                                        tolerance: model.Absolute.Value, rebuild: kind, rebuildPointCount: points, refitTolerance: refit,
                                        preserveHeight: edit.MaintainHeight, autoAdjust: edit.AutoAdjust));
                                }),
                            })));
            },
            loft: static (model, edit) => {
                Op op = Op.Of(name: nameof(Loft));
                return ModelGate.BorrowMany<Curve, Built<LoftSlot>>(handles: edit.Shapes, key: op, body: shapes =>
                    edit.Tangency.Case switch {
                        LoftTangency tangency => ModelGate.Borrow<Brep, Built<LoftSlot>>(handle: tangency.StartOwner, key: op, body: startOwner =>
                            ModelGate.Borrow<Brep, Built<LoftSlot>>(handle: tangency.EndOwner, key: op, body: endOwner =>
                                from _ in guard(
                                    tangency.StartTrim >= 0 && tangency.StartTrim < startOwner.Trims.Count
                                    && tangency.EndTrim >= 0 && tangency.EndTrim < endOwner.Trims.Count
                                    && edit.Fit is LoftFit.AsIs,
                                    op.InvalidInput())
                                from built in op.Catch(() => Lofted(op, () => Brep.CreateFromLoft(
                                    curves: shapes.AsIterable(), start: edit.Ends.StartOrUnset, end: edit.Ends.EndOrUnset,
                                    StartTangent: tangency.StartTangent, EndTangent: tangency.EndTangent,
                                    StartTrim: startOwner.Trims[tangency.StartTrim], EndTrim: endOwner.Trims[tangency.EndTrim],
                                    loftType: edit.Kind, closed: edit.Closed)))
                                select built)),
                        _ => op.Catch(() => Lofted(op, () => edit.Fit switch {
                            LoftFit.Rebuild fit => Brep.CreateFromLoftRebuild(
                                curves: shapes.AsIterable(), start: edit.Ends.StartOrUnset, end: edit.Ends.EndOrUnset,
                                loftType: edit.Kind, closed: edit.Closed, angleTol: model.Angle.Value, rebuildPointCount: fit.Points),
                            LoftFit.Refit => Brep.CreateFromLoftRefit(
                                curves: shapes.AsIterable(), start: edit.Ends.StartOrUnset, end: edit.Ends.EndOrUnset,
                                loftType: edit.Kind, closed: edit.Closed, angleTol: model.Angle.Value, refitTolerance: model.Absolute.Value),
                            _ => Brep.CreateFromLoft(
                                curves: shapes.AsIterable(), start: edit.Ends.StartOrUnset, end: edit.Ends.EndOrUnset,
                                loftType: edit.Kind, closed: edit.Closed, angleTol: model.Angle.Value),
                        })),
                    });
            },
            patch: static (model, edit) => {
                Op op = Op.Of(name: nameof(Patch));
                return ModelGate.BorrowMany<GeometryBase, Built<LoftSlot>>(handles: edit.Geometry, key: op, body: constraints =>
                    edit.StartingSurface.Case switch {
                        GeometryHandle starting => ModelGate.Borrow<Surface, Built<LoftSlot>>(handle: starting, key: op,
                            body: surface => Patched(op, () => Brep.CreatePatch(
                                geometry: constraints.AsIterable(), startingSurface: surface,
                                uSpans: edit.Law.USpans, vSpans: edit.Law.VSpans, trim: edit.Law.Trim, tangency: edit.Law.Tangency,
                                pointSpacing: edit.Law.PointSpacing, flexibility: edit.Law.Flexibility, surfacePull: edit.Law.SurfacePull,
                                fixEdges: edit.Law.FixedEdges, tolerance: model.Absolute.Value))),
                        _ => Patched(op, () => Brep.CreatePatch(
                            geometry: constraints.AsIterable(), startingSurface: null,
                            uSpans: edit.Law.USpans, vSpans: edit.Law.VSpans, trim: edit.Law.Trim, tangency: edit.Law.Tangency,
                            pointSpacing: edit.Law.PointSpacing, flexibility: edit.Law.Flexibility, surfacePull: edit.Law.SurfacePull,
                            fixEdges: edit.Law.FixedEdges, tolerance: model.Absolute.Value)),
                    });
            },
            variational: static (model, edit) => {
                Op op = Op.Of(name: nameof(Variational));
                return ModelGate.BorrowMany<Curve, Built<LoftSlot>>(handles: edit.Edges.Map(static row => row.Curve), key: op, body: edgeCurves =>
                    ModelGate.BorrowMany<Curve, Built<LoftSlot>>(handles: edit.InternalCurves.Map(static row => row.Curve), key: op, allowEmpty: true, body: interiorCurves => {
                        Fin<Built<LoftSlot>> Solve(Option<Surface> initial) =>
                            from settings in edit.Law.Rig(domain: model, initial: initial, key: op)
                            from built in op.Catch(() => {
                                Brep patch = Brep.CreateVariationalPatch(
                                    edges: edgeCurves.Zip(edit.Edges.Map(static row => row.Continuity))
                                        .Map(static pair => new Brep.CurveConstraint(curve: pair.First, continuity: pair.Second)).AsIterable(),
                                    internalCurves: interiorCurves.Zip(edit.InternalCurves.Map(static row => row.Continuity))
                                        .Map(static pair => new Brep.CurveConstraint(curve: pair.First, continuity: pair.Second)).AsIterable(),
                                    points: edit.Points.Map(static point => new Brep.PointConstraint(point: point)).AsIterable(),
                                    settings: settings, multiThreading: edit.MultiThreaded,
                                    cancelToken: edit.Cancel, progress: edit.Progress.IfNoneUnsafe((IProgress<double>?)null),
                                    results: out Brep.VariationalPatchResult verdict);
                                return ModelGate.Own(built: patch, key: op).Map(owned => new Built<LoftSlot>(
                                    Products: Seq(owned),
                                    Evidence: BuildReceipt<LoftSlot>.Of(slot: LoftSlot.Solved, body: new BuildBody.Tally(Count: 1))
                                        + BuildReceipt<LoftSlot>.Of(slot: LoftSlot.Warning, body: new BuildBody.Text(Value: verdict.Warning ?? string.Empty))
                                        + BuildReceipt<LoftSlot>.Of(slot: LoftSlot.Error, body: new BuildBody.Text(Value: verdict.Error ?? string.Empty))
                                        + BuildReceipt<LoftSlot>.Of(slot: LoftSlot.G0Interior, body: new BuildBody.Flag(Value: verdict.G0Int ?? false))
                                        + BuildReceipt<LoftSlot>.Of(slot: LoftSlot.G0, body: new BuildBody.Flag(Value: verdict.G0 ?? false))
                                        + BuildReceipt<LoftSlot>.Of(slot: LoftSlot.G1, body: new BuildBody.Flag(Value: verdict.G1 ?? false))
                                        + BuildReceipt<LoftSlot>.Of(slot: LoftSlot.G2, body: new BuildBody.Flag(Value: verdict.G2 ?? false))));
                            })
                            select built;
                        return edit.Law.InitialSurface.Case switch {
                            GeometryHandle seed => ModelGate.Borrow<Surface, Built<LoftSlot>>(handle: seed, key: op, body: surface => Solve(initial: Some(surface))),
                            _ => Solve(initial: Option<Surface>.None),
                        };
                    }));
            },
            developable: static (_, edit) => {
                Op op = Op.Of(name: nameof(Developable));
                return edit.Law.Switch(
                    state: (Edit: edit, Op: op),
                    byDensity: static (ctx, law) => ModelGate.Borrow<Curve, Built<LoftSlot>>(handle: ctx.Edit.Rail0, key: ctx.Op, body: rail0 =>
                        ModelGate.Borrow<Curve, Built<LoftSlot>>(handle: ctx.Edit.Rail1, key: ctx.Op, body: rail1 =>
                            Developed(ctx.Op, () => Brep.CreateDevelopableLoft(
                                crv0: rail0, crv1: rail1, reverse0: law.Reverse0, reverse1: law.Reverse1, density: law.Density)))),
                    byRulings: static (ctx, law) => ModelGate.Borrow<NurbsCurve, Built<LoftSlot>>(handle: ctx.Edit.Rail0, key: ctx.Op, body: rail0 =>
                        ModelGate.Borrow<NurbsCurve, Built<LoftSlot>>(handle: ctx.Edit.Rail1, key: ctx.Op, body: rail1 =>
                            Developed(ctx.Op, () => Brep.CreateDevelopableLoft(
                                rail0: rail0, rail1: rail1, fixedRulings: law.FixedRulings.AsIterable())))));
            },
            solveRuling: static (_, edit) => {
                Op op = Op.Of(name: nameof(SolveRuling));
                return ModelGate.Borrow<NurbsCurve, Built<LoftSlot>>(handle: edit.Rail0, key: op, body: rail0 =>
                    ModelGate.Borrow<NurbsCurve, Built<LoftSlot>>(handle: edit.Rail1, key: op, body: rail1 =>
                        edit.Law.Switch(
                            state: (Rail0: rail0, Rail1: rail1, Seed: edit.Seed, Op: op),
                            local: static (ctx, law) => ctx.Op.Catch(() => {
                                double t0 = ctx.Seed.X;
                                double t1 = ctx.Seed.Y;
                                int verdict = DevelopableSrf.GetLocalDevopableRuling(
                                    rail0: ctx.Rail0, t0: ctx.Seed.X, dom0: law.Domain0,
                                    rail1: ctx.Rail1, t1: ctx.Seed.Y, dom1: law.Domain1,
                                    t0_out: ref t0, t1_out: ref t1);
                                return Fin.Succ(value: new Built<LoftSlot>(
                                    Products: Seq<GeometryHandle>(),
                                    Evidence: BuildReceipt<LoftSlot>.Of(slot: LoftSlot.Rulings, body: new BuildBody.UvRows(Rows: Seq(new Point2d(t0, t1))))
                                        + BuildReceipt<LoftSlot>.Of(slot: LoftSlot.Rulings, body: new BuildBody.Code(Value: verdict))));
                            }),
                            minTwistSecond: static (ctx, law) => ctx.Op.Catch(() => {
                                double t1 = ctx.Seed.Y;
                                double cosine = 0.0;
                                return ctx.Op.Confirm(success: DevelopableSrf.RulingMinTwist(
                                        rail0: ctx.Rail0, t0: ctx.Seed.X, rail1: ctx.Rail1, t1: ctx.Seed.Y,
                                        dom1: law.Domain1, t1_out: ref t1, cos_twist_out: ref cosine))
                                    .Map(_ => RulingBuilt(t0: ctx.Seed.X, t1: t1, cosine: cosine));
                            }),
                            minTwistBoth: static (ctx, law) => ctx.Op.Catch(() => {
                                double t0 = ctx.Seed.X;
                                double t1 = ctx.Seed.Y;
                                double cosine = 0.0;
                                return ctx.Op.Confirm(success: DevelopableSrf.RulingMinTwist(
                                        rail0: ctx.Rail0, t0: ctx.Seed.X, dom0: law.Domain0,
                                        rail1: ctx.Rail1, t1: ctx.Seed.Y, dom1: law.Domain1,
                                        t0_out: ref t0, t1_out: ref t1, cos_twist_out: ref cosine))
                                    .Map(_ => RulingBuilt(t0: t0, t1: t1, cosine: cosine));
                            }))));
            },
            adjustRulings: static (_, edit) => {
                Op op = Op.Of(name: nameof(AdjustRulings));
                return ModelGate.Borrow<NurbsCurve, Built<LoftSlot>>(handle: edit.Rail0, key: op, body: rail0 =>
                    ModelGate.Borrow<NurbsCurve, Built<LoftSlot>>(handle: edit.Rail1, key: op, body: rail1 =>
                        op.Catch(() => {
                            System.Collections.Generic.IEnumerable<Point2d> rulings = edit.Rulings.AsIterable();
                            return op.Confirm(success: DevelopableSrf.UntwistRulings(rail0: rail0, rail1: rail1, rulings: ref rulings))
                                .Map(_ => new Built<LoftSlot>(
                                    Products: Seq<GeometryHandle>(),
                                    Evidence: BuildReceipt<LoftSlot>.Of(slot: LoftSlot.Rulings, body: new BuildBody.UvRows(Rows: toSeq(rulings)))));
                        })));
            });

    private static Fin<Built<LoftSlot>> Swept(Op op, Func<Brep[]> run) =>
        ModelGate.OwnMany(built: run(), key: op).Map(owned => new Built<LoftSlot>(
            Products: owned,
            Evidence: BuildReceipt<LoftSlot>.Of(slot: LoftSlot.Swept, body: new BuildBody.Tally(Count: owned.Count))));

    private static Fin<Built<LoftSlot>> Lofted(Op op, Func<Brep[]> run) =>
        ModelGate.OwnMany(built: run(), key: op).Map(owned => new Built<LoftSlot>(
            Products: owned,
            Evidence: BuildReceipt<LoftSlot>.Of(slot: LoftSlot.Lofted, body: new BuildBody.Tally(Count: owned.Count))));

    private static Fin<Built<LoftSlot>> Patched(Op op, Func<Brep?> run) =>
        op.Catch(() => ModelGate.Own(built: run(), key: op).Map(owned => new Built<LoftSlot>(
            Products: Seq(owned),
            Evidence: BuildReceipt<LoftSlot>.Of(slot: LoftSlot.Patched, body: new BuildBody.Tally(Count: 1)))));

    private static Fin<Built<LoftSlot>> Developed(Op op, Func<Brep[]> run) =>
        op.Catch(() => ModelGate.OwnMany(built: run(), key: op).Map(owned => new Built<LoftSlot>(
            Products: owned,
            Evidence: BuildReceipt<LoftSlot>.Of(slot: LoftSlot.Developed, body: new BuildBody.Tally(Count: owned.Count)))));

    private static Built<LoftSlot> RulingBuilt(double t0, double t1, double cosine) => new(
        Products: Seq<GeometryHandle>(),
        Evidence: BuildReceipt<LoftSlot>.Of(slot: LoftSlot.Rulings, body: new BuildBody.UvRows(Rows: Seq(new Point2d(t0, t1))))
            + BuildReceipt<LoftSlot>.Of(slot: LoftSlot.Rulings, body: new BuildBody.Measure(Value: cosine)));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Lofts {
    public static Fin<Built<LoftSlot>> Build(Context context, params ReadOnlySpan<LoftOp> operations) {
        Op op = Op.Of();
        return from domain in Optional(context).ToFin(Fail: op.MissingContext())
               from _ in guard(operations.Length > 0, op.InvalidInput())
               from built in ModelGate.Folded(
                   context: domain,
                   operations: toSeq(operations.ToArray()),
                   apply: static (operation, model) => operation.Apply(domain: model))
               select built;
    }
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]           | [OWNER]          | [FORM]                                                     | [ENTRY]                      |
| :-----: | :------------------ | :--------------- | :--------------------------------------------------------- | :--------------------------- |
|  [01]   | sweep frame         | `SweepFrameLaw`  | freeform plus every roadlike preset and direction          | `SweepOne` payload           |
|  [02]   | sweep fit           | `SweepFit`       | as-is, rebuild, refit as one union                         | `SweepOne` / `SweepTwo`      |
|  [03]   | sweep modality      | `LoftOp`         | station policy selects static, engine, or partitioned      | `ShapeParams` / `Stations`   |
|  [04]   | loft fit + tangency | `LoftFit`        | context-rigged native overloads behind one case            | `LoftOp.Loft`                |
|  [05]   | direct patch        | `PatchLaw`       | spans, pull, flexibility, fixed edges as one value         | `LoftOp.Patch`               |
|  [06]   | variational solver  | `VariationalLaw` | whole `Brep.VariationalPatchSettings` surface as one value | `LoftOp.Variational` / `Rig` |
|  [07]   | solver verdict      | `LoftOp`         | warning, error, and G-continuity verdicts as facts         | `LoftSlot.Solved` facts      |
|  [08]   | developable loft    | `DevelopableLaw` | density or pinned-ruling modality                          | `LoftOp.Developable`         |
|  [09]   | ruling solve        | `RulingSolve`    | local or minimum-twist outputs as evidence                 | `LoftOp.SolveRuling`         |
|  [10]   | ruling adjustment   | `LoftOp`         | untwisted uv rows as evidence, no product                  | `LoftOp.AdjustRulings`       |
|  [11]   | loft verbs          | `LoftOp`         | one flat `[Union]`, total generated dispatch               | `Lofts.Build`                |

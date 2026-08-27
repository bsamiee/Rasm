# [RASM_GRASSHOPPER_CANVAS_WIRES]

Wire visuals for the Grasshopper boundary fold through one owner set — route admission over the public `WireShape` family, custom-route installation through the leased `ShapeType` hook, point picking through the result-typed canvas query, and the wire pass as a PLAN PRODUCER: `WirePass.Plan` answers a `GhPlan` the paint executor draws under its pass-scoped stock, so the per-wire-per-layer brush and pen minting this page once did inside its own loop — the exact defect the paint stock law names — has no loop left to live in.

`Canvas.WireDrawCache` and its `WireRepository` are internal, so point picking composes the public `Canvas.ResolvePick` boundary `Canvas/canvas.md` owns. Document-side traversal, mutation, split, and undo stay `Document/graph.md`'s `GraphScope`; this page consumes resolved `WireEnds` and pin attributes without touching the graph.

## [01]-[INDEX]

- [02]-[ROUTES]: `WireRoute` + `RouteStyle` + `Traced` — the admitted route capsule, the leased custom-shape hook, and the partitioned route producer.
- [03]-[PICKING]: `WirePick` — result-typed point picking and host marquee selection.
- [04]-[PENS]: `EndSelection` + `WirePens` + `WireSkinLens` + `WirePass` — the four-corner palette correspondence, perceptual pen evidence, skin derivation, and the plan producer.

## [02]-[ROUTES]

- Owner: `WireRoute` — the ADMISSION capsule and nothing more: one polymorphic `Of` discriminating on input shape (an endpoint pair routes raw points; a pin-attribute pair routes outlet-to-inlet, both under `Op.Catch` because the host `Create` throws on a pin-less attribute), and the public `Shape` the admission proved. Six rename members (`Nearest`/`Gap`/`Crosses`/`Touches`/`Extent`/`Endpoints`) are DELETED — the page's own no-rename-wrapper law, finally applied to itself: a consumer reads the host contract off `Shape` directly, and four of the six had zero call sites anywhere.
- Owner: `RouteStyle` — the custom-route hook over `WireShape.ShapeType`. `Install(Type, FaultCell, Op?)` → `Fin<Lease<Mounted<Unit>>>` admits the candidate through THREE accumulated clauses (`WireShape`-derived, closed and concrete, public two-`PointF` constructor — a candidate failing two reads both), then claims the HOST SLOT ITSELF — the one authority: a held slot, whether this plugin's or a sibling's, refuses `InvalidContext` instead of silently overriding, and release clears only the candidate it still owns — the fan's one capsule, no shadow seat to disagree with the slot.
- Owner: `Traced` — the route producer on the ruled partial-success posture: `Of` maps every pin row onto the admission path and PARTITIONS — accepted routes beside typed refusals, each refusal carrying its `WireEnds` in the fault detail — so a single detached pin never voids the pass — the composition root's paint PLANNER (`Platform/composition.md` roster row [07]) is the consumer: it folds `Traced.Of` → `WirePass.Plan` each pass and parks the refusal lane on the composition's `FaultCell`.
- Law: `WireShape.ShapeType` is PROCESS-GLOBAL host state and the ONE authority — every canvas and every co-resident plugin reads the same slot, so installation is leased custody over the slot itself: `Install` runs at plugin load inside the UI marshal (read-check-write is single-threaded by the marshal law), a held slot refuses rather than stacks, and the lease's inverse writes the slot back to unclaimed.
- Law: a route is rebuilt when its endpoints move, never cached across layout — construction is two points and a spline mint, and the host repaints wires per frame.
- Boundary: wire creation, deletion, endpoint rewiring, and the split into `Shout`/`Listen` are `Document/graph.md`'s; the straighten nudge candidate is `Canvas/layout.md`'s row; hover proximity is a `Shape.IsCoincident` read with the caller's tolerance — the consumer's policy datum, never a folder constant.
- Packages: Grasshopper2 (`WireShape`, `WireShapeDefault.CreateSpline`, `IParameterAttributes`, `WireEnds`), LanguageExt.Core (`Validation`, `Partition`), `Rasm.Domain` (`Op`, `Cell`, `FaultCell`, `Lease<T>`).
- Growth: a new route geometry is one installed `WireShape` subclass — zero edits here.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;

namespace Rasm.Grasshopper.Canvas;

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct WireRoute {
    private WireRoute(WireShape shape) => Shape = shape;

    public WireShape Shape { get; }

    public static Fin<WireRoute> Of(PointF source, PointF target) {
        return Try.lift(() => Fin.Succ(new WireRoute(shape: WireShape.Create(source, target)))).Run().Bind(static inner => inner);
    }

    public static Fin<WireRoute> Of(IParameterAttributes source, IParameterAttributes target) {
        return (Admit.Need(value: source).ToValidation(), Admit.Need(value: target).ToValidation())
            .Apply(static (origin, goal) => (Origin: origin, Goal: goal))
            .As().ToFin()
            .Bind(pair => Try.lift(() => Fin.Succ(new WireRoute(shape: WireShape.Create(pair.Origin, pair.Goal)))).Run().Bind(static inner => inner));
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct TracedRoutes(Seq<(WireEnds Ends, WireRoute Route)> Routes, Seq<Error> Refused);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RouteStyle {
    public static Option<Type> Current => Optional(WireShape.ShapeType);

    public static Fin<Lease<Mounted<Unit>>> Install(Type routeType, FaultCell faults) {
        return from candidate in Admit.Need(value: routeType)
               from admitted in (
                       Clause(typeof(WireShape).IsAssignableFrom(candidate), "a WireShape-derived type"),
                       Clause(!candidate.IsAbstract && !candidate.ContainsGenericParameters, "a closed concrete type"),
                       Clause(candidate.GetConstructor([typeof(PointF), typeof(PointF)]) is not null, "a public (PointF, PointF) constructor"))
                   .Apply(static (_, _, _) => unit).As().ToFin()
               from free in guard(WireShape.ShapeType is null, new KernelFault.InvalidContext())
               from seated in Try.lift(() => {
                   WireShape.ShapeType = candidate;
                   return Fin.Succ((Lease<Mounted<Unit>>)new Lease<Mounted<Unit>>.Owned(Value: new Mounted<Unit>(
                       release: () => guard(ReferenceEquals(WireShape.ShapeType, candidate), new KernelFault.InvalidContext()).ToFin()
                           .Map(_ => HostEdge.Side(static () => WireShape.ShapeType = null)),
                       faults: faults)));
               }).Run().Bind(static inner => inner).Rollback(
                   release: () => ReferenceEquals(WireShape.ShapeType, candidate)
                       ? Try.lift(static () => WireShape.ShapeType = null).Run().Bind(static inner => inner)
                       : Fin.Succ(unit))
               select seated;
    }

    private static Validation<Error, Unit> Clause(bool holds, string requirement) => holds
        ? Validation<Error, Unit>.Success(unit)
        : Validation<Error, Unit>.Fail(new KernelFault.InvalidValue(Label: nameof(RouteStyle), Requirement: requirement));
}

public static class Traced {
    public static TracedRoutes Of(Seq<(WireEnds Ends, IParameterAttributes Source, IParameterAttributes Target)> pins) {
        (Seq<Error> refused, Seq<(WireEnds, WireRoute)> routed) = pins
            .Map(row => WireRoute.Of(source: row.Source, target: row.Target)
                .Map(route => (row.Ends, route)))
            .Partition();
        return new TracedRoutes(Routes: routed, Refused: refused);
    }
}
```

## [03]-[PICKING]

- Owner: `WirePick` — the two pick modalities over public host contracts. `At` composes the result-typed `CanvasQuery.Pick(at, PickGates.Wiring, None)` — the answer arrives as `Picked` directly, so the projection-union ladder this member once paid is gone — and projects the hit through the generated PARTIAL overload: one wire arm, one default, so the four identical `None` arms have no spelling and an unknown future host `Pick` kind still refuses upstream at `PickHit.Of`. `Windowed` folds the host `WindowSelection.Selects(WireShape, float)` overload, retaining the host's crossing-versus-containing law.
- Law: pick admission is gate policy — whether wires participate in a marquee is `Canvas/canvas.md`'s `SelectAxis` set, and whether a pick verb is allowed is its `ActionGate` rows; this owner resolves geometry and never consults policy.
- Packages: Grasshopper2 (`WindowSelection`, `WireEnds`), `Canvas/canvas.md` (`CanvasQuery.Pick`, `CanvasOperator.Read`, `PickHit`), kernel `PickGates`, LanguageExt.Core, `Rasm.Domain`.
- Growth: a new pick modality is one method over an existing host read.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Interaction;

namespace Rasm.Grasshopper.Canvas;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class WirePick {
    public static Fin<Option<WireEnds>> At(PointF at) {
        return from query in CanvasQuery.Pick(at: at, gates: PickGates.Wiring)
               from picked in CanvasOperator.Read(query: query)
               select picked.Hit.SwitchPartially(
                   @default: static _ => Option<WireEnds>.None,
                   wireCase: static wire => Some(wire.Wire));
    }

    public static Seq<WireEnds> Windowed(WindowSelection window, Seq<(WireEnds Ends, WireRoute Route)> routes, float fuzz) =>
        routes.Filter(row => window.Selects(row.Route.Shape, fuzz)).Map(static row => row.Ends).Strict();
}
```

## [04]-[PENS]

- Owner: `EndSelection` `[SmartEnum<int>]` — the four-corner palette correspondence the HOST itself names: `Neither`/`Source`/`Target`/`Both` are the four outcomes of `WireSkin.ResolveColours(sourceSelected, targetSelected, out, out)`, and each row carries that resolve as its column, admitting the two host colours onto the kernel colour owner — the two loose bools and the out-pair lift are one keyed row read. `WirePens` — the resolved ink evidence in PERCEPTUAL colour (`Source`/`Target` ends, required `Outer` edge, optional `Inner` detail edge); the Eto quantization happens once at the executor's pen stock, never on this value.
- Owner: `WireSkinLens.Styled` — the one derivation fold projecting the corpus `Option` vocabulary onto the host `Modify` nullable-slot fold through the kernel `HostEdge.Nullable` — the six hand `Match(… null)` arms are one owner's projection. `WireSkin.Interpolate`/`Fade` stay host-direct; a perceptual blend between palettes crosses the kernel `Tween.Between`.
- Owner: `WirePass` — the PLAN PRODUCER: `Plan(skin, wires, detailing)` culls nothing and draws nothing — it answers a `GhPlan` of `GhMark.WireCase` rows (ONE per wire; both present layers ride the row's `Ink`, so the edge is stored once), and `Canvas/paint.md`'s executor draws them under its pass-scoped stock, where at most the palette's four corners per edge mint a pen. Culling, tallies, the settled pass, and stock custody all arrive from the executor — the hand loop with its `if`-continue cull, manual counter, and float-threshold gate is deleted whole.
- Law: the inner detail edge is a ZUI policy read — a wire row carries `Inner` only when the caller's `detailing` read admits it (the producer CLEARS the slot otherwise), so the detail gate is plan data, not a draw-time branch.
- Law: selection state arrives as data on the wire rows — the pass never reads document selection; the caller projects selection truth into `EndSelection` rows, keeping the producer pure over its inputs.
- Packages: Grasshopper2 (`WireSkin`, `EdgeDescription`, `Canvas.ZuiWireDetailingState`), `Rasm.Interaction` (`PaintColor`, `Tween`, `HostEdge.Nullable`), `Rasm.Numerics` (`PerceptualColor`), `Canvas/paint.md` (`GhPlan`, `GhMark.WireCase`), LanguageExt.Core, `Rasm.Domain`.
- Growth: a new wire treatment is a `Styled` derivation; a new pass policy is one plan parameter — the draw hook lives at the executor and never forks.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Interaction;
using Rasm.Numerics;

namespace Rasm.Grasshopper.Canvas;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class EndSelection {
    public static readonly EndSelection Neither = new(key: 0, source: false, target: false);
    public static readonly EndSelection Source = new(key: 1, source: true, target: false);
    public static readonly EndSelection Target = new(key: 2, source: false, target: true);
    public static readonly EndSelection Both = new(key: 3, source: true, target: true);

    internal bool SelectsSource { get; }
    internal bool SelectsTarget { get; }

    public Fin<WirePens> Pens(WireSkin skin) {
        skin.ResolveColours(SelectsSource, SelectsTarget, out Color source, out Color target);
        return from a in PaintColor.OfHost(host: source)
               from b in PaintColor.OfHost(host: target)
               select new WirePens(Source: a, Target: b, Outer: skin.Outer, Inner: Optional(skin.Inner));
    }
}

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct WirePens(
    PerceptualColor Source, PerceptualColor Target, EdgeDescription Outer, Option<EdgeDescription> Inner);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class WireSkinLens {
    public static WireSkin Styled(
        WireSkin skin, Option<Color> normal = default, Option<Color> selected = default,
        Option<Color> selectedOpposite = default, Option<Color> selectedGlow = default,
        Option<EdgeDescription> outerEdge = default, Option<EdgeDescription> innerEdge = default) =>
        skin.Modify(
            normal: HostEdge.Nullable(normal),
            selected: HostEdge.Nullable(selected),
            selectedOpposite: HostEdge.Nullable(selectedOpposite),
            selectedGlow: HostEdge.Nullable(selectedGlow),
            outerEdge: HostEdge.Nullable(outerEdge),
            innerEdge: HostEdge.Nullable(innerEdge));
}

public static class WirePass {
    public static Fin<GhPlan> Plan(
        WireSkin skin, Seq<(WireRoute Route, EndSelection Ends)> wires, float detailing) {
        return wires
            .Traverse(row => row.Ends.Pens(skin: skin).Map(pens =>
                new GhMark.WireCase(
                    Route: row.Route.Shape,
                    Ink: detailing > 0f ? pens : pens with { Inner = Option<EdgeDescription>.None }) as GhMark))
            .As()
            .Map(static marks => new GhPlan(Marks: marks));
    }
}
```

## [05]-[DENSITY_BAR]

| [INDEX] | [CONCERN]        | [OWNER]                     | [RESULT]                                        | [CASES] |
| :-----: | :--------------- | :-------------------------- | :---------------------------------------------- | :-----: |
|  [01]   | route admission  | `WireRoute`                 | one polymorphic `Of`, public proved `Shape`     |    2    |
|  [02]   | custom routes    | `RouteStyle` + `Mounted`    | host-slot custody, accumulated clauses          |    1    |
|  [03]   | route production | `Traced`                    | one `Partition` fold, refusals carry their ends |    1    |
|  [04]   | wire picking     | `WirePick`                  | result-typed query + partial-overload project   |    2    |
|  [05]   | palette port     | `EndSelection` + `WirePens` | four host-named corners, perceptual ends        |    4    |
|  [06]   | wire pass        | `WirePass`                  | plan producer over the paint executor           |    1    |

Per-wire brush/pen mint loop, the six rename wrappers, the third release capsule, the hand CAS seat and its hazard comment, the three sequential guards, the two-lane hand fold, and the six `Match(… null)` arms are all deleted; the process-global `ShapeType` custody law survives verbatim as this page's strongest passage.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

# [RASM_GRASSHOPPER_CANVAS_WIRES]

The wire-visual owner of the Grasshopper boundary — the canvas half of the census `WireOp`: route geometry over the host `WireShape` family, custom-route installation through the host's own `ShapeType` seam, wire picking through the public pick map, marquee wire selection through the host `WindowSelection` algebra, and the wire-pen pass over `WireSkin` state. The census reflection rails are adjudicated against the live decompile and KILLED in both directions: `Canvas.WireDrawCache` and the repository `WireAt` member the campaign charter presumed public are `internal` on the decompiled host — phantom members with no reachable contract — so wire picking rides `ResolvePick` with the wires gate and route observation rides routes this owner builds from pin attributes; the census `WireRouteSolver` (a local orthogonal grid router), `WireStyle` simple-name type reflection, `WireRepositoryRail` member discovery, `WireRoutingProfile`, and `PickTolerance` are ruled kills with no successor — a custom route is a `WireShape` subclass installed through `RouteStyle`, exactly the extension contract the host publishes. The document half of the census file — traversal, mutation, split, undo — landed as `Document/graph.md`'s `GraphScope`; this page consumes `WireEnds` values and pin attributes as given and never touches the graph.

## [01]-[INDEX]

- [02]-[ROUTES]: `WireRoute` + `RouteStyle` — the admitted route capsule over the `WireShape` family and the one custom-shape installation seam.
- [03]-[PICKING]: `WirePick` — point picking through the gated pick map and marquee picking through the `WindowSelection` fuzz algebra.
- [04]-[PENS]: `WirePens` + `WireSkinLens` + `WirePass` — wire-pen resolution by selection state, skin derivation folds, and the detail-gated draw pass.

## [02]-[ROUTES]

- Owner: `WireRoute` `readonly record struct` `[BoundaryAdapter]` — the admitted route capsule holding one host `WireShape`. Admission is ONE polymorphic `Of` discriminating on input shape: an endpoint pair (`PointF`, `PointF`) routes raw points, and a pin-attribute pair (`IParameterAttributes`, `IParameterAttributes`) routes outlet-to-inlet — the host `Create` throws on a source without an outlet or a target without an inlet, so both arms run under `Op.Catch` and a refused pin surfaces as the typed fault. The capsule's queries are the full verified geometry contract renamed to canonical verbs: `Nearest(PointF)` (`Project` — closest point on the route), `Gap(PointF)` (`DistanceTo`), `Crosses(RectangleF)` (`Intersects`), `Touches(PointF, float)` (`IsCoincident`), `Extent` (`Bounds`), and `Endpoints` (`Source`/`Target`).
- Owner: `RouteStyle` — the custom-route seam over the static `WireShape.ShapeType` property: `Install(Type routeType, Op? key = null)` demands the candidate derive from `WireShape` and expose a public two-`PointF` constructor BEFORE the host assignment (the host activates routes through that constructor shape, and a type installed without it fails at first wire draw instead of at install — this gate moves the failure to the mount), `Reset()` restores the `WireShapeDefault` fallback by clearing the slot, and `Current` reads the active type as `Option<Type>`. The host family behind the seam is `WireShapeDefault` (the cubic-spline route, its `Spline` a `BezierF` minted by the verified `CreateSpline(PointF, PointF) → BezierF` static) plus the internal `WireShapeLinear` and `WireShapeBiArc` variants the host selects itself — the census `WireShapeElbow` does not exist on the decompiled assembly and is a phantom kill; an elbow, orthogonal, or bundled route is a `WireShape` subclass a plugin installs through this seam, which is the ruled successor to the killed local router.
- Owner: `Traced` — the route-set producer: `Traced.Of(Seq<(WireEnds Ends, IParameterAttributes Source, IParameterAttributes Target)> pins, Op key)` folds pin rows into `TracedRoutes` through the attribute admission arm — every routed pin lands in `Routes`, every refused pin lands in `Refused` as typed evidence beside its `WireEnds`, so a single detached pin never voids the pass: the draw fold strokes what routed and a diagnostics consumer folds what refused. Pin resolution — `Guid` to `IParameterAttributes` — is graph territory; the rows arrive resolved.
- Law: a route is rebuilt when its endpoints move, never cached across layout — construction is two points and a spline mint, and the host repaints wires per frame anyway; the killed draw-cache observation has no successor because the observed cache was never a public contract.
- Boundary: wire creation, deletion, endpoint rewiring, and the split into `Shout`/`Listen` are `Document/graph.md`'s `GraphScope.Mutate`; the straighten NUDGE candidate (`SnappingAction.CreateStraightenWireAction`) is `Canvas/layout.md`'s row; this page owns route geometry and its rendering only.
- Packages: Grasshopper2 (`WireShape.Create`/`Project`/`DistanceTo`/`Intersects`/`IsCoincident`/`Draw`/`Bounds`/`Source`/`Target`/`ShapeType`, `WireShapeDefault.CreateSpline`, `IParameterAttributes.HasInlet`/`HasOutlet`/`Inlet`/`Outlet`, `WireEnds`), Eto.Drawing (`PointF`, `RectangleF`, `BezierF`, `Graphics`, `Pen`), LanguageExt.Core, `Rasm.Domain`.
- Growth: a new route geometry is one installed `WireShape` subclass — zero edits here; a new route query is one capsule member over the host contract.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Csp;

namespace Rasm.Grasshopper.Canvas;

// --- [MODELS] -------------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct WireRoute {
    private WireRoute(WireShape shape) => Shape = shape;
    internal WireShape Shape { get; }

    public static Fin<WireRoute> Of(PointF source, PointF target, Op? key = null) {
        Op op = key.OrDefault();
        return op.Catch(body: () => Fin.Succ(new WireRoute(shape: WireShape.Create(source, target))));
    }

    public static Fin<WireRoute> Of(IParameterAttributes source, IParameterAttributes target, Op? key = null) {
        Op op = key.OrDefault();
        return from origin in op.Need(value: source)
               from goal in op.Need(value: target)
               from route in op.Catch(body: () => Fin.Succ(new WireRoute(shape: WireShape.Create(origin, goal))))
               select route;
    }

    public PointF Nearest(PointF point) => Shape.Project(point);
    public float Gap(PointF point) => Shape.DistanceTo(point);
    public bool Crosses(RectangleF box) => Shape.Intersects(box);
    public bool Touches(PointF point, float tolerance) => Shape.IsCoincident(point, tolerance);
    public RectangleF Extent => Shape.Bounds;
    public (PointF Source, PointF Target) Endpoints => (Shape.Source, Shape.Target);
    internal Unit Render(Graphics graphics, Pen edge) => Op.Side(action: () => Shape.Draw(graphics, edge));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
[BoundaryAdapter]
public static class RouteStyle {
    public static Option<Type> Current => Optional(WireShape.ShapeType);

    public static Fin<Unit> Install(Type routeType, Op? key = null) {
        Op op = key.OrDefault();
        return from candidate in op.Need(value: routeType)
               from derived in guard(typeof(WireShape).IsAssignableFrom(candidate), op.InvalidInput()).ToFin()
               from constructible in guard(
                   candidate.GetConstructor([typeof(PointF), typeof(PointF)]) is not null, op.InvalidInput()).ToFin()
               from _ in op.Catch(body: () => Fin.Succ(Op.Side(action: () => WireShape.ShapeType = candidate)))
               select unit;
    }

    public static Unit Reset() => Op.Side(action: static () => WireShape.ShapeType = null!);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct TracedRoutes(Seq<(WireEnds Ends, WireRoute Route)> Routes, Seq<(WireEnds Ends, Error Refusal)> Refused);

[BoundaryAdapter]
public static class Traced {
    public static TracedRoutes Of(Seq<(WireEnds Ends, IParameterAttributes Source, IParameterAttributes Target)> pins, Op key) =>
        pins.Fold(
            new TracedRoutes(Routes: Seq<(WireEnds, WireRoute)>(), Refused: Seq<(WireEnds, Error)>()),
            (held, row) => WireRoute.Of(source: row.Source, target: row.Target, key: key).Match(
                Succ: route => held with { Routes = held.Routes.Add((row.Ends, route)) },
                Fail: fault => held with { Refused = held.Refused.Add((row.Ends, fault)) }));
}
```

## [03]-[PICKING]

- Owner: `WirePick` — the two pick modalities over public host contracts. Point picking: `At(PointF at, Op? key = null)` → `Fin<Option<WireEnds>>` rides `CanvasOperator.Read` with `PickGates.WiresOnly` — the pick map resolves the wire under the point through the host's own id buffer, and a non-wire verdict folds to `None`; the killed `WireAt` reflection path has no successor because `ResolvePick` IS the public spelling of the same read. Marquee picking: `Windowed(WindowSelection window, Seq<(WireEnds Ends, WireRoute Route)> routes, float fuzz)` → `Seq<WireEnds>` — a pure fold over the host `WindowSelection.Selects(WireShape, float)` overload, so crossing-versus-containing semantics (`IsCrossing` — right-to-left drags select touching wires) stay the host's marquee law, never a re-derived rectangle test.
- Law: pick admission is gate policy — whether wires participate in a marquee at all is `Canvas/canvas.md`'s `SelectGates`, and whether a pick verb is allowed at all is its `ActionGate` rows (`WireSelect`, `MakeWire`, `DeleteWire`, `ModifyWire`); this owner resolves geometry and never consults policy.
- Law: hover proximity is route geometry — `route.Touches(point, tolerance)` with the caller's tolerance value; the census `PickTolerance` constant carrier is killed, the tolerance is the consumer's policy datum.
- Packages: Grasshopper2 (`WindowSelection.Selects(WireShape, float)`/`IsCrossing`/`Box`, `SelectionResult`, `WireEnds`), `Canvas/canvas.md` (`CanvasOperator.Read`, `PickGates`, `PickHit`), LanguageExt.Core, `Rasm.Domain`.
- Growth: a new pick modality is one method over an existing host read; the gates and the fuzz algebra never fork.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Csp;

namespace Rasm.Grasshopper.Canvas;

// --- [OPERATIONS] ---------------------------------------------------------------------------
[BoundaryAdapter]
public static class WirePick {
    public static Fin<Option<WireEnds>> At(PointF at, Op? key = null) {
        Op op = key.OrDefault();
        return CanvasOperator.Read(lens => lens.Pick(at: at, gates: PickGates.WiresOnly, key: op)
            .Map(static receipt => receipt.Hit.Switch(
                wireCase: static wire => Some(wire.Wire),
                inletCase: static _ => Option<WireEnds>.None,
                outletCase: static _ => Option<WireEnds>.None,
                surfaceCase: static _ => Option<WireEnds>.None,
                voidCase: static _ => Option<WireEnds>.None)), key: op);
    }

    public static Seq<WireEnds> Windowed(WindowSelection window, Seq<(WireEnds Ends, WireRoute Route)> routes, float fuzz) =>
        routes.Filter(row => window.Selects(row.Route.Shape, fuzz)).Map(static row => row.Ends).Strict();
}
```

## [04]-[PENS]

- Owner: `WirePens` `readonly record struct` — the resolved wire-ink evidence: `Source` and `Target` END colours from the host's own state resolution (`WireSkin.ResolveColours(sourceSelected, targetSelected, out sourceColour, out targetColour)` — the half-selected wire legitimately renders two colours, one per end), plus the `Outer` and `Inner` `EdgeDescription` STROKE layers the skin carries. Ends and layers are orthogonal axes: each stroke layer mints one pen whose ink is solid when the end colours agree and a source-to-target `LinearGradientBrush` along the route endpoints when they differ, styled through `EdgeDescription.AssignToPen` and disposed with the stroke.
- Owner: `WireSkinLens` — the two folds that ADD shape over the verified `WireSkin` surface: `Pens(WireSkin, bool sourceSelected, bool targetSelected)` lifts the `out`-pair onto `WirePens`, and `Styled(WireSkin, ...)` projects the corpus `Option` vocabulary onto the host `Modify` nullable-slot fold (`normal`/`selected`/`selectedOpposite`/`selectedGlow`/`outerEdge`/`innerEdge`) so a themed wire palette is one derivation expression, never a rebuilt skin. `WireSkin.Interpolate` and `Fade` are host-direct — a rename fold beside them is the deleted form, the same law that keeps the `Skin` `With` algebra unwrapped on `Canvas/paint.md`. The full palette row set — `Normal`, `Selected`, `Unselected`, `SelectedGlow` — is the decompiled truth; a perceptual blend between OUR palettes crosses through `Pigment` onto the kernel `PerceptualBlend` rows, while `Interpolate` remains the host's own palette blend.
- Owner: `WirePass` — the detail-gated draw fold: `Draw(PaintScene scene, WireSkin skin, Seq<(WireRoute Route, bool SourceSelected, bool TargetSelected)> wires, Op key)` → `Fin<int>` culls each route's `Extent` against the scene's visible frame, resolves the end-colour pair per selection state, strokes the `Outer` layer and then the `Inner` layer through `WireShape.Draw` with gradient ink when the ends disagree, and gates the inner detail stroke on the scene canvas's `ZuiWireDetailingState` — the host's own zoom-dependent wire detailing signal — so a zoomed-out canvas pays one stroke per wire. The pass runs inside a `Canvas/paint.md` window (`BeforeWires`/`AfterWires` rows for under- and over-painting the host wire layer).
- Law: selection state arrives as data on the wire rows — the pass never reads document selection; the caller projects selection truth (a `Document/graph.md` read) into the row flags, keeping the draw fold pure over its inputs.
- Packages: Grasshopper2 (`WireSkin.ResolveColours`/`Interpolate`/`Fade`/`Modify`/`Normal`/`Selected`/`Unselected`/`SelectedGlow`/`Outer`/`Inner`, `EdgeDescription.AssignToPen`/`Width`/`Cap`/`Dash`, `Canvas.ZuiWireDetailingState`), Eto.Drawing (`Pen`, `Brush`, `SolidBrush`, `LinearGradientBrush`, `Color`, `Graphics`), `Canvas/paint.md` (`PaintScene`), Wacton.Unicolour via `Pigment`, LanguageExt.Core, `Rasm.Domain`.
- Growth: a new wire treatment is a `Styled` derivation or one skin row through the host `Modify` slots; a new pass policy (glow, bundle dimming) is one fold parameter — the draw seam never forks.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Csp;

namespace Rasm.Grasshopper.Canvas;

// --- [MODELS] -------------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct WirePens(Color Source, Color Target, EdgeDescription Outer, EdgeDescription Inner);

// --- [OPERATIONS] ---------------------------------------------------------------------------
[BoundaryAdapter]
public static class WireSkinLens {
    public static WirePens Pens(WireSkin skin, bool sourceSelected, bool targetSelected) {
        skin.ResolveColours(sourceSelected, targetSelected, out Color source, out Color target);
        return new WirePens(Source: source, Target: target, Outer: skin.Outer, Inner: skin.Inner);
    }

    public static WireSkin Styled(
        WireSkin skin, Option<Color> normal = default, Option<Color> selected = default,
        Option<Color> selectedOpposite = default, Option<Color> selectedGlow = default,
        Option<EdgeDescription> outerEdge = default, Option<EdgeDescription> innerEdge = default) =>
        skin.Modify(
            normal: normal.MatchUnsafe(Some: static c => c, None: static () => (Color?)null),
            selected: selected.MatchUnsafe(Some: static c => c, None: static () => (Color?)null),
            selectedOpposite: selectedOpposite.MatchUnsafe(Some: static c => c, None: static () => (Color?)null),
            selectedGlow: selectedGlow.MatchUnsafe(Some: static c => c, None: static () => (Color?)null),
            outerEdge: outerEdge.MatchUnsafe(Some: static e => e, None: static () => null),
            innerEdge: innerEdge.MatchUnsafe(Some: static e => e, None: static () => null));
}

[BoundaryAdapter]
public static class WirePass {
    public static Fin<int> Draw(
        PaintScene scene, WireSkin skin,
        Seq<(WireRoute Route, bool SourceSelected, bool TargetSelected)> wires, Op key) =>
        key.Catch(body: () => {
            float detailing = scene.Surface.ZuiWireDetailingState;
            Graphics graphics = scene.Graphics.Content;
            int drawn = wires.Fold(0, (count, row) => {
                if (!scene.Visible.Intersects(row.Route.Extent)) { return count; }
                WirePens pens = WireSkinLens.Pens(skin: skin, sourceSelected: row.SourceSelected, targetSelected: row.TargetSelected);
                _ = Stroke(graphics: graphics, route: row.Route, pens: pens, edge: pens.Outer);
                if (detailing > 0f) { _ = Stroke(graphics: graphics, route: row.Route, pens: pens, edge: pens.Inner); }
                return count + 1;
            });
            return Fin.Succ(drawn);
        });

    private static Unit Stroke(Graphics graphics, WireRoute route, WirePens pens, EdgeDescription edge) {
        (PointF source, PointF target) = route.Endpoints;
        using Brush ink = pens.Source == pens.Target
            ? new SolidBrush(pens.Source)
            : new LinearGradientBrush(pens.Source, pens.Target, source, target);
        using Pen pen = new(ink, edge.Width);
        edge.AssignToPen(pen);
        return route.Render(graphics: graphics, edge: pen);
    }
}
```

```mermaid
flowchart LR
    Graph["Document/graph WireEnds + pin attributes"] -->|resolved rows| Trace["Traced.Of → Seq&lt;(WireEnds, WireRoute)&gt;"]
    Trace --> Route["WireRoute over host WireShape family"]
    Plugin["custom route subclass"] -->|"Install(Type)"| Style["RouteStyle over WireShape.ShapeType"]
    Style --> Route
    PickGate["WirePick.At → Fin&lt;Option&lt;WireEnds&gt;&gt;"] -->|"PickGates.WiresOnly"| CanvasPage["Canvas/canvas CanvasOperator.Read"]
    Marquee["WirePick.Windowed"] -->|"Selects(WireShape, fuzz)"| Host["host WindowSelection algebra"]
    Pass["WirePass.Draw"] -->|"pen pair per selection state"| SkinLens["WireSkinLens over WireSkin"]
    Pass -->|"BeforeWires / AfterWires window"| PaintPage["Canvas/paint PaintScene"]
```

## [05]-[DENSITY_BAR]

| [INDEX] | [CONCERN]           | [OWNER]                     | [KIND]                                             | [RAIL]                                    | [CASES] |
| :-----: | :------------------ | :-------------------------- | :-------------------------------------------------- | :----------------------------------------- | :-----: |
|  [01]   | route geometry      | `WireRoute`                 | admitted capsule, one polymorphic `Of`             | `Of → Fin<WireRoute>`                     |    2    |
|  [02]   | custom routes       | `RouteStyle`                | gated host `ShapeType` seam                        | `Install → Fin<Unit>`                     |    1    |
|  [03]   | route production    | `Traced` + `TracedRoutes`   | partial-success fold over pin rows                 | `Of → TracedRoutes`                       |    1    |
|  [04]   | wire picking        | `WirePick`                  | pick-map point read + marquee fuzz fold            | `At → Fin<Option<WireEnds>>`              |    2    |
|  [05]   | pen resolution      | `WirePens` + `WireSkinLens` | out-pair lift + `Modify` slot projection           | pure                                       |    2    |
|  [06]   | wire draw pass      | `WirePass`                  | detail-gated cull-and-stroke fold                  | `Draw → Fin<int>`                         |    1    |

`CanvasOperator`, `PickGates`, `PaintScene`, `Pigment`, `Op`, and the host `WindowSelection`/`WireSkin` algebras are composed upstream owners. The census `WireOp` operation roster, `WireTraversal`, `WireEdit`, `WireRouteSolver`, `WireStyle` reflection, `WireRepositoryRail`, `WireRoutingProfile`, and `PickTolerance` have no successor shape here — the document half landed in `Document/graph.md`, the visual half lands as the capsules and folds above, and the `WireDrawCache`/`WireAt`/`WireShapeElbow` members are phantom kills adjudicated `internal`-or-absent on the decompiled host.

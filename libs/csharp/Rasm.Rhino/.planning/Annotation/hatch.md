# [RASM_RHINO_ANNOTATION_HATCH]

Hatch rail (`Rasm.Rhino.Annotation`). `PatternDef` projects catalogued hatch-pattern content as an ordered `LineDef` run. Default authoring derives from `GetDefaultHatchPatterns()` instead of duplicating its roster as code rows, while `.pat` interchange admits arbitrary definitions through verified host factories. Custom definition construction remains excluded because the sealed catalog proves neither `HatchPattern`/`HatchLine` constructors nor `HatchLine` setters. `HatchSpec` carries explicit loops, nested curves, or a planar brep face into plural `Hatch` geometry under one `FillPlacement`. Gradient assignment, pattern scaling, boundary extraction, display geometry, solid projection, and explosion ride the shared spine and receipt. Reclamation stays `TableOp.Reclaim(TableKind.HatchPatterns)`; placed hatches resolve as `RhinoObject.Geometry is Hatch` because no `HatchObject` exists.

## [01]-[INDEX]

- [02]-[PATTERN_MODEL]: `FillKind`, `LineDef`, and `PatternDef` over the generated default census.
- [03]-[HATCH_GEOMETRY]: `FillPlacement` and the `HatchSpec` construction union.
- [04]-[HATCH_RAIL]: `HatchOp`, `HatchTransaction`, and the `Hatches` entry pair.
- [05]-[ASK_FAMILY]: `HatchAsk`/`HatchAnswer` — pattern snapshot, defaults, preview, display geometry, loops, fill, solid, pieces.
- [06]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[PATTERN_MODEL]

- Owner: `FillKind` `[SmartEnum<int>]` — fill vocabulary keyed on explicit `HatchPatternFillType` values; `LineDef` — one detached dash-line generator row; `PatternDef` — complete detached pattern content: name, description, fill kind, unit regime, distance policy, and ordered generator run.
- Law: default census and authoring derive from `HatchPattern.GetDefaultHatchPatterns()`; no copied nine-case vocabulary can drift from the host roster.
- Law: arbitrary definitions enter through `ReadFromFile` until construction APIs are proven. Table content changes still require fully composed values through `Modify`; live table mutation remains absent.
- Law: a `Lines`-kind pattern demands at least one generator row and a `Solid`/`Gradient` kind demands none — the def factory enforces the coupling so an empty line-definition set can never render as invisible fill.
- RESEARCH: admit custom `PatternDef` construction only after decompilation proves public `HatchPattern`/`HatchLine` constructors and settable `HatchLine.Angle`/`BasePoint`/`Offset`. Query `Rhino.DocObjects.HatchPattern` and `Rhino.DocObjects.HatchLine` through the `rhino-common` rail; current fences contain none of those unverified members.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class FillKind {
    public static readonly FillKind Solid = new(key: (int)HatchPatternFillType.Solid);
    public static readonly FillKind Lines = new(key: (int)HatchPatternFillType.Lines);
    public static readonly FillKind Gradient = new(key: (int)HatchPatternFillType.Gradient);

    internal HatchPatternFillType Host => (HatchPatternFillType)Key;
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record LineDef(double Angle, Point2d Base, Vector2d Offset, Seq<double> Dashes) {
    public static Fin<LineDef> Of(double angle, Point2d @base, Vector2d offset, params ReadOnlySpan<double> dashes) {
        Op op = Op.Of(name: nameof(LineDef));
        return from turn in op.AcceptInput(value: angle)
               from anchor in op.AcceptInput(value: @base)
               from step in op.AcceptInput(value: offset)
               from run in toSeq(dashes.ToArray()).TraverseM(dash => op.AcceptInput(value: dash)).As()
               select new LineDef(Angle: turn, Base: anchor, Offset: step, Dashes: run);
    }
}

public sealed record PatternDef(
    string Name,
    Option<string> Description,
    FillKind Fill,
    UnitSystem Units,
    bool AlwaysModelDistances,
    Seq<LineDef> Lines) {
    public static Fin<PatternDef> Of(
        string name, FillKind fill, UnitSystem units,
        Option<string> description = default, bool alwaysModelDistances = false,
        params ReadOnlySpan<LineDef> lines) {
        Op op = Op.Of(name: nameof(PatternDef));
        return from label in op.AcceptText(value: name)
               from kind in Optional(fill).ToFin(Fail: op.InvalidInput())
               from run in toSeq(lines.ToArray()).TraverseM(line => Optional(line).ToFin(Fail: op.InvalidInput())).As()
               from _ in guard(kind == FillKind.Lines ? !run.IsEmpty : run.IsEmpty, op.InvalidInput()).ToFin()
               select new PatternDef(
                   Name: label, Description: description, Fill: kind, Units: units,
                   AlwaysModelDistances: alwaysModelDistances, Lines: run);
    }
}
```

## [03]-[HATCH_GEOMETRY]

- Owner: `FillPlacement` — the placement parameters every construction shares: the pattern address, in-plane rotation, and pattern scale; `HatchSpec` `[Union]` — the three boundary constructions: `Bounded` over an explicit outer loop and holes, `Resolved` over nested closed curves the host partitions into one-or-more hatches, `FromFace` over a planar brep face with its base point.
- Law: the pattern binds by resolution — `FillPlacement` carries a `ResourceRef`, the mint resolves it against the pattern lens inside the grant, and the host receives an index that provably exists; a bare `int` pattern index in request data is the deleted form.
- Boundary: `HatchPatternTable` exposes name and index lookup but no id lookup; this lens returns typed absence for `ResourceRef.ById` instead of inventing an enumeration member.
- Law: `Resolved` answers plurality — nested closed curves legitimately partition into several hatches, so the mint's product is always `Seq<Hatch>` and the single-loop constructions land as one-element sequences, keeping one product shape across the union.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
public sealed record FillPlacement(ResourceRef Pattern, double RotationRadians = 0.0, double Scale = 1.0) {
    public static Fin<FillPlacement> Of(ResourceRef pattern, double rotationRadians = 0.0, double scale = 1.0, Op? key = null) {
        Op op = key.OrDefault();
        return from address in Optional(pattern).ToFin(Fail: op.InvalidInput())
               from _ in op.AcceptInput(value: rotationRadians)
               from factor in op.Positive(value: scale)
               select new FillPlacement(Pattern: address, RotationRadians: rotationRadians, Scale: factor);
    }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HatchSpec {
    private HatchSpec() { }
    public sealed record Bounded(Plane Plane, Curve Outer, Seq<Curve> Holes) : HatchSpec;
    public sealed record Resolved(Seq<Curve> Curves, Option<double> Tolerance = default) : HatchSpec;
    public sealed record FromFace(Brep Source, int FaceIndex, Point3d BasePoint) : HatchSpec;

    internal static readonly ResourceLens<HatchPattern> Lens = new(
        ById: static (_, _) => null,
        ByName: static (document, name) => document.HatchPatterns.FindName(name: name),
        ByIndex: static (document, index) => document.HatchPatterns.FindIndex(index: index));

    internal Fin<Seq<Hatch>> Mint(RhinoDoc document, FillPlacement placement, Op op) =>
        from pattern in placement.Pattern.Resolve(document: document, lens: Lens, key: op)
        from hatches in Switch(
            (Index: pattern.Index, Placement: placement, Op: op, Document: document),
            bounded: static (context, spec) => context.Op.Catch(() => Optional(Hatch.Create(
                    hatchPlane: spec.Plane, outerLoop: spec.Outer, innerLoops: spec.Holes.AsIterable(),
                    hatchPatternIndex: context.Index, rotationRadians: context.Placement.RotationRadians, scale: context.Placement.Scale))
                .Map(static hatch => Seq(hatch))
                .ToFin(Fail: context.Op.InvalidResult())),
            resolved: static (context, spec) => context.Op.Catch(() => Optional(spec.Tolerance.Match(
                    Some: tolerance => Hatch.Create(
                        curves: spec.Curves.AsIterable(), hatchPatternIndex: context.Index,
                        rotationRadians: context.Placement.RotationRadians, scale: context.Placement.Scale, tolerance: tolerance),
                    None: () => Hatch.Create(
                        curves: spec.Curves.AsIterable(), hatchPatternIndex: context.Index,
                        rotationRadians: context.Placement.RotationRadians, scale: context.Placement.Scale)))
                .Map(static hatches => toSeq(hatches))
                .Filter(static hatches => !hatches.IsEmpty)
                .ToFin(Fail: context.Op.InvalidResult())),
            fromFace: static (context, spec) => context.Op.Catch(() => Optional(Hatch.CreateFromBrep(
                    brep: spec.Source, brepFaceIndex: spec.FaceIndex, hatchPatternIndex: context.Index,
                    rotationRadians: context.Placement.RotationRadians, scale: context.Placement.Scale, basePoint: spec.BasePoint))
                .Map(static hatch => Seq(hatch))
                .ToFin(Fail: context.Op.InvalidResult())))
        select hatches;
}
```

## [04]-[HATCH_RAIL]

- Owner: `HatchOp` `[Union]` — default-pattern authoring, rename, deletion, `.pat` import/export, hatch placement, gradient assignment, and pattern rescale on placed hatches; `HatchTransaction` — the commit plan; `Hatches` — the `Commit`/`Ask` entry pair.
- Law: `AuthorDefault` refuses an existing pattern name; `MintName` supplies a rename target for a subsequently authored or imported pattern.
- Law: placed-hatch mutation is duplicate-then-`Replace` — `Regrade` and `Rescale` resolve the object, cast its geometry to `Hatch`, mutate the duplicate (`SetGradientFill`, `ScalePattern`), and land through `ObjectTable.Replace` inside the bracket.
- Law: import lands per pattern — `.pat` files carry many patterns, each read definition adds as its own row, and the first name collision stops the monadic fold with a typed failure.
- Boundary: `Marks.Render`'s `HatchCase` and the pipeline's `DrawHatch` draw this rail's geometry; `CreateDisplayGeometry` here resolves the drawable primitives, the Display rail owns the pixels.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HatchOp {
    private HatchOp() { }
    public sealed record AuthorDefault(string Name) : HatchOp;
    public sealed record Rename(ResourceRef Target, string Name) : HatchOp;
    public sealed record Delete(ResourceRef Target, bool Quiet = true) : HatchOp;
    public sealed record Import(string Path, bool Quiet = true) : HatchOp;
    public sealed record Export(string Path, Seq<ResourceRef> Targets) : HatchOp;
    public sealed record Place(HatchSpec Spec, FillPlacement Placement, Option<ObjectAttributes> Attributes = default) : HatchOp;
    public sealed record Regrade(TableTarget Target, ColorGradient Fill) : HatchOp;
    public sealed record Rescale(TableTarget Target, Transform Motion) : HatchOp;

    internal Fin<DraftReceipt> Apply(RhinoDoc document, Op op) =>
        Switch(
            (Document: document, Op: op),
            authorDefault: static (context, edit) =>
                from name in context.Op.AcceptText(value: edit.Name)
                from built in context.Op.Catch(() => toSeq(HatchPattern.GetDefaultHatchPatterns())
                    .Find(pattern => string.Equals(pattern.Name, name, StringComparison.OrdinalIgnoreCase))
                    .ToFin(Fail: context.Op.MissingContext()))
                from _ in guard(context.Document.HatchPatterns.FindName(name: built.Name) is null, context.Op.InvalidInput()).ToFin()
                from index in Added(document: context.Document, pattern: built, op: context.Op)
                select DraftReceipt.Component(slot: DraftSlot.Authored, index: index),
            rename: static (context, edit) =>
                from pattern in edit.Target.Resolve(document: context.Document, lens: HatchSpec.Lens, key: context.Op)
                from name in context.Op.AcceptText(value: edit.Name)
                from _ in context.Op.Confirm(success: context.Document.HatchPatterns.Rename(
                    hatchPatternIndex: pattern.Index, hatchPatternName: name))
                select DraftReceipt.Component(slot: DraftSlot.Renamed, index: pattern.Index),
            delete: static (context, edit) =>
                from pattern in edit.Target.Resolve(document: context.Document, lens: HatchSpec.Lens, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.HatchPatterns.Delete(
                    hatchPatternIndex: pattern.Index, quiet: edit.Quiet))
                select DraftReceipt.Component(slot: DraftSlot.Deleted, index: pattern.Index),
            import: static (context, edit) =>
                from path in context.Op.AcceptText(value: edit.Path)
                from read in context.Op.Catch(() => Optional(HatchPattern.ReadFromFile(filename: path, quiet: edit.Quiet))
                    .Map(static patterns => toSeq(patterns))
                    .Filter(static patterns => !patterns.IsEmpty)
                    .ToFin(Fail: context.Op.InvalidResult()))
                from landed in read.TraverseM(pattern =>
                    guard(context.Document.HatchPatterns.FindName(name: pattern.Name) is null, context.Op.InvalidInput()).ToFin()
                        .Bind(_ => Added(document: context.Document, pattern: pattern, op: context.Op))).As()
                select landed.Fold(
                    DraftReceipt.Path(slot: DraftSlot.Imported, path: path),
                    static (state, index) => state + DraftReceipt.Component(slot: DraftSlot.Imported, index: index)),
            export: static (context, edit) =>
                from path in context.Op.AcceptText(value: edit.Path)
                from patterns in edit.Targets.TraverseM(target =>
                    target.Resolve(document: context.Document, lens: HatchSpec.Lens, key: context.Op)).As()
                from _ in guard(!patterns.IsEmpty, context.Op.InvalidInput()).ToFin()
                from __ in context.Op.Confirm(success: HatchPattern.WriteToFile(
                    filename: path, hatchPatterns: patterns.AsIterable()))
                select DraftReceipt.Path(slot: DraftSlot.Exported, path: path),
            place: static (context, edit) =>
                from hatches in edit.Spec.Mint(document: context.Document, placement: edit.Placement, op: context.Op)
                from ids in hatches.TraverseM(hatch => context.Op.Catch(() =>
                    context.Document.Objects.Add(
                        geometry: hatch,
                        attributes: edit.Attributes.IfNoneUnsafe((ObjectAttributes?)null),
                        history: null,
                        reference: false) is var id && id != Guid.Empty
                        ? Fin.Succ(value: id)
                        : Fin.Fail<Guid>(error: context.Op.InvalidResult()))).As()
                select DraftReceipt.Objects(slot: DraftSlot.Placed, ids: ids),
            regrade: static (context, edit) =>
                Reworked(document: context.Document, target: edit.Target, op: context.Op, slot: DraftSlot.Restyled,
                    change: (hatch, key) => key.Catch(() => { hatch.SetGradientFill(fill: edit.Fill); return Fin.Succ(value: unit); })),
            rescale: static (context, edit) =>
                Reworked(document: context.Document, target: edit.Target, op: context.Op, slot: DraftSlot.Scaled,
                    change: (hatch, key) =>
                        from _ in key.AcceptInput(value: edit.Motion)
                        from __ in key.Catch(() => { hatch.ScalePattern(xform: edit.Motion); return Fin.Succ(value: unit); })
                        select unit));

    private static Fin<int> Added(RhinoDoc document, HatchPattern pattern, Op op) =>
        op.Catch(() => document.HatchPatterns.Add(pattern: pattern) is var index && index >= 0
            ? Fin.Succ(value: index)
            : Fin.Fail<int>(error: op.InvalidResult()));

    internal static Fin<DraftReceipt> Reworked(
        RhinoDoc document, TableTarget target, Op op, DraftSlot slot,
        Func<Hatch, Op, Fin<Unit>> change) =>
        from ids in target.Resolve(document: document, key: op)
        from amended in ids.TraverseM(id =>
            from native in Optional(document.Objects.FindId(id)).ToFin(Fail: op.MissingContext())
            from source in Optional(native.Geometry as Hatch).ToFin(Fail: op.InvalidInput())
            from copy in Optional(source.Duplicate() as Hatch).ToFin(Fail: op.InvalidResult())
            from _ in change(copy, op)
            from __ in op.Confirm(success: document.Objects.Replace(objectId: id, geometry: copy, ignoreModes: false))
            select id).As()
        select DraftReceipt.Objects(slot: slot, ids: amended);
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record HatchTransaction(string Name, Seq<HatchOp> Operations, RedrawPolicy Redraw, bool UndoRecorded = true) {
    public static HatchTransaction Batch(string name, params ReadOnlySpan<HatchOp> operations) =>
        new(Name: name, Operations: toSeq(operations.ToArray()), Redraw: RedrawPolicy.Deferred);
}
```

## [05]-[ASK_FAMILY]

- Owner: `PatternSnapshot` — identity, usage, and one nested `PatternDef` read through `HatchLines`; `HatchDisplay` — boundary curves, pattern lines, and solid-fill brep from one `CreateDisplayGeometry` call; `HatchAsk`/`HatchAnswer` — typed request/result pairs over patterns and placed hatches.
- Law: the snapshot round-trips the def — `LineDef` rows read through `HatchLines`/`GetDashes`, so a snapshot re-feeds `PatternDef.Of` and pattern content survives detached.
- Law: boundary extraction states its frame — `Loops` selects hatch-plane 2d versus world 3d and perimeter versus holes as request data (`Get2dCurves`/`Get3dCurves` with the `outer` grant), never two sibling asks.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record PatternSnapshot(
    Guid Key,
    int Index,
    bool InUse,
    PatternDef Definition) : IDetachedDocumentResult;

public sealed record HatchDisplay(Seq<Curve> Bounds, Seq<Line> Lines, Option<Brep> Solid) : IDetachedDocumentResult;

// --- [OPERATIONS] ---------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HatchAsk {
    private HatchAsk() { }
    public sealed record PatternState(ResourceRef Target) : HatchAsk;
    public sealed record Defaults : HatchAsk;
    public sealed record MintName : HatchAsk;
    public sealed record Preview(ResourceRef Target, int Width, int Height, double Angle) : HatchAsk;
    public sealed record Display(TableTarget Target, double PatternScale = 1.0) : HatchAsk;
    public sealed record Loops(TableTarget Target, bool Outer = true, bool World = true) : HatchAsk;
    public sealed record Fill(TableTarget Target) : HatchAsk;
    public sealed record Solid(TableTarget Target) : HatchAsk;
    public sealed record Pieces(TableTarget Target) : HatchAsk;

    internal Fin<HatchAnswer> Answer(RhinoDoc document, Op op) =>
        Switch(
            context: (Document: document, Op: op),
            patternState: static (ctx, ask) =>
                from pattern in ask.Target.Resolve(document: ctx.Document, lens: HatchSpec.Lens, key: ctx.Op)
                from lines in ctx.Op.Catch(() => Fin.Succ(value: toSeq(pattern.HatchLines).Map(static line =>
                    new LineDef(Angle: line.Angle, Base: line.BasePoint, Offset: line.Offset, Dashes: toSeq(line.GetDashes)))))
                from fill in Optional(FillKind.TryGet((int)pattern.FillType, out FillKind? kind) ? kind : null)
                    .ToFin(Fail: ctx.Op.InvalidResult())
                select (HatchAnswer)new HatchAnswer.Pattern(Snapshot: new PatternSnapshot(
                    Key: pattern.Id,
                    Index: pattern.Index,
                    InUse: pattern.InUse,
                    Definition: new PatternDef(
                        Name: pattern.Name,
                        Description: Optional(pattern.Description).Filter(static text => text.Length > 0),
                        Fill: fill,
                        Units: pattern.PatternUnitSystem,
                        AlwaysModelDistances: pattern.AlwaysModelDistances,
                        Lines: lines))),
            defaults: static (ctx, _) => ctx.Op.Catch(() => Fin.Succ<HatchAnswer>(value: new HatchAnswer.Rows(
                Patterns: toSeq(HatchPattern.GetDefaultHatchPatterns())
                    .Map(static pattern => pattern.Name),
                CurrentIndex: ctx.Document.HatchPatterns.CurrentHatchPatternIndex))),
            mintName: static (ctx, _) =>
                from minted in ctx.Op.AcceptText(value: ctx.Document.HatchPatterns.GetUnusedHatchPatternName())
                select (HatchAnswer)new HatchAnswer.Minted(Name: minted),
            preview: static (ctx, ask) =>
                from pattern in ask.Target.Resolve(document: ctx.Document, lens: HatchSpec.Lens, key: ctx.Op)
                from _ in guard(ask.Width > 0 && ask.Height > 0, ctx.Op.InvalidInput()).ToFin()
                from angle in ctx.Op.AcceptInput(value: ask.Angle)
                from lines in ctx.Op.Catch(() => Optional(pattern.CreatePreviewGeometry(
                        width: ask.Width, height: ask.Height, angle: angle))
                    .Map(static preview => toSeq(preview))
                    .ToFin(Fail: ctx.Op.InvalidResult()))
                select (HatchAnswer)new HatchAnswer.Previewed(Lines: lines),
            display: static (ctx, ask) =>
                from hatch in Single(document: ctx.Document, target: ask.Target, key: ctx.Op)
                from scale in ctx.Op.Positive(value: ask.PatternScale)
                from pattern in Optional(ctx.Document.HatchPatterns.FindIndex(index: hatch.Geometry.PatternIndex))
                    .ToFin(Fail: ctx.Op.MissingContext())
                from display in ctx.Op.Catch(() => {
                    hatch.Geometry.CreateDisplayGeometry(
                        pattern: pattern, patternScale: scale,
                        bounds: out Curve[] bounds, lines: out Line[] lines, solidBrep: out Brep solid);
                    return Fin.Succ(value: new HatchDisplay(
                        Bounds: toSeq(bounds ?? []), Lines: toSeq(lines ?? []), Solid: Optional(solid)));
                })
                select (HatchAnswer)new HatchAnswer.Drawable(Display: display),
            loops: static (ctx, ask) =>
                from hatch in Single(document: ctx.Document, target: ask.Target, key: ctx.Op)
                from curves in ctx.Op.Catch(() => Fin.Succ(value: toSeq(ask.World
                    ? hatch.Geometry.Get3dCurves(outer: ask.Outer)
                    : hatch.Geometry.Get2dCurves(outer: ask.Outer))))
                select (HatchAnswer)new HatchAnswer.Boundary(Curves: curves),
            fill: static (ctx, ask) =>
                from hatch in Single(document: ctx.Document, target: ask.Target, key: ctx.Op)
                from gradient in ctx.Op.Catch(() => Fin.Succ(value: Optional(hatch.Geometry.GetGradientFill())))
                select (HatchAnswer)new HatchAnswer.Graded(Fill: gradient),
            solid: static (ctx, ask) =>
                from hatch in Single(document: ctx.Document, target: ask.Target, key: ctx.Op)
                from brep in ctx.Op.Catch(() => Optional(hatch.Geometry.ToBrep()).ToFin(Fail: ctx.Op.InvalidResult()))
                select (HatchAnswer)new HatchAnswer.Solidified(Region: brep),
            pieces: static (ctx, ask) =>
                from hatch in Single(document: ctx.Document, target: ask.Target, key: ctx.Op)
                from products in ctx.Op.Catch(() => Optional(hatch.Geometry.Explode())
                    .Map(static exploded => toSeq(exploded))
                    .ToFin(Fail: ctx.Op.InvalidResult()))
                select (HatchAnswer)new HatchAnswer.Pieces(Products: products));

    private static Fin<(Guid Id, Hatch Geometry)> Single(RhinoDoc document, TableTarget target, Op key) =>
        from ids in target.Resolve(document: document, key: key)
        from id in ids switch { [Guid only] => Fin.Succ(value: only), _ => Fin.Fail<Guid>(error: key.InvalidInput()) }
        from native in Optional(document.Objects.FindId(id)).ToFin(Fail: key.MissingContext())
        from hatch in Optional(native.Geometry as Hatch).ToFin(Fail: key.InvalidInput())
        select (id, hatch);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HatchAnswer : IDetachedDocumentResult {
    private HatchAnswer() { }
    public sealed record Pattern(PatternSnapshot Snapshot) : HatchAnswer;
    public sealed record Rows(Seq<string> Patterns, int CurrentIndex) : HatchAnswer;
    public sealed record Minted(string Name) : HatchAnswer;
    public sealed record Previewed(Seq<Line> Lines) : HatchAnswer;
    public sealed record Drawable(HatchDisplay Display) : HatchAnswer;
    public sealed record Boundary(Seq<Curve> Curves) : HatchAnswer;
    public sealed record Graded(Option<ColorGradient> Fill) : HatchAnswer;
    public sealed record Solidified(Brep Region) : HatchAnswer;
    public sealed record Pieces(Seq<GeometryBase> Products) : HatchAnswer;
}

public static class Hatches {
    public static Fin<DraftReceipt> Commit(DocumentSession session, HatchTransaction plan) {
        Op op = Op.Of();
        return from active in Optional(plan).ToFin(Fail: op.InvalidInput())
               from _ in guard(!active.Operations.IsEmpty, op.InvalidInput()).ToFin()
               from receipt in DraftSpine.Commit(
                   session: session, name: active.Name, redraw: active.Redraw, recording: active.UndoRecorded,
                   run: document => active.Operations
                       .TraverseM(operation => operation.Apply(document: document, op: op)).As()
                       .Map(static receipts => receipts.Fold(DraftReceipt.Empty, static (state, value) => state + value)),
                   op: op)
               select receipt;
    }

    public static Fin<HatchAnswer> Ask(DocumentSession session, HatchAsk request) {
        Op op = Op.Of();
        return from active in Optional(request).ToFin(Fail: op.InvalidInput())
               from answer in session.Demand(
                   use: document => active.Answer(document: document, op: op),
                   key: op,
                   needs: [SessionNeed.Read])
               select answer;
    }
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]           | [OWNER]           | [FORM]                                                | [ENTRY]                 |
| :-----: | :------------------ | :---------------- | :----------------------------------------------------- | :----------------------- |
|  [01]   | pattern content     | `PatternDef`      | identity + fill + unit + ordered `LineDef` run          | `HatchAsk.PatternState`  |
|  [02]   | built-in roster     | `HatchPattern`    | generated names feeding default authoring               | `HatchOp.AuthorDefault`  |
|  [03]   | hatch construction  | `HatchSpec`       | loops/nested/brep-face union, plural product            | `HatchOp.Place`          |
|  [04]   | pattern binding     | `FillPlacement`   | `ResourceRef` resolution, rotation, scale               | `Mint`                   |
|  [05]   | hatch mutations     | `HatchOp`         | table verbs + `.pat` interchange + placed-hatch amends  | `Hatches.Commit`         |
|  [06]   | drawable resolution | `HatchDisplay`    | bounds + pattern lines + solid brep in one call         | `HatchAsk.Display`       |
|  [07]   | hatch reads         | `HatchAsk`        | snapshot, defaults, preview, loops, fill, solid, pieces | `Hatches.Ask`            |

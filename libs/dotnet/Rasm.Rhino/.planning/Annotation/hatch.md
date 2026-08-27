# [RASM_RHINO_ANNOTATION_HATCH]

`PatternDef` round-trips complete detached pattern definitions, `HatchSpec` constructs placed fills through one boundary family, and `Hatches.Commit` folds pattern and object mutation through the shared drafting spine. Pattern-table lifecycle rides the shared `TableOp` over one `TableGrip`; placed state preserves every mutable hatch parameter, and native geometry crosses the document boundary through explicit custody.

`Rasm.Drawing` owns exact fill synthesis — winding-parity courses over region loops, through `Hatching.Apply` onto the host-neutral `HatchResult` wire — so this page holds the HOST hatch-table and placement altitude alone.

## [01]-[INDEX]

- [02]-[PATTERN]: generated pattern owners, line generators, tags, and native projection.
- [03]-[PLACEMENT]: the gradient owner, invariant-carrying placement, and the closed hatch construction family.
- [04]-[MUTATION]: the shared table verbs over the pattern grip, the generator list interface, and the placement, gradient, and scale operations this table alone carries.
- [05]-[PROJECTION]: complete pattern and placed state, previews, loops, display geometry, regions, and pieces.

## [02]-[PATTERN]

- Owner: `FillKind`, `PatternDistance`, `LineDef`, and `PatternDef` admit the complete detached pattern definition once.
- Law: `PatternDef` couples fill kind with line generators and preserves the pattern user-string bag in every read-modify-write cycle.
- Law: equality is GENERATED, never hand-written — the name comparison is `ResourceName`'s own declared ordinal-ignore-case policy, the generator run and the tag bag are structural by their carriers' own equality, and `==` is therefore the whole probe. A hand `Equivalent` beside a `[ComplexValueObject]` is a second authority over a fact the owner already answers, and it drifts the moment a column lands.
- Law: admission ACCUMULATES — a definition refused on its fill/generator agreement and on its tag bag alike reports both clauses, and every `is not null` re-check of an already-admitted owner is the interior re-validating what the boundary decided.
- Boundary: `PatternDef.Mint` shapes a fresh native and `Apply` shapes an existing duplicate through the same fold, so authoring and amendment cannot disagree on what a definition means; both verify `SetHatchLines` against the admitted generator count and treat every refused user-string write as failure.
- Packages: `Annotation/style.md` (`DraftAngle`, `TagEdit`, `TagSurface`, `TableGrip`, `TableOp`, `ListEdit`, `ListSurface`), `Document/session.md` (`DraftFault`), `Document/tables.md` (`ResourceName`, `ResourceIndex`, `TagOp.Snapshot`), `Domain/results` (`Custody`); RhinoCommon `HatchPattern`/`HatchLine` per `.api/api-rhinocommon-drafting-resources.md`; `Rasm.Drawing` `Hatching.Apply` for exact fill synthesis.
- Growth: a pattern attribute lands in `PatternDef`, its validation gate, `Apply`, and `Read`; no mutation case carries a partial parallel definition.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rasm.Rhino.Annotation;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class FillKind {
    public static readonly FillKind Solid = new(key: (int)HatchPatternFillType.Solid);
    public static readonly FillKind Lines = new(key: (int)HatchPatternFillType.Lines);
    public static readonly FillKind Gradient = new(key: (int)HatchPatternFillType.Gradient);

    internal HatchPatternFillType Host => (HatchPatternFillType)Key;
}

[SmartEnum<bool>]
public sealed partial class PatternDistance {
    public static readonly PatternDistance PatternUnits = new(key: false);
    public static readonly PatternDistance ModelUnits = new(key: true);
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError]
public sealed partial class LineDef {
    public DraftAngle Angle { get; }
    public Point2d Base { get; }
    public Vector2d Offset { get; }
    public Seq<SegmentRow> Dashes { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref DraftAngle angle, ref Point2d @base, ref Vector2d offset, ref Seq<SegmentRow> dashes) {
        (Point2d origin, Vector2d step) = (@base, offset);
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
            (!origin.IsValid, static () => new ValidationClause(string.Join(" | ", new object?[] { nameof(Base) }))),
            (!step.IsValid, static () => new ValidationClause(string.Join(" | ", new object?[] { nameof(Offset) })))));
    }

    public static Fin<LineDef> Of(DraftAngle angle, Point2d @base, Vector2d offset, Seq<SegmentRow> dashes) =>
        FactoryBridge.Accept<LineDef>(
            fault: Validate(angle, @base, offset, dashes, out LineDef? admitted), admitted: admitted);

    internal Fin<HatchLine> Mint() => Try.lift(() => {
        HatchLine line = new() { Angle = Angle.Value, BasePoint = Base, Offset = Offset };
        line.SetDashes(dashes: Dashes.Map(static row => row.Signed).AsIterable());
        return Fin.Succ(value: line);
    }).Run().Bind(static inner => inner);
}

[ComplexValueObject]
[ValidationError]
public sealed partial class PatternDef {
    public ResourceName Name { get; }
    public Option<string> Description { get; }
    public FillKind Fill { get; }
    public ModelUnit Units { get; }
    public PatternDistance Distances { get; }
    public Seq<LineDef> Lines { get; }
    public HashMap<string, string> Tags { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref ResourceName name, ref Option<string> description, ref FillKind fill,
        ref ModelUnit units, ref PatternDistance distances, ref Seq<LineDef> lines, ref HashMap<string, string> tags) {
        (FillKind kind, Seq<LineDef> generators, Option<string> text, HashMap<string, string> bag) =
            (fill, lines, description, tags);
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
            (kind == FillKind.Lines ? generators.IsEmpty : !generators.IsEmpty,
                () => new ValidationClause(string.Join(" | ", new object?[] { nameof(Lines), $"a generator run present for {FillKind.Lines.Key} and empty for every other fill" }))),
            (text.Exists(string.IsNullOrWhiteSpace),
                static () => new ValidationClause(string.Join(" | ", new object?[] { nameof(Description) }))),
            (!bag.ForAll(static pair => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value)),
                static () => new ValidationClause(string.Join(" | ", new object?[] { nameof(Tags) })))));
    }

    public static Fin<PatternDef> Of(
        ResourceName name, FillKind fill, ModelUnit units, PatternDistance distances, Seq<LineDef> lines,
        Option<string> description = default, HashMap<string, string> tags = default) =>
        FactoryBridge.Accept<PatternDef>(
            fault: Validate(name, description, fill, units, distances, lines, tags, out PatternDef? admitted),
            admitted: admitted);

    internal Fin<HatchPattern> Mint() =>
        from pattern in Try.lift(() => Fin.Succ(value: new HatchPattern())).Run().Bind(static inner => inner)
        from _ in Apply(pattern: pattern)
            .Rollback(release: () => Custody.Dispose(held: Seq(pattern)))
        select pattern;

    internal Fin<Unit> Apply(HatchPattern pattern) =>
        from lines in Lines.TraverseM(line => line.Mint()).As()
        from _ in Try.lift(() => Fin.Succ(value: HostEdge.Side(() => {
            pattern.Name = Name.Value;
            pattern.Description = Description.IfNone(noneValue: string.Empty);
            pattern.FillType = Fill.Host;
            pattern.PatternUnitSystem = Units.System;
            pattern.AlwaysModelDistances = Distances.Key;
        }))).Run().Bind(static inner => inner)
        from seated in Try.lift(() => Fin.Succ(value: pattern.SetHatchLines(hatchLines: lines.AsIterable()))).Run().Bind(static inner => inner)
        from __ in Admit.Confirm(success: seated == lines.Count)
        from ___ in new TagEdit.Replace(Tags: Tags).Apply(owner: Surface(pattern))
        select unit;

    internal static TagSurface Surface(HatchPattern pattern) => new(
        pattern.GetUserStrings, pattern.SetUserString, pattern.DeleteUserString, pattern.DeleteAllUserStrings);

    internal static Fin<HatchPattern> Canonical(HatchPattern pattern) =>
        Read(pattern: pattern).Bind(definition => definition.Mint());

    internal static Fin<PatternDef> Read(HatchPattern pattern) => Try.lift(() =>
        from fill in FactoryBridge.Accept<FillKind>(candidate: (int)pattern.FillType)
        from distances in FactoryBridge.Accept<PatternDistance>(candidate: pattern.AlwaysModelDistances)
        from lines in toSeq(pattern.HatchLines).TraverseM(line =>
            from dashes in toSeq(line.GetDashes).TraverseM(dash => SegmentRow.Of(
                length: double.Abs(dash), role: DashRole.For(signed: dash))).As()
            from angle in FactoryBridge.Accept<DraftAngle>(candidate: line.Angle)
            from admitted in LineDef.Of(
                angle: angle, @base: line.BasePoint, offset: line.Offset, dashes: dashes)
            select admitted).As()
        from units in ModelUnit.Of(value: pattern.PatternUnitSystem)
        from name in FactoryBridge.Accept<ResourceName>(candidate: pattern.Name)
        from definition in Of(
            name: name,
            fill: fill,
            units: units,
            distances: distances,
            lines: lines,
            description: Optional(pattern.Description).Filter(static text => !string.IsNullOrWhiteSpace(text)),
            tags: TagOp.Snapshot(pattern.GetUserStrings()))
        select definition).Run().Bind(static inner => inner);
}
```

## [03]-[PLACEMENT]

- Owner: `GradientForm`, `GradientStop`, and `FillGradient` own the host gradient whole — form row, `PerceptualColor` stop pairs, repeat factor, and segment — so `GetGradientFill`/`SetGradientFill` are the only places a raw `ColorGradient` exists and no host gradient crosses this page's surface in either direction.
- Owner: `FillPlacement` carries the resolved pattern address with the shared `DraftAngle` rotation and `DraftScale` scale; the section page reads the SAME host properties through the same owners.
- Owner: `HatchSpec` closes explicit loops, host-resolved nested loops, and planar-face construction under one plural product.
- Law: colour crosses the boundary through the kernel's ONE correspondence — `PerceptualColor.OfHost` inbound and `ToDrawing` outbound — so a gamut-refused stop lands on the carrier rather than inside a statement block, and this folder declares no colour round trip of its own.
- Law: the host-resolved arm's tolerance is a `Tolerance` on the `Closure` lane — the gap a boundary run may carry and still close into a loop — so the band decides admissibility once at its own gate instead of a positivity guard re-deciding at each construction.
- Boundary: `HatchSpec.Lens` remains the frozen pattern lookup interface and refuses id lookup because `HatchPatternTable` exposes only name and index resolution.
- Boundary: every boundary curve and source brep enters as a `GeometryHandle` and its native is read inside one `DraftBorrow` scope that also runs the arm's admission, so the loops crossing in name the same custody the loops crossing out do.
- Law: a drawing gradient form demands at least two stops and a non-degenerate segment; a held form keeps its stored rows so suppressing a gradient never destroys it.
- Law: curve construction accumulates `Requirement.AreaMass` failures before host dispatch and returns `Seq<Hatch>` because nested loops may partition into multiple fills.
- Packages: `Numerics/atoms` (`PerceptualColor.OfHost`/`ToDrawing`), `Domain/context` (`Context.Of`, `Tolerance`, `ToleranceLane.Closure`), `Domain/validation` (`Requirement.AreaMass`), `Annotation/style.md` (`DraftBorrow`, `DraftScale`, `DraftAngle`); RhinoCommon `ColorGradient`/`Hatch.Create`/`Hatch.CreateFromBrep`.
- Growth: a construction form is one `HatchSpec` case with its arm; a gradient axis is one `FillGradient` column.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class GradientForm {
    public static readonly GradientForm None = new(key: (int)GradientType.None, draws: false);
    public static readonly GradientForm Linear = new(key: (int)GradientType.Linear, draws: true);
    public static readonly GradientForm Radial = new(key: (int)GradientType.Radial, draws: true);
    public static readonly GradientForm LinearHeld = new(key: (int)GradientType.LinearDisabled, draws: false);
    public static readonly GradientForm RadialHeld = new(key: (int)GradientType.RadialDisabled, draws: false);

    internal bool Draws { get; }
    internal GradientType Host => (GradientType)Key;
}

[ComplexValueObject]
[ValidationError]
public sealed partial class GradientStop {
    public PerceptualColor Color { get; }
    public double Position { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref PerceptualColor color, ref double position) {
        double at = position;
        validationError = double.IsFinite(at)
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { nameof(Position), at, "a finite stop position" }));
    }

    public static Fin<GradientStop> Of(PerceptualColor color, double position) =>
        FactoryBridge.Accept<GradientStop>(
            fault: Validate(color, position, out GradientStop? admitted), admitted: admitted);
}

[ComplexValueObject]
[ValidationError]
public sealed partial class FillGradient {
    public GradientForm Form { get; }
    public double Repeat { get; }
    public Point3d Start { get; }
    public Point3d End { get; }
    public Seq<GradientStop> Stops { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref GradientForm form, ref double repeat,
        ref Point3d start, ref Point3d end, ref Seq<GradientStop> stops) {
        (GradientForm row, double factor, Point3d from, Point3d to, Seq<GradientStop> run) =
            (form, repeat, start, end, stops);
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
            (!double.IsFinite(factor), () => new ValidationClause(string.Join(" | ", new object?[] { nameof(Repeat), factor, "a finite signed repeat factor" }))),
            (!from.IsValid || !to.IsValid, static () => new ValidationClause(string.Join(" | ", new object?[] { nameof(Start) }))),
            (row.Draws && (run.Count < 2 || from == to), static () => new ValidationClause(string.Join(" | ", new object?[] { nameof(Stops), "two stops and a non-degenerate segment on a drawing gradient form" })))));
    }

    public static Fin<FillGradient> Of(
        GradientForm form, double repeat, Point3d start, Point3d end, Seq<GradientStop> stops) =>
        FactoryBridge.Accept<FillGradient>(
            fault: Validate(form, repeat, start, end, stops, out FillGradient? admitted), admitted: admitted);

    internal Fin<ColorGradient> Mint() =>
        from stops in Stops.TraverseM(stop => stop.Color.ToDrawing()
            .Map(color => new ColorStop(color: color, t: stop.Position))).As()
        from gradient in Try.lift(() => {
            ColorGradient value = new() {
                GradientType = Form.Host,
                Repeat = Repeat,
                StartPoint = Start,
                EndPoint = End,
            };
            value.SetColorStops(stops: stops.AsIterable());
            return Fin.Succ(value: value);
        }).Run().Bind(static inner => inner)
        select gradient;

    internal static Fin<FillGradient> Read(ColorGradient gradient) => Try.lift(() =>
        from form in FactoryBridge.Accept<GradientForm>(candidate: (int)gradient.GradientType)
        from stops in toSeq(gradient.GetColorStops()).TraverseM(stop =>
            from color in PerceptualColor.OfHost(host: stop.Color)
            from row in Of(color: color, position: stop.Position)
            select row).As()
        from admitted in Of(
            form: form,
            repeat: gradient.Repeat,
            start: gradient.StartPoint,
            end: gradient.EndPoint,
            stops: stops)
        select admitted).Run().Bind(static inner => inner);
}

[ComplexValueObject]
public sealed partial class FillPlacement {
    public ResourceRef Pattern { get; }
    public DraftAngle Rotation { get; }
    public DraftScale Scale { get; }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HatchSpec {
    private HatchSpec() { }
    public sealed record Bounded(Plane Plane, GeometryHandle Outer, Seq<GeometryHandle> Holes) : HatchSpec;
    public sealed record Resolved(Seq<GeometryHandle> Curves, Option<Tolerance> Tolerance = default) : HatchSpec;
    public sealed record FromFace(GeometryHandle Source, int FaceIndex, Point3d BasePoint) : HatchSpec;

    internal static readonly ResourceLens<HatchPattern> Lens = new(
        ById: static (_, _) => null,
        ByName: static (document, name) => document.HatchPatterns.FindName(name: name),
        ByIndex: static (document, index) => document.HatchPatterns.FindIndex(index: index));

    internal Fin<Seq<Hatch>> Mint(RhinoDoc document, FillPlacement placement) =>
        from pattern in placement.Pattern.Resolve(document: document, lens: Lens)
        from hatches in Switch(
            (Document: document, Index: pattern.Index, Placement: placement),
            bounded: static (context, spec) => spec.Outer.Typed<Curve, Seq<Hatch>>(project: outer =>
                spec.Holes.Typed<Curve, Seq<Hatch>>(project: holes =>
                    from _ in Acceptance.Rows(spec.Plane)
                    from __ in Area(document: context.Document, curves: Seq(outer) + holes)
                    from made in Try.lift(() => Optional(Hatch.Create(
                            hatchPlane: spec.Plane,
                            outerLoop: outer,
                            innerLoops: holes.AsIterable(),
                            hatchPatternIndex: context.Index,
                            rotationRadians: context.Placement.Rotation.Value,
                            scale: context.Placement.Scale.Value))
                        .Map(static hatch => Seq(hatch))
                        .ToFin(Fail: new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
                    select made)),
            resolved: static (context, spec) => spec.Curves.Typed<Curve, Seq<Hatch>>(project: curves =>
                from _ in guard(!curves.IsEmpty, new KernelFault.InvalidInput()).ToFin()
                from __ in Area(document: context.Document, curves: curves)
                from made in Try.lift(() => Optional(spec.Tolerance.Match(
                        Some: gate => Hatch.Create(
                            curves: curves.AsIterable(),
                            hatchPatternIndex: context.Index,
                            rotationRadians: context.Placement.Rotation.Value,
                            scale: context.Placement.Scale.Value,
                            tolerance: gate.Value),
                        None: () => Hatch.Create(
                            curves: curves.AsIterable(),
                            hatchPatternIndex: context.Index,
                            rotationRadians: context.Placement.Rotation.Value,
                            scale: context.Placement.Scale.Value)))
                    .Map(static values => toSeq(values))
                    .Filter(static values => !values.IsEmpty)
                    .ToFin(Fail: new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
                select made),
            fromFace: static (context, spec) => spec.Source.Typed<Brep, Seq<Hatch>>(project: brep =>
                from _ in guard(spec.FaceIndex >= 0 && spec.FaceIndex < brep.Faces.Count, new KernelFault.InvalidInput()).ToFin()
                from __ in Acceptance.Rows(spec.BasePoint)
                from made in Try.lift(() => Optional(Hatch.CreateFromBrep(
                        brep: brep,
                        brepFaceIndex: spec.FaceIndex,
                        hatchPatternIndex: context.Index,
                        rotationRadians: context.Placement.Rotation.Value,
                        scale: context.Placement.Scale.Value,
                        basePoint: spec.BasePoint))
                    .Map(static hatch => Seq(hatch))
                    .ToFin(Fail: new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
                select made))
        select hatches;

    private static Fin<Unit> Area(RhinoDoc document, Seq<Curve> curves) =>
        from context in Rasm.Domain.Context.Of(doc: document).ToFin()
        from admitted in curves.Traverse(curve => Requirement.AreaMass.Apply(
            context: context, value: curve)).As().ToFin()
        select unit;
}
```

## [04]-[MUTATION]

- Owner: `HatchProgram` is the pattern-table and placed-hatch mutation program consumed by `Hatches.Commit`: one `Table` case carrying the namespace's shared eight verbs over this table's grip, beside the five verbs this table alone has — stock-roster authoring, incremental generator revision, placement, gradient regrade, and pattern rescale.
- Law: authoring, amendment, renaming, retagging, deletion, current selection, and file interchange are the SHARED `TableOp` over `Grip` — the duplicate-then-`Modify` law, the compensated import, the plural delete arity, and the tag algebra are the namespace owner's and this page re-spells none of them. A page-local `Author`/`Amend`/`Rename`/`Retag`/`Delete`/`Import`/`Export` arm executes the deleted form.
- Law: amendment revises a DUPLICATE through `PatternDef.Apply` and lands through `Modify`, the same law every other component table walks — the whole-aggregate mint-and-replace this program once carried reached the same state by a second route, so the two disagreed on custody and only one of them released its native.
- Law: `Relist` is the incremental generator verb beside that whole-aggregate replacement, mirroring the linetype page's segment verb — `ListEdit<LineDef>` revises a native `HatchPattern` copy in place through the declared `ListSurface` and lands through the same `Modify`, so amending one generator of a thirty-line pattern never re-admits the twenty-nine it left alone and a legacy row whose angle or offset `LineDef` refuses stays editable.
- Law: the generator list publishes no in-place setter, so its surface leaves `Write` absent and the shared vocabulary spells a replace as a bounded remove-then-append ONCE; the list's floor is zero, so `Clear` is legal here and refuses on the linetype list with the same declaration.
- Law: an empty generator set under `FillKind.Lines` refuses at the next `PatternDef.Read`, so a `Relist` batch proves a non-empty result before `Modify`.
- Boundary: default and imported host patterns cross `PatternDef.Read` and `Mint`, so one canonical detached shape reaches every table addition; each host-minted roster — the `GetDefaultHatchPatterns` array and the `ReadFromFile` batch alike — releases through kernel `Custody` the moment its canonical copy exists, and the grip's file row carries that drain so the shared import arm never sees a raw native.
- Boundary: placement rollback deletes landed objects while the release policy settles every minted native on both outcomes; placed-hatch rework retains original and revised clones through compensation, and a custody refusal after commit restores the originals.
- Law: absence never crosses as `null` — an optional attribute set and the unused history slot project through the kernel's one host-slot spelling.
- Entry: `Hatches.Commit` preserves the frozen wire and accepts `DraftPlan<HatchProgram>` with shared redraw and undo policy.
- Packages: `Annotation/style.md` (`TableGrip`, `TableOp`, `ListEdit`, `ListSurface`, `TagSurface`, `DraftPlan`, `DraftSpine`), `Document/commit.md` (`DocumentCommit.Compensated`, `HostInteraction`), `Domain/results` (`Custody`, `Lease<T>`, `HostEdge.Slot`); RhinoCommon `HatchPatternTable`/`HatchPattern.ReadFromFile`/`WriteToFile`.
- Growth: a verb every component table shares lands on `TableOp`; a hatch-only verb is one case here.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HatchProgram {
    private HatchProgram() { }
    public sealed record Table(TableOp<HatchPattern, PatternDef> Verb) : HatchProgram;
    public sealed record AuthorDefault(ResourceName Name, HostInteraction Interaction) : HatchProgram;
    public sealed record Relist(ResourceRef Target, Seq<ListEdit<LineDef>> Edits, HostInteraction Interaction) : HatchProgram;
    public sealed record Place(HatchSpec Spec, FillPlacement Placement, Option<ObjectAttributes> Attributes = default) : HatchProgram;
    public sealed record Regrade(TableTarget Target, FillGradient Fill) : HatchProgram;
    public sealed record Rescale(TableTarget Target, Transform Motion) : HatchProgram;

    internal static readonly TableGrip<HatchPattern, PatternDef> Grip = new(
        HatchSpec.Lens,
        Named: static def => def.Name,
        Title: static (pattern, key) => FactoryBridge.Accept<ResourceName>(candidate: pattern.Name),
        Index: static pattern => pattern.Index,
        Duplicate: static pattern => new HatchPattern(other: pattern),
        Tags: PatternDef.Surface,
        Mint: static (_, def, key) => def.Mint(),
        Revise: static (_, copy, def, key) => def.Apply(pattern: copy),
        Retitle: static (copy, name, key) => Try.lift(() => Fin.Succ(value: HostEdge.Side(() => copy.Name = name.Value))).Run().Bind(static inner => inner),
        Modify: static (document, copy, index, interaction, key) => Admit.Confirm(success: document.HatchPatterns.Modify(
            hatchPattern: copy, hatchPatternIndex: index, quiet: interaction.IsQuiet)),
        Seat: static (document, pattern, key) => Try.lift(() => ResourceIndex.Admit(
            document.HatchPatterns.Add(pattern: pattern))).Run().Bind(static inner => inner),
        Retire: static (document, indices, interaction, key) =>
            from removed in Try.lift(() => Fin.Succ(value: document.HatchPatterns.Delete(
                hatchPatternIndices: indices.AsIterable(), quiet: interaction.IsQuiet))).Run().Bind(static inner => inner)
            from _ in guard(removed == indices.Count, new KernelFault.InvalidResult())
            select unit,
        Elect: static (document, index, _, key) => Try.lift(() => Fin.Succ(value: HostEdge.Side(
            () => document.HatchPatterns.CurrentHatchPatternIndex = index))).Run().Bind(static inner => inner),
        Ingest: static (path, interaction, key) =>
            from raw in Try.lift(() => Optional(HatchPattern.ReadFromFile(
                    filename: path.Value, quiet: interaction.IsQuiet))
                .Map(static patterns => toSeq(patterns).Strict())
                .ToFin(Fail: new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
            from canonical in raw.TraverseM(pattern => PatternDef.Canonical(pattern: pattern)).As()
                .Rollback(release: () => Custody.Dispose(held: raw))
            from _ in Custody.Dispose(held: raw)
            select canonical,
        Emit: static (path, patterns, key) => Admit.Confirm(success: HatchPattern.WriteToFile(
            filename: path.Value, hatchPatterns: patterns.AsIterable())));

    internal Fin<Unit> Apply(RhinoDoc document) => Switch(
        document,
        table: static (context, edit) => edit.Verb.Apply(grip: Grip, document: context),
        authorDefault: static (context, edit) =>
            from stock in Try.lift(() => Fin.Succ(value: toSeq(HatchPattern.GetDefaultHatchPatterns()).Strict())).Run().Bind(static inner => inner)
            from definition in stock
                .Find(candidate => string.Equals(candidate.Name, edit.Name.Value, StringComparison.OrdinalIgnoreCase))
                .ToFin(Fail: new KernelFault.MissingContext())
                .Bind(source => PatternDef.Read(pattern: source))
                .Rollback(release: () => Custody.Dispose(held: stock))
            from _ in Custody.Dispose(held: stock)
            from __ in new TableOp<HatchPattern, PatternDef>.Author(Def: definition, Interaction: edit.Interaction)
                .Apply(grip: Grip, document: context)
            select unit,
        relist: static (context, edit) =>
            from _ in guard(!edit.Edits.IsEmpty, new KernelFault.InvalidInput()).ToFin()
            from __ in Grip.Revised(
                target: edit.Target, document: context,
                interaction: edit.Interaction,
                revise: (copy, key) =>
                    from applied in edit.Edits.TraverseM(row => row.Apply(surface: Generators(copy))).As()
                    from __ in guard(copy.FillType != HatchPatternFillType.Lines || copy.HatchLineCount > 0,
                        new KernelFault.InvalidResult())
                    select unit)
            select unit,
        place: static (context, edit) =>
            from hatches in edit.Spec.Mint(document: context, placement: edit.Placement)
            from _ in DocumentCommit.Compensated(
                source: hatches,
                land: hatch => Try.lift(() => ResourceId.Admit(context.Objects.Add(
                    geometry: hatch,
                    attributes: HostEdge.Slot(edit.Attributes),
                    history: HostEdge.Slot(Option<HistoryRecord>.None),
                    reference: false))).Run().Bind(static inner => inner),
                rollback: landed => Admit.Confirm(success: context.Objects.Delete(
                    objectIds: landed.Map(static id => id.Value).AsIterable(), quiet: true) == landed.Count),
                release: minted => Custody.Dispose(held: minted))
            select unit,
        regrade: static (context, edit) => Reworked(
            document: context, target: edit.Target,
            change: (hatch, key) => edit.Fill.Mint()
                .Bind(fill => Try.lift(() => Fin.Succ(value: HostEdge.Side(() => hatch.SetGradientFill(fill: fill)))).Run().Bind(static inner => inner))),
        rescale: static (context, edit) => Reworked(
            document: context, target: edit.Target,
            change: (hatch, key) => Acceptance.Rows(edit.Motion)
                .Bind(_ => Try.lift(() => Fin.Succ(value: HostEdge.Side(() => hatch.ScalePattern(xform: edit.Motion)))).Run().Bind(static inner => inner))));

    private static ListSurface<LineDef> Generators(HatchPattern pattern) => new(
        Count: () => pattern.HatchLineCount,
        Append: (row, key) =>
            from line in row.Mint()
            from index in Try.lift(() => Fin.Succ(value: pattern.AddHatchLine(hatchLine: line))).Run().Bind(static inner => inner)
            from _ in guard(index >= 0, new KernelFault.InvalidResult())
            select unit,
        Remove: (index, key) => Admit.Confirm(success: pattern.RemoveHatchLine(hatchLineIndex: index)),
        Write: default,
        Purge: Some<Func< Fin<Unit>>>(() => Try.lift(() => Fin.Succ(value: HostEdge.Side(pattern.RemoveAllHatchLines))).Run().Bind(static inner => inner)),
        Floor: 0);

    private sealed record HatchRevision(Guid Id, Hatch Original, Hatch Revised) {
        internal Seq<Hatch> Custody => Seq(Original, Revised);
    }

    private static Fin<Unit> Reworked(
        RhinoDoc document, TableTarget target, Func<Hatch, Fin<Unit>> change) =>
        from ids in target.Resolve(document: document)
        from revisions in DocumentCommit.Compensated(
            source: ids,
            land: id => Prepare(document: document, id: id, change: change),
            rollback: landed => Custody.Dispose(held: landed.Bind(static row => row.Custody)))
        from _ in DocumentCommit.Compensated(
            source: revisions,
            land: revision => Admit.Confirm(success: document.Objects.Replace(
                    objectId: revision.Id, geometry: revision.Revised, ignoreModes: false))
                .Map(_ => (revision.Id, revision.Original)),
            rollback: landed => landed.Traverse(row => Admit.Confirm(success: document.Objects.Replace(
                    objectId: row.Id, geometry: row.Original, ignoreModes: false))
                .ToValidation()).As().ToFin().Map(static _ => unit),
            release: rows => Custody.Dispose(held: rows.Bind(static row => row.Custody)))
        select unit;

    private static Fin<HatchRevision> Prepare(
        RhinoDoc document, Guid id, Func<Hatch, Fin<Unit>> change) =>
            from native in Optional(document.Objects.FindId(id)).ToFin(Fail: new KernelFault.MissingContext())
            from source in Admit.Need(native.Geometry as Hatch)
            from original in Try.lift(() => Optional(source.Duplicate() as Hatch).ToFin(Fail: new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
            from revised in Try.lift(() => Optional(source.Duplicate() as Hatch).ToFin(Fail: new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
                .Rollback(release: () => Custody.Dispose(held: Seq(original)))
            from _ in change(revised, op)
                .Rollback(release: () => Custody.Dispose(held: Seq(original, revised)))
            select new HatchRevision(Id: id, Original: original, Revised: revised);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Hatches {
    public static Fin<Unit> Commit(DocumentSession session, DraftPlan<HatchProgram> plan) =>
        DraftSpine.Commit(session: session, plan: plan,
            apply: static (document, operation, key) => operation.Apply(document: document));

    public static Fin<HatchAnswer> Ask(DocumentSession session, HatchAsk request) {
        return from admitted in Acceptance.Input(value: request)
               from answer in Admit.Demand(
                   use: document => admitted.Answer(document: document), needs: [SessionNeed.Read])
               select answer;
    }
}
```

## [05]-[PROJECTION]

- Owner: `PatternSnapshot` and `HatchState` preserve the complete detached definition and every mutable placed-hatch parameter; `HatchDisplay` carries the bound run, the pattern lines, and the optional solid region as three TYPED columns.
- Law: `LoopKind` and `LoopFrame` replace independent boolean switches with named perimeter and coordinate-frame values, and the FRAME row carries its own host reader — the ternary that once selected between `Get3dCurves` and `Get2dCurves` names the boolean-driven branch a behaviour column deletes, exactly as the dimension family's length channel does. The loops they classify are shape-compatible ingress for the kernel's `HatchOp.Regions` — exact fill synthesized over host-authored boundaries.
- Law: a preview names its extent through the namespace's shared `PreviewSpec`, so the ceiling, the overflow-safe pixel product, and the positivity of both edges are the kernel `AssetExtent`'s and no arm re-guards them.
- Law: `CreateDisplayGeometry` returns `void` with three `out` slots and no verdict, so the read funnels through ONE `Fin<(Seq<Curve>, Seq<Line>, Option<Brep>)>` where presence IS the evidence: an absent bound array reads as the empty run it is, an absent brep is `None`, and neither degrades to a null-coalesced `[]` a consumer reads as "the host answered nothing".
- Boundary: loops, display bounds, solid regions, and exploded pieces cross through `DraftCrossing.Crossed`, the namespace's one detach fold, which releases accepted handles on crossing refusal and raw products on every exit; the display read crosses its bounds and its solid as TWO batches so the solid handle is recovered by NAME, never by ordinal into a fused run, and a refusal on the second batch releases the first.
- Boundary: custody release rides the RESULT — `HatchAnswer.Release` returns `Fin<Unit>` and the accumulated release fault reaches the caller typed, where a `void` disposer swallows it.
- Law: default-pattern projection returns full `PatternDef` rows rather than a name census, so built-in capability remains authorable without a second lookup grammar.
- Packages: `Annotation/style.md` (`PreviewSpec`, `PreviewSurface`, `DraftCrossing`, `DraftScale`, `DraftAngle`), `Interaction/asset.md` (`AssetExtent`), `Document/tables.md` (`GeometryHandle`), `Domain/results` (`Custody`); RhinoCommon `Hatch.Get3dCurves`/`Get2dCurves`/`ToBrep`/`Explode`/`CreateDisplayGeometry` and `HatchPattern.CreatePreviewGeometry`.
- Growth: a read is one `HatchAsk` case with its `HatchAnswer` twin; a loop axis is one row carrying its own host reader.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<bool>]
public sealed partial class LoopKind {
    public static readonly LoopKind Perimeter = new(key: true);
    public static readonly LoopKind Voids = new(key: false);
}

[SmartEnum<bool>]
public sealed partial class LoopFrame {
    public static readonly LoopFrame World = new(key: true, read: static (hatch, kind) => hatch.Get3dCurves(outer: kind.Key));
    public static readonly LoopFrame Plane = new(key: false, read: static (hatch, kind) => hatch.Get2dCurves(outer: kind.Key));

    [UseDelegateFromConstructor]
    internal partial Curve[] Read(Hatch hatch, LoopKind kind);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PatternSnapshot(
    ResourceId Key,
    ResourceIndex Index,
    bool InUse,
    PatternDef Definition) : IDetachedDocumentResult;

public sealed record HatchState(
    ResourceId Key,
    ResourceIndex Pattern,
    Plane Plane,
    Point3d BasePoint,
    DraftAngle PatternRotation,
    DraftScale PatternScale,
    Option<FillGradient> Gradient) : IDetachedDocumentResult;

public sealed record HatchDisplay(
    Seq<GeometryHandle> Bounds,
    Seq<Line> Lines,
    Option<GeometryHandle> Solid) : IDetachedDocumentResult;

// --- [OPERATIONS] ----------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HatchAsk {
    private HatchAsk() { }
    public sealed record PatternState(ResourceRef Target) : HatchAsk;
    public sealed record Defaults : HatchAsk;
    public sealed record MintName : HatchAsk;
    public sealed record Preview(ResourceRef Target, PreviewSpec Extent, DraftAngle Angle) : HatchAsk;
    public sealed record State(TableTarget Target) : HatchAsk;
    public sealed record Display(TableTarget Target, DraftScale PatternScale) : HatchAsk;
    public sealed record Loops(TableTarget Target, LoopKind Kind, LoopFrame Frame) : HatchAsk;
    public sealed record Solid(TableTarget Target) : HatchAsk;
    public sealed record Pieces(TableTarget Target) : HatchAsk;

    internal Fin<HatchAnswer> Answer(RhinoDoc document) => Switch(
        document,
        patternState: static (context, ask) =>
            from pattern in ask.Target.Resolve(document: context, lens: HatchSpec.Lens)
            from definition in PatternDef.Read(pattern: pattern)
            select (HatchAnswer)new HatchAnswer.Pattern(new PatternSnapshot(
                ResourceId.Create(pattern.Id), ResourceIndex.Create(pattern.Index), pattern.InUse, definition)),
        defaults: static (context, _) =>
            from stock in Try.lift(() => Fin.Succ(value: toSeq(HatchPattern.GetDefaultHatchPatterns()).Strict())).Run().Bind(static inner => inner)
            from definitions in stock.TraverseM(pattern => PatternDef.Read(pattern: pattern)).As()
                .Rollback(release: () => Custody.Dispose(held: stock))
            from _ in Custody.Dispose(held: stock)
            select (HatchAnswer)new HatchAnswer.Definitions(
                definitions, ResourceIndex.Create(context.HatchPatterns.CurrentHatchPatternIndex)),
        mintName: static (context, _) =>
            from name in FactoryBridge.Accept<ResourceName>(candidate: context.HatchPatterns.GetUnusedHatchPatternName())
            select (HatchAnswer)new HatchAnswer.Minted(name),
        preview: static (context, ask) =>
            from pattern in ask.Target.Resolve(document: context, lens: HatchSpec.Lens)
            from lines in Try.lift(() => Optional(pattern.CreatePreviewGeometry(
                    width: ask.Extent.Extent.PixelWidth, height: ask.Extent.Extent.PixelHeight, angle: ask.Angle.Value))
                .Map(static values => toSeq(values))
                .ToFin(Fail: new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
            select (HatchAnswer)new HatchAnswer.Previewed(lines),
        state: static (context, ask) =>
            from hatch in Single(document: context, target: ask.Target)
            from gradient in Optional(hatch.Geometry.GetGradientFill())
                .Traverse(native => FillGradient.Read(gradient: native)).As()
            from rotation in FactoryBridge.Accept<DraftAngle>(candidate: hatch.Geometry.PatternRotation)
            from scale in FactoryBridge.Accept<DraftScale>(candidate: hatch.Geometry.PatternScale)
            select (HatchAnswer)new HatchAnswer.Placed(new HatchState(
                ResourceId.Create(hatch.Id),
                ResourceIndex.Create(hatch.Geometry.PatternIndex),
                hatch.Geometry.Plane,
                hatch.Geometry.BasePoint,
                rotation,
                scale,
                gradient)),
        display: static (context, ask) =>
            from hatch in Single(document: context, target: ask.Target)
            from pattern in Optional(context.HatchPatterns.FindIndex(index: hatch.Geometry.PatternIndex))
                .ToFin(Fail: new KernelFault.MissingContext())
            from raw in DisplayGeometry(
                hatch: hatch.Geometry, pattern: pattern, scale: ask.PatternScale)
            from bounds in DraftCrossing.Crossed(products: raw.Bounds)
            from solid in raw.Solid
                .Traverse(brep => DraftCrossing.Crossed(products: Seq(brep))).As()
                .Rollback(release: () => Custody.Dispose(held: bounds))
            select (HatchAnswer)new HatchAnswer.Drawable(new HatchDisplay(
                Bounds: bounds,
                Lines: raw.Lines,
                Solid: solid.Bind(static handles => handles.Head))),
        loops: static (context, ask) =>
            from hatch in Single(document: context, target: ask.Target)
            from curves in Try.lift(() => Fin.Succ(value: toSeq(
                ask.Frame.Read(hatch: hatch.Geometry, kind: ask.Kind)))).Run().Bind(static inner => inner)
            from handles in DraftCrossing.Crossed(products: curves)
            select (HatchAnswer)new HatchAnswer.Boundary(handles),
        solid: static (context, ask) =>
            from hatch in Single(document: context, target: ask.Target)
            from brep in Try.lift(() => Optional(hatch.Geometry.ToBrep()).ToFin(
                Fail: new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
            from handles in DraftCrossing.Crossed(products: Seq(brep))
            from region in handles.Head.ToFin(Fail: new KernelFault.InvalidResult())
            select (HatchAnswer)new HatchAnswer.Solidified(region),
        pieces: static (context, ask) =>
            from hatch in Single(document: context, target: ask.Target)
            from products in Try.lift(() => Optional(hatch.Geometry.Explode())
                .Map(static values => toSeq(values))
                .ToFin(Fail: new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
            from handles in DraftCrossing.Crossed(products: products)
            select (HatchAnswer)new HatchAnswer.Pieces(handles));

    private static Fin<(Seq<Curve> Bounds, Seq<Line> Lines, Option<Brep> Solid)> DisplayGeometry(
        Hatch hatch, HatchPattern pattern, DraftScale scale) =>
        Try.lift(() => {
            hatch.CreateDisplayGeometry(
                pattern: pattern,
                patternScale: scale.Value,
                bounds: out Curve[] bounds,
                lines: out Line[] lines,
                solidBrep: out Brep solid);
            return Fin.Succ(value: (
                Bounds: Optional(bounds).Map(toSeq).IfNone(Seq<Curve>()),
                Lines: Optional(lines).Map(toSeq).IfNone(Seq<Line>()),
                Solid: Optional(solid)));
        }).Run().Bind(static inner => inner);

    private static Fin<(Guid Id, Hatch Geometry)> Single(RhinoDoc document, TableTarget target) =>
        from row in target.Only<RhinoObject>(document: document)
        from hatch in Admit.Need(row.Native.Geometry as Hatch)
        select (row.Id, hatch);
}

[Union(MapMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads, ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HatchAnswer : IDetachedDocumentResult {
    private HatchAnswer() { }
    public sealed record Pattern(PatternSnapshot Snapshot) : HatchAnswer;
    public sealed record Definitions(Seq<PatternDef> Patterns, ResourceIndex Current) : HatchAnswer;
    public sealed record Minted(ResourceName Name) : HatchAnswer;
    public sealed record Previewed(Seq<Line> Lines) : HatchAnswer;
    public sealed record Placed(HatchState Snapshot) : HatchAnswer;
    public sealed record Drawable(HatchDisplay Display) : HatchAnswer;
    public sealed record Boundary(Seq<GeometryHandle> Curves) : HatchAnswer;
    public sealed record Solidified(GeometryHandle Region) : HatchAnswer;
    public sealed record Pieces(Seq<GeometryHandle> Products) : HatchAnswer;

    public Fin<Unit> Release() => SwitchPartially(
        @default: static _ => Fin.Succ(value: unit),
        drawable: static (row) => Custody.Dispose(
            held: row.Display.Bounds + row.Display.Solid.ToSeq()),
        boundary: static (row) => Custody.Dispose(held: row.Curves),
        solidified: static (row) => Custody.Dispose(held: Seq(row.Region)),
        pieces: static (row) => Custody.Dispose(held: row.Products));
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

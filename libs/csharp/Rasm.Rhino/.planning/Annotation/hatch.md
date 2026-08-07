# [RASM_RHINO_ANNOTATION_HATCH]

`PatternDef` round-trips complete detached pattern definitions, `HatchSpec` constructs placed fills through one boundary family, and `Hatches.Commit` folds pattern and object mutation through the shared drafting spine. Pattern batches compensate failed additions, placed state preserves every mutable hatch parameter, and all native geometry crosses the document boundary through explicit custody.

## [01]-[INDEX]

- [02]-[PATTERN]: generated pattern owners, line generators, tags, and native projection.
- [03]-[PLACEMENT]: the gradient owner, invariant-carrying placement, and the closed hatch construction family.
- [04]-[MUTATION]: atomic pattern lifecycle, the `LineEdit` incremental generator vocabulary, placement, gradient, and scale operations.
- [05]-[PROJECTION]: complete pattern and placed state, previews, loops, display geometry, regions, and pieces.

## [02]-[PATTERN]

- Owner: `FillKind`, `PatternDistance`, `LineDef`, and `PatternDef` admit the complete detached pattern definition once.
- Law: `PatternDef` couples fill kind with line generators and preserves the pattern user-string bag in every read-modify-write cycle.
- Boundary: `PatternDef.Mint` verifies `SetHatchLines` against the admitted generator count and treats every refused user-string write as failure.
- Growth: a pattern attribute lands in `PatternDef`, its validation gate, `Mint`, and `Read`; no mutation case carries a partial parallel definition.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using LanguageExt.UnsafeValueAccess;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rasm.Rhino.Annotation;

// --- [TYPES] --------------------------------------------------------------------------------
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

// --- [MODELS] -------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class LineDef {
    public DraftAngle Angle { get; }
    public Point2d Base { get; }
    public Vector2d Offset { get; }
    public Seq<SegmentRow> Dashes { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref DraftAngle angle, ref Point2d @base, ref Vector2d offset, ref Seq<SegmentRow> dashes) =>
        validationError = angle is not null && @base.IsValid && offset.IsValid
            && dashes.ForAll(static dash => dash is not null)
            ? null
            : new ValidationError(message: "Hatch line definition is invalid.");

    public static Fin<LineDef> Of(DraftAngle angle, Point2d @base, Vector2d offset, Seq<SegmentRow> dashes, Op? key = null) =>
        Admission.Admitted(
            fault: Validate(angle, @base, offset, dashes, out LineDef? admitted),
            value: admitted,
            refusal: key.OrDefault().InvalidInput());

    internal Fin<HatchLine> Mint(Op key) => key.Catch(() => {
        HatchLine line = new() { Angle = Angle.Value, BasePoint = Base, Offset = Offset };
        line.SetDashes(dashes: Dashes.Map(static row => row.Signed).AsIterable());
        return Fin.Succ(value: line);
    });
}

[ComplexValueObject]
public sealed partial class PatternDef {
    public ResourceName Name { get; }
    public Option<string> Description { get; }
    public FillKind Fill { get; }
    public ModelUnit Units { get; }
    public PatternDistance Distances { get; }
    public Seq<LineDef> Lines { get; }
    public HashMap<string, string> Tags { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref ResourceName name, ref Option<string> description, ref FillKind fill,
        ref ModelUnit units, ref PatternDistance distances, ref Seq<LineDef> lines, ref HashMap<string, string> tags) {
        bool shape = fill == FillKind.Lines ? !lines.IsEmpty : lines.IsEmpty;
        bool validTags = tags.ForAll(static pair =>
            !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value));
        validationError = name is not null && fill is not null && units is not null && distances is not null
            && description.ForAll(static text => !string.IsNullOrWhiteSpace(text))
            && lines.ForAll(static line => line is not null) && shape && validTags
            ? null
            : new ValidationError(message: "Hatch pattern definition is invalid.");
    }

    public static Fin<PatternDef> Of(
        ResourceName name, FillKind fill, ModelUnit units, PatternDistance distances, Seq<LineDef> lines,
        Option<string> description = default, HashMap<string, string> tags = default, Op? key = null) =>
        Admission.Admitted(
            fault: Validate(name, description, fill, units, distances, lines, tags, out PatternDef? admitted),
            value: admitted,
            refusal: key.OrDefault().InvalidInput());

    internal Fin<HatchPattern> Mint(Op key) =>
        from lines in Lines.TraverseM(line => line.Mint(key: key)).As()
        from pattern in key.Catch(() => Fin.Succ(value: new HatchPattern {
            Name = Name.Value,
            Description = Description.IfNone(noneValue: string.Empty),
            FillType = Fill.Host,
            PatternUnitSystem = Units.System,
            AlwaysModelDistances = Distances.Key,
        }))
        from count in key.Catch(() => Fin.Succ(value: pattern.SetHatchLines(hatchLines: lines.AsIterable())))
        from _ in key.Confirm(success: count == lines.Count)
        from __ in TagBag.Apply(Tags, new TagSurface(pattern.GetUserStrings, pattern.SetUserString, pattern.DeleteAllUserStrings), key)
        select pattern;

    internal bool Equivalent(PatternDef other) => other is not null
        && string.Equals(Name.Value, other.Name.Value, StringComparison.OrdinalIgnoreCase)
        && Description.Equals(other.Description)
        && Fill.Equals(other.Fill)
        && Units.Equals(other.Units)
        && Distances.Equals(other.Distances)
        && Lines.AsIterable().SequenceEqual(other.Lines.AsIterable())
        && Tags.Count == other.Tags.Count
        && Tags.ForAll(pair => other.Tags.Find(pair.Key).Match(
            Some: value => string.Equals(value, pair.Value, StringComparison.Ordinal),
            None: static () => false));

    internal static Fin<HatchPattern> Canonical(HatchPattern pattern, Op key) =>
        Read(pattern: pattern, key: key).Bind(definition => definition.Mint(key: key));

    internal static Fin<PatternDef> Read(HatchPattern pattern, Op key) => key.Catch(() =>
        from fill in key.AcceptValidated<FillKind>(candidate: (int)pattern.FillType)
        from distances in key.AcceptValidated<PatternDistance>(candidate: pattern.AlwaysModelDistances)
        from lines in toSeq(pattern.HatchLines).TraverseM(line =>
            from dashes in toSeq(line.GetDashes).TraverseM(dash => SegmentRow.Of(
                length: double.Abs(dash), solid: dash >= 0.0, key: key)).As()
            from admitted in LineDef.Of(
                angle: DraftAngle.Create(line.Angle), @base: line.BasePoint, offset: line.Offset, dashes: dashes, key: key)
            select admitted).As()
        from units in ModelUnit.Of(value: pattern.PatternUnitSystem, key: key)
        from definition in Of(
            name: ResourceName.Create(pattern.Name),
            fill: fill,
            units: units,
            distances: distances,
            lines: lines,
            description: Optional(pattern.Description).Filter(static text => !string.IsNullOrWhiteSpace(text)),
            tags: TagOp.Snapshot(pattern.GetUserStrings()),
            key: key)
        select definition);
}
```

## [03]-[PLACEMENT]

- Owner: `GradientForm`, `GradientStop`, and `FillGradient` own the host gradient whole — form row, `PerceptualColor` stop pairs, repeat factor, and segment — so `GetGradientFill`/`SetGradientFill` are the only places a raw `ColorGradient` exists and no host gradient crosses this page's surface in either direction.
- Owner: `FillPlacement` carries the resolved pattern address with the shared `DraftAngle` rotation and `DraftScale` scale through one generated aggregate gate; the section page reads the SAME host properties through the same owners.
- Owner: `HatchSpec` closes explicit loops, host-resolved nested loops, and planar-face construction under one plural product.
- Boundary: `HatchSpec.Lens` remains the frozen pattern lookup seam and refuses id lookup because `HatchPatternTable` exposes only name and index resolution.
- Boundary: every boundary curve and source brep enters as a `GeometryHandle` and its native is read inside one `DraftBorrow` scope that also runs the arm's admission, so the loops crossing in name the same custody the loops crossing out do.
- Law: a drawing gradient form demands at least two stops and a non-degenerate segment; a held form keeps its stored rows so suppressing a gradient never destroys it.
- Law: curve construction accumulates `Requirement.AreaMass` failures before host dispatch and returns `Seq<Hatch>` because nested loops may partition into multiple fills.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class GradientForm {
    public static readonly GradientForm None = new(key: (int)GradientType.None, draws: false);
    public static readonly GradientForm Linear = new(key: (int)GradientType.Linear, draws: true);
    public static readonly GradientForm Radial = new(key: (int)GradientType.Radial, draws: true);
    public static readonly GradientForm LinearHeld = new(key: (int)GradientType.LinearDisabled, draws: false);
    public static readonly GradientForm RadialHeld = new(key: (int)GradientType.RadialDisabled, draws: false);

    // The two `*Disabled` rows keep a gradient's stops and geometry on the hatch while suppressing its draw, so a
    // consumer asks the column instead of testing two enum members it would have to re-spell.
    internal bool Draws { get; }
    internal GradientType Host => (GradientType)Key;
}

[ComplexValueObject]
public sealed partial class GradientStop {
    public PerceptualColor Color { get; }
    public double Position { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref PerceptualColor color, ref double position) =>
        validationError = color is not null && double.IsFinite(position)
            ? null
            : new ValidationError(message: "Gradient stop is invalid.");

    public static Fin<GradientStop> Of(PerceptualColor color, double position, Op? key = null) =>
        Admission.Admitted(
            fault: Validate(color, position, out GradientStop? admitted),
            value: admitted,
            refusal: key.OrDefault().InvalidInput());
}

[ComplexValueObject]
public sealed partial class FillGradient {
    public GradientForm Form { get; }
    // `Repeat` is SIGNED: above one it reflects, below minus-one it wraps, so the positive-only `DraftScale` owner
    // would refuse every wrapped gradient the host writes; the gate here is finiteness alone.
    public double Repeat { get; }
    public Point3d Start { get; }
    public Point3d End { get; }
    public Seq<GradientStop> Stops { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref GradientForm form, ref double repeat,
        ref Point3d start, ref Point3d end, ref Seq<GradientStop> stops) =>
        validationError = form is not null && double.IsFinite(repeat)
            && start.IsValid && end.IsValid
            && stops.ForAll(static stop => stop is not null)
            && (!form.Draws || (stops.Count >= 2 && start != end))
            ? null
            : new ValidationError(message: "Gradient fill is invalid.");

    public static Fin<FillGradient> Of(
        GradientForm form, double repeat, Point3d start, Point3d end, Seq<GradientStop> stops, Op? key = null) =>
        Admission.Admitted(
            fault: Validate(form, repeat, start, end, stops, out FillGradient? admitted),
            value: admitted,
            refusal: key.OrDefault().InvalidInput());

    internal Fin<ColorGradient> Mint(Op key) => key.Catch(() => {
        ColorGradient gradient = new() {
            GradientType = Form.Host,
            Repeat = Repeat,
            StartPoint = Start,
            EndPoint = End,
        };
        gradient.SetColorStops(stops: Stops.Map(static stop =>
            new ColorStop(color: stop.Color.Sys(), t: stop.Position)).AsIterable());
        return Fin.Succ(value: gradient);
    });

    internal static Fin<FillGradient> Read(ColorGradient gradient, Op key) => key.Catch(() =>
        from form in key.AcceptValidated<GradientForm>(candidate: (int)gradient.GradientType)
        from stops in toSeq(gradient.GetColorStops()).TraverseM(stop =>
            from color in stop.Color.Admitted(key)
            from row in Of(color: color, position: stop.Position, key: key)
            select row).As()
        from admitted in Of(
            form: form,
            repeat: gradient.Repeat,
            start: gradient.StartPoint,
            end: gradient.EndPoint,
            stops: stops,
            key: key)
        select admitted);
}

[ComplexValueObject]
public sealed partial class FillPlacement {
    public ResourceRef Pattern { get; }
    public DraftAngle Rotation { get; }
    public DraftScale Scale { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref ResourceRef pattern, ref DraftAngle rotation, ref DraftScale scale) =>
        validationError = pattern is not null && rotation is not null && scale is not null
            ? null
            : new ValidationError(message: "Hatch placement is invalid.");

    public static Fin<FillPlacement> Of(ResourceRef pattern, DraftAngle rotation, DraftScale scale, Op? key = null) =>
        Admission.Admitted(
            fault: Validate(pattern, rotation, scale, out FillPlacement? admitted),
            value: admitted,
            refusal: key.OrDefault().InvalidInput());
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HatchSpec {
    private HatchSpec() { }
    public sealed record Bounded(Plane Plane, GeometryHandle Outer, Seq<GeometryHandle> Holes) : HatchSpec;
    public sealed record Resolved(Seq<GeometryHandle> Curves, Option<double> Tolerance = default) : HatchSpec;
    public sealed record FromFace(GeometryHandle Source, int FaceIndex, Point3d BasePoint) : HatchSpec;

    internal static readonly ResourceLens<HatchPattern> Lens = new(
        ById: static (_, _) => null,
        ByName: static (document, name) => document.HatchPatterns.FindName(name: name),
        ByIndex: static (document, index) => document.HatchPatterns.FindIndex(index: index));

    // Admission and construction share ONE borrow scope per arm: a boundary curve lives only inside its handle's
    // lease, so an area-mass gate run in a prior pass would read natives the scope has already closed.
    internal Fin<Seq<Hatch>> Mint(RhinoDoc document, FillPlacement placement, Op op) =>
        from pattern in placement.Pattern.Resolve(document: document, lens: Lens, key: op)
        from hatches in Switch(
            (Document: document, Index: pattern.Index, Placement: placement, Op: op),
            bounded: static (context, spec) => spec.Outer.Typed<Curve, Seq<Hatch>>(key: context.Op, project: outer =>
                spec.Holes.Typed<Curve, Seq<Hatch>>(key: context.Op, project: holes =>
                    from _ in context.Op.AcceptInput(value: spec.Plane)
                    from __ in Area(document: context.Document, curves: Seq(outer) + holes)
                    from made in context.Op.Catch(() => Optional(Hatch.Create(
                            hatchPlane: spec.Plane,
                            outerLoop: outer,
                            innerLoops: holes.AsIterable(),
                            hatchPatternIndex: context.Index,
                            rotationRadians: context.Placement.Rotation.Value,
                            scale: context.Placement.Scale.Value))
                        .Map(static hatch => Seq(hatch))
                        .ToFin(Fail: context.Op.InvalidResult()))
                    select made)),
            resolved: static (context, spec) => spec.Curves.Typed<Curve, Seq<Hatch>>(key: context.Op, project: curves =>
                from _ in guard(!curves.IsEmpty, context.Op.InvalidInput()).ToFin()
                from __ in Area(document: context.Document, curves: curves)
                from tolerance in spec.Tolerance.Traverse(value => context.Op.Positive(value: value)).As()
                from made in context.Op.Catch(() => Optional(tolerance.Match(
                        Some: value => Hatch.Create(
                            curves: curves.AsIterable(),
                            hatchPatternIndex: context.Index,
                            rotationRadians: context.Placement.Rotation.Value,
                            scale: context.Placement.Scale.Value,
                            tolerance: value),
                        None: () => Hatch.Create(
                            curves: curves.AsIterable(),
                            hatchPatternIndex: context.Index,
                            rotationRadians: context.Placement.Rotation.Value,
                            scale: context.Placement.Scale.Value)))
                    .Map(static values => toSeq(values))
                    .Filter(static values => !values.IsEmpty)
                    .ToFin(Fail: context.Op.InvalidResult()))
                select made),
            fromFace: static (context, spec) => spec.Source.Typed<Brep, Seq<Hatch>>(key: context.Op, project: brep =>
                from _ in guard(spec.FaceIndex >= 0 && spec.FaceIndex < brep.Faces.Count, context.Op.InvalidInput()).ToFin()
                from __ in context.Op.AcceptInput(value: spec.BasePoint)
                from made in context.Op.Catch(() => Optional(Hatch.CreateFromBrep(
                        brep: brep,
                        brepFaceIndex: spec.FaceIndex,
                        hatchPatternIndex: context.Index,
                        rotationRadians: context.Placement.Rotation.Value,
                        scale: context.Placement.Scale.Value,
                        basePoint: spec.BasePoint))
                    .Map(static hatch => Seq(hatch))
                    .ToFin(Fail: context.Op.InvalidResult()))
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

- Owner: `HatchOp` is the complete pattern-table and placed-hatch mutation program consumed by `Hatches.Commit`; `LineEdit` folds append, replace, remove, and clear into one detached generator-revision vocabulary `Reline` carries.
- Law: pattern import, multi-hatch placement, and batch replacement fold through the shared `DocumentCommit.Compensated` landed-state algebra, with minted and cloned custody settled through its source-release policy.
- Boundary: default and imported host patterns cross `PatternDef.Read` and `Mint`, so one canonical detached shape reaches every table addition; each host-minted roster — the `GetDefaultHatchPatterns` array and the `ReadFromFile` batch alike — releases through `DraftCustody` the moment its canonical copy exists, and every canonical copy leases across the `Add` the table copies it into.
- Law: delete resolves and deduplicates every target before one batch table call; one retained row refuses the whole request.
- Boundary: placement rollback deletes landed objects while the release policy settles every minted native on both outcomes; compensation faults accumulate without masking the initiating refusal.
- Law: pattern amendment always mints a complete `PatternDef` and lands through `Modify` — a fresh admitted aggregate replaces the row, the named discriminant that keeps this rail off the `TableGrip` duplicate-then-revise law; `Retag` first reconstructs that definition, so tag-only edits cannot erase generators or config.
- Law: `Reline` is the incremental generator rail beside that whole-aggregate replacement, mirroring the linetype page's `Resegment`/`SegmentEdit` pair — `LineEdit` revises a native `HatchPattern` copy in place and lands through the same `Modify`, so amending one generator of a thirty-line pattern never re-admits the twenty-nine it left alone and a legacy row whose angle or offset `LineDef` refuses stays editable. `new HatchPattern(other)` copy-constructs the whole native pattern including its name, the same shape the linetype `TableGrip` takes for the same reason.
- Law: every `LineEdit` index bounds against the live `HatchLineCount` read from the copy, and `Clear` is the one edit that may empty a `Lines` pattern — an empty generator set under `FillKind.Lines` refuses at the next `PatternDef.Read`, so the batch proves a non-empty result before `Modify`.
- Boundary: placed-hatch rework retains original and revised clones through compensation; the release policy settles both clones on every outcome, and a custody refusal after commit restores the originals.
- Entry: `Hatches.Commit` preserves the frozen wire and accepts `DraftPlan<HatchOp>` with shared redraw and undo policy.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LineEdit {
    private LineEdit() { }
    public sealed record Append(LineDef Line) : LineEdit;
    public sealed record Replace(int Index, LineDef Line) : LineEdit;
    public sealed record Remove(int Index) : LineEdit;
    public sealed record Clear : LineEdit;

    internal Fin<Unit> Apply(HatchPattern pattern, Op key) => Switch(
        (Pattern: pattern, Op: key),
        append: static (context, edit) =>
            from line in edit.Line.Mint(key: context.Op)
            from index in context.Op.Catch(() => Fin.Succ(value: context.Pattern.AddHatchLine(hatchLine: line)))
            from _ in guard(index >= 0, context.Op.InvalidResult()).ToFin()
            select unit,
        replace: static (context, edit) =>
            from _ in guard(edit.Index >= 0 && edit.Index < context.Pattern.HatchLineCount, context.Op.InvalidInput()).ToFin()
            from line in edit.Line.Mint(key: context.Op)
            // Host truth: the generator list carries no in-place setter, so a replace is a bounded remove then append.
            from __ in context.Op.Confirm(success: context.Pattern.RemoveHatchLine(hatchLineIndex: edit.Index))
            from index in context.Op.Catch(() => Fin.Succ(value: context.Pattern.AddHatchLine(hatchLine: line)))
            from ___ in guard(index >= 0, context.Op.InvalidResult()).ToFin()
            select unit,
        remove: static (context, edit) =>
            from _ in guard(edit.Index >= 0 && edit.Index < context.Pattern.HatchLineCount, context.Op.InvalidInput()).ToFin()
            from __ in context.Op.Confirm(success: context.Pattern.RemoveHatchLine(hatchLineIndex: edit.Index))
            select unit,
        clear: static (context, _) => context.Op.Catch(() => Op.Side(context.Pattern.RemoveAllHatchLines)));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HatchOp {
    private HatchOp() { }
    public sealed record Author(PatternDef Def) : HatchOp;
    public sealed record AuthorDefault(ResourceName Name) : HatchOp;
    public sealed record Amend(ResourceRef Target, PatternDef Def, HostInteraction Interaction) : HatchOp;
    public sealed record Reline(ResourceRef Target, Seq<LineEdit> Edits, HostInteraction Interaction) : HatchOp;
    public sealed record Retag(ResourceRef Target, HashMap<string, string> Tags, HostInteraction Interaction) : HatchOp;
    public sealed record Rename(ResourceRef Target, ResourceName Name) : HatchOp;
    public sealed record Delete(Seq<ResourceRef> Targets, HostInteraction Interaction) : HatchOp;
    public sealed record Import(DraftPath Path, HostInteraction Interaction) : HatchOp;
    public sealed record Export(DraftPath Path, Seq<ResourceRef> Targets) : HatchOp;
    public sealed record Place(HatchSpec Spec, FillPlacement Placement, Option<ObjectAttributes> Attributes = default) : HatchOp;
    public sealed record Regrade(TableTarget Target, FillGradient Fill) : HatchOp;
    public sealed record Rescale(TableTarget Target, Transform Motion) : HatchOp;

    internal Fin<DraftReceipt> Apply(RhinoDoc document, Op op) => Switch(
        (Document: document, Op: op),
        author: static (context, edit) =>
            from _ in guard(context.Document.HatchPatterns.FindName(name: edit.Def.Name.Value) is null,
                context.Op.InvalidInput()).ToFin()
            from pattern in edit.Def.Mint(key: context.Op)
            from receipt in new Lease<HatchPattern>.Owned(Value: pattern).Use(owned =>
                from index in Added(document: context.Document, pattern: owned, op: context.Op)
                from authored in DraftReceipt.Component(
                    slot: DraftSlot.Authored, componentKind: DraftComponentKind.Hatch, index: index, key: context.Op)
                select authored)
            select receipt,
        // `GetDefaultHatchPatterns` hands back a whole freshly minted array, so the roster releases after the one
        // row it answers is canonicalized, and the canonical copy releases once the table has copied it.
        authorDefault: static (context, edit) =>
            from stock in context.Op.Catch(() => Fin.Succ(value: toSeq(HatchPattern.GetDefaultHatchPatterns()).Strict()))
            from pattern in stock
                .Find(candidate => string.Equals(candidate.Name, edit.Name.Value, StringComparison.OrdinalIgnoreCase))
                .ToFin(Fail: context.Op.MissingContext())
                .Bind(source => PatternDef.Canonical(pattern: source, key: context.Op))
                .BindFail(primary => DraftCustody.Failed<HatchPattern, HatchPattern>(
                    primary: primary, values: stock, op: context.Op))
            from _ in DraftCustody.Release(values: stock, op: context.Op)
            from receipt in new Lease<HatchPattern>.Owned(Value: pattern).Use(owned =>
                from __ in guard(context.Document.HatchPatterns.FindName(name: owned.Name) is null,
                    context.Op.InvalidInput()).ToFin()
                from index in Added(document: context.Document, pattern: owned, op: context.Op)
                from authored in DraftReceipt.Component(
                    slot: DraftSlot.Authored, componentKind: DraftComponentKind.Hatch, index: index, key: context.Op)
                select authored)
            select receipt,
        amend: static (context, edit) => ReplacePattern(
            document: context.Document, target: edit.Target, definition: edit.Def, interaction: edit.Interaction,
            slot: DraftSlot.Amended, op: context.Op),
        reline: static (context, edit) =>
            from _ in guard(!edit.Edits.IsEmpty && edit.Edits.ForAll(static row => row is not null),
                context.Op.InvalidInput()).ToFin()
            from active in edit.Target.Resolve(document: context.Document, lens: HatchSpec.Lens, key: context.Op)
            from receipt in Revised(
                document: context.Document, active: active, interaction: edit.Interaction, op: context.Op,
                revise: (copy, key) =>
                    from applied in edit.Edits.TraverseM(row => row.Apply(pattern: copy, key: key)).As()
                    from __ in guard(copy.FillType != HatchPatternFillType.Lines || copy.HatchLineCount > 0,
                        key.InvalidResult()).ToFin()
                    select unit)
            select receipt,
        retag: static (context, edit) =>
            from pattern in edit.Target.Resolve(document: context.Document, lens: HatchSpec.Lens, key: context.Op)
            from current in PatternDef.Read(pattern: pattern, key: context.Op)
            from changed in PatternDef.Of(
                name: current.Name, fill: current.Fill, units: current.Units, distances: current.Distances,
                lines: current.Lines, description: current.Description, tags: edit.Tags, key: context.Op)
            from receipt in ReplacePattern(
                document: context.Document, target: edit.Target, definition: changed, interaction: edit.Interaction,
                slot: DraftSlot.Amended, op: context.Op)
            select receipt,
        rename: static (context, edit) =>
            from pattern in edit.Target.Resolve(document: context.Document, lens: HatchSpec.Lens, key: context.Op)
            from _ in context.Op.Confirm(success: context.Document.HatchPatterns.Rename(
                hatchPatternIndex: pattern.Index, hatchPatternName: edit.Name.Value))
            from receipt in DraftReceipt.Component(
                slot: DraftSlot.Renamed, componentKind: DraftComponentKind.Hatch,
                index: ResourceIndex.Create(pattern.Index), key: context.Op)
            select receipt,
        delete: static (context, edit) =>
            from _ in guard(!edit.Targets.IsEmpty, context.Op.InvalidInput()).ToFin()
            from patterns in edit.Targets.TraverseM(target => target.Resolve(
                document: context.Document, lens: HatchSpec.Lens, key: context.Op)).As()
            let indices = patterns.Map(static pattern => pattern.Index)
            from __ in guard(indices.Distinct().Count == indices.Count, context.Op.InvalidInput()).ToFin()
            from deleted in context.Op.Catch(() => Fin.Succ(value: context.Document.HatchPatterns.Delete(
                hatchPatternIndices: indices.AsIterable(), quiet: edit.Interaction.IsQuiet)))
            from ___ in guard(deleted == indices.Count, context.Op.InvalidResult()).ToFin()
            from receipts in indices.TraverseM(index => DraftReceipt.Component(
                slot: DraftSlot.Deleted, componentKind: DraftComponentKind.Hatch,
                index: ResourceIndex.Create(index), key: context.Op)).As()
            select receipts.Fold(DraftReceipt.Empty, static (receipt, next) => receipt + next),
        import: static (context, edit) =>
            from read in context.Op.Catch(() => Optional(HatchPattern.ReadFromFile(
                    filename: edit.Path.Value, quiet: edit.Interaction.IsQuiet))
                .Map(static patterns => toSeq(patterns))
                .Filter(static patterns => !patterns.IsEmpty)
                .ToFin(Fail: context.Op.InvalidResult()))
            from patterns in read.TraverseM(pattern => PatternDef.Canonical(pattern: pattern, key: context.Op)).As()
                .BindFail(primary => DraftCustody.Failed<Seq<HatchPattern>, HatchPattern>(
                    primary: primary, values: read, op: context.Op))
            from raw in DraftCustody.Release(values: read, op: context.Op)
            from _ in guard(
                patterns.AsIterable().Select(static pattern => pattern.Name)
                    .Distinct(StringComparer.OrdinalIgnoreCase).Count() == patterns.Count
                && !patterns.Exists(pattern => context.Document.HatchPatterns.FindName(name: pattern.Name) is not null),
                context.Op.InvalidInput()).ToFin()
            from indices in DocumentCommit.Compensated(
                source: patterns,
                land: pattern => Added(document: context.Document, pattern: pattern, op: context.Op),
                rollback: landed => context.Op.Confirm(success: context.Document.HatchPatterns.Delete(
                    hatchPatternIndices: landed.Map(static index => index.Value).AsIterable(), quiet: true) == landed.Count),
                release: minted => DraftCustody.Release(values: minted, op: context.Op))
            from pathReceipt in DraftReceipt.Path(slot: DraftSlot.Imported, path: edit.Path, key: context.Op)
            from components in indices.TraverseM(index => DraftReceipt.Component(
                slot: DraftSlot.Imported, componentKind: DraftComponentKind.Hatch, index: index, key: context.Op)).As()
            select components.Fold(pathReceipt, static (state, receipt) => state + receipt),
        export: static (context, edit) =>
            from patterns in edit.Targets.TraverseM(target => target.Resolve(
                document: context.Document, lens: HatchSpec.Lens, key: context.Op)).As()
            from _ in guard(!patterns.IsEmpty, context.Op.InvalidInput()).ToFin()
            from __ in context.Op.Confirm(success: HatchPattern.WriteToFile(
                filename: edit.Path.Value, hatchPatterns: patterns.AsIterable()))
            from receipt in DraftReceipt.Path(slot: DraftSlot.Exported, path: edit.Path, key: context.Op)
            select receipt,
        place: static (context, edit) =>
            from hatches in edit.Spec.Mint(document: context.Document, placement: edit.Placement, op: context.Op)
            from ids in DocumentCommit.Compensated(
                source: hatches,
                land: hatch => context.Op.Catch(() => ResourceId.Admit(context.Document.Objects.Add(
                    geometry: hatch,
                    attributes: edit.Attributes.ValueUnsafe(),
                    history: null,
                    reference: false), context.Op)),
                rollback: landed => context.Op.Confirm(success: context.Document.Objects.Delete(
                    objectIds: landed.Map(static id => id.Value).AsIterable(), quiet: true) == landed.Count),
                release: minted => DraftCustody.Release(values: minted, op: context.Op))
            from receipt in DraftReceipt.Objects(slot: DraftSlot.Placed, ids: ids, key: context.Op)
            select receipt,
        regrade: static (context, edit) => Reworked(
            document: context.Document, target: edit.Target, op: context.Op, slot: DraftSlot.Restyled,
            change: (hatch, key) => key.AcceptInput(value: edit.Fill)
                .Bind(admitted => admitted.Mint(key: key))
                .Bind(fill => key.Catch(() => hatch.SetGradientFill(fill: fill)))),
        rescale: static (context, edit) => Reworked(
            document: context.Document, target: edit.Target, op: context.Op, slot: DraftSlot.Scaled,
            change: (hatch, key) => key.AcceptInput(value: edit.Motion)
                .Bind(_ => key.Catch(() => hatch.ScalePattern(xform: edit.Motion)))));

    private static Fin<DraftReceipt> ReplacePattern(
        RhinoDoc document, ResourceRef target, PatternDef definition, HostInteraction interaction, DraftSlot slot, Op op) =>
        from active in target.Resolve(document: document, lens: HatchSpec.Lens, key: op)
        from minted in definition.Mint(key: op)
        from _ in op.Confirm(success: document.HatchPatterns.Modify(
            hatchPattern: minted, hatchPatternIndex: active.Index, quiet: interaction.IsQuiet))
        from receipt in DraftReceipt.Component(
            slot: slot, componentKind: DraftComponentKind.Hatch, index: ResourceIndex.Create(active.Index), key: op)
        select receipt;

    private static Fin<DraftReceipt> Revised(
        RhinoDoc document, HatchPattern active, HostInteraction interaction, Op op, Func<HatchPattern, Op, Fin<Unit>> revise) =>
        from copy in op.Catch(() => Optional(new HatchPattern(other: active)).ToFin(Fail: op.InvalidResult()))
        from _ in revise(copy, op).BindFail(primary => DraftCustody.Failed<Unit, HatchPattern>(primary: primary, values: Seq(copy), op: op))
        from __ in op.Confirm(success: document.HatchPatterns.Modify(
                hatchPattern: copy, hatchPatternIndex: active.Index, quiet: interaction.IsQuiet))
            .BindFail(primary => DraftCustody.Failed<Unit, HatchPattern>(primary: primary, values: Seq(copy), op: op))
        from ___ in DraftCustody.Release(values: Seq(copy), op: op)
        from receipt in DraftReceipt.Component(
            slot: DraftSlot.Amended, componentKind: DraftComponentKind.Hatch, index: ResourceIndex.Create(active.Index), key: op)
        select receipt;

    private static Fin<ResourceIndex> Added(RhinoDoc document, HatchPattern pattern, Op op) =>
        op.Catch(() => ResourceIndex.Admit(document.HatchPatterns.Add(pattern: pattern), op));

    private sealed record HatchRevision(Guid Id, Hatch Original, Hatch Revised) {
        internal Seq<Hatch> Custody => Seq(Original, Revised);
    }

    private static Fin<DraftReceipt> Reworked(
        RhinoDoc document, TableTarget target, Op op, DraftSlot slot, Func<Hatch, Op, Fin<Unit>> change) =>
        from ids in target.Resolve(document: document, key: op)
        from revisions in DocumentCommit.Compensated(
            source: ids,
            land: id => Prepare(document: document, id: id, change: change, op: op),
            rollback: landed => DraftCustody.Release(values: landed.Bind(static row => row.Custody), op: op))
        from amended in DocumentCommit.Compensated(
            source: revisions,
            land: revision => op.Confirm(success: document.Objects.Replace(
                    objectId: revision.Id, geometry: revision.Revised, ignoreModes: false))
                .Map(_ => (revision.Id, revision.Original)),
            rollback: landed => landed.Traverse(row => op.Confirm(success: document.Objects.Replace(
                    objectId: row.Id, geometry: row.Original, ignoreModes: false))
                .ToValidation()).As().ToFin().Map(static _ => unit),
            release: rows => DraftCustody.Release(values: rows.Bind(static row => row.Custody), op: op))
        from receipt in DraftReceipt.Objects(slot: slot, ids: amended.Map(static row => ResourceId.Create(row.Id)), key: op)
        select receipt;

    private static Fin<HatchRevision> Prepare(
        RhinoDoc document, Guid id, Func<Hatch, Op, Fin<Unit>> change, Op op) =>
            from native in Optional(document.Objects.FindId(id)).ToFin(Fail: op.MissingContext())
            from source in op.Need(native.Geometry as Hatch)
            from original in op.Catch(() => Optional(source.Duplicate() as Hatch).ToFin(Fail: op.InvalidResult()))
            from revised in op.Catch(() => Optional(source.Duplicate() as Hatch).ToFin(Fail: op.InvalidResult()))
                .BindFail(primary => DraftCustody.Failed<Hatch, Hatch>(primary: primary, values: Seq(original), op: op))
            from _ in change(revised, op)
                .BindFail(primary => DraftCustody.Failed<Unit, Hatch>(primary: primary, values: Seq(original, revised), op: op))
            select new HatchRevision(Id: id, Original: original, Revised: revised);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Hatches {
    public static Fin<DraftReceipt> Commit(DocumentSession session, DraftPlan<HatchOp> plan) =>
        DraftSpine.Commit(session: session, plan: plan,
            apply: static (document, operation, key) => operation.Apply(document: document, op: key),
            op: Op.Of(name: nameof(Hatches)));

    public static Fin<HatchAnswer> Ask(DocumentSession session, HatchAsk request) {
        Op op = Op.Of(name: nameof(Hatches));
        return session.Demand(use: document => request.Answer(document: document, op: op), key: op, needs: [SessionNeed.Read]);
    }
}
```

## [05]-[PROJECTION]

- Owner: `PatternSnapshot` and `HatchState` preserve the complete detached definition and every mutable placed-hatch parameter.
- Law: `LoopKind` and `LoopFrame` replace independent boolean switches with named perimeter and coordinate-frame values.
- Boundary: loops, display bounds, solid regions, and exploded pieces cross through `DraftCustody.Crossed`, the namespace's one detach fold, which releases accepted handles on crossing refusal and raw products on every exit; answer cases expose only `GeometryHandle` custody.
- Law: default-pattern projection returns full `PatternDef` rows rather than a name census, so built-in capability remains authorable without a second lookup grammar.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<bool>]
public sealed partial class LoopKind {
    public static readonly LoopKind Perimeter = new(key: true);
    public static readonly LoopKind Voids = new(key: false);
}

[SmartEnum<bool>]
public sealed partial class LoopFrame {
    public static readonly LoopFrame World = new(key: true);
    public static readonly LoopFrame Plane = new(key: false);
}

// --- [MODELS] -------------------------------------------------------------------------------
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

// --- [OPERATIONS] ---------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HatchAsk {
    private HatchAsk() { }
    public sealed record PatternState(ResourceRef Target) : HatchAsk;
    public sealed record Defaults : HatchAsk;
    public sealed record MintName : HatchAsk;
    public sealed record Preview(ResourceRef Target, int Width, int Height, DraftAngle Angle) : HatchAsk;
    public sealed record State(TableTarget Target) : HatchAsk;
    public sealed record Display(TableTarget Target, DraftScale PatternScale) : HatchAsk;
    public sealed record Loops(TableTarget Target, LoopKind Kind, LoopFrame Frame) : HatchAsk;
    public sealed record Solid(TableTarget Target) : HatchAsk;
    public sealed record Pieces(TableTarget Target) : HatchAsk;

    internal Fin<HatchAnswer> Answer(RhinoDoc document, Op op) => Switch(
        (Document: document, Op: op),
        patternState: static (context, ask) =>
            from pattern in ask.Target.Resolve(document: context.Document, lens: HatchSpec.Lens, key: context.Op)
            from definition in PatternDef.Read(pattern: pattern, key: context.Op)
            select (HatchAnswer)new HatchAnswer.Pattern(new PatternSnapshot(
                ResourceId.Create(pattern.Id), ResourceIndex.Create(pattern.Index), pattern.InUse, definition)),
        defaults: static (context, _) =>
            from stock in context.Op.Catch(() => Fin.Succ(value: toSeq(HatchPattern.GetDefaultHatchPatterns()).Strict()))
            from definitions in stock.TraverseM(pattern => PatternDef.Read(pattern: pattern, key: context.Op)).As()
                .BindFail(primary => DraftCustody.Failed<Seq<PatternDef>, HatchPattern>(
                    primary: primary, values: stock, op: context.Op))
            from _ in DraftCustody.Release(values: stock, op: context.Op)
            select (HatchAnswer)new HatchAnswer.Definitions(
                definitions, ResourceIndex.Create(context.Document.HatchPatterns.CurrentHatchPatternIndex)),
        mintName: static (context, _) =>
            from name in context.Op.AcceptText(value: context.Document.HatchPatterns.GetUnusedHatchPatternName())
            select (HatchAnswer)new HatchAnswer.Minted(ResourceName.Create(name)),
        preview: static (context, ask) =>
            from pattern in ask.Target.Resolve(document: context.Document, lens: HatchSpec.Lens, key: context.Op)
            from _ in guard(ask.Width > 0 && ask.Height > 0, context.Op.InvalidInput()).ToFin()
            from lines in context.Op.Catch(() => Optional(pattern.CreatePreviewGeometry(
                    width: ask.Width, height: ask.Height, angle: ask.Angle.Value))
                .Map(static values => toSeq(values))
                .ToFin(Fail: context.Op.InvalidResult()))
            select (HatchAnswer)new HatchAnswer.Previewed(lines),
        state: static (context, ask) =>
            from hatch in Single(document: context.Document, target: ask.Target, key: context.Op)
            from gradient in Optional(hatch.Geometry.GetGradientFill())
                .Traverse(native => FillGradient.Read(gradient: native, key: context.Op)).As()
            select (HatchAnswer)new HatchAnswer.Placed(new HatchState(
                ResourceId.Create(hatch.Id),
                ResourceIndex.Create(hatch.Geometry.PatternIndex),
                hatch.Geometry.Plane,
                hatch.Geometry.BasePoint,
                DraftAngle.Create(hatch.Geometry.PatternRotation),
                DraftScale.Create(hatch.Geometry.PatternScale),
                gradient)),
        display: static (context, ask) =>
            from hatch in Single(document: context.Document, target: ask.Target, key: context.Op)
            from pattern in Optional(context.Document.HatchPatterns.FindIndex(index: hatch.Geometry.PatternIndex))
                .ToFin(Fail: context.Op.MissingContext())
            from raw in context.Op.Catch(() => {
                hatch.Geometry.CreateDisplayGeometry(
                    pattern: pattern,
                    patternScale: ask.PatternScale.Value,
                    bounds: out Curve[] bounds,
                    lines: out Line[] lines,
                    solidBrep: out Brep solid);
                return Fin.Succ(value: (
                    Bounds: toSeq(bounds ?? []),
                    Lines: toSeq(lines ?? []),
                    Solid: Optional(solid)));
            })
            let products = raw.Bounds.Map(static value => (GeometryBase)value)
                + raw.Solid.Match(
                    Some: static value => Seq<GeometryBase>(value),
                    None: static () => Seq<GeometryBase>())
            from handles in DraftCustody.Crossed(products: products, op: context.Op)
            select (HatchAnswer)new HatchAnswer.Drawable(new HatchDisplay(
                Bounds: handles.Take(raw.Bounds.Count),
                Lines: raw.Lines,
                Solid: raw.Solid.Map(_ => handles[raw.Bounds.Count]))),
        loops: static (context, ask) =>
            from hatch in Single(document: context.Document, target: ask.Target, key: context.Op)
            from curves in context.Op.Catch(() => Fin.Succ(value: toSeq(ask.Frame.Key
                ? hatch.Geometry.Get3dCurves(outer: ask.Kind.Key)
                : hatch.Geometry.Get2dCurves(outer: ask.Kind.Key))))
            from handles in DraftCustody.Crossed(products: curves, op: context.Op)
            select (HatchAnswer)new HatchAnswer.Boundary(handles),
        solid: static (context, ask) =>
            from hatch in Single(document: context.Document, target: ask.Target, key: context.Op)
            from brep in context.Op.Catch(() => Optional(hatch.Geometry.ToBrep()).ToFin(
                Fail: context.Op.InvalidResult()))
            from handles in DraftCustody.Crossed(products: Seq(brep), op: context.Op)
            select (HatchAnswer)new HatchAnswer.Solidified(handles[0]),
        pieces: static (context, ask) =>
            from hatch in Single(document: context.Document, target: ask.Target, key: context.Op)
            from products in context.Op.Catch(() => Optional(hatch.Geometry.Explode())
                .Map(static values => toSeq(values))
                .ToFin(Fail: context.Op.InvalidResult()))
            from handles in DraftCustody.Crossed(products: products, op: context.Op)
            select (HatchAnswer)new HatchAnswer.Pieces(handles));

    private static Fin<(Guid Id, Hatch Geometry)> Single(RhinoDoc document, TableTarget target, Op key) =>
        from row in target.Only<RhinoObject>(document: document, key: key)
        from hatch in key.Need(row.Native.Geometry as Hatch)
        select (row.Id, hatch);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
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
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

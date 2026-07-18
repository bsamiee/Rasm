# [RASM_RHINO_ANNOTATION_HATCH]

`PatternDef` round-trips complete detached pattern definitions, `HatchSpec` constructs placed fills through one boundary family, and `Hatches.Commit` folds pattern and object mutation through the shared drafting spine. Pattern batches compensate failed additions, placed state preserves every mutable hatch parameter, and all native geometry crosses the document boundary through explicit custody.

## [01]-[INDEX]

- [02]-[PATTERN]: generated pattern owners, line generators, tags, and native projection.
- [03]-[PLACEMENT]: invariant-carrying placement and the closed hatch construction family.
- [04]-[MUTATION]: atomic pattern lifecycle, placement, gradient, and scale operations.
- [05]-[PROJECTION]: complete pattern and placed state, previews, loops, display geometry, regions, and pieces.

## [02]-[PATTERN]

- Owner: `FillKind`, `PatternDistance`, `LineDef`, and `PatternDef` admit the complete detached pattern definition once.
- Law: `PatternDef` couples fill kind with line generators and preserves the pattern user-string bag in every read-modify-write cycle.
- Boundary: `PatternDef.Mint` verifies `SetHatchLines` against the admitted generator count and treats every refused user-string write as failure.
- Growth: a pattern attribute lands in `PatternDef`, its validation gate, `Mint`, and `Read`; no mutation case carries a partial parallel definition.

```csharp signature
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
    public double Angle { get; }
    public Point2d Base { get; }
    public Vector2d Offset { get; }
    public Seq<SegmentRow> Dashes { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref double angle, ref Point2d @base, ref Vector2d offset, ref Seq<SegmentRow> dashes) =>
        validationError = double.IsFinite(angle) && @base.IsValid && offset.IsValid
            && dashes.ForAll(static dash => dash is not null)
            ? null
            : new ValidationError(message: "Hatch line definition is invalid.");

    public static Fin<LineDef> Of(double angle, Point2d @base, Vector2d offset, Seq<SegmentRow> dashes, Op? key = null) =>
        Validate(angle, @base, offset, dashes, out LineDef? admitted) is null && admitted is not null
            ? Fin.Succ(value: admitted)
            : Fin.Fail<LineDef>(error: key.OrDefault().InvalidInput());

    internal Fin<HatchLine> Mint(Op key) => key.Catch(() => {
        HatchLine line = new() { Angle = Angle, BasePoint = Base, Offset = Offset };
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
        Validate(name, description, fill, units, distances, lines, tags, out PatternDef? admitted) is null && admitted is not null
            ? Fin.Succ(value: admitted)
            : Fin.Fail<PatternDef>(error: key.OrDefault().InvalidInput());

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
        from __ in TagBag.Apply(Tags, pattern.SetUserString, pattern.DeleteAllUserStrings, key)
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
                angle: line.Angle, @base: line.BasePoint, offset: line.Offset, dashes: dashes, key: key)
            select admitted).As()
        from units in ModelUnit.Of(value: pattern.PatternUnitSystem, key: key)
        from definition in Of(
            name: ResourceName.Create(pattern.Name),
            fill: fill,
            units: units,
            distances: distances,
            lines: lines,
            description: Optional(pattern.Description).Filter(static text => !string.IsNullOrWhiteSpace(text)),
            tags: TagBag.Read(pattern.GetUserStrings()),
            key: key)
        select definition);
}
```

## [03]-[PLACEMENT]

- Owner: `FillPlacement` carries the resolved pattern address, rotation, and positive scale through one generated aggregate gate.
- Owner: `HatchSpec` closes explicit loops, host-resolved nested loops, and planar-face construction under one plural product.
- Boundary: `HatchSpec.Lens` remains the frozen pattern lookup seam and refuses id lookup because `HatchPatternTable` exposes only name and index resolution.
- Law: curve construction accumulates `Requirement.AreaMass` failures before host dispatch and returns `Seq<Hatch>` because nested loops may partition into multiple fills.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class FillPlacement {
    public ResourceRef Pattern { get; }
    public double RotationRadians { get; }
    public double Scale { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref ResourceRef pattern, ref double rotationRadians, ref double scale) =>
        validationError = pattern is not null && double.IsFinite(rotationRadians) && double.IsFinite(scale) && scale > 0.0
            ? null
            : new ValidationError(message: "Hatch placement is invalid.");

    public static Fin<FillPlacement> Of(ResourceRef pattern, double rotationRadians = 0.0, double scale = 1.0, Op? key = null) =>
        Validate(pattern, rotationRadians, scale, out FillPlacement? admitted) is null && admitted is not null
            ? Fin.Succ(value: admitted)
            : Fin.Fail<FillPlacement>(error: key.OrDefault().InvalidInput());
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
        from _ in Admit(document, op)
        from pattern in placement.Pattern.Resolve(document: document, lens: Lens, key: op)
        from hatches in Switch(
            (Index: pattern.Index, Placement: placement, Op: op),
            bounded: static (context, spec) => context.Op.Catch(() => Optional(Hatch.Create(
                    hatchPlane: spec.Plane,
                    outerLoop: spec.Outer,
                    innerLoops: spec.Holes.AsIterable(),
                    hatchPatternIndex: context.Index,
                    rotationRadians: context.Placement.RotationRadians,
                    scale: context.Placement.Scale))
                .Map(static hatch => Seq(hatch))
                .ToFin(Fail: context.Op.InvalidResult())),
            resolved: static (context, spec) => context.Op.Catch(() => Optional(spec.Tolerance.Match(
                    Some: tolerance => Hatch.Create(
                        curves: spec.Curves.AsIterable(),
                        hatchPatternIndex: context.Index,
                        rotationRadians: context.Placement.RotationRadians,
                        scale: context.Placement.Scale,
                        tolerance: tolerance),
                    None: () => Hatch.Create(
                        curves: spec.Curves.AsIterable(),
                        hatchPatternIndex: context.Index,
                        rotationRadians: context.Placement.RotationRadians,
                        scale: context.Placement.Scale)))
                .Map(static values => toSeq(values))
                .Filter(static values => !values.IsEmpty)
                .ToFin(Fail: context.Op.InvalidResult())),
            fromFace: static (context, spec) => context.Op.Catch(() => Optional(Hatch.CreateFromBrep(
                    brep: spec.Source,
                    brepFaceIndex: spec.FaceIndex,
                    hatchPatternIndex: context.Index,
                    rotationRadians: context.Placement.RotationRadians,
                    scale: context.Placement.Scale,
                    basePoint: spec.BasePoint))
                .Map(static hatch => Seq(hatch))
                .ToFin(Fail: context.Op.InvalidResult())))
        select hatches;

    private Fin<Unit> Admit(RhinoDoc document, Op key) => Switch(
        (Document: document, Op: key),
        bounded: static (context, spec) =>
            from _ in context.Op.AcceptInput(value: spec.Plane)
            from __ in Area(document: context.Document, curves: Seq(spec.Outer).Concat(spec.Holes))
            select unit,
        resolved: static (context, spec) =>
            from _ in guard(!spec.Curves.IsEmpty, context.Op.InvalidInput()).ToFin()
            from __ in Area(document: context.Document, curves: spec.Curves)
            from ___ in spec.Tolerance.Traverse(value => context.Op.Positive(value: value)).As()
            select unit,
        fromFace: static (context, spec) =>
            from _ in context.Op.AcceptInput(value: spec.Source)
            from __ in guard(spec.FaceIndex >= 0 && spec.FaceIndex < spec.Source.Faces.Count, context.Op.InvalidInput()).ToFin()
            from ___ in context.Op.AcceptInput(value: spec.BasePoint)
            select unit);

    private static Fin<Unit> Area(RhinoDoc document, Seq<Curve> curves) =>
        from context in Rasm.Domain.Context.Of(doc: document).ToFin()
        from admitted in curves.Traverse(curve => Requirement.AreaMass.Apply(
            context: context, value: curve)).As().ToFin()
        select unit;
}
```

## [04]-[MUTATION]

- Owner: `HatchOp` is the complete pattern-table and placed-hatch mutation program consumed by `Hatches.Commit`.
- Law: pattern import, multi-hatch placement, and batch replacement fold through the shared `DocumentCommit.Compensated` landed-state algebra, with minted and cloned custody settled through its source-release policy.
- Boundary: default and imported host patterns cross `PatternDef.Read` and `Mint`, so one canonical detached shape reaches every table addition.
- Law: delete resolves and deduplicates every target before one batch table call; one retained row refuses the whole request.
- Boundary: placement rollback deletes landed objects while the release policy settles every minted native on both outcomes; compensation faults accumulate without masking the initiating refusal.
- Law: pattern amendment always mints a complete `PatternDef` and lands through `Modify` — a fresh admitted aggregate replaces the row, the named discriminant that keeps this rail off the `TableGrip` duplicate-then-revise law; `Retag` first reconstructs that definition, so tag-only edits cannot erase generators or config.
- Boundary: placed-hatch rework retains original and revised clones through compensation; the release policy settles both clones on every outcome, and a custody refusal after commit restores the originals.
- Entry: `Hatches.Commit` preserves the frozen wire and accepts `DraftPlan<HatchOp>` with shared redraw and undo policy.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HatchOp {
    private HatchOp() { }
    public sealed record Author(PatternDef Def) : HatchOp;
    public sealed record AuthorDefault(ResourceName Name) : HatchOp;
    public sealed record Amend(ResourceRef Target, PatternDef Def, WriteMode Mode) : HatchOp;
    public sealed record Retag(ResourceRef Target, HashMap<string, string> Tags, WriteMode Mode) : HatchOp;
    public sealed record Rename(ResourceRef Target, ResourceName Name) : HatchOp;
    public sealed record Delete(Seq<ResourceRef> Targets, WriteMode Mode) : HatchOp;
    public sealed record Import(DraftPath Path, WriteMode Mode) : HatchOp;
    public sealed record Export(DraftPath Path, Seq<ResourceRef> Targets) : HatchOp;
    public sealed record Place(HatchSpec Spec, FillPlacement Placement, Option<ObjectAttributes> Attributes = default) : HatchOp;
    public sealed record Regrade(TableTarget Target, ColorGradient Fill) : HatchOp;
    public sealed record Rescale(TableTarget Target, Transform Motion) : HatchOp;

    internal Fin<DraftReceipt> Apply(RhinoDoc document, Op op) => Switch(
        (Document: document, Op: op),
        author: static (context, edit) =>
            from _ in guard(context.Document.HatchPatterns.FindName(name: edit.Def.Name.Value) is null,
                context.Op.InvalidInput()).ToFin()
            from pattern in edit.Def.Mint(key: context.Op)
            from index in Added(document: context.Document, pattern: pattern, op: context.Op)
            from receipt in DraftReceipt.Component(slot: DraftSlot.Authored, componentKind: DraftComponentKind.Hatch, index: index)
            select receipt,
        authorDefault: static (context, edit) =>
            from source in context.Op.Catch(() => toSeq(HatchPattern.GetDefaultHatchPatterns())
                .Find(candidate => string.Equals(candidate.Name, edit.Name.Value, StringComparison.OrdinalIgnoreCase))
                .ToFin(Fail: context.Op.MissingContext()))
            from pattern in PatternDef.Canonical(pattern: source, key: context.Op)
            from _ in guard(context.Document.HatchPatterns.FindName(name: pattern.Name) is null,
                context.Op.InvalidInput()).ToFin()
            from index in Added(document: context.Document, pattern: pattern, op: context.Op)
            from receipt in DraftReceipt.Component(slot: DraftSlot.Authored, componentKind: DraftComponentKind.Hatch, index: index)
            select receipt,
        amend: static (context, edit) => ReplacePattern(
            document: context.Document, target: edit.Target, definition: edit.Def, mode: edit.Mode,
            slot: DraftSlot.Amended, op: context.Op),
        retag: static (context, edit) =>
            from pattern in edit.Target.Resolve(document: context.Document, lens: HatchSpec.Lens, key: context.Op)
            from current in PatternDef.Read(pattern: pattern, key: context.Op)
            from changed in PatternDef.Of(
                name: current.Name, fill: current.Fill, units: current.Units, distances: current.Distances,
                lines: current.Lines, description: current.Description, tags: edit.Tags, key: context.Op)
            from receipt in ReplacePattern(
                document: context.Document, target: edit.Target, definition: changed, mode: edit.Mode,
                slot: DraftSlot.Amended, op: context.Op)
            select receipt,
        rename: static (context, edit) =>
            from pattern in edit.Target.Resolve(document: context.Document, lens: HatchSpec.Lens, key: context.Op)
            from _ in context.Op.Confirm(success: context.Document.HatchPatterns.Rename(
                hatchPatternIndex: pattern.Index, hatchPatternName: edit.Name.Value))
            from receipt in DraftReceipt.Component(
                slot: DraftSlot.Renamed, componentKind: DraftComponentKind.Hatch, index: ResourceIndex.Create(pattern.Index))
            select receipt,
        delete: static (context, edit) =>
            from _ in guard(!edit.Targets.IsEmpty, context.Op.InvalidInput()).ToFin()
            from patterns in edit.Targets.TraverseM(target => target.Resolve(
                document: context.Document, lens: HatchSpec.Lens, key: context.Op)).As()
            let indices = patterns.Map(static pattern => pattern.Index)
            from __ in guard(indices.Distinct().Count == indices.Count, context.Op.InvalidInput()).ToFin()
            from deleted in context.Op.Catch(() => Fin.Succ(value: context.Document.HatchPatterns.Delete(
                hatchPatternIndices: indices.AsIterable(), quiet: edit.Mode.QuietWrite)))
            from ___ in guard(deleted == indices.Count, context.Op.InvalidResult()).ToFin()
            from receipts in indices.TraverseM(index => DraftReceipt.Component(
                slot: DraftSlot.Deleted, componentKind: DraftComponentKind.Hatch, index: ResourceIndex.Create(index))).As()
            select receipts.Fold(DraftReceipt.Empty, static (receipt, next) => receipt.Contribute(next)),
        import: static (context, edit) =>
            from read in context.Op.Catch(() => Optional(HatchPattern.ReadFromFile(
                    filename: edit.Path.Value, quiet: edit.Mode.QuietWrite))
                .Map(static patterns => toSeq(patterns))
                .Filter(static patterns => !patterns.IsEmpty)
                .ToFin(Fail: context.Op.InvalidResult()))
            from patterns in read.TraverseM(pattern => PatternDef.Canonical(pattern: pattern, key: context.Op)).As()
            from _ in guard(
                patterns.AsIterable().Select(static pattern => pattern.Name)
                    .Distinct(StringComparer.OrdinalIgnoreCase).Count() == patterns.Count
                && !patterns.Exists(pattern => context.Document.HatchPatterns.FindName(name: pattern.Name) is not null),
                context.Op.InvalidInput()).ToFin()
            from indices in DocumentCommit.Compensated(
                source: patterns,
                land: pattern => Added(document: context.Document, pattern: pattern, op: context.Op),
                rollback: landed => context.Op.Confirm(success: context.Document.HatchPatterns.Delete(
                    hatchPatternIndices: landed.Map(static index => index.Value).AsIterable(), quiet: true) == landed.Count))
            from pathReceipt in DraftReceipt.Path(slot: DraftSlot.Imported, path: edit.Path)
            from components in indices.TraverseM(index => DraftReceipt.Component(
                slot: DraftSlot.Imported, componentKind: DraftComponentKind.Hatch, index: index)).As()
            select components.Fold(pathReceipt, static (state, receipt) => state.Contribute(receipt)),
        export: static (context, edit) =>
            from patterns in edit.Targets.TraverseM(target => target.Resolve(
                document: context.Document, lens: HatchSpec.Lens, key: context.Op)).As()
            from _ in guard(!patterns.IsEmpty, context.Op.InvalidInput()).ToFin()
            from __ in context.Op.Confirm(success: HatchPattern.WriteToFile(
                filename: edit.Path.Value, hatchPatterns: patterns.AsIterable()))
            from receipt in DraftReceipt.Path(slot: DraftSlot.Exported, path: edit.Path)
            select receipt,
        place: static (context, edit) =>
            from hatches in edit.Spec.Mint(document: context.Document, placement: edit.Placement, op: context.Op)
            from ids in DocumentCommit.Compensated(
                source: hatches,
                land: hatch => context.Op.Catch(() => ResourceId.Admit(context.Document.Objects.Add(
                    geometry: hatch,
                    attributes: edit.Attributes.IfNoneUnsafe((ObjectAttributes?)null),
                    history: null,
                    reference: false), context.Op)),
                rollback: landed => context.Op.Confirm(success: context.Document.Objects.Delete(
                    objectIds: landed.Map(static id => id.Value).AsIterable(), quiet: true) == landed.Count),
                release: minted => Release(values: minted, op: context.Op))
            from receipt in DraftReceipt.Objects(slot: DraftSlot.Placed, ids: ids)
            select receipt,
        regrade: static (context, edit) => Reworked(
            document: context.Document, target: edit.Target, op: context.Op, slot: DraftSlot.Restyled,
            change: (hatch, key) => key.AcceptInput(value: edit.Fill)
                .Bind(fill => key.Catch(() => hatch.SetGradientFill(fill: fill)))),
        rescale: static (context, edit) => Reworked(
            document: context.Document, target: edit.Target, op: context.Op, slot: DraftSlot.Scaled,
            change: (hatch, key) => key.AcceptInput(value: edit.Motion)
                .Bind(_ => key.Catch(() => hatch.ScalePattern(xform: edit.Motion)))));

    private static Fin<DraftReceipt> ReplacePattern(
        RhinoDoc document, ResourceRef target, PatternDef definition, WriteMode mode, DraftSlot slot, Op op) =>
        from active in target.Resolve(document: document, lens: HatchSpec.Lens, key: op)
        from minted in definition.Mint(key: op)
        from _ in op.Confirm(success: document.HatchPatterns.Modify(
            hatchPattern: minted, hatchPatternIndex: active.Index, quiet: mode.QuietWrite))
        from receipt in DraftReceipt.Component(
            slot: slot, componentKind: DraftComponentKind.Hatch, index: ResourceIndex.Create(active.Index))
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
            rollback: landed => Release(values: landed.Bind(static row => row.Custody), op: op))
        from amended in DocumentCommit.Compensated(
            source: revisions,
            land: revision => op.Confirm(success: document.Objects.Replace(
                    objectId: revision.Id, geometry: revision.Revised, ignoreModes: false))
                .Map(_ => (revision.Id, revision.Original)),
            rollback: landed => landed.Traverse(row => op.Confirm(success: document.Objects.Replace(
                    objectId: row.Id, geometry: row.Original, ignoreModes: false))
                .ToValidation()).As().ToFin().Map(static _ => unit),
            release: rows => Release(values: rows.Bind(static row => row.Custody), op: op))
        from receipt in DraftReceipt.Objects(slot: slot, ids: amended.Map(static row => ResourceId.Create(row.Id)))
        select receipt;

    private static Fin<HatchRevision> Prepare(
        RhinoDoc document, Guid id, Func<Hatch, Op, Fin<Unit>> change, Op op) =>
            from native in Optional(document.Objects.FindId(id)).ToFin(Fail: op.MissingContext())
            from source in Optional(native.Geometry as Hatch).ToFin(Fail: op.InvalidInput())
            from original in op.Catch(() => Optional(source.Duplicate() as Hatch).ToFin(Fail: op.InvalidResult()))
            from revised in op.Catch(() => Optional(source.Duplicate() as Hatch).ToFin(Fail: op.InvalidResult()))
                .BindFail(primary => Failed<Hatch, Hatch>(primary: primary, values: Seq(original), op: op))
            from _ in change(revised, op)
                .BindFail(primary => Failed<Unit, Hatch>(primary: primary, values: Seq(original, revised), op: op))
            select new HatchRevision(Id: id, Original: original, Revised: revised);

    internal static Fin<Unit> Release<T>(Seq<T> values, Op op) where T : class, IDisposable =>
        values.Traverse(value => op.Catch(() => Fin.Succ(value: Op.Side(value.Dispose))).ToValidation())
            .As().ToFin().Map(static _ => unit);

    internal static Fin<TValue> Failed<TValue, TResource>(Error primary, Seq<TResource> values, Op op)
        where TResource : class, IDisposable =>
        Release(values: values, op: op).Match(
            Succ: _ => Fin.Fail<TValue>(error: primary),
            Fail: cleanup => Fin.Fail<TValue>(error: primary + cleanup));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Hatches {
    public static Fin<DraftReceipt> Commit(DocumentSession session, DraftPlan<HatchOp> plan) =>
        DraftSpine.Commit(session: session, plan: plan,
            apply: static (document, operation, key) => operation.Apply(document: document, op: key), op: Op.Of());

    public static Fin<HatchAnswer> Ask(DocumentSession session, HatchAsk request) {
        Op op = Op.Of();
        return session.Demand(use: document => request.Answer(document: document, op: op), key: op, needs: [SessionNeed.Read]);
    }
}
```

## [05]-[PROJECTION]

- Owner: `PatternSnapshot` and `HatchState` preserve the complete detached definition and every mutable placed-hatch parameter.
- Policy: `LoopKind` and `LoopFrame` replace independent boolean switches with named perimeter and coordinate-frame values.
- Boundary: loops, display bounds, solid regions, and exploded pieces cross through `GeometryCrossing`; answer cases expose only `GeometryHandle` custody.
- Boundary: one harvest fold releases accepted handles on crossing refusal and raw products on every exit.
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
    double PatternRotation,
    double PatternScale,
    Option<ColorGradient> Gradient) : IDetachedDocumentResult;

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
    public sealed record Preview(ResourceRef Target, int Width, int Height, double Angle) : HatchAsk;
    public sealed record State(TableTarget Target) : HatchAsk;
    public sealed record Display(TableTarget Target, double PatternScale = 1.0) : HatchAsk;
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
            from definitions in toSeq(HatchPattern.GetDefaultHatchPatterns())
                .TraverseM(pattern => PatternDef.Read(pattern: pattern, key: context.Op)).As()
            select (HatchAnswer)new HatchAnswer.Definitions(
                definitions, ResourceIndex.Create(context.Document.HatchPatterns.CurrentHatchPatternIndex)),
        mintName: static (context, _) =>
            from name in context.Op.AcceptText(value: context.Document.HatchPatterns.GetUnusedHatchPatternName())
            select (HatchAnswer)new HatchAnswer.Minted(ResourceName.Create(name)),
        preview: static (context, ask) =>
            from pattern in ask.Target.Resolve(document: context.Document, lens: HatchSpec.Lens, key: context.Op)
            from _ in guard(ask.Width > 0 && ask.Height > 0, context.Op.InvalidInput()).ToFin()
            from angle in context.Op.AcceptInput(value: ask.Angle)
            from lines in context.Op.Catch(() => Optional(pattern.CreatePreviewGeometry(
                    width: ask.Width, height: ask.Height, angle: angle))
                .Map(static values => toSeq(values))
                .ToFin(Fail: context.Op.InvalidResult()))
            select (HatchAnswer)new HatchAnswer.Previewed(lines),
        state: static (context, ask) =>
            from hatch in Single(document: context.Document, target: ask.Target, key: context.Op)
            from gradient in context.Op.Catch(() => Fin.Succ(value: Optional(hatch.Geometry.GetGradientFill())))
            select (HatchAnswer)new HatchAnswer.Placed(new HatchState(
                ResourceId.Create(hatch.Id),
                ResourceIndex.Create(hatch.Geometry.PatternIndex),
                hatch.Geometry.Plane,
                hatch.Geometry.BasePoint,
                hatch.Geometry.PatternRotation,
                hatch.Geometry.PatternScale,
                gradient)),
        display: static (context, ask) =>
            from hatch in Single(document: context.Document, target: ask.Target, key: context.Op)
            from scale in context.Op.Positive(value: ask.PatternScale)
            from pattern in Optional(context.Document.HatchPatterns.FindIndex(index: hatch.Geometry.PatternIndex))
                .ToFin(Fail: context.Op.MissingContext())
            from raw in context.Op.Catch(() => {
                hatch.Geometry.CreateDisplayGeometry(
                    pattern: pattern,
                    patternScale: scale,
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
            from handles in Crossed(products: products, key: context.Op)
            select (HatchAnswer)new HatchAnswer.Drawable(new HatchDisplay(
                Bounds: handles.Take(raw.Bounds.Count),
                Lines: raw.Lines,
                Solid: raw.Solid.Map(_ => handles[raw.Bounds.Count]))),
        loops: static (context, ask) =>
            from hatch in Single(document: context.Document, target: ask.Target, key: context.Op)
            from curves in context.Op.Catch(() => Fin.Succ(value: toSeq(ask.Frame.Key
                ? hatch.Geometry.Get3dCurves(outer: ask.Kind.Key)
                : hatch.Geometry.Get2dCurves(outer: ask.Kind.Key))))
            from handles in Crossed(products: curves, key: context.Op)
            select (HatchAnswer)new HatchAnswer.Boundary(handles),
        solid: static (context, ask) =>
            from hatch in Single(document: context.Document, target: ask.Target, key: context.Op)
            from brep in context.Op.Catch(() => Optional(hatch.Geometry.ToBrep()).ToFin(
                Fail: context.Op.InvalidResult()))
            from handles in Crossed(products: Seq(brep), key: context.Op)
            select (HatchAnswer)new HatchAnswer.Solidified(handles[0]),
        pieces: static (context, ask) =>
            from hatch in Single(document: context.Document, target: ask.Target, key: context.Op)
            from products in context.Op.Catch(() => Optional(hatch.Geometry.Explode())
                .Map(static values => toSeq(values))
                .ToFin(Fail: context.Op.InvalidResult()))
            from handles in Crossed(products: products, key: context.Op)
            select (HatchAnswer)new HatchAnswer.Pieces(handles));

    private static Fin<Seq<GeometryHandle>> Crossed<TGeometry>(Seq<TGeometry> products, Op key)
        where TGeometry : GeometryBase =>
        DocumentCommit.Compensated(
            source: products,
            land: product => GeometryCrossing.Cross(source: product, mode: CrossingMode.Detach, key: key),
            rollback: landed => HatchOp.Release(values: landed, op: key),
            release: sources => HatchOp.Release(values: sources, op: key));

    private static Fin<(Guid Id, Hatch Geometry)> Single(RhinoDoc document, TableTarget target, Op key) =>
        from row in target.Only<RhinoObject>(document: document, key: key)
        from hatch in Optional(row.Native.Geometry as Hatch).ToFin(Fail: key.InvalidInput())
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

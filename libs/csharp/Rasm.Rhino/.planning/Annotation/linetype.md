# [RASM_RHINO_ANNOTATION_LINETYPE]

`StrokeDef` admits the complete authorable linetype aggregate, `SegmentRow` supplies the shared dash/gap atom, and `Linetypes.Commit` folds resource mutation through the shared drafting spine. Authoring and import compensate provisional rows, host display enums terminate at generated policy owners, tags round-trip as one bag, and shape projection states the host API's aggregate evidence boundary without promising unavailable glyph reconstruction.

## [01]-[INDEX]

- [02]-[DEFINITION]: generated segment, shape, taper, display-policy, and stroke owners.
- [03]-[MUTATION]: compensated authoring, lifecycle, defaults, import, and tag operations.
- [04]-[PROJECTION]: detached stroke evidence, table state, object resolution, and pattern text.

## [02]-[DEFINITION]

- Owner: `SegmentRow` carries positive length and dash/gap role; only `Signed` projects the host's signed run.
- Owner: `ShapeRow`, `TaperRow`, and `StrokeDef` close embedded glyphs, taper, display config, distance policy, pattern locking, and tags under one aggregate.
- Boundary: `LinetypeCap` and `LinetypeJoin` map host enums at the edge, while `ShapeRow.TextShape` composes `TextSpec.Mint` and `StyleOp.Lens` inside the document grant.
- Law: `StrokeDef.Apply` consumes an already admitted aggregate and mutates only a detached `DuplicateLinetype`; live rows change through `LinetypeTable.Modify`.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class SegmentRow {
    public double Length { get; }
    public bool Solid { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double length, ref bool solid) =>
        validationError = double.IsFinite(length) && length > 0.0
            ? null
            : new ValidationError(message: "Linetype segment is invalid.");

    public static Fin<SegmentRow> Of(double length, bool solid, Op? key = null) =>
        Validate(length, solid, out SegmentRow? admitted) is null && admitted is not null
            ? Fin.Succ(value: admitted)
            : Fin.Fail<SegmentRow>(error: key.OrDefault().InvalidInput());

    internal double Signed => Solid ? Length : -Length;
}

[SmartEnum<int>]
public sealed partial class LinetypeCap {
    public static readonly LinetypeCap Butt = new(key: (int)LineCapStyle.Flat);
    public static readonly LinetypeCap Round = new(key: (int)LineCapStyle.Round);
    public static readonly LinetypeCap Square = new(key: (int)LineCapStyle.Square);

    internal LineCapStyle Host => (LineCapStyle)Key;
}

[SmartEnum<int>]
public sealed partial class LinetypeJoin {
    public static readonly LinetypeJoin Round = new(key: (int)LineJoinStyle.Round);
    public static readonly LinetypeJoin Miter = new(key: (int)LineJoinStyle.Miter);
    public static readonly LinetypeJoin Bevel = new(key: (int)LineJoinStyle.Bevel);

    internal LineJoinStyle Host => (LineJoinStyle)Key;
}

[SmartEnum<bool>]
public sealed partial class PatternMeasure {
    public static readonly PatternMeasure Millimeters = new(key: true);
    public static readonly PatternMeasure Inches = new(key: false);
}

[SmartEnum<bool>]
public sealed partial class PatternLock {
    public static readonly PatternLock Editable = new(key: false);
    public static readonly PatternLock Locked = new(key: true);
}

[SmartEnum<bool>]
public sealed partial class DeletedRows {
    public static readonly DeletedRows Include = new(key: false);
    public static readonly DeletedRows Ignore = new(key: true);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ShapeRow {
    private ShapeRow() { }
    public sealed record CurveShape(Curve Glyph, double Offset) : ShapeRow;
    public sealed record TextShape(TextSpec Spec, Plane Frame, ResourceRef Style, double Offset) : ShapeRow;

    internal Fin<Unit> Apply(RhinoDoc document, Linetype linetype, Op key) => Switch(
        (Document: document, Linetype: linetype, Op: key),
        curveShape: static (context, row) =>
            from glyph in context.Op.AcceptInput(value: row.Glyph)
            from offset in context.Op.AcceptInput(value: row.Offset)
            from _ in context.Op.Confirm(success: context.Linetype.AddShape(
                shapeCurve: glyph, offset: offset))
            select unit,
        textShape: static (context, row) =>
            from spec in Optional(row.Spec).ToFin(Fail: context.Op.InvalidInput())
            from frame in context.Op.AcceptInput(value: row.Frame)
            from address in Optional(row.Style).ToFin(Fail: context.Op.InvalidInput())
            from style in address.Resolve(document: context.Document, lens: StyleOp.Lens, key: context.Op)
            from glyph in spec.Mint(plane: frame, style: style, key: context.Op)
            from offset in context.Op.AcceptInput(value: row.Offset)
            from _ in context.Op.Confirm(success: context.Linetype.AddShape(text: glyph, offset: offset))
            select unit);
}

[ComplexValueObject]
public sealed partial class TaperRow {
    public double StartWidth { get; }
    public Option<Point2d> Mid { get; }
    public double EndWidth { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref double startWidth, ref Option<Point2d> mid, ref double endWidth) =>
        validationError = double.IsFinite(startWidth) && startWidth > 0.0
            && mid.ForAll(static point => point.IsValid)
            && double.IsFinite(endWidth) && endWidth > 0.0
            ? null
            : new ValidationError(message: "Linetype taper is invalid.");

    public static Fin<TaperRow> Of(double startWidth, double endWidth, Option<Point2d> mid = default, Op? key = null) =>
        Validate(startWidth, mid, endWidth, out TaperRow? admitted) is null && admitted is not null
            ? Fin.Succ(value: admitted)
            : Fin.Fail<TaperRow>(error: key.OrDefault().InvalidInput());

    internal Fin<Unit> Apply(Linetype linetype, Op key) => Mid.Match(
        Some: point => key.Catch(() => linetype.SetTaper(startWidth: StartWidth, taperPoint: point, endWidth: EndWidth)),
        None: () => key.Catch(() => linetype.SetTaper(startWidth: StartWidth, endWidth: EndWidth)));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SegmentEdit {
    private SegmentEdit() { }
    public sealed record Append(SegmentRow Segment) : SegmentEdit;
    public sealed record Replace(int Index, SegmentRow Segment) : SegmentEdit;
    public sealed record Remove(int Index) : SegmentEdit;

    internal Fin<Unit> Apply(Linetype linetype, Op key) => Switch(
        (Linetype: linetype, Op: key),
        append: static (context, edit) =>
            from index in context.Op.Catch(() => Fin.Succ(value: context.Linetype.AppendSegment(
                length: edit.Segment.Length, isSolid: edit.Segment.Solid)))
            from _ in guard(index >= 0, context.Op.InvalidResult()).ToFin()
            select unit,
        replace: static (context, edit) =>
            from _ in guard(edit.Index >= 0 && edit.Index < context.Linetype.SegmentCount, context.Op.InvalidInput()).ToFin()
            from __ in context.Op.Confirm(success: context.Linetype.SetSegment(
                index: edit.Index, length: edit.Segment.Length, isSolid: edit.Segment.Solid))
            select unit,
        remove: static (context, edit) =>
            from _ in guard(context.Linetype.SegmentCount > 1
                && edit.Index >= 0 && edit.Index < context.Linetype.SegmentCount, context.Op.InvalidInput()).ToFin()
            from __ in context.Op.Confirm(success: context.Linetype.RemoveSegment(index: edit.Index))
            select unit);
}

// --- [MODELS] -------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class StrokeDef {
    public ResourceName Name { get; }
    public Seq<SegmentRow> Segments { get; }
    public Seq<ShapeRow> Shapes { get; }
    public Option<TaperRow> Taper { get; }
    public LinetypeCap Cap { get; }
    public LinetypeJoin Join { get; }
    public double Width { get; }
    public ModelUnit WidthUnits { get; }
    public PatternDistance Distances { get; }
    public PatternLock Lock { get; }
    public HashMap<string, string> Tags { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref ResourceName name, ref Seq<SegmentRow> segments, ref Seq<ShapeRow> shapes,
        ref Option<TaperRow> taper, ref LinetypeCap cap, ref LinetypeJoin join, ref double width, ref ModelUnit widthUnits,
        ref PatternDistance distances, ref PatternLock @lock, ref HashMap<string, string> tags) {
        bool validTags = tags.ForAll(static pair =>
            !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value));
        validationError = name is not null && !segments.IsEmpty
            && segments.ForAll(static segment => segment is not null)
            && shapes.ForAll(static shape => shape is not null)
            && taper.ForAll(static row => row is not null)
            && cap is not null && join is not null && widthUnits is not null
            && distances is not null && @lock is not null
            && double.IsFinite(width) && width > 0.0 && validTags
            ? null
            : new ValidationError(message: "Linetype definition is invalid.");
    }

    public static Fin<StrokeDef> Of(
        ResourceName name, Seq<SegmentRow> segments, Seq<ShapeRow> shapes, Option<TaperRow> taper,
        LinetypeCap cap, LinetypeJoin join, double width, ModelUnit widthUnits, PatternDistance distances,
        PatternLock @lock, HashMap<string, string> tags = default, Op? key = null) =>
        Validate(name, segments, shapes, taper, cap, join, width, widthUnits, distances, @lock, tags, out StrokeDef? admitted) is null
            && admitted is not null
            ? Fin.Succ(value: admitted)
            : Fin.Fail<StrokeDef>(error: key.OrDefault().InvalidInput());

    internal Seq<double> SignedRun => Segments.Map(static row => row.Signed);

    internal Fin<Unit> Apply(RhinoDoc document, Linetype linetype, Op key) =>
        from unlocked in key.Catch(() => linetype.IsPatternLocked = false)
        from segments in key.Confirm(success: linetype.SetSegments(segments: SignedRun.AsIterable()))
        from shapesRemoved in key.Catch(linetype.RemoveAllShapes)
        from shapes in Shapes.TraverseM(shape => shape.Apply(document: document, linetype: linetype, key: key)).As()
        from taper in Taper.Match(
            Some: taper => taper.Apply(linetype: linetype, key: key),
            None: () => key.Catch(linetype.RemoveTaper))
        from configured in key.Catch(() => {
            linetype.Name = Name.Value;
            linetype.LineCapStyle = Cap.Host;
            linetype.LineJoinStyle = Join.Host;
            linetype.Width = Width;
            linetype.WidthUnits = WidthUnits.System;
            linetype.AlwaysModelDistances = Distances.Key;
            linetype.IsPatternLocked = Lock.Key;
        })
        from tags in TagBag.Apply(Tags, linetype.SetUserString, linetype.DeleteAllUserStrings, key)
        select unit;
}
```

## [03]-[MUTATION]

- Owner: `LinetypeOp` is the complete linetype-table mutation program consumed by `Linetypes.Commit`; `SegmentEdit` folds append, replace, and remove into one detached revision vocabulary; every duplicate-then-`Modify` revision rides `LinetypeOp.Grip`, whose duplicate row restores the name `DuplicateLinetype` drops.
- Law: aggregate authoring compensates its provisional row when duplicate shaping or `Modify` fails; a preflighted import folds through the shared `DocumentCommit.Compensated` algebra.
- Law: `Rename` and `Retag` preserve embedded shapes by revising a host duplicate, while `Amend` intentionally replaces the complete authorable aggregate.
- Law: `Undelete` alone resolves through the deleted-inclusive id, name, and index lens; every active operation retains the active-only lens.
- Entry: `Linetypes.Commit` preserves the frozen wire and accepts `DraftPlan<LinetypeOp>` with shared redraw and undo policy.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LinetypeOp {
    private LinetypeOp() { }
    public sealed record Author(StrokeDef Def) : LinetypeOp;
    public sealed record AuthorPattern(
        ResourceName Name,
        string Pattern,
        PatternMeasure Measure,
        HashMap<string, string> Tags = default) : LinetypeOp;
    public sealed record AuthorReference(Linetype Definition) : LinetypeOp;
    public sealed record Amend(ResourceRef Target, StrokeDef Def, WriteMode Mode) : LinetypeOp;
    public sealed record Resegment(ResourceRef Target, Seq<SegmentEdit> Edits, WriteMode Mode) : LinetypeOp;
    public sealed record Rename(ResourceRef Target, ResourceName Name, WriteMode Mode) : LinetypeOp;
    public sealed record Retag(ResourceRef Target, HashMap<string, string> Tags, WriteMode Mode) : LinetypeOp;
    public sealed record Revert(ResourceRef Target) : LinetypeOp;
    public sealed record Reset(ResourceRef Target, WriteMode Mode) : LinetypeOp;
    public sealed record Delete(ResourceRef Target, WriteMode Mode) : LinetypeOp;
    public sealed record Undelete(ResourceRef Target) : LinetypeOp;
    public sealed record LoadDefaults(DeletedRows Policy) : LinetypeOp;
    public sealed record SetCurrent(ResourceRef Target, WriteMode Mode) : LinetypeOp;
    public sealed record Import(DraftPath Path) : LinetypeOp;

    internal static readonly ResourceLens<Linetype> Lens = WithPolicy(DeletedRows.Ignore);

    private static readonly ResourceLens<Linetype> ReviveLens = WithPolicy(DeletedRows.Include);

    private static ResourceLens<Linetype> WithPolicy(DeletedRows policy) => new(
        ById: (document, id) => document.Linetypes.Find(
            id: id, ignoreDeletedLinetypes: policy.Key) is var index && index >= 0
            ? document.Linetypes.FindIndex(index: index)
            : null,
        ByName: (document, name) => document.Linetypes.Find(
            name: name, ignoreDeletedLinetypes: policy.Key) is var index && index >= 0
            ? document.Linetypes.FindIndex(index: index)
            : null,
        ByIndex: (document, index) => document.Linetypes.FindIndex(index: index) is { } row
            && (!policy.Key || !row.IsDeleted)
            ? row
            : null);

    internal static readonly TableGrip<Linetype> Grip = new(
        Lens, DraftComponentKind.Linetype,
        Index: static (_, linetype) => linetype.LinetypeIndex,
        Duplicate: static live => {
            Linetype detached = live.DuplicateLinetype();
            detached.Name = live.Name;
            return detached;
        },
        Modify: static (document, copy, index, quiet) => document.Linetypes.Modify(linetype: copy, index: index, quiet: quiet));

    internal Fin<DraftReceipt> Apply(RhinoDoc document, Op op) => Switch(
        (Document: document, Op: op),
        author: static (context, edit) =>
            from _ in guard(context.Document.Linetypes.FindName(name: edit.Def.Name.Value) is null,
                context.Op.InvalidInput()).ToFin()
            from index in context.Op.Catch(() => ResourceIndex.Admit(context.Document.Linetypes.Add(
                name: edit.Def.Name.Value, segmentLengths: edit.Def.SignedRun.AsIterable()), context.Op))
            from address in ResourceRef.Of(index: index.Value)
            from receipt in Grip.Revised(target: address, document: context.Document, slot: DraftSlot.Authored,
                mode: WriteMode.Quiet, op: context.Op,
                revise: (linetype, key) => edit.Def.Apply(document: context.Document, linetype: linetype, key: key)).Match(
                    Succ: static value => Fin.Succ(value: value),
                    Fail: error => context.Op.Confirm(success: context.Document.Linetypes.Delete(
                        index: index.Value, quiet: true)).Match(
                            Succ: _ => Fin.Fail<DraftReceipt>(error: error),
                            Fail: rollback => Fin.Fail<DraftReceipt>(error: error + rollback)))
            select receipt,
        authorPattern: static (context, edit) =>
            from pattern in context.Op.AcceptText(value: edit.Pattern)
            from _ in guard(context.Document.Linetypes.FindName(name: edit.Name.Value) is null,
                context.Op.InvalidInput()).ToFin()
            from built in context.Op.Catch(() => Optional(Linetype.CreateFromPatternString(
                    patternString: pattern, millimeters: edit.Measure.Key))
                .ToFin(Fail: context.Op.InvalidResult()))
            from __ in context.Op.Catch(() => built.Name = edit.Name.Value)
            from ___ in TagBag.Apply(edit.Tags, built.SetUserString, built.DeleteAllUserStrings, context.Op)
            from index in context.Op.Catch(() => ResourceIndex.Admit(context.Document.Linetypes.Add(linetype: built), context.Op))
            from receipt in DraftReceipt.Component(slot: DraftSlot.Authored, componentKind: DraftComponentKind.Linetype, index: index)
            select receipt,
        authorReference: static (context, edit) =>
            from definition in context.Op.AcceptInput(value: edit.Definition)
            from name in context.Op.AcceptText(value: definition.Name)
            from _ in guard(context.Document.Linetypes.FindName(name: name) is null, context.Op.InvalidInput()).ToFin()
            from index in context.Op.Catch(() =>
                ResourceIndex.Admit(context.Document.Linetypes.AddReferenceLinetype(linetype: definition), context.Op))
            from receipt in DraftReceipt.Component(slot: DraftSlot.Authored, componentKind: DraftComponentKind.Linetype, index: index)
            select receipt,
        amend: static (context, edit) =>
            Grip.Revised(target: edit.Target, document: context.Document, slot: DraftSlot.Amended, mode: edit.Mode, op: context.Op,
                revise: (copy, key) => edit.Def.Apply(document: context.Document, linetype: copy, key: key)),
        resegment: static (context, edit) =>
            from _ in guard(!edit.Edits.IsEmpty, context.Op.InvalidInput()).ToFin()
            from receipt in Grip.Revised(target: edit.Target, document: context.Document, slot: DraftSlot.Amended,
                mode: edit.Mode, op: context.Op,
                revise: (copy, key) =>
                    from locked in key.Catch(() => {
                        bool value = copy.IsPatternLocked;
                        copy.IsPatternLocked = false;
                        return Fin.Succ(value: value);
                    })
                    from applied in edit.Edits.TraverseM(row => row.Apply(linetype: copy, key: key)).As()
                    from restored in key.Catch(() => copy.IsPatternLocked = locked)
                    select unit)
            select receipt,
        rename: static (context, edit) =>
            Grip.Revised(target: edit.Target, document: context.Document, slot: DraftSlot.Renamed, mode: edit.Mode, op: context.Op,
                revise: (copy, key) => key.Catch(() => copy.Name = edit.Name.Value)),
        retag: static (context, edit) =>
            Grip.Revised(target: edit.Target, document: context.Document, slot: DraftSlot.Amended, mode: edit.Mode, op: context.Op,
                revise: (copy, key) => TagBag.Apply(edit.Tags, copy.SetUserString, copy.DeleteAllUserStrings, key)),
        revert: static (context, edit) =>
            from linetype in edit.Target.Resolve(document: context.Document, lens: Lens, key: context.Op)
            from _ in context.Op.Confirm(success: context.Document.Linetypes.UndoModify(index: linetype.LinetypeIndex))
            from receipt in DraftReceipt.Component(
                slot: DraftSlot.Amended, componentKind: DraftComponentKind.Linetype, index: ResourceIndex.Create(linetype.LinetypeIndex))
            select receipt,
        reset: static (context, edit) =>
            Grip.Revised(target: edit.Target, document: context.Document, slot: DraftSlot.Amended, mode: edit.Mode, op: context.Op,
                revise: static (copy, key) => key.Catch(copy.Default)),
        delete: static (context, edit) =>
            from linetype in edit.Target.Resolve(document: context.Document, lens: Lens, key: context.Op)
            from _ in context.Op.Confirm(success: context.Document.Linetypes.Delete(
                index: linetype.LinetypeIndex, quiet: edit.Mode.QuietWrite))
            from receipt in DraftReceipt.Component(
                slot: DraftSlot.Deleted, componentKind: DraftComponentKind.Linetype, index: ResourceIndex.Create(linetype.LinetypeIndex))
            select receipt,
        undelete: static (context, edit) =>
            from linetype in edit.Target.Resolve(document: context.Document, lens: ReviveLens, key: context.Op)
            from _ in context.Op.Confirm(success: context.Document.Linetypes.Undelete(index: linetype.LinetypeIndex))
            from receipt in DraftReceipt.Component(
                slot: DraftSlot.Revived, componentKind: DraftComponentKind.Linetype, index: ResourceIndex.Create(linetype.LinetypeIndex))
            select receipt,
        loadDefaults: static (context, edit) =>
            from count in context.Op.Catch(() => Fin.Succ(value: context.Document.Linetypes.LoadDefaultLinetypes(
                ignoreDeleted: edit.Policy.Key)))
            from _ in guard(count >= 0, context.Op.InvalidResult()).ToFin()
            from receipt in DraftReceipt.Tally(slot: DraftSlot.Loaded, count: DraftCount.Create(count))
            select receipt,
        setCurrent: static (context, edit) =>
            from linetype in edit.Target.Resolve(document: context.Document, lens: Lens, key: context.Op)
            from _ in context.Op.Confirm(success: context.Document.Linetypes.SetCurrentLinetypeIndex(
                linetypeIndex: linetype.LinetypeIndex, quiet: edit.Mode.QuietWrite))
            from receipt in DraftReceipt.Component(
                slot: DraftSlot.Current, componentKind: DraftComponentKind.Linetype, index: ResourceIndex.Create(linetype.LinetypeIndex))
            select receipt,
        import: static (context, edit) =>
            from read in context.Op.Catch(() => Optional(Linetype.ReadFromFile(path: edit.Path.Value))
                .Map(static values => toSeq(values))
                .Filter(static values => !values.IsEmpty)
                .ToFin(Fail: context.Op.InvalidResult()))
            from _ in guard(
                read.AsIterable().Select(static value => value.Name)
                    .Distinct(StringComparer.OrdinalIgnoreCase).Count() == read.Count
                && !read.Exists(value => context.Document.Linetypes.FindName(name: value.Name) is not null),
                context.Op.InvalidInput()).ToFin()
            from indices in DocumentCommit.Compensated(
                source: read,
                land: definition => context.Op.Catch(() =>
                    ResourceIndex.Admit(context.Document.Linetypes.Add(linetype: definition), context.Op)),
                rollback: landed => context.Op.Confirm(success: context.Document.Linetypes.Delete(
                    indices: landed.Map(static index => index.Value).AsIterable(), quiet: true)))
            from pathReceipt in DraftReceipt.Path(slot: DraftSlot.Imported, path: edit.Path)
            from components in indices.TraverseM(index => DraftReceipt.Component(
                slot: DraftSlot.Imported, componentKind: DraftComponentKind.Linetype, index: index)).As()
            select components.Fold(pathReceipt, static (state, receipt) => state.Contribute(receipt)));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Linetypes {
    public static Fin<DraftReceipt> Commit(DocumentSession session, DraftPlan<LinetypeOp> plan) =>
        DraftSpine.Commit(session: session, plan: plan,
            apply: static (document, operation, key) => operation.Apply(document: document, op: key), op: Op.Of());

    public static Fin<LinetypeAnswer> Ask(DocumentSession session, LinetypeAsk request) {
        Op op = Op.Of();
        return from admitted in op.AcceptInput(value: request)
               from answer in session.Demand(
                   use: document => admitted.Answer(document: document, op: op), key: op, needs: [SessionNeed.Read])
               select answer;
    }
}
```

## [04]-[PROJECTION]

- Owner: `StrokeSnapshot` preserves identity, segment run, pattern length, aggregate shape evidence, taper points, display policies, distance policy, lifecycle state, tags, and `.lin` pattern text.
- Boundary: `Linetype` exposes no embedded-shape getter, so projection records host aggregate shape evidence and never fabricates a reconstructable `ShapeRow` roster.
- Law: `PatternMeasure` names the unit regime consumed by both `CreateFromPatternString` and `PatternString`; a raw `bool` never escapes the edge.
- Law: unused-name minting carries no deleted-row policy because the live host overload ignores that discriminant; `DeletedRows` remains the `LoadDefaults` policy.
- Consumer: `ForObject` delegates layer and parent inheritance to `LinetypeIndexForObject` and returns the canonical `ResourceRef` address.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record ShapeEvidence(
    bool Present,
    double Spacing,
    double Gap,
    Vector2d LocalOffset,
    Option<BoundingBox> Bounds) : IDetachedDocumentResult;

public sealed record StrokeSnapshot(
    ResourceId Key,
    ResourceIndex Index,
    ResourceName Name,
    Seq<SegmentRow> Segments,
    double PatternLength,
    ShapeEvidence Shapes,
    Seq<Point2d> TaperPoints,
    LinetypeCap Cap,
    LinetypeJoin Join,
    double Width,
    ModelUnit WidthUnits,
    PatternDistance Distances,
    PatternLock Lock,
    bool InUse,
    bool IsModified,
    HashMap<string, string> Tags,
    string PatternText) : IDetachedDocumentResult;

public sealed record LinetypeTableState(
    DraftCount Active,
    ResourceIndex Current,
    ObjectLinetypeSource CurrentSource,
    double Scale,
    ResourceName Continuous,
    ResourceName ByLayer,
    ResourceName ByParent) : IDetachedDocumentResult;

// --- [OPERATIONS] ---------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LinetypeAsk {
    private LinetypeAsk() { }
    public sealed record State(ResourceRef Target, PatternMeasure Measure) : LinetypeAsk;
    public sealed record TableState : LinetypeAsk;
    public sealed record ForObject(TableTarget Target) : LinetypeAsk;
    public sealed record MintName : LinetypeAsk;

    internal Fin<LinetypeAnswer> Answer(RhinoDoc document, Op op) => Switch(
        (Document: document, Op: op),
        state: static (context, ask) => context.Op.Catch(() =>
            from linetype in ask.Target.Resolve(document: context.Document, lens: LinetypeOp.Lens, key: context.Op)
            from segments in toSeq(Enumerable.Range(start: 0, count: linetype.SegmentCount))
                .TraverseM(index => {
                    linetype.GetSegment(index: index, length: out double length, isSolid: out bool solid);
                    return SegmentRow.Of(length: double.Abs(length), solid: solid, key: context.Op);
                }).As()
            from cap in context.Op.AcceptValidated<LinetypeCap>(candidate: (int)linetype.LineCapStyle)
            from join in context.Op.AcceptValidated<LinetypeJoin>(candidate: (int)linetype.LineJoinStyle)
            from widthUnits in ModelUnit.Of(value: linetype.WidthUnits, key: context.Op)
            from distances in context.Op.AcceptValidated<PatternDistance>(candidate: linetype.AlwaysModelDistances)
            from lockState in context.Op.AcceptValidated<PatternLock>(candidate: linetype.IsPatternLocked)
            from text in context.Op.AcceptText(value: linetype.PatternString(millimeters: ask.Measure.Key))
            let tags = TagBag.Read(linetype.GetUserStrings())
            select (LinetypeAnswer)new LinetypeAnswer.State(new StrokeSnapshot(
                ResourceId.Create(linetype.Id),
                ResourceIndex.Create(linetype.LinetypeIndex),
                ResourceName.Create(linetype.Name),
                segments,
                linetype.PatternLength,
                new ShapeEvidence(
                    linetype.HasShapes,
                    linetype.ShapeSpacing,
                    linetype.ShapeGap,
                    linetype.ShapeLocalOffset,
                    linetype.HasShapes ? Some(linetype.ShapeBounds) : None),
                toSeq(linetype.GetTaperPoints() ?? []),
                cap,
                join,
                linetype.Width,
                widthUnits,
                distances,
                lockState,
                linetype.InUse,
                linetype.IsModified,
                tags,
                text))),
        tableState: static (context, _) => context.Op.Catch(() => Fin.Succ<LinetypeAnswer>(
            value: new LinetypeAnswer.Rows(
                new LinetypeTableState(
                    DraftCount.Create(context.Document.Linetypes.ActiveCount),
                    ResourceIndex.Create(context.Document.Linetypes.CurrentLinetypeIndex),
                    context.Document.Linetypes.CurrentLinetypeSource,
                    context.Document.Linetypes.LinetypeScale,
                    ResourceName.Create(context.Document.Linetypes.ContinuousLinetypeName),
                    ResourceName.Create(context.Document.Linetypes.ByLayerLinetypeName),
                    ResourceName.Create(context.Document.Linetypes.ByParentLinetypeName))))),
        forObject: static (context, ask) =>
            from row in ask.Target.Only<RhinoObject>(document: context.Document, key: context.Op)
            from index in context.Op.Catch(() => Fin.Succ(value: context.Document.Linetypes.LinetypeIndexForObject(
                rhinoObject: row.Native)))
            from address in ResourceRef.Of(index: index)
            select (LinetypeAnswer)new LinetypeAnswer.Resolved(address),
        mintName: static (context, _) =>
            from name in context.Op.Catch(() => context.Op.AcceptText(
                value: context.Document.Linetypes.GetUnusedLinetypeName()))
            select (LinetypeAnswer)new LinetypeAnswer.Minted(ResourceName.Create(name)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LinetypeAnswer : IDetachedDocumentResult {
    private LinetypeAnswer() { }
    public sealed record State(StrokeSnapshot Snapshot) : LinetypeAnswer;
    public sealed record Rows(LinetypeTableState Table) : LinetypeAnswer;
    public sealed record Resolved(ResourceRef Linetype) : LinetypeAnswer;
    public sealed record Minted(ResourceName Name) : LinetypeAnswer;
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

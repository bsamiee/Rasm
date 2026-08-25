# [RASM_RHINO_ANNOTATION_LINETYPE]

`StrokeDef` admits the complete authorable linetype aggregate, `SegmentRow` supplies the shared dash/gap atom, and `Linetypes.Commit` folds resource mutation through the shared drafting spine. Table lifecycle rides the namespace's shared `TableOp` over one `TableGrip` whose unlock bracket owns the pattern-lock toggle. `StrokeStandard` derives an ISO 128-2 stroke from the kernel line-type ladder, host display enums terminate at generated policy owners, tags round-trip as one bag, and shape projection states the host API's aggregate evidence boundary.

## [01]-[INDEX]

- [02]-[DEFINITION]: the dash role, generated segment, shape, taper, display-policy, and stroke owners beside the ISO stroke derivation.
- [03]-[MUTATION]: the shared table verbs over the linetype grip and the five verbs this table alone carries.
- [04]-[PROJECTION]: detached stroke evidence, table state, object resolution, and pattern text.

## [02]-[DEFINITION]

- Owner: `DashRole` is the dash-versus-gap vocabulary and owns the host's SIGNED run projection as its own column, so a run length and its role are one value and the sign is never a discriminant a reader re-derives; `SegmentRow` carries the positive length beside that role.
- Owner: `ShapeRow`, `TaperRow`, and `StrokeDef` close embedded glyphs, taper, display config, distance policy, pattern locking, and tags under one aggregate.
- Owner: `PatternText` admits the `.lin` pattern grammar the host compiles — an alignment token followed by a comma-separated signed run — so a malformed pattern refuses at the boundary rather than inside `CreateFromPatternString`, which answers null and names nothing.
- Owner: `StrokeStandard` derives a stroke from the kernel drawing ladders: an ISO 128-2 line type answers its element proportions in multiples of the line width, `LineType.Rhythm(width)` resolves them to absolute drawn-and-gap pairs at the chosen ISO 128-24 rung, and this page projects that run onto its own segment rows. A hand-typed dash roster for a standard line type is the deleted form.
- Boundary: `LinetypeCap` and `LinetypeJoin` map host enums at the edge, while `ShapeRow.TextShape` composes `TextSpec.Mint` and `StyleOp.Lens` inside the document grant, and `ShapeRow.CurveShape` carries a `GeometryHandle` read inside one `DraftBorrow` scope — the namespace law a raw `Curve` in a public payload breaks.
- Law: `StrokeDef.Apply` consumes an already admitted aggregate and mutates only a detached copy; live rows change through the grip's `Modify`, and the pattern-lock toggle the write needs is the grip's own bracket, not a save-and-restore inside the body.
- Law: `StrokeDef.Read` is the inverse of `Apply` for the channels the host publishes back and REFUSES a native carrying embedded shapes or a taper, because neither channel has a getter that reconstructs its authored form; a style holding an embedded linetype composes this read rather than a table lookup it has no row for.
- Law: admission ACCUMULATES — the aggregate's eleven columns report every violated clause at once, where one message for the widest gate in the namespace told a caller nothing about which column it had to fix.
- Packages: `Rasm.Drawing` (`LineType`, `LineType.Rhythm`, `LineWidth`), `Annotation/style.md` (`TableGrip`, `TableOp`, `ListEdit`, `ListSurface`, `TagEdit`, `TagSurface`, `DraftBorrow`, `StyleOp.Lens`), `Document/session.md` (`DraftFault`), `Document/tables.md` (`ResourceName`, `ResourceRef`, `GeometryHandle`), `Domain/rails` (`Custody`); RhinoCommon `Linetype`/`LinetypeTable` per `.api/api-rhinocommon-drafting-resources.md`.
- Growth: a display axis is one owner and one `StrokeDef` column; a standard line type is already one `LineType` row on the kernel ladder.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Globalization;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rasm.Rhino.Annotation;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<bool>]
public sealed partial class DashRole {
    public static readonly DashRole Dash = new(key: true, signed: static length => length);
    public static readonly DashRole Gap = new(key: false, signed: static length => -length);

    [UseDelegateFromConstructor]
    internal partial double Signed(double length);

    internal static DashRole For(double signed) => signed >= 0.0 ? Dash : Gap;
}

[ComplexValueObject]
[ValidationError]
public sealed partial class SegmentRow {
    public double Length { get; }
    public DashRole Role { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double length, ref DashRole role) {
        double run = length;
        validationError = double.IsFinite(run) && run > 0.0
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { Op.Of(), nameof(Length), run, "a finite positive segment length" }));
    }

    public static Fin<SegmentRow> Of(double length, DashRole role, Op? key = null) =>
        key.OrDefault().AcceptValidated<SegmentRow>(
            fault: Validate(length, role, out SegmentRow? admitted), admitted: admitted);

    internal double Signed => Role.Signed(length: Length);
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

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ShapeRow {
    private ShapeRow() { }
    public sealed record CurveShape(GeometryHandle Glyph, double Offset) : ShapeRow;
    public sealed record TextShape(TextSpec Spec, Plane Frame, ResourceRef Style, double Offset) : ShapeRow;

    internal Fin<Unit> Apply(RhinoDoc document, Linetype linetype, Op key) => Switch(
        (Document: document, Linetype: linetype, Op: key),
        curveShape: static (context, row) => row.Glyph.Typed<Curve, Unit>(key: context.Op, project: glyph =>
            from offset in context.Op.Accept(row.Offset)
            from _ in context.Op.Confirm(success: context.Linetype.AddShape(shapeCurve: glyph, offset: row.Offset))
            select unit),
        textShape: static (context, row) =>
            from frame in context.Op.Accept(row.Frame)
            from style in row.Style.Resolve(document: context.Document, lens: StyleOp.Lens, key: context.Op)
            from glyph in row.Spec.Mint(plane: row.Frame, style: style, key: context.Op)
            from offset in context.Op.Accept(row.Offset)
            from _ in context.Op.Confirm(success: context.Linetype.AddShape(text: glyph, offset: row.Offset))
            select unit);
}

[ComplexValueObject]
[ValidationError]
public sealed partial class TaperRow {
    public double StartWidth { get; }
    public Option<Point2d> Mid { get; }
    public double EndWidth { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref double startWidth, ref Option<Point2d> mid, ref double endWidth) {
        (double start, Option<Point2d> waist, double end) = (startWidth, mid, endWidth);
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
            (!double.IsFinite(start) || start <= 0.0, () => new ValidationClause(string.Join(" | ", new object?[] { Op.Of(), nameof(StartWidth), start, "a finite positive width" }))),
            (!double.IsFinite(end) || end <= 0.0, () => new ValidationClause(string.Join(" | ", new object?[] { Op.Of(), nameof(EndWidth), end, "a finite positive width" }))),
            (waist.Exists(static point => !point.IsValid), static () => new ValidationClause(string.Join(" | ", new object?[] { Op.Of(), nameof(Mid) })))));
    }

    public static Fin<TaperRow> Of(double startWidth, double endWidth, Option<Point2d> mid = default, Op? key = null) =>
        key.OrDefault().AcceptValidated<TaperRow>(
            fault: Validate(startWidth, mid, endWidth, out TaperRow? admitted), admitted: admitted);

    internal Fin<Unit> Apply(Linetype linetype, Op key) => Mid.Match(
        Some: point => key.Catch(() => Fin.Succ(value: Op.Side(
            () => linetype.SetTaper(startWidth: StartWidth, taperPoint: point, endWidth: EndWidth)))),
        None: () => key.Catch(() => Fin.Succ(value: Op.Side(
            () => linetype.SetTaper(startWidth: StartWidth, endWidth: EndWidth)))));
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ObjectFactory<string>]
[ValidationError]
public sealed partial class PatternText {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        string candidate = value;
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
            (candidate.Length is 0, static () => new ValidationClause(string.Join(" | ", new object?[] { Op.Of(), nameof(PatternText) }))),
            (candidate.Length > 0 && !Runs(candidate: candidate), static () => new ValidationClause(string.Join(" | ", new object?[] { Op.Of(), nameof(PatternText), "an alignment token followed by a comma-separated signed run of segment lengths" })))));
    }

    private static bool Runs(string candidate) {
        ReadOnlySpan<char> text = candidate.AsSpan();
        int head = text.IndexOf(',');
        if (head < 0) return false;
        ReadOnlySpan<char> run = text[(head + 1)..];
        int fields = 0;
        foreach (Range field in run.Split(',')) {
            if (!double.TryParse(run[field].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out _)) return false;
            fields++;
        }
        return fields > 0;
    }
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError]
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
        (Seq<SegmentRow> run, double stroke, HashMap<string, string> bag) = (segments, width, tags);
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
            (run.IsEmpty, static () => new ValidationClause(string.Join(" | ", new object?[] { Op.Of(), nameof(Segments) }))),
            (!double.IsFinite(stroke) || stroke <= 0.0, () => new ValidationClause(string.Join(" | ", new object?[] { Op.Of(), nameof(Width), stroke, "a finite positive stroke width" }))),
            (!bag.ForAll(static pair => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value)),
                static () => new ValidationClause(string.Join(" | ", new object?[] { Op.Of(), nameof(Tags) })))));
    }

    public static Fin<StrokeDef> Of(
        ResourceName name, Seq<SegmentRow> segments, Seq<ShapeRow> shapes, Option<TaperRow> taper,
        LinetypeCap cap, LinetypeJoin join, double width, ModelUnit widthUnits, PatternDistance distances,
        PatternLock @lock, HashMap<string, string> tags = default, Op? key = null) =>
        key.OrDefault().AcceptValidated<StrokeDef>(
            fault: Validate(name, segments, shapes, taper, cap, join, width, widthUnits, distances, @lock, tags,
                out StrokeDef? admitted),
            admitted: admitted);

    internal Seq<double> SignedRun => Segments.Map(static row => row.Signed);

    internal Fin<Unit> Apply(RhinoDoc document, Linetype linetype, Op key) =>
        from segments in key.Confirm(success: linetype.SetSegments(segments: SignedRun.AsIterable()))
        from cleared in key.Catch(() => Fin.Succ(value: Op.Side(linetype.RemoveAllShapes)))
        from shapes in Shapes.TraverseM(shape => shape.Apply(document: document, linetype: linetype, key: key)).As()
        from taper in Taper.Match(
            Some: row => row.Apply(linetype: linetype, key: key),
            None: () => key.Catch(() => Fin.Succ(value: Op.Side(linetype.RemoveTaper))))
        from configured in key.Catch(() => Fin.Succ(value: Op.Side(() => {
            linetype.Name = Name.Value;
            linetype.LineCapStyle = Cap.Host;
            linetype.LineJoinStyle = Join.Host;
            linetype.Width = Width;
            linetype.WidthUnits = WidthUnits.System;
            linetype.AlwaysModelDistances = Distances.Key;
            linetype.IsPatternLocked = Lock.Key;
        })))
        from tags in new TagEdit.Replace(Tags: Tags).Apply(owner: Surface(linetype), op: key)
        select unit;

    internal static TagSurface Surface(Linetype linetype) => new(
        linetype.GetUserStrings, linetype.SetUserString, linetype.DeleteUserString, linetype.DeleteAllUserStrings);

    internal static Fin<StrokeDef> Read(Linetype linetype, Op key) => key.Catch(() =>
        from _ in guard(!linetype.HasShapes && Optional(linetype.GetTaperPoints()).Map(static rows => rows.Length).IfNone(0) is 0,
            key.Unsupported(valueType: typeof(Linetype), outputType: typeof(StrokeDef))).ToFin()
        from name in key.AcceptValidated<ResourceName>(candidate: linetype.Name)
        from segments in toSeq(Range(from: 0, count: linetype.SegmentCount))
            .TraverseM(index => LinetypeOp.Segment(linetype: linetype, index: index, key: key)).As()
        from cap in key.AcceptValidated<LinetypeCap>(candidate: (int)linetype.LineCapStyle)
        from join in key.AcceptValidated<LinetypeJoin>(candidate: (int)linetype.LineJoinStyle)
        from widthUnits in ModelUnit.Of(value: linetype.WidthUnits, key: key)
        from distances in key.AcceptValidated<PatternDistance>(candidate: linetype.AlwaysModelDistances)
        from lockState in key.AcceptValidated<PatternLock>(candidate: linetype.IsPatternLocked)
        from definition in Of(
            name: name,
            segments: segments,
            shapes: Seq<ShapeRow>(),
            taper: Option<TaperRow>.None,
            cap: cap,
            join: join,
            width: linetype.Width,
            widthUnits: widthUnits,
            distances: distances,
            @lock: lockState,
            tags: TagOp.Snapshot(linetype.GetUserStrings()),
            key: key)
        select definition);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class StrokeStandard {
    public static Fin<StrokeDef> Def(
        ResourceName name, LineType type, LineWidth width, HashMap<string, string> tags = default, Op? key = null) {
        Op op = key.OrDefault();
        return from units in ModelUnit.Of(value: UnitSystem.Millimeters, key: op)
               from segments in Rhythm(type: type, width: width, key: op)
               from definition in StrokeDef.Of(
                   name: name,
                   segments: segments,
                   shapes: Seq<ShapeRow>(),
                   taper: Option<TaperRow>.None,
                   cap: LinetypeCap.Butt,
                   join: LinetypeJoin.Round,
                   width: width.Width.Millimeters,
                   widthUnits: units,
                   distances: PatternDistance.ModelUnits,
                   @lock: PatternLock.Editable,
                   tags: tags,
                   key: op)
               select definition;
    }

    private static Fin<Seq<SegmentRow>> Rhythm(LineType type, LineWidth width, Op key) =>
        type.IsContinuous
            ? SegmentRow.Of(length: width.Width.Millimeters, role: DashRole.Dash, key: key).Map(static row => Seq(row))
            : type.Rhythm(width: width)
                .TraverseM(pair =>
                    from drawn in SegmentRow.Of(length: pair.Drawn.Millimeters, role: DashRole.Dash, key: key)
                    from gap in SegmentRow.Of(length: pair.Gap.Millimeters, role: DashRole.Gap, key: key)
                    select Seq(drawn, gap))
                .As()
                .Map(static runs => runs.Bind(static run => run));
}
```

## [03]-[MUTATION]

- Owner: `LinetypeOp` is the linetype-table mutation program consumed by `Linetypes.Commit`: one `Table` case carrying the namespace's shared eight verbs over this table's grip, beside the six verbs this table alone has — pattern-string authoring, reference authoring, incremental segment revision, host undo of the last modify, host reset to stock, and the deleted-row revival and default roster load.
- Law: authoring, amendment, renaming, retagging, deletion, current selection, and import are the SHARED `TableOp` over `Grip` — the duplicate-then-`Modify` law, the compensated import, the plural delete arity, and the tag algebra are the namespace owner's and this page re-spells none of them. The hand land-then-delete compensation authoring once carried is GONE with the provisional row it compensated: the shared verb mints a detached native, shapes it whole, and seats it in one terminal `Add`, so there is no half-authored row to roll back.
- Law: the pattern-lock toggle is the grip's `Scoped` bracket — enter clears the lock and answers the exit that restores it, and the grip runs that exit on EVERY leg. The save-clear-restore this page once wrote inside the revise body short-circuited before its restore, so a refused segment edit left an unlocked duplicate on the way to `Modify`; a bracket the owner runs never skips.
- Law: `Relist` is the incremental segment rail beside whole-aggregate amendment — `ListEdit<SegmentRow>` revises a native copy in place through the declared `ListSurface` and lands through the same `Modify`, so amending one segment of a long run never re-admits the rest. The list's floor is ONE, so a remove that empties the run refuses and `Clear` refuses outright, both from the same declaration.
- Law: `Undelete` alone resolves through the deleted-inclusive id, name, and index lens; every active operation retains the active-only lens, and `WithPolicy` is the one row-driven factory both memoized lenses instantiate.
- Law: the table publishes a `.lin` READER and no writer, so the grip states `Ingest` and leaves `Emit` absent — a `TableOp.Export` against this table refuses typed rather than compiling against a writer that does not exist.
- Entry: `Linetypes.Commit` preserves the frozen wire and accepts `DraftPlan<LinetypeOp>` with shared redraw and undo policy.
- Packages: `Annotation/style.md` (`TableGrip`, `TableOp`, `ListEdit`, `ListSurface`, `TagEdit`, `DraftPlan`, `DraftSpine`, `DraftCount`), `Document/commit.md` (`HostInteraction`), `Document/tables.md` (`ResourceLens`, `ResourceRef`, `ResourceIndex`), `Domain/rails` (`Custody`); RhinoCommon `LinetypeTable` per `.api/api-rhinocommon-drafting-resources.md`.
- Growth: a verb every component table shares lands on `TableOp`; a linetype-only verb is one case here.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<ObjectLinetypeSource>]
public sealed partial class LinetypeSource {
    public static readonly LinetypeSource ByLayer = new(key: ObjectLinetypeSource.LinetypeFromLayer);
    public static readonly LinetypeSource ByObject = new(key: ObjectLinetypeSource.LinetypeFromObject);
    public static readonly LinetypeSource ByParent = new(key: ObjectLinetypeSource.LinetypeFromParent);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LinetypeOp {
    private LinetypeOp() { }
    public sealed record Table(TableOp<Linetype, StrokeDef> Verb) : LinetypeOp;
    public sealed record AuthorPattern(ResourceName Name, PatternText Pattern, PatternMeasure Measure, HostInteraction Interaction, HashMap<string, string> Tags = default) : LinetypeOp;
    public sealed record AuthorReference(ResourceRef Source) : LinetypeOp;
    public sealed record Relist(ResourceRef Target, Seq<ListEdit<SegmentRow>> Edits, HostInteraction Interaction) : LinetypeOp;
    public sealed record Revert(ResourceRef Target) : LinetypeOp;
    public sealed record Reset(ResourceRef Target, HostInteraction Interaction) : LinetypeOp;
    public sealed record Undelete(ResourceRef Target) : LinetypeOp;
    public sealed record LoadDefaults(DeletedRows Policy) : LinetypeOp;

    internal static Fin<SegmentRow> Segment(Linetype linetype, int index, Op key) =>
        key.Catch(() => {
            linetype.GetSegment(index: index, length: out double length, isSolid: out bool solid);
            return SegmentRow.Of(length: double.Abs(length), role: solid ? DashRole.Dash : DashRole.Gap, key: key);
        });

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

    internal static readonly TableGrip<Linetype, StrokeDef> Grip = new(
        Lens,
        Named: static def => def.Name,
        Title: static (linetype, key) => key.AcceptValidated<ResourceName>(candidate: linetype.Name),
        Index: static linetype => linetype.LinetypeIndex,
        Duplicate: static live => new Linetype(other: live),
        Tags: StrokeDef.Surface,
        Mint: static (document, def, key) =>
            from shaped in key.Catch(() => Fin.Succ(value: new Linetype()))
            from _ in def.Apply(document: document, linetype: shaped, key: key)
                .BindFail(primary => Fin.Fail<Unit>(error: primary).Rollback(
                    release: () => Custody.Dispose(held: Seq(shaped), key: key), key: key))
            select shaped,
        Revise: static (document, copy, def, key) => def.Apply(document: document, linetype: copy, key: key),
        Retitle: static (copy, name, key) => key.Catch(() => Fin.Succ(value: Op.Side(() => copy.Name = name.Value))),
        Modify: static (document, copy, index, interaction, key) => key.Confirm(success: document.Linetypes.Modify(
            linetype: copy, index: index, quiet: interaction.IsQuiet)),
        Seat: static (document, linetype, key) => key.Catch(() => ResourceIndex.Admit(
            document.Linetypes.Add(linetype: linetype), key)),
        Retire: static (document, indices, interaction, key) => key.Confirm(success: document.Linetypes.Delete(
            indices: indices.AsIterable(), quiet: interaction.IsQuiet)),
        Elect: static (document, index, interaction, key) => key.Confirm(success: document.Linetypes.SetCurrentLinetypeIndex(
            linetypeIndex: index, quiet: interaction.IsQuiet)),
        Scoped: static (copy, key) => key.Catch(() => {
            bool locked = copy.IsPatternLocked;
            copy.IsPatternLocked = false;
            return Fin.Succ<Func<Op, Fin<Unit>>>(value: exit =>
                exit.Catch(() => Fin.Succ(value: Op.Side(() => copy.IsPatternLocked = locked))));
        }),
        Ingest: static (path, _, key) => key.Catch(() => Optional(Linetype.ReadFromFile(path: path.Value))
            .Map(static values => toSeq(values).Strict())
            .ToFin(Fail: key.InvalidResult())));

    internal Fin<Unit> Apply(RhinoDoc document, Op op) => Switch(
        (Document: document, Op: op),
        table: static (context, edit) => edit.Verb.Apply(grip: Grip, document: context.Document, op: context.Op),
        authorPattern: static (context, edit) =>
            from _ in guard(!Grip.Occupied(context.Document, edit.Name), context.Op.InvalidInput()).ToFin()
            from built in context.Op.Catch(() => Optional(Linetype.CreateFromPatternString(
                    patternString: edit.Pattern.Value, millimeters: edit.Measure.Key))
                .ToFin(Fail: context.Op.InvalidResult()))
            from __ in new Lease<Linetype>.Owned(Value: built).Use(owned =>
                from ___ in context.Op.Catch(() => Fin.Succ(value: Op.Side(() => owned.Name = edit.Name.Value)))
                from ____ in new TagEdit.Replace(Tags: edit.Tags).Apply(owner: StrokeDef.Surface(owned), op: context.Op)
                from _____ in Grip.Seat(context.Document, owned, context.Op)
                select unit)
            select unit,
        authorReference: static (context, edit) =>
            from definition in edit.Source.Resolve(document: context.Document, lens: Lens, key: context.Op)
            from name in context.Op.AcceptValidated<ResourceName>(candidate: definition.Name)
            from _ in guard(!Grip.Occupied(context.Document, name), context.Op.InvalidInput()).ToFin()
            from __ in context.Op.Catch(() =>
                ResourceIndex.Admit(context.Document.Linetypes.AddReferenceLinetype(linetype: definition), context.Op))
            select unit,
        relist: static (context, edit) =>
            from _ in guard(!edit.Edits.IsEmpty, context.Op.InvalidInput()).ToFin()
            from __ in Grip.Revised(
                target: edit.Target, document: context.Document,
                interaction: edit.Interaction, op: context.Op,
                revise: (copy, key) => edit.Edits
                    .TraverseM(row => row.Apply(surface: Run(copy), op: key)).As().Map(static _ => unit))
            select unit,
        revert: static (context, edit) =>
            from linetype in edit.Target.Resolve(document: context.Document, lens: Lens, key: context.Op)
            from _ in context.Op.Confirm(success: context.Document.Linetypes.UndoModify(index: linetype.LinetypeIndex))
            select unit,
        reset: static (context, edit) =>
            Grip.Revised(target: edit.Target, document: context.Document,
                interaction: edit.Interaction, op: context.Op,
                revise: static (copy, key) => key.Catch(() => Fin.Succ(value: Op.Side(copy.Default)))),
        undelete: static (context, edit) =>
            from linetype in edit.Target.Resolve(document: context.Document, lens: ReviveLens, key: context.Op)
            from _ in context.Op.Confirm(success: context.Document.Linetypes.Undelete(index: linetype.LinetypeIndex))
            select unit,
        loadDefaults: static (context, edit) =>
            from count in context.Op.Catch(() => Fin.Succ(value: context.Document.Linetypes.LoadDefaultLinetypes(
                ignoreDeleted: edit.Policy.Key)))
            from _ in guard(count >= 0, context.Op.InvalidResult()).ToFin()
            select unit);

    private static ListSurface<SegmentRow> Run(Linetype linetype) => new(
        Count: () => linetype.SegmentCount,
        Append: (row, key) =>
            from index in key.Catch(() => Fin.Succ(value: linetype.AppendSegment(
                length: row.Length, isSolid: row.Role == DashRole.Dash)))
            from _ in guard(index >= 0, key.InvalidResult()).ToFin()
            select unit,
        Remove: (index, key) => key.Confirm(success: linetype.RemoveSegment(index: index)),
        Write: Some<Func<int, SegmentRow, Op, Fin<Unit>>>((index, row, key) => key.Confirm(success: linetype.SetSegment(
            index: index, length: row.Length, isSolid: row.Role == DashRole.Dash))),
        Purge: default,
        Floor: 1);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Linetypes {
    public static Fin<Unit> Commit(DocumentSession session, DraftPlan<LinetypeOp> plan) =>
        DraftSpine.Commit(session: session, plan: plan,
            apply: static (document, operation, key) => operation.Apply(document: document, op: key),
            op: Op.Of(name: nameof(Linetypes)));

    public static Fin<LinetypeAnswer> Ask(DocumentSession session, LinetypeAsk request) {
        Op op = Op.Of(name: nameof(Linetypes));
        return from admitted in op.AcceptInput(value: request)
               from answer in session.Demand(
                   use: document => admitted.Answer(document: document, op: op), key: op, needs: [SessionNeed.Read])
               select answer;
    }
}
```

## [04]-[PROJECTION]

- Owner: `StrokeSnapshot` preserves identity, segment run, pattern length, aggregate shape evidence, taper points, display policies, distance policy, lifecycle state, tags, and `.lin` pattern text.
- Owner: `ShapeEvidence` states what the host DOES publish about embedded glyphs — spacing, gap, local offset, and the optional bounding box — and presence is the box's own `IsSome`, because both once read one `HasShapes` probe and a second column over one fact drifts the moment either write lands.
- Boundary: `Linetype` exposes no embedded-shape getter, so projection records host aggregate shape evidence and never fabricates a reconstructable `ShapeRow` roster.
- Law: `PatternMeasure` names the unit regime consumed by both `CreateFromPatternString` and `PatternString`; a raw `bool` never escapes the edge, and the pattern text a read answers admits through the same `PatternText` grammar an authoring caller supplies.
- Law: `GetSegment` reports no verdict beside its two `out` values, so the read folds both through `SegmentRow`'s positive-length gate and an out-of-range index surfaces as that admission refusal rather than a silently zero-length row.
- Law: the table state crosses `LinetypeSource`, never raw `ObjectLinetypeSource`, and `AuthorReference` addresses its source through `ResourceRef` — a live `Linetype` stays inside the document grant. The row roster carries no by-object projection column: nothing in the corpus read it, and a policy column no consumer selects on is decorative.
- Law: unused-name minting carries no deleted-row policy — the host's `GetUnusedLinetypeName(bool)` overload is `[Obsolete]` and delegates straight to the parameterless form, so the fence spells the live one; `DeletedRows` remains the `LoadDefaults` and revive-lens policy.
- Law: `ForObject` delegates layer and parent inheritance to `LinetypeIndexForObject` and returns the canonical `ResourceRef` address.
- Packages: `Annotation/style.md` (`DraftScale`, `DraftCount`, `TargetResolution`), `Document/tables.md` (`ResourceRef`, `ResourceName`, `ResourceIndex`); RhinoCommon `Linetype`/`LinetypeTable` read members per `.api/api-rhinocommon-drafting-resources.md`.
- Growth: a read is one `LinetypeAsk` case with its `LinetypeAnswer` twin.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record ShapeEvidence(
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
    PatternText Pattern) : IDetachedDocumentResult;

public sealed record LinetypeTableState(
    DraftCount Active,
    ResourceIndex Current,
    LinetypeSource CurrentSource,
    DraftScale Scale,
    ResourceName Continuous,
    ResourceName ByLayer,
    ResourceName ByParent) : IDetachedDocumentResult;

// --- [OPERATIONS] ----------------------------------------------------------------------
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
            from name in context.Op.AcceptValidated<ResourceName>(candidate: linetype.Name)
            from segments in toSeq(Range(from: 0, count: linetype.SegmentCount))
                .TraverseM(index => LinetypeOp.Segment(linetype: linetype, index: index, key: context.Op)).As()
            from cap in context.Op.AcceptValidated<LinetypeCap>(candidate: (int)linetype.LineCapStyle)
            from join in context.Op.AcceptValidated<LinetypeJoin>(candidate: (int)linetype.LineJoinStyle)
            from widthUnits in ModelUnit.Of(value: linetype.WidthUnits, key: context.Op)
            from distances in context.Op.AcceptValidated<PatternDistance>(candidate: linetype.AlwaysModelDistances)
            from lockState in context.Op.AcceptValidated<PatternLock>(candidate: linetype.IsPatternLocked)
            from pattern in context.Op.AcceptValidated<PatternText>(
                candidate: linetype.PatternString(millimeters: ask.Measure.Key))
            select (LinetypeAnswer)new LinetypeAnswer.State(new StrokeSnapshot(
                ResourceId.Create(linetype.Id),
                ResourceIndex.Create(linetype.LinetypeIndex),
                name,
                segments,
                linetype.PatternLength,
                new ShapeEvidence(
                    linetype.ShapeSpacing,
                    linetype.ShapeGap,
                    linetype.ShapeLocalOffset,
                    linetype.HasShapes ? Some(linetype.ShapeBounds) : None),
                Optional(linetype.GetTaperPoints()).Map(toSeq).IfNone(Seq<Point2d>()),
                cap,
                join,
                linetype.Width,
                widthUnits,
                distances,
                lockState,
                linetype.InUse,
                linetype.IsModified,
                TagOp.Snapshot(linetype.GetUserStrings()),
                pattern))),
        tableState: static (context, _) =>
            from source in context.Op.AcceptValidated<LinetypeSource>(
                candidate: context.Document.Linetypes.CurrentLinetypeSource)
            from scale in context.Op.AcceptValidated<DraftScale>(
                candidate: context.Document.Linetypes.LinetypeScale)
            from active in context.Op.AcceptValidated<DraftCount>(
                candidate: context.Document.Linetypes.ActiveCount)
            from continuous in context.Op.AcceptValidated<ResourceName>(
                candidate: context.Document.Linetypes.ContinuousLinetypeName)
            from byLayer in context.Op.AcceptValidated<ResourceName>(
                candidate: context.Document.Linetypes.ByLayerLinetypeName)
            from byParent in context.Op.AcceptValidated<ResourceName>(
                candidate: context.Document.Linetypes.ByParentLinetypeName)
            select (LinetypeAnswer)new LinetypeAnswer.Rows(new LinetypeTableState(
                active,
                ResourceIndex.Create(context.Document.Linetypes.CurrentLinetypeIndex),
                source,
                scale,
                continuous,
                byLayer,
                byParent)),
        forObject: static (context, ask) =>
            from row in ask.Target.Only<RhinoObject>(document: context.Document, key: context.Op)
            from index in context.Op.Catch(() => Fin.Succ(value: context.Document.Linetypes.LinetypeIndexForObject(
                rhinoObject: row.Native)))
            from address in ResourceRef.Of(index: index)
            select (LinetypeAnswer)new LinetypeAnswer.Resolved(address),
        mintName: static (context, _) =>
            from name in context.Op.Catch(() => context.Op.AcceptValidated<ResourceName>(
                candidate: context.Document.Linetypes.GetUnusedLinetypeName()))
            select (LinetypeAnswer)new LinetypeAnswer.Minted(name));
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
-->

(none)

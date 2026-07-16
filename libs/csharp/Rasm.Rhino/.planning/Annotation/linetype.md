# [RASM_RHINO_ANNOTATION_LINETYPE]

Linetype rail (`Rasm.Rhino.Annotation`). `StrokeDef` owns dash/gap segments, embedded curve/text shapes, optional taper, and stroke configuration behind `LinetypeTable`. A segment is the verified `(length, isSolid)` pair; `SegmentRow.Signed` alone projects the positive-dash/negative-gap encoding consumed by `SetSegments` and `.lin` grammar. Host defaults remain table names plus `LoadDefaultLinetypes`, never a fabricated static roster. Embedded text shapes carry `TextSpec`, frame, and style reference, then mint through the text rail inside the document grant. `.lin` interchange rides `PatternString`/`CreateFromPatternString`/`ReadFromFile`. Reclamation stays `TableOp.Reclaim(TableKind.Linetypes)`.

## [01]-[INDEX]

- [02]-[STROKE_MODEL]: `SegmentRow`, `ShapeRow`, `TaperRow`, and the `StrokeDef` definition model.
- [03]-[LINETYPE_RAIL]: `LinetypeOp`, `LinetypeTransaction`, and the `Linetypes` entry pair.
- [04]-[ASK_FAMILY]: `LinetypeAsk`/`LinetypeAnswer` — definition snapshot, table state, object resolution, and pattern text.
- [05]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[STROKE_MODEL]

- Owner: `SegmentRow` — one dash or gap with its length; `ShapeRow` `[Union]` — an embedded curve or text glyph positioned by offset along the pattern; `TaperRow` — the start/end width pair with an optional mid taper point; `StrokeDef` — the whole definition: name, segment run, shapes, taper, caps, joins, width, width units, and the model-distance grant.
- Law: the signed convention lives on one projection — `SegmentRow.Signed` renders positive dash and negative gap for the table's `Add(name, segmentLengths)` and the `.lin` grammar, and the typed pair is what travels; a raw signed `double` in request data is the deleted form.
- Law: definition content applies to a duplicate — `Apply` writes segments (`SetSegments`), shapes (`AddShape` per row after `RemoveAllShapes`), taper (`SetTaper`/`RemoveTaper`), and stroke config onto a `DuplicateLinetype` copy, and the table `Modify` lands it inside the bracket; `CommitChanges` on a live linetype bypasses the transaction and is the deleted form.
- Law: a text shape embeds detached geometry — the `TextEntity` enters through this page's sibling `TextSpec.Mint`, so linetype glyphs and document text share one construction path.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
public readonly record struct SegmentRow(double Length, bool Solid) {
    public static Fin<SegmentRow> Of(double length, bool solid, Op? key = null) =>
        key.OrDefault().Positive(value: length).Map(valid => new SegmentRow(Length: valid, Solid: solid));

    internal double Signed => Solid ? Length : -Length;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ShapeRow {
    private ShapeRow() { }
    public sealed record CurveShape(Curve Glyph, double Offset) : ShapeRow;
    public sealed record TextShape(TextSpec Spec, Plane Frame, ResourceRef Style, double Offset) : ShapeRow;

    internal Fin<Unit> Apply(RhinoDoc document, Linetype linetype, Op key) =>
        Switch(
            state: (Document: document, Linetype: linetype, Op: key),
            curveShape: static (ctx, row) =>
                from glyph in Optional(row.Glyph).ToFin(Fail: ctx.Op.InvalidInput())
                from offset in ctx.Op.AcceptInput(value: row.Offset)
                from added in ctx.Op.Confirm(success: ctx.Linetype.AddShape(shapeCurve: glyph, offset: offset))
                select added,
            textShape: static (ctx, row) =>
                from spec in Optional(row.Spec).ToFin(Fail: ctx.Op.InvalidInput())
                from address in Optional(row.Style).ToFin(Fail: ctx.Op.InvalidInput())
                from style in address.Resolve(document: ctx.Document, lens: StyleOp.Lens, key: ctx.Op)
                from glyph in spec.Mint(plane: row.Frame, style: style, key: ctx.Op)
                from offset in ctx.Op.AcceptInput(value: row.Offset)
                from added in ctx.Op.Confirm(success: ctx.Linetype.AddShape(text: glyph, offset: offset))
                select added);
}

public readonly record struct TaperRow(double StartWidth, Option<Point2d> Mid, double EndWidth) {
    public static Fin<TaperRow> Of(double startWidth, double endWidth, Option<Point2d> mid = default, Op? key = null) {
        Op op = key.OrDefault();
        return from start in op.Positive(value: startWidth)
               from end in op.Positive(value: endWidth)
               from _ in mid.Match(
                   Some: point => op.AcceptInput(value: point).Map(static _ => unit),
                   None: () => Fin.Succ(value: unit))
               select new TaperRow(StartWidth: start, Mid: mid, EndWidth: end);
    }
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record StrokeDef(
    string Name,
    Seq<SegmentRow> Segments,
    Seq<ShapeRow> Shapes,
    Option<TaperRow> Taper,
    Rhino.Display.LineCapStyle Cap,
    Rhino.Display.LineJoinStyle Join,
    double Width,
    UnitSystem WidthUnits,
    bool AlwaysModelDistances = false) {
    public static Fin<StrokeDef> Of(
        string name, Seq<SegmentRow> segments,
        Option<TaperRow> taper = default,
        Rhino.Display.LineCapStyle cap = Rhino.Display.LineCapStyle.Round,
        Rhino.Display.LineJoinStyle join = Rhino.Display.LineJoinStyle.Round,
        double width = 1.0, UnitSystem widthUnits = UnitSystem.Millimeters,
        bool alwaysModelDistances = false,
        params ReadOnlySpan<ShapeRow> shapes) {
        Op op = Op.Of(name: nameof(StrokeDef));
        return from label in op.AcceptText(value: name)
               from run in segments.TraverseM(segment => SegmentRow.Of(
                   length: segment.Length, solid: segment.Solid, key: op)).As()
               from _ in guard(!run.IsEmpty, op.InvalidInput()).ToFin()
               from narrowedTaper in taper.Traverse(row => TaperRow.Of(
                   startWidth: row.StartWidth, endWidth: row.EndWidth, mid: row.Mid, key: op)).As()
               from stroke in op.Positive(value: width)
               from glyphs in toSeq(shapes.ToArray()).TraverseM(shape => Optional(shape).ToFin(Fail: op.InvalidInput())).As()
               select new StrokeDef(
                   Name: label, Segments: run, Shapes: glyphs, Taper: narrowedTaper,
                   Cap: cap, Join: join, Width: stroke, WidthUnits: widthUnits,
                   AlwaysModelDistances: alwaysModelDistances);
    }

    internal Seq<double> SignedRun => Segments.Map(static row => row.Signed);

    internal Fin<Unit> Apply(RhinoDoc document, Linetype linetype, Op key) {
        return from self in Of(
                   name: Name, segments: Segments, taper: Taper,
                   cap: Cap, join: Join, width: Width, widthUnits: WidthUnits,
                   alwaysModelDistances: AlwaysModelDistances, shapes: Shapes.ToArray())
               from _ in key.Confirm(success: linetype.SetSegments(segments: self.SignedRun.AsIterable()))
               from __ in key.Catch(() => {
                   linetype.RemoveAllShapes();
                   return Fin.Succ(value: unit);
               })
               from ___ in self.Shapes.TraverseM(shape => shape.Apply(document: document, linetype: linetype, key: key)).As()
               from ____ in key.Catch(() => {
                   _ = self.Taper.Match(
                       Some: taper => taper.Mid.Match(
                           Some: mid => fun(() => linetype.SetTaper(startWidth: taper.StartWidth, taperPoint: mid, endWidth: taper.EndWidth))(),
                           None: () => fun(() => linetype.SetTaper(startWidth: taper.StartWidth, endWidth: taper.EndWidth))()),
                       None: () => fun(linetype.RemoveTaper)());
                   linetype.LineCapStyle = self.Cap;
                   linetype.LineJoinStyle = self.Join;
                   linetype.Width = self.Width;
                   linetype.WidthUnits = self.WidthUnits;
                   linetype.AlwaysModelDistances = self.AlwaysModelDistances;
                   return Fin.Succ(value: unit);
               })
               select unit;
    }
}
```

## [03]-[LINETYPE_RAIL]

- Owner: `LinetypeOp` `[Union]` — authoring from a def or a `.lin` pattern string, amendment, host-side revert, deletion and revival, default loading, current selection, `.lin` file import, and the user-string bag; `LinetypeTransaction` — the commit plan; `Linetypes` — the `Commit`/`Ask` entry pair.
- Law: authoring is add-then-shape — `Add(name, segmentLengths)` mints the row from the signed run, then shapes, taper, and stroke config land through duplicate-and-`Modify`; a receipt appears only after `Modify` succeeds.
- Law: `Revert` is the host's definition rollback — `UndoModify(index)` restores pre-`Modify` content and stays distinct from the shared document undo record.
- Law: defaults load as a verb — `LoadDefaultLinetypes()` answers a tally receipt, and the named-default rows (`ContinuousLinetypeName`, `ByLayerLinetypeName`, `ByParentLinetypeName`) are roster facts, never a static vocabulary this rail re-mints.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LinetypeOp {
    private LinetypeOp() { }
    public sealed record Author(StrokeDef Def) : LinetypeOp;
    public sealed record AuthorPattern(string Name, string Pattern, bool Millimeters = true) : LinetypeOp;
    public sealed record Amend(ResourceRef Target, StrokeDef Def, bool Quiet = true) : LinetypeOp;
    public sealed record Revert(ResourceRef Target) : LinetypeOp;
    public sealed record Delete(ResourceRef Target, bool Quiet = true) : LinetypeOp;
    public sealed record Undelete(ResourceRef Target) : LinetypeOp;
    public sealed record LoadDefaults : LinetypeOp;
    public sealed record SetCurrent(ResourceRef Target, bool Quiet = true) : LinetypeOp;
    public sealed record Import(string Path) : LinetypeOp;
    public sealed record Tag(ResourceRef Target, string Key, Option<string> Value = default) : LinetypeOp;

    internal static readonly ResourceLens<Linetype> Lens = new(
        ById: static (document, id) => document.Linetypes.Find(id: id, ignoreDeletedLinetypes: true) is var index && index >= 0
            ? document.Linetypes.FindIndex(index: index)
            : null,
        ByName: static (document, name) => document.Linetypes.FindName(name: name),
        ByIndex: static (document, index) => document.Linetypes.FindIndex(index: index));

    internal Fin<DraftReceipt> Apply(RhinoDoc document, Op op) =>
        Switch(
            (Document: document, Op: op),
            author: static (context, edit) =>
                from _ in guard(context.Document.Linetypes.FindName(name: edit.Def.Name) is null, context.Op.InvalidInput()).ToFin()
                from index in context.Op.Catch(() => context.Document.Linetypes.Add(
                        name: edit.Def.Name, segmentLengths: edit.Def.SignedRun.AsIterable()) is var added && added >= 0
                    ? Fin.Succ(value: added)
                    : Fin.Fail<int>(error: context.Op.InvalidResult()))
                from receipt in Revised(document: context.Document, index: index, quiet: true, op: context.Op,
                    revise: (linetype, key) => edit.Def.Apply(document: context.Document, linetype: linetype, key: key), slot: DraftSlot.Authored)
                select receipt,
            authorPattern: static (context, edit) =>
                from name in context.Op.AcceptText(value: edit.Name)
                from pattern in context.Op.AcceptText(value: edit.Pattern)
                from _ in guard(context.Document.Linetypes.FindName(name: name) is null, context.Op.InvalidInput()).ToFin()
                from built in context.Op.Catch(() => Optional(Linetype.CreateFromPatternString(
                        patternString: pattern, millimeters: edit.Millimeters))
                    .ToFin(Fail: context.Op.InvalidResult()))
                from index in context.Op.Catch(() => {
                    built.Name = name;
                    int added = context.Document.Linetypes.Add(linetype: built);
                    return added >= 0 ? Fin.Succ(value: added) : Fin.Fail<int>(error: context.Op.InvalidResult());
                })
                select DraftReceipt.Component(slot: DraftSlot.Authored, index: index),
            amend: static (context, edit) =>
                from linetype in edit.Target.Resolve(document: context.Document, lens: Lens, key: context.Op)
                from receipt in Revised(document: context.Document, index: linetype.LinetypeIndex, quiet: edit.Quiet, op: context.Op,
                    revise: (copy, key) => edit.Def.Apply(document: context.Document, linetype: copy, key: key), slot: DraftSlot.Amended)
                select receipt,
            revert: static (context, edit) =>
                from linetype in edit.Target.Resolve(document: context.Document, lens: Lens, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.Linetypes.UndoModify(index: linetype.LinetypeIndex))
                select DraftReceipt.Component(slot: DraftSlot.Amended, index: linetype.LinetypeIndex),
            delete: static (context, edit) =>
                from linetype in edit.Target.Resolve(document: context.Document, lens: Lens, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.Linetypes.Delete(index: linetype.LinetypeIndex, quiet: edit.Quiet))
                select DraftReceipt.Component(slot: DraftSlot.Deleted, index: linetype.LinetypeIndex),
            undelete: static (context, edit) =>
                from linetype in edit.Target.Resolve(document: context.Document, lens: Lens, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.Linetypes.Undelete(index: linetype.LinetypeIndex))
                select DraftReceipt.Component(slot: DraftSlot.Revived, index: linetype.LinetypeIndex),
            loadDefaults: static (context, _) =>
                from tally in context.Op.Catch(() => Fin.Succ(value: context.Document.Linetypes.LoadDefaultLinetypes()))
                select DraftReceipt.Tally(slot: DraftSlot.Loaded, count: tally),
            setCurrent: static (context, edit) =>
                from linetype in edit.Target.Resolve(document: context.Document, lens: Lens, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.Linetypes.SetCurrentLinetypeIndex(
                    linetypeIndex: linetype.LinetypeIndex, quiet: edit.Quiet))
                select DraftReceipt.Component(slot: DraftSlot.Current, index: linetype.LinetypeIndex),
            import: static (context, edit) =>
                from path in context.Op.AcceptText(value: edit.Path)
                from read in context.Op.Catch(() => Optional(Linetype.ReadFromFile(path: path))
                    .Map(static linetypes => toSeq(linetypes))
                    .Filter(static linetypes => !linetypes.IsEmpty)
                    .ToFin(Fail: context.Op.InvalidResult()))
                from landed in read.TraverseM(linetype =>
                    guard(context.Document.Linetypes.FindName(name: linetype.Name) is null, context.Op.InvalidInput()).ToFin()
                        .Bind(_ => context.Op.Catch(() => context.Document.Linetypes.Add(linetype: linetype) is var index && index >= 0
                            ? Fin.Succ(value: index)
                            : Fin.Fail<int>(error: context.Op.InvalidResult())))).As()
                select landed.Fold(
                    DraftReceipt.Path(slot: DraftSlot.Imported, path: path),
                    static (state, index) => state + DraftReceipt.Component(slot: DraftSlot.Imported, index: index)),
            tag: static (context, edit) =>
                from linetype in edit.Target.Resolve(document: context.Document, lens: Lens, key: context.Op)
                from key in context.Op.AcceptText(value: edit.Key)
                from receipt in Revised(
                    document: context.Document,
                    index: linetype.LinetypeIndex,
                    quiet: true,
                    op: context.Op,
                    revise: (copy, op) => edit.Value.Match(
                        Some: value => op.Confirm(success: copy.SetUserString(key: key, value: value)),
                        None: () => op.Confirm(success: copy.DeleteUserString(key: key))),
                    slot: DraftSlot.Amended)
                select receipt);

    private static Fin<DraftReceipt> Revised(
        RhinoDoc document, int index, bool quiet, Op op,
        Func<Linetype, Op, Fin<Unit>> revise, DraftSlot slot) =>
        from live in Optional(document.Linetypes.FindIndex(index: index)).ToFin(Fail: op.MissingContext())
        from copy in op.Catch(() => Fin.Succ(value: live.DuplicateLinetype()))
        from _ in revise(copy, op)
        from __ in op.Confirm(success: document.Linetypes.Modify(linetype: copy, index: index, quiet: quiet))
        select DraftReceipt.Component(slot: slot, index: index);
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record LinetypeTransaction(string Name, Seq<LinetypeOp> Operations, RedrawPolicy Redraw, bool UndoRecorded = true) {
    public static LinetypeTransaction Batch(string name, params ReadOnlySpan<LinetypeOp> operations) =>
        new(Name: name, Operations: toSeq(operations.ToArray()), Redraw: RedrawPolicy.Deferred);
}
```

## [04]-[ASK_FAMILY]

- Owner: `StrokeSnapshot` — identity, segment run through `GetSegment`, pattern length, shape facts, taper points, stroke config, usage/dirty flags, lock state, and `.lin` pattern text; `LinetypeAsk`/`LinetypeAnswer` — definition snapshot, catalogued table state, per-object resolution, and unused-name minting.
- Law: segment and stroke configuration round-trip through the snapshot; embedded shape geometry does not. Shape spacing, gap, local offset, and bounds cross as detached facts, never live glyph handles.
- Law: per-object resolution is the host's — `LinetypeIndexForObject` answers the effective index under layer and parent inheritance, and a consumer re-deriving the source chain from `ObjectLinetypeSource` re-implements what the host already resolves.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record StrokeSnapshot(
    Guid Key,
    int Index,
    string Name,
    Seq<SegmentRow> Segments,
    double PatternLength,
    bool HasShapes,
    double ShapeSpacing,
    double ShapeGap,
    Vector2d ShapeLocalOffset,
    Option<BoundingBox> ShapeBounds,
    Seq<Point2d> TaperPoints,
    Rhino.Display.LineCapStyle Cap,
    Rhino.Display.LineJoinStyle Join,
    double Width,
    UnitSystem WidthUnits,
    bool AlwaysModelDistances,
    bool IsPatternLocked,
    bool InUse,
    bool IsModified,
    string PatternText) : IDetachedDocumentResult;

// --- [OPERATIONS] ---------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LinetypeAsk {
    private LinetypeAsk() { }
    public sealed record State(ResourceRef Target, bool Millimeters = true) : LinetypeAsk;
    public sealed record TableState : LinetypeAsk;
    public sealed record ForObject(TableTarget Target) : LinetypeAsk;
    public sealed record MintName : LinetypeAsk;

    internal Fin<LinetypeAnswer> Answer(RhinoDoc document, Op op) =>
        Switch(
            context: (Document: document, Op: op),
            state: static (ctx, ask) =>
                from linetype in ask.Target.Resolve(document: ctx.Document, lens: LinetypeOp.Lens, key: ctx.Op)
                from segments in ctx.Op.Catch(() => Fin.Succ(value: toSeq(Enumerable.Range(start: 0, count: linetype.SegmentCount))
                    .Map(index => {
                        linetype.GetSegment(index: index, length: out double length, isSolid: out bool solid);
                        return new SegmentRow(Length: double.Abs(length), Solid: solid);
                    })))
                from snapshot in ctx.Op.Catch(() => Fin.Succ(value: new StrokeSnapshot(
                    Key: linetype.Id,
                    Index: linetype.LinetypeIndex,
                    Name: linetype.Name,
                    Segments: segments,
                    PatternLength: linetype.PatternLength,
                    HasShapes: linetype.HasShapes,
                    ShapeSpacing: linetype.ShapeSpacing,
                    ShapeGap: linetype.ShapeGap,
                    ShapeLocalOffset: linetype.ShapeLocalOffset,
                    ShapeBounds: linetype.HasShapes ? Some(linetype.ShapeBounds) : None,
                    TaperPoints: toSeq(linetype.GetTaperPoints() ?? []),
                    Cap: linetype.LineCapStyle,
                    Join: linetype.LineJoinStyle,
                    Width: linetype.Width,
                    WidthUnits: linetype.WidthUnits,
                    AlwaysModelDistances: linetype.AlwaysModelDistances,
                    IsPatternLocked: linetype.IsPatternLocked,
                    InUse: linetype.InUse,
                    IsModified: linetype.IsModified,
                    PatternText: linetype.PatternString(millimeters: ask.Millimeters))))
                select (LinetypeAnswer)new LinetypeAnswer.State(Snapshot: snapshot),
            tableState: static (ctx, _) => ctx.Op.Catch(() => Fin.Succ<LinetypeAnswer>(value: new LinetypeAnswer.Rows(
                ActiveCount: ctx.Document.Linetypes.ActiveCount,
                CurrentIndex: ctx.Document.Linetypes.CurrentLinetypeIndex,
                Scale: ctx.Document.Linetypes.LinetypeScale,
                ContinuousName: ctx.Document.Linetypes.ContinuousLinetypeName,
                ByLayerName: ctx.Document.Linetypes.ByLayerLinetypeName,
                ByParentName: ctx.Document.Linetypes.ByParentLinetypeName))),
            forObject: static (ctx, ask) =>
                from ids in ask.Target.Resolve(document: ctx.Document, key: ctx.Op)
                from id in ids switch { [Guid only] => Fin.Succ(value: only), _ => Fin.Fail<Guid>(error: ctx.Op.InvalidInput()) }
                from native in Optional(ctx.Document.Objects.FindId(id)).ToFin(Fail: ctx.Op.MissingContext())
                from index in ctx.Op.Catch(() => Fin.Succ(value: ctx.Document.Linetypes.LinetypeIndexForObject(rhinoObject: native)))
                from address in ResourceRef.Of(index: index)
                select (LinetypeAnswer)new LinetypeAnswer.Resolved(Linetype: address),
            mintName: static (ctx, _) =>
                from minted in ctx.Op.AcceptText(value: ctx.Document.Linetypes.GetUnusedLinetypeName())
                select (LinetypeAnswer)new LinetypeAnswer.Minted(Name: minted));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LinetypeAnswer : IDetachedDocumentResult {
    private LinetypeAnswer() { }
    public sealed record State(StrokeSnapshot Snapshot) : LinetypeAnswer;
    public sealed record Rows(
        int ActiveCount,
        int CurrentIndex,
        double Scale,
        string ContinuousName,
        string ByLayerName,
        string ByParentName) : LinetypeAnswer;
    public sealed record Resolved(ResourceRef Linetype) : LinetypeAnswer;
    public sealed record Minted(string Name) : LinetypeAnswer;
}

public static class Linetypes {
    public static Fin<DraftReceipt> Commit(DocumentSession session, LinetypeTransaction plan) {
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

    public static Fin<LinetypeAnswer> Ask(DocumentSession session, LinetypeAsk request) {
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

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]           | [OWNER]          | [FORM]                                                  | [ENTRY]                  |
| :-----: | :------------------ | :--------------- | :------------------------------------------------------- | :------------------------ |
|  [01]   | segment atom        | `SegmentRow`     | typed dash/gap pair, signed `.lin` projection            | `StrokeDef` / `Signed`    |
|  [02]   | embedded shapes     | `ShapeRow`       | curve/text glyph union over `AddShape`                   | `StrokeDef.Apply`         |
|  [03]   | definition model    | `StrokeDef`      | segments + shapes + taper + stroke config as one value   | `LinetypeOp.Author`       |
|  [04]   | linetype mutations  | `LinetypeOp`     | add-then-shape, revert, defaults, `.lin` import, tags    | `Linetypes.Commit`        |
|  [05]   | linetype reads      | `LinetypeAsk`    | definition snapshot, table state, object resolution     | `Linetypes.Ask`           |

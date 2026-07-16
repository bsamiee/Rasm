# [RASM_RHINO_ANNOTATION_TEXT]

Text-annotation rail (`Rasm.Rhino.Annotation`). One `TextSeed` union carries plain and RTF content into `TextEntity` and `Leader` construction; `RunEdit` owns live rich-text mutation, while `RunStyle` carries the smaller formatting delta `FormatRtfString` actually accepts. Text-to-geometry outlining is one `OutlineSpec` over four egress forms crossed with grouping and small-caps metrics, covering sixteen modes through eight maximal overloads and named arguments because grouped solid/shell members transpose `smallCapsScale` before `height` and the base member spells `CreatePolySurfaces` where its grouped sibling spells `CreatePolysurfacesGrouped`. `FieldExpr` carries all twenty-nine catalogued `TextFields` evaluators as typed cases; `FieldProgram` preserves positional optional arguments, quotes tokens, and resolves exclusively through document-explicit `TextFields.TryFormat`. Per-annotation restyling composes `StylePatch.Overlay`. Block-attribute extraction remains outside this owner because the current Blocks seam publishes no corresponding boundary.

## [01]-[INDEX]

- [02]-[CONTENT_MODEL]: `TextSeed`, `TextSpec`, `LeaderSpec`, live `RunEdit`, and detached `RunStyle`.
- [03]-[FIELD_FORMULAS]: `CoordAxis`, `FieldExpr`, `TextRun`, and the `FieldProgram` compose/evaluate pair.
- [04]-[OUTLINING]: `GlyphMetrics`, `Grouping`, `OutlineForm`, `OutlineSpec`, and the typed `OutlineProduct`.
- [05]-[TEXT_RAIL]: `TextOp`, `TextTransaction`, the `Texts` entry pair, and the `TextState` snapshot with doc-object projections.
- [06]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[CONTENT_MODEL]

- Owner: `TextSeed` `[Union]` — `Plain` over validated text, `Rich` over an RTF source — the content discriminant `TextEntity.Create`/`CreateWithRichText` and `Leader.Create`/`CreateWithRichText` dispatch on; `TextSpec` — text-frame construction: seed, wrap width, in-plane rotation; `LeaderSpec` — leader construction: seed plus an ordered point run of at least two points; `RunEdit` `[Union]` — live span replacement, whole-content formatting, facename assignment, and positive-width re-wrap; `RunStyle` — detached RTF formatting deltas only.
- Law: the seed selects the host overload — a caller states content, never picks between `Create` and `CreateWithRichText`; RTF admission is the host parser's, and `PlainTextToRtf` lifts plain text into the run model where a formatting edit demands it.
- Law: live and detached mutations share only the host's real intersection — `RunEdit` reaches live `AnnotationBase` members, and `RunStyle` reaches `FormatRtfString`'s formatting-only clear/set arguments. `Replace` and `Wrap` never masquerade as detached RTF operations.
- Law: `SetBold`/`SetItalic`/`SetUnderline`/`SetFacename` are whole-content operations — the host exposes no run-range formatting member, so the only span-scoped mutation is `Replace` over run indices from `GetPlainTextWithRunMap`; a fence claiming range-scoped bold is a phantom.
- Boundary: `Wrap` re-flows to `FormatWidth`; wrap state reads back through `TextIsWrapped`/`FormatWidth`/`TextModelWidth` on the snapshot, and DPI or viewport scaling never enters the run model.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextSeed {
    private TextSeed() { }
    public sealed record Plain(string Value) : TextSeed;
    public sealed record Rich(string Rtf) : TextSeed;

    public static Fin<TextSeed> Of(string text) =>
        Op.Of(name: nameof(TextSeed)).AcceptText(value: text).Map(static valid => (TextSeed)new Plain(Value: valid));

    public static Fin<TextSeed> OfRtf(string rtf) =>
        Op.Of(name: nameof(TextSeed)).AcceptText(value: rtf).Map(static valid => (TextSeed)new Rich(Rtf: valid));
}

public sealed record TextSpec(TextSeed Seed, Option<double> WrapWidth, double RotationRadians = 0.0) {
    public static Fin<TextSpec> Of(TextSeed seed, Option<double> wrapWidth = default, double rotationRadians = 0.0, Op? key = null) {
        Op op = key.OrDefault();
        return from content in Optional(seed).ToFin(Fail: op.InvalidInput())
               from _ in wrapWidth.Match(Some: width => op.Positive(value: width).Map(static _ => unit), None: () => Fin.Succ(value: unit))
               from __ in op.AcceptInput(value: rotationRadians)
               select new TextSpec(Seed: content, WrapWidth: wrapWidth, RotationRadians: rotationRadians);
    }

    internal Fin<TextEntity> Mint(Plane plane, DimensionStyle style, Op key) =>
        key.Catch(() => Optional(Seed.Switch(
                state: (Plane: plane, Style: style, Wrapped: WrapWidth.IsSome, Width: WrapWidth.IfNone(noneValue: 0.0), Rotation: RotationRadians),
                plain: static (ctx, seed) => TextEntity.Create(
                    text: seed.Value, plane: ctx.Plane, style: ctx.Style,
                    wrapped: ctx.Wrapped, rectWidth: ctx.Width, rotationRadians: ctx.Rotation),
                rich: static (ctx, seed) => TextEntity.CreateWithRichText(
                    richTextString: seed.Rtf, plane: ctx.Plane, style: ctx.Style,
                    wrapped: ctx.Wrapped, rectWidth: ctx.Width, rotationRadians: ctx.Rotation)))
            .ToFin(Fail: key.InvalidResult()));
}

public sealed record LeaderSpec(TextSeed Seed, Seq<Point3d> Points) {
    public static Fin<LeaderSpec> Of(TextSeed seed, params ReadOnlySpan<Point3d> points) {
        Op op = Op.Of(name: nameof(LeaderSpec));
        return from content in Optional(seed).ToFin(Fail: op.InvalidInput())
               from run in toSeq(points.ToArray()).TraverseM(point => op.AcceptInput(value: point)).As()
               from _ in guard(run.Count >= 2, op.InvalidInput()).ToFin()
               select new LeaderSpec(Seed: content, Points: run);
    }

    internal Fin<Leader> Mint(Plane plane, DimensionStyle style, Op key) {
        LeaderSpec self = this;
        return key.Catch(() => Optional(self.Seed.Switch(
                state: (Plane: plane, Style: style, Points: self.Points.ToArray()),
                plain: static (ctx, seed) => Leader.Create(text: seed.Value, plane: ctx.Plane, dimstyle: ctx.Style, points: ctx.Points),
                rich: static (ctx, seed) => Leader.CreateWithRichText(richText: seed.Rtf, plane: ctx.Plane, dimstyle: ctx.Style, points: ctx.Points)))
            .ToFin(Fail: key.InvalidResult()));
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RunEdit {
    private RunEdit() { }
    public sealed record Replace(string Text, int StartRun, int StartPosition, int EndRun, int EndPosition) : RunEdit;
    public sealed record Bold(bool On) : RunEdit;
    public sealed record Italic(bool On) : RunEdit;
    public sealed record Underline(bool On) : RunEdit;
    public sealed record Facename(bool On, string Face) : RunEdit;
    public sealed record Wrap(double Width) : RunEdit;

    internal Fin<Unit> Apply(AnnotationBase annotation, Op key) =>
        Switch(
            state: (Annotation: annotation, Op: key),
            replace: static (ctx, edit) =>
                from _ in guard(edit.StartRun >= 0 && edit.StartPosition >= 0 && edit.EndRun >= 0 && edit.EndPosition >= 0, ctx.Op.InvalidInput()).ToFin()
                from replaced in ctx.Op.Confirm(success: ctx.Annotation.RunReplace(
                    replaceString: edit.Text, startRunIndex: edit.StartRun, startRunPosition: edit.StartPosition,
                    endRunIndex: edit.EndRun, endRunPosition: edit.EndPosition))
                select replaced,
            bold: static (ctx, edit) => ctx.Op.Confirm(success: ctx.Annotation.SetBold(setOn: edit.On)),
            italic: static (ctx, edit) => ctx.Op.Confirm(success: ctx.Annotation.SetItalic(setOn: edit.On)),
            underline: static (ctx, edit) => ctx.Op.Confirm(success: ctx.Annotation.SetUnderline(setOn: edit.On)),
            facename: static (ctx, edit) =>
                from face in ctx.Op.AcceptText(value: edit.Face)
                from changed in ctx.Op.Confirm(success: ctx.Annotation.SetFacename(setOn: edit.On, facename: face))
                select changed,
            wrap: static (ctx, edit) =>
                from width in ctx.Op.Positive(value: edit.Width)
                from wrapped in ctx.Op.Catch(() => {
                    ctx.Annotation.FormatWidth = width;
                    ctx.Annotation.WrapText();
                    return Fin.Succ(value: unit);
                })
                select wrapped);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public readonly record struct RunFace(bool Bold, bool Italic, bool Underline, string Face);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FaceDelta {
    private FaceDelta() { }
    public sealed record Clear : FaceDelta;
    public sealed record Set(string Name) : FaceDelta;
}

public sealed record RunStyle(
    Option<bool> Bold = default,
    Option<bool> Italic = default,
    Option<bool> Underline = default,
    Option<FaceDelta> Face = default);

public static class TextRtf {
    public static Fin<string> FromPlain(string text) =>
        Op.Of().AcceptText(value: text).Map(static valid => AnnotationBase.PlainTextToRtf(str: valid));

    public static Fin<string> Restyled(string rtf, RunStyle delta) {
        Op op = Op.Of();
        return from source in op.AcceptText(value: rtf)
               from edit in Optional(delta).ToFin(Fail: op.InvalidInput())
               from _ in guard(edit.Bold.IsSome || edit.Italic.IsSome || edit.Underline.IsSome || edit.Face.IsSome, op.InvalidInput()).ToFin()
               from face in edit.Face.Match(
                   Some: value => value.Switch(
                       clear: static _ => Fin.Succ(value: Some<FaceDelta>(new FaceDelta.Clear())),
                       set: change => op.AcceptText(value: change.Name)
                           .Map(name => Some<FaceDelta>(new FaceDelta.Set(Name: name)))),
                   None: () => Fin.Succ(Option<FaceDelta>.None))
               let faceArgs = face.Match(
                   Some: static value => value.Switch(
                       clear: static _ => (Clear: true, Set: false, Name: string.Empty),
                       set: static change => (Clear: false, Set: true, Name: change.Name)),
                   None: static () => (Clear: false, Set: false, Name: string.Empty))
               from formatted in op.Catch(() => Fin.Succ(value: AnnotationBase.FormatRtfString(
                   rtfIn: source,
                   clearBold: edit.Bold == Some(false), setBold: edit.Bold == Some(true),
                   clearItalic: edit.Italic == Some(false), setItalic: edit.Italic == Some(true),
                   clearUnderline: edit.Underline == Some(false), setUnderline: edit.Underline == Some(true),
                   clearFacename: faceArgs.Clear,
                   setFacename: faceArgs.Set,
                   facename: faceArgs.Name)))
               select formatted;
    }

    public static Fin<RunFace> Probe(string rtf) {
        Op op = Op.Of();
        return op.AcceptText(value: rtf).Bind(source => op.Catch(() => {
            (bool bold, bool italic, bool underline, string face) = (false, false, false, string.Empty);
            return AnnotationBase.FirstCharProperties(rtfStr: source, bold: ref bold, italic: ref italic, underline: ref underline, facename: ref face)
                ? Fin.Succ(value: new RunFace(Bold: bold, Italic: italic, Underline: underline, Face: face))
                : Fin.Fail<RunFace>(error: op.InvalidResult());
        }));
    }
}
```

## [03]-[FIELD_FORMULAS]

- Owner: `CoordAxis` `[SmartEnum<string>]` — the axis token vocabulary the coordinate evaluators consume; `FieldExpr` `[Union]` — one typed case per `TextFields` evaluator across the document, object, page, and block families, each projecting `(name, args)` through one `Token` fold; `TextRun` `[Union]` — literal or field segments; `FieldProgram` — the ordered segment run with `Compose` and session-bound `Evaluate`.
- Law: composition is typed, evaluation is host — a program renders to the host token grammar (`%<Name("arg",...)>%`) through `Compose`, preserves empty positional arguments when a later optional is present, escapes quoted content, and resolves only through `TextFields.TryFormat(text, document, out result)` inside the session grant.
- Law: the case set is the verified host roster — the geometry evaluator is `Volume`, no `VolumeAll` member exists, and `TryParse(text, document, out tokens)` is the one token splitter; a formula string concatenated by hand at a call site is the deleted form.
- Law: ids travel typed — object, layer, detail, and block-instance references enter as `Guid` and render as strings only inside `Token`, because the host grammar is stringly and this union is where that boundary stops.
- Boundary: `BlockAttributeText` and `GetInstanceAttributeFields` stay outside this owner; the current Blocks seam publishes no attribute-text boundary. This union carries only block field evaluators (`BlockName`, `BlockInstanceName`, `BlockDescription`, `BlockInstanceCount`, `BlockInsertionCoordinate`).
- Growth: a new host evaluator is one case with its `Token` arm; `Compose`, `Evaluate`, and every consumer read it with zero new surface.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class CoordAxis {
    public static readonly CoordAxis X = new(key: "X");
    public static readonly CoordAxis Y = new(key: "Y");
    public static readonly CoordAxis Z = new(key: "Z");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FieldExpr {
    private FieldExpr() { }
    public sealed record Date(Option<string> Format = default, Option<string> Language = default) : FieldExpr;
    public sealed record DateModified(Option<string> Format = default, Option<string> Language = default) : FieldExpr;
    public sealed record Notes : FieldExpr;
    public sealed record ModelUnits : FieldExpr;
    public sealed record FileName(Option<string> Options = default) : FieldExpr;
    public sealed record DocumentText(string Key) : FieldExpr;
    public sealed record PageNumber : FieldExpr;
    public sealed record NumPages : FieldExpr;
    public sealed record PageName(Option<Guid> Page = default) : FieldExpr;
    public sealed record PaperName : FieldExpr;
    public sealed record PageWidth : FieldExpr;
    public sealed record PageHeight : FieldExpr;
    public sealed record DetailScale(Guid Detail, string ScaleFormat) : FieldExpr;
    public sealed record LayoutUserText(Option<Guid> Layout, string Key) : FieldExpr;
    public sealed record ObjectName(Option<Guid> Target = default) : FieldExpr;
    public sealed record ObjectLayer(Guid Target) : FieldExpr;
    public sealed record LayerName(Guid Layer) : FieldExpr;
    public sealed record ObjectPageName(Guid Target) : FieldExpr;
    public sealed record ObjectPageNumber(Guid Target) : FieldExpr;
    public sealed record PointCoordinate(Guid Point, CoordAxis Axis) : FieldExpr;
    public sealed record UserText(Guid Target, string Key, Option<string> Prompt = default, Option<string> Default = default) : FieldExpr;
    public sealed record Area(Guid Target, Option<string> UnitSystem = default) : FieldExpr;
    public sealed record Volume(Guid Target, Option<string> UnitSystem = default, bool AllowOpen = false) : FieldExpr;
    public sealed record CurveLength(Guid Target, Option<string> UnitSystem = default) : FieldExpr;
    public sealed record BlockName(Guid Instance) : FieldExpr;
    public sealed record BlockInstanceName(Guid Instance) : FieldExpr;
    public sealed record BlockDescription(string NameOrId) : FieldExpr;
    public sealed record BlockInstanceCount(string NameOrId) : FieldExpr;
    public sealed record BlockInsertionCoordinate(Guid Instance, CoordAxis Axis) : FieldExpr;

    internal (string Name, Seq<string> Args) Token() => Switch(
        date: static f => (nameof(Date), Positionals(f.Format, f.Language)),
        dateModified: static f => (nameof(DateModified), Positionals(f.Format, f.Language)),
        notes: static _ => (nameof(Notes), Seq<string>()),
        modelUnits: static _ => (nameof(ModelUnits), Seq<string>()),
        fileName: static f => (nameof(FileName), Positionals(f.Options)),
        documentText: static f => (nameof(DocumentText), Seq(f.Key)),
        pageNumber: static _ => (nameof(PageNumber), Seq<string>()),
        numPages: static _ => (nameof(NumPages), Seq<string>()),
        pageName: static f => (nameof(PageName), Positionals(f.Page.Map(static id => id.ToString("D")))),
        paperName: static _ => (nameof(PaperName), Seq<string>()),
        pageWidth: static _ => (nameof(PageWidth), Seq<string>()),
        pageHeight: static _ => (nameof(PageHeight), Seq<string>()),
        detailScale: static f => (nameof(DetailScale), Seq(f.Detail.ToString("D"), f.ScaleFormat)),
        layoutUserText: static f => (nameof(LayoutUserText), f.Layout.Match(
            Some: id => Seq(id.ToString("D"), f.Key),
            None: () => Seq(f.Key))),
        objectName: static f => (nameof(ObjectName), Positionals(f.Target.Map(static id => id.ToString("D")))),
        objectLayer: static f => (nameof(ObjectLayer), Seq(f.Target.ToString("D"))),
        layerName: static f => (nameof(LayerName), Seq(f.Layer.ToString("D"))),
        objectPageName: static f => (nameof(ObjectPageName), Seq(f.Target.ToString("D"))),
        objectPageNumber: static f => (nameof(ObjectPageNumber), Seq(f.Target.ToString("D"))),
        pointCoordinate: static f => (nameof(PointCoordinate), Seq(f.Point.ToString("D"), f.Axis.Key)),
        userText: static f => (nameof(UserText), Seq(f.Target.ToString("D"), f.Key) + Positionals(f.Prompt, f.Default)),
        area: static f => (nameof(Area), Seq(f.Target.ToString("D")) + Positionals(f.UnitSystem)),
        volume: static f => (nameof(Volume), Seq(f.Target.ToString("D")) + Positionals(f.UnitSystem, f.AllowOpen ? Some("true") : Option<string>.None)),
        curveLength: static f => (nameof(CurveLength), Seq(f.Target.ToString("D")) + Positionals(f.UnitSystem)),
        blockName: static f => (nameof(BlockName), Seq(f.Instance.ToString("D"))),
        blockInstanceName: static f => (nameof(BlockInstanceName), Seq(f.Instance.ToString("D"))),
        blockDescription: static f => (nameof(BlockDescription), Seq(f.NameOrId)),
        blockInstanceCount: static f => (nameof(BlockInstanceCount), Seq(f.NameOrId)),
        blockInsertionCoordinate: static f => (nameof(BlockInsertionCoordinate), Seq(f.Instance.ToString("D"), f.Axis.Key)));

    private static Seq<string> Positionals(params ReadOnlySpan<Option<string>> args) {
        Option<string>[] values = args.ToArray();
        int last = Array.FindLastIndex(values, static value => value.IsSome);
        return last < 0
            ? Seq<string>()
            : toSeq(values[..(last + 1)]).Map(static value => value.IfNone(string.Empty));
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextRun {
    private TextRun() { }
    public sealed record Literal(string Value) : TextRun;
    public sealed record Field(FieldExpr Expr) : TextRun;
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record FieldProgram(Seq<TextRun> Runs) {
    public static Fin<FieldProgram> Of(params ReadOnlySpan<TextRun> runs) {
        Op op = Op.Of(name: nameof(FieldProgram));
        return toSeq(runs.ToArray())
            .TraverseM(run => Optional(run).ToFin(Fail: op.InvalidInput())).As()
            .Bind(admitted => guard(!admitted.IsEmpty, op.InvalidInput()).ToFin().Map(_ => new FieldProgram(Runs: admitted)));
    }

    public string Compose() =>
        Runs.Fold(string.Empty, static (text, run) => text + run.Switch(
            literal: static segment => segment.Value,
            field: static segment => segment.Expr.Token() switch {
                (var name, { IsEmpty: true }) => $"%<{name}()>%",
                var (name, args) => $"%<{name}({string.Join(",", args.Map(static arg => $"\"{Quote(arg)}\""))})>%",
            }));

    private static string Quote(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);

    internal Fin<string> Evaluate(RhinoDoc document, Op key) =>
        key.Catch(() => TextFields.TryFormat(text: Compose(), doc: document, out string result)
            ? Fin.Succ(value: result)
            : Fin.Fail<string>(error: key.InvalidResult()));
}
```

## [04]-[OUTLINING]

- Owner: `GlyphMetrics` — the small-caps scale and inter-glyph spacing pair; `Grouping` `[SmartEnum]` — merged versus per-glyph egress; `OutlineForm` `[Union]` — the four egress forms: `Strokes` (glyph outline curves with the open-stroke grant), `Faces` (planar capped breps), `Solids` (extruded-and-capped breps at a height), `Shells` (glyph extrusions at a height); `OutlineSpec` — form, metrics, grouping as one request; `OutlineProduct` `[Union]` — one typed result case per form, every case carrying per-glyph groups where the merged route lands as one group.
- Law: one request covers sixteen configuration modes — form and grouping select eight maximal host overloads, while metrics supplies `makeSmallCaps`, `smallCapsScale`, and spacing; a caller never names a host member.
- Law: every host call spells named arguments — the grouped solid and shell overloads transpose `smallCapsScale` before `height` relative to their flat siblings, and the flat member spells `CreatePolySurfaces` where the grouped sibling spells `CreatePolysurfacesGrouped`; positional transcription of either family is the trap this law forecloses.
- Law: products detach at the call — outline curves, breps, and extrusions are freshly minted host geometry the caller owns; nothing document-bound rides an `OutlineProduct`.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum]
public sealed partial class Grouping {
    public static readonly Grouping Merged = new();
    public static readonly Grouping PerGlyph = new();
}

public readonly record struct GlyphMetrics(Option<double> SmallCaps, double Spacing) {
    public static GlyphMetrics Default { get; } = new(SmallCaps: None, Spacing: 0.0);

    internal bool MakeSmallCaps => SmallCaps.IsSome;
    internal double Scale => SmallCaps.IfNone(noneValue: 1.0);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OutlineForm {
    private OutlineForm() { }
    public sealed record Strokes(bool AllowOpen) : OutlineForm;
    public sealed record Faces : OutlineForm;
    public sealed record Solids(double Height) : OutlineForm;
    public sealed record Shells(double Height) : OutlineForm;
}

public sealed record OutlineSpec(OutlineForm Form, GlyphMetrics Metrics, Grouping Grouping) {
    internal Fin<OutlineProduct> Apply(TextEntity text, DimensionStyle style, Op key) =>
        from _ in Admit(key)
        from product in Form.Switch(
            state: (Text: text, Style: style, Metrics: Metrics, Grouping: Grouping, Op: key),
            strokes: static (ctx, form) => ctx.Op.Catch(() => Fin.Succ<OutlineProduct>(value: new OutlineProduct.Curves(
                Glyphs: ctx.Grouping == Grouping.PerGlyph
                    ? toSeq(ctx.Text.CreateCurvesGrouped(
                        dimstyle: ctx.Style, allowOpen: form.AllowOpen, makeSmallCaps: ctx.Metrics.MakeSmallCaps,
                        smallCapsScale: ctx.Metrics.Scale, spacing: ctx.Metrics.Spacing)).Map(static glyph => toSeq(glyph))
                    : Seq(toSeq(ctx.Text.CreateCurves(
                        dimstyle: ctx.Style, allowOpen: form.AllowOpen, makeSmallCaps: ctx.Metrics.MakeSmallCaps,
                        smallCapsScale: ctx.Metrics.Scale, spacing: ctx.Metrics.Spacing)))))),
            faces: static (ctx, _) => ctx.Op.Catch(() => Fin.Succ<OutlineProduct>(value: new OutlineProduct.Faces(
                Glyphs: ctx.Grouping == Grouping.PerGlyph
                    ? toSeq(ctx.Text.CreateSurfacesGrouped(
                        dimstyle: ctx.Style, makeSmallCaps: ctx.Metrics.MakeSmallCaps,
                        smallCapsScale: ctx.Metrics.Scale, spacing: ctx.Metrics.Spacing)).Map(static glyph => toSeq(glyph))
                    : Seq(toSeq(ctx.Text.CreateSurfaces(
                        dimstyle: ctx.Style, makeSmallCaps: ctx.Metrics.MakeSmallCaps,
                        smallCapsScale: ctx.Metrics.Scale, spacing: ctx.Metrics.Spacing)))))),
            solids: static (ctx, form) => ctx.Op.Catch(() => Fin.Succ<OutlineProduct>(value: new OutlineProduct.Solids(
                Glyphs: ctx.Grouping == Grouping.PerGlyph
                    ? toSeq(ctx.Text.CreatePolysurfacesGrouped(
                        dimstyle: ctx.Style, makeSmallCaps: ctx.Metrics.MakeSmallCaps,
                        smallCapsScale: ctx.Metrics.Scale, height: form.Height, spacing: ctx.Metrics.Spacing)).Map(static glyph => toSeq(glyph))
                    : Seq(toSeq(ctx.Text.CreatePolySurfaces(
                        dimstyle: ctx.Style, height: form.Height, makeSmallCaps: ctx.Metrics.MakeSmallCaps,
                        smallCapsScale: ctx.Metrics.Scale, spacing: ctx.Metrics.Spacing)))))),
            shells: static (ctx, form) => ctx.Op.Catch(() => Fin.Succ<OutlineProduct>(value: new OutlineProduct.Shells(
                Glyphs: ctx.Grouping == Grouping.PerGlyph
                    ? toSeq(ctx.Text.CreateExtrusionsGrouped(
                        dimstyle: ctx.Style, makeSmallCaps: ctx.Metrics.MakeSmallCaps,
                        smallCapsScale: ctx.Metrics.Scale, height: form.Height, spacing: ctx.Metrics.Spacing)).Map(static glyph => toSeq(glyph))
                    : Seq(toSeq(ctx.Text.CreateExtrusions(
                        dimstyle: ctx.Style, height: form.Height, makeSmallCaps: ctx.Metrics.MakeSmallCaps,
                        smallCapsScale: ctx.Metrics.Scale, spacing: ctx.Metrics.Spacing)))))))
        select product;

    private Fin<Unit> Admit(Op key) =>
        from _ in Optional(Form).ToFin(Fail: key.InvalidInput())
        from __ in Optional(Grouping).ToFin(Fail: key.InvalidInput())
        from ___ in key.AcceptInput(value: Metrics.Spacing)
        from ____ in Metrics.SmallCaps.Match(
            Some: scale => key.Positive(value: scale).Map(static _ => unit),
            None: () => Fin.Succ(value: unit))
        from _____ in Form.Switch(
            strokes: static _ => Fin.Succ(value: unit),
            faces: static _ => Fin.Succ(value: unit),
            solids: form => key.Positive(value: form.Height).Map(static _ => unit),
            shells: form => key.Positive(value: form.Height).Map(static _ => unit))
        select unit;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OutlineProduct : IDetachedDocumentResult {
    private OutlineProduct() { }
    public sealed record Curves(Seq<Seq<Curve>> Glyphs) : OutlineProduct;
    public sealed record Faces(Seq<Seq<Brep>> Glyphs) : OutlineProduct;
    public sealed record Solids(Seq<Seq<Brep>> Glyphs) : OutlineProduct;
    public sealed record Shells(Seq<Seq<Extrusion>> Glyphs) : OutlineProduct;
}
```

## [05]-[TEXT_RAIL]

- Owner: `TextOp` `[Union]` — placement of text and leader geometry, run amendment, wrap re-flow, formula assignment, and per-annotation restyle/unstyle over the style page's patch algebra; `TextTransaction` — the commit plan; `Texts` — the `Commit`/`Ask` entry pair; `TextState` — the one-pass annotation read: content triple, wrap state, font facts, mask facts, override presence, and the doc-object projections `DisplayText` and `HasMeasurableTextFields`.
- Law: live annotation mutation is duplicate-then-`Replace` — the target resolves to one `AnnotationObjectBase`, its geometry duplicates, the change applies to the copy, and `ObjectTable.Replace` lands it inside the bracket; mutating the live geometry in place bypasses the undo record and is the deleted form.
- Law: `Restyle` composes `StylePatch.Overlay` — the override style mints on the duplicate before `Replace`, so per-annotation customization and style-table authoring share one field algebra; `Unstyle` is `ClearPropertyOverrides` on the same kernel.
- Law: a formula lands as source — `Reformula` writes `TextFormula` from `FieldProgram.Compose()` and the host resolves display text on its own schedule; the `Evaluate` ask is the read-side resolution and never mutates.
- Boundary: `Marks.Render`'s `WorldTextCase`/`AnnotationCase` draw this page's geometry through the display pipeline; drawing is the Display rail's, and this rail never names a pipeline member.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextOp {
    private TextOp() { }
    public sealed record PlaceText(TextSpec Spec, Plane Frame, ResourceRef Style, Option<StylePatch> Overrides = default, Option<ObjectAttributes> Attributes = default) : TextOp;
    public sealed record PlaceLeader(LeaderSpec Spec, Plane Frame, ResourceRef Style, Option<StylePatch> Overrides = default, Option<ObjectAttributes> Attributes = default) : TextOp;
    public sealed record Amend(TableTarget Target, Seq<RunEdit> Edits) : TextOp;
    public sealed record Reformula(TableTarget Target, FieldProgram Program) : TextOp;
    public sealed record Restyle(TableTarget Target, StylePatch Patch) : TextOp;
    public sealed record Unstyle(TableTarget Target) : TextOp;

    internal Fin<DraftReceipt> Apply(RhinoDoc document, Op op) =>
        Switch(
            (Document: document, Op: op),
            placeText: static (context, edit) =>
                from style in edit.Style.Resolve(document: context.Document, lens: StyleOp.Lens, key: context.Op)
                from minted in edit.Spec.Mint(plane: edit.Frame, style: style, key: context.Op)
                from _ in edit.Overrides.Match(
                    Some: patch => patch.Overlay(annotation: minted, key: context.Op).Map(static _ => unit),
                    None: () => Fin.Succ(value: unit))
                from id in Added(document: context.Document, geometry: minted, attributes: edit.Attributes, op: context.Op)
                select DraftReceipt.Objects(slot: DraftSlot.Placed, ids: Seq(id)),
            placeLeader: static (context, edit) =>
                from style in edit.Style.Resolve(document: context.Document, lens: StyleOp.Lens, key: context.Op)
                from minted in edit.Spec.Mint(plane: edit.Frame, style: style, key: context.Op)
                from _ in edit.Overrides.Match(
                    Some: patch => patch.Overlay(annotation: minted, key: context.Op).Map(static _ => unit),
                    None: () => Fin.Succ(value: unit))
                from id in Added(document: context.Document, geometry: minted, attributes: edit.Attributes, op: context.Op)
                select DraftReceipt.Objects(slot: DraftSlot.Placed, ids: Seq(id)),
            amend: static (context, edit) =>
                from _ in guard(!edit.Edits.IsEmpty, context.Op.InvalidInput()).ToFin()
                from receipt in Reworked(document: context.Document, target: edit.Target, op: context.Op, slot: DraftSlot.Amended,
                    change: (annotation, key) => edit.Edits
                        .TraverseM(run => Optional(run).ToFin(Fail: key.InvalidInput())
                            .Bind(active => active.Apply(annotation: annotation, key: key))).As().Map(static _ => unit))
                select receipt,
            reformula: static (context, edit) =>
                Reworked(document: context.Document, target: edit.Target, op: context.Op, slot: DraftSlot.Reformulated,
                    change: (annotation, key) => key.Catch(() => {
                        annotation.TextFormula = edit.Program.Compose();
                        return Fin.Succ(value: unit);
                    })),
            restyle: static (context, edit) =>
                Reworked(document: context.Document, target: edit.Target, op: context.Op, slot: DraftSlot.Restyled,
                    change: (annotation, key) => edit.Patch.Overlay(annotation: annotation, key: key).Map(static _ => unit)),
            unstyle: static (context, edit) =>
                Reworked(document: context.Document, target: edit.Target, op: context.Op, slot: DraftSlot.Restyled,
                    change: static (annotation, key) => key.Confirm(success: annotation.ClearPropertyOverrides())));

    private static Fin<Guid> Added(RhinoDoc document, AnnotationBase geometry, Option<ObjectAttributes> attributes, Op op) =>
        op.Catch(() => document.Objects.Add(
                geometry: geometry,
                attributes: attributes.IfNoneUnsafe((ObjectAttributes?)null),
                history: null,
                reference: false) is var id && id != Guid.Empty
            ? Fin.Succ(value: id)
            : Fin.Fail<Guid>(error: op.InvalidResult()));

    internal static Fin<DraftReceipt> Reworked(
        RhinoDoc document, TableTarget target, Op op, DraftSlot slot,
        Func<AnnotationBase, Op, Fin<Unit>> change) =>
        from ids in target.Resolve(document: document, key: op)
        from amended in ids.TraverseM(id =>
            from native in Optional(document.Objects.FindId(id)).ToFin(Fail: op.MissingContext())
            from source in Optional((native as AnnotationObjectBase)?.AnnotationGeometry).ToFin(Fail: op.InvalidInput())
            from copy in Optional(source.Duplicate() as AnnotationBase).ToFin(Fail: op.InvalidResult())
            from _ in change(copy, op)
            from __ in op.Confirm(success: document.Objects.Replace(objectId: id, geometry: copy, ignoreModes: false))
            select id).As()
        select DraftReceipt.Objects(slot: slot, ids: amended);
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record TextTransaction(string Name, Seq<TextOp> Operations, RedrawPolicy Redraw, bool UndoRecorded = true) {
    public static TextTransaction Batch(string name, params ReadOnlySpan<TextOp> operations) =>
        new(Name: name, Operations: toSeq(operations.ToArray()), Redraw: RedrawPolicy.Deferred);
}

public sealed record TextState(
    Guid Key,
    AnnotationType Kind,
    string PlainText,
    string PlainTextWithFields,
    string RichText,
    Option<string> Formula,
    bool Wrapped,
    double FormatWidth,
    double ModelWidth,
    double TextHeight,
    double RotationRadians,
    string FontFace,
    string FirstCharFace,
    int FontIndex,
    bool HasRtfFormatting,
    bool MaskEnabled,
    PerceptualColor MaskColor,
    DimensionStyle.MaskType MaskColorSource,
    DimensionStyle.MaskFrame MaskFrame,
    double MaskOffset,
    bool MaskUsesViewportColor,
    bool DrawTextFrame,
    bool HasPropertyOverrides,
    string DisplayText,
    bool HasMeasurableTextFields) : IDetachedDocumentResult {
    internal static Fin<TextState> Of(AnnotationObjectBase native, Op key) =>
        from annotation in Optional(native.AnnotationGeometry).ToFin(Fail: key.InvalidResult())
        from mask in key.Catch(() => PerceptualColor.OfRgb(
            red: annotation.MaskColor.R,
            green: annotation.MaskColor.G,
            blue: annotation.MaskColor.B,
            alpha: annotation.MaskColor.A / 255.0,
            key: key))
        from state in key.Catch(() => Fin.Succ(value: new TextState(
            Key: native.Id,
            Kind: annotation.AnnotationType,
            PlainText: annotation.PlainText,
            PlainTextWithFields: annotation.PlainTextWithFields,
            RichText: annotation.RichText,
            Formula: Optional(annotation.TextFormula).Filter(static formula => formula.Length > 0),
            Wrapped: annotation.TextIsWrapped,
            FormatWidth: annotation.FormatWidth,
            ModelWidth: annotation.TextModelWidth,
            TextHeight: annotation.TextHeight,
            RotationRadians: annotation.TextRotationRadians,
            FontFace: annotation.Font.FaceName,
            FirstCharFace: annotation.FirstCharFont.FaceName,
            FontIndex: annotation.FontIndex,
            HasRtfFormatting: annotation.TextHasRtfFormatting,
            MaskEnabled: annotation.MaskEnabled,
            MaskColor: mask,
            MaskColorSource: annotation.MaskColorSource,
            MaskFrame: annotation.MaskFrame,
            MaskOffset: annotation.MaskOffset,
            MaskUsesViewportColor: annotation.MaskUsesViewportColor,
            DrawTextFrame: annotation.DrawTextFrame,
            HasPropertyOverrides: annotation.HasPropertyOverrides,
            DisplayText: native.DisplayText,
            HasMeasurableTextFields: native.HasMeasurableTextFields)))
        select state;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextAsk {
    private TextAsk() { }
    public sealed record State(TableTarget Target) : TextAsk;
    public sealed record RunMap(TableTarget Target) : TextAsk;
    public sealed record Evaluate(FieldProgram Program) : TextAsk;
    public sealed record Tokens(string Text) : TextAsk;
    public sealed record Outline(TableTarget Target, OutlineSpec Spec) : TextAsk;
    public sealed record Scale(ResourceRef Style, Viewport.ViewportTarget Viewport) : TextAsk;

    internal Fin<TextAnswer> Answer(RhinoDoc document, Op op) =>
        Switch(
            context: (Document: document, Op: op),
            state: static (ctx, ask) =>
                from native in Single(document: ctx.Document, target: ask.Target, key: ctx.Op)
                from snapshot in TextState.Of(native: native, key: ctx.Op)
                select (TextAnswer)new TextAnswer.State(Snapshot: snapshot),
            runMap: static (ctx, ask) =>
                from native in Single(document: ctx.Document, target: ask.Target, key: ctx.Op)
                from annotation in Optional(native.AnnotationGeometry).ToFin(Fail: ctx.Op.InvalidResult())
                from mapped in ctx.Op.Catch(() => {
                    int[] map = [];
                    string text = annotation.GetPlainTextWithRunMap(map: ref map);
                    return Fin.Succ(value: (Text: text, Map: toSeq(map)));
                })
                select (TextAnswer)new TextAnswer.Mapped(Text: mapped.Text, RunMap: mapped.Map),
            evaluate: static (ctx, ask) =>
                from resolved in ask.Program.Evaluate(document: ctx.Document, key: ctx.Op)
                select (TextAnswer)new TextAnswer.Resolved(Text: resolved),
            tokens: static (ctx, ask) =>
                from text in ctx.Op.AcceptText(value: ask.Text)
                from split in ctx.Op.Catch(() => TextFields.TryParse(text: text, doc: ctx.Document, out List<string> result)
                    ? Fin.Succ(value: toSeq(result))
                    : Fin.Fail<Seq<string>>(error: ctx.Op.InvalidResult()))
                select (TextAnswer)new TextAnswer.Split(Tokens: split),
            outline: static (ctx, ask) =>
                from native in Single(document: ctx.Document, target: ask.Target, key: ctx.Op)
                from text in Optional(native.AnnotationGeometry as TextEntity).ToFin(Fail: ctx.Op.InvalidInput())
                from style in Optional(text.DimensionStyle).ToFin(Fail: ctx.Op.MissingContext())
                from product in ask.Spec.Apply(text: text, style: style, key: ctx.Op)
                select (TextAnswer)new TextAnswer.Outlined(Product: product),
            scale: static (ctx, ask) =>
                from style in ask.Style.Resolve(document: ctx.Document, lens: StyleOp.Lens, key: ctx.Op)
                from rows in ask.Viewport.Resolve(document: ctx.Document, key: ctx.Op)
                from row in rows switch { [var only] => Fin.Succ(value: only), _ => Fin.Fail<Viewport.ViewportRef>(error: ctx.Op.InvalidInput()) }
                from factor in ctx.Op.Catch(() => Fin.Succ(value: AnnotationBase.GetDimensionScale(
                    doc: ctx.Document, dimstyle: style, vport: row.Viewport)))
                select (TextAnswer)new TextAnswer.Scaled(Factor: factor));

    internal static Fin<AnnotationObjectBase> Single(RhinoDoc document, TableTarget target, Op key) =>
        from ids in target.Resolve(document: document, key: key)
        from id in ids switch { [Guid only] => Fin.Succ(value: only), _ => Fin.Fail<Guid>(error: key.InvalidInput()) }
        from native in Optional(document.Objects.FindId(id)).ToFin(Fail: key.MissingContext())
        from annotation in Optional(native as AnnotationObjectBase).ToFin(Fail: key.InvalidInput())
        select annotation;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextAnswer : IDetachedDocumentResult {
    private TextAnswer() { }
    public sealed record State(TextState Snapshot) : TextAnswer;
    public sealed record Mapped(string Text, Seq<int> RunMap) : TextAnswer;
    public sealed record Resolved(string Text) : TextAnswer;
    public sealed record Split(Seq<string> Tokens) : TextAnswer;
    public sealed record Outlined(OutlineProduct Product) : TextAnswer;
    public sealed record Scaled(double Factor) : TextAnswer;
}

public static class Texts {
    public static Fin<DraftReceipt> Commit(DocumentSession session, TextTransaction plan) {
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

    public static Fin<TextAnswer> Ask(DocumentSession session, TextAsk request) {
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

| [INDEX] | [CONCERN]        | [OWNER]               | [FORM]                                 | [ENTRY]                           |
| :-----: | :--------------- | :-------------------- | :------------------------------------- | :-------------------------------- |
|  [01]   | content seed     | `TextSeed`             | plain/RTF host-construction choice      | `TextSpec.Mint` / `LeaderSpec.Mint` |
|  [02]   | run mutation     | `RunEdit` / `RunStyle` | live edits / detached formatting delta  | `Apply` / `TextRtf.Restyled`       |
|  [03]   | field formula    | `FieldExpr`            | evaluator cases, one `Token` fold       | `FieldProgram.Compose`             |
|  [04]   | field resolution | `FieldProgram`         | session-bound `TryFormat`               | `TextAsk.Evaluate`                 |
|  [05]   | outlining        | `OutlineSpec`          | form × grouping × metrics               | `TextAsk.Outline`                  |
|  [06]   | mutation rail    | `TextOp`               | one union, duplicate-then-`Replace`      | `Texts.Commit`                     |
|  [07]   | read rail        | `TextAsk`              | state, runs, fields, outlines, scale     | `Texts.Ask`                        |

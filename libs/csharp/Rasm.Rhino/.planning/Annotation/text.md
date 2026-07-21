# [RASM_RHINO_ANNOTATION_TEXT]

`TextSeed`, `TextSpec`, and `LeaderSpec` admit content and placement once; `RunEdit` owns both live and detached formatting; `FieldKind` generates the evaluator space from signature data instead of a mirrored case roster.

`OutlineSpec` crosses form, grouping, and transform in one flat dispatch and returns detached geometry with content identity and bounds; `TextOp` collapses text and leader placement into one mutation rail.

## [01]-[INDEX]

- [02]-[CONTENT_MODEL]: admitted content, placement, run edits, and detached RTF formatting.
- [03]-[FIELD_FORMULAS]: evaluator rows, typed signature data, programs, composition, and evaluation.
- [04]-[OUTLINING]: transform-aware form/group dispatch with detached geometry evidence.
- [05]-[TEXT_RAIL]: placement, mutation, snapshot, query, and shared commit entry.
- [06]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[CONTENT_MODEL]

- Owner: `TextSeed` owns plain-versus-rich source, `TextSpec` owns text creation, `LeaderPath` and `LeaderSpec` share point-run admission across creation and repointing, and `RunEdit` owns replacement, formatting, and wrapping.
- Law: public factories admit every reference row before dispatch; raw strings, numeric widths, angles, ranges, and point runs enter before a host constructor or mutation sees them.
- Law: `RunFormat` is the sole formatting vocabulary for live annotation edits and `FormatRtfString`; no second delta shape reconstructs bold, italic, underline, or face state.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rasm.Rhino.Objects;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rasm.Rhino.Annotation;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class TextFormat {
    public static readonly TextFormat Plain = new(key: 0, usesRichText: false);
    public static readonly TextFormat Rich = new(key: 1, usesRichText: true);
    internal bool UsesRichText { get; }
}

[ComplexValueObject]
public sealed partial class TextSeed {
    public TextFormat Format { get; }
    public string Value { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref TextFormat format, ref string value) {
        value ??= string.Empty;
        if (format is null || string.IsNullOrWhiteSpace(value))
            validationError = new ValidationError("Text format and content are required.");
    }
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public sealed partial class TextWidth {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        if (!double.IsFinite(value) || value <= 0.0) validationError = new ValidationError("Text width must be finite and positive.");
    }
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public sealed partial class TextAngle {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        if (!double.IsFinite(value)) validationError = new ValidationError("Text angle must be finite.");
    }
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public sealed partial class TextValue {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (value is null) validationError = new ValidationError("Text value cannot be null.");
    }
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public sealed partial class FieldSource {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        if (value.Length == 0) validationError = new ValidationError("Field source is required.");
    }
}

[ComplexValueObject]
public sealed partial class TextSpec {
    public TextSeed Seed { get; }
    public Option<TextWidth> WrapWidth { get; }
    public TextAngle Rotation { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref TextSeed seed, ref Option<TextWidth> wrapWidth, ref TextAngle rotation) {
        if (seed is null || rotation is null)
            validationError = new ValidationError("Text creation state is incomplete.");
    }

    internal Fin<TextEntity> Mint(Plane plane, DimensionStyle style, Op key) => key.Catch(() => Optional(
        Seed.Format.UsesRichText
            ? TextEntity.CreateWithRichText(
                richTextString: Seed.Value,
                plane: plane,
                style: style,
                wrapped: WrapWidth.IsSome,
                rectWidth: WrapWidth.Map(static width => width.Value).IfNone(0.0),
                rotationRadians: Rotation.Value)
            : TextEntity.Create(
                text: Seed.Value,
                plane: plane,
                style: style,
                wrapped: WrapWidth.IsSome,
                rectWidth: WrapWidth.Map(static width => width.Value).IfNone(0.0),
                rotationRadians: Rotation.Value))
        .ToFin(Fail: key.InvalidResult()));
}

public sealed record LeaderPath {
    private LeaderPath(Seq<Point3d> points) => Points = points;
    public Seq<Point3d> Points { get; }

    public static Fin<LeaderPath> Of(params ReadOnlySpan<Point3d> points) {
        Op op = Op.Of();
        return from run in LanguageExt.Iterable<Point3d>.FromSpan(points)
                   .ToSeq()
                   .Traverse(point => op.AcceptInput(value: point).ToValidation())
                   .As()
                   .ToFin()
               from _ in guard(run.Count >= 2, op.InvalidInput()).ToFin()
               select new LeaderPath(points: run);
    }
}

public sealed record LeaderSpec {
    private LeaderSpec(TextSeed seed, LeaderPath path) { Seed = seed; Path = path; }
    public TextSeed Seed { get; }
    public LeaderPath Path { get; }

    public static Fin<LeaderSpec> Of(TextSeed seed, params ReadOnlySpan<Point3d> points) {
        Op op = Op.Of();
        return from admitted in op.AcceptInput(value: seed)
               from path in LeaderPath.Of(points)
               select new LeaderSpec(seed: admitted, path: path);
    }

    internal Fin<Leader> Mint(Plane plane, DimensionStyle style, Op key) => key.Catch(() => Optional(
        Seed.Format.UsesRichText
            ? Leader.CreateWithRichText(
                richText: Seed.Value, plane: plane, dimstyle: style, points: Path.Points.ToArray())
            : Leader.Create(
                text: Seed.Value, plane: plane, dimstyle: style, points: Path.Points.ToArray()))
        .ToFin(Fail: key.InvalidResult()));
}

[SmartEnum<int>]
public sealed partial class TextToggle {
    public static readonly TextToggle Off = new(key: 0, enabled: false);
    public static readonly TextToggle On = new(key: 1, enabled: true);
    internal bool Enabled { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FaceDelta {
    private FaceDelta() { }
    public sealed record Clear : FaceDelta;
    public sealed record Set(ResourceName Name) : FaceDelta;

    internal Fin<FaceDelta> Admit(Op key) => Switch(
        state: key,
        clear: static (_, _) => Fin.Succ<FaceDelta>(value: new Clear()),
        set: static (op, change) => op.AcceptInput(value: change.Name)
            .Map(name => (FaceDelta)new Set(Name: name)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RunFormat {
    private RunFormat() { }
    public sealed record Bold(TextToggle Value) : RunFormat;
    public sealed record Italic(TextToggle Value) : RunFormat;
    public sealed record Underline(TextToggle Value) : RunFormat;
    public sealed record Face(FaceDelta Value) : RunFormat;

    internal Fin<RunFormat> Admit(Op key) => Switch(
        state: key,
        bold: static (op, edit) => op.AcceptInput(value: edit.Value)
            .Map(value => (RunFormat)new Bold(Value: value)),
        italic: static (op, edit) => op.AcceptInput(value: edit.Value)
            .Map(value => (RunFormat)new Italic(Value: value)),
        underline: static (op, edit) => op.AcceptInput(value: edit.Value)
            .Map(value => (RunFormat)new Underline(Value: value)),
        face: static (op, edit) => op.AcceptInput(value: edit.Value)
            .Bind(value => value.Admit(key: op))
            .Map(value => (RunFormat)new Face(Value: value)));

    internal Fin<Unit> Apply(AnnotationBase annotation, Op key) => Switch(
        state: (Annotation: annotation, Op: key),
        bold: static (ctx, edit) => ctx.Op.Confirm(success: ctx.Annotation.SetBold(setOn: edit.Value.Enabled)),
        italic: static (ctx, edit) => ctx.Op.Confirm(success: ctx.Annotation.SetItalic(setOn: edit.Value.Enabled)),
        underline: static (ctx, edit) => ctx.Op.Confirm(success: ctx.Annotation.SetUnderline(setOn: edit.Value.Enabled)),
        face: static (ctx, edit) => edit.Value.Switch(
            state: ctx,
            clear: static (state, _) => state.Op.Confirm(success: state.Annotation.SetFacename(setOn: false, facename: string.Empty)),
            set: static (state, change) => state.Op.Confirm(success: state.Annotation.SetFacename(
                setOn: true, facename: change.Name.Value))));
}

[ComplexValueObject]
public sealed partial class RunSpan {
    public int StartRun { get; }
    public int StartPosition { get; }
    public int EndRun { get; }
    public int EndPosition { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int startRun, ref int startPosition, ref int endRun, ref int endPosition) {
        if (startRun < 0 || startPosition < 0 || endRun < startRun || endPosition < 0
            || (endRun == startRun && endPosition < startPosition))
            validationError = new ValidationError("Run span is reversed or negative.");
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RunEdit {
    private RunEdit() { }
    public sealed record Replace(TextValue Text, RunSpan Span) : RunEdit;
    public sealed record Format(Seq<RunFormat> Changes) : RunEdit;
    public sealed record Wrap(TextWidth Width) : RunEdit;

    internal Fin<RunEdit> Admit(Op key) => Switch(
        state: key,
        replace: static (op, edit) =>
            from text in op.AcceptInput(value: edit.Text)
            from span in op.AcceptInput(value: edit.Span)
            select (RunEdit)new Replace(Text: text, Span: span),
        format: static (op, edit) =>
            from changes in edit.Changes.TraverseM(change => op.AcceptInput(value: change)
                .Bind(value => value.Admit(key: op))).As()
            from _ in guard(!changes.IsEmpty, op.InvalidInput()).ToFin()
            select (RunEdit)new Format(Changes: changes),
        wrap: static (op, edit) => op.AcceptInput(value: edit.Width)
            .Map(width => (RunEdit)new Wrap(Width: width)));

    internal Fin<Unit> Apply(AnnotationBase annotation, Op key) => Switch(
        state: (Annotation: annotation, Op: key),
        replace: static (ctx, edit) => ctx.Op.Confirm(success: ctx.Annotation.RunReplace(
            replaceString: edit.Text.Value,
            startRunIndex: edit.Span.StartRun,
            startRunPosition: edit.Span.StartPosition,
            endRunIndex: edit.Span.EndRun,
            endRunPosition: edit.Span.EndPosition)),
        format: static (ctx, edit) =>
            from _ in edit.Changes.TraverseM(change => change.Apply(annotation: ctx.Annotation, key: ctx.Op)).As()
            select unit,
        wrap: static (ctx, edit) => ctx.Op.Catch(() => {
            ctx.Annotation.FormatWidth = edit.Width.Value;
            ctx.Annotation.WrapText();
        }));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class TextRtf {
    private readonly record struct FormatArguments(
        bool ClearBold,
        bool SetBold,
        bool ClearItalic,
        bool SetItalic,
        bool ClearUnderline,
        bool SetUnderline,
        bool ClearFace,
        bool SetFace,
        string Face) {
        internal static FormatArguments Empty { get; } = default(FormatArguments) with { Face = string.Empty };

        internal FormatArguments Apply(RunFormat format) => format.Switch(
            bold: value => this with { ClearBold = !value.Value.Enabled, SetBold = value.Value.Enabled },
            italic: value => this with { ClearItalic = !value.Value.Enabled, SetItalic = value.Value.Enabled },
            underline: value => this with { ClearUnderline = !value.Value.Enabled, SetUnderline = value.Value.Enabled },
            face: value => value.Value.Switch(
                clear: _ => this with { ClearFace = true, SetFace = false, Face = string.Empty },
                set: change => this with { ClearFace = false, SetFace = true, Face = change.Name.Value }));
    }

    public static Fin<string> FromPlain(string text) =>
        Op.Of().AcceptText(value: text).Map(static value => AnnotationBase.PlainTextToRtf(str: value));

    public static Fin<string> Restyled(string rtf, params ReadOnlySpan<RunFormat> formats) {
        Op op = Op.Of();
        return from source in op.AcceptText(value: rtf)
               from admitted in LanguageExt.Iterable<RunFormat>.FromSpan(formats)
                   .ToSeq().TraverseM(format => op.AcceptInput(value: format)
                       .Bind(value => value.Admit(key: op))).As()
               from _ in guard(!admitted.IsEmpty, op.InvalidInput()).ToFin()
               let args = admitted.Fold(FormatArguments.Empty, static (state, format) => state.Apply(format))
               from formatted in op.Catch(() => Fin.Succ(value: AnnotationBase.FormatRtfString(
                   rtfIn: source,
                   clearBold: args.ClearBold, setBold: args.SetBold,
                   clearItalic: args.ClearItalic, setItalic: args.SetItalic,
                   clearUnderline: args.ClearUnderline, setUnderline: args.SetUnderline,
                   clearFacename: args.ClearFace, setFacename: args.SetFace, facename: args.Face)))
               select formatted;
    }
}
```

## [03]-[FIELD_FORMULAS]

- Owner: `FieldKind` is the evaluator table; each row names one exact `TextFields` member and declares every admitted argument signature. `FieldExpr` pairs one row with validated typed values.
- Law: `FieldProgram.Compose` derives evaluator name and argument positions from row data; adding an evaluator is one row, never a union case plus a mirrored switch arm.
- Law: optional positions remain explicit `FieldValue.Absent` values until trailing omissions trim, preserving host positional grammar.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class CoordAxis {
    public static readonly CoordAxis X = new(key: "X");
    public static readonly CoordAxis Y = new(key: "Y");
    public static readonly CoordAxis Z = new(key: "Z");
}

[SmartEnum]
public sealed partial class FieldValueKind {
    public static readonly FieldValueKind Text = new(accepts: static value => value is FieldValue.Text);
    public static readonly FieldValueKind Resource = new(accepts: static value => value is FieldValue.Resource);
    public static readonly FieldValueKind Axis = new(accepts: static value => value is FieldValue.Axis);
    public static readonly FieldValueKind Flag = new(accepts: static value => value is FieldValue.Flag);

    [UseDelegateFromConstructor]
    internal partial bool Accepts(FieldValue value);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FieldValue {
    private FieldValue() { }
    public sealed record Text(TextValue Value) : FieldValue;
    public sealed record Resource(ResourceId Value) : FieldValue;
    public sealed record Axis(CoordAxis Value) : FieldValue;
    public sealed record Flag(TextToggle Value) : FieldValue;
    public sealed record Absent : FieldValue;

    internal Fin<FieldValue> Admit(Op key) => Switch(
        state: key,
        text: static (op, value) => op.AcceptInput(value: value.Value)
            .Map(admitted => (FieldValue)new Text(Value: admitted)),
        resource: static (op, value) => op.AcceptInput(value: value.Value)
            .Map(admitted => (FieldValue)new Resource(Value: admitted)),
        axis: static (op, value) => op.AcceptInput(value: value.Value)
            .Map(admitted => (FieldValue)new Axis(Value: admitted)),
        flag: static (op, value) => op.AcceptInput(value: value.Value)
            .Map(admitted => (FieldValue)new Flag(Value: admitted)),
        absent: static (_, _) => Fin.Succ<FieldValue>(value: new Absent()));

    internal string Render() => Switch(
        text: static value => value.Value.Value,
        resource: static value => value.Value.Value.ToString("D"),
        axis: static value => value.Value.Key,
        flag: static value => value.Value.Enabled ? "true" : "false",
        absent: static _ => string.Empty);
}

public readonly record struct FieldSlot(FieldValueKind Kind, bool Required);

public sealed record FieldSignature(Seq<FieldSlot> Slots) {
    internal bool Accepts(Seq<FieldValue> values) =>
        values.Count <= Slots.Count
        && toSeq(Enumerable.Range(0, Slots.Count)).ForAll(index => index < values.Count
            ? Slots[index].Kind.Accepts(values[index]) || (!Slots[index].Required && values[index] is FieldValue.Absent)
            : !Slots[index].Required);
}

[SmartEnum<string>]
public sealed partial class FieldKind {
    public static readonly FieldKind Date = Row(nameof(TextFields.Date), Sig(Opt(FieldValueKind.Text), Opt(FieldValueKind.Text)));
    public static readonly FieldKind DateModified = Row(nameof(TextFields.DateModified), Sig(Opt(FieldValueKind.Text), Opt(FieldValueKind.Text)));
    public static readonly FieldKind Notes = Row(nameof(TextFields.Notes), Sig());
    public static readonly FieldKind ModelUnits = Row(nameof(TextFields.ModelUnits), Sig());
    public static readonly FieldKind FileName = Row(nameof(TextFields.FileName), Sig(Opt(FieldValueKind.Text)));
    public static readonly FieldKind DocumentText = Row(nameof(TextFields.DocumentText), Sig(Req(FieldValueKind.Text)));
    public static readonly FieldKind PageNumber = Row(nameof(TextFields.PageNumber), Sig());
    public static readonly FieldKind NumPages = Row(nameof(TextFields.NumPages), Sig());
    public static readonly FieldKind PageName = Row(nameof(TextFields.PageName), Sig(Opt(FieldValueKind.Resource)));
    public static readonly FieldKind PaperName = Row(nameof(TextFields.PaperName), Sig());
    public static readonly FieldKind PageWidth = Row(nameof(TextFields.PageWidth), Sig());
    public static readonly FieldKind PageHeight = Row(nameof(TextFields.PageHeight), Sig());
    public static readonly FieldKind DetailScale = Row(nameof(TextFields.DetailScale), Sig(Req(FieldValueKind.Resource), Req(FieldValueKind.Text)));
    public static readonly FieldKind LayoutUserText = Row(nameof(TextFields.LayoutUserText),
        Sig(Req(FieldValueKind.Text)), Sig(Req(FieldValueKind.Resource), Req(FieldValueKind.Text)));
    public static readonly FieldKind ObjectName = Row(nameof(TextFields.ObjectName), Sig(Opt(FieldValueKind.Resource)));
    public static readonly FieldKind ObjectLayer = Row(nameof(TextFields.ObjectLayer), Sig(Req(FieldValueKind.Resource)));
    public static readonly FieldKind LayerName = Row(nameof(TextFields.LayerName), Sig(Req(FieldValueKind.Resource)));
    public static readonly FieldKind ObjectPageName = Row(nameof(TextFields.ObjectPageName), Sig(Req(FieldValueKind.Resource)));
    public static readonly FieldKind ObjectPageNumber = Row(nameof(TextFields.ObjectPageNumber), Sig(Req(FieldValueKind.Resource)));
    public static readonly FieldKind PointCoordinate = Row(nameof(TextFields.PointCoordinate), Sig(Req(FieldValueKind.Resource), Req(FieldValueKind.Axis)));
    public static readonly FieldKind UserText = Row(nameof(TextFields.UserText), Sig(
        Req(FieldValueKind.Resource), Req(FieldValueKind.Text), Opt(FieldValueKind.Text), Opt(FieldValueKind.Text)));
    public static readonly FieldKind Area = Row(nameof(TextFields.Area), Sig(Req(FieldValueKind.Resource), Opt(FieldValueKind.Text)));
    public static readonly FieldKind Volume = Row(nameof(TextFields.Volume), Sig(
        Req(FieldValueKind.Resource), Opt(FieldValueKind.Text), Opt(FieldValueKind.Flag)));
    public static readonly FieldKind CurveLength = Row(nameof(TextFields.CurveLength), Sig(Req(FieldValueKind.Resource), Opt(FieldValueKind.Text)));
    public static readonly FieldKind BlockName = Row(nameof(TextFields.BlockName), Sig(Req(FieldValueKind.Resource)));
    public static readonly FieldKind BlockDescription = Row(nameof(TextFields.BlockDescription), Sig(Req(FieldValueKind.Text)));
    public static readonly FieldKind BlockInstanceCount = Row(nameof(TextFields.BlockInstanceCount), Sig(Req(FieldValueKind.Text)));
    public static readonly FieldKind BlockInsertionCoordinate = Row(nameof(TextFields.BlockInsertionCoordinate), Sig(
        Req(FieldValueKind.Resource), Req(FieldValueKind.Axis)));

    private static FieldKind Row(string name, params FieldSignature[] signatures) =>
        new(key: name, signatures: toSeq(signatures));
    private static FieldSignature Sig(params FieldSlot[] slots) => new(Slots: toSeq(slots));
    private static FieldSlot Req(FieldValueKind kind) => new(Kind: kind, Required: true);
    private static FieldSlot Opt(FieldValueKind kind) => new(Kind: kind, Required: false);

    internal Seq<FieldSignature> Signatures { get; }
    internal bool Accepts(Seq<FieldValue> values) => Signatures.Exists(signature => signature.Accepts(values));
}

public sealed record FieldExpr {
    private FieldExpr(FieldKind kind, Seq<FieldValue> values) { Kind = kind; Values = values; }
    public FieldKind Kind { get; }
    public Seq<FieldValue> Values { get; }

    public static Fin<FieldExpr> Of(FieldKind kind, params ReadOnlySpan<FieldValue> values) {
        Op op = Op.Of();
        return from admittedKind in op.AcceptInput(value: kind)
               from admittedValues in LanguageExt.Iterable<FieldValue>.FromSpan(values)
                   .ToSeq().TraverseM(value => op.AcceptInput(value: value)
                       .Bind(admitted => admitted.Admit(key: op))).As()
               let buffer = admittedValues.ToArray()
               let last = Array.FindLastIndex(buffer, static value => value is not FieldValue.Absent)
               let positional = last < 0 ? Seq<FieldValue>() : toSeq(buffer[..(last + 1)])
               from _ in guard(admittedKind.Accepts(positional), op.InvalidInput()).ToFin()
               select new FieldExpr(kind: admittedKind, values: positional);
    }

    internal (string Name, Seq<string> Args) Token() => (Kind.Key, Values.Map(static value => value.Render()));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextRun {
    private TextRun() { }
    public sealed record Literal(TextValue Value) : TextRun;
    public sealed record Field(FieldExpr Expr) : TextRun;

    internal Fin<TextRun> Admit(Op key) => Switch(
        state: key,
        literal: static (op, run) => op.AcceptInput(value: run.Value)
            .Map(value => (TextRun)new Literal(Value: value)),
        field: static (op, run) => op.AcceptInput(value: run.Expr)
            .Map(value => (TextRun)new Field(Expr: value)));
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record FieldProgram {
    private FieldProgram(Seq<TextRun> runs) => Runs = runs;
    public Seq<TextRun> Runs { get; }

    public static Fin<FieldProgram> Of(params ReadOnlySpan<TextRun> runs) {
        Op op = Op.Of();
        return from admitted in LanguageExt.Iterable<TextRun>.FromSpan(runs)
                   .ToSeq().TraverseM(run => op.AcceptInput(value: run)
                       .Bind(value => value.Admit(key: op))).As()
               from _ in guard(!admitted.IsEmpty, op.InvalidInput()).ToFin()
               select new FieldProgram(runs: admitted);
    }

    public string Compose() => string.Concat(Runs.Map(static run => run.Switch(
        literal: static segment => segment.Value.Value,
        field: static segment => segment.Expr.Token() switch {
            (var name, { IsEmpty: true }) => $"%<{name}()>%",
            var (name, args) => $"%<{name}({string.Join(",", args.Map(static arg => $"\"{Quote(arg)}\""))})>%",
        })));

    internal Fin<string> Evaluate(RhinoDoc document, Op key) => key.Catch(() =>
        TextFields.TryFormat(text: Compose(), doc: document, out string result)
            ? Fin.Succ(value: result)
            : Fin.Fail<string>(error: key.InvalidResult()));

    private static string Quote(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
}
```

## [04]-[OUTLINING]

- Owner: `OutlineSpec` crosses form, grouping, metrics, and text frame; `OutlineProduct` returns one evidence carrier per host geometry family.
- Law: the `(form, grouping)` tuple is one flat exhaustive dispatch over the eight verified overloads; no nested modality branch survives.
- Law: `TextFrame.Model` composes `GetTextTransform` and `Transform`; every output geometry carries `DataCRC(0)` and accurate `GetBoundingBox(true)` evidence.
- Law: `OutlineSpec` admits explicit transforms only when `Transform.IsValid`; host-generated model transforms remain guarded by the terminal `Transform` result.
- Boundary: `OutlineSpec.Apply` owns its duplicated `TextEntity` through one lease spanning every transform and outline exit.
- Boundary: `OutlineProduct` receives native geometry only after strict evidence capture; capture failure releases the complete raw batch, and product disposal releases every transferred item.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class OutlineGrouping {
    public static readonly OutlineGrouping Merged = new(key: 0);
    public static readonly OutlineGrouping PerGlyph = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class CurveClosure {
    public static readonly CurveClosure Closed = new(key: 0, allowOpen: false);
    public static readonly CurveClosure Open = new(key: 1, allowOpen: true);
    internal bool AllowOpen { get; }
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public sealed partial class OutlineHeight {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        if (!double.IsFinite(value) || value <= 0.0) validationError = new ValidationError("Outline height must be finite and positive.");
    }
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public sealed partial class GlyphSpacing {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        if (!double.IsFinite(value)) validationError = new ValidationError("Glyph spacing must be finite.");
    }
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public sealed partial class OutlineScale {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        if (!double.IsFinite(value) || value <= 0.0) validationError = new ValidationError("Outline scale must be finite and positive.");
    }
}

[ComplexValueObject]
public sealed partial class GlyphMetrics {
    public Option<OutlineScale> SmallCaps { get; }
    public GlyphSpacing Spacing { get; }
    internal bool MakeSmallCaps => SmallCaps.IsSome;
    internal double SmallCapsScale => SmallCaps.Map(static value => value.Value).IfNone(1.0);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref Option<OutlineScale> smallCaps, ref GlyphSpacing spacing) {
        if (spacing is null) validationError = new ValidationError("Glyph spacing is required.");
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OutlineForm {
    private OutlineForm() { }
    public sealed record Strokes(CurveClosure Closure) : OutlineForm;
    public sealed record Faces : OutlineForm;
    public sealed record Solids(OutlineHeight Height) : OutlineForm;
    public sealed record Shells(OutlineHeight Height) : OutlineForm;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextFrame {
    private TextFrame() { }
    public sealed record Natural : TextFrame;
    public sealed record Model(OutlineScale Scale) : TextFrame;
    public sealed record Explicit(Transform Transform) : TextFrame;
}

public readonly record struct GeometryEvidence<TGeometry>(TGeometry Geometry, uint Crc, BoundingBox Bounds)
    where TGeometry : GeometryBase {
    internal static Fin<Unit> Release(Seq<TGeometry> geometry, Op key) =>
        geometry.Traverse(value => key.Catch(value.Dispose).ToValidation())
            .As().ToFin().Map(static _ => unit);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OutlineProduct : IDetachedDocumentResult, IDisposable {
    private OutlineProduct() { }
    public sealed record Curves(Seq<Seq<GeometryEvidence<Curve>>> Glyphs) : OutlineProduct;
    public sealed record Faces(Seq<Seq<GeometryEvidence<Brep>>> Glyphs) : OutlineProduct;
    public sealed record Solids(Seq<Seq<GeometryEvidence<Brep>>> Glyphs) : OutlineProduct;
    public sealed record Shells(Seq<Seq<GeometryEvidence<Extrusion>>> Glyphs) : OutlineProduct;

    public void Dispose() => Switch(
        curves: static product => GeometryEvidence<Curve>.Release(
            geometry: product.Glyphs.Bind(static group => group).Map(static row => row.Geometry), key: Op.Of()),
        faces: static product => GeometryEvidence<Brep>.Release(
            geometry: product.Glyphs.Bind(static group => group).Map(static row => row.Geometry), key: Op.Of()),
        solids: static product => GeometryEvidence<Brep>.Release(
            geometry: product.Glyphs.Bind(static group => group).Map(static row => row.Geometry), key: Op.Of()),
        shells: static product => GeometryEvidence<Extrusion>.Release(
            geometry: product.Glyphs.Bind(static group => group).Map(static row => row.Geometry), key: Op.Of()))
        .ThrowIfFail();
}

[ComplexValueObject]
public sealed partial class OutlineSpec {
    public OutlineForm Form { get; }
    public GlyphMetrics Metrics { get; }
    public OutlineGrouping Grouping { get; }
    public TextFrame Frame { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref OutlineForm form, ref GlyphMetrics metrics,
        ref OutlineGrouping grouping, ref TextFrame frame) {
        bool admittedForm = form is OutlineForm.Faces
            or OutlineForm.Strokes { Closure: not null }
            or OutlineForm.Solids { Height: not null }
            or OutlineForm.Shells { Height: not null };
        bool admittedFrame = frame is TextFrame.Natural
            or TextFrame.Model { Scale: not null }
            or TextFrame.Explicit { Transform.IsValid: true };
        if (!admittedForm || metrics is null || grouping is null || !admittedFrame)
            validationError = new ValidationError("Outline request is incomplete.");
    }

    internal Fin<OutlineProduct> Apply(TextEntity source, DimensionStyle style, Op key) =>
        from text in Optional(source.Duplicate() as TextEntity).ToFin(Fail: key.InvalidResult())
        from product in new Lease<TextEntity>.Owned(Value: text).Use(owned =>
            from _ in Frame.Switch(
                natural: static _ => Fin.Succ(value: unit),
                model: frame => key.Catch(() => {
                    Transform transform = owned.GetTextTransform(textscale: frame.Scale.Value, dimstyle: style);
                    return key.Confirm(success: owned.Transform(transform: transform, style: style));
                }),
                @explicit: frame => key.Confirm(success: owned.Transform(transform: frame.Transform, style: style)))
            from shaped in (Form, Grouping) switch {
            (OutlineForm.Strokes form, var grouping) when grouping == OutlineGrouping.Merged => key.Catch(() => Captured(
                geometry: Seq(toSeq(owned.CreateCurves(
                    dimstyle: style, allowOpen: form.Closure.AllowOpen, makeSmallCaps: Metrics.MakeSmallCaps,
                    smallCapsScale: Metrics.SmallCapsScale, spacing: Metrics.Spacing.Value))),
                project: static groups => new OutlineProduct.Curves(Glyphs: groups), key: key)),
            (OutlineForm.Strokes form, _) => key.Catch(() => Captured(
                geometry: toSeq(owned.CreateCurvesGrouped(
                    dimstyle: style, allowOpen: form.Closure.AllowOpen, makeSmallCaps: Metrics.MakeSmallCaps,
                    smallCapsScale: Metrics.SmallCapsScale, spacing: Metrics.Spacing.Value)).Map(static group => toSeq(group)),
                project: static groups => new OutlineProduct.Curves(Glyphs: groups), key: key)),
            (OutlineForm.Faces _, var grouping) when grouping == OutlineGrouping.Merged => key.Catch(() => Captured(
                geometry: Seq(toSeq(owned.CreateSurfaces(
                    dimstyle: style, makeSmallCaps: Metrics.MakeSmallCaps,
                    smallCapsScale: Metrics.SmallCapsScale, spacing: Metrics.Spacing.Value))),
                project: static groups => new OutlineProduct.Faces(Glyphs: groups), key: key)),
            (OutlineForm.Faces _, _) => key.Catch(() => Captured(
                geometry: toSeq(owned.CreateSurfacesGrouped(
                    dimstyle: style, makeSmallCaps: Metrics.MakeSmallCaps,
                    smallCapsScale: Metrics.SmallCapsScale, spacing: Metrics.Spacing.Value)).Map(static group => toSeq(group)),
                project: static groups => new OutlineProduct.Faces(Glyphs: groups), key: key)),
            (OutlineForm.Solids form, var grouping) when grouping == OutlineGrouping.Merged => key.Catch(() => Captured(
                geometry: Seq(toSeq(owned.CreatePolySurfaces(
                    dimstyle: style, height: form.Height.Value, makeSmallCaps: Metrics.MakeSmallCaps,
                    smallCapsScale: Metrics.SmallCapsScale, spacing: Metrics.Spacing.Value))),
                project: static groups => new OutlineProduct.Solids(Glyphs: groups), key: key)),
            (OutlineForm.Solids form, _) => key.Catch(() => Captured(
                geometry: toSeq(owned.CreatePolysurfacesGrouped(
                    dimstyle: style, makeSmallCaps: Metrics.MakeSmallCaps, smallCapsScale: Metrics.SmallCapsScale,
                    height: form.Height.Value, spacing: Metrics.Spacing.Value)).Map(static group => toSeq(group)),
                project: static groups => new OutlineProduct.Solids(Glyphs: groups), key: key)),
            (OutlineForm.Shells form, var grouping) when grouping == OutlineGrouping.Merged => key.Catch(() => Captured(
                geometry: Seq(toSeq(owned.CreateExtrusions(
                    dimstyle: style, height: form.Height.Value, makeSmallCaps: Metrics.MakeSmallCaps,
                    smallCapsScale: Metrics.SmallCapsScale, spacing: Metrics.Spacing.Value))),
                project: static groups => new OutlineProduct.Shells(Glyphs: groups), key: key)),
            (OutlineForm.Shells form, _) => key.Catch(() => Captured(
                geometry: toSeq(owned.CreateExtrusionsGrouped(
                    dimstyle: style, makeSmallCaps: Metrics.MakeSmallCaps, smallCapsScale: Metrics.SmallCapsScale,
                    height: form.Height.Value, spacing: Metrics.Spacing.Value)).Map(static group => toSeq(group)),
                project: static groups => new OutlineProduct.Shells(Glyphs: groups), key: key)),
            _ => Fin.Fail<OutlineProduct>(key.InvalidInput()),
            }
            select shaped)
        select product;

    private static Fin<OutlineProduct> Captured<TGeometry>(
        Seq<Seq<TGeometry>> geometry,
        Func<Seq<Seq<GeometryEvidence<TGeometry>>>, OutlineProduct> project,
        Op key) where TGeometry : GeometryBase {
        Seq<TGeometry> custody = geometry.Bind(static group => group).Strict();
        return key.Catch(() => Fin.Succ(value: project(geometry.Map(group => group.Map(item =>
            new GeometryEvidence<TGeometry>(
                Geometry: item,
                Crc: item.DataCRC(currentRemainder: 0u),
                Bounds: item.GetBoundingBox(accurate: true))).Strict()).Strict())))
            .BindFail(primary => GeometryEvidence<TGeometry>.Release(geometry: custody, key: key).Match(
                Succ: _ => Fin.Fail<OutlineProduct>(error: primary),
                Fail: cleanup => Fin.Fail<OutlineProduct>(error: primary + cleanup)));
    }
}
```

## [05]-[TEXT_RAIL]

- Owner: `AnnotationSeed` closes text and leader placement; `TextOp` owns placement, run amendment, formula assignment, leader repointing, and per-object style overrides; `TextAsk` owns detached content, frame, bounds, style, override, leader, and geometry evidence; native-bearing answer cases own their leases and disposal.
- Law: placement and duplicate-then-replace amendments hold native geometry in one owned lease through override, add, edit, and replace failure.
- Law: formula assignment uses `SetRichText(rtfText, dimstyle)`; snapshot evidence includes first-character underline, style and parent identity, the schema-keyed override census, mirrored annotation-style values, natural bounds, and both leader text alignments.
- Law: the dimension-scale probe carries the Document-owned `ViewportTarget` address and resolves it to one native viewport through `ResolveViewport` inside the session demand immediately before `GetDimensionScale`, so no live `RhinoViewport` handle rides the detached request.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnnotationSeed {
    private AnnotationSeed() { }
    public sealed record Text(TextSpec Spec) : AnnotationSeed;
    public sealed record Leader(LeaderSpec Spec) : AnnotationSeed;

    internal Fin<AnnotationSeed> Admit(Op key) => Switch(
        state: key,
        text: static (op, seed) => op.AcceptInput(value: seed.Spec)
            .Map(spec => (AnnotationSeed)new Text(Spec: spec)),
        leader: static (op, seed) => op.AcceptInput(value: seed.Spec)
            .Map(spec => (AnnotationSeed)new Leader(Spec: spec)));

    internal Fin<AnnotationBase> Mint(Plane plane, DimensionStyle style, Op key) => Switch(
        text: spec => spec.Spec.Mint(plane: plane, style: style, key: key).Map(static minted => (AnnotationBase)minted),
        leader: spec => spec.Spec.Mint(plane: plane, style: style, key: key).Map(static minted => (AnnotationBase)minted));
}

[SmartEnum<int>]
public sealed partial class ObjectResidency {
    public static readonly ObjectResidency Model = new(key: 0, isReference: false);
    public static readonly ObjectResidency Reference = new(key: 1, isReference: true);
    internal bool IsReference { get; }
}

[ComplexValueObject]
public sealed partial class AnnotationPlacement {
    public Plane Frame { get; }
    public ResourceRef Style { get; }
    public Option<StylePatch> Overrides { get; }
    public Option<ObjectAttributes> Attributes { get; }
    public Option<HistoryRecord> History { get; }
    public ObjectResidency Residency { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Plane frame, ref ResourceRef style,
        ref Option<StylePatch> overrides, ref Option<ObjectAttributes> attributes,
        ref Option<HistoryRecord> history, ref ObjectResidency residency) {
        if (!frame.IsValid || style is null || residency is null)
            validationError = new ValidationError("Annotation placement is incomplete.");
    }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextOp {
    private TextOp() { }
    public sealed record Place(AnnotationSeed Seed, AnnotationPlacement Placement) : TextOp;
    public sealed record Amend(TableTarget Target, Seq<RunEdit> Edits) : TextOp;
    public sealed record Reformula(TableTarget Target, FieldProgram Program) : TextOp;
    public sealed record Repoint(TableTarget Target, LeaderPath Path) : TextOp;
    public sealed record Restyle(TableTarget Target, StylePatch Patch) : TextOp;
    public sealed record Unstyle(TableTarget Target) : TextOp;

    internal Fin<TextOp> Admit(Op key) => Switch(
        key,
        place: static (op, edit) =>
            from seed in op.AcceptInput(value: edit.Seed).Bind(value => value.Admit(key: op))
            from placement in op.AcceptInput(value: edit.Placement)
            select (TextOp)new Place(Seed: seed, Placement: placement),
        amend: static (op, edit) =>
            from target in op.AcceptInput(value: edit.Target)
            from edits in edit.Edits.TraverseM(item => op.AcceptInput(value: item)
                .Bind(value => value.Admit(key: op))).As()
            from _ in guard(!edits.IsEmpty, op.InvalidInput()).ToFin()
            select (TextOp)new Amend(Target: target, Edits: edits),
        reformula: static (op, edit) =>
            from target in op.AcceptInput(value: edit.Target)
            from program in op.AcceptInput(value: edit.Program)
            select (TextOp)new Reformula(Target: target, Program: program),
        repoint: static (op, edit) =>
            from target in op.AcceptInput(value: edit.Target)
            from path in op.AcceptInput(value: edit.Path)
            select (TextOp)new Repoint(Target: target, Path: path),
        restyle: static (op, edit) =>
            from target in op.AcceptInput(value: edit.Target)
            from patch in op.AcceptInput(value: edit.Patch)
            select (TextOp)new Restyle(Target: target, Patch: patch),
        unstyle: static (op, edit) => op.AcceptInput(value: edit.Target)
            .Map(target => (TextOp)new Unstyle(Target: target)));

    internal Fin<DraftReceipt> Apply(RhinoDoc document, Op op) => Switch(
        (Document: document, Op: op),
        place: static (ctx, edit) => Placed(document: ctx.Document, edit: edit, op: ctx.Op),
        amend: static (ctx, edit) =>
            from _ in guard(!edit.Edits.IsEmpty, ctx.Op.InvalidInput()).ToFin()
            from receipt in Reworked(
                document: ctx.Document, target: edit.Target, op: ctx.Op, slot: DraftSlot.Amended,
                change: (annotation, key) =>
                    edit.Edits.TraverseM(item => item.Apply(annotation: annotation, key: key)).As().Map(static _ => unit))
            select receipt,
        reformula: static (ctx, edit) => Reworked(
            document: ctx.Document, target: edit.Target, op: ctx.Op, slot: DraftSlot.Reformulated,
            change: (annotation, key) => key.Catch(() =>
                annotation.SetRichText(rtfText: edit.Program.Compose(), dimstyle: annotation.DimensionStyle))),
        repoint: static (ctx, edit) => Reworked(
            document: ctx.Document, target: edit.Target, op: ctx.Op, slot: DraftSlot.Adjusted,
            change: (annotation, key) =>
                from leader in Optional(annotation as Leader).ToFin(Fail: key.InvalidInput())
                from _ in key.Catch(() => leader.Points3D = edit.Path.Points.ToArray())
                select unit),
        restyle: static (ctx, edit) => Reworked(
            document: ctx.Document, target: edit.Target, op: ctx.Op, slot: DraftSlot.Restyled,
            change: (annotation, key) => edit.Patch.Overlay(annotation: annotation, key: key).Map(static _ => unit)),
        unstyle: static (ctx, edit) => Reworked(
            document: ctx.Document, target: edit.Target, op: ctx.Op, slot: DraftSlot.Restyled,
            change: static (annotation, key) => key.Confirm(success: annotation.ClearPropertyOverrides())));

    private static Fin<DraftReceipt> Placed(RhinoDoc document, Place edit, Op op) =>
        from style in edit.Placement.Style.Resolve(document: document, lens: StyleOp.Lens, key: op)
        from geometry in edit.Seed.Mint(plane: edit.Placement.Frame, style: style, key: op)
        from receipt in new Lease<AnnotationBase>.Owned(Value: geometry).Use(owned =>
            from _ in edit.Placement.Overrides.Traverse(patch => patch.Overlay(annotation: owned, key: op)).As()
            from id in Added(document: document, geometry: owned, placement: edit.Placement, op: op)
            from placed in DraftReceipt.Objects(slot: DraftSlot.Placed, ids: Seq(id))
            select placed)
        select receipt;

    private static Fin<ResourceId> Added(
        RhinoDoc document, AnnotationBase geometry, AnnotationPlacement placement, Op op) => op.Catch(() => {
        ObjectAttributes? attributes = placement.Attributes.IfNoneUnsafe((ObjectAttributes?)null);
        HistoryRecord? history = placement.History.IfNoneUnsafe((HistoryRecord?)null);
        return ResourceId.Admit(geometry switch {
            TextEntity text => document.Objects.AddText(
                text: text, attributes: attributes, history: history, reference: placement.Residency.IsReference),
            Leader leader => document.Objects.AddLeader(
                leader: leader, attributes: attributes, history: history, reference: placement.Residency.IsReference),
            _ => Guid.Empty,
        }, op);
    });

    internal static Fin<DraftReceipt> Reworked(
        RhinoDoc document, TableTarget target, Op op, DraftSlot slot,
        Func<AnnotationBase, Op, Fin<Unit>> change) =>
        from ids in target.Resolve(document: document, key: op)
        from amended in ids.TraverseM(id =>
            from native in Optional(document.Objects.FindId(id)).ToFin(Fail: op.MissingContext())
            from source in Optional((native as AnnotationObjectBase)?.AnnotationGeometry).ToFin(Fail: op.InvalidInput())
            from copy in Optional(source.Duplicate() as AnnotationBase).ToFin(Fail: op.InvalidResult())
            from amended in new Lease<AnnotationBase>.Owned(Value: copy).Use(owned =>
                from _ in change(owned, op)
                from __ in op.Confirm(success: document.Objects.Replace(objectId: id, geometry: owned, ignoreModes: false))
                select ResourceId.Create(id))
            select amended).As()
        from receipt in DraftReceipt.Objects(slot: slot, ids: amended)
        select receipt;
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record TextContentState(
    string Plain, string PlainWithFields, string Rich, string Display,
    bool HasRtfFormatting, bool HasMeasurableFields);

public sealed record TextFormatState(
    string Face, string FirstFace,
    bool FirstBold, bool FirstItalic, bool FirstUnderlined,
    bool AllBold, bool AllItalic, bool AllUnderlined,
    double Height, double RotationRadians,
    bool Wrapped, double FormatWidth, double ModelWidth);

public sealed record TextMaskState(
    bool Enabled, PerceptualColor Color,
    DimensionStyle.MaskType Source, DimensionStyle.MaskFrame Frame,
    double Offset, bool UsesViewportColor, bool DrawFrame);

public sealed record TextStyleState(
    ResourceId Style,
    Option<ResourceId> Parent,
    Seq<StyleField> Overridden,
    char DecimalSeparator,
    bool UseKerning,
    double LineSpaceScale,
    double DimensionScale,
    bool DrawForward,
    LengthDisplayRow LengthDisplay,
    LengthDisplayRow AlternateLengthDisplay);

public sealed record TextState(
    ResourceId Key, AnnotationType Kind,
    Plane Frame, BoundingBox Bounds,
    TextContentState Content, TextFormatState Format, TextMaskState Mask,
    TextStyleState Style, bool HasPropertyOverrides) : IDetachedDocumentResult {
    internal static Fin<TextState> Of(AnnotationObjectBase native, Op key) => key.Catch(() =>
        from annotation in Optional(native.AnnotationGeometry).ToFin(Fail: key.InvalidResult())
        from mask in annotation.MaskColor.Admitted(key)
        from lengthDisplay in key.AcceptValidated<LengthDisplayRow>(candidate: (int)annotation.DimensionLengthDisplay)
        from alternateLengthDisplay in key.AcceptValidated<LengthDisplayRow>(candidate: (int)annotation.AlternateDimensionLengthDisplay)
        select new TextState(
            Key: ResourceId.Create(native.Id),
            Kind: annotation.AnnotationType,
            Frame: annotation.Plane,
            Bounds: annotation.GetBoundingBox(xform: Transform.Identity),
            Content: new TextContentState(
                annotation.PlainText, annotation.PlainTextWithFields, annotation.RichText, native.DisplayText,
                annotation.TextHasRtfFormatting, native.HasMeasurableTextFields),
            Format: new TextFormatState(
                annotation.Font.FaceName,
                annotation.FirstCharFont.FaceName,
                annotation.FirstCharFont.Bold,
                annotation.FirstCharFont.Italic,
                annotation.FirstCharUnderlined,
                annotation.IsAllBold(),
                annotation.IsAllItalic(),
                annotation.IsAllUnderlined(),
                annotation.TextHeight,
                annotation.TextRotationRadians,
                annotation.TextIsWrapped,
                annotation.FormatWidth,
                annotation.TextModelWidth),
            Mask: new TextMaskState(
                annotation.MaskEnabled, mask, annotation.MaskColorSource, annotation.MaskFrame,
                annotation.MaskOffset, annotation.MaskUsesViewportColor, annotation.DrawTextFrame),
            Style: new TextStyleState(
                Style: ResourceId.Create(annotation.DimensionStyleId),
                Parent: Optional(annotation.ParentDimensionStyle)
                    .Map(static style => ResourceId.Create(style.Id)),
                Overridden: annotation.HasPropertyOverrides
                    ? toSeq(StyleField.Items).Filter(field => annotation.IsPropertyOverridden(field: field.Host))
                    : Seq<StyleField>(),
                DecimalSeparator: annotation.DecimalSeparator,
                UseKerning: annotation.UseKerning,
                LineSpaceScale: annotation.LineSpaceScale,
                DimensionScale: annotation.DimensionScale,
                DrawForward: annotation.DrawForward,
                LengthDisplay: lengthDisplay,
                AlternateLengthDisplay: alternateLengthDisplay),
            HasPropertyOverrides: annotation.HasPropertyOverrides));
}

public sealed record LeaderFacts(
    ResourceId Key, Seq<Point2d> Points2D, Seq<Point3d> Points3D, Option<Lease<NurbsCurve>> Spline,
    DimensionStyle.ArrowType ArrowType, double ArrowSize, Option<ResourceId> ArrowBlock,
    DimensionStyle.LeaderCurveStyle CurveStyle,
    DimensionStyle.LeaderContentAngleStyle ContentAngleStyle,
    TextHorizontalAlignment HorizontalAlignment,
    TextVerticalAlignment VerticalAlignment,
    bool HasLanding, double LandingLength) : IDetachedDocumentResult, IDisposable {
    public void Dispose() {
        _ = Spline.Iter(static spline => spline.Dispose());
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextAsk {
    private TextAsk() { }
    public sealed record State(TableTarget Target) : TextAsk;
    public sealed record LeaderState(TableTarget Target) : TextAsk;
    public sealed record RunMap(TableTarget Target) : TextAsk;
    public sealed record Evaluate(FieldProgram Program) : TextAsk;
    public sealed record Tokens(FieldSource Text) : TextAsk;
    public sealed record Outline(TableTarget Target, OutlineSpec Spec) : TextAsk;
    public sealed record Pieces(TableTarget Target) : TextAsk;
    public sealed record Scale(ResourceRef Style, ViewportTarget Target) : TextAsk;

    internal Fin<TextAsk> Admit(Op key) => Switch(
        key,
        state: static (op, ask) => op.AcceptInput(value: ask.Target)
            .Map(target => (TextAsk)new State(Target: target)),
        leaderState: static (op, ask) => op.AcceptInput(value: ask.Target)
            .Map(target => (TextAsk)new LeaderState(Target: target)),
        runMap: static (op, ask) => op.AcceptInput(value: ask.Target)
            .Map(target => (TextAsk)new RunMap(Target: target)),
        evaluate: static (op, ask) => op.AcceptInput(value: ask.Program)
            .Map(program => (TextAsk)new Evaluate(Program: program)),
        tokens: static (op, ask) => op.AcceptInput(value: ask.Text)
            .Map(text => (TextAsk)new Tokens(Text: text)),
        outline: static (op, ask) =>
            from target in op.AcceptInput(value: ask.Target)
            from spec in op.AcceptInput(value: ask.Spec)
            select (TextAsk)new Outline(Target: target, Spec: spec),
        pieces: static (op, ask) => op.AcceptInput(value: ask.Target)
            .Map(target => (TextAsk)new Pieces(Target: target)),
        scale: static (op, ask) =>
            from style in op.AcceptInput(value: ask.Style)
            from target in op.AcceptInput(value: ask.Target)
            select (TextAsk)new Scale(Style: style, Target: target));

    internal Fin<TextAnswer> Answer(RhinoDoc document, Op op) => Switch(
        (Document: document, Op: op),
        state: static (ctx, ask) =>
            from native in Single(document: ctx.Document, target: ask.Target, key: ctx.Op)
            from snapshot in TextState.Of(native: native, key: ctx.Op)
            select (TextAnswer)new TextAnswer.State(Snapshot: snapshot),
        leaderState: static (ctx, ask) =>
            from native in Single(document: ctx.Document, target: ask.Target, key: ctx.Op)
            from facts in ctx.Op.Catch(() =>
                from leader in Optional(native.AnnotationGeometry as Leader).ToFin(Fail: ctx.Op.InvalidInput())
                select new LeaderFacts(
                    Key: ResourceId.Create(native.Id),
                    Points2D: toSeq(leader.Points2D),
                    Points3D: toSeq(leader.Points3D),
                    ArrowType: leader.LeaderArrowType,
                    ArrowSize: leader.LeaderArrowSize,
                    ArrowBlock: ResourceId.Maybe(leader.LeaderArrowBlockId),
                    CurveStyle: leader.LeaderCurveStyle,
                    ContentAngleStyle: leader.LeaderContentAngleStyle,
                    HorizontalAlignment: leader.LeaderTextHorizontalAlignment,
                    VerticalAlignment: leader.LeaderTextVerticalAlignment,
                    HasLanding: leader.LeaderHasLanding,
                    LandingLength: leader.LeaderLandingLength,
                    Spline: Optional(leader.Curve).Map(static value =>
                        (Lease<NurbsCurve>)new Lease<NurbsCurve>.Owned(Value: (NurbsCurve)value.Duplicate()))))
            select (TextAnswer)new TextAnswer.LeaderState(Facts: facts),
        runMap: static (ctx, ask) =>
            from native in Single(document: ctx.Document, target: ask.Target, key: ctx.Op)
            from annotation in Optional(native.AnnotationGeometry).ToFin(Fail: ctx.Op.InvalidResult())
            from mapped in ctx.Op.Catch(() => {
                int[] map = [];
                string text = annotation.GetPlainTextWithRunMap(map: ref map);
                return guard(map.Length % 3 == 0, ctx.Op.InvalidResult()).ToFin().Map(_ => (
                    Text: text,
                    Runs: toSeq(map.Chunk(3)).Map(static row => new RunLocation(row[0], row[1], row[2]))));
            })
            select (TextAnswer)new TextAnswer.Mapped(Text: mapped.Text, Runs: mapped.Runs),
        evaluate: static (ctx, ask) =>
            from value in ask.Program.Evaluate(document: ctx.Document, key: ctx.Op)
            select (TextAnswer)new TextAnswer.Resolved(Text: value),
        tokens: static (ctx, ask) =>
            from split in ctx.Op.Catch(() => TextFields.TryParse(text: ask.Text.Value, doc: ctx.Document, out List<string> result)
                ? Fin.Succ(value: toSeq(result))
                : Fin.Fail<Seq<string>>(error: ctx.Op.InvalidResult()))
            select (TextAnswer)new TextAnswer.Split(Tokens: split),
        outline: static (ctx, ask) =>
            from native in Single(document: ctx.Document, target: ask.Target, key: ctx.Op)
            from text in Optional(native.AnnotationGeometry as TextEntity).ToFin(Fail: ctx.Op.InvalidInput())
            from style in Optional(text.DimensionStyle).ToFin(Fail: ctx.Op.MissingContext())
            from product in ask.Spec.Apply(source: text, style: style, key: ctx.Op)
            select (TextAnswer)new TextAnswer.Outlined(Product: product),
        pieces: static (ctx, ask) =>
            from native in Single(document: ctx.Document, target: ask.Target, key: ctx.Op)
            from answer in (native.AnnotationGeometry switch {
                TextEntity text => ctx.Op.Catch(() => Optional(text.Explode()).ToFin(Fail: ctx.Op.InvalidResult()))
                    .Bind(curves => TextAnswer.Pieces.Own(
                        products: toSeq(curves).Map(static curve => (GeometryBase)curve), key: ctx.Op)),
                Leader leader => ctx.Op.Catch(() => Optional(leader.Explode()).ToFin(Fail: ctx.Op.InvalidResult()))
                    .Bind(products => TextAnswer.Pieces.Own(products: toSeq(products), key: ctx.Op)),
                var geometry => Fin.Fail<TextAnswer>(ctx.Op.Unsupported(
                    geometryType: geometry.GetType(), outputType: typeof(GeometryBase))),
            })
            select answer,
        scale: static (ctx, ask) =>
            from style in ask.Style.Resolve(document: ctx.Document, lens: StyleOp.Lens, key: ctx.Op)
            from viewport in ask.Target.ResolveViewport(document: ctx.Document, key: ctx.Op)
            from factor in ctx.Op.Catch(() => Fin.Succ(value: AnnotationBase.GetDimensionScale(
                doc: ctx.Document, dimstyle: style, vport: viewport)))
            select (TextAnswer)new TextAnswer.Scaled(Factor: factor));

    internal static Fin<AnnotationObjectBase> Single(RhinoDoc document, TableTarget target, Op key) =>
        target.Only<AnnotationObjectBase>(document: document, key: key).Map(static row => row.Native);
}

public readonly record struct RunLocation(int Run, int Start, int Length);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextAnswer : IDetachedDocumentResult {
    private TextAnswer() { }
    public sealed record State(TextState Snapshot) : TextAnswer;
    public sealed record LeaderState(LeaderFacts Facts) : TextAnswer, IDisposable {
        public void Dispose() => Facts.Dispose();
    }
    public sealed record Mapped(string Text, Seq<RunLocation> Runs) : TextAnswer;
    public sealed record Resolved(string Text) : TextAnswer;
    public sealed record Split(Seq<string> Tokens) : TextAnswer;
    public sealed record Outlined(OutlineProduct Product) : TextAnswer, IDisposable {
        public void Dispose() => Product.Dispose();
    }
    public sealed record Pieces : TextAnswer, IDisposable {
        private Pieces(Seq<Lease<GeometryBase>> products) => Products = products;

        public Seq<Lease<GeometryBase>> Products { get; }

        internal static Fin<TextAnswer> Own(Seq<GeometryBase> products, Op key) {
            Seq<GeometryBase> custody = products.Strict();
            Seq<GeometryBase> releasable = custody.Filter(static product => product is not null).Strict();
            return guard(releasable.Count == custody.Count, key.InvalidResult()).ToFin()
                .Map(_ => (TextAnswer)new Pieces(products: releasable.Map(static product =>
                    (Lease<GeometryBase>)new Lease<GeometryBase>.Owned(Value: product)).Strict()))
                .BindFail(primary => GeometryEvidence<GeometryBase>.Release(geometry: releasable, key: key).Match(
                    Succ: _ => Fin.Fail<TextAnswer>(error: primary),
                    Fail: cleanup => Fin.Fail<TextAnswer>(error: primary + cleanup)));
        }

        public void Dispose() {
            Op key = Op.Of();
            Products.Traverse(product => key.Catch(product.Dispose).ToValidation()).As().ToFin().ThrowIfFail();
        }
    }
    public sealed record Scaled(double Factor) : TextAnswer;
}

public static class Texts {
    public static Fin<DraftReceipt> Commit(DocumentSession session, DraftPlan<TextOp> plan) =>
        DraftSpine.Commit(session: session, plan: plan,
            apply: static (document, operation, key) => key.AcceptInput(value: operation)
                .Bind(value => value.Admit(key: key))
                .Bind(value => value.Apply(document: document, op: key)),
            op: Op.Of());

    public static Fin<TextAnswer> Ask(DocumentSession session, TextAsk request) {
        Op op = Op.Of();
        return from candidate in op.AcceptInput(value: request)
               from admitted in candidate.Admit(key: op)
               from answer in session.Demand(
                   use: document => admitted.Answer(document: document, op: op), key: op, needs: [SessionNeed.Read])
               select answer;
    }
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]        | [OWNER]          | [FORM]                                                    | [ENTRY]          |
| :-----: | :--------------- | :--------------- | :-------------------------------------------------------- | :--------------- |
|  [01]   | content ingress  | `TextSeed`       | admitted format plus source                               | `Create`         |
|  [02]   | live formatting  | `RunEdit`        | replacement, shared formatting rows, or wrap              | `Apply`          |
|  [03]   | field formulas   | `FieldKind`      | evaluator rows carrying admissible typed signatures       | `FieldExpr.Of`   |
|  [04]   | outline egress   | `OutlineSpec`    | flat form/group dispatch plus transform and evidence       | `Apply`          |
|  [05]   | text mutations   | `TextOp`         | unified placement plus duplicate-then-replace edits        | `Texts.Commit`   |
|  [06]   | text evidence    | `TextAsk`        | owned detached evidence family                              | `Texts.Ask`      |

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

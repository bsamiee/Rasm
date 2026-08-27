# [RASM_RHINO_ANNOTATION_TEXT]

`TextSeed`, `TextSpec`, and `LeaderSpec` admit content and placement once; `RunFormat` carries every decoration edit through the namespace's one `FaceDecoration` roster, live and RTF alike; `FieldKind` generates the evaluator space from signature data instead of a mirrored case roster.

`OutlineSpec` dispatches form and grouping through two closed folds and returns detached geometry with content identity and bounds; `TextOp` collapses text and leader placement into one mutation pipeline.

## [01]-[INDEX]

- [02]-[CONTENT_MODEL]: admitted content, placement, run edits, and detached RTF formatting.
- [03]-[FIELD_FORMULAS]: evaluator rows, typed signature data, programs, composition, and evaluation.
- [04]-[OUTLINING]: transform-aware form and grouping dispatch with detached geometry evidence.
- [05]-[TEXT_PIPELINE]: placement, mutation, snapshot, query, and shared commit entry.
- [06]-[SURFACE_LEDGER]: owner table over every surface above.
- [07]-[RESEARCH]: open questions.

## [02]-[CONTENT_MODEL]

- Owner: `TextSeed` owns plain-versus-rich source, `TextSpec` owns text creation, `LeaderPath` and `LeaderSpec` share point-run admission across creation and repointing, and `RunEdit` owns replacement, formatting, and wrapping.
- Law: every union on this page seals — a private owner constructor closes the case family and a public `Fin`-returning factory is the sole ingress, so a value that exists is admitted and no interior re-validates what it was handed. `Admit` folds re-screened already-constructed cases, which is a second admission authority answering after construction.
- Law: `RunFormat` is the sole formatting vocabulary for live annotation edits and `FormatRtfString` alike; the decoration axis is `Annotation/typeface.md`'s `FaceDecoration`, whose rows carry the host members that write and probe them, so no second delta shape reconstructs bold, italic, or underline state.
- Law: a `RunFormat.Decorate` naming a decoration the host publishes no setter for refuses typed at the write — `FaceDecoration.Mark` is absent on exactly that row, so the refusal is the roster's own declared coverage rather than a guard this page maintains.
- Law: every plural admission on this page ACCUMULATES — point runs, format runs, formula values, program runs, and run edits each report their whole refusal set, so a caller repairs one batch in one pass rather than one member per round trip.
- Law: every scalar owner admits through `ValidityClaim` and refuses with a typed `DraftFault` clause set, so a refused width, angle, or span names the column and the requirement instead of one sentence covering four columns.
- Packages: `Annotation/typeface.md` (`FaceDecoration`); `Document/tables.md` (`ResourceName`); `Document/session.md` (`DraftFault`); `Domain/results` (`ValidityClaim`, `Op` receivers); `Domain/validation` (`CapabilitySet<T>`, `Op.AcceptValidated`); RhinoCommon `TextEntity`/`Leader`/`AnnotationBase` per `.api/api-rhinocommon-annotation.md`; Thinktecture.Runtime.Extensions; LanguageExt.Core.
- Growth: a host formatting member joins as a column on the `FaceDecoration` row that already names its concept; a new run edit is one case with its factory and its arm.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rasm.Rhino.Annotation;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<bool>]
public sealed partial class TextFormat {
    public static readonly TextFormat Plain = new(key: false);
    public static readonly TextFormat Rich = new(key: true);
}

[SmartEnum<bool>]
public sealed partial class TextToggle {
    public static readonly TextToggle Off = new(key: false);
    public static readonly TextToggle On = new(key: true);
}

[ComplexValueObject]
[ValidationError]
public sealed partial class TextSeed {
    public TextFormat Format { get; }
    public string Value { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref TextFormat format, ref string value) {
        value ??= string.Empty;
        string content = value;
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
            (string.IsNullOrWhiteSpace(content), static () => new ValidationClause(string.Join(" | ", new object?[] { nameof(Value) })))));
    }
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError]
public sealed partial class TextWidth {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        double width = value;
        validationError = ValidityClaim.Positive(value: width)
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { nameof(TextWidth), width, "a finite positive wrap width" }));
    }
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError]
public sealed partial class TextAngle {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        double radians = value;
        validationError = ValidityClaim.Finite(value: radians)
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { nameof(TextAngle), radians, "a finite radian rotation" }));
    }
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError]
public sealed partial class TextValue {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = value is not null
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { nameof(TextValue) }));
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError]
public sealed partial class FormulaText {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        string source = value;
        validationError = source.Length > 0
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { nameof(FormulaText) }));
    }
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError]
public sealed partial class TextSpec {
    public TextSeed Seed { get; }
    public Option<TextWidth> WrapWidth { get; }
    public TextAngle Rotation { get; }

    internal Fin<TextEntity> Mint(Plane plane, DimensionStyle style) => Try.lift(() => Optional(
        Seed.Format.Key
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
        .ToFin(Fail: new KernelFault.InvalidResult())).Run().Bind(static inner => inner);
}

public sealed record LeaderPath {
    private LeaderPath(Seq<Point3d> points) => Points = points;
    public Seq<Point3d> Points { get; }

    public static Fin<LeaderPath> Of(params ReadOnlySpan<Point3d> points) {
        return from run in Acceptance.Rows(values: points)
               from _ in guard(ValidityClaim.CountAtLeast(count: run.Count, floor: 2), new KernelFault.InvalidInput())
               select new LeaderPath(points: run);
    }
}

public sealed record LeaderSpec {
    private LeaderSpec(TextSeed seed, LeaderPath path) {
        Seed = seed;
        Path = path;
    }

    public TextSeed Seed { get; }
    public LeaderPath Path { get; }

    public static Fin<LeaderSpec> Of(TextSeed seed, params ReadOnlySpan<Point3d> points) {
        return from admitted in Acceptance.Input(value: seed)
               from path in LeaderPath.Of(points)
               select new LeaderSpec(seed: admitted, path: path);
    }

    internal Fin<Leader> Mint(Plane plane, DimensionStyle style) => Try.lift(() => Optional(
        Seed.Format.Key
            ? Leader.CreateWithRichText(
                richText: Seed.Value, plane: plane, dimstyle: style, points: Path.Points.ToArray())
            : Leader.Create(
                text: Seed.Value, plane: plane, dimstyle: style, points: Path.Points.ToArray()))
        .ToFin(Fail: new KernelFault.InvalidResult())).Run().Bind(static inner => inner);
}

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FaceDelta {
    private FaceDelta() { }
    public sealed record Clear : FaceDelta;
    public sealed record Set(ResourceName Name) : FaceDelta;

    internal Option<string> Named => Switch(
        clear: static _ => Option<string>.None,
        set: static change => Some(change.Name.Value));

    internal Fin<Unit> Apply(AnnotationBase annotation) => Switch(
        state: annotation,
        clear: static (context, _) => Admit.Confirm(
            success: context.SetFacename(setOn: false, facename: string.Empty)),
        set: static (context, change) => Admit.Confirm(
            success: context.SetFacename(setOn: true, facename: change.Name.Value)));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RunFormat {
    private RunFormat() { }
    public sealed record Decorate(FaceDecoration Row, TextToggle Value) : RunFormat;
    public sealed record Face(FaceDelta Value) : RunFormat;

    internal Fin<Unit> Apply(AnnotationBase annotation) => Switch(
        annotation,
        decorate: static (context, edit) =>
            from mark in edit.Row.Mark.ToFin(new KernelFault.Unsupported(
                valueType: typeof(FaceDecoration), OutputType: typeof(Unit)))
            from _ in Admit.Confirm(success: mark(arg1: context, arg2: edit.Value.Key))
            select unit,
        face: static (context, edit) => edit.Value.Apply(annotation: context));
}

[ComplexValueObject]
[ValidationError]
public sealed partial class RunSpan {
    public int StartRun { get; }
    public int StartPosition { get; }
    public int EndRun { get; }
    public int EndPosition { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int startRun, ref int startPosition, ref int endRun, ref int endPosition) {
        (int firstRun, int firstPosition, int lastRun, int lastPosition) = (startRun, startPosition, endRun, endPosition);
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
            (firstRun < 0 || lastRun < 0, () => new ValidationClause(string.Join(" | ", new object?[] { nameof(StartRun), firstRun, "a non-negative run ordinal" }))),
            (firstPosition < 0 || lastPosition < 0, () => new ValidationClause(string.Join(" | ", new object?[] { nameof(StartPosition), firstPosition, "a non-negative run position" }))),
            (lastRun < firstRun || (lastRun == firstRun && lastPosition < firstPosition),
                () => new ValidationClause(string.Join(" | ", new object?[] { nameof(RunSpan), "an end at or after the start" })))));
    }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RunEdit {
    private RunEdit() { }
    private sealed record ReplaceCase(TextValue Text, RunSpan Span) : RunEdit;
    private sealed record FormatCase(Seq<RunFormat> Changes) : RunEdit;
    private sealed record WrapCase(TextWidth Width) : RunEdit;

    public static Fin<RunEdit> Replace(TextValue text, RunSpan span) {
        return (Admit.Need(value: text).ToValidation(), Admit.Need(value: span).ToValidation())
            .Apply(static (value, range) => (RunEdit)new ReplaceCase(Text: value, Span: range)).As().ToFin();
    }

    public static Fin<RunEdit> Format(params ReadOnlySpan<RunFormat> changes) {
        return from admitted in Acceptance.Rows(values: changes)
               from _ in guard(!admitted.IsEmpty, new KernelFault.InvalidInput())
               select (RunEdit)new FormatCase(Changes: admitted);
    }

    public static Fin<RunEdit> Wrap(TextWidth width) =>
        key.OrDefault().Need(value: width).Map(static value => (RunEdit)new WrapCase(Width: value));

    internal Fin<Unit> Apply(AnnotationBase annotation) => Switch(
        annotation,
        replaceCase: static (context, edit) => Admit.Confirm(success: context.RunReplace(
            replaceString: edit.Text.Value,
            startRunIndex: edit.Span.StartRun,
            startRunPosition: edit.Span.StartPosition,
            endRunIndex: edit.Span.EndRun,
            endRunPosition: edit.Span.EndPosition)),
        formatCase: static (context, edit) => edit.Changes
            .TraverseM(change => change.Apply(annotation: context)).As()
            .Map(static _ => unit),
        wrapCase: static (context, edit) => Try.lift(() => Fin.Succ(value: HostEdge.Side(() => {
            context.FormatWidth = edit.Width.Value;
            context.WrapText();
        }))).Run().Bind(static inner => inner));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TextRtf {
    public static Fin<string> FromPlain(string text) =>
        key.OrDefault().AcceptText(value: text).Map(static value => AnnotationBase.PlainTextToRtf(str: value));

    public static Fin<string> Restyled(string rtf, params ReadOnlySpan<RunFormat> formats) {
        return from source in Acceptance.Text(value: rtf)
               from admitted in Acceptance.Rows(values: formats)
               from _ in guard(!admitted.IsEmpty, new KernelFault.InvalidInput())
               let delta = admitted.Fold(
                   (Decorations: HashMap<FaceDecoration, TextToggle>(), Face: Option<FaceDelta>.None),
                   static (state, format) => format.Switch(
                       state: state,
                       decorate: static (carried, edit) => carried with {
                           Decorations = carried.Decorations.AddOrUpdate(edit.Row, edit.Value),
                       },
                       face: static (carried, edit) => carried with { Face = Some(edit.Value) }))
               from formatted in Try.lift(() => Fin.Succ(value: AnnotationBase.FormatRtfString(
                   rtfIn: source,
                   clearBold: Clears(delta.Decorations, FaceDecoration.Bold),
                   setBold: Sets(delta.Decorations, FaceDecoration.Bold),
                   clearItalic: Clears(delta.Decorations, FaceDecoration.Italic),
                   setItalic: Sets(delta.Decorations, FaceDecoration.Italic),
                   clearUnderline: Clears(delta.Decorations, FaceDecoration.Underline),
                   setUnderline: Sets(delta.Decorations, FaceDecoration.Underline),
                   clearFacename: delta.Face.Exists(static change => change.Named.IsNone),
                   setFacename: delta.Face.Exists(static change => change.Named.IsSome),
                   facename: delta.Face.Bind(static change => change.Named).IfNone(string.Empty)))).Run().Bind(static inner => inner)
               select formatted;
    }

    private static bool Clears(HashMap<FaceDecoration, TextToggle> rows, FaceDecoration row) =>
        rows.Find(row).Exists(static value => !value.Key);

    private static bool Sets(HashMap<FaceDecoration, TextToggle> rows, FaceDecoration row) =>
        rows.Find(row).Exists(static value => value.Key);
}
```

## [03]-[FIELD_FORMULAS]

- Owner: `FieldKind` is the evaluator table; each row names one exact `TextFields` member and declares every admitted argument signature. `FieldExpr` pairs one row with validated typed values and `FieldProgram` composes a run of literals and expressions into the host's `%<…>%` grammar.
- Law: `FieldProgram.Compose` derives evaluator name and argument positions from row data; adding an evaluator is one row, never a union case with a mirrored switch arm.
- Law: `FormulaValue` answers its own `FormulaKind` through one generated fold — the type-test predicate roster this page carried held a second authority over the union's own case list, and a case added without its predicate admitted nothing.
- Law: trailing `Absent` positions trim through one right fold, so the host's positional grammar survives without an index sentinel; an interior `Absent` stays, because the host reads position, not count.
- Law: the page/sheet and title-block evaluator rows answer HOST-EVALUATED tokens for facts the kernel `Drawing/sheet` owns — sheet extent, sheet numbering, and the ISO 7200 title-block field roster — so a consumer rendering those facts without the host reaches `SheetSize`, `SheetNumber`, and `TitleField.Read`, and no sheet-size, sheet-number, or title-field vocabulary is minted here.
- Packages: `Domain/validation` (`Op.Accept`, `Op.Need`); `Document/tables.md` (`ResourceId`); RhinoCommon `TextFields` per `.api/api-rhinocommon-annotation.md`.
- Growth: a catalog-proven `TextFields` member is one `FieldKind` row with its signature run; every program, composition, and evaluation gains it without another surface.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class CoordAxis {
    public static readonly CoordAxis X = new(key: "X");
    public static readonly CoordAxis Y = new(key: "Y");
    public static readonly CoordAxis Z = new(key: "Z");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FormulaKind {
    public static readonly FormulaKind Text = new(key: "text");
    public static readonly FormulaKind Resource = new(key: "resource");
    public static readonly FormulaKind Axis = new(key: "axis");
    public static readonly FormulaKind Flag = new(key: "flag");
    public static readonly FormulaKind Absent = new(key: "absent");
}

[SmartEnum<bool>]
public sealed partial class FormulaDemand {
    public static readonly FormulaDemand Optional = new(key: false);
    public static readonly FormulaDemand Required = new(key: true);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FormulaValue {
    private FormulaValue() { }
    public sealed record Text(TextValue Value) : FormulaValue;
    public sealed record Resource(ResourceId Value) : FormulaValue;
    public sealed record Axis(CoordAxis Value) : FormulaValue;
    public sealed record Flag(TextToggle Value) : FormulaValue;
    public sealed record Absent : FormulaValue;

    internal static readonly FormulaValue Nothing = new Absent();

    internal FormulaKind Kind => Switch(
        text: static _ => FormulaKind.Text,
        resource: static _ => FormulaKind.Resource,
        axis: static _ => FormulaKind.Axis,
        flag: static _ => FormulaKind.Flag,
        absent: static _ => FormulaKind.Absent);

    internal string Render() => Switch(
        text: static value => value.Value.Value,
        resource: static value => value.Value.Value.ToString("D"),
        axis: static value => value.Value.Key,
        flag: static value => value.Value.Key ? "true" : "false",
        absent: static _ => string.Empty);
}

public readonly record struct FieldSlot(FormulaKind Kind, FormulaDemand Demand) {
    internal bool Admits(FormulaValue value) =>
        Kind == value.Kind || (!Demand.Key && value.Kind == FormulaKind.Absent);
}

public sealed record FieldSignature(Seq<FieldSlot> Slots) {
    internal bool Accepts(Seq<FormulaValue> values) =>
        values.Count <= Slots.Count
        && Slots.Zip(values + Padding(count: Slots.Count - values.Count))
            .ForAll(static pair => pair.First.Admits(value: pair.Second));

    private static Seq<FormulaValue> Padding(int count) =>
        toSeq(Enumerable.Repeat(FormulaValue.Nothing, count));
}

[SmartEnum<string>]
public sealed partial class FieldKind {
    public static readonly FieldKind Date = Row(nameof(TextFields.Date), Sig(Opt(FormulaKind.Text), Opt(FormulaKind.Text)));
    public static readonly FieldKind DateModified = Row(nameof(TextFields.DateModified), Sig(Opt(FormulaKind.Text), Opt(FormulaKind.Text)));
    public static readonly FieldKind Notes = Row(nameof(TextFields.Notes), Sig());
    public static readonly FieldKind ModelUnits = Row(nameof(TextFields.ModelUnits), Sig());
    public static readonly FieldKind FileName = Row(nameof(TextFields.FileName), Sig(Opt(FormulaKind.Text)));
    public static readonly FieldKind DocumentText = Row(nameof(TextFields.DocumentText), Sig(Req(FormulaKind.Text)));
    public static readonly FieldKind PageNumber = Row(nameof(TextFields.PageNumber), Sig());
    public static readonly FieldKind NumPages = Row(nameof(TextFields.NumPages), Sig());
    public static readonly FieldKind PageName = Row(nameof(TextFields.PageName), Sig(Opt(FormulaKind.Resource)));
    public static readonly FieldKind PaperName = Row(nameof(TextFields.PaperName), Sig());
    public static readonly FieldKind PageWidth = Row(nameof(TextFields.PageWidth), Sig());
    public static readonly FieldKind PageHeight = Row(nameof(TextFields.PageHeight), Sig());
    public static readonly FieldKind DetailScale = Row(nameof(TextFields.DetailScale), Sig(Req(FormulaKind.Resource), Req(FormulaKind.Text)));
    public static readonly FieldKind LayoutUserText = Row(nameof(TextFields.LayoutUserText),
        Sig(Req(FormulaKind.Text)), Sig(Req(FormulaKind.Resource), Req(FormulaKind.Text)));
    public static readonly FieldKind ObjectName = Row(nameof(TextFields.ObjectName), Sig(Opt(FormulaKind.Resource)));
    public static readonly FieldKind ObjectLayer = Row(nameof(TextFields.ObjectLayer), Sig(Req(FormulaKind.Resource)));
    public static readonly FieldKind LayerName = Row(nameof(TextFields.LayerName), Sig(Req(FormulaKind.Resource)));
    public static readonly FieldKind ObjectPageName = Row(nameof(TextFields.ObjectPageName), Sig(Req(FormulaKind.Resource)));
    public static readonly FieldKind ObjectPageNumber = Row(nameof(TextFields.ObjectPageNumber), Sig(Req(FormulaKind.Resource)));
    public static readonly FieldKind PointCoordinate = Row(nameof(TextFields.PointCoordinate), Sig(Req(FormulaKind.Resource), Req(FormulaKind.Axis)));
    public static readonly FieldKind UserText = Row(nameof(TextFields.UserText), Sig(
        Req(FormulaKind.Resource), Req(FormulaKind.Text), Opt(FormulaKind.Text), Opt(FormulaKind.Text)));
    public static readonly FieldKind Area = Row(nameof(TextFields.Area), Sig(Req(FormulaKind.Resource), Opt(FormulaKind.Text)));
    public static readonly FieldKind Volume = Row(nameof(TextFields.Volume), Sig(
        Req(FormulaKind.Resource), Opt(FormulaKind.Text), Opt(FormulaKind.Flag)));
    public static readonly FieldKind CurveLength = Row(nameof(TextFields.CurveLength), Sig(Req(FormulaKind.Resource), Opt(FormulaKind.Text)));
    public static readonly FieldKind BlockName = Row(nameof(TextFields.BlockName), Sig(Req(FormulaKind.Resource)));
    public static readonly FieldKind BlockDescription = Row(nameof(TextFields.BlockDescription), Sig(Req(FormulaKind.Text)));
    public static readonly FieldKind BlockInstanceCount = Row(nameof(TextFields.BlockInstanceCount), Sig(Req(FormulaKind.Text)));
    public static readonly FieldKind BlockInsertionCoordinate = Row(nameof(TextFields.BlockInsertionCoordinate), Sig(
        Req(FormulaKind.Resource), Req(FormulaKind.Axis)));

    private static FieldKind Row(string name, params FieldSignature[] signatures) =>
        new(key: name, signatures: toSeq(signatures));
    private static FieldSignature Sig(params FieldSlot[] slots) => new(Slots: toSeq(slots));
    private static FieldSlot Req(FormulaKind kind) => new(Kind: kind, Demand: FormulaDemand.Required);
    private static FieldSlot Opt(FormulaKind kind) => new(Kind: kind, Demand: FormulaDemand.Optional);

    internal Seq<FieldSignature> Signatures { get; }
    internal bool Accepts(Seq<FormulaValue> values) => Signatures.Exists(signature => signature.Accepts(values));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FieldExpr {
    private FieldExpr(FieldKind kind, Seq<FormulaValue> values) {
        Kind = kind;
        Values = values;
    }

    public FieldKind Kind { get; }
    public Seq<FormulaValue> Values { get; }

    public static Fin<FieldExpr> Of(FieldKind kind, params ReadOnlySpan<FormulaValue> values) {
        return from admittedKind in Admit.Need(value: kind)
               from admitted in Acceptance.Rows(values: values)
               let positional = admitted.FoldBack(
                   Seq<FormulaValue>(),
                   static (tail, value) => tail.IsEmpty && value.Kind == FormulaKind.Absent ? tail : value.Cons(tail))
               from _ in guard(admittedKind.Accepts(positional), new KernelFault.InvalidInput())
               select new FieldExpr(kind: admittedKind, values: positional);
    }

    internal (string Name, Seq<string> Args) Token() => (Kind.Key, Values.Map(static value => value.Render()));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextRun {
    private TextRun() { }
    public sealed record Literal(TextValue Value) : TextRun;
    public sealed record Field(FieldExpr Expr) : TextRun;
}

public sealed record FieldProgram {
    private FieldProgram(Seq<TextRun> runs) => Runs = runs;
    public Seq<TextRun> Runs { get; }

    public static Fin<FieldProgram> Of(params ReadOnlySpan<TextRun> runs) {
        return from admitted in Acceptance.Rows(values: runs)
               from _ in guard(!admitted.IsEmpty, new KernelFault.InvalidInput())
               select new FieldProgram(runs: admitted);
    }

    public string Compose() => string.Concat(Runs.Map(static run => run.Switch(
        literal: static segment => segment.Value.Value,
        field: static segment => segment.Expr.Token() switch {
            (var name, { IsEmpty: true }) => $"%<{name}()>%",
            var (name, args) => $"%<{name}({string.Join(",", args.Map(static arg => $"\"{Quote(arg)}\""))})>%",
        })));

    internal Fin<string> Evaluate(RhinoDoc document) => Try.lift(() =>
        TextFields.TryFormat(text: Compose(), doc: document, out string result)
            ? Fin.Succ(value: result)
            : Fin.Fail<string>(error: new KernelFault.InvalidResult())).Run().Bind(static inner => inner);

    private static string Quote(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
}
```

## [04]-[OUTLINING]

- Owner: `OutlineSpec` crosses form, grouping, metrics, and text frame; `OutlineProduct` returns one evidence carrier per host geometry family.
- Law: dispatch is TWO closed folds — the form union answers which host member pair to reach and the grouping row answers which half of that pair — so the eight verified overloads land as four arms and no tuple pattern needs the catch-all that turns a new form into a silent runtime refusal.
- Law: `TextFrame.Model` composes `GetTextTransform` and `Transform`; every output geometry carries `DataCRC(0)` and accurate `GetBoundingBox(true)` evidence.
- Law: `OutlineSpec` admits an explicit transform only when `Transform.IsValid`; a host-generated model transform stays guarded by the terminal `Transform` result.
- Law: `OutlineProduct` carries its owned natives as a BASE POSITIONAL column and exposes typed `Release(Op)`; cleanup never parks inside a value the caller may not inspect.
- Boundary: `OutlineSpec.Apply` owns its duplicated `TextEntity` through one lease spanning every transform and outline exit.
- Boundary: `OutlineProduct` receives native geometry only after strict evidence capture; capture failure and product release both settle through kernel `Custody`, so no outline path spells a cleanup fold of its own.
- Packages: `Domain/results` (`Custody`, `Lease<T>`, `ValidityClaim`); RhinoCommon `TextEntity` outline members per `.api/api-rhinocommon-annotation.md`.
- Growth: a new outline family is one `OutlineForm` case, one arm naming its host pair, and one `OutlineProduct` case; the grouping fold and the evidence capture gain it unchanged.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class OutlineGrouping {
    public static readonly OutlineGrouping Merged = new(key: 0);
    public static readonly OutlineGrouping PerGlyph = new(key: 1);
}

[SmartEnum<bool>]
public sealed partial class CurveClosure {
    public static readonly CurveClosure Closed = new(key: false);
    public static readonly CurveClosure Open = new(key: true);
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError]
public sealed partial class OutlineHeight {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        double height = value;
        validationError = ValidityClaim.Positive(value: height)
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { nameof(OutlineHeight), height, "a finite positive height" }));
    }
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError]
public sealed partial class GlyphSpacing {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        double spacing = value;
        validationError = ValidityClaim.Finite(value: spacing)
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { nameof(GlyphSpacing), spacing, "a finite glyph spacing" }));
    }
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError]
public sealed partial class OutlineScale {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        double scale = value;
        validationError = ValidityClaim.Positive(value: scale)
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { nameof(OutlineScale), scale, "a finite positive scale" }));
    }
}

[ComplexValueObject]
[ValidationError]
public sealed partial class GlyphMetrics {
    public Option<OutlineScale> SmallCaps { get; }
    public GlyphSpacing Spacing { get; }

    internal bool MakeSmallCaps => SmallCaps.IsSome;
    internal double SmallCapsScale => SmallCaps.Map(static value => value.Value).IfNone(1.0);
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

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct GeometryEvidence<TGeometry>(TGeometry Geometry, uint Crc, BoundingBox Bounds)
    where TGeometry : GeometryBase;

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OutlineProduct(Seq<GeometryBase> Owned) : IDetachedDocumentResult {
    public sealed record Curves(
        Seq<Seq<GeometryEvidence<Curve>>> Glyphs, Seq<GeometryBase> Owned) : OutlineProduct(Owned);
    public sealed record Faces(
        Seq<Seq<GeometryEvidence<Brep>>> Glyphs, Seq<GeometryBase> Owned) : OutlineProduct(Owned);
    public sealed record Solids(
        Seq<Seq<GeometryEvidence<Brep>>> Glyphs, Seq<GeometryBase> Owned) : OutlineProduct(Owned);
    public sealed record Shells(
        Seq<Seq<GeometryEvidence<Extrusion>>> Glyphs, Seq<GeometryBase> Owned) : OutlineProduct(Owned);

    public Fin<Unit> Release() => Custody.Dispose(held: Owned);
}

[ComplexValueObject]
[ValidationError]
public sealed partial class OutlineSpec {
    public OutlineForm Form { get; }
    public GlyphMetrics Metrics { get; }
    public OutlineGrouping Grouping { get; }
    public TextFrame Frame { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref OutlineForm form, ref GlyphMetrics metrics, ref OutlineGrouping grouping, ref TextFrame frame) {
        TextFrame requested = frame;
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
            (requested is TextFrame.Explicit { Transform.IsValid: false },
                static () => new ValidationClause(string.Join(" | ", new object?[] { nameof(Frame), "a valid explicit transform" })))));
    }

    internal Fin<OutlineProduct> Apply(TextEntity source, DimensionStyle style) =>
        from copy in Admit.Need(source.Duplicate() as TextEntity)
        from product in new Lease<TextEntity>.Owned(Value: copy).Use(
            body: owned =>
                from _ in Frame.Switch(
                    state: (Text: owned, Style: style),
                    natural: static (_, _) => Fin.Succ(value: unit),
                    model: static (context, frame) => Try.lift(() => Admit.Confirm(
                        success: context.Text.Transform(
                            transform: context.Text.GetTextTransform(textscale: frame.Scale.Value, dimstyle: context.Style),
                            style: context.Style))).Run().Bind(static inner => inner),
                    @explicit: static (context, frame) => Admit.Confirm(
                        success: context.Text.Transform(transform: frame.Transform, style: context.Style)))
                from shaped in Drawn(text: owned, style: style)
                select shaped)
        select product;

    private Fin<OutlineProduct> Drawn(TextEntity text, DimensionStyle style) => Form.Switch(
        state: (Text: text, Style: style, Metrics: Metrics, Grouping: Grouping),
        strokes: static (context, form) => Emit(
            grouping: context.Grouping,
            merged: () => context.Text.CreateCurves(
                dimstyle: context.Style, allowOpen: form.Closure.Key, makeSmallCaps: context.Metrics.MakeSmallCaps,
                smallCapsScale: context.Metrics.SmallCapsScale, spacing: context.Metrics.Spacing.Value),
            grouped: () => context.Text.CreateCurvesGrouped(
                dimstyle: context.Style, allowOpen: form.Closure.Key, makeSmallCaps: context.Metrics.MakeSmallCaps,
                smallCapsScale: context.Metrics.SmallCapsScale, spacing: context.Metrics.Spacing.Value),
            project: static (glyphs, owned) => new OutlineProduct.Curves(glyphs, owned)),
        faces: static (context, _) => Emit(
            grouping: context.Grouping,
            merged: () => context.Text.CreateSurfaces(
                dimstyle: context.Style, makeSmallCaps: context.Metrics.MakeSmallCaps,
                smallCapsScale: context.Metrics.SmallCapsScale, spacing: context.Metrics.Spacing.Value),
            grouped: () => context.Text.CreateSurfacesGrouped(
                dimstyle: context.Style, makeSmallCaps: context.Metrics.MakeSmallCaps,
                smallCapsScale: context.Metrics.SmallCapsScale, spacing: context.Metrics.Spacing.Value),
            project: static (glyphs, owned) => new OutlineProduct.Faces(glyphs, owned)),
        solids: static (context, form) => Emit(
            grouping: context.Grouping,
            merged: () => context.Text.CreatePolySurfaces(
                dimstyle: context.Style, height: form.Height.Value, makeSmallCaps: context.Metrics.MakeSmallCaps,
                smallCapsScale: context.Metrics.SmallCapsScale, spacing: context.Metrics.Spacing.Value),
            grouped: () => context.Text.CreatePolysurfacesGrouped(
                dimstyle: context.Style, makeSmallCaps: context.Metrics.MakeSmallCaps,
                smallCapsScale: context.Metrics.SmallCapsScale, height: form.Height.Value,
                spacing: context.Metrics.Spacing.Value),
            project: static (glyphs, owned) => new OutlineProduct.Solids(glyphs, owned)),
        shells: static (context, form) => Emit(
            grouping: context.Grouping,
            merged: () => context.Text.CreateExtrusions(
                dimstyle: context.Style, height: form.Height.Value, makeSmallCaps: context.Metrics.MakeSmallCaps,
                smallCapsScale: context.Metrics.SmallCapsScale, spacing: context.Metrics.Spacing.Value),
            grouped: () => context.Text.CreateExtrusionsGrouped(
                dimstyle: context.Style, makeSmallCaps: context.Metrics.MakeSmallCaps,
                smallCapsScale: context.Metrics.SmallCapsScale, height: form.Height.Value,
                spacing: context.Metrics.Spacing.Value),
            project: static (glyphs, owned) => new OutlineProduct.Shells(glyphs, owned)));

    private static Fin<OutlineProduct> Emit<TGeometry>(
        OutlineGrouping grouping, Func<TGeometry[]> merged, Func<TGeometry[][]> grouped,
        Func<Seq<Seq<GeometryEvidence<TGeometry>>>, Seq<GeometryBase>, OutlineProduct> project) where TGeometry : GeometryBase =>
        Try.lift(() => Captured(
            geometry: grouping.Switch(
                state: (Merged: merged, Grouped: grouped),
                merged: static pair => Seq(toSeq(pair.Merged())),
                perGlyph: static pair => toSeq(pair.Grouped()).Map(static group => toSeq(group))),
            project: project)).Run().Bind(static inner => inner);

    private static Fin<OutlineProduct> Captured<TGeometry>(
        Seq<Seq<TGeometry>> geometry,
        Func<Seq<Seq<GeometryEvidence<TGeometry>>>, Seq<GeometryBase>, OutlineProduct> project) where TGeometry : GeometryBase {
        Seq<TGeometry> custody = geometry.Bind(static group => group).Strict();
        return Try.lift(() => Fin.Succ(value: project(
                geometry.Map(group => group.Map(item => new GeometryEvidence<TGeometry>(
                    Geometry: item,
                    Crc: item.DataCRC(currentRemainder: 0u),
                    Bounds: item.GetBoundingBox(accurate: true))).Strict()).Strict(),
                custody.Map(static item => (GeometryBase)item)))).Run().Bind(static inner => inner)
            .Rollback(release: () => Custody.Dispose(held: custody, key: key));
    }
}
```

## [05]-[TEXT_PIPELINE]

- Owner: `AnnotationSeed` closes text and leader placement; `TextOp` owns placement, run amendment, formula assignment, leader repointing, and the per-object style verbs; `TextAsk` owns detached content, frame, bounds, style, override, leader, and geometry evidence; a native-bearing answer case carries a lease or a `GeometryHandle` run and settles it through kernel `Custody`.
- Law: the per-annotation style verbs are `Annotation/style.md`'s `AnnotationStyleOp` under ONE `TextOp.Style` case — overlay and clear were two cases spelling one concern, the annotation's relation to its style, and the dimension family spells the same pair against the same owner.
- Law: every host enum crossing a snapshot boundary lands on a bounded owner — `AnnotationKind` for the annotation kind, `MaskSource`/`TextMaskFrame` for the mask pair, `LeaderArrow`/`LeaderCurve`/`LeaderContentAngle`/`TextAlignAcross`/`TextAlignDown` for the leader vocabulary — each admitted through the kernel's host-enum `Op.Row` arm, so an undefined host value is an `InvalidResult` naming the enum rather than an `(int)` cast landing on no row.
- Law: `AnnotationKind` carries the WHOLE `AnnotationType` roster with a `Measures` column, so this page's own text and leader snapshots resolve and a measuring consumer narrows through the column; a vocabulary spanning the dimension rows alone refuses the majority of the objects this operation places.
- Law: snapshot decorations are `CapabilitySet<FaceDecoration>` values off the shared roster — the first-character set reads the resolved `FirstCharFont` and the run-wide set reads the three host `IsAll*` probes, so the roster's own coverage names why the two sets differ instead of six booleans that state nothing.
- Law: `TextMask` is a union, not a flag beside dead columns — a disabled mask carries no colour, source, frame, or offset, and the enabled case carries its traits as one set.
- Law: an override census IS the presence answer — `TextStyleState.Overridden` empty says what a second `HasPropertyOverrides` column said, and two authorities over one host verdict drift the moment either read changes.
- Law: native-bearing answers expose one total `Release(Op)` fold; cleanup faults stay typed on that result and no disposer parks them inside the answer.
- Law: placement and duplicate-then-replace amendments hold native geometry in one owned lease through override, add, edit, and replace failure, and an absent attributes or history payload lowers through `HostEdge.Slot` — the ONE spelling where an absent optional becomes a host `null`, never `ValueUnsafe`, which throws on `None`.
- Law: formula assignment uses `SetRichText(rtfText, dimstyle)`; snapshot evidence reads first-character decoration off `FirstCharFont` because the host publishes no per-annotation decoration member.
- Law: the dimension-scale probe carries the Document-owned `ViewportTarget` address and resolves it to one native viewport through `ResolveViewport` inside the session demand immediately before `GetDimensionScale`, so no live `RhinoViewport` handle rides the detached request.
- Packages: `Annotation/style.md` (`AnnotationStyleOp`, `StylePatch`, `StyleField`, `StyleOp.Lens`, `DraftCrossing`, `DraftPlan`, `DraftSpine`, `LengthDisplayRow`); `Annotation/typeface.md` (`FaceDecoration`); `Document/tables.md` (`TableTarget`, `TargetResolution.Only`, `ResourceId`, `GeometryHandle`); `Document/session.md` (`DocumentSession.Demand`, `SessionNeed`, `DraftFault`); `Domain/results` (`Custody`, `Lease<T>`, `HostEdge.Slot`); `Numerics/atoms` (`PerceptualColor.OfHost`); RhinoCommon `AnnotationObjectBase`/`Leader`/`TextFields` per `.api/api-rhinocommon-annotation.md`.
- Growth: a new text mutation is one `TextOp` case with its factory and arm; a new read is one `TextAsk` case beside its answer case, and the commit and ask entries gain both unchanged.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnnotationSeed {
    private AnnotationSeed() { }
    public sealed record Text(TextSpec Spec) : AnnotationSeed;
    public sealed record Leader(LeaderSpec Spec) : AnnotationSeed;

    internal Fin<AnnotationBase> Mint(Plane plane, DimensionStyle style) => Switch(
        (Frame: plane, Style: style),
        text: static (context, seed) => seed.Spec
            .Mint(plane: context.Frame, style: context.Style)
            .Map(static minted => (AnnotationBase)minted),
        leader: static (context, seed) => seed.Spec
            .Mint(plane: context.Frame, style: context.Style)
            .Map(static minted => (AnnotationBase)minted));
}

[SmartEnum<bool>]
public sealed partial class ObjectResidency {
    public static readonly ObjectResidency Model = new(key: false);
    public static readonly ObjectResidency Reference = new(key: true);
}

[ComplexValueObject]
[ValidationError]
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
        Plane plane = frame;
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
            (!plane.IsValid, static () => new ValidationClause(string.Join(" | ", new object?[] { nameof(Frame), "a valid placement plane" })))));
    }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextOp {
    private TextOp() { }
    private sealed record PlaceCase(AnnotationSeed Seed, AnnotationPlacement Placement) : TextOp;
    private sealed record AmendCase(TableTarget Target, Seq<RunEdit> Edits) : TextOp;
    private sealed record ReformulaCase(TableTarget Target, FieldProgram Program) : TextOp;
    private sealed record RepointCase(TableTarget Target, LeaderPath Path) : TextOp;
    private sealed record StyleCase(TableTarget Target, AnnotationStyleOp Edit) : TextOp;

    public static Fin<TextOp> Place(AnnotationSeed seed, AnnotationPlacement placement) {
        return (Admit.Need(value: seed).ToValidation(), Admit.Need(value: placement).ToValidation())
            .Apply(static (source, where) => (TextOp)new PlaceCase(Seed: source, Placement: where)).As().ToFin();
    }

    public static Fin<TextOp> Amend(TableTarget target, params ReadOnlySpan<RunEdit> edits) {
        return from admitted in Acceptance.Rows(values: edits)
               from _ in guard(!admitted.IsEmpty, new KernelFault.InvalidInput())
               from source in Admit.Need(value: target)
               select (TextOp)new AmendCase(Target: source, Edits: admitted);
    }

    public static Fin<TextOp> Reformula(TableTarget target, FieldProgram program) {
        return (Admit.Need(value: target).ToValidation(), Admit.Need(value: program).ToValidation())
            .Apply(static (source, formula) => (TextOp)new ReformulaCase(Target: source, Program: formula)).As().ToFin();
    }

    public static Fin<TextOp> Repoint(TableTarget target, LeaderPath path) {
        return (Admit.Need(value: target).ToValidation(), Admit.Need(value: path).ToValidation())
            .Apply(static (source, run) => (TextOp)new RepointCase(Target: source, Path: run)).As().ToFin();
    }

    public static Fin<TextOp> Style(TableTarget target, AnnotationStyleOp edit) {
        return (Admit.Need(value: target).ToValidation(), Admit.Need(value: edit).ToValidation())
            .Apply(static (source, style) => (TextOp)new StyleCase(Target: source, Edit: style)).As().ToFin();
    }

    internal Fin<Unit> Apply(RhinoDoc document) => Switch(
        document,
        placeCase: static (context, edit) => Placed(
            document: context, seed: edit.Seed, placement: edit.Placement),
        amendCase: static (context, edit) => Reworked(
            document: context, target: edit.Target,
            change: (annotation, key) => edit.Edits
                .TraverseM(item => item.Apply(annotation: annotation, key: key)).As().Map(static _ => unit)),
        reformulaCase: static (context, edit) => Reworked(
            document: context, target: edit.Target,
            change: (annotation, key) => Try.lift(() => Fin.Succ(value: HostEdge.Side(() => annotation.SetRichText(
                rtfText: edit.Program.Compose(), dimstyle: annotation.DimensionStyle)))).Run().Bind(static inner => inner)),
        repointCase: static (context, edit) => Reworked(
            document: context, target: edit.Target,
            change: (annotation, key) =>
                from leader in Admit.Need(annotation as Leader)
                from _ in Try.lift(() => Fin.Succ(value: HostEdge.Side(
                    () => leader.Points3D = edit.Path.Points.ToArray()))).Run().Bind(static inner => inner)
                select unit),
        styleCase: static (context, edit) => Reworked(
            document: context, target: edit.Target,
            change: (annotation, key) => edit.Edit.Apply(annotation: annotation, op: key)));

    private static Fin<Unit> Placed(
        RhinoDoc document, AnnotationSeed seed, AnnotationPlacement placement) =>
        from style in placement.Style.Resolve(document: document, lens: StyleOp.Lens)
        from geometry in seed.Mint(plane: placement.Frame, style: style)
        from _ in new Lease<AnnotationBase>.Owned(Value: geometry).Use(
            body: owned =>
                from _ in placement.Overrides.Traverse(patch => patch.Overlay(annotation: owned, key: op)).As()
                from __ in Added(document: document, geometry: owned, placement: placement)
                select unit)
        select unit;

    private static Fin<Unit> Added(
        RhinoDoc document, AnnotationBase geometry, AnnotationPlacement placement) => Try.lift(() =>
        ResourceId.Admit(geometry switch {
            TextEntity text => document.Objects.AddText(
                text: text,
                attributes: HostEdge.Slot(value: placement.Attributes),
                history: HostEdge.Slot(value: placement.History),
                reference: placement.Residency.Key),
            Leader leader => document.Objects.AddLeader(
                leader: leader,
                attributes: HostEdge.Slot(value: placement.Attributes),
                history: HostEdge.Slot(value: placement.History),
                reference: placement.Residency.Key),
            _ => Guid.Empty,
        }).Map(static _ => unit)).Run().Bind(static inner => inner);

    internal static Fin<Unit> Reworked(
        RhinoDoc document, TableTarget target,
        Func<AnnotationBase, Fin<Unit>> change) =>
        from ids in target.Resolve(document: document)
        from _ in ids.TraverseM(id =>
            from native in Optional(document.Objects.FindId(id)).ToFin(Fail: new KernelFault.MissingContext())
            from source in Optional((native as AnnotationObjectBase)?.AnnotationGeometry).ToFin(Fail: new KernelFault.InvalidInput())
            from copy in Admit.Need(source.Duplicate() as AnnotationBase)
            from __ in new Lease<AnnotationBase>.Owned(Value: copy).Use(
                body: owned =>
                    from ___ in change(owned)
                    from ____ in Admit.Confirm(success: document.Objects.Replace(
                        objectId: id, geometry: owned, ignoreModes: false))
                    select unit)
            select unit).As()
        select unit;
}

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ContentTrait : ICapability<ContentTrait> {
    public static readonly ContentTrait RichFormatting = new(
        key: "rich-formatting", held: static (_, annotation) => annotation.TextHasRtfFormatting);
    public static readonly ContentTrait MeasurableFields = new(
        key: "measurable-fields", held: static (native, _) => native.HasMeasurableTextFields);

    [UseDelegateFromConstructor]
    internal partial bool Held(AnnotationObjectBase native, AnnotationBase annotation);

    internal static CapabilitySet<ContentTrait> On(AnnotationObjectBase native, AnnotationBase annotation) =>
        CapabilitySet<ContentTrait>.Of(
            toSeq(Items).Filter(row => row.Held(native: native, annotation: annotation)).ToArray());
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MaskTrait : ICapability<MaskTrait> {
    public static readonly MaskTrait ViewportColor = new(
        key: "viewport-color", held: static annotation => annotation.MaskUsesViewportColor);
    public static readonly MaskTrait Frame = new(
        key: "frame", held: static annotation => annotation.DrawTextFrame);

    [UseDelegateFromConstructor]
    internal partial bool Held(AnnotationBase annotation);

    internal static CapabilitySet<MaskTrait> On(AnnotationBase annotation) =>
        CapabilitySet<MaskTrait>.Of(toSeq(Items).Filter(row => row.Held(annotation: annotation)).ToArray());
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StyleTrait : ICapability<StyleTrait> {
    public static readonly StyleTrait Kerning = new(
        key: "kerning", held: static annotation => annotation.UseKerning);
    public static readonly StyleTrait DrawForward = new(
        key: "draw-forward", held: static annotation => annotation.DrawForward);

    [UseDelegateFromConstructor]
    internal partial bool Held(AnnotationBase annotation);

    internal static CapabilitySet<StyleTrait> On(AnnotationBase annotation) =>
        CapabilitySet<StyleTrait>.Of(toSeq(Items).Filter(row => row.Held(annotation: annotation)).ToArray());
}

public sealed record TextContentState(
    string Plain, string PlainWithFields, string Rich, string Display, CapabilitySet<ContentTrait> Traits);

public sealed record TextFormatState(
    string Face, string FirstFace,
    CapabilitySet<FaceDecoration> First, CapabilitySet<FaceDecoration> Across,
    double Height, double RotationRadians,
    bool Wrapped, double FormatWidth, double ModelWidth);

[SmartEnum<int>]
public sealed partial class AnnotationKind {
    public static readonly AnnotationKind Unset = new(key: (int)AnnotationType.Unset, measures: false);
    public static readonly AnnotationKind Aligned = new(key: (int)AnnotationType.Aligned, measures: true);
    public static readonly AnnotationKind Angular = new(key: (int)AnnotationType.Angular, measures: true);
    public static readonly AnnotationKind Diameter = new(key: (int)AnnotationType.Diameter, measures: true);
    public static readonly AnnotationKind Radius = new(key: (int)AnnotationType.Radius, measures: true);
    public static readonly AnnotationKind Rotated = new(key: (int)AnnotationType.Rotated, measures: true);
    public static readonly AnnotationKind Ordinate = new(key: (int)AnnotationType.Ordinate, measures: true);
    public static readonly AnnotationKind ArcLength = new(key: (int)AnnotationType.ArcLen, measures: true);
    public static readonly AnnotationKind CenterMark = new(key: (int)AnnotationType.CenterMark, measures: true);
    public static readonly AnnotationKind Text = new(key: (int)AnnotationType.Text, measures: false);
    public static readonly AnnotationKind Leader = new(key: (int)AnnotationType.Leader, measures: false);
    public static readonly AnnotationKind Angular3pt = new(key: (int)AnnotationType.Angular3pt, measures: true);

    internal bool Measures { get; }
    internal AnnotationType Host => (AnnotationType)Key;
}

[SmartEnum<int>]
public sealed partial class MaskSource {
    public static readonly MaskSource BackgroundColor = new(key: (int)DimensionStyle.MaskType.BackgroundColor);
    public static readonly MaskSource MaskColor = new(key: (int)DimensionStyle.MaskType.MaskColor);
}

[SmartEnum<int>]
public sealed partial class TextMaskFrame {
    public static readonly TextMaskFrame None = new(key: (int)DimensionStyle.MaskFrame.NoFrame);
    public static readonly TextMaskFrame Rectangle = new(key: (int)DimensionStyle.MaskFrame.RectFrame);
    public static readonly TextMaskFrame Capsule = new(key: (int)DimensionStyle.MaskFrame.CapsuleFrame);
    public static readonly TextMaskFrame Circle = new(key: (int)DimensionStyle.MaskFrame.CircleFrame);
    public static readonly TextMaskFrame Square = new(key: (int)DimensionStyle.MaskFrame.SquareFrame);
    public static readonly TextMaskFrame Diamond = new(key: (int)DimensionStyle.MaskFrame.DiamondFrame);
    public static readonly TextMaskFrame Triangle = new(key: (int)DimensionStyle.MaskFrame.TriangleFrame);
    public static readonly TextMaskFrame Hexagon = new(key: (int)DimensionStyle.MaskFrame.HexagonFrame);
    public static readonly TextMaskFrame HexagonCapsule = new(key: (int)DimensionStyle.MaskFrame.HexagonCapsuleFrame);
    public static readonly TextMaskFrame RoundRectangle = new(key: (int)DimensionStyle.MaskFrame.RoundRectFrame);
}

[SmartEnum<int>]
public sealed partial class LeaderArrow {
    public static readonly LeaderArrow None = new(key: (int)DimensionStyle.ArrowType.None);
    public static readonly LeaderArrow UserBlock = new(key: (int)DimensionStyle.ArrowType.UserBlock);
    public static readonly LeaderArrow SolidTriangle = new(key: (int)DimensionStyle.ArrowType.SolidTriangle);
    public static readonly LeaderArrow Dot = new(key: (int)DimensionStyle.ArrowType.Dot);
    public static readonly LeaderArrow Tick = new(key: (int)DimensionStyle.ArrowType.Tick);
    public static readonly LeaderArrow ShortTriangle = new(key: (int)DimensionStyle.ArrowType.ShortTriangle);
    public static readonly LeaderArrow OpenArrow = new(key: (int)DimensionStyle.ArrowType.OpenArrow);
    public static readonly LeaderArrow Rectangle = new(key: (int)DimensionStyle.ArrowType.Rectangle);
    public static readonly LeaderArrow LongTriangle = new(key: (int)DimensionStyle.ArrowType.LongTriangle);
    public static readonly LeaderArrow LongerTriangle = new(key: (int)DimensionStyle.ArrowType.LongerTriangle);
    public static readonly LeaderArrow SolidDatumTriangle = new(key: (int)DimensionStyle.ArrowType.SolidDatumTriangle);
}

[SmartEnum<int>]
public sealed partial class LeaderCurve {
    public static readonly LeaderCurve None = new(key: (int)DimensionStyle.LeaderCurveStyle.None);
    public static readonly LeaderCurve Polyline = new(key: (int)DimensionStyle.LeaderCurveStyle.Polyline);
    public static readonly LeaderCurve Spline = new(key: (int)DimensionStyle.LeaderCurveStyle.Spline);
}

[SmartEnum<int>]
public sealed partial class LeaderContentAngle {
    public static readonly LeaderContentAngle Horizontal = new(key: (int)DimensionStyle.LeaderContentAngleStyle.Horizontal);
    public static readonly LeaderContentAngle Aligned = new(key: (int)DimensionStyle.LeaderContentAngleStyle.Aligned);
    public static readonly LeaderContentAngle Rotated = new(key: (int)DimensionStyle.LeaderContentAngleStyle.Rotated);
}

[SmartEnum<int>]
public sealed partial class TextAlignAcross {
    public static readonly TextAlignAcross Left = new(key: (int)TextHorizontalAlignment.Left);
    public static readonly TextAlignAcross Center = new(key: (int)TextHorizontalAlignment.Center);
    public static readonly TextAlignAcross Right = new(key: (int)TextHorizontalAlignment.Right);
    public static readonly TextAlignAcross Auto = new(key: (int)TextHorizontalAlignment.Auto);
    public static readonly TextAlignAcross Justify = new(key: (int)TextHorizontalAlignment.Justify);
}

[SmartEnum<int>]
public sealed partial class TextAlignDown {
    public static readonly TextAlignDown Top = new(key: (int)TextVerticalAlignment.Top);
    public static readonly TextAlignDown MiddleOfTop = new(key: (int)TextVerticalAlignment.MiddleOfTop);
    public static readonly TextAlignDown BottomOfTop = new(key: (int)TextVerticalAlignment.BottomOfTop);
    public static readonly TextAlignDown Middle = new(key: (int)TextVerticalAlignment.Middle);
    public static readonly TextAlignDown MiddleOfBottom = new(key: (int)TextVerticalAlignment.MiddleOfBottom);
    public static readonly TextAlignDown Bottom = new(key: (int)TextVerticalAlignment.Bottom);
    public static readonly TextAlignDown BottomOfBoundingBox = new(key: (int)TextVerticalAlignment.BottomOfBoundingBox);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextMask {
    private TextMask() { }
    public sealed record Off : TextMask;
    public sealed record On(
        PerceptualColor Color, MaskSource Source, TextMaskFrame Frame,
        double Offset, CapabilitySet<MaskTrait> Traits) : TextMask;

    internal static Fin<TextMask> Read(AnnotationBase annotation) =>
        annotation.MaskEnabled
            ? from color in PerceptualColor.OfHost(host: annotation.MaskColor)
              from source in FactoryBridge.Row<DimensionStyle.MaskType, MaskSource>(
                  candidate: annotation.MaskColorSource, ordinal: static value => (int)value)
              from frame in FactoryBridge.Row<DimensionStyle.MaskFrame, TextMaskFrame>(
                  candidate: annotation.MaskFrame, ordinal: static value => (int)value)
              select (TextMask)new On(
                  Color: color, Source: source, Frame: frame, Offset: annotation.MaskOffset,
                  Traits: MaskTrait.On(annotation: annotation))
            : Fin.Succ<TextMask>(value: new Off());
}

public sealed record TextStyleState(
    ResourceId Style,
    Option<ResourceId> Parent,
    Seq<StyleField> Overridden,
    char DecimalSeparator,
    CapabilitySet<StyleTrait> Traits,
    double LineSpaceScale,
    double DimensionScale,
    LengthDisplayRow LengthDisplay,
    LengthDisplayRow AlternateLengthDisplay);

public sealed record TextState(
    ResourceId Key, AnnotationKind Kind,
    Plane Frame, BoundingBox Bounds,
    TextContentState Content, TextFormatState Format, TextMask Mask,
    TextStyleState Style) : IDetachedDocumentResult {
    internal static Fin<TextState> Of(AnnotationObjectBase native) => key.Catch(() =>
        from annotation in Optional(native.AnnotationGeometry).ToFin(Fail: new KernelFault.InvalidResult())
        from id in ResourceId.Admit(native.Id)
        from kind in FactoryBridge.Row<AnnotationType, AnnotationKind>(
            candidate: annotation.AnnotationType, ordinal: static value => (int)value)
        from mask in TextMask.Read(annotation: annotation)
        from lengthDisplay in FactoryBridge.Row<DimensionStyle.LengthDisplay, LengthDisplayRow>(
            candidate: annotation.DimensionLengthDisplay, ordinal: static value => (int)value)
        from alternateLengthDisplay in FactoryBridge.Row<DimensionStyle.LengthDisplay, LengthDisplayRow>(
            candidate: annotation.AlternateDimensionLengthDisplay, ordinal: static value => (int)value)
        from style in ResourceId.Admit(annotation.DimensionStyleId)
        select new TextState(
            Key: id,
            Kind: kind,
            Frame: annotation.Plane,
            Bounds: annotation.GetBoundingBox(xform: Transform.Identity),
            Content: new TextContentState(
                annotation.PlainText, annotation.PlainTextWithFields, annotation.RichText, native.DisplayText,
                ContentTrait.On(native: native, annotation: annotation)),
            Format: new TextFormatState(
                annotation.Font.FaceName,
                annotation.FirstCharFont.FaceName,
                FaceDecoration.On(font: annotation.FirstCharFont),
                FaceDecoration.Across(annotation: annotation),
                annotation.TextHeight,
                annotation.TextRotationRadians,
                annotation.TextIsWrapped,
                annotation.FormatWidth,
                annotation.TextModelWidth),
            Mask: mask,
            Style: new TextStyleState(
                Style: style,
                Parent: Optional(annotation.ParentDimensionStyle)
                    .Bind(row => ResourceId.Maybe(row.Id)),
                Overridden: annotation.HasPropertyOverrides
                    ? toSeq(StyleField.Items).Filter(field => annotation.IsPropertyOverridden(field: field.Host))
                    : Seq<StyleField>(),
                DecimalSeparator: annotation.DecimalSeparator,
                Traits: StyleTrait.On(annotation: annotation),
                LineSpaceScale: annotation.LineSpaceScale,
                DimensionScale: annotation.DimensionScale,
                LengthDisplay: lengthDisplay,
                AlternateLengthDisplay: alternateLengthDisplay)));
}

public sealed record LeaderFacts(
    ResourceId Key, Seq<Point2d> Points2D, Seq<Point3d> Points3D, Option<Lease<NurbsCurve>> Spline,
    LeaderArrow ArrowType, double ArrowSize, Option<ResourceId> ArrowBlock,
    LeaderCurve CurveStyle,
    LeaderContentAngle ContentAngleStyle,
    TextAlignAcross HorizontalAlignment,
    TextAlignDown VerticalAlignment,
    Option<double> Landing) : IDetachedDocumentResult {
    public Fin<Unit> Release() => Custody.Dispose(held: Spline.ToSeq());
}

public readonly record struct RunLocation(int Run, int Start, int Length);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextAsk {
    private TextAsk() { }
    public sealed record State(TableTarget Target) : TextAsk;
    public sealed record LeaderState(TableTarget Target) : TextAsk;
    public sealed record RunMap(TableTarget Target) : TextAsk;
    public sealed record Evaluate(FieldProgram Program) : TextAsk;
    public sealed record Tokens(FormulaText Text) : TextAsk;
    public sealed record Outline(TableTarget Target, OutlineSpec Spec) : TextAsk;
    public sealed record Pieces(TableTarget Target) : TextAsk;
    public sealed record Scale(ResourceRef Style, ViewportTarget Target) : TextAsk;

    internal Fin<TextAnswer> Answer(RhinoDoc document) => Switch(
        document,
        state: static (context, ask) =>
            from native in Single(document: context, target: ask.Target)
            from snapshot in TextState.Of(native: native)
            select (TextAnswer)new TextAnswer.State(Snapshot: snapshot),
        leaderState: static (context, ask) =>
            from native in Single(document: context, target: ask.Target)
            from facts in context.Op.Catch(() =>
                from leader in Admit.Need(native.AnnotationGeometry as Leader)
                from id in ResourceId.Admit(native.Id)
                from arrow in FactoryBridge.Row<DimensionStyle.ArrowType, LeaderArrow>(
                    candidate: leader.LeaderArrowType, ordinal: static value => (int)value)
                from curve in FactoryBridge.Row<DimensionStyle.LeaderCurveStyle, LeaderCurve>(
                    candidate: leader.LeaderCurveStyle, ordinal: static value => (int)value)
                from angle in FactoryBridge.Row<DimensionStyle.LeaderContentAngleStyle, LeaderContentAngle>(
                    candidate: leader.LeaderContentAngleStyle, ordinal: static value => (int)value)
                from across in FactoryBridge.Row<TextHorizontalAlignment, TextAlignAcross>(
                    candidate: leader.LeaderTextHorizontalAlignment, ordinal: static value => (int)value)
                from down in FactoryBridge.Row<TextVerticalAlignment, TextAlignDown>(
                    candidate: leader.LeaderTextVerticalAlignment, ordinal: static value => (int)value)
                select new LeaderFacts(
                    Key: id,
                    Points2D: toSeq(leader.Points2D),
                    Points3D: toSeq(leader.Points3D),
                    ArrowType: arrow,
                    ArrowSize: leader.LeaderArrowSize,
                    ArrowBlock: ResourceId.Maybe(leader.LeaderArrowBlockId),
                    CurveStyle: curve,
                    ContentAngleStyle: angle,
                    HorizontalAlignment: across,
                    VerticalAlignment: down,
                    Landing: leader.LeaderHasLanding ? Some(leader.LeaderLandingLength) : Option<double>.None,
                    Spline: Optional(leader.Curve).Map(static value =>
                        (Lease<NurbsCurve>)new Lease<NurbsCurve>.Owned(Value: (NurbsCurve)value.Duplicate()))))
            select (TextAnswer)new TextAnswer.LeaderState(Facts: facts),
        runMap: static (context, ask) =>
            from native in Single(document: context, target: ask.Target)
            from annotation in Optional(native.AnnotationGeometry).ToFin(Fail: new KernelFault.InvalidResult())
            from mapped in Try.lift(() => {
                int[] map = [];
                string text = annotation.GetPlainTextWithRunMap(map: ref map);
                return guard(map.Length % 3 == 0, new KernelFault.InvalidResult()).ToFin().Map(_ => (
                    Text: text,
                    Runs: toSeq(map.Chunk(3)).Map(static row => new RunLocation(row[0], row[1], row[2]))));
            }).Run().Bind(static inner => inner)
            select (TextAnswer)new TextAnswer.Mapped(Text: mapped.Text, Runs: mapped.Runs),
        evaluate: static (context, ask) =>
            from value in ask.Program.Evaluate(document: context)
            select (TextAnswer)new TextAnswer.Resolved(Text: value),
        tokens: static (context, ask) =>
            from split in Try.lift(() => TextFields.TryParse(
                    text: ask.Text.Value, doc: context, out List<string> result)
                ? Fin.Succ(value: toSeq(result))
                : Fin.Fail<Seq<string>>(error: new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
            select (TextAnswer)new TextAnswer.Split(Tokens: split),
        outline: static (context, ask) =>
            from native in Single(document: context, target: ask.Target)
            from text in Admit.Need(native.AnnotationGeometry as TextEntity)
            from style in Optional(text.DimensionStyle).ToFin(Fail: new KernelFault.MissingContext())
            from product in ask.Spec.Apply(source: text, style: style)
            select (TextAnswer)new TextAnswer.Outlined(Product: product),
        pieces: static (context, ask) =>
            from native in Single(document: context, target: ask.Target)
            from products in (native.AnnotationGeometry switch {
                TextEntity text => Try.lift(() => Optional(text.Explode())
                    .Map(static curves => toSeq(curves).Map(static curve => (GeometryBase)curve))
                    .ToFin(Fail: new KernelFault.InvalidResult())).Run().Bind(static inner => inner),
                Leader leader => Try.lift(() => Optional(leader.Explode())
                    .Map(static values => toSeq(values))
                    .ToFin(Fail: new KernelFault.InvalidResult())).Run().Bind(static inner => inner),
                var geometry => Fin.Fail<Seq<GeometryBase>>(new KernelFault.Unsupported(
                    InputType: geometry.GetType(), OutputType: typeof(GeometryBase))),
            })
            from handles in DraftCrossing.Crossed(products: products)
            select (TextAnswer)new TextAnswer.Pieces(Products: handles),
        scale: static (context, ask) =>
            from style in ask.Style.Resolve(document: context, lens: StyleOp.Lens)
            from viewport in ask.Target.ResolveViewport(document: context)
            from factor in Try.lift(() => Fin.Succ(value: AnnotationBase.GetDimensionScale(
                doc: context, dimstyle: style, vport: viewport))).Run().Bind(static inner => inner)
            select (TextAnswer)new TextAnswer.Scaled(Factor: factor));

    internal static Fin<AnnotationObjectBase> Single(RhinoDoc document, TableTarget target) =>
        target.Only<AnnotationObjectBase>(document: document).Map(static row => row.Native);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextAnswer : IDetachedDocumentResult {
    private TextAnswer() { }
    public sealed record State(TextState Snapshot) : TextAnswer;
    public sealed record LeaderState(LeaderFacts Facts) : TextAnswer;
    public sealed record Mapped(string Text, Seq<RunLocation> Runs) : TextAnswer;
    public sealed record Resolved(string Text) : TextAnswer;
    public sealed record Split(Seq<string> Tokens) : TextAnswer;
    public sealed record Outlined(OutlineProduct Product) : TextAnswer;
    public sealed record Pieces(Seq<GeometryHandle> Products) : TextAnswer;
    public sealed record Scaled(double Factor) : TextAnswer;

    public Fin<Unit> Release() => Switch(
        context: key,
        state: static (_, _) => Fin.Succ(unit),
        leaderState: static (answer) => answer.Facts.Release(),
        mapped: static (_, _) => Fin.Succ(unit),
        resolved: static (_, _) => Fin.Succ(unit),
        split: static (_, _) => Fin.Succ(unit),
        outlined: static (answer) => answer.Product.Release(),
        pieces: static (answer) => Custody.Dispose(held: answer.Products),
        scaled: static (_, _) => Fin.Succ(unit));
}

public static class Texts {
    public static Fin<Unit> Commit(DocumentSession session, DraftPlan<TextOp> plan) =>
        DraftSpine.Commit(session: session, plan: plan,
            apply: static (document, operation) => operation.Apply(document: document));

    public static Fin<TextAnswer> Ask(DocumentSession session, TextAsk request) {
        return from admitted in Acceptance.Input(value: request)
               from answer in session.Demand(
                   use: document => admitted.Answer(document: document, op: op), needs: [SessionNeed.Read])
               select answer;
    }
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]       | [OWNER]          | [FORM]                                              | [ENTRY]          |
| :-----: | :-------------- | :--------------- | :-------------------------------------------------- | :--------------- |
|  [01]   | content ingress | `TextSeed`       | admitted format plus source                         | `Create`         |
|  [02]   | live formatting | `RunEdit`        | replacement, shared decoration rows, or wrap        | `Replace`/`Wrap` |
|  [03]   | RTF delta       | `TextRtf`        | keyed decoration fold onto the host's eight flags   | `Restyled`       |
|  [04]   | field formulas  | `FieldKind`      | evaluator rows carrying admissible typed signatures | `FieldExpr.Of`   |
|  [05]   | annotation kind | `AnnotationKind` | whole host roster with the measuring column         | `TextState.Of`   |
|  [06]   | outline egress  | `OutlineSpec`    | two closed folds plus transform and evidence        | `Apply`          |
|  [07]   | outline custody | `OutlineProduct` | declared owned run with typed cleanup               | `Release`        |
|  [08]   | text mutations  | `TextOp`         | sealed placement plus duplicate-then-replace edits  | `Texts.Commit`   |
|  [09]   | text evidence   | `TextAsk`        | owned detached evidence family                      | `Texts.Ask`      |

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

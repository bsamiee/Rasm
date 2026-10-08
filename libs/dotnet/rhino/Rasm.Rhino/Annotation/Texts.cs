using Rasm.Rhino.Annotation.Styles;
using Rasm.Rhino.Document.Shapes;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Annotation;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextForm {
    private TextForm(string richText, Plane plane) => (RichText, Plane) = (richText, plane);

    public string RichText { get; }

    public Plane Plane { get; }

    public sealed record Note(string RichText, Plane Plane, Option<double> WrapWidth, double RotationRadians) : TextForm(RichText, Plane);

    public sealed record Callout(string RichText, Plane Plane, Seq<Point3d> Points) : TextForm(RichText, Plane);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RunEdit {
    public sealed record RunReplace(string ReplaceString, int StartRunIndex, int StartRunPosition, int EndRunIndex, int EndRunPosition) : RunEdit;

    public sealed record SetBold(bool On) : RunEdit;

    public sealed record SetItalic(bool On) : RunEdit;

    public sealed record SetUnderline(bool On) : RunEdit;

    public sealed record SetFacename(Option<string> Facename) : RunEdit;

    public sealed record WrapText(double FormatWidth) : RunEdit;

    public sealed record SetRichText(string RichText) : RunEdit;

    public Fin<Unit> Apply(AnnotationBase target) =>
        Switch<AnnotationBase, Fin<Unit>>(
            target,
            runReplace: static (annotation, edit) => Refused.Unless(
                annotation.RunReplace(edit.ReplaceString, edit.StartRunIndex, edit.StartRunPosition, edit.EndRunIndex, edit.EndRunPosition),
                nameof(AnnotationBase.RunReplace)),
            setBold: static (annotation, edit) => Refused.Unless(annotation.SetBold(edit.On), nameof(AnnotationBase.SetBold)),
            setItalic: static (annotation, edit) => Refused.Unless(annotation.SetItalic(edit.On), nameof(AnnotationBase.SetItalic)),
            setUnderline: static (annotation, edit) => Refused.Unless(annotation.SetUnderline(edit.On), nameof(AnnotationBase.SetUnderline)),
            setFacename: static (annotation, edit) => Refused.Unless(
                annotation.SetFacename(edit.Facename.IsSome, Conversions.Unset(edit.Facename)),
                nameof(AnnotationBase.SetFacename)),
            wrapText: static (annotation, edit) => {
                annotation.TextIsWrapped = true;
                annotation.FormatWidth = edit.FormatWidth;
                return unit;
            },
            setRichText: static (annotation, edit) => {
                annotation.RichText = edit.RichText;
                return unit;
            });
}

public sealed record TextMask(System.Drawing.Color MaskColor, DimensionStyle.MaskType MaskColorSource, DimensionStyle.MaskFrame MaskFrame, double MaskOffset);

public sealed record TextState(
    AnnotationType AnnotationType,
    Plane Plane,
    BoundingBox Bounds,
    string PlainText,
    string PlainTextWithFields,
    string RichText,
    string DisplayText,
    bool TextHasRtfFormatting,
    bool HasMeasurableTextFields,
    FontQuery.Quartet Font,
    FontQuery.Quartet FirstCharFont,
    bool IsAllBold,
    bool IsAllItalic,
    bool IsAllUnderlined,
    double TextHeight,
    double TextRotationRadians,
    Option<double> WrapWidth,
    Option<TextMask> Mask,
    Guid DimensionStyleId,
    Seq<DimensionStyle.Field> Overridden,
    char DecimalSeparator,
    bool UseKerning,
    bool DrawForward,
    double LineSpaceScale,
    double DimensionScale,
    DimensionStyle.LengthDisplay DimensionLengthDisplay,
    DimensionStyle.LengthDisplay AlternateDimensionLengthDisplay);

public sealed record LeaderState(
    Seq<Point3d> Points3D,
    DimensionStyle.ArrowType LeaderArrowType,
    double LeaderArrowSize,
    Option<Guid> LeaderArrowBlockId,
    DimensionStyle.LeaderCurveStyle LeaderCurveStyle,
    DimensionStyle.LeaderContentAngleStyle LeaderContentAngleStyle,
    TextHorizontalAlignment LeaderTextHorizontalAlignment,
    TextVerticalAlignment LeaderTextVerticalAlignment,
    Option<double> LeaderLanding);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class TextMapper {
    // --- [PROJECTIONS]
    [MapProperty(nameof(AnnotationBase.PlainText), nameof(TextState.PlainText), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(AnnotationBase.PlainTextWithFields), nameof(TextState.PlainTextWithFields), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(AnnotationBase.RichText), nameof(TextState.RichText), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(AnnotationBase.Font), nameof(TextState.Font), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(AnnotationBase.FirstCharFont), nameof(TextState.FirstCharFont), SuppressNullMismatchDiagnostic = true)]
    [MapPropertyFromSource(nameof(TextState.Bounds), Use = nameof(Bounds))]
    [MapPropertyFromSource(nameof(TextState.IsAllBold), Use = nameof(IsAllBold))]
    [MapPropertyFromSource(nameof(TextState.IsAllItalic), Use = nameof(IsAllItalic))]
    [MapPropertyFromSource(nameof(TextState.IsAllUnderlined), Use = nameof(IsAllUnderlined))]
    [MapPropertyFromSource(nameof(TextState.WrapWidth), Use = nameof(WrapWidth))]
    [MapPropertyFromSource(nameof(TextState.Mask), Use = nameof(Mask))]
    [MapPropertyFromSource(nameof(TextState.Overridden), Use = nameof(Overridden))]
    internal static partial TextState ToState(AnnotationBase annotation, string displayText, bool hasMeasurableTextFields);

    [MapProperty(nameof(Leader.Points3D), nameof(LeaderState.Points3D), Use = nameof(@Conversions.Rows))]
    [MapPropertyFromSource(nameof(LeaderState.LeaderLanding), Use = nameof(LeaderLanding))]
    internal static partial LeaderState ToState(Leader leader);

    private static partial TextMask ToMask(AnnotationBase annotation);

    [UserMapping]
    private static FontQuery.Quartet Face(Font font) => FontMapper.ToState(font);

    // --- [COMPUTED]
    private static BoundingBox Bounds(AnnotationBase annotation) => annotation.GetBoundingBox(accurate: true);

    private static bool IsAllBold(AnnotationBase annotation) => annotation.IsAllBold();

    private static bool IsAllItalic(AnnotationBase annotation) => annotation.IsAllItalic();

    private static bool IsAllUnderlined(AnnotationBase annotation) => annotation.IsAllUnderlined();

    private static Option<double> WrapWidth(AnnotationBase annotation) => Callbacks.Found(annotation.TextIsWrapped, annotation.FormatWidth);

    private static Option<TextMask> Mask(AnnotationBase annotation) => annotation.MaskEnabled ? Some(ToMask(annotation)) : None;

    private static Seq<DimensionStyle.Field> Overridden(AnnotationBase annotation) => DimensionStyles.Overrides(annotation.IsPropertyOverridden);

    private static Option<double> LeaderLanding(Leader leader) => Callbacks.Found(leader.LeaderHasLanding, leader.LeaderLandingLength);
}

public static class Texts {
    // --- [CREATION]
    public static IO<AnnotationBase> Create(
        RhinoDoc doc, TextForm form, Option<ComponentRef<DimensionStyle>> style, Option<Func<DimensionStyle, IO<Unit>>> overrides) =>
        from parent in style.Match(
            Some: address => TableOps.Find(doc.DimStyles, address, includeDeleted: false),
            None: () => IO.lift(() => doc.DimStyles.Current))
        from annotation in form.Switch(
            parent,
            note: static (row, note) => Copies.Acquire<AnnotationBase>(
                () => TextEntity.CreateWithRichText(note.RichText, note.Plane, row, note.WrapWidth.IsSome, note.WrapWidth.IfNone(0d), note.RotationRadians),
                nameof(TextEntity.CreateWithRichText)),
            callout: static (row, callout) => Copies.Acquire<AnnotationBase>(
                () => Leader.CreateWithRichText(callout.RichText, callout.Plane, row, [.. callout.Points]),
                nameof(Leader.CreateWithRichText)))
        from overridden in DisposalOps.OnFailure(
            overrides.Traverse(edit => DimensionStyles.SetOverride(doc, annotation, edit)).As(),
            IO.lift(annotation.Dispose))
        select annotation;

    // --- [EDITS]
    public static IO<Unit> Modify(RhinoDoc doc, Guid id, Seq<RunEdit> edits) =>
        DimensionStyles.Edit<AnnotationBase>(doc, id, annotation =>
            IO.lift(() => edits.TraverseM(edit => edit.Apply(annotation)).As())
                .Bind(_ => when(annotation.TextIsWrapped, IO.lift(annotation.WrapText)).As()));

    public static IO<Unit> Repoint(RhinoDoc doc, Guid id, Seq<Point3d> points) =>
        DimensionStyles.Edit<Leader>(doc, id, leader => IO.lift(() => { leader.Points3D = [.. points]; }));

    // --- [READS]
    public static IO<TextState> State(AnnotationObjectBase owner, AnnotationBase annotation) =>
        IO.lift(() => TextMapper.ToState(annotation, owner.DisplayText, owner.HasMeasurableTextFields));

    public static IO<LeaderState> State(Leader leader) =>
        IO.lift(() => TextMapper.ToState(leader));

    public static IO<(string PlainText, Seq<(int Run, int Start, int Length)> Runs)> RunMap(AnnotationBase annotation) =>
        IO.lift(() => {
            int[] map = [];
            string plainText = annotation.GetPlainTextWithRunMap(ref map);
            return (PlainText: plainText, Runs: toSeq(map.Chunk(3)).Map(static run => (Run: run[0], Start: run[1], Length: run[2])).Strict());
        });

    // --- [FRAMES]
    public static IO<Transform> GetTextTransform(TextEntity text, DimensionStyle style, double scale, Option<RhinoViewport> viewport) =>
        viewport.Match(
            Some: view => use(() => new ViewportInfo(view)).Bind(info => IO.lift(() => text.GetTextTransform(info, scale, style))).Bracket(),
            None: () => IO.lift(() => text.GetTextTransform(scale, style)));

    public static IO<Seq<Point3d>> GetTextCorners(TextObject text, RhinoViewport viewport) =>
        IO.lift(() => Conversions.NonEmpty(toSeq(text.GetTextCorners(viewport)), nameof(TextObject.GetTextCorners)));

    // --- [PIECES]
    public static IO<A> TextPieces<A>(TextEntity text, Func<Seq<Curve>, IO<A>> body) =>
        Copies.Acquire(() => text.Explode(), nameof(TextEntity.Explode)).Bracket(Use: body, Fin: DisposalOps.Release);

    public static IO<A> LeaderPieces<A>(Leader leader, Func<Seq<GeometryBase>, IO<A>> body) =>
        Copies.Acquire(() => leader.Explode(), nameof(Leader.Explode)).Bracket(Use: body, Fin: DisposalOps.Release);

    public static IO<A> LeaderCurve<A>(Leader leader, Func<NurbsCurve, IO<A>> body) =>
        IO.lift(() => Missing.Unless(leader.Curve, nameof(Leader.Curve))).Bind(curve => use(Copies.DuplicateShallow(curve)).Bind(body).Bracket());
}

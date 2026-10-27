using Rasm.RhinoCommon.Annotation.Styles;
using Rasm.RhinoCommon.Document.Tables;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

namespace Rasm.RhinoCommon.Annotation;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record TextForm {
    private TextForm(string richText, Plane plane) => (RichText, Plane) = (richText, plane);

    public string RichText { get; }

    public Plane Plane { get; }

    public sealed record Note(string RichText, Plane Plane, Option<double> WrapWidth, double RotationRadians) : TextForm(RichText, Plane);

    public sealed record Callout(string RichText, Plane Plane, Seq<Point3d> Points) : TextForm(RichText, Plane);
}

public sealed record TextMask(bool MaskEnabled, System.Drawing.Color MaskColor, DimensionStyle.MaskType MaskColorSource, DimensionStyle.MaskFrame MaskFrame, double MaskOffset);

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
    Font Font,
    Font FirstCharFont,
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
    [MapProperty(nameof(Leader.Points3D), nameof(LeaderState.Points3D), Use = nameof(@Conversions.Rows))]
    [MapPropertyFromSource(nameof(LeaderState.LeaderLanding), Use = nameof(LeaderLanding))]
    internal static partial LeaderState ToState(Leader leader);

    internal static partial TextMask Mask(AnnotationBase annotation);

    private static Option<double> LeaderLanding(Leader leader) => Conversions.Found(leader.LeaderHasLanding, leader.LeaderLandingLength);

    // --- [UPDATES]
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update((bool TextIsWrapped, double FormatWidth) source, AnnotationBase target);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapPropertyFromSource(nameof(Leader.Points3D))]
    internal static partial void Update(Seq<Point3d> source, Leader target);
}

public static class Texts {
    // --- [CREATION]
    public static IO<AnnotationBase> Create(RhinoDoc doc, TextForm form, Option<ComponentRef<DimensionStyle>> style, Option<Func<DimensionStyle, IO<Unit>>> overrides) =>
        form.Switch(
            (Doc: doc, Style: style, Overrides: overrides),
            note: static (state, note) => Dimensions.Create(
                state.Doc, state.Style, state.Overrides,
                parent => Products.Made<AnnotationBase>(TextEntity.CreateWithRichText(note.RichText, note.Plane, parent, note.WrapWidth.IsSome, note.WrapWidth.IfNone(0d), note.RotationRadians))),
            callout: static (state, callout) => Dimensions.Create(
                state.Doc, state.Style, state.Overrides,
                parent => Products.Made<AnnotationBase>(Leader.CreateWithRichText(callout.RichText, callout.Plane, parent, [.. callout.Points]))));

    // --- [EDITS]
    public static IO<Unit> Modify(RhinoDoc doc, Guid id, Seq<Func<AnnotationBase, IO<Unit>>> edits, Option<double> wrapWidth) =>
        DimensionStyles.Edit<AnnotationBase>(doc, id, annotation =>
            edits.TraverseM(edit => edit(annotation)).Map(static _ => unit).As()
                >> IO.lift(() => {
                    _ = wrapWidth.Iter(width => TextMapper.Update((TextIsWrapped: true, FormatWidth: width), annotation));
                    if (annotation.TextIsWrapped)
                        annotation.WrapText();
                }));

    public static IO<Unit> Repoint(RhinoDoc doc, Guid id, Seq<Point3d> points) =>
        DimensionStyles.Edit<Leader>(doc, id, leader => IO.lift(() => TextMapper.Update(points, leader)));

    // --- [READS]
    public static IO<TextState> State(AnnotationObjectBase owner, AnnotationBase annotation) =>
        IO.lift(() => new TextState(
            annotation.AnnotationType,
            annotation.Plane,
            annotation.GetBoundingBox(accurate: true),
            annotation.PlainText,
            annotation.PlainTextWithFields,
            annotation.RichText,
            owner.DisplayText,
            annotation.TextHasRtfFormatting,
            owner.HasMeasurableTextFields,
            annotation.Font,
            annotation.FirstCharFont,
            annotation.IsAllBold(),
            annotation.IsAllItalic(),
            annotation.IsAllUnderlined(),
            annotation.TextHeight,
            annotation.TextRotationRadians,
            Conversions.Found(annotation.TextIsWrapped, annotation.FormatWidth),
            annotation.MaskEnabled || annotation.MaskFrame is not DimensionStyle.MaskFrame.NoFrame ? Some(TextMapper.Mask(annotation)) : None,
            annotation.DimensionStyleId,
            DimensionStyles.Overrides(annotation.IsPropertyOverridden),
            annotation.DecimalSeparator,
            annotation.UseKerning,
            annotation.DrawForward,
            annotation.LineSpaceScale,
            annotation.DimensionScale,
            annotation.DimensionLengthDisplay,
            annotation.AlternateDimensionLengthDisplay));

    public static IO<LeaderState> State(Leader leader) =>
        IO.lift(() => TextMapper.ToState(leader));

    public static IO<(string PlainText, Seq<(int Run, int Start, int Length)> Runs)> RunMap(AnnotationBase annotation) =>
        IO.lift(() => {
            int[] map = [];
            string plainText = annotation.GetPlainTextWithRunMap(ref map);
            return (PlainText: plainText, Runs: toSeq(map.Chunk(size: 3)).Map(static run => (Run: run[0], Start: run[1], Length: run[2])).Strict());
        });

    // --- [FRAMES]
    public static IO<Transform> GetTextTransform(TextEntity text, DimensionStyle style, double scale, Option<RhinoViewport> viewport) =>
        viewport.Match(
            Some: view => use(() => new ViewportInfo(view)).Bind(info => IO.lift(() => text.GetTextTransform(info, scale, style))).Bracket(),
            None: () => IO.lift(() => text.GetTextTransform(scale, style)));

    public static IO<Seq<Point3d>> GetTextCorners(TextObject text, RhinoViewport viewport) =>
        from corners in IO.lift(() => toSeq(text.GetTextCorners(viewport)))
        from present in IO.lift(() => RhinoFault.Unless(!corners.IsEmpty))
        select corners;

    // --- [PIECES]
    public static IO<T> TextPieces<T>(TextEntity text, Func<Seq<Curve>, IO<T>> body) =>
        (from exploded in IO.lift(() => text.Explode())
         from pieces in Products.Made(exploded)
         from value in body(pieces)
         select value).Bracket();

    public static IO<T> LeaderPieces<T>(Leader leader, Func<Seq<GeometryBase>, IO<T>> body) =>
        (from exploded in IO.lift(() => leader.Explode())
         from pieces in Products.Made(exploded)
         from value in body(pieces)
         select value).Bracket();

    public static IO<T> LeaderCurve<T>(Leader leader, Func<NurbsCurve, IO<T>> body) =>
        from curve in IO.lift(() => Conversions.Found(leader.Curve))
        from value in use(() => (NurbsCurve)curve.DuplicateShallow()).Bind(body).Bracket()
        select value;
}

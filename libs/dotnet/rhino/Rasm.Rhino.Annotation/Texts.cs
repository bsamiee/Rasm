using Rasm.Rhino.Document;
using Rasm.Rhino.Objects;
using Rhino;
using Rhino.DocObjects;
using Rhino.Runtime;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Annotation;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextContent {
    public sealed record Plain(string Text) : TextContent;

    public sealed record Rich(string RichText) : TextContent;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextForm {
    public sealed record TextEntity(TextContent Content, Plane Plane, bool Wrapped, double RectWidth, double RotationRadians) : TextForm;

    public sealed record Leader(TextContent Content, Plane Plane, Seq<Point3d> Points) : TextForm;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RunEdit {
    public sealed record Replace(string ReplaceString, int StartRunIndex, int StartRunPosition, int EndRunIndex, int EndRunPosition) : RunEdit;

    public sealed record Bold(bool On) : RunEdit;

    public sealed record Italic(bool On) : RunEdit;

    public sealed record Underline(bool On) : RunEdit;

    public sealed record Facename(bool On, string Name) : RunEdit;

    public sealed record Wrap(double FormatWidth) : RunEdit;

    public sealed record Content(string RichText) : RunEdit;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OutlineKind {
    public sealed record Curves(bool AllowOpen) : OutlineKind;

    public sealed record Surfaces() : OutlineKind;

    public sealed record PolySurfaces(double Height) : OutlineKind;

    public sealed record Extrusions(double Height) : OutlineKind;
}

public sealed record OutlineOptions(OutlineKind Kind, bool MakeSmallCaps, double SmallCapsScale, double Spacing, Option<Transform> PreTransform);

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
    string FontFaceName,
    string FirstCharFontFaceName,
    bool FirstCharFontBold,
    bool FirstCharFontItalic,
    bool FirstCharFontUnderlined,
    bool FirstCharFontStrikeout,
    bool IsAllBold,
    bool IsAllItalic,
    bool IsAllUnderlined,
    double TextHeight,
    double TextRotationRadians,
    bool TextIsWrapped,
    double FormatWidth,
    double TextModelWidth,
    bool MaskEnabled,
    System.Drawing.Color MaskColor,
    DimensionStyle.MaskType MaskColorSource,
    DimensionStyle.MaskFrame MaskFrame,
    double MaskOffset,
    Guid DimensionStyleId,
    bool HasPropertyOverrides,
    Seq<DimensionStyle.Field> Overridden,
    char DecimalSeparator,
    bool UseKerning,
    bool DrawForward,
    double LineSpaceScale,
    double DimensionScale,
    DimensionStyle.LengthDisplay DimensionLengthDisplay,
    DimensionStyle.LengthDisplay AlternateDimensionLengthDisplay);

public sealed record LeaderState(
    Seq<Point2d> Points2D,
    Seq<Point3d> Points3D,
    DimensionStyle.ArrowType LeaderArrowType,
    double LeaderArrowSize,
    Guid LeaderArrowBlockId,
    DimensionStyle.LeaderCurveStyle LeaderCurveStyle,
    DimensionStyle.LeaderContentAngleStyle LeaderContentAngleStyle,
    TextHorizontalAlignment LeaderTextHorizontalAlignment,
    TextVerticalAlignment LeaderTextVerticalAlignment,
    bool LeaderHasLanding,
    double LeaderLandingLength);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class Texts {
    // --- [PLACEMENT]
    public static IO<Guid> Place(RhinoDoc doc, TextForm form, DimensionStyle style, Option<ObjectAttributes> attributes, Option<HistoryRecord> history, bool reference) =>
        Disposal.Using(
            IO.lift(() => form.Switch(
                style,
                textEntity: static (parent, text) => text.Content.Switch(
                    (Parent: parent, Form: text),
                    plain: static (state, plain) => Missing.Unless<AnnotationBase>(
                        TextEntity.Create(plain.Text, state.Form.Plane, state.Parent, state.Form.Wrapped, state.Form.RectWidth, state.Form.RotationRadians),
                        nameof(TextEntity.Create)),
                    rich: static (state, rich) => Missing.Unless<AnnotationBase>(
                        TextEntity.CreateWithRichText(rich.RichText, state.Form.Plane, state.Parent, state.Form.Wrapped, state.Form.RectWidth, state.Form.RotationRadians),
                        nameof(TextEntity.CreateWithRichText))),
                leader: static (parent, leader) => leader.Content.Switch(
                    (Parent: parent, Form: leader),
                    plain: static (state, plain) => Missing.Unless<AnnotationBase>(Leader.Create(plain.Text, state.Form.Plane, state.Parent, [.. state.Form.Points]), nameof(Leader.Create)),
                    rich: static (state, rich) => Missing.Unless<AnnotationBase>(Leader.CreateWithRichText(rich.RichText, state.Form.Plane, state.Parent, [.. state.Form.Points]), nameof(Leader.CreateWithRichText))))),
            annotation =>
                from ids in TableOps.Apply(doc, new TableOp.Add(Seq(new GeometryPair(annotation, attributes)), history, reference))
                from id in IO.lift(() => ids.Head.ToFin(new Missing(nameof(TableOps.Apply))))
                select id);

    // --- [EDITS]
    public static IO<Unit> Modify(RhinoDoc doc, Guid id, Seq<RunEdit> edits) =>
        RhinoObjects.ReplaceGeometry<AnnotationBase>(doc, id, annotation => edits.TraverseM(edit => Applied(annotation, edit)).As().Map(static _ => unit));

    private static IO<Unit> Applied(AnnotationBase annotation, RunEdit edit) =>
        IO.lift(() => edit.Switch(
            annotation,
            replace: static (target, replace) => Refused.Unless(
                target.RunReplace(replace.ReplaceString, replace.StartRunIndex, replace.StartRunPosition, replace.EndRunIndex, replace.EndRunPosition),
                nameof(AnnotationBase.RunReplace)),
            bold: static (target, bold) => Refused.Unless(target.SetBold(bold.On), nameof(AnnotationBase.SetBold)),
            italic: static (target, italic) => Refused.Unless(target.SetItalic(italic.On), nameof(AnnotationBase.SetItalic)),
            underline: static (target, underline) => Refused.Unless(target.SetUnderline(underline.On), nameof(AnnotationBase.SetUnderline)),
            facename: static (target, facename) => Refused.Unless(target.SetFacename(facename.On, facename.Name), nameof(AnnotationBase.SetFacename)),
            wrap: static (target, wrap) => {
                target.FormatWidth = wrap.FormatWidth;
                target.WrapText();
                return Fin.Succ(unit);
            },
            content: static (target, content) => {
                target.RichText = content.RichText;
                return Fin.Succ(unit);
            }));

    // --- [READS]
    public static IO<TextState> State(RhinoDoc doc, Guid id) =>
        from resolved in Queries.Resolve<AnnotationObjectBase, AnnotationBase>(doc, id)
        from state in IO.lift(() => Project(resolved.Geometry, resolved.Object.DisplayText, resolved.Object.HasMeasurableTextFields))
        select state;

    [MapProperty(nameof(AnnotationBase.PlainText), nameof(TextState.PlainText), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(AnnotationBase.PlainTextWithFields), nameof(TextState.PlainTextWithFields), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(AnnotationBase.RichText), nameof(TextState.RichText), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(@AnnotationBase.Font.FaceName), nameof(TextState.FontFaceName), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(@AnnotationBase.FirstCharFont.FaceName), nameof(TextState.FirstCharFontFaceName), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(@AnnotationBase.FirstCharFont.Bold), nameof(TextState.FirstCharFontBold), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(@AnnotationBase.FirstCharFont.Italic), nameof(TextState.FirstCharFontItalic), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(@AnnotationBase.FirstCharFont.Underlined), nameof(TextState.FirstCharFontUnderlined), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(@AnnotationBase.FirstCharFont.Strikeout), nameof(TextState.FirstCharFontStrikeout), SuppressNullMismatchDiagnostic = true)]
    [MapPropertyFromSource(nameof(TextState.Bounds), Use = nameof(Bounds))]
    [MapPropertyFromSource(nameof(TextState.IsAllBold), Use = nameof(IsAllBold))]
    [MapPropertyFromSource(nameof(TextState.IsAllItalic), Use = nameof(IsAllItalic))]
    [MapPropertyFromSource(nameof(TextState.IsAllUnderlined), Use = nameof(IsAllUnderlined))]
    [MapPropertyFromSource(nameof(TextState.Overridden), Use = nameof(Overridden))]
    private static partial TextState Project(AnnotationBase annotation, string displayText, bool hasMeasurableTextFields);

    private static BoundingBox Bounds(AnnotationBase annotation) =>
        annotation.GetBoundingBox(accurate: true);

    private static bool IsAllBold(AnnotationBase annotation) =>
        annotation.IsAllBold();

    private static bool IsAllItalic(AnnotationBase annotation) =>
        annotation.IsAllItalic();

    private static bool IsAllUnderlined(AnnotationBase annotation) =>
        annotation.IsAllUnderlined();

    private static Seq<DimensionStyle.Field> Overridden(AnnotationBase annotation) =>
        DimensionStyles.Fields(annotation.IsPropertyOverridden);

    public static IO<LeaderState> ReadLeader(RhinoDoc doc, Guid id) =>
        from resolved in Queries.Resolve<RhinoObject, Leader>(doc, id)
        from state in IO.lift(() => Project(resolved.Geometry))
        select state;

    [MapPropertyFromSource(nameof(LeaderState.Points2D), Use = nameof(Points2D))]
    [MapPropertyFromSource(nameof(LeaderState.Points3D), Use = nameof(Points3D))]
    private static partial LeaderState Project(Leader leader);

    private static Seq<Point2d> Points2D(Leader leader) =>
        toSeq(leader.Points2D);

    private static Seq<Point3d> Points3D(Leader leader) =>
        toSeq(leader.Points3D);

    public static IO<(string Text, Seq<(int Run, int Start, int Length)> Runs)> ReadRunMap(RhinoDoc doc, Guid id) =>
        from resolved in Queries.Resolve<RhinoObject, AnnotationBase>(doc, id)
        from mapped in IO.lift(() => {
            int[] map = [];
            string text = resolved.Geometry.GetPlainTextWithRunMap(ref map);
            return toSeq(map.Chunk(3))
                .TraverseM(static triple => triple is [int run, int start, int length]
                    ? (Fin<(int Run, int Start, int Length)>)(run, start, length)
                    : new Mismatch(nameof(AnnotationBase.GetPlainTextWithRunMap)))
                .As()
                .Map(runs => (Text: text, Runs: runs));
        })
        select mapped;

    public static IO<TValue> WithOutline<TValue>(RhinoDoc doc, Guid id, OutlineOptions options, Func<Seq<Seq<GeometryBase>>, IO<TValue>> body) =>
        from resolved in Queries.Resolve<RhinoObject, TextEntity>(doc, id)
        from valid in IO.lift(() => Invalid.Unless(options.PreTransform.ForAll(static xform => xform.IsValid), nameof(Transform.IsValid)))
        from value in Disposal.Using(() => resolved.Geometry.DimensionStyle, style => Disposal.Bracketed(
            options.PreTransform.Match(
                Some: xform => GeometryOps.WithGeometry(resolved.Geometry, DuplicateMode.Duplicate, copy => IO.lift(
                    from moved in Refused.Unless(copy.Transform(xform, style), nameof(TextEntity.Transform))
                    select Outlined(copy, style, options))),
                None: () => IO.lift(() => Outlined(resolved.Geometry, style, options))),
            static groups => Disposal.Release(groups.Flatten()),
            body))
        select value;

    private static Seq<Seq<GeometryBase>> Outlined(TextEntity text, DimensionStyle style, OutlineOptions options) =>
        options.Kind.Switch(
            (Text: text, Style: style, Options: options),
            curves: static (state, curves) =>
                Groups(state.Text.CreateCurvesGrouped(state.Style, curves.AllowOpen, state.Options.MakeSmallCaps, state.Options.SmallCapsScale, state.Options.Spacing)),
            surfaces: static (state, _) =>
                Groups(state.Text.CreateSurfacesGrouped(state.Style, state.Options.MakeSmallCaps, state.Options.SmallCapsScale, state.Options.Spacing)),
            polySurfaces: static (state, solids) =>
                Groups(state.Text.CreatePolysurfacesGrouped(state.Style, state.Options.MakeSmallCaps, state.Options.SmallCapsScale, solids.Height, state.Options.Spacing)),
            extrusions: static (state, extrusions) =>
                Groups(state.Text.CreateExtrusionsGrouped(state.Style, state.Options.MakeSmallCaps, state.Options.SmallCapsScale, extrusions.Height, state.Options.Spacing)));

    private static Seq<Seq<GeometryBase>> Groups<T>(List<T[]> groups) where T : GeometryBase =>
        toSeq(groups).Map(static group => toSeq<GeometryBase>(group)).Strict();

    public static IO<Seq<Point3d>> TextCorners(RhinoDoc doc, Guid id, ViewportTarget target) =>
        from resolved in Queries.Resolve<TextObject, TextEntity>(doc, id)
        from row in Viewports.ResolveViewport(doc, target)
        from corners in IO.lift(() => Answers.NonEmpty(toSeq(resolved.Object.GetTextCorners(row.Viewport)), nameof(TextObject.GetTextCorners)))
        select corners;

    public static IO<double> DimensionScale(RhinoDoc doc, DimensionStyle style, ViewportTarget target) =>
        from row in Viewports.ResolveViewport(doc, target)
        from scale in IO.lift(() => AnnotationBase.GetDimensionScale(doc, style, row.Viewport))
        select scale;

    // --- [FIELDS]
    public static IO<string> FormatFields(RhinoDoc doc, string text) =>
        IO.lift(() => Refused.Unless(TextFields.TryFormat(text, doc, out string result), result, nameof(TextFields.TryFormat)));

    public static IO<Seq<string>> ParseFields(RhinoDoc doc, string text) =>
        IO.lift(() => Refused.Unless(TextFields.TryParse(text, doc, out List<string> tokens), toSeq(tokens), nameof(TextFields.TryParse)));
}

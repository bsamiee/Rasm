using System.Runtime.CompilerServices;
using LanguageExt.ClassInstances;
using Rasm.Drafting;
using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Objects;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Annotation.Styles;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record FontQuery {
    // --- [CASES]
    public sealed record Quartet(string QuartetName, bool Bold, bool Italic) : FontQuery;
    public sealed record Properties(string FamilyName, Font.FontWeight Weight, Font.FontStyle Style, Font.FontStretch Stretch, bool Underlined, bool Strikethrough) : FontQuery;
    public sealed record RichText(string RichTextFontName, bool Bold, bool Italic, bool Underlined, bool Strikethrough) : FontQuery;

    // --- [RESOLUTION]
    public IO<Font> Resolve() => Switch(
        quartet: static face => IO.lift(() => Missing.Unless(Font.FromQuartetProperties(face.QuartetName, face.Bold, face.Italic), nameof(Font.FromQuartetProperties))),
        properties: static face => IO.lift(() => new Font(face.FamilyName, face.Weight, face.Style, face.Stretch, face.Underlined, face.Strikethrough)),
        richText: static face => IO.lift(() => Missing.Unless(
            Font.FromRichTextProperties(face.RichTextFontName, face.Bold, face.Italic, face.Underlined, face.Strikethrough), nameof(Font.FromRichTextProperties))));
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record DimensionStyleSource {
    public sealed record BuiltIn(string Name) : DimensionStyleSource;

    public sealed record Row(ComponentRef<DimensionStyle> Address) : DimensionStyleSource;
}

public sealed record DimensionStyleSpec(
    Option<DimensionStyleSource> Source,
    Option<ComponentRef<DimensionStyle>> Parent,
    Option<StyleMetrics> Lettering,
    Option<FontQuery> Font,
    HashMap<EqStringOrdinalIgnoreCase, string, string> UserStrings,
    Option<Func<DimensionStyle, IO<Unit>>> Edit);

public sealed record DimensionStyleRow(
    Option<Guid> ParentId,
    Seq<DimensionStyle.Field> Overridden,
    UnitSystem DimensionLengthDisplayUnit,
    UnitSystem AlternateDimensionLengthDisplayUnit,
    HashMap<EqStringOrdinalIgnoreCase, string, string> UserStrings);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DimensionStyles {
    // --- [FIELDS]
    private static readonly Seq<DimensionStyle.Field> Fields =
        toSeq(Enum.GetValues<DimensionStyle.Field>()).Filter(static field => field is not (DimensionStyle.Field.Unset or DimensionStyle.Field.Count)).Strict();

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern double GetDouble(DimensionStyle style, DimensionStyle.Field field);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern void SetDouble(DimensionStyle style, DimensionStyle.Field field, double value);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern int GetInt(DimensionStyle style, DimensionStyle.Field field);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern void SetInt(DimensionStyle style, DimensionStyle.Field field, int value);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern bool GetBool(DimensionStyle style, DimensionStyle.Field field);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern string GetString(DimensionStyle style, DimensionStyle.Field field);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern System.Drawing.Color GetColor(DimensionStyle style, DimensionStyle.Field field);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern Guid GetGuid(DimensionStyle style, DimensionStyle.Field field);

    internal static Seq<DimensionStyle.Field> Overrides(Func<DimensionStyle.Field, bool> marked) =>
        Fields.Filter(marked).Strict();

    public static IO<bool> Unchanged(DimensionStyle live, DimensionStyle staged) =>
        IO.lift(() =>
            string.Equals(live.Name, staged.Name, StringComparison.Ordinal)
            && live.ParentId == staged.ParentId
            && SameFont(live.Font, staged.Font)
            && (live.DecimalSeparator, live.FitText, live.FitArrow) == (staged.DecimalSeparator, staged.FitText, staged.FitArrow)
            && UserStrings.Held(live.GetUserStrings()) == UserStrings.Held(staged.GetUserStrings())
            && Fields.ForAll(field =>
                (!staged.IsChild || live.IsFieldOverriden(field) == staged.IsFieldOverriden(field))
                && GetDouble(live, field).Equals(GetDouble(staged, field))
                && (GetInt(live, field), GetBool(live, field), GetString(live, field), GetGuid(live, field))
                    == (GetInt(staged, field), GetBool(staged, field), GetString(staged, field), GetGuid(staged, field))
                && Conversions.Same(GetColor(live, field), GetColor(staged, field))));

    private static bool SameFont(Font left, Font right) =>
        (left.PostScriptName, left.RichTextFontName, left.Weight, left.Style, left.Stretch, left.Underlined, left.Strikeout, left.PointSize)
            == (right.PostScriptName, right.RichTextFontName, right.Weight, right.Style, right.Stretch, right.Underlined, right.Strikeout, right.PointSize);

    // --- [EFFECTIVE]
    public static IO<DimensionStyle> Parent(RhinoDoc doc, AnnotationBase annotation) =>
        IO.lift(() => Missing.Unless(doc.DimStyles.Find(annotation.DimensionStyleId, ignoreDeleted: false), nameof(DimStyleTable.Find)));

    public static IO<DimensionStyle> Effective(RhinoDoc doc, AnnotationBase annotation, Option<DimensionStyle> parent) =>
        from held in parent.Match(Some: static style => IO.pure(style), None: () => Parent(doc, annotation))
        from effective in IO.lift(() => Missing.Unless(annotation.GetDimensionStyle(held), nameof(AnnotationBase.GetDimensionStyle)))
        select effective;

    public static IO<Unit> Edit<T>(RhinoDoc doc, Guid id, Func<T, IO<Unit>> edit) where T : AnnotationBase =>
        RhinoObjects.ReplaceGeometry(doc, id, (T copy) =>
            from parent in Parent(doc, copy)
            from bound in IO.lift(() => { copy.ParentDimensionStyle = parent; })
            from edited in edit(copy)
            select edited);

    // --- [READS]
    public static IO<DimensionStyleRow> Row(RhinoDoc doc, DimensionStyle style) =>
        IO.lift(() => new DimensionStyleRow(
            Conversions.Present(style.ParentId),
            Overrides(style.IsFieldOverriden),
            style.DimensionLengthDisplayUnit(doc.RuntimeSerialNumber),
            style.AlternateDimensionLengthDisplayUnit(doc.RuntimeSerialNumber),
            UserStrings.Held(style.GetUserStrings())));

    // --- [AUTHORING]
    public static IO<Unit> Written(RhinoDoc doc, DimensionStyle staged, DimensionStyleSpec spec) =>
        from copied in spec.Source.Traverse(source => Copied(doc, staged, source)).As()
        from parented in spec.Parent.Traverse(parent =>
            from found in TableOps.Find(doc.DimStyles, parent, includeDeleted: false)
            from assigned in IO.lift(() => { staged.ParentId = found.Id; })
            select assigned).As()
        from lettered in spec.Lettering.Traverse(metrics => Lettered(doc, staged, metrics)).As()
        from font in spec.Font.Traverse(static query => query.Resolve()).As()
        from faced in IO.lift(() => font.Filter(wanted => !SameFont(staged.Font, wanted)).Iter(wanted => staged.Font = wanted))
        from held in IO.lift(() => UserStrings.Held(staged.GetUserStrings()))
        from strung in UserStrings.Write(held, staged.SetUserString, UserStrings.Replacing(held, spec.UserStrings))
        from edited in spec.Edit.Traverse(edit => edit(staged)).As()
        select unit;

    private static IO<Unit> Copied(RhinoDoc doc, DimensionStyle staged, DimensionStyleSource source) =>
        source.Switch(
            (Doc: doc, Staged: staged),
            builtIn: static (state, builtIn) =>
                (from styles in use(() => Conversions.Rows(state.Doc.DimStyles.BuiltInStyles), DisposalOps.Release)
                 from found in IO.lift(styles.Find(style => TableOps.Names<DimensionStyle>().Equals(style.Name, builtIn.Name)).ToFin(new Missing(nameof(DimStyleTable.BuiltInStyles))))
                 from copied in IO.lift(() => state.Staged.CopyFrom(found))
                 select copied).Bracket(),
            row: static (state, row) =>
                from found in TableOps.Find(state.Doc.DimStyles, row.Address, includeDeleted: false)
                from copied in IO.lift(() => state.Staged.CopyFrom(found))
                select copied);

    private static IO<Unit> Lettered(RhinoDoc doc, DimensionStyle staged, StyleMetrics metrics) =>
        IO.lift(() => {
            (LengthUnit page, LengthUnit model) = (doc.PageUnits, doc.ModelUnits);
            int arrow = (int)metrics.Terminator.Map(
                closedArrow: DimensionStyle.ArrowType.SolidTriangle,
                openArrow: DimensionStyle.ArrowType.OpenArrow,
                oblique: DimensionStyle.ArrowType.Tick,
                dot: DimensionStyle.ArrowType.Dot);
            Seq<(DimensionStyle.Field Field, Length Value, LengthUnit Unit)> lengths = [
                (DimensionStyle.Field.TextHeight, metrics.TextHeight, page),
                (DimensionStyle.Field.TextGap, metrics.TextGap, page),
                (DimensionStyle.Field.BaselineSpacing, metrics.BaselineSpacing, page),
                (DimensionStyle.Field.MaskBorder, metrics.MaskOffset, page),
                (DimensionStyle.Field.Arrowsize, metrics.ArrowLength, page),
                (DimensionStyle.Field.LeaderArrowsize, metrics.ArrowLength, page),
                (DimensionStyle.Field.ClippingArrowSize, metrics.ArrowLength, page),
                (DimensionStyle.Field.ExtensionLineOffset, metrics.ExtensionLineOffset, page),
                (DimensionStyle.Field.ExtensionLineExtension, metrics.ExtensionLineExtension, page),
                (DimensionStyle.Field.LeaderLandingLength, metrics.LeaderLandingLength, page),
                (DimensionStyle.Field.DimensionScale, metrics.Scale.ToModel(Quantities.From(1d, page)), model)];
            Seq<(DimensionStyle.Field Field, double Value)> ratios = [
                (DimensionStyle.Field.LineSpaceScale, metrics.LineSpaceScale.ToDouble()),
                (DimensionStyle.Field.StackTextheightScale, metrics.StackHeightScale.ToDouble())];
            Seq<(DimensionStyle.Field Field, int Value)> codes = [
                (DimensionStyle.Field.ArrowType1, arrow),
                (DimensionStyle.Field.ArrowType2, arrow),
                (DimensionStyle.Field.LengthResolution, metrics.LengthResolution),
                (DimensionStyle.Field.AlternateLengthResolution, metrics.AlternateLengthResolution),
                (DimensionStyle.Field.DimensionLengthDisplay, (int)DimensionStyle.LengthDisplay.InchesFractional),
                (DimensionStyle.Field.AlternateDimensionLengthDisplay, (int)DimensionStyle.LengthDisplay.Millmeters),
                (DimensionStyle.Field.UnitSystem, (int)doc.PageUnitSystem)];
            lengths.Filter(row => Quantities.From(GetDouble(staged, row.Field), row.Unit) != row.Value).Iter(row => SetDouble(staged, row.Field, Quantities.As(row.Value, row.Unit)));
            ratios.Filter(row => !GetDouble(staged, row.Field).Equals(row.Value)).Iter(row => SetDouble(staged, row.Field, row.Value));
            codes.Filter(row => GetInt(staged, row.Field) != row.Value).Iter(row => SetInt(staged, row.Field, row.Value));
        });

    // --- [OVERRIDES]
    public static IO<Unit> SetOverride(RhinoDoc doc, AnnotationBase annotation, Func<DimensionStyle, IO<Unit>> edit) =>
        (from effective in use(Effective(doc, annotation, None))
         from child in use(IO.lift(() => Callbacks.Thrown<InvalidOperationException, DimensionStyle>(
             () => effective.Duplicate("", Guid.Empty, annotation.DimensionStyleId), nameof(DimensionStyle.Duplicate))))
         from edited in edit(child)
         from applied in IO.lift(() => Refused.Unless(annotation.SetOverrideDimStyle(child), nameof(AnnotationBase.SetOverrideDimStyle)))
         select applied).Bracket();

    public static IO<Unit> ClearOverrides(AnnotationBase annotation) =>
        IO.lift(() => annotation.HasPropertyOverrides ? Refused.Unless(annotation.ClearPropertyOverrides(), nameof(AnnotationBase.ClearPropertyOverrides)) : unit);

    public static IO<Unit> SaveOverrides(RhinoDoc doc, Guid annotationId, bool modifyParent) =>
        from resolved in ObjectTarget.Resolve<RhinoObject, AnnotationBase>(doc, annotationId)
        from saved in when(resolved.Geometry.HasPropertyOverrides,
            (from parent in Parent(doc, resolved.Geometry)
             let modify = modifyParent || parent.IsChild
             from effective in use(Effective(doc, resolved.Geometry, Some(parent)))
             from staged in use(IO.lift(() => Callbacks.Thrown<InvalidOperationException, DimensionStyle>(
                 () => effective.Duplicate(modify ? parent.Name : "", modify ? parent.Id : Guid.Empty, modify ? parent.ParentId : parent.Id), nameof(DimensionStyle.Duplicate))))
             from index in IO.lift(() => modify
                 ? Refused.Unless(doc.DimStyles.Modify(staged, parent.Index, quiet: true), parent.Index, nameof(DimStyleTable.Modify))
                 : Conversions.Required(doc.DimStyles.Add(staged, reference: false), nameof(DimStyleTable.Add)))
             from replaced in Edit<AnnotationBase>(doc, annotationId, copy =>
                from bound in IO.lift(() => { copy.DimensionStyleId = doc.DimStyles[index].Id; })
                from cleared in ClearOverrides(copy)
                select cleared)
             select replaced).Bracket()).As()
        select saved;

    public static IO<Unit> SetCurrent(RhinoDoc doc, ComponentRef<DimensionStyle> address) =>
        from style in TableOps.Find(doc.DimStyles, address, includeDeleted: false)
        from current in IO.lift(() => doc.DimStyles.CurrentId == style.Id ? unit : Refused.Unless(doc.DimStyles.SetCurrent(style.Index, quiet: true), nameof(DimStyleTable.SetCurrent)))
        select current;
}

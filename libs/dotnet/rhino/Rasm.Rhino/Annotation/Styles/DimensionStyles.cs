using System.Runtime.CompilerServices;
using LanguageExt.ClassInstances;
using Rasm.Drafting;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Objects;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.DocObjects;

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
        quartet: static face => IO.lift(() => Conversions.Found(Font.FromQuartetProperties(face.QuartetName, face.Bold, face.Italic))),
        properties: static face => IO.lift(() => new Font(face.FamilyName, face.Weight, face.Style, face.Stretch, face.Underlined, face.Strikethrough)),
        richText: static face => IO.lift(() => Conversions.Found(Font.FromRichTextProperties(face.RichTextFontName, face.Bold, face.Italic, face.Underlined, face.Strikethrough))));
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
    private static extern int GetInt(DimensionStyle style, DimensionStyle.Field field);

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
    public static IO<DimensionStyle> Resolve(RhinoDoc doc, Option<ComponentRef<DimensionStyle>> style) =>
        style.Match(
            Some: address => TableOps.Find(doc.DimStyles, address, includeDeleted: false),
            None: () => IO.lift(() => doc.DimStyles.Current));

    public static IO<DimensionStyle> Parent(RhinoDoc doc, AnnotationBase annotation) =>
        IO.lift(() => Conversions.Found(doc.DimStyles.Find(annotation.DimensionStyleId, ignoreDeleted: false)));

    public static IO<DimensionStyle> Effective(RhinoDoc doc, AnnotationBase annotation, Option<DimensionStyle> parent) =>
        from held in parent.Match(Some: static style => IO.pure(style), None: () => Parent(doc, annotation))
        from effective in Products.Made(annotation.GetDimensionStyle(held))
        select effective;

    public static IO<Unit> Edit<T>(RhinoDoc doc, Guid id, Func<T, IO<Unit>> edit) where T : AnnotationBase =>
        RhinoObjects.ReplaceGeometry(doc, id, (T target) =>
            from parent in Parent(doc, target)
            from bound in IO.lift(() => { target.ParentDimensionStyle = parent; })
            from edited in edit(target)
            select (target, edited));

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
                (from styles in Products.Made(state.Doc.DimStyles.BuiltInStyles)
                 from found in IO.lift(() => styles.Find(style => TableOps.Names<DimensionStyle>().Equals(style.Name, builtIn.Name))
                     .ToFin(new RhinoFault(builtIn.Name, RhinoReason.Missing)))
                 from copied in IO.lift(() => state.Staged.CopyFrom(found))
                 select copied).Bracket(),
            row: static (state, row) =>
                from found in TableOps.Find(state.Doc.DimStyles, row.Address, includeDeleted: false)
                from copied in IO.lift(() => state.Staged.CopyFrom(found))
                select copied);

    private static IO<Unit> Lettered(RhinoDoc doc, DimensionStyle staged, StyleMetrics metrics) =>
        IO.lift(() => {
            double Page(Length length) => Quantities.As(length, doc.PageUnits);
            (DimensionStyle.ArrowType arrow, double arrowLength) = (
                metrics.Terminator.Map(closedArrow: DimensionStyle.ArrowType.SolidTriangle, openArrow: DimensionStyle.ArrowType.OpenArrow, oblique: DimensionStyle.ArrowType.Tick, dot: DimensionStyle.ArrowType.Dot), Page(metrics.ArrowLength));
            (staged.TextHeight, staged.TextGap, staged.BaselineSpacing, staged.MaskOffset) = (Page(metrics.TextHeight), Page(metrics.TextGap), Page(metrics.BaselineSpacing), Page(metrics.MaskOffset));
            (staged.ArrowLength, staged.LeaderArrowLength, staged.ClippingArrowLength) = (arrowLength, arrowLength, arrowLength);
            (staged.ExtensionLineOffset, staged.ExtensionLineExtension, staged.LeaderLandingLength) = (Page(metrics.ExtensionLineOffset), Page(metrics.ExtensionLineExtension), Page(metrics.LeaderLandingLength));
            (staged.LineSpaceScale, staged.StackHeightScale) = (metrics.LineSpaceScale.ToDouble(), metrics.StackHeightScale.ToDouble());
            (staged.ArrowType1, staged.ArrowType2, staged.UnitSystem) = (arrow, arrow, doc.PageUnitSystem);
            staged.DimensionScale = Quantities.As(metrics.Scale.ToModel(Quantities.From(1d, doc.PageUnits)), doc.ModelUnits);
            (staged.LengthResolution, staged.AlternateLengthResolution) = (
                Precision(metrics.Resolution, staged.DimensionLengthDisplay, staged.DimensionLengthDisplayUnit(doc.RuntimeSerialNumber)),
                Precision(metrics.Resolution, staged.AlternateDimensionLengthDisplay, staged.AlternateDimensionLengthDisplayUnit(doc.RuntimeSerialNumber)));
        });

    private static int Precision(Length resolution, DimensionStyle.LengthDisplay display, UnitSystem system) =>
        (display is DimensionStyle.LengthDisplay.InchesFractional or DimensionStyle.LengthDisplay.FeetAndInches ? (Place: Length.FromInches(1), Radix: 2) : (Place: Quantities.From(1d, LengthUnit.FromKnownUnitSystem(system)), Radix: 10)) switch {
            var (place, radix) => LanguageExt.List.unfold(place, held => held > resolution ? Some((held, held / radix)) : None).Count(),
        };

    // --- [OVERRIDES]
    public static IO<Unit> SetOverride(RhinoDoc doc, AnnotationBase annotation, Func<DimensionStyle, IO<Unit>> edit) =>
        (from effective in Effective(doc, annotation, None)
         from applied in Overridden(annotation, effective, edit)
         select applied).Bracket();

    public static IO<Unit> ClearOverrides(AnnotationBase annotation) =>
        IO.lift(() => annotation.HasPropertyOverrides ? RhinoFault.Unless(annotation.ClearPropertyOverrides()) : unit);

    public static IO<Unit> SaveOverrides(RhinoDoc doc, Guid annotationId, bool modifyParent) =>
        from resolved in ObjectTarget.Resolve<RhinoObject, AnnotationBase>(doc, annotationId)
        from saved in when(resolved.Geometry.HasPropertyOverrides,
            from parent in Parent(doc, resolved.Geometry)
            let modify = modifyParent || parent.IsChild
            from effective in Effective(doc, resolved.Geometry, Some(parent))
            from replaced in (from staged in use(() => effective.Duplicate(modify ? parent.Name : "", modify ? parent.Id : Guid.Empty, modify ? parent.ParentId : parent.Id))
                              from index in IO.lift(() => modify
                                  ? RhinoFault.Unless(doc.DimStyles.Modify(staged, parent.Index, quiet: true)).Map(_ => parent.Index)
                                  : Conversions.Found(Conversions.Present(doc.DimStyles.Add(staged, reference: false))))
                              from replaced in Edit<AnnotationBase>(doc, annotationId, copy => IO.lift(() => copy.DimensionStyleId = doc.DimStyles[index].Id).Bind(_ => ClearOverrides(copy)))
                              select replaced).Bracket()
            from released in release(effective)
            select released).As()
        select saved;

    public static IO<Unit> SetCurrent(RhinoDoc doc, ComponentRef<DimensionStyle> address) =>
        from style in TableOps.Find(doc.DimStyles, address, includeDeleted: false)
        from current in IO.lift(() => doc.DimStyles.CurrentId == style.Id ? unit : RhinoFault.Unless(doc.DimStyles.SetCurrent(style.Index, quiet: true)))
        select current;

    private static IO<Unit> Overridden(AnnotationBase annotation, DimensionStyle effective, Func<DimensionStyle, IO<Unit>> edit) =>
        (from child in use(() => effective.Duplicate("", Guid.Empty, annotation.DimensionStyleId))
         from edited in edit(child)
         from assigned in IO.lift(() => RhinoFault.Unless(annotation.SetOverrideDimStyle(child)))
         select assigned).Bracket();
}

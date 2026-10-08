using System.Runtime.CompilerServices;
using LanguageExt.ClassInstances;
using Rasm.Drafting;
using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Objects;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Annotation.Styles;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FontQuery {
    public abstract IO<Font> Resolve();

    public sealed record Quartet(string QuartetName, bool Bold, bool Italic) : FontQuery {
        public override IO<Font> Resolve() =>
            IO.lift(() => Missing.Unless(Font.FromQuartetProperties(QuartetName, Bold, Italic), nameof(Font.FromQuartetProperties)));
    }

    public sealed record Properties(string FamilyName, Font.FontWeight Weight, Font.FontStyle Style, Font.FontStretch Stretch, bool Underlined, bool Strikethrough) : FontQuery {
        public override IO<Font> Resolve() =>
            IO.lift(() => new Font(FamilyName, Weight, Style, Stretch, Underlined, Strikethrough));
    }

    public sealed record RichText(string RichTextFontName, bool Bold, bool Italic, bool Underlined, bool Strikethrough) : FontQuery {
        public override IO<Font> Resolve() =>
            IO.lift(() => Missing.Unless(
                Font.FromRichTextProperties(RichTextFontName, Bold, Italic, Underlined, Strikethrough), nameof(Font.FromRichTextProperties)));
    }
}

[Union]
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
[Mapper]
public static partial class FontMapper {
    [MapProperty(nameof(Font.QuartetName), nameof(FontQuery.Quartet.QuartetName), SuppressNullMismatchDiagnostic = true)]
    public static partial FontQuery.Quartet ToState(Font font);
}

public static class DimensionStyles {
    // --- [FIELDS]
    private static readonly Seq<DimensionStyle.Field> Fields =
        toSeq(Enum.GetValues<DimensionStyle.Field>()).Filter(static field => field is not (DimensionStyle.Field.Unset or DimensionStyle.Field.Count)).Strict();

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetDouble")]
    private static extern double GetDouble(DimensionStyle style, DimensionStyle.Field field);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "SetDouble")]
    private static extern void SetDouble(DimensionStyle style, DimensionStyle.Field field, double value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetInt")]
    private static extern int GetInt(DimensionStyle style, DimensionStyle.Field field);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "SetInt")]
    private static extern void SetInt(DimensionStyle style, DimensionStyle.Field field, int value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetBool")]
    private static extern bool GetBool(DimensionStyle style, DimensionStyle.Field field);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetString")]
    private static extern string GetString(DimensionStyle style, DimensionStyle.Field field);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetColor")]
    private static extern System.Drawing.Color GetColor(DimensionStyle style, DimensionStyle.Field field);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetGuid")]
    private static extern Guid GetGuid(DimensionStyle style, DimensionStyle.Field field);

    internal static Seq<DimensionStyle.Field> Overrides(Func<DimensionStyle.Field, bool> marked) =>
        Fields.Filter(marked).Strict();

    public static IO<bool> Unchanged(DimensionStyle live, DimensionStyle staged) =>
        IO.lift(() =>
            string.Equals(live.Name, staged.Name, StringComparison.Ordinal)
            && live.ParentId == staged.ParentId
            && FontMapper.ToState(live.Font) == FontMapper.ToState(staged.Font)
            && (live.DecimalSeparator, live.FitText, live.FitArrow) == (staged.DecimalSeparator, staged.FitText, staged.FitArrow)
            && UserStrings.Held(live.GetUserStrings()) == UserStrings.Held(staged.GetUserStrings())
            && Fields.ForAll(field =>
                live.IsFieldOverriden(field) == staged.IsFieldOverriden(field)
                && GetDouble(live, field).Equals(GetDouble(staged, field))
                && (GetInt(live, field), GetBool(live, field), GetString(live, field), GetGuid(live, field))
                    == (GetInt(staged, field), GetBool(staged, field), GetString(staged, field), GetGuid(staged, field))
                && Conversions.Same(GetColor(live, field), GetColor(staged, field))));

    // --- [EFFECTIVE]
    public static IO<DimensionStyle> Parent(RhinoDoc doc, AnnotationBase annotation) =>
        IO.lift(() => Missing.Unless(doc.DimStyles.Find(annotation.DimensionStyleId, ignoreDeleted: false), nameof(DimStyleTable.Find)));

    public static IO<A> Effective<A>(RhinoDoc doc, AnnotationBase annotation, Option<DimensionStyle> parent, Func<DimensionStyle, IO<A>> read) =>
        from held in parent.Match(Some: static style => IO.pure(style), None: () => Parent(doc, annotation))
        from value in use(IO.lift(() => Missing.Unless(annotation.GetDimensionStyle(held), nameof(AnnotationBase.GetDimensionStyle)))).Bind(read).Bracket()
        select value;

    public static IO<Unit> Edit<T>(RhinoDoc doc, Guid id, Func<T, IO<Unit>> edit) where T : AnnotationBase =>
        RhinoObjects.ReplaceGeometry<T>(doc, id, copy =>
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

    public static IO<A> BuiltIns<A>(RhinoDoc doc, Func<Seq<DimensionStyle>, IO<A>> read) =>
        IO.lift(() => Conversions.Rows(doc.DimStyles.BuiltInStyles)).Bracket(Use: read, Fin: DisposalOps.Release);

    // --- [AUTHORING]
    public static IO<Unit> Written(RhinoDoc doc, DimensionStyle staged, DimensionStyleSpec spec) =>
        from copied in spec.Source.Traverse(source => Copied(doc, staged, source)).As()
        from parented in spec.Parent.Traverse(parent =>
            TableOps.Find(doc.DimStyles, parent, includeDeleted: false).Bind(found => IO.lift(() => { staged.ParentId = found.Id; }))).As()
        from lettered in spec.Lettering.Traverse(metrics => Lettered(doc, staged, metrics)).As()
        from font in spec.Font.Traverse(static query => query.Resolve()).As()
        from faced in IO.lift(() => font.Filter(wanted => FontMapper.ToState(staged.Font) != FontMapper.ToState(wanted)).Iter(wanted => staged.Font = wanted))
        from held in IO.lift(() => UserStrings.Held(staged.GetUserStrings()))
        from strung in UserStrings.Write(held, staged.SetUserString, UserStrings.Replacing(held, spec.UserStrings))
        from edited in spec.Edit.Traverse(edit => edit(staged)).As()
        select unit;

    private static IO<Unit> Copied(RhinoDoc doc, DimensionStyle staged, DimensionStyleSource source) =>
        source.Switch(
            (Doc: doc, Staged: staged),
            builtIn: static (state, builtIn) => BuiltIns(state.Doc, styles =>
                IO.lift(styles.Find(style => TableOps.Names<DimensionStyle>().Equals(style.Name, builtIn.Name)).ToFin(new Missing(nameof(DimStyleTable.BuiltInStyles))))
                    .Map(fun<DimensionStyle>(state.Staged.CopyFrom))),
            row: static (state, row) => TableOps.Find(state.Doc.DimStyles, row.Address, includeDeleted: false).Map(fun<DimensionStyle>(state.Staged.CopyFrom)));

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
        Effective(doc, annotation, None, effective =>
            (from child in use(IO.lift(() => Callbacks.Thrown<InvalidOperationException, DimensionStyle>(
                 () => effective.Duplicate("", Guid.Empty, annotation.DimensionStyleId), nameof(DimensionStyle.Duplicate))))
             from edited in edit(child)
             from applied in IO.lift(() => Refused.Unless(annotation.SetOverrideDimStyle(child), nameof(AnnotationBase.SetOverrideDimStyle)))
             select applied).Bracket());

    public static IO<Unit> ClearOverrides(AnnotationBase annotation) =>
        IO.lift(() => annotation.HasPropertyOverrides ? Refused.Unless(annotation.ClearPropertyOverrides(), nameof(AnnotationBase.ClearPropertyOverrides)) : unit);

    public static IO<Unit> ModifyFromAnnotation(RhinoDoc doc, Guid annotationId) =>
        from resolved in ObjectTarget.Resolve<RhinoObject, AnnotationBase>(doc, annotationId)
        from modified in when(resolved.Geometry.HasPropertyOverrides,
            TableOps.ModifyRow(doc, TableKinds.DimensionStyles, resolved.Geometry.DimensionStyleId, staged =>
                Effective(doc, resolved.Geometry, None, effective => IO.lift(() => {
                    Guid parent = staged.ParentId;
                    staged.CopyFrom(effective);
                    staged.ParentId = parent;
                })))
                .Bind(_ => Edit<AnnotationBase>(doc, annotationId, ClearOverrides))).As()
        select modified;

    public static IO<Unit> SaveOverrides(RhinoDoc doc, Guid annotationId) =>
        from resolved in ObjectTarget.Resolve<RhinoObject, AnnotationBase>(doc, annotationId)
        from saved in when(resolved.Geometry.HasPropertyOverrides,
            Edit<AnnotationBase>(doc, annotationId, copy =>
                Effective(doc, copy, None, effective => IO.lift(Fin<Unit> () =>
                    doc.DimStyles.Modify(effective, copy) switch {
                        ModifyType.Modify or ModifyType.Override => unit,
                        ModifyType.NotSaved => new InvalidAnswer(nameof(DimStyleTable.Modify)),
                    })))).As()
        select saved;

    public static IO<Unit> SetCurrent(RhinoDoc doc, ComponentRef<DimensionStyle> address) =>
        from style in TableOps.Find(doc.DimStyles, address, includeDeleted: false)
        from current in IO.lift(() => doc.DimStyles.CurrentId == style.Id ? unit : Refused.Unless(doc.DimStyles.SetCurrent(style.Index, quiet: true), nameof(DimStyleTable.SetCurrent)))
        select current;
}

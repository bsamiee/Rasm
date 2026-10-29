using LanguageExt.ClassInstances;
using Rasm.Drafting;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.DocObjects;

namespace Rasm.Rhino.Annotation.Styles;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record DimensionStyleSource {
    public sealed record BuiltIn(string Name) : DimensionStyleSource;

    public sealed record Row(ComponentRef<DimensionStyle> Address) : DimensionStyleSource;
}

public sealed record DimensionStyleSpec {
    public Option<DimensionStyleSource> Source { get; init; }
    public Option<Font> Font { get; init; }
    public Option<StyleMetrics> Lettering { get; init; }
    public HashMap<EqStringOrdinalIgnoreCase, string, string> UserStrings { get; init; }
    public Seq<Step<DimensionStyle>> Edits { get; init; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DimensionStyles {
    // --- [FIELDS]
    private static readonly Seq<DimensionStyle.Field> Fields =
        toSeq(Enum.GetValues<DimensionStyle.Field>()).Filter(static field => field is not (DimensionStyle.Field.Unset or DimensionStyle.Field.Count)).Strict();

    extension(DimensionStyle style) {
        public Seq<DimensionStyle.Field> Overridden => Fields.Filter(style.IsFieldOverriden).Strict();
    }

    extension(AnnotationBase annotation) {
        public Seq<DimensionStyle.Field> Overridden => Fields.Filter(annotation.IsPropertyOverridden).Strict();
    }

    public static IO<bool> Unchanged(DimensionStyle live, DimensionStyle staged) =>
        IO.lift(() =>
            live.ParentId == staged.ParentId
            && SameFont(live.Font, staged.Font)
            && (live.DecimalSeparator, live.FitText, live.FitArrow) == (staged.DecimalSeparator, staged.FitText, staged.FitArrow)
            && UserStrings.Of(live.GetUserStrings()) == UserStrings.Of(staged.GetUserStrings())
            && Fields.ForAll(field =>
                (!staged.IsChild || live.IsFieldOverriden(field) == staged.IsFieldOverriden(field))
                && DimensionStyleFields.GetDouble(live, field).Equals(DimensionStyleFields.GetDouble(staged, field))
                && (DimensionStyleFields.GetInt(live, field), DimensionStyleFields.GetBool(live, field), DimensionStyleFields.GetString(live, field), DimensionStyleFields.GetGuid(live, field))
                    == (DimensionStyleFields.GetInt(staged, field), DimensionStyleFields.GetBool(staged, field), DimensionStyleFields.GetString(staged, field), DimensionStyleFields.GetGuid(staged, field))
                && Conversions.Same(DimensionStyleFields.GetColor(live, field), DimensionStyleFields.GetColor(staged, field))));

    private static bool SameFont(Font left, Font right) =>
        (left.PostScriptName, left.RichTextFontName, left.Weight, left.Style, left.Stretch, left.Underlined, left.Strikeout, left.PointSize)
            == (right.PostScriptName, right.RichTextFontName, right.Weight, right.Style, right.Stretch, right.Underlined, right.Strikeout, right.PointSize);

    // --- [EFFECTIVE]
    public static IO<DimensionStyle> Resolve(RhinoDoc doc, Option<ComponentRef<DimensionStyle>> style) =>
        style.Match(
            Some: address => Rows.Find(doc.DimStyles, address),
            None: () => IO.lift(() => doc.DimStyles.Current));

    public static IO<DimensionStyle> Parent(RhinoDoc doc, AnnotationBase annotation) =>
        IO.lift(() => Conversions.Required(doc.DimStyles.Find(annotation.DimensionStyleId, ignoreDeleted: false)));

    public static IO<DimensionStyle> Effective(RhinoDoc doc, AnnotationBase annotation, Option<DimensionStyle> parent) =>
        from held in parent.Match(Some: static style => IO.pure(style), None: () => Parent(doc, annotation))
        from effective in Products.Of(() => annotation.GetDimensionStyle(held))
        select effective;

    // --- [AUTHORING]
    public static IO<Unit> Written(RhinoDoc doc, DimensionStyle staged, DimensionStyleSpec spec) =>
        from copied in spec.Source.Traverse(source => Copied(doc, staged, source)).As()
        from faced in IO.lift(() => spec.Font.Filter(wanted => !SameFont(staged.Font, wanted)).Iter(wanted => staged.Font = wanted))
        from lettered in spec.Lettering.Traverse(metrics => Lettered(doc, staged, metrics)).As()
        from strung in StyleTables.Strung(staged.GetUserStrings, staged.SetUserString, spec.UserStrings)
        from edited in Edits.Apply(Stages.InPlace<DimensionStyle>(), staged, spec.Edits)
        select unit;

    private static IO<Unit> Copied(RhinoDoc doc, DimensionStyle staged, DimensionStyleSource source) =>
        source.Switch(
            (Doc: doc, Staged: staged),
            builtIn: static (state, builtIn) =>
                (from styles in Products.Many(() => state.Doc.DimStyles.BuiltInStyles)
                 from found in IO.lift(() => styles.Find(style => Rows.Names<DimensionStyle>().Equals(style.Name, builtIn.Name)).ToFin(new RhinoFault(builtIn.Name, RhinoReason.Missing)))
                 from copied in IO.lift(() => state.Staged.CopyFrom(found))
                 select copied).Bracket(),
            row: static (state, row) =>
                from found in Rows.Find(state.Doc.DimStyles, row.Address)
                from copied in IO.lift(() => state.Staged.CopyFrom(found))
                select copied);

    private static IO<Unit> Lettered(RhinoDoc doc, DimensionStyle staged, StyleMetrics metrics) =>
        (from advance in Advance(staged)
         let modelPerPage = Quantities.As(metrics.Scale.ToModel(Quantities.From(1d, doc.PageUnits)), doc.ModelUnits)
         from paper in use(() => LengthValue.Create(1d, doc.ModelUnitSystem, LengthValue.StringFormat.CleanProperFraction))
         from model in use(() => LengthValue.Create(modelPerPage, doc.ModelUnitSystem, LengthValue.StringFormat.CleanProperFraction))
         from scale in use(() => ScaleValue.Create(paper, model, ScaleValue.ScaleStringFormat.RatioFormat))
         from written in IO.lift(() => {
             double Page(Length length) => Quantities.As(length, doc.PageUnits);
             (DimensionStyle.ArrowType arrow, double arrowLength) = (
                 metrics.Terminator.Map(closedArrow: DimensionStyle.ArrowType.SolidTriangle, openArrow: DimensionStyle.ArrowType.OpenArrow, oblique: DimensionStyle.ArrowType.Tick, dot: DimensionStyle.ArrowType.Dot),
                 Page(metrics.TerminatorLength));
             (staged.UnitSystem, staged.DimensionScaleValue) = (doc.PageUnitSystem, scale);
             (staged.TextHeight, staged.TextGap, staged.MaskOffset, staged.BaselineSpacing) =
                 (Page(metrics.Height), Page(metrics.CharacterSpacing), Page(metrics.CharacterSpacing), Page(metrics.DimensionLineSpacing));
             (staged.LineSpaceScale, staged.StackHeightScale) = ((metrics.BaselineSpacing / metrics.Height).ToDouble() / advance, (metrics.LowercaseHeight / metrics.Height).ToDouble());
             (staged.ArrowType1, staged.ArrowType2, staged.ArrowLength, staged.LeaderArrowLength, staged.ClippingArrowLength) = (arrow, arrow, arrowLength, arrowLength, arrowLength);
             (staged.ExtensionLineOffset, staged.ExtensionLineExtension, staged.LeaderLandingLength) =
                 (Page(metrics.ExtensionGap), Page(metrics.ExtensionOvershoot), Page(metrics.ReferenceLineLength));
             (staged.LengthResolution, staged.AlternateLengthResolution) = (
                 Digits(staged.DimensionLengthDisplay, metrics.Resolution, staged.DimensionLengthDisplayUnit(doc.RuntimeSerialNumber)),
                 Digits(staged.AlternateDimensionLengthDisplay, metrics.Resolution, staged.AlternateDimensionLengthDisplayUnit(doc.RuntimeSerialNumber)));
         })
         select written).Bracket();

    private static int Digits(DimensionStyle.LengthDisplay display, Length resolution, UnitSystem system) =>
        LanguageExt.List.unfold(0, digits =>
            Quantities.Resolution(display is DimensionStyle.LengthDisplay.InchesFractional or DimensionStyle.LengthDisplay.FeetAndInches ? global::Rhino.UI.DistanceDisplayMode.Fractional : global::Rhino.UI.DistanceDisplayMode.Decimal,
                digits, LengthUnit.FromKnownUnitSystem(system)) > resolution
                ? Some((digits, digits + 1))
                : None).Count();

    private static IO<double> Advance(DimensionStyle staged) =>
        (from probe in use(() => staged.Duplicate())
         from sized in IO.lift(() => { (probe.TextHeight, probe.LineSpaceScale, probe.DimensionScale) = (1d, 1d, 1d); })
         from lines in Products.Of(() => TextEntity.Create("H\nH", Plane.WorldXY, probe, wrapped: false, rectWidth: 0d, rotationRadians: 0d))
         select lines.GetBoundingBox(accurate: true).Diagonal.Y - probe.TextHeight).Bracket();

    // --- [OVERRIDES]
    public static IO<Unit> Override(RhinoDoc doc, AnnotationBase annotation, Seq<Step<DimensionStyle>> steps) =>
        (from effective in Effective(doc, annotation, None)
         from child in Products.Of(() => effective.Duplicate("", Guid.Empty, annotation.DimensionStyleId))
         from edited in Edits.Apply(Stages.InPlace<DimensionStyle>(), child, steps)
         from assigned in Products.Accepted(() => annotation.SetOverrideDimStyle(edited))
         select assigned).Bracket();

    public static IO<Unit> Saved(RhinoDoc doc, AnnotationBase annotation) =>
        IO.lift(() => annotation.HasPropertyOverrides).Bind(overridden => when(overridden,
            (from parent in Parent(doc, annotation)
             from rooted in IO.lift(() => RhinoFault.Unless(!parent.IsChild, RhinoReason.Unsupported, (parent.Name, parent.ParentId)))
             from effective in Effective(doc, annotation, Some(parent))
             from staged in Products.Of(() => effective.Duplicate(parent.Name, parent.Id, Guid.Empty))
             from plain in IO.lift(staged.ClearAllFieldOverrides)
             from saved in Products.Accepted(() => doc.DimStyles.Modify(staged, parent.Index, quiet: true))
             from cleared in Products.Accepted(annotation.ClearPropertyOverrides)
             select cleared).Bracket()).As());

    // --- [CURRENT]
    public static IO<Unit> SetCurrent(RhinoDoc doc, ComponentRef<DimensionStyle> address) =>
        from style in Rows.Find(doc.DimStyles, address)
        from current in unless(doc.DimStyles.CurrentId == style.Id, Products.Accepted(() => doc.DimStyles.SetCurrent(style.Index, quiet: true))).As()
        select current;
}

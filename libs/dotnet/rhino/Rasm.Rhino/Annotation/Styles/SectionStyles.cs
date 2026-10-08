using System.Drawing;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;

namespace Rasm.Rhino.Annotation.Styles;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct BoundaryWidthScale : System.Numerics.IMinMaxValue<BoundaryWidthScale> {
    public static BoundaryWidthScale MinValue { get; } = new(double.BitIncrement(0.0));
    public static BoundaryWidthScale MaxValue { get; } = new(double.MaxValue);
    public static BoundaryWidthScale Unit { get; } = new(1.0);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionFill {
    public sealed record NoFill() : SectionFill;

    public sealed record ViewportFill() : SectionFill;

    public sealed record SolidFill(Option<Color> Color, Option<Color> PrintColor) : SectionFill;

    public static SectionFill FromHost(SectionStyle style) =>
        style.BackgroundFillMode switch {
            SectionBackgroundFillMode.None => new NoFill(),
            SectionBackgroundFillMode.Viewport => new ViewportFill(),
            SectionBackgroundFillMode.SolidColor => new SolidFill(Conversions.Present(style.BackgroundFillColor), Conversions.Present(style.BackgroundFillPrintColor)),
        };

    public void Write(SectionStyle staged) =>
        Switch(
            staged,
            noFill: static (target, _) => target.BackgroundFillMode = SectionBackgroundFillMode.None,
            viewportFill: static (target, _) => target.BackgroundFillMode = SectionBackgroundFillMode.Viewport,
            solidFill: static (target, solid) => {
                target.BackgroundFillMode = SectionBackgroundFillMode.SolidColor;
                target.BackgroundFillColor = Conversions.Unset(solid.Color);
                target.BackgroundFillPrintColor = Conversions.Unset(solid.PrintColor);
            });
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BoundaryLinetype {
    public sealed record Table(LinetypeRef Reference) : BoundaryLinetype;

    public sealed record Embedded(LinetypeDefinition Definition) : BoundaryLinetype;

    public static Option<BoundaryLinetype> FromHost(Option<LinetypeDefinition> embedded, Option<LinetypeRef> table) =>
        embedded.Map<BoundaryLinetype>(static definition => new Embedded(definition)) || table.Map<BoundaryLinetype>(static reference => new Table(reference));

    public IO<Unit> Written(RhinoDoc doc, SectionStyle staged) =>
        Switch(
            (Doc: doc, Staged: staged),
            table: static (state, table) =>
                from index in table.Reference.Resolve(state.Doc)
                from written in IO.lift(() => {
                    state.Staged.RemoveBoundaryLinetype();
                    state.Staged.BoundaryLinetypeIndex = index;
                })
                select unit,
            embedded: static (state, embedded) =>
                (from linetype in use(static () => new Linetype())
                 from defined in Linetypes.Written(linetype, embedded.Definition)
                 from written in IO.lift(() => {
                     state.Staged.SetBoundaryLinetype(linetype);
                     state.Staged.BoundaryLinetypeIndex = RhinoMath.UnsetIntIndex;
                 })
                 select unit).Bracket());
}

public sealed record SectionBoundary(Option<Color> Color, Option<Color> PrintColor, BoundaryWidthScale WidthScale, Option<PlotWeight> PrintWidth, Option<BoundaryLinetype> Linetype) {
    public static Fin<Option<SectionBoundary>> FromHost(SectionStyle style, Option<BoundaryLinetype> linetype) =>
        style.BoundaryVisible
            ? Conversions.Validated<BoundaryWidthScale, double, InvalidRhinoValue>(style.BoundaryWidthScale).Map(width => Some(new SectionBoundary(
                Conversions.Present(style.BoundaryColor),
                Conversions.Present(style.BoundaryPrintColor),
                width,
                PlotWeight.FromInheritable(style.BoundaryPlotWeightMillimeters),
                linetype)))
            : Option<SectionBoundary>.None;

    public IO<Unit> Written(RhinoDoc doc, SectionStyle staged) =>
        from settings in IO.lift(() => {
            staged.BoundaryColor = Conversions.Unset(Color);
            staged.BoundaryPrintColor = Conversions.Unset(PrintColor);
            staged.BoundaryWidthScale = WidthScale.ToValue();
            staged.BoundaryPlotWeightMillimeters = PlotWeight.ToInheritable(PrintWidth);
        })
        from linetype in Linetype.Match(
            Some: held => held.Written(doc, staged),
            None: () => IO.lift(() => {
                staged.RemoveBoundaryLinetype();
                staged.BoundaryLinetypeIndex = RhinoMath.UnsetIntIndex;
            }))
        select unit;
}

public sealed record SectionHatch(ComponentRef<HatchPattern> Pattern, HatchScale Scale, double Rotation, Option<Color> Color, Option<Color> PrintColor, Option<PlotWeight> PrintWidth) {
    public static Fin<Option<SectionHatch>> FromHost(SectionStyle style, Option<ComponentRef<HatchPattern>> pattern) =>
        pattern.Traverse(address => Conversions.Validated<HatchScale, double, InvalidRhinoValue>(style.HatchScale).Map(scale => new SectionHatch(
                address,
                scale,
                style.HatchRotationRadians,
                Conversions.Present(style.HatchPatternColor),
                Conversions.Present(style.HatchPatternPrintColor),
                PlotWeight.FromInheritable(style.HatchPatternPlotWeightMillimeters))))
            .As();

    public void Write(SectionStyle staged) {
        staged.HatchScale = Scale.ToValue();
        staged.HatchRotationRadians = Rotation;
        staged.HatchPatternColor = Conversions.Unset(Color);
        staged.HatchPatternPrintColor = Conversions.Unset(PrintColor);
        staged.HatchPatternPlotWeightMillimeters = PlotWeight.ToInheritable(PrintWidth);
    }
}

public sealed record SectionStyleDefinition(ObjectSectionFillRule SectionFillRule, SectionFill Fill, Option<SectionBoundary> Boundary, Option<SectionHatch> Hatch);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SectionStyles {
    // --- [READS]
    public static IO<SectionStyleDefinition> Definition(SectionStyle style) =>
        Definition(style, static index => Fin.Succ<ComponentRef<HatchPattern>>(index), LinetypeRef.FromHost);

    public static IO<bool> Unchanged(SectionStyle live, SectionStyle staged) =>
        (Definition(live), Definition(staged)).Apply(static (held, wanted) => held == wanted).As();

    private static IO<SectionStyleDefinition> Definition(SectionStyle style, Func<int, Fin<ComponentRef<HatchPattern>>> pattern, Func<int, Option<LinetypeRef>> table) =>
        from embedded in IO.lift(() => Optional(style.GetBoundaryLinetype())).Bracket(
            Use: static held => held.Traverse(Linetypes.Definition).As(),
            Fin: static held => DisposalOps.Release(held.ToSeq()))
        from hatch in IO.lift(() => Conversions.Present(style.HatchIndex).Traverse(pattern).As())
        from definition in IO.lift(() =>
            (SectionBoundary.FromHost(style, BoundaryLinetype.FromHost(embedded, table(style.BoundaryLinetypeIndex))).ToValidation(), SectionHatch.FromHost(style, hatch).ToValidation())
                .Apply((boundary, hatched) => new SectionStyleDefinition(style.SectionFillRule, SectionFill.FromHost(style), boundary, hatched))
                .As()
                .ToFin())
        select definition;

    // --- [WRITES]
    public static TableKind<SectionStyle> References { get; } =
        TableKinds.SectionStyles with { Add = static (doc, row) => doc.SectionStyles.AddReferenceSectionStyle(row) };

    public static IO<Unit> Written(RhinoDoc doc, SectionStyle staged, SectionStyleDefinition definition) =>
        from pattern in TableOps.Index(doc.HatchPatterns, definition.Hatch.Map(static hatch => hatch.Pattern))
        from settings in IO.lift(() => {
            staged.SectionFillRule = definition.SectionFillRule;
            staged.BoundaryVisible = definition.Boundary.IsSome;
            staged.HatchIndex = pattern;
            definition.Fill.Write(staged);
            definition.Hatch.Iter(hatch => hatch.Write(staged));
        })
        from stroked in definition.Boundary.Traverse(boundary => boundary.Written(doc, staged)).As()
        select unit;

    public static IO<int> Add(RhinoDoc doc, TableKind<SectionStyle> kind, string name, SectionStyleDefinition definition) =>
        TableOps.AddRow(doc, kind, staged =>
            from named in TableOps.Named(staged, Some(name))
            from written in Written(doc, staged, definition)
            select unit);

    // --- [FILES]
    public static IO<(Seq<(string Name, SectionStyleDefinition Definition)> Styles, Seq<(string Name, HatchPatternDefinition Definition)> Patterns)> ReadFile(string path) =>
        from existing in IO.lift(() => Exchange.ExistingPath(path))
        from file in IO.lift(() => Refused.Unless(
                SectionStyle.ReadFromFile(existing, out SectionStyle[] styles, out HatchPattern[] patterns),
                (Styles: toSeq(styles), Patterns: toSeq(patterns)),
                nameof(SectionStyle.ReadFromFile)))
            .Bracket(
                Use: static read =>
                    from named in read.Styles.TraverseM(style =>
                            from definition in Definition(
                                style,
                                index => read.Patterns.At(index).ToFin(new InvalidAnswer(nameof(SectionStyle.ReadFromFile))).Map<ComponentRef<HatchPattern>>(static pattern => pattern.Name),
                                static _ => Option<LinetypeRef>.None)
                            select (Name: style.Name, Definition: definition))
                        .As()
                    from held in read.Patterns.TraverseM(static pattern => HatchPatterns.Definition(pattern).Map(definition => (Name: pattern.Name, Definition: definition))).As()
                    select (Styles: named, Patterns: held),
                Fin: static read => DisposalOps.Release<IDisposable>([.. read.Styles, .. read.Patterns]))
        select file;

    public static IO<Committed<Seq<(string Name, int Index)>>> Import(RhinoDoc doc, string path, string name, RedrawPolicy redraw) =>
        from file in ReadFile(path)
        from landed in Commits.Commit(doc, name, redraw,
            from patternRows in TableOps.Rows(doc.HatchPatterns)
            from patternPlan in IO.lift(TableOps.Plan(patternRows, new TableSpec<HatchPatternDefinition>(file.Patterns, Exclusive: false)).ToFin())
            from patterns in TableOps.Upsert(doc, TableKinds.HatchPatterns, patternPlan.Upserts.Filter(static upsert => upsert.Existing.IsNone), HatchPatterns.Written, HatchPatterns.Unchanged)
            from styleRows in TableOps.Rows(doc.SectionStyles)
            from stylePlan in IO.lift(TableOps.Plan(styleRows, new TableSpec<SectionStyleDefinition>(file.Styles, Exclusive: false)).ToFin())
            from styles in TableOps.Upsert(doc, TableKinds.SectionStyles, stylePlan.Upserts, (staged, definition) => Written(doc, staged, definition), Unchanged)
            select styles)
        select landed;

    // --- [USAGE]
    public static IO<(bool InUse, int InstanceDefinitions, int Objects, int Layers)> Usage(RhinoDoc doc, ComponentRef<SectionStyle> address) =>
        TableOps.Find(doc.SectionStyles, address, includeDeleted: false)
            .Map(row => (InUse: doc.SectionStyles.InUse(row.Index, out int definitions, out int objects, out int layers), InstanceDefinitions: definitions, Objects: objects, Layers: layers));
}

# [RASM_RHINO_ANNOTATION_TYPEFACE]

`FaceForm` carries quartet and axis inputs through one `FaceQuery` admission, while `FaceInfo` detaches every catalogued identity, axis, decoration, installation, simulation, and specialty-font fact.

`SectionSpec` closes section fill, boundary, hatch, and fill-rule state without impossible products; `TypefaceOp` and `SectionOp` share `DraftPlan<T>`, `DraftSpine`, write policy, and typed receipts.

## [01]-[INDEX]

- [02]-[FACE_MODEL]: explicit host axes, quartet policy, decorations, query admission, and detached evidence.
- [03]-[TYPEFACE_RAIL]: polymorphic census, resolution, and document binding.
- [04]-[SECTION_MODEL]: closed fill, boundary, hatch, and whole-style composition.
- [05]-[SECTION_RAIL]: mutation, import reconciliation, usage evidence, and read projection.
- [06]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[FACE_MODEL]

- Owner: `FaceWeight`, `FaceSlant`, and `FaceStretch` key the full explicit host axes; `QuartetFace` owns the four legal bold/italic combinations and their availability probes; `FaceDecoration` owns underline and strikeout.
- Law: every axis includes host `Unset`; omission remains host data rather than collapsing into `Normal`, `Upright`, or `Medium`.
- Law: `FaceQuery.Of` folds every `FaceForm` case and decoration row through one admission, rejects duplicate decorations, checks quartet availability before resolution, and constructs an immutable `Font` through one union rail.
- Law: `FaceResolution` carries substitute evidence when `IsInstalled` is false; no consumer renders against a silently substituted face.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rasm.Rhino.Annotation;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class FaceWeight {
    public static readonly FaceWeight Unset = new(key: (int)Font.FontWeight.Unset);
    public static readonly FaceWeight Thin = new(key: (int)Font.FontWeight.Thin);
    public static readonly FaceWeight Ultralight = new(key: (int)Font.FontWeight.Ultralight);
    public static readonly FaceWeight Light = new(key: (int)Font.FontWeight.Light);
    public static readonly FaceWeight Normal = new(key: (int)Font.FontWeight.Normal);
    public static readonly FaceWeight Medium = new(key: (int)Font.FontWeight.Medium);
    public static readonly FaceWeight Semibold = new(key: (int)Font.FontWeight.Semibold);
    public static readonly FaceWeight Bold = new(key: (int)Font.FontWeight.Bold);
    public static readonly FaceWeight Ultrabold = new(key: (int)Font.FontWeight.Ultrabold);
    public static readonly FaceWeight Heavy = new(key: (int)Font.FontWeight.Heavy);
    internal Font.FontWeight Host => (Font.FontWeight)Key;
}

[SmartEnum<int>]
public sealed partial class FaceSlant {
    public static readonly FaceSlant Unset = new(key: (int)Font.FontStyle.Unset);
    public static readonly FaceSlant Upright = new(key: (int)Font.FontStyle.Upright);
    public static readonly FaceSlant Italic = new(key: (int)Font.FontStyle.Italic);
    public static readonly FaceSlant Oblique = new(key: (int)Font.FontStyle.Oblique);
    internal Font.FontStyle Host => (Font.FontStyle)Key;
}

[SmartEnum<int>]
public sealed partial class FaceStretch {
    public static readonly FaceStretch Unset = new(key: (int)Font.FontStretch.Unset);
    public static readonly FaceStretch Ultracondensed = new(key: (int)Font.FontStretch.Ultracondensed);
    public static readonly FaceStretch Extracondensed = new(key: (int)Font.FontStretch.Extracondensed);
    public static readonly FaceStretch Condensed = new(key: (int)Font.FontStretch.Condensed);
    public static readonly FaceStretch Semicondensed = new(key: (int)Font.FontStretch.Semicondensed);
    public static readonly FaceStretch Medium = new(key: (int)Font.FontStretch.Medium);
    public static readonly FaceStretch Semiexpanded = new(key: (int)Font.FontStretch.Semiexpanded);
    public static readonly FaceStretch Expanded = new(key: (int)Font.FontStretch.Expanded);
    public static readonly FaceStretch Extraexpanded = new(key: (int)Font.FontStretch.Extraexpanded);
    public static readonly FaceStretch Ultraexpanded = new(key: (int)Font.FontStretch.Ultraexpanded);
    internal Font.FontStretch Host => (Font.FontStretch)Key;
}

[SmartEnum<int>]
public sealed partial class QuartetFace {
    public static readonly QuartetFace Regular = new(key: 0, usesBold: false, usesItalic: false, available: static q => q.HasRegularFont);
    public static readonly QuartetFace Bold = new(key: 1, usesBold: true, usesItalic: false, available: static q => q.HasBoldFont);
    public static readonly QuartetFace Italic = new(key: 2, usesBold: false, usesItalic: true, available: static q => q.HasItalicFont);
    public static readonly QuartetFace BoldItalic = new(key: 3, usesBold: true, usesItalic: true, available: static q => q.HasBoldItalicFont);
    internal bool UsesBold { get; }
    internal bool UsesItalic { get; }
    [UseDelegateFromConstructor]
    internal partial bool Available(FontQuartet quartet);
}

[SmartEnum]
public sealed partial class FaceDecoration {
    public static readonly FaceDecoration Underline = new(underlined: true, strikeout: false);
    public static readonly FaceDecoration Strikeout = new(underlined: false, strikeout: true);
    internal bool Underlined { get; }
    internal bool Strikeout { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FaceForm {
    private FaceForm() { }
    public sealed record Quartet(string Name, QuartetFace Face) : FaceForm;
    public sealed record Axes(
        string Family, FaceWeight Weight, FaceSlant Slant, FaceStretch Stretch,
        Seq<FaceDecoration> Decorations) : FaceForm;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FaceQuery {
    private FaceQuery() { }
    private sealed record QuartetCase(ResourceName Name, QuartetFace Face) : FaceQuery;
    private sealed record AxesCase(
        ResourceName Family, FaceWeight Weight, FaceSlant Slant, FaceStretch Stretch,
        Seq<FaceDecoration> Decorations) : FaceQuery;

    public static Fin<FaceQuery> Of(FaceForm? form, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(value: form).Bind(value => value.Switch(
            state: op,
            quartet: static (gate, input) =>
                from label in gate.AcceptText(value: input.Name)
                from face in gate.Need(value: input.Face)
                select (FaceQuery)new QuartetCase(Name: ResourceName.Create(label), Face: face),
            axes: static (gate, input) =>
                from family in gate.AcceptText(value: input.Family)
                from weight in gate.Need(value: input.Weight)
                from slant in gate.Need(value: input.Slant)
                from stretch in gate.Need(value: input.Stretch)
                from decorations in input.Decorations.TraverseM(decoration => gate.Need(value: decoration)).As()
                from _ in guard(decorations.Distinct().Count == decorations.Count, gate.InvalidInput()).ToFin()
                select (FaceQuery)new AxesCase(
                    Family: ResourceName.Create(family), Weight: weight, Slant: slant,
                    Stretch: stretch, Decorations: decorations)));
    }

    internal Fin<Font> Resolve(Op key) =>
        Switch(
            state: key,
            quartetCase: static (op, query) =>
                from family in op.Catch(() => toSeq(Font.InstalledFontsAsQuartets())
                    .Find(row => string.Equals(row.QuartetName, query.Name.Value, StringComparison.OrdinalIgnoreCase))
                    .ToFin(Fail: op.MissingContext()))
                from _ in guard(query.Face.Available(family), op.MissingContext()).ToFin()
                from font in op.Catch(() => Optional(Font.FromQuartetProperties(
                        quartetName: query.Name.Value, bold: query.Face.UsesBold, italic: query.Face.UsesItalic))
                    .ToFin(Fail: op.MissingContext()))
                select font,
            axesCase: static (op, query) => op.Catch(() => Optional(new Font(
                    familyName: query.Family.Value,
                    weight: query.Weight.Host,
                    style: query.Slant.Host,
                    stretch: query.Stretch.Host,
                    underlined: query.Decorations.Exists(static item => item.Underlined),
                    strikethrough: query.Decorations.Exists(static item => item.Strikeout)))
                .ToFin(Fail: op.MissingContext())));
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record FaceInfo(
    string FaceName, string FamilyName, string FamilyPlusFaceName, string QuartetName,
    string PostScriptName, string LogfontName, string RichTextFontName, string Description,
    string EnglishFaceName, string EnglishFamilyName, string EnglishQuartetName,
    FaceWeight Weight, FaceSlant Slant, FaceStretch Stretch,
    bool Bold, bool Italic, bool Underlined, bool Strikeout, double PointSize,
    bool IsInstalled, bool IsSimulated, bool IsEngravingFont, bool IsSymbolFont,
    bool IsSingleStrokeFont, bool IsGeometricToleranceFont) {
    internal static Fin<FaceInfo> Of(Font font, Op key) => key.Catch(() =>
        from weight in key.AcceptValidated<FaceWeight>(candidate: (int)font.Weight)
        from slant in key.AcceptValidated<FaceSlant>(candidate: (int)font.Style)
        from stretch in key.AcceptValidated<FaceStretch>(candidate: (int)font.Stretch)
        select new FaceInfo(
            font.FaceName, font.FamilyName, font.FamilyPlusFaceName, font.QuartetName,
            font.PostScriptName, font.LogfontName, font.RichTextFontName, font.Description,
            font.EnglishFaceName, font.EnglishFamilyName, font.EnglishQuartetName,
            weight, slant, stretch, font.Bold, font.Italic, font.Underlined, font.Strikeout, font.PointSize,
            font.IsInstalled, font.IsSimulated, font.IsEngravingFont, font.IsSymbolFont,
            font.IsSingleStrokeFont, font.IsGeometricToleranceFont));
}

public readonly record struct QuartetInfo(
    ResourceName Name, bool HasRegular, bool HasBold, bool HasItalic, bool HasBoldItalic);
public sealed record FaceResolution(FaceInfo Face, Option<FaceInfo> Substitute);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FaceCensus {
    private FaceCensus() { }
    public sealed record Installed(Option<ResourceName> Family) : FaceCensus;
    public sealed record Quartets : FaceCensus;
    public sealed record Names : FaceCensus;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FaceCensusAnswer {
    private FaceCensusAnswer() { }
    public sealed record Faces(Seq<FaceInfo> Items) : FaceCensusAnswer;
    public sealed record Quartets(Seq<QuartetInfo> Items) : FaceCensusAnswer;
    public sealed record Names(Seq<ResourceName> Items) : FaceCensusAnswer;
}
```

## [03]-[TYPEFACE_RAIL]

- Owner: `Typefaces.Resolve` handles immutable face resolution, `Typefaces.Census` handles machine-state discovery, and `TypefaceOp.Bind` handles the sole document mutation.
- Law: `Bind` compares the complete `FaceInfo` projection, adds only on a miss, and commits through the shared Annotation spine.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TypefaceOp {
    private TypefaceOp() { }
    public sealed record Bind(FaceQuery Query, Option<ResourceRef> Template) : TypefaceOp;

    internal Fin<DraftReceipt> Apply(RhinoDoc document, Op op) =>
        Switch(
            state: (Document: document, Op: op),
            bind: static (ctx, edit) =>
                from font in edit.Query.Resolve(key: ctx.Op)
                from face in FaceInfo.Of(font: font, key: ctx.Op)
                from template in edit.Template.Traverse(address => address.Resolve(
                    document: ctx.Document, lens: StyleOp.Lens, key: ctx.Op)).As()
                from candidates in ctx.Op.Catch(() => toSeq(ctx.Document.DimStyles)
                    .Filter(static style => !style.IsDeleted)
                    .TraverseM(style => FaceInfo.Of(font: style.Font, key: ctx.Op)
                        .Map(resolved => (Style: style, Face: resolved))).As())
                from index in candidates.Find(row => row.Face == face).Match(
                        Some: static held => Fin.Succ(value: ResourceIndex.Create(held.Style.Index)),
                        None: () =>
                            from fresh in ctx.Op.Catch(() => Fin.Succ(value: template
                                .IfNone(() => ctx.Document.DimStyles.Current)
                                .Duplicate(newName: ctx.Document.DimStyles.GetUnusedStyleName(), newId: Guid.NewGuid(), newParentId: Guid.Empty)))
                            from _ in ctx.Op.Catch(() => fresh.Font = font)
                            from added in ctx.Op.Catch(() =>
                                ResourceIndex.Admit(ctx.Document.DimStyles.Add(dimstyle: fresh, reference: false), ctx.Op))
                            select added))
                from receipt in DraftReceipt.Component(slot: DraftSlot.Bound, componentKind: DraftComponentKind.Style, index: index)
                select receipt);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Typefaces {
    public static Fin<FaceResolution> Resolve(FaceQuery query) {
        Op op = Op.Of();
        return from admitted in op.AcceptInput(value: query)
               from font in admitted.Resolve(key: op)
               from face in FaceInfo.Of(font: font, key: op)
               from substitute in face.IsInstalled
                   ? Fin.Succ(Option<FaceInfo>.None)
                   : op.Catch(() => Optional(font.GetSubstituteFont())
                       .Traverse(fallback => FaceInfo.Of(font: fallback, key: op)).As())
               select new FaceResolution(Face: face, Substitute: substitute);
    }

    public static Fin<FaceCensusAnswer> Census(FaceCensus request) {
        Op op = Op.Of();
        return op.AcceptInput(value: request).Bind(admitted => admitted.Switch(
            installed: query =>
                from fonts in op.Catch(() => Fin.Succ(value: toSeq(query.Family.Match(
                    Some: static family => Font.InstalledFonts(familyName: family.Value),
                    None: static () => Font.InstalledFonts()))))
                from faces in fonts.TraverseM(font => FaceInfo.Of(font: font, key: op)).As()
                select (FaceCensusAnswer)new FaceCensusAnswer.Faces(Items: faces),
            quartets: _ => op.Catch(() => Fin.Succ<FaceCensusAnswer>(
                value: new FaceCensusAnswer.Quartets(
                    Items: toSeq(Font.InstalledFontsAsQuartets()).Map(static row => new QuartetInfo(
                        Name: ResourceName.Create(row.QuartetName),
                        HasRegular: row.HasRegularFont,
                        HasBold: row.HasBoldFont,
                        HasItalic: row.HasItalicFont,
                        HasBoldItalic: row.HasBoldItalicFont))))),
            names: _ => op.Catch(() => Fin.Succ<FaceCensusAnswer>(
                value: new FaceCensusAnswer.Names(
                    Items: toSeq(Font.AvailableFontFaceNames()).Map(static name => ResourceName.Create(name)))))));
    }

    public static Fin<DraftReceipt> Commit(DocumentSession session, DraftPlan<TypefaceOp> plan) =>
        DraftSpine.Commit(session: session, plan: plan,
            apply: static (document, operation, key) => operation.Apply(document: document, op: key), op: Op.Of());
}
```

## [04]-[SECTION_MODEL]

- Owner: `SectionFill`, `SectionBoundary`, and `SectionHatch` encode only realizable host states; `SectionSpec` composes them with the fill rule and admitted name.
- Law: every inactive fill, boundary, and hatch branch restores its host-default subordinate fields before landing.
- Law: absent fill, hidden boundary, and absent hatch carry no dead colors, scales, rotations, weights, or resource addresses.
- Law: resource addresses resolve inside the document grant before any table index is written; every numeric host input is a generated value object.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SectionRule {
    public static readonly SectionRule ClosedCurves = new(key: (int)ObjectSectionFillRule.ClosedCurves);
    public static readonly SectionRule SolidObjects = new(key: (int)ObjectSectionFillRule.SolidObjects);
    internal ObjectSectionFillRule Host => (ObjectSectionFillRule)Key;
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public sealed partial class SectionScale {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        if (!double.IsFinite(value) || value <= 0.0) validationError = new ValidationError("Section scale must be finite and positive.");
    }
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public sealed partial class SectionAngle {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        if (!double.IsFinite(value)) validationError = new ValidationError("Section angle must be finite.");
    }
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public sealed partial class SectionPlotWeight {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        if (!double.IsFinite(value) || value < 0.0) validationError = new ValidationError("Section plot weight must be finite and non-negative.");
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionFill {
    private SectionFill() { }
    public sealed record None : SectionFill;
    public sealed record Viewport : SectionFill;
    public sealed record Solid(PerceptualColor Display, PerceptualColor Print) : SectionFill;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionBoundary {
    private SectionBoundary() { }
    public sealed record Hidden : SectionBoundary;
    public sealed record Stroke(
        PerceptualColor Display, PerceptualColor Print,
        SectionScale Width, SectionPlotWeight PlotWeight,
        Option<ResourceRef> Linetype) : SectionBoundary;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionHatch {
    private SectionHatch() { }
    public sealed record None : SectionHatch;
    public sealed record Pattern(
        ResourceRef Resource, SectionScale Scale, SectionAngle Rotation,
        PerceptualColor Display, PerceptualColor Print) : SectionHatch;
}

[ComplexValueObject]
public sealed partial class SectionSpec {
    public ResourceName Name { get; }
    public SectionFill Fill { get; }
    public SectionBoundary Boundary { get; }
    public SectionHatch Hatch { get; }
    public SectionRule Rule { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ResourceName name, ref SectionFill fill, ref SectionBoundary boundary,
        ref SectionHatch hatch, ref SectionRule rule) {
        if (name is null || fill is null || boundary is null || hatch is null || rule is null)
            validationError = new ValidationError("Section presentation is incomplete.");
    }

    internal Fin<Unit> Apply(SectionStyle style, RhinoDoc document, Op key) =>
        from clear in key.Catch(() => Fin.Succ(value: new SectionStyle()))
        from linetype in (Boundary is SectionBoundary.Stroke { Linetype: var address }
                ? address.Traverse(item => item.Resolve(document: document, lens: LinetypeOp.Lens, key: key)).As()
                : Fin.Succ(Option<Linetype>.None))
        from pattern in (Hatch is SectionHatch.Pattern { Resource: var address }
                ? address.Resolve(document: document, lens: HatchSpec.Lens, key: key).Map(Some)
                : Fin.Succ(Option<HatchPattern>.None))
        from _ in key.Catch(() => {
            style.Name = Name.Value;
            style.SectionFillRule = Rule.Host;
        })
        from __ in Fill.Switch(
            state: (Style: style, Clear: clear, Key: key),
            none: static (ctx, _) => ctx.Key.Catch(() => {
                ctx.Style.BackgroundFillMode = SectionBackgroundFillMode.None;
                ctx.Style.BackgroundFillColor = ctx.Clear.BackgroundFillColor;
                ctx.Style.BackgroundFillPrintColor = ctx.Clear.BackgroundFillPrintColor;
            }),
            viewport: static (ctx, _) => ctx.Key.Catch(() => {
                ctx.Style.BackgroundFillMode = SectionBackgroundFillMode.Viewport;
                ctx.Style.BackgroundFillColor = ctx.Clear.BackgroundFillColor;
                ctx.Style.BackgroundFillPrintColor = ctx.Clear.BackgroundFillPrintColor;
            }),
            solid: static (ctx, fill) => ctx.Key.Catch(() => {
                ctx.Style.BackgroundFillMode = SectionBackgroundFillMode.SolidColor;
                ctx.Style.BackgroundFillColor = fill.Display.Sys();
                ctx.Style.BackgroundFillPrintColor = fill.Print.Sys();
            }))
        from ___ in Boundary.Switch(
            state: (Style: style, Clear: clear, Linetype: linetype, Key: key),
            hidden: static (ctx, _) => ctx.Key.Catch(() => {
                ctx.Style.BoundaryVisible = false;
                ctx.Style.BoundaryColor = ctx.Clear.BoundaryColor;
                ctx.Style.BoundaryPrintColor = ctx.Clear.BoundaryPrintColor;
                ctx.Style.BoundaryWidthScale = ctx.Clear.BoundaryWidthScale;
                ctx.Style.BoundaryPlotWeightMillimeters = ctx.Clear.BoundaryPlotWeightMillimeters;
                ctx.Style.RemoveBoundaryLinetype();
            }),
            stroke: static (ctx, boundary) => ctx.Key.Catch(() => {
                ctx.Style.BoundaryVisible = true;
                ctx.Style.BoundaryColor = boundary.Display.Sys();
                ctx.Style.BoundaryPrintColor = boundary.Print.Sys();
                ctx.Style.BoundaryWidthScale = boundary.Width.Value;
                ctx.Style.BoundaryPlotWeightMillimeters = boundary.PlotWeight.Value;
                ctx.Linetype.Match(Some: ctx.Style.SetBoundaryLinetype, None: ctx.Style.RemoveBoundaryLinetype);
            }))
        from ____ in Hatch.Switch(
            state: (Style: style, Clear: clear, Pattern: pattern, Key: key),
            none: static (ctx, _) => ctx.Key.Catch(() => {
                ctx.Style.HatchIndex = ctx.Clear.HatchIndex;
                ctx.Style.HatchScale = ctx.Clear.HatchScale;
                ctx.Style.HatchRotationRadians = ctx.Clear.HatchRotationRadians;
                ctx.Style.HatchPatternColor = ctx.Clear.HatchPatternColor;
                ctx.Style.HatchPatternPrintColor = ctx.Clear.HatchPatternPrintColor;
            }),
            pattern: static (ctx, hatch) => ctx.Key.Catch(() => {
                ctx.Style.HatchIndex = ctx.Pattern.Map(static value => value.Index).IfNone(-1);
                ctx.Style.HatchScale = hatch.Scale.Value;
                ctx.Style.HatchRotationRadians = hatch.Rotation.Value;
                ctx.Style.HatchPatternColor = hatch.Display.Sys();
                ctx.Style.HatchPatternPrintColor = hatch.Print.Sys();
            }))
        select unit;
}
```

## [05]-[SECTION_RAIL]

- Owner: `SectionSource` carries local versus reference admission behavior; `SectionOp` owns author, amend, guarded plural delete, and `.secstyles` import; `SectionSnapshot` returns the same `SectionSpec` composition plus usage and table state.
- Law: amendment rides `SectionOp.Grip` — the shared `TableGrip` duplicate-then-`Modify` law — with the copy constructor as the duplicate row and id-resolved index as the index row.
- Law: import canonicalizes hatch references through `PatternDef`, preflights names, compensates added patterns plus added or replaced styles in reverse landing order, and records every outcome in the shared receipt algebra.
- Law: delete resolves and usage-gates every target before one `SectionStyleTable.Delete(IEnumerable<int>, bool, int)` call; any retained row makes the whole request fail before mutation.
- Boundary: section projection captures host reads and every generated admission constructor inside one `Op.Catch` rail.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum]
public sealed partial class SectionSource {
    public static readonly SectionSource Local = new(add: static (document, style) =>
        document.SectionStyles.Add(sectionstyle: style));
    public static readonly SectionSource Reference = new(add: static (document, style) =>
        document.SectionStyles.AddReferenceSectionStyle(sectionstyle: style));

    [UseDelegateFromConstructor]
    internal partial int Add(RhinoDoc document, SectionStyle style);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionOp {
    private SectionOp() { }
    public sealed record Author(SectionSpec Spec, SectionSource Source) : SectionOp;
    public sealed record Amend(ResourceRef Target, SectionSpec Spec, WriteMode Mode) : SectionOp;
    public sealed record Delete(Seq<ResourceRef> Targets, WriteMode Mode) : SectionOp;
    public sealed record Import(DraftPath Path, WriteMode Mode) : SectionOp;

    internal static readonly ResourceLens<SectionStyle> Lens = new(
        ById: static (document, id) => document.SectionStyles.Find(id: id, ignoreDeletedSectionStyles: true) is var index && index >= 0
            ? document.SectionStyles.FindIndex(index: index)
            : null,
        ByName: static (document, name) => document.SectionStyles.FindName(name: name),
        ByIndex: static (document, index) => document.SectionStyles.FindIndex(index: index));

    internal static readonly TableGrip<SectionStyle> Grip = new(
        Lens, DraftComponentKind.Section,
        Index: static (document, style) => document.SectionStyles.Find(id: style.Id, ignoreDeletedSectionStyles: true),
        Duplicate: static style => new SectionStyle(style),
        Modify: static (document, copy, index, quiet) => document.SectionStyles.Modify(sectionstyle: copy, index: index, quiet: quiet));

    private sealed record PatternIntent(int Source, HatchPattern Pattern, Option<ResourceIndex> Existing) {
        internal static Fin<PatternIntent> Admit(RhinoDoc document, HatchPattern pattern, Op op) =>
            from source in op.Need(value: pattern)
            from definition in PatternDef.Read(pattern: source, key: op)
            from canonical in definition.Mint(key: op)
            from existing in Optional(document.HatchPatterns.FindName(name: canonical.Name)).Match(
                Some: held =>
                    from current in PatternDef.Read(pattern: held, key: op)
                    from _ in guard(definition.Equivalent(current), op.InvalidInput()).ToFin()
                    select Some(ResourceIndex.Create(held.Index)),
                None: static () => Fin.Succ(Option<ResourceIndex>.None))
            select new PatternIntent(Source: source.Index, Pattern: canonical, Existing: existing);
    }
    private sealed record PatternLanding(int Source, ResourceIndex Target, bool Added);
    private sealed record SectionExisting(ResourceIndex Index, SectionStyle Original);
    private sealed record SectionIntent(SectionStyle Style, Option<SectionExisting> Existing);
    private sealed record SectionLanding(DraftSlot Slot, ResourceIndex Index, Option<SectionStyle> Original);

    internal Fin<DraftReceipt> Apply(RhinoDoc document, Op op) => Switch(
        (Document: document, Op: op),
        author: static (ctx, edit) =>
            from _ in guard(ctx.Document.SectionStyles.FindName(name: edit.Spec.Name.Value) is null, ctx.Op.InvalidInput()).ToFin()
            from seed in ctx.Op.Catch(() => Fin.Succ(value: new SectionStyle()))
            from __ in edit.Spec.Apply(style: seed, document: ctx.Document, key: ctx.Op)
            from index in ctx.Op.Catch(() => ResourceIndex.Admit(edit.Source.Add(document: ctx.Document, style: seed), ctx.Op))
            from receipt in DraftReceipt.Component(DraftSlot.Authored, DraftComponentKind.Section, index)
            select receipt,
        amend: static (ctx, edit) =>
            Grip.Revised(target: edit.Target, document: ctx.Document, slot: DraftSlot.Amended, mode: edit.Mode, op: ctx.Op,
                revise: (copy, key) => edit.Spec.Apply(style: copy, document: ctx.Document, key: key)),
        delete: static (ctx, edit) =>
            from _ in guard(!edit.Targets.IsEmpty, ctx.Op.InvalidInput()).ToFin()
            from indices in edit.Targets.TraverseM(target =>
                from style in target.Resolve(document: ctx.Document, lens: Lens, key: ctx.Op)
                from index in ctx.Op.Catch(() => ResourceIndex.Admit(Grip.Index(ctx.Document, style), ctx.Op))
                from usage in SectionUsage.Read(document: ctx.Document, index: index.Value, key: ctx.Op)
                from __ in guard(!usage.Bound, ctx.Op.InvalidInput()).ToFin()
                select index.Value).As()
            from __ in guard(indices.Distinct().Count == indices.Count, ctx.Op.InvalidInput()).ToFin()
            from deleted in ctx.Op.Catch(() => Fin.Succ(value: ctx.Document.SectionStyles.Delete(
                sectionStyleIndices: indices, quiet: edit.Mode.QuietWrite, deleteWarning: 0)))
            from ___ in guard(deleted == indices.Count, ctx.Op.InvalidResult()).ToFin()
            from receipts in indices.TraverseM(index => DraftReceipt.Component(
                DraftSlot.Deleted, DraftComponentKind.Section, ResourceIndex.Create(index))).As()
            select receipts.Fold(DraftReceipt.Empty, static (receipt, next) => receipt.Contribute(next)),
        import: static (ctx, edit) => ImportFile(
            document: ctx.Document, path: edit.Path, mode: edit.Mode, op: ctx.Op));

    private static Fin<DraftReceipt> ImportFile(RhinoDoc document, DraftPath path, WriteMode mode, Op op) =>
        from read in op.Catch(() => SectionStyle.ReadFromFile(
                filename: path.Value, sectionStyles: out SectionStyle[] styles, hatchPatterns: out HatchPattern[] patterns)
            ? Fin.Succ(value: (Styles: toSeq(styles ?? []), Patterns: toSeq(patterns ?? [])))
            : Fin.Fail<(Seq<SectionStyle>, Seq<HatchPattern>)>(error: op.InvalidResult()))
        from plan in Preflight(document: document, styles: read.Styles, patterns: read.Patterns, op: op)
        from patternFacts in DocumentCommit.Compensated(
            source: plan.Patterns,
            land: intent => LandPattern(document: document, intent: intent, op: op),
            rollback: landed => RollbackPatterns(document: document, landed: landed, op: op))
        from receipt in ImportSections(
            document: document, path: path, mode: mode, plan: plan.Styles, patterns: patternFacts, op: op)
        select receipt;

    private static Fin<(Seq<PatternIntent> Patterns, Seq<SectionIntent> Styles)> Preflight(
        RhinoDoc document, Seq<SectionStyle> styles, Seq<HatchPattern> patterns, Op op) => op.Catch(() =>
        from _ in guard(!styles.IsEmpty, op.InvalidResult()).ToFin()
        from admittedPatterns in patterns.TraverseM(pattern => PatternIntent.Admit(
            document: document, pattern: pattern, op: op)).As()
        from __ in guard(
            admittedPatterns.AsIterable().Select(static row => row.Source).Distinct().Count() == admittedPatterns.Count
            && admittedPatterns.AsIterable().Select(static row => row.Pattern.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase).Count() == admittedPatterns.Count,
            op.InvalidInput()).ToFin()
        from admittedStyles in styles.TraverseM(style =>
            from value in op.AcceptInput(value: style)
            from name in op.AcceptText(value: value.Name)
            from existing in Optional(document.SectionStyles.FindName(name: name)).Match(
                Some: held =>
                    from index in op.Catch(() => ResourceIndex.Admit(document.SectionStyles.Find(name: held.Name), op))
                    from original in op.Catch(() => Fin.Succ(value: new SectionStyle(held)))
                    select Some(new SectionExisting(Index: index, Original: original)),
                None: static () => Fin.Succ(Option<SectionExisting>.None))
            select new SectionIntent(Style: value, Existing: existing)).As()
        from ___ in guard(
            admittedStyles.AsIterable().Select(static row => row.Style.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase).Count() == admittedStyles.Count
            && admittedStyles.ForAll(row => row.Style.HatchIndex < 0
                || admittedPatterns.Exists(pattern => pattern.Source == row.Style.HatchIndex)),
            op.InvalidInput()).ToFin()
        select (admittedPatterns, admittedStyles));

    private static Fin<PatternLanding> LandPattern(RhinoDoc document, PatternIntent intent, Op op) =>
        intent.Existing.Match(
            Some: target => Fin.Succ(value: new PatternLanding(Source: intent.Source, Target: target, Added: false)),
            None: () => op.Catch(() => ResourceIndex.Admit(document.HatchPatterns.Add(pattern: intent.Pattern), op)
                .Map(target => new PatternLanding(Source: intent.Source, Target: target, Added: true))));

    private static Fin<DraftReceipt> ImportSections(
        RhinoDoc document, DraftPath path, WriteMode mode,
        Seq<SectionIntent> plan, Seq<PatternLanding> patterns, Op op) {
        Fin<Seq<SectionLanding>> landed = DocumentCommit.Compensated(
            source: plan,
            land: intent => LandSection(document: document, intent: intent, patterns: patterns, mode: mode, op: op),
            rollback: changes => RollbackSections(document: document, landed: changes, op: op));
        return landed.Match(
            Succ: changes => FinishImport(
                path: path, changes: changes, patternCount: patterns.Count).Match(
                Succ: static receipt => Fin.Succ(value: receipt),
                Fail: primary => RollbackImport(
                    document: document, sections: changes, patterns: patterns, op: op).Match(
                    Succ: _ => Fin.Fail<DraftReceipt>(error: primary),
                    Fail: rollback => Fin.Fail<DraftReceipt>(error: primary + rollback))),
            Fail: primary => RollbackPatterns(document: document, landed: patterns, op: op).Match(
                Succ: _ => Fin.Fail<DraftReceipt>(error: primary),
                Fail: rollback => Fin.Fail<DraftReceipt>(error: primary + rollback)));
    }

    private static Fin<DraftReceipt> FinishImport(
        DraftPath path, Seq<SectionLanding> changes, int patternCount) =>
        from sectionReceipts in changes.TraverseM(change => DraftReceipt.Component(
                    slot: change.Slot, componentKind: DraftComponentKind.Section, index: change.Index)).As()
        from patternReceipt in DraftReceipt.Tally(slot: DraftSlot.Imported, count: DraftCount.Create(patternCount))
        from pathReceipt in DraftReceipt.Path(slot: DraftSlot.Imported, path: path)
        select sectionReceipts.Fold(
            pathReceipt.Contribute(patternReceipt), static (state, next) => state.Contribute(next));

    private static Fin<SectionLanding> LandSection(
        RhinoDoc document, SectionIntent intent, Seq<PatternLanding> patterns, WriteMode mode, Op op) =>
        from hatch in intent.Style.HatchIndex < 0
            ? Fin.Succ(value: intent.Style.HatchIndex)
            : patterns.Find(pattern => pattern.Source == intent.Style.HatchIndex)
                .Map(static pattern => pattern.Target.Value).ToFin(Fail: op.MissingContext())
        from _ in op.Catch(() => intent.Style.HatchIndex = hatch)
        from landed in intent.Existing.Match(
            Some: existing =>
                from __ in op.Confirm(success: document.SectionStyles.Modify(
                    sectionstyle: intent.Style, index: existing.Index.Value, quiet: mode.QuietWrite))
                select new SectionLanding(
                    Slot: DraftSlot.Amended, Index: existing.Index, Original: Some(existing.Original)),
            None: () => op.Catch(() => ResourceIndex.Admit(document.SectionStyles.Add(sectionstyle: intent.Style), op)
                .Map(index => new SectionLanding(
                    Slot: DraftSlot.Authored, Index: index, Original: Option<SectionStyle>.None))))
        select landed;

    private static Fin<Unit> RollbackPatterns(RhinoDoc document, Seq<PatternLanding> landed, Op op) =>
        Rollback(
            rows: toSeq(landed.Filter(static row => row.Added).AsIterable().Reverse()),
            restore: row => op.Confirm(success: document.HatchPatterns.Delete(
                hatchPatternIndex: row.Target.Value, quiet: true)));

    private static Fin<Unit> RollbackSections(RhinoDoc document, Seq<SectionLanding> landed, Op op) =>
        Rollback(
            rows: toSeq(landed.AsIterable().Reverse()),
            restore: row => row.Original.Match(
                Some: original => op.Confirm(success: document.SectionStyles.Modify(
                    sectionstyle: original, index: row.Index.Value, quiet: true)),
                None: () => op.Confirm(success: document.SectionStyles.Delete(index: row.Index.Value, quiet: true))));

    private static Fin<Unit> RollbackImport(
        RhinoDoc document, Seq<SectionLanding> sections, Seq<PatternLanding> patterns, Op op) {
        Fin<Unit> sectionRollback = RollbackSections(document: document, landed: sections, op: op);
        Fin<Unit> patternRollback = RollbackPatterns(document: document, landed: patterns, op: op);
        return sectionRollback.Match(
            Succ: _ => patternRollback,
            Fail: sectionFault => patternRollback.Match(
                Succ: _ => Fin.Fail<Unit>(error: sectionFault),
                Fail: patternFault => Fin.Fail<Unit>(error: sectionFault + patternFault)));
    }

    private static Fin<Unit> Rollback<T>(Seq<T> rows, Func<T, Fin<Unit>> restore) =>
        rows.Fold(
            Fin.Succ(value: unit),
            (state, row) => restore(row).Match(
                Succ: _ => state,
                Fail: error => state.Match(
                    Succ: _ => Fin.Fail<Unit>(error: error),
                    Fail: prior => Fin.Fail<Unit>(error: prior + error))));
}

// --- [MODELS] -------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class SectionUsage {
    public bool HostBound { get; }
    public int Definitions { get; }
    public int Objects { get; }
    public int Layers { get; }
    public bool Bound => HostBound || Definitions > 0 || Objects > 0 || Layers > 0;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref bool hostBound, ref int definitions, ref int objects, ref int layers) {
        if (definitions < 0 || objects < 0 || layers < 0)
            validationError = new ValidationError("Section usage cannot be negative.");
    }

    internal static Fin<SectionUsage> Read(RhinoDoc document, int index, Op key) => key.Catch(() => {
        bool hostBound = document.SectionStyles.InUse(
            index: index,
            instanceDefinitionCount: out int definitions,
            objectCount: out int objects,
            layerCount: out int layers);
        return Fin.Succ(value: Create(
            hostBound: hostBound,
            definitions: definitions,
            objects: objects,
            layers: layers));
    });
}

public sealed record SectionSnapshot(
    ResourceId Key, SectionSpec Spec, bool InUse, bool IsUnset, SectionUsage Usage) : IDetachedDocumentResult;

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionAsk {
    private SectionAsk() { }
    public sealed record State(ResourceRef Target) : SectionAsk;
    public sealed record TableState : SectionAsk;
    public sealed record MintName : SectionAsk;

    internal Fin<SectionAnswer> Answer(RhinoDoc document, Op op) => Switch(
        (Document: document, Op: op),
        state: static (ctx, ask) => ctx.Op.Catch(() =>
            from style in ask.Target.Resolve(document: ctx.Document, lens: SectionOp.Lens, key: ctx.Op)
            from index in ctx.Op.Catch(() => Fin.Succ(value: SectionOp.Grip.Index(ctx.Document, style)))
            from usage in SectionUsage.Read(document: ctx.Document, index: index, key: ctx.Op)
            from spec in Project(style: style, key: ctx.Op)
            select (SectionAnswer)new SectionAnswer.State(Snapshot: new SectionSnapshot(
                Key: ResourceId.Create(style.Id), Spec: spec, InUse: style.InUse, IsUnset: style.IsUnset, Usage: usage))),
        tableState: static (ctx, _) => ctx.Op.Catch(() => Fin.Succ<SectionAnswer>(
            value: new SectionAnswer.Rows(ActiveCount: DraftCount.Create(ctx.Document.SectionStyles.ActiveCount)))),
        mintName: static (ctx, _) => ctx.Op.Catch(() =>
            ctx.Op.AcceptText(value: ctx.Document.SectionStyles.GetUnusedSectionStyleName())
                .Map(name => (SectionAnswer)new SectionAnswer.Minted(Name: ResourceName.Create(name))));

    private static Fin<SectionSpec> Project(SectionStyle style, Op key) => key.Catch(() =>
        from fillDisplay in style.BackgroundFillColor.Admitted(key)
        from fillPrint in style.BackgroundFillPrintColor.Admitted(key)
        from boundaryDisplay in style.BoundaryColor.Admitted(key)
        from boundaryPrint in style.BoundaryPrintColor.Admitted(key)
        from hatchDisplay in style.HatchPatternColor.Admitted(key)
        from hatchPrint in style.HatchPatternPrintColor.Admitted(key)
        from linetype in Optional(style.GetBoundaryLinetype()).Traverse(item => ResourceRef.Of(id: item.Id)).As()
        from hatch in style.HatchIndex >= 0
            ? ResourceRef.Of(index: style.HatchIndex).Map(Some)
            : Fin.Succ(Option<ResourceRef>.None)
        from rule in key.AcceptValidated<SectionRule>(candidate: (int)style.SectionFillRule)
        from fill in style.BackgroundFillMode switch {
            SectionBackgroundFillMode.None => Fin.Succ<SectionFill>(new SectionFill.None()),
            SectionBackgroundFillMode.Viewport => Fin.Succ<SectionFill>(new SectionFill.Viewport()),
            SectionBackgroundFillMode.SolidColor => Fin.Succ<SectionFill>(new SectionFill.Solid(fillDisplay, fillPrint)),
            _ => Fin.Fail<SectionFill>(key.InvalidResult()),
        }
        select SectionSpec.Create(
            name: ResourceName.Create(style.Name),
            fill: fill,
            boundary: style.BoundaryVisible
                ? new SectionBoundary.Stroke(
                    boundaryDisplay, boundaryPrint,
                    SectionScale.Create(style.BoundaryWidthScale),
                    SectionPlotWeight.Create(style.BoundaryPlotWeightMillimeters),
                    linetype)
                : new SectionBoundary.Hidden(),
            hatch: hatch.Match<SectionHatch>(
                Some: address => new SectionHatch.Pattern(
                    address,
                    SectionScale.Create(style.HatchScale),
                    SectionAngle.Create(style.HatchRotationRadians),
                    hatchDisplay,
                    hatchPrint),
                None: static () => new SectionHatch.None()),
            rule: rule));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionAnswer : IDetachedDocumentResult {
    private SectionAnswer() { }
    public sealed record State(SectionSnapshot Snapshot) : SectionAnswer;
    public sealed record Rows(DraftCount ActiveCount) : SectionAnswer;
    public sealed record Minted(ResourceName Name) : SectionAnswer;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Sections {
    public static Fin<DraftReceipt> Commit(DocumentSession session, DraftPlan<SectionOp> plan) =>
        DraftSpine.Commit(session: session, plan: plan,
            apply: static (document, operation, key) => operation.Apply(document: document, op: key), op: Op.Of());

    public static Fin<SectionAnswer> Ask(DocumentSession session, SectionAsk request) {
        Op op = Op.Of();
        return from admitted in op.AcceptInput(value: request)
               from answer in session.Demand(
                   use: document => admitted.Answer(document: document, op: op), key: op, needs: [SessionNeed.Read])
               select answer;
    }
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]           | [OWNER]           | [FORM]                                               | [ENTRY]             |
| :-----: | :------------------ | :---------------- | :--------------------------------------------------- | :------------------ |
|  [01]   | face construction   | `FaceQuery`       | quartet policy or full explicit axes                 | `Typefaces.Resolve` |
|  [02]   | machine census      | `FaceCensus`      | installed faces, quartet grid, or face names         | `Typefaces.Census`  |
|  [03]   | document face bind  | `TypefaceOp`      | resolved face probe plus style-table add             | `Typefaces.Commit`  |
|  [04]   | section composition | `SectionSpec`     | closed fill, boundary, hatch, and rule state         | `Create` / `Apply`  |
|  [05]   | section mutations   | `SectionOp`       | local/reference author, amend, plural delete, import | `Sections.Commit`   |
|  [06]   | section evidence    | `SectionSnapshot` | composed spec plus usage and table state             | `Sections.Ask`      |

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

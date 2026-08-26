# [RASM_RHINO_ANNOTATION_TYPEFACE]

`FaceDecoration` and `FaceTrait` are the namespace's typeface capability rosters, `FaceQuery` the one face admission, and `FaceInfo` the detached descriptor every census and document bind compares.

`SectionField` instantiates the drafting-schema mechanism over `SectionStyle`, so every section write rides one result-typed row fold; `TypefaceOp` and `SectionOp` share `DraftPlan<T>`, `DraftSpine`, and the `TableGrip` revision law.

## [01]-[INDEX]

- [02]-[FACE_MODEL]: explicit host axes, the quartet roster, the decoration and trait capability sets, query admission, and detached evidence.
- [03]-[TYPEFACE_PIPELINE]: polymorphic census, resolution with substitute evidence, and the document face bind.
- [04]-[SECTION_MODEL]: `SectionField` row schema, its host-default snapshot, and closed fill, boundary, and hatch composition.
- [05]-[SECTION_PIPELINE]: sourced authoring, `.secstyles` import reconciliation, usage evidence, and read projection.
- [06]-[SURFACE_LEDGER]: owner table over every surface above.
- [07]-[RESEARCH]: open questions.

## [02]-[FACE_MODEL]

- Owner: `FaceWeight`, `FaceSlant`, and `FaceStretch` key the full explicit host axes; `QuartetFace` owns the four legal bold/italic corners and their per-family availability probe; `FaceDecoration` owns the four host decoration readings and every host member that answers one; `FaceTrait` owns the installation and specialty-font readings; `FaceQuery` is the one face admission and `FaceInfo` the detached descriptor.
- Law: every axis includes host `Unset`; omission remains host data rather than collapsing into `Normal`, `Upright`, or `Medium`.
- Law: `FaceDecoration` carries THREE host columns because the host answers one decoration through three different members — `Held` reads a resolved `Font`, `Sweep` probes a whole annotation run, and `Mark` writes one. `Strikeout` publishes only the font read, so its other two columns are ABSENT rather than faked, and `Across`/`RunFormat` reach the rows that answer them instead of a page re-spelling the roster.
- Law: `QuartetFace` keeps `UsesBold`/`UsesItalic` as a bool PAIR — all four corners are legal, no law bars one, and both values are `Font.FromQuartetProperties` arguments — so the pair is the host call's own argument shape, not a capability set wearing two flags.
- Law: `FaceDecoration.AxisLaw` bars `Bold` and `Italic` from an AXES query: the `Font(family, weight, style, stretch, underlined, strikethrough)` constructor spells those two as `FaceWeight`/`FaceSlant`, so admitting them twice lets one query carry two authorities over one host argument.
- Law: `FaceQuery.Of` admits every `FaceForm` case through one fold and `Mint` constructs an immutable `Font` through one union switch; the quartet arm gates on the family's own `CapabilitySet<QuartetFace>` before resolution, so a missing corner refuses before the host call rather than answering a substituted face.
- Law: `FaceResolution` carries substitute evidence when the face's `Traits` omit `Installed`; no consumer renders against a silently substituted face.
- Packages: RhinoCommon `Font`/`FontQuartet` per `.api/api-rhinocommon-drafting-resources.md` `[FONT_RESOLUTION]`/`[FONT_NAMES]`/`[FONT_METRICS]`/`[FONT_QUARTET]`, `AnnotationBase` decoration members per `.api/api-rhinocommon-annotation.md` `[03]`/`[04]`; `Domain/validation` (`CapabilitySet<T>`, `CapabilityLaw<T>`, `ICapability<T>`, `Op.Row`, `Op.AcceptValidated`); `Document/tables.md` (`ResourceName`); `Document/session.md` (`DraftFault`); Thinktecture.Runtime.Extensions; LanguageExt.Core.
- Growth: a host decoration member joins as one column on the row that already names its concept; a new specialty reading is one `FaceTrait` row every census and descriptor gains without another column.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Specialized;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using SectionFields = Rasm.Rhino.Annotation.FieldTable<Rhino.DocObjects.SectionStyle, Rhino.DocObjects.ObjectSectionFillRule>;

namespace Rasm.Rhino.Annotation;

// --- [TYPES] ---------------------------------------------------------------------------
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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class QuartetFace : ICapability<QuartetFace> {
    public static readonly QuartetFace Regular = new(
        key: "regular", usesBold: false, usesItalic: false, held: static quartet => quartet.HasRegularFont);
    public static readonly QuartetFace Bold = new(
        key: "bold", usesBold: true, usesItalic: false, held: static quartet => quartet.HasBoldFont);
    public static readonly QuartetFace Italic = new(
        key: "italic", usesBold: false, usesItalic: true, held: static quartet => quartet.HasItalicFont);
    public static readonly QuartetFace BoldItalic = new(
        key: "bold-italic", usesBold: true, usesItalic: true, held: static quartet => quartet.HasBoldItalicFont);

    internal bool UsesBold { get; }
    internal bool UsesItalic { get; }

    [UseDelegateFromConstructor]
    internal partial bool Held(FontQuartet quartet);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FaceDecoration : ICapability<FaceDecoration> {
    public static readonly FaceDecoration Bold = new(
        key: "bold", held: static font => font.Bold,
        sweep: static annotation => annotation.IsAllBold(),
        mark: static (annotation, on) => annotation.SetBold(setOn: on));
    public static readonly FaceDecoration Italic = new(
        key: "italic", held: static font => font.Italic,
        sweep: static annotation => annotation.IsAllItalic(),
        mark: static (annotation, on) => annotation.SetItalic(setOn: on));
    public static readonly FaceDecoration Underline = new(
        key: "underline", held: static font => font.Underlined,
        sweep: static annotation => annotation.IsAllUnderlined(),
        mark: static (annotation, on) => annotation.SetUnderline(setOn: on));
    public static readonly FaceDecoration Strikeout = new(
        key: "strikeout", held: static font => font.Strikeout, sweep: default, mark: default);

    internal static readonly CapabilityLaw<FaceDecoration> AxisLaw = CapabilityLaw<FaceDecoration>.Forbidden(
        barred: Seq(CapabilitySet<FaceDecoration>.Of(Bold), CapabilitySet<FaceDecoration>.Of(Italic)));

    internal Option<Func<AnnotationBase, bool>> Sweep { get; }
    internal Option<Func<AnnotationBase, bool, bool>> Mark { get; }

    [UseDelegateFromConstructor]
    internal partial bool Held(Font font);

    internal static CapabilitySet<FaceDecoration> On(Font font) =>
        CapabilitySet<FaceDecoration>.Of(toSeq(Items).Filter(row => row.Held(font: font)).ToArray());

    internal static CapabilitySet<FaceDecoration> Across(AnnotationBase annotation) =>
        CapabilitySet<FaceDecoration>.Of(
            toSeq(Items).Filter(row => row.Sweep.Exists(probe => probe(arg: annotation))).ToArray());
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FaceTrait : ICapability<FaceTrait> {
    public static readonly FaceTrait Installed = new(key: "installed", held: static font => font.IsInstalled);
    public static readonly FaceTrait Simulated = new(key: "simulated", held: static font => font.IsSimulated);
    public static readonly FaceTrait Engraving = new(key: "engraving", held: static font => font.IsEngravingFont);
    public static readonly FaceTrait Symbol = new(key: "symbol", held: static font => font.IsSymbolFont);
    public static readonly FaceTrait SingleStroke = new(key: "single-stroke", held: static font => font.IsSingleStrokeFont);
    public static readonly FaceTrait Tolerance = new(key: "tolerance", held: static font => font.IsGeometricToleranceFont);

    [UseDelegateFromConstructor]
    internal partial bool Held(Font font);

    internal static CapabilitySet<FaceTrait> On(Font font) =>
        CapabilitySet<FaceTrait>.Of(toSeq(Items).Filter(row => row.Held(font: font)).ToArray());
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FaceForm {
    private FaceForm() { }
    public sealed record Quartet(string Name, QuartetFace Face) : FaceForm;
    public sealed record Axes(
        string Family, FaceWeight Weight, FaceSlant Slant, FaceStretch Stretch,
        CapabilitySet<FaceDecoration> Decorations) : FaceForm;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FaceQuery {
    private FaceQuery() { }
    private sealed record QuartetCase(ResourceName Name, QuartetFace Face) : FaceQuery;
    private sealed record AxesCase(
        ResourceName Family, FaceWeight Weight, FaceSlant Slant, FaceStretch Stretch,
        CapabilitySet<FaceDecoration> Decorations) : FaceQuery;

    public static Fin<FaceQuery> Of(FaceForm? form, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(value: form).Bind(value => value.Switch(
            state: op,
            quartet: static (gate, input) =>
                from label in gate.AcceptValidated<ResourceName>(candidate: input.Name)
                from face in gate.Need(value: input.Face)
                select (FaceQuery)new QuartetCase(Name: label, Face: face),
            axes: static (gate, input) =>
                from family in gate.AcceptValidated<ResourceName>(candidate: input.Family)
                from weight in gate.Need(value: input.Weight)
                from slant in gate.Need(value: input.Slant)
                from stretch in gate.Need(value: input.Stretch)
                from decorations in FaceDecoration.AxisLaw.Admit(held: input.Decorations)
                select (FaceQuery)new AxesCase(
                    Family: family, Weight: weight, Slant: slant, Stretch: stretch, Decorations: decorations)));
    }

    internal Fin<Font> Mint(Op key) =>
        Switch(
            state: key,
            quartetCase: static (op, query) =>
                from family in op.Catch(() => toSeq(Font.InstalledFontsAsQuartets())
                    .Find(row => string.Equals(row.QuartetName, query.Name.Value, StringComparison.OrdinalIgnoreCase))
                    .ToFin(Fail: op.MissingContext()))
                from info in QuartetInfo.Of(quartet: family, key: op)
                from _ in guard(info.Faces.Admits(capability: query.Face), op.MissingContext()).ToFin()
                from font in op.Catch(() => Optional(Font.FromQuartetProperties(
                        quartetName: query.Name.Value, bold: query.Face.UsesBold, italic: query.Face.UsesItalic))
                    .ToFin(Fail: op.MissingContext()))
                select font,
            axesCase: static (op, query) => op.Catch(() => Optional(new Font(
                    familyName: query.Family.Value,
                    weight: query.Weight.Host,
                    style: query.Slant.Host,
                    stretch: query.Stretch.Host,
                    underlined: query.Decorations.Admits(capability: FaceDecoration.Underline),
                    strikethrough: query.Decorations.Admits(capability: FaceDecoration.Strikeout)))
                .ToFin(Fail: op.MissingContext())));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FaceInfo(
    string FaceName, string FamilyName, string FamilyPlusFaceName, string QuartetName,
    string PostScriptName, string LogfontName, string RichTextFontName, string Description,
    string EnglishFaceName, string EnglishFamilyName, string EnglishQuartetName,
    FaceWeight Weight, FaceSlant Slant, FaceStretch Stretch,
    CapabilitySet<FaceDecoration> Decorations, CapabilitySet<FaceTrait> Traits, double PointSize) {
    internal static Fin<FaceInfo> Of(Font font, Op key) => key.Catch(() =>
        from weight in key.Row<Font.FontWeight, FaceWeight>(candidate: font.Weight, ordinal: static value => (int)value)
        from slant in key.Row<Font.FontStyle, FaceSlant>(candidate: font.Style, ordinal: static value => (int)value)
        from stretch in key.Row<Font.FontStretch, FaceStretch>(candidate: font.Stretch, ordinal: static value => (int)value)
        select new FaceInfo(
            font.FaceName, font.FamilyName, font.FamilyPlusFaceName, font.QuartetName,
            font.PostScriptName, font.LogfontName, font.RichTextFontName, font.Description,
            font.EnglishFaceName, font.EnglishFamilyName, font.EnglishQuartetName,
            weight, slant, stretch,
            FaceDecoration.On(font: font), FaceTrait.On(font: font), font.PointSize));
}

public readonly record struct QuartetInfo(ResourceName Name, CapabilitySet<QuartetFace> Faces) {
    internal static Fin<QuartetInfo> Of(FontQuartet quartet, Op key) =>
        key.AcceptValidated<ResourceName>(candidate: quartet.QuartetName)
            .Map(name => new QuartetInfo(
                Name: name,
                Faces: CapabilitySet<QuartetFace>.Of(
                    toSeq(QuartetFace.Items).Filter(row => row.Held(quartet: quartet)).ToArray())));
}

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

## [03]-[TYPEFACE_PIPELINE]

- Owner: `Typefaces.Resolve` answers immutable face resolution with substitute evidence, `Typefaces.Census` answers machine-state discovery, and `TypefaceOp.Bind` is the sole document mutation.
- Law: `Bind` compares the whole `FaceInfo` projection and stops at the first seated match, projecting a candidate style only until one answers — the table read is LIVE per call, because a memo of document rows answers a style the running command has since modified.
- Law: `FaceInfo` equality is the bind's identity: the descriptor carries two `CapabilitySet` columns whose generated unordered equality makes `==` a value comparison, so no arm re-spells a member-by-member probe.
- Law: `FaceCensus.Names` is a PUBLIC request row and owes no in-package caller — the branch ruling seats altitude proof for public entries at the `apps/<app>/` plugin-shell command that composes them.
- Packages: RhinoCommon `Font` statics and `DimStyleTable` per `.api/api-rhinocommon-drafting-resources.md` `[FONT_RESOLUTION]`; `Document/tables.md` (`ResourceRef`, `ResourceIndex`); `Domain/results` (`Lease<T>.Acquire`/`Use`, `Op` receivers).
- Growth: a second face mutation is one `TypefaceOp` case beside the bind; the census gains a request row and its answer row together.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TypefaceOp {
    private TypefaceOp() { }
    public sealed record Bind(FaceQuery Query, Option<ResourceRef> Template) : TypefaceOp;

    internal Fin<Unit> Apply(RhinoDoc document, Op op) =>
        Switch(
            (Document: document, Op: op),
            bind: static (context, edit) =>
                from font in edit.Query.Mint(key: context.Op)
                from face in FaceInfo.Of(font: font, key: context.Op)
                from template in edit.Template.Traverse(address => address.Resolve(
                    document: context.Document, lens: StyleOp.Lens, key: context.Op)).As()
                from seated in Seated(document: context.Document, face: face, op: context.Op)
                from _ in seated
                    ? Fin.Succ(value: unit)
                    : Fresh(document: context.Document, template: template, font: font, op: context.Op)
                select unit);

    private static Fin<bool> Seated(RhinoDoc document, FaceInfo face, Op op) =>
        toSeq(document.DimStyles).Filter(static style => !style.IsDeleted).Fold(
            Fin.Succ(value: false),
            (state, style) => state.Bind(found => found
                ? Fin.Succ(value: true)
                : FaceInfo.Of(font: style.Font, key: op).Map(resolved => resolved == face)));

    private static Fin<Unit> Fresh(
        RhinoDoc document, Option<DimensionStyle> template, Font font, Op op) =>
        Lease<DimensionStyle>.Acquire(
                mint: () => template.IfNone(() => document.DimStyles.Current).Duplicate(
                    newName: document.DimStyles.GetUnusedStyleName(), newId: Guid.NewGuid(), newParentId: Guid.Empty),
                key: op)
            .Bind(lease => lease.Use(
                body: owned =>
                    from _ in op.Catch(() => Fin.Succ(value: Op.Side(() => owned.Font = font)))
                    from __ in op.Catch(() => ResourceIndex.Admit(
                        document.DimStyles.Add(dimstyle: owned, reference: false), op))
                    select unit,
                key: op));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Typefaces {
    public static Fin<FaceResolution> Resolve(FaceQuery query, Op? key = null) {
        Op op = key.OrDefault();
        return from admitted in op.AcceptInput(value: query)
               from font in admitted.Mint(key: op)
               from face in FaceInfo.Of(font: font, key: op)
               from substitute in face.Traits.Admits(capability: FaceTrait.Installed)
                   ? Fin.Succ(Option<FaceInfo>.None)
                   : op.Catch(() => Optional(font.GetSubstituteFont())
                       .Traverse(fallback => FaceInfo.Of(font: fallback, key: op)).As())
               select new FaceResolution(Face: face, Substitute: substitute);
    }

    public static Fin<FaceCensusAnswer> Census(FaceCensus request, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptInput(value: request).Bind(admitted => admitted.Switch(
            state: op,
            installed: static (gate, query) =>
                from fonts in gate.Catch(() => Fin.Succ(value: toSeq(query.Family.Match(
                    Some: static family => Font.InstalledFonts(familyName: family.Value),
                    None: static () => Font.InstalledFonts()))))
                from faces in fonts.TraverseM(font => FaceInfo.Of(font: font, key: gate)).As()
                select (FaceCensusAnswer)new FaceCensusAnswer.Faces(Items: faces),
            quartets: static (gate, _) =>
                from rows in gate.Catch(() => Fin.Succ(value: toSeq(Font.InstalledFontsAsQuartets())))
                from items in rows.TraverseM(row => QuartetInfo.Of(quartet: row, key: gate)).As()
                select (FaceCensusAnswer)new FaceCensusAnswer.Quartets(Items: items),
            names: static (gate, _) =>
                from names in gate.Catch(() => Fin.Succ(value: toSeq(Font.AvailableFontFaceNames())))
                from items in names.TraverseM(name =>
                    gate.AcceptValidated<ResourceName>(candidate: name)).As()
                select (FaceCensusAnswer)new FaceCensusAnswer.Names(Items: items)));
    }

    public static Fin<Unit> Commit(DocumentSession session, DraftPlan<TypefaceOp> plan, Op? key = null) =>
        DraftSpine.Commit(session: session, plan: plan,
            apply: static (document, operation, op) => operation.Apply(document: document, op: op),
            op: key.OrDefault());
}
```

## [04]-[SECTION_MODEL]

- Owner: `SectionAxis` and `SectionField` are the section drafting schema — one row per proven `SectionStyle` property, each binding a `FieldSeat<SectionStyle>` off the shared `FieldTable` mechanism; `SectionEdit` is the admitted field/payload pair; `SectionDefaults` the host-default snapshot; `SectionFillMode` the fill-mode row; `SectionFill`, `SectionBoundary`, `SectionStroke`, and `SectionHatch` encode only realizable host states; `SectionSpec` composes them with the fill rule and admitted name.
- Law: the write path is RESULT-TYPED end to end — every section property write is one `SectionField.Write`, whose payload projection is a `Fin`, so a colour leaving through the gamut boundary refuses AT the field. Statement blocks inside `Op.Catch` cannot see that refusal and land a default-constructed colour beside it.
- Law: `SectionStyle` publishes no field-override enum, so `SectionField` keys on its OWN ordinals and the mechanism's host-enum parameter carries the schema's one keyed host vocabulary, `ObjectSectionFillRule`; the second host enum this page reads admits through the kernel `Op.Row` arm the mechanism itself forwards to.
- Law: `SectionField` carries the SIX adapters `SectionStyle` proves — real, whole, pick, flag, script, and tint — and none it cannot back; a declared adapter no row reaches is decorative schema.
- Law: `SectionDefaults` reads every row off ONE leased native at first touch and releases it there, so the snapshot is VALUES; the retained native this page held for the process was one live host resource per section write it never needed.
- Law: an inactive fill, boundary, or hatch branch restores its axis from that snapshot — `SectionDefaults.On(axis)` is the whole restore, so a branch cannot forget a column and no arm hardcodes a host default.
- Law: `SectionStroke` closes the host's two boundary-linetype channels and every landing writes BOTH — `Tabled` seats `BoundaryLinetypeIndex` and clears the embedded copy, `Embedded` mints its native from the whole `StrokeDef` and clears the index, and an absent stroke restores the index default and clears the copy. Writing one side alone leaves the other channel's stale value drawing the boundary.
- Law: `SectionFillMode` closes the host fill-mode admission in both directions — the row answers the union case a projection needs and the host value a write needs — so the read path carries no catch-all arm over a roster the host closes.
- Law: absent fill, hidden boundary, and absent hatch carry no dead colours, scales, rotations, weights, or resource addresses.
- Law: resource addresses resolve inside the document grant before any table index is written; every numeric host input composes the namespace's `DraftScale`/`DraftAngle` owners and the folder's `PrintPen` plot-weight ingress onto the kernel ISO 128-24 ladder, the same owners the hatch and linetype pages write these host properties through.
- Boundary: `SectionSpec` states no cross-column invariant because every column is an admitted owner — the generated null guard IS the whole admission, and a hand-written re-check restates what construction already proved.
- Packages: `Annotation/style.md` (`FieldTable<TOwner, THostEnum>`, `FieldSeat<TOwner>`, `StyleValue`, `DraftScale`, `DraftAngle`); `Annotation/linetype.md` (`StrokeDef`, `LinetypeOp.Lens`); `Annotation/hatch.md` (`HatchSpec.Lens`); `Document/layers.md` (`PrintPen`); `Domain/results` (`Custody`); `Numerics/atoms` (`PerceptualColor`); RhinoCommon `SectionStyle` per `.api/api-rhinocommon-drafting-resources.md` `[SECTION_STYLE]`/`[SECTION_FILL]`/`[SECTION_BOUNDARY]`/`[SECTION_HATCH]`/`[SECTION_STATE]`.
- Growth: a catalog-proven `SectionStyle` property is one `SectionField` row through its adapter; the defaults snapshot, every restore, and every write gain it without another surface.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SectionAxis {
    public static readonly SectionAxis Identity = new(key: 0);
    public static readonly SectionAxis Fill = new(key: 1);
    public static readonly SectionAxis Boundary = new(key: 2);
    public static readonly SectionAxis Hatch = new(key: 3);
}

[SmartEnum<int>]
public sealed partial class SectionRule {
    public static readonly SectionRule ClosedCurves = new(key: (int)ObjectSectionFillRule.ClosedCurves);
    public static readonly SectionRule SolidObjects = new(key: (int)ObjectSectionFillRule.SolidObjects);
    internal ObjectSectionFillRule Host => (ObjectSectionFillRule)Key;
}

[SmartEnum<int>]
public sealed partial class SectionFillMode {
    public static readonly SectionFillMode None = new(
        key: (int)SectionBackgroundFillMode.None, fill: static (_, _) => new SectionFill.None());
    public static readonly SectionFillMode Viewport = new(
        key: (int)SectionBackgroundFillMode.Viewport, fill: static (_, _) => new SectionFill.Viewport());
    public static readonly SectionFillMode Solid = new(
        key: (int)SectionBackgroundFillMode.SolidColor,
        fill: static (display, print) => new SectionFill.Solid(Display: display, Print: print));

    internal SectionBackgroundFillMode Host => (SectionBackgroundFillMode)Key;

    [UseDelegateFromConstructor]
    internal partial SectionFill Fill(PerceptualColor display, PerceptualColor print);
}

[SmartEnum<int>]
public sealed partial class SectionField {
    // --- [IDENTITY]
    public static readonly SectionField Name = Script(0, SectionAxis.Identity, static s => s.Name, static (s, v) => s.Name = v);
    public static readonly SectionField FillRule = Pick(1, SectionAxis.Identity, static s => s.SectionFillRule, static (s, v) => s.SectionFillRule = v);

    // --- [FILL]
    public static readonly SectionField FillMode = Pick(2, SectionAxis.Fill, static s => s.BackgroundFillMode, static (s, v) => s.BackgroundFillMode = v);
    public static readonly SectionField FillColor = Tint(3, SectionAxis.Fill, static s => s.BackgroundFillColor, static (s, v) => s.BackgroundFillColor = v);
    public static readonly SectionField FillPrintColor = Tint(4, SectionAxis.Fill, static s => s.BackgroundFillPrintColor, static (s, v) => s.BackgroundFillPrintColor = v);

    // --- [BOUNDARY]
    public static readonly SectionField BoundaryVisible = Flag(5, SectionAxis.Boundary, static s => s.BoundaryVisible, static (s, v) => s.BoundaryVisible = v);
    public static readonly SectionField BoundaryColor = Tint(6, SectionAxis.Boundary, static s => s.BoundaryColor, static (s, v) => s.BoundaryColor = v);
    public static readonly SectionField BoundaryPrintColor = Tint(7, SectionAxis.Boundary, static s => s.BoundaryPrintColor, static (s, v) => s.BoundaryPrintColor = v);
    public static readonly SectionField BoundaryWidthScale = Real(8, SectionAxis.Boundary, static s => s.BoundaryWidthScale, static (s, v) => s.BoundaryWidthScale = v);
    public static readonly SectionField BoundaryPlotWeight = Real(9, SectionAxis.Boundary, static s => s.BoundaryPlotWeightMillimeters, static (s, v) => s.BoundaryPlotWeightMillimeters = v);
    public static readonly SectionField BoundaryLinetypeIndex = Whole(10, SectionAxis.Boundary, static s => s.BoundaryLinetypeIndex, static (s, v) => s.BoundaryLinetypeIndex = v);

    // --- [HATCH]
    public static readonly SectionField HatchIndex = Whole(11, SectionAxis.Hatch, static s => s.HatchIndex, static (s, v) => s.HatchIndex = v);
    public static readonly SectionField HatchScale = Real(12, SectionAxis.Hatch, static s => s.HatchScale, static (s, v) => s.HatchScale = v);
    public static readonly SectionField HatchRotation = Real(13, SectionAxis.Hatch, static s => s.HatchRotationRadians, static (s, v) => s.HatchRotationRadians = v);
    public static readonly SectionField HatchColor = Tint(14, SectionAxis.Hatch, static s => s.HatchPatternColor, static (s, v) => s.HatchPatternColor = v);
    public static readonly SectionField HatchPrintColor = Tint(15, SectionAxis.Hatch, static s => s.HatchPatternPrintColor, static (s, v) => s.HatchPatternPrintColor = v);

    internal SectionAxis Axis { get; }

    [UseDelegateFromConstructor]
    internal partial bool Accepts(StyleValue value);

    [UseDelegateFromConstructor]
    internal partial Fin<StyleValue> Read(SectionStyle style, Op key);

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Write(SectionStyle style, StyleValue value, Op key);

    internal static Seq<SectionField> On(SectionAxis axis) => ByAxis.Value[axis];

    private static readonly Lazy<FrozenDictionary<SectionAxis, Seq<SectionField>>> ByAxis = new(static () =>
        toSeq(Items).GroupBy(static row => row.Axis)
            .ToFrozenDictionary(static group => group.Key, static group => toSeq(group).Strict()));

    internal static Fin<Unit> Apply(SectionStyle style, Seq<SectionEdit> run, Op key) =>
        run.TraverseM(edit => edit.Field.Write(style: style, value: edit.Value, key: key)).As().Map(static _ => unit);

    private static SectionField Real(int key, SectionAxis axis, Func<SectionStyle, double> get, Action<SectionStyle, double> set) =>
        new(key: key, axis: axis, seat: SectionFields.Real(get, set));

    private static SectionField Whole(int key, SectionAxis axis, Func<SectionStyle, int> get, Action<SectionStyle, int> set) =>
        new(key: key, axis: axis, seat: SectionFields.Whole(get, set));

    private static SectionField Pick<TEnum>(int key, SectionAxis axis, Func<SectionStyle, TEnum> get, Action<SectionStyle, TEnum> set)
        where TEnum : struct, Enum =>
        new(key: key, axis: axis, seat: SectionFields.Pick(get, set));

    private static SectionField Flag(int key, SectionAxis axis, Func<SectionStyle, bool> get, Action<SectionStyle, bool> set) =>
        new(key: key, axis: axis, seat: SectionFields.Flag(get, set));

    private static SectionField Script(int key, SectionAxis axis, Func<SectionStyle, string> get, Action<SectionStyle, string> set) =>
        new(key: key, axis: axis, seat: SectionFields.Script(get, set));

    private static SectionField Tint(int key, SectionAxis axis, Func<SectionStyle, System.Drawing.Color> get, Action<SectionStyle, System.Drawing.Color> set) =>
        new(key: key, axis: axis, seat: SectionFields.Tint(get, set));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SectionEdit {
    private SectionEdit(SectionField field, StyleValue value) {
        Field = field;
        Value = value;
    }

    public SectionField Field { get; }
    public StyleValue Value { get; }

    internal static Fin<SectionEdit> Of(SectionField field, StyleValue value, Op key) =>
        guard(field.Accepts(value: value), key.InvalidInput()).ToFin()
            .Map(_ => new SectionEdit(field: field, value: value));

    internal static Fin<Seq<SectionEdit>> All(Op key, params ReadOnlySpan<(SectionField Field, StyleValue Value)> rows) =>
        toSeq(rows.ToArray()).Traverse(row => Of(field: row.Field, value: row.Value, key: key).ToValidation())
            .As().ToFin();
}

internal static class SectionDefaults {
    internal static Fin<Seq<SectionEdit>> On(SectionAxis axis) =>
        Rows.Value.Map(rows => rows.Filter(row => row.Field.Axis == axis));

    internal static Fin<SectionEdit> Row(SectionField field) =>
        Rows.Value.Bind(rows => rows.Find(row => row.Field == field)
            .ToFin(Fail: Op.Of(name: nameof(SectionDefaults)).MissingContext()));

    private static readonly Lazy<Fin<Seq<SectionEdit>>> Rows = new(static () => {
        Op key = Op.Of(name: nameof(SectionDefaults));
        return Lease<SectionStyle>.Acquire(mint: static () => new SectionStyle(), key: key).Bind(lease => lease.Use(
            body: seed => toSeq(SectionField.Items).Traverse(field => field
                .Read(style: seed, key: key)
                .Bind(value => SectionEdit.Of(field: field, value: value, key: key))
                .ToValidation()).As().ToFin(),
            key: key));
    });
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionFill {
    private SectionFill() { }
    public sealed record None : SectionFill;
    public sealed record Viewport : SectionFill;
    public sealed record Solid(PerceptualColor Display, PerceptualColor Print) : SectionFill;

    internal Fin<Seq<SectionEdit>> Rows(Op key) => Switch(
        state: key,
        none: static (op, _) => Restored(mode: SectionFillMode.None, key: op),
        viewport: static (op, _) => Restored(mode: SectionFillMode.Viewport, key: op),
        solid: static (op, fill) => SectionEdit.All(op,
            (SectionField.FillMode, (StyleValue)StyleValue.Of(SectionFillMode.Solid.Host)),
            (SectionField.FillColor, new StyleValue.Tint(Value: fill.Display)),
            (SectionField.FillPrintColor, new StyleValue.Tint(Value: fill.Print))));

    private static Fin<Seq<SectionEdit>> Restored(SectionFillMode mode, Op key) =>
        from defaults in SectionDefaults.On(axis: SectionAxis.Fill)
        from stated in SectionEdit.Of(field: SectionField.FillMode, value: StyleValue.Of(mode.Host), key: key)
        select defaults.Filter(static row => row.Field != SectionField.FillMode).Add(stated);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionStroke {
    private SectionStroke() { }
    public sealed record Tabled(ResourceRef Source) : SectionStroke;
    public sealed record Embedded(StrokeDef Definition) : SectionStroke;

    internal Fin<Unit> Attach(SectionStyle style, RhinoDoc document, Op key) => Switch(
        (Style: style, Document: document, Key: key),
        tabled: static (context, row) =>
            from live in row.Source.Resolve(document: context.Document, lens: LinetypeOp.Lens, key: context.Key)
            from seated in SectionEdit.Of(
                field: SectionField.BoundaryLinetypeIndex,
                value: new StyleValue.Whole(Value: live.LinetypeIndex), key: context.Key)
            from _ in SectionField.Apply(style: context.Style, run: Seq(seated), key: context.Key)
            from __ in context.Key.Catch(() => Fin.Succ(value: Op.Side(context.Style.RemoveBoundaryLinetype)))
            select unit,
        embedded: static (context, row) =>
            from _ in Detach(style: context.Style, key: context.Key)
            from seed in Lease<Linetype>.Acquire(mint: static () => new Linetype(), key: context.Key)
            from __ in seed.Use(
                body: native =>
                    from ___ in row.Definition.Apply(document: context.Document, linetype: native, key: context.Key)
                    from ____ in context.Key.Confirm(success: context.Style.SetBoundaryLinetype(native))
                    select unit,
                key: context.Key)
            select unit);

    internal static Fin<Unit> Detach(SectionStyle style, Op key) =>
        from row in SectionDefaults.Row(field: SectionField.BoundaryLinetypeIndex)
        from _ in SectionField.Apply(style: style, run: Seq(row), key: key)
        from __ in key.Catch(() => Fin.Succ(value: Op.Side(style.RemoveBoundaryLinetype)))
        select unit;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionBoundary {
    private SectionBoundary() { }
    public sealed record Hidden : SectionBoundary;
    public sealed record Stroke(
        PerceptualColor Display, PerceptualColor Print,
        DraftScale Width, PrintPen PlotWeight,
        Option<SectionStroke> Linetype) : SectionBoundary;

    internal Fin<Seq<SectionEdit>> Rows(Op key) => Switch(
        state: key,
        hidden: static (op, _) =>
            from defaults in SectionDefaults.On(axis: SectionAxis.Boundary)
            from stated in SectionEdit.Of(
                field: SectionField.BoundaryVisible, value: new StyleValue.Flag(Value: false), key: op)
            select defaults
                .Filter(static row => row.Field != SectionField.BoundaryVisible
                    && row.Field != SectionField.BoundaryLinetypeIndex)
                .Add(stated),
        stroke: static (op, boundary) => SectionEdit.All(op,
            (SectionField.BoundaryVisible, (StyleValue)new StyleValue.Flag(Value: true)),
            (SectionField.BoundaryColor, new StyleValue.Tint(Value: boundary.Display)),
            (SectionField.BoundaryPrintColor, new StyleValue.Tint(Value: boundary.Print)),
            (SectionField.BoundaryWidthScale, new StyleValue.Real(Value: boundary.Width.Value)),
            (SectionField.BoundaryPlotWeight, new StyleValue.Real(Value: boundary.PlotWeight.ToHost()))));

    internal Fin<Unit> Bind(SectionStyle style, RhinoDoc document, Op key) => Switch(
        (Style: style, Document: document, Key: key),
        hidden: static (context, _) => SectionStroke.Detach(style: context.Style, key: context.Key),
        stroke: static (context, boundary) => boundary.Linetype.Match(
            Some: row => row.Attach(style: context.Style, document: context.Document, key: context.Key),
            None: () => SectionStroke.Detach(style: context.Style, key: context.Key)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionHatch {
    private SectionHatch() { }
    public sealed record None : SectionHatch;
    public sealed record Pattern(
        ResourceRef Resource, DraftScale Scale, DraftAngle Rotation,
        PerceptualColor Display, PerceptualColor Print) : SectionHatch;

    internal Fin<Seq<SectionEdit>> Rows(RhinoDoc document, Op key) => Switch(
        (Document: document, Key: key),
        none: static (_, _) => SectionDefaults.On(axis: SectionAxis.Hatch),
        pattern: static (context, hatch) =>
            from live in hatch.Resource.Resolve(document: context.Document, lens: HatchSpec.Lens, key: context.Key)
            from rows in SectionEdit.All(context.Key,
                (SectionField.HatchIndex, (StyleValue)new StyleValue.Whole(Value: live.Index)),
                (SectionField.HatchScale, new StyleValue.Real(Value: hatch.Scale.Value)),
                (SectionField.HatchRotation, new StyleValue.Real(Value: hatch.Rotation.Value)),
                (SectionField.HatchColor, new StyleValue.Tint(Value: hatch.Display)),
                (SectionField.HatchPrintColor, new StyleValue.Tint(Value: hatch.Print)))
            select rows);
}

[ComplexValueObject]
[ValidationError]
public sealed partial class SectionSpec {
    public ResourceName Name { get; }
    public SectionFill Fill { get; }
    public SectionBoundary Boundary { get; }
    public SectionHatch Hatch { get; }
    public SectionRule Rule { get; }

    internal Fin<Unit> Apply(SectionStyle style, RhinoDoc document, Op key) =>
        from identity in SectionEdit.All(key,
            (SectionField.Name, (StyleValue)new StyleValue.Script(Value: Name.Value)),
            (SectionField.FillRule, StyleValue.Of(Rule.Host)))
        from fill in Fill.Rows(key: key)
        from boundary in Boundary.Rows(key: key)
        from hatch in Hatch.Rows(document: document, key: key)
        from _ in SectionField.Apply(style: style, run: identity + fill + boundary + hatch, key: key)
        from __ in Boundary.Bind(style: style, document: document, key: key)
        select unit;
}
```

## [05]-[SECTION_PIPELINE]

- Owner: `SectionSource` carries local versus reference seating; `SectionOp` carries one `Table` case over the namespace's shared verbs beside the two this table alone has — source-discriminated authoring and `.secstyles` import reconciliation; `ImportLanding` carries one landed row beside its own reversal; `SectionUsage` and `SectionSnapshot` carry the read projection.
- Law: amendment, renaming, retagging, plural delete, and current election are the SHARED `TableOp` over `SectionOp.Grip` — the duplicate-then-`Modify` law, the plural delete arity, and the tag algebra are the namespace owner's and this page re-spells none of them. Re-spelling any of those verbs as a page-local arm executes the deleted form.
- Law: `Author` stays a page-local case because `SectionSource` is an axis no `TDef` carries — `SectionStyleTable` publishes TWO seats, `Add` and `AddReferenceSectionStyle`, while the grip's one `Seat` column reaches the first alone; the def still mints and drains through `Grip.Mint`, so the source column decides the seat and nothing else.
- Law: the section table publishes neither a user-string bag nor a current row, so `Tags` answers a surface whose read is empty and whose writes refuse, and `Elect` refuses typed — a `TableOp.Retag` carrying pairs and every `TableOp.SetCurrent` answer an unsupported fault rather than a silent success, while clearing an absent bag stays honestly vacuous.
- Law: delete usage-gates every resolved target inside the grip's `Retire` row before one `SectionStyleTable.Delete(IEnumerable<int>, bool)` call; any retained row makes the whole request fail before mutation, and the in-use-warning verdict stays the host's own `quiet`-derived value — the three-argument overload exists to OVERRIDE that derivation, so re-deriving it here forks one rule across two owners.
- Law: import stays a page-local case because the grip's `Ingest` column reads a file into detached natives with no document in hand while section import LANDS a second table's rows: it canonicalizes hatch references through `PatternDef`, preflights names, and compensates added patterns and added or replaced styles in reverse landing order.
- Law: every landed row carries its OWN reversal, so rollback is one fold over any landing run and a reused pattern states a vacuous undo instead of an `Added` flag every rollback re-tests.
- Law: preflight ACCUMULATES — both admission runs traverse onto `Validation` — and each uniqueness question is a KEYED carrier the later lookup reads, so the duplicate guard and the hatch re-key share one authority instead of a scan beside a count test. Name keys are `ResourceName` values, which carry the host tables' ordinal-ignore-case comparer.
- Law: every rollback and drain leg spells `HostInteraction.Silent`; the host `quiet` boolean has ONE owner on this spine and a literal beside it forks the interaction axis.
- Law: import owns every native it reads or retains — both `ReadFromFile` out-arrays and each copy-retained pre-existing style — and drains all three sets through kernel `Custody` on success and on every refusal leg.
- Law: `SectionUsage` is the ONE usage authority — the snapshot carries it rather than a second host `InUse` read whose verdict can disagree with the census beside it.
- Boundary: the projection reads host properties directly instead of routing through the schema's `Read` column, because it must reshape into the closed fill, boundary, and hatch cases and every row unwraps twice on that route; the schema's read column serves the defaults snapshot, which needs no reshaping.
- Packages: `Annotation/style.md` (`TableGrip<T, TDef>`, `TableOp<T, TDef>`, `TagSurface`, `DraftScale`, `DraftAngle`); `Annotation/hatch.md` (`PatternDef`); `Annotation/linetype.md` (`StrokeDef`); `Document/commit.md` (`DocumentCommit.Compensated`, `HostInteraction`); `Document/session.md` (`DocumentSession.Demand`, `SessionNeed`, `DraftFault`); `Domain/results` (`Custody`); RhinoCommon `SectionStyleTable` per `.api/api-rhinocommon-drafting-resources.md` `[SECTION_TABLE]`.
- Growth: a section-only verb is one `SectionOp` case with its arm; a verb every component table shares is one `TableOp` case; a new landing kind is one `ImportLanding` mint carrying its reversal.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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
    public sealed record Table(TableOp<SectionStyle, SectionSpec> Verb) : SectionOp;
    public sealed record Author(SectionSpec Spec, SectionSource Source) : SectionOp;
    public sealed record Import(DraftPath Path, HostInteraction Interaction) : SectionOp;

    internal static readonly ResourceLens<SectionStyle> Lens = new(
        ById: static (document, id) => document.SectionStyles.Find(id: id, ignoreDeletedSectionStyles: true) is var index && index >= 0
            ? document.SectionStyles.FindIndex(index: index)
            : null,
        ByName: static (document, name) => document.SectionStyles.FindName(name: name),
        ByIndex: static (document, index) => document.SectionStyles.FindIndex(index: index));

    internal static readonly TableGrip<SectionStyle, SectionSpec> Grip = new(
        Lens,
        Named: static def => def.Name,
        Title: static (style, key) => key.AcceptValidated<ResourceName>(candidate: style.Name),
        Index: static style => style.Index,
        Duplicate: static style => new SectionStyle(style),
        Tags: static _ => new TagSurface(
            Read: static () => new NameValueCollection(),
            Set: static (_, _) => false,
            Drop: static _ => false,
            Clear: static () => { }),
        Mint: static (document, def, key) =>
            from shaped in key.Catch(() => Fin.Succ(value: new SectionStyle()))
            from _ in def.Apply(style: shaped, document: document, key: key)
                .BindFail(primary => Fin.Fail<Unit>(error: primary).Rollback(
                    release: () => Custody.Dispose(held: Seq(shaped), key: key), key: key))
            select shaped,
        Revise: static (document, copy, def, key) => def.Apply(style: copy, document: document, key: key),
        Retitle: static (copy, name, key) => key.Catch(() => Fin.Succ(value: Op.Side(() => copy.Name = name.Value))),
        Modify: static (document, copy, index, interaction, key) => key.Confirm(success: document.SectionStyles.Modify(
            sectionstyle: copy, index: index, quiet: interaction.IsQuiet)),
        Seat: static (document, style, key) => key.Catch(() => ResourceIndex.Admit(
            document.SectionStyles.Add(sectionstyle: style), key)),
        Retire: static (document, indices, interaction, key) =>
            from _ in indices.TraverseM(index =>
                from usage in SectionUsage.Read(document: document, index: index, key: key)
                from __ in guard(!usage.Bound, key.InvalidInput()).ToFin()
                select unit).As()
            from removed in key.Catch(() => Fin.Succ(value: document.SectionStyles.Delete(
                sectionStyleIndices: indices.AsIterable(), quiet: interaction.IsQuiet)))
            from __ in guard(removed == indices.Count, key.InvalidResult()).ToFin()
            select unit,
        Elect: static (_, _, _, key) => Fin.Fail<Unit>(error: key.Unsupported(
            valueType: typeof(SectionStyle), outputType: typeof(Unit))));

    private sealed record PatternIntent(int Source, HatchPattern Pattern, Option<ResourceIndex> Existing) {
        internal static Fin<PatternIntent> Admit(RhinoDoc document, HatchPattern pattern, Op op) =>
            from source in op.Need(value: pattern)
            from definition in PatternDef.Read(pattern: source, key: op)
            from canonical in definition.Mint(key: op)
            from existing in Optional(document.HatchPatterns.FindName(name: canonical.Name)).Match(
                Some: held =>
                    from current in PatternDef.Read(pattern: held, key: op)
                    from _ in guard(definition == current, op.InvalidInput()).ToFin()
                    select Some(ResourceIndex.Create(held.Index)),
                None: static () => Fin.Succ(Option<ResourceIndex>.None))
            select new PatternIntent(Source: source.Index, Pattern: canonical, Existing: existing);
    }

    private sealed record ImportSeat(ResourceIndex Index, SectionStyle Original);
    private sealed record SectionIntent(SectionStyle Style, ResourceName Name, Option<ImportSeat> Seat);

    private sealed record ImportLanding(ResourceIndex Index, Func<Op, Fin<Unit>> Undo);

    private readonly record struct ImportSpoil(Seq<SectionStyle> Styles, Seq<HatchPattern> Patterns);

    internal Fin<Unit> Apply(RhinoDoc document, Op op) => Switch(
        (Document: document, Op: op),
        table: static (context, edit) => edit.Verb.Apply(grip: Grip, document: context.Document, op: context.Op),
        author: static (context, edit) =>
            from _ in guard(!Grip.Occupied(context.Document, edit.Spec.Name), context.Op.InvalidInput()).ToFin()
            from minted in Grip.Mint(context.Document, edit.Spec, context.Op)
            from __ in new Lease<SectionStyle>.Owned(Value: minted).Use(
                body: owned =>
                    from ___ in context.Op.Catch(() => ResourceIndex.Admit(
                        edit.Source.Add(document: context.Document, style: owned), context.Op))
                    select unit,
                key: context.Op)
            select unit,
        import: static (context, edit) => ImportFile(
            document: context.Document, path: edit.Path, interaction: edit.Interaction, op: context.Op));

    private static Fin<Unit> ImportFile(RhinoDoc document, DraftPath path, HostInteraction interaction, Op op) =>
        from read in op.Catch(() => SectionStyle.ReadFromFile(
                filename: path.Value, sectionStyles: out SectionStyle[] styles, hatchPatterns: out HatchPattern[] patterns)
            ? Fin.Succ(value: new ImportSpoil(Styles: toSeq(styles ?? []), Patterns: toSeq(patterns ?? [])))
            : Fin.Fail<ImportSpoil>(error: op.InvalidResult()))
        from plan in Preflight(document: document, spoil: read, op: op)
            .BindFail(primary => Drained<(Seq<PatternIntent> Patterns, Seq<SectionIntent> Styles)>(
                primary: primary, spoil: read, op: op))
        let owned = read with { Styles = read.Styles + Retained(plan.Styles) }
        from patterns in DocumentCommit.Compensated(
                source: plan.Patterns,
                land: intent => LandPattern(document: document, intent: intent, op: op),
                rollback: landed => Rollback(landed: landed, op: op))
            .BindFail(primary => Drained<Seq<ImportLanding>>(primary: primary, spoil: owned, op: op))
        let targets = toHashMap(plan.Patterns.Zip(
            patterns, static (intent, landing) => (intent.Source, landing.Index)))
        from _ in ImportSections(
                document: document, interaction: interaction,
                plan: plan.Styles, patterns: patterns, targets: targets, op: op)
            .BindFail(primary => Drained<Unit>(primary: primary, spoil: owned, op: op))
        from __ in Custody.Dispose(held: owned.Styles, key: op)
        from ___ in Custody.Dispose(held: owned.Patterns, key: op)
        select unit;

    private static Seq<SectionStyle> Retained(Seq<SectionIntent> plan) =>
        plan.Choose(static intent => intent.Seat.Map(static seat => seat.Original));

    private static Fin<T> Drained<T>(Error primary, ImportSpoil spoil, Op op) =>
        Fin.Fail<T>(error: primary)
            .Rollback(release: () => Custody.Dispose(held: spoil.Styles, key: op), key: op)
            .Rollback(release: () => Custody.Dispose(held: spoil.Patterns, key: op), key: op);

    private static Fin<(Seq<PatternIntent> Patterns, Seq<SectionIntent> Styles)> Preflight(
        RhinoDoc document, ImportSpoil spoil, Op op) => op.Catch(() =>
        from _ in guard(!spoil.Styles.IsEmpty, op.InvalidResult()).ToFin()
        from patterns in spoil.Patterns.Traverse(pattern => PatternIntent.Admit(
            document: document, pattern: pattern, op: op).ToValidation()).As().ToFin()
        from bySource in Keyed(rows: patterns, key: static row => row.Source, op: op)
        from __ in Keyed(rows: patterns, key: static row => ResourceName.Create(row.Pattern.Name), op: op)
        from styles in spoil.Styles.Traverse(style => (
            from value in op.AcceptInput(value: style)
            from name in op.AcceptValidated<ResourceName>(candidate: value.Name)
            from seat in Optional(document.SectionStyles.FindName(name: name.Value)).Match(
                Some: held =>
                    from index in op.Catch(() => ResourceIndex.Admit(document.SectionStyles.Find(name: held.Name), op))
                    from original in op.Catch(() => Fin.Succ(value: new SectionStyle(held)))
                    select Some(new ImportSeat(Index: index, Original: original)),
                None: static () => Fin.Succ(Option<ImportSeat>.None))
            select new SectionIntent(Style: value, Name: name, Seat: seat)).ToValidation()).As().ToFin()
        from ___ in Keyed(rows: styles, key: static row => row.Name, op: op)
        from ____ in guard(
            styles.ForAll(row => row.Style.HatchIndex < 0 || bySource.ContainsKey(row.Style.HatchIndex)),
            op.InvalidInput()).ToFin()
        select (patterns, styles));

    private static Fin<HashMap<TKey, TRow>> Keyed<TKey, TRow>(Seq<TRow> rows, Func<TRow, TKey> key, Op op) {
        HashMap<TKey, TRow> map = rows.Fold(
            HashMap<TKey, TRow>(), (state, row) => state.AddOrUpdate(key(arg: row), row));
        return guard(map.Count == rows.Count, op.InvalidInput()).ToFin().Map(_ => map);
    }

    private static Fin<ImportLanding> LandPattern(RhinoDoc document, PatternIntent intent, Op op) =>
        intent.Existing.Match(
            Some: target => Fin.Succ(value: new ImportLanding(
                Index: target, Undo: static _ => Fin.Succ(unit))),
            None: () => op.Catch(() => ResourceIndex.Admit(document.HatchPatterns.Add(pattern: intent.Pattern), op)
                .Map(target => new ImportLanding(
                    Index: target,
                    Undo: key => key.Confirm(success: document.HatchPatterns.Delete(
                        hatchPatternIndex: target.Value, quiet: HostInteraction.Silent.IsQuiet))))));

    private static Fin<Unit> ImportSections(
        RhinoDoc document, HostInteraction interaction,
        Seq<SectionIntent> plan, Seq<ImportLanding> patterns, HashMap<int, ResourceIndex> targets, Op op) =>
        DocumentCommit.Compensated(
                source: plan,
                land: intent => LandSection(
                    document: document, intent: intent, targets: targets, interaction: interaction, op: op),
                rollback: landed => Rollback(landed: landed, op: op))
            .Map(static _ => unit)
            .BindFail(primary => Reverted(primary: primary, runs: Seq(patterns), op: op));

    private static Fin<ImportLanding> LandSection(
        RhinoDoc document, SectionIntent intent, HashMap<int, ResourceIndex> targets,
        HostInteraction interaction, Op op) =>
        from hatch in intent.Style.HatchIndex < 0
            ? Fin.Succ(value: intent.Style.HatchIndex)
            : targets.Find(intent.Style.HatchIndex).Map(static index => index.Value)
                .ToFin(Fail: op.MissingContext())
        from _ in op.Catch(() => Fin.Succ(value: Op.Side(() => intent.Style.HatchIndex = hatch)))
        from landed in intent.Seat.Match(
            Some: seat =>
                from __ in op.Confirm(success: document.SectionStyles.Modify(
                    sectionstyle: intent.Style, index: seat.Index.Value, quiet: interaction.IsQuiet))
                select new ImportLanding(
                    Index: seat.Index,
                    Undo: key => key.Confirm(success: document.SectionStyles.Modify(
                        sectionstyle: seat.Original, index: seat.Index.Value,
                        quiet: HostInteraction.Silent.IsQuiet))),
            None: () => op.Catch(() => ResourceIndex.Admit(
                    document.SectionStyles.Add(sectionstyle: intent.Style), op)
                .Map(index => new ImportLanding(
                    Index: index,
                    Undo: key => key.Confirm(success: document.SectionStyles.Delete(
                        index: index.Value, quiet: HostInteraction.Silent.IsQuiet))))))
        select landed;

    private static Fin<Unit> Rollback(Seq<ImportLanding> landed, Op op) =>
        toSeq(landed.AsIterable().Reverse()).Fold(
            Fin.Succ(value: unit), (state, row) => Merge(prior: state, next: row.Undo(arg: op)));

    private static Fin<T> Reverted<T>(Error primary, Seq<Seq<ImportLanding>> runs, Op op) =>
        runs.Fold(Fin.Succ(value: unit), (state, run) => Merge(prior: state, next: Rollback(landed: run, op: op)))
            .Match(Succ: _ => Fin.Fail<T>(error: primary), Fail: cleanup => Fin.Fail<T>(error: primary + cleanup));

    private static Fin<Unit> Merge(Fin<Unit> prior, Fin<Unit> next) => prior.Match(
        Succ: _ => next,
        Fail: first => next.Match(
            Succ: _ => Fin.Fail<Unit>(error: first),
            Fail: second => Fin.Fail<Unit>(error: first + second)));
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError]
public sealed partial class SectionUsage {
    public bool HostBound { get; }
    public int Definitions { get; }
    public int Objects { get; }
    public int Layers { get; }
    public bool Bound => HostBound || Definitions > 0 || Objects > 0 || Layers > 0;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref bool hostBound, ref int definitions, ref int objects, ref int layers) {
        (int instances, int rows, int layerRows) = (definitions, objects, layers);
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
            (instances < 0, () => new ValidationClause(string.Join(" | ", new object?[] { Op.Of(), nameof(Definitions), instances, "a non-negative tally" }))),
            (rows < 0, () => new ValidationClause(string.Join(" | ", new object?[] { Op.Of(), nameof(Objects), rows, "a non-negative tally" }))),
            (layerRows < 0, () => new ValidationClause(string.Join(" | ", new object?[] { Op.Of(), nameof(Layers), layerRows, "a non-negative tally" })))));
    }

    internal static Fin<SectionUsage> Read(RhinoDoc document, int index, Op key) => key.Catch(() => {
        bool hostBound = document.SectionStyles.InUse(
            index: index,
            instanceDefinitionCount: out int definitions,
            objectCount: out int objects,
            layerCount: out int layers);
        return key.AcceptValidated<SectionUsage>(
            fault: Validate(hostBound, definitions, objects, layers, out SectionUsage? admitted), admitted: admitted);
    });
}

public sealed record SectionSnapshot(
    ResourceId Key, SectionSpec Spec, bool IsUnset, SectionUsage Usage) : IDetachedDocumentResult;

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionAsk {
    private SectionAsk() { }
    public sealed record State(ResourceRef Target) : SectionAsk;
    public sealed record TableState : SectionAsk;
    public sealed record MintName : SectionAsk;

    internal Fin<SectionAnswer> Answer(RhinoDoc document, Op op) => Switch(
        (Document: document, Op: op),
        state: static (context, ask) => context.Op.Catch(() =>
            from style in ask.Target.Resolve(document: context.Document, lens: SectionOp.Lens, key: context.Op)
            from usage in SectionUsage.Read(
                document: context.Document, index: SectionOp.Grip.Index(style), key: context.Op)
            from spec in Project(style: style, key: context.Op)
            select (SectionAnswer)new SectionAnswer.State(Snapshot: new SectionSnapshot(
                Key: ResourceId.Create(style.Id), Spec: spec, IsUnset: style.IsUnset, Usage: usage))),
        tableState: static (context, _) => context.Op.Catch(() =>
            context.Op.AcceptValidated<DraftCount>(candidate: context.Document.SectionStyles.ActiveCount)
                .Map(static count => (SectionAnswer)new SectionAnswer.Rows(ActiveCount: count))),
        mintName: static (context, _) => context.Op.Catch(() =>
            context.Op.AcceptValidated<ResourceName>(
                    candidate: context.Document.SectionStyles.GetUnusedSectionStyleName())
                .Map(static name => (SectionAnswer)new SectionAnswer.Minted(Name: name))));

    private static Fin<SectionSpec> Project(SectionStyle style, Op key) => key.Catch(() =>
        from fillDisplay in PerceptualColor.OfHost(host: style.BackgroundFillColor, key: key)
        from fillPrint in PerceptualColor.OfHost(host: style.BackgroundFillPrintColor, key: key)
        from boundaryDisplay in PerceptualColor.OfHost(host: style.BoundaryColor, key: key)
        from boundaryPrint in PerceptualColor.OfHost(host: style.BoundaryPrintColor, key: key)
        from hatchDisplay in PerceptualColor.OfHost(host: style.HatchPatternColor, key: key)
        from hatchPrint in PerceptualColor.OfHost(host: style.HatchPatternPrintColor, key: key)
        from plotWeight in PrintPen.OfHost(weight: style.BoundaryPlotWeightMillimeters, key: key)
        from width in key.AcceptValidated<DraftScale>(candidate: style.BoundaryWidthScale)
        from name in key.AcceptValidated<ResourceName>(candidate: style.Name)
        from rule in SectionFields.Row<SectionRule>(field: style.SectionFillRule, key: key)
        from mode in key.Row<SectionBackgroundFillMode, SectionFillMode>(
            candidate: style.BackgroundFillMode, ordinal: static value => (int)value)
        from linetype in Stroke(style: style, key: key)
        from hatch in Hatch(style: style, display: hatchDisplay, print: hatchPrint, key: key)
        select SectionSpec.Create(
            name: name,
            fill: mode.Fill(display: fillDisplay, print: fillPrint),
            boundary: style.BoundaryVisible
                ? new SectionBoundary.Stroke(boundaryDisplay, boundaryPrint, width, plotWeight, linetype)
                : new SectionBoundary.Hidden(),
            hatch: hatch,
            rule: rule));

    private static Fin<Option<SectionStroke>> Stroke(SectionStyle style, Op key) =>
        Optional(style.GetBoundaryLinetype()) is { IsSome: true, Case: Linetype embedded }
            ? new Lease<Linetype>.Owned(Value: embedded).Use(
                body: owned => StrokeDef.Read(linetype: owned, key: key)
                    .Map(static definition => Some<SectionStroke>(new SectionStroke.Embedded(Definition: definition))),
                key: key)
            : style.BoundaryLinetypeIndex >= 0
                ? ResourceRef.Of(index: style.BoundaryLinetypeIndex, key: key)
                    .Map(static address => Some<SectionStroke>(new SectionStroke.Tabled(Source: address)))
                : Fin.Succ(Option<SectionStroke>.None);

    private static Fin<SectionHatch> Hatch(
        SectionStyle style, PerceptualColor display, PerceptualColor print, Op key) =>
        style.HatchIndex < 0
            ? Fin.Succ<SectionHatch>(value: new SectionHatch.None())
            : from address in ResourceRef.Of(index: style.HatchIndex, key: key)
              from scale in key.AcceptValidated<DraftScale>(candidate: style.HatchScale)
              from rotation in key.AcceptValidated<DraftAngle>(candidate: style.HatchRotationRadians)
              select (SectionHatch)new SectionHatch.Pattern(address, scale, rotation, display, print);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionAnswer : IDetachedDocumentResult {
    private SectionAnswer() { }
    public sealed record State(SectionSnapshot Snapshot) : SectionAnswer;
    public sealed record Rows(DraftCount ActiveCount) : SectionAnswer;
    public sealed record Minted(ResourceName Name) : SectionAnswer;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Sections {
    public static Fin<Unit> Commit(DocumentSession session, DraftPlan<SectionOp> plan, Op? key = null) =>
        DraftSpine.Commit(session: session, plan: plan,
            apply: static (document, operation, op) => operation.Apply(document: document, op: op),
            op: key.OrDefault());

    public static Fin<SectionAnswer> Ask(DocumentSession session, SectionAsk request, Op? key = null) {
        Op op = key.OrDefault();
        return from admitted in op.AcceptInput(value: request)
               from answer in session.Demand(
                   use: document => admitted.Answer(document: document, op: op), key: op, needs: [SessionNeed.Read])
               select answer;
    }
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]           | [OWNER]           | [FORM]                                                | [ENTRY]             |
| :-----: | :------------------ | :---------------- | :---------------------------------------------------- | :------------------ |
|  [01]   | face axes           | `FaceWeight`      | full host weight, slant, and stretch rosters          | `Host` projection   |
|  [02]   | quartet corners     | `QuartetFace`     | four legal corners with the family availability probe | `Held(quartet)`     |
|  [03]   | decoration roster   | `FaceDecoration`  | font read, run sweep, and run write per row           | `On` / `Across`     |
|  [04]   | face traits         | `FaceTrait`       | installation and specialty-font readings              | `On(font)`          |
|  [05]   | face construction   | `FaceQuery`       | quartet policy or full explicit axes under one law    | `Typefaces.Resolve` |
|  [06]   | machine census      | `FaceCensus`      | installed faces, quartet grid, or face names          | `Typefaces.Census`  |
|  [07]   | document face bind  | `TypefaceOp`      | short-circuiting seat probe plus style-table add      | `Typefaces.Commit`  |
|  [08]   | section schema      | `SectionField`    | one row per proven property over `FieldTable`         | `Read` / `Write`    |
|  [09]   | host defaults       | `SectionDefaults` | one leased snapshot read through the schema rows      | `On(axis)` / `Row`  |
|  [10]   | section composition | `SectionSpec`     | closed fill, boundary, hatch, and rule state          | `Create` / `Apply`  |
|  [11]   | boundary linetype   | `SectionStroke`   | table address or whole embedded definition            | `Attach` / `Detach` |
|  [12]   | section mutations   | `SectionOp`       | shared table verbs, sourced author, import reconcile  | `Sections.Commit`   |
|  [13]   | import reversal     | `ImportLanding`   | one landed row carrying its own undo                  | `Rollback`          |
|  [14]   | section evidence    | `SectionSnapshot` | composed spec plus usage and table state              | `Sections.Ask`      |

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

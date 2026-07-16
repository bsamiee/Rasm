# [RASM_RHINO_ANNOTATION_TYPEFACE]

Typeface and section-presentation rail (`Rasm.Rhino.Annotation`). `FaceQuery` resolves a validated quartet face or a full-axis `Font` constructor into `FaceInfo`, including every catalogued name, style axis, capability flag, installation verdict, and substitute. Installed census remains session-free machine state; document binding rides `RhinoDoc.Fonts.FindOrCreate`, whose answer is a `DimStyles` index because the host font table is a face over the style table. Section authoring seeds `new SectionStyle()`, amendment seeds the copy constructor, and both land the composed `SectionSpec` through the table; `.secstyles` import remaps referenced hatch indices before adding styles. `FontOrigin`/`FontType` remain absent because no public `Font` property exposes them, and `Font` itself is immutable — every property is read-only, so a face never mutates and only construction or resolution mints one.

## [01]-[INDEX]

- [02]-[FACE_MODEL]: `FaceWeight`, `FaceSlant`, `FaceStretch`, `FaceQuery`, and the detached `FaceInfo` projection.
- [03]-[TYPEFACE_ENTRIES]: session-free resolution and census statics plus the `DimStyles`-backed document bind.
- [04]-[SECTION_MODEL]: `SectionFillMode`, `SectionRule`, and the `SectionSpec` composition over sibling resources.
- [05]-[SECTION_RAIL]: `SectionOp`, `SectionTransaction`, usage-censused snapshot, and catalogued table state.
- [06]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[FACE_MODEL]

- Owner: `FaceWeight`/`FaceSlant`/`FaceStretch` `[SmartEnum<int>]` — the three style-axis vocabularies keyed on the explicit `Font.FontWeight`/`Font.FontStyle`/`Font.FontStretch` byte values; `FaceQuery` `[Union]` — quartet resolution versus full-axis construction; `FaceInfo` — the one detached typeface projection.
- Law: resolution answers evidence — `FaceInfo` carries `IsInstalled` beside the resolved names, and an uninstalled face resolves its substitute through `GetSubstituteFont` as a second `FaceInfo` on the answer, so a consumer never renders against a face the host will silently swap.
- Law: the English name twins are exactly three — `EnglishFaceName`, `EnglishFamilyName`, `EnglishQuartetName`; PostScript, logfont, rich-text, and description names have no English form, and a projection claiming one is a phantom.
- Law: quartet availability is the family's own fact — `FontQuartet` answers the four-face grid (`HasRegularFont`/`HasBoldFont`/`HasItalicFont`/`HasBoldItalicFont`), so a bold/italic grant validates against the quartet before resolution rather than after a silent substitution.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class FaceWeight {
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
    public static readonly FaceSlant Upright = new(key: (int)Font.FontStyle.Upright);
    public static readonly FaceSlant Italic = new(key: (int)Font.FontStyle.Italic);
    public static readonly FaceSlant Oblique = new(key: (int)Font.FontStyle.Oblique);

    internal Font.FontStyle Host => (Font.FontStyle)Key;
}

[SmartEnum<int>]
public sealed partial class FaceStretch {
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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FaceQuery {
    private FaceQuery() { }
    public sealed record Quartet(string Name, bool Bold, bool Italic) : FaceQuery;
    public sealed record Axes(
        string Family, FaceWeight Weight, FaceSlant Slant, FaceStretch Stretch,
        bool Underlined = false, bool Strikethrough = false) : FaceQuery;

    internal Fin<Font> Resolve(Op key) =>
        Switch(
            state: key,
            quartet: static (op, query) =>
                from name in op.AcceptText(value: query.Name)
                from family in op.Catch(() => toSeq(Font.InstalledFontsAsQuartets())
                    .Find(row => string.Equals(row.QuartetName, name, StringComparison.OrdinalIgnoreCase))
                    .ToFin(Fail: op.MissingContext()))
                from _ in guard((query.Bold, query.Italic) switch {
                    (false, false) => family.HasRegularFont,
                    (true, false) => family.HasBoldFont,
                    (false, true) => family.HasItalicFont,
                    (true, true) => family.HasBoldItalicFont,
                }, op.MissingContext()).ToFin()
                from font in op.Catch(() => Optional(Font.FromQuartetProperties(
                        quartetName: name, bold: query.Bold, italic: query.Italic))
                    .ToFin(Fail: op.MissingContext()))
                select font,
            axes: static (op, query) =>
                from family in op.AcceptText(value: query.Family)
                from weight in Optional(query.Weight).ToFin(Fail: op.InvalidInput())
                from slant in Optional(query.Slant).ToFin(Fail: op.InvalidInput())
                from stretch in Optional(query.Stretch).ToFin(Fail: op.InvalidInput())
                from font in op.Catch(() => Optional(new Font(
                        familyName: family, weight: weight.Host, style: slant.Host,
                        stretch: stretch.Host, underlined: query.Underlined, strikethrough: query.Strikethrough))
                    .ToFin(Fail: op.MissingContext()))
                select font);
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record FaceInfo(
    string FaceName,
    string FamilyName,
    string FamilyPlusFaceName,
    string QuartetName,
    string PostScriptName,
    string LogfontName,
    string RichTextFontName,
    string Description,
    string EnglishFaceName,
    string EnglishFamilyName,
    string EnglishQuartetName,
    FaceWeight Weight,
    FaceSlant Slant,
    FaceStretch Stretch,
    bool Bold,
    bool Italic,
    bool Underlined,
    bool Strikeout,
    double PointSize,
    bool IsInstalled,
    bool IsSimulated,
    bool IsEngravingFont,
    bool IsSymbolFont,
    bool IsSingleStrokeFont,
    bool IsGeometricToleranceFont) {
    internal static Fin<FaceInfo> Of(Font font, Op key) =>
        Optional(font).ToFin(Fail: key.InvalidInput()).Bind(face => key.Catch(() =>
            from weight in Optional(FaceWeight.TryGet((int)face.Weight, out FaceWeight? w) ? w : null).ToFin(Fail: key.InvalidResult())
            from slant in Optional(FaceSlant.TryGet((int)face.Style, out FaceSlant? s) ? s : null).ToFin(Fail: key.InvalidResult())
            from stretch in Optional(FaceStretch.TryGet((int)face.Stretch, out FaceStretch? st) ? st : null).ToFin(Fail: key.InvalidResult())
            select new FaceInfo(
                FaceName: face.FaceName, FamilyName: face.FamilyName, FamilyPlusFaceName: face.FamilyPlusFaceName,
                QuartetName: face.QuartetName, PostScriptName: face.PostScriptName, LogfontName: face.LogfontName,
                RichTextFontName: face.RichTextFontName, Description: face.Description,
                EnglishFaceName: face.EnglishFaceName, EnglishFamilyName: face.EnglishFamilyName,
                EnglishQuartetName: face.EnglishQuartetName,
                Weight: weight, Slant: slant, Stretch: stretch,
                Bold: face.Bold, Italic: face.Italic, Underlined: face.Underlined, Strikeout: face.Strikeout,
                PointSize: face.PointSize,
                IsInstalled: face.IsInstalled, IsSimulated: face.IsSimulated,
                IsEngravingFont: face.IsEngravingFont, IsSymbolFont: face.IsSymbolFont,
                IsSingleStrokeFont: face.IsSingleStrokeFont, IsGeometricToleranceFont: face.IsGeometricToleranceFont)));
}

public readonly record struct QuartetInfo(string Name, bool HasRegular, bool HasBold, bool HasItalic, bool HasBoldItalic);

public sealed record FaceResolution(FaceInfo Face, Option<FaceInfo> Substitute);
```

## [03]-[TYPEFACE_ENTRIES]

- Owner: `Typefaces` — `Resolve` into `FaceResolution`, `Installed` optionally family-scoped, `Quartets` as the four-face grid, `FaceNames` as the flat roster, and the session-bound `Bind`.
- Law: census is session-free — installed fonts are machine state the host answers without a document, so those entries take no session and mutate nothing.
- Law: document binding is style-table state — `RhinoDoc.Fonts` projects `DimStyles` (its indexer answers `DimStyles[index].Font`), and its one mutator `FindOrCreate(face, bold, italic[, template_style])` answers a `DimStyles` index — so `Bind` runs under the mutation grant, resolves an optional template through `StyleOp.Lens`, and returns the index as a style `ResourceRef`, never a font handle.

```csharp signature
// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Typefaces {
    public static Fin<FaceResolution> Resolve(FaceQuery query) {
        Op op = Op.Of();
        return from active in Optional(query).ToFin(Fail: op.InvalidInput())
               from font in active.Resolve(key: op)
               from face in FaceInfo.Of(font: font, key: op)
               from substitute in face.IsInstalled
                   ? Fin.Succ(Option<FaceInfo>.None)
                   : op.Catch(() => Optional(font.GetSubstituteFont())
                       .Traverse(fallback => FaceInfo.Of(font: fallback, key: op)).As())
               select new FaceResolution(Face: face, Substitute: substitute);
    }

    public static Fin<Seq<FaceInfo>> Installed(Option<string> family = default) {
        Op op = Op.Of();
        return family.Traverse(name => op.AcceptText(value: name)).As()
            .Bind(scope => op.Catch(() => Fin.Succ(value: toSeq(scope.Match(
                Some: static name => Font.InstalledFonts(familyName: name),
                None: static () => Font.InstalledFonts())))))
            .Bind(fonts => fonts.TraverseM(font => FaceInfo.Of(font: font, key: op)).As());
    }

    public static Fin<Seq<QuartetInfo>> Quartets() {
        Op op = Op.Of();
        return op.Catch(() => Fin.Succ(value: toSeq(Font.InstalledFontsAsQuartets()).Map(static quartet => new QuartetInfo(
            Name: quartet.QuartetName,
            HasRegular: quartet.HasRegularFont,
            HasBold: quartet.HasBoldFont,
            HasItalic: quartet.HasItalicFont,
            HasBoldItalic: quartet.HasBoldItalicFont))));
    }

    public static Fin<Seq<string>> FaceNames() =>
        Op.Of().Catch(() => Fin.Succ(value: toSeq(Font.AvailableFontFaceNames())));

    public static Fin<ResourceRef> Bind(DocumentSession session, string face, bool bold, bool italic, Option<ResourceRef> template = default) {
        Op op = Op.Of();
        return from name in op.AcceptText(value: face)
               from address in session.Demand(
                   use: document =>
                       from seed in template.Traverse(target => target.Resolve(document: document, lens: StyleOp.Lens, key: op)).As()
                       from index in op.Catch(() => seed.Match(
                           Some: style => document.Fonts.FindOrCreate(face: name, bold: bold, italic: italic, template_style: style),
                           None: () => document.Fonts.FindOrCreate(face: name, bold: bold, italic: italic)) is var found && found >= 0
                           ? Fin.Succ(value: found)
                           : Fin.Fail<int>(error: op.InvalidResult()))
                       from bound in ResourceRef.Of(index: index)
                       select bound,
                   key: op,
                   needs: [SessionNeed.Mutate])
               select address;
    }
}
```

## [04]-[SECTION_MODEL]

- Owner: `SectionFillMode`/`SectionRule` `[SmartEnum<int>]` — the cut-face fill and fill-rule vocabularies keyed on explicit host values; `SectionFill` — background mode with display and print colors; `SectionBoundary` — visibility, stroke colors, width scale, plot weight, and the linetype address; `SectionHatch` — the pattern address with scale, rotation, and colors; `SectionSpec` — the whole presentation composed of the three rows plus the fill rule.
- Law: composition binds by resolution — the boundary linetype and cut-fill pattern enter as `ResourceRef` values resolved against the sibling lenses inside the bracket before any index writes, so a section style can never bind an index its tables do not hold.
- Law: colors quantize at the write — every fill, boundary, and hatch color is a kernel `PerceptualColor` minting its `System.Drawing.Color` inside `Apply`, never a host channel average.
- Law: the seed is a constructor pair — `new SectionStyle()` births authoring and `new SectionStyle(other)` the amendment copy; the host exposes no `Duplicate()` member, and `Apply` stamps the spec's name so an amendment carrying a fresh name is also the rename.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SectionFillMode {
    public static readonly SectionFillMode None = new(key: (int)SectionBackgroundFillMode.None);
    public static readonly SectionFillMode Viewport = new(key: (int)SectionBackgroundFillMode.Viewport);
    public static readonly SectionFillMode SolidColor = new(key: (int)SectionBackgroundFillMode.SolidColor);

    internal SectionBackgroundFillMode Host => (SectionBackgroundFillMode)Key;
}

[SmartEnum<int>]
public sealed partial class SectionRule {
    public static readonly SectionRule ClosedCurves = new(key: (int)ObjectSectionFillRule.ClosedCurves);
    public static readonly SectionRule SolidObjects = new(key: (int)ObjectSectionFillRule.SolidObjects);

    internal ObjectSectionFillRule Host => (ObjectSectionFillRule)Key;
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record SectionFill(SectionFillMode Mode, PerceptualColor Color, PerceptualColor PrintColor);

public sealed record SectionBoundary(
    bool Visible,
    PerceptualColor Color,
    PerceptualColor PrintColor,
    double WidthScale,
    double PlotWeightMillimeters,
    Option<ResourceRef> Linetype = default);

public sealed record SectionHatch(
    Option<ResourceRef> Pattern,
    double Scale,
    double RotationRadians,
    PerceptualColor Color,
    PerceptualColor PrintColor);

public sealed record SectionSpec(string Name, SectionFill Fill, SectionBoundary Boundary, SectionHatch Hatch, SectionRule Rule) {
    public static Fin<SectionSpec> Of(
        string name, SectionFill fill, SectionBoundary boundary, SectionHatch hatch, SectionRule rule, Op? key = null) {
        Op op = key.OrDefault();
        return from label in op.AcceptText(value: name)
               from admittedFill in Optional(fill).ToFin(Fail: op.InvalidInput())
               from admittedBoundary in Optional(boundary).ToFin(Fail: op.InvalidInput())
               from admittedHatch in Optional(hatch).ToFin(Fail: op.InvalidInput())
               from admittedRule in Optional(rule).ToFin(Fail: op.InvalidInput())
               from _ in op.Positive(value: admittedBoundary.WidthScale)
               from __ in guard(admittedBoundary.PlotWeightMillimeters >= 0.0, op.InvalidInput()).ToFin()
               from ___ in op.Positive(value: admittedHatch.Scale)
               from ____ in op.AcceptInput(value: admittedHatch.RotationRadians)
               select new SectionSpec(
                   Name: label, Fill: admittedFill, Boundary: admittedBoundary,
                   Hatch: admittedHatch, Rule: admittedRule);
    }

    internal Fin<Unit> Apply(SectionStyle style, RhinoDoc document, Op key) {
        SectionSpec self = this;
        return from linetype in self.Boundary.Linetype
                   .Traverse(address => address.Resolve(document: document, lens: LinetypeOp.Lens, key: key)).As()
               from pattern in self.Hatch.Pattern
                   .Traverse(address => address.Resolve(document: document, lens: HatchSpec.Lens, key: key)).As()
               from _ in key.Catch(() => {
                   style.Name = self.Name;
                   style.BackgroundFillMode = self.Fill.Mode.Host;
                   style.BackgroundFillColor = Sys(self.Fill.Color);
                   style.BackgroundFillPrintColor = Sys(self.Fill.PrintColor);
                   style.BoundaryVisible = self.Boundary.Visible;
                   style.BoundaryColor = Sys(self.Boundary.Color);
                   style.BoundaryPrintColor = Sys(self.Boundary.PrintColor);
                   style.BoundaryWidthScale = self.Boundary.WidthScale;
                   style.BoundaryPlotWeightMillimeters = self.Boundary.PlotWeightMillimeters;
                   _ = linetype.Match(
                       Some: resolved => fun(() => style.SetBoundaryLinetype(linetype: resolved))(),
                       None: () => fun(style.RemoveBoundaryLinetype)());
                   style.HatchIndex = pattern.Map(static resolved => resolved.Index).IfNone(noneValue: -1);
                   style.HatchScale = self.Hatch.Scale;
                   style.HatchRotationRadians = self.Hatch.RotationRadians;
                   style.HatchPatternColor = Sys(self.Hatch.Color);
                   style.HatchPatternPrintColor = Sys(self.Hatch.PrintColor);
                   style.SectionFillRule = self.Rule.Host;
                   return Fin.Succ(value: unit);
               })
               select unit;
    }

    private static System.Drawing.Color Sys(PerceptualColor color) {
        (byte red, byte green, byte blue, double alpha) = color.ToRgb();
        return System.Drawing.Color.FromArgb((byte)Math.Round(alpha * 255.0), red, green, blue);
    }
}
```

## [05]-[SECTION_RAIL]

- Owner: `SectionOp` `[Union]` — spec authoring and amendment, usage-gated deletion, and coupled `.secstyles` import landing styles beside remapped hatch patterns; `SectionTransaction` — the commit plan; `SectionSnapshot` — one-pass read with three-way usage census; `Sections` — the `Commit`/`Ask` entry pair.
- Law: `Author` refuses an existing name and lands a fresh seed through `Add`; `Amend` seeds the copy constructor from the resolved style and lands through `Modify` by index, so the live component never mutates in the table.
- Law: delete reads the census first — `InUse(index, out definitions, out objects, out layers)` answers who binds the style, the counts ride the snapshot, and a delete against a used style is the host's refusal surfaced typed, never a silent orphaning.
- Law: import is one bracket, two tables — `SectionStyle.ReadFromFile` answers styles and referenced patterns together; patterns land first, source hatch indices remap to target indices, then styles enter `SectionStyleTable`.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionOp {
    private SectionOp() { }
    public sealed record Author(SectionSpec Spec) : SectionOp;
    public sealed record Amend(ResourceRef Target, SectionSpec Spec, bool Quiet = true) : SectionOp;
    public sealed record Delete(ResourceRef Target, bool Quiet = true) : SectionOp;
    public sealed record Import(string Path) : SectionOp;

    internal static readonly ResourceLens<SectionStyle> Lens = new(
        ById: static (document, id) => document.SectionStyles.Find(id: id, ignoreDeletedSectionStyles: true) is var index && index >= 0
            ? document.SectionStyles.FindIndex(index: index)
            : null,
        ByName: static (document, name) => document.SectionStyles.FindName(name: name),
        ByIndex: static (document, index) => document.SectionStyles.FindIndex(index: index));

    internal Fin<DraftReceipt> Apply(RhinoDoc document, Op op) =>
        Switch(
            (Document: document, Op: op),
            author: static (context, edit) =>
                from _ in guard(context.Document.SectionStyles.FindName(name: edit.Spec.Name) is null, context.Op.InvalidInput()).ToFin()
                from seed in context.Op.Catch(() => Fin.Succ(value: new SectionStyle()))
                from __ in edit.Spec.Apply(style: seed, document: context.Document, key: context.Op)
                from index in context.Op.Catch(() => context.Document.SectionStyles.Add(sectionstyle: seed) is var added && added >= 0
                    ? Fin.Succ(value: added)
                    : Fin.Fail<int>(error: context.Op.InvalidResult()))
                select DraftReceipt.Component(slot: DraftSlot.Authored, index: index),
            amend: static (context, edit) =>
                from style in edit.Target.Resolve(document: context.Document, lens: Lens, key: context.Op)
                from index in context.Op.Catch(() => Fin.Succ(value: context.Document.SectionStyles.Find(
                    id: style.Id, ignoreDeletedSectionStyles: true)))
                from copy in context.Op.Catch(() => Fin.Succ(value: new SectionStyle(style)))
                from _ in edit.Spec.Apply(style: copy, document: context.Document, key: context.Op)
                from __ in context.Op.Confirm(success: context.Document.SectionStyles.Modify(
                    sectionstyle: copy, index: index, quiet: edit.Quiet))
                select DraftReceipt.Component(slot: DraftSlot.Amended, index: index),
            delete: static (context, edit) =>
                from style in edit.Target.Resolve(document: context.Document, lens: Lens, key: context.Op)
                from index in context.Op.Catch(() => Fin.Succ(value: context.Document.SectionStyles.Find(
                    id: style.Id, ignoreDeletedSectionStyles: true)))
                from usage in context.Op.Catch(() => {
                    bool bound = context.Document.SectionStyles.InUse(
                        index: index,
                        instanceDefinitionCount: out int definitions,
                        objectCount: out int objects,
                        layerCount: out int layers);
                    return Fin.Succ(value: (
                        HostBound: bound,
                        Counts: new SectionUsage(Definitions: definitions, Objects: objects, Layers: layers)));
                })
                from _ in guard(!usage.HostBound && !usage.Counts.Bound, context.Op.InvalidInput()).ToFin()
                from __ in context.Op.Confirm(success: context.Document.SectionStyles.Delete(index: index, quiet: edit.Quiet))
                select DraftReceipt.Component(slot: DraftSlot.Deleted, index: index),
            import: static (context, edit) =>
                from path in context.Op.AcceptText(value: edit.Path)
                from read in context.Op.Catch(() => SectionStyle.ReadFromFile(
                        filename: path, sectionStyles: out SectionStyle[] styles, hatchPatterns: out HatchPattern[] patterns)
                    ? Fin.Succ(value: (Styles: toSeq(styles ?? []), Patterns: toSeq(patterns ?? [])))
                    : Fin.Fail<(Seq<SectionStyle>, Seq<HatchPattern>)>(error: context.Op.InvalidResult()))
                from _ in guard(!read.Styles.IsEmpty, context.Op.InvalidResult()).ToFin()
                from patternFacts in read.Patterns.TraverseM(pattern =>
                    Optional(context.Document.HatchPatterns.FindName(name: pattern.Name)).Match(
                        Some: existing => Fin.Succ(value: (Source: pattern.Index, Target: existing.Index)),
                        None: () => context.Op.Catch(() => context.Document.HatchPatterns.Add(pattern: pattern) is var added && added >= 0
                            ? Fin.Succ(value: (Source: pattern.Index, Target: added))
                            : Fin.Fail<(int Source, int Target)>(error: context.Op.InvalidResult())))).As()
                from styleFacts in read.Styles.TraverseM(style =>
                    from hatchIndex in style.HatchIndex >= 0
                        ? patternFacts.Find(pair => pair.Source == style.HatchIndex)
                            .Map(static pair => pair.Target)
                            .ToFin(Fail: context.Op.MissingContext())
                        : Fin.Succ(value: -1)
                    from added in context.Op.Catch(() => {
                        style.HatchIndex = hatchIndex;
                        int index = context.Document.SectionStyles.Add(sectionstyle: style);
                        return index >= 0
                            ? Fin.Succ(value: index)
                            : Fin.Fail<int>(error: context.Op.InvalidResult());
                    })
                    select added).As()
                select styleFacts.Fold(
                    patternFacts.Fold(
                        DraftReceipt.Path(slot: DraftSlot.Imported, path: path),
                        static (state, pair) => state + DraftReceipt.Component(slot: DraftSlot.Imported, index: pair.Target)),
                    static (state, index) => state + DraftReceipt.Component(slot: DraftSlot.Authored, index: index)));
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record SectionTransaction(string Name, Seq<SectionOp> Operations, RedrawPolicy Redraw, bool UndoRecorded = true) {
    public static SectionTransaction Batch(string name, params ReadOnlySpan<SectionOp> operations) =>
        new(Name: name, Operations: toSeq(operations.ToArray()), Redraw: RedrawPolicy.Deferred);
}

public readonly record struct SectionUsage(int Definitions, int Objects, int Layers) {
    public bool Bound => Definitions > 0 || Objects > 0 || Layers > 0;
}

public sealed record SectionSnapshot(
    Guid Key,
    string Name,
    SectionFillMode FillMode,
    PerceptualColor FillColor,
    PerceptualColor FillPrintColor,
    bool BoundaryVisible,
    PerceptualColor BoundaryColor,
    PerceptualColor BoundaryPrintColor,
    double BoundaryWidthScale,
    double BoundaryPlotWeightMillimeters,
    Option<ResourceRef> BoundaryLinetype,
    Option<ResourceRef> Hatch,
    double HatchScale,
    double HatchRotationRadians,
    PerceptualColor HatchColor,
    PerceptualColor HatchPrintColor,
    SectionRule Rule,
    bool InUse,
    bool IsUnset,
    SectionUsage Usage) : IDetachedDocumentResult {
    internal static Fin<SectionSnapshot> Of(SectionStyle style, SectionUsage usage, Op key) =>
        from fillMode in Optional(SectionFillMode.TryGet((int)style.BackgroundFillMode, out SectionFillMode? fill) ? fill : null)
            .ToFin(Fail: key.InvalidResult())
        from rule in Optional(SectionRule.TryGet((int)style.SectionFillRule, out SectionRule? sectionRule) ? sectionRule : null)
            .ToFin(Fail: key.InvalidResult())
        from fillColor in Color(style.BackgroundFillColor, key)
        from fillPrintColor in Color(style.BackgroundFillPrintColor, key)
        from boundaryColor in Color(style.BoundaryColor, key)
        from boundaryPrintColor in Color(style.BoundaryPrintColor, key)
        from hatchColor in Color(style.HatchPatternColor, key)
        from hatchPrintColor in Color(style.HatchPatternPrintColor, key)
        from boundaryLinetype in Optional(style.GetBoundaryLinetype())
            .Traverse(linetype => ResourceRef.Of(id: linetype.Id)).As()
        from hatch in style.HatchIndex >= 0
            ? ResourceRef.Of(index: style.HatchIndex).Map(static address => Some(address))
            : Fin.Succ(Option<ResourceRef>.None)
        from snapshot in key.Catch(() => Fin.Succ(value: new SectionSnapshot(
            Key: style.Id,
            Name: style.Name,
            FillMode: fillMode,
            FillColor: fillColor,
            FillPrintColor: fillPrintColor,
            BoundaryVisible: style.BoundaryVisible,
            BoundaryColor: boundaryColor,
            BoundaryPrintColor: boundaryPrintColor,
            BoundaryWidthScale: style.BoundaryWidthScale,
            BoundaryPlotWeightMillimeters: style.BoundaryPlotWeightMillimeters,
            BoundaryLinetype: boundaryLinetype,
            Hatch: hatch,
            HatchScale: style.HatchScale,
            HatchRotationRadians: style.HatchRotationRadians,
            HatchColor: hatchColor,
            HatchPrintColor: hatchPrintColor,
            Rule: rule,
            InUse: style.InUse,
            IsUnset: style.IsUnset,
            Usage: usage)))
        select snapshot;

    private static Fin<PerceptualColor> Color(System.Drawing.Color value, Op key) =>
        PerceptualColor.OfRgb(
            red: value.R,
            green: value.G,
            blue: value.B,
            alpha: value.A / 255.0,
            key: key);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionAsk {
    private SectionAsk() { }
    public sealed record State(ResourceRef Target) : SectionAsk;
    public sealed record TableState : SectionAsk;
    public sealed record MintName : SectionAsk;

    internal Fin<SectionAnswer> Answer(RhinoDoc document, Op op) =>
        Switch(
            context: (Document: document, Op: op),
            state: static (ctx, ask) =>
                from style in ask.Target.Resolve(document: ctx.Document, lens: SectionOp.Lens, key: ctx.Op)
                from index in ctx.Op.Catch(() => Fin.Succ(value: ctx.Document.SectionStyles.Find(
                    id: style.Id, ignoreDeletedSectionStyles: true)))
                from usage in ctx.Op.Catch(() => {
                    bool bound = ctx.Document.SectionStyles.InUse(
                        index: index, instanceDefinitionCount: out int definitions, objectCount: out int objects, layerCount: out int layers);
                    return Fin.Succ(value: (Bound: bound, Usage: new SectionUsage(Definitions: definitions, Objects: objects, Layers: layers)));
                })
                from snapshot in SectionSnapshot.Of(style: style, usage: usage.Usage, key: ctx.Op)
                select (SectionAnswer)new SectionAnswer.State(Snapshot: snapshot),
            tableState: static (ctx, _) => ctx.Op.Catch(() => Fin.Succ<SectionAnswer>(value: new SectionAnswer.Rows(
                ActiveCount: ctx.Document.SectionStyles.ActiveCount))),
            mintName: static (ctx, _) =>
                from minted in ctx.Op.AcceptText(value: ctx.Document.SectionStyles.GetUnusedSectionStyleName())
                select (SectionAnswer)new SectionAnswer.Minted(Name: minted));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionAnswer : IDetachedDocumentResult {
    private SectionAnswer() { }
    public sealed record State(SectionSnapshot Snapshot) : SectionAnswer;
    public sealed record Rows(int ActiveCount) : SectionAnswer;
    public sealed record Minted(string Name) : SectionAnswer;
}

public static class Sections {
    public static Fin<DraftReceipt> Commit(DocumentSession session, SectionTransaction plan) {
        Op op = Op.Of();
        return from active in Optional(plan).ToFin(Fail: op.InvalidInput())
               from _ in guard(!active.Operations.IsEmpty, op.InvalidInput()).ToFin()
               from receipt in DraftSpine.Commit(
                   session: session, name: active.Name, redraw: active.Redraw, recording: active.UndoRecorded,
                   run: document => active.Operations
                       .TraverseM(operation => operation.Apply(document: document, op: op)).As()
                       .Map(static receipts => receipts.Fold(DraftReceipt.Empty, static (state, value) => state + value)),
                   op: op)
               select receipt;
    }

    public static Fin<SectionAnswer> Ask(DocumentSession session, SectionAsk request) {
        Op op = Op.Of();
        return from active in Optional(request).ToFin(Fail: op.InvalidInput())
               from answer in session.Demand(
                   use: document => active.Answer(document: document, op: op),
                   key: op,
                   needs: [SessionNeed.Read])
               select answer;
    }
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]            | [OWNER]       | [FORM]                                                   | [ENTRY]                                |
| :-----: | :------------------- | :------------ | :------------------------------------------------------- | :------------------------------------- |
|  [01]   | face resolution      | `FaceQuery`   | quartet/axes union over explicit-value axis vocabularies | `Typefaces.Resolve`                    |
|  [02]   | face projection      | `FaceInfo`    | every name face + axes + capability flags, detached      | `FaceResolution`                       |
|  [03]   | installed census     | `Typefaces`   | session-free statics over the host font census           | `Installed` / `Quartets` / `FaceNames` |
|  [04]   | font-table bind      | `Typefaces`   | `RhinoDoc.Fonts.FindOrCreate` answering a style address  | `Bind`                                 |
|  [05]   | section presentation | `SectionSpec` | fill + boundary + hatch rows binding sibling resources   | `SectionOp.Author` / `Amend`           |
|  [06]   | section mutations    | `SectionOp`   | author/amend + usage-gated delete + remapped `.secstyles` import | `Sections.Commit`              |
|  [07]   | section reads        | `SectionAsk`  | usage-censused snapshot, table state, name mint          | `Sections.Ask`                         |

# [APPUI_THEME_SEMI]

Rasm.AppUi binds the shipped `Semi.Avalonia` design-token vocabulary to the generated catalogue through one correspondence: `SemiSlot` rows map every claimed shipped key onto a generated rung, `SemiExclusion` rows carve what is deliberately not claimed WITH the reason, and the conformance rail proves both against a roster WALKED from the live theme graph — so a claimed key the shipped theme never defines, a shipped key nobody claimed or carved, and an authored row that cannot mint are three typed refusals rather than silent dead dictionary entries. `Theme/tokens.md` owns the generation this page projects; `Theme/emission.md` folds `Slots` into the one emitted dictionary.

## [01]-[INDEX]

- [02]-[CORRESPONDENCE]: The `SemiSlot` union, the exclusion verdicts, and the claimed-slot roster.
- [03]-[CONFORMANCE]: The walked shipped roster and the two-halved mint/coverage proof.

## [02]-[CORRESPONDENCE]

- Owner: `SemiSlot` `[Union]` the shipped-key correspondence; `SemiExclusion` `[SmartEnum<string>]` the carve verdicts; `SemiCorrespondence` the claimed-slot roster with its severity and role-state folds.
- Cases: `SemiSlot` = Pigment | Hue | Extent | Shade | Size | Weight | Family — the brush and colour rows are two arms of one axis because the shipped vocabulary is not uniformly twinned: the semantic `SemiColor*` slots are brush-only, the numbered `SemiBackground<N>Color` slots are colour-only, and the hue scale is twinned, so minting a brush under a colour-only key type-checks here and fails at every template binding, exactly as the inverse does; a Size or Weight slot names a (role, emphasis) CELL of the generated type table.
- Law: a `SemiSlot` row exists only where the Semi key names a ROLE the catalogue owns; `SemiSpacing*`, `SemiThickness*`, and `SemiBorderRadiusSpacing*` name their own VALUE on a fixed step scale, so a density-selected metric written under them would make the token lie — they are `SemiExclusion.StepScale` verdicts; Semi's raw `Semi<Hue><N>` scale is NOT a write target because its semantic brushes bind the scale through `{StaticResource}` resolved at parse, so a scale write re-tints nothing.
- Entry: `SemiCorrespondence.Slots` — the whole claimed roster, its role-state, numbered-ramp, and severity families derived by three folds rather than authored per slot.
- Packages: Avalonia, Semi.Avalonia, Dock.Avalonia, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new Semi slot is one `SemiSlot` row or one `SemiExclusion` verdict; a fourth severity-keyed family is one `Severity(prefix, suffix, rung)` call.
- Boundary: `DockSurfaceWorkbenchBrush` and `DockSeparatorBrush` resolve as `DynamicResource` in the Dock skin yet no shipped dictionary defines them, so the correspondence MINTS both and every other `Dock*` key already resolves to a `SemiColor*` slot — the palette override re-tints the whole docking estate with no dock-side edit; the shipped `Banner*` and `NotificationCard*`/`ToastCard*` families re-tint through these slot overrides rather than a parallel control theme, severity landing on the status ladder's LIGHT rung for the fill and its base rung for the rim; the toast card carries NO shadow key at all — `NotificationCardBoxShadows` belongs to the corner card — so the toast tier binds its depth through the plane hosting it and authoring a card-scoped shadow here would write a slot the shipped dictionary never defines.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------

// Semi is one flat resource bag, so a row's product is the boxed resource the dictionary takes — the erasure is
// Avalonia's own last hop, never an interior shape, and the case names the axis it reads so the real type stays
// recoverable. The semantic slots are BRUSHES, so a paint re-emits as a SolidColorBrush.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SemiSlot(string Slot) {
    public sealed record Pigment(PaintRole Role, int Rung, string Slot) : SemiSlot(Slot);
    public sealed record Hue(PaintRole Role, int Rung, string Slot) : SemiSlot(Slot);
    public sealed record Extent(MetricFamily Family, int Step, string Slot) : SemiSlot(Slot);
    public sealed record Shade(DepthTier Tier, string Slot) : SemiSlot(Slot);
    public sealed record Size(TypographyRole Role, TypeEmphasis Emphasis, string Slot) : SemiSlot(Slot);
    public sealed record Weight(TypographyRole Role, TypeEmphasis Emphasis, string Slot) : SemiSlot(Slot);
    public sealed record Family(string Slot) : SemiSlot(Slot);

    public Option<object> Mint(ResolvedTheme resolved) => Switch(
        state: resolved,
        pigment: static (r, p) => r.Paint(p.Role, p.Rung).Map(static color => (object)new SolidColorBrush(color)),
        hue: static (r, h) => r.Paint(h.Role, h.Rung).Map(static color => (object)color),
        extent: static (r, e) => r.Metric(e.Family, e.Step).Map(static value => (object)value),
        shade: static (r, s) => r.Depths.TryGetValue(s.Tier.ShadowKey, out BoxShadows shadows) ? Some((object)shadows) : None,
        size: static (r, s) => r.Type(s.Role, s.Emphasis).Map(static row => (object)row.Size),
        weight: static (r, w) => r.Type(w.Role, w.Emphasis).Map(static row => (object)(FontWeight)row.Weight),
        family: static (_, _) => Some((object)new FontFamily(EmbeddedFace.Variable.Family)));
}

// Exclusions are VERDICTS carrying their reason, never silent absence: the conformance rail matches every
// shipped key the correspondence does not claim against this roster, so an unmatched key is a real gap and a
// deliberate carve is a row nobody can mistake for an oversight.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SemiExclusion {
    public static readonly SemiExclusion HueScale = new("hue-scale",
        static key => key.StartsWith("Semi", StringComparison.Ordinal) && HueScalePattern().IsMatch(key),
        "the semantic brushes bind the scale through StaticResource at parse, so a scale write re-tints nothing");
    public static readonly SemiExclusion StepScale = new("step-scale",
        static key => key.StartsWith("SemiSpacing", StringComparison.Ordinal)
            || key.StartsWith("SemiThickness", StringComparison.Ordinal)
            || key.StartsWith("SemiBorderRadiusSpacing", StringComparison.Ordinal),
        "the key names its own VALUE on a fixed step scale, so a density-selected metric under it would make the token lie");
    public static readonly SemiExclusion AiAccent = new("ai-accent",
        static key => key.StartsWith("SemiColorAI", StringComparison.Ordinal) || key.StartsWith("SemiAI", StringComparison.Ordinal),
        "gradient-valued AI identity slots the catalogue mints no anchor for; they keep their shipped pigment");
    public static readonly SemiExclusion Absolute = new("absolute",
        static key => key is "SemiBlack" or "SemiWhite" or "SemiBlackColor" or "SemiWhiteColor" or "SemiColorBlack" or "SemiColorWhite",
        "fixed white and black are absolute anchors by definition and no ladder rung names them");
    // Glyph carves the shipped icon SET and every control-scoped geometry slot beside it: a severity glyph is a
    // path the asset rail owns, so tinting it is a foreground write and re-authoring its outline here would
    // fork the shipped glyph source.
    public static readonly SemiExclusion Glyph = new("glyph",
        static key => key.StartsWith("SemiIcon", StringComparison.Ordinal) || GeometryPattern().IsMatch(key),
        "path geometries owned by Theme/assets as the shipped glyph source");
    public static readonly SemiExclusion ControlGeometry = new("control-geometry",
        static key => ControlGeometryPattern().IsMatch(key),
        "shipped control-internal padding, margin, and size slots stay on the shipped scale beside the role-named extents");
    public static readonly SemiExclusion PillSentinel = new("pill-sentinel",
        static key => key is "SemiBorderRadiusFull",
        "a pill radius is a sentinel large enough to round any height, never a step on the radius scale");
    public static readonly SemiExclusion ZeroDefault = new("zero-default",
        static key => key is "SemiBorderSpacing" or "SemiBorderThickness",
        "the unsuffixed pair ships zero as the no-border default, and no generated stroke or space step names zero");
    public static readonly SemiExclusion UnpairedRung = new("unpaired-rung",
        static key => key is "SemiFontSizeHeader2" or "SemiFontSizeHeader4" or "SemiFontWeightLight",
        "shipped ladder rungs the type-role ladder mints no role for; they keep their shipped value until a role earns them");

    [UseDelegateFromConstructor]
    public partial bool Matches(string key);

    public string Reason { get; }

    [GeneratedRegex(@"^Semi(Amber|Blue|Cyan|Green|Grey|Indigo|LightBlue|LightGreen|Lime|Orange|Pink|Purple|Red|Teal|Violet|Yellow)[0-9](Color)?$")]
    private static partial Regex HueScalePattern();

    [GeneratedRegex(@"(Padding|Margin|MinWidth|MinHeight|MaxWidth|MaxHeight|Spacing)$")]
    private static partial Regex ControlGeometryPattern();

    [GeneratedRegex(@"(IconGeometry|IconPathData|PathData)$")]
    private static partial Regex GeometryPattern();
}

// --- [TABLES] ---------------------------------------------------------------------------

public static partial class SemiCorrespondence {
    public static readonly Seq<SemiSlot> Slots = [
        // Seven roles over six states plus the four disabled slots: the state rungs ride the accent and status
        // ladders, so a re-seed carries every interaction state of every intent with it.
        .. RoleStates(PaintRole.Accent, "Primary"),
        .. RoleStates(PaintRole.Highlight, "Secondary"),
        .. RoleStates(PaintRole.Link, "Tertiary"),
        .. RoleStates(PaintRole.Success, "Success"),
        .. RoleStates(PaintRole.Warning, "Warning"),
        .. RoleStates(PaintRole.Error, "Danger"),
        .. RoleStates(PaintRole.Info, "Information"),
        new SemiSlot.Pigment(PaintRole.Disabled, 0, "SemiColorPrimaryDisabled"),
        new SemiSlot.Pigment(PaintRole.Disabled, 0, "SemiColorSecondaryDisabled"),
        new SemiSlot.Pigment(PaintRole.Disabled, 0, "SemiColorSuccessDisabled"),
        new SemiSlot.Pigment(PaintRole.Disabled, 0, "SemiColorInformationDisabled"),
        // The numbered surface and fill ramps land on the generated ladders one for one; the surface ramp also
        // ships a colour-only twin set with no brush, so those five slots take the Hue case.
        .. Numbered(PaintRole.Surface, "SemiColorBackground", 5),
        .. Numbered(PaintRole.Raised, "SemiColorFill", 3),
        .. ThemeCatalog.Steps(5).Map(static index => (SemiSlot)new SemiSlot.Hue(PaintRole.Surface, index, $"SemiBackground{index}Color")),
        // The numbered text ramp is FOUR EMPHASIS LEVELS, not four rungs of one role: each level is its own
        // contrast-solved role, so mapping the ramp onto one role's rung index would collapse the whole ladder
        // onto the primary ink while still resolving.
        new SemiSlot.Pigment(PaintRole.Text, 0, "SemiColorText0"),
        new SemiSlot.Pigment(PaintRole.TextMuted, 0, "SemiColorText1"),
        new SemiSlot.Pigment(PaintRole.TextMuted, 1, "SemiColorText2"),
        new SemiSlot.Pigment(PaintRole.TextFaint, 0, "SemiColorText3"),
        new SemiSlot.Pigment(PaintRole.Border, 0, "SemiColorBorder"),
        // The focus TRIO: the ring colour beside the two variant-invariant geometry slots the shipped themes
        // read for focus thickness and offset, so the double-ring recipe binds shipped keys rather than
        // describing a geometry no control theme resolves.
        new SemiSlot.Pigment(PaintRole.Focus, 0, "SemiColorFocusBorder"),
        new SemiSlot.Extent(MetricFamily.Stroke, 1, "SemiBorderThicknessControlFocus"),
        new SemiSlot.Extent(MetricFamily.Space, 0, "SemiBorderSpacingControlFocus"),
        new SemiSlot.Extent(MetricFamily.Stroke, 0, "SemiBorderThicknessControl"),
        new SemiSlot.Extent(MetricFamily.Space, 0, "SemiBorderSpacingControl"),
        new SemiSlot.Pigment(PaintRole.Link, 0, "SemiColorLink"),
        new SemiSlot.Pigment(PaintRole.Link, 1, "SemiColorLinkPointerover"),
        new SemiSlot.Pigment(PaintRole.Link, 2, "SemiColorLinkActive"),
        new SemiSlot.Pigment(PaintRole.Link, 3, "SemiColorLinkVisited"),
        new SemiSlot.Pigment(PaintRole.Highlight, 0, "SemiColorHighlight"),
        new SemiSlot.Pigment(PaintRole.Selection, 0, "SemiColorHighlightBackground"),
        // The global disabled set and the two surface slots that carry the whole scrim and nav vocabulary.
        new SemiSlot.Pigment(PaintRole.Well, 0, "SemiColorDisabledBackground"),
        new SemiSlot.Pigment(PaintRole.Border, 1, "SemiColorDisabledBorder"),
        new SemiSlot.Pigment(PaintRole.Disabled, 1, "SemiColorDisabledFill"),
        new SemiSlot.Pigment(PaintRole.Disabled, 0, "SemiColorDisabledText"),
        new SemiSlot.Pigment(PaintRole.Panel, 0, "SemiColorNavBackground"),
        new SemiSlot.Pigment(PaintRole.Scrim, 0, "SemiColorOverlayBackground"),
        new SemiSlot.Pigment(PaintRole.Scrim, 0, "SemiColorShadow"),
        // Role-named extents: radius, control height, and icon width re-seed; the numbered spacing and
        // thickness ladders are SemiExclusion.StepScale.
        new SemiSlot.Extent(MetricFamily.Radius, 0, "SemiBorderRadiusExtraSmall"),
        new SemiSlot.Extent(MetricFamily.Radius, 1, "SemiBorderRadiusSmall"),
        new SemiSlot.Extent(MetricFamily.Radius, 2, "SemiBorderRadiusMedium"),
        new SemiSlot.Extent(MetricFamily.Radius, 3, "SemiBorderRadiusLarge"),
        new SemiSlot.Extent(MetricFamily.Extent, 0, "SemiHeightControlSmall"),
        new SemiSlot.Extent(MetricFamily.Extent, 1, "SemiHeightControlDefault"),
        new SemiSlot.Extent(MetricFamily.Extent, 2, "SemiHeightControlLarge"),
        new SemiSlot.Extent(MetricFamily.Icon, 0, "SemiWidthIconExtraSmall"),
        new SemiSlot.Extent(MetricFamily.Icon, 1, "SemiWidthIconSmall"),
        new SemiSlot.Extent(MetricFamily.Icon, 2, "SemiWidthIconMedium"),
        new SemiSlot.Extent(MetricFamily.Icon, 3, "SemiWidthIconLarge"),
        new SemiSlot.Extent(MetricFamily.Icon, 4, "SemiWidthIconExtraLarge"),
        // Typography seats: a shipped ladder rung re-seeds from the generated type table, so density and text
        // scale move it; the shipped bold weight is the BODY role at strong emphasis, not a second role.
        new SemiSlot.Size(TypographyRole.Caption, TypeEmphasis.Regular, "SemiFontSizeSmall"),
        new SemiSlot.Size(TypographyRole.Body, TypeEmphasis.Regular, "SemiFontSizeRegular"),
        new SemiSlot.Size(TypographyRole.Section, TypeEmphasis.Regular, "SemiFontSizeHeader6"),
        new SemiSlot.Size(TypographyRole.Title, TypeEmphasis.Regular, "SemiFontSizeHeader5"),
        new SemiSlot.Size(TypographyRole.Headline, TypeEmphasis.Regular, "SemiFontSizeHeader3"),
        new SemiSlot.Size(TypographyRole.Display, TypeEmphasis.Regular, "SemiFontSizeHeader1"),
        new SemiSlot.Weight(TypographyRole.Body, TypeEmphasis.Regular, "SemiFontWeightRegular"),
        new SemiSlot.Weight(TypographyRole.Body, TypeEmphasis.Strong, "SemiFontWeightBold"),
        new SemiSlot.Family("SemiFontFamilyRegular"),
        // Elevation: the one global token plus every shipped control-scoped shadow slot, each mapped to the
        // tier whose stack its surface class earns.
        new SemiSlot.Shade(DepthTier.Raised, "SemiShadowElevated"),
        new SemiSlot.Shade(DepthTier.Card, "BorderCardBoxShadow"),
        new SemiSlot.Shade(DepthTier.Flyout, "FlyoutBorderBoxShadow"),
        new SemiSlot.Shade(DepthTier.Flyout, "MenuFlyoutBorderBoxShadow"),
        new SemiSlot.Shade(DepthTier.Flyout, "ComboBoxPopupBoxShadow"),
        new SemiSlot.Shade(DepthTier.Flyout, "AutoCompleteBoxPopupBoxShadow"),
        new SemiSlot.Shade(DepthTier.Flyout, "CommandBarOverflowBoxShadow"),
        new SemiSlot.Shade(DepthTier.Flyout, "CalendarDatePickerPopupBoxShadows"),
        new SemiSlot.Shade(DepthTier.Flyout, "DateTimePickerFlyoutBoxShadow"),
        new SemiSlot.Shade(DepthTier.Floating, "NotificationCardBoxShadows"),
        new SemiSlot.Shade(DepthTier.Raised, "ToggleSwitchIndicatorBoxShadow"),
        new SemiSlot.Shade(DepthTier.Dialog, "WindowBorderShadow"),
        // Dock chrome: the two DynamicResource keys no shipped dictionary defines — minted here on purpose.
        new SemiSlot.Pigment(PaintRole.Workbench, 0, "DockSurfaceWorkbenchBrush"),
        new SemiSlot.Pigment(PaintRole.Separator, 0, "DockSeparatorBrush"),
        // Notification families re-tint through SLOT OVERRIDES rather than a parallel control theme: severity
        // lands on the status ladder's LIGHT rung for the fill and its base rung for the rim, so four levels
        // read as one family against a neutral surface and the ink carries the level.
        .. Severity("Banner", "Background", rung: 3),
        .. Severity("Banner", "BorderBrush", rung: 1),
        new SemiSlot.Pigment(PaintRole.Border, 0, "BannerBorderBrush"),
        new SemiSlot.Pigment(PaintRole.TextMuted, 0, "BannerCloseButtonForeground"),
        new SemiSlot.Extent(MetricFamily.Radius, 2, "BannerCornerRadius"),
        new SemiSlot.Extent(MetricFamily.Stroke, 0, "BannerBorderThickness"),
        new SemiSlot.Size(TypographyRole.Section, TypeEmphasis.Regular, "BannerTitleFontSize"),
        // The corner card and the toast card share one severity vocabulary and differ in frame alone.
        .. Severity("NotificationCardLight", "Background", rung: 3),
        .. Severity("NotificationCardLight", "BorderBrush", rung: 1),
        .. Severity("NotificationCard", "IconForeground", rung: 0),
        new SemiSlot.Pigment(PaintRole.Overlay, 2, "NotificationCardLightBackground"),
        new SemiSlot.Pigment(PaintRole.Border, 0, "NotificationCardLightBorderBrush"),
        new SemiSlot.Pigment(PaintRole.Overlay, 2, "NotificationCardBackground"),
        new SemiSlot.Extent(MetricFamily.Stroke, 0, "NotificationCardBorderThickness"),
        new SemiSlot.Extent(MetricFamily.Radius, 2, "NotificationCardCornerRadius"),
        new SemiSlot.Extent(MetricFamily.Icon, 1, "NotificationCardIconHeight"),
        new SemiSlot.Extent(MetricFamily.Icon, 1, "NotificationCardIconWidth"),
        new SemiSlot.Size(TypographyRole.Body, TypeEmphasis.Strong, "NotificationCardTitleFontSize"),
        new SemiSlot.Weight(TypographyRole.Body, TypeEmphasis.Strong, "NotificationCardTitleFontWeight"),
        new SemiSlot.Pigment(PaintRole.Text, 0, "NotificationCardTitleForeground"),
        new SemiSlot.Size(TypographyRole.Body, TypeEmphasis.Regular, "NotificationCardMessageFontSize"),
        new SemiSlot.Weight(TypographyRole.Body, TypeEmphasis.Regular, "NotificationCardMessageFontWeight"),
        new SemiSlot.Pigment(PaintRole.TextMuted, 0, "NotificationCardMessageForeground"),
        new SemiSlot.Pigment(PaintRole.Overlay, 2, "ToastCardBackground"),
        new SemiSlot.Extent(MetricFamily.Stroke, 0, "ToastCardBorderThickness"),
        new SemiSlot.Extent(MetricFamily.Radius, 2, "ToastCardCornerRadius"),
        new SemiSlot.Extent(MetricFamily.Icon, 1, "ToastCardIconHeight"),
        new SemiSlot.Extent(MetricFamily.Icon, 1, "ToastCardIconWidth"),
        new SemiSlot.Weight(TypographyRole.Body, TypeEmphasis.Regular, "ToastCardContentFontWeight"),
        new SemiSlot.Pigment(PaintRole.Text, 0, "ToastCardContentForeground"),
    ];

    // Six states over one role ladder: bare, pointerover, active, and the three Light-family rungs the shipped
    // themes select for quiet intent arms.
    static Seq<SemiSlot> RoleStates(PaintRole role, string intent) => [
        new SemiSlot.Pigment(role, 0, $"SemiColor{intent}"),
        new SemiSlot.Pigment(role, 1, $"SemiColor{intent}Pointerover"),
        new SemiSlot.Pigment(role, 2, $"SemiColor{intent}Active"),
        new SemiSlot.Pigment(role, 3, $"SemiColor{intent}Light"),
        new SemiSlot.Pigment(role, 3, $"SemiColor{intent}LightPointerover"),
        new SemiSlot.Pigment(role, 2, $"SemiColor{intent}LightActive"),
    ];

    // The four severity families ride ONE fold over the status ladder, so a shipped banner, notification, and
    // toast key set costs one row apiece; the affix pair is the whole difference between the families.
    static Seq<SemiSlot> Severity(string prefix, string suffix, int rung) =>
        Seq((Role: PaintRole.Info, Level: "Information"), (Role: PaintRole.Success, Level: "Success"),
            (Role: PaintRole.Warning, Level: "Warning"), (Role: PaintRole.Error, Level: "Error"))
            .Map(row => (SemiSlot)new SemiSlot.Pigment(row.Role, rung, $"{prefix}{row.Level}{suffix}"));

    // ONE slot per generated rung: a shipped ramp longer than the role's ladder leaves its tail unclaimed on
    // the conformance rail rather than clamping several slots onto the last rung, which resolves cleanly while
    // flattening the top of the ramp.
    static Seq<SemiSlot> Numbered(PaintRole role, string prefix, int count) =>
        ThemeCatalog.Steps(Math.Min(count, role.Rungs))
            .Map(index => (SemiSlot)new SemiSlot.Pigment(role, index, $"{prefix}{index}"));
}
```

## [03]-[CONFORMANCE]

- Owner: `SemiRosterReading` — the walked shipped roster; `SemiRoster` — the live-graph descent and the two minted Dock keys; the `SemiMints`/`SemiCovered` pair — the boot half and the roster-dependent proof half.
- Law: every exclusion and every claimed slot is proven against a GENERATED roster of the shipped keys, because a key the page claims that the shipped theme never defines writes a dead dictionary entry and re-tints nothing, and that defect is invisible to any check the page's own row list can perform; the roster is derived by WALKING the live theme graph, not by reading metadata — compiled AXAML keeps every key inside a `XamlClosure` body, so an assembly read recovers one opaque resource blob, while instantiating the theme and descending its resource graph recovers the vocabulary whole.
- Entry: `SemiRoster.Walk(Seq<IStyle> chain)` — the descent; `SemiCorrespondence.SemiMints(ResolvedTheme)` — the roster-free boot half proving every authored row mints, run on the mount path; `SemiCorrespondence.SemiCovered(ResolvedTheme, SemiRosterReading)` — the three-banded proof the headless proof lane folds, its refusal an ACCUMULATED `ManyErrors` whose members name unminted, undefined, and unclaimed keys apiece rather than one joined string a reader must re-parse.
- Auto: a key is recorded against the variant it was reached UNDER, so a claim against a variant-scoped key only one partition carries is itself checkable; a `Type`-keyed entry is a control theme and descends as a style rather than landing in the token roster; the walk carries a REFERENCE-KEYED visited set, because `MergedDictionaries` may share one dictionary instance across partitions and an unguarded descent re-walks or cycles on exactly the sharing the merge exists for.
- Packages: Avalonia, Semi.Avalonia, LanguageExt.Core, BCL inbox
- Growth: a package bump re-derives the roster instead of re-transcribing it; a new proof band is one filter beside the three.
- Boundary: the walk needs a live application, so conformance splits — `SemiMints` costs one pass over the correspondence and runs where a generation gap must be a typed fault at boot, while the roster-dependent halves fold in the headless proof lane beside the accessibility sweep; the two minted Dock keys are absent from every shipped partition ON PURPOSE, so the rail admits a claimed key with no shipped definition only when it appears in `SemiRoster.Minted`.

```csharp signature
// --- [MODELS] ---------------------------------------------------------------------------

public sealed record SemiRosterReading(
    FrozenSet<string> Tokens,
    FrozenDictionary<ThemeVariant, FrozenSet<string>> Variants,
    FrozenSet<Type> ControlThemes);

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class SemiRoster {
    // The two Dock chrome keys the skin binds through DynamicResource and no shipped dictionary defines: the
    // correspondence MINTS them, and the conformance rail admits a claimed-but-undefined key only from here.
    public static readonly FrozenSet<string> Minted =
        FrozenSet.Create(StringComparer.Ordinal, "DockSurfaceWorkbenchBrush", "DockSeparatorBrush");

    // `MergedDictionaries` and `ThemeDictionaries` live on the CONCRETE ResourceDictionary, not on the
    // IResourceDictionary the style surfaces hand back, so the descent pattern-matches the concrete type at
    // every hop; walking the interface alone reaches the top-level keys and silently misses every merged and
    // variant-scoped partition, which is the whole palette.
    public static SemiRosterReading Walk(Seq<IStyle> chain) =>
        chain.Fold((Reading: Empty, Seen: new HashSet<IResourceDictionary>(ReferenceEqualityComparer.Instance)),
                static (state, style) => (Style(state.Reading, style, state.Seen), state.Seen))
            .Reading;

    static readonly SemiRosterReading Empty = new(
        FrozenSet<string>.Empty,
        FrozenDictionary<ThemeVariant, FrozenSet<string>>.Empty,
        FrozenSet<Type>.Empty);

    // `StyleBase.Children` is an `IList<IStyle>` and `Styles` a framework collection, so every descent lifts
    // into the carrier before folding — the fold is the carrier's member, not the list's.
    static SemiRosterReading Style(SemiRosterReading reading, IStyle style, HashSet<IResourceDictionary> seen) => style switch {
        Styles styles => toSeq(styles).Fold(Dictionary(reading, styles.Resources, None, seen), (state, child) => Style(state, child, seen)),
        ControlTheme theme => toSeq(theme.Children).Fold(
            Dictionary(reading with { ControlThemes = Add(reading.ControlThemes, Optional(theme.TargetType)) }, theme.Resources, None, seen),
            (state, child) => Style(state, child, seen)),
        StyleBase basis => toSeq(basis.Children).Fold(Dictionary(reading, basis.Resources, None, seen), (state, child) => Style(state, child, seen)),
        _ => reading,
    };

    // A key is recorded against the variant it was reached UNDER. The visited set is reference-keyed: merged
    // partitions may SHARE a dictionary instance, and an unguarded descent re-walks the shared subtree once
    // per sharer — or forever, where a merge chain reaches itself.
    static SemiRosterReading Dictionary(SemiRosterReading reading, IResourceDictionary dictionary, Option<ThemeVariant> variant, HashSet<IResourceDictionary> seen) =>
        dictionary switch {
            ResourceDictionary concrete when seen.Add(concrete) => toSeq(concrete.Keys).Fold(
                    variant.Match(
                        Some: row => reading with { Variants = Partition(reading.Variants, row, concrete.Keys) },
                        None: () => reading),
                    (state, key) => key switch {
                        string token => state with { Tokens = Add(state.Tokens, Some(token)) },
                        Type target => concrete[key] is ControlTheme theme
                            ? Style(state with { ControlThemes = Add(state.ControlThemes, Some(target)) }, theme, seen)
                            : state with { ControlThemes = Add(state.ControlThemes, Some(target)) },
                        _ => state,
                    })
                switch {
                    var seeded => toSeq(concrete.ThemeDictionaries).Fold(
                        toSeq(concrete.MergedDictionaries).Fold(seeded, (state, merged) =>
                            merged is IResourceDictionary nested ? Dictionary(state, nested, variant, seen) : state),
                        (state, entry) => entry.Value is IResourceDictionary scoped ? Dictionary(state, scoped, Some(entry.Key), seen) : state),
                },
            _ => reading,
        };

    static FrozenDictionary<ThemeVariant, FrozenSet<string>> Partition(
        FrozenDictionary<ThemeVariant, FrozenSet<string>> variants, ThemeVariant row, ICollection<object> keys) =>
        toSeq(variants).Filter(entry => entry.Key != row)
            .Add((row, (variants.TryGetValue(row, out FrozenSet<string>? held) ? toSeq(held) : Seq<string>())
                .Concat(toSeq(keys).Choose(static key => key is string token ? Some(token) : None))
                .ToFrozenSet(StringComparer.Ordinal)))
            .ToFrozenDictionary(static entry => entry.Key, static entry => entry.Value);

    static FrozenSet<T> Add<T>(FrozenSet<T> set, Option<T> value) =>
        value.Match(Some: item => toSeq(set).Add(item).ToFrozenSet(set.Comparer), None: () => set);
}

public static partial class SemiCorrespondence {
    // The BOOT half: every authored row mints from the resolve. This needs no roster, so it runs on the mount
    // path where a generation gap must be a typed fault rather than a control silently keeping its shipped
    // pigment.
    public static Fin<Unit> SemiMints(ResolvedTheme resolved) =>
        Slots.Filter(slot => slot.Mint(resolved).IsNone).Map(static slot => slot.Slot) switch {
            { IsEmpty: true } => Fin.Succ(unit),
            var unminted => Fin.Fail<Unit>(new ThemeFault.PaletteRejected(Report("unminted", unminted))),
        };

    // The PROOF half: three independent bands ACCUMULATE, so one lane run reports the unminted rows, the dead
    // claims, and the unclaimed remainder together as typed members of one ManyErrors — a joined string was a
    // report a reader had to re-parse and a rail nothing could Filter.
    public static Fin<Unit> SemiCovered(ResolvedTheme resolved, SemiRosterReading roster) =>
        (Band(SemiMints(resolved).Match(Succ: static _ => Seq<string>(), Fail: static error => Seq(error.Message)), "unminted"),
         Band(Slots.Map(static slot => slot.Slot).Filter(slot => !roster.Tokens.Contains(slot) && !SemiRoster.Minted.Contains(slot)), "undefined"),
         Band(toSeq(roster.Tokens)
             .Filter(key => !Slots.Exists(slot => slot.Slot == key))
             .Filter(key => !toSeq(SemiExclusion.Items).Exists(row => row.Matches(key))), "unclaimed"))
            .Apply(static (_, _, _) => unit)
            .As()
            .ToFin();

    static Validation<Error, Unit> Band(Seq<string> keys, string band) =>
        keys.IsEmpty
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail((Error)new ThemeFault.PaletteRejected(Report(band, keys)));

    static string Report(string band, Seq<string> keys) =>
        keys.IsEmpty ? string.Empty : $"{band}: {string.Join(", ", keys.Take(12))}";
}
```

## [04]-[RESEARCH]

(none)

# [MATERIALS_REINFORCEMENT]

THE FIRST REALIZED CONNECTIONFAMILY and THE REINFORCED-CONCRETE SECTION OWNER. The reinforcement vocabulary — the `RebarGrade` yield-strength axis, the `BarSize` nominal-bar axis, the `RebarSection` cross-section receipt (nominal bar diameter / cross-sectional area / unit weight), and the ACI 318 standard bend/hook geometry carried as a scalar bend-angle/radius/hook-extension tuple — is the realized reinforcing-bar vocabulary one `connection#CONNECTION_OWNER` `ConnectionItem` carries in the `ConnectionFamily.Reinforcement` case, and THIS owner is ALSO the host-neutral reinforced-concrete-section assembler: the `RcSection.Of` boundary composes the `VividOrange.Sections` `ConcreteSection` from a `profile#PROFILE_OWNER` `Profile` concrete outline, a `VividOrange.Materials` `EnConcreteMaterial`/`EnRebarMaterial` EN grade, and the `FaceReinforcementLayer`/`PerimeterReinforcementLayer` rebar-layout engines over the EN-10080 `BarDiameter` catalogue, lifting the `IConcreteSection` the `Profiles/capacity#SECTION_CAPACITY` transformed-section/N-M-M solvers consume. A #5 rebar is a `ConnectionItem` row, never a `Rebar` type: the bar size, the grade, the section receipt, and the bend template are reinforcement-`ConnectionItem` columns, and the `RebarSection` projection feeds the same `connection#CONNECTION_OWNER` `ConnectionItem.Of` admission and the same `ConnectionCatalogue.Build` fold the fastener and hanger families drive — a reinforcing schedule places through the construction layout fold over one `ConnectionItem`, never a per-family schedule owner. The reinforcement vocabulary grows by data — a new bar size is one `BarSize` row keyed to its `BarDiameter` catalogue entry, a new grade one `RebarGrade` row bound to its `EnRebarGrade`, a new designation one `RebarRow` catalogue entry — never a per-bar type. The bend geometry is a scalar bend-angle/radius/hook-extension tuple the host materializes into a curve, NEVER a host curve here (the host-neutral scalar-`Placement` discipline `Construction/layout#ASSEMBLY_FOLD` keeps — the resolved layout is a `Seq<Placement>` of scalar tuples the host boundary materializes). The page composes `connection#CONNECTION_OWNER` for the `ConnectionItem`/`ConnectionId`/`ConnectionSection`/`ConnectionFault` shape, the `Rasm` kernel `PositiveMagnitude` value-object for every length/area/weight column, the `VividOrange.Sections` reinforced-section + reinforcement-layout owner (`ConcreteSection`/`Rebar`/`Link`/`FaceReinforcementLayer`/`PerimeterReinforcementLayer`/`ReinforcementLayoutByCount`/`ReinforcementLayoutBySpacing`/`MinimumReinforcementSpacing`/`BarDiameter`), the `VividOrange.Materials` EN grade DATA (`EnConcreteMaterial`/`EnRebarMaterial`/`EnRebarFactory`) bound to its `VividOrange.Standards` `En1992` citation, the `Appearance/graph#MATERIAL_LIBRARY` `MaterialId` (`metal.iron`/`metal.steel`) appearance column each row carries, and the `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` capacity receipt the connection-design seam reads by `MaterialId`, never re-derived here; the fastener, hanger, and joint families land their own sibling vocabularies on `Connection/fastener#FASTENER_FAMILY`, `Connection/hanger#HANGER_FAMILY`, and `Connection/joint#JOINT_FAMILY`, and the elastic transformed-section and ultimate N-M-M capacity of the RC section land on `Profiles/capacity#SECTION_CAPACITY` over the `IConcreteSection` THIS owner mints.

## [01]-[INDEX]

- [01]-[REINFORCEMENT_FAMILY]: the `RebarGrade` yield-strength axis bound to its `EnRebarGrade`, the `BarSize` nominal-bar axis keyed to the EN-10080 `BarDiameter`, the `RebarBend` ACI 318 bend/hook scalar tuple, the `RebarSection` cross-section receipt, and the `ConnectionCatalogue.BuildRebarRows` ASTM A615/A706 row table.
- [02]-[RC_SECTION]: the `RcSection` reinforced-concrete-section owner — the `VividOrange.Sections` `ConcreteSection` admission boundary over a `Profile` concrete outline + an `EnConcreteMaterial`/`EnRebarMaterial` EN grade + a `RebarLayout` `[Union]` (count/spacing × face/perimeter) over the `BarDiameter` catalogue, lifting the `IConcreteSection` the `Profiles/capacity#SECTION_CAPACITY` solvers consume, with the EN grade admission lowering the `VividOrange.Materials` derivation throws onto the typed `ConnectionFault` rail and the EC2 `MinimumReinforcementSpacing` clear-spacing rule composed.

## [02]-[REINFORCEMENT_FAMILY]

- Owner: the reinforcement vocabulary (`RebarGrade` the ASTM A615/A706 yield-strength axis, `BarSize` the #3..#18 / 10M..55M nominal-bar axis, `RebarStandard` the spec discriminant, `RebarBend` the ACI 318 bend/hook scalar tuple, `RebarSection` the cross-section receipt); `ConnectionCatalogue.BuildRebarRows` the registered-row seed `connection#CONNECTION_OWNER` `ConnectionCatalogue.Build` folds; the `RebarSection.StandardHook` projection emitting the ACI 318 hook geometry the host materializes.
- Cases: grade {A615 Gr40 / Gr60 / Gr75 / Gr80 (carbon-steel, non-weldable above Gr60 without supplemental S1) · A706 Gr60 / Gr80 (low-alloy, weldable) · CSA G30.18 400W / 500W (metric, weldable)} · size {imperial #3..#11, #14, #18 · metric 10M..55M} · hook {ninety / one-thirty-five / one-eighty standard bend angles, the ACI 318 Table 25.3.1/25.3.2 minimum-bend-diameter and hook-extension rule} — a bar is a `ConnectionItem` row over one `RebarGrade` and one `BarSize`, never a bar subtype.
- Entry: `public Fin<RebarBend> StandardHook(RebarHook hook, Op key)` on `RebarSection` — the ACI 318 standard-hook projection resolving the bend angle, the minimum inside bend diameter (a `BarSize`-dependent multiple of the bar diameter), and the straight hook extension into the scalar `RebarBend` tuple the host curve-materializes; `ConnectionCatalogue.BuildRebarRows(context)` folds the A615/A706 `RebarRow` table through `RebarOf` into the registered `ConnectionItem` rows `ConnectionCatalogue.Build` concatenates — one polymorphic catalogue fold, never a `GetBarBySize`/`GetByGrade` family.
- Packages: Rasm (project — `PositiveMagnitude` for the bar-diameter/area/weight/bend-geometry length columns, never an int-backed `Dimension` that truncates a fractional millimeter), VividOrange.Sections (`BarDiameter` the EN-10080 D6..D50 catalogue diameter the metric `BarSize` rows key, `.api/api-vividorange-sections.md`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` for the grade/size/hook axes with the generated total `Switch`, `[KeyMemberEqualityComparer]` for the catalogue key), LanguageExt.Core (`Fin`/`Seq`/`Choose`/`Fold` for the admission rail and the catalogue fold), BCL inbox (`FrozenDictionary`).
- Growth: the reinforcement vocabulary grows by data — a new bar size is one `BarSize` row carrying its nominal diameter/area/weight (the metric rows keyed to a `BarDiameter` catalogue value), a new grade one `RebarGrade` row carrying its minimum yield bound to its `EnRebarGrade`, a new designation one `RebarRow` catalogue entry, a new hook one `RebarHook` row — never a per-bar type, never a per-grade `ConnectionItem` variant. A fastener/hanger/joint family lands its own vocabulary on its own page the way reinforcement carries `RebarGrade`/`BarSize`/`RebarSection`; `anchor` folds as a `FastenerKind` arm on `Connection/fastener#FASTENER_FAMILY`, so the `ConnectionFamily` axis stays CLOSED at four (reinforcement/fastener/hanger/joint, `joint` the deliberate continuous weld/adhesive/stud widening at `Connection/joint#JOINT_FAMILY`), never a fifth sibling family or a per-bar type. The reinforced-concrete SECTION the rebar populates (the `RcSection` `ConcreteSection` assembler) is the `[02]-[RC_SECTION]` owner block on this page, its elastic transformed-section and ultimate N-M-M capacity composed at `Profiles/capacity#SECTION_CAPACITY`.
- Boundary: the reinforcement vocabulary is the first realized `ConnectionFamily` — a per-bar `Rebar` class is the deleted form; `RebarSection` composes the `Rasm` kernel `PositiveMagnitude` (the double-backed `> 0` finite magnitude) for every column so the section never re-mints a length primitive and a fractional ASTM nominal diameter (`#5 = 15.875 mm`, `10M = 11.3 mm`) admits without the truncation an int-backed `Dimension` count would force, the `NominalAreaMm2` and `UnitWeightKgM` columns carried as `PositiveMagnitude` receipts the `IfcReinforcingBar` wire and the structural-design seam read; the ACI 318 bend geometry is the scalar `RebarBend(BendDegrees, InsideBendDiameterMm, HookExtensionMm)` tuple `StandardHook` emits — NEVER a host `Curve`, the host boundary lofts the bar centerline and the bend arc from the scalar tuple exactly as the construction layout materializes a `Placement` tuple, so this owner stays host-neutral; the appearance assignment crosses to `Appearance/graph#MATERIAL_LIBRARY` as the `MaterialId` (`metal.iron` for plain carbon A615, `metal.steel` for low-alloy A706) the row's `ConnectionItem.AppearanceId` column carries, never a reinforcement-specific shade; the mechanical capacity crosses to `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` (`YieldStrengthMpa`/`YoungsModulusMpa`) read by `MaterialId` through the `MaterialPropertySet` key, never re-derived here, so the `RebarGrade.MinimumYieldMpa` axis is the spec-nominal grade band and the measured capacity is the property-library receipt; `ConnectionCatalogue.BuildRebarRows` seeds the `connection#CONNECTION_OWNER` `ConnectionCatalogue.Rows` table with the A615/A706/G30.18 rows keyed `connection.<designation>` (`connection.rebar-no5-gr60`, `connection.rebar-10m-400w`), the dimensional columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` so a malformed row drops through `Choose` rather than seeding a degenerate `ConnectionItem`; the placement of a rebar schedule reads `Construction/layout#ASSEMBLY_FOLD` `StationStep` over the SAME realized fold, never a parallel reinforcement-layout owner.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using VividOrange.Sections.Reinforcement;     // BarDiameter (EN-10080 D6..D50 catalogue diameter)
using VividOrange.Materials.StandardMaterials.En;  // EnRebarGrade, EnRebarFactory, EnRebarMaterial

// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ConnectionKeyPolicy, string>]
public sealed partial class RebarStandard {
    public static readonly RebarStandard A615 = new("astm-a615", weldable: false, authority: "ASTM A615/A615M");
    public static readonly RebarStandard A706 = new("astm-a706", weldable: true, authority: "ASTM A706/A706M");
    public static readonly RebarStandard G30 = new("csa-g30.18", weldable: true, authority: "CSA G30.18");
    public static readonly RebarStandard En10080 = new("en-10080", weldable: true, authority: "EN 1992-1-1 / EN 10080");
    public bool Weldable { get; }
    public string Authority { get; }
}

// EnGrade is the VividOrange.Materials EnRebarGrade an EN-bodied grade maps to (None for the ASTM/CSA bands that
// carry no EN equivalent), so RcSection.Of can lower the matched EN grade to its EnRebarMaterial fck/Es DATA without
// re-keying f_yk; the RebarGrade band stays the spec-nominal yield the connection schedule reads.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ConnectionKeyPolicy, string>]
public sealed partial class RebarGrade {
    public static readonly RebarGrade Gr40  = new("gr40",  minimumYieldMpa: 280.0, standard: RebarStandard.A615,   appearanceId: "metal.iron",  enGrade: None);
    public static readonly RebarGrade Gr60  = new("gr60",  minimumYieldMpa: 420.0, standard: RebarStandard.A615,   appearanceId: "metal.iron",  enGrade: None);
    public static readonly RebarGrade Gr75  = new("gr75",  minimumYieldMpa: 520.0, standard: RebarStandard.A615,   appearanceId: "metal.iron",  enGrade: None);
    public static readonly RebarGrade Gr80  = new("gr80",  minimumYieldMpa: 550.0, standard: RebarStandard.A615,   appearanceId: "metal.iron",  enGrade: None);
    public static readonly RebarGrade Gr60W = new("gr60w", minimumYieldMpa: 420.0, standard: RebarStandard.A706,   appearanceId: "metal.steel", enGrade: None);
    public static readonly RebarGrade Gr80W = new("gr80w", minimumYieldMpa: 550.0, standard: RebarStandard.A706,   appearanceId: "metal.steel", enGrade: None);
    public static readonly RebarGrade G400W = new("400w",  minimumYieldMpa: 400.0, standard: RebarStandard.G30,    appearanceId: "metal.steel", enGrade: None);
    public static readonly RebarGrade G500W = new("500w",  minimumYieldMpa: 500.0, standard: RebarStandard.G30,    appearanceId: "metal.steel", enGrade: None);
    public static readonly RebarGrade B500B = new("b500b", minimumYieldMpa: 500.0, standard: RebarStandard.En10080, appearanceId: "metal.steel", enGrade: Some(EnRebarGrade.B500B));
    public static readonly RebarGrade B500C = new("b500c", minimumYieldMpa: 500.0, standard: RebarStandard.En10080, appearanceId: "metal.steel", enGrade: Some(EnRebarGrade.B500C));
    public double MinimumYieldMpa { get; }
    public RebarStandard Standard { get; }
    public string AppearanceId { get; }
    public Option<EnRebarGrade> EnGrade { get; }   // Some only for the EN-bodied B500B/B500C the RC σ(ε) law reads
    public bool Weldable => Standard.Weldable;
}

// Catalogue is the EN-10080 BarDiameter the metric rows resolve a Rebar diameter from rather than a raw Length —
// None for the ASTM/CSA bands the catalogue does not enumerate, so RcSection.Of feeds a catalogued BarDiameter when
// present and a raw Length(nominalDiameterMm) otherwise; the BarSize band stays the spec-nominal section receipt.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ConnectionKeyPolicy, string>]
public sealed partial class BarSize {
    public static readonly BarSize No3  = new("#3",  nominalDiameterMm: 9.525,  nominalAreaMm2: 71.0,   unitWeightKgM: 0.560,  bendFactor: 6.0,  catalogue: None);
    public static readonly BarSize No4  = new("#4",  nominalDiameterMm: 12.700, nominalAreaMm2: 129.0,  unitWeightKgM: 0.994,  bendFactor: 6.0,  catalogue: None);
    public static readonly BarSize No5  = new("#5",  nominalDiameterMm: 15.875, nominalAreaMm2: 199.0,  unitWeightKgM: 1.552,  bendFactor: 6.0,  catalogue: None);
    public static readonly BarSize No6  = new("#6",  nominalDiameterMm: 19.050, nominalAreaMm2: 284.0,  unitWeightKgM: 2.235,  bendFactor: 6.0,  catalogue: None);
    public static readonly BarSize No7  = new("#7",  nominalDiameterMm: 22.225, nominalAreaMm2: 387.0,  unitWeightKgM: 3.042,  bendFactor: 6.0,  catalogue: None);
    public static readonly BarSize No8  = new("#8",  nominalDiameterMm: 25.400, nominalAreaMm2: 510.0,  unitWeightKgM: 3.973,  bendFactor: 6.0,  catalogue: None);
    public static readonly BarSize No9  = new("#9",  nominalDiameterMm: 28.651, nominalAreaMm2: 645.0,  unitWeightKgM: 5.060,  bendFactor: 8.0,  catalogue: None);
    public static readonly BarSize No10 = new("#10", nominalDiameterMm: 32.258, nominalAreaMm2: 819.0,  unitWeightKgM: 6.404,  bendFactor: 8.0,  catalogue: None);
    public static readonly BarSize No11 = new("#11", nominalDiameterMm: 35.814, nominalAreaMm2: 1006.0, unitWeightKgM: 7.907,  bendFactor: 8.0,  catalogue: None);
    public static readonly BarSize No14 = new("#14", nominalDiameterMm: 43.002, nominalAreaMm2: 1452.0, unitWeightKgM: 11.380, bendFactor: 10.0, catalogue: None);
    public static readonly BarSize No18 = new("#18", nominalDiameterMm: 57.328, nominalAreaMm2: 2581.0, unitWeightKgM: 20.240, bendFactor: 10.0, catalogue: None);
    public static readonly BarSize M10  = new("10M", nominalDiameterMm: 11.300, nominalAreaMm2: 100.0,  unitWeightKgM: 0.785,  bendFactor: 6.0,  catalogue: Some(BarDiameter.D12));
    public static readonly BarSize M15  = new("15M", nominalDiameterMm: 16.000, nominalAreaMm2: 200.0,  unitWeightKgM: 1.570,  bendFactor: 6.0,  catalogue: Some(BarDiameter.D16));
    public static readonly BarSize M20  = new("20M", nominalDiameterMm: 19.500, nominalAreaMm2: 300.0,  unitWeightKgM: 2.355,  bendFactor: 6.0,  catalogue: Some(BarDiameter.D20));
    public static readonly BarSize M25  = new("25M", nominalDiameterMm: 25.200, nominalAreaMm2: 500.0,  unitWeightKgM: 3.925,  bendFactor: 6.0,  catalogue: Some(BarDiameter.D25));
    public static readonly BarSize M30  = new("30M", nominalDiameterMm: 29.900, nominalAreaMm2: 700.0,  unitWeightKgM: 5.495,  bendFactor: 8.0,  catalogue: Some(BarDiameter.D32));
    public static readonly BarSize M35  = new("35M", nominalDiameterMm: 35.700, nominalAreaMm2: 1000.0, unitWeightKgM: 7.850,  bendFactor: 8.0,  catalogue: Some(BarDiameter.D40));
    public static readonly BarSize M45  = new("45M", nominalDiameterMm: 43.700, nominalAreaMm2: 1500.0, unitWeightKgM: 11.775, bendFactor: 10.0, catalogue: Some(BarDiameter.D50));
    public static readonly BarSize M55  = new("55M", nominalDiameterMm: 56.400, nominalAreaMm2: 2500.0, unitWeightKgM: 19.625, bendFactor: 10.0, catalogue: None);
    public double NominalDiameterMm { get; }
    public double NominalAreaMm2 { get; }
    public double UnitWeightKgM { get; }
    public double BendFactor { get; }
    public Option<BarDiameter> Catalogue { get; }   // Some -> RcSection feeds the catalogued BarDiameter to a Rebar
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ConnectionKeyPolicy, string>]
public sealed partial class RebarHook {
    public static readonly RebarHook Ninety       = new("90", bendDegrees: 90.0,  extensionFactor: 12.0);
    public static readonly RebarHook OneThirtyFive = new("135", bendDegrees: 135.0, extensionFactor: 6.0);
    public static readonly RebarHook OneEighty     = new("180", bendDegrees: 180.0, extensionFactor: 4.0);
    public double BendDegrees { get; }
    public double ExtensionFactor { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct RebarBend(double BendDegrees, double InsideBendDiameterMm, double HookExtensionMm);

public readonly record struct RebarSection(
    BarSize Size,
    RebarGrade Grade,
    PositiveMagnitude DiameterMm,
    PositiveMagnitude NominalAreaMm2,
    PositiveMagnitude UnitWeightKgM) {

    public MaterialId AppearanceId => MaterialId.Of(Grade.AppearanceId);
    public double YieldForceKn => Grade.MinimumYieldMpa * NominalAreaMm2.Value * 1e-3;

    public Fin<RebarBend> StandardHook(RebarHook hook, Op key) =>
        from valid in guard(hook.ExtensionFactor > 0.0 && Size.BendFactor > 0.0, ConnectionFault.Capacity(key, $"<degenerate-hook:{Grade.Key}:{Size.Key}>"))
        let diameter = DiameterMm.Value
        let insideBendDiameterMm = Size.BendFactor * diameter
        let hookExtensionMm = hook.ExtensionFactor * diameter
        select new RebarBend(hook.BendDegrees, insideBendDiameterMm, hookExtensionMm);
}

public readonly record struct RebarRow(string Designation, string Size, string Grade);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ConnectionCatalogue {
    static readonly Seq<RebarRow> RebarRows = Seq(
        new RebarRow("connection.rebar-no3-gr60",   "#3",  "gr60"),
        new RebarRow("connection.rebar-no4-gr60",   "#4",  "gr60"),
        new RebarRow("connection.rebar-no5-gr60",   "#5",  "gr60"),
        new RebarRow("connection.rebar-no6-gr60",   "#6",  "gr60"),
        new RebarRow("connection.rebar-no7-gr75",   "#7",  "gr75"),
        new RebarRow("connection.rebar-no8-gr75",   "#8",  "gr75"),
        new RebarRow("connection.rebar-no9-gr80",   "#9",  "gr80"),
        new RebarRow("connection.rebar-no11-gr80",  "#11", "gr80"),
        new RebarRow("connection.rebar-no5-gr60w",  "#5",  "gr60w"),
        new RebarRow("connection.rebar-no8-gr80w",  "#8",  "gr80w"),
        new RebarRow("connection.rebar-10m-400w", "10M", "400w"),
        new RebarRow("connection.rebar-15m-400w", "15M", "400w"),
        new RebarRow("connection.rebar-25m-500w", "25M", "500w"),
        new RebarRow("connection.rebar-35m-500w", "35M", "500w"));

    static Fin<(ConnectionId Id, ConnectionItem Item)> RebarOf(RebarRow r, Context context, Op key) =>
        from size in BarSize.TryGet(r.Size, out BarSize? s) ? Fin.Succ(s!) : Fin.Fail<BarSize>(ConnectionFault.Designation(key, $"<unknown-bar-size:{r.Size}>"))
        from grade in RebarGrade.TryGet(r.Grade, out RebarGrade? g) ? Fin.Succ(g!) : Fin.Fail<RebarGrade>(ConnectionFault.Grade(key, $"<unknown-grade:{r.Grade}>"))
        from diameter in key.AcceptValidated<PositiveMagnitude>(candidate: size.NominalDiameterMm)
        from area in key.AcceptValidated<PositiveMagnitude>(candidate: size.NominalAreaMm2)
        from weight in key.AcceptValidated<PositiveMagnitude>(candidate: size.UnitWeightKgM)
        let section = new RebarSection(size, grade, diameter, area, weight)
        from item in ConnectionItem.Of(ConnectionFamily.Reinforcement, r.Designation, new ConnectionSection.Reinforcement(section), section.AppearanceId, section.AppearanceId, key)
        select (ConnectionId.Of(r.Designation), item);

    public static FrozenDictionary<ConnectionId, ConnectionItem> BuildRebarRows(Context context) =>
        RebarRows
            .Choose(row => RebarOf(row, context, default).ToOption())
            .ToFrozenDictionary(static r => r.Id, static r => r.Item, ConnectionKeyPolicy.EqualityComparer);
}
```

## [03]-[RC_SECTION]

- Owner: `RcSection` the host-neutral reinforced-concrete-section assembler over the `VividOrange.Sections` `ConcreteSection`; `RebarLayout` `[Union]` the closed rebar-arrangement axis (`FaceCount`/`FaceSpacing`/`PerimeterCount`/`PerimeterSpacing`) collapsing the four `VividOrange.Sections` layout-engine constructors; `EnGrade` the static EN-grade admission boundary lowering `VividOrange.Materials` `EnConcreteMaterial`/`EnRebarMaterial` derivation throws onto the typed `ConnectionFault` rail; `RcSection.Of` the one builder lifting the `IConcreteSection` the `Profiles/capacity#SECTION_CAPACITY` solvers consume.
- Cases: layout {`FaceCount` (n bars on a named `SectionFace` — `ReinforcementLayoutByCount` + `FaceReinforcementLayer`), `FaceSpacing` (max-spacing bars on a face — `ReinforcementLayoutBySpacing` + `FaceReinforcementLayer`), `PerimeterCount` (n bars round the whole section — `ReinforcementLayoutByCount` + `PerimeterReinforcementLayer`, no face), `PerimeterSpacing` (max-spacing perimeter bars — `ReinforcementLayoutBySpacing` + `PerimeterReinforcementLayer`, no face)} — the face cases over the `VividOrange.Sections` `SectionFace` floor enum (`Top`/`Left`/`Right`/`Bottom`/`Sides`, NO `Perimeter` member — perimeter distribution is the separate `PerimeterReinforcementLayer` engine, never a `SectionFace` value), the perimeter cases carrying no face; a rebar arrangement is a `RebarLayout` case, never a per-engine constructor scattered at the call site; a stirrup is the `Link` admitted once with its `MinimumMandrelDiameter`, the EC2 clear spacing the one `MinimumReinforcementSpacing` rule.
- Entry: `public static Fin<RcSection> Of(Profile concrete, EnConcreteGrade concreteGrade, RebarGrade barGrade, BarSize linkSize, Seq<RebarLayout> layout, double coverMm, NationalAnnex annex, Op key)` — the ONE reinforced-section boundary: it lowers the `concreteGrade` through `EnGrade.Concrete(concreteGrade, annex, key)` to an `EnConcreteMaterial`, the `barGrade.EnGrade` through `EnGrade.Rebar(...)` to an `EnRebarMaterial` (a non-EN `barGrade.EnGrade == None` railing `ConnectionFault.Grade`), builds the `ConcreteSection` from the `Profile`'s `IProfile` perimeter (the `profile#PROFILE_OWNER` `ParametricSection`/`SectionReader` `IProfile` the section solver already consumes) + the concrete `IMaterial` + a `Link` (the `linkSize.Catalogue` `BarDiameter` over the rebar `IMaterial`) + the `coverMm` `Length`, folds each `RebarLayout` case to its `FaceReinforcementLayer`/`PerimeterReinforcementLayer` and `AddRebarLayer`s it, and reads back `section.Rebars` — the `IConcreteSection` the elastic transformed-section (`ConcreteSectionProperties`) and ultimate N-M-M (`InteractionDiagram`) solvers at `Profiles/capacity#SECTION_CAPACITY` consume; `public Fin<double> MinimumBarSpacingMm(NationalAnnex annex, BarSize size, double maxAggregateMm, Op key)` reads the EC2 `MinimumReinforcementSpacing.GetMinimumReinforcementSpacing` clear-spacing rule for the annex, one polymorphic boundary, never a `BuildRcByCount`/`BuildRcBySpacing` family.
- Packages: VividOrange.Sections (`ConcreteSection`/`Section`, `Rebar`/`Link`/`LongitudinalReinforcement`, `FaceReinforcementLayer`/`PerimeterReinforcementLayer`, `ReinforcementLayoutByCount`/`ReinforcementLayoutBySpacing`, `MinimumReinforcementSpacing`, `SectionFace`, `BarDiameter`; the `InvalidMaterialTypeException`/`InvalidProfileTypeException` boundary throws trapped here; `.api/api-vividorange-sections.md`), VividOrange.Materials (`EnConcreteMaterial`/`EnRebarMaterial` grade DATA, `EnConcreteFactory`/`EnRebarFactory`; the `ArgumentException`/`MissingNationalAnnexException` derivation throws trapped here; `.api/api-vividorange-materials.md`), VividOrange.Standards (`En1992`/`NationalAnnex` the grade cites; `.api/api-vividorange-standards.md`), VividOrange.Profiles + Profiles.Perimeter + Geometry (the `IProfile`/`IPerimeter`/`ILocalPoint2d` the `ConcreteSection` consumes, via the `profile#PROFILE_OWNER` `ParametricSection`), UnitsNet (`Length` cover/diameter at the edge), Rasm (project — `PositiveMagnitude`), LanguageExt.Core (`Fin`/`Seq`/`Fold`), Thinktecture.Runtime.Extensions (`[Union]` for `RebarLayout`, `[SmartEnum]` for the `SectionFace` band).
- Growth: a new rebar arrangement is one `RebarLayout` `[Union]` case binding its `VividOrange.Sections` layout engine, a new section face one `SectionFace` band row, a new constitutive concrete law a `Profiles/capacity#SECTION_CAPACITY` concern reading the same `EnConcreteMaterial` — never a per-arrangement RC builder, never a hand-keyed `f_yk`/`f_ck` where the EN grade carries it, never a raw-`Length` bar diameter where the `BarDiameter` catalogue enumerates it; the EN grade DATA admission is the ONE boundary, the capacity computation the `Profiles/capacity` owner over the `IConcreteSection` minted here.
- Boundary: `RcSection.Of` is the BOUNDARY_ADMISSION point where the `VividOrange.Sections`/`Materials` exception-throwing surface is admitted EXACTLY ONCE — the `EnConcreteMaterial`/`EnRebarMaterial` ctors throw `ArgumentException` (unknown grade) / `MissingNationalAnnexException` (untabulated annex) and the `ConcreteSection`/layout construction throws `InvalidMaterialTypeException`/`InvalidProfileTypeException`, all trapped at THIS boundary and lowered onto the typed `ConnectionFault.Grade`/`ConnectionFault.Capacity` rail (the `.api` `[BOUNDARY_EXCEPTION_LAW]`), so no `VividOrange` throw and no `null` reaches an interior signature and the `IConcreteSection` egress carries only validated DATA; the `barGrade` binds to its `EnRebarGrade` through `RebarGrade.EnGrade` so a `Rebar` carries a registered EN grade rather than a hand-keyed `f_yk` (the spec-nominal `MinimumYieldMpa` band stays the connection-schedule receipt, the EN grade DATA the RC-capacity strength), and a metric `BarSize.Catalogue` resolves a `Rebar(IMaterial, BarDiameter)` over the EN-10080 catalogue rather than a raw `Length`; the four `VividOrange.Sections` layout-engine constructors (`FaceReinforcementLayer`/`PerimeterReinforcementLayer` × count/spacing) collapse into the one `RebarLayout` `[Union]` the `Of` fold dispatches — the 4+-parallel-constructor collapse trigger — so a bar arrangement is a `RebarLayout` case never a scattered `new FaceReinforcementLayer(...)`; the EC2 clear spacing reads the `MinimumReinforcementSpacing(annex)` rule, never an inline EC2 constant; the assembled `IConcreteSection` is host-neutral DATA (the `VividOrange.Geometry` `ILocalPoint2d` bar positions, never a `Rhino.Geometry` type) the `Profiles/capacity#SECTION_CAPACITY` elastic transformed-section + ultimate N-M-M solvers consume and the `VividOrange.Serialization` round-trip persists inside the C# layer (`.api/api-vividorange-serialization.md`, the C#-internal cache wire, distinct from the canonical Thinktecture wire); the RC section is NOT a `ConnectionItem` — a `ConnectionItem` is one discrete rebar in the schedule, the `RcSection` the populated concrete member the rebar reinforces, the two meeting at the `BarSize`/`RebarGrade` vocabulary this page already owns.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using VividOrange.Sections;                          // ConcreteSection, Section, SectionFace, IConcreteSection
using VividOrange.Sections.Reinforcement;            // Rebar, Link, FaceReinforcementLayer, PerimeterReinforcementLayer, MinimumReinforcementSpacing, BarDiameter, IReinforcementLayer
using VividOrange.Materials.StandardMaterials.En;    // EnConcreteMaterial, EnRebarMaterial, EnConcreteGrade
using VividOrange.Profiles;                          // IProfile (the concrete-outline perimeter)
using VividOrange.Standards.Eurocode;                // NationalAnnex citation context (En1992 the grade carries)
using UnitsNet;                                      // Length (cover / diameter at the edge)

// --- [MODELS] ------------------------------------------------------------------------------
// One RebarLayout [Union] collapses the four VividOrange.Sections layout-engine constructors — face/perimeter ×
// count/spacing — each case carrying its own layout-strategy arguments, never four scattered `new ...Layer(...)` sites.
[Union]
public abstract partial record RebarLayout {
    private RebarLayout() { }
    public sealed record FaceCount(SectionFace Face, BarSize Bar, int Count) : RebarLayout;
    public sealed record FaceSpacing(SectionFace Face, BarSize Bar, double MaxSpacingMm) : RebarLayout;
    public sealed record PerimeterCount(BarSize Bar, int Count) : RebarLayout;
    public sealed record PerimeterSpacing(BarSize Bar, double MaxSpacingMm) : RebarLayout;
}

// The reinforced-concrete section receipt: the assembled VividOrange.Sections IConcreteSection plus the resolved
// gross steel area the Profiles/capacity solvers and the QTO seam read, never a re-derived bar-area sum.
public sealed record RcSection(IConcreteSection Section, EnConcreteMaterial Concrete, EnRebarMaterial Rebar, double GrossSteelAreaMm2, double CoverMm) {
    public Profile ConcreteProfile { get; init; }   // the Materials Profile the concrete outline came from (QTO key)
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The EN-grade admission boundary: the VividOrange.Materials grade ctors throw on an unknown grade / untabulated annex
// (the .api [BOUNDARY_EXCEPTION_LAW]); EnGrade traps the throw ONCE and lowers it onto the typed ConnectionFault rail,
// so no VividOrange throw and no non-EN grade reaches the RcSection.Of interior.
public static class EnGrade {
    public static Fin<EnConcreteMaterial> Concrete(EnConcreteGrade grade, NationalAnnex annex, Op key) =>
        Try(() => new EnConcreteMaterial(grade, annex)).ToFin()
            .MapFail(e => ConnectionFault.Grade(key, $"<en-concrete-grade:{grade}:{annex}:{e.Message}>"));

    public static Fin<EnRebarMaterial> Rebar(Option<EnRebarGrade> grade, NationalAnnex annex, Op key) =>
        grade.Match(
            Some: g => Try(() => new EnRebarMaterial(g, annex)).ToFin()
                .MapFail(e => ConnectionFault.Grade(key, $"<en-rebar-grade:{g}:{annex}:{e.Message}>")),
            None: () => Fin.Fail<EnRebarMaterial>(ConnectionFault.Grade(key, "<rebar-grade-not-en-bodied-for-rc-section>")));
}

public static class RcSectionBuilder {
    // The ONE reinforced-section boundary: lower the EN grades, build the ConcreteSection from the Profile's IProfile +
    // concrete IMaterial + a Link + cover, fold each RebarLayout to its placement engine and AddRebarLayer it, read back
    // section.Rebars. Every VividOrange throw (InvalidMaterialTypeException/InvalidProfileTypeException/ArgumentException)
    // is trapped here onto ConnectionFault — the IConcreteSection egress carries only validated DATA.
    public static Fin<RcSection> Of(Profile concrete, EnConcreteGrade concreteGrade, RebarGrade barGrade, BarSize linkSize, Seq<RebarLayout> layout, double coverMm, NationalAnnex annex, Op key) =>
        from concreteMaterial in EnGrade.Concrete(concreteGrade, annex, key)
        from rebarMaterial in EnGrade.Rebar(barGrade.EnGrade, annex, key)
        from profile in ParametricSection.ProfileOf(concrete, key)   // profile#PROFILE_OWNER IProfile perimeter
        from built in Try(() => Build(profile, concreteMaterial, rebarMaterial, linkSize, layout, coverMm)).ToFin()
            .MapFail(e => ConnectionFault.Capacity(key, $"<rc-section-build:{concrete.Family.Key}:{e.Message}>"))
        let area = built.Rebars.Sum(r => Math.PI * 0.25 * r.Rebar.Diameter.Millimeters * r.Rebar.Diameter.Millimeters * r.CountPerBundle)
        select new RcSection(built, concreteMaterial, rebarMaterial, area, coverMm) { ConcreteProfile = concrete };

    static ConcreteSection Build(IProfile profile, EnConcreteMaterial concrete, EnRebarMaterial rebar, BarSize linkSize, Seq<RebarLayout> layout, double coverMm) {
        Link link = new(rebar, LinkDiameter(linkSize, rebar));
        ConcreteSection section = new(profile, concrete, link, Length.FromMillimeters(coverMm));
        layout.Iter(l => section.AddRebarLayer(LayerOf(l, rebar)));
        return section;
    }

    // Each RebarLayout case -> its VividOrange.Sections placement engine; a metric BarSize feeds the catalogued
    // BarDiameter ctor, an imperial/CSA BarSize a raw Length(nominalDiameterMm) — one Rebar per layout, never a literal.
    // The generated [Union] Switch is the totality proof: a fifth RebarLayout case breaks this arm at compile time,
    // never a runtime-silent `_` (the deleted `layout switch { … _ => throw }` form).
    static IReinforcementLayer LayerOf(RebarLayout layout, EnRebarMaterial rebar) => layout.Switch(
        faceCount:        c => new FaceReinforcementLayer(c.Face, RebarOf(c.Bar, rebar), c.Count),
        faceSpacing:      s => new FaceReinforcementLayer(s.Face, RebarOf(s.Bar, rebar), Length.FromMillimeters(s.MaxSpacingMm)),
        perimeterCount:   c => (IReinforcementLayer)new PerimeterReinforcementLayer(RebarOf(c.Bar, rebar), c.Count),
        perimeterSpacing: s => new PerimeterReinforcementLayer(RebarOf(s.Bar, rebar), Length.FromMillimeters(s.MaxSpacingMm)));

    static Rebar RebarOf(BarSize size, EnRebarMaterial rebar) =>
        size.Catalogue.Match(Some: d => new Rebar(rebar, d), None: () => new Rebar(rebar, Length.FromMillimeters(size.NominalDiameterMm)));

    static BarDiameter LinkDiameter(BarSize size, EnRebarMaterial rebar) => size.Catalogue.IfNone(BarDiameter.D8);

    // The EC2 clear bar-spacing rule for the annex + bar diameter, read from MinimumReinforcementSpacing rather than an
    // inline EC2 constant; the maxAggregateMm tunes the +(d_g) term the EC2 rule adds to the bar-diameter governed spacing.
    public static Fin<double> MinimumBarSpacingMm(NationalAnnex annex, BarSize size, double maxAggregateMm, Op key) =>
        Try(() => new MinimumReinforcementSpacing(annex).GetMinimumReinforcementSpacing(Length.FromMillimeters(size.NominalDiameterMm)).Millimeters).ToFin()
            .MapFail(e => ConnectionFault.Capacity(key, $"<min-bar-spacing:{annex}:{size.Key}:{e.Message}>"));
}
```

## [04]-[RESEARCH]

- [REBAR_ROW_TRANSCRIPTION]: REALIZED — the ASTM A615/A706 + CSA G30.18 reinforcing-bar catalogue (the #3..#18 imperial and 10M..55M metric nominal columns, the A615 Gr40/60/75/80, A706 Gr60/80 weldable, and CSA 400W/500W weldable grades) seeds through `ConnectionCatalogue.BuildRebarRows(context)` over the `RebarRow` designation/size/grade table, the nominal-diameter/area/weight columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` into the kernel value-object and the grade/size/hook algebra realized as the reinforcement vocabulary; the bar rows are the seed the connection-layout fold consumes and a new bar is one `RebarRow` data addition plus, if novel, one `BarSize`/`RebarGrade` row. The nominal diameter/area/unit-weight values transcribe the published ASTM A615/A706 (US-customary soft-metric) and CSA G30.18 (metric 10M..55M) dimensions; a non-positive column rails the `AcceptValidated` `Fin` so a malformed row drops through `Choose` rather than seeding a degenerate `ConnectionItem`. The `BarSize.BendFactor` is the ACI 318-19 Table 25.3.1 minimum-inside-bend-diameter multiple (6dᵦ for #3..#8, 8dᵦ for #9..#11, 10dᵦ for #14/#18 and the matching metric bands); the `RebarHook.ExtensionFactor` is the ACI 318-19 Table 25.3.2 standard-hook straight extension (12dᵦ for a 90° hook, 6dᵦ for 135°, 4dᵦ for 180°, the latter floored at 65 mm at the host materialization, never here).
- [REBAR_BEND_HOST_MATERIALIZATION]: the `RebarBend(BendDegrees, InsideBendDiameterMm, HookExtensionMm)` scalar tuple is the host-neutral bend receipt — the host boundary lofts the bar centerline polyline and fillets each bend by the inside-bend-diameter radius into the host `Curve`, exactly as `Construction/layout#ASSEMBLY_FOLD` materializes a `Seq<Placement>` of scalar tuples into host geometry. This owner NEVER constructs a host curve: the scalar tuple is the portable schedule data the `Rasm.Bim` wire serializes and the host plug-in materializes, so the reinforcement catalogue stays a leaf below the host boundary. A bent-bar schedule (a stirrup, a column tie, a development hook) is a `RebarBend` per bend plus a `Construction/layout` station-stepped centerline, never a per-shape rebar curve type.
- [IFC_REINFORCING_WIRE]: a reinforcing bar round-trips to IFC 4.3 as an `IfcReinforcingBar` (`PredefinedType` ∈ {MAIN, LIGATURE, STUD, PUNCHING, RING, SHEAR, EDGE, ANCHORING, SPACEBAR}) carrying `NominalDiameter` (the `RebarSection.DiameterMm`), `CrossSectionArea` (`NominalAreaMm2`), `BarLength`, `BarSurface` (PLAIN | TEXTURED), and `SteelGrade` (the `RebarGrade.Key`); a reinforcement mesh round-trips as an `IfcReinforcingMesh` (`MeshLength`/`MeshWidth`/`LongitudinalBarNominalDiameter`/`TransverseBarNominalDiameter`/`LongitudinalBarSpacing`/`TransverseBarSpacing`) over the same `RebarSection` columns. The wire mapping is the `Rasm.Bim` boundary projection (`MaterialId`-keyed to the `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` `YieldStrengthMpa` the `SteelGrade` asserts), host-neutral here — this page emits the scalar columns the wire reads, never an IFC entity. The `RebarBend` ACI 318 hook geometry maps to the `IfcReinforcingBar` `BendingShapeCode`/`BendingParameters` (the ISO 3766 / BS 8666 schedule shape) the federation reads, the angle and bend-diameter the scalar tuple already carries.
- [RC_SECTION_COMPOSITION]: REALIZED — the `[03]-[RC_SECTION]` `RcSection` owner is the host-neutral reinforced-concrete-section assembler over the `VividOrange.Sections` `ConcreteSection`: `RcSectionBuilder.Of` lowers an `EnConcreteGrade`/`EnRebarGrade` through `EnGrade.Concrete`/`Rebar` to the `VividOrange.Materials` `EnConcreteMaterial`/`EnRebarMaterial` DATA (trapping the `ArgumentException`/`MissingNationalAnnexException` derivation throws onto `ConnectionFault.Grade`), builds the `ConcreteSection` from the `profile#PROFILE_OWNER` `ParametricSection.ProfileOf` `IProfile` concrete outline + the concrete `IMaterial` + a `Link` over the EN-10080 `BarDiameter` + the `coverMm` `Length`, and folds the closed `RebarLayout` `[Union]` (the four `FaceReinforcementLayer`/`PerimeterReinforcementLayer` × count/spacing constructors collapsed into one) through `AddRebarLayer` — replacing the hand-rolled RC-section assembly the `.api/api-vividorange-sections.md` `[RAIL_LAW]` `Reject` clause names. The `IConcreteSection` egress is the input the `Profiles/capacity#SECTION_CAPACITY` `ConcreteSectionProperties` transformed-section solver and the `VividOrange.InteractionDiagram` N-M-M capacity engine consume; the EC2 clear bar spacing reads `MinimumReinforcementSpacing.GetMinimumReinforcementSpacing` rather than an inline constant. A metric `BarSize.Catalogue` resolves a `Rebar(IMaterial, BarDiameter)` over the published catalogue, an imperial/CSA size a raw `Length(nominalDiameterMm)`; the `RcSection` is NOT a `ConnectionItem` (a discrete schedule bar) — it is the populated concrete member, the two meeting at the `BarSize`/`RebarGrade` vocabulary. Ripple counterpart: `Profiles/capacity` `[SECTION_CAPACITY]` (the elastic transformed-section + ultimate N-M-M capacity solvers over the `IConcreteSection` minted here).
- [EN_GRADE_BINDING]: REALIZED — `RebarGrade` carries an `Option<EnRebarGrade> EnGrade` (`Some` only for the EN-bodied `B500B`/`B500C` rows added to the band, `None` for the ASTM/CSA spec-nominal bands), so the RC σ(ε)-law path reads a registered EN grade through `RcSectionBuilder.Of` rather than a hand-keyed `f_yk`, while the spec-nominal `MinimumYieldMpa` band stays the connection-schedule receipt — the two strengths disjoint by source (spec-nominal for the schedule, EN-registered for the RC capacity). `BarSize` carries an `Option<BarDiameter> Catalogue` binding the metric rows to the EN-10080 D6..D50 catalogue the `VividOrange.Sections` `Rebar(IMaterial, BarDiameter)` ctor consumes, so a metric bar resolves its diameter from the published catalogue rather than a raw millimetre literal at the `Rebar` boundary; the EN-bodied `RebarStandard.En10080` band cites `En1992` at the `RcSection` `Concrete`/`Rebar` admission.

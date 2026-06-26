# [MATERIALS_JOINT]

THE FOURTH REALIZED CONNECTIONFAMILY and THE CONTINUOUS-CONNECTION VOCABULARY. The joint vocabulary — the `JointKind` weld/adhesive/stud discriminant, the `WeldType`/`AdhesiveClass`/`StudClass` axes, the `JointSection` continuous-connection receipt (throat/bond-line/stud-diameter), and the AWS D1.1 / structural-adhesive `JointRow` table — is the fourth realized `connection#CONNECTION_OWNER` `ConnectionItem` vocabulary, carried in the `ConnectionFamily.Joint` case, the metallurgical/adhesive complement to the discrete reinforcement/fastener/hanger items the axis already owns. A fillet weld is a `ConnectionItem` row, never a `Weld` type: the weld geometry, the electrode strength, and the throat are joint-`ConnectionItem` columns, and the `JointSection` projection feeds the SAME `connection#CONNECTION_OWNER` `ConnectionItem.Of` admission and the SAME `ConnectionCatalogue.Build` fold the three discrete families drive — a weld schedule, a bonded curtain-wall joint, and a composite shear-stud row place through the construction layout fold over one `ConnectionItem`, never a per-joint schedule owner.

A continuous weld/bond/stud is STRUCTURALLY DISTINCT from a discrete placed item: it carries no thread or bar-diameter section, so it cannot fold into an existing family arm the way `anchor` folds into `fastener` (`connection#CONNECTION_OWNER`, `connection.md` line 12). This is the SINGLE deliberate widening of the closed-at-three axis to closed-at-four — justified because `STEEL_COMPOSITE_AND_COLDFORMED` depends on the `StudClass` vocabulary landing here and because the metallurgical/adhesive continuous-connection discipline has no representation among the discrete items. The joint vocabulary grows by data — a new electrode is one `ElectrodeClass` row, a new adhesive one `AdhesiveClass` row, a new stud one `StudClass` row, a new designation one `JointRow` catalogue entry — never a per-joint type. The bond-line/throat/stud geometry is a scalar receipt the host materializes, NEVER a host curve here (the host-neutral scalar-`Placement` discipline `Construction/layout#ASSEMBLY_FOLD` keeps). The page composes `connection#CONNECTION_OWNER` for the `ConnectionItem`/`ConnectionId`/`ConnectionSection`/`ConnectionFault` shape, the `Rasm` kernel `PositiveMagnitude` value-object for every length/area column, the `Appearance/graph#MATERIAL_LIBRARY` `MaterialId` (`metal.steel`/`metal.iron`) appearance column each row carries, and the `properties#MATERIAL_PROPERTY` `Mechanical`/`electrode` capacity receipt the connection-design seam reads by `MaterialId`, never re-derived here.

## [01]-[INDEX]

- [01]-[JOINT_FAMILY]: the `JointKind` weld/adhesive/stud discriminant, the `WeldType`/`ElectrodeClass`/`AdhesiveClass`/`StudClass` axes, the `JointSection` continuous-connection receipt, the `ConnectionSection.Joint` arm with its `NominalMm` throat/bond-line/stud-diameter dispatch, and the `ConnectionCatalogue.BuildJointRows` AWS D1.1 / structural-adhesive row table.

## [02]-[JOINT_FAMILY]

- Owner: the joint vocabulary (`JointKind` the weld/adhesive/stud discriminant, `WeldType` the AWS D1.1 fillet/groove/plug/butt geometry, `ElectrodeClass` the AWS A5.1 filler-metal strength axis, `AdhesiveClass` the structural-adhesive lap-shear/peel axis, `StudClass` the AWS D1.1 stud-welded shear-connector axis, `JointSection` the continuous-connection receipt); `ConnectionCatalogue.BuildJointRows` the registered-row seed `connection#CONNECTION_OWNER` `ConnectionCatalogue.Build` concatenates; the `JointSection.AllowableForceKn` projection emitting the weld/bond/stud capacity the structural-connection-design seam reads.
- Cases: kind {`Weld` (continuous fusion over `WeldType` × `ElectrodeClass`), `Adhesive` (structural bond over `AdhesiveClass`), `Stud` (welded shear connector over `StudClass`)} — the closed continuous-connection family carried in one `JointSection` `[Union]` arm; weld {fillet / groove / plug / butt geometries} over the AWS A5.1 E60XX..E110XX electrode strength band; adhesive {epoxy / methacrylate / polyurethane / silicone-structural} over the lap-shear/peel allowable band; stud {the 1/2in..7/8in AWS D1.1 Type-B headed shear connectors} over diameter/height/spacing — a joint is a `ConnectionItem` row over one `JointKind`, never a joint subtype.
- Entry: `public Fin<double> AllowableForceKn(MaterialId capacity, Func<MaterialId, Fin<MaterialPropertySet>> resolve, Op key)` on `JointSection` — the continuous-connection capacity projection: a weld resolves the AWS D1.1 `0.6·FEXX·Aw` design throat shear from its `ElectrodeClass.TensileMpa` and effective throat area, an adhesive the `LapShearMpa·BondArea` allowable, a stud the AISC 360 Eq I8-1 `0.5·Asc·√(f'c·Ec)` capped at `Rg·Rp·Asc·Fu` shear strength its composite-section seam reads; `ConnectionCatalogue.BuildJointRows(context)` folds the AWS D1.1 / structural-adhesive `JointRow` table through `JointOf` into the registered `ConnectionItem` rows `ConnectionCatalogue.Build` concatenates — one polymorphic catalogue fold, never a `GetWeldByType`/`GetStudBySize` family.
- Packages: Rasm (project — `PositiveMagnitude` for the throat/leg/bond-line/stud-diameter/area columns, never an int-backed `Dimension` that truncates a fractional millimetre throat), Thinktecture.Runtime.Extensions (`[Union]` for `JointKind`, `[SmartEnum<string>]` for the weld/electrode/adhesive/stud axes with the generated total `Switch`, `[KeyMemberEqualityComparer]` for the catalogue key), LanguageExt.Core (`Fin`/`Seq`/`Choose`/`Fold` for the admission rail and the catalogue fold), BCL inbox (`FrozenDictionary`).
- Growth: the joint vocabulary grows by data — a new weld geometry is one `WeldType` row carrying its throat-from-leg factor, a new electrode one `ElectrodeClass` row carrying its tensile strength, a new adhesive one `AdhesiveClass` row carrying its lap-shear allowable, a new stud one `StudClass` row carrying its diameter/height/ultimate, a new designation one `JointRow` catalogue entry — never a per-joint type, never a `Weld`/`AdhesiveBond`/`ShearStud` sibling type. The `ConnectionFamily` axis is now closed-at-FOUR (reinforcement/fastener/hanger/joint), the continuous weld/bond/stud the deliberate fourth case; `anchor` stays a `FastenerKind` arm inside the fastener vocabulary, never a fifth family. A composite floor beam's shear interface references `StudClass` here (`steel#STEEL_FAMILY` `Composite` arm), the one shear-stud vocabulary, never a parallel stud owner.
- Boundary: the joint vocabulary is the fourth realized `ConnectionFamily` — a per-joint `Weld`/`ShearStud` class is the deleted form; `JointSection` composes the `Rasm` kernel `PositiveMagnitude` (the double-backed `> 0` finite magnitude) for every column so the section never re-mints a length primitive and a fractional weld throat (`a = 0.707·leg` for an equal-leg fillet) admits without the truncation an int-backed `Dimension` count would force, the `EffectiveThroatMm`/`BondAreaMm2`/`StudDiameterMm` columns carried as `PositiveMagnitude` receipts the `IfcMechanicalFastener` wire and the structural-design seam read; the continuous weld/bond/stud has no thread or bar-diameter section, so the `ConnectionSection.Joint` arm projects `NominalMm` to the throat (weld), the bond-line thickness (adhesive), or the stud diameter (stud) — the one `NominalMm` dispatch the `connection#CONNECTION_OWNER` `ConnectionSection` already exposes, widened by one arm; the weld throat geometry is the scalar `WeldType.EffectiveThroat(legMm, depthMm)` projection (the AWS D1.1 `0.707·leg` fillet, the `depth` groove, the partial-penetration reduction) the host materializes into the weld bead — NEVER a host `Curve`, the host boundary lofts the weld profile from the scalar throat exactly as the construction layout materializes a `Placement` tuple, so this owner stays host-neutral; the appearance assignment crosses to `Appearance/graph#MATERIAL_LIBRARY` as the `MaterialId` (`metal.steel` for a weld/stud, `metal.iron` for a cast joint) the row's `ConnectionItem.AppearanceId` column carries, never a joint-specific shade; the mechanical capacity crosses to `properties#MATERIAL_PROPERTY` `Mechanical` (`YieldStrengthMpa`/`YoungsModulusMpa`) read by the `CapacityKey` `MaterialId` through the `MaterialPropertySet` key, never re-derived here, so the `ElectrodeClass.TensileMpa` axis is the spec-nominal filler-metal strength and the base-metal capacity is the property-library receipt the `AllowableForceKn` projection composes; `ConnectionCatalogue.BuildJointRows` seeds the `connection#CONNECTION_OWNER` `ConnectionCatalogue.Rows` table with the AWS D1.1 weld / structural-adhesive / shear-stud rows keyed `connection.<designation>` (`connection.weld-fillet-8mm-e70`, `connection.stud-19mm-h100`, `connection.adhesive-epoxy-2mm`), the dimensional columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` so a malformed row drops through `Choose` rather than seeding a degenerate `ConnectionItem`; the placement of a weld schedule or stud pattern reads `Construction/layout#ASSEMBLY_FOLD` `StationStep` over the SAME realized fold (a stud row keyed by spacing is a station-stepped pattern, a continuous weld a single run-length placement), never a parallel joint-layout owner; the item serializes to the IFC 4.3 `IfcMechanicalFastener` (`PredefinedType` the shear-stud `STUDSHEARCONNECTOR`, the weld bead a generic fastener) plus an `IfcRelConnectsWithRealizingElements` for the weld/stud realizing the structural connection at the `Rasm.Bim` boundary (portable scalar data here, never an interior `IfcOpenShell` evaluation) — the `FastenerType` token the `Rasm.Bim` `Semantics/connection#CONNECTION_WIRE` `ConnectionItemWire` carries is the verified `IfcMechanicalFastenerTypeEnum` member name (`STUDSHEARCONNECTOR`/`SHEARCONNECTOR` for the welded stud, `BOLT`/`ANCHORBOLT` for a discrete fastener), never the bare reinforcing-bar `STUD` enum the cast-in bar carries.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ConnectionKeyPolicy, string>]
public sealed partial class WeldType {
    // EffectiveThroat: AWS D1.1 — fillet throat is 0.707·leg (equal-leg), groove throat is the joint depth,
    // plug/slot throat is the plate thickness, butt (CJP) throat is the thinner connected part.
    public static readonly WeldType Fillet = new("fillet", static (leg, depth) => 0.707 * leg);
    public static readonly WeldType Groove = new("groove", static (_, depth) => depth);
    public static readonly WeldType Plug   = new("plug",   static (_, depth) => depth);
    public static readonly WeldType Butt   = new("butt",   static (_, depth) => depth);

    [UseDelegateFromConstructor]
    public partial double EffectiveThroat(double legMm, double depthMm);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ConnectionKeyPolicy, string>]
public sealed partial class ElectrodeClass {
    // AWS A5.1 carbon-steel covered electrodes; TensileMpa is the FEXX minimum filler-metal tensile strength.
    public static readonly ElectrodeClass E60  = new("e60",  tensileMpa: 415.0, appearanceId: "metal.iron");
    public static readonly ElectrodeClass E70  = new("e70",  tensileMpa: 485.0, appearanceId: "metal.steel");
    public static readonly ElectrodeClass E80  = new("e80",  tensileMpa: 550.0, appearanceId: "metal.steel");
    public static readonly ElectrodeClass E90  = new("e90",  tensileMpa: 620.0, appearanceId: "metal.steel");
    public static readonly ElectrodeClass E100 = new("e100", tensileMpa: 690.0, appearanceId: "metal.steel");
    public static readonly ElectrodeClass E110 = new("e110", tensileMpa: 760.0, appearanceId: "metal.steel");
    public double TensileMpa { get; }
    public string AppearanceId { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ConnectionKeyPolicy, string>]
public sealed partial class AdhesiveClass {
    // Structural-adhesive lap-shear (ASTM D1002) and T-peel (ASTM D1876) allowables.
    public static readonly AdhesiveClass Epoxy        = new("epoxy",        lapShearMpa: 30.0, peelNmm: 5.0,  serviceCelsius: 80.0);
    public static readonly AdhesiveClass Methacrylate = new("methacrylate", lapShearMpa: 25.0, peelNmm: 12.0, serviceCelsius: 100.0);
    public static readonly AdhesiveClass Polyurethane = new("polyurethane", lapShearMpa: 15.0, peelNmm: 20.0, serviceCelsius: 90.0);
    public static readonly AdhesiveClass SiliconeStructural = new("silicone-structural", lapShearMpa: 1.0, peelNmm: 8.0, serviceCelsius: 150.0);
    public double LapShearMpa { get; }
    public double PeelNmm { get; }
    public double ServiceCelsius { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ConnectionKeyPolicy, string>]
public sealed partial class StudClass {
    // AWS D1.1 Type-B headed shear connectors (composite construction); diameter / standard height / ultimate strength.
    public static readonly StudClass S13 = new("stud-1/2",  diameterMm: 12.7,  standardHeightMm: 75.0,  ultimateMpa: 450.0);
    public static readonly StudClass S16 = new("stud-5/8",  diameterMm: 15.9,  standardHeightMm: 100.0, ultimateMpa: 450.0);
    public static readonly StudClass S19 = new("stud-3/4",  diameterMm: 19.1,  standardHeightMm: 100.0, ultimateMpa: 450.0);
    public static readonly StudClass S22 = new("stud-7/8",  diameterMm: 22.2,  standardHeightMm: 125.0, ultimateMpa: 450.0);
    public double DiameterMm { get; }
    public double StandardHeightMm { get; }
    public double UltimateMpa { get; }
    public double AreaMm2 => Math.PI * 0.25 * DiameterMm * DiameterMm;
}

// --- [MODELS] ------------------------------------------------------------------------------
// One continuous-connection receipt across all three joint kinds, the connection#CONNECTION_OWNER
// ConnectionSection.Joint arm carries; NominalMm dispatches throat (weld), bond-line (adhesive), diameter (stud).
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record JointSection {
    private JointSection() { }

    public sealed record Weld(WeldType Type, ElectrodeClass Electrode, PositiveMagnitude LegMm, PositiveMagnitude DepthMm, PositiveMagnitude LengthMm) : JointSection;
    public sealed record Adhesive(AdhesiveClass Class, PositiveMagnitude BondLineMm, PositiveMagnitude OverlapMm, PositiveMagnitude WidthMm) : JointSection;
    public sealed record Stud(StudClass Class, PositiveMagnitude HeightMm, PositiveMagnitude SpacingMm) : JointSection;

    public JointKind Kind => Switch<JointKind>(
        weld:     static _ => JointKind.Weld,
        adhesive: static _ => JointKind.Adhesive,
        stud:     static _ => JointKind.Stud);

    public PositiveMagnitude NominalMm => Switch(
        weld:     static w => PositiveMagnitude.Create(w.Type.EffectiveThroat(w.LegMm.Value, w.DepthMm.Value)),
        adhesive: static a => a.BondLineMm,
        stud:     static s => PositiveMagnitude.Create(s.Class.DiameterMm));

    public MaterialId AppearanceId => Switch(
        weld:     static w => MaterialId.Of(w.Electrode.AppearanceId),
        adhesive: static _ => MaterialId.Of("polymer.adhesive"),
        stud:     static _ => MaterialId.Of("metal.steel"));

    // Continuous-connection design capacity: weld = AWS D1.1 0.6·FEXX·throat·length, adhesive = lap-shear·bond-area,
    // stud = AISC 360 Eq I8-1 0.5·Asc·√(f'c·Ec) capped at Rg·Rp·Asc·Fu (base-metal f'c/Ec read from the property set).
    public Fin<double> AllowableForceKn(MaterialId capacity, Func<MaterialId, Fin<MaterialPropertySet>> resolve, Op key) => Switch(
        weld:     w => Fin.Succ(0.6 * w.Electrode.TensileMpa * w.Type.EffectiveThroat(w.LegMm.Value, w.DepthMm.Value) * w.LengthMm.Value * 1e-3),
        adhesive: a => Fin.Succ(a.Class.LapShearMpa * a.OverlapMm.Value * a.WidthMm.Value * 1e-3),
        stud:     s => from props in resolve(capacity)
                       from mech in props.Mechanical.ToFin(ConnectionFault.Capacity(key, $"<stud-base-metal-missing:{capacity.Value}>"))
                       let ec = Math.Sqrt(mech.YoungsModulusMpa)
                       let qn = 0.5 * s.Class.AreaMm2 * Math.Sqrt(Math.Max(0.0, mech.YieldStrengthMpa) * ec)
                       let cap = s.Class.AreaMm2 * s.Class.UltimateMpa
                       select Math.Min(qn, cap) * 1e-3);
}

public readonly record struct JointRow(JointKind Kind, string Designation, string Type, double AMm, double BMm, double CMm);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ConnectionCatalogue {
    // AWS D1.1 weld / structural-adhesive / shear-stud seed rows; A=primary dim, B=secondary, C=run-length/overlap.
    static readonly Seq<JointRow> JointRows = Seq(
        new JointRow(JointKind.Weld,     "connection.weld-fillet-6mm-e70",  "fillet:e70",  6.0,  6.0,  100.0),
        new JointRow(JointKind.Weld,     "connection.weld-fillet-8mm-e70",  "fillet:e70",  8.0,  8.0,  150.0),
        new JointRow(JointKind.Weld,     "connection.weld-fillet-10mm-e80", "fillet:e80",  10.0, 10.0, 200.0),
        new JointRow(JointKind.Weld,     "connection.weld-groove-12mm-e80", "groove:e80",  12.0, 12.0, 250.0),
        new JointRow(JointKind.Weld,     "connection.weld-butt-cjp-e90",    "butt:e90",    16.0, 16.0, 300.0),
        new JointRow(JointKind.Stud,     "connection.stud-13mm-h75",        "stud-1/2",    13.0, 75.0,  150.0),
        new JointRow(JointKind.Stud,     "connection.stud-19mm-h100",       "stud-3/4",    19.0, 100.0, 200.0),
        new JointRow(JointKind.Stud,     "connection.stud-22mm-h125",       "stud-7/8",    22.0, 125.0, 250.0),
        new JointRow(JointKind.Adhesive, "connection.adhesive-epoxy-2mm",   "epoxy",        2.0,  25.0,  50.0),
        new JointRow(JointKind.Adhesive, "connection.adhesive-mma-1mm",     "methacrylate", 1.0,  20.0,  40.0));

    static Fin<(ConnectionId Id, ConnectionItem Item)> JointOf(JointRow r, Context context, Op key) =>
        from section in r.Kind.Switch(
            weld:     _ => WeldOf(r, key),
            adhesive: _ => AdhesiveOf(r, key),
            stud:     _ => StudOf(r, key))
        from item in ConnectionItem.Of(ConnectionFamily.Joint, r.Designation, new ConnectionSection.Joint(section), section.AppearanceId, section.AppearanceId, key)
        select (ConnectionId.Of(r.Designation), item);

    static Fin<JointSection> WeldOf(JointRow r, Op key) =>
        from type in r.Type.Split(':') is [var t, _] && WeldType.TryGet(t, out WeldType? wt) ? Fin.Succ(wt!) : Fin.Fail<WeldType>(ConnectionFault.Designation(key, $"<unknown-weld-type:{r.Type}>"))
        from electrode in r.Type.Split(':') is [_, var e] && ElectrodeClass.TryGet(e, out ElectrodeClass? ec) ? Fin.Succ(ec!) : Fin.Fail<ElectrodeClass>(ConnectionFault.Grade(key, $"<unknown-electrode:{r.Type}>"))
        from leg in key.AcceptValidated<PositiveMagnitude>(candidate: r.AMm)
        from depth in key.AcceptValidated<PositiveMagnitude>(candidate: r.BMm)
        from length in key.AcceptValidated<PositiveMagnitude>(candidate: r.CMm)
        select (JointSection)new JointSection.Weld(type, electrode, leg, depth, length);

    static Fin<JointSection> AdhesiveOf(JointRow r, Op key) =>
        from cls in AdhesiveClass.TryGet(r.Type, out AdhesiveClass? a) ? Fin.Succ(a!) : Fin.Fail<AdhesiveClass>(ConnectionFault.Grade(key, $"<unknown-adhesive:{r.Type}>"))
        from bond in key.AcceptValidated<PositiveMagnitude>(candidate: r.AMm)
        from overlap in key.AcceptValidated<PositiveMagnitude>(candidate: r.BMm)
        from width in key.AcceptValidated<PositiveMagnitude>(candidate: r.CMm)
        select (JointSection)new JointSection.Adhesive(cls, bond, overlap, width);

    static Fin<JointSection> StudOf(JointRow r, Op key) =>
        from cls in StudClass.Items.Find(s => Math.Abs(s.DiameterMm - r.AMm) < 0.5).ToFin(ConnectionFault.Designation(key, $"<unknown-stud-diameter:{r.AMm}>"))
        from height in key.AcceptValidated<PositiveMagnitude>(candidate: r.BMm)
        from spacing in key.AcceptValidated<PositiveMagnitude>(candidate: r.CMm)
        select (JointSection)new JointSection.Stud(cls, height, spacing);

    public static FrozenDictionary<ConnectionId, ConnectionItem> BuildJointRows(Context context) =>
        JointRows
            .Choose(row => JointOf(row, context, default).ToOption())
            .ToFrozenDictionary(static r => r.Id, static r => r.Item, ConnectionKeyPolicy.EqualityComparer);
}
```

## [03]-[RESEARCH]

- [AWS_D11_WELD_TABLE]: REALIZED — the AWS D1.1 structural-welding geometry seeds through `ConnectionCatalogue.BuildJointRows(context)` over the `JointRow` kind/designation/type table, the leg/depth/length columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` into the kernel value-object. The `WeldType.EffectiveThroat` factor is the AWS D1.1 throat rule (`0.707·leg` for an equal-leg fillet, the joint depth for a groove/PJP, the plate thickness for a plug/slot, the thinner part for a CJP butt); the `ElectrodeClass.TensileMpa` is the AWS A5.1 minimum filler-metal tensile (E60XX=415, E70XX=485, E80XX=550, E90XX=620, E100XX=690, E110XX=760 MPa). The weld design shear is the AWS D1.1 `0.6·FEXX·throat·length`, the `0.6` the shear-fraction-of-tensile factor; a non-positive column rails the `AcceptValidated` `Fin` so a malformed row drops through `Choose`. The matching base-metal capacity is the `properties#MATERIAL_PROPERTY` `Mechanical.YieldStrengthMpa` read by `CapacityKey`, never duplicated as a weld column.
- [STRUCTURAL_ADHESIVE_ALLOWABLE]: REALIZED — the `AdhesiveClass` lap-shear (ASTM D1002 single-lap-joint) and T-peel (ASTM D1876) allowables seed the structural-adhesive rows; the bonded-joint design capacity is `LapShearMpa·overlap·width`, the overlap length the shear-transfer area a lap joint develops. Epoxy (30 MPa lap-shear, 80 °C service), methacrylate/MMA (25 MPa, high peel, 100 °C), polyurethane (15 MPa, very high peel/flexibility), and structural silicone (1 MPa, the SSG curtain-wall sealant the highest service temperature) are the seed; a measured manufacturer adhesive lands one `AdhesiveClass` row with its datasheet lap-shear/peel/service columns. The bonded curtain-wall joint the structural-silicone case admits keys and serializes the way a bolt does.
- [AWS_D11_SHEAR_STUD]: REALIZED — the AWS D1.1 Type-B headed shear connectors (the composite-construction welded studs) seed the `StudClass` axis at the standard 1/2in..7/8in diameters, the standard heights, and the 450 MPa minimum ultimate tensile. The composite shear capacity is the AISC 360-22 Eq I8-1 `Qn = 0.5·Asc·√(f'c·Ec) ≤ Rg·Rp·Asc·Fu`, the concrete-strength terms (`f'c`/`Ec`) read from the base-metal/concrete `MaterialPropertySet` the `CapacityKey` names and `Ec = √Es` approximating the concrete elastic modulus from the stored Young's modulus, the `Asc·Fu` cap the stud-steel ultimate. This is the `StudClass` vocabulary the `steel#STEEL_FAMILY` `Composite` arm references for the steel-concrete shear interface (the `STEEL_COMPOSITE_AND_COLDFORMED` task), the one shear-stud owner both pages read — a composite floor beam's stud row keys `connection.stud-19mm-h100` and the composite section reads its `Qn` per stud from this projection, never a re-minted stud type.
- [IFC_JOINT_WIRE]: a continuous weld/stud round-trips to IFC 4.3 as an `IfcMechanicalFastener` (`PredefinedType` the `STUDSHEARCONNECTOR` shear stud, a generic fastener for the weld bead — `STUD` is the `IfcReinforcingBarTypeEnum` cast-in bar, NOT a `IfcMechanicalFastenerTypeEnum` value, so the welded stud is `STUDSHEARCONNECTOR` on the fastener wire) carrying its diameter and run-length on the associated `IfcMaterialProfileSetUsage` circle-profile cross-section (the verified GeometryGym round-trip channel — `IfcMechanicalFastener.mNominalDiameter`/`mNominalLength` are `internal` fields with no public getter on the occurrence OR its type, so the wire never writes them directly), realizing the structural connection through an `IfcRelConnectsWithRealizingElements` relating the realizing weld/stud element to the connected structural members; a bonded joint round-trips as the same realizing-element relation with the adhesive as the realizing element. The wire mapping is the `Rasm.Bim` boundary projection (`MaterialId`-keyed to the `properties#MATERIAL_PROPERTY` `Mechanical` the electrode/adhesive/stud asserts), host-neutral here — this page emits the scalar columns the `ConnectionItemWire` carrier reads, never an IFC entity. Ripple counterpart: `Rasm.Bim` `Semantics/connection#CONNECTION_WIRE` (the `ConnectionWire.Author` serializer of the `ConnectionItem` axis).

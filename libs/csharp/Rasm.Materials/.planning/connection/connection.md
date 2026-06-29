# [MATERIALS_CONNECTION]

THE POLYMORPHIC CONNECTION OWNER and THE FAMILY GROWTH AXIS. One `ConnectionItem` is the canonical structural-connection-material concept every reinforcement bar, fastener, framing connector, and continuous joint parameterizes — an item shape (the family discriminant, a sectional unit row, an installation-capacity receipt key, an appearance assignment) plus the family vocabulary its layout schedule reads; one `ConnectionFamily` `[SmartEnum<string>]` closes the family-kind axis (reinforcement · fastener · hanger · joint), all four realized as sibling catalogue vocabularies the one `ConnectionCatalogue.Build` folds — the EXACT structural mirror of the `profiles/Profile` owner (`profile#PROFILE_OWNER`, `profile.md` line 13). A connection is NEVER a per-item class: a #5 rebar is a `ConnectionItem` in the `reinforcement` family (`Connection/reinforcement#REINFORCEMENT_FAMILY`), a 3/8in hex bolt a `ConnectionItem` in the `fastener` family (`Connection/fastener#FASTENER_FAMILY`), a Simpson-style joist hanger a `ConnectionItem` in the `hanger` family (`Connection/hanger#HANGER_FAMILY`), an 8 mm fillet weld a `ConnectionItem` in the `joint` family (`Connection/joint#JOINT_FAMILY`), differing only in section columns and the family discriminant — never a `Rebar`/`Bolt`/`Hanger`/`Weld` type. The axis is CLOSED at FOUR families: `anchor` folds as a `FastenerKind` arm inside the fastener vocabulary, never a fifth sibling family; `joint` is the ONE deliberate widening past the discrete reinforcement/fastener/hanger triple, the continuous weld/adhesive/stud carrying no thread or bar-diameter section so it cannot fold into an existing arm the way `anchor` folds into `fastener`. The owner is `IfcReinforcingBar`/`IfcMechanicalFastener`-aligned (the canonical structural-connection elements the BIM wire serializes at the `Rasm.Bim` boundary). The OOP capsule lives at the boundary (the `[ValueObject]` `ConnectionId` key, the `[ValidationError]`-derived `ConnectionFault` band), the FP-ROP rail owns the internals (the `Fin<T>` admission, the `Seq`/`Fold` catalogue projection). The page composes the `Rasm` kernel for the `PositiveMagnitude`/`Dimension` section value-objects exactly as `profile#PROFILE_OWNER` does, `Appearance/graph#MATERIAL_LIBRARY` for the `MaterialId` appearance column (`metal.iron`/`metal.steel`, the `Profile.AppearanceId` mirror at `graph.md` line 212), `properties#MATERIAL_PROPERTY_CATALOGUE` for the `Mechanical` capacity receipt key (read by `MaterialId`, never re-derived here — `properties.md` line 12), and `Construction/layout#ASSEMBLY_FOLD` for the station-stepped placement a rebar schedule or fastener pattern reads over the SAME realized `Resolve` fold (`layout.md` line 66 / `StationStep` line 95), never a parallel layout owner. Each family vocabulary lives on its own sibling page (`reinforcement`/`fastener`/`hanger`/`joint`); this page owns the one `ConnectionItem` shape, the closed family axis, the `ConnectionId` key, the band-2360 `ConnectionFault`, and the `ConnectionCatalogue` registered-row table.

## [01]-[INDEX]

- [01]-[CONNECTION_OWNER]: the one `ConnectionItem` `[Union]` over the closed `ConnectionFamily` `[SmartEnum<string>]` axis (reinforcement/fastener/hanger/joint), the `ConnectionId` `[ValueObject]` key with `ComparerAccessors.StringOrdinal` accessor, the band-2360 `ConnectionFault` `[Union]`, the `ConnectionItem.Of` polymorphic admission entry, and the context-folded `ConnectionCatalogue` registered-row table over all four family builders.
- [02]-[CONNECTION_WIRE]: the host-neutral `ConnectionItemWire` portable carrier this owner DECLARES (the connection-item seam contract `Rasm.Bim` reads, NEVER a `Rasm.Materials` type crossing the boundary) and the `ConnectionWire.ToWire` producer fold lowering a `ConnectionItem` onto it, the egress half of the `Connection → csharp:Rasm.Bim/Model [WIRE]` seam; the material/composition/property egress is RETIRED — the `Projection/material#MATERIAL_PROJECTOR` lowers it into the seam `ElementGraph` `Rasm.Bim` reads directly.

## [02]-[CONNECTION_OWNER]

- Owner: `ConnectionItem` over the closed `ConnectionFamily` axis; `ConnectionId` key; `ConnectionFault` `[Union]` band 2360; `ComparerAccessors.StringOrdinal` accessor; `ConnectionCatalogue` the registered-row table.
- Cases: one `ConnectionItem` shape across all families — `Family` (the discriminant), `Designation` (the `ConnectionId` key, `connection.<designation>`), `Section` (the family-projected sectional unit row as a `ConnectionSection` `[Union]` arm: `Reinforcement`/`Fastener`/`Hanger`/`Joint`), `CapacityKey` (the `MaterialId` whose `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` row carries the installation capacity), `AppearanceId` (the `graph#MATERIAL_LIBRARY` `MaterialId` row); family {reinforcement, fastener, hanger, joint} closed at four (`anchor` is a `FastenerKind` arm inside the fastener vocabulary, never a family; `joint` is the deliberate fourth case carrying the continuous weld/adhesive/stud at `Connection/joint#JOINT_FAMILY`); an item is a `ConnectionFamily` ROW, never a connection subtype.
- Entry: `public static Fin<ConnectionItem> Of(ConnectionFamily family, string designation, ConnectionSection section, MaterialId capacityKey, MaterialId appearanceId, Op key)` — one polymorphic admission entry, never a `GetById`/`GetByFamily` family (the `Profile.Of` law, `profile.md` line 13); `Fin<T>` aborts on a malformed designation (`ConnectionFault.Designation`, key-correlated), a family/section discriminant mismatch (`ConnectionFault.Family`), an out-of-band grade (`ConnectionFault.Grade`), or a non-positive capacity column (`ConnectionFault.Capacity`); `ConnectionCatalogue.Build(context)` folds every family's row builder (`reinforcement#REINFORCEMENT_FAMILY` `BuildRebarRows` plus `fastener#FASTENER_FAMILY` `BuildFastenerRows` plus `hanger#HANGER_FAMILY` `BuildHangerRows` plus `joint#JOINT_FAMILY` `BuildJointRows`) into the one frozen registry, `ConnectionCatalogue.Lookup(rows, id, key)` resolves a registered `ConnectionId` to its catalogue `ConnectionItem`, and the same `Of` admits an ad-hoc item through the row validation a registered row passes — one polymorphic entry.
- Packages: Rasm (project — `PositiveMagnitude`/`Dimension`/`UnitInterval` value-objects the family sections compose), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`/`[ValueObject]` generators at their deepest surface — generated total `Switch`, `[KeyMemberEqualityComparer]`/`[KeyMemberComparer]`), LanguageExt.Core (`Fin`/`Seq`/`Fold` for the admission rail and the catalogue fold), BCL inbox (`FrozenDictionary`); no new external package — the connection owner is the structural copy of the proven `profile#PROFILE_OWNER` shape, never a re-invention.
- Growth: a new structural connection is one `ConnectionItem` row in the matching `ConnectionFamily`; a new family is one `ConnectionFamily` case carrying its section vocabulary on its own sibling page folded into `ConnectionCatalogue.Build`; a new fault is one `ConnectionFault` case — never a `Rebar`/`Bolt`/`Hanger`/`Weld` type, never a per-family `ConnectionItem` variant. All four families are realized, each its own `ConnectionFamily` case with its own `ConnectionSection` arm and `BuildXRows` catalogue builder: `reinforcement` carries the `RebarGrade`/`BarSize` axis and the ASTM A615/A706 bar-diameter/area/unit-weight rows plus the ACI 318 bend/hook scalar tuple, `fastener` the `FastenerKind` (bolt/nut/screw/anchor) discriminant and the ISO 898-1/SAE J429 `FastenerGrade` proof/tensile axis over the thread/pitch/shank/head columns, `hanger` the `HangerType` framing-connector discriminant over the carried-member/download-uplift/gauge columns, `joint` the `JointKind` (weld/adhesive/stud) discriminant and the AWS D1.1 / structural-adhesive throat/bond-line/stud-diameter columns over the continuous-connection receipt — a new bar size, bolt grade, hanger, or electrode is a row data addition, never a new surface. The axis is closed at four: `joint` is the deliberate continuous-connection widening, the metallurgical/adhesive complement to the discrete items, justified because a weld/bond/stud has no `NominalMm` thread or bar-diameter and the `STEEL_COMPOSITE_AND_COLDFORMED` composite leg reads the `joint#JOINT_FAMILY` `StudClass` vocabulary.
- Boundary: `ConnectionItem` is the ONE structural-connection concept — a per-item class is the deleted form; the `ConnectionSection` `[Union]` (`Reinforcement`/`Fastener`/`Hanger`) carries each family's projected columns so the one item shape never branches into per-family types, and every length column composes the `Rasm` kernel `PositiveMagnitude` value-object (the double-backed `> 0` finite magnitude) exactly as `ProfileUnit` does (`profile.md` line 16) so a connection never re-mints a dimension primitive and a fractional bar diameter — an 11.3 mm #4 bar, a 9.525 mm 3/8in bolt — admits without the truncation an int-backed `Dimension` count would force, the `Dimension` carrier reserved for discrete counts (bar layers, thread starts); `ConnectionFault` is the one fault every `Fin.Fail` reads (designation/grade/capacity/family slots), an `Expected`-derived `Error` (`IValidationError<ConnectionFault>`) whose 2360 band IS the `Expected` `Code` so a bare typed case lifts directly into the `Fin<T>` rail (the `ProfileFault` law, `profile.md` lines 41-48), disjoint from `ProfileFault` 2300 / `ConstructionFault` 2350 / kernel `GeometryFault` 2400 / `MaterialFault` 2450 (the four-band statement, `bsdf.md` line 3 + `ARCHITECTURE.md` line 39), so a connection schedule never throws and never returns a sentinel; the `[ValueObject]` `ConnectionId` key and the `[ValidationError]`-derived `ConnectionFault` are the OOP capsule at the edge, the `Fin`/`Seq`/`Fold` catalogue projection is the FP-ROP internal; the capacity receipt is NOT re-derived here — `CapacityKey` is the `MaterialId` whose `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` row (`DensityKgM3`/`YoungsModulusMpa`/`YieldStrengthMpa`/`PoissonsRatio`/`ThermalExpansionPerK`, `properties.md` line 12) the structural-connection-design seam reads by key, so a bolt's proof load and a bar's yield are read once from the property library, never duplicated as a connection column; the appearance assignment crosses to `Appearance/graph#MATERIAL_LIBRARY` as a `MaterialId` row a `ConnectionItem.AppearanceId` column carries (the `Profile.AppearanceId` mirror, `graph.md` line 212), never a connection-specific surface; the placement that turns a `ConnectionItem` schedule into a station-stepped stream is `Construction/layout#ASSEMBLY_FOLD` `Resolve`/`StationStep` (`layout.md` line 66 / line 95), composed not re-derived here — a rebar schedule and a fastener pattern are station-stepped placements over the SAME fold, never a parallel `connection/layout` owner; the bend/hook geometry of a reinforcement bar is a scalar bend-angle/radius/hook-extension tuple the host materializes (`Connection/reinforcement#REINFORCEMENT_FAMILY`), NEVER a host curve here (the host-neutral scalar-`Placement` discipline, `layout.md` line 17); the family vocabularies (`RebarGrade`/`BarSize`/`RebarSection`, `FastenerKind`/`FastenerGrade`/`FastenerSection`, `HangerType`/`HangerSection`, `JointKind`/`WeldType`/`ElectrodeClass`/`AdhesiveClass`/`StudClass`/`JointSection`) live on their sibling pages, and the `ConnectionCatalogue.Build` registered-row table folds every family's `BuildXRows` into one frozen registry so a registered row keys the same way as each family lands its own builder; the item serializes to the IFC 4.3 `IfcReinforcingBar`/`IfcReinforcingMesh`/`IfcMechanicalFastener`/`IfcFastener` element (a weld/stud realizing a connection through `IfcRelConnectsWithRealizingElements`) at the `Rasm.Bim` boundary (portable scalar data here, never an interior `IfcOpenShell` evaluation), the per-family wire shape noted on each sibling page.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct ConnectionId {
    static partial void NormalizeAndValidate(ref string value, ref ValidationError? validationError) =>
        validationError = string.IsNullOrWhiteSpace(value) || !value.StartsWith("connection.", StringComparison.Ordinal) || value.Length <= "connection.".Length
            ? new ValidationError("<connection-id requires 'connection.<designation>'>")
            : null;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConnectionFamily {
    public static readonly ConnectionFamily Reinforcement = new("reinforcement");
    public static readonly ConnectionFamily Fastener = new("fastener");
    public static readonly ConnectionFamily Hanger = new("hanger");
    public static readonly ConnectionFamily Joint = new("joint");
}

// --- [ERRORS] ------------------------------------------------------------------------------
[Union]
public abstract partial record ConnectionFault : Expected, IValidationError<ConnectionFault> {
    private ConnectionFault(Op key, string detail) : base(detail, 2360, None) => Key = key;
    public Op Key { get; }
    public static ConnectionFault Create(string message) => new Family(default, message);
    public sealed record Designation(Op Key, string Detail) : ConnectionFault(Key, Detail) { public override string Category => "Designation"; }
    public sealed record Grade(Op Key, string Detail) : ConnectionFault(Key, Detail) { public override string Category => "Grade"; }
    public sealed record Capacity(Op Key, string Detail) : ConnectionFault(Key, Detail) { public override string Category => "Capacity"; }
    public sealed record Family(Op Key, string Detail) : ConnectionFault(Key, Detail) { public override string Category => "Family"; }
}

// --- [SERVICES] ----------------------------------------------------------------------------

// --- [MODELS] ------------------------------------------------------------------------------
[Union]
public abstract partial record ConnectionSection {
    public sealed record Reinforcement(RebarSection Bar) : ConnectionSection;
    public sealed record Fastener(FastenerSection Bolt) : ConnectionSection;
    public sealed record Hanger(HangerSection Connector) : ConnectionSection;
    public sealed record Joint(JointSection Continuous) : ConnectionSection;

    public ConnectionFamily Family => Switch(
        reinforcement: static _ => ConnectionFamily.Reinforcement,
        fastener:      static _ => ConnectionFamily.Fastener,
        hanger:        static _ => ConnectionFamily.Hanger,
        joint:         static _ => ConnectionFamily.Joint);

    // The continuous weld/bond/stud has no thread or bar-diameter; NominalMm resolves to throat / bond-line / stud-diameter.
    public PositiveMagnitude NominalMm => Switch(
        reinforcement: static r => r.Bar.DiameterMm,
        fastener:      static f => f.Bolt.ThreadDiameterMm,
        hanger:        static h => h.Connector.CarriedMemberWidthMm,
        joint:         static j => j.Continuous.NominalMm);
}

public sealed record ConnectionItem(
    ConnectionFamily Family,
    ConnectionId Designation,
    ConnectionSection Section,
    MaterialId CapacityKey,
    MaterialId AppearanceId) {

    public static Fin<ConnectionItem> Of(ConnectionFamily family, string designation, ConnectionSection section, MaterialId capacityKey, MaterialId appearanceId, Op key) =>
        from id in ConnectionId.Validate(value: designation, provider: CultureInfo.InvariantCulture, item: out ConnectionId? built) is { } error
            ? Fin.Fail<ConnectionId>(ConnectionFault.Designation(key, $"<malformed-designation:{designation}:{error.Message}>"))
            : Fin.Succ(built!)
        from matched in guard(section.Family == family, ConnectionFault.Family(key, $"<family-section-mismatch:{family.Key}<>{section.Family.Key}>"))
        from positive in guard(section.NominalMm.Value > 0.0, ConnectionFault.Capacity(key, $"<non-positive-nominal:{section.NominalMm.Value}>"))
        select new ConnectionItem(family, id, section, capacityKey, appearanceId);
}

// --- [TABLES] ------------------------------------------------------------------------------
public static class ConnectionCatalogue {
    public static FrozenDictionary<ConnectionId, ConnectionItem> Build(Context context) =>
        Reinforcement.ConnectionCatalogue.BuildRebarRows(context)
            .Concat(Fastener.ConnectionCatalogue.BuildFastenerRows(context))
            .Concat(Hanger.ConnectionCatalogue.BuildHangerRows(context))
            .Concat(Joint.ConnectionCatalogue.BuildJointRows(context))
            .ToFrozenDictionary(static r => r.Key, static r => r.Value, ComparerAccessors.StringOrdinal.EqualityComparer);

    public static Fin<ConnectionItem> Lookup(FrozenDictionary<ConnectionId, ConnectionItem> rows, ConnectionId id, Op key) =>
        rows.TryGetValue(id, out ConnectionItem? row) ? Fin.Succ(row!) : Fin.Fail<ConnectionItem>(ConnectionFault.Family(key, $"<unregistered-connection:{id.Value}>"));
}
```

## [03]-[CONNECTION_WIRE]

- Owner: `ConnectionItemWire` the portable connection-item carrier THIS owner declares as the host-neutral seam contract the `Rasm.Bim` `Model` consumer reads; `ConnectionWire` the static egress kernel folding a `ConnectionItem` onto it — the producer half of the `Connection → csharp:Rasm.Bim/Model [WIRE]` seam, host-neutral scalar/string data, NEVER a `Rasm.Materials` type crossing the boundary. The material/composition/property egress is RETIRED from this owner: the `Projection/material#MATERIAL_PROJECTOR` lowers a material's `MaterialComposition`/`MaterialPropertySet`/`Appearance`/`Assessment` into the seam `ElementGraph`, so the prior `MaterialAssignmentWire`/`MaterialPropertyWire` carriers (which lowered the retired `MaterialAssignment` and the relocated lifecycle cases) are gone — `Rasm.Bim` reads the material subgraph from the seam graph, never a Materials wire.
- Cases: one `ConnectionItemWire` shape across all four families — `ConnectionId` (the `Designation.Value` string), `Family` (the IFC entity discriminant the Bim author switches on: `ReinforcingBar` for a reinforcement item, `Fastener` otherwise), `Section` (the family `SmartEnum` key the `IfcMaterialProfileSet` name carries, e.g. `#5`/`M12`/`joist-hanger`/`fillet`/`stud-3/4`), `NominalDiameter` (the `ConnectionSection.NominalMm.Value` — bar diameter / thread diameter / weld throat / stud diameter, the cross-section radius the fastener round-trips), `Length` (the run/bar/shank/stud length the section carries), `FastenerType` (the verified `IfcMechanicalFastenerTypeEnum` member spelling: `STUDSHEARCONNECTOR`/`SHEARCONNECTOR` for a welded stud, `BOLT`/`ANCHORBOLT` for a discrete fastener, empty for a reinforcing bar — NEVER the `IfcReinforcingBarTypeEnum` `STUD` the cast-in bar carries); one carrier spanning every family by discriminant, never a per-family wire.
- Entry: `public static ConnectionItemWire ToWire(ConnectionItem item)` projects one registered/admitted `ConnectionItem` onto its portable carrier (the `Family`/`Section`/`FastenerType` derived from the `ConnectionSection` arm through the generated total `Switch`, the `NominalDiameter`/`Length` read from the section's `PositiveMagnitude` columns); the projection is TOTAL (no `Fin<T>` rail — the owner already admitted every column at `ConnectionItem.Of`, so egress only reads validated scalars and a malformed source can never reach here).
- Packages: Thinktecture.Runtime.Extensions (the `ConnectionSection`/`JointSection` arm dispatch the projection reads through the generated total `Switch`), LanguageExt.Core (`Seq`/`Map` for the carrier projection), Rasm (project — the `PositiveMagnitude` value-objects the carrier unwraps to scalar at the egress); no new external package — the wire carrier is a plain record and the `ToWire` fold reads the realized owner.
- Growth: a new connection family is one `ConnectionSection` arm the `ToWire` `Switch` reads (no new carrier — the `ConnectionItemWire` shape spans every family by the `Family`/`Section`/`FastenerType` discriminant); a future `Connection` projection (one `IElementProjection` registration row, the `Projection/material#MATERIAL_PROJECTOR` sibling) would subsume even this carrier by lowering a connection item into the seam `Object` graph — until then the `ConnectionItemWire` is the connection-item egress, never a `Fin<T>` rail, never a Materials owner type crossing the boundary.
- Boundary: this owner declares the ONE `ConnectionItemWire` carrier as the connection-item seam contract — the `Rasm.Bim` `Model` consumer reads the IDENTICAL field shapes one-hop and emits the IFC 4.3 `IfcReinforcingBar`/`IfcReinforcingMesh`/`IfcMechanicalFastener`/`IfcFastener` element (a weld/stud realizing a connection through `IfcRelConnectsWithRealizingElements`), the seam aligning by the `ConnectionId` content-key; the egress is `BOUNDARY_ADMISSION` parameterized (every column already admitted once at `ConnectionItem.Of`, so `ToWire` reads only validated scalars, no `null`/sentinel, no host shape); the `FastenerType` token is the VERIFIED `IfcMechanicalFastenerTypeEnum` member spelling (`STUDSHEARCONNECTOR`/`SHEARCONNECTOR`/`BOLT`/`ANCHORBOLT`, the joint stud the `STUDSHEARCONNECTOR` the `Connection/joint#JOINT_FAMILY` `[IFC_JOINT_WIRE]` fixes — never the `IfcReinforcingBarTypeEnum` `STUD` the cast-in bar carries), so the Bim `Enum.TryParse<IfcMechanicalFastenerTypeEnum>` round-trips the token the producer emits; the MATERIAL binding of a connection item (a rebar's steel grade, a bolt's material, its embodied carbon and classification) rides the `Projection/material#MATERIAL_PROJECTOR` material subgraph and the `Associate` edge keyed by the connection's element NodeId, NOT a wire column — the connection-item geometry/IFC-class egress is this carrier, the material/property/composition egress the seam graph; this owner re-mints NO IFC entity and NO `IfcOpenShell` evaluation — it produces ONLY the portable scalar/string carrier, the IFC author living strictly in `Rasm.Bim`.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// The portable connection-item carrier THIS owner declares as the host-neutral seam contract — scalar/string rows
// the Rasm.Bim Model consumer reads with the IDENTICAL field shapes one-hop, NEVER a Rasm.Materials ConnectionItem
// crossing the seam. The MaterialAssignmentWire/MaterialPropertyWire carriers are RETIRED: the MaterialProjector
// lowers a material's composition/property/appearance/assessment into the seam ElementGraph Rasm.Bim reads directly.
public sealed record ConnectionItemWire(string ConnectionId, string Family, string Section, double NominalDiameter, double Length, string FastenerType);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class ConnectionWire {
    // The connection egress: one ConnectionItem -> its portable carrier, the Family/Section/NominalDiameter/Length/FastenerType
    // read from the ConnectionSection arm through the generated total Switch. A reinforcement bar is the IfcReinforcingBar family
    // with an empty FastenerType (the bar carries no IfcMechanicalFastenerTypeEnum); a fastener carries its FastenerKind.IfcPredefinedType
    // (BOLT/NUT/SCREW/ANCHORBOLT); a hanger is a BOLT-fastened connector; a joint derives its token through the JointSection arm
    // (a welded stud STUDSHEARCONNECTOR, a weld/adhesive bead an empty token the Bim author lands as its generic-fastener default).
    // The Section token is the family SmartEnum key (#5 / M12 / joist-hanger / fillet / stud-3/4) the Bim IfcMaterialProfileSet name carries;
    // NominalDiameter is the ConnectionSection.NominalMm.Value the fastener round-trips as its circle-profile radius; Length the run/shank/depth.
    // TOTAL (no Fin rail — every column already admitted once at ConnectionItem.Of, so egress reads only validated scalars).
    public static ConnectionItemWire ToWire(ConnectionItem item) =>
        item.Section.Switch(
            reinforcement: r => new ConnectionItemWire(item.Designation.Value, "ReinforcingBar", r.Bar.Size.Key, r.Bar.DiameterMm.Value, 0.0, ""),
            fastener:      f => new ConnectionItemWire(item.Designation.Value, "Fastener", f.Bolt.Size.Key, f.Bolt.ThreadDiameterMm.Value, f.Bolt.ShankLengthMm.Value, f.Bolt.Kind.IfcPredefinedType),
            hanger:        h => new ConnectionItemWire(item.Designation.Value, "Fastener", h.Connector.Type.IfcDesignation, h.Connector.CarriedMemberWidthMm.Value, h.Connector.CarriedMemberDepthMm.Value, "BOLT"),
            joint:         j => JointWire(item.Designation.Value, j.Continuous));

    // The joint arm has no flat section/length/token column (the continuous weld/bond/stud is structurally distinct), so the carrier
    // derives through the JointSection's own generated Switch: a weld carries its WeldType key + run-length, an adhesive its class key
    // + overlap, a stud its class key + height and the verified STUDSHEARCONNECTOR token (the only IfcMechanicalFastenerTypeEnum joint member —
    // STUD is the IfcReinforcingBarTypeEnum cast-in bar, not a fastener value, Connection/joint#JOINT_FAMILY [IFC_JOINT_WIRE]).
    static ConnectionItemWire JointWire(string designation, JointSection section) =>
        section.Switch(
            weld:     w => new ConnectionItemWire(designation, "Fastener", w.Type.Key,  section.NominalMm.Value, w.LengthMm.Value,  ""),
            adhesive: a => new ConnectionItemWire(designation, "Fastener", a.Class.Key, section.NominalMm.Value, a.OverlapMm.Value, ""),
            stud:     s => new ConnectionItemWire(designation, "Fastener", s.Class.Key, section.NominalMm.Value, s.HeightMm.Value,  "STUDSHEARCONNECTOR"));
}
```

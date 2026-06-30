# [MATERIALS_CONNECTION]

THE POLYMORPHIC CONNECTION OWNER and THE CONNECTION-ELEMENT PROJECTOR. One `ConnectionItem` is the canonical structural-connection-material concept every reinforcement bar, fastener, framing connector, and continuous joint parameterizes — an item shape (the family discriminant, a sectional unit row, an installation-capacity receipt key, an appearance assignment) plus the family vocabulary its layout schedule reads; one `ConnectionFamily` `[SmartEnum<string>]` closes the family-kind axis (reinforcement · fastener · hanger · joint), all four realized as sibling catalogue vocabularies the one `ConnectionCatalogue.Build` folds — the EXACT structural mirror of the `profiles/Profile` owner (`profile#PROFILE_OWNER`). A connection is NEVER a per-item class: a #5 rebar is a `ConnectionItem` in the `reinforcement` family (`Connection/reinforcement#REINFORCEMENT_FAMILY`), a 3/8in hex bolt a `ConnectionItem` in the `fastener` family (`Connection/fastener#FASTENER_FAMILY`), a joist hanger a `ConnectionItem` in the `hanger` family (`Connection/hanger#HANGER_FAMILY`), an 8 mm fillet weld a `ConnectionItem` in the `joint` family (`Connection/joint#JOINT_FAMILY`), differing only in section columns and the family discriminant — never a `Rebar`/`Bolt`/`Hanger`/`Weld` type. The axis is CLOSED at FOUR families: `anchor` folds as a `FastenerKind` arm inside the fastener vocabulary, never a fifth sibling family; `joint` is the ONE deliberate widening past the discrete reinforcement/fastener/hanger triple, the continuous weld/adhesive/stud carrying no thread or bar-diameter section so it cannot fold into an existing arm the way `anchor` folds into `fastener`.

A connection item IS a structural ELEMENT (an `IfcReinforcingBar`/`IfcReinforcingMesh`/`IfcMechanicalFastener`/`IfcFastener`), so it reaches `Rasm.Bim` the SAME way every material does — projected onto the shared `Rasm.Element` seam graph, NEVER a second wire carrier crossing the boundary. The `ConnectionProjector` is the connection analog of the `Projection/material#MATERIAL_PROJECTOR` `MaterialProjector`: a second `Rasm.Materials` `IElementProjection` that lowers a connection schedule into a `GraphDelta` of content-keyed `Rasm_ConnectionRealization` `PropertySet` detail-bag nodes (one per occurrence) plus the `Assign.PropertyDefinition` edge binding each bag onto its already-minted, `ctx.ElementIds`-vouched connection element — the IDENTICAL bag shape the `Rasm.Bim` `Semantics/connection#CONNECTION_DETAIL` `ConnectionProjection.Detail` reads back at IFC ingress and the `Projection/semantic#IFC_EGRESS` `Emit` round-trips, so an authored connection and an imported one are one node family on the one graph. Like the `MaterialProjector`, it authors NO `Object` node and mints NO element identity (the IFC class — `IfcReinforcingBar`/`IfcMechanicalFastener`/`IfcFastener` — rides the `ConnectionItem.IfcEntity` projection, the predefined token the `PredefinedToken` projection, both stamped by whoever minted the element `Object`: Bim at ingest or the app from-scratch). The prior `ConnectionItemWire`/`ConnectionWire` second wire is RETIRED, the deleted form the `Rasm.Bim` consumer already names (`Semantics/connection#CONNECTION_DETAIL` EGRESS_AND_WIRE_RETIREMENT), mirroring the `MaterialAssignmentWire`/`MaterialPropertyWire` retirement; a connection's MATERIAL binding (its steel grade, capacity, embodied carbon, appearance) rides the `MaterialProjector`'s `Associate` edge onto the connection element NodeId [H12], never a column on the connection bag.

The OOP capsule lives at the boundary (the `[ValueObject]` `ConnectionId` key, the `[ValidationError]`-derived `ConnectionFault` band), the FP-ROP rail owns the internals (the `Fin<T>` admission, the `Seq`/`Fold` catalogue projection, the projector's `Fin<GraphDelta>`). The page composes the `Rasm` kernel for the `PositiveMagnitude`/`Dimension` section value-objects exactly as `profile#PROFILE_OWNER` does, the `Appearance/graph#MATERIAL_LIBRARY` `MaterialId` appearance column (`metal.iron`/`metal.steel`, the `Profile.AppearanceId` mirror), the `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` capacity receipt key (read by `MaterialId`, never re-derived here), `Construction/layout#ASSEMBLY_FOLD` for the station-stepped placement a rebar schedule or fastener pattern reads over the SAME realized `Resolve` fold, and the `Rasm.Element` seam contracts (`IElementProjection`/`ProjectionContext`/`GraphDelta`/`Node`/`NodeId`/`Classification`/`PredefinedType`/`PropertyBag`/`PropertyValue`/`MeasureValue`/`Relationship`) the projector lowers onto — re-minting no seam type and authoring no host or IFC entity. Each family vocabulary lives on its own sibling page (`reinforcement`/`fastener`/`hanger`/`joint`); this page owns the one `ConnectionItem` shape, the closed family axis, the `ConnectionId` key, the band-2360 `ConnectionFault`, the `ConnectionCatalogue` registered-row table, and the one `ConnectionProjector`.

## [01]-[INDEX]

- [01]-[CONNECTION_OWNER]: the one `ConnectionItem` `[Union]` over the closed `ConnectionFamily` `[SmartEnum<string>]` axis (reinforcement/fastener/hanger/joint), the `ConnectionId` `[ValueObject]` key with `ComparerAccessors.StringOrdinal` accessor, the band-2360 `ConnectionFault` `[Union]`, the `ConnectionItem.Of` polymorphic admission entry, the `ConnectionSection.IfcEntity`/`PredefinedToken` egress projections, and the context-folded `ConnectionCatalogue` registered-row table over all four family builders.
- [02]-[CONNECTION_PROJECTOR]: the `ConnectionProjector` `IElementProjection` (the connection analog of the `Projection/material#MATERIAL_PROJECTOR`) lowering a `Seq<ConnectionSpec>` connection schedule into a `GraphDelta` of content-keyed `Rasm_ConnectionRealization` `PropertySet` detail-bag nodes (the `Rasm.Bim` `Semantics/connection#CONNECTION_DETAIL` reads/round-trips the IDENTICAL shape) + the `Assign.PropertyDefinition` edge binding each bag onto its `ctx.ElementIds`-vouched element [H12]; it authors NO `Object` node (the connection element's identity is its MINTER's — Bim at ingest or the app from-scratch, exactly as the `MaterialProjector` authors no element), the prior `ConnectionItemWire`/`ConnectionWire` second wire RETIRED, a connection's material binding riding the `MaterialProjector`'s `Associate` edge.

## [02]-[CONNECTION_OWNER]

- Owner: `ConnectionItem` over the closed `ConnectionFamily` axis; `ConnectionId` key; `ConnectionFault` `[Union]` band 2360; `ComparerAccessors.StringOrdinal` accessor; `ConnectionCatalogue` the registered-row table; the `ConnectionSection` `[Union]` carrying the per-family columns plus the `IfcEntity`/`PredefinedToken` egress projections the `ConnectionProjector` reads.
- Cases: one `ConnectionItem` shape across all families — `Family` (the discriminant), `Designation` (the `ConnectionId` key, `connection.<designation>`), `Section` (the family-projected sectional unit row as a `ConnectionSection` `[Union]` arm: `Reinforcement`/`Fastener`/`Hanger`/`Joint`), `CapacityKey` (the `MaterialId` whose `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` row carries the installation capacity the design seam reads), `AppearanceId` (the `graph#MATERIAL_LIBRARY` `MaterialId` row the appearance projection reads); the two `MaterialId` slots are INDEPENDENT — a galvanized fastener whose capacity steel and finish coincide on `metal.steel` is the common case, but a coated rebar (capacity `metal.steel`, appearance an epoxy-coat row) keeps them distinct, so neither column derives from the other; family {reinforcement, fastener, hanger, joint} closed at four (`anchor` is a `FastenerKind` arm inside the fastener vocabulary, never a family; `joint` is the deliberate fourth case carrying the continuous weld/adhesive/stud at `Connection/joint#JOINT_FAMILY`); an item is a `ConnectionFamily` ROW, never a connection subtype.
- Entry: `public static Fin<ConnectionItem> Of(ConnectionFamily family, string designation, ConnectionSection section, MaterialId capacityKey, MaterialId appearanceId, Op key)` — one polymorphic admission entry, never a `GetById`/`GetByFamily` family (the `Profile.Of` law); `Fin<T>` aborts on a malformed designation (`ConnectionFault.Designation`, key-correlated), a family/section discriminant mismatch (`ConnectionFault.Family`), or a non-positive nominal column (`ConnectionFault.Capacity`); `ConnectionCatalogue.Build(context)` folds every family's row builder (`reinforcement#REINFORCEMENT_FAMILY` `BuildRebarRows` plus `fastener#FASTENER_FAMILY` `BuildFastenerRows` plus `hanger#HANGER_FAMILY` `BuildHangerRows` plus `joint#JOINT_FAMILY` `BuildJointRows`) into the one frozen registry, `ConnectionCatalogue.Lookup(rows, id, key)` resolves a registered `ConnectionId` to its catalogue `ConnectionItem`, and the same `Of` admits an ad-hoc item through the row validation a registered row passes — one polymorphic entry.
- Packages: Rasm (project — `PositiveMagnitude`/`Dimension`/`UnitInterval` value-objects the family sections compose), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`/`[ValueObject]` generators at their deepest surface — generated total `Switch`, `[KeyMemberEqualityComparer]`/`[KeyMemberComparer]`), LanguageExt.Core (`Fin`/`Seq`/`Fold` for the admission rail and the catalogue fold), BCL inbox (`FrozenDictionary`); no new external package — the connection owner is the structural copy of the proven `profile#PROFILE_OWNER` shape, never a re-invention.
- Growth: a new structural connection is one `ConnectionItem` row in the matching `ConnectionFamily`; a new family is one `ConnectionFamily` case carrying its section vocabulary on its own sibling page folded into `ConnectionCatalogue.Build`; a new fault is one `ConnectionFault` case — never a `Rebar`/`Bolt`/`Hanger`/`Weld` type, never a per-family `ConnectionItem` variant. All four families are realized, each its own `ConnectionFamily` case with its own `ConnectionSection` arm and `BuildXRows` catalogue builder: `reinforcement` carries the `RebarGrade`/`BarSize` axis and the ASTM A615/A706 bar-diameter/area/unit-weight rows plus the ACI 318 bend/hook scalar tuple, `fastener` the `FastenerKind` (bolt/nut/screw/anchor) discriminant and the ISO 898-1/SAE J429 `FastenerGrade` proof/tensile axis over the thread/pitch/shank/head columns, `hanger` the `HangerType` framing-connector discriminant over the carried-member/download-uplift/gauge columns, `joint` the `JointKind` (weld/adhesive/stud) discriminant and the AWS D1.1 / structural-adhesive throat/bond-line/stud-diameter columns over the continuous-connection receipt — a new bar size, bolt grade, hanger, or electrode is a row data addition, never a new surface. The axis is closed at four: `joint` is the deliberate continuous-connection widening, the metallurgical/adhesive complement to the discrete items, justified because a weld/bond/stud has no `NominalMm` thread or bar-diameter and the `STEEL_COMPOSITE_AND_COLDFORMED` composite leg reads the `joint#JOINT_FAMILY` `StudClass` vocabulary.
- Boundary: `ConnectionItem` is the ONE structural-connection concept — a per-item class is the deleted form; the `ConnectionSection` `[Union]` (`Reinforcement`/`Fastener`/`Hanger`/`Joint`) carries each family's projected columns so the one item shape never branches into per-family types, and every length column composes the `Rasm` kernel `PositiveMagnitude` value-object (the double-backed `> 0` finite magnitude) exactly as `ProfileUnit` does so a connection never re-mints a dimension primitive and a fractional bar diameter — an 11.3 mm #4 bar, a 9.525 mm 3/8in bolt — admits without the truncation an int-backed `Dimension` count would force, the `Dimension` carrier reserved for discrete counts (bar layers, thread starts); `ConnectionFault` is the one fault every `Fin.Fail` reads (designation/grade/capacity/family slots), an `Expected`-derived `Error` (`IValidationError<ConnectionFault>`) whose 2360 band IS the `Expected` `Code` so a bare typed case lifts directly into the `Fin<T>` rail (the `ProfileFault` law), disjoint from `ProfileFault` 2300 / `ConstructionFault` 2350 / kernel `GeometryFault` 2400 / `MaterialFault` 2450 / `ProjectionFault` 2470, so a connection schedule never throws and never returns a sentinel; the `[ValueObject]` `ConnectionId` key and the `[ValidationError]`-derived `ConnectionFault` are the OOP capsule at the edge, the `Fin`/`Seq`/`Fold` catalogue projection is the FP-ROP internal; the capacity receipt is NOT re-derived here — `CapacityKey` is the `MaterialId` whose `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` row (`DensityKgM3`/`YoungsModulusMpa`/`YieldStrengthMpa`/`PoissonsRatio`/`ThermalExpansionPerK`) the structural-connection-design seam reads by key, so a bolt's proof load and a bar's yield are read once from the property library, never duplicated as a connection column; the appearance assignment crosses to `Appearance/graph#MATERIAL_LIBRARY` as a `MaterialId` row a `ConnectionItem.AppearanceId` column carries (the `Profile.AppearanceId` mirror), never a connection-specific surface; the placement that turns a `ConnectionItem` schedule into a station-stepped stream is `Construction/layout#ASSEMBLY_FOLD` `Resolve`/`StationStep`, composed not re-derived here — a rebar schedule and a fastener pattern are station-stepped placements over the SAME fold, never a parallel `connection/layout` owner; the bend/hook geometry of a reinforcement bar is a scalar bend-angle/radius/hook-extension tuple the host materializes (`Connection/reinforcement#REINFORCEMENT_FAMILY`), NEVER a host curve here (the host-neutral scalar-`Placement` discipline); the family vocabularies (`RebarGrade`/`BarSize`/`RebarSection`, `FastenerKind`/`FastenerGrade`/`FastenerSection`, `HangerType`/`HangerSection`, `JointKind`/`WeldType`/`ElectrodeClass`/`AdhesiveClass`/`StudClass`/`JointSection`) live on their sibling pages, and the `ConnectionCatalogue.Build` registered-row table folds every family's `BuildXRows` into one frozen registry so a registered row keys the same way as each family lands its own builder; the IFC class and the predefined token a connection item carries (read by whoever authors the connection element's `Object`) are the `ConnectionSection.IfcEntity`/`PredefinedToken` projections (`IfcReinforcingBar` for `reinforcement`, `IfcMechanicalFastener` for the discrete `fastener` and the welded `stud`, the `IfcDiscreteAccessory` the fabricated framing connector physically IS for `hanger`, `IfcFastener` for a `weld`/`adhesive`) each DELEGATING to its family vocabulary field (`FastenerKind.IfcPredefinedType`, `HangerType.IfcAccessoryType`, `JointKind.IfcPredefinedType`) — never a host-side `IfcOpenShell` evaluation and never a second wire carrier here.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Globalization;          // CultureInfo.InvariantCulture (the ConnectionId.Validate format provider)
using LanguageExt;
using Rasm.Vectors;                  // PositiveMagnitude (>0 finite magnitude), Dimension (>=1 discrete count) — the kernel value-object atoms live in Rasm.Vectors, NOT Rasm.Domain
using Rasm.Domain;                   // Op (the boundary-admission key), the AcceptValidated admission extension, Context
using Rasm.Element;                  // MaterialId (the seam-carried material identity the ConnectionItem AppearanceId/CapacityKey reference)
using Thinktecture;
using Expected = Rasm.Domain.Expected;   // the kernel Expected (parameterless ctor + virtual Category), NOT LanguageExt.Common.Expected
using static LanguageExt.Prelude;

// connection#CONNECTION_OWNER is the PARENT Rasm.Materials.Connection — it owns ConnectionId/ConnectionFamily/
// ConnectionFault/ConnectionSection/ConnectionItem/ConnectionProjector and folds each family's own catalogue by the
// sub-namespace-qualified name (Reinforcement./Fastener./Hanger./Joint.ConnectionCatalogue, each its own
// Rasm.Materials.Connection.<Family> so the four sibling `ConnectionCatalogue` static classes are distinct types, the
// child leaf visible from the parent without a using — the EXACT mirror of the profiles/Profile owner namespace law).
namespace Rasm.Materials.Connection;

// --- [TYPES] -------------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct ConnectionId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
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
// The connection-sub-domain fault band (2360): Expected-derived over the kernel Rasm.Domain.Expected so band 2360 IS
// the Expected Code and a typed case lifts BARE onto Fin<T>/Validation<Error,T> (no .ToError() hop). The kernel base
// ctor is PARAMETERLESS (Code a virtual Error member, Message abstract, Category virtual) — so band 2360 is a
// `Code => 2360` override and `Message => Detail`, and the per-case Category override drives
// FaultExtensions.Category(error); the legacy `base(detail, 2360, None)` form targeted the OTHER
// LanguageExt.Common.Expected (no Category to override) and was the defect. [SkipUnionOps] skips the generated
// implicit-conversion ops (every case carries an explicit Op) and emits NO per-case factory, so the band declares its
// own (the production UiFault / seam ElementFault shape): a nested `…Case` record carries the data and a same-name-less
// static factory ConnectionFault.Grade(key, detail) returns the Expected-derived base so the case lifts BARE onto
// Fin<T>/Validation<Error,T> with no `new` and no .ToError() hop — the `…Case` suffix frees the unsuffixed factory name
// (a same-named nested type + method is CS0102). Create routes the unspecific case under a boundary-admission Op.
[SkipUnionOps]
[Union]
public abstract partial record ConnectionFault : Expected, IValidationError<ConnectionFault> {
    private ConnectionFault(Op key, string detail) { Key = key; Detail = detail; }
    public Op Key { get; }
    public string Detail { get; }
    public override int Code => 2360;
    public override string Message => Detail;
    private static readonly Op Admission = Op.Of(name: nameof(Admission));

    public sealed record DesignationCase(Op Key, string Detail) : ConnectionFault(Key, Detail) { public override string Category => "Designation"; }
    public sealed record GradeCase(Op Key, string Detail) : ConnectionFault(Key, Detail) { public override string Category => "Grade"; }
    public sealed record CapacityCase(Op Key, string Detail) : ConnectionFault(Key, Detail) { public override string Category => "Capacity"; }
    public sealed record FamilyCase(Op Key, string Detail) : ConnectionFault(Key, Detail) { public override string Category => "Family"; }

    public static ConnectionFault Designation(Op key, string detail) => new DesignationCase(key, detail);
    public static ConnectionFault Grade(Op key, string detail) => new GradeCase(key, detail);
    public static ConnectionFault Capacity(Op key, string detail) => new CapacityCase(key, detail);
    public static ConnectionFault Family(Op key, string detail) => new FamilyCase(key, detail);
    public static ConnectionFault Create(string message) => Family(Admission, message);
}

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

    // The neutral IFC entity class a connection item carries onto the seam Object node's Classification.Code (system
    // "ifc") — a reinforcing bar is IfcReinforcingBar, a discrete bolt/screw/anchor AND a welded shear stud an
    // IfcMechanicalFastener, a fabricated framing connector (joist-hanger saddle / framing-angle / strap / hold-down) the
    // IfcDiscreteAccessory it physically IS (Connection/hanger#IFC_DISCRETE_ACCESSORY_WIRE — the connector is the
    // fabricated cold-formed plate, the attaching nails/bolts a SEPARATE IfcMechanicalFastener the Bim egress relates),
    // a continuous weld/adhesive bead the non-mechanical IfcFastener (GG api-geometrygym-ifc: IfcReinforcingBar :
    // IfcReinforcingElement, IfcMechanicalFastener / IfcFastener : IfcElementComponent; IfcDiscreteAccessory :
    // IfcElementComponent). The egress is one Switch over the closed family — no per-family Object subtype, the Rasm.Bim
    // Emit resolving the IfcClass row from this code + re-stamping PredefinedToken through the C6 AdmitPredefined gate.
    public string IfcEntity => Switch(
        reinforcement: static _ => "IfcReinforcingBar",
        fastener:      static _ => "IfcMechanicalFastener",
        hanger:        static _ => "IfcDiscreteAccessory",
        joint:         static j => j.Continuous.Kind == JointKind.Stud ? "IfcMechanicalFastener" : "IfcFastener");

    // The verified PredefinedType token the seam Object node carries (the C6 typed value the Bim egress validates against
    // the frozen IfcClass valid-set), each family READING its own vocabulary field, never re-hardcoding it here. The
    // fastener token is the FastenerKind.IfcPredefinedType (BOLT/SCREW/ANCHORBOLT/DOWEL/RIVET/USERDEFINED, the IfcMechanicalFastenerTypeEnum members; the kind axis bolt/nut/screw/anchor/dowel/rivet/coupler maps nut+coupler to USERDEFINED since the enum has NO NUT member),
    // the joint token the JointKind.IfcPredefinedType (the welded stud the IfcMechanicalFastenerTypeEnum.STUDSHEARCONNECTOR
    // — the only joint member on the fastener enum, STUD is the IfcReinforcingBarTypeEnum cast-in bar, NOT a fastener value
    // — a weld bead the IfcFastenerTypeEnum WELD, an adhesive bead GLUE), the framing connector the
    // HangerType.IfcAccessoryType (the IfcDiscreteAccessoryTypeEnum SHOE/BRACKET/ANCHORPLATE the connector physically IS,
    // Connection/hanger#IFC_DISCRETE_ACCESSORY_WIRE — NOT the IfcMechanicalFastenerTypeEnum NAILPLATE/SHEARCONNECTOR a
    // hardcoded arm wrongly read, which are fastener-enum members the discrete-accessory entity cannot carry; the attaching
    // nailplate/nails ride the SEPARATE IfcMechanicalFastener the Bim egress relates through IfcRelConnectsWithRealizingElements,
    // its token the HangerType.IfcFastenerType the hanger detail bag carries); a reinforcing bar's MAIN role is the
    // catalogue default (the IfcReinforcingBarTypeEnum member, refined to LIGATURE/STUD/SHEAR/… when the reinforcement
    // vocabulary grows a role column). Every arm DELEGATES to its family vocabulary field — no hardcoded token here.
    public string PredefinedToken => Switch(
        reinforcement: static _ => "MAIN",
        fastener:      static f => f.Bolt.Kind.IfcPredefinedType,
        hanger:        static h => h.Connector.Type.IfcAccessoryType,
        joint:         static j => j.Continuous.Kind.IfcPredefinedType);
}

public sealed record ConnectionItem(
    ConnectionFamily Family,
    ConnectionId Designation,
    ConnectionSection Section,
    MaterialId CapacityKey,
    MaterialId AppearanceId) {

    // The seam IFC class + predefined token a connection occurrence projects onto its Object node — read straight off the
    // ConnectionSection arm (the section discriminant owns both), so the projector never re-derives the entity per family.
    public string IfcEntity => Section.IfcEntity;
    public string PredefinedToken => Section.PredefinedToken;

    public static Fin<ConnectionItem> Of(ConnectionFamily family, string designation, ConnectionSection section, MaterialId capacityKey, MaterialId appearanceId, Op key) =>
        from id in ConnectionId.Validate(designation, CultureInfo.InvariantCulture, out ConnectionId? built) is { } error
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

## [03]-[CONNECTION_PROJECTOR]

- Owner: `ConnectionProjector` the sealed `IElementProjection` (the connection analog of the `Projection/material#MATERIAL_PROJECTOR` `MaterialProjector`); `ConnectionSpec` the one-occurrence projection unit (a context-vouched element `NodeId` paired with a catalogue/admitted `ConnectionItem`); `ConnectionSchedule` the captured-source aggregate the projector folds; the `ConnectionSet` detail-bag name + the `Joint`/`Token`/`Measured`/`Rows` bag-row constructors mirroring the `Rasm.Bim` `Semantics/connection#CONNECTION_DETAIL` reader so an AUTHORED connection and an IMPORTED one are one node family on the one graph. The projector rails the shared Materials-projection `Projection/material#MATERIAL_PROJECTOR` `ProjectionFault` band 2470 (NOT a parallel band), reusing the `Unvouched` arm the `MaterialProjector` uses.
- Cases: one `ConnectionProjector` shape over a `Seq<ConnectionSpec>` — each `ConnectionSpec` is a `NodeId Element` (the rooted connection occurrence id the app/`Rasm.Bim` minted and seeded into `ctx.ElementIds`, §4C — the projector NEVER mints an element identity) plus a `ConnectionItem` (the catalogue or `Of`-admitted item carrying its `IfcEntity`/`PredefinedToken`/`Section`); the projector is NEVER a per-family projector type, never a `RebarProjector`/`FastenerProjector` sibling — one projector folds the whole connection schedule, exactly as one `MaterialProjector` folds the whole material subgraph.
- Entry: `public Fin<GraphDelta> Project(ProjectionContext ctx)` — the ONE seam contract op, `schedule.Specs.TraverseM(spec => ProjectConnection(spec, ctx))` accumulating each spec's OWN delta (a content-keyed `Rasm_ConnectionRealization` `PropertySet` detail bag + the binding `Assign.PropertyDefinition` edge onto the vouched element) then folding them through the cancellation-correct `Graph/delta#GRAPH_DELTA` `GraphDelta.Merge` MONOID — the EXACT `Projection/material#MATERIAL_PROJECTOR` / `Projection/projection#PROJECTION_CONTRACT` `Assemble` shape (`TraverseM` → `Merge`-fold), never a hand-threaded accumulator; `Fin<T>` aborting on a binding to an element the context does not vouch for (`ProjectionFault.Unvouched`, the §4C/[H12] gate), an EMPTY `ctx.ElementIds` authoring the bag node alone with no edge (the pure-isolation path, never a fault); `ConnectionProjector.Of(ConnectionSchedule schedule)` captures the schedule once, and the seam's `Projection/projection#PROJECTION_CONTRACT` `ProjectionAssembly.Assemble(projectors, constraints, seed, ctx)` folds this projector's delta with the `MaterialProjector` delta (and `Rasm.Bim`'s `SemanticProjector`) into one `ElementGraph` — adding the connection projector is one registration row at the app composition root, never a seam edit; `ConnectionSchedule.Capture(items, elementOf)` pairs each registered `ConnectionItem` with the rooted element NodeId the app/`Rasm.Bim` minted for it (an INFALLIBLE capture — `elementOf` is a total `Func<ConnectionItem, NodeId>`, so unlike the `MaterialProjector` source's `Fin`-returning capture, the connection schedule derives no fallible usage and threads no `Op key`), the same `Capture`-then-`Project` shape the `MaterialProjector` source uses.
- Packages: Rasm.Element (project — `IElementProjection`/`ProjectionContext`/`GraphDelta`/`Node`/`NodeId`/`PropertyBag`/`PropertyName`/`PropertyValue`/`MeasureValue`/`Dimension`/`InheritanceMode`/`Relationship`/`AssignKind`, the seam this folder projects onto — NOT `Object`/`Classification`/`PredefinedType`/`SchemaSpan`/`RepresentationContentHash`, the element-minter's surface this projector never touches), Rasm (project — `Op` the fault-correlation key, the seed-zero `XxHash128` content seed the seam `NodeId.Content` composes), Thinktecture.Runtime.Extensions (the `ConnectionSection`/`JointSection` generated `Switch` the `JointType` detail projection reads), LanguageExt.Core (`Fin`/`Seq`/`TraverseM`/`Fold`/`Bind`/`Map` for the projection rail); no new external package — the projector composes the realized owner and the seam contracts, the `Projection/material#MATERIAL_PROJECTOR` shape its template.
- Growth: a new connection family is one `ConnectionSection` arm the `IfcEntity`/`PredefinedToken`/`JointType` projections already dispatch over (the projector reads them, no projector arm); a new detail row is one `Measured`/`Token` row on the family's `Detail` fold; a new occurrence binding into a vouched element is one more `ConnectionSpec`; a new projection fault routes the shared `ProjectionFault` band — never a second projector surface, never a per-family projector, never a `ConnectionItemWire` carrier beside the graph. The connection projector is the ONE registration row the §4D/§6 "a future `Connection` projection (the `MaterialProjector` sibling)" anticipated, now realized.
- Boundary: `ConnectionProjector` lowers a connection schedule onto the seam `ElementGraph` EXACTLY as the `MaterialProjector` lowers the material subgraph — a `ConnectionItemWire`/`ConnectionWire` second wire crossing the `Rasm.Bim` boundary is the deleted form the `Rasm.Bim` `Semantics/connection#CONNECTION_DETAIL` already names (mirroring the `MaterialAssignmentWire`/`MaterialPropertyWire` retirement), so a connection reaches `Rasm.Bim` ONLY as seam nodes the `Bake` fold reads, never a parallel carrier; the projector NEVER mints an element identity AND NEVER authors the connection element's `Object` node — each `ConnectionSpec.Element` is a context-vouched rooted `NodeId` the app/`Rasm.Bim` minted (§4C), so a PRIMARY projector (Bim at IFC ingest, or the app from-scratch through `NodeId.Rooted()`) already authored the `Object` node — stamping its `ObjectKind.Occurrence`, the generic `Classification("ifc", item.IfcEntity)` (the IFC entity class the seam carries as a neutral code, the `IfcClass` roster `Rasm.Bim`'s egress concern), the typed `PredefinedType.Create(item.PredefinedToken)` (the C6 token the Bim `Emit` validates against the frozen valid-set), the designation as `Tag`, and `RepresentationContentHash.Empty` (the connection geometry host-materialized from the scalar section, content-keyed at the GLB wire, never a node coordinate field) off THIS owner's `ConnectionItem` egress projections — and a Materials-stratum `ConnectionProjector` that ALSO `Put` a `Node.Object` with that vouched id is the deleted form (it usurps the minter's element-identity authorship and collides on the duplicate node at `AdmitOnto`, exactly as the `MaterialProjector` authors NO `Object` and binds material via `Associate` onto a pre-minted element); a spec whose element is absent from `ctx.ElementIds` rails `ProjectionFault.Unvouched` and an EMPTY `ctx.ElementIds` authors the bag node alone with no binding edge (the pure-Materials isolation path, never a fault — the `MaterialProjector.AuthorBindings` empty-context law); the detail bag is the content-keyed `Rasm_ConnectionRealization` `PropertySet` node the `Rasm.Bim` `Semantics/connection#CONNECTION_DETAIL` `ConnectionProjection.Detail` reads back at IFC ingress and `Projection/semantic#IFC_EGRESS` `Emit` round-trips — the IDENTICAL set name, `OccurrenceWins` `InheritanceMode`, `JointType` allowed-set, dimension-only `MeasureValue.OfSi` SI-base columns, and shared row-name vocabulary (`JointType`/`FastenerType`/`AccessoryType`/`BarType`/`NominalDiameter`/`NominalLength`/`CrossSectionArea`/`CarriedMemberWidth`/`CarriedMemberDepth`/`EffectiveThroat`/`BondLine`/`Overlap`), so a re-imported authored connection reads through the SAME `Detail` reader and two bags carrying the same rows-and-values content-key identically (an authored bag carrying a subset of rows is a faithfully different node, never a forced byte-match); the bag is minted through the seam `NodeId.Content` over `Node.ToCanonicalBytes` (id excluded) so two structurally-identical connection details dedup to one node, NEVER a second `(GeometryKey, DetailKey)` hasher; the connection's MATERIAL binding (its steel grade, capacity `Mechanical` row, embodied carbon, classification, appearance) rides the `MaterialProjector`'s `Associate` edge onto the connection element NodeId [H12], NOT a column on this bag — a `SteelGrade`/`EmbodiedCarbon` row here is the named seam violation, the material subgraph the `MaterialProjector`'s; the joint TOPOLOGY (which members a weld/stud realizes) is the `Rasm.Bim` `Projection/semantic#RELATION_ALGEBRA` `Connect(ConnectKind.Realizing)` edge from `IfcRelConnectsWithRealizingElements`, NOT this projector's (the projector authors the realizing element + its detail; the app or Bim authors the `Connect` edge once both member ids are known); the projected `GraphDelta` is gated by the seam's `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint.Validate` ([M3], `Rasm.Bim`-implemented) for IFC-semantic legality before the seam folds it, so the projector enforces only the structural invariants it owns (content-key idempotence, context-vouched edges) and defers IFC legality to the constraint.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
// (composes the section-02 prelude — LanguageExt, Rasm.Vectors, Rasm.Domain, Thinktecture, static Prelude — plus the seam contracts;
//  the Dimension below is the SEAM Rasm.Element.Dimension physical signature (LengthDim/AreaDim), NOT the kernel Rasm.Vectors.Dimension count.)
using Rasm.Element;                  // NodeId, Node, PropertyBag, PropertyName, PropertyValue, MeasureValue, Dimension,
                                     // InheritanceMode, Relationship, AssignKind, GraphDelta, ProjectionContext, IElementProjection
                                     // (NO Object/Classification/PredefinedType/SchemaSpan/RepresentationContentHash — the
                                     //  Object node is the element-minter's, this projector authors only the detail bag + edge)

// Same parent owner namespace as the section-02 fence — ConnectionSpec/ConnectionSchedule/ConnectionProjector are
// connection#CONNECTION_OWNER types in Rasm.Materials.Connection, the partial namespace the two fences share.
namespace Rasm.Materials.Connection;

// --- [MODELS] ------------------------------------------------------------------------------
// One connection occurrence to project: the rooted element NodeId the app/Rasm.Bim minted and seeded into ctx.ElementIds
// (§4C — the projector binds to it, never invents it) paired with the catalogue/admitted ConnectionItem carrying the
// IfcEntity/PredefinedToken/Section. NO material binding here — a connection's steel grade rides the MaterialProjector's
// Associate edge onto this same Element id [H12], so the connection schedule carries only the element identity + the item.
public readonly record struct ConnectionSpec(NodeId Element, ConnectionItem Item);

// The captured projection source: the per-occurrence specs the projector folds. Captured ONCE (§4C inversion) so the
// seam op carries only the ProjectionContext; the seam ProjectionAssembly.Assemble merges this delta with every sibling.
public sealed record ConnectionSchedule(Seq<ConnectionSpec> Specs) {
    public static readonly ConnectionSchedule Empty = new(Seq<ConnectionSpec>());
    public ConnectionSchedule With(ConnectionSpec spec) => this with { Specs = Specs.Add(spec) };

    // The worked capture: pair each registered/admitted ConnectionItem with the rooted element NodeId the app minted for
    // it (elementOf), seeding the SAME element ids into ctx.ElementIds before Assemble — the MaterialProjector.Capture shape.
    public static ConnectionSchedule Capture(Seq<ConnectionItem> items, Func<ConnectionItem, NodeId> elementOf) =>
        items.Fold(Empty, (acc, item) => acc.With(new ConnectionSpec(elementOf(item), item)));
}

// --- [SERVICES] ----------------------------------------------------------------------------
// The one IElementProjection the Connection sub-domain publishes — the connection analog of the MaterialProjector.
// Captures the schedule internally (§4C) so the seam op carries only the ProjectionContext; the seam Assemble fold merges
// this delta with the MaterialProjector delta (and Rasm.Bim's SemanticProjector) into one ElementGraph.
public sealed class ConnectionProjector : IElementProjection {
    readonly ConnectionSchedule schedule;
    ConnectionProjector(ConnectionSchedule schedule) => this.schedule = schedule;
    public static ConnectionProjector Of(ConnectionSchedule schedule) => new(schedule);

    // TraverseM each spec to its OWN delta (the monadic accumulation short-circuiting on the first Unvouched spec), then
    // fold the per-spec deltas through the cancellation-correct Graph/delta#GRAPH_DELTA GraphDelta.Merge MONOID — the EXACT
    // Projection/material#MATERIAL_PROJECTOR / Projection/projection#PROJECTION_CONTRACT Assemble shape (TraverseM -> Merge-fold),
    // NEVER a hand-threaded `Fold(Fin.Succ(Empty), (acc, s) => acc.Bind(d => Project(s, d)))` accumulator (the deleted form the
    // seam doctrine names): each spec builds on GraphDelta.Empty so per-spec projection is decoupled from the running delta,
    // and Merge keeps the projector's contribution a faithful single delta the seam re-merges with every sibling.
    public Fin<GraphDelta> Project(ProjectionContext ctx) =>
        schedule.Specs.TraverseM(spec => ProjectConnection(spec, ctx)).As()
            .Map(static deltas => deltas.Fold(GraphDelta.Empty, static (acc, delta) => acc.Merge(delta)));

    // --- [SUBGRAPH_FOLD]
    // One spec -> its OWN content-addressed Rasm_ConnectionRealization PropertySet detail bag + the binding
    // Assign.PropertyDefinition edge onto the element — built on GraphDelta.Empty (the seam Merge composes the per-spec
    // deltas), never a threaded accumulator. The connection's Object node is AUTHORED BY ITS MINTER, never here: the
    // ConnectionSpec.Element is already in ctx.ElementIds, so a PRIMARY projector (Rasm.Bim at IFC ingress, or the app
    // from-scratch authoring through NodeId.Rooted() — Projection/projection#PROJECTION_CONTRACT) already authored the
    // Object node (stamping its Classification("ifc", item.IfcEntity) + PredefinedType.Create(item.PredefinedToken) off the
    // ConnectionItem's egress projections) and seeded the id; a Materials-stratum projector that ALSO Put a Node.Object with
    // that same id is the deleted form — it usurps the element-identity authorship the seam reserves for the minter and
    // collides on the duplicate node at AdmitOnto, exactly as the MaterialProjector authors NO Object and binds material
    // via Associate onto a pre-minted element. This projector authors ONLY the non-rooted content-keyed detail bag (the
    // Materials-stratum prerogative the Material/Appearance nodes share) and the PropertyDefinition edge.
    //
    // The element id is context-VOUCHED (§4C/H12) — a spec the context does not own rails Unvouched. An EMPTY ctx.ElementIds
    // (a pure-Materials connection run with no element identities yet minted) authors the bag node ALONE with NO binding
    // edge and is NOT a fault — the connection detail subgraph stands in isolation (the MaterialProjector.AuthorBindings
    // empty-context law), so Rasm.Materials authors connection details usable in isolation yet aligned-not-coupled.
    Fin<GraphDelta> ProjectConnection(ConnectionSpec spec, ProjectionContext ctx) {
        Node.PropertySet bag = Mint(new PropertyBag(ConnectionSet, Detail(spec.Item), InheritanceMode.OccurrenceWins), ctx.Header.Tolerance);
        GraphDelta withBag = GraphDelta.Empty.Put(bag);
        return ctx.ElementIds.IsEmpty
            ? Fin.Succ(withBag)
            : ctx.Owns(spec.Element)
                ? Fin.Succ(withBag.Link(new Relationship.Assign(spec.Element, bag.Id, AssignKind.PropertyDefinition)))
                : ProjectionFault.Unvouched(ctx.Key, $"<connection-element-not-in-context:{spec.Element.Value}>");
    }

    // The content-keyed seam PropertySet mint the Rasm.Bim Semantics/connection + Semantics/composition share: construct the
    // node with a discarded placeholder id, then re-key from the seam Node.ToCanonicalBytes (id excluded) so two structurally-
    // identical connection details dedup to one node — never a second (GeometryKey, DetailKey) hasher in this folder.
    static Node.PropertySet Mint(PropertyBag bag, double tolerance) {
        Node.PropertySet draft = new(NodeId.Content(ReadOnlySpan<byte>.Empty), bag);
        return draft with { Id = NodeId.Content(draft.ToCanonicalBytes(tolerance).Span) };
    }

    // --- [DETAIL]
    // The Rasm_ConnectionRealization detail bag the Rasm.Bim Semantics/connection#CONNECTION_DETAIL reader authors from
    // IMPORTED IFC and the Emit round-trips — the IDENTICAL set name, row names, and SI dimensions, so an authored connection
    // and an imported one content-key to one node. ONE polymorphic Detail discriminating the ConnectionSection arm (never a
    // RebarDetail/BoltDetail sibling family), the section's PositiveMagnitude columns coerced mm->SI-base (the IFC scalars are
    // SI-base, so the imported bag matches). The JointType modality matches the Bim JointKinds allowed set (Bolted/Welded/
    // Bonded/Bearing/Cast); the FastenerType/BarType token mirrors the Object node's PredefinedToken for the detailer's bag read,
    // EXCEPT the hanger arm — its Object node carries the IfcDiscreteAccessoryTypeEnum (AccessoryType row = PredefinedToken),
    // and the FastenerType row carries the SEPARATE attaching IfcMechanicalFastenerTypeEnum (HangerType.IfcFastenerType), the
    // two tokens distinct because a fabricated connector is an IfcDiscreteAccessory fastened BY an IfcMechanicalFastener.
    static Map<PropertyName, PropertyValue> Detail(ConnectionItem item) => item.Section.Switch(
        reinforcement: r => Rows(
            Joint("Cast"),
            Token("BarType", item.PredefinedToken),
            Measured("NominalDiameter", Dimension.LengthDim, r.Bar.DiameterMm.Value * 1e-3),
            Measured("CrossSectionArea", Dimension.AreaDim, r.Bar.NominalAreaMm2.Value * 1e-6)),
        fastener: f => Rows(
            Joint("Bolted"),
            Token("FastenerType", item.PredefinedToken),
            Measured("NominalDiameter", Dimension.LengthDim, f.Bolt.ThreadDiameterMm.Value * 1e-3),
            Measured("NominalLength", Dimension.LengthDim, f.Bolt.ShankLengthMm.Value * 1e-3)),
        hanger: h => Rows(
            Joint("Bolted"),
            Token("AccessoryType", item.PredefinedToken),                       // the IfcDiscreteAccessoryTypeEnum the connector IS (= HangerType.IfcAccessoryType)
            Token("FastenerType", h.Connector.Type.IfcFastenerType),            // the SEPARATE attaching IfcMechanicalFastenerTypeEnum (NAILPLATE/NAIL/SCREW/BOLT) the Bim egress relates
            Measured("CarriedMemberWidth", Dimension.LengthDim, h.Connector.CarriedMemberWidthMm.Value * 1e-3),
            Measured("CarriedMemberDepth", Dimension.LengthDim, h.Connector.CarriedMemberDepthMm.Value * 1e-3)),
        joint: j => j.Continuous.Switch(
            weld: w => Rows(
                Joint("Welded"),
                Token("FastenerType", item.PredefinedToken),
                Measured("EffectiveThroat", Dimension.LengthDim, j.Continuous.NominalMm.Value * 1e-3),
                Measured("NominalLength", Dimension.LengthDim, w.LengthMm.Value * 1e-3)),
            adhesive: a => Rows(
                Joint("Bonded"),
                Token("FastenerType", item.PredefinedToken),
                Measured("BondLine", Dimension.LengthDim, j.Continuous.NominalMm.Value * 1e-3),
                Measured("Overlap", Dimension.LengthDim, a.OverlapMm.Value * 1e-3)),
            stud: s => Rows(
                Joint("Welded"),
                Token("FastenerType", item.PredefinedToken),
                Measured("NominalDiameter", Dimension.LengthDim, j.Continuous.NominalMm.Value * 1e-3),
                Measured("NominalLength", Dimension.LengthDim, s.HeightMm.Value * 1e-3))));

    // --- [ROWS]
    // The bag-row constructors (the Rasm.Bim Semantics/connection#CONNECTION_DETAIL row vocabulary) so each arm is a flat
    // declarative list, never a repeated MeasureValue.OfSi construction. Rows folds the rows into the one bag (AddOrUpdate
    // last-write-wins); the JointType allowed set is the five Bim modalities the egress facet validates against. The Measured
    // SI value carries the DIMENSION-only QuantityType (the dimension-only OfSi overload Bim uses) so an authored and an
    // imported NominalDiameter content-key identically. The material grade rides the seam Material subgraph, never a row here.
    static readonly Seq<string> JointKinds = Seq("Bolted", "Welded", "Bonded", "Bearing", "Cast");
    static (PropertyName, PropertyValue) Joint(string kind) => (PropertyName.Create("JointType"), new PropertyValue.Enumerated(Seq(kind), JointKinds));
    static (PropertyName, PropertyValue) Token(string name, string value) => (PropertyName.Create(name), new PropertyValue.Text(value));
    static (PropertyName, PropertyValue) Measured(string name, Dimension dim, double si) => (PropertyName.Create(name), new PropertyValue.Measure(MeasureValue.OfSi(dim, si)));

    static Map<PropertyName, PropertyValue> Rows(params (PropertyName Name, PropertyValue Value)[] rows) =>
        rows.ToSeq().Fold(Map<PropertyName, PropertyValue>(), static (bag, r) => bag.AddOrUpdate(r.Name, r.Value));

    const string ConnectionSet = "Rasm_ConnectionRealization";
}
```

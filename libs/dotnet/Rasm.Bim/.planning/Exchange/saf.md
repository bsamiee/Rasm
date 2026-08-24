# [SAF_EXCHANGE]

`Rasm.Bim` owns the SAF (Structural Analysis Format) XLSX interchange codec the `Exchange/format#FORMAT_AXIS` `saf` row's `CanImport`/`CanExport` capabilities stand on: `SafCodec` validates and executes both workbook directions, lowers the seam `Graph/element#ELEMENT_GRAPH` `ElementGraph` onto an `ExcelModel`, and realizes an import by AUTHORING the GeometryGym structural-analysis entities the ONE `Projection/semantic#SEMANTIC_PROJECTOR` then ingests. SAF is an ANALYTICAL exchange, so its member spine is the physical-to-analytical `CorrespondenceRow` set this page folds off the seam Generic edges, and the DOF verdicts the `Model/structural#STRUCTURAL_PROJECTION` reader stamped cross as the SAF constraint pair. The workbook's own information sheet is authored here too — the seam STEP header's provenance beside the `ExcelNationalCode` design code, elected off the `Model/eurocode#EUROCODE_ALGEBRA` `AnnexRegime` row whose KEY IS the SAF member name, so the design-code correspondence derives from that roster rather than standing a second table at the wire.

Both legs read the reader's vocabulary rather than re-spelling it: the restraint families, the `LoadFamily` roster the action lowering dispatches on, and the `StructuralRows` axis families are the structural owner's. Foreign cells cross ONE `SafCell` admission that accumulates every missing column, the flat transcription rides ONE Mapperly seam, refusals rail `Model/faults#FAULT_BAND` `BimFault`, and the authoring residue accumulates on the `Projection/fidelity#FIDELITY_LEDGER` `WriterT<FidelityLog, Fin, A>` carrier.

## [01]-[INDEX]

- [02]-[SAF_EXCHANGE]: `SafOp` the direction carrier, `StructuralCorrespondence` with its `CorrespondenceRole` and `CurveVariety`/`SafCaseType` correspondences, `CorrespondenceRow`/`CorrespondenceJoint` with the `JointEnd` discriminant, `SafCell` the foreign-cell admission, the `Saf*` carriers with `SafInformation`, `SafSeam` the Mapperly transcription, and `SafCodec` — `Run`, `Correspondence`, `Workbook` under its stated `AnnexRegime` code, `Author`.

## [02]-[SAF_EXCHANGE]

- Owner: `SafCodec` the SAF interchange codec — the ONLY page authoring SAF-sourced GeometryGym entities and the ONLY lowering of the seam graph onto `ExcelModel`; `StructuralCorrespondence` the closed `[SmartEnum<string>]` roster keyed on the analytical entity name, binding the physical member classes to their idealized counterparts with the schema-derived variety sets and, on a connection row, the `IfcBoundaryCondition` family its restraint lowers through; `CorrespondenceRow`/`CorrespondenceJoint` the typed member-and-joint spine both legs consume; `SafCell` the ONE foreign-cell admission; `SafInformation` the project-information row imaging the seam STEP header's provenance onto SAF's own project cells beside the elected design code; `SafSeam` the ONE `[Mapper]` owning the flat carrier-to-worksheet transcription.
- Law: SAF import AUTHORS GeometryGym entities and re-enters through the ONE `SemanticProjector`, so a SAF-side projector minting seam member nodes is the deleted standalone form; `Exchange/import#IMPORT_RAIL` performs that re-entry off the returned database.
- Entry: `SafCodec.Run(SafOp, IExcelImportService, IExcelExportService, IExcelValidator, Op)` validates and executes both directions over `ExcelModel.Objects`; the source version derives from `ExcelModel.OriginalVersion`, which the import service alone assigns, so a GRAPH-authored export model coalesces onto the target version rather than handing the validator a null `Version` — the ONE lawful coalesce on this page, resolved once at the entry. `SafCodec.Correspondence(ElementGraph, Op)` yields the typed member-and-joint rows; `SafCodec.Workbook(ElementGraph, GeometrySource, Option<AnnexRegime>, Op)` lowers the graph onto the `ExcelModel` the export leg writes, geometry crossing ONLY through the seam `GeometrySource` port and the design code electing off the stated regime row — REQUIRED and undefaulted exactly as the eurocode policy is, `None` writing no code cell rather than a fabricated `EC_Standard_EN`; `SafCodec.Author(DatabaseIfc, IfcSpatialElement, ExcelModel, Op)` returns `WriterT<FidelityLog, Fin, Unit>`, the residue riding the ledger rather than a bare string run a caller can drop.
- Auto: every dimensioned cell the authoring needs crosses `SafCell` ONCE into an admitted carrier, and the columns of one row ACCUMULATE — a point whose X and Z are unset names both. That admission is what deletes the fabricated measurement: an absent SAF coordinate no longer becomes a point at the origin, an absent thickness a zero-metre plate, an absent factor a unit multiplier, or an absent reference the empty lookup key that silently misses. `GuidOf` keeps `Guid.Empty` on a non-hex identity because SAF references are NAME-strung, so no cross-reference rides the Guid.
- Receipt: the authoring's `FidelityLog` names every SAF payload this codec carries no IFC counterpart for — the surface-connection subsoil and point-support-deformation rows (GeometryGym's face condition and displacement components are sealed internal fields with no public authoring path), the rigid-link/member/cross relations (no IFC counterpart entity), the SAF result tables, and every directional or non-linear constraint DEGRADED to its linear base, one fact per occurrence with the row's own name as the anchor.
- Packages: StructuralAnalysisFormat, GeometryGymIFC_Core, Riok.Mapperly, UnitsNet, Rasm.Element, Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new SAF worksheet is one admitted carrier, one `SafSeam` mapping, and one arm on the `Workbook` and `Author` folds beside the roster row that classifies it; a new national design regime is one `Model/eurocode#EUROCODE_ALGEBRA` `AnnexRegime` row and reaches this page as data, the import-side inverse costing no surface at all because `AnnexRegime.TryGet(code.ToString())` recovers the row from an imported workbook's own cell; a new SAF-to-IFC vocabulary axis is ONE row set carrying both directions as columns, never a map and its hand-kept inverse; a new physical member family is one entry on the owning roster row's physical map, its SAF role riding the same entry; a new analytical family is one roster row, never a second classifier beside the roster.
- Boundary: the seam graph is the ONLY export source and this page's `Workbook` the ONLY lowering, so a second `Relationship.Generic` walker or a rail-side `ExcelModel` assembly is the deleted parallel form; geometry crosses by CONTENT KEY alone through the seam `GeometrySource` — a member whose `Axis` key resolves nothing emits its row without a length cell, the named degradation, never a fabricated span, and a connection's position is its own `PlacementTransform` origin rather than a second geometry lane; the design-code cell elects off the `Model/eurocode#EUROCODE_ALGEBRA` `AnnexRegime` row's own KEY, so a second annex-to-`ExcelNationalCode` correspondence on this page, a `NationalAnnex` switch at the wire, and a stem derived from the annex abbreviation are each the deleted form — the abbreviation table is internal to `VividOrange.Standards` and its values do not stem the SAF token anyway; the named export negatives are stated per arm — the eccentricity content key is a preserved STEP fragment, not a Y/Z scalar pair, so the SAF eccentricity columns stay unset; the thermal gradient rows name no SAF cell, because TempL/R/T/B are fibre temperatures needing a section height no row carries, so only the constant DeltaT crosses; and the EN 1990 combination roster stays off the workbook, because SAF's combination table wants per-case factor arrays where the seam stores the package-generated `Definition` expressions, so a hand-parsed factor array off that text is the deleted form; an analytical member class outside the roster faults rather than silently skipping idealization, and a malformed eccentricity key faults the same way, because this codec's peer stamped it and a non-hex payload is corruption, never vocabulary; every quantity mints FROM already-SI seam magnitudes through the UnitsNet `From*` factories and reads back through the typed SI accessors, so neither `ToUnit(UnitSystem.SI)` nor `QuantityTypeConverter` is reached on this lane.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GeometryGym.Ifc;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Bim.Model;
using Rasm.Bim.Projection;
using Rasm.Domain;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Riok.Mapperly.Abstractions;
using SAF.DataAccess.Contracts;
using SAF.DataAccess.Models;
using SAF.DataAccess.Models.Enums;
using SAF.DataAccess.Models.Loads;
using SAF.DataAccess.Models.StructuralElements;
using SAF.DataAccess.Models.Subtypes;
using Thinktecture;
using UnitsNet;
using VividOrange.Loads.Cases;         // ActionClass — the nature token the seam Case row carries onto the SAF case sheet
using static LanguageExt.Prelude;

// Assembly policy root, seated at the branch's heaviest mapper seam (api-mapperly [03]): ExplicitCast conversions
// are landmines over Option-carrying seams — RMG001 escalates beside this in .editorconfig.
[assembly: MapperDefaults(EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]

namespace Rasm.Bim.Exchange;

// --- [TYPES] ------------------------------------------------------------------------------
[Union]
public abstract partial record SafOp {
    private SafOp() { }

    public sealed record Import(Stream Workbook, Version TargetVersion) : SafOp;
    public sealed record Export(Stream Workbook, ExcelModel Model, Version TargetVersion) : SafOp;
}

// What a roster row idealizes. A connection row carries an empty physical map because a connection idealizes a
// JOINT and IfcRelConnectsStructuralElement never binds one, but the emptiness is a CONSEQUENCE of the role,
// never the discriminant: the retired `!Physical.IsEmpty` read made every member/connection partition depend
// on a map's cardinality. Dimension is an INDEPENDENT axis — a curve member and a curve connection share it.
[SmartEnum<string>]
public sealed partial class CorrespondenceRole {
    public static readonly CorrespondenceRole Member = new("member");
    public static readonly CorrespondenceRole Connection = new("connection");
}

// Which member end a joint sits at. The seam row is a Boolean the IFC wire froze, so the CARRIER is the row
// set and the boolean stays at that one crossing. NAMED NEGATIVE: SAF's ExcelPosition.Both has no seam
// spelling — an IfcRelConnectsStructuralMember binds ONE connection — so an imported Both fans to two joints
// on import and no export row ever elects it.
[SmartEnum<string>]
public sealed partial class JointEnd {
    public static readonly JointEnd Start = new("start", atStart: true, position: ExcelPosition.Begin);
    public static readonly JointEnd End = new("end", atStart: false, position: ExcelPosition.End);

    public bool AtStart { get; }
    public ExcelPosition Position { get; }

    public static JointEnd Of(bool atStart) => atStart ? Start : End;
}

// The physical-to-analytical correspondence the seam graph carries but nothing owned: ONE closed roster keyed
// on the analytical entity name. A member row carries the physical IfcClass-to-SAF-role map its family admits
// (the keys classify the physical counterpart, the values elect the SAF member Type token — IfcPile and other
// roles the SAF enum lacks ride ExcelFlexibleEnum's own other-text lane), the schema-derived variety
// allowed-set its PredefinedType tokens draw from, and the analytical topology dimension; a connection row
// carries the IfcBoundaryCondition family its restraint lowers through — the node selects at dimension 0, the
// two subgrade selects at dimension 1, the SEALED face condition at dimension 2 — and every row names the SAF
// worksheet classes it exchanges as. Produced by the Correspondence fold; consumed by the Workbook and Author
// arms alone, never a second reader beside this owner.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class StructuralCorrespondence {
    public static readonly StructuralCorrespondence CurveMember = new("IfcStructuralCurveMember",
        role: CorrespondenceRole.Member,
        physical: toMap(Seq((nameof(IfcBeam), nameof(ExcelMember1DType.Beam)), (nameof(IfcColumn), nameof(ExcelMember1DType.Column)),
            (nameof(IfcMember), nameof(ExcelMember1DType.General)), (nameof(IfcPile), "Pile"))),
        varieties: toSeq(Enum.GetNames<IfcStructuralCurveMemberTypeEnum>()), dimension: 1,
        condition: Option<string>.None, saf: Seq(nameof(ExcelStructuralCurveMember)));
    public static readonly StructuralCorrespondence SurfaceMember = new("IfcStructuralSurfaceMember",
        role: CorrespondenceRole.Member,
        physical: toMap(Seq((nameof(IfcSlab), nameof(ExcelMember2DType.Plate)), (nameof(IfcWall), nameof(ExcelMember2DType.Wall)),
            (nameof(IfcPlate), nameof(ExcelMember2DType.Plate)), (nameof(IfcFooting), nameof(ExcelMember2DType.Plate)))),
        varieties: toSeq(Enum.GetNames<IfcStructuralSurfaceMemberTypeEnum>()), dimension: 2,
        condition: Option<string>.None, saf: Seq(nameof(ExcelStructuralSurfaceMember)));
    public static readonly StructuralCorrespondence PointConnection = new("IfcStructuralPointConnection",
        role: CorrespondenceRole.Connection,
        physical: Map<string, string>(), varieties: Seq<string>(), dimension: 0,
        condition: Some(nameof(IfcBoundaryNodeCondition)),
        saf: Seq(nameof(ExcelStructuralPointConnection), nameof(ExcelStructuralPointSupport), nameof(ExcelRelConnectsStructuralMember)));
    public static readonly StructuralCorrespondence CurveConnection = new("IfcStructuralCurveConnection",
        role: CorrespondenceRole.Connection,
        physical: Map<string, string>(), varieties: Seq<string>(), dimension: 1,
        condition: Some(nameof(IfcBoundaryEdgeCondition)),
        saf: Seq(nameof(ExcelStructuralCurveConnection), nameof(ExcelStructuralEdgeConnection)));
    public static readonly StructuralCorrespondence SurfaceConnection = new("IfcStructuralSurfaceConnection",
        role: CorrespondenceRole.Connection,
        physical: Map<string, string>(), varieties: Seq<string>(), dimension: 2,
        condition: Some(nameof(IfcBoundaryFaceCondition)),
        saf: Seq(nameof(ExcelStructuralSurfaceConnection)));

    public CorrespondenceRole Role { get; }
    public Map<string, string> Physical { get; }
    public Seq<string> Varieties { get; }
    public int Dimension { get; }
    public Option<string> Condition { get; }
    public Seq<string> Saf { get; }

    static readonly FrozenDictionary<string, StructuralCorrespondence> ByAnalytical =
        Items.ToFrozenDictionary(static row => row.Key, static row => row, StringComparer.OrdinalIgnoreCase);

    static readonly FrozenDictionary<string, StructuralCorrespondence> ByPhysical =
        Items.SelectMany(static row => row.Physical.Keys.Select(cls => (Class: cls, Row: row)))
            .ToFrozenDictionary(static pair => pair.Class, static pair => pair.Row, StringComparer.OrdinalIgnoreCase);

    public static Option<StructuralCorrespondence> OfAnalytical(string ifcClass) =>
        ByAnalytical.TryGetValue(ifcClass, out StructuralCorrespondence? row) && row is { } hit ? Some(hit) : None;

    public static Option<StructuralCorrespondence> OfPhysical(string ifcClass) =>
        ByPhysical.TryGetValue(ifcClass, out StructuralCorrespondence? row) && row is { } hit ? Some(hit) : None;
}

// ONE variety correspondence declared in ONE direction, both projections derived off it: the export reads the
// row's behaviour column and the import elects the row the wire behaviour ELECTS. CABLE and TENSION_MEMBER
// share TensionOnly on the wire, so the elected column names which variety an imported tension-only member
// reads and the wire's own loss of the cable distinction is a declared column rather than a shorter inverse
// map that could silently disagree with its forward twin.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CurveVariety {
    public static readonly CurveVariety RigidJoined = new(nameof(IfcStructuralCurveMemberTypeEnum.RIGID_JOINED_MEMBER),
        ExcelCurveBehaviour.Standard, IfcStructuralCurveMemberTypeEnum.RIGID_JOINED_MEMBER, elected: true);
    public static readonly CurveVariety PinJoined = new(nameof(IfcStructuralCurveMemberTypeEnum.PIN_JOINED_MEMBER),
        ExcelCurveBehaviour.AxialForceOnly, IfcStructuralCurveMemberTypeEnum.PIN_JOINED_MEMBER, elected: true);
    public static readonly CurveVariety Cable = new(nameof(IfcStructuralCurveMemberTypeEnum.CABLE),
        ExcelCurveBehaviour.TensionOnly, IfcStructuralCurveMemberTypeEnum.CABLE, elected: false);
    public static readonly CurveVariety Tension = new(nameof(IfcStructuralCurveMemberTypeEnum.TENSION_MEMBER),
        ExcelCurveBehaviour.TensionOnly, IfcStructuralCurveMemberTypeEnum.TENSION_MEMBER, elected: true);
    public static readonly CurveVariety Compression = new(nameof(IfcStructuralCurveMemberTypeEnum.COMPRESSION_MEMBER),
        ExcelCurveBehaviour.CompressionOnly, IfcStructuralCurveMemberTypeEnum.COMPRESSION_MEMBER, elected: true);

    public ExcelCurveBehaviour Behaviour { get; }
    public IfcStructuralCurveMemberTypeEnum Variety { get; }
    // Which row a wire behaviour elects back: the tie-break is a COLUMN, so the inverse index derives from the
    // same rows and cannot drift from the forward read.
    public bool Elected { get; }

    static readonly FrozenDictionary<ExcelCurveBehaviour, CurveVariety> ByBehaviour =
        Items.Where(static row => row.Elected).ToFrozenDictionary(static row => row.Behaviour, static row => row);

    static readonly FrozenDictionary<string, CurveVariety> ByVariety =
        Items.ToFrozenDictionary(static row => row.Key, static row => row, StringComparer.Ordinal);

    // The seam variety token is a round-tripped PredefinedType value, not a compile-time member, so the read is
    // TOTAL: an unrostered variety (USERDEFINED, NOTDEFINED, a release the roster predates) carries no wire
    // behaviour and the SAF cell stays unset rather than asserting Standard for a member nothing classified.
    public static Option<ExcelCurveBehaviour> BehaviourOf(string variety) =>
        ByVariety.TryGetValue(variety, out CurveVariety? row) && row is { } hit ? Some(hit.Behaviour) : None;

    // The USERDEFINED/NOTDEFINED residue and any variety the roster does not carry read Standard, the SAF
    // vocabulary's own unrestricted behaviour.
    public static IfcStructuralCurveMemberTypeEnum VarietyOf(Option<ExcelCurveBehaviour> behaviour) =>
        behaviour.Bind(static b => ByBehaviour.TryGetValue(b, out CurveVariety? row) && row is { } hit ? Some(hit) : None)
            .Map(static row => row.Variety)
            .IfNone(IfcStructuralCurveMemberTypeEnum.RIGID_JOINED_MEMBER);
}

// The case correspondence as ONE row set over the SAF load-case nature, both directions as columns: Seam is
// the consumer's closed dead/live/snow/wind/seismic token this codec exports under, Source the IFC action
// source the ingest CaseSources tier re-classifies on the next read, so a SAF round trip lands the same
// ActionRow the IFC wire would. Dead and live export as Others because SAF's SelfWeight names the GENERATED
// self-weight case specifically, which the seam token does not assert; Dynamic and Static carry no IFC source
// of their own and are absent, which the nature tier absorbs as NOTDEFINED.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SafCaseType {
    public static readonly SafCaseType SelfWeight = new("SelfWeight", ExcelLoadCaseType.SelfWeight, IfcActionSourceTypeEnum.DEAD_LOAD_G, None);
    public static readonly SafCaseType Prestress = new("Prestress", ExcelLoadCaseType.Prestress, IfcActionSourceTypeEnum.PRESTRESSING_P, None);
    public static readonly SafCaseType Temperature = new("Temperature", ExcelLoadCaseType.Temperature, IfcActionSourceTypeEnum.TEMPERATURE_T, None);
    public static readonly SafCaseType Wind = new("Wind", ExcelLoadCaseType.Wind, IfcActionSourceTypeEnum.WIND_W, Some("wind"));
    public static readonly SafCaseType Snow = new("Snow", ExcelLoadCaseType.Snow, IfcActionSourceTypeEnum.SNOW_S, Some("snow"));
    public static readonly SafCaseType Maintenance = new("Maintenance", ExcelLoadCaseType.Maintenance, IfcActionSourceTypeEnum.LIVE_LOAD_Q, None);
    public static readonly SafCaseType Fire = new("Fire", ExcelLoadCaseType.Fire, IfcActionSourceTypeEnum.FIRE, None);
    public static readonly SafCaseType Moving = new("Moving", ExcelLoadCaseType.Moving, IfcActionSourceTypeEnum.TRANSPORT, None);
    public static readonly SafCaseType Seismic = new("Seismic", ExcelLoadCaseType.Seismic, IfcActionSourceTypeEnum.EARTHQUAKE_E, Some("seismic"));
    public static readonly SafCaseType Others = new("Others", ExcelLoadCaseType.Others, IfcActionSourceTypeEnum.NOTDEFINED, Some("dead"), Some("live"));

    public ExcelLoadCaseType Wire { get; }
    public IfcActionSourceTypeEnum Source { get; }
    public Seq<string> Seam { get; }

    static readonly FrozenDictionary<ExcelLoadCaseType, SafCaseType> ByWire =
        Items.ToFrozenDictionary(static row => row.Wire, static row => row);

    static readonly FrozenDictionary<string, SafCaseType> BySeam =
        Items.SelectMany(static row => row.Seam.Select(token => (Token: token, Row: row)))
            .ToFrozenDictionary(static pair => pair.Token, static pair => pair.Row, StringComparer.Ordinal);

    public static Option<ExcelLoadCaseType> WireOf(string seamToken) =>
        BySeam.TryGetValue(seamToken, out SafCaseType? row) && row is { } hit ? Some(hit.Wire) : None;

    public static IfcActionSourceTypeEnum SourceOf(Option<ExcelLoadCaseType> wire) =>
        wire.Bind(static w => ByWire.TryGetValue(w, out SafCaseType? row) && row is { } hit ? Some(hit) : None)
            .Map(static row => row.Source)
            .IfNone(IfcActionSourceTypeEnum.NOTDEFINED);
}

// --- [MODELS] -----------------------------------------------------------------------------
// One physical-to-analytical correspondence fact off the seam graph: the analytical member node, its optional
// physical counterpart (an analytical-only model binds none), the roster row that classified it, the member's
// own variety token, the SAF role the physical class elects (None when unbound — the SAF Type column stays
// unset rather than fabricating a role), and the member's joints.
public readonly record struct CorrespondenceRow(
    NodeId Analytical, Option<NodeId> Physical, StructuralCorrespondence Kind, string Variety,
    Option<string> SafRole, Seq<CorrespondenceJoint> Joints);

// One member joint: the connection node, its roster row, the end discriminant, and the eccentricity content
// key — each read back through the SAME owner-declared row the structural reader stamped on the
// IfcRelConnectsStructuralMember Generic edge, so producer and consumer share one spelling.
public readonly record struct CorrespondenceJoint(
    NodeId Connection, StructuralCorrespondence Kind, Option<JointEnd> End, Option<UInt128> Eccentricity);

// --- [BOUNDARIES]
// The ONE foreign-cell admission. Every SAF cell the authoring reads crosses here exactly once, so the
// interior sees admitted values alone and no read coalesces an absent measurement to zero, an absent factor to
// unity, or an absent reference to the empty lookup key that silently misses instead of refusing. Columns
// ACCUMULATE on Validation, so one malformed row names every cell it is missing rather than the first.
internal static class SafCell {
    internal static Validation<Error, string> Text(string? cell, string row, string column, Op key) =>
        (Optional(cell).Filter(static value => value.Length > 0)
            .ToFin(new BimFault.Refused(key, BimScope.Model, BimReason.Rejected, string.Join(':', new object?[] { "saf-invalid", row, column, "absent" })))).ToValidation();

    internal static Validation<Error, double> Si<TQuantity>(
        TQuantity? cell, Func<TQuantity, double> si, string row, string column, Op key) where TQuantity : struct, IQuantity =>
        (Optional(cell).Map(si).ToFin(new BimFault.Refused(key, BimScope.Model, BimReason.Rejected, string.Join(':', new object?[] { "saf-invalid", row, column, "absent" })))).ToValidation();

    // SAF publishes a station as a loosely-typed cell, so the numeric admission is the boundary's own unbox:
    // a present cell that is not a number is malformed data, not an absent one.
    internal static Validation<Error, double> Number(object? cell, string row, string column, Op key) =>
        (Optional(cell as double?).ToFin(new BimFault.Refused(key, BimScope.Model, BimReason.Rejected, string.Join(':', new object?[] { "saf-invalid", row, column, "unreadable" })))).ToValidation();

    internal static Validation<Error, Seq<A>> All<A>(Seq<Validation<Error, A>> rows) => rows.Traverse(identity).As();
}

// The admitted SAF rows: every column is a value the boundary PROVED, so no arm below reads a nullable. Each
// carrier keeps its own worksheet name so a refusal, a residue fact, and a lookup all cite the same anchor.
// The workbook's own information row, carried FROM the seam header rather than left unwritten: the STEP
// FILE_NAME/FILE_DESCRIPTION provenance the ingest preserved images onto SAF's project cells, and the design
// code elects off the AnnexRegime the caller stated. Every column is named exactly as its SAF cell, so a
// renamed cell is a compile break at the Mapperly seam rather than a silently unwritten sheet.
internal readonly record struct SafInformation(
    string Name, Option<string> Description, Option<string> Owner, Option<string> SourceCompany,
    string SourceApplication, string SourceType, Option<ExcelNationalCode> NationalCode,
    ExcelSystemOfUnits SystemOfUnits);

internal readonly record struct SafPoint(string Name, Guid Id, double X, double Y, double Z);

internal readonly record struct SafCurve(
    string Name, Guid Id, string StartNode, string EndNode, Option<ExcelCurveBehaviour> Behaviour, Option<string> Role);

internal readonly record struct SafSurface(
    string Name, Guid Id, Seq<string> Corners, string Material, double Thickness, Option<string> Role);

internal readonly record struct SafRestraint(Seq<SafConstraint> Translations, Seq<SafConstraint> Rotations) {
    internal bool Linearized => (Translations + Rotations).Exists(static c => c.Linearized);
}

internal readonly record struct SafSupport(string Name, string Node, SafRestraint Restraint);

internal readonly record struct SafHinge(string Name, string Member, Option<ExcelPosition> Position, SafRestraint Restraint);

internal readonly record struct SafCase(string Name, IfcActionTypeEnum Nature, IfcActionSourceTypeEnum Source);

internal readonly record struct SafCombination(string Name, Seq<string> Cases, Seq<double> Factors, bool Ultimate);

// One admitted action, whatever its worksheet: the target reference, the case it belongs to, the frame, and
// the components already read as SI doubles. The four worksheet shapes differ only in WHICH components the
// row carries, so they share one carrier keyed by the LoadFamily row the authoring dispatches on.
internal readonly record struct SafAction(
    string Name, string Case, LoadFamily Family, string Target, bool Global, bool Projected,
    Seq<double> Components, Option<double> Start, Option<double> End);

// The whole admitted workbook: every dimensioned cell the authoring reads, proved BEFORE the first entity is
// constructed, so the authoring fold below holds no nullable and no coalesce. The Ends read is the ONE place
// SAF's ExcelPosition.Both fans — an IfcRelConnectsStructuralMember binds ONE connection, so Both authors two
// relationships rather than silently landing on the start alone.
internal readonly record struct SafWorkbook(
    Seq<SafPoint> Points, Seq<SafCurve> Curves, Seq<SafSurface> Surfaces, Seq<SafSupport> Supports,
    Seq<SafHinge> Hinges, Seq<SafCase> Cases, Seq<SafCombination> Combinations, Seq<SafAction> Actions) {
    internal Seq<string> Ends(SafHinge hinge) =>
        Curves.Find(row => row.Name == hinge.Member).ToSeq().Bind(row =>
            // An unstated position is the member START — the SAF vocabulary's own reading, declared once here
            // rather than as an arm that shares a body with Begin and hides which one answered.
            hinge.Position.IfNone(ExcelPosition.Begin) switch {
                ExcelPosition.Both => Seq(row.StartNode, row.EndNode),
                ExcelPosition.End => Seq(row.EndNode),
                _ => Seq(row.StartNode),
            });
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// ONE DOF row lowered to the SAF constraint pair and back: the Boolean verdict is Rigid/Free, the Measure
// verdict Flexible beside its SI spring, and an absent row an unset cell — the seam's single-row custody
// arriving intact on SAF's split Type/Stiffness columns. Linearized names the directional and non-linear
// constraint kinds this codec authors as their LINEAR base, so the degradation is a column the residue reads
// rather than a scan re-deriving the same predicate beside every authoring site.
internal readonly record struct SafConstraint(ExcelConstraintType? Type, double Si) {
    internal static SafConstraint Of(Map<PropertyName, PropertyValue> attrs, PropertyName dof) =>
        attrs.Find(dof).Match(
            Some: static value => value switch {
                PropertyValue.Boolean b => new SafConstraint(b.Value ? ExcelConstraintType.Rigid : ExcelConstraintType.Free, 0d),
                PropertyValue.Measure m => new SafConstraint(ExcelConstraintType.Flexible, m.Value.Si),
                _ => new SafConstraint(null, 0d),
            },
            None: static () => new SafConstraint(null, 0d));

    internal TQuantity? Spring<TQuantity>(Func<double, TQuantity> mint) where TQuantity : struct, IQuantity =>
        Type == ExcelConstraintType.Flexible ? mint(Si) : null;

    internal bool Linearized => Type is ExcelConstraintType.CompressionOnly or ExcelConstraintType.TensionOnly
        or ExcelConstraintType.NonLinear or ExcelConstraintType.FlexibleCompressionOnly or ExcelConstraintType.FlexibleTensionOnly;

    // The SAF constraint pair authored back as the GG DOF select, the exact inverse of the read: a directional
    // or non-linear constraint authors its LINEAR base — rigid for the rigid-acting kinds, its spring for the
    // flexible-acting kinds — and Linearized names every such row in the residue, so it is counted, never silent.
    internal IfcTranslationalStiffnessSelect Translational() => Type switch {
        ExcelConstraintType.Flexible or ExcelConstraintType.FlexibleCompressionOnly or ExcelConstraintType.FlexibleTensionOnly
            when Si > 0d => new IfcTranslationalStiffnessSelect(Si),
        ExcelConstraintType.Rigid or ExcelConstraintType.CompressionOnly or ExcelConstraintType.TensionOnly
            or ExcelConstraintType.NonLinear => new IfcTranslationalStiffnessSelect(true),
        _ => new IfcTranslationalStiffnessSelect(false),
    };

    internal IfcRotationalStiffnessSelect Rotational() => Type switch {
        ExcelConstraintType.Flexible or ExcelConstraintType.FlexibleCompressionOnly or ExcelConstraintType.FlexibleTensionOnly
            when Si > 0d => new IfcRotationalStiffnessSelect(Si),
        ExcelConstraintType.Rigid or ExcelConstraintType.CompressionOnly or ExcelConstraintType.TensionOnly
            or ExcelConstraintType.NonLinear => new IfcRotationalStiffnessSelect(true),
        _ => new IfcRotationalStiffnessSelect(false),
    };
}

// The ONE Mapperly seam over the SAF worksheet rows: the admitted carriers are the source of record in both
// directions, so the flat column-by-column transcription is generated and the per-type Option converters below
// are the only nullable crossing. A row's DIVERGENT columns carry a [MapProperty]; the rest map by name, which
// is what makes a renamed SAF column a compile break rather than a silently unwritten cell.
[Mapper(EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]
internal static partial class SafSeam {
    // Every information column already carries its SAF cell name, so this row maps entirely by name — the seam's
    // own proof that the correspondence is the SAF vocabulary's rather than a translation this page invented.
    internal static partial ExcelModelInformation ToWorksheet(SafInformation information);

    [MapProperty(nameof(SafPoint.X), nameof(ExcelStructuralPointConnection.X))]
    [MapProperty(nameof(SafPoint.Y), nameof(ExcelStructuralPointConnection.Y))]
    [MapProperty(nameof(SafPoint.Z), nameof(ExcelStructuralPointConnection.Z))]
    internal static partial ExcelStructuralPointConnection ToWorksheet(SafPoint point);

    [MapProperty(nameof(SafCurve.StartNode), nameof(ExcelStructuralCurveMember.NodeStartName))]
    [MapProperty(nameof(SafCurve.EndNode), nameof(ExcelStructuralCurveMember.NodeEndName))]
    [MapProperty(nameof(SafCurve.Role), nameof(ExcelStructuralCurveMember.Type), Use = nameof(Member1D))]
    internal static partial ExcelStructuralCurveMember ToWorksheet(SafCurve curve);

    [MapperIgnoreSource(nameof(SafSurface.Corners))]
    [MapProperty(nameof(SafSurface.Role), nameof(ExcelStructuralSurfaceMember.Type), Use = nameof(Member2D))]
    internal static partial ExcelStructuralSurfaceMember ToWorksheet(SafSurface surface);

    // Length, not the raw double: SAF's own column type is the UnitsNet quantity, so the mint happens once at
    // the seam and no arm re-mints a metre.
    [UserMapping]
    private static Length Metres(double si) => Length.FromMeters(si);

    // A 2D member's constant thickness is SAF's own two-cell carrier; a varying thickness has no seam row, so
    // only the first cell fills and the second stays unset rather than repeating the constant as a taper.
    [UserMapping]
    private static ExcelMemberThickness Plate(double si) => new() { ThicknessFirst = Length.FromMeters(si) };

    // The per-TYPE Option converters the rung law requires — a generic T? Map<T>(Option<T>) is refused, so each
    // nullable SAF column names the type it admits and a new column type is a new converter, never a silent cast.
    [UserMapping]
    private static string? Cell(Option<string> value) => value.IfNoneUnsafe(() => null);

    [UserMapping]
    private static ExcelCurveBehaviour? Cell(Option<ExcelCurveBehaviour> value) =>
        value.Match(Some: static behaviour => (ExcelCurveBehaviour?)behaviour, None: static () => null);

    // An absent regime leaves the design-code cell UNSET rather than writing EC_Standard_EN, which would certify
    // a model checked to no Eurocode as designed to the EN recommended values — the same law the eurocode owner
    // states for its own absent policy, composed here rather than re-decided at the wire.
    [UserMapping]
    private static ExcelNationalCode? Cell(Option<ExcelNationalCode> value) =>
        value.Match(Some: static code => (ExcelNationalCode?)code, None: static () => null);

    [UserMapping]
    private static ExcelFlexibleEnum<ExcelMember1DType>? Member1D(Option<string> role) => Flexible<ExcelMember1DType>(role);

    [UserMapping]
    private static ExcelFlexibleEnum<ExcelMember2DType>? Member2D(Option<string> role) => Flexible<ExcelMember2DType>(role);

    // ONE flexible-enum mint over the two member-type vocabularies: a role the SAF enum names parses, and a
    // role it lacks rides the other-text lane the flexible enum owns rather than being dropped.
    private static ExcelFlexibleEnum<TEnum>? Flexible<TEnum>(Option<string> role) where TEnum : struct, Enum =>
        role.Map(static value => Enum.TryParse(value, out TEnum known)
                ? new ExcelFlexibleEnum<TEnum>(known)
                : new ExcelFlexibleEnum<TEnum>(value))
            .Match(Some: static flexible => flexible, None: static () => (ExcelFlexibleEnum<TEnum>?)null);
}

public static class SafCodec {
    public static Fin<ExcelModel> Run(
        SafOp operation,
        IExcelImportService imports,
        IExcelExportService exports,
        IExcelValidator validator,
        Op key) =>
        operation.Switch<Fin<ExcelModel>>(
            import: request => key.Catch(() => imports.Import(request.Workbook, request.TargetVersion))
                .Bind(model => Admitted(validator.ValidateForImport(model, request.TargetVersion, model.OriginalVersion), key)),
            // A GRAPH-authored ExcelModel carries no source workbook, so its OriginalVersion is unset (the ctor
            // never assigns it — only the import service does); the target IS the source currency for a model
            // born at that version, so the coalesce resolves ONCE here rather than at both service calls.
            export: request => Source(request).Apply(source =>
                Admitted(validator.ValidateForExport(request.Model, request.TargetVersion, source), key)
                    .Bind(model => key.Catch(() => exports.Export(request.Workbook, model, request.TargetVersion, source)))
                    .Bind(result => result.IsSuccess
                        ? Fin.Succ(result.Model)
                        : Fin.Fail<ExcelModel>(new BimFault.Refused(key, BimScope.Model, BimReason.Rejected, string.Join(':', new object?[] { "saf-invalid", "export", ExcelValidationResult.Format(result.ValidationResults) }))))));

    private static Version Source(SafOp.Export request) => request.Model.OriginalVersion ?? request.TargetVersion;

    private static Fin<ExcelModel> Admitted(ExcelModel model, Op key) =>
        model.ValidationErrors.Any(static error => error.Severity == ExcelValidationMessageSeverity.Error)
            ? Fin.Fail<ExcelModel>(new BimFault.Refused(key, BimScope.Model, BimReason.Rejected, string.Join(':', new object?[] { "saf-invalid", "validation", ExcelValidationResult.Format(model.ValidationErrors) })))
            : Fin.Succ(model);

    // --- [CORRESPONDENCE]

    // The physical-to-analytical read path over the ONE seam graph: every analytical member node classifies
    // through the roster, its optional physical counterpart resolves off the IfcRelConnectsStructuralElement
    // Generic edge, and its joints off the IfcRelConnectsStructuralMember edges — the end discriminant and the
    // eccentricity content key read back through the SAME rows the structural reader stamped.
    public static Fin<Seq<CorrespondenceRow>> Correspondence(ElementGraph graph, Op key) {
        Seq<Relationship.Generic> generics = toSeq(graph.Edges).Choose(static edge => edge is Relationship.Generic g ? Some(g) : None);
        // AddOrUpdate, never toMap: the edge set is FILE-controlled, and a source binding one idealized member
        // from two physical elements is malformed data a throwing duplicate-key Add would escalate into an
        // unhandled exception across the Fin rail. Last-wins is the deterministic election over the ordered fold.
        Map<NodeId, NodeId> physicals = generics
            .Filter(static edge => edge.WireName == IfcRelKind.ConnectsStructElement.Key)
            .Fold(Map<NodeId, NodeId>(), static (map, edge) => map.AddOrUpdate(edge.Target, edge.Source));
        Seq<Relationship.Generic> joints = generics.Filter(static edge => edge.WireName == IfcRelKind.ConnectsStructMember.Key);
        return graph.ObjectNodes
            .Choose(node => StructuralCorrespondence.OfAnalytical(node.Classification.Code)
                .Filter(static row => row.Role == CorrespondenceRole.Member)
                .Map(row => (Node: node, Kind: row)))
            .TraverseM(member => joints.Filter(joint => joint.Source == member.Node.Id)
                .TraverseM(joint => JointOf(graph, joint, key)).As()
                .Map(resolved => new CorrespondenceRow(
                    member.Node.Id, physicals.Find(member.Node.Id), member.Kind, member.Node.PredefinedType.Token,
                    physicals.Find(member.Node.Id)
                        .Bind(graph.Find)
                        .Bind(node => node is Node.Object o ? member.Kind.Physical.Find(o.Classification.Code) : None),
                    resolved)))
            .As();
    }

    private static Fin<CorrespondenceJoint> JointOf(ElementGraph graph, Relationship.Generic joint, Op key) =>
        from kind in graph.Find(joint.Target)
            .Bind(static node => node is Node.Object o ? StructuralCorrespondence.OfAnalytical(o.Classification.Code) : None)
            .Filter(static row => row.Role == CorrespondenceRole.Connection)
            .ToFin(new BimFault.Refused(key, BimScope.Model, BimReason.Rejected, string.Join(':', new object?[] { "correspondence-connection-unrostered", joint.Target.Value })))
        from eccentricity in joint.Attributes.Find(StructuralProjection.Eccentricity).Match(
            Some: value => value is PropertyValue.Text text && UInt128.TryParse(text.Value, NumberStyles.HexNumber, null, out UInt128 parsed)
                ? Fin.Succ(Some(parsed))
                : Fin.Fail<Option<UInt128>>(new BimFault.Refused(key, BimScope.Model, BimReason.Rejected, string.Join(':', new object?[] { "correspondence-eccentricity-malformed", joint.Target.Value }))),
            None: static () => Fin.Succ(Option<UInt128>.None))
        select new CorrespondenceJoint(joint.Target, kind,
            joint.Attributes.Find(StructuralRows.AtStart)
                .Bind(static value => value is PropertyValue.Boolean b ? Some(JointEnd.Of(b.Value)) : None),
            eccentricity);

    // --- [WORKBOOK]

    // The graph-to-ExcelModel lowering the export leg realizes. Geometry crosses ONLY by content key through
    // the seam GeometrySource — a member whose Axis key resolves nothing emits its row without a length cell —
    // and a connection's position is its OWN placement origin, so no second geometry lane opens here.
    // regime is REQUIRED and undefaulted for the reason the Model/eurocode#EUROCODE_ALGEBRA policy is: a default
    // lets every landed caller elect nothing and the design-code cell silently never writes. None IS a lawful
    // election — a model checked to IBC, NBR, SIA 26x, or the Malaysian annex names an ExcelNationalCode member
    // no NationalAnnex declares, so it states no regime here and the cell stays unset.
    public static Fin<ExcelModel> Workbook(
        ElementGraph graph, GeometrySource geometry, Option<AnnexRegime> regime, Op key) =>
        Correspondence(graph, key).Map(rows => {
            Map<NodeId, Node.Object> objects = toMap(graph.ObjectNodes.Map(static node => (node.Id, node)));
            Seq<Relationship.Generic> generics = toSeq(graph.Edges).Choose(static edge => edge is Relationship.Generic g ? Some(g) : None);
            Seq<Relationship.Generic> joints = generics.Filter(static edge => edge.WireName == IfcRelKind.ConnectsStructMember.Key);
            Seq<Relationship.Generic> activities = generics.Filter(static edge => edge.WireName == IfcRelKind.ConnectsStructActivity.Key);

            Seq<IExcelModuleObject> points = Nodes(graph, StructuralCorrespondence.PointConnection)
                .Map(node => (IExcelModuleObject)SafSeam.ToWorksheet(new SafPoint(
                    SafName(node), GuidOf(node),
                    node.Placement.Map(static frame => frame.Location.X).IfNone(0d),
                    node.Placement.Map(static frame => frame.Location.Y).IfNone(0d),
                    node.Placement.Map(static frame => frame.Location.Z).IfNone(0d))));

            Seq<IExcelModuleObject> members = rows
                .Choose(row => objects.Find(row.Analytical).Map(node => (Row: row, Node: node)))
                .Map(member => member.Row.Kind.Dimension == 1
                    ? (IExcelModuleObject)Curve(objects, member.Row, member.Node, geometry)
                    : Surface(graph, member.Row, member.Node));

            Seq<IExcelModuleObject> supports = graph.ObjectNodes
                .Choose(node => StructuralCorrespondence.OfAnalytical(node.Classification.Code)
                    .Filter(static row => row.Role == CorrespondenceRole.Connection)
                    .Map(row => (Node: node, Kind: row)))
                .Bind(connection => SupportsOf(graph, joints, objects, connection.Node, connection.Kind));

            Seq<IExcelModuleObject> releases = joints
                .Filter(static joint => StructuralProjection.Release.Dofs.Exists(joint.Attributes.ContainsKey))
                .Map(joint => Hinge(objects, joint));

            Seq<IExcelModuleObject> cases = Cases(activities);
            Seq<IExcelModuleObject> loads = activities.Bind(edge => Actions(objects, edge));

            Seq<IExcelModuleObject> information = Seq<IExcelModuleObject>(
                SafSeam.ToWorksheet(Information(graph, regime)));

            return new ExcelModel(
                (information + points + members + supports + releases + cases + loads).ToList(),
                new List<ExcelValidationResult>(), ExcelSystemOfUnits.Metric);
        });

    // The information sheet the lowering authors off the seam header. The design code is the ONE cell this page
    // does not read from the graph: it elects from the regime row the caller states, whose KEY IS the
    // ExcelNationalCode member name, so the wire value PARSES off the Model/eurocode#EUROCODE_ALGEBRA roster and
    // no second annex-to-code table exists here — the same derivation CombinationSet's key takes onto
    // ExcelLoadCaseCombinationStandard. The inverse costs no surface either: AnnexRegime.TryGet(code.ToString())
    // recovers the row from an imported workbook's own cell whenever a consumer wants the regime back.
    // NAMED LOSS: the Created/LastUpdate cells stay unset because StepHeader.Empty carries a default Instant a
    // graph authored in-process cannot distinguish from an authored 1970 stamp, and the coordinate-system cells
    // stay unset because SAF names axis CONVENTIONS where the seam header carries a GeoReference CRS identity.
    // WITNESS: every remaining information cell the seam header holds is written by this row.
    private static SafInformation Information(ElementGraph graph, Option<AnnexRegime> regime) =>
        new(graph.Header.Step.Name,
            graph.Header.Step.Descriptions.Head,
            graph.Header.Step.Authors.Head,
            graph.Header.Step.Organizations.Head,
            graph.Header.Step.OriginatingSystem,
            graph.Header.Step.Preprocessor,
            regime.Bind(static row => Enum.TryParse(row.Key, out ExcelNationalCode code) ? Some(code) : None),
            ExcelSystemOfUnits.Metric);

    private static Seq<Node.Object> Nodes(ElementGraph graph, StructuralCorrespondence kind) =>
        graph.ObjectNodes.Filter(node => StructuralCorrespondence.OfAnalytical(node.Classification.Code).Exists(row => row == kind));

    // One 1D member row: joints order start-first onto the SAF begin-to-end node list, the role cell fills only
    // off a BOUND physical counterpart, and the axis chord fills Length when the content key resolves.
    private static ExcelStructuralCurveMember Curve(
        Map<NodeId, Node.Object> objects, CorrespondenceRow row, Node.Object node, GeometrySource geometry) {
        Seq<CorrespondenceJoint> ordered =
            row.Joints.Filter(static joint => joint.End == Some(JointEnd.Start))
            + row.Joints.Filter(static joint => joint.End != Some(JointEnd.Start));
        ExcelStructuralCurveMember member = SafSeam.ToWorksheet(new SafCurve(
            SafName(node), GuidOf(node),
            ordered.Head.Map(joint => Host(objects, joint.Connection)).IfNone(""),
            ordered.Last.Map(joint => Host(objects, joint.Connection)).IfNone(""),
            CurveVariety.BehaviourOf(row.Variety), row.SafRole));
        member.Nodes = ordered.Map(joint => Host(objects, joint.Connection)).ToArray();
        geometry.Axis(node.Representations).IfSome(curve => member.Length = Length.FromMeters(curve.Length));
        return member;
    }

    // One 2D member row: the constant thickness off the member's own entity bag, the outline nodes off the
    // joints — a varying thickness has no seam row and stays a SAF-side authoring concern.
    private static ExcelStructuralSurfaceMember Surface(ElementGraph graph, CorrespondenceRow row, Node.Object node) {
        ExcelStructuralSurfaceMember member = SafSeam.ToWorksheet(new SafSurface(
            SafName(node), GuidOf(node), Seq<string>(), "",
            StructuralProjection.Si(BagOf(graph, node.Id), StructuralRow.Thickness.Name), row.SafRole));
        return member;
    }

    // The support rows one connection lowers: dimension 0 the SAF point support off the node DOF verdicts (the
    // connection's own bag first, else the first incident joint edge's Support family — one custody, two stamp
    // sites), dimension 1 the curve connection off the subgrade verdicts (Pressure and
    // RotationalStiffnessPerLength — the two reaction quantities one exponent below the node pair, exactly the
    // measure split the ingest read), and dimension 2 the bare surface-connection row: the face condition's
    // stiffness is SEALED at the source, so no subsoil cell is ever fabricated for it.
    private static Seq<IExcelModuleObject> SupportsOf(
        ElementGraph graph, Seq<Relationship.Generic> joints, Map<NodeId, Node.Object> objects,
        Node.Object connection, StructuralCorrespondence kind) {
        Map<PropertyName, PropertyValue> bag = BagOf(graph, connection.Id);
        Map<PropertyName, PropertyValue> attrs = Restrained(bag)
            ? bag
            : joints.Filter(joint => joint.Target == connection.Id).Map(static joint => joint.Attributes)
                .Filter(Restrained).Head.IfNone(bag);
        SafRestraint restraint = RestraintOf(attrs, StructuralProjection.Support);
        string member = joints.Filter(joint => joint.Target == connection.Id).Head
            .Map(joint => Host(objects, joint.Source)).IfNone("");
        return kind.Dimension switch {
            0 => Seq((IExcelModuleObject)new ExcelStructuralPointSupport {
                Id = GuidOf(connection), Name = SafName(connection), Node = SafName(connection),
                Type = Predefined(restraint), BoundaryCondition = ExcelStructuralPointSupportType.InNode,
                TranslationXType = restraint.Translations[0].Type,
                TranslationXStiffness = restraint.Translations[0].Spring(ForcePerLength.FromNewtonsPerMeter),
                TranslationYType = restraint.Translations[1].Type,
                TranslationYStiffness = restraint.Translations[1].Spring(ForcePerLength.FromNewtonsPerMeter),
                TranslationZType = restraint.Translations[2].Type,
                TranslationZStiffness = restraint.Translations[2].Spring(ForcePerLength.FromNewtonsPerMeter),
                RotationXType = restraint.Rotations[0].Type,
                RotationXStiffness = restraint.Rotations[0].Spring(RotationalStiffness.FromNewtonMetersPerRadian),
                RotationYType = restraint.Rotations[1].Type,
                RotationYStiffness = restraint.Rotations[1].Spring(RotationalStiffness.FromNewtonMetersPerRadian),
                RotationZType = restraint.Rotations[2].Type,
                RotationZStiffness = restraint.Rotations[2].Spring(RotationalStiffness.FromNewtonMetersPerRadian),
            }),
            1 => Seq((IExcelModuleObject)new ExcelStructuralCurveConnection {
                Id = GuidOf(connection), Name = SafName(connection), Member = member,
                TranslationXType = restraint.Translations[0].Type,
                TranslationXStiffness = restraint.Translations[0].Spring(Pressure.FromPascals),
                TranslationYType = restraint.Translations[1].Type,
                TranslationYStiffness = restraint.Translations[1].Spring(Pressure.FromPascals),
                TranslationZType = restraint.Translations[2].Type,
                TranslationZStiffness = restraint.Translations[2].Spring(Pressure.FromPascals),
                RotationXType = restraint.Rotations[0].Type,
                RotationXStiffness = restraint.Rotations[0].Spring(RotationalStiffnessPerLength.FromNewtonMetersPerRadianPerMeter),
                RotationYType = restraint.Rotations[1].Type,
                RotationYStiffness = restraint.Rotations[1].Spring(RotationalStiffnessPerLength.FromNewtonMetersPerRadianPerMeter),
                RotationZType = restraint.Rotations[2].Type,
                RotationZStiffness = restraint.Rotations[2].Spring(RotationalStiffnessPerLength.FromNewtonMetersPerRadianPerMeter),
            }),
            _ => Seq((IExcelModuleObject)new ExcelStructuralSurfaceConnection {
                Id = GuidOf(connection), Name = SafName(connection), Member2D = member,
            }),
        };
    }

    private static ExcelRelConnectsStructuralMember Hinge(Map<NodeId, Node.Object> objects, Relationship.Generic joint) {
        SafRestraint restraint = RestraintOf(joint.Attributes, StructuralProjection.Release);
        return new ExcelRelConnectsStructuralMember {
            Name = $"{Host(objects, joint.Source)}-{Host(objects, joint.Target)}",
            Member = Host(objects, joint.Source),
            Position = joint.Attributes.Find(StructuralRows.AtStart)
                .Bind(static value => value is PropertyValue.Boolean b ? Some(JointEnd.Of(b.Value)) : None)
                .Match(Some: static end => (ExcelPosition?)end.Position, None: static () => null),
            TranslationXType = restraint.Translations[0].Type,
            TranslationXStiffness = restraint.Translations[0].Spring(ForcePerLength.FromNewtonsPerMeter),
            TranslationYType = restraint.Translations[1].Type,
            TranslationYStiffness = restraint.Translations[1].Spring(ForcePerLength.FromNewtonsPerMeter),
            TranslationZType = restraint.Translations[2].Type,
            TranslationZStiffness = restraint.Translations[2].Spring(ForcePerLength.FromNewtonsPerMeter),
            RotationXType = restraint.Rotations[0].Type,
            RotationXStiffness = restraint.Rotations[0].Spring(RotationalStiffness.FromNewtonMetersPerRadian),
            RotationYType = restraint.Rotations[1].Type,
            RotationYStiffness = restraint.Rotations[1].Spring(RotationalStiffness.FromNewtonMetersPerRadian),
            RotationZType = restraint.Rotations[2].Type,
            RotationZStiffness = restraint.Rotations[2].Spring(RotationalStiffness.FromNewtonMetersPerRadian),
        };
    }

    // ONE bag-to-restraint read the three worksheet rows share: the three verbatim DOF column blocks each
    // re-spelled these six lookups. NAMED LOSS: SAF's generated row types expose twelve FLAT columns whose
    // stiffness type differs per worksheet, so the column NAMES still appear per row — what collapses is the
    // bag read, the flexible gate, and the linearization scan, each now one owner.
    private static SafRestraint RestraintOf(Map<PropertyName, PropertyValue> attrs, RestraintFamily family) =>
        new(StructuralRows.Axes.Map(axis => SafConstraint.Of(attrs, family.Translation[axis])),
            StructuralRows.Axes.Map(axis => SafConstraint.Of(attrs, family.Rotation[axis])));

    private static bool Restrained(Map<PropertyName, PropertyValue> attrs) =>
        StructuralRows.Axes.Exists(axis => attrs.ContainsKey(StructuralRows.Translation[axis]));

    // The SAF predefined-support cell as ROWS over the verdict shape the DOF rows already carry, so the cell
    // never asserts a named condition the rows contradict and Custom is a stated residue, not a fall-through.
    private static readonly Seq<(Seq<ExcelConstraintType?> T, Seq<ExcelConstraintType?> R, ExcelBoundaryNodeCondition Cell)> Predefines = Seq(
        (Rigid3, Rigid3, ExcelBoundaryNodeCondition.Fixed),
        (Rigid3, Free3, ExcelBoundaryNodeCondition.Hinged),
        (Seq((ExcelConstraintType?)ExcelConstraintType.Free, ExcelConstraintType.Free, ExcelConstraintType.Rigid), Free3,
            ExcelBoundaryNodeCondition.Sliding));

    private static Seq<ExcelConstraintType?> Rigid3 => Seq((ExcelConstraintType?)ExcelConstraintType.Rigid, ExcelConstraintType.Rigid, ExcelConstraintType.Rigid);

    private static Seq<ExcelConstraintType?> Free3 => Seq((ExcelConstraintType?)ExcelConstraintType.Free, ExcelConstraintType.Free, ExcelConstraintType.Free);

    private static ExcelBoundaryNodeCondition Predefined(SafRestraint restraint) =>
        Predefines.Find(row => row.T == restraint.Translations.Map(static c => c.Type)
                            && row.R == restraint.Rotations.Map(static c => c.Type))
            .Map(static row => row.Cell)
            .IfNone(ExcelBoundaryNodeCondition.Custom);

    private static Seq<IExcelModuleObject> Cases(Seq<Relationship.Generic> activities) =>
        activities.Choose(static edge => edge.Attributes.Find(StructuralRows.Case).Bind(Text)).Distinct()
            .Map(token => (IExcelModuleObject)new ExcelStructuralLoadCase {
                Name = token,
                ActionType = activities
                    .Filter(edge => edge.Attributes.Find(StructuralRows.Case).Bind(Text) == Some(token))
                    .Choose(static edge => edge.Attributes.Find(StructuralRow.ActionClassRow.Name).Bind(Text)).Head
                    .Map(static nature => nature switch {
                        nameof(ActionClass.Permanent) => ExcelActionType.Permanent,
                        nameof(ActionClass.Accidental) => ExcelActionType.Accidental,
                        _ => ExcelActionType.Variable,
                    })
                    .Match(Some: static nature => (ExcelActionType?)nature, None: static () => null),
                LoadType = SafCaseType.WireOf(token).Match(Some: static type => (ExcelLoadCaseType?)type, None: static () => null),
            });

    // The SAF actions one activity edge lowers, dispatched on the SAME closed LoadFamily roster the reader's
    // component projection keys on, so a new load family breaks HERE rather than falling to a silent empty.
    // A displacement edge carries no component rows and emits nothing.
    private static Seq<IExcelModuleObject> Actions(Map<NodeId, Node.Object> objects, Relationship.Generic edge) {
        Map<PropertyName, PropertyValue> attrs = edge.Attributes;
        string host = Host(objects, edge.Source);
        ActionHost at = ActionHost.Of(objects, edge, host);
        string caseName = attrs.Find(StructuralRows.Case).Bind(Text).IfNone("live");
        ExcelCoordinateSystem system = attrs.Find(StructuralRow.GlobalOrLocal.Name).Bind(Text)
            .Exists(static frame => frame == nameof(IfcGlobalOrLocalEnum.LOCAL_COORDS))
            ? ExcelCoordinateSystem.Local : ExcelCoordinateSystem.Global;
        return StructuralProjection.LoadTypeOf(attrs).Match(
            None: static () => Seq<IExcelModuleObject>(),
            Some: family => family.Switch<Seq<IExcelModuleObject>>(
                singleForce: () => Seq((IExcelModuleObject)new ExcelStructuralPointAction {
                        Name = caseName, LoadCase = caseName, CoordinateSystem = system,
                        Direction = ExcelActionDirection.Vector,
                        DirectionVector = VectorOf(attrs, StructuralRows.Force, Force.FromNewtons),
                        ReferenceNode = at.Node, ReferenceMember = at.Member,
                        CoordinateDefinition = at.Definition, PositionX = at.PositionX, Origin = at.Origin,
                    })
                    + Axes(attrs, StructuralRows.Moment).Map(axis => (IExcelModuleObject)new ExcelStructuralPointMoment {
                        Name = caseName, LoadCase = caseName, CoordinateSystem = system,
                        Direction = MomentDirection(axis),
                        Value = Torque.FromNewtonMeters(StructuralProjection.Si(attrs, StructuralRows.Moment[axis])),
                        ReferenceNode = at.Node, ReferenceMember = at.Member,
                        CoordinateDefinition = at.Definition, PositionX = at.PositionX, Origin = at.Origin,
                    }),
                linearForce: () => Seq((IExcelModuleObject)new ExcelStructuralCurveAction {
                        Name = caseName, Member = host, LoadCase = caseName, CoordinateSystem = system,
                        Distribution = ExcelCurveDistribution.Uniform, Direction = ExcelActionDirection.Vector,
                        DirectionVector = VectorOf(attrs, StructuralRows.Force, ForcePerLength.FromNewtonsPerMeter),
                    })
                    + Axes(attrs, StructuralRows.Moment).Map(axis => (IExcelModuleObject)new ExcelStructuralCurveMoment {
                        Name = caseName, Member = host, LoadCase = caseName, CoordinateSystem = system,
                        Distribution = ExcelCurveDistribution.Uniform, Direction = MomentDirection(axis),
                        Value1 = TorquePerLength.FromNewtonMetersPerMeter(StructuralProjection.Si(attrs, StructuralRows.Moment[axis])),
                    }),
                planarForce: () => Axes(attrs, StructuralRows.PlanarForce)
                    .Map(axis => (IExcelModuleObject)new ExcelStructuralSurfaceAction {
                        Name = caseName, Member2DReference = host, LoadCase = caseName, CoordinateSystem = system,
                        Direction = axis switch { "X" => ExcelActionDirection.X, "Y" => ExcelActionDirection.Y, _ => ExcelActionDirection.Z },
                        Value = Pressure.FromPascals(StructuralProjection.Si(attrs, StructuralRows.PlanarForce[axis])),
                    }),
                temperature: () => Seq((IExcelModuleObject)new ExcelStructuralCurveActionThermal {
                    Name = caseName, Member = host, LoadCase = caseName,
                    DeltaT = Temperature.FromKelvins(StructuralProjection.Si(attrs, StructuralRows.DeltaT["Constant"])),
                }),
                displacement: static () => Seq<IExcelModuleObject>(),
                configuration: () => Seq((IExcelModuleObject)new ExcelStructuralCurveAction {
                    Name = caseName, Member = host, LoadCase = caseName, CoordinateSystem = system,
                    Distribution = ExcelCurveDistribution.Trapezoidal, Direction = ExcelActionDirection.Vector,
                    DirectionVector = VectorOf(attrs, StructuralRows.Start, ForcePerLength.FromNewtonsPerMeter),
                    DirectionVector2 = VectorOf(attrs, StructuralRows.End, ForcePerLength.FromNewtonsPerMeter),
                    CoordinateDefinition = ExcelCoordinateDefinition.Absolute, Origin = ExcelOrigin.FromStart,
                    StartPoint = StructuralProjection.Si(attrs, StructuralRow.SpanStart.Name),
                    EndPoint = StructuralProjection.Si(attrs, StructuralRow.SpanEnd.Name),
                })));
    }

    // Where a SAF action attaches, as ONE value: a node action names its node and carries no station, a member
    // action names its member and carries the relative station. The retired `onNode ? … : null` ternary ran
    // five times per row across two arms and re-derived the same partition at each.
    private readonly record struct ActionHost(
        string? Node, string? Member, ExcelCoordinateDefinition? Definition, object? PositionX, ExcelOrigin? Origin) {
        internal static ActionHost Of(Map<NodeId, Node.Object> objects, Relationship.Generic edge, string host) =>
            objects.Find(edge.Source)
                .Bind(static node => StructuralCorrespondence.OfAnalytical(node.Classification.Code))
                .Exists(static row => row.Role == CorrespondenceRole.Connection)
                ? new ActionHost(host, null, null, null, null)
                : new ActionHost(null, host, ExcelCoordinateDefinition.Relative,
                    edge.Attributes.Find(StructuralRows.Station)
                        .Bind(static value => value is PropertyValue.Measure m ? Some(m.Value.Si) : None)
                        .Match(Some: static station => (object)station, None: static () => null),
                    ExcelOrigin.FromStart);
    }

    private static Seq<string> Axes(Map<PropertyName, PropertyValue> attrs, Map<string, PropertyName> family) =>
        StructuralRows.Axes.Filter(axis => attrs.ContainsKey(family[axis]));

    private static ExcelMomentDirection MomentDirection(string axis) =>
        axis switch { "X" => ExcelMomentDirection.Mx, "Y" => ExcelMomentDirection.My, _ => ExcelMomentDirection.Mz };

    private static ExcelLoadDirectionVector<TQuantity> VectorOf<TQuantity>(
        Map<PropertyName, PropertyValue> attrs, Map<string, PropertyName> family, Func<double, TQuantity> mint)
        where TQuantity : struct, IQuantity =>
        new() {
            X = mint(StructuralProjection.Si(attrs, family["X"])),
            Y = mint(StructuralProjection.Si(attrs, family["Y"])),
            Z = mint(StructuralProjection.Si(attrs, family["Z"])),
        };

    // The entity-bag read: every PropertySet bag assigned to the owner folds into one map — the same
    // Assign.PropertyDefinition walk the seam Bake takes, so this leg reads the bags the ingest arms landed.
    private static Map<PropertyName, PropertyValue> BagOf(ElementGraph graph, NodeId owner) =>
        toSeq(graph.EdgesAt(owner))
            .Choose(edge => edge is Relationship.Assign assign
                && assign.Subject == owner && assign.SubKind == AssignKind.PropertyDefinition
                ? Some(assign.Definition) : None)
            .Choose(definition => graph.Find(definition).Bind(static node => node is Node.PropertySet set ? Some(set.Bag.Values) : None))
            .Fold(Map<PropertyName, PropertyValue>(), static (folded, values) => folded.AddRange(values.ToSeq()));

    private static string Host(Map<NodeId, Node.Object> objects, NodeId id) =>
        objects.Find(id).Map(SafName).IfNone(id.Value);

    private static string SafName(Node.Object node) => node.Name.Length > 0 ? node.Name : node.Id.Value;

    // SAF's Id is a Guid: the 32-hex NodeId re-keys verbatim through the exact "N" parse; a non-hex identity
    // keeps Guid.Empty and the NAME stays the join key, because SAF references are name-strung throughout.
    private static Guid GuidOf(Node.Object node) =>
        Guid.TryParseExact(node.Id.Value, "N", out Guid id) ? id : Guid.Empty;

    private static Option<string> Text(PropertyValue value) => value is PropertyValue.Text text ? Some(text.Value) : None;

    // --- [AUTHORING]

    // The import leg: the ExcelModel AUTHORS the GeometryGym structural-analysis entities on the target
    // database — nodes, members, supports, releases, cases, combinations, actions — and the ONE
    // SemanticProjector then ingests that database, so the SAF wire re-enters through the exact fold the IFC
    // wire takes. Admission runs FIRST and WHOLE: every dimensioned cell the authoring needs is proved before
    // a single entity is constructed, so a malformed workbook refuses with every missing column named rather
    // than authoring a model of origin-points and zero-thickness plates. The residue rides the ledger, so a
    // caller cannot drop it; the GG ctor is the one throwing seam and crosses as BimFault.Refused.
    public static WriterT<FidelityLog, Fin, Unit> Author(
        DatabaseIfc db, IfcSpatialElement host, ExcelModel model, Op key) =>
        from admitted in Fidelity.Lift(Admit(model, key))
        from analysis in Fidelity.Lift(Boundary(key, () =>
            new IfcStructuralAnalysisModel(host, "SAF", IfcAnalysisModelTypeEnum.LOADING_3D)))
        from nodes in Fidelity.Lift(Boundary(key, () => Points(db, analysis, admitted.Points)))
        from members in Fidelity.Lift(Boundary(key, () => Curves(db, analysis, admitted.Curves, nodes)))
        from surfaces in Fidelity.Lift(Boundary(key, () => Surfaces(db, analysis, admitted.Surfaces, nodes)))
        from cases in Fidelity.Lift(Boundary(key, () => LoadCases(analysis, admitted.Cases)))
        from _supports in Supports(db, nodes, admitted.Supports, key)
        from _hinges in Hinges(db, nodes, members, admitted, key)
        from _combinations in Fidelity.Lift(Boundary(key, () => Combinations(analysis, cases, admitted.Combinations)))
        from _actions in Fidelity.Lift(Boundary(key, () => Applied(db, cases, nodes, members, surfaces, admitted)))
        from _unmapped in Unmapped(model)
        select unit;

    // The named authoring negatives: sealed-at-the-source payloads (face-condition subsoil, displacement
    // components), IFC-counterpartless relations, and the SAF result tables an authoring leg never carries —
    // one fact per PRESENT object type, so an absent table names nothing.
    private static readonly Seq<string> Uncarried = Seq(
        nameof(ExcelStructuralSurfaceConnection), nameof(ExcelStructuralPointSupportDeformation),
        nameof(ExcelRelConnectsRigidLink), nameof(ExcelRelConnectsRigidMember), nameof(ExcelRelConnectsRigidCross),
        nameof(ExcelResultInternalForce1D), nameof(ExcelResultInternalForce2D));

    private static WriterT<FidelityLog, Fin, Unit> Unmapped(ExcelModel model) =>
        Uncarried.Filter(type => model.Objects.Exists(row => row.GetType().Name == type))
            .TraverseM(static type => Fidelity.Drop(FidelityDrop.SafResidue, type, unit)).As()
            .Map(static _ => unit);

    private static WriterT<FidelityLog, Fin, Unit> Supports(
        DatabaseIfc db, Map<string, IfcStructuralPointConnection> nodes, Seq<SafSupport> supports, Op key) =>
        supports.TraverseM(support => nodes.Find(support.Node).Match(
                Some: connection => Fidelity.Lift(Boundary(key, () => {
                        connection.AppliedCondition = Condition(db, support.Name, support.Restraint);
                        return unit;
                    }))
                    .Bind(_ => Degraded(support.Name, support.Restraint)),
                None: () => Fidelity.Drop(FidelityDrop.SafResidue, support.Name, unit)))
            .As().Map(static _ => unit);

    private static WriterT<FidelityLog, Fin, Unit> Hinges(
        DatabaseIfc db, Map<string, IfcStructuralPointConnection> nodes,
        Map<string, IfcStructuralCurveMember> members, SafWorkbook admitted, Op key) =>
        admitted.Hinges.TraverseM(hinge => admitted.Ends(hinge)
                .TraverseM(end => (members.Find(hinge.Member), nodes.Find(end))
                    .Apply((member, connection) => Fidelity.Lift(Boundary(key, () => {
                        _ = new IfcRelConnectsStructuralMember(member, connection) {
                            AppliedCondition = Condition(db, hinge.Name, hinge.Restraint),
                        };
                        return unit;
                    })))
                    .IfNone(() => Fidelity.Drop(FidelityDrop.SafResidue, hinge.Name, unit)))
                .As()
                .Bind(_ => Degraded(hinge.Name, hinge.Restraint)))
            .As().Map(static _ => unit);

    private static WriterT<FidelityLog, Fin, Unit> Degraded(string name, SafRestraint restraint) =>
        restraint.Linearized ? Fidelity.Drop(FidelityDrop.SafResidue, $"constraint-linearized:{name}", unit) : Fidelity.Clean(unit);

    private static IfcBoundaryNodeCondition Condition(DatabaseIfc db, string name, SafRestraint restraint) =>
        new(db, name,
            restraint.Translations[0].Translational(), restraint.Translations[1].Translational(), restraint.Translations[2].Translational(),
            restraint.Rotations[0].Rotational(), restraint.Rotations[1].Rotational(), restraint.Rotations[2].Rotational());

    private static Fin<A> Boundary<A>(Op key, Func<A> author) =>
        key.Catch(author);

    // --- [ADMISSION]

    // The WHOLE workbook admitted in one accumulating pass: eight worksheet families, each row's columns
    // accumulating and each family's rows accumulating, so one refusal names every missing cell in the book.
    // This is the ONE crossing where a SAF cell becomes a value — every `?? 0d` that turned an absent
    // coordinate into a point at the origin, an absent thickness into a zero-metre plate, an absent load factor
    // into unity, and an absent reference into the empty key lived past this line and is gone with it.
    private static Fin<SafWorkbook> Admit(ExcelModel model, Op key) =>
        (
            (SafCell.All(Rows<ExcelStructuralPointConnection>(model).Map(row => Point(row, key))),
             SafCell.All(Rows<ExcelStructuralCurveMember>(model).Map(row => Curve(row, key))),
             SafCell.All(Rows<ExcelStructuralSurfaceMember>(model).Map(row => Surface(row, key))),
             SafCell.All(Rows<ExcelStructuralPointSupport>(model).Map(row => Support(row, key))),
             SafCell.All(Rows<ExcelRelConnectsStructuralMember>(model).Map(row => Hinge(row, key))),
             SafCell.All(Rows<ExcelStructuralLoadCase>(model).Map(row => Case(row, key))),
             SafCell.All(Rows<ExcelStructuralLoadCombination>(model).Map(row => Combination(row, key))),
             SafCell.All(Actions(model, key)))
            .Apply((points, curves, surfaces, supports, hinges, cases, combinations, actions) =>
                new SafWorkbook(points, curves, surfaces, supports, hinges, cases, combinations, actions)).As()).ToFin();

    private static Seq<TRow> Rows<TRow>(ExcelModel model) => toSeq(model.Objects.OfType<TRow>());

    private static Validation<Error, SafPoint> Point(ExcelStructuralPointConnection row, Op key) =>
        (SafCell.Text(row.Name, nameof(ExcelStructuralPointConnection), nameof(row.Name), key),
         SafCell.Si(row.X, static c => c.Meters, nameof(ExcelStructuralPointConnection), nameof(row.X), key),
         SafCell.Si(row.Y, static c => c.Meters, nameof(ExcelStructuralPointConnection), nameof(row.Y), key),
         SafCell.Si(row.Z, static c => c.Meters, nameof(ExcelStructuralPointConnection), nameof(row.Z), key))
        .Apply((name, x, y, z) => new SafPoint(name, row.Id, x, y, z)).As();

    private static Validation<Error, SafCurve> Curve(ExcelStructuralCurveMember row, Op key) =>
        (SafCell.Text(row.Name, nameof(ExcelStructuralCurveMember), nameof(row.Name), key),
         SafCell.Text(row.NodeStartName, nameof(ExcelStructuralCurveMember), nameof(row.NodeStartName), key),
         SafCell.Text(row.NodeEndName, nameof(ExcelStructuralCurveMember), nameof(row.NodeEndName), key))
        .Apply((name, start, end) => new SafCurve(name, row.Id, start, end, Optional(row.Behaviour),
            Optional(row.Type).Bind(static type => type.IsOther ? Some(type.ToString()) : None))).As();

    // The outline is a SAF NODE-NAME run and the thickness a declared cell: a surface with fewer than three
    // corners closes no loop, so it refuses here rather than authoring a degenerate face.
    private static Validation<Error, SafSurface> Surface(ExcelStructuralSurfaceMember row, Op key) =>
        (SafCell.Text(row.Name, nameof(ExcelStructuralSurfaceMember), nameof(row.Name), key),
         SafCell.All(toSeq(row.Nodes ?? []).Map(corner => SafCell.Text(corner, nameof(ExcelStructuralSurfaceMember), nameof(row.Nodes), key)))
            .Bind(corners => (corners.Count >= 3
                ? Fin.Succ(corners)
                : Fin.Fail<Seq<string>>(new BimFault.Refused(key, BimScope.Model, BimReason.Rejected, string.Join(':', new object?[] { "saf-invalid", nameof(ExcelStructuralSurfaceMember), nameof(row.Nodes), "under-three" })))).ToValidation()),
         SafCell.Si(row.Thickness?.ThicknessFirst, static c => c.Meters, nameof(ExcelStructuralSurfaceMember), nameof(row.Thickness), key))
        // The ONE surviving string coalesce on this page and the only lawful one: IFC requires a material NAME
        // where SAF leaves it optional, so an unnamed SAF plate authors an unnamed IfcMaterial rather than
        // refusing a geometrically complete surface over a label the standard does not demand.
        .Apply((name, corners, thickness) => new SafSurface(name, row.Id, corners, row.Material ?? "", thickness,
            Optional(row.Type).Bind(static type => type.IsOther ? Some(type.ToString()) : None))).As();

    private static Validation<Error, SafSupport> Support(ExcelStructuralPointSupport row, Op key) =>
        (SafCell.Text(row.Name, nameof(ExcelStructuralPointSupport), nameof(row.Name), key),
         SafCell.Text(row.Node, nameof(ExcelStructuralPointSupport), nameof(row.Node), key))
        .Apply((name, node) => new SafSupport(name, node, new SafRestraint(
            Seq(Cell(row.TranslationXType, row.TranslationXStiffness?.NewtonsPerMeter),
                Cell(row.TranslationYType, row.TranslationYStiffness?.NewtonsPerMeter),
                Cell(row.TranslationZType, row.TranslationZStiffness?.NewtonsPerMeter)),
            Seq(Cell(row.RotationXType, row.RotationXStiffness?.NewtonMetersPerRadian),
                Cell(row.RotationYType, row.RotationYStiffness?.NewtonMetersPerRadian),
                Cell(row.RotationZType, row.RotationZStiffness?.NewtonMetersPerRadian))))).As();

    private static Validation<Error, SafHinge> Hinge(ExcelRelConnectsStructuralMember row, Op key) =>
        (SafCell.Text(row.Name, nameof(ExcelRelConnectsStructuralMember), nameof(row.Name), key),
         SafCell.Text(row.Member, nameof(ExcelRelConnectsStructuralMember), nameof(row.Member), key))
        .Apply((name, member) => new SafHinge(name, member, Optional(row.Position), new SafRestraint(
            Seq(Cell(row.TranslationXType, row.TranslationXStiffness?.NewtonsPerMeter),
                Cell(row.TranslationYType, row.TranslationYStiffness?.NewtonsPerMeter),
                Cell(row.TranslationZType, row.TranslationZStiffness?.NewtonsPerMeter)),
            Seq(Cell(row.RotationXType, row.RotationXStiffness?.NewtonMetersPerRadian),
                Cell(row.RotationYType, row.RotationYStiffness?.NewtonMetersPerRadian),
                Cell(row.RotationZType, row.RotationZStiffness?.NewtonMetersPerRadian))))).As();

    // A DOF cell pair is genuinely OPTIONAL — an unset SAF constraint column means the axis is unconstrained,
    // which the null Type already states — so it admits without refusing while the magnitude beside it stays
    // zero-valued only where the type says Rigid or Free, never where it says Flexible.
    private static SafConstraint Cell(ExcelConstraintType? type, double? si) => new(type, si ?? 0d);

    private static Validation<Error, SafCase> Case(ExcelStructuralLoadCase row, Op key) =>
        SafCell.Text(row.Name, nameof(ExcelStructuralLoadCase), nameof(row.Name), key)
            .Map(name => new SafCase(name, row.ActionType switch {
                ExcelActionType.Permanent => IfcActionTypeEnum.PERMANENT_G,
                ExcelActionType.Accidental => IfcActionTypeEnum.EXTRAORDINARY_A,
                ExcelActionType.Variable => IfcActionTypeEnum.VARIABLE_Q,
                _ => IfcActionTypeEnum.NOTDEFINED,
            }, SafCaseType.SourceOf(Optional(row.LoadType)))).As();

    // The factor run pairs POSITIONALLY with the case run, so a short or absent factor array is a malformed
    // combination that refuses: the retired `factor ?? 1d` fabricated a unit multiplier on every missing cell,
    // which reads downstream as a deliberate unfactored action nothing retracts.
    private static Validation<Error, SafCombination> Combination(ExcelStructuralLoadCombination row, Op key) =>
        (SafCell.Text(row.Name, nameof(ExcelStructuralLoadCombination), nameof(row.Name), key),
         SafCell.All(toSeq(row.LoadCases ?? []).Map(c => SafCell.Text(c, nameof(ExcelStructuralLoadCombination), nameof(row.LoadCases), key))),
         SafCell.All(toSeq(row.LoadFactors ?? []).Map(f =>
             (Optional(f).ToFin(new BimFault.Refused(key, BimScope.Model, BimReason.Rejected, string.Join(':', new object?[] { "saf-invalid", nameof(ExcelStructuralLoadCombination), nameof(row.LoadFactors), "absent" })))).ToValidation())))
        .Apply((name, cases, factors) => new SafCombination(name, cases, factors,
            row.Category == ExcelLoadCaseCombinationCategory.UltimateLimitState))
        .Bind(combination => (combination.Cases.Count == combination.Factors.Count
            ? Fin.Succ(combination)
            : Fin.Fail<SafCombination>(new BimFault.Refused(key, BimScope.Model, BimReason.Rejected, string.Join(':', new object?[] { "saf-invalid", nameof(ExcelStructuralLoadCombination), combination.Name, "factor-arity" })))).ToValidation()).As();

    // The four action worksheets admit into ONE carrier keyed by the LoadFamily row they author, so the
    // authoring fold below dispatches once over the closed family instead of four times over four shapes.
    private static Seq<Validation<Error, SafAction>> Actions(ExcelModel model, Op key) =>
        Rows<ExcelStructuralPointAction>(model).Map(row => Action(
            row.Name, row.LoadCase, LoadFamily.SingleForce, row.ReferenceNode, row.CoordinateSystem, ExcelLocation.Length,
            Vector(row.DirectionVector, static q => q.Newtons), None, None, nameof(ExcelStructuralPointAction), key))
        + Rows<ExcelStructuralCurveAction>(model).Map(row => row.Distribution == ExcelCurveDistribution.Trapezoidal
            ? Ramp(row, key)
            : Action(row.Name, row.LoadCase, LoadFamily.LinearForce, row.Member, row.CoordinateSystem, row.Location,
                Vector(row.DirectionVector, static q => q.NewtonsPerMeter), None, None, nameof(ExcelStructuralCurveAction), key))
        + Rows<ExcelStructuralSurfaceAction>(model).Map(row => Action(
            row.Name, row.LoadCase, LoadFamily.PlanarForce, row.Member2DReference, row.CoordinateSystem, ExcelLocation.Length,
            Directed(row.Direction, Optional(row.Value).Map(static v => v.Pascals)), None, None,
            nameof(ExcelStructuralSurfaceAction), key))
        + Rows<ExcelStructuralCurveActionThermal>(model).Map(row => Action(
            row.Name, row.LoadCase, LoadFamily.Temperature, row.Member, ExcelCoordinateSystem.Global, ExcelLocation.Length,
            Optional(row.DeltaT).Map(static t => Seq(t.Kelvins, 0d, 0d)), None, None,
            nameof(ExcelStructuralCurveActionThermal), key));

    // A trapezoid whose span cells are unset is the fabrication the retired `?? 0d`/`?? 1d` pair made
    // unobservable: a ramp claiming the member's full length. Both bounds admit or the row refuses.
    private static Validation<Error, SafAction> Ramp(ExcelStructuralCurveAction row, Op key) =>
        (Components(Vector(row.DirectionVector, static q => q.NewtonsPerMeter), nameof(ExcelStructuralCurveAction), key),
         Components(Vector(row.DirectionVector2, static q => q.NewtonsPerMeter), nameof(ExcelStructuralCurveAction), key),
         SafCell.Text(row.Name, nameof(ExcelStructuralCurveAction), nameof(row.Name), key),
         SafCell.Text(row.LoadCase, nameof(ExcelStructuralCurveAction), nameof(row.LoadCase), key),
         SafCell.Text(row.Member, nameof(ExcelStructuralCurveAction), nameof(row.Member), key),
         SafCell.Number(row.StartPoint, nameof(ExcelStructuralCurveAction), nameof(row.StartPoint), key),
         SafCell.Number(row.EndPoint, nameof(ExcelStructuralCurveAction), nameof(row.EndPoint), key))
        .Apply((start, end, name, loadCase, member, from, to) => new SafAction(
            name, loadCase, LoadFamily.Configuration, member,
            row.CoordinateSystem != ExcelCoordinateSystem.Local, row.Location == ExcelLocation.Projection,
            start + end, Some(from), Some(to))).As();

    private static Validation<Error, SafAction> Action(
        string? name, string? loadCase, LoadFamily family, string? target, ExcelCoordinateSystem system,
        ExcelLocation location, Option<Seq<double>> components, Option<double> start, Option<double> end,
        string worksheet, Op key) =>
        (SafCell.Text(name, worksheet, nameof(SafAction.Name), key),
         SafCell.Text(loadCase, worksheet, nameof(SafAction.Case), key),
         SafCell.Text(target, worksheet, nameof(SafAction.Target), key),
         Components(components, worksheet, key))
        .Apply((admitted, admittedCase, admittedTarget, values) => new SafAction(
            admitted, admittedCase, family, admittedTarget,
            system != ExcelCoordinateSystem.Local, location == ExcelLocation.Projection, values, start, end)).As();

    private static Validation<Error, Seq<double>> Components(Option<Seq<double>> components, string worksheet, Op key) =>
        (components.ToFin(new BimFault.Refused(key, BimScope.Model, BimReason.Rejected, string.Join(':', new object?[] { "saf-invalid", worksheet, "components", "absent" })))).ToValidation();

    private static Option<Seq<double>> Vector<TQuantity>(
        ExcelLoadDirectionVector<TQuantity>? vector, Func<TQuantity, double> si) where TQuantity : struct, IQuantity =>
        from carried in Optional(vector)
        from x in Optional(carried.X)
        from y in Optional(carried.Y)
        from z in Optional(carried.Z)
        select Seq(si(x), si(y), si(z));

    // SAF's surface cell is single-valued per DIRECTION, so the admitted triple places the magnitude on the
    // named axis and states zero on the other two — a declared component of the same row, never a fabricated
    // measurement, because the worksheet asserts exactly one direction per action.
    private static Option<Seq<double>> Directed(ExcelActionDirection? direction, Option<double> magnitude) =>
        magnitude.Map(value => direction switch {
            ExcelActionDirection.X => Seq(value, 0d, 0d),
            ExcelActionDirection.Y => Seq(0d, value, 0d),
            _ => Seq(0d, 0d, value),
        });

    // --- [ENTITY_BUILDERS]

    private static Map<string, IfcStructuralPointConnection> Points(
        DatabaseIfc db, IfcStructuralAnalysisModel analysis, Seq<SafPoint> points) =>
        toMap(points.Map(point => (point.Name, new IfcStructuralPointConnection(analysis,
            new IfcVertexPoint(new IfcCartesianPoint(db, point.X, point.Y, point.Z))) { Name = point.Name })));

    private static Map<string, IfcStructuralCurveMember> Curves(
        DatabaseIfc db, IfcStructuralAnalysisModel analysis, Seq<SafCurve> curves,
        Map<string, IfcStructuralPointConnection> nodes) =>
        toMap(curves.Map(static (row, ordinal) => (Row: row, Ordinal: ordinal))
            .Choose(pair =>
                from a in nodes.Find(pair.Row.StartNode)
                from b in nodes.Find(pair.Row.EndNode)
                select (pair.Row.Name, new IfcStructuralCurveMember(analysis, a, b, new IfcDirection(db, 0, 0, 1), pair.Ordinal + 1) {
                    Name = pair.Row.Name,
                    PredefinedType = CurveVariety.VarietyOf(pair.Row.Behaviour),
                    ObjectType = pair.Row.Role.IfNone(""),
                })));

    // The SAF outline nodes close an IfcPolyLoop on a plane through the loop — the analytical face, not a
    // display body — with the admitted constant thickness.
    private static Map<string, IfcStructuralSurfaceMember> Surfaces(
        DatabaseIfc db, IfcStructuralAnalysisModel analysis, Seq<SafSurface> surfaces,
        Map<string, IfcStructuralPointConnection> nodes) =>
        toMap(surfaces.Map(static (row, ordinal) => (Row: row, Ordinal: ordinal))
            .Choose(pair => pair.Row.Corners.Traverse(nodes.Find).As()
                .Map(corners => {
                    Seq<IfcCartesianPoint> outline = corners.Map(static corner => (IfcCartesianPoint)((IfcVertexPoint)corner.Vertex).VertexGeometry);
                    return (pair.Row.Name, new IfcStructuralSurfaceMember(
                        analysis,
                        new IfcFaceSurface(
                            new IfcFaceOuterBound(new IfcPolyLoop(outline), true),
                            new IfcPlane(new IfcAxis2Placement3D(outline[0])), true),
                        new IfcMaterial(db, pair.Row.Material), pair.Ordinal + 1,
                        pair.Row.Thickness) { Name = pair.Row.Name });
                })));

    private static Map<string, IfcStructuralLoadCase> LoadCases(IfcStructuralAnalysisModel analysis, Seq<SafCase> cases) =>
        toMap(cases.Map(row => (row.Name, new IfcStructuralLoadCase(analysis, row.Name) {
            ActionType = row.Nature, ActionSource = row.Source,
        })));

    private static Unit Combinations(
        IfcStructuralAnalysisModel analysis, Map<string, IfcStructuralLoadCase> cases, Seq<SafCombination> combinations) =>
        combinations.Iter(row => ignore(new IfcStructuralLoadGroup(analysis, row.Name,
            row.Factors.ToList(),
            row.Cases.Choose(cases.Find).Map(static loadCase => (IfcStructuralLoadGroup)loadCase).ToList(),
            ULS: row.Ultimate)));

    // ONE authoring dispatch over the closed LoadFamily roster the export lowering also keys on: the four
    // worksheet shapes arrived as one admitted carrier, so the arms differ only in which GG load they mint and
    // which host they attach to.
    private static Unit Applied(
        DatabaseIfc db, Map<string, IfcStructuralLoadCase> cases,
        Map<string, IfcStructuralPointConnection> nodes, Map<string, IfcStructuralCurveMember> members,
        Map<string, IfcStructuralSurfaceMember> surfaces, SafWorkbook admitted) =>
        admitted.Actions.Iter(action => cases.Find(action.Case).Iter(loadCase => action.Family.Switch(
            singleForce: () => nodes.Find(action.Target).Iter(at => ignore(new IfcStructuralPointAction(loadCase, at,
                new IfcStructuralLoadSingleForce(db, action.Components[0], action.Components[1], action.Components[2]),
                action.Global))),
            linearForce: () => members.Find(action.Target).Iter(member => ignore(new IfcStructuralCurveAction(loadCase, member,
                Line(db, action.Components), action.Global, action.Projected, IfcStructuralCurveActivityTypeEnum.CONST))),
            configuration: () => members.Find(action.Target).Iter(member => ignore(new IfcStructuralCurveAction(loadCase, member,
                new IfcStructuralLoadConfiguration(
                    Line(db, action.Components.Take(3)), action.Start.IfNone(0d),
                    Line(db, action.Components.Skip(3)), action.End.IfNone(0d)),
                action.Global, action.Projected, IfcStructuralCurveActivityTypeEnum.LINEAR))),
            planarForce: () => surfaces.Find(action.Target).Iter(surface => ignore(new IfcStructuralSurfaceAction(loadCase, surface,
                new IfcStructuralLoadPlanarForce(db) {
                    PlanarForceX = action.Components[0], PlanarForceY = action.Components[1], PlanarForceZ = action.Components[2],
                },
                action.Global, projected: action.Projected, IfcStructuralSurfaceActivityTypeEnum.CONST))),
            temperature: () => members.Find(action.Target).Iter(member => ignore(new IfcStructuralCurveAction(loadCase, member,
                new IfcStructuralLoadTemperature(db, action.Components[0], 0d, 0d),
                action.Global, projected: action.Projected, IfcStructuralCurveActivityTypeEnum.CONST))),
            displacement: static () => unit)));

    private static IfcStructuralLoadLinearForce Line(DatabaseIfc db, Seq<double> components) =>
        new(db, components[0], components[1], components[2], 0d, 0d, 0d);
}
```

## [03]-[RESEARCH]

(none)

# [ELEMENT_GEO_REFERENCE]

The host-neutral coordinate-reference owner: one `GeoReference` record carrying the full map-conversion-and-CRS state — the eastings/northings/orthogonal-height translation, the X-axis abscissa/ordinate rotation cosine pair, the per-axis `ScaleX`/`ScaleY`/`ScaleZ`, the `GeodeticDatum`/`VerticalDatum` names, one `ProjectedCrs` `[ComplexValueObject]` CRS identity, and the `Option<double> Epoch` decimal-year coordinate epoch (the dynamic-datum/ITRF plate-motion anchor the Bim OSR leg threads through `SpatialReference.SetCoordinateEpoch`; no IFC attribute carries it, so an IFC ingest lands `None` and a survey/GIS ingest supplies it) — and the `CrsResolution` `[SmartEnum<string>]` policy column that records HOW the CRS resolves (an EPSG authority code, an inline OGC WKT definition, a `MapProjection`+`MapZone` projection identity — round-trippable identification no engine builds a transform from, its own typed mode — or no reference at all) — every accepted mode carrying exactly the evidence its consumer reads. The record constructor is PRIVATE: `Admit` and the pre-admitted `Identity` are the only entries, so an unadmitted frame is unrepresentable. The reference is HOST-NEUTRAL and PURE DATA: it carries the parameters a downstream `Rasm.Bim` projector folds into a rigid map-conversion transform (over the kernel transform algebra) and a `ProjNET`/OSR datum-to-datum reprojection — the seam references NO ProjNET, NO kernel `Transform` type, and materializes no geometry, because geometry is referenced by content hash. The translation and per-axis scale doubles are METRE-NORMALIZED at ingest: the `Rasm.Bim` projector composes the `IfcProjectedCRS.MapUnit` `IfcNamedUnit.SIFactor()` model-unit↔CRS-unit factor onto the scale BEFORE handing the tuple to `Admit`, so the seam frame is one metre frame a federation reconciles every model onto from one record and the seam carries NO `MapUnit` field (a US-survey-foot State Plane zone is reconciled to metres in Bim, never left as an ambiguous CRS-native double on the seam).

The CRS is a THREE-STATE concept, not the two-state EPSG/none slice the migration source modeled: an `IfcProjectedCRS` may carry an EPSG authority `Name` (`EPSG:25832`), an inline `IfcWellKnownText.WellKnownText` OGC WKT definition (a GIS-origin CRS with NO authority code), or — for a non-georeferenced model — neither. The `ProjectedCrs` `[ComplexValueObject]` carries the authority `Name`, the parsed `Epsg`, the `Wkt` definition, and the `MapProjection`/`MapZone` projection identity together, and `CrsResolution` discriminates the state a consumer reads as a column. The `Admit` factory is the tuple's VALUE-ADMISSION gate: the independent legs accumulate — the translation finite, the direction-cosine pair finite and non-degenerate, the per-axis scales finite and strictly positive, the optional epoch finite and positive, and the CRS leg — so a NaN eastings or a zero scale faults BESIDE a bad CRS rather than canonicalizing stably into `Header.CanonicalBytes` as a silently-mislocated frame; the CRS leg FAULTS (not silently skips) ONLY when a CRS name is present but resolves to NEITHER an EPSG code NOR a WKT definition NOR a projection+zone — an unresolvable georeference surfaces as `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` rather than a silently-mislocated model — and a WKT-defined CRS is VALID (the `ProjNET` `CoordinateSystemFactory.CreateFromWkt` / OSR `SpatialReference.ImportFromWkt` resolves it), never faulted as "unresolvable". `GeoReference` rides ONLY the `Graph/element#ELEMENT_GRAPH` `Header` and the `Geospatial/coverage#COVERAGE_NODE` `Coverage` node — it is DROPPED from the `Object` node, because an object's placement is geometry the kernel owns by content hash, and the model-wide georeference is a header fact, not a per-object one. The page composes the kernel `Op` op-key and, for the coverage-node content identity, the `Projection/address#CANONICAL_WRITER` `CanonicalWriter` (a coverage's `NodeId` derives from its CRS, so `GeoReference` owns the `CanonicalBytes` projection of its full state), folding the `Epsg` AND the `Wkt`/`MapProjection`/`MapZone` so two EPSG-less WKT CRSs that differ only in their WKT address as the distinct frames they are.

## [01]-[INDEX]

- [01]-[GEO_REFERENCE]: the `GeoReference` map-conversion-and-CRS record, the `ProjectedCrs` `[ComplexValueObject]` three-state CRS identity (authority `Name`+parsed `Epsg`, inline `Wkt`, `MapProjection`/`MapZone`), the `CrsResolution` `[SmartEnum<string>]` resolution-mode policy column, the accumulating `Admit` factory (finite-translation / non-degenerate-direction / positive-scale metre-frame gates + the fault-on-fully-unresolvable CRS leg), and the `RotationRadians` direction-cosine projection.

## [02]-[GEO_REFERENCE]

- Owner: `GeoReference` the host-neutral coordinate-reference record carrying the metre-normalized map-conversion-and-CRS state; `ProjectedCrs` the `[ComplexValueObject]` CRS identity carrying the authority `Name` (across the `EPSG:NNNN`/`urn:ogc:def:crs:EPSG::NNNN`/`urn:ogc:def:crs:EPSG:6.18.3:NNNN`/authority forms), the parsed `Epsg`, the inline `Wkt` OGC definition, and the `MapProjection`/`MapZone` projection identity, structurally admitted so a CRS with neither an EPSG nor a WKT nor a projection is the rejected form; `CrsResolution` the `[SmartEnum<string>]` resolution-mode column (`Epsg`/`Wkt`/`Projection`/`Unreferenced`) — `Projection` the mode a `MapProjection`+`MapZone` identity with no WKT payload takes, so the WKT build path never receives an empty payload.
- Entry: `GeoReference.Admit(...)` admits the tuple through its accumulating K-kinded `K<Validation<Error>,_>` legs (the `[ADMISSION_SLOTS]` compile contract — the shipped tuple `.Apply` binds only on `(K<F,A>, …)` receivers) collapsed once to the `Fin<T>` rail — the translation finite, the direction-cosine pair finite and non-degenerate then normalized to one unit vector (scalar-equivalent cosine pairs mint one frame), the per-axis scales finite and strictly positive (zero collapses, negative mirrors), the optional coordinate epoch finite and positive when present (the trailing `Option<double> epoch = default` arg — a decimal-year ITRF/dynamic-datum epoch no IFC attribute carries, so the Bim projector passes none and a GIS/survey ingest supplies it named), and the CRS leg building the `ProjectedCrs` from the CRS `Name`/`Wkt`/`MapProjection`/`MapZone` and railing `ElementFault.ValueRejected` ONLY when a non-blank CRS name (or a non-blank WKT) carries no recoverable identity at all (no EPSG, no WKT, no projection+zone — never a silent skip); `GeoReference.Identity` the non-georeferenced default a model without a map-conversion carries so ingest never blocks; `IsGeoreferenced` tests a present CRS (EPSG OR WKT) or a non-identity map conversion (offset, rotation, or per-axis scale — the origin-only test blind to a rotation-only conversion is the deleted form); `Resolution` reads the `CrsResolution` state a downstream transform consumer branches on; `RotationRadians` projects the admitted unit X-axis direction onto a rotation angle (`Atan2(ordinate, abscissa)`); `Scale` reads the uniform map-conversion scale when the three axes agree (the IFC single-`Scale` egress read), `Option<double>.None` when they differ.
- Auto: `Admit` gates the metre-frame doubles (finite translation, finite non-degenerate direction pair, finite positive scales — each one named accumulating slot), trims the datum tokens at the one boundary, and builds the CRS through `ProjectedCrs.Of` — a blank `Name` AND blank `Wkt` yields the no-CRS `None`/`Unreferenced` state (valid, so a non-georeferenced model never blocks), a name resolving to an EPSG code yields the `Epsg` state, a WKT-only CRS (blank or unresolvable `Name`, non-blank `Wkt`) yields the `Wkt` state (VALID — ProjNET/OSR resolve WKT), and a name+projection+zone with no EPSG and no WKT yields the `Projection` state carrying its `MapProjection`/`MapZone` tokens (typed identification the Bim datum leg faults by CASE as transform-unbuildable — never a `Wkt`-labelled empty payload); only a name (or WKT marker) present with no EPSG, no WKT, and no projection+zone faults `ElementFault.ValueRejected`; `ProjectedCrs.ValidateFactoryArguments` trims the `Name`/`Wkt`/`MapProjection`/`MapZone` and rejects the all-blank product so the structural identity is never empty; `ProjectedCrs.Epsg` matches the `EPSG:` prefix, the OGC URN colon tail (one arm owns both the `::NNNN` and versioned `:6.18.3:NNNN` forms, gated on the `EPSG` authority token so an `ESRI:NNNN` or foreign-authority URN never mis-parses as an EPSG identity), and a bare integer, returning `Option<int>`; `RotationRadians` is the pure direction-cosine-to-angle projection (the IFC convention carrying the rotation as a direction rather than an angle), no kernel transform materialized.
- Receipt: the `GeoReference` is the coordinate-reference evidence the `Header` and a `Coverage` node carry; a downstream `Rasm.Bim` projector reads the metre-frame tuple to build the rigid map-conversion transform over the kernel `Transform` algebra and to drive the `ProjNET` `CoordinateSystemServices` (EPSG-keyed) OR `CoordinateSystemFactory.CreateFromWkt` (WKT-keyed) datum-to-datum reprojection, escalating to the OSR PROJ engine for what the managed algebra cannot express — the seam carrying only the parameters, the host-neutral rotation scalar, and the `CrsResolution` mode that selects the EPSG-vs-WKT transform-build path.
- Packages: Thinktecture.Runtime.Extensions (`[ComplexValueObject]` + `[ValidationError<ElementFault>]` so the generated `Validate`/`TryCreate`/`Create` and the `ref ElementFault?` `ValidateFactoryArguments` admission hook rail the seam fault, the `[SmartEnum<string>]` + the generated `Key`/`TryGet` for `CrsResolution`, the `[MemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]` for the case-insensitive CRS `Name`/`MapProjection`/`MapZone` equality and the `[MemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]` for the byte-exact `Wkt`, the `[StructLayout(LayoutKind.Auto)]` on the multi-member readonly struct), LanguageExt.Core (`Option`/`Fin`/`Bind` + `Validation<Error,_>` the accumulating `Admit` legs ride — the tuple `.Apply` join, `.ToValidation()` lifting the CRS leg, `.As().ToFin()` collapsing once — + the implicit `Error`→`Fin`/`Validation` lift the bare `ElementFault` case rides), `Projection/address#CANONICAL_WRITER` (`CanonicalWriter` the `CanonicalBytes` CRS projection writes through), `Rasm` (the kernel `Op` op-key + `Op.Of` the keyless-fault re-key).
- Growth: a new map-conversion parameter is one column on `GeoReference` a projector folds into the transform; a new CRS-name scheme is one arm on `ProjectedCrs.Epsg`; a new CRS-identity carrier (a future EPSG-coordinate-epoch for a dynamic datum) is one member on `ProjectedCrs` + one `CrsResolution` row; a new datum is the projector's `ProjNET`/OSR concern resolved from the EPSG code or WKT; never a per-CRS class, never a parallel WKT-vs-EPSG `GeoReference` family, and never a transform owner on the seam.
- Boundary: `GeoReference` is HOST-NEUTRAL pure data — a kernel `Transform` field, a `ProjNET` `MathTransform`, an OSR `SpatialReference`, or a host coordinate type on the seam is the named seam violation, the transform materialization and the datum reprojection being the `Rasm.Bim` projector's concern over the kernel transform algebra and the admitted `ProjNET`/`MaxRev.Gdal.Core` engines; the translation/scale doubles are METRE-NORMALIZED (the `Rasm.Bim` projector composes the `IfcProjectedCRS.MapUnit` `IfcNamedUnit.SIFactor()` factor onto the scale at ingest), so a CRS-native-unit double on the seam, a `MapUnit` field on the tuple, or a unit-bearing `MeasureValue` on the translation is the rejected form — the rotation cosines and scales are dimensionless and the translation is metres, so the tuple stays a flat metre-frame parameter record; the CRS is a THREE-STATE `[ComplexValueObject]` (EPSG / WKT-or-projection / unreferenced) and the two-state `Option<int> Epsg` slice that faults a fully-resolvable WKT CRS is the deleted form (a GIS-origin IFC carrying `IfcWellKnownText` with no authority code blocks ingest under the old slice); every `CrsResolution` mode carries exactly the evidence its consumer reads — `Epsg` the code, `Wkt` a non-empty payload, `Projection` the `MapProjection`/`MapZone` identification the egress round-trips and the datum leg faults by case — so a projection-identified CRS routed down the WKT arm with an empty payload is the deleted payload-less label; the record constructor is PRIVATE and `Admit`/`Identity` the only entries, a positional public ctor beside the gate being the deleted bypass; the `Admit` factory is the tuple's ONE value-admission gate — the translation/direction/scale legs refuse a non-finite, direction-less, or non-positive-scale conversion (the `CanonicalWriter` canonicalizes a NaN stably, so an ungated NaN mints a stable identity for a meaningless frame — admission, not the codec, owns the rejection) and it FAULTS when a CRS name is present but resolves to NO identity at all (no EPSG, no WKT, no projection+zone) rather than silently skipping, so a mislocated model is a typed fault the ingest surfaces, while a WKT-resolvable or projection-bearing CRS is VALID; the seam owns the CRS-identity VOCABULARY (the `Name`/`Epsg`/`Wkt`/`MapProjection`/`MapZone` carry + the EPSG parse) but NOT the transform build (the EPSG-vs-WKT `ProjNET`/OSR transform construction is the Bim projector's, selected off `Resolution`); `GeoReference` rides ONLY the `Header` and the `Coverage` node and is DROPPED from the `Object` node — an object's placement is content-hashed geometry the kernel owns, the model-wide georeference a header fact; `CanonicalBytes` folds the `Epsg` AND the `Wkt`/`MapProjection`/`MapZone` so two EPSG-less CRSs differing only in WKT address distinctly (an `Epsg`-only canon that drops the WKT is the deleted form that collides every WKT CRS onto one identity).

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Runtime.InteropServices;
using LanguageExt;
using LanguageExt.Traits;
using Rasm.Domain;
using Rasm.Element.Projection;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element.Geospatial;

// --- [TYPES] ------------------------------------------------------------------------------
// HOW a CRS resolves to a transform-buildable identity — the policy column the downstream Rasm.Bim transform owner
// branches on to select the ProjNET EPSG-keyed CoordinateSystemServices path vs the WKT-keyed CreateFromWkt path,
// read as a column rather than re-branching `Epsg.IsSome` per consumer. Unreferenced is the non-georeferenced model
// (Identity); a CRS is never half-resolved — a constructed ProjectedCrs is Epsg or Wkt, GeoReference.Identity is Unreferenced.
[SmartEnum<string>]
public sealed partial class CrsResolution {
 public static readonly CrsResolution Unreferenced = new("unreferenced"); // no map conversion / no CRS — Identity, the no-transform leg
 public static readonly CrsResolution Epsg = new("epsg");                 // an EPSG authority code resolves the CRS (the ProjNET CoordinateSystemServices path)
 public static readonly CrsResolution Wkt = new("wkt");                   // an inline OGC WKT payload defines it, no authority code (the CreateFromWkt path)
 public static readonly CrsResolution Projection = new("projection");     // a MapProjection+MapZone identity with NO WKT payload — identification evidence, engine-unbuildable

 // The build path is the CASE ITSELF — the Rasm.Bim GeoTransform owner dispatches the GENERATED total `Switch`
 // (`resolution.Switch(epsg: …, wkt: …, projection: …, unreferenced: …)`) to select the EPSG-keyed
 // CoordinateSystemServices build, the WKT-keyed CreateFromWkt build, the typed projection-only verdict (a bare
 // MapProjection+MapZone identity is round-trippable IDENTIFICATION the egress re-emits, but NEITHER managed
 // engine builds a transform from it — the Bim leg faults `crs-projection-only-unbuildable` on the CASE instead
 // of sniffing an empty Wkt payload, the deleted payload-less Wkt label), or the no-transform leg, so a new
 // resolution mode breaks every build site at compile time. A `BuildsByEpsg`/`BuildsByWkt` boolean pair the
 // consumer chains as `if (resolution.BuildsByEpsg)` is the deleted COLLAPSE_SCAN [04] re-branch — the smart-enum
 // owns the dispatch, never a derived bool that re-states the case as a flag and silently misses the next mode.
 // The seam owns the DISCRIMINANT; Bim owns the ProjNET/OSR build and the per-mode verdict.
}

// The CRS identity a THREE-STATE [ComplexValueObject], NOT the two-state Option<int> Epsg slice the migration source
// carried: an IfcProjectedCRS may carry an EPSG authority Name, an inline IfcWellKnownText.WellKnownText OGC definition
// (a GIS-origin CRS with NO authority code), or a MapProjection+MapZone projection identity. All four carriers (Name,
// the parsed Epsg, Wkt, MapProjection/MapZone) ride ONE value-object so a consumer reads the whole CRS identity in one
// hop and Resolution discriminates the EPSG-vs-WKT path. Identity is the (Name, MapProjection, MapZone, Wkt) product
// under SPLIT comparer policy (the authority Name/MapProjection/MapZone case-insensitive — CRS authority tokens are
// case-stable; the Wkt byte-exact — a WKT is a structured definition, not a case-fold token), mirroring the sibling
// Classification/classification#CLASSIFICATION_AXIS Classification axis (the other neutral cross-cutting value-object).
// The all-blank product is the rejected form — a CRS with no identity at all is not constructible.
[ComplexValueObject]
[ValidationError<ElementFault>]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct ProjectedCrs {
 [MemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>] public string Name { get; }
 [MemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>] public string MapProjection { get; }
 [MemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>] public string MapZone { get; }
 [MemberEqualityComparer<ComparerAccessors.StringOrdinal, string>] public string Wkt { get; }

 // Trim the four CRS carriers in place, then reject the STRUCTURALLY empty product: a value with NO authority Name, NO
 // inline Wkt, NO MapProjection, and NO MapZone carries no identity. The keyless ValidateFactoryArguments fault is
 // re-stamped to the caller's Op in `Of` (the Classification/classification#CLASSIFICATION_AXIS re-key idiom). This is
 // the STRUCTURAL gate (some identity string present); the SEMANTIC resolvability (EPSG OR Wkt OR projection+zone — a
 // name-only "GibberishName" is structurally valid but semantically unresolvable) is GeoReference.Admit's richer gate.
 private static partial void ValidateFactoryArguments(ref ElementFault? validationError, ref string name, ref string mapProjection, ref string mapZone, ref string wkt) {
  (name, mapProjection, mapZone, wkt) = (name.Trim(), mapProjection.Trim(), mapZone.Trim(), wkt.Trim());
  if (name.Length == 0 && mapProjection.Length == 0 && mapZone.Length == 0 && wkt.Length == 0) {
   validationError = ElementFault.ValueRejected(Op.Of(name: nameof(ProjectedCrs)), "projected CRS must carry an authority name, an inline WKT, or a projection+zone");
  }
 }

 // The seam-rail admission a Rasm.Bim projector takes: re-keys the keyless ValidateFactoryArguments fault to the
 // caller's Op so the operation context survives (the Classification/classification#CLASSIFICATION_AXIS `Of` idiom).
 public static Fin<ProjectedCrs> Of(string name, string mapProjection, string mapZone, string wkt, Op key) =>
  Validate(name, mapProjection, mapZone, wkt, out ProjectedCrs value) is { } fault
   ? ElementFault.ValueRejected(key, fault.Message)
   : Fin.Succ(value);

 // EPSG resolution: the EPSG:NNNN prefix, ONE colon-tail arm owning both OGC URN forms (::NNNN and the versioned
 // :6.18.3:NNNN — the code is the last-colon tail and the EXACT authority segment sits third-from-last in both
 // arities), and a bare authority code. The URN arm gates on segment EQUALITY with the EPSG authority token, so an
 // ESRI:NNNN, a foreign-authority URN, or a name merely CONTAINING "EPSG" as a substring never mis-parses as an
 // EPSG identity — it resolves through the Wkt/projection path. A WKT-only or projection-only CRS is None.
 public Option<int> Epsg =>
  Name.StartsWith("EPSG:", StringComparison.OrdinalIgnoreCase) && int.TryParse(Name.AsSpan(5), out int prefixed)
  ? Some(prefixed)
  : Name.Split(':') is { Length: >= 3 } segs
    && segs[^3].Equals("EPSG", StringComparison.OrdinalIgnoreCase)
    && int.TryParse(segs[^1], out int urnCode)
  ? Some(urnCode)
  : int.TryParse(Name, out int bare)
  ? Some(bare)
  : None;

 // SEMANTIC resolvability: a CRS a transform can actually be built for — an EPSG code, an inline WKT, OR a complete
 // projection+zone (a synthesizable WKT). A name-only "GibberishName" passes the STRUCTURAL ValidateFactoryArguments
 // (a non-blank Name) yet is NOT resolvable here, so GeoReference.Admit reads this property to fault it; the Rasm.Bim
 // transform owner reads the SAME property before attempting a build. The predicate lives on the CRS owner, never
 // re-spelled inline at Admit or in Bim (the ONE_HOP_RESOLUTION owner co-location).
 public bool IsResolvable => Epsg.IsSome || Wkt.Length > 0 || (MapProjection.Length > 0 && MapZone.Length > 0);

 // The resolution mode: an EPSG code wins (the authority is the densest identity a ProjNET CoordinateSystemServices
 // build keys on); a WKT-carrying CRS resolves through the CreateFromWkt path; a projection+zone identity with NO
 // WKT payload takes its OWN Projection mode — identification the egress round-trips, transform-unbuildable by
 // either engine, so the Bim leg faults it by CASE rather than receiving a Wkt label over an empty payload (every
 // accepted mode carries exactly the evidence its consumer reads). Never Unreferenced — that state is
 // GeoReference.Identity, which carries no CRS.
 public CrsResolution Resolution =>
  Epsg.IsSome ? CrsResolution.Epsg
  : Wkt.Length > 0 ? CrsResolution.Wkt
  : CrsResolution.Projection;

 // The CRS-identity content projection GeoReference.CanonicalBytes folds: the parsed Epsg (the dense identity when
 // present, so two names resolving the same EPSG code address identically whether or not the name string differs) AND
 // the Wkt/MapProjection/MapZone (so two EPSG-LESS CRSs differing only in WKT or projection address distinctly — an
 // Epsg-only canon collides every WKT CRS onto one identity, the deleted form). The authority Name string itself is
 // EXCLUDED when an Epsg resolves (Epsg IS the identity); for a WKT/projection CRS the Wkt + projection ARE the identity.
 public void CanonicalBytes(CanonicalWriter w) {
  w.Bool(Epsg.IsSome);
  Epsg.IfSome(e => w.Ordinal(e));
  w.String(Wkt).String(MapProjection).String(MapZone);
 }
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record GeoReference {
 public double Eastings { get; }
 public double Northings { get; }
 public double OrthogonalHeight { get; }
 public double XAxisAbscissa { get; }
 public double XAxisOrdinate { get; }
 public double ScaleX { get; }
 public double ScaleY { get; }
 public double ScaleZ { get; }
 public string GeodeticDatum { get; }
 public string VerticalDatum { get; }
 public Option<ProjectedCrs> Crs { get; }
 public Option<double> Epoch { get; }

 // PRIVATE ctor + GET-ONLY members (the AssessmentPayload shape): Admit is the ONLY public admission and Identity
 // the one pre-admitted constant, so a non-finite translation, a direction-less cosine pair, a collapsing or
 // mirroring scale, or an unresolvable CRS is UNREPRESENTABLE — the positional public ctor beside Admit was the
 // bypass that could mint a silently-mislocated frame straight into Header.CanonicalBytes; a wire or persistence
 // decoder re-admits through the SAME gate (the ContentAddress.Verify distrust posture), and no init/set survives
 // for a `with`/object-initializer to re-open an invariant.
 private GeoReference(
  double eastings, double northings, double orthogonalHeight, double abscissa, double ordinate,
  double scaleX, double scaleY, double scaleZ, string geodeticDatum, string verticalDatum,
  Option<ProjectedCrs> crs, Option<double> epoch) =>
  (Eastings, Northings, OrthogonalHeight, XAxisAbscissa, XAxisOrdinate, ScaleX, ScaleY, ScaleZ,
   GeodeticDatum, VerticalDatum, Crs, Epoch) =
  (eastings, northings, orthogonalHeight, abscissa, ordinate, scaleX, scaleY, scaleZ,
   geodeticDatum, verticalDatum, crs, epoch);

 public static readonly GeoReference Identity =
  new(0.0, 0.0, 0.0, 1.0, 0.0, 1.0, 1.0, 1.0, "", "", None, None);

 // The parsed EPSG (the CRS identity for the EPSG-keyed ProjNET transform build) — None for a WKT-only or
 // non-georeferenced CRS, where Resolution reads Wkt and the Bim transform owner takes the CreateFromWkt path.
 public Option<int> Epsg => Crs.Bind(static c => c.Epsg);

 // HOW the CRS resolves: Unreferenced when no CRS, else the ProjectedCrs.Resolution (Epsg or Wkt) — the column the
 // downstream Rasm.Bim transform owner branches on to select the EPSG-keyed vs WKT-keyed transform-build path.
 public CrsResolution Resolution =>
  Crs is { IsSome: true, Case: ProjectedCrs projected } ? projected.Resolution : CrsResolution.Unreferenced;

 // A WKT-only CRS (no EPSG, no non-zero origin) is STILL georeferenced — the two-state Epsg.IsSome test that reported
 // false for a WKT CRS is the deleted form. A non-identity map conversion alone also counts (an engineering offset,
 // a TrueNorth-folded rotation, or a per-axis scale with no CRS): the origin-only test blind to a rotation-only or
 // scale-only conversion is the deleted form — the predicate reads the WHOLE conversion tuple, not four of its doubles.
 public bool IsGeoreferenced =>
  Crs.IsSome || Eastings != 0.0 || Northings != 0.0 || OrthogonalHeight != 0.0
  || XAxisAbscissa != 1.0 || XAxisOrdinate != 0.0 || ScaleX != 1.0 || ScaleY != 1.0 || ScaleZ != 1.0;

 // IFC carries the map-conversion rotation as a direction cosine, not an angle.
 public double RotationRadians => Math.Atan2(XAxisOrdinate, XAxisAbscissa);

 // The uniform map-conversion scale the IFC single-`Scale` egress read recovers WHEN the three metre-frame axes agree
 // (the common LoGeoRef-50 case carries one Scale); Option<double>.None when an IfcMapConversionScaled set distinct
 // per-axis factors, so an egress fold reads the per-axis ScaleX/Y/Z instead of silently emitting one wrong scale.
 public Option<double> Scale => ScaleX == ScaleY && ScaleY == ScaleZ ? Some(ScaleX) : None;

 // The CRS content projection a Geospatial/coverage#COVERAGE_NODE CoverageGrid delegates to for its node identity:
 // the map-conversion translation/rotation/per-axis metre-frame scale, the datum names, and the full ProjectedCrs
 // identity (Epsg + Wkt + MapProjection/MapZone) through the shared Projection/address#CANONICAL_WRITER IEEE-754 canon
 // — so two georeferences resolving the same EPSG code address identically, two EPSG-less WKT CRSs differing only in
 // WKT address distinctly, and a CRS change (EPSG, WKT, projection, or datum) forks the coverage's NodeId.
 public void CanonicalBytes(CanonicalWriter w) {
  w.Double(Eastings).Double(Northings).Double(OrthogonalHeight)
   .Double(XAxisAbscissa).Double(XAxisOrdinate)
   .Double(ScaleX).Double(ScaleY).Double(ScaleZ)
   .String(GeodeticDatum).String(VerticalDatum)
   .Bool(Crs.IsSome);
  Crs.IfSome(c => c.CanonicalBytes(w));
  w.Bool(Epoch.IsSome);                              // presence-delimited (injectivity law) — an epoch change forks the frame identity
  Epoch.IfSome(e => w.Double(e));
 }

 // The metre-frame admission: the Rasm.Bim projector has already composed the IfcProjectedCRS.MapUnit SIFactor() onto
 // the per-axis scale, so the doubles arrive in metres. The independent legs ACCUMULATE (VALIDATION_MONOID): the
 // translation finite, the direction-cosine pair finite and non-degenerate ((0,0) has no direction — RotationRadians
 // would silently read 0), the per-axis scales finite and strictly positive (a zero scale collapses the frame, a
 // negative one mirrors it — neither is a map conversion), and the CRS leg — a value-admission gate, because a NaN
 // eastings would otherwise canonicalize stably into Header.CanonicalBytes and mislocate every model silently.
 // Dependence binds INSIDE the CRS leg (COMPOSITE_ADMISSION): a blank Name AND blank Wkt AND blank projection yields
 // the no-CRS None state (valid, an ungeoreferenced model never blocks); else ProjectedCrs.Of is the STRUCTURAL
 // admission (some identity string present, re-keyed to `key`), then crs.IsResolvable the SEMANTIC gate — an
 // EPSG-bearing name, a WKT-defined CRS, or a projection+zone CRS all SUCCEED, while a name-only "GibberishName"
 // FAULTS ValueRejected (never a silent skip, never a faulted WKT). The datum strings trim at this one boundary so a
 // padded IFC datum token never forks the CanonicalBytes identity.
 public static Fin<GeoReference> Admit(
 double eastings, double northings, double orthogonalHeight,
 double abscissa, double ordinate, double scaleX, double scaleY, double scaleZ,
 string geodeticDatum, string verticalDatum,
 string projectedCrsName, string wkt, string mapProjection, string mapZone, Op key,
 Option<double> epoch = default) =>
 (Gate(double.IsFinite(eastings) && double.IsFinite(northings) && double.IsFinite(orthogonalHeight), key,
   $"<map-conversion-translation-non-finite:{eastings:R}:{northings:R}:{orthogonalHeight:R}>"),
  Direction(abscissa, ordinate, key),
  Gate(double.IsFinite(scaleX) && scaleX > 0.0 && double.IsFinite(scaleY) && scaleY > 0.0 && double.IsFinite(scaleZ) && scaleZ > 0.0, key,
   $"<map-conversion-scale-non-finite-or-non-positive:{scaleX:R}:{scaleY:R}:{scaleZ:R}>"),
  Gate(epoch.ForAll(static e => double.IsFinite(e) && e > 0.0), key,
   $"<coordinate-epoch-non-finite-or-non-positive:{epoch.Map(static e => e.ToString("R")).IfNone("none")}>"),
  AdmitCrs(projectedCrsName, wkt, mapProjection, mapZone, key))
 .Apply((_, direction, _, _, crs) => new GeoReference(
  eastings, northings, orthogonalHeight, direction.Abscissa, direction.Ordinate, scaleX, scaleY, scaleZ,
  geodeticDatum.Trim(), verticalDatum.Trim(), crs, epoch))
 .As().ToFin();

 // One accumulating gate slot (the coverage.md Gate shape): holds -> unit, else the NAMED ValueRejected riding the
 // implicit Validation lift behind the K upcast — the K-KINDED return is the compile contract, because the shipped
 // tuple Apply binds only on (K<F,A>, …) receivers (the material.md [ADMISSION_SLOTS] law).
 private static K<Validation<Error>, Unit> Gate(bool holds, Op key, string detail) =>
  holds ? unit : ElementFault.ValueRejected(key, detail);

 // IFC defines this pair as direction cosines. Normalization makes scalar-equivalent inputs one frame identity and
 // keeps the canonical bytes, equality, and RotationRadians projection on the same unit-vector evidence.
 private static K<Validation<Error>, (double Abscissa, double Ordinate)> Direction(double abscissa, double ordinate, Op key) {
  double magnitude = double.Hypot(abscissa, ordinate);
  return double.IsFinite(magnitude) && magnitude > 0.0
   ? (abscissa / magnitude, ordinate / magnitude)
   : ElementFault.ValueRejected(key, $"<map-conversion-direction-degenerate:{abscissa:R}:{ordinate:R}>");
 }

 // The dependent CRS leg: no-identity -> None (valid), else structural-then-semantic on the Fin rail, ToValidation
 // lifting it K-kinded into the accumulating tuple so a bad CRS reports BESIDE a bad conversion tuple, never instead of it.
 private static K<Validation<Error>, Option<ProjectedCrs>> AdmitCrs(string name, string wkt, string mapProjection, string mapZone, Op key) =>
  (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(wkt) && string.IsNullOrWhiteSpace(mapProjection) && string.IsNullOrWhiteSpace(mapZone)
   ? Fin.Succ(Option<ProjectedCrs>.None)
   : ProjectedCrs.Of(name, mapProjection, mapZone, wkt, key).Bind(crs =>
      crs.IsResolvable
       ? Fin.Succ(Some(crs))
       : ElementFault.ValueRejected(key, $"<crs-name-unresolvable:{name}>")))
  .ToValidation();
}
```

## [03]-[RESEARCH]

- [MAP_CONVERSION_TUPLE]: the tuple extends the migration source's eight-field `Rasm.Bim` `GeoReference` — adding the per-axis `ScaleX`/`ScaleY`/`ScaleZ` (the IFC `IfcMapConversion` carries a single `Scale` plus optional `ScaleY`/`ScaleZ`; the IFC4.3 ADD2 `IfcMapConversionScaled` carries `FactorX`/`FactorY`/`FactorZ`), the `GeodeticDatum`/`VerticalDatum` names (from `IfcCoordinateReferenceSystem.GeodeticDatum` and `IfcProjectedCRS.VerticalDatum`), and the three-state `ProjectedCrs` so a federation reconciles onto one metre frame from one record; the `Rasm.Bim` projector reads `IfcMapConversion`/`IfcMapConversionScaled`/`IfcProjectedCRS` into this tuple at ingest, composes the `IfcProjectedCRS.MapUnit` `IfcNamedUnit.SIFactor()` metre-per-map-unit factor onto the scale (so the seam doubles are metres, NOT CRS-native units), builds the rigid transform over the kernel transform algebra, and drives the `ProjNET`/OSR datum-to-datum reprojection — the seam carrying the metre parameters, the host-neutral rotation scalar, and the `CrsResolution` mode, never the transform, the reprojection, or the `MapUnit`.
- [THREE_STATE_CRS]: an `IfcProjectedCRS` resolves to one of three states, NOT the two-state EPSG/none slice the migration source modeled — `IfcCoordinateReferenceSystem.Name` (an EPSG authority code), `IfcCoordinateReferenceSystem.WellKnownText.WellKnownText` (an inline OGC WKT definition for a GIS-origin CRS with NO authority code, decompile-verified on `IfcWellKnownText`), or `IfcProjectedCRS.MapProjection`+`MapZone` (a named projection identity, decompile-verified strings on `IfcProjectedCRS`). The `ProjectedCrs` `[ComplexValueObject]` carries all four IFC carriers (`Name`, the parsed `Epsg`, `Wkt`, `MapProjection`/`MapZone`) so a consumer reads the whole CRS identity in one hop and `CrsResolution` discriminates the EPSG / WKT / projection transform-build path (a projection+zone identity takes its OWN `Projection` mode the Bim leg verdicts by case — never a `Wkt` label over an empty payload); a WKT-defined CRS is FULLY resolvable (the `ProjNET` `CoordinateSystemFactory.CreateFromWkt` / OSR `SpatialReference.ImportFromWkt`), so faulting it as "unresolvable" — the two-state `Option<int> Epsg` slice's behavior — is the deleted form that blocks GIS-origin IFC ingest.
- [FAULT_ON_UNRESOLVABLE]: a present-but-fully-unresolvable CRS is a FAULT, not a silent skip — `Admit` builds the `ProjectedCrs` from the `Name`/`Wkt`/`MapProjection`/`MapZone` and rails `ElementFault.ValueRejected` ONLY when a name (or WKT marker) is present yet resolves to NEITHER an EPSG code NOR a WKT definition NOR a projection+zone, so a mislocated model surfaces at ingest rather than producing silently-wrong coordinates downstream; a blank name AND blank WKT yields `Identity`-style no-CRS state so a non-georeferenced model never blocks, and an EPSG-bearing, WKT-defined, or projection+zone CRS all SUCCEED (the `IfcWellKnownText` carry is decompile-verified, and ProjNET/OSR resolve WKT, so the WKT state is valid coordinate evidence, never a fault). The CRS leg is ONE of four accumulating admission legs — the finite-translation, non-degenerate-direction, and positive-scale gates report beside it (VALIDATION_MONOID over independent legs, dependence bound inside the CRS leg's structural-then-semantic chain), so a lowering with a NaN offset AND a gibberish CRS names both faults in one `Fin.Fail`.
- [METRE_NORMALIZED_FRAME]: the translation and per-axis scale doubles carry NO unit ambiguity — the `Rasm.Bim` projector composes the `IfcProjectedCRS.MapUnit` `IfcNamedUnit.SIFactor()` model-unit↔CRS-unit factor (the abstract `public abstract double SIFactor()` on `IfcNamedUnit`, decompile-verified, resolved through `IfcSIUnit`/`IfcConversionBasedUnit` for a US-survey-foot State Plane zone) onto the scale at ingest, so the seam tuple is one metre frame and a federation reconciles every model onto it from one record; the seam carries NO `MapUnit` field (the unit reconciliation is the Bim projector's ingest concern, the seam doubles are metres), so a CRS-native-unit double on the seam or a unit-bearing `MeasureValue` on the translation is the rejected form — the rotation cosines and scales stay dimensionless, the translation metres, and the tuple a flat metre-frame parameter record whose finiteness, direction, and scale positivity `Admit` gates once (the interior and the `CanonicalBytes` projection then trust the admitted frame).

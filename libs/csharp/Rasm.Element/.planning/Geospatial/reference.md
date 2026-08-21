# [ELEMENT_GEO_REFERENCE]

`GeoReference` owns the host-neutral coordinate reference — one record carrying the full map-conversion-and-CRS state — the eastings/northings/orthogonal-height translation, the X-axis abscissa/ordinate rotation cosine pair, the per-axis `ScaleX`/`ScaleY`/`ScaleZ`, the `GeodeticDatum` name, one `ProjectedCrs` `[ComplexValueObject]` horizontal CRS identity beside one `VerticalCrs` vertical identity, and the `Option<double> Epoch` decimal-year coordinate epoch (the dynamic-datum/ITRF plate-motion anchor the Bim OSR leg threads through `SpatialReference.SetCoordinateEpoch`; no IFC attribute carries it, so an IFC ingest lands `None` and a survey/GIS ingest supplies it) — and the `CrsResolution` `[SmartEnum<string>]` policy column that records HOW the CRS resolves (an EPSG authority code, an inline OGC WKT definition, a `MapProjection`+`MapZone` projection identity — round-trippable identification no engine builds a transform from, its own typed mode — or no reference at all) — every accepted mode carrying exactly the evidence its consumer reads. `Admit` and the pre-admitted `Identity` are the only entries over a PRIVATE record constructor, so an unadmitted frame is unrepresentable. `GeoReference` stays HOST-NEUTRAL and PURE DATA, carrying the parameters a downstream `Rasm.Bim` projector folds into a rigid map-conversion transform (over the kernel transform algebra) and a `ProjNET`/OSR datum-to-datum reprojection — the seam references NO ProjNET, NO kernel `Transform` type, and materializes no geometry, because geometry is referenced by content hash. Translation and per-axis scale doubles arrive METRE-NORMALIZED at ingest: the `Rasm.Bim` projector composes the `IfcProjectedCRS.MapUnit` `IfcNamedUnit.SIFactor()` model-unit↔CRS-unit factor onto the scale BEFORE handing the tuple to `Admit`, so the seam frame is one metre frame a federation reconciles every model onto from one record and the seam carries NO `MapUnit` field (a US-survey-foot State Plane zone is reconciled to metres in Bim, never left as an ambiguous CRS-native double on the seam).

`ProjectedCrs` models the CRS in THREE states, never a two-state EPSG/none slice: an `IfcProjectedCRS` may carry an EPSG authority `Name` (`EPSG:25832`), an inline `IfcWellKnownText.WellKnownText` OGC WKT definition (a GIS-origin CRS with NO authority code), or — for a non-georeferenced model — neither. Its `[ComplexValueObject]` carries the authority `Name`, the parsed `Epsg`, the `Wkt` definition, and the `MapProjection`/`MapZone` projection identity together, and `CrsResolution` discriminates the state a consumer reads as a column. `Admit` is the tuple's VALUE-ADMISSION gate: the independent legs accumulate — the translation finite, the direction-cosine pair finite and non-degenerate, the per-axis scales finite and strictly positive, the optional epoch finite and positive, and the CRS leg — so a NaN eastings or a zero scale faults BESIDE a bad CRS rather than canonicalizing stably into `Header.CanonicalBytes` as a silently-mislocated frame; the CRS leg FAULTS (not silently skips) ONLY when a CRS name is present but resolves to NEITHER an EPSG code NOR a WKT definition NOR a projection+zone — an unresolvable georeference surfaces as `KernelFault.InvalidValue` rather than a silently-mislocated model — and a WKT-defined CRS is VALID (the `ProjNET` `CoordinateSystemFactory.CreateFromWkt` / OSR `SpatialReference.ImportFromWkt` resolves it), never faulted as "unresolvable". `GeoReference` rides ONLY the `Graph/element#ELEMENT_GRAPH` `Header` and the `Geospatial/coverage#COVERAGE_NODE` `Coverage` node — it is DROPPED from the `Object` node, because an object's placement is geometry the kernel owns by content hash, and the model-wide georeference is a header fact, not a per-object one. `VerticalCrs` makes the reference COMPOUND, carrying the vertical half over the same three states, so an orthometric height reconciles against a named vertical frame rather than a bare datum string. `GeoReference` composes the kernel `Op` op-key and, for the coverage-node content identity, the kernel `Rasm/Domain/identity#CONTENT_KEY` `CanonicalWriter` (a coverage's `NodeId` derives from its CRS, so `GeoReference` owns the `CanonicalBytes` projection of its full state), folding the `Epsg` AND the `Wkt`/`MapProjection`/`MapZone` AND the vertical frame so two EPSG-less WKT CRSs that differ only in their WKT — or two frames sharing a horizontal CRS over different vertical datums — address as the distinct frames they are.

## [01]-[INDEX]

- [02]-[GEO_REFERENCE]: `GeoReference` records the map conversion and CRS — `ProjectedCrs` the three-state horizontal identity (authority `Name`+parsed `Epsg`, inline `Wkt`, `MapProjection`/`MapZone`), `VerticalCrs` the vertical identity (stored `Epsg` + ordinal-compared datum `Name`), `CrsResolution` the resolution-mode policy column both read, `Admit` the accumulating factory (the shared `Finite`/`In`/`Optional` metre-frame slots over kernel `Band` rows, the direction leg, the near-uniform scale snap, and the CRS legs riding each identity owner's own gate), and `RotationRadians` the direction-cosine projection.

## [02]-[GEO_REFERENCE]

- Owner: `GeoReference` the host-neutral coordinate-reference record carrying the metre-normalized map-conversion-and-CRS state; `ProjectedCrs` the `[ComplexValueObject]` CRS identity carrying the authority `Name` (across the `EPSG:NNNN`/`urn:ogc:def:crs:EPSG::NNNN`/`urn:ogc:def:crs:EPSG:6.18.3:NNNN`/authority forms), the `Epsg` STORED at construction by the validate hook's one parse, the inline `Wkt` OGC definition, and the `MapProjection`/`MapZone` projection identity — admitted structurally AND semantically at construction, so a CRS with no identity at all, or a name-only one no engine resolves, is unrepresentable and every held value answers one of the three resolution states; `VerticalCrs` the `[ComplexValueObject]` vertical identity carrying the stored authority `Epsg` and the ordinal-compared datum `Name`, structurally admitted so a vertical frame with neither is the rejected form; `CrsResolution` the `[SmartEnum<string>]` resolution-mode column (`Epsg`/`Wkt`/`Projection`/`Unreferenced`) both identities read — `Projection` the naming-evidence-alone mode a `MapProjection`+`MapZone` pair or a code-less vertical datum takes, so the WKT build path never receives an empty payload.
- Entry: `GeoReference.Admit(...)` accumulates kernel scalar gates with the page's `Direction`, `AdmitCrs`, and `AdmitVertical` semantic legs, then collapses once to `Fin<T>`.
- Auto: `Admit` gates the metre-frame doubles through the shared slots — `Finite` over the three named translation ordinates, the page's own `Direction` normalization leg, one `In(scale, Band.Positive, name, key)` per axis, `Optional(epoch, Band.Positive, ...)` for the trailing epoch — so no slot builds a token on the passing arm, then SNAPS a per-axis scale triple within `ScaleUniformityTolerance` of its own mean onto that mean so uniformity is an admission verdict the stored frame carries rather than a downstream float test over three separately-composed `SIFactor()` products (the exact `Scale` compare is then honest, equality and the canonical bytes agreeing with it), trims the datum tokens at the one boundary, and builds the CRS through `ProjectedCrs.Of` — a blank `Name` AND blank `Wkt` yields the no-CRS `None`/`Unreferenced` state (valid, so a non-georeferenced model never blocks), a name resolving to an EPSG code yields the `Epsg` state, a WKT-only CRS (blank or unresolvable `Name`, non-blank `Wkt`) yields the `Wkt` state (VALID — ProjNET/OSR resolve WKT), and a name+projection+zone with no EPSG and no WKT yields the `Projection` state carrying its `MapProjection`/`MapZone` tokens (typed identification the Bim datum leg faults by CASE as transform-unbuildable — never a `Wkt`-labelled empty payload); only a name (or WKT marker) present with no EPSG, no WKT, and no projection+zone returns `KernelFault.InvalidValue` — AT `ProjectedCrs.Of` itself, whose validate hook trims the carriers, derives and STORES the `Epsg`, and rejects both the all-blank product and the semantically unresolvable one, so no downstream re-check exists; `ProjectedCrs.EpsgOf` — the static authority parse the instance `Epsg` column and the `Rasm.Bim` vertical-datum leg both compose, so one grammar serves both halves of a compound reference — matches the `EPSG:` prefix, the OGC URN colon tail (one arm owns both the `::NNNN` and versioned `:6.18.3:NNNN` forms, gated on the `EPSG` authority token so an `ESRI:NNNN` or foreign-authority URN never mis-parses as an EPSG identity), and NOTHING else — a bare integer name carries no authority evidence, so it resolves `None` and — absent a WKT or projection+zone — refuses at construction rather than georeferencing a model onto an undeclared frame; `RotationRadians` is the pure direction-cosine-to-angle projection (the IFC convention carrying the rotation as a direction rather than an angle), no kernel transform materialized.
- Receipt: the `GeoReference` is the coordinate-reference evidence the `Header` and a `Coverage` node carry; a downstream `Rasm.Bim` projector reads the metre-frame tuple to build the rigid map-conversion transform over the kernel `Transform` algebra and to drive the `ProjNET` `CoordinateSystemServices` (EPSG-keyed) OR `CoordinateSystemFactory.CreateFromWkt` (WKT-keyed) datum-to-datum reprojection, escalating to the OSR PROJ engine for what the managed algebra cannot express — the seam carrying only the parameters, the host-neutral rotation scalar, and the `CrsResolution` mode that selects the EPSG-vs-WKT transform-build path.
- Packages: Thinktecture.Runtime.Extensions (`[ComplexValueObject]`, `[ValidationError]`, `[SmartEnum<string>]`, member comparers), LanguageExt.Core (`Option`/`Fin`/`Validation`), `Rasm/Domain/identity#CONTENT_KEY` (`CanonicalWriter`), `Rasm` (`Op`, `AcceptValidated`, and `Rasm/Domain/validation#ADMISSION_SLOTS`).
- Growth: a new map-conversion parameter is one column on `GeoReference` a projector folds into the transform; a new CRS-name scheme is one arm on `ProjectedCrs.EpsgOf` (both the horizontal instance column and the Bim vertical leg gain it at once); a new CRS-identity carrier is one member on `ProjectedCrs` or `VerticalCrs` + one `CrsResolution` row; a new datum is the projector's `ProjNET`/OSR concern resolved from the EPSG code or WKT; never a per-CRS class, never a parallel WKT-vs-EPSG `GeoReference` family, and never a transform owner on the seam.
- Boundary: `GeoReference` is HOST-NEUTRAL pure data — a kernel `Transform` field, a `ProjNET` `MathTransform`, an OSR `SpatialReference`, or a host coordinate type on the seam is the named seam violation, the transform materialization and the datum reprojection being the `Rasm.Bim` projector's concern over the kernel transform algebra and the admitted `ProjNET`/`MaxRev.Gdal.Core` engines; the translation/scale doubles are METRE-NORMALIZED (the `Rasm.Bim` projector composes the `IfcProjectedCRS.MapUnit` `IfcNamedUnit.SIFactor()` factor onto the scale at ingest), so a CRS-native-unit double on the seam, a `MapUnit` field on the tuple, or a unit-bearing `MeasureValue` on the translation is the rejected form — the rotation cosines and scales are dimensionless and the translation is metres, so the tuple stays a flat metre-frame parameter record; the reference is COMPOUND — the horizontal `ProjectedCrs` beside the vertical `VerticalCrs`, which ABSORBS the vertical-datum name, so a bare `VerticalDatum` string with no code a survey ingest can attach is the deleted form and an orthometric height reconciles against a named frame; EPSG identity requires EPSG AUTHORITY EVIDENCE on every arm — an unattributed bare integer resolves `Unresolvable`, never EPSG, because "25832" is equally an ESRI code, a local grid ordinal, or a vendor id and reading it as EPSG georeferences a model onto a frame nobody declared; the CRS is a THREE-STATE `[ComplexValueObject]` (EPSG / WKT-or-projection / unreferenced) and a two-state `Option<int> Epsg` slice faulting a fully-resolvable WKT CRS is the deleted form, since a GIS-origin IFC carrying `IfcWellKnownText` with no authority code blocks ingest under it; every `CrsResolution` mode carries exactly the evidence its consumer reads — `Epsg` the code, `Wkt` a non-empty payload, `Projection` the `MapProjection`/`MapZone` identification the egress round-trips and the datum leg faults by case — so a projection-identified CRS routed down the WKT arm with an empty payload is the deleted payload-less label; the record constructor is PRIVATE and `Admit`/`Identity` the only entries, a positional public ctor beside the gate being the deleted bypass; the `Admit` factory is the tuple's ONE value-admission gate — the translation/direction/scale legs refuse a non-finite, direction-less, or non-positive-scale conversion (the `CanonicalWriter` canonicalizes a NaN stably, so an ungated NaN mints a stable identity for a meaningless frame — admission, not the codec, owns the rejection) and the CRS legs FAULT at their OWN owners' construction when a name resolves to NO identity at all (no EPSG, no WKT, no projection+zone) rather than silently skipping, so a mislocated model is a typed fault the ingest surfaces, while a WKT-resolvable or projection-bearing CRS is VALID; the seam owns the CRS-identity VOCABULARY (the `Name`/`Epsg`/`Wkt`/`MapProjection`/`MapZone` carry + the EPSG parse) but NOT the transform build (the EPSG-vs-WKT `ProjNET`/OSR transform construction is the Bim projector's, selected off `Resolution`); `GeoReference` rides ONLY the `Header` and the `Coverage` node and is DROPPED from the `Object` node — an object's placement is content-hashed geometry the kernel owns, the model-wide georeference a header fact; `CanonicalBytes` folds the `Epsg` AND the `Wkt`/`MapProjection`/`MapZone` so two EPSG-less CRSs differing only in WKT address distinctly (an `Epsg`-only canon that drops the WKT is the deleted form that collides every WKT CRS onto one identity).

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Runtime.InteropServices;
using Generator.Equals;
using LanguageExt;
using LanguageExt.Traits;
using Rasm.Domain;
using Rasm.Element.Projection;
using Thinktecture;
using static LanguageExt.Prelude;
using static Rasm.Domain.AdmissionSlots;
using Band = Rasm.Numerics.Band;

namespace Rasm.Element.Geospatial;

// --- [TYPES] ------------------------------------------------------------------------------
// HOW a CRS resolves to a transform-buildable identity — the policy column the downstream Rasm.Bim transform owner
// branches on to select the ProjNET EPSG-keyed CoordinateSystemServices path vs the WKT-keyed CreateFromWkt path,
// read as a column rather than re-branching `Epsg.IsSome` per consumer. A constructed ProjectedCrs resolves to exactly
// one of THREE states — Epsg, Wkt, or Projection — and never to Unreferenced, which is GeoReference.Identity's state
// alone: a value-carrying CRS always answers some identity, because the all-blank product is not constructible.
[SmartEnum<string>]
public sealed partial class CrsResolution {
 public static readonly CrsResolution Unreferenced = new("unreferenced"); // no map conversion / no CRS — Identity, the no-transform leg
 public static readonly CrsResolution Epsg = new("epsg");                 // an EPSG authority code resolves the CRS (the ProjNET CoordinateSystemServices path)
 public static readonly CrsResolution Wkt = new("wkt");                   // an inline OGC WKT payload defines it, no authority code (the CreateFromWkt path)
 public static readonly CrsResolution Projection = new("projection");     // naming evidence alone — a MapProjection+MapZone pair or a vertical datum name, engine-unbuildable

 // CrsResolution makes the build path the CASE ITSELF — the Rasm.Bim GeoTransform owner dispatches the GENERATED `Switch`
 // (`resolution.Switch(epsg: …, wkt: …, projection: …, unreferenced: …)`) to select the EPSG-keyed
 // CoordinateSystemServices build, the WKT-keyed CreateFromWkt build, the typed projection-only verdict (a bare
 // MapProjection+MapZone identity is round-trippable IDENTIFICATION the egress re-emits, but NEITHER managed
 // engine builds a transform from it — the Bim leg faults `crs-projection-only-unbuildable` on the CASE instead
 // of sniffing an empty Wkt payload, the deleted payload-less Wkt label), or the no-transform leg, so a new
 // resolution mode breaks every build site at compile time. A `BuildsByEpsg`/`BuildsByWkt` boolean pair the
 // consumer chains as `if (resolution.BuildsByEpsg)` is the deleted COLLAPSE_SCAN [04] re-branch — the smart-enum
 // owns the dispatch, never a derived bool that re-states the case as a flag and silently misses the next mode.
 // CrsResolution owns the DISCRIMINANT; Bim owns the ProjNET/OSR build and the per-mode verdict.
}

// ProjectedCrs carries the CRS identity as a THREE-STATE [ComplexValueObject], never a two-state Option<int> Epsg slice: an
// IfcProjectedCRS may carry an EPSG authority Name, an inline IfcWellKnownText.WellKnownText OGC definition
// (a GIS-origin CRS with NO authority code), or a MapProjection+MapZone projection identity. All four carriers (Name, the
// parsed Epsg, Wkt, MapProjection/MapZone) ride ONE value-object so a consumer reads the whole CRS identity in one
// hop and Resolution discriminates the EPSG-vs-WKT path. Identity is the (Name, MapProjection, MapZone, Wkt) product
// under SPLIT comparer policy (the authority Name/MapProjection/MapZone case-insensitive — CRS authority tokens are
// case-stable; the Wkt byte-exact — a WKT is a structured definition, not a case-fold token), mirroring the sibling
// Classification/classification#CLASSIFICATION_AXIS Classification axis (the other neutral cross-cutting value-object).
// ValidateFactoryArguments rejects the all-blank product — a CRS with no identity at all is not constructible.
[ComplexValueObject]
[ValidationError]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct ProjectedCrs {
 [MemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>] public string Name { get; }
 [MemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>] public string MapProjection { get; }
 [MemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>] public string MapZone { get; }
 [MemberEqualityComparer<ComparerAccessors.StringOrdinal, string>] public string Wkt { get; }
 // STORED at construction (the ValidateFactoryArguments ref hook derives it from the trimmed Name, the
 // InstrumentSpec dimensions-canonicalization idiom), so the authority parse runs ONCE and Resolution and the
 // canonical fold read the admitted value — a per-read re-parse was the deleted form.
 public Option<int> Epsg { get; }

 // Trim the four CRS carriers, DERIVE the Epsg, then gate BOTH admissions: the structurally empty product (no
 // identity string at all) and the semantically unresolvable one (a name-only "GibberishName" — no EPSG authority,
 // no WKT, no projection+zone) are equally unconstructible, so EVERY ProjectedCrs value answers one of the three
 // resolution states and the Projection state can never carry an empty payload. The keyless fault re-stamps to the
 // caller's Op in `Of`.
 private static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string name, ref string mapProjection, ref string mapZone, ref string wkt, ref Option<int> epsg) {
  (name, mapProjection, mapZone, wkt) = (name.Trim(), mapProjection.Trim(), mapZone.Trim(), wkt.Trim());
  epsg = EpsgOf(name);
  validationError =
   name.Length == 0 && mapProjection.Length == 0 && mapZone.Length == 0 && wkt.Length == 0
    ? new ValidationError("projected CRS must carry an authority name, an inline WKT, or a projection+zone")
   : epsg.IsNone && wkt.Length == 0 && !(mapProjection.Length > 0 && mapZone.Length > 0)
    ? new ValidationError("projected CRS carries no resolvable identity")
   : null;
 }

 // Of lifts the generated admission through the caller's operation key.
 public static Fin<ProjectedCrs> Of(string name, string mapProjection, string mapZone, string wkt, Op key) =>
  key.AcceptValidated<ProjectedCrs>(
   Validate(name, mapProjection, mapZone, wkt, default, out ProjectedCrs value),
   value);

 // EpsgOf parses STATICALLY because two owners read one grammar: this owner's construction hook, and the vertical
 // code the Rasm.Bim height-datum leg admits off a bare IfcVerticalDatum declaration. Every arm requires the EPSG
 // authority token — the EPSG:NNNN prefix, or the URN colon-tail arm owning both ::NNNN and versioned :6.18.3:NNNN
 // forms (segment EQUALITY with the token, so ESRI:NNNN or a name merely containing "EPSG" never mis-parses). A
 // BARE integer name carries no authority at all — "25832" is equally an ESRI code, a local grid ordinal, or a
 // vendor id — so it resolves None; reading it as EPSG silently georeferences a model onto a frame nobody declared.
 public static Option<int> EpsgOf(string name) =>
  name.StartsWith("EPSG:", StringComparison.OrdinalIgnoreCase) && int.TryParse(name.AsSpan(5), out int prefixed)
  ? Some(prefixed)
  : name.Split(':') is { Length: >= 3 } segs
    && segs[^3].Equals("EPSG", StringComparison.OrdinalIgnoreCase)
    && int.TryParse(segs[^1], out int urnCode)
  ? Some(urnCode)
  : None;

 // Resolution ranks the mode: an EPSG code wins (the densest identity a ProjNET CoordinateSystemServices build keys
 // on); a WKT-carrying CRS resolves through the CreateFromWkt path; a projection+zone identity takes its OWN
 // Projection mode — round-trippable identification the Bim leg faults by CASE as transform-unbuildable, its
 // payload non-empty BY CONSTRUCTION. Never Unreferenced — that state is GeoReference.Identity, which carries no CRS.
 public CrsResolution Resolution =>
  Epsg.IsSome ? CrsResolution.Epsg
  : Wkt.Length > 0 ? CrsResolution.Wkt
  : CrsResolution.Projection;

 // The parsed Epsg IS the identity when present (two names resolving one code address identically); for a
 // WKT/projection CRS the Wkt + projection tokens are — an Epsg-only canon collides every WKT CRS onto one identity.
 public void CanonicalBytes(CanonicalWriter w) =>
  w.Optional(Epsg, static (e, wr) => wr.Ordinal(e)).String(Wkt).String(MapProjection).String(MapZone);
}

// VerticalCrs carries the VERTICAL half of a compound reference, the horizontal ProjectedCrs's sibling: an orthometric
// height reconciles only against a NAMED vertical frame, so a bare VerticalDatum string strands every height a federation tries to
// align. Resolution is the same three states the horizontal owner carries, read off the SAME CrsResolution column: an
// authority code (EPSG:5701 Newlyn, EPSG:5703 NAVD88) a Rasm.Bim height-transform build keys on, a datum name alone
// (identification the egress round-trips, engine-unbuildable), and absence — the Option<VerticalCrs> None a model
// whose heights are ellipsoidal or unreferenced carries. The Epsg is a STORED member, not a Name parse: IFC supplies the
// vertical datum as a name and a survey/GIS ingest supplies the code separately, so the two are independent
// evidence rather than one string two readers disagree about. Name is ORDINAL-compared because the CanonicalWriter
// writes it verbatim — a case-folding comparer rules two frames equal whose canonical bytes differ.
[ComplexValueObject]
[ValidationError]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct VerticalCrs {
 public Option<int> Epsg { get; }
 [MemberEqualityComparer<ComparerAccessors.StringOrdinal, string>] public string Name { get; }

 // ValidateFactoryArguments gates structurally: a vertical CRS with neither a code nor a name carries no identity and is not constructible.
 private static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Option<int> epsg, ref string name) {
  name = name.Trim();
  if (epsg.IsNone && name.Length == 0) {
   validationError = new ValidationError("vertical CRS must carry an authority code or a datum name");
  }
 }

 public static Fin<VerticalCrs> Of(Option<int> epsg, string name, Op key) =>
  key.AcceptValidated<VerticalCrs>(Validate(epsg, name, out VerticalCrs value), value);

 public CrsResolution Resolution => Epsg.IsSome ? CrsResolution.Epsg : CrsResolution.Projection;

 public void CanonicalBytes(CanonicalWriter w) =>
  w.Optional(Epsg, static (e, wr) => wr.Ordinal(e)).String(Name);
}

// --- [MODELS] -----------------------------------------------------------------------------
// [Equatable] because the frame is STORED (Header, Coverage) and the Rasm.Persistence StructuralMerge drills a
// changed member rather than replacing the whole frame where a coverage band drills.
[Equatable]
public sealed partial record GeoReference {
 public double Eastings { get; }
 public double Northings { get; }
 public double OrthogonalHeight { get; }
 public double XAxisAbscissa { get; }
 public double XAxisOrdinate { get; }
 public double ScaleX { get; }
 public double ScaleY { get; }
 public double ScaleZ { get; }
 public string GeodeticDatum { get; }
 public Option<ProjectedCrs> Crs { get; }
 // Vertical carries the compound reference's vertical half — it ABSORBS the vertical-datum name, so the frame carries one
 // vertical identity rather than a bare string beside a code nobody can attach to it.
 public Option<VerticalCrs> Vertical { get; }
 public Option<double> Epoch { get; }

 // PRIVATE ctor + GET-ONLY members (the AssessmentPayload shape): Admit is the ONLY public admission and Identity the
 // one pre-admitted constant, so a non-finite translation, a direction-less cosine pair, a collapsing or
 // mirroring scale, or an unresolvable CRS is UNREPRESENTABLE — a positional public ctor beside Admit is the bypass
 // that mints a silently-mislocated frame straight into Header.CanonicalBytes; a wire or persistence
 // decoder re-admits through the SAME gate (the ContentAddress.Verify distrust posture), and no init/set survives
 // for a `with`/object-initializer to re-open an invariant.
 private GeoReference(
  double eastings, double northings, double orthogonalHeight, double abscissa, double ordinate,
  double scaleX, double scaleY, double scaleZ, string geodeticDatum,
  Option<ProjectedCrs> crs, Option<VerticalCrs> vertical, Option<double> epoch) =>
  (Eastings, Northings, OrthogonalHeight, XAxisAbscissa, XAxisOrdinate, ScaleX, ScaleY, ScaleZ,
   GeodeticDatum, Crs, Vertical, Epoch) =
  (eastings, northings, orthogonalHeight, abscissa, ordinate, scaleX, scaleY, scaleZ,
   geodeticDatum, crs, vertical, epoch);

 public static readonly GeoReference Identity =
  new(0.0, 0.0, 0.0, 1.0, 0.0, 1.0, 1.0, 1.0, "", None, None, None);

 // Epsg reads the parsed code (the CRS identity for the EPSG-keyed ProjNET transform build) — None for a WKT-only, a
 // projection-only, or a non-georeferenced CRS, where Resolution reads Wkt, Projection, or Unreferenced and the Bim
 // transform owner dispatches the CreateFromWkt build, the unbuildable verdict, or the no-transform leg.
 public Option<int> Epsg => Crs.Bind(static c => c.Epsg);

 // HOW the CRS resolves: Unreferenced when no CRS, else the ProjectedCrs.Resolution (Epsg, Wkt, or Projection) — the
 // column the downstream Rasm.Bim transform owner dispatches to select the EPSG-keyed build, the WKT-keyed build, or the
 // typed projection-only unbuildable verdict.
 public CrsResolution Resolution =>
  Crs is { IsSome: true, Case: ProjectedCrs projected } ? projected.Resolution : CrsResolution.Unreferenced;

 // IFC carries the map-conversion rotation as a direction cosine, not an angle. A "was this georeferenced" read is
 // `this != Identity` — structural equality against the one pre-admitted constant, so no parallel predicate exists
 // to drift from the carriers the record actually holds.

 public double RotationRadians => Math.Atan2(XAxisOrdinate, XAxisAbscissa);

 // Scale recovers the uniform map-conversion factor the IFC single-`Scale` egress read takes WHEN the three axes agree
 // (the common LoGeoRef-50 case carries one Scale); Option<double>.None when an IfcMapConversionScaled set distinct
 // per-axis factors, so an egress fold reads the per-axis ScaleX/Y/Z instead of silently emitting one wrong scale.
 // Exact comparison is honest because Admit already SNAPPED a near-uniform triple to exactly uniform: uniformity is
 // an admission verdict, never a downstream float test.
 public Option<double> Scale => ScaleX == ScaleY && ScaleY == ScaleZ ? Some(ScaleX) : None;

 // CanonicalBytes projects the CRS a Geospatial/coverage#COVERAGE_NODE CoverageGrid delegates to for node identity: the
 // map-conversion translation/rotation/per-axis metre-frame scale, the geodetic datum name, the full ProjectedCrs
 // identity (Epsg + Wkt + MapProjection/MapZone), and the vertical frame, through the shared
 // Projection/address#IMPLEMENTATION_LAW IEEE-754 canon — so two georeferences resolving the same EPSG code address
 // identically, two EPSG-less WKT CRSs differing only in WKT address distinctly, and a change to either half of the
 // compound reference (horizontal EPSG/WKT/projection, geodetic datum, vertical code or datum) forks the coverage's
 // NodeId. Every Option is presence-prefixed (the injectivity law), so absence and presence can never blur.
 public void CanonicalBytes(CanonicalWriter w) =>
  w.Double(Eastings).Double(Northings).Double(OrthogonalHeight)
   .Double(XAxisAbscissa).Double(XAxisOrdinate)
   .Double(ScaleX).Double(ScaleY).Double(ScaleZ)
   .String(GeodeticDatum)
   .Optional(Crs, static (c, wr) => c.CanonicalBytes(wr))
   .Optional(Vertical, static (v, wr) => v.CanonicalBytes(wr))
   .Optional(Epoch, static (e, wr) => wr.Double(e));

 // Admit gates the metre frame: the Rasm.Bim projector has already composed the IfcProjectedCRS.MapUnit SIFactor() onto the
 // per-axis scale, so the doubles arrive in metres. The independent legs ACCUMULATE through the shared slots —
 // Finite over the named translation ordinates, In(Band.Positive) per scale axis (a zero scale collapses the frame,
 // a negative one mirrors it — neither is a map conversion), Optional(Band.Positive) on the epoch — a value-admission
 // gate, because a NaN eastings otherwise canonicalizes stably into Header.CanonicalBytes and mislocates every model
 // silently. Dependence binds INSIDE each CRS leg (COMPOSITE_ADMISSION): a blank Name AND blank Wkt AND blank
 // projection yields the no-CRS None state (valid, an ungeoreferenced model never blocks); else ProjectedCrs.Of is
 // the ONE structural-and-semantic admission under `key` — an EPSG-bearing name, a WKT-defined CRS, or a
 // projection+zone CRS succeeds, while a name-only unresolved token returns the generated kernel refusal. The
 // vertical leg follows the same shape over its own two carriers. The datum strings trim at this one boundary so a
 // padded IFC datum token never forks the CanonicalBytes identity.
 public static Fin<GeoReference> Admit(
 double eastings, double northings, double orthogonalHeight,
 double abscissa, double ordinate, double scaleX, double scaleY, double scaleZ,
 string geodeticDatum, string verticalDatum,
 string projectedCrsName, string wkt, string mapProjection, string mapZone, Op key,
 Option<double> epoch = default, Option<int> verticalEpsg = default) =>
 (Finite(key, ("map-eastings", eastings), ("map-northings", northings), ("map-orthogonal-height", orthogonalHeight)),
  Direction(abscissa, ordinate, key),
  In(scaleX, Band.Positive, "map-scale-x", key),
  In(scaleY, Band.Positive, "map-scale-y", key),
  In(scaleZ, Band.Positive, "map-scale-z", key),
  Optional(epoch, Band.Positive, "coordinate-epoch", key),
  AdmitCrs(projectedCrsName, wkt, mapProjection, mapZone, key),
  AdmitVertical(verticalEpsg, verticalDatum, key))
 .Apply((_, direction, x, y, z, admittedEpoch, crs, vertical) => Uniform(x, y, z) switch {
   var (sx, sy, sz) => new GeoReference(
    eastings, northings, orthogonalHeight, direction.Abscissa, direction.Ordinate, sx, sy, sz,
    geodeticDatum.Trim(), crs, vertical, admittedEpoch),
  })
 .As().ToFin();

 // ScaleUniformityTolerance is the relative band inside which three independently-composed per-axis factors ARE one factor.
 private const double ScaleUniformityTolerance = 1e-12;

 // Scale uniformity is decided at ADMISSION and stored, never re-tested downstream: the three axes reach Admit as
 // SEPARATE IfcNamedUnit.SIFactor() products, so a genuinely uniform conversion routinely differs in the last ULP and
 // a stored triple like that reads as three distinct factors forever after — the single-`Scale` egress then emits
 // per-axis factors for a frame that has exactly one. A triple within ScaleUniformityTolerance of its own mean
 // therefore SNAPS to that mean before construction, so the Scale read's exact compare answers the truth, equality
 // rules two identically-derived frames equal, and CanonicalBytes writes one factor three times rather than three
 // near-equal ones that fork the coverage NodeId. Runs only on the success projection, where the scale gate has
 // already proved all three finite and strictly positive, so the mean is positive and the relative band is total.
 private static (double X, double Y, double Z) Uniform(double scaleX, double scaleY, double scaleZ) {
  double mean = (scaleX + scaleY + scaleZ) / 3.0;
  double band = ScaleUniformityTolerance * mean;
  return Math.Abs(scaleX - mean) <= band && Math.Abs(scaleY - mean) <= band && Math.Abs(scaleZ - mean) <= band
   ? (mean, mean, mean)
   : (scaleX, scaleY, scaleZ);
 }

 // IFC defines this pair as direction cosines. Normalization makes scalar-equivalent inputs one frame identity and
 // keeps the canonical bytes, equality, and RotationRadians projection on the same unit-vector evidence.
 private static Validation<Error, (double Abscissa, double Ordinate)> Direction(double abscissa, double ordinate, Op key) {
  double magnitude = double.Hypot(abscissa, ordinate);
  return double.IsFinite(magnitude) && magnitude > 0.0
   ? (abscissa / magnitude, ordinate / magnitude)
   : new ElementFault.ValueRejected(key, $"<map-conversion-direction-degenerate:{abscissa:R}:{ordinate:R}>");
 }

 // AdmitCrs runs the dependent horizontal CRS leg: no-identity -> None (valid), else the ONE ProjectedCrs.Of gate —
 // which now owns BOTH admissions (structural presence and semantic resolvability), so no second stage exists here
 // to drift from it. ToValidation lifts the leg into the accumulating tuple so a bad CRS reports BESIDE a bad
 // conversion tuple, never instead of it.
 private static Validation<Error, Option<ProjectedCrs>> AdmitCrs(string name, string wkt, string mapProjection, string mapZone, Op key) =>
  (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(wkt) && string.IsNullOrWhiteSpace(mapProjection) && string.IsNullOrWhiteSpace(mapZone)
   ? Fin.Succ(Option<ProjectedCrs>.None)
   : ProjectedCrs.Of(name, mapProjection, mapZone, wkt, key).Map(Some))
  .ToValidation();

 // AdmitVertical runs the vertical leg, structurally the horizontal twin over its own two carriers: neither code nor name
 // yields the None state (a model with ellipsoidal or unreferenced heights), and either one alone mints the frame.
 private static Validation<Error, Option<VerticalCrs>> AdmitVertical(Option<int> epsg, string datum, Op key) =>
  (epsg.IsNone && string.IsNullOrWhiteSpace(datum)
   ? Fin.Succ(Option<VerticalCrs>.None)
   : VerticalCrs.Of(epsg, datum, key).Map(Some))
  .ToValidation();
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

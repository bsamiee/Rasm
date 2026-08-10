# [MATERIALS_ACQUISITION]

`Acquisition.Import` folds the closed measured-BRDF, SVBRDF-map, spectral-reflectance, and neural-plane `CaptureSource` family into the `AcquiredMaterial` product — the `MaterialParameters` row the material library registers and the lobe family shades, its measured `Provenance`, and, for a plane-bearing capture, the admitted `Raster/set#TEXTURE_SET` `TextureSet` itself, so a photo-to-PBR import returns a spatially-varying shadeable material rather than only the mean the seam key carries. Capture fitting composes the GGX/Smith/Fresnel kernel over reconstructed incident and outgoing directions, fits the anisotropic conductor-or-dielectric parameter vector, grounds spectral color, and gates the admitted row through the white-furnace and material-fault rails. `Provenance` carries capture method, sample counts, residual, conditioning, and grounded-color chromaticity as the one measured-evidence receipt shared by wire and pigment-mix consumers. Per-capture acquired-material types and private BRDF kernels are deleted forms.

## [01]-[INDEX]

- [02]-[ACQUISITION]: `CaptureSource` closes the `[Union]` capture family, `BrdfSample` records angular reflectance, `AcquiredMaterial` carries the import product, and `Provenance` carries the measured-capture receipt over the `CaptureMethod` `[SmartEnum]` instrument discriminant (counts, conditioning, chromaticity, and model-attribution columns beside the residual); `Acquisition.Import` composes the generic `Microfacet<T>`-composed anisotropic conductor/dielectric forward model over reconstructed `LocalVector<T>` directions, the kernel `Lm.Minimize` fit-and-ground fold over a box-reparameterized `(αx, αy, φ[, η])` dual residual, the per-texel SVBRDF field average, the neural-plane arm's `SetBind` summary over an admitted set, the chart-solved colour calibration every arm crosses, the `Freeze`/`Thaw` spectral-EXR round trip a measured curve persists through, and the `BrdfArchive` tensor-container reader the EPFL RGL capture admits by.

## [02]-[ACQUISITION]

- Owner: `Acquisition` static import fold; `CaptureSource` `[Union]` (measured-brdf · svbrdf-map · spectral-reflectance · neural-planes); `AcquiredMaterial` the one import product; `BrdfSample` the goniophotometer angular-reflectance record over `(θi, θo, Δφ)`; `CaptureMethod` `[SmartEnum<string>]` the measurement-instrument discriminant (goniophotometer · spectrophotometer · neural-svbrdf · neural-planes · pigment-mix · authored); `Provenance` the measured-capture receipt this page owns, keyed by `CaptureMethod` and carrying the angular-sample and spectral-band COUNTS, the `FitConditionNumber`/`FitRank` conditioning witness, the grounded-color `DominantWavelengthNm`/`ExcitationPurity`/`CctKelvin`/`CctDuv` chromaticity readout beside the `FitResidual`, the `Option<ModelCardId>`/`Option<LicenseClass>`/`Option<ContentAddress>` model attribution a neural capture fills and every other capture leaves absent, the `Option<IngestProvenance>` third-party custody evidence carried in the `Raster/set#SET_INGEST` shape itself rather than as a second source/licence/reference spelling, and the `Calibrated`/`CalibrationDeltaE` chart witness; `SpectralCurve` the durable sampled-spectrum carrier with its scale-invariant `LuminousEfficacy` fold; `SpectralKind` the reflective/emissive claim each carrying its own container part factory; `BrdfArchive` the admitted EPFL RGL tensor container over its `TensorField`/`TensorDtype` vocabularies; `CaptureCalibration` the twenty-four measured chart patches.
- Cases: capture {`MeasuredBrdf` (an angular `Seq<BrdfSample>` over incident/outgoing zenith and relative azimuth, the dielectric IOR seed, and the conductor/dielectric Fresnel discriminant — `Option<ComplexIor>` whose `Some` carries a measured complex IOR fixing `FresnelConductor` so only `(αx, αy, φ)` are fit, whose `None` fits the dielectric `(αx, αy, φ, η)`), `SvbrdfMap` (a per-texel `Seq<MaterialParameters>` field from a neural fit, collapsed to one row and discarded), `SpectralReflectance` (a `SpectralCurve` reflectance grid with its metalness/roughness — the carrier that persists, where an `Spd` cannot answer what grid built it), `NeuralPlanes` (an admitted `TextureSet` assembled from the `neural#MODEL_REGISTRY` stage plan's outputs, its `Seq<StageResult>` evidence, and the caller's fallback row — the planes SURVIVE the import)} — the closed capture family: `CaptureSource` admits a capture as one case, never a capture subtype, and `CaptureMethod` admits every measurement instrument as one ROW (the goniophotometer, the spectrophotometer, the neural-SVBRDF field, the neural-plane set, the `finish#FINISH` Kubelka-Munk pigment-mix, the authored sentinel), never a per-instrument capture type.
- Entry: `public static Fin<AcquiredMaterial> Import(CaptureSource source, Provenance provenance, Op key, Option<CaptureCalibration> calibration = default)` — the import fold, its optional chart context the one axis no capture value carries pairing the fitted closed `MaterialParameters` vector with its measured `Provenance` and, on the plane-bearing arm, the admitted set (the `MeasuredBrdf` arm runs the realized `csharp:Rasm.Compute/Tensor/blas#DENSE_ALGEBRA`-shaped overdetermined thin-`QR` Gauss-Newton solve fitting the anisotropic GGX `(αx, αy)`, the grain azimuth, and the dielectric IOR when no conductor IOR is supplied to the angular samples, projects the fit to the Disney `Roughness`/`Anisotropy`/`AnisotropyRotation` row columns, stamps the `goniophotometer` `CaptureMethod` and the sample count onto the receipt, and writes the witnessed `FitResidual` beside the final-Jacobian `FitConditionNumber`/`FitRank`; the `SpectralReflectance` arm grounds the base color through `surface#SPECTRAL_UPSAMPLE` and stamps the `spectrophotometer` method; the `SvbrdfMap` arm AVERAGES the per-texel field — the scene-linear base-color mean, the per-column scalar mean, and the per-band `SubsurfaceRadius` mean — into one row and stamps the `neural-svbrdf` method with the texel count; the `NeuralPlanes` arm binds the admitted set through the ONE `Raster/set#SET_BIND` `BindTarget.Summary` measured fold, returns the set alongside the summary row, and stamps the `neural-planes` method with the set digest as its device, the summed tiles inferred as its sample count, the WORST stage golden delta as its residual, and the MOST RESTRICTIVE contributing `ModelCard`/`LicenseClass` as its attribution), `Fin<T>` aborting on an underdetermined capture (`<measured-brdf-underdetermined>`, `m ≤ n` against the arm's own widened unknown count), a non-finite Jacobian or diverged fit (`MaterialFault.Parameter`), an out-of-gamut grounded color (`MaterialFault.Gamut`), or an empty capture; the produced row re-admits through `MaterialParameters.Of`, the ONE re-admission site also stamping the grounded base color's chromaticity readout (`DominantWavelength`/`ExcitationPurity`/`Temperature` → `DominantWavelengthNm`/`ExcitationPurity`/`CctKelvin`/`CctDuv`) onto the receipt for EVERY arm, and the residual-bearing provenance rides the product beside the row the way `finish#FINISH` `Resolve` returns its `(Row, Provenance)`; `SyntheticGrid(int seed, int count, Op key)` rails the deterministic stratified synthetic capture the benchmark corpus pins — a non-positive count refuses on the same `Fin` rail every sibling entry holds — geometry, ground-truth alphas, and a ground-truth grain azimuth derived wholly from the seed through the KERNEL's one lane-keyed draw, reflectance the kernel-composed dielectric forward model at those parameters so the fit workload has a known answer in direction as well as magnitude. `public static Fin<ReadOnlyMemory<byte>> Freeze(SpectralCurve curve, SpectralKind kind, Op key)` and `public static Fin<(SpectralCurve Curve, SpectralKind Kind)> Thaw(ReadOnlyMemory<byte> payload, int wavelengthCount, Op key)` are the durable pair a measured spectrum persists through — REFLECTANCE and EMISSION alike, each row binding its own container part factory — the receipt's own `WavelengthCount` the round-trip witness the thaw proves against and the kind the physical claim it preserves.
- Packages: Wacton.Unicolour (composed — `new Unicolour(PortValue.SceneLinear, Spd)`→`Xyz`→scene-linear `Acescg` for the spectral-reflectance grounding, the `RgbLinear.Triplet` channel read for the SVBRDF base-color mean, `IsInRgbGamut` for the fit gate, and the `DominantWavelength`/`ExcitationPurity`/`Temperature` chromaticity readout stamped onto the receipt), MathNet.Numerics (composed for the LINEAR chart solve alone — `Matrix<double>` the dense carrier, `Matrix<double>.Build.Dense` the design build, `Matrix<double>.QR(QRMethod.Thin)` + `QR<double>.Solve` the overdetermined 3x3 colour correction, `Control.UseManaged` the osx-arm64 provider — the direct AEC-domain pin, catalogued in `libs/csharp/.api/api-mathnet-numerics.md`), Rasm (project — `Rasm.Solving` `Lm.Minimize`/`IDualResidual`/`DualModel`/`Dual<T>`/`SolvePolicy`, the kernel's ONE nonlinear least-squares functor and the forward-mode dual it differentiates through), `Rasm.Materials.Raster` (composed — `TextureSet` the acquired plane bundle, `SetBind.Bind` under `BindTarget.Summary` the ONE measured summary fold, `TextureSet.Digest` the receipt's set identity, `IngestProvenance` the ONE third-party custody shape this receipt carries rather than re-spelling), `neural#MODEL_REGISTRY` (composed — `StageResult` the stage evidence, `ModelRegistry.Rows` the card lookup the attribution folds, `ModelCardId`/`LicenseClass`/`ModelCard.Artefact` the receipt columns), TinyEXR.NET (composed — `Spectral.CreateReflectivePart`/`CreateEmissivePart` the two part factories the `SpectralKind` rows bind, `IsSpectral`/`GetSpectrumType`/`GetWavelengths` the header reads, `IsSpectralChannel`/`TryParseChannelWavelength`/`TryGetStokesComponent` the re-ingest parse triple, `PartLevel.Channels`/`GetChannel` the channel enumeration, `ExrFile.SaveToMemory`/`LoadFromMemory` over `WriterResult<byte[]>`/`ReaderResult<Image>`, `Compression.ZIP` the durable row — the ONE container a measured curve persists through), Wacton.Unicolour.Datasets (composed — `Macbeth.All`, the twenty-four-patch reference the chart solve targets), Rasm (project — `Op`, `Deterministic` the one splitmix64 draw owner, `Rasm.Element.Projection` `ContentAddress`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new capture modality is one `CaptureSource` case carrying its fit arm and one `AcquiredMaterial` construction — never a per-capture material, a second import owner, or a `ImportPlanes`/`ImportBrdf` entry pair; a new measurement instrument is one `CaptureMethod` row the receipt stamps, never a per-device receipt type; a new fit parameter is one `(Lo, Hi)` row on `BrdfResidual.Bounds` projected onto a column the existing `graph#MATERIAL_LIBRARY` `MaterialParameters` already carries — `Dof` reads the bounds length, the box map is column-generic, and the Jacobian derives itself, so widening the unknown set is one row and never a solver edit; a new receipt fact is one init-defaulted column on the `Provenance` record (every existing construction binds unchanged), and the generated `interchange#MATERIAL_WIRE` `WireMap.ToWire(Provenance)` runs `RequiredMappingStrategy.Both`, so the new column compels a `WireProvenance` mirror or an explicit ignore row at the wire build — a compile-forced decision, never a silent drop. Measured-BRDF fitting shares the `bsdf#BSDF_GOLDEN` energy-conservation rows so an acquired material round-trips in-gamut; spectral grounding shares the `surface#SPECTRAL_UPSAMPLE` `Spd` construction with the `surface#CONDUCTOR_IOR` conductor rows, one upsampling owner.
- Law: the `SolveGgx` iteration exemption RETIRES with the loop it named — the fit is now the kernel functor's fold and this page states a residual row, so no bounded-trip accumulation, no mutable parameter vector, and no perturb-restore Jacobian survive to carve out. What remains is the `Reflectance` forward model (a fixed-width geometry evaluation), the `Solve` chart least-squares and the `Freeze`/`Thaw` container legs (their design-matrix build and their `WriterResult`/`ReaderResult` probes), and `SpectralCurve.LuminousEfficacy` (a span fold that crosses no lambda) — the admitted boundary-numeric-kernel carve-out from the immutable-fold law, the same one `surface#SPECTRAL_UPSAMPLE` `ToCurve` names; every other operation on the page — `Import`, `Calibrate`, `FitBrdf`, `AverageField`, `GroundSpectral`, the `FieldSum` fold — is expression-bodied and rail-threaded.
- Boundary: `Acquisition.Import` is the ONE import path and `AcquiredMaterial` its ONE product — a per-capture acquired-material type is the deleted form; the `MeasuredBrdf` arm states its residual and hands the fit to the KERNEL functor — `BrdfResidual : IDualResidual` declares the `m` angular samples as rows and the anisotropic GGX `(αx, αy)`, the grain azimuth `φ`, and the dielectric IOR on the `None`-conductor arm as its `Dof ∈ {3, 4}` unconstrained coordinates, and `Lm.Minimize(new DualModel(residual), SolvePolicy.Canonical, key)` runs the one damped Gauss-Newton λ-ladder the kernel owns, `DualModel` deriving the EXACT Jacobian from the residual code alone. The hand-rolled twelve-trip loop, its `0.5·Δ` half-step damping, its thin-`QR` step, its `Svd(true)` truncated-pseudo-inverse fallback, and its central-difference Jacobian are all the deleted form — each was this page re-deriving what the kernel already owns on one accept/reject ladder with its own rank-deficiency verdict, and the differencing step size in particular was a number this page had no basis to choose. Bounds ride a DIFFERENTIABLE box reparameterization `p = Lo + (Hi − Lo)·σ(u)` rather than a per-iteration clamp: every iterate is interior by construction and the map is smooth everywhere, where a clamp has no derivative at its own boundary and reads a zero gradient exactly where the box binds. The `bsdf#MICROFACET_KERNEL` GGX/Smith/Fresnel is the forward model `D·G·F/(4·cosθi·cosθo)`; the forward model is SINGLE-SOURCED on the kernel — `Acquisition.Reflectance` reconstructs the incident `wi = (sinθi, 0, cosθi)` and outgoing `wo = (sinθo·cosΔφ, sinθo·sinΔφ, cosθo)` `bsdf#SHADING_FRAME` `LocalVector<T>` directions from the sample's `(θi, θo, Δφ)`, rotates both by `−φ` about local Z so the fitted grain axis IS the basis the anisotropic terms read (the same rotate-the-lobe-not-the-frame form `bsdf#LOBE_FAMILY` shades through, so fit and shade share one convention), forms the true half-vector `h = (wi+wo).Normalize()`, and reads `Microfacet<T>.Ndf(h, αx, αy)` · `Microfacet<T>.MaskingShadowing(wo, wi, αx, αy)` · the discriminant-routed Fresnel (the measured `ComplexIor` bands folded through the kernel's own luminance weights when the capture carries a conductor IOR, `Microfacet<T>.FresnelDielectric(wo·h, η)` otherwise) / `(4·|cosθi|·|cosθo|)` — `Reflectance` being GENERIC over its scalar is what makes that single-sourcing REACHABLE at all: the synthetic capture evaluates the body at `double` and the residual row evaluates the SAME body at the solver's dual scalar, so the fit differentiates precisely what it predicts and no dual-arithmetic transcription of the microfacet model exists to drift from it, so the residual carries the genuine microfacet response at each measured geometry — a brushed-metal or anisotropic capture fits `(αx, αy)` against the conductor Fresnel and lands `Metalness = 1.0` with the fitted `Anisotropy`, never a rough dielectric with a hardcoded metalness — and the page NEVER re-mints the NDF, the Smith masking, or the Fresnel term as a private kernel (the prior hand-rolled `GgxModel`/`SmithG1`/inline-`D` with a `cos((θi−θo)/2)` half-angle that dropped the azimuth is the deleted form); the fitted parameters project to the row as `Roughness = (αx·αy)^¼`, `Anisotropy = (1 − min/max)/0.9`, and `AnisotropyRotation` the fitted azimuth of the `αx` axis on the row's unit convention — the FULL inverse of the Disney aspect remap, so a brushed-metal capture round-trips its grain DIRECTION as well as its magnitude; the azimuth was always IN the residual surface (the design matrix carries `(θi, θo, Δφ)` per sample and the forward model reconstructs `wo` from all three) and only the projection threw it away; the fit is witnessed by the relative residual the functor's OWN 106-bit objective reports against the capture's log scale, written onto `Provenance.FitResidual` and gated through `bsdf#BSDF_GOLDEN`, and by an observability witness read off the CAPTURE GEOMETRY rather than off a second factorization of the converged Jacobian: a capture whose relative-azimuth spread is degenerate can measure neither the second alpha nor the grain lying along it, so it reports `FitRank < Dof` with `+Inf` `ConditionNumber` and names its own cause, where a condition number could only say that something was ill-posed. That witness is one pass over the samples and it survives the ladder's own regularization — LM converges a deficient system to SOME answer, and what a reader needs is the reason that answer is not evidence. The managed provider is selected once through `Control.UseManaged` for the chart solve (osx-arm64 has no native MKL/OpenBLAS — a per-call-site `TryUseNativeMKL` is the named defect); the spectral grounding composes the `surface#SPECTRAL_UPSAMPLE` `Spd`→scene-linear `Acescg` through the ONE `PortValue.SceneLinear` working space (the kernel `RgbProfile.Acescg.Configuration` instance the graph page names — one instance, one Unicolour lazy-conversion cache identity) so a measured reflectance curve becomes a base color the SAME way a library row grounds, never a re-minted inline `new Configuration(RgbConfiguration.Acescg)` at any tier and never a local `SceneConfig` re-export alias; the `SvbrdfMap` arm computes a REAL field average — the base color averaged in scene-linear through `RgbLinear`, every Disney scalar column averaged, and the `SubsurfaceRadius` averaged per band — not a `texels.Head` first-row read (the prior placeholder the prose mislabelled as an average is the deleted form), so a 256×256 neural SVBRDF field collapses to the one representative row the renderer shades; the produced `MaterialParameters` re-admits through `graph#MATERIAL_LIBRARY` `MaterialParameters.Of` so an acquired row passes the same gamut/unit/IOR gate a registered row passes, the ONE re-admission site stamping the admitted row's grounded base-color chromaticity — `DominantWavelength`/`ExcitationPurity`/`Temperature` onto `DominantWavelengthNm`/`ExcitationPurity`/`CctKelvin`/`CctDuv`, the `photometric#PHOTOMETRIC` `EmissionInput` Wxy-projection precedent applied uniformly to every arm (`CctDuv` carries the off-locus distance, so a reader gates `CctKelvin` on `|Duv| ≤ 0.05` before trusting the correlated temperature) — and the `Provenance` receipt carries the `CaptureMethod` instrument, the angular sample or spectral band count, the fit residual, the observability witness, the chromaticity readout, and — for a capture this estate did not perform — the `Raster/set#SET_INGEST` `IngestProvenance` custody shape ITSELF, so a capture's third-party custody and an ingested set's are ONE question read at two grains rather than two rosters that drift — the generated `interchange#MATERIAL_WIRE` `WireMap.ToWire(Provenance)` mirrors `Device`/`WavelengthCount`/`FitResidual`/`Method`/`AngularSamples` by member name and derives `Measured` off the `CaptureMethod` column (`Method != CaptureMethod.Authored`), its `RequiredMappingStrategy.Both` diagnostics forcing every receipt column onto a wire mirror or an explicit ignore row; the `NeuralPlanes` arm consumes an ALREADY-ADMITTED `TextureSet` and never assembles one — `Rasm.Compute` executes the stage plan, the app root resolves each `StageResult.Planes` channel output — the frozen prior-drop projection, so a `delit` or `depth` prior reaches no set — through `Raster/codec#RASTER_CODEC` and admits the bundle through `Raster/set#TEXTURE_SET` `TextureSet.Of`, and this owner binds what admission already proved, so an inference product reaches the same extent, transfer, convention, and payload gates a pressed set passes; the averaged row comes from the ONE `SetBind` `Average` measured fold rather than a second mean spelled here, and the set rides back so the caller keeps a spatially-varying material where the `SvbrdfMap` arm deliberately discards its field; the EPFL RGL `.bsdf` decode is HAND-AUTHORED here because no managed reader exists and the reference implementation's licence permits clean transcription: `BrdfArchive` admits the tensor container by its magic word, gates each `TensorField` row's payload against the RANK the row declares (never a positional roster a producer could reorder), and reads its element widths off the `TensorDtype` vocabulary so an unadmitted dtype is named at the field gate rather than discovered at a cast. Two facts are DERIVED rather than declared: ISOTROPY is `phi_i.Length <= 2` — an isotropic measurement carries at most two incident-azimuth slices, so a flag beside that would be a second truth to reconcile — and the WAVELENGTH COUNT is read off the `wavelengths` field, never pinned, since the published materials sample from a handful of bands to nearly two hundred and a constant admits exactly one dataset while mis-striding every other. The admitted archive lowers into the `Seq<BrdfSample>` the `MeasuredBrdf` arm fits and, per band, the `SpectralCurve` the `SpectralReflectance` arm grounds through `surface#SPECTRAL_UPSAMPLE`. The neural-SVBRDF `.exr` map stays the host-edge import boundary the `Rasm.Bim`/app root owns, this owner consuming its decoded `Seq<MaterialParameters>` portable data; a malformed, empty, or out-of-gamut capture rails `MaterialFault`, never a sentinel row. COLOUR CALIBRATION is the measurement rigour the receipt's own vocabulary claims: `CaptureMethod.NeuralPlanes` and `NeuralSvbrdf` both stamp a receipt reading MEASURED on a base colour derived from a photograph whose white balance and exposure nothing verified, so an optional `CaptureCalibration` of the twenty-four ColorChecker patches solves one 3x3 through a thin-`QR` LINEAR least squares — a linear correction is not a nonlinear fit, so it stays on the dense-algebra route and never reaches for the ladder — and stamps `Calibrated` beside the mean CIEDE2000 residual it left — a chart-solved matrix with its own delta-E is what separates a measured reflectance from a camera's guess; the solve runs in AP1-linear with `Macbeth`'s sRGB/D50 references rebased through `ConvertToConfiguration` FIRST (the colorimetric boundary `graph#MATERIAL_LIBRARY` `Named` enforces), so the matrix corrects WITHIN one space rather than silently absorbing a chromatic adaptation, and it applies at the one pre-admission site every arm crosses rather than inside a single arm a fifth modality would then miss. A MEASURED CURVE PERSISTS: `SpectralCurve` is the durable sampled-spectrum carrier because Wacton's `Spd` is a one-way XYZ intake that republishes no grid, and `Freeze`/`Thaw` round-trip it through a wavelength-sampled EXR part whose channel names ARE the sampling grid — one texel wide, `Compression.ZIP`, `MaximumWavelengthCount` bounding a 195-wavelength goniophotometer grid with decades of headroom. The durable leg spans BOTH claims: `SpectralKind` rows the reflective and emissive part factories, so the `photometric#PHOTOMETRIC` measured emission SPD persists exactly as a spectrophotometer reflectance does where a reflective-only freeze left it with no durable form while the emissive factory sat catalogued and unconsumed, and a polarised part — a four-component Stokes family per wavelength, not one sampled curve — takes no row and refuses at the thaw. Re-ingest PARSES the container's own channel names through `IsSpectralChannel`/`TryParseChannelWavelength`/`TryGetStokesComponent` rather than re-minting the names this page would have written and looking those up, so a part written at another grid or by another producer decodes rather than missing every lookup; the Stokes half of that parse is not optional, since parsing wavelengths alone flattens four polarisation components of one wavelength onto one channel, and a non-zero component is dropped rather than summed into an unpolarised sample it is not; a `codec#RASTER_FORMAT` row is the WRONG home and stays unminted, because a wavelength grid is not a texture plane and admitting it as one would put a spectrum through extent, transfer, and mip gates that describe pixels. Without the pair a grounded triple survived an import and the curve did not, so a re-ground under a different working space was impossible; `Provenance.WavelengthCount` is the round-trip WITNESS the thaw proves its decoded grid against rather than a bare count nothing checks.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Runtime.InteropServices;     // MemoryMarshal — the spectral channel's float window
using LanguageExt;                        // Seq, Option, Fin
using Rasm.Domain;                        // Op, Deterministic (the ONE splitmix64 draw owner)
using System.Numerics;                    // the generic-math floor the shared forward model is written on
using Rasm.Materials.Appearance.Bsdf;    // Microfacet<T> (GGX/Smith/Fresnel), LocalVector<T> (the reconstructed directions), RgbSpectrum, ComplexIor, MaterialFault
using Rasm.Materials.Appearance.Surface; // SpectralUpsample (ToSpd + the SceneLinear grounding gate)
using Rasm.Materials.Appearance.Graph;   // MaterialParameters, SubsurfaceRadius, PortValue (the SceneLinear Acescg Configuration)
using Rasm.Materials.Appearance.Texture; // SamplerState — the read policy SetBind.Bind names with no default anywhere
using Rasm.Element.Projection;            // ContentAddress — the seam address spelling the artefact column carries
using Rasm.Materials.Raster;              // TextureSet, SetBind, BindTarget, SetBinding — the acquired plane set and its ONE measured summary fold
using Rasm.Numerics;                      // GamutPolicy — the kernel working-gamut row the grounding gate checks through
using MathNet.Numerics;                   // Control.UseManaged
using MathNet.Numerics.LinearAlgebra;     // Matrix<double>, Vector<double>
using MathNet.Numerics.LinearAlgebra.Factorization; // QRMethod — the chart solve's LINEAR least squares; the nonlinear fit rides the kernel functor
using Rasm.Solving;                       // Lm, ILmModel, IDualResidual, DualModel, Dual<T>, SolvePolicy, LmResult — the kernel's ONE nonlinear least-squares functor
using DoubleDouble;                       // ddouble — the 106-bit objective the dual residual is stated in
using TinyEXR.V3;                         // ExrFile, Image, Part, Spectral, Compression — the durable spectral-curve container
using Wacton.Unicolour;                   // Unicolour, Spd, ColourSpace, ColourTriplet, DeltaE
using Wacton.Unicolour.Datasets;          // Macbeth — the 24-patch ColorChecker reference the calibration solve targets
using Thinktecture;                       // [SmartEnum]/[Union]/[KeyMember*] + ComparerAccessors.StringOrdinal (the Thinktecture ordinal-key policy every folder table pins)
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance;      // folder-root, beside graph#MATERIAL_LIBRARY MaterialParameters and finish#FINISH

// --- [TYPES] -------------------------------------------------------------------------------
// CaptureMethod keys Provenance by measurement instrument: a goniophotometer angular BRDF, a
// spectrophotometer reflectance curve, a neural-SVBRDF inference, the finish#FINISH Kubelka-Munk pigment-mix, and the
// authored sentinel — so a consumer reads HOW a row was measured, not just THAT it was. A new instrument is one row,
// never a per-device receipt type; a Kubelka-Munk pigment mix is a MEASURED-pigment finish (its pigments carry measured
// K/S reflectance), so it stamps PigmentMix and reads as measured (!= Authored), never the authored sentinel.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CaptureMethod {
    public static readonly CaptureMethod Goniophotometer = new("goniophotometer");
    public static readonly CaptureMethod Spectrophotometer = new("spectrophotometer");
    public static readonly CaptureMethod NeuralSvbrdf = new("neural-svbrdf");
    public static readonly CaptureMethod NeuralPlanes = new("neural-planes");   // the neural#MODEL_REGISTRY stage plan's PLANE product — a whole set, distinct from the SvbrdfMap per-texel field that collapses to one row
    public static readonly CaptureMethod GlossMeter = new("gloss-meter");   // the tri-angle 20/60/85-degree specular reading — a real instrument whose evidence is roughness alone
    public static readonly CaptureMethod PigmentMix = new("pigment-mix");   // the finish#FINISH Kubelka-Munk pigment-mix receipt — a MEASURED-pigment finish, not authored
    public static readonly CaptureMethod Authored = new("authored");
}

// SpectralKind is what a measured curve MEASURES, carried as a ROW with its own container part factory. A
// reflectance and an emission spectrum are the SAME sampled shape under different physical claims, and the container
// records that claim in its own channel-name prefix — so the durable leg is ONE body for both and the
// photometric#PHOTOMETRIC emission SPD persists exactly as a spectrophotometer reflectance does. A reflective-only
// Freeze left the estate's other measured spectrum with no durable form at all while the emissive part sat
// catalogued and unconsumed, which is the gap the second row closes. Polarised measurement takes NO row: a Stokes
// family is four components per wavelength, not one sampled curve, and admitting it here would read four
// polarisation channels as four wavelengths.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpectralKind {
    public static readonly SpectralKind Reflectance = new("reflectance", SpectrumType.Reflective, Spectral.CreateReflectivePart);
    public static readonly SpectralKind Emission = new("emission", SpectrumType.Emissive, Spectral.CreateEmissivePart);

    public SpectrumType Container { get; }

    [UseDelegateFromConstructor]
    public partial Part Part(int width, int height, ReadOnlySpan<float> wavelengths, ReadOnlySpan<float> samples, string? units, Compression compression);

    // The reverse read a Thaw performs: a container type this page carries no row for crosses as ABSENCE, so a
    // polarised part refuses rather than decoding as an unpolarised curve.
    public static Option<SpectralKind> Of(SpectrumType container) =>
        toSeq(Items).Find(row => row.Container == container);
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct BrdfSample(double IncidentZenith, double OutgoingZenith, double AzimuthDelta, RgbSpectrum Reflectance);

// SpectralCurve carries a durable sampled spectrum — start wavelength, sample interval, and the non-negative
// coefficient grid, admitted ONCE at Of so the interior never re-checks and Spd never sees a rejected grid.
// Wacton's Spd is a ONE-WAY XYZ intake: the package publishes no reader for the grid a construction handed it, so a
// measured curve that must persist, round-trip, or be re-ground under a different working space cannot live as an
// Spd and lives here instead, crossing at the point of integration through ToSpd. The photometric#PHOTOMETRIC
// emission SPD reads this SAME carrier, so one sampled-spectrum shape spans reflectance and emission rather than
// two column triples that drift apart the first time either grows a column.
public readonly record struct SpectralCurve(int StartNm, int IntervalNm, ReadOnlyMemory<double> Coefficients) {
    public static Fin<SpectralCurve> Of(int startNm, int intervalNm, ReadOnlyMemory<double> coefficients, Op key) =>
        intervalNm is not (0 or 1 or 5)                                                    // the Spd.IsValid interval domain, proven BEFORE construction
            ? MaterialFault.Parameter(key, $"<spectral-curve-interval:{intervalNm}>")
            : startNm <= 0 || coefficients.Length < 2
                ? MaterialFault.Parameter(key, $"<spectral-curve-extent:{startNm}@{coefficients.Length}>")
                : coefficients.Span.ContainsAnyInRange(double.NegativeInfinity, -double.Epsilon)
                    ? MaterialFault.Parameter(key, "<spectral-curve-negative-coefficient>")
                    : Fin.Succ(new SpectralCurve(startNm, intervalNm, coefficients));

    public Spd ToSpd() => new(StartNm, IntervalNm, Coefficients.ToArray());
    public double WavelengthAt(int index) => StartNm + (index * IntervalNm);

    // LuminousEfficacy folds the photopic-band radiant-power fraction the photometric#PHOTOMETRIC luminous divide
    // scales by: ∫V(λ)S(λ)dλ / ∫S(λ)dλ over this curve's OWN sampling grid, which is why it lives on the curve and
    // not on the gate that consumes it. The ratio is SCALE-INVARIANT — both integrals are linear in S, so a common
    // factor cancels — and therefore a RELATIVE captured spectrum answers it exactly; an absolute SPD is NOT
    // required. The absolute premise held only for a tristimulus readout, whose Y the package normalizes against the
    // reference white and which consequently cannot recover the fraction at any scale, and that is exactly why the
    // fold reads these samples rather than crossing a Unicolour conversion to reach them.
    // Exemption: a span fold crosses no lambda, so the accumulation has no expression form.
    public double LuminousEfficacy() {
        ReadOnlySpan<double> samples = Coefficients.Span;
        (double weighted, double total) = (0.0, 0.0);
        for (int i = 0; i < samples.Length; i++) {
            (weighted, total) = (weighted + (Photopic(WavelengthAt(i)) * samples[i]), total + samples[i]);
        }
        // The gate's own contract is (0,1] by physics, so a degenerate all-zero curve reports the declared unity
        // idealization rather than a zero that would divide the luminous coercion by nothing.
        return total > 0.0 ? Math.Clamp(weighted / total, double.Epsilon, 1.0) : 1.0;
    }

    // V(λ) is the CIE photopic luminosity function, DERIVED from its own two-lobe analytic form rather than carried
    // as a transcribed lattice: each lobe is a Gaussian whose width differs either side of its own peak — the
    // asymmetry is what lets two lobes carry a curve a symmetric sum cannot — so the whole weighting is four
    // constants per lobe a reader checks against the definition instead of against a table nothing regenerates.
    // Past the visible band both lobes have already decayed, so a sample outside it contributes nothing and the fold
    // needs no domain gate; the peak evaluates to unity at 555 nm, which is the anchor the 683 lm/W divide assumes.
    static double Photopic(double nm) =>
        (0.821 * Lobe(nm, peak: 568.8, lower: 46.9, upper: 40.5)) + (0.286 * Lobe(nm, peak: 530.9, lower: 16.3, upper: 31.1));

    static double Lobe(double nm, double peak, double lower, double upper) =>
        (nm - peak) / (nm < peak ? lower : upper) switch { var t => Math.Exp(-0.5 * t * t) };
}

// CaptureCalibration holds the twenty-four patch triples a ColorChecker photograph yields, in the SAME order Macbeth.All
// publishes — order IS the correspondence, so a shuffled roster solves a matrix onto the wrong references and no
// residual can detect it, which is why the arity gate names the count rather than trusting a caller's zip.
public readonly record struct CaptureCalibration(Seq<Unicolour> Patches) {
    public static Fin<CaptureCalibration> Of(Seq<Unicolour> patches, Op key) =>
        patches.Count == toSeq(Macbeth.All).Count
            ? Fin.Succ(new CaptureCalibration(patches))
            : MaterialFault.Parameter(key, $"<capture-calibration-arity:{patches.Count}>");
}

// Provenance is the measured-capture receipt — a sealed record, never a struct: Method is a reference-typed row and a
// struct `default` would ghost it null past every initializer. Generated interchange#MATERIAL_WIRE
// WireMap.ToWire(Provenance) mirrors Device/WavelengthCount/FitResidual/Method/AngularSamples and the six evidence
// columns below by member name, derives Measured off the CaptureMethod column, and lowers the Option-typed
// attribution pair through its own readers onto the WireProvenance ModelCard/License keys — RequiredMappingStrategy.Both
// makes an unmirrored column a BUILD BREAK, so a receipt field never reaches a peer as silence;
// every later column is init-defaulted enrichment (every existing
// 3-arg construction binds, the authored sentinel and a Kubelka-Munk mix both defaulting Method to Authored):
// FitConditionNumber/FitRank carry the capture's own observability witness (FitRank < n with +Inf condition reads
// an under-observed fit — an in-plane capture whose constant azimuth cannot measure alphaY or the grain along it); DominantWavelengthNm/ExcitationPurity/
// CctKelvin/CctDuv carry the grounded base-color chromaticity Import stamps at the one re-admission site (CctDuv gates
// CctKelvin validity at |Duv| <= 0.05). A measured row is structurally `!= Authored`, the measured-vs-authored signal.
public sealed record Provenance(string Device, int WavelengthCount, double FitResidual) {
    public CaptureMethod Method { get; init; } = CaptureMethod.Authored;
    public int AngularSamples { get; init; }
    public double FitConditionNumber { get; init; }
    public int FitRank { get; init; }
    public double DominantWavelengthNm { get; init; }
    public double ExcitationPurity { get; init; }
    public double CctKelvin { get; init; }
    public double CctDuv { get; init; }

    // Calibrated says a chart-solved matrix corrected this capture and CalibrationDeltaE reports
    // the mean CIEDE2000 residual it left over the twenty-four patches. Both matter because a capture that reads
    // MEASURED on the strength of a photograph nothing white-balanced is an authored guess wearing a receipt, and a
    // solved matrix with a large residual is a chart photographed under conditions the linear model cannot correct.
    // Explicit presence rides the residual: an uncalibrated capture measures no residual, and a zero on a required
    // slot reads to a grader as a perfect chart solve. Its python peer already declares this column
    // `float | None` for exactly that reason, so a bare double here emits a proto zero that peer's absent arm can
    // never see, and both sides then disagree on what an uncalibrated capture reports.
    public Option<double> CalibrationDeltaE { get; init; }
    public bool Calibrated { get; init; }

    // ModelCard/License carry the attribution a neural capture holds and every other capture genuinely lacks — TYPED ABSENCE,
    // never an empty-string sentinel: a goniophotometer row has no model card, and Option collapses the reader's "no model" vs
    // "a model whose id happened to be blank" distinction. Both columns travel together because a licence is meaningless
    // without the artefact it governs, and the wire flattens the pair to their keys.
    public Option<ModelCardId> ModelCard { get; init; }
    public Option<LicenseClass> License { get; init; }

    // Ingest is the THIRD-PARTY custody evidence, carried as the Raster/set#SET_INGEST `IngestProvenance` shape
    // ITSELF rather than as a second spelling of source/licence/reference columns beside it. A capture and a
    // downloaded texture set are the same custody question at two grains — who produced this, under what declared
    // licence, and against what reference — so the two owners share one carrier and a consumer that can read an
    // ingested set's custody can read a capture's without learning a second vocabulary. Typed absence is the honest
    // default: an estate-produced goniophotometer capture has no third-party custody to declare, and a row that
    // filled the shape with its own device name would forge provenance out of a measurement it performed itself.
    public Option<IngestProvenance> Ingest { get; init; }

    // ModelArtefact addresses the BYTES the governing card ran: a card names a model and this
    // names the weights, so a receipt can answer which artefact produced a plane rather than only which row
    // authorized one. Optional at BOTH ends by the same reason the neural registry gives — caller-supplied weights
    // are a deployment fact the registry never addresses — so a mandatory column here would forge an address for
    // exactly the rows that have none.
    public Option<ContentAddress> ModelArtefact { get; init; }

    // Measured reads the METHOD, the one column that says what took the measurement — never a whole-record compare
    // against the authored default. Record equality made every enrichment column part of the answer, so a genuine
    // goniophotometer capture whose device, counts, and residual happened to match the default read AUTHORED, and a
    // new receipt column silently changed the predicate. One discriminant, one reading, and the wire mirror composes
    // this member rather than restating the comparison.
    public bool Measured => Method != CaptureMethod.Authored;

    public static readonly Provenance Authored = new("authored", 0, 0.0);

    // Provenance.Of stamps the acquisition arms with a measured device name, its spectral band or angular
    // sample count, the fit residual, and the CaptureMethod instrument — one factory so no arm hand-spells the receipt.
    public static Provenance Of(CaptureMethod method, string device, int wavelengthCount, int angularSamples, double fitResidual) =>
        new(device, wavelengthCount, fitResidual) { Method = method, AngularSamples = angularSamples };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptureSource {
    private CaptureSource() { }

    // Conductor is the Fresnel discriminant: Some carries the measured complex IOR (FresnelConductor, alphas-only fit,
    // Metalness 1.0); None fits the dielectric (alphaX, alphaY, eta) over the Ior seed.
    public sealed record MeasuredBrdf(Seq<BrdfSample> Samples, double Ior, Option<ComplexIor> Conductor) : CaptureSource;
    public sealed record SvbrdfMap(Seq<MaterialParameters> Texels) : CaptureSource;
    // Reflectance carries a curve rather than a bare Spd: a measured reflectance the estate can PERSIST and re-ground is the whole
    // point of a spectrophotometer capture, and an Spd cannot answer what grid it was built from.
    public sealed record SpectralReflectance(SpectralCurve Reflectance, double Metalness, double Roughness) : CaptureSource;

    // NeuralPlanes carries the photo-to-PBR PLANE product — an admitted Raster/set#TEXTURE_SET assembled from the
    // neural#MODEL_REGISTRY stage plan's outputs, beside the Seq<StageResult> evidence that produced it — and what SURVIVES
    // separates it from SvbrdfMap: SvbrdfMap collapses a per-texel parameter field to one row and discards it, while this arm
    // keeps the planes, so the acquired material stays spatially varying and the averaged row serves as the SUMMARY the seam
    // AppearanceSummary and the LOD fallback read, not the whole result. Fallback supplies the caller's own base row for every
    // column the set does not carry, so the arm names no library key and hardcodes no neutral.
    public sealed record NeuralPlanes(TextureSet Planes, Seq<StageResult> Stages, MaterialParameters Fallback) : CaptureSource;

    // GlossMeter carries the cheapest real measurement in the estate: a handheld tri-angle gloss meter reads
    // specular reflectance at 20, 60, and 85 degrees in GLOSS UNITS against a polished black-glass standard, and
    // those three numbers are genuine instrument evidence a specification already records for coatings, panels, and
    // finishes. The triple crosses whole rather than as one number because the ANGLES discriminate: 20 degrees
    // separates the high-gloss band the 60 reading saturates on, 85 separates the matte band it bottoms out on, and
    // a single-angle reading cannot tell a satin coating from a semi-gloss one. Base carries the caller's own row so
    // the arm answers roughness alone and invents no colour a gloss meter never measured.
    public sealed record GlossMeter(double Gu20, double Gu60, double Gu85, GlossCurve Curve, MaterialParameters Base) : CaptureSource;
}

// GlossCurve is the GU-to-roughness CALIBRATION as a caller-supplied policy row, not a table this page transcribes:
// the correspondence depends on the instrument's aperture, the standard it zeroes against, and the coating family
// being read, so an estate that ships one curve as law states a number nobody measured. The row carries the
// SELECTION band each angle governs and the monotone map itself as a delegate, so a lab that characterizes its own
// meter lands one row and every consumer of that lab's captures reads its calibration rather than a default.
// Angles are the instrument's, so the band edges are GU on the SAME scale the reading is.
public readonly record struct GlossCurve(double HighBand, double MatteBand, Func<double, double> Roughness) {
    // Select routes the reading to the angle whose band it falls in — the tri-angle instrument's own reading law,
    // stated once here rather than at each consumer: above the high band the 20-degree reading discriminates, below
    // the matte band the 85-degree one does, and the 60-degree reading governs the middle it was designed for.
    public double Of(double gu20, double gu60, double gu85) =>
        Roughness(gu60 >= HighBand ? gu20 : gu60 <= MatteBand ? gu85 : gu60);
}

// What an import PRODUCES: the parameter row every arm yields, its measured receipt, and — for a plane-bearing
// capture — the admitted set itself. The set is Option-typed rather than a fourth arm on a widened tuple because
// three of the four capture arms genuinely produce no planes, and a caller reading Planes.IsSome learns exactly
// whether a shadeable spatially-varying material came back or only its summary row.
public sealed record AcquiredMaterial(MaterialParameters Row, Provenance Provenance, Option<TextureSet> Planes) {
    public static AcquiredMaterial Summary(MaterialParameters row, Provenance provenance) => new(row, provenance, None);
}

// TensorField rows the ten fields an EPFL RGL `.bsdf` tensor container declares, each carrying the RANK its payload
// must hold — the reader gates on the ROW rather than on a positional roster a producer could reorder, and a field
// the container omits is a typed absence the admission names rather than an index that silently reads its neighbour.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TensorField {
    public static readonly TensorField Description = new("description", rank: 1);
    public static readonly TensorField ThetaI = new("theta_i", rank: 1);
    public static readonly TensorField PhiI = new("phi_i", rank: 1);
    public static readonly TensorField Sigma = new("sigma", rank: 2);
    public static readonly TensorField Ndf = new("ndf", rank: 2);
    public static readonly TensorField Vndf = new("vndf", rank: 4);
    public static readonly TensorField Luminance = new("luminance", rank: 4);
    public static readonly TensorField Rgb = new("rgb", rank: 5);
    public static readonly TensorField Spectra = new("spectra", rank: 5);
    public static readonly TensorField Wavelengths = new("wavelengths", rank: 1);

    public int Rank { get; }
}

// The tensor container's element vocabulary. The reader admits the float widths a measurement uses and refuses the
// rest at the field gate rather than at a cast, so a container carrying an unadmitted dtype names it.
[SmartEnum<byte>]
public sealed partial class TensorDtype {
    public static readonly TensorDtype UInt8 = new(key: 1, width: 1);
    public static readonly TensorDtype Int32 = new(key: 2, width: 4);
    public static readonly TensorDtype UInt32 = new(key: 3, width: 4);
    public static readonly TensorDtype Float16 = new(key: 4, width: 2);
    public static readonly TensorDtype Float32 = new(key: 5, width: 4);
    public static readonly TensorDtype Float64 = new(key: 6, width: 8);

    public int Width { get; }
}

// BrdfArchive is the admitted EPFL RGL container: the magic, the field roster, and the two facts a consumer must
// never guess. ISOTROPY IS DERIVED, never a flag — an isotropic measurement carries at most two incident-azimuth
// slices, so `phi_i.Length <= 2` IS the property and a container column asserting otherwise would be a second truth
// to reconcile. The WAVELENGTH COUNT is likewise a FILE fact read off the `wavelengths` field, never a pinned
// literal: the published materials sample anywhere from a handful of bands to nearly two hundred, and a reader
// carrying a constant admits exactly one dataset and mis-strides every other.
public sealed record BrdfArchive(HashMap<TensorField, ReadOnlyMemory<double>> Fields, Seq<int> Extents) {
    public const ulong Magic = 0x00465344425F4C54;   // "TLB_FSDF" — the tensor container's own leading word

    public bool Isotropic => Fields.Find(TensorField.PhiI).Map(static phi => phi.Length <= 2).IfNone(true);
    public int WavelengthCount => Fields.Find(TensorField.Wavelengths).Map(static grid => grid.Length).IfNone(0);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Acquisition {
    static Acquisition() => Control.UseManaged();   // provider selected ONCE — osx-arm64 rides managed (native MKL/OpenBLAS are x64-only; the per-call-site TryUseNativeMKL is the named defect)

    // One import entry pairing the produced row with its measured Provenance (the finish#FINISH Resolve shape): the
    // MeasuredBrdf arm runs the overdetermined anisotropic conductor/dielectric fit and stamps the goniophotometer
    // receipt with the witnessed residual + conditioning, the SpectralReflectance arm grounds and stamps the
    // spectrophotometer method, the SvbrdfMap arm averages the per-texel field and stamps the neural texel count; every
    // row re-admits through MaterialParameters.Of, and the ONE re-admission site stamps the admitted row's grounded
    // base-color chromaticity onto the receipt uniformly (the photometric#PHOTOMETRIC EmissionInput Wxy precedent).
    public static Fin<AcquiredMaterial> Import(CaptureSource source, Provenance provenance, Op key, Option<CaptureCalibration> calibration = default) =>
        source.Switch(
            state: (provenance, key),
            measuredBrdf:        static (s, c) => FitBrdf(c.Samples, c.Ior, c.Conductor, s.provenance, s.key),
            svbrdfMap:           static (s, c) => AverageField(c.Texels, s.provenance, s.key),
            spectralReflectance: static (s, c) => GroundSpectral(c.Reflectance.ToSpd(), c.Metalness, c.Roughness, s.key)
                                                    .Map(row => AcquiredMaterial.Summary(row, s.provenance with { Method = CaptureMethod.Spectrophotometer })),
            neuralPlanes:        static (s, c) => ImportPlanes(c.Planes, c.Stages, c.Fallback, s.provenance, s.key),
            glossMeter:          static (s, c) => FitGloss(c, s.provenance, s.key))
        .Bind(acquired => Calibrate(acquired, calibration, key))
        .Bind(acquired => MaterialParameters.Of(acquired.Row, key).Map(row => acquired with { Row = row, Provenance = acquired.Provenance with {
            DominantWavelengthNm = row.BaseColor.DominantWavelength,
            ExcitationPurity = row.BaseColor.ExcitationPurity,
            CctKelvin = row.BaseColor.Temperature.Cct,
            CctDuv = row.BaseColor.Temperature.Duv } }));

    // FitGloss lands the instrument reading as ROUGHNESS ALONE on the caller's own base row: a gloss meter measures
    // specular reflectance and nothing else, so writing a colour, a metalness, or an anisotropy here would fabricate
    // the columns the instrument is silent about. The GU triple crosses onto the receipt as its angular sample count
    // and the mapped roughness carries NO residual, because a calibrated single-point reading has no fit to leave
    // one — a zero residual on this arm is the honest statement that nothing was solved, not a perfect solve.
    static Fin<AcquiredMaterial> FitGloss(CaptureSource.GlossMeter c, Provenance provenance, Op key) =>
        double.IsFinite(c.Gu20) && double.IsFinite(c.Gu60) && double.IsFinite(c.Gu85)
        && c.Gu20 >= 0.0 && c.Gu60 >= 0.0 && c.Gu85 >= 0.0
            ? c.Curve.Of(c.Gu20, c.Gu60, c.Gu85) switch {
                var roughness when double.IsFinite(roughness) && roughness is >= 0.0 and <= 1.0 =>
                    Fin.Succ(AcquiredMaterial.Summary(
                        c.Base with { Roughness = roughness },
                        Provenance.Of(CaptureMethod.GlossMeter, provenance.Device, provenance.WavelengthCount,
                            angularSamples: 3, fitResidual: 0.0))),
                var roughness => MaterialFault.Parameter(key, $"<gloss-curve-out-of-unit:{roughness:R}>"),
            }
            : MaterialFault.Parameter(key, $"<gloss-reading-out-of-range:{c.Gu20:R},{c.Gu60:R},{c.Gu85:R}>");

    // Calibrate sits at the ONE pre-admission site every arm crosses rather than inside the neural arm
    // alone: a photograph feeds the SVBRDF field and the plane set alike, a goniophotometer capture simply passes
    // None, and a per-arm copy would drift the moment a fifth capture modality lands. The solve runs in AP1-linear —
    // Macbeth publishes under sRGB/D50, so ConvertToConfiguration rebases BOTH sides onto the scene-linear working
    // space FIRST (the same colorimetric boundary graph#MATERIAL_LIBRARY Named enforces) and the matrix is then a
    // correction within one space rather than a fit that silently absorbs a chromatic adaptation.
    static Fin<AcquiredMaterial> Calibrate(AcquiredMaterial acquired, Option<CaptureCalibration> calibration, Op key) =>
        calibration
            .Map(chart => Solve(chart, key).Map(fit => acquired with {
                Row = acquired.Row with { BaseColor = Correct(acquired.Row.BaseColor, fit.Matrix) },
                Provenance = acquired.Provenance with { Calibrated = true, CalibrationDeltaE = Some(fit.DeltaE) } }))
            .IfNone(Fin.Succ(acquired));

    // Least squares over the twenty-four patch rows: the measured AP1 triples are the design matrix, the references
    // the multi-right-hand-side, and one thin-QR solve returns the 3x3 the whole capture is multiplied by — the SAME
    // dense-overdetermined route SolveGgx composes, never a second numeric owner. The residual is the mean CIEDE2000
    // the corrected patches still carry, which is the number that separates a usable chart shot from a hopeful one.
    static Fin<(Matrix<double> Matrix, double DeltaE)> Solve(CaptureCalibration chart, Op key) {
        Seq<Unicolour> reference = toSeq(Macbeth.All).Map(static patch => patch.ConvertToConfiguration(PortValue.SceneLinear));
        Matrix<double> measured = Matrix<double>.Build.Dense(chart.Patches.Count, 3, (row, band) => Band(chart.Patches[row], band));
        Matrix<double> target = Matrix<double>.Build.Dense(reference.Count, 3, (row, band) => Band(reference[row], band));
        Matrix<double> fit = measured.QR(QRMethod.Thin).Solve(target);
        return fit.Enumerate().All(double.IsFinite)
            ? Fin.Succ((fit, chart.Patches
                .Map((patch, index) => Correct(patch, fit).Difference(reference[index], DeltaE.Ciede2000))
                .Fold(0.0, static (sum, delta) => sum + delta) / chart.Patches.Count))
            : MaterialFault.Parameter(key, "<capture-calibration-degenerate>");
    }

    static double Band(Unicolour colour, int index) =>
        colour.RgbLinear.Triplet switch { var t => index is 0 ? t.First : index is 1 ? t.Second : t.Third };

    static Unicolour Correct(Unicolour colour, Matrix<double> fit) =>
        colour.RgbLinear.Triplet switch {
            var t => Linear(
                (t.First * fit[0, 0]) + (t.Second * fit[1, 0]) + (t.Third * fit[2, 0]),
                (t.First * fit[0, 1]) + (t.Second * fit[1, 1]) + (t.Third * fit[2, 1]),
                (t.First * fit[0, 2]) + (t.Second * fit[1, 2]) + (t.Third * fit[2, 2])),
        };

    // Freeze/Thaw give the ONE measured product this estate produces and could not persist a durable container: a
    // wavelength-sampled EXR part, one texel wide, whose channel names ARE the sampling grid. A RasterFormat row is
    // the WRONG home — a wavelength grid is not a texture plane, and admitting it as one would put a spectrum through
    // extent, transfer, and mip gates that describe pixels. Provenance.WavelengthCount becomes the round-trip
    // WITNESS rather than a bare count: a thaw whose decoded grid disagrees with the receipt refuses.
    public static Fin<ReadOnlyMemory<byte>> Freeze(SpectralCurve curve, SpectralKind kind, Op key) {
        float[] wavelengths = [.. Enumerable.Range(0, curve.Coefficients.Length).Select(i => (float)curve.WavelengthAt(i))];
        float[] samples = [.. curve.Coefficients.ToArray().Select(static v => (float)v)];
        WriterResult<byte[]> written = ExrFile.SaveToMemory(
            new TinyEXR.V3.Image([kind.Part(width: 1, height: 1, wavelengths, samples, units: "nm", Compression.ZIP)]),
            Compression.ZIP, options: null);
        return written is { IsSuccess: true, Value: { } bytes }
            ? Fin.Succ((ReadOnlyMemory<byte>)bytes)
            : MaterialFault.Parameter(key, $"<spectral-freeze:{written.Status}>");
    }

    // Thaw returns the KIND beside the curve, so the round trip preserves the physical claim rather than the claim
    // surviving only in whichever caller happened to know it. A part whose spectrum type this page carries no row
    // for — a polarised measurement — refuses here rather than being read as an unpolarised reflectance.
    public static Fin<(SpectralCurve Curve, SpectralKind Kind)> Thaw(ReadOnlyMemory<byte> payload, int wavelengthCount, Op key) {
        ReaderResult<TinyEXR.V3.Image> read = ExrFile.LoadFromMemory(payload, options: null);
        return read is { IsSuccess: true, Value: { } file } && toSeq(file.Parts).Head.Case is Part part && Spectral.IsSpectral(part.Header)
            ? SpectralKind.Of(Spectral.GetSpectrumType(part.Header)).Case is SpectralKind kind
                ? Spectral.GetWavelengths(part.Header) is { Length: > 1 } grid && grid.Length == wavelengthCount
                    ? Samples(part, key).Bind(samples =>
                          SpectralCurve.Of((int)grid[0], (int)Math.Round(grid[1] - grid[0]), samples, key).Map(curve => (curve, kind)))
                    : MaterialFault.Parameter(key, $"<spectral-thaw-grid:{wavelengthCount}>")
                : MaterialFault.Parameter(key, $"<spectral-thaw-kind:{Spectral.GetSpectrumType(part.Header)}>")
            : MaterialFault.Parameter(key, $"<spectral-thaw:{read.Status}>");
    }

    // The re-ingest PARSES the channel names the container actually holds instead of re-minting the names this page
    // would have written and looking those up: a part written at another grid, by another producer, or under another
    // spectrum type then answers honestly rather than missing every lookup a mint assumed. TryGetStokesComponent is
    // not optional beside TryParseChannelWavelength — a spectral read that parses wavelengths alone flattens the four
    // Stokes components of one wavelength onto one channel — so the pair travels together and a non-zero component is
    // DROPPED rather than summed into an unpolarised sample it is not. Ordering is by parsed wavelength, so the grid
    // the header declares and the samples this returns index the same spectrum by construction.
    static Fin<ReadOnlyMemory<double>> Samples(Part part, Op key) =>
        part.GetLevel(0, 0) switch {
            var level => toSeq(level.Channels)
                .Filter(Spectral.IsSpectralChannel)
                .Choose(name => Spectral.TryParseChannelWavelength(name, out float nm)
                             && Spectral.TryGetStokesComponent(name, out int stokes) && stokes == 0
                                ? Some((Nm: nm, Name: name))
                                : Option<(float Nm, string Name)>.None)
                .OrderBy(static channel => channel.Nm)
                .Map(channel => (double)MemoryMarshal.Cast<byte, float>(level.GetChannel(channel.Name).Data.Span)[0]) switch {
                var samples => samples.IsEmpty
                    ? Fin.Fail<ReadOnlyMemory<double>>(MaterialFault.Parameter(key, "<spectral-thaw-no-channels>"))
                    : Fin.Succ((ReadOnlyMemory<double>)samples.ToArray()),
            },
        };

    // ImportPlanes closes the round trip photo → stages → planes → SHADEABLE MATERIAL: the admitted set binds through the ONE
    // Raster/set#SET_BIND Average target, whose measured per-channel fold over each plane's base level is the summary the seam
    // key carries — never a pyramid tail, never a re-derivation here — and the set rides back beside it so the caller keeps the
    // spatially-varying material rather than only its mean.
    static Fin<AcquiredMaterial> ImportPlanes(TextureSet planes, Seq<StageResult> stages, MaterialParameters fallback, Provenance provenance, Op key) =>
        stages.IsEmpty
            ? MaterialFault.Parameter(key, "<neural-planes-no-stage-evidence>")
            : from binding in SetBind.Bind(planes, fallback, BindTarget.Summary, SamplerState.Default, key)
              from row in binding is SetBinding.Row bound
                  ? Fin.Succ(bound.Parameters)
                  : Fin.Fail<MaterialParameters>(MaterialFault.Graph(key, "<neural-planes-summary-not-a-row>"))
              select new AcquiredMaterial(row, Attributed(provenance, planes, stages), Some(planes));

    // Attribution over a MULTI-MODEL plan: the receipt carries the MOST RESTRICTIVE card in the chain, never the
    // last or the largest, because the grant a consumer must honour is the strictest one any contributing model
    // imposes — and the residual is likewise the WORST golden delta, since a set is only as trustworthy as its
    // weakest stage. Tiles inferred fill the sample count the goniophotometer arm fills with angular samples: both
    // are the count of measurement units the receipt's own instrument produced. A stage whose card left the
    // registry contributes its result without an attribution rather than blocking the import — the plan already
    // gated the grant at request construction, and this column is evidence rather than a second gate. The device
    // column carries the SET key, so a receipt names the exact planes it summarizes without a second identity field.
    static Provenance Attributed(Provenance provenance, TextureSet planes, Seq<StageResult> stages) {
        Option<ModelCard> governing = stages
            .Choose(static result => ModelRegistry.Rows.TryGetValue(result.ModelCardId, out ModelCard? card) ? Some(card!) : Option<ModelCard>.None)
            .Fold(Option<ModelCard>.None, static (strictest, card) => strictest.Filter(held => held.License.Rank >= card.License.Rank).IsSome ? strictest : Some(card));
        return Provenance.Of(CaptureMethod.NeuralPlanes, planes.Digest.ToValue(), provenance.WavelengthCount,
                angularSamples: stages.Fold(0, static (tiles, result) => tiles + result.TilesEmitted),
                fitResidual: stages.Fold(0.0, static (worst, result) => Math.Max(worst, result.GoldenDelta)))
            with { ModelCard = governing.Map(static card => card.Id), License = governing.Map(static card => card.License),
                   ModelArtefact = governing.Bind(static card => card.Artefact) };
    }

    // SyntheticGrid mints the deterministic capture the benchmark corpus pins (BenchInput.Synthetic → GgxFit): geometry is a
    // ⌈√count⌉-lane stratified (θi, θo) grid with Δφ swept uniformly over [0, π) so alphaX ≠ alphaY stays observable,
    // each zenith jittered inside its cell by a SplitMix64 stream off the seed; the reflectance column is the
    // kernel-composed dielectric forward model at seed-derived ground-truth alphas, so the fit workload has a known
    // answer and a byte-stable capture — no fixture file, no RNG state outside (seed, count).
    public static Fin<Seq<BrdfSample>> SyntheticGrid(int seed, int count, Op key) {
        if (count < 1) { return MaterialFault.Parameter(key, $"<synthetic-grid-count:{count}>"); }
        var (alphaX, alphaY) = (0.1 + 0.4 * Draw(seed, 0), 0.1 + 0.4 * Draw(seed, 1));
        double rotation = Draw(seed, 2) * double.Pi;                                   // ground-truth grain azimuth the fit must recover
        // ⌈√count⌉ lanes: count ≤ lanes² makes index / lanes < lanes by construction, so the row index needs no
        // modulo wrap — the floor-lane form aliased the overflow row onto row 0 and double-filled one stratum.
        int lanes = int.Max((int)double.Ceiling(double.Sqrt(count)), 1);
        return Fin.Succ(toSeq(Enumerable.Range(0, count).Select(index => {
            double thetaI = (index / lanes + Draw(seed, 2L * index + 3L)) / lanes * (double.Pi / 2.0 - 0.02);
            double thetaO = (index % lanes + Draw(seed, 2L * index + 4L)) / lanes * (double.Pi / 2.0 - 0.02);
            double deltaPhi = (index + 0.5) / count * double.Pi;
            var sample = new BrdfSample(thetaI, thetaO, deltaPhi, RgbSpectrum.Black);
            return sample with { Reflectance = RgbSpectrum.White.Scale(Reflectance(sample, alphaX, alphaY, rotation, eta: 1.5, conductor: None)) };
        })));
    }

    // Draw defers to the KERNEL — Rasm.Domain Deterministic owns the one splitmix64 stream, and its lane fold
    // mixes each lane into the state where a local multiply-then-use folded them by one multiply and lost the
    // finalizer's avalanche. The seed rides the dedicated seed argument rather than a leading lane, so a full
    // 64-bit policy seed stays one value and two captures never collide by re-packing seed and lane into one span.
    static double Draw(int seed, long lane) => Deterministic.Unit(lanes: [lane], seed: seed);

    // SolveGgx fits overdetermined GGX/Smith/Fresnel least squares through a thin-QR Gauss-Newton iteration over the angular
    // reflectance design matrix (the m measured BrdfSample geometries the rows, the bounded parameter vector the
    // n ∈ {3,4} unknowns — (alphaX, alphaY, phi) under a supplied conductor IOR, (alphaX, alphaY, phi, eta) for the dielectric —
    // Acquisition.Reflectance the bsdf#MICROFACET_KERNEL forward model) — the Rasm.Compute/Tensor/blas#DENSE_ALGEBRA
    // dense-overdetermined route shape, never a hand-rolled Levenberg-Marquardt loop. Fitted alphas project to the
    // Disney Roughness/Anisotropy row columns; the recomputed true residual and the converged-Jacobian conditioning
    // ride the goniophotometer-stamped receipt bsdf#BSDF_GOLDEN gates and interchange#MATERIAL_WIRE
    // WireProvenance carries, real measured evidence vs an authored guess. Provenance rides ALONGSIDE the row
    // (the finish#FINISH Resolve precedent), never a phantom column on MaterialParameters, whose positional core is closed and whose every later axis is init-defaulted enrichment.
    static Fin<AcquiredMaterial> FitBrdf(Seq<BrdfSample> samples, double ior, Option<ComplexIor> conductor, Provenance provenance, Op key) =>
        guard(samples.Count > (conductor.IsSome ? 3 : 4), MaterialFault.Parameter(key, $"<measured-brdf-underdetermined:{samples.Count}>")).ToFin()
            .Bind(_ => SolveGgx(samples, ior, conductor, key))
            .Bind(fit => SpectralUpsample.ToSpd(
                    samples.Map(static s => s.Reflectance).Fold(RgbSpectrum.Black, static (acc, r) => acc.Add(r)).Scale(1.0 / samples.Count), key)
                .Bind(spd => GroundSpectral(spd, metalness: conductor.IsSome ? 1.0 : 0.0, fit.Roughness, key))
                .Map(row => AcquiredMaterial.Summary(
                             row with { Ior = fit.Eta, Anisotropy = fit.Anisotropy, AnisotropyRotation = fit.Rotation },
                             Provenance.Of(CaptureMethod.Goniophotometer, provenance.Device, provenance.WavelengthCount, samples.Count, fit.Residual)
                                 with { FitConditionNumber = fit.ConditionNumber, FitRank = fit.Rank })));

    // BrdfFit carries the fitted Disney projection — magnitude AND direction — the witnessed relative residual, and
    // the observability witness that says whether the direction was measurable at all.
    readonly record struct BrdfFit(double Roughness, double Anisotropy, double Rotation, double Eta, double Residual, double ConditionNumber, int Rank);

    // SolveGgx hands the capture to the KERNEL functor and reads its receipt. What is left here is the RESIDUAL —
    // the only part of a fit that is this page's own knowledge — and the projection of the converged parameters onto
    // the row; every iteration mechanic belongs to Rasm/Solving/solver and none of it is respelled.
    static Fin<BrdfFit> SolveGgx(Seq<BrdfSample> samples, double ior0, Option<ComplexIor> conductor, Op key) =>
        new BrdfResidual(samples, ior0, conductor) switch {
            var residual => Lm.Minimize(new DualModel(residual), SolvePolicy.Canonical, key)
                .Bind(result => residual.Project(result, key)),
        };

    // BrdfResidual states the capture's residual ROW AT A TIME in dual arithmetic and lets DualModel derive the exact
    // Jacobian — the differencing step this page used to choose is gone, and with it the question of what step size
    // an anisotropic GGX surface tolerates. The forward model stays SINGLE-SOURCED on bsdf#MICROFACET_KERNEL: that
    // kernel is generic over its scalar, so the SAME body the synthetic capture evaluates at `double` evaluates here
    // at the solver's dual scalar and the fit differentiates precisely what it predicts.
    // Bounds ride a differentiable BOX REPARAMETERIZATION, never a clamp: each column is p = Lo + (Hi − Lo)·σ(u) with
    // σ the logistic, so the solver optimizes an UNCONSTRAINED u, every iterate is interior by construction, and the
    // map is smooth everywhere. A clamp has no derivative at its own boundary, so the ladder's accept/reject step
    // reads a zero gradient exactly where the box binds — which is precisely where an in-plane capture's alphaY sits,
    // the one case the conditioning readout exists to report honestly.
    sealed class BrdfResidual(Seq<BrdfSample> samples, double ior0, Option<ComplexIor> conductor) : IDualResidual {
        // The phi box spans a HALF turn: an ellipse is symmetric under a half rotation, so [0, π) is the whole
        // observable domain and a [0, 2π) box would carry two minima the ladder could oscillate between.
        static readonly (double Lo, double Hi)[] AlphaPhi = [(1e-4, 1.0), (1e-4, 1.0), (0.0, Math.PI)];
        static readonly (double Lo, double Hi)[] AlphaPhiEta = [.. AlphaPhi, (1.0, 2.5)];

        internal (double Lo, double Hi)[] Bounds => conductor.IsSome ? AlphaPhi : AlphaPhiEta;
        public int Dof => Bounds.Length;             // [alphaX, alphaY, phi(, eta)] — the conductor Fresnel is measured, not fit
        public int Rows => samples.Count;

        // The seed is the BOUNDED parameter inverted through the reparameterization, so the solver starts from an
        // unconstrained coordinate whose image is the physical seed rather than from a raw value the box would clip.
        public double[] Seed => [.. Bounds.Zip(
            conductor.IsSome
                ? [Microfacet<double>.AlphaOf(0.3), Microfacet<double>.AlphaOf(0.3), 0.0]
                : [Microfacet<double>.AlphaOf(0.3), Microfacet<double>.AlphaOf(0.3), 0.0, Math.Clamp(ior0, 1.0, 2.5)],
            static (box, value) => Free(value, box))];

        // Each row is the LOG residual: reflectance spans decades across a capture, so a linear residual lets the
        // near-specular geometries dominate every step and the grazing tail contributes nothing to the fit.
        public Dual<ddouble> Row(int index, ReadOnlySpan<Dual<ddouble>> parameters) =>
            Dual<ddouble>.Of(ddouble.Log(ddouble.Max(Floor, (ddouble)samples[index].Reflectance.Luminance)))
            - Dual<ddouble>.Log(Model(samples[index], parameters));

        // The bounded parameter vector, reconstructed from the solver's unconstrained coordinates inside the dual
        // arithmetic so the box map itself differentiates rather than sitting outside the derivative chain.
        Dual<ddouble> Model(BrdfSample sample, ReadOnlySpan<Dual<ddouble>> u) =>
            Reflectance(sample,
                alphaX:   Bound(u[0], Bounds[0]), alphaY: Bound(u[1], Bounds[1]), rotation: Bound(u[2], Bounds[2]),
                eta:      u.Length == 4 ? Bound(u[3], Bounds[3]) : Dual<ddouble>.Of((ddouble)1.5),
                conductor: conductor);

        // Project reads the ladder's receipt onto the row: the parameters bound back through the same map, and the
        // witnessed relative residual off the functor's OWN 106-bit objective rather than a norm recomputed here.
        internal Fin<BrdfFit> Project(LmResult result, Op key) =>
            result.IsValid
                ? [.. Bounds.Zip(result.Parameters, static (box, free) => Bound(free, box))] switch {
                    var p => Fin.Succ(new BrdfFit(
                        Roughness: Math.Sqrt(Math.Sqrt(p[0] * p[1])),                                  // alpha_geo = √(αx·αy) = roughness²
                        Anisotropy: Math.Clamp((1.0 - (Math.Min(p[0], p[1]) / Math.Max(p[0], p[1]))) / 0.9, 0.0, 1.0),   // inverse Disney aspect² = min/max
                        // Rotation projects the fitted azimuth onto the row's UNIT column (1 is a half turn), taken
                        // from the ALPHA-X axis: when the fit lands alphaY as the rougher of the pair the rougher
                        // axis is a quarter turn away, so the projection adds that quarter rather than reporting the
                        // smooth axis as the grain.
                        Rotation: Math.Clamp(((p[2] + (p[0] >= p[1] ? 0.0 : Math.PI / 2.0)) / Math.PI) % 1.0, 0.0, 1.0),
                        Eta: p.Length == 4 ? p[3] : 1.5,                                               // conductor rows keep the coat-side Disney default — Ior is unread at Metalness 1
                        Residual: result.Norm / Math.Max(1e-6, Scale),
                        ConditionNumber: Observable ? 1.0 / Math.Max(1e-12, AzimuthSpread) : double.PositiveInfinity,
                        Rank: Observable ? Dof : Dof - 2))
                }
                : MaterialFault.Parameter(key, "<ggx-fit-diverged>");

        // The observability witness is a property of the CAPTURE GEOMETRY, not of a converged Jacobian, so it costs
        // one pass over the samples and it names its own cause. An in-plane capture — every sample at one relative
        // azimuth — cannot observe the second alpha OR the grain it lies along, so it reports two unobserved columns
        // with an infinite conditioning number and the receipt says under-observed instead of the fit fabricating a
        // grain. Deriving it from the capture is what lets the kernel functor own the numerics whole: the ladder
        // already regularizes a deficient system into SOME answer, and what a reader needs is the reason that answer
        // is not evidence.
        double AzimuthSpread => samples.Fold((Lo: double.MaxValue, Hi: double.MinValue),
            static (span, s) => (Math.Min(span.Lo, s.AzimuthDelta), Math.Max(span.Hi, s.AzimuthDelta))) switch {
            var span => span.Hi - span.Lo,
        };
        bool Observable => AzimuthSpread > 1e-3;

        // The log-measured magnitude the relative residual normalizes against — the capture's own scale, folded once.
        double Scale => Math.Sqrt(samples.Fold(0.0, static (sum, s) =>
            Math.Log(Math.Max(1e-6, s.Reflectance.Luminance)) switch { var l => sum + (l * l) }));

        static readonly ddouble Floor = (ddouble)1e-6;

        // The logistic box and its inverse: one pair, so a bound and a seed can never disagree about the same box.
        static Dual<ddouble> Bound(Dual<ddouble> free, (double Lo, double Hi) box) =>
            Dual<ddouble>.Of((ddouble)box.Lo)
            + (Dual<ddouble>.Of((ddouble)(box.Hi - box.Lo))
               / (Dual<ddouble>.Of(ddouble.One) + Dual<ddouble>.Exp(-free)));
        static double Bound(double free, (double Lo, double Hi) box) => box.Lo + ((box.Hi - box.Lo) / (1.0 + Math.Exp(-free)));
        static double Free(double bounded, (double Lo, double Hi) box) =>
            Math.Clamp((bounded - box.Lo) / (box.Hi - box.Lo), 1e-9, 1.0 - 1e-9) switch { var t => Math.Log(t / (1.0 - t)) };
    }

    // Reflectance SINGLE-SOURCES the microfacet forward model on bsdf#MICROFACET_KERNEL: reconstruct the real incident/outgoing
    // LocalVector<T> directions from a sample's (θi, θo, Δφ), form the true half-vector, and read the kernel's anisotropic
    // GGX NDF + Smith height-correlated masking + the discriminant-routed Fresnel — the measured-conductor luminance over
    // ComplexIor, or the dielectric term over eta — NEVER a re-minted D/G/F with a cos((θi−θo)/2) specular-plane
    // half-angle that drops the azimuth (Δφ is what makes alphaX ≠ alphaY observable). alphas are GGX alphas directly
    // (roughness² per the Disney remap); the per-cell evaluation rides the SolveGgx kernel carve-out.
    // Reflectance is GENERIC over its scalar, which is what lets ONE body serve both consumers: the synthetic capture
    // evaluates it at `double` and the residual row evaluates the SAME body at the solver's dual scalar, so the fit
    // differentiates exactly what it predicts and no second transcription exists to drift. The conductor arm lifts
    // the measured bands into the scalar and folds them through the kernel's own luminance weights, so the Fresnel
    // discriminant stays the capture's own fact rather than a branch the solver has to see through.
    static T Reflectance<T>(BrdfSample s, T alphaX, T alphaY, T rotation, T eta, Option<ComplexIor> conductor)
        where T : INumber<T>, IRootFunctions<T>, IPowerFunctions<T>, IExponentialFunctions<T>, ILogarithmicFunctions<T>, ITrigonometricFunctions<T> {
        (T zi, T zo, T dphi) = (T.CreateChecked(s.IncidentZenith), T.CreateChecked(s.OutgoingZenith), T.CreateChecked(s.AzimuthDelta));
        LocalVector<T> wi = new LocalVector<T>(T.Sin(zi), T.Zero, T.Cos(zi)).Normalize().RotateZ(-rotation);
        LocalVector<T> wo = new LocalVector<T>(T.Sin(zo) * T.Cos(dphi), T.Sin(zo) * T.Sin(dphi), T.Cos(zo)).Normalize().RotateZ(-rotation);
        T floor = T.CreateChecked(1e-6);
        if (!wi.SameHemisphere(wo)) { return floor; }
        LocalVector<T> h = wi.Add(wo).Normalize();
        T d = Microfacet<T>.Ndf(h, alphaX, alphaY);
        T g = Microfacet<T>.MaskingShadowing(wo, wi, alphaX, alphaY);
        T f = conductor.Match(
            Some: ior => Luminance<T>(ior, T.Abs(wo.Dot(h))),
            None: () => Microfacet<T>.FresnelDielectric(wo.Dot(h), eta));
        T guard = T.CreateChecked(1e-4);
        T denom = T.CreateChecked(4.0) * T.Max(guard, T.Abs(wi.CosTheta)) * T.Max(guard, T.Abs(wo.CosTheta));
        return T.Max(floor, d * g * f / denom);
    }

    // The measured conductor's per-band Fresnel reduced to the ONE luminance the log residual reads, folded through
    // the bsdf#LOBE_FAMILY luminance-weight owner so the reduction follows a working-space change rather than pinning
    // a triple this page would have to maintain.
    static T Luminance<T>(ComplexIor ior, T cosI)
        where T : INumber<T>, IRootFunctions<T>, IPowerFunctions<T>, IExponentialFunctions<T>, ILogarithmicFunctions<T>, ITrigonometricFunctions<T> =>
        (T.CreateChecked(RgbSpectrum.LuminanceWeights.R) * Microfacet<T>.FresnelConductor(cosI, T.CreateChecked(ior.Eta.R), T.CreateChecked(ior.K.R)))
        + (T.CreateChecked(RgbSpectrum.LuminanceWeights.G) * Microfacet<T>.FresnelConductor(cosI, T.CreateChecked(ior.Eta.G), T.CreateChecked(ior.K.G)))
        + (T.CreateChecked(RgbSpectrum.LuminanceWeights.B) * Microfacet<T>.FresnelConductor(cosI, T.CreateChecked(ior.Eta.B), T.CreateChecked(ior.K.B)));

    // AverageField computes the REAL per-texel SVBRDF mean — the neural fit's spatially-varying field collapsed to ONE representative
    // row in a single fold pass (never a texels.Head first-row read, never 17 LINQ traversals): FieldSum accumulates the
    // scene-linear base/emission channels and every Disney scalar + SubsurfaceRadius band, then Mean divides once. The
    // base/emission average runs in RgbLinear (the shading-truth channel) so two texels' colors blend physically, not in
    // a display-encoded space. Averaged rows re-admit through MaterialParameters.Of (the Import Bind), so a degenerate
    // mean rails the row gate rather than passing a bad scatter band; the neural-svbrdf method + texel count stamp the receipt.
    static Fin<AcquiredMaterial> AverageField(Seq<MaterialParameters> texels, Provenance provenance, Op key) =>
        texels.IsEmpty
            ? MaterialFault.Parameter(key, "<svbrdf-map-empty>")
            : Fin.Succ(AcquiredMaterial.Summary(
                        texels.Fold(FieldSum.Zero, static (acc, t) => acc.Add(t)).Mean(texels.Count),
                        Provenance.Of(CaptureMethod.NeuralSvbrdf, provenance.Device, provenance.WavelengthCount, texels.Count, provenance.FitResidual)));

    // FieldSum accumulates in one pass: the six scene-linear base/emission channels (read once through RgbLinear), the
    // twelve Disney scalars with EmissionLuminance, and the 3 SubsurfaceRadius bands, summed in one fold and divided in
    // Mean — the fold algebra the immutable-accumulation law mandates over a mutable running total. Film is NOT averaged:
    // a neural field carries no interference column, so the mean row keeps the init-defaulted ThinFilm.None.
    readonly record struct FieldSum(
        double BaseR, double BaseG, double BaseB, double EmitR, double EmitG, double EmitB,
        double Metalness, double Roughness, double SpecularTint, double Anisotropy, double Ior,
        double Transmission, double TransmissionRoughness, double Sheen, double SheenTint, double Clearcoat, double ClearcoatRoughness,
        double Subsurface, double EmissionLuminance, double RadiusR, double RadiusG, double RadiusB) {
        public static readonly FieldSum Zero = default;
        public FieldSum Add(MaterialParameters t) {
            ColourTriplet b = t.BaseColor.RgbLinear.Triplet, e = t.Emission.RgbLinear.Triplet;
            return new FieldSum(BaseR + b.First, BaseG + b.Second, BaseB + b.Third, EmitR + e.First, EmitG + e.Second, EmitB + e.Third,
                Metalness + t.Metalness, Roughness + t.Roughness, SpecularTint + t.SpecularTint, Anisotropy + t.Anisotropy, Ior + t.Ior,
                Transmission + t.Transmission, TransmissionRoughness + t.TransmissionRoughness, Sheen + t.Sheen, SheenTint + t.SheenTint, Clearcoat + t.Clearcoat, ClearcoatRoughness + t.ClearcoatRoughness,
                Subsurface + t.Subsurface, EmissionLuminance + t.EmissionLuminance, RadiusR + t.SubsurfaceRadius.R, RadiusG + t.SubsurfaceRadius.G, RadiusB + t.SubsurfaceRadius.B);
        }
        public MaterialParameters Mean(int count) {
            double n = Math.Max(1, count);
            return new MaterialParameters(
                BaseColor: Linear(BaseR / n, BaseG / n, BaseB / n), Metalness: Metalness / n, Roughness: Roughness / n, SpecularTint: SpecularTint / n,
                Anisotropy: Anisotropy / n, Ior: Ior / n, Transmission: Transmission / n, TransmissionRoughness: TransmissionRoughness / n,
                Sheen: Sheen / n, SheenTint: SheenTint / n, Clearcoat: Clearcoat / n, ClearcoatRoughness: ClearcoatRoughness / n,
                Subsurface: Subsurface / n, SubsurfaceRadius: SubsurfaceRadius.Create(RadiusR / n, RadiusG / n, RadiusB / n),
                Emission: Linear(EmitR / n, EmitG / n, EmitB / n), EmissionLuminance: EmissionLuminance / n);
        }
    }

    // GroundSpectral resolves measured Spd to a scene-linear Acescg base color through the ONE
    // PortValue.SceneLinear working space (a re-minted inline Configuration forks the Unicolour lazy-conversion cache).
    // Gamut admission rejects FIRST (an out-of-gamut capture faults, never a silent chroma reduction), then the SceneLinear
    // grounding is CONSUMED — the finite-gated grounded triple IS the row base color, never a discarded validation binding.
    // Named-arg construction over the closed positional core makes a column reorder break loudly at compile time; every init-defaulted enrichment column takes its own default, which is what a spectrophotometer capture genuinely measures.
    static Fin<MaterialParameters> GroundSpectral(Spd reflectance, double metalness, double roughness, Op key) =>
        from color in Fin.Succ(new Unicolour(PortValue.SceneLinear, reflectance))
        from _ in guard(GamutPolicy.Perceptual.Contains(color), MaterialFault.Gamut(key, "<acquired-color-out-of-gamut>"))
        from grounded in SpectralUpsample.SceneLinear(color, key)
        select new MaterialParameters(
            BaseColor: Linear(grounded.R, grounded.G, grounded.B), Metalness: metalness, Roughness: roughness, SpecularTint: 0.0, Anisotropy: 0.0, Ior: 1.5,
            Transmission: 0.0, TransmissionRoughness: 0.0, Sheen: 0.0, SheenTint: 0.0, Clearcoat: 0.0, ClearcoatRoughness: 0.0,
            Subsurface: 0.0, SubsurfaceRadius: SubsurfaceRadius.None, Emission: Linear(0.0, 0.0, 0.0), EmissionLuminance: 0.0);

    // Linear is the one scene-linear Unicolour constructor — graph#MATERIAL_GRAPH PortValue.SceneLinear (the one
    // Acescg working space), so every base/emission color this page mints reads the same scene-linear channel basis the
    // library rows do, never a second ColourSpace wrapper or a re-minted Configuration.
    static Unicolour Linear(double r, double g, double b) => new(PortValue.SceneLinear, ColourSpace.RgbLinear, r, g, b);
}
```

## [03]-[RESEARCH]

(none)

# [COMPUTE_TASKLOG]

The open and closed work for measured execution, distilled from `IDEAS.md`. Each open task carries a status marker and the capability-to-build, packages, integration points/boundaries, and key considerations; one idea spawns one or more tasks across one or more files. Closed cards record already-settled cleanup and the residual live-host probes whose owner shape is complete.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[NATIVE-BLAS]-[BLOCKED]: activate the native-OpenBLAS/MKL dense and sparse execution row.
- Capability: `LinearProvider.Select` activates the native dense and sparse provider row only when the RID resolves the admitted OpenBLAS or MKL native asset and the MathNet `Control.TryUseNativeOpenBLAS`/`TryUseNativeMKL` probe succeeds.
- Shape: the `Tensor/blas#DENSE_ALGEBRA` and `Tensor/factor#SPARSE_SOLVE` lanes stay shape-complete as one RID-keyed provider axis, with the managed terminal proved end-to-end on osx-arm64 and the native row entering as provider data rather than a second solve surface.
- Unlocks: win-x64 and linux-x64 hosts can prove the same dense factorization, sparse factorization, residual witness, and receipt path against native acceleration without changing call sites.
- Anchors: `Tensor/blas#DENSE_ALGEBRA`, `Tensor/factor#SPARSE_SOLVE`, `MathNet.Numerics.Providers.MKL`, `MathNet.Numerics.Providers.OpenBLAS`, `MathNet.Numerics.MKL.Win-x64`, and `MathNet.Numerics.MKL.Linux-x64`.
- Tension: blocked on a host RID that resolves the native asset; the managed fallback is the correct cold start until then.

[COMPUTE_QUANTITY_SPECTRAL_BRDF_PORT]-[QUEUED]: Rasm.Compute owns the QuantityFamily illuminance unit algebra the photometric emission admission coerces, the algorithms QR/Levenberg-Marquardt least-squares seam the acquisition BRDF fit composes, and the transitive ONNX Runtime spectral-inference path the SPECTRAL_REFLECTANCE_GROUNDING idea routes the ConductorIor RGB anchors through — the Compute seam the bsdf, photometric, and acquisition pages compose once, never re-minting a unit owner, a solver, or a spectral model.
- Capability: The compute owner supplying the QuantityFamily.Illuminance row the photometric EmissionSpectrum coerces emission luminance through at admission, the algorithms#ROUTE_SPINE thin-QR / Levenberg-Marquardt least-squares the acquisition FitBrdf minimizes the GGX/Smith residual through (MathNet.Numerics transitive, never a direct Materials reference), and the ONNX Runtime spectral-reconstruction model the SPECTRAL_REFLECTANCE_GROUNDING idea routes the RGB conductor anchors through to a per-wavelength complex-IOR curve.
- Shape: A Rasm.Compute QuantityFamily port, an algorithms least-squares seam, and an ONNX spectral-inference surface the Materials photometric/acquisition/bsdf pages compose at the seam, the producer end of the Appearance -> Rasm.Compute [PORT] seams.
- Unlocks: The photometric emission admits its illuminance through the one Compute unit algebra, the measured BRDF fits its parameter vector through the Compute least-squares seam, and the conductor table extends from three RGB bands to a full spectral curve through the Compute ONNX path the EPFL-RGL upstream-blocked leg awaits.
- Anchors: ARCHITECTURE.md [02]-[SEAMS] Appearance/bsdf and photometric <- Rasm.Compute/Symbolic [PORT] QuantityFamily illuminance (lines 46-47); acquisition.md the FitBrdf composes csharp:algorithms#ROUTE_SPINE thin QR (line 81, 'never hand-rolls a Levenberg-Marquardt loop'); bsdf.md the spectral conductor curve upstream-blocked note; the SPECTRAL_REFLECTANCE_GROUNDING ONNX leg and the BRDF_FIT_COMPUTE_SEAM task; realizes the `SPECTRAL_REFLECTANCE_GROUNDING` idea.
- Ripple: counterpart of `Rasm.Materials` `[SPECTRAL_REFLECTANCE_GROUNDING]` idea + `[BRDF_FIT_COMPUTE_SEAM]` task.

[COMPUTE_BSDD_PORT_TRANSPORT]-[QUEUED]: Compute lands the injected BsddPort transport implementation at the composition edge over its TRANSPORT_AXIS so the Bim BsddResolution live-wire leg binds a real buildingSMART bSDD dictionary fetch, the Bim side owning only the BsddPort interface and the LocalShape degrade.
- Capability: Grounds the Bim classification/property/IDS dictionary evidence path against a live transport: Compute owns the in-process request issue and response stream over its channel, projecting the bSDD BsddClassResponse the Bim BsddClass.Of consumes, never a Bim-minted transport.
- Shape: A Compute TRANSPORT_AXIS BsddPort implementation issuing GET /api/Class/v1 against the bSDD dictionary URI and streaming the BsddClassResponse back over the existing companion channel, injected into Bim at the app-platform composition root.
- Unlocks: The Bim T-BSDD-TRANSPORT card unblocks fully: Classification.Classify, the PropertyKey template fold, and the IDS Classification facet resolve live dictionary classes, degrading to LocalShape only on transport miss.
- Anchors: csharp:Rasm.Compute/Runtime/channels#TRANSPORT_AXIS; Semantics/classification#BSDD_RESOLUTION BsddPort.Fetch/BsddClassResponse; realizes the `PROPERTY_TEMPLATE_RESOLUTION` idea.
- Tension: The transport is Compute's channel concern; the bSDD wire contract (GET /api/Class/v1 BsddClassResponse) is the buildingSMART service shape the Bim BsddClass.Of already projects, so Compute owns the channel and Bim owns the response projection.
- Ripple: counterpart of `Rasm.Bim` `[PROPERTY_TEMPLATE_RESOLUTION]` idea + `[BSDD_TRANSPORT_UNBLOCK]` task.

[COMPUTE_IFCTESTER_ORACLE_RPC]-[QUEUED]: Compute and the Python geometry companion land the ifctester cross-tool IDS audit over the settled TWO_HOP companion rpc: Compute orchestrates the IDS-XML-to-ifctester invocation and the per-specification pass/fail GlobalId projection back, the Python ifc-companion runs the IfcOpenShell ids oracle, so the Bim IdsAudit shape diffs against a standards-conformant result on GlobalId-plus-facet.
- Capability: Gives the Bim in-process IdsAudit a deterministic external oracle: the Bim owner authors and parses the IDS spec, Compute orchestrates the companion rpc identically to the tessellation TWO_HOP pattern, and the Python ifctester returns the buildingSMART-audit-test-suite-conformant verdict.
- Shape: A Compute companion-rpc leg passing the IDS XML payload to the Python ifc-companion ifctester and projecting the per-spec pass/fail GlobalId set back into the Bim IdsAudit shape, plus the Python ifctester decoder producing the GlobalId-plus-facet verdict.
- Unlocks: The Bim T-IDS-IFCTESTER-COMPANION card unblocks: requirement-driven model acceptance compares the C# self-audit with the conformant ifctester oracle on GlobalId and facet, never a message diff.
- Anchors: Review/validation#IDS_FACETS IdsAudit; Exchange/tessellation#TESSELLATION_BRIDGE TWO_HOP; csharp:Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION; python:geometry/ifc-companion ifctester.
- Tension: The rpc rides the existing companion path the tessellation bridge proves, so Compute mints no new transport: it adds one ifctester invocation shape beside the IfcConvert one, the Python companion owning the IfcOpenShell ids oracle.
- Ripple: counterpart of `Rasm.Bim` `[IDS_IFCTESTER_UNBLOCK]` task (cross-tool IDS audit oracle).

[COMPUTE_UNITS_CUTPARAM_INGRESS_CONTRACT]-[QUEUED]: State the Rasm.Compute Symbolic units boundary contract canonicalizing Fabrication cut-parameter quantity text (mm/min feed, m/min surface speed, rpm spindle) to SI raw doubles at ingress, the upstream-owner alternative to the in-folder UNITSNET_CUTPARAM_ADMISSION, so the strata adjudication is stated on both sides rather than asserted from Fabrication only.
- Capability: Process/physics admits quantity-bearing cut-parameter text through the Rasm.Compute/Symbolic/units#QUANTITIES UnitsNet boundary, which canonicalizes to the SI scalar the toolpath interior operates on; the physics page (line 3/18/20) forbids a UnitsNet member spelled in-folder and routes all quantity ingress through this Compute boundary. The ripple states the Compute units owner's side: the QUANTITIES boundary exposes a canonicalize-to-SI surface the AEC-domain Fabrication consumer reads, owning the UnitsNet enums so the consumer never re-mints one — the architecture-density counter to UNITSNET_CUTPARAM_ADMISSION that keeps the units owner upstream.
- Shape: A Rasm.Compute Symbolic/units.md page note naming the cut-parameter quantity-ingress contract (feed/speed/depth/spindle text -> SI raw double) as a consumer-facing QUANTITIES export the Fabrication Process/physics boundary reads, reconciling the Fabrication-side routing claim against the Compute owner's actual canonicalization surface.
- Unlocks: A bidirectionally-owned quantity-ingress contract so the Fabrication physics page reads a real Compute surface, the adjudication between cross-strata-read and in-folder-admission stated explicitly rather than duplicating the units boundary.
- Anchors: csharp:Rasm.Compute/Symbolic/units#QUANTITIES the UnitsNet canonicalization boundary; Fabrication/Process/physics#CUT_PARAMETER RemovalParameter.Admit (lines 138-143); the strata law forbidding the AEC->app-platform downward edge; UNITSNET_CUTPARAM_ADMISSION the competing in-folder resolution
- Tension: The strata law forbids Fabrication (AEC-domain) a downward edge to Rasm.Compute (app-platform), yet physics.md routes quantity ingress THROUGH the Compute units boundary — the contract must clarify whether the canonicalization is a legal peer-export the AEC consumer reads or whether the in-folder UnitsNet admission is the strata-correct resolution; the critique wave resolves toward UNITSNET_CUTPARAM_ADMISSION, this ripple holds the upstream alternative.
- Ripple: counterpart of `Rasm.Fabrication` `[UNITSNET_CUTPARAM_ADMISSION]` task (`Symbolic/units` canonicalizes cut-parameter quantities).

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)

# [PY_ARTIFACTS_IDEAS]

Artifact ideas extend the standalone host-free publication platform; folder tasks carry promoted work.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[XML_TRUST_RAILS]-[QUEUED]: XML handling converges on trust-origin rails.
- Capability: one hardened parse fold owns every untrusted XML admission while self-generated fragments keep their in-page builders, so the folder's four XML stacks read as two ruled rails.
- Shape: the hardened `lxml` parse fold homes once in the `document/emit.md` `_hardened_parse` form; `document/model.md` `_mathml`, the `document/lens.md` external-file read, and the `document/tagged.md` XMP admission compose it, `defusedxml` retires; `chart/export.md` `_split_layers` and `diagram/draw.md` fragment building stay in-page as trusted-fragment work.
- Unlocks: one hardening decision audited once; the dual-stack ambiguity dies.
- Anchors: the XML trust-origin ruling at `libs/python/artifacts/RULINGS.md`; the four current stacks — `lxml` twice hand-hardened, `defusedxml`, stdlib `ElementTree`.
- Tension: `defusedxml` retirement executes the catalog-alignment touch-point set at its owners in the landing pass.

[QUALITY_GATE]-[QUEUED]: Graded artifact quality gate the transmittal refuses on.
- Capability: one `QualityGate` folding raster measured scores, PDF/UA and PDF/X preflight verdicts, PAdES conformance verdicts, and lens extraction audits into one graded per-artifact verdict, per-kind thresholds carried as policy rows an office tunes without code.
- Shape: one new page `libs/python/artifacts/.planning/delivery/gate.md` owning the verdict fold and threshold policy; `libs/python/artifacts/.planning/delivery/transmittal.md` gains the refusal seam; `libs/python/artifacts/.planning/document/lens.md` gains the `Page.debug_tablefinder` extraction-audit overlay feeding the gate.
- Unlocks: an issue that cannot ship a failing sheet; one graded quality surface over the folder's scattered verdict producers.
- Anchors: `graphic/raster/measure.md` perceptual scores; `document/tagged.md` `UaCheck` and preflight; `exchange/conformance.md` `ConformanceVerdict`; `pdfplumber` `Page.debug_tablefinder`; the data `QualityProfile` seam.

[MANAGED_CONFIG_SPACE]-[QUEUED]: color management resolves through a versioned config graph, never a synthesized profile.
- Capability: managed egress answers working space, display, view, and look by NAME out of a shareable config, so a scene-linear reference is a role a project declares rather than a matrix or profile any producer hardcodes, and deep planes, display rasters, and separations all resolve their ends through one graph.
- Shape: `libs/python/artifacts/.planning/graphic/color/managed.md` widens its transform axis to carry the config-resolved leg beside the ICC leg, and the two boundary sentences at `graphic/texture/plane.md` and `graphic/texture/ibl.md` that already name this owner stop pointing at a capability the folder admits and composes nowhere.
- Unlocks: deep-pixel paths whose scene-linear end is declared instead of assumed, with LUT egress and shader-side transforms reachable from the same resolution the CPU path takes.
- Anchors: `opencolorio` with its `.api/` catalog — shipped ACES CG and Studio configs reachable with no file on disk, the role graph `scene_linear` leads, `Processor` compiling to a CPU applier over float buffers or a shader emitter, `Baker` as the LUT egress; the `IccTransform` bundle and `ManagedCodec` egress the new leg seats beside; the package-scoped build variable making the distribution resolve at the floor.
- Tension: two color authorities then stand on one page — the ICC profile edge and the config graph — and the discriminant has to be which one OWNS a given end rather than which one can express it, or every arm grows a second knob answering the same question.

[DEEP_MEASUREMENT]-[QUEUED]: the deep-pixel estate grades its own products.
- Capability: float planes carry measured fidelity and periodicity evidence the way a display raster carries perceptual scores, so encode loss, resample error, derivation error, and tile-seam energy are numbers a caller reads off the receipt rather than properties a producer asserts.
- Shape: measurement homes with the deep substrate under `libs/python/artifacts/.planning/graphic/texture/`, its scalars riding the `Texture` receipt band `libs/python/artifacts/.planning/core/receipt.md` already declares as the map preimage.
- Unlocks: lossy codec rows provable against their lossless siblings, `tiled` claims backed by a measurement, derivation chains whose error is visible before a set ships.
- Anchors: `graphic/raster/measure.md` as the measured-score precedent, 8-bit and gated on a distribution the floor refuses; numpy spectral kernels `graphic/texture/derive.md` already owns; the C# tile gate as the cross-branch counterpart shape.
- Tension: `graphic/raster/measure.md` is the obvious host and the wrong one — it terminates in the 8-bit funnel this folder's own substrate ruling splits away from, so measurement either duplicates a page's charter or splits the score vocabulary across two substrates.

[FLOOR_UNGATING]-[BLOCKED]: every admitted distribution builds at the interpreter floor.
- Capability: the folder's package registry carries no interpreter marker a probe has not reproduced, so a source-built distribution admits on the estate build lane instead of waiting for an upstream wheel, and a marker that survives names the break it reproduces and the release that retires it.
- Shape: the four marked rows in the root `pyproject.toml` — `scikit-image` beside the `[IMAGING]` card and `vtk`/`pyvista`/`usd-core` beside the `[SCENE]` card at `libs/python/artifacts/README.md` — each drop their marker under their own route.
- Unlocks: the acceptance bar is zero unexplained interpreter markers on artifacts rows; the measured-score half and the USD stage half stop carrying a build-lane caveat their own design pages never state.
- Anchors: the ungated `openexr` and `opencolorio` admissions this folder already carries as the source-lane precedent, the package-scoped build-variable mechanism that repaired the second, the `colour-cxf` static-metadata block as the pure-wheel cap precedent, and the Forge python-overlay `.pth` already resolving the scene natives at the floor.
- Arms: pythran shipping a release that pins `gast>=0.7`, and an upstream cp315 wheel per scene distribution.
- Tension: the imaging route waits on an upstream toolchain fix this estate cannot force while the scene route waits on a wheel, so the marker set retires in two unrelated moves rather than one sweep.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[DASHBOARD_ARTIFACT]-[COMPLETE]: landed as `visualization/dashboard.md` — `DashboardPlan` folds a `DashPane` deck (chart/table/figure) into one offline document over ONE `vl_convert.javascript_bundle(vl_version=…)` runtime with per-pane `vegaEmbed` mounts, markup assembled through `string.templatelib.Template` under the destination-keyed `_ESCAPE` fold, minting the new banded `ArtifactReceipt.Dashboard` kind; the carded claim that `javascript_bundle` takes `snippet` positionally with no default is REFUTED — both parameters default `None` and the default snippet publishes `vegaEmbed`/`vega`/`vegaLite` onto `window`, which is what makes one shared bundle serve every pane.
[PRODUCTION_HOOKS]-[COMPLETE]: landed as `core/hooks.md` — `ArtifactHook`/`ARTIFACT_POINTS` over the runtime `Hooks` registry with the scope-keyed `Production` register/fire/subscribe surface, fire seams live in `core/issue.md` (`_issued`/`_planned`/`_driven`) and `core/receipt.md` `contribute`.
[ISSUE_ATTRIBUTION]-[COMPLETE]: landed as the `ArtifactIssue._scoped` bracket — issue scope with parent-respecting tenant baggage under `ISSUE_BAGGAGE`/`TENANT_BAGGAGE` with `bound_contextvars` log keys; tenant metric promotion stays runtime-owned per `core/receipt.md` `[METRIC_SIGNALS]`, the issue scope a log/baggage dimension by cardinality law.
[TRANSMITTAL_NOTICE]-[COMPLETE]: landed as `delivery/notice.md` `TransmittalNotice` — validated CloudEvents envelope with W3C trace injection and structured/binary rows — folded into `delivery/transmittal.md` `_emit` as the soft terminal notice firing `NOTICE_ISSUED`.
[PRODUCER_BENCH]-[COMPLETE]: landed as `core/bench.md` — `CORPUS` subject rows with typed `BenchFeed` deterministic-input edges, seeded `RECIPES`, threshold policy, and the `benched` grade fold over runtime `Bench.run`/`BenchmarkReceipt`.
[DIAGRAM_DXF]-[COMPLETE]: realized as `export/dxf`'s `Diagram` arm — `DiagramLower` lowers the positioned glyph sequence to `DxfEntity` cases under `Standard.seed` regime-pen layers, glyphset owning the shared lowering derivations (`mark`, `Port.seat`, `AreaMark.centroid`, `ER_CAPS`, `ENTITY_BAND`); draw stays two-arm.
[WTPDF_AUDIT]-[COMPLETE]: landed as `UaCheck.WTPDF_ACCESSIBILITY`/`WTPDF_REUSE` clause rows on `document/tagged#ACCESS` — `pdfd:conformsTo` spellings confirmed at the PDF Association source (erratum-canonical `wtpdf#accessibility1.0`/`wtpdf#reuse1.0` with the as-published `/#` forms), read off the raw `/Metadata` packet because the pikepdf mapping view cannot decode the declarations bag.

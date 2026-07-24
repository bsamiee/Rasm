# [PY_ARTIFACTS_IDEAS]

Forward pool of the folder's higher-order concepts, each grounded in artifact production and the host-free companion charter. Open ideas are cards in `[1]-[OPEN]`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition. Each idea drives one or more `TASKLOG.md` tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
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

[DASHBOARD_ARTIFACT]-[QUEUED]: Self-contained interactive HTML dashboard as a first-class artifact kind.
- Capability: multi-pane chart, table, and diagram composition into one offline single-file HTML document — filterable charts, sortable schedule tables, register indexes — produced host-free and theme-graded, never a live UI.
- Shape: one new page `libs/python/artifacts/.planning/visualization/dashboard.md` owning `DashboardPlan`; `libs/python/artifacts/.planning/visualization/chart/export.md` gains the interactive-HTML format row.
- Unlocks: receipts, QTO, and delivery registers readable as one shareable file; the folder's dashboards-axis answer inside the no-UI charter.
- Anchors: `altair` `JupyterChart` and HTML export; `vl-convert-python`; `TablePlan` HTML arm; `DiagramDraw` layered SVG; `Theme` rows.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[PRODUCTION_HOOKS]-[COMPLETE]: landed as `core/hooks.md` — `ArtifactHook`/`ARTIFACT_POINTS` over the runtime `Hooks` registry with the scope-keyed `Production` register/fire/subscribe surface, fire seams live in `core/issue.md` (`_issued`/`_planned`/`_driven`) and `core/receipt.md` `contribute`.
[ISSUE_ATTRIBUTION]-[COMPLETE]: landed as the `ArtifactIssue._scoped` bracket — issue scope with parent-respecting tenant baggage under `ISSUE_BAGGAGE`/`TENANT_BAGGAGE` with `bound_contextvars` log keys; tenant metric promotion stays runtime-owned per `core/receipt.md` `[METRIC_SIGNALS]`, the issue scope a log/baggage dimension by cardinality law.
[TRANSMITTAL_NOTICE]-[COMPLETE]: landed as `delivery/notice.md` `TransmittalNotice` — validated CloudEvents envelope with W3C trace injection and structured/binary rows — folded into `delivery/transmittal.md` `_emit` as the soft terminal notice firing `NOTICE_ISSUED`.
[PRODUCER_BENCH]-[COMPLETE]: landed as `core/bench.md` — `CORPUS` subject rows with typed `BenchFeed` deterministic-input edges, seeded `RECIPES`, threshold policy, and the `benched` grade fold over runtime `Bench.run`/`BenchmarkReceipt`.
[DIAGRAM_DXF]-[COMPLETE]: realized as `export/dxf`'s `Diagram` arm — `DiagramLower` lowers the positioned glyph sequence to `DxfEntity` cases under `Standard.seed` regime-pen layers, glyphset owning the shared lowering derivations (`mark`, `Port.seat`, `AreaMark.centroid`, `ER_CAPS`, `ENTITY_BAND`); draw stays two-arm.
[WTPDF_AUDIT]-[COMPLETE]: landed as `UaCheck.WTPDF_ACCESSIBILITY`/`WTPDF_REUSE` clause rows on `document/tagged#ACCESS` — `pdfd:conformsTo` spellings confirmed at the PDF Association source (erratum-canonical `wtpdf#accessibility1.0`/`wtpdf#reuse1.0` with the as-published `/#` forms), read off the raw `/Metadata` packet because the pikepdf mapping view cannot decode the declarations bag.

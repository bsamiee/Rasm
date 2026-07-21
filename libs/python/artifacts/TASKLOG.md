# [PY_ARTIFACTS_TASKLOG]

`artifacts` open and closed work, distilled from `IDEAS.md`. Open tasks are cards in `[1]-[OPEN]` with a `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]` leader; closed tasks move to `[2]-[CLOSED]` with `[COMPLETE]`/`[DROPPED]`. Each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <present only on a BLOCKED or gated card; the exact observable that flips it actionable — a catalog row landing, a member query returning evidence, a package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart card — cross-folder as `pkg` `[SLUG]` or a same-folder prerequisite `[SLUG]`, prefixed follows/precedes/mirrors when build order is load-bearing>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[MEDIA_CANON_SEAM]-[QUEUED]: the media canonical-encoder seam resolves to one spelling.
- Capability: every media sibling imports the canonical encoder trio by names the source module actually exports, and the test-pattern painter stops shadowing the encoder helper's name.
- Shape: `libs/python/artifacts/.planning/media/container.md` re-exports `CANON`/`framed`/`_lapsed` under one canonical spelling; the sibling import blocks on `media/timeline.md`, `media/subtitle.md`, `media/analysis.md`, and `media/synthesis.md` align to it, and `media/synthesis.md` renames its local `_framed` test-pattern painter.
- Unlocks: the four sibling pages stop importing names container never defines — the seam compiles as written.
- Anchors: container's import of `CANON`/`framed` from the scene spec; the four sibling underscore-form import blocks.
- Atomic: one export decision and five import-block touches.

[MEDIA_LANE_FIELD_ORDER]-[QUEUED]: media producer structs construct as declared.
- Capability: every media producer Struct orders required fields before defaulted ones, so class creation never raises on the msgspec field-order law.
- Shape: `Timeline` on `libs/python/artifacts/.planning/media/timeline.md` and `Subtitle` on `libs/python/artifacts/.planning/media/subtitle.md` hoist `lane: LanePolicy` above their defaulted fields, matching the `Media` ordering on `media/container.md`.
- Unlocks: both producer pages transcribe to importable modules.
- Anchors: msgspec Struct field-order semantics; the `Media` sibling ordering.
- Atomic: two field hoists.

[MEDIA_AUDIO_CHANNEL_AXIS]-[QUEUED]: multichannel audio times correctly on the interleaved axis.
- Capability: every last-axis audio window, slice, and pick index scales by the channel count, so multichannel media neither mistimes by the channel factor nor rotates channels.
- Shape: `_pcm`, `_filter_trim`, `_audio_xfade`, `_sped`, and `_reverse` on `libs/python/artifacts/.planning/media/timeline.md` scale by `av.AudioLayout(layout).nb_channels` and frame-align pick and reversal indices — or reshape to frames-by-channels before windowing.
- Unlocks: stereo and surround timelines cut, fade, and retime at the same fidelity as mono.
- Anchors: the interleaved packed last axis the `Voice` carrier joins on; `av.AudioLayout`.

[TERMINATOR_ARROW_TABLE]-[QUEUED]: one Terminator lowering table serves every drawing consumer.
- Capability: the arrow-terminator lowering — block name and tick size per `Terminator` member — mints once beside the vocabulary owner, collapsing three divergent tables to one `OBLIQUE_STROKE` spelling.
- Shape: one canonical table on `libs/python/artifacts/.planning/drawing/regime.md` beside the `Terminator` enum; `drawing/standard.md` `_TERMINATOR`, `drawing/annotate.md` `_DXF_ARROW`, and `drawing/symbol.md` `_ARROW` compose it, the DIMTSZ-versus-block choice staying with each consumer.
- Unlocks: a terminator edit lands once and every DXF lowering agrees.
- Anchors: the `Terminator` vocabulary on regime; the import edges annotate already holds to regime and symbol.
- Atomic: one table home and three consumer folds.

[PRODUCER_ADMISSION_IDIOM]-[QUEUED]: every producer head admits through the one configured ingress.
- Capability: one admission idiom across the drawing producers, so every refusal rides the `_FAULTS` fold as `ValueError` and no head raises outside its boundary tuple.
- Shape: `Dimension.over`/`Symbol.over` plain `@beartype` and `Annotate.over`/`Detail.over` structural `match`-plus-raise all move to the `@beartype(conf=_INGRESS)` form `Schedule.of` already carries, `_INGRESS` homed once for the folder.
- Unlocks: the folder admission-idiom ruling realized on every producer head.
- Anchors: the admission-idiom ruling at `libs/python/artifacts/RULINGS.md`; `_INGRESS` on `drawing/schedule.md`.

[MEDIA_WORKER_ASPECT]-[QUEUED]: one worker aspect serves the media plane.
- Capability: the offload-worker beartype aspect exists once, generic over its product type, and every media producer composes it.
- Shape: `_worker` on `libs/python/artifacts/.planning/media/container.md` generalizes over the result payload; `media/subtitle.md` and `media/analysis.md` drop their local copies and import it as `media/timeline.md` already does.
- Unlocks: one aspect edit reaches every media offload seam.
- Anchors: the three near-identical `_worker` definitions; timeline's existing composing import.
- Atomic: one generic signature and two import substitutions.

[FILTER_DRAIN_KERNEL]-[QUEUED]: one libavfilter drain kernel with one exception vocabulary.
- Capability: the filtergraph drain loop exists once with the verified pull-exception pair, collapsing three copies and two exception vocabularies.
- Shape: one shared drain kernel on `libs/python/artifacts/.planning/media/filtergraph.md` replacing `_pulled` and `_staged_pull`; `media/analysis.md` `_pull_frames` composes it.
- Unlocks: a drain fix or exception-vocabulary correction lands once.
- Anchors: the divergent except clauses — builtin `BlockingIOError`/`EOFError` versus `av.error.*`.
- Route: verify against the installed PyAV distribution which exception types `FilterContext.pull` raises, then pin that pair in the kernel.

[XFADE_BLEND_OWNER]-[QUEUED]: overlap blending homes once and the xfade payload is read.
- Capability: one blend owner consumes the transition payload it is handed, so behaviorally identical dissolves stop minting distinct content keys off dead payload slots.
- Shape: overlap blending homes on `libs/python/artifacts/.planning/media/filtergraph.md` — the dissolve arm reads the `(offset, duration, transition)` payload it receives or the payload shrinks to what the arm consumes — and `media/timeline.md` `_blended` composes that owner instead of carrying a second numpy blend kernel.
- Unlocks: content-key identity reflects behavior; one blend kernel serves wipes and dissolves.
- Anchors: the `FilterNode.xfade` payload slots; the dissolve arm reading only its window; timeline's `_MASK` wipe kernel.

[WIRED_FAULT_ARMS]-[QUEUED]: caller-reachable graph refusals surface typed.
- Capability: a `Timeline.Effect` program that fails graph admission surfaces as a typed input refusal, never an opaque worker fault.
- Shape: one `ValueError`-to-`invalid` arm in `_transcode` on `libs/python/artifacts/.planning/media/container.md`, folding the `_arity`/`_build_graph` raises that caller data can reach.
- Unlocks: media callers distinguish a bad effect program from an engine failure.
- Anchors: the `_arity`/`_build_graph` raises on `media/filtergraph.md`; `_transcode`'s existing `ImportError`/`FFmpegError` arms.
- Atomic: one except arm on one fold.

[LAYER_RENAME_PROJECTION]-[QUEUED]: layer renaming homes on the Layer owner.
- Capability: the rename-by-index projection over a names tuple exists once on the layer vocabulary owner.
- Shape: one `renamed(layers, names)` projection on `libs/python/artifacts/.planning/export/layered.md` beside `Layer`; the `_placed_layers` copies on `composition/compose.md`, `composition/sheet.md`, and `composition/imposition.md` compose it.
- Unlocks: one projection edit reaches all three composition pages.
- Anchors: the three `_placed_layers` signatures; the `Layer` Struct on layered.
- Tension: compose's synthetic `layer-{index}` fallback and sheet/imposition's existing-name fallback reconcile to one policy at the owner.
- Atomic: one projection and three call-site folds.

[FORMULA_SINGLE_TYPESET]-[QUEUED]: one typeset serves both math-note consumers.
- Capability: a math textnote lays out once and the laid fragment threads to both its measure and render consumers.
- Shape: `_text_span` and `_note_group` on `libs/python/artifacts/.planning/drawing/annotate.md` share one `Formula(...).laid()` result — metrics and svg threaded, the second heavy typeset deleted.
- Unlocks: annotate authoring drops half its typeset cost per note.
- Anchors: the two `.laid()` call sites; the laid fragment's metrics-plus-svg shape.
- Atomic: one hoist and two consumer threads.

[ANNOTATION_LEAF_CONSTRUCTOR]-[QUEUED]: the annotation layer-leaf projection homes on the layer owner.
- Capability: one constructor builds the annotation `LayerNode.Leaf` from a name and fragment bytes, so intent, aec, and z defaults live once.
- Shape: one annotation-leaf constructor on the `LayerNode` owner of `libs/python/artifacts/.planning/graphic/layer.md`; the four `_row` copies on `drawing/symbol.md`, `drawing/dimension.md`, `drawing/annotate.md`, and `drawing/detail.md` compose it.
- Unlocks: a leaf-shape edit reaches every drawing producer; the per-producer default drift dies.
- Anchors: the four `_row` signatures; the `LayerNode`/`LayerMeta`/`LayerContent` owners on layered graphics.
- Atomic: one constructor and four call-site folds.

[DXF_EXTENT_HELPER]-[QUEUED]: one DXF extent probe serves the drawing producers.
- Capability: the modelspace bounding-extent probe with its fallback exists once, an `Option` fallback parameter covering both call shapes.
- Shape: one extent helper beside the drawing standard owner; the byte-identical `_dxf_extent` copies on `drawing/annotate.md` and `drawing/detail.md` and the float sibling `_extent` on `drawing/dimension.md` compose it.
- Unlocks: an extent-probe fix lands once across the DXF lowerings.
- Anchors: `ezdxf` `bbox.extents(msp, fast=True)`; the three current copies.
- Atomic: one helper and three call-site folds.

[IDML_FAULT_MONOID]-[QUEUED]: IDML admission reports every casualty.
- Capability: the steps-program admission accumulates all independent structural casualties into one refusal, realizing the folder fault-monoid law on the IDML gate.
- Shape: the seven per-axis first-offender probes on `libs/python/artifacts/.planning/export/indesign.md` fold into an accumulating `IdmlFault` (or a monoid over it), replacing the first-non-None `match`; the fail-fast prose falls with it.
- Unlocks: a bad steps program surfaces every defect in one round instead of one per attempt.
- Anchors: the fault-monoid ruling at `libs/python/artifacts/RULINGS.md`; `core/issue.md`'s `BoundaryFault.combine` reduce as the pattern.
- Atomic: one admission fold and one union widening.

[REFLOW_STORY_CHILDREN]-[QUEUED]: reflow deposits the children its inverse recovers.
- Capability: the HTML-into-PDF reflow arm authors structured children, so the lens story arm's recover-to inverse holds on real content rather than a childless placeholder.
- Shape: `_reflow_arm` on `libs/python/artifacts/.planning/document/report.md` deposits recoverable child nodes on its `PageNode`; the inverse seam prose on `document/lens.md` holds unchanged.
- Unlocks: an authored reflow round-trips through the lens with content, not just a page shell.
- Anchors: the childless `PageNode` construction in `_reflow_arm`; `_story_arm`'s structured recovery.

[FAULT_CONF_IMPORT]-[QUEUED]: the canonical fence conf is imported, never re-minted.
- Capability: one conf value with one provenance fences every classify-rail head, so a canonical conf change reaches every page.
- Shape: the four local `_GUARD` mints of the byte-identical conf on `libs/python/artifacts/.planning/composition/sheet.md`, `composition/compose.md`, `composition/imposition.md`, and `delivery/register.md` become imported `FAULT_CONF` applications; the `_CONTRACT` numeric-tower conf on the marks and vector pages gains `violation_type=BeartypeCallHintViolation` beside `is_pep484_tower=True`.
- Unlocks: the per-rail admission ruling realized outside drawing.
- Anchors: `FAULT_CONF` on the runtime faults owner; the admission-idiom ruling at `libs/python/artifacts/RULINGS.md`.
- Atomic: four import swaps and one conf field.

[GATE_PAGE]-[QUEUED]: Author `libs/python/artifacts/.planning/delivery/gate.md` — the `QualityGate` verdict fold and threshold policy.
- Capability: graded verdict union folding measured scores, preflight, conformance, and extraction audits, per-kind thresholds as policy rows.
- Shape: one page owning the fold, the grade vocabulary, and the transmittal refusal seam contract.
- Unlocks: IDEAS.md [QUALITY_GATE] landing surface; deepens the `delivery` stub with real capability.
- Anchors: `graphic/raster/measure.md`; `document/tagged.md` `UaCheck`; `exchange/conformance.md` `ConformanceVerdict`.

[GATE_LENS_AUDIT]-[QUEUED]: Land the `Page.debug_tablefinder` extraction-audit overlay projection in `libs/python/artifacts/.planning/document/lens.md`.
- Capability: table-extraction QA overlays as a lens examination projection whose audit scalars feed the gate fold.
- Shape: one projection row on the lens examination ops.
- Unlocks: IDEAS.md [QUALITY_GATE] extraction evidence; exploits the unexploited `pdfplumber` member.
- Anchors: `pdfplumber` `Page.debug_tablefinder`; `document/lens.md` examination ops.
- Atomic: one projection row on one existing page.

[DASH_PAGE]-[QUEUED]: Author `libs/python/artifacts/.planning/visualization/dashboard.md` — the `DashboardPlan` composed-pane owner.
- Capability: chart, table, and diagram panes composed into one offline single-file HTML artifact, theme-graded and register-indexable.
- Shape: one page owning the pane vocabulary, the composition fold, and the single-file emit contract.
- Unlocks: IDEAS.md [DASHBOARD_ARTIFACT] landing surface; deepens the `visualization` direct tier.
- Anchors: `altair` `JupyterChart`; `TablePlan` HTML arm; `DiagramDraw`; `Theme` rows.

[DASH_HTML_ROW]-[QUEUED]: Land the interactive-HTML format row in `libs/python/artifacts/.planning/visualization/chart/export.md`.
- Capability: self-contained HTML chart output beside the existing raster and vector formats, host-free.
- Shape: one format-dispatch row naming the HTML arm and its embed policy.
- Unlocks: IDEAS.md [DASHBOARD_ARTIFACT] pane source; exploits the unexploited `altair` member.
- Anchors: `altair` `JupyterChart` and HTML export; `vl-convert-python`.
- Atomic: one format row on one existing page.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[HOOKS_PAGE]-[COMPLETE]: landed as `core/hooks.md` — `ArtifactHook` id vocabulary, closed payload Structs, `ARTIFACT_POINTS` row table, and the scope-keyed `Production` surface over the runtime registry.
[HOOKS_SEAMS]-[COMPLETE]: fire seams live at `core/issue.md` `_issued`/`_planned`/`_driven` (admitted veto, planned, refused, front drained) and `core/receipt.md` `contribute` (`ReceiptEmitted`/`TransmittalIssued`), payloads projected from evidence in hand.
[ATTRIB_BIND]-[COMPLETE]: landed as `core/issue.md` `_scoped` — issue scope minted per call, tenant seeded only when the parent context carries none, token-attached around the drive so the worker crossing's `propagate.inject` carries both entries.
[ATTRIB_DIMS]-[COMPLETE]: landed as ruled at `core/receipt.md` `[METRIC_SIGNALS]`/`[SPAN_ERROR]` — tenant folds through the runtime metrics `_attributed` and log promotion, the per-call issue scope stays a `bound_contextvars` log/baggage dimension and never a metric attribute by cardinality law.
[NOTICE_PAGE]-[COMPLETE]: landed as `delivery/notice.md` `TransmittalNotice` — checked `cloudevents.v1` envelope, spec-grammar extension vocabulary, `propagate.inject` trace-and-baggage continuity, structured/binary `_LOWERED` rows.
[NOTICE_FOLD]-[COMPLETE]: landed as `delivery/transmittal.md` `_emit` — the soft terminal `TransmittalNotice.issued` seal over the settled receipt, issued register, and `_extended` evidence scalars, fired on `NOTICE_ISSUED`.
[BENCH_PAGE]-[COMPLETE]: landed as `core/bench.md` — the roster owner is `CORPUS: Final[Block[BenchSubject]]` (the carded `BenchCorpus` name superseded) with `BenchFeed` input edges, `RECIPES`, thresholds, and the `benched` fold.

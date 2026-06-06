# Project Docs And Tool Docs Findings

Source: `../final-report.md`
Scope: Non-standards `docs/**`, tool/operator docs, architecture/roadmap prose, and code/test comment surfaces.
Finding count: 14

The finding blocks below are copied verbatim from `../final-report.md`.

## Findings

#### F-DOC-01: Ordinary docs use instruction-only invocation markers
- Severity: high
- Reports: `agent-reports/07-docs-non-standards.md` 2.1
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/README.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/system-api-map/bcl.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/xunit/api.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/system-api-map/packages.md:59`
- Evidence: all 38 target non-standards docs matched `[IMPORTANT]` or `[CRITICAL]`.
- Why it may poison context: reference docs can be mistaken for higher-ranked instructions, and future artifacts may reproduce agent-weighting syntax in ordinary reader-facing prose.
- Suggested disposition: replace invocation markers with ordinary lead prose or GitHub alerts only when the container genuinely interrupts the reader; reserve `[IMPORTANT]`/`[CRITICAL]` for instruction surfaces.

#### F-DOC-02: Generic-looking library/test API docs carry Rasm, host, analyzer, testkit, and local-path context
- Severity: high
- Reports: `agent-reports/07-docs-non-standards.md` 2.2, 2.6
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/languageext/api.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/languageext/api.md:35`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/mathnet/api.md:18`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/thinktecture/api.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/archunit/api.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/cscheck/api.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/verify/api.md:42`
- Evidence: generic `api.md` leaves mention workspace imports, local XML proof, project usage owners, package-state docs, architecture test project, local analyzer, project testkit, and `tests/csharp/_tooling/ModuleInitializers.cs`; examples use `DomainRoot`, `BoundaryAdapterAttribute`, `AdmissionRegistry`, `Vectors.Dimension.TryCreate`, "Yuksel WSE", and "Cholesky vs LU".
- Why it may poison context: portable API references become project instruction carriers and can import Rasm/Rhino/testkit assumptions into unrelated work.
- Suggested disposition: keep portable package API facts in `api.md`; move local graph/XML/testkit/analyzer/path facts into explicit project posture files such as `rasm.md`, `docs/usage.md`, `docs/system-api-map/**`, or test overlays.

#### F-DOC-03: Non-standards docs use ad hoc source/provenance labels without proof-standard fields
- Severity: high
- Reports: `agent-reports/07-docs-non-standards.md` 2.3
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/xunit/api.md:15`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/cscheck/api.md:64`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/cscheck/api.md:120`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/stryker/api.md:52`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/coverlet/api.md:14`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/mathnet/api.md:26`
- Evidence: docs use `[SOURCE]`, "local proof showed", and "Verified control surfaces include..." without claim-local `Evidence:`, `Last verified:`, `Review trigger:`, or `Proof gap:`.
- Why it may poison context: provenance-looking text appears authoritative but is not refreshable and teaches source-label chatter instead of claim-level proof.
- Suggested disposition: convert only drift-prone claims to proof-standard fields; remove decorative source labels and route stable facts to owners.

#### F-DOC-04: Project graph, proof order, and local command routing are duplicated across non-standards docs
- Severity: medium
- Reports: `agent-reports/07-docs-non-standards.md` 2.4
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/usage.md:58`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/README.md:24`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/system-api-map/README.md:28`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/system-api-map/packages.md:76`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/README.md:18`
- Evidence: package graph, host policy, cross-stack usage, proof routing, local XML/decompile proof, boundary adapters, testing package injection, mutation tooling, and direct test rails repeat across multiple docs.
- Why it may poison context: generated docs can copy route boilerplate and agent-only context, and owners drift when proof order changes.
- Suggested disposition: keep proof/order routing in `docs/usage.md`, `docs/system-api-map/**`, and owner-local instructions; reduce generic indexes to short owner links.

#### F-DOC-05: Non-standards docs retain defensive version/migration wording without consistent freshness triggers
- Severity: medium
- Reports: `agent-reports/07-docs-non-standards.md` 2.5
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/csharp/language.md:6`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/csharp/language.md:101`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/languageext/effects.md:46`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/thinktecture/sourcegen.md:31`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/system-api-map/bcl.md:155`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/host-libraries.md:41`
- Evidence: docs warn against `LangVersion=preview`/`latest`, list out-of-scope statuses, carry v4/v5 and v10 migration deltas, reject legacy spans, and instruct refreshing latest package versions before first consumer.
- Why it may poison context: stale baseline caveats can become active doctrine and encourage defensive "old-vs-new" prose in durable docs.
- Suggested disposition: keep version facts only in explicit support/API delta references with freshness fields; rewrite target guidance as direct rules where current availability is not the point.

#### F-DOC-06: Bracketed group-label overuse reinforces template-style prose in ordinary docs
- Severity: low
- Reports: `agent-reports/07-docs-non-standards.md` 2.7
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/README.md:16`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/system-api-map/bcl.md:28`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/system-api-map/replacements.md:25`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/cscheck/api.md:48`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/verify/api.md:29`
- Evidence: repeated `[FILES]`, `[USE]`, `[OWNER]`, `[DETAIL]`, `[WHY_VERIFY_FITS]`, and `[EXTRAS]` labels behave like heading surrogates or template residue.
- Why it may poison context: future generated docs may emit agent-facing label packets instead of normal prose or real headings.
- Suggested disposition: keep group labels only when they introduce a real set; convert repeated local labels to H3 sections or prose.

#### F-TOOL-01: Assay README carries transitional adoption state as durable operator truth
- Severity: high
- Reports: `agent-reports/12-code-comments-tools.md` L-01
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/README.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/README.md:7`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/README.md:18`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/README.md:218`
- Evidence: README repeats Assay as intended successor to `tools.quality`, not root-canonical yet, use-now/update-when guidance, command replacement intent, and old payload-location migration notes.
- Why it may poison context: agents reason from an in-between migration state and preserve compatibility language instead of following the current root owner route.
- Suggested disposition: collapse to one short status fact or remove after root routing settles; keep command behavior in command tables and migration policy in the root quality owner.

#### F-TOOL-02: Assay README embeds proof/source process schema in operator manual
- Severity: medium
- Reports: `agent-reports/12-code-comments-tools.md` L-02
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/README.md:300`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/README.md:302`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/README.md:337`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/README.md:350`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/README.md:374`
- Evidence: operator manual uses `Current truth`, `Caveat`, `Reader action`, "how agents consume proof", "task System Information", source-owner tables, maintainer-flow instructions, README validation commands, and blocker recording.
- Why it may poison context: tool manual becomes a second docs standard and reinforces task-process vocabulary.
- Suggested disposition: keep operator-facing contract only; move proof/source-owner grammar to overlays or docs standards; remove task-process references.

#### F-TOOL-03: `tools/quality` README mixes operator reference with agent instruction and validation boilerplate
- Severity: medium
- Reports: `agent-reports/12-code-comments-tools.md` L-03
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/quality/README.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/quality/README.md:13`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/quality/README.md:262`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/quality/README.md:286`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/quality/README.md:295`
- Evidence: README calls tool an "agent-only CLI", tells readers how to report evidence, includes "no World-A/World-B duality", has `AGENT_ROUTING`, tells readers to load `.claude/skills/coding-python/SKILL.md`, and includes README/Mermaid/dependency-currency validation commands.
- Why it may poison context: README acts as standing instruction authority instead of just tool reference.
- Suggested disposition: reduce `AGENT_ROUTING` to operator rail selection if needed; move skill-loading and validation ladders to overlays or root quality route.

#### F-TOOL-04: Rhino bridge README preserves agent guidance, legacy wire migration, and local WIP evidence
- Severity: high
- Reports: `agent-reports/12-code-comments-tools.md` L-04
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/rhino-bridge/README.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/rhino-bridge/README.md:18`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/rhino-bridge/README.md:32`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/rhino-bridge/README.md:203`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/rhino-bridge/README.md:235`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/rhino-bridge/README.md:241`
- Evidence: README instructs coding agents, names `Coding Agent` in diagram, describes "structured agent evidence", has `Agent guidance`, `Wire format migration`, legacy `key=value` parsing warning, and current local XML/WIP evidence.
- Why it may poison context: runtime operator reference becomes agent memo, migration note, and evidence transcript; legacy wire behavior stays alive in retrieval.
- Suggested disposition: keep current marker/output/scenario contract; move bridge-use policy to `tools/rhino-bridge/AGENTS.md`; move transient WIP/XML evidence to API/source owner or dated report.

#### F-TOOL-05: AppUi/Compute/Persistence docs carry transient host gates, version-scared caveats, and `.claude` source anchors
- Severity: medium
- Reports: `agent-reports/12-code-comments-tools.md` L-05, L-06
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.AppUi/_ARCHITECTURE.md:54`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.AppUi/_ARCHITECTURE.md:112`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.AppUi/ROADMAP.md:75`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Compute/_ARCHITECTURE.md:180`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Persistence/_ARCHITECTURE.md:35`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Persistence/_ARCHITECTURE.md:165`
- Evidence: docs include `[DEFERRED]` GH2-Avalonia embedding research, decompiled DLL evidence, "per WIP drop" commands, settled/still-open native gates, "needs an agent with the macOS host", CoreML/ORT/Rhino crash caveats, deprecated package choices, no-version-pin language, and `.claude/skills` source anchors.
- Why it may poison context: architecture and roadmap docs preserve investigation transcripts and version-sensitive package fear instead of stable decisions plus proof routes.
- Suggested disposition: split durable constraints from evidence chronology; route command gates/dates to runbook/report; remove `.claude` skill paths from architecture unless they are active instruction sources.

#### F-TOOL-06: Test modules embed audit metadata, placeholders, and repeated `BRIDGE-DEFERRED` posture
- Severity: medium
- Reports: `agent-reports/12-code-comments-tools.md` L-07, L-08
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tests/tools/assay/core/test_engine.py:1`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tests/tools/assay/core/test_model.py:1`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tests/tools/assay/core/test_rail_package.py:1`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tests/tools/assay/conftest.py:269`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tests/csharp/libs/Rasm/Analysis/Bounds.spec.cs:9`
- Evidence: tests include `Source surface`, `Laws`, `xfail strict`, `bedrock: coverage pending`, methodology-hole comments, `foundation/W3 law`, and repeated all-caps `BRIDGE-DEFERRED` comments across static specs.
- Why it may poison context: retrieval can treat law-matrix metadata and deferred proof tokens as current proof inventory or accepted incomplete state.
- Suggested disposition: keep executable test names/assertions as truth; shorten or remove module docstrings that restate ownership; replace `bedrock` skips with issue/xfail conditions or tests; keep bridge-deferred comments only where they change behavior.

#### F-TOOL-07: Project-coupled examples appear in reusable layout/tool docs
- Severity: low
- Reports: `agent-reports/12-code-comments-tools.md` L-09
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/apps/README.md:13`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/apps/README.md:41`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/README.md:141`
- Evidence: docs use `apps/grasshopper/Radyab/`, `apps/grasshopper/Radyab/Radyab.csproj`, exact folder layout, `tests/csharp/libs/Rasm.Rhino`, and `rasm-bridge` as examples.
- Why it may poison context: if treated as reusable conventions, future agents may copy Radyab or `rasm-bridge` paths into unrelated plugin/package work.
- Suggested disposition: keep only if intentionally repo-specific; otherwise use placeholders or label as current exemplar.

#### F-TOOL-08: Source comments are mostly clean; a few retain narrative/provenance phrasing
- Severity: note
- Reports: `agent-reports/12-code-comments-tools.md` L-10
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/composition/registry.py:564`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/composition/registry.py:613`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/quality/rails/api.py:658`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Grasshopper/UI/Motion.cs:2169`
- Evidence: sampled source comments mostly explain invariants, but a few use provenance/history phrasing such as probe-cache token provenance and "stale lie".
- Suggested disposition: leave invariant comments unless touching the file; phrase as current invariant rather than history if edited.

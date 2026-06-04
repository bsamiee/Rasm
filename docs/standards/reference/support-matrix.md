# [SUPPORT_MATRIX_STANDARDS]

A support matrix states which runtime, platform, version, feature, or integration surface is `Supported`, `Maintenance`, `Limited`, `Deprecated`, `End of support`, `Retired`, or `Unsupported`, names the exact fix classes each status still grants, and points each status at refreshable evidence. It is a policy-backed reference document, not a roadmap, a release note, or a recovery procedure: it answers "is this combination supported right now, and until when," and nothing else.

Source of truth: this standard governs support-matrix document structure; each matrix names its own upstream source; lifecycle-model vocabulary follows [endoflife.date API](https://endoflife.date/docs/api), [Microsoft lifecycle](https://learn.microsoft.com/en-us/lifecycle/), and [Kubernetes version-skew policy](https://kubernetes.io/releases/version-skew-policy/).
Last verified: 2026-06-04
Review trigger: cross-cutting standards change, or canonical lifecycle/skew model changes.

This standard anchors to three lifecycle models: endoflife.date release-line dates, Microsoft lifecycle intersections, and Kubernetes-style version skew. The lifecycle vocabulary carries end of active support, end of life, and end of extended support as separate dates; the intersection model derives support from the earlier co-governing lifecycle; and the skew model states numeric bounds, direction, upgrade order, and unsupported combinations.

## [1][USE_WHEN]

Use a support matrix when a reader compares support facts across rows:

- product, runtime, platform, host, toolchain, browser, device, or deployment support;
- component version skew and compatibility bounds between two or more moving parts;
- feature availability by plan, edition, runtime, API version, region, or integration;
- deprecation, removal, retirement, or migration status of a named surface.

Route away when the reader's need is a sequence rather than a lookup. Future work order, release narrative, step-by-step migration, and operational recovery each belong to a different document type; the README corpus map owns that routing.

## [2][SOURCE_TRUTH]

Resolve every support fact against the first source that proves it, because a local restatement drifts the moment upstream policy moves:

1. Machine-readable repository truth: source, manifests, lockfiles, generated contracts, release metadata, package indexes, and runnable compatibility checks.
2. Official lifecycle, support, release, deprecation, migration, and compatibility policy from the owning vendor or upstream project.
3. Maintainer-controlled release notes, migration guides, known-limitations pages, issue records, or support announcements.
4. Local support-matrix prose, which holds only the subset that changes a reader decision.

Do not invent local lifecycle semantics when upstream policy owns the phase, date, support entitlement, or compatibility rule. When a generated compatibility check and prose disagree, the check controls and the prose is corrected.

## [3][PROFILES]

A support matrix carries one of five profiles. Choose the profile by what the reader is comparing, then apply that profile's extra required fields on top of the shared matrix fields. Split the page when a second profile would force a second status vocabulary or a second source of truth into the same matrix.

Lifecycle and platform profiles in one matrix:

```text conceptual
support-matrix/
  product-lifecycle.md      # release lines, phases, the three lifecycle dates, security posture
  runtime-support.md        # OS, language runtime, host, toolchain, browser, device, dependency floors
  compatibility.md          # component version skew, intersection cells, supported combinations
  feature-availability.md   # capability by plan, edition, runtime, region
  deprecations.md           # deprecated surface, replacement, removal window
```

- Product lifecycle: release lines, support phases, the three lifecycle dates, retirement, and security posture. Extra required content per row: the phase definition (which fix classes the phase still receives) and the three lifecycle dates as distinct facts.
- Runtime or platform support: operating systems, language runtimes, host versions, toolchains, browsers, devices, or deployment environments. Extra required content per row: minimum and maximum supported version of the runtime, the dependency or toolchain floor, and the rule that local support never extends past the upstream's own end of life.
- Compatibility matrix: component version skew, supported combinations, minimum and maximum versions, and upgrade-order constraints. Extra required content per row: the quantified skew bound, the skew direction, the upgrade order, the no-skip rule, the mixed-version limit, and at least one named unsupported combination. When two lifecycles co-govern, the matrix is a two-axis intersection table with a reading rule.
- Feature availability: capabilities by plan, edition, runtime, platform, API version, region, or integration. Extra required content per row: the entitlement or bound that gates the feature.
- API or feature deprecation: deprecated surfaces, replacements, warning signals, removal versions or dates, and migration paths. Extra required content per row: replacement surface and removal version or stated policy window.

## [4][SUPPORT_REGIME]

Name the support regime in `Scope`, because the regime is itself a support precondition the reader must satisfy. Two regimes govern how a row stays supported:

- Rolling (Modern): support holds only while the surface stays current-configured. Falling behind the current line drops support regardless of any published date. State "must stay current" as a precondition, not a footnote.
- Fixed-term: support holds for a fixed term independent of configuration. The published dates are the whole contract.

A matrix that mixes both regimes states which regime governs each row. A matrix with no named regime is non-conforming, because the reader cannot tell whether staying current is a condition of support.

## [5][PLACEMENT]

Place the matrix where the owner that can refresh it first looks:

- Shared support corpus: `docs/support/<surface>.md` when more than one owner reads it.
- Reference-adjacent matrix: the owner's reference corpus when the matrix is lookup data alongside other reference leaves.
- Package-local matrix: `{owner}/SUPPORT.md` only when support truth is local to that one owner.

## [6][REQUIRED_STRUCTURE]

Author every support matrix with these H2 sections in order. The opening metadata block follows the H1. Each row below states the section's cardinality, so an author knows what is mandatory, what is optional, and what repeats per surface.

```markdown template
# [SURFACE_SUPPORT_MATRIX]

Source of truth: <owning vendor policy page | generated compat-check | manifest path>
Last verified: YYYY-MM-DD
Review trigger: <upstream release | lifecycle-policy update | compat-check change>
Owner: <refresh owner, when one exists>

## [1][SCOPE]

## [2][SOURCE_TRUTH]

## [3][STATUS_VOCABULARY]

## [4][LIFECYCLE_DATES]

## [5][MATRIX]

## [6][READING_RULE]

## [7][COMPATIBILITY_BOUNDS]

## [8][DEPENDENCY_FLOORS]

## [9][EXCLUSIONS]

## [10][LIMITATIONS]

## [11][DEPRECATIONS_REMOVALS]

## [12][MIGRATION_PATHS]

## [13][EVIDENCE_CONDITIONAL]

## [14][BOUNDARIES]

## [15][REVIEW_CHECKLIST]

```

Section cardinality:

- Opening metadata: required, single; carries `Source of truth`, `Last verified`, and `Review trigger`, plus `Owner` where a refresh owner exists.
- `Scope`: required, single; names the supported surface, the one profile, and the support regime.
- `Source of truth`: required, single.
- `Status vocabulary`: required, single; defines only the statuses the matrix uses, each by the fix classes it grants.
- `Lifecycle dates`: required for the product-lifecycle and deprecation profiles, optional otherwise; one definition block per release line.
- `Matrix`: required, repeatable — one table or grouped subsection per profile axis.
- `Reading rule`: required when any matrix is a two-axis or intersection table; omit otherwise.
- `Compatibility bounds`: required for the compatibility profile, optional otherwise.
- `Dependency floors`: required when support is tied to an upstream runtime, OS, or toolchain; omit otherwise.
- `Exclusions`: required, single; enumerates unsupported configurations and combinations explicitly.
- `Limitations`: optional, repeatable per limited surface.
- `Deprecations and removals`: required when any row is `Deprecated`, `End of support`, or `Retired`; omit when no row carries those statuses.
- `Migration paths`: required when a deprecation has a replacement; omit otherwise.
- `Evidence`: conditional; use a page-level section only when one source and one trigger prove the whole matrix, otherwise attach evidence per row.
- `Boundaries`: required, single, one link per adjacent owner.
- `Review checklist`: required, single.

## [7][STATUS_VOCABULARY]

Define only the statuses the matrix actually uses, and define each by the exact fix classes it grants rather than by a color or icon. Name which of these the status receives: security fixes, bug fixes, feature work, documentation updates, assisted support, self-service support, and compatibility testing. A status that names no fix classes is non-conforming, because the reader cannot tell what the status actually entitles.

`Supported`: security, bug, and feature fixes; eligible for all stated support channels. This is the status a current stable or generally available release line carries.
`Maintenance`: security fixes and critical bug fixes only; no feature work, per the owning policy.
`Limited`: supported only for the stated capabilities, environments, entitlements, or bounds.
`Deprecated`: still present, discouraged, and scheduled or eligible for removal under a stated policy.
`End of support`: receives no new fixes, security updates, assisted support, or content updates unless an explicit extended-support program says otherwise.
`Retired`: the service, product, release, or feature is removed and no longer available.
`Unsupported`: not intended to work.

Carry status through the labeled term, not through color, icon, or badge as the only signal. A reader using a screen reader or copying the table as plain text must recover the status from the text.

## [8][LIFECYCLE_DATES]

State each release line as a definition block carrying three distinct lifecycle dates, never one collapsed date. The three-date taxonomy is load-bearing: collapsing it hides whether a surface still receives security fixes after feature and bug fixes stop. A lifecycle row with a single undifferentiated date is non-conforming.

The three dates carry fixed meanings, following the canonical schema:

- End of active support (`eoas`): bug fixes stop; security fixes may continue.
- End of life (`eol`): all fixes stop, including security; the mandatory date.
- End of extended support (`eoes`): the extended program ends, where one exists; otherwise `n/a`.

State what each lifecycle phase grants, and preserve the owner's date precision exactly. A phase block must name which fix classes the phase still receives when they differ. Use exact dates when the owner publishes exact dates, and month-level dates only when the owner publishes month-level dates; do not convert a month-level upstream date into a fabricated day.

Encode an unknown or undecided date explicitly, never as a blank. Use `still supported, date undecided` when the surface is supported but the date is unannounced, and `n/a — newer release expected` when no date applies. A blank reads as "no data" and ships an ambiguous claim.

```text conceptual
Release line: 8.x
Status: Maintenance
Phase grants: security fixes + critical bug fixes; no feature work
Released: 2024-11-12
Latest: 8.4.2
End of active support: 2025-11-12        # eoas: bug fixes stop
End of life: 2026-11-12                   # eol: security fixes stop
End of extended support: 2028-11-12       # eoes: extended program ends, or n/a
LTS: yes
Source of truth: vendor lifecycle policy page
```

## [9][MATRIX]

Use a table when row-and-column scanning is the clearest comparison, and keep it inside the table ceiling that [information-structure.md](../information-structure.md) owns. When a matrix exceeds that ceiling, split it by profile, status, platform, or phase before it loses scannability. When a single surface is read by field rather than compared across rows, use a definition block, not a one-row table. When a row needs a paragraph, nested steps, or a migration explanation, move that surface into a grouped subsection or a footnote.

Each row must stand alone. Include this field set, marking each field required, optional, or repeatable for the chosen profile:

- Surface: required; product, component, feature, integration, runtime, platform, or API.
- Version or scope: required; version, release line, channel, edition, plan, region, or environment.
- Status: required; one term from the status vocabulary.
- Key date: required for lifecycle and deprecation, optional otherwise; lifecycle profile carries all three dates in its block.
- Supported capabilities: optional; what the surface does support under this status.
- Unsupported capabilities: optional; what this status explicitly excludes.
- Compatibility bound: required for compatibility, optional otherwise; minimum, maximum, and skew direction.
- Requirement: optional and repeatable; dependency, entitlement, patch, or toolchain floor.
- Replacement: required when `Deprecated`, optional otherwise; replacement surface or migration target.
- Evidence: required; source path, contract, command, or official policy link.

Do not copy a large generated or vendor-owned matrix when the official source is stronger. Link the source and publish only the local subset that changes a reader decision.

When two lifecycles co-govern a cell, use a two-axis intersection table: each axis header carries its own end-of-support date, and the cell carries the derived supported-until date or `n/a` for an incompatible pair. Carry any conditional or exception support in a numbered footnote, never as prose inside a cell; a footnote keeps the cell atomic — a date, `n/a`, or a marker — while the condition lives below the table.

Conceptual two-axis intersection matrix:

| [OS_EOL]               | [OFFICE_2024_EOL] | [OFFICE_2021_EOL] |
| :--------------------- | :---------------- | :---------------- |
| OS 2025 — EOL Oct 2034 | Oct 2029          | Oct 2026 [^1]     |
| OS 2022 — EOL Oct 2031 | Oct 2026          | Oct 2026          |

[^1]: Conditional: security-bridge extension to a stated date; feature updates end at the earlier end of life.

Note: Rows above are conceptual samples; replace them with owner-verified lifecycle data in a real support matrix.

Conceptual compatibility record table:

| [INDEX] | [COMP_A]    | [COMP_B]      | [STATUS]      | [REASON]              | [EVIDENCE]                          |
| :-----: | :---------- | :------------ | :------------ | :-------------------- | :---------------------------------- |
|   [1]   | API 3.4-3.6 | Agent 3.4-3.6 | `Supported`   | API >= agent          | `docs/support/compatibility.md` row |
|   [2]   | API 3.6     | Agent 3.3     | `Unsupported` | skew exceeds 2 minor  | upstream compatibility policy       |
|   [3]   | API 3.3     | Agent 3.6     | `Unsupported` | newer agent forbidden | generated `compat-check` output     |

Note: Rows above are conceptual samples; replace them with owner-verified compatibility data in a real support matrix.

## [10][READING_RULE]

State the cell-derivation rule for any two-axis or intersection matrix, immediately above or below the first such table. Without it the grid is unreadable: the reader cannot tell how a cell value is derived. State the rule as a field-and-value sentence:

```text conceptual
Supported window = the earlier of the two component end-of-life dates.
Incompatible pair = n/a.
```

## [11][COMPATIBILITY_BOUNDS]

State the bounds explicitly for any compatibility-profile row, because an unstated direction reads as bidirectional and ships a false support claim. Quantify the skew numerically; a direction alone is insufficient. For each compatible pair, state every field below as a discrete, extractable fact:

```text conceptual
Skew bound: agent may be up to 3 minor versions older than server.
Direction: server may lead; agent must never be newer.
Upgrade order: server first, then agents.
Skip rule: no minor-version skips on upgrade.
Mixed-version limit: <= 2 minor for <= a stated duration during rollout.
Named unsupported combination: server 3.6 with agent 3.3.
```

The named unsupported combination makes the boundary concrete rather than implied. The mixed-version limit must carry a time bound when the mixed state is permitted only transiently during rollout.

## [12][DEPENDENCY_FLOORS]

State the dependency floor for any row whose support is tied to an upstream runtime, OS, or toolchain, because the matrix must never over-promise beyond what the upstream itself supports. For each such row, state the minimum upstream version, the ceiling upstream version where one is bounded, and the controlling rule: local support never extends past the upstream's own end of life. When the upstream reaches end of life, the dependent row reaches end of support on the same date regardless of any local schedule.

## [13][EXCLUSIONS]

Enumerate the unsupported configurations and combinations explicitly, as a bulleted list of named cases. The boundary must be concrete, not inferred from a missing row: an agent must not read the absence of a row as support. State each unsupported configuration as its own item, naming the specific combination that is excluded.

```text conceptual
- Server 3.6 paired with any agent older than 3.4.
- The bundled runtime on an OS past its own end of life.
- Mixed-region deployment under the single-region plan.
```

## [14][DEPRECATIONS_REMOVALS]

Distinguish `Deprecated`, `End of support`, `Retired`, and `Unsupported` explicitly; collapsing them hides whether the surface still runs. Render each deprecation entry as a definition block or per-item record block with one `label: value` line per field — not a bullet list of fields. The seven fields each entry carries are:

- the deprecated surface and the first deprecated version or announcement date;
- current availability (does it still run, and under which status);
- the warning signal emitted at use, when one exists;
- the replacement surface;
- the removal version or the stated policy window;
- the behavior change a caller will observe;
- the source of truth that owns the removal decision.

```text conceptual
Surface: legacy mesh-extraction endpoint v1
Status: Deprecated
Available: still runs under Deprecated status
Warning signal: deprecation notice logged at first call per process
Replacement: mesh-extraction endpoint v2
Removal: 9.0 (next major), per 2-major-version policy window
Behavior change: v1 returns coarse tessellation; v2 returns owner-precision mesh
Source of truth: vendor deprecation-policy page
```

## [15][MIGRATION_PATHS]

Keep migration guidance in a support matrix decision-oriented and bounded; the full procedure lives in a how-to guide. For each migration, name the source and target versions or features, whether the path is direct or staged, the prerequisites, the known breaking changes, the validation signal that confirms success, and a link to the owning how-to or migration guide. Move step-by-step migration work to a how-to guide and operational recovery to a runbook; this matrix records the target and the status, not the keystrokes.

## [16][EVIDENCE_CONDITIONAL]

Attach evidence to every support status, beside the row or surface it proves, using the freshness fields the evidence standard defines. Use a page-level evidence section only when one source and one trigger prove the whole matrix. Otherwise attach claim-level evidence per drift-prone row. Prefer local manifests, lockfiles, generated contracts, compatibility-test output, official lifecycle pages, release notes, migration guides, security bulletins, and known-limitations pages from the owner.

Name the event that makes a status stale with a `Review trigger:` field on the drift-prone row or surface, and use a calendar `Last verified:` date only when the upstream source changes on a schedule. Triggers that apply to support matrices include:

- an upstream release, patch, or lifecycle-policy update;
- a support announcement, retirement notice, or security advisory;
- a package, runtime, host, platform, or generated-contract version change;
- a compatibility-test, certification, or conformance-matrix change;
- a migration-guide or deprecation-policy update;
- a local manifest, lockfile, toolchain, or supported-environment change.

```text conceptual
Status: Supported
Evidence: Directory.Packages.props pin + generated compat-check pass
Last verified: 2026-06-04
Review trigger: host SDK version change or compat-check failure
```

## [17][BOUNDARIES]

- [reference.md](reference.md) owns curated lookup facts when support status is one fact among many rather than the whole table.
- [api.md](api.md) owns generated or contract-backed API surface truth that a support row points at.
- [roadmap.md](../explanation/roadmap.md) owns future support intent and milestone exit proof.
- [how-to.md](../task/how-to.md) owns step-by-step migration procedures.
- [runbook.md](../task/runbook.md) owns operational recovery for support-impacting incidents.
- [README.md](../README.md) owns document-type routing, placement, and lifecycle.

## [18][REVIEW_CHECKLIST]

- [ ] Opening metadata carries `Source of truth`, `Last verified`, and `Review trigger`.
- [ ] Scope names the supported surface, one profile, and the support regime (rolling or fixed-term).
- [ ] Source of truth is explicit and ordered.
- [ ] Each status used is defined by the fix classes it grants, not by an icon.
- [ ] Each lifecycle line carries end of active support, end of life, and end of extended support (or `n/a`) as distinct dates.
- [ ] Lifecycle rows state which fix classes the phase grants and preserve the owner's date precision.
- [ ] Tables stay within the table ceiling that [information-structure.md](../information-structure.md) owns, and carry no paragraph cells.
- [ ] A single surface read by field uses a definition block, not a one-row table.
- [ ] Two-axis matrices state the earlier-end-of-life reading rule, and each cell is a date or `n/a`.
- [ ] Compatibility rows quantify skew, state direction, upgrade order, skip rule, mixed-version limit, and one named unsupported combination.
- [ ] Dependency-floor rows state the upstream minimum and assert support never exceeds the upstream's end of life.
- [ ] Exclusions enumerate unsupported configurations explicitly, never inferred from a missing row.
- [ ] Deprecations distinguish `Deprecated`, `End of support`, `Retired`, and `Unsupported`.
- [ ] Deprecation entries use definition blocks or per-item record blocks, not flat bullet lists of fields.
- [ ] Migration rows name a target and link the owning how-to, not embedded steps.
- [ ] Unknown or undecided dates are encoded explicitly, never left blank.
- [ ] Conditional or exception support uses footnotes, never prose inside a cell.
- [ ] Every status has claim-level evidence with a `Review trigger:` or `Last verified:` field.

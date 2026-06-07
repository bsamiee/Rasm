Question: Which Mermaid renderer claims are source-proven enough to promote into Rasm standards?
Type: source-scan
Lane: track-external-research
Merge key: docs/standards/proof.md :: Mermaid renderer support proof packet :: promote
Target owner: docs/standards/proof.md
Source basis: official Mermaid docs, mermaid-cli GitHub/source, local `package.json`, `pnpm-workspace.yaml`, `pnpm-lock.yaml`, `pnpm exec mmdc -h`, and active standards.
Promotion target: docs/standards/proof.md
Outcome: PROMOTE

## [SOURCE_SET_READ]

Local instruction and session sources:
- `CLAUDE.md`
- `AGENTS.md`
- `docs/standards/README.md`
- `docs/standards/AGENTS.md`
- `.reports/AGENTS.md`
- `.reports/standards-structure-notation-060626/README.md`
- `docs/standards/information-structure.md`
- `docs/standards/proof.md`
- `docs/standards/explanation/architecture.md`
- `docs/standards/reference/support-matrix.md`
- `.reports/standards-structure-notation-060626/track-synthesis/00-collective-task-list.md`

External and installed renderer sources:
- Mermaid configuration: https://mermaid.js.org/config/configuration.html
- Mermaid directives: https://mermaid.js.org/config/directives
- Mermaid layouts: https://mermaid.js.org/config/layouts.html
- Mermaid config schema: https://mermaid.js.org/config/schema-docs/config.html
- Mermaid theme configuration: https://mermaid.js.org/config/theming
- Mermaid accessibility: https://mermaid.js.org/config/accessibility.html
- Mermaid CLI repository and README: https://github.com/mermaid-js/mermaid-cli
- Mermaid CLI latest release: https://github.com/mermaid-js/mermaid-cli/releases/tag/11.15.0
- `package.json`
- `pnpm-workspace.yaml`
- `pnpm-lock.yaml`
- `node_modules/@mermaid-js/mermaid-cli/package.json`
- `node_modules/@mermaid-js/mermaid-cli/README.md`
- `node_modules/@mermaid-js/mermaid-cli/src/index.js`
- `pnpm exec mmdc --version`
- `pnpm exec mmdc -h`

## [FINDINGS]

### [1][FRONTMATTER_CONFIG_IS_SOURCE_PROVEN]

File/section finding: `docs/standards/information-structure.md` lines 438-440 allow Mermaid `config:` inside a diagram fence, require exact `mermaid` fences, prefer `layout: elk`, `look: neo`, `theme: base`, and place accessibility text after the diagram declaration.

Evidence:
- Official Mermaid configuration docs list default config, site `initialize`, frontmatter from v10.5.0, and deprecated directives as configuration sources; frontmatter applies to render config and can override all configuration except secure configs: https://mermaid.js.org/config/configuration.html.
- Official directives docs state directives are deprecated from v10.5.0 and tell authors to use the `config` key in frontmatter: https://mermaid.js.org/config/directives.
- Local standard evidence: `docs/standards/information-structure.md:438-440`.

Weakness: the active standard states the rule correctly but the proof owner does not yet name the maintained source and local render proof route for that renderer-dependent claim.

Correction: promote a `docs/standards/proof.md` renderer-support packet that says Mermaid diagram frontmatter `config:` is accepted only when backed by maintained Mermaid docs and, for repository-dependent examples, local `mmdc` render proof or an explicit `Proof gap:`.

Owner: `docs/standards/proof.md`.

Ripple: `docs/standards/information-structure.md`, `docs/standards/explanation/architecture.md`, `docs/standards/reference/support-matrix.md`.

Decision: PROMOTE.

Proof gaps: no active diagram was rendered in this pass; this report proves source support, not current active-example render output.

### [2][ELK_LOOK_THEME_NEED_A_SPLIT_PROOF_RECORD]

File/section finding: `docs/standards/information-structure.md:440`, `docs/standards/explanation/architecture.md:260-270`, and `docs/standards/reference/support-matrix.md:195-205` use the same renderer config packet: `layout: elk`, `look: neo`, `theme: base`, and `elk` options.

Evidence:
- Mermaid layouts docs list `elk` as a supported layout and say layout can be specified in YAML config or initialization options: https://mermaid.js.org/config/layouts.html.
- Mermaid config schema defines `look` with allowed values `classic`, `handDrawn`, and `neo`, defaulting to `classic`: https://mermaid.js.org/config/schema-docs/config.html.
- Mermaid config schema defines `layout`, defaulting to `dagre`, and exposes the `elk` config object: https://mermaid.js.org/config/schema-docs/config.html.
- Mermaid theming docs list `base` as an available theme and the only modifiable theme for customizations: https://mermaid.js.org/config/theming.
- Mermaid config schema includes `theme` enum values including `default`, `base`, `dark`, `forest`, `neutral`, `neo`, and related variants: https://mermaid.js.org/config/schema-docs/config.html.
- Local examples: `docs/standards/explanation/architecture.md:260-270`; `docs/standards/reference/support-matrix.md:195-205`.

Weakness: local standards combine renderer support, house style, and diagram-content guidance in one sentence. That makes future agents treat `layout`, `look`, `theme`, and ELK tuning as equally mandatory renderer facts, even though `theme: base` is only needed when theme variables are customized and ELK option behavior still needs local render proof.

Correction: keep form guidance in `information-structure.md`, but promote proof-owned wording that separates source-proven renderer options from local house defaults:
- Source-proven: frontmatter `config`, `layout: elk`, `look: neo`, `theme: base`, and `elk` object existence.
- Local default: prefer the shared config packet for standards examples.
- Render proof: run `mmdc` against changed diagram-bearing Markdown before claiming local examples render.

Owner: `docs/standards/proof.md`.

Ripple: `docs/standards/information-structure.md`, `docs/standards/explanation/architecture.md`, `docs/standards/reference/support-matrix.md`, and any active standards examples that reuse the shared config packet.

Decision: PROMOTE.

Proof gaps: no ELK visual layout screenshot or SVG inspection was produced; local render proof remains required before claiming a diagram is visually correct.

### [3][ACCESSIBILITY_CLAIMS_ARE_SUPPORTED_BUT_RENDERER_DEPENDENT]

File/section finding: `docs/standards/information-structure.md:440` requires `accTitle` and `accDescr` near Mermaid declarations, while `docs/standards/proof.md:154-163` classifies diagram accessibility text as a renderer claim.

Evidence:
- Mermaid accessibility docs define `accTitle:` as a single-line title and `accDescr:` as either a single-line description or a multi-line block in braces: https://mermaid.js.org/config/accessibility.html.
- The same docs show generated SVG labeling with `aria-labelledby`, `aria-describedby`, `title`, and `desc`: https://mermaid.js.org/config/accessibility.html.
- Mermaid CLI README shows Markdown transformation preserving custom title/description as generated image alt/title text: `node_modules/@mermaid-js/mermaid-cli/README.md:61-104`.
- Local proof rule: `docs/standards/proof.md:154-163`.

Weakness: source support proves Mermaid syntax and intended SVG accessibility output, but not that every downstream renderer consuming the Markdown will expose that accessibility text identically.

Correction: promote a proof packet that treats `accTitle` and `accDescr` as required source syntax for local Mermaid examples, while requiring one of these proof classes before claiming output behavior: official Mermaid accessibility source, local `mmdc` SVG inspection, or `Proof gap:` for the target renderer.

Owner: `docs/standards/proof.md`.

Ripple: `docs/standards/information-structure.md`, `docs/standards/explanation/architecture.md`, `docs/standards/reference/support-matrix.md`, and any diagram-bearing type standards.

Decision: PROMOTE.

Proof gaps: this pass did not inspect generated SVG from local standards diagrams.

### [4][RENDERING_LIMITS_AND_AUTHORING_LIMITS_MUST_NOT_COLLAPSE]

File/section finding: `docs/standards/information-structure.md:450-452` sets human-maintained diagram size guidance, while Mermaid config schema exposes renderer limits such as `maxTextSize` and `maxEdges`.

Evidence:
- Mermaid config schema sets `maxTextSize` default to `50000` and `maxEdges` default to `500`: https://mermaid.js.org/config/schema-docs/config.html.
- The same schema marks `secure`, `securityLevel`, `startOnLoad`, `maxTextSize`, `suppressErrorRendering`, and `maxEdges` as secure defaults that cannot be changed by unsafe author-level overrides: https://mermaid.js.org/config/schema-docs/config.html.
- Local standard sets a stricter authoring-review shape: one diagram, one reader question, roughly 5-9 nodes, about 12 meaningful edges, split when multiple decision nodes, lifecycles, unrelated subgraphs, or table-cell repetition appear: `docs/standards/information-structure.md:450-452`.

Weakness: agents may misread the local 5-9 node guidance as a Mermaid renderer limit, or misread Mermaid's larger renderer limits as permission to publish unreadable standards diagrams.

Correction: promote proof wording that says renderer constraints and Rasm authoring constraints are separate:
- Mermaid schema limits prove what the renderer bounds.
- Rasm standards limits control source review, reader action, and maintainability.
- A diagram can render and still fail the Rasm information-structure rule.

Owner: `docs/standards/proof.md` for proof vocabulary; `docs/standards/information-structure.md` for diagram authoring limits.

Ripple: `docs/standards/explanation/architecture.md`, `docs/standards/reference/support-matrix.md`, `docs/standards/task/how-to.md`, `docs/standards/task/runbook.md`, and README standards that discuss diagrams.

Decision: PROMOTE.

Proof gaps: no active standards diagram was counted or rendered in this pass; the finding is a rule-separation correction.

### [5][LOCAL_MMDC_ROUTE_IS_CONFIGURED_AND_USABLE]

File/section finding: `docs/standards/proof.md:140-165` requires configured gates and local render proof for renderer-dependent claims; the session task list asks for a renderer-support proof packet and configured-gate record at `track-synthesis/00-collective-task-list.md:330-355`.

Evidence:
- Root `package.json` declares `@mermaid-js/mermaid-cli` as a dev dependency via catalog: `package.json:26`.
- `pnpm-workspace.yaml` pins the catalog entry to `11.14.0`: `pnpm-workspace.yaml:30`.
- `pnpm-lock.yaml` locks `@mermaid-js/mermaid-cli` to `11.14.0`: `pnpm-lock.yaml:33-35`, `pnpm-lock.yaml:272-274`.
- Installed package source declares `mmdc` as the bin and version `11.14.0`: `node_modules/@mermaid-js/mermaid-cli/package.json:1-11`.
- `pnpm exec mmdc --version` returned `11.14.0`.
- `pnpm exec mmdc -h` proved the local command supports Markdown input, `svg/png/pdf` output, artifact routing via `--artefacts`, Mermaid JSON config via `--configFile`, Puppeteer config, and stdin input.
- CLI README confirms Markdown transformation finds Mermaid diagrams, emits SVG files, and rewrites Markdown references: `node_modules/@mermaid-js/mermaid-cli/README.md:61-104`.
- CLI source confirms `.md`/`.markdown` inputs are treated as Markdown, fenced Mermaid code blocks are extracted, and artifact output can be redirected: `node_modules/@mermaid-js/mermaid-cli/src/index.js:113-118`, `node_modules/@mermaid-js/mermaid-cli/src/index.js:455-545`.

Weakness: the repo has local Mermaid CLI availability, but no single standards-owned proof packet tells agents when to run it, what output proves, or what not to claim.

Correction: promote this configured-gate record to `docs/standards/proof.md`:

```text template
Renderer: Mermaid
Configured command: `pnpm exec mmdc -i <changed-markdown> -a .artifacts/mermaid -q`
Use when: changed Markdown contains Mermaid diagrams and the change claims Mermaid render behavior, generated diagram output, diagram accessibility output, or visual layout.
Evidence: `package.json`, `pnpm-workspace.yaml`, `pnpm-lock.yaml`, `pnpm exec mmdc --version`, `pnpm exec mmdc -h`, and generated artifacts when run.
Proof gap: state when local render proof is intentionally unrun, unavailable, or not needed because no render claim changed.
Review trigger: Mermaid CLI version, Mermaid package version, diagram syntax, renderer config, artifact path, accessibility text, or changed diagram-bearing Markdown.
```

Owner: `docs/standards/proof.md`.

Ripple: `docs/standards/AGENTS.md`; `tools/assay/README.md` only if active standards decide to route Mermaid validation through that tool instead of direct `mmdc`.

Decision: PROMOTE.

Proof gaps: no full active-corpus Mermaid render gate ran in this pass.

### [6][UPSTREAM_LATEST_IS_NOT_LOCAL_INSTALLED_TRUTH]

File/section finding: standards should cite current Mermaid docs for capability support, but local proof claims must name the installed CLI version when they depend on repository tooling.

Evidence:
- GitHub latest release for `mermaid-js/mermaid-cli` is `11.15.0`, released 2026-05-13, with changes including `--jobs`, fractional scaling, and `@mermaid-js/layout-elk` bump: https://github.com/mermaid-js/mermaid-cli/releases/tag/11.15.0.
- Local repository manifests and installed package are on `@mermaid-js/mermaid-cli` `11.14.0`: `pnpm-workspace.yaml:30`; `pnpm-lock.yaml:33-35`; `node_modules/@mermaid-js/mermaid-cli/package.json:1-11`; `pnpm exec mmdc --version`.

Weakness: current web research may tempt a standards edit to claim latest CLI flags or behavior that this checkout does not have. Conversely, local `11.14.0` should not block citing stable official Mermaid syntax docs for generic Mermaid capabilities.

Correction: promote proof wording that uses two evidence classes:
- Current maintained Mermaid docs for syntax/config/accessibility support.
- Local manifest/tool output for available `mmdc` flags, version-specific CLI behavior, and repository validation commands.

Owner: `docs/standards/proof.md`.

Ripple: `docs/standards/information-structure.md`, `docs/standards/AGENTS.md`, `tools/assay/README.md`, and any future Mermaid validation script.

Decision: HOLD until active wording is edited; do not upgrade or repin tooling as part of this report.

Proof gaps: no comparison render was run between CLI `11.14.0` and upstream `11.15.0`; no claim is made that local diagrams fail or need an upgrade.

## [RECOMMENDATIONS]

Promote one proof-owned renderer-support packet instead of spreading source URLs across diagram form rules. The durable rule should say:
- Mermaid diagram syntax and per-diagram `config:` frontmatter are sourced from official Mermaid docs.
- Local validation of repository Markdown uses the installed `mmdc` command, not upstream latest flags unless the manifest is updated.
- Renderer-dependent claims require local render proof, maintained renderer docs, or an explicit `Proof gap:`.
- Diagram accessibility claims must distinguish source syntax from generated SVG or target-renderer behavior.
- Human diagram-size guidance belongs to `information-structure.md`; renderer hard limits belong to proof/source notes only when a claim depends on them.

Keep active standards untouched in this pass. This report supplies source evidence for the existing task-list items:
- `Add renderer-support proof packet`
- `Clarify configured docs gates versus proof gaps`
- `Fix architecture Mermaid source indentation`

## [PROOF_GAPS]

- Local `mmdc` was not run against active standards diagrams in this pass.
- Generated SVG accessibility output was not inspected for active standards examples.
- GitHub Markdown rendering behavior was not separately verified; only official Mermaid and local CLI sources were used for Mermaid claims.
- Link, anchor, and docs-build gates remain outside this Mermaid renderer pass unless a configured command is later found.

## [VALIDATION]

Report path: `.reports/standards-structure-notation-060626/track-external-research/02-mermaid-renderer-proof.md`.

Required validation: `git diff --check -- .reports/standards-structure-notation-060626/track-external-research/02-mermaid-renderer-proof.md`.

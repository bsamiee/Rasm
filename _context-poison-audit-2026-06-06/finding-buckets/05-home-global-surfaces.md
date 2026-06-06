# Home Global Surfaces Findings

Source: `../final-report.md`
Scope: `~/.codex` and `~/.claude` active global instruction and skill surfaces.
Finding count: 11

The finding blocks below are copied verbatim from `../final-report.md`.

## Findings

#### F-HOME-CODEX-01: `~/.codex/AGENTS.md` duplicates repo-level policy in global scope
- Severity: high
- Reports: `agent-reports/10-home-codex-skills.md` J1
- Locations:
  - `/Users/bardiasamiee/.codex/AGENTS.md:4`
  - `/Users/bardiasamiee/.codex/AGENTS.md:15`
  - `/Users/bardiasamiee/.codex/AGENTS.md:28`
  - `/Users/bardiasamiee/.codex/AGENTS.md:42`
  - `/Users/bardiasamiee/.codex/AGENTS.md:65`
- Evidence: global `AGENTS.md` defines file-type skill routing, greenfield refactor doctrine, polymorphism/helper rules, planning shape, navigation, research provider order, and language baselines, then says repository routes/command catalogs belong in active repo instructions.
- Why it may poison context: Rasm-style defaults pre-load into every repository before local repo instructions can define their own routes, versions, and planning policy.
- Suggested disposition: keep only portable prompt/document hygiene and safety in home `AGENTS.md`; move skill routing, language baselines, command ownership, and planning shape to repo owners.

#### F-HOME-CODEX-02: `cs-analyzer-rulecraft` is Rasm-specific but stored as a global reusable skill
- Severity: high
- Reports: `agent-reports/10-home-codex-skills.md` J2
- Locations:
  - `/Users/bardiasamiee/.codex/skills/cs-analyzer-rulecraft/SKILL.md:3`
  - `/Users/bardiasamiee/.codex/skills/cs-analyzer-rulecraft/SKILL.md:15`
  - `/Users/bardiasamiee/.codex/skills/cs-analyzer-rulecraft/SKILL.md:41`
  - `/Users/bardiasamiee/.codex/skills/cs-analyzer-rulecraft/SKILL.md:75`
  - `/Users/bardiasamiee/.codex/skills/cs-analyzer-rulecraft/SKILL.md:117`
- Evidence: skill names `tools/cs-analyzer`, `docs/system-api-map`, analyzer output locking, C# 14, LanguageExt, Thinktecture, CSP-style rules, and broad Rasm-like production proof loops.
- Why it may poison context: unrelated analyzer repos inherit Rasm analyzer topology, docs routes, and proof habits.
- Suggested disposition: move to Rasm-scoped project skills or rewrite as generic analyzer-rule guidance using repo-declared analyzer/project/proof routes.

#### F-HOME-CODEX-03: Global `testing-ts` skill contains Parametric Portal imports, topology, hooks, and app ports
- Severity: high
- Reports: `agent-reports/10-home-codex-skills.md` J3
- Locations:
  - `/Users/bardiasamiee/.codex/skills/testing-ts/SKILL.md:47`
  - `/Users/bardiasamiee/.codex/skills/testing-ts/SKILL.md:85`
  - `/Users/bardiasamiee/.codex/skills/testing-ts/references/categories.md:136`
  - `/Users/bardiasamiee/.codex/skills/testing-ts/references/guardrails.md:16`
  - `/Users/bardiasamiee/.codex/skills/testing-ts/templates/unit-pbt.spec.template.md:24`
  - `/Users/bardiasamiee/.codex/skills/testing-ts/templates/contract.spec.template.md:25`
- Evidence: skill names `packages/server`, `@parametric-portal/*`, `.claude/hooks/validate-spec.sh`, Playwright agent pipeline, triple-app bootstrap `api (:4000)`, `parametric_icons (:3001)`, and `test_harness (:3002)`.
- Why it may poison context: all TypeScript test generation can inherit Parametric Portal package names, hooks, ports, import order, and topology.
- Suggested disposition: move examples/templates into a project-scoped skill; replace imports with neutral placeholders and route app bootstrap to active repo test instructions.

#### F-HOME-CODEX-04: Global `testing-cs` carries Rasm-style test rails and runtime scenario ownership
- Severity: high
- Reports: `agent-reports/10-home-codex-skills.md` J4
- Locations:
  - `/Users/bardiasamiee/.codex/skills/testing-cs/SKILL.md:16`
  - `/Users/bardiasamiee/.codex/skills/testing-cs/SKILL.md:24`
  - `/Users/bardiasamiee/.codex/skills/testing-cs/SKILL.md:40`
  - `/Users/bardiasamiee/.codex/skills/testing-cs/SKILL.md:117`
  - `/Users/bardiasamiee/.codex/skills/testing-cs/references/rails-tooling.md:8`
  - `/Users/bardiasamiee/.codex/skills/testing-cs/references/bridge-runtime.md:17`
- Evidence: skill defines active repo testkit, `docs/testing-libs`, exact spec/testkit/runtime/mutation/architecture/tooling/benchmark/fuzz locations, quality-router validation ladder, and runtime scenario rules.
- Why it may poison context: global C# testing guidance imports Rasm host/runtime separation and quality-router behavior into other C# repos.
- Suggested disposition: keep generic law/oracle/test-shape guidance global; move Rasm rails to repo instructions or project-scoped skills.

#### F-HOME-CODEX-05: Home skills duplicate command catalogs, validation ladders, frozen versions, orchestration meta, and local examples
- Severity: medium
- Reports: `agent-reports/10-home-codex-skills.md` J5, J6, J7, J8
- Locations:
  - `/Users/bardiasamiee/.codex/skills/nx-tools/SKILL.md:20`
  - `/Users/bardiasamiee/.codex/skills/testing-ts/SKILL.md:55`
  - `/Users/bardiasamiee/.codex/skills/github-actions/references/act_usage.md:12`
  - `/Users/bardiasamiee/.codex/skills/speech/SKILL.md:9`
  - `/Users/bardiasamiee/.codex/skills/cs-analyzer-rulecraft/SKILL.md:43`
  - `/Users/bardiasamiee/.codex/skills/nx-tools/SKILL.md:45`
- Evidence: skills contain Nx command catalogs, `pnpm exec nx test -- --coverage`, `npx stryker run`, current actionlint/act versions, hard-coded OpenAI model names/behavior, explicit 8-10 sub-agent orchestration, `@parametric-portal/types`, and `CLAUDE.md` examples.
- Why it may poison context: home skills become stale source owners and inject local examples/orchestration into unrelated work.
- Suggested disposition: route commands to tool help or active repo owners; replace current-version/model claims with live lookup or fallback-snapshot labels; gate orchestration on explicit user request; neutralize local examples.

#### F-HOME-CLAUDE-01: Global `.claude/CLAUDE.md` duplicates repo-owned policy
- Severity: high
- Reports: `agent-reports/11-home-claude-active.md` F1, F8
- Locations:
  - `/Users/bardiasamiee/.claude/CLAUDE.md:34`
  - `/Users/bardiasamiee/.claude/CLAUDE.md:39`
  - `/Users/bardiasamiee/.claude/CLAUDE.md:43`
  - `/Users/bardiasamiee/.claude/CLAUDE.md:49`
  - `/Users/bardiasamiee/.claude/CLAUDE.md:58`
  - `/Users/bardiasamiee/.claude/rules/output-conventions.md:3`
  - `/Users/bardiasamiee/.claude/rules/research-protocol.md:3`
- Evidence: global `.claude` repeats repo plan shape, output structure, research freshness, documentation hygiene, greenfield/refactor posture, and quality gate reminders.
- Why it may poison context: repo-specific norms become ambient user preferences for non-Rasm repos and create multiple authority layers for Rasm.
- Suggested disposition: shrink global `.claude/CLAUDE.md` to stable user preferences; let repo `CLAUDE.md`/`AGENTS.md` own repo policy and detailed anti-patterns.

#### F-HOME-CLAUDE-02: Active research freshness windows conflict
- Severity: high
- Reports: `agent-reports/11-home-claude-active.md` F2
- Locations:
  - `/Users/bardiasamiee/.claude/CLAUDE.md:28`
  - `/Users/bardiasamiee/.claude/rules/research-protocol.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md:34`
- Evidence: active rules mention latest-stable assumptions, `<=6 months`, `2025+`, repo `last 9 months`, and the current global execution policy uses `3-4 months`.
- Why it may poison context: agents spend reasoning budget reconciling conflicting freshness windows and may pick the wrong one.
- Suggested disposition: keep only "use current primary sources for freshness-sensitive facts" globally; let repo manifests own exact windows.

#### F-HOME-CLAUDE-03: Mandatory `Further Considerations` footer conflicts with concision
- Severity: medium
- Reports: `agent-reports/11-home-claude-active.md` F3
- Locations:
  - `/Users/bardiasamiee/.claude/CLAUDE.md:39`
  - `/Users/bardiasamiee/.claude/rules/output-conventions.md:7`
- Evidence: global file requires every substantive response to end with `Further Considerations` while also demanding minimized token waste, no preamble, no sign-off summaries, and every paragraph changing action.
- Why it may poison context: fixed footer sections become process-form boilerplate even for direct status updates or concise final answers.
- Suggested disposition: delete the mandatory footer; include caveats only when they change the decision.

#### F-HOME-CLAUDE-04: Global no-control-flow rule is broader than repo domain-logic policy
- Severity: high
- Reports: `agent-reports/11-home-claude-active.md` F4
- Locations:
  - `/Users/bardiasamiee/.claude/CLAUDE.md:9`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md:60`
- Evidence: global rule bans `if`/`else`/`for`/`while`/`switch`/`try`/`catch` across all languages, while repo policy scopes the strict rule to domain logic with boundary exceptions.
- Why it may poison context: adapters, scripts, CLIs, generated code, tests, and operational glue can appear invalid despite skill/repo exceptions.
- Suggested disposition: move language mechanics to skills and keep global preference as a bias; preserve repo's narrower domain-logic formulation.

#### F-HOME-CLAUDE-05: Fixed subagent delegation trigger is process narration
- Severity: medium
- Reports: `agent-reports/11-home-claude-active.md` F5
- Locations:
  - `/Users/bardiasamiee/.claude/CLAUDE.md:21`
  - `/Users/bardiasamiee/.claude/settings.json:26`
- Evidence: global rule mandates delegation when reading more than three files, despite this being an arbitrary process trigger.
- Why it may poison context: agents may fragment simple work and produce unnecessary delegation boilerplate.
- Suggested disposition: replace with "delegate only when a suitable agent exists and the task boundary is independently verifiable."

#### F-HOME-CLAUDE-06: Benchmark artifacts and plan archives sit in paths that can be mistaken for active context
- Severity: medium
- Reports: `agent-reports/11-home-claude-active.md` F6, F7
- Locations:
  - `/Users/bardiasamiee/.claude/skills/coding-pg-workspace/iteration-2/benchmark.md:1`
  - `/Users/bardiasamiee/.claude/plans`
- Evidence: `.claude/skills/.../benchmark.md` is a benchmark report with eval failures/future iteration suggestions and no `SKILL.md`; `.claude/plans` contains 345 generated plan/prompt/agent/history files with no active boundary marker.
- Why it may poison context: benchmark evidence under `skills/` or unmarked generated plans can be mistaken for skill instructions or active planning context.
- Suggested disposition: move benchmark artifacts out of `.claude/skills` or mark non-loadable; add an archival boundary for `.claude/plans`.

# Agent 10 Report: Home Codex Skills

Scope audited:
- `/Users/bardiasamiee/.codex/AGENTS.md`
- `/Users/bardiasamiee/.codex/skills/**`

Exclusions honored: memories, plugin caches, vendor imports, tmp folders, and rollout summaries. I inventoried 38 `SKILL.md` files, including the hidden `.system` skill folder, and used targeted reads for references/templates where searches surfaced poisoning smells. I did not audit every script body line-by-line because the requested smells are instruction, reference, and template poisoning risks rather than implementation bugs.

## Executive Findings

| ID | Severity | Surface | Finding |
| :-- | :-- | :-- | :-- |
| J1 | High | `~/.codex/AGENTS.md` | Global policy duplicates repo instruction authority and can override or pre-bias unrelated repositories. |
| J2 | High | `cs-analyzer-rulecraft` | A Rasm-specific analyzer rulecraft workflow lives as a reusable global skill. |
| J3 | High | `testing-ts` | Parametric Portal paths, package names, hooks, app topology, and templates are baked into a reusable TypeScript testing skill. |
| J4 | High | `testing-cs` | Rasm-style C# test rails and runtime scenario ownership are encoded as global skill doctrine. |
| J5 | Medium | multiple skills | Validation ladders and command catalogs are duplicated inside reusable skills instead of routing to source owners. |
| J6 | Medium | multiple skills | Version and current-model catalogs are embedded in reusable guidance and will drift. |
| J7 | Medium | `cs-analyzer-rulecraft`, `testing-ts`, system skills | Agent orchestration and source-loading meta are embedded in reusable task guidance. |
| J8 | Medium | `nx-tools`, `testing-ts` templates | Examples bake in local package names, local root files, and local project topology. |

## J1: Global AGENTS Duplicates Repo Policy

`/Users/bardiasamiee/.codex/AGENTS.md` is framed as a global execution policy, but it carries repo-like skill routing, language baselines, research provider order, navigation rules, and plan discipline. Those are exactly the kinds of claims the same file says should stay with active repo instructions and source owners.

Evidence:
- `/Users/bardiasamiee/.codex/AGENTS.md:4-13` defines file-type skill routing and says repo `CLAUDE.md` wins.
- `/Users/bardiasamiee/.codex/AGENTS.md:15-26` repeats greenfield refactor, polymorphism, helper, and language-baseline doctrine.
- `/Users/bardiasamiee/.codex/AGENTS.md:28-32` repeats non-trivial planning and `CLAUDE.md` plan-rule routing.
- `/Users/bardiasamiee/.codex/AGENTS.md:42-59` repeats navigation, research, and language-baseline rules.
- `/Users/bardiasamiee/.codex/AGENTS.md:65-66` says repository routes and command catalogs belong in active repo instructions, which conflicts with the duplicated global content above it.

Why this poisons context:
- It can pre-load Rasm-style expectations into every repo before the active repo has a chance to define its own skill routing, research posture, validation gates, or language version assumptions.
- It creates a priority ambiguity: a global file tells the model both to follow its defaults and to defer to local `CLAUDE.md`, increasing chance of copied policy drift.

Recommended cleanup:
- Keep only portable prompt/document hygiene and safety constraints in home `AGENTS.md`.
- Move repo-specific skill routing, language version baselines, command ownership, and planning shape into each repo owner.
- Replace file-type routing with "use the active repo's documented skill route; otherwise use built-in defaults."

## J2: Rasm Analyzer Rulecraft Lives As A Global Skill

`cs-analyzer-rulecraft` is useful, but it is not project-agnostic. It names `tools/cs-analyzer`, `docs/system-api-map`, analyzer output locking, C# 14, LanguageExt, Thinktecture, CSP-style rules, and Rasm-like production proof loops.

Evidence:
- `/Users/bardiasamiee/.codex/skills/cs-analyzer-rulecraft/SKILL.md:3-8` triggers on custom C# analyzer rules in `tools/cs-analyzer`.
- `/Users/bardiasamiee/.codex/skills/cs-analyzer-rulecraft/SKILL.md:15-21` describes project-wide propagation, C# 14, approved external libraries, and repo-documented BCL/system surfaces.
- `/Users/bardiasamiee/.codex/skills/cs-analyzer-rulecraft/SKILL.md:41-43` tells agents to use repo docs and launch broad read-only sub-agents across analyzer internals, tests, coding standards, external-lib docs, and `system-api-map`.
- `/Users/bardiasamiee/.codex/skills/cs-analyzer-rulecraft/SKILL.md:75` routes API misuse through `docs/system-api-map`.
- `/Users/bardiasamiee/.codex/skills/cs-analyzer-rulecraft/SKILL.md:117` references `tools/cs-analyzer/obj` lock behavior.

Why this poisons context:
- In another C# analyzer repo, the skill will inject Rasm's analyzer topology, docs routes, and proof habits.
- The skill claims not to couple to one project, path, namespace, or incident, but then names the project-specific analyzer path and local docs owners.

Recommended cleanup:
- Move this skill under Rasm-scoped project skills or memory, not global user skills.
- If kept global, make it generic: "the repo-declared analyzer project", "the repo-declared API/system owner", "the repo-declared analyzer proof route."
- Remove launch-packet orchestration from the reusable skill or turn it into an optional audit pattern explicitly requested by the user.

## J3: TypeScript Testing Skill Is Parametric Portal Specific

`testing-ts` has the strongest direct project leakage in the audit. It hard-codes Parametric Portal import paths, package topology, a `.claude` hook, and a triple-app E2E bootstrap.

Evidence:
- `/Users/bardiasamiee/.codex/skills/testing-ts/SKILL.md:47` says most `packages/server/` modules are unit PBT.
- `/Users/bardiasamiee/.codex/skills/testing-ts/SKILL.md:85-88` defines import order with `@parametric-portal/*`.
- `/Users/bardiasamiee/.codex/skills/testing-ts/SKILL.md:145-146` defines category routes and says E2E uses a Playwright agent pipeline.
- `/Users/bardiasamiee/.codex/skills/testing-ts/references/categories.md:136-138` defines an agent pipeline and triple-app bootstrap: `api (:4000)`, `parametric_icons (:3001)`, `test_harness (:3002)`.
- `/Users/bardiasamiee/.codex/skills/testing-ts/references/guardrails.md:16` names `.claude/hooks/validate-spec.sh`.
- `/Users/bardiasamiee/.codex/skills/testing-ts/references/guardrails.md:31` repeats `@parametric-portal/*` import order.
- `/Users/bardiasamiee/.codex/skills/testing-ts/templates/unit-pbt.spec.template.md:2` says the template targets `packages/server/` modules.
- `/Users/bardiasamiee/.codex/skills/testing-ts/templates/unit-pbt.spec.template.md:24`, `contract.spec.template.md:25-26`, and `integration.spec.template.md:26` import from `@parametric-portal/...`.

Why this poisons context:
- This skill will bias all TypeScript test work toward Parametric Portal's package names, hooks, app ports, import order, and test topology.
- The templates can directly generate wrong imports in any other repo.

Recommended cleanup:
- Move Parametric Portal examples/templates into a project-scoped skill or repo docs.
- Replace package names with neutral placeholders such as `<workspace-package>`, `<server-package>`, and `<source-import>`.
- Remove `.claude/hooks` and app bootstrap claims from the reusable skill; route to active repo test instructions.

## J4: C# Testing Skill Encodes Rasm Test Rails Globally

`testing-cs` is less direct than `testing-ts`, but it carries Rasm-like C# test ownership: active repo testkit, runtime scenario rails, docs/testing-libs, quality-router placeholders, and host-native runtime separation. This should be project-scoped or neutralized.

Evidence:
- `/Users/bardiasamiee/.codex/skills/testing-cs/SKILL.md:16` defines canonical unit rails as xUnit v3/MTP + CsCheck + active repo testkit and runtime scenarios.
- `/Users/bardiasamiee/.codex/skills/testing-cs/SKILL.md:24-32` requires reading owning source, sibling tests, active repo testkit, `docs/testing-libs`, platform/API owners, and then validating with scoped gates.
- `/Users/bardiasamiee/.codex/skills/testing-cs/SKILL.md:40-49` defines exact static spec, testkit, runtime scenario, mutation, architecture, tooling, benchmark, and fuzz locations.
- `/Users/bardiasamiee/.codex/skills/testing-cs/SKILL.md:117-126` points test-tool APIs to `docs/testing-libs/...`.
- `/Users/bardiasamiee/.codex/skills/testing-cs/SKILL.md:143-160` carries a validation ladder with `<quality-router>`, `<test-runner>`, and `<runtime-runner>`.
- `/Users/bardiasamiee/.codex/skills/testing-cs/references/rails-tooling.md:8-20` is a command catalog for static, test, coverage, mutation, and target runs.
- `/Users/bardiasamiee/.codex/skills/testing-cs/references/bridge-runtime.md:17-25` defines repo runtime scenario rules and artifact ownership.

Why this poisons context:
- A global skill should not define repo test paths, repo scenario placement, or repo quality-router behavior.
- The runtime rail language will import Rasm's Rhino/host separation style into other C# repos even when they do not have a live host scenario rail.

Recommended cleanup:
- Split reusable C# testing doctrine from Rasm rails.
- Keep generic law/oracle/test-shape guidance global.
- Move `docs/testing-libs`, quality-router, runtime scenario root, and artifact ownership into Rasm repo instructions.

## J5: Command Catalogs And Validation Ladders Are Repeated In Reusable Skills

Several skills duplicate command catalogs that should be source-owned by the active repo, tool README, or the tool's own CLI help.

Evidence:
- `/Users/bardiasamiee/.codex/skills/nx-tools/SKILL.md:20-33` lists an Nx command catalog.
- `/Users/bardiasamiee/.codex/skills/nx-tools/SKILL.md:38-58` lists `uv run $CODEX_HOME/...` invocation examples.
- `/Users/bardiasamiee/.codex/skills/testing-ts/SKILL.md:55-56` tells agents to run `pnpm exec nx test -- --coverage` and `npx stryker run`.
- `/Users/bardiasamiee/.codex/skills/testing-ts/references/validation.md:24-28` repeats `pnpm exec nx test -- --coverage` and 95 percent per-file gates.
- `/Users/bardiasamiee/.codex/skills/testing-ts/references/validation.md:58-64` repeats `npx stryker run`.
- `/Users/bardiasamiee/.codex/skills/testing-cs/references/rails-tooling.md:8-20` lists a full managed command table.
- `/Users/bardiasamiee/.codex/skills/speech/references/cli.md:3` explicitly calls itself a "command catalog" for the bundled speech CLI.

Why this poisons context:
- Reusable skills become stale command owners.
- They can override repo-specific quality routers or tool-specific help output.

Recommended cleanup:
- Keep only the high-level route in `SKILL.md`: "use the bundled CLI" or "use the repo-declared quality route."
- Put detailed command usage in CLI `--help`, source READMEs, or narrowly bundled skill references only when the skill itself owns the script.
- For repo tools, never put concrete command catalogs in global skills.

## J6: Version And "Current" Catalogs Will Drift

Multiple global skills freeze versions, dates, model names, and runtime deadlines. Some GitHub Actions deadlines are already in the past relative to this audit date, June 6, 2026. Even when accurate today, these entries invite stale advice.

Evidence:
- `/Users/bardiasamiee/.codex/skills/github-actions/references/act_usage.md:12` says actionlint current version is `1.7.10 (December 30, 2025)`.
- `/Users/bardiasamiee/.codex/skills/github-actions/references/act_usage.md:87` says act current version is `v0.2.84 (January 1, 2026)`.
- `/Users/bardiasamiee/.codex/skills/github-actions/references/modern_features.md:215-230` embeds Node runtime migration dates through Summer 2026.
- `/Users/bardiasamiee/.codex/skills/github-actions/references/supply_chain.md:210-220` repeats Node runtime current/deprecated status and current action major claims.
- `/Users/bardiasamiee/.codex/skills/speech/SKILL.md:9` and `:58` hard-code `gpt-4o-mini-tts-2025-12-15`.
- `/Users/bardiasamiee/.codex/skills/transcribe/SKILL.md:19-22` hard-codes transcription and diarization model behavior.
- `/Users/bardiasamiee/.codex/skills/.system/imagegen/references/image-api.md:15-18` and `:71-73` hard-code GPT Image model capabilities.

Why this poisons context:
- The model may treat frozen references as current truth without a live lookup.
- Current/current-version language is especially risky for OpenAI and GitHub surfaces.

Recommended cleanup:
- For volatile external products, route to current official docs or tool output at use time.
- Keep bundled version references explicitly labeled as examples or fallback snapshots, not current truth.
- Avoid "current version" wording in reusable skills unless the skill also mandates live verification before use.

## J7: Agent Orchestration And Source-Loading Meta Leak Into Skills

Reusable skills repeatedly tell the agent how many agents to launch, how E2E generation should be delegated, how to fetch manuals, or how to manage source routes. Some of this is legitimate in system skills, but it is still context-heavy and can distract from the user's task.

Evidence:
- `/Users/bardiasamiee/.codex/skills/cs-analyzer-rulecraft/SKILL.md:43` says to send many read-only sub-agents.
- `/Users/bardiasamiee/.codex/skills/cs-analyzer-rulecraft/SKILL.md:53-58` prescribes dispatching 8-10 read-only agents and a scorecard.
- `/Users/bardiasamiee/.codex/skills/cs-analyzer-rulecraft/SKILL.md:113` asks for follow-up sub-agents after propagation.
- `/Users/bardiasamiee/.codex/skills/testing-ts/references/categories.md:135-138` defines an E2E agent pipeline and app bootstrap.
- `/Users/bardiasamiee/.codex/skills/.system/openai-docs/SKILL.md:45-76` contains a long source route for Codex manual, Docs MCP, temp cache, and bounded uncertainty.
- `/Users/bardiasamiee/.codex/skills/.system/skill-creator/SKILL.md:48-55` instructs use of subagents for validation.

Why this poisons context:
- Agent orchestration meta becomes a default behavior even when the user asked for a normal implementation or review.
- It can increase context load and induce unnecessary parallel-agent framing in reusable work.

Recommended cleanup:
- Keep orchestration in task prompts when needed, not global skills.
- Convert hard counts like "8-10 agents" into optional review tactics gated by explicit user request or broad discovery tasks.
- Keep system skill source routes only where the skill's purpose is source routing.

## J8: Examples Bake In Local Paths, Files, And Packages

Several examples are not neutral. They name local root files, local package names, or project-specific targets.

Evidence:
- `/Users/bardiasamiee/.codex/skills/nx-tools/SKILL.md:45` uses `@parametric-portal/types`.
- `/Users/bardiasamiee/.codex/skills/nx-tools/SKILL.md:56` uses `CLAUDE.md` as a token-count example.
- `/Users/bardiasamiee/.codex/skills/nx-tools/SKILL.md:73-75` repeats `@parametric-portal/types`.
- `/Users/bardiasamiee/.codex/skills/testing-ts/templates/contract.spec.template.md:25-26` imports from `@parametric-portal/${PKG_A_PATH}` and `@parametric-portal/${PKG_B_PATH}`.
- `/Users/bardiasamiee/.codex/skills/testing-ts/templates/integration.spec.template.md:26` imports from `@parametric-portal/server/${SOURCE_PATH}`.
- `/Users/bardiasamiee/.codex/skills/testing-ts/templates/unit-pbt.spec.template.md:24` imports from `@parametric-portal/server/${SOURCE_PATH}`.

Why this poisons context:
- Templates are high-risk because they can generate wrong code directly.
- Local package names are not just examples; they become autocomplete anchors and import-shape defaults.

Recommended cleanup:
- Replace local package names with neutral placeholders.
- Use sample packages like `@workspace/<pkg>` or `<source-package>` only inside template placeholders.
- Avoid `CLAUDE.md` examples in global tool skills unless the skill specifically concerns Codex instruction files.

## Lower-Risk Notes

- `openai-docs` and `.system/openai-docs` necessarily include source/citation routing because their job is current OpenAI documentation. The risk is length and source-route chatter, not project leakage.
- `plugin-creator` and `skill-installer` include home paths such as `~/plugins`, `~/.agents/plugins/marketplace.json`, and `$CODEX_HOME/skills`. Those are product/workflow paths, not local repo poisoning, though they should remain clearly scoped to Codex plugin/skill creation.
- `coding-bash`, `coding-pg`, `coding-python`, and `coding-csharp` contain strong ecosystem and version doctrine. I found less direct project leakage there than in `testing-ts`, `testing-cs`, and `cs-analyzer-rulecraft`, but these skills still risk over-prescribing local preferences across unrelated repos.

## Recommended Remediation Order

1. Move `cs-analyzer-rulecraft` into Rasm-scoped skill storage or rewrite it as a generic analyzer-rule skill.
2. Rewrite `testing-ts` to remove Parametric Portal imports, `.claude` hook claims, `packages/server` routes, and triple-app bootstrap.
3. Split `testing-cs` into generic C# adversarial testing guidance plus Rasm-owned runtime/quality rails.
4. Shrink `/Users/bardiasamiee/.codex/AGENTS.md` to portable hygiene and safety only.
5. Replace time-sensitive version/model catalogs with live-doc/tool-output routing or clearly labeled fallback snapshots.
6. Remove command catalogs from global skills unless the skill owns the bundled script.

## Gaps And Residual Risk

- I did not line-by-line audit all 191 Markdown/YAML/template files under `~/.codex/skills/**`; I used pattern searches plus targeted reads of high-signal references/templates.
- I did not audit script implementation bodies for code smells, only instruction-level poisoning surfaces.
- Hidden `.system` skills were included after a second inventory pass; they are likely generated/bundled and may be less appropriate to edit directly, but they are inside the requested tree.
- No external freshness verification was performed for GitHub/OpenAI version claims; the finding is about the presence of frozen current-version guidance, not whether each claim is factually wrong today.

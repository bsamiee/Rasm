# Project Skills Findings

Source: `../final-report.md`
Scope: Project-local `.claude/skills/**` surfaces.
Finding count: 8

The finding blocks below are copied verbatim from `../final-report.md`.

## Findings

#### F-SKILL-PROJ-01: GitHub Actions skill contradicts live-resolution policy with static version/SHA catalogs
- Severity: high
- Reports: `agent-reports/09-project-claude-skills.md` F1
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/github-actions/SKILL.md:22`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/github-actions/SKILL.md:137`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/github-actions/references/version-discovery.md:76`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/github-actions/references/common_errors.md:84`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/github-actions/examples/docker-build-push.yml:26`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/github-actions/examples/dependency-review.yml:24`
- Evidence: skill says to resolve action versions live and never embed static SHAs, but references/examples include current-version tables, exact action versions, exact SHAs, Node runtime timing, and pinned example SHAs.
- Why it may poison context: agents may copy stale SHAs/majors as current truth instead of resolving actions at generation time.
- Suggested disposition: delete static current-version/SHA tables; keep resolution protocol, pinning format, and validation shape; use placeholder SHAs or mark static SHAs unusable.

#### F-SKILL-PROJ-02: Perplexity tool skill normalizes visible reasoning and `<think>` handling
- Severity: high
- Reports: `agent-reports/09-project-claude-skills.md` F2
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/perplexity-tools/SKILL.md:12`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/perplexity-tools/SKILL.md:56`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/perplexity-tools/SKILL.md:93`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/perplexity-tools/scripts/perplexity.py:11`
- Evidence: user-facing skill docs describe visible reasoning, `strip` for research/reason commands, and `<think>` tags by default; wrapper usage repeats "strip thinking".
- Why it may poison context: generated research workflows may request, preserve, or strip reasoning traces instead of treating reasoning as unavailable/private.
- Suggested disposition: remove visible chain-of-thought wording; expose only summarized answer content; make any stripping internal or reject reasoning traces at the wrapper boundary.

#### F-SKILL-PROJ-03: Project-local `testing-cs` skill is heavily Rasm-coupled while still acting reusable
- Severity: high
- Reports: `agent-reports/09-project-claude-skills.md` F3
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-cs/SKILL.md:16`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-cs/SKILL.md:40`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-cs/SKILL.md:73`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-cs/SKILL.md:117`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-cs/references/bridge-runtime.md:17`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-cs/references/rails-tooling.md:8`
- Evidence: skill embeds repo paths, `_testkit`, `_architecture`, `_tooling`, `_benchmarks`, `_fuzz`, repo scenario roots, harness variables, fact channels, retired bridge flow, runtime health command, artifact paths, and `docs/testing-libs/*` facts.
- Why it may poison context: generic C# test generation can inherit Rasm runtime/quality-router assumptions and tool behavior becomes owned by both skill and repo docs.
- Suggested disposition: split reusable testing law/oracle material from Rasm-local scenario and quality-router overlay; route command catalogs to the quality owner.

#### F-SKILL-PROJ-04: Project-local skills duplicate validation ladders and command catalogs from owners
- Severity: high
- Reports: `agent-reports/09-project-claude-skills.md` F4
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-cs/SKILL.md:143`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-cs/references/rails-tooling.md:8`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-ts/SKILL.md:48`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-ts/references/validation.md:15`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-bash/SKILL.md:161`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/github-actions/SKILL.md:155`
- Evidence: skills embed static/test/runtime commands, mutation/coverage commands, Bash validation pipelines, GitHub Actions validation pipelines, best-practice checks, and troubleshooting tables.
- Why it may poison context: stale skill validation text becomes a second owner for tool behavior and can cause false proof or over-validation.
- Suggested disposition: replace command ladders with owner routes and selection criteria; keep only invariant skill-local checks.

#### F-SKILL-PROJ-05: Skill templates/examples encode contradictory or placeholder artifact patterns
- Severity: medium
- Reports: `agent-reports/09-project-claude-skills.md` F5
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-ts/SKILL.md:166`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-ts/templates/unit-pbt.spec.template.md:25`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-ts/references/validation.md:37`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/github-actions/templates/docker-action.template.yml:72`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-bash/templates/standard.template.md:120`
- Evidence: `testing-ts` says pass schemas directly but template/reference still use `Arbitrary.make(Schema)`; action templates and Bash templates include executable placeholder/wrapper behavior.
- Why it may poison context: generated artifacts can copy contradictory or placeholder code into production output.
- Suggested disposition: fix the `testing-ts` schema contradiction and remove placeholder behavior from executable templates.

#### F-SKILL-PROJ-06: Skill-eval prompt/meta sections are embedded in runtime skill context
- Severity: medium
- Reports: `agent-reports/09-project-claude-skills.md` F6
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-bash/SKILL.md:167`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-python/SKILL.md:155`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-python/references/validation.md:93`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-ts/SKILL.md:167`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-pg/references/validation.md:176`
- Evidence: skills include explicit invocation, implicit invocation, noisy context, negative control, and compliance check examples.
- Why it may poison context: runtime skills become prompt-test transcripts and steer agents toward meta compliance.
- Suggested disposition: move eval prompts to private evaluation fixtures outside runtime skill context.

#### F-SKILL-PROJ-07: Tool skills leak local environment details into reusable surfaces
- Severity: medium
- Reports: `agent-reports/09-project-claude-skills.md` F7
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/context7-tools/SKILL.md:25`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/exa-tools/SKILL.md:12`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/perplexity-tools/SKILL.md:10`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/tavily-tools/SKILL.md:12`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/sonarcloud-tools/SKILL.md:25`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/hostinger-tools/SKILL.md:55`
- Evidence: skills mention `$CLAUDE_HOME`, `$CODEX_HOME`, 1Password API-key injection, `SONAR_TOKEN`, repo-root `sonar-project.properties`, coverage paths, and concrete VPS/firewall IDs.
- Why it may poison context: generated instructions may depend on one workstation/runtime.
- Suggested disposition: use `<skill-root>/scripts/...`, keep secret injection in local install notes, and replace concrete IDs with placeholders.

#### F-SKILL-PROJ-08: Skills preserve defensive version/caveat manuals and project/brand coupling
- Severity: medium
- Reports: `agent-reports/09-project-claude-skills.md` F8, F9
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-bash/SKILL.md:4`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-bash/references/version-features.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/github-actions/references/version-discovery.md:65`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/mermaid-diagramming/SKILL.md:99`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-python/SKILL.md:128`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-csharp/references/validation.md:91`
- Evidence: skills carry version gates/fallback chains/history, paused rollout history, beta caveats, "Noesis", CSP analyzer mappings, and project proof language.
- Why it may poison context: reusable skills generate compatibility chatter or local analyzer assumptions instead of current owner truth.
- Suggested disposition: keep only version boundaries that change generation behavior; remove project/brand/analyzer specifics from reusable layers or rename them as project overlays.

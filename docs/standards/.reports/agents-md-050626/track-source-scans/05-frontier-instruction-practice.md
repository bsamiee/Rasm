# [FRONTIER_01_INSTRUCTION_PRACTICE]

## [TRANSCRIPT]

I loaded the Rasm instruction chain for this docs research pass: `CLAUDE.md`, the root `AGENTS.md` supplied in the task, `docs/standards/README.md`, `docs/standards/AGENTS.md`, `docs/standards/agents-md.md`, ``, and `docs/standards/proof.md`. The local standard already defines `AGENTS.md` as a scoped behavioral overlay, not a README, command catalog, provider manual, session record, or research summary.

I used the current OpenAI Codex manual through the `openai-docs` skill helper on 2026-06-05. The relevant OpenAI pages were Codex best practices, prompting, custom instructions with `AGENTS.md`, and rules. I treated OpenAI's loading and precedence behavior as current provider behavior, not as a portable truth for every agent.

I checked Anthropic Claude Code documentation on 2026-06-05 for `CLAUDE.md` memory files, `AGENTS.md` interoperability, path-scoped rules, load order, imports, and the distinction between behavioral guidance and settings or hooks. I treated Claude Code behavior as a separate lane: Claude reads `CLAUDE.md`; it reads `AGENTS.md` only through import or symlink.

I checked Google Gemini documentation on 2026-06-05 for prompt design, system instructions, long-context placement, and structured output. I treated Gemini guidance as prompt and instruction-shape guidance, not repository instruction-file loading guidance.

I sampled public project examples on 2026-06-05 only where they added complementary practice: the open `AGENTS.md` format site, OpenAI Codex's own repository `AGENTS.md`, OpenClaw, and acpx. The useful examples showed concrete scoped validation, route-to-owner language, context-size discipline, and evidence requirements. They also showed patterns Rasm should avoid: broad command inventories, current-version facts without proof routes, and long root files that mix repo policy, release policy, docs policy, and implementation facts.

I did not edit active standards files. This report is written as temporary research material for a future `agents-md.md` refinement pass.

## [SOURCES]

- OpenAI, "Custom instructions with AGENTS.md." https://developers.openai.com/codex/guides/agents-md. Accessed 2026-06-05 through the current Codex manual helper.
- OpenAI, "Best practices." https://developers.openai.com/codex/learn/best-practices. Accessed 2026-06-05 through the current Codex manual helper.
- OpenAI, "Prompting." https://developers.openai.com/codex/prompting. Accessed 2026-06-05 through the current Codex manual helper.
- OpenAI, "Rules." https://developers.openai.com/codex/rules. Accessed 2026-06-05 through the current Codex manual helper.
- Anthropic, "How Claude remembers your project." https://code.claude.com/docs/en/memory. Accessed 2026-06-05.
- Anthropic, "Claude Code settings." https://code.claude.com/docs/en/settings. Accessed 2026-06-05.
- Anthropic, "Hooks reference." https://code.claude.com/docs/en/hooks. Accessed 2026-06-05.
- Anthropic, "Security." https://code.claude.com/docs/en/security. Accessed 2026-06-05.
- Google, "Prompt design strategies." https://ai.google.dev/gemini-api/docs/prompting-strategies. Accessed 2026-06-05.
- Google, "Text generation / system instructions." https://ai.google.dev/gemini-api/docs/text-generation. Accessed 2026-06-05.
- Google, "Long context." https://ai.google.dev/gemini-api/docs/long-context. Accessed 2026-06-05.
- Google, "Structured outputs." https://ai.google.dev/gemini-api/docs/structured-output. Accessed 2026-06-05.
- agentsmd, "AGENTS.md - a simple, open format for guiding coding agents." https://github.com/agentsmd/agents.md. Accessed 2026-06-05.
- OpenAI Codex repository, `AGENTS.md`. https://github.com/openai/codex/blob/main/AGENTS.md. Accessed 2026-06-05.
- OpenClaw repository, `AGENTS.md`. https://github.com/openclaw/openclaw/blob/main/AGENTS.md. Accessed 2026-06-05.
- acpx repository, `AGENTS.md`. https://github.com/openclaw/acpx/blob/main/AGENTS.md. Accessed 2026-06-05.

## [FINDINGS]

Provider behavior converges on layering, but not on one filename. Codex natively builds an `AGENTS.md` chain from global scope through project root down to the working directory, with closer instructions overriding earlier guidance and a project-doc byte budget. Claude Code natively reads `CLAUDE.md` and related Claude rule surfaces; it can share `AGENTS.md` content only through import or symlink. Gemini guidance is about prompt/system-instruction shape, output format, and long-context placement, not repo file discovery.

Instruction files are context-shaping, not mechanical enforcement. OpenAI rules, Anthropic settings, Anthropic hooks, sandboxing, and tool permissions are the stronger place for command authorization or hard blocking. `AGENTS.md` should name local proof selectors and stop conditions, but should not pretend prose can enforce safety, prevent tool use, or prove a gate passed.

The strongest official guidance favors concise, concrete, structured instructions. OpenAI says short accurate `AGENTS.md` files beat vague large ones and recommends adding durable rules after repeated mistakes. Anthropic says concise, specific, well-structured `CLAUDE.md` works best and calls out context cost. Google Gemini says direct, well-structured prompts with clear constraints perform best, with critical behavioral constraints and output format at the system instruction or beginning of the user prompt.

Long-context guidance supports Rasm's front-and-close rule. Google recommends placing large context before the specific task and anchoring the final ask with a bridge phrase. Anthropic and Codex both expose context-window pressure. For `AGENTS.md`, that means the lead must carry scope and highest-risk invariant, while long inventories should move out of the overlay.

Root-vs-folder layering should stay behavioral. Root should declare load order, first-hop nested overlay discovery, conflict rules, and owner routes. Folder overlays should declare only local deltas: what to read for that subtree, which owner rail to extend, what bad local addition to reject, and when to stop.

Future-forward standards need two separate claim modes. Normative target language can be strong: "replace older local practice with the newest viable standard." Present-tense implementation claims require proof: source path, command output, generated contract, or maintained provider documentation. This separation allows ruthless replacement posture without false current-state claims.

High-signal examples show value in trigger-driven validation and owner-specific hazards. OpenAI Codex's repository `AGENTS.md` is useful where it names concrete forbidden changes, scoped test commands, and specific context-size review rules. acpx is useful where it maps changed surfaces to validation commands and demands code-root-cause evidence for bug fixes. These are importable only as patterns, not as copied content.

The open `AGENTS.md` format is useful as a low-common-denominator entry point, but Rasm's standard should remain stricter. "README for agents" is acceptable as public positioning, but Rasm should define `AGENTS.md` as a behavioral overlay because README-like content invites duplicated manuals and stale command catalogs.

## [PATTERNS_TO_IMPORT]

- State provider load semantics only when they change author action. For Codex, that means root-to-leaf discovery, one file per directory, override precedence, empty-file skipping, and byte-budget truncation. For Claude, that means `CLAUDE.md` is native and `AGENTS.md` requires import or symlink.
- Write overlays as deltas from parent guidance. A nested `AGENTS.md` should answer what changes locally, not restate repo policy.
- Put scope, parent relation, highest-risk invariant, and route-away in the lead. Put proof gap, stop condition, or owner route at the close.
- Express local engineering posture as trigger-action-owner rules: "When changing <surface>, extend <owner rail> and prove with <local gate>." This beats slogans such as "avoid helpers" because it names the replacement path.
- Use proof selectors, not command catalogs. Name the local selector only where parent instructions cannot infer it, and route command syntax to the tool README or proof owner.
- Separate behavioral guidance from enforcement. If the action must be blocked, route to hooks, rules, settings, sandbox, CI, or a repo tool. If the behavior should be preferred, keep it in `AGENTS.md`.
- Add durable rules from observed failures. Official OpenAI and Anthropic guidance both support updating instruction files when the same correction repeats.
- Keep context cost visible. Prefer short overlays, path-local files, and route links over giant roots. Treat large command inventories, generated catalogs, and provider manuals as context debt.
- Use evidence-before-synthesis for factual work. Require source spans, paths, commands, or generated artifacts before claiming current behavior.
- Phrase future-forward posture as normative target, not as current-state fact. "Use the newest viable target when changing this subtree" is safe; "this subtree already uses X" needs proof.

## [PATTERNS_TO_REJECT]

- Reject copied provider manuals. `AGENTS.md` may name the provider behavior that changes local authoring, but detailed Codex, Claude, Gemini, or tool docs belong at their maintained source.
- Reject README-shaped overlays. Project overview, onboarding, architecture maps, API catalogs, and release policy belong in their document types unless they directly change agent behavior in the governed folder.
- Reject generic validation sections. Use only local stop conditions, local selectors, or proof hazards that parent guidance and tool docs do not already own.
- Reject current-baseline caveats. Do not weaken standards with "the current code still uses..." unless the sentence is a proof-backed present-state claim in the correct owner document.
- Reject false enforcement language. Do not say an `AGENTS.md` "prevents" a command, "guarantees" a check, or "blocks" unsafe behavior. Prose instructs; tools enforce.
- Reject broad command catalogs. A command belongs in an overlay only when the overlay owns the choice of selector or the stop condition. Otherwise route to README, tool docs, CI, or proof.
- Reject fixed sub-agent counts. Use bounded delegation needs and merge constraints when parallel research is durable; do not encode task-specific counts.
- Reject unmaintained fragile facts. Versions, package IDs, host SDK details, member lists, generated paths, and provider capability claims need a source route and freshness trigger or should stay out.
- Reject compatibility and deprecation shims as instruction defaults. If direct replacement is possible, say replacement. If compatibility is truly required, name the owning external contract and proof route.
- Reject source inventories as proof. A source list does not prove a drift-prone claim unless each claim maps to current evidence.

## [AGENTS_MD_WORDING]

Use wording like this in `agents-md.md` or Rasm overlays:

```markdown
`AGENTS.md` is a scoped behavioral overlay. It states the local delta from parent instructions: what to read, which owner rail to extend, which nearby route controls adjacent truth, and when to stop.
```

```markdown
State provider loading behavior only when it changes author action. Codex-native `AGENTS.md` discovery, Claude-native `CLAUDE.md` discovery, and Gemini prompt-shape guidance are separate claims and need current provider proof when repeated here.
```

```markdown
Root instructions route. Folder instructions specialize. A root file names load order, nested-overlay discovery, conflict rules, and cross-stack owners; a folder file names only local deltas that would be lost if the file were deleted.
```

```markdown
Write replacement posture as a target rule, not a current-state claim: "When changing this surface, replace older local practice with the newest viable standard and extend the canonical owner before adding a parallel surface."
```

```markdown
Do not soften the target because current code lags. Mark current implementation facts only where the document makes a present-tense claim, and attach source, command, generated contract, or provider-doc proof beside that claim.
```

```markdown
Pair every local rejection with the replacement owner: "Do not add another receipt list; extend the existing receipt fold and expose the needed projection." A prohibition without an owner route is too weak for an agent-facing overlay.
```

```markdown
Use proof selectors instead of command catalogs. Name a command only when this folder owns the selector or stop condition; otherwise route command syntax and expected output to the maintained tool documentation.
```

```markdown
Treat prose instructions as behavioral guidance. Route hard blocking, command allow/deny behavior, permissions, sandboxing, and CI enforcement to the tool, hook, settings, rules, or workflow surface that can actually enforce it.
```

```markdown
Promote a repeated correction only after it becomes durable local behavior. Keep one-off task prompts, critique notes, transcripts, generated mirrors, and external examples out of `AGENTS.md` unless a trusted repo route promotes the rule.
```

```markdown
Close each overlay with the binding route or stop rule: missing proof, wrong owner, unavailable runtime, unsafe permission state, or the exact source that must be read before continuing.
```

## [CONFIDENCE]

High for the core import patterns: official OpenAI, Anthropic, and Google sources align with Rasm's existing direction toward short scoped overlays, edge-positioned constraints, concrete instructions, and proof-backed provider claims.

High for the provider-boundary correction: Codex `AGENTS.md`, Claude `CLAUDE.md`, and Gemini prompt/system-instruction guidance are different surfaces and should not be collapsed into one universal load model.

Medium for community/project examples: they are useful as practice samples, but they are not normative. Their content should inform pattern wording only after filtering through Rasm's active standards.

Medium-high for the replacement/refactor wording: it preserves the ruthless target posture while separating normative future standards from present-tense implementation claims that require evidence.

---
name: agent-dispatch
description: >-
    Owns delegation craft: delegation of work — fork, subagent, agent team, workflow,
    or machine automation — and the decision-complete worker contract (territory,
    exclusions, return shape, acceptance, root discipline), result-flow topology,
    depth and fan-out budgeting, worker model tiering, and `SendMessage` resume. Use when
    "orchestrate", "parallelize" , "delegate this", "subagent or workflow?", "run in the
    background". Every operation on a chosen workflow belongs to workflow-creator; offload
    to codex and Gemini to agy; settings, and memory to harness-steering.
---

# [AGENT_DISPATCH]

Dispatch is three decisions taken in order: placement — which execution surface holds the work; contract — what the worker receives and what it returns; topology — how many workers run and how their results flow back.

Official platform facts riding any dispatch — prompting surfaces, model behavior, memory, limits, official values — resolve live, never from recall: anything Claude routes through the claudeCodeDocs MCP tools, anything OpenAI or Codex through the openaiDeveloperDocs MCP tools.

## [01]-[ROUTING]

- [01]-[SURFACES](references/surfaces.md): per-surface mechanics — permission surfacing, `SendMessage` resume guards, transcript persistence.
- [02]-[PROMPTING](references/prompting.md): delegation contract — startup context, contract fields, receipt discipline, and meta-delegation prompts.
- [03]-[TOPOLOGIES](references/topologies.md): result-flow shape mechanics, the file-ownership law, and stop-condition law.

## [02]-[PLACEMENT]

| [INDEX] | [SURFACE]          | [CONTEXT]                                  | [SELECT_WHEN]                                                         |
| :-----: | :----------------- | :----------------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | Main turn          | Full history, full tools                   | Iterative back-and-forth, phases share context, targeted change       |
|  [02]   | Fork               | Inherits history + system prompt + cache   | Side task needing accumulated context; parallel takes on a base       |
|  [03]   | Subagent           | Fresh: own prompt, memory, git snapshot    | Noisy or verbose work whose transcript stays out of the parent        |
|  [04]   | Nested subagent    | Fresh, spawned by a worker                 | A delegated task that itself splits; grandchild noise stays hidden    |
|  [05]   | Agent team         | Independent peer sessions, tasks, mailbox  | Workers must trade findings, challenge, self-claim tasks              |
|  [06]   | Dynamic workflow   | Script vars hold every intermediate result | Dozens to hundreds of agents, codified reruns, bounded loops          |
|  [07]   | Codex offload      | Separate model + context, one report       | Transcript-heavy mechanical or research legs — codex skill owns these |
|  [08]   | Machine automation | No agent; launchd, webhook rows            | Recurring, watch-shaped, or event work with no per-firing judgment    |

Placement law rides four axes: cost rises down the table, so the cheapest surface that isolates the noise wins; a fork beats a fresh subagent when the worker needs the conversation so far, because the fork reuses the parent prompt cache; a team beats parallel subagents only when workers must communicate, since each teammate is a full session priced accordingly; a workflow beats a team when the plan is codifiable and the intermediates belong in script variables instead of any context window.

Placement law is hierarchy-independent: every agent at every depth applies the same placement economics and the same model mixture. Writing and judgment run at capable tiers; recon and mechanical legs offload to the workhorse lineage; an independent perspective comes from an external lineage — codex for workhorse second reads, agy for Gemini judgment and visual legs — never a second reviewer of the same one; no agent escalates its own model tier beyond its brief. Each active repo's model table prices the tiers; the codex and agy skills own their legs once this law picks one.

Fan-out width is budgeted like depth: worker count tracks genuine task breadth — one or none for a lookup, a few for a bounded comparison, many only for a breadth-heavy sweep across disjoint territories. Delegation costs roughly an order of magnitude more tokens than a main-turn pass, so coupled work, dependent-step chains, and low-value edits stay in the main turn; a fan-out earns its cost only when territories are independent and the parallel work exceeds one window. Over-fanning a trivial or serial task is the dominant delegation spam, as wasteful as leaving noisy work undelegated.

## [03]-[CONTRACT]

A non-fork worker sees none of the parent conversation — no prior reads, no invoked skills, no history — so every delegation prompt is decision-complete: objective, territory, exclusions, output contract, and success criteria, with no reliance on mid-run clarification since `AskUserQuestion` is withheld from subagents.

COMMIT DISCIPLINE is a contract field: a worker that writes to a repo commits each completed unit in scoped, signed commits as it lands — explicit pathspecs, `[scope]: action`, `git status` before staging so a concurrent worker's hunks stay frozen, never `git add -A`/`-u`. A worker that dies mid-run then loses only its uncommitted tail, and the receipt's commit hashes hand a successor the trail to resume from. That trail RECORDS progress, never DECIDES work — a successor reads the current tree to know what remains, never the changelog.

ROOT DISCIPLINE is a contract field beside commit discipline: a worker resolves every defect it touches at the cause, and a quirk noticed beyond its stated territory is fixed in the same run — never deferred, never annotated as another worker's scope. A defect genuinely beyond reach — a contended file, a locked seam, an absent credential — travels back in the receipt as an explicit unreachable naming the owner that closes it, never a silent residual a successor must rediscover.

## [04]-[TOPOLOGY]

Result flow is chosen before the first spawn — the shape decides who consolidates, what each worker returns, and when the run stops. Topology table owns the roster and its selection criteria.

## [05]-[DEPTH]

Subagents spawn subagents to a fixed ceiling of five levels below the main conversation; at the floor the Agent tool is withheld, and a worker's depth is pinned at spawn — resuming it from a shallower context restores no headroom. Depth is spent to shed noise, never for parallelism theater: a worker earns the Agent tool only when its own task splits, and its prompt then carries an explicit split policy — what each child owns, what returns, and the remaining depth budget. A fork spawns named subagents but never another fork.

Depth ledgers are per-lineage: the five-level ceiling meters Claude spawning Claude only — an offload to the other lineage spends no Claude depth, and that lineage's own spawn bound is its skill's law (codex).

## [06]-[RUNTIME]

Every dispatch runs in the background, and this governs every tool that offers the choice — workers, shell commands, watches, builds, long reads. A holding pattern is the defect: blocking on work already running spends the turn on nothing, and polling for a result the harness announces on its own spends it twice. One exception, taken deliberately: riding along to WATCH a process — debugging a live failure, following a build for the moment it breaks, running a validation to see it stay clean — where the output as it streams is the product.

Background permission prompts surface in the main session named by requester; approval releases one call, Esc denies that call without killing the worker. Stopped workers resume through `SendMessage` with full history intact; an API-error death returns the worker's last output rather than losing the run.

## [07]-[COMPOSITION]

- Runnable workflow scripts — `meta`, `agent()`, `pipeline()`, schemas — belong to workflow-creator; this skill decides fit and agent prompting.
- Offload to codex belongs to the codex skill, which owns its models, tiers, and dispatch surfaces; the placement table names the trigger.
- Hook construction for `SubagentStart`, `SubagentStop`, `TeammateIdle`, and task gates belongs to hooks-builder; this skill names where a gate pays.
- Recurring machine work — launchd rows, the signed webhook inbox — belongs to the estate machine owner; the table names when work leaves agents.
- Memory files, rules, settings, model and effort defaults, and headless lanes belong to harness-steering; a subagent's frontmatter stays here.
- [ROUTING]: codex owns offload mechanics, workflow-creator script shapes, this skill placement and contract law, harness-steering settings and hooks.

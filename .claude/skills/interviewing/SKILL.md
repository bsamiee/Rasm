---
name: interviewing
description: >-
  Adversarial elicitation that converts unknowns into durable decision artifacts: corpus-grounded
  interviews whose questions confront the user with what exists, blindspot passes along a named
  interrogation-axis catalog, direction brainstorms and wargames, pre-merge comprehension quizzes,
  buy-in packets, and vocabulary teaching — each terminating in a schema-declared artifact
  (decision record, direction set, roadmap brief, blindspot ledger, capability entry) at its ruled
  altitude. Owns the questioning method, the axis catalog, the artifact schemas, and the
  durable-versus-ephemeral ruling; rendering a page is html-artifacts. Use when requirements are
  ambiguous or live in the user's head, before authoring any brief, spec, roadmap, or direction
  document, when a conversation must produce a durable decision or roadmap artifact, or when the
  user says "interview me", "find my blindspots", "what am I missing", "wargame this", "brainstorm
  this", "quiz me", or "teach me the domain".
---

# [INTERVIEWING]

An interview converts the gap between the request and the territory into visible rulings before work seals: the questions confront the user with what the corpus actually holds, every answer is challenged before it becomes a ruling, and every ruling lands in a schema-declared artifact at its ruled altitude. A stenographer that collects answers without grounding or challenge ships a confident wrong spec.

## [01]-[OPERATING_LOOP]

Elicitation is a hard gate before any brief, spec, roadmap, or direction document: a change that looks too simple to design still passes intent, constraints, options, and validation. The loop runs five moves; each carries its exit test.

- [GROUND]: Read the corpus along the interrogation axes before the first question; exit when the confrontation set holds verified anchors for every axis the request touches.
- [FRAME]: Classify the unknowns into the four buckets and route each to its technique; exit when every named unknown has a bucket and no bucket with signal has zero coverage.
- [INTERROGATE]: Run impact-ranked rounds under the question law; exit when two consecutive rounds change no ruling.
- [RULE]: Convert each answer into a marked ruling — fact, assumption, inference, or open — and surface every conflict as its own question; exit when no ruling contradicts another and no answer remains unclassified.
- [LAND]: Select the artifact kind by altitude, instantiate its schema, and hand off; exit when a cold read of the instance raises no new question.

Pacing is bounded on both ends: one load-bearing question at a time when answers cascade, independent questions batched, roughly ten questions per session with nonessential gaps defaulted and named as defaults in the record.

## [02]-[GROUNDING]

Questions confront the user with what is, never with an abstraction the user must imagine.

- The corpus check precedes every round: a question whose answer the corpus, the stated constraints, or the domain default already holds is a wasted round.
- The confrontation set is the grounding product: each entry carries the exact anchor (path and line), the observed fact, and its axis token. A question earns force by embedding its anchor — the user reacts to a named surface, a real weakness, a real structure.
- A corpus beyond one session's reading budget partitions into read-only investigation legs, each assigned axes and returning anchored facts; the legs fold into the confrontation set before round one.
- An unverified candidate finding never enters a question; the anchor is re-opened on disk first.

The axis catalog — each axis with its probe method, question shape, and confirming evidence — is [references/axes.md](references/axes.md), loaded before any grounding pass.

## [03]-[UNKNOWNS_ROUTING]

Every request decomposes into four knowledge buckets; the bucket selects the technique. Blindspot and brainstorm run before interview rounds — the interview covers the residuals they leave.

| [INDEX] | [BUCKET]         | [SIGNAL]                         | [TECHNIQUE]                    |
| :-----: | :--------------- | :------------------------------- | :----------------------------- |
|  [01]   | known knowns     | stated in the request            | recorded as constraints        |
|  [02]   | known unknowns   | the user names the open question | interview rounds               |
|  [03]   | unknown knowns   | recognizable only when seen      | direction fanout and prototype |
|  [04]   | unknown unknowns | unfamiliar territory or corpus   | blindspot pass along the axes  |

Two degenerate signals re-route before any decision question: a user who cannot describe the target supplies a reference, and source code outranks any screenshot or description of it; a user who cannot evaluate options enters teach-me — fanning variations at someone who cannot judge them wastes the fan.

## [04]-[MODES]

Every mode obeys the operating loop and differs in what it extracts and lands; staged procedures with per-stage exit tests are [references/modes.md](references/modes.md), loaded when a mode begins.

| [INDEX] | [MODE]           | [TRIGGER]                          | [DEFAULT_LANDING]               |
| :-----: | :--------------- | :--------------------------------- | :------------------------------ |
|  [01]   | `interview`      | requirements ambiguous or unstated | `decision-record` set           |
|  [02]   | `blindspot-pass` | unfamiliar ground or pre-seal      | `blindspot-ledger`              |
|  [03]   | `brainstorm`     | direction uncommitted              | `direction-set`                 |
|  [04]   | `quiz`           | comprehension unproven pre-merge   | scored gate, ephemeral          |
|  [05]   | `buy-in`         | acceptance needed outside the loop | packet; ruling graduates        |
|  [06]   | `teach-me`       | user cannot evaluate or phrase     | vocabulary map, binds interview |

The landing column is the default; the altitude law owns kind selection. Roadmap-altitude rulings compose a `roadmap-brief` from any mode, and `capability-entry` rows land from the capability-weakness and touch-point axes of a blindspot pass or from an interview walking a capability surface.

## [05]-[QUESTION_LAW]

The always-hot core; composition depth — option design, timeline walks, challenge protocols, batch mechanics — is [references/question-craft.md](references/question-craft.md), loaded when composing any round.

- A question earns its round only when undetermined, consequential, and ranked: not answerable from the corpus, carrying at least two options that change the work differently, and batched ahead of everything its answer invalidates. Architecture outranks interface outranks wording.
- Past specifics outrank hypotheticals: a generality is anchored to its most recent occurrence, and a future-tense answer is recorded as an assumption, never as a fact ruling.
- State the target behavior a question must extract; a prohibition phrased as a question plants the pattern it forbids.
- Options are probes: each names its cost or consequence, cites its corpus anchor where one exists, and the `(Recommended)` marker leads only where one option dominates.
- A conflict between rulings surfaces as its own question with the consequence of each resolution spelled out, never silently reconciled.
- The interviewer holds its own stance by structure: a pushback earns a concession only through a new verified fact, the user's certainty and rebuttal evidence are tested like any other claim, and a premise is checked against the corpus before questions build inside it.

## [06]-[ALTITUDE]

Every output lands at exactly one altitude; the altitude rules whether it is durable.

- [DIRECTION]: Why and where — committed and rejected directions with their kill conditions. Durable.
- [ROADMAP]: What order — outcomes sequenced by confidence horizon with promotion conditions between horizons. Durable. Horizons are confidence bands; a dated task list inside a roadmap is a plan leaking upward.
- [PLAN]: How — tasks, sequencing, mechanics. Ephemeral: a plan is consumed by its execution and never enters the artifact home.

The durability test: a future session consults the artifact as law — a standing direction, a ruled decision, a verified finding, a capability fact. Anything consumed by its own execution — transcripts, quizzes, buy-in packets, rendered interactive boards — is working material; its surviving ruling graduates to the owning durable kind and the carrier dies. Wargame scoring survives as the direction set's own wargame section; a buy-in acceptance survives as a decision record.

## [07]-[ARTIFACTS]

Five durable kinds, each a fixed schema; an instance conforms byte-structurally — heading census matches the template, zero residual slot tokens, leader tokens drawn from the template's declared vocabularies (the blindspot ledger adds the axis catalog), every entry marked fact, assumption, inference, or open where the schema carries marks. Entry ids are zero-padded ordinals scoped to the instance, optionally led by one capital kind letter (`[01]`, `[F01]`), and never reused after deletion. An instance is authored in working space named `<kind>.<scope>[.<slug>].md` — the artifact home's naming law with the rendered page's extension swapped — and enters the artifact home only through rendering. The gate enforces conformance; run it on every instance before the seal.

```bash
python scripts/check_instance.py [--json] <instance.md>...
```

| [INDEX] | [KIND]             | [CARRIES]                                   | [TEMPLATE]                                                      |
| :-----: | :----------------- | :------------------------------------------ | :-------------------------------------------------------------- |
|  [01]   | `decision-record`  | one ruling, options, consequence, lifecycle | [templates/decision-record.md](templates/decision-record.md)   |
|  [02]   | `direction-set`    | competing directions, kills, flip triggers  | [templates/direction-set.md](templates/direction-set.md)       |
|  [03]   | `roadmap-brief`    | confidence horizons, outcomes, promotions   | [templates/roadmap-brief.md](templates/roadmap-brief.md)       |
|  [04]   | `blindspot-ledger` | verified findings, blast radius, fold-backs | [templates/blindspot-ledger.md](templates/blindspot-ledger.md) |
|  [05]   | `capability-entry` | owner, edges, maturity, gaps                | [templates/capability-entry.md](templates/capability-entry.md) |

Worked accepted and rejected instances are [examples/artifacts.md](examples/artifacts.md); worked question batches are [examples/batches.md](examples/batches.md) — consult the matching set before instantiating a kind or composing a first round.

## [08]-[HANDOFF]

- The ruling record binds downstream: the authoring pass honors every ruling and re-litigates none; an entry overturned later is edited in place with its superseding ruling.
- Every terminal artifact carries its fold-back — the copyable prompt that carries its content into the next task as binding input.
- Comparative and spatial artifacts render as interactive pages through the html-artifacts type rows — direction set as brainstorm, roadmap brief as roadmap, scored boards as wargame, quiz as quiz, packet as buy-in, capability entries as capability-atlas; the schema instance stays the source of truth the page renders. The rendered page is the consultation surface — a record nobody reads rots regardless of its schema, so rendering and homing are part of landing, never decoration.
- An artifact seals only when a fresh cold read raises no new question; author satisfaction is not the exit.

## [09]-[GOTCHAS]

- Agreement is the trap on both sides: an answer mirroring the question's framing is re-anchored to a specific instance before it becomes a ruling, and a concession without a recorded forcing fact is an unforced flip that poisons every round inheriting it.
- A round without a prepared learning target is theater; prepare the three highest-value targets before opening any round.
- Survey-sized batches make the user pick defaults; a batch stays narrow enough that every option is a real tradeoff.
- Softening a finding to spare the corpus author loses the finding; the anchor states the fact and the fact is the kindness.
- An artifact that needs its transcript to be understood failed the seal; the instance carries everything the consumer needs.
- The quiz gate passes only on a perfect score; a partial pass re-teaches and re-quizzes.

## [10]-[REPO_INTEGRATION]

When the host repo declares an artifact home and naming law in its instruction chain, that law binds durable homing — in this repo `docs/atlas/` under `<kind>.<scope>[.<slug>].html`, rendered through html-artifacts. Corpus grounding routes through the host's search owners — in this repo `assay code` for structural search and LSP for symbol navigation; the portable law is verb-neutral: verify on disk before asking.

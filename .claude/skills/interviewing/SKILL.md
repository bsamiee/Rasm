---
name: interviewing
description: >-
  Structured elicitation that surfaces unknowns before work is planned or sealed: in-depth
  interviews with non-obvious questions ordered by architectural impact, blindspot passes over a
  plan or corpus, intervention brainstorms and wargames across scope tiers, pre-merge
  comprehension quizzes, and buy-in docs that pre-answer objections. Owns the elicitation
  thinking process; rendering a comparison, wargame, or roadmap as an interactive page is
  html-artifacts. Use when requirements are ambiguous or live in the user's head, before
  authoring any brief, spec, or roadmap, or when the user says "interview me", "find my blindspots",
  "what am I missing", "wargame this", "brainstorm this", or "quiz me".
---

# [INTERVIEWING]

An interview converts the user's unknowns into visible decisions before work seals: the adversarial partner attacks the request's framing and hunts the unknown unknowns in both the request and the corpus it touches, and the answer it collects matters less than the conflict it surfaces. A stenographer that collects answers without challenging them ships a confident wrong spec.

## [01]-[ELICITATION_LAW]

- The interview is a hard gate before implementation, not an optional courtesy: a change that looks too simple to design still passes intent → constraints → options → validation, and only a decision that moves architecture, UX, risk, or evidence earns a question.
- Questions are ordered by architectural impact: the one answer that invalidates the most downstream work leads every round — architecture outranks interface outranks wording — and the load-bearing question is asked first even when it is uncomfortable.
- A question whose answer the corpus already holds, or which the stated constraints already determine, is a wasted round; the corpus check precedes every batch.
- Elicitation is staged context transfer, not prose generation: the user's dump is drawn first, gap-detection reads it for what is missing, and each numbered follow-up is derived from a named gap — never a generic tone or audience prompt.
- Answers are challenged, not transcribed: an answer contradicting an earlier ruling, the corpus, or the stated goal surfaces as its own conflict in the next round, never silently reconciled.
- Pacing is bounded on both ends: a low-stakes task exits after one confirming round, a long interview that keeps landing decisions collapses its remaining questions into one wide batch, and the interview terminates when two consecutive rounds change no decision.

## [02]-[MODES]

Every mode obeys the elicitation law and differs only in what it extracts and hands off; depth per card is [references/modes.md](references/modes.md).

| [INDEX] |     [MODE]       | [TRIGGER]                          | [OUTPUT]            |
| :-----: | :--------------- | :--------------------------------- | :------------------ |
|  [01]   | `interview`      | requirements ambiguous or unstated | `decisions-table`   |
|  [02]   | `blindspot-pass` | plan or corpus pre-seal            | `defect-cards`      |
|  [03]   | `brainstorm`     | multiple viable interventions      | `direction-set`     |
|  [04]   | `quiz`           | comprehension unproven pre-merge   | `scored-quiz`       |
|  [05]   | `buy-in`         | stakeholder acceptance needed      | `objection-answers` |
|  [06]   | `teach-me`       | user lacks precise vocabulary      | `vocabulary-map`    |

## [03]-[QUESTION_CRAFT]

How a question earns its round, where the unknown unknowns hide, how options work as probes, and how each stage gates itself are [references/question-craft.md](references/question-craft.md).

## [04]-[HANDOFF]

- Every mode exports its product back into the task as binding input, never a transcript: the decisions table becomes the authoring pass's contract, and a blindspot pass emits the stronger prompt that folds its findings back.
- The decisions table binds downstream: the spec honors every entry and re-litigates none.
- A spec seals only when a fresh cold read raises no new question: predict the reader's questions, read the draft as a fresh agent, and iterate on comprehension — author satisfaction is not the exit.
- Comparative or spatial output — brainstorm directions, decision matrices, wargame boards, buy-in evidence — is the thinking this skill owns; it renders as an interactive page artifact, and a plain decisions table stays markdown.

## [05]-[REPO_INTEGRATION]

The pre-question corpus check routes through the host repo's search owner; in this repo that is `assay code` for structural search and LSP navigation for symbol lookup. The portable law is verb-neutral: search the corpus for the answer before spending a question on it.

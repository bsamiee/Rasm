# [QUESTION_CRAFT]

Composition law for every round: how a question earns its slot, how evidence is extracted instead of opinion, how settled answers are challenged, and how the record stays honest. The always-hot core rides in the skill entry; this page is the depth loaded when composing rounds.

## [01]-[WORTH_A_ROUND]

A question earns its round only when all three hold:

- Undetermined: The answer is not in the corpus, not derivable from stated constraints, not the domain default. The grounding pass has already run; a question the confrontation set answers is deleted.
- Consequential: At least two options lead to materially different work. A question with one sane answer is a statement wearing a question mark — state it and move on.
- Ranked: Among the undetermined and consequential, the question whose answer invalidates the most downstream decisions leads. Architecture outranks interface outranks wording, and the load-bearing question is asked first even when it is uncomfortable.

Prepare the three highest-value learning targets before any round opens; a round without a prepared target is theater regardless of how the conversation feels.

## [02]-[EVIDENCE_EXTRACTION]

Answers are evidence only when anchored to specifics; the interviewer repairs a drifting conversation by changing the question, never by asking the user to be more accurate.

- Ask about what happened, never about a hypothetical future: the most recent occurrence, the current workaround, what the problem cost, who was involved, what was tried and abandoned. A future-tense answer is optimism — record it as an assumption.
- Anchor every generality: "usually", "always", "we tend to" converts to "walk me through the last time." The episode walk — what happened next, what was compared, what was eliminated, what changed the priority — yields the facts the generality hid.
- Mine ideas and feature requests for their motivation: the request enters the record as the constraint or pain that produced it, never as the feature itself.
- Deflect agreement: a compliment, a "sounds right", or an answer mirroring the question's framing is re-anchored to a concrete instance before it becomes a ruling.
- Capture the user's own vocabulary verbatim where it names a concept; replacing it with the interviewer's vocabulary erases the signal that a term meant something specific.
- An answer earns force by changing the next action — a ruling that alters no artifact entry, no option ranking, and no downstream decision was a wasted round; treat it as a signal the question was not consequential.

## [03]-[CHALLENGE_PROTOCOLS]

Settled judgments get challenged by structure, not by tone; each protocol has its trigger.

- [ASSUMPTIONS_CHECK] — Trigger: a design line treated as settled. Write the line, articulate its stated and unstated premises, and for each ask why it must be true, what makes it fail, and whether the judgment survives its failure. Only premises that must be true survive as assumptions; the rest convert to open questions.
- [PROSPECTIVE_HINDSIGHT] — Trigger: a plan or direction about to seal. Declare the failure as already true — "this shipped and failed; what killed it" — and collect causes before any discussion of likelihood. Assuming the failure as fact is what makes the uncomfortable causes speakable; asking "what might go wrong" invites politeness instead.
- [DISCONFIRMATION] — Trigger: competing explanations or directions. Evaluate options by the evidence that is inconsistent with each, never by collecting support; the option that survives disconfirmation wins, and evidence consistent with every option decides nothing.
- [INVERSION] — Trigger: a stated goal. Assume the goal is met and the outcome still counts as failure; the causes name the constraints the goal statement omitted.
- [MARKED_ADVOCACY] — Trigger: one dominant view with no live alternative. Build the strongest contrary case — most vulnerable assumption, contrary evidence, neglected alternative — and mark it explicitly as the contrarian exercise so the record never confuses the challenge with the ruling.

## [04]-[OPTIONS_AND_BATCHES]

- Options are probes, not menus: each names its cost or consequence, cites its confrontation-set anchor where one exists, and the `(Recommended)` marker leads only where one option dominates. A preview carries a concrete artifact — schema shape, worked example, rendered direction — when the choice is between comparable forms.
- Free-text is the escape valve, not the plan: a question expecting "Other" was not ready to ask.
- Each question stands independent of the others' answers; a dependent question waits for the next round, and a cascade of dependents signals a one-question round.
- A batch stays narrow enough that every option is a real tradeoff; a survey-sized batch makes the user pick defaults.
- The question budget is part of the protocol: roughly ten per session, nonessential gaps defaulted with the default named in the record. When the budget nears exhaustion with decisions still landing, the remaining questions collapse into one wide batch ranked by impact.
- When the user hesitates on an open question, convert it to a closed menu of concrete options with costs; hesitation signals the question outran the user's vocabulary, not their intent.

## [05]-[STARTING_POINT]

Open by establishing the user's position, not the task's: where they are in their thinking, what they tried, what they already rejected and why. The same request from a user who explored the space for a week and one who thought of it an hour ago needs opposite interviews — depth for the first, framing for the second. Give this position to every downstream round: it decides which bucket dominates and which mode leads.

## [06]-[RECORDING]

- Every round appends to the ruling record: question, ruling, consequence, and the ruling's mark — fact (anchored evidence), assumption (must-be-true, unverified), inference (derived, named premises), open (tracked, unresolved).
- Marks are load-bearing: an assumption entering a durable artifact carries its mark and its invalidating condition; a fact carries its anchor.
- An entry overturned later is edited in place with the superseding ruling, never duplicated; the record reads as current law, not as a transcript.
- Conflicts get their own entry: the two rulings, the question that surfaced the conflict, and the resolution with its consequence.

## [07]-[EXIT_TESTS]

Each stage carries an explicit completion criterion; a stage with no criterion runs past its gap.

- Context-gathering ends when the user's dump adds no new fact, not when the first answer arrives.
- Follow-up ends when gap-detection finds no new gap in the latest answer; follow-ups are derived from named gaps, never from a generic prompt bank.
- The round loop ends when two consecutive rounds change no ruling.
- The seal test is a fresh cold read raising no new question — reader comprehension, never author satisfaction.

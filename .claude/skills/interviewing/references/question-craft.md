# [QUESTION_CRAFT]

## [01]-[WORTH_A_ROUND]

A question earns its round only when all three hold:

- Undetermined: The answer is not in the corpus, not derivable from stated constraints, not the domain default. Search first, ask second.
- Consequential: At least two options lead to materially different work. A question with one sane answer is a statement wearing a question mark — state it and move on.
- Ranked: Among the undetermined and consequential, batch the questions whose answers invalidate the most downstream decisions. Architecture before interface, interface before wording.

## [02]-[POSITIVE_FRAMING]

A rule or a question phrased as a prohibition activates the very pattern it forbids — "never ask a generic question" plants the generic question. State the target behavior instead: name the concrete decision the question must extract, the boundary it must pin, the cost each option carries. This governs the interview's own guidance and every question it emits.

## [03]-[HUNTING_UNKNOWN_UNKNOWNS]

The user answers what is asked; the unknown unknowns hide in what nobody asked. Hunt them in:

- Boundaries: Empty, maximum, concurrent access, partial failure, the second run.
- Collisions: Where the request touches an existing owner — the corpus fact the user forgot exists.
- Inversions: The user states a goal; ask what outcome they count as failure even with the goal met.
- Omissions: Lifecycle stages the request skips — migration, teardown, observation, recovery — and consumers it never names.
- Load-bearing vagueness: Every abstract noun ("clean", "flexible", "standard") hides a decision; force each into a concrete choice with named costs.

## [04]-[BATCH_MECHANICS]

- Each question stands independent of the others' answers; a dependent question waits for the next round.
- Options are probes, not menus: each names its cost or consequence, the `(Recommended)` marker leads only where one option dominates, and a preview carries a concrete artifact — mockup, schema shape, worked example — when the choice is between comparable forms.
- Free-text is the escape valve, not the plan: a question that expects "Other" was not ready to ask.
- A conflict surfaces as its own question — "X from round 2 contradicts Y from round 4; which holds?" — with the consequence of each resolution spelled out.
- Each `AskUserQuestion` batch stays narrow enough that every option is a real tradeoff; a survey-sized batch makes the user pick defaults.

## [05]-[STARTING_POINT]

Open by establishing the user's position, not the task's: where they are in their thinking, what they tried, what they already rejected and why. The same request from a user who explored the space for a week and one who thought of it an hour ago needs opposite interviews — depth for the first, framing for the second.

## [06]-[EXIT_TESTS]

Each stage carries an explicit completion criterion, or the agent stops early against the next visible step:

- Context-gathering ends when the user's dump adds no new fact, not when the first answer arrives.
- Follow-up ends when gap-detection finds no new gap in the latest answer.
- The round loop and the spec seal carry their own gates — the pacing rule and the fresh-read gate; a stage with no criterion of its own runs past its gap.

## [07]-[RECORDING]

Every round appends to a running decisions table: question, ruling, consequence. The table, not the transcript, is the interview's product; it travels to the authoring pass as binding input, and an entry overturned later is edited in place with the superseding ruling, never duplicated.

# [GENERATED]

Generators choose cases an author did not think of, and a property, model, or second implementation gives the verdict. Example tests stay beside generated ones for exact formats, cost limits, and documented special cases.

## [01]-[PROPERTIES]

[REACH]:
- Table rows follow a rule one sentence states (round trip, order, bound, conservation)
- Code has an inverse, a slower obvious implementation, a previous version, or paths that must agree (fast and scalar, operator and in-place)
- Input combinations outnumber cases a person can list

[KINDS]:

| [INDEX] | [KIND]         | [CHECK]                                             | [PASSES_WRONGLY]                                   |
| :-----: | :------------- | :-------------------------------------------------- | :------------------------------------------------- |
|  [01]   | Round trip     | Decode after encode returns its input               | Wasteful or non-canonical output an inverse undoes |
|  [02]   | Invariant      | Every output keeps a bound, order, or conserved sum | Constant or empty output that keeps it             |
|  [03]   | Algebraic law  | Identity, idempotence, absorption, commutativity    | Symmetric laws hold with one operation swapped     |
|  [04]   | Reference      | Output equals a different, simpler algorithm's      | Misconception present in each implementation       |
|  [05]   | Relation       | Related inputs (permuted, scaled, subset) relate    | Constant function                                  |
|  [06]   | Checked answer | Answer hard to compute is verified by a cheap check | Check weaker than its contract                     |

[METHOD]:
- Stack every relation one generated input can feed into one property
- Add hostile values by hand (empty, line-ending pairs, combining marks) and each past failing shape to every draw
- Print a seed on failure; runs before merge use a fixed or recoverable seed, scheduled runs explore with fresh ones and keep a failure store

[FORMS]:

| [INDEX] | [FORM]                                                                | [SIGN_OF_FAILURE]                                        |
| :-----: | :-------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | Oracle is a definition or a different algorithm with no shared helper | Expected value built from a result's own fields          |
|  [02]   | Kinds added until identity, empty, and constant implementations fail  | Every property passes for a function returning its input |
|  [03]   | Reference named in its test and tested against an outside oracle      | Untested reference, comparison in a narrower type        |
|  [04]   | Valid and related inputs constructed (subset as a difference)         | Filter relating independent draws, health check off      |
|  [05]   | Invalid shapes normalized, by a shrinker too                          | Shrunk value that breaks validity                        |
|  [06]   | Distribution shaped until its target relation is common               | Uniform draws where collisions or density decide         |
|  [07]   | Reach at a target path measured through generator statistics          | No statistics, silent return on an invalid draw          |
|  [08]   | Equal keys carry a provenance tag, combining functions are asymmetric | Merge property passing with arguments swapped            |
|  [09]   | "Different" value built by one small change to its pair               | Independent random pair assumed different                |
|  [10]   | Count drawn, sizes derived from it                                    | Shrunk failure holding large unrelated values            |
|  [11]   | Generator narrowed, with a comment, where a property is false         | Filter or weaker assertion added after a limit failure   |
|  [12]   | Counterexample pinned as a named example, its class raised in draws   | Fix with no named test and no generator change           |
|  [13]   | Comment states where a property does not hold                         | Check weakened under an old name, self-comparison        |
|  [14]   | Each property fails for a stated fault                                | Purity check on a pure function, layout copied from code |

Row 06 shapes are overlapping keys, small alphabets, and sizes on each side of an internal threshold.

## [02]-[MODELS]

[REACH]:
- Component holds state across calls (cache, store, queue, repository, state machine) and order of operations decides results
- Concurrent structure has a sequential specification

[KINDS]:

| [INDEX] | [KIND]                   | [CHECK]                                                            |
| :-----: | :----------------------- | :----------------------------------------------------------------- |
|  [01]   | Model comparison         | Each command runs on system and model, states compared per command |
|  [02]   | Invariant fold           | Pure transition function folded over a command log, no model       |
|  [03]   | Sequential specification | Interleavings explored against a single-threaded class             |

[METHOD]:
- Print the shrunk command sequence on failure
- List unsupported states as skips with a reason

[FORMS]:

| [INDEX] | [FORM]                                                                    | [SIGN_OF_FAILURE]                                    |
| :-----: | :------------------------------------------------------------------------ | :--------------------------------------------------- |
|  [01]   | Model is a standard-library structure (map for a cache, list for a queue) | Model with an index or cache of its own              |
|  [02]   | Commands generated from model state and gated by preconditions            | Reads and deletes hitting missing keys alone         |
|  [03]   | Invariants relate fields and run after every command                      | Check on final state, or on types and signs alone    |
|  [04]   | Conserved quantity counted along more than one path                       | Total trusted from one counter                       |
|  [05]   | Model has an invariant check of its own                                   | Flaky trace through harness state, fix in test alone |
|  [06]   | Legal nondeterminism written as a named allowance                         | Comparison loosened for every entry, input edited    |
|  [07]   | Commands weighted to build state, random subset disabled per run          | Every run drawing one mix, rare paths never hit      |
|  [08]   | Interleavings explored with few threads and operations                    | Bounds lowered in CI to finish                       |

Row 06 allowance for a cache: an evicted entry can be absent, and equals its model value when present.

## [03]-[FUZZ_TARGETS]

[REACH]:
- Code parses, decodes, or deserializes input it does not control
- Code holds dense internal assertions a random input can trip
- Rewrite has its previous implementation available

[KINDS]:

| [INDEX] | [KIND]           | [INPUT]                                                    |
| :-----: | :--------------- | :--------------------------------------------------------- |
|  [01]   | Byte input       | Raw bytes handed to a parser                               |
|  [02]   | Structured input | Well-formed programs or command sequences from a generator |
|  [03]   | Differential     | One input run through a second or previous implementation  |

[METHOD]:
- Run with assertions enabled, internal assertions check state no output shows

[FORMS]:

| [INDEX] | [FORM]                                                               | [SIGN_OF_FAILURE]                                      |
| :-----: | :------------------------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | Target checks more than a crash: round trip, model, or reference     | Body that only calls a parser                          |
|  [02]   | Round trip compared on parsed values                                 | Failure from escaping that differs in text alone       |
|  [03]   | Valid values built from every input                                  | Inputs ending at a first validation check              |
|  [04]   | Fixed seeds produce every outcome class, or runs print a histogram   | Target green, fast, and never failed                   |
|  [05]   | One seed is a run's whole input, clock and scheduling drawn from it  | One seed giving different results                      |
|  [06]   | Differential outcomes classed equal, failed alike, or not comparable | Early return on any error from one side                |
|  [07]   | Allowed differences listed by name with their specification          | Tolerance with no reason, unmeasured skip rate         |
|  [08]   | Generator constrained toward comparable inputs                       | Comparator loosened for a difference a setting removes |
|  [09]   | Deliberate fault applied after an oracle is relaxed                  | Relaxed check that has not failed since                |
|  [10]   | Finding committed as a corpus file or minimized example              | Fix recording a seed alone                             |

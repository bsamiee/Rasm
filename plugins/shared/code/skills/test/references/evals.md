# [EVALS]

Components with a model inside (prompt, skill, agent instruction, tool description) give different output per run. Verdicts are rates over repeated runs against a baseline.

## [01]-[COMPONENT_EVALS]

[REACH]:
- Prompt, skill, agent instruction, tool description, or model is added or changed
- Claim is made that a component improves results

[KINDS]:

| [INDEX] | [GRADER]      | [CHECK]                                                                    |
| :-----: | :------------ | :------------------------------------------------------------------------- |
|  [01]   | Deterministic | Pattern in output, file present, tool called, result file of a run command |
|  [02]   | Hidden tests  | Suite or seeded fault a run never saw                                      |
|  [03]   | Judge         | Model verdict on criteria, blind to which arm produced an output           |

[METHOD]:
- Write cases from realistic requests before component text grows
- For a Claude Code plugin or skill, `claude plugin eval <path>` runs each case 3 times per arm by default and prints each arm's score and their difference

[FORMS]:

| [INDEX] | [FORM]                                                               | [SIGN_OF_FAILURE]                                       |
| :-----: | :------------------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | Each case runs with and without its component                        | Pass rate with no baseline, instruction assumed to help |
|  [02]   | Revision's baseline is its previous version                          | Revision compared with nothing                          |
|  [03]   | Each arm repeated, with mean, spread, and difference reported        | Verdict from one run                                    |
|  [04]   | Assertion fails in a baseline arm                                    | Assertion green with and without a component            |
|  [05]   | Deterministic grader preferred, judge accepted where a signal agrees | Judge alone decides                                     |
|  [06]   | Judge reads output without its author's claims of correctness        | Judge shown an author's summary                         |
|  [07]   | Grader detecting component use kept out of both scores               | Difference inflated by a use check                      |
|  [08]   | Transcripts read for component use, results split by it              | Average over runs that never loaded a component         |
|  [09]   | Control of equal length and unrelated content                        | Distraction and content reported as one effect          |
|  [10]   | Triggering tested with near-miss negatives on a held-out split       | Description tuned on requests that score it             |
|  [11]   | Tokens and time reported beside quality                              | Quality gain reported with no cost                      |

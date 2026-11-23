# [GRADING]

Grades rest on one mechanical fact per test: a named break in production code turned it red. Readings of a test show its form, and a judged verdict counts where a run agrees with it.

## [01]-[FAULT_INJECTION]

[REACH]:
- Test was written for existing code, or has never been red
- Tests are about to be refactored, deleted, or graded
- Oracle was relaxed against a false positive

[KINDS]:

| [INDEX] | [KIND]       | [BREAK]                               |
| :-----: | :----------- | :------------------------------------ |
|  [01]   | Named break  | One realistic edit to production code |
|  [02]   | Reverted fix | Bug fix reverted with its tests kept  |
|  [03]   | Linter       | No break, test smells a parser finds  |

[METHOD]:
1. Run a test linter for assertion-free tests, conditional assertions, self-comparisons, skipped or focused tests, and fixed waits
2. Name a break before running anything: wrong limit (`>` for `>=`), wrong branch, dropped effect, swapped arguments, removed guard
3. Apply it to production code and run tests claiming that behavior, each must turn red for that reason; an imagined break proves nothing
4. Restore code and see green

[FORMS]:

| [INDEX] | [FORM]                                                                     | [SIGN_OF_FAILURE]                              |
| :-----: | :------------------------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | Red comes from an assertion on behavior                                    | Red from a setup or compile error              |
|  [02]   | Uncaught break answered by a limit asserted from each side at an interface | Test asserting a broken line's internals       |
|  [03]   | Expected values checked against their source apart from caught breaks      | Breaks caught by tests copied from faulty code |

## [02]-[CRITIQUE]

[REACH]:
- Own tests are finished and about to be reported
- Another author's tests are under review

[KINDS]:

| [INDEX] | [JUDGEMENT] | [EVIDENCE]                                                                        |
| :-----: | :---------- | :-------------------------------------------------------------------------------- |
|  [01]   | Oracle      | Source named for each expected value: specification line, worked value, reference |
|  [02]   | Sensitivity | Break applied and red run recorded                                                |
|  [03]   | Specificity | List of what a test touches, or a green run after a rewrite of internals          |
|  [04]   | Signal      | Uncontrolled inputs read, runs repeated in shuffled order, text of a red run      |
|  [05]   | Partitions  | Map from each partition and error path of a source to its test                    |
|  [06]   | Cost        | Run time, and unrelated changes that forced an edit                               |

[METHOD]:
1. Judge in table order, and rewrite or delete a test failing oracle or sensitivity before reading later rows
2. Write one line per test: behavior, oracle source, break and result, findings
3. Close with partitions that have no test and judgements left unverified

[FORMS]:

| [INDEX] | [FORM]                                                 | [SIGN_OF_FAILURE]                                      |
| :-----: | :----------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | Finding cites a run or a line                          | Finding argued from style                              |
|  [02]   | Oracle evidence comes from a behavior's source         | Expected value accepted for matching what code returns |
|  [03]   | Author's report and rationale read as claims           | Finding lowered by a stated reason                     |
|  [04]   | Findings reported per test with evidence               | Score, or uniform pass with no break applied           |
|  [05]   | Unverified judgement carries a command that settles it | Reading of a test reported as a verdict                |

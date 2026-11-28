---
name: test
description: "Use when writing tests for new or existing code, fixing a failing or flaky test, refactoring tests or code that tests cover, or reviewing and grading tests, in any language, covering test-first order, oracle and case choice, test structure, independence from the code's author, doubles, generated inputs, snapshots, and fault injection"
---

# [CODE_TEST]

Tests are detectors: each answers whether one behavior is correct, turns red when it is wrong, and stays green when it is right, whatever happens to code structure. Reading an implementation and asserting what it does is the cheapest way to a passing test, and that test agrees with every fault in code it was copied from. Work starts at a behavior's source of truth, chooses what decides pass or fail before test code exists, and ends when each test was seen red for a named reason.

[REFERENCES]:
- [01]-[GENERATED](references/generated.md): Tests with generated inputs (properties, models, fuzz targets)
- [02]-[BOUNDARIES](references/boundaries.md): Tests of code that calls a database, service, or third-party package, and every test double
- [03]-[OUTPUTS](references/outputs.md): Snapshots, characterization of code with no specification, and end-to-end journeys
- [04]-[GRADING](references/grading.md): Fault injection and critique of existing tests
- [05]-[EVALS](references/evals.md): Evals of components with a model inside

## [01]-[DETECTOR]

Passing tests prove only that they ran. Lines count as checked when some assertion would fail if their result changed. Each property below is lost in a way a reader can see:

| [INDEX] | [PROPERTY]  | [HOLDS_WHEN]                                     | [LOST_WHEN]                                                         |
| :-----: | :---------- | :----------------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | Sensitivity | Wrong behavior turns test red                    | Expected value has no source outside code, or no assertion runs     |
|  [02]   | Specificity | Correct rewrite of internals leaves test green   | Test reads call order, private members, or incidental wording       |
|  [03]   | Signal      | Same code, same verdict, failure names its input | Verdict varies between runs, or failure hides which input broke     |
|  [04]   | Cost        | Test runs inside edit loop                       | Suite is skipped while editing, unrelated changes force a test edit |

Lost sensitivity shows no symptom, every test below passes on every run:
- Tautological: expected value recomputed by code's own formula, built by its helper, read from a result's own fields, or pasted from a run
- Vacuous: no assertion executes (empty loop, uncalled callback, promise not awaited, assertion under a condition, swallowed error, shadowed name)
- Weak: assertion on non-null, truthy, type, length, or absence of an error, which many wrong outputs satisfy
- Echo: asserted value is one a stub was told to return, with no work by code under test between setup and assertion
- Blind negative: negative case that passes for any rejection, or with a predicate false whether or not code is correct
- Unreached: setup never routes into the case a test's name claims
- Unlabelled pin: expected value recorded from a run and presented as specification

Lost specificity and signal have names:
- Change detector: a test that fails only when someone changes a decision on purpose (a constant, exact wording), never when behavior breaks
- Coupled: a test that turns red after a correct rewrite of internals, with return values, reachable state, outside effects, and errors unchanged
- Flaky: a test with a verdict that changes between runs of unchanged code, which teaches readers to disregard red

## [02]-[SEQUENCE]

Task decides where work enters:

| [INDEX] | [TASK]                      | [ENTRY]                                                  |
| :-----: | :-------------------------- | :------------------------------------------------------- |
|  [01]   | New behavior                | Steps in order, each test before its implementation      |
|  [02]   | Bug fix                     | Test red on bug before any fix, kept afterward as guard  |
|  [03]   | Existing code, source known | Steps in order, red at step 8 from a break in production |
|  [04]   | Existing code, no source    | Characterization before any change                       |
|  [05]   | Red test                    | Cause explained from source before any edit              |
|  [06]   | Flaky test                  | Uncontrolled input found and fixed where it enters       |
|  [07]   | Review or grade             | Fault injection, then critique                           |

1. Source: read specification, issue, documentation, worked example, or user's statement of behavior before reading or writing implementation
2. Contract: state each behavior as precondition, action, and postcondition at an interface callers use, and ask the user about undecided cases
3. Worth: test behavior that holds a decision, and skip code with none, facts a type checker proves, dependency internals, and covered behavior
4. Scope: choose the lowest scope where behavior is observable and keep every collaborator inside it real
5. Oracle: choose what decides pass or fail from `[03]` before test code exists
6. Cases: list partitions and boundaries from `[04]`, one case per partition
7. Test: write one test in `[05]` form and name the production change that turns it red, a test with none is rebuilt around observable behavior
8. Red: run it and read its failure, which must come from the missing or broken behavior it names
9. Green: write the least code that passes and leave the test as written
10. Next: return to step 7 with one more case, chosen from what the last cycle showed
11. Form: once cases share a shape, raise them to a `[06]` form
12. Proof: run affected tests while editing, whole suite at end, fault injection on changed code, then the application's own entry point

Scope is drawn around one behavior, at the point a caller can observe it, and can span more than one class or function. Use a double only where code meets something slow, nondeterministic, or outside its process. When one fault fails many tests, narrow scope to locate it and keep collaborators real.

Tests that pass before their implementation exists check behavior that was already there, or check nothing: rewrite them until they fail. When a test fails after step 9, decide from its source whether code or test is wrong, then fix code, or rewrite the test for a contract that changed on purpose. Expected values change only when their source changes.

Tests written in bulk before any code describe imagined behavior and lock in structure before anything was learned. Each test is a tracer bullet: its result shows where to aim the next one.

Step 12 runs the application because suites written beside their code can pass while the product fails.

Report each test with the source of its expected value and the break that turned it red.

## [03]-[ORACLE]

Oracles decide pass or fail. Left unchosen, an oracle defaults to what code returned when run, which can only confirm. Use the strongest row available:

| [INDEX] | [ORACLE]        | [FORM]                                                   |
| :-----: | :-------------- | :------------------------------------------------------- |
|  [01]   | Specified value | Literal from specification or hand calculation           |
|  [02]   | Reference       | Trusted implementation by a different, simpler algorithm |
|  [03]   | Property        | Fact every correct output has                            |
|  [04]   | Relation        | Fact linking outputs of related inputs                   |
|  [05]   | Approved output | Output a person read against its input and approved      |
|  [06]   | No crash        | Absence of failure, when no other row applies            |

- Write expected values in a form unlike the computation under test: a literal in another notation, a hand-worked value, a specification vector
- Give inputs distinct, non-default, asymmetric values, a reused, swapped, reversed, or echoed input then changes the result
- With no specified value, take a weaker independent row before recorded output

## [04]-[CASES]

Happy path is one partition. Faults sit at limits, empty inputs, and error paths:

| [INDEX] | [FORM]                                                                | [SIGN_OF_FAILURE]                                      |
| :-----: | :-------------------------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | Each case names its partition                                         | Cases differ in incidental values alone                |
|  [02]   | Cases on each side of every documented limit                          | Cases mid-partition, or one side of a limit            |
|  [03]   | Zero, one, and many, each form of empty its own case                  | One "empty" case for null, empty, missing, and blank   |
|  [04]   | Each sub-condition decides one case's outcome alone                   | Compound condition met by all-true and all-false cases |
|  [05]   | Negative case names its rejection: error kind, message key, or branch | Deleting its guard leaves test green                   |
|  [06]   | New code path adds rows to its behavior's table                       | Optimization merged with no row routed through it      |
|  [07]   | Setup asserts it reached its partition                                | Empty result equals empty expectation                  |
|  [08]   | Known-wrong behavior asserted under a comment naming its issue        | Skip on a test for known-bad behavior                  |

Assert message text only when code under test owns it and users read it.

## [05]-[TESTS]

Each test reads as a specification of one behavior: arrange, act once, assert, with its distinguishing input, action, and expected outcome visible in its body:

| [INDEX] | [FORM]                                                    | [SIGN_OF_FAILURE]                                              |
| :-----: | :-------------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | Name states behavior and condition                        | Name states mechanism, an issue number, or a counter           |
|  [02]   | One behavior, as many assertions as it needs              | Failure leaves open which behavior or input broke              |
|  [03]   | Assertions and read-back through an interface callers use | Direct store query, production member only tests call          |
|  [04]   | Failure prints input from case data, then got and want    | Row index, hand-typed label, `expected true`                   |
|  [05]   | Logic enumerates inputs or derives cases from an oracle   | Loop or branch recomputes expected value or picks an assertion |
|  [06]   | Clock, random source, locale, zone, order, network fixed  | Retry, raised threshold, or skip added after a flaky run       |
|  [07]   | Unordered results compared as sets or sorted              | Assertion on iteration order no contract states                |
|  [08]   | Fake blocks until released, test waits for idle tasks     | Sleep or poll loop deciding task order                         |
|  [09]   | Added test raises runner's test count                     | Count unchanged, "not applicable" reported as a pass           |
|  [10]   | Short arrangement                                         | Long arrangement from a wide scope or a missing builder        |

Assert on internals only for a property no interface exposes, in a separate file that states why. Waiting for idle tasks lets a test assert absence exactly, which a poll with a timeout cannot.

## [06]-[STRUCTURE]

Remove repetition by raising the form of a test, in table order. State mechanics once and keep every case written out with its data visible. A helper extracted to shorten a test leaves its repetition in place and moves input or expected value out of sight:

| [INDEX] | [REPETITION]                   | [FORM]                                     | [CONDITION]                         |
| :-----: | :----------------------------- | :----------------------------------------- | :---------------------------------- |
|  [01]   | Tests alike except for data    | Table or parameterized test                | Rows share action and assertion     |
|  [02]   | Rows following a rule          | Property                                   | One sentence states its rule        |
|  [03]   | Construction of valid objects  | Builder with defaults                      | No default decides an outcome       |
|  [04]   | Environment setup              | Runner's fixture, lifecycle, or fake clock | Fixture returns a value             |
|  [05]   | Assertion lines on one concept | Domain assertion or matcher                | Message names concept or law broken |

- Split cases that differ in kind into separate tables or tests, a mode column turns into branches inside the loop
- Read installed runner's documentation and use its features before writing a helper: parametrization, subtests, fixtures, matchers with diffs, traces
- Start builder defaults empty and let each test opt in to what decides its result

Keep a helper that performs action and assertion together when every condition holds:
- Distinguishing input and expected value are literal arguments at each call site
- One helper serves one kind of expected outcome, with no boolean or mode parameter
- Failures are attributed to its calling test's line
- Failure output beats a generic assertion's: a diff, a named law, or its input
- Arguments that pass trivially are refused (one object given as each side of an equality check)

Remove a helper that takes a flag, reads fixture fields in place of parameters, returns a boolean, or must be opened to learn which input ran.

## [07]-[INDEPENDENCE]

Code and test written from one mental model share its mistakes. Each test needs one row below to be independent of its code's author, and rows 01 to 05 need no second agent:

| [INDEX] | [DIFFERENT]     | [OBTAINED_BY]                                                  |
| :-----: | :-------------- | :------------------------------------------------------------- |
|  [01]   | Source of truth | Cases and expected values fixed at steps 1 to 6                |
|  [02]   | Time            | Test red before its implementation exists                      |
|  [03]   | Mechanism       | Generated inputs under a property, relation, or reference      |
|  [04]   | Mechanism       | Applied break as proof a test can fail                         |
|  [05]   | Mechanism       | Comparison with a trusted implementation or a previous version |
|  [06]   | Author          | Contract below                                                 |

Delegating to another agent adds a different author only when the request is the contract below. Prompts written for the occasion carry their writer's reading of implementation, choice of cases, and expected values, and come back confirming them:

```xml
<request>{{REQUEST}}</request>
<behavior_source>
{{BEHAVIOR_SOURCE}}
</behavior_source>
<public_interface>
{{PUBLIC_INTERFACE}}
</public_interface>
<tests>{{TEST_PATH}}</tests>
<run>{{TEST_COMMAND}}</run>
<rules>
Sender wrote or read the implementation. Sender's expected values, case list, and reasoning are withheld on purpose: a verdict that shares them shares their mistakes.
Derive every behavior, case, and expected value from behavior_source. Where behavior_source leaves a case open, report it as unspecified and take no answer from code.
When request is author: write tests at tests through public_interface before opening any implementation file, then run them. Leave a test that fails against the implementation unchanged, leave the implementation unchanged, and report the disagreement.
When request is critique: read tests at tests. For each test, name one production change that must turn it red, apply it, run the test, record red or green, and restore the code. Compare each expected value with behavior_source, never with what the code returns.
</rules>
<report>
One line per test: name, behavior, source of expected value, break applied, result under that break.
Then: behaviors in behavior_source with no test, unspecified cases, disagreements between tests and implementation.
</report>
```

- Send: fill slots and send the block whole to a fresh agent, adding no text outside slot values; a fork inherits its sender's conversation
- `{{REQUEST}}`: `author` or `critique`
- `{{BEHAVIOR_SOURCE}}`: specification text or paths, copied whole, with no paraphrase and no line derived from implementation
- `{{PUBLIC_INTERFACE}}`: signatures, commands, or endpoints, without bodies
- `{{TEST_PATH}}`, `{{TEST_COMMAND}}`: where tests are written or read, and their run command
- Absent source: code with no source to copy has no independent author, and takes characterization
- Return: reports are claims, rerun returned tests and one break per test before accepting
- Disagreement: a returned test red against implementation is settled from its source, an edit that turns it green discards its independence

## [08]-[REFACTORING]

Each step changes either production code or tests, and the side left alone checks the side that changed:

| [INDEX] | [CHANGED]       | [FIXED]         | [PROOF]                                                 |
| :-----: | :-------------- | :-------------- | :------------------------------------------------------ |
|  [01]   | Production code | Tests           | Covering tests green per step, whole suite green at end |
|  [02]   | Tests           | Production code | Every break caught before is caught after               |

Production code under tests:
- Before a first edit, apply breaks to code about to change and add a test for each break nothing catches
- Tests red under a behavior-preserving change show changed behavior or coupling, rewrite a coupled test at an interface in its own step
- When an interface changes on purpose, rewrite its tests for the new shape in a separate step
- Use `code:rebuild` for production rebuilds

Tests:
- Before a first edit, run fault injection over production files under test and record breaks caught
- After editing, rerun each recorded break
- Comparison proves only the breaks that were tried, choose breaks at each limit and branch a test names
- Keep expected values independent of code and keep every case that existed before
- Delete a test that repeats a partition, asserts nothing, tests structure, or guards a removed symbol's absence, once breaks caught stay unchanged

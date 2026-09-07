---
name: work-planning
description: Use in plan mode to define intended behavior, implementation ownership, dependencies, evidence, and completion criteria.
---

# [WORK_PLANNING]

Plan work so a fresh session can implement it from the recorded intent, repository context, and decisions. Assign disjoint ownership and order steps by their dependencies, including shared-file changes. Use `work-execution` for running the plan.

[REFERENCES]:
- [ORCHESTRATED_RUN](references/orchestrated-run.md): Ownership and dependencies for coordinated execution

## [01]-[READS]

Read each file in scope whole and the context needed to understand its consumers before defining its change. Use the language tooling for source navigation and `clean-prose` for guidance review. Record measurements only when they establish a requested correction, with the source revision or working diff they describe.

Resolve harness and tool facts through their documentation and configured public operations. Reproduce disputed behavior within the task's testing scope. Record deciding evidence and distinguish files to change from files read for context.

## [02]-[QUESTIONS]

Resolve routine choices from the user's instructions, repository evidence, and owning tool contracts. Ask for missing intent or a consequential choice the available context cannot settle, using the session's question mechanism. Continue independent research while an answer is pending.

Record the user's decisions without changing their meaning. State assumptions with the steps they affect, and update them when later evidence changes the design.

## [03]-[COMPANION_SET]

Keep the plan in the location the active harness or user supplies, within its plan-mode write permissions. Add companion files when supporting detail or independently maintained work records need a separate location. Keep smaller plans self-contained. Preserve existing companion names when execution already refers to them, and make their paths available to a fresh session.

| [INDEX] | [FILE] | [PURPOSE] |
| :-----: | :----- | :----- |
| [01] | `STATUS.md` | Track implementation steps and completion |
| [02] | `SYSTEM.md` | Explain file responsibilities and dependencies |
| [03] | `FINDINGS.md` | Preserve decisions, evidence, and open questions |
| [04] | `CHANGES.md` | Specify each change and its completion check |
| [05] | `REVIEW.md` | Record findings and their resolution |

Use the records the plan needs:
- The status record states the intent, implementation constraints, completion checks, and any system record's location
- The status record groups the files in scope by system with new files marked, then lists the steps in order, each one line with its change id
- The system record draws how the files reach each other as a diagram when a list cannot show the relation
- The findings record holds the open questions in its last section
- Record review disagreements with the affected plan item, deciding evidence, and correction
- Use stable step identifiers when records or owners refer to them, and state each change by its intended behavior and destination
- Give each change its reason and the public check that establishes completion within the requested testing scope
- Retain rejected changes only when their reason explains a design constraint or a decision a later agent needs

## [04]-[PLAN_FILE]

Organize the plan by the information a fresh agent needs. Combine sections when they repeat the same context:

| [INDEX] | [SECTION]         | [HOLDS]                                                                                   |
| :-----: | :---------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | Intro             | What a fresh agent needs before the plan                                                  |
|  [02]   | Context           | The system as it is and the intent                                                        |
|  [03]   | Established facts | Every fact the design rests on, each with its source                                      |
|  [04]   | Design            | Systems, files, and the decisions needed for implementation           |
|  [05]   | Build order       | Pre-dispatch steps for the shared files, steps per system with a proof each, dependencies |
|  [06]   | Structure         | The systems and their files, the files the main agent owns, the companion directory       |
|  [07]   | Verification      | Requested checks, expected results, and completion conditions                            |

The intro tells a fresh agent:
- It holds none of the planner's inferred understanding, and it reads the named files whole, the smallest set that supplies the understanding
- The intent in the user's words
- The implementation constraints and required behavior to preserve, with coherent rewrites allowed when structure causes the problem
- Correct wrong file names, API shapes, locations, or claims against the intended behavior and repository evidence
- Record corrected plan details with their deciding evidence and notify affected owners
- Known dependencies and missing inputs, with the operation or decision that resolves each
- Missing packages go into the catalog, missing settings into their owning file, wrong rules into the rule, in the same run

State shared standards and checks once, and reference them from briefs by file kind. Distinguish lasting repository content from temporary planning records. For each requested runtime check, name its input, expected result, and public command.

## [05]-[ADVERSARIAL_PLANNER]

Assign an independent reviewer the plan, relevant files, user decisions, and evidence. Have it challenge assumptions, dependency order, ownership, and completion checks. Keep plan edits with an assigned owner and resolve findings before closing.

The reviewer sends questions it cannot settle to the planner. Resolve them from existing decisions and evidence before asking the user for missing information. Read the final plan against the intended result and the review corrections.

## [06]-[CLOSE]

The plan closes when each check over the plan file and the companion files holds:
- Existing paths resolve, and planned files are marked with their creation step and owner
- Each step has its intended change, owner, dependencies, and requested completion check, consistent across any companion records
- Every decision on the user's behalf is stated with its step
- The intended result and applicable user standards are recorded for execution without the planner's private context

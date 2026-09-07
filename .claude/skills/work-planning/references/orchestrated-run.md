# [ORCHESTRATED_RUN]

When the plan runs by orchestration, by `main` or by one orchestrator per system, the plan's structure carries the decisions an orchestrator otherwise re-derives, and every brief points at a plan section. Use `work-execution` for the messaging mechanics and the brief templates.

## [01]-[SYSTEMS]

Each system is sized for one orchestrator to read whole and carries its own steps, checks, and done-when, and two systems that name one file between them split at the file or hand it to `main`.

## [02]-[FILE_OWNERSHIP]

The plan's Structure section places every file in scope under one system or under `main`:
- `main` owns the manifests, the locks, and the shared references that more than one system edits
- The plan names each `main` file with the reason it sits outside the systems

## [03]-[SHARED_FILES]

Changes to a `main` file land before the systems start or from a message, and the plan states which for each:
- Changes known at planning time are pre-dispatch steps in the Build order, proven before the first dispatch
- Changes a system discovers mid-run land from its orchestrator's message to `main`
- The plan names each changes record entry that lands from a message

## [04]-[DEPENDENCIES]

The plan states the cross-system dependencies as the complete list, and systems with no listed dependency wait for nothing:
- Reports that start a system
- Steps that read another system's output
- Steps that wait for a sibling's report

## [05]-[MESSAGING_PATH]

The plan's Structure section states the messaging path once, with the orchestrators named, as a destination per kind of finding:

| [INDEX] | [FINDING]                                          | [DESTINATION]                                                       |
| :-----: | :------------------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | Outside an agent's step                            | Its orchestrator                                                    |
|  [02]   | Outside an orchestrator's system                   | `main`, with the sibling system named, relayed to its orchestrator  |
|  [03]   | In a file outside every system                     | `main`, which lands it in the file it owns                          |
|  [04]   | Plan correction that touches a sibling's work      | `main`, in the same round                                           |
|  [05]   | Question for the user                              | `main`, with the options the sender sees                            |

## [06]-[REPORT_CONTRACT]

Every orchestrator reports under the contract `work-execution` names, and the plan states the bound in lines and that a report pastes no file content.

## [07]-[COMPANION_ADJUSTMENTS]

The companion files gain the columns and sections that let each orchestrator write its own part of a shared file:
- Steps in the status record carry an owner column (`main` or the system letter) and a state column the run fills
- The changes record opens with the pre-dispatch section and holds one section per system
- The findings record holds the pre-dispatch findings first and one section per system
- `main` fills the review frame at the close

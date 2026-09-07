# [ORCHESTRATED_RUN]

Record the decisions each coordinator needs to implement its assigned system, and link briefs to the relevant plan sections. Use `work-execution` for dispatch and coordination.

## [01]-[SYSTEMS]

Group work into independently owned systems with their intended result, steps, checks, and completion criteria. Add a coordinator when the system has implementation decisions or dependencies to manage.

## [02]-[FILE_OWNERSHIP]

Assign each file or shared configuration field to one active owner. Record shared manifests, locks, and references with their owner and affected systems. Keep shared edits with the main agent or an assigned specialist, and separate ownership before concurrent work begins.

## [03]-[SHARED_FILES]

Place known shared-file changes before the steps that consume them, with the requested checks. Independent work can start without waiting for an unrelated shared edit.

Send newly discovered shared changes to the assigned owner with the evidence, proposed correction, and affected steps. Record the resulting plan correction and confirm the implemented change before updating consumers.

## [04]-[DEPENDENCIES]

Name each cross-system dependency by its producer, required output or decision, consumer, and completion evidence. Reports start dependent work only when they supply a required fact. Run independent steps concurrently, and update the plan when implementation exposes another dependency.

## [05]-[MESSAGING_PATH]

Name the coordinators and route findings by their affected ownership:

| [INDEX] | [FINDING] | [DESTINATION] |
| :-----: | :----- | :----- |
| [01] | Outside an agent's step | Its coordinator |
| [02] | Shared-file correction | Assigned owner and affected coordinators |
| [03] | Unassigned file | Main agent for ownership assignment |
| [04] | Cross-system plan correction | Main agent and affected coordinators |
| [05] | Unresolved user intent | Planning session |

## [06]-[REPORT_CONTRACT]

Require the evidence that lets a consumer proceed: implemented behavior, changed contracts, completed checks, and unresolved dependencies. Link to the files or results that establish the claim. Include source excerpts only when they explain a decision the recipient needs.

## [07]-[COMPANION_ADJUSTMENTS]

When the plan uses companion records, keep their identifiers and ownership consistent with the plan. Track each step's owner and completion state, and place shared prerequisites before their consumers. Record findings beside the affected system or shared operation.

Assign shared records to an owner or disjoint sections before concurrent updates. Record review findings and their resolution when review occurs, without creating empty report frames.

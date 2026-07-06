# [CONSTRUCTION]

Per-type construction binds here — the question a type answers, what its marks assert, and the characteristic failure modes and truth tests that type carries. Type selection and the general soundness audit stay methodology's property.

## [01]-[FLOWCHART]

- Question: what exists and how same-level parts relate under one relation.
- Node: an owner or operation at one abstraction level.
- Edge: the single declared relation, its direction load-bearing.
- Method:
  1. Fix the relation semantic first.
  2. Place the dominant rail from start to outcome.
  3. Attach branches off discriminators only.
  4. Class nodes by semantic role.
  5. Label every non-obvious edge with the relation verb.
- Failure modes:
  - the god-flowchart absorbing lifecycle, sequence, or schema payloads.
  - control flow drawn beside data flow, both unlabeled.
  - decorative arrowheads on a symmetric relation.
  - branch arms that never rejoin, leaking the merge.
- Logic checks:
  - every path from entry reaches a terminal.
  - each discriminator's out-labels are exhaustive and disjoint.
  - no edge contradicts the subject's declared direction law, a dependency edge pointing one way throughout.

## [02]-[SEQUENCE]

- Question: who exchanges what, in what order, to complete one scenario.
- Participant: an autonomous runtime actor — process, service, or thread — never a data object.
- Message: one protocol step its sender can actually initiate.
- Method:
  1. Pick one scenario, happy path plus at most one fault split.
  2. Order participants left-to-right by first touch.
  3. Write messages as verb phrases carrying the payload name.
  4. Add activation only where lifetime matters.
  5. Close on the terminal response or fault.
- Failure modes:
  - the god-participant sending and receiving everything, hiding decomposition.
  - response arrows omitted, leaving causality unverifiable.
  - `alt` or `par` blocks nesting past two levels, the signal to split the scenario.
  - data objects promoted to participants.
- Logic checks:
  - every request expecting a reply carries its dotted return.
  - no message departs a participant destroyed or never created.
  - the message order is achievable, no reply preceding its request.

## [03]-[STATE]

- Question: which modes an entity occupies and which events move it.
- State: a mode the system rests in, observable between events.
- Transition: an event plus optional guard; guards leaving one state stay disjoint.
- Method:
  1. Enumerate resting modes first, rejecting any activity.
  2. Fix entry `[*]` and every terminal.
  3. Add one transition per event with its guard.
  4. Nest a composite only where substates share every external transition.
  5. Name transitions by event, never by target state.
- Failure modes:
  - an activity drawn as a state, a step mistaken for a mode.
  - overlapping guards making a transition nondeterministic.
  - composite nesting used for visual grouping over shared-transition semantics.
  - a missing terminal, so the lifecycle never ends.
- Logic checks:
  - every non-terminal state holds an exit.
  - guards leaving one state are mutually exclusive and cover the event's domain.
  - each composite's substates genuinely share its external transitions.

## [04]-[CLASS]

- Question: how declared types relate under inheritance, composition, or dependency.
- Class: a real declared type in the system.
- Relation: a compile-time relationship the source proves.
- Method:
  1. Select the type cluster answering the question, never the whole codebase.
  2. Choose which relation kinds appear, two at most.
  3. Carry only members that discriminate the design.
  4. Group by namespace when the ownership boundary is the point.
- Failure modes:
  - member dumps of every field and method drowning the relations.
  - inheritance and dependency arrows mixed without visual distinction.
  - interface lollipops attached to everything.
  - speculative types with no source anchor.
- Logic checks:
  - every drawn relation verifies in source.
  - no cycle among the inheritance arrows.
  - cardinality appears only where the association is the point.

## [05]-[ER]

- Question: what data entities exist, how they identify each other, and what multiplicity constrains them.
- Entity: a persisted noun with identity.
- Relationship: a join path whose cardinality storage enforces.
- Method:
  1. Start from the aggregate root.
  2. Add entities one join away until the question closes.
  3. Mark PK and FK on every drawn entity.
  4. Set cardinality from the constraint storage enforces, never intended usage.
  5. Name relations with the owning verb.
- Failure modes:
  - value objects promoted to entities.
  - cardinality drawn from hope — the code allows many, the diagram says one.
  - an FK attribute shown without its relationship edge, or the edge without the FK.
  - attribute dumps past the identifying and discriminating columns.
- Logic checks:
  - every FK attribute has a matching relationship edge and the reverse.
  - every entity carries a PK.
  - cardinality matches the schema constraint.

## [06]-[ARCHITECTURE]

- Question: which deployable units run where and reach each other over what path.
- Service: a deployable unit.
- Edge: network reachability over a real port or protocol.
- Failure modes:
  - logical components drawn as services.
- Logic checks:
  - every edge traces a real port or protocol path.

## [07]-[C4]

- Question: what the system landscape holds at one audience's zoom level.
- Element: a context, container, or component fixed at one level per view.
- Failure modes:
  - levels mixed inside one view.
- Logic checks:
  - every element resolves into the next zoom level.

## [08]-[GANTT]

- Question: which owned work commits to which dates, and what waits on what.
- Task: owned work with real start and end dates.
- Dependency: an `after` chain onto a prior task.
- Failure modes:
  - dependency-free bars that decorate rather than commit.
- Logic checks:
  - every `after` chain matches a real dependency.

## [09]-[TIMELINE]

- Question: what occurred across successive periods.
- Event: a dated occurrence under its period; adjacency asserts no cause.
- Failure modes:
  - causality implied by mere adjacency.
- Logic checks:
  - the period sequence runs strictly chronological.

## [10]-[MINDMAP]

- Question: how one root concept decomposes into an owned hierarchy.
- Node: a child owned by exactly one parent, the tree admitting no explicit edge.
- Failure modes:
  - a needed cross-link, proving the subject a graph rather than a tree.
- Logic checks:
  - every node traces one path back to the root.

## [11]-[KANBAN]

- Question: which workflow stage holds which work right now.
- Column: a work queue.
- Card: one task carrying its stage.
- Failure modes:
  - columns that are statuses rather than queues.
- Logic checks:
  - every card sits in exactly one column.

## [12]-[REQUIREMENT]

- Question: which requirements trace to which satisfying or verifying elements.
- Requirement: a demanded capability node.
- Element: a design or artifact node bound by `satisfies`, `verifies`, or `traces`.
- Failure modes:
  - a requirement orphaned from any satisfying or verifying element.
- Logic checks:
  - every requirement reaches at least one element via `satisfies` or `verifies`.

## [13]-[GITGRAPH]

- Question: how branches diverge, commit, and merge across repository history.
- Commit: one node on a branch.
- Branch: a named line joined by parentage or merge.
- Failure modes:
  - fabricated history matching no real repo state.
  - a branch merged or checked out before it exists.
- Logic checks:
  - every commit id stays unique and every branch predates its checkout.

## [14]-[QUADRANT]

- Question: where items place across two independent judgments.
- Point: an item positioned by two coordinates.
- Axis: one independent judgment bounded low to high.
- Failure modes:
  - axes that are not independent, collapsing the plane to a line.
  - points placed by narrative rather than measured coordinates.
- Logic checks:
  - every coordinate falls within its declared axis range.

## [15]-[PACKET]

- Question: how bits lay out across a wire-format field contract.
- Field: a named bit range in the packet.
- Failure modes:
  - field ranges that gap or overlap the wire contract.
- Logic checks:
  - field ranges match the wire spec exactly, no gaps or overlaps.

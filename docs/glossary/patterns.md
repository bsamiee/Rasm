# [PATTERNS_GLOSSARY]

Shape vocabulary and replicated-state vocabulary bind every branch alike, so one concept under three language spellings stays one concept.

## [01]-[SHAPE]

- `rail`: carries a computation's success and failure channels as one typed value, so domain logic returns outcomes rather than throwing.
- `receipt`: records how one computation resolved — route, status, sampling, solver, host evidence — as typed fields a consumer reads instead of re-deriving.
    - [NOT]: purchase and delivery receipts; only computation evidence carries this word.
- `fold`: reduces a structure to one value through a single owner, replacing accumulation across call sites.
- `arm`: handles one case of a closed family inside a dispatch, and adding a case breaks every dispatch site loudly.
    - [NOT]: ARM instruction sets; match-branch ownership earns the word.
- `case`: names one member of a closed vocabulary a discriminant selects.
    - [NOT]: test cases and switch labels in the generic sense; closed-family membership earns the word.
- `row`: carries one instance of a settled concept as data, and new capability lands as another row before any new surface appears.
- `entry`: names the one polymorphic entrypoint folding modality, arity, tenancy, topology, and provider off the request shape.
    - [NOT]: dictionary entries and log entries; only the folded public entrypoint carries this word.
- `axis`: names one dimension of deployment or policy variation whose value arrives as data at the composition root.
- `policy row`: carries one settled configuration decision as a data row a dispatch reads, never as a call-site branch.
- `posture`: names one published call form of an operator — pipe-subject or direct — both minted from one dual definition.
    - [NOT]: security and risk postures; only call form carries this word.
- `projection`: maps a source shape onto a derived shape a consumer reads, minted at the source and never stored twice.
    - [NOT]: CQRS read-model projection, one instance rather than this definition, and geometric projection, which the drawing owners spell in full.
- `statechart`: declares a hierarchical transition system as data — nodes, guards, ordered transitions — whose macrostep is a fold over that declaration.

## [02]-[BOUNDARY]

- `port`: declares an abstract capability a consumer requires, satisfied by binding at the composition root.
- `adapter`: translates one foreign surface's vocabulary into a port's vocabulary at the boundary that port declares.
- `anti-corruption layer`: isolates a foreign model behind a translation boundary so its vocabulary never leaks inward.
    - [NOT]: no surface wears this name; boundary adapters at declared ports carry the concern.
- `composition root`: binds every port, host edge, and cross-branch peer in one leaf place no lower owner reaches.
- `capability descriptor`: carries one open-axis value as a data row the supplying branch shapes, so the axis grows without re-anchoring its roster.

## [03]-[ALGEBRA]

- `idempotence`: holds where applying one operation twice yields the same state as applying it once.
- `commutativity`: holds where two operations reach one result under either application order.
- `associativity`: holds where regrouping a chain of operations leaves the result unchanged.
- `convergence`: reaches one common state across replicas that observed the same operation set.
    - [NOT]: numerical convergence toward a solver tolerance, which the compute owners spell with residual and tolerance terms.
- `strong eventual consistency`: guarantees that replicas observing the same updates hold identical state without coordination.

## [04]-[REPLICATED_STATE]

- `CRDT`: replicates a data type whose merge is commutative, associative, and idempotent, so concurrent edits converge with no consensus.
- `CmRDT`: replicates operations, requiring reliable causal broadcast so commutative application converges at every replica.
- `CvRDT`: replicates whole states, merging through a semilattice join that needs no delivery guarantee.
- `delta-state CRDT`: ships join-irreducible state fragments instead of whole states, merging each fragment through the same join.
- `LWW`: resolves concurrent writes by picking the highest timestamp, discarding the loser rather than merging it.
- `tombstone`: marks a removal as a retained entry so a later merge cannot resurrect the removed value.
    - [NOT]: storage-engine deletion markers alone; retention against resurrection earns the word.

## [05]-[LOGICAL_TIME]

- `Lamport clock`: counts one monotonic logical tick per event, ordering causally related events and leaving concurrent ones untotaled.
- `version vector`: tracks one counter per replica, and comparing two vectors answers before, after, equal, or concurrent.
- `hybrid logical clock`: stamps events with physical time bounded by logical counters, so stamps stay close to wall time and still order causally.
- `causal delivery`: holds an arriving update until every update it depends on has been delivered.
- `frontier`: marks the lower bound of times still in flight, so anything below it is settled.
    - [NOT]: exploration frontiers in graph search; only the settled-time boundary carries this word.
- `watermark`: asserts that no further input below one time arrives, licensing a window to close.
    - [NOT]: document overlay stamps, which the artifacts owners spell as an overlay operation.

## [06]-[DURABLE_HISTORY]

- `event sourcing`: persists every state change as an append-only event, and current state is a fold over that history.
    - [NOT]: `EventSource` diagnostic emitters and DOM event sources, neither of which persists anything.
- `domain event`: records one fact that happened in the domain, named in past tense and never carrying a command's intent.
- `aggregate`: bounds one consistency unit whose invariants hold on every write through its single root.
    - [NOT]: SQL aggregate functions and collection aggregation; only the consistency boundary carries this word.
- `read model`: shapes stored data for one query's shape, rebuilt from the write side and never written to directly.
- `CQRS`: splits the write path's model from the read path's model, so each shape answers to its own load.
- `journal`: appends durable entries in commit order, and replay from any point reconstructs the state that follows.
- `op log`: records each operation as a replayable entry carrying the identity and causal metadata a merge needs.
- `snapshot`: captures one materialized state at a point so replay starts there rather than at the beginning.
    - [NOT]: virtual-machine and volume snapshots; only a replay-shortening state capture carries this word.
- `upcasting`: rewrites an older persisted entry into the current shape at read time, so replay never reads a retired shape.
- `compaction`: collapses superseded history into the surviving state, bounded below by the retention frontier.
    - [NOT]: mesh decimation and archive compression; only history collapse carries this word.
- `outbox pattern`: writes an outbound message into the same transaction as its state change, and a relay ships it afterward.
- `lease`: grants time-bounded exclusive claim over a resource, expiring on its own rather than on a release call.
    - [NOT]: rental agreements; expiry without a release call earns the word.

## [07]-[DERIVED_VIEWS]

- `differential dataflow`: recomputes a dataflow incrementally from input deltas, and each operator emits only its own output deltas.
- `incremental view maintenance`: updates a materialized view from the change set alone rather than re-running its query.
- `as-of query`: reads state as it stood at one declared time rather than at the newest commit.
    - [NOT]: time travel, naming the same capability in a spelling this corpus never uses.

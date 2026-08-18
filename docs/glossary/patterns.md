# [PATTERNS_GLOSSARY]

Shape vocabulary and replicated-state vocabulary bind every branch alike, so one concept under every language spelling stays one concept.

## [01]-[SHAPE]

- `rail`: Carries a computation's success and failure channels as one typed value, so domain logic returns outcomes rather than throwing.
- `receipt`: Records how a computation resolved — route, status, sampling, solver, host evidence — as typed fields consumers read, never re-derive.
    - [NOT]: Purchase and delivery receipts; only computation evidence carries this word.
- `fold`: Reduces a structure to one value through a single owner, replacing accumulation across call sites.
- `arm`: Handles one case of a closed family inside a dispatch, and adding a case breaks every dispatch site loudly.
    - [NOT]: ARM instruction sets; match-branch ownership earns the word.
- `case`: Names one member of a closed vocabulary a discriminant selects.
    - [NOT]: Test cases and switch labels in the generic sense; closed-family membership earns the word.
- `row`: Carries one instance of a settled concept as data, and new capability lands as another row before any new surface appears.
- `entry`: Names the one polymorphic entrypoint folding modality, arity, tenancy, topology, and provider off the request shape.
    - [NOT]: Dictionary entries and log entries; only the folded public entrypoint carries this word.
- `axis`: Names one dimension of deployment or policy variation whose value arrives as data at the composition root.
- `policy row`: Carries one settled configuration decision as a data row a dispatch reads, never as a call-site branch.
- `posture`: Names one published call form of an operator — pipe-subject or direct — both minted from one dual definition.
    - [NOT]: Security and risk postures; only call form carries this word.
- `projection`: Maps a source shape onto a derived shape a consumer reads, minted at the source and never stored twice.
    - [NOT]: CQRS read-model projection, one instance rather than this definition, and geometric projection, which the drawing owners spell in full.
- `statechart`: Declares a hierarchical transition system as data — nodes, guards, ordered transitions — whose macrostep folds that declaration.

## [02]-[BOUNDARY]

- `port`: Declares an abstract capability a consumer requires, satisfied by binding at the composition root.
- `adapter`: Translates one foreign surface's vocabulary into a port's vocabulary at the boundary that port declares.
- `anti-corruption layer`: Isolates a foreign model behind a translation boundary so its vocabulary never leaks inward.
    - [NOT]: No surface wears this name; boundary adapters at declared ports carry the concern.
- `composition root`: Binds every port, host edge, and cross-branch peer in one leaf place no lower owner reaches.
- `capability descriptor`: Carries one open-axis value as a data row the supplying branch shapes, so the axis grows without re-anchoring its roster.

## [03]-[ALGEBRA]

- `idempotence`: Holds where applying one operation twice yields the same state as applying it once.
- `commutativity`: Holds where two operations reach one result under either application order.
- `associativity`: Holds where regrouping a chain of operations leaves the result unchanged.
- `convergence`: Reaches one common state across replicas that observed the same operation set.
    - [NOT]: Numerical convergence toward a solver tolerance, which the compute owners spell with residual and tolerance terms.
- `strong eventual consistency`: Guarantees that replicas observing the same updates hold identical state without coordination.

## [04]-[REPLICATED_STATE]

- `CRDT`: Replicates a data type whose merge is commutative, associative, and idempotent, so concurrent edits converge with no consensus.
- `CmRDT`: Replicates operations, requiring reliable causal broadcast so commutative application converges at every replica.
- `CvRDT`: Replicates whole states, merging through a semilattice join that needs no delivery guarantee.
- `delta-state CRDT`: Ships join-irreducible state fragments instead of whole states, merging each fragment through the same join.
- `LWW`: Resolves concurrent writes by picking the highest timestamp, discarding the loser rather than merging it.
- `tombstone`: Marks a removal as a retained entry so a later merge cannot resurrect the removed value.
    - [NOT]: Storage-engine deletion markers alone; retention against resurrection earns the word.

## [05]-[LOGICAL_TIME]

- `Lamport clock`: Counts one monotonic logical tick per event, ordering causally related events and leaving concurrent ones untotaled.
- `version vector`: Tracks one counter per replica, and comparing two vectors answers before, after, equal, or concurrent.
- `hybrid logical clock`: Stamps events with physical time bounded by logical counters, so stamps stay close to wall time and still order causally.
- `causal delivery`: Holds an arriving update until every update it depends on has been delivered.
- `frontier`: Marks the lower bound of times still in flight, so anything below it is settled.
    - [NOT]: Exploration frontiers in graph search; only the settled-time boundary carries this word.
- `watermark`: Asserts that no further input below one time arrives, licensing a window to close.
    - [NOT]: Document overlay stamps, which the artifacts owners spell as an overlay operation.

## [06]-[DURABLE_HISTORY]

- `event sourcing`: Persists every state change as an append-only event, and current state is a fold over that history.
    - [NOT]: `EventSource` diagnostic emitters and DOM event sources, neither of which persists anything.
- `domain event`: Records one fact that happened in the domain, named in past tense and never carrying a command's intent.
- `aggregate`: Bounds one consistency unit whose invariants hold on every write through its single root.
    - [NOT]: SQL aggregate functions and collection aggregation; only the consistency boundary carries this word.
- `read model`: Shapes stored data for one query's shape, rebuilt from the write side and never written to directly.
- `CQRS`: Splits the write path's model from the read path's model, so each shape answers to its own load.
- `journal`: Appends durable entries in commit order, and replay from any point reconstructs the state that follows.
- `op log`: Records each operation as a replayable entry carrying the identity and causal metadata a merge needs.
- `snapshot`: Captures one materialized state at a point so replay starts there rather than at the beginning.
    - [NOT]: Virtual-machine and volume snapshots; only a replay-shortening state capture carries this word.
- `upcasting`: Rewrites an older persisted entry into the current shape at read time, so replay never reads a retired shape.
- `compaction`: Collapses superseded history into the surviving state, bounded below by the retention frontier.
    - [NOT]: Mesh decimation and archive compression; only history collapse carries this word.
- `outbox pattern`: Writes an outbound message into the same transaction as its state change, and a relay ships it afterward.
- `lease`: Grants time-bounded exclusive claim over a resource, expiring on its own rather than on a release call.
    - [NOT]: Rental agreements; expiry without a release call earns the word.

## [07]-[DERIVED_VIEWS]

- `differential dataflow`: Recomputes a dataflow incrementally from input deltas, and each operator emits only its own output deltas.
- `incremental view maintenance`: Updates a materialized view from the change set alone rather than re-running its query.
- `as-of query`: Reads state as it stood at one declared time rather than at the newest commit.
    - [NOT]: Time travel, which refuses bare use, so each qualified spelling takes its own entry while this term names one read.

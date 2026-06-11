# Schedule Policy Algebra

[CURVES_ARE_PROJECTIONS]:
- The named backoff curves are projections of one primary generator, not parallel cases: the constant curve is an index-blind map over the infinite stream, the exponential curve is the indexed map `seed * factor^i` over the same stream, and any custom curve is one indexed map — never a new case record. Only the recurrence-bearing curves (linear, fibonacci) are bespoke stateful iterators, because a pure index map cannot express a term that depends on the prior term; the recurrences are the named exception to the derivation.
- The policy type carries the functor/monad surface directly — map, indexed map, filter, bind, and query-expression forms — so policy composes, slices, and merges entirely in the duration domain before any effect touches it. Slicing is value-level, and attempt count is stream truncation, never a counter threaded through the effect.
- The policy algebra is a semigroup with no identity: there is no empty policy, and the never/forever values are convenience generators, not algebraic units. A fold over a set of policies must seed with a real schedule, not an assumed zero.
- Time-driven schedules emit zero-gap elements while a wall-clock condition holds: a zero duration is a "go now" signal — the absence of a gap — structurally distinct from stream exhaustion, which is the loop's exit. Conflating the two mistimes the final attempt; the zero element continues the loop immediately, the stream end terminates it.

[BOUNDING_LAW]:
- Bounding an infinite backoff requires a truncating operator: an attempt-count transformer, a cumulative-delay gate, or intersection with a finite stream. Union runs to the longer operand by design, so unioning anything finite onto an infinite curve does not bound it — the merge that tightens magnitudes is exactly the merge that cannot terminate the stream.
- The cumulative gate bounds applied wall-delay, not attempt count; the per-step cap and the cumulative gate stack into "never wait longer than X per gap, never more than Y in total" as two composed transformers. The driver's zero-prepend means a backoff curve always describes the gaps between attempts, never a delay before the first try.

```csharp
static readonly Schedule Backoff =
    (Schedule.recurs(6)
   | Schedule.exponential(50 * ms)
   | Schedule.maxDelay(2 * seconds)
   | Schedule.decorrelate(factor: 0.2, seed: 7)
   | Schedule.maxCumulativeDelay(30 * seconds))
   & Schedule.spaced(100 * ms);
```

- The composed value reads as one policy traveling as data: transformer operands apply in pipeline order through the operator chain, and the final intersection floors every gap at the constant stream's magnitude while inheriting the left side's bounded length. The seeded perturbation transformers are the determinism seam — pinning the seed makes a jittered schedule reproducible under test — and the de-correlating transformer exists for many parallel clients de-phasing their retry storms, at the deliberate cost of roughly doubling the stream.

[DRIVE_POLARITY]:
- Exhaustion polarity is the load-bearing difference between the two drivers: retry threads the last error (seeded with the empty error) and stream exhaustion is the final failure surfaced to the caller; repeat threads the last value and exhaustion is success. The predicate variants compose with the iterator walk, not with the schedule — the schedule bounds how many attempts and how spaced, the predicate bounds whether the observed value or error warrants another step, and whichever fires first halts.
- The resource contract of iteration is fixed by which scope each driver re-enters per step: repeat reclaims an iteration's resources at the end of every iteration including the successful last one, so a repeated computation can never acquire-and-return a live resource — the value escapes but its backing is already released. Retry reclaims only failed attempts' resources, so attempts never accumulate acquisitions and the successful attempt's resources survive to the caller's scope.
- The inversion trap: interposing an unconditional resource scope between an acquisition and its retry driver discards the successful attempt's resource before the driver observes success. A resource-producing effect that must survive its retry loop is handed to the retry combinator directly — the driver supplies the correct fail-only scope itself.

[TIMED_AGGREGATION]:
- The fold driver reuses the same iterator walk but threads an accumulator instead of discarding the bound value: the schedule governs both cadence and termination, so an unbounded schedule yields a timed aggregator bounded only by the predicate, and a bounded schedule yields a fixed-window sampler — cadence policy as a value, with no timer and no mutable accumulator.
- Schedule and predicate are orthogonal stop conditions joined by short-circuit: an infinite schedule plus a state predicate is a converge-until-stable loop whose only bound is the data, and the recursion is trampolined inside the interpreter, so an unbounded policy drives unbounded iteration without stack growth — which is exactly why an infinite backoff on the rail must be intersected with a bound rather than relied on to stop itself.
- In a converge loop over an atomically advanced cell, the two intersected operands carry independent duties: the spaced floor is a contention throttle — it bounds the minimum interval between compare-and-swap attempts so cell contention never degenerates into a busy-spin — while the attempt bound is the safety net against an advance that never stabilizes. Neither substitutes for the other; the intersection is the policy. This duty split is the mining seed for the planned concurrency page's iteration law.

# [ITERATORS]

## [01]-[PIPELINES]

Each adapter states one transformation, and a pipeline reads top to bottom as the order of work:

| [INDEX] | [STEP]                                 | [FORM]                                          |
| :-----: | :------------------------------------- | :---------------------------------------------- |
|  [01]   | Read each window of an iterator        | `map_windows`                                   |
|  [02]   | Merge adjacent items a rule joins      | `coalesce`                                      |
|  [03]   | Keep items through the first rejected  | `take_while_inclusive`                          |
|  [04]   | Join keys across a sorted pair         | `merge_join_by`                                 |
|  [05]   | Merge many sorted sequences            | `kmerge_by`                                     |
|  [06]   | Keep the `k` least by a key            | `k_smallest_relaxed_by_key`                     |
|  [07]   | Take the one item a filter leaves      | `exactly_one`, `collect_array::<N>` for `N`     |
|  [08]   | Chain iterables passed as arguments    | `iter::chain`                                   |
|  [09]   | Group items into `[T; N]` and back     | `array_chunks`, `as_chunks`, `Vec::into_chunks` |
|  [10]   | Split tuples into a collection each    | `collect::<(Vec<_>, Vec<_>)>()`                 |
|  [11]   | Collect at least one item or none      | `try_into_nonempty_iter`                        |
|  [12]   | Join items into text                   | `format`                                        |
|  [13]   | Build a `Vec` from each index          | `Vec::from_fn`                                  |
|  [14]   | Join arms yielding different iterators | `Either`, `iter::once` for a single item        |
|  [15]   | Apply one step to a fixed set          | `[left, right].map`, `try_map`                  |
|  [16]   | Pair each item with every later one    | `array_combinations::<2>`                       |
|  [17]   | Walk iterables in lockstep             | `izip!` yielding one flat tuple                 |
|  [18]   | Number items in a target integer type  | `iter::zip(0_u32.., items)`                     |
|  [19]   | Treat the first or last item apart     | `with_position`                                 |

Gaps in a sorted series fill in one flat expression:

```rust
pub fn missing(readings: &[Reading], interval: Interval) -> impl Iterator<Item = Timestamp> {
    let step = interval.into_inner();
    readings.array_windows().flat_map(move |[before, after]| {
        iter::successors(Some(before.at), move |at| at.checked_add(step).ok())
            .skip(1)
            .take_while(move |at| *at < after.at)
    })
}
```

`array_windows` borrows each `[before, after]` window of a slice. `map_windows` passes each `&[T; N]` window of an iterator from its own buffer and clones no item. `iter::successors` retains the current timestamp when `checked_add` cannot produce its successor, and `take_while` excludes `after`. `Interval` enforces a positive `Span`; nested closures copy the extracted span. `coalesce` joins adjacent items and feeds the merged item into the next comparison, ranges sorted by start joining overlaps in one pass. `merge_join_by` walks a sorted pair once and yields `Left`, `Right`, or `Both` per key without a separate set. `exactly_one` returns an error that iterates the items it read when none or more than one remains. `array_chunks` groups an iterator without a collected `Vec`; borrowed slices take `as_chunks` or `as_chunks_mut` with the remainder handled by the domain. `Vec::into_chunks` regroups owned storage, can adjust capacity, and drops an incomplete tail; use another form when the domain retains or rejects that remainder. `as_flattened` reverses fixed-array grouping.

Named adapters retain their ordering and consumption contracts; `fold` expresses reductions no member supplies. Replacing `fold(seed, combine)` with `reduce(combine)` requires an identity seed and equivalent empty-input, evaluation, and failure behavior. Reusing a first owned buffer can remove work, while adapter shape alone proves no allocation saving. Conditional shared work can use a local `LazyCell`, per-item work a `OnceLock` beside its item; separate indexed storage stays where a foreign bound requires it.

Selection follows the input's ownership and the consumer's ordering contract. `k_smallest_relaxed_by_key` buffers at most `2k` items and returns the least `k` in `O(n + k log k)`, with `k` within its supported size range. An existing mutable slice takes `partial_sort_unstable`, `_by`, or `_by_key` under `slice_partial_sort_unstable` to sort only the requested range in place at that complexity with no allocation. These unstable operations can reorder equal items; partial sorting can also reorder items outside its range. A stable sorted prefix keeps equal-key identity and order, so replacement needs a deterministic total comparator or a consumer that treats ties equivalently.

## [02]-[GROUPING]

Grouping takes the form its consumer reads. Keyed groups over an unordered input fold per key without building the groups, and runs over an ordered input are borrowed chunks:

```rust
#[must_use]
pub fn latest(readings: &[Reading]) -> HashMap<SensorId, &Reading> { readings.iter().into_grouping_map_by(|reading| reading.sensor).max_by_key(|_, reading| reading.at) }

pub fn sessions(events: &[Event], gap: SignedDuration) -> impl Iterator<Item = &[Event]> { events.chunk_by(move |before, after| after.at.duration_since(before.at) <= gap) }
```

`into_grouping_map_by` keeps one accumulator per key, reduced through `max_by_key`, `minmax_by_key`, `sum`, or `fold`, or gathered through `collect` into any container per key (a set). `into_group_map_by` (`_with_hasher` under a set hasher) keeps every member for a consumer reading members in place of a fold pushing through `entry(key).or_default()`, `counts_by` one count per key. Itertools `chunk_by` groups consecutive equal-key items of an iterator. Groups borrow the `ChunkBy` value, and the pipeline ends inside the function creating it.

## [03]-[GENERATORS]

Cursors whose next step consumes input the previous item sized are `gen fn` bodies, their return type the item each `yield` produces on pull. The cursor is a local holding a borrow of a parameter, and no borrow of a local is held across a `yield`. Decoders yield a failure that ends their input as their last item. Cursors a package advances (`cycles.next(&graph)`) are `iter::from_fn`, an alternation between steps `cycle().scan`, and a run a peeked key bounds `peeking_take_while`, with no `gen` block. `process_results` runs an infallible pipeline over the successes and returns the failure:

```rust
pub gen fn frames(bytes: &[u8]) -> Result<&[u8], WireError> {
    let mut rest = bytes;
    while let Some((&length, body)) = rest.split_first() {
        let Some((frame, tail)) = body.split_at_checked(usize::from(length)) else {
            yield Err(WireError::Truncated { declared: length, available: body.len() });
            break;
        };
        yield Ok(frame);
        rest = tail;
    }
}

pub fn longest(bytes: &[u8]) -> Result<Option<usize>, WireError> { frames(bytes).process_results(|decoded| decoded.map(<[u8]>::len).max()) }
```

Frames borrow `bytes` without a copy, and `longest` collects nothing. `process_results` observes only input its consumer pulls; a short-circuiting consumer changes which later failures are read. Preserve the operation's consumption contract. `map_ok`, `filter_ok`, and `flatten_ok` transform successes for a consumer that handles each failure as it arrives.

## [04]-[PARALLELISM]

Independent CPU work can use `par_iter`, `par_chunk_by`, and `par_array_windows`, with enough work per partition to repay scheduling and synchronization. The consumer's order and failure contract stay part of the operation:

```rust
#[must_use]
pub fn fingerprints(blobs: &[Blob]) -> Validated<Vec<Fingerprint>, StoreError> {
    blobs.par_iter().map(|blob| fingerprint(blob).map_err(|source| StoreError::Fingerprint { blob: blob.id, source })).collect()
}
```

Every blob runs to its outcome with its typed subject. Successful indexed collection keeps source order; accumulated failure order is unspecified. Deterministic failures collect indexed results into a `Vec`, then sequentially into `Validated`. Ordered interfaces take `IntoParallelIterator<Item = T, Iter: IndexedParallelIterator>`. Independent CPU results take `rayon::join` and applicative error composition. Rayon schedules scoped tasks on its existing work-stealing pool; parallel iterators and `join` avoid per-task heap allocation when they express the work.

`map_init` initializes scratch or non-`Sync` handles per job, without a one-per-thread guarantee. Persistent indexed output reuses capacity through `collect_into_vec`; fresh collection already specializes exact length. `Vec::recycle` drops all elements and returns an empty buffer of a supported new element type under `Recyclable`, `vec_recycle`, and `transmutability`. Buffer reuse retains capacity only while later work needs that allocation.

Native splittable sources preserve parallel partitioning. `par_bridge` synchronizes sequential pulls and preserves no input order, fitting a source that cannot split rather than a slice with native parallel iteration. `flat_map_iter` keeps short inner work sequential inside parallel outer jobs. Job-local folds batch accumulation before merging when their identity and combination preserve the domain's order and failure contract.

Scheduling-independent randomness uses one generator per domain job, forked sequentially before parallel iteration or selected by `set_stream(job)` from one seed. `fold_chunks` fixes the runs a reduction reads. Parallel floating reductions change association and can change rounding; preserve the domain's reproducibility and accuracy requirements before changing the reduction tree. `walk_tree_prefix` and `walk_tree_postfix` express tree traversal. Grid kernels take an `ArrayView` generic over `D: Dimension`, and one `Zip::par_map_collect` serves a profile, plane, and volume:

```rust
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Envelope {
    Rights,
    Collection,
}

#[must_use]
pub fn envelope<D>(kind: Envelope, cells: ArrayView<'_, Point3<f64>, D>, edges: &[[Point3<f64>; 2]], suns: &[UnitVector3<f64>]) -> Array<Point3<f64>, D>
where D: Dimension {
    let (sign, bound): (f64, fn(f64, f64) -> f64) = match kind {
        Envelope::Rights => (-1.0, f64::min),
        Envelope::Collection => (1.0, f64::max),
    };
    let paths = suns.iter().filter(|sun| sun.z > 0.0).filter_map(|sun| Unit::try_new_and_get(sun.xy() * sign, 0.0).map(|(heading, run)| (heading, -sign * sun.z / run))).collect::<Vec<_>>();
    Zip::from(cells).par_map_collect(|cell| {
        let reach = edges
            .iter()
            .cartesian_product(&paths)
            .filter_map(|([start, end], (heading, slope))| {
                let (edge, offset) = (end.xy() - start.xy(), start.xy() - cell.xy());
                let across = heading.perp(&edge);
                let (distance, share) = (offset.perp(&edge) / across, offset.perp(heading) / across);
                (distance >= 0.0 && (0.0..=1.0).contains(&share)).then(|| slope.mul_add(distance, start.lerp(end, share).z))
            })
            .fold(cell.z, bound);
        Point3::new(cell.x, cell.y, reach)
    })
}
```

One `match` yields each kind's heading sign and bound `fn`, reach folding through the bound in place of a sign product negating each value twice.

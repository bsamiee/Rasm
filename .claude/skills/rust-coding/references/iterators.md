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

Gaps in a sorted series fill in one flat expression:

```rust
pub fn missing(readings: &[Reading], interval: Interval) -> impl Iterator<Item = Timestamp> {
    readings.array_windows().flat_map(move |[before, after]| before.at.series(interval.into_inner()).skip(1).take_while(move |at| *at < after.at))
}
```

`array_windows` borrows each `[before, after]` window of a slice. `map_windows` passes each `&[T; N]` window of an iterator from its own buffer and clones no item. `Timestamp::series` steps from `before` until a step overflows, and `take_while` ends each gap at `after`. `Interval` holds a positive `Span` its constructor enforces, a zero step never reaching `after`. `coalesce` hands its rule each adjacent pair and keeps the merged item for the next, and ranges sorted by start join every overlap in one pass. `merge_join_by` walks a sorted pair once and yields `Left`, `Right`, or `Both` per key, and reconciling expected against found needs no set. `k_smallest_relaxed_by_key` holds `2k` items and returns the `k` least in order in linear time with no sort of its input. `exactly_one` returns an error that iterates every item it read when none or more than one remains. `array_chunks` reads an iterator's chunks with no collected `Vec`, `Vec::into_chunks` reinterprets an owned buffer without a copy, and `as_flattened` reverses either.

## [02]-[GROUPING]

Grouping takes the form its consumer reads. Keyed groups over an unordered input fold per key without building the groups, and runs over an ordered input are borrowed chunks:

```rust
#[must_use]
pub fn latest(readings: &[Reading]) -> HashMap<SensorId, &Reading> { readings.iter().into_grouping_map_by(|reading| reading.sensor).max_by_key(|_, reading| reading.at) }

pub fn sessions(events: &[Event], gap: SignedDuration) -> impl Iterator<Item = &[Event]> { events.chunk_by(move |before, after| after.at.duration_since(before.at) <= gap) }
```

`into_grouping_map_by` keeps one accumulator per key, reduced through `max_by_key`, `minmax_by_key`, `sum`, or `fold`. `into_group_map_by` (`_with_hasher` under a set hasher) keeps every member for a consumer reading members in place of a fold pushing through `entry(key).or_default()`, `counts_by` one count per key. Itertools `chunk_by` groups consecutive equal-key items of an iterator. Groups borrow the `ChunkBy` value, and the pipeline ends inside the function creating it.

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

Frames borrow `bytes` without a copy, and `longest` collects nothing. `map_ok`, `filter_ok`, and `flatten_ok` transform successes for a consumer that handles each failure as it arrives.

## [04]-[PARALLELISM]

`par_iter`, `par_chunk_by`, and `par_array_windows` replace their sequential forms in a pipeline that collects once into `Validated`:

```rust
#[must_use]
pub fn fingerprints(blobs: &[Blob]) -> Validated<Vec<Fingerprint>, StoreError> {
    blobs.par_iter().map(|blob| fingerprint(blob).map_err(|source| StoreError::Fingerprint { blob: blob.id, source })).collect()
}
```

Every blob runs to its outcome, successes keep input order, and failures join in completion order and each names its blob, a failure list read in input order collecting into a `Vec` first, then sequentially into `Validated`. Parallel inputs take `IntoParallelIterator<Item = T, Iter: IndexedParallelIterator>`, keeping order and length. Pairs of independent CPU results run through `rayon::join`, each side lifted into `Validated` and joined by `map2`. Parallel steps read their item and shared immutable values alone, and a scratch buffer a step reuses comes from `map_init`, one per job. Seeded results stay independent of scheduling through one generator per job, each `rng.fork()` taken sequentially ahead of `into_par_iter`, or `set_stream(job)` over one seed. `fold_chunks` folds fixed-size runs for a result no split decides. `walk_tree_prefix` and `walk_tree_postfix` walk a tree from its root and a children closure. Grid kernels take an `ArrayView` generic over `D: Dimension`, and one `Zip::par_map_collect` serves a profile, a plane, and a volume:

```rust
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Envelope {
    Rights,
    Collection,
}

#[must_use]
pub fn envelope<D>(kind: Envelope, cells: ArrayView<'_, Point3<f64>, D>, edges: &[[Point3<f64>; 2]], suns: &[UnitVector3<f64>]) -> Array<Point3<f64>, D>
where D: Dimension {
    let sign = match kind {
        Envelope::Rights => -1.0,
        Envelope::Collection => 1.0,
    };
    let paths = suns.iter().filter(|sun| sun.z > 0.0).filter_map(|sun| Unit::try_new_and_get(sun.xy() * sign, 0.0).map(|(heading, run)| (heading, sun.z / run))).collect::<Vec<_>>();
    Zip::from(cells).par_map_collect(|cell| {
        let reach = edges
            .iter()
            .cartesian_product(&paths)
            .filter_map(|([start, end], (heading, slope))| {
                let (edge, offset) = (end.xy() - start.xy(), start.xy() - cell.xy());
                let across = heading.perp(&edge);
                let (distance, share) = (offset.perp(&edge) / across, offset.perp(heading) / across);
                (distance >= 0.0 && (0.0..=1.0).contains(&share)).then(|| slope.mul_add(-distance, sign * start.lerp(end, share).z))
            })
            .fold(sign * cell.z, f64::max);
        Point3::new(cell.x, cell.y, sign * reach)
    })
}
```

`sign` states each kind's direction once, and the bound folds as `f64::max` under it in place of a second field stating the same fact.

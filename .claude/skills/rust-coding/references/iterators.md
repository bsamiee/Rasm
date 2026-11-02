# [ITERATORS]

## [01]-[PIPELINES]

Each adapter states one transformation, and a pipeline reads top to bottom as the order of work:

| [INDEX] | [STEP]                                | [FORM]                                          |
| :-----: | :------------------------------------ | :---------------------------------------------- |
|  [01]   | Read each window of an iterator       | `map_windows`                                   |
|  [02]   | Merge adjacent items a rule joins     | `coalesce`                                      |
|  [03]   | Keep items through the first rejected | `take_while_inclusive`                          |
|  [04]   | Join keys across a sorted pair        | `merge_join_by`                                 |
|  [05]   | Merge many sorted sequences           | `kmerge_by`                                     |
|  [06]   | Keep the `k` least by a key           | `k_smallest_relaxed_by_key`                     |
|  [07]   | Take the one item a filter leaves     | `exactly_one`                                   |
|  [08]   | Chain iterables passed as arguments   | `iter::chain`                                   |
|  [09]   | Split a buffer into `[T; N]` and back | `as_chunks`, `Vec::into_chunks`, `as_flattened` |
|  [10]   | Split tuples into a collection each   | `multiunzip`                                    |
|  [11]   | Collect at least one item or none     | `try_into_nonempty_iter`                        |
|  [12]   | Join items into text                  | `format`                                        |
|  [13]   | Build a `Vec` from each index         | `Vec::from_fn`                                  |

Gaps in a sorted series fill in one flat expression:

```rust
pub fn missing(readings: &[Reading], interval: Interval) -> impl Iterator<Item = Timestamp> {
    readings.array_windows().flat_map(move |[before, after]| before.at.series(interval.into_inner()).skip(1).take_while(move |at| *at < after.at))
}
```

`array_windows` borrows each `[before, after]` window of a slice. `map_windows` passes each `&[T; N]` window of an iterator from its own buffer and clones no item. `Timestamp::series` steps from `before` until a step overflows, and `take_while` ends each gap at `after`. `Interval` holds a positive `Span` its constructor enforces, a zero step never reaching `after`. `coalesce` hands its rule each adjacent pair and keeps the merged item for the next, and ranges sorted by start join every overlap in one pass. `merge_join_by` walks a sorted pair once and yields `Left`, `Right`, or `Both` per key, and reconciling expected against found needs no set. `k_smallest_relaxed_by_key` holds `2k` items and returns the `k` least in order in linear time with no sort of its input. `exactly_one` returns an error that iterates every item it read when none or more than one remains.

## [02]-[GROUPING]

Grouping takes the form its consumer reads. Keyed groups over an unordered input fold per key without building the groups, and runs over an ordered input are borrowed chunks:

```rust
#[must_use]
pub fn latest(readings: &[Reading]) -> HashMap<SensorId, &Reading> { readings.iter().into_grouping_map_by(|reading| reading.sensor).max_by_key(|_, reading| reading.at) }

pub fn sessions(events: &[Event], gap: SignedDuration) -> impl Iterator<Item = &[Event]> { events.chunk_by(move |before, after| after.at.duration_since(before.at) <= gap) }
```

`into_grouping_map_by` keeps one accumulator per key, reduced through `max_by_key`, `minmax_by_key`, `sum`, or `fold`. `into_group_map_by` keeps every member for a consumer reading members, `counts_by` one count per key. Itertools `chunk_by` groups consecutive equal-key items of an iterator. Groups borrow the `ChunkBy` value, and the pipeline ends inside the function creating it.

## [03]-[GENERATORS]

Cursors whose next step consumes input the previous item sized are `gen fn` bodies, their return type the item each `yield` produces on pull. The cursor is a local holding a borrow of a parameter, and no borrow of a local is held across a `yield`. Decoders yield a failure that ends their input as their last item. `process_results` runs an infallible pipeline over the successes and returns the failure:

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

Every blob runs to its outcome, successes keep input order, and failures join in completion order and each names its blob. Parallel steps read their item and shared immutable values alone, and a scratch buffer a step reuses comes from `map_init`, one per job. Seeded results stay independent of scheduling through one generator per job, each `rng.fork()` taken sequentially ahead of `into_par_iter`, or `set_stream(job)` over one seed. `fold_chunks` folds fixed-size runs for a result no split decides. `walk_tree_prefix` and `walk_tree_postfix` walk a tree from its root and a children closure.

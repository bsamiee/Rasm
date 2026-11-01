# [ITERATORS]

Covers iterator pipelines and data parallelism.

## [01]-[PIPELINES]

Each adapter states one transformation, and a pipeline reads top to bottom as the order of work:

| [INDEX] | [STEP]                                | [FORM]                 |
| :-----: | :------------------------------------ | :--------------------- |
|  [01]   | Merge adjacent items a rule joins     | `coalesce`             |
|  [02]   | Keep items through the first rejected | `take_while_inclusive` |
|  [03]   | Join keys across a sorted pair        | `merge_join_by`        |
|  [04]   | Merge many sorted sequences           | `kmerge_by`            |

Gaps in a sorted series fill in one flat expression:

```rust
pub fn missing(readings: &[Reading], interval: Interval) -> impl Iterator<Item = Timestamp> {
    readings.array_windows().flat_map(move |[before, after]| before.at.series(interval.into_inner()).skip(1).take_while(move |at| *at < after.at))
}
```

`array_windows` borrows each `[before, after]` window of a slice, and `tuple_windows` clones each item of an iterator into its tuple. `Timestamp::series` steps from `before` until a step overflows, and `take_while` ends each gap at `after`. `Interval` holds a positive `Span` its constructor enforces, a zero step never reaching `after`. The returned iterator borrows `readings` and runs nothing until a consumer pulls. Producers collect only to own their items past the borrow, and their return type adds `+ DoubleEndedIterator`, `+ ExactSizeIterator`, or `+ Clone` for each trait a consumer calls. Reductions over an input that can be empty return `Option` through `reduce` or `max_by_key`, and a seed states the result an empty input defines. `coalesce` hands its rule each adjacent pair and keeps the merged item for the next, and ranges sorted by start join every overlap in one pass. `merge_join_by` walks a sorted pair once and yields `Left`, `Right`, or `Both` per key, and a reconcile of expected against found reads every difference with no set.

## [02]-[GROUPING]

Grouping takes the form its consumer reads. Keyed groups over an unordered input fold per key without building the groups, and runs over an ordered input are borrowed chunks:

```rust
#[must_use]
pub fn latest(readings: &[Reading]) -> HashMap<SensorId, &Reading> { readings.iter().into_grouping_map_by(|reading| reading.sensor).max_by_key(|_, reading| reading.at) }

pub fn sessions(events: &[Event], gap: SignedDuration) -> impl Iterator<Item = &[Event]> { events.chunk_by(move |before, after| after.at.duration_since(before.at) <= gap) }
```

`into_grouping_map_by` keeps one accumulator per key as items arrive and reduces it through `max_by_key`, `minmax_by_key`, `sum`, or `fold`, `into_group_map_by` keeps every member for a consumer that reads members, and `counts_by` one count per key. `slice::chunk_by` splits wherever the test over an adjacent pair fails and returns runs that borrow the slice. Itertools `chunk_by` groups consecutive items of an iterator by an equal key, and its groups borrow the `ChunkBy` value, ending the pipeline inside the function that creates it.

## [03]-[GENERATORS]

Cursors whose next step consumes an amount of input the previous item decided are `gen` blocks, each `yield` producing one item as the consumer pulls. The cursor is a block local holding a borrow of input from outside the block, and no borrow of a block local is held across a `yield`. Decoders whose failure ends their input yield that failure as their last item, and `process_results` runs any infallible pipeline over the successes and returns the failure:

```rust
pub fn frames(bytes: &[u8]) -> impl Iterator<Item = Result<&[u8], WireError>> {
    gen move {
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

Every blob runs to its outcome, successes keep input order, and failures join in completion order and each names its blob. Parallel steps read their item and shared immutable values alone, and a scratch buffer a step reuses comes from `map_init`, one per job. Reductions across the thread pool take an identity closure and an associative step: `reduce` and `try_reduce` combine items, and `fold` builds one accumulator per split that a following `reduce` joins.

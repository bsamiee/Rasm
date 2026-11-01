# [ITERATORS]

Covers iterator pipelines, from adjacency and grouping to generator blocks and data parallelism.

## [01]-[PIPELINES]

Each adapter states one transformation, and a pipeline reads top to bottom as the order of work:

| [INDEX] | [STEP]                                | [ADAPTER]              |
| :-----: | :------------------------------------ | :--------------------- |
|  [01]   | Fold a running value into each item   | `scan`                 |
|  [02]   | Transform until the first absent item | `map_while`            |
|  [03]   | Keep items through the first rejected | `take_while_inclusive` |
|  [04]   | Read each item beside its neighbors   | `array_windows`        |
|  [05]   | Merge adjacent items a rule joins     | `coalesce`             |
|  [06]   | Join keys across a sorted pair        | `merge_join_by`        |
|  [07]   | Merge many sorted sequences           | `kmerge_by`            |

Adjacent pairs and a bounded range per pair fill the gaps of a sorted series in one flat expression:

```rust
pub fn missing(readings: &[Reading], interval: Interval) -> impl Iterator<Item = Timestamp> {
    readings.array_windows().flat_map(move |[before, after]| before.at.series(interval.into_inner()).skip(1).take_while(move |at| *at < after.at))
}
```

`array_windows` on a slice borrows each `[before, after]` window, and on an iterator clones each item into its array. `Timestamp::series` steps from `before` until a step overflows, `take_while` ends each gap at `after`, and `Interval` holds a positive `Span` its constructor checks, since a zero step repeats one timestamp without end. The returned iterator borrows `readings` and runs nothing until a consumer pulls, so `next` computes the first gap alone. Producers collect only to own their items past the borrow, and their return type adds `+ DoubleEndedIterator`, `+ ExactSizeIterator`, or `+ Clone` for each trait a consumer calls. `coalesce` hands its rule each adjacent pair and keeps the merged item for the next pair, so ranges sorted by start join every overlap in one pass. `merge_join_by` walks a sorted pair once and yields `Left`, `Right`, or `Both` per key, so reconciling expected against found reads every difference without building a set.

## [02]-[GROUPING]

Grouping takes the form its consumer reads. Keyed groups over an unordered input fold per key without building the groups, and runs over an ordered input are borrowed chunks:

```rust
#[must_use]
pub fn latest(readings: &[Reading]) -> HashMap<SensorId, &Reading> {
    readings.iter().into_grouping_map_by(|reading| reading.sensor).max_by_key(|_, reading| reading.at)
}

pub fn sessions(events: &[Event], gap: SignedDuration) -> impl Iterator<Item = &[Event]> {
    events.chunk_by(move |before, after| after.at.duration_since(before.at) <= gap)
}
```

`into_grouping_map_by` holds one accumulator per key as items arrive: `max_by_key`, `min_by_key`, `sum`, and `fold` reduce, and `aggregate` drops the key whose step returns `None`. `into_group_map_by` holds every member for a consumer that reads members, `counts_by` one count per key. `slice::chunk_by` splits wherever the test over an adjacent pair fails and returns runs that borrow the slice. Itertools `chunk_by` groups consecutive items of an iterator by an equal key, and its groups borrow the `ChunkBy` value, so that pipeline ends inside the function that creates it.

## [03]-[GENERATORS]

Cursors whose next step consumes an amount of input the previous item decided are `gen` blocks holding the cursor as a block local, each `yield` producing one item as the consumer pulls. Decoders whose failure ends their input yield that failure as their last item, and `process_results` runs any infallible pipeline over the successes and returns the failure:

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

Each length byte decides where the next frame starts, frames borrow `bytes` without a copy, and `longest` collects nothing. Items whose failures leave each other valid collect through `Validated` in [RESULTS](results.md), and `map_ok`, `filter_ok`, and `flatten_ok` transform successes for a consumer that handles each failure as it arrives.

## [04]-[PARALLELISM]

Independent CPU-bound work per item runs on the `rayon` pool, `par_iter` replaces `iter` in the same pipeline, and the pipeline collects once into `Validated`:

```rust
#[must_use]
pub fn fingerprints(blobs: &[Blob]) -> Validated<Vec<Fingerprint>, StoreError> {
    blobs.par_iter().map(|blob| fingerprint(blob).map_err(|source| StoreError::Fingerprint { blob: blob.id, source })).collect()
}
```

Every blob runs to its outcome, successes keep input order, and each failure names its blob. Parallel steps read their item and shared immutable values alone. Reductions across the pool take an identity closure and an associative step: `reduce` and `try_reduce` combine items, and `fold` builds one accumulator per split that a following `reduce` joins.

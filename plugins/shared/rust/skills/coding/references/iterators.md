# [ITERATORS]

## [01]-[PIPELINES]

Pipeline steps take these members, top to bottom in order of work:

| [INDEX] | [STEP]                                  | [FORM]                                                               |
| :-----: | :-------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | Read each fixed window                  | `array_windows` over a slice, `map_windows` over an iterator         |
|  [02]   | Merge adjacent items a rule joins       | `coalesce`                                                           |
|  [03]   | Keep items through the first refused    | `take_while_inclusive`                                               |
|  [04]   | Join keys across a sorted pair          | `merge_join_by`                                                      |
|  [05]   | Merge many sorted sequences             | `kmerge_by`                                                          |
|  [06]   | Keep the `k` least by a key             | `k_smallest_relaxed_by_key`, `partial_sort_unstable_by_key` in place |
|  [07]   | Take the one item a filter leaves       | `exactly_one`, `collect_array::<N>` for `N`                          |
|  [08]   | Chain iterables passed as arguments     | `iter::chain`                                                        |
|  [09]   | Group items into `[T; N]` and back      | `array_chunks`, `as_chunks`, `Vec::into_chunks`, `as_flattened`      |
|  [10]   | Split tuples into a collection each     | `collect::<(Vec<_>, Vec<_>)>()`                                      |
|  [11]   | Move matching items out of a collection | `extract_if`                                                         |
|  [12]   | Collect at least one item or none       | `try_into_nonempty_iter`                                             |
|  [13]   | Join items into text                    | `format`                                                             |
|  [14]   | Build a `Vec` from each index           | `Vec::from_fn`                                                       |
|  [15]   | Join arms yielding different iterators  | `Either`, `iter::once` for a single item                             |
|  [16]   | Apply one step to a fixed set           | `[left, right].map`, `try_map`                                       |
|  [17]   | Pair each item with every later one     | `array_combinations::<2>`                                            |
|  [18]   | Walk iterables in lockstep              | `izip!` yielding one flat tuple                                      |
|  [19]   | Number items in a target integer type   | `iter::zip(0_u32.., items)`                                          |
|  [20]   | Treat the first or last item apart      | `with_position`                                                      |

Gaps in a sorted series are the instants each window expects before its next reading:

```rust
pub fn missing(readings: &[Reading], step: Step) -> impl Iterator<Item = Timestamp> {
    readings.array_windows().flat_map(move |[before, after]| {
        let expected = iter::successors(Some(before.at), move |at| at.checked_add(step.get()).ok());
        expected.skip(1).take_while(move |at| *at < after.at)
    })
}
```

`Step` holds a positive `SignedDuration`, and each series ends where `checked_add` overflows or `take_while` reaches `after`:
- `array_windows` binds each `[before, after]` window by pattern, and `map_windows` (`iter_map_windows`) buffers an iterator's window
- `coalesce` compares each merged item with the next, and ranges sorted by start merge every overlap in one pass
- `exactly_one` returns an error that iterates every input item, none lost to the test
- `as_chunks` returns a remainder beside its chunks, `array_chunks` (`iter_array_chunks`) through `into_remainder`
- `Vec::into_chunks` (`vec_into_chunks`) drops the remainder, and a domain that keeps or refuses one takes `as_chunks`
- `k_smallest_relaxed_by_key` holds at most `2k` items, and `partial_sort_unstable_by_key(..k, key)` sorts the least `k` of a slice in place
- Unstable selection orders equal keys arbitrarily, and ties that keep input order key on `(key, index)`
- `partial_sort_unstable_by_key` takes `slice_partial_sort_unstable`, and array `try_map` takes `array_try_map`
- `Vec::extract_if(range, filter)` yields each removed item in one pass, where a `retain` beside a cloned `filter` reads every item twice

## [02]-[FIXED_FIELDS]

Text with fixed fields splits into an array through `next_chunk::<N>` (`iter_next_chunk`), and one slice pattern names each field:

```rust
#[must_use]
pub fn pinned<'list>(list: &'list str, name: &str) -> Option<(&'list str, &'list str)> {
    list.lines().find_map(|line| {
        let mut words = line.split_ascii_whitespace();
        let [named, version, digest] = words.next_chunk().ok()?;
        (named == name && words.next().is_none()).then_some((version, digest))
    })
}
```

Each line of `list` holds a name, a version, and a digest:
- `next_chunk` returns `Err` holding what it read when fewer than `N` fields remain, and a parser maps that `Err` to its refusal
- `words.next().is_none()` refuses a field past `N`, and a record read by its leading fields omits that test
- `split_once` splits at the first separator, `rsplit_once` at the last, each returning `None` where the separator is absent
- Keyed lines read through `strip_prefix` chained by `?` (`line.strip_prefix(key)?.strip_prefix('=')`)
- Indexing a collected `Vec` of fields panics on a short line, where the pattern returns `None`

## [03]-[GROUPING]

Keyed groups over an unordered input fold per key without building the groups, and runs over an ordered input are borrowed chunks:

```rust
#[must_use]
pub fn latest(readings: &[Reading]) -> HashMap<SensorId, &Reading> {
    let by_sensor = readings.iter().into_grouping_map_by(|reading| reading.sensor);
    by_sensor.max_by_key(|_, reading| reading.at)
}

pub fn sessions(events: &[Event], gap: SignedDuration) -> impl Iterator<Item = &[Event]> {
    let joined = move |before: &Event, after: &Event| after.at.duration_since(before.at) <= gap;
    events.chunk_by(joined)
}
```

`Reading` holds `sensor` and `at`, and `Event` holds `at`:
- `into_grouping_map_by` keeps one accumulator per key, reduced through `max_by_key`, `minmax_by_key`, `sum`, or `fold`, or gathered by `collect`
- `into_group_map_by` keeps every member for a consumer reading members, in place of a `fold` pushing through `entry(key).or_default()`
- `counts_by` keeps one count per key
- Itertools `chunk_by` groups runs of an iterator, its groups borrowing the `ChunkBy` value inside the function creating it

## [04]-[GENERATORS]

Cursors whose next step consumes input the previous item sized are `gen fn` bodies under `gen_blocks`, each `yield` producing an item on pull:

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

Frames borrow `bytes` with no copy, and `longest` collects nothing:
- Cursors borrow from a reference parameter, and a borrow of anything the body owns held across `yield` fails to compile
- Decoders yield the failure that ends their input as their last item
- `process_results` runs an infallible pipeline over the successes and returns the first failure, reading no item after it
- `map_ok`, `filter_ok`, and `flatten_ok` transform successes for a consumer handling each failure as it arrives
- Cursors a package advances through a context argument (a walker's `next(&graph)`) are `iter::from_fn`, with no `gen` block
- `take_while` drops the first item its test refuses, and `peeking_take_while` over a `Peekable` leaves it for the next read

## [05]-[PARALLELISM]

Independent CPU work runs through the parallel form of its sequential member (`par_iter`, `par_chunk_by`, `par_array_windows`):

```rust
#[must_use]
pub fn fingerprints(blobs: &[Blob]) -> Validated<Vec<Fingerprint>, StoreError> {
    blobs.par_iter().map(|blob| fingerprint(blob).map_err(|source| StoreError::Fingerprint { blob: blob.id, source })).collect()
}
```

Every blob runs to its outcome under the `rayon` feature of `validated`, successes in source order and failures in completion order:
- Failures reported in a fixed order collect into a `Vec` of results, then sequentially into `Validated`
- Functions reading order bound `IntoParallelIterator<Iter: IndexedParallelIterator>`
- Independent computations run through `rayon::join` or a parallel iterator, where each `rayon::scope` spawn heap-allocates its job
- `map_init` builds scratch or a non-`Sync` handle once per rayon job, and one thread runs many jobs
- `collect_into_vec` clears and refills a kept `Vec`, reusing its capacity
- `par_bridge` serves a source that cannot split and yields in no input order, and a slice takes `par_iter`
- `flat_map_iter` keeps short inner work sequential inside each parallel job
- Accumulations fold per job (`fold(Vec::new, ..)`) and merge once through `reduce`
- Random draws reproducible across runs seed one `ChaCha8Rng` per chunk of `par_chunks(..).enumerate()`, `set_stream(index)` over one seed
- Floating sums reproducible across runs fold fixed chunks through `fold_chunks`, collect the partials, and add them in order
- Parallel tree walks take `walk_tree_prefix` or `walk_tree_postfix` over a function returning each node's children
- Grid kernels take `ArrayView<'_, T, D>` with `D: Dimension`, and one `Zip::from(view).par_map_collect(..)` serves a row, a plane, and a volume

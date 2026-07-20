# [PY_TESTS_API_INLINE_SNAPSHOT]

`inline-snapshot` captures a value into a `snapshot()` literal that the test source carries, then compares subsequent runs against it. Rasm admits it for genuine wire goldens only — payloads an independent producer emits — under a report-only default: `default-flags = ["short-report"]` reports every mismatch without mutating a snapshot, and the `fix` run flag is the single sanctioned rewrite path. Storage for outsourced blobs and external files lives under `.cache/inline-snapshot`.

## [01]-[PACKAGE_SURFACE]

- package: `inline-snapshot` · version `0.34.2` · license `MIT`
- namespace: `inline_snapshot`; extra assertion helpers `inline_snapshot.extra`
- asset: pure-Python wheel; a pytest plugin binding the `--inline-snapshot=` run-flag vocabulary
- rail: wire-golden equality — the Python round-trip rail for `tests/contracts/` assets

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]      | [KIND]        | [CAPABILITY]                                                                               |
| :-----: | :------------ | :------------ | :----------------------------------------------------------------------------------------- |
|  [01]   | `Snapshot[T]` | generic type  | the value a `snapshot()` call stands in for; comparison and containment resolve against it |
|  [02]   | `SnapshotArg` | type alias    | a `Snapshot[T] \| T` parameter — a helper accepts a live value or a snapshot placeholder   |
|  [03]   | `Is(value)`   | wrapper       | embeds a runtime value inside a snapshot literal so a dynamic field stays comparable       |
|  [04]   | `Category`    | Literal alias | the change categories `create`/`fix`/`fix-assert`/`trim`/`update` a run flag activates     |
|  [05]   | `Format`      | protocol      | the serialization contract `register_format` binds for an external-file suffix             |
|  [06]   | `HasRepr`     | wrapper       | asserts an object's `repr` inside a snapshot without an equality-comparable value          |
|  [07]   | `UsageError`  | exception     | raised on a malformed snapshot call or an unmanaged-value misuse                           |

```python signature
def snapshot(obj: T = ...) -> T: ...                                    # empty snapshot() records on the create flag
def external(name: str | None = None) -> Snapshot[object]: ...          # value stored in an external file, not inline
def external_file(path: Path | str, *, format: str | None = None) -> Snapshot[object]: ...  # bind an explicit external asset path
def outsource(data: object, suffix: str | None = None, storage: str | None = None) -> object: ...  # large value → hash-keyed store
def register_format(format: type[Format] | Format | None = None, *, replace_handler: bool = False): ...  # serializer per suffix
def get_snapshot_value(snapshot: Snapshot[T]) -> T: ...                 # read the recorded value out of a placeholder
```

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                              | [KIND]          | [CAPABILITY]                                                                |
| :-----: | :------------------------------------- | :-------------- | :-------------------------------------------------------------------------- |
|  [01]   | `snapshot(obj=...)`                    | recorder        | placeholder; `==`/`in`/`<=` against a `snapshot(...)` all compare           |
|  [02]   | `external(name=None)`                  | recorder        | routes the recorded value to an external file under the storage directory   |
|  [03]   | `outsource(data, suffix=None)`         | recorder        | stores a large payload by content hash; the literal stays a short reference |
|  [04]   | `Is(value)`                            | wrapper         | injects a live value so a nondeterministic field stays comparable           |
|  [05]   | `extra.raises(snapshot(...))`          | context manager | asserts a raised exception's rendering against a snapshot                   |
|  [06]   | `extra.prints(stdout=..., stderr=...)` | context manager | asserts captured stdout/stderr against snapshots                            |
|  [07]   | `extra.warns(snapshot([...]))`         | context manager | asserts emitted warnings against a snapshot list                            |

```python signature
from inline_snapshot import snapshot, external, outsource, Is
from inline_snapshot.extra import raises, prints, warns
def test_wire_golden(produce: Callable[[], bytes]) -> None:
    assert produce() == snapshot()                                      # records under --inline-snapshot=create, compares thereafter
    assert decode(produce()) == snapshot({"id": Is(runtime_id), "kind": "shape"})
```

## [04]-[IMPLEMENTATION_LAW]

[INLINE_SNAPSHOT_TOPOLOGY]:
- `[tool.inline-snapshot]` sets `default-flags = ["short-report"]` and `storage-dir = ".cache/inline-snapshot"`: a bare run compares and reports, never mutates.
- `--inline-snapshot=` names the run-flag vocabulary — `create`/`fix`/`trim`/`update`/`disable` for mutation modes, `report`/`short-report`/`review` for reporting; `create` fills an empty `snapshot()`, `fix` rewrites a mismatched literal, `trim` drops an over-broad container element, `update` re-renders after a token-stream change, and `review` walks each change interactively.
- A snapshot compares a producer-emitted payload only; a value the test itself computes is a tautology, never a golden.
- `outsource` and `external` keep large payloads out of the source literal, hash-keyed under the storage directory; `register_format` binds a serializer per external suffix.

[STACKING]:
- `dirty-equals`(`.api/dirty-equals.md`): an `Is*` matcher lives inside a `snapshot(...)` literal to hold a nondeterministic field (`IsNow`, `IsUUID`) partial while the surrounding structure stays exact.
- `tests/contracts/README.md`(`../../contracts/README.md`): inline-snapshot is the Python leg of the cross-language contract round-trip — a corpus asset decodes, re-encodes, and asserts byte identity against its stored golden.
- `hypothesis`(`.api/hypothesis.md`): a snapshot asserts a fixed producer output; a generated-input law proves through the `spec.py` algebraic oracles, never a snapshot.

[LOCAL_ADMISSION]:
- Admitted on the `tests/` dev plane; the pytest plugin binds the run flags and the report-only default governs a bare session.
- Snapshot rewrites are a deliberate `--inline-snapshot=fix` action reviewed as a source diff, never an ambient side effect of a passing run.

[RAIL_LAW]:
- Package: `inline-snapshot`
- Owns: literal-carried golden capture, mismatch reporting, external/outsourced storage, and the `Category` change vocabulary a run flag activates.
- Accept: `snapshot()` over a producer-emitted payload; `Is`/`dirty-equals` matchers for nondeterministic fields; `external`/`outsource` for large blobs; `--inline-snapshot=fix` as the sanctioned rewrite.
- Reject: a snapshot over a test-computed value; an ambient snapshot mutation on a passing run; a whole-value golden where a `dirty-equals` partial expresses the real invariant; a run without the report-only default.

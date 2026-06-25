# [PYTHON_SYSTEM_APIS]

This page is the stdlib-owner-replacement law: the high-churn surface where a yearly standard-library delta retires a local helper, kept disjoint from the stable language-form law so the language page never churns with a stdlib addition. A standard-library API replaces local machinery only when it owns the concern; it never replaces an `expression` rail, `pydantic`/`msgspec` admission, the numeric route owners, or the structured-concurrency owner. Each row names the owning stdlib surface and the local loop, wrapper, or pattern it deletes, and a snippet composes that surface under the `expression` rail. A snippet whose stdlib core leaves a boundary type-shape implicit carries a `beartype` contract proving that shape at the seam, the value-level invariant the types cannot carry staying on the `Result` rail; the full violation-redirecting contract weave — `beartype(conf=BeartypeConf(violation_type=...))` folded onto the fault — is `surfaces-and-dispatch.md`'s aspect and is composed there, never re-derived as a factory here. Every member is fixed at the active interpreter surface `language.md` declares — a regressed owner moves to the language page's replacement column with the older spelling, and nothing here predates that floor. Two platform-forced statement seams are named: the `pickle` protocol-5 `buffer_callback` sink that collects each out-of-band block as the reduction streams, and the `try`/`except` that converts a `strict=`-mode iterator's ragged-tail raise into the rail — both are the signal the stdlib API forces, and both still leave the result as an `expression` `Result`.

## [01]-[SMELL_LOOKUP]

This table is a lookup by repeated local smell; the owning section card states the placement law.

| [INDEX] | [SMELL]                            | [OWNER]                         |
| :-----: | :--------------------------------- | :------------------------------ |
|  [01]   | stringly `os.walk`/`os.path` flow  | `pathlib.Path` algebra          |
|  [02]   | manual chunked hash loop           | `hashlib.file_digest`           |
|  [03]   | local chunk/batch helper loop      | `itertools.batched`             |
|  [04]   | `zip` product fold                 | `math.sumprod`                  |
|  [05]   | timestamp-prefixed UUID wrapper    | `uuid.uuid7`                    |
|  [06]   | ambiguous `re.match` prefix check  | `re.prefixmatch`                |
|  [07]   | `datetime.strptime` slicing        | `date.strptime`/`time.strptime` |
|  [08]   | subprocess or bespoke zstd adapter | `compression.zstd`              |
|  [09]   | `bytes(buffer)` plus `clear()`     | `bytearray.take_bytes`          |
|  [10]   | `os.read` copy slice               | `os.readinto`                   |
|  [11]   | copy-heavy pickle blob             | pickle protocol-5 OOB buffers   |
|  [12]   | negated-priority heap wrapper      | max-heap `heapq` APIs           |
|  [13]   | `sum(counter.values())` fold       | `Counter.total`                 |
|  [14]   | `denominator == 1` integrality     | `Fraction.is_integer`           |

## [02]-[PATHS_AND_FILES]

[PATH_ALGEBRA]:
- Owner: `pathlib.Path` and `PurePath` — `Path.walk`, `Path.copy`, `Path.copy_into`, `Path.move`, `Path.move_into`, `Path.info`, `Path.from_uri`, `PurePath.full_match`, `PurePath.with_segments`, `os.path.splitroot`, and `os.path.realpath(strict=os.path.ALLOW_MISSING)`.
- Replace: stringly `os.walk` flow, `shutil` transfer wrappers, repeated `stat()` probes, hand-parsed `file:` URLs, recursive glob predicates, drive/root string slicing, and symlink-prefix resolution loops.
- Gate: `recurse_symlinks=`/`follow_symlinks=` carries symlink policy explicitly; `Path.info` caches the stat result on one `_Info` accessor, so `exists`/`is_dir`/`is_file`/`is_symlink` share one syscall instead of one probe each.
- Rule: `Path.copy_into`/`Path.move_into` target a directory and keep the source name, the spelling a rooted right segment would otherwise corrupt; `Path.walk` yields `(dirpath, dirnames, filenames)` and replaces the `os.walk` string idiom end to end.

[FILE_IO]:
- Owner: `NamedTemporaryFile(delete_on_close=...)`, `shutil.rmtree(onexc=...)`, `os.readinto`, `importlib.resources.as_file`, `tomllib` (TOML 1.1.0), and `mimetypes.guess_file_type`.
- Replace: `mkstemp` unlink ladders, `onerror` tuple handlers, `os.read` copy slices, `__file__` extraction loops, `tomli`/parser shims, and path use of `mimetypes.guess_type`.
- Gate: persisted text I/O states `encoding="utf-8"` (or `encoding="locale"` only at a genuine locale boundary), never an implicit default at a durable seam.
- Rule: `importlib.resources.as_file` materializes a packaged resource to a real filesystem path inside a `with` scope and reclaims it on exit, the `__file__`-relative path computation it retires; a config payload parses through `tomllib.load`/`loads` (TOML 1.1.0), never a `tomli` backport shim the active interpreter subsumes.

```python conceptual
from collections.abc import Callable
from enum import StrEnum
from os.path import ALLOW_MISSING, realpath
from pathlib import Path
from typing import Literal

from beartype import beartype
from builtins import frozendict
from expression import Error, Ok, Result

type TransferFault = Literal["<missing-source>", "<target-not-dir>"]


class Transfer(StrEnum):
    COPY = "<mode-a>"
    MOVE = "<mode-b>"


TRANSFER: frozendict[Transfer, Callable[[Path, Path], Path]] = frozendict({
    Transfer.COPY: Path.copy_into,
    Transfer.MOVE: Path.move_into,
})


@beartype
def transferred(mode: Transfer, source: Path, into: Path, /) -> Result[Path, TransferFault]:
    target = into.with_segments(realpath(into, strict=ALLOW_MISSING))
    return (
        Error("<missing-source>")
        if not source.info.exists()
        else Error("<target-not-dir>")
        if not target.info.is_dir()
        else Ok(TRANSFER[mode](source, target))
    )
```

## [03]-[TEXT_REGEX_TIME]

[REGEX]:
- Owner: `re` with `re.prefixmatch`, `Pattern.prefixmatch`, `re.PatternError`, and compiled module-level patterns.
- Replace: streamed-input completeness probes built on `re.match` retries, and broad `re.error` catches.
- Rule: a structural grammar compiles to one module-level pattern; `Pattern.prefixmatch(string)` reports a partial match when the input is a valid prefix of a complete match, the "incomplete but still valid" verdict `re.match` collapses into the same `None` as outright failure, so a streaming scanner advances on it instead of re-running the match; a parse-error path catches `re.PatternError`, never the broad `re.error`.

[DATETIME]:
- Owner: `datetime.date.strptime`, `datetime.time.strptime`, and timezone-aware `datetime`.
- Replace: `datetime.strptime` followed by `.date()`/`.time()` slicing and naive datetime arithmetic at boundaries.
- Boundary: a wire timestamp admits to an aware `datetime` at the seam, and the interior carries the aware value, never a naive one.

[TEMPLATE_RENDER]:
- Owner: t-string processors over `string.templatelib.Template` consumed at the render boundary.
- Replace: f-string pre-parsing, rendered-string reparsing, and string concatenation hiding template policy.
- Boundary: template structure — segments, interpolations, conversions, format specs — is a language-form concern owned by `language.md`; this row owns only the render-time consumption of the already-built `Template`.

## [04]-[NUMERIC_PRIMITIVES]

[SCALAR_MATH]:
- Owner: `math.integer`, `math.fma`, `math.fmax`/`math.fmin`, `math.isnormal`/`math.issubnormal`/`math.signbit`, `math.sumprod`, `fractions.Fraction.from_number`, and `fractions.Fraction.is_integer`.
- Replace: float math on integer algorithms, rounded multiply-add, NaN-aware min/max wrappers, bit-level float probes, `zip` product folds, `Fraction` constructor branch ladders, and `denominator == 1` integrality probes.
- Gate: `math.fma(a, b, c)` is one correctly-rounded operation where `a * b + c` rounds twice; `math.sumprod(p, q)` is the fused dot product a `sum(x * y for ...)` fold loses accuracy and speed to.
- Rule: `Fraction.from_number(value)` admits a `float`/`Decimal` to an exact rational in one call where a constructor branch ladder dispatches on the input kind, and `Fraction.is_integer()` is the `denominator == 1` integrality probe stated as a method, never re-derived from the reduced terms.
- Boundary: every array, matrix, density-estimation, and seeded-sampling concern — kernel-density estimates, inverse-CDF resamplers, and any draw over an estimated distribution — routes to `algorithms.md`; this card owns scalar invariants on stdlib numeric primitives only, never a sample-level statistical estimator.

[IDENTITY_AND_HEAP]:
- Owner: `uuid.uuid7`, `uuid.NIL`, `uuid.MAX`, `Counter.subtract`, the max-heap `heapq` APIs `heapify_max`/`heappush_max`/`heappop_max`/`heapreplace_max`, and `operator.is_none`/`operator.is_not_none`.
- Replace: timestamp-prefixed UUID wrappers, magic UUID boundary literals, manual count-difference folds, negated-priority heap wrappers, and one-off `lambda x: x is None`.
- Rule: `uuid.uuid7` is monotonic within a timestamp tick, so a time-ordered key carries its own sort order and needs no separate sequence column.
- Rule: the `_max` heap family is the native max-priority surface a value-negation `heappush` wrapper otherwise simulates, and `Counter.subtract` keeps the signed count delta the multiset `-` operator clamps to positive and a paired-iteration fold restates by hand.

```python conceptual
from dataclasses import dataclass
from fractions import Fraction
from heapq import heapify_max, heappop_max
from math import fma, sumprod
from typing import Literal
from uuid import NIL, UUID, uuid7

from beartype import beartype
from expression import Error, Ok, Result

type WeightFault = Literal["<rank-mismatch>", "<empty>", "<non-integral>", "<unseeded>"]


@dataclass(frozen=True, slots=True, kw_only=True)
class Receipt:
    identity: UUID
    weighted: Fraction
    corrected: float
    peak: Fraction


@beartype
def receipted(weights: tuple[Fraction, ...], values: tuple[Fraction, ...], prior: UUID, /) -> Result[Receipt, WeightFault]:
    return (
        Error("<unseeded>")
        if prior == NIL
        else Error("<empty>")
        if not weights
        else Error("<rank-mismatch>")
        if len(weights) != len(values)
        else Error("<non-integral>")
        if not (exact := Fraction.from_number(sumprod(weights, values))).is_integer()
        else Ok(
            Receipt(
                identity=(minted := uuid7()) if minted > prior else prior,
                weighted=exact,
                corrected=fma(float(weights[0]), float(values[0]), float(exact)),
                peak=heappop_max((heap := list(weights), heapify_max(heap))[0]),
            )
        )
    )
```

## [05]-[BINARY_AND_INTEGRITY]

[BUFFERS]:
- Owner: `inspect.BufferFlags`, `array` and `memoryview` complex codes (`Zf`/`Zd`), `memoryview.index`/`memoryview.count`, `os.readinto`, `bytearray.take_bytes`, and pickle protocol-5 out-of-band buffers.
- Replace: magic integer buffer flags, struct-packed complex buffers, `os.read` copy slices, `bytes(buffer)` plus `clear()`, and copy-heavy pickle blobs.
- Rule: a protocol-5 reduction routes its payload through `pickle.PickleBuffer`, the `buffer_callback` collects each out-of-band block, and `loads(..., buffers=...)` restores the view without a copy of the numeric body.
- Rule: `bytearray.take_bytes` moves the backing store out in one operation where `bytes(buf)` followed by `buf.clear()` copies the bytes and then zeroes the source; `os.readinto(fd, buffer)` fills a caller-owned `bytearray` in place where `os.read` allocates a fresh copy slice per call.
- Boundary: `collections.abc.Buffer` is the protocol-form owner declared by `language.md`; the live-memory borrow window — `ctypes.memoryview_at` opened, copied, and `view.release()`-closed before the foreign free, and any zero-copy ownership held across an `await` — is the FFI-lifetime concern `boundaries.md` owns. This card owns only the stdlib in-process buffer-surface selection, never the protocol form and never the native borrow lifetime.

[CODECS_AND_DIGEST]:
- Owner: `compression.zstd` (with `ZstdDict`, `train_dict`, and `CompressionParameter`), `base64.z85encode`/`z85decode`, base-N decoders (`canonical=`/`padded=`/`ignorechars=` on `b32decode`/`b64decode`), the base-N `wrapcol=` encode control, `hashlib.file_digest`, and `zlib.adler32_combine`/`zlib.crc32_combine`.
- Replace: subprocess or bespoke zstd adapters, local Z85 codecs, padding-bit postchecks, manual line-wrap formatting, chunked hash loops, and recompress-to-checksum loops.
- Gate: a wire digest streams through `hashlib.file_digest` (file) or one `hashlib` one-shot (in-memory); a non-cryptographic cache key uses a fast non-cryptographic hash, never SHA; a repeated-corpus payload trains one `ZstdDict` rather than recompressing a shared prefix per message.
- Rule: `canonical=True` rejects non-canonical base-N input at decode instead of a post-decode padding-bit check; `wrapcol=` is an encode-side line-width control and never appears on a decode call.

```python conceptual
import compression.zstd as zstd
import hashlib
import pickle
from collections.abc import Buffer, Callable
from dataclasses import dataclass
from typing import Literal

from beartype import beartype
from expression import Error, Ok, Result

type FrameFault = Literal["<empty-view>"]


@dataclass(frozen=True, slots=True, kw_only=True)
class Frame:
    body: bytes
    blocks: tuple[pickle.PickleBuffer, ...]
    digest: str


class Carrier[T: Buffer]:
    __slots__ = ("_view",)

    def __init__(self, payload: T, /) -> None:
        self._view = memoryview(payload)

    def __reduce_ex__(self, protocol: int, /) -> tuple[Callable[[pickle.PickleBuffer], memoryview], tuple[pickle.PickleBuffer]]:
        return memoryview, (pickle.PickleBuffer(self._view),)


@beartype
def framed[T: Buffer](payload: T, level: int, *, shared: zstd.ZstdDict | None = None) -> Result[Frame, FrameFault]:
    blocks: list[pickle.PickleBuffer] = []  # Exemption: pickle protocol-5 buffer_callback sink.
    stream = pickle.dumps(Carrier(payload), protocol=5, buffer_callback=blocks.append)
    return (
        Error("<empty-view>")
        if memoryview(payload).nbytes == 0
        else Ok(Frame(body=zstd.compress(stream, level=level, zstd_dict=shared), blocks=tuple(blocks), digest=hashlib.sha256(stream).hexdigest()))
    )
```

## [06]-[ITERATION_AND_FUNCTIONAL]

[ITERATION]:
- Owner: `itertools.batched(strict=...)`, `functools.Placeholder`, and `collections.Counter.total`.
- Replace: local chunk/batch helper loops, lambda wrappers for partial argument gaps, `sum(counter.values())` cardinality folds, and post-truncation length asserts.
- Gate: the arity invariant on a paired or mapped iteration is the chooser's `zip(strict=True)`/`map(strict=True)` form `language.md` owns; this card owns the chunking, partial-application, and multiset-fold primitives that retire a local loop, never that arity-form law.
- Rule: `itertools.batched(it, n, strict=True)` raises on a final short batch instead of returning it silently, and `Counter.total()` is the multiset cardinality a `sum(counter.values())` fold restates while `Counter.subtract` (the identity-card owner) keeps the signed delta the `-` operator clamps away.
- Rule: `functools.Placeholder` fills a leading or interior `partial` gap, but a trailing `Placeholder` raises `TypeError` at construction, so the held arguments stay left-justified and a fully-trailing bind uses a direct call.

[SCOPED_CONTEXT]:
- Owner: the `ContextVar.set` token-as-context-manager.
- Replace: a `reset(token)` call paired in a `finally` block.
- Rule: `ContextVar.set(value)` returns a `Token` usable directly as a `with` context manager that restores the prior value on exit, retiring the `token = cv.set(v); try: ... finally: cv.reset(token)` ladder; this is the same-task scoped-restore primitive, distinct from `runtime.md`'s `copy_context().run` which carries `ContextVar` state across a thread or guard boundary.

```python conceptual
from collections import Counter
from collections.abc import Callable, Iterable
from functools import Placeholder, partial
from itertools import batched
from typing import Literal

from beartype import beartype
from expression import Error, Ok, Result
from expression.collections import Block

type WindowFault = Literal["<ragged-window>"]


@beartype
def windowed[T](stream: Iterable[T], width: int, weigh: Callable[[tuple[T, ...], int], int], floor: int, /) -> Result[tuple[Block[int], int], WindowFault]:
    score = partial(weigh, Placeholder, floor)
    try:  # Exemption: strict= signals the ragged tail only by raising.
        scored = Block.of_seq(batched(stream, width, strict=True)).map(score)
    except ValueError:
        return Error("<ragged-window>")
    return Ok((scored, Counter(scored).total()))
```

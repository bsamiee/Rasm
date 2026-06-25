# [PYTHON_SYSTEM_APIS]

This page is the stdlib-owner-replacement law: the high-churn surface where a yearly standard-library delta retires a local helper, kept disjoint from the stable language-form law so the language page never churns with a stdlib addition. A standard-library API replaces local machinery only when it owns the concern; it never replaces an `expression` rail, `pydantic`/`msgspec` admission, the numeric route owners, or the structured-concurrency owner. Each card names the owning stdlib surface and the local loop, wrapper, or pattern it deletes, and a snippet composes that surface under the `expression` rail. A snippet whose stdlib core leaves a boundary type-shape implicit carries a `beartype` contract proving that shape at the seam, the value-level invariant the types cannot carry staying on the `Result` rail; the violation-redirecting contract weave — `beartype(conf=BeartypeConf(violation_type=...))` folded onto the fault — is `surfaces-and-dispatch.md`'s aspect and is composed there, never re-derived here. Every member is fixed at the active interpreter surface `language.md` declares — a regressed owner moves to the language page's replacement column with the older spelling, and nothing here predates that floor. The platform-forced statement seams are named where they appear: the `pickle` protocol-5 `buffer_callback` sink collecting each out-of-band block, the `os.readinto` fill of a caller-owned buffer, the `heapq` in-place heapify, and the `try`/`except` converting a `strict=`-mode iterator's ragged-tail raise into the rail — each is the signal the stdlib API forces, and each still leaves the result an `expression` `Result`.

## [01]-[SMELL_LOOKUP]

This table is a lookup by repeated local smell; the owning card states the placement law and the spelling it deletes.

| [INDEX] | [SMELL]                            | [OWNER]                         |
| :-----: | :--------------------------------- | :------------------------------ |
|  [01]   | stringly `os.walk`/`os.path` flow  | `pathlib.Path` algebra          |
|  [02]   | `shutil` transfer wrapper          | `Path.copy_into`/`move_into`    |
|  [03]   | repeated `stat()` probe per check  | `Path.info` one-syscall cache   |
|  [04]   | `datetime.strptime` then slicing   | `date.strptime`/`time.strptime` |
|  [05]   | float math on an integer algorithm | `math.integer`/`math.fma`       |
|  [06]   | `zip` product fold                 | `math.sumprod`                  |
|  [07]   | `Fraction` constructor branch      | `Fraction.from_number`          |
|  [08]   | timestamp-prefixed UUID wrapper    | `uuid.uuid7`                    |
|  [09]   | negated-priority heap wrapper      | `heapq` `_max` family           |
|  [10]   | subprocess or bespoke zstd adapter | `compression.zstd`              |
|  [11]   | `os.read` copy slice per call      | `os.readinto`                   |
|  [12]   | `bytes(buf)` plus `buf.clear()`    | `bytearray.take_bytes`          |
|  [13]   | copy-heavy pickle blob             | protocol-5 out-of-band buffers  |
|  [14]   | local chunk/batch helper loop      | `itertools.batched`             |
|  [15]   | `sum(counter.values())` fold       | `Counter.total`                 |
|  [16]   | `reset(token)` in a `finally`      | `ContextVar.set` token-as-`with`|

## [02]-[PATHS_AND_FILES]

[PATH_ALGEBRA]:
- Owner: `pathlib.Path`/`PurePath` — `Path.walk`, `Path.copy`/`copy_into`, `Path.move`/`move_into`, `Path.info`, `Path.from_uri`, `PurePath.full_match`, `PurePath.with_segments`, `os.path.splitroot`, and `os.path.realpath(strict=os.path.ALLOW_MISSING)`.
- Gate: `recurse_symlinks=`/`follow_symlinks=` carries symlink policy as a value; `Path.info` caches one `stat` on a single `_Info` accessor, so `exists`/`is_dir`/`is_file`/`is_symlink` share one syscall instead of one probe each.
- Rule: `Path.copy_into`/`move_into` target a directory and keep the source name — the spelling a rooted right segment would corrupt — so the transfer verb is a `frozendict` value over the bound method, never a `shutil` wrapper branching on a `copy: bool`; `Path.walk` yields `(dirpath, dirnames, filenames)` and replaces the `os.walk` string idiom end to end.
- Reject: a stringly `os.walk` flow; a `shutil.copy`/`move` wrapper; one `stat()` per `exists`/`is_dir` check; drive/root string slicing for `os.path.splitroot`; a symlink-prefix resolution loop for `realpath(strict=ALLOW_MISSING)`.

[FILE_IO]:
- Owner: `NamedTemporaryFile(delete_on_close=...)`, `shutil.rmtree(onexc=...)`, `importlib.resources.as_file`, `tomllib` (TOML 1.1.0), and `mimetypes.guess_file_type`.
- Gate: persisted text I/O states `encoding="utf-8"` (or `encoding="locale"` only at a genuine locale boundary), never an implicit default at a durable seam.
- Rule: `importlib.resources.as_file` materializes a packaged resource to a real path inside a `with` scope and reclaims it on exit — the `__file__`-relative computation it retires; a config payload parses through `tomllib.load`/`loads`, never a `tomli` backport the active interpreter subsumes; `mimetypes.guess_file_type` takes the path directly where `guess_type` re-parses a URL.
- Reject: `mkstemp` unlink ladders; an `onerror` tuple handler for `rmtree(onexc=...)`; a `__file__` extraction loop; a `tomli` shim; path use of `mimetypes.guess_type`.

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
- Owner: `re` with `re.PatternError`, `re.RegexFlag` composition, and compiled module-level patterns.
- Rule: a structural grammar compiles once to a module-level `Pattern` and the flags compose as one `RegexFlag` value (`re.MULTILINE | re.VERBOSE`), never a re-compiled inline string per call; a parse-error path catches `re.PatternError`, the precise subtype, never the broad `re.error` alias.
- Reject: an inline `re.compile` inside the call; a flag passed as a bare `int` the body re-derives; a broad `except re.error` masking a grammar bug as input failure.

[DATETIME]:
- Owner: `datetime.date.strptime`, `datetime.time.strptime`, and timezone-aware `datetime`.
- Boundary: a wire timestamp admits to an aware `datetime` at the seam and the interior carries the aware value; `date.strptime`/`time.strptime` return the component type directly where `datetime.strptime(...).date()`/`.time()` parses the whole stamp only to discard half.
- Reject: `datetime.strptime` followed by `.date()`/`.time()` slicing; naive datetime arithmetic past the seam.

[TEMPLATE_RENDER]:
- Owner: t-string processors over `string.templatelib.Template` consumed at the render boundary.
- Boundary: template structure — segments, interpolations, conversions, format specs — is the language-form concern `language.md` owns; this card owns only the render-time consumption of the already-built `Template`, so a processor reads `.strings`/`.interpolations` and never re-parses rendered text.
- Reject: f-string pre-parsing; rendered-string reparsing; string concatenation hiding template policy.

## [04]-[NUMERIC_PRIMITIVES]

[SCALAR_MATH]:
- Owner: `math.integer`, `math.fma`, `math.fmax`/`fmin`, `math.isnormal`/`issubnormal`/`signbit`, `math.sumprod`, `fractions.Fraction.from_number`, and `fractions.Fraction.is_integer`.
- Gate: `math.fma(a, b, c)` is one correctly-rounded fused multiply-add where `a * b + c` rounds twice; `math.sumprod(p, q)` is the fused dot product a `sum(x * y for ...)` fold loses accuracy and speed to.
- Rule: `Fraction.from_number(value)` admits a `float`/`Decimal` to an exact rational in one call where a constructor branch ladder dispatches on the input kind; `Fraction.is_integer()` is the `denominator == 1` integrality probe as a method, never re-derived from reduced terms.
- Boundary: every array, matrix, density-estimate, and seeded-sampling concern routes to `algorithms.md`; this card owns scalar invariants on stdlib numeric primitives only, never a sample-level estimator.
- Reject: float math on an integer algorithm; a rounded multiply-add; a NaN-aware min/max wrapper; a bit-level float probe; a `zip` product fold; a `Fraction` constructor branch; a `denominator == 1` re-derivation.

[IDENTITY_AND_HEAP]:
- Owner: `uuid.uuid7`, `uuid.NIL`, `uuid.MAX`, `Counter.subtract`, the max-heap `heapq` family `heapify_max`/`heappush_max`/`heappop_max`/`heapreplace_max`, and `operator.is_none`/`is_not_none`.
- Rule: `uuid.uuid7` is monotonic within a timestamp tick, so a time-ordered key carries its own sort order and needs no separate sequence column; the minted value is compared against a prior before binding, so the successor is `max(minted, prior)` rather than a blind overwrite.
- Rule: the `_max` heap family is the native max-priority surface a value-negation `heappush` wrapper simulates, and `Counter.subtract` keeps the signed count delta the multiset `-` operator clamps to positive and a paired-iteration fold restates by hand.
- Reject: a timestamp-prefixed UUID wrapper; a magic UUID boundary literal where `uuid.NIL`/`MAX` name it; a manual count-difference fold; a negated-priority heap; a `lambda x: x is None`.

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
    identity, heap = max(uuid7(), prior), list(weights)
    heapify_max(heap)  # Exemption: heapq mutates the buffer in place before the railed return.
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
                identity=identity,
                weighted=exact,
                corrected=fma(float(weights[0]), float(values[0]), float(exact)),
                peak=heappop_max(heap),
            )
        )
    )
```

## [05]-[BINARY_AND_INTEGRITY]

[BUFFERS]:
- Owner: `array`/`memoryview` complex codes (`Zf`/`Zd`) with `memoryview.index`/`count`, `inspect.BufferFlags`, `os.readinto`, `bytearray.take_bytes`, and pickle protocol-5 out-of-band buffers.
- Rule: a protocol-5 reduction routes its payload through `pickle.PickleBuffer`, the `buffer_callback` collects each out-of-band block, and `loads(..., buffers=...)` restores the view without copying the numeric body; `os.readinto(fd, buffer)` fills a caller-owned `bytearray` in place where `os.read` allocates a fresh slice, and `bytearray.take_bytes` moves the backing store out in one operation where `bytes(buf)` then `buf.clear()` copies and then zeroes.
- Boundary: `collections.abc.Buffer` is the protocol-form owner `language.md` declares; the live-memory borrow window — `ctypes.memoryview_at` opened, copied, and `view.release()`-closed before the foreign free, any zero-copy ownership held across an `await` — is the FFI-lifetime concern `boundaries.md` owns. This card owns the stdlib in-process buffer-surface selection only.
- Reject: a struct-packed complex buffer where the `Zf`/`Zd` code is native; a magic integer in place of `inspect.BufferFlags`; an `os.read` copy slice; `bytes(buf)` plus `clear()`; a copy-heavy pickle blob.

[CODECS_AND_DIGEST]:
- Owner: `compression.zstd` (with `ZstdDict`, `train_dict`, `CompressionParameter`), `base64.z85encode`/`z85decode`, the base-N decode controls (`canonical=`/`padded=`/`ignorechars=` on `b32decode`/`b64decode`), the base-N `wrapcol=` encode control, `hashlib.file_digest`, and `zlib.adler32_combine`/`crc32_combine`.
- Gate: a wire digest streams through `hashlib.file_digest` (file) or one `hashlib` one-shot (in-memory); a non-cryptographic cache key uses a fast non-cryptographic hash, never SHA; a repeated-corpus payload trains one `ZstdDict` rather than recompressing a shared prefix per message.
- Rule: `canonical=True` rejects non-canonical base-N input at decode instead of a post-decode padding-bit check, and `wrapcol=` is an encode-side line-width control that never appears on a decode call.
- Reject: a subprocess or bespoke zstd adapter; a local Z85 codec; a padding-bit postcheck; manual line-wrap formatting; a chunked hash loop; a recompress-to-checksum loop.

```python conceptual
import compression.zstd as zstd
import hashlib
import os
import pickle
from collections.abc import Buffer, Callable
from dataclasses import dataclass
from typing import Literal

from beartype import beartype
from expression import Error, Ok, Result

type FrameFault = Literal["<empty-span>", "<short-read>"]


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
def framed(fd: int, span: int, *, shared: zstd.ZstdDict | None = None, level: int = 3) -> Result[Frame, FrameFault]:
    if span == 0:
        return Error("<empty-span>")
    sink = bytearray(span)
    read = os.readinto(fd, sink)  # Exemption: os.readinto fills the caller-owned buffer in place.
    if read < span:
        return Error("<short-read>")
    blocks: list[pickle.PickleBuffer] = []  # Exemption: protocol-5 buffer_callback out-of-band sink.
    stream = pickle.dumps(Carrier(sink.take_bytes()), protocol=5, buffer_callback=blocks.append)
    return Ok(Frame(body=zstd.compress(stream, level=level, zstd_dict=shared), blocks=tuple(blocks), digest=hashlib.sha256(stream).hexdigest()))
```

## [06]-[ITERATION_AND_CONTEXT]

[ITERATION]:
- Owner: `itertools.batched(strict=...)`, `functools.Placeholder`, and `collections.Counter.total`.
- Gate: the arity invariant on a paired or mapped iteration is the chooser's `zip(strict=True)`/`map(strict=True)` form `language.md` owns; this card owns the chunking, partial-application, and multiset-fold primitives that retire a local loop, never that arity-form law.
- Rule: `itertools.batched(it, n, strict=True)` raises on a final short batch instead of returning it silently, and `Counter.total()` is the multiset cardinality a `sum(counter.values())` fold restates while `Counter.subtract` keeps the signed delta the `-` operator clamps away.
- Rule: `functools.Placeholder` fills a leading or interior `partial` gap, but a trailing `Placeholder` raises `TypeError` at construction, so held arguments stay left-justified and a fully-trailing bind uses a direct call.
- Reject: a local chunk/batch loop; a lambda wrapper for a partial argument gap; a `sum(counter.values())` fold; a post-truncation length assert.

[SCOPED_CONTEXT]:
- Owner: the `ContextVar.set` token-as-context-manager.
- Rule: `ContextVar.set(value)` returns a `Token` usable directly as a `with` context manager that restores the prior value on exit, retiring the `token = cv.set(v); try: ... finally: cv.reset(token)` ladder; this is the same-task scoped-restore primitive, distinct from `runtime.md`'s `copy_context().run` which carries `ContextVar` state across a thread or guard boundary.
- Reject: a `reset(token)` paired in a `finally`; a manual save-and-restore of the prior value.

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

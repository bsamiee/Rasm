# [PY_CAD_SPOOL]

`ProviderPolicy` is the admitted budget every served call reads, and this owner rules the custody each call's paths obey from admission to retirement. One `msgspec.convert` mints the budgets at the composition root, one derivation turns the caller's stated deadline into the scope the spine opens, one operation directory holds every input and output path a call touches, and one boundary converts the artifact helper's raises and rails into `CadFault`.

Runtime `transport/artifact` owns identity, extent, framing, and spool custody, and `ArtifactTransfer` reads the live `anyio.current_effective_deadline` for every dial, so no window is ever re-threaded through a signature. `faults#ROWS` supplies `POLICY_SHAPE`, `SOURCE_BUDGET`, `SOURCE_SHAPE`, `ARTIFACT_PROOF`, and `ARTIFACT_ADMISSION`; `lane#LANE` owns the source shapes these rows resolve into, and `provider#PROVIDER` opens the scope and the directory this page rules.

## [01]-[INDEX]

- [02]-[POLICY]: Admitted budget struct, its bounded fields, the one convert that mints it, and the compression roster.
- [03]-[BUDGET]: Single deadline read, the scope it opens, and the law that no inner dial re-threads a window.
- [04]-[SPOOL]: Source resolution under budget, the call-owned path lifecycle, and the seal, publish, and confirm chain.

## [02]-[POLICY]

- Owner: `ProviderPolicy` carries every admitted budget as an `Annotated[..., Meta(...)]` bound, and `admitted` is its one mint.
- Law: `Meta` guards the CONVERT and NEVER the constructor — `ProviderPolicy(read_max_bytes=-5)` constructs happily and so does a string in an integer slot, so the bound is decode-time law and the constructor is left to `admitted` alone.
- Law: `msgspec.convert` on an already-`ProviderPolicy` value short-circuits by IDENTITY and validates nothing, so the admission argument is a raw mapping and re-admitting an admitted row proves exactly nothing.
- Law: five sequential `raise ValueError` statements over a multi-cause domain are the deleted form — the ladder returned no carrier, named its field only in a bare string, refused nothing the type system reads, and re-derived at each call site whether construction had been legal.
- Law: `ValidationError` subclasses `DecodeError` subclasses `ValueError`, so the specific arm orders first; it reports the FIRST failing field alone and this page never claims it accumulates.
- Law: a finite `le` bound on `call_seconds` subsumes the `isfinite` probe, because `inf` fails the bound — the probe was a second guard for a condition the bound already states.
- Law: `CONTRACT_CEILING` is the corpus `ArtifactRef.artifact_bytes` ceiling, so no admitted budget may exceed the extent the wire itself refuses; a policy row above it describes an artifact protovalidate rejects.
- Law: `COMPRESSIONS` seats zstd ahead of gzip with identity always surviving negotiation, and an unset roster negotiates gzip and identity alone — the declared `zstandard` distribution therefore reaches the wire only because this row is passed to both the mount and every dial.
- Growth: a new budget is one field with its `Meta` bound and nothing else; a new codec is one `Compression` value on `COMPRESSIONS`.
- Boundary: the composition root reads the mapping off its own settings surface; this page admits it, states its bounds, and sources none of it.

```python signature
from collections.abc import AsyncIterator, Awaitable, Callable, Mapping
from contextlib import AsyncExitStack
from typing import Annotated, Final, Protocol, assert_never

from connectrpc.compression import Compression
from connectrpc.compression.gzip import GzipCompression
from connectrpc.compression.zstd import ZstdCompression
from connectrpc.errors import ConnectError
from expression import Error, Ok, Result
from msgspec import Meta, Struct, ValidationError, convert
from protobuf import Message
from rasm.contracts import AdmissionError
from rasm.runtime.transport.artifact import (
    ArtifactCycle,
    ArtifactEmpty,
    ArtifactError,
    ArtifactExtent,
    ArtifactIdentity,
    ArtifactOpaque,
    ArtifactReference,
    ArtifactRefusal,
    ArtifactResealed,
    ArtifactSink,
    ArtifactTransfer,
    ArtifactWidth,
    OwnedArtifact,
    references,
    rendered,
)
from rasm.contracts.rasm.contracts.artifact.artifact_pb import (
    ArtifactRef,
    FetchRequest,
    FetchResponse,
    PutRequest,
    PutResponse,
)

from rasm.cad.faults import ARTIFACT_ADMISSION, ARTIFACT_PROOF, POLICY_SHAPE, SOURCE_BUDGET, SOURCE_SHAPE, CadFault, CadRail
from rasm.cad.service.lane import SourceRow

# --- [TYPES] ----------------------------------------------------------------------------

# unadmitted mapping the composition root reads off its own settings surface
type PolicyRow = Mapping[str, object]


# --- [CONSTANTS] ------------------------------------------------------------------------

# corpus `ArtifactRef.artifact_bytes` ceiling: no admitted budget may describe an artifact protovalidate refuses
CONTRACT_CEILING: Final[int] = 1 << 30
# finite ceiling that DELETES the `isfinite` probe, because `inf` fails an `le` bound on its own
CALL_CEILING: Final[float] = 3600.0


# --- [MODELS] ---------------------------------------------------------------------------


class ProviderPolicy(Struct, frozen=True, gc=False, kw_only=True):
    # `Meta` guards the CONVERT and never the constructor: `ProviderPolicy(read_max_bytes=-5, ...)` constructs
    # happily and so does a string in an integer slot, so `admitted` is the one mint and no fence calls this ctor.
    read_max_bytes: Annotated[int, Meta(gt=0, le=CONTRACT_CEILING)]
    artifact_bytes: Annotated[int, Meta(gt=0, le=CONTRACT_CEILING)]
    source_refs: Annotated[int, Meta(gt=0)]
    source_bytes: Annotated[int, Meta(gt=0, le=CONTRACT_CEILING)]
    call_seconds: Annotated[float, Meta(gt=0.0, le=CALL_CEILING)]

    @staticmethod
    def admitted(row: PolicyRow, /) -> CadRail["ProviderPolicy"]:
        # admission takes the raw MAPPING: `convert` short-circuits by identity on an already-admitted value and
        # validates nothing, so passing one back through here certifies a row nothing re-checked.
        try:
            return Ok(convert(row, type=ProviderPolicy))
        except ValidationError as refused:
            return Error(POLICY_SHAPE.at(f"cad.policy:{refused}"))


# --- [TABLES] ---------------------------------------------------------------------------

# identity always survives negotiation and an UNSET roster seats gzip beside it alone, so zstd reaches the wire
# only because this row is passed explicitly to the mount and to every dial that shares the same profile.
COMPRESSIONS: Final[tuple[Compression, ...]] = (ZstdCompression(level=3), GzipCompression(level=6))
```

## [03]-[BUDGET]

- Owner: `budget` is the one derivation from the caller's stated deadline onto the delay the spine's `fail_after` opens.
- Law: `ctx.timeout_ms` is read ONCE per call, at the outermost scope, because it is a property recomputing time REMAINING off a monotonic deadline on every read and it goes negative past that deadline with no clamp — two reads inside one method are two different numbers by construction.
- Law: reading it twice and flooring the second at `max(1, int(...))` is the deleted form, and it was a live defect: the second read forwarded a window nobody measured, and the floor certified one millisecond of budget to a call whose budget had already elapsed.
- Law: no inner call re-threads a window the enclosing scope already bounds — `ArtifactTransfer` reads `anyio.current_effective_deadline` itself for each dial, so the forwarded `timeout_ms` argument the old fence threaded is deleted rather than recomputed.
- Law: an absent caller deadline settles on `call_seconds`, so an unbounded handler has no seat and every served call is bounded by an admitted ceiling whether or not the caller stated one.
- Boundary: `provider#PROVIDER` opens the scope and converts its `TimeoutError`; this page derives the delay and owns nothing that cancels.

```python signature
# --- [OPERATIONS] -----------------------------------------------------------------------


def budget(timeout_ms: float | None, ceiling: float, /) -> float:
    # `timeout_ms` arrives from the spine's SINGLE read of `ctx.timeout_ms`. It recomputes time REMAINING on each
    # read with no clamp below zero, so an elapsed budget floors at zero here rather than at a forged millisecond.
    return ceiling if timeout_ms is None else min(ceiling, max(0.0, timeout_ms / 1000.0))
```

## [04]-[SPOOL]

- Owner: `sources` resolves every admitted reference onto a call-owned path under the admitted budget, and `published` seals, publishes, and confirms the one output the call produced.
- Law: `ArtifactPort` is the generated `ArtifactService` client contract this page dials — `put` sends an `AsyncIterator[PutRequest]` and answers one `PutResponse`, `fetch` answers a `FetchResponse` stream — and `ArtifactTransfer` composes both; this page frames nothing, mints no envelope, and keeps no second store.
- Law: the port is declared HERE because the contracts helper's own client protocol is private, so a consumer either restates the generated members structurally or couples to a name it may not import; the structural port is the honest half of that choice and it admits the generated client and a test double alike.
- Law: the provider owns every input and output path — `output(suffix=)` mints ONE operation directory per call and retires it under a shielded cancel scope on success, refusal, elapsed budget, and worker death alike, so no path outlives the call that made it.
- Law: each fetched input enters the call's `AsyncExitStack`, so the inputs unwind in REVERSE before that directory retires, and an acquisition that refuses mid-build releases every earlier handle before it propagates.
- Law: `references` answers a rail, so the traversal's foreign-extension and cycle refusals are the CALLER's own message shape and grade `INVALID_ARGUMENT` beside the source rows, never `INTERNAL` as a provider defect.
- Law: `seal()` takes NO source, because the native kernel already wrote `sink.path` — the seal proves those octets in place and copies nothing, a refused seal leaves custody open, and only a proved seal consumes the single use.
- Law: `ArtifactTransfer.publish` is the composed publish leg and it confirms the service echoed the same reference on both axes, so a reply's `ArtifactRef` names an artifact that is durable before the reply exists.
- Law: the fetch fold is SEQUENTIAL because each fetch is an async context whose custody must outlive the step that opened it, which no task-group fan can own; this is the concurrency page's stated statement-bearing exemption, not an unfolded loop.
- Law: a `ConnectError` the artifact client raises stays the PEER's fault — its code and detail belong to the peer's span, and this leg refuses TERMINAL because narrowing a re-drive posture is always safe where widening one is not.
- Cases: `_refusal` maps the closed `ArtifactRefusal` family onto three rows by WHOSE law broke — the caller's message shape, this provider's own custody discipline, or the artifact's octet proof — and `assert_never` closes the dispatch so a new refusal case breaks the build instead of silently taking a catch-all arm.
- Growth: a new artifact law is one `ArtifactRefusal` case at the contracts helper and one arm here; a new budget axis is one `ProviderPolicy` field read by `_budgeted`.
- Boundary: atomic storage, frame width, digest computation, and reference confirmation are the contracts helper's; this page composes them and re-implements none.

```python signature
# --- [BOUNDARIES] -----------------------------------------------------------------------


class ArtifactPort(Protocol):
    # generated `ArtifactService` client's own two members, declared structurally because the contracts helper's
    # client protocol is private; the generated client and a test double both satisfy this shape.
    def fetch(self, request: FetchRequest, *, timeout_ms: int | None = None) -> AsyncIterator[FetchResponse]: ...

    async def put(self, request: AsyncIterator[PutRequest], *, timeout_ms: int | None = None) -> PutResponse: ...


# every raise the artifact seam can still produce, ordered specific-first at each site that names the tuple
_TRANSFER_RAISES: Final[tuple[type[Exception], ...]] = (ArtifactError, AdmissionError, ConnectError)


def _refusal(refused: ArtifactRefusal, /) -> CadFault:
    match refused:
        case ArtifactOpaque() | ArtifactCycle():
            # traversal met a foreign-extension slot or a self-referencing message: both are the CALLER's own body
            # shape, so they grade beside the source rows rather than as a defect of this provider.
            return SOURCE_SHAPE.at(f"cad.sources.{rendered(refused)}")
        case ArtifactResealed():
            return ARTIFACT_ADMISSION.at(f"cad.artifact.{rendered(refused)}")
        case ArtifactEmpty() | ArtifactExtent() | ArtifactWidth() | ArtifactIdentity() | ArtifactReference():
            return ARTIFACT_PROOF.at(f"cad.artifact.{rendered(refused)}")
        case _ as unreachable:
            assert_never(unreachable)


def _raised(refused: ArtifactError | AdmissionError | ConnectError, /) -> CadFault:
    match refused:
        case ArtifactError(refusal=refusal):
            return _refusal(refusal)
        case AdmissionError(phase=phase):
            # only the INJECTED client's `BodyAdmission(CLIENT)` reaches here: `BodyAdmission(SERVER)` refuses a
            # served body as a `ConnectError` the interceptor raises before any handler body runs.
            return ARTIFACT_ADMISSION.at(f"cad.artifact.admission.{phase.value}")
        case ConnectError(code=code):
            # peer's own code and detail stay on its own span; this leg refuses TERMINAL because narrowing a
            # re-drive posture is always safe where widening one is not.
            return ARTIFACT_PROOF.at(f"cad.artifact.remote.{code.value}")


# --- [OPERATIONS] -----------------------------------------------------------------------


async def bound[T, R](rail: CadRail[T], arrow: Callable[[T], Awaitable[CadRail[R]]], /) -> CadRail[R]:
    # `Result.bind` cannot await, so this is the package's one asynchronous kleisli step; each async stage threads
    # through it instead of unwrapping the carrier into an `if` ladder that re-derives the error arm every time.
    match rail:
        case Result(tag="ok", ok=held):
            return await arrow(held)
        case Result(tag="error", error=refused):
            return Error(refused)


def _budgeted(found: tuple[ArtifactRef, ...], policy: ProviderPolicy, /) -> CadRail[tuple[ArtifactRef, ...]]:
    extent = sum(artifact.artifact_bytes for artifact in found)
    return (
        Ok(found)
        if len(found) <= policy.source_refs and extent <= policy.source_bytes
        else Error(SOURCE_BUDGET.at(f"cad.sources:{len(found)}/{extent}"))
    )


async def _fetched(artifact: ArtifactRef, artifacts: ArtifactTransfer, stack: AsyncExitStack, /) -> CadRail[OwnedArtifact]:
    # fetched spools enter the CALL's exit stack, so every input unwinds before the output directory retires;
    # `fetch` reads the live effective deadline itself and this fence forwards no window of its own.
    try:
        received = await stack.enter_async_context(artifacts.fetch(artifact))
    except _TRANSFER_RAISES as raised:
        return Error(_raised(raised))
    return received.map_error(_refusal)


async def sources(
    message: Message,
    artifacts: ArtifactTransfer,
    stack: AsyncExitStack,
    policy: ProviderPolicy,
    /,
) -> CadRail[tuple[SourceRow, ...]]:
    match references(message).map_error(_refusal).bind(lambda found: _budgeted(found, policy)):
        case Result(tag="error", error=refused):
            return Error(refused)
        case Result(tag="ok", ok=found):
            rows: list[SourceRow] = []
            for artifact in found:  # Exemption: each fetch is a context outliving its step, so no fan can own it
                held = await _fetched(artifact, artifacts, stack)
                if held.is_error():
                    return Error(held.error)
                rows.append((artifact.sha256, str(held.ok.path)))
            return Ok(tuple(rows))


async def published(sink: ArtifactSink, artifacts: ArtifactTransfer, /) -> CadRail[ArtifactRef]:
    # `seal()` takes no source because the native kernel already wrote `sink.path`; `publish` then frames those
    # sealed octets, re-proves them, and confirms the service echoed the same reference on both axes.
    try:
        match await sink.seal():
            case Result(tag="ok", ok=owned):
                return (await artifacts.publish(owned)).map_error(_refusal)
            case Result(tag="error", error=refused):
                return Error(_refusal(refused))
    except _TRANSFER_RAISES as raised:
        return Error(_raised(raised))
```

## [05]-[RESEARCH]

- [ARTIFACT_REMOTE]-[OPEN]: earns a peer `ConnectError` its own `faults#ROWS` row lifting the peer's stated recovery; verify at `transport.md` `[FAULT_DETAIL]`.

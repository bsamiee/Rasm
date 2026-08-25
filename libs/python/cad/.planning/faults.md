# [PY_CAD_FAULTS]

`CadFault` is the provider's one refusal value and `CadRail` the carrier every owner returns. One frozen row set spells each refusal shape once — its leg, its frozen `rasm.cad` case ordinal, the Connect code it crosses under, and the producer's own three-arm recovery verdict — so a leg refusal, its wire projection, and its re-drive posture read from one declaration instead of parallel exception families and a translation tax paid at every seam.

This owner imports no sibling and every sub-domain reaches it, so the row roster seats below the exchange codecs, the B-rep owners, and the served lane alike. `expression` supplies the carrier and `msgspec` the frozen row and fault records; both pickle by reference, so a refusal minted inside the `anyio.to_process` worker crosses the seam as a VALUE and no custom exception transports an inner fault across it. `FaultRecovery`'s throttled arm IS `google.rpc.RetryInfo`, so estate detail and standard detail carry ONE message and never two projections that can disagree.

## [01]-[INDEX]

- [02]-[ROWS]: Closed leg, case, and recovery vocabularies, the frozen row roster, and the fault value they mint.
- [03]-[PROJECTION]: One wire projection folding a row onto `FaultDetail` and the `FaultRecovery` arm its standard detail seat re-packs.

## [02]-[ROWS]

- Owner: `CadFault` — the one refusal value; `FaultRow.at` is its only mint and every owner returns `CadRail[T]`.
- Cases: `CadCase` is the FROZEN ordinal set under `domain="rasm.cad"` — an issued code outlives its row, so a member appends and never renumbers.
- Law: `CadLeg` and `CadCase` are independent columns — four legs raise `INPUT` and one leg raises five cases, so either collapse loses a join axis.
- Law: a per-leg `kind` `Literal` is the deleted form — nested subsets forced a re-wrap at each leg and rebuilt the ordinal at the serve edge.
- Law: `code` and `case` are independent axes — `SOURCE_BUDGET` and `SOURCE_SHAPE` share case `INPUT` while grading different Connect codes.
- Law: policy belongs to the row, never the case — one case crossing under two codes cannot decide either grading alone.
- Law: `Recovery` mirrors `FaultRecovery.kind` arm for arm, so the verdict is a VALUE the row carries and the third arm stays spellable.
- Law: a `transient: bool` knob is the deleted form — it collapsed three arms onto two and made the stated window unreachable from every row.
- Law: a windowed row is DERIVED, never rostered — `FaultRow.windowed` reads the admitted `call_seconds`, so the delay is a measured worst case.
- Law: `at` returns the fault and never raises it — a raise survives only at the `to_process` crossing and the serve edge, each naming its seam.
- Growth: a refusal shape is one `FaultRow`; a wire case, one `CadCase` member; a posture, one `Recovery` case beside one `_PROJECTED` row.
- Boundary: refusal SHAPE alone lives here — correlation, stamp, and tenant arrive as `FaultStamp`, and the raise is `service/provider`'s collapse.

```python signature
from collections.abc import Callable
from enum import EnumCheck, IntEnum, StrEnum, verify
from typing import Final, Literal

from builtins import frozendict
from connectrpc.code import Code
from expression import Error, Ok, Result, case, tag, tagged_union
from msgspec import Struct
from protobuf import Message, Oneof
from protobuf.wkt import Duration, Empty
from rasm.contracts.google.rpc.error_details_pb import RetryInfo
from rasm.contracts.rasm.contracts.clock.hlc_pb import Hlc
from rasm.contracts.rasm.contracts.fault.fault_pb import FaultDetail, FaultRecovery

# --- [TYPES] ----------------------------------------------------------------------------

type CadRail[T] = Result[T, "CadFault"]

DOMAIN: Final[str] = "rasm.cad"


class CadLeg(StrEnum):
    EXCHANGE = "exchange"
    BREP = "brep"
    METROLOGY = "metrology"
    TESSELLATION = "tessellation"
    SERVICE = "service"


@verify(EnumCheck.UNIQUE)
class CadCase(IntEnum):
    INPUT = 1
    PROTOCOL = 2
    KERNEL = 3
    OUTPUT = 4
    ARTIFACT = 5
    DEADLINE = 6
    WORKER = 7
    BUSY = 8


# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class Recovery:
    tag: Literal["terminal", "transient", "retry_after"] = tag()
    terminal: None = case()
    transient: None = case()
    retry_after: float = case()


TERMINAL: Final[Recovery] = Recovery(terminal=None)
TRANSIENT: Final[Recovery] = Recovery(transient=None)


class FaultRow(Struct, frozen=True, gc=False):
    leg: CadLeg
    case: CadCase
    code: Code
    recovery: Recovery

    def at(self, coordinate: str, /) -> "CadFault":
        return CadFault(row=self, coordinate=coordinate)

    def windowed(self, seconds: float, /) -> "FaultRow":
        return FaultRow(leg=self.leg, case=self.case, code=self.code, recovery=Recovery(retry_after=seconds))


class CadFault(Struct, frozen=True, gc=False):
    row: FaultRow
    coordinate: str


class FaultStamp(Struct, frozen=True, gc=False):
    correlation: bytes
    stamp: Hlc
    tenant: str | None = None

    @staticmethod
    def of(correlation: bytes, stamp: Hlc, tenant: str | None = None, /) -> CadRail["FaultStamp"]:
        return (
            Ok(FaultStamp(correlation=correlation, stamp=stamp, tenant=tenant))
            if len(correlation) == 16
            else Error(STAMP_SHAPE.at(f"fault-stamp.correlation:{len(correlation)}"))
        )


# --- [ROWS] -----------------------------------------------------------------------------

STEP_READ: Final[FaultRow] = FaultRow(leg=CadLeg.EXCHANGE, case=CadCase.INPUT, code=Code.INVALID_ARGUMENT, recovery=TERMINAL)
STEP_SCHEMA: Final[FaultRow] = FaultRow(leg=CadLeg.EXCHANGE, case=CadCase.PROTOCOL, code=Code.INVALID_ARGUMENT, recovery=TERMINAL)
STEP_WRITE: Final[FaultRow] = FaultRow(leg=CadLeg.EXCHANGE, case=CadCase.OUTPUT, code=Code.DATA_LOSS, recovery=TERMINAL)
CAF_TRANSFER: Final[FaultRow] = FaultRow(leg=CadLeg.EXCHANGE, case=CadCase.INPUT, code=Code.INVALID_ARGUMENT, recovery=TERMINAL)
CAF_ROOTS: Final[FaultRow] = FaultRow(leg=CadLeg.EXCHANGE, case=CadCase.INPUT, code=Code.INVALID_ARGUMENT, recovery=TERMINAL)
BREP_INPUT: Final[FaultRow] = FaultRow(leg=CadLeg.BREP, case=CadCase.INPUT, code=Code.INVALID_ARGUMENT, recovery=TERMINAL)
BREP_KERNEL: Final[FaultRow] = FaultRow(leg=CadLeg.BREP, case=CadCase.KERNEL, code=Code.INTERNAL, recovery=TERMINAL)
BREP_OUTPUT: Final[FaultRow] = FaultRow(leg=CadLeg.BREP, case=CadCase.OUTPUT, code=Code.DATA_LOSS, recovery=TERMINAL)

MEASURE_DEGENERATE: Final[FaultRow] = FaultRow(leg=CadLeg.METROLOGY, case=CadCase.OUTPUT, code=Code.DATA_LOSS, recovery=TERMINAL)
CENSUS_DECODE: Final[FaultRow] = FaultRow(leg=CadLeg.METROLOGY, case=CadCase.OUTPUT, code=Code.DATA_LOSS, recovery=TERMINAL)

MESH_KERNEL: Final[FaultRow] = FaultRow(leg=CadLeg.TESSELLATION, case=CadCase.KERNEL, code=Code.INTERNAL, recovery=TERMINAL)
MESH_BUDGET: Final[FaultRow] = FaultRow(leg=CadLeg.TESSELLATION, case=CadCase.OUTPUT, code=Code.RESOURCE_EXHAUSTED, recovery=TERMINAL)
EMIT_WRITE: Final[FaultRow] = FaultRow(leg=CadLeg.TESSELLATION, case=CadCase.OUTPUT, code=Code.DATA_LOSS, recovery=TERMINAL)
EMIT_EXTENT: Final[FaultRow] = FaultRow(leg=CadLeg.TESSELLATION, case=CadCase.OUTPUT, code=Code.RESOURCE_EXHAUSTED, recovery=TERMINAL)

SOURCE_SHAPE: Final[FaultRow] = FaultRow(leg=CadLeg.SERVICE, case=CadCase.INPUT, code=Code.INVALID_ARGUMENT, recovery=TERMINAL)
SOURCE_BUDGET: Final[FaultRow] = FaultRow(leg=CadLeg.SERVICE, case=CadCase.INPUT, code=Code.RESOURCE_EXHAUSTED, recovery=TERMINAL)
POLICY_SHAPE: Final[FaultRow] = FaultRow(leg=CadLeg.SERVICE, case=CadCase.KERNEL, code=Code.INTERNAL, recovery=TERMINAL)
STAMP_SHAPE: Final[FaultRow] = FaultRow(leg=CadLeg.SERVICE, case=CadCase.KERNEL, code=Code.INTERNAL, recovery=TERMINAL)
NATIVE_INIT: Final[FaultRow] = FaultRow(leg=CadLeg.SERVICE, case=CadCase.KERNEL, code=Code.INTERNAL, recovery=TERMINAL)
NATIVE_DEFECT: Final[FaultRow] = FaultRow(leg=CadLeg.SERVICE, case=CadCase.KERNEL, code=Code.INTERNAL, recovery=TERMINAL)
ARTIFACT_PROOF: Final[FaultRow] = FaultRow(leg=CadLeg.SERVICE, case=CadCase.ARTIFACT, code=Code.DATA_LOSS, recovery=TERMINAL)
ARTIFACT_ADMISSION: Final[FaultRow] = FaultRow(leg=CadLeg.SERVICE, case=CadCase.ARTIFACT, code=Code.INTERNAL, recovery=TERMINAL)
CALL_DEADLINE: Final[FaultRow] = FaultRow(leg=CadLeg.SERVICE, case=CadCase.DEADLINE, code=Code.DEADLINE_EXCEEDED, recovery=TRANSIENT)
NATIVE_WORKER: Final[FaultRow] = FaultRow(leg=CadLeg.SERVICE, case=CadCase.WORKER, code=Code.UNAVAILABLE, recovery=TRANSIENT)
LANE_SATURATED: Final[FaultRow] = FaultRow(leg=CadLeg.SERVICE, case=CadCase.BUSY, code=Code.RESOURCE_EXHAUSTED, recovery=TRANSIENT)
```

## [03]-[PROJECTION]

- Owner: `refused` — the one fold from an admitted `(CadFault, FaultStamp)` pair onto the detail set a `ConnectError` carries.
- Law: one `_PROJECTED` row per arm mints ONE `RetryInfo` and seats it in both the `FaultRecovery.kind` oneof and the detail tail.
- Law: two tables keyed on one tag are the deleted form — a stated window drifts the moment one side's arm changes alone.
- Law: `domain` is the PRODUCING family and `case` its own ordinal, never the code, which rides `ConnectError.code` on the same refusal.
- Law: `connectrpc.Code` is a string `Enum`, so `int(code)` raises and no ordinal of it is ever a wire fact; a peer keeps the pair opaque.
- Output: `terminal` and `transient` project the detail alone and `retry_after` re-seats its own arm as one standard `RetryInfo`, so tuple length is the arm's own consequence.
- Boundary: `refused` builds the value on the refusing arm alone, so a passing call prices no detail work; `service/provider` raises it.

```python signature
_PROJECTED: Final[frozendict[str, Callable[[Recovery], tuple[Oneof, tuple[Message, ...]]]]] = frozendict({
    "terminal": lambda _held: (Oneof("terminal", Empty()), ()),
    "transient": lambda _held: (Oneof("transient", Empty()), ()),
    "retry_after": lambda held: (lambda advice: (Oneof("retry_after", advice), (advice,)))(
        RetryInfo(retry_delay=Duration.from_seconds(held.retry_after))
    ),
})


def refused(fault: CadFault, stamp: FaultStamp, /) -> tuple[Code, str, tuple[Message, ...]]:
    row = fault.row
    kind, window = _PROJECTED[row.recovery.tag](row.recovery)
    detail = FaultDetail(
        domain=DOMAIN,
        case=int(row.case),
        correlation=stamp.correlation,
        stamp=stamp.stamp,
        tenant=stamp.tenant,
        recovery=FaultRecovery(kind=kind),
    )
    return row.code, f"{row.leg}:{fault.coordinate}", (detail, *window)
```

## [04]-[RESEARCH]

(none)

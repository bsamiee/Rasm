# [PY_CAD_EXCHANGE_IDENTITY]

`identity` rules the provider's byte-stability contract: two runs of one admitted operation over identical geometry emit identical STEP octets, which is what makes `CadService.Execute` idempotent at the artifact rather than merely at the reply. This owner holds the canonical header stamp, the canonical product identity every emission carries, and the `Interface_Static` policy the whole worker process runs under.

`exchange/step#CODEC` composes `canonical` between transfer and write and proves `EMITTED` on readback, so this page seats beneath `step` inside the folder and imports nothing from it. `service/lane` applies `pinned` once per worker process and owns that gate alone, never the row set; `faults#ROWS` supplies `STEP_WRITE` and the exchange-leg `EXCHANGE_REGIME`, and every refusal is `Error(<ROW>.at(...))` on `CadRail`.

## [01]-[INDEX]

- [02]-[PINS]: Exchange controller init, the `Interface_Static` unit and schema rows, and the one fold that seats them.
- [03]-[CANONICAL]: Canonical header identity rows, canonical `StepBasic_Product` identity, and the fields left unstamped.

## [02]-[PINS]

- Owner: `_CONTROLLERS` and `_PINS` are the whole process-global exchange policy, and `pinned` is their one fold — no second site in the package calls `Interface_Static`.
- Cases: `Pin` closes over `text` and `count`, one arm per `Interface_Static` accessor pair, so a new value kind stays unspellable until `_APPLIED` grows its arm.
- Law: controller init precedes every static, because a static seated before its protocol registers reverts silently and reads back its default.
- Law: each pin sets AND reads back under one arm, so a setter reporting success without taking effect refuses instead of leaving the process silently mis-configured.
- Law: OCCT defaults length to millimetres and keeps writer schema process-global, so leaving either implicit breaks metre receipts and idempotent artifact identity together.
- Law: `write.step.schema` and `EMITTED` are one decision written twice — OCCT ordinal and wire enum — and `exchange/step#CODEC` proves they agree on every emitted file.
- Law: an unpinned writer emits `AUTOMOTIVE_DESIGN` rather than AP242, so a missing `write.step.schema` silently ships a file declaring a protocol this page never chose; that is the failure `sealed`'s re-read catches.
- Law: refusal carries `EXCHANGE_REGIME` because the exchange leg DECLARES which controllers and pins the byte-stability contract requires; `service/lane` merely applies them once and refuses under its own `NATIVE_INIT`.
- Growth: a new process-global exchange knob is one `_PINS` row; a new accessor pair is one `Pin` case beside one `_APPLIED` arm.
- Boundary: WHEN this fold runs — once per worker process, ahead of any reader, writer, property fold, or GLB export — is `service/lane`'s decision, not this owner's.

```python signature
from collections.abc import Callable
from typing import Final, Literal

from builtins import frozendict
from expression import Error, Ok, case, tag, tagged_union
from expression.collections import Block
from expression.extra.result import traverse
from OCP.APIHeaderSection import APIHeaderSection_MakeHeader
from OCP.IGESControl import IGESControl_Controller
from OCP.Interface import Interface_Static
from OCP.STEPControl import STEPControl_Controller
from OCP.StepBasic import StepBasic_Product
from OCP.StepData import StepData_StepModel
from OCP.TCollection import TCollection_HAsciiString
from rasm.contracts.gen.rasm.contracts.cad.v1.types_pb import StepProtocol

from rasm.cad.faults import EXCHANGE_REGIME, STEP_WRITE, CadRail

# --- [CONSTANTS] ------------------------------------------------------------------------

# `write.step.schema=5` selects AP242 DIS in OCCT and `EMITTED` is the same choice on the wire; `exchange/step`
# compares a readback against `EMITTED`, so the two spellings cannot drift apart unobserved.
UNIT: Final[str] = "M"
SCHEMA: Final[int] = 5
EMITTED: Final[StepProtocol] = StepProtocol.AP242


# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class Pin:
    # arm per `Interface_Static` accessor pair, so the value kind and the setter it needs are one recoverable fact
    tag: Literal["text", "count"] = tag()
    text: str = case()
    count: int = case()


# --- [POLICIES] -------------------------------------------------------------------------

# Each arm SETS and READS BACK in one expression: `SetCVal_s` answers True on a name OCCT then ignores, so the
# readback is the only evidence the process actually runs under the pin.
_APPLIED: Final[frozendict[str, Callable[[str, Pin], bool]]] = frozendict({
    "text": lambda name, pin: Interface_Static.SetCVal_s(name, pin.text) and Interface_Static.CVal_s(name) == pin.text,
    "count": lambda name, pin: Interface_Static.SetIVal_s(name, pin.count) and Interface_Static.IVal_s(name) == pin.count,
})

# Controller init registers the exchange protocols the statics key on, so it is ordered ahead of every pin.
_CONTROLLERS: Final[frozendict[str, Callable[[], bool]]] = frozendict({
    "STEPControl_Controller": STEPControl_Controller.Init_s,
    "IGESControl_Controller": IGESControl_Controller.Init_s,
})

_PINS: Final[frozendict[str, Pin]] = frozendict({
    "xstep.cascade.unit": Pin(text=UNIT),
    "write.step.unit": Pin(text=UNIT),
    "write.step.schema": Pin(count=SCHEMA),
})


# --- [OPERATIONS] -----------------------------------------------------------------------


def _started(row: tuple[str, Callable[[], bool]], /) -> CadRail[str]:
    name, init = row
    return Ok(name) if init() else Error(EXCHANGE_REGIME.at(f"occt.controller.{name}"))


def _seated(row: tuple[str, Pin], /) -> CadRail[str]:
    name, pin = row
    return Ok(name) if _APPLIED[pin.tag](name, pin) else Error(EXCHANGE_REGIME.at(f"interface-static.{name}"))


def pinned() -> CadRail[None]:
    # `traverse` short-circuits on the first refusal, which is the right disposition: a half-pinned process emits
    # millimetre geometry under an AP242 header, and reporting every casualty would not make that state usable.
    return (
        traverse(_started, Block.of_seq(_CONTROLLERS.items()))
        .bind(lambda _live: traverse(_seated, Block.of_seq(_PINS.items())))
        .map(lambda _rows: None)
    )
```

## [03]-[CANONICAL]

- Owner: `canonical` is the one mutation an emitted STEP model receives; `_HEADER` and the product constants beside it are the whole canonical identity.
- Law: `_HEADER` rows every FILE_NAME slot OCCT derives from the emitting run — name, timestamp, author, organization, preprocessor version, originating system, authorisation — so a new slot is one row and no setter floats loose in the body.
- Law: canonicalization runs AFTER `Transfer` and before `Write`, because `Transfer` is what populates FILE_NAME; on an unpopulated model `HasFn()` reads False, `NbAuthor()` reads 0, and every `Set*Value` returns cleanly while storing nothing.
- Law: that silent no-op is the forged-canonicalization shape — a page claiming byte stability while emitting every OCCT default intact — so an unsized header refuses on `STEP_WRITE` exactly as a product-less model does.
- Law: authorship rides the INDEXED setters `SetAuthorValue` and `SetOrganizationValue` at entry one, and the row carries that arity because the aggregate setters take an array this page never builds.
- Law: pinning `preprocessor_version` erases the OCCT build from the emitted octets deliberately — that slot ships the kernel version verbatim, so an upgrade moves the bytes and breaks idempotent artifact identity; build provenance lives in the deployment image instead.
- Law: every `StepBasic_Product` id, name, and description is the second family of process-derived labels, so stamping the header alone leaves a file two runs cannot reproduce.
- Law: an emission carrying no `StepBasic_Product` has no identity to canonicalize, so its octets are not stable and the write refuses rather than passing a silent no-op forward.
- Law: `Block.fold` drives the product walk because folding is the eager form; a lazy projection over the same range leaves the mutation unperformed and still returns a `Block`.
- Law: OCCT entity indices are one-based and `NbEntities` is inclusive, so the range opens at 1 and closes past the count.
- Exemption: FILE_DESCRIPTION `description` and `implementation_level` stay unstamped — both sit outside FILE_NAME, neither is probed for run-varying defaults, and neither carries an `.api` row.
- Boundary: GLB emission carries no byte-stability law on any page today; `tessellation/emission` owns that gap, and this owner asserts nothing over glTF octets.

```python signature
# --- [CONSTANTS] ------------------------------------------------------------------------

# Fixed file name, epoch timestamp, fixed authorship and authorisation, fixed toolchain identity, and one fixed
# product identity: together these replace every label OCCT derives from the emitting run, clock, or kernel build.
NAME: Final[str] = "rasm-cad"
TIMESTAMP: Final[str] = "1970-01-01T00:00:00"
AUTHOR: Final[str] = "rasm"
AUTHORISATION: Final[str] = "rasm"
ORGANIZATION: Final[str] = "rasm"
ORIGINATOR: Final[str] = "rasm-cad"
PREPROCESSOR: Final[str] = "rasm-cad"
PRODUCT_ID: Final[str] = "rasm-cad"
PRODUCT_NAME: Final[str] = "Rasm CAD artifact"
PRODUCT_DESCRIPTION: Final[str] = "Canonical B-rep exchange"


# --- [POLICIES] -------------------------------------------------------------------------

# One row per canonicalized FILE_NAME slot, carrying its value beside the setter that seats it. Author and
# organization are STEP aggregates, so their rows spell the indexed setter and its entry-one arity in place; a
# slot left off this table keeps an OCCT default, and every default here is build-varying or run-varying.
_HEADER: Final[frozendict[str, tuple[str, Callable[[APIHeaderSection_MakeHeader, TCollection_HAsciiString], None]]]] = frozendict({
    "name": (NAME, lambda header, value: header.SetName(value)),
    "time_stamp": (TIMESTAMP, lambda header, value: header.SetTimeStamp(value)),
    "author": (AUTHOR, lambda header, value: header.SetAuthorValue(1, value)),
    "organization": (ORGANIZATION, lambda header, value: header.SetOrganizationValue(1, value)),
    "originating_system": (ORIGINATOR, lambda header, value: header.SetOriginatingSystem(value)),
    "preprocessor_version": (PREPROCESSOR, lambda header, value: header.SetPreprocessorVersion(value)),
    "authorisation": (AUTHORISATION, lambda header, value: header.SetAuthorisation(value)),
})


# --- [OPERATIONS] -----------------------------------------------------------------------


def _identified(entity: object, /) -> int:
    # answers 1 for a stamped product and 0 otherwise, so the caller's fold counts identity rather than entities
    if not isinstance(entity, StepBasic_Product):
        return 0
    entity.SetId(TCollection_HAsciiString(PRODUCT_ID))
    entity.SetName(TCollection_HAsciiString(PRODUCT_NAME))
    entity.SetDescription(TCollection_HAsciiString(PRODUCT_DESCRIPTION))
    return 1


def _stamped(header: APIHeaderSection_MakeHeader, /) -> None:
    # `SetAuthorValue`/`SetOrganizationValue` write into an aggregate entry rather than growing one; `Transfer`
    # sizes that aggregate to entry one, which is why the caller proves `HasFn()` before this walk runs.
    for value, write in _HEADER.values():
        write(header, TCollection_HAsciiString(value))


def canonical(model: StepData_StepModel, /) -> CadRail[StepData_StepModel]:
    # `HasFn()` is the ORDERING PROOF: before `Transfer` populates FILE_NAME every setter below returns cleanly and
    # stores nothing, so an unguarded pre-transfer call emits a fully defaulted file and reports total success.
    header = APIHeaderSection_MakeHeader(model)
    if not header.HasFn():
        return Error(STEP_WRITE.at("file-name.unsized"))
    _stamped(header)
    identified = Block.of_seq(range(1, model.NbEntities() + 1)).fold(
        lambda count, index: count + _identified(model.Value(index)), 0
    )
    return Ok(model) if identified else Error(STEP_WRITE.at("step-basic-product.absent"))
```

## [04]-[RESEARCH]

- [HEADER_RESIDUE]-[OPEN]: do FILE_DESCRIPTION `description` and `implementation_level` vary run to run, and do `SetDescriptionValue` and `SetImplementationLevel` deserve `.api` rows; diff two emissions, then seat the rows before either lands in this fence.
- [GLB_STABILITY]-[OPEN]: which `RWGltf_CafWriter` outputs vary run to run — generator string, node ordering, buffer padding; diff two emissions of one admitted document and seat the resulting law at `tessellation/emission`.

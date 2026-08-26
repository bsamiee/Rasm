# [PY_CAD_EXCHANGE_IDENTITY]

`identity` rules the provider's byte-stability contract: two runs of one admitted operation over identical geometry emit identical STEP octets, which is what makes `CadService.Execute` idempotent at the artifact rather than merely at the reply. This owner holds the canonical header stamp, the canonical product identity every emission carries, and the `Interface_Static` policy the whole worker process runs under.

`exchange/step#CODEC` composes `canonical` between transfer and write and proves `EMITTED` on readback, so this page seats beneath `step` inside the folder and imports nothing from it. `service/lane#REGIME` holds the ONE executable controller-and-pin table and fold, run once per worker process under its own `NATIVE_INIT`; this page declares the membership and the values — `UNIT`, `SCHEMA`, the three pin coordinates — and executes none of it. `faults#ROWS` supplies `STEP_WRITE`, and every refusal is `Error(<ROW>.at(...))` on `CadResult`.

## [01]-[INDEX]

- [02]-[PINS]: The membership and values of the process-global exchange regime, executed at `service/lane#REGIME`.
- [03]-[CANONICAL]: Canonical header identity rows, canonical `StepBasic_Product` identity, and the fields left unstamped.

## [02]-[PINS]

- Owner: `UNIT`, `SCHEMA`, and `EMITTED` are the byte-stability values, and this section's roster — both exchange controllers, `xstep.cascade.unit`, `write.step.unit`, `write.step.schema` — is the MEMBERSHIP the contract requires; `service/lane#REGIME` holds the one executable table and fold and imports these values, so the ruling and its execution cannot drift apart.
- Law: controller init precedes every static, because a static seated before its protocol registers reverts silently and reads back its default.
- Law: OCCT defaults length to millimetres and keeps writer schema process-global, so leaving either implicit breaks metre measurement and idempotent artifact identity together.
- Law: `write.step.schema` and `EMITTED` are one decision written twice — OCCT ordinal and wire enum — and `exchange/step#CODEC` proves they agree on every emitted file.
- Law: an unpinned writer emits `AUTOMOTIVE_DESIGN` rather than AP242, so a missing `write.step.schema` silently ships a file declaring a protocol this page never chose; that is the failure `sealed`'s re-read catches.
- Growth: a new process-global exchange knob is one value declared here beside one `Pin` row at `service/lane#REGIME` consuming it.
- Boundary: execution — the `Interface_Static` calls, the read-back proof, the once-per-process cache, and the `NATIVE_INIT` refusal — is `service/lane#REGIME`'s alone; no fence here touches OCCT process state.

```python
from collections.abc import Callable
from typing import Final

from builtins import frozendict
from expression import Error, Ok
from expression.collections import Block
from OCP.APIHeaderSection import APIHeaderSection_MakeHeader
from OCP.StepBasic import StepBasic_Product
from OCP.StepData import StepData_StepModel
from OCP.TCollection import TCollection_HAsciiString
# Contracts are retired from this logic.

from rasm.cad.faults import STEP_WRITE, CadResult

# --- [CONSTANTS] ------------------------------------------------------------------------

UNIT: Final[str] = "M"
SCHEMA: Final[int] = 5
EMITTED: Final[StepProtocol] = StepProtocol.AP242
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

```python
# --- [CONSTANTS] ------------------------------------------------------------------------

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
    if not isinstance(entity, StepBasic_Product):
        return 0
    entity.SetId(TCollection_HAsciiString(PRODUCT_ID))
    entity.SetName(TCollection_HAsciiString(PRODUCT_NAME))
    entity.SetDescription(TCollection_HAsciiString(PRODUCT_DESCRIPTION))
    return 1


def _stamped(header: APIHeaderSection_MakeHeader, /) -> None:
    for value, write in _HEADER.values():
        write(header, TCollection_HAsciiString(value))


def canonical(model: StepData_StepModel, /) -> CadResult[StepData_StepModel]:
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

(none)

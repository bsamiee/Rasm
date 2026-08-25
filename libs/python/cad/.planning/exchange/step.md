# [PY_CAD_EXCHANGE_STEP]

`step` owns ISO 10303 protocol admission and the provider's one STEP codec pair: `unsealed` lowers a call-owned file into kernel topology, `sealed` raises kernel topology back onto a call-owned file, and one schema roster decides which application protocol either side is reading. `sourced` is the forward leg's map-resolving entry — one lookup in the call's sha256-keyed path map ahead of `unsealed` — so both native B-rep bands reach source resolution here, one stratum down, and neither imports the fold apex for it. Artifact transport resolves `SealedStep.artifact` onto a path before this owner runs, so no member here opens a store, performs transport, or returns STEP octets.

`exchange/identity#PINS` seats beneath this page and rules both the process statics and the canonical header this codec stamps; `faults#ROWS` supplies `STEP_READ`, `STEP_SCHEMA`, and `STEP_WRITE`, and every refusal is `Error(<ROW>.at(...))` on `CadRail`. `gated` is this page's export of the one `IFSelect_ReturnStatus` guard, and `exchange/assembly#DOCUMENT` composes it over the CAF readers instead of re-spelling the comparison.

## [01]-[INDEX]

- [02]-[PROTOCOL]: One schema roster, its derived forward and inverse maps, and file-local application-protocol admission.
- [03]-[CODEC]: `gated`, the `unsealed`/`sealed` pair composing every OCCT exchange status onto one rail, and `sourced` the map-resolving forward entry.

## [02]-[PROTOCOL]

- Owner: `_ROSTER` is the one schema correspondence, and `_PROTOCOLS` and `_SCHEMAS` both derive from it rather than being hand-kept beside it.
- Cases: `StepProtocol` admits AP203, AP214, and AP242; `STEP_PROTOCOL_UNSPECIFIED` reaches no row and therefore never admits.
- Law: `FILE_SCHEMA` read through `APIHeaderSection_MakeHeader` is the only file authority — `StepModel().Protocol().SchemaName(model)` reports process selection, and `FsValue()` returns an unregistered type.
- Law: AP214 spells two file tokens against one wire protocol, so aliases ride the row and the derived inverse elects one canonical token per protocol.
- Law: exactly one schema identifier admits; absent or plural `FILE_SCHEMA` refuses on cardinality before any token lookup runs.
- Law: `schema` is the inverse's one public reader, so a mismatch coordinate names both the found and the expected token instead of a bare enum repr.
- Growth: a new application protocol is one `_ROSTER` row beside its generated enum member, and both derived maps follow.
- Boundary: this cluster reads the header; writing canonical header fields belongs to `exchange/identity#CANONICAL`.

```python
from collections.abc import Callable
from functools import partial
from pathlib import Path
from typing import Final

from builtins import frozendict
from expression import Error, Ok, Option
from expression.extra.result import pipeline
from msgspec import Struct
from OCP.APIHeaderSection import APIHeaderSection_MakeHeader
from OCP.IFSelect import IFSelect_ReturnStatus
from OCP.STEPControl import STEPControl_Reader, STEPControl_StepModelType, STEPControl_Writer
from OCP.StepData import StepData_StepModel
from OCP.TopoDS import TopoDS_Shape
from rasm.contracts.rasm.contracts.cad.types_pb import SealedStep, StepProtocol

from rasm.cad.exchange.identity import EMITTED, canonical
from rasm.cad.faults import BREP_INPUT, STEP_READ, STEP_SCHEMA, STEP_WRITE, CadRail, FaultRow

# --- [TYPES] ----------------------------------------------------------------------------

type ExchangeArrow[A] = Callable[[A], CadRail[A]]


# --- [MODELS] ---------------------------------------------------------------------------


class _Schema(Struct, frozen=True):
    protocol: StepProtocol
    schema: str
    aliases: tuple[str, ...] = ()


# --- [PROTOCOLS] ------------------------------------------------------------------------

_ROSTER: Final[tuple[_Schema, ...]] = (
    _Schema(protocol=StepProtocol.AP203, schema="CONFIG_CONTROL_DESIGN"),
    _Schema(protocol=StepProtocol.AP214, schema="AUTOMOTIVE_DESIGN", aliases=("AUTOMOTIVE_DESIGN_CC2",)),
    _Schema(protocol=StepProtocol.AP242, schema="AP242_MANAGED_MODEL_BASED_3D_ENGINEERING_MIM_LF"),
)
_PROTOCOLS: Final[frozendict[str, StepProtocol]] = frozendict({
    token: row.protocol for row in _ROSTER for token in (row.schema, *row.aliases)
})
_SCHEMAS: Final[frozendict[StepProtocol, str]] = frozendict({row.protocol: row.schema for row in _ROSTER})


# --- [OPERATIONS] -----------------------------------------------------------------------


def schema(protocol: StepProtocol, /) -> str:
    return _SCHEMAS[protocol]


def _token(identifier: str, /) -> str:
    return identifier.partition("{")[0].strip().removesuffix(".").rstrip()


def _admitted(token: str, /) -> CadRail[StepProtocol]:
    return Option.of_optional(_PROTOCOLS.get(token)).to_result_with(lambda: STEP_SCHEMA.at(f"file-schema:{token}"))


def declared(model: StepData_StepModel, /) -> CadRail[StepProtocol]:
    header = APIHeaderSection_MakeHeader(model)
    return (
        Ok(header)
        .filter_with(
            lambda held: held.HasFs() and held.NbSchemaIdentifiers() == 1,
            lambda held: STEP_SCHEMA.at(f"file-schema.cardinality:{held.NbSchemaIdentifiers()}"),
        )
        .bind(lambda held: _admitted(_token(held.SchemaIdentifiersValue(1).ToCString())))
    )
```

## [03]-[CODEC]

- Owner: `unsealed` and `sealed` are the forward and inverse of one correspondence, so they share this owner and neither moves without the other; `sourced` is the forward leg over the call's path map, seated with the codec because resolving a `SealedStep` to its decoded shape IS this correspondence, and seating it at the fold apex forced set algebra to import back into it.
- Law: `gated` is the package's sole `IFSelect_RetDone` comparison — every read, transfer, write, and probe crosses it, and its coordinate keeps the failing status rather than collapsing to a bare call name.
- Law: `TransferRoots` answers a count and `OneShape` a possibly-null shape, so those two legs state their own predicate instead of forcing a status shape neither call produces.
- Law: `sealed` stamps canonical identity between transfer and write, then re-reads the emitted file and proves it declares `EMITTED`, which is what makes the pinned `write.step.schema` evidence rather than intent.
- Law: readback refusal re-spells `declared`'s `STEP_SCHEMA` onto `STEP_WRITE` through `map_error`, because a file this owner just wrote is never the caller's argument.
- Law: exchange precision stays file-owned; no process-global healing knob reinterprets source topology inside this codec.
- Entry: `pipeline` composes each leg kleisli at four arrows, under the six its overload ladder types before degrading to `Result[Any, Any]`.
- Boundary: path custody, artifact publication, and `SealedStep` construction belong to `service/spool` and `brep/operation`; this owner reads and writes paths handed to it, and `sourced` only looks a digest up in the map the serve layer already resolved — it opens no store.

```python
# --- [OPERATIONS] -----------------------------------------------------------------------


def gated[A](row: FaultRow, coordinate: str, call: Callable[[A], IFSelect_ReturnStatus], /) -> ExchangeArrow[A]:
    def arrow(held: A, /) -> CadRail[A]:
        status = call(held)
        return Ok(held) if status == IFSelect_ReturnStatus.IFSelect_RetDone else Error(row.at(f"{coordinate}:{status}"))

    return arrow


def _opened(path: Path, /) -> CadRail[STEPControl_Reader]:
    return gated(STEP_READ, "STEPControl_Reader.ReadFile", lambda held: held.ReadFile(str(path)))(STEPControl_Reader())


def _matched(protocol: StepProtocol, /) -> ExchangeArrow[STEPControl_Reader]:
    def arrow(reader: STEPControl_Reader, /) -> CadRail[STEPControl_Reader]:
        return declared(reader.StepModel()).bind(
            lambda found: Ok(reader)
            if found == protocol
            else Error(STEP_SCHEMA.at(f"file-schema:{schema(found)}!={schema(protocol)}"))
        )

    return arrow


def _transferred(reader: STEPControl_Reader, /) -> CadRail[STEPControl_Reader]:
    return Ok(reader) if reader.TransferRoots() else Error(STEP_READ.at("STEPControl_Reader.TransferRoots:0"))


def _one(reader: STEPControl_Reader, /) -> CadRail[TopoDS_Shape]:
    shape = reader.OneShape()
    return Error(STEP_READ.at("STEPControl_Reader.OneShape:null")) if shape.IsNull() else Ok(shape)


def unsealed(value: SealedStep, path: Path, /) -> CadRail[TopoDS_Shape]:
    return pipeline(_opened, _matched(value.protocol), _transferred, _one)(path)


def sourced(value: SealedStep, sources: frozendict[bytes, Path], /) -> CadRail[TopoDS_Shape]:
    return (
        Option.of_optional(sources.get(value.artifact.sha256))
        .to_result_with(lambda: BREP_INPUT.at(f"source.absent:{value.artifact.sha256.hex()}"))
        .bind(partial(unsealed, value))
    )


def _staged(shape: TopoDS_Shape, /) -> ExchangeArrow[STEPControl_Writer]:
    return gated(
        STEP_WRITE,
        "STEPControl_Writer.Transfer",
        lambda held: held.Transfer(shape, STEPControl_StepModelType.STEPControl_AsIs),
    )


def _stamped(writer: STEPControl_Writer, /) -> CadRail[STEPControl_Writer]:
    return canonical(writer.Model()).map(lambda _model: writer)


def _written(path: Path, /) -> Callable[[STEPControl_Writer], CadRail[Path]]:
    def arrow(writer: STEPControl_Writer, /) -> CadRail[Path]:
        return gated(STEP_WRITE, "STEPControl_Writer.Write", lambda held: held.Write(str(path)))(writer).map(
            lambda _held: path
        )

    return arrow


def _probed(path: Path, /) -> CadRail[StepProtocol]:
    return (
        gated(STEP_WRITE, "STEPControl_Reader.ReadFile", lambda held: held.ReadFile(str(path)))(STEPControl_Reader())
        .bind(lambda probe: declared(probe.StepModel()).map_error(lambda fault: STEP_WRITE.at(fault.coordinate)))
        .bind(
            lambda found: Ok(found)
            if found == EMITTED
            else Error(STEP_WRITE.at(f"write.step.schema:{schema(found)}!={schema(EMITTED)}"))
        )
    )


def sealed(shape: TopoDS_Shape, path: Path, /) -> CadRail[StepProtocol]:
    return pipeline(_staged(shape), _stamped, _written(path), _probed)(STEPControl_Writer())
```

## [04]-[RESEARCH]

(none)

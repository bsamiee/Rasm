# [PY_DATA_SCENARIO]

Prospective-background build owner — the producer half of the carrier's `premise_background` proof: `ScenarioBuild` drives the `premise` transform of a present-day ecoinvent database toward an IAM `(model, pathway, year)` scenario and registers the result back into the open Brightway project, so the database name `PremiseSolve.background` consumes and `_from_prospective` proves against `bd.databases.list` gains its producer here. The build is an ecoinvent-licensed, self-parallelizing, multi-hour sector transform a composition runs OUT OF BAND — this page owns the typed build request, the write-back target vocabulary, the floor gate, and `ScenarioResult`; the carrier hosts no build and this page computes no LCIA.

`premise` is FLOOR-GATED — admission stands, reach does not: `build` opens on a `find_spec` row and refuses every request through the `SCENARIO_GATED` row, whose `import_` arm names the absent module while the marker holds, per the folder's floor-gate law; the gate runs at entry, ahead of the module-scope `lazy` binding's first use, so the two forms compose.

## [01]-[INDEX]

- [02]-[SCENARIO]: the `ScenarioBuild` owner — the scenario roster, the `BuildKind` write-back axis, the sector selection, the floor gate, and the registered-name result.

## [02]-[SCENARIO]

- Owner: `ScenarioBuild` — one frozen build request: the `Scenario` roster (each row the provider's own `{model, pathway, year}` triple), the source coordinate (`source_db` in the open project, `system_model`, the decryption `key`), and the `BuildKind` write-back axis; `Sector` selection rides the request as a tuple where empty means the provider's own all-sectors default.
- Law: the floor gate leads every entry — `find_spec("premise")` refuses through `SCENARIO_GATED.raised(...)` before the module-scope `lazy import premise` is ever dereferenced, so the deferred binding cannot carry the failure into the offloaded band as a provider crash; every surface naming the gated coordinate derives from this one gate, the folder's floor-gate law. The build fence names the raise set it can PROVE — `bw2data`'s family and the transform's own I/O — and premise's own roster stays unnameable while the gate holds, so a premise-native raise propagates as the defect it is rather than arriving typed off a guess.
- Law: the registered database name is the CONSUMER CONTRACT — `database` writes `write_db_to_brightway(name)` and `ScenarioResult` carries exactly that name beside the scenario tuple; a build that returns a different name strands the carrier's `background` proof.
- Law: the build self-parallelizes over scenarios and sectors — the entry adds NO outer process pool (the catalog's own contention law) and rides one `on_thread` band hop so a composition's loop never hosts the multi-hour transform; `update(sectors)` runs the named sector subset and the change report stays a provider surface the composition reads out of band.
- Cases: `BuildKind.database` is the canonical build→score path (one database per scenario); `superstructure` folds every scenario into one scenario-difference database for a `MultiLCA` sweep; `increment` stacks sector transforms step-by-step (`IncrementalDatabase.update(sectors)` then `write_increment_db_to_brightway`) for step sensitivity; `pathways` spans a year grid (`PathwaysDataPackage.create_datapackage`) for the time-series tool; `datapackage` emits the shareable `bw_processing` superstructure.
- Law: one `ScenarioResult` per write-back carries the registered names, scenario tuples, build kind, and deterministic content key over `(scenarios, source, system_model, kind)`, so an identical build keys identically and any coordinate change re-keys. Cache hygiene remains the provider's own `clear_inventory_cache` operation after an inventory or ecoinvent change.
- Packages: `premise` (`NewDatabase(scenarios, source_type='brightway', source_db=, key=, system_model=).update(sectors).write_db_to_brightway(name)`, `write_superstructure_db_to_brightway`, `write_datapackage`, `IncrementalDatabase.update`/`write_increment_db_to_brightway`, `PathwaysDataPackage(scenarios, years=, ...).create_datapackage(name)` — catalog-verified members behind the floor gate), `bw2data` (`databases.list`/`databases.version`, the registry the result and the carrier both read, `errors.BW2Exception` the family root the build fence names), runtime (`RuntimeResult`/`boundary`/`Catch`/`FaultRow`/`scoped`/`on_thread`).
- Growth: a new IAM model or pathway is one `Scenario` row; a new write-back form is one `BuildKind` case plus one arm; a new sector is one `Sector` member mirroring the provider's transformer roster; a new refusal law is one `FaultRow` row on this module's `RAISES` table, and premise's own raise classes join `_build_raises` the pass the floor gate lifts; zero new surface.
- Boundary: no LCIA (the carrier's solve arms score the written background), no ingestion (`impact/inventory#INVENTORY` fills the source project), no license custody — the decryption `key` arrives as a caller-supplied credential from the composition's secret plane, never a stored field this page persists; the carrier's `_from_prospective` refusal stays the read-side gate and this producer never weakens it.

```python
from enum import StrEnum
from importlib.util import find_spec
from typing import Final, Literal, assert_never

from expression import Error, Ok, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct
from msgspec import json as msgjson
from opentelemetry import trace

lazy import bw2data as bd
lazy import premise

from rasm.data.tabular.interop import DataLeg
from rasm.runtime.faults import TERMINAL, TRANSIENT, Catch, FaultRow, RuntimeResult, boundary, rostered, scoped
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import on_thread

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.impact.scenario")
_ENCODER: Final = msgjson.Encoder(order="deterministic")


# --- [CONSTANTS] ------------------------------------------------------------------------


def _build_raises() -> Catch:
    return (bd.errors.BW2Exception, KeyError, TypeError, ValueError, OSError)


SCENARIO_GATED: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.SCENARIO, point="gate", arm="import_", defect="floor-gated", retriability=TERMINAL, slots=("ceiling",)
)
SCENARIO_BUILD: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.SCENARIO, point="build", arm="boundary", defect="prospective-build", retriability=TRANSIENT
)
SCENARIO_UNREGISTERED: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.SCENARIO, point="build.registry", arm="config", defect="unregistered", retriability=TERMINAL, slots=("names",)
)
RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([SCENARIO_GATED, SCENARIO_BUILD, SCENARIO_UNREGISTERED]))


class Sector(StrEnum):
    ELECTRICITY = "electricity"
    CEMENT = "cement"
    STEEL = "steel"
    FUELS = "fuels"
    TRANSPORT = "transport"
    HEAT = "heat"
    METALS = "metals"
    BATTERY = "battery"
    BIOMASS = "biomass"
    EMISSIONS = "emissions"


class Scenario(Struct, frozen=True, gc=False):
    model: str
    pathway: str
    year: int

    def row(self) -> dict[str, object]:
        return {"model": self.model, "pathway": self.pathway, "year": self.year}


@tagged_union(frozen=True)
class BuildKind:
    tag: Literal["database", "superstructure", "increment", "pathways", "datapackage"] = tag()
    database: tuple[str, ...] = case()
    superstructure: str = case()
    increment: tuple[str, ...] = case()
    pathways: tuple[str, tuple[int, ...]] = case()
    datapackage: str = case()


class ScenarioResult(Struct, frozen=True, gc=False):
    names: tuple[str, ...]
    scenarios: tuple[Scenario, ...]
    kind: str
    content_key: ContentKey

class ScenarioBuild(Struct, frozen=True):
    scenarios: tuple[Scenario, ...]
    source_db: str
    system_model: Literal["cutoff", "consequential"] = "cutoff"
    sectors: tuple[Sector, ...] = ()

    async def build(self, kind: BuildKind, key: str) -> "RuntimeResult[ScenarioResult]":
        if find_spec("premise") is None:
            return Error(SCENARIO_GATED.raised("numba below cp315"))

        def run() -> "RuntimeResult[ScenarioResult]":
            rows = [scenario.row() for scenario in self.scenarios]
            sectors = [sector.value for sector in self.sectors] or None
            match kind:
                case BuildKind(tag="database", database=names):
                    built = premise.NewDatabase(rows, source_type="brightway", source_db=self.source_db, key=key, system_model=self.system_model)
                    built.update(sectors=sectors)
                    built.write_db_to_brightway(list(names))
                    registered = names
                case BuildKind(tag="superstructure", superstructure=name):
                    built = premise.NewDatabase(rows, source_type="brightway", source_db=self.source_db, key=key, system_model=self.system_model)
                    built.update(sectors=sectors)
                    built.write_superstructure_db_to_brightway(name)
                    registered = (name,)
                case BuildKind(tag="increment", increment=names):
                    built = premise.IncrementalDatabase(rows, source_type="brightway", source_db=self.source_db, key=key, system_model=self.system_model)
                    built.update({sector.value: True for sector in self.sectors} or None)
                    built.write_increment_db_to_brightway(list(names))
                    registered = names
                case BuildKind(tag="pathways", pathways=(name, years)):
                    package = premise.PathwaysDataPackage(rows, years=list(years), source_type="brightway", key=key)
                    package.create_datapackage(name)
                    registered = (name,)
                case BuildKind(tag="datapackage", datapackage=name):
                    built = premise.NewDatabase(rows, source_type="brightway", source_db=self.source_db, key=key, system_model=self.system_model)
                    built.update(sectors=sectors)
                    built.write_datapackage(name)
                    registered = (name,)
                case unreachable:
                    assert_never(unreachable)
            missing = tuple(name for name in registered if kind.tag in {"database", "superstructure", "increment"} and name not in bd.databases.list)
            if missing:
                return Error(SCENARIO_UNREGISTERED.raised(",".join(missing)))
            identity = _ENCODER.encode((sorted((s.model, s.pathway, s.year) for s in self.scenarios), self.source_db, self.system_model, kind.tag))
            return Ok(ScenarioResult(
                names=tuple(registered), scenarios=self.scenarios, kind=kind.tag, content_key=ContentIdentity.key("impact", identity)
            ))

        with _TRACER.start_as_current_span("scenario.build", attributes={"rasm.impact.scenarios": len(self.scenarios), "rasm.impact.kind": kind.tag}):
            fenced = await on_thread(lambda: boundary(SCENARIO_BUILD, run, catch=_build_raises()))
            return fenced.bind(lambda fence: fence).bind(lambda body: body)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

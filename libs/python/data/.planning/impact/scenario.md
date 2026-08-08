# [PY_DATA_SCENARIO]

Prospective-background build owner — the producer half of the carrier's `premise_background` proof: `ScenarioBuild` drives the `premise` transform of a present-day ecoinvent database toward an IAM `(model, pathway, year)` scenario and registers the result back into the open Brightway project, so the database name `PremiseSolve.background` consumes and `_from_prospective` proves against `bd.databases.list` gains its producer here. The build is an ecoinvent-licensed, self-parallelizing, multi-hour sector transform a composition runs OUT OF BAND — this page owns the typed build request, the write-back target vocabulary, the floor gate, and the receipt; the carrier hosts no build and this page computes no LCIA.

`premise` is FLOOR-GATED — admission stands, reach does not: `build` opens on an import-time `find_spec` row and refuses every request with `BoundaryFault(import_=)` naming the absent module while the marker holds, per the folder's floor-gate law.

## [01]-[INDEX]

- [02]-[SCENARIO]: the `ScenarioBuild` owner — the scenario roster, the `BuildKind` write-back axis, the sector selection, the floor gate, the registered-name receipt.

## [02]-[SCENARIO]

- Owner: `ScenarioBuild` — one frozen build request: the `Scenario` roster (each row the provider's own `{model, pathway, year}` triple), the source coordinate (`source_db` in the open project, `system_model`, the decryption `key`), and the `BuildKind` write-back axis; `Sector` selection rides the request as a tuple where empty means the provider's own all-sectors default.
- Law: the floor gate leads every entry — `find_spec("premise")` refuses with `BoundaryFault(import_=("premise", ...))` before any provider import, so an ungated lazy import can never defer the failure into the offloaded band as a provider crash; every surface naming the gated coordinate derives from this one gate, the folder's floor-gate law.
- Law: the registered database name is the CONSUMER CONTRACT — `database` writes `write_db_to_brightway(name)` and the receipt carries exactly that name beside the scenario tuple and `bd.databases.version(name)`, the identity triple the carrier's reuse ledger joins; a build that writes under a name the receipt does not spell strands the carrier's `background` proof.
- Law: the build self-parallelizes over scenarios and sectors — the entry adds NO outer process pool (the catalog's own contention law) and rides one `on_thread` band hop so a composition's loop never hosts the multi-hour transform; `update(sectors)` runs the named sector subset and the change report stays a provider surface the composition reads out of band.
- Cases: `BuildKind.database` is the canonical build→score path (one database per scenario); `superstructure` folds every scenario into one scenario-difference database for a `MultiLCA` sweep; `increment` stacks sector transforms step-by-step (`IncrementalDatabase.update(sectors)` then `write_increment_db_to_brightway`) for step sensitivity; `pathways` spans a year grid (`PathwaysDataPackage.create_datapackage`) for the time-series tool; `datapackage` emits the shareable `bw_processing` superstructure.
- Receipt: one `ScenarioReceipt` per write-back — the registered names, the scenario tuples, the build kind — under `domain="impact"`/`kind="scenario"`, its key the deterministic encoding of `(scenarios, source, system_model, kind)` so an identical build dedupes and any coordinate change re-keys; cache hygiene is the provider's own (`clear_inventory_cache` after an inventory or ecoinvent-version change), a composition step the receipt's coordinates make checkable.
- Packages: `premise` (`NewDatabase(scenarios, source_type='brightway', source_db=, key=, system_model=).update(sectors).write_db_to_brightway(name)`, `write_superstructure_db_to_brightway`, `write_datapackage`, `IncrementalDatabase.update`/`write_increment_db_to_brightway`, `PathwaysDataPackage(scenarios, years=, ...).create_datapackage(name)` — catalog-verified members behind the floor gate), `bw2data` (`databases.list`/`databases.version`, the registry the receipt and the carrier both read), runtime (`RuntimeRail`/`BoundaryFault`/`boundary`/`scoped`/`on_thread`).
- Growth: a new IAM model or pathway is one `Scenario` row; a new write-back form is one `BuildKind` case plus one arm; a new sector is one `Sector` member mirroring the provider's transformer roster; zero new surface.
- Boundary: no LCIA (the carrier's solve arms score the written background), no ingestion (`impact/inventory#INVENTORY` fills the source project), no license custody — the decryption `key` arrives as a caller-supplied credential from the composition's secret plane, never a stored field this page persists; the carrier's `_from_prospective` refusal stays the read-side gate and this producer never weakens it.

```python signature
from enum import StrEnum
from importlib.util import find_spec
from typing import TYPE_CHECKING, Final, Literal, assert_never

from expression import Error, case, tag, tagged_union
from msgspec import Struct
from msgspec import json as msgjson
from opentelemetry import trace

from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary, scoped
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import on_thread
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    from collections.abc import Iterable

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.impact.scenario")
_ENCODER: Final = msgjson.Encoder(order="deterministic")


class Sector(StrEnum):
    # mirrors the provider's per-sector transformer roster; the wire value IS the provider's sector key.
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
    model: str  # REMIND | IMAGE | TIAM-UCL | GCAM — the provider's own model vocabulary
    pathway: str
    year: int

    def row(self) -> dict[str, object]:
        return {"model": self.model, "pathway": self.pathway, "year": self.year}


@tagged_union(frozen=True)
class BuildKind:
    tag: Literal["database", "superstructure", "increment", "pathways", "datapackage"] = tag()
    database: tuple[str, ...] = case()  # per-scenario registered names, positionally aligned
    superstructure: str = case()  # one scenario-difference database name
    increment: tuple[str, ...] = case()  # per-increment names
    pathways: tuple[str, tuple[int, ...]] = case()  # (package name, year grid)
    datapackage: str = case()  # shareable bw_processing superstructure name


class ScenarioReceipt(Struct, frozen=True, gc=False):
    names: tuple[str, ...]
    scenarios: tuple[Scenario, ...]
    kind: str
    content_key: ContentKey

    def contribute(self) -> "Iterable[Receipt]":
        Metrics.record({"rasm.impact.backgrounds": float(len(self.names))}, domain="impact", kind="scenario")
        yield Receipt.of(
            "scenario",
            (
                "emitted",
                self.kind,
                {"domain": "impact", "kind": "scenario", "key": self.content_key.hex, "names": ",".join(self.names)},
            ),
        )


class ScenarioBuild(Struct, frozen=True):
    scenarios: tuple[Scenario, ...]
    source_db: str
    system_model: Literal["cutoff", "consequential"] = "cutoff"
    sectors: tuple[Sector, ...] = ()

    async def build(self, kind: BuildKind, key: str) -> "RuntimeRail[ScenarioReceipt]":
        # floor gate FIRST: while the manifest marker holds, premise resolves nowhere on the supported
        # interpreter, so every request refuses typed here and no lazy import defers the crash into the band.
        if find_spec("premise") is None:
            return Error(BoundaryFault(import_=("premise", "floor-gated: numba ceiling below cp315")))

        def run() -> ScenarioReceipt:
            import bw2data as bd  # ruff:ignore[import-outside-top-level] — banded boundary import
            import premise  # ruff:ignore[import-outside-top-level]

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
            # write-back proof: every registered database name resolves in the registry the carrier reads,
            # so the receipt never names a background `_from_prospective` would refuse.
            missing = tuple(name for name in registered if kind.tag in {"database", "superstructure", "increment"} and name not in bd.databases.list)
            if missing:
                raise ValueError(f"<unregistered:{missing}>")
            identity = _ENCODER.encode((sorted((s.model, s.pathway, s.year) for s in self.scenarios), self.source_db, self.system_model, kind.tag))
            return ScenarioReceipt(
                names=tuple(registered), scenarios=self.scenarios, kind=kind.tag, content_key=ContentIdentity.key("impact", identity)
            )

        # premise self-parallelizes over scenarios and sectors: ONE band hop, no outer pool.
        with _TRACER.start_as_current_span("scenario.build", attributes={"rasm.impact.scenarios": len(self.scenarios), "rasm.impact.kind": kind.tag}):
            railed = await on_thread(lambda: boundary("scenario.build", run))
            return railed.bind(lambda rail: rail)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

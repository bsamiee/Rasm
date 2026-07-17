# [PY_RUNTIME_RECIPE]

Local recipe execution — the one owner turning a queenbee-schema simulation workflow into a typed deliverable. `RecipeExecution.execute` is the single modality-polymorphic entry: it absorbs one `RecipeSpec` or a `Block[RecipeSpec]`, acquires remote assets through the `transport/roots#RESOURCE` rail, gates the external Radiance/OpenStudio/EnergyPlus engines through `reliability/resilience#RESILIENCE` `guarded_sync` BEFORE the subprocess spends minutes, executes `queenbee local run` off the event loop through the `execution/lanes#LANE` offload, and reads the typed deliverable back through `output_value_by_name` — the product is the handled output value set, never the raw project folder, and the parsed luigi evidence decides the rail, never the exit path alone. Each run is content-keyed over the recipe identity plus the handled `inputs.json` bytes, so a parametric batch drains as `keyed` units and an identical simulation replays from the lane cache rather than re-running the engine.

Recipe VOCABULARY stays queenbee's and the execution machinery stays `lbt-recipes`': this owner re-implements no luigi scheduler, no handler resolution, and no engine-version probe — it composes `Recipe`/`RecipeSettings`/`Recipe.run` and projects the contract through `RecipeInterface.from_recipe` for the submission-constructing consumer (geometry energy/simulate binds `Job`/`RecipeInterface` construction to this owner, the named `python:` consumer). One `RECIPES` seed table is the executable catalog — outputs, engine set, worker sizing as row data — so a new workflow is one row, never a per-recipe method or a sibling runner. `lbt-recipes`/`pollination-handlers` distributions and the `queenbee-local` luigi runner are AGPL-3.0 network copyleft: admissible only under the companion's process-boundary, non-distributed execution charter — engines run as external subprocesses, the AGPL tree imports function-local at the execution boundary, and nothing links into a distributed host binary.

## [01]-[INDEX]

- [01]-[RECIPE]: the one `RecipeExecution` owner — `RecipeSpec` request shape, `RECIPES` catalog, content-keyed lane execution with engine prechecks, and the luigi-evidence `RecipeReceipt`.

## [02]-[RECIPE]

- Owner: `RecipeExecution` holds the one `LanePolicy` its runs drain and offload under — capacity and deadline arrive as the caller's `execution/admission#CONTEXT` budget projection at construction, never a per-call knob — and the `Option[ResourceRoot]` remote specs resolve against. `RecipeSpec.recipe` selector discriminates by VALUE — a catalog member or an external folder path — never a `packaged: bool` beside it, and an external folder's empty readback roster derives from the baked `package.json` contract through `_declared`, never a hand-mirrored list.
- Entry: `execute` threads the caller's session cache as a value — a prior `DrainReceipt.cache` re-enters as the next call's carrier, so elision is threaded state, never hidden owner state. `interface` is the schema projection for submission-constructing consumers; execution always returns through `execute`, never a second runner.
- Auto: span presence IS execution evidence — one `recipe.execute` span wraps the executed leg, the engine gate and coercion ride the `guarded` derivation span at staging, and a cache-elided replay opens no execute span. Subprocess environment discipline stays `lbt_recipes`' own (`--env` PATH/RAYPATH, cleared `PYTHONHOME`) — this owner never re-derives the shell line. Deliverables are row-driven and typed: handler-parsed lists and `DataCollection` objects, never a path the caller must re-parse.
- Growth: a new simulation workflow is one `RecipeName` member plus one `RECIPES` row; a new engine one `Engine` member plus one `ENGINE_CHECK` row; a new remote asset kind one `AssetFetch` on the spec; a new run-policy dimension one `RecipeRow` column or one `RecipeSpec` Option folded into the `RecipeSettings` default; the cloud-submission modality (a Pollination platform `Job` POST composing the queenbee shapes against `interface`) enters as one more execute arm over the same `RecipeSpec` when a consumer names it, never a parallel owner.
- Boundary: no luigi scheduling, no handler resolution or chain ordering, no engine probing beside `version.check_*`, and no recipe-schema re-mint — queenbee owns the vocabulary, and a `msgspec`/protobuf mirror of a queenbee model is a single-mint violation. queenbee's click CLI and urllib transfer stay rejected: `cyclopts` and the roots rail own those concerns. No durable run ledger — the session cache is lane-local, and durable reuse stays the C# `Rasm.Persistence` ledger consumed at the wire. Engines are external binaries; no simulation runs in-process.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Buffer, Iterable
from enum import StrEnum
from pathlib import Path
from typing import TYPE_CHECKING, Final

from expression import Error, Nothing, Ok, Option, Result
from expression.collections import Block, Map
from expression.extra.result import traverse
from msgspec import Struct
from opentelemetry import trace

from rasm.runtime.faults import SCOPES, BoundaryFault, RuntimeRail, Scope
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import Admit, LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait
from rasm.runtime.receipts import OPEN, DrainReceipt, Receipt, receipted
from rasm.runtime.resilience import RetryClass, guarded_sync
from rasm.runtime.roots import Delivery, ResourceRoot

if TYPE_CHECKING:  # the AGPL lbt tree loads function-local at the execution boundary; annotations resolve here
    from lbt_recipes.recipe import Recipe
    from lbt_recipes.settings import RecipeSettings
    from queenbee.recipe.recipe import RecipeInterface

# --- [TYPES] ----------------------------------------------------------------------------


class RecipeName(StrEnum):
    # `Recipe.__init__` normalizes the hyphenated value onto the underscore install folder, so the wire spelling stays hyphenated.
    ANNUAL_DAYLIGHT = "annual-daylight"
    ANNUAL_DAYLIGHT_ENHANCED = "annual-daylight-enhanced"
    ANNUAL_IRRADIANCE = "annual-irradiance"
    CUMULATIVE_RADIATION = "cumulative-radiation"
    DIRECT_SUN_HOURS = "direct-sun-hours"
    DAYLIGHT_FACTOR = "daylight-factor"
    POINT_IN_TIME_GRID = "point-in-time-grid"
    POINT_IN_TIME_VIEW = "point-in-time-view"
    SKY_VIEW = "sky-view"
    IMAGELESS_ANNUAL_GLARE = "imageless-annual-glare"
    ANNUAL_ENERGY_USE = "annual-energy-use"
    ADAPTIVE_COMFORT_MAP = "adaptive-comfort-map"
    PMV_COMFORT_MAP = "pmv-comfort-map"
    UTCI_COMFORT_MAP = "utci-comfort-map"


class Engine(StrEnum):
    RADIANCE = "radiance"
    OPENSTUDIO = "openstudio"
    ENERGYPLUS = "energyplus"


# --- [CONSTANTS] ------------------------------------------------------------------------

# each engine keys its lbt_recipes.version probe name; the gate resolves the callable inside the
# boundary kernel so the AGPL tree never loads at module scope and a new engine is one row.
ENGINE_CHECK: Final[Map[Engine, str]] = Map.of_seq([
    (Engine.RADIANCE, "check_radiance_date"),
    (Engine.OPENSTUDIO, "check_openstudio_version"),
    (Engine.ENERGYPLUS, "check_energyplus_version"),
])

_COMFORT_ENGINES: Final[frozenset[Engine]] = frozenset({Engine.RADIANCE, Engine.OPENSTUDIO, Engine.ENERGYPLUS})

# --- [MODELS] ---------------------------------------------------------------------------


class AssetFetch(Struct, frozen=True):
    source: str  # root-relative path resolved against the execution's ResourceRoot
    relative: str  # project-folder destination the handlers read


class RecipeRow(Struct, frozen=True):
    outputs: tuple[str, ...]
    engines: frozenset[Engine]
    workers: int
    # run-reuse policy paired with the content-key elision: an identical spec replayed into a
    # reused project folder lets luigi skip its completed tasks instead of re-running the engine.
    reload: bool = True


class RecipeSpec(Struct, frozen=True):
    recipe: RecipeName | str
    inputs: Map[str, object] = Map.empty()
    settings: "Option[RecipeSettings]" = Nothing
    assets: Block[AssetFetch] = Block.empty()
    outputs: tuple[str, ...] = ()  # external-folder readback names; empty derives from the baked contract
    engines: frozenset[Engine] = frozenset()  # external-folder engine gate; catalog rows carry their own
    debug: Option[str] = Nothing  # RecipeSettings.debug_folder intermediate-artifact capture

    def row(self) -> RecipeRow:
        match self.recipe:
            case RecipeName() as name:
                return RECIPES[name]
            case _:
                return RecipeRow(outputs=self.outputs, engines=self.engines, workers=1)


class RecipeReceipt(Struct, frozen=True):
    simulation_id: str
    recipe: str
    tag: str
    engines: tuple[Engine, ...]
    summary: str
    failure: Option[str]
    errors: Option[str]  # parsed err.log error_summary — the failure arm alone pays the log walk
    output_count: int
    content_key: ContentKey

    def contribute(self) -> Iterable[Receipt]:
        # full luigi summary stays on this struct; the emitted fact carries the scalar evidence alone.
        yield Receipt.of(
            SCOPES[Scope.RECIPE],
            (
                "emitted",
                f"{self.recipe}:{self.simulation_id}",
                {
                    "tag": self.tag,
                    "engines": [str(engine) for engine in self.engines],
                    "outputs": self.output_count,
                    "failed": self.failure.is_some(),
                    "content_key": self.content_key.hex,
                },
            ),
        )


class RecipeProduct(Struct, frozen=True):
    outputs: Map[str, object]
    receipt: RecipeReceipt

    def contribute(self) -> Iterable[Receipt]:
        # product delegates to its receipt so the `@receipted(OPEN)` harvest reads one stream, never a second contributor shape.
        yield from self.receipt.contribute()


class _Staged(Struct, frozen=True):
    recipe: "Recipe"
    row: RecipeRow
    settings: "RecipeSettings"
    outputs: tuple[str, ...]  # the resolved readback roster — row/caller names, else the baked contract's own
    key: ContentKey


# --- [SERVICES] -------------------------------------------------------------------------


class RecipeExecution(Struct, frozen=True):
    lane: LanePolicy
    root: Option[ResourceRoot] = Nothing

    async def execute(
        self, spec: "RecipeSpec | Block[RecipeSpec]", cache: Map[ContentKey, RecipeProduct] = Map.empty()
    ) -> "RuntimeRail[RecipeProduct] | DrainReceipt[RecipeProduct]":
        match spec:
            case Block() as many:
                units: Block[Admit[RecipeProduct]] = Block.empty()
                for one in many:  # Exemption: async sequential prepare — staging awaits asset IO per spec; the drain is the concurrent leg.
                    units = units.append(Block.singleton(await self._admitted(one)))
                return await self.lane.drain(units, cache)
            case lone:
                staged = await self._prepared(lone)
                return await staged.map(
                    lambda ready: cache.try_find(ready.key).map(_elided).default_with(lambda: self._observed(ready))
                ).default_with(_refused)

    async def interface(self, spec: RecipeSpec) -> "RuntimeRail[RecipeInterface]":
        # baked-contract folder parse is blocking I/O, so the kernel declares the RELEASING trait and its raises cross the one fence.
        return await self.lane.offload(Kernel.of(_interface, KernelTrait.RELEASING), spec)

    async def _admitted(self, spec: RecipeSpec) -> Admit[RecipeProduct]:
        staged = await self._prepared(spec)
        return staged.map(lambda ready: Admit(keyed=(ready.key, lambda: self._observed(ready)))).default_with(
            lambda fault: Admit(bare=lambda: _refused(fault))
        )

    async def _prepared(self, spec: RecipeSpec) -> "RuntimeRail[_Staged]":
        # staging is blocking work (engine probes, handler coercion copying artifact trees), so it declares the RELEASING trait — the
        # offload fence converts the handler/spawn raises. Project root resolves FIRST, so every asset lands beneath the one
        # folder the handler chains read and `run` executes in; the acquisition fault short-circuits.
        rooted = await self.lane.offload(Kernel.of(_rooted, KernelTrait.RELEASING), spec)
        if rooted.is_error():
            return rooted
        acquired = await self._acquired(spec.assets, Path(rooted.ok))
        if acquired.is_error():
            return acquired
        return (await self.lane.offload(Kernel.of(_staged, KernelTrait.RELEASING), spec, rooted.ok)).bind(lambda rail: rail)

    async def _acquired(self, assets: Block[AssetFetch], root: Path) -> "RuntimeRail[int]":
        if assets.is_empty():
            return Ok(0)
        return await self.root.map(lambda live: self._fetched(live, assets, root)).default_with(
            lambda: _refused(BoundaryFault(resource=("recipe", "asset-fetch-without-root")))
        )

    async def _fetched(self, live: ResourceRoot, assets: Block[AssetFetch], root: Path) -> "RuntimeRail[int]":
        # the WHOLE destination roster confines beneath the resolved project root before the first byte lands, so a
        # later bad row never strands a half-landed asset set behind a refusal.
        roster = traverse(lambda asset: _confined(root, asset.relative), assets)
        if roster.is_error():
            return roster
        landed: RuntimeRail[int] = Ok(0)
        for asset, destination in zip(assets, roster.ok, strict=True):  # Exemption: async sequential acquisition — each read awaits the roots rail, each landing awaits the thread-band hop, the carrier rebinds per step.
            match await live.child(asset.source).map(lambda ref: live.read(ref, Delivery.WHOLE)).default_with(_refused):
                case Result(tag="error") as refused:
                    return refused
                case Result(tag="ok", ok=payload):
                    written = await self.lane.offload(Kernel.of(_landed, KernelTrait.RELEASING), str(destination), payload)
                    landed = landed.bind(lambda n: written.map(lambda one: n + one))
            if landed.is_error():
                return landed
        return landed

    async def _observed(self, staged: _Staged) -> "RuntimeRail[RecipeProduct]":
        # blocking child-process wait crosses on the RELEASING trait under the lane's deadline and limiter; the flatten joins the
        # offload rail onto the kernel rail, and `_emit` harvests the receipt.
        with trace.get_tracer(SCOPES[Scope.RECIPE]).start_as_current_span("recipe.execute"):
            return (await self.lane.offload(Kernel.of(_execute, KernelTrait.RELEASING), staged)).bind(lambda rail: rail).map(_emit)


# --- [OPERATIONS] -----------------------------------------------------------------------


async def _elided(product: RecipeProduct) -> "RuntimeRail[RecipeProduct]":
    return Ok(product)


async def _refused[T](fault: BoundaryFault) -> "RuntimeRail[T]":
    return Error(fault)


@receipted(OPEN)
def _emit(product: RecipeProduct) -> RecipeProduct:
    return product


def _rooted(spec: RecipeSpec) -> str:
    # project root resolves BEFORE any asset byte lands: the caller's settings folder when supplied, else the recipe's
    # own default project folder — one folder owns asset landing, handler reads, and the `run` execution alike, where a
    # cwd-anchored landing strands every asset outside the folder the handler chains read.
    from lbt_recipes.recipe import Recipe  # noqa: PLC0415 — AGPL boundary import

    root = Path(
        spec.settings.bind(lambda held: Option.of_optional(held.folder)).default_with(lambda: Recipe(str(spec.recipe)).default_project_folder)
    ).resolve()
    root.mkdir(parents=True, exist_ok=True)
    return str(root)


def _confined(root: Path, relative: str) -> "RuntimeRail[Path]":
    # spec-supplied destinations confine beneath the `_rooted`-resolved project root: an absolute path or a `..`
    # traversal refuses typed before any byte lands — never a write outside the folder the handler chains read.
    candidate = Path(relative)
    resolved = (root / candidate).resolve()
    return (
        Error(BoundaryFault(config=("recipe.asset", f"destination-escapes-project:{relative}")))
        if candidate.is_absolute() or not resolved.is_relative_to(root)
        else Ok(resolved)
    )


def _landed(destination_path: str, got: Buffer) -> int:
    # payload lands at the `_confined`-resolved destination; the blocking mkdir/write crosses on the thread band.
    destination = Path(destination_path)
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(bytes(got))
    return 1


def _declared(recipe: "Recipe") -> tuple[str, ...]:
    # baked `package.json` contract — never a hand-mirrored list — is the roster's source of truth; the cold schema tree loads
    # only at this derivation seam.
    from queenbee.recipe.recipe import BakedRecipe, RecipeInterface  # noqa: PLC0415 — boundary import beside the AGPL tree

    return tuple(out.name for out in RecipeInterface.from_recipe(BakedRecipe.from_folder(recipe.path)).outputs)


def _staged(spec: RecipeSpec, root: str) -> "RuntimeRail[_Staged]":
    # ONE coercion seam — engine gate, Recipe construction, input assignment, handled inputs.json; the run key derives from the recipe
    # identity + handled bytes, so an identical simulation elides through the lane cache and the persistence ledger dedupes at the wire.
    from lbt_recipes import version  # noqa: PLC0415 — AGPL boundary import
    from lbt_recipes.recipe import Recipe  # noqa: PLC0415
    from lbt_recipes.settings import RecipeSettings  # noqa: PLC0415

    row = spec.row()
    gate = traverse(
        lambda engine: guarded_sync(RetryClass.ENGINE, getattr(version, ENGINE_CHECK[engine]), subject=f"recipe.engine.{engine}"),
        Block.of_seq(sorted(row.engines)),
    )

    def staged() -> _Staged:
        recipe = Recipe(str(spec.recipe))
        for name, value in spec.inputs.to_seq():
            recipe.input_value_by_name(name, value)
        # handled inputs and the settings default both pin the `_rooted` folder, so coercion, landed assets, and the
        # luigi run share one project root; caller-supplied settings already carry the folder `_rooted` read.
        handled = Path(recipe.write_inputs_json(project_folder=root, indent=0))
        key = ContentIdentity.key("recipe", f"{recipe.name}:{recipe.tag}:".encode() + handled.read_bytes())
        # run policy is data, never a per-call knob; `report_out` stays rejected — `silent=True` owns the report surface, the receipt the evidence.
        settings = spec.settings.default_with(
            lambda: RecipeSettings(folder=root, workers=row.workers, reload_old=row.reload, debug_folder=spec.debug.to_optional())
        )
        return _Staged(recipe=recipe, row=row, settings=settings, outputs=row.outputs or _declared(recipe), key=key)

    return gate.map(lambda _verdicts: staged())


def _execute(staged: _Staged) -> "RuntimeRail[RecipeProduct]":
    # exit path never decides alone: `failure_message` and the luigi summary are the verdict, `error_summary` the structured
    # detail the fault carries.
    folder = staged.recipe.run(settings=staged.settings, silent=True)
    failure = Option.of_optional(staged.recipe.failure_message(folder) or None)
    errors = failure.bind(lambda _: Option.of_optional(staged.recipe.error_summary(folder) or None))
    receipt = RecipeReceipt(
        simulation_id=staged.recipe.simulation_id,
        recipe=staged.recipe.name,
        tag=staged.recipe.tag,
        engines=tuple(sorted(staged.row.engines)),
        summary=staged.recipe.luigi_execution_summary(folder),
        failure=failure,
        errors=errors,
        output_count=len(staged.outputs),
        content_key=staged.key,
    )
    return failure.map(lambda message: Error(BoundaryFault(resource=("recipe", errors.default_value(message))))).default_with(
        lambda: Ok(
            RecipeProduct(
                outputs=Map.of_seq([(name, staged.recipe.output_value_by_name(name, folder)) for name in staged.outputs]),
                receipt=receipt,
            )
        )
    )


def _interface(spec: RecipeSpec) -> "RecipeInterface":
    from lbt_recipes.recipe import Recipe  # noqa: PLC0415 — AGPL boundary import
    from queenbee.recipe.recipe import BakedRecipe, RecipeInterface  # noqa: PLC0415

    return RecipeInterface.from_recipe(BakedRecipe.from_folder(Recipe(str(spec.recipe)).path))


# --- [COMPOSITION] ----------------------------------------------------------------------

# output rows are the recipes' REAL declared output names (each recipe's package.json contract) —
# a row naming an output the recipe never declares breaks output_value_by_name at readback.
RECIPES: Final[Map[RecipeName, RecipeRow]] = Map.of_seq([
    (RecipeName.ANNUAL_DAYLIGHT, RecipeRow(outputs=("da", "cda", "udi", "udi-lower", "udi-upper", "grid-summary"), engines=frozenset({Engine.RADIANCE}), workers=2)),
    (RecipeName.ANNUAL_DAYLIGHT_ENHANCED, RecipeRow(outputs=("da", "cda", "udi", "udi-lower", "udi-upper", "grid-summary"), engines=frozenset({Engine.RADIANCE}), workers=2)),
    (RecipeName.ANNUAL_IRRADIANCE, RecipeRow(outputs=("average-irradiance", "peak-irradiance", "cumulative-radiation"), engines=frozenset({Engine.RADIANCE}), workers=2)),
    (RecipeName.CUMULATIVE_RADIATION, RecipeRow(outputs=("cumulative-radiation", "average-irradiance"), engines=frozenset({Engine.RADIANCE}), workers=2)),
    (RecipeName.DIRECT_SUN_HOURS, RecipeRow(outputs=("direct-sun-hours", "cumulative-sun-hours"), engines=frozenset({Engine.RADIANCE}), workers=2)),
    (RecipeName.DAYLIGHT_FACTOR, RecipeRow(outputs=("results", "grid-summary"), engines=frozenset({Engine.RADIANCE}), workers=2)),
    (RecipeName.POINT_IN_TIME_GRID, RecipeRow(outputs=("results",), engines=frozenset({Engine.RADIANCE}), workers=2)),
    (RecipeName.POINT_IN_TIME_VIEW, RecipeRow(outputs=("results",), engines=frozenset({Engine.RADIANCE}), workers=2)),
    (RecipeName.SKY_VIEW, RecipeRow(outputs=("results",), engines=frozenset({Engine.RADIANCE}), workers=2)),
    (RecipeName.IMAGELESS_ANNUAL_GLARE, RecipeRow(outputs=("ga", "results"), engines=frozenset({Engine.RADIANCE}), workers=2)),
    (RecipeName.ANNUAL_ENERGY_USE, RecipeRow(outputs=("eui", "sql"), engines=frozenset({Engine.OPENSTUDIO, Engine.ENERGYPLUS}), workers=2)),
    (RecipeName.ADAPTIVE_COMFORT_MAP, RecipeRow(outputs=("tcp", "csp", "hsp", "condition", "degrees-from-neutral"), engines=_COMFORT_ENGINES, workers=2)),
    (RecipeName.PMV_COMFORT_MAP, RecipeRow(outputs=("tcp", "csp", "hsp", "condition", "pmv"), engines=_COMFORT_ENGINES, workers=2)),
    (RecipeName.UTCI_COMFORT_MAP, RecipeRow(outputs=("tcp", "csp", "hsp", "condition", "utci", "category"), engines=_COMFORT_ENGINES, workers=2)),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

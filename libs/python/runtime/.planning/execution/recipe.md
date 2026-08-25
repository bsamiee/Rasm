# [PY_RUNTIME_RECIPE]

Local recipe execution — the one owner turning a queenbee-schema simulation workflow into a typed deliverable. `RecipeExecution.execute` is the single modality-polymorphic entry: it absorbs one `RecipeSpec` or a `Block[RecipeSpec]`, acquires remote assets through the `transport/roots#RESOURCE` rail, gates the external Radiance/OpenStudio/EnergyPlus engines through `reliability/resilience#RESILIENCE` `guarded_sync` BEFORE the subprocess spends minutes, executes `queenbee local run` off the event loop through the `execution/lanes#LANE` offload, and reads the typed deliverable back through `output_value_by_name` — the product is the handled output value set, never the raw project folder, and the parsed luigi evidence decides the rail, never the exit path alone. Each run is content-keyed over the recipe identity and the handled `inputs.json` bytes, so a parametric batch drains as `keyed` units and an identical simulation replays from the lane cache rather than re-running the engine.

Recipe VOCABULARY stays queenbee's and the execution machinery stays `lbt-recipes`': this owner re-implements no luigi scheduler, no handler resolution, and no engine-version probe — it composes `Recipe`/`RecipeSettings`/`Recipe.run` and projects the contract through `RecipeInterface.from_recipe` for the submission-constructing consumer (geometry energy/simulate binds `Job`/`RecipeInterface` construction to this owner, the named `python:` consumer). One `RECIPES` seed table is the executable catalog — outputs, engine set, worker sizing as row data — so a new workflow is one row, never a per-recipe method or a sibling runner. `lbt-recipes`/`pollination-handlers` distributions and the `queenbee-local` luigi runner are AGPL-3.0 network copyleft: admissible only under the companion's process-boundary, non-distributed execution charter — engines run as external subprocesses, and nothing links into a distributed host binary. That charter bans the MODULE-SCOPE BINDING itself, not merely the import cost: a static license audit reads the import graph, so an lbt name bound at module scope marks every importer of this module AGPL-coupled, and the deferred-import dialect cannot serve the ban — a `lazy` statement is lexically module-scope by design, carrying the very graph edge whose load it defers. Every lbt binding therefore stays function-local at its boundary seam, confining the lexical coupling to the one function that crosses; annotations resolve through `if TYPE_CHECKING:`, which binds nothing at runtime, and `ENGINE_CHECK` carries probe NAMES its seam resolves by `getattr` because no module-scope `version` handle exists to hold. `queenbee` is MIT and carries no copyleft of its own, but every site reaching it here holds an lbt `Recipe` first, so it rides this one band law rather than a second posture.

## [01]-[INDEX]

- [02]-[RECIPE]: the one `RecipeExecution` owner — `RecipeSpec` request shape, `RECIPES` catalog, content-keyed lane execution with engine prechecks, and the luigi-evidence `RecipeReceipt`.

## [02]-[RECIPE]

- Owner: `RecipeExecution` holds the one `LanePolicy` its runs drain and offload under — capacity and deadline arrive as the caller's `execution/admission#CONTEXT` budget projection at construction, never a per-call knob — and the `Option[ResourceRoot]` remote specs resolve against. `RecipeSpec.recipe` selector discriminates by VALUE — a catalog member or an external folder path — never a `packaged: bool` beside it, and an external folder's empty readback roster derives from the baked `package.json` contract through `_declared`, never a hand-mirrored list.
- Entry: `execute` threads the caller's session cache as a value — a prior `DrainReceipt.cache` re-enters as the next call's carrier, so elision is threaded state, never hidden owner state. `interface` is the schema projection for submission-constructing consumers; execution always returns through `execute`, never a second runner.
- Auto: span presence IS execution evidence — one `recipe.execute` span wraps the executed leg, the engine gate and coercion ride the `guarded` derivation span at staging, and a cache-elided replay opens no execute span. Subprocess environment discipline stays `lbt_recipes`' own (`--env` PATH/RAYPATH, cleared `PYTHONHOME`) — this owner never re-derives the shell line. Deliverables are row-driven and typed: handler-parsed lists and `DataCollection` objects, never a path the caller must re-parse.
- Law: every refusal resolves ONE `reliability/faults#FAULT` `RAISES` anchor under `RuntimeLeg.RECIPE` and derives its subject from that leg — the escaping destination, the luigi error summary, and the engine gate's own window all ride NAMED slots on their rows rather than free subject strings the resilience keys then read.
- Growth: a new simulation workflow is one `RecipeName` member with one `RECIPES` row, its output roster riding the shared anchor the moment a second row declares the same set; a new engine one `Engine` member with one `ENGINE_CHECK` row; a new remote asset kind one `AssetFetch` on the spec; a new run-policy dimension one `RecipeRow` column or one `RecipeSpec` Option folded into the `RecipeSettings` default; the cloud-submission modality (a Pollination platform `Job` POST composing the queenbee shapes against `interface`) enters as one more execute arm with its own `@overload` over the same `RecipeSpec` when a consumer names it, never a parallel owner — an arm with no overload lands every caller on the runtime union.
- Boundary: no luigi scheduling, no handler resolution or chain ordering, no engine probing beside `version.check_*`, and no recipe-schema re-mint — queenbee owns the vocabulary, and a `msgspec`/protobuf mirror of a queenbee model is a single-mint violation. queenbee's click CLI and urllib transfer stay rejected: `cyclopts` and the roots rail own those concerns. No durable run ledger — the session cache is lane-local, and durable reuse stays the C# `Rasm.Persistence` ledger consumed at the wire. Engines are external binaries; no simulation runs in-process.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Buffer, Iterable
from enum import StrEnum
from pathlib import Path
from typing import TYPE_CHECKING, Final, overload

from expression import Error, Nothing, Ok, Option, Result
from expression.collections import Block, Map
from expression.extra.result import traverse
from msgspec import Struct
from opentelemetry import trace

from rasm.runtime.faults import RECIPE_ASSET, RECIPE_ENGINE, RECIPE_ROOT, RECIPE_RUN, SCOPES, BoundaryFault, RuntimeRail, Scope, scoped
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import Admit, LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait
from rasm.runtime.receipts import OPEN, DrainReceipt, Receipt, receipted
from rasm.runtime.resilience import RetryClass, guarded_sync
from rasm.runtime.roots import Delivery, ResourceRoot

if TYPE_CHECKING:
    from lbt_recipes.recipe import Recipe
    from lbt_recipes.settings import RecipeSettings
    from queenbee.recipe.recipe import RecipeInterface

_TRACER: Final[trace.Tracer] = scoped(trace.get_tracer, SCOPES[Scope.RECIPE])

# --- [TYPES] ----------------------------------------------------------------------------


class RecipeName(StrEnum):
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

ENGINE_CHECK: Final[Map[Engine, str]] = Map.of_seq([
    (Engine.RADIANCE, "check_radiance_date"),
    (Engine.OPENSTUDIO, "check_openstudio_version"),
    (Engine.ENERGYPLUS, "check_energyplus_version"),
])

_COMFORT_ENGINES: Final[frozenset[Engine]] = frozenset({Engine.RADIANCE, Engine.OPENSTUDIO, Engine.ENERGYPLUS})
_RADIANCE: Final[frozenset[Engine]] = frozenset({Engine.RADIANCE})
_DAYLIGHT_METRICS: Final[tuple[str, ...]] = ("da", "cda", "udi", "udi-lower", "udi-upper", "grid-summary")
_IRRADIANCE_METRICS: Final[tuple[str, ...]] = ("average-irradiance", "peak-irradiance", "cumulative-radiation")
_COMFORT_METRICS: Final[tuple[str, ...]] = ("tcp", "csp", "hsp", "condition")

# --- [MODELS] ---------------------------------------------------------------------------


class AssetFetch(Struct, frozen=True):
    source: str
    relative: str


class RecipeRow(Struct, frozen=True):
    outputs: tuple[str, ...]
    engines: frozenset[Engine]
    workers: int
    reload: bool = True


class RecipeSpec(Struct, frozen=True):
    recipe: RecipeName | str
    inputs: Map[str, object] = Map.empty()
    settings: "Option[RecipeSettings]" = Nothing
    assets: Block[AssetFetch] = Block.empty()
    outputs: tuple[str, ...] = ()
    engines: frozenset[Engine] = frozenset()
    debug: Option[str] = Nothing

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
    errors: Option[str]
    output_count: int
    content_key: ContentKey

    def contribute(self) -> Iterable[Receipt]:
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
        yield from self.receipt.contribute()


class _Staged(Struct, frozen=True):
    recipe: "Recipe"
    row: RecipeRow
    settings: "RecipeSettings"
    outputs: tuple[str, ...]
    key: ContentKey


# --- [SERVICES] -------------------------------------------------------------------------


class RecipeExecution(Struct, frozen=True):
    lane: LanePolicy
    root: Option[ResourceRoot] = Nothing

    @overload
    async def execute(self, spec: Block[RecipeSpec], cache: Map[ContentKey, RecipeProduct] = ...) -> "DrainReceipt[RecipeProduct]": ...
    @overload
    async def execute(self, spec: RecipeSpec, cache: Map[ContentKey, RecipeProduct] = ...) -> "RuntimeRail[RecipeProduct]": ...
    async def execute(
        self, spec: "RecipeSpec | Block[RecipeSpec]", cache: Map[ContentKey, RecipeProduct] = Map.empty()
    ) -> "RuntimeRail[RecipeProduct] | DrainReceipt[RecipeProduct]":
        match spec:
            case Block() as many:
                units: Block[Admit[RecipeProduct]] = Block.of_seq([await self._admitted(one) for one in many])
                return await self.lane.drain(units, cache)
            case lone:
                staged = await self._prepared(lone)
                return await staged.map(
                    lambda ready: cache.try_find(ready.key).map(_elided).default_with(lambda: self._observed(ready))
                ).default_with(_refused)

    async def interface(self, spec: RecipeSpec) -> "RuntimeRail[RecipeInterface]":
        return await self.lane.offload(Kernel.of(_interface, KernelTrait.RELEASING), spec)

    async def _admitted(self, spec: RecipeSpec) -> Admit[RecipeProduct]:
        staged = await self._prepared(spec)
        return staged.map(lambda ready: Admit(keyed=(ready.key, lambda: self._observed(ready)))).default_with(
            lambda fault: Admit(bare=lambda: _refused(fault))
        )

    async def _prepared(self, spec: RecipeSpec) -> "RuntimeRail[_Staged]":
        rooted = await self.lane.offload(Kernel.of(_rooted, KernelTrait.RELEASING), spec)
        if rooted.is_error():
            return Error(rooted.error)
        acquired = await self._acquired(spec.assets, Path(rooted.ok))
        if acquired.is_error():
            return Error(acquired.error)
        return (await self.lane.offload(Kernel.of(_staged, KernelTrait.RELEASING), spec, rooted.ok)).bind(lambda rail: rail)

    async def _acquired(self, assets: Block[AssetFetch], root: Path) -> "RuntimeRail[int]":
        if assets.is_empty():
            return Ok(0)
        return await self.root.map(lambda live: self._fetched(live, assets, root)).default_with(
            lambda: _refused(RECIPE_ROOT.raised())
        )

    async def _fetched(self, live: ResourceRoot, assets: Block[AssetFetch], root: Path) -> "RuntimeRail[int]":
        roster = traverse(lambda asset: _confined(root, asset.relative), assets)
        if roster.is_error():
            return Error(roster.error)
        landed: RuntimeRail[int] = Ok(0)
        for asset, destination in zip(assets, roster.ok, strict=True):
            match await live.child(asset.source).map(lambda ref: live.read(ref, Delivery.WHOLE)).default_with(_refused):
                case Result(tag="error", error=fault):
                    return Error(fault)
                case Result(tag="ok", ok=payload):
                    written = await self.lane.offload(Kernel.of(_landed, KernelTrait.RELEASING), str(destination), payload)
                    landed = landed.bind(lambda n: written.map(lambda one: n + one))
            if landed.is_error():
                return landed
        return landed

    async def _observed(self, staged: _Staged) -> "RuntimeRail[RecipeProduct]":
        with _TRACER.start_as_current_span("recipe.execute"):
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
    from lbt_recipes.recipe import Recipe

    root = Path(
        spec.settings.bind(lambda held: Option.of_optional(held.folder)).default_with(lambda: Recipe(str(spec.recipe)).default_project_folder)
    ).resolve()
    root.mkdir(parents=True, exist_ok=True)
    return str(root)


def _confined(root: Path, relative: str) -> "RuntimeRail[Path]":
    candidate = Path(relative)
    resolved = (root / candidate).resolve()
    return (
        Error(RECIPE_ASSET.raised(relative))
        if candidate.is_absolute() or not resolved.is_relative_to(root)
        else Ok(resolved)
    )


def _landed(destination_path: str, got: Buffer) -> int:
    destination = Path(destination_path)
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(bytes(got))
    return 1


def _declared(recipe: "Recipe") -> tuple[str, ...]:
    from queenbee.recipe.recipe import BakedRecipe, RecipeInterface

    return tuple(out.name for out in RecipeInterface.from_recipe(BakedRecipe.from_folder(recipe.path)).outputs)


def _staged(spec: RecipeSpec, root: str) -> "RuntimeRail[_Staged]":
    from lbt_recipes import version
    from lbt_recipes.recipe import Recipe
    from lbt_recipes.settings import RecipeSettings

    row = spec.row()
    gate = traverse(
        lambda engine: guarded_sync(RetryClass.ENGINE, getattr(version, ENGINE_CHECK[engine]), at=RECIPE_ENGINE),
        Block.of_seq(sorted(row.engines)),
    )

    def staged() -> _Staged:
        recipe = Recipe(str(spec.recipe))
        for name, value in spec.inputs.to_seq():
            recipe.input_value_by_name(name, value)
        handled = Path(recipe.write_inputs_json(project_folder=root, indent=0))
        key = ContentIdentity.key("recipe", f"{recipe.name}:{recipe.tag}:".encode() + handled.read_bytes())
        settings = spec.settings.default_with(
            lambda: RecipeSettings(folder=root, workers=row.workers, reload_old=row.reload, debug_folder=spec.debug.to_optional())
        )
        return _Staged(recipe=recipe, row=row, settings=settings, outputs=row.outputs or _declared(recipe), key=key)

    return gate.map(lambda _verdicts: staged())


def _execute(staged: _Staged) -> "RuntimeRail[RecipeProduct]":
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
    return failure.map(lambda message: Error(RECIPE_RUN.raised(errors.default_value(message)))).default_with(
        lambda: Ok(
            RecipeProduct(
                outputs=Map.of_seq([(name, staged.recipe.output_value_by_name(name, folder)) for name in staged.outputs]),
                receipt=receipt,
            )
        )
    )


def _interface(spec: RecipeSpec) -> "RecipeInterface":
    from lbt_recipes.recipe import Recipe
    from queenbee.recipe.recipe import BakedRecipe, RecipeInterface

    return RecipeInterface.from_recipe(BakedRecipe.from_folder(Recipe(str(spec.recipe)).path))


# --- [COMPOSITION] ----------------------------------------------------------------------

RECIPES: Final[Map[RecipeName, RecipeRow]] = Map.of_seq([
    (RecipeName.ANNUAL_DAYLIGHT, RecipeRow(outputs=_DAYLIGHT_METRICS, engines=_RADIANCE, workers=2)),
    (RecipeName.ANNUAL_DAYLIGHT_ENHANCED, RecipeRow(outputs=_DAYLIGHT_METRICS, engines=_RADIANCE, workers=2)),
    (RecipeName.ANNUAL_IRRADIANCE, RecipeRow(outputs=_IRRADIANCE_METRICS, engines=_RADIANCE, workers=2)),
    (RecipeName.CUMULATIVE_RADIATION, RecipeRow(outputs=("cumulative-radiation", "average-irradiance"), engines=_RADIANCE, workers=2)),
    (RecipeName.DIRECT_SUN_HOURS, RecipeRow(outputs=("direct-sun-hours", "cumulative-sun-hours"), engines=_RADIANCE, workers=2)),
    (RecipeName.DAYLIGHT_FACTOR, RecipeRow(outputs=("results", "grid-summary"), engines=_RADIANCE, workers=2)),
    (RecipeName.POINT_IN_TIME_GRID, RecipeRow(outputs=("results",), engines=_RADIANCE, workers=2)),
    (RecipeName.POINT_IN_TIME_VIEW, RecipeRow(outputs=("results",), engines=_RADIANCE, workers=2)),
    (RecipeName.SKY_VIEW, RecipeRow(outputs=("results",), engines=_RADIANCE, workers=2)),
    (RecipeName.IMAGELESS_ANNUAL_GLARE, RecipeRow(outputs=("ga", "results"), engines=_RADIANCE, workers=2)),
    (RecipeName.ANNUAL_ENERGY_USE, RecipeRow(outputs=("eui", "sql"), engines=frozenset({Engine.OPENSTUDIO, Engine.ENERGYPLUS}), workers=2)),
    (RecipeName.ADAPTIVE_COMFORT_MAP, RecipeRow(outputs=(*_COMFORT_METRICS, "degrees-from-neutral"), engines=_COMFORT_ENGINES, workers=2)),
    (RecipeName.PMV_COMFORT_MAP, RecipeRow(outputs=(*_COMFORT_METRICS, "pmv"), engines=_COMFORT_ENGINES, workers=2)),
    (RecipeName.UTCI_COMFORT_MAP, RecipeRow(outputs=(*_COMFORT_METRICS, "utci", "category"), engines=_COMFORT_ENGINES, workers=2)),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

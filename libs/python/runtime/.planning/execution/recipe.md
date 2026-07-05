# [PY_RUNTIME_RECIPE]

Local recipe execution — the one owner that turns a queenbee-schema simulation workflow into a typed deliverable. `RecipeExecution.execute` is the single modality-polymorphic entry: it absorbs one `RecipeSpec` or a `Block[RecipeSpec]` over one `match` head, acquires any remote recipe-folder/weather assets through the `transport/roots#RESOURCE` `Transfer` rail, gates the external Radiance/OpenStudio/EnergyPlus engines through the `reliability/resilience#RESILIENCE` `guarded_sync` envelope on the `RetryClass.ENGINE` row BEFORE the subprocess spawns, coerces every input through `lbt_recipes` `Recipe.write_inputs_json` (the one `pollination_handlers`-chained coercion seam), executes `queenbee local run` off the event loop through the `execution/lanes#LANE` offload under the lane's one deadline and slot allocator, then reads the typed deliverable back through `output_value_by_name` — the product is the handled output value set, never the raw project folder, and the subprocess exit path is never the sole success signal: the parsed `luigi_execution_summary`/`error_summary`/`failure_message` evidence decides the rail. Each run is content-keyed by `evidence/identity#IDENTITY` `ContentIdentity.key` over the recipe identity plus the handled `inputs.json` bytes, so a parametric batch drains as `keyed` `Admit` units and an identical simulation replays its `Ok` from the threaded lane cache rather than re-running the engine.

The recipe VOCABULARY stays queenbee's and the execution machinery stays `lbt-recipes`': this owner re-implements no luigi DAG scheduler, no handler resolution, no `IOAliasHandler` chain order, and no engine-version probe — it composes `Recipe(recipe_name)`/`RecipeSettings`/`Recipe.run` and projects the recipe contract through `RecipeInterface.from_recipe` for a sibling that constructs a submission schema (`geometry` energy/simulate binds `Job`/`RecipeInterface` construction to this owner's execution — the named `python:` consumer). The catalog of executable recipes is one `RECIPES` seed table — output names, engine set, and worker sizing as row data — so a new simulation workflow is one row, never a per-recipe method or a sibling runner class. The `lbt-recipes`/`pollination-handlers` distributions (and the `queenbee-local` luigi runner the subprocess reaches) are AGPL-3.0 network copyleft: admissible only under the companion's process-boundary, non-distributed execution charter — the engines run as external subprocesses, the AGPL tree imports function-local at the execution boundary, and nothing links into a distributed host binary.

## [01]-[INDEX]

- [01]-[RECIPE]: the one `RecipeExecution` owner — the `RecipeSpec` request shape, the `RECIPES` catalog table with its contract-derived readback fallback, the content-keyed lane execution with engine prechecks and THREAD-offloaded asset landing, the handled `output_value_by_name` deliverable, and the luigi-evidence `RecipeReceipt` carrying the parsed `error_summary`.

## [02]-[RECIPE]

- Owner: `RecipeExecution` — the frozen execution owner holding the one `LanePolicy` its runs drain and offload under (capacity and deadline arrive as the caller's `execution/admission#CONTEXT` budget projection at construction, never a per-call knob) and the `Option[ResourceRoot]` asset root remote specs resolve against; `RecipeSpec` the one request shape carrying the `RecipeName | str` recipe selector (a catalog row member or an external recipe-folder path — the discriminant is the value, never a `packaged: bool`), the `Map[str, object]` input assignments keyed by recipe input name, the `Option[RecipeSettings]` run settings, the `Block[AssetFetch]` remote acquisitions staged before coercion, the caller-named `outputs` tuple and `engines` gate an external folder supplies where a catalog row carries its own, and the `debug` capture Option threading `RecipeSettings.debug_folder`; `RecipeRow` the catalog row — the `outputs` tuple `output_value_by_name` reads back, the `engines` frozenset the precheck gate folds, the `workers` sizing hint, and the `reload` run-reuse column pairing `RecipeSettings.reload_old` with the content-key elision — so the recipe catalog is seed DATA over one generative execute shape, and an empty readback roster derives from the baked `package.json` contract itself through `_declared` rather than a hand-mirrored list; `RecipeProduct` the deliverable pairing the handled `outputs: Map[str, object]` readback with the typed `RecipeReceipt`; `RecipeReceipt` the `ReceiptContributor` evidence — simulation id, recipe name/tag, per-engine verdict presence, the parsed luigi summary, the `Option[str]` failure message, the `Option[str]` parsed `error_summary` the failure arm alone reads, output count, and the run `ContentKey`.
- Cases: `RecipeName` rows the fourteen packaged Ladybug Tools recipes — `ANNUAL_DAYLIGHT` · `ANNUAL_DAYLIGHT_ENHANCED` · `ANNUAL_IRRADIANCE` · `CUMULATIVE_RADIATION` · `DIRECT_SUN_HOURS` · `DAYLIGHT_FACTOR` · `POINT_IN_TIME_GRID` · `POINT_IN_TIME_VIEW` · `SKY_VIEW` · `IMAGELESS_ANNUAL_GLARE` · `ANNUAL_ENERGY_USE` · `ADAPTIVE_COMFORT_MAP` · `PMV_COMFORT_MAP` · `UTCI_COMFORT_MAP` — each keying one `RECIPES` row (`Recipe.__init__` normalizes the hyphenated value onto the underscore install folder); `Engine` rows `RADIANCE` · `OPENSTUDIO` · `ENERGYPLUS`, each keying its `lbt_recipes.version` check name in `ENGINE_CHECK` so the precheck gate is a table fold, never three hand-called functions; `AssetFetch` pairs a root-relative source with its project-relative destination so a remote EPW/recipe-folder/asset lands through the roots rail before any handler reads it.
- Entry: `RecipeExecution.execute` absorbs `RecipeSpec | Block[RecipeSpec]` beside the one caller-threaded `Map[ContentKey, RecipeProduct]` session cache — a prior `DrainReceipt.cache` re-enters as the next call's carrier, so elision is a threaded value, never hidden owner state. A lone spec runs one content-keyed offloaded execution and resolves `RuntimeRail[RecipeProduct]`, the cache probe replaying an already-keyed run through `_elided` with no span opened; a `Block` folds each spec into a `keyed` `Admit` unit (the run's `ContentKey` the cache probe) and drains the batch through `self.lane.drain` under the same carrier, so a parametric sweep rides the lane's one `CapacityLimiter`/`move_on_after` scope, an identical spec replays its `Ok` from the threaded session cache, and the batch resolves one `DrainReceipt[RecipeProduct]` carrying values, faults, cache, and the five-column tally. The per-spec pipeline is one staged fold: `_acquired` stages every `AssetFetch` through `ResourceRoot.read` (whole-modality; the roots plan carries its own retry class) and lands each payload at its project-relative destination through the lane's THREAD offload — the `mkdir`/`write_bytes` landing is blocking filesystem work that never sits on the event loop — `_staged` — itself offloaded on the lane's THREAD modality, because the handler chains copy artifact trees and write the handled JSON, blocking work that never sits on the event loop — constructs `Recipe(...)`, folds the row's `engines` through `guarded_sync(RetryClass.ENGINE, check, subject=...)` so a transiently-failing probe retries under one stamina row and a genuinely missing engine resolves a typed `BoundaryFault` BEFORE the subprocess spends minutes, assigns each input through `input_value_by_name`, calls `write_inputs_json` — the ONE coercion seam where every `pollination_handlers` input chain runs and the handled JSON materializes — keys the run with `ContentIdentity.key("recipe", ...)` over the recipe name, tag, and handled-inputs bytes, folds the run policy into the `RecipeSettings` default (`workers` and `reload_old` off the row, `debug_folder` off the spec), and resolves the readback roster (row/caller names, else the baked contract's own `_declared` output set), `_execute` (the offloaded kernel) runs the blocking `Recipe.run(settings, silent=True)` child-process wait on the lane's THREAD modality (never an event-loop stall; the lane's deadline and limiter bound it exactly as every drain unit), and the same kernel reads `luigi_execution_summary` for the run evidence, treats a non-empty `failure_message` as the fault regardless of the exit path — the fault detail carrying the parsed `error_summary` where the log walk yields one — and folds the staged roster through `output_value_by_name` into the handled `Map[str, object]` deliverable. `interface` is the schema projection for submission-constructing consumers: it loads the recipe's `package.json` contract as a queenbee `BakedRecipe` and returns `RecipeInterface.from_recipe(...)` on the lane's THREAD offload — the baked-folder parse is blocking I/O exactly as the staged and executed kernels are — and geometry's energy plane constructs `Job`/`JobArgument` shapes against this projection and hands execution back to `execute`, never a second runner.
- Auto: one `recipe.execute` span minted from the `reliability/faults#FAULT` instrumentation-scope row wraps the executed leg — the offloaded run and readback — while the engine gate and coercion ride the `resilience.guarded` derivation span at staging and a cache-elided replay opens no `recipe.execute` span, so span presence IS execution evidence (never four sibling spans, never a phantom execute span on an elided run), and the `@receipted(OPEN)` aspect harvests the `RecipeReceipt.contribute` stream on the cleared `Ok` so telemetry and receipt egress ride composition, never call-site threading; every foreign raise converts exactly once — the AGPL import, the handler `ValueError` a failing input precondition raises, the subprocess spawn `OSError`, and the log-parse fault all cross the offload fence into `BoundaryFault`, and the interior past the fence is total over the rail; the subprocess environment discipline stays `lbt_recipes`' own (`--env` PATH/RAYPATH, cleared `PYTHONHOME`) — this owner never re-derives the shell line, and `RecipeSettings.workers` defaults from the row's `workers` hint rather than a hardcoded count; the deliverable readback is row-driven — a recipe whose `results` output is a folder of per-grid metrics returns the handler-parsed lists/`DataCollection` objects the `pollination_handlers.outputs` readers produce, so the product crosses to the caller typed, never as a path the caller must re-parse.
- Packages: `queenbee` (`BakedRecipe.from_folder`/`RecipeInterface.from_recipe` the schema projection, `Job` + `queenbee.io.inputs.job` `JobArgument`/`JobPathArgument` the submission shapes a consumer constructs against `interface` — schema only; queenbee's click CLI and urllib transfer stay rejected, `cyclopts` and the roots rail own those concerns), `lbt-recipes` (`Recipe(recipe_name)`/`input_value_by_name`/`write_inputs_json(project_folder, indent, cpu_count)`/`run(settings, radiance_check, openstudio_check, energyplus_check, queenbee_path, silent, debug_folder)`/`output_value_by_name(output_name, project_folder)`/`luigi_execution_summary`/`error_summary`/`failure_message`, `RecipeSettings(folder, workers, reload_old, report_out, debug_folder)`, `version.check_radiance_date`/`check_openstudio_version`/`check_energyplus_version` — all bound function-local inside the boundary kernels; AGPL-3.0, process-boundary companion execution only), `pollination-handlers` (the `inputs.*`/`outputs.*` coercion functions `lbt_recipes` resolves by `importlib` from each recipe's `IOAliasHandler` spec — never imported statically here; AGPL-3.0, same posture), `expression` (`Block`/`Map`/`Option`/`Ok`/`Error`), `msgspec` (`Struct` the frozen carriers), `opentelemetry-api` (the one scope-row-minted tracer), stdlib `pathlib` (the handled `inputs.json` bytes the run key digests), runtime (`ContentIdentity`/`ContentKey` the run key, `RuntimeRail`/`BoundaryFault` the rail — the lane offload fence converts the staged, executed, and projection kernels' raises, `RetryClass`/`guarded_sync` the engine gate, `LanePolicy`/`Admit`/`DrainReceipt` the execution lane, `ResourceRoot`/`ReadModality` the asset rail, `Receipt`/`OPEN`/`receipted` the evidence egress).
- Growth: a new simulation workflow is one `RecipeName` member plus one `RECIPES` row (outputs, engines, workers, reload) — never a runner subclass; an external recipe folder is a `str` selector whose `outputs`/`engines` ride the spec or derive from the baked contract, no new entry; a new engine is one `Engine` member plus one `ENGINE_CHECK` row; a new remote asset kind is one `AssetFetch` on the spec; a new run-policy dimension is one `RecipeRow` column or one `RecipeSpec` Option folded into the `RecipeSettings` default, never a per-call knob; a submission-schema consumer composes `interface` — zero new surface; the deferred cloud-submission modality (a Pollination platform `Job` POST composing the queenbee `Job`/`JobArgument`/`JobPathArgument` shapes against `interface`) enters as one more execute arm over the same `RecipeSpec` when a consumer names it, never a parallel owner.
- Boundary: no luigi scheduling, no handler resolution or chain ordering (`lbt_recipes._RecipeParameter` owns `importlib` binding), no engine-version probing beside `version.check_*`, no recipe-schema re-mint (queenbee owns the vocabulary; a `msgspec`/protobuf mirror of a queenbee model is the deleted single-mint violation), no durable run ledger (the session cache is lane-local; durable reuse stays the C# `Rasm.Persistence` ledger consumed at the wire), and no in-process simulation — the engines are external binaries. The deleted forms: a bare `recipe.run()` on the event loop where the lane offload owns the blocking wait; trusting the exit code where `failure_message`/`luigi_execution_summary` decide the rail; returning the project folder where the row's handled outputs are the product; a `radiance_check=True` flag threaded per call where the row's `engines` fold the gate once; a per-recipe `run_annual_daylight`/`run_energy_use` function family where one `execute` discriminates on the spec; a module-top `import lbt_recipes` that loads the AGPL honeybee tree into every companion start where the function-local boundary import defers it to first execution; a hand-opened `stamina` block where `guarded_sync(RetryClass.ENGINE, ...)` rides the one policy row; an un-keyed batch where the `ContentKey` elides identical simulations; an event-loop `mkdir`/`write_bytes` in the acquisition fold where the THREAD offload owns the blocking landing; a success-path `error_summary` log walk where only the failure arm pays the parse; a hand-mirrored readback roster for an external folder where `_declared` reads the baked contract's own `outputs`; `report_out` threading where `silent=True` owns the report surface and the receipt the evidence; and a second submission runner beside `interface` where the schema projection already serves the constructing consumer.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Buffer, Iterable
from enum import StrEnum
from pathlib import Path
from typing import TYPE_CHECKING, Final

from expression import Error, Nothing, Ok, Option
from expression.collections import Block, Map
from expression.extra.result import traverse
from msgspec import Struct
from opentelemetry import trace

from rasm.runtime.faults import SCOPES, BoundaryFault, RuntimeRail, Scope
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import Admit, LanePolicy, Modality
from rasm.runtime.receipts import OPEN, DrainReceipt, Receipt, receipted
from rasm.runtime.resilience import RetryClass, guarded_sync
from rasm.runtime.roots import ReadModality, ResourceRoot

if TYPE_CHECKING:  # the AGPL lbt tree loads function-local at the execution boundary; annotations resolve here
    from lbt_recipes.recipe import Recipe
    from lbt_recipes.settings import RecipeSettings
    from queenbee.recipe.recipe import RecipeInterface

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
        # the receipts owner's Evidence triple routes the `fact` case; the full luigi summary stays
        # on this struct — the emitted fact carries the scalar evidence alone.
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
        # structural ReceiptContributor conformance — the product delegates to its receipt so the
        # @receipted(OPEN) _emit harvest reads one stream, never a second contributor shape.
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
        # schema projection for submission-constructing consumers (geometry energy/simulate builds
        # queenbee Job/JobArgument against this; execution always returns through `execute`); the
        # baked-contract folder parse is blocking I/O, so the kernel rides the lane's THREAD
        # offload and its raises cross the one offload fence.
        return await self.lane.offload(_interface, spec, modality=Modality.THREAD)

    async def _admitted(self, spec: RecipeSpec) -> Admit[RecipeProduct]:
        staged = await self._prepared(spec)
        return staged.map(lambda ready: Admit(keyed=(ready.key, lambda: self._observed(ready)))).default_with(
            lambda fault: Admit(bare=lambda: _refused(fault))
        )

    async def _prepared(self, spec: RecipeSpec) -> "RuntimeRail[_Staged]":
        # staging is blocking work (engine probes, Recipe construction, handler coercion copying
        # artifact trees into the project folder), so it rides the lane's THREAD modality — the
        # offload fence converts the handler/spawn raises; the acquisition fault short-circuits.
        acquired = await self._acquired(spec.assets)
        if acquired.is_error():
            return acquired
        return (await self.lane.offload(_staged, spec, modality=Modality.THREAD)).bind(lambda rail: rail)

    async def _acquired(self, assets: Block[AssetFetch]) -> "RuntimeRail[int]":
        if assets.is_empty():
            return Ok(0)
        return await self.root.map(lambda live: self._fetched(live, assets)).default_with(
            lambda: _refused(BoundaryFault(resource=("recipe", "asset-fetch-without-root")))
        )

    async def _fetched(self, live: ResourceRoot, assets: Block[AssetFetch]) -> "RuntimeRail[int]":
        landed: RuntimeRail[int] = Ok(0)
        for asset in assets:  # Exemption: async sequential acquisition — each read awaits the roots rail, each landing awaits the THREAD hop, the carrier rebinds per step.
            match await live.child(asset.source).map(lambda ref: live.read(ref, ReadModality.WHOLE)).default_with(_refused):
                case Error(_) as refused:
                    return refused
                case Ok(payload):
                    written = await self.lane.offload(_landed, asset.relative, payload, modality=Modality.THREAD)
                    landed = landed.bind(lambda n: written.map(lambda one: n + one))
            if landed.is_error():
                return landed
        return landed

    async def _observed(self, staged: _Staged) -> "RuntimeRail[RecipeProduct]":
        # the blocking `queenbee local run` child-process wait crosses the lane on the THREAD
        # modality under the lane's one deadline and limiter; the kernel below owns the AGPL calls,
        # the flatten joins the offload rail onto the kernel rail, and _emit harvests the receipt.
        with trace.get_tracer(SCOPES[Scope.RECIPE]).start_as_current_span("recipe.execute"):
            return (await self.lane.offload(_execute, staged, modality=Modality.THREAD)).bind(lambda rail: rail).map(_emit)


# --- [OPERATIONS] -----------------------------------------------------------------------


async def _elided(product: RecipeProduct) -> "RuntimeRail[RecipeProduct]":
    return Ok(product)


async def _refused[T](fault: BoundaryFault) -> "RuntimeRail[T]":
    return Error(fault)


@receipted(OPEN)
def _emit(product: RecipeProduct) -> RecipeProduct:
    return product


def _landed(relative: str, got: Buffer) -> int:
    # the acquired payload lands at the project-relative destination the handler chains read; the
    # mkdir/write is blocking filesystem work, so the kernel crosses the lane on the THREAD
    # modality — never an event-loop stall inside the async acquisition fold.
    destination = Path(relative)
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(bytes(got))
    return 1


def _declared(recipe: "Recipe") -> tuple[str, ...]:
    # the readback roster the baked `package.json` contract itself declares: an external folder
    # with no caller-named outputs still reads back its full declared set, and the queenbee
    # contract — never a hand-mirrored list — is the roster's source of truth. Cold schema tree;
    # loads only at this derivation seam.
    from queenbee.recipe.recipe import BakedRecipe, RecipeInterface  # noqa: PLC0415 — boundary import beside the AGPL tree

    return tuple(out.name for out in RecipeInterface.from_recipe(BakedRecipe.from_folder(recipe.path)).outputs)


def _staged(spec: RecipeSpec) -> "RuntimeRail[_Staged]":
    # ONE coercion seam: engine gate, Recipe construction, per-input assignment, and the handled
    # inputs.json; the run key derives from the recipe identity + handled bytes so an identical
    # simulation elides through the lane cache and the persistence ledger dedupes at the wire.
    # Runs on the lane's THREAD modality; a handler ValueError crosses the offload fence.
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
        handled = Path(recipe.write_inputs_json(indent=0))
        key = ContentIdentity.key("recipe", f"{recipe.name}:{recipe.tag}:".encode() + handled.read_bytes())
        # run policy is data, never a per-call knob: `reload_old` is the row's reuse column and
        # `debug_folder` the spec's capture Option; `report_out` stays rejected — silent=True owns
        # the report surface, the receipt the evidence.
        settings = spec.settings.default_with(
            lambda: RecipeSettings(workers=row.workers, reload_old=row.reload, debug_folder=spec.debug.to_optional())
        )
        return _Staged(recipe=recipe, row=row, settings=settings, outputs=row.outputs or _declared(recipe), key=key)

    return gate.map(lambda _verdicts: staged())


def _execute(staged: _Staged) -> "RuntimeRail[RecipeProduct]":
    # subprocess leg + evidence readback; the exit path never decides alone — failure_message and
    # the luigi summary are the verdict, `error_summary` the structured detail the fault carries,
    # and the deliverable is the handled output value set over the staged readback roster.
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

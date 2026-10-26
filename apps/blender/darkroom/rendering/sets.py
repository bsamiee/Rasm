"""Darkroom render setups: render set categories, the setup capture of category settings and chain entries with their locks, and setup tiles over the render proxy."""

from collections.abc import Generator
from contextlib import contextmanager
from functools import cached_property, partial
from types import GenericAlias
from typing import Annotated, Final, Self

import bpy
from cycles.presets import AddPresetIntegrator, AddPresetPerformance, AddPresetSampling, AddPresetViewportSampling
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
from typing_extensions import TypeForm

from blender.bpy.commands.operators import Call, Done, Operator, Own
from blender.bpy.data.rna.descriptors import rooted
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.render.compositor.effects import EffectName
from blender.bpy.render.compositor.entries import Configured, projected
from blender.bpy.render.outputs.framing import Framing
from blender.bpy.render.sessions.sets import Categories, Category, CategorySettings, Members, SetupScope
from blender.bpy.settings.attachments import Attachment, Stored
from blender.bpy.settings.properties import Changed, Choices
from blender.bpy.ui.layout.rows import Line, Member, OperatorButton, realized, RowContent, RowDraw
from blender.bpy.ui.layout.templates import IconView
from blender.bpy.ui.layout.values import hosted, Verb
from blender.records.faults import accumulated, DomainFault, paired
from blender.records.fields import Case

from ..chain.previews import Recipe, Tiles
from ..chain.stack import Chain
from ..chain.stages import Effect

# --- [TYPES] ----------------------------------------------------------------------------

type FieldPath = tuple[Key | int, ...]

# --- [CONSTANTS] ------------------------------------------------------------------------

OWNER: Final = "effect_lock_owner"

# --- [MODELS] ---------------------------------------------------------------------------


class Setup[R: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Category settings of a scene and its view layers, and the chain entries it holds by name in stack order."""

    settings: CategorySettings
    effects: dict[EffectName, Configured[R]]


class LockRequest(msgspec.Struct, frozen=True, kw_only=True):
    """Field path a lock button toggles on the entry it drew, as a JSON array, the empty array locking the entry whole."""

    path: str = "[]"


class EffectLocks(msgspec.Struct, frozen=True, kw_only=True):
    """Field paths of each entry setups leave unchanged, the empty path locking the entry whole."""

    held: dict[EffectName, frozenset[FieldPath]] = msgspec.field(default_factory=dict)

    @staticmethod
    def bound(identity: Identity) -> BlenderResult[Stored["EffectLocks"]]:
        """Binds the scene entry lock store."""
        return identity.name(Name.ATTACHMENT, "render_effect_locks").map(lambda name: Stored(name=name, decoder=msgspec.msgpack.Decoder(EffectLocks)))

    @staticmethod
    def of(store: Stored["EffectLocks"], scene: bpy.types.Scene) -> BlenderResult["EffectLocks"]:
        """Reads scene entry locks, none before the first lock."""
        return store.read(scene).map(lambda held: held.default_with(EffectLocks))

    @staticmethod
    def read(identity: Identity, scene: bpy.types.Scene) -> BlenderResult["EffectLocks"]:
        """Binds the store and reads scene entry locks."""
        return EffectLocks.bound(identity).bind(lambda store: EffectLocks.of(store, scene))

    @property
    def entries(self) -> frozenset[EffectName]:
        """Entries locked whole."""
        return frozenset(name for name, paths in self.held.items() if () in paths)

    def locking(self, name: EffectName, path: FieldPath) -> tuple[bool, Self]:
        """Answers whether the entry holds the path's lock, with the locks toggling it."""
        paths = self.held.get(name, frozenset())
        return path in paths, msgspec.structs.replace(self, held=self.held | {name: paths ^ {path}})

    def kept(self, current: dict[EffectName, Configured[Effect]], recipe: dict[EffectName, Configured[Effect]]) -> dict[EffectName, Configured[Effect]]:
        """Projects recipe entries over current ones, locked entries and fields kept."""
        return projected(current, recipe, locked=self.entries, fields=self.held)


class SetupPick(msgspec.Struct, frozen=True, kw_only=True):
    """Setup preset a tile pick applies."""

    setup: Annotated[str, msgspec.Meta(title="Setup", description="Render setup preset applied to unlocked members")] = ""


# --- [SERVICES] -------------------------------------------------------------------------


class Setups(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Render set capture of category settings and chain entries, entry locks kept by every setup."""

    chain: Chain
    tiles: Tiles
    categories: tuple[Category, ...] = ()

    @cached_property
    def settings(self) -> Categories:
        """Category capture the entries extend."""
        return Categories(categories=self.categories)

    @cached_property
    def form(self) -> TypeForm[Setup[Effect]]:
        """Setup record over the chain's offered effect records."""
        return GenericAlias(Setup, (self.chain.rows.form,))  # type: ignore[return-value]  # ty: ignore[invalid-type-form]

    def captured(self, members: Members, scene: bpy.types.Scene, /) -> BlenderResult[Setup[Effect]]:
        """Captures category settings and chain entries."""
        return paired(self.settings.captured(members, scene), self.chain.rows.captured(members.identity, scene)).map(lambda pair: Setup(settings=pair[0], effects=pair[1]))

    def projected(self, members: Members, scene: bpy.types.Scene, recipe: Setup[Effect], current: Setup[Effect], /) -> BlenderResult[tuple[Setup[Effect], Setup[Effect]]]:
        """Projects recipe settings and entries over current ones, setting and entry locks kept."""
        return paired(self.settings.projected(members, scene, recipe.settings, current.settings), EffectLocks.read(members.identity, scene)).map(
            lambda parts: (Setup(settings=parts[0][0], effects=recipe.effects), Setup(settings=parts[0][1], effects=parts[1].kept(current.effects, recipe.effects)))
        )

    def applied(self, members: Members, scene: bpy.types.Scene, sparse: Setup[Effect], /) -> BlenderResult[None]:
        """Writes sparse settings and entries, locked entries and fields kept."""
        return paired(
            self.settings.applied(members, scene, sparse.settings),
            EffectLocks.read(members.identity, scene).bind(lambda locks: self.chain.rows.applied(members.identity, scene, sparse.effects, locked=locks.entries, fields=locks.held)),
        ).map(lambda _: None)

    def rows(self, _members: Members, scope: SetupScope[Setup[Effect]], /) -> tuple[RowContent, ...]:
        """Draws setup tiles above the member lists."""
        return (SetupGallery(setups=self, scope=scope),)

    def recipe(self, identity: Identity, scene: bpy.types.Scene, setup: Setup[Effect]) -> BlenderResult[Recipe]:
        """Projects setup entries over the scene's entries, locks kept, as the preview-enabled chain."""
        return paired(self.chain.rows.captured(identity, scene), EffectLocks.read(identity, scene)).map(
            lambda parts: Recipe(entries=tuple(held.entry for held in parts[1].kept(parts[0], setup.effects).values() if held.preview))
        )


class SetupGallery(Line, frozen=True, kw_only=True):
    """Setup presets as tiles of the chain each projects over the render proxy, a pick applying one."""

    setups: Setups
    scope: SetupScope[Setup[Effect]]

    @property
    def attachment(self) -> Attachment[SetupPick]:
        """Picked preset holder listing preset tiles, each pick applying its preset."""
        preset = self.scope.listing.preset

        def items(identity: Identity, _owner: "bpy.types.bpy_struct[object]", context: bpy.types.Context | None) -> tuple[tuple[str, str, str, int, int], ...]:
            def tiled(scene: bpy.types.Scene, stem: str) -> int:
                return (
                    preset
                    .found(identity, stem)
                    .bind(preset.read)
                    .bind(lambda recipe: self.setups.recipe(identity, scene, recipe))
                    .to_option()
                    .bind(lambda recipe: Tiles.icon(identity, f"{preset.key} {scene.session_uid} {stem}", recipe))
                    .default_value(0)
                )

            scenes = Option.of_optional(context).bind(lambda held: rooted(bpy.types.Scene, held).to_option()).to_list()
            return tuple((stem, name, tip, tiled(scene, stem), index) for scene in scenes for index, (stem, name, tip) in enumerate(preset.entries(identity)))

        return Attachment(key="render_setup_pick", form=SetupPick, owner=bpy.types.WindowManager, changed=Some(self.picking), bindings=Some({vars(SetupPick)["setup"]: Choices(items)}))

    @contextmanager
    def picking(self, identity: Identity) -> Generator[Changed]:
        """Defers one apply per pick, its undo step named by the apply verb."""
        preset = self.scope.listing.preset

        def applied(stem: str) -> BlenderResult[Option[str]]:
            return (
                preset
                .found(identity, stem)
                .bind(preset.read)
                .bind(lambda recipe: self.scope.applied(bpy.context, recipe))
                .map(lambda outcome: Some(Verb.APPLY.title) if isinstance(outcome, Done) else Nothing)
            )

        with identity.reports.deferred(applied) as deferred:
            yield lambda owner, _context, prop: deferred(getattr(owner, prop.identifier))

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Acquires the tiles and the pick holder."""
        return Ok((self.setups.tiles, self.attachment))

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Draws preset tiles at the host preview size."""
        (field,) = msgspec.structs.fields(SetupPick)
        return self.attachment.bound(identity).bind(lambda attached: realized(IconView(target=Member(root=bpy.types.WindowManager, path=f"{attached.name}.{field.name}"), show_labels=True), identity))


# --- [OPERATIONS] -----------------------------------------------------------------------


def locked(call: Call[LockRequest]) -> BlenderResult[Done]:
    """Toggles the request's lock on the entry its button drew, in the entry scene's lock store."""

    def toggled(owner: object, store: Stored[EffectLocks], path: FieldPath) -> BlenderResult[Done]:
        match owner:
            case bpy.types.SceneCompositorEffect(id_data=bpy.types.Scene() as scene, name=name):
                return EffectLocks.of(store, scene).bind(lambda locks: store.write(scene, locks.locking(name, path)[1])).map(lambda _: Done())
            case _:
                return Error((BlenderAbsentFault(owner=repr(owner), key=OWNER),))

    path: BlenderResult[FieldPath] = catch(lambda: msgspec.json.decode(call.record.path, type=FieldPath), exception=msgspec.DecodeError)().map_error(
        lambda error: (DomainFault(subject=repr(call.record), value=call.record.path, accepted=str(error)),)
    )
    return paired(call.identity.name(Name.ATTACHMENT, OWNER).bind(lambda name: hosted(name, call.context)), EffectLocks.bound(call.identity), path).bind(lambda parts: toggled(*parts))


def drawn(path: FieldPath, identity: Identity) -> BlenderResult[RowDraw]:
    """Draws a locked or unlocked toggle of a field path's lock on the drawn entry."""

    def toggle(name: str, store: Stored[EffectLocks], draws: dict[bool, RowDraw], layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
        layout.context_pointer_set(name, holder)
        match holder:
            case bpy.types.SceneCompositorEffect(id_data=bpy.types.Scene() as scene, name=entry):
                identity.reports.reported(lambda: EffectLocks.of(store, scene).map(lambda locks: draws[locks.locking(entry, path)[0]](layout, holder, context)))().to_optional()
            case _:
                pass

    request = Own(command=LOCK, values=LockRequest(path=msgspec.json.encode(path).decode()))
    return paired(
        identity.name(Name.ATTACHMENT, OWNER),
        EffectLocks.bound(identity),
        accumulated(OperatorButton(target=request, text="", icon=Some("LOCKED" if held else "UNLOCKED"), emboss=False).drawn(identity) for held in (False, True)),
    ).map(lambda parts: partial(toggle, parts[0], parts[1], dict(zip((False, True), parts[2]))))


def locks(path: Option[FieldPath]) -> RowContent:
    """Returns the lock command and the toggle of a field path, the whole entry where none is given."""
    return LOCK, partial(drawn, path.default_value(()))


# --- [COMPOSITION] ----------------------------------------------------------------------

LOCK: Final = Operator(key="render_effect_lock", title="Lock Effect", description="Toggle an entry or field lock against setup changes", form=LockRequest, run=locked)
categories: Final = (
    Category(
        key="sampling",
        settings=(
            "render.engine",
            "cycles.device",
            *AddPresetSampling.preset_values,
            *AddPresetViewportSampling.preset_values,
            "cycles.denoising_use_gpu",
            "cycles.preview_denoising_use_gpu",
            "eevee",
        ),
    ),
    Category(key="light_paths", settings=(*AddPresetIntegrator.preset_values, "cycles.sample_clamp_direct", "cycles.sample_clamp_indirect")),
    Category(
        key="simplify",
        settings=(
            "render.use_simplify",
            "render.simplify_subdivision_render",
            "render.simplify_child_particles_render",
            "render.simplify_volumes",
            "cycles.dicing_rate",
            "cycles.preview_dicing_rate",
            "cycles.offscreen_dicing_scale",
            "cycles.max_subdivisions",
        ),
    ),
    Category(key="motion_blur", settings=("render.use_motion_blur", "render.motion_blur_shutter", "render.motion_blur_position", "render.motion_blur_shutter_curve")),
    Category(
        key="film",
        settings=(
            "render.film_transparent",
            "render.filter_size",
            "cycles.film_exposure",
            "cycles.film_transparent_glass",
            "cycles.film_transparent_roughness",
            "cycles.pixel_filter_type",
            "cycles.filter_width",
        ),
    ),
    Category(key="performance", settings=(*AddPresetPerformance.preset_values, "cycles.use_auto_tile", "render.compositor_device")),
    Category(key="color_management", settings=("display_settings", "view_settings", "sequencer_colorspace_settings")),
    Category(key="resolution", settings=tuple(f"render.{member}" for member in Framing.native)),
    Category(key="output_format", settings=("render.image_settings",)),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LOCK", "EffectLocks", "Setup", "SetupGallery", "Setups", "categories", "locks"]

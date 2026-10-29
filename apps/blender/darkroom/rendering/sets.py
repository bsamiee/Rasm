"""Darkroom render setups: render set categories, the setup capture of category settings and chain entries with their locks, and setup tiles over the render proxy."""

from collections.abc import Generator, Mapping
from contextlib import contextmanager
import functools
from functools import cached_property, partial
from types import GenericAlias
from typing import Final

import bpy
from cycles.presets import AddPresetIntegrator, AddPresetPerformance, AddPresetSampling, AddPresetViewportSampling
from expression import Nothing, Ok, Option, Some
import msgspec
from typing_extensions import TypeForm

from blender.bpy.commands.operators import Done
from blender.bpy.data.rna.mirrors import Changed
from blender.bpy.data.rna.paths import rooted, Struct
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.render.compositor.effects import Enabled, Entry
from blender.bpy.render.compositor.recipes import applied, projected, recorded, Valued
from blender.bpy.render.outputs.framing import Framing
from blender.bpy.render.sessions.members import Category, Members
from blender.bpy.render.sessions.setups import Categories, CategorySettings, Locking, Locks, SetupScope
from blender.bpy.settings.attachments import Attachment
from blender.bpy.ui.layout.rows import FUNCTIONS, gridded, Layout, Line, Member, Property, realized, RowContent, RowDraw
from blender.bpy.ui.layout.values import Verb
from blender.records.faults import Outcome, paired
from blender.records.fields import Case, Name

from ..chain.previews import Recipe, Tiles
from ..chain.stack import Chain
from ..chain.stages import Effect

# --- [MODELS] ---------------------------------------------------------------------------


class Setup[R: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Category settings of a scene and its view layers, and the chain entries it holds by name in stack order."""

    settings: CategorySettings
    effects: Mapping[Name, Valued[Entry[R]]]


class SetupPick(bpy.types.PropertyGroup):
    """Setup preset a tile pick applies, its tile rows bound at registration."""

    setup: bpy.props.EnumProperty(name="Setup", description="Render setup preset applied to unlocked members", items=())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


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

    def captured(self, members: Members, scene: bpy.types.Scene, /) -> Outcome[Setup[Effect]]:
        """Captures category settings and chain entries."""
        return paired(self.settings.captured(members, scene), recorded(self.chain.rows, members.identity, scene)).map(lambda pair: Setup(settings=pair[0], effects=pair[1]))

    def projected(self, members: Members, scene: bpy.types.Scene, recipe: Setup[Effect], current: Setup[Effect], /) -> Outcome[tuple[Setup[Effect], Setup[Effect]]]:
        """Projects recipe settings and entries over current ones, setting and entry locks kept."""
        return paired(self.settings.projected(members, scene, recipe.settings, current.settings), EFFECT_LOCKS.read(members.identity, scene)).map(
            lambda parts: (Setup(settings=parts[0][0], effects=recipe.effects), Setup(settings=parts[0][1], effects=kept(parts[1], current.effects, recipe.effects)))
        )

    def applied(self, members: Members, scene: bpy.types.Scene, sparse: Setup[Effect], /) -> Outcome[None]:
        """Writes sparse settings and entries, locked entries and fields kept."""
        return paired(
            self.settings.applied(members, scene, sparse.settings),
            EFFECT_LOCKS.read(members.identity, scene).bind(lambda locks: applied(self.chain.rows, members.identity, scene, sparse.effects, locked=locks.whole(""), fields=locks.held)),
        ).map(lambda _: None)

    def rows(self, _members: Members, scope: SetupScope[Setup[Effect]], /) -> tuple[RowContent, ...]:
        """Draws setup tiles above the member lists."""
        return (SetupGallery(setups=self, scope=scope),)

    def recipe(self, identity: Identity, scene: bpy.types.Scene, setup: Setup[Effect]) -> Outcome[Recipe]:
        """Projects setup entries over the scene's entries, locks kept, as the preview-enabled chain."""
        return paired(recorded(self.chain.rows, identity, scene), EFFECT_LOCKS.read(identity, scene)).map(
            lambda parts: Recipe(entries=tuple(held for held in kept(parts[1], parts[0], setup.effects).values() if Enabled.PREVIEW in held.enables))
        )


class SetupGallery(Line, frozen=True, kw_only=True):
    """Setup presets as tiles of the chain each projects over the render proxy, a pick applying one."""

    setups: Setups
    scope: SetupScope[Setup[Effect]]

    @property
    def attachment(self) -> Attachment[SetupPick]:
        """Picked preset holder, each pick applying its preset."""
        return Attachment(key="render_setup_pick", form=SetupPick, owner=bpy.types.WindowManager, changed=Some(self.picking))

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Acquires the tiles and the pick holder, binding the pick rows to the preset tiles of the extension."""
        preset = self.scope.listing.preset

        def items(_owner: Struct, context: bpy.types.Context | None) -> tuple[tuple[str, str, str, int, int], ...]:
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
            return _rows(tuple((stem, name, tip, tiled(scene, stem), index) for scene in scenes for stem, name, tip, _, index in preset.entries(identity)))

        SetupPick.__annotations__["setup"].keywords["items"] = items
        return Ok((self.setups.tiles, self.attachment))

    @contextmanager
    def picking(self, identity: Identity) -> Generator[Changed]:
        """Defers one apply per pick, its undo step named by the apply verb."""
        preset = self.scope.listing.preset

        def applied(stem: str) -> Outcome[Option[str]]:
            return (
                preset
                .found(identity, stem)
                .bind(preset.read)
                .bind(lambda recipe: self.scope.applied(bpy.context, recipe))
                .map(lambda outcome: Some(bpy.path.display_name(Verb.APPLY.name.lower())) if isinstance(outcome, Done) else Nothing)
            )

        with identity.reports.deferred(applied) as deferred:

            def picked(owner: Struct, _context: bpy.types.Context) -> None:
                pick: SetupPick = owner  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                deferred(pick.setup)

            yield picked

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Draws preset tiles at the host preview size."""
        return (
            self.attachment
            .bound(identity)
            .map(lambda attached: Property(target=Member(root=bpy.types.WindowManager, path=(attached.name, "setup")), function=FUNCTIONS["template_icon_view"], keywords={"show_labels": True}))
            .bind(lambda tiles: realized((Layout(rows=(tiles,), container=Some(partial(gridded, "Setup"))),), identity))
            .map(self.conditioned)
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


@functools.cache
def _rows(rows: tuple[tuple[str, str, str, int, int], ...]) -> tuple[tuple[str, str, str, int, int], ...]:
    """Holds setup tile enum rows so the strings Blender reads stay referenced."""
    return rows


def _effect_holder(holder: Struct) -> Option[tuple[bpy.types.Scene, str]]:
    """Returns the scene and name of a drawn chain entry."""
    match holder:
        case bpy.types.SceneCompositorEffect(id_data=bpy.types.Scene() as scene, name=name):
            return Some((scene, name))
        case _:
            return Nothing


def kept(locks: Locks[Key], current: Mapping[Name, Valued[Entry[Effect]]], recipe: Mapping[Name, Valued[Entry[Effect]]]) -> dict[Name, Valued[Entry[Effect]]]:
    """Projects recipe entries over current ones, entries locked whole by the empty key, locked inputs, and locked enables kept."""
    return projected(current, recipe, locked=locks.whole(""), fields=locks.held)


def locks(key: Option[Key]) -> RowContent:
    """Returns the lock command and the toggle of an input key, the whole entry where none is given."""
    return Layout(rows=lambda identity: EFFECT_LOCKS.drawn(key.default_value(""), identity).map(lambda draw: (draw,)), prerequisites=(EFFECT_LOCKS.operator,))


# --- [COMPOSITION] ----------------------------------------------------------------------

EFFECT_LOCKS: Final = Locking[Key](key="render_effect_locks", form=Locks[Key], holder=_effect_holder, text=str, parsed=Ok)
categories: Final = (
    Category(
        key="sampling",
        settings=(
            ("render", "engine"),
            ("cycles", "device"),
            *AddPresetSampling.preset_values,
            *AddPresetViewportSampling.preset_values,
            ("cycles", "denoising_use_gpu"),
            ("cycles", "preview_denoising_use_gpu"),
            ("eevee",),
        ),
    ),
    Category(key="light_paths", settings=(*AddPresetIntegrator.preset_values, ("cycles", "sample_clamp_direct"), ("cycles", "sample_clamp_indirect"))),
    Category(
        key="simplify",
        settings=(
            ("render", "use_simplify"),
            ("render", "simplify_subdivision_render"),
            ("render", "simplify_child_particles_render"),
            ("render", "simplify_volumes"),
            ("cycles", "dicing_rate"),
            ("cycles", "preview_dicing_rate"),
            ("cycles", "offscreen_dicing_scale"),
            ("cycles", "max_subdivisions"),
        ),
    ),
    Category(key="motion_blur", settings=(("render", "use_motion_blur"), ("render", "motion_blur_shutter"), ("render", "motion_blur_position"), ("render", "motion_blur_shutter_curve"))),
    Category(
        key="film",
        settings=(
            ("render", "film_transparent"),
            ("render", "filter_size"),
            ("cycles", "film_exposure"),
            ("cycles", "film_transparent_glass"),
            ("cycles", "film_transparent_roughness"),
            ("cycles", "pixel_filter_type"),
            ("cycles", "filter_width"),
        ),
    ),
    Category(key="performance", settings=(*AddPresetPerformance.preset_values, ("cycles", "use_auto_tile"), ("render", "compositor_device"))),
    Category(key="color_management", settings=(("display_settings",), ("view_settings",), ("sequencer_colorspace_settings",))),
    Category(key="resolution", settings=tuple(("render", member) for member in Framing.native)),
    Category(key="output_format", settings=(("render", "image_settings"),)),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EFFECT_LOCKS", "Setup", "SetupGallery", "Setups", "categories", "kept", "locks"]

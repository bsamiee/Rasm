"""Darkroom looks: flim's default, nostalgia, and silver film stocks installed as view transforms, the OCIO looks the formation's view offers tiled over the render proxy, and the layer stack looks, gathered in the Image Formation panel."""

from collections.abc import Generator, Mapping
from contextlib import contextmanager
from functools import partial
from typing import Annotated, Final

from bl_ui.properties_render import RENDER_PT_color_management
import bpy
from expression import Option, Some
import msgspec
import PyOpenColorIO

from rasm_bpy.data import Callbacks, choices, Handle, IconedItem, id_data, Identity, Struct, written
from rasm_bpy.nodes import item_key
from rasm_bpy.render import ConfigInstall, ConfigView, Entry, NO_LOOK, Valued, viewed
from rasm_bpy.settings import Attachment
from rasm_bpy.ui import aligned, Child, Gallery, Layout, Member, OperatorButton, Panel, Section
from rasm_imaging import DisplayView, Film
from rasm_records import Nothing, Outcome

from ..catalog import stacks, tiles
from ..effects.formation.display import displays, Formation, reformed
from ..effects.formation.tone import download_tony_tables
from .luts import install_tone_view

# --- [MODELS] ---------------------------------------------------------------------------


class FormationLook(bpy.types.PropertyGroup):
    """Look the formation's view applies, one of the OCIO looks Blender offers that view, read from the formation and written into it on each pick."""

    look: Annotated[str, bpy.props.EnumProperty(name="Look", description="Look the formation's view applies ahead of its transform", items=())]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _looks(config: PyOpenColorIO.Config) -> tuple[str, ...]:
    """Returns the disabled look and every look the configuration declares, the pick's values their positions."""
    return (NO_LOOK, *config.getLookNames())


def _viewing(identity: Identity, scene: bpy.types.Scene) -> bpy.types.Scene | bpy.types.CompositorNodeConvertToDisplay:
    """Returns the earliest formation entry's display node, else the scene, the holder of the view a look applies to."""
    return next((node for _, node in displays(identity, scene)), scene)


def _offered(identity: Identity, owner: Struct, _context: bpy.types.Context | None) -> tuple[IconedItem, ...]:
    """Item rows of the looks Blender offers the holder's view, labeled by the host, each under its tile through the formation."""
    config = PyOpenColorIO.GetCurrentConfig()

    def tile(scene: bpy.types.Scene, view: DisplayView, name: str) -> int:
        record = Formation(display_view=msgspec.structs.replace(view, looks_override=name, looks_override_enabled=name != NO_LOOK))
        return tiles.icon(identity, scene, item_key(_LOOKS.key, name), Valued(structure=Entry(record=record))).default_value(0)

    return choices(
        (name, label, Option.of_optional(config.getLook(name)).map(PyOpenColorIO.Look.getDescription).default_value(""), tile(scene, view, name), index)
        for scene in id_data(bpy.types.Scene, owner).to_list()
        for holder in (_viewing(identity, scene),)
        for view in (viewed(holder),)
        for index, name in enumerate(_looks(config))
        for label in (bpy.types.UILayout.enum_item_name(holder.view_settings, "look", name),)
        if label
    )


def _current(identity: Identity, owner: Struct) -> int:
    """Returns the position of the look the holder's view applies."""
    looks = _looks(PyOpenColorIO.GetCurrentConfig())
    look = id_data(bpy.types.Scene, owner).map(lambda scene: viewed(_viewing(identity, scene)).looks_override).default_value(NO_LOOK)
    return looks.index(look)


@contextmanager
def _looking(identity: Identity) -> Generator[Mapping[str, Callbacks[FormationLook]]]:
    """Binds the pick's items and live read, and defers each picked look's write into the formation."""

    def write(handle: Handle[bpy.types.Scene], look: str) -> Outcome[Option[str]]:
        return (
            handle
            .resolve()
            .bind(
                lambda scene: reformed(
                    identity,
                    scene,
                    lambda view: msgspec.structs.replace(view, looks_override=look, looks_override_enabled=look != NO_LOOK),
                    lambda: written(scene, ("view_settings", "look"), look).map(Option.is_some),
                )
            )
            .map(lambda wrote: Some(f"Look {look}") if wrote else Nothing)
        )

    with identity.reports.deferred(write) as deferred:

        def pick(owner: Struct, value: int) -> None:
            for scene in id_data(bpy.types.Scene, owner).to_list():
                deferred(Handle.of(scene), _looks(PyOpenColorIO.GetCurrentConfig())[value])

        yield {"look": Callbacks[FormationLook](items=partial(_offered, identity), get=partial(_current, identity), set=pick)}


# --- [COMPOSITION] ----------------------------------------------------------------------

_film: Final = Film()
_stocks: Final = (
    ("Film", "Negative and print film at flim's default stocks", _film),
    (
        "Film Nostalgia",
        "Warm faded film at flim's nostalgia stocks",
        msgspec.structs.replace(
            _film,
            pre_exposure=5.563035,
            gains=(1.1, _film.gains[1], 1.2),
            span=23.0 - _film.low,
            negative=msgspec.structs.replace(_film.negative, exposure=5.8),
            backlight=(0.99, 1.1, 1.035989),
            positive=msgspec.structs.replace(_film.positive, density=40.0),
            black_point=-0.005,
            saturation=1.1,
        ),
    ),
    (
        "Film Silver",
        "Cool dense film at flim's silver stocks",
        msgspec.structs.replace(
            _film,
            pre_exposure=3.9,
            pre_filter=(0.95, 0.975, 1.0),
            gains=(_film.gains[0], _film.gains[1], 1.06),
            negative=msgspec.structs.replace(_film.negative, exposure=4.7, density=7.0),
            backlight=(0.9992, 0.99, 1.0),
            positive=msgspec.structs.replace(_film.positive, exposure=4.7, density=30.0),
            black_point=0.0005,
            post_filter=(1.0, 1.0, 0.96),
            saturation=1.0,
        ),
    ),
)
_install: Final = ConfigInstall(views=tuple(ConfigView(name=name, description=description, transform=stock.transform, space=Film.space) for name, description, stock in _stocks))
_LOOKS: Final = Attachment(key="looks", form=FormationLook, owner=bpy.types.Scene, callbacks=Some(_looking))
rows: Final = (
    Panel(
        section=Section(
            key="image_formation",
            rows=(
                Gallery(target=Member(root=_LOOKS, path=("look",))),
                stacks.look,
                Layout(rows=(OperatorButton(target=_install.installing), OperatorButton(target=install_tone_view), OperatorButton(target=_install.removing)), container=Some(aligned)),
                Layout(rows=(OperatorButton(target=download_tony_tables),)),
            ),
        ),
        at=Child(parent=RENDER_PT_color_management),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["rows"]

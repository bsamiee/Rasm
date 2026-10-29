"""Darkroom film views: flim's default, nostalgia, and silver stocks installed as view transforms over Blender's bundled configuration."""

from typing import Final

import msgspec
import PyOpenColorIO

from blender.bpy.render.color.spaces import ConfigInstall, ConfigView
from blender.imaging.color.authoring import ConfigBuild
from blender.imaging.color.film import Film
from blender.records.faults import Outcome

# --- [OPERATIONS] -----------------------------------------------------------------------


def view(name: str, description: str, film: Film) -> ConfigView:
    """Returns the installed view developing `film` in its process space."""

    def developed(_base: PyOpenColorIO.Config) -> Outcome[PyOpenColorIO.Transform]:
        return film.transform()

    return ConfigView(name=name, description=description, transform=developed, space=Film.space)


# --- [COMPOSITION] ----------------------------------------------------------------------

film: Final = Film()
red, green, blue = film.gains
install: Final = ConfigInstall(
    build=ConfigBuild(view_transforms=()),
    views=(
        view("Film", "Negative and print film at flim's default stocks", film),
        view(
            "Film Nostalgia",
            "Warm faded film at flim's nostalgia stocks",
            msgspec.structs.replace(
                film,
                pre_exposure=5.563035,
                gains=(1.1, green, 1.2),
                span=23.0 - film.low,
                negative=msgspec.structs.replace(film.negative, exposure=5.8),
                backlight=(0.99, 1.1, 1.035989),
                positive=msgspec.structs.replace(film.positive, density=40.0),
                black_point=-0.005,
                saturation=1.1,
            ),
        ),
        view(
            "Film Silver",
            "Cool dense film at flim's silver stocks",
            msgspec.structs.replace(
                film,
                pre_exposure=3.9,
                pre_filter=(0.95, 0.975, 1.0),
                gains=(red, green, 1.06),
                negative=msgspec.structs.replace(film.negative, exposure=4.7, density=7.0),
                backlight=(0.9992, 0.99, 1.0),
                positive=msgspec.structs.replace(film.positive, exposure=4.7, density=30.0),
                black_point=0.0005,
                post_filter=(1.0, 1.0, 0.96),
                saturation=1.0,
            ),
        ),
    ),
)
rows: Final = (install,)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["install", "rows"]

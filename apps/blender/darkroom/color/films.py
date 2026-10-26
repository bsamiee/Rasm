"""Darkroom film formations: flim's default, nostalgia, and silver stocks installed as views over Blender's bundled configuration, with the working colors row."""

from typing import Final

import msgspec
import PyOpenColorIO

from blender.bpy.render.color.spaces import ConfigInstall, ConfigView, WorkingColors
from blender.imaging.color.authoring import ConfigBuild
from blender.imaging.color.film import Film
from blender.imaging.faults import ImagingResult
from blender.records.fields import Gated, Rgb

# --- [OPERATIONS] -----------------------------------------------------------------------


def view(name: str, description: str, film: Film) -> ConfigView:
    """Returns the installed view developing `film` in its process space."""

    def developed(_base: PyOpenColorIO.Config) -> ImagingResult[PyOpenColorIO.Transform]:
        return film.transform()

    return ConfigView(name=name, description=description, transform=developed, space=Film.space)


# --- [COMPOSITION] ----------------------------------------------------------------------

film: Final = Film()
red, green, blue = film.sensitivities
install: Final = ConfigInstall(
    build=ConfigBuild(formations=()),
    views=(
        view("Film", "Negative and print film at flim's default stocks", film),
        view(
            "Film Nostalgia",
            "Warm faded film at flim's nostalgia stocks",
            msgspec.structs.replace(
                film,
                pre_exposure=5.563035,
                sensitivities=(msgspec.structs.replace(red, gain=1.1), green, msgspec.structs.replace(blue, gain=1.2)),
                characteristic=msgspec.structs.replace(film.characteristic, span=23.0 - film.characteristic.low),
                negative=msgspec.structs.replace(film.negative, exposure=5.8),
                backlight=Rgb(red=0.99, green=1.1, blue=1.035989),
                positive=msgspec.structs.replace(film.positive, density=40.0),
                black_point=Gated(enabled=True, value=-0.005),
                saturation=1.1,
            ),
        ),
        view(
            "Film Silver",
            "Cool dense film at flim's silver stocks",
            msgspec.structs.replace(
                film,
                pre_exposure=3.9,
                pre_filter=Rgb(red=0.95, green=0.975, blue=1.0),
                sensitivities=(red, green, msgspec.structs.replace(blue, gain=1.06)),
                negative=msgspec.structs.replace(film.negative, exposure=4.7, density=7.0),
                backlight=Rgb(red=0.9992, green=0.99, blue=1.0),
                positive=msgspec.structs.replace(film.positive, exposure=4.7, density=30.0),
                black_point=Gated(enabled=True, value=0.0005),
                post_filter=Rgb(red=1.0, green=1.0, blue=0.96),
                saturation=1.0,
            ),
        ),
    ),
)
rows: Final = (WorkingColors(), install)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["install", "rows"]

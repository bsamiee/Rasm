"""Native nonfinite wind refusal at the physical particle boundary."""

from functools import partial

from expression import Result
import msgspec
import numpy as np
import OpenImageIO
import PyOpenColorIO
import pytest

from apps.blender.darkroom.effects.optics.bases import evaluated, Perlin
from apps.blender.darkroom.effects.optics.emission import spectrum
from apps.blender.darkroom.effects.optics.particles import Particles
from blender.bpy.nodes import textures
from blender.imaging.geometry.frames import Region
from blender.imaging.sampling.integration import Population, Precision
from blender.records.faults import Reason
from tests.python.libs.blender.bpy.background.session import Acquired

# --- [TESTS] ----------------------------------------------------------------------------


def test_clamped_native_nan_is_refused_by_physical_particles(acquired: Acquired) -> None:
    """Raw NaN survives native sampling and becomes a typed refusal before physical conversion."""
    points = np.asarray(((0.25, 0.5), (0.75, 0.125)), dtype=np.float64)
    negative = Perlin(texture=textures.Noise(noise_type="HETERO_TERRAIN", offset=-1000.0, roughness=1.0, detail=15.0))
    positive = msgspec.structs.replace(negative, texture=msgspec.structs.replace(negative.texture, offset=1000.0))
    record = Particles(wind=negative, turbulence=0.1, motion_blur=False, layers=1, population=Population(spacing=0.5, density=1.0, size=0.5))
    with acquired(()) as identity:
        sample = partial(evaluated, identity, points=points, time=np.zeros(len(points)), seed=0, streams=(0,), outputs=lambda density, _tone: (density,))
        raw = sample(negative).default_with(lambda faults: pytest.fail(str(faults)))
        np.testing.assert_array_equal(raw[0], np.full(len(points), np.nan, dtype=np.float32))
        apply = partial(
            Particles.applied,
            identity=identity,
            plane=OpenImageIO.ImageBuf(np.full((4, 4, 4), 0.5, dtype=np.float32)),
            bounds=Region(),
            frame=0.0,
            rate=24.0,
            shutter=(0.0, 0.0),
            precision=Precision(power=0, replicates=2),
            linear_rgb=partial(spectrum, identity, config=PyOpenColorIO.GetCurrentConfig()),
        )
        with np.errstate(all="raise"):
            rejected = apply(record)
        match rejected:
            case Result(tag="error", error=faults):
                for fault in faults:
                    assert fault.reason == Reason.OUTSIDE
                    assert isinstance(fault.cause, np.ndarray)
                    assert np.isnan(fault.cause).all()
            case _:
                pytest.fail(repr(rejected))
        np.testing.assert_array_equal(sample(positive).default_with(lambda faults: pytest.fail(str(faults)))[0], np.ones(len(points), dtype=np.float32))
        with np.errstate(all="raise"):
            accepted = apply(msgspec.structs.replace(record, wind=positive))
        match accepted:
            case Result(tag="ok", ok=estimate):
                assert np.isfinite(estimate.replicates).all()
            case _:
                pytest.fail(repr(accepted))

"""Darkroom preferences, page sections, and typed settings access."""

from collections.abc import Callable
from typing import Annotated

import msgspec

from rasm.blender.extension.identity import Identity
from rasm.blender.faults import BlenderResult
from rasm.blender.render.previews.nodes import NodePreviews
from rasm.blender.render.previews.queue import Budget
from rasm.blender.render.review.calibration import Charting
from rasm.blender.render.review.history import Reviewing
from rasm.blender.render.sessions.queue import Queueing
from rasm.blender.ui.layout.browser import BrowserLayout
from rasm.blender.ui.layout.sections import Single
from rasm.blender.ui.layout.status import Status
from rasm.blender.ui.surfaces.preferences import stored, value
from rasm.imaging.effects.generators.noise import Precision
from rasm.rendering.history import Retention

# --- [MODELS] ---------------------------------------------------------------------------


class Settings(msgspec.Struct, frozen=True, kw_only=True):
    """Preferences one user keeps across Blender files."""

    queue: Annotated[Queueing, msgspec.Meta(title="Render Queue", description="Target, notices, power hold, heartbeat, and Flamenco scripts folder of queued renders")] = Queueing()
    history: Annotated[Retention, msgspec.Meta(title="Section History", description="Value sets each Darkroom section keeps for restore")] = Retention()
    review: Annotated[Reviewing, msgspec.Meta(title="Stored Renders", description="Render history the image editor keeps and its comparison bars")] = Reviewing()
    previews: Annotated[NodePreviews, msgspec.Meta(title="Node Previews", description="Node editor thumbnails, their trees, preview light, and drawing")] = NodePreviews()
    budget: Annotated[Budget, msgspec.Meta(title="Preview Budget", description="Previews one step renders and its time budget")] = Budget()
    precision: Annotated[
        Precision, msgspec.Meta(title="Numerical Quality", description="Integration samples, error replicates, seed, and particle profile tolerances. Increasing sampling costs time and memory")
    ] = Precision(power=6, replicates=4, seed=0, absolute=1e-4, relative=1e-3)
    charting: Annotated[Charting, msgspec.Meta(title="Chart Calibration", description="Chart, normalization, clamp, and overlay opacity a new calibration takes")] = Charting()
    presets: Annotated[BrowserLayout, msgspec.Meta(title="Preset Browser", description="Layout and thumbnail size of the render setup browser")] = BrowserLayout()


# --- [COMPOSITION] ----------------------------------------------------------------------

category = "Darkroom"
sections = (Single(key="status", title="Status", rows=(Status(),)),)
queueing = value(Settings, lambda settings: settings.queue)
retention = value(Settings, lambda settings: settings.history)
reviewing = value(Settings, lambda settings: settings.review)
previewing = value(Settings, lambda settings: settings.previews)
previewed: Callable[[Identity, NodePreviews], BlenderResult[None]] = stored(Settings, lambda settings, previews: msgspec.structs.replace(settings, previews=previews))
budgeting = value(Settings, lambda settings: settings.budget)
precision = value(Settings, lambda settings: settings.precision)
charting = value(Settings, lambda settings: settings.charting)
preset_layout = value(Settings, lambda settings: settings.presets)
preset_stored: Callable[[Identity, BrowserLayout], BlenderResult[None]] = stored(Settings, lambda settings, layout: msgspec.structs.replace(settings, presets=layout))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Settings", "budgeting", "category", "charting", "precision", "preset_layout", "preset_stored", "previewed", "previewing", "queueing", "retention", "reviewing", "sections"]

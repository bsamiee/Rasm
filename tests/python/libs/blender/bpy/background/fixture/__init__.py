"""Fixture extension registering the rows of every observed area through one registration scope."""

from blender.bpy.extension.registration import Registration

from . import commands, drawing, extension, images, layout, resources, screens, sections, surfaces

# --- [COMPOSITION] ----------------------------------------------------------------------

AREAS = (commands, screens, resources, drawing, layout, sections, surfaces, images, extension)

REGISTRATION = Registration(__name__, tuple(row for area in AREAS for row in area.rows))
register, unregister = REGISTRATION.register, REGISTRATION.unregister

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["AREAS", "register", "unregister"]

"""Fixture extension registering the rows of every observed area through one registration scope."""

from blender.bpy.extension.registration import Registration

from . import commands, drawing, elements, extension, images, layout, modules, queue, resources, screens

# --- [COMPOSITION] ----------------------------------------------------------------------

AREAS = (commands, screens, resources, drawing, layout, elements, images, queue, modules, extension)

REGISTRATION = Registration(__name__, tuple(row for area in AREAS for row in area.rows))
register, unregister = REGISTRATION.register, REGISTRATION.unregister

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["AREAS", "register", "unregister"]

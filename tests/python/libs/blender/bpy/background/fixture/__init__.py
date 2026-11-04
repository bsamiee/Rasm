"""Native extension entry points for declaration scopes supplied by tests."""

from blender.bpy.extension.registration import Registration

# --- [COMPOSITION] ----------------------------------------------------------------------

_REGISTRATION = Registration(__name__, ())
register, unregister = _REGISTRATION.register, _REGISTRATION.unregister

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["register", "unregister"]

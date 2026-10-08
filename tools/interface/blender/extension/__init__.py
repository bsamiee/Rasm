"""Interface extension with viewport navigation, collapsed add-on panels, icon sidebar tabs, packed toolbars of the workspace's owners, asset shelves, command aliases, and the unit switch."""

from .registration import register, unregister

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["register", "unregister"]

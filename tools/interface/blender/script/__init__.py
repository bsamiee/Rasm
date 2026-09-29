"""Blender's scripted session entries the host bootstraps: `start`, the interface run, and `installation`, the facts the host resolves packages against."""

from interface.blender.script.addons import installation
from interface.blender.script.steps import start

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["installation", "start"]

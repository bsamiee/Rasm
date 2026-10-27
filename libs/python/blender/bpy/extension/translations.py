"""English text translations registered under extension package."""

from collections.abc import Generator, Mapping
from contextlib import contextmanager, ExitStack

import bpy
from expression import Ok
import msgspec

from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisition, Band
from blender.records.faults import Outcome

# --- [SERVICES] -------------------------------------------------------------------------


class Translations(msgspec.Struct, frozen=True, kw_only=True):
    """English text translations registered under extension package."""

    table: Mapping[str, Mapping[str, str]]

    def acquisitions(self, identity: Identity) -> Outcome[tuple[tuple[Band, Acquisition], ...]]:
        """Builds table registration acquisition.

        Args:
            identity: Extension with package keying translation table.

        Returns:
            Ok of one registry acquisition.
        """
        return Ok(((Band.REGISTRIES, self.registered(identity)),))

    @contextmanager
    def registered(self, identity: Identity) -> Generator[None]:
        """Registers translations under default and operator contexts.

        Args:
            identity: Extension with package keying translation table.

        Yields:
            None while Blender holds table.
        """
        contexts = bpy.app.translations.contexts
        pair: tuple[str, str] = (contexts.default, contexts.operator_default)  # type: ignore[attr-defined]
        with ExitStack() as stack:
            bpy.app.translations.register(identity.package, {locale: {(context, english): text for english, text in entries.items() for context in pair} for locale, entries in self.table.items()})
            stack.callback(bpy.app.translations.unregister, identity.package)
            yield


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Translations"]

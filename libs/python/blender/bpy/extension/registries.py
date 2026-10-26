"""Translations, CLI commands, and Online Manual links."""

import base64
from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager

import bpy
from expression import Ok, Option, Result
import msgspec

from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisition, Band
from blender.bpy.faults import BlenderReason
from blender.records.faults import Fault, Faults, Outcome, RecordReason

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
        bpy.app.translations.register(
            identity.package,
            {
                locale: {
                    (context, english): text
                    for english, text in entries.items()
                    for context in (contexts.default, contexts.operator_default)  # type: ignore[attr-defined]
                }
                for locale, entries in self.table.items()
            },
        )
        try:
            yield
        finally:
            bpy.app.translations.unregister(identity.package)


class CliCommand[A](msgspec.Struct, frozen=True, kw_only=True):
    """Command-line command with base64 JSON first argument."""

    key: Key
    decoder: msgspec.json.Decoder[A]
    run: Callable[[A, tuple[str, ...]], Outcome[object]]

    def acquisitions(self, identity: Identity) -> Outcome[tuple[tuple[Band, Acquisition], ...]]:
        """Builds named command acquisition.

        Args:
            identity: Extension with manifest ID prefixing command name.

        Returns:
            Ok of one registry acquisition, register fault for refused name.
        """
        return identity.name(Name.CLI, self.key).map(lambda command: ((Band.REGISTRIES, self.registered(identity, command)),))

    @contextmanager
    def registered(self, identity: Identity, command: str) -> Generator[Outcome[None] | None]:
        """Registers command, unregistering its handle at exit.

        Args:
            identity: Extension with reports holding decode and run faults.
            command: Name `blender --command` takes.

        Yields:
            None while Blender holds command, register fault for refused name.
        """

        @identity.reports.reported
        def executed(arguments: list[str]) -> Outcome[object]:
            """Decodes first argument, passing rest unchanged."""
            try:
                document, *trailing = arguments
                decoded = self.decoder.decode(base64.b64decode(document, validate=True))
            except (ValueError, msgspec.DecodeError) as error:
                return Result[object, Faults](error=(Fault(reason=RecordReason.OUTSIDE, subject="Command arguments", member=str(error), cause=" ".join(arguments)),))
            else:
                return self.run(decoded, tuple(trailing))

        try:
            handle = bpy.utils.register_cli_command(command, lambda arguments: executed(arguments).map(lambda _: 0).default_value(1))
        except ValueError as error:
            yield Result[None, Faults](error=(Fault(reason=BlenderReason.UNREGISTERED, subject=command, cause=error),))
        else:
            try:
                yield None
            finally:
                bpy.utils.unregister_cli_command(handle)


class ManualMap(msgspec.Struct, frozen=True, kw_only=True):
    """Online Manual links from manifest website and extension operators."""

    def acquisitions(self, identity: Identity) -> Outcome[tuple[tuple[Band, Acquisition], ...]]:
        """Builds manual hook acquisition over manifest website.

        Args:
            identity: Extension with website and generated names hook reads.

        Returns:
            Ok of one registry acquisition, or absent website fault.
        """
        return (
            Option
            .of_optional(identity.manifest.website)
            .map(lambda website: ((Band.REGISTRIES, self.registered(identity, website)),))
            .to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=identity.package, member="website"),))
        )

    @staticmethod
    @contextmanager
    def registered(identity: Identity, website: str) -> Generator[None]:
        """Holds manual hook over extension operators, types, and properties.

        Args:
            identity: Extension with manifest ID prefixing every generated name.
            website: Documentation URL prefix hook returns.

        Yields:
            None while Blender holds hook.
        """
        category = identity.manifest.id.lower()

        def mapping() -> tuple[str, tuple[tuple[str, str], ...]]:
            """Returns website with extension operator, type, and property patterns."""
            return website, (*((f"bpy.ops.{category}.{key}", f"#{key}") for key in dir(getattr(bpy.ops, category))), (f"bpy.types.{category}_*", ""), (f"bpy.types.*.{category}_*", ""))

        bpy.utils.register_manual_map(mapping)
        try:
            yield
        finally:
            bpy.utils.unregister_manual_map(mapping)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CliCommand", "ManualMap", "Translations"]

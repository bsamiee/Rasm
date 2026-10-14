"""Native translation tables, command-line commands, and Online Manual links."""

import base64
from collections.abc import Callable, Generator
from contextlib import contextmanager

import bpy
from expression import Ok, Option, Result
import msgspec

from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisition, Band
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderReadFault, BlenderRegisterFault, BlenderResult

# --- [SERVICES] -------------------------------------------------------------------------


class Translations(msgspec.Struct, frozen=True, kw_only=True):
    """English text translated under an extension's native registration key."""

    table: dict[str, dict[str, str]]

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds the table's registration acquisition.

        Args:
            identity: Extension whose package keys the translation table.

        Returns:
            Ok of one registry acquisition.
        """
        return Ok(((Band.REGISTRIES, self.registered(identity)),))

    @contextmanager
    def registered(self, identity: Identity) -> Generator[None]:
        """Registers translated text under the native interface and operator contexts.

        Args:
            identity: Extension whose package keys the translation table.

        Yields:
            None while the host holds the table.
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
    """Base64 JSON command with a typed decoder and trailing native arguments."""

    key: Key
    decoder: msgspec.json.Decoder[A]
    run: Callable[[A, tuple[str, ...]], BlenderResult[object]]

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Names the command and builds its registration acquisition.

        Args:
            identity: Extension whose manifest ID prefixes the native command name.

        Returns:
            Ok of one registry acquisition, or the refused generated name.
        """
        return identity.name(Name.CLI, self.key).map(lambda command: ((Band.REGISTRIES, self.registered(identity, command)),))

    @contextmanager
    def registered(self, identity: Identity, command: str) -> Generator[BlenderResult[None] | None]:
        """Registers a JSON command and releases its opaque native handle.

        Args:
            identity: Extension whose reports hold decode and execution failures.
            command: Name Blender exposes through its command-line command option.

        Yields:
            None while the host holds the command, or a register fault when Blender refuses its identifier.
        """

        @identity.reports.reported
        def executed(arguments: list[str]) -> BlenderResult[object]:
            """Decodes the first argument and passes remaining arguments unchanged."""
            try:
                document, *trailing = arguments
                decoded = self.decoder.decode(base64.b64decode(document, validate=True))
            except (ValueError, msgspec.DecodeError) as error:
                return Result[object, BlenderFaults](error=(BlenderReadFault(request=" ".join(arguments), error=error),))
            else:
                return self.run(decoded, tuple(trailing))

        try:
            handle = bpy.utils.register_cli_command(command, lambda arguments: executed(arguments).map(lambda _: 0).default_value(1))
        except ValueError as error:
            yield Result[None, BlenderFaults](error=(BlenderRegisterFault(request=(command,), error=error),))
        else:
            try:
                yield None
            finally:
                bpy.utils.unregister_cli_command(handle)


class ManualMap(msgspec.Struct, frozen=True, kw_only=True):
    """Online Manual links derived from the extension's manifest and native operators."""

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Admits the manifest website and builds the manual hook acquisition.

        Args:
            identity: Extension whose website and generated names the hook consumes.

        Returns:
            Ok of one registry acquisition, or an absent website fault.
        """
        return (
            Option
            .of_optional(identity.manifest.website)
            .map(lambda website: ((Band.REGISTRIES, self.registered(identity, website)),))
            .to_result_with(lambda: (BlenderAbsentFault(owner=identity.package, key="website"),))
        )

    @staticmethod
    @contextmanager
    def registered(identity: Identity, website: str) -> Generator[None]:
        """Holds one manual hook over the extension's operators, groups, and attachments.

        Args:
            identity: Extension whose manifest ID prefixes every native name.
            website: Manifest documentation prefix returned by the hook.

        Yields:
            None while the host holds the hook.
        """
        category = identity.manifest.id.lower()

        def mapping() -> tuple[str, tuple[tuple[str, str], ...]]:
            """Reads native operators when Blender requests a manual link."""
            return website, (*((f"bpy.ops.{category}.{key}", f"#{key}") for key in dir(getattr(bpy.ops, category))), (f"bpy.types.{category}_*", ""), (f"bpy.types.*.{category}_*", ""))

        bpy.utils.register_manual_map(mapping)
        try:
            yield
        finally:
            bpy.utils.unregister_manual_map(mapping)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CliCommand", "ManualMap", "Translations"]

"""Command-line commands `blender --command` runs over a base64 JSON first argument."""

import base64
from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
from functools import partial

import bpy
from expression import Error, Result
from expression.extra.result import catch
import msgspec

from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisition, Band
from blender.bpy.faults import BlenderReason
from blender.records.faults import Fault, Outcome, RecordReason

# --- [SERVICES] -------------------------------------------------------------------------


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
            Ok of one registry acquisition, register fault for refused key.
        """
        return identity.identifier(self.key).map(lambda command: ((Band.REGISTRIES, self._registered(identity, command)),))

    def executed(self, command: str, arguments: list[str]) -> Outcome[object]:
        """Decodes the first argument, passing the rest unchanged.

        Args:
            command: Name `blender --command` takes.
            arguments: Arguments after the command name.

        Returns:
            Run result, read fault for an undecodable document, absent fault for no argument.
        """
        match arguments:
            case [document, *trailing]:
                decoded = Result[bytes, ValueError | msgspec.DecodeError].bind(
                    catch(base64.b64decode, exception=ValueError)(document, validate=True), catch(self.decoder.decode, exception=msgspec.DecodeError)
                )
                return decoded.map_error(partial(Fault.of, RecordReason.UNREADABLE, command)).bind(lambda value: self.run(value, tuple(trailing)))
            case _:
                return Error(Fault.of(BlenderReason.ABSENT, command, member="arguments"))

    @contextmanager
    def _registered(self, identity: Identity, command: str) -> Generator[Outcome[None]]:
        """Holds command while the scope stands, its decode and run faults held in reports, register fault for a refused name."""
        executed = identity.reports.reported(partial(self.executed, command))
        with ExitStack() as stack:
            yield (
                catch(bpy.utils.register_cli_command, exception=ValueError)(command, lambda arguments: executed(arguments).map(lambda _: 0).default_value(1))
                .map(lambda handle: stack.callback(bpy.utils.unregister_cli_command, handle))
                .map(lambda _: None)
                .map_error(partial(Fault.of, BlenderReason.UNREGISTERED, command))
            )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CliCommand"]

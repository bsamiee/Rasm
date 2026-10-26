"""Failure every fitout command returns, with message and exit status."""

lazy from collections.abc import Sequence
lazy import subprocess

import msgspec

# --- [ERRORS] ---------------------------------------------------------------------------


class Fault(msgspec.Struct, frozen=True):
    """Failure of a fitout command, rendered as its message and exit status."""

    message: str
    status: int = 1

    @classmethod
    def joined(cls, faults: Sequence[Fault]) -> Fault:
        """Fault stating every fault of independent failed steps in order.

        Args:
            faults: Faults of failed steps, at least one.

        Returns:
            Lone fault with its exit status, else fault joining every message.
        """
        match faults:
            case [fault]:
                return fault
            case _:
                return cls(message="\n".join(fault.message for fault in faults))

    @classmethod
    def of(cls, error: Exception) -> Fault:
        """Fault stating a caught error.

        Args:
            error: Error a call raised, a group rendered from its leaves.

        Returns:
            Fault with the error's own text, and a failed child process's exit status.
        """
        match error:
            case subprocess.CalledProcessError(returncode=status):
                return cls(message=str(error), status=status)
            case ExceptionGroup(exceptions=leaves):
                return cls.joined([cls.of(leaf) for leaf in leaves])
            case _:
                return cls(message=str(error))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Fault"]

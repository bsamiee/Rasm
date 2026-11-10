"""Command results on the standard streams and the exit status of each fault reason."""

from enum import StrEnum
import sys
from typing import Final

import sysexits
lazy from expression import Result
lazy import msgspec

from rasm_records import Reason
lazy from rasm_records import Outcome

# --- [CONSTANTS] ------------------------------------------------------------------------

EXITS: Final = frozendict[StrEnum, int]({
    **dict.fromkeys((Reason.OUTSIDE, Reason.INVALID, Reason.ABSENT, Reason.UNMET), sysexits.EX_DATAERR),
    Reason.UNREADABLE: sysexits.EX_NOINPUT,
    Reason.UNWRITABLE: sysexits.EX_CANTCREAT,
    Reason.FAILED: sysexits.EX_SOFTWARE,
})

# --- [OPERATIONS] -----------------------------------------------------------------------


def exited(result: Outcome[object] | None, /) -> int:
    """Writes a command's result on stdout as JSON lines, one per item of a tuple, or each distinct fault message on stderr.

    Returns:
        Exit status: zero for help, version, and a written result, an integer result itself, and the `EXITS` status of the first fault's reason.
    """
    match result:
        case None:
            return 0
        case Result(tag="ok", ok=int(status)):
            return status
        case Result(tag="ok", ok=value):
            sys.stdout.buffer.write(msgspec.json.Encoder().encode_lines(value if isinstance(value, tuple) else (value,)))
            return 0
        case Result(error=faults):
            sys.stderr.writelines(dict.fromkeys(f"{fault.message}\n" for fault in faults))
            return EXITS[faults[0].reason]


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EXITS", "exited"]

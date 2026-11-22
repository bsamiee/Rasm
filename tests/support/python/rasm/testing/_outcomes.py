"""Values the plugins of the package share: the stash key of a test's outcome and the outcome a test step settles."""

from expression import Error, Ok
import pytest

from rasm.records import Fault, Outcome

# --- [CONSTANTS] ------------------------------------------------------------------------

FAILED = pytest.StashKey[bool]()

# --- [OPERATIONS] -----------------------------------------------------------------------


def settled[T](outcome: Outcome[T], /) -> T:
    """Returns the value of an outcome, or fails the test with each fault's report text.

    Returns:
        The value of an `Ok` outcome.
    """
    match outcome:
        case Ok(value):
            return value
        case Error(faults):
            pytest.fail("\n".join(fault.message for fault in faults), pytrace=False)


def raised[T](outcome: Outcome[T], /) -> T:
    """Returns the value of an outcome, or raises the exception its first fault holds as cause, a fault holding none settled as `settled` does.

    Returns:
        The value of an `Ok` outcome.
    """
    match outcome:
        case Error((Fault(cause=BaseException() as cause), *_)):
            raise cause
        case _:
            return settled(outcome)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FAILED", "raised", "settled"]

"""Record faults and result accumulation."""

from abc import abstractmethod
from collections.abc import Callable, Iterable
from importlib.resources.abc import Traversable
from itertools import chain
from pathlib import Path
import struct
from typing import ClassVar, Literal, overload, override, Protocol

from expression import Error, Ok, Result
from expression.collections import Block
import msgspec
import numpy as np
import numpy.typing as npt

# --- [TYPES] ----------------------------------------------------------------------------

type Faults[F] = tuple[F, *tuple[F, ...]]
type _Numeric = FloatingPointError | OverflowError | MemoryError | ValueError
type RecordFault = NumericFault | FileFault | DomainFault
type RecordResult[T] = Result[T, Faults[RecordFault]]


class _Refused[R, F](Protocol):
    """Fault constructor over request and numeric error."""

    def __call__(self, *, request: R, error: _Numeric) -> F:
        """Constructs fault."""
        ...


# --- [MODELS] ---------------------------------------------------------------------------


class Fault(msgspec.Struct, frozen=True, kw_only=True):
    """Refused or unmet operation with its report text."""

    @property
    @abstractmethod
    def message(self) -> str:
        """Report text of the fault."""


class Refusal[R, E](Fault, frozen=True, kw_only=True):
    """Request with error that refused it."""

    template: ClassVar[str]
    request: R
    error: E

    @property
    @override
    def message(self) -> str:
        """Report text of the refusal."""
        return self.template.format(request=self.request, error=self.error)


# --- [ERRORS] ---------------------------------------------------------------------------


class NumericFault(Refusal[msgspec.Struct | npt.NDArray[np.float64] | tuple[float | msgspec.Struct, ...], _Numeric | TypeError], frozen=True, kw_only=True, eq=False):
    """Request numeric computation, allocation, or native representation refused."""

    @property
    @override
    def message(self) -> str:
        """Report text naming the numeric error type."""
        return f"Numeric operation for {self.request} failed ({type(self.error).__name__}). {self.error}"


class FileFault(
    Refusal[str | Path | Traversable, OSError | ValueError | TypeError | KeyError | OverflowError | MemoryError | NotImplementedError | struct.error | msgspec.DecodeError | str],
    frozen=True,
    kw_only=True,
):
    """File or stored form its reader or writer refused."""

    access: Literal["read", "write"]

    @property
    @override
    def message(self) -> str:
        """Report text naming the refused direction."""
        return f"{self.request} is {'unreadable' if self.access == 'read' else 'unwritable'}. {self.error}"


class DomainFault(Fault, frozen=True, kw_only=True, eq=False):
    """Value outside the domain its operation accepts."""

    subject: str
    value: object
    accepted: str

    @property
    @override
    def message(self) -> str:
        """Report text naming subject, accepted domain, and value."""
        return f"{self.subject} accepts {self.accepted}, not {self.value}"


# --- [OPERATIONS] -----------------------------------------------------------------------


def accumulated[T, F](results: Iterable[Result[T, Faults[F]]]) -> Result[tuple[T, ...], Faults[F]]:
    """Combines results, keeping every fault in input order."""
    block: Block[Result[T, Faults[F]]] = Block.of_seq(results)
    faults: tuple[F, ...] = tuple(chain.from_iterable(block.choose(lambda result: result.swap().to_option())))
    return Error((faults[0], *faults[1:])) if faults else Ok(tuple(block.choose(lambda result: result.to_option())))


@overload
def paired[A, B, FA, FB](a: Result[A, Faults[FA]], b: Result[B, Faults[FB]], /) -> Result[tuple[A, B], Faults[FA | FB]]: ...
@overload
def paired[A, B, C, FA, FB, FC](a: Result[A, Faults[FA]], b: Result[B, Faults[FB]], c: Result[C, Faults[FC]], /) -> Result[tuple[A, B, C], Faults[FA | FB | FC]]: ...
@overload
def paired[A, B, C, D, FA, FB, FC, FD](
    a: Result[A, Faults[FA]], b: Result[B, Faults[FB]], c: Result[C, Faults[FC]], d: Result[D, Faults[FD]], /
) -> Result[tuple[A, B, C, D], Faults[FA | FB | FC | FD]]: ...
@overload
def paired[A, B, C, D, E, FA, FB, FC, FD, FE](
    a: Result[A, Faults[FA]], b: Result[B, Faults[FB]], c: Result[C, Faults[FC]], d: Result[D, Faults[FD]], e: Result[E, Faults[FE]], /
) -> Result[tuple[A, B, C, D, E], Faults[FA | FB | FC | FD | FE]]: ...
@overload
def paired[A, B, C, D, E, G, FA, FB, FC, FD, FE, FG](
    a: Result[A, Faults[FA]], b: Result[B, Faults[FB]], c: Result[C, Faults[FC]], d: Result[D, Faults[FD]], e: Result[E, Faults[FE]], g: Result[G, Faults[FG]], /
) -> Result[tuple[A, B, C, D, E, G], Faults[FA | FB | FC | FD | FE | FG]]: ...
def paired[F](*results: Result[object, Faults[F]]) -> Result[tuple[object, ...], Faults[F]]:
    """Combines two to six results into one tuple, keeping every fault in input order."""
    return accumulated(results)


def guarded[R, T, F](
    fault: _Refused[R, F], request: R, compute: Callable[[], Result[T, Faults[F]]], raised: frozenset[Literal["over", "divide", "invalid"]] = frozenset(("over", "divide", "invalid"))
) -> Result[T, Faults[F]]:
    """Runs `compute` raising on the `raised` floating-point conditions, returning numeric error as `fault`."""
    try:
        with np.errstate(over="raise" if "over" in raised else None, divide="raise" if "divide" in raised else None, invalid="raise" if "invalid" in raised else None):
            return compute()
    except (FloatingPointError, OverflowError, MemoryError, ValueError) as error:
        return Error((fault(request=request, error=error),))


def scoped[R, T, F](acquired: Result[R, Faults[F]], use: Callable[[R], Result[T, Faults[F]]], release: Callable[[R], Result[object, Faults[F]]]) -> Result[T, Faults[F]]:
    """Uses and releases acquired resource on every path, release fault after use fault."""

    def held(resource: R) -> Result[T, Faults[F]]:
        try:
            used = use(resource)
        finally:
            released = release(resource)
        return paired(used, released).map(lambda pair: pair[0])

    return acquired.bind(held)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["DomainFault", "Fault", "Faults", "FileFault", "NumericFault", "RecordFault", "RecordResult", "Refusal", "accumulated", "guarded", "paired", "scoped"]

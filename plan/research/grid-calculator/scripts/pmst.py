"""Parse a compiled `idrc_PMST` string resource into its key and label pairs."""

from collections.abc import Iterator
from pathlib import Path
import struct
import sys

HEADER = 12
"""Bytes of version, reserved, and count that precede the first length-prefixed string."""

LENGTH = struct.Struct("<H")
"""Little-endian `uint16` that prefixes each ASCII string."""


def strings(data: bytes) -> Iterator[str]:
    """Walks the length-prefixed strings that follow the header, a byte outside UTF-8 prints as its escape.

    A zero length and a length that runs past the end both skip the prefix and resume at the next one.

    Args:
        data: Whole resource file.

    Yields:
        Each entry in file order.
    """
    offset = HEADER
    while offset + LENGTH.size <= len(data):
        (size,) = LENGTH.unpack_from(data, offset)
        offset += LENGTH.size
        if size == 0 or offset + size > len(data):
            continue
        yield data[offset : offset + size].decode(errors="backslashreplace")
        offset += size


def report(path: Path) -> str:
    """Renders one resource: a banner line, then a `key -> label` line per pair.

    Args:
        path: Compiled `idrc_PMST` resource.

    Returns:
        The report text, an odd trailing entry dropped.
    """
    values = tuple(strings(path.read_bytes()))
    return f"===== {path}\n" + "".join(f"{key!r} -> {label!r}\n" for key, label in zip(values[::2], values[1::2], strict=False))


def main() -> int:
    """Writes the pairs of every resource the arguments name, usage and 2 with no argument.

    Returns:
        The process exit code.
    """
    match [Path(argument) for argument in sys.argv[1:]]:
        case []:
            sys.stdout.write("pmst.py <file>.idrc...\n")
            return 2
        case paths:
            sys.stdout.write("".join(report(path) for path in paths))
            return 0


if __name__ == "__main__":
    sys.exit(main())

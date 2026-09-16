"""Query the arm64 disassembly listing: `PMReal` helper-call traces, widget-ID loads, the callees of a range, and the callers of a target.

Usage: disassembly.py trace <file>.dis <helper>,<helper>,...
       disassembly.py widgets <file>.dis <widget id>...
       disassembly.py callees <file>.dis <low> <high>
       disassembly.py callers <file>.dis funcs.txt <target>...
A pseudo-function opens at the first instruction after a `ret` and closes at the next one, addresses are hexadecimal.
"""

from bisect import bisect_right
from collections import Counter
from collections.abc import Iterable, Iterator
from itertools import dropwhile, groupby, islice, takewhile
from operator import itemgetter
from pathlib import Path
import re
import sys
from types import MappingProxyType

HELPERS = MappingProxyType({0x13490: "new", 0x157C8: "div", 0x15A7C: "mul", 0x17C4C: "add", 0x28080: "sub", 0x15ABC: "ROUND", 0x37D70: "FLOOR", 0x15808: "ge", 0x14140: "gt", 0x152F0: "dec"})
"""Entry address of each `PMReal` helper against the name the traces print."""

SECTION = re.compile(r"^Disassembly of section (\S+):$")
"""Header line objdump prints before each section's instructions."""

INSTRUCTION = re.compile(r"^\s+([0-9a-f]+):\s+(\S+)\s*(.*)$")
"""Disassembly line: address, mnemonic, and operands."""

BRANCH = re.compile(r"^0x([0-9a-f]+)")
"""`bl` operands: the target address ahead of the symbol objdump names it by."""

IMMEDIATE = re.compile(r"^w(\d+), #0x([0-9a-f]+)\b")
"""`mov` operands loading a 16-bit immediate into a register, ahead of objdump's decimal comment."""

HIGH_HALF = re.compile(r"^w(\d+), #0x([0-9a-f]+), lsl #16")
"""`movk` operands raising the upper half of a loaded immediate."""

CONTEXT = 4
"""Calls printed on either side of a trace match."""

type Instruction = tuple[int, str, str]
type Block = tuple[int, tuple[Instruction, ...]]
type Call = tuple[int, str]
type Span = tuple[int, int, str]


def section(lines: Iterable[str], name: str) -> Iterator[str]:
    """Keeps the lines of one section of the listing.

    Args:
        lines: Listing lines in file order.
        name: Section name as objdump prints it, such as `__TEXT,__text`.

    Returns:
        The lines after the section's header up to the next header.
    """
    return takewhile(lambda line: not SECTION.prefixmatch(line), islice(dropwhile(lambda line: line != f"Disassembly of section {name}:", lines), 1, None))


def instructions(lines: Iterable[str]) -> Iterator[Instruction]:
    """Parses the instruction lines of the listing, header and blank lines dropped.

    Args:
        lines: Listing lines in file order.

    Returns:
        The tuple (address, mnemonic, operands) per instruction.
    """
    return ((int(found.group(1), 16), found.group(2), found.group(3)) for line in lines if (found := INSTRUCTION.prefixmatch(line)))


def blocks(listing: Iterable[Instruction]) -> Iterator[Block]:
    """Splits the instructions at every `ret` into pseudo-functions.

    Args:
        listing: Instructions in address order.

    Yields:
        The tuple (start, body), where start is the entry address and body holds the instructions through the `ret`.
    """
    start: int | None = None
    body: list[Instruction] = []
    for instruction in listing:
        address, mnemonic, _ = instruction
        if start is None:
            start = address
        body.append(instruction)
        if mnemonic == "ret":
            yield start, tuple(body)
            start, body = None, []
    if start is not None:
        yield start, tuple(body)


def branches(body: Iterable[Instruction]) -> Iterator[tuple[int, int]]:
    """Lists the `bl` instructions with their targets.

    Args:
        body: Instructions to read.

    Returns:
        The tuple (address, target) per call.
    """
    return ((address, int(found.group(1), 16)) for address, mnemonic, operands in body if mnemonic == "bl" and (found := BRANCH.prefixmatch(operands)))


def helper_calls(body: Iterable[Instruction]) -> tuple[Call, ...]:
    """Keeps the `bl` instructions reaching a `PMReal` helper.

    Args:
        body: Instructions to read.

    Returns:
        The tuple (address, helper name) per call in address order.
    """
    return tuple((address, HELPERS[target]) for address, target in branches(body) if target in HELPERS)


def widget_loads(body: Iterable[Instruction]) -> Iterator[tuple[int, int]]:
    """Pairs each `movk` raising a register's upper half with the last `mov` that loaded its lower half.

    Args:
        body: Instructions of one pseudo-function.

    Yields:
        The tuple (address of the `movk`, 32-bit immediate).
    """
    low: dict[str, int] = {}
    for address, mnemonic, operands in body:
        if mnemonic == "mov" and (found := IMMEDIATE.prefixmatch(operands)):
            low[found.group(1)] = int(found.group(2), 16)
        elif mnemonic == "movk" and (found := HIGH_HALF.prefixmatch(operands)) and (register := found.group(1)) in low:
            yield address, int(found.group(2), 16) << 16 | low[register]


def hit(block: Block, pattern: tuple[str, ...]) -> str:
    """Renders the first run of one pseudo-function whose helper names hold the pattern end to end, with its surrounding calls.

    Args:
        block: Pseudo-function to search.
        pattern: Helper names to match end to end.

    Returns:
        The report line, empty when no run matches.
    """
    start, body = block
    calls = helper_calls(body)
    names = tuple(name for _, name in calls)
    width = len(pattern)
    if (i := next((i for i in range(len(names) - width + 1) if names[i : i + width] == pattern), None)) is None:
        return ""
    window = names[max(0, i - CONTEXT) : i + width + CONTEXT]
    return f"func@{start:x} hit@{calls[i][0]:x} seq={','.join(window)}\n"


def trace(listing: Path, pattern: tuple[str, ...]) -> str:
    """Renders one line per pseudo-function whose helper calls hold the pattern.

    Args:
        listing: Disassembly listing of the arm64 slice.
        pattern: Helper names to match end to end.

    Returns:
        The report text, empty when nothing matches.
    """
    return "".join(hit(block, pattern) for block in blocks(instructions(listing.read_text(encoding="utf-8").splitlines())))


def widgets(listing: Path, wanted: frozenset[int]) -> str:
    """Renders, per wanted widget ID, each pseudo-function loading it with the count and first address of the loads.

    Args:
        listing: Disassembly listing of the arm64 slice.
        wanted: Widget IDs to report.

    Returns:
        The report text, empty when no pseudo-function loads a wanted ID.
    """
    loads = sorted((widget, start, address) for start, body in blocks(instructions(listing.read_text(encoding="utf-8").splitlines())) for address, widget in widget_loads(body) if widget in wanted)
    report: list[str] = []
    for widget, in_widget in groupby(loads, itemgetter(0)):
        report.append(f"widget {widget:x}\n")
        for start, in_block in groupby(in_widget, itemgetter(1)):
            addresses = [address for _, _, address in in_block]
            report.append(f"  func@{start:x} x{len(addresses)} first@{addresses[0]:x}\n")
    return "".join(report)


def callees(listing: Path, low: int, high: int) -> str:
    """Renders each text-section target the instructions in [low, high) call, other than the helpers, with its call count and helper sequence.

    Args:
        listing: Disassembly listing of the arm64 slice.
        low: First address of the range.
        high: First address past the range.

    Returns:
        The report text, one line per target in address order.
    """
    text = tuple(instructions(section(listing.read_text(encoding="utf-8").splitlines(), "__TEXT,__text")))
    functions = dict(blocks(text))
    calls = Counter(target for address, target in branches(text) if low <= address < high and text[0][0] <= target <= text[-1][0] and target not in HELPERS)
    sequences = {target: tuple(name for _, name in helper_calls(functions.get(target, ()))) for target in calls}
    return "".join(
        f"callee@{target:x} calls={count} helpers={len(sequences[target])}" + (f" seq={','.join(sequences[target])}" if sequences[target] else "") + "\n" for target, count in sorted(calls.items())
    )


def owner(spans: tuple[Span, ...], address: int) -> str:
    """Names the function whose span holds the address.

    Args:
        spans: The tuple (entry, size, name) per function, sorted by entry.
        address: Address to place.

    Returns:
        The function name, `none@<address>` outside every span.
    """
    index = bisect_right(spans, address, key=itemgetter(0))
    match spans[index - 1 : index]:
        case [(entry, size, name)] if address < entry + size:
            return name
        case _:
            return f"none@{address:x}"


def callers(listing: Path, functions: Path, targets: tuple[int, ...]) -> str:
    """Renders, per target, the functions of the `ListFunctions` map that call it with their call counts.

    Args:
        listing: Disassembly listing of the arm64 slice.
        functions: `ListFunctions` output, `<entry> <size> <name>` per line.
        targets: Call targets to report, in the order given.

    Returns:
        The report text, callers sorted by name under each target.
    """
    spans = tuple(sorted((int(entry, 16), int(size), name) for entry, size, name in (line.split() for line in functions.read_text(encoding="utf-8").splitlines())))
    calls = Counter((target, owner(spans, address)) for address, target in branches(instructions(listing.read_text(encoding="utf-8").splitlines())) if target in targets)
    return "".join(f"target {target:x}\n" + "".join(f"  {name} x{count}\n" for (called, name), count in sorted(calls.items()) if called == target) for target in targets)


def main() -> int:
    """Writes the report the arguments select, usage and 2 for any other form.

    Returns:
        The process exit code.
    """
    match sys.argv[1:]:
        case ["trace", listing, pattern]:
            sys.stdout.write(trace(Path(listing), tuple(pattern.split(","))))
        case ["widgets", listing, *ids]:
            sys.stdout.write(widgets(Path(listing), frozenset(int(widget, 16) for widget in ids)))
        case ["callees", listing, low, high]:
            sys.stdout.write(callees(Path(listing), int(low, 16), int(high, 16)))
        case ["callers", listing, functions, *targets]:
            sys.stdout.write(callers(Path(listing), Path(functions), tuple(int(target, 16) for target in targets)))
        case _:
            sys.stdout.write("disassembly.py trace <file>.dis <helper>,<helper>,... | widgets <file>.dis <id>... | callees <file>.dis <low> <high> | callers <file>.dis funcs.txt <target>...\n")
            return 2
    return 0


if __name__ == "__main__":
    sys.exit(main())

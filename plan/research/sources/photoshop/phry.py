"""Read-only reader for the 8BIMphry group hierarchy inside a Photoshop .psp preset store."""

from __future__ import annotations

import struct
import sys
from pathlib import Path


def _utf16_text(data: bytes, offset: int) -> tuple[str, int]:
    (count,) = struct.unpack_from(">I", data, offset)
    offset += 4
    raw = data[offset : offset + count * 2]
    offset += count * 2
    return raw.decode("utf-16-be", "replace").rstrip("\x00"), offset


def _walk(data: bytes, offset: int, depth: int, out: list[str], limit: int) -> int:
    """Walk one descriptor body, emitting Grup names and preset names."""
    end = len(data)
    while offset < end and len(out) < limit:
        marker = data[offset : offset + 4]
        if marker == b"Grup":
            offset += 4
            name, offset = _read_name(data, offset)
            out.append(f"{'  ' * depth}GROUP {name}")
            depth += 1
        elif marker == b"grpE" or data[offset : offset + 8] == b"groupEnd":
            offset += 8 if data[offset : offset + 8] == b"groupEnd" else 4
            depth = max(0, depth - 1)
        else:
            offset += 1
    return offset


def _read_name(data: bytes, offset: int) -> tuple[str, int]:
    idx = data.find(b"Nm  TEXT", offset, offset + 64)
    if idx < 0:
        return "<?>", offset
    return _utf16_text(data, idx + 8)


def hierarchy(path: Path) -> list[str]:
    data = path.read_bytes()
    idx = data.find(b"8BIMphry")
    if idx < 0:
        return ["<no 8BIMphry block>"]
    body = data[idx:]
    lines: list[str] = []
    pos = 0
    depth = 0
    counts: list[int] = []
    while pos < len(body):
        if body[pos : pos + 4] == b"Grup":
            name, npos = _read_name(body, pos + 4)
            lines.append(f"{'  ' * depth}[GROUP] {name}")
            counts.append(0)
            depth += 1
            pos = npos
        elif body[pos : pos + 8] == b"groupEnd":
            depth = max(0, depth - 1)
            if counts:
                total = counts.pop()
                lines.append(f"{'  ' * depth}         -> {total} presets")
            pos += 8
        elif body[pos : pos + 6] == b"preset":
            if counts:
                counts[-1] += 1
            pos += 6
        else:
            pos += 1
    return lines


def main() -> int:
    for arg in sys.argv[1:]:
        path = Path(arg)
        print(f"########## {path.name}  ({path.stat().st_size} bytes) ##########")
        for line in hierarchy(path):
            print(line)
        print()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

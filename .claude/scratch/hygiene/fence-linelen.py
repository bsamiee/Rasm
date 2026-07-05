#!/usr/bin/env python3
"""Fence-line length gate for codemap/seam fences in markdown docs.

Reports file:line:length for lines INSIDE tree/seam fences that exceed the cap;
prose outside those fences is never reported. A fence is in scope when its info
string carries `codemap` or `seams`, or when its body draws tree/seam glyphs.

Usage: fence-linelen.py [--cap N] <file|dir> [...]
"""
import pathlib
import re
import sys

CAP = 120
GLYPH = re.compile(r"[│├└⇄←→]")

args = []
it = iter(sys.argv[1:])
for a in it:
    if a == "--cap":
        CAP = int(next(it))
    else:
        args.append(a)

files = []
for a in args:
    p = pathlib.Path(a)
    if p.is_dir():
        files.extend(sorted(p.rglob("*.md")))
    else:
        files.append(p)

hits = 0
for f in files:
    try:
        lines = f.read_text(encoding="utf-8").splitlines()
    except (OSError, UnicodeDecodeError):
        continue
    in_fence = False
    scope = False
    tagged = False
    for i, line in enumerate(lines, 1):
        if line.startswith("```"):
            if not in_fence:
                in_fence = True
                info = line[3:].strip().lower()
                tagged = ("codemap" in info) or ("seams" in info)
                scope = tagged
            else:
                in_fence = False
                scope = False
            continue
        if in_fence and not scope and not tagged and GLYPH.search(line):
            scope = True
        if in_fence and scope and len(line) > CAP:
            hits += 1
            print(f"{f}:{i}: {len(line)} chars (cap {CAP}); overflow: ...{line[CAP:CAP + 60]}")

print(f"-- {hits} offending fence line(s) at cap {CAP}", file=sys.stderr)

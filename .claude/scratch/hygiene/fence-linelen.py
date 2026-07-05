#!/usr/bin/env python3
"""Deterministic hygiene gates for planning docs. Two scoped checks, zero prose reach beyond them.

FENCE GATE: lines inside tree/seam fences (info string carries `codemap`/`seams`, or the body
draws tree/seam glyphs) longer than the cap. Prose outside those fences is never reported.

CARD GATE (README.md only): package-card rows shaped `- \x60<package>\x60 ...` — reports a row
longer than the cap, and parenthesis notation after the package span (the dash-notation law:
one concise dash-led line, never parentheses).

Usage: fence-linelen.py [--cap N] [--no-cards] <file|dir> [...]
"""
import pathlib
import re
import sys

CAP = 160
CARDS = True
GLYPH = re.compile(r"[│├└⇄←→]")
CARD = re.compile(r"^\s*- `[^`]+`(\s*)(.*)$")

args = []
it = iter(sys.argv[1:])
for a in it:
    if a == "--cap":
        CAP = int(next(it))
    elif a == "--no-cards":
        CARDS = False
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
    is_readme = f.name == "README.md"
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
        if in_fence:
            if not scope and not tagged and GLYPH.search(line):
                scope = True
            if scope and len(line) > CAP:
                hits += 1
                print(f"{f}:{i}: FENCE {len(line)} chars (cap {CAP}); overflow: ...{line[CAP:CAP + 60]}")
            continue
        if CARDS and is_readme:
            m = CARD.match(line)
            if m:
                if len(line) > CAP:
                    hits += 1
                    print(f"{f}:{i}: CARD {len(line)} chars (cap {CAP}); overflow: ...{line[CAP:CAP + 60]}")
                if m.group(2).startswith("("):
                    hits += 1
                    print(f"{f}:{i}: CARD parenthesis notation; use one dash-led line: {line.strip()[:90]}")

print(f"-- {hits} finding(s) at cap {CAP}", file=sys.stderr)

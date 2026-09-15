"""Recompute the cloud-drive.md palette tables from the canonical JSON with coloraide."""

import json
import re
import sys
from pathlib import Path

from coloraide import Color

SP = Path("/private/tmp/claude-501/-Users-bardiasamiee-Documents-99-Github-Rasm/5491787f-d5bf-4f29-95b6-7ed9e2976566/scratchpad")
doc = (SP / "design/cloud-drive.md").read_text()
data = json.loads((SP / "illustrator/d-my-color-palette.json").read_text())


def rgb_of(sw):
    c = sw["color"]
    if c["type"] == "RGBColor":
        return (c["red"], c["green"], c["blue"]), "process"
    if c["type"] == "SpotColor":
        s = c["spotColor"]
        return (s["red"], s["green"], s["blue"]), "spot"
    return None, c["type"]


def a98(rgb):
    c = Color("srgb", [v / 255 for v in rgb]).convert("a98-rgb")
    clipped = c.clone().clip()
    return tuple(round(v * 255) for v in clipped.coords()), c.in_gamut("a98-rgb")


canon = {}
for sw in data["swatches"]:
    rgb, kind = rgb_of(sw)
    if rgb is None:
        continue
    canon.setdefault(sw["name"], []).append((sw["group"], rgb, kind))

rows = re.findall(r"^\| \[(\d+)\] \| (Root|Base|Expanded) \| ([^|]+?) \| (process|spot) \| ([\d,]+) \| ([\d,]+) \| ([^|]+?) \| ([\d,]+) \| (\d+) \|$", doc, re.M)
print("table rows", len(rows))
bad = 0
for idx, group, name, kind, srgb, target, ase, tmpl, delta in rows:
    nm = name.split(" (")[0].split(" → ")[0].strip()
    srgb_t = tuple(int(x) for x in srgb.split(","))
    target_t = tuple(int(x) for x in target.split(","))
    ase_t = tuple(int(x) for x in re.findall(r"\d+", ase)[:3])
    comp, ingamut = a98(srgb_t)
    d = max(abs(a - b) for a, b in zip(ase_t, comp))
    entries = canon.get(nm, [])
    found = [e for e in entries if e[1] == srgb_t]
    problems = []
    if not found:
        problems.append(f"srgb not in canon {entries}")
    else:
        g, _, k = found[0]
        if k != kind:
            problems.append(f"kind {k}")
    if comp != target_t:
        problems.append(f"target {target_t} -> {comp}")
    if d != int(delta):
        problems.append(f"delta {delta} -> {d}")
    if not ingamut:
        problems.append("out of a98 gamut before clip")
    if problems:
        bad += 1
        print(idx, nm, problems)
print("rows with problems", bad)

# count solids by group in canon
from collections import Counter
cnt = Counter()
for sw in data["swatches"]:
    rgb, kind = rgb_of(sw)
    if rgb is not None:
        cnt[(sw["group"], kind)] += 1
print("solids by group/kind", dict(cnt))

print("--- neutral ramps")
for i in range(1, 10):
    L = 0.15 + 0.10 * (i - 1)
    out = []
    for h in (70, 250):
        c = Color("oklch", [L, 0.012, h]).convert("a98-rgb")
        fitted = c.clone().fit("a98-rgb")
        out.append((tuple(round(v * 255) for v in fitted.coords()), c.in_gamut("a98-rgb")))
    print(i, round(L, 2), out)

print("--- accents")
for name, rgb in (("A", (58, 103, 162)), ("B", (222, 80, 19)), ("C", (87, 138, 90))):
    base = Color("a98-rgb", [v / 255 for v in rgb])
    ok = base.convert("oklch")
    light = base.mix(Color("a98-rgb", [1, 1, 1]), 0.40, space="oklab").convert("a98-rgb")
    dark = ok.clone()
    dark["lightness"] = dark["lightness"] - 0.15
    dark_a = dark.convert("a98-rgb")
    print(name, [round(v, 3) for v in ok.coords()], "light", tuple(round(v * 255) for v in light.clone().fit("a98-rgb").coords()), light.in_gamut("a98-rgb"), "dark", tuple(round(v * 255) for v in dark_a.clone().fit("a98-rgb").coords()), dark_a.in_gamut("a98-rgb"))

print("--- tints")
for name, rgb in (("Site", (188, 174, 145)), ("Water", (58, 103, 162)), ("Veg", (94, 120, 71)), ("Circ", (222, 80, 19))):
    print(name, [tuple(round(255 + t * (v - 255)) for v in rgb) for t in (0.2, 0.3, 0.4)])

print("--- HSB row")
h, s, v = 2850 / 182.04, 3084 / 65535, 33410 / 65535
c = Color("hsv", [h, s, v])
print(round(h, 1), round(s * 100, 1), round(v * 100, 1), tuple(round(x * 255) for x in c.convert("srgb").coords()), tuple(round(x * 255) for x in c.convert("a98-rgb").coords()))

print("--- Colorplan 8-bit check")
rows = re.findall(r"^\| \[\d+\] \| ([^|]+?) \| ([\d.,]+) \| ([\d,]+) \| Colorplan", doc, re.M)
mism = [(n, f, e) for n, f, e in rows if tuple(round(float(x) * 255) for x in f.split(",")) != tuple(int(x) for x in e.split(","))]
print(len(rows), "mismatches", mism)

print("--- ACO CMYK check")
rows = re.findall(r"^\| \[\d+\] \| Show It Better Color Palette (\d)\.aco \| (\d) \| CMYK \| ([\d,]+) \| CMYK ([\d.,]+) \|", doc, re.M)
mism = []
for f, slot, raw, frac in rows:
    r = [int(x) for x in raw.split(",")]
    exp = tuple(round((65535 - x) / 65535, 4) for x in r)
    got = tuple(float(x) for x in frac.split(","))
    if exp != got:
        mism.append((f, slot, exp, got))
print(len(rows), "mismatches", mism)

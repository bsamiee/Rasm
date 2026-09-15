# Font inventory

Read-only survey taken 2026-09-11. Nothing was moved, installed, or activated. Every claim below rests on a table read out of the file itself (`name`, `OS/2`, `head`, `fvar`, `cmap`, `GSUB`, `GPOS`, `post`, `morx`), on a byte hash, or on a manifest the vendor wrote.

Companion files in this directory:

| File | Contents |
| :--- | :--- |
| `font-inventory.tsv` | 1784 rows, one per face, 33 columns, sources A + B + C + D |
| `system-arabic.tsv` | 74 rows, the `/System/Library/Fonts` faces that `fc-list :lang=ar` returns |
| `scan.py` | The extractor that produced both tables |
| `views.sql`, `q.sh`, `q2.sh` | DuckDB views and query helpers over the tables |
| `md5_AB.txt`, `dataless.txt`, `list_*.txt` | Hashes, cloud-placeholder list, and the file enumerations |

## 1. Totals and what was actually read

1561 desktop files were opened and parsed, yielding 1784 faces (TrueType collections expand) across 163 distinct typographic families. Parsing produced zero errors: every `.otf`, `.ttf`, `.ttc` in scope was structurally sound.

| Tier | Location | Files | Faces | Families |
| :--- | :--- | ---: | ---: | ---: |
| A-paid | Drive `00.Paid Premium Fonts` | 52 | 52 | 6 |
| A-premium | Drive `01.Premium Fonts` | 318 | 318 | 39 |
| A-free | Drive `02.Free Fonts` | 235 | 305 | 16 |
| B-owned | `design-tools/fonts/owned` | 425 | 425 | 64 |
| B-google | `design-tools/fonts/google/source-sans-3` | 2 | 2 | 1 |
| C-forge | `~/Library/Fonts/HomeManager` | 119 | 272 | 25 |
| C-user | `~/Library/Fonts` loose | 5 | 5 | 4 |
| C-system | `/Library/Fonts` | 87 | 87 | 10 |
| D-adobe | Adobe CoreSync `livetype/.r` + `.w` | 318 | 318 | 55 |

Not parsed, counted only:

- **Google Fonts mirror** at `design-tools/fonts/google/`: **1687 family directories, 3124 files** (3120 `.ttf`, 4 `.otf`). Naming is `<family>-<style>.ttf` with `-variable` suffixes for VF builds, so this is a scripted mirror of the Google Fonts catalogue rather than a curated set. Only `source-sans-3` was parsed, as instructed: two variable files, roman and italic, `wght 200–900`, rev 3.052, 2478 glyphs, Latin/Greek/Cyrillic, with `smcp` and `onum`.
- **Web-only formats in the Drive library**: **606 files — 237 `.woff` + 369 `.woff2`** — plus **226 `.eot`**, 12 `.css`, 9 `.html`. These are excluded from the TSV by design.

### Cloud placeholder status

The Drive library is **182 MB** and is almost entirely materialised on disk. `ls -lO` reports the `dataless` flag on exactly **141 files, and every one of them is a `.woff` or `.woff2`** (60 and 81 respectively), concentrated in `TT_Supermolot` (63), `Univers` (54), `Geist Typeface` (20) and `Server Mono` (4).

**No desktop font file in the Drive library is dataless.** Every `.otf`, `.ttf` and `.ttc` in sources A was present locally and fully readable, so the metadata in this report is read from real font binaries, not inferred from filenames. The `dataless` column in the TSV is `no` on all 1784 rows for that reason.

## 2. Families by tier

### Paid premium — Drive `00.Paid Premium Fonts` (6 families, 52 files)

| Family | Styles | Format | Glyphs | Rev | smcp | onum | tnum | Note |
| :--- | ---: | :--- | ---: | :--- | :---: | :---: | :---: | :--- |
| Ivar Text | 8 | OTF/CFF | 614 | 1.802 | yes | yes | yes | Complete OT: `c2sc frac sups subs case ordn numr dnom locl ss01 ss02 ss12`, `mark`+`mkmk` |
| Ivar Display | 8 | OTF/CFF | 493 | 1.802 | yes | yes | yes | Same feature set, display cut |
| Ivar Headline | 8 | OTF/CFF | 493 | 1.802 | yes | yes | yes | Same feature set |
| Ivar Fine | 10 | OTF/CFF | 493 | 1.802 | yes | yes | yes | Same feature set, finest optical size |
| Swedish Gothic | 14 | OTF/CFF | 474 | 1.401 | no | no | yes | No small caps, no oldstyle; figures and `mark`/`mkmk` present |
| Tor Grotesk Mix | 4 | OTF + VF TTF | 638 | 1.000 | no | yes | yes | Variable axis `DOSZ 1–100` (custom, non-registered); three static OTF cuts plus one VF |

This is the only tier with provenance in the folder: `_LicenseAgreement.txt` sits beside Ivar, Ivar Fine and Swedish Gothic (Letters from Sweden, designer Göran Söderström in `name` ID 9). The four Ivar subfamilies together form a genuine optical-size system — Text 614 glyphs, Display/Headline/Fine 493 — and are the single strongest editorial asset in the whole collection. Tor Grotesk Mix has no licence file and revision 1.000 with an unregistered axis tag, consistent with a small-foundry or beta release.

### Premium — Drive `01.Premium Fonts` (39 families, 318 files)

Ten of the eighteen folders are **web-font kits**, not desktop releases. Each contains `stylesheet.css`, `demo.html`, and every style in four parallel formats — `.eot`, `.ttf`, `.woff`, `.woff2`. The `.ttf` in such a kit is a web conversion: the format an `@font-face` generator emits, stripped of the CFF outlines and the hinting the retail release ships with.

| Kit folder | Family | Styles | Evidence |
| :--- | :--- | ---: | :--- |
| `Akzidenz-Grotesk` | Akzidenz-Grotesk Next | 14 | eot/ttf/woff/woff2 + css + html |
| `BauerBodoni` | Bauer Bodoni Std | 8 | same |
| `Bembo Book` | Bembo Std | 8 | same |
| `DIN Mittelschrift` | DIN Schrift | 2 | same |
| `Frutiger` | Frutiger LT Std | 14 | same |
| `Frutiger Neue` | Frutiger Neue LT W1G | 40 | same |
| `Gill Sans Nova` | Gill Sans Nova (+ Deco, Inline, Shadowed) | 43 | same |
| `Gotham` | Gotham | 16 | same |
| `TT_Supermolot` | TT Supermolot Neue | 54 | same |
| `Univers` | Univers LT Std | 27 | same |

226 `.eot` files across those ten folders is the count that pins it. The conversions vary in how much they preserved:

- **Gill Sans Nova** survived well — 1167 glyphs, `smcp c2sc onum tnum frac sups swsh ss01`, Latin Extended (145 cps), Greek (75), Cyrillic (136). Usable, if not the retail build.
- **Bauer Bodoni Std** (431 glyphs, rev 2.003) and **Bembo Std** (500, rev 2.045) kept `smcp c2sc onum tnum frac sups` but carry only 11 Latin Extended codepoints — the original Adobe Std character set, so nothing was lost that the Std release had.
- **Frutiger Neue LT W1G** is the widest coverage in the tier (655 glyphs, Latin Ext 133 / Greek 75 / Cyrillic 98) but exposes **no `smcp`, `onum` or `tnum`** at all.
- **Univers LT Std** (255 glyphs) and **Frutiger LT Std** (263) are Std character sets with **no discretionary features whatsoever** — `GSUB` is present but exposes nothing beyond defaults.
- **TT Supermolot Neue** (54 styles, 626 glyphs, Cyrillic 94) has `onum` and `tnum` but no small caps.
- **DIN Schrift** is the weakest: 232 glyphs, **`GSUB` empty**, version string `OTF 1.0;PS 001.001;Core 1.0.22` — a 1990s Type 1 conversion.

The eight non-kit folders are worse, not better:

- **`Futura/`** contains nineteen mismatched files from at least four sources — `futur.ttf`, `Futura Bold font.ttf`, `futura light bt.ttf`, `tt0205m_.ttf`, `unicode.futurab.ttf`, one stray `Futura-CondensedLight.otf` — and **a file named `sharefonts.net.txt` whose contents read `Free download fonts at http://sharefonts.net`**. Several of these parse as *Futura Bk/Md/Hv/Lt/XBlk BT* with version strings like `mfgpctt-v1.52 Tuesday, January 12, 1993 3:27:55 pm (EST)`, 260 glyphs, and **no `GSUB` or `GPOS` at all**. One face, family `Futura-Bold`, has **`head.fontRevision` 0.000 and no version name record** — the signature of a re-exported rip.
- **`Futura Italicized Cufonfonts/`** is 20 `FuturaLT-*.ttf` at rev 6.000, vendor ID `PfEd` (FontForge), **no `GSUB`, no `GPOS`, 235 glyphs**. `PfEd` means the file was opened and re-saved in FontForge, which is how layout tables get dropped. The folder name states the source.
- **`Helvetica Font/`** mixes `Helvetica.ttf` (2197 glyphs, Latin Ext 297, Cyrillic 208 — a real Linotype build) with `helvetica-compressed-5871d14b6903a.otf`, `helvetica-light-587ebe5a59211.ttf`, `helvetica-rounded-bold-5871d05ead8de.otf`. The hex-suffixed names are download-site artefacts; those three have **no `GSUB`**, 229 glyphs, and one carries the version string `Converter: Windows Type 1 Installer V1.0d.` One `Helvetica` face has revision 0.000.
- **`Memphis-font/`** — three faces (`Memphis-Bold`, `Memphis-Light`, `Memphis-LightItalic`) at **revision 0.000 with no version name and no layout tables**, 399 glyphs. Beside them, `Memphis Extra Bold` and `Memphis Medium` at rev 2.000 with the version string `1.001; 05-25-93` and vendor `LINO` — genuine Linotype, but still layout-free.
- **`meta-pro/`** is 26 uppercase `.TTF` files, FF Meta Pro rev 7.600, 1205 glyphs, Latin Ext 155 / Greek 79 / Cyrillic 136, with `smcp` and `tnum` but **no `onum`**. The most complete non-kit item in the tier.
- **`avenir-lt-pro/`** is 12 clean OTF/CFF files, rev 1.000, 388 glyphs, but with **no `smcp`, `onum` or `tnum`** — the plain LT Pro character set.
- **`Futura Standard Heavy/`** is a single `Futura Std Heavy.otf`, rev 1.029, 253 glyphs, no discretionary features.

### Free — Drive `02.Free Fonts` (16 families, 235 files, 305 faces)

| Family | Styles | Formats | Axes | Glyphs | Rev | Note |
| :--- | ---: | :--- | :--- | ---: | :--- | :--- |
| Inter / Inter Display | 108 each | OTF, TTF, TTC | — | 2937 | 4.000 and 4.001 | Two complete releases side by side, see §3 |
| Inter Variable | 4 | VF TTF | `opsz 14–32`, `wght 100–900` | 2937 | 4.000 / 4.001 | Full optical-size VF, `smcp` + `tnum` |
| Lato | 18 | TTF | — | 3055 | 3.100 | **Build defect, see below** |
| Roboto | 16 | TTF | — | 1037 | 1.000 | Older Roboto 1.x; `smcp`, no `onum`; specimen PDF included |
| Geist / Geist Mono | 9 each | OTF | — | 683 / 694 | 1.002 | Plus one VF each, `wght 100–1100` |
| Overused Grotesk | 16 + 1 VF | TTF/OTF | `wght 300–900` | 1361 | 0.005 | Pre-1.0 version number; Cyrillic 132; `tnum`, no small caps |
| Semplicità | 7 | OTF | — | 542 | 2.520 | Latin Ext 169; the `Ombra` shadow cut has **no `GSUB`** (expected for a display cut) |
| Server Mono | 2 | OTF | — | 421 | 1.000 | **Ships its own sources**: `ServerMono-Regular.glyphs`, two `.ufo` packages, 854 `.glif`, `features.fea` |
| Architects | 2 | TTF/OTF | — | 76 | 1.000 | 76 glyphs, no layout tables — a display face, uppercase only |
| CODEINK | 2 | TTF | — | 112 | 1.000 | Same: 112 glyphs, no layout tables |
| Bradley Hand ITC | 1 | TTF | — | 251 | 1.000 | ITC vendor ID but no layout tables |

**The Lato 3.100 build is broken and it is worth knowing before a tournament.** Verified twice, with `fontTools` and independently with `hb-info --list-tables --list-features`:

- Every **upright** face (`Lato-Regular.ttf` and the eight other romans) has a **16-byte, empty `GSUB`** — no `liga`, no `calt`, no `onum`, nothing — and a full 273 KB `GPOS` with `kern mark mkmk`.
- Every **italic** face has a full 26 KB `GSUB` (`aalt calt case ccmp dlig dnom frac hist liga lnum` …) and a **16-byte, empty `GPOS`** — that is, no kerning at all.

So Lato romans set without ligatures or figure sets, and Lato italics set without kerning. Both copies of Lato in the collection (Drive and `design-tools/owned`) are byte-identical and carry the same defect. The Adobe Fonts activation of Lato is a different, older build (rev 2.015, 3023 glyphs) that has both tables intact.

### Forge — `~/Library/Fonts/HomeManager` (25 families, 119 files, 272 faces)

Installed by home-manager into `noto/`, `opentype/` and `truetype/` subdirectories.

| Family | Styles | Axes | Glyphs | Rev |
| :--- | ---: | :--- | ---: | :--- |
| Iosevka, Iosevka Fixed, Iosevka Term | 54 each | — | 48202 | 34.030 |
| IBM Plex Sans Condensed | 16 | — | 1022 | 3.000 |
| IBM Plex Sans Arabic | 8 | — | 1704 | 1.005 |
| IBM Plex Sans Devanagari / Hebrew / Thai / Thai Looped | 8 each | — | 480–1119 | 1.002–1.003 |
| IBM Plex Sans JP / KR / SC / TC | 8 each | — | 12283–29562 | 1.000–1.004 |
| IBM Plex Mono Var, IBM Plex Serif Var | 2 each | `wght 100–700` | 1207 / 1055 | 1.000 |
| IBM Plex Math | 1 | — | 7092 | 1.000 |
| Geist, Geist Mono | 2 each | `wght 100–900` | 975 / 1159 | 1.800 / 1.700 |
| Noto Sans Arabic | 1 | `wght 100–900`, `wdth 62.5–100` | 1399 | 2.013 |
| Noto Naskh Arabic | 1 | `wght 400–700` | 1415 | 2.021 |
| Noto Sans Mono | 1 | `wght 100–900`, `wdth 62.5–100` | 3911 | 2.013 |
| Scheherazade New | 4 | — | 1822 | 4.400 |
| Hack | 4 | — | 1655 | 3.003 |
| Symbols Nerd Font, Symbols Nerd Font Mono | 1 each | — | 10522 | 1.000 |

Two facts about this tier matter operationally:

1. **CoreText sees these fonts; `fontconfig` does not.** `system_profiler SPFontsDataType` lists e.g. `/Users/bardiasamiee/Library/Fonts/HomeManager/truetype/ScheherazadeNew-Bold.ttf` with family `Scheherazade New`, so they are live in Font Book, InDesign, Illustrator and every native app. But `fc-list` returns zero rows containing `HomeManager`, and `fc-list :lang=fa` misses Noto Naskh Arabic, Noto Sans Arabic, Scheherazade New and IBM Plex Sans Arabic entirely. Any CLI pipeline driven by fontconfig will not find them.
2. The Forge Geist (rev 1.800 / 1.700, 975 and 1159 glyphs, variable `wght 100–900`) is **newer and larger** than the Drive Geist (rev 1.002, 683 and 694 glyphs, `wght 100–1100`). Note the axis range differs, so the two are not interchangeable in a document.

### Other user fonts — `~/Library/Fonts` loose (4 families, 5 files)

| Family | File | Axes | Glyphs | Rev |
| :--- | :--- | :--- | ---: | :--- |
| Playfair Display | `PlayfairDisplay[wght].ttf`, `PlayfairDisplay-Italic[wght].ttf` | `wght 400–900` | 1103 | 1.203 |
| Markazi Text | `MarkaziText-VF.ttf` | `wght 400–700` | 1240 | 1.000 |
| Reem Kufi | `ReemKufi[wght].ttf` | `wght 400–700` | 815 | 2.000 |
| Qahiri | `Qahiri-Regular.ttf` | — | 512 | 4.000 |

Playfair Display carries `smcp` and `onum`; the other three are the Perso-Arabic set covered in §4.

### System — `/Library/Fonts` (10 families, 87 files)

Entirely Apple's SF programme: SF Pro (Display, Text, Rounded, plus a two-instance VF with `wdth 30–150`, `opsz 17–28`, `wght 1–1000`), SF Compact (Display, Text, Rounded, VF), SF Arabic and SF Arabic Rounded. 35 000+ glyphs per face, `smcp` and `tnum` on the Latin cuts. Licensed for Apple-platform UI work, not for print or third-party products.

### Adobe Fonts — `livetype/.r` and `.w` (55 families, 318 files)

Covered in §7.

## 3. Duplicates

### Byte-identical copies

MD5 over all 1350 files in sources A, B and D found **396 redundant copies consuming 45.6 MB**:

| Overlap | Redundant copies |
| :--- | ---: |
| Drive `01.Premium` ↔ `design-tools/owned` | 310 |
| Drive `00.Paid Premium` ↔ `design-tools/owned` | 49 |
| Drive `02.Free` ↔ `design-tools/owned` | 35 |
| `design-tools/owned` ↔ Adobe `.w` | 2 |

The finding is structural: **`design-tools/fonts/owned/` is very nearly a re-foldered copy of the Drive library.** 393 of its 425 files have a byte-identical twin in Drive. No duplicate hash appears twice *within* a single location, so neither store duplicates internally.

Only **32 files in `design-tools/owned` have no Drive twin**, and they are the interesting ones:

| Directory | Files | What it is |
| :--- | ---: | :--- |
| `Futura-PT` | 6 | Rev 1.007, 640 glyphs, Cyrillic 175 — one revision behind the Adobe activation (1.009) |
| `Arnold` | 5 | Rev 1.000, 415 glyphs, Cyrillic 94, **no `GSUB`** |
| `Acumin-Pro` | 4 | 2 of these 4 are **byte-identical to Adobe Fonts cache files** in `livetype/.w` |
| `Futura-LT-Pro` | 3 | Rev 1.000, 393 glyphs, no discretionary features |
| `Berlin-Sans-FB` + `-Demi` | 3 | Font Bureau, **no layout tables** |
| `Gilroy` | 2 | Rev 1.000, 554 glyphs, `tnum` only |
| `Minion-Pro` | 2 | Rev 2.112, superseded by the Adobe activation at 2.115 |
| `Less` | 2 | 87 glyphs, `Fontself Maker 1.1.1`, no layout tables |
| `Brush-Script-Std`, `Franklin-Gothic-Medium`, `Questa-Grande`, `Simplifica`, `TimeBurner` | 1 each | Singles; Questa Grande has `smcp`+`tnum`, the rest have no `GSUB` |

Conversely **211 Drive desktop files have no twin in `design-tools/owned`**: the whole of `Inter Font Typeface` (148), `Geist Typeface` (20), `Overused Grotesk` (16), `Roboto` (16), `Helvetica Font` (5), `Futura` (3), `TorGroteskMix` (3).

### Same PostScript name in more than one file

461 PostScript names occur in more than one file. The largest cluster is **Inter: every one of its 36 PostScript names exists in six files**, because the Drive folder holds two complete releases (top level = 4.000, `Inter 4.1/` = 4.001) and each release ships the same styles three ways — `extras/otf/`, `extras/ttf/`, and a packed `Inter.ttc`. That is 148 desktop files for what is really one family at two versions. Activating this folder wholesale will produce six competing entries per style in every font menu.

### Same family in OTF and TTF

| Family | Where | Note |
| :--- | :--- | :--- |
| Inter, Inter Display | Drive free | OTF + TTF + TTC, same release |
| Architects | Drive free (TTF) + owned (OTF) | Same glyph set, 76 glyphs |
| Futura | Drive premium | `Futura-CondensedLight.otf` among 18 TTFs |
| Overused Grotesk | Drive free | 16 TTF statics + OTF set + 1 VF |
| Tor Grotesk Mix | Drive paid | 3 static OTF + 1 variable TTF, by design |
| Lato | Drive/owned TTF, Adobe `.w` OTF | Different versions, see below |
| Roboto | Drive TTF 1.000, Adobe `.r` OTF 2.000 | Different versions |
| Geist, Geist Mono | Drive OTF 1.002, Forge TTF 1.700/1.800 | Different versions and axis ranges |

### Multiple `head.fontRevision` per family

Highest revision and where it lives:

| Family | Highest | Held by | All revisions present |
| :--- | :--- | :--- | :--- |
| Inter, Inter Display, Inter Variable | 4.001 | Drive `Inter 4.1/` (and owned, for the VF) | 4.000, 4.001 |
| Lato | 3.100 | Drive free + owned | 3.100, 2.015 (Adobe) |
| Roboto | 2.000 | Adobe `.r` | 2.000, 1.000 (Drive) |
| Geist | 1.800 | Forge | 1.800, 1.002 (Drive) |
| Geist Mono | 1.700 | Forge | 1.700, 1.002 (Drive) |
| Minion Pro | 2.115 | Adobe `.w` | 2.115, 2.112 (owned), 2.108 |
| Futura PT | 1.009 | Adobe `.r` | 1.009, 1.007 (owned) |
| Univers LT Std | 1.029 | Drive premium + owned (52 faces) | 1.029, 1.000 (2 faces) |
| Trade Gothic Next LT Pro | 3.000 | Adobe `.r` | 3.000, 2.000 |
| Sabon LT Pro | 2.000 | Adobe `.r` | 2.000, 1.000 |
| Rockwell Nova | 1.130 | Adobe `.r` | 1.130, 1.100 |
| DIN 2014 | 1.001 | Adobe `.w` | 1.001, 1.000 |
| Helvetica | 1.000 | Drive premium | 1.000, 0.000 |

Pattern: **where a family exists both locally and on Adobe Fonts, the Adobe copy is the newer one** — Roboto, Minion Pro, Futura PT, Acumin Pro. The exceptions run the other way only for Lato (local 3.100 is newer but defective) and Geist (Forge is newer).

## 4. Perso-Arabic capable families

Criterion: more than 20 codepoints present in U+0600–06FF. Ten families in the scanned sources qualify, and **all ten cover the full Persian set** — پ U+067E, چ U+0686, ژ U+0698, ک U+06A9, گ U+06AF, ی U+06CC — plus U+0640 tatweel.

| Family | Source | Arabic cps | Presentation forms | Supp + Ext-A | init/medi/fina | isol | rlig | ccmp | mark | mkmk | curs | Axes |
| :--- | :--- | ---: | ---: | ---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :--- |
| IBM Plex Sans Arabic | Forge | 252 | 336 | 88 | yes | no | yes | yes | yes | yes | no | — (8 statics) |
| Noto Naskh Arabic | Forge | 256 | 772 | 144 | yes | no | yes | yes | yes | yes | no | `wght 400–700` |
| Noto Sans Arabic | Forge | 256 | 772 | 144 | yes | no | yes | yes | yes | yes | no | `wght 100–900`, `wdth 62.5–100` |
| Scheherazade New | Forge | 256 | 31 | 144 | yes | no | yes | yes | yes | yes | no | — (4 statics) |
| SF Arabic | `/Library/Fonts` | 255 | 329 | 113 | yes | no | yes | yes | yes | yes | no | `wght 1–1000`, `opsz 17–80` |
| SF Arabic Rounded | `/Library/Fonts` | 255 | 329 | 113 | yes | no | yes | yes | yes | yes | no | `wght 1–1000`, `opsz 17–80` |
| Markazi Text | `~/Library/Fonts` | 99 | 201 | 0 | yes | no | yes | yes | yes | yes | no | `wght 400–700` |
| Reem Kufi | `~/Library/Fonts` | 118 | 0 | 1 | yes | no | yes | yes | yes | yes | no | `wght 400–700` |
| Qahiri | `~/Library/Fonts` | 100 | 0 | 3 | yes | no | yes | yes | yes | yes | **yes** | — |
| Segoe UI | **Adobe Fonts** (`.w`) | 256 | 319 | 121 | yes | **yes** | yes | yes | yes | yes | no | — (1 style) |

Notes on that table:

- **`isol` absent is correct, not a gap.** A modern OpenType Arabic font leaves isolated forms as the default glyph and only substitutes `init`/`medi`/`fina`; HarfBuzz applies all four in order and falls through. Only Segoe UI declares `isol` explicitly.
- **`cswh` appears in none of them.** Kashida elongation in these fonts is not done through contextual swash. The two mechanisms actually present are the tatweel glyph U+0640 (in all ten) and, on the Apple legacy fonts below, the AAT `just` table.
- **`curs` (cursive attachment) appears only in Qahiri**, which is a Kufi display face where baseline joining is part of the design.
- **Only one Perso-Arabic family comes from Adobe Fonts: Segoe UI**, and only its Regular. Everything else Perso-Arabic is Forge-installed, macOS-bundled, or a loose Google Fonts download. The subscription is contributing essentially nothing to Persian work.

### macOS system Arabic fonts

`fc-list :lang=fa` returns 34 family names and `:lang=ar` returns 49 (both counts include the Arabic- and Devanagari-script aliases macOS registers for the same families, and three hidden UI variants). Parsed from `/System/Library/Fonts` (74 faces, in `system-arabic.tsv`), they split into two groups:

**OpenType-shaped** — work everywhere, including HarfBuzz-based tools (browsers, Figma, Sketch):

| Family | Arabic cps | init/medi/fina | isol | rlig | mark | mkmk | curs | Glyphs |
| :--- | ---: | :---: | :---: | :---: | :---: | :---: | :---: | ---: |
| Arial | 235 | yes | yes | yes | yes | yes | no | 3381 |
| Times New Roman | 235 | yes | yes | yes | yes | yes | no | 3380 |
| Tahoma | 235 | yes | yes | yes | yes | yes | no | 3301 |
| Courier New | 235 | yes | yes | yes | yes | yes | no | 3151 |
| Microsoft Sans Serif | 235 | yes | yes | yes | yes | yes | no | 3053 |
| Arial Unicode MS | 194 | yes | yes | no | yes | no | no | 50377 |
| Noto Nastaliq Urdu | 178 | yes | yes | yes | yes | yes | **yes** | 1138 |
| Damascus (5 styles) | 250 | yes | no | no | AAT | AAT | — | 1637 |
| SF Arabic / Rounded (system copies) | 255 | yes | no | yes | yes | yes | no | 1435 |

**AAT-shaped only** — `morx` + `feat` + `just`, **no `GSUB` and no `GPOS` at all**: Geeza Pro, Al Bayan, Al Nile, Al Tarikh, Baghdad, Beirut, DecoType Naskh, Diwan Kufi, Diwan Thuluth, Farah, Farisi, KufiStandardGK, Mishafi, Mishafi Gold, Muna, Nadeem, Sana, Waseem, DecoType Nastaleeq Urdu.

This distinction is the practical one for Persian typesetting. These fonts shape correctly in CoreText applications — TextEdit, Pages, Safari on macOS, Adobe apps through their own engine — and produce unshaped, disconnected letterforms in anything that only reads OpenType tables. They also carry a `just` table, which is where macOS kashida justification actually comes from: Geeza Pro, Al Bayan, Nadeem and Damascus all have it. No OpenType font in the collection offers an equivalent.

Coverage within the AAT group is thin — Al Bayan 86 Arabic codepoints, Baghdad 88, Nadeem 88, Farah 84, Sana 84 — and several (Al Nile, Al Tarikh, Beirut, Diwan Kufi, Farah, Mishafi Gold, Muna, Sana) cover **only four of the six Persian letters**, missing ک U+06A9 and ی U+06CC. Geeza Pro and Damascus are the two that cover all six with 250 codepoints.

**Recommended Perso-Arabic shortlist for real work**: Noto Sans Arabic (variable, widest coverage), Noto Naskh Arabic (text setting), IBM Plex Sans Arabic (pairs with the Plex Latin already installed), SF Arabic (only for Apple-platform UI), Scheherazade New (scholarly Naskh, 1822 glyphs), Markazi Text and Reem Kufi for display. Avoid the AAT-only Apple fonts in any cross-tool pipeline.

## 5. Monospaced families

Determined from `post.isFixedPitch` and PANOSE proportion 9, widened by name.

| Family | Source | Styles | Format | Axes | Glyphs | Rev | `zero` | `ss01` | `calt` | `liga` |
| :--- | :--- | ---: | :--- | :--- | ---: | :--- | :---: | :---: | :---: | :---: |
| Iosevka | Forge | 54 | TTC | — | 48202 | 34.030 | yes | yes | yes | no |
| Iosevka Fixed | Forge | 54 | TTC | — | 48202 | 34.030 | yes | yes | no | no |
| Iosevka Term | Forge | 54 | TTC | — | 48202 | 34.030 | yes | yes | yes | no |
| IBM Plex Mono Var | Forge | 2 | VF TTF | `wght 100–700` | 1207 | 1.000 | yes | yes | no | no |
| Noto Sans Mono | Forge | 1 | VF TTF | `wght 100–900`, `wdth 62.5–100` | 3911 | 2.013 | yes | no | no | no |
| Hack | Forge | 4 | TTF | — | 1655 | 3.003 | no | no | no | no |
| Geist Mono | Forge | 2 | VF TTF | `wght 100–900` | 1159 | 1.700 | no | yes | no | no |
| Geist Mono | Drive free | 9 + 1 VF | OTF / VF TTF | `wght 100–1100` | 694 | 1.002 | no | yes | no | yes |
| Roboto Mono | Adobe | 10 | OTF | — | 1041 | 2.001 | no | no | no | no |
| Courier Std | Adobe | 4 | OTF | — | 382 | 2.067 | yes | no | no | no |
| Letter Gothic Std | Adobe | 4 | OTF | — | 382 | 2.059 | yes | no | no | no |
| OCR B Std | Adobe | 1 | OTF | — | 267 | 2.053 | yes | no | no | no |
| Server Mono | Drive free + owned | 2 | OTF | — | 421 | 1.000 | no | yes | no | no |
| Symbols Nerd Font (+ Mono) | Forge | 1 each | TTF | — | 10522 | 1.000 | — | — | — | — |
| CODEINK | Drive free + owned | 2 | TTF | — | 112 | 1.000 | — | — | — | — |

Iosevka at 48 202 glyphs with a slashed zero, stylistic sets and contextual alternates is the only mono here with real breadth. Server Mono is the interesting one editorially — a small 421-glyph design that ships its own `.glyphs` and `.ufo` sources, so it can be extended. CODEINK at 112 glyphs is a display face, not a text mono.

## 6. Quality verdict

### Plausibly premium-quality, complete OpenType

Families exposing `smcp` **and** `onum` **and** `tnum` somewhere in the family:

| Family | Where | Styles | Glyphs | c2sc | dlig | frac | sups | ss01 | swsh | mkmk |
| :--- | :--- | ---: | ---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Ivar Text** | Drive paid + owned | 8 | 614 | yes | no | yes | yes | yes | no | yes |
| **Ivar Display / Headline / Fine** | Drive paid + owned | 8 / 8 / 10 | 493 | yes | no | yes | yes | yes | no | yes |
| **Adobe Garamond Pro** | Adobe | 6 | 939 | yes | yes | yes | yes | no | yes | no |
| **Adobe Caslon Pro** | Adobe | 6 | 806 | yes | yes | yes | yes | no | yes | no |
| **Mrs Eaves OT** | Adobe | 4 | 1161 | yes | yes | yes | yes | no | no | no |
| **Baskerville Display PT** | Adobe | 4 | 770 | yes | yes | yes | yes | yes | yes | no |
| **Baskerville Poster PT** | Adobe | 2 | 770 | yes | yes | yes | yes | yes | yes | no |
| **Clarendon Text Pro** | Adobe | 4 | 594 | yes | yes | yes | yes | yes | no | no |
| **Minion Pro** | Adobe + owned | 2 | 1864 | yes | yes | yes | yes | yes | no | no |
| **Mundial** | Adobe | 14 | 745 | yes | no | yes | yes | yes | no | no |
| **TT Commons Pro** | Adobe | 20 | 1548 | yes | yes | yes | yes | yes | no | yes |
| **Montserrat** | Adobe | 5 | 1946 | yes | yes | yes | yes | yes | no | yes |
| **Gill Sans Nova** | Drive premium + owned | 33 | 1167 | yes | no | yes | yes | yes | yes | no |
| **Bembo Std** | Drive premium + owned | 8 | 500 | yes | no | yes | yes | no | no | no |
| **Bauer Bodoni Std** | Drive premium + owned | 8 | 431 | yes | no | yes | yes | no | no | no |
| Calibri, Segoe UI, Noto Sans, Roboto, Roboto Condensed | Adobe / Drive | — | 3249–5392 | yes | mixed | yes | mixed | yes | no | yes |

For **editorial and architectural work specifically** — running text, captions, drawing annotation, tabular schedules — the defensible picks are:

1. **Ivar** (Text for body, Display/Headline for titling, Fine for the largest sizes). A true four-optical-size system, licensed, complete features, `mark`+`mkmk`, at the highest revision available anywhere in the collection.
2. **Adobe Garamond Pro** and **Adobe Caslon Pro** — 939 and 806 glyphs with small caps, both case sets, oldstyle and tabular figures, fractions, superiors, swashes. Subscription-activated, so they travel with the Adobe account rather than the machine.
3. **Minion Pro** — 1864 glyphs including Greek (84) and Cyrillic (102); take the Adobe rev 2.115 copy, not the 2.112 in `design-tools/owned`.
4. **Gill Sans Nova** — the only *local* sans with a complete feature set (33 styles, small caps, both figure sets, Greek and Cyrillic). Caveat: it is a web-kit TTF conversion, so treat it as a working copy rather than a production master.
5. **Mundial** and **TT Commons Pro** from Adobe for contemporary grotesque work — 14 and 20 styles, full figure sets, `ss01`, and `mkmk` on TT Commons.
6. **Meta Pro** for information design — 26 styles, 1205 glyphs, Greek and Cyrillic, `smcp` and `tnum`, though no `onum`.
7. For tabular/annotation duty: **IBM Plex Mono Var** or **Iosevka** (both have `zero`), or **Courier Std** / **Letter Gothic Std** / **OCR B Std** from Adobe for a technical-drawing register.

### Clearly web rips, conversions, or incomplete

Evidence given per item — this is not inference from the folder name alone.

**Zero-revision, no-version-string, no-layout-table files** (the strongest rip signal):

| Family | Files | Location |
| :--- | ---: | :--- |
| Futura-Bold | 1 | Drive `01.Premium Fonts/Futura` |
| Memphis-Bold, Memphis-Light, Memphis-LightItalic | 3 ×2 copies | Drive premium + owned |
| one Helvetica face | 1 | Drive `Helvetica Font` |

**Named-source rips**:

- `01.Premium Fonts/Futura/sharefonts.net.txt` — the download site left its calling card in the folder.
- `01.Premium Fonts/Futura Italicized Cufonfonts/` — 20 files, vendor ID `PfEd` (re-saved in FontForge), **all layout tables stripped**, 235 glyphs.
- `Helvetica Font/helvetica-compressed-5871d14b6903a.otf` and siblings — hex-hash filenames from a download service, no `GSUB`, 229 glyphs.

**Web-font-kit TTF conversions** (10 folders, 226 styles, each with `.eot` + `.woff` + `.woff2` + `stylesheet.css` + `demo.html`): Akzidenz-Grotesk Next, Bauer Bodoni Std, Bembo Std, DIN Schrift, Frutiger LT Std, Frutiger Neue LT W1G, Gill Sans Nova, Gotham, TT Supermolot Neue, Univers LT Std. These render, and several kept their features, but none is the retail desktop build and none should be treated as a licensed production master.

**Incomplete or feature-stripped, all sources**:

| Family | Where | Defect |
| :--- | :--- | :--- |
| Lato 3.100 | Drive free + owned | Romans have empty `GSUB`; italics have empty `GPOS`. Verified with `hb-info`. |
| DIN Schrift | Drive premium + owned | `GSUB` empty, 232 glyphs, `OTF 1.0;PS 001.001;Core 1.0.22` |
| Frutiger LT Std | Drive premium + owned | No `smcp`/`onum`/`tnum`, 263 glyphs |
| Univers LT Std | Drive premium + owned | No `smcp`/`onum`/`tnum`, 255 glyphs |
| Frutiger Neue LT W1G | Drive premium + owned | No `smcp`/`onum`/`tnum` despite 655 glyphs and W1G coverage |
| Avenir LT Pro | Drive premium + owned | No discretionary features at all |
| Futura Std, Memphis LT Std | Drive premium + owned | No discretionary features, 253 glyphs |
| Arnold | owned | No `GSUB`, 415 glyphs |
| Berlin Sans FB, Berlin Sans FB Demi | owned | No layout tables |
| Franklin Gothic Medium | owned | No layout tables |
| Bradley Hand ITC | Drive free + owned | No layout tables |
| Less | owned | 87 glyphs, `Fontself Maker 1.1.1` |
| Simplifica, TimeBurner | owned | No `GSUB` |
| Architects, CODEINK | Drive free + owned | 76 and 112 glyphs — display faces, not typefaces |
| Futura *BT* family group | Drive premium + owned | 260 glyphs, 1993 `mfgpctt` version strings, no layout tables |
| Futura LT | Drive premium + owned | 40 files, rev 6.000, `PfEd`, no layout tables |
| Overused Grotesk | Drive free + owned | `head.fontRevision` 0.005 — pre-release version numbering |
| Mrs Eaves (non-OT) | Adobe | No layout tables at all; the OT companion `Mrs Eaves OT` is the one to use |
| Adobe Handwriting, Carbon | Adobe | No `GSUB` |

**The Futura situation deserves a single line of summary.** Across Drive, `design-tools/owned` and Adobe there are **16 distinct "Futura" families in 100 files** — Futura, Futura-Black, Futura-Bold, Futura LT, Futura LT Pro, Futura PT, Futura Std, and nine Bitstream `Futura * BT` variants — at four different provenances, of which only **Futura PT** (Adobe, rev 1.009, 636 glyphs, Cyrillic 175, `tnum`) is a legitimate current release. Everything else is a 1990s Bitstream conversion, a FontForge re-save, or a download-site file.

## 7. Adobe Fonts activations currently synced

The manifest is `~/Library/Application Support/Adobe/CoreSync/plugins/livetype/.c/entitlements.xml` (416 KB, last written 2026-09-11 04:46). It holds **632 `<font>` entries** and splits cleanly by `<owner>`:

| Owner | Families | Faces | `installState` | On-disk location |
| :--- | ---: | ---: | :--- | :--- |
| Adobe Fonts | **55** | **318** | 225 `OS`, 93 `CC` | `.r/` (225 `.otf`), `.w/` (93 `.otf`) |
| Self | 26 | 314 | empty | `.e/` (318 opaque blobs) |

The counts reconcile exactly: 225 files in `.r` + 93 in `.w` = 318 = the Adobe Fonts entry count. The `.e` directory holds 318 non-font binaries with random headers — the encrypted counterparts. `.t`, `.u` and `.x` are empty; `GudeLivetype/` holds only `sqliteResumeTransfer.db`; `entitlements-downloading.xml` is zero bytes, so no sync is in flight.

The 26 "Self" families are the user's own fonts registered with Creative Cloud for sync — Akzidenz-Grotesk Next, Avenir LT Pro, Bauer Bodoni Std, Bembo Std, DIN Schrift, Frutiger LT Std, Frutiger Neue LT W1G, Geist Mono, Gotham, Inter, Inter Display, Ivar ×4, Memphis ×6, Meta Pro, Server Mono, Swedish Gothic, TT Supermolot Neue, Univers LT Std. That set is the `design-tools/owned` library seen from the Adobe side, which explains the two Acumin Pro files that hash identically across both.

### The 55 Adobe Fonts families, with face counts

Acumin Pro (2), Adobe Caslon Pro (6), Adobe Garamond Pro (6), Adobe Handwriting (1), Arimo (2), Barlow (3), Baskerville Display PT (4), Baskerville Poster PT (2), BookmanJFPro (2), Calibri (2), Carbon (1), Clarendon Text Pro (4), Courier Std (4), DIN 2014 (5), Europa-Bold (1), Europa-Regular (1), Franklin Gothic ATF (4), Futura PT (14), Grotesque Disp MT Std (1), ITC Avant Garde Gothic Pro (3), ITC Franklin Gothic LT Pro (6), ITCFranklinGothic LT Pro (6), Lato (1), Letter Gothic Std (4), Minion Pro (2), Montserrat (5), Mrs Eaves (5), Mrs Eaves OT (4), Mundial (14), Myriad Pro (1), Neo Sans W1G (12), Neue Aachen Pro (3), Neue Haas Grotesk Display Pro (16), Neue Haas Grotesk Text Pro (6), Neue Haas Unica W1G (18), Noto Sans (2), OCR B Std (1), Open Sans (5), Oswald (1), Poppins (4), Purista (1), Roboto (12), Roboto Condensed (6), Roboto Mono (10), Roboto Slab (4), Rockwell Nova (14), Sabon LT Pro (4), Segoe UI (1), Swiss721 BT (1), TT Commons Pro (20), TT Travels Next (20), Trade Gothic Next LT Pro (17), Trade Gothic Next SR Pro (9), Trajan Pro 3 (1), Unitext (14).

Notable within that set:

- **Neue Haas Grotesk** (Display Pro 16 + Text Pro 6) and **Neue Haas Unica W1G** (18 styles, Greek 75, Cyrillic 122, `smcp` + `tnum`) are the licensed answer to every Helvetica rip sitting in the Drive premium folder. Use these instead.
- **Trade Gothic Next** (LT Pro 17 + SR Pro 9) and **Franklin Gothic ATF** (4) are the licensed answer to the Franklin Gothic single in `owned`.
- **Futura PT** (14, rev 1.009) supersedes all thirteen local Futura variants.
- **Neo Sans W1G** (12, Greek 75, Cyrillic 126, `onum` + `tnum`) is the widest-coverage grotesque in the subscription.
- **Rockwell Nova** (14 styles, Greek 71, Cyrillic 104) covers the slab register that the Memphis rips were standing in for.
- **Segoe UI** is the only Perso-Arabic face in the whole subscription, and only its Regular is activated.
- Two families are activated in both a legacy and an OpenType cut — **Mrs Eaves** (5, no layout tables) and **Mrs Eaves OT** (4, full features). Only the OT one is worth having active.
- Two spellings of the same family are both activated — **ITC Franklin Gothic LT Pro** (388 glyphs, no features) and **ITCFranklinGothic LT Pro** (527 glyphs, `smcp` + `onum`). They will collide in font menus; the second is the better file.

## 8. Typeface Beta

`~/Library/Containers/com.criminalbird.typeface.beta/Data/Library/Application Support/` holds no bookmarks file or folder list. State lives in a 23 MB Realm database, `default.realm`, alongside `license.typeface-license`, a `backups/backup-26-09-11-161112.typeface-backup`, `layouts/default-text-layout.txt`, and a `downloads/Google/` directory that is present but empty.

Extracting path strings from the Realm file yields **8631 distinct font paths**, spanning:

- `/System/Library/Fonts` and `/System/Library/Fonts/Supplemental`
- `/Library/Fonts`
- `/Library/AssetsV2/com_apple_MobileAsset_Font8/…` (on-demand system font assets, e.g. PingFang)
- `/Users/bardiasamiee/Library/Fonts/HomeManager/**` — the Forge tree, including the full IBM Plex opentype set and the Nerd Fonts symbols
- **`/Library/Fonts/Nix Fonts/<nix-store-hash>/share/fonts/...`** — a *stale* root. That directory does not exist on disk today (`ls` returns no such file), so these rows are leftovers from a previous home-manager generation that installed system-wide instead of per-user.

So Typeface Beta has indexed the system and Forge trees, carries stale entries from an earlier Nix font layout, and its `downloads/Google/` folder is empty — it has not been used to pull Google Fonts. It appears not to have imported the Drive library or `design-tools/fonts/owned` as custom folders.

## 9. Observations worth acting on before a tournament

1. **`design-tools/fonts/owned` and the Drive library are the same collection twice.** 393 of 425 files are byte-identical. Pick one as the master; the 32 files unique to `owned` and the 211 unique to Drive are the only content that would be lost by collapsing them.
2. **Seven in ten files in the "premium" tier are not desktop fonts.** 226 of 318 files in `01.Premium Fonts` come out of `@font-face` kits, and about 45 more are traceable rips. Ranking these against Ivar or Adobe Garamond compares different kinds of object.
3. **Lato is broken in the copies you hold.** Either use the Adobe Fonts activation (rev 2.015) or re-download 3.100 from the foundry.
4. **Inter is installed six times over.** Two releases × three formats. Choose 4.001 and one format.
5. **The Forge tree is invisible to `fontconfig` but live in CoreText.** Any tooling you build on `fc-list` will silently miss Noto Sans Arabic, Noto Naskh Arabic, Scheherazade New, IBM Plex Sans Arabic, Iosevka and Hack.
6. **Persian coverage is thin and entirely outside the paid tiers.** Nine usable families, all free or system, plus Segoe UI Regular from Adobe. No paid or premium Latin family in this collection has any Arabic coverage at all.
7. **The Apple Arabic fonts are AAT-only.** Geeza Pro, Al Bayan, Nadeem and the rest have no `GSUB`/`GPOS`. They are fine in Adobe and Apple applications and wrong everywhere else — and they are the only fonts here with a `just` table for kashida.

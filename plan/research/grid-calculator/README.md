# Grid Calculator Publishing Edition, research notes

Research date: 2026-09-11 (fetches ran 2026-09-12 00:20 to 00:30 UTC). Nothing was purchased, no installer ran, no binary executed, no license entered, InDesign was not touched. The `.pkg` was expanded with `pkgutil --expand` and its payloads unpacked with `cpio` into the session scratchpad, then copied here.

The premise "commercial macOS app" needs one correction: Grid Calculator Publishing Edition (GCPE) is a native Adobe InDesign plug-in pair plus a small AppKit activator, not a standalone application. The vendor's standalone product, Grid Calculator Standalone Edition (SE), is unreleased.

## Sources

| Index | URL | What it is | Version or date on page | Fetched | Saved as |
| :---: | :-- | :-- | :-- | :-- | :-- |
| 01 | https://designersbookshop.com/ | Vendor home, Astro site marked "being updated" | Latest update rows dated 2026-04-03 | 2026-09-12 | not saved (text in this README) |
| 02 | https://designersbookshop.com/products/grid-calculator-pe/ | Product page, feature copy, FAQ, pricing, download button | "30-day demo. Compatible with Adobe InDesign CC 2026." | 2026-09-12 | `docs/site-2026/products_grid-calculator-pe.html`, `docs/site-2026/product-page.txt` |
| 03 | https://designersbookshop.com/updates/grid-calculator-pe/ | Release notes for GCPE 1.0 to 1.4 | Newest entry 2026-04-03 v1.4 | 2026-09-12 | `docs/site-2026/updates_grid-calculator-pe.html`, `docs/site-2026/updates.txt` |
| 04 | https://designersbookshop.com/kb/grid-calculator-pe/getting-started/ | Only KB article for PE on the new site | undated | 2026-09-12 | `docs/site-2026/kb_grid-calculator-pe_getting-started.html`, `kb-getting-started.txt` |
| 05 | https://designersbookshop.com/kb/grid-calculator-se/about-standalone-edition/ | Standalone Edition description and PE versus SE table | undated | 2026-09-12 | `docs/site-2026/kb_grid-calculator-se_about-standalone-edition.html`, `kb-standalone-edition.txt` |
| 06 | https://designersbookshop.com/about/ | Vendor history | undated | 2026-09-12 | `docs/site-2026/about.html`, `about.txt` |
| 07 | https://designersbookshop.com/products/grid-calculator-pe/layout-wizard/ | Web Layout Wizard that exports a plug-in preset | page ships inline JS | 2026-09-12 | `docs/site-2026/products_grid-calculator-pe_layout-wizard.html`, `layout-wizard.txt`, `scripts/layout-wizard-client.js` |
| 08 | https://layout-wizard-calculator.noisy-art-eee3.workers.dev | Cloudflare Worker the wizard POSTs to; answers 403 to a bare `curl`, so it was exercised through the page in a headless browser only | presets it returns say `Version: 1.3;` | 2026-09-12 | `data/layout-wizard-samples/*` |
| 09 | https://designersbookshop.s3.amazonaws.com/products/grid_calculator_pe/1.4/cc26/GCPE_CC26_Mac_14.pkg.zip | The public 30-day demo installer (same file as the licensed build; the license is entered after install) | 2,796,709 bytes, zip entry dated 2026-05-27, sha256 `df4c27aa09e9b5b00478f7cf6469306081201095548cf007713f7d2e2e2a6e55` | 2026-09-12 | `bundle/GCPE_CC26_Mac_14.pkg.zip` |
| 10 | https://designersbookshop.s3.us-east-1.amazonaws.com/products/updates/grid_calculator_pe/xml/gcpe.xml | Update feed the plug-in polls (URL found in the UI binary) | lists cc20 to cc26, Mac and windows64 enclosures | 2026-09-12 | `data/update-feed-gcpe.xml` |
| 11 | https://05xg1fy0se.execute-api.us-east-1.amazonaws.com/prod/announcement | Announcement and server status API behind the S3 status page | announcement dated 2026-03-17, `serverStatus: offline`, download link to 1.3 | 2026-09-12 | `data/announcement-feed.json` |
| 12 | https://designersbookshop.s3.amazonaws.com/products/updates/grid_calculator_pe/index.html | S3 status page (menu item "Updates and Server Status") | shell page, content from source 11 | 2026-09-12 | `docs/site-2026/s3-status-updates.html` |
| 13 | https://designersbookshop.s3.amazonaws.com/products/knowledgebase/grid_calculator_pe/ (`index.html`, `content/config.json`, `content/articles/introduction.md`) | S3 knowledge base (menu item "Knowledge Base") | one article, placeholder text about a CSS grid calculator, unrelated to the real product | 2026-09-12 | `docs/site-2026/s3-knowledgebase-index.html`, `s3-kb-config.json`, `s3-kb-introduction.md` |
| 14 | https://web.archive.org/web/20220516*/https://www.designersbookshop.com/knowledge-base/user-manual/* | The Pro Edition user manual, ten articles. The live URLs now redirect to the home page, so the Wayback Machine snapshots of 2022-05-16 are the only copy | manual written for v4.0 to 4.2 (2016 to 2019 era) | 2026-09-12 | `docs/manual-2022-wayback/knowledge-base_user-manual_*.html` and `.txt` |
| 15 | Wayback snapshots of `grid-calculator-pro-edition.html` (2023-02-03), `-knowledge-base.html` (2023-09-29), `-layout-wizard.html` (2022-01-16), `-questions.html` (2023-03-30), `-videos.html` (2023-03-30) | Old product, KB index, wizard, Q and A, video pages | as dated | 2026-09-12 | `docs/manual-2022-wayback/grid-calculator-pro-edition*.html` and `.txt` |
| 16 | `Uninstallation_Guide.pdf` inside payload `03-GC_Plugins.pkg` | Vendor uninstall guide | written for 2025, "applies to other versions" | from source 09 | `docs/Uninstallation_Guide.pdf` |
| 17 | `Resources/en.lproj/{welcome,readme,license}.rtf` and `Distribution` inside the `.pkg` | Installer welcome, read me with "Power Tips", EULA, distribution script | installer title "Grid Calculator PE 2026 (1.4)" | from source 09 | `docs/installer/*.txt`, `docs/installer/Distribution.xml`, `docs/installer/postinstall.sh` |
| 18 | https://designersbookshop.onfastspring.com/gcpe-indesign-creative-cloud-subscription and `...-yearly-subscription` | Checkout links on the product page | not opened | not fetched | none |
| 19 | https://www.linkedin.com/learning/designing-with-grids-in-indesign-2/grid-calculator-pro | Third-party course chapter the product page links | not fetched | none | none |
| 20 | https://appsource.microsoft.com/en-us/product/web-apps/designersbookshop.gridcalculator-pe | Old Microsoft AppSource listing seen in search results, claims "Windows and Mac" | unverified, not fetched | none | none |

## Product

| Fact | Value | Source |
| :-- | :-- | :-- |
| Vendor | Designers Bookshop, one-person studio of Abraham Georges, Stockholm, Sweden, founded 2008; the plug-in began as his Berghs School of Communication graduation project and launched 2009 | 06, code signing identity "Abraham Georges (5ZCTQ6T4SQ)" |
| Product name history | Grid Calculator Pro Edition (CS3 through CC 2019, versions 3.x to 5.1) became Grid Calculator Publishing Edition (versions 1.0 to 1.4); the UI binary still carries Pro Edition install paths for CS3 to CC 2019 and PE paths for 2020, 2022, 2026 | 02, 06, 15, `data/strings-GridCalculatorUI-plugin.txt` |
| Editions today | Publishing Edition (InDesign plug-in, available); Standalone Edition (desktop app, in development, exports to InDesign and Affinity Publisher, Windows planned); Merge CRM (unrelated, unreleased). There is no "Pro" or "Web" edition on sale. The product page calls the wizard page "Layout Wizard" and the S3 KB is a stub | 01, 05 |
| PE versus SE | PE requires InDesign, no Affinity, macOS. SE requires no InDesign, Affinity yes, macOS with Windows planned | 05 |
| Price | Monthly USD 15, yearly USD 175, FastSpring checkout, cancel anytime, 30-day refund policy, one license activates on 2 computers and runs on 1 at a time | 02 |
| Demo | 30-day demo from the same installer; demo activates with name and email, license with name, email and key. Online activation is mandatory | 02, 04, 17 |
| Current version | 1.4, released 2026-04-03 ("Layout Wizard & Presets"); bundle `CFBundleShortVersionString` 1.4; installer signed 2026-05-27 | 03, 09 |
| Changelog | 2026-04-03 v1.4 Layout Wizard button and web presets; 2026-03-09 v1.3 new activation server ("we aim to have everyone starting from version 2027 on the new server"); 2025-12-16 v1.2 "InDesign 2026 — Mac support released. Grid Calculator PE is now fully compatible with Adobe InDesign 2026 on macOS. This version no longer requires InDesign 2025 to be installed alongside it."; 2025-11-18 v1.2 InDesign 2026 beta notice (InDesign 2025 had to stay installed); 2025-06-10 v1.2 preset overwrite dialog and three menu items; 2025-06-05 v1.1 preset crash fix; 2025-05-26 v1.0 macOS 15.4 licensing fix | 03 |
| InDesign support | "Adobe InDesign CC 2026 and future versions (Mac)"; each InDesign year needs its own plug-in build; the installer refuses to run unless `/Applications/Adobe InDesign 2026/Adobe InDesign 2026.app` exists | 02, 17 |
| Platform | FAQ: macOS only. Plug-ins declare `LSMinimumSystemVersion` 13.0, the activator 12.4, all universal arm64 and x86_64. The update feed also lists `windows64` enclosures for cc20 to cc26, so a Windows build may exist or have existed; unverified | 02, 09, 10 |
| Server state | The announcement API reported `serverStatus: offline` with a 2026-03-17 note about moving servers and building a standalone app for Affinity and InDesign | 11 |
| Presets location | `~/Documents/Grid Calculator Publishing Edition/Presets/<client folder>/<name>.txt` | 02, 16, UI strings `GetPresetsFolder` |

## Bundle

The download is a zip holding one flat `.pkg` built with the Packages tool. `Distribution` declares `hostArchitectures="arm64,x86_64"`, `customize="never"`, and three component packages installed as root.

| Component | Identifier | Installs | Notes |
| :-- | :-- | :-- | :-- |
| `01-GC_Activator.pkg` | `com.designersbookshop.gridcalculatorpe.activator.2026` | `/Applications/Adobe InDesign 2026/Plug-Ins/Grid Calculator Publishing Edition/Activator 2026.app` | AppKit app, 21 files |
| `02-GC_Registration.pkg` | `com.designersbookshop.gridcalculatorpe.registration.2026` | nothing (payload 0 KB), runs `postinstall` | script writes `/Applications/.GridCalculator/` and `/Library/Application Support/.dbs/.framework_data` (an "encrypted UUID" that replaces the MAC address as machine id), runs `RunBundleHelper Registration.bundle`, logs to `/tmp/gc_encrypted_install.log` |
| `03-GC_Plugins.pkg` | `com.designersbookshop.gridcalculatorpe.2026` | the two `.InDesignPlugin` bundles and `Uninstallation_Guide.pdf` in the same folder | 106 files |

| Bundle | Value |
| :-- | :-- |
| `GridCalculator.InDesignPlugin` id | `com.DesignersBookshop.GridCalculator`, `CFBundlePackageType` `InD3`, `CFBundleSignature` `InDn`, version 1.4 / 1.4, min macOS 13.0, SDK macosx15.2, Xcode 16.2 |
| `GridCalculatorUI.InDesignPlugin` id | `com.DesignersBookshop.GridCalculatorUI`, same versions and toolchain |
| `Activator 2026.app` id | `com.designersbookshop.Activator-2026`, version 1.0 (1), min macOS 12.4, `NSMainNibFile` `MainMenuCC17`, `NSPrincipalClass` `NSApplication` |
| Architectures | Mach-O universal x86_64 and arm64 for all three binaries and for `RunBundleHelper` |
| Signing | `Developer ID Application: Abraham Georges (5ZCTQ6T4SQ)`, hardened runtime flag `0x10000(runtime)`, timestamp 2026-05-27 05:58 |
| Entitlements | `codesign -d --entitlements :-` printed none for all three; no sandbox, no apple-events entitlement |
| `Info.plist` keys absent | no `NSAppleEventsUsageDescription`, no `OSAScriptingDefinition`, no `CFBundleURLTypes`, no `CFBundleDocumentTypes` |
| Linked libraries, model plug-in | `@rpath/InDesignModel.framework`, Foundation, libobjc, libc++, libSystem |
| Linked libraries, UI plug-in | `@rpath/InDesignModelAndUI.framework`, `ASLSupportLib.dylib`, `WidgetBinLib.dylib`, `DV_WidgetBinLib.dylib`, `TextPanelLib.dylib`, `PMRuntime.dylib`, `PublicLib.dylib`, `DataBaseLib.dylib`, `ObjectModelLib.dylib`, Cocoa, AppKit, CoreFoundation, CoreServices, IOKit, Foundation |
| Linked libraries, activator | Cocoa, AppKit, Foundation, CoreFoundation, IOKit, libc++ |
| UI toolkit | InDesign SDK C++ plug-in (ODFRC compiled resources `idrc_VIEW`, `idrc_MENR`, `idrc_ACTD`, `idrc_PMST`, `idrc_LOCR`, `idrc_CLST`, `idrc_FACT`, `idrc_PVER`, `idrc_PLUG`, `idrc_PNGA`, `idrc_SCML`, `idrc_IDPL`, `idrc_ITAG`) with InDesign's own widget library; the activator is plain AppKit with a nib. Not Electron, Catalyst, SwiftUI, Xojo or Qt |
| Resources of interest | `Uninstallation_Guide.pdf` (one page), `idrc_PMST/351.idrc` holds the page-size dropdown entries, `idrc_PNGA/1310.idrc` a PNG icon. No `.sdef`, `.scpt`, `.jsx`, `.idjs`, `.js`, `.json`, `.csv`, `.html`, `.lproj` beyond `English.lproj` in the activator |
| Model plug-in classes | `GCPreferences`, `GCDocPreferences`, `GCSpreadPreferences` implementing `IGCPreferences`, `IGCDocPreferences`, `IGCSpreadPreferences` (the only 28 strings in that binary), so per-document and per-spread settings persist inside the `.indd` as plug-in data |
| Diagnostics | the UI binary still contains a debug assertion path `/Applications/Xcode.app/.../MacOSX15.2.sdk/usr/include/c++/v1/vector:1400` |

Command output that decided the above (abridged):

```text
$ file GridCalculatorUI
Mach-O universal binary with 2 architectures: [x86_64 ... dynamically linked shared library] [arm64 ... dynamically linked shared library]
$ codesign -dv --verbose=2 GridCalculator.InDesignPlugin/Versions/A/GridCalculator
Identifier=com.DesignersBookshop.GridCalculator
Format=bundle with Mach-O universal (x86_64 arm64)
CodeDirectory v=20500 size=784 flags=0x10000(runtime) hashes=17+3 location=embedded
Authority=Developer ID Application: Abraham Georges (5ZCTQ6T4SQ)
Timestamp=May 27, 2026 at 05:58:39
```

## InDesign integration

GCPE drives InDesign from inside the process as a native SDK plug-in. There is no scripting bridge of any kind.

| Probe | Result |
| :-- | :-- |
| `strings -n 6` grep for `osascript`, `tell application`, `ExtendScript`, `.jsx`, `idjs`, `UXP`, `doScript`, `app.activeDocument`, `marginPreferences`, `gridPreferences`, `baselineDivision`, `baselineStart`, `baselineFrameGridOption`, `columnGutter` | zero hits in `GridCalculator`, `GridCalculatorUI`, `Activator 2026`, `RunBundleHelper` |
| `com.adobe` | zero hits; "Adobe InDesign ..." appears only as product names in dialogs and install paths |
| `open "http..."` | 12 shell `open` calls to vendor URLs for menu items (updates, KB, layout wizard, purchase pages) |
| Network endpoints in the binary | `https://gcpe-activation-proxy.noisy-art-eee3.workers.dev` (activation), `https://designersbookshop.s3.us-east-1.amazonaws.com/products/updates/grid_calculator_pe/xml/gcpe.xml` (update check), `/Applications/.GridCalculator/com.apple.agdbdactivation18.0.plist` (activation state file with `INVALIDATED` key) |
| Embedded JavaScript | none. `scripts/` holds only the vendor's public web page script |

What the plug-in writes into the document, as the manual and the strings state:

- Document settings: width, height, facing pages, bleed and slug (preset keys `Bleed (Top)` ... `Slug (Outside)`), unit.
- Document grid (InDesign "Grids" preferences) as the layout's unit lattice: horizontal subdivision width and vertical subdivision height, and the baseline grid (leading). The manual explains that the plug-in "only uses native settings already existing within InDesign", so files open without the plug-in, and that exporting to IDML or INX clears the plug-in's own data so the layout can no longer be edited by the plug-in.
- Master spreads named `GC-1` to `GC-5` (five is the hard maximum, string "You have already created 5 masters for this document which is the maximum amount possible."), each with its own margins, columns, rows, image-lines; masters and layers carry a `[GC]` prefix that must not be renamed.
- Layers such as `[GC-1] Columns`, `[GC-1] Main Columns`, `[GC-1] Subcolumns`, `[GC-1] Secondary Columns`, `[GC-1] Image-lines`, `[GC] Rows`, `[GC] Secondary Rows`, `[GC] Subcolumns`, `[GC] Images`, `[GC] Text`, `[GC] Break Vertical`, `[GC] Break Horizontal`, with ruler guides for rows, subcolumns, secondary columns, image-lines and the type-area grid, plus optional drawn vertical lines with a chosen stroke.
- Paragraph and character styles `[GC] Body Copy #1` (created on first run and kept synchronized with the leading), style groups such as Body Copy, Caption | Info, Headline, with leading options that are multiples of the baseline or of the subdivision.
- Presets as plain `Key: value;` text files, one block per master, separated by the literal line `/*********** DO NOT REMOVE OR CHANGE THIS SEPARATOR ***********/`. The full key list sits at lines 388 to 553 of `data/strings-GridCalculatorUI-plugin.txt` and three real examples in `data/layout-wizard-samples/`.

Excerpt of a preset the vendor's wizard produced for A4 (`data/layout-wizard-samples/a4-preset.txt`):

```text
Type: Grid Calculator Publishing Edition;
Version: 1.3;
Document Setup: Smart;
Facing Pages: true;
Unit: Millimeters;
Width 4: 210;
H. Module 2: 42;
H. Module 3: 5;
H. Subdiv 2: 1;
H. Subdiv 3: 5;
Height 4: 297;
V. Module 2: 70;
V. Module 3: 4.243;
V. Subdiv 2: 1;
V. Subdiv 3: 4.243;
Leading: 4.243;
Style Leading: 12.027 pt;
[GC-1]
Top margin (lines): 5;
Bottom margin (lines): 6;
Inside margin (lines): 4;
Outside margin (lines): 3;
Main Columns: 4 columns (8 lines); gutter: 1 line;
Main Rows: 6 rows (9 lines); gutter: 1 line;
```

Reading of the keys: `H. Module 2` is the module count across the width, `H. Module 3` the module size in the unit, `H. Subdiv 2` subdivisions per module, `H. Subdiv 3` the subdivision size; the same four for the vertical axis, where the subdivision size equals the leading (4.243 mm = 12.027 pt). Margins, column widths and gutters are integers in subdivision units ("lines").

## Grid model as documented

Vocabulary the vendor uses:

| Term | Meaning in the manual and strings |
| :-- | :-- |
| Document grid | InDesign's document grid, set by the plug-in; horizontal subdivision (`H. Subdiv`) is the grid width, vertical subdivision (`V. Subdiv`) is the leading |
| Modules and subdivisions | "different levels of slicing": modules are the primary division of width or height, subdivisions slice each module; entering 5 modules and 7 subdivisions equals 1 module and 35 subdivisions except that modules have a maximum output value |
| Line mode versus Value mode | Line mode: margins expressed in document-grid lines. Value mode (checkbox "Value"): margins as entered values. Value mode exists in Quick mode only; vertical Value mode exists on master GC-1 only because the document has one baseline grid |
| Fit Leading | Quick mode checkbox. Checked: the entered leading is recalculated to the closest value that fits the page height an integer number of times. Unchecked: the exact leading is applied and the bottom margin absorbs the remainder so both vertical margins move in leading increments |
| Subdivision (Quick mode) | divides or multiplies the applied leading up to 5 times (the manual example uses 4 pt leading with "x3" to get 12.027 pt baselines over a 4.009 pt fine grid) |
| Grid Width (Quick mode, since 4.0) | desired horizontal grid unit; "Calculate" applies the closest fitting division of the width; with Fit Leading off and leading set to that value the grid is square |
| Apply Square Grid | takes the applied leading and applies it horizontally, expanding the document width to make the grid square |
| Proportions | enter a target ratio (example 1.618) and pick which of width or height changes; the panel also displays Doc. Prop. (page ratio) and Type Area (type area ratio, width and height) |
| Based On | takes the leading from an existing paragraph style and fits it |
| Quick mode | default; custom margins allowed, can "break the document grid" and set a type-area grid (Custom Setup with "Fit to margins" or "Fit to page"), custom columns and rows with numeric width and gutter; only mode that supports Alternate Layout masters |
| Modular mode | tied to the document grid; horizontal and vertical grids independent ("non-proportional"); Calculate finds the closest module size to a desired grid width and height; margins set freely in grid lines |
| Smart mode | tied to the document grid and synchronized with Modular; enter column count, row count, column width, row height and gutter in lines, or enter desired margins and browse the combinations that fit; margins are "whatever areas are left over"; secondary column filter; errors "Gutter too large for this number of columns/rows" |
| Columns, Subcolumns, Secondary Columns | main columns divide the type area; subcolumns split main columns; secondary columns are a second column layout on guides; same three levels for rows |
| Lock | keep column and row settings while moving the type area by editing one margin |
| Image-lines | guides at the top of lowercase x (x-height) or capital H (H-height) of Body Copy #1 above each baseline, or a custom height; choosing image-lines snaps the top margin so the first line's x or H top sits on the margin; in Quick mode a feature sets the font size so the image-line height equals the grid width |
| Indent, Exdent, Tabs | based on Em (font size), H. Subdiv (grid width) or custom; "Center Tabs: 4 Ems" appears as a default |
| Units | Millimeters, Inches, Points and Pixels; leading always displays in points; the leading field accepts a unit suffix such as "3 mm" |
| Page sizes | dropdowns "DIN sizes.", "ANSI and Arch sizes." and "Phone, Web and Tablet sizes."; the ANSI, Arch and Digital entries are stored as strings (see `data/page-sizes.csv`); the DIN entries are not stored as strings and are presumably generated |
| Limits | width between 0.0139 and 216 inches (InDesign's own limits), leading must be smaller than the page height, five masters per document, Value mode presets do not store rows for GC-2 to GC-5 correctly (manual, 4.2) |

Formulas the vendor states or demonstrates:

```text
Fit Leading (Quick mode, manual example A4, 12 pt)
  H_pt      = 297 mm * 72 / 25.4 = 841.890 pt
  N         = round(H_pt / L_desired) = round(70.157) = 70
  L_applied = H_pt / N = 12.027 pt            (manual: "12,027 pt")

Subdivision x3 with 4 pt entered
  L_applied = 841.890 / round(841.890 / 4) = 841.890 / 210 = 4.009 pt
  baseline  = 3 * 4.009 = 12.027 pt           (manual: "4,009 x 3 = 12,027 pt")

Wizard client hint (scripts/layout-wizard-client.js, lines 29 to 49)
  docHpt  = round(docHmm * 2.834645 * 1000) / 1000
  N       = round(docHpt / desired)
  applied = round(docHpt / N * 1000) / 1000
```

Behaviour observed from three wizard runs (the worker is closed source; these are readings of its output, in `data/layout-wizard-samples/`):

| Run | Inputs | Output |
| :-- | :-- | :-- |
| a4 | 210 x 297 mm, inside 20, outside 20, 4 columns, gutter 5, top 20, bottom 20, 6 rows, row gutter 5, 12 pt, spread | horizontal unit 5 mm (42 modules of 5 mm), vertical unit 297/70 = 4.243 mm; inside 4, outside 3, column 8, gutter 1; top 5, bottom 6, row 9, gutter 1 (all in units) |
| a4-odd | same page, inside 17, outside 23, 5 columns, gutter 4, top 15, bottom 25, 7 rows, row gutter 4, 11 pt | horizontal unit 1 mm; inside 17, outside 22, column 31, gutter 4; vertical unit 297/77 = 3.857 mm (10.934 pt); top 4, bottom 11, row 8, gutter 1 |
| letter-in | 8.5 x 11 in, 0.75 in margins, 3 columns, gutter 0.1667, 14 pt, single page | the worker treated the inch values as millimetres (2 baselines, unit 0.0001 in, preset says `Unit: Millimeters`); the inch path is broken as of 2026-09-12 |

Readings: the horizontal unit is the greatest common divisor of width, inside, outside and gutter at the entered precision; the vertical unit comes from the leading fit; gutters round to the nearest unit; column width rounds to the nearest unit and the outside margin takes the difference (7.75 became 8, 30.8 became 31, outside shrank from 20 to 15 and 23 to 22); row height is the largest count that fits and the bottom margin takes the difference (9.17 became 9, 8.71 became 8, bottom grew from 5 to 6 and 6 to 11). The wizard page itself says "finds the largest value that divides everything evenly, your grid unit. If a column or row height isn't a whole multiple, the margins are adjusted automatically to fit the grid perfectly."

Where the vendor is silent: there are no margin presets (no Van de Graaf, Tschichold, or golden-section canon; the only ratio inputs are the document and type-area proportion fields, and the margins article remarks that classic typography doubled the outer and bottom margins), no formula for x-height or H-height (the plug-in reads the font's metrics for Body Copy #1 and the manual tells users to outline a glyph to get a custom value), no rule for tie-breaking when two column combinations fit equally, no description of how "closest fitting" chooses between rounding up and down, no DPI setting (points at 72 per inch, `PT_PER_MM = 2.834645` in the client script), and no statement of the module maximum.

## What we build ourselves

Variables: `W`, `H` page width and height in points; `L` desired leading in points; `u_h`, `u_v` horizontal and vertical grid units in points; `m_t`, `m_b`, `m_i`, `m_o` top, bottom, inside, outside margins; `c`, `r` column and row counts; `g_c`, `g_r` gutters; `x`, `h_cap` x-height and cap height in points for the body font at size `s`. `round` is round half away from zero, `floor` and `ceil` as usual. Each formula is tagged **confirmed** (vendor documentation or vendor output reproduces it) or **derived** (our reading of standard practice).

Baseline fit, Fit Leading on (**confirmed**, manual A4 example and wizard client):

```text
N          = round(H / L)                 baseline count over the full page
L_fit      = H / N                        applied leading, page height divides exactly
u_v        = L_fit
```

Baseline fit inside the type area, vertical Value mode (**confirmed** in words: "Leading will be recalculated to fit inside the type area"; formula **derived**):

```text
T          = H - m_t - m_b                type area height with entered margins
N_t        = round(T / L)
L_fit      = T / N_t
baselineStart = m_t                       grid starts at the top margin, relative to page top
```

Exact leading, Fit Leading off (**confirmed** in words: "the bottom margin will adjust so that both the top and bottom margin jumps in increment of the entered leading value"; formula **derived**):

```text
L_fit      = L
k_t        = round(m_t / L)               top margin in lines
N_avail    = floor((H - k_t * L) / L)     lines below the top margin
k_b        = N_avail - k_rows_used        bottom margin in lines, absorbs the remainder
m_t'       = k_t * L
m_b'       = H - m_t' - (N_avail - k_b) * L
```

Horizontal unit and square grid (**confirmed**: "closest fitting value" and "Apply Square Grid"):

```text
M_h        = round(W / g_desired)         grid width fit, Quick and Modular Calculate
u_h        = W / M_h
square     : u_h = u_v, W' = ceil(W / u_v) * u_v        Apply Square Grid expands the width
modular    : u_h = W / (modules_h * subdiv_h), u_v = H / (modules_v * subdiv_v) * leading_multiplier
```

Modular fields in Smart mode, all integers in units (**confirmed** by the preset format and the three wizard outputs):

```text
K_h        = W / u_h                      lines across the page
K_v        = H / u_v                      lines down the page
k_i, k_o   = round(m_i / u_h), round(m_o / u_h)
k_gc       = round(g_c / u_h)
k_col      = round((K_h - k_i - k_o - (c - 1) * k_gc) / c)          wizard rounds to nearest
k_o'       = K_h - k_i - c * k_col - (c - 1) * k_gc                 outside margin absorbs
k_t, k_b   = round(m_t / u_v), round(m_b / u_v)
k_gr       = round(g_r / u_v)
k_row      = floor((K_v - k_t - k_b - (r - 1) * k_gr) / r)          wizard floors
k_b'       = K_v - k_t - r * k_row - (r - 1) * k_gr                 bottom margin absorbs
valid      : k_col >= 1, k_row >= 1, k_o' >= 0, k_b' >= 0, else "Gutter too large"
```

Smart mode enumeration when only counts are known (**derived** from "browse the different available setups" and "whatever areas are left over"): for each `k_col` from 1 to `floor(K_h / c)` and each `k_gc` from 0 to `k_col`, keep combinations with `k_i + k_o = K_h - c * k_col - (c - 1) * k_gc >= 0`, order by distance of `(k_i, k_o)` to the desired margins, then by gutter closeness; same for rows. The vendor's tie-breaking order is unknown.

Gutter as baseline unit (**confirmed** in the preset: `gutter: 1 line`): the gutter is an integer number of grid lines on its axis, so a horizontal gutter is `k_gc * u_h` and a vertical gutter `k_gr * u_v`; with a square grid both equal the leading, the Müller-Brockmann convention of one line of leading between fields (Josef Müller-Brockmann, Grid Systems in Graphic Design, 1981, the "empty line" between fields).

x-height and H-height (**derived**; the vendor reads the font, the manual says outline a glyph to measure): with font units per em `upm`, the OS/2 `sxHeight` and `sCapHeight` values, or measured glyph bounds of `x` and `H`,

```text
x          = s * sxHeight / upm
h_cap      = s * sCapHeight / upm
image line offset above each baseline = x   (or h_cap, or custom)
top margin with image-lines: m_t' = (k_t * L_fit) - x         the manual says the top margin is "compensated for the image-lines height" and the baseline grid then starts at the top margin instead of the page top
font size that makes the image-line height equal the grid width (Quick mode feature): s = u_h * upm / sxHeight (or sCapHeight), rounded to InDesign's 0.001 pt, and the strings admit the match can be inexact
```

Type area and page proportion display (**confirmed** the panel shows them, formula trivial):

```text
P_doc      = H / W                        Doc. Prop.
P_type     = (H - m_t - m_b) / (W - m_i - m_o)      Type Area proportion
```

Margin canons (**derived**, absent from the vendor product; standard references): Van de Graaf canon divides the page so that `m_i : m_o = 1 : 2`, `m_t : m_b = 1 : 2`, type area height equals page width, and `m_i = W / 9`, `m_t = H / 9` for a 2:3 page (Tschichold, The Form of the Book, describing Van de Graaf); Tschichold's golden canon for a 2:3 page gives margins in the ratio 2 : 3 : 4 : 6 (inside, top, outside, bottom); Bringhurst (The Elements of Typographic Style, 8.2) lists page proportions such as 1:1.618 and 2:3 and suggests the type block share the page proportion. Any of these becomes an input to the formulas above after snapping to lines: `k_i = round(m_i / u_h)` and so on, exactly how the vendor's Line mode treats margins.

Unit conversions (**confirmed** by the client script and by the preset values): `1 in = 72 pt`, `1 mm = 2.834645 pt` (client uses that constant; `72 / 25.4 = 2.8346457`), pixels equal points in the "Points and Pixels" unit. Rounding on display is three decimals (`12.027`), which matches InDesign's 0.001 pt precision.

InDesign DOM properties an equivalent sets, in the order a document is built:

| Object | Property | Value from the model |
| :-- | :-- | :-- |
| `app.documents.add()` `documentPreferences` | `pageWidth`, `pageHeight`, `facingPages`, `documentBleedTopOffset` and siblings, `documentSlugTopOffset` and siblings, `pagesPerDocument` | `W`, `H`, spread flag, bleed and slug keys of the preset |
| `document.viewPreferences` | `horizontalMeasurementUnits`, `verticalMeasurementUnits`, `rulerOrigin` | chosen unit, `RulerOrigin.PAGE_ORIGIN` |
| `document.gridPreferences` | `baselineDivision`, `baselineStart`, `baselineGridRelativeOption`, `baselineGridShown`, `documentGridShown`, `horizontalGridlineDivision`, `horizontalGridSubdivision`, `verticalGridlineDivision`, `verticalGridSubdivision`, `gridsInBack` | `L_fit`; `m_t'` or 0; `BaselineGridRelativeOption.TOP_OF_PAGE_OF_BASELINE_GRID_RELATIVE_OPTION` for Fit Leading, `TOP_OF_MARGIN_...` for vertical Value mode and image-lines; `modules_h * subdiv_h` as `horizontalGridlineDivision = u_h * subdiv_h` with `horizontalGridSubdivision = subdiv_h` (same vertically with `u_v`) |
| `masterSpread.pages[n].marginPreferences` | `top`, `bottom`, `left` (inside), `right` (outside), `columnCount`, `columnGutter`, `columnsPositions`, `columnDirection` | `k_t * u_v`, `k_b' * u_v`, `k_i * u_h`, `k_o' * u_h`, `c`, `k_gc * u_h`; for custom (unequal) columns `columnsPositions` |
| `document.masterSpreads.add()` and `masterSpread.name`, `baseName`, `namePrefix` | one per `GC-n` | prefix `GC`, up to five |
| `document.layers.add()` and `layer.name`, `layer.locked`, `layer.visible`, `layer.layerColor` | `[GC-1] Columns` and the rest | colours from Preferences |
| `page.guides.add()` `guide.orientation`, `guide.location`, `guide.itemLayer`, `guide.fitToPage` | row lines at `m_t' + j * (k_row + k_gr) * u_v` and `+ k_row * u_v`, subcolumn and secondary column lines, image-lines at `baseline - x` per baseline | `HorizontalOrVertical.HORIZONTAL` or `VERTICAL` |
| `document.paragraphStyles.add()` and `paragraphStyleGroups` | `[GC] Body Copy #1` | `appliedFont`, `fontStyle`, `pointSize = s`, `leading = L_fit` or its multiple, `alignToBaseline = true`, `firstLineIndent`, `leftIndent`, `tabList`, `tracking`, `kerningMethod`, `otfFigureStyle`, `gridAlignment`, `justification` |
| `document.characterStyles.add()` | `[GC] Body Copy #1` | font, size, leading |
| `document.textFramePreferences` (defaults) | `firstBaselineOffset = FirstBaseline.CAP_HEIGHT`, `useMinimumHeightForAutoSizing` | the vendor Q and A recommends Cap Height for first baseline so text snaps to the first line under the top margin |
| `document.textDefaults` | `leading`, `alignToBaseline` | `L_fit`, true |
| `document.documentPreferences.documentGridSnapto`, `guidePreferences.guidesSnapto` | true | snapping |
| `document.exportFile(ExportFormat.INDESIGN_MARKUP, ...)` | IDML export as the interchange output of a standalone tool | the vendor's SE plans "creates a fully configured document" in InDesign; IDML carries margins, columns, grids, styles, guides, layers but no plug-in data |

## Folder tree

```text
grid-calculator/
├── README.md                          this file
├── bundle/                            9.5 MB, gitignored, unpacked read-only copies
│   ├── GCPE_CC26_Mac_14.pkg.zip       the public demo installer as downloaded
│   ├── Activator 2026.app             AppKit activator (from 01-GC_Activator.pkg)
│   ├── GridCalculator.InDesignPlugin  model plug-in (from 03-GC_Plugins.pkg)
│   ├── GridCalculatorUI.InDesignPlugin  UI plug-in (from 03-GC_Plugins.pkg)
│   └── Registration.bundle            registration helper bundle (from 02-GC_Registration.pkg/Scripts)
├── data/
│   ├── page-sizes.csv                 vendor dropdown names with standard dimensions
│   ├── layout-wizard-samples/         three wizard runs, result JSON and exported preset text
│   ├── update-feed-gcpe.xml           the plug-in's update feed
│   ├── announcement-feed.json         the status API response
│   ├── strings-GridCalculatorUI-plugin.txt   strings -n 6 of the UI plug-in (3052 lines)
│   ├── strings-GridCalculator-plugin.txt     strings -n 6 of the model plug-in (28 lines)
│   └── idrc-resource-strings.txt      ASCII strings of every idrc resource
├── docs/
│   ├── Uninstallation_Guide.pdf       embedded in the installer
│   ├── installer/                     welcome, readme (Power Tips), license text, Distribution.xml, postinstall.sh
│   ├── manual-2022-wayback/           ten Pro Edition manual articles plus five site pages, html and extracted txt
│   └── site-2026/                     current product, updates, KB, about, layout wizard pages, S3 KB and status pages
└── scripts/
    └── layout-wizard-client.js        the wizard page's inline script (baseline hint math, worker call, preset export)
```

## Gaps

- The plug-in was not run, so the panel layout, the Modular "maximum output value" for modules, and the Smart mode combination ordering are undocumented beyond the strings.
- The wizard worker source is not public; its rules above are readings of three outputs, and its inch mode is broken.
- No vendor PDF manual exists; the Pro Edition manual survives only through the Wayback Machine (2022 snapshots) and describes version 4.x; the new site's knowledge base has one real article (Getting started) and one placeholder article on S3 that describes an unrelated CSS grid calculator.
- The DIN size list is generated by code and not recoverable from strings; `data/page-sizes.csv` supplies ISO 216 values for it.
- Windows availability is contradictory: FAQ says macOS only, the update feed lists `windows64` enclosures, the old AppSource listing claims both. Unverified.
- Font metric handling (x-height, cap height) is inside the plug-in and not described; the formulas above are derived.

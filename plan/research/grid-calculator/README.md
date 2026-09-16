# [GRID_CALCULATOR]

Grid Calculator Publishing Edition 1.4 is a native Adobe InDesign plug-in pair with a small AppKit activator, sold by Designers Bookshop. This folder holds the public demo installer unpacked, the vendor documents, the data pulled out of the bundle and the vendor's web wizard, and the scripts that recovered the computation from the binary. Use `plan/design/typography.md` for the grid the repository builds.

Web pages were fetched 2026-09-12, the binary recovery ran 2026-09-15. Nothing was purchased, no installer ran, no binary executed, no license was entered, InDesign was not touched. The `.pkg` was expanded with `pkgutil --expand` and its payloads unpacked with `cpio`. Paths below are relative to this folder, citations are `file:line` for text and hexadecimal addresses in the arm64 slice for code.

## [01]-[CONTENTS]

```text
grid-calculator/
├── README.md
├── bundle/                                  9.5 MB, gitignored, unpacked read-only copies
│   ├── GCPE_CC26_Mac_14.pkg.zip             the public 30-day demo installer as downloaded
│   ├── Activator 2026.app                   AppKit activator, from 01-GC_Activator.pkg
│   ├── GridCalculator.InDesignPlugin        model plug-in, from 03-GC_Plugins.pkg
│   ├── GridCalculatorUI.InDesignPlugin      UI plug-in, from 03-GC_Plugins.pkg
│   └── Registration.bundle                  registration helper, from 02-GC_Registration.pkg/Scripts
├── data/
│   ├── page-sizes.csv                       50 dropdown rows with their standard dimensions
│   ├── layout-wizard-samples/               three wizard runs, result JSON and exported preset text
│   ├── update-feed-gcpe.xml                 the feed the plug-in polls for updates
│   ├── announcement-feed.json               the announcement and server-status API response
│   ├── strings-GridCalculatorUI-plugin.txt  `strings -n 6` of the UI plug-in, 3052 lines
│   ├── strings-GridCalculator-plugin.txt    `strings -n 6` of the model plug-in, 28 lines
│   └── idrc-resource-strings.txt            ASCII strings of every compiled resource
├── docs/
│   ├── Uninstallation_Guide.pdf             one page, embedded in the installer
│   ├── installer/                           welcome, readme, license, Distribution.xml, postinstall.sh
│   ├── manual-2022-wayback/                 ten Pro Edition manual articles and five site pages
│   └── site-2026/                           current product, updates, KB, about, wizard, S3 pages
├── decompiled/                              one C file per recovered grid behaviour, [15]
├── scripts/
│   ├── Decompile.java                       Ghidra: decompile every function, the functions at listed addresses, or those referencing a string
│   ├── DefinedString.java                   Ghidra: the defined-string selection Decompile and ListStringReferences share
│   ├── ListStringReferences.java            Ghidra: list the functions referencing each matching string
│   ├── ListFunctions.java                   Ghidra: list every function with its size
│   ├── pmst.py                              parse a compiled `idrc_PMST` string resource
│   ├── disassembly.py                       helper-call traces, widget-ID loads, callees, and callers over the disassembly
│   └── excerpts.py                          write `decompiled/` from the Decompile outputs
└── sources/                                 gitignored, vendor material read as evidence
    └── layout-wizard-client.js              the wizard page's inline script
```

| [INDEX] | [PART]                      | [WHAT IT IS]                                                                     |
| :-----: | :-------------------------- | :------------------------------------------------------------------------------- |
|  [01]   | `bundle/`                   | Installed components, the only copy of the compiled computation                  |
|  [02]   | `data/page-sizes.csv`       | Value source for the dropdown, whose resource carries names without dimensions    |
|  [03]   | `data/layout-wizard-samples/` | Recorded runs of the vendor's web wizard, the only live product output         |
|  [04]   | `data/strings-*.txt`        | Preset key vocabulary, layer names, alerts, and endpoint URLs                    |
|  [05]   | `docs/manual-2022-wayback/` | Pro Edition manual for 4.0 to 4.2, the only manual that exists                   |
|  [06]   | `docs/site-2026/`           | Product pages for 1.4, release notes, and both knowledge bases                   |
|  [07]   | `scripts/`                  | Recovery tooling of [13]                                                         |
|  [08]   | `sources/`                  | The vendor's own wizard client script, third-party material read as evidence      |
|  [09]   | `decompiled/`               | Decompiled C of every routine a RECOVERED formula cites, [15]                     |

## [02]-[SOURCES]

| [INDEX] | [SOURCE]                                                                         | [WHAT IT IS]                                                             | [SAVED AS]                                            |
| :-----: | :------------------------------------------------------------------------------- | :------------------------------------------------------------------------ | :----------------------------------------------------- |
|  [01]   | `designersbookshop.com/`                                                         | Vendor home, Astro site, update rows dated 2026-04-03                     | Text in [03]                                          |
|  [02]   | `/products/grid-calculator-pe/`                                                  | Product page, features, FAQ, pricing, download                           | `docs/site-2026/products_grid-calculator-pe.html`, `product-page.txt` |
|  [03]   | `/updates/grid-calculator-pe/`                                                   | Release notes 1.0 to 1.4                                                 | `docs/site-2026/updates_grid-calculator-pe.html`, `updates.txt` |
|  [04]   | `/kb/grid-calculator-pe/getting-started/`                                        | One knowledge-base article for PE                                        | `docs/site-2026/kb_grid-calculator-pe_getting-started.html`, `kb-getting-started.txt` |
|  [05]   | `/kb/grid-calculator-se/about-standalone-edition/`                               | Standalone Edition description, PE against SE table                      | `docs/site-2026/kb_grid-calculator-se_about-standalone-edition.html`, `kb-standalone-edition.txt` |
|  [06]   | `/about/`                                                                        | Vendor history                                                           | `docs/site-2026/about.html`, `about.txt`              |
|  [07]   | `/products/grid-calculator-pe/layout-wizard/`                                    | Web Layout Wizard that exports a plug-in preset                          | `docs/site-2026/products_grid-calculator-pe_layout-wizard.html`, `layout-wizard.txt`, `sources/layout-wizard-client.js` |
|  [08]   | `layout-wizard-calculator.noisy-art-eee3.workers.dev`                            | Cloudflare Worker the wizard posts to, 403 to a bare `curl`, exercised through the page in a headless browser | `data/layout-wizard-samples/*` |
|  [09]   | `designersbookshop.s3.amazonaws.com/products/grid_calculator_pe/1.4/cc26/GCPE_CC26_Mac_14.pkg.zip` | Public 30-day demo installer, identical to the licensed build, 2,796,709 bytes, sha256 `df4c27aa09e9b5b00478f7cf6469306081201095548cf007713f7d2e2e2a6e55` | `bundle/GCPE_CC26_Mac_14.pkg.zip` |
|  [10]   | `.../products/updates/grid_calculator_pe/xml/gcpe.xml`                           | Update feed the plug-in polls, cc20 to cc26, Mac and windows64 enclosures | `data/update-feed-gcpe.xml`                           |
|  [11]   | `05xg1fy0se.execute-api.us-east-1.amazonaws.com/prod/announcement`               | Announcement and server-status API, `serverStatus: offline`              | `data/announcement-feed.json`                         |
|  [12]   | `.../products/updates/grid_calculator_pe/index.html`                             | S3 status page behind the menu item Updates and Server Status            | `docs/site-2026/s3-status-updates.html`               |
|  [13]   | `.../products/knowledgebase/grid_calculator_pe/`                                 | S3 knowledge base, one placeholder article about a CSS grid calculator   | `docs/site-2026/s3-knowledgebase-index.html`, `s3-kb-config.json`, `s3-kb-introduction.md` |
|  [14]   | Wayback snapshots of `designersbookshop.com/knowledge-base/user-manual/*`, 2022-05-16 | Pro Edition user manual for 4.0 to 4.2, saved because the live URLs now redirect to the home page | `docs/manual-2022-wayback/knowledge-base_user-manual_*.html` and `.txt` |
|  [15]   | Wayback snapshots of the Pro Edition product, KB index, wizard, questions, and video pages, 2022-01-16 to 2023-09-29 | Older product surface                            | `docs/manual-2022-wayback/grid-calculator-pro-edition*.html` and `.txt` |
|  [16]   | `Uninstallation_Guide.pdf` inside `03-GC_Plugins.pkg`                            | Vendor uninstall guide written for 2025                                  | `docs/Uninstallation_Guide.pdf`                       |
|  [17]   | `Resources/en.lproj/{welcome,readme,license}.rtf` and `Distribution` in the `.pkg` | Installer welcome, Power Tips, EULA, distribution script, installer title Grid Calculator PE 2026 (1.4) | `docs/installer/*`     |

Unopened links on the product page: the FastSpring monthly and yearly checkouts, a LinkedIn Learning chapter, and an older Microsoft AppSource listing claiming Windows and Mac.

## [03]-[PRODUCT]

| [INDEX] | [FACT]              | [VALUE]                                                                                                                                                     | [SOURCE]        |
| :-----: | :------------------ | :----------------------------------------------------------------------------------------------------------------------------------------------------------- | :-------------- |
|  [01]   | Vendor              | Designers Bookshop, the one-person studio of Abraham Georges, Stockholm; the plug-in began as his Berghs School of Communication graduation project and launched 2009 | [06], code signing identity |
|  [02]   | Name history        | Grid Calculator Pro Edition for CS3 through CC 2019 at versions 3.x to 5.1, then Grid Calculator Publishing Edition at 1.0 to 1.4; the UI binary still carries Pro Edition install paths for CS3 to CC 2019 beside PE paths for 2020, 2022, 2026 | [02], [06], [15] |
|  [03]   | Editions            | Publishing Edition available; Standalone Edition in development, exporting to InDesign and Affinity Publisher with Windows planned; Merge CRM unreleased     | [01], [05]      |
|  [04]   | PE against SE       | PE requires InDesign and macOS, no Affinity; SE requires no InDesign, adds Affinity, macOS with Windows planned                                              | [05]            |
|  [05]   | Price               | Monthly USD 15, yearly USD 175, FastSpring checkout, 30-day refund; one license activates on 2 computers and runs on 1 at a time                             | [02]            |
|  [06]   | Demo                | 30-day demo from the same installer, activated with name and email, the license with name, email, and key; online activation is mandatory                    | [02], [04], [17] |
|  [07]   | Current version     | 1.4 released 2026-04-03 as Layout Wizard and Presets; `CFBundleShortVersionString` 1.4; installer signed 2026-05-27                                          | [03], [09]      |
|  [08]   | Changelog           | 1.4 the Layout Wizard button and web presets; 1.3 a new activation server; 1.2 InDesign 2026 macOS support, which dropped the requirement that InDesign 2025 stay installed, and before that the preset overwrite dialog and three menu items; 1.1 a preset crash fix; 1.0 a macOS 15.4 licensing fix | [03] |
|  [09]   | InDesign support    | Adobe InDesign CC 2026 and later on Mac; each InDesign year needs its own plug-in build; the installer refuses to run unless `/Applications/Adobe InDesign 2026/Adobe InDesign 2026.app` exists | [02], [17] |
|  [10]   | Platform            | FAQ says macOS only; the plug-ins declare `LSMinimumSystemVersion` 13.0 and the activator 12.4, all universal arm64 and x86_64; the update feed also lists `windows64` enclosures for cc20 to cc26 | [02], [09], [10] |
|  [11]   | Server state        | Announcement API reports `serverStatus: offline` with a 2026-03-17 note about moving servers and building a standalone app                               | [11]            |
|  [12]   | Presets location    | `~/Documents/Grid Calculator Publishing Edition/Presets/<client folder>/<name>.txt`, sorted by cached file date                                              | [02], [16], `data/strings-GridCalculatorUI-plugin.txt:350-352` |

## [04]-[BUNDLE]

`GCPE_CC26_Mac_14.pkg.zip` holds one flat `.pkg` built with the Packages tool. `Distribution` declares `hostArchitectures="arm64,x86_64"`, `customize="never"`, and the component packages below, all installed as root.

| [INDEX] | [COMPONENT]              | [IDENTIFIER]                                             | [INSTALLS]                                                                                      |
| :-----: | :----------------------- | :------------------------------------------------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | `01-GC_Activator.pkg`    | `com.designersbookshop.gridcalculatorpe.activator.2026`  | `/Applications/Adobe InDesign 2026/Plug-Ins/Grid Calculator Publishing Edition/Activator 2026.app`, 21 files |
|  [02]   | `02-GC_Registration.pkg` | `com.designersbookshop.gridcalculatorpe.registration.2026` | Nothing; `postinstall` writes `/Applications/.GridCalculator/` and `/Library/Application Support/.dbs/.framework_data`, an encrypted UUID that replaces the MAC address as the machine id, runs `RunBundleHelper Registration.bundle`, and logs to `/tmp/gc_encrypted_install.log` |
|  [03]   | `03-GC_Plugins.pkg`      | `com.designersbookshop.gridcalculatorpe.2026`            | Both `.InDesignPlugin` bundles and `Uninstallation_Guide.pdf`, 106 files                        |

| [INDEX] | [BUNDLE FACT]                  | [VALUE]                                                                                                                                                  |
| :-----: | :----------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | Model plug-in id               | `com.DesignersBookshop.GridCalculator`, `CFBundlePackageType` `InD3`, `CFBundleSignature` `InDn`, version 1.4, min macOS 13.0, SDK macosx15.2, Xcode 16.2 |
|  [02]   | UI plug-in id                  | `com.DesignersBookshop.GridCalculatorUI`, same versions and toolchain                                                                                     |
|  [03]   | Activator id                   | `com.designersbookshop.Activator-2026`, version 1.0, min macOS 12.4, `NSMainNibFile` `MainMenuCC17`, `NSPrincipalClass` `NSApplication`                   |
|  [04]   | Architectures                  | Mach-O universal x86_64 and arm64 for every plug-in binary and for `RunBundleHelper`                                                                      |
|  [05]   | Signing                        | `Developer ID Application: Abraham Georges (5ZCTQ6T4SQ)`, hardened runtime flag `0x10000(runtime)`, timestamp 2026-05-27 05:58                            |
|  [06]   | Entitlements                   | `codesign -d --entitlements :-` prints none for any of them; no sandbox, no apple-events entitlement                                                      |
|  [07]   | `Info.plist` keys absent       | `NSAppleEventsUsageDescription`, `OSAScriptingDefinition`, `CFBundleURLTypes`, `CFBundleDocumentTypes`                                                    |
|  [08]   | Model plug-in libraries        | `@rpath/InDesignModel.framework`, Foundation, libobjc, libc++, libSystem                                                                                 |
|  [09]   | UI plug-in libraries           | `@rpath/InDesignModelAndUI.framework`, `ASLSupportLib.dylib`, `WidgetBinLib.dylib`, `DV_WidgetBinLib.dylib`, `TextPanelLib.dylib`, `PMRuntime.dylib`, `PublicLib.dylib`, `DataBaseLib.dylib`, `ObjectModelLib.dylib`, Cocoa, AppKit, CoreFoundation, CoreServices, IOKit, Foundation |
|  [10]   | Activator libraries            | Cocoa, AppKit, Foundation, CoreFoundation, IOKit, libc++                                                                                                  |
|  [11]   | Toolkit                        | InDesign SDK C++ plug-in with InDesign's own widget library, resources compiled by ODFRC as `idrc_VIEW`, `idrc_MENR`, `idrc_ACTD`, `idrc_PMST`, `idrc_LOCR`, `idrc_CLST`, `idrc_FACT`, `idrc_PVER`, `idrc_PLUG`, `idrc_PNGA`, `idrc_SCML`, `idrc_IDPL`, `idrc_ITAG`; the activator is plain AppKit with a nib |
|  [12]   | Resources of interest          | `idrc_PMST/351.idrc` holds the page-size dropdown, `idrc_PNGA/1310.idrc` a PNG icon; no `.sdef`, `.scpt`, `.jsx`, `.idjs`, `.js`, `.json`, `.csv`, `.html`, and no `.lproj` beyond `English.lproj` in the activator |
|  [13]   | Model plug-in classes          | `GCPreferences`, `GCDocPreferences`, `GCSpreadPreferences` implementing `IGCPreferences`, `IGCDocPreferences`, `IGCSpreadPreferences`, the whole of that binary's 28 strings, so per-document and per-spread settings persist inside the `.indd` as plug-in data |
|  [14]   | Diagnostics                    | UI binary keeps a debug assertion path `/Applications/Xcode.app/.../MacOSX15.2.sdk/usr/include/c++/v1/vector:1400`                                    |

## [05]-[INTEGRATION]

Grid Calculator drives InDesign from inside the process through the SDK and carries no scripting bridge.

| [INDEX] | [PROBE]                                                                                                                                                      | [RESULT]                                                                                          |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------ | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `strings -n 6` for `osascript`, `tell application`, `ExtendScript`, `.jsx`, `idjs`, `UXP`, `doScript`, `app.activeDocument`, `marginPreferences`, `gridPreferences`, `baselineDivision`, `baselineStart`, `baselineFrameGridOption`, `columnGutter` | Zero hits in every binary |
|  [02]   | `com.adobe`                                                                                                                                                  | Zero hits; Adobe InDesign appears as a product name in dialogs and install paths alone             |
|  [03]   | `open "http...`                                                                                                                                              | 12 shell `open` calls to vendor URLs behind menu items                                             |
|  [04]   | Network endpoints                                                                                                                                            | `https://gcpe-activation-proxy.noisy-art-eee3.workers.dev` for activation, the `gcpe.xml` update feed, and the activation state file `/Applications/.GridCalculator/com.apple.agdbdactivation18.0.plist` with its `INVALIDATED` key |
|  [05]   | Embedded JavaScript                                                                                                                                          | None                                                                                               |

What the plug-in writes into the document, from the manual and the string table:

- Document settings: width, height, facing pages, bleed and slug per side, and the measurement unit
- Document grid: the horizontal subdivision as the grid width, the vertical subdivision as the leading, and the baseline grid
- Master spreads `GC-1` to `GC-5`, each with its own margins, columns, rows, and image-lines
- Layers prefixed `[GC]` or `[GC-n]`, carrying ruler guides for rows, subcolumns, secondary columns, image-lines, and the type-area grid, and optional drawn vertical lines with a chosen stroke
- Paragraph and character styles `[GC] Body Copy #1`, created on first run and kept synchronized with the leading, inside style groups such as Body Copy, Caption, Info, and Headline, whose leading options are multiples of the baseline or of the subdivision
- Presets as plain text under the user's Documents folder

Native InDesign settings carry the whole layout, as the manual states, so a document opens without the plug-in. Exporting to IDML or INX clears the plug-in's own data, after which the plug-in can no longer edit that layout.

## [06]-[GRID_MODEL]

Vocabulary as the vendor uses it across the manual and the string table.

| [INDEX] | [TERM]                        | [MEANING]                                                                                                                                                         |
| :-----: | :---------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | Document grid                 | InDesign's own document grid; the horizontal subdivision is the grid width, the vertical subdivision the leading                                                   |
|  [02]   | Modules and subdivisions      | Levels of slicing; modules divide the width or height, subdivisions slice each module, and 5 modules by 7 subdivisions equals 1 module by 35 except for the module maximum |
|  [03]   | Line mode, Value mode         | Line mode states margins in document-grid lines, Value mode as entered values; Value mode exists in Quick mode alone, and vertical Value mode on master `GC-1` alone because a document holds one baseline grid |
|  [04]   | Fit Leading                   | Quick-mode checkbox; checked, the leading recalculates to the closest value that fits the page height a whole number of times; unchecked, the exact leading applies and the bottom margin absorbs the remainder |
|  [05]   | Subdivision                   | Quick mode divides or multiplies the applied leading up to five steps                                                                                              |
|  [06]   | Grid Width                    | Quick mode since 4.0; Calculate applies the closest fitting division of the width, and with Fit Leading off and the leading set to that value the grid is square    |
|  [07]   | Apply Square Grid             | Takes the applied leading, applies it horizontally, and expands the document width                                                                                 |
|  [08]   | Proportions                   | Target ratio and a choice of which of width or height changes; the panel displays Doc. Prop. and Type Area                                                       |
|  [09]   | Based On                      | Takes the leading from an existing paragraph style and fits it                                                                                                     |
|  [10]   | Quick mode                    | Default; custom margins, breaking the document grid, a type-area grid through Custom Setup at Fit to margins or Fit to page, custom columns and rows with numeric width and gutter, and the only mode supporting Alternate Layout masters |
|  [11]   | Modular mode                  | Tied to the document grid with independent horizontal and vertical grids; Calculate finds the closest module size to a desired grid width and height, and margins set freely in grid lines |
|  [12]   | Smart mode                    | Tied to the document grid and synchronized with Modular; takes column and row counts, widths, heights, and gutters in lines, or desired margins with a browse over the combinations that fit, leaving margins as the areas left over; raises Gutter too large for this number of columns or rows |
|  [13]   | Column levels                 | Main columns divide the type area, subcolumns split main columns, secondary columns are a second layout on guides; rows carry the same three levels                 |
|  [14]   | Lock                          | Keeps column and row settings while one margin edit moves the type area                                                                                            |
|  [15]   | Image-lines                   | Guides at the x-height or H-height of Body Copy #1 above each baseline, or at a custom height; choosing image-lines snaps the top margin so the first line's top sits on the margin, and Quick mode can set the font size so the image-line height equals the grid width |
|  [16]   | Indent, Exdent, Tabs          | Based on the em, on the horizontal subdivision, or on a custom value; Center Tabs: 4 Ems appears as a default                                                       |
|  [17]   | Units                         | Millimeters, Inches, Points and Pixels; leading always displays in points and its field accepts a suffix such as `3 mm`                                             |
|  [18]   | Page sizes                    | Dropdowns DIN sizes., ANSI and Arch sizes., Phone, Web and Tablet sizes.                                                                                            |
|  [19]   | Limits                        | Width 0.0139 to 216 inches, leading smaller than the page height, five masters per document; Value mode presets do not store rows for `GC-2` to `GC-5` correctly    |

## [07]-[RECOVERY]

Recovery reads the arm64 slice of the UI plug-in. `lipo -thin arm64` over `bundle/GridCalculatorUI.InDesignPlugin/Versions/A/GridCalculatorUI` yields a 2,483,824-byte Mach-O dynamic library, and over the model plug-in an 87,744-byte library that holds no floating-point code. The UI slice holds about 5,734 functions and is built at `-O0`, so InDesign's `PMReal` arithmetic appears as helper calls rather than inline floating-point instructions, which is what makes each formula readable as a call sequence.

Ghidra 12.1.3 publishes native decompiler binaries for `linux_x86_64` and `win_x86_64` alone, so the macOS decompiler is built from the sources in the same release and installed into the Ghidra tree:

```sh
make -C ghidra_12.1.3_PUBLIC/Ghidra/Features/Decompiler/src/decompile/cpp ghidra_opt
cp ghidra_opt ghidra_12.1.3_PUBLIC/Ghidra/Features/Decompiler/os/mac_arm_64/decompile
```

Import and whole-program decompile run headless, and later passes reuse the imported program:

```sh
ghidra_12.1.3_PUBLIC/support/analyzeHeadless <project dir> GCUI -import GCUI.arm64 \
  -scriptPath scripts -postScript Decompile.java <out>.c all
ghidra_12.1.3_PUBLIC/support/analyzeHeadless <project dir> GCUI -process GCUI.arm64 -noanalysis \
  -scriptPath scripts -postScript Decompile.java <out>.c at 1101a0,110ffc,12335c
```

Helper calls in the arm64 slice, each identified by its call target and confirmed by its operand and return pattern across call sites:

| [INDEX] | [ADDRESS] | [HELPER]                                     | [NOTE]                                                        |
| :-----: | :-------- | :------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | 0x13490   | `PMReal` construct                            | Takes the double, returns the boxed value                     |
|  [02]   | 0x157c8   | `operator/`                                   | —                                                             |
|  [03]   | 0x15a7c   | `operator*`                                   | —                                                             |
|  [04]   | 0x17c4c   | `operator+`                                   | —                                                             |
|  [05]   | 0x28080   | `operator-`                                   | —                                                             |
|  [06]   | 0x15abc   | `Round`                                       | 268 call sites, counted over the disassembly                  |
|  [07]   | 0x37d70   | `floor`                                       | —                                                             |
|  [08]   | 0x14140   | Greater-than comparison                       | Reads as `a > b + 1e-8`                                       |
|  [09]   | 0x15808   | Greater-or-equal comparison                   | Reads as `a >= b - 1e-8`                                      |
|  [10]   | 0x152f0   | Decimal quantise                              | Mode 3 gives `Round(x·1000)/1000`, mode 4 four decimals       |

`disassembly.py trace` matches a named helper sequence such as `div,ROUND,div` across the whole disassembly and prints each hit with its enclosing pseudo-function, which is how the formula sites of [08] were located; `Decompile.java string` reaches the sites that carry an alert string and decompiles them, `Decompile.java at` the sites named by address.

## [08]-[FORMULAS]

Primitives, all recovered:

| [INDEX] | [PRIMITIVE]          | [DEFINITION]                                                                                  | [ADDRESS]                                                 |
| :-----: | :------------------- | :--------------------------------------------------------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | `Round`              | `Round(x) = floor(x + 0.5)`, not half away from zero                                          | 0x15abc, 268 call sites                                   |
|  [02]   | `Fit`                | `Fit(X, v) = X / Round(X / v)`                                                                | FUN_001101a0, also 0x107cc8, 0x1116b4, 0x1712bc, 0x123500 |
|  [03]   | Quantise             | `Round(x · 1000) / 1000` before display and application; a fourth mode quantises to 4 decimals | 0x152f0 modes 3 and 4                                     |
|  [04]   | Millimetre           | `1 mm = 2.834646464646465 pt`, equal to `841.89 / 297`, which pins A4 height to 841.890 pt     | Literal at 0x208428                                       |
|  [05]   | Inch, pixel          | `1 in = 72 pt`; pixels equal points in the Points and Pixels unit                              | Binary and manual                                         |
|  [06]   | Comparison tolerance | `a > b` reads as `a > b + 1e-8`, `a >= b` as `a >= b - 1e-8`                                  | 0x14140, 0x15808                                          |
|  [07]   | Page width limits    | 0.0139 in to 216 in, InDesign's own limits                                                    | Literal 216.0                                             |

Variables: `W`, `H` page width and height in points; `L` the desired leading; `L_fit` the applied leading; `u_h`, `u_v` the horizontal and vertical grid units; `m_t`, `m_b`, `m_i`, `m_o` the top, bottom, inside, and outside margins; `T` the type-area span on the axis in question; `c`, `r` column and row counts; `g_c`, `g_r` gutters; `K_h = W / u_h` and `K_v = H / u_v` the lines across and down the page; `k_*` the same measures counted in lines; `s` the body size; `upm` the font's units per em; `x` and `h_cap` the x-height and cap height at `s`.

Evidence classes: RECOVERED cites the address in the arm64 slice and its file under `decompiled/` ([15]), CONFIRMED the manual article and line or the wizard sample that reproduces it, DERIVED the vendor statement the formula follows from. Widget IDs named below are the `0x15d3xx` and `0x15d4xx` immediates the observers load: 0x15d31a width, 0x15d31b height, 0x15d31c leading, 0x15d31d applied leading, 0x15d323 Fit Leading, 0x15d31f Based On, 0x15d49a to 0x15d4a1 the margin line and value fields, 0x15d4a3 and 0x15d4a4 the vertical and horizontal Value boxes, 0x15d4a8 and 0x15d4a9 the type-area width and height, 0x15d4c5 and 0x15d4c6 the row and column Lock boxes, 0x15d339 the module count, 0x15d4c7 and 0x15d4c9 the column and row browse lists.

| [INDEX] | [MODE]                      | [FORMULA]                                                                                                                                                                      | [EVIDENCE]                                                                     |
| :-----: | :-------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :------------------------------------------------------------------------------- |
|  [01]   | Fit Leading, Quick          | `N = Round(H / L)`, `L_fit = Fit(H, L)`, `u_v = L_fit`; the guards are `N ≠ 0`, `L_fit > 1.0`, `L_fit < H`, and a failed guard raises "The output value for the leading is either the same as the document height or greater" | RECOVERED 0x1101a0, `decompiled/1101a0-fit-leading.c`                          |
|  [02]   | Quick horizontal unit       | `u_h = L_fit · (W / H)`, as many horizontal units as baselines                                                                                                                 | RECOVERED 0x107cc8, 0x1116b4, 0x123500, `decompiled/10772c-subdivision.c`      |
|  [03]   | Subdivision, Quick          | Dropdown index `i`: 0 none; 1 to 4 Division with `k = i + 1`, `L_fit = Fit(H, L)`, fine `= L_fit / k`; `i > 4` Multiplication with `k = i − 3`, `L_fit = Fit(H, L · k)`, fine `= L_fit / k`, rejected when `L_fit > H` | RECOVERED 0x10772c, `decompiled/10772c-subdivision.c`                          |
|  [04]   | Grid Width, Calculate       | `M_h = Round(W / g_desired)`, `u_h = W / M_h`; InDesign receives `M_h · subdiv_h` as the horizontal gridline division                                                           | RECOVERED 0x110ffc, `decompiled/110ffc-grid-width.c`                           |
|  [05]   | Modular unit                | `u = W / modules / subdivisions`; a blank module count gives `modules = Round(W / moduleSize_desired)`; one routine serves both axes                                            | RECOVERED 0x16bc0, `decompiled/16bc0-modular-unit.c`                           |
|  [06]   | Margins, line mode          | `k = Round(m / u)`, applied `m' = k · u`; the paired form `Round((m_a + m_b) / u) · u − m_b` lets one margin absorb the remainder                                              | RECOVERED 0x109e04, 0x1773a4, 0x17e768, `decompiled/109e04-margins-lines.c`    |
|  [07]   | Apply Square Grid           | `k = Round(W / u_v)`, `W' = k · u_v`, `u_h := u_v`; the document width expands to make the grid square                                                                          | RECOVERED 0x12335c, `decompiled/12335c-square-grid.c`                          |
|  [08]   | Columns                     | `w_col = (T − (c − 1) · g) / c`, with `T` the type-area span or the page; `w_col ≤ 0` raises "Gutter too large for this number of columns."; `c` is an integer at 2 or above, else "The minimum value must be 2."; guides emit as alternating `w_col/u` and `g/u` runs | RECOVERED 0x131edc, `decompiled/131edc-columns.c`                              |
|  [09]   | Smart column fields         | `k_i = Round(m_i / u_h)`, `k_o = Round(m_o / u_h)`, `k_gc = Round(g_c / u_h)`, `k_col = Round((K_h − k_i − k_o − (c − 1) · k_gc) / c)`, `k_o' = K_h − k_i − c · k_col − (c − 1) · k_gc` | CONFIRMED `data/layout-wizard-samples/a4-preset.txt`, `a4-odd-preset.txt`      |
|  [10]   | Smart row fields            | `k_t = Round(m_t / u_v)`, `k_b = Round(m_b / u_v)`, `k_gr = Round(g_r / u_v)`, `k_row = floor((K_v − k_t − k_b − (r − 1) · k_gr) / r)`, `k_b' = K_v − k_t − r · k_row − (r − 1) · k_gr`; valid when `k_col ≥ 1`, `k_row ≥ 1`, `k_o' ≥ 0`, `k_b' ≥ 0` | CONFIRMED the same two samples                                                 |
|  [11]   | Gutter in lines             | Whole number of grid lines on its axis, `k_gc · u_h` horizontally and `k_gr · u_v` vertically; on a square grid both equal the leading                            | CONFIRMED preset key `Main Columns: 4 columns (8 lines); gutter: 1 line`       |
|  [12]   | Proportion readouts         | `P_doc = H / W` for Doc. Prop., `P_type = (H − m_t − m_b) / (W − m_i − m_o)` for Type Area                                                                                      | CONFIRMED `docs/manual-2022-wayback/knowledge-base_user-manual_interface-top-section.txt` |
|  [13]   | Unit conversion             | `1 in = 72 pt`, `1 mm = 2.834646464646465 pt`, pixels equal points; display rounds to 3 decimals                                                                                | CONFIRMED literal 0x208428 against `sources/layout-wizard-client.js:29-49`     |
|  [14]   | Vertical Value mode leading | `T = H − m_t − m_b` from the value fields, `T = H` while both are blank; `N_t = Round(T / L)`, `L_fit = T / N_t` quantised to 0.001 pt; with image-lines the fit spans `T + x`; `u_h = L_fit · (W − m_i − m_o) / T` and `M_h = Round((W − m_i − m_o) / u_h)`; a master other than `GC-1` snaps `T` to `Round(T / L) · L` | RECOVERED 0x17184, 0x166204, 0x12fc3c, 0xe8428, `decompiled/17184-vertical-value-mode.c` |
|  [15]   | Fit Leading off             | `L_fit = L`; `N = Round(H / L)`, one line dropped when `N · L > H`, `r = H − N · L`; `m_t' = k_t · L` (`+ x` with image-lines), `m_b' = k_b · L + r`; `u_h = Fit(H, L) · W / H` and `M_h = Round(W / u_h)` keep the fitted leading; the subdivision list offers exact `L · k` | RECOVERED 0x128118, 0x14a5c, 0x163ee0, `decompiled/128118-fit-leading-off.c` |
|  [16]   | Smart enumeration           | Over the span `T` between the current margins: for `n = 2, 3, …` while `n · L ≤ T / 2` and `g = 1 … n`, keep `q = quantise((T − n · L) / ((n + g) · L))` when integral and `< 41` as `c = q + 1` columns of `n` lines with a `g`-line gutter; then `c = 2 … K / 2`, `K = Round(T / L)`, `K mod c = 0`, `c < 41` as gutterless columns of `K / c` lines; `std::sort` by the "Sort Columns & Rows Based On" choice with the other two keys as tie-breaks (columns, lines, gutter; lines, columns, gutter; gutter, columns, lines), reversed under Descending; `>` steps to the next entry and wraps to "No Columns", `<` steps back and wraps to the last; rows reuse the routine, with image-lines the row span carries `L − x` | RECOVERED 0x1668f4, 0x1d2528, 0x1d2648, 0x1d2768, 0x1d1c74, 0x1d1ed0, 0x1d21fc, 0x1256bc, `decompiled/1668f4-smart-enumeration.c`; `a4-preset.txt` reproduces `q = 3` for 4 columns of 8 lines in 35 and `q = 5` for 6 rows of 9 lines in 59 |
|  [17]   | Image-line heights          | A text frame on the "[GC] Text" layer in the body font, style, and size holds the glyph, is converted to outlines, and the outline bounding box gives `x = bottom − top` for glyph `x`, `h_cap` for glyph `H`, quantised to 0.001 pt, overshoot included; f-height and l-height presets are replaced with x or H on load | RECOVERED 0x1ca224, `decompiled/1ca224-image-lines.c` |
|  [18]   | Top margin with image-lines | `m_t' = k_t · L + x`, the row span `(K − k_t − k_b) · L − x`, the Value-mode fit over `T + x`                                                                                   | RECOVERED 0x163ee0, 0x129a38, 0x17184, `decompiled/1ca224-image-lines.c`         |
|  [19]   | Size matching a grid square | From `s = 4 pt` in 1 pt steps until the measured height reaches `u_h`, then a bisection to 0.001 pt between the last two sizes; an inexact match sets the closest size and alerts; refused in Modular and Smart mode, with a subdivision, with a Custom kind, or when `u_h > L_fit` | RECOVERED 0x14f5bc, `decompiled/1ca224-image-lines.c`                            |
|  [20]   | Based On                    | The chosen paragraph style supplies `L` = its leading attribute, auto leading as `autoLeading · size`, and `s` = its size; row 01 then runs on `L`                              | RECOVERED 0x1049ec, 0x1d1bc, `decompiled/1049ec-based-on.c`                       |
|  [21]   | Lock                        | Stores `X = k_t + k_b` (rows) or `k_i + k_o` (columns) and disables the line fields; an edit writes the other margin as `X − k_new`, an edit with `X − k_new < 0` reverts to `X − k_other`; rows or columns reapply with the same counts and gutters; unlock stores 0 | RECOVERED 0x157dec, 0x129a38, `decompiled/157dec-lock.c`                          |
|  [22]   | Module cap                  | No count cap; `s = W / modules` must satisfy `1 pt ≤ s ≤ 1000 pt` (0.352778 to 352.778 mm, 0.0138889 to 13.8889 in), else the size field blanks; the grid division is `modules · subdivisions`; "The value must be between 1 and 1000." is referenced by the string table alone | RECOVERED 0x1154e4, `decompiled/1154e4-module-cap.c`                              |

Readback checks, one per formerly unknown row, each run in the installed product on a scratch document and read through the InDesign DOM, confirm the recovered rows and feed the oracle fixtures:

| [INDEX] | [ROW]                   | [CHECK]                                                                                                                                                                             |
| :-----: | :---------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | 15 Fit Leading off      | Quick, A4, leading 12 pt, Fit Leading unchecked, top margin 20 mm; `gridPreferences.baselineDivision` reads 12, `marginPreferences.bottom` reads `k_b · 12 + 841.89 − 70 · 12` |
|  [02]   | 14 Vertical Value mode  | `GC-1` top 20 mm and bottom 20 mm through the vertical Value box, leading 12 pt; `baselineDivision` reads `Fit(841.89 − 113.386, 12)`                                            |
|  [03]   | 16 Smart enumeration    | A4, 4 columns, gutter 1 line, sort by number of columns; five presses of `>` walk the sorted entries and `columnCount`, `columnGutter` follow each `(c, n, g)`                     |
|  [04]   | 17 Image-line measure   | A body style on a font whose OS/2 `sxHeight` differs from the outline height of glyph `x`; each image-line guide `location` minus its baseline reads the outline height              |
|  [05]   | 20 Based On             | A paragraph style at 13.4 pt leading on A4; `baselineDivision` reads 13.363                                                                                                         |
|  [06]   | 21 Lock                 | 4 columns with a 1-line gutter, Lock on, inside margin raised 2 lines; the outside margin drops 2 lines and `columnsPositions` shift by two units                                   |
|  [07]   | 22 Module cap           | Modular mode, A4, 596 modules; the module size field blanks                                                                                                                         |

## [09]-[PRESETS]

Preset grammar comes from the UI plug-in's string table at 0x221c83 to 0x222860, listed at `data/strings-GridCalculatorUI-plugin.txt:388-553`, and is checked against the preset files under `data/layout-wizard-samples/`. A preset is `Key: value;` one per line, newline separated, `Type:` first, then a global block, then one `[GC-n]` block per master, then two guard lines: `/*********** DO NOT REMOVE OR CHANGE THIS SEPARATOR ***********/` and `/*WARNING: Please do not change the format of this file, otherwise it can cause the application to malfunction*/`. A key suffixed `2` is the count of a level and `3` its size, and `Width 4` and `Height 4` are the fourth widget of their axis.

Global keys in emission order:

| [INDEX] | [GROUP]        | [KEYS]                                                                                                                    |
| :-----: | :------------- | :-------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | Header         | `Type`, `Version`, `Preset Name`, `Date`                                                                                  |
|  [02]   | Setup          | `Document Setup` at `Quick`, `Modular`, or `Smart`, `Number of Masters`, `Facing Pages`, `Unit`                           |
|  [03]   | Horizontal     | `Width 4`, `H. Module 2`, `H. Module 3`, `H. Subdiv 2`, `H. Subdiv 3`                                                     |
|  [04]   | Vertical       | `Height 4`, `V. Module 2`, `V. Module 3`, `V. Subdiv 2`, `V. Subdiv 3`, `Leading`, `Correct Leading (pt)`                 |
|  [05]   | Grid width     | `Desired Grid Width`, `Applied Grid Width`, `Square`, `Fit Leading`, `Subdivision`                                         |
|  [06]   | Body style     | `Font`, `Style`, `Size (pt)`, `Style Leading`, `Alignment`, `Tracking`, `Kerning`, `Indent/Exdent`, `Tabs`, `Figure Style` |
|  [07]   | Bleed and slug | `Bleed Chain`, `Bleed Top`, `Bleed Bottom`, `Bleed Inside`, `Bleed Outside`, then the five `Slug` keys                     |
|  [08]   | Vertical mode  | `Vertical Value Mode`                                                                                                     |

Per-master `[GC-n]` keys:

| [INDEX] | [GROUP]           | [KEYS]                                                                                                                          |
| :-----: | :---------------- | :-------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | Master            | `No. of Master Pages`, `Horizontal Value Mode`                                                                                   |
|  [02]   | Image-lines       | `Image-lines height (pt)`, `Image-lines` with the kinds `f-height`, `x-height`, `l-height`, `H-height`                            |
|  [03]   | Smart state       | `Smart Columns applied`, `Smart Rows applied`                                                                                    |
|  [04]   | Desired margins   | `Desired Top Margin`, `Desired Bottom Margin`, `Desired Inside Margin`, `Desired Outside Margin`                                 |
|  [05]   | Applied margins   | `Top margin (lines)`, `Bottom margin (lines)`, `Inside margin (lines)`, `Outside margin (lines)`, and the four unit-valued twins |
|  [06]   | Columns           | `Main Columns: N columns (K lines); gutter: G line(s)`, `Custom Columns: Num: n, Width: w, Gutter: g`                            |
|  [07]   | Subcolumns        | `Main Subcolumns`, `Custom Subcolumns`                                                                                          |
|  [08]   | Secondary columns | `Fit sec. columns` at `Fit to margins` or `Fit to page`, `Main Secondary Columns`, `Custom Secondary Columns`                    |
|  [09]   | Rows              | `Fit rows`, `Main Rows`, `Custom Rows`, `Main Subrows`, `Custom Subrows`                                                         |
|  [10]   | Secondary rows    | `Fit sec. rows`, `Main Secondary Rows`, `Custom Secondary Rows`                                                                  |
|  [11]   | Type-area grid    | `Typearea Grid (Horizontal): L lines - v`, `Typearea Grid (Subleading)`                                                          |

Preset the wizard produced for A4, abridged from `data/layout-wizard-samples/a4-preset.txt`:

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

Reading of those keys: `H. Module 2` is the module count across the width, `H. Module 3` the module size in the unit, `H. Subdiv 2` the subdivisions per module, `H. Subdiv 3` the subdivision size; the vertical four are the same, where the subdivision size equals the leading at 4.243 mm, which is 12.027 pt. Margins, column widths, and gutters are integers in subdivision units, which the vendor calls lines. The plug-in stores only `Body Copy #1` and its own masters in a preset, as its own warning dialog states at `data/strings-GridCalculatorUI-plugin.txt:1368`.

## [10]-[PAGE_SIZES]

Page-size dropdown reads the compiled resource `bundle/GridCalculatorUI.InDesignPlugin/Versions/A/Resources/idrc_PMST/351.idrc`, which `scripts/pmst.py` parses. Header is `uint32 version = 1`, `uint32 reserved = 0`, `uint32 count = 71`, and each entry is a `uint16 len` and ASCII string twice, the key and its display label, identical in every size row.

| [INDEX] | [FAMILY] | [ENTRIES]                                                                                                 |
| :-----: | :------- | :---------------------------------------------------------------------------------------------------------- |
|  [01]   | DIN      | A0 to A10, B0 to B10, C0 to C10                                                                           |
|  [02]   | U.S.     | ANSI A to ANSI E, Arch A to Arch E                                                                        |
|  [03]   | Digital  | 23 pixel sizes from `480 x 320 (Phone)` to `2048 x 1536 (Tablet)`                                          |

Each family name is an entry of its own, and the tail of the resource carries the Smart-mode field labels `Number of columns/rows`, `Number of lines per column/row`, and `Number of lines per gutter`. The resource holds names alone: every dimension is computed in code, DIN entries included, so `data/page-sizes.csv` is the value source, 50 rows over the four families with an ISO 216, ISO 269, ANSI, or Arch citation per row.

## [11]-[NAMING]

Every name the plug-in writes carries a `[GC]` prefix, which the manual forbids renaming. The `[GC-n]` forms are composed per master at run time and appear in the string table at their `GC-1` spelling.

| [INDEX] | [KIND]          | [NAMES]                                                                                                                                                      | [LINE]                                              |
| :-----: | :-------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------ | :---------------------------------------------------- |
|  [01]   | Master layers   | `[GC-1] Columns`, `[GC-1] Main Columns`, `[GC-1] Subcolumns`, `[GC-1] Secondary Columns`, `[GC-1] Image-lines`                                                | 1061, 1062, 945, 944, 934                           |
|  [02]   | Document layers | `[GC] Rows`, `[GC] Secondary Rows`, `[GC] Subcolumns`, `[GC] Images`, `[GC] Text`, `[GC] Break Vertical`, `[GC] Break Horizontal`                             | 326, 327, 328, 1058, 932, 151, 883                  |
|  [03]   | Styles          | `[GC] Body Copy #1` paragraph and character, in the style group `[GC] Body Copy`                                                                              | 230, 223                                            |
|  [04]   | Undo names      | `[GC] Set Leading`, `[GC] Square: `, `[GC] Subdivision: `, `[GC] Desired Grid Width: `, `[GC] Smart Setup`, `[GC] CGS`, `[GC] Edit Existing`                  | 928, 406, 929, 581, 198, 197, 1015                  |
|  [05]   | Masters         | `GC-1` to `GC-5`, a hard cap of five behind the alert "You have already created 5 masters for this document which is the maximum amount possible."            | 186, 1054, 1053                                     |

Lines are in `data/strings-GridCalculatorUI-plugin.txt`.

## [12]-[VALIDATION]

| [INDEX] | [CASE]                       | [INPUT]                                                    | [RECOVERED FORMULA GIVES]                                        | [PRODUCT GIVES]                            | [SOURCE]                                                             |
| :-----: | :--------------------------- | :--------------------------------------------------------- | :----------------------------------------------------------------- | :------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | Fit Leading                  | A4, 12 pt                                                  | `Round(841.890 / 12) = 70`, `841.890 / 70 = 12.027`               | 12,027 pt                                  | `docs/manual-2022-wayback/knowledge-base_user-manual_quick-mode.txt:21` |
|  [02]   | Subdivision, multiply by 3   | A4, 4 pt, one Multiplication step of 3                     | `Fit(841.890, 12) = 12.027`, fine `= 12.027 / 3 = 4.009`          | 4,009 x 3 = 12,027 pt                      | `docs/manual-2022-wayback/knowledge-base_user-manual_paragraph-character-styles.txt:52` |
|  [03]   | Fit Leading, wizard          | 210 × 297 mm, 12 pt                                        | 70 vertical modules of 4.243 mm                                   | `V. Module 2: 70`, `V. Module 3: 4.243`    | `data/layout-wizard-samples/a4-preset.txt`                           |
|  [04]   | Fit Leading, wizard          | 210 × 297 mm, 11 pt                                        | 77 vertical modules of 3.857 mm, which is 10.934 pt               | `V. Module 2: 77`, `V. Module 3: 3.857`    | `data/layout-wizard-samples/a4-odd-preset.txt`                       |
|  [05]   | Smart column and row fields  | A4, inside 20, outside 20, 4 columns, gutter 5, top 20, bottom 20, 6 rows, row gutter 5 | Inside 4, outside 3, column 8, gutter 1; top 5, bottom 6, row 9, gutter 1 | Same lines                         | `data/layout-wizard-samples/a4-preset.txt`                           |
|  [06]   | Smart column and row fields  | A4, inside 17, outside 23, 5 columns, gutter 4, top 15, bottom 25, 7 rows, row gutter 4 | Inside 17, outside 22, column 31, gutter 4; top 4, bottom 11, row 8, gutter 1 | Same lines                     | `data/layout-wizard-samples/a4-odd-preset.txt`                       |

Wizard rounding shows in both runs: a column width rounds to the nearest unit and the outside margin takes the difference, 7.75 becoming 8 and 30.8 becoming 31 while the outside margin falls from 20 to 15 and from 23 to 22; a row height is the largest count that fits and the bottom margin takes the difference, 9.17 becoming 9 and 8.71 becoming 8 while the bottom margin grows from 5 to 6 and from 6 to 11.

Wizard computes its horizontal unit by a different algorithm from the plug-in's and is no oracle for `u_h`: it takes the greatest common divisor of width, inside, outside, and gutter at the entered precision, which the page itself describes as finding the largest value that divides everything evenly. Its vertical unit does come from the leading fit and does agree with the plug-in, since `sources/layout-wizard-client.js:29-49` computes `docHpt = Round(docHmm · 2.834645 · 1000) / 1000`, `N = Round(docHpt / desired)`, `applied = Round(docHpt / N · 1000) / 1000`. A third run at 8.5 × 11 in with 0.75 in margins, 3 columns, gutter 0.1667, 14 pt returned 2 baselines at a 0.0001 in unit and a preset reading `Unit: Millimeters`, so the worker multiplies inch values as millimetres and its inch path is broken as of 2026-09-12.

## [13]-[SCRIPTS]

Ghidra scripts take their arguments after `-postScript <name>` and run under the headless invocation of [07], the Java sources of `scripts/` compiling together as one bundle. Python scripts run under the interpreter directly. Every `Decompile.java` output tags each routine with a `//==== FUNC @ <request> -> <entry>` line, the request being the listed address or the entry itself, and decompiles at a one-hour timeout with a 1000 MB decompiler payload, the 63 KB observers needing `GHIDRA_HEADLESS_MAXMEM=16G` on the process.

| [INDEX] | [SCRIPT]                          | [WHAT IT DOES]                                                                                                     | [INVOCATION]                                                                 |
| :-----: | :-------------------------------- | :------------------------------------------------------------------------------------------------------------------ | :---------------------------------------------------------------------------- |
|  [01]   | `Decompile.java`                  | Decompiles every function in the program into one file                                                              | `-postScript Decompile.java <out>.c all`                                     |
|  [02]   | `Decompile.java`                  | Decompiles the functions containing the listed addresses, disassembling and creating a function where analysis left none | `-postScript Decompile.java <out>.c at 1101a0,110ffc,12335c`                 |
|  [03]   | `Decompile.java`                  | Decompiles every function that references a defined string containing one of the needles                            | `-postScript Decompile.java <out>.c string 'Gutter too large\|minimum value'` |
|  [04]   | `pmst.py`                         | Parses a compiled `idrc_PMST` resource and prints its key and label pairs                                           | `python3 scripts/pmst.py <file>.idrc`                                        |
|  [05]   | `disassembly.py trace`            | Prints every pseudo-function whose `PMReal` helper-call sequence matches the pattern                                | `python3 scripts/disassembly.py trace GCUI.dis div,ROUND,div`                |
|  [06]   | `sources/layout-wizard-client.js` | Wizard page's own inline script: baseline hint math, the worker call, preset export                                 | Read; the page runs it                                                       |
|  [07]   | `ListStringReferences.java`       | Lists every defined string containing a needle with the address and function of each reference to it                | `-postScript ListStringReferences.java <out>.txt 'Fit Leading\|kGCLockKey'`   |
|  [08]   | `ListFunctions.java`              | Lists every function with its entry and size, the map `disassembly.py callers` reads                                | `-postScript ListFunctions.java funcs.txt`                                   |
|  [09]   | `disassembly.py widgets`          | Prints the pseudo-functions loading each given widget ID immediate                                                  | `python3 scripts/disassembly.py widgets GCUI.dis 15d323 15d4a9`              |
|  [10]   | `disassembly.py callees`          | Text-section call targets of one address range with each target's helper sequence                                   | `python3 scripts/disassembly.py callees GCUI.dis 18f340 19e940`              |
|  [11]   | `disassembly.py callers`          | Functions of the `ListFunctions` map calling each target, with call counts                                          | `python3 scripts/disassembly.py callers GCUI.dis funcs.txt 1668f4`           |
|  [12]   | `excerpts.py`                     | Writes `decompiled/` from the `Decompile` outputs, the table of files and routines inside it                        | `python3 scripts/excerpts.py <decompile dir> decompiled`                     |

## [14]-[GAPS]

- Every formula of [08] is recovered from the binary; the plug-in was not run, so the readback checks of [08] stand as the only unexecuted step, and the baseline grid start relative to the compensated top margin of row 18 is read from the document rather than the code, since the plug-in writes it through InDesign commands the decompile shows only as boss IDs
- Wizard worker is closed source, its rules read from the recorded outputs, and its inch path is broken
- No vendor PDF manual exists; the Pro Edition manual survives through the Wayback Machine alone and describes version 4.x, while the new knowledge base holds one real article and one placeholder about an unrelated CSS grid calculator
- Windows availability is contradictory: the FAQ says macOS only, the update feed lists `windows64` enclosures, and the old AppSource listing claims both
- Vendor states no margin canon and no DPI setting; the tie-break between column combinations is the sort of row 16, the closest-fitting rule is `Round` throughout, and the module maximum is the size bound of row 22

## [15]-[DECOMPILED]

`decompiled/` holds the C the Ghidra decompiler produced for every routine a RECOVERED row of [08] cites, one file per behaviour named `<entry address>-<behaviour>.c`, each opening with a comment that states the behaviour, the routines, and the helper-call addresses the arithmetic reads as. Functions keep Ghidra's `FUN_<address>` names; the widget IDs are the `0x15d3xx` and `0x15d4xx` immediates listed in [08].

| [INDEX] | [FILE]                              | [BEHAVIOUR]                                        | [ROUTINES]                                                                                                   | [HELPER CALLS]                                                                 |
| :-----: | :---------------------------------- | :------------------------------------------------- | :----------------------------------------------------------------------------------------------------------- | :----------------------------------------------------------------------------- |
|  [01]   | `1101a0-fit-leading.c`              | Fit Leading, Quick, row 01                         | `FUN_001101a0`                                                                                               | 0x157c8, 0x15abc, 0x152f0, 0x132fc, 0x3208c, 0x15808                          |
|  [02]   | `10772c-subdivision.c`              | Subdivision and the Quick horizontal unit, rows 02, 03 | `FUN_0010772c`                                                                                           | 0x157c8, 0x15a7c, 0x15abc, 0x152f0, 0x14140                                    |
|  [03]   | `110ffc-grid-width.c`               | Grid Width, Calculate, row 04                      | `FUN_00110ffc`                                                                                               | 0x157c8, 0x15abc, 0x15a7c, 0x152f0                                             |
|  [04]   | `16bc0-modular-unit.c`              | Modular unit, row 05                               | `FUN_00016bc0`                                                                                               | 0x157c8, 0x15abc, 0x132fc                                                      |
|  [05]   | `109e04-margins-lines.c`            | Margins in line mode, row 06                       | `FUN_00109e04`, `FUN_001773a4`, `FUN_0017e768`                                                               | 0x157c8, 0x15abc, 0x15a7c, 0x28080, 0x17c4c, 0x31d38                           |
|  [06]   | `12335c-square-grid.c`              | Apply Square Grid, row 07                          | `FUN_0012335c`                                                                                               | 0x157c8, 0x15abc, 0x15a7c, 0x152f0                                             |
|  [07]   | `131edc-columns.c`                  | Columns, row 08                                    | `FUN_00131edc`                                                                                               | 0x157c8, 0x15a7c, 0x28080, 0x31d38                                             |
|  [08]   | `15abc-primitives.c`                | Round, quantise, the tolerance comparisons         | `FUN_00015abc`, `FUN_000152f0`, `FUN_000132fc`, `FUN_00018504`, `FUN_00014140`, `FUN_00015808`, `FUN_00031d38`, `FUN_0003208c`, `FUN_0005c3d4`, `FUN_000c54f8`, `FUN_000c5540` | 0x37d70, 0x15a7c, 0x157c8, the 1e-8 literal at 0x208408 |
|  [09]   | `128118-fit-leading-off.c`          | Fit Leading off, row 15                            | `FUN_00128118`, `FUN_00014a5c`, `FUN_00163ee0`                                                               | 0x157c8, 0x15abc, 0x15a7c, 0x28080, 0x14140, 0x152f0, model setter vtable + 0x130 and getter + 0x128 |
|  [10]   | `17184-vertical-value-mode.c`       | Vertical Value mode, row 14                        | `FUN_00017184`, `FUN_00166204`, `FUN_0012fc3c`, `FUN_000e8428`                                               | 0x1544c, 0x157c8, 0x15abc, 0x17c4c, 0x28080, 0x152f0, 0x132fc                   |
|  [11]   | `1668f4-smart-enumeration.c`        | Smart enumeration, sort, browse, row 16            | `FUN_001668f4`, `FUN_001d1c74`, `FUN_001d1ed0`, `FUN_001d21fc`, `FUN_001d2528`, `FUN_001d2648`, `FUN_001d2768`, `FUN_001256bc` | 0x15a7c, 0x17c4c, 0x28080, 0x157c8, 0x152f0, 0x15abc, 0x132fc, 0x3208c, 0x31d38, `std::sort` 0xc54f8, `std::reverse` 0xc5540 |
|  [12]   | `1ca224-image-lines.c`              | Image-line heights, top margin, size matching, rows 17 to 19 | `FUN_001ca224`, `FUN_00163ee0`, `FUN_0014f5bc`                                                     | 0x28080 over the PMRect fields at +0x8 and +0x18, 0x152f0, 0x17c4c, 0x157c8, 0x3208c, 0x31d38, 0x14140, 0x132fc |
|  [13]   | `1049ec-based-on.c`                 | Based On, row 20                                   | `FUN_001049ec`, `FUN_0001d1bc`                                                                               | 0x15a7c, 0x132fc, `QueryByClassID` over 0x1b1b, 0x1b1a, 0x1b03                 |
|  [14]   | `157dec-lock.c`                     | Lock, row 21                                       | `FUN_00157dec`, `FUN_00129a38`                                                                               | 0x17c4c, 0x28080, 0x15808, 0x157c8, 0x15abc, model setters vtable + 0x1e0 and + 0x1f0, getter + 0x1d8 |
|  [15]   | `1154e4-module-cap.c`               | Module cap, row 22                                 | `FUN_001154e4`                                                                                               | 0x157c8, 0x15a7c, 0x3208c, 0x14140, model setter vtable + 0xf0                 |

Regeneration from the bundle, every path relative to this folder and the Ghidra tree built as in [07]:

```sh
lipo -thin arm64 bundle/GridCalculatorUI.InDesignPlugin/Versions/A/GridCalculatorUI -output GCUI.arm64
objdump -d GCUI.arm64 > GCUI.dis
ghidra_12.1.3_PUBLIC/support/analyzeHeadless proj GCUI -import GCUI.arm64 -scriptPath scripts -postScript ListFunctions.java funcs.txt
ghidra_12.1.3_PUBLIC/support/analyzeHeadless proj GCUI -process GCUI.arm64 -noanalysis -readOnly -scriptPath scripts \
  -postScript Decompile.java targets.c at 110574,107cc8,1116b4,16dd8,10a010,177ab0,17edb4,150a0
```

`Decompile.java at` runs per source file of `scripts/excerpts.py`, one headless process each over a copy of `proj` when they run side by side, with the address lists that file names; the 63 KB Smart observer at 0x18f340 needs `GHIDRA_HEADLESS_MAXMEM=16G` on the process, and 0x1154d0, where analysis defined no function, gets one created before its decompile. `python3 scripts/excerpts.py <dir holding the outputs> decompiled` then writes the files above.

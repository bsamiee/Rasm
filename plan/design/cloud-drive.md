# [CLOUD_DRIVE]

Drive-side specification for the cloud-library branch: the unified swatch set and its placement, templates, fonts inventory and Typeface, Drive reorganization (names, junk, format policy, TK9, scripts, iCloud), machine ledger, lanes. Every value below was read on 2026-09-11 from the Drive, the machine, the thesis folder, or the named page. "Cloud" means Google Drive alone. Adobe Creative Cloud Libraries are an excluded route everywhere in this branch: no swatch, style, or template is published to a CC Library, no Libraries panel integration is built, and the Illustrator panel command `Add selected Swatches and Swatch Groups to my current Library` is never run.

Paths: `D` = `<drive.designLibrary>`; `S` = `D/05.Software Related Assets`; `T` = `D/98.Templates`; `M` = `~/Library/Application Support/design-tools`; `C` = `/Users/bardiasamiee/Library/CloudStorage/OneDrive-Personal/05.School/Fall 2026/Masters Preperation` (the thesis folder); `A` = `.artifacts/creative-cloud/library` in the repo; `SP` = `plan/research/sources` (read-only source tree); `I` = `plan/inputs` (artefacts an agent regenerates); `<host.key>` = a value of `.artifacts/creative-cloud/resolved-paths.json` (`rasm-integration.md` [04]-[13] key schema; `<major>`, `<build>`, and `<year>` in a stale-folder path name the release edition that entry records).

## [01]-[INTENT]

| [INDEX] | [RULE] | [TEST] |
| :-----: | :----- | :----- |
| [01] | Every colour a lane writes is a row of the unified swatch set `S/99.Default Profiles/Shared/Default Palette.ase` ([03.1]); no lane recreates, extends, or redesigns it | `palette.json` = the `swatch.parse` read of that file (92 rows, two groups) plus alias keys; every colour row in a template, an import, or a preference cites a `palette.json` key; no file states a colour value of its own |
| [02] | The set's Adobe RGB (1998) 8-bit values are placed unchanged; every receiving document is Adobe RGB (1998) | `inspect` readback in the Illustrator template equals `palette.json`; `default-palette.json` `workingProfile.name` = `Adobe RGB (1998)` |
| [03] | An alias (`accent`, `primary accent blue`, `ink`, `field`) names one row of the set and carries no value | every alias in `palette.json` is `{of: <swatch name>}` |
| [04] | Every Drive change is a manifest row with an identity recorded before the change, reviewed before it applies | `A/manifests/*.plan.json` rows carry `sha256` (local) or `bytes+mtime` (placeholder) and `reviewed: true` before `status: applied` |
| [05] | A Drive deletion is a move to Drive trash, inside the authorized scope of `plan` [01], never a permanent delete | `rasm-organize` deletes through Finder `delete`; no `os.remove`, `shutil.rmtree`, or `rm` touches `D` |
| [06] | A twin is retired only after its replacement exists and its identity is recorded | every `retire` row names the `kept` path and both hashes |
| [07] | Nothing is deleted from the machine before its research copy hash matches | every machine ledger row with a research copy has `sha256_copy == sha256_original` |
| [08] | Fonts inside a template package stay beside that package; every font on Drive stays where it is; the three font sources and their Typeface source rows are `illustrator-acrobat-cc.md` [07.6] | the Typeface sidebar after job T1 ([05.2]) |
| [09] | Purchased material with no rebuild use is reported, not deleted, unless it is junk: a package whose leaf folders each hold one to four files around a single template with no distinct InDesign document, or a web scrape, goes to Drive trash after its survey row ([04.2] rows 12 and 17, M14); the valuable packages stay | PowerPoint packs, Kelman and Typefool sources stay and the report lists them; `logo bank package` and `Pack of Templates` are M14 rows; `A/templates-packages.tsv` holds one row per purchased package (folders, files, files per leaf folder, formats, distinct InDesign documents, verdict) reviewed before M14 applies |
| [10] | A step that must run inside an Adobe application is a job card with its inputs ready before the card is issued | every job in [08] names its input files and its evidence file |
| [11] | The library carries no junk; [06.2] M08 rows [30]–[33] name the classes | `fd` over `D` with the M08 name rules returns 0 after apply |
| [12] | Every svg and eps under `03.Design Assets` (`99.Art` keeps its svg), `04.Architectural Assets`, and `98.Templates` is one `.ai` master per pack with the originals in Drive trash ([06.2] M10) | `fd -e svg -e eps` over those roots after apply returns the `99.Art` svg alone; every M10 master's `inspect` item count ≥ the sum of its sources' item counts |
| [13] | Every TK9 copy has a row and the final set is named before any deletion ([06.2] M11) | every M11 path resolves on disk or is the Drive-trash purge row |
| [14] | Every pack states what already conforms before what changes | every M10, M11, and M12 row names the kept files first, then the moved, merged, or retired ones |

## [02]-[CURRENT_STATE]

### [02.1]-[DRIVE_TREE]

Walk of `D` with `os.lstat`, `st_flags & SF_DATALESS`, 2026-09-11 22:09; a second walk at 23:12 returned the same 158,793 files and 398,169,849,070 bytes with 144,135 placeholders (1,199 fewer under `98.Templates`, 1 fewer under `05.Software Related Assets`: the IDML and twin reads between the two walks materialised them; `99.Art` unchanged at 77,761 files / 77,735 placeholders). Placeholder counts below are the 22:09 values.

| [INDEX] | [FOLDER] | [FILES] | [BYTES] | [PLACEHOLDERS] | [THESIS_INDEX_FILES] | [CURRENT → TARGET] |
| :-----: | :------- | ------: | ------: | -------------: | ------------------: | :----------------- |
| [01] | `D` (all) | 158,793 | 398,169,849,070 | 145,335 (144,135 at 23:12) | 80,912 (99.Art excluded) | as below |
| [02] | `My Default Profile.ai`, `.DS_Store` | 2 | 65,173,427 | 0 | 1 | root `.ai` stays (fork of [02.3]); superseded by 5a and gui-stream, reported |
| [03] | `00.General/` | 340 | 2,791,062,415 | 249 | 338 | unchanged |
| [04] | `01.Fonts/` | 2,340 | 254,085,080 | 1,260 (854 `.glif`, 226 `.eot`, 81 `.woff2`, 60 `.woff`, 39 other; 0 `.otf/.ttf/.ttc`) | 2,340 | unchanged; its premium folder is the Typeface location of `illustrator-acrobat-cc.md` [07.6] row 02, every other folder imported by nothing |
| [05] | `03.Design Assets/` | 120,438 | 317,154,261,138 | 112,306 (99.Art 77,735) | 42,587 | typos fixed, genre moves into `99.Art`, genre CSH deleted |
| [06] | `04.Architectural Assets/` | 10,815 | 19,377,356,081 | 9,217 | 10,810 | typos fixed |
| [07] | `04.Software Related Assets/` | 3 | 111,268 | 0 | 3 | deleted (holds one IconJar backup) |
| [08] | `05.Software Related Assets/` | 16,683 | 38,755,355,597 | 16,110 (IconJar 15,462; 16,109 at 23:12) | 16,681 | IconJar deleted (15,483 files: 7,740 + 7,740 + 3, `fd -H` 23:05), swatch forks retired, Von Glitshcka renamed |
| [09] | `06.Videography Assets/` | 6 | 196,683,564 | 6 | 6 | unchanged |
| [10] | `97.Photography Presets/` | 0 | 0 | 0 | 0 | unchanged (empty) |
| [11] | `98.Templates/` | 8,166 | 19,575,760,500 | 6,187 (4,988 at 23:12) | 8,146 | typos fixed, twins retired, modern saves |

`98.Templates` by extension: jpg 2,766, ai 1,552, png 537, pdf 468, psd 381, ttf 350, idml 314, indd 297, pptx 264, otf 172, xml 140, svg 138, jpeg 125, dwg 96, key 94, dgn 73, txt 68, lst 66, thmx 47, woff2 38, indt 16, fig 16, docx 16, eps 14, xlsx 13, pat 10, doc 10, gdoc 7, rar 6, mp4 6, dwt 6, bak 5, ppt 4, ctb 3, ait 2, zip 2. The plan's "17 INDT" is the count over `D`: 16 under `98.Templates` (none inside `logo bank package` or `Megapack 2`; `fd -H -e indt` 23:05) plus `S/99.Default Profiles/InDesign/Default Template.indt`; 314 IDML, 297 INDD, 2 AIT recounted the same way.

### [02.2]-[DUPLICATES_BY_HASH]

`shasum -a 256` over `S/02.Color Swatches`, `S/99.Default Profiles`, the root `.ai`, every twin candidate in `T`, and the 26 Kelman files (2026-09-11 22:10–22:40; the full-tree pass over `T` continues as the bootstrap `rasm-inventory` run, which hashes every local file and records placeholders by bytes and mtime).

| [INDEX] | [KEPT] | [TWIN] | [BYTES] | [SHA256_PREFIX] |
| :-----: | :----- | :----- | ------: | :-------------- |
| [01] | `S/02.Color Swatches/00.Illustrator & InDesign/00.Base Swatch.ai` | `00.Base Swatch 2.ai` | 62,156 | `d77dd24c5fc6` |
| [02] | `S/02.Color Swatches/00.Illustrator & InDesign/01.Expanded Colors.ai` | `01.Expanded Colors 2.ai` | 103,008 | `5fa1c732ff37` |
| [03] | `T/02.Documentation Templates/01.Business Related Templates/Architecture Proposal Template Design Layout.indt` | `Architecture Proposal Template Design Layout 2.indt` | 5,922,816 | `b3e3b64b574b` |
| [04] | `T/04.Architectural Documentation Templates/Site Analysis & Mapping Checklist.pdf` | `mapping checklist_LandSpace Architecture.pdf` | — | `e2e2367fc2bc` |

Hash-equal " 2" twins (sha256 of both files equal, 2026-09-11 22:35; retired under [06] row M03 [12]): `Bold-Brand-Guidelines_Template 2.indd` (8,859,648, `f5e4c48c6bcb…`), `Bold-Brand-Guidelines_Template-Legacy 2.idml` (9,801,332, `68e605c0248d…`), `Brand-Presentation-Template 2.indd` (3,219,456, `08ae95717fba…`), `Brand-Presentation-Template-Legacy 2.indd` (3,190,784, `326688ac097a…`), `Brand-strategy-Questionnaire 2.indd` (4,460,544, `e5b5e081ac54…`), `Brand-strategy-Questionnaire 2.idml` (235,955, `5bb369348877…`), `Brand-Strategy-Summary 2.indd` (8,396,800, `6197d7ca3879…`), `Brand-Strategy-Summary 2.idml` (1,191,967, `154cb394e553…`), `Creative-Direction_Template-Legacy 2.idml` (340,333, `3ef327e277d8…`); 12 licence-text twins, all hash-equal to the file without " 2": ten `FONTS/DM_Sans|DM_Mono/OFL 2.txt` (Brand Presentation, Brand Strategy ×2, Creative Directions, Project Proposal, Service and Pricing Guide, Return Brief, Project Onboarding, Project Offboarding), `Branding guidelines template package/license-README-FIRST 2.txt`, and `D/03.Design Assets/01.Overlays. Effects, & Maps/00.Graphical Overlays/01.Artistic Overlays/03.Digital Style Effects/03.Glitch Effects/Package 1/Personal-License-graphic-assets 2.txt`. `Project Pages/54 copy.indd` (1,310,720) differs from `54.indd` (954,368): both stay. The other 113 " 2" files in `T` whose kept twin exists (`.ttf` 54, `.jpg` 19, `.png` 15, `.pdf` 12, `.ai` 10, `.svg` 1, `.key` 1, `.indt` 1; `os.walk` + `sha256`, 23:20): 65 hash-equal (the 54 DM Sans/DM Mono `FONTS/… 2.ttf`, the `.indt` of row [03], Gradient Backgrounds, Grid-Example, the Typefool preview and Terms PDFs), 2 differ and stay (`Project Presentation Template/Links/AUVI_socialmediapost_Tekengebied 1 kopie 2.png` 469,723 vs 837,910; `Studio RBA Portfolio/Links/Glenavon 2.jpg` 2,524,454 vs 2,468,230), 46 are placeholder pairs not hashed (Typefool `Links/*.jpg|png`, the Legacy `.ai` pairs, `Proposal_Template 2.key`; 45 size-equal, `Mila_socialmediapost_Tekengebied 1 kopie 2.png` 2,271,530 vs 2,789,725 differs by size).

### [02.3]-[FORKS_AND_PROFILES]

| [INDEX] | [FILE] | [BYTES] | [DATE] | [FACT] |
| :-----: | :----- | ------: | :----- | :----- |
| [01] | `D/My Default Profile.ai` | 65,165,231 | 2025-03-09 | 94 swatches, 167 symbols, 16 styles; sRGB IEC61966-2.1; the source named by `default-palette.json` (row 06: `source.path`, sha `1895879942…`) |
| [02] | `S/99.Default Profiles/00.Adobe Illustrator/My Default Profile.ai` | 65,175,370 | 2025-01-28 | 94 swatches, 167 symbols, 33 styles |
| [03] | `S/02.Color Swatches/My Color Palette.ai` | 41,485,537 | 2025-02-28 | 143 swatches (5 groups), 224 symbols, 38 styles, 8 gradients, 6 patterns; sRGB; canonical |
| [04] | `S/99.Default Profiles/00.Adobe Illustrator/AI28Settings_Nov 19, 2024_13 56` | 82,234,851 | 2024-11-19 | placeholder (`compressed,dataless`); Illustrator 28 settings export |
| [05] | `S/99.Default Profiles/Shared/Default Palette.ase` | 4,412 | 2026-09-11 | the unified swatch set ([03.1]); placed, never rebuilt |
| [06] | `S/99.Default Profiles/Shared/default-palette.json` | — | 2026-09-11 | records `source` = [01], `workingProfile` = Adobe RGB (1998), conversion ImageMagick 7.1.2-29 + LittleCMS relative + BPC |
| [07] | `S/99.Default Profiles/InDesign/Default Template.indt` | — | 2026-09-11 | `C/tooling/indesign-template-evidence.json`: 612×792 pt, baseline 15 pt start 7.216, RGB profile Adobe RGB (1998), CMYK GRACoL2013_CRPC6, 22 paragraph styles all Source Sans 3 (11/15 body, 9/12 caption, 18/21 H1, 30/33 title), 2 palette groups, preflight embedded |
| [08] | `<illustrator.supportFolder>/en_US/Swatches/Default Palette.ase` | 4,412 | 2026-09-11 16:23 | the only user swatch library; sha256 equal to [05] (`shasum -a 256`, 2026-09-14); `Window > Swatch Libraries > User Defined` shows it; `Brushes/`, `Symbols/`, `Graphic Styles/` beside it are empty |

### [02.4]-[ICONJAR]

| [INDEX] | [PATH] | [FILES] | [KB] |
| :-----: | :----- | ------: | ---: |
| [01] | `S/98.IconJar Folder/IconJar.ijlibrary` | 7,740 | 2,808 |
| [02] | `S/98.IconJar Folder/IconJar-backup-08-06-2024@12-39-11.ijlibrary` | 7,740 | 2,760 |
| [03] | `D/04.Software Related Assets/IconJar-backup-07-25-2026@04-42-16.ijlibrary` | 3 | 116 |

### [02.5]-[NAME_DEFECTS]

| [INDEX] | [CURRENT] | [TARGET] |
| :-----: | :-------- | :------- |
| [01] | `D/04.Architectural Assets/01.Rrhino Assets` | `01.Rhino Assets` |
| [02] | `D/03.Design Assets/01.Overlays. Effects, & Maps` | `01.Overlays, Effects, & Maps` |
| [03] | `T/00.Grid Templates/00.General/Utillity Patterns - Grid Styles` | `Utility Patterns - Grid Styles` |
| [04] | `D/03.Design Assets/03.Pattern Assets/98.PAT Files/Utillity Grid Patterns` | `Utility Grid Patterns` |
| [05] | `S/01.Brush Sets/00.Adobe Illustrator Brushes/Seemless Celtic Patterns` | `Seamless Celtic Patterns` |
| [06] | `S/00.Automation Assets/01.Adobe Photoshop/VIsual Effects PSD` | `Visual Effects PSD` |
| [07] | `D/03.Design Assets/07.Figma Assets/Color Palletes (400+)` | `Color Palettes (400+)` |
| [08] | `T/00.Grid Templates/00.General/Grid Backgrounds Pack/PAT FIle Version` | `PAT File Version` |
| [09] | `T/04.Architectural Documentation Templates/ArchAdema Mega Pack` | `ArchAdemia Mega Pack` |
| [10] | `D/04.Architectural Assets/03.CAD Assets/05.Templates/ArchAdema Mega Pack` | `ArchAdemia Mega Pack` |
| [11] | `T/02.Documentation Templates/00.General/Verlag Magazine/Template/Verglag_Template.{indd,indt,idml}` | `Verlag_Template.{indd,indt,idml}` |
| [12] | `S/02.Color Swatches/01.Photoshop/Gradient Maps/Design Syndrome Gradient Map Pack/DERSIGN SYNDROME GRADIENT MAP PACK.grd` | `Design Syndrome Gradient Map Pack.grd` |
| [13] | `S/01.Brush Sets/00.Adobe Illustrator Brushes/Spot Texture & Painterly Brush Sets/Von Glitshcka Spot Texture Brush Set.ai`, `Von Glitshcka Painterly Brush Set.ai` | `Von Glitschka …` (the file, not a folder; `Drag and Draw Brushes by Von Glitschka.ai` is already correct) |
| [14] | `D/04.Architectural Assets/98.SketchUp Assets/00.Skletchup Models` | `00.SketchUp Models` |
| [15] | `D/03.Design Assets/02.Texture Assets/02.PBR Textures/Terrazo` | `Terrazzo` |
| [16] | `T/02.Documentation Templates/01.Business Related Templates/Project Admin Templates/InDesign/US-Letter/Style 02/{Payment-Reciept_Style-02_US-Letter.indd, Legacy-Files/Payment-Reciept_Style-02_US-Letter.idml}` | `Payment-Receipt_…` |
| [17] | `T/02.Documentation Templates/01.Business Related Templates/The Perfect Proposal/Templates/01_Full Template/InDesign-Template/Oudated-InDesign-Version.idml`, `02_Single Page Template/InDesign-Single-Page-Template/SIngle-Page-Template.indd` | `Outdated-InDesign-Version.idml`, `Single-Page-Template.indd` |
| [18] | trailing space: `D/04.Architectural Assets/02.Grasshopper Assets/01.Clusters `, `…/00.Skletchup Models/Tattu Edinburgh Restaurant Interior `, `D/03.Design Assets/02.Texture Assets/00.Stylized Textures/00.Dyed Colors `, 45 leaf folders under `02.PBR Textures/` (Tiles 13, Metal 9, Sidewalk 8, Flooring 4, Terrazo 2, Fabric 2, Stone 2, Marble, Wall Cladding, Roofing, Earth, Plaster); 48 directories in all (`fd -H -t d ' $'`, 23:05) | same name without the trailing space |
| [19] | double space: `T/03.PowerPoint Templates/Adv. PowerPoint - Resource Files/2. Adv. PowerPoint  - Exercise Decks - Filled` (the only directory; 49 directories with rows [18]–[19]) | single space |
| [20] | 181 files whose names end with a space or contain two consecutive spaces (`fd -H -t f '( $\|  )'`, 23:05: 181, none with a trailing space) | collapsed name, one rule in `organize.py` |
| [21] | `D/03.Design Assets/99.Art/00.General/00.General Illustrations/Medevial Armored Knights.ai` (9,149,492 B, 2025-03-05) | `Medieval Armored Knights.ai`; moved by M04 row [16] after the rename |
| [22] | `D/03.Design Assets/07.Figma Assets/Harmony Color Set (420) .fig` (474,187,904 B, local; a space before the extension) | `Harmony Color Set (420).fig`; the `organize.py` space rule also strips a space before the extension |

### [02.6]-[DESIGN_TOOLS_FOLDER]

`M` = 26,827,640 KB (`du -sk`, 23:02; `M/setup` 22,920,884).

| [INDEX] | [PATH] | [KB] | [CONTENT] | [VERDICT] |
| :-----: | :----- | ---: | :-------- | :-------- |
| [01] | `M/setup/photoshop/brushes` | 14,253,368 | generated ABR libraries, never imported | delete after research copy |
| [02] | `M/setup/photoshop/{prepare-brush-libraries.py,brush-preparation.json,brush-payload-identities.json}` | 60 | generator and manifests | delete after research copy |
| [03] | `M/setup/patterns/*.pat` (10), `Super Riso 300 dpi.asl` | 8,604,000 | generated, never imported | delete after research copy |
| [04] | `M/setup/patterns/illustrator`, `prepare-pattern-libraries.py` | 45,940 | collectors, never ran | delete after research copy |
| [05] | `M/setup/illustrator` | 836 | `default-template.ai` 141,959 B, `Default Toolbar`, `Previous Toolbar`, `Default Style - 3 px.ai`, `Default Style - 3 px RGB.ai`, `build-brush-libraries.{jsx,applescript}`, `brush-library-manifest.json`, `brush-variant-manifest.json`, `Noise Wave - Vector Tile.svg`, empty `brush-libraries/`, and `~ai-56f3f3e4-c6b0-4f1a-97a9-d499af1ee7f6_.tmp` (509,971 B, 2026-09-11 22:10:37; an Illustrator temporary save beside `default-template.ai`; no Illustrator process at 23:25, `pgrep -f "Adobe Illustrator"` matches no app) | delete after research copy (the toolbar is on Drive, sha `386fdd17…`); the `.tmp` is not copied |
| [06] | `M/fonts/google` | 1,898,000 | 1,687 family directories, 3,125 files = 3,124 font files + the `.typeface-location` marker at the root (`find -type f`, 23:02) | keep ([07] row [05]) |
| [07] | `M/fonts/owned` | 48,136 | 434 files = 425 font programs (299 `.ttf`, 29 `.TTF`, 97 `.otf`) + 9 `_LicenseAgreement.txt`, 54 family directories; 393 of the 425 hash-equal to a file under `D/01.Fonts` (`shasum -a 256` over both sides, 23:02; `probe-cloud/owned.sha256`, `drive-fonts.sha256`) | keep |
| [08] | `M/tk9-v4/downloads` | 936,044 | `TK9-v4-plugin.zip` 9,478,402 B (20 members, every CRC32 equal to `M/tk9-v4/package`), `TK9-v4-update-videos.zip` 949,026,414 B (28 members, every CRC32 and size equal to [09]) | delete |
| [09] | `M/tk9-v4/update-videos` | 972,992 | 28 files; sha256 of all 28 equal to `D/00.General/Plugins/TK9/Update videos` (`shasum -a 256` over both trees, diff empty) | delete |
| [10] | `M/tk9-v4/package` | 11,120 | 20 files = Drive `Package` (13) + the 7 manuals absent from Drive (`1 - Read this first.pdf`, `2 - Installation.pdf`, `TK9-v4-Instructions-Manual.pdf`, `TK9-v4-release-notes.pdf`, `TK9 Example Actions V4.pdf`, `TK9 v4 Button Modifiers.pdf`, `End-User-License-Agreement.pdf`) | keep (the only copy of the manuals and EULA) |
| [11] | `M/backups` | 40,364 | `illustrator-2026-2026-09-11`, `indesign-2026-2026-09-11`, `photoshop-2026-2026-09-11`, `uxp` | keep |
| [12] | `M/color` | 96 | `palette-command.js` 24,063, `default-palette.json` 31,312, `illustrator.jsx` 14,637, `indesign.jsx` 10,827, `photoshop.jsx` 7,748, `native-common.jsx` 3,488 | keep until the L-I1, I2, and L-P2 swatch readbacks, then delete |
| [13] | `M/licenses/license.typeface-license` | 4 | Typeface licence (also in the Beta container and OneDrive `03.Personal`) | keep |
| [14] | `~/Library/Logs/design-tools` | 264 | 7 logs | keep |

### [02.7]-[RECOVERY_LEDGER_VERSUS_DRIVE]

| [INDEX] | [CLAIM] (`C/tooling/asset-recovery-sources.json`, `assets/catalogs/templates.md`) | [DRIVE_NOW] | [STATUS] |
| :-----: | :------------------------------------------------------------------------------ | :---------- | :------- |
| [01] | 16 Kelman report/brochure files moved into `Report-Grids/` and `Brochure-Grids/` with sha256 | all 16 present, sha256 equal | verified |
| [02] | 10 Kelman presentation files in `Presentation-Grids/` with sha256 | all 10 present, sha256 equal (`kelman_presentation_sources`, rehashed 23:10, 10/10) | verified |
| [03] | 193 slide files moved to `T/03.PowerPoint Templates/Infographics Mega Bundle/` | 193 sources absent, 178 targets present (193 − 15 retired) | verified |
| [04] | 15 numbered Keynote copies retired, retained originals byte-identical | 15 retained originals present, 15 candidates absent | verified (hash equality per the ledger's `candidate_sha256` rows) |
| [05] | Verlag `Template.7z` (61,740,392 B) retired; three members expanded beside it | `Verglag_Template.{indd 35,254,272; indt 35,254,272; idml 237,379}` present | verified |
| [06] | 7 medieval CSH restored with matching sha256 | 7 files present, local, sha256 equal to `cloud_finalization.restored_assets` (W1 292,222,443 … `Medieval empyre-maker.csh` 22,430,303; rehashed 23:10, 7/7) | verified |
| [07] | 68 files / 1,040,109,417 B removed, 15 empty folder roots removed | Drive trash not readable from this machine; cached DriveFS trash 324 entries (262 files, 62 folders); Google purges it 30 days after 2026-09-11; nothing in it is restored (the `TK9 V3` rows are [06.2] M11 row [58]) | ledger record |
| [08] | index: 80,912 files under `D` (99.Art excluded) | 158,793 − 77,735 (99.Art) = 81,058 | consistent (146 new `.DS_Store` and session files) |
| [09] | `01.Fonts` 1,595 placeholders at index time | 1,260 now | consistent (335 files materialised by the index read) |
| [10] | Default Template.indt fonts all Source Sans 3 | evidence file only; the file is a placeholder in `S/99.Default Profiles/InDesign` | not re-read here; typography rebuilds it |

## [03]-[PALETTE]

### [03.1]-[THE_UNIFIED_SWATCH_SET]

The unified swatch set exists and is placed, never rebuilt, extended, or redesigned by any lane; a value changes only through one adjudication row of [03.2], and the hash below is re-recorded after the last such row. One file, two identical copies (`shasum -a 256`, 2026-09-14):

| [INDEX] | [COPY] | [BYTES] | [MTIME] | [SHA256] |
| :-----: | :----- | ------: | :------ | :------- |
| [01] | `S/99.Default Profiles/Shared/Default Palette.ase` (the master) | 4,412 | 2026-09-11 16:23 | `afd7121a5f723da1632acc6c414ae5e8ad89ca8ac12644135267fb6bd1fa1302` |
| [02] | `<illustrator.supportFolder>/en_US/Swatches/Default Palette.ase` (the only user swatch library; `Window > Swatch Libraries > User Defined` lists it) | 4,412 | 2026-09-11 16:23 | equal to [01] |

Content (ASE 1.0 block walk, 2026-09-14): 96 blocks, 92 colours, every colour model `RGB`, every colour type `Global` (type 0), two groups `Default Palette / Base` (13: `RGB White`, `RGB Black`, `True Gray`, `Red`, `Green`, `Blue`, `Yellow`, `Cyan`, `Magenta`, `Orange`, `Purple`, `Brown`, `Base Beige`) and `Default Palette / Expanded` (79, `Persian Plum` … `White Smoke`, one `Beige`, `Turquoise`); every value is Adobe RGB (1998) 8-bit (`default-palette.json` `workingProfile.name`). Conformance: the group names already carry the `<Library> / <Name>` form; the three swatch names that differ from `My Color Palette.ai` (`RGB White`, `RGB Black`, `Base Beige`) stay as named, one verdict: Illustrator overwrites an identically named swatch on load, so `RGB White` and `RGB Black` keep the document's factory `White` and `Black` intact and `Base Beige` keeps the Expanded `Beige` intact. Every colour row any lane writes (`palette.json`, the InDesign colours block, the Photoshop import, the Acrobat border) is a row of this file, read through `swatch` 0.4.0 (`swatch.parse`), never typed.

### [03.2]-[GAP_AGAINST_THE_CANONICAL_SOURCE]

Adjudicated one value at a time: the set's 92 values are the Adobe RGB conversion of `D/My Default Profile.ai` (row 06 of `default-palette.json`, sha `1895879942…`), while `S/02.Color Swatches/My Color Palette.ai` holds the same 92 names in sRGB with 66 values that differ once converted (`coloraide` `Color("srgb", v/255).convert("a98-rgb")`, clipped, ×255 rounded; largest deltas Magenta 96, Blue 91, Green 61, Cyan 53, Mardi Gras 44, Aquamarine 44, Spring Bud 44, Byzantine Blue 41); the live `Default Template.indt` carries the set's values (93 of 93 solids equal). `palettes.py` writes the table to `A/palette-drift.tsv` with two further columns, `verdict` (`master` or `source`) and `reason`; lane L-A fills them for each of the 66 differing rows, one row at a time, with a reason the specimen sheet or the conversion shows (a clipped channel, a hue the sheet proves wrong, a value the source document's own history contradicts); a row with no reason keeps the master's value, no verdict applies to a group or to the set at once, and each `source` verdict is written into the master through `swatch` and re-hashed into [03.1] and `A/library.sha256` before `palette.json` is emitted. Two further gap rows: `My Color Palette.ai`'s `Pastel Colors` (26) and `SIteplan Color Scheme 1` (9 unnamed spots) are outside the set and enter no template; `Power Line Dot Color` (255,182,0) is a graphic-style ink owned by 5e.

Columns: sRGB in `My Color Palette.ai` (d), its Adobe RGB conversion, the set, the live template, max channel delta set vs conversion.

| [INDEX] | [GROUP] | [NAME] | [KIND_IN_SOURCE] | [SRGB_CANONICAL] | [A98_TARGET] | [ASE_SHARED] | [TEMPLATE_LIVE] | [ASE_VS_TARGET] |
| :-----: | :------ | :----- | :--------------- | :--------------- | :----------- | :---------- | :--------------- | :------------: |
| [01] | Root | White | process | 255,255,255 | 255,255,255 | 255,255,255 | 255,255,255 | 0 |
| [02] | Root | Black | process | 0,0,0 | 0,0,0 | 0,0,0 | 0,0,0 | 0 |
| [03] | Root | True Gray | process | 128,128,128 | 127,127,127 | 127,127,127 | 127,127,127 | 0 |
| [04] | Base | Red | process | 255,36,0 | 220,41,10 | 204,53,41 | 204,53,41 | 31 |
| [05] | Base | Green | process | 102,255,0 | 164,255,60 | 152,194,78 | 152,194,78 | 61 |
| [06] | Base | Blue | process | 0,0,255 | 0,0,250 | 68,84,159 | 68,84,159 | 91 |
| [07] | Base | Yellow | process | 255,255,0 | 255,255,60 | 242,233,59 | 242,233,59 | 22 |
| [08] | Base | Cyan | process | 0,255,255 | 144,255,255 | 144,202,218 | 144,202,218 | 53 |
| [09] | Base | Magenta | process | 255,0,255 | 219,0,250 | 162,83,154 | 162,83,154 | 96 |
| [10] | Base | Orange | process | 255,121,0 | 226,120,28 | 216,119,45 | 216,119,45 | 17 |
| [11] | Base | Purple | process | 111,45,168 | 98,49,164 | 98,63,146 | 98,63,146 | 18 |
| [12] | Base | Brown | process | 96,56,19 | 87,59,28 | 88,60,31 | 88,60,31 | 3 |
| [13] | Base | Beige | process | 245,245,220 | 245,245,220 | 244,244,219 (as `Base Beige`) | 244,244,219 | 1 |
| [14] | Expanded | Persian Plum | spot | 112,28,28 | 97,34,34 | 97,34,34 | 97,34,34 | 0 |
| [15] | Expanded | Carmine | spot | 150,0,24 | 128,0,30 | 128,33,35 | 128,33,35 | 33 |
| [16] | Expanded | Turkish Red | spot | 169,17,1 | 144,24,8 | 144,35,39 | 144,35,39 | 31 |
| [17] | Expanded | Persian Red | spot | 204,51,51 | 176,54,54 | 176,54,54 | 176,54,54 | 0 |
| [18] | Expanded | Vermilion | spot | 227,66,52 | 197,68,56 | 197,68,56 | 197,68,56 | 0 |
| [19] | Expanded | Phthalo Green | spot | 18,53,36 | 37,56,41 | 37,56,41 | 37,56,41 | 0 |
| [20] | Expanded | Pakistan Green | spot | 0,64,26 | 37,66,34 | 40,66,38 | 40,66,38 | 4 |
| [21] | Expanded | Hunter Green | spot | 53,94,59 | 70,94,63 | 70,95,64 | 70,95,64 | 1 |
| [22] | Expanded | Fern Green | spot | 79,121,66 | 94,120,71 | 94,120,71 | 94,120,71 | 0 |
| [23] | Expanded | Sea Green | spot | 46,139,87 | 87,138,90 | 86,138,90 | 86,138,90 | 1 |
| [24] | Expanded | Islamic Green | spot | 19,136,8 | 78,135,35 | 80,136,73 | 80,136,73 | 38 |
| [25] | Expanded | Avocado | spot | 86,130,3 | 101,129,32 | 101,130,66 | 101,130,66 | 34 |
| [26] | Expanded | Apple Green | spot | 141,182,0 | 153,181,42 | 153,182,75 | 153,182,75 | 33 |
| [27] | Expanded | Yellow Green | spot | 154,205,50 | 169,204,69 | 168,201,75 | 168,201,75 | 6 |
| [28] | Expanded | Spring Bud | spot | 167,252,0 | 195,252,59 | 183,208,72 | 183,208,72 | 44 |
| [29] | Expanded | Pistachio | spot | 147,197,114 | 162,196,118 | 162,196,118 | 162,196,118 | 0 |
| [30] | Expanded | Space Cadet | spot | 30,41,82 | 38,45,82 | 38,45,82 | 38,45,82 | 0 |
| [31] | Expanded | Prussian Blue | spot | 0,49,83 | 30,52,83 | 34,53,82 | 34,53,82 | 4 |
| [32] | Expanded | Royal Blue | spot | 0,35,102 | 22,40,100 | 41,48,97 | 41,48,97 | 19 |
| [33] | Expanded | Zaffre | spot | 0,20,168 | 15,27,163 | 49,57,143 | 49,57,143 | 34 |
| [34] | Expanded | Persian Blue | spot | 28,57,187 | 43,60,182 | 62,75,155 | 62,75,155 | 27 |
| [35] | Expanded | Medium Persian Blue | spot | 0,103,165 | 58,103,162 | 58,103,163 | 58,103,163 | 1 |
| [36] | Expanded | Byzantine Blue | spot | 52,87,213 | 66,88,208 | 79,94,167 | 79,94,167 | 41 |
| [37] | Expanded | Saffron | spot | 244,196,48 | 231,195,66 | 231,195,64 | 231,195,64 | 2 |
| [38] | Expanded | Gold | spot | 255,215,0 | 244,214,50 | 243,214,50 | 243,214,50 | 1 |
| [39] | Expanded | Canary | spot | 255,239,0 | 250,238,56 | 249,237,59 | 249,237,59 | 3 |
| [40] | Expanded | Caribbean Current | spot | 0,109,111 | 61,109,110 | 61,109,110 | 61,109,110 | 0 |
| [41] | Expanded | Dark Cyan | spot | 0,139,139 | 78,138,138 | 78,138,138 | 78,138,138 | 0 |
| [42] | Expanded | Persian Green | spot | 0,166,147 | 93,164,146 | 93,164,146 | 93,164,146 | 0 |
| [43] | Expanded | Tiffany Blue | spot | 129,216,208 | 159,215,207 | 158,207,202 | 158,207,202 | 8 |
| [44] | Expanded | Turqoise → Turquoise | spot | 64,224,208 | 136,223,207 | 136,197,193 | 136,197,193 | 26 |
| [45] | Expanded | Aquamarine | spot | 127,255,212 | 175,255,213 | 171,211,191 | 171,211,191 | 44 |
| [46] | Expanded | Rose Red | spot | 194,30,86 | 166,35,85 | 166,35,86 | 166,35,86 | 1 |
| [47] | Expanded | Red-Violet | spot | 199,21,133 | 170,28,129 | 170,29,129 | 170,29,129 | 1 |
| [48] | Expanded | Persian Rose | spot | 254,40,162 | 219,44,158 | 206,61,145 | 206,61,145 | 17 |
| [49] | Expanded | Persian Pink | spot | 247,127,190 | 220,126,186 | 213,128,177 | 213,128,177 | 9 |
| [50] | Expanded | Bittersweet | spot | 254,111,94 | 224,110,95 | 215,110,96 | 215,110,96 | 9 |
| [51] | Expanded | Salmon Pink | spot | 255,145,164 | 230,144,162 | 221,143,162 | 221,143,162 | 9 |
| [52] | Expanded | Desert Sand | spot | 237,201,175 | 227,200,175 | 227,201,176 | 227,201,176 | 1 |
| [53] | Expanded | Aerospace Orange | spot | 255,79,0 | 222,80,19 | 209,83,43 | 209,83,43 | 24 |
| [54] | Expanded | Spanish Orange | spot | 232,97,0 | 204,97,23 | 203,98,45 | 203,98,45 | 22 |
| [55] | Expanded | Tangerine | spot | 242,133,0 | 217,132,31 | 215,132,47 | 215,132,47 | 16 |
| [56] | Expanded | Xanthous | spot | 241,180,47 | 225,179,63 | 224,178,61 | 224,178,61 | 2 |
| [57] | Expanded | Persian Orange | spot | 217,144,88 | 198,143,92 | 198,143,91 | 198,143,91 | 1 |
| [58] | Expanded | Russian Violet | spot | 50,23,77 | 48,29,77 | 48,32,77 | 48,32,77 | 3 |
| [59] | Expanded | Persian Indigo | spot | 50,18,122 | 47,25,119 | 54,46,115 | 54,46,115 | 21 |
| [60] | Expanded | Tyrian Purple | spot | 102,2,60 | 88,9,61 | 87,22,62 | 87,22,62 | 13 |
| [61] | Expanded | Byzantium | spot | 112,41,99 | 98,45,97 | 98,45,97 | 98,45,97 | 0 |
| [62] | Expanded | Eminence | spot | 108,48,130 | 96,51,127 | 96,51,127 | 96,51,127 | 0 |
| [63] | Expanded | Mardi Gras | spot | 136,0,133 | 116,0,129 | 114,44,127 | 114,44,127 | 44 |
| [64] | Expanded | Amethyst | spot | 153,102,204 | 140,102,200 | 131,104,169 | 131,104,169 | 31 |
| [65] | Expanded | Mulberry | spot | 197,75,140 | 172,76,137 | 172,77,137 | 172,77,137 | 1 |
| [66] | Expanded | French Mauve | spot | 212,115,212 | 189,114,208 | 174,118,175 | 174,118,175 | 33 |
| [67] | Expanded | Black Bean | spot | 61,12,2 | 55,20,10 | 54,27,22 | 54,27,22 | 12 |
| [68] | Expanded | Seal Brown | spot | 89,38,11 | 79,42,21 | 79,44,25 | 79,44,25 | 4 |
| [69] | Expanded | Sepia | spot | 112,66,20 | 101,68,30 | 102,69,34 | 102,69,34 | 4 |
| [70] | Expanded | Coffee | spot | 111,78,55 | 103,79,59 | 103,80,60 | 103,80,60 | 1 |
| [71] | Expanded | Chocolate | spot | 123,63,0 | 109,65,15 | 109,67,35 | 109,67,35 | 20 |
| [72] | Expanded | Golden Brown | spot | 153,101,21 | 139,101,35 | 140,102,48 | 140,102,48 | 13 |
| [73] | Expanded | Copper | spot | 184,115,51 | 166,114,58 | 167,114,57 | 167,114,57 | 1 |
| [74] | Expanded | Caramel | spot | 204,127,59 | 185,126,66 | 185,126,65 | 185,126,65 | 1 |
| [75] | Expanded | Camel | spot | 193,154,107 | 181,153,109 | 181,152,109 | 181,152,109 | 1 |
| [76] | Expanded | Khaki | spot | 195,176,145 | 188,174,145 | 189,174,145 | 189,174,145 | 1 |
| [77] | Expanded | Sand | spot | 194,178,128 | 188,176,129 | 188,176,130 | 188,176,130 | 1 |
| [78] | Expanded | Tan | spot | 210,180,140 | 201,179,141 | 201,178,141 | 201,178,141 | 1 |
| [79] | Expanded | Wheat | spot | 245,222,179 | 238,221,180 | 238,221,180 | 238,221,180 | 0 |
| [80] | Expanded | Almond | spot | 239,222,205 | 234,221,204 | 234,221,205 | 234,221,205 | 1 |
| [81] | Expanded | Beige (duplicate of [13]) | spot | 245,245,220 | 245,245,220 | 244,244,220 | 244,244,220 | 1 |
| [82] | Expanded | Bone | spot | 227,218,201 | 224,217,200 | 223,217,200 | 223,217,200 | 1 |
| [83] | Expanded | Gunmetal | spot | 42,52,57 | 49,55,59 | 49,55,59 | 49,55,59 | 0 |
| [84] | Expanded | Davy's Gray | spot | 85,85,85 | 86,86,86 | 86,86,86 | 86,86,86 | 0 |
| [85] | Expanded | Dim Gray | spot | 105,105,105 | 105,105,105 | 105,105,105 | 105,105,105 | 0 |
| [86] | Expanded | Battleship Gray | spot | 132,132,130 | 131,131,129 | 131,131,129 | 131,131,129 | 0 |
| [87] | Expanded | French Gray | spot | 190,191,197 | 189,190,195 | 189,190,195 | 189,190,195 | 0 |
| [88] | Expanded | Silver | spot | 192,192,192 | 191,191,191 | 191,191,191 | 191,191,191 | 0 |
| [89] | Expanded | Platinum | spot | 229,228,226 | 228,227,225 | 228,227,225 | 228,227,225 | 0 |
| [90] | Expanded | Seasalt | spot | 247,247,247 | 247,247,247 | 247,247,247 | 247,247,247 | 0 |
| [91] | Expanded | Snow | spot | 255,250,250 | 254,250,250 | 254,250,250 | 254,250,250 | 0 |
| [92] | Expanded | White Smoke | spot | 245,245,245 | 245,245,245 | 245,245,245 | 245,245,245 | 0 |

### [03.3]-[COMPANION_LIBRARIES]

| [INDEX] | [FILE] | [CONTENT] | [PLACEMENT] |
| :-----: | :----- | :-------- | :---------- |
| [01] | `S/02.Color Swatches/00.Illustrator & InDesign/Kelman Colorplan/Colorplan swatches.ase` (2,348 B) | ASE 1.0, 51 blocks, 49 `RGB` colours, all `Global`, one group `Colorplan`, sRGB floats (every Kelman IDML declares `RGBProfile="sRGB IEC61966-2.1"` in `designmap.xml`) | stays where it is, its own library: Illustrator `Open Swatch Library > Other Library` per document, Photoshop `Import Swatches` in job L-P2; not merged into the set |
| [02] | `S/02.Color Swatches/01.Photoshop/Show It Better Color Palette {1,2,3}.aco` (204 B each) | version 1 block of 7 colours each plus a version 2 block with names `1`…`7`; 20 CMYK (space 2), 1 HSB (space 1) | Photoshop `Import Swatches` in L-P2; Illustrator and InDesign have no ACO route and receive nothing from them |
| [03] | `S/02.Color Swatches/My Color Palette.ai` (41,485,537 B, 2025-02-28) | 143 swatches in 5 groups, 8 gradients, 6 patterns, 302 brushes, 224 symbols, 38 graphic styles | the pattern, style, and symbol source of `cloud-libraries.md` [05.1] row [13], [06.1], [06.3]; its colours enter nothing (the set is the colour source, [03.1]) |
| [04] | `S/02.Color Swatches/00.Illustrator & InDesign/{00.Base Swatch, 01.Expanded Colors}.ai` | the two group documents | stay; their " 2" twins are M03 rows [08]–[09] |


### [03.4]-[DELIVERABLES_AND_IMPORT_ROUTES]

| [INDEX] | [DELIVERABLE] | [WRITER] | [DESTINATIONS] | [IMPORT_ROUTE] (current Adobe pages, dates in [10]) |
| :-----: | :------------ | :------- | :------------- | :---------------------------------------------------- |
| [01] | `A/palette.json` | `library/palettes.py`: `swatch.parse` of the set ([03.1]) → 92 rows `{name, group, a98: [r,g,b], cmyk: [c,m,y,k], source: "Default Palette.ase", sha256}`; the `cmyk` column is the Adobe RGB value converted to GRACoL2013 CRPC6 through `pillow` `ImageCms` (`coloraide` a98-rgb → Lab, `ImageCms.buildTransform(createProfile("LAB"), "S/99.Default Profiles/Shared/Profiles/GRACoL2013_CRPC6.icc", "LAB", "CMYK", Intent.RELATIVE_COLORIMETRIC, Flags.BLACKPOINTCOMPENSATION)`, integer percentages); alias keys `accent` = `Medium Persian Blue`, `primary accent blue` = `Medium Persian Blue`, `ink` = `RGB Black`, `field` = `Seasalt`, each `{of: <name>}` and no value of its own | repo (`A/`); `S/99.Default Profiles/Shared/default-palette.json` replaced by it | none (data) |
| [02] | `Default Palette.ase` | nothing: the master and the user copy exist and are hash-equal ([03.1]); the inventory lane writes the master's row into `A/library.sha256` and marks the library `Persistent` (`cloud-libraries.md` [08.2] row 03) | as [03.1] | Illustrator: `Window > Swatch Libraries > User Defined > Default Palette`; swatches drag or `Add To Swatches`; identically named swatches overwrite on load (Adobe, 2025-10-27) |
| [03] | Illustrator template swatches | job L-I1: `illustrator-scripts` `import_swatches {palette: A/palette.json, groups: ["Default Palette / Base", "Default Palette / Expanded"]}` into the Adobe RGB template, then `inspect` readback (92 rows, two groups) | the template `.ait` (gui-stream owns the file) | DOM (`swatchGroups.add`, `swatches.add`, `SwatchGroup.addSwatch`); no panel |
| [04] | InDesign swatch rows | `palettes.py` emits the `colors` block of `styles-graph.json` (name, `ColorModel.PROCESS`; `ColorSpace.CMYK` from the `cmyk` column for every print family, `ColorSpace.RGB` from the `a98` column for `Digital 3840x2160`; the three aliases as swatches `Accent`, `Ink`, `Field`; `gui-indesign.md` [07] rows 01–03) | typography's `styles-graph.json`; `build_template` creates colours first | DOM; the ASE is also loadable through `Window > Color > Swatches > panel menu > Load Swatches` (all) or `New Color Swatch > Color Mode > Other Library` (selected); accepted sources INDD, INDT, AI, EPS, ASE (Adobe InDesign pages 2026-06-02 and 2026-08-07) |
| [05] | Photoshop swatches | job L-P2: `Window > Swatches > panel menu > Import Swatches` on the set, the Colorplan ASE, then each `.aco` (Adobe, 2025-10-27: "Import swatches from other applications to Photoshop by selecting Import Swatches option from Swatches panel"); Photoshop writes `<photoshop.prefsFolder>/Swatches.psp` on quit | that `Swatches.psp` (present, 2026-09-11); no export back to Drive (the master already exists) | panel; `list_presets {kind: 'swatch'}` readback: 92 + 49 + 21 names |
| [06] | Acrobat forms border | `palette.json` key `accent` | acrobat `preferences.json` | data |

Excluded route: Creative Cloud Libraries (`Add selected Swatches … to my current Library`, `Add To My Library`) is never used. No `Colorplan `-prefixed or `Show It Better `-prefixed group and no Neutral, Accent, or Diagram Tint ramp is built: the set is the user's and is placed as it stands.

### [03.5]-[VERIFIER]

| [INDEX] | [CHECK] | [PASS] |
| :-----: | :------ | :----- |
| [01] | `swatch.parse` of the master equals `palette.json` (name, group, model `RGB`, type `Global`, values within 1/255) and the master's sha256 equals [03.1] row [01] | 92 rows, 0 mismatches, hash equal |
| [02] | `inspect` after L-I1 equals `palette.json` (92 rows in two groups; names and 8-bit values; `GetSwatches` caps at 100 names and is not used) | 0 mismatches; every name unique |
| [03] | `inspect` on the template: `colorProfileName` = `Adobe RGB (1998)` | equal |
| [04] | adjudication record `A/palette-drift.tsv` ([03.2] table) written | 92 rows; every one of the 66 differing rows carries a `verdict` and a `reason`; the master's hash in [03.1] equals `shasum -a 256` of the file after the last `source` row |
| [05] | InDesign `document.colors` after I2 contains the 92 names plus `Accent`, `Ink`, `Field` with the `palette.json` values | 0 mismatches |
| [06] | Photoshop `list_presets {kind: 'swatch'}` after L-P2 lists the 92 + 49 + 21 names | present |

## [04]-[TEMPLATES]

### [04.1]-[INVENTORY_FACTS]

`library/idml.py` reads every IDML on Drive through `simpleidml` (architecture [02] row 10; `simpleidml 1.3.2a0` is in the repo `.venv`); the inspection below is the `zipfile` read of `Resources/Fonts.xml` and `designmap.xml` run 2026-09-11 over 314 files (all local), 0 errors: DOMVersion 14.0 ×136, 15.0 ×78, 19.0 ×34, 18.0 ×33, 17.0 ×24, 15.1 ×3, 20.0 ×2, 12.1 ×1 (`Advertising Storyboard Template - Portrait.idml`), 19.4 ×1, 19.5 ×1, 20.4 ×1 (recounted 23:15). Font references (family occurrences over 314 files): Myriad Pro 314, Minion Pro 314, Kozuka Mincho Pr6N 313 (InDesign defaults), Spartan 197 (Architectural Portfolio Mega Pack alone), Roboto 47, Inter 28, Futura PT 28, DM Sans 28, Adobe Arabic 27, Futura Md BT 16, Roobert PRO TRIAL 12, Futura LT 11, Neutra 9, Adobe Caslon Pro 9, Gilroy 8, Neue Haas Grotesk 7, Helvetica 7, Overused Grotesk 5. Roobert PRO TRIAL appears in 12 IDML across 12 folders (Typefool: Brand Discovery Brief v2 ×2, Brand Guidelines Template, Brand Identity Presentation, File Format Instructions v1 and v2, Logo Usage Guidelines v1, Project Presentation, Stylesheet v2 ×2, Design Portfolio v1 and v2). Kelman IDML reference Neue Haas Grotesk Text Pro (and Display Pro in the A4 2-column) plus the three defaults; Neue Haas Grotesk Display Pro (16) and Text Pro (6) are activated on Adobe Fonts (font inventory [07]). Fonts bundled inside packages: DM Sans OFL ×9 packages, DM Mono, Inter 4.0, Alegreya/Alegreya Sans (CreativePro), League Spartan (30X40), Gilroy EULA ×2.

### [04.2]-[VERDICTS]

Rules: `keep-modern` = job I5 opens the source in InDesign Beta, resolves fonts by the map in [04.3], relinks, saves the `.indd` in place and a `.indt` beside it, records the readback; `retire-idml` = trash the `.idml` twin after the modern save readback of its `.indd`; `collapse-v1` = trash the v1 folder after the v2 modern save readback, when v2's InDesign document set ⊇ v1's by name; `retire-twin` = trash the " 2"/copy file after `sha256` equality with the kept file; `report` = untouched, listed in the closeout report.

| [INDEX] | [SET] | [FILES] | [VERDICT] |
| :-----: | :---- | :------ | :-------- |
| [01] | Kelman `Report-Grids/` | 8 `.indd`, 2 `.idml`, 1 `.indt` (`fd`, 23:05: A4 1 Column and A4 2 Column with `.idml` twins; US Letter 2 Column, 3 Column, 3 Column f-Height, Portrait, White Paper, White Paper f-Height; `US Letter Portrait Grid System` has both `.indd` and `.indt` at 1,785,856 B) | keep-modern; retire-idml ×2; keep the `.indt`, retire the `.indd` after I5 confirms equal page and style counts |
| [02] | Kelman `Brochure-Grids/` | 3 `.indd`, `Brochure with Baseline & F-height.indt`, `US Letter Corporate Brochure Grid System/{.indd, Links/Pie Chart.ai}` | keep-modern; the `Links/` folder stays beside its `.indd` |
| [03] | Kelman `Presentation-Grids/` | 4 `.indd`, 4 `.idml`, `Tabloid Margins Grid.{ai,eps}` | keep-modern; retire-idml ×4; retire `Tabloid Margins Grid.eps` (the `.ai` master exists, 281,549 B) |
| [04] | Kelman `Colours Grid System Series/` | `6 × 9`, `8.5 × 11`, `A4`, `A5 Grid System.indd` | keep-modern (the free four-format family; feeds the size catalog's A4/A5/Letter/6×9 grids) |
| [05] | `00.General` singles | `InDesign Masterfile.indd`, `Swiss Typography Layout.ai`, `Cover Design Poster Layouts.ai`, `CreativePro 6x9 Book Template/` (43 files, `Document fonts/` bundled) | keep-modern for the two `.indd`; `.ai` untouched |
| [06] | Verlag Magazine | `Verglag_Template.{indd,indt,idml}` (rename [02.5] row 11), help PDF, `Fonts/FREE FONTS.txt` | keep-modern; 48 linked images are absent from the package (link read: 50 occurrences, 48 unique names, 0 present) → report as incomplete; retire-idml after save |
| [07] | Typefool branding (`01.Presentation Templates/Branding Related Presentations`, 25 IDML, 28 INDD = 23 distinct + 5 " 2" twins, 3 INDT; `fd`, 23:05) | v1/v2 pairs: Brand Discovery Brief, Brand Strategy Presentation, File Format Instructions (identical sizes), Logo Usage Guidelines, Massive Brand Guidelines; " 2" twins ×9 (5 `.indd`, 4 `.idml`) | keep-modern on v2 and singles; collapse-v1 ×5; retire-twin ×9; Roobert PRO TRIAL replaced per [04.3] |
| [08] | Typefool business (`02.Documentation Templates/01.Business Related Templates`, 32 IDML, 37 INDD, 4 INDT) | v1/v2 pairs: Design Services Contract, Invoice Template, Proposal Template, Single Page Proposal; `Legacy`/`Legacy-Files` IDML ×19; `Architecture Proposal … Layout 2.indt` twin (hash equal) | keep-modern; collapse-v1 ×4; retire-idml on every `Legacy` IDML after its `.indd` save; retire-twin ×1 |
| [09] | General Presentations (6 INDD, 8 IDML, 3 INDT) | Stylescapes v1/v2, Stylesheet v1/v2, `Presentation Board A0 Portrait 1` (`.indd` 111 MB + `.idml` 29 MB), 3 `.indt`, 2 Storyboard `.idml` (no `.indd`; Portrait is DOMVersion 12.1 with Courier, Landscape 18.0 with Avenir Next and Univers) | keep-modern; collapse-v1 ×2; retire-idml where an `.indd` exists; the two Storyboard IDML are converted to `.indd` by I5 (IDML opens as untitled, saved beside); Avenir Next and Univers resolve through `font-map.json` |
| [10] | Architectural presentations | ArchAdemia ×4 `.indd` (80 MB report), Competitions Archi 20 `.indd`, `Scholarship Boards.idml`, Sun Study `.indd` | keep-modern (fonts Futura PT via Adobe Fonts; Replica absent → substitute map) |
| [11] | Portfolios: Architectural Portfolio Mega Pack (`Adobe Indesign Versions/`, 133 INDD, 209 IDML; `fd`, 23:20) | `Project Pages/` 76 `.indd` (1–75 + `54 copy`) with 75 `.idml` twins and 75 PDF previews; `Project Title/` 57 `.indd`, 84 `.idml` (56 twins, 28 IDML-only); `Contents Page/` 50 `.idml` (no `.indd`) | keep; retire-idml for the 131 twins after a batch modern save; the 78 IDML-only files (Contents Page 50, Project Title 28) converted to `.indd` by the same batch; `54 copy.indd` kept (differs) |
| [12] | Portfolios: Design Portfolio v1/v2, Pack of Templates, Studio RBA Portfolio, Photography (2) | Pack of Templates: 112 folders, 621 files (255 jpg, 67 otf, 57 ttf, 60 png, 33 pdf, 32 psd, 0 indd; 72 of the 112 leaf folders hold 1–4 files, one PSD template with its previews and vendor fonts each; `fd`, 2026-09-15). Studio RBA Portfolio: 2 folders, 64 files (1 `.indd`, 53 jpg previews, 7 font files, 1 pdf) | keep-modern for Design Portfolio v2; collapse-v1; Pack of Templates is junk (M14 row [67]: one-file-per-folder nesting, no InDesign document); Studio RBA keep-modern on its `.indd`, the previews stay beside it; Photography untouched |
| [13] | Resumes | `Resume Template.indt`, Template 3, Template 4 (`.indd` + `.idml`) | keep-modern; retire-idml ×1 |
| [14] | `99.School & Personal Templates` | 12 files: `School Document Template.indd`, `My PowerPoint Template.pptx`, 7 `.gdoc`, `.gsite`, `.gform`, `.gsheet` | keep-modern for the `.indd` (user's own); the Google shortcuts untouched |
| [15] | `99.Default Profiles/InDesign/Default Template.indt` | live build, Source Sans 3 ([02.3] row 07) | replaced by typography's job I2 output; kept until I2 readback, then retire-twin |
| [16] | `99.Miscellaneous Templates/logo-package-artboards-{digital,print}-template.ait` | Logo Package Express artboard templates | keep (Illustrator; not part of the InDesign job) |
| [17] | `logo bank package` | 2,676 files: 1,399 `.jpg`, 1,245 `.ai`, 18 `.png`, 2 `.eps`, all placeholders (0 KB local), 1,411 folders of which 1,110 hold exactly two files (a `.jpg` beside its `.ai`), `… - google search_files` directories, no licence or source list (`fd`, 2026-09-15) | junk: a web scrape, trashed whole by M14 row [66]; excluded from hashing until then (`organize.py` skip list) |
| [18] | PowerPoint `Megapack 1`, `Megapack 2`, `Infographics Mega Bundle`, `General Templates`, `Adv. PowerPoint` | `Megapack 1` 18 `.pptx` (526 MB); `Megapack 2` 27 folders, 388 files (114 `.pptx`, 118 `.xml`, 112 `.png`, 17 `.psd`, 5 `.eps` twins); `Infographics Mega Bundle` 178 (89 `.pptx`, 87 `.key`, 2 `.txt`); `General Templates` 211; `Adv. PowerPoint` 27 | the deck files stay untouched; the L-B3 manifest organizes and cleans their folders alone (placement, naming, junk siblings) and reports the rows; no PowerPoint tool is in scope; `My PowerPoint Template.pptx` does not receive the palette in this branch; the five `27_Icons/*.eps` twins of same-stem `.ai` are M10 row [47] |

### [04.3]-[FONT_AND_LINK_RESOLUTION]

| [INDEX] | [RULE] |
| :-----: | :----- |
| [01] | Job I5 reads `document.fonts` (`status`) and `document.links` (`status`) before any change and writes them to `A/templates-inspection.tsv` (path, pack, folder depth, files in its folder, missing fonts, missing links, DOMVersion); lane L-B3 rolls the same walk up per purchased package into `A/templates-packages.tsv` (folders, files, files per leaf folder, formats, distinct InDesign documents, verdict) before M14 |
| [02] | A font bundled in the package resolves from a `Document Fonts` folder beside the document: "Fonts in a Document Fonts folder that is in the same location as an InDesign document are temporarily installed when the document is opened" (Adobe, InDesign fonts page, 2024-12-11). 69 directories named `Document fonts` exist under `T` (`fd -i`, 23:25), 62 of them beside an `.indd` (the Drive volume is case-insensitive APFS; `document FONTS` resolves the CreativePro folder). Nine Typefool packages keep their fonts in a `FONTS/` or `Inter-4.0/` folder beside the `Adobe InDesign/` folder, not beside the `.indd`: manifest M07 row [26] copies those files into `Document Fonts/` beside each `.indd` before I5 (Brand Presentation 12 fonts / 2 indd, Brand Strategy 24 / 4, Creative Directions 12 / 1, Project Offboarding 12 / 1, Project Onboarding 12 / 1, Project Proposal 12 / 2, Return Brief 12 / 1, Service and Pricing Guide 12 / 4, Style Guide Kit `Inter-4.0` 74 / 1); Verlag's `Fonts/` holds no font file (`FREE FONTS.txt` only) |
| [03] | Adobe Fonts families (Neue Haas Grotesk, Minion Pro, Futura PT, Adobe Caslon Pro, Roboto, Montserrat, Poppins, Noto Sans) resolve through the active subscription; missing = activation request through Creative Cloud, no download |
| [04] | `Roobert PRO TRIAL` and `Roobert` (14 references) → the house sans (typography round-1 pick) by `Find Font > Replace`; the map is `A/font-map.json` `{from, to, style-by-style}` |
| [05] | Web-kit and rip families (Helvetica 7, Futura LT/BT 36, Univers 1, Gilroy 8, Arnold 3, Less 3, Simplifica, TimeBurner, Berlin Sans FB) → the house family per role in `font-map.json`; never activated from `01.Fonts` |
| [06] | Every replacement is recorded as `{document, from, to, count}` in the inspection TSV; a template with a font the map lacks is reported, not saved |
| [07] | Links: `relink` to the package `Links/` folder by name; a link whose `.svg` became an `.ai` under M10 row [45] relinks to `<stem>.ai` (the M10 plan rows are the map); a link absent from the package (Verlag ×48) is reported; no download |
| [08] | Modern save: `document.save(path)` in place, then `saveACopy` as `.indt` with the same name; `.idml` twin retired after `app.open(indd)` readback (`fonts` all `INSTALLED`, `links` all `NORMAL` or reported) |

### [04.4]-[KELMAN_AND_TYPEFOOL_INPUTS_TO_THE_MASTER]

Inputs the library extracts (through `idml.py`) and hands to typography's `grid.json` builder:

| [INDEX] | [SOURCE] | [EXTRACTED] | [USE] |
| :-----: | :------- | :---------- | :---- |
| [01] | `A4 1 Column Report Grid System.idml` | 24 A4 pages, 31 paragraph styles, body 11.25/15, 1 parent | the report grid form (`text height = a + N·L`), leading-first sizing |
| [02] | `A4 2 Column Report Grid System.idml` | 26 pages, 29 styles, body 9.35/11, small 7.0125 | the dense variant |
| [03] | `Digital Presentation {Landscape,Portrait} Grid System v2.idml` | 16:9 digital presentations, 15 pt baseline (Kelman catalogue) | the digital INDT parent pages (title, section, image-led, text-led, comparison, diagram, grid) |
| [04] | `Letter Landscape`, `Tabloid Presentation Grid System.idml` | Letter and Tabloid landscape grids, 12 pt baseline (Tabloid) | the size catalog's Letter/Tabloid landscape rows |
| [05] | `Colours Grid System Series/*.indd` | A4, A5, US Letter, 6×9; 18 fields; 11 pt (A4), 8.5 pt (A5) baselines | the size catalog's four small formats |
| [06] | `US Letter … (with f-Height Grid).indd` ×2, `Brochure with Baseline & F-height.indt` | the f-height second grid | `grid.json` `fHeightOffset` per body face (recomputed from the real font's f ascender, not copied) |
| [07] | Typefool `Stylesheet Template v2` | digital 1920×3400 pt and A3 297×420 mm sheets; 15-field three-column grid | the digital size row check against the one digital canvas, `Digital 3840x2160` |
| [08] | Typefool branding set | 1920×1080 pt pages | the catalogue's digital row (`Digital 3840x2160`, the one digital canvas) |

### [04.5]-[POWERPOINT_AND_LOGO_BANK_RECOMMENDATION]

| [INDEX] | [ITEM] | [EVIDENCE] | [RECOMMENDATION] |
| :-----: | :----- | :--------- | :--------------- |
| [01] | `logo bank package` | 2,676 files, 0 B local, 1,110 two-file folders named `<subject> - google search_files`, no source list or licence file | trash whole (M14 row [66]): a scraped reference set, not purchased material, and no rebuild consumes it; never hashed (hashing would download 2,676 files) |
| [02] | `Megapack 1`, `Megapack 2`, `Infographics Mega Bundle`, `General Templates`, `Adv. PowerPoint` | 715 placeholders of 831 files; the Infographics slide move and 15 Keynote retirements are verified in [02.7] | the deck files stay untouched; their folders are organized and cleaned (placement, naming, junk siblings) by the L-B3 manifest and reported; PowerPoint theme work is outside this branch |

## [05]-[FONTS]

### [05.1]-[INVENTORY_FACTS]

| [INDEX] | [FACT] | [SOURCE] |
| :-----: | :----- | :------- |
| [01] | Drive `01.Fonts`: 2,340 files; 0 desktop placeholders; 1,260 placeholders are web/source formats; the two font inventories are the ones `illustrator-acrobat-cc.md` [07.6] names: `inputs/fonts/font-inventory.tsv` for the installed sources and the Stage 2 scan `.artifacts/creative-cloud/typography/drive-premium-fonts.tsv` for the premium folder | `illustrator-acrobat-cc.md` [07.6]; the walk in [02.1] |
| [02] | `M/fonts/owned`: 425 files, 54 family directories; 393 byte-identical to Drive; 32 unique (Futura-PT 6, Arnold 5, Acumin-Pro 4, Futura-LT-Pro 3, Berlin Sans 3, Gilroy 2, Minion-Pro 2, Less 2, 5 singles) | `inputs/fonts/font-inventory.tsv`; [09] row 30 |
| [03] | Drive-only desktop files: 211 (Inter 148 = two releases × three formats, Geist 20, Overused Grotesk 16, Roboto 16, Helvetica 5, Futura 3, Tor Grotesk Mix 3) | [09] row 30 (the two-sided hash comparison) |
| [04] | `M/fonts/google`: 1,687 family directories, 3,124 files; only `source-sans-3` (2 variable files, 3.052) is used; Typeface displays 7,066 native entries for it | `illustrator-acrobat-cc.md` [07.3] row 01; `C/tooling/typeface.md` |
| [05] | Typeface Beta 4.5.0 (5246), `com.criminalbird.typeface.beta`; locations: System (auto), `M/fonts/google`, `M/fonts/owned`; Prefer Temporary Activation on, clear on quit on, Remove Missing Fonts on; auto-activation Illustrator, InDesign, Other Apps; persistent set = Source Sans 3 ×16 styles | `defaults read com.criminalbird.typeface.beta` (23:02): `version = 5246`, `preferTemporaryActivation = 1`, `removeMissing = 1`, `restoreActivations = 1`, `autoActivation = (Illustrator, InDesign, System)`, `lastSelectedTag = "//Google"`, `lastBackupSaveDirectory` = the Drive backup path; the locations, the clear-on-quit setting, and the persistent set are not in `defaults`: `C/tooling/typeface.md` lines 103 and 129 (the tag export records the three imported locations) |
| [06] | Typeface tag backup: `S/99.Default Profiles/Typeface/default-typeface-tags.typeface-backup` (`lastBackupSaveDirectory`), 6 authored tags, 7 classification records, 2,581-entry activation record | `typeface.md` |
| [07] | Forge `modules/home/environments/media.nix:21` lists `M/fonts` (the parent) as a fontconfig directory; no reference to `fonts/google/source-sans-3` by path | `rg` over Parametric_Forge |
| [08] | Adobe Fonts: 55 families, 318 faces synced; Neue Haas Grotesk Display/Text Pro, Minion Pro 2.115, Futura PT 1.009 among them | font-inventory [07] |
| [09] | Typeface docs: fonts must be local (cloud placeholder files cannot be previewed); with `Remove Missing Fonts` on, a font moved to Trash is removed from the library and its tags are backed up internally and restored on re-import; a folder moved on the same disk is followed; `Relink Location` only for a folder moved elsewhere; temporary activations clear on quit while `Clear temporary activations on quit` is on; a backup restores tags by internal font data, "import your fonts first, then apply a backup file"; the app writes an automatic backup on every quit and keeps 10 days | typefaceapp.com missing-imports, backups, activation (read 2026-09-11 23:00) |

### [05.2]-[TYPEFACE_JOB_T1]

The three font sources and their Typeface source rows are `illustrator-acrobat-cc.md` [07.6]; the `M/fonts` tree stays whole ([07.3] row 04). Job T1, in order:

| [INDEX] | [STEP] | [ACTOR] | [EVIDENCE] |
| :-----: | :----- | :------ | :--------- |
| [01] | Typeface `File > Export > Backup Tags…` to `S/99.Default Profiles/Typeface/default-typeface-tags.typeface-backup` (overwrite the maintained export) | GUI operator (job T1) | file mtime, size |
| [02] | Read `Typeface > Settings > Library > Remove Missing Fonts`; it is on (`removeMissing = 1`); no change | job T1 | screenshot |
| [03] | `File > Import > Fonts from Folder…` on the `illustrator-acrobat-cc.md` [07.6] row 02 folder (probe T4 of its [07.5]) | job T1 | the sidebar location row; the day's log `Importing done!, … unloadable, 0` |
| [04] | The tags import of `illustrator-acrobat-cc.md` [07.4] row 04 (`nx run @rasm/creative-cloud-desktop:apply`, `open -b <typeface.bundleId> <file>`) | job T1 | the sidebar tag tree; `activatefonts` return for one house name |
| [05] | InDesign: `app.fonts.itemByName("<families.json latin.sans>\tRegular").status` = `INSTALLED`; the same for `persian` and `arabic` | job T1 | readback |

### [05.3]-[NOT_IMPORTED_AND_WHY]

| [INDEX] | [SET] | [REASON] |
| :-----: | :---- | :------- |
| [01] | Drive `01.Fonts` beyond its premium folder as a Typeface location | the premium folder is the imported location (`illustrator-acrobat-cc.md` [07.6] row 02); the sibling folders are archive and web or source formats, imported by nothing and moved by nothing |
| [02] | Drive Inter (148 files) | `owned/Inter-Variable` holds the official 4.001 pair; six copies per style otherwise |
| [03] | Drive Geist (20) | Forge owns Geist 1.800 / Geist Mono 1.700 (newer) |
| [04] | Drive Roboto (16, rev 1.000) | Adobe Fonts Roboto 2.000 |
| [05] | Drive Helvetica (5), Futura (3 + BT/LT rips), Memphis rips | rips; Neue Haas Grotesk and Futura PT on Adobe Fonts |
| [06] | Lato 3.100 (Drive + owned) | roman `GSUB` empty, italic `GPOS` empty; excluded from the house system; not deleted |
| [07] | Overused Grotesk 0.5-alpha.2 | Roman only; excluded from the house system; not deleted |
| [08] | Fonts bundled inside template packages (DM Sans ×9, DM Mono, Inter 4.0, Alegreya, League Spartan, Gilroy) | the font programs stay beside their package and InDesign resolves them from `Document Fonts`; their `OFL.txt` and EULA texts hold no key and are M08 rows |
| [09] | Web kits (`.woff`, `.woff2`, `.eot`, `.css`, `.html`) and Server Mono sources (`.glif`, `.ufo`, `.fea`) | not font programs for activation; stay as archive |

## [06]-[DRIVE_REORGANIZATION]

### [06.1]-[MECHANICS]

| [INDEX] | [MECHANISM] |
| :-----: | :---------- |
| [01] | `rasm-organize --manifest A/manifests/<id>.json --dry-run` reads the manifest's rules, walks `D`, and writes `<id>.plan.json`: rows `{op: rename \| move \| copy \| delete \| retire, from, to, kept, reason, identity: {sha256} or {bytes, mtime, placeholder: true}, status: planned}`; placeholder files are not hashed (reading one downloads it); a `copy` row's identity is the source `sha256` |
| [02] | The verifier reads the plan, checks every `from` exists, every `to` is absent, every `retire` row's `kept` exists with equal `sha256`, then writes `reviewed: true`; a `retire` row whose pair is a placeholder on either side is not reviewable until the lane downloads both files and hashes them |
| [03] | `rasm-organize --manifest <id>.json --apply` refuses without `reviewed: true`; `rename`/`move` = `os.rename` (same volume); `delete`/`retire` = Finder `delete` through `osascript` (`tell application "Finder" to delete (POSIX file "<path>" as alias)`); Google: a file trashed in Drive for desktop is trashed everywhere; trashed files are deleted forever after 30 days |
| [04] | After apply: every row gets `status: applied`, `applied_at`; `rasm-inventory` regenerates `A/inventory.tsv` (path, bytes, mtime, placeholder, sha256 for local files) and replaces the output of the thesis folder's `assets/refresh-source-indexes.py` |
| [05] | Verifier after inventory: every `from` absent; every `to` present with the recorded identity; every `delete`/`retire` absent; no path in the previous inventory is missing from the new one without a manifest row; `A/library.sha256` updated |
| [06] | Manifest order is the table order below; M01, M02, M03, M06, M08, M09, M11, M12, M13 need no application and run at bootstrap; M04 runs after M01 (paths renamed first); M05 after M04; M10 after M04 and M08 (genre folders moved and junk gone before the masters are built) with job L-I7 as its Illustrator step; M07 waits for job I5; M14 runs after M08 once `A/templates-packages.tsv` is reviewed |
| [07] | `organize.py` skip list for hashing alone: `T/00.Grid Templates/01.Logo Related/logo bank package/**`, `T/03.PowerPoint Templates/**`, `D/03.Design Assets/99.Art/**` (placeholders); names still enter the inventory and their M08 and M10 rows apply |
| [08] | Rows shared with `cloud-libraries.md`: one document executes each. This document executes M03 rows [08]–[09] (the Base/Expanded " 2" forks; `cloud-libraries.md` [02.2] rows 05–06 list them and delete nothing) and M05 rows [23]–[24] (the CSH packs under `03.Design Assets`); `cloud-libraries.md`'s 5b purge manifest executes M05 row [22] (the ABR pack under its lane folder `01.Brush Sets`) and its 5c move manifest moves `Seemless Celtic Patterns` to `03.Pattern Assets/01.Pattern Layouts/Celtic Patterns/` (`cloud-libraries.md` [02.4] row [02]), so [02.5] row [05] has no M01 row. M01 runs at bootstrap before every sibling lane; sibling rows naming `Utillity Grid Patterns`, `01.Rrhino Assets`, or `Von Glitshcka` resolve through the regenerated inventory to the renamed paths |
| [09] | Twin or content: for every svg or eps of an M10 pack, an `.ai` in the pack whose `inspect` item count ≥ that file's item count and whose stem equals the file's stem (or whose stem equals the pack's name) is its master and the file is a format twin (`retire`); every other svg or eps merges into the pack master `<pack>.ai` (`merge`, job L-I7) and is then retired. A `<stem> CS2.ai` beside a `<stem>.ai` is a legacy twin (`retire`) after the same count check |
| [10] | Job L-I7 (`merge_vectors {sources, master, gutter: 45}`, an `execute` body): per source `app.open(File(source))`, record `pageItems.length`, `duplicate` every page item into the master document on a layer named after the source stem, close the source without saving; items sit on a grid whose cell is the pack's largest item bound plus the 45 pt gutter, columns = `ceil(sqrt(n))`; a pack whose grid exceeds the 16,383 pt canvas is split into the fewest masters `<pack> 1.ai`, `<pack> 2.ai` and the split is reported; `saveAs` with default `IllustratorSaveOptions` (`pdfCompatible` true); `snapshot` of the master's artboard writes `<pack>.preview.png` beside it; the master's `inspect` item count is the receipt |

### [06.2]-[MANIFESTS]

| [INDEX] | [ID] | [OP] | [FROM] | [TO] | [REASON] | [PRECONDITION] |
| :-----: | :--- | :--- | :----- | :--- | :------- | :------------- |
| [01] | M01 | rename | every row of [02.5] [01]–[04], [06]–[17], [21] | the target column | spelling | none |
| [02] | M01 | rename | every directory and file matching `* ` or `*  *` (58 directories, 181 files) | name with trailing spaces stripped and runs of spaces collapsed | spelling | none |
| [03] | M02 | delete | `S/98.IconJar Folder/IconJar.ijlibrary` (7,740 files) | Drive trash | IconJar retired | none |
| [04] | M02 | delete | `S/98.IconJar Folder/IconJar-backup-08-06-2024@12-39-11.ijlibrary` (7,740) | Drive trash | IconJar retired | none |
| [05] | M02 | delete | `D/04.Software Related Assets/IconJar-backup-07-25-2026@04-42-16.ijlibrary` (3) | Drive trash | IconJar retired | none |
| [06] | M02 | delete | `S/98.IconJar Folder/` (empty after [03]–[04]) | Drive trash | empty | rows [03]–[04] applied |
| [07] | M02 | delete | `D/04.Software Related Assets/` (empty after [05]) | Drive trash | the "merge into 05" has no remaining content | row [05] applied |
| [08] | M03 | retire | `S/02.Color Swatches/00.Illustrator & InDesign/00.Base Swatch 2.ai` | kept `00.Base Swatch.ai` | sha256 equal (`d77dd24c5fc6…`) | none |
| [09] | M03 | retire | `…/01.Expanded Colors 2.ai` | kept `01.Expanded Colors.ai` | sha256 equal (`5fa1c732ff37…`) | none |
| [10] | M03 | retire | `T/02.Documentation Templates/01.Business Related Templates/Architecture Proposal Template Design Layout 2.indt` | kept `… Layout.indt` | sha256 equal (`b3e3b64b574b…`) | none |
| [11] | M03 | retire | `T/04.Architectural Documentation Templates/mapping checklist_LandSpace Architecture.pdf` | kept `Site Analysis & Mapping Checklist.pdf` | sha256 equal (`e2e2367fc2bc…`) | none |
| [12] | M03 | retire | the 9 Typefool " 2" InDesign twins, the 12 " 2" licence text twins, and the 65 other hash-equal " 2" files of [02.2]; the 46 placeholder " 2" pairs of [02.2] after lane L-B3 downloads and hashes both sides (the 2 differing files stay) | kept = the file without " 2" | sha256 equal (86 pairs verified 2026-09-11 22:35–23:20; 46 pairs hashed by L-B3) | none for the 86; the L-B3 hash for the 46 |
| [13] | M04 | move | `D/03.Design Assets/06.Graphical Vector Assets/07.UI Assets/01.Fantasy Style/` (625 files) | `D/03.Design Assets/99.Art/05.Fantasy/UI Fantasy Style/` | genre | M01 |
| [14] | M04 | move | `D/03.Design Assets/06.Graphical Vector Assets/07.UI Assets/02.Sci Fi Style/` | `D/03.Design Assets/99.Art/06.Sci-Fi/UI Sci-Fi Style/` | genre | M01 |
| [15] | M04 | move | `D/03.Design Assets/06.Graphical Vector Assets/00.General/Sci-Fi Panels Vectors/` (285) | `D/03.Design Assets/99.Art/06.Sci-Fi/Sci-Fi Panels Vectors/` | genre | M01 |
| [16] | M04 | move | `D/03.Design Assets/99.Art/00.General/00.General Illustrations/{50 Medieval Illustrations (53), Medieval Nights Graphic Pack (126), Medieval Vectors (689), Medieval Knights.ai, Medieval Armored Knights.ai}` (the last renamed by M01 from [02.5] row [21]) | `D/03.Design Assets/99.Art/07.Medieval/<same name>` | genre inside 99.Art | M01 |
| [17] | M04 | move | `D/03.Design Assets/99.Art/00.General/05.Ornamental Illustrations/01.Heraldric/` (12,748 files: `Heraldric Assets`, `Medieval Heraldic Alpha Elements and Symbols`, `Medieval Heraldic Elements`) | `D/03.Design Assets/99.Art/08.Heraldry/` | genre inside 99.Art, spelling | M01 |
| [18] | M04 | move | `D/03.Design Assets/99.Art/00.General/00.General Illustrations/Miscellaneous Illustrations/{AI,PNG}/Crown - {Decorative - ,}Royal - Heraldry.*` (4 files) | `D/03.Design Assets/99.Art/08.Heraldry/Crowns/` | genre | M01 |
| [19] | M04 | none | Horror: no folder or file name in `D` matches horror, zombie, demon, witch, monster | no `Horror` folder is created | — | — |
| [20] | M04 | move | `D/03.Design Assets/99.Art/00.General/05.Ornamental Illustrations/00.General/Hand Painted Fantasy Viking Symbols/` (2,000 files) | `D/03.Design Assets/99.Art/05.Fantasy/Hand Painted Fantasy Viking Symbols/` | the vendor names the pack Fantasy | M01 |
| [21] | M04 | none | `…/04.Occult Philosophy Illustrations/Snakes & Dragons` (143; an occult-illustration set inside the existing 99.Art taxonomy), `…/Hand Painted Alpha Magic & Mystery Signs/*/Alphabets/Dragon Alphabet (Skyrim)` and every `Skull`/`Dragon` subfolder (members of a pack; a pack is never split), `02.Texture Assets/06.Backgrounds/Castles`, `Dragon Scale Textures`, `Skull Distorted Pack` (photos and textures stay) | stay | — | — |
| [22] | M05 | delete | `S/01.Brush Sets/01.Adobe Photoshop Brushes/Concept Art Cheat Brushes Mega Pack/` (7 ABR: Destroyed City, FX, Ilndustrial, Mechanical objects, Medieval, Modern Militory, Vehicles; 1,210,972 KB) | Drive trash | genre brushes | none (5b's source list excludes them) |
| [23] | M05 | delete | `D/03.Design Assets/05.PSD Based Assets/CSH Files/Concept Art Cheat Shapes Mega Pack/` (20 CSH, 186,092 KB) | Drive trash | genre shapes | none |
| [24] | M05 | delete | `D/03.Design Assets/05.PSD Based Assets/CSH Files/Medieval Battle Generator Tool/` (W1–W5 + `Example.jpg`, 690,384 KB) | Drive trash | genre shapes; the 7 restored files of [02.7] row [06] are this set plus row [23]'s `Medieval empyre-maker.csh` | none |
| [25] | M06 | retire | `T/02.Documentation Templates/00.General/Presentation-Grids/Tabloid Margins Grid.eps` | kept `Tabloid Margins Grid.ai` | `.ai` master exists | none |
| [26] | M07 | copy | the font files of the nine `FONTS/` and `Inter-4.0/` folders of [04.3] row [02] | `Document Fonts/` beside each of their 17 `.indd` | InDesign installs a `Document Fonts` folder beside the document | M03 (the " 2" OFL twins gone first) |
| [27] | M07 | retire | every `.idml` of [04.2] rows [01], [03], [06], [08], [09], [11], [13] | kept = the `.indd` beside it | modern save readback | job I5 row applied for that `.indd` |
| [28] | M07 | delete | v1 folders of [04.2] rows [07], [08], [09], [12] | Drive trash | v2 modern save readback | job I5 |
| [29] | M07 | retire | `Report-Grids/US Letter Portrait Grid System.indd` | kept `.indt` | I5 equal page/style counts | job I5 |
| [30] | M08 | delete | every `.txt`, `.rtf`, `.md`, `.nfo` under `D` whose name matches, case-insensitive, `readme`, `read ?me`, `read this`, `how ?to`, `install`, `licen[cs]e`, `eula`, `terms`, `thank`, or `^ofl` (65 files, `fd`, 2026-09-14; 0 under `99.Art`): the vendor readmes and install notes, the Typefool and GenerateColorValues `license-README-FIRST.txt`, the DM Sans and DM Mono `OFL.txt`, the PageExporterUtility GPL text, the Acid Lite `License Agreement.txt` (its body says the key is in the receipt), the Glitch Effects `Personal-License-graphic-assets.txt` | Drive trash | junk: a licence text survives only when its body holds a key (`rg -e '[A-Z0-9]{4,}(-[A-Z0-9]{4,}){2,}' -e '(?i)license key\|serial\|activation code'` with a value on the line); 0 such files exist | M01, M03 row [12] (the " 2" twins first) |
| [31] | M08 | delete | every `.url` (2) and `.bak` (37) under `D`, every `.DS_Store` (109, `fd -H`, 2026-09-14) and `Icon\r` the walk meets, every `Source.json` (0 today) | Drive trash | junk: bookmarks, AutoCAD backups, Finder metadata, generated recovery records | none |
| [32] | M08 | delete | each of the 36 `.zip` and 10 `.rar` under `D` (99.Art excluded) whose every member (`tar -tf` lists them; bsdtar 3.5.3 with libarchive 3.7.4 reads both formats; `unrar` is not installed) has a same-named file beside it with an equal sha256 (`bsdtar -xOf <archive> <member> \| shasum -a 256` against the sibling, streamed, no file written) | Drive trash | expanded archive wrapper; an archive with a member missing or differing beside it stays and is listed in the plan as `kept, unexpanded` | none |
| [33] | M08 | none | `D/00.Licenses & Receipts/` | not created | no key-bearing file exists on Drive (row [30]) and no receipt or order file exists outside templates (`find` for receipt, invoice, order, purchase: 0 non-template hits) | — |
| [34] | M09 | copy | `M/setup/photoshop/{prepare-brush-libraries.py, brush-preparation.json, brush-payload-identities.json}`, `M/setup/patterns/prepare-pattern-libraries.py`, `M/setup/patterns/illustrator/{prepare-library-request.py, library-request.json, collect-pattern-libraries.jsx, pattern-proof.jsx, repair-proof.jsx}` and the three `.applescript` beside them (`ls`, 2026-09-14), `M/setup/illustrator/{build-brush-libraries.jsx, build-brush-libraries.applescript, brush-library-manifest.json, brush-variant-manifest.json, default-template.ai}` (the generated ABR, PAT, ASL, the toolbars, and the `~ai-*.tmp` are not copied: [02.6] rows [01], [03], [05]) | `A/sources/<same relative path under M/setup/>` | the only copies of the generator scripts and manifests; `shasum -a 256` of each source equals its copy, both hashes in `A/machine-ledger.tsv`, before [07] row [01] deletes `M/setup/` | none |
| [35] | M10 | merge | `D/03.Design Assets/06.Graphical Vector Assets/02.Decorative Assets/03.Decorative Components/Hand Painted Art Deco Designs/*.svg` (2,250; 2,370 `.png` stay) | `…/Hand Painted Art Deco Designs/Hand Painted Art Deco Designs.ai` + `.preview.png` | format policy; no `.ai` exists in the pack | M04, M08; job L-I7 |
| [36] | M10 | merge | `…/03.Decorative Components/Ornamental Corners, Dividers, Elements/*.svg` (50; 50 `.png`, 1 `.pdf` stay) | `…/Ornamental Corners, Dividers, Elements.ai` + `.preview.png` | format policy | M04, M08; L-I7 |
| [37] | M10 | merge | `…/03.Decorative Components/Ornamental Elements (Master of Decoration 1)/*.svg` (4,300; 4,300 `.png`, 3 `.pdf` stay) | `…/Ornamental Elements (Master of Decoration 1).ai` (split per [06.1] row [10] when the grid exceeds the canvas) + `.preview.png` | format policy | M04, M08; L-I7 |
| [38] | M10 | retire or merge | `…/02.Decorative Assets/01.Variety Assets/Aztech Vector Pack/*.eps` (139; 139 `.png` stay; 1 `.ai` present) | the present `.ai` when its item count ≥ 139 (twins retired), else `Aztech Vector Pack.ai` | [06.1] row [09] | M04, M08; L-I7 |
| [39] | M10 | merge | `…/03.Scientific & Graphs/250 Scientific Diagrams Pack/*.svg` (251) | `…/250 Scientific Diagrams Pack.ai` + `.preview.png` | format policy | M04, M08; L-I7 |
| [40] | M10 | merge | `…/03.Scientific & Graphs/Diagram Vector Pack/*.svg` (50) | `…/Diagram Vector Pack.ai` + `.preview.png` | format policy | M04, M08; L-I7 |
| [41] | M10 | consolidate, retire | `…/03.Scientific & Graphs/Retro Futuristic Shapes/`: 15 `.ai` (`Individual files/`, `Catalog files/`) kept as content, 14 `.svg` + 15 `.eps` twins retired, 15 `.psd` + 14 `.png` stay | one `Retro Futuristic Shapes.ai` holding the 15 `.ai` items on layers named by stem; the 15 singles retired after its item count equals their sum | one file per pack ([06.1] row [09]) | M04, M08; L-I7 |
| [42] | M10 | merge | `…/06.Vector Shapes/01.Dotted Style/Wavy Transition Landscapes/*.svg` (96) | `…/Wavy Transition Landscapes.ai` + `.preview.png` | format policy | M04, M08; L-I7 |
| [43] | M10 | retire or merge | `…/01.Icons, Symbols, & Packaging/03.Infographic Assets/Infographics Mega Bundle/**/*.eps` (2; 1,837 `.ai` beside them) | the same-stem `.ai` when present (twin), else `Infographics Mega Bundle.ai` | [06.1] row [09] | M04, M08; L-I7 |
| [44] | M10 | retire | `…/99.Logo Assets/Logo Creation Kit/`: kept `Illustrator/{Logo Elements, Logo Templates, Textures}.ai`, `Font Combinations/Font Combinations.ai`, `UPDATE 1 Frames/U1 Elements.ai`, `UPDATE 2 Icons/U2 Elements.ai` (the 6 current `.ai` of 11); retired `EPS/{Logo Elements, Logo Templates, Textures}.eps`, `U1 Elements.eps`, `U2 Elements.eps` (format twins) and the five `* CS2.ai` (legacy twins) | kept = the same-stem current `.ai` | [06.1] row [09]; `.abr`, `.csh`, `.psd`, `.png` stay | M04, M08; L-I7 |
| [45] | M10 | merge, relink | `T/01.Presentation Templates/Branding Related Presentations/Brand Guidelines Template Pack - BOLD/Links/Grid-Example.svg` (its ` 2` twin is M03 row [12]), `Brand Strategy Presentation Template v1/Links/Ragged_Edge_S.svg`, `… v2/InDesign/Links for InDesign & Illustrator/Ragged_Edge_S.svg` | `<stem>.ai` beside each; the `.indd` links relink in job I5 ([04.3] row [07]) | format policy inside a template package | M08; L-I7 before I5 |
| [46] | M10 | none | `T/00.Grid Templates/01.Logo Related/logo bank package/**/*.eps` (2) | — | the package is trashed whole by M14 row [66]; no master is built | M14 |
| [47] | M10 | retire | `T/03.PowerPoint Templates/Megapack 2/27_Icons/{Corporate, Economics, Finance, Seo, Startup}.eps` (5) | kept = the same-stem `.ai` beside each | format twins; the decks stay | M08; L-I7 (count check) |
| [48] | M10 | merge | `T/00.Grid Templates/00.General/Composition Guides Pack/ImageFiles/**/*.svg` (134) | `…/Composition Guides Pack/Composition Guides.ai` + `.preview.png`; `ApplicationAddons/` (AI, PS, PROCREATE_BRUSHES, CREATIVE_CLOUD_LIBRARY) and `Composition Guides Instructions.pdf` stay | format policy | M08; L-I7 |
| [49] | M10 | retire | `T/01.Presentation Templates/Architectural Presentations/Storytelling Architecture Illustration/Vector sets for perspective_LandSpace Architecture{,-01,-02,-03,-04,-05}.eps` (6) | kept = `Vector sets for perspective_LandSpace Architecture.ai` (artboard count ≥ 5 and item count ≥ each eps) | format twins (per-artboard exports) | M08; L-I7 (count check) |
| [50] | M10 | merge | `D/03.Design Assets/03.Pattern Assets/02.Image Patterns/Tileable Ornamental Patterns and Elements/**/*.svg` (395; the 395 `.png` and the `.pdf` stay) | `…/Tileable Ornamental Patterns and Elements.ai` + `.preview.png` | format policy; raster twins stay as reference | M08; L-I7 |
| [51] | M10 | merge | `D/03.Design Assets/99.Art/00.General/01.Architectural Illustrations/Open Gate Classic.eps` | `…/Open Gate Classic.ai` | format policy (99.Art keeps svg and png, not eps) | M08; L-I7 |
| [52] | M10 | merge | `D/03.Design Assets/99.Art/00.General/04.Occult Philosophy Illustrations/Dark Pixel Graphic Pack/{Detailed Option/72-shapes-detailed, Simple Option/72-shapes-simple}.eps` | `…/Dark Pixel Graphic Pack/Dark Pixel Graphic Pack.ai`, layers `Detailed` and `Simple` | format policy | M08; L-I7 |
| [53] | M10 | merge | `D/03.Design Assets/07.Figma Assets/Color Palletes (400+)/SVG.zip` members (10 `Tabloid - N.svg`, 1,231,412 B expanded) | `…/Color Palettes (400+)/Color Palettes.ai` + `.preview.png`; the zip retired after the merge; `Color Palletes (400+).fig`, `Harmony Color Set (420).fig`, `PDF.pdf`, `Inter (Google Font)/` stay (Figma's own format and its previews), reported | format policy | M01 ([02.5] rows [07], [22]); L-I7 |
| [54] | M10 | none | `D/04.Architectural Assets/` | no row: 0 `.svg` and 0 `.eps` (`fd -e svg -e eps`, 2026-09-14); the 125 AutoCAD `.pat` hatches are CAD's native hatch format and stay | — | — |
| [55] | M11 | copy | `M/tk9-v4/package/Documents and TK9 example actions/{TK9-v4-Instructions-Manual.pdf, TK9-v4-release-notes.pdf, TK9 v4 Button Modifiers.pdf}` and `…/Documents and TK9 example actions/David Tillett TK9 example actions/TK9 Example Actions V4.pdf` (`fd -e pdf`, 2026-09-14) | the same relative paths under `D/00.General/Plugins/TK9/Package/` (the Drive `Documents and TK9 example actions/David Tillett TK9 example actions/` folder holds the 4 `.atn` today and no PDF) | Drive is the master; the four manuals exist on the machine alone; `1 - Read this first.pdf`, `2 - Installation.pdf`, `End-User-License-Agreement.pdf` are M08-class texts and are not copied | none; [07] row [04] deletes `M/tk9-v4/` after the copy's hashes equal |
| [56] | M11 | none | `D/00.General/Plugins/TK9/` (41 files, 1,000,576,774 B, 0 placeholders: 9 `.ccx` v4.0.0, 4 `.atn`, 19 update `.mp4`, 9 practice `.dng`) | stays: the final source set, plus the four manuals of row [55] | conforms | — |
| [57] | M11 | none | `<drive.root>/99.Resource Database/00.General/TK9 Learning Guide/` (80 files, 5,566,222,887 B, 0 placeholders: the course videos, 3 course PDFs, `TK9-Instructions-Manual.pdf`, `Setting Up the Color Working Space.pdf`, practice images) | stays, reported: a course outside `D`, not a plugin copy | — | — |
| [58] | M11 | none | Drive trash `03.Digital Asset Database/00.General/Plugins/TK9 V3/` (the 21 v3.0.0 `.ccx` and documents trashed 2026-09-11; `C/tooling/asset-recovery-sources.json` `recorded_cloud_consolidation`) | no local path exists (`fd -i 'tk9 v3'` over the Drive root and `~/.Trash`: 0); the web trash, this entry included, is emptied once by the machine-ledger builder's first step (`branches/cloud-library.md` [03]) and never again, so every later Drive-trash row stays visible for the user's review; nothing is restored | superseded by v4 | — |
| [59] | M11 | none | the nine installed v4.0.0 UXP plugins `~/Library/Application Support/Adobe/UXP/Plugins/External/com.tk.{comboV8, cxV8, export, multimask, myactionsV8, myactionstab1, myactionstab2, myactionstab3, myactionstab4}_4.0.0/` (10,696 KB) and their `UXP/PluginsStorage/com.tk.*` settings | stay: the installed payloads; layout and preferences are gui-stream's (`gui-photoshop.md` [06] rows 21–30) | final set = rows [55], [56], [59] | — |
| [60] | M12 | retire | `S/00.Automation Assets/02.Adobe InDesign/CreativePro InDesign Script Sampler/{Alt Text From Captions, Smarter Title Case, Sync Layers Across Book}.jsx`, `CreativePro_Scripts_For_Long_Documents/Collect All Objects With Style.jsx` | kept = the same-named file in `CreativePro_InDesign_Script_Set/` | sha256 equal (`f8815cfcfd3d…`, `93aad18df69d…`, `9132913d5047…`, `8b12f8f77c79…`, 2026-09-14) | none |
| [61] | M12 | rename, move | `CreativePro_InDesign_Script_Set/` → `CreativePro/`; the remaining files of `CreativePro InDesign Script Sampler/` (1 `.pdf`) and `CreativePro_Scripts_For_Long_Documents/` (6 `.jsx`, `MergeFiles.jsxbin`, 1 `.pdf`, `PageExporterUtility/` with its `.jsx`; the GPL text is an M08 row) into `CreativePro/`; the two emptied folders deleted | `S/00.Automation Assets/02.Adobe InDesign/CreativePro/` (one CreativePro folder: 18 scripts = 9 `.jsx` + `NINA.js` + 6 `.jsx` + `MergeFiles.jsxbin` + `PageExporterUtility5.0.1.jsx`, 3 manuals; `fd`, 2026-09-14) | one folder per vendor; `GenerateColorValues_Script/` (1 `.jsx`, 1 `.pdf`) conforms and stays | row [60] |
| [62] | M12 | none | `MergeFiles.jsxbin` (compiled ExtendScript, no source beside it), `NINA.js` | stay, reported: no modern form exists without the source | — | — |
| [63] | M13 | copy | `~/Library/Mobile Documents/com~apple~CloudDocs/Algorithm Texture 02.png` (62,374,258 B, `890aa82e9d4699e06e4bac59f56602e57f5b33ae5eada9d853e2a9c71af88f31`), `Algorithm Texture 08.png` (62,424,956 B, `45d954fc6336f6c3796a7fefa1f7bd1012b508b9a7f92fa982f05acf2d0c50a5`); both local, PNG 6000 × 4000 RGBA | `D/03.Design Assets/02.Texture Assets/05.Large Textures/Algorithm Textures/` | iCloud holds no library content | none |
| [64] | M13 | delete | the two iCloud originals of row [63] | iCloud trash (Finder `delete`) | copies hash-equal on Drive | row [63] hashes equal |
| [65] | M13 | delete | `~/Library/Mobile Documents/com~apple~CloudDocs/Documents/LA_AI_Scripts/{bigBang, CROA, Cropulka, Duplicator, Randomus}__setting.json` (16, 12, 57, 39, 90 B) and the emptied folder | iCloud trash | orphaned script settings: no script named `bigBang`, `CROA`, `Cropulka`, `Duplicator`, or `Randomus` exists under `S/00.Automation Assets`, `D/00.General`, `<illustrator.supportFolder>`, `<illustrator.prefsFolder>`, or the app's `Presets.localized` (`fd -i`, 2026-09-14); the audit row per file records name, bytes, and this search | none |
| [66] | M14 | delete | `T/00.Grid Templates/01.Logo Related/logo bank package/` (1,411 folders, 2,674 files: 1,398 jpg, 1,244 ai, 18 png, 2 eps, all placeholders; 1,110 folders hold exactly two files, `… - google search_files` scrape folders; `fd`, 2026-09-15) | Drive trash | junk: a web scrape with no distinct document, licence, or source list ([01] row 09) | its `A/templates-packages.tsv` row reviewed |
| [67] | M14 | delete | `T/02.Documentation Templates/Portfolios/Pack of Templates/` (112 folders, 621 files: 255 jpg, 67 otf, 57 ttf, 60 png, 33 pdf, 32 psd, 0 indd; 72 leaf folders hold 1–4 files; `fd`, 2026-09-15) | Drive trash | junk: one PSD template per folder with its previews and vendor fonts, no InDesign document ([01] row 09); the bundled font files are the package's and go with it ([05.3] row 08) | its `A/templates-packages.tsv` row reviewed (one line per leaf folder: files, formats, the template file) |

Genre folder numbering continues `99.Art`'s scheme (`00.General`, `01.Alchemy`, `02.Astrology`, `03.Sacred Geometry`, `04.Tarot`) with `05.Fantasy`, `06.Sci-Fi`, `07.Medieval`, `08.Heraldry`; the plan's `Horror` gets no folder.

## [07]-[MACHINE_LEDGER]

Every row is `{path, bytes, sha256_copy, reason, order}` in `A/machine-ledger.tsv`. Sizes are `du -sk` on 2026-09-11.

| [INDEX] | [PATH] | [KB] | [VERDICT] | [ORDER / PRECONDITION] |
| :-----: | :----- | ---: | :-------- | :--------------------- |
| [01] | `M/setup/` | 22,920,884 | delete | after [06.2] M09 row [34] (its copies hash-equal in `A/machine-ledger.tsv`; the generated ABR, PAT, and ASL are not copied: regenerable, never imported) |
| [02] | `M/tk9-v4/downloads` (2 files, 958,504,816 B) | 936,044 | delete | none (archives whose members equal [03] and Drive `TK9` by CRC32 and size) |
| [03] | `M/tk9-v4/update-videos` (28 files, 996,293,927 B) | 972,992 | delete | none (28 files sha256-equal to Drive `00.General/Plugins/TK9/Update videos`) |
| [04] | `M/tk9-v4/package` (20 files, 11,351,981 B) and the emptied `M/tk9-v4/` | 11,120 | delete | after [06.2] M11 row [55] (the four manuals copied to Drive, hashes equal); the other 13 files equal Drive `Package/` by sha256 and the 3 readme and EULA PDFs are M08-class |
| [05] | `M/fonts/google` (1,687 family directories and the `.typeface-location` marker), `M/backups`, `M/licenses`, `~/Library/Logs/design-tools` | 1,898,000 + 40,364 + 4 + 264 | keep | `illustrator-acrobat-cc.md` [07.3] row 04, [07.6] |
| [06] | `M/fonts/owned` (54 family directories) | 48,136 | keep | 393 of 425 files hash-equal to Drive `01.Fonts` ([09] row [30]); `illustrator-acrobat-cc.md` [07.3] row 04 |
| [07] | `M/color/` | 96 | delete | after the swatch readbacks of jobs L-I1 (Illustrator `inspect`), I2 (InDesign `document.colors`), and L-P2 (Photoshop `list_presets`) |
| [08] | `~/Library/Application Support/Adobe/Adobe Illustrator <major>/` | 1,436 | delete | the seven `New Document Profiles/*.ai` are sha256-identical to the Beta's own copies (7/7, rehashed 23:25); `<illustrator.prefsFolder>/en_US/PresetDocumentProfileDataV10.json` (150,204 B, 2026-08-19, never rewritten by the Beta) names the release path in all 141 `settingsFile` rows (`jq`, 23:05), and the Prefs rows `startupFileType_5/_6/_7` name it too (gui-illustrator [06.1] rows 02–03); job L-A1 runs first: rename the folder to `Adobe Illustrator <major>.retired`, create one document from a Print preset in the Beta; a document opens → delete the renamed folder; the dialog fails → the folder stays renamed until gui-illustrator [06.2] rows 05–06 (the JSON `settingsFile` and Prefs `startupFileType` repoint, gui-stream's rows) apply, then delete; this document writes neither file |
| [09] | the `AIRobin <build>` entry of `<illustrator.staleSupportFolders>` (a build no installed app carries) | 16 | delete | none; the `AIRobin <build>` folder of the installed build is the Mockup add-on's data folder and stays (add-ons are kept); proof: `ls ~/Library/Application Support/Adobe/ \| rg AIRobin` lists the installed build's folder alone after the row |
| [10] | `~/Library/Application Support/Adobe/Adobe Photoshop <year>/` | 0 | delete | none (empty `en_US`) |
| [11] | `~/Library/Preferences/Adobe InDesign/Version <major>.0/` | 5,812 | delete | `lsof +D` empty (checked 2026-09-11 22:09); release InDesign absent; the Beta writes `<indesign.prefsFolder>/` |
| [12] | `~/Library/Application Support/Adobe/UXP/PluginsStorage/PHSPBETA/23` (0 entries, `ls -A` 2026-09-14); `…/PluginsStorage/IDSN/` once row [19] removes its only content (`IDSN/<major>/External/8ebe7f95/`) | 0 | delete | `PHSPBETA/23` none; `IDSN/` after row [19] |
| [13] | `/Library/Application Support/Adobe/CEP/extensions/CC_LIBRARIES_PANEL_EXTENSION_{3_8_299, 3_13_164, 3_25_28}` | 29,708 + 68,728 + 110,952 | delete (root-owned; `sudo rm -r`) | `4_14_50` (118,376 KB) stays |
| [14] | `<photoshop.prefsFolder>/sniffer-out{,1,2,3,4}.txt` | 5 × 3 KB | delete | none |
| [15] | `<illustrator.supportFolder>/en_US/IllustratorSession.lck$$` | 0 | delete | a stale session lock (mtime 2026-09-11 16:14; no Illustrator process at 23:25, `pgrep -f "Adobe Illustrator"` matches no app); delete while Illustrator Beta is not running |
| [16] | `~/Library/Preferences/<photoshop.processName> Paths` | 85 B | delete | none |
| [17] | `~/.Trash/""` | empty directory | delete | none |
| [18] | `M/setup/illustrator/~ai-56f3f3e4-c6b0-4f1a-97a9-d499af1ee7f6_.tmp` | 500 (509,971 B, 2026-09-11 22:10:37) | delete | inside row [01]; an Illustrator temporary save; not copied by M09; Illustrator Beta not running |
| [19] | `/usr/local/lib/indesign-sidekick` (135,944), `/usr/local/bin/indesign-sidekick` (symlink to `…/bin/indesign-sidekick`), `~/Library/Application Support/Adobe/UXP/Plugins/External/8ebe7f95_1.0.22` (68; manifest `Sidekick` 1.0.22, `ws://localhost:6001`) | 136,012 | delete | gate G12 in two steps, producer then consumer: (1) on the first `hello` of `rasm.indesign.bridge` (gui-stream job I1), app-validation removes the `indesign-sidekick` row from `.mcp.json` and its `.claude/settings.json` allow rows and reports the removal to main; main forwards it to cloud-library; (2) this ledger: stop the `bash /usr/local/bin/indesign-sidekick start` launcher and its child `/usr/local/lib/indesign-sidekick/bin/node /usr/local/lib/indesign-sidekick/bin/run start` (listening on 6001, InDesign Beta connected; `lsof -nP -i :6001` and `ps -o ppid,etime,command` on the listener, 23:05), quit InDesign Beta, `lsof -nP -iTCP:6001 -sTCP:LISTEN` empty, then delete the three paths, `~/Library/Application Support/Adobe/UXP/PluginsStorage/IDSN/<major>/External/8ebe7f95/` and `…/IDSNBETA/<major>/External/8ebe7f95/`, `~/Library/Application Support/indesign-sidekick/`, `~/.cache/indesign-sidekick/`, `~/Library/Logs/design-tools/sidekick.log`, the `ID.json` row `pluginId "8ebe7f95"` (`jq` delete, readback empty), and `sudo pkgutil --forget nl.eastpole.indesign-sidekick`; the `UPI.db` `Tb_ExtBasicInfo` row is recorded and left (no writer in the design) |
| [20] | `<illustrator.supportFolder>/en_US/Swatches/Default Palette.ase` | 4 | keep | sha256 equal to the master ([03.1]); `Persistent` chosen once (`cloud-libraries.md` [08.2] row 03) |
| [21] | `<indesign.prefsFolder>/en_US/Find-Change Queries/GREP/{Collapse Ordinary Spaces, Trim Trailing Spaces and Tabs}.xml` | — | keep | — |
| [22] | `~/.codex/config.toml` (8 KB; `adobe-illustrator` MCP block `enabled = false`, bearer token equal to `AIMCPServer/AuthToken`), `~/.codex/*.sqlite{,-wal,-shm}` (`logs_2.sqlite` 1,491,008; `queue_1`, `goals_1`, `memories_1` and journals 11,060) | 8 + 1,502,068 (`~/.codex` 14,075,476 in all; `du -sk`, 23:05) | keep | — |
| [23] | `/Applications/Scripta.app` (`CFBundleShortVersionString` 2.0, `CFBundleVersion` 315, `org.iwashi.Scripta`; bundle dated 2026-09-09) | 61,720 | delete (Finder trash) | support folder: none exists; `mdfind "kMDItemCFBundleIdentifier == 'org.iwashi.Scripta'"` returns the bundle alone and `~/Library/{Application Support,Containers,Group Containers,Preferences,Caches,Saved Application State,HTTPStorages,WebKit}` hold no `iwashi` or `Scripta` entry (`ls \| rg -i`, 23:05), so the bundle is the whole row. `C/tooling/scripta.md`: never configured (no folders, shortcuts, login item, Automation grant, or script bindings); no Homebrew or Nix owner exists; `pgrep -f Scripta` empty first |
| [24] | Deletion route for every `delete` row above | — | Finder trash through `osascript` (`tell application "Finder" to delete` over the row's paths in one list) | `~/.Trash` holds them until the user empties it |

After every row applies: `forge-default-applications check` → 101 verified, 0 changed.


## [08]-[LANES_AND_JOB_CARDS]

| [INDEX] | [LANE] | [SCOPE] | [RUNS] | [INPUTS] | [EVIDENCE] |
| :-----: | :----- | :------ | :----- | :------- | :--------- |
| [01] | L-A palette | `palettes.py` (`swatch.parse` of the set, `cmyk` column, alias keys), `palette.json`, the gap record; no swatch file is written | bootstrap, Python only | the set ([03.1]), `d-my-color-palette.json` (gap columns), `GRACoL2013_CRPC6.icc` | `A/palette.json` (92 rows + 4 aliases), `A/palette-drift.tsv`, the master's `library.sha256` row |
| [02] | L-B1 Drive `05.Software Related Assets` + `04.Software Related Assets` | M01 rows there, M02, M03 rows [08]–[09], M08 rows there | bootstrap | manifests | plans, inventory |
| [03] | L-B2 Drive `03.Design Assets` except `06.Graphical Vector Assets` and `07.Figma Assets` | M01, M04, M05, M08 rows there, M10 rows [50]–[52] | bootstrap; L-I7 for the three masters | manifests | plans, inventory, `.preview.png` per master |
| [04] | L-B3 Drive `98.Templates` | M01, M03 rows [10]–[12], M06, M08 rows there, M10 rows [45]–[49], `A/templates-packages.tsv` then M14 rows [66]–[67] | bootstrap (the 46 placeholder pairs of M03 row [12] after this lane downloads and hashes them); L-I7 before I5; M07 after I5 | manifests, `templates-inspection.tsv` | plans, inventory, `.preview.png` per master |
| [05] | L-B4 Drive `04.Architectural Assets` (the whole tree: `00.Vector Assets` masters are lane 5f's, the other eight subfolders are this lane's) | M01, M08 rows there (the 37 `.bak` sit here and under `98.Templates`), inventory rows for `02.Grasshopper`, `03.CAD` (125 `.pat` hatches stay), `04.ArchiCAD`, `05.Revit`, `06.Blender`, `98.SketchUp`, `99.Render Presets`; M10 has no row ([06.2] row [54]) | bootstrap | manifests | plans, inventory |
| [06] | L-C templates | `idml.py` inspection TSV, `font-map.json`, job I5 | I5 after typography's I2 and round-1 pick | [04.2], [04.3] | I5 readback rows, modern files |
| [07] | L-D fonts | the job T1 card ([05.2]); no machine row, no Drive row (`illustrator-acrobat-cc.md` [07.6]) | the Typeface pass | [05.2] | Typeface screenshots, the sidebar tree, InDesign readback |
| [08] | L-E machine | M09 (row [34]) and rows [07] [02]–[03], [09]–[18], [23] now ([15] and [18] while Illustrator Beta is not running); [01] after M09; [04] after M11 row [55]; [07] after the L-I1, I2, and L-P2 readbacks; [08] after job L-A1; [19] after G12 step (1) | staged | `machine-ledger.tsv` | `du` before/after, `forge-default-applications check` |
| [09] | L-B5 Drive `03.Design Assets/06.Graphical Vector Assets` + `07.Figma Assets` | M10 rows [35]–[44], [53]; M01 ([02.5] rows [07], [22]); M08 rows there | after L-B2's M04 applied (the genre folders leave `06.Graphical Vector Assets` first); L-I7 for the masters | manifests, `inspect` counts | plans, `.preview.png` per master, inventory |
| [10] | L-B6 Drive `05.Software Related Assets/00.Automation Assets/{02.Adobe InDesign, Coding, Rhino 3D, Shared}` + `03.Style Assets/Rhino 3D` | M12; inventory rows (`Coding` 1 file, `Rhino 3D` 17, `Shared` 6, `03.Style Assets/Rhino 3D` 20 `.ini` + `00.Style Environment Maps`; all stay); M08 rows there | after M01 | listing, hashes | plan, inventory |
| [11] | L-B7 iCloud Drive | M13 rows [63]–[65] | bootstrap | the two hashes of row [63], the `fd -i` search of row [65] | plan rows with both hashes; `ls` of the iCloud root and `Documents/` after apply |
| [12] | L-B8 OneDrive, reported only | `OneDrive-MazanGroup` (2,513 files, 4,097,480 KB, 3 placeholders; business project folders; `04.Mazan Design/00.Active Projects/00.New Project Template` exists and is empty); `OneDrive-Personal` (44,188 files, 21,227,760 KB, 37,808 placeholders; `99.Bayt al-Hikmah/00.Foundational Assets/{Background Images, Profile Picture}` 4 `.png`, `Parametric_Arsenal_v1…v4` Rhino and Grasshopper libraries, `05.School/…` = `C`); no lane writes either mount | none | `ls`, `fd`, `du`, `stat` 2026-09-14 | the two rows in the closeout report |
| [13] | mzn Drive outside `D`, reported only | `00.Destination & Sharing` 661 files / 1,388,640 KB; `01.Document Database` 80 / 40,872 (`99.Templates/US Letter Template.indd` is an InDesign template outside `T`); `01.Document Developemnt` 123 / 56,872 (misspelled, outside M01's walk); `02.Media Database` 1,071 / 6,102,912; `02.Media Development` 22 / 327,708; `04.Digital Asset Development` 9 / 3,008 (`00.Backburner`: 9 identity `.ai`); `05.GIS Asset Database` and `05.GIS Development Database` empty; `99.Resource Database` 3,952 / 6,632,216 (M11 row [57]); `100.Company Assets` 242 / 102,308 (the live brand files); 0 placeholders in all ten; no lane writes them | none | `fd`, `du` 2026-09-14 | the ten rows in the closeout report |
| [14] | L-B9 Drive `00.General`, `01.Fonts`, `06.Videography Assets`, `97.Photography Presets`, and the two root files (`My Default Profile.ai`, `.DS_Store`) | M11 row [55]; M08 rows there (the root `.DS_Store` included); inventory rows (`01.Fonts` and `06.Videography Assets` unchanged, `97.Photography Presets` empty) | bootstrap | manifests, the four manuals' hashes | plans, inventory, `shasum -a 256` of the four copied manuals equal on both sides |

Lanes L-B1…L-B7 and L-B9 touch disjoint folders (L-B5 after L-B2's M04, L-B6 after M01) and run concurrently; L-A, L-D, L-E are independent of them. L-B1 stops at `02.Color Swatches`, `98.IconJar Folder`, and `99.Default Profiles`: `01.Brush Sets`, `03.Style Assets`, and `03.Pattern Assets` belong to `cloud-libraries.md`'s lanes ([06.1] row 08), which execute M05 row [22] and the M08 rows inside their folders within their own manifests. Every manifest runs through the `library` project (`rasm-organize`, `rasm-inventory`, `idml.py`, `palettes.py`): repo-code builds it (unit 1) and reports its `check` green to main; cloud-library alone executes it; typography reads its outputs.

Job cards (inputs ready before issue):

| [INDEX] | [JOB] | [APP] | [INPUTS] | [STEPS] | [EVIDENCE BACK] |
| :-----: | :---- | :---- | :------- | :------ | :-------------- |
| [01] | L-I1 | Illustrator Beta | `A/palette.json`; the Adobe RGB template open | `import_swatches` of the two groups; `inspect` | readback JSON equal to `palette.json` (92 rows); profile name |
| [02] | L-P2 | Photoshop Beta | the set, the Colorplan ASE, 3 `.aco` | `Swatches panel menu > Import Swatches` ×5 ([03.4] row 05); quit Photoshop (writes `Swatches.psp`); `list_presets {kind: 'swatch'}` | names list (92 + 49 + 21); `Swatches.psp` mtime |
| [03] | I5 | InDesign Beta | `A/templates-inspection.tsv`, `A/font-map.json`, manifest M07 row [26] applied (`Document Fonts` beside every `.indd`), the M10 row [45] plan (svg → ai relink map), the batch `execute` body (open, `fonts`/`links` readback, Find Font replace by map, relink to `Links/`, `save`, `saveACopy .indt`, close) | one batch per [04.2] row; Verlag and Contents Page IDML conversions included | per-file readback rows; the M07 precondition flags |
| [04] | L-I7 | Illustrator Beta | the M10 plan rows [35]–[53] with `reviewed: true`, the source lists per pack | `merge_vectors` per pack ([06.1] row [10]); `inspect` per master; `snapshot` per master | master paths + hashes, item counts per source and per master, `.preview.png` per master |
| [05] | T1 | Typeface Beta | the `apply` target's tags file ready (`illustrator-acrobat-cc.md` [07.4] row 04); `families.json` (G6 and round 3) | [05.2] steps [01]–[05] | backup file, screenshots, the sidebar tree, InDesign readback |
| [06] | L-A1 | Illustrator Beta | the `Adobe Illustrator <major>` entry of `<illustrator.staleSupportFolders>` renamed to `<name>.retired` by lane L-E | `File > New > Print > Letter > Create`; report whether a document opened | the answer; on failure the folder stays renamed until gui-stream's JSON and Prefs repoint ([07] row 08), then a second attempt |

## [09]-[VERIFICATION_TABLE]

Every claim the specification rests on, with its evidence; the one open row ([14], the 46 placeholder twin pairs) carries its remaining step.

| [INDEX] | [CLAIM] | [STATE] | [REMAINING_STEP] |
| :-----: | :------ | :------ | :--------------- |
| [01] | The unified swatch set: one master, one identical user copy, 92 `RGB` `Global` colours in `Default Palette / Base` (13) and `Default Palette / Expanded` (79) | verified 2026-09-14 (`shasum -a 256` both copies `afd7121a…`; ASE 1.0 block walk: 96 blocks, 92 colour blocks, group names read from the `C001` blocks; `fd -e ase` over `D`: the master and the Colorplan file are the only ASE files; no swatch-class file changed after 2026-09-12) | — |
| [02] | the set = Adobe RGB conversion of `My Default Profile.ai` (b); 66 rows differ from the `My Color Palette.ai` conversion | verified (92/92 within 1; 26 equal, 66 differ) | reported in `A/palette-drift.tsv`, decided by the user |
| [03] | Colorplan 49 RGB global, floats; ACO 20 CMYK + 1 HSB | verified (parsed bytes; block walk 2026-09-14: 51 blocks, 49 colours, one group `Colorplan`) | — |
| [04] | Colorplan floats are sRGB numbers | verified: all six Kelman IDML under `Report-Grids` and `Presentation-Grids` declare `RGBProfile="sRGB IEC61966-2.1"` (`designmap.xml`, 23:25) | — |
| [05] | The 92 gap-table conversions (`coloraide` sRGB → a98-rgb) | verified twice (`coloraide 8.12.1`; `SP/design/probe-cloud/palette-check.py` 23:00 reproduces the table) | — |
| [06] | `swatch 0.4.0` parses ASE with Color Groups and RGB rows of type Global | verified (nsfmc/swatch README and pypi.org/project/swatch: `swatch.parse(filename)`; modes RGB, CMYK, Gray, LAB; types Process, Global, Spot); the package is pure Python (`swatch-0.4.0.dist-info/WHEEL`: `Root-Is-Purelib: true`, `Tag: py3-none-any`; `parser.py`, `writer.py`, no compiled member) | — |
| [07] | The `library` project is not in the repo tree today | verified 2026-09-14: `apps/creative-cloud` is absent, `import swatch` fails in the repo `.venv`; `mise.toml [tools]` holds `rust = "latest"` and `cmake = "latest"` (the sdist build of `pyroscope-io` 1.2.3 needs them; PyPI ships its wheels for cp310–cp314 only); the reference copy `SP/design/repo-copy/apps/creative-cloud/library` holds `pyproject.toml` (`msgspec`, `swatch`) and `inventory.py` | the `library` gate: repo-code unit 1 builds the project with `psd-tools` and `swatch` in its dependency group and reports `check` green to main, forwarded to the cloud-library orchestrator before the first manifest runs |
| [08] | Illustrator ASE routes: `Open Swatch Library > Other Library` in, `Save Swatch Library as ASE` out; identically named swatches overwrite | verified (Adobe 2025-02-12 and 2025-10-27) | — |
| [09] | InDesign `Load Swatches` / `Other Library` accept INDD, INDT, AI, EPS, ASE; Photoshop `Import Swatches` | verified (Adobe InDesign 2026-06-02, 2026-08-07; Illustrator share page 2025-10-27) | — |
| [10] | Photoshop Beta current-swatch store is `Swatches.psp` in `<photoshop.prefsFolder>` | verified (file present) | — |
| [11] | InDesign installs a `Document Fonts` folder beside the document; the Drive volume resolves `Document fonts` case-insensitively | verified (Adobe using-fonts page, last updated 2024-11-29 as served 2026-09-11 23:20: "Fonts in a Document Fonts folder that is in the same location as an InDesign document are temporarily installed when the document is opened"; `diskutil info /` = APFS, `document FONTS` lookup resolves; 69 such directories under `T`, 62 beside an `.indd`) | — |
| [12] | 26 Kelman files present with the ledger's sha256 | verified | — |
| [13] | 193 slide moves, 15 Keynote retentions (sha256 = `candidate_sha256`), Verlag members, 7 medieval sizes | verified | — |
| [14] | Every " 2" twin in `T` and `02.Color Swatches` classified | verified: 9 InDesign, 12 licence texts, 2 swatch `.ai`, 1 `.indt`, 1 PDF hash-equal (22:35, 23:15); of the other 113 in `T`, 65 hash-equal, 2 differ, 46 placeholder pairs (23:20) | M03 row [12]: L-B3 downloads and hashes the 46 pairs before their retire rows are reviewable |
| [15] | `Adobe Illustrator <major>/en_US/New Document Profiles/*.ai` = the Beta's copies | verified (7/7 sha256 equal, rehashed 23:25); the stale JSON and Prefs paths are handled by job L-A1 with both outcomes decided ([07] row 08); the JSON lives in `~/Library/Preferences/…/en_US/`, not Application Support | — |
| [16] | the release `Adobe InDesign/Version <major>.0` preferences folder (`indesign.stalePrefsFolders`) is an orphan | verified (`lsof +D` empty, release app absent) | — |
| [17] | Sidekick process | verified (the node `… run start` process under `bash /usr/local/bin/indesign-sidekick start`, port 6001, InDesign Beta connected; `lsof`, `ps`, 23:05) | — |
| [18] | TK9 update videos and archives | verified (28/28 sha256 equal Drive vs machine; zip CRC32 equal to the expanded copies) | — |
| [19] | Typeface locations and settings | verified (`defaults read com.criminalbird.typeface.beta` for the five keys of [05.1] row 05; `C/tooling/typeface.md` lines 103 and 129 for the locations, clear-on-quit, and the persistent set; `typeface-fonts-inventory.json` `native_state` holds no `locations` or `settings` key) | — |
| [20] | `Remove Missing Fonts` removes deleted Google fonts and keeps tags | verified (Typeface docs) | — |
| [21] | Drive for desktop trash = Drive trash; 30-day purge | verified (Google page) | — |
| [22] | Genre packs | decided (M04 rows [13]–[21]): the three ambiguous packs move to `05.Fantasy` as artwork, nothing deleted; the two witch/demon-named occult packs stay | — |
| [23] | `Scripta.app` | verified installed (2.0/315, `Info.plist`); no support folder anywhere under `~/Library` (`mdfind`, `ls`); removed ([07] row 23) | — |
| [24] | Adobe Creative Cloud Libraries | excluded route, no row anywhere writes to one | — |
| [25] | No Horror-named content on Drive | disproven: `Witches & Wizards` (211) and `Demonic` (200) under `99.Art/00.General/04.Occult Philosophy Illustrations`; the IconJar sets (21 ×2, deleted by M02) are the only other matches; `gothic` matches a font family and a Xerox effect pack, not artwork | M04 row [21]: both stay, reported |
| [26] | IDML DOMVersions | verified: 11 distinct values over 314 files (12.1 and 19.4 one each; recount 23:15) | — |
| [27] | Drive tree totals | verified twice (22:09, 23:12): files and bytes equal, placeholders 145,335 → 144,135 | — |
| [28] | Illustrator Beta state | verified: not running at 23:25; the session lock and the `~ai-*.tmp` are stale files ([07] rows 15, 18) | — |
| [29] | Report-Grids 8 `.indd`, Branding 28/25/3, Business 37/32/4, General Presentations 6/8/3, Portfolio Mega Pack 133/209, Resumes 2/1/1, Verlag 1/1/1 (`.indd`/`.idml`/`.indt`) | verified (`fd -H`, 23:05–23:20) | — |
| [30] | `M/fonts/owned` 393 of 425 hash-equal to Drive; the 32 unique files by family | verified (`shasum -a 256` both sides, 23:02) | — |
| [31] | Drive trash is not restored: its 18 groups (262 files, `C/tooling/asset-recovery-sources.json` `source_history_accounting.trash_groups`) are duplicates, wrappers, superseded versions, and the old Lato and Overused Grotesk files, every kept replacement present; Google purges trash 30 days after deletion (2026-10-11) | verified (retained targets present; sha256 of every retained Keynote file equals the ledger's `candidate_sha256`; both font families are outside the house system, the 5 templates referencing Overused re-fonted by [04.3] row 05) | — |
| [32] | svg and eps per pack under `03.Design Assets`, `04.Architectural Assets`, `98.Templates` ([06.2] M10) | verified 2026-09-14 (`fd -e svg -e eps` per root, grouped by pack path, re-run the same day per M10 pack folder: every row count equal): `06.Graphical Vector Assets` 7,011 svg + 161 eps in nine packs, `02.Image Patterns` 395 svg, `99.Art` 3 eps, `98.Templates` 138 svg + 14 eps in eight packs, `04.Architectural Assets` 0 | item counts per source and master: job L-I7 |
| [33] | Every TK9 copy ([06.2] M11) | verified 2026-09-14 (`fd`, `stat`, `du`, `ls`; the recovery ledger for the trash group; `fd -i 'tk9 v3'` over the Drive root and `~/.Trash` empty; CEP extension folders hold no `com.tk.*`, the UXP `Plugins/External` folder holds the nine; the four manuals sit under `package/Documents and TK9 example actions/`, `fd -e pdf`) | — |
| [34] | The five LA_AI settings are orphans | verified 2026-09-14 (`fd -i` over the five script homes returns nothing; the Drive scripts folder holds 44 `.jsx`/`.js` in nine folders, none of the five names) | — |
| [35] | The two iCloud textures are local PNGs | verified 2026-09-14 (`ls -lO` flags `-`, `file`: PNG 6000 × 4000 8-bit RGBA, `shasum -a 256` as [06.2] row [63]) | — |
| [36] | OneDrive and mzn-Drive-outside-`D` counts ([08] rows [12]–[13]) | verified 2026-09-14 (`fd -H -I -t f \| wc -l`, `du -sk`, `stat -f %b` zero-block count) | — |
| [37] | Junk-class counts ([06.2] M08) | verified 2026-09-14 (`fd` regex over `D` excluding `99.Art`: 65 texts, 2 `.url`, 37 `.bak`, 0 `.webloc`, 109 `.DS_Store` (`fd -H`), 0 `Source.json`, 36 `.zip`, 10 `.rar`, 0 `.7z`; `99.Art`: 0 texts); the Acid Lite licence text's only key-like lines are the heading `9. License Key` and the sentence pointing at the receipt | the M08 dry run lists every match with its rule |
| [38] | InDesign script twins ([06.2] M12) | verified 2026-09-14 (`shasum -a 256` over the 23 scripts: 4 hash-equal pairs, 19 singletons; the merged `CreativePro/` holds 18 scripts and 3 manuals) | — |
| [39] | Figma `SVG.zip` members | verified 2026-09-14 (`unzip -l`: 10 `Tabloid - N.svg`, 1,231,412 B) | — |

## [10]-[SOURCES]

| [INDEX] | [SOURCE] | [DATE] |
| :-----: | :------- | :----- |
| [01] | `I/illustrator/d-my-color-palette.json`, `I/illustrator/b-my-default-profile-drive-root.json`, `SP/illustrator/probe-illustrator.md` | dumps 2026-09-11 18:27–18:37 |
| [02] | `SP/fonts/font-inventory.md`, `I/fonts/font-inventory.tsv` (1,784 rows) | 2026-09-11 |
| [03] | `indesign-acrobat-depth.md` [01], [02], [05] | 2026-09-11 |
| [04] | `architecture.md` [02], [06] | 2026-09-11 |
| [05] | `C/tooling/asset-recovery-sources.json` (2,064,812 B), `assets-recovery.md`, `assets/catalogs/templates.md`, `assets/SOURCE-COLLECTIONS.tsv`, `tooling/typeface.md`, `typeface-fonts-inventory.json`, `indesign-template-evidence.json`, `indesign-font-evidence.json` | updated 2026-09-11 12:33–16:27 |
| [06] | Drive walk (`os.lstat`, `SF_DATALESS`), `find`, `du`, `shasum -a 256`, IDML `zipfile` scan (314 files) | 2026-09-11 22:05–22:20 |
| [07] | Machine reads: `du`, `ls -lO`, `lsof +D`, `lsof -nP -i :6001`, `defaults read com.criminalbird.typeface.beta`, `/Applications`, `PresetDocumentProfileDataV10.json` | 2026-09-11 22:09–22:18 |
| [08] | `coloraide 8.12.1`, `fonttools 4.64.0` in the repo `.venv` (python 3.15) | 2026-09-11 |
| [09] | helpx.adobe.com/illustrator/using/using-creating-swatches.html | last updated 2025-02-12 |
| [10] | helpx.adobe.com/indesign/using/swatches.html | last updated 2025-04-10 |
| [11] | helpx.adobe.com/photoshop/using/customizing-color-pickers-swatches.html | last updated 2023-05-24 |
| [12] | helpx.adobe.com/photoshop/using/presets.html | last updated 2023-05-24 |
| [13] | helpx.adobe.com/illustrator/using/files-templates.html (`.ait` opens as an untitled `.ai`; presets saved in the New Document dialog) | last updated 2025-01-14 |
| [14] | helpx.adobe.com/indesign/using/saving-documents.html (template save, IDML save) | last updated 2024-08-09 |
| [15] | typefaceapp.com/help/troubleshooting/missing-imports, /help/troubleshooting/backups, /help/articles/activation, /help (FAQ) | read 2026-09-11 (pages undated) |
| [16] | support.google.com/drive/answer/2375102 (trash everywhere, 30-day purge) | read 2026-09-11 (page undated) |
| [17] | `Parametric_Forge/modules/home/environments/media.nix` | 2026-09-11 |
| [18] | helpx.adobe.com/illustrator/desktop/manage-colors/use-swatches/share-swatches-between-applications.html (`Save Swatch Library as ASE`; InDesign `Load Swatches`; Photoshop `Import Swatches`) | last updated 2025-10-27 |
| [19] | helpx.adobe.com/indesign/desktop/apply-color/define-and-manage-color-assets/import-and-share-swatch-libraries.html (sources INDD, INDT, AI, EPS, ASE; `Other Library`; `Load Swatches`) | 2026-06-02 |
| [20] | helpx.adobe.com/indesign/desktop/apply-color/define-and-manage-color-assets/organize-and-reuse-color-swatches.html (`Save Swatches`, `Load Swatches`) | 2026-08-07 |
| [21] | helpx.adobe.com/indesign/using/using-fonts.html (`Document Fonts` folder installed with the document) | last updated 2024-12-11 |
| [22] | github.com/nsfmc/swatch README.rst (`swatch.write`, Color Group, modes, types) | read 2026-09-11 |
| [23] | pypi.org/pypi/pyroscope-io/json (1.2.3, wheels cp310–cp314), pypi.org/pypi/cffi/json (2.1.1, cp315 wheels), github.com/grafana/pyroscope-rs releases (lib 2.1.1, 2026-07-21) | read 2026-09-11 |
| [24] | Kelman IDML `designmap.xml` `RGBProfile` / `CMYKProfile` (sRGB IEC61966-2.1; Coated FOGRA39 or U.S. Web Coated (SWOP) v2) | read 2026-09-11 |
| [25] | afterhourscreativestudio.com (Colorplan CMYK, "the mills do not publish CMYK or pantone references"), superluxurybusinesscards.co.uk, cardstock-warehouse.com (three differing trade CMYK lists) | 2021-05-01, 2025-05-07, read 2026-09-11 |
| [26] | `C/tooling/scripta.md` (Scripta 2.0/315 removal pending) | 2026-09-11 |
| [27] | `diskutil info /` (APFS, case-insensitive); `lsof -nP -i :6001`; `ps -ww -p 97732` | 2026-09-11 22:33 |
| [28] | Second pass: `SP/design/probe-cloud/palette-check.py` (coloraide recompute of every palette table), `SP/design/probe-cloud/manifests-table.md`, `shasum -a 256` over `M/fonts/owned`, Drive `01.Fonts`, and the TK9 trees; Drive walk, `fd -H` counts, `zipfile` IDML scan, `sha256` of the ledger's `kelman_presentation_sources` and `restored_assets` records, twin hashing, `du -sk`, `jq` over `PresetDocumentProfileDataV10.json`, `defaults read`, `mdfind`, `mdls`, `Info.plist` reads, `pgrep`, `ps -p`, `lsof` | 2026-09-11 23:00–23:30 |
| [29] | pypi.org/project/swatch (0.4.0, 2014-04-16); the installed `swatch` 0.4.0 wheel's `WHEEL` metadata and `swatch/{__init__,parser,writer}.py` | read 2026-09-11 23:00 |
| [30] | typefaceapp.com/help/articles/activation (temporary activation, `Clear temporary activations on quit`), /help/troubleshooting/backups (fonts first, then the backup; automatic backups on quit, 10 days) | read 2026-09-11 23:00 (pages undated) |
| [31] | helpx.adobe.com/indesign/using/using-fonts.html as served through exa (`Document installed fonts`) | last updated 2024-11-29, read 2026-09-11 23:20 |
| [32] | `cloud-libraries.md` [02.2]–[02.4], [02.7], [09]; `gui-illustrator.md` [06.1]–[06.3]; `architecture.md` [02] | 2026-09-11 |
| [33] | Read-only listings of 2026-09-14: `ls -lO`, `fd -H -I`, `stat -f '%z %b %Sm'`, `du -sk`, `shasum -a 256`, `file`, `unzip -l`, `xxd -l 10 -p` over the Drive mounts, iCloud Drive, both OneDrive mounts, `~/Library/Application Support/Adobe/UXP`, `~/.Trash`; a `struct` walk of the two ASE files | 2026-09-14 |
| [34] | `C/tooling/asset-recovery-sources.json` `recorded_cloud_consolidation` rows naming `TK9 V3` (`jq`, 2026-09-14) | 2026-09-14 |
| [35] | ai-scripting.docsforadobe.dev/jsobjref/{SwatchGroup, Symbols, PlacedItem, IllustratorSaveOptions}.html (`SwatchGroup.addSwatch`, `Symbols.add(sourceArt, registrationPoint)`, `IllustratorSaveOptions` defaults) | read 2026-09-14 |

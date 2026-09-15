# [INDESIGN_AND_ACROBAT_DEPTH]

Research date 2026-09-11. Every claim below carries the page it came from. Anything a live page did not state is marked `unverified`. Two vendor caveats: `helpx.adobe.com` returns HTTP 403 to plain fetches, so Adobe pages were read through readers that render them; and Adobe restructured both help trees in 2026, so the live shapes are `helpx.adobe.com/indesign/desktop/<area>/<sub>/<page>.html` and `helpx.adobe.com/acrobat/desktop/<area>/<sub>/<page>.html`, with `/using/*.html` surviving as legacy aliases.

## [00]-[VERSION_ANCHORS]

| [PRODUCT] | [CHANNEL] | [VERSION] | [DATE] | [SOURCE] |
| :-------- | :-------- | :-------- | :----- | :------- |
| InDesign | Shipping | 21.5.1 (InDesign 2026) | 2026-08-13 | https://community.adobe.com/announcements-670/what-s-new-in-indesign-21-5-1-2026-create-more-accessible-documents-and-work-faster-1631581 |
| InDesign | Help release notes | 21.4.1, page updated 2026-06-18 | 2026-06 | https://helpx.adobe.com/indesign/desktop/whats-new/release-notes.html |
| InDesign | Beta | 21.6.0.23 | 2026-08-17 | https://community.adobe.com/announcements-670/ai-assistant-is-now-available-in-additional-languages-with-indesign-21-6-beta-1636297 |
| InDesign | LTS | 20.5.2 | 2026-02 | https://helpx.adobe.com/indesign/using/whats-new.html |
| Acrobat | Continuous | 26.002.21901 | 2026-09-08 | https://www.adobe.com/devnet-docs/acrobatetk/tools/ReleaseNotesDC/index.html |
| Acrobat | Classic 2024 | 24.001.30429 | 2026-09-08 | same |
| Acrobat | Classic 2020 | 20.005.30838 | 2025-12-09 | same |

The Beta channel is ahead of shipping by roughly one feature train. Adobe's own help pages document shipping only, so beta-only surfaces (Copy Editor, the newest AI Assistant languages) have no helpx article at all.

InDesign 21.0 (MAX, October 2025) is the release that matters structurally: Flex Layout, real-time browser text editing through InCopy on the web, PDF and Illustrator conversion into editable InDesign, and math expressions. 21.2 (January 2026) added AI alt text and ARIA roles for EPUB; 21.3 (March 2026) added Flex Layout under object styles plus nested Flex; 21.4 (May 2026) added layout-aware Rewrite and Fit Text; 21.5 (August 2026) added table accessibility (header rows and columns, captions) in PDF and EPUB export, keyboard-shortcut search, and Data Merge with UTF-8 and multiline records.

---

# [PART_A]-[INDESIGN]

## [01]-[STEPHEN_KELMAN]

### [01.1]-[CORRECTION]

The brief conflated two unrelated sellers. They are separate people and separate catalogues.

| [ENTITY] | [WHO] | [SITE] | [GUMROAD] | [SELLS] |
| :------- | :---- | :----- | :-------- | :------ |
| Stephen Kelman | Glasgow graphic designer, ~20 years, editorial and Swiss-style grids | https://stephenkelman.co.uk/ | https://stephenkelman.gumroad.com/ | InDesign modular baseline grid systems |
| Typefool | Jen Ditters, Netherlands brand studio | https://typefool.com | https://typefool.gumroad.com/ | Brand-guidelines templates, `.idml`, 15-field three-column grid |

"Colours Grid System" and "f-height" are Kelman's alone. Nothing on typefool.com mentions either. Kelman's channels: Instagram `@stephenkelman_` (bio "Editorial design, Swiss typography and grid systems"), YouTube `@stephenkelman_`, Behance https://www.behance.net/stephenkelman (member since 2012-08-29, 588k views), TikTok `@stephenkelman3`. No newsletter found.

### [01.2]-[CATALOGUE]

Gumroad renders client-side, so its prices below come from search snippets; Creative Market prices were fetched.

| [PRODUCT] | [FORMAT] | [FIELDS] | [BASELINE] | [PRICE] |
| :-------- | :------- | :------- | :--------- | :------ |
| Colours Grid System Series | A4, A5, US Letter, 6×9 in | 18 | 11 pt (A4), 8.5 pt (A5) | Free, `$0+` |
| A4 Grid System | A4 portrait, 8.5 pt body | 18 | 10 pt, optional f-height | £19 |
| eBook PDF Grid System | A5 5.8×8.3 in, 2 col | 18 | 10 pt | $28 / $35 / $90 (Creative Market, listed 2021-07-08) |
| A3 Presentation | A3 + Tabloid, both orientations | 36 | 12 pt + 12 pt document grid | £19 / $28 |
| A2 Presentation | A2, both orientations | 72 | 11.5 pt | — |
| A1 Presentation | A1, both orientations | 72 | 11.5 pt | — |
| Swiss Style A0 Poster | A0 841×1189, light + dark, 10 layouts | 16 | 36 pt | — |
| Digital Presentation | 16:9 landscape + portrait | 24 | 15 pt | — |
| Insight Presentation | 16:9 | 48 and 36 | yes, value not stated | — |
| Bento Style Presentation | 16:9 | 36 | document grid | — |
| Serif Series US Letter | US Letter | 36 | 12 pt + f-height | — |
| A4 Landscape Portfolio | A4 landscape | 32 | 10 pt | £19 |
| Architecture Studio Presentation and Portfolio Bundle, A-Series | A-series | — | — | ~$269 |

Product pages: https://stephenkelman.co.uk/colours-grid-system-series, https://stephenkelman.co.uk/a4-editorial-design-grid-system-for-indesign, https://stephenkelman.co.uk/a3-presentation-grid-system-for-indesign, https://stephenkelman.co.uk/a2-presentation-grid-system-for-indesign, https://stephenkelman.co.uk/a1-presentation-grid-system-for-indesign, https://stephenkelman.co.uk/swiss-style-a0-poster-grid-system-for-indesign, https://stephenkelman.co.uk/digital-presentation-grid-system-for-indesign, https://stephenkelman.co.uk/insight-presentation-grid-system-for-indesign, https://stephenkelman.co.uk/bento-style-brand-presentation-grid-system-for-indesign, https://creativemarket.com/stephenkelman.

**No 16:10 anywhere in the catalogue, and no ANSI sizes beyond Letter and Tabloid.** Files ship as `.indd` and `.idml` with modular grid, baseline grid, optional f-height grid, image-caption grid, style sheets (headers A–F, numbered lists, bullets, quotes, captions), proportional leading and sample layouts. **Fonts are not included**; layouts call Adobe Fonts, so a Creative Cloud subscription is a hard dependency. No Figma, no native Illustrator.

### [01.3]-[METHOD]

**f-height**, verbatim from the A4 product page: *"An f-height grid complements the baseline grid. It sits at the height of the body copy's lowercase f on every line, giving the typographer the option of running images, graphics and display typography that align perfectly with the body copy."*

That is the entire idea. A baseline grid gives one horizontal line per line of text, at the bottom. An f-height grid gives a second line per text line, at the ascender of lowercase *f* — the practical optical top of the text block. The two bracket each line, so an image edge or rule snaps flush to the visual top rather than to a baseline. It is not x-height and not cap-height: in most text faces lowercase *f* reaches the ascender line, above cap height, so it marks the tallest routine lowercase extent. Designers Bookshop's Grid Calculator PE sells the same idea as "image-lines based on x- or H-height" (https://designersbookshop.com/products/grid-calculator-pe/), which confirms the technique is a general one rather than a proprietary invention.

**Baseline scales with format** — a real, consistent rule visible across his catalogue: 8.5 pt (A5) → 10 pt (A4, US Letter presentation) → 11.5 pt (A1, A2) → 12 pt (A3, Serif Letter) → 15 pt (16:9 screen) → 36 pt (A0 poster). Field count rises with format: 16 (A0), 18 (A4), 24 (16:9), 32, 36, 48, 72 (A1/A2).

**Proportional leading** means sizing type as a percentage of leading rather than the reverse. His free calculator https://pointsizer.com/ makes it explicit: input a body leading value plus two percentages (body/small size %, header size %), and it emits sizes and leading for small, body and header. Leading is the primitive; point size is derived. That is what locks every style to the baseline grid.

**Margins** are conceptual in his writing, not numeric. https://stephenkelman.co.uk/journal/margins-are-not-an-afterthought (2026-07-08) argues modernist tight margins make the page edge an active element, classical generous margins slow reading, asymmetry reads as intentional, long formats want wider inner margins, and creep and cutter tolerance must be respected. The Colours system uses "narrow, equidistant margins designed for asymmetric layouts". No margin formula or field-derived ratio is published.

Type-scale ratios, measure targets and hanging-punctuation rules are **not published** anywhere public by him.

### [01.4]-[FREE_MATERIAL]

| [ITEM] | [URL] | [DATE] |
| :----- | :---- | :----- |
| Colours Grid System Series, four formats, full styles | https://stephenkelman.gumroad.com/ (`gumroad.com/l/jZgwn`) | — |
| "How to create a modular grid in InDesign" | https://stephenkelman.co.uk/journal/how-to-create-a-modular-grid-in-indesign | 2026-08-18 |
| "Margins are not an afterthought" | https://stephenkelman.co.uk/journal/margins-are-not-an-afterthought | 2026-07-08 |
| "InDesign — Align to Baseline Grid and Modular Grid" | https://www.youtube.com/watch?v=Ikzft91PlY0 | unverified |
| "Working with Modular Baseline Grids" | https://www.youtube.com/watch?v=KtgygB-gvFo | 2020-10 |
| Point Sizer, leading-first calculator | https://pointsizer.com/ | — |
| Tschichold Archive, 2,184 works, 21,242 scans, CC-BY-SA 4.0 | https://tschicholdarchive.com/ | — |
| Font League, weekly font census of new sites | https://fontleague.com/ | live |

### [01.5]-[VERDICT]

**Do not buy for the method — the method is public.** f-height is one sentence and reproducible in ten minutes: set the baseline grid, then add a second grid or guide layer offset upward by the f-ascender height of the body face at body size. Proportional leading is free at pointsizer.com. The baseline-per-format ladder is readable off his own product pages.

**Buy for the artefact** if the labour matters: a prebuilt `.indd`/`.idml` with 36–72 fields, an image-caption grid, headers A–F, quote and list styles tuned to the baseline, and roughly ten sample layouts, for £19–£35. The A1 and A2 72-field systems are described as originally developed for architectural studios, for plans and large-scale presentation boards — the closest fit to board work. The hard caveat is that no fonts ship, so Adobe Fonts substitution risk is acknowledged by the seller.

**The size catalogue is generated by our own tools, not bought.** Grid Calculator Publishing Edition (https://designersbookshop.com/products/grid-calculator-pe/; macOS-only, subscription, InDesign 2026 support 2025-12-16, v1.4 with Layout Wizard and Presets 2026-04-03) is the studied product: its fitting baseline grid (`N = round(T / L)`, `L_fit = T / N`), margins from the document grid, Quick, Modular, and Smart modes, generated styles with x- or H-height image lines, and multi-parent files are the capabilities `@rasm/typographic-grid` (`grid`) and the `indesign_apply_grid`, `build_template`, and `build_size_catalog` tools reproduce (main plan decision 16; `plan/research/grid-calculator/README.md`); the one grid rule across the 59 catalogue files is `gui-indesign.md` [07].

---

## [02]-[SHEET_SIZE_CATALOGUE]

### [02.1]-[ISO_216]

ISO 216:2007 (edition 2, published 2007-09, **confirmed current 2021-09-10**, https://www.iso.org/standard/36631.html) defines the trimmed A and B series. Table 1 and Table 2 verbatim from the standard's public preview, https://cdn.standards.iteh.ai/samples/36631/c0883203ea25445c9992bb09343620c5/ISO-216-2007.pdf:

| [A] | [mm] | [B] | [mm] |
| :-- | :--- | :-- | :--- |
| A0 | 841 × 1189 | B0 | 1000 × 1414 |
| A1 | 594 × 841 | B1 | 707 × 1000 |
| A2 | 420 × 594 | B2 | 500 × 707 |
| A3 | 297 × 420 | B3 | 353 × 500 |
| A4 | 210 × 297 | B4 | 250 × 353 |
| A5 | 148 × 210 | B5 | 176 × 250 |
| A6 | 105 × 148 | B6 | 125 × 176 |

A0 has an area of 1 m²; the standard derives x = 0.841 m, y = 1.189 m. The standard's own note adds 2A0 = 1189 × 1682 and 4A0 = 1682 × 2378. The B series is the geometric mean between adjacent A sizes, and the standard says it is "intended for use only in exceptional circumstances".

### [02.2]-[ANSI_AND_ASME]

ASME Y14.1-2020, *Drawing Sheet Size and Format*, published **2020-12-18**, consolidated the former inch (Y14.1) and metric (Y14.1M) standards into one document (https://webstore.ansi.org/standards/asme/asmey142020, history at https://en.wikipedia.org/wiki/ANSI/ASME_Y14.1).

| [NAME] | [in] | [mm] | [RATIO] | [ALIAS] | [~ISO] |
| :----- | :--- | :--- | :------ | :------ | :----- |
| ANSI A | 8.5 × 11 | 216 × 279 | 1.2941 | Letter | A4 |
| ANSI B | 11 × 17 | 279 × 432 | 1.5455 | Ledger / Tabloid | A3 |
| ANSI C | 17 × 22 | 432 × 559 | 1.2941 | — | A2 |
| ANSI D | 22 × 34 | 559 × 864 | 1.5455 | — | A1 |
| ANSI E | 34 × 44 | 864 × 1118 | 1.2941 | — | A0 |
| ANSI F | 28 × 40 | 711 × 1016 | 1.4286 | — | A0 |
| ANSI G/H/J/K | roll formats, 11/28/34/40 in high, variable width | — | — | — | — |

Size F breaks the alternating-ratio series. G is 11 in high in 8.5 in width increments up to 90 in. The metric companion adds elongated sizes A1.0 = 594 × 1189, A2.1 = 420 × 841, A2.0 = 420 × 1189, A3.2 = 297 × 594, A3.1 = 297 × 841, A3.0 = 297 × 1189.

**Naming hazard, confirmed.** The US National CAD Standard V6 UDS Module 2 sample figure is captioned *"11" x 17" Mock-up sheet of a full size ANSI D 34" x 44" sheet"* (https://www.nationalcadstandard.org/ncs6/pdfs/ncs6_uds2.pdf) — which contradicts ASME Y14.1, where 34 × 44 is size **E**. The US Army Corps A/E/C Graphics Standard Release 2.2 (ERDC/ITL SR-23-1, August 2023) Table 2-1 gets it right: F 28.0 × 40.0, E 34.0 × 44.0, D 22.0 × 34.0, C 17.0 × 22.0, B 11.0 × 17.0, A 8.5 × 11.0 (https://www.wbdg.org/FFC/AECCAD/ERDC_ITL_SR_23_1_2023.pdf). **Name sheets by dimension in the template, never by letter alone.**

### [02.3]-[ARCH]

ARCH is a de facto US trade series with no ISO or ASME standard behind it. It is the series US architects actually plot on, while ANSI is the engineering series (https://www.archtoolbox.com/paper-sizes/, https://papersizes.net/us, https://www.archisoup.com/architectural-paper-sizes).

| [NAME] | [in] | [mm] | [RATIO] |
| :----- | :--- | :--- | :------ |
| ARCH A | 9 × 12 | 229 × 305 | 3:4 |
| ARCH B | 12 × 18 | 305 × 457 | 2:3 |
| ARCH C | 18 × 24 | 457 × 610 | 3:4 |
| ARCH D | 24 × 36 | 610 × 914 | 2:3 |
| ARCH E | 36 × 48 | 914 × 1219 | 3:4 |
| ARCH E1 | 30 × 42 | 762 × 1067 | 5:7 |
| ARCH E2 | 26 × 38 | 660 × 965 | 13:19 |
| ARCH E3 | 27 × 39 | 686 × 991 | 9:13 |

Sources: https://www.bgcarto.com/download/Paper%20Sizes.pdf, https://papersdb.com/architectural, https://resources.printhandbook.com/pages/paper-size-chart.php, https://www.engineersupply.com/Drawing-Size-Reference-Table.aspx.

### [02.4]-[US_LOOSE]

| [NAME] | [in] | [mm] |
| :----- | :--- | :--- |
| Letter | 8.5 × 11 | 216 × 279 |
| Legal | 8.5 × 14 | 216 × 356 |
| Tabloid | 11 × 17 portrait | 279 × 432 |
| Ledger | 17 × 11 landscape | 432 × 279 |
| Half Letter | 5.5 × 8.5 | 140 × 216 |
| Executive | 7.25 × 10.5 | 184 × 267 |

Tabloid and Ledger are the same sheet in two orientations; Canva's size guide states this explicitly (https://www.canva.com/sizes/north-american-paper/), as does https://www.archtoolbox.com/paper-sizes/. Legal is not part of any ANSI drawing series.

### [02.5]-[PRACTICE_SPLIT]

| [DOMAIN] | [SERIES] | [EVIDENCE] |
| :------- | :------- | :--------- |
| US architecture, plotted sets | ARCH D 24×36 and ARCH E 36×48, E1 30×42 for tight plotters | archtoolbox, archisoup |
| US federal / USACE contract documents | **ANSI D sheets mandatory**, ANSI E or F for oversized (installation master plans, civil works). International projects use **ISO A1**, A0 oversized | ERDC/ITL SR-23-1 clause 2.1.1 |
| US engineering | ANSI A–E | ASME Y14.1-2020 |
| DIN / ISO world, technical drawings | ISO 5457 trimmed A0–A4 plus elongated sizes | https://cdn.standards.iteh.ai/samples/29017/e46c0ec5d98f470aab82dae76889f229/ISO-5457-1999.pdf |
| European competitions | **A1 portrait**, 594 × 841 | Europan 18 rules |
| Open-ideas competitions | **A2 landscape single board**, or A1 landscape 150 dpi | Buildner, Volume Zero |

ISO 5457:1999 (+A1:2010) Table 1 gives trimmed sheet, drawing space and untrimmed sheet: A0 trimmed 841 × 1189, drawing space 821 × 1159, untrimmed 880 × 1230; A1 594 × 841 / 574 × 811 / 625 × 880; A2 420 × 594 / 400 × 564 / 450 × 625; A3 297 × 420 / 277 × 390 / 330 × 450; A4 210 × 297 / 180 × 277 / 240 × 330. **Those drawing-space numbers are the DIN answer to "what are the margins", and they belong in the template as the default type area for technical sheets.**

### [02.6]-[SHEET_ORGANISATION]

The ERDC A/E/C Graphics Standard is free, current (August 2023), and the most specific published sheet grammar in US practice. Directly usable in a parent page:

- **Clause 2.1.2, margins**: minimum 3/16 in from paper edge to any item; border and grid callouts centred vertically and shifted right as far as practical to keep a binding edge on the left.
- **Clause 2.1.3, areas**: three zones — Production Data Area (optional, upper right, outside the drawing border), Drawing Area, Title Block Area (vertical, right margin).
- **Clause 2.1.5, modules**: the Center's border sheet uses **square modules of 1.5 in × 1.5 in**. The grid is labelled A, B, C … from lower left upward and 1, 2, 3 … from upper left rightward, **skipping I and O**. Drawing blocks are unions of whole modules referenced from their lower-left corner; a graphic must fill its block exactly, and leftover space in a block may not host another item.
- **Clause 5.1.2, text height**: minimum plotted height for dimensions, notes, callouts and schedule text **3/32 in (2.4 mm)**; subtitles **1/8 in (3 mm)**; titles **3/16 in (5 mm)**. Text height and width take equal values. **Line spacing equals one-half the text height** — i.e. leading = 1.5× cap height.

The NCS UDS lecture summary gives a module of roughly 5-3/4 in high × 6 in wide, notes with keynotes and legends in the rightmost column and the key plan in the lowest module of that column (https://constdociniar.wordpress.com/wp-content/uploads/2012/08/lecture3_uds.pdf). The two module sizes differ because both standards say the module is user-defined but must stay constant across the set.

### [02.7]-[DIN_TYPE_SCALE]

ISO 3098-1:2015 defines lettering for technical product documentation. Nominal sizes are a √2 ladder derived from ISO 216: **1.8, 2.5, 3.5, 5, 7, 10, 14, 20 mm** (cap height *h*), with lowercase x-height `c1 = (10/14)h ≈ 0.714h`, line width `h/10` (type B) or `h/14` (type A), and character spacing of at least twice the line width (https://cdn.standards.iteh.ai/samples/65679/d6c3a5303d6a48cba4486718d1947b0c/ISO-3098-1-2015.pdf, https://www.iso.org/standard/65679.html). In practice: 2.5 mm for general notes and dimensions, 3.5 mm for SECTION A-A and schedule headings, 5 mm for drawing titles, 7 mm for cover sheets and title blocks (https://blog.draftsperson.net/text-heights-in-drawings/).

**This is a standards-grounded, √2 type scale that already matches the A-series paper ladder**, and it is the correct scale for the technical-sheet half of the catalogue. Convert to points at 1 mm = 2.8346 pt: 2.5 mm ≈ 7.09 pt cap height, which for a face with cap height ≈ 0.70 em means roughly 10 pt type.

### [02.8]-[DIGITAL_CANVASES]

There is no standard here, only display hardware and competition briefs. 16:10 = 1.600 exactly.

| [CANVAS] | [px] | [RATIO] | [NOTE] |
| :------- | :--- | :------ | :----- |
| 3840 × 2400 | — | 1.600 | True 16:10, WQUXGA |
| 2560 × 1600 | — | 1.600 | True 16:10, the old 13" MacBook Pro panel |
| 1920 × 1200 | — | 1.600 | True 16:10 baseline |
| 1920 × 1080 / 3840 × 2160 | — | 1.778 | 16:9, the projector and Keynote default |
| 2560 × 1664 | — | 1.538 | MacBook Air 13" M5 native, https://www.apple.com/macbook-air/specs/ |
| 3024 × 1964 | — | 1.540 | MacBook Pro 14" native, https://www.apple.com/macbook-pro/specs/ |
| 3456 × 2234 | — | 1.547 | MacBook Pro 16" native |
| 5120 × 2880 | — | 1.778 | Studio Display, https://www.apple.com/studio-display/specs/ |

**Verdict.** Modern notched MacBooks are **not** 16:10 — they are ≈1.54, because the menu-bar strip beside the camera housing adds height. A 16:10 canvas therefore letterboxes slightly on a MacBook and pillarboxes badly on a 16:9 projector. A board that is projected or screen-shared is 16:9, so the one master digital canvas is `Digital 3840x2160` (`gui-indesign.md` [07] row 21) and the other screen sizes are the named screen formats of that file's screen table; InDesign document intent sets pixel documents at 72 ppi, so 3840 × 2160 px = 53.33 × 30 in at 72 ppi.

### [02.9]-[COMPETITION_FORMATS]

These are the only hard, quotable board conventions from 2025-26.

| [COMPETITION] | [REQUIREMENT] | [URL] |
| :------------ | :------------ | :---- |
| Europan 18 | Three vertical A1 panels, 594 × 841 mm, PDF max 20 MB; a blank 60 × 40 mm box top-left for the anonymity code with the city name beside it; panels numbered 1–3 top-right; every drawing carries a graphic scale; A4 vertical text document 3–4 pages; three symbol images ≤ 1 MB; 800-character text | https://www.europan-europe.eu/en/session/europan-18/rules/ |
| Buildner Unbuilt Award 2026 | One A2 landscape panel, ≤ 10 MB, single static board, anonymous | https://architecturecompetitions.com/unbuilt2026/ |
| Little Big Loo 2025 | A1 841 × 594 landscape, 150 dpi, JPEG, ≤ 5 MB; "no set minimum font size; however, it is important to keep the font legible"; 200-word text excluding captions, dimensions and legends | https://volumezerocompetitions.com/little-big-loo-2025 |
| Linha do Horizonte 2026 | One PDF up to 4 pages, A1 594 × 841, portrait or landscape, ≤ 20 MB; site plan 1:500, general plan 1:200, units 1:100 and 1:50, one section 1:100 | https://archup.net/linha-do-horizonte-competition-2026/ |
| eVolo Skyscraper | Two boards **24 in high × 48 in wide** horizontal, 150 dpi, RGB, JPG, participation number top-right, filenames `[reg]-1.jpg` | https://competitions.archi/competition/2025-skyscraper-competition/ |

24 × 48 in = 610 × 1219 mm, a 1:2 ratio that matches no paper series — it needs its own row in the catalogue. **No brief in this set specifies a typeface, a minimum font size, or a resolution above 150 dpi.** The commonly repeated "300 dpi standard" for boards is unverified.

### [02.10]-[RECOMMENDED_CATALOGUE]

The catalogue that covers every case above is the 59-file sentence of `gui-indesign.md` [07]; the families it draws from this section:

- **ISO A**: A0, A1, A2, A3, A4, A5, A6, each portrait and landscape (the template can carry one preset per size with orientation toggled at creation)
- **ISO B**: B0–B6, portrait
- **ANSI**: A, B, C, D, E, F
- **ARCH**: A, B, C, D, E, E1, E2, E3
- **US loose**: Letter, Legal, Tabloid, Half Letter
- **Digital**: the master `Digital 3840x2160` and the named screen formats of InDesign 21.6's Web and Mobile intents
- **Competition specials**: 24×48 in landscape, A1 portrait with the Europan anonymity block already drawn on a parent page

---

## [03]-[NATIVE_FEATURES_FOR_ONE_MASTER_TEMPLATE]

### [03.1]-[WHAT_DEFAULT_ACTUALLY_MEANS]

InDesign has three levels, and confusing them is the most common template failure:

1. **Application defaults** — set with **no document open**. Paragraph, character, object, table and cell styles; swatches; document setup; dictionary and language; preferences. Everything created there lands in every new document from that point on. Existing documents are untouched, and resetting preferences wipes it all. Sources: https://creativepro.com/setting-defaults-indesign/ (Erica Gamet, 2015-10-20), https://creativepro.com/creating-default-swatches/, https://creativepro.com/indesign-how-to-set-default-paragraph-styles/.
2. **Document defaults** — set with a document open and nothing selected. Apply to new objects in that document only.
3. **Object formatting** — set with something selected.

**Do not edit `[Basic Paragraph]`.** The house rule from the CreativePro forums stylesheet guidelines (https://creativepro.com/topic/guidelines-for-stylesheet-creation/) is: paragraph styles are never based on `[Basic Paragraph]`, object styles are never based on `[Basic Graphics Frame]` or `[Basic Text Frame]`, a named Default style is created for both, and parent/child depth is held to **three tiers**. Create a `Base` style instead, and hang everything from it.

The template artefact itself is a `.indt`, which opens as an untitled copy so the master cannot be overwritten (https://helpx.adobe.com/indesign/desktop/save-export-and-publish/save-and-export/save-documents.html). For a size catalogue, the `.indt` per size plus a **Book file** with one style-source document is the cleanest structure: synchronising copies styles, swatches, text variables, numbered lists, cross-reference formats, conditional-text settings, trap presets and parent pages from the style source into every other document, replacing matching names, adding new ones, leaving unmatched ones alone (https://helpx.adobe.com/indesign/desktop/create-and-organize-pages/create-and-manage-book-files/sync-documents-books.html, updated 2026-06-02; behaviour analysis at https://creativepro.com/synchronize-multiple-indesign-documents/). The caveat on that page: synchronise early to preserve parent-page overrides, and use one style source, or exclude documents with divergent parent pages.

### [03.2]-[GRID_CONSTRUCTION]

Nigel French's method (https://creativepro.com/working-with-grids-in-indesign/, InDesign Magazine issue 40) is the procedure to encode:

- Grid increment equals body leading. Never auto leading — 11 pt auto gives 13.2 pt, which will not divide.
- Start at 0, Relative To **Top Margin**, so the grid shows only inside the type area.
- Lower View Threshold below the 75% default so the grid stays visible at working zoom.
- Make the type-area height divisible by the increment: measure the partial increment at the bottom with a rectangle frame, then add its height to the bottom margin in Layout ▸ Margins and Columns using `+` arithmetic in the field.
- For square grid fields, match rows and columns to the page ratio: 13 rows × 10 columns on Letter (1:1.3), 14 × 10 on A4 (1:1.4).
- For rows that hold whole numbers of lines, set the row gutter equal to the grid increment, then subtract `(rows − 1)` gutters from the type-area height and check the remainder divides. Worked: 53 lines − 5 gutters = 48 ÷ 6 rows = 8 lines per row; 55 − 6 = 49 ÷ 7 = 7; 55 − 7 = 48 ÷ 8 = 6.
- Put superimposed grids (a 12-column with 3- and 4-column permutations) on their own coloured, lockable layers.

Per-frame grids: Object ▸ Text Frame Options ▸ Baseline Options ▸ Use Custom Baseline Grid, Relative To Top of Frame. Unlike the document grid it has no View Threshold and shows at any zoom, and Align to Baseline Grid then targets the frame's grid rather than the document's (https://creativepro.com/understanding-indesigns-text-frame-options/, https://creativepro.com/how-to-create-custom-baseline-grids-in-indesign/, 2025-10-14; https://creativepro.com/how-to-balance-text-columns-automatically-and-with-baseline-grid/, 2026-08-25).

### [03.3]-[FEATURE_MAP]

| [#] | [FEATURE] | [WHY IT MATTERS IN THE MASTER TEMPLATE] | [URL] |
| :-- | :-------- | :-------------------------------------- | :---- |
| 1 | Paragraph and character styles, Based On, style groups | Every text decision lives in one definition; three-tier depth keeps it legible | https://helpx.adobe.com/indesign/desktop/format-and-style-text/text-styles/create-and-edit-text-styles.html |
| 2 | Next Style | Applies a whole heading-to-body cascade in one command over pasted copy | https://helpx.adobe.com/indesign/desktop/format-and-style-text/text-styles/apply-sequential-text-styles.html |
| 3 | GREP styles | Formats by pattern with no tagging: units, figure numbers, run-in leads stay right forever | https://helpx.adobe.com/indesign/desktop/format-and-style-text/text-styles/create-grep-styles.html |
| 4 | GREP Find/Change, expression builder | House cleanup encoded as reusable queries | https://helpx.adobe.com/indesign/desktop/language-and-proofing/glyphs-characters-and-expressions/find-and-replace-with-text-patterns-grep.html · https://helpx.adobe.com/indesign/desktop/language-and-proofing/glyphs-characters-and-expressions/construct-a-grep-expression.html |
| 5 | Nested styles, nested line styles | Run-in heads and first-line small caps inside the style, not as overrides. Keep them simple — Nigel French warns complex nests cost more to build than to hand-format and break in others' hands | https://helpx.adobe.com/indesign/desktop/format-and-style-text/text-styles/created-nested-styles.html · https://creativepro.com/automatic-text-formatting-in-indesign-with-nested-styles/ (2024-06-06) |
| 6 | Keep Options | Heads never strand at column feet when copy reflows | https://helpx.adobe.com/indesign/desktop/format-and-style-text/composition-and-text-wrapping/paragraph-break-options-in-indesign.html |
| 7 | H&J and composer | Sets the grey value of body text once. Adobe Paragraph Composer is also a precondition for Span/Split Columns | https://helpx.adobe.com/indesign/desktop/format-and-style-text/composition-and-text-wrapping/set-text-composition.html · https://helpx.adobe.com/indesign/desktop/format-and-style-text/composition-and-text-wrapping/control-hyphenation-and-word-breaks.html |
| 8 | Optical Margin Alignment | One switch makes every measure edge read straight. **Story-level, not in paragraph styles** | No dedicated 2026 page. Nearest https://helpx.adobe.com/indesign/desktop/format-and-style-text/character-formatting/apply-drop-caps-text-positioning.html · https://creativepro.com/hanging-punctuation-with-optical-margin-alignment-in-indesign/ |
| 9 | Baseline grid, custom per-frame grid, Align to Grid | Cross-column and cross-spread registration | https://helpx.adobe.com/indesign/desktop/layout-and-grid-tools/grids/use-a-baseline-grid.html · https://helpx.adobe.com/indesign/desktop/layout-and-grid-tools/grids/create-customize-layout-grids.html |
| 10 | Span Columns / Split Columns | Straddle heads and short split lists in one frame. Also the one-style trick for space around a list: Span All with space before and after | No dedicated page. https://creativepro.com/indesigns-magical-span-and-split/ · https://creativepro.com/adding-space-above-below-lists-span-column/ |
| 11 | **Flex Layout** (21.0, Oct 2025) | Frames that reflow as content grows — the first new structural primitive since Liquid Layout; now settable through object styles and nestable (21.3) | https://helpx.adobe.com/indesign/desktop/layout-and-grid-tools/apply-layout-adjustments/flex-layout-overview.html · .../create-a-flex-layout.html · .../flex-layout-conflicts.html |
| 12 | Copy Editor (Beta) | Modal, text-only, no formatting or styles, for typing lag in long or footnote-heavy stories. **Not new** — shipped in InDesign 2020 v15.1.2 (Aug 2020), still Beta, no helpx page. Reported file corruption in 2021 and no speed gain on table-heavy documents | https://creativepro.com/whats-new-in-indesign-2021/ · https://community.adobe.com/questions-671/copy-editor-beta-864741 |
| 13 | Text variables | Running heads, chapter numbers, file name and output date write themselves from applied styles | https://helpx.adobe.com/indesign/desktop/add-and-manage-text/conditional-and-variable-text/text-variables-overview.html · .../create-running-headers-footers.html |
| 14 | Sections and numbering, section markers | Roman front matter, arabic body, no manual renumbering | https://helpx.adobe.com/indesign/desktop/create-and-organize-pages/page-numbers-chapters-and-sections/add-page-numbers-and-markers.html |
| 15 | Lists, defined lists, restart/continue across stories | Numbering survives copy moving between frames and documents | https://helpx.adobe.com/indesign/desktop/format-and-style-text/lists-and-numbering/define-and-manage-list-options.html · .../restart-or-continue-numbered-lists.html · .../create-multi-level-lists.html |
| 16 | Cross-references | "See page 14" repaginates itself and exports as a live PDF link | https://helpx.adobe.com/indesign/desktop/indexes-and-references/references-and-bookmarks/insert-cross-references.html |
| 17 | Footnotes and endnotes | Note styling, numbering and column spanning fixed at template level | https://helpx.adobe.com/indesign/desktop/indexes-and-references/footnotes-and-endnotes/create-and-manage-footnotes.html |
| 18 | Conditional text, condition sets | One master file yields client/internal, print/web or language variants | https://helpx.adobe.com/indesign/desktop/add-and-manage-text/conditional-and-variable-text/conditional-text-overview.html |
| 19 | Object styles, anchored-object options, Auto-Size | Captions, pull quotes and sidebars carry their own geometry and grow with their text | https://helpx.adobe.com/indesign/desktop/add-graphics-and-media/manage-object-styles/define-and-apply-object-styles.html · .../create-position-anchored-objects.html · Auto-Size in https://helpx.adobe.com/indesign/desktop/add-and-manage-text/add-and-manage-text-frames/change-text-frame-properties.html |
| 20 | Table and cell styles, region styles, repeating headers | Tables become a style application; header rows repeat across breaks and (21.5) export accessibly | https://helpx.adobe.com/indesign/desktop/add-tables-and-data/table-and-cell-styles/table-and-cell-styles.html |
| 21 | Right-Indent Tab (`Shift+Tab`, `^y`), Indent to Here | Right-aligns to the frame edge with no tab stop, so it survives every frame resize. A leader comes from the rightmost tab stop on the ruler | No dedicated page. https://creativepro.com/easy-toc-formatting-in-indesign/ (Keith Gilbert) · https://helpx.adobe.com/indesign/desktop/format-and-style-text/tabs-indents-and-spacing/set-and-repeat-tabs.html |
| 22 | TOC and TOC styles | Regenerates the contents from heading styles. **Between Entry and Number = `^y` gives page numbers right-aligned with no dot leader**; `^t` plus a leader tab stop gives the traditional look | https://helpx.adobe.com/indesign/desktop/indexes-and-references/add-a-table-of-contents/customize-toc-style.html · .../generate-maintain-tocs.html |
| 23 | Index | Index entry and generated-index styles belong in the template | https://helpx.adobe.com/indesign/desktop/indexes-and-references/create-an-index/generate-and-format-an-index.html |
| 24 | Data Merge | Turns the template into a production engine; UTF-8 and multiline CSV as of 21.5 | https://helpx.adobe.com/indesign/desktop/automation-and-scripting/merge-data/data-merging-overview.html |
| 25 | Interactive form fields, buttons, radio, checkboxes | Prebuilt, prestyled widgets and a fixed tab order ship with the template | https://helpx.adobe.com/indesign/desktop/interactive-elements-and-forms/forms-and-pdfs/create-fillable-forms.html |
| 26 | PDF export presets `.joboptions` | Press-ready and screen-ready output become one menu pick, identical on every machine | https://helpx.adobe.com/indesign/desktop/save-export-and-publish/save-and-export/manage-pdf-presets.html |
| 27 | Preflight profiles `.idpp` | A profile embedded in the template flags spec violations live | https://helpx.adobe.com/indesign/desktop/print/preflight/create-and-manage-preflight-profiles.html |
| 28 | Parent pages, Primary Text Frame, Layout Adjustment | Page furniture inherits; a Primary Text Frame lets a parent swap reflow the story; Layout Adjustment rescues a size change. Primary Text Frame has no dedicated page | https://helpx.adobe.com/indesign/desktop/create-and-organize-pages/create-and-manage-parent-pages/about-parent-pages.html · https://helpx.adobe.com/indesign/desktop/layout-and-grid-tools/apply-layout-adjustments/adjust-page-or-spread-layout.html |
| 29 | Liquid Layout, Alternate Layouts | One template serves A4/Letter or print/tablet from one content stream. Rules: Scale, Re-center, Guide-based, Object-based; one rule per page | https://helpx.adobe.com/indesign/desktop/layout-and-grid-tools/apply-layout-adjustments/liquid-page-rules-overview.html · .../create-alternate-layouts.html |
| 30 | Document presets | Trim, margins, columns, bleed, slug recalled by name — the spine of the size catalogue | https://helpx.adobe.com/indesign/desktop/create-and-organize-pages/create-documents/create-documents-with-presets.html |
| 31 | Find/Change queries | House cleanup routines travel with the template | https://helpx.adobe.com/indesign/desktop/language-and-proofing/glyphs-characters-and-expressions/save-and-manage-find-and-replace-queries.html |
| 32 | Keyboard shortcut sets `.indk` | Style and script shortcuts standardised; 21.5 adds shortcut search | https://helpx.adobe.com/indesign/desktop/get-started/settings-and-preferences/keyboard-shortcuts.html |
| 33 | Menu sets | Hides commands house style forbids, colours the ones it depends on | https://helpx.adobe.com/indesign/desktop/get-started/settings-and-preferences/customize-menus.html |
| 34 | Scripts panel | Template automation a double-click away | https://helpx.adobe.com/indesign/desktop/automation-and-scripting/document-automation/automate-workflows-with-scripts.html |
| 35 | Paragraph borders and shading | Callouts and code blocks as paragraph attributes that reflow, not drawn boxes | https://helpx.adobe.com/indesign/desktop/format-and-style-text/composition-and-text-wrapping/apply-paragraph-borders-and-backgrounds.html |
| 36 | Style Packs and Auto Style | Six paragraph styles mapped to heading/subheading/list/paragraph roles, applied by Sensei on selected frames, offline. Also a design-variant switch: several packs with the same style names | https://helpx.adobe.com/indesign/desktop/format-and-style-text/text-styles/create-a-style-pack.html (2026-06-02) · https://creativepro.com/format-text-automatically-with-style-packs-in-indesign/ (2025-04-29) |
| 37 | `.indt` template, CC Libraries | `.indt` opens as an untitled copy; CC Libraries syndicate styles and colours across a team | https://helpx.adobe.com/indesign/desktop/save-export-and-publish/save-and-export/save-documents.html · .../add-and-manage-text-styles-from-cc-libraries.html |
| 38 | Export and import user settings | Moves workspaces, menu sets, keyboard shortcuts and glyph sets to another machine without hand-copying folders | https://helpx.adobe.com/indesign/desktop/get-started/settings-and-preferences/export-and-import-user-settings.html |

### [03.4]-[JUSTIFICATION_STARTING_VALUES]

Charles Nix of Monotype (https://creativepro.com/spacing-type-in-indesign/, 2022-07-01, and the video at https://creativepro.com/how-to-get-better-justified-type-in-indesign/, last modified 2025-08-29): 100% word spacing and glyph scaling and 0% letter spacing are the font's own values, so treat them as the origin, not a rule. Glyph scaling is defensible at **98 / 100 / 102** and nowhere beyond. Word spacing tightens by lowering the Desired value — which works for ragged text too, despite the dialog's name. Ilene Strizver's hyphenation floor (https://creativepro.com/take-control-of-your-hyphenation/, 2019-04-08): minimum three-letter hyphenations, no more than two in a row, and deselect Hyphenate Capitalized Words, Last Word and Across Column.

Optical Margin Alignment: Type ▸ Story, and the point-size field sets the overhang depth. Adobe says match it to the text size; Nigel French reports 12 pt works well for left-aligned text. It is story-wide, so the paragraph-style counterpart is **Ignore Optical Margin** in Paragraph Style Options. That asymmetry is the thing to encode: switch it on per story, and exempt display lines through their styles.

### [03.5]-[SIZE_CATALOGUE_MECHANISM]

Four routes, in order of preference for this job:

1. **One `.indt` per size, generated by script, synchronised by Book.** `app.documents.add()`, `documentPreferences.pageWidth/pageHeight`, `marginPreferences.properties`, then save. A worked community script is at https://community.adobe.com/questions-671/indesign-script-for-batch-creation-891234. The DOM members are documented for UXP at https://developer.adobe.com/indesign/uxp/dom/api/d/document-preset/ and mirrored for ExtendScript at https://www.indesignjs.de/extendscriptAPI/indesign-latest/DocumentPreset.html. UXP `.idjs` scripts run from the Scripts panel in InDesign 2023+ (https://developer.adobe.com/indesign/uxp/scripts/getting-started/); ExtendScript is not removed and Adobe maintains a migration guide (https://developer.adobe.com/indesign/uxp/resources/migration-guides/extendscript/, updated 2026-05-13).
2. **Grid Calculator PE presets.** One preset per size, recomputed rather than scaled, which keeps the baseline grid a whole number at every trim.
3. **Document presets alone.** Fine for trim, margin, column, bleed and slug; carries no grid or style payload.
4. **Alternate Layouts with Liquid Layout.** Right for A4↔Letter or print↔tablet in one file. **Wrong for A4→A0**: Scale keeps relative positions but introduces gaps at different ratios, Re-center drops content off the page when shrinking, and Guide-based guides affect every object they touch (https://creativepro.com/understanding-liquid-layouts-part-one/, .../part-two/, .../part-four/). A poster is not a scaled page — its baseline is 36 pt, not 10 pt.

---

## [04]-[TYPOGRAPHY_AND_BOARD_LANGUAGE_2025_2026]

### [04.1]-[HONEST_FRAMING]

Very little genuinely new was published in the last twelve months. The canonical numbers (Bringhurst 45–75 characters, Butterick 120–145% leading) are undated living references, not 2025-26 work. Smashing Magazine's typography category has published nothing since August 2024. A List Apart has no 2025-26 typography article. Typographica and ilovetypography return 403. Treat the table below as "what a bleeding-edge designer can actually cite", not as a trend list.

| [SUBJECT] | [GUIDANCE] | [SOURCE] | [DATE] |
| :-------- | :--------- | :------- | :----- |
| Fluid type scale | Compressed fluid typography: `--scale-factor: calc(16px / 1rem)` equals 1 at 16px and 0.666 at 24px; a `--compression-strength` of 0.7 damps growth as the user's base size rises. `clamp(1rem, 1rem + (3vw * var(--scale-factor)), 3rem)`. Ratios in use: perfect fifth 1.5, golden 1.618, octave 2. The author concedes it does not fully satisfy WCAG 1.4.4 | https://matthiasott.com/notes/compressed-fluid-typography | 2025-10-12 |
| ↳ generator | https://sizematters.netlify.app/ | https://www.smashingmagazine.com/the-smashing-newsletter/smashing-newsletter-issue-531/ | 2025-10-28 |
| Leading floor | WCAG 2.2 SC 1.4.12: line height ≥ 1.5×, paragraph spacing ≥ 2×, letter spacing ≥ 0.12×, word spacing ≥ 0.16× font size. **Read it correctly** — the criterion requires content to survive a user override at those values, it does not mandate setting them. "The values in the SC are a baseline. Authors are encouraged to allow spacing to surpass the values specified, not see them as a ceiling." | https://www.w3.org/WAI/WCAG22/Understanding/text-spacing.html | updated 2025-10-01 |
| Leading, print | 120–145% of point size | https://practicaltypography.com/line-spacing.html | undated |
| Measure | 45–90 characters including spaces, or two to three alphabets | https://practicaltypography.com/line-length.html | undated |
| ↳ | Bringhurst 45–75 for single-column serif; Material Design 40–60 on screen; as measure widens, leading must widen with it | https://fonts.google.com/knowledge/using_type/understanding_measure_line_length | undated |
| ↳ current consensus | 50–75 on desktop, 66 the cited sweet spot, 30–50 on mobile; `max-width: 65ch` | https://typeyeah.com/learn/ideal-line-length/ (2026-08-21), https://137foundry.com/articles/responsive-typography-scale-that-holds-up (2026-08-07) | 2026 |
| Hierarchy from one family | Bretons magazine 2026 redesign (AD David Yven) keeps Base 9 Sans as masthead, Futura Now for display, and puts **all editorial matter in Bilzig**, building hierarchy inside one family through size, weight and colour rather than adding faces | https://fontsinuse.com/uses/79425/bretons-magazine-2026-redesign | 2026-09-03 |
| Superfamily pairing | Serif + sans + slab on one skeleton (IBM Plex, Source, PT, Noto) is the lowest-risk route to harmony | https://fonts.google.com/knowledge/choosing_type/pairing_typefaces_within_a_family_superfamily | undated |
| Optical size axis | At small sizes widen letterforms, raise x-height, open counters, add weight, cut contrast, loosen spacing; reverse at display sizes | https://fonts.google.com/knowledge/glossary/optical_size_axis | undated |
| ↳ shipping example | GT Standard, released 2025, 336 styles, optical sizes S/M/L across Compressed→Expanded widths | https://www.grillitype.com/typeface/gt-standard | 2025 |
| Numerals | Tabular = fixed advance for column alignment; lining = for scanning data. CSS `font-variant-numeric: lining-nums tabular-nums`; InDesign OpenType menu | https://alistapart.com/article/web-typography-numerals/ · https://practicaltypography.com/alternate-figures.html · https://ilovetypography.com/2025/05/22/a-font-lovers-guide-to-numerals/ | ILT 2025-05-22 |
| Hanging punctuation, web | `hanging-punctuation` is **not Baseline** — Safari-only in practice | https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/Properties/hanging-punctuation | modified 2026-04-20 |
| ↳ cross-browser fix | OpticalMargin (MIT, `@liiift-studio/opticalmargin`) measures real glyph bounds via Canvas `measureText`, subtracts visual bounds from advance width, applies a negative margin. Defaults: hyphens and dashes hang 1.0, quotes and terminal punctuation 0.8, opening brackets and mid-sentence marks 0.6, threshold 0.5 px, maxHangRatio 0.9 | https://opticalmargin.com/ · https://github.com/Liiift-Studio/OpticalMargin | age unverified |
| Hanging punctuation, print | InDesign Optical Margin Alignment; the point-size field need not match the text size; exempt paragraphs with Ignore Optical Margin | https://creativepro.com/typetalk-hung-punctuation-optical-margin-alignment/ | undated |
| TOC without leaders | "Many designers consider a leader to look old-fashioned, so they eliminate it, to make a cleaner, more modern-looking page"; alternatives use two separator ranks (vertical bar plus centred dot) | https://creativepro.com/dot-font-tables-of-contents/ | undated |
| Caption systems | No dated 2025-26 authority found. The one concrete practice is a **dedicated image-caption grid** distinct from the text grid, as in Kelman's A1, A2 and 16:9 systems | — | unverified |
| Trend context | Experimental letterforms rejecting standardised typeface rules, patchwork and collage editorial, the "warning low ink" photocopier aesthetic; handmade and scanned texture grows because AI image generators still struggle to replicate it | https://www.itsnicethat.com/features/forward-thinking-graphic-trends-2026-graphic-design-120126 | 2026-01-12 |

### [04.2]-[ARCHITECTURAL_BOARD_LANGUAGE]

Dezeen, The Architectural Review, Domus, Metropolis and Archinect all return HTTP 403, so nothing is cited from them. What is verifiable:

- **Europan 18 results, 2025-11-17**: 45 winners, 47 runners-up, 61 special mentions out of 804 teams across 47 sites in 12 countries, theme "Re-sourcing", €822,000, average entrant age 30.7 — https://www.europan-europe.eu/en/session/europan-18/results/press-release.
- **Architecture Drawing Prize 2025** (WAF / Make / Sir John Soane's Museum): fifteen winners after **category silos were removed**, "acknowledging the growing influence of digital and AI-assisted approaches"; hand-drawing prize to Jason Wang's graphite *Dockyard X*; Erhang Wang's winner composites plan, section, perspective and axonometric in one sheet — https://www.archdaily.com/1035513/meet-the-category-winners-of-the-2025-architecture-drawing-prize. The 2026 edition requires a 150-word description and a statement of "means and method of production, including… how AI has been deployed" — https://www.worldarchitecturefestival.com/WAF2026/en/page/the-architecture-drawing-prize.
- **Exploded axonometry as analytic method**: "axonometric sections and exploded views act as X-rays of architectural works… an activity of knowledge, study, and analysis"; it "makes it possible to visualize the operation of each space, component, process, or construction logic" — https://www.archdaily.com/1037623/decomposition-as-expression-disassembled-axonometry-as-design-tool, 2026-01-20.
- **Representation is decentring from drawing**: Pippo Ciorra, "Technology, political consciousness, and media strategies borrowed from the art world are offering radical new approaches" — practice extending into performance, film, textiles, thermography — https://www.koozarch.com/interviews/stop-drawing-pippo-ciorra-on-the-evolving-role-of-architectural-representation, 2025-06-16.
- **OMA/AMO**: *Diagrams* opened at Fondazione Prada Venice 2025-05-08 (running to 2025-11-24) and was restaged at Prada Rong Zhai Shanghai on 2026-03-17 — https://www.oma.com/news/diagrams-an-exhibition-by-amo-oma-opens-at-fondazione-prada-in-venice. This is the single most relevant current reference for a diagram-driven board language.
- **MVRDV**, third-party analysis: palettes "vibrant, leaning toward saturated… rather than replicate reality"; figures chosen for "clothing, age, activity, ethnicity, posture, and color"; "recognizable almost immediately, even without captions or logos" — https://parametric-architecture.com/ai-mvrdv-architectural-representation/, 2026-06-26.
- **Board layout advice** (ArchDaily/Lumion, sponsored): read left-to-right and top-to-bottom, use white space deliberately, leverage grids for consistency in scale and alignment, stick to a unified style for fonts, text and colours; strip context from massing and exploded-axo diagrams so the geometry reads — https://www.archdaily.com/catalog/en/products/37234/how-to-create-architectural-presentation-boards-lumion, https://lumion.com/tips-guides/architectural-presentation-boards.

**Verdict.** The only firm 2025-26 conventions are the briefs' own: three A1 portrait panels for Europan or one A2 landscape ≤ 10 MB elsewhere; an anonymity code block at top-left and a panel number at top-right; a graphic scale on every drawing; 150 dpi as the actual stated resolution. Flat-colour palettes, collage, key-plan systems and drawing-title systems are widely practised but **unverified as current winning conventions** from any fetchable 2025-26 source. Typographically, the only verified instruction in the whole competition record is negative: "no set minimum font size; however, it is important to keep the font legible". The ERDC 3/32 in (2.4 mm) plotted minimum in [02.6] is the defensible number to adopt instead.

---

## [05]-[SOURCES_TO_MINE]

| [SOURCE] | [WHAT TO EXTRACT] | [ACCESS] | [URL] |
| :------- | :---------------- | :------- | :---- |
| CreativePro Network | The highest-value login. **InDesign Magazine ended at issue #150 (Oct 2021)** and became **CreativePro Magazine #1 (2021-11-01)**, now at **issue #58 (Aug 2026)** — still published, renamed. Membership includes every back issue plus all 150 InDesign Magazine issues, members-only tutorials, scripts, templates and forum | **$78/yr ($6.50/mo)**; student, faculty and over-69 discounts; free starter kit and sample issue | https://creativepro.com/become-a-member/ · https://creativepro.com/issues/ · https://creativepro.com/membership-faq/ |
| InDesignSecrets | **Merged into CreativePro on 2020-10-14.** 2,500+ free articles, podcasts and templates moved into the CreativePro library; no separate archive to mine | free portion public | https://creativepro.com/indesignsecrets-and-creativepro-merge/ |
| Peter Kahrel | **`kahrel.plus.com` is gone.** The repository now lives at CreativePro. Take the full free script index, and *GREP in InDesign*, 3rd edition, 129 pp PDF | scripts free; book $14.95, free with membership | https://creativepro.com/files/kahrel/ · https://creativepro.com/product/grep-in-indesign/ |
| id-extras.com (Ariel Walden) | Live and current. 30+ commercial scripts. Standouts for RTL: **Dropword III** (whole-word dropcaps, "essential for traditional Hebrew typography"), AutoFit Columns, document translation, PDF forms. Free tier is blog plus one free script | mostly paid | https://www.id-extras.com/ |
| indiscripts.com (Marc Autret) | **Still actively updated** — latest post 2026-07-04 ("On ExtendScript's Lexical Scope"). Take **IndexMatic³** (FAQ updated 2026-05-11), **IdExtenso 2.6** (2026-05-02), "InDesign Scripting Forum Roundup #14" (2026-04-21), **"FlexObject (InDesign 2026) on the Scripting Side"** (2025-11-07), the MathML series. Claquos and StyleSniffer still listed | free articles, paid tools | https://indiscripts.com/ |
| Adobe help, complete, as one PDF | The entire InDesign help tree in a single mineable document — the fastest way to grep the feature surface | free | https://helpx.adobe.com/pdf/indesign_reference.pdf |
| Adobe What's New and release notes | Version-by-version deltas, LTS line | free | https://helpx.adobe.com/indesign/desktop/whats-new/whats-new.html · .../release-notes.html |
| Adobe Beta and Prerelease | Two channels. Public Beta: Creative Cloud desktop ▸ Apps ▸ Beta. Prerelease: sign in, Available Programs, InDesign Prerelease, Join, which adds a Prerelease tab. Also InDesign Developer Prereleases and the Family SDK Download Program | free, Adobe ID | https://www.adobeprerelease.com/ · https://helpx.adobe.com/x-productkb/global/creative-cloud-beta.html |
| UXP for InDesign | `.idjs` scripts run from the Scripts panel, InDesign 2023 (18.0)+, with UXP Developer Tool 1.7. ExtendScript is not removed | free | https://developer.adobe.com/indesign/uxp/ · .../scripts/getting-started/ · .../resources/migration-guides/extendscript/ |
| ExtendScript DOM reference | Adobe ships no scripting reference PDF. Use the object-model viewer, or the community-maintained API reference through 21.0 | free | https://lohriialo.github.io/Adobe-InDesign-Scripting-API-Reference-21.0/ · https://www.indesignjs.de/extendscriptAPI/indesign-latest/ |
| Adobe UserVoice, SDK/Scripting | Where Persian ZWNJ defects are logged and voteable — the only public record of what is broken for Farsi | free | https://indesign.uservoice.com/forums/913162-adobe-indesign-sdk-scripting-bugs-and-features |
| Adobe Community forums | Bug repros with screenshots. The ME and RTL threads are the only place the HarfBuzz kerning and ZWNJ regressions are documented | free | https://community.adobe.com/questions-671 |
| Nigel French | *InDesign Type*, 4th edition; LinkedIn Learning *InDesign: Typography* (updated 2024); the CreativePro grid and nested-style articles cited throughout [03] | book paid; LiL subscription | https://www.nigelfrench.com/Graphic-DesignWritingTrainingCourses/IDT |
| Laurent Tournier | Adobe France session "Master InDesign: Scripts, GREP and AI", streamed 2025-01-24 | free | https://www.youtube.com/watch?v=5XeZsArBq5Y |
| Typefi | InDesign Server automation at scale; webinar recordings; hosts a free Kahrel scripts page | free | https://www.typefi.com/automated-publishing-solutions/adobe-indesign-automation/ |
| Designers Bookshop | Grid Calculator PE release notes track InDesign majors within weeks — a useful early-warning feed for grid-affecting changes | product paid | https://designersbookshop.com/grid-calculator-basic-edition.html |

---

## [06]-[PERSO_ARABIC_IN_INDESIGN]

### [06.1]-[WORLD_READY_COMPOSER]

Adobe rewrote this documentation in 2026 and it now reads as two pages with different answers:

| [PAGE] | [UPDATED] | [SAYS] |
| :----- | :-------- | :----- |
| Adobe World-Ready Composer overview | 2026-08-18 | WRC is "a text composition engine in InDesign" supporting Middle Eastern, Indic and South-East Asian scripts, with bidi, contextual shaping and kashida justification. Framed as a general InDesign feature |
| Set up World-Ready composers for Arabic and Hebrew | 2026-08-07 | "To access **full** right-to-left and typographic support for these languages, **install Hebrew or Arabic versions of InDesign through the Creative Cloud desktop app**." Then Window ▸ Type & Tables ▸ Paragraph ▸ panel menu ▸ Adobe World-Ready Paragraph Composer |

URLs: https://helpx.adobe.com/indesign/desktop/language-and-proofing/language-settings/adobe-world-ready-composer-overview.html and https://helpx.adobe.com/ca/indesign/desktop/language-and-proofing/arabic-and-hebrew/set-up-arabic-and-hebrew.html.

**The famous enable-WRC script is now largely obsolete, and its usual sources are dead or historical.** The canonical writeup is Thomas Phinney, "World-Ready Composer in Adobe CS4" (https://www.thomasphinney.com/2009/01/adobe-world-ready-composer/), which records that Adobe shipped WRC in CS4 with no UI but full scripting access. CreativePro's coverage (https://creativepro.com/indesign-cs4s-hidden-world-ready-composer/) is banner-flagged as no longer current. Kahrel's `kahrel.plus.com` host is gone. The working modern property, confirmed on a live Adobe Community thread of 2024-03-06 (https://community.adobe.com/questions-671/create-right-to-left-paragraph-style-based-on-existing-left-to-right-style-888518):

```js
myParagraphStyle.composer = "Adobe World-Ready Paragraph Composer";
myParagraphStyle.justification = Justification.RIGHT_ALIGN;
// application default:
app.textDefaults.composer = "Adobe World-Ready Paragraph Composer";
```

**Does the composer appear in the standard UI?** Yes. A non-ME Windows user filed https://community.adobe.com/questions-671/indesign-19-0-2024-windows-defaults-to-adobe-world-ready-paragraph-composer-886028 (2023-10-26) because WRPC had become the **default** for paragraph styles in 19.0, breaking Optical Margin Alignment and UXP `bulletsAndNumberingResultText`. The entry is present in the Paragraph panel menu of standard builds.

**Do the ME features appear without the ME edition?** No. Paragraph and character direction, story direction, table direction, digit type, kashida, Insert Special ME Character and the Right-to-Left preferences pane are ME/MENA-edition UI. Adobe's consolidated page carries the note explicitly (https://helpx.adobe.com/indesign/using/arabic-hebrew.html, updated 2025-07-18). **The ME build is free with an existing subscription**: switch the Creative Cloud desktop app language to English Arabic or English Hebrew and install (https://helpx.adobe.com/indesign/kb/access-install-hebrew-arabic-indesign-illustrator-CC.html, 2025-04-21). **Verdict: install the ME build. Do not script around it.**

**Shaping engine.** Since 19.0 (October 2023) **HarfBuzz is the default WRC shaping engine**, changeable only by script — `app.textPreferences.shapeIndicAndLatinWithHarbuzz = false` (https://helpx.adobe.com/th_th/indesign/kb/shaping-engine-world-ready-composer.html).

**Third-party plug-ins: stale.** In-Tools World Tools advertises CS4–CC only (http://in-tools.com/products/plugins/world-tools/). **DecoType Tasmeem is dead**: WinSoft states "As of April 2021, we have decided to discontinue the technical development and distribution of Tasmeem… The last release of Tasmeem is for InDesign CC 2021" (https://winsoft-international.com/tasmeem/). DecoType's own site still documents ACE and the Tasmeem manual (https://decotype.com/index.php/9-products) but ships no product for v20 or v21.

### [06.2]-[CONTROL_MAP]

| [CONTROL] | [UI PATH] | [DOC] |
| :-------- | :-------- | :---- |
| Paragraph direction RTL/LTR | Paragraph panel direction buttons | `.../arabic-and-hebrew/control-text-direction.html` |
| Character direction | Character panel menu ▸ Character Direction | same |
| Story direction | Window ▸ Type & Tables ▸ Story | same |
| Table direction | Table ▸ Insert Table ▸ Direction; existing tables via Table panel (`Shift+F9`) LTR/RTL icons | — |
| Digit type: Arabic, Hindi, Farsi | Character panel ▸ Digits; Preferences ▸ Advanced Type ▸ *Use Native Digits when typing in Arabic Scripts* | `.../choose-digit-types.html` |
| Digit transliteration | Edit ▸ Find/Change ▸ **Transliterate** tab, Arabic ↔ Hindi ↔ Farsi | — |
| Kashida | Paragraph panel ▸ Insert Kashida: None / Short / Medium / Long / Stylistic. **Justified paragraphs only.** Character level via Character panel menu ▸ Kashidas | `.../justify-arabic-text.html` |
| Justification Alternates | Paragraph panel menu ▸ Justification; Character panel menu ▸ Justification Alternate. Carried by **Adobe Arabic, Myriad Arabic, Adobe Naskh** only | — |
| Ligatures | Character/Control panel menu ▸ Ligatures; OpenType ▸ Discretionary Ligatures | `.../apply-ligatures-arabic-hebrew.html` |
| Diacritic positioning | Character panel ▸ Adjust Horizontal / Vertical Diacritic Position | `.../adjust-diacritical-marks.html` |
| Diacritic colouring | Find/Change ▸ Query ▸ **Change Arabic Diacritic Color** | same |
| Neutral character direction | Preferences ▸ Right to Left ▸ *Force Neutral Character Direction According to the Keyboard Input* | — |
| Cursor movement | Preferences ▸ Right to Left ▸ Visual / Logical | — |
| Legacy encodings (AXT) | Supported but deprecated; Missing Glyph Protection on by default in Advanced Type | — |
| Word paste | Auto-sets alignment and direction | `.../copy-arabic-hebrew-text.html` |
| Page numbering styles | Layout ▸ Numbering & Section Options ▸ Arabic Abjad, Alef-Ba-Tah, Hebrew Biblical Standard, Hebrew Non-Standard Decimal | — |
| Story Editor direction | Preferences ▸ Story Editor Display ▸ *Indicate Writing Direction* | — |

All under the `helpx.adobe.com/indesign/desktop/language-and-proofing/arabic-and-hebrew/` prefix.

### [06.3]-[GOTCHAS]

**ZWNJ (U+200C) is the main hazard.** In InDesign it is `^j` in the Find/Change text tab and `~j` in GREP.

- **Stripped from text variables and on EPUB export** — https://community.adobe.com/questions-671/zwnj-character-unicode-will-be-omitted-in-text-variable-and-export-to-epub-891373 (2024-08-17, reported on the ME version). Same loss at https://graphicdesign.stackexchange.com/questions/143424/export-indesign-to-epub-removed-j-character, with the workaround of Find/Change `^j` to a sentinel, export, unzip, substitute the HTML entity, rezip.
- **Hunspell ignores it**: words containing ZWNJ are flagged misspelled even when present in the `.dic` — https://community.adobe.com/questions-671/developing-hunspell-for-persian-language-878099, plus the open request https://indesign.uservoice.com/forums/913162-adobe-indesign-sdk-scripting-bugs-and-features/suggestions/49041956-request-for-full-support-of-persian-standard-half (2024-11-03). Index entries ignore it too.
- **HarfBuzz regression**: the new engine ignores ZWNJ and applies kerning anyway where the old engine suppressed it, with a Persian sample (می‌گرداندند) at https://community.adobe.com/questions-671/different-implementing-kerning-from-harfbuzz-to-old-text-shaping-engine-897734 (2025-10-04). **This silently reflows legacy IDML. Choose the shaping engine once per project and never migrate mid-job.**

**Persian ye and kaf.** Persian uses U+06CC (farsi yeh) and U+06A9 (keheh); Arabic uses U+064A and U+0643. A font missing `YehFarsi` initial and medial forms renders broken — a 2025 case with an InDesign repro is at https://forum.glyphsapp.com/t/issue-with-missing-yehfarsi-ar-files-during-font-export/34012. **Rule for the template: normalise at import with a saved GREP query (ي→ی, ك→ک), and test any candidate face by setting those two codepoints in initial and medial position before adopting it.**

**Digits in RTL runs.** Set digit type on the selection, not globally; bulk-convert through the Transliterate tab. Mixed Latin and Persian lines need Character Direction on the numeric run plus *Force Neutral Character Direction According to the Keyboard Input* to stop punctuation flipping.

**Justification.** Adobe states the interaction directly: with kashida insertion enabled, kashidas are inserted and non-Arabic text is **not** hyphenated; disable kashida and only non-Arabic text is considered for hyphenation. For Persian, kashida is stylistically contentious, so spacing-based justification plus Justification Alternates is the safer default — but Justification Alternates exist only in Adobe Arabic, Myriad Arabic and Adobe Naskh.

**Adobe Fonts does carry Persian-capable faces.** Filter at https://fonts.adobe.com/fonts?browse_mode=default&languages=fa. Verified with Persian in the description: **Parastoo** ("a refined Persian typeface… designed primarily for Farsi", Saber Rastikerdar, https://fonts.adobe.com/fonts/parastoo), **Mirza** ("Arabic and Persian aesthetics", https://fonts.adobe.com/fonts/mirza), Arabic Typesetting, Cairo, PF Aljamal (Arabic/Latin/Persian/Urdu/Kurdish), Brando Arabic, Kohinoor Arabic, Noto Naskh Arabic.

### [06.4]-[FOUNDRIES]

| [FOUNDRY] | [PERSIAN-CAPABLE FAMILIES] | [LICENCE] | [PERSIAN STATED?] | [URL] |
| :-------- | :------------------------- | :-------- | :---------------- | :---- |
| 29LT | Bukra, Zarid / Zarid Sans / Zarid Text, Idris (Naskh system), Azahar, Ada, Makina, Okaso + Oskura | Perpetual, one-time, six licence types (Desktop, Web, App & ePub, Corporate, Broadcasting, Server) | **Yes** — product pages list "Farsi [Persian]" and Urdu; Bukra was built "to cover Arabic, Farsi and Urdu scripts" | https://29lt.com/ · https://www.29lt.com/info/ |
| TPTQ Arabic | **Shekanj** (Nastaʿlīq-derived, Amir Mahdi Moslehi, engineered by Amin Abedi, 2021–2025, €96), **Roshan** (2025-03), Mizan, Symbio, Greta Arabic | Per-user perpetual; Print & Web or Web-only; no modification | Implied by Nastaʿlīq lineage; the language page says Arabic + Latin, 85 languages — **verify per family** | https://tptq-arabic.com/ · https://tptq-arabic.com/fonts/shekanj/about |
| Typotheque (sister) | Arabic collection page is titled "Professional Arabic and **Farsi** fonts" | Retail perpetual | **Yes** | https://www.typotheque.com/fonts/arabic |
| Rosetta Type | **Mehraban Arabic** (Amir Mahdi Moslehi, 2023), **Adapter Arabic** (Borna Izadpanah, 2020), Nassim Arabic, Skolar Sans Arabic, Eskorte Arabic, Aisha Arabic | Retail tiers plus resellers | **Yes** — Skolar Sans Arabic "for Arabic, Persian, Urdu…"; Nassim specimen sets Persian | https://rosettatype.com/ |
| Indian Type Foundry / Kohinoor | **Kohinoor Arabic** (Bahman Eslami, 2016, five weights) | Retail; also Adobe Fonts and Fontstand | **Yes, explicitly** — "Supported Languages: Arabic, Persian (Farsi), Urdū" | https://www.indiantypefoundry.com/fonts/kohinoor-arabic |
| Boutros | Tanseek Modern (five weights, Arabic + Latin), custom commissions | Retail plus custom | Not stated | https://www.boutrosfonts.com/ |
| Naghi Naghachian | ~10 Arabic families (Afshid etc.) | MyFonts / Linotype, from ~$88 | Not stated | https://www.naghachian.com/font_design.html — live but static, © 2012. `naghi.com` is dead |
| Arabetics (Saad D. Abulhab) | Arabetics Harfi, Mutamathil family | MyFonts, $59/style, $149 family | **Yes** — site advertises Persian, Urdu, Kurdish | http://arabetics.com/ — dated HTML, timeout-prone |
| Mohamad Dakak | Jali Arabic | MyFonts / Adobe Fonts | — | https://fonts.adobe.com/designers/mohamad-dakak |

**Could not confirm as existing foundries: "Sahar Type", "Dar al-Kutub", "Blanka / Ana Type".** Do not cite them. "Dar al-Kutub" is most likely a naskh *style* named for the Egyptian national library, not a vendor.

| [FREE FONT] | [LICENCE] | [STATUS] | [URL] |
| :---------- | :-------- | :------- | :---- |
| **Vazirmatn** | SIL OFL 1.1 | **Active**, v33.003, 3.1k stars, npm/AUR/Fedora, variable | https://github.com/rastikerdar/vazirmatn |
| Shabnam | SIL OFL 1.1 | **Archived 2022-08-12**; last v5.0.1 (2019-11) | https://github.com/rastikerdar/shabnam-font |
| Sahel | OFL (Latin from Open Sans, Apache 2.0) | Last release v3.4.0, 2020-02; ships a full-Persian-digits variant | https://github.com/rastikerdar/sahel-font |
| **Yekan Bakh** | **Proprietary, not free.** FontIran states plainly that it is not free and threatens action over redistribution; ~€29–49 at NoonFont | Active; eight weights × seven widths, variable; "Supported languages: Persian, English, Urdu, Arabic, Kurdish" | https://fontiran.com/fonts/yekan-bakh · https://noonfont.com/fonts/yekan-bakh |
| **IRANSans** | **Proprietary.** The font metadata itself says a licence must be obtained from fontiran.com | Widely pirated — do not ship without a licence | https://github.com/akiarostami/iransans |
| Noto Naskh Arabic / Noto Nastaliq Urdu | SIL OFL — usable in all products, print or digital, commercial or not | Active; Nastaliq Urdu "also used for… Iran"; 1,138 glyphs | https://fonts.google.com/noto/specimen/Noto+Naskh+Arabic · https://fonts.google.com/noto/specimen/Noto+Nastaliq+Urdu |
| Google Fonts Arabic subset | OFL | live filter | https://fonts.google.com/?subset=arabic |

**Verdict for the template.** The Persian and Arabic house families are typography's round 3 pick among the families of `illustrator-acrobat-cc.md` [07.6] row 01; this table is the foundry record behind a coverage finding and names no face for the template; `families.json` reads `Noto Naskh Arabic` for both keys until that pick. IRANSans and Yekan Bakh carry FontIran licences and enter no row.

---

# [PART_B]-[ACROBAT_PRO_26_ON_MACOS]

## [07]-[PREFERENCES_AND_UI]

### [07.1]-[TRACK_AND_VERSION]

The product is now called **Adobe Acrobat** / Acrobat Pro; the *DC* suffix is gone from the release-note titles but survives in the enterprise URL slug (`ReleaseNotesDC`) and in the plist dictionary hive name. Versioning is `26.00X.NNNNN`. The authoritative per-build list is the DevNet enterprise index, **not** the helpx release-note page (which 403s to automation): https://www.adobe.com/devnet-docs/acrobatetk/tools/ReleaseNotesDC/index.html. The desktop help pages that do render are https://helpx.adobe.com/acrobat/desktop/whats-new/release-notes.html (updated 2026-06-09) and https://helpx.adobe.com/acrobat/desktop/whats-new/whats-new-acrobat-desktop.html. Note from the June 2026 entry: **macOS 12.x support is discontinued**, along with Windows 7, 8 and 8.1.

### [07.2]-[PREFERENCE_PANES]

Main reference: https://helpx.adobe.com/acrobat/using/viewing-pdfs-viewing-preferences.html, updated 2026-02-26.

| [PANE] | [WHAT TO SET] |
| :----- | :------------ |
| Documents | Restore Last View Settings When Reopening; Open Cross-document Links In Same Window; **Always use filename as document title** (off by default, turn on); Recently Used list length; Save As Optimizes For Fast Web View; Always reduce size for files over 10 MB; PDF/A View Mode (Never / Only For PDF/A); **Remove Hidden Information When Closing / When Sending By Email** |
| General | **Use Single Key Accelerators** (off by default, turn on); Make Hand Tool Select Text & Images; Make Hand Tool Use Mouse-wheel Zooming; **Show Online Storage When Opening Files / When Saving Files** (turn off to kill the cloud picker); Open PDFs from last session on launch; Open Documents As New Tabs; Touch Mode; Show Quick actions on text selection; Use Only Certified Plug-Ins; Reset All Warnings |
| Page Display | Default page layout, zoom, smooth text, Rendering group. **"Use 2D Graphics Acceleration" should be treated as Windows-only** — the companion General option is literally labelled "Check 2D Graphics Accelerator (Windows only)" (https://helpx.adobe.com/acrobat/kb/2d-graphics-acceleration-gpu-support.html) |
| Theme | Not a preference — **View ▸ Display Theme ▸ System Theme / Light Gray / Dark Gray** |
| Forms | See [07.3] |
| JavaScript | **Enable Acrobat JavaScript**; Enable Menu Items JavaScript Execution Privileges; Enable Global Object Security Policy — https://helpx.adobe.com/acrobat/desktop/protect-documents/mitigate-security-risks/restrict-javascript-api.html |
| Security (Enhanced) | Enhanced security toggle; **Privileged Locations** (Add File / Add Folder Path / Add Host); sandbox protection — https://helpx.adobe.com/acrobat/desktop/protect-documents/enhanced-security/trust-locations.html |
| Protected View | **Windows only** — Adobe's page is titled "Protected View feature for PDFs (Windows only)" (https://helpx.adobe.com/acrobat/current/protected-view-feature-pdfs-windows.html) |
| Full Screen | Escape Key Exits; Show Navigation Bar; Disable All Page Transitions; Which Monitor To Use; Advance Every _ Seconds |
| Commenting, Identity, Units & Guides, Measuring (2D), Accessibility, Reading, Search, Trust Manager, Signatures, Convert To/From PDF, Email Accounts, Language | Panes exist; the plist dictionaries are `Comments`, `Access`, `Accessibility`, `UnitsAndGuides`, `TrustManager`, `Security`, `AVConversionToPDF`, `AVConversionFromPDF`, `SendMail`/`WebMail`, `Intl`/`Language`, `Spelling`, `Recognition`, `Scan`. A current per-pane helpx article for each is **unverified** |

### [07.3]-[FORMS_PREFERENCES_VERBATIM]

From https://helpx.adobe.com/acrobat/desktop/work-with-pdf-forms/explore-basics/preferences.html, updated 2025-09-23:

- **General**: Automatically calculate field values · Automatically adjust tab order when modifying fields · Show focus rectangle · Show text field overflow indicator · Show field preview when creating or editing form fields · **Automatically detect Form fields** · Auto-enable text editing in Prepare Form
- **Highlight Color**: **Show border hover color for fields** · Fields highlight color · Required fields highlight color
- **Auto-Complete**: Off / Basic / Advanced · Remember numerical data · Edit Entry List

Turning auto-detection off is the General checkbox. **"Always hide document message bar" is not on this page** — it either moved or was dropped; `unverified`.

### [07.4]-[MACOS_STORAGE_AND_LOCKDOWN]

| [ITEM] | [VALUE] | [SOURCE] |
| :----- | :------ | :------- |
| User plist | `~/Library/Preferences/com.adobe.Acrobat.Pro.plist`, addressed as `…plist/Root/(track)/<Dictionary>` | https://www.adobe.com/devnet-docs/acrobatetk/tools/Preferences_by_version/Macintosh/6_6_2023.html |
| Machine plist | `/Library/Preferences/com.adobe.Acrobat.Pro.plist` — placed at deploy time, **replaces** any file already there; permissions 755 | https://www.adobe.com/devnet-docs/acrobatetk/tools/AdminGuide_Mac/predeployment_configuration.html · .../predeployment_configuration_advanced.html |
| Version hives | All tracks live in one file under a version hive: `DC`, or `2015|2017|2020` | predeployment_configuration_advanced.html |
| Path identity | "Locking support on Mac is identical to the support on Windows… the path to a specific lockable preference is identical to Windows" | same |
| Hungarian notation | Unlocked preference names do not use it; **all locked settings do** (`b` boolean, `i` integer), matching the Windows spelling | https://www.adobe.com/devnet-docs/acrobatetk/tools/PrefRef/Macintosh/FeatureLockDown.html |
| Preference Reference, macOS | https://www.adobe.com/devnet-docs/acrobatetk/tools/PrefRef/Macintosh/home.html — Windows View, Macintosh View, **Lockable**, Glossary, Index, Preferences by Version |
| Admin Guide, macOS | https://www.adobe.com/devnet-docs/acrobatetk/tools/AdminGuide_Mac/index.html — last update 2025-03-17 |

Documented macOS dictionaries under `Root/(track)`: `3D, Access, Accessibility, AcroApps, AdobeViewer, Annots, Attachments, AutoSaveDocs, AVAlert, AVConversionFromPDF, AVConversionToPDF, AVDisplay, AVEntitlement, AVGeneral, AVPrivate, AVTracker, Collab, Comments, Distiller, DocumentStatus, FavoriteFilesSync, FeatureLockDown, FeatureState, FormPrefs, FTEDialog, HomeWelcome, Intl, IPM, JSPrefs, Language, LegacyReview, OptionalContent, Originals, Privileged, Recognition, RememberedViews, Scan, Security, Selection, SendMail, Spelling, TouchUp, TrustManager, UnitsAndGuides, Updater-Mac, Updater-Win, UsageMeasurement, WebMail, Workflow, Workflows` (https://www.adobe.com/devnet-docs/acrobatetk/tools/PrefRef/Macintosh/contents.html).

Keys worth knowing:

| [KEY] | [DICT] | [MEANING] |
| :---- | :----- | :-------- |
| `EnableAV2Enterprise` | FeatureLockDown | Enables the **Modern Viewer** — the new Acrobat UI, admin-controlled |
| `bEnableGentech` | FeatureLockDown | **0 disables all generative AI features.** See [07.6] |
| `bEnableJS`, `bEnableMenuItems`, `bEnableGlobalSecurity` | JSPrefs | JavaScript lockdown — https://www.adobe.com/devnet-docs/acrobatetk/tools/AppSec/javascript.html |
| `cJavaScript`, `cJavaScriptURL`, `cScriptInjection`, `cAlwaysTrustedForJavaScript`, `cUnsafeJavaScript`, `tBlackList` | TrustManager / cJavaScriptPerms | JavaScript trust and API blacklist |
| `bAdobeSendPluginToggle`, `bDisableSharePointFeatures`, `bEnableFillSign`, `bEnableSignPane`, `bEnableCertificate`, `bDisableADCFileStore`, `bEnableAcrobatHS`, `bCreatePDFOnline` | Workflows | Service and UI lockdown — https://www.adobe.com/devnet-docs/acrobatetk/tools/PrefRef/Macintosh/Workflows.html |
| `bCloudATFeatureEnable`, `EnableCloudBasedAT` | FeatureLockDown / user | New auto-tagging vs old; cloud auto-tag |
| `bToggleAdobeDocumentServices`, `bToggleWebConnectors` | machine | Cloud processing master switch; Box/Dropbox/Drive connector framework |

**Corrections to common folklore.** `bDisableJavaScript` is not a documented name; the documented switch is **`bEnableJS`** under `JSPrefs`. `bEnableFlash` is legacy with no current page; `unverified`. And the macOS "Preferences by version" listing appears to stop at **June 2023**, so treat the whole Preference Reference as **stale relative to the 26.x build** — the dictionary names are stable, the per-version deltas are not published.

### [07.5]-[THE_NEW_UI]

| [QUESTION] | [ANSWER] | [SOURCE] |
| :--------- | :------- | :------- |
| Can you still revert to classic in 2026? | **Yes.** macOS: **View ▸ Enable new Acrobat** to switch in, **View ▸ Disable new Acrobat** to revert; a restart dialog follows | https://helpx.adobe.com/acrobat/desktop/get-started/preferences-and-settings/switch-new-acrobat.html, updated 2025-09-23 |
| Customise quick tools | Ellipsis (⋯) on the Quick action toolbar ▸ **Customize toolbar** ▸ pick a category, **+** to add, arrows to reorder, trash to remove, Save | https://helpx.adobe.com/sg/acrobat/desktop/get-set-up/preferences-and-settings/customize-toolbar.html |
| Enterprise control | `EnableAV2Enterprise` under FeatureLockDown | Preferences_by_version 6_6_2023 |

No Adobe page announces removal of the classic toggle. **Verdict: it survives as of the September 2025 doc revision.**

### [07.6]-[DISABLING_AI_ASSISTANT]

| [LAYER] | [CONTROL] | [SOURCE] |
| :------ | :-------- | :------- |
| Per user, desktop | Close all documents ▸ **View ▸ Preferences ▸ Generative AI** (macOS) ▸ deselect **Enable generative AI features** ▸ OK | https://helpx.adobe.com/acrobat/desktop/use-acrobat-ai/set-up-acrobat-generative-ai/turn-off-ai.html, updated 2026-04-28 |
| Machine, macOS plist | `bEnableGentech` = `false` in the `FeatureLockDown` dictionary of `/Library/Preferences/com.adobe.Acrobat.Pro.plist` under the `DC` hive (and `com.adobe.Reader.plist`) | Key documented on Windows at `HKLM\SOFTWARE\Policies\Adobe\Adobe Acrobat\DC\FeatureLockDown` (ADMX at https://gpedit.tplant.com.au/en-us/policy/Adobe.Policies.Adobe_x64/bEnableGentechDC_x64/, Adobe community 2026-07-27 at https://community.adobe.com/questions-12/disabling-generative-ai-assistant-in-adobe-acrobat-reader-dc-via-the-windows-registry-1634019). The macOS translation rests on Adobe's own statement that "the path to a specific lockable preference is identical to Windows" and that locked settings use Hungarian notation — **not on an Adobe page that spells out the macOS form** |
| Browser extension | `DisableGenAI` string policy via managed storage; macOS through MCX `dscl -mcximport` | https://helpx.adobe.com/acrobat/using/turn-off-generative-ai-gpo-enterprise.html, 2025-10-01 — **this page covers the browser extension only** |
| Organisation | Admin Console ▸ **Adobe Acrobat AI** service ▸ Off. Adobe's table shows "Off (Only blocks all AI features in Acrobat)" as a valid product-profile state | https://helpx.adobe.com/business/enterprise/manage-services/configure-services/enable-disable-services.html, 2026-04-07 |

Note: with the machine key at 0 the **Generative AI preference category disappears entirely** and users cannot re-enable it; a user-hive value only sets the default and leaves the category visible.

### [07.7]-[HOME_SUPPRESSION]

**Unverified.** No current Adobe page documents a "Show Home when no document open" switch. The closest verified General options are *Open PDFs from last session on Acrobat launch* and *Show Starred Files In Recent Tab*. Community reports point at a "switch to classic home" checkbox in 25.001.x, unconfirmed.

### [07.8]-[WHAT_IS_PORTABLE]

| [ITEM] | [EXT] | [macOS LOCATION] | [UI PATH] | [NOTE] |
| :----- | :---- | :--------------- | :-------- | :----- |
| Action Wizard action | `.sequ` | `~/Library/Application Support/Adobe/Acrobat/<version>/Sequences` | Action Wizard panel ▸ Export / Import | `<version>` is `DC` on subscription; the 26.x folder name is **unverified** |
| Preflight profile | `.kfp` | Preflight library | Print Production ▸ Preflight ▸ Options ▸ Import/Export Preflight Profile | https://helpx.adobe.com/acrobat/using/preflight-libraries-acrobat-pro.html |
| Preflight droplet | app bundle | anywhere | Preflight ▸ Options ▸ Create Preflight Droplet | same |
| **Adobe PDF settings** | `.joboptions` | **`~/Library/Application Support/Adobe/Adobe PDF/Settings`** (user) or `/Library/Application Support/Adobe/Adobe PDF/Settings` (all users) | Distiller ▸ Settings ▸ Add Adobe PDF Settings | Adobe states the macOS path directly: https://helpx.adobe.com/acrobat/desktop/create-documents/explore-advanced-conversion-settings/share-pdf-settings.html, 2025-09-23. The SDK adds that the preferences file is `com.adobe.AdobePDFSettings.plist` with an `AdobePDFSettingsPath` key: https://opensource.adobe.com/dc-acrobat-sdk-docs/library/pdfcreation/index.html. **This folder is shared with InDesign and Illustrator** |
| Custom stamps | PDF | `~/Library/Application Support/Adobe/Acrobat/<version>/Stamps` | Comment ▸ Stamps ▸ Custom Stamps ▸ Create | `unverified` for 26.x |
| Security policies | policy export | — | Export security settings | https://helpx.adobe.com/acrobat/desktop/protect-documents/manage-security-policies/ |
| Optimizer preset | preset | PDF Optimizer dialog | Save preset | extension `unverified` |
| **Custom tool sets** | `.aaui` (classic) | — | Classic: Tools ▸ Create Custom Tool; import by double-clicking | **No current Adobe doc covers custom tool sets in the new UI.** Only the classic-era tutorial survives (https://acrobatusers.com/tutorials/how-to-import-a-custom-tool-set/). Treat as removed; `unverified` |
| **Preferences** | — | — | — | **No official export/import.** Copy `com.adobe.Acrobat.Pro.plist`, or deploy the machine plist per the Admin Guide |

For comparison, InDesign **does** have an official route: **File ▸ User Settings ▸ Export User Settings** covers workspaces, menu sets, keyboard shortcuts and glyph sets (https://helpx.adobe.com/indesign/desktop/get-started/settings-and-preferences/export-and-import-user-settings.html). Acrobat has no equivalent.

---

## [08]-[FORM_FIELD_AND_PRODUCTION_POWER]

| [FEATURE] | [DETAIL] | [SOURCE] |
| :-------- | :------- | :------- |
| **Use Current Properties As New Defaults** | Right-click a configured field. Sets defaults **for new fields of that type only** — not existing fields, not per document | https://helpx.adobe.com/acrobat/current/pdf-form-field-properties.html. Documented on the `current` page; **unverified on a 2026-dated `/desktop/` page** |
| **Font size 0 = Auto** | Appearance tab ▸ Font Size ▸ **Auto**. A field is Auto *or* a fixed size, never both. This is the single setting that makes every field render consistently regardless of height | https://helpx.adobe.com/acrobat/desktop/work-with-pdf-forms/customize-form-fields/field-settings.html |
| Appearance | Border Color (or No Color), Fill Color, **Line Thickness**, **Line Style** (Solid, Dashed, Beveled, Inset, Underline), Font Size, Text Color. Set these once on a prototype field, then Use Current Properties As New Defaults | same |
| Multiline, scroll, rich text, comb, char limit | Text Field ▸ Options. **Leave Multi-line off by default**; comb requires a character limit; the character limit also bounds how small Auto text shrinks | https://helpx.adobe.com/acrobat/desktop/work-with-pdf-forms/customize-form-fields/field-properties.html, 2025-09-23 |
| Check boxes | Check Box Style (marker shape: check, circle, cross, diamond, square, star), Export Value, "checked by default" | field-properties.html |
| Radio buttons | Grouped by **identical field name** with distinct export values; a mutual-exclusion option governs whether same-name same-value buttons select in unison | field-properties.html; exact 2026 wording `unverified` |
| Hierarchical names | `parent.child` dotted names build a tree for data export and scripting | **unverified on a current Adobe page** |
| Tab order | Prepare Form ▸ Fields pane ▸ **Tab Order**: Order Tabs by **Structure / Row / Column / Unspecified**; drag in the Fields pane to override. Pair with the Forms preference *Automatically adjust tab order when modifying fields* | https://helpx.adobe.com/acrobat/desktop/work-with-pdf-forms/customize-form-fields/set-field-navigation.html |
| Auto-detect off | Preferences ▸ Forms ▸ General ▸ **Automatically detect Form fields** | preferences.html ([07.3]) |
| Calculations | Sum/product presets, **Simplified field notation** (field names with `+ - * /`; breaks on names containing spaces or punctuation), Custom calculation script, and a **calculation order** list reordered with Up/Down | https://helpx.adobe.com/acrobat/desktop/work-with-pdf-forms/customize-form-fields/set-calculation-fields.html |
| **Document-level JS** | Tools ▸ JavaScript ▸ **Document JavaScript**; also `Doc.addScript()`. Document actions via Set Document Action (Will/Did Save, Will/Did Print, Will Close) | https://opensource.adobe.com/dc-acrobat-sdk-docs/library/jsdevguide/JS_Dev_Contexts.html |
| **Folder-level JS** | Two folders, **App** (inside the bundle) and **User**. Adobe's instruction is to resolve them at runtime with `app.getPath("app","javascript")` and `app.getPath("user","javascript")`. The User folder may hold `glob.js` (persistent globals) and `config.js` (UI customisation); every `.js` there loads at start-up, after the App folder | same |
| macOS JS paths | User: `~/Library/Application Support/Adobe/Acrobat/DC/JavaScripts` (subscription) or `…/Acrobat/<year>/JavaScripts` (perpetual). App: `/Applications/Adobe Acrobat DC/Adobe Acrobat.app/Contents/Resources/JavaScripts/` | https://community.adobe.com/t5/acrobat-sdk/what-is-the-location-of-quot-javascripts-quot-folder-on-mac-os/m-p/8749816 — community, not doc. **Whether `DC` becomes `26` in the 2026 build is unverified; settle it on the machine with `app.getPath`** |
| **JavaScript API Reference** | https://opensource.adobe.com/dc-acrobat-sdk-docs/library/jsapiref/index.html — **"Last update: Dec 18, 2023"**. **Verdict: there is no 2026 edition; the JS reference has not been revised for this release** | same |
| Action Wizard | Create, edit, export and import actions; batch over files or folders | https://helpx.adobe.com/acrobat/using/action-wizard-acrobat-pro.html |
| Preflight | Custom profiles, fixups, libraries, droplets, `.kfp`, Output Preview, PDF/X and PDF/A conversion and verification, Ink Manager | https://helpx.adobe.com/acrobat/using/preflight-libraries-acrobat-pro.html |
| **Set Page Boxes** | Tools ▸ Print Production ▸ Set Page Boxes; *Show All Boxes* colour-codes CropBox black, ArtBox red, TrimBox green, BleedBox blue. **Art, Trim and Bleed are Acrobat Pro only** | https://helpx.adobe.com/acrobat/desktop/edit-documents/organize-pages/crop-pages.html |
| Compare Files | All tools ▸ Compare files; *Compare text only*; swap old and new | https://helpx.adobe.com/acrobat/using/compare-documents.html |
| Redact and Sanitize | Redact, redaction codes and code sets, Sanitize (Remove Hidden Information); plus the Documents preference to remove hidden information on close or email | https://helpx.adobe.com/acrobat/desktop/protect-documents/redact-pdfs/sanitize-pdfs.html |
| Scan & OCR | Output styles **Searchable Image**, **Searchable Image (Exact)**, **Editable Text & Images**; language; Downsample To | https://helpx.adobe.com/acrobat/desktop/create-documents/scan-documents-to-pdfs/scanned-pdf-settings.html |
| Combine and Optimize | Combine Files; PDF Optimizer settings with custom presets | https://helpx.adobe.com/acrobat/desktop/create-documents/optimize-pdfs/ |
| Accessibility, PDF/UA | Accessibility Checker, Reading Order tool, autotag including cloud auto-tagging | https://helpx.adobe.com/acrobat/using/creating-accessible-pdfs.html · https://helpx.adobe.com/acrobat/using/cloud-auto-tagging-accessibility-pdfs.html |

### [08.1]-[THE_INDESIGN_TO_ACROBAT_SEAM]

Adobe's own position: "For advanced form workflows, you can export the basic form and then continue editing it in Adobe Acrobat" (https://helpx.adobe.com/be_en/indesign/using/forms.html). InDesign supports the field types, positions and basic appearance; it does **not** support phone and zip formatting, text alignment inside fields, leading, calculations or most advanced text properties (https://creativepro.com/creating-pdf-forms-in-indesign/; Adobe Experience League confirmation 2026-05-20 at https://experienceleaguecommunities.adobe.com/adobe-experience-manager-forms-10/help-styling-indesign-form-to-export-to-interactive-pdf-250620).

**Verdict: draw field geometry in InDesign on the grid, set field behaviour in Acrobat.** The two settings that make this painless are Auto font size (so heights vary without size drift) and Use Current Properties As New Defaults on a prototype field before drawing the rest.

---

## [09]-[SHELL_AUTOMATION_ON_MACOS]

### [09.1]-[APPLESCRIPT_AND_IAC]

Adobe's Interapplication Communication guide is live at https://opensource.adobe.com/dc-acrobat-sdk-docs/library/interapp/toc.html, with `IAC_DevApp_AppleEvents.html` and `IAC_API_AppleEvtObjects.html`. Quoted: *"Acrobat supports the following categories of Apple events: Required events … Core events … Acrobat-specific events … Miscellaneous Apple events"*, and *"When programming for Mac OS, use AppleScript with Acrobat whenever possible. For Apple events that are not available through AppleScript, handle them with C or other programming languages."*

The dictionary covers the required suite (open, print, quit, run), the core suite (close, count, delete, exists, get, make, move, save, set), Acrobat-specific events (find text, goto, zoom, bring to front, page and annotation operations) and **`do script`**, which executes Acrobat JavaScript. On the JS side the corresponding event type is **External/Exec** — "the result of an external access, for example, through OLE, AppleScript, or loading an FDF".

```bash
osascript -e 'tell application "Adobe Acrobat" to do script "app.alert(1)"'
```

**Verdicts.** Acrobat runs natively on Apple silicon and Adobe's guidance is that Intel-compiled plug-ins do not load in native mode (https://helpx.adobe.com/download-install/apps/system-requirements/apps-compatibility-mac-apple-silicon.html). **No Adobe page states that Apple events were removed or broken on the ARM build, and none states they still work** — `unverified`; run the one-liner above before designing around it. There is **no command-line entry to Action Wizard**; nothing in helpx or the SDK exposes actions to a shell. The only documented shell hook is `osascript` driving `do script`, or a third-party plug-in such as Evermap AutoBatch. Both the IAC guide and the JS reference date to the previous SDK generation, so treat them as stable but unrefreshed.

### [09.2]-[CLI_ALTERNATIVES]

| [TOOL] | [VERSION] | [DATE] | [LICENCE] | [BREW] | [BEST AT] |
| :----- | :-------- | :----- | :-------- | :----- | :-------- |
| qpdf | 12.4.1 | 2026-08-27 | Apache-2.0 | `qpdf` | Lossless structural surgery: linearize, decrypt, JSON round-trip, object inspection |
| pdfcpu | v0.15.0 | 2026-08-11 | Apache-2.0 | `pdfcpu` | Corpus validation, split, merge, watermark, stamp, `--progress` |
| OCRmyPDF | v17.11.0 | 2026-08-28 | MPL-2.0 | `ocrmypdf` | Repeatable OCR with concurrency and idempotent skip |
| Ghostscript | 10.08.0 | 2026-09-08 | AGPL-3.0-or-later | `ghostscript` | Deterministic re-distillation, colour conversion, PDF/A via `pdfa_def.ps` |
| poppler | 26.09.0 | 2026-09-03 | GPL-2.0/3.0 | `poppler` | `pdftotext`, `pdftoppm`, `pdfinfo` in pipelines |
| MuPDF (`mutool`) | 1.28.3 | `unverified` | AGPL-3.0-or-later | `mupdf-tools` | One static binary: draw, clean, extract, run JS over documents |
| veraPDF | 1.30.2 | `unverified` | GPL-3.0 or MPL-2.0 | `verapdf` | Authoritative PDF/A and PDF/UA conformance with machine-readable reports |
| tesseract | 5.5.3 | 2026-07-24 | Apache-2.0 | `tesseract` | OCR engine tuning, hOCR/ALTO/PAGE-XML output |
| pdftk-java | 3.3.3 | `unverified` | GPL-2.0-or-later | `pdftk-java` | FDF/XFDF form fill, `dump_data_fields` |
| img2pdf | 0.6.3 | `unverified` | LGPL-3.0-or-later | `img2pdf` | Lossless image to PDF, no recompression |
| cpdf | `unverified` | — | Commercial | not in core brew | Broad scripted page, metadata and annotation manipulation — https://www.coherentpdf.com/ |

### [09.3]-[VERDICTS]

| [TASK] | [ACROBAT] | [CLI] | [RECOMMENDATION] |
| :----- | :-------- | :---- | :--------------- |
| Build or repair an interactive form | Prepare Form, field properties, calc order | pdftk-java (fill only) | **Acrobat** |
| Fill a form from data at scale | Action Wizard, GUI-bound | pdftk-java + FDF/XFDF | **CLI** |
| OCR a directory nightly | Scan & OCR, one file at a time | ocrmypdf + tesseract | **CLI** |
| PDF/A or PDF/UA verdict for an audit | Preflight | veraPDF | **veraPDF** — auditable, machine-readable |
| PDF/A conversion with print intent | Preflight profiles and fixups | Ghostscript | **Acrobat** for fidelity, Ghostscript for volume |
| Split, merge, rotate, stamp | Organize Pages | qpdf, pdfcpu | **CLI** |
| Shrink a file predictably | PDF Optimizer presets | Ghostscript `-dPDFSETTINGS` | **CLI** for repeatability |
| Redact for real | Redact + Sanitize | none equivalent | **Acrobat** — no CLI does true redaction |
| Page boxes for print | Set Page Boxes (Pro) | mutool, cpdf | **Acrobat**, scripted via cpdf when batched |
| Extract text or images for indexing | Export | pdftotext, pdftoppm, mutool | **CLI** |
| Visual diff of two revisions | Compare Files | none equivalent | **Acrobat** |
| Drive Acrobat from a script | — | `osascript … do script` | **AppleScript `do script`** — the only documented shell hook; verify on the ARM build first |

---

## [10]-[CONFLICTS_AND_GAPS]

**Resolved conflicts.**
- Shipping InDesign is **21.5.1** (Adobe community, 2026-08-13) even though the helpx release-notes page still ends at 21.4.1 (2026-06-18). Beta is **21.6.0.23** (2026-08-17). Both are true; the help pages lag.
- "Copy Editor" has **no Adobe documentation** because it is not new and not shipping-documented: it arrived in InDesign 2020 v15.1.2 (August 2020) as `Edit ▸ Edit in Copy Editor (Beta)`, a modal text-only window, and has remained Beta since. Reported file corruption and no speed gain on table-heavy documents. **Do not build the template around it.**
- 34 × 44 in is ASME size **E**, not D. The NCS V6 UDS figure caption that calls it "ANSI D" is wrong; the USACE table is right.

**Verified gaps, listed so nobody re-searches them.**
- No dedicated Adobe page exists for: Optical Margin Alignment, Span/Split Columns, Primary Text Frame, Right-Indent Tab, Indent to Here, Quick Apply, Load Styles from another document. All are in the product; CreativePro carries the documentation.
- `.idpp` and `.dcst` extensions and the `en_US` folder names for Swatches, Preflight profiles and Document presets are not stated on any fetchable page. The fastest route in is **Scripts panel ▸ right-click the User folder ▸ Reveal in Finder**, which lands inside the `[Language]` folder (https://creativepro.com/uncovering-indesigns-hidden-files/). Adobe's own path page gives only the two roots `~/Library/Preferences/Adobe InDesign/Version <#>` and `~/Library/Caches/Adobe InDesign/Version <#>` and does not show the `en_US` level (https://helpx.adobe.com/indesign/desktop/troubleshoot/settings-interface-and-feature-issues/preferences-support-file-locations.html, 2026-06-02); its version table stops at 20.x.
- Acrobat: no official preferences export; custom tool sets appear to have no place in the new UI; Home suppression is undocumented; the JavaScript API reference stopped at 2023-12-18; the macOS Preference Reference stopped at 2023-06.
- Architecture: flat-colour palettes, collage, key-plan systems, caption typography and drawing-title systems are **unverified as 2025-26 winning conventions** — the major architecture publications all return 403, and winning panels publish as image PDFs. Caption systems have no dated authority at all.
- Kelman publishes no type-scale ratio, no measure target and no hanging-punctuation rule.

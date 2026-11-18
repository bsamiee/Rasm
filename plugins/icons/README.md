# Rasm plugin icons, v2: Layered Echo, nastaliq

`generate.py` draws every stroke by sweeping a flat reed along hand-set cubic Bézier skeletons. The generator uses no font, and no outline is traced. `CRITIQUE.md` says why v1 was rebuilt, and `SOURCES.md` lists the evidence.

## Reed

- The reed is 84 units wide on a 1024 canvas, down from 86, and every icon uses the same reed.
- At rest the edge leans back at 112°. Down-left strokes come out heavy and up-left rises thin, which is how a nastaliq bowl behaves.
- **The reed turns.** Each stroke carries keys for edge angle and width along its arc length, eased with smoothstep. The alif turns its reed to 150°, so the head is a full oblique cut instead of v1's 0.34-width sliver. Bowls and kashidas turn a few degrees through the stroke.
- **Every stroke opens with a reed cut and closes on a hairline.** The head is the edge itself, cut at the edge angle. The exit narrows to 7% of the reed (the corner of the nib), with no rounded pressure taper. On the sharpest reference strokes the heavy parts run 12–14 times the hairline.
- The outline is the exact sweep: the union of the quadrilaterals the turning edge traces between 200 samples per Bézier segment.

## Measure

- A **nuqta** is the reed drawn its own width at 40°, against the 112° edge: a rhomb with 72° and 108° corners.
- The **alif** in rasm stands three nuqtas tall, the nastaliq measure. The nuqtas stack vertically with a gap connection (Yaghan, PLOS One 2020).
- An msbuild **brick** is a nuqta drawn on: the reed pulled 2.6 widths along the line, both ends cut on the edge angle.

## Echo

Copies of the ink sit straight below it, in this paint order: slate +44, ground +30, slate +22, ground +8, then ink at 0. The offsets are unchanged from v1. Each SVG defines the ink path once and draws the five layers with `<use>`. Every composition is centered on its ink and echo together, and all of them sit inside the 12% safe zone.

| Palette | Ground    | Ink       | Slate     | Slate on ground |
| :------ | :-------- | :-------- | :-------- | :-------------- |
| Light   | `#EFEAE2` | `#090909` | `#97A9B0` | 2.03:1          |
| Dark    | `#0E0E0E` | `#EFEAE2` | `#4A5D66` | 2.80:1          |

The light slate changed from `#AFBDC2`, which sat at 1.61:1 on the ground: half the visibility of the dark echo, and gone below 64 px. The new value keeps the poster's hue (196°) and saturation, and drops lightness from 0.72 to 0.64.

## Icons

| Plugin           | Strokes                                                                                             | Meaning                                                                    |
| :--------------- | :-------------------------------------------------------------------------------------------------- | :------------------------------------------------------------------------- |
| `rasm`           | An alif three nuqtas tall, beside a vertical column of three nuqtas                                 | The calligrapher's measure: one reed width sizes every stroke in the set   |
| `python`         | Three bowls, each set in a step further than the one above                                          | Blocks nested by indentation                                               |
| `orchestration`  | Three threads leaving from staggered points and converging on one nuqta                             | Parallel agent tasks dispatched at different moments, joined in one result |
| `msbuild`        | Six nuqta-bricks in a 3-2-1 running bond                                                            | Targets built in order, each resting on the targets it depends on          |
| `dotnet`         | Two kashidas crossed by two slanted, alif-like strokes                                              | C#, the sharp: the language the plugin designs for                         |
| `blender`        | Two socket nuqtas joined by an S-shaped stroke, heavy through its diagonal                          | A node link: the node trees, data, and render graph Blender is driven by   |
| `uxp`            | A frame with a heavy kashida header and footer and hairline sides, holding one nuqta                | Plugin code running inside an Adobe host panel                             |
| `function-hooks` | A kashida cut by a long hairline, with the part past the cut slid along it to a lower line         | An agent command intercepted and set on a corrected course                 |
| `cpp` | Two short kashidas, each crossed by one slanted, alif-like stroke | C++, the increment: the language the plugin designs for, beside dotnet's sharp |
| `rust` | A kashida forking near its exit, one tail lifting to a hairline and one dropping to a nuqta | A result: one computation with two typed outcomes, the error held as a value |
| `swift` | One kashida cut twice on the reed edge, each later piece set a step lower | Structured tasks suspending at each `await` and resuming further on |
| `typescript` | Three scattered nuqtas, a hairline, and one kashida past it | Schema decoding unknown input at a boundary into one typed value |
| `search` | Three light kashidas as lines of text, with one heavy kashida set into the middle line | A match found among lines of source, web pages, channels, and shell history |
| `rhino` | A heavy curve swept inside its control polygon, with a nuqta on each control point | A NURBS curve and its control points: the geometry Rhino and Grasshopper build |
| `tool` | A brick with three nuqtas broken from its end, each smaller than the last | A built block taken apart: Ghidra reading a binary, watch-app a running app |
| `web` | Three nuqtas joined in a triangle by strokes that turn heavy or hairline with their direction | Linked documents and sources: Workspace files through gog, notebook sources in Gemini |
| `delegate` | Five nuqtas set evenly on a circle | A council: one task handed to several agents and models at once |
| `terminal` | A chevron of two strokes, one turned heavy against the reed, beside a brick cursor on its baseline | The agent's shell prompt: every command recorded, the session resumable where its cursor stopped |
| `media` | A nuqta between two pairs of arcs, the reed turned so each arc swells at its middle | One source sounding outward: voice first, then audio, video, and images |
| `browser` | Three small nuqtas and a heavy kashida as a header, over a frame of hairline sides and a light kashida base | A browser window, its controls and address bar above the page each engine draws |

None of the arrangements occurs in Perso-Arabic script. Crossings (dotnet, cpp, function-hooks) never happen inside a letter. Dots appear only beside strokes or at their ends, never in diacritic positions above or below a stroke. Bowls are shallow and repeated, so none stands alone as a noon. The alif in rasm appears only as the measure diagram.

## Files

- `<plugin>/icon.svg`, `<plugin>/icon-dark.svg`: 1024 viewBox, full-bleed ground, optimized with `svgo --multipass`
- `assets/icon.png`, `assets/icon-dark.png` in the plugin folder of the same name, else in `<plugin>/`: 1024×1024 from `resvg`, then `oxipng -o 4`
- `generate.py`: the reed model and every skeleton. `uv run --script generate.py [plugin…]` writes the SVGs and prints bounds, the safe-zone check, and ink coverage
- `build.sh`: runs the generator, then svgo, resvg, and oxipng

# [ICONS]

Image sources of products and libraries, and builds deriving each host form from them.

Use `assets/iconography/<host>/README.md` for each host's native icons, captures, guidance, and third-party examples

## [01]-[SOURCES]

Packages keep image sources as SVG masters in `icons/` at their package root:
- `icons/` holds one flat file per glyph, named by what it stands for or by its typed key, with no raster or copy
- Host formats no SVG states (`.ghicon`, Blender `.dat`) sit in `icons/` as their host reads them
- `icons/mark.svg` is a product's mark, one master for its plug-in, extension, package, and site icons
- Masters draw in `currentColor`, and class `accent` marks parts each palette fills apart
- Glyphs a dependency publishes resolve through that package at build
- Libraries keep glyphs their own interface draws in their own `icons/`, and consumers reach each through its typed member
- Host libraries keep the default mark of their host family, as `@rasm/uxp` does for Adobe plug-ins, and a product passes its own where it differs

## [02]-[APPLE]

Apple targets keep containers `actool` compiles at their target root, where Xcode's app template places them:
- `AppIcon.icon` is the Icon Composer document `ASSETCATALOG_COMPILER_APPICON_NAME` names, its layer `Assets/mark.svg` the mark
- Layers stroke at 3/32 of their canvas, 2.25 units of 24
- Layers of a mark with a master elsewhere repeat its paths at that weight, as `actool` reads only a real file inside `Assets/`
- `Assets.xcassets` holds glyphs views draw, each imageset read through its generated `ImageResource` member
- Imagesets reading a mark or a package glyph hold a relative Git symlink to it
- Xcode resources phases and SwiftPM `.process` run `actool`, and no compiled form is tracked

## [03]-[ADOBE]

UXP plug-ins draw every icon in Spectrum 2 outline, whatever set a host's own glyphs use:
- Glyphs are `currentColor` outlines at one 1.5-unit stroke, round caps and joins, and 1.5-unit corner rounding, with no fill, color, plate, or text
- Stroke centerlines sit on quarter units, the subdivision Spectrum 2 states for its 1.5-pixel stroke
- Marks draw on Spectrum 2's 20-unit canvas inside its 1-unit safe area, panel glyphs on the host's 23-unit grid around it
- Plug-in list icons are `pluginList` PNGs at 24 and 48 pixels of `icons/mark.svg` of `@rasm/uxp`, its 20-unit canvas centred at 1 and 2 pixels per unit
- Panel icons are `toolbar` PNGs at 23 and 46 pixels from `icons/<entrypoint id>.svg` of the plug-in package, and a panel without one fails the build
- Icons paint black for light themes and white for dark themes at the opacity of the host's own glyphs, stated per host in `HOSTS` of `@rasm/uxp`
- Brand tiles, colored list icons, and copies of host glyphs stay out of every host form
- Acrobat add-ons run no UXP and register one 20 x 20 ARGB outline glyph per `app.addToolButton`, gray between theme icons `0x6F6F6F` and `0xC2C2C2`

## [04]-[RHINO]

Rhino plug-ins draw on transparency in Rhino 9's toolbar language, Open Color grays with one blue accent:
- Command faces and marks are 36-unit artboards with 1.5-unit strokes and round joins, 1 px at the 24 px button
- Strokes stay heavier than the 0.5-unit outlines of Rhino's own command glyphs, a third of a pixel at the 24 px button
- Command faces take their typed key as file name (`Lancet.svg`), and toolbars fall back to the mark for a command with none
- Linework paints `RhinoIconColor` in light and `RhinoIconDarkColor` in dark, gray 9 and Rhino's dark linework gray by default in `Rasm.Rhino.props`
- `RhinoIconPart` rows fill and outline one class each in both themes, `accent` in blue 5 and its edge tone by default
- Each master paints into a `<stem>.svg` and `<stem>_darkmode.svg` resource, the pair Rhino's asset system and the `.rui` writer read
- `IconResource.PlugIn` reads the `mark.svg` pair for license, splash, and toolbar art
- Forms that cannot switch theme paint the mark in one mid-tone clearing 3:1 against both Settings backgrounds (`#ffffff` light, `#212121` dark)
- Mid-tone is `RhinoPlugInIconColor`, Open Color gray 6 by default at 3.32:1 and 4.85:1
- Plug-in icons are `PlugIn.ico`, the full-frame mid-tone mark at 16, 24, 32, 48, and 256 px, as RhinoCommon reads `DescriptionType.Icon` as ICO
- Yak icons are a 64 px `icon.png` of the mid-tone mark, as Package Manager draws one PNG on both themes
- Plug-ins declaring toolbars write `<TargetName>.rui` beside the `.rhp`, every toolbar inside a toolbar group so it shows on first load
- Library buttons read Rhino's stock glyph through `ResourceIds` where one names the action, at rest, as `_hot` marks hover and `_gray` disabled
- Pages without an icon draw `PlugIn.ico` through `PlugIn.Icon`

## [05]-[GRASSHOPPER_2]

Grasshopper 2 is the one host outside the build theming path, as its SVG reader colors only `gh:<Role>` paints and `currentColor`, turns arcs into straight lines, and drops rounded corners, classes, and transforms:
- `usvg` writes constant RGB and refuses `gh:` values, so no build step writes a Grasshopper 2 icon
- Icons are `.ghicon` files from the Grasshopper 2 Icon panel, which keeps arcs and names color roles
- Color roles (`Input`, `Output`, `Analysis`, `Symbolic`, `Foreground`) resolve per skin and canvas
- Icons draw on a 24-unit frame aligned to logical pixels, parameters as a hexagon with a glyph and components as free drawings
- `icons/<TypeName>.ghicon` embeds under its file name, and `DocumentObject.IconInternal` loads a component's icon with no code
- Value types read `<TypeName>.ghicon` before their key type's icon, and plug-ins read `mark.ghicon` before a letter disc
- One role icon serves light and dark canvases, with no `_darkmode` pair or second drawing
- Regions outline in their role's `Edge` shade, never black

## [06]-[BLENDER]

Blender extensions draw on Blender's UI icon grid in one white shape, and the theme decides every color:
- Glyphs draw on a 16-unit view box, Blender's 1600-unit icon document scaled, with 1-unit padding and 1-unit minimum features and lines
- Each glyph is one `currentColor` silhouette, secondary parts at lower opacity, toggle pairs hollow and filled, with no text, color, or Blender logo
- Built-in names (`icon='NAME'`) serve every concept Blender has a glyph for, as Blender recolors its own icons
- `Icons` loads each SVG once per registration through `bpy.utils.previews` `load(name, path, 'IMAGE')`, keeps alpha, and drops the untinted preview
- Previews draw untinted, so draws write glyph alpha in their text color, one preview per place, rewritten on a theme change
- Menus and pies tint by their item widget, dropdowns, operators, toolbar items, and toggles by their own, labels and flat buttons by region text
- Sidebar tab marks tint by `wcol_tab` text and set `bl_icon_value` on every panel of their category, as a tab takes its first panel's icon
- Tabs with a mark keep a readable `bl_category`, which compact tabs show as the tooltip
- Toggles on, chosen enum rows, and depressed operators take selected text, and highlighted gizmos take the region's highlighted text
- Hover keeps a layout button's resting tint, as Blender reruns no layout draw on hover and draws preview pixels as written
- Toolbar tools take `.dat` triangle icons, the one form Blender's toolbar draws, by built-in name (`ops.generic.select_box`) or `icons/<name>.dat`
- Tool icons keep Blender's accents (green adds, red removes, purple edits, orange selects), and Blender inverts their lightness in light themes
- Previews free with their collection at unregister, and a name the collection holds refuses a second `load`

## [07]-[BUILDS]

Builds derive every host form from masters into build output:
- `resvg` rasterizes and `usvg` writes themed SVG, both CLIs mise installs from `linebender/resvg`
- Build theming writes each host tone over `currentColor` and part classes as a stylesheet in build output, the form both CLIs read, for all hosts but Grasshopper 2
- `@rasm/mark` paints a `Tone` into a wrapper SVG placing a canvas at a whole pixel scale or artwork inside a padding for `resvg`
- Tones come from the host's library (`HOSTS` of `@rasm/uxp`, `Rasm.Rhino.props`), a web app's theme, or a product's own value, a Chrome extension's `config.brand`
- `@rasm/hosting/head` rasterizes web favicons, touch icons, and PWA icons through `@vite-pwa/assets-generator`
- ImageMagick packs PNG frames into an ICO and draws nothing
- Libraries read masters from `icons/` of their package, and a caller passes a master another package publishes and names it as a build input
- Hosts theming at run time (Blender preview tint, Apple template rendering) read an untinted master

## [08]-[HOSTS]

Each host form, its build owner, and files it writes:

| [INDEX] | [HOST]        | [OWNER]                                                             | [WRITES]                                          |
| :-----: | :------------ | :------------------------------------------------------------------ | :------------------------------------------------ |
|  [01]   | Rhino         | `Rasm.Rhino.targets`, `products/apps/rhino/Directory.Build.targets` | Theme pairs, `PlugIn.ico`, `icon.png`, `.rui`     |
|  [02]   | Grasshopper 2 | `Rasm.Grasshopper.targets`                                          | Embedded `<TypeName>.ghicon` and `mark.ghicon`    |
|  [03]   | Blender       | `rasm.extension.ui` at load and draw                                | Previews tinted per place, turned variants        |
|  [04]   | Adobe UXP     | `@rasm/uxp/vite`                                                    | Plug-in and panel PNGs per theme and scale        |
|  [05]   | Chrome        | `@rasm/vite/chrome`                                                 | Manifest PNG sizes                                |
|  [06]   | Web           | `@rasm/hosting/head`                                                | ICO, touch and PWA icons, painted SVG, head links |
|  [07]   | Apple         | `actool`                                                            | `Assets.car`, `.icns`, `ImageResource` members    |

# [THEMES_AND_GROUNDS]

Themes are the session's look (ground, colors, glass, sound, style), set through the `Theme` service, and its scenes and layers draw from `render.Backdrop` and `render.Underlay` rows. Colors and palettes follow the engine's own `plugin-authoring` skill (`look.md`), which this reference leaves to it.

## [01]-[THEME]

`Theme` holds one theme per session, whoever set it:
- `set(attributes)` replaces the whole theme, each attribute typed by the engine: `backdrop` (a scene by name or a ground), `glass`, `overrideColorScheme`, `colors`, `sound`, `style`
- `clear` removes the session's theme, any plugin's, answering `isDrawn: false` with a reason
- `preview(attributes)` is scoped: closing the scope restores what stood when the first open preview began, and nested previews restore one by one
- Sets or clears by anyone during a preview stand at the scope's close
- Another plugin's theme with a scene fails `preview`, as this plugin cannot draw that plugin's scene again
- Themes outlive `/clear` and go with the plugin that set them when that plugin unloads
- Ask before keeping a look: `Effect.scoped(Effect.andThen(theme.preview(look), ask(...)))`, then `theme.set(look)` on the keep answer
- Mods set a theme at `session.start` or on the person's ask, never on a timer

## [02]-[BACKDROP]

`render.Backdrop` keys one scene row by the scene name the mod names in `Theme.set({ backdrop: { scene } })`:
- `draw` answers the scene's tree (an `Svg` or `Shader`), reading `e.props.glass` and `e.props.overrideColorScheme` the engine fills
- `pointer(e, state)` hears clicks on the scene (`e.x`, `e.y`), then the library redraws every site and passes the event on
- Backdrops draw on desktop and mobile alone, and a scene row draws only for the plugin that set the theme
- Shader uniforms come from the state value, so a pointer writes the state and the redraw follows

## [03]-[UNDERLAY]

`render.Underlay` is a layer under the whole terminal window in fullscreen, raised for each plugin's own layer:
- Main screens never ask it, and a dialog, question, picker, or exit flow hides it
- `Raster` elements in it repaint by `Placed.blit({ requestId: 'site:underlay', key, ...cells })` without a redraw
- `UnderlayList` orders plugins' layers, first on top
- Underlays and backdrops stay rare: ambient visuals with no fact the person must read

## [04]-[SURFACES]

| [INDEX] | [SITE]     | [TERMINAL]       | [DESKTOP] | [VS_CODE] | [MOBILE] |
| :-----: | :--------- | :--------------- | :-------- | :-------- | :------- |
|  [01]   | `Backdrop` | Never            | Scene     | Never     | Scene    |
|  [02]   | `Underlay` | Fullscreen alone | Never     | Never     | Never    |

# [CAPTURES]

Pictures of a scene, an area, or the window, and the evaluated state record they answer to.

## [01]-[CHANGES]

Each change ends with one call that records the evaluated state and draws the changed objects against the records before it:
- New targets take their baseline from the first `snapshot("<name>")` and `capture("<name>-<view>")` after the build
- Existing targets take the baseline before the first edit, one capture per view the work judges
- Later changes pass the previous names as `since`, and each new name becomes the next baseline

```python
# [EXECUTE_BLENDER_CODE] Snapshot and front capture of the changed object against earlier ones
from capture import capture
from results import as_result
from snapshot import snapshot

result = {"changes": as_result(snapshot("<after>", since="<before>")), "front": as_result(capture("<after>-front", objects=("<Object>",), since="<before>-front"))}
```

Readings of each comparison decide the next step:

| [INDEX] | [READING]                                     | [NEXT_STEP]                                                        |
| :-----: | :-------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | Snapshot `changed` holds the edited values    | Read `<after>-front-diff.png` for where the edit shows             |
|  [02]   | Snapshot `changed` holds a value never edited | Trace its owner (modifier input, driver, node link) first          |
|  [03]   | Snapshot `added` or `removed` lists a name    | Check the count against the edit, `datablocks` counts each ID kind |
|  [04]   | Hash `shape` alone changed                    | Read the counts beside it or the F-curve keys                      |
|  [05]   | Capture `outside` is true                     | Capture again without `since` to frame the new extent whole        |
|  [06]   | Capture `changed` is 0 after a visible edit   | Capture another view, or `view="user"` under the viewport shading  |
|  [07]   | `MissingCapture` or `MissingSnapshot`         | Take the baseline under that name first                            |

- Capture `changed` counts pixels past Blender's render-test threshold
- `<name>-diff.png` draws the earlier capture at half brightness and mixes changed pixels half with red
- Snapshot `changed` nests each value's before and after under its keys and list indexes, and `comparison` is `None` without `since`
- Solid shading draws a material's viewport color under `color_type` `MATERIAL` alone, other material edits read from the snapshot

## [02]-[CAPTURE]

`capture("<name>", objects=, view=, size=, since=)` writes `.artifacts/blender/<name>.png`, overwriting a repeated name:
- `view` takes `iso` (default) in perspective, `top`, `bottom`, `front`, `back`, `right`, and `left` orthographic, and `user`
- `view="user"` draws the largest 3D Viewport's view at its horizontal field of view, live or from the file's stored view
- Frames fit the evaluated box of each object in `objects`, or of every object the viewport shows, with 2.5% of the longest side added around
- `WIRE` and `BOUNDS` display types, curves with no bevel or extrusion, loose edges, and empties draw nothing
- User views take the viewport's device pixels, other views the render's pixel count at the framed extent's aspect
- Sizes stay within 2000 px on the long side, and `size` names one exactly
- `Read` downscales an image past 2000 px and re-encodes one past 500 KB as JPEG, default captures stay under both
- Pixel reads (levels, line widths, band edges, dither) pass `size=(region.width, region.height)` for the viewport's device pixels
- `since="<before>"` redraws the view, frame, and size `<before>.png` stores, a user view compares while the viewport's view stays
- PNGs hold no ICC profile, `magick <png> -format "%[pixel:p{<x>,<y>}]" info:` reads a byte as drawn
- Background calls render Workbench under the viewport's stored Solid shading, surfaces drawing the bytes of the live draw
- Background fill follows the preferences' theme, lighter under the factory preferences of `run` than in a `start` session

Cases other than `Capture` name what the call lacked:

| [INDEX] | [CASE]             | [NEXT_STEP]                                                   |
| :-----: | :----------------- | :------------------------------------------------------------ |
|  [01]   | `UnknownObjects`   | Read names from `get_objects_summary`                         |
|  [02]   | `HiddenInViewport` | Leave local view or unhide the object in the viewport         |
|  [03]   | `EmptyFrame`       | Name objects with faces, or read their bounds from `snapshot` |
|  [04]   | `NoViewport`       | Take an axis or iso view                                      |
|  [05]   | `UnknownView`      | Pick a name from `views`                                      |

## [03]-[SNAPSHOT]

`snapshot("<name>", objects=, since=)` writes `.artifacts/blender/<name>.json` for the named or every object of the scene:
- Objects hold world transform, evaluated bounds with instances, material slots, visibility, and geometry counts with a hash
- Geometry hashes cover positions and instance transforms, and animation holds action, slot, and a hash per F-curve
- Objects hold modifiers with the settings each type adds, constraints, and drivers
- Scenes hold frame, engine, camera, unit and color settings, orphan count, library files, and missing paths
- Files hold material settings, datablock counts per kind, and node trees of registered types as `nodes.py` digests

## [04]-[WINDOW]

Window pictures come from the MCP screenshot tools and from `screencapture` at 1:1 device pixels:
- `get_screenshot_of_window_as_image` draws the window content without its title bar
- `blender` screenshots past 785 KB shrink bilinearly to logical pixels, then until they fit, with no note in the answer
- `get_viewport_screenshot` draws the context screen's first 3D Viewport offscreen at `max_size` with grid and cursor and no text
- `get_viewport_screenshot` answers `No 3D viewport found` while the context window shows a maximized render view

```bash
# Layer-0 windows of the GUI with their ids, the main window titled `<file> — Blender <version>`
uv run --with pyobjc-framework-Quartz python -c 'import Quartz as q; [print(w[q.kCGWindowNumber], w.get(q.kCGWindowName)) for w in q.CGWindowListCopyWindowInfo(q.kCGWindowListOptionOnScreenOnly, q.kCGNullWindowID) if w[q.kCGWindowOwnerName] == "Blender" and w[q.kCGWindowLayer] == 0]'

# Window at 1:1 device pixels under the display's ICC profile, without activating it
screencapture -x -o -l <id> .artifacts/blender/<name>.png

# Readable copy in sRGB at logical pixels as an undithered palette
magick .artifacts/blender/<name>.png -profile "/System/Library/ColorSync/Profiles/sRGB Profile.icc" -resize 50% -strip +dither -colors 256 PNG8:.artifacts/blender/<name>-read.png

# One area at device pixels from its `x`, `y`, `width`, and `height`, `y` counted from the window's bottom edge
magick .artifacts/blender/<name>.png -profile "/System/Library/ColorSync/Profiles/sRGB Profile.icc" -gravity SouthWest -crop <width>x<height>+<x>+<y> +repage .artifacts/blender/<name>-<area>.png
```

- Window ids change at every launch, and a stale id answers `could not create image from window`
- Theme and view writes show after `area.tag_redraw()` on every area and a return from the call
- Region writes that rebuild no region (`active_panel_category`) show a frame later

Use interface.md for second windows and popups.

# [TS_RUNTIME_API_EFFECT_PRINTER_ANSI]

`@effect/printer-ansi` instantiates the `@effect/printer` annotation parameter to `Ansi`: `Ansi` is a monoid of terminal directives (SGR text weight/style, 3/4-bit foreground and background color, cursor movement, screen/line erase, bell), `AnsiDoc` is the `Doc<Ansi>` alias, and `AnsiDoc.render` is the terminal renderer that lowers a styled document and resolves each `Ansi` annotation to escape codes through the stream's push/pop-annotation events (correctly nesting and resetting styles). Color is a closed 8-variant `Color` union lifted to normal/bright/background/bright-background through four parameterized constructors. It is the concrete-markup edge of the render rail; the terminal string is produced here and nowhere else.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/printer-ansi`
- package: `@effect/printer-ansi`
- license: MIT
- asset: ESM `.d.ts` declaration surface (`dist/dts/*.d.ts`); peer `effect` + `@effect/printer`
- owner: `edge`
- rail: render
- peer: `@effect/printer` (`Doc`/`DocStream`/`Layout` — `Ansi` is the annotation `A`), `effect` (`Match` on `Color`, `Monoid` over `Ansi`)
- namespaces: `Ansi` (the annotation monoid + directive constructors), `Color` (the 8-color union + `toCode`), `AnsiDoc` (`Doc<Ansi>` alias + directive documents + `render`)
- capability: monoidal terminal-directive annotation (weight/style/fg/bg color, cursor, erase, bell), the `AnsiDoc = Doc<Ansi>` styled-document alias, and the escape-code renderer that resolves annotations via push/pop-annotation stream events with correct style nesting
- import: `import { Ansi, AnsiDoc, Color } from "@effect/printer-ansi"`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: Ansi annotation monoid (`Ansi`)
- rail: render
- `Ansi` is the concrete annotation the printer's phantom `A` becomes. It is a monoid: `Ansi.combine` accumulates directives left-to-right, so `bold ⊕ color(red) ⊕ underlined` is one composite annotation applied by a single `Doc.annotate`. `stringify` projects an `Ansi` to its raw SGR/CSI escape string for non-document use.

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]      | [RAIL]                                                           |
| :-----: | :---------------- | :----------------- | :--------------------------------------------------------------- |
|  [01]   | `Ansi.Ansi`       | branded interface  | monoid of terminal directives; the printer annotation `A = Ansi` |
|  [02]   | `Ansi.AnsiTypeId` | unique symbol type | nominal brand on the `Ansi` interface                            |

[PUBLIC_TYPE_SCOPE]: Color model (`Color`)
- rail: render
- `Color` is the closed 3-bit base palette; the bright and background variants are produced by the `Ansi` constructors, not by separate color values. Refine with `Match.type<Color>()` or `Color.toCode` — never a string switch.

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]  | [RAIL]                                                  |
| :-----: | :------------------------------------ | :------------- | :------------------------------------------------------ |
|  [01]   | `Color.Color`                         | tagged union   | `Black\|Red\|Green\|Yellow\|Blue\|Magenta\|Cyan\|White` |
|  [02]   | `Color.Black`/`Red`/`Green`/…/`White` | node interface | the eight base-color singleton interfaces               |

[PUBLIC_TYPE_SCOPE]: styled document alias (`AnsiDoc`)
- rail: render
- `AnsiDoc` is a plain type alias, so the entire `Doc` combinator surface applies unchanged — `AnsiDoc` composes with `Doc.hsep`/`group`/`align` directly. `AnsiDoc.RenderConfig` mirrors `Doc.RenderConfig` (compact | pretty | smart) but the renderer emits escape codes.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                                                         |
| :-----: | :--------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `AnsiDoc.AnsiDoc`      | type alias    | `Doc<Ansi>` — a styled terminal document                       |
|  [02]   | `AnsiDoc.RenderConfig` | config union  | `render` discriminant: `compact` \| `pretty`/`smart` + options |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: text style + color directives (`Ansi`)
- rail: render
- Weight/style directives are constants; color is four parameterized constructors over a `Color` (`color`/`brightColor` for foreground, `bgColor`/`bgColorBright` for background). Named color constants (`Ansi.red`, `Ansi.bgBlueBright`, …) are fixed rows over those four constructors — reach for the constant when the color is literal, the constructor when the `Color` is computed.

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :---------------------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `Ansi.bold` / `italicized` / `underlined` / `strikethrough` | style          | text weight + decoration constants          |
|  [02]   | `Ansi.color(c)` / `Ansi.brightColor(c)`                     | fg constructor | foreground normal / bright of a `Color`     |
|  [03]   | `Ansi.bgColor(c)` / `Ansi.bgColorBright(c)`                 | bg constructor | background normal / bright of a `Color`     |
|  [04]   | `Ansi.red`/`green`/…/`whiteBright` (16 fg constants)        | fg row         | literal rows over `color`/`brightColor`     |
|  [05]   | `Ansi.bgRed`/…/`bgWhiteBright` (16 bg constants)            | bg row         | literal rows over `bgColor`/`bgColorBright` |

[ENTRYPOINT_SCOPE]: cursor + erase + bell directives (`Ansi`)
- rail: render
- Terminal-control directives drive live/interactive output (progress bars, spinners, wizards). Each is an `Ansi` annotation; the `AnsiDoc` namespace re-exports the same names as `Doc<Ansi>` values so they compose inside a document.

| [INDEX] | [SURFACE]                                                                      | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :----------------------------------------------------------------------------- | :------------- | :------------------------------------- |
|  [01]   | `Ansi.cursorTo(col, row?)` / `Ansi.cursorMove(col, row?)`                      | cursor         | absolute / relative cursor positioning |
|  [02]   | `Ansi.cursorUp(n?)`/`cursorDown`/`cursorForward`/`cursorBackward`/`cursorLeft` | cursor         | directional cursor movement            |
|  [03]   | `Ansi.cursorNextLine(n?)`/`cursorPrevLine(n?)`                                 | cursor         | line-start move down / up              |
|  [04]   | `Ansi.cursorSavePosition`/`cursorRestorePosition`/`cursorHide`/`cursorShow`    | cursor         | save/restore/visibility                |
|  [05]   | `Ansi.eraseLines(rows)`/`eraseLine`/`eraseEndLine`/`eraseStartLine`            | erase          | line-scoped clear directives           |
|  [06]   | `Ansi.eraseScreen`/`eraseDown`/`eraseUp`                                       | erase          | screen-region clear directives         |
|  [07]   | `Ansi.beep`                                                                    | signal         | terminal bell directive                |

[ENTRYPOINT_SCOPE]: monoid ops + color codes
- rail: render
- Combine directives into one composite annotation, project a `Color` to its base code, or stringify an `Ansi` outside a document.

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :------------------------------------------------ | :------------- | :---------------------------------------------- |
|  [01]   | `Ansi.combine(self, that)` / `Ansi.combine(that)` | monoid         | dual: accumulate two directives into one `Ansi` |
|  [02]   | `Ansi.stringify(self)`                            | projection     | raw SGR/CSI escape string for the directive     |
|  [03]   | `Color.toCode(color)`                             | projection     | base 3-bit color code (`number`) for `Color`    |
|  [04]   | `Color.black`/`red`/…/`white`                     | color row      | the eight base-`Color` value constants          |

[ENTRYPOINT_SCOPE]: styled-document rendering (`AnsiDoc`)
- rail: render
- `AnsiDoc.render` is the terminal renderer: it lowers `Doc<Ansi>` through the chosen layout algorithm and, at each `PushAnnotation`/`PopAnnotation` stream event, emits the accumulated `Ansi` escape sequence and its reset — so nested styles restore correctly. This replaces `Doc.render` (which drops annotations) at the terminal edge.

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [RAIL]                                                   |
| :-----: | :-------------------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `AnsiDoc.render(self, config)` / `AnsiDoc.render(config)` | render façade  | dual: lower + resolve `Ansi` to escape codes -> `string` |
|  [02]   | `AnsiDoc.cursorTo`/`cursorUp`/`eraseLines`/`beep`/…       | directive doc  | `Ansi` control directives as `Doc<Ansi>` values          |

## [04]-[IMPLEMENTATION_LAW]

[ANSI_TOPOLOGY]:
- `Ansi` is a monoid of terminal directives; accumulate style with `Ansi.combine` and attach the composite with a single `Doc.annotate`, never one `annotate` per style bit. `bold`/`italicized`/`underlined`/`strikethrough` are constants; color is four constructors (`color`/`brightColor`/`bgColor`/`bgColorBright`) over the closed `Color` union, with 32 named constants as their fixed rows.
- `AnsiDoc = Doc<Ansi>` inherits the full `@effect/printer` combinator surface unchanged; styling is orthogonal to layout. Author structure with `Doc` combinators, attach `Ansi` with `annotate`, render with `AnsiDoc.render`.
- rendering resolves annotations at stream `PushAnnotation`/`PopAnnotation` boundaries, emitting the SGR escape and its reset in balanced pairs so nested styles do not leak. `Doc.render` (annotation-erasing) is the wrong renderer for styled output.
- cursor/erase directives exist as both `Ansi` annotations and `AnsiDoc` documents; live output (spinners, progress, wizards) composes the `AnsiDoc` directive values into the document stream rather than emitting raw escapes.

[STACKS_WITH]:
- `@effect/printer` (`.api/effect-printer.md`): this package *is* the `A = Ansi` instantiation of that algebra. Every `Doc` constructor/combinator (`hsep`/`group`/`align`/`encloseSep`/`reflow`) produces `AnsiDoc` values directly; `Doc.reAnnotate` retargets `Ansi => OtherAnsi` for theme remapping, and `Doc.unAnnotate` strips styling to a plain `Doc<never>` for non-TTY output. Compose the document abstractly, bind `Ansi` only where a literal style is chosen.
- `@effect/cli` (`.api/effect-cli.md`): the `cli/render` rows fold structured verb output to `AnsiDoc` and terminate with `AnsiDoc.render`; `@effect/cli`'s `HelpDoc`/usage rendering already targets this renderer. doctor/replay/inspect ops family emits `AnsiDoc` diagnostics, and a `--no-color`/non-TTY branch swaps in `Doc.unAnnotate` before render — one document, two egress forms.
- `effect` (`.api/effect.md`): `Color` is refined with `Match.type<Color>()`; `Ansi` composes through `effect/Monoid.combineAll` for directive accumulation, and `AnsiDoc.render` is wrapped in `Effect.sync` (or written to `Console`) at the effectful edge. TTY capability detection (`process.stdout.isTTY`) gates the styled-vs-plain renderer choice via a `Match` on the terminal context.

[LOCAL_ADMISSION]:
- `cli/render` folder builds every styled terminal artifact as `AnsiDoc`: semantic roles (error/warn/path/emphasis) map to reusable `Ansi` monoid values, structure composes through `@effect/printer` combinators, and the boundary folds with `AnsiDoc.render(config)` under the appropriate layout algorithm.
- Gate color at the terminal-context edge, not per call site: a non-TTY or `--no-color` context strips annotations with `Doc.unAnnotate` before render so the same document serves both piped and interactive output.
- Live/interactive output composes the `AnsiDoc` cursor/erase directives into the document stream (progress redraw, spinner frames), keeping raw escape sequences out of application code entirely.

[RAIL_LAW]:
- Package: `@effect/printer-ansi`
- Owns: the `Ansi` terminal-directive monoid, the `Color` palette, the `AnsiDoc = Doc<Ansi>` alias, and the escape-code terminal renderer
- Accept: `Ansi.combine`d directives attached via `Doc.annotate`, `color`/`bgColor` constructors over `Color`, `AnsiDoc` composition through the `@effect/printer` surface, `AnsiDoc.render` at the terminal edge, `Doc.unAnnotate` for the non-styled branch
- Reject: raw ANSI escape strings in application code, one `annotate` per style bit, `Doc.render` for styled output, string-switch color selection, unconditional styling that ignores TTY/`--no-color` context

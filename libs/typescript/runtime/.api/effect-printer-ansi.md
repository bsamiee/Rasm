# [TS_RUNTIME_API_EFFECT_PRINTER_ANSI]

`@effect/printer-ansi` instantiates the `@effect/printer` annotation parameter to `Ansi`, a monoid of terminal directives that `Doc.annotate` attaches and `AnsiDoc.render` lowers to escape codes. It owns the concrete-markup edge of the render rail: the terminal string is produced here and nowhere else.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/printer-ansi`
- package: `@effect/printer-ansi` (MIT)
- module: ESM `.d.ts` surface (`dist/dts/*.d.ts`)
- owner: `edge`
- rail: render
- peer: `@effect/printer` (`Doc`/`DocStream`/`Layout`; `Ansi` is the annotation `A`), `effect` (`Match`, `Monoid`)
- namespaces: `Ansi` (directive monoid + constructors), `Color` (8-color union + `toCode`), `AnsiDoc` (`Doc<Ansi>` alias + directive documents + `render`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: Ansi annotation monoid (`Ansi`)

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]      | [CAPABILITY]                                                     |
| :-----: | :---------------- | :----------------- | :--------------------------------------------------------------- |
|  [01]   | `Ansi.Ansi`       | branded interface  | monoid of terminal directives; the printer annotation `A = Ansi` |
|  [02]   | `Ansi.AnsiTypeId` | unique symbol type | nominal brand on the `Ansi` interface                            |

[PUBLIC_TYPE_SCOPE]: Color model (`Color`)

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]  | [CAPABILITY]                                            |
| :-----: | :------------------------------------ | :------------- | :------------------------------------------------------ |
|  [01]   | `Color.Color`                         | tagged union   | `Black\|Red\|Green\|Yellow\|Blue\|Magenta\|Cyan\|White` |
|  [02]   | `Color.Black`/`Red`/`Green`/…/`White` | node interface | the eight base-color singleton interfaces               |

[PUBLIC_TYPE_SCOPE]: styled document alias (`AnsiDoc`)

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :--------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `AnsiDoc.AnsiDoc`      | type alias    | `Doc<Ansi>` — a styled terminal document                       |
|  [02]   | `AnsiDoc.RenderConfig` | config union  | `render` discriminant: `compact` \| `pretty`/`smart` + options |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: text style + color directives (`Ansi`)

| [INDEX] | [SURFACE]                                                   | [SHAPE] | [CAPABILITY]                                |
| :-----: | :---------------------------------------------------------- | :------ | :------------------------------------------ |
|  [01]   | `Ansi.bold` / `italicized` / `underlined` / `strikethrough` | static  | text weight + decoration constants          |
|  [02]   | `Ansi.color(c)` / `Ansi.brightColor(c)`                     | factory | foreground normal / bright of a `Color`     |
|  [03]   | `Ansi.bgColor(c)` / `Ansi.bgColorBright(c)`                 | factory | background normal / bright of a `Color`     |
|  [04]   | `Ansi.red`/`green`/…/`whiteBright` (16 fg constants)        | static  | literal rows over `color`/`brightColor`     |
|  [05]   | `Ansi.bgRed`/…/`bgWhiteBright` (16 bg constants)            | static  | literal rows over `bgColor`/`bgColorBright` |

[ENTRYPOINT_SCOPE]: cursor + erase + bell directives (`Ansi`)

| [INDEX] | [SURFACE]                                                                      | [SHAPE] | [CAPABILITY]                           |
| :-----: | :----------------------------------------------------------------------------- | :------ | :------------------------------------- |
|  [01]   | `Ansi.cursorTo(col, row?)` / `Ansi.cursorMove(col, row?)`                      | factory | absolute / relative cursor positioning |
|  [02]   | `Ansi.cursorUp(n?)`/`cursorDown`/`cursorForward`/`cursorBackward`/`cursorLeft` | factory | directional cursor movement            |
|  [03]   | `Ansi.cursorNextLine(n?)`/`cursorPrevLine(n?)`                                 | factory | line-start move down / up              |
|  [04]   | `Ansi.cursorSavePosition`/`cursorRestorePosition`/`cursorHide`/`cursorShow`    | static  | save/restore/visibility                |
|  [05]   | `Ansi.eraseLines(rows)`/`eraseLine`/`eraseEndLine`/`eraseStartLine`            | factory | line-scoped clear directives           |
|  [06]   | `Ansi.eraseScreen`/`eraseDown`/`eraseUp`                                       | static  | screen-region clear directives         |
|  [07]   | `Ansi.beep`                                                                    | static  | terminal bell directive                |

[ENTRYPOINT_SCOPE]: monoid ops + color codes (`Ansi`, `Color`)

| [INDEX] | [SURFACE]                                         | [SHAPE] | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------ | :------ | :---------------------------------------------- |
|  [01]   | `Ansi.combine(self, that)` / `Ansi.combine(that)` | fold    | dual: accumulate two directives into one `Ansi` |
|  [02]   | `Ansi.stringify(self)`                            | fold    | raw SGR/CSI escape string for the directive     |
|  [03]   | `Color.toCode(color)`                             | fold    | base 3-bit color code (`number`) for `Color`    |
|  [04]   | `Color.black`/`red`/…/`white`                     | static  | the eight base-`Color` value constants          |

[ENTRYPOINT_SCOPE]: styled-document rendering (`AnsiDoc`)

| [INDEX] | [SURFACE]                                                 | [SHAPE] | [CAPABILITY]                                             |
| :-----: | :-------------------------------------------------------- | :------ | :------------------------------------------------------- |
|  [01]   | `AnsiDoc.render(self, config)` / `AnsiDoc.render(config)` | fold    | dual: lower + resolve `Ansi` to escape codes -> `string` |
|  [02]   | `AnsiDoc.cursorTo`/`cursorUp`/`eraseLines`/`beep`/…       | factory | `Ansi` control directives as `Doc<Ansi>` values          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Ansi` is a monoid: accumulate directives with `Ansi.combine` and attach the composite with one `Doc.annotate`, never one `annotate` per style bit; `Ansi.stringify` projects a directive to its raw SGR/CSI string outside a document.
- Weight/style are constants and color is four constructors (`color`/`brightColor`/`bgColor`/`bgColorBright`) over the closed `Color` union with 32 named constants as fixed rows — the constant serves a literal color, the constructor a computed `Color`.
- Bright and background variants come from the constructors, never separate `Color` values, and a `Color` refines through `Match.type<Color>()` or `Color.toCode`, never a string switch.
- `AnsiDoc = Doc<Ansi>` inherits the full `@effect/printer` combinator surface unchanged: author structure with `Doc` combinators, attach `Ansi` with `annotate`, render with `AnsiDoc.render`; styling stays orthogonal to layout.
- `AnsiDoc.render` resolves annotations at stream `PushAnnotation`/`PopAnnotation` boundaries, emitting each SGR escape and its reset in balanced pairs so nested styles never leak; `Doc.render` erases annotations and misrenders styled output.
- Cursor and erase directives carry both an `Ansi` annotation and an `AnsiDoc` document form, so live output — spinners, progress, wizards — composes the `AnsiDoc` directive values into the document stream.

[STACKING]:
- `@effect/printer`(`.api/effect-printer.md`): this package IS the `A = Ansi` instantiation of that algebra — every `Doc` combinator (`hsep`/`group`/`align`/`encloseSep`/`reflow`) yields `AnsiDoc` directly, `Doc.reAnnotate` retargets `Ansi => OtherAnsi` for theme remapping, and `Doc.unAnnotate` strips to `Doc<never>` for non-TTY output.
- `@effect/cli`(`.api/effect-cli.md`): `cli/render` folds structured verb output to `AnsiDoc` and terminates with `AnsiDoc.render`; `HelpDoc.toAnsiDoc` is an `AnsiDoc`, so parse-error help and app output share this one render rail.
- `effect`(`.api/effect.md`): `Color` refines through `Match.type<Color>()`, `Ansi` accumulates through `Monoid.combineAll`, and `AnsiDoc.render` runs inside `Effect.sync` or writes to `Console` at the effectful edge, a `Match` on `process.stdout.isTTY` gating styled versus plain.

[LOCAL_ADMISSION]:
- `cli/render` folder builds every styled terminal artifact as `AnsiDoc`: semantic roles (error/warn/path/emphasis) map to reusable `Ansi` monoid values, structure composes through `@effect/printer` combinators, and the boundary folds with `AnsiDoc.render(config)`.
- A non-TTY or `--no-color` context strips annotations with `Doc.unAnnotate` before render, so one document serves piped and interactive output, and live redraw composes the `AnsiDoc` cursor/erase directives.

[RAIL_LAW]:
- Package: `@effect/printer-ansi`
- Owns: the `Ansi` terminal-directive monoid, the `Color` palette, the `AnsiDoc = Doc<Ansi>` alias, and the escape-code terminal renderer
- Accept: `Ansi.combine`d directives attached via `Doc.annotate`, `color`/`bgColor` constructors over `Color`, `AnsiDoc` composed through the `@effect/printer` surface, `AnsiDoc.render` at the terminal edge, `Doc.unAnnotate` for the non-styled branch
- Reject: raw ANSI escape strings in application code, one `annotate` per style bit, `Doc.render` for styled output, string-switch color selection, unconditional styling that ignores TTY/`--no-color` context

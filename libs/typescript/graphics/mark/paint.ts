import { run } from '@rasm/process';
import { type Color, formatHex, formatRgb } from 'culori';
import { Array, Effect, FileSystem, Match, Order, Path, Record, Schema, Stream, UndefinedOr } from 'effect';
import { imageSize } from 'image-size';

// --- [TYPES] ---------------------------------------------------------------------------

interface Tone {
    readonly color: Color;
    readonly parts?: Readonly<Record<string, Readonly<Record<string, Color>>>>;
}

// --- [MODELS] --------------------------------------------------------------------------

const Bounds = Schema.TemplateLiteralParser([Schema.Finite, ',', Schema.Finite, ',', Schema.Finite, ',', Schema.Finite]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const paint = Effect.fnUntraced(function* (drawings: Array.NonEmptyReadonlyArray<string>, { color, parts = {} }: Tone, frame?: { readonly width: number; readonly padding?: number | undefined }) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folder = yield* fs.makeTempDirectoryScoped();
    const [stylesheet, painted] = [path.join(folder, 'tone.css'), path.join(folder, 'painted.png')];
    const sized = yield* Effect.forEach(drawings, (drawing) => Effect.map(fs.readFile(drawing), (source) => ({ drawing, canvas: imageSize(source) })), { concurrency: 'unbounded' });
    const byWidth = Order.mapInput(Order.Number, ({ canvas }: (typeof sized)[number]) => canvas.width);
    const { drawing, canvas } = UndefinedOr.match(frame, {
        onUndefined: () => Array.max(sized, byWidth),
        onDefined: ({ width, padding = 0 }) => {
            const held = sized.filter(({ canvas: { width: side } }) => side <= width - 2 * padding);
            const leftover = Order.mapInput(Order.Number, ({ canvas: { width: side } }: (typeof sized)[number]) => (width - 2 * padding) % side);
            return Array.isReadonlyArrayNonEmpty(held) ? Array.min(held, Order.combine(leftover, Order.flip(byWidth))) : Array.min(sized, byWidth);
        },
    });
    yield* fs.writeFileString(stylesheet, [`svg { color: ${formatHex(color)}; opacity: ${String(color.alpha ?? 1)} }`, ...Record.collect(parts, (part, paints) => `.${part} { ${Record.collect(paints, (property, shade) => `${property}: ${formatRgb(shade)}`).join('; ')} }`)].join('\n'));
    const { stdout: themed } = yield* run(['usvg', '--stylesheet', stylesheet, drawing, '-c']);
    const [x, y, across, down] = yield* Match.value(frame).pipe(
        Match.when({ padding: Match.number }, ({ width, padding }) =>
            run(['resvg', '--query-bbox', '--stylesheet', stylesheet, drawing]).pipe(
                Effect.flatMap(({ stdout }) => Schema.decodeUnknownEffect(Bounds)(stdout.trim())),
                Effect.map(([left, , top, , wide, , tall]) => {
                    const side = (Math.max(wide, tall) * width) / (width - 2 * padding);
                    return [left + (wide - side) / 2, top + (tall - side) / 2, side, side] as const;
                }),
            ),
        ),
        Match.when({ width: (width: number) => width >= canvas.width }, ({ width }) => {
            const side = width / Math.floor(width / canvas.width);
            return Effect.succeed([(canvas.width - side) / 2, (canvas.height - side) / 2, side, side] as const);
        }),
        Match.orElse(() => Effect.succeed([0, 0, canvas.width, canvas.height] as const)),
    );
    const document = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="${[x, y, across, down].join(' ')}" width="${String(across)}" height="${String(down)}"><image width="${String(canvas.width)}" height="${String(canvas.height)}" href="data:image/svg+xml;base64,${new TextEncoder().encode(themed).toBase64()}"/></svg>`;
    return yield* UndefinedOr.match(frame, {
        onUndefined: () => Effect.succeed(new TextEncoder().encode(document)),
        onDefined: ({ width }) => run(['resvg', '-w', String(width), '--resources-dir', path.dirname(drawing), '-', painted], { stdin: Stream.make(new TextEncoder().encode(document)) }).pipe(Effect.andThen(fs.readFile(painted))),
    });
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { paint, type Tone };

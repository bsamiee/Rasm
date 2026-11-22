import { run } from '@rasm/process';
import { Array, Effect, FileSystem, Match, Option, Order, Path, Record, Schema, UndefinedOr } from 'effect';
import { imageSize } from 'image-size';

// --- [MODELS] --------------------------------------------------------------------------

const Bounds = Schema.TemplateLiteralParser([Schema.Finite, ',', Schema.Finite, ',', Schema.Finite, ',', Schema.Finite]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const paint = Effect.fnUntraced(function* (drawings: Array.NonEmptyReadonlyArray<string>, style: string, frame?: { readonly width: number; readonly padding?: number | undefined }) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folder = yield* fs.makeTempDirectoryScoped();
    const { stylesheet, themed, framed, painted } = Record.map({ stylesheet: 'style.css', themed: 'themed.svg', framed: 'framed.svg', painted: 'painted.png' }, (name) => path.join(folder, name));
    const sized = yield* Effect.forEach(
        drawings,
        (drawing, index) => {
            const native = path.join(folder, `${String(index)}.png`);
            return run(['resvg', drawing, native]).pipe(
                Effect.andThen(fs.readFile(native)),
                Effect.map((image) => ({ drawing, canvas: imageSize(image) })),
            );
        },
        { concurrency: 'unbounded' },
    );
    const ordered = Array.sort(
        sized,
        Order.mapInput(Order.Number, ({ canvas }: (typeof sized)[number]) => canvas.width),
    );
    const { drawing, canvas } = Option.getOrElse(
        Option.flatMap(Option.fromUndefinedOr(frame), ({ width, padding = 0 }) => Array.findFirst(ordered, ({ canvas: { width: side } }) => width - 2 * padding <= 2 * side)),
        () => Array.lastNonEmpty(ordered),
    );
    yield* fs.writeFileString(stylesheet, style);
    yield* run(['usvg', '--stylesheet', stylesheet, drawing, themed]);
    const [x, y, across, down] = yield* Match.value(frame).pipe(
        Match.when({ padding: Match.number }, ({ width, padding }) =>
            run(['resvg', '--query-bbox', themed]).pipe(
                Effect.flatMap(({ stdout }) => Schema.decodeUnknownEffect(Bounds)(stdout.trim())),
                Effect.map(([left, , top, , wide, , tall]) => {
                    const side = (Math.max(wide, tall) * width) / (width - 2 * padding);
                    return [left + (wide - side) / 2, top + (tall - side) / 2, side, side] as const;
                }),
            ),
        ),
        Match.orElse(() => Effect.succeed([0, 0, canvas.width, canvas.height] as const)),
    );
    const document = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="${[x, y, across, down].join(' ')}" width="${String(across)}" height="${String(down)}"><image width="${String(canvas.width)}" height="${String(canvas.height)}" href="data:image/svg+xml;base64,${(yield* fs.readFile(themed)).toBase64()}"/></svg>`;
    return yield* UndefinedOr.match(frame, {
        onUndefined: () => Effect.succeed(new TextEncoder().encode(document)),
        onDefined: ({ width }) => fs.writeFileString(framed, document).pipe(Effect.andThen(run(['resvg', '-w', String(width), framed, painted])), Effect.andThen(fs.readFile(painted))),
    });
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { paint };

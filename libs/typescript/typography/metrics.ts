// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Config, Data, Effect, Equal, FileSystem, Option, Path, type PlatformError, Predicate, Record, Result, Schema, Struct } from 'effect';
import { type BBOX, type Font, openSync } from 'fontkit';

// --- [TYPES] ---------------------------------------------------------------------------

declare module 'fontkit' {
    interface Font {
        readonly namedVariations: Readonly<Record<string, Readonly<Record<string, number>>>>;
    }
}

interface Metrics {
    readonly postScriptName: string;
    readonly familyName: string;
    readonly subfamilyName: string;
    readonly unitsPerEm: number;
    readonly ascent: number;
    readonly descent: number;
    readonly capHeight: number;
    readonly xHeight: number;
    readonly fTop: number;
    readonly bbox: BBOX;
    readonly glyphs: Readonly<Record<keyof typeof _GLYPHS, BBOX>>;
    readonly features: readonly string[];
    readonly axes: Readonly<Record<string, NonNullable<Font['variationAxes'][string]>>>;
    readonly namedVariations: Font['namedVariations'];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _GLYPHS = { x: 0x78, capital: 0x48, f: 0x66 } as const;
const _SCAN = { depth: 3, extensions: ['.otf', '.ttf', '.ttc', '.dfont'] } as const;

// --- [ERRORS] --------------------------------------------------------------------------

type MetricsError = Data.TaggedEnum<{
    readonly fontNotFound: { readonly postScriptName: string; readonly scanned: readonly string[] };
    readonly metricsMissing: { readonly postScriptName: string; readonly fields: Array.NonEmptyReadonlyArray<'capHeight' | 'xHeight'> };
    readonly faceNotReadable: { readonly file: string; readonly cause: unknown };
    readonly faceNotInFile: { readonly file: string; readonly postScriptName: string };
}>;

const MetricsError: Data.TaggedEnum.Constructor<MetricsError> = Data.taggedEnum<MetricsError>();

// --- [MODELS] --------------------------------------------------------------------------

const _Mtime = Schema.OptionFromNullOr(Schema.Number);
const _Index = Schema.fromJsonString(
    Schema.Struct({
        roots: Schema.Record(Schema.String, _Mtime),
        faces: Schema.Record(Schema.String, Schema.Struct({ family: Schema.String, style: Schema.String, file: Schema.String, mtime: _Mtime })),
        unreadable: Schema.Array(Schema.String),
    }),
    { space: 2 },
);

// --- [FACES] ---------------------------------------------------------------------------

const roots: Effect.Effect<readonly string[], Config.ConfigError, Path.Path> = Effect.gen(function* () {
    const path = yield* Path.Path;
    const home = yield* Config.String('HOME');
    return [
        '/Library/Fonts',
        '/System/Library/Fonts',
        '/System/Library/Fonts/Supplemental',
        path.join(home, 'Library', 'Fonts'),
        path.join(home, 'Library', 'Application Support', 'Adobe', 'CoreSync', 'plugins', 'livetype', '.r'),
    ];
});

const _faces = (file: string): Effect.Effect<readonly Font[], Extract<MetricsError, { readonly _tag: 'faceNotReadable' }>> =>
    Effect.map(Effect.try({ try: () => openSync(file), catch: (cause) => MetricsError.faceNotReadable({ file, cause }) }), (opened) => ('fonts' in opened ? opened.fonts : [opened]));

const read: (file: string, postScriptName: string) => Effect.Effect<Metrics, MetricsError> = Effect.fnUntraced(function* (file: string, postScriptName: string) {
    const font = yield* Effect.fromOption(
        Array.findFirst(yield* _faces(file), (face) => face.postscriptName === postScriptName),
        () => MetricsError.faceNotInFile({ file, postScriptName }),
    );
    const os2 = Option.fromNullishOr(font['OS/2']);
    const missing = Array.filterMap(
        [
            ['capHeight', Option.flatMap(os2, (table) => Option.fromNullishOr(table.capHeight))],
            [
                'xHeight',
                Option.filter(
                    Option.flatMap(os2, (table) => Option.fromNullishOr(table.xHeight)),
                    (height) => height !== 0,
                ),
            ],
        ] as const,
        ([field, value]) => (Option.isNone(value) ? Result.succeed(field) : Result.failVoid),
    );
    yield* Array.match(missing, { onEmpty: () => Effect.void, onNonEmpty: (fields) => Effect.fail(MetricsError.metricsMissing({ postScriptName, fields })) });
    const glyphs = Record.map(_GLYPHS, (codePoint) => font.glyphForCodePoint(codePoint).bbox);
    return {
        postScriptName: font.postscriptName,
        familyName: font.familyName,
        subfamilyName: font.subfamilyName,
        unitsPerEm: font.unitsPerEm,
        ascent: font.ascent,
        descent: font.descent,
        capHeight: font.capHeight,
        xHeight: font.xHeight,
        fTop: glyphs.f.maxY / font.unitsPerEm,
        bbox: font.bbox,
        glyphs,
        features: font.availableFeatures,
        axes: Record.filter(font.variationAxes, Predicate.isNotUndefined),
        namedVariations: font.namedVariations,
    };
});

// --- [INDEX] ---------------------------------------------------------------------------

const _mtime = (file: string): Effect.Effect<Option.Option<number>, PlatformError.PlatformError, FileSystem.FileSystem> =>
    Effect.map(
        FileSystem.FileSystem.use((fs) => fs.stat(file)),
        (info) => Option.map(info.mtime, (date) => date.getTime()),
    );

const _entries: (root: string) => Effect.Effect<readonly string[], PlatformError.PlatformError, FileSystem.FileSystem | Path.Path> = Effect.fnUntraced(function* (root: string) {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    return Array.filterMap(yield* fs.readDirectory(root, { recursive: true }), (entry) =>
        entry.split(path.sep).length <= _SCAN.depth && Array.some(_SCAN.extensions, (extension) => entry.toLowerCase().endsWith(extension)) ? Result.succeed(path.join(root, entry)) : Result.failVoid,
    );
});

const _scan: (file: string) => Effect.Effect<Result.Result<ReadonlyArray<readonly [string, (typeof _Index.Type)['faces'][string]]>, string>, PlatformError.PlatformError, FileSystem.FileSystem> =
    Effect.fnUntraced(function* (file: string) {
        const opened = yield* Effect.result(_faces(file));
        const mtime = yield* _mtime(file);
        return Result.mapBoth(opened, {
            onFailure: Struct.get('file'),
            onSuccess: (faces) =>
                Array.getSomes(
                    Array.map(faces, (face) =>
                        Option.map(
                            Option.all({ postScriptName: Option.fromNullishOr(face.postscriptName), family: Option.fromNullishOr(face.familyName), style: Option.fromNullishOr(face.subfamilyName) }),
                            ({ postScriptName, family, style }) => [postScriptName, { family, style, file, mtime }] as const,
                        ),
                    ),
                ),
        });
    });

const _index: Effect.Effect<typeof _Index.Type, Config.ConfigError | PlatformError.PlatformError | Schema.SchemaError, FileSystem.FileSystem | Path.Path> = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const scanned = yield* roots;
    const stamps = Record.fromEntries(Array.zip(scanned, yield* Effect.forEach(scanned, _mtime)));
    const file = path.resolve(import.meta.dirname, '..', '..', '..', '.cache', 'typography', 'font-index.json');
    const text = yield* fs.readFileString(file).pipe(
        Effect.map(Option.some),
        Effect.catchIf(
            (error: PlatformError.PlatformError) => error.reason._tag === 'NotFound',
            () => Effect.succeedNone,
        ),
    );
    const cached = yield* Effect.transposeOption(Option.map(text, Schema.decodeEffect(_Index)));
    return yield* Option.match(
        Option.filter(cached, (candidate) => Equal.equals(candidate.roots, stamps)),
        {
            onNone: () =>
                Effect.gen(function* () {
                    const files = Array.flatten(yield* Effect.forEach(scanned, _entries));
                    const [unreadable, faces] = Array.separate(yield* Effect.forEach(files, _scan));
                    const built = { roots: stamps, faces: Record.fromEntries(Array.flatten(faces)), unreadable };
                    yield* fs.makeDirectory(path.dirname(file), { recursive: true });
                    yield* fs.writeFileString(file, yield* Schema.encodeEffect(_Index)(built));
                    return built;
                }),
            onSome: Effect.succeed,
        },
    );
});

const resolve = (postScriptName: string): Effect.Effect<string, MetricsError | Config.ConfigError | PlatformError.PlatformError | Schema.SchemaError, FileSystem.FileSystem | Path.Path> =>
    Effect.flatMap(_index, ({ roots: scanned, faces }) =>
        Effect.fromOption(Option.map(Record.get(faces, postScriptName), Struct.get('file')), () => MetricsError.fontNotFound({ postScriptName, scanned: Record.keys(scanned) })),
    );

const metrics = (
    postScriptName: string,
    size: number,
): Effect.Effect<
    Metrics & { readonly size: number; readonly x: number; readonly hCap: number; readonly f: number },
    MetricsError | Config.ConfigError | PlatformError.PlatformError | Schema.SchemaError,
    FileSystem.FileSystem | Path.Path
> =>
    Effect.map(
        Effect.flatMap(resolve(postScriptName), (file) => read(file, postScriptName)),
        (face) => ({
            ...face,
            size,
            x: (size * face.xHeight) / face.unitsPerEm,
            hCap: (size * face.capHeight) / face.unitsPerEm,
            f: size * face.fTop,
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Metrics };
export { MetricsError, metrics, read, resolve, roots };

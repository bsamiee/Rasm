// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Config, Effect, Equal, FileSystem, Option, Path, type PlatformError, Predicate, Record, Result, Schema, Struct } from 'effect';
import { type Font, openSync } from 'fontkit';

// --- [TYPES] ---------------------------------------------------------------------------

declare module 'fontkit' {
    interface Font {
        readonly namedVariations: Readonly<Record<string, Readonly<Record<string, number>>>>;
    }
}

type Metrics = (typeof Metrics)['Type'];
type Measured = (typeof Measured)['Type'];
type MetricsError = (typeof MetricsError)['Type'];

// --- [CONSTANTS] -----------------------------------------------------------------------

const _GLYPHS = { x: 0x78, capital: 0x48, f: 0x66 } as const;
const _SCAN = { depth: 3, extensions: ['.otf', '.ttf', '.ttc', '.dfont'] } as const;

// --- [ERRORS] --------------------------------------------------------------------------

const MetricsError: Schema.TaggedUnion<{
    readonly fontNotFound: Schema.TaggedStruct<'fontNotFound', { readonly postScriptName: Schema.String; readonly scanned: Schema.$Array<Schema.String> }>;
    readonly metricsMissing: Schema.TaggedStruct<
        'metricsMissing',
        { readonly postScriptName: Schema.String; readonly fields: Schema.NonEmptyArray<Schema.Literals<readonly ['capHeight', 'xHeight']>> }
    >;
    readonly faceNotReadable: Schema.TaggedStruct<'faceNotReadable', { readonly file: Schema.String; readonly cause: Schema.Defect }>;
    readonly faceNotInFile: Schema.TaggedStruct<'faceNotInFile', { readonly file: Schema.String; readonly postScriptName: Schema.String }>;
}> = Schema.TaggedUnion({
    fontNotFound: { postScriptName: Schema.String, scanned: Schema.Array(Schema.String) },
    metricsMissing: { postScriptName: Schema.String, fields: Schema.NonEmptyArray(Schema.Literals(['capHeight', 'xHeight'])) },
    faceNotReadable: { file: Schema.String, cause: Schema.Defect() },
    faceNotInFile: { file: Schema.String, postScriptName: Schema.String },
});

// --- [MODELS] --------------------------------------------------------------------------

const _Box: Schema.Struct<{ readonly minX: Schema.Number; readonly minY: Schema.Number; readonly maxX: Schema.Number; readonly maxY: Schema.Number }> = Schema.Struct({
    minX: Schema.Number,
    minY: Schema.Number,
    maxX: Schema.Number,
    maxY: Schema.Number,
});

const Metrics: Schema.Struct<{
    readonly postScriptName: Schema.String;
    readonly familyName: Schema.String;
    readonly subfamilyName: Schema.String;
    readonly unitsPerEm: Schema.Number;
    readonly ascent: Schema.Number;
    readonly descent: Schema.Number;
    readonly lineGap: Schema.Number;
    readonly underlinePosition: Schema.Number;
    readonly underlineThickness: Schema.Number;
    readonly italicAngle: Schema.Number;
    readonly capHeight: Schema.Number;
    readonly xHeight: Schema.Number;
    readonly fTop: Schema.Number;
    readonly bbox: typeof _Box;
    readonly glyphs: Schema.$Record<Schema.Literals<Array<keyof typeof _GLYPHS>>, typeof _Box>;
    readonly features: Schema.$Array<Schema.String>;
    readonly axes: Schema.$Record<Schema.String, Schema.Struct<{ readonly name: Schema.String; readonly min: Schema.Number; readonly default: Schema.Number; readonly max: Schema.Number }>>;
    readonly namedVariations: Schema.$Record<Schema.String, Schema.$Record<Schema.String, Schema.Number>>;
}> = Schema.Struct({
    postScriptName: Schema.String,
    familyName: Schema.String,
    subfamilyName: Schema.String,
    unitsPerEm: Schema.Number,
    ascent: Schema.Number,
    descent: Schema.Number,
    lineGap: Schema.Number,
    underlinePosition: Schema.Number,
    underlineThickness: Schema.Number,
    italicAngle: Schema.Number,
    capHeight: Schema.Number,
    xHeight: Schema.Number,
    fTop: Schema.Number,
    bbox: _Box,
    glyphs: Schema.Record(Schema.Literals(Struct.keys(_GLYPHS)), _Box),
    features: Schema.Array(Schema.String),
    axes: Schema.Record(Schema.String, Schema.Struct({ name: Schema.String, min: Schema.Number, default: Schema.Number, max: Schema.Number })),
    namedVariations: Schema.Record(Schema.String, Schema.Record(Schema.String, Schema.Number)),
});

const Measured: Schema.Struct<
    (typeof Metrics)['fields'] & { readonly file: Schema.String; readonly size: Schema.Number; readonly x: Schema.Number; readonly hCap: Schema.Number; readonly f: Schema.Number }
> = Schema.Struct({ ...Metrics.fields, file: Schema.String, size: Schema.Number, x: Schema.Number, hCap: Schema.Number, f: Schema.Number });

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

const _roots: Effect.Effect<readonly string[], Config.ConfigError, Path.Path> = Effect.gen(function* () {
    const path = yield* Path.Path;
    const home = yield* Config.String('HOME');
    return [
        '/Library/Fonts',
        '/System/Library/Fonts',
        '/System/Library/Fonts/Supplemental',
        path.join(home, 'Library', 'Fonts'),
        path.join(home, 'Library', 'Application Support', 'Adobe', 'CoreSync', 'plugins', 'livetype', '.r'),
        path.join(home, 'Library', 'Application Support', 'Adobe', 'CoreSync', 'plugins', 'livetype', '.w'),
    ];
});

const _faces = (file: string): Effect.Effect<readonly Font[], Extract<MetricsError, { readonly _tag: 'faceNotReadable' }>> =>
    Effect.map(Effect.try({ try: () => openSync(file), catch: (cause) => MetricsError.cases.faceNotReadable.make({ file, cause }) }), (opened) => ('fonts' in opened ? opened.fonts : [opened]));

const read: (file: string, postScriptName: string) => Effect.Effect<Metrics, MetricsError> = Effect.fnUntraced(function* (file: string, postScriptName: string) {
    const font = yield* Effect.fromOption(
        Array.findFirst(yield* _faces(file), (face) => face.postscriptName === postScriptName),
        () => MetricsError.cases.faceNotInFile.make({ file, postScriptName }),
    );
    const os2 = Option.fromNullishOr(font['OS/2']);
    const required = Record.map(
        { capHeight: Option.flatMap(os2, (table) => Option.fromNullishOr(table.capHeight)), xHeight: Option.flatMap(os2, (table) => Option.fromNullishOr(table.xHeight)) },
        Option.filter((height) => height !== 0),
    );
    const { capHeight, xHeight } = yield* Option.match(Option.all(required), {
        onSome: Effect.succeed,
        onNone: () =>
            Effect.fail(
                MetricsError.cases.metricsMissing.make({
                    postScriptName,
                    fields: Option.isSome(required.capHeight) ? ['xHeight'] : ['capHeight', ...(Option.isSome(required.xHeight) ? [] : ['xHeight' as const])],
                }),
            ),
    });
    const glyphs = Record.map(_GLYPHS, (codePoint) => font.glyphForCodePoint(codePoint).bbox);
    return {
        postScriptName: font.postscriptName,
        familyName: font.familyName,
        subfamilyName: font.subfamilyName,
        unitsPerEm: font.unitsPerEm,
        ascent: font.ascent,
        descent: font.descent,
        lineGap: font.lineGap,
        underlinePosition: font.underlinePosition,
        underlineThickness: font.underlineThickness,
        italicAngle: font.italicAngle,
        capHeight,
        xHeight,
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
        entry.split(path.sep).length <= _SCAN.depth && Array.contains(_SCAN.extensions, path.extname(entry).toLowerCase()) ? Result.succeed(path.join(root, entry)) : Result.failVoid,
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
    const scanned = yield* _roots;
    const stamps = Record.fromEntries(Array.zip(scanned, yield* Effect.forEach(scanned, _mtime)));
    const file = path.resolve(import.meta.dirname, '..', '..', '..', '.cache', 'typography', 'font-index.json');
    const text = yield* fs.readFileString(file).pipe(
        Effect.map(Option.some),
        Effect.catchIf(
            (error: PlatformError.PlatformError) => error.reason._tag === 'NotFound',
            () => Effect.succeedNone,
        ),
    );
    const cached = Option.filter(Option.flatMap(text, Schema.decodeUnknownOption(_Index)), (candidate) => Equal.equals(candidate.roots, stamps));
    if (Option.isSome(cached)) {
        return cached.value;
    }
    const files = Array.flatten(yield* Effect.forEach(scanned, _entries));
    const [unreadable, faces] = Array.separate(yield* Effect.forEach(files, _scan));
    const built = { roots: stamps, faces: Record.fromEntries(Array.flatten(faces)), unreadable };
    yield* fs.makeDirectory(path.dirname(file), { recursive: true });
    yield* fs.writeFileString(file, yield* Schema.encodeEffect(_Index)(built));
    return built;
});

const resolve = (postScriptName: string): Effect.Effect<string, MetricsError | Config.ConfigError | PlatformError.PlatformError | Schema.SchemaError, FileSystem.FileSystem | Path.Path> =>
    Effect.flatMap(_index, ({ roots: scanned, faces }) =>
        Effect.fromOption(Option.map(Record.get(faces, postScriptName), Struct.get('file')), () => MetricsError.cases.fontNotFound.make({ postScriptName, scanned: Record.keys(scanned) })),
    );

const metrics = (
    postScriptName: string,
    size: number,
): Effect.Effect<Measured, MetricsError | Config.ConfigError | PlatformError.PlatformError | Schema.SchemaError, FileSystem.FileSystem | Path.Path> =>
    Effect.flatMap(resolve(postScriptName), (file) =>
        Effect.map(read(file, postScriptName), (face) => ({
            ...face,
            file,
            size,
            x: (size * face.xHeight) / face.unitsPerEm,
            hCap: (size * face.capHeight) / face.unitsPerEm,
            f: size * face.fTop,
        })),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { Measured, Metrics, MetricsError, metrics, read, resolve };

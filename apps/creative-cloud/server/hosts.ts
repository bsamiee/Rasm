// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Config, Context, Data, Effect, FileSystem, identity, Path, type PlatformError, Record, Result, Schema, SchemaGetter, String } from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/unstable/process';
import type { NonZeroExit } from './errors.ts';
import { reply } from './osascript.ts';
import { AbsolutePath, HOSTS, type HostId, type Row } from './values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = (typeof Resolved)['Type'];

interface Info {
    readonly executable: string;
    readonly name: string;
    readonly version: string;
}

type Keys<T> = T extends unknown ? keyof T : never;

type ResolvedKey = Exclude<Keys<Resolved>, Keys<Row>>;

type Hosts = { readonly [K in HostId]: Extract<Resolved, { readonly id: K }> };

type Extras = { readonly [K in HostId]: Omit<Hosts[K], Exclude<keyof typeof _common, Keys<Row>>> };

type HostKeyError = Data.TaggedEnum<{
    readonly unresolved: { readonly host: HostId; readonly key: ResolvedKey; readonly cause: NonZeroExit | PlatformError.PlatformError | Schema.SchemaError };
    readonly missing: { readonly host: HostId; readonly key: ResolvedKey; readonly path: AbsolutePath };
}>;

type Read<A> = Effect.Effect<A, Array.NonEmptyReadonlyArray<HostKeyError>, ChildProcessSpawner.ChildProcessSpawner | FileSystem.FileSystem>;

interface Site {
    readonly home: string;
    readonly details: Info;
    readonly path: Path.Path;
}

// --- [MODELS] --------------------------------------------------------------------------

const HostKeyError: Data.TaggedEnum.Constructor<HostKeyError> = Data.taggedEnum<HostKeyError>();

const _Info: Schema.Codec<Info, string> = Schema.fromJsonString(
    Schema.Struct({ executable: Schema.NonEmptyString, name: Schema.NonEmptyString, version: Schema.NonEmptyString }).pipe(
        Schema.encodeKeys({ executable: 'CFBundleExecutable', name: 'CFBundleName', version: 'CFBundleShortVersionString' }),
    ),
);

const _common: { readonly bundleId: Schema.String; readonly bundlePath: typeof AbsolutePath; readonly version: Schema.String; readonly processName: Schema.String } = {
    bundleId: Schema.String,
    bundlePath: AbsolutePath,
    version: Schema.String,
    processName: Schema.String,
};

const Resolved: Schema.Union<
    readonly [
        Schema.Struct<typeof _common & { readonly id: Schema.Literal<'illustrator'>; readonly channel: Schema.Literal<'osascript'> }>,
        Schema.Struct<typeof _common & { readonly id: Schema.Literal<'photoshop'>; readonly channel: Schema.Literal<'socket'>; readonly port: Schema.Int }>,
        Schema.Struct<typeof _common & { readonly id: Schema.Literal<'indesign'>; readonly channel: Schema.Literal<'socket'>; readonly port: Schema.Int }>,
        Schema.Struct<
            typeof _common & {
                readonly id: Schema.Literal<'acrobat'>;
                readonly channel: Schema.Literal<'osascript'>;
                readonly prefsFolder: typeof AbsolutePath;
                readonly supportFolder: typeof AbsolutePath;
                readonly sequencesFolder: typeof AbsolutePath;
                readonly startupVolume: Schema.String;
            }
        >,
    ]
> = Schema.Union([
    Schema.Struct({ ..._common, id: Schema.Literal('illustrator'), channel: Schema.Literal('osascript') }),
    Schema.Struct({ ..._common, id: Schema.Literal('photoshop'), channel: Schema.Literal('socket'), port: Schema.Int }),
    Schema.Struct({ ..._common, id: Schema.Literal('indesign'), channel: Schema.Literal('socket'), port: Schema.Int }),
    Schema.Struct({
        ..._common,
        id: Schema.Literal('acrobat'),
        channel: Schema.Literal('osascript'),
        prefsFolder: AbsolutePath,
        supportFolder: AbsolutePath,
        sequencesFolder: AbsolutePath,
        startupVolume: Schema.String,
    }),
]);

// --- [SERVICES] ------------------------------------------------------------------------

const Hosts: Context.Service<Hosts, Hosts> = Context.Service<Hosts>('Hosts');

// --- [READS] ---------------------------------------------------------------------------

const _key = <A, R>(
    host: HostId,
    key: ResolvedKey,
    reading: Effect.Effect<A, NonZeroExit | PlatformError.PlatformError | Schema.SchemaError, R>,
): Effect.Effect<A, Array.NonEmptyArray<HostKeyError>, R> => Effect.mapError(reading, (cause) => Array.of(HostKeyError.unresolved({ host, key, cause })));

const _existing = (host: HostId, key: ResolvedKey, candidate: string): Effect.Effect<AbsolutePath, Array.NonEmptyArray<HostKeyError>, FileSystem.FileSystem> =>
    Effect.flatMap(_key(host, key, Schema.decodeEffect(AbsolutePath)(candidate)), (path) =>
        _key(
            host,
            key,
            FileSystem.FileSystem.use((fs) => fs.exists(path)),
        ).pipe(
            Effect.filterOrFail(identity, () => Array.of(HostKeyError.missing({ host, key, path }))),
            Effect.as(path),
        ),
    );

const _validated = (results: Record.ReadonlyRecord<string, Result.Result<unknown, Array.NonEmptyReadonlyArray<HostKeyError>>>): Effect.Effect<void, Array.NonEmptyArray<HostKeyError>> =>
    Effect.mapError(Effect.validate(Record.values(results), Effect.fromResult, { discard: true }), Array.flatten);

const _startupVolume = (host: HostId, path: Path.Path): Effect.Effect<string, Array.NonEmptyArray<HostKeyError>, FileSystem.FileSystem> =>
    Effect.flatMap(
        _key(
            host,
            'startupVolume',
            FileSystem.FileSystem.use((fs) =>
                Effect.flatMap(
                    fs.readDirectory('/Volumes'),
                    Effect.findFirst((name) => Effect.map(fs.realPath(path.join('/Volumes', name)), (real) => real === '/')),
                ),
            ),
        ),
        Effect.fromOption(() => Array.of(HostKeyError.missing({ host, key: 'startupVolume', path: AbsolutePath.make('/Volumes') }))),
    );

const _EXTRAS: { readonly [K in HostId]: (host: (typeof HOSTS)[K], site: Site) => Read<Extras[K]> } = {
    illustrator: Effect.succeed,
    photoshop: Effect.succeed,
    indesign: Effect.succeed,
    acrobat: (acrobat, { home, details, path }) => {
        const support = path.join(home, 'Library', 'Application Support', 'Adobe', details.name);
        return Effect.all(
            {
                prefsFolder: _existing(acrobat.id, 'prefsFolder', path.join(home, 'Library', 'Preferences', `${acrobat.bundleId}.plist`)),
                supportFolder: _existing(acrobat.id, 'supportFolder', support),
                sequencesFolder: _existing(acrobat.id, 'sequencesFolder', path.join(support, 'DC', 'Sequences')),
                startupVolume: _startupVolume(acrobat.id, path),
            },
            { mode: 'result' },
        ).pipe(
            Effect.flatMap((extras) => Effect.andThen(_validated(extras), Effect.fromResult(Result.all(extras)))),
            Effect.map((extras) => ({ ...acrobat, ...extras })),
        );
    },
};

const bundle = (host: Row): Effect.Effect<AbsolutePath, NonZeroExit | Schema.SchemaError, ChildProcessSpawner.ChildProcessSpawner> =>
    reply(ChildProcess.make('mdfind', [`kMDItemCFBundleIdentifier == '${host.bundleId}'`])).pipe(
        Effect.flatMap(
            Schema.decodeEffect(
                Schema.String.pipe(Schema.decodeTo(Schema.NonEmptyArray(AbsolutePath), { decode: SchemaGetter.transform(String.split('\n')), encode: SchemaGetter.transform(Array.join('\n')) })),
            ),
        ),
        Effect.map(Array.headNonEmpty),
    );

const info = (bundlePath: string): Effect.Effect<Info, NonZeroExit | Schema.SchemaError, ChildProcessSpawner.ChildProcessSpawner | Path.Path> =>
    Effect.flatMap(Path.Path, (path) =>
        Effect.flatMap(reply(ChildProcess.make('plutil', ['-convert', 'json', '-o', '-', path.join(bundlePath, 'Contents', 'Info.plist')])), Schema.decodeEffect(_Info)),
    );

const _resolved = Effect.fnUntraced(function* <K extends HostId>(id: K, home: string) {
    const host = HOSTS[id];
    const path = yield* Path.Path;
    const bundlePath = yield* _key(id, 'bundlePath', bundle(host));
    const details = yield* _key(id, 'version', info(bundlePath));
    const row = yield* _EXTRAS[id](host, { home, details, path });
    return { ...row, bundlePath, version: details.version, processName: details.executable };
});

const resolve: Effect.Effect<Hosts, Array.NonEmptyReadonlyArray<HostKeyError> | Config.ConfigError, ChildProcessSpawner.ChildProcessSpawner | FileSystem.FileSystem | Path.Path> = Effect.gen(
    function* () {
        const home = yield* Config.String('HOME');
        const results = yield* Effect.all(
            { illustrator: _resolved('illustrator', home), photoshop: _resolved('photoshop', home), indesign: _resolved('indesign', home), acrobat: _resolved('acrobat', home) },
            { mode: 'result' },
        );
        return yield* Effect.andThen(_validated(results), Effect.fromResult(Result.all(results)));
    },
);

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Info };
export { bundle, Hosts, info, Resolved, resolve };

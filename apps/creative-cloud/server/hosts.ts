// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Config, Context, Data, Effect, FileSystem, identity, Option, Path, type PlatformError, Record, Result, Schema, SchemaGetter, String } from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/unstable/process';
import type { BridgeError, NonZeroExit } from './errors.ts';
import { doScript, read, reply } from './osascript.ts';
import { AbsolutePath, HOSTS, type HostId, type Row } from './values.ts';

// --- [TABLE] ---------------------------------------------------------------------------

const _LIBRARY = {
    illustrator: { prefs: ['Preferences', 'Adobe Illustrator 30.9.0 Beta Settings'], support: ['Application Support', 'Adobe', 'Adobe Illustrator 30'] },
    photoshop: { prefs: ['Preferences', 'Adobe Photoshop (Beta) Settings'], support: ['Application Support', 'Adobe', 'Adobe Photoshop (Beta)'] },
    indesign: { prefs: ['Preferences', 'Adobe InDesign (Beta)'], support: ['Preferences', 'Adobe InDesign (Beta)', 'Version 21.0-ME'] },
    acrobat: { prefs: ['Preferences', 'com.adobe.Acrobat.Pro.plist'], support: ['Application Support', 'Adobe', 'Acrobat'] },
} as const satisfies Record<HostId, { readonly prefs: readonly string[]; readonly support: readonly string[] }>;

const _UXP = { pluginsFolder: ['Plugins', 'External'], registry: ['PluginsInfo', 'v1', 'PS.json'], pluginData: ['PluginsStorage', 'PHSPBETA', '27', 'External'] } as const;

const _LIVE_READ_MS = 10_000;

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = (typeof Resolved)['Type'];

type Keys<T> = T extends unknown ? keyof T : never;

type ResolvedKey = Exclude<Keys<Resolved>, Keys<Row>>;

type Hosts = { readonly [K in HostId]: Extract<Resolved, { readonly id: K }> };

type Extras = { readonly [K in HostId]: Omit<Hosts[K], Exclude<keyof typeof _common, Keys<Row>>> };

type HostKeyError = Data.TaggedEnum<{
    readonly unresolved: { readonly host: HostId; readonly key: ResolvedKey; readonly cause: BridgeError | NonZeroExit | PlatformError.PlatformError | Schema.SchemaError };
    readonly missing: { readonly host: HostId; readonly key: ResolvedKey; readonly path: AbsolutePath };
}>;

type Read<A> = Effect.Effect<A, Array.NonEmptyReadonlyArray<HostKeyError>, ChildProcessSpawner.ChildProcessSpawner | FileSystem.FileSystem>;

interface Site {
    readonly home: string;
    readonly bundlePath: AbsolutePath;
    readonly path: Path.Path;
}

// --- [MODELS] --------------------------------------------------------------------------

const HostKeyError: Data.TaggedEnum.Constructor<HostKeyError> = Data.taggedEnum<HostKeyError>();

const _common: {
    readonly bundleId: Schema.String;
    readonly processName: Schema.String;
    readonly bundlePath: typeof AbsolutePath;
    readonly version: Schema.String;
    readonly prefsFolder: typeof AbsolutePath;
    readonly supportFolder: typeof AbsolutePath;
    readonly installFolder: typeof AbsolutePath;
} = {
    bundleId: Schema.String,
    processName: Schema.String,
    bundlePath: AbsolutePath,
    version: Schema.String,
    prefsFolder: AbsolutePath,
    supportFolder: AbsolutePath,
    installFolder: AbsolutePath,
};
const _FeatureSet = Schema.Literal('righttoleft');

const Resolved: Schema.Union<
    readonly [
        Schema.Struct<typeof _common & { readonly id: Schema.Literal<'illustrator'>; readonly channel: Schema.Literal<'osascript'>; readonly onDemandModulesFolder: typeof AbsolutePath }>,
        Schema.Struct<
            typeof _common & {
                readonly id: Schema.Literal<'photoshop'>;
                readonly channel: Schema.Literal<'socket'>;
                readonly port: Schema.Int;
                readonly pluginsFolder: typeof AbsolutePath;
                readonly registry: typeof AbsolutePath;
                readonly pluginData: typeof AbsolutePath;
            }
        >,
        Schema.Struct<
            typeof _common & {
                readonly id: Schema.Literal<'indesign'>;
                readonly channel: Schema.Literal<'socket'>;
                readonly port: Schema.Int;
                readonly featureSet: Schema.OptionFromNullOr<Schema.Literal<'righttoleft'>>;
            }
        >,
        Schema.Struct<
            typeof _common & {
                readonly id: Schema.Literal<'acrobat'>;
                readonly channel: Schema.Literal<'osascript'>;
                readonly viewerVersion: Schema.OptionFromNullOr<Schema.Number>;
                readonly sequencesFolder: typeof AbsolutePath;
                readonly startupVolume: Schema.String;
            }
        >,
    ]
> = Schema.Union([
    Schema.Struct({ ..._common, id: Schema.Literal('illustrator'), channel: Schema.Literal('osascript'), onDemandModulesFolder: AbsolutePath }),
    Schema.Struct({ ..._common, id: Schema.Literal('photoshop'), channel: Schema.Literal('socket'), port: Schema.Int, pluginsFolder: AbsolutePath, registry: AbsolutePath, pluginData: AbsolutePath }),
    Schema.Struct({ ..._common, id: Schema.Literal('indesign'), channel: Schema.Literal('socket'), port: Schema.Int, featureSet: Schema.OptionFromNullOr(_FeatureSet) }),
    Schema.Struct({
        ..._common,
        id: Schema.Literal('acrobat'),
        channel: Schema.Literal('osascript'),
        viewerVersion: Schema.OptionFromNullOr(Schema.Number),
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
    reading: Effect.Effect<A, BridgeError | NonZeroExit | PlatformError.PlatformError | Schema.SchemaError, R>,
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

const _live = <T>(host: Row, statement: string, schema: Schema.Codec<T, unknown>): Effect.Effect<Option.Option<T>, BridgeError | Schema.SchemaError, ChildProcessSpawner.ChildProcessSpawner> =>
    read(host.id, host.bundleId, _LIVE_READ_MS, statement, Option.none()).pipe(
        Effect.flatMap(Schema.decodeEffect(schema)),
        Effect.asSome,
        Effect.catchTag('hostNotRunning', () => Effect.succeedNone),
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
    illustrator: (illustrator, { bundlePath, path }) =>
        Effect.map(
            _existing(illustrator.id, 'onDemandModulesFolder', path.join('/Library', 'Application Support', 'Adobe', path.basename(path.dirname(bundlePath)), 'OnDemandModules')),
            (onDemandModulesFolder) => ({ ...illustrator, onDemandModulesFolder }),
        ),
    photoshop: (photoshop, { home, path }) =>
        Effect.all(
            Record.map(_UXP, (segments, key) => _existing(photoshop.id, key, path.join(home, 'Library', 'Application Support', 'Adobe', 'UXP', ...segments))),
            { mode: 'result' },
        ).pipe(
            Effect.flatMap((folders) => Effect.andThen(_validated(folders), Effect.fromResult(Result.all(folders)))),
            Effect.map((folders) => ({ ...photoshop, ...folders })),
        ),
    indesign: (indesign) => Effect.map(_key(indesign.id, 'featureSet', _live(indesign, 'do script "app.featureSet" language javascript', _FeatureSet)), (featureSet) => ({ ...indesign, featureSet })),
    acrobat: (acrobat, { home, path }) =>
        Effect.all(
            {
                viewerVersion: _key(acrobat.id, 'viewerVersion', _live(acrobat, doScript('(function () { return JSON.stringify(app.viewerVersion); })()'), Schema.fromJsonString(Schema.Number))),
                sequencesFolder: _existing(acrobat.id, 'sequencesFolder', path.join(home, 'Library', ..._LIBRARY.acrobat.support, 'DC', 'Sequences')),
                startupVolume: _startupVolume(acrobat.id, path),
            },
            { mode: 'result' },
        ).pipe(
            Effect.flatMap((extras) => Effect.andThen(_validated(extras), Effect.fromResult(Result.all(extras)))),
            Effect.map((extras) => ({ ...acrobat, ...extras })),
        ),
};

const _resolved = Effect.fnUntraced(function* <K extends HostId>(id: K, home: string) {
    const host = HOSTS[id];
    const path = yield* Path.Path;
    const bundlePath = yield* _key(
        id,
        'bundlePath',
        reply(ChildProcess.make('mdfind', [`kMDItemCFBundleIdentifier == '${host.bundleId}'`])).pipe(
            Effect.flatMap(
                Schema.decodeEffect(
                    Schema.String.pipe(Schema.decodeTo(Schema.NonEmptyArray(AbsolutePath), { decode: SchemaGetter.transform(String.split('\n')), encode: SchemaGetter.transform(Array.join('\n')) })),
                ),
            ),
            Effect.map(Array.headNonEmpty),
        ),
    );
    const results = yield* Effect.all(
        {
            version: _key(
                id,
                'version',
                Effect.flatMap(
                    reply(ChildProcess.make('/usr/libexec/PlistBuddy', ['-c', 'Print :CFBundleShortVersionString', path.join(bundlePath, 'Contents', 'Info.plist')])),
                    Schema.decodeEffect(Schema.NonEmptyString),
                ),
            ),
            prefsFolder: _existing(id, 'prefsFolder', path.join(home, 'Library', ..._LIBRARY[id].prefs)),
            supportFolder: _existing(id, 'supportFolder', path.join(home, 'Library', ..._LIBRARY[id].support)),
            installFolder: _existing(id, 'installFolder', path.dirname(bundlePath)),
            row: _EXTRAS[id](host, { home, bundlePath, path }),
        },
        { mode: 'result' },
    );
    const { row, ...common } = yield* Effect.andThen(_validated(results), Effect.fromResult(Result.all(results)));
    return { ...row, bundlePath, ...common };
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

export { Hosts, Resolved, resolve };

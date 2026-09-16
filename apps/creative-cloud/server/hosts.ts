// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Config, Context, Data, Effect, FileSystem, Option, Path, Schema, SchemaGetter, String, Struct } from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/unstable/process';
import type { BridgeError } from './errors.ts';
import { doScript, read, reply } from './osascript.ts';
import { AbsolutePath, type HostId } from './values.ts';

// --- [TABLE] ---------------------------------------------------------------------------

const HOSTS = {
    illustrator: {
        id: 'illustrator',
        bundleId: 'com.adobe.illustratorBeta',
        processName: 'Adobe Illustrator',
        channel: 'osascript',
        prefs: ['Library', 'Preferences', 'Adobe Illustrator 30.9.0 Beta Settings'],
        support: ['Library', 'Application Support', 'Adobe', 'Adobe Illustrator 30'],
    },
    photoshop: {
        id: 'photoshop',
        bundleId: 'com.adobe.Photoshop',
        processName: 'Adobe Photoshop 2026',
        channel: 'socket',
        port: 39_217,
        prefs: ['Library', 'Preferences', 'Adobe Photoshop (Beta) Settings'],
        support: ['Library', 'Application Support', 'Adobe', 'Adobe Photoshop (Beta)'],
    },
    indesign: {
        id: 'indesign',
        bundleId: 'com.adobe.InDesign',
        processName: 'Adobe InDesign 2026 (Beta)',
        channel: 'socket',
        port: 39_218,
        prefs: ['Library', 'Preferences', 'Adobe InDesign (Beta)'],
        support: ['Library', 'Preferences', 'Adobe InDesign (Beta)', 'Version 21.0-ME'],
    },
    acrobat: {
        id: 'acrobat',
        bundleId: 'com.adobe.Acrobat.Pro',
        processName: 'AdobeAcrobat',
        channel: 'osascript',
        prefs: ['Library', 'Preferences', 'com.adobe.Acrobat.Pro.plist'],
        support: ['Library', 'Application Support', 'Adobe', 'Acrobat'],
    },
} as const;

const _LIVE_READ_MS = 10_000;

// --- [TYPES] ---------------------------------------------------------------------------

type Row = (typeof HOSTS)[HostId];

interface Common {
    readonly bundleId: string;
    readonly processName: string;
    readonly bundlePath: AbsolutePath;
    readonly version: string;
    readonly prefsFolder: AbsolutePath;
    readonly supportFolder: AbsolutePath;
    readonly installFolder: AbsolutePath;
}

type Resolved =
    | (Common & { readonly id: 'illustrator'; readonly channel: 'osascript'; readonly onDemandModulesFolder: AbsolutePath })
    | (Common & {
          readonly id: 'photoshop';
          readonly channel: 'socket';
          readonly port: number;
          readonly pluginsFolder: AbsolutePath;
          readonly registry: AbsolutePath;
          readonly pluginData: AbsolutePath;
      })
    | (Common & { readonly id: 'indesign'; readonly channel: 'socket'; readonly port: number; readonly featureSet: Option.Option<'righttoleft'> })
    | (Common & { readonly id: 'acrobat'; readonly channel: 'osascript'; readonly viewerVersion: Option.Option<number> });

type Keys<T> = T extends unknown ? keyof T : never;

type ResolvedKey = Exclude<Keys<Resolved>, Keys<Row>>;

type HostKeyError = Data.TaggedEnum<{
    readonly unresolved: { readonly host: HostId; readonly key: ResolvedKey; readonly cause: BridgeError | Schema.SchemaError };
}>;

type Hosts = Readonly<Record<HostId, Resolved>>;

// --- [MODELS] --------------------------------------------------------------------------

const HostKeyError: Data.TaggedEnum.Constructor<HostKeyError> = Data.taggedEnum<HostKeyError>();

const _common = {
    bundleId: Schema.String,
    processName: Schema.String,
    bundlePath: AbsolutePath,
    version: Schema.String,
    prefsFolder: AbsolutePath,
    supportFolder: AbsolutePath,
    installFolder: AbsolutePath,
};
const _FeatureSet = Schema.Literal('righttoleft');

const Resolved: Schema.Codec<Resolved, unknown> = Schema.Union([
    Schema.Struct({ ..._common, id: Schema.Literal('illustrator'), channel: Schema.Literal('osascript'), onDemandModulesFolder: AbsolutePath }),
    Schema.Struct({ ..._common, id: Schema.Literal('photoshop'), channel: Schema.Literal('socket'), port: Schema.Int, pluginsFolder: AbsolutePath, registry: AbsolutePath, pluginData: AbsolutePath }),
    Schema.Struct({ ..._common, id: Schema.Literal('indesign'), channel: Schema.Literal('socket'), port: Schema.Int, featureSet: Schema.OptionFromNullOr(_FeatureSet) }),
    Schema.Struct({ ..._common, id: Schema.Literal('acrobat'), channel: Schema.Literal('osascript'), viewerVersion: Schema.OptionFromNullOr(Schema.Number) }),
]);

// --- [SERVICES] ------------------------------------------------------------------------

const Hosts: Context.Service<Hosts, Hosts> = Context.Service<Hosts>('Hosts');

// --- [READS] ---------------------------------------------------------------------------

const _existing = Schema.decodeEffect(
    AbsolutePath.pipe(
        Schema.decodeTo(AbsolutePath, {
            decode: SchemaGetter.checkEffect((path) => Effect.orDie(FileSystem.FileSystem.use((fs) => fs.exists(path)))),
            encode: SchemaGetter.passthroughSupertype(),
        }),
    ),
);

const _bundles = Schema.decodeEffect(
    Schema.String.pipe(Schema.decodeTo(Schema.NonEmptyArray(AbsolutePath), { decode: SchemaGetter.transform(String.split('\n')), encode: SchemaGetter.transform(Array.join('\n')) })),
);

const _key = <A, R>(host: HostId, key: ResolvedKey, reading: Effect.Effect<A, BridgeError | Schema.SchemaError, R>): Effect.Effect<A, HostKeyError, R> =>
    Effect.mapError(reading, (cause) => HostKeyError.unresolved({ host, key, cause }));

const _live = <T>(host: Row, statement: string, schema: Schema.Codec<T, unknown>): Effect.Effect<Option.Option<T>, BridgeError | Schema.SchemaError, ChildProcessSpawner.ChildProcessSpawner> =>
    read(host.id, host.bundleId, _LIVE_READ_MS, statement, Option.none()).pipe(
        Effect.flatMap(Schema.decodeEffect(schema)),
        Effect.asSome,
        Effect.catchTag('hostNotRunning', () => Effect.succeedNone),
    );

const _resolved = Effect.fnUntraced(function* <H extends Row, K extends Record<string, unknown>>(
    host: H,
    home: string,
    keys: (bundlePath: AbsolutePath) => Effect.Effect<K, HostKeyError, ChildProcessSpawner.ChildProcessSpawner | FileSystem.FileSystem>,
) {
    const path = yield* Path.Path;
    const bundlePath = yield* _key(
        host.id,
        'bundlePath',
        reply(host.id, ChildProcess.make('mdfind', [`kMDItemCFBundleIdentifier == '${host.bundleId}'`])).pipe(Effect.flatMap(_bundles), Effect.map(Array.headNonEmpty)),
    );
    const plist = path.join(bundlePath, 'Contents', 'Info.plist');
    const [common, extra] = yield* Effect.all([
        Effect.all({
            version: _key(
                host.id,
                'version',
                Effect.flatMap(reply(host.id, ChildProcess.make('/usr/libexec/PlistBuddy', ['-c', 'Print :CFBundleShortVersionString', plist])), Schema.decodeEffect(Schema.NonEmptyString)),
            ),
            prefsFolder: _key(host.id, 'prefsFolder', _existing(path.join(home, ...host.prefs))),
            supportFolder: _key(host.id, 'supportFolder', _existing(path.join(home, ...host.support))),
            installFolder: _key(host.id, 'installFolder', _existing(path.dirname(bundlePath))),
        }),
        keys(bundlePath),
    ]);
    return { ...Struct.omit(host, ['prefs', 'support']), bundlePath, ...common, ...extra };
});

const resolve: Effect.Effect<Hosts, HostKeyError | Config.ConfigError, ChildProcessSpawner.ChildProcessSpawner | FileSystem.FileSystem | Path.Path> = Effect.gen(function* () {
    const home = yield* Config.String('HOME');
    const path = yield* Path.Path;
    const uxp = path.join(home, 'Library', 'Application Support', 'Adobe', 'UXP');
    return yield* Effect.all({
        illustrator: _resolved(HOSTS.illustrator, home, (bundlePath) =>
            Effect.all({
                onDemandModulesFolder: _key(
                    'illustrator',
                    'onDemandModulesFolder',
                    _existing(path.join('/Library', 'Application Support', 'Adobe', path.basename(path.dirname(bundlePath)), 'OnDemandModules')),
                ),
            }),
        ),
        photoshop: _resolved(HOSTS.photoshop, home, () =>
            Effect.all({
                pluginsFolder: _key('photoshop', 'pluginsFolder', _existing(path.join(uxp, 'Plugins', 'External'))),
                registry: _key('photoshop', 'registry', _existing(path.join(uxp, 'PluginsInfo', 'v1', 'PS.json'))),
                pluginData: _key('photoshop', 'pluginData', _existing(path.join(uxp, 'PluginsStorage', 'PHSPBETA', '27', 'External'))),
            }),
        ),
        indesign: _resolved(HOSTS.indesign, home, () =>
            Effect.all({ featureSet: _key('indesign', 'featureSet', _live(HOSTS.indesign, 'do script "app.featureSet" language javascript', _FeatureSet)) }),
        ),
        acrobat: _resolved(HOSTS.acrobat, home, () =>
            Effect.all({
                viewerVersion: _key('acrobat', 'viewerVersion', _live(HOSTS.acrobat, doScript('(function () { return JSON.stringify(app.viewerVersion); })()'), Schema.fromJsonString(Schema.Number))),
            }),
        ),
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { HOSTS, Hosts, Resolved, resolve };

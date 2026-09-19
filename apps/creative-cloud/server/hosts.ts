// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Config, Context, Data, Effect, FileSystem, flow, identity, Layer, Option, Order, Path, type PlatformError, pipe, Record, Result, Schema, String, Struct } from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/unstable/process';
import { BridgeError, exited, type NonZeroExit } from './errors.ts';
import { reply } from './osascript.ts';
import { AbsolutePath, HOSTS, HostId, type Row, VOLUMES } from './values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Channel = (typeof Channel)['Type'];
type Info = (typeof _Info)['Type'];
type Discovered = (typeof Bundle)['Type'];
type Unresolved = BridgeError | PlatformError.PlatformError | Schema.SchemaError;

interface Folders {
    readonly prefsFolder: AbsolutePath;
    readonly supportFolder: AbsolutePath;
    readonly sequencesFolder: AbsolutePath;
    readonly startupVolume: string;
}

type Host<K extends HostId> = Discovered & { readonly id: K } & (K extends 'acrobat' ? Folders : unknown);

type Hosts = { readonly [K in HostId]: Result.Result<Host<K>, Array.NonEmptyReadonlyArray<HostKeyError>> };

type ResolvedKey = Exclude<keyof Host<'acrobat'>, keyof Row>;

type HostKeyError = Data.TaggedEnum<{
    readonly unresolved: { readonly host: HostId; readonly key: ResolvedKey; readonly cause: Unresolved };
    readonly missing: { readonly host: HostId; readonly key: ResolvedKey; readonly path: AbsolutePath };
}>;

interface Finding extends Struct.Lambda {
    <R extends Row>(host: R): Effect.Effect<Discovered & { readonly id: R['id'] }, Array.NonEmptyArray<HostKeyError>, ChildProcessSpawner.ChildProcessSpawner | Path.Path>;
    readonly '~lambda.out': this['~lambda.in'] extends { readonly id: infer Id extends HostId }
        ? Effect.Effect<Discovered & { readonly id: Id }, Array.NonEmptyArray<HostKeyError>, ChildProcessSpawner.ChildProcessSpawner | Path.Path>
        : never;
}

// --- [MODELS] --------------------------------------------------------------------------

const HostKeyError: Data.TaggedEnum.Constructor<HostKeyError> = Data.taggedEnum<HostKeyError>();

const Channel: Schema.Literals<readonly ['release', 'prerelease', 'beta']> = Schema.Literals(['release', 'prerelease', 'beta']);

const _Info: Schema.Struct<{ readonly bundleId: Schema.NonEmptyString; readonly executable: Schema.NonEmptyString; readonly name: Schema.NonEmptyString; readonly version: Schema.NonEmptyString }> =
    Schema.Struct({ bundleId: Schema.NonEmptyString, executable: Schema.NonEmptyString, name: Schema.NonEmptyString, version: Schema.NonEmptyString });

const _Plist = Schema.fromJsonString(_Info.pipe(Schema.encodeKeys({ bundleId: 'CFBundleIdentifier', executable: 'CFBundleExecutable', name: 'CFBundleName', version: 'CFBundleShortVersionString' })));

const Bundle: Schema.Struct<(typeof _Info)['fields'] & { readonly id: typeof HostId; readonly channel: typeof Channel; readonly bundlePath: typeof AbsolutePath }> = Schema.Struct({
    ..._Info.fields,
    id: HostId,
    channel: Channel,
    bundlePath: AbsolutePath,
});

const _suffix = Schema.decodeUnknownOption(Schema.Literals(['', 'prerelease', 'beta']).transform(Channel.literals));

const _marked = Schema.decodeUnknownOption(Schema.Literals(['Beta', 'Prerelease', 'Prerelease-Debug']).transform(['beta', 'prerelease', 'prerelease']));

const _named = Schema.decodeUnknownOption(Schema.TemplateLiteralParser([Schema.String, ' (', Schema.String, ')']));

const _parts = Schema.decodeUnknownOption(Schema.Array(Schema.NumberFromString));

const _wanted: Config.Config<Option.Option<Channel>> = Config.option(Config.Literals(Channel.literals, 'CREATIVE_CLOUD_CHANNEL'));

// --- [SERVICES] ------------------------------------------------------------------------

const Hosts: Context.Service<Hosts, Hosts> = Context.Service<Hosts>('Hosts');

// --- [DISCOVERY] -----------------------------------------------------------------------

const _newest: Order.Order<Pick<Discovered, 'version'>> = Order.mapInput(
    Order.make((self: readonly number[], that: readonly number[]) =>
        Option.getOrElse(
            Array.findFirst(Array.zipWith(self, that, Order.Number), (ordering) => ordering !== 0),
            () => Order.Number(self.length, that.length),
        ),
    ),
    (row) => Option.getOrElse(_parts(String.split(row.version, '.')), () => []),
);

const info = (bundlePath: string): Effect.Effect<Info, NonZeroExit | Schema.SchemaError, ChildProcessSpawner.ChildProcessSpawner | Path.Path> =>
    Effect.flatMap(Path.Path, (path) =>
        Effect.flatMap(reply(ChildProcess.make('plutil', ['-convert', 'json', '-o', '-', path.join(bundlePath, 'Contents', 'Info.plist')])), Schema.decodeEffect(_Plist)),
    );

const _identified = (host: Row, bundlePath: AbsolutePath, details: Info): Option.Option<Discovered> =>
    pipe(
        Option.liftPredicate(String.toLowerCase(details.bundleId), String.startsWith(String.toLowerCase(host.bundleId))),
        Option.flatMap(flow(String.slice(host.bundleId.length), _suffix)),
        Option.map((suffix) => ({
            ...details,
            bundlePath,
            id: host.id,
            channel: Option.getOrElse(
                Option.flatMap(_named(details.name), ([, , marked]) => _marked(marked)),
                () => suffix,
            ),
        })),
    );

const discoverOn: (host: Pick<Row, 'id'>, wanted: Option.Option<Channel>) => Effect.Effect<Discovered, Array.NonEmptyArray<HostKeyError>, ChildProcessSpawner.ChildProcessSpawner | Path.Path> =
    Effect.fnUntraced(function* (host: Pick<Row, 'id'>, wanted: Option.Option<Channel>) {
        const owner = HOSTS[host.id];
        const listing = yield* reply(ChildProcess.make('mdfind', [`kMDItemCFBundleIdentifier == '${owner.bundleId}*'c && kMDItemContentType == 'com.apple.application-bundle'`])).pipe(
            Effect.mapError((error) => Array.of(HostKeyError.unresolved({ host: owner.id, key: 'bundlePath', cause: exited(owner.id)(error) }))),
        );
        const [failures, candidates] = yield* Effect.partition(
            Array.filter(String.linesIterator(listing), String.isNonEmpty),
            (line) =>
                info(line).pipe(
                    Effect.catchTag('nonZeroExit', flow(exited(owner.id), Effect.fail)),
                    Effect.map((details) => _identified(owner, AbsolutePath.make(line), details)),
                    Effect.mapError((cause) => HostKeyError.unresolved({ host: owner.id, key: 'bundlePath', cause })),
                ),
            { concurrency: 'unbounded' },
        );
        const found = Array.filter(Array.getSomes(candidates), (row) => Option.isNone(wanted) || Option.contains(wanted, row.channel));
        if (Array.isArrayNonEmpty(found)) {
            return Array.max(found, _newest);
        }
        if (Array.isArrayNonEmpty(failures)) {
            return yield* Effect.fail(failures);
        }
        return yield* Effect.fail(
            Array.of(HostKeyError.unresolved({ host: owner.id, key: 'bundlePath', cause: BridgeError.cases.hostNotInstalled.make({ host: owner.id, unresolved: ['bundlePath'] }) })),
        );
    });

const discover = (host: Pick<Row, 'id'>): Effect.Effect<Discovered, Array.NonEmptyArray<HostKeyError> | Config.ConfigError, ChildProcessSpawner.ChildProcessSpawner | Path.Path> =>
    Effect.flatMap(_wanted, (channel) => discoverOn(host, channel));

const bundle: (host: Pick<Row, 'id'>) => Effect.Effect<AbsolutePath, Array.NonEmptyArray<HostKeyError> | Config.ConfigError, ChildProcessSpawner.ChildProcessSpawner | Path.Path> = flow(
    discover,
    Effect.map(Struct.get('bundlePath')),
);

const installed = <K extends HostId>(id: K, resolved: Hosts[K]): Effect.Effect<Host<K>, BridgeError> =>
    Effect.mapError(Effect.fromResult(resolved), (unresolved) => BridgeError.cases.hostNotInstalled.make({ host: id, unresolved: Array.map(unresolved, Struct.get('key')) }));

// --- [READS] ---------------------------------------------------------------------------

const _key = <A, R>(host: HostId, key: ResolvedKey, reading: Effect.Effect<A, Unresolved, R>): Effect.Effect<A, HostKeyError, R> =>
    Effect.mapError(reading, (cause) => HostKeyError.unresolved({ host, key, cause }));

const _existing = Effect.fnUntraced(function* (host: HostId, key: ResolvedKey, candidate: string) {
    const path = yield* _key(host, key, Schema.decodeEffect(AbsolutePath)(candidate));
    yield* Effect.filterOrFail(
        _key(
            host,
            key,
            FileSystem.FileSystem.use((fs) => fs.exists(path)),
        ),
        identity,
        () => HostKeyError.missing({ host, key, path }),
    );
    return path;
});

const _startupVolume = Effect.fnUntraced(function* (host: HostId, path: Path.Path) {
    const fs = yield* FileSystem.FileSystem;
    const names = yield* _key(host, 'startupVolume', fs.readDirectory(VOLUMES));
    const found = yield* _key(
        host,
        'startupVolume',
        Effect.findFirst(names, (name) => Effect.map(fs.realPath(path.join(VOLUMES, name)), (real) => real === path.sep)),
    );
    return yield* Effect.fromOption(found, () => HostKeyError.missing({ host, key: 'startupVolume', path: AbsolutePath.make(VOLUMES) }));
});

const _folders = (acrobat: Discovered, home: string, path: Path.Path): Effect.Effect<Folders, Array.NonEmptyArray<HostKeyError>, FileSystem.FileSystem> => {
    const support = path.join(home, 'Library', 'Application Support', 'Adobe', acrobat.name);
    return Effect.flatMap(
        Effect.all(
            {
                prefsFolder: _existing(acrobat.id, 'prefsFolder', path.join(home, 'Library', 'Preferences', `${acrobat.bundleId}.plist`)),
                supportFolder: _existing(acrobat.id, 'supportFolder', support),
                sequencesFolder: _key(acrobat.id, 'sequencesFolder', Schema.decodeEffect(AbsolutePath)(path.join(support, 'DC', 'Sequences'))),
                startupVolume: _startupVolume(acrobat.id, path),
            },
            { mode: 'result' },
        ),
        (folders) => Effect.andThen(Effect.validate(Record.values(folders), Effect.fromResult, { discard: true }), Effect.orDie(Effect.fromResult(Result.all(folders)))),
    );
};

const resolve: Effect.Effect<Hosts, Config.ConfigError, ChildProcessSpawner.ChildProcessSpawner | FileSystem.FileSystem | Path.Path> = Effect.gen(function* () {
    const [home, wanted, path] = yield* Effect.all([Config.String('HOME'), _wanted, Path.Path]);
    const found = Struct.lambda<Finding>((host) => Effect.map(discoverOn(host, wanted), (row) => ({ ...row, id: host.id })));
    const rows = Struct.map(HOSTS, found);
    return yield* Effect.all(
        { ...rows, acrobat: Effect.flatMap(rows.acrobat, (acrobat) => Effect.map(_folders(acrobat, home, path), (folders) => ({ ...acrobat, ...folders }))) },
        { mode: 'result', concurrency: 'unbounded' },
    );
});

// --- [LAYER] ---------------------------------------------------------------------------

const layer: Layer.Layer<Hosts, Config.ConfigError, ChildProcessSpawner.ChildProcessSpawner | FileSystem.FileSystem | Path.Path> = Layer.effect(Hosts, resolve);

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Discovered, Host, HostKeyError, Info };
export { Bundle, bundle, Channel, discover, discoverOn, Hosts, info, installed, layer, resolve };

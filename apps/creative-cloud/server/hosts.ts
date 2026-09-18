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
    <R extends Row>(host: R): Effect.Effect<Discovered & { readonly id: R['id'] }, Array.NonEmptyArray<HostKeyError>>;
    readonly '~lambda.out': this['~lambda.in'] extends { readonly id: infer Id extends HostId } ? Effect.Effect<Discovered & { readonly id: Id }, Array.NonEmptyArray<HostKeyError>> : never;
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

const _identified = (bundlePath: AbsolutePath, details: Info): Option.Option<Discovered> =>
    Array.findFirst(Record.values(HOSTS), (row) =>
        pipe(
            Option.liftPredicate(String.toLowerCase(details.bundleId), String.startsWith(String.toLowerCase(row.bundleId))),
            Option.flatMap(flow(String.slice(row.bundleId.length), _suffix)),
            Option.map((suffix) => ({
                ...details,
                bundlePath,
                id: row.id,
                channel: Option.getOrElse(
                    Option.flatMap(_named(details.name), ([, , marked]) => _marked(marked)),
                    () => suffix,
                ),
            })),
        ),
    );

const _installed: Effect.Effect<readonly Discovered[], NonZeroExit | Schema.SchemaError, ChildProcessSpawner.ChildProcessSpawner | Path.Path> = Effect.gen(function* () {
    const listing = yield* reply(
        ChildProcess.make('mdfind', [
            `(${Array.join(
                Array.map(Record.values(HOSTS), (row) => `kMDItemCFBundleIdentifier == '${row.bundleId}*'c`),
                ' || ',
            )}) && kMDItemContentType == 'com.apple.application-bundle'`,
        ]),
    );
    const bundles = yield* Effect.forEach(
        Array.filter(String.linesIterator(listing), String.isNonEmpty),
        (line) => Effect.map(info(line), (details) => _identified(AbsolutePath.make(line), details)),
        {
            concurrency: 'unbounded',
        },
    );
    return Array.getSomes(bundles);
});

const _located = <R>(
    host: Pick<Row, 'id'>,
    listing: Effect.Effect<readonly Discovered[], NonZeroExit | Schema.SchemaError, R>,
    wanted: Option.Option<Channel>,
): Effect.Effect<Discovered, Unresolved, R> =>
    listing.pipe(
        Effect.catchTag('nonZeroExit', flow(exited(host.id), Effect.fail)),
        Effect.map(Array.filter((row) => row.id === host.id && (Option.isNone(wanted) || Option.contains(wanted, row.channel)))),
        Effect.flatMap((rows) =>
            Effect.fromOption(Array.match(rows, { onEmpty: Option.none, onNonEmpty: (found) => Option.some(Array.max(found, _newest)) }), () =>
                BridgeError.cases.hostNotInstalled.make({ host: host.id, unresolved: ['bundlePath'] }),
            ),
        ),
    );

const discoverOn = (host: Pick<Row, 'id'>, channel: Option.Option<Channel>): Effect.Effect<Discovered, Unresolved, ChildProcessSpawner.ChildProcessSpawner | Path.Path> =>
    _located(host, _installed, channel);

const discover = (host: Pick<Row, 'id'>): Effect.Effect<Discovered, Unresolved | Config.ConfigError, ChildProcessSpawner.ChildProcessSpawner | Path.Path> =>
    Effect.flatMap(_wanted, (channel) => discoverOn(host, channel));

const bundle: (host: Pick<Row, 'id'>) => Effect.Effect<AbsolutePath, Unresolved | Config.ConfigError, ChildProcessSpawner.ChildProcessSpawner | Path.Path> = flow(
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
    const [home, wanted, path, bundles] = yield* Effect.all([Config.String('HOME'), _wanted, Path.Path, Effect.result(_installed)]);
    const found = Struct.lambda<Finding>((host) =>
        Effect.map(Effect.mapError(_key(host.id, 'bundlePath', _located(host, Effect.fromResult(bundles), wanted)), Array.of), (row) => ({ ...row, id: host.id })),
    );
    const rows = Struct.map(HOSTS, found);
    return yield* Effect.all({ ...rows, acrobat: Effect.flatMap(rows.acrobat, (acrobat) => Effect.map(_folders(acrobat, home, path), (folders) => ({ ...acrobat, ...folders }))) }, { mode: 'result' });
});

// --- [LAYER] ---------------------------------------------------------------------------

const layer: Layer.Layer<Hosts, Config.ConfigError, ChildProcessSpawner.ChildProcessSpawner | FileSystem.FileSystem | Path.Path> = Layer.effect(Hosts, resolve);

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Discovered, Host, HostKeyError, Info };
export { Bundle, bundle, Channel, discover, discoverOn, Hosts, info, installed, layer, resolve };

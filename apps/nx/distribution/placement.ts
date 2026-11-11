import { constants } from 'node:os';
import { pid, platform } from 'node:process';
import { ToolError } from '@rasm/nx-devkit/process';
import ast from '@xml-tools/ast';
import xml, { type DocumentCstNode } from '@xml-tools/parser';
import { Array, Config, Effect, FileSystem, Formatter, Option, Path, type PlatformError, pipe, Record, Schema, UndefinedOr } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import { applyEdits, modify, parse } from 'jsonc-parser';

// --- [TYPES] ---------------------------------------------------------------------------

interface Placement {
    readonly folder: string;
    readonly names: readonly string[];
    readonly text: string;
    readonly dry: boolean;
    readonly removeData: boolean;
    readonly variables: Option.Option<readonly [certificate: string, password: string]>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const OWNER_ONLY = 0o600;
const RENAME_SWAP = 0x2;
const RENAME_EXCHANGE = 0x2;
const AT_FDCWD = -100;
const SUPPORT: Partial<Record<NodeJS.Platform, (path: Path.Path) => Config.Config<string>>> = {
    darwin: (path) => Config.map(Config.String('HOME'), (home) => path.join(home, 'Library', 'Application Support')),
    win32: () => Config.String('APPDATA'),
};
const EXCHANGE: Partial<Record<NodeJS.Platform, (ffi: typeof import('node:ffi'), from: string, to: string) => number>> = {
    darwin: ({ dlopen, getInt32 }, from, to) => {
        using handle = dlopen(null, { renamex_np: { arguments: ['string', 'string', 'u32'], return: 'i32' }, __error: { return: 'pointer' } });
        return handle.functions.renamex_np(from, to, RENAME_SWAP) === 0 ? 0 : getInt32(handle.functions.__error());
    },
    linux: ({ dlopen, getInt32 }, from, to) => {
        using handle = dlopen(null, { renameat2: { arguments: ['i32', 'string', 'i32', 'string', 'u32'], return: 'i32' }, __errno_location: { return: 'pointer' } });
        return handle.functions.renameat2(AT_FDCWD, from, AT_FDCWD, to, RENAME_EXCHANGE) === 0 ? 0 : getInt32(handle.functions.__errno_location());
    },
};

// --- [MODELS] --------------------------------------------------------------------------

const Plugin = Schema.fromJsonString(Schema.Struct({ id: Schema.String, name: Schema.String, version: Schema.String, host: Schema.Struct({ app: Schema.String, minVersion: Schema.String }) }));
const Registry = Schema.Struct({ plugins: Schema.Array(Schema.StructWithRest(Schema.Struct({ pluginId: Schema.String }), [Schema.Record(Schema.String, Schema.Unknown)])) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const adobe = Effect.gen(function* () {
    const path = yield* Path.Path;
    const located = yield* Effect.mapError(Effect.fromNullishOr(SUPPORT[platform]), ToolError.of(platform, 'has no per-user application-support folder this install reads'));
    return path.join(yield* Effect.mapError(located(path), ToolError.of(platform, 'states no per-user application-support folder')), 'Adobe');
});

const registered = (text: string, row: { readonly pluginId: string }) =>
    Effect.mapBoth(Schema.decodeUnknownEffect(Registry)(parse(text)), {
        onFailure: ToolError.of('UXP registry', 'holds no plugins array'),
        onSuccess: ({ plugins }) => {
            const held = plugins.findIndex((existing) => existing.pluginId === row.pluginId);
            return { held: Option.fromUndefinedOr(plugins[held]), text: applyEdits(text, modify(text, ['plugins', held === -1 ? plugins.length : held], row, {})) };
        },
    });

const swapped = Effect.fnUntraced(function* <E, R>(target: string, fill: (staged: string) => Effect.Effect<void, E, R>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const container = path.dirname(target);
    const staged = path.join(path.dirname(container), `${path.basename(target)}.${pid}`);
    yield* fs.makeDirectory(container, { recursive: true });
    yield* Effect.onError(fill(staged), () => Effect.ignore(fs.remove(staged, { recursive: true, force: true })));
    const aside = `${staged}.old`;
    yield* (yield* fs.exists(target))
        ? UndefinedOr.match(EXCHANGE[platform], {
              onUndefined: () =>
                  Effect.andThen(
                      fs.rename(target, aside),
                      Effect.onError(fs.rename(staged, target), () => Effect.ignore(fs.rename(aside, target))),
                  ),
              onDefined: Effect.fnUntraced(function* (exchange) {
                  const errno = exchange(yield* Effect.promise(() => import('node:ffi')), staged, target);
                  const named = Option.getOrElse(
                      Option.map(
                          Record.findFirst(constants.errno, (code) => code === errno),
                          ([name]) => name,
                      ),
                      () => `errno ${errno}`,
                  );
                  return yield* errno === 0 ? Effect.void : ToolError.make({ subject: target, detail: `does not exchange with ${staged}: ${named}` });
              }),
          })
        : fs.rename(staged, target);
    yield* Effect.forEach([staged, aside], (at) => fs.remove(at, { recursive: true, force: true }), { discard: true });
});

const uxp = Effect.fnUntraced(function* ({ folder, names, text, dry, removeData }: Placement) {
    const [fs, path, root] = yield* Effect.all([FileSystem.FileSystem, Path.Path, adobe]);
    const { id, name, version, host } = yield* Effect.mapError(Schema.decodeEffect(Plugin)(text), ToolError.of(path.join(folder, 'manifest.json'), 'states no id, name, version, host.app, and host.minVersion'));
    const external = path.join(root, 'UXP', 'Plugins', 'External');
    const target = path.join(external, `${id}_${version}`);
    const registry = path.join(root, 'UXP', 'PluginsInfo', 'v1', `${host.app}.json`);
    const row = { hostMinVersion: host.minVersion, name, path: `$localPlugins/External/${id}_${version}`, pluginId: id, status: 'enabled', type: 'uxp', versionString: version };
    const before = yield* Effect.orElseSucceed(fs.readFileString(registry), () => '{"plugins":[]}');
    const { held, text: after } = yield* registered(before, row);
    const listed = (at: string) =>
        Effect.map(
            Effect.orElseSucceed(fs.readDirectory(at), Array.empty<string>),
            Array.map((entry) => path.join(at, entry)),
        );
    const stale = (yield* listed(external)).filter((at) => path.basename(at).startsWith(`${id}_`) && at !== target);
    const releases = removeData ? (yield* Effect.forEach(yield* listed(path.join(root, 'UXP', 'PluginsStorage')), listed)).flat() : [];
    const data = yield* Effect.filter(
        releases.map((release) => path.join(release, 'External', id)),
        (at) => fs.exists(at),
    );
    const mode = yield* Effect.orElseSucceed(
        Effect.map(fs.stat(registry), (info) => info.mode),
        () => OWNER_ONLY,
    );
    const verb = (did: string, would: string) => (dry ? would : did);
    yield* dry
        ? Effect.void
        : Effect.all(
              [
                  swapped(target, (staged) => Effect.forEach(names, (file) => Effect.andThen(fs.makeDirectory(path.dirname(path.join(staged, file)), { recursive: true }), fs.copyFile(path.join(folder, file), path.join(staged, file))), { discard: true })),
                  fs.makeDirectory(path.dirname(registry), { recursive: true }),
                  fs.writeFileString(`${registry}.${pid}`, after, { mode }),
                  fs.rename(`${registry}.${pid}`, registry),
                  Effect.forEach([...stale, ...data], (at) => fs.remove(at, { recursive: true }), { discard: true }),
              ],
              { discard: true },
          );
    yield* Effect.forEach(
        [
            `${verb('Replaced', 'Would replace')} ${target} with the build in ${folder}`,
            ...(before === after ? [] : [`${registry} row for ${id}: ${Formatter.formatJson(Option.getOrNull(held))} -> ${Formatter.formatJson(row)}`]),
            ...stale.map((at) => `${verb('Removed', 'Would remove')} ${at}, an older version of ${id}`),
            ...data.map((at) => `${verb('Removed', 'Would remove')} plug-in data ${at}`),
            `A running ${host.app} host loads this build at its next launch, since it reads ${registry} at launch`,
        ],
        (line) => Effect.logInfo(line),
        { discard: true },
    );
});

const cep = Effect.fnUntraced(function* ({ folder, text, dry }: Placement, sealed: (staged: string) => Effect.Effect<void, ToolError | PlatformError.PlatformError, FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner>) {
    const [path, root] = yield* Effect.all([Path.Path, adobe]);
    const manifest = path.join(folder, 'CSXS/manifest.xml');
    const { cst, tokenVector } = xml.parse(text);
    const document = yield* pipe(
        Option.liftPredicate(cst, (node): node is DocumentCstNode => node.name === 'document'),
        Option.flatMap((node) => Option.fromNullOr(ast.buildAst(node, tokenVector).rootElement)),
        Effect.fromOption,
        Effect.mapError(ToolError.of(manifest, 'holds no root element')),
    );
    const attribute = (element: typeof document, key: string) =>
        Option.flatMap(
            Array.findFirst(element.attributes, (held) => held.key === key),
            ({ value }) => Option.fromNullOr(value),
        );
    const descendants = (element: typeof document): readonly (typeof document)[] => [element, ...element.subElements.flatMap(descendants)];
    const id = yield* Effect.mapError(Effect.fromOption(attribute(document, 'ExtensionBundleId')), ToolError.of(manifest, 'states no ExtensionBundleId'));
    const hosts = Array.getSomes(descendants(document).map((element) => (element.name === 'Host' ? attribute(element, 'Name') : Option.none())));
    const target = path.join(root, 'CEP', 'extensions', id);
    yield* dry ? Effect.void : swapped(target, sealed);
    yield* Effect.forEach([`${dry ? 'Would replace' : 'Replaced'} ${target} with the signed build in ${folder}`, `A running ${hosts.join(' or ')} host loads this build at its next launch`], (line) => Effect.logInfo(line), { discard: true });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { cep, type Placement, uxp };

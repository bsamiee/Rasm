import { constants, openAsBlob } from 'node:fs';
import { homedir } from 'node:os';
import { basename, dirname, join, matchesGlob } from 'node:path';
import { arch, platform } from 'node:process';
import { patch } from '@decimalturn/toml-patch';
import type { ExecutorContext } from '@nx/devkit';
import { globWithWorkspaceContext, quoteShellArg } from '@nx/devkit/internal';
import { written } from '@rasm/nx-devkit/executor';
import { PLATFORM } from '@rasm/nx-devkit/plugin';
import { printed, ToolError } from '@rasm/nx-devkit/process';
import ast from '@xml-tools/ast';
import xml, { type DocumentCstNode } from '@xml-tools/parser';
import { Array, Config, Effect, FileSystem, flow, Option, Path, type PlatformError, pipe, Record, Result, Schema, SemVer, Stream, String } from 'effect';
import { Ini, Toml, Yaml, Zip } from 'effect/encoding';
import { HttpClient, HttpClientRequest, HttpClientResponse } from 'effect/http';
import type { ChildProcessSpawner } from 'effect/process';
import { applyEdits, modify, parse } from 'jsonc-parser';

// --- [TYPES] ---------------------------------------------------------------------------

type Services = FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner;
type Step<A = void> = Effect.Effect<A, ToolError | PlatformError.PlatformError, Services>;
type Stored<A = void> = Effect.Effect<A, Error, Services | HttpClient.HttpClient>;

interface Fields {
    readonly name: string;
    readonly version?: string;
    readonly foreign?: boolean;
    readonly folders?: readonly string[];
    readonly references?: readonly { readonly name: string; readonly enabled: boolean }[];
    readonly installed?: boolean;
    readonly implicit?: readonly string[];
}

interface Found {
    readonly host: Host;
    readonly at: string;
    readonly name: string;
    readonly version: string;
    readonly foreign: boolean;
    readonly text: string;
    readonly names: readonly string[];
}

interface Host {
    readonly manifest: string;
    readonly technology: string;
    readonly archive?: string;
    readonly platforms?: readonly string[];
    readonly standalone?: true;
    readonly held?: (descriptor: string) => Effect.Effect<boolean, never, Services>;
    readonly operations?: Partial<Record<'check' | 'test' | 'pack' | 'install', { readonly command: string; readonly options: Readonly<Record<string, unknown>> }>>;
    readonly read: (text: string, file: string) => Effect.Effect<Option.Option<Fields>, ToolError>;
    readonly write?: (text: string, version: string) => Effect.Effect<string, ToolError>;
    readonly build?: { readonly executor: string; readonly command: string; readonly lock?: string; readonly runtime?: readonly string[]; readonly pack?: (folder: Found, file: string, timestampAuthority: string) => Step };
    readonly install?: { readonly command: string; readonly probe?: Array.NonEmptyReadonlyArray<string>; readonly platforms?: readonly string[]; readonly unpacked?: true; readonly run: (found: Found) => Step };
    readonly publish?: { readonly store: string; readonly command: string; readonly held: (found: Found, store: string) => Stored<boolean>; readonly push: (found: Found, store: string) => Stored };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const EXTENSION = 'blender_manifest.toml';
const SIGNER = 'ZXPSignCmd';
const PYTHON = ['run', '--no-project', 'python', '-c'] as const;
const LOCATED = "shutil.which('blender_ext.py', os.F_OK)";
const BUILDER = [...PYTHON, `import os, runpy, shutil, sys; sys.argv[0] = ${LOCATED}; runpy.run_path(sys.argv[0], run_name='__main__')`] as const;
const BLENDER: Partial<Record<NodeJS.Platform, string>> = { darwin: 'macos', linux: 'linux', win32: 'windows' };
const UNREAL: Partial<Record<NodeJS.Platform, { readonly name: string; readonly settings: readonly string[] }>> = {
    darwin: { name: 'Mac', settings: ['Library', 'Application Support', 'Epic'] },
    linux: { name: 'Linux', settings: ['.config', 'Epic'] },
};
const ENGINE_PATH = /[/\\]/u;
const ADOBE: Partial<Record<NodeJS.Platform, Config.Config<string>>> = {
    darwin: Config.succeed(join(homedir(), 'Library', 'Application Support', 'Adobe')),
    win32: Config.map(Config.String('APPDATA'), (roaming) => join(roaming, 'Adobe')),
};

// --- [MODELS] --------------------------------------------------------------------------

const Platforms = Schema.optionalKey(Schema.Array(Schema.String));
const Extension = Schema.Struct({ schema_version: Schema.String, id: Schema.String, version: Schema.String, platforms: Platforms, build: Schema.optionalKey(Schema.Struct({ generated: Schema.optionalKey(Schema.Struct({ platforms: Platforms })) })) });
const Listed = Schema.Struct({ data: Schema.Array(Schema.Struct({ id: Schema.String, version: Schema.String, platforms: Platforms })) });
const Yak = Schema.Struct({ name: Schema.String, version: Schema.String, authors: Schema.Array(Schema.String), description: Schema.String });
const Pushed = Schema.Struct({ distributions: Schema.Array(Schema.Struct({ filename: Schema.String })) });
const Uxp = Schema.fromJsonString(Schema.Struct({ manifestVersion: Schema.Finite, id: Schema.String, name: Schema.String, version: Schema.String, host: Schema.Struct({ app: Schema.String, minVersion: Schema.String }) }));
const Registry = Schema.Struct({ plugins: Schema.Array(Schema.StructWithRest(Schema.Struct({ pluginId: Schema.String, path: Schema.optionalKey(Schema.String) }), [Schema.Record(Schema.String, Schema.Unknown)])) });
const Cep = Schema.Struct({ ExtensionBundleId: Schema.String, ExtensionBundleVersion: Schema.String });
const References = Schema.optionalKey(Schema.Array(Schema.Struct({ Name: Schema.String, Enabled: Schema.optionalKey(Schema.Boolean) })));
const Project = Schema.fromJsonString(Schema.Struct({ FileVersion: Schema.Int, Plugins: References, AdditionalPluginDirectories: Schema.optionalKey(Schema.Array(Schema.String)) }));
const Plugin = Schema.fromJsonString(
    Schema.Struct({ FileVersion: Schema.Int, Version: Schema.optionalKey(Schema.Int), VersionName: Schema.optionalKey(Schema.String), Installed: Schema.optionalKey(Schema.Boolean), Plugins: References, nx: Schema.optionalKey(Schema.Struct({ implicitDependencies: Schema.Array(Schema.String) })) }),
);
const Engined = Schema.fromJsonString(Schema.Struct({ EngineAssociation: Schema.optionalKey(Schema.String) }));
const Launcher = Schema.fromJsonString(Schema.Struct({ InstallationList: Schema.Array(Schema.Struct({ AppName: Schema.String, InstallLocation: Schema.String })) }));
const Registered = Schema.Struct({ Installations: Schema.Record(Schema.String, Schema.String) });
const Built = Schema.fromJsonString(Schema.Struct({ MajorVersion: Schema.Int, MinorVersion: Schema.Int }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const exact = (subject: string, version: string) =>
    Effect.fromResult(SemVer.parse(version)).pipe(
        Effect.filterOrFail((parsed) => !(SemVer.isPrerelease(parsed) || SemVer.hasBuild(parsed))),
        Effect.mapError(ToolError.of(subject, `takes a major.minor.patch version alone, which ${version} is not`)),
    );

const extension = (text: string) => {
    const { cst, tokenVector } = xml.parse(text);
    return pipe(
        Option.liftPredicate(cst, (node): node is DocumentCstNode => node.name === 'document'),
        Option.flatMap((document) => Option.fromNullOr(ast.buildAst(document, tokenVector).rootElement)),
    );
};

const adobe = Effect.mapError(Effect.flatten(Effect.fromNullishOr(ADOBE[platform])), ToolError.of(platform, 'holds no per-user Adobe folder'));

const copied = Effect.fnUntraced(function* ({ at, names }: Found, to: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    yield* Effect.forEach(names, (name) => Effect.andThen(fs.makeDirectory(path.dirname(path.join(to, name)), { recursive: true }), fs.copyFile(path.join(at, name), path.join(to, name))), { discard: true });
});

const zipped = Effect.fnUntraced(function* (folder: string, names: readonly string[], file: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const entries = yield* Effect.forEach(names.toSorted(), (name) => Effect.map(fs.stat(path.join(folder, name)), (info) => ({ name, info })));
    yield* Stream.fromIterable(entries.filter(({ info }) => info.type === 'File')).pipe(
        Stream.map(({ name, info }) => Zip.fileStream(name.split(path.sep).join('/'), fs.stream(path.join(folder, name)), { mode: info.mode % (constants.S_IRWXU + constants.S_IRWXG + constants.S_IRWXO + 1) })),
        Stream.pipeThroughChannel(Zip.encode()),
        Stream.run(fs.sink(file)),
        Effect.mapError(ToolError.of(file, 'does not write')),
    );
});

const entry = (file: string, name: string) =>
    Effect.flatMap(FileSystem.FileSystem, (fs) =>
        fs.stream(file).pipe(
            Stream.pipeThroughChannel(Zip.decode()),
            Stream.mapEffect((member) =>
                matchesGlob(member.header.path, name)
                    ? pipe(
                          Stream.decodeText(member.body),
                          Stream.mkString,
                          Effect.map((text) => ({ at: member.header.path, text })),
                          Effect.asSome,
                      )
                    : Effect.succeedNone,
            ),
            Stream.filter(Option.isSome),
            Stream.runHead,
            Effect.mapBoth({ onFailure: ToolError.of(file, 'does not read as a zip archive'), onSuccess: Option.flatten }),
        ),
    );

const engine = Effect.fnUntraced(function* (descriptor: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const host = yield* Effect.mapError(Effect.fromNullishOr(UNREAL[platform]), ToolError.of(platform, `is no supported Unreal Engine host; supported: ${Object.keys(UNREAL).join(', ')}`));
    const settings = path.join(homedir(), ...host.settings);
    const declared = yield* Effect.flatMap(fs.readFileString(descriptor), Schema.decodeEffect(Engined));
    const named = Option.filter(Option.fromUndefinedOr(declared.EngineAssociation), String.isNonEmpty);
    const read = (at: readonly string[]) => Effect.option(fs.readFileString(path.join(settings, ...at)));
    const [registered, launched] = yield* Effect.all([read(['UnrealEngine', 'Install.ini']), read(['UnrealEngineLauncher', 'LauncherInstalled.dat'])]);
    const above = Array.unfold(path.dirname(descriptor), (at) => (path.dirname(at) === at ? Option.none() : Option.some([at, path.dirname(at)] as const)));
    const native = yield* Effect.filter(above, (at) => fs.exists(path.join(at, 'Engine', 'Build', 'Build.version')));
    const root = yield* Effect.fromOption(
        Option.firstSomeOf([
            Option.flatMap(named, (key) => Option.liftPredicate(path.resolve(path.dirname(descriptor), key), () => ENGINE_PATH.test(key))),
            Option.match(named, { onNone: () => Array.head(native), onSome: () => Option.none() }),
            Option.flatMap(Option.all([named, Option.flatMap(registered, (text) => Schema.decodeUnknownOption(Registered)(Ini.parse(text)))]), ([key, { Installations }]) => Record.get(Installations, key)),
            Option.flatMap(Option.all([named, Option.flatMap(launched, (text) => Schema.decodeOption(Launcher)(text))]), ([key, { InstallationList }]) =>
                Option.map(
                    Array.findFirst(InstallationList, ({ AppName }) => AppName === `UE_${key}`),
                    ({ InstallLocation }) => InstallLocation,
                ),
            ),
        ]),
        () =>
            ToolError.make({
                subject: descriptor,
                detail: Option.match(named, { onNone: () => 'names no engine, and no engine tree encloses it', onSome: (key) => `names engine "${key}", which the engine tree around it, Install.ini, and LauncherInstalled.dat under ${settings} do not list` }),
            }),
    );
    const { MajorVersion, MinorVersion } = yield* Effect.flatMap(fs.readFileString(path.join(root, 'Engine', 'Build', 'Build.version')), Schema.decodeEffect(Built));
    return { root, platform: host.name, version: `${MajorVersion}.${MinorVersion}`, binaries: path.join(root, 'Engine', 'Binaries', host.name), scripts: path.join(root, 'Engine', 'Build', 'BatchFiles') };
});

const located = Effect.fnUntraced(function* (context: ExecutorContext, start: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { project, node, selected, outputs, files } = yield* written(context, start);
    const rows = HOSTS.filter(({ technology }) => node.data.metadata?.technologies?.includes(technology) === true);
    const sources = Array.isReadonlyArrayNonEmpty(selected) ? files : (yield* Effect.promise(() => globWithWorkspaceContext(context.root, [path.join(node.data.root, '**')]))).map((file) => path.join(context.root, file));
    const stated = (host: Host, at: string, text: string, file: string, names: readonly string[]) =>
        Effect.map(
            host.read(text, file),
            Option.flatMap(({ name, version, foreign = false }) => Option.map(Option.fromUndefinedOr(version), (known): Found => ({ host, at, name, version: known, foreign, text, names }))),
        );
    const packages = yield* Effect.forEach(
        files.flatMap((file) => rows.filter(({ archive }) => archive !== undefined && file.endsWith(archive)).map((host) => ({ file, host }))),
        ({ file, host }) => Effect.flatMap(entry(file, `{,*/}${host.manifest}`), Option.match({ onNone: () => Effect.succeedNone, onSome: ({ at, text }) => stated(host, file, text, path.join(file, at), []) })),
    );
    const under = (folder: string) => sources.filter((other) => other.startsWith(`${folder}${path.sep}`)).map((other) => path.relative(folder, other));
    const folders = yield* Effect.forEach(
        sources.flatMap((file) => rows.filter(({ build, install }) => build?.pack !== undefined || install?.unpacked === true).map((host) => ({ host, file, folder: path.join(file, path.relative(host.manifest, '.')) }))).filter(({ host, file, folder }) => matchesGlob(path.relative(folder, file), host.manifest)),
        ({ host, file, folder }) => Effect.flatMap(fs.readFileString(file), (text) => stated(host, folder, text, file, under(folder))),
    );
    return { project, outputs, packages: Array.getSomes(packages), folders: Array.getSomes(folders) };
});

// --- [HOSTS] ---------------------------------------------------------------------------

const HOSTS: readonly Host[] = [
    {
        manifest: EXTENSION,
        technology: 'blender',
        archive: '.zip',
        read: (text, file) =>
            Effect.fromResult(Toml.parse(text)).pipe(
                Effect.flatMap(Schema.decodeUnknownEffect(Extension)),
                Effect.mapBoth({
                    onFailure: ToolError.of(file, 'does not decode'),
                    onSuccess: ({ id, version, platforms, build }) => Option.some({ name: id, version, foreign: Option.exists(Option.fromUndefinedOr(build?.generated?.platforms ?? platforms), (built) => !built.includes(`${BLENDER[platform]}-${arch}`)) }),
                }),
            ),
        write: (text, version) => Effect.mapBoth(Effect.fromResult(Toml.parse(text)), { onFailure: ToolError.of(EXTENSION, 'does not parse as TOML'), onSuccess: (document) => patch(text, { ...document, version }) }),
        build: {
            executor: 'extension-build',
            command: 'blender_ext.py build',
            lock: 'uv.lock',
            runtime: ['uv --version', ['uv', ...PYTHON, `import hashlib, os, shutil; print(hashlib.file_digest(open(${LOCATED}, 'rb'), 'sha256').hexdigest())`].map(quoteShellArg).join(' ')],
        },
        install: {
            command: 'blender --command extension install-file',
            probe: ['blender', '--version'],
            run: ({ at, name }) =>
                Effect.flatMap(printed('blender', ['--command', 'extension', 'install-file', '--repo', 'user_default', '--enable', at]), (stdout) =>
                    new RegExp(`^STATUS (Installed|Reinstalled) "${RegExp.escape(name)}"$`, 'mu').test(stdout) ? Effect.void : ToolError.make({ subject: at, detail: `blender exited 0 without installing it: ${stdout.trim()}` }),
                ),
        },
        publish: {
            store: 'https://extensions.blender.org',
            command: 'POST /api/v1/extensions/<id>/versions/upload/',
            held: Effect.fnUntraced(function* ({ name, version, text }, store) {
                const { platforms, build } = yield* Effect.flatMap(Effect.fromResult(Toml.parse(text)), Schema.decodeUnknownEffect(Extension));
                const { data } = yield* HttpClient.get(`${store}/api/v1/extensions/`).pipe(Effect.flatMap(HttpClientResponse.filterStatusOk), Effect.flatMap(HttpClientResponse.schemaBodyJson(Listed)));
                return data.some((listed) => listed.id === name && listed.version === version && Array.difference(build?.generated?.platforms ?? platforms ?? [], listed.platforms ?? []).length === 0);
            }),
            push: Effect.fnUntraced(function* ({ at, name }, store) {
                const [token, archive] = yield* Effect.all([Config.Redacted('BLENDER_EXTENSIONS_TOKEN'), Effect.tryPromise({ try: () => openAsBlob(at), catch: ToolError.of(at, 'does not open') })]);
                const request = HttpClientRequest.post(`${store}/api/v1/extensions/${name}/versions/upload/`).pipe(HttpClientRequest.bearerToken(token), HttpClientRequest.bodyFormDataRecord({ version_file: new File([archive], basename(at)) }));
                yield* Effect.flatMap(HttpClient.execute(request), HttpClientResponse.matchStatus({ 404: () => Effect.fail(ToolError.make({ subject: name, detail: `is not listed on ${store}, upload its first version there once` })), orElse: HttpClientResponse.filterStatusOk }));
            }),
        },
    },
    {
        manifest: 'manifest.yml',
        technology: 'yak',
        archive: '.yak',
        read: (text) => Effect.succeed(Option.map(Option.flatMap(Option.liftThrowable(Yaml.parse)(text), Schema.decodeUnknownOption(Yak)), ({ name, version }) => ({ name, version }))),
        build: {
            executor: 'package',
            command: 'yak build',
            pack: ({ at }, file) =>
                Effect.andThen(
                    Effect.flatMap(FileSystem.FileSystem, (fs) => fs.copy(at, dirname(file))),
                    Effect.asVoid(printed('yak', ['build'], { cwd: dirname(file) })),
                ),
        },
        install: { command: 'yak install', platforms: ['darwin', 'win32'], run: ({ at }) => Effect.asVoid(printed('yak', ['install', at])) },
        publish: {
            store: 'https://yak.rhino3d.com',
            command: 'yak push',
            held: ({ at, name, version }, store) =>
                Effect.flatMap(
                    HttpClient.get(`${store}/versions/${name}/${version}`),
                    HttpClientResponse.matchStatus({
                        404: () => Effect.succeed(false),
                        orElse: (response) => Effect.map(Effect.flatMap(HttpClientResponse.filterStatusOk(response), HttpClientResponse.schemaBodyJson(Pushed)), ({ distributions }) => distributions.some(({ filename }) => filename.toLowerCase() === basename(at).toLowerCase())),
                    }),
                ),
            push: ({ at }, store) => Effect.asVoid(printed('yak', ['push', at, '--source', store])),
        },
    },
    {
        manifest: 'manifest.json',
        technology: 'uxp',
        archive: '.ccx',
        read: (text) => Effect.succeed(Option.map(Schema.decodeOption(Uxp)(text), ({ id, version }) => ({ name: id, version }))),
        write: (text, version) => Effect.as(exact('manifest.json', version), applyEdits(text, modify(text, ['version'], version, {}))),
        build: { executor: 'package', command: 'a zip archive named .ccx', pack: ({ at, names }, file) => zipped(at, names, file) },
        install: {
            command: "a copy into the per-user UXP plug-in folder and the plug-in's row in its host's PluginsInfo registry",
            platforms: Object.keys(ADOBE),
            unpacked: true,
            run: Effect.fnUntraced(function* (found) {
                const [fs, path, root] = yield* Effect.all([FileSystem.FileSystem, Path.Path, adobe]);
                const { id, name, version, host } = yield* Effect.mapError(Schema.decodeEffect(Uxp)(found.text), ToolError.of(found.at, 'holds no UXP manifest'));
                const [registry, local, placed] = [path.join(root, 'UXP', 'PluginsInfo', 'v1', `${host.app}.json`), (at: string) => at.replace('$localPlugins', path.join(root, 'UXP', 'Plugins')), `$localPlugins/External/${id}_${version}`];
                const before = yield* fs.readFileString(registry).pipe(Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed('{"plugins":[]}')));
                const rows = (yield* Effect.mapError(Schema.decodeUnknownEffect(Registry)(parse(before)), ToolError.of(registry, 'holds no plugins array'))).plugins;
                const held = rows.findIndex(({ pluginId }) => pluginId === id);
                const row = { ...rows[held], hostMinVersion: host.minVersion, name, path: placed, pluginId: id, status: 'enabled', type: 'uxp', versionString: version };
                yield* Effect.forEach(Array.dedupe([...Array.fromNullishOr(rows[held]?.path), placed]), (at) => fs.remove(local(at), { recursive: true, force: true }), { discard: true });
                yield* copied(found, local(placed));
                yield* fs.makeDirectory(path.dirname(registry), { recursive: true });
                yield* fs.writeFileString(registry, applyEdits(before, modify(before, ['plugins', held === -1 ? rows.length : held], row, {})));
            }),
        },
    },
    {
        manifest: 'CSXS/manifest.xml',
        technology: 'cep',
        archive: '.zxp',
        platforms: Object.keys(ADOBE),
        read: (text) =>
            Effect.succeed(
                pipe(
                    extension(text),
                    Option.flatMap((root) => Schema.decodeUnknownOption(Cep)(Record.fromEntries(Array.filterMap(root.attributes, ({ key, value }) => (key === null ? Result.failVoid : Result.succeed([key, value] as const)))))),
                    Option.map(({ ExtensionBundleId, ExtensionBundleVersion }) => ({ name: ExtensionBundleId, version: ExtensionBundleVersion })),
                ),
            ),
        write: (text, version) =>
            Effect.andThen(
                exact('CSXS/manifest.xml', version),
                pipe(
                    extension(text),
                    Option.flatMap((root) => Array.findFirst(root.attributes, ({ key }) => key === 'ExtensionBundleVersion')),
                    Option.flatMap(({ syntax }) => Option.fromUndefinedOr(syntax.value)),
                    Effect.fromOption,
                    Effect.mapBoth({ onFailure: ToolError.of('CSXS/manifest.xml', 'states no ExtensionBundleVersion'), onSuccess: (value) => `${text.slice(0, value.startOffset + 1)}${version}${text.slice(value.endOffset)}` }),
                ),
            ),
        build: {
            executor: 'package',
            command: `${SIGNER} -selfSignedCert, then -sign`,
            pack: Effect.fnUntraced(function* (found, file, timestampAuthority) {
                const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
                const stage = yield* fs.makeTempDirectoryScoped();
                const [staged, certificate] = [path.join(stage, found.name), path.join(stage, 'certificate.p12')];
                yield* copied(found, staged);
                yield* printed(SIGNER, ['-selfSignedCert', '', '', '', found.name, found.name, certificate]);
                yield* printed(SIGNER, ['-sign', staged, file, certificate, found.name, '-tsa', timestampAuthority]);
            }, Effect.scoped),
        },
        install: {
            command: 'an unpack of the signed package into the per-user CEP extensions folder',
            run: Effect.fnUntraced(function* ({ at, name }) {
                const [fs, path, root] = yield* Effect.all([FileSystem.FileSystem, Path.Path, adobe]);
                const target = path.join(root, 'CEP', 'extensions', name);
                yield* fs.remove(target, { recursive: true, force: true });
                yield* fs.stream(at).pipe(
                    Stream.pipeThroughChannel(Zip.decode()),
                    Stream.filter(({ header }) => header.type === 'File'),
                    Stream.mapEffect(({ header, body }) => Effect.andThen(fs.makeDirectory(path.dirname(path.join(target, header.path)), { recursive: true }), Stream.run(body, fs.sink(path.join(target, header.path))))),
                    Stream.runDrain,
                    Effect.mapError(ToolError.of(at, 'does not unpack')),
                );
            }),
        },
    },
    {
        manifest: '*.uproject',
        technology: 'unreal',
        platforms: Object.keys(UNREAL),
        standalone: true,
        read: (text, file) =>
            Effect.mapBoth(Schema.decodeEffect(Project)(text), {
                onFailure: ToolError.of(file, 'does not decode'),
                onSuccess: ({ Plugins, AdditionalPluginDirectories }) => Option.some({ name: basename(file, '.uproject'), folders: ['Plugins', ...(AdditionalPluginDirectories ?? [])], references: (Plugins ?? []).map(({ Name, Enabled }) => ({ name: Name, enabled: Enabled === true })) }),
            }),
        held: flow(engine, Effect.isSuccess),
        build: { executor: 'unreal-build', command: 'UnrealBuildTool' },
        operations: { check: { command: 'UnrealBuildTool -StaticAnalyzer=Default -StaticAnalyzerProjectOnly -DisableUnity -NoPCH', options: { operation: 'check' } } },
    },
    {
        manifest: '*.uplugin',
        technology: 'unreal',
        archive: '.zip',
        platforms: Object.keys(UNREAL),
        standalone: true,
        read: (text, file) =>
            Effect.mapBoth(Schema.decodeEffect(Plugin)(text), {
                onFailure: ToolError.of(file, 'does not decode'),
                onSuccess: ({ VersionName, Installed, Plugins, nx }) =>
                    Option.some({
                        name: basename(file, '.uplugin'),
                        ...(VersionName !== undefined && { version: VersionName }),
                        ...(nx !== undefined && { implicit: nx.implicitDependencies }),
                        installed: Installed === true,
                        references: (Plugins ?? []).map(({ Name, Enabled }) => ({ name: Name, enabled: Enabled === true })),
                    }),
            }),
        write: (text, version) =>
            Effect.mapBoth(Schema.decodeEffect(Plugin)(text), {
                onFailure: ToolError.of('uplugin', 'does not decode'),
                onSuccess: ({ Version }) => applyEdits(text, [...modify(text, ['VersionName'], version, {}), ...modify(text, ['Version'], (Version ?? 0) + 1, {})]),
            }),
        build: { executor: 'unreal-build', command: 'AutomationTool BuildPlugin -StrictIncludes', runtime: [PLATFORM.runtime] },
        operations: {
            test: { command: 'UnrealEditor-Cmd -ExecCmds="Automation RunTests <plug-in name>;Quit" -nullrhi -unattended in each project loading the plug-in, then a read of each test state from the exported report', options: { operation: 'test' } },
            pack: { command: 'the store checks, then a zip of each BuildPlugin package and a zip of the tracked sources per engine, the form Fab takes', options: { operation: 'pack' } },
            install: { command: 'a copy of the BuildPlugin package into the Plugins folder of each project enabling the plug-in outside its folders', options: { operation: 'install' } },
        },
    },
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { BUILDER, EXTENSION, engine, type Fields, type Found, HOSTS, type Host, located, zipped };

import { glob } from 'node:fs/promises';
import { hostname } from 'node:os';
import { basename, matchesGlob } from 'node:path';
import { pid } from 'node:process';
import { patch } from '@decimalturn/toml-patch';
import { type ExecutorContext, getOutputsForTargetAndConfiguration } from '@nx/devkit';
import { getDependencyConfigs, globWithWorkspaceContext, workspaceDataDirectory } from '@nx/devkit/internal';
import { ancestors, line } from '@rasm/nx-devkit/inference';
import { printed, ran, ToolError } from '@rasm/nx-devkit/process';
import ast from '@xml-tools/ast';
import xml, { type DocumentCstNode } from '@xml-tools/parser';
import { Array, Config, Effect, FileSystem, Match, Option, Path, type PlatformError, pipe, Record, Redacted, Schema, SemVer, Stream } from 'effect';
import { Base64, Toml, Xml, Yaml, Zip } from 'effect/encoding';
import { FetchHttpClient, HttpClient, HttpClientResponse } from 'effect/http';
import type { ChildProcessSpawner } from 'effect/process';
import { applyEdits, modify } from 'jsonc-parser';
import { cep, type Placement, uxp } from '#/plugins/placement';

// --- [TYPES] ---------------------------------------------------------------------------

interface Fields {
    readonly name: string;
    readonly version?: string;
    readonly platforms: readonly string[];
    readonly folders?: readonly string[];
    readonly references?: readonly { readonly name: string; readonly enabled: boolean }[];
    readonly installed?: boolean;
}

interface Package extends Fields {
    readonly version: string;
    readonly host: Host;
    readonly file: string;
    readonly root: string;
}

interface Publish {
    readonly dryRun: boolean;
    readonly registry: Option.Option<string>;
}

interface Host {
    readonly manifest: string;
    readonly archive?: string;
    readonly technology: string;
    readonly platforms: Partial<Record<NodeJS.Platform, string>>;
    readonly standalone?: true;
    readonly read: (text: string, file: string) => Effect.Effect<Option.Option<Fields>, ToolError>;
    readonly write?: (text: string, version: string) => Effect.Effect<string, ToolError>;
    readonly build?: {
        readonly executor: string;
        readonly command: string;
        readonly variables?: readonly [certificate: string, password: string];
        readonly pack?: (folder: string, names: readonly string[], file: string, timestampAuthority: Option.Option<string>) => Effect.Effect<void, ToolError | PlatformError.PlatformError, FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner>;
    };
    readonly install?: { readonly tool: string; readonly command: string; readonly run: (bundle: Package) => Effect.Effect<Option.Option<string>, ToolError | PlatformError.PlatformError, ChildProcessSpawner.ChildProcessSpawner> };
    readonly place?: (placement: Placement) => Effect.Effect<void, ToolError | PlatformError.PlatformError, FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner>;
    readonly publish?: { readonly command: string; readonly run: (bundle: Package, options: Publish) => Effect.Effect<void, ToolError | PlatformError.PlatformError, FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner> };
    readonly upgrade?: { readonly tool: string; readonly executor: string; readonly manifest: string; readonly before: string; readonly roots: (workspaceRoot: string, root: string) => Effect.Effect<readonly string[], never, FileSystem.FileSystem | Path.Path> };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const EXTENSION = 'blender_manifest.toml';
const YAK_EXECUTABLE = 'YakExecutable';
const LOCK = 'uv.lock';
const PUBLISHED = /^Successfully published.*$/mu;
const REPOSITORY = /^Repository: /mu;
const REPOSITORY_ID = /\(id=(?<id>[^)]+)\)/u;
const DISTRIBUTION = /-(?<platform>\w+)\.yak(?=\/|$)/u;
const UNREAL: Partial<Record<NodeJS.Platform, string>> = { darwin: 'Mac', linux: 'Linux' };
const SIGNING = ['ZXP_CERTIFICATE', 'ZXP_CERTIFICATE_PASSWORD'] as const;
const SIGNER = 'ZXPSignCmd';

// --- [MODELS] --------------------------------------------------------------------------

const Extension = Schema.Struct({
    schema_version: Schema.String,
    id: Schema.String,
    version: Schema.String,
    platforms: Schema.optionalKey(Schema.Array(Schema.String)),
    build: Schema.optionalKey(Schema.Struct({ generated: Schema.optionalKey(Schema.Struct({ platforms: Schema.Array(Schema.String) })) })),
});
const Lock = Schema.Struct({
    manifest: Schema.Struct({ members: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({ members: [] }))),
    package: Schema.Array(Schema.Struct({ name: Schema.String, source: Schema.Struct({ editable: Schema.optionalKey(Schema.String), virtual: Schema.optionalKey(Schema.String), directory: Schema.optionalKey(Schema.String) }) })),
});
const Yak = Schema.Struct({ name: Schema.String, version: Schema.String, authors: Schema.Array(Schema.String), description: Schema.String });
const Uxp = Schema.fromJsonString(Schema.Struct({ manifestVersion: Schema.Finite, id: Schema.String, name: Schema.String, version: Schema.String, host: Schema.Struct({ app: Schema.String }) }));
const Cep = Schema.Struct({ ExtensionBundleId: Schema.String, ExtensionBundleName: Schema.String, ExtensionBundleVersion: Schema.String });
const Pushed = Schema.Struct({ distributions: Schema.Array(Schema.Struct({ filename: Schema.String })) });
const References = Schema.optionalKey(Schema.Array(Schema.Struct({ Name: Schema.String, Enabled: Schema.optionalKey(Schema.Boolean) })));
const Project = Schema.fromJsonString(Schema.Struct({ FileVersion: Schema.Int, Plugins: References, AdditionalPluginDirectories: Schema.optionalKey(Schema.Array(Schema.String)) }));
const Printed = Schema.Trim.check(Schema.isNonEmpty());
const RepositoryGroups = Schema.Struct({ id: Schema.String });
const DistributionGroups = Schema.Struct({ platform: Schema.String });
const Plugin = Schema.fromJsonString(Schema.Struct({ FileVersion: Schema.Int, Version: Schema.optionalKey(Schema.Int), VersionName: Schema.String, Installed: Schema.optionalKey(Schema.Boolean), Plugins: References }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const reported = (command: string, args: readonly string[], signal: RegExp) =>
    Effect.flatMap(printed(command, args), (stdout) =>
        Option.match(Option.fromNullishOr(signal.exec(stdout)), {
            onSome: ([matched]) => Effect.logInfo(matched),
            onNone: () => ToolError.make({ subject: line([command, ...args]), detail: `printed no line matching ${signal.source}: ${stdout.trim()}` }),
        }),
    );

const yak = (bundle: Package) =>
    Effect.flatMap(printed('dotnet', ['msbuild', `-getProperty:${YAK_EXECUTABLE}`], { cwd: bundle.root }), (stdout) =>
        Option.match(Schema.decodeOption(Printed)(stdout), {
            onSome: Effect.succeed,
            onNone: () => ToolError.make({ subject: bundle.root, detail: `evaluates no ${YAK_EXECUTABLE}, which MSBuild sets from the Rhino install it resolves; install Rhino or set RhinoInstallPath` }),
        }),
    );

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
            Effect.map(Option.flatten),
        ),
    );

const exact = (subject: string, version: string) =>
    Effect.fromResult(SemVer.parse(version)).pipe(
        Effect.filterOrFail((parsed) => !(SemVer.isPrerelease(parsed) || SemVer.hasBuild(parsed))),
        Effect.mapError(ToolError.of(subject, `takes a major.minor.patch version alone, which ${version} is not`)),
    );

const built = Effect.fnUntraced(function* (context: ExecutorContext, configuration: Option.Option<string>, start: string) {
    const path = yield* Path.Path;
    const project = yield* Effect.fromNullishOr(context.projectName);
    const node = yield* Effect.fromNullishOr(context.projectGraph.nodes[project]);
    const targets = node.data.targets ?? {};
    const running = Record.fromIterableWith(
        Object.values(context.taskGraph?.tasks ?? {}).filter(({ target }) => target.project === project),
        ({ target }) => [target.target, target.configuration],
    );
    const reached = (names: ReadonlySet<string>): ReadonlySet<string> => {
        const next = new Set([
            ...names,
            ...[...names]
                .flatMap((name) => getDependencyConfigs({ project, target: name }, {}, context.projectGraph, Record.keys(targets)) ?? [])
                .filter(({ projects }) => projects?.includes(project) === true)
                .map(({ target }) => target),
        ]);
        return next.size === names.size ? names : reached(next);
    };
    const selected = [...reached(new Set([start]))].filter((target) => target !== start && target in targets);
    const declared = selected.filter((target) => targets[target]?.outputs !== undefined);
    const outputs = declared.flatMap((target) => {
        const chosen = Option.getOrUndefined(Option.orElse(configuration, () => Option.fromUndefinedOr(running[target] ?? targets[target]?.defaultConfiguration)));
        return getOutputsForTargetAndConfiguration({ project, target, ...(chosen === undefined ? {} : { configuration: chosen }) }, {}, node);
    });
    const listed = yield* Stream.runCollect(
        Stream.fromAsyncIterable(
            glob(
                outputs.filter((output) => !output.startsWith('!')).flatMap((output) => [output, path.join(output, '**')]),
                { cwd: context.root, exclude: outputs.filter((output) => output.startsWith('!')).map((output) => output.slice(1)), withFileTypes: true },
            ),
            ToolError.of(project, 'cannot list its build outputs'),
        ),
    );
    const written = Array.dedupe(listed.filter((dirent) => dirent.isFile()).map((dirent) => path.resolve(context.root, dirent.parentPath, dirent.name)));
    const packages = yield* Effect.forEach(
        written.flatMap((file) => HOSTS.filter((host) => host.archive !== undefined && file.endsWith(host.archive)).map((host) => ({ file, host }))),
        Effect.fnUntraced(function* ({ file, host }) {
            const manifest = yield* entry(file, host.manifest);
            const fields = Option.flatten(yield* Effect.transposeOption(Option.map(manifest, ({ text, at }) => host.read(text, path.join(file, at)))));
            const version = Option.flatMap(fields, (read) => Option.fromUndefinedOr(read.version));
            return Option.map(Option.all({ read: fields, version }), ({ read, version: known }): Package => ({ ...read, version: known, host, file, root: path.join(context.root, node.data.root) }));
        }),
    );
    return {
        project,
        root: node.data.root,
        outputs,
        configurations: Array.dedupe(selected.flatMap((target) => Record.keys(targets[target]?.configurations ?? {}))),
        written: Array.match(selected, { onEmpty: Option.none, onNonEmpty: () => Option.some(written) }),
        packages: Array.getSomes(packages),
    };
});

const sourced = Effect.fnUntraced(function* (context: ExecutorContext, configuration: Option.Option<string>, start: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { project, root, written } = yield* built(context, configuration, start);
    const files = yield* Option.match(written, {
        onSome: Effect.succeed,
        onNone: () =>
            Effect.map(
                Effect.promise(() => globWithWorkspaceContext(context.root, [path.join(root, '**')])),
                Array.map((file) => path.join(context.root, file)),
            ),
    });
    const candidates = files.flatMap((file) => HOSTS.map((host) => ({ host, file, folder: path.join(file, path.relative(host.manifest, '.')) })).filter(({ host, folder }) => matchesGlob(path.relative(folder, file), host.manifest)));
    const manifests = yield* Effect.forEach(
        candidates,
        Effect.fnUntraced(function* (candidate) {
            const text = yield* fs.readFileString(candidate.file);
            return { ...candidate, text, fields: yield* candidate.host.read(text, candidate.file), names: files.filter((other) => other.startsWith(`${candidate.folder}/`)).map((other) => path.relative(candidate.folder, other)) };
        }),
    );
    return { project, manifests };
});

const locked = Effect.fnUntraced(function* (workspaceRoot: string, folder: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const text = yield* fs.readFileString(path.join(workspaceRoot, folder, LOCK));
    const { manifest, package: packages } = yield* Effect.flatMap(Effect.fromResult(Toml.parse(text)), Schema.decodeUnknownEffect(Lock));
    const placed = packages.map(({ name, source }) => ({ name, at: Option.map(Option.fromUndefinedOr(source.editable ?? source.virtual ?? source.directory), (at) => path.join(folder, at)) }));
    return { folder, folders: Array.getSomes(placed.map(({ at }) => at)), members: Array.getSomes(placed.filter(({ name }) => manifest.members.includes(name)).map(({ at }) => at)) };
});

const locking = Effect.fnUntraced(
    function* (workspaceRoot: string, root: string) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const own = yield* locked(workspaceRoot, root);
        const loaded = new Set(own.folders);
        const enclosing = yield* Effect.forEach(own.folders, (folder) =>
            Effect.map(
                Effect.filter(ancestors(path, folder), (at) => fs.exists(path.join(workspaceRoot, at, LOCK))),
                Array.head,
            ),
        );
        const others = yield* Effect.forEach(
            Array.dedupe(Array.getSomes(enclosing)).filter((folder) => folder !== root),
            (folder) => locked(workspaceRoot, folder),
        );
        return [root, ...others.filter(({ folder, members }) => members.every((member) => member === folder || loaded.has(member))).map(({ folder }) => folder)];
    },
    Effect.orElseSucceed(() => Array.empty<string>()),
);

const zipped = Effect.fnUntraced(function* (folder: string, names: readonly string[], file: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    yield* Stream.fromIterable(names).pipe(
        Stream.map((name) => Zip.fileStream(name.split(path.sep).join('/'), fs.stream(path.join(folder, name)))),
        Stream.pipeThroughChannel(Zip.encode()),
        Stream.run(fs.sink(file)),
        Effect.mapError(ToolError.of(file, 'does not write')),
    );
});

const certified = Option.match({
    onSome: Effect.fnUntraced(function* ([certificate, password]: readonly [certificate: string, password: string]) {
        const [p12, secret] = yield* Effect.all([Config.Redacted(certificate), Config.Redacted(password)]);
        return { p12: yield* Effect.fromResult(Base64.decode(Redacted.value(p12))), password: secret };
    }),
    onNone: Effect.fnUntraced(function* () {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const name = hostname();
        const file = path.join(workspaceDataDirectory, SIGNER, `${name}.p12`);
        yield* (yield* fs.exists(file))
            ? Effect.void
            : Effect.all([fs.makeDirectory(path.dirname(file), { recursive: true }), printed(SIGNER, ['-selfSignedCert', '', '', '', name, name, `${file}.${pid}`]), fs.rename(`${file}.${pid}`, file), Effect.logInfo(`Created ${file}, the self-signed certificate CEP installs on this machine sign with`)], {
                  discard: true,
              });
        return { p12: yield* fs.readFile(file), password: Redacted.make(name) };
    }),
});

const signed = Effect.fnUntraced(
    function* (variables: Option.Option<readonly [certificate: string, password: string]>, folder: string, names: readonly string[], file: string, timestampAuthority: Option.Option<string>) {
        const [fs, path, { p12, password }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, certified(variables)]);
        const copy = path.join(yield* fs.makeTempDirectoryScoped(), path.basename(folder));
        yield* Effect.forEach(names, (name) => Effect.andThen(fs.makeDirectory(path.dirname(path.join(copy, name)), { recursive: true }), fs.copyFile(path.join(folder, name), path.join(copy, name))), { discard: true });
        const { code } = yield* ran(SIGNER, ['-sign', copy, file, '/dev/fd/3', Redacted.value(password), ...Option.toArray(timestampAuthority).flatMap((url) => ['-tsa', url])], {
            stdout: 'inherit',
            stderr: 'inherit',
            additionalFds: { fd3: { type: 'input', stream: Stream.make(p12) } },
        });
        return yield* code === 0 ? Effect.void : ToolError.make({ subject: `${SIGNER} -sign ${folder}`, detail: `exited ${code}` });
    },
    Effect.scoped,
    Effect.mapError((error) => (Schema.is(ToolError)(error) ? error : ToolError.make({ subject: `${SIGNER} -sign`, detail: 'does not sign', cause: error }))),
);

const located = (text: string) => {
    const { cst, tokenVector } = xml.parse(text);
    return pipe(
        Option.liftPredicate(cst, (node): node is DocumentCstNode => node.name === 'document'),
        Option.flatMap((document) => Option.fromNullOr(ast.buildAst(document, tokenVector).rootElement)),
        Option.flatMap(({ attributes }) => Array.findFirst(attributes, ({ key, value }) => key === 'ExtensionBundleVersion' && value !== null)),
        Option.flatMap(({ syntax }) => Option.fromUndefinedOr(syntax.value)),
    );
};

// --- [HOSTS] ---------------------------------------------------------------------------

const HOSTS: readonly Host[] = [
    {
        manifest: EXTENSION,
        archive: '.zip',
        technology: 'blender',
        platforms: { darwin: 'macos', win32: 'windows', linux: 'linux' },
        read: (text, file) =>
            Effect.fromResult(Toml.parse(text)).pipe(
                Effect.flatMap(Schema.decodeUnknownEffect(Extension)),
                Effect.mapBoth({ onFailure: ToolError.of(file, 'does not decode'), onSuccess: ({ id, version, platforms, build }) => Option.some({ name: id, version, platforms: build?.generated?.platforms ?? platforms ?? [] }) }),
            ),
        write: (text, version) => Effect.mapBoth(Effect.fromResult(Toml.parse(text)), { onFailure: ToolError.of(EXTENSION, 'does not parse as TOML'), onSuccess: (document) => patch(text, { ...document, version }) }),
        build: { executor: 'extension-build', command: 'blender --command extension build' },
        upgrade: { tool: 'blender', executor: 'extension-constraints', manifest: 'pyproject.toml', before: 'upgrade--uv', roots: locking },
        install: {
            tool: 'blender',
            command: 'blender --command extension install-file',
            run: (bundle) =>
                Effect.flatMap(printed('blender', ['--command', 'extension', 'list']), (listing) =>
                    reported(
                        'blender',
                        [
                            '--command',
                            'extension',
                            'install-file',
                            '--repo',
                            Option.getOrElse(
                                Option.flatMap(
                                    Array.findFirst(listing.split(REPOSITORY), (section) => section.includes(`\n  ${bundle.name} [installed]`)),
                                    (section) => Option.map(Schema.decodeUnknownOption(RepositoryGroups)(REPOSITORY_ID.exec(section)?.groups), ({ id }) => id),
                                ),
                                () => 'user_default',
                            ),
                            '--enable',
                            bundle.file,
                        ],
                        new RegExp(`^STATUS (Installed|Reinstalled) "${RegExp.escape(bundle.name)}"$`, 'mu'),
                    ),
                ).pipe(
                    Effect.andThen(printed('blender', ['--quiet', '--background', '--factory-startup', '--python-expr', 'import bpy; print(bpy.app.binary_path)'])),
                    Effect.map((binary) => Option.some(binary.trim())),
                ),
        },
    },
    {
        manifest: 'manifest.yml',
        archive: '.yak',
        technology: 'rhino',
        platforms: { darwin: 'mac', win32: 'win' },
        read: (text, file) =>
            Effect.succeed(
                Option.map(Option.flatMap(Option.liftThrowable(Yaml.parse)(text), Schema.decodeUnknownOption(Yak)), ({ name, version }) => ({
                    name,
                    version,
                    platforms: Array.filter(Option.toArray(Option.map(Schema.decodeUnknownOption(DistributionGroups)(DISTRIBUTION.exec(file)?.groups), ({ platform }) => platform)), (platform) => platform !== 'any'),
                })),
            ),
        install: { tool: 'yak', command: 'yak install', run: (bundle) => Effect.flatMap(yak(bundle), (executable) => Effect.as(reported(executable, ['install', bundle.file], new RegExp(`^Successfully installed ${RegExp.escape(bundle.name)}`, 'mu')), Option.some(executable))) },
        publish: {
            command: 'yak push',
            run: Effect.fnUntraced(
                function* (bundle: Package, options: Publish) {
                    const path = yield* Path.Path;
                    const registry = Option.getOrElse(options.registry, () => 'https://yak.rhino3d.com');
                    const pushed = yield* Effect.flatMap(
                        HttpClient.get(`${registry}/versions/${bundle.name}/${bundle.version}`),
                        HttpClientResponse.matchStatus({
                            404: () => Effect.succeed(Array.empty<string>()),
                            orElse: (response) => Effect.map(Effect.flatMap(HttpClientResponse.filterStatusOk(response), HttpClientResponse.schemaBodyJson(Pushed)), ({ distributions }) => distributions.map(({ filename }) => filename)),
                        }),
                    );
                    const args = ['push', bundle.file, ...Option.toArray(options.registry).flatMap((source) => ['--source', source])];
                    return yield* Match.value({ present: pushed.includes(path.basename(bundle.file)), dry: options.dryRun }).pipe(
                        Match.when({ present: true }, () => Effect.logInfo(`Skipped ${path.basename(bundle.file)}, ${registry} already holds it`)),
                        Match.when({ dry: true }, () => Effect.logInfo(`Would run ${line(['yak', ...args])}; ${registry} holds ${Array.match(pushed, { onEmpty: () => 'no distribution', onNonEmpty: (names) => names.join(', ') })} of ${bundle.name} ${bundle.version}`)),
                        Match.orElse(() => Effect.flatMap(yak(bundle), (executable) => reported(executable, args, PUBLISHED))),
                    );
                },
                (effect, bundle) =>
                    effect.pipe(
                        Effect.catchTags({
                            HttpClientError: ToolError.of(bundle.file, 'does not reach the Yak server'),
                            SchemaError: ToolError.of(bundle.file, 'reads an unexpected Yak server reply'),
                        }),
                        Effect.provide(FetchHttpClient.layer),
                    ),
            ),
        },
    },
    {
        manifest: 'manifest.json',
        archive: '.ccx',
        technology: 'uxp',
        platforms: {},
        read: (text) => Effect.succeed(Option.map(Schema.decodeOption(Uxp)(text), ({ name, version }) => ({ name, version, platforms: [] }))),
        write: (text, version) => Effect.as(exact('manifest.json', version), applyEdits(text, modify(text, ['version'], version, {}))),
        build: { executor: 'package', command: 'zip', pack: zipped },
        place: uxp,
    },
    {
        manifest: 'CSXS/manifest.xml',
        archive: '.zxp',
        technology: 'cep',
        platforms: {},
        read: (text) =>
            Stream.make(text).pipe(
                Stream.pipeThroughChannel(Xml.decodeString()),
                Stream.pipeThroughChannel(Xml.elements(['ExtensionManifest'])),
                Stream.runHead,
                Effect.map(Option.flatMap(({ attributes }) => Schema.decodeUnknownOption(Cep)(attributes))),
                Effect.map(Option.map(({ ExtensionBundleName, ExtensionBundleVersion }) => ({ name: ExtensionBundleName, version: ExtensionBundleVersion, platforms: [] }))),
                Effect.orElseSucceed(Option.none),
            ),
        write: (text, version) => Effect.andThen(exact('CSXS/manifest.xml', version), Effect.mapBoth(Effect.fromOption(located(text)), { onFailure: ToolError.of('CSXS/manifest.xml', 'states no ExtensionBundleVersion'), onSuccess: (at) => `${text.slice(0, at.startOffset + 1)}${version}${text.slice(at.endOffset)}` })),
        build: { executor: 'package', command: `${SIGNER} -sign`, variables: SIGNING, pack: (folder, names, file, timestampAuthority) => signed(Option.some(SIGNING), folder, names, file, timestampAuthority) },
        place: (placement) =>
            cep(
                placement,
                Effect.fnUntraced(function* (staged: string) {
                    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
                    const file = path.join(yield* fs.makeTempDirectoryScoped(), `${path.basename(staged)}.zxp`);
                    yield* signed(placement.variables, placement.folder, placement.names, file, Option.none());
                    yield* fs.stream(file).pipe(
                        Stream.pipeThroughChannel(Zip.decode()),
                        Stream.mapEffect(({ header, body }) =>
                            header.type === 'Directory' ? fs.makeDirectory(path.join(staged, header.path), { recursive: true }) : Effect.andThen(fs.makeDirectory(path.dirname(path.join(staged, header.path)), { recursive: true }), Stream.run(body, fs.sink(path.join(staged, header.path)))),
                        ),
                        Stream.runDrain,
                        Effect.mapError((error) => (Schema.is(ToolError)(error) ? error : ToolError.make({ subject: file, detail: 'does not unpack', cause: error }))),
                    );
                }, Effect.scoped),
            ),
    },
    {
        manifest: '*.uproject',
        technology: 'unreal',
        platforms: UNREAL,
        standalone: true,
        read: (text, file) =>
            Effect.mapBoth(Schema.decodeEffect(Project)(text), {
                onFailure: ToolError.of(file, 'does not decode'),
                onSuccess: ({ Plugins, AdditionalPluginDirectories }) => Option.some({ name: basename(file, '.uproject'), platforms: [], folders: ['Plugins', ...(AdditionalPluginDirectories ?? [])], references: (Plugins ?? []).map(({ Name, Enabled }) => ({ name: Name, enabled: Enabled === true })) }),
            }),
        build: { executor: 'unreal-build', command: 'RunUBT.sh -TargetType=Editor' },
    },
    {
        manifest: '*.uplugin',
        archive: '.zip',
        technology: 'unreal',
        platforms: UNREAL,
        standalone: true,
        read: (text, file) =>
            Effect.mapBoth(Schema.decodeEffect(Plugin)(text), {
                onFailure: ToolError.of(file, 'does not decode'),
                onSuccess: ({ VersionName, Installed, Plugins }) => Option.some({ name: basename(file, '.uplugin'), version: VersionName, platforms: [], installed: Installed === true, references: (Plugins ?? []).map(({ Name, Enabled }) => ({ name: Name, enabled: Enabled === true })) }),
            }),
        write: (text, version) =>
            Effect.mapBoth(Schema.decodeEffect(Plugin)(text), {
                onFailure: ToolError.of('uplugin', 'does not decode'),
                onSuccess: ({ Version }) => applyEdits(text, [...modify(text, ['VersionName'], version, {}), ...modify(text, ['Version'], (Version ?? 0) + 1, {})]),
            }),
        build: { executor: 'unreal-build', command: 'RunUAT.sh BuildPlugin' },
    },
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { built, EXTENSION, type Fields, HOSTS, type Host, type Package, type Publish, sourced, UNREAL };

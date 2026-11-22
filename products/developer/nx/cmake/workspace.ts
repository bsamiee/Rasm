import { type } from 'node:os';
import { basename, delimiter, dirname, join, posix, relative, resolve } from 'node:path';
import process from 'node:process';
import { fileURLToPath } from 'node:url';
import { normalizePath, readJsonFile } from '@nx/devkit';
import { globWithWorkspaceContext, type PackageJson } from '@nx/devkit/internal';
import { runner, spawned } from '@rasm/nx-distribution/inference';
import { Array, Cause, Config, Context, Effect, FileSystem, Function, Layer, Match, Option, pipe, Record, Result, Schema, SemVer } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#/package.json'))).name;
const MANIFEST = 'CMakeLists.txt';
const PRESETS = 'CMakePresets.json';
const PACKAGING = 'CPackConfig.cmake';
const CLIENT = 'client-nx-cmake';
const QUERY = 'query.json';
const API = ['.cmake', 'api', 'v1'] as const;
const CALL = /^[ \t]*project[ \t]*\(/imu;
const TIDY = /^CMAKE_[A-Z]+_CLANG_TIDY$/u;
const WORKING = '$env{PWD}';
const MACRO = /\$(?<space>env|penv)?\{(?<name>\w+)\}/gu;
const ENVIRONMENT = /\$env\{(?<name>\w+)\}/gu;
const ARGUMENT = /#\[(?<comment>=*)\[[\s\S]*?\]\k<comment>\]|#[^\n]*|\[(?<bracket>=*)\[[\s\S]*?\]\k<bracket>\]|"(?:\\[\s\S]|[^"\\])*"|[^\s()#"]+|\)/gu;

// --- [MODELS] --------------------------------------------------------------------------

const Workflow = Schema.Struct({
    name: Schema.String,
    configure: Schema.String,
    tree: Schema.String,
    build: Schema.optionalKey(Schema.String),
    test: Schema.optionalKey(Schema.String),
    after: Schema.optionalKey(Schema.String),
    tidy: Schema.Array(Schema.Struct({ variable: Schema.String, launcher: Schema.String })),
});
const Packaging = Schema.Struct({ name: Schema.String, configurePreset: Schema.String });
const Launcher = Schema.Union([Schema.String, Schema.Struct({ value: Schema.String })]);
const Presets = Schema.fromJsonString(
    Schema.Struct({
        configurePresets: Schema.Array(
            Schema.Struct({
                name: Schema.String,
                inherits: Schema.optionalKey(Schema.Union([Schema.String, Schema.Array(Schema.String)])),
                generator: Schema.optionalKey(Schema.String),
                binaryDir: Schema.optionalKey(Schema.String),
                cacheVariables: Schema.optionalKey(Schema.Record(Schema.String, Schema.Unknown)),
                environment: Schema.optionalKey(Schema.Record(Schema.String, Schema.NullOr(Schema.String))),
            }),
        ).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        packagePresets: Schema.Array(Packaging).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        workflowPresets: Schema.Array(Schema.Struct({ name: Schema.String, steps: Schema.Array(Schema.Struct({ type: Schema.String, name: Schema.String })) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    }),
);
const Reference = Schema.Struct({ jsonFile: Schema.String });
const Index = Schema.fromJsonString(
    Schema.Struct({
        cmake: Schema.Struct({ paths: Schema.Struct({ ctest: Schema.String }) }),
        reply: Schema.Struct({ [CLIENT]: Schema.Struct({ [QUERY]: Schema.Struct({ responses: Schema.Tuple([Reference, Reference]) }) }) }),
    }),
);
const Codemodel = Schema.fromJsonString(
    Schema.Struct({
        configurations: Schema.NonEmptyArray(
            Schema.Struct({
                directories: Schema.Array(Schema.Struct({ source: Schema.String, build: Schema.String, projectIndex: Schema.Int, jsonFile: Schema.String })),
                projects: Schema.Array(Schema.Struct({ name: Schema.String, directoryIndexes: Schema.NonEmptyArray(Schema.Int) })),
                targets: Schema.Array(Schema.Struct({ projectIndex: Schema.Int, jsonFile: Schema.String })),
            }),
        ),
    }),
);
const Directory = Schema.fromJsonString(Schema.Struct({ installers: Schema.Array(Schema.Unknown) }));
const Links = Schema.Array(Schema.Struct({ id: Schema.optionalKey(Schema.String), backtrace: Schema.optionalKey(Schema.Int) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([])));
const Target = Schema.fromJsonString(
    Schema.Struct({
        id: Schema.String,
        name: Schema.String,
        nameOnDisk: Schema.optionalKey(Schema.String),
        link: Schema.optionalKey(Schema.Unknown),
        artifacts: Schema.Array(Schema.Struct({ path: Schema.String })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        sources: Schema.Array(Schema.Struct({ path: Schema.String, isGenerated: Schema.optionalKey(Schema.Boolean) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        compileGroups: Schema.Array(Schema.Struct({ includes: Schema.Array(Schema.Struct({ path: Schema.String })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        backtraceGraph: Schema.Struct({ files: Schema.Array(Schema.String), nodes: Schema.Array(Schema.Struct({ file: Schema.Int, command: Schema.optionalKey(Schema.Int) })) }),
        linkLibraries: Links,
        interfaceLinkLibraries: Links,
        dependencies: Links,
    }),
);
const Inputs = Schema.fromJsonString(Schema.Struct({ inputs: Schema.Array(Schema.Struct({ path: Schema.String, isGenerated: Schema.optionalKey(Schema.Boolean) })) }));
const Listing = Schema.fromJsonString(
    Schema.Struct({
        tests: Schema.Array(Schema.Struct({ backtrace: Schema.Int })),
        backtraceGraph: Schema.Struct({ files: Schema.Array(Schema.String), nodes: Schema.Array(Schema.Struct({ file: Schema.Int, parent: Schema.optionalKey(Schema.Int) })) }),
    }),
);
const Tree = Schema.Struct({
    root: Schema.String,
    workflows: Schema.Array(Workflow),
    packages: Schema.Array(Packaging),
    members: Schema.Array(
        Schema.Struct({
            name: Schema.String,
            root: Schema.String,
            build: Schema.String,
            installs: Schema.Boolean,
            packaged: Schema.Boolean,
            tested: Schema.Boolean,
            folders: Schema.Array(Schema.String),
            targets: Schema.Array(Schema.Struct({ name: Schema.String, artifacts: Schema.Array(Schema.Struct({ path: Schema.String })), binary: Schema.optionalKey(Schema.String) })),
            links: Schema.Array(Schema.Struct({ root: Schema.String, file: Schema.String })),
        }),
    ),
    inputs: Schema.Array(Schema.String),
});

type Tree = typeof Tree.Type;

// --- [SERVICES] ------------------------------------------------------------------------

class Linked extends Context.Service<Linked, Map<string, Tree['members']>>()(`${PLUGIN}/workspace/Linked`) {
    static readonly layer = Layer.sync(Linked, () => new Map());
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const versioned = (text: string, at: string) => {
    const stated = Option.flatMap(Option.fromNullOr(CALL.exec(text)), (call) => {
        const start = call.index + call[0].length;
        const tokens = Array.takeWhile(
            [...text.slice(start).matchAll(ARGUMENT)].filter(([token]) => !token.startsWith('#')),
            ([token]) => token !== ')',
        );
        return Option.map(
            Option.flatMap(
                Array.findFirstIndex(tokens, ([token]) => token === 'VERSION'),
                (keyword) => Array.get(tokens, keyword + 1),
            ),
            ({ 0: token, index }) => {
                const quote = token.startsWith('"') ? 1 : 0;
                return { version: token.slice(quote, token.length - quote), start: start + index + quote, end: start + index + token.length - quote };
            },
        );
    });
    return Effect.mapError(Effect.fromOption(stated), (cause) => new Cause.UnknownError(cause, `${at} states no VERSION in a project() call, which nx release reads and writes`));
};

const restated = Effect.fnUntraced(function* (text: string, version: string, at: string) {
    yield* Effect.fromResult(SemVer.parse(version)).pipe(
        Effect.filterOrFail((parsed) => !(SemVer.isPrerelease(parsed) || SemVer.hasBuild(parsed))),
        Effect.mapError((cause) => new Cause.UnknownError(cause, `${at} takes a major.minor.patch version alone in project(VERSION), which ${version} is not`)),
    );
    const { start, end } = yield* versioned(text, at);
    return `${text.slice(0, start)}${version}${text.slice(end)}`;
});

const expanded = (named: Readonly<Record<string, string>>, environment: Readonly<Record<string, string>>) => (text: string) =>
    text.replaceAll(MACRO, (macro, space: string | undefined, name: string): string =>
        Match.value(space).pipe(
            Match.when(Match.undefined, () => named[name] ?? macro),
            Match.when('env', () => Option.match(Record.get(environment, name), { onNone: () => macro, onSome: expanded(named, Record.remove(environment, name)) })),
            Match.orElse(() => `$env{${name}}`),
        ),
    );

const filled = Effect.fnUntraced(function* (folder: string) {
    const names = Array.dedupe(folder.matchAll(ENVIRONMENT).map(([, name = '']) => name));
    const values = yield* Effect.forEach(names, (name) => Config.withDefault(Config.String(name), ''));
    return Array.zip(names, values).reduce((text, [name, value]) => text.replaceAll(`$env{${name}}`, value), folder);
});

const configured = (configure: string, defines: Readonly<Record<string, string>> = {}): Array.NonEmptyReadonlyArray<string> => ['cmake', '--preset', configure, '-S', '.', ...Record.collect(defines, (name, value) => `-D${name}=${value}`)];

const replied = Effect.fnUntraced(function* (workspaceRoot: string, root: string, workflow: typeof Workflow.Type, located: string) {
    const fs = yield* FileSystem.FileSystem;
    const inside = (at: string) => normalizePath(relative(workspaceRoot, resolve(workspaceRoot, root, at))) || '.';
    const tree = join(workspaceRoot, located);
    const [query, reply] = [join(tree, ...API, 'query', CLIENT), join(tree, ...API, 'reply')];
    const read = <S extends Schema.Decoder<unknown>>(schema: S, file: string) => Effect.flatMap(fs.readFileString(join(reply, file)), Schema.decodeEffect(schema));
    yield* fs.makeDirectory(query, { recursive: true });
    yield* fs.writeFileString(join(query, QUERY), '{"requests":[{"kind":"codemodel","version":2},{"kind":"cmakeFiles","version":1}]}');
    yield* spawned(configured(workflow.configure), { cwd: join(workspaceRoot, root) });
    const newest = yield* Effect.flatMap(fs.readDirectory(reply), (names) => Effect.fromOption(Array.last(names.filter((name) => name.startsWith('index-')).toSorted())));
    const index = yield* read(Index, newest);
    const [modelled, filed] = index.reply[CLIENT][QUERY].responses;
    const [codemodel, { inputs }, listing] = yield* Effect.all([read(Codemodel, modelled.jsonFile), read(Inputs, filed.jsonFile), Effect.flatMap(spawned([index.cmake.paths.ctest, '--test-dir', tree, '--show-only=json-v1'], { cwd: workspaceRoot }), Schema.decodeEffect(Listing))], {
        concurrency: 'unbounded',
    });
    const { directories, projects, targets } = Array.headNonEmpty(codemodel.configurations);
    const loaded = yield* Effect.forEach(targets, ({ projectIndex, jsonFile }) => Effect.map(read(Target, jsonFile), (target) => ({ projectIndex, ...target })), { concurrency: 'unbounded' });
    const owners = new Map(loaded.map(({ projectIndex, id }) => [id, projectIndex] as const));
    const directed = new Map(directories.map(({ source, projectIndex }) => [inside(source), projectIndex] as const));
    const origin = (at: number): Option.Option<number> => Option.flatMap(Array.get(listing.backtraceGraph.nodes, at), ({ file, parent }) => (parent === undefined ? Option.some(file) : origin(parent)));
    const tested = new Set(
        Array.getSomes(
            listing.tests.map(({ backtrace }) =>
                pipe(
                    origin(backtrace),
                    Option.flatMap((file) => Array.get(listing.backtraceGraph.files, file)),
                    Option.flatMap((file) => Option.fromUndefinedOr(directed.get(inside(dirname(file))))),
                ),
            ),
        ),
    );
    const placed = yield* Effect.forEach(
        projects,
        Effect.fnUntraced(function* ({ name, directoryIndexes }, at) {
            const { source, build, jsonFile } = yield* Effect.fromOption(Array.get(directories, Array.headNonEmpty(directoryIndexes)));
            const [{ installers }, packaging] = yield* Effect.all([read(Directory, jsonFile), fs.exists(join(tree, build, PACKAGING))], { concurrency: 'unbounded' });
            return { name, root: inside(source), build, installs: installers.length > 0, packaged: installers.length > 0 && packaging, tested: tested.has(at) };
        }),
    );
    const stated = loaded.flatMap(({ projectIndex: from, backtraceGraph, linkLibraries, interfaceLinkLibraries, dependencies }) => [...linkLibraries, ...interfaceLinkLibraries, ...dependencies].map((link) => ({ from, ...backtraceGraph, ...link })));
    const linked = Array.getSomes(
        stated.map(({ from, files, nodes, id, backtrace }) =>
            Option.all({
                from: Option.some(from),
                link: Option.all({
                    root: pipe(
                        Option.fromUndefinedOr(id),
                        Option.flatMap((target) => Option.fromUndefinedOr(owners.get(target))),
                        Option.filter((owner) => owner !== from),
                        Option.flatMap((owner) => Array.get(placed, owner)),
                        Option.map((owner) => owner.root),
                    ),
                    file: pipe(
                        Option.fromUndefinedOr(backtrace),
                        Option.flatMap((at) => Array.get(nodes, at)),
                        Option.filter(({ command }) => command !== undefined),
                        Option.flatMap(({ file }) => Array.get(files, file)),
                        Option.map(inside),
                    ),
                }),
            }),
        ),
    );
    const listed = loaded.flatMap(({ projectIndex, sources }) => sources.filter(({ isGenerated }) => isGenerated !== true).map((source) => ({ projectIndex, folder: posix.dirname(inside(source.path)) })));
    const included = loaded.flatMap(({ projectIndex, compileGroups }) => compileGroups.flatMap(({ includes }) => includes).map((include) => ({ projectIndex, folder: inside(include.path) })));
    const searched = [...listed, ...included].filter(({ folder }) => posix.relative(located, folder).startsWith('..'));
    const binaries = new Map(loaded.flatMap(({ id, link, nameOnDisk, artifacts }) => artifacts.filter(({ path: file }) => link !== undefined && basename(file) === nameOnDisk).map(({ path: file }) => [id, { binary: file }] as const)));
    const members = placed.map((member, at) => ({
        ...member,
        folders: Array.dedupe(searched.filter(({ projectIndex, folder }) => projectIndex === at && !posix.relative(member.root, folder).startsWith('..')).map(({ folder }) => folder)),
        targets: loaded.filter(({ projectIndex, artifacts }) => projectIndex === at && artifacts.length > 0).map(({ id, name, artifacts }) => ({ name, artifacts, ...binaries.get(id) })),
        links: Array.dedupe(linked.filter(({ from }) => from === at).map(({ link }) => link)),
    }));
    const tracked = yield* Effect.promise(() =>
        globWithWorkspaceContext(
            workspaceRoot,
            inputs.filter(({ isGenerated }) => isGenerated !== true).map(({ path: at }) => inside(at)),
        ),
    );
    return { members, inputs: tracked };
});

const resolved = Effect.fnUntraced(function* (workspaceRoot: string, presets: string, manifests: readonly string[]) {
    const fs = yield* FileSystem.FileSystem;
    const root = posix.dirname(presets);
    const source = join(workspaceRoot, root);
    const { configurePresets, packagePresets: packages, workflowPresets } = yield* Effect.flatMap(fs.readFileString(join(workspaceRoot, presets)), Schema.decodeEffect(Presets));
    type Steps = (typeof workflowPresets)[number]['steps'];
    const chain = (name: string): readonly (typeof configurePresets)[number][] => configurePresets.flatMap((preset) => (preset.name === name ? [preset, ...Array.ensure(preset.inherits ?? []).flatMap(chain)] : []));
    const macros = (configure: string, at: string) =>
        expanded(
            {
                sourceDir: at,
                sourceParentDir: `${at}/..`,
                sourceDirName: basename(source),
                presetName: configure,
                configurePresetName: configure,
                hostSystemName: process.platform === 'win32' ? 'Windows' : type(),
                fileDir: at,
                dollar: '$',
                pathListSep: delimiter,
                ...Record.getSomes({ generator: Array.findFirst(chain(configure), ({ generator }) => Option.fromUndefinedOr(generator)) }),
            },
            Record.getSomes(
                Record.map(
                    Record.fromEntries(
                        chain(configure)
                            .toReversed()
                            .flatMap(({ environment = {} }) => Record.toEntries(environment)),
                    ),
                    Option.fromNullOr,
                ),
            ),
        );
    const stepped = (listed: Steps, kind: string) =>
        Option.map(
            Array.findFirst(listed, (each) => each.type === kind),
            (each) => each.name,
        );
    const followed = (listed: Steps) =>
        Option.flatMap(
            Array.findFirstIndex(listed, (each) => each.type === 'test'),
            (at) => stepped(listed.slice(at + 1), 'build'),
        );
    const rooted = (configure: string) =>
        normalizePath(
            relative(
                workspaceRoot,
                resolve(
                    source,
                    Option.match(
                        Array.findFirst(chain(configure), ({ binaryDir }) => Option.fromUndefinedOr(binaryDir)),
                        { onNone: () => '.', onSome: macros(configure, source) },
                    ),
                ),
            ),
        ) || '.';
    const launchers = (configure: string) =>
        Record.collect(
            Record.filterMap(
                Record.fromEntries(
                    chain(configure)
                        .toReversed()
                        .flatMap(({ cacheVariables = {} }) => Record.toEntries(cacheVariables)),
                ),
                (stated, variable) => (TIDY.test(variable) ? Result.fromOption(Schema.decodeUnknownOption(Launcher)(stated), Function.constVoid) : Result.failVoid),
            ),
            (variable, named) => ({ variable, launcher: macros(configure, WORKING)(typeof named === 'string' ? named : named.value) }),
        );
    const workflows = Array.getSomes(
        workflowPresets.map(({ name, steps }) => Option.map(stepped(steps, 'configure'), (configure): typeof Workflow.Type => ({ ...Record.getSomes({ build: stepped(steps, 'build'), test: stepped(steps, 'test'), after: followed(steps) }), tree: rooted(configure), name, configure, tidy: launchers(configure) }))),
    );
    const declaring = yield* Effect.filter(
        manifests.filter((file) => !posix.relative(root, file).startsWith('..')),
        (file) => Effect.map(fs.readFileString(join(workspaceRoot, file)), (text) => CALL.test(text)),
        { concurrency: 'unbounded' },
    );
    return yield* Option.match(
        Option.filter(Array.head(workflows), () => declaring.length > 0),
        {
            onNone: () => Effect.succeed<Tree>({ root, workflows, packages, members: [], inputs: [] }),
            onSome: (workflow) =>
                Effect.map(
                    Effect.flatMap(filled(workflow.tree), (located) => replied(workspaceRoot, root, workflow, located)),
                    (reply): Tree => ({ root, workflows, packages, ...reply }),
                ),
        },
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = runner(Linked.layer);

// --- [EXPORTS] -------------------------------------------------------------------------

export { configured, ENVIRONMENT, Linked, MANIFEST, PACKAGING, PLUGIN, PRESETS, resolved, restated, run, type Tree, versioned, WORKING };

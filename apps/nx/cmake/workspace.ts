import { dirname, join, relative, resolve } from 'node:path';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { globWithWorkspaceContext, type PackageJson } from '@nx/devkit/internal';
import { Array, Config, Context, Effect, FileSystem, Formatter, Function, Layer, ManagedRuntime, Number, Option, pipe, Record, Result, Schema, type Scope, SemVer, Stream } from 'effect';
import { ChildProcess, CommandLine } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'CMakeLists.txt';
const PRESETS = 'CMakePresets.json';
const PACKAGING = 'CPackConfig.cmake';
const CLIENT = 'client-nx-cmake';
const QUERY = 'query.json';
const API = ['.cmake', 'api', 'v1'] as const;
const CALL = /^[ \t]*project[ \t]*\(/imu;
const TIDY = /^CMAKE_[A-Z]+_CLANG_TIDY$/u;
const SOURCE_DIR = /\$\{sourceDir\}/gu;
const PRESET_NAME = /\$\{presetName\}/gu;
const ENVIRONMENT = /\$env\{(?<name>\w+)\}/gu;
const ARGUMENT = /#\[(?<comment>=*)\[[\s\S]*?\]\k<comment>\]|#[^\n]*|\[(?<bracket>=*)\[[\s\S]*?\]\k<bracket>\]|"(?:\\[\s\S]|[^"\\])*"|[^\s()#"]+|\)/gu;

// --- [MODELS] --------------------------------------------------------------------------

const Workflow = Schema.Struct({
    name: Schema.String,
    configure: Schema.String,
    tree: Schema.String,
    stated: Schema.Boolean,
    build: Schema.optionalKey(Schema.String),
    test: Schema.optionalKey(Schema.String),
    after: Schema.optionalKey(Schema.String),
    tidy: Schema.Array(Schema.Struct({ variable: Schema.String, launcher: Schema.String })),
});
const Packaging = Schema.Struct({ name: Schema.String, configurePreset: Schema.String });
const Launcher = Schema.Union([Schema.String, Schema.Struct({ value: Schema.String })]);
const Presets = Schema.fromJsonString(
    Schema.Struct({
        configurePresets: Schema.Array(Schema.Struct({ name: Schema.String, inherits: Schema.optionalKey(Schema.Union([Schema.String, Schema.Array(Schema.String)])), binaryDir: Schema.optionalKey(Schema.String), cacheVariables: Schema.optionalKey(Schema.Record(Schema.String, Schema.Unknown)) })).pipe(
            Schema.withDecodingDefaultKey(Effect.succeed([])),
        ),
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
                directories: Schema.Array(Schema.Struct({ source: Schema.String, build: Schema.String, jsonFile: Schema.String })),
                projects: Schema.Array(Schema.Struct({ name: Schema.String, parentIndex: Schema.optionalKey(Schema.Int), directoryIndexes: Schema.NonEmptyArray(Schema.Int) })),
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
const Listing = Schema.fromJsonString(Schema.Struct({ tests: Schema.Array(Schema.Unknown) }));
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
            targets: Schema.Array(Schema.Struct({ name: Schema.String, artifacts: Schema.Array(Schema.Struct({ path: Schema.String })) })),
            links: Schema.Array(Schema.Struct({ root: Schema.String, file: Schema.String })),
        }),
    ),
    inputs: Schema.Array(Schema.String),
});

const Reason = Schema.TaggedUnion({ Exited: { code: Schema.Int, said: Schema.String }, Undecoded: { form: Schema.String }, Missing: { what: Schema.String }, Refused: { rule: Schema.String } });

type Tree = typeof Tree.Type;

// --- [ERRORS] --------------------------------------------------------------------------

class CMakeError extends Schema.TaggedError<CMakeError, { readonly brand: unique symbol }>()('CMakeError', {
    reason: Reason,
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
}) {
    static readonly of = (reason: CMakeError['reason'], subject: string) => (cause?: unknown) => CMakeError.make({ reason, subject, ...(cause === undefined ? {} : { cause }) });

    override get message() {
        const reason = Reason.match(this.reason, { Exited: ({ code, said }) => [`exits ${code}`, ...(said === '' ? [] : [said])].join('\n'), Undecoded: ({ form }) => `does not read as ${form}`, Missing: ({ what }) => `states no ${what}`, Refused: ({ rule }) => rule });
        return [`${this.subject}: ${reason}`, ...(this.cause === undefined ? [] : [Formatter.format(this.cause)])].join(': ');
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

class Linked extends Context.Service<Linked, Map<string, Tree['members']>>()('@rasm/nx-cmake/workspace/Linked') {
    static readonly layer = Layer.sync(Linked, () => new Map());
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const printed = Effect.fnUntraced(function* (command: string, args: readonly string[], options: ChildProcess.CommandOptions) {
    const handle = yield* ChildProcess.make(command, args, { extendEnv: true, detached: false, stdin: 'ignore', ...options });
    const [stdout, said, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? stdout : yield* CMakeError.of({ _tag: 'Exited', code, said }, CommandLine.display([command, ...args]))();
}, Effect.scoped);

const manifested = (workspaceRoot: string, root: string) =>
    Effect.map(
        Effect.flatMap(FileSystem.FileSystem, (fs) => fs.exists(join(workspaceRoot, root, 'package.json'))),
        (held): Readonly<Record<string, TargetConfiguration>> => (held ? (readJsonFile<PackageJson>(join(workspaceRoot, root, 'package.json')).nx?.targets ?? {}) : {}),
    );

const versioned = (text: string, at: string) => {
    const from = Option.map(Option.fromNullOr(CALL.exec(text)), (call) => call.index + call[0].length);
    const stated = Option.flatMap(from, (start) => {
        const tokens = Array.takeWhile(
            [...text.slice(start).matchAll(ARGUMENT)].filter(([token]) => !token.startsWith('#')),
            ([token]) => token !== ')',
        );
        return Option.map(Array.get(tokens, tokens.findIndex(([token]) => token === 'VERSION') + 1 || tokens.length), ({ 0: token, index }) => {
            const quote = token.startsWith('"') ? 1 : 0;
            return { version: token.slice(quote, token.length - quote), start: start + index + quote, end: start + index + token.length - quote };
        });
    });
    return Effect.mapError(Effect.fromOption(stated), CMakeError.of({ _tag: 'Missing', what: 'VERSION in a project() call, which nx release reads and writes' }, at));
};

const restated = Effect.fnUntraced(function* (text: string, version: string, at: string) {
    yield* Effect.fromResult(SemVer.parse(version)).pipe(
        Effect.filterOrFail((parsed) => !(SemVer.isPrerelease(parsed) || SemVer.hasBuild(parsed))),
        Effect.mapError(CMakeError.of({ _tag: 'Refused', rule: `takes a major.minor.patch version alone in project(VERSION), which ${version} is not` }, at)),
    );
    const { start, end } = yield* versioned(text, at);
    return `${text.slice(0, start)}${version}${text.slice(end)}`;
});

const expanded = Effect.fnUntraced(function* (folder: string) {
    const names = Array.dedupe([...folder.matchAll(ENVIRONMENT)].map(([, name = '']) => name));
    const read = yield* Effect.forEach(names, (name) => Config.option(Config.String(name)));
    return Option.map(Option.all(read), (values) => {
        const held = Record.fromEntries(Array.zip(names, values));
        return folder.replaceAll(ENVIRONMENT, (whole, name: string) => Option.getOrElse(Record.get(held, name), Function.constant(whole)));
    });
});

const planted = (outputPath: Option.Option<string>, root: string, tool: string, name: string) => Option.match(outputPath, { onNone: () => join(root, 'build', tool, name), onSome: (at) => join(at, tool, root, name) });

const configured = (root: string, { configure, stated }: Pick<typeof Workflow.Type, 'configure' | 'stated'>, tree: string, defines: Readonly<Record<string, string>> = {}): Array.NonEmptyReadonlyArray<string> => [
    'cmake',
    '--preset',
    configure,
    '-S',
    root,
    ...(stated ? [] : ['-B', tree]),
    ...Record.collect(defines, (name, value) => `-D${name}=${value}`),
];

const replied = Effect.fnUntraced(function* (workspaceRoot: string, root: string, workflow: typeof Workflow.Type, located: string) {
    const fs = yield* FileSystem.FileSystem;
    const inside = (at: string) => relative(workspaceRoot, resolve(workspaceRoot, root, at)) || '.';
    const tree = join(workspaceRoot, located);
    const [query, reply] = [join(tree, ...API, 'query', CLIENT), join(tree, ...API, 'reply')];
    const read = <S extends Schema.Decoder<unknown>>(schema: S, file: string) => Effect.flatMap(fs.readFileString(join(reply, file)), Schema.decodeEffect(schema));
    yield* fs.makeDirectory(query, { recursive: true });
    yield* fs.writeFileString(join(query, QUERY), '{"requests":[{"kind":"codemodel","version":2},{"kind":"cmakeFiles","version":1}]}');
    const [configure, ...configuring] = configured(root, workflow, tree);
    yield* printed(configure, configuring, { cwd: workspaceRoot });
    const newest = yield* Effect.flatMap(fs.readDirectory(reply), (names) => Effect.fromOption(Array.last(names.filter((name) => name.startsWith('index-')).toSorted())));
    const index = yield* read(Index, newest);
    const [modelled, filed] = index.reply[CLIENT][QUERY].responses;
    const [codemodel, { inputs }] = yield* Effect.all([read(Codemodel, modelled.jsonFile), read(Inputs, filed.jsonFile)], { concurrency: 'unbounded' });
    const { directories, projects, targets } = Array.headNonEmpty(codemodel.configurations);
    const loaded = yield* Effect.forEach(targets, ({ projectIndex, jsonFile }) => Effect.map(read(Target, jsonFile), (target) => ({ projectIndex, ...target })), { concurrency: 'unbounded' });
    const owners = new Map(loaded.map(({ projectIndex, id }) => [id, projectIndex] as const));
    const placed = yield* Effect.forEach(
        projects,
        Effect.fnUntraced(function* ({ name, parentIndex, directoryIndexes }) {
            const { source, build, jsonFile } = yield* Effect.fromOption(Array.get(directories, Array.headNonEmpty(directoryIndexes)));
            const [{ installers }, { tests }, packaging] = yield* Effect.all([read(Directory, jsonFile), Effect.flatMap(printed(index.cmake.paths.ctest, ['--test-dir', join(tree, build), '--show-only=json-v1'], { cwd: workspaceRoot }), Schema.decodeEffect(Listing)), fs.exists(join(tree, build, PACKAGING))], {
                concurrency: 'unbounded',
            });
            return { name, parentIndex, root: inside(source), build, installs: installers.length > 0, packaged: installers.length > 0 && packaging, tests: tests.length };
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
    const listed = loaded.flatMap(({ projectIndex, sources }) => sources.filter(({ isGenerated }) => isGenerated !== true).map((source) => ({ projectIndex, folder: dirname(inside(source.path)) })));
    const included = loaded.flatMap(({ projectIndex, compileGroups }) => compileGroups.flatMap(({ includes }) => includes).map((include) => ({ projectIndex, folder: inside(include.path) })));
    const searched = [...listed, ...included].filter(({ folder }) => relative(located, folder).startsWith('..'));
    const members = placed.map(({ parentIndex: _, tests, ...member }, at) => ({
        ...member,
        tested: tests > Number.sumAll(placed.filter(({ parentIndex }) => parentIndex === at).map((nested) => nested.tests)),
        folders: Array.dedupe(searched.filter(({ projectIndex, folder }) => projectIndex === at && !relative(member.root, folder).startsWith('..')).map(({ folder }) => folder)),
        targets: loaded.filter(({ projectIndex, artifacts }) => projectIndex === at && artifacts.length > 0).map(({ name, artifacts }) => ({ name, artifacts })),
        links: linked.filter(({ from }) => from === at).map(({ link }) => link),
    }));
    const tracked = yield* Effect.promise(() =>
        globWithWorkspaceContext(
            workspaceRoot,
            inputs.filter(({ isGenerated }) => isGenerated !== true).map(({ path: at }) => inside(at)),
        ),
    );
    return { members, inputs: tracked };
});

const resolved = Effect.fnUntraced(
    function* (workspaceRoot: string, presets: string, manifests: readonly string[], placed: (preset: string) => string) {
        const fs = yield* FileSystem.FileSystem;
        const root = dirname(presets);
        const { configurePresets, packagePresets: packages, workflowPresets } = yield* Effect.flatMap(fs.readFileString(join(workspaceRoot, presets)), Schema.decodeEffect(Presets));
        type Steps = (typeof workflowPresets)[number]['steps'];
        const chain = (name: string): readonly (typeof configurePresets)[number][] => configurePresets.filter((preset) => preset.name === name).flatMap((preset) => [preset, ...Array.ensure(preset.inherits ?? []).flatMap(chain)]);
        const stepped = (listed: Steps, type: string) =>
            Option.map(
                Array.findFirst(listed, (each) => each.type === type),
                (each) => each.name,
            );
        const followed = (listed: Steps) => stepped(listed.slice(listed.findIndex(({ type }) => type === 'test') + 1 || listed.length), 'build');
        const rooted = (configure: string) =>
            Option.match(
                Array.findFirst(chain(configure), ({ binaryDir }) => Option.fromUndefinedOr(binaryDir)),
                {
                    onNone: () => ({ tree: placed(configure), stated: false }),
                    onSome: (folder) => ({
                        tree: relative(
                            workspaceRoot,
                            folder.replaceAll(SOURCE_DIR, () => join(workspaceRoot, root)).replaceAll(PRESET_NAME, () => configure),
                        ),
                        stated: true,
                    }),
                },
            );
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
                (variable, named) => ({ variable, launcher: typeof named === 'string' ? named : named.value }),
            );
        const workflows = Array.getSomes(
            workflowPresets.map(({ name, steps }) => Option.map(stepped(steps, 'configure'), (configure): typeof Workflow.Type => ({ ...Record.getSomes({ build: stepped(steps, 'build'), test: stepped(steps, 'test'), after: followed(steps) }), ...rooted(configure), name, configure, tidy: launchers(configure) }))),
        );
        const declaring = yield* Effect.filter(
            manifests.filter((file) => file !== join(root, MANIFEST) && !relative(root, file).startsWith('..')),
            (file) => Effect.map(fs.readFileString(join(workspaceRoot, file)), (text) => CALL.test(text)),
            { concurrency: 'unbounded' },
        );
        const held = declaring.length > 0 && Option.isSome(yield* Effect.option(printed('cmake', ['--version'], {})));
        const head = Option.filter(Array.head(workflows), () => held);
        const located = Option.flatten(yield* Effect.transposeOption(Option.map(head, (workflow) => expanded(workflow.tree))));
        return yield* Option.match(Option.all({ workflow: head, located }), {
            onNone: () => Effect.succeed({ tree: { root, workflows, packages, members: [], inputs: [] } satisfies Tree, partial: declaring.length > 0 && workflows.length > 0 }),
            onSome: (selected) => Effect.map(replied(workspaceRoot, root, selected.workflow, selected.located), (reply) => ({ tree: { root, workflows, packages, ...reply } satisfies Tree, partial: false })),
        });
    },
    Effect.catchTag('NoSuchElementError', (cause) => Effect.fail(CMakeError.of({ _tag: 'Missing', what: 'CMake file API reply naming its projects' }, 'cmake')(cause))),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const runtime = ManagedRuntime.make(Layer.merge(Linked.layer, layer));

const run = <A, E>(effect: Effect.Effect<A, E, Linked | NodeServices | Scope.Scope>) => runtime.runPromise(Effect.scoped(effect));

// --- [EXPORTS] -------------------------------------------------------------------------

export { CMakeError, configured, ENVIRONMENT, Linked, MANIFEST, manifested, PACKAGING, PRESETS, planted, printed, resolved, restated, run, type Tree, versioned };

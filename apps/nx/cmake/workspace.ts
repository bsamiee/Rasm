import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { present, printed, ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, FileSystem, Option, Path, pipe, Record, Schema, SemVer } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'CMakeLists.txt';
const PRESETS = 'CMakePresets.json';
const PACKAGING = 'CPackConfig.cmake';
const CLIENT = 'client-nx-cmake';
const QUERY = 'query.json';
const API = ['.cmake', 'api', 'v1'] as const;
const CALL = /^[ \t]*project[ \t]*\(/imu;
const ARGUMENT = /#\[(?<comment>=*)\[[\s\S]*?\]\k<comment>\]|#[^\n]*|\[(?<bracket>=*)\[[\s\S]*?\]\k<bracket>\]|"(?:\\[\s\S]|[^"\\])*"|[^\s()#"]+|\)/gu;

// --- [MODELS] --------------------------------------------------------------------------

const Workflow = Schema.Struct({ name: Schema.String, configure: Schema.String, build: Schema.optionalKey(Schema.String), test: Schema.optionalKey(Schema.String), package: Schema.optionalKey(Schema.String) });
const Presets = Schema.fromJsonString(Schema.Struct({ workflowPresets: Schema.Array(Schema.Struct({ name: Schema.String, steps: Schema.Array(Schema.Struct({ type: Schema.String, name: Schema.String })) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }));
const Reference = Schema.Struct({ jsonFile: Schema.String });
const Index = Schema.fromJsonString(
    Schema.Struct({
        cmake: Schema.Struct({ paths: Schema.Struct({ ctest: Schema.String }) }),
        reply: Schema.Struct({ [CLIENT]: Schema.Struct({ [QUERY]: Schema.Struct({ responses: Schema.Tuple([Reference, Reference, Reference]) }) }) }),
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
        backtraceGraph: Schema.Struct({ files: Schema.Array(Schema.String), nodes: Schema.Array(Schema.Struct({ file: Schema.Int, command: Schema.optionalKey(Schema.Int) })) }),
        linkLibraries: Links,
        interfaceLinkLibraries: Links,
        dependencies: Links,
    }),
);
const Inputs = Schema.fromJsonString(Schema.Struct({ inputs: Schema.Array(Schema.Struct({ path: Schema.String, isGenerated: Schema.optionalKey(Schema.Boolean) })) }));
const Toolchains = Schema.fromJsonString(Schema.Struct({ toolchains: Schema.Array(Schema.Struct({ language: Schema.String, compiler: Schema.Struct({ path: Schema.optionalKey(Schema.String) }) })) }));
const Listing = Schema.fromJsonString(Schema.Struct({ tests: Schema.Array(Schema.Unknown) }));
const Tree = Schema.Struct({
    root: Schema.String,
    workflows: Schema.Array(Workflow),
    members: Schema.Array(
        Schema.Struct({
            name: Schema.String,
            root: Schema.String,
            build: Schema.String,
            installs: Schema.Boolean,
            packaged: Schema.Boolean,
            tested: Schema.Boolean,
            targets: Schema.Array(Schema.Struct({ name: Schema.String, artifacts: Schema.Array(Schema.Struct({ path: Schema.String })) })),
            links: Schema.Array(Schema.Struct({ root: Schema.String, file: Schema.String })),
        }),
    ),
    inputs: Schema.Array(Schema.String),
    toolchains: Schema.Array(Schema.Struct({ language: Schema.String, compiler: Schema.String })),
});

type Tree = typeof Tree.Type;

// --- [OPERATIONS] ----------------------------------------------------------------------

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
    return Effect.mapError(Effect.fromOption(stated), ToolError.of(at, 'states no VERSION in a project() call, which nx release reads and writes'));
};

const restated = Effect.fnUntraced(function* (text: string, version: string, at: string) {
    yield* Effect.fromResult(SemVer.parse(version)).pipe(
        Effect.filterOrFail((parsed) => !(SemVer.isPrerelease(parsed) || SemVer.hasBuild(parsed))),
        Effect.mapError(ToolError.of(at, `takes a major.minor.patch version alone in project(VERSION), which ${version} is not`)),
    );
    const { start, end } = yield* versioned(text, at);
    return `${text.slice(0, start)}${version}${text.slice(end)}`;
});

const planted = (path: Path.Path, placed: (tool: string, name: string) => Option.Option<string>, root: string, tool: string, name: string) => Option.getOrElse(placed(tool, path.join(root, name)), () => path.join(root, 'build', tool, name));

const invoked = (launcher: readonly string[], command: Array.NonEmptyReadonlyArray<string>): Array.NonEmptyReadonlyArray<string> => Array.prependAll(command, launcher);

const configured = (launcher: readonly string[], root: string, preset: string, tree: string, defines: Readonly<Record<string, string>> = {}) => invoked(launcher, ['cmake', '--preset', preset, '-S', root, '-B', tree, ...Record.collect(defines, (name, value) => `-D${name}=${value}`)]);

const replied = Effect.fnUntraced(function* (workspaceRoot: string, root: string, launcher: readonly string[], preset: string, tree: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const inside = (at: string) => path.relative(workspaceRoot, path.resolve(workspaceRoot, root, at)) || '.';
    const [query, reply] = [path.join(tree, ...API, 'query', CLIENT), path.join(tree, ...API, 'reply')];
    const read = <S extends Schema.Decoder<unknown>>(schema: S, file: string) => Effect.flatMap(fs.readFileString(path.join(reply, file)), Schema.decodeEffect(schema));
    yield* fs.makeDirectory(query, { recursive: true });
    yield* fs.writeFileString(path.join(query, QUERY), '{"requests":[{"kind":"codemodel","version":2},{"kind":"cmakeFiles","version":1},{"kind":"toolchains","version":1}]}');
    const [configure, ...configuring] = configured(launcher, root, preset, tree);
    yield* printed(configure, configuring, { cwd: workspaceRoot });
    const newest = yield* Effect.flatMap(fs.readDirectory(reply), (names) => Effect.fromOption(Array.last(names.filter((name) => name.startsWith('index-')).toSorted())));
    const index = yield* read(Index, newest);
    const [modelled, filed, compiled] = index.reply[CLIENT][QUERY].responses;
    const [codemodel, { inputs }, { toolchains }] = yield* Effect.all([read(Codemodel, modelled.jsonFile), read(Inputs, filed.jsonFile), read(Toolchains, compiled.jsonFile)], { concurrency: 'unbounded' });
    const { directories, projects, targets } = Array.headNonEmpty(codemodel.configurations);
    const loaded = yield* Effect.forEach(targets, ({ projectIndex, jsonFile }) => Effect.map(read(Target, jsonFile), (target) => ({ projectIndex, ...target })), { concurrency: 'unbounded' });
    const owners = new Map(loaded.map(({ projectIndex, id }) => [id, projectIndex] as const));
    const placed = yield* Effect.forEach(
        projects,
        Effect.fnUntraced(function* ({ name, parentIndex, directoryIndexes }) {
            const { source, build, jsonFile } = yield* Effect.fromOption(Array.get(directories, Array.headNonEmpty(directoryIndexes)));
            const [{ installers }, { tests }, packaging] = yield* Effect.all([read(Directory, jsonFile), Effect.flatMap(printed(index.cmake.paths.ctest, ['--test-dir', path.join(tree, build), '--show-only=json-v1'], { cwd: workspaceRoot }), Schema.decodeEffect(Listing)), fs.exists(path.join(tree, build, PACKAGING))], {
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
    const members = placed.map(({ parentIndex: _, tests, ...member }, at) => ({
        ...member,
        tested: tests > placed.filter(({ parentIndex }) => parentIndex === at).reduce((count, nested) => count + nested.tests, 0),
        targets: loaded.filter(({ projectIndex, artifacts }) => projectIndex === at && artifacts.length > 0).map(({ name, artifacts }) => ({ name, artifacts })),
        links: linked.filter(({ from }) => from === at).map(({ link }) => link),
    }));
    const tracked = yield* Effect.promise(() =>
        globWithWorkspaceContext(
            workspaceRoot,
            inputs.filter(({ isGenerated }) => isGenerated !== true).map(({ path: at }) => inside(at)),
        ),
    );
    return { members, inputs: tracked, toolchains: Array.getSomes(toolchains.map(({ language, compiler }) => Option.map(Option.fromUndefinedOr(compiler.path), (at) => ({ language, compiler: path.relative(workspaceRoot, at) })))) };
});

const resolved = Effect.fnUntraced(
    function* (workspaceRoot: string, presets: string, launcher: readonly string[], placed: (preset: string) => string) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const root = path.dirname(presets);
        const { workflowPresets } = yield* Effect.flatMap(fs.readFileString(path.join(workspaceRoot, presets)), Schema.decodeEffect(Presets));
        const workflows = Array.getSomes(workflowPresets.map(({ name, steps }) => Schema.decodeUnknownOption(Workflow)({ ...Record.fromIterableWith(steps.toReversed(), (step) => [step.type, step.name]), name })));
        const [probe, ...probed] = invoked(launcher, ['cmake', '--version']);
        const held = Option.isSome(yield* present(probe, probed, { cwd: workspaceRoot }));
        return yield* Option.match(
            Option.filter(Array.head(workflows), () => held),
            {
                onNone: () => Effect.succeed({ tree: { root, workflows, members: [], inputs: [], toolchains: [] } satisfies Tree, partial: workflows.length > 0 }),
                onSome: ({ configure }) => Effect.map(replied(workspaceRoot, root, launcher, configure, path.join(workspaceRoot, placed(configure))), (reply) => ({ tree: { root, workflows, ...reply } satisfies Tree, partial: false })),
            },
        );
    },
    (effect, _, presets) => Effect.mapError(effect, ToolError.of(presets, 'does not resolve as a CMake build tree')),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { configured, invoked, MANIFEST, PACKAGING, PRESETS, planted, resolved, restated, type Tree, versioned };

import { type } from 'node:os';
import { basename, delimiter, join, posix, relative, resolve } from 'node:path';
import process from 'node:process';
import { fileURLToPath } from 'node:url';
import { normalizePath } from '@nx/devkit';
import { below, inside, runner } from '@rasm/nx/inference';
import { Array, Cause, Combiner, Context, Effect, Equal, FileSystem, Function, Graph, Layer, Match, Option, Predicate, Record, Result, Schema, SchemaTransformation, SemVer } from 'effect';
import plugin from '#/package.json' with { type: 'json' };

// --- [TYPES] ---------------------------------------------------------------------------

type Tree = Effect.Success<ReturnType<typeof resolved>>;
type Workflow = Tree['workflows'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const IDENTIFIER = fileURLToPath(import.meta.resolve('#/compiler.cmake'));
const MANIFEST = 'CMakeLists.txt';
const PRESETS = 'CMakePresets.json';
const PACKAGING = 'CPackConfig.cmake';
const REGISTRY = { manifest: 'vcpkg.json', configuration: 'vcpkg-configuration.json' } as const;
const DEFINING = new Set(['add_library', 'add_executable', 'add_custom_target']);
const INHERITED = Record.makeReducerUnion<string, unknown>(Combiner.last());
const TIDY = /^CMAKE_[A-Z]+_CLANG_TIDY$/u;
const WORKING = '$env{PWD}';
const MACRO = /\$(?<space>env|penv)?\{(?<name>\w+)\}/gu;
const TOKEN = /#\[(?<comment>=*)\[[\s\S]*?\]\k<comment>\]|#[^\n]*|\[(?<bracket>=*)\[[\s\S]*?\]\k<bracket>\]|"(?<quoted>(?:\\[\s\S]|[^"\\])*)"|(?<head>[A-Za-z_]\w*)[ \t]*\(|[^\s()#"]+|[()]/gu;

// --- [MODELS] --------------------------------------------------------------------------

const Launcher = Schema.Union([Schema.String, Schema.Struct({ value: Schema.String }).pipe(Schema.decodeTo(Schema.String, SchemaTransformation.transform({ decode: ({ value }) => value, encode: (value) => ({ value }) })))]);
class Presets extends Schema.Struct({
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
    packagePresets: Schema.Array(Schema.Struct({ name: Schema.String, configurePreset: Schema.String })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    workflowPresets: Schema.Array(Schema.Struct({ name: Schema.String, steps: Schema.Array(Schema.Struct({ type: Schema.String, name: Schema.String })) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
}).pipe(Schema.fromJsonString, Schema.Opaque<Presets>()) {}

// --- [SERVICES] ------------------------------------------------------------------------

class Linked extends Context.Service<Linked, Map<string, Tree['members']>>()(`${plugin.name}/workspace/Linked`) {
    static readonly layer = Layer.sync(Linked, () => new Map());
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const commands = (text: string) => {
    const [, tokens] = Array.mapAccum(
        [...text.matchAll(TOKEN)].filter(([token]) => !token.startsWith('#')),
        { depth: 0, call: -1 },
        ({ depth, call }, { 0: token, index, groups = {} }) => {
            const opened = depth === 0 && groups['head'] !== undefined;
            const next = { depth: Math.max(0, depth + Number(token.endsWith('(')) - Number(token === ')')), call: opened ? call + 1 : call };
            const value = groups['head'] ?? groups['quoted'] ?? token;
            const start = index + (groups['quoted'] === undefined ? 0 : 1);
            return [next, { call: next.call, name: opened ? Option.some(value.toLowerCase()) : Option.none(), argument: depth > 0 && token !== '(' && token !== ')' ? Option.some({ value, start, end: start + value.length }) : Option.none() }] as const;
        },
    );
    const argued = Map.groupBy(
        tokens.flatMap(({ call, argument }) => Option.toArray(argument).map((held) => ({ call, ...held }))),
        ({ call }) => call,
    );
    return tokens.flatMap(({ call, name }) => Option.toArray(name).map((named) => ({ name: named, args: argued.get(call) ?? [] })));
};

const versioned = (text: string, at: string) =>
    Array.findFirst(commands(text), ({ name }) => name === 'project').pipe(
        Option.flatMap(({ args }) => Array.findFirst(args, ({ value }, index) => (value === 'VERSION' ? Array.get(args, index + 1) : Option.none()))),
        Effect.fromOption,
        Effect.mapError((cause) => new Cause.UnknownError(cause, `${at} states no VERSION in a project() call, which nx release reads and writes`)),
    );

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

const configured = (configure: string, defines: Readonly<Record<string, string>> = {}): Array.NonEmptyReadonlyArray<string> => ['cmake', '--preset', configure, '-S', '.', ...Record.collect(defines, (name, value) => `-D${name}=${value}`)];

const resolved = Effect.fnUntraced(function* (workspaceRoot: string, presets: string, matched: ReadonlySet<string>) {
    const fs = yield* FileSystem.FileSystem;
    const root = posix.dirname(presets);
    const source = join(workspaceRoot, root);
    const { configurePresets, packagePresets: packages, workflowPresets } = yield* Effect.flatMap(fs.readFileString(join(workspaceRoot, presets)), Schema.decodeEffect(Presets));
    const inherited = configurePresets.flatMap(({ name, inherits = [] }) => Array.ensure(inherits).map((parent) => [name, parent] as const));
    const inheriting = Graph.directed<Presets['configurePresets'][number], undefined>((mutable) => {
        const placed = new Map(configurePresets.map((preset) => [preset.name, Graph.addNode(mutable, preset)] as const));
        inherited
            .flatMap(([child, parent]) => Array.cartesian(Array.fromNullishOr(placed.get(child)), Array.fromNullishOr(placed.get(parent))))
            .forEach(([from, to]) => {
                Graph.addEdge(mutable, from, to, undefined);
            });
    });
    yield* Effect.when(new Cause.UnknownError(inheriting, `${presets} lists configure presets inheriting each other in a cycle, which cmake refuses`), Effect.succeed(!Graph.isAcyclic(inheriting)));
    const chains = new Map(inheriting.pipe(Graph.nodes, Graph.entries, Array.fromIterable).map(([at, preset]) => [preset.name, inheriting.pipe(Graph.dfs({ start: [at] }), Graph.values, Array.reverse)] as const));
    const chain = (name: string) => chains.get(name) ?? [];
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
                ...Record.getSomes({ generator: Array.findLast(chain(configure), ({ generator }) => Option.fromUndefinedOr(generator)) }),
            },
            Record.filter(INHERITED.combineAll(chain(configure).map(({ environment = {} }) => environment)), Predicate.isString),
        );
    const stepped = (listed: Presets['workflowPresets'][number]['steps'], kind: string) => listed.find((each) => each.type === kind)?.name;
    const followed = (listed: Presets['workflowPresets'][number]['steps']) => Option.getOrUndefined(Array.findFirst(listed, (each, at) => (each.type === 'test' ? Option.fromUndefinedOr(stepped(listed.slice(at + 1), 'build')) : Option.none())));
    const rooted = (configure: string) =>
        Array.findLast(chain(configure), ({ binaryDir }) => Option.fromUndefinedOr(binaryDir)).pipe(
            Option.map(macros(configure, source)),
            Option.getOrElse(() => '.'),
            (at) => normalizePath(relative(workspaceRoot, resolve(source, at))) || '.',
        );
    const variables = (configure: string) => INHERITED.combineAll(chain(configure).map(({ cacheVariables = {} }) => cacheVariables));
    const toolchain = (configure: string) =>
        Array.findFirst(['VCPKG_CHAINLOAD_TOOLCHAIN_FILE', 'CMAKE_TOOLCHAIN_FILE'], (variable) => Option.liftPredicate(variables(configure)[variable], Predicate.isString)).pipe(
            Option.map((stated) => posix.normalize(macros(configure, root)(stated))),
            Option.getOrUndefined,
        );
    const launchers = (configure: string) =>
        Record.collect(
            Record.filterMap(variables(configure), (stated, variable) => (TIDY.test(variable) ? Result.mapError(Schema.decodeUnknownResult(Launcher)(stated), Function.constVoid) : Result.failVoid)),
            (variable, launcher) => ({ variable, launcher: macros(configure, WORKING)(launcher) }),
        );
    const workflows = workflowPresets.flatMap(({ name, steps }) =>
        Array.fromNullishOr(stepped(steps, 'configure')).map((configure) => ({ build: stepped(steps, 'build'), test: stepped(steps, 'test'), after: followed(steps), tree: rooted(configure), name, configure, tidy: launchers(configure), toolchain: toolchain(configure) })),
    );
    const trees = Array.filter(matched, (file) => posix.basename(file) === PRESETS && file !== presets && inside(root)(file)).map(posix.dirname);
    const inputs = Array.filter(matched, (file) => inside(root)(file) && !trees.some((nested) => inside(nested)(file)));
    const parsed = yield* Effect.forEach(
        inputs.filter((file) => posix.basename(file) === MANIFEST),
        (file) => Effect.map(fs.readFileString(join(workspaceRoot, file)), (text) => ({ file, folder: posix.dirname(file), calls: commands(text) })),
        { concurrency: 'unbounded' },
    );
    const invoked = parsed.flatMap(({ file, folder, calls }) => calls.map((call) => ({ file, folder, ...call })));
    const declared = Array.dedupeWith(
        invoked.flatMap(({ name, folder, args }) => (name === 'project' ? Array.take(args, 1).map(({ value }) => ({ name: value, folder, args })) : [])),
        (one, other) => one.folder === other.folder,
    );
    const placed = declared.filter(({ folder }) => folder === root || !declared.some((outer) => outer.folder !== root && below(outer.folder)(folder)));
    const folders = new Set(placed.map((held) => held.folder));
    const owner = (folder: string) => Array.findLast(Array.scan(posix.relative(root, folder).split('/'), root, posix.join), (at) => folders.has(at) && inside(at)(folder)).pipe(Option.getOrElse(() => root));
    const defined = Record.fromEntries(invoked.filter(({ name }) => DEFINING.has(name)).flatMap(({ folder, args }) => Array.take(args, 1).map(({ value }) => [value, owner(folder)] as const)));
    const linked = Map.groupBy(
        invoked.flatMap(({ file, folder, args }) => {
            const from = Option.getOrElse(
                Option.flatMap(Array.head(args), ({ value }) => Record.get(defined, value)),
                () => owner(folder),
            );
            return Array.filterMap(Array.drop(args, 1), ({ value }) => Result.fromOption(Option.filter(Record.get(defined, value), Predicate.not(Equal.equals(from))), Function.constVoid)).map((target) => ({ from, root: target, file }));
        }),
        ({ from }) => from,
    );
    const registries = Map.groupBy(
        inputs.filter((file) => posix.basename(file) === REGISTRY.manifest).map((manifest) => ({ folder: posix.dirname(manifest), inputs: [manifest, posix.join(posix.dirname(manifest), REGISTRY.configuration)].filter((at) => matched.has(at)) })),
        ({ folder }) => owner(folder),
    );
    const members = placed.map(({ name, folder, args }) => ({
        name,
        root: folder,
        build: posix.relative(root, folder),
        compiled: folder !== root && !args.some(({ value }) => value === 'NONE'),
        packaged: args.some(({ value }) => value === 'VERSION'),
        links: Array.dedupe((linked.get(folder) ?? []).map(({ root: to, file }) => ({ root: to, file }))),
        registries: registries.get(folder) ?? [],
    }));
    return { root, workflows, packages, members, inputs, identifier: normalizePath(relative(workspaceRoot, IDENTIFIER)) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = runner(Linked.layer);

// --- [EXPORTS] -------------------------------------------------------------------------

export { configured, Linked, MANIFEST, PACKAGING, PRESETS, REGISTRY, resolved, restated, run, type Tree, versioned, WORKING, type Workflow };

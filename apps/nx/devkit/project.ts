import { existsSync } from 'node:fs';
import { dirname, extname, join, relative, resolve } from 'node:path';
import { platform } from 'node:process';
import { AggregateCreateNodesError, type CreateDependenciesContext, type CreateNodesContext, type CreateNodesResult, createNodesFromFiles, getOutputsForTargetAndConfiguration, hashArray, normalizePath, type ProjectConfiguration, parseJson, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, getNamedInputs, quoteShellArg } from '@nx/devkit/internal';
import { type RawTsconfigJsonCache, walkTsconfigExtendsChain } from '@nx/js/internal';
import { Array, Cause, Effect, FileSystem, Function, flow, Option, Predicate, Record, Result, Schema } from 'effect';
import { Toml, Yaml } from 'effect/encoding';

// --- [TYPES] ---------------------------------------------------------------------------

type Members = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;
type Input = NonNullable<TargetConfiguration['inputs']>[number];

interface Stated extends ProjectConfiguration {
    readonly clean: string;
    readonly packaged?: readonly string[];
    readonly publish?: TargetConfiguration;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const DEVKIT = '@rasm/nx-devkit';
const SOURCES = ['default', '^default'];
const PRODUCTION = /^(?<upstream>\^?)production$/u;
const PLATFORM = { runtime: 'node -p process.platform+process.arch' } as const;
const TSCONFIG = 'tsconfig.json';
const DOCUMENTS: Readonly<Record<string, (text: string) => Option.Option<unknown>>> = {
    '.json': Option.liftThrowable(parseJson<object>),
    '.toml': flow(Toml.parse, Result.getSuccess),
    '.yaml': Option.liftThrowable(Yaml.parse),
    '.yml': Option.liftThrowable(Yaml.parse),
};

// --- [MODELS] --------------------------------------------------------------------------

const Referencing = Schema.Struct({ references: Schema.Array(Schema.Struct({ path: Schema.String })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) });
const Hosted = Schema.fromJsonString(
    Schema.Struct({
        os: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        nx: Schema.optionalKey(Schema.Struct({ targets: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ inputs: Schema.optionalKey(Schema.Array(Schema.declare((input): input is Input => Predicate.isString(input) || Predicate.isObject(input)))) }))) })),
    }),
).annotate({ description: 'package.json of a project root: os lists the platforms it runs on, nx.targets the target rows Nx merges over every inferred target' });
const Table = Schema.Record(Schema.String, Schema.Unknown);
const Marked = Schema.Struct({ marker: Schema.Struct({ file: Schema.String, table: Schema.optionalKey(Schema.Array(Schema.String)) }) });
const Tooled = Schema.Struct({
    tools: Schema.Record(
        Schema.String,
        Schema.Record(
            Schema.String,
            Schema.declare((row): row is TargetConfiguration & typeof Marked.Type => Schema.is(Marked)(row)),
        ),
    ).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
}).annotate({
    description: 'tools: targets a plugin row of nx.json states as data. tools.<verb>.<tool> holds Nx target fields beside marker, and a project gets <verb>--<tool> where its root holds marker.file, a path relative to the root, and that JSON, TOML, or YAML file holds the key path marker.table when given',
});
const Shared = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) }).annotate({
    description: `Options every plugin row reads, each defaulting to the value under ${DEVKIT} in nx.json pluginsConfig. outputPath: workspace folder a target writes generated output under as <outputPath>/<tool>/<project name>, absent leaves each tool its own folder`,
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const line = (argv: readonly string[]) => argv.map(quoteShellArg).join(' ');

const ancestors = (folder: string): readonly string[] =>
    Array.unfold(
        Option.some(folder),
        Option.map((at: string) => [at, Option.liftPredicate(dirname(at), () => at !== '.')] as const),
    );

const chained = (workspaceRoot: string, roots: readonly string[]): ReadonlyMap<string, readonly string[]> => {
    const parsed: RawTsconfigJsonCache = new Map();
    const referenced = (at: string, raw: unknown) =>
        Option.match(Schema.decodeUnknownOption(Referencing)(raw), {
            onNone: () => [],
            onSome: ({ references }) => references.map((to) => resolve(dirname(at), to.path)).map((to) => (extname(to) === '.json' ? to : join(to, TSCONFIG))),
        });
    const reached = (entry: string, seen: Set<string>): Set<string> => {
        const found: string[] = [];
        walkTsconfigExtendsChain(
            entry,
            (at, raw) => {
                seen.add(at);
                found.push(...(at === entry ? referenced(at, raw) : []));
                return 'continue';
            },
            { jsonCache: parsed },
        );
        return found.filter((to) => !seen.has(to)).reduce((held, to) => reached(to, held), seen);
    };
    return new Map(
        roots.map((root) => {
            const files = Option.match(
                Array.findFirst(
                    ancestors(root).map((at) => join(workspaceRoot, at, TSCONFIG)),
                    existsSync,
                ),
                { onNone: () => [], onSome: (config) => [...reached(config, new Set())] },
            );
            return [root, files.map((at) => normalizePath(relative(workspaceRoot, at))).filter((at) => !(at.startsWith('..') || at.split('/').includes('node_modules')))] as const;
        }),
    );
};

const placed =
    (options: unknown, { nxJsonConfiguration: { pluginsConfig } }: CreateNodesContext) =>
    (tool: string, name = '{projectName}') =>
        Option.map(
            Option.flatMap(Schema.decodeOption(Shared)({ ...pluginsConfig?.[DEVKIT], ...(Predicate.isObject(options) ? options : {}) }), ({ outputPath }) => Option.fromUndefinedOr(outputPath)),
            (at) => join(at, tool, name),
        );

const produced = (projects: CreateDependenciesContext['projects']) => {
    const targets = Record.toEntries(projects).flatMap(([name, data]) => Record.keys(Record.filter(data.targets ?? {}, (target) => target.outputs !== undefined)).map((target) => ({ name, target, data })));
    const selections = targets.flatMap(({ name, target, data }) => [{}, ...Record.keys(data.targets?.[target]?.configurations ?? {}).map((configuration) => ({ configuration }))].map((selected) => ({ name, target, data, selected })));
    return selections.flatMap(({ name, target, data, selected }) =>
        getOutputsForTargetAndConfiguration({ project: name, target, ...selected }, {}, { name, type: 'lib', data })
            .filter((output) => !output.startsWith('!'))
            .map((output) => ({ root: data.root, output })),
    );
};

const linked = ({ projects, externalNodes = {} }: CreateDependenciesContext, edges: readonly RawProjectGraphDependency[]): RawProjectGraphDependency[] => {
    const names = createProjectRootMappingsFromProjectConfigurations(projects);
    const named = (at: string) => Option.orElse(Option.fromUndefinedOr(names.get(at)), () => Option.liftPredicate(at, (id) => id in externalNodes));
    const resolved = Array.getSomes(edges.map((edge) => Option.map(Option.all([named(edge.source), named(edge.target)]), ([source, target]): RawProjectGraphDependency => ({ ...edge, source, target })))).filter(({ source, target }) => source !== target);
    return [...new Map(resolved.map((edge) => [hashArray([edge.source, edge.target, edge.type]), edge] as const)).values()];
};

const collected = (read: (file: string, index: number) => Promise<CreateNodesResult>, files: readonly string[], options: unknown, context: CreateNodesContext) =>
    Effect.tryPromise({
        try: () => createNodesFromFiles((file, _options, _context, index) => read(file, index), files, options, context),
        catch: (cause) => (cause instanceof AggregateCreateNodesError ? cause : new AggregateCreateNodesError([[null, Predicate.isError(cause) ? cause : new Cause.UnknownError(cause, 'createNodes failed outside a file')]], [])),
    });

const tooled = Effect.fnUntraced(function* (members: Members, options: unknown, { workspaceRoot }: CreateNodesContext, root: string) {
    const fs = yield* FileSystem.FileSystem;
    const { tools } = yield* Schema.decodeEffect(Tooled)(options ?? {});
    const rows = Record.collect(tools, (verb, group) => Record.collect(group, (tool, { marker, ...target }) => ({ verb, tool, marker, target }))).flat();
    const within = (at: Option.Option<unknown>, key: string) => Option.flatMap(Option.flatMap(at, Schema.decodeUnknownOption(Table)), Record.get(key));
    const held = yield* Effect.filter(rows, ({ marker: { file, table } }) =>
        table === undefined ? fs.exists(join(workspaceRoot, root, file)) : Effect.map(Effect.option(fs.readFileString(join(workspaceRoot, root, file))), (text) => Option.isSome(table.reduce(within, Option.flatMap(text, DOCUMENTS[extname(file)] ?? Function.constant(Option.none()))))),
    );
    return Record.union(
        members,
        Record.map(
            Array.groupBy(held, ({ verb }) => verb),
            Record.fromIterableWith(({ tool, target }: (typeof rows)[number]): readonly [string, TargetConfiguration] => [tool, target]),
        ),
        (own, stated) => ({ ...own, ...stated }),
    );
});

const configured = Effect.fnUntraced(function* ({ clean: cleaning = {}, attach: attaching = {}, ...members }: Members, { clean, packaged = [], publish, ...project }: Stated, context: CreateNodesContext) {
    const fs = yield* FileSystem.FileSystem;
    const { os, nx } = yield* fs.readFileString(join(context.workspaceRoot, project.root, 'package.json')).pipe(
        Effect.flatMap(Schema.decodeEffect(Hosted)),
        Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed<typeof Hosted.Type>({ os: [] })),
    );
    const restated = Record.map(
        Array.groupBy(
            Record.collect({ ...nx?.targets, ...project.targets }, (name, { inputs = [] }) => ({ name, inputs })),
            ({ name }) => name.split('--', 1).join(''),
        ),
        Array.flatMap(({ inputs }) => inputs.filter((input) => input !== '...')),
    );
    const allowed = os.filter((entry) => !entry.startsWith('!'));
    const technologies = project.metadata?.technologies ?? [];
    const lowered = 'production' in { ...getNamedInputs(project.root, context), ...project.namedInputs } ? '$<upstream>production' : '$<upstream>default';
    const read = ({ inputs, cache }: TargetConfiguration): Pick<TargetConfiguration, 'inputs'> => {
        const named = (inputs ?? SOURCES).map((input) => (Predicate.isString(input) ? input.replace(PRODUCTION, lowered) : input));
        return inputs === undefined && cache !== true ? {} : { inputs: [...named, ...(named.some(Predicate.hasProperty('externalDependencies')) ? [] : [{ externalDependencies: [] }])] };
    };
    const written = Record.collect(members, (verb, group) => Record.keys(Record.filter(group, (target) => (target.outputs ?? []).length > 0)).map((tool) => `${verb}--${tool}`)).flat();
    const whole: Members = {
        ...members,
        clean: Record.map({ ...(written.length > 0 && { [clean]: {} }), ...cleaning }, (target: TargetConfiguration) => ({
            executor: `${DEVKIT}:clean`,
            ...target,
            options: { ...target.options, targets: written },
            metadata: { description: `Removes the outputs ${written.join(', ')} declare`, ...target.metadata },
        })),
        attach: {
            ...attaching,
            ...(packaged.length > 0 && {
                gh: {
                    executor: `${DEVKIT}:attach`,
                    dependsOn: [...packaged],
                    metadata: { description: `Uploads the files ${packaged.join(', ')} wrote to the GitHub release of the version the project's manifest states through gh release upload`, help: { command: 'gh release upload --help', example: { options: { dryRun: true } } } },
                },
            }),
        },
    };
    const tools = Record.collect(whole, (verb, group) =>
        Record.collect(group, (tool, target) => ({
            verb,
            name: `${verb}--${tool}`,
            target: {
                ...target,
                ...(target.command !== undefined && { options: { cwd: project.root, ...target.options } }),
                ...read(target),
                metadata: { technologies, ...target.metadata },
            },
        })),
    ).flat();
    const tags = [...(project.release === undefined ? [] : ['release']), ...allowed.map((system) => `os:${system}`), ...(project.tags ?? [])];
    return {
        ...project,
        targets:
            os.includes(`!${platform}`) || !(allowed.length === 0 || allowed.includes(platform))
                ? {}
                : {
                      ...Record.fromIterableWith(tools, ({ name, target }): readonly [string, TargetConfiguration] => [name, target]),
                      ...Record.map(
                          Array.groupBy(tools, ({ verb }) => verb),
                          (group, verb): TargetConfiguration => ({
                              executor: 'nx:noop',
                              dependsOn: [`${verb}--*`],
                              inputs: Array.dedupe(['...', ...group.flatMap(({ target }) => target.inputs ?? SOURCES), ...(restated[verb] ?? [])]),
                              outputs: [],
                              metadata: { technologies, description: `Runs every ${verb}--* target` },
                          }),
                      ),
                      ...(publish !== undefined && { 'nx-release-publish': { executor: `${DEVKIT}:publish`, ...publish, dependsOn: [...packaged, '^nx-release-publish'] } }),
                      ...project.targets,
                  },
        ...(tags.length > 0 && { tags }),
    } satisfies ProjectConfiguration;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { ancestors, chained, collected, configured, line, linked, type Members, PLATFORM, placed, produced, type Stated, tooled };

import { findPackageJSON } from 'node:module';
import { matchesGlob } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    AggregateCreateNodesError,
    type CreateDependencies,
    type CreateDependenciesContext,
    type CreateNodes,
    type CreateNodesContext,
    type CreateNodesResultArray,
    detectPackageManager,
    getOutputsForTargetAndConfiguration,
    getPackageManagerCommand,
    hashArray,
    type ProjectConfiguration,
    type ProjectGraphExternalNode,
    parseJson,
    type RawProjectGraphDependency,
    type TargetConfiguration,
} from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, getNamedInputs, globWithWorkspaceContext, hashFile, hashObject, hashWithWorkspaceContext, interpolate, mergeTargetConfigurations, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, type ManagedRuntime, Option, Order, Path, Predicate, Record, Result, Schema, type Scope, UndefinedOr } from 'effect';
import { hashMultiGlobWithWorkspaceContext } from 'nx/src/devkit-internals';
import { runtime, ToolError } from '#/process';
import type { Inputs } from '#/target';

// --- [TYPES] ---------------------------------------------------------------------------

type Verb = 'build' | 'check' | 'fix' | 'test' | 'serve' | 'preview' | 'install' | 'deploy' | 'upgrade' | 'clean';
type Members = { readonly [V in Verb]?: Readonly<Record<string, TargetConfiguration>> };
type Services = ManagedRuntime.ManagedRuntime.Services<typeof runtime> | Scope.Scope;
type Failure = readonly [file: string, error: Error];
type Identity = Effect.Success<ReturnType<typeof identity>>;

interface Project {
    readonly file: string;
    readonly root: string;
    readonly technologies: readonly string[];
    readonly members: Members;
    readonly name?: string;
    readonly clean?: string;
    readonly targets?: Readonly<Record<string, TargetConfiguration>>;
    readonly kinds?: Readonly<Record<string, readonly string[]>>;
    readonly configuration?: Omit<ProjectConfiguration, 'root' | 'name' | 'targets' | 'tags' | 'metadata'>;
    readonly externalNodes?: Readonly<Record<string, ProjectGraphExternalNode>>;
}

interface Unit {
    readonly key: string;
    readonly file: string;
    readonly salt?: string;
}

interface Sources {
    readonly tracked: readonly string[];
    readonly untracked?: readonly string[];
}

interface Reading<R> {
    readonly report: R;
    readonly failures?: readonly Failure[];
    readonly partial?: boolean;
}

interface Request<O> {
    readonly files: readonly string[];
    readonly options: O;
    readonly context: CreateNodesContext;
    readonly plugin: Identity;
    readonly path: Path.Path;
    readonly exec: string;
    readonly placed: (tool: string, name?: string) => Option.Option<string>;
    readonly named: (root: string) => 'production' | 'default';
}

interface Declaration<O, U extends Unit, R, E> {
    readonly module: string;
    readonly files: readonly string[];
    readonly options: Schema.Decoder<O>;
    readonly units: (request: Request<O>) => Effect.Effect<readonly U[], E, Services>;
    readonly read: (missing: Array.NonEmptyReadonlyArray<U>, request: Request<O>) => Effect.Effect<readonly Result.Result<Reading<R>, Error>[], E, Services>;
    readonly sources: (report: R, unit: U, request: Request<O>) => Sources;
    readonly projects: (report: R, unit: U, request: Request<O>) => readonly Project[];
    readonly edges?: (reports: readonly (readonly [U, R])[], context: CreateDependenciesContext, request: Request<O>) => Effect.Effect<readonly RawProjectGraphDependency[], E, Services>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLATFORM = { runtime: 'node -p process.platform+process.arch' } as const;
const UNSTATED = ['default', '^default'] as const satisfies Inputs;
const MANIFEST = 'package.json';

// --- [MODELS] --------------------------------------------------------------------------

const Dependencies = Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));
const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String, version: Schema.String, dependencies: Dependencies, optionalDependencies: Dependencies }));
const Row = Schema.Union([Schema.Literal(false), Schema.declare((value: unknown): value is TargetConfiguration & { readonly name?: string } => Predicate.isObject(value) && (!Predicate.hasProperty(value, 'name') || Predicate.isString(value.name)))]);
const Common = Schema.Struct({
    outputPath: Schema.optionalKey(Schema.String),
    targets: Schema.Record(Schema.String, Row).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    tags: Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
}).annotate({
    description:
        'Options every plugin row reads, each defaulting to the value under the devkit package name in nx.json pluginsConfig. outputPath: workspace folder a target writes generated output under as <outputPath>/<tool>/<project name>, absent leaves each tool its own folder. targets: inferred target name or glob to false (removes it) or a target configuration Nx merges over it, its name renaming it. tags: kind a plugin states for a project (os, host, release) to the tag it takes, {<kind>} standing for the value',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const ancestors = (path: Path.Path, folder: string): readonly string[] =>
    Array.unfold(
        Option.some(folder),
        Option.map((at: string) => [at, Option.liftPredicate(path.dirname(at), () => at !== '.')] as const),
    );

const packaged = Effect.fnUntraced(function* (entry: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const file = yield* Effect.fromNullishOr(findPackageJSON(entry));
    return { file, folder: path.dirname(file), ...(yield* Effect.flatMap(fs.readFileString(file), Schema.decodeEffect(Manifest))) };
});

const identity = Effect.fn('identity')(function* (entry: string, workspaceRoot: string) {
    const path = yield* Path.Path;
    const [plugin, shared] = yield* Effect.all([packaged(entry), packaged(import.meta.url)]);
    const packages = [plugin.name, ...Record.keys(plugin.optionalDependencies), shared.name];
    const modules = (url: string) => path.join(path.relative(workspaceRoot, path.dirname(fileURLToPath(url))), `**/*${path.extname(url)}`);
    const source = yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [modules(entry), modules(import.meta.url)]));
    const external = Array.difference(Array.dedupe([plugin, shared].flatMap((manifest) => Record.keys(manifest.dependencies))), packages);
    const inputs: Inputs = plugin.file.split(path.sep).includes('node_modules') ? [{ externalDependencies: packages }] : [{ input: 'default', projects: packages }, { externalDependencies: external }];
    return { name: plugin.name, folder: plugin.folder, shared: shared.name, digest: hashArray([source, plugin.version, shared.version]), inputs, cleaner: `${shared.name}:clean`, publisher: `${shared.name}:publish` };
});

const manifested = Effect.fnUntraced(function* (workspaceRoot: string, roots: readonly string[]) {
    const path = yield* Path.Path;
    const found = yield* Effect.promise(() =>
        globWithWorkspaceContext(
            workspaceRoot,
            roots.map((root) => path.join(root, MANIFEST)),
        ),
    );
    return new Set(found.map((file) => path.dirname(file)));
});

const listed = Effect.fnUntraced(function* (workspaceRoot: string, roots: readonly string[]) {
    const path = yield* Path.Path;
    const files = yield* Effect.promise(() =>
        globWithWorkspaceContext(
            workspaceRoot,
            roots.map((root) => path.join(root, '**')),
        ),
    );
    const owners = Array.sort(
        Array.dedupe([...roots, ...files.filter((file) => path.basename(file) === MANIFEST).map((file) => path.dirname(file))]),
        Order.mapInput(Order.flip(Order.Number), (folder: string) => folder.length),
    );
    const owned = Array.groupBy(files, (file) => owners.find((folder) => folder === '.' || file.startsWith(`${folder}/`)) ?? '.');
    return new Map(roots.map((root) => [root, hashArray(owned[root] ?? [])] as const));
});

const produced = (projects: CreateDependenciesContext['projects']) => {
    const targets = Record.toEntries(projects).flatMap(([name, data]) => Record.keys(Record.filter(data.targets ?? {}, (target) => target.outputs !== undefined)).map((target) => ({ name, target, data })));
    const selections = targets.flatMap(({ name, target, data }) => [{}, ...Record.keys(data.targets?.[target]?.configurations ?? {}).map((configuration) => ({ configuration }))].map((selected) => ({ name, target, data, selected })));
    return selections.flatMap(({ name, target, data, selected }) =>
        getOutputsForTargetAndConfiguration({ project: name, target, ...selected }, {}, { name, type: 'lib', data })
            .filter((output) => !output.startsWith('!'))
            .map((output) => ({ root: data.root, output })),
    );
};

const detached = <A extends object>(value: A) => parseJson<A>(JSON.stringify(value));

const targeted = (project: Project, plugin: Identity, rows: (typeof Common.Type)['targets']): Readonly<Record<string, TargetConfiguration>> => {
    const technologies = [...project.technologies];
    const applied = ({ verb, old, target }: { readonly verb: string; readonly old: string; readonly target: TargetConfiguration }) => {
        const matched = Record.collect(
            Record.filter(rows, (_, pattern) => pattern === old || matchesGlob(old, pattern)),
            (_, row) => row,
        );
        const merged = matched.filter((row) => row !== false);
        return matched.length > merged.length
            ? Result.failVoid
            : Result.succeed({
                  verb,
                  old,
                  name: Option.getOrElse(
                      Array.findLast(
                          merged.map(({ name }) => name),
                          Predicate.isNotUndefined,
                      ),
                      () => old,
                  ),
                  target: merged.reduce((base, { name: _, ...row }) => mergeTargetConfigurations(row, base, undefined, undefined, undefined, false), target),
              });
    };
    const stated = project.clean === undefined ? undefined : project.members.clean?.[project.clean];
    const tools = Array.filterMap(
        Object.entries(project.members)
            .flatMap(([verb, group]) => Record.collect(group, (tool, target) => ({ verb, tool, old: `${verb}--${tool}`, target })))
            .filter(({ verb, tool }) => !(verb === 'clean' && tool === project.clean))
            .map(({ verb, old, target }) => ({ verb, old, target: { ...target, ...(target.cache === true && { inputs: Array.dedupe([...(target.inputs ?? UNSTATED), PLATFORM]) }), metadata: { technologies, ...target.metadata } } })),
        applied,
    );
    const written = tools.filter(({ target }) => (target.outputs ?? []).length > 0).map(({ name }) => name);
    const cleaning =
        project.clean === undefined || (written.length === 0 && stated === undefined)
            ? []
            : Array.filterMap(
                  [
                      {
                          verb: 'clean',
                          old: `clean--${project.clean}`,
                          target: Array.match(written, {
                              onEmpty: (): TargetConfiguration => ({ ...stated, metadata: { technologies, ...stated?.metadata } }),
                              onNonEmpty: (targets): TargetConfiguration => ({ ...stated, executor: plugin.cleaner, options: { ...stated?.options, targets }, metadata: { technologies, description: `Removes the outputs ${targets.join(', ')} declare`, ...stated?.metadata } }),
                          }),
                      },
                  ],
                  applied,
              );
    const members = [...tools, ...cleaning];
    const aggregates = Record.collect(
        Array.groupBy(members, ({ verb }) => verb),
        (verb, group) => ({ verb, old: verb, target: { executor: 'nx:noop', dependsOn: [`${verb}--*`], inputs: ['...', ...Array.dedupe(group.flatMap(({ target }) => target.inputs ?? UNSTATED))], metadata: { technologies, description: `Runs every ${verb}--* target` } } satisfies TargetConfiguration }),
    );
    const settled = [...members, ...Array.filterMap([...aggregates, ...Record.collect(project.targets ?? {}, (old, target) => ({ verb: old, old, target }))], applied)];
    const renames = new Map(settled.map(({ old, name }) => [old, name] as const));
    const linked = (to: string) => (to.startsWith('^') ? `^${renames.get(to.slice(1)) ?? to.slice(1)}` : (renames.get(to) ?? to));
    return Record.fromIterableWith(settled, ({ verb, old, name, target }): readonly [string, TargetConfiguration] => [
        name,
        target.dependsOn === undefined
            ? target
            : {
                  ...target,
                  dependsOn: [
                      ...target.dependsOn.map((dependency) => (Predicate.isString(dependency) ? linked(dependency) : { ...dependency, target: linked(dependency.target) })),
                      ...settled.filter((member) => old === verb && member.verb === verb && member.old !== verb && !matchesGlob(member.name, `${verb}--*`)).map((member) => member.name),
                  ],
              },
    ]);
};

const configured = (project: Project, plugin: Identity, { tags: templates, targets }: typeof Common.Type): Omit<ProjectConfiguration, 'root'> => {
    const tags = Record.collect(templates, (kind, template) => (project.kinds?.[kind] ?? []).map((value) => interpolate(template, { [kind]: value }))).flat();
    return { ...(project.name !== undefined && { name: project.name }), metadata: { technologies: [...project.technologies] }, targets: targeted(project, plugin, targets), ...(tags.length > 0 && { tags }), ...project.configuration };
};

const stamped = Effect.fnUntraced(function* <O, U extends Unit, R, E>(declaration: Declaration<O, U, R, E>, request: Request<O>, held: readonly (readonly [U, R])[]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const base = hashArray([request.plugin.digest, hashObject(request.context.nxJsonConfiguration.namedInputs ?? {})]);
    const sources = held.map(([unit, report]) => declaration.sources(report, unit, request));
    const tracked = Array.isReadonlyArrayNonEmpty(held)
        ? yield* Effect.promise(() =>
              hashMultiGlobWithWorkspaceContext(
                  request.context.workspaceRoot,
                  sources.map((source) => [...source.tracked]),
              ),
          )
        : [];
    const folders = Array.dedupe(sources.flatMap((source) => source.untracked ?? []));
    const names = yield* Effect.forEach(folders, (folder) => Effect.orElseSucceed(fs.readDirectory(folder), Array.empty<string>), { concurrency: 'unbounded' });
    const hashed = new Map(Array.zipWith(folders, names, (folder, entries) => [folder, entries.toSorted().flatMap((entry) => [path.join(folder, entry), ...Option.toArray(Option.fromNullishOr(hashFile(path.join(folder, entry))))])] as const));
    const untracked = sources.map((source) => hashArray((source.untracked ?? []).flatMap((folder) => hashed.get(folder) ?? [])));
    return Array.zipWith(Array.zip(held, tracked), untracked, ([[unit], content], files) => hashArray([base, unit.key, content, files, ...Option.toArray(Option.fromUndefinedOr(unit.salt))]));
});

const read = Effect.fnUntraced(function* <O, U extends Unit, R, E>(declaration: Declaration<O, U, R, E>, request: Request<O>, settings: object, units: readonly U[]) {
    const path = yield* Path.Path;
    const cache = new PluginCache<{ readonly report: R; readonly digest: string }>(path.join(workspaceDataDirectory, `${path.basename(request.plugin.name)}-${hashObject(settings)}.hash`));
    const stored = Array.filterMap(units, (unit) =>
        Result.map(
            Result.fromNullishOr(cache.get(unit.key), () => unit),
            (entry) => [unit, entry] as const,
        ),
    );
    const now = yield* stamped(
        declaration,
        request,
        stored.map(([unit, { report }]) => [unit, report] as const),
    );
    const valid = new Map(Array.filterMap(Array.zip(stored, now), ([[unit, entry], digest]) => (entry.digest === digest ? Result.succeed([unit.key, Result.succeed<Reading<R>>({ report: entry.report })] as const) : Result.failVoid)));
    const missing = units.filter((unit) => !valid.has(unit.key));
    const fresh = Array.isReadonlyArrayNonEmpty(missing) ? Array.zip(missing, yield* declaration.read(missing, request)) : [];
    const complete = Array.filterMap(fresh, ([unit, outcome]) => Result.flatMap(outcome, ({ report, failures = [], partial = false }) => (failures.length === 0 && !partial ? Result.succeed([unit, report] as const) : Result.failVoid)));
    const digests = yield* stamped(declaration, request, complete);
    yield* Effect.sync(() => Array.zip(complete, digests).forEach(([[unit, report], digest]) => cache.set(unit.key, { report, digest })));
    yield* Array.isReadonlyArrayNonEmpty(complete) ? Effect.sync(() => cache.writeToDisk()) : Effect.void;
    const outcomes = new Map<string, Result.Result<Reading<R>, Error>>([...valid, ...fresh.map(([unit, outcome]) => [unit.key, outcome] as const)]);
    return units.map((unit) => [unit, outcomes.get(unit.key) ?? Result.fail(ToolError.make({ subject: unit.file, detail: 'was not read' }))] as const);
});

const nodes = Effect.fnUntraced(function* <O, U extends Unit, R, E>(declaration: Declaration<O, U, R, E>, memo: Map<string, { readonly request: Request<O>; readonly reports: readonly (readonly [U, R])[] }>, files: readonly string[], options: unknown, context: CreateNodesContext) {
    const path = yield* Path.Path;
    const plugin = yield* identity(declaration.module, context.workspaceRoot);
    const settings = { ...context.nxJsonConfiguration.pluginsConfig?.[plugin.shared], ...(Predicate.isObject(options) ? options : {}) };
    const [common, own] = yield* Effect.all([Schema.decodeEffect(Common)(settings), Schema.decodeEffect(declaration.options)(settings)]);
    const request: Request<O> = {
        files: files.toSorted(),
        options: own,
        context,
        plugin,
        path,
        exec: getPackageManagerCommand(detectPackageManager(context.workspaceRoot)).exec,
        placed: (tool, name = '{projectName}') => Option.map(Option.fromUndefinedOr(common.outputPath), (at) => path.join(at, tool, name)),
        named: (root) => ('production' in getNamedInputs(root, context) ? 'production' : 'default'),
    };
    const readings = yield* read(declaration, request, settings, yield* declaration.units(request));
    const reports = Array.filterMap(readings, ([unit, outcome]) => Result.map(outcome, ({ report }) => [unit, report] as const));
    memo.set(hashArray([context.workspaceRoot, hashObject(options ?? {})]), { request, reports });
    const failures = readings.flatMap(([unit, outcome]) => Result.match(outcome, { onFailure: (error): readonly Failure[] => [[unit.file, error]], onSuccess: ({ failures: partial = [] }) => partial }));
    const results: CreateNodesResultArray = Record.toEntries(
        detached(
            Record.map(
                Array.groupBy(
                    reports.flatMap(([unit, report]) => declaration.projects(report, unit, request)),
                    (project) => project.file,
                ),
                (group) => ({
                    projects: Record.fromIterableWith(group, (project) => [project.root, configured(project, plugin, common)] as const),
                    externalNodes: Record.fromEntries(group.flatMap((project) => Record.toEntries(project.externalNodes ?? {}))),
                }),
            ),
        ),
    );
    return yield* Array.match(failures, {
        onEmpty: () => Effect.succeed(results),
        onNonEmpty: (errors) =>
            Effect.fail(
                new AggregateCreateNodesError(
                    errors.map(([file, error]) => [file, error]),
                    results,
                ),
            ),
    });
});

const dependencies = Effect.fnUntraced(function* <O, U extends Unit, R, E>(declaration: Declaration<O, U, R, E>, held: { readonly request: Request<O>; readonly reports: readonly (readonly [U, R])[] }, context: CreateDependenciesContext) {
    const names = createProjectRootMappingsFromProjectConfigurations(context.projects);
    const named = (at: string) => Option.orElse(Option.fromUndefinedOr(names.get(at)), () => Option.liftPredicate(at, (id) => id in (context.externalNodes ?? {})));
    const edges = declaration.edges === undefined ? [] : yield* declaration.edges(held.reports, context, held.request);
    const linked = Array.getSomes(edges.map((edge) => Option.map(Option.all([named(edge.source), named(edge.target)]), ([source, target]): RawProjectGraphDependency => ({ ...edge, source, target })))).filter(({ source, target }) => source !== target);
    return [...new Map(linked.map((edge) => [hashArray([edge.source, edge.target, edge.type]), edge] as const)).values()];
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const inferred = <O, U extends Unit, R, E extends Error>(declaration: Declaration<O, U, R, E>): { readonly createNodes: CreateNodes; readonly createDependencies: CreateDependencies } => {
    const memo = new Map<string, { readonly request: Request<O>; readonly reports: readonly (readonly [U, R])[] }>();
    return {
        createNodes: [combineGlobPatterns(declaration.files.map((file) => `**/${file}`)), (files, options, context) => runtime.runPromise(Effect.scoped(nodes(declaration, memo, files, options, context)))],
        createDependencies: (options, context) =>
            UndefinedOr.match(memo.get(hashArray([context.workspaceRoot, hashObject(options ?? {})])), {
                onUndefined: () => [],
                onDefined: (held) => runtime.runPromise(Effect.scoped(dependencies(declaration, held, context))),
            }),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { ancestors, type Declaration, type Identity, inferred, listed, MANIFEST, type Members, manifested, type Project, produced, type Reading, type Request, type Sources, type Unit };

import { layer } from '@effect/platform-node/NodeServices';
import { cruise, type IConfiguration, type ICruiseOptions, type IReachabilityForbiddenRuleType, type IRegularForbiddenRuleType } from 'dependency-cruiser';
import { Array, Effect, FileSystem, Iterable, Option, Path, Record, Result, Struct, Tuple } from 'effect';
import { extendScriptEntry, Installed, libraryRoot, members, Package } from '#/eng/typescript/manifest';

// --- [TYPES] ---------------------------------------------------------------------------

type Install = Effect.Success<ReturnType<typeof install>>;
type Rule = readonly [string, string, IRegularForbiddenRuleType['from'], IRegularForbiddenRuleType['to']];

// --- [CONSTANTS] -----------------------------------------------------------------------

const RUNTIME_PACKAGES = ['@effect/platform-[^/]+/', '@effect/[^/]+-(?:browser|bun|node)/', 'react-dom/(?:client|profiling|server|static)\\b'];
const NESTED = ['**/node_modules/**'];
const INSTALLED = /(?:^|\/)node_modules\//u;
const STATIC_IMPORTS = {
    exclude: { dynamic: true },
    enhancedResolveOptions: { exportsFields: ['exports'], conditionNames: ['import'] },
    skipAnalysisNotInRules: true,
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const library = Effect.fnUntraced(function* (directory: string) {
    const [fs, path, manifest] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Package.read(directory)]);
    const root = libraryRoot(manifest);
    if (Option.isNone(root)) {
        return Option.none();
    }
    const script = extendScriptEntry(manifest);
    const dependencies = yield* Effect.forEach(Record.keys({ ...manifest.dependencies, ...manifest.devDependencies }), (name) => fs.realPath(path.join(directory, 'node_modules', name)), { concurrency: 'unbounded' });
    const pattern = (entry: string) => {
        const file = path.join(directory, entry);
        return path.basename(file) === 'index.ts' ? `^${RegExp.escape(path.dirname(file))}/` : `^${RegExp.escape(file)}$`;
    };
    return Option.some({
        dependencies,
        sources: [root.value, ...Option.toArray(script)].map((file) => path.join(directory, file)),
        entries: Object.values(Record.remove(manifest.exports, '.')).map(pattern),
        extendscript: Option.map(script, pattern),
    });
});
const install = Effect.fnUntraced(function* (real: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const [{ name, dependencies, optionalDependencies, peerDependencies, napi }, binaries] = yield* Effect.all([Installed.read(real), fs.glob('**/{*.wasm,*.node,binding.gyp}', { root: real, exclude: NESTED })]);
    const near = path.join(real, ...name.split('/').map(() => '..'));
    const declared = [...Record.keys({ ...dependencies, ...optionalDependencies }).map((dependency) => Tuple.make(dependency, false)), ...Record.keys(peerDependencies).map((dependency) => Tuple.make(dependency, true))];
    const present = yield* Effect.filter(declared, ([dependency]) => fs.exists(path.join(near, dependency)), { concurrency: 'unbounded' });
    const edges = yield* Effect.forEach(present, ([dependency, peer]) => Effect.all([fs.realPath(path.join(near, dependency)), Effect.succeed(dependency), Effect.succeed(peer)]), { concurrency: 'unbounded' });
    return { real, ships: Option.isSome(napi) || Array.isReadonlyArrayNonEmpty(binaries), edges };
});
const walk = (seen: ReadonlyMap<string, Install>, pending: readonly string[]): Effect.Effect<ReadonlyMap<string, Install>, Effect.Error<ReturnType<typeof install>>, Effect.Services<ReturnType<typeof install>>> =>
    Array.match(Array.dedupe(pending.filter((real) => !seen.has(real))), {
        onEmpty: () => Effect.succeed(seen),
        onNonEmpty: (fresh) => Effect.flatMap(Effect.forEach(fresh, install, { concurrency: 'unbounded' }), (installs) => walk(new Map([...seen, ...installs.map((held) => Tuple.make(held.real, held))]), installs.flatMap(Struct.get('edges')).map(Tuple.get(0)))),
    });
const named = Effect.fnUntraced(function* ({ real, edges }: Install, reaching: ReadonlySet<string>) {
    const candidates = edges.filter(([target]) => reaching.has(target));
    if (candidates.every(([, , peer]) => peer)) {
        return Tuple.make(real, candidates.map(Tuple.get(0)));
    }
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const files = yield* fs.glob('**/*.{js,mjs,cjs}', { root: real, exclude: NESTED });
    const texts = yield* Effect.forEach(files, (file) => fs.readFileString(path.join(real, file)), { concurrency: 'unbounded' });
    const mentioned = ([, name, peer]: Install['edges'][number]) => peer || texts.some(RegExp.prototype.test.bind(new RegExp(`(['"])${RegExp.escape(name)}(?:/[^'"]*)?\\1`, 'u')));
    return Tuple.make(real, candidates.filter(mentioned).map(Tuple.get(0)));
});
const closure = (held: ReadonlySet<string>, edges: readonly (readonly [string, readonly string[]])[]): ReadonlySet<string> => {
    const incoming = Map.groupBy(
        edges.flatMap(([source, targets]) => targets.map((target) => Tuple.make(target, source))),
        Tuple.get(0),
    );
    const reached = new Set(held);
    Iterable.forEach(reached, (target) => Array.forEach(incoming.get(target) ?? [], ([, source]) => reached.add(source)));
    return reached;
};
const analyzed = (sources: string[], policy: ICruiseOptions) =>
    Effect.promise(() => cruise(sources, { ...STATIC_IMPORTS, ...policy })).pipe(
        Effect.map(Struct.get('output')),
        Effect.filterOrFail((output) => typeof output !== 'string'),
    );
const builtinLoaders = Effect.fnUntraced(function* (files: string[]) {
    if (files.length === 0) {
        return new Set<string>();
    }
    const { modules, summary } = yield* analyzed(files, {
        doNotFollow: { path: `^(?!.*${INSTALLED.source})`, dependencyTypes: ['dynamic-import'] },
        validate: true,
        ruleSet: { forbidden: [{ from: {}, to: { dependencyTypes: ['core'] } }] },
    });
    return closure(
        new Set(summary.violations.map(Struct.get('from'))),
        modules.map(({ source, dependencies }) => Tuple.make(source, dependencies.map(Struct.get('resolved')))),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const [entryPatterns, scriptPatterns, bound, loaders, firstParty, resolution] = await Effect.gen(function* () {
    const [path, manifests] = yield* Effect.all([Path.Path, members('.', Package.file)]);
    const libraries = Array.getSomes(yield* Effect.forEach(manifests.map(path.dirname), library, { concurrency: 'unbounded' }));
    const entries = libraries.flatMap(Struct.get('entries'));
    const extendscript = Array.getSomes(libraries.map(Struct.get('extendscript')));
    const options = { ...STATIC_IMPORTS, doNotFollow: { path: [INSTALLED.source, ...Array.difference(entries, extendscript)], dependencyTypes: ['dynamic-import'] }, detectProcessBuiltinModuleCalls: true } satisfies ICruiseOptions;
    const { modules } = yield* analyzed(libraries.flatMap(Struct.get('sources')), options);
    const relative = (real: string) => path.relative('.', real).replaceAll(path.sep, '/');
    const declared = libraries.flatMap(Struct.get('dependencies')).filter((real) => INSTALLED.test(relative(real)));
    const directories = new Map(declared.map((real) => [relative(real), real] as const));
    const packageDirectory = new RegExp(`^(${[...directories.keys()].map(RegExp.escape).join('|')})/`, 'u');
    const vendor = Array.dedupe(modules.flatMap(({ dependencies }) => dependencies.map(Struct.get('resolved'))));
    const used = Array.filterMap(vendor, (file) =>
        Result.map(
            Result.fromNullishOr(directories.get(packageDirectory.exec(file)?.[1] ?? ''), () => undefined),
            (real) => Tuple.make(file, real),
        ),
    );
    const paths = Array.dedupe(used.map(Tuple.get(1)));
    const installs = [...(yield* walk(new Map(), paths)).values()];
    const shipping = new Set(installs.filter(Struct.get('ships')).map(Struct.get('real')));
    const reaching = closure(
        shipping,
        installs.map(({ real, edges }) => Tuple.make(real, edges.map(Tuple.get(0)))),
    );
    const loading = closure(
        shipping,
        yield* Effect.forEach(
            installs.filter(({ real }) => reaching.has(real) && !shipping.has(real)),
            (held) => named(held, reaching),
            { concurrency: 'unbounded' },
        ),
    );
    const loaded = yield* builtinLoaders(used.filter(([, real]) => !loading.has(real)).map(Tuple.get(0)));
    return [entries, extendscript, paths.filter((real) => loading.has(real)).map((real) => `^${RegExp.escape(relative(real))}/`), [...loaded].map((file) => `^${RegExp.escape(file)}$`), manifests.map(path.dirname).map((directory) => `^${RegExp.escape(directory)}/`), options] as const;
}).pipe(Effect.provide(layer), Effect.runPromise);

export default {
    forbidden: [
        ...(
            [
                ['host-module', 'a builtin, or a module no installed package exports under the import condition', {}, { dependencyTypes: ['core', 'unknown'] }],
                ['runtime-package', 'a package whose installed load chain ships wasm, a Node addon, or loads a builtin, or a runtime-bound package', {}, { path: [`node_modules/(?:${RUNTIME_PACKAGES.join('|')})`, ...bound, ...loaders] }],
                ['asset', 'a stylesheet or other file no runtime loads as a module', {}, { pathNot: '\\.(?:[cm]?[jt]sx?|json)$', dependencyTypesNot: ['core', 'unknown'] }],
                ...Array.match(entryPatterns, { onEmpty: () => [], onNonEmpty: (paths): readonly Rule[] => [['library-entry', 'another entry of a library', { pathNot: [...paths] }, { path: [...paths] }]] }),
            ] satisfies readonly Rule[]
        ).map(
            ([kind, loads, from, to]): IRegularForbiddenRuleType => ({
                name: `${kind}-in-root-entry`,
                comment: `A library root entry static closure loads in every runtime importing its package: a module loading ${loads} belongs in the entry named for the runtime or dependency it needs`,
                severity: 'error',
                from,
                to,
            }),
        ),
        ...Array.match(scriptPatterns, {
            onEmpty: () => [],
            onNonEmpty: (paths): readonly IReachabilityForbiddenRuleType[] => [
                {
                    name: 'package-in-extendscript-entry',
                    comment: 'An ExtendScript entry bundles into one ES3 script the host evaluates with no module loader or ES2015 built-ins: its static closure loads first-party modules alone',
                    severity: 'error',
                    from: { path: [...paths] },
                    to: { pathNot: [...firstParty], reachable: true },
                },
            ],
        }),
    ],
    options: resolution,
} satisfies IConfiguration;

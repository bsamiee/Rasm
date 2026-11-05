import { layer } from '@effect/platform-node/NodeServices';
import { cruise, type IConfiguration, type ICruiseResult, type IReachabilityForbiddenRuleType, type IRegularForbiddenRuleType } from 'dependency-cruiser';
import { Array, Effect, FileSystem, Option, Path, Record, Struct, Tuple } from 'effect';
import { extendScriptEntry, Installed, libraryRoot, members, Package } from '#/eng/typescript/manifest';

// --- [TYPES] ---------------------------------------------------------------------------

type Install = Effect.Success<ReturnType<typeof install>>;
type Rule = readonly [string, string, IRegularForbiddenRuleType['from'], IRegularForbiddenRuleType['to']];

// --- [CONSTANTS] -----------------------------------------------------------------------

const RUNTIME_PACKAGES = ['@effect/platform-[^/]+/', '@effect/[^/]+-(?:browser|bun|node)/', 'react-dom/(?:client|profiling|server|static)\\b'];
const NESTED = ['**/node_modules/**'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const library = Effect.fnUntraced(function* (directory: string) {
    const [fs, path, manifest] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Package.read(directory)]);
    const pattern = (entry: string) => {
        const file = path.join(directory, entry);
        return path.basename(file) === 'index.ts' ? `^${RegExp.escape(path.dirname(file))}/` : `^${RegExp.escape(file)}$`;
    };
    return yield* Effect.transposeOption(
        Option.as(
            libraryRoot(manifest),
            Effect.map(
                Effect.forEach(Record.keys({ ...manifest.dependencies, ...manifest.devDependencies }), (name) => Effect.all([fs.realPath(path.join(directory, 'node_modules', name)), Effect.succeed(name)]), { concurrency: 'unbounded' }),
                (rows) => ({ rows, entries: Object.values(Record.remove(manifest.exports, '.')).map(pattern), extendscript: Option.map(extendScriptEntry(manifest), pattern) }),
            ),
        ),
    );
});
const install = Effect.fnUntraced(function* ([real, name]: readonly [string, string]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const near = path.join(real, ...name.split('/').map(() => '..'));
    const [{ dependencies, peerDependencies, napi }, binaries] = yield* Effect.all([Installed.read(real), fs.glob('**/{*.wasm,*.node,binding.gyp}', { root: real, exclude: NESTED })]);
    const declared = [...Record.keys(dependencies).map((dependency) => Tuple.make(dependency, false)), ...Record.keys(peerDependencies).map((dependency) => Tuple.make(dependency, true))];
    const present = yield* Effect.filter(declared, ([dependency]) => fs.exists(path.join(near, dependency)), { concurrency: 'unbounded' });
    const edges = yield* Effect.forEach(present, ([dependency, peer]) => Effect.all([fs.realPath(path.join(near, dependency)), Effect.succeed(dependency), Effect.succeed(peer)]), { concurrency: 'unbounded' });
    return { real, ships: Option.isSome(napi) || Array.isReadonlyArrayNonEmpty(binaries), edges };
});
const walk = (seen: ReadonlyMap<string, Install>, pending: readonly (readonly [string, string])[]): Effect.Effect<ReadonlyMap<string, Install>, Effect.Error<ReturnType<typeof install>>, Effect.Services<ReturnType<typeof install>>> =>
    Array.match(
        Array.dedupeWith(
            pending.filter(([real]) => !seen.has(real)),
            (left, right) => left[0] === right[0],
        ),
        {
            onEmpty: () => Effect.succeed(seen),
            onNonEmpty: (fresh) =>
                Effect.flatMap(Effect.forEach(fresh, install, { concurrency: 'unbounded' }), (installs) =>
                    walk(
                        new Map([...seen, ...installs.map((held) => Tuple.make(held.real, held))]),
                        installs.flatMap(Struct.get('edges')).map(([target, name]) => Tuple.make(target, name)),
                    ),
                ),
        },
    );
const named = Effect.fnUntraced(function* ({ real, edges }: Install, reaching: ReadonlySet<string>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const files = yield* fs.glob('**/*.{js,mjs,cjs}', { root: real, exclude: NESTED });
    const texts = yield* Effect.forEach(files, (file) => fs.readFileString(path.join(real, file)), { concurrency: 'unbounded' });
    const mentioned = new Set(
        Array.cartesian(edges, texts)
            .filter(([[, name], text]) => new RegExp(`(['"])${RegExp.escape(name)}(?:/[^'"]*)?\\1`, 'u').test(text))
            .map(([[target]]) => target),
    );
    return Tuple.make(real, edges.filter(([target, , peer]) => reaching.has(target) && (peer || mentioned.has(target))).map(Tuple.get(0)));
});
const closure = (held: ReadonlySet<string>, edges: readonly (readonly [string, readonly string[]])[]): ReadonlySet<string> => {
    const grown = new Set([...held, ...edges.filter(([, targets]) => targets.some((target) => held.has(target))).map(Tuple.get(0))]);
    return grown.size === held.size ? held : closure(grown, edges);
};
const builtinLoaders = Effect.fnUntraced(function* (real: string) {
    const path = yield* Path.Path;
    const directory = path.relative('.', real);
    const { modules, summary } = yield* Effect.promise(() =>
        cruise([directory], {
            doNotFollow: { path: `^(?!${RegExp.escape(directory)}/)`, dependencyTypes: ['dynamic-import'] },
            exclude: { dynamic: true },
            enhancedResolveOptions: { exportsFields: ['exports'], conditionNames: ['import'] },
            validate: true,
            ruleSet: { forbidden: [{ from: {}, to: { dependencyTypes: ['core'] } }] },
        }),
    ).pipe(
        Effect.map(Struct.get('output')),
        Effect.filterOrFail((output): output is ICruiseResult => typeof output !== 'string'),
    );
    return closure(
        new Set(summary.violations.map(Struct.get('from'))),
        modules.map(({ source, dependencies }) => Tuple.make(source, dependencies.map(Struct.get('resolved')))),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const [entries, extendscript, bound, loaders, firstParty] = await Effect.gen(function* () {
    const [path, manifests] = yield* Effect.all([Path.Path, members('.', Package.file)]);
    const libraries = Array.getSomes(yield* Effect.forEach(manifests.map(path.dirname), library, { concurrency: 'unbounded' }));
    const rows = libraries.flatMap(Struct.get('rows')).filter(([real]) => real.includes('/node_modules/'));
    const installs = [...(yield* walk(new Map(), rows)).values()];
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
    const loaded = yield* Effect.forEach(Array.dedupe(rows.map(Tuple.get(0)).filter((real) => !loading.has(real))), builtinLoaders);
    return [
        libraries.flatMap(Struct.get('entries')),
        Array.getSomes(libraries.map(Struct.get('extendscript'))),
        Array.dedupe(rows.filter(([real]) => loading.has(real)).map(Tuple.get(1))),
        loaded.flatMap((files) => [...files]).map((file) => `^${RegExp.escape(file)}$`),
        manifests.map(path.dirname).map((directory) => `^${RegExp.escape(directory)}/`),
    ] as const;
}).pipe(Effect.provide(layer), Effect.runPromise);

export default {
    forbidden: [
        ...(
            [
                ['host-module', 'a builtin, or a module no installed package exports under the import condition', {}, { dependencyTypes: ['core', 'unknown'] }],
                ['runtime-package', 'a package whose installed load chain ships wasm, a Node addon, or loads a builtin, or a runtime-bound package', {}, { path: [`node_modules/(?:${[...bound.map((name) => `${RegExp.escape(name)}/`), ...RUNTIME_PACKAGES].join('|')})`, ...loaders] }],
                ['asset', 'a stylesheet or other file no runtime loads as a module', {}, { pathNot: '\\.(?:[cm]?[jt]sx?|json)$', dependencyTypesNot: ['core', 'unknown'] }],
                ...Array.match(entries, { onEmpty: () => [], onNonEmpty: (paths): readonly Rule[] => [['library-entry', 'another entry of a library', { pathNot: [...paths] }, { path: [...paths] }]] }),
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
        ...Array.match(extendscript, {
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
    options: {
        doNotFollow: { path: ['node_modules', ...Array.difference(entries, extendscript)], dependencyTypes: ['dynamic-import'] },
        exclude: { dynamic: true },
        detectProcessBuiltinModuleCalls: true,
        enhancedResolveOptions: { exportsFields: ['exports'], conditionNames: ['import'] },
        skipAnalysisNotInRules: true,
    },
} satisfies IConfiguration;

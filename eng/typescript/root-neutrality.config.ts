import { layer } from '@effect/platform-node/NodeServices';
import type { IConfiguration, ICruiseOptions, IReachabilityForbiddenRuleType, IRegularForbiddenRuleType } from 'dependency-cruiser';
import { Array, Effect, Option, Path, Record, Struct } from 'effect';
import { extendScriptEntry, libraryRoot, members, Package } from '#/eng/typescript/manifest';

// --- [TYPES] ---------------------------------------------------------------------------

type Rule = readonly [string, string, IRegularForbiddenRuleType['from'], IRegularForbiddenRuleType['to']];

// --- [OPERATIONS] ----------------------------------------------------------------------

const library = Effect.fnUntraced(function* (directory: string) {
    const [path, manifest] = yield* Effect.all([Path.Path, Package.read(directory)]);
    const pattern = (entry: string) => {
        const file = path.join(directory, entry);
        return path.basename(file) === 'index.ts' ? `^${RegExp.escape(path.dirname(file))}/` : `^${RegExp.escape(file)}$`;
    };
    return Option.as(libraryRoot(manifest), {
        entries: Object.values(Record.remove(manifest.exports, '.')).map(pattern),
        extendscript: Option.map(extendScriptEntry(manifest), pattern),
    });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const [entryPatterns, scriptPatterns, firstParty, resolution] = await Effect.gen(function* () {
    const [path, manifests] = yield* Effect.all([Path.Path, members('.', Package.file)]);
    const libraries = Array.getSomes(yield* Effect.forEach(manifests.map(path.dirname), library, { concurrency: 'unbounded' }));
    const entries = libraries.flatMap(Struct.get('entries'));
    const extendscript = Array.getSomes(libraries.map(Struct.get('extendscript')));
    return [
        entries,
        extendscript,
        manifests.map((manifest) => `^${RegExp.escape(path.dirname(manifest))}/`),
        {
            exclude: { dynamic: true },
            enhancedResolveOptions: { exportsFields: ['exports'], conditionNames: ['import'] },
            skipAnalysisNotInRules: true,
            doNotFollow: { path: Array.difference(entries, extendscript), dependencyTypes: ['dynamic-import'] },
            cache: { folder: '.cache/dependency-cruiser', strategy: 'content', compress: true },
        } satisfies ICruiseOptions,
    ] as const;
}).pipe(Effect.provide(layer), Effect.runPromise);

export default {
    forbidden: [
        ...(
            [
                ['host-module', 'a builtin, or a module no installed package exports under the import condition', {}, { dependencyTypes: ['core', 'unknown'] }],
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

import { FileSystem, Path } from '@effect/platform';
import { NodeContext } from '@effect/platform-node';
import { describe, expect, it, layer } from '@effect/vitest';
import { Array, Effect, Option, Order, Record, Schema } from 'effect';

// --- [CONSTANTS] -------------------------------------------------------------------------

const _HOME = new URL('../..', import.meta.url).pathname;
const _ROOT = new URL('../../../..', import.meta.url).pathname;

// Every dependency block is audited — a stray pin hides as easily in devDependencies as in dependencies.
const _BLOCKS = ['dependencies', 'devDependencies', 'peerDependencies'] as const;

// Engine-identity law: the property engine is the one effect/FastCheck re-export; a second engine copy
// breaks Arbitrary class identity across the kit's input dispatch, so the package itself is refused.
const _REFUSED = ['fast-check'] as const;

// Workspace facts as data rows: each pattern is one line-grammar claim over the raw manifest text.
const _FACTS = [
    { fact: 'catalogMode is strict', pattern: /catalogMode:\s*strict/ },
    { fact: 'the spec estate rides the workspace graph', pattern: /-\s*'tests\/typescript\/\*'/ },
    { fact: 'the runtime branch rides the workspace graph', pattern: /-\s*'libs\/typescript'/ },
    { fact: 'the @effect/vitest peer tolerance is recorded as an allowedVersions row', pattern: /allowedVersions:[\s\S]{0,600}?vitest:\s*\d/ },
] as const;

// A counterfeit manifest missing every fact: each pattern must refuse it, or the fact row is a tautology.
const _COUNTERFEIT = "catalogMode: loose\npackages:\n- 'apps/*'\npeerDependencyRules:\nallowedVersions:\n";

// The [04] promotion layer: every doctrine anti-pattern no compiler flag or shipped Biome rule
// rejects is one GritQL rule at error under tools/biome/ — this roster is the law biome.json realizes.
const _PROMOTED = [
    'no-arity-sibling',
    'no-barrel-reexport',
    'no-domain-throw',
    'no-mutable-accumulation',
    'no-nullable-return',
    'no-rename-wrapper',
    'no-run-in-domain',
    'no-standalone-brand',
    'no-stdlib-in-domain',
] as const;

// Laws a single-pattern rule cannot express (cross-declaration shape comparison, naming judgment,
// composition review): review-owned by declaration, so a missing rule is a ruling, never an oversight.
const _REVIEW_ONLY = [
    'anticipatory-collapse',
    'composed-implementation',
    'const-type-restatement',
    'inline-composition',
    'parallel-schema-beside-owner',
    'parallel-union-vs-vocabulary',
    'semantic-naming',
] as const;

// Every promoted rule binds the domain estate and exempts the spec dialects. Plugin includes match
// the full file path, so the estate glob leads with `**/` — the bare `libs/typescript/**` form never
// matches and silently disarms the whole promotion layer.
const _PLUGIN_SCOPE = ['**/libs/typescript/**', '!**/*.spec.ts', '!**/*.test.ts', '!**/*.bench.ts'] as const;

// The type-aware, import-graph, and test domains stay armed at recommended.
const _DOMAINS = ['project', 'test', 'types'] as const;

// A rule file is armed only when it registers an error diagnostic and carries both proof spans.
const _ARMED = [
    { mark: 'registers a diagnostic', want: /register_diagnostic\(/ },
    { mark: 'promotes at error', want: /severity\s*=\s*"error"/ },
    { mark: 'carries its firing span', want: /\/\/ FIRES:/ },
    { mark: 'carries its non-firing span', want: /\/\/ CLEAN:/ },
] as const;

// --- [MODELS] ----------------------------------------------------------------------------

const _Pins = Schema.optionalWith(Schema.Record({ key: Schema.String, value: Schema.String }), { default: () => ({}) });
const _Manifest = Schema.Struct({
    name: Schema.NonEmptyString,
    dependencies: _Pins,
    devDependencies: _Pins,
    peerDependencies: _Pins,
});

const _Biome = Schema.Struct({
    plugins: Schema.Array(Schema.Struct({ path: Schema.String, includes: Schema.Array(Schema.String) })),
    linter: Schema.Struct({
        domains: Schema.Record({ key: Schema.String, value: Schema.String }),
        rules: Schema.Struct({
            preset: Schema.String,
            suspicious: Schema.Struct({ noExplicitAny: Schema.String }),
        }),
    }),
});

// --- [OPERATIONS] ------------------------------------------------------------------------

const _decode = Schema.decodeUnknown(Schema.parseJson(_Manifest));

const _decodeBiome = Schema.decodeUnknown(Schema.parseJson(_Biome));

// The lint-legislature predicate: one violation list over the decoded config, falsifiable in isolation.
const _legislated = (config: typeof _Biome.Type): ReadonlyArray<string> =>
    Array.flatten([
        config.linter.rules.preset === 'recommended' ? [] : ['linter.rules.preset must hold the recommended preset'],
        config.linter.rules.suspicious.noExplicitAny === 'error' ? [] : ['suspicious.noExplicitAny must be legislated at error, never inherited'],
        Array.filterMap(_DOMAINS, (domain) =>
            config.linter.domains[domain] === 'recommended' ? Option.none() : Option.some(`linter.domains.${domain} must be recommended`),
        ),
        Array.filterMap(_PROMOTED, (rule) =>
            Array.some(config.plugins, (plugin) => plugin.path === `./tools/biome/${rule}.grit`)
                ? Option.none()
                : Option.some(`promoted rule ${rule} is missing from plugins`),
        ),
        Array.filterMap(config.plugins, (plugin) =>
            Array.every(_PLUGIN_SCOPE, (glob) => Array.some(plugin.includes, (own) => own === glob))
                ? Option.none()
                : Option.some(`${plugin.path} must scope [${_PLUGIN_SCOPE.join(', ')}]`),
        ),
    ]);

// The armed-rule predicate over one .grit text.
const _disarmed = (rule: string, text: string): ReadonlyArray<string> =>
    Array.filterMap(_ARMED, (row) => (row.want.test(text) ? Option.none() : Option.some(`${rule} never ${row.mark}`)));

// Version facts live only in pnpm-workspace.yaml: a spec-estate dependency resolves through the catalog or the workspace graph.
const _admitted = (pin: string): boolean => pin === 'catalog:' || pin.startsWith('workspace:');

const _entries = (manifest: typeof _Manifest.Type): ReadonlyArray<readonly [block: string, name: string, pin: string]> =>
    Array.flatMap(_BLOCKS, (block) => Array.map(Record.toEntries(manifest[block]), ([name, pin]) => [block, name, pin] as const));

// An overlay copy names its canonical twin at another tier by full catalog path; a canonical copy carries no such pointer.
const _overlay = (name: string): RegExp => new RegExp(`\`(libs|tests)/typescript(/[a-z]+)?/\\.api/${name.replaceAll('.', '\\.')}\``);

const _manifests = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const entries = yield* fs.readDirectory(_HOME);
    const decoded = yield* Effect.forEach(
        Array.filter(entries, (entry) => !entry.startsWith('.')),
        (entry) => Effect.option(Effect.flatMap(fs.readFileString(path.join(_HOME, entry, 'package.json')), (raw) => Effect.orDie(_decode(raw)))),
    );
    return Array.getSomes(decoded);
});

// The catalog tiers: the two fixed tiers plus every folder tier discovered on disk — a package holds
// exactly one canonical catalog; a shared basename is legal only as a declared overlay naming it.
const _tiers = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const branch = path.join(_ROOT, 'libs/typescript');
    const folders = yield* fs.readDirectory(branch);
    const candidates = Array.appendAll(
        [path.join(branch, '.api'), path.join(_ROOT, 'tests/typescript/.api')],
        Array.map(
            Array.filter(folders, (folder) => !folder.startsWith('.') && !folder.startsWith('_')),
            (folder) => path.join(branch, folder, '.api'),
        ),
    );
    return yield* Effect.forEach(candidates, (tier) =>
        Effect.map(
            Effect.orElseSucceed(fs.readDirectory(tier), () => []),
            (names) => [tier, Array.filter(names, (name) => name.endsWith('.md'))] as const,
        ),
    );
});

// --- [SPECS] -----------------------------------------------------------------------------

layer(NodeContext.layer)('workspace admission', (it) => {
    it.effect('every spec-estate pin across every dependency block resolves through catalog: or workspace: only', () =>
        Effect.gen(function* () {
            const packages = yield* _manifests;
            expect(packages.length).toBeGreaterThan(0);
            const stray = Array.flatMap(packages, (manifest) =>
                Array.filterMap(_entries(manifest), ([block, name, pin]) =>
                    _admitted(pin) ? Option.none() : Option.some(`${manifest.name}.${block}: ${name}@${pin}`),
                ),
            );
            expect(stray).toEqual([]);
        }),
    );

    it.effect('the refused property engine never enters a spec-estate manifest', () =>
        Effect.gen(function* () {
            const packages = yield* _manifests;
            const smuggled = Array.flatMap(packages, (manifest) =>
                Array.filterMap(_entries(manifest), ([block, name]) =>
                    Array.some(_REFUSED, (refused) => refused === name) ? Option.some(`${manifest.name}.${block}: ${name}`) : Option.none(),
                ),
            );
            expect(smuggled).toEqual([]);
        }),
    );

    it.effect('the workspace manifest carries every recorded fact', () =>
        Effect.gen(function* () {
            const fs = yield* FileSystem.FileSystem;
            const path = yield* Path.Path;
            const manifest = yield* fs.readFileString(path.join(_ROOT, 'pnpm-workspace.yaml'));
            const missing = Array.filterMap(_FACTS, (row) => (row.pattern.test(manifest) ? Option.none() : Option.some(row.fact)));
            expect(missing).toEqual([]);
        }),
    );

    it.effect('the promotion layer is legislated: config law holds, the rule roster matches disk, every rule is armed', () =>
        Effect.gen(function* () {
            const fs = yield* FileSystem.FileSystem;
            const path = yield* Path.Path;
            const config = yield* Effect.orDie(_decodeBiome(yield* fs.readFileString(path.join(_ROOT, 'biome.json'))));
            expect(_legislated(config)).toEqual([]);
            const home = path.join(_ROOT, 'tools/biome');
            const onDisk = Array.sort(
                Array.filterMap(yield* fs.readDirectory(home), (name) =>
                    name.endsWith('.grit') ? Option.some(name.slice(0, -'.grit'.length)) : Option.none(),
                ),
                Order.string,
            );
            expect(onDisk).toEqual([..._PROMOTED]);
            const dead = yield* Effect.forEach(_PROMOTED, (rule) =>
                Effect.map(fs.readFileString(path.join(home, `${rule}.grit`)), (text) => _disarmed(rule, text)),
            );
            expect(Array.flatten(dead)).toEqual([]);
        }),
    );

    it.effect('a package holds one canonical catalog; every extra copy is a declared overlay naming it', () =>
        Effect.gen(function* () {
            const fs = yield* FileSystem.FileSystem;
            const path = yield* Path.Path;
            const tiers = yield* _tiers;
            const homes = Array.reduce(tiers, Record.empty<string, ReadonlyArray<string>>(), (acc, [tier, names]) =>
                Array.reduce(names, acc, (held, name) =>
                    Record.modifyOption(held, name, Array.append(tier)).pipe(Option.getOrElse(() => Record.set(held, name, Array.of(tier)))),
                ),
            );
            const rogue = yield* Effect.forEach(
                Array.filter(Record.toEntries(homes), ([, where]) => where.length > 1),
                ([name, where]) =>
                    Effect.map(
                        Effect.forEach(where, (tier) => fs.readFileString(path.join(tier, name))),
                        (texts) =>
                            Array.filter(texts, (text) => !_overlay(name).test(text)).length === 1
                                ? Option.none<string>()
                                : Option.some(`${name} -> ${where.join(' + ')}`),
                    ),
            );
            expect(Array.getSomes(rogue)).toEqual([]);
        }),
    );
});

describe('admission predicates', () => {
    it('the pin predicate refuses the inline version literals the gauge exists to catch', () => {
        expect(_admitted('catalog:')).toBe(true);
        expect(_admitted('workspace:*')).toBe(true);
        expect(_admitted('^4.1.9')).toBe(false);
        expect(_admitted('latest')).toBe(false);
    });

    it('every workspace-fact pattern refuses the counterfeit manifest', () => {
        const undead = Array.filterMap(_FACTS, (row) => (row.pattern.test(_COUNTERFEIT) ? Option.some(row.fact) : Option.none()));
        expect(undead).toEqual([]);
    });

    it('the legislature predicate refuses a downgraded config on every axis', () => {
        const lawful = {
            plugins: Array.map(_PROMOTED, (rule) => ({ path: `./tools/biome/${rule}.grit`, includes: [..._PLUGIN_SCOPE] })),
            linter: {
                domains: { project: 'recommended', test: 'recommended', types: 'recommended' },
                rules: { preset: 'recommended', suspicious: { noExplicitAny: 'error' } },
            },
        };
        expect(_legislated(lawful)).toEqual([]);
        expect(_legislated({ ...lawful, plugins: Array.drop(lawful.plugins, 1) })).not.toEqual([]);
        expect(_legislated({ ...lawful, plugins: Array.map(lawful.plugins, (row) => ({ ...row, includes: ['libs/typescript/**'] })) })).not.toEqual(
            [],
        );
        expect(
            _legislated({ ...lawful, linter: { ...lawful.linter, rules: { preset: 'all', suspicious: { noExplicitAny: 'error' } } } }),
        ).not.toEqual([]);
        expect(
            _legislated({ ...lawful, linter: { ...lawful.linter, rules: { preset: 'recommended', suspicious: { noExplicitAny: 'warn' } } } }),
        ).not.toEqual([]);
        expect(_legislated({ ...lawful, linter: { ...lawful.linter, domains: { project: 'recommended', test: 'recommended' } } })).not.toEqual([]);
    });

    it('the armed predicate refuses a rule with no error severity or missing proof spans', () => {
        const armed =
            'language js(typescript, jsx)\n// FIRES: throw new Error(x)\n// CLEAN: Effect.fail(new Fault())\n`throw $e` where {\n register_diagnostic(span=$e, message="m", severity="error")\n}';
        expect(_disarmed('specimen', armed)).toEqual([]);
        expect(_disarmed('specimen', armed.replace('severity="error"', 'severity="warn"'))).not.toEqual([]);
        expect(_disarmed('specimen', armed.replace('// FIRES:', '//'))).not.toEqual([]);
        expect(_disarmed('specimen', armed.replace('// CLEAN:', '//'))).not.toEqual([]);
        expect(_disarmed('specimen', 'language js(typescript, jsx)\n`throw $e`')).not.toEqual([]);
    });

    it('the promoted and review-only rosters split the law set with no overlap', () => {
        const promoted: ReadonlyArray<string> = _PROMOTED;
        const doubled = Array.filter(_REVIEW_ONLY, (law) => Array.some(promoted, (rule) => rule === law));
        expect(doubled).toEqual([]);
    });

    it('the overlay mark separates a declared overlay from a second canonical', () => {
        expect(
            _overlay('apache-arrow.md').test('the branch catalogue is the ui folder (`libs/typescript/ui/.api/apache-arrow.md`); seam facts only'),
        ).toBe(true);
        expect(_overlay('apache-arrow.md').test('# [apache-arrow] — the one columnar wire of the data plane')).toBe(false);
    });
});

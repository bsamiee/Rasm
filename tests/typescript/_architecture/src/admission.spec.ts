import { FileSystem, Path } from '@effect/platform';
import { NodeContext } from '@effect/platform-node';
import { describe, expect, it, layer } from '@effect/vitest';
import { Array, Effect, Option, Record, Schema } from 'effect';

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

// --- [MODELS] ----------------------------------------------------------------------------

const _Pins = Schema.optionalWith(Schema.Record({ key: Schema.String, value: Schema.String }), { default: () => ({}) });
const _Manifest = Schema.Struct({
    name: Schema.NonEmptyString,
    dependencies: _Pins,
    devDependencies: _Pins,
    peerDependencies: _Pins,
});

// --- [OPERATIONS] ------------------------------------------------------------------------

const _decode = Schema.decodeUnknown(Schema.parseJson(_Manifest));

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

    it('the overlay mark separates a declared overlay from a second canonical', () => {
        expect(_overlay('apache-arrow.md').test('the branch catalogue is the ui folder (`libs/typescript/ui/.api/apache-arrow.md`); seam facts only')).toBe(true);
        expect(_overlay('apache-arrow.md').test('# [apache-arrow] — the one columnar wire of the data plane')).toBe(false);
    });
});

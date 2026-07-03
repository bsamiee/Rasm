import { FileSystem, Path } from '@effect/platform';
import { NodeContext } from '@effect/platform-node';
import { describe, expect, it, layer } from '@effect/vitest';
import { Audit, Imports } from '@rasm/ts-testkit/gauges';
import { Array, Effect, HashSet, Number, Option, pipe, Schema } from 'effect';

// --- [TYPES] -----------------------------------------------------------------------------

type LedgerRow = { readonly folder: string; readonly edges: ReadonlyArray<string>; readonly wave: number };
type TagTriple = { readonly folder: string; readonly scope: string; readonly runtime: string; readonly plane: string };

// --- [CONSTANTS] -------------------------------------------------------------------------

const _ROOT = new URL('../../../..', import.meta.url).pathname;

// Legacy _tmp material and the authoring corpora never join a source verdict.
const _PRUNE = /(^|\/)(node_modules|dist|coverage|\.git|_tmp|\.planning|\.api)(\/|$)/;

// The branch-wide migrator ban: DDL is idempotent declarative ensure, and PgMigrator has no legal importer.
const _BANNED = [/^@effect\/sql\/Migrator/, /^@effect\/sql-pg\/PgMigrator/] as const;

// Folder-scoped external admissions: each family is a zone, and the permitted [folder, family] pairs
// below are the only legal crossings; an unlisted external package is substrate and stays unaudited.
const _ADMISSIONS = [
    { family: 'ext:pulumi', pattern: /^@pulumi\//, zones: ['iac'] },
    { family: 'ext:sql-driver', pattern: /^@effect\/sql-(pg|sqlite)/, zones: ['store'] },
    { family: 'ext:jose', pattern: /^jose($|\/)/, zones: ['security'] },
    { family: 'ext:arctic', pattern: /^arctic($|\/)/, zones: ['security'] },
    { family: 'ext:webauthn', pattern: /^@simplewebauthn\//, zones: ['security'] },
    { family: 'ext:react', pattern: /^react($|-|\/)/, zones: ['ui', 'browser'] },
] as const;

// The security sub-folder admissions ride the same engine at depth-2 zones.
const _CRYPTO = [
    { family: 'ext:jose', pattern: /^jose($|\/)/, zones: ['security/sign'] },
    { family: 'ext:arctic', pattern: /^arctic($|\/)/, zones: ['security/authn'] },
    { family: 'ext:webauthn', pattern: /^@simplewebauthn\//, zones: ['security/authn'] },
] as const;

// The runtime-direction law: an importing runtime may only reach the runtimes on its row.
const _RUNTIME_MAY = {
    browser: ['browser', 'neutral'],
    bun: ['bun', 'neutral'],
    neutral: ['neutral'],
    node: ['node', 'neutral'],
} as const;

const _PLANES = ['runtime', 'deploy', 'dev'] as const;

// --- [MODELS] ----------------------------------------------------------------------------

const _Project = Schema.Struct({ name: Schema.NonEmptyString, tags: Schema.Array(Schema.NonEmptyString) });

// --- [OPERATIONS] ------------------------------------------------------------------------

const _decodeProject = Schema.decodeUnknown(Schema.parseJson(_Project));

const _segments = (path: string): ReadonlyArray<string> => Array.filter(path.split('/'), (part) => part.length > 0 && part !== '.');

const _resolved = (from: string, specifier: string): ReadonlyArray<string> =>
    Array.reduce(_segments(specifier), Array.dropRight(_segments(from), 1), (stack, part) =>
        part === '..' ? Array.dropRight(stack, 1) : Array.append(stack, part),
    );

const _ticked = (cell: string): ReadonlyArray<string> =>
    Array.filterMap(Array.fromIterable(cell.matchAll(/`([^`]+)`/g)), (hit) => Option.fromNullable(hit[1]));

// The permitted-edge table parses live from its owning page: a reshaped or vanished table yields zero
// rows and fails the gauge loudly — the page is the law's single source, never a transcribed copy.
const _parsedLedger = (page: string): ReadonlyArray<LedgerRow> =>
    pipe(
        Array.filterMap(Array.fromIterable(page.matchAll(/^\|\s*`([a-z]+)`\s*\|([^|]*)\|\s*W(\d)\s*\|/gm)), (row) =>
            Option.all({
                folder: Option.fromNullable(row[1]),
                tokens: Option.some(_ticked(row[2] ?? '')),
                wave: Option.flatMap(Option.fromNullable(row[3]), Number.parse),
            }),
        ),
        (raw) =>
            pipe(HashSet.fromIterable(Array.map(raw, (row) => row.folder)), (folders) =>
                Array.map(raw, (row) => ({
                    folder: row.folder,
                    edges: Array.filter(row.tokens, (token) => HashSet.has(folders, token) && token !== row.folder),
                    wave: row.wave,
                })),
            ),
    );

// Acyclicity by expression fixpoint: peel nodes whose every edge points outside the live set; a
// non-empty residue after |rows| passes is the cycle core.
const _acyclic = (rows: ReadonlyArray<LedgerRow>): boolean =>
    pipe(
        Array.reduce(Array.range(1, rows.length + 1), rows, (live) =>
            Array.filter(live, (row) => Array.some(row.edges, (edge) => Array.some(live, (other) => other.folder === edge))),
        ),
        Array.isEmptyReadonlyArray,
    );

const _rules = (rows: ReadonlyArray<LedgerRow>): Parameters<typeof Imports.verdict>[1] =>
    pipe(HashSet.fromIterable(Array.map(rows, (row) => row.folder)), (folders) => ({
        banned: [..._BANNED],
        permitted: Array.appendAll(
            Array.flatMap(rows, (row) => Array.map(row.edges, (edge) => [row.folder, edge] as const)),
            Array.flatMap(_ADMISSIONS, (row) => Array.map(row.zones, (zone) => [zone, row.family] as const)),
        ),
        zone: (path: string) => Option.filter(Array.head(_segments(path)), (head) => HashSet.has(folders, head)),
        zoneOf: (specifier: string, from: string) =>
            specifier.startsWith('.')
                ? Option.filter(Array.head(_resolved(from, specifier)), (head) => HashSet.has(folders, head))
                : specifier.startsWith('@rasm/ts/')
                  ? Option.filter(Array.head(_segments(specifier.slice('@rasm/ts'.length))), (head) => HashSet.has(folders, head))
                  : Option.map(
                        Array.findFirst(_ADMISSIONS, (row) => row.pattern.test(specifier)),
                        (row) => row.family,
                    ),
    }));

const _cryptoRules: Parameters<typeof Imports.verdict>[1] = {
    banned: [],
    permitted: Array.flatMap(_CRYPTO, (row) => Array.map(row.zones, (zone) => [zone, row.family] as const)),
    zone: (path: string) =>
        pipe(
            _segments(path),
            (parts) =>
                Array.head(parts).pipe(
                    Option.filter((head) => head === 'security'),
                    Option.flatMap(() => Array.head(Array.filter(Array.drop(parts, 1), (part) => part !== 'src'))),
                ),
            Option.map((sub) => `security/${sub}`),
        ),
    zoneOf: (specifier: string) =>
        Option.map(
            Array.findFirst(_CRYPTO, (row) => row.pattern.test(specifier)),
            (row) => row.family,
        ),
};

const _drawn = (verdict: ReturnType<typeof Imports.verdict>): ReadonlyArray<string> =>
    Audit.$match(verdict, {
        Unsupported: () => [],
        Audited: ({ violations }) => Array.map(violations, (violation) => `${violation.path} -[${violation.kind}]-> ${violation.specifier}`),
    });

const _ledger = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    return _parsedLedger(yield* fs.readFileString(path.join(_ROOT, 'libs/typescript/.planning/composition-system.md')));
});

const _triples = (rows: ReadonlyArray<LedgerRow>) =>
    Effect.gen(function* () {
        const fs = yield* FileSystem.FileSystem;
        const path = yield* Path.Path;
        return yield* Effect.forEach(rows, (row) =>
            Effect.map(
                Effect.option(
                    Effect.flatMap(fs.readFileString(path.join(_ROOT, 'libs/typescript', row.folder, 'project.json')), (raw) =>
                        Effect.orDie(_decodeProject(raw)),
                    ),
                ),
                (project) => [row.folder, project] as const,
            ),
        );
    });

const _tagged = (folder: string, tags: ReadonlyArray<string>): Option.Option<TagTriple> =>
    Option.all({
        folder: Option.some(folder),
        scope: Array.findFirst(tags, (tag) => tag.startsWith('scope:')),
        runtime: Array.findFirst(tags, (tag) => tag.startsWith('runtime:')),
        plane: Array.findFirst(tags, (tag) => tag.startsWith('plane:')),
    });

// --- [SPECS] -----------------------------------------------------------------------------

layer(NodeContext.layer)('edge ledger', (it) => {
    it.effect('the owning page yields a coherent acyclic permitted-edge table', () =>
        Effect.gen(function* () {
            const rows = yield* _ledger;
            expect(rows.length).toBeGreaterThan(0);
            expect(_acyclic(rows)).toBe(true);
            const climbing = Array.flatMap(rows, (row) =>
                Array.filterMap(row.edges, (edge) =>
                    Option.flatMap(
                        Array.findFirst(rows, (other) => other.folder === edge),
                        (target) => (target.wave > row.wave ? Option.some(`${row.folder}(W${row.wave}) -> ${edge}(W${target.wave})`) : Option.none()),
                    ),
                ),
            );
            expect(climbing).toEqual([]);
        }),
    );

    it.effect('every ledger folder carries its tag triple and the runtime-direction law holds', () =>
        Effect.gen(function* () {
            const rows = yield* _ledger;
            const projects = yield* _triples(rows);
            const untagged = Array.filterMap(projects, ([folder, project]) =>
                Option.match(
                    Option.flatMap(project, (found) => _tagged(folder, found.tags)),
                    {
                        onNone: () => Option.some(folder),
                        onSome: () => Option.none(),
                    },
                ),
            );
            expect(untagged).toEqual([]);
            const triples = Array.getSomes(
                Array.map(projects, ([folder, project]) => Option.flatMap(project, (found) => _tagged(folder, found.tags))),
            );
            const drifted = Array.filterMap(triples, (triple) => {
                const runtime = triple.runtime.slice('runtime:'.length);
                const plane = triple.plane.slice('plane:'.length);
                const legal = triple.scope === `scope:${triple.folder}` && runtime in _RUNTIME_MAY && Array.some(_PLANES, (kind) => kind === plane);
                return legal ? Option.none() : Option.some(`${triple.folder}: ${triple.scope},${triple.runtime},${triple.plane}`);
            });
            expect(drifted).toEqual([]);
            const runtimeOf = (folder: string): Option.Option<keyof typeof _RUNTIME_MAY> =>
                Array.findFirst(triples, (triple) => triple.folder === folder).pipe(
                    Option.map((triple) => triple.runtime.slice('runtime:'.length)),
                    Option.filter((kind): kind is keyof typeof _RUNTIME_MAY => kind in _RUNTIME_MAY),
                );
            const crossed = Array.flatMap(rows, (row) =>
                Array.filterMap(row.edges, (edge) =>
                    Option.match(Option.all([runtimeOf(row.folder), runtimeOf(edge)]), {
                        onNone: () => Option.some(`${row.folder} -> ${edge}: runtime unresolved`),
                        onSome: ([from, to]) =>
                            Array.some(_RUNTIME_MAY[from], (kind) => kind === to)
                                ? Option.none<string>()
                                : Option.some(`${row.folder}(${from}) -> ${edge}(${to})`),
                    }),
                ),
            );
            expect(crossed).toEqual([]);
            const inverted = Array.flatMap(rows, (row) =>
                Array.filterMap(row.edges, (edge) =>
                    Option.match(
                        Array.findFirst(triples, (triple) => triple.folder === edge && triple.plane === 'plane:deploy'),
                        {
                            onNone: () => Option.none<string>(),
                            onSome: () => Option.some(`${row.folder} -> ${edge}: deploy plane is depended on by nothing`),
                        },
                    ),
                ),
            );
            expect(inverted).toEqual([]);
        }),
    );

    it.effect('the branch source audit runs the real table and stays honest while no source ships', () =>
        Effect.gen(function* () {
            const rows = yield* _ledger;
            const path = yield* Path.Path;
            const modules = yield* Imports.load(path.join(_ROOT, 'libs/typescript'), _PRUNE);
            expect(_drawn(Imports.verdict(modules, _rules(rows)))).toEqual([]);
            expect(_drawn(Imports.verdict(modules, _cryptoRules))).toEqual([]);
        }),
    );
});

describe('gauge falsification', () => {
    const rows = _parsedLedger(['| `kernel` | — | W0 |', '| `state` | `kernel` | W1 |', '| `store` | `kernel`, `state` | W3 |'].join('\n'));

    it('the ledger parser reads rows and refuses malformed tables', () => {
        expect(rows).toEqual([
            { folder: 'kernel', edges: [], wave: 0 },
            { folder: 'state', edges: ['kernel'], wave: 1 },
            { folder: 'store', edges: ['kernel', 'state'], wave: 3 },
        ]);
        expect(_parsedLedger('| folder | edges | wave |')).toEqual([]);
    });

    it('the acyclicity gauge refutes a cyclic table', () => {
        expect(_acyclic(rows)).toBe(true);
        expect(
            _acyclic([
                { folder: 'host', edges: ['security'], wave: 1 },
                { folder: 'security', edges: ['host'], wave: 1 },
            ]),
        ).toBe(false);
    });

    it('a banned module import is a violation the engine names', () => {
        const scanned = Imports.scan([{ path: 'store/src/journal.ts', text: 'import { PgMigrator } from "@effect/sql-pg/PgMigrator";' }]);
        expect(_drawn(Imports.verdict(scanned, _rules(rows)))).toEqual(['store/src/journal.ts -[banned]-> @effect/sql-pg/PgMigrator']);
    });

    it('an edge outside the table is a violation, a permitted edge is not', () => {
        const scanned = Imports.scan([
            { path: 'kernel/src/identity.ts', text: 'import { fold } from "@rasm/ts/state";' },
            { path: 'store/src/journal.ts', text: 'import { brand } from "@rasm/ts/kernel";\nimport { PgClient } from "@effect/sql-pg";' },
        ]);
        expect(_drawn(Imports.verdict(scanned, _rules(rows)))).toEqual(['kernel/src/identity.ts -[edge]-> @rasm/ts/state']);
    });

    it('a relative reach-around resolves to its folder zone and is audited', () => {
        const scanned = Imports.scan([{ path: 'state/src/fold.ts', text: 'import { key } from "../../store/src/journal.ts";' }]);
        expect(_drawn(Imports.verdict(scanned, _rules(rows)))).toEqual(['state/src/fold.ts -[edge]-> ../../store/src/journal.ts']);
    });

    it('a crypto admission outside its sub-folder is a violation', () => {
        const scanned = Imports.scan([
            { path: 'security/src/session/cookie.ts', text: 'import { SignJWT } from "jose";' },
            { path: 'security/src/sign/jwt.ts', text: 'import { SignJWT } from "jose";' },
        ]);
        expect(_drawn(Imports.verdict(scanned, _cryptoRules))).toEqual(['security/src/session/cookie.ts -[edge]-> jose']);
    });
});

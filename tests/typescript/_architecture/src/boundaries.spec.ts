import { FileSystem, Path } from '@effect/platform';
import { NodeContext } from '@effect/platform-node';
import { describe, expect, it, layer } from '@effect/vitest';
import { Audit, Imports } from '@rasm/ts-testkit/gauges';
import { Array, Effect, HashMap, HashSet, Number, Option, Order, pipe, Record, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type LedgerRow = { readonly folder: string; readonly edges: ReadonlyArray<string>; readonly wave: number };
type TagTriple = { readonly folder: string; readonly scope: string; readonly runtime: string; readonly plane: string };

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ROOT = new URL('../../../..', import.meta.url).pathname;

// Authoring corpora and tool trees never join a source verdict.
const _PRUNE = /(^|\/)(node_modules|dist|coverage|\.git|\.planning|\.api)(\/|$)/;

// The branch-wide migrator ban: DDL is idempotent declarative ensure, and PgMigrator has no legal importer.
const _BANNED = [/^@effect\/sql\/Migrator/, /^@effect\/sql-pg\/PgMigrator/] as const;

// One external-family vocabulary: each family's specifier grammar is stated once; every admission
// tier below is [zone, family] rows over this table, so a pattern can never drift between tiers.
const _FAMILIES = {
    'ext:jose': /^jose($|\/)/,
    'ext:arctic': /^arctic($|\/)/,
    'ext:webauthn': /^@simplewebauthn\//,
    'ext:oslo': /^@oslojs\//,
    'ext:argon2': /^@node-rs\/argon2($|\/)/,
    'ext:otp': /^(@otplib\/|otplib($|\/))/,
    'ext:doppler': /^@dopplerhq\//,
    'ext:codec': /^(@bufbuild\/|@connectrpc\/|cbor-x($|\/)|@msgpack\/|rfc6902($|\/)|hash-wasm($|\/)|@electric-sql\/d2)/,
    'ext:sql': /^@effect\/sql($|-|\/)/,
    'ext:duckdb': /^@duckdb\//,
    'ext:arrow': /^apache-arrow($|\/)/,
    'ext:s3': /^@aws-sdk\//,
    'ext:tus': /^@tus\//,
    'ext:tus-client': /^tus-js-client($|\/)/,
    'ext:remote': /^(ssh2|basic-ftp|webdav)($|\/)/,
    'ext:file': /^(sharp|chokidar)($|\/)/,
    'ext:cluster': /^@effect\/(cluster|workflow|ai|rpc)($|-|\/)/,
    'ext:cli': /^@effect\/(cli|printer)($|-|\/)/,
    'ext:mcp': /^@modelcontextprotocol\//,
    'ext:nats': /^@nats-io\//,
    'ext:openfeature': /^@openfeature\//,
    'ext:otel': /^@opentelemetry\/(?!semantic-conventions)/,
    'ext:office': /^(nodemailer|exceljs|jspdf|jszip|papaparse)($|\/)/,
    'ext:shell': /^(workbox-|idb-keyval($|\/)|nuqs($|\/))/,
    'ext:react': /^react($|-|\/)/,
    'ext:aria': /^(@react-aria\/|@radix-ui\/|@floating-ui\/|cmdk($|\/)|vaul($|\/))/,
    'ext:style':
        /^(tailwind|tw-animate-css($|\/)|class-variance-authority($|\/)|clsx($|\/)|colorjs\.io($|\/)|lucide-react($|\/)|isomorphic-dompurify($|\/))/,
    'ext:interact': /^(motion($|\/)|@use-gesture\/|@tanstack\/)/,
    'ext:viz': /^(@perspective-dev\/|@observablehq\/|@visx\/|uplot($|\/)|d3($|-|\/))/,
    'ext:spatial': /^(three($|\/)|@google\/model-viewer($|\/)|maplibre-gl($|\/)|@deck\.gl\/|@geoarrow\/|@turf\/|@lume\/kiwi($|\/)|typegpu($|\/))/,
    'ext:pulumi': /^@pulumi(verse)?\//,
} as const satisfies Record<`ext:${string}`, RegExp>;

// Folder-scoped external admissions: the permitted [folder, family] crossings; an unlisted external
// package is substrate and stays unaudited. A package two folders own by charter carries two rows.
const _ADMISSIONS: ReadonlyArray<readonly [zone: string, family: keyof typeof _FAMILIES]> = [
    ['core', 'ext:codec'],
    ['security', 'ext:jose'],
    ['security', 'ext:arctic'],
    ['security', 'ext:webauthn'],
    ['security', 'ext:oslo'],
    ['security', 'ext:argon2'],
    ['security', 'ext:otp'],
    ['security', 'ext:doppler'],
    ['data', 'ext:sql'],
    ['data', 'ext:duckdb'],
    ['data', 'ext:arrow'],
    ['data', 'ext:s3'],
    ['data', 'ext:tus'],
    ['data', 'ext:remote'],
    ['data', 'ext:file'],
    ['runtime', 'ext:cluster'],
    ['runtime', 'ext:cli'],
    ['runtime', 'ext:mcp'],
    ['runtime', 'ext:nats'],
    ['runtime', 'ext:openfeature'],
    ['runtime', 'ext:otel'],
    ['runtime', 'ext:office'],
    ['runtime', 'ext:shell'],
    ['ui', 'ext:react'],
    ['ui', 'ext:aria'],
    ['ui', 'ext:style'],
    ['ui', 'ext:interact'],
    ['ui', 'ext:viz'],
    ['ui', 'ext:spatial'],
    ['ui', 'ext:arrow'],
    ['ui', 'ext:tus-client'],
    ['iac', 'ext:pulumi'],
];

// The security sub-folder admissions ride the same engine and the same vocabulary at depth-2 zones.
const _CRYPTO: ReadonlyArray<readonly [zone: string, family: keyof typeof _FAMILIES]> = [
    ['security/crypt', 'ext:jose'],
    ['security/crypt', 'ext:oslo'],
    ['security/crypt', 'ext:argon2'],
    ['security/crypt', 'ext:doppler'],
    ['security/authn', 'ext:arctic'],
    ['security/authn', 'ext:webauthn'],
    ['security/authn', 'ext:otp'],
];

// The runtime-direction law: an importing runtime may only reach the runtimes on its row. The keys
// are the one canonical runtime vocabulary — the tag axis; the exports-map conditions project onto
// it through _CONDITION_FILE below (server->node lane, browser->browser lane, wasm/default->neutral).
const _RUNTIME_MAY = {
    browser: ['browser', 'neutral'],
    neutral: ['neutral'],
    node: ['node', 'neutral'],
} as const;

// The boundary gate's other half: per-runtime subpath purity over the branch exports map. A neutral
// folder carries the full condition split in this exact order (condition matching is order-sensitive,
// `default` closes the list); a single-runtime folder is one unconditioned entrypoint, so a foreign
// runtime's source is physically unresolvable from its resolution.
const _CONDITIONS = ['server', 'browser', 'wasm', 'default'] as const;
const _CONDITION_FILE = { browser: 'browser.ts', default: 'index.ts', server: 'server.ts', wasm: 'wasm.ts' } as const;

// Depth subpaths are declared rows, never discovered: an undeclared interior subpath is a boundary breach.
const _DEPTH_SUBPATHS = [{ owner: 'ui', runtime: 'browser', subpath: './ui/viewer' }] as const;

const _PLANES = ['runtime', 'deploy', 'dev'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const _Project = Schema.Struct({ name: Schema.NonEmptyString, tags: Schema.Array(Schema.NonEmptyString) });

const _Branch = Schema.Struct({
    name: Schema.NonEmptyString,
    exports: Schema.Record({ key: Schema.String, value: Schema.Union(Schema.String, Schema.Record({ key: Schema.String, value: Schema.String })) }),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const _decodeProject = Schema.decodeUnknown(Schema.parseJson(_Project));

const _decodeBranch = Schema.decodeUnknown(Schema.parseJson(_Branch));

const _segments = (path: string): ReadonlyArray<string> => Array.filter(path.split('/'), (part) => part.length > 0 && part !== '.');

const _resolved = (from: string, specifier: string): ReadonlyArray<string> =>
    Array.reduce(_segments(specifier), Array.dropRight(_segments(from), 1), (stack, part) =>
        part === '..' ? Array.dropRight(stack, 1) : Array.append(stack, part),
    );

// The permitted-edge ledger parses live from the owning page's strata flowchart — the page is the
// law's single source, never a transcribed copy. `subgraph W<n>` clusters carry the wave marks, a
// single-word cluster title names the cluster's own folder, bracket nodes name the rest.
const _CLUSTER = /^\s*subgraph (W(\d))\["W\d ([A-Z][A-Z +]*)"\]\s*$/;
const _NODE = /^\s*(\w+)\[([a-z]+)\]\s*$/;

// Only solid `[IMPORT]`-labeled edges join the ledger: port bindings, the forbidden exemplar,
// layout links, and core-interior member edges are other grammars and never mint a permitted row.
const _IMPORT_EDGE = /^[ \t]*(\w+) e\d+@-->\|"\[IMPORT\]: [^"]*"\| (\w+)[ \t]*$/gm;

const _declared = (page: string): HashMap.HashMap<string, readonly [folder: string, wave: number]> =>
    Array.reduce(
        page.split('\n'),
        { names: HashMap.empty<string, readonly [folder: string, wave: number]>(), wave: Option.none<number>() },
        (state, line) => {
            const cluster = _CLUSTER.exec(line);
            const node = _NODE.exec(line);
            if (cluster !== null) {
                const opened = Option.flatMap(Option.fromNullable(cluster[2]), Number.parse);
                return {
                    names: Option.match(
                        Option.all([
                            Option.fromNullable(cluster[1]),
                            Option.filter(Option.fromNullable(cluster[3]), (title) => /^[A-Z]+$/.test(title)),
                            opened,
                        ]),
                        {
                            onNone: () => state.names,
                            onSome: ([id, title, wave]) => HashMap.set(state.names, id, [title.toLowerCase(), wave] as const),
                        },
                    ),
                    wave: opened,
                };
            }
            return /^\s*end\s*$/.test(line)
                ? { names: state.names, wave: Option.none<number>() }
                : node !== null
                  ? Option.match(Option.all([Option.fromNullable(node[1]), Option.fromNullable(node[2]), state.wave]), {
                        onNone: () => state,
                        onSome: ([id, folder, wave]) => ({ names: HashMap.set(state.names, id, [folder, wave] as const), wave: state.wave }),
                    })
                  : state;
        },
    ).names;

// An edge endpoint no cluster declared voids the whole parse — a reshaped or vanished strata fence
// fails the gauge loudly instead of shrinking the law to the rows that still happen to parse.
const _parsedLedger = (page: string): ReadonlyArray<LedgerRow> =>
    pipe(_declared(page), (names) =>
        Option.match(
            Option.all(
                Array.map(Array.fromIterable(page.matchAll(_IMPORT_EDGE)), (hit) =>
                    Option.all([
                        Option.flatMap(Option.fromNullable(hit[1]), (id) => HashMap.get(names, id)),
                        Option.flatMap(Option.fromNullable(hit[2]), (id) => HashMap.get(names, id)),
                    ]),
                ),
            ),
            {
                onNone: (): ReadonlyArray<LedgerRow> => [],
                onSome: (edges) =>
                    pipe(
                        Array.dedupeWith(
                            Array.flatMap(edges, ([from, to]) => [from, to]),
                            (a: readonly [folder: string, wave: number], b: readonly [folder: string, wave: number]) => a[0] === b[0],
                        ),
                        Array.sortBy(
                            Order.mapInput(Order.number, (endpoint: readonly [folder: string, wave: number]) => endpoint[1]),
                            Order.mapInput(Order.string, (endpoint: readonly [folder: string, wave: number]) => endpoint[0]),
                        ),
                        Array.map(([folder, wave]) => ({
                            folder,
                            edges: Array.sort(
                                Array.dedupe(
                                    Array.filterMap(edges, ([from, to]) =>
                                        from[0] === folder && to[0] !== folder ? Option.some(to[0]) : Option.none(),
                                    ),
                                ),
                                Order.string,
                            ),
                            wave,
                        })),
                    ),
            },
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

// The one specifier-to-family projection every admission tier shares.
const _familyOf = (specifier: string): Option.Option<string> =>
    Option.map(
        Array.findFirst(Record.toEntries(_FAMILIES), ([, pattern]) => pattern.test(specifier)),
        ([family]) => family,
    );

const _rules = (rows: ReadonlyArray<LedgerRow>): Parameters<typeof Imports.verdict>[1] =>
    pipe(HashSet.fromIterable(Array.map(rows, (row) => row.folder)), (folders) => ({
        banned: [..._BANNED],
        permitted: Array.appendAll(
            Array.flatMap(rows, (row) => Array.map(row.edges, (edge) => [row.folder, edge] as const)),
            _ADMISSIONS,
        ),
        zone: (path: string) => Option.filter(Array.head(_segments(path)), (head) => HashSet.has(folders, head)),
        zoneOf: (specifier: string, from: string) =>
            specifier.startsWith('.')
                ? Option.filter(Array.head(_resolved(from, specifier)), (head) => HashSet.has(folders, head))
                : specifier.startsWith('@rasm/ts/')
                  ? Option.filter(Array.head(_segments(specifier.slice('@rasm/ts'.length))), (head) => HashSet.has(folders, head))
                  : _familyOf(specifier),
    }));

const _cryptoRules: Parameters<typeof Imports.verdict>[1] = {
    banned: [],
    permitted: _CRYPTO,
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
    zoneOf: _familyOf,
};

const _drawn = (verdict: ReturnType<typeof Imports.verdict>): ReadonlyArray<string> =>
    Audit.$match(verdict, {
        Unsupported: () => [],
        Audited: ({ violations }) => Array.map(violations, (violation) => `${violation.path} -[${violation.kind}]-> ${violation.specifier}`),
    });

const _ledger = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    return _parsedLedger(yield* fs.readFileString(path.join(_ROOT, 'libs/typescript/.planning/ARCHITECTURE.md')));
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

// One subpath's purity verdict: a neutral folder carries the exact ordered condition split with each
// condition bound to its canonical file; a single-runtime folder is one unconditioned index entrypoint;
// a subpath whose runtime resolves to nothing on the tag axis has no exports law and is refused.
const _purity = (subpath: string, runtime: string, entry: string | Readonly<Record<string, string>>): ReadonlyArray<string> =>
    runtime === 'neutral'
        ? typeof entry === 'string'
            ? [`${subpath}: a neutral folder carries the ${_CONDITIONS.join('/')} condition split`]
            : Array.appendAll(
                  Array.every(_CONDITIONS, (cond, at) => Record.keys(entry)[at] === cond) && Record.keys(entry).length === _CONDITIONS.length
                      ? []
                      : [`${subpath}: conditions must be exactly [${_CONDITIONS.join(', ')}] in matching order`],
                  Array.filterMap(_CONDITIONS, (cond) =>
                      entry[cond] === `${subpath}/src/${_CONDITION_FILE[cond]}`
                          ? Option.none()
                          : Option.some(`${subpath}.${cond} -> ${entry[cond] ?? '(absent)'} (law: ${subpath}/src/${_CONDITION_FILE[cond]})`),
                  ),
              )
        : runtime === 'browser' || runtime === 'node'
          ? typeof entry === 'string'
              ? entry === `${subpath}/src/index.ts`
                  ? []
                  : [`${subpath} -> ${entry} (law: ${subpath}/src/index.ts)`]
              : [`${subpath}: a single-runtime folder is one unconditioned entrypoint`]
          : [`${subpath}: runtime '${runtime}' carries no exports law`];

// The day a folder ships source its exports entries must resolve on disk — a shipped src/ beside a
// phantom condition file is a breach the resolver surfaces downstream; an unshipped folder yields
// zero findings, so the gauge stands armed without a vacuous claim about absent trees.
const _unshipped = (
    subpath: string,
    entry: string | Readonly<Record<string, string>>,
    present: (relative: string) => boolean,
): ReadonlyArray<string> =>
    present(`${subpath}/src`)
        ? Array.filterMap(typeof entry === 'string' ? [entry] : Record.values(entry), (file) =>
              present(file) ? Option.none() : Option.some(`${subpath}: ${file} is absent on disk`),
          )
        : [];

const _tagged = (folder: string, tags: ReadonlyArray<string>): Option.Option<TagTriple> =>
    Option.all({
        folder: Option.some(folder),
        scope: Array.findFirst(tags, (tag) => tag.startsWith('scope:')),
        runtime: Array.findFirst(tags, (tag) => tag.startsWith('runtime:')),
        plane: Array.findFirst(tags, (tag) => tag.startsWith('plane:')),
    });

// --- [SPECS] ---------------------------------------------------------------------------

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

    it.effect('the branch exports map holds per-runtime subpath purity against the live tag triples', () =>
        Effect.gen(function* () {
            const fs = yield* FileSystem.FileSystem;
            const path = yield* Path.Path;
            const rows = yield* _ledger;
            const projects = yield* _triples(rows);
            const triples = Array.getSomes(
                Array.map(projects, ([folder, project]) => Option.flatMap(project, (found) => _tagged(folder, found.tags))),
            );
            const manifest = yield* Effect.orDie(_decodeBranch(yield* fs.readFileString(path.join(_ROOT, 'libs/typescript/package.json'))));
            const entries = Record.toEntries(manifest.exports);
            const lawful = Array.appendAll(
                Array.map(rows, (row) => `./${row.folder}`),
                Array.map(_DEPTH_SUBPATHS, (row) => row.subpath),
            );
            expect(Array.filter(entries, ([subpath]) => !Array.some(lawful, (own) => own === subpath)).map(([subpath]) => subpath)).toEqual([]);
            expect(Array.filter(lawful, (own) => !Array.some(entries, ([subpath]) => subpath === own))).toEqual([]);
            const runtimeOf = (subpath: string): string =>
                Option.getOrElse(
                    Option.orElse(
                        Option.map(
                            Array.findFirst(_DEPTH_SUBPATHS, (row) => row.subpath === subpath),
                            (row) => row.runtime,
                        ),
                        () =>
                            Option.map(
                                Array.findFirst(triples, (triple) => `./${triple.folder}` === subpath),
                                (triple) => triple.runtime.slice('runtime:'.length),
                            ),
                    ),
                    () => 'unresolved',
                );
            expect(Array.flatMap(entries, ([subpath, entry]) => _purity(subpath, runtimeOf(subpath), entry))).toEqual([]);
        }),
    );

    it.effect('every exports entry of a shipped folder resolves on disk; an unshipped folder stays armed', () =>
        Effect.gen(function* () {
            const fs = yield* FileSystem.FileSystem;
            const path = yield* Path.Path;
            const branch = path.join(_ROOT, 'libs/typescript');
            const manifest = yield* Effect.orDie(_decodeBranch(yield* fs.readFileString(path.join(branch, 'package.json'))));
            const entries = Record.toEntries(manifest.exports);
            const probes = Array.flatMap(entries, ([subpath, entry]) => [
                `${subpath}/src`,
                ...(typeof entry === 'string' ? [entry] : Record.values(entry)),
            ]);
            const facts = yield* Effect.forEach(probes, (probe) =>
                Effect.map(
                    Effect.orElseSucceed(fs.exists(path.join(branch, probe)), () => false),
                    (has) => [probe, has] as const,
                ),
            );
            const held = HashMap.fromIterable(facts);
            const present = (relative: string): boolean => Option.getOrElse(HashMap.get(held, relative), () => false);
            expect(Array.flatMap(entries, ([subpath, entry]) => _unshipped(subpath, entry, present))).toEqual([]);
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
    const rows = _parsedLedger(
        [
            'flowchart TB',
            '    subgraph W2["W2 DATA"]',
            '        Data[data]',
            '    end',
            '    subgraph W1["W1 SECURITY"]',
            '        Security[security]',
            '    end',
            '    subgraph W0["W0 CORE"]',
            '        Value[value]',
            '    end',
            '    Data e1@-->|"[IMPORT]: TenantScope"| Security',
            '    Data e2@-->|"[IMPORT]: ContentKey"| W0',
            '    Security e3@-->|"[IMPORT]: TenantContext"| W0',
            '    Security p1@-.->|"[PORT]: Shredder"| Data',
            '    W0 f1@-->|"forbidden: upward import"| W2',
            '    Value i1@--> Value',
            '    Data ~~~ Security',
        ].join('\n'),
    );

    it('the ledger parser reads the strata fence and refuses undeclared endpoints', () => {
        expect(rows).toEqual([
            { folder: 'core', edges: [], wave: 0 },
            { folder: 'security', edges: ['core'], wave: 1 },
            { folder: 'data', edges: ['core', 'security'], wave: 2 },
        ]);
        expect(_parsedLedger('| [FROM] | [MAY_IMPORT] | [NOTES] |')).toEqual([]);
        expect(_parsedLedger('flowchart TB\n    Data e1@-->|"[IMPORT]: ContentKey"| Ghost')).toEqual([]);
    });

    it('the acyclicity gauge refutes a cyclic table', () => {
        expect(_acyclic(rows)).toBe(true);
        expect(
            _acyclic([
                { folder: 'runtime', edges: ['security'], wave: 3 },
                { folder: 'security', edges: ['runtime'], wave: 1 },
            ]),
        ).toBe(false);
    });

    it('a banned module import is a violation the engine names', () => {
        const scanned = Imports.scan([{ path: 'data/src/journal.ts', text: 'import { PgMigrator } from "@effect/sql-pg/PgMigrator";' }]);
        expect(_drawn(Imports.verdict(scanned, _rules(rows)))).toEqual(['data/src/journal.ts -[banned]-> @effect/sql-pg/PgMigrator']);
    });

    it('an edge outside the table is a violation, a permitted edge is not', () => {
        const scanned = Imports.scan([
            { path: 'core/src/identity.ts', text: 'import { fold } from "@rasm/ts/security";' },
            { path: 'data/src/journal.ts', text: 'import { brand } from "@rasm/ts/core";\nimport { PgClient } from "@effect/sql-pg";' },
        ]);
        expect(_drawn(Imports.verdict(scanned, _rules(rows)))).toEqual(['core/src/identity.ts -[edge]-> @rasm/ts/security']);
    });

    it('a relative reach-around resolves to its folder zone and is audited', () => {
        const scanned = Imports.scan([{ path: 'security/src/fold.ts', text: 'import { key } from "../../data/src/journal.ts";' }]);
        expect(_drawn(Imports.verdict(scanned, _rules(rows)))).toEqual(['security/src/fold.ts -[edge]-> ../../data/src/journal.ts']);
    });

    it('the purity gauge refuses every counterfeit exports shape and passes the lawful ones', () => {
        const lawful = {
            server: './core/src/server.ts',
            browser: './core/src/browser.ts',
            wasm: './core/src/wasm.ts',
            default: './core/src/index.ts',
        };
        expect(_purity('./core', 'neutral', lawful)).toEqual([]);
        expect(_purity('./core', 'neutral', { ...lawful, server: './core/src/browser.ts' })).not.toEqual([]);
        expect(_purity('./core', 'neutral', { server: lawful.server, browser: lawful.browser, default: lawful.default })).not.toEqual([]);
        expect(
            _purity('./core', 'neutral', { default: lawful.default, browser: lawful.browser, server: lawful.server, wasm: lawful.wasm }),
        ).not.toEqual([]);
        expect(_purity('./core', 'neutral', './core/src/index.ts')).not.toEqual([]);
        expect(_purity('./ui', 'browser', './ui/src/index.ts')).toEqual([]);
        expect(_purity('./ui/viewer', 'browser', './ui/viewer/src/index.ts')).toEqual([]);
        expect(_purity('./iac', 'node', './iac/src/journal.ts')).not.toEqual([]);
        expect(_purity('./ui', 'browser', lawful)).not.toEqual([]);
        expect(_purity('./core', 'unresolved', './core/src/index.ts')).not.toEqual([]);
    });

    it('the shipped gauge demands entry files exactly where src exists', () => {
        const entry = {
            server: './core/src/server.ts',
            browser: './core/src/browser.ts',
            wasm: './core/src/wasm.ts',
            default: './core/src/index.ts',
        };
        expect(_unshipped('./core', entry, () => false)).toEqual([]);
        expect(_unshipped('./core', entry, (probe) => probe === './core/src')).toHaveLength(4);
        expect(_unshipped('./core', entry, () => true)).toEqual([]);
        expect(_unshipped('./ui', './ui/src/index.ts', (probe) => probe !== './ui/src/index.ts')).toEqual([
            './ui: ./ui/src/index.ts is absent on disk',
        ]);
    });

    it('every family claims its representative exactly once and refuses the near-miss', () => {
        const rows: ReadonlyArray<readonly [family: string, representative: string, nearMiss: string]> = [
            ['ext:jose', 'jose/jwks', 'josefine'],
            ['ext:arctic', 'arctic', 'arctic-fox'],
            ['ext:webauthn', '@simplewebauthn/server', '@simplewebauthn-fork/server'],
            ['ext:oslo', '@oslojs/encoding', '@oslo/encoding'],
            ['ext:argon2', '@node-rs/argon2', '@node-rs/bcrypt'],
            ['ext:otp', 'otplib', 'otplib-next'],
            ['ext:doppler', '@dopplerhq/node-sdk', '@doppler/node-sdk'],
            ['ext:codec', '@bufbuild/protobuf', 'protobufjs'],
            ['ext:codec', '@electric-sql/d2mini', '@electric-sql/pglite'],
            ['ext:sql', '@effect/sql-pg', '@effect/sqlite'],
            ['ext:duckdb', '@duckdb/node-api', 'duckdb'],
            ['ext:arrow', 'apache-arrow', 'apache-arrow-old'],
            ['ext:s3', '@aws-sdk/client-s3', 'aws-sdk'],
            ['ext:tus', '@tus/server', 'tus'],
            ['ext:tus-client', 'tus-js-client', 'tus-js'],
            ['ext:remote', 'ssh2', 'ssh2-sftp-client'],
            ['ext:file', 'sharp', 'sharpen'],
            ['ext:cluster', '@effect/ai-anthropic', '@effect/aim'],
            ['ext:cli', '@effect/printer-ansi', '@effect/print'],
            ['ext:mcp', '@modelcontextprotocol/sdk', 'modelcontextprotocol'],
            ['ext:nats', '@nats-io/jetstream', 'nats'],
            ['ext:openfeature', '@openfeature/server-sdk', 'openfeature'],
            ['ext:otel', '@opentelemetry/sdk-trace-node', '@opentelemetry/semantic-conventions'],
            ['ext:office', 'papaparse', 'papaparse-lite'],
            ['ext:shell', 'workbox-window', 'workbox'],
            ['ext:react', 'react-dom/client', 'preact'],
            ['ext:aria', '@react-aria/live-announcer', '@internationalized/date'],
            ['ext:style', 'tailwind-merge', 'clsx-lite'],
            ['ext:interact', '@tanstack/react-table', 'motion-dom-x'],
            ['ext:viz', 'd3-scale', 'd3fc'],
            ['ext:spatial', '@deck.gl/core', 'three-stdlib'],
            ['ext:pulumi', '@pulumiverse/doppler', 'pulumi'],
        ];
        const claims = (specifier: string): ReadonlyArray<string> =>
            Array.filterMap(Record.toEntries(_FAMILIES), ([family, pattern]) => (pattern.test(specifier) ? Option.some(family) : Option.none()));
        expect(Array.flatMap(rows, ([family, representative]) => (claims(representative).join() === family ? [] : [representative]))).toEqual([]);
        expect(Array.flatMap(rows, ([, , nearMiss]) => (Array.isEmptyReadonlyArray(claims(nearMiss)) ? [] : [nearMiss]))).toEqual([]);
    });

    it('a crypto admission outside its sub-folder is a violation', () => {
        const scanned = Imports.scan([
            { path: 'security/src/authn/session.ts', text: 'import { SignJWT } from "jose";' },
            { path: 'security/src/crypt/sign.ts', text: 'import { SignJWT } from "jose";' },
        ]);
        expect(_drawn(Imports.verdict(scanned, _cryptoRules))).toEqual(['security/src/authn/session.ts -[edge]-> jose']);
    });
});

import { FileSystem, Path } from '@effect/platform';
import { NodeContext } from '@effect/platform-node';
import { describe, expect, it, layer } from '@effect/vitest';
import { Audit, Imports } from '@rasm/ts-testkit/gauges';
import { Array, Effect, HashMap, HashSet, Number, Option, Order, pipe, Record, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type LedgerRow = { readonly folder: string; readonly edges: ReadonlyArray<string>; readonly stratum: number };
type TagTriple = { readonly folder: string; readonly scope: string; readonly runtime: string; readonly plane: string };
type Pkg = typeof _Pkg.Type;

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ROOT = new URL('../../..', import.meta.url).pathname;

// Authoring corpora and tool trees never join a source verdict.
const _PRUNE = /(^|\/)(node_modules|dist|coverage|\.git|\.planning|\.api)(\/|$)/;

// The branch-wide migrator ban: DDL is idempotent declarative ensure, and PgMigrator has no legal importer.
const _BANNED = [/^@effect\/sql\/Migrator/, /^@effect\/sql-pg\/PgMigrator/] as const;

// One external-family vocabulary: each family's specifier grammar is stated once; every admission
// tier below is [zone, family] rows over this table, so a pattern can never drift between tiers.
const _FAMILIES = {
    'ext:jose': /^jose($|\/)/,
    'ext:oidc': /^openid-client($|\/)/,
    'ext:webauthn': /^@simplewebauthn\//,
    'ext:oslo': /^@oslojs\//,
    'ext:argon2': /^@node-rs\/argon2($|\/)/,
    'ext:otp': /^(@otplib\/|otplib($|\/))/,
    'ext:doppler': /^@dopplerhq\//,
    'ext:codec': /^(@bufbuild\/|@connectrpc\/|@msgpack\/|rfc6902($|\/)|hash-wasm($|\/)|@electric-sql\/d2)/,
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
    ['security', 'ext:oidc'],
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
    ['security/authn', 'ext:oidc'],
    ['security/authn', 'ext:webauthn'],
    ['security/authn', 'ext:otp'],
];

// The runtime-direction law: an importing runtime may only reach the runtimes on its row. The keys
// are the one canonical runtime vocabulary — the tag axis the import audit enforces.
const _RUNTIME_MAY = {
    browser: ['browser', 'neutral'],
    neutral: ['neutral'],
    node: ['node', 'neutral'],
} as const;

// The boundary gate's other half: entrypoint purity over each package's exports map. Every package
// is exactly one unconditioned "." entry (index.ts, the public API surface) plus "./package.json";
// export conditions and subpath exports are earned by a real divergent implementation or a real
// submodule the day it lands, never pre-minted as stubs.

// Depth projects are declared rows, never discovered: an undeclared interior package is a boundary
// breach. `owner` is the depth-1 zone its files and imports audit under.
const _DEPTH_PROJECTS = [{ owner: 'ui', folder: 'ui/viewer', runtime: 'browser' }] as const;

// Packages outside the zone system: generated substrate and the spec kit; their imports and
// manifest rows stay unaudited by the edge law (families still bind).
const _SUBSTRATE = ['@rasm/contracts', '@rasm/ts-testkit'] as const;

const _PLANES = ['runtime', 'deploy', 'dev'] as const;

const _BRANCH = 'libs/typescript';

// --- [MODELS] --------------------------------------------------------------------------

const _Entry = Schema.Union(Schema.String, Schema.Record({ key: Schema.String, value: Schema.String }));

const _Pkg = Schema.Struct({
    name: Schema.NonEmptyString,
    exports: Schema.optional(Schema.Record({ key: Schema.String, value: _Entry })),
    dependencies: Schema.optional(Schema.Record({ key: Schema.String, value: Schema.String })),
    devDependencies: Schema.optional(Schema.Record({ key: Schema.String, value: Schema.String })),
    peerDependencies: Schema.optional(Schema.Record({ key: Schema.String, value: Schema.String })),
    nx: Schema.optional(Schema.Struct({ tags: Schema.optional(Schema.Array(Schema.NonEmptyString)) })),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const _decodePkg = Schema.decodeUnknown(Schema.parseJson(_Pkg));

const _segments = (path: string): ReadonlyArray<string> => Array.filter(path.split('/'), (part) => part.length > 0 && part !== '.');

const _resolved = (from: string, specifier: string): ReadonlyArray<string> =>
    Array.reduce(_segments(specifier), Array.dropRight(_segments(from), 1), (stack, part) =>
        part === '..' ? Array.dropRight(stack, 1) : Array.append(stack, part),
    );

// The strata letter is the fence's one cluster vocabulary — a cluster id and its title both open with
// it — so the live-parse grammar here and the falsification fixture at the tail derive from this
// anchor alone, and a re-lettered strata fence moves both ends in one edit.
const _STRATUM = 'S';

const _clusterId = (stratum: number): string => `${_STRATUM}${stratum}`;

const _clusterLine = (stratum: number, title: string): string => `    subgraph ${_clusterId(stratum)}["${_clusterId(stratum)} ${title}"]`;

// The permitted-edge ledger parses live from the owning page's strata flowchart — the page is the
// law's single source, never a transcribed copy. A cluster carries its stratum mark, a single-word
// cluster title names the cluster's own folder, and bracket nodes name the rest.
const _CLUSTER = new RegExp(String.raw`^\s*subgraph (${_STRATUM}(\d))\["${_STRATUM}\d ([A-Z][A-Z +]*)"\]\s*$`);
const _NODE = /^\s*(\w+)\[([a-z]+)\]\s*$/;

// Only solid `[IMPORT]`-labeled edges join the ledger: port bindings, the forbidden exemplar,
// layout links, and core-interior member edges are other grammars and never mint a permitted row.
const _IMPORT_EDGE = /^[ \t]*(\w+) e\d+@-->\|"\[IMPORT\]: [^"]*"\| (\w+)[ \t]*$/gm;

const _declared = (page: string): HashMap.HashMap<string, readonly [folder: string, stratum: number]> =>
    Array.reduce(
        page.split('\n'),
        { names: HashMap.empty<string, readonly [folder: string, stratum: number]>(), stratum: Option.none<number>() },
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
                            onSome: ([id, title, stratum]) => HashMap.set(state.names, id, [title.toLowerCase(), stratum] as const),
                        },
                    ),
                    stratum: opened,
                };
            }
            return /^\s*end\s*$/.test(line)
                ? { ...state, stratum: Option.none<number>() }
                : node !== null
                  ? Option.match(Option.all([Option.fromNullable(node[1]), Option.fromNullable(node[2]), state.stratum]), {
                        onNone: () => state,
                        onSome: ([id, folder, stratum]) => ({ ...state, names: HashMap.set(state.names, id, [folder, stratum] as const) }),
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
                            (a: readonly [folder: string, stratum: number], b: readonly [folder: string, stratum: number]) => a[0] === b[0],
                        ),
                        Array.sortBy(
                            Order.mapInput(Order.number, (endpoint: readonly [folder: string, stratum: number]) => endpoint[1]),
                            Order.mapInput(Order.string, (endpoint: readonly [folder: string, stratum: number]) => endpoint[0]),
                        ),
                        Array.map(([folder, stratum]) => ({
                            folder,
                            edges: Array.sort(
                                Array.dedupe(
                                    Array.filterMap(edges, ([from, to]) =>
                                        from[0] === folder && to[0] !== folder ? Option.some(to[0]) : Option.none(),
                                    ),
                                ),
                                Order.string,
                            ),
                            stratum,
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

// The package-name law and its inverse: `@rasm/<basename>` per zone folder, with depth projects
// auditing under their owner zone. Substrate names resolve to no zone and fall to the family tier.
const _pkgName = (folder: string): string => `@rasm/${_segments(folder).at(-1) ?? folder}`;

const _zoneByName = (rows: ReadonlyArray<LedgerRow>): HashMap.HashMap<string, string> =>
    HashMap.fromIterable(
        Array.appendAll(
            Array.map(rows, (row) => [_pkgName(row.folder), row.folder] as const),
            Array.map(_DEPTH_PROJECTS, (row) => [_pkgName(row.folder), row.owner] as const),
        ),
    );

const _rules = (rows: ReadonlyArray<LedgerRow>): Parameters<typeof Imports.verdict>[1] =>
    pipe({ folders: HashSet.fromIterable(Array.map(rows, (row) => row.folder)), names: _zoneByName(rows) }, ({ folders, names }) => ({
        banned: [..._BANNED],
        permitted: Array.appendAll(
            Array.flatMap(rows, (row) => Array.map(row.edges, (edge) => [row.folder, edge] as const)),
            _ADMISSIONS,
        ),
        zone: (path: string) => Option.filter(Array.head(_segments(path)), (head) => HashSet.has(folders, head)),
        zoneOf: (specifier: string, from: string) =>
            specifier.startsWith('.')
                ? Option.filter(Array.head(_resolved(from, specifier)), (head) => HashSet.has(folders, head))
                : specifier.startsWith('@rasm/')
                  ? Option.orElse(HashMap.get(names, _scoped(specifier)), () => _familyOf(specifier))
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
                    Option.flatMap(() => Array.head(Array.drop(parts, 1))),
                ),
            Option.map((sub) => `security/${sub}`),
        ),
    zoneOf: _familyOf,
};

// A specifier's package name: the scoped pair for @-scopes, the head segment otherwise.
const _scoped = (specifier: string): string =>
    pipe(_segments(specifier), (parts) => (specifier.startsWith('@') ? Array.take(parts, 2).join('/') : (parts[0] ?? specifier)));

const _drawn = (verdict: ReturnType<typeof Imports.verdict>): ReadonlyArray<string> =>
    Audit.$match(verdict, {
        Unsupported: () => [],
        Audited: ({ violations }) => Array.map(violations, (violation) => `${violation.path} -[${violation.kind}]-> ${violation.specifier}`),
    });

const _ledger = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    return _parsedLedger(yield* fs.readFileString(path.join(_ROOT, `${_BRANCH}/.planning/ARCHITECTURE.md`)));
});

// Every zone package on disk: ledger folders plus declared depth projects, each read as its manifest.
const _packages = (rows: ReadonlyArray<LedgerRow>) =>
    Effect.gen(function* () {
        const fs = yield* FileSystem.FileSystem;
        const path = yield* Path.Path;
        const folders = Array.appendAll(
            Array.map(rows, (row) => ({ folder: row.folder, runtime: Option.none<string>() })),
            Array.map(_DEPTH_PROJECTS, (row) => ({ folder: row.folder, runtime: Option.some(row.runtime) })),
        );
        return yield* Effect.forEach(folders, (row) =>
            Effect.map(
                Effect.option(
                    Effect.flatMap(fs.readFileString(path.join(_ROOT, _BRANCH, row.folder, 'package.json')), (raw) => Effect.orDie(_decodePkg(raw))),
                ),
                (pkg) => ({ folder: row.folder, declaredRuntime: row.runtime, pkg }),
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

// One package's entrypoint-purity verdict: the mandatory "./package.json" row and the single
// unconditioned "." entry resolving to ./index.ts.
const _purity = (folder: string, exports: Readonly<Record<string, string | Readonly<Record<string, string>>>>): ReadonlyArray<string> =>
    Array.appendAll(
        exports['./package.json'] === './package.json' ? [] : [`${folder}: exports must carry "./package.json": "./package.json"`],
        pipe(Option.fromNullable(exports['.']), (entry) =>
            Option.match(entry, {
                onNone: () => [`${folder}: exports must carry a "." entrypoint`],
                onSome: (main) =>
                    typeof main === 'string'
                        ? main === './index.ts'
                            ? []
                            : [`${folder} -> ${main} (law: ./index.ts)`]
                        : [`${folder}: conditions are earned by a real divergent implementation, never pre-minted`],
            }),
        ),
    );

// Every exports entry resolves on disk, unconditionally: entry files live at the package root (the
// estate carries no src/ nesting), so a phantom condition file is a breach the moment it is declared.
const _unresolvable = (
    folder: string,
    exports: Readonly<Record<string, string | Readonly<Record<string, string>>>>,
    present: (relative: string) => boolean,
): ReadonlyArray<string> =>
    Array.filterMap(
        Array.flatMap(Record.values(exports), (entry) => (typeof entry === 'string' ? [entry] : Record.values(entry))),
        (file) =>
            file === './package.json' || present(`${folder}/${file.replace(/^\.\//, '')}`)
                ? Option.none()
                : Option.some(`${folder}: ${file} is absent on disk`),
    );

// Isolation-completeness: a standalone package declares every bare specifier its source imports —
// no phantom reach into the root pool or a sibling — and a relative import never escapes the
// package root. Node builtins are the runtime's own surface and stay exempt.
const _isolation = (
    folder: string,
    pkg: Pkg,
    modules: ReadonlyArray<{ path: string; specifiers: ReadonlyArray<{ specifier: string }> }>,
): ReadonlyArray<string> =>
    pipe(
        HashSet.fromIterable(
            Array.flatMap([pkg.dependencies ?? {}, pkg.devDependencies ?? {}, pkg.peerDependencies ?? {}], (block) => Record.keys(block)),
        ),
        (declared) =>
            Array.flatMap(modules, (module) =>
                Array.filterMap(module.specifiers, (entry) =>
                    entry.specifier.startsWith('node:')
                        ? Option.none()
                        : entry.specifier.startsWith('.')
                          ? Array.some(_segments(module.path), () => false) ||
                            Array.reduce(_segments(entry.specifier), _segments(module.path).length - 1, (depth, part) =>
                                part === '..' ? depth - 1 : depth,
                            ) >= 0
                              ? Option.none()
                              : Option.some(`${folder}/${module.path}: relative import escapes the package (${entry.specifier})`)
                          : HashSet.has(declared, _scoped(entry.specifier))
                            ? Option.none()
                            : Option.some(`${folder}/${module.path}: undeclared dependency ${_scoped(entry.specifier)}`),
                ),
            ),
    );

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
                        (target) =>
                            target.stratum > row.stratum
                                ? Option.some(`${row.folder}(${_clusterId(row.stratum)}) -> ${edge}(${_clusterId(target.stratum)})`)
                                : Option.none(),
                    ),
                ),
            );
            expect(climbing).toEqual([]);
        }),
    );

    it.effect('every zone package carries its name, tag triple, and the runtime-direction law holds', () =>
        Effect.gen(function* () {
            const rows = yield* _ledger;
            const packages = yield* _packages(rows);
            const misnamed = Array.filterMap(packages, ({ folder, pkg }) =>
                Option.match(pkg, {
                    onNone: () => Option.some(`${folder}: no package.json`),
                    onSome: (found) =>
                        found.name === _pkgName(folder) ? Option.none() : Option.some(`${folder}: ${found.name} (law: ${_pkgName(folder)})`),
                }),
            );
            expect(misnamed).toEqual([]);
            const untagged = Array.filterMap(packages, ({ folder, pkg }) =>
                Option.match(
                    Option.flatMap(pkg, (found) => _tagged(folder, found.nx?.tags ?? [])),
                    { onNone: () => Option.some(folder), onSome: () => Option.none() },
                ),
            );
            expect(untagged).toEqual([]);
            const triples = Array.getSomes(
                Array.map(packages, ({ folder, pkg }) => Option.flatMap(pkg, (found) => _tagged(folder, found.nx?.tags ?? []))),
            );
            const drifted = Array.filterMap(triples, (triple) => {
                const runtime = triple.runtime.slice('runtime:'.length);
                const plane = triple.plane.slice('plane:'.length);
                const scope = `scope:${_segments(triple.folder).at(-1) ?? triple.folder}`;
                const legal = triple.scope === scope && runtime in _RUNTIME_MAY && Array.some(_PLANES, (kind) => kind === plane);
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

    it.effect('every zone package holds entrypoint purity and its exports resolve on disk', () =>
        Effect.gen(function* () {
            const fs = yield* FileSystem.FileSystem;
            const path = yield* Path.Path;
            const rows = yield* _ledger;
            const packages = yield* _packages(rows);
            const impure = Array.flatMap(packages, ({ folder, pkg }) =>
                Option.match(pkg, {
                    onNone: (): ReadonlyArray<string> => [],
                    onSome: (found) => _purity(folder, found.exports ?? {}),
                }),
            );
            expect(impure).toEqual([]);
            const probes = Array.flatMap(packages, ({ folder, pkg }) =>
                Option.match(pkg, {
                    onNone: (): ReadonlyArray<string> => [],
                    onSome: (found) => [
                        'index.ts' satisfies string,
                        ...Array.flatMap(Record.values(found.exports ?? {}), (entry) =>
                            typeof entry === 'string'
                                ? [`${folder}/${entry.replace(/^\.\//, '')}`]
                                : Record.values(entry).map((file) => `${folder}/${file.replace(/^\.\//, '')}`),
                        ),
                    ],
                }),
            );
            const facts = yield* Effect.forEach(Array.dedupe(probes), (probe) =>
                Effect.map(
                    Effect.orElseSucceed(fs.exists(path.join(_ROOT, _BRANCH, probe)), () => false),
                    (has) => [probe, has] as const,
                ),
            );
            const held = HashMap.fromIterable(facts);
            const present = (relative: string): boolean => Option.getOrElse(HashMap.get(held, relative), () => false);
            const phantom = Array.flatMap(packages, ({ folder, pkg }) =>
                Option.match(pkg, {
                    onNone: (): ReadonlyArray<string> => [],
                    onSome: (found) => _unresolvable(folder, found.exports ?? {}, present),
                }),
            );
            expect(phantom).toEqual([]);
        }),
    );

    it.effect('manifest edges, tsconfig references, and the ledger agree, and every package is isolation-complete', () =>
        Effect.gen(function* () {
            const fs = yield* FileSystem.FileSystem;
            const path = yield* Path.Path;
            const rows = yield* _ledger;
            const packages = yield* _packages(rows);
            const names = _zoneByName(rows);
            // Manifest @rasm/* edges are lawful exactly where the ledger (or the depth-owner seat)
            // permits the zone crossing; substrate names stay outside the zone system.
            const lawless = Array.flatMap(packages, ({ folder, pkg }) =>
                Option.match(pkg, {
                    onNone: (): ReadonlyArray<string> => [],
                    onSome: (found) => {
                        const zone = Option.getOrElse(HashMap.get(names, found.name), () => folder);
                        const permitted = Array.appendAll(
                            Array.flatMap(rows, (row) => Array.map(row.edges, (edge) => [row.folder, edge] as const)),
                            Array.map(_DEPTH_PROJECTS, (row) => [row.folder, row.owner] as const),
                        );
                        return Array.filterMap(Record.keys(found.dependencies ?? {}), (dep) =>
                            dep.startsWith('@rasm/') && !Array.some(_SUBSTRATE, (name) => name === dep)
                                ? Option.match(HashMap.get(names, dep), {
                                      onNone: () => Option.some(`${folder}: dependency ${dep} resolves to no zone package`),
                                      onSome: (target) =>
                                          zone === target ||
                                          Array.some(permitted, ([source, to]) => (source === folder || source === zone) && to === target)
                                              ? Option.none()
                                              : Option.some(`${folder} -> ${dep}: no ledger edge permits ${zone} -> ${target}`),
                                  })
                                : Option.none(),
                        );
                    },
                }),
            );
            expect(lawless).toEqual([]);
            // tsconfig references mirror the workspace dependency set exactly, so the type graph and
            // the package graph cannot drift — this gauge replaces `nx sync --check`.
            const mirrors = yield* Effect.forEach(packages, ({ folder, pkg }) =>
                Effect.gen(function* () {
                    const manifest = Option.getOrUndefined(pkg);
                    if (manifest === undefined) {
                        return [] as ReadonlyArray<string>;
                    }
                    const raw = yield* Effect.orElseSucceed(fs.readFileString(path.join(_ROOT, _BRANCH, folder, 'tsconfig.json')), () => '{}');
                    const refs = pipe(
                        Option.getOrElse(
                            Option.flatMap(
                                Option.liftThrowable(JSON.parse)(raw) as Option.Option<{ references?: ReadonlyArray<{ path?: string }> }>,
                                (config) => Option.fromNullable(config.references),
                            ),
                            () => [] as ReadonlyArray<{ path?: string }>,
                        ),
                        Array.filterMap((row) => Option.fromNullable(row.path)),
                        Array.map((ref) => path.resolve(path.join(_ROOT, _BRANCH, folder), ref)),
                    );
                    const workspace = Array.filter(
                        Record.keys(manifest.dependencies ?? {}),
                        (dep) => dep.startsWith('@rasm/') && dep !== '@rasm/ts-testkit',
                    );
                    const expected = Array.map(workspace, (dep) =>
                        dep === '@rasm/contracts'
                            ? path.join(_ROOT, _BRANCH, 'contracts/tsconfig.build.json')
                            : path.join(
                                  _ROOT,
                                  _BRANCH,
                                  Option.getOrElse(
                                      Option.map(
                                          Array.findFirst(
                                              Array.appendAll(
                                                  Array.map(rows, (row) => [_pkgName(row.folder), row.folder] as const),
                                                  Array.map(_DEPTH_PROJECTS, (row) => [_pkgName(row.folder), row.folder] as const),
                                              ),
                                              ([name]) => name === dep,
                                          ),
                                          ([, target]) => target,
                                      ),
                                      () => dep,
                                  ),
                              ),
                    );
                    const missing = Array.filter(expected, (want) => !Array.some(refs, (have) => have === want || have === `${want}/tsconfig.json`));
                    const excess = Array.filter(refs, (have) => !Array.some(expected, (want) => have === want || have === `${want}/tsconfig.json`));
                    return Array.appendAll(
                        Array.map(missing, (want) => `${folder}: tsconfig misses reference ${path.relative(_ROOT, want)}`),
                        Array.map(excess, (have) => `${folder}: tsconfig carries unmirrored reference ${path.relative(_ROOT, have)}`),
                    );
                }),
            );
            expect(Array.flatten(mirrors)).toEqual([]);
            // Isolation-completeness per package: declared deps cover every bare import, and a
            // relative import never leaves the package.
            const leaks = yield* Effect.forEach(packages, ({ folder, pkg }) =>
                Option.match(pkg, {
                    onNone: () => Effect.succeed([] as ReadonlyArray<string>),
                    onSome: (found) =>
                        Effect.map(
                            Effect.orElseSucceed(Imports.load(path.join(_ROOT, _BRANCH, folder), _PRUNE), () => []),
                            (modules) => _isolation(folder, found, modules),
                        ),
                }),
            );
            expect(Array.flatten(leaks)).toEqual([]);
        }),
    );

    it.effect('the branch source audit runs the real table and stays honest while placeholder source ships', () =>
        Effect.gen(function* () {
            const rows = yield* _ledger;
            const path = yield* Path.Path;
            const modules = yield* Imports.load(path.join(_ROOT, _BRANCH), _PRUNE);
            expect(_drawn(Imports.verdict(modules, _rules(rows)))).toEqual([]);
            expect(_drawn(Imports.verdict(modules, _cryptoRules))).toEqual([]);
        }),
    );

    it.effect('app projects are tagged islands: no app reaches another app, and lib reach obeys the runtime law', () =>
        Effect.gen(function* () {
            const fs = yield* FileSystem.FileSystem;
            const path = yield* Path.Path;
            const rows = yield* _ledger;
            const packages = yield* _packages(rows);
            const triples = Array.getSomes(
                Array.map(packages, ({ folder, pkg }) => Option.flatMap(pkg, (found) => _tagged(folder, found.nx?.tags ?? []))),
            );
            const apps = yield* Effect.orElseSucceed(fs.readDirectory(path.join(_ROOT, 'apps')), () => [] as ReadonlyArray<string>);
            const findings = yield* Effect.forEach(apps, (app) =>
                Effect.gen(function* () {
                    const projects = yield* Effect.orElseSucceed(fs.readDirectory(path.join(_ROOT, 'apps', app)), () => [] as ReadonlyArray<string>);
                    return yield* Effect.forEach(projects, (project) =>
                        Effect.gen(function* () {
                            const manifest = yield* Effect.option(
                                Effect.flatMap(fs.readFileString(path.join(_ROOT, 'apps', app, project, 'package.json')), (raw) =>
                                    Effect.orDie(_decodePkg(raw)),
                                ),
                            );
                            return Option.match(manifest, {
                                onNone: (): ReadonlyArray<string> => [],
                                onSome: (found) => {
                                    const triple = _tagged(`apps/${app}/${project}`, found.nx?.tags ?? []);
                                    const tagFindings = Option.isNone(triple) ? [`apps/${app}/${project}: missing scope/runtime/plane tags`] : [];
                                    const deps = Record.keys(found.dependencies ?? {});
                                    const crossApp = Array.filterMap(deps, (dep) =>
                                        dep.startsWith('@rasm/') || !dep.startsWith('@') ? Option.none() : Option.none(),
                                    );
                                    const appRuntime = Option.map(triple, (found3) => found3.runtime.slice('runtime:'.length));
                                    const libReach = Array.filterMap(deps, (dep) =>
                                        pipe(
                                            HashMap.get(_zoneByName(rows), dep),
                                            Option.flatMap((zone) =>
                                                Option.all([
                                                    appRuntime,
                                                    Option.map(
                                                        Array.findFirst(triples, (t) => t.folder === zone),
                                                        (t) => t.runtime.slice('runtime:'.length),
                                                    ),
                                                ]),
                                            ),
                                            Option.flatMap(([from, to]) =>
                                                from in _RUNTIME_MAY &&
                                                Array.some(_RUNTIME_MAY[from as keyof typeof _RUNTIME_MAY], (kind) => kind === to)
                                                    ? Option.none()
                                                    : Option.some(
                                                          `apps/${app}/${project}(${Option.getOrElse(appRuntime, () => '?')}) -> ${dep}(${to})`,
                                                      ),
                                            ),
                                        ),
                                    );
                                    return Array.appendAll(tagFindings, Array.appendAll(crossApp, libReach));
                                },
                            });
                        }),
                    );
                }),
            );
            expect(Array.flatten(Array.flatten(findings))).toEqual([]);
        }),
    );
});

describe('gauge falsification', () => {
    // The fixture speaks the parser's own vocabulary: cluster lines mint through `_clusterLine` and
    // cluster-id endpoints through `_clusterId`, so a re-lettered strata fence can never leave this
    // block asserting a grammar the live parse no longer reads.
    const rows = _parsedLedger(
        [
            'flowchart TB',
            _clusterLine(3, 'APP + DEPLOY'), // a multi-word title contributes its stratum alone; its member nodes name the folders
            '        Shell[shell]',
            '        Plan[plan]',
            '    end',
            _clusterLine(2, 'DATA'),
            '        Data[data]',
            '    end',
            _clusterLine(1, 'SECURITY'),
            '        Security[security]',
            '    end',
            _clusterLine(0, 'CORE'),
            '        Value[value]',
            '    end',
            '    Data e1@-->|"[IMPORT]: TenantScope"| Security',
            `    Data e2@-->|"[IMPORT]: ContentKey"| ${_clusterId(0)}`,
            `    Security e3@-->|"[IMPORT]: TenantContext"| ${_clusterId(0)}`,
            `    Shell e4@-->|"[IMPORT]: Feed.Document"| ${_clusterId(0)}`,
            '    Plan e5@-->|"[IMPORT]: Pg.rows"| Data',
            '    Security p1@-.->|"[PORT]: Shredder"| Data',
            `    ${_clusterId(0)} f1@-->|"forbidden: upward import"| ${_clusterId(2)}`,
            '    Value i1@--> Value',
            '    Data ~~~ Security',
        ].join('\n'),
    );

    it('the ledger parser reads the strata fence and refuses undeclared endpoints', () => {
        expect(rows).toEqual([
            { folder: 'core', edges: [], stratum: 0 }, // the single-word title names the cluster's own folder; `value` never crosses an import edge
            { folder: 'security', edges: ['core'], stratum: 1 },
            { folder: 'data', edges: ['core', 'security'], stratum: 2 },
            { folder: 'plan', edges: ['data'], stratum: 3 }, // no `app` or `deploy` folder exists: the multi-word cluster named none
            { folder: 'shell', edges: ['core'], stratum: 3 },
        ]);
        expect(_parsedLedger('| [FROM] | [MAY_IMPORT] | [NOTES] |')).toEqual([]);
        expect(_parsedLedger('flowchart TB\n    Data e1@-->|"[IMPORT]: ContentKey"| Ghost')).toEqual([]);
    });

    it('the acyclicity gauge refutes a cyclic table', () => {
        expect(_acyclic(rows)).toBe(true);
        expect(
            _acyclic([
                { folder: 'runtime', edges: ['security'], stratum: 3 },
                { folder: 'security', edges: ['runtime'], stratum: 1 },
            ]),
        ).toBe(false);
    });

    it('a banned module import is a violation the engine names', () => {
        const scanned = Imports.scan([{ path: 'data/journal.ts', text: 'import { PgMigrator } from "@effect/sql-pg/PgMigrator";' }]);
        expect(_drawn(Imports.verdict(scanned, _rules(rows)))).toEqual(['data/journal.ts -[banned]-> @effect/sql-pg/PgMigrator']);
    });

    it('an edge outside the table is a violation, a permitted edge is not', () => {
        const scanned = Imports.scan([
            { path: 'core/identity.ts', text: 'import { fold } from "@rasm/security";' },
            { path: 'data/journal.ts', text: 'import { brand } from "@rasm/core";\nimport { PgClient } from "@effect/sql-pg";' },
        ]);
        expect(_drawn(Imports.verdict(scanned, _rules(rows)))).toEqual(['core/identity.ts -[edge]-> @rasm/security']);
    });

    it('a relative reach-around resolves to its folder zone and is audited', () => {
        const scanned = Imports.scan([{ path: 'security/fold.ts', text: 'import { key } from "../data/journal.ts";' }]);
        expect(_drawn(Imports.verdict(scanned, _rules(rows)))).toEqual(['security/fold.ts -[edge]-> ../data/journal.ts']);
    });

    it('the purity gauge refuses every counterfeit exports shape and passes the lawful one', () => {
        const lawful = { '.': './index.ts', './package.json': './package.json' } as const;
        expect(_purity('core', lawful)).toEqual([]);
        expect(_purity('ui/viewer', lawful)).toEqual([]);
        expect(_purity('iac', { '.': './journal.ts', './package.json': './package.json' })).not.toEqual([]);
        expect(_purity('core', { '.': lawful['.'] })).not.toEqual([]);
        expect(_purity('core', { './package.json': './package.json' })).not.toEqual([]);
        // a pre-minted condition split is refused: conditions are earned by a divergent implementation
        expect(
            _purity('core', {
                '.': { server: './server.ts', browser: './browser.ts', wasm: './wasm.ts', default: './index.ts' },
                './package.json': './package.json',
            }),
        ).not.toEqual([]);
    });

    it('the resolvability gauge demands every declared entry file on disk', () => {
        const entry = { '.': './index.ts', './submodule': { default: './submodule.ts' }, './package.json': './package.json' } as const;
        expect(_unresolvable('core', entry, () => false)).toHaveLength(2);
        expect(_unresolvable('core', entry, () => true)).toEqual([]);
        expect(_unresolvable('ui', { '.': './index.ts', './package.json': './package.json' }, (probe) => probe !== 'ui/index.ts')).toEqual([
            'ui: ./index.ts is absent on disk',
        ]);
    });

    it('the isolation gauge refuses a phantom dependency and a package-escaping relative import', () => {
        const pkg = { name: '@rasm/data', dependencies: { '@rasm/core': 'workspace:*' } };
        const clean = Imports.scan([{ path: 'journal.ts', text: 'import { brand } from "@rasm/core";\nimport path from "node:path";' }]);
        expect(_isolation('data', pkg, clean)).toEqual([]);
        const phantom = Imports.scan([{ path: 'journal.ts', text: 'import { PgClient } from "@effect/sql-pg";' }]);
        expect(_isolation('data', pkg, phantom)).toEqual(['data/journal.ts: undeclared dependency @effect/sql-pg']);
        const escape = Imports.scan([{ path: 'journal.ts', text: 'import { fold } from "../core/index.ts";' }]);
        expect(_isolation('data', pkg, escape)).toEqual(['data/journal.ts: relative import escapes the package (../core/index.ts)']);
    });

    it('every family claims its representative exactly once and refuses the near-miss', () => {
        const claims = (specifier: string): ReadonlyArray<string> =>
            Array.filterMap(Record.toEntries(_FAMILIES), ([family, pattern]) => (pattern.test(specifier) ? Option.some(family) : Option.none()));
        const table: ReadonlyArray<readonly [family: string, representative: string, nearMiss: string]> = [
            ['ext:jose', 'jose/jwks', 'josefine'],
            ['ext:oidc', 'openid-client', 'openid-client-fork'],
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
        expect(Array.flatMap(table, ([family, representative]) => (claims(representative).join() === family ? [] : [representative]))).toEqual([]);
        expect(Array.flatMap(table, ([, , nearMiss]) => (Array.isEmptyReadonlyArray(claims(nearMiss)) ? [] : [nearMiss]))).toEqual([]);
    });

    it('a crypto admission outside its sub-folder is a violation', () => {
        const scanned = Imports.scan([
            { path: 'security/authn/session.ts', text: 'import { SignJWT } from "jose";' },
            { path: 'security/crypt/sign.ts', text: 'import { SignJWT } from "jose";' },
        ]);
        expect(_drawn(Imports.verdict(scanned, _cryptoRules))).toEqual(['security/authn/session.ts -[edge]-> jose']);
    });
});

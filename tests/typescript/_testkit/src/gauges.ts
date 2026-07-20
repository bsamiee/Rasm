import { FileSystem, Path } from '@effect/platform';
import { parseSync } from '@swc/core';
import { Array, Data, Effect, HashSet, Option, Predicate, pipe } from 'effect';

// --- [TYPES] -----------------------------------------------------------------------------

declare namespace Imports {
    type Specifier = { readonly specifier: string; readonly typeOnly: boolean };
    type Module = { readonly path: string; readonly specifiers: ReadonlyArray<Specifier> };
    type Violation = {
        readonly path: string;
        readonly specifier: string;
        readonly kind: 'banned' | 'edge';
        readonly typeOnly: boolean;
    };
    type Rules = {
        readonly banned: ReadonlyArray<RegExp>;
        readonly zone: (path: string) => Option.Option<string>;
        readonly zoneOf: (specifier: string, from: string) => Option.Option<string>;
        readonly permitted: ReadonlyArray<readonly [from: string, to: string]>;
    };
}

declare namespace Snapshots {
    // A golden dialect is a row on the one audit engine: claim the snapshot entries, project the candidate owning specs.
    type Dialect = {
        readonly snapshot: (entry: string) => boolean;
        readonly owners: (entry: string, path: Path.Path) => ReadonlyArray<string>;
    };
}

type Audit = Data.TaggedEnum<{
    Unsupported: { readonly reason: string };
    Audited: { readonly scanned: number; readonly violations: ReadonlyArray<Imports.Violation> };
}>;
const Audit: Data.TaggedEnum.Constructor<Audit> = Data.taggedEnum<Audit>();

// --- [CONSTANTS] -------------------------------------------------------------------------

const _SNAP = { home: '__snapshots__', extension: '.snap' } as const;
const _GOLDEN = { home: 'goldens', spec: /\.pw\.[mc]?ts$/ } as const;

// Every compilable source dialect joins the audit — a .tsx or .mts module must never evade a banned-module verdict.
const _SOURCE = { include: /\.(?:[mc]?ts|tsx)$/, declaration: /\.d\.[mc]?ts$/ } as const;

// Dependency, build, output, cache, and VCS trees never join a source audit: a routed artifact or a workspace symlink under the root would sweep foreign code.
const _PRUNE = /(^|\/)(node_modules|dist|build|coverage|\.git|\.cache|\.artifacts)(\/|$)/;

// --- [ERRORS] ----------------------------------------------------------------------------

class GaugeFault extends Data.TaggedError('GaugeFault')<{ readonly reason: 'unreadable'; readonly detail: string }> {}

// --- [OPERATIONS] ------------------------------------------------------------------------

const _walked = (root: string): Effect.Effect<ReadonlyArray<string>, GaugeFault, FileSystem.FileSystem> =>
    Effect.mapError(
        Effect.flatMap(FileSystem.FileSystem, (fs) => fs.readDirectory(root, { recursive: true })),
        (fault) => new GaugeFault({ reason: 'unreadable', detail: fault.message }),
    );

const _specifiers = (path: string, text: string): ReadonlyArray<Imports.Specifier> => {
    // BOUNDARY ADAPTER: the swc parse walk is a native callback seam; the accumulator detaches immutable at the return.
    // The walk is recursive over every AST value so a dynamic `import("<specifier>")` buried in a body cannot evade a
    // banned-module verdict, and an unparsable source throws loud — a span that fails to parse proves nothing.
    const found: Array<Imports.Specifier> = [];
    const visit = (node: unknown): void => {
        if (Array.isArray(node)) {
            for (const item of node) {
                visit(item);
            }
            return;
        }
        if (!Predicate.isRecord(node)) {
            return;
        }
        const { source, type } = node;
        if (
            (type === 'ImportDeclaration' || type === 'ExportAllDeclaration' || type === 'ExportNamedDeclaration') &&
            Predicate.isRecord(source) &&
            Predicate.isString(source['value'])
        ) {
            found.push({ specifier: source['value'], typeOnly: node['typeOnly'] === true });
        } else if (type === 'CallExpression' && Predicate.isRecord(node['callee']) && node['callee']['type'] === 'Import') {
            const head = Array.isArray(node['arguments']) ? node['arguments'][0] : undefined;
            const expression = Predicate.isRecord(head) ? head['expression'] : undefined;
            if (Predicate.isRecord(expression) && expression['type'] === 'StringLiteral' && Predicate.isString(expression['value'])) {
                found.push({ specifier: expression['value'], typeOnly: false });
            }
        }
        for (const value of Object.values(node)) {
            visit(value);
        }
    };
    visit(parseSync(text, { syntax: 'typescript', tsx: path.endsWith('.tsx'), decorators: true }));
    return found;
};

// {home}/{...template metadata}/{...specPath}/{arg}: the metadata run between the home and the spec-suffixed segment is
// snapshotPathTemplate data (project, platform, ...), so every metadata/spec-path split is a candidate owner — the gauge
// never couples to one template arity.
const _goldenOwners = (entry: string): ReadonlyArray<string> => {
    const segments = entry.split('/');
    const home = segments.indexOf(_GOLDEN.home);
    return pipe(
        Array.findFirstIndex(segments, (segment) => _GOLDEN.spec.test(segment)),
        Option.filter((at) => home >= 0 && at > home && segments.length > at + 1),
        Option.match({
            onNone: (): ReadonlyArray<string> => [],
            onSome: (at) => Array.map(Array.range(home + 1, at), (from) => [...segments.slice(0, home), ...segments.slice(from, at + 1)].join('/')),
        }),
    );
};

// The golden dialect rows: each claims its snapshot entries and projects candidate owning specs; a new golden layout is a row.
const _DIALECTS = {
    // Runner snapshots: __snapshots__/<spec>.snap owned by the sibling spec beside the snapshot home.
    runner: {
        snapshot: (entry) => entry.endsWith(_SNAP.extension) && entry.includes(_SNAP.home),
        owners: (entry, path) => [path.join(path.dirname(path.dirname(entry)), path.basename(entry, _SNAP.extension))],
    },
    // Playwright goldens: the snapshotPathTemplate embeds the owning spec path as the directory run closing at the spec segment.
    playwright: {
        snapshot: (entry) => Array.isNonEmptyReadonlyArray(_goldenOwners(entry)),
        owners: (entry) => _goldenOwners(entry),
    },
} as const satisfies Record<string, Snapshots.Dialect>;

// Standing snapshot-hygiene gauge over every golden dialect: a snapshot no candidate spec owns — or that no dialect can
// attribute at all — is stale evidence, flagged for deletion.
const Snapshots = {
    dialects: _DIALECTS,
    audit: (
        root: string,
        dialects: ReadonlyArray<Snapshots.Dialect> = [_DIALECTS.runner, _DIALECTS.playwright],
    ): Effect.Effect<{ readonly scanned: number; readonly orphans: ReadonlyArray<string> }, GaugeFault, FileSystem.FileSystem | Path.Path> =>
        Effect.gen(function* () {
            const path = yield* Path.Path;
            const entries = yield* _walked(root);
            const present = HashSet.fromIterable(entries);
            const claimed = Array.filterMap(entries, (entry) =>
                Option.map(
                    Array.findFirst(dialects, (dialect) => dialect.snapshot(entry)),
                    (dialect) => ({ entry, dialect }),
                ),
            );
            const orphans = Array.filterMap(claimed, ({ entry, dialect }) =>
                Array.some(dialect.owners(entry, path), (owner) => HashSet.has(present, owner)) ? Option.none() : Option.some(entry),
            );
            return { scanned: claimed.length, orphans };
        }),
} as const;

// Import-graph gauge engine: pure over supplied sources, parameterized by zone projection and permitted-edge rows.
const Imports = {
    scan: (sources: ReadonlyArray<{ readonly path: string; readonly text: string }>): ReadonlyArray<Imports.Module> =>
        Array.map(sources, (source) => ({ path: source.path, specifiers: _specifiers(source.path, source.text) })),
    load: (root: string, prune: RegExp = _PRUNE): Effect.Effect<ReadonlyArray<Imports.Module>, GaugeFault, FileSystem.FileSystem | Path.Path> =>
        Effect.gen(function* () {
            const fs = yield* FileSystem.FileSystem;
            const path = yield* Path.Path;
            const entries = yield* _walked(root);
            const files = Array.filter(entries, (entry) => _SOURCE.include.test(entry) && !_SOURCE.declaration.test(entry) && !prune.test(entry));
            return Imports.scan(
                yield* Effect.forEach(files, (file) =>
                    Effect.map(
                        Effect.mapError(
                            fs.readFileString(path.join(root, file)),
                            (fault) => new GaugeFault({ reason: 'unreadable', detail: fault.message }),
                        ),
                        (text) => ({ path: file, text }),
                    ),
                ),
            );
        }),
    // An empty module set is UNSUPPORTED, never a vacuous green: the gauge names what it could not audit.
    verdict: (modules: ReadonlyArray<Imports.Module>, rules: Imports.Rules): Audit =>
        Array.isNonEmptyReadonlyArray(modules)
            ? Audit.Audited({
                  scanned: modules.length,
                  violations: Array.flatMap(modules, (module) =>
                      Array.filterMap(module.specifiers, (entry) =>
                          Array.some(rules.banned, (pattern) => pattern.test(entry.specifier))
                              ? Option.some<Imports.Violation>({
                                    path: module.path,
                                    specifier: entry.specifier,
                                    kind: 'banned',
                                    typeOnly: entry.typeOnly,
                                })
                              : pipe(
                                    Option.all([rules.zone(module.path), rules.zoneOf(entry.specifier, module.path)]),
                                    Option.filter(
                                        ([from, to]) =>
                                            from !== to && !Array.some(rules.permitted, ([source, target]) => source === from && target === to),
                                    ),
                                    Option.map(
                                        (): Imports.Violation => ({
                                            path: module.path,
                                            specifier: entry.specifier,
                                            kind: 'edge',
                                            typeOnly: entry.typeOnly,
                                        }),
                                    ),
                                ),
                      ),
                  ),
              })
            : Audit.Unsupported({ reason: 'zero modules scanned' }),
} as const;

// --- [EXPORTS] ---------------------------------------------------------------------------

export { Audit, GaugeFault, Imports, Snapshots };

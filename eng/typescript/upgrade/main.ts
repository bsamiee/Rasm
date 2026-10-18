import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { compare as comparePep440, valid } from '@renovatebot/pep440';
import { Array, Cause, Effect, Exit, FileSystem, flow, Order, Path, Record, Schema, Stdio, Tuple } from 'effect';
import { parse as parseToml } from 'smol-toml';
import { compare, isValid } from 'verkit';
import { parse as parseYaml } from 'yaml';

// --- [OPERATIONS] ----------------------------------------------------------------------

const check = Effect.fn('check')(function* (baseline: string, lock: (typeof LOCKS)[number]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const read = (file: string) =>
        fs.readFileString(file).pipe(
            Effect.flatMap((text) => lock.rows(text).pipe(Effect.mapError((cause) => new Cause.UnknownError(cause, `${file} did not parse`)))),
            Effect.map(flow(Array.filter(flow(Tuple.get(1), lock.valid)), Array.groupBy(Tuple.get(0)), Record.map(Array.map(Tuple.get(1))))),
        );
    const [started, current] = yield* Effect.all([read(path.join(baseline, lock.file)), read(lock.file)], { concurrency: 'unbounded' });
    const top = (versions: readonly string[]) => (Array.isReadonlyArrayNonEmpty(versions) ? [Array.max(versions, lock.order)] : []);
    const downgraded = Object.entries(started).flatMap(([name, before]) => {
        const after = current[name] ?? before;
        return Array.cartesian(top(Array.difference(before, after)), top(Array.difference(after, before)))
            .filter(([from, to]) => Order.isLessThan(lock.order)(to, from))
            .map(([from, to]) => `${lock.file} downgraded ${name} ${from} -> ${to}`);
    });
    return yield* downgraded.length === 0 ? Effect.void : Effect.fail(new Cause.UnknownError(undefined, downgraded.join('\n')));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const toml = (text: string) =>
    Effect.try(() => parseToml(text)).pipe(
        Effect.flatMap(Schema.decodeUnknownEffect(Schema.Struct({ package: Schema.Array(Schema.Struct({ name: Schema.String, version: Schema.optionalKey(Schema.String) })) }))),
        Effect.map(({ package: listed }) => listed.flatMap(({ name, version }) => (version === undefined ? [] : [[name, version] as const]))),
    );
const semver = { valid: isValid, order: compare };
const LOCKS = [
    { file: 'Cargo.lock', rows: toml, ...semver },
    { file: 'uv.lock', rows: toml, valid: (version: string) => valid(version) !== null, order: (self: string, that: string) => Order.Number(comparePep440(self, that), 0) },
    {
        file: 'pnpm-lock.yaml',
        rows: (text: string) =>
            Effect.try(() => parseYaml(text)).pipe(
                Effect.flatMap(Schema.decodeUnknownEffect(Schema.Struct({ packages: Schema.Record(Schema.String, Schema.Unknown) }))),
                Effect.map(({ packages }) => Object.keys(packages).map((key) => [key.slice(0, key.lastIndexOf('@')), key.slice(key.lastIndexOf('@') + 1)] as const)),
            ),
        ...semver,
    },
];

Stdio.Stdio.pipe(
    Effect.flatMap((stdio) => stdio.args),
    Effect.flatMap(Schema.decodeUnknownEffect(Schema.Tuple([Schema.String]))),
    Effect.flatMap(([baseline]) => Effect.forEach(LOCKS, (lock) => Effect.exit(check(baseline, lock)), { concurrency: 'unbounded' })),
    Effect.flatMap(Exit.asVoidAll),
    Effect.provide(layer),
    runMain,
);

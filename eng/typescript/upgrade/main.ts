import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { compare as comparePep440, valid } from '@renovatebot/pep440';
import { Array, Cause, Effect, Exit, FileSystem, flow, Option, Order, Path, Record, Schema, String, Tuple } from 'effect';
import { parse as parseToml } from 'smol-toml';
import { compare, compareMain, getPrerelease, isValid } from 'verkit';
import { parse as parseYaml } from 'yaml';
import { stdout } from '#/eng/typescript/process';

// --- [MODELS] --------------------------------------------------------------------------

const toml = flow(
    (text: string) => Effect.try(() => parseToml(text)),
    Effect.flatMap(Schema.decodeUnknownEffect(Schema.Struct({ package: Schema.Array(Schema.Struct({ name: Schema.String, version: Schema.optionalKey(Schema.String) })) }))),
    Effect.map(({ package: listed }) => listed.flatMap(({ name, version }) => Option.toArray(Option.fromUndefinedOr(version)).map((found) => [name, found] as const))),
);
const registry = (name: string) => stdout('pnpm', ['view', name, 'time', '--json']).pipe(Effect.flatMap(Schema.decodeEffect(Schema.fromJsonString(Schema.Record(Schema.String, Schema.String)))));
const semver = { valid: isValid, order: compare, published: Option.none<typeof registry>() };
const LOCKS = {
    'Cargo.lock': { rows: toml, ...semver },
    'uv.lock': { rows: toml, valid: (version: string) => valid(version) !== null, order: (self: string, that: string) => Order.Number(comparePep440(self, that), 0), published: Option.none<typeof registry>() },
    'pnpm-lock.yaml': {
        rows: flow(
            (text: string) => Effect.try(() => parseYaml(text)),
            Effect.flatMap(Schema.decodeUnknownEffect(Schema.Struct({ packages: Schema.Record(Schema.String, Schema.Unknown) }))),
            Effect.map(({ packages }) => Object.keys(packages).map((key) => Tuple.make(key.slice(0, key.lastIndexOf('@')), key.slice(key.lastIndexOf('@') + 1)))),
        ),
        ...semver,
        published: Option.some(registry),
    },
    'Package.resolved': {
        rows: flow(
            Schema.decodeUnknownEffect(Schema.fromJsonString(Schema.Struct({ pins: Schema.Array(Schema.Struct({ identity: Schema.String, state: Schema.Struct({ version: Schema.optionalKey(Schema.String) }) })) }))),
            Effect.map(({ pins }) => pins.flatMap(({ identity, state }) => Option.toArray(Option.fromUndefinedOr(state.version)).map((found) => [identity, found] as const))),
        ),
        ...semver,
    },
};

// --- [ERRORS] --------------------------------------------------------------------------

class Downgrade extends Schema.TaggedError<Downgrade, { readonly brand: unique symbol }>()('Downgrade', { file: Schema.String, moves: Schema.NonEmptyArray(Schema.Struct({ name: Schema.String, from: Schema.String, to: Schema.String })) }) {
    override get message(): string {
        return this.moves.map(({ name, from, to }) => `${this.file} downgraded ${name} ${from} -> ${to}`).join('\n');
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const downgrades = Effect.fn('downgrades')(function* (file: string, lock: (typeof LOCKS)[keyof typeof LOCKS]) {
    const fs = yield* FileSystem.FileSystem;
    const versions = (text: string) =>
        lock.rows(text).pipe(
            Effect.mapError((cause) => new Cause.UnknownError(cause, `${file} did not parse`)),
            Effect.map(flow(Array.filter(flow(Tuple.get(1), lock.valid)), Array.groupBy(Tuple.get(0)), Record.map(Array.map(Tuple.get(1))))),
        );
    const [started, current] = yield* Effect.all([Effect.flatMap(stdout('git', ['show', `:${file}`]), versions), Effect.flatMap(fs.readFileString(file), versions)], { concurrency: 'unbounded' });
    const top = flow(Option.liftPredicate(Array.isReadonlyArrayNonEmpty<string>), Option.map(Array.max(lock.order)));
    const moves = Object.entries(started).flatMap(([name, before]) => {
        const after = current[name] ?? before;
        return Option.toArray(Option.filter(Option.all({ from: top(Array.difference(before, after)), to: top(Array.difference(after, before)) }), ({ from, to }) => Order.isLessThan(lock.order)(to, from))).map((move) => ({ name, ...move }));
    });
    const labelled = ({ from, to }: { readonly from: string; readonly to: string }) =>
        compareMain(from, to) === 0 &&
        Option.exists(
            Array.findFirst(Array.zip(getPrerelease(from) ?? [], getPrerelease(to) ?? []), ([left, right]) => left !== right),
            ([left, right]) => typeof left === 'string' || typeof right === 'string',
        );
    const confirmed = yield* Effect.filter(
        moves,
        (move) =>
            Option.match(
                Option.filter(lock.published, () => labelled(move)),
                {
                    onNone: () => Effect.succeed(true),
                    onSome: (published) => Effect.map(published(move.name), (times) => Option.match(Option.all([Record.get(times, move.to), Record.get(times, move.from)]), { onNone: () => true, onSome: ([to, from]) => to < from })),
                },
            ),
        { concurrency: 'unbounded' },
    );
    return yield* Array.match(confirmed, { onEmpty: () => Effect.void, onNonEmpty: (found) => Effect.fail(Downgrade.make({ file, moves: found })) });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const path = yield* Path.Path;
    const tracked = Array.groupBy((yield* stdout('git', ['ls-files', '--cached', '-z', '--', ...Object.keys(LOCKS).map((name) => `:(glob)**/${name}`)])).split('\0').filter(String.isNonEmpty), (file) => path.basename(file));
    const exits = yield* Effect.forEach(
        Object.entries(LOCKS).flatMap(([name, lock]) => (tracked[name] ?? []).map((file) => Tuple.make(file, lock))),
        ([file, lock]) => Effect.exit(downgrades(file, lock)),
        { concurrency: 'unbounded' },
    );
    return yield* Exit.asVoidAll(exits);
}).pipe(Effect.provide(layer), runMain);

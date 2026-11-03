import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { compare as comparePep440, valid } from '@renovatebot/pep440';
import { Array, Cause, Effect, FileSystem, flow, Option, Order, Path, Record, Schema, Tuple } from 'effect';
import { compare, compareMain, getPrerelease, isValid } from 'verkit';
import { accumulate } from '#/eng/typescript/accumulate';
import { Toml, Yaml } from '#/eng/typescript/parsed';
import { records, run } from '#/eng/typescript/process';

// --- [TYPES] ---------------------------------------------------------------------------

interface Lock {
    readonly rows: (text: string) => Effect.Effect<readonly (readonly [string, string])[], Schema.SchemaError>;
    readonly valid: (version: string) => boolean;
    readonly order: Order.Order<string>;
    readonly confirm: (move: typeof Move.Type) => Effect.Effect<boolean, Effect.Error<ReturnType<typeof registry>>, Effect.Services<ReturnType<typeof registry>>>;
}

// --- [MODELS] --------------------------------------------------------------------------

const TomlLock = Toml.pipe(Schema.decodeTo(Schema.Struct({ package: Schema.Array(Schema.Struct({ name: Schema.String, version: Schema.optionalKey(Schema.String) })) })));
const PnpmLock = Yaml.pipe(Schema.decodeTo(Schema.Struct({ packages: Schema.Record(Schema.String, Schema.Unknown) })));
const Resolved = Schema.fromJsonString(Schema.Struct({ pins: Schema.Array(Schema.Struct({ identity: Schema.String, state: Schema.Struct({ version: Schema.optionalKey(Schema.String) }) })) }));
const Published = Schema.fromJsonString(Schema.Record(Schema.String, Schema.String));
const Move = Schema.Struct({ name: Schema.String, from: Schema.String, to: Schema.String });

// --- [ERRORS] --------------------------------------------------------------------------

class Downgrade extends Schema.TaggedError<Downgrade, { readonly brand: unique symbol }>()('Downgrade', { file: Schema.String, moves: Schema.NonEmptyArray(Move) }) {
    override get message(): string {
        return this.moves.map(({ name, from, to }) => `${this.file} downgraded ${name} ${from} -> ${to}`).join('\n');
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [ROWS]

const pinned = (rows: readonly { readonly name: string; readonly version?: string }[]) => rows.flatMap(({ name, version }) => Option.toArray(Option.fromUndefinedOr(version)).map((found) => [name, found] as const));
const toml = (text: string) => Effect.map(Schema.decodeEffect(TomlLock)(text), ({ package: listed }) => pinned(listed));
const registry = (name: string) => run('pnpm', ['view', name, 'time', '--json']).pipe(Effect.flatMap(Schema.decodeEffect(Published)));

// --- [CHECKS]

const labelled = ({ from, to }: typeof Move.Type) =>
    compareMain(from, to) === 0 &&
    Option.exists(
        Array.findFirst(Array.zip(getPrerelease(from) ?? [], getPrerelease(to) ?? []), ([left, right]) => left !== right),
        ([left, right]) => typeof left === 'string' || typeof right === 'string',
    );
const downgrades = Effect.fn('downgrades')(function* (file: string, lock: Lock) {
    const fs = yield* FileSystem.FileSystem;
    const versions = (text: string) => lock.rows(text).pipe(Effect.mapBoth({ onFailure: (cause) => new Cause.UnknownError(cause, `${file} did not parse`), onSuccess: flow(Array.filter(flow(Tuple.get(1), lock.valid)), Array.groupBy(Tuple.get(0)), Record.map(Array.map(Tuple.get(1)))) }));
    const [started, current] = yield* Effect.all([Effect.flatMap(run('git', ['show', `:${file}`]), versions), Effect.flatMap(fs.readFileString(file), versions)], { concurrency: 'unbounded' });
    const top = flow(Option.liftPredicate(Array.isReadonlyArrayNonEmpty<string>), Option.map(Array.max(lock.order)));
    const moves = Record.collect(started, (name, before) => {
        const after = current[name] ?? before;
        return Option.map(
            Option.filter(Option.all({ from: top(Array.difference(before, after)), to: top(Array.difference(after, before)) }), ({ from, to }) => Order.isLessThan(lock.order)(to, from)),
            (move) => ({ name, ...move }),
        );
    });
    const confirmed = yield* Effect.filter(Array.getSomes(moves), lock.confirm, { concurrency: 'unbounded' });
    return yield* Array.match(confirmed, { onEmpty: () => Effect.void, onNonEmpty: (found) => Effect.fail(Downgrade.make({ file, moves: found })) });
});

// --- [TABLES] --------------------------------------------------------------------------

const SEMVER = { valid: isValid, order: compare, confirm: () => Effect.succeed(true) };
const LOCKS = {
    'Cargo.lock': { rows: toml, ...SEMVER },
    'uv.lock': { rows: toml, ...SEMVER, valid: (version) => valid(version) !== null, order: (self, that) => Order.Number(comparePep440(self, that), 0) },
    'pnpm-lock.yaml': {
        rows: (text: string) => Effect.map(Schema.decodeEffect(PnpmLock)(text), ({ packages }) => Object.keys(packages).map((key) => Tuple.make(key.slice(0, key.lastIndexOf('@')), key.slice(key.lastIndexOf('@') + 1)))),
        ...SEMVER,
        confirm: (move) => (labelled(move) ? Effect.map(registry(move.name), (times) => Option.match(Option.all([Record.get(times, move.to), Record.get(times, move.from)]), { onNone: () => true, onSome: ([to, from]) => to < from })) : Effect.succeed(true)),
    },
    'Package.resolved': { rows: (text: string) => Effect.map(Schema.decodeEffect(Resolved)(text), ({ pins }) => pinned(pins.map(({ identity, state }) => ({ name: identity, ...state })))), ...SEMVER },
} as const satisfies Record.ReadonlyRecord<string, Lock>;

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const path = yield* Path.Path;
    const tracked = Array.groupBy(yield* records('\0', 'git', ['ls-files', '--cached', '-z', '--', ...Object.keys(LOCKS).map((name) => `:(glob)**/${name}`)]), (file) => path.basename(file));
    return yield* accumulate(Record.collect(LOCKS, (name, lock) => (tracked[name] ?? []).map((file) => Tuple.make(file, lock))).flat(), ([file, lock]) => downgrades(file, lock), { concurrency: 'unbounded' });
}).pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);

import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { compare as comparePep440, valid } from '@renovatebot/pep440';
import { Array, Cause, Effect, FileSystem, flow, Option, Order, Path, Record, Result, Schema, Struct, Tuple } from 'effect';
import { compare, compareMain, getPrerelease, isValid } from 'verkit';
import { accumulate } from '#/eng/typescript/accumulate';
import { Toml, Yaml } from '#/eng/typescript/parsed';
import { records, run } from '#/eng/typescript/process';

// --- [TYPES] ---------------------------------------------------------------------------

interface Lock {
    readonly rows: (text: string) => Effect.Effect<readonly (typeof Row.Type)[], Schema.SchemaError>;
    readonly valid: (version: string) => boolean;
    readonly order: Order.Order<string>;
    readonly confirm: typeof published;
}

// --- [MODELS] --------------------------------------------------------------------------

const Row = Schema.Struct({ name: Schema.String, version: Schema.optionalKey(Schema.String) });
const TomlLock = Toml.pipe(Schema.decodeTo(Schema.Struct({ package: Schema.Array(Row) })));
const PnpmLock = Yaml.pipe(Schema.decodeTo(Schema.Struct({ packages: Schema.Record(Schema.String, Schema.Unknown) })));
const Resolved = Schema.fromJsonString(Schema.Struct({ pins: Schema.Array(Schema.Struct({ identity: Schema.String, state: Schema.Struct({ version: Schema.optionalKey(Schema.String) }) })) }));
const Published = Schema.fromJsonString(Schema.Record(Schema.String, Schema.String));

// --- [OPERATIONS] ----------------------------------------------------------------------

const toml = flow(Schema.decodeEffect(TomlLock), Effect.map(Struct.get('package')));
const published = ({ name, from, to }: { readonly name: string; readonly from: string; readonly to: string }) =>
    compareMain(from, to) === 0 &&
    Option.exists(
        Array.findFirst(Array.zip(getPrerelease(from) ?? [], getPrerelease(to) ?? []), ([left, right]) => left !== right),
        ([left, right]) => typeof left === 'string' || typeof right === 'string',
    )
        ? run('pnpm', ['view', name, 'time', '--json']).pipe(
              Effect.flatMap(Schema.decodeEffect(Published)),
              Effect.map((times) => Option.match(Option.all([Record.get(times, to), Record.get(times, from)]), { onNone: () => true, onSome: ([later, earlier]) => later < earlier })),
          )
        : Effect.succeed(true);
const downgrades = Effect.fn('downgrades')(function* (file: string, lock: Lock) {
    const fs = yield* FileSystem.FileSystem;
    const versions = (text: string) =>
        lock.rows(text).pipe(
            Effect.mapBoth({
                onFailure: (cause) => new Cause.UnknownError(cause, `${file} did not parse`),
                onSuccess: flow(
                    Array.filterMap(({ name, version }) => (version !== undefined && lock.valid(version) ? Result.succeed(Tuple.make(name, version)) : Result.failVoid)),
                    Array.groupBy(Tuple.get(0)),
                    Record.map(Array.map(Tuple.get(1))),
                ),
            }),
        );
    const [started, current] = yield* Effect.all([Effect.flatMap(run('git', ['show', `:${file}`]), versions), Effect.flatMap(fs.readFileString(file), versions)], { concurrency: 'unbounded' });
    const top = flow(Option.liftPredicate(Array.isReadonlyArrayNonEmpty<string>), Option.map(Array.max(lock.order)));
    const moves = Array.getSomes(
        Record.collect(started, (name, before) => {
            const after = current[name] ?? before;
            return Option.map(
                Option.filter(Option.all({ from: top(Array.difference(before, after)), to: top(Array.difference(after, before)) }), ({ from, to }) => Order.isLessThan(lock.order)(to, from)),
                (move) => ({ name, ...move }),
            );
        }),
    );
    return yield* Effect.filter(moves, lock.confirm, { concurrency: 'unbounded' }).pipe(Effect.filterOrFail(Array.isArrayEmpty, (found) => new Cause.UnknownError(found, found.map(({ name, from, to }) => `${file} downgraded ${name} ${from} -> ${to}`).join('\n'))));
});

// --- [TABLES] --------------------------------------------------------------------------

const SEMVER = { valid: isValid, order: compare, confirm: () => Effect.succeed(true) };
const LOCKS = {
    'Cargo.lock': { rows: toml, ...SEMVER },
    'uv.lock': { rows: toml, ...SEMVER, valid: (version) => valid(version) !== null, order: (self, that) => Order.Number(comparePep440(self, that), 0) },
    'pnpm-lock.yaml': {
        rows: (text: string) => Effect.map(Schema.decodeEffect(PnpmLock)(text), ({ packages }) => Object.keys(packages).map((key) => ({ name: key.slice(0, key.lastIndexOf('@')), version: key.slice(key.lastIndexOf('@') + 1) }))),
        ...SEMVER,
        confirm: published,
    },
    'Package.resolved': { rows: (text: string) => Effect.map(Schema.decodeEffect(Resolved)(text), ({ pins }) => pins.map(({ identity, state }) => ({ name: identity, ...state }))), ...SEMVER },
} as const satisfies Record.ReadonlyRecord<string, Lock>;

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const path = yield* Path.Path;
    const tracked = Array.groupBy(yield* records('\0', 'git', ['ls-files', '--cached', '-z', '--', ...Object.keys(LOCKS).map((name) => `:(glob)**/${name}`)]), (file) => path.basename(file));
    return yield* accumulate(Record.collect(LOCKS, (name, lock) => (tracked[name] ?? []).map((file) => Tuple.make(file, lock))).flat(), ([file, lock]) => downgrades(file, lock), { concurrency: 'unbounded' });
}).pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);

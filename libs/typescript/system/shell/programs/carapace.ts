import { run } from '@rasm/process';
import { Array, Effect, Order, Record, Schema, SchemaGetter, SchemaIssue } from 'effect';
import { Flag, overlay, Usage } from '#/programs/usage';
import { ShellError } from '#/syntax/script';

// --- [TYPES] ---------------------------------------------------------------------------

interface Export {
    readonly Name: string;
    readonly Aliases?: readonly string[];
    readonly LocalFlags?: readonly (typeof Pflag.Encoded)[];
    readonly PersistentFlags?: readonly (typeof Pflag.Encoded)[];
    readonly Commands?: readonly Export[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const switches: ReadonlySet<string> = new Set(['bool', 'boolfunc', 'boolSlice', 'count']);

// --- [MODELS] --------------------------------------------------------------------------

const Pflag = Schema.Struct({ Longhand: Schema.optionalKey(Schema.String), Shorthand: Schema.optionalKey(Schema.String), Type: Schema.String, NoOptDefVal: Schema.optionalKey(Schema.String) }).pipe(
    Schema.decodeTo(Schema.toType(Flag), {
        decode: SchemaGetter.transformEffect((declared) => {
            const valued = declared.NoOptDefVal === undefined ? ('one' as const) : ('optional' as const);
            return Array.match([...Array.fromNullishOr(declared.Longhand).map((name) => `--${name}`), ...Array.fromNullishOr(declared.Shorthand).map((name) => `-${name}`)], {
                onEmpty: () => Effect.fail(new SchemaIssue.InvalidValue({ message: 'a carapace flag names a long or short spelling' }, declared)),
                onNonEmpty: (spellings) => Effect.succeed({ spellings, arity: switches.has(declared.Type) ? ('none' as const) : valued }),
            });
        }),
        encode: SchemaGetter.forbidden(() => 'carapace exports are read, never written'),
    }),
);
const Command: Schema.Codec<Usage, Export> = Schema.Struct({
    Name: Schema.String,
    Aliases: Schema.String.pipe(Schema.Array, Schema.optionalKey),
    LocalFlags: Pflag.pipe(Schema.Array, Schema.optionalKey),
    PersistentFlags: Pflag.pipe(Schema.Array, Schema.optionalKey),
    Commands: Schema.suspend(() => Command).pipe(Schema.Array, Schema.optionalKey),
}).pipe(
    Schema.decodeTo(Schema.toType(Usage), {
        decode: SchemaGetter.transform((command) => {
            const inherited = overlay([], command.PersistentFlags ?? [], true);
            const persistent = new Set(inherited.flatMap(({ spellings }) => spellings));
            const flags = overlay([], command.LocalFlags ?? [], true).filter(({ spellings }) => persistent.isDisjointFrom(new Set(spellings)));
            return Usage.make({ ...Usage.blank, name: command.Name, aliases: command.Aliases ?? [], flags, inherited, commands: command.Commands ?? [] });
        }),
        encode: SchemaGetter.forbidden(() => 'carapace exports are read, never written'),
    }),
);
const Listing = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Array(Schema.Struct({ group: Schema.String }))));
const Export = Schema.fromJsonString(Command);
const Row = Schema.fromJsonString(Usage);
const Chunk = Schema.Struct({ carapace: Schema.String, groups: Schema.Array(Schema.String), programs: Schema.Record(Schema.String, Row) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const carapace = (...args: readonly string[]) => Effect.map(run(['carapace', ...args]), ({ stdout }) => stdout);
const release = Effect.all([carapace('--version'), Effect.flatMap(carapace('--list'), Schema.decodeEffect(Listing))]).pipe(
    Effect.map(([version, completers]) => ({ carapace: version.trim(), groups: Array.sort(Array.dedupe(Record.values(completers).flatMap(Array.map(({ group }) => group))), Order.String), completers })),
    Effect.asSome,
    Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeedNone),
    Effect.mapError(ShellError.of({ _tag: 'Host' }, 'carapace --version --list')),
);
const describe = (program: string) =>
    carapace(program, 'export').pipe(
        Effect.flatMap((printed) => (printed.trim() === '' ? Effect.succeedNone : Effect.asSome(Schema.decodeEffect(Export)(printed)))),
        Effect.mapError(ShellError.of({ _tag: 'Host' }, `carapace ${program} export`)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { Chunk, describe, release, Row };

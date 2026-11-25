import { Array, Effect, Filter, Function, Option, Predicate, Record, Schema, SchemaGetter, SchemaIssue, Trie } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface UsageEncoded {
    readonly name: string;
    readonly aliases?: readonly string[];
    readonly flags?: readonly (typeof Tupled.Encoded)[];
    readonly inherited?: readonly (typeof Tupled.Encoded)[];
    readonly values?: Readonly<Record<string, typeof Slot.Encoded>>;
    readonly operands?: readonly (typeof Slot.Encoded)[];
    readonly input?: typeof Slot.Encoded;
    readonly output?: typeof Slot.Encoded;
    readonly abbreviates?: boolean;
    readonly interspersed?: boolean;
    readonly commands?: readonly UsageEncoded[];
}
interface CarapaceCommand {
    readonly Name: string;
    readonly Aliases?: readonly string[];
    readonly LocalFlags?: readonly (typeof Flag.Type)[];
    readonly PersistentFlags?: readonly (typeof Flag.Type)[];
    readonly Commands?: readonly CarapaceCommand[];
}
interface CarapaceExport {
    readonly Name: string;
    readonly Aliases?: readonly string[];
    readonly LocalFlags?: readonly (typeof CarapaceFlag.Encoded)[];
    readonly PersistentFlags?: readonly (typeof CarapaceFlag.Encoded)[];
    readonly Commands?: readonly CarapaceExport[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const carapaceTypes = { bool: 'none', boolSlice: 'none', count: 'none', duration: 'one', float64: 'one', int: 'one', int64: 'one', uint: 'one', uint64: 'one', string: 'one', intSlice: 'one', stringArray: 'one', stringSlice: 'one', stringToString: 'one' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const omitted = <S extends Schema.Top>(schema: S, empty: S['Encoded'], kept: Predicate.Predicate<S['Encoded']>) => Schema.optionalKey(Schema.toEncoded(schema)).pipe(Schema.decodeTo(schema, { decode: SchemaGetter.withDefault(Effect.succeed(empty)), encode: SchemaGetter.transformOptional(Option.filter(kept)) }));
const listed = <S extends Schema.Top>(item: S) => omitted(Schema.Array(item), [], Array.isReadonlyArrayNonEmpty);
const Role = Schema.Literals(['read', 'write', 'create', 'delete', 'execute', 'directory', 'host', 'user', 'container', 'assignment', 'unset', 'command', 'shell', 'script', 'code', 'placeholder']);
const Arity = Schema.Literals(['none', 'one', 'optional', 'rest']);
const Slot = Schema.Struct({
    roles: listed(Role),
    count: omitted(Schema.Literals(['one', 'optional', 'rest']), 'one', (count) => count !== 'one'),
    tree: omitted(Schema.Boolean, false, Function.identity),
    ends: listed(Schema.String),
    when: listed(Schema.String),
    unless: listed(Schema.String),
});
const Flag = Schema.Struct({ spellings: Schema.NonEmptyArray(Schema.String), arity: Arity });
const Tupled = Schema.TupleWithRest(Schema.Tuple([Arity, Schema.String]), [Schema.String]).pipe(
    Schema.decodeTo(Flag, {
        decode: SchemaGetter.transform(([arity, first, ...rest]) => ({ spellings: Array.make(first, ...rest), arity })),
        encode: SchemaGetter.transform(({ spellings, arity }) => [arity, ...spellings] as const),
    }),
);
const unified = (base: readonly (typeof Flag.Type)[], over: readonly (typeof Flag.Type)[]): readonly (typeof Flag.Type)[] =>
    over.reduce<readonly (typeof Flag.Type)[]>((flags, flag) => {
        const [joined, apart] = Array.partition(
            flags,
            Filter.fromPredicate((held: typeof Flag.Type) => !new Set(held.spellings).isDisjointFrom(new Set(flag.spellings))),
        );
        return [
            ...apart,
            {
                ...flag,
                spellings: Array.dedupe(
                    Array.appendAll(
                        flag.spellings,
                        joined.flatMap(({ spellings }) => spellings),
                    ),
                ),
            },
        ];
    }, base);

class Usage extends Schema.Class<Usage, { readonly brand: unique symbol }>('@rasm/shell/Usage')(
    Schema.Struct({
        name: Schema.String,
        aliases: listed(Schema.String),
        flags: listed(Tupled),
        inherited: listed(Tupled),
        values: omitted(Schema.Record(Schema.String, Slot), {}, Predicate.not(Record.isEmptyRecord)),
        operands: Schema.OptionFromOptionalKey(Schema.Array(Slot)),
        input: Schema.OptionFromOptionalKey(Slot),
        output: Schema.OptionFromOptionalKey(Slot),
        abbreviates: Schema.OptionFromOptionalKey(Schema.Boolean),
        interspersed: Schema.OptionFromOptionalKey(Schema.Boolean),
        commands: listed(Schema.suspend((): Schema.Codec<Usage, UsageEncoded> => Usage)),
    }).check(
        Schema.makeFilter(({ flags, inherited }) => {
            const spellings = [...flags, ...inherited].flatMap((flag) => flag.spellings);
            return new Set(spellings).size === spellings.length || 'each spelling belongs to one flag';
        }),
    ),
) {
    flag(spelling: string): Option.Option<typeof Flag.Type> {
        return Trie.get(indexed(this).spelled, spelling);
    }

    abbreviated(prefix: string): readonly (typeof Flag.Type)[] {
        return Option.getOrElse(this.abbreviates, () => false) && prefix.startsWith('--') ? Array.dedupe(Trie.valuesWithPrefix(indexed(this).spelled, prefix)) : [];
    }

    command(word: string): Option.Option<Usage> {
        const { commands, descended } = indexed(this);
        return Option.map(Option.fromUndefinedOr(commands.get(word)), descended);
    }

    value(flag: typeof Flag.Type): Option.Option<typeof Slot.Type> {
        return Array.findFirst(flag.spellings, (spelling) => Record.get(this.values, spelling));
    }

    merge(over: Usage): Usage {
        const spelled = new Set(this.inherited.flatMap(({ spellings }) => spellings));
        const [persistent, local] = Array.partition(
            over.flags,
            Filter.fromPredicate(({ spellings }: typeof Flag.Type) => !spelled.isDisjointFrom(new Set(spellings))),
        );
        const commands = new Map(over.commands.map((command) => [command.name, command]));
        const held = new Set(this.commands.map(({ name }) => name));
        return Usage.make({
            name: this.name,
            aliases: Array.dedupe([...this.aliases, ...over.aliases]),
            flags: unified(this.flags, local),
            inherited: unified(this.inherited, [...persistent, ...over.inherited]),
            values: { ...this.values, ...over.values },
            operands: Option.orElse(over.operands, () => this.operands),
            input: Option.orElse(over.input, () => this.input),
            output: Option.orElse(over.output, () => this.output),
            abbreviates: Option.orElse(over.abbreviates, () => this.abbreviates),
            interspersed: Option.orElse(over.interspersed, () => this.interspersed),
            commands: [...this.commands.map((command) => Option.match(Option.fromUndefinedOr(commands.get(command.name)), { onNone: () => command, onSome: (row) => command.merge(row) })), ...over.commands.filter(({ name }) => !held.has(name))],
        });
    }
}

const indexed = Function.memoize((usage: Usage) => ({
    spelled: Trie.fromIterable([...usage.flags, ...usage.inherited].flatMap((flag) => flag.spellings.map((spelling) => [spelling, flag] as const))),
    commands: new Map([...usage.commands.flatMap((command) => command.aliases.map((alias) => [alias, command] as const)), ...usage.commands.map((command) => [command.name, command] as const)]),
    descended: Function.memoize((child: Usage) => {
        const own = new Set(child.flags.flatMap(({ spellings }) => spellings));
        return Usage.make({
            ...child,
            inherited: unified(
                usage.inherited.filter(({ spellings }) => own.isDisjointFrom(new Set(spellings))),
                child.inherited,
            ),
            abbreviates: Option.orElse(child.abbreviates, () => usage.abbreviates),
            interspersed: Option.orElse(child.interspersed, () => usage.interspersed),
        });
    }),
}));

const CarapaceFlag = Schema.Struct({ Longhand: Schema.optionalKey(Schema.String), Shorthand: Schema.optionalKey(Schema.String), Type: Schema.Literals(Record.keys(carapaceTypes)), NoOptDefVal: Schema.optionalKey(Schema.String) }).pipe(
    Schema.decodeTo(Flag, {
        decode: SchemaGetter.transformEffect((declared) =>
            Array.match([...Option.toArray(Option.map(Option.fromUndefinedOr(declared.Longhand), (name) => `--${name}`)), ...Option.toArray(Option.map(Option.fromUndefinedOr(declared.Shorthand), (name) => `-${name}`))], {
                onEmpty: () => Effect.fail(new SchemaIssue.InvalidValue({ message: 'a carapace flag names a long or short spelling' }, declared)),
                onNonEmpty: (spellings) => Effect.succeed({ spellings, arity: carapaceTypes[declared.Type] === 'one' && declared.NoOptDefVal !== undefined ? 'optional' : carapaceTypes[declared.Type] }),
            }),
        ),
        encode: SchemaGetter.forbidden(() => 'carapace exports are read, never written'),
    }),
);
const CarapaceCommand: Schema.Codec<CarapaceCommand, CarapaceExport> = Schema.Struct({
    Name: Schema.String,
    Aliases: Schema.optionalKey(Schema.Array(Schema.String)),
    LocalFlags: Schema.optionalKey(Schema.Array(CarapaceFlag)),
    PersistentFlags: Schema.optionalKey(Schema.Array(CarapaceFlag)),
    Commands: Schema.optionalKey(Schema.Array(Schema.suspend(() => CarapaceCommand))),
});
const carapaced = (command: CarapaceCommand): Usage => {
    const inherited = unified([], command.PersistentFlags ?? []);
    const spelled = new Set(inherited.flatMap(({ spellings }) => spellings));
    return Usage.make({
        name: command.Name,
        aliases: command.Aliases ?? [],
        flags: unified([], command.LocalFlags ?? []).filter(({ spellings }) => spelled.isDisjointFrom(new Set(spellings))),
        inherited,
        values: {},
        operands: Option.none(),
        input: Option.none(),
        output: Option.none(),
        abbreviates: Option.none(),
        interspersed: Option.none(),
        commands: (command.Commands ?? []).map(carapaced),
    });
};
const Carapace = Schema.fromJsonString(CarapaceCommand).pipe(Schema.decodeTo(Schema.toType(Usage), { decode: SchemaGetter.transform(carapaced), encode: SchemaGetter.forbidden(() => 'carapace exports are read, never written') }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Carapace, Usage, type UsageEncoded };

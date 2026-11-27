import { Array, Effect, Equal, Filter, Function, HashMap, Option, Predicate, Record, Schema, SchemaGetter, Trie } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Encoded {
    readonly name: string;
    readonly aliases?: readonly string[];
    readonly flags?: readonly (typeof Flag.Encoded)[];
    readonly inherited?: readonly (typeof Flag.Encoded)[];
    readonly values?: Readonly<Record<string, typeof Slot.Encoded>>;
    readonly operands?: readonly (typeof Slot.Encoded)[];
    readonly input?: typeof Slot.Encoded;
    readonly output?: typeof Slot.Encoded;
    readonly abbreviates?: boolean;
    readonly interspersed?: boolean;
    readonly commands?: readonly Encoded[];
}

// --- [MODELS] --------------------------------------------------------------------------

const Default = <S extends Schema.Top>(schema: S, value: S['Encoded']) => schema.pipe(Schema.toEncoded, Schema.optionalKey, Schema.decodeTo(schema, { decode: SchemaGetter.withDefault(Effect.succeed(value)), encode: SchemaGetter.transformOptional(Option.filter((held) => !Equal.equals(held, value))) }));
const List = <S extends Schema.Top>(item: S) => Default(Schema.Array(item), []);
const Arity = Schema.Literals(['none', 'one', 'optional', 'rest']);
const Slot = Schema.Struct({
    roles: List(Schema.Literals(['read', 'write', 'create', 'delete', 'execute', 'copy', 'directory', 'host', 'user', 'container', 'assignment', 'variable', 'unset', 'command', 'shell', 'script', 'code', 'placeholder'])),
    count: Default(Arity.pick(['one', 'optional', 'rest']), 'one'),
    tree: Default(Schema.Boolean, false),
    ends: List(Schema.String),
    when: List(Schema.String),
    unless: List(Schema.String),
});
const Flag = Schema.TupleWithRest(Schema.Tuple([Arity, Schema.String]), [Schema.String]).pipe(
    Schema.decodeTo(Schema.Struct({ spellings: Schema.NonEmptyArray(Schema.String), arity: Arity }), {
        decode: SchemaGetter.transform(([arity, first, ...rest]) => ({ spellings: Array.make(first, ...rest), arity })),
        encode: SchemaGetter.transform(({ spellings, arity }) => [arity, ...spellings] as const),
    }),
);

const overlay = (base: readonly (typeof Flag.Type)[], over: readonly (typeof Flag.Type)[], absorbs: boolean) =>
    over.reduce((flags, flag) => {
        const [cut, whole] = Array.partition(
            flags.map((held) => ({ held, apart: held.spellings.filter((spelling) => !flag.spellings.includes(spelling)) })),
            Filter.fromPredicate(({ held, apart }) => apart.length < held.spellings.length),
        );
        const kept = cut.flatMap(({ held, apart }) => (Array.isArrayNonEmpty(apart) ? [{ ...held, spellings: apart }] : []));
        return [
            ...whole.map(({ held }) => held),
            ...(absorbs
                ? [
                      {
                          ...flag,
                          spellings: Array.appendAll(
                              flag.spellings,
                              kept.flatMap(({ spellings }) => spellings),
                          ),
                      },
                  ]
                : [...kept, flag]),
        ];
    }, base);

class Usage extends Schema.Class<Usage, { readonly brand: unique symbol }>('@rasm/shell/Usage')(
    Schema.Struct({
        name: Schema.String,
        aliases: List(Schema.String),
        flags: List(Flag),
        inherited: List(Flag),
        values: Default(Schema.Record(Schema.String, Slot), {}),
        operands: Slot.pipe(Schema.Array, Schema.OptionFromOptionalKey),
        input: Schema.OptionFromOptionalKey(Slot),
        output: Schema.OptionFromOptionalKey(Slot),
        abbreviates: Schema.OptionFromOptionalKey(Schema.Boolean),
        interspersed: Schema.OptionFromOptionalKey(Schema.Boolean),
        commands: List(Schema.suspend((): Schema.Codec<Usage, Encoded> => Usage)),
    }).check(
        Schema.makeFilter(({ flags, inherited }) => {
            const spellings = [...flags, ...inherited].flatMap((flag) => flag.spellings);
            return new Set(spellings).size === spellings.length || 'each spelling belongs to one flag';
        }),
    ),
) {
    // --- [READING]

    flag(spelling: string): Option.Option<Usage['flags'][number]> {
        const { spellings } = index(this);
        const abbreviates = Option.getOrElse(this.abbreviates, () => false) && spelling.startsWith('--');
        return Option.orElse(Trie.get(spellings, spelling), () => Option.map(Option.liftPredicate(abbreviates ? Array.dedupe(Trie.valuesWithPrefix(spellings, spelling)) : [], Predicate.isTupleOf(1)), Array.headNonEmpty));
    }

    command(word: string): Option.Option<Usage> {
        const { commands, descend } = index(this);
        return Option.map(HashMap.get(commands, word), descend);
    }

    value(flag: Usage['flags'][number]): Option.Option<typeof Slot.Type> {
        return Array.findFirst(flag.spellings, (spelling) => Record.get(this.values, spelling));
    }

    // --- [LAYERING]

    merge(over: Usage): Usage {
        const spelled = new Set(this.inherited.flatMap(({ spellings }) => spellings));
        const [persistent, local] = Array.partition(
            over.flags,
            Filter.fromPredicate(({ spellings }: Usage['flags'][number]) => !spelled.isDisjointFrom(new Set(spellings))),
        );
        return Usage.make({
            name: this.name,
            aliases: Array.dedupe([...this.aliases, ...over.aliases]),
            flags: overlay(this.flags, local, false),
            inherited: overlay(this.inherited, [...persistent, ...over.inherited], false),
            values: { ...this.values, ...over.values },
            operands: Option.orElse(over.operands, () => this.operands),
            input: Option.orElse(over.input, () => this.input),
            output: Option.orElse(over.output, () => this.output),
            abbreviates: Option.orElse(over.abbreviates, () => this.abbreviates),
            interspersed: Option.orElse(over.interspersed, () => this.interspersed),
            commands: Record.collect(
                Array.groupBy([...this.commands, ...over.commands], ({ name }) => name),
                (_, rows) => rows.reduce((base, row) => base.merge(row)),
            ),
        });
    }

    static readonly blank = Usage.make({ name: '', aliases: [], flags: [], inherited: [], values: {}, operands: Option.none(), input: Option.none(), output: Option.none(), abbreviates: Option.none(), interspersed: Option.none(), commands: [] });
}

const index = Function.memoize((usage: Usage) => ({
    spellings: Trie.fromIterable([...usage.flags, ...usage.inherited].flatMap((flag) => flag.spellings.map((spelling) => [spelling, flag] as const))),
    commands: HashMap.fromIterable(usage.commands.flatMap((command) => [...command.aliases, command.name].map((name) => [name, command] as const))),
    descend: Function.memoize((child: Usage) => {
        const own = new Set(child.flags.flatMap(({ spellings }) => spellings));
        return Usage.make({
            ...child,
            inherited: overlay(
                usage.inherited.filter(({ spellings }) => own.isDisjointFrom(new Set(spellings))),
                child.inherited,
                true,
            ),
            abbreviates: Option.orElse(child.abbreviates, () => usage.abbreviates),
            interspersed: Option.orElse(child.interspersed, () => usage.interspersed),
        });
    }),
}));

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Encoded, Flag, overlay, Usage };

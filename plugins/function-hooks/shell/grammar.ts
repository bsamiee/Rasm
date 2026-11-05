import { Array, Effect, Option, Result, Schema, String } from 'effect';
import { HookError, Host } from '#/host';
import type { Program } from '#/shell/programs';
import type { Word } from '#/shell/script';

// --- [TYPES] ---------------------------------------------------------------------------

interface Spec {
    readonly name: string;
    readonly arity: number;
}
interface Grammar {
    readonly options: ReadonlyMap<string, Spec>;
    readonly commands: ReadonlyMap<string, Grammar>;
}
interface Item {
    readonly names: readonly string[];
    readonly words: readonly [Word, ...Word[]];
    readonly values: readonly string[];
}
interface Parsed {
    readonly items: readonly Item[];
    readonly path: readonly string[];
    readonly tail: readonly Word[];
    readonly rest: readonly Word[];
    readonly separator: Option.Option<Word>;
}
interface Exported {
    readonly Name: string;
    readonly Aliases?: readonly string[];
    readonly Commands?: readonly Exported[];
    readonly LocalFlags?: readonly (typeof ExportedFlag.Type)[];
    readonly PersistentFlags?: readonly (typeof ExportedFlag.Type)[];
}
interface Dumped {
    readonly commandName: string;
    readonly arguments?: readonly (typeof DumpedArgument.Type)[];
    readonly subcommands?: readonly Dumped[];
}
interface Cursor {
    readonly at: number;
    readonly grammar: Grammar;
    readonly pending: readonly (readonly string[])[];
    readonly operated: boolean;
    readonly ended: boolean;
    readonly separated: boolean;
    readonly subcommand: string;
}
type Step = readonly [Partial<Parsed>, Cursor];
interface Reading {
    readonly specs: readonly Spec[];
    readonly attached: Option.Option<string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const OPTION = /^-./u;
const CAMEL = /[A-Z]/gu;
const NOTATION = /^(?:(?<short>-[^,]+), )?(?<long>-[^=]+)(?:=(?<arity>\d?))?$/u;
const FLAG_TYPES: ReadonlySet<string> = new Set(['bool', 'boolSlice', 'count']);
const NO_GRAMMAR: Grammar = { options: new Map(), commands: new Map() };

// --- [MODELS] --------------------------------------------------------------------------

const ExportedFlag = Schema.Struct({ Longhand: Schema.optionalKey(Schema.String), Shorthand: Schema.optionalKey(Schema.String), Type: Schema.String, NoOptDefVal: Schema.optionalKey(Schema.String) });
const ExportedCommand: Schema.Codec<Exported> = Schema.Struct({
    Name: Schema.String,
    Aliases: Schema.optionalKey(Schema.Array(Schema.String)),
    Commands: Schema.optionalKey(Schema.Array(Schema.suspend((): Schema.Codec<Exported> => ExportedCommand))),
    LocalFlags: Schema.optionalKey(Schema.Array(ExportedFlag)),
    PersistentFlags: Schema.optionalKey(Schema.Array(ExportedFlag)),
});
const Export = Schema.fromJsonString(ExportedCommand);

const DumpedArgument = Schema.Struct({ kind: Schema.String, names: Schema.optionalKey(Schema.Array(Schema.Struct({ kind: Schema.String, name: Schema.String }))) });
const DumpedCommand: Schema.Codec<Dumped> = Schema.Struct({
    commandName: Schema.String,
    arguments: Schema.optionalKey(Schema.Array(DumpedArgument)),
    subcommands: Schema.optionalKey(Schema.Array(Schema.suspend((): Schema.Codec<Dumped> => DumpedCommand))),
});
const Dump = Schema.fromJsonString(Schema.Struct({ command: DumpedCommand }));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SOURCES]

const spellings = (spelled: readonly string[], arity: number): readonly (readonly [string, Spec])[] => spelled.map((spelling) => [spelling, { name: spelled.find((other) => other.startsWith('--')) ?? spelling, arity }]);

const flagEntries = ({ Longhand, Shorthand, Type, NoOptDefVal }: typeof ExportedFlag.Type) => spellings([...(Longhand === undefined ? [] : [`--${Longhand}`]), ...(Shorthand === undefined ? [] : [`-${Shorthand}`])], FLAG_TYPES.has(Type) || NoOptDefVal !== undefined ? 0 : 1);

const notated = (flag: string) => {
    const { short, long = flag, arity } = NOTATION.exec(flag)?.groups ?? {};
    return spellings([long, ...(short === undefined ? [] : [short])], arity === undefined ? 0 : Number(arity || 1));
};

const exportedGrammar = (node: Exported, inherited: ReadonlyMap<string, Spec>, overlay: ReadonlyMap<string, Spec>): Grammar => {
    const persistent = new Map([...inherited, ...(node.PersistentFlags ?? []).flatMap(flagEntries)]);
    return {
        options: new Map([...persistent, ...(node.LocalFlags ?? []).flatMap(flagEntries), ...overlay]),
        commands: new Map((node.Commands ?? []).flatMap((command) => [command.Name, ...(command.Aliases ?? [])].map((name) => [name, exportedGrammar(command, persistent, overlay)] as const))),
    };
};

const dumpedGrammar = (node: Dumped): Grammar => ({
    options: new Map(
        (node.arguments ?? [])
            .filter(({ kind }) => kind !== 'positional')
            .flatMap(({ kind, names = [] }) =>
                spellings(
                    names.map(({ kind: style, name }) => (style === 'long' ? `--${name}` : `-${name}`)),
                    kind === 'option' ? 1 : 0,
                ),
            ),
    ),
    commands: new Map((node.subcommands ?? []).map((command) => [command.commandName, dumpedGrammar(command)])),
});

const grammarOf = Effect.fnUntraced(function* (program: string, row: Program, primary: string) {
    const host = yield* Host;
    if (row.dump === true) {
        const { stdout } = yield* host.run([program, '--experimental-dump-help']);
        return dumpedGrammar((yield* Effect.mapError(Schema.decodeEffect(Dump)(stdout), HookError.of(program))).command);
    }
    const exported = (name: string) => host.run(['carapace', name, 'export'], { env: { CARAPACE_BUILTINS: 'bash', CARAPACE_SHELL: 'bash', CARAPACE_BRIDGES: '', XDG_CONFIG_HOME: host.path.join(host.root, '.config') } });
    const own = yield* exported(program);
    const { stdout } = String.isEmpty(String.trim(own.stdout)) ? yield* exported(primary) : own;
    const overlay = new Map([...(row.flags ?? []), ...[...(row.walks?.names ?? []), ...(row.code ?? [])].map((name) => `${name}=`)].flatMap(notated));
    return String.isEmpty(String.trim(stdout)) ? { ...NO_GRAMMAR, options: overlay } : exportedGrammar(yield* Effect.mapError(Schema.decodeEffect(Export)(stdout), HookError.of('carapace')), new Map(), overlay);
});

// --- [PARSE]

const optionOf = (text: string, grammar: Grammar, row: Program): Option.Option<Reading> => {
    const cut = text.indexOf('=');
    const given = cut < 0 ? text : text.slice(0, cut);
    const head = row.camelCase === true ? given.replaceAll(CAMEL, (letter) => `-${letter.toLowerCase()}`) : given;
    const attached = cut < 0 ? Option.none() : Option.some(text.slice(cut + 1));
    const exact = grammar.options.get(head);
    if (exact !== undefined) {
        return Option.some({ specs: [exact], attached });
    }
    if (!OPTION.test(text) || (row.closed === true && !given.startsWith('--') && [...given.slice(1)].some((letter) => !grammar.options.has(`-${letter}`)))) {
        return Option.none();
    }
    if (given.startsWith('--') || ![...grammar.options.keys()].every((spelling) => spelling.startsWith('--') || spelling.length === 2)) {
        return row.closed === true ? Option.none() : Option.some({ specs: [{ name: head, arity: 0 }], attached });
    }
    const letters = [...text.slice(1)];
    const specs = letters.map((letter): Spec => grammar.options.get(`-${letter}`) ?? { name: `-${letter}`, arity: 0 });
    const taking = specs.findIndex(({ arity }) => arity > 0);
    return Option.some(taking < 0 ? { specs, attached: Option.none() } : { specs: specs.slice(0, taking + 1), attached: Option.liftPredicate(letters.slice(taking + 1).join(''), String.isNonEmpty) });
};

const flagged = (word: Word, { specs, attached }: Reading, cursor: Cursor, words: readonly Word[], row: Program): Step => {
    const spec = specs.at(-1);
    const from = cursor.at + 1;
    const taken = row.execs?.includes(spec?.name ?? '') === true ? words.slice(from, words.findIndex(({ text }, index) => index >= from && (text === ';' || text === '+')) + 1 || words.length) : words.slice(from, from + (spec?.arity ?? 0));
    const supplied = Option.isSome(attached) ? [] : taken;
    const names = specs.map(({ name }) => name);
    return [{ items: [{ names, words: [word, ...supplied], values: [...Option.toArray(attached), ...supplied.map(({ text }) => text)] }] }, { ...cursor, at: cursor.at + 1 + supplied.length, ended: names.some((name) => name === row.module || row.ends?.includes(name) === true) }];
};

const operated = (word: Word, cursor: Cursor, words: readonly Word[], row: Program): Step => {
    const deeper = Array.filterMap(cursor.pending, ([step, ...steps]) => (step === word.text ? Result.succeed(steps) : Result.failVoid));
    const next = { ...cursor, at: cursor.at + 1 };
    if (deeper.length > 0) {
        return [{ path: [word.text] }, { ...next, pending: deeper, grammar: cursor.grammar.commands.get(word.text) ?? cursor.grammar }];
    }
    const descended = cursor.operated ? undefined : cursor.grammar.commands.get(word.text);
    if (descended !== undefined) {
        return [{ path: [word.text] }, { ...next, grammar: descended, subcommand: cursor.subcommand || word.text }];
    }
    if (cursor.pending.some((path) => path.length === 0) && (row.leading ?? 0) === 0) {
        return [{ rest: words.slice(cursor.at) }, { ...cursor, at: words.length }];
    }
    return [{ items: [{ names: [], words: [word], values: [] }] }, { ...next, operated: true, ended: row.stops === true || (row.stops?.includes(cursor.subcommand) ?? false) || cursor.pending.some((path) => path.length === 0) }];
};

const advanced = (word: Word, cursor: Cursor, words: readonly Word[], row: Program): Step => {
    const end = { ...cursor, at: words.length };
    if (cursor.ended) {
        return [{ rest: words.slice(cursor.at) }, end];
    }
    if (word.text === '--' && !cursor.separated) {
        if (cursor.pending.some((path) => path.length === 0)) {
            return [{}, { ...cursor, at: cursor.at + 1, separated: true }];
        }
        return [row.wraps === '--' ? { rest: words.slice(cursor.at + 1) } : { separator: Option.some(word), tail: words.slice(cursor.at + 1) }, end];
    }
    return Option.match(OPTION.test(word.text) && !cursor.separated ? optionOf(word.text, cursor.grammar, row) : Option.none(), {
        onNone: () => operated(word, cursor, words, row),
        onSome: (reading) => flagged(word, reading, cursor, words, row),
    });
};

const parsed = (words: readonly Word[], grammar: Grammar, row: Program): Parsed => {
    const start: Cursor = { at: 0, grammar, pending: row.wraps === '--' ? [] : (row.wraps ?? []), operated: false, ended: false, separated: false, subcommand: '' };
    const steps = Array.unfold(start, (cursor) => Option.map(Option.fromNullishOr(words[cursor.at]), (word) => advanced(word, cursor, words, row)));
    return {
        items: steps.flatMap(({ items = [] }) => items),
        path: steps.flatMap(({ path = [] }) => path),
        tail: steps.flatMap(({ tail = [] }) => tail),
        rest: steps.flatMap(({ rest = [] }) => rest),
        separator: Option.firstSomeOf(steps.map(({ separator = Option.none() }) => separator)),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Grammar, grammarOf, type Item, NO_GRAMMAR, type Parsed, parsed };

import { Array, Effect, Option, Result, Schema, String } from 'effect';
import { Host, HostError } from '#/host';
import type { Program } from '#/shell/programs';
import { slicedWord, type Word } from '#/shell/script';

// --- [TYPES] ---------------------------------------------------------------------------

interface Flag {
    readonly name: string;
    readonly arity: number;
}
interface Usage {
    readonly flags: ReadonlyMap<string, Flag>;
    readonly subcommands: ReadonlyMap<string, Usage>;
}
interface Argument {
    readonly names: readonly string[];
    readonly words: readonly [Word, ...Word[]];
    readonly values: readonly Word[];
}
interface Arguments {
    readonly args: readonly Argument[];
    readonly path: readonly string[];
    readonly tail: readonly Word[];
    readonly rest: readonly Word[];
    readonly separator: Option.Option<Word>;
    readonly names: ReadonlySet<string>;
    readonly operands: readonly Word[];
    readonly leading: readonly Word[];
    readonly inputs: readonly Word[];
    readonly has: (...names: readonly string[]) => boolean;
    readonly values: (...names: readonly string[]) => readonly string[];
    readonly supplied: (...names: readonly string[]) => readonly Word[];
}
interface CarapaceCommand {
    readonly Name: string;
    readonly Aliases?: readonly string[];
    readonly Commands?: readonly CarapaceCommand[];
    readonly LocalFlags?: readonly (typeof CarapaceFlag.Type)[];
    readonly PersistentFlags?: readonly (typeof CarapaceFlag.Type)[];
}
interface CommandInfo {
    readonly commandName: string;
    readonly arguments?: readonly (typeof ArgumentInfo.Type)[];
    readonly subcommands?: readonly CommandInfo[];
}
interface Scan {
    readonly at: number;
    readonly usage: Usage;
    readonly wrapped: readonly (readonly string[])[];
    readonly operand: boolean;
    readonly ended: boolean;
    readonly separated: boolean;
    readonly subcommand: Option.Option<string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const OPTION = /^-./u;
const CAMEL = /[A-Z]/gu;
const NOTATION = /^(?:(?<short>-[^,]+), )?(?<long>-[^=]+)(?:=(?<arity>\d?))?$/u;
const NO_USAGE: Usage = { flags: new Map(), subcommands: new Map() };

// --- [MODELS] --------------------------------------------------------------------------

const CarapaceFlag = Schema.Struct({ Longhand: Schema.optionalKey(Schema.String), Shorthand: Schema.optionalKey(Schema.String), Type: Schema.String, NoOptDefVal: Schema.optionalKey(Schema.String) });
const CarapaceCommand: Schema.Codec<CarapaceCommand> = Schema.Struct({
    Name: Schema.String,
    Aliases: Schema.optionalKey(Schema.Array(Schema.String)),
    Commands: Schema.optionalKey(Schema.Array(Schema.suspend((): Schema.Codec<CarapaceCommand> => CarapaceCommand))),
    LocalFlags: Schema.optionalKey(Schema.Array(CarapaceFlag)),
    PersistentFlags: Schema.optionalKey(Schema.Array(CarapaceFlag)),
});
const CarapaceExport = Schema.fromJsonString(CarapaceCommand);

const ArgumentInfo = Schema.Struct({ kind: Schema.String, names: Schema.optionalKey(Schema.Array(Schema.Struct({ kind: Schema.String, name: Schema.String }))) });
const CommandInfo: Schema.Codec<CommandInfo> = Schema.Struct({
    commandName: Schema.String,
    arguments: Schema.optionalKey(Schema.Array(ArgumentInfo)),
    subcommands: Schema.optionalKey(Schema.Array(Schema.suspend((): Schema.Codec<CommandInfo> => CommandInfo))),
});
const ToolInfo = Schema.fromJsonString(Schema.Struct({ command: CommandInfo }));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [USAGE]

const flagsOf = (spellings: readonly string[], arity: number): readonly (readonly [string, Flag])[] => spellings.map((spelling) => [spelling, { name: spellings.find((other) => other.startsWith('--')) ?? spelling, arity }]);

const carapaceFlags = ({ Longhand, Shorthand, Type, NoOptDefVal }: typeof CarapaceFlag.Type) => flagsOf([...(Longhand === undefined ? [] : [`--${Longhand}`]), ...(Shorthand === undefined ? [] : [`-${Shorthand}`])], ['bool', 'boolSlice', 'count'].includes(Type) || NoOptDefVal !== undefined ? 0 : 1);

const carapaceUsage = (command: CarapaceCommand, inherited: Usage['flags'], declared: Usage['flags']): Usage => {
    const persistent = new Map([...inherited, ...(command.PersistentFlags ?? []).flatMap(carapaceFlags)]);
    return {
        flags: new Map([...persistent, ...(command.LocalFlags ?? []).flatMap(carapaceFlags), ...declared]),
        subcommands: new Map((command.Commands ?? []).flatMap((sub) => [sub.Name, ...(sub.Aliases ?? [])].map((name) => [name, carapaceUsage(sub, persistent, declared)] as const))),
    };
};

const infoUsage = (command: CommandInfo): Usage => ({
    flags: new Map(
        (command.arguments ?? [])
            .filter(({ kind }) => kind !== 'positional')
            .flatMap(({ kind, names = [] }) =>
                flagsOf(
                    names.map(({ kind: style, name }) => (style === 'long' ? `--${name}` : `-${name}`)),
                    kind === 'option' ? 1 : 0,
                ),
            ),
    ),
    subcommands: new Map((command.subcommands ?? []).map((sub) => [sub.commandName, infoUsage(sub)])),
});

const usageOf = Effect.fnUntraced(function* (program: string, row: Program, primary: string) {
    const host = yield* Host;
    if (row.dumpsHelp === true) {
        if (!(yield* host.onPath(program))) {
            return NO_USAGE;
        }
        const { stdout } = yield* host.run([program, '--experimental-dump-help']);
        return infoUsage((yield* Effect.mapError(Schema.decodeEffect(ToolInfo)(stdout), HostError.of(program))).command);
    }
    const exported = (name: string) => host.run(['carapace', name, 'export'], { env: { CARAPACE_BUILTINS: 'bash', CARAPACE_SHELL: 'bash', CARAPACE_BRIDGES: '', XDG_CONFIG_HOME: host.path.join(host.root, '.config') } });
    const own = yield* exported(row.completer ?? program);
    const { stdout } = row.completer === undefined && program !== primary && String.isEmpty(String.trim(own.stdout)) ? yield* exported(primary) : own;
    const declared = new Map(
        [...(row.flags ?? []), ...[...(row.recursive?.names ?? []), ...(row.code ?? [])].map((name) => `${name}=`)].flatMap((flag) => {
            const { short, long = flag, arity } = NOTATION.exec(flag)?.groups ?? {};
            return flagsOf([long, ...(short === undefined ? [] : [short])], arity === undefined ? 0 : Number(arity || 1));
        }),
    );
    return String.isEmpty(String.trim(stdout)) ? { ...NO_USAGE, flags: declared } : carapaceUsage(yield* Effect.mapError(Schema.decodeEffect(CarapaceExport)(stdout), HostError.of('carapace')), new Map(), declared);
});

// --- [ARGUMENTS]

const optionOf = ({ text }: Word, usage: Usage, row: Program): Option.Option<readonly [flags: readonly Flag[], attached: Option.Option<number>]> => {
    const cut = text.indexOf('=');
    const given = cut < 0 ? text : text.slice(0, cut);
    const head = row.camelCase === true ? given.replaceAll(CAMEL, (letter) => `-${letter.toLowerCase()}`) : given;
    const attached = cut < 0 ? Option.none() : Option.some(cut + 1);
    const exact = usage.flags.get(head);
    if (exact !== undefined) {
        return Option.some([[exact], attached]);
    }
    if (row.strict === true && !given.startsWith('--') && [...given.slice(1)].some((letter) => !usage.flags.has(`-${letter}`))) {
        return Option.none();
    }
    if (given.startsWith('--') || ![...usage.flags.keys()].every((spelling) => spelling.startsWith('--') || spelling.length === 2)) {
        return row.strict === true ? Option.none() : Option.some([[{ name: head, arity: 0 }], attached]);
    }
    const letters = [...text.slice(1)];
    const flags = letters.map((letter): Flag => usage.flags.get(`-${letter}`) ?? { name: `-${letter}`, arity: 0 });
    const taking = flags.findIndex(({ arity }) => arity > 0);
    return Option.some(taking < 0 ? [flags, Option.none()] : [flags.slice(0, taking + 1), taking + 1 === letters.length ? Option.none() : Option.some(letters.slice(0, taking + 1).join('').length + 1)]);
};

const optionStep = (word: Word, scan: Scan, words: readonly Word[], row: Program): Option.Option<readonly [Partial<Arguments>, Scan]> =>
    Option.flatMap(optionOf(word, scan.usage, row), ([flags, attached]) =>
        Option.map(Option.match(attached, { onNone: () => Option.some(Option.none<Word>()), onSome: (at) => Option.map(slicedWord(word, at), Option.some) }), (value) => {
            const flag = flags.at(-1);
            const from = scan.at + 1;
            const taken = flag !== undefined && row.execs?.includes(flag.name) === true ? words.slice(from, words.findIndex(({ text }, index) => index >= from && (text === ';' || text === '+')) + 1 || words.length) : words.slice(from, from + (flag?.arity ?? 0));
            const supplied = Option.isSome(value) ? [] : taken;
            const names = flags.map(({ name }) => name);
            return [{ args: [{ names, words: [word, ...supplied], values: [...Option.toArray(value), ...supplied] }] }, { ...scan, at: from + supplied.length, ended: names.some((name) => name === row.module || row.restAfter?.includes(name) === true) }];
        }),
    );

const operandStep = (word: Word, scan: Scan, words: readonly Word[], row: Program): readonly [Partial<Arguments>, Scan] => {
    const deeper = Array.filterMap(scan.wrapped, ([first, ...later]) => (first === word.text ? Result.succeed(later) : Result.failVoid));
    const next = { ...scan, at: scan.at + 1 };
    const direct = scan.wrapped.some((path) => path.length === 0);
    const stops = row.stopsAtOperand;
    if (deeper.length > 0) {
        return [{ path: [word.text] }, { ...next, wrapped: deeper, usage: scan.usage.subcommands.get(word.text) ?? scan.usage }];
    }
    const descended = scan.operand ? undefined : scan.usage.subcommands.get(word.text);
    if (descended !== undefined) {
        return [{ path: [word.text] }, { ...next, usage: descended, subcommand: Option.orElse(scan.subcommand, () => Option.some(word.text)) }];
    }
    if (direct && (row.leading ?? 0) === 0) {
        return [{ rest: words.slice(scan.at) }, { ...scan, at: words.length }];
    }
    return [{ args: [{ names: [], words: [word], values: [] }] }, { ...next, operand: true, ended: direct || stops === true || (stops !== undefined && Option.exists(scan.subcommand, (name) => stops.includes(name))) }];
};

const step = (word: Word, scan: Scan, words: readonly Word[], row: Program): readonly [Partial<Arguments>, Scan] => {
    const end = { ...scan, at: words.length };
    if (scan.ended) {
        return [{ rest: words.slice(scan.at) }, end];
    }
    if (word.text !== '--' || scan.separated) {
        return Option.getOrElse(OPTION.test(word.text) && !scan.separated ? optionStep(word, scan, words, row) : Option.none(), () => operandStep(word, scan, words, row));
    }
    if (scan.wrapped.some((path) => path.length === 0)) {
        return [{}, { ...scan, at: scan.at + 1, separated: true }];
    }
    return [row.wraps === word.text ? { rest: words.slice(scan.at + 1) } : { separator: Option.some(word), tail: words.slice(scan.at + 1) }, end];
};

const parseArguments = (words: readonly Word[], usage: Usage, row: Program): Arguments => {
    const start: Scan = { at: 0, usage, wrapped: row.wraps === '--' ? [] : (row.wraps ?? []), operand: false, ended: false, separated: false, subcommand: Option.none() };
    const steps = Array.unfold(start, (scan) => Option.map(Option.fromNullishOr(words[scan.at]), (word) => step(word, scan, words, row)));
    const args = steps.flatMap(({ args: found = [] }) => found);
    const path = steps.flatMap(({ path: found = [] }) => found);
    const tail = steps.flatMap(({ tail: found = [] }) => found);
    const rest = steps.flatMap(({ rest: found = [] }) => found);
    const names = new Set(args.flatMap((argument) => argument.names));
    const reached = path.reduce((deeper, name) => deeper.subcommands.get(name) ?? deeper, usage);
    const canonical = (name: string): string => reached.flags.get(name)?.name ?? usage.flags.get(name)?.name ?? name;
    const has = (...listed: readonly string[]): boolean => listed.some((name) => names.has(canonical(name)));
    const supplied = (...listed: readonly string[]): readonly Word[] => {
        const wanted = listed.map(canonical);
        return args.filter((argument) => argument.names.some((name) => wanted.includes(name))).flatMap(({ values }) => values);
    };
    const operands = [...args.filter((argument) => argument.names.length === 0).flatMap((argument) => argument.words), ...tail, ...(row.wraps !== undefined || row.module !== undefined || row.restAfter !== undefined ? [] : rest)];
    const count = has(...(row.leadingUnless ?? [])) ? 0 : (row.leading ?? 0);
    return {
        args,
        path,
        tail,
        rest,
        separator: Option.firstSomeOf(steps.map(({ separator = Option.none() }) => separator)),
        names,
        operands,
        leading: operands.slice(0, count),
        inputs: operands.slice(count),
        has,
        values: (...listed) => supplied(...listed).map(({ text }) => text),
        supplied,
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Argument, type Arguments, NO_USAGE, parseArguments, type Usage, usageOf };

import { tmpdir } from 'node:os';
import { Array, Cache, Config, Context, Effect, Layer, Match, Option, Path, Result, String } from 'effect';
import { KeyValueStore } from 'effect/persistence';
import { RedirOperator, type Stmt } from 'sh-syntax/sync';
import { Host, type HostError, KEEP_SUCCESSES, Machine } from '#/host';
import { type Arguments, NO_USAGE, parseArguments, type Usage, usageOf } from '#/shell/arguments';
import type { Program } from '#/shell/programs';
import { type Command, concat, parsed, referenced, type Script, scriptOf, type Word } from '#/shell/script';

// --- [TYPES] ---------------------------------------------------------------------------

interface Invocation extends Arguments {
    readonly command: Command;
    readonly program: string;
    readonly row: Program;
    readonly head: Word;
    readonly assigned: readonly Word[];
    readonly words: readonly [Word, ...Word[]];
    readonly outer: readonly Invocation[];
    readonly roots: readonly Word[];
    readonly informational: boolean;
    readonly repeated: boolean;
    readonly last: boolean;
    readonly readsStdin: boolean;
    readonly sources: readonly Word[];
    readonly script: Option.Option<string>;
}
interface Analysis extends Script {
    readonly invocations: readonly Invocation[];
}
type Start = Pick<Command, 'cwd' | 'origin'> & Partial<Pick<Command, 'fed' | 'looping'>> & { readonly visited?: ReadonlySet<string> };

// --- [CONSTANTS] -----------------------------------------------------------------------

const ASSIGNMENT = /^[A-Za-z_]\w*=/u;
const VERSIONED = /(?:@.*|\d[\d.]*)$/u;
const LEADING_OPTION = /^-[A-Za-z]$/u;
const TOOLCHAIN = /^\+./u;
const HELP: ReadonlySet<string> = new Set(['--help', '--version', '-h', '-V', '-help', '-version']);
const SCRATCH: readonly string[] = [tmpdir(), '/tmp', '/private/tmp', '/var/folders', '/private/var/folders'];
const WRITES: ReadonlySet<RedirOperator> = new Set([RedirOperator.RdrOut, RedirOperator.AppOut, RedirOperator.RdrClob, RedirOperator.AppClob, RedirOperator.RdrAll, RedirOperator.AppAll, RedirOperator.RdrAllClob, RedirOperator.AppAllClob]);
const COPIES: ReadonlySet<string> = new Set(['cat', 'tee']);
const ZSH_FEATURE = /\bzsh feature\b/u;

// --- [SERVICES] ------------------------------------------------------------------------

const Stored = Layer.unwrap(
    Effect.gen(function* () {
        const [machine, path, data] = yield* Effect.all([Machine, Path.Path, Config.option(Config.String('CLAUDE_PLUGIN_DATA'))]);
        return yield* Option.match(data, {
            onNone: () => Effect.succeed(KeyValueStore.layerMemory),
            onSome: (folder) => Effect.map(machine.run(['carapace', '--version'], { cwd: folder }), ({ stdout }) => Layer.orDie(KeyValueStore.layerFileSystem(path.join(folder, String.trim(stdout))))),
        });
    }),
);

class Parser extends Context.Service<Parser>()('@rasm/function-hooks/shell/invocation/Parser', {
    make: Effect.fnUntraced(function* (programs: ReadonlyMap<string, Program>) {
        const usages = yield* Cache.makeWith(
            (program: string) => {
                const row = programs.get(program) ?? {};
                const [primary] = programs.entries().find(([, held]) => held === row) ?? [program];
                return usageOf(program, row, primary);
            },
            { capacity: programs.size, ...KEEP_SUCCESSES },
        );
        yield* Effect.forkScoped(Effect.forEach(programs.keys(), (program) => Effect.ignore(Cache.get(usages, program)), { concurrency: 'unbounded', discard: true }));
        return { usages, programs, interpreters: new Map(Array.reverse(Array.fromIterable(programs).flatMap(([name, { extensions = [] }]) => extensions.map((extension) => [extension, name] as const)))) };
    }),
}) {
    static readonly layer = (programs: ReadonlyMap<string, Program>) => Layer.effect(this, this.make(programs)).pipe(Layer.provide(Layer.orDie(Stored)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const invocationOf = (command: Command, head: Word, args: readonly Word[], assigned: readonly Word[], outer: readonly Invocation[], program: string, row: Program, usage: Usage): Invocation => {
    const parsedArguments = parseArguments(row.toolchain === true && Option.exists(Array.head(args), ({ text }) => TOOLCHAIN.test(text)) ? args.slice(1) : args, usage, row);
    const { names, leading, inputs, has, supplied } = parsedArguments;
    const { informational = [], readsUnless = [], code = [], shell = [] } = row;
    const inner = row.wraps === undefined && !has(...Option.toArray(Option.fromNullishOr(row.module))) ? [] : parsedArguments.rest;
    const own = args.slice(0, args.length - inner.length);
    const roots = Option.match(
        Option.filter(Option.fromNullishOr(row.recursive), ({ when }) => when === undefined || has(...when)),
        {
            onNone: (): readonly Word[] => [],
            onSome: ({ until, roots: rooted = [] }) => {
                const given = [
                    ...supplied(...rooted),
                    ...(until === undefined
                        ? inputs
                        : Array.takeWhile(
                              Array.dropWhile(own, ({ text }) => LEADING_OPTION.test(text)),
                              ({ text }) => !until.some((prefix) => text.startsWith(prefix)),
                          )),
                ];
                return [...given, ...(given.length === 0 && !command.fed ? [{ ...head, raw: '.', text: '.', known: true, glob: false }] : [])];
            },
        },
    );
    const reads = row.reads === true && !leading.some(({ text }) => row.readsNothing?.test(text) === true) && !has(...readsUnless, ...code, ...shell);
    return {
        ...parsedArguments,
        command,
        program,
        row,
        head,
        assigned,
        words: [head, ...own.filter((word) => word !== head)],
        outer,
        roots,
        informational: names.values().some((name) => HELP.has(name) || name.startsWith('--help-')) || has(...informational),
        repeated: command.looping || outer.some((outside) => outside.row.wraps !== undefined && outside.row.reads === true),
        last: inner.length === 0,
        readsStdin: reads && ((row.stopsAtOperand === true ? inputs.slice(0, 1) : inputs).some(({ text }) => text === '-') || (inputs.length === 0 && roots.length === 0)),
        sources: [],
        script: Option.none(),
    };
};

const wrappedWords = ({ row, args, rest, informational, last }: Invocation): readonly (readonly Word[])[] => {
    const execs = args.filter(({ names }) => names.some((name) => row.execs?.includes(name) === true)).map(({ values }) => values.filter(({ text }) => text !== ';' && text !== '+'));
    const module = Array.filterMap(args, ({ names, values: [value] }) => (value !== undefined && row.module !== undefined && names.includes(row.module) ? Result.succeed({ ...value, text: value.text.slice(value.text.lastIndexOf('.') + 1) }) : Result.failVoid));
    if (informational || (execs.length === 0 && last && module.length === 0)) {
        return [];
    }
    return execs.length > 0 ? execs : [[...module, ...rest]];
};

const invocations = Effect.fnUntraced(function* (command: Command, words: readonly Word[], outer: readonly Invocation[]): Effect.fn.Return<readonly Invocation[], HostError, Parser | Host> {
    const [{ usages, programs, interpreters }, { path }] = yield* Effect.all([Parser, Host]);
    const assigned = Array.takeWhile(words, ({ text }) => ASSIGNMENT.test(text));
    const [head, ...args] = words.slice(assigned.length);
    if (head === undefined) {
        return [];
    }
    const leaf = path.basename(head.text);
    const named = [leaf, leaf.replace(VERSIONED, '')].find((candidate) => programs.has(candidate));
    const interpreter = Option.filter(Option.fromUndefinedOr(interpreters.get(path.extname(leaf))), () => named === undefined && head.known);
    const program = named ?? Option.getOrElse(interpreter, () => leaf);
    const invocation = invocationOf(command, head, Option.isSome(interpreter) ? [head, ...args] : args, assigned, outer, program, programs.get(program) ?? {}, programs.has(program) ? yield* Cache.get(usages, program) : NO_USAGE);
    return [invocation, ...(yield* Effect.forEach(wrappedWords(invocation), (inner) => invocations(command, inner, [...outer, invocation]))).flat()];
});

const written = (host: Host['Service'], file: string, { command }: Invocation, found: readonly Invocation[]): Option.Option<Option.Option<string>> =>
    Option.map(
        Array.findLast(found, (writer) => {
            const at = (word: Word) => word.known && Option.contains(host.resolve(word.text, writer.command.cwd), file);
            return writer.command.source === command.source && writer.command.start < command.start && (writer.command.redirects.some(({ op, target }) => WRITES.has(op) && Option.exists(target, at)) || (writer.program === 'tee' && writer.operands.some(at)));
        }),
        (writer) => Option.filter(writer.command.input, () => COPIES.has(writer.program)),
    );

const contentOf = Effect.fnUntraced(function* (invocation: Invocation, text: string, found: readonly Invocation[], visited: ReadonlySet<string>) {
    const host = yield* Host;
    const file = Option.getOrUndefined(Option.filter(host.resolve(text, invocation.command.cwd), (unvisited) => !visited.has(unvisited)));
    if (file === undefined) {
        return Option.none();
    }
    const visiting = visited.union(new Set([file]));
    const writes = written(host, file, invocation, found);
    const authored = [host.root, ...SCRATCH].some((folder) => host.inside(file, folder));
    const read = Option.isSome(writes) || !authored || (yield* host.tracked(file)) ? Option.flatten(writes) : yield* Effect.option(host.fs.readFileString(file));
    return Option.map(read, (body) => ({ text: body, visited: visiting }));
});

const interpreted = (invocation: Invocation, found: readonly Invocation[], visited: ReadonlySet<string>): Effect.Effect<Invocation, HostError, Host> => {
    const { row, has, operands, supplied } = invocation;
    const inline = row.code === undefined || has(...row.code, ...Option.toArray(Option.fromNullishOr(row.module)));
    const named = Option.match(Option.fromNullishOr(row.scripts), { onNone: () => operands.slice(0, 1), onSome: (flags) => supplied(...flags) });
    const sources = (inline ? [] : named).filter(({ known, text }) => known && text !== '-');
    return Effect.map(
        Effect.forEach(sources, ({ text }) => contentOf(invocation, text, found, visited)),
        (read) => ({ ...invocation, sources, script: Option.map(Option.filter(Option.all(read), Array.isReadonlyArrayNonEmpty), (held) => held.map(({ text }) => text).join('\n')) }),
    );
};

const bodyAnalyzed = (invocation: Invocation, origin: Command['origin'], visited: ReadonlySet<string>, enclosing: 'bash' | 'zsh', found: readonly Invocation[]) => {
    const { row, has, operands, command } = invocation;
    const stated = Match.value({ joined: row.concatenates === true, inline: row.shell === undefined ? undefined : has(...row.shell), first: operands[0] }).pipe(
        Match.when({ joined: true }, () => Option.some({ text: operands.map(({ text }) => text).join(' '), file: false })),
        Match.when({ inline: true, first: Match.defined }, ({ first }) => Option.some({ text: first.text, file: false })),
        Match.when({ inline: false, first: Match.undefined }, () => Option.map(command.input, (text) => ({ text, file: false }))),
        Match.when({ inline: false, first: { known: true } }, ({ first }) => Option.some({ text: first.text, file: true })),
        Match.orElse(() => Option.none()),
    );
    return Option.match(stated, {
        onNone: () => Effect.succeedNone,
        onSome: Effect.fnUntraced(function* ({ text, file }) {
            const content = file ? yield* contentOf(invocation, text, found, visited) : Option.some({ text, visited });
            const named = invocation.program === 'zsh' || (file && text.endsWith('.zsh')) ? 'zsh' : 'bash';
            const dialect = row.concatenates === true ? enclosing : named;
            const stmts = Option.flatten(yield* Effect.transposeOption(Option.map(content, (body) => Effect.option(parsed(body.text, dialect)))));
            return yield* Effect.transposeOption(Option.map(Option.all({ body: content, held: stmts }), ({ body, held }) => analysisOf(held, body.text, { cwd: command.cwd, fed: command.fed, looping: invocation.repeated, origin: file || origin === 'file' ? 'file' : 'body', visited: body.visited }, dialect)));
        }),
    });
};

const analysisOf = Effect.fnUntraced(function* (stmts: readonly Stmt[], text: string, { visited = new Set(), ...start }: Start, dialect: 'bash' | 'zsh'): Effect.fn.Return<Analysis, HostError, Parser | Host> {
    const host = yield* Host;
    const names = referenced(text);
    const environment = new Map(Array.zip(names, yield* Effect.forEach(names, host.environment)));
    const script = scriptOf(stmts, text, { ...start, dialect, home: host.home, environment, resolve: host.resolve });
    const listed = (yield* Effect.forEach(script.commands, (command) => invocations(command, command.words, []))).flat();
    const found = yield* Effect.forEach(listed, (invocation) => interpreted(invocation, listed, visited), { concurrency: 'unbounded' });
    const nested = yield* Effect.forEach(
        found.filter(({ last, informational }) => last && !informational),
        (invocation) => bodyAnalyzed(invocation, start.origin, visited, dialect, found),
        { concurrency: 'unbounded' },
    );
    const scripts = Array.getSomes(nested);
    return { ...concat([script, ...scripts]), invocations: [...found, ...scripts.flatMap((analysis) => analysis.invocations)] };
});

const analyzed = Effect.fnUntraced(function* (text: string, start: Start, dialect: 'bash' | 'zsh' = 'bash') {
    const stated = yield* Effect.result(parsed(text, dialect));
    const read = Result.isFailure(stated) && dialect === 'bash' && ZSH_FEATURE.test(stated.failure.message) ? 'zsh' : dialect;
    const stmts = yield* Effect.option(read === dialect ? Effect.fromResult(stated) : parsed(text, read));
    return yield* analysisOf(
        Option.getOrElse(stmts, () => []),
        text,
        start,
        read,
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Analysis, analyzed, type Invocation, Parser, WRITES };

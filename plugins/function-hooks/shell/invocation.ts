import { Array, Cache, Context, Effect, Layer, Option, Result, Schema } from 'effect';
import { Host, HostError, KEEP_SUCCESSES } from '#/host';
import { type Arguments, NO_USAGE, parseArguments, type Usage, usageOf } from '#/shell/arguments';
import type { Program } from '#/shell/programs';
import { type Command, concat, referenced, type Script, scriptOf, Tree, type Word } from '#/shell/script';

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
}
interface Analysis extends Script {
    readonly invocations: readonly Invocation[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const ASSIGNMENT = /^[A-Za-z_]\w*=/u;
const VERSIONED = /(?:@.*|\d[\d.]*)$/u;
const SCRIPT_FILE = /\.(?:sh|bash|zsh)$/u;
const LEADING_OPTION = /^-[A-Za-z]$/u;
const HELP: readonly string[] = ['--help', '--version', '-h', '-V', '-help', '-version'];

// --- [SERVICES] ------------------------------------------------------------------------

class Parser extends Context.Service<Parser>()('@rasm/function-hooks/shell/invocation/Parser', {
    make: (programs: ReadonlyMap<string, Program>) =>
        Effect.map(
            Effect.all({
                trees: Cache.makeWith(tree, { capacity: 1024, ...KEEP_SUCCESSES }),
                usages: Cache.makeWith(
                    (program: string) => {
                        const row = programs.get(program) ?? {};
                        const [[primary] = [program]] = [...programs].filter(([, held]) => held === row);
                        return usageOf(program, row, primary);
                    },
                    { capacity: programs.size, ...KEEP_SUCCESSES },
                ),
            }),
            (caches) => ({ ...caches, programs }),
        ),
}) {
    static readonly layer = (programs: ReadonlyMap<string, Program>) => Layer.effect(this, this.make(programs));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const tree = Effect.fnUntraced(function* ([dialect, source]: readonly [dialect: 'bash' | 'zsh', source: string]) {
    const host = yield* Host;
    const { code, stdout, stderr } = yield* host.run(['shfmt', '--to-json', '-ln', dialect], { stdin: source });
    const { Stmts = [] } = code === 0 ? yield* Effect.mapError(Schema.decodeEffect(Tree)(stdout), HostError.of('shfmt')) : yield* HostError.of('shfmt')(stderr);
    return Stmts;
});

const invocationOf = (command: Command, head: Word, args: readonly Word[], assigned: readonly Word[], outer: readonly Invocation[], program: string, row: Program, usage: Usage): Invocation => {
    const parsed = parseArguments(args, usage, row);
    const { names, leading, inputs, has, supplied } = parsed;
    const { informational = [], readsUnless = [], code = [], shell = [] } = row;
    const inner = row.wraps === undefined && row.module === undefined ? [] : parsed.rest;
    const own = args.slice(0, args.length - inner.length);
    const { until, when, roots: rooted = [] } = row.recursive ?? {};
    const given = [
        ...supplied(...rooted),
        ...(until === undefined
            ? inputs
            : Array.takeWhile(
                  Array.dropWhile(own, ({ text }) => LEADING_OPTION.test(text)),
                  ({ text }) => !until.some((prefix) => text.startsWith(prefix)),
              )),
    ];
    const roots = row.recursive === undefined || (when !== undefined && !has(...when)) ? [] : [...given, ...(given.length === 0 && !command.fed ? [{ ...head, raw: '.', text: '.', known: true, glob: false }] : [])];
    const reads = row.reads === true && !leading.some(({ text }) => row.readsNothing?.test(text) === true) && !has(...readsUnless, ...code, ...shell);
    return {
        ...parsed,
        command,
        program,
        row,
        head,
        assigned,
        words: [head, ...own],
        outer,
        roots,
        informational: HELP.some((name) => names.has(name)) || has(...informational),
        repeated: command.looping || outer.some((outside) => outside.row.wraps !== undefined && outside.row.reads === true),
        last: inner.length === 0,
        readsStdin: reads && ((row.stopsAtOperand === true ? inputs.slice(0, 1) : inputs).some(({ text }) => text === '-') || (inputs.length === 0 && roots.length === 0)),
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
    const { usages, programs } = yield* Parser;
    const assigned = Array.takeWhile(words, ({ text }) => ASSIGNMENT.test(text));
    const [head, ...args] = words.slice(assigned.length);
    if (head === undefined) {
        return [];
    }
    const leaf = head.text.slice(head.text.lastIndexOf('/') + 1);
    const program = [leaf, leaf.replace(VERSIONED, '')].find((candidate) => programs.has(candidate)) ?? leaf;
    const invocation = invocationOf(command, head, args, assigned, outer, program, programs.get(program) ?? {}, programs.has(program) ? yield* Cache.get(usages, program) : NO_USAGE);
    return [invocation, ...(yield* Effect.forEach(wrappedWords(invocation), (inner) => invocations(command, inner, [...outer, invocation]))).flat()];
});

const bodies = (invocation: Invocation) => {
    const { row, has, operands, command, head, program } = invocation;
    if (row.concatenates === true) {
        return [{ invocation, text: operands.map(({ text }) => text).join(' '), file: false }];
    }
    const [first] = operands;
    const shell = row.shell !== undefined;
    const inline = shell && has(...row.shell);
    return [
        ...(inline && first !== undefined ? [{ invocation, text: first.text, file: false }] : []),
        ...(shell && !inline && first === undefined ? Option.toArray(Option.map(command.input, (text) => ({ invocation, text, file: false }))) : []),
        ...(shell && !inline && first?.known === true ? [{ invocation, text: first.text, file: true }] : []),
        ...(!shell && head.known && program === head.text.slice(head.text.lastIndexOf('/') + 1) && head.text.includes('/') && SCRIPT_FILE.test(head.text) ? [{ invocation, text: head.text, file: true }] : []),
    ];
};

const bodyAnalyzed = Effect.fnUntraced(function* ({ invocation, text, file }: ReturnType<typeof bodies>[number], origin: Command['origin'], visited: ReadonlySet<string>): Effect.fn.Return<Option.Option<Analysis>, HostError, Parser | Host> {
    const host = yield* Host;
    const { cwd, fed } = invocation.command;
    const script = Option.getOrUndefined(Option.filter(host.resolve(text, cwd), (unvisited) => !visited.has(unvisited)));
    const tracked = !file || script === undefined || (host.inside(script, host.root) && (yield* Effect.orElseSucceed(host.run(['git', 'ls-files', '--error-unmatch', '--', script]), () => ({ code: 1 }))).code === 0);
    const content = !tracked && script !== undefined ? Option.map(yield* Effect.option(host.fs.readFileString(script)), (read) => ({ text: read, visited: visited.union(new Set([script])) })) : Option.liftPredicate({ text, visited }, () => !file);
    return yield* Effect.transposeOption(Option.map(content, (body) => analyzed(body.text, { cwd, fed, looping: invocation.repeated, origin: file || origin === 'file' ? 'file' : 'body', visited: body.visited }, invocation.program === 'zsh' || (file && text.endsWith('.zsh')) ? 'zsh' : 'bash')));
});

const analyzed = Effect.fnUntraced(function* (text: string, { visited = new Set(), ...start }: Pick<Command, 'cwd' | 'origin'> & Partial<Pick<Command, 'fed' | 'looping'>> & { readonly visited?: ReadonlySet<string> }, dialect: 'bash' | 'zsh' = 'bash'): Effect.fn.Return<Analysis, HostError, Parser | Host> {
    const [{ trees }, host] = yield* Effect.all([Parser, Host]);
    const names = referenced(text);
    const environment = new Map(Array.zip(names, yield* Effect.forEach(names, host.environment)));
    const script = scriptOf(yield* Cache.get(trees, [dialect, text]), text, { ...start, home: host.home, environment, resolve: host.resolve });
    const found = (yield* Effect.forEach(script.commands, (command) => invocations(command, command.words, []))).flat();
    const nested = yield* Effect.forEach(found.filter(({ last, informational }) => last && !informational).flatMap(bodies), (body) => bodyAnalyzed(body, start.origin, visited), { concurrency: 'unbounded' });
    const scripts = Array.getSomes(nested);
    return { ...concat([script, ...scripts]), invocations: [...found, ...scripts.flatMap((analysis) => analysis.invocations)] };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Analysis, analyzed, type Invocation, Parser };

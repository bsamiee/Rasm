import { Array, Cache, Context, Duration, Effect, Exit, Layer, Option, Result } from 'effect';
import { type HookError, Host } from '#/host';
import { type Grammar, grammarOf, NO_GRAMMAR, type Parsed, parsed } from '#/shell/grammar';
import type { Program } from '#/shell/programs';
import { type Beginning, type Command, merged, type Script, scripted, tree, type Word } from '#/shell/script';

// --- [TYPES] ---------------------------------------------------------------------------

interface Site extends Parsed {
    readonly command: Command;
    readonly program: string;
    readonly row: Program;
    readonly head: Word;
    readonly assigned: readonly Word[];
    readonly words: readonly [Word, ...Word[]];
    readonly outer: readonly Site[];
    readonly operands: readonly Word[];
    readonly leading: readonly Word[];
    readonly inputs: readonly Word[];
    readonly names: ReadonlySet<string>;
    readonly informational: boolean;
    readonly repeated: boolean;
    readonly last: boolean;
    readonly has: (...names: readonly string[]) => boolean;
    readonly values: (...names: readonly string[]) => readonly string[];
    readonly supplied: (...names: readonly string[]) => readonly Word[];
    readonly on: (role: 'timer' | 'endless' | 'closes') => boolean;
}
interface Analysis extends Script {
    readonly sites: readonly Site[];
}
interface Body {
    readonly site: Site;
    readonly text: string;
    readonly file: boolean;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const ASSIGNMENT = /^[A-Za-z_]\w*=/u;
const VERSIONED = /(?:@.*|\d[\d.]*)$/u;
const SCRIPT_FILE = /\.(?:sh|bash|zsh)$/u;
const HELP: readonly string[] = ['--help', '--version', '-h', '-V', '-help', '-version'];
const KEEP_SUCCESSES = { timeToLive: (exit: Exit.Exit<unknown, unknown>) => (Exit.isSuccess(exit) ? Duration.infinity : Duration.zero) };

// --- [SERVICES] ------------------------------------------------------------------------

class Shell extends Context.Service<Shell>()('@rasm/function-hooks/shell/site/Shell', {
    make: (programs: ReadonlyMap<string, Program>) =>
        Effect.map(
            Effect.all({
                trees: Cache.makeWith((source: string) => tree(source, 'bash'), { capacity: 1024, ...KEEP_SUCCESSES }),
                grammars: Cache.makeWith((program: string) => grammarOf(program, programs.get(program) ?? {}, [...programs].find(([, row]) => row === programs.get(program))?.[0] ?? program), { capacity: programs.size, ...KEEP_SUCCESSES }),
                installed: Cache.makeWith((program: string) => Host.use((host) => host.installed(program)), { capacity: programs.size, ...KEEP_SUCCESSES }),
                ignored: Cache.makeWith(ignoredAny, { capacity: 1024, ...KEEP_SUCCESSES }),
            }),
            (caches) => ({ ...caches, programs }),
        ),
}) {
    static readonly layer = (programs: ReadonlyMap<string, Program>) => Layer.effect(this, this.make(programs));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const ignoredAny = Effect.fnUntraced(function* (places: readonly string[]) {
    const host = yield* Host;
    const reals = yield* Effect.forEach(places, host.real, { concurrency: 'unbounded' });
    const owned = reals.filter((real) => host.inside(real, host.root));
    return owned.length > 0 && (yield* host.run(['git', 'check-ignore', '--no-index', '--stdin', '-z'], { stdin: owned.join('\0') })).code === 0;
});

const built = (command: Command, head: Word, assigned: readonly Word[], args: readonly Word[], grammar: Grammar, program: string, row: Program, outer: readonly Site[]): Site => {
    const reading = parsed(args, grammar, row);
    const launching = row.wraps !== undefined || row.module !== undefined;
    const before = reading.items.filter((item) => item.names.length === 0).flatMap(({ words }) => words);
    const operands = [...before, ...reading.tail, ...(launching || row.ends !== undefined ? [] : reading.rest)];
    const names = new Set(reading.items.flatMap((item) => item.names));
    const reached = reading.path.reduce((deeper, name) => deeper.commands.get(name) ?? deeper, grammar);
    const canonical = (name: string): string => reached.options.get(name)?.name ?? grammar.options.get(name)?.name ?? name;
    const holding = (listed: readonly string[]) => {
        const wanted = listed.map(canonical);
        return reading.items.filter((item) => item.names.some((name) => wanted.includes(name)));
    };
    const has = (...listed: readonly string[]): boolean => listed.some((name) => names.has(canonical(name)));
    const count = has(...(row.leadingUnless ?? [])) ? 0 : (row.leading ?? 0);
    return {
        ...reading,
        command,
        program,
        row,
        head,
        assigned,
        words: [head, ...args.slice(0, args.length - (launching ? reading.rest.length : 0))],
        outer,
        operands,
        leading: operands.slice(0, count),
        inputs: operands.slice(count),
        names,
        informational: HELP.some((name) => names.has(name)) || has(...(row.informational ?? [])),
        repeated: command.looping || outer.some(({ row: wrapper }) => wrapper.reads === 'always'),
        last: !launching || reading.rest.length === 0,
        has,
        values: (...listed) => holding(listed).flatMap(({ values }) => values),
        supplied: (...listed) => holding(listed).flatMap(({ words: [, ...supplied] }) => supplied),
        on: (role) => {
            const when = row[role];
            return when === true || (when !== undefined && has(...when));
        },
    };
};

const innerWords = ({ row, items, rest, informational, last }: Site): readonly (readonly Word[])[] => {
    const launched = items.filter(({ names }) => names.some((name) => row.execs?.includes(name) === true)).map(({ words: [, ...supplied] }) => supplied.filter(({ text }) => text !== ';' && text !== '+'));
    const module = Array.filterMap(items, ({ names, words: [, value] }) => (value !== undefined && names.includes(row.module ?? '') ? Result.succeed({ ...value, text: value.text.slice(value.text.lastIndexOf('.') + 1) }) : Result.failVoid));
    if (informational || (launched.length === 0 && last && module.length === 0)) {
        return [];
    }
    return launched.length > 0 ? launched : [[...module, ...rest]];
};

const invocations = Effect.fnUntraced(function* (command: Command, words: readonly Word[], outer: readonly Site[]): Effect.fn.Return<readonly Site[], HookError, Shell> {
    const { grammars, programs } = yield* Shell;
    const assigned = Array.takeWhile(words, ({ text }) => ASSIGNMENT.test(text));
    const [head, ...args] = words.slice(assigned.length);
    if (head === undefined) {
        return [];
    }
    const leaf = head.text.slice(head.text.lastIndexOf('/') + 1);
    const name = leaf.replace(VERSIONED, '');
    const program = [leaf, name].find((held) => programs.has(held)) ?? leaf;
    const site = built(command, head, assigned, args, programs.has(program) ? yield* Cache.get(grammars, program) : NO_GRAMMAR, program, programs.get(program) ?? {}, outer);
    return [site, ...(yield* Effect.forEach(innerWords(site), (inner) => invocations(command, inner, [...outer, site]))).flat()];
});

const bodies = (site: Site): readonly Body[] => {
    const { row, has, operands, command, head, program } = site;
    if (row.joins === true) {
        return [{ site, text: operands.map(({ text }) => text).join(' '), file: false }];
    }
    const [first] = operands;
    const shell = row.shell !== undefined;
    const inline = shell && has(...row.shell);
    return [
        ...(inline && first !== undefined ? [{ site, text: first.text, file: false }] : []),
        ...(shell && !inline && first === undefined ? Option.toArray(Option.map(command.input, (text) => ({ site, text, file: false }))) : []),
        ...(shell && !inline && first?.known === true ? [{ site, text: first.text, file: true }] : []),
        ...(!shell && head.known && program === head.text.slice(head.text.lastIndexOf('/') + 1) && head.text.includes('/') && SCRIPT_FILE.test(head.text) ? [{ site, text: head.text, file: true }] : []),
    ];
};

const bodyAnalyzed = Effect.fnUntraced(function* ({ site, text, file }: Body, { origin, visited = new Set() }: Beginning): Effect.fn.Return<Option.Option<Analysis>, HookError, Shell | Host> {
    const host = yield* Host;
    const place = Option.getOrUndefined(Option.filter(host.place(text, site.command.cwd), (held) => !visited.has(held)));
    const tracked = !file || place === undefined || (host.inside(place, host.root) && (yield* Effect.orElseSucceed(host.run(['git', 'ls-files', '--error-unmatch', '--', place]), () => ({ code: 1 }))).code === 0);
    const content = !tracked && place !== undefined ? Option.map(yield* Effect.option(host.fs.readFileString(place)), (held) => ({ text: held, visited: visited.union(new Set([place])) })) : Option.liftPredicate({ text, visited }, () => !file);
    return yield* Effect.transposeOption(Option.map(content, (held) => analyzed(held.text, { cwd: site.command.cwd, fed: site.command.fed, looping: site.repeated, origin: file || origin === 'file' ? 'file' : 'body', visited: held.visited }, site.program === 'zsh' || (file && text.endsWith('.zsh')) ? 'zsh' : 'bash')));
});

const analyzed = Effect.fnUntraced(function* (text: string, beginning: Beginning, dialect: 'bash' | 'zsh' = 'bash'): Effect.fn.Return<Analysis, HookError, Shell | Host> {
    const { trees } = yield* Shell;
    const script = yield* scripted(dialect === 'bash' ? yield* Cache.get(trees, text) : yield* tree(text, dialect), text, beginning);
    const sites = (yield* Effect.forEach(script.commands, (command) => invocations(command, command.words, []))).flat();
    const nested = yield* Effect.forEach(sites.filter(({ last, informational }) => last && !informational).flatMap(bodies), (body) => bodyAnalyzed(body, beginning), { concurrency: 'unbounded' });
    const found = Array.getSomes(nested);
    return { ...merged([script, ...found]), sites: [...sites, ...found.flatMap((analysis) => analysis.sites)] };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Analysis, analyzed, Shell, type Site };

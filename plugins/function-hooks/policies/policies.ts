import { Option } from '../composition.ts';
import { type Command, parse, printed, type Script, spliced, type Word } from './command.ts';
import { identified } from './instances.ts';
import { declared, firstOperand, type Invocation, informational, invocations, marked, operands, option, optional, placed, unquoted, waits, within } from './invocation.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Decision = { readonly kind: 'deny'; readonly reason: string } | { readonly kind: 'allow' } | { readonly kind: 'rewrite'; readonly command: string; readonly notice: Option<string>; readonly timeout: Option<number>; readonly background: boolean };
type Request = { readonly kind: 'foreground'; readonly timeout: Option<number> } | { readonly kind: 'background' } | { readonly kind: 'monitor' } | { readonly kind: 'plain' };
type Verdict = { readonly kind: 'refuse'; readonly reason: string } | { readonly kind: 'edit'; readonly start: number; readonly end: number; readonly text: string; readonly notice: Option<string>; readonly timeout: Option<number> } | { readonly kind: 'background'; readonly notice: string };
type Route = { readonly kind: 'rewrite'; readonly program: string; readonly start: Word; readonly text: string } | { readonly kind: 'refuse'; readonly reason: string };
type Statement = { readonly kind: (typeof STATEMENTS)[number][0] | 'drive' } | { readonly kind: 'open'; readonly path: string } | { readonly kind: 'shell'; readonly inner: string };

interface System {
    readonly run: (argv: Invocation, init: { readonly cwd: string; readonly stdin: string }) => Promise<{ readonly exitCode: number; readonly stdout: string; readonly stderr: string }>;
    readonly exists: (path: string) => Promise<boolean>;
    readonly read: (path: string) => Promise<string>;
    readonly environment: (name: string) => Promise<Option<string>>;
    readonly scanner: string;
    readonly plugin: string;
    readonly root: Option<string>;
    readonly cwd: string;
    readonly home: Option<string>;
}
interface Call {
    readonly system: System;
    readonly source: Uint8Array;
    readonly script: Script;
    readonly project: string;
    readonly request: Request;
    readonly apps: ReadonlyMap<string, string>;
    readonly writers: ReadonlyMap<string, string>;
    readonly routes: ReadonlyMap<Command, Route>;
}
interface Waited {
    readonly commands: readonly Command[];
    readonly command: Command;
    readonly invocation: Invocation;
    readonly at: number;
    readonly kind: Request['kind'];
}
interface Launcher {
    readonly app: Option<string>;
    readonly waits: readonly string[];
    readonly kept: readonly string[];
}
interface Target {
    readonly options?: { readonly command?: string; readonly commands?: readonly (string | { readonly command: string })[] };
    readonly dependsOn?: readonly (string | { readonly target: string })[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORKTREE = 'creates second checkout. Work in main checkout';
const BRANCH = 'creates a branch. Stay on current branch';
const UNPLACED = 'follows unresolved cd. cd to a literal path';
const ALIAS = 'defines alias hiding refused subcommands';
const OVERWRITES = 'overwrites edits. Use the Edit tool';
const HEADLESS = 'analyzeHeadless';
const PROJECT = 'GHIDRA_PROJECT_DIR';
const SERVE = 'Serve.java';
const CEILING = 600_000;
const BREAK = /(?<!\\)(?:\\\\)*\\n/u;
const EXPANDS = /[$`]/u;
const GLOB = /[*?[{]/u;
const NAMED = /[\p{L}\p{N}]/u;
const SAFE = /^[\w@%+=:,./-]+$/u;
const LETTER = /^\w/u;
const OWNED = /^(?<name>GHIDRA_(?:HEADLESS_)?MAXMEM)(?:=|$)/u;
const PROJECTS = /^"?\$\{?GHIDRA_PROJECT_DIR\}?"?$/u;
const PROJECTED = /\.(?:gpr|rep)$/u;
const LOCK = /^(?<stem>[^/]*?)\.?lock[*~]*$/u;
const DURATION = /^(?<value>\d+(?:\.\d+)?)(?<unit>[smh]?)$/u;
const BUNDLE = /^(?<bundle>\/.+?\.app)\/Contents\/MacOS\/(?<executable>[^/]+)$/u;
const EXECUTABLE = /\/Contents\/MacOS\/(?<executable>[^/]+)$/u;
const SUBSTITUTION = /\$\(|`/u;
const TASK = /\/tasks\/(?<id>[^/]+)\.output$/u;
const TASKS = /\/tasks\/(?<id>[^/\s'"]+)\.output/gu;
const TELL = /^tell (?:application|app) (?:id )?"(?<app>[^"]+)"(?: to (?<body>.+))?$/u;
const AIMED = /^(?<body>\w+) (?:application|app) (?:id )?"(?<app>[^"]+)"$/u;
const OPENS = /^open (?:POSIX file )?"(?<path>[^"]+)"$/u;
const SHELLS = /^do shell script "(?<inner>(?:[^"\\]|\\.)*)"$/u;
const STATEMENTS = [
    ['launch', /^(?:activate|launch|run|reopen)$/u],
    ['quit', /^quit(?: saving (?:no|yes|ask))?$/u],
    ['hide', /^hide$/u],
    ['script', /^do (?:javascript|script)\b/iu],
] as const;
const SCRIPTED = /\b(?:Application|NSRunningApplication|NSWorkspace)\b/u;
const IDENTIFIED = /^bundleid=(?<id>.+)$/iu;
const WEB = /^https?:/iu;
const PID = /^\d+$/u;
const DAEMON = /\btype="BackgroundOnly"/u;
const SIGNAL = /^-?(?:SIG)?/u;
const SPACES = /\s+/u;
const EVENTS = 'System Events';
const WATCHER = 'watch-apps';
const FINDERS: ReadonlySet<string> = new Set(['ps', 'pgrep', 'pidof']);
const LAUNCHERS = {
    open: { app: Option.none, waits: ['-W', '--wait-apps', '--stdin', '--stdout', '--stderr'], kept: ['-a', '-b', '-n', '--new', '-g', '--background', '-j', '--hide', '-F', '--fresh', '--env', '-u', '--url'] },
    xed: { app: Option.some('com.apple.dt.Xcode'), waits: ['-w', '--wait'], kept: ['-x', '--launch', '-b', '--background'] },
} as const satisfies Readonly<Record<string, Launcher>>;
const ARGUMENT = '--arguments';
const DOCUMENT = '--document';
const CHECKER = /^(?:lint|format|typecheck):(?<tool>[^:]+)/u;
const CACHED = /^(?:check|typecheck(?::.+)?)$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [VERDICTS]

const refuse = (reason: string): Verdict => ({ kind: 'refuse', reason });

const edit = (start: number, end: number, text: string, notice: Option<string> = Option.none, timeout: Option<number> = Option.none): Verdict => ({ kind: 'edit', start, end, text, notice, timeout });

const backgrounded = (cause: string): Verdict => ({ kind: 'background', notice: `${cause} backgrounded the call. Wait for its completion notice` });

const corrected = (command: Command, change: Verdict, reason: string): Verdict => (command.origin === 'call' ? change : refuse(reason));

// --- [WORDS]

const quote = (word: string): string => (SAFE.test(word) ? word : `'${word.replaceAll("'", String.raw`'\''`)}'`);

const items = (program: string, [head, ...rest]: readonly Word[]): readonly { readonly words: readonly Word[]; readonly names: readonly string[] }[] => {
    if (head === undefined || head.text === '--') {
        return [];
    }
    const { names, taken } = optional(declared(program), head.text) ? option(declared(program), head.text) : { names: [], taken: 0 };
    return [{ words: [head, ...rest.slice(0, taken)], names }, ...items(program, rest.slice(taken))];
};

const positional = (program: string, words: readonly Word[]): Option<Word> => Option.given(items(program, words).find(({ names }) => names.length === 0)?.words[0]);

const milliseconds = (amount: string): Option<number> => {
    const groups = DURATION.exec(amount)?.groups;
    const second = 1000;
    const value = Number(groups?.value) * second * 60 ** 'smh'.indexOf(String(groups?.unit));
    return value > 0 ? Option.some(value) : Option.none;
};

// --- [DELETE]

const deletes = async ({ system, script: { commands }, project }: Call): Promise<readonly Verdict[]> => {
    const { root, home } = system;
    const removing = (invocation: Invocation): readonly Invocation[] => {
        if (invocation[0] !== 'git') {
            return ['rm', 'rimraf', 'trash', 'unlink', 'shred'].includes(invocation[0]) ? [invocation] : [];
        }
        const [key, ...args] = invocation.slice(firstOperand(invocation, 1));
        return key === 'rm' && !args.includes('--cached') ? [['rm', ...args]] : [];
    };
    const searched = (invocation: Invocation, cwd: Option<string>): { readonly roots: readonly string[]; readonly folder: Option<string>; readonly patterns: readonly string[] } => {
        const [program, ...args] = invocation;
        const { roots, base, leading, values } = operands(invocation);
        const tests = ['-name', '-iname', '-path', '-ipath', '-wholename', '-iwholename', '-regex', '-iregex'];
        const patterns = program === 'find' || program === 'gfind' ? args.flatMap((word, at) => (tests.includes(word) ? args.slice(at + 1, at + 2) : [])) : [...leading, ...values.flatMap(([name, value]) => (name === '-e' || name === '--extension' ? [value] : []))];
        return { roots, folder: base.reduce((folder, word) => placed(word, folder, home), cwd), patterns };
    };
    const anchored = (word: string, unbounded: boolean, cwd: Option<string>): Option<string> => {
        const cut = unbounded ? word.search(GLOB) : -1;
        return placed(cut < 0 ? word : word.slice(0, word.lastIndexOf('/', cut) + 1), cwd, home);
    };
    const judged = commands.flatMap((command): readonly (readonly Verdict[] | Promise<readonly Verdict[]>)[] => {
        const { words, invocations: chain, cwd } = command;
        const last = chain.at(-1);
        if (last === undefined) {
            return [];
        }
        const [program] = last;
        const tail = words.slice(words.length - last.length);
        const judge =
            (name: string, covered: (word: string) => string, start: Option<string>, patterns: readonly string[]) =>
            async (word: string): Promise<readonly Verdict[]> => {
                const spoken = tail.filter(({ text, unbounded }) => unbounded && text === word);
                const [expanded] = EXPANDS.test(word) ? spoken : [];
                const path = expanded === undefined ? word : unquoted(expanded.literal);
                if (expanded !== undefined && EXPANDS.test(path)) {
                    return [refuse(`${name} ${word} expands at run time. Name literal path`)];
                }
                const [place] = anchored(path, spoken.length > 0, start).toArray();
                if (place === undefined) {
                    return [refuse(`${name} ${UNPLACED}`)];
                }
                const selective = patterns.some((pattern) => NAMED.test(pattern));
                if (root.toArray().some((top) => !selective && within(top, place))) {
                    return [refuse(covered(path))];
                }
                const stated = expanded === undefined ? [] : spoken.map(({ raw, span, literal }) => corrected(command, edit(span.start, span.end, literal, Option.some(`${name} took ${literal} for ${raw}`)), `${name} ${raw} expands at run time. Name ${literal}`));
                const [target = place] = placed(path, start, home).toArray();
                const targets = patterns.length === 0 ? [target] : patterns.map((pattern) => `${target}/${pattern}`);
                return [...stated, ...(await Promise.all(targets.map((candidate) => locked(system, name, candidate)))).flat()];
            };
        const { options, inner } = operands(last);
        const [remover] = removing(last);
        const [feeder] = chain.slice(0, -1).filter(([name]) => declared(name).stdin === 'always');
        if (remover !== undefined) {
            const name = program === 'git' ? 'git rm' : program;
            return feeder === undefined ? [...new Set(operands(remover).inputs)].map(judge(name, (word) => `${name} ${word} covers repository root. Run nx run ${project}:clean`, cwd, [])) : [[refuse(`${name} reads paths from ${feeder[0]}. Use fd -X ${name}`)]];
        }
        if (!(options.includes('-delete') || invocations(inner).some((launched) => removing(launched).length > 0))) {
            return [];
        }
        const { roots, folder, patterns } = searched(last, cwd);
        return [...new Set(roots)].map(judge(program, (word) => `${program} deletes under ${word} unfiltered. Add -name pattern`, folder, patterns));
    });
    return (await Promise.all(judged)).flat();
};

// --- [GIT]

const flagged = (args: readonly string[], reason: string, words: readonly string[], prefixes: readonly string[]): readonly string[] =>
    args
        .filter((word) => words.includes(word) || prefixes.some((prefix) => word.startsWith(prefix)))
        .slice(0, 1)
        .map((word) => `${word} ${reason}`);

const GIT = new Map(
    Object.entries<(args: readonly string[], present: (word: string) => boolean) => readonly string[]>({
        branch: (args) => {
            const deleting = flagged(args, 'deletes or force-moves a branch', ['-d', '-D', '-M', '--delete'], ['--force']);
            const listing = args.some((word) => ['-l', '--list', '-a', '--all', '-r', '--remotes', '-v', '-vv', '--verbose', '--show-current', '--contains', '--no-contains', '--merged', '--no-merged', '--points-at'].includes(word));
            const creating = flagged(args, BRANCH, ['-c', '-C', '--copy', '-m', '--move'], []);
            return [...deleting, ...creating, ...(!listing && args.some((word) => !word.startsWith('-')) ? [BRANCH] : [])].slice(0, 1);
        },
        checkout: (args, present) => {
            const forced = flagged(args, 'discards local changes', ['-f', '-B', '-p', '--patch', '--ours', '--theirs'], ['--force']);
            const creating = flagged(args, BRANCH, ['-b', '--orphan', '-t', '--track'], []);
            const [first, ...more] = args.filter((word) => word === '-' || !word.startsWith('-'));
            const separated = args.includes('--');
            const paths = separated || more.length > 0 || first === '.' || first?.startsWith(':') === true || (first !== undefined && first !== '-' && present(first));
            return [...forced, ...creating, ...(args.includes('--detach') || !paths ? [] : [`${first ?? '--'} ${OVERWRITES}`])].slice(0, 1);
        },
        clean: () => ['deletes untracked files. rm named files'],
        config: (args) => args.flatMap((word, at) => (word.startsWith('alias.') && args.slice(at + 1).some((value) => !value.startsWith('-')) ? [`${word} ${ALIAS}`] : [])).slice(0, 1),
        push: (args) => flagged(args, 'rewrites or deletes remote history', ['-f', '-d', '--delete', '--mirror', '--prune'], ['--force', '+', ':']),
        rebase: () => ['rewrites commits other sessions hold'],
        reflog: (args) => flagged(args, 'erases reflog entries', ['delete', 'drop', 'expire'], []),
        reset: (args, present) => {
            const forced = flagged(args, 'discards working-tree or index state', ['--hard', '--merge', '--keep'], []);
            const targets = args.filter((word) => !word.startsWith('-'));
            const [target] = targets;
            return forced.length > 0 || target === undefined || args.includes('--') || targets.some(present) ? forced : [`${target} moves HEAD and drops branch commits`];
        },
        restore: (args) => {
            const { options } = operands(['git', ...args]);
            return options.some((name) => name === '-S' || name === '--staged') && !options.some((name) => name === '-W' || name === '--worktree') ? [] : [OVERWRITES];
        },
        revert: () => ['reverses committed history'],
        stash: ([first]) => (first === 'list' || first === 'show' ? [] : ['hides uncommitted work other sessions need']),
        switch: (args) => {
            const forced = flagged(args, 'discards local changes', ['-f', '-C', '--discard-changes'], ['--force']);
            return [...forced, ...flagged(args, BRANCH, ['-c', '--create', '--orphan'], [])].slice(0, 1);
        },
        worktree: ([first]) => (first === 'list' ? [] : [WORKTREE]),
    }),
);

const git = async ({ system: { exists, root, home }, script: { commands } }: Call): Promise<readonly Verdict[]> => {
    const [top] = root.toArray();
    if (top === undefined) {
        return [];
    }
    const calls = commands.flatMap(({ invocations: chain, cwd }) =>
        chain
            .filter(([program]) => program === 'git')
            .flatMap((invocation) => {
                const index = firstOperand(invocation, 1);
                const { values } = operands(['git', ...invocation.slice(1, index)]);
                const moved = values.reduce((folder, [name, value]) => (name === '-C' ? placed(value, folder, home) : folder), cwd);
                const [key, ...args] = invocation.slice(index);
                return key === undefined || moved.toArray().some((folder) => !within(folder, top)) ? [] : [{ values, key, args, moved }];
            }),
    );
    const probed = calls.flatMap(({ args, moved }) => args.filter((word) => !word.startsWith('-')).flatMap((word) => placed(word, moved, home).toArray()));
    const found = new Set((await Promise.all(probed.map(async (path) => ((await exists(path)) ? [path] : [])))).flat());
    return calls.flatMap(({ values, key, args, moved }) => {
        const aliased = values.flatMap(([name, value]) => (value.startsWith('alias.') ? [refuse(`git ${name} ${value} ${ALIAS}`)] : []));
        const rule = GIT.get(key);
        const judged =
            rule === undefined
                ? []
                : [
                      ...rule(args, (word) =>
                          placed(word, moved, home)
                              .toArray()
                              .some((place) => found.has(place)),
                      ),
                      ...(moved.toArray().length === 0 ? rule(args, () => true) : []),
                  ];
        if (aliased.length > 0) {
            return aliased;
        }
        if (moved.toArray().length === 0) {
            return judged.length > 0 ? [refuse(`git ${key} ${UNPLACED}`)] : [];
        }
        return judged.map((tail) => refuse(`git ${key} ${tail}`));
    });
};

// --- [WALK]

const walks = ({ system: { root, home }, script: { commands } }: Call): readonly Verdict[] =>
    commands.flatMap(({ invocations: chain, cwd }) => {
        const last = chain.at(-1);
        if (last === undefined || chain.slice(0, -1).some(([name]) => declared(name).stdin === 'always')) {
            return [];
        }
        const { roots, base } = operands(last);
        const folder = base.reduce((place, word) => placed(word, place, home), cwd);
        return roots.flatMap((word) => {
            const wide = (place: string): boolean => place.lastIndexOf('/') === 0 || home.toArray().some((user) => within(user, place)) || root.toArray().some((top) => top !== place && within(top, place));
            return placed(word, folder, home)
                .toArray()
                .filter(wide)
                .map((place) => refuse(`${last[0]} root ${place} is too wide. Pass package folder`));
        });
    });

// --- [PROBE]

const probes = ({ script: { commands } }: Call): readonly Verdict[] =>
    commands.flatMap(({ invocations: chain }) =>
        chain.flatMap((invocation) => {
            const [program] = invocation;
            const { previews } = declared(program);
            const {
                leading: [find, replacement],
                inputs,
                options,
            } = operands(invocation);
            if (previews === undefined || find === undefined) {
                return [];
            }
            const preview = `Preview with ${program} ${previews[0]}`;
            if (find === '') {
                return [refuse(`${program} empty pattern matches everywhere. ${preview}`)];
            }
            const [line] = find.split('\n');
            return find === replacement && inputs.length > 0 && !options.some((name) => previews.includes(name)) ? [refuse(`${program} replaces ${quote(line ?? find)} with itself. ${preview}`)] : [];
        }),
    );

// --- [INPUT]

const inlined = ({ system: { home }, script: { commands } }: Call): readonly Verdict[] =>
    commands.flatMap(({ fed, invocations: chain, cwd }, index) => {
        const written = new Set(commands.slice(0, index).flatMap((writer) => writer.written.flatMap((path) => placed(path, writer.cwd, home).toArray())));
        const ran = (word: string): boolean =>
            placed(word, cwd, home)
                .toArray()
                .some((path) => written.has(path));
        return chain.flatMap((invocation) => {
            const [program] = invocation;
            const { code, shell } = declared(program);
            const { options, inputs } = operands(invocation);
            const coded = code !== undefined && (options.some((name) => code.includes(name)) || (fed && waits(invocation)));
            return [...(coded || ((code ?? shell) !== undefined && inputs.slice(0, 1).some(ran)) ? [refuse(`${program} runs inline code. Use Edit or the installed tool`)] : []), ...(!fed && waits(invocation) ? [refuse(`${program} waits on unfed stdin. Pass a file or pipe`)] : [])];
        });
    });

// --- [WAIT]

const searched = (command: Command, listed: Command | undefined, invocation: Invocation, at: number): readonly Verdict[] => {
    const [program] = invocation;
    const { options } = operands(invocation);
    const piped = program === 'grep' && command.fed && at === 0 && listed?.piping === true && listed.invocations.at(-1)?.[0] === 'ps' && options.length === 0;
    const [word] = ((program === 'pgrep' && options.includes('-f')) || piped) && command.origin !== 'file' ? positional(program, command.parts[at]?.words.slice(1) ?? []).toArray() : [];
    const text = word === undefined || word.unbounded || !LETTER.test(word.text) ? undefined : quote(`[${word.text.slice(0, 1)}]${word.text.slice(1)}`);
    return word === undefined || text === undefined ? [] : [corrected(command, edit(word.span.start, word.span.end, text), `${program} ${word.raw} matches its own shell. Use ${text}`)];
};

const selfless = ({ script: { commands } }: Call): readonly Verdict[] => commands.flatMap((command, index) => command.invocations.flatMap((invocation, at) => searched(command, commands[index - 1], invocation, at)));

const blind = ({ commands, command, invocation, at, kind }: Waited): readonly Verdict[] => {
    const { timer, endless } = declared(invocation[0]);
    const { options } = operands(invocation);
    const holding = at === command.invocations.length - 1 && (marked(timer, options) || marked(endless, options));
    const watched = commands.flatMap(({ invocations: chain }) => chain.filter(([program]) => program === WATCHER));
    const reason = watched.length === 0 ? 'Command waits blindly. Use Monitor or run_in_background' : `Command waits blindly on ${WATCHER}. ${streamed({ kind }, watched.flatMap(addressed))}`;
    return kind === 'foreground' && (holding || command.spinning) ? [refuse(reason)] : [];
};

const corrections = ({ commands, command, invocation, at }: Waited): readonly Verdict[] => {
    const [program] = invocation;
    const { options, values } = operands(invocation);
    const [first] = command.parts[at]?.words ?? [];
    const next = command.parts[at + 1]?.words[0];
    const [amount] = values.flatMap(([name, value]) => (name === '-t' ? [value] : []));
    const leaking = program === 'read' && marked(declared(program).timer, options) && command.fed && commands.some((other) => other.substituted && other !== command && other.extent.start >= command.extent.start && other.extent.end <= command.extent.end);
    return [
        ...(program === 'caffeinate' && first !== undefined && next !== undefined ? [corrected(command, edit(first.span.start, next.span.start, '', Option.some(`caffeinate dropped, ${next.text} runs directly`)), `caffeinate wraps ${next.text}. Run ${next.text} directly`)] : []),
        ...(leaking && amount !== undefined ? [corrected(command, edit(command.extent.start, command.extent.end, `sleep ${amount}`, Option.some(`read -t ${amount} became sleep ${amount}`)), `read -t leaks its input process. Use sleep ${amount}`)] : []),
    ];
};

const settled = ({ request, routes, script: { commands } }: Call): readonly { readonly command: Command; readonly verdict: Verdict }[] => {
    const own = commands.filter(({ origin }) => origin === 'call').toSorted((left, right) => left.extent.start - right.extent.start);
    const driving = own.some((command) => !command.looped && (routes.get(command)?.kind === 'rewrite' || command.invocations.some(([program]) => program === WATCHER)));
    const answered = request.kind === 'foreground' || request.kind === 'plain';
    return own.flatMap((command, at) => {
        const [only, ...more] = command.invocations;
        const [next, last] = [own[at + 1], own[at - 1]];
        if (!(driving && answered && only?.[0] === 'sleep' && more.length === 0) || command.looped || command.fed || command.piping) {
            return [];
        }
        const [start, end] = next === undefined ? [last?.extent.end ?? command.extent.start, command.extent.end] : [command.extent.start, next.extent.start];
        return [{ command, verdict: edit(start, end, '', Option.some(`sleep dropped, ${WATCHER} prints current states`)) }];
    });
};

const notified = (id: string): string => `Task ${id} notifies on completion. Continue until its notice arrives`;

const polled = ({ request, apps, source, script: { commands } }: Call): readonly string[] => {
    const looped = commands.filter(({ looped: repeating }) => repeating);
    const sleeping = looped.some(({ invocations: chain }) => chain.some(([program]) => marked(declared(program).timer, [])));
    const waited = sleeping ? [...new TextDecoder().decode(source).matchAll(TASKS)].flatMap((match) => Option.given(match.groups?.id).toArray()) : [];
    return [
        ...[...new Set(waited)].map(notified),
        ...looped.flatMap(({ invocations: chain }) =>
            chain.flatMap((invocation) => {
                const names =
                    invocation[0] === WATCHER
                        ? [addressed(invocation)]
                        : numbered(invocation)
                              .flatMap((pid) => Option.given(apps.get(pid)).toArray())
                              .map((bundle) => [bundle]);
                return names.map((name) => `Loop polls ${name.join(' ') || WATCHER}. ${streamed(request, name)}`);
            }),
        ),
    ];
};

const following = ({ invocations: chain }: Command): readonly string[] => chain.flatMap((invocation) => (invocation[0] === 'tail' && marked(declared('tail').endless, operands(invocation).options) ? operands(invocation).inputs.filter((file) => TASK.test(file)) : []));

const writing = async (system: System, request: Request, commands: readonly Command[]): Promise<ReadonlyMap<string, string>> => {
    const files = request.kind === 'monitor' ? [...new Set(commands.flatMap(following))] : [];
    const probed = await Promise.all(
        files.map(async (file) => {
            const holders = (await system.run(['lsof', '-t', file], { cwd: system.plugin, stdin: '' })).stdout.split('\n').filter((pid) => PID.test(pid));
            const table = holders.length === 0 ? '' : (await system.run(['ps', '-o', 'pid=,ppid=', '-p', holders.join(',')], { cwd: system.plugin, stdin: '' })).stdout;
            const [root] = table.split('\n').flatMap((row) => {
                const [pid, parent] = row.trim().split(SPACES);
                return pid !== undefined && PID.test(pid) && !holders.includes(parent ?? '') ? [pid] : [];
            });
            return root === undefined ? [] : [[file, root] as const];
        }),
    );
    return new Map(probed.flat());
};

const downstream = (commands: readonly Command[], index: number): readonly Command[] => {
    const next = commands[index + 1];
    return commands[index]?.piping === true && next !== undefined ? [next, ...downstream(commands, index + 1)] : [];
};

const followed = ({ request, writers, script: { commands } }: Call): readonly Verdict[] =>
    request.kind === 'monitor'
        ? commands.flatMap((command, index) => {
              const files = following(command);
              const words = command.parts[command.invocations.findIndex(([program]) => program === 'tail')]?.words ?? [];
              const [first, last] = [words[0], words.at(-1)];
              const ended = downstream(commands, index).some(({ invocations: chain }) => chain.some((invocation) => marked(declared(invocation[0]).closes, operands(invocation).options)));
              const roots = files.flatMap((file) => Option.given(writers.get(file)).toArray());
              const ids = files.flatMap((file) => Option.given(TASK.exec(file)?.groups?.id).toArray()).join(' ');
              if (files.length === 0 || first === undefined || last === undefined || ended) {
                  return [];
              }
              return roots.length < files.length ? [refuse(notified(ids))] : [edit(first.span.start, first.span.start, '{ '), edit(last.span.end, last.span.end, ` & ${roots.map((pid) => `caffeinate -w ${pid}`).join('; ')}; kill $!; }`, Option.some(`tail ends with task ${ids}`))];
          })
        : [];

const waiting = (call: Call): readonly Verdict[] => {
    const {
        request,
        script: { commands },
    } = call;
    const dropped = settled(call);
    const polling = polled(call).length > 0;
    const kept = commands.filter((command) => !(dropped.some((drop) => drop.command === command) || (polling && command.looped)));
    const sites = kept.flatMap((command) => command.invocations.map((invocation, at): Waited => ({ commands, command, invocation, at, kind: request.kind })));
    return [...dropped.map(({ verdict }) => verdict), ...sites.flatMap((site) => [...blind(site), ...corrections(site)])];
};

const detached = ({ request, source, script: { commands, jobs } }: Call): readonly Verdict[] => {
    const [job, ...more] = jobs;
    if (job === undefined || commands.some(({ invocations: chain }) => chain.some(([program]) => program === 'wait'))) {
        return [];
    }
    const trailing = more.length === 0 && job.origin === 'call' && new TextDecoder().decode(source.subarray(job.end)).trim() === '';
    if (!trailing) {
        return [refuse(`& job outlives the call. ${request.kind === 'plain' ? 'Run it as its own call' : 'Set run_in_background'}`)];
    }
    return [edit(job.start, job.end, ''), ...(request.kind === 'foreground' ? [backgrounded('&')] : [])];
};

// --- [GHIDRA]

const overridden = (name: string): string => `${name} overrides the analyzeHeadless heap. Drop it`;

const heaped = (command: Command, word: Word): readonly Verdict[] => {
    const [, name] = OWNED.exec(word.text) ?? [];
    const next = command.words.find(({ span }) => span.start > word.span.start) ?? word;
    return name === undefined ? [] : [corrected(command, edit(word.span.start, next.span.start, '', Option.some(`${name} dropped, analyzeHeadless sets the heap`)), overridden(name))];
};

const headless = (root: Option<string>, { invocations: chain, parts, origin }: Command): readonly Verdict[] =>
    chain.flatMap((invocation, at) => {
        const words = invocation[0] === HEADLESS ? (parts[at]?.words.slice(1) ?? []) : [];
        const [location] = words;
        return [...(location === undefined || PROJECTS.test(location.raw) ? [] : [refuse(`${HEADLESS} project is not $GHIDRA_PROJECT_DIR. Follow use-ghidra`)]), ...(origin === 'call' && location !== undefined ? root.toArray().flatMap((folder) => logged(folder, words)) : [])];
    });

const ghidra = ({ request, system: { root }, script: { commands, assignments } }: Call): readonly Verdict[] => {
    const runs = commands.filter(({ invocations: chain }) => chain.some((invocation) => invocation[0] === HEADLESS && !informational(HEADLESS, operands(invocation).options)));
    const prefixed = commands.flatMap((command) => command.parts.flatMap(({ assigned }) => assigned.map((word) => ({ command, word }))));
    return [...prefixed.flatMap(({ command, word }) => heaped(command, word)), ...assignments.flatMap(({ name }) => (OWNED.test(name) ? [refuse(overridden(name))] : [])), ...runs.flatMap((run) => headless(root, run)), ...(request.kind === 'foreground' && runs.length > 0 ? [backgrounded(HEADLESS)] : [])];
};

const logged = (root: string, words: readonly Word[]): readonly Verdict[] =>
    (
        [
            ['-log', 'ghidra.log'],
            ['-scriptlog', 'ghidra-script.log'],
        ] as const
    ).flatMap(([flag, file]) => {
        const log = quote(`${root}/.artifacts/logs/${file}`);
        const at = words.findIndex(({ text }) => text === flag);
        const [value, tail] = [words[at + 1], words.at(-1)];
        const notice = Option.some(`${HEADLESS} logs to .artifacts/logs`);
        if (at < 0) {
            return tail === undefined ? [] : [edit(tail.span.end, tail.span.end, ` ${flag} ${log}`, notice)];
        }
        return value === undefined || value.raw === log ? [] : [edit(value.span.start, value.span.end, log, notice)];
    });

// --- [PIPELINE]

const pipefail = ({ script: { commands } }: Call): readonly Verdict[] => {
    const own = commands.filter(({ origin }) => origin === 'call');
    const set = own.some(({ invocations: chain }) => chain.some((invocation) => invocation[0] === 'set' && operands(invocation).values.some(([, value]) => value === 'pipefail')));
    const masked = own.some(({ piping, invocations: chain }) => piping && chain.slice(-1).some(([program]) => declared(program).verdict === true));
    const closing = own.some(({ fed, invocations: chain }) => fed && chain.some((invocation) => marked(declared(invocation[0]).closes, operands(invocation).options)));
    return masked && !set && !closing ? [edit(0, 0, 'set -o pipefail; ')] : [];
};

// --- [REPEAT]

const repeated = ({ script: { commands, jobs } }: Call): readonly Verdict[] => {
    const runs = commands.filter(({ invocations: chain }) => chain.some(([program]) => program === HEADLESS));
    return [
        ...(commands.some(({ looped, invocations: chain }) => looped && chain.some((invocation) => invocation[0] === 'nx' && operands(invocation).inputs[0] !== 'show')) ? [refuse('Loop reruns nx tasks. Use one nx run-many')] : []),
        ...(runs.some(({ looped }) => looped) || (runs.length > 1 && jobs.length > 0) ? [refuse(`${HEADLESS} fans out per input. Use one Serve.java process`)] : []),
    ];
};

// --- [LAUNCH]

const streamed = ({ kind }: Pick<Request, 'kind'>, names: readonly string[]): string => {
    const shown = names.length === 0 ? ['<app>'] : names.map(quote);
    return kind === 'plain' ? `Run ${[WATCHER, 'read', ...shown].join(' ')}` : `Run ${[WATCHER, 'watch', ...shown].join(' ')} in Monitor`;
};

const addressed = (invocation: Invocation): readonly string[] => {
    const [subcommand, ...names] = operands(invocation).inputs;
    return subcommand === 'perform' || subcommand === 'set' ? [] : names;
};

const signalled = ([, ...args]: Invocation): { readonly force: Option<readonly string[]>; readonly pids: readonly string[] } => {
    const [first] = args;
    const valued = first === '-s' || first === '-n';
    const cut = valued ? 2 : Number(first !== undefined && first !== '--' && first.startsWith('-'));
    const quits: Readonly<Record<string, readonly string[]>> = { TERM: [], 15: [], KILL: ['--force'], 9: ['--force'] };
    const [signal = 'TERM'] = args.slice(Math.max(cut - 1, 0), cut).map((word) => word.toUpperCase().replace(SIGNAL, ''));
    return { force: Option.given(quits[signal]), pids: args.slice(cut).filter((word) => word !== '--') };
};

const numbered = (invocation: Invocation): readonly string[] => {
    const [program] = invocation;
    const pids = program === 'kill' ? signalled(invocation).pids : operands(invocation).values.flatMap(([name, value]) => (program === 'ps' && name === '-p' ? value.split(',') : []));
    return pids.filter((pid) => PID.test(pid));
};

const running = async (system: System, commands: readonly Command[]): Promise<ReadonlyMap<string, string>> => {
    const pids = [...new Set(commands.flatMap(({ invocations: chain }) => chain.flatMap(numbered)))];
    const probed = await Promise.all(
        pids.map(async (pid) => {
            const { stdout } = await system.run(['lsappinfo', 'info', '-only', 'bundleID,ApplicationType', pid], { cwd: system.plugin, stdin: '' });
            return DAEMON.test(stdout) ? [] : identified(stdout).toArray().map((bundle) => [pid, bundle] as const);
        }),
    );
    return new Map(probed.flat());
};

const killed = (apps: ReadonlyMap<string, string>, head: Word, invocation: Invocation): Option<Route> => {
    const { force, pids } = signalled(invocation);
    const [flags] = force.toArray();
    if (flags === undefined || pids.length === 0 || !pids.every((pid) => apps.has(pid))) {
        return Option.none;
    }
    const quits = pids.map((pid) => [WATCHER, 'quit', '--pid', pid, ...flags].join(' '));
    return Option.some({ kind: 'rewrite', program: 'kill', start: head, text: quits.length === 1 ? quits.join('') : `{ ${quits.join('; ')}; }` });
};

const upstream = (commands: readonly Command[], index: number): readonly Command[] => {
    const previous = commands[index - 1];
    return previous?.piping === true ? [previous, ...upstream(commands, index - 1)] : [];
};

const drives = ({ script: { commands } }: Call): readonly Verdict[] => {
    const ended = [...new Set(commands.flatMap(({ invocations: chain }) => chain.flatMap(([program]) => (program === 'killall' || program === 'pkill' ? [program] : []))))];
    return [
        ...ended.map((program) => refuse(`${program} ends every match. Kill one pid pgrep lists`)),
        ...commands.flatMap((command, index) => {
            const quitting = command.invocations.some((invocation) => invocation[0] === 'kill' && signalled(invocation).force.toArray().length > 0);
            const sources = [...(command.fed ? upstream(commands, index) : []), ...commands.filter((other) => other !== command && other.extent.start >= command.extent.start && other.extent.end <= command.extent.end)];
            const [finder] = sources.flatMap(({ invocations: chain }) => chain.flatMap(([program]) => (FINDERS.has(program) ? [program] : [])));
            const watched = sources.flatMap(({ invocations: chain }) => chain.filter(([program]) => program === WATCHER));
            return quitting ? [...(finder === undefined ? [] : [refuse(`${finder} feeds kill every match. Kill one pid ${finder} lists`)]), ...(watched.length === 0 ? [] : [refuse(`kill takes pids from ${WATCHER}. Use ${[WATCHER, 'quit', ...watched.flatMap(addressed).map(quote)].join(' ')}`)])] : [];
        }),
    ];
};

const observes = (call: Call): readonly Verdict[] => {
    const {
        request,
        script: { commands },
    } = call;
    const streaming = commands.flatMap(({ invocations: chain }) => chain.filter((invocation) => invocation[0] === WATCHER && operands(invocation).inputs[0] === 'watch' && !informational(WATCHER, operands(invocation).options)));
    return [...polled(call).map(refuse), ...(request.kind === 'monitor' ? [] : streaming.map((invocation) => refuse(`${WATCHER} watch never exits. ${streamed(request, addressed(invocation))}`)))];
};

const executable = async (system: System, bundle: string): Promise<Option<{ readonly executable: string; readonly identifier: string }>> => {
    const info = await printed<{ readonly CFBundleExecutable?: string; readonly CFBundleIdentifier?: string }>(system, ['plutil', '-convert', 'json', '-o', '-', `${bundle}/Contents/Info.plist`], system.plugin);
    const [{ CFBundleExecutable, CFBundleIdentifier } = {}] = info.toArray();
    return CFBundleExecutable !== undefined && CFBundleIdentifier !== undefined ? Option.some({ executable: CFBundleExecutable, identifier: CFBundleIdentifier }) : Option.none;
};

const sought = async (system: System, cwd: Option<string>, app: Word, identifying: boolean): Promise<Option<string>> => {
    if (app.text.includes('/')) {
        return placed(app.text, cwd, system.home);
    }
    const file = app.text.endsWith('.app') ? app.text : `${app.text}.app`;
    const query = identifying ? `kMDItemCFBundleIdentifier == "${app.text}"` : `kMDItemContentType == "com.apple.application-bundle" && kMDItemFSName == "${file}"c`;
    const [found, ...more] = (await system.run(['mdfind', query], { cwd: system.plugin, stdin: '' })).stdout.split('\n').filter((line) => line !== '');
    return more.length === 0 ? Option.given(found) : Option.none;
};

const unwindowed = async (system: System, cwd: Option<string>, app: Word, identifying: boolean, passed: readonly Word[]): Promise<Option<string>> => {
    const [bundle] = (await sought(system, cwd, app, identifying)).toArray();
    const [run] = bundle === undefined ? [] : (await executable(system, bundle)).toArray();
    return bundle !== undefined && run !== undefined && declared(run.executable).headless?.some((flag) => passed.some(({ text }) => text === flag)) === true ? Option.some(quote(`${bundle}/Contents/MacOS/${run.executable}`)) : Option.none;
};

const opened = async (system: System, request: Request, { cwd }: Command, program: keyof typeof LAUNCHERS, head: Word, args: readonly Word[]): Promise<Option<Route>> => {
    const { app: fixed, waits: blocking, kept }: Launcher = LAUNCHERS[program];
    const cut = args.findIndex(({ text }) => text === '--args');
    const listed = items(program, cut < 0 ? args : args.slice(0, cut));
    const passed = cut < 0 ? [] : args.slice(cut + 1);
    const valued = (...flags: readonly string[]): readonly Word[] => listed.flatMap(({ names, words: [, value] }) => (value !== undefined && names.some((name) => flags.includes(name)) ? [value] : []));
    const loose = listed.flatMap(({ names, words: [word] }) => (names.length === 0 && word !== undefined ? [word] : []));
    const switches = listed.flatMap(({ names }) => names);
    const [app] = [...valued('-a', '-b'), ...loose.filter(({ text }) => text.endsWith('.app'))];
    const subjects = [...fixed.toArray(), ...Option.given(app?.raw).toArray()];
    const target = subjects.join(' ') || '<app>';
    const documents = loose.filter((word) => word !== app);
    const [url] = [...valued('-u', '--url'), ...documents.filter(({ text }) => URL.canParse(text))];
    const [waited] = switches.filter((name) => blocking.includes(name));
    const foreign = switches.filter((name) => !(kept.includes(name) || blocking.includes(name)));
    const refusal = (reason: string): Option<Route> => Option.some({ kind: 'refuse', reason });
    if (informational(program, switches) || subjects.length + documents.length === 0) {
        return Option.none;
    }
    if (url !== undefined) {
        return refusal(WEB.test(url.text) ? `${program} ${url.raw} reaches a browser. Use playwright-cli` : `${program} ${url.raw} launches its handler. Use ${WATCHER} launch ${target}`);
    }
    if (waited !== undefined) {
        return refusal(`${program} ${waited} waits on ${target}. ${streamed(request, subjects)}`);
    }
    if (foreign.length > 0) {
        return refusal(`${program} ${foreign.join(' ')} launches ${target}. Use ${WATCHER} launch ${target}`);
    }
    const direct = app !== undefined && passed.length > 0 ? await unwindowed(system, cwd, app, valued('-b').includes(app), passed) : Option.none;
    const environment = valued('--env');
    const replacement = direct.match(
        (path) => [...environment.map(({ raw }) => raw), path, ...passed.map(({ raw }) => raw)],
        () => [WATCHER, 'launch', ...subjects, ...(switches.some((name) => name === '-n' || name === '--new') ? ['--new'] : []), ...documents.map(({ raw }) => `${DOCUMENT} ${raw}`), ...environment.map(({ raw }) => `--environment=${raw}`), ...passed.map(({ raw }) => `${ARGUMENT}=${raw}`)],
    );
    return Option.some({ kind: 'rewrite', program, start: head, text: replacement.join(' ') });
};

const statement = (body: string): Statement => {
    const path = OPENS.exec(body)?.groups?.path;
    const inner = SHELLS.exec(body)?.groups?.inner;
    if (path !== undefined) {
        return { kind: 'open', path };
    }
    if (inner !== undefined) {
        return { kind: 'shell', inner: inner.replaceAll(/\\(?<char>.)/gu, '$<char>') };
    }
    return { kind: STATEMENTS.find(([, pattern]) => pattern.test(body))?.[0] ?? 'drive' };
};

const spoken = (values: readonly (readonly [string, string])[]): readonly { readonly app: Option<string>; readonly statement: Statement }[] =>
    values
        .flatMap(([name, value]) => (name === '-e' ? value.split('\n') : []))
        .map((line) => line.trim())
        .filter((line) => line !== '')
        .reduce<{ readonly stack: readonly string[]; readonly said: readonly { readonly app: Option<string>; readonly statement: Statement }[] }>(
            ({ stack, said }, line) => {
                const groups = TELL.exec(line)?.groups ?? AIMED.exec(line)?.groups;
                if (line === 'end tell') {
                    return { stack: stack.slice(0, -1), said };
                }
                return groups?.app !== undefined && groups.body === undefined ? { stack: [...stack, groups.app], said } : { stack, said: [...said, { app: Option.given(groups?.app ?? stack.at(-1)), statement: statement(groups?.body ?? line) }] };
            },
            { stack: [], said: [] },
        ).said;

const told = (head: Word, invocation: Invocation): Option<Route> => {
    const { values, inputs } = operands(invocation);
    const refusal = (reason: string): Option<Route> => Option.some({ kind: 'refuse', reason });
    if (values.some(([name, value]) => name === '-l' && value === 'JavaScript')) {
        return values.some(([name, value]) => name === '-e' && SCRIPTED.test(value)) ? refusal(`JavaScript osascript drives apps. Use ${WATCHER} launch, quit, or perform`) : Option.none;
    }
    const said = spoken(values);
    const aimed = said.flatMap(({ app, statement: spoke }) => app.toArray().map((name) => ({ app: name, statement: spoke })));
    const apps = [...new Set(aimed.map(({ app }) => app))].map(quote);
    const [shell] = said.flatMap(({ statement: spoke }) => (spoke.kind === 'shell' ? [spoke.inner] : []));
    const documents = aimed.flatMap(({ statement: spoke }) => (spoke.kind === 'open' ? [`${DOCUMENT} ${quote(spoke.path)}`] : []));
    const verb = (
        [
            ['launch', ['launch', 'open']],
            ['quit', ['quit']],
        ] as const
    ).find(([, kinds]) => aimed.every(({ statement: spoke }) => kinds.some((kind) => kind === spoke.kind)))?.[0];
    const [refused] = (
        [
            [inputs.length > 0, `osascript ${inputs.join(' ')} is opaque. Pass -e lines`],
            [aimed.some(({ app }) => app === EVENTS), `${EVENTS} hits frontmost app. Use ${WATCHER} perform`],
            [shell !== undefined, `osascript runs a shell command. Run ${shell ?? ''} directly`],
            [aimed.some(({ statement: spoke }) => spoke.kind === 'script'), `osascript scripts ${apps.join(' ')}. Use Atelier execute_script`],
            [aimed.some(({ statement: spoke }) => spoke.kind === 'hide'), `${apps.join(' ')} instances launch hidden. Run ${WATCHER} launch ${apps.join(' ')}`],
            [aimed.length > 0 && (verb === undefined || (documents.length > 0 && apps.length > 1)), `osascript drives ${apps.join(' ')}. Use ${WATCHER} perform`],
        ] as const
    ).flatMap(([held, text]) => (held ? [text] : []));
    if (refused !== undefined) {
        return refusal(refused);
    }
    return aimed.length === 0 || verb === undefined ? Option.none : Option.some({ kind: 'rewrite', program: 'osascript', start: head, text: [WATCHER, verb, ...apps, ...documents].join(' ') });
};

const registered = (head: Word, invocation: Invocation): Option<Route> => {
    const [subcommand, ...rest] = operands(invocation).inputs;
    const [id] = rest.flatMap((word) => Option.given(IDENTIFIED.exec(word)?.groups?.id).toArray());
    if (subcommand !== 'launch') {
        return Option.none;
    }
    return Option.some(id === undefined ? { kind: 'refuse', reason: `lsappinfo launch bypasses ${WATCHER}. Use ${WATCHER} launch <app>` } : { kind: 'rewrite', program: 'lsappinfo', start: head, text: `${WATCHER} launch ${quote(id)}` });
};

const submitted = (invocation: Invocation): Option<Route> => {
    const at = invocation.indexOf('--');
    const [program, ...args] = at < 0 ? [] : invocation.slice(at + 1);
    const executed = program === undefined ? undefined : EXECUTABLE.exec(program)?.groups?.executable;
    return invocation[1] === 'submit' && executed !== undefined && declared(executed).headless?.some((flag) => args.includes(flag)) !== true ? Option.some({ kind: 'refuse', reason: `launchctl submit keeps ${executed} alive. Use ${WATCHER} launch ${quote(executed)}` }) : Option.none;
};

const bundled = async (system: System, invocation: Invocation, { assigned, words: [head, ...args] }: Command['parts'][number]): Promise<Option<Route>> => {
    const [program] = invocation;
    const groups = head === undefined ? undefined : BUNDLE.exec(head.text)?.groups;
    if (head === undefined || informational(program, operands(invocation).options) || declared(program).headless?.some((name) => invocation.includes(name)) === true) {
        return Option.none;
    }
    if (groups?.bundle === undefined) {
        return SUBSTITUTION.test(head.raw) && EXECUTABLE.test(head.text) ? Option.some({ kind: 'refuse', reason: `${program} path expands at run time. Use ${WATCHER} launch ${quote(program)}` }) : Option.none;
    }
    const [info] = (await executable(system, groups.bundle)).toArray();
    return info !== undefined && info.executable === groups.executable ? Option.some({ kind: 'rewrite', program, start: assigned[0] ?? head, text: [WATCHER, 'launch', info.identifier, ...args.map(({ raw }) => `${ARGUMENT}=${raw}`), ...assigned.map(({ raw }) => `--environment=${raw}`)].join(' ') }) : Option.none;
};

const launching = async (system: System, request: Request, apps: ReadonlyMap<string, string>, command: Command): Promise<Option<Route>> => {
    const last = command.invocations.at(-1);
    const part = command.parts.at(-1);
    const [head, ...args] = part?.words ?? [];
    if (last === undefined || part === undefined || head === undefined) {
        return Option.none;
    }
    switch (last[0]) {
        case 'open':
        case 'xed':
            return await opened(system, request, command, last[0], head, args);
        case 'osascript':
            return told(head, last);
        case 'kill':
            return killed(apps, head, last);
        case 'lsappinfo':
            return registered(head, last);
        case 'launchctl':
            return submitted(last);
        case 'screencapture':
            return Option.some({ kind: 'refuse', reason: `screencapture captures the whole display. Use ${WATCHER} capture <app>` });
        case 'qlmanage':
            return last.includes('-p') ? Option.some({ kind: 'refuse', reason: 'qlmanage -p opens a preview window. Use qlmanage -t' }) : Option.none;
        default:
            return await bundled(system, last, part);
    }
};

// --- [REWRITE]

const deadlines = async ({ request, system, script: { commands } }: Call): Promise<readonly Verdict[]> => {
    const applied = request.kind === 'foreground' && request.timeout.toArray().length === 0;
    const limited = commands.flatMap((command) =>
        command.invocations.flatMap((invocation, at) => {
            const [from] = command.parts[at]?.words ?? [];
            const [to] = [...(command.parts[at + 1]?.assigned ?? []), ...(command.parts[at + 1]?.words ?? [])];
            const [amount] = operands(invocation).leading;
            const [limit] = invocation[0] === 'timeout' && amount !== undefined ? milliseconds(amount).toArray() : [];
            return from === undefined || to === undefined || limit === undefined ? [] : [{ command, from, to, amount, limit }];
        }),
    );
    const installed =
        limited.length > 0 &&
        (await system.run(['timeout', '--version'], { cwd: system.plugin, stdin: '' }).then(
            ({ exitCode }) => exitCode === 0,
            () => false,
        ));
    return installed
        ? []
        : limited.map(({ command, from, to, amount, limit }) => {
              const capped = limit > CEILING ? 'capped at maximum' : `set to ${amount}`;
              return corrected(command, edit(from.span.start, to.span.start, '', Option.some(`timeout is not installed. Call timeout ${applied ? capped : 'stays'}`), applied ? Option.some(Math.min(limit, CEILING)) : Option.none), 'timeout is not installed. Drop it');
          });
};

const respelled = (program: string, words: readonly Word[], table: Readonly<Record<string, string>>, reason = ''): readonly Verdict[] =>
    items(program, words).flatMap(({ words: [head, ...taken], names }) => {
        const kept = names.map((name) => table[name] ?? name).filter((name) => name !== '');
        const [first] = kept;
        if (head === undefined || kept.join(' ') === names.join(' ')) {
            return [];
        }
        if (first === undefined) {
            return [edit(head.span.start - 1, (taken.at(-1) ?? head).span.end, '', Option.some(`${program} dropped ${head.text}${reason}`))];
        }
        const { joined } = option(declared(program), head.text);
        const text = first.startsWith('--') ? [first, ...joined].join('=') : `-${kept.map((name) => name.slice(1)).join('')}${joined.join('')}`;
        return [edit(head.span.start, head.span.end, text, Option.some(`${program} took ${text} for ${head.text}`))];
    });

const replaced = (invocation: Invocation, [head, ...rest]: readonly Word[]): readonly Verdict[] => {
    const { options, leading } = operands(invocation);
    const [find] = leading;
    const [word] = items('sd', rest).flatMap(({ names, words: [first] }) => (names.length === 0 && first?.text.startsWith('-') === true ? [first] : []));
    const breaks = find !== undefined && (find.includes('\n') || (!options.some((name) => name === '-F' || name === '--fixed-strings') && BREAK.test(find)));
    return [
        ...(head === undefined || !breaks || options.some((name) => name === '-A' || name === '--across') ? [] : [edit(head.span.end, head.span.end, ' -A')]),
        ...(word === undefined || rest.some(({ text }) => text === '--') ? [] : [edit(word.span.start, word.span.start, '-- ', Option.some(`sd took -- before ${word.raw}`))]),
    ];
};

const uncached = (invocation: Invocation, words: readonly Word[]): readonly Verdict[] => {
    const { inputs, values } = operands(invocation);
    const listed = values.flatMap(([name, value]) => (['-t', '--targets', '--target'].includes(name) ? value.split(',') : []));
    const targets =
        inputs[0] === 'run'
            ? inputs
                  .slice(1, 2)
                  .filter((word) => word.includes(':'))
                  .map((word) => word.slice(word.indexOf(':') + 1))
            : [...listed, ...inputs.slice(0, listed.length === 0 ? 1 : 0)];
    return targets.length > 0 && targets.every((target) => CACHED.test(target)) ? respelled('nx', words, Object.fromEntries(['--skip-nx-cache', '--skipNxCache', '--disable-nx-cache', '--disableNxCache'].map((name) => [name, ''])), `, ${targets.join(' ')} inputs hash every source`) : [];
};

const finals = (invocation: Invocation, words: readonly Word[]): readonly Verdict[] => {
    const [program] = invocation;
    const [head, ...rest] = words;
    const { options } = operands(invocation);
    const stated = (names: readonly string[]): boolean => informational(program, options) || names.some((name) => options.includes(name));
    if (head === undefined) {
        return [];
    }
    switch (program) {
        case 'fd':
            return stated(['-H', '--hidden', '--no-hidden', '-u', '--unrestricted']) ? [] : [edit(head.span.end, head.span.end, ' --hidden')];
        case 'rg':
            return stated(['-.', '--hidden', '--no-hidden', '-u', '--unrestricted', '--no-config']) ? [] : [edit(head.span.end, head.span.end, ' --hidden')];
        case 'sd':
            return informational(program, options) ? [] : replaced(invocation, words);
        case 'tree':
            return respelled(program, rest, { '-a': '', '--all': '', '--noreport': '', '--dirsfirst': '--group-directories-first' });
        case 'refurb': {
            const text = 'python -m tools.refurb';
            return [edit(head.span.start, head.span.end, text, Option.some(`refurb ran as ${text} over mypy cache`))];
        }
        case 'nx':
            return uncached(invocation, rest);
        default:
            return [];
    }
};

// --- [TARGET]

const keyed = (invocation: Invocation): ReadonlySet<string> => {
    const { options, values } = operands(invocation);
    return new Set([...options, ...values.map(([name, value]) => `${name}=${value}`)]);
};

const grouped = (graph: Readonly<Record<string, Target>>, target: string): string => {
    const parent = target.split(':').slice(0, 2).join(':');
    const named = (graph[parent]?.dependsOn ?? []).map((entry) => (typeof entry === 'string' ? entry : entry.target));
    return named.some((name) => name === target || (name.endsWith('*') && target.startsWith(name.slice(0, -1)))) ? parent : target;
};

const rebased = (word: Word, cwd: string, root: string, home: Option<string>): Option<string> => {
    const [place] =
        word.unbounded || EXPANDS.test(word.raw)
            ? []
            : placed(word.text, Option.some(cwd), home)
                  .toArray()
                  .filter((path) => within(path, root));
    return place === undefined ? Option.none : Option.some(quote(place.slice(root.length + 1) || '.'));
};

const owner = async ({ system, project }: Call, selectors: readonly string[], invocation: Invocation, cwd: string, root: string): Promise<Option<{ readonly name: string; readonly targets: Readonly<Record<string, Target>> }>> => {
    const shown = async (named: string) => {
        const graph = await printed<{ readonly targets: Readonly<Record<string, Target>> }>(system, ['mise', 'exec', '--', 'nx', 'show', 'project', named, '--json'], root);
        return Option.given(graph.toArray()[0]).map(({ targets }) => ({ name: named, targets }));
    };
    const [value, ...more] = operands(invocation).values.flatMap(([flag, argument]) => (selectors.includes(flag) ? [argument] : []));
    const [place] = placed(value ?? '.', Option.some(cwd), system.home)
        .toArray()
        .filter((path) => within(path, root));
    const located = (path: string): Option<string> => {
        const leaf = path.slice(path.lastIndexOf('/') + 1);
        const folder = leaf === 'tsconfig.json' ? path.slice(0, path.lastIndexOf('/')) : path;
        return folder === path && leaf.lastIndexOf('.') > 0 ? Option.none : Option.some(folder.slice(root.length + 1) || '.');
    };
    if (selectors.length === 0) {
        return shown(project);
    }
    if (more.length > 0) {
        return Option.none;
    }
    const [pattern] = (place !== undefined && (await system.exists(place)) ? located(place) : Option.given(value)).toArray();
    if (pattern === undefined) {
        return Option.none;
    }
    const [found, guessed] = await Promise.all([printed<readonly string[]>(system, ['mise', 'exec', '--', 'nx', 'show', 'projects', '--json', '-p', pattern], root), shown(pattern)]);
    const [name, ...others] = found.toArray().flat();
    if (name === undefined || others.length > 0) {
        return Option.none;
    }
    return guessed.toArray().some((candidate) => candidate.name === name) ? guessed : shown(name);
};

const runs = async ({ system }: Call, graph: Readonly<Record<string, Target>>, program: string, subcommand: string | undefined) => {
    const listed = Object.entries(graph).flatMap(([target, { options }]) => {
        const tool = CHECKER.exec(target)?.groups?.tool;
        const texts = [options?.command, ...(options?.commands ?? []).map((entry) => (typeof entry === 'string' ? entry : entry.command))];
        return tool === program || tool === subcommand ? texts.flatMap((text) => (text === undefined ? [] : [{ target, text }])) : [];
    });
    const parsed = await Promise.all(listed.map(async ({ target, text }) => ({ target, script: await parse(system, text) })));
    return parsed.flatMap(({ target, script }) =>
        script
            .toArray()
            .flatMap(({ commands }) => commands)
            .flatMap(({ invocations: chain }) => chain.slice(-1))
            .filter((ran) => ran[0] === program && operands(ran).inputs[0] === subcommand)
            .map((ran) => ({ target, keys: keyed(ran) })),
    );
};

const chosen = (graph: Readonly<Record<string, Target>>, found: readonly { readonly target: string; readonly keys: ReadonlySet<string> }[], wanted: readonly string[]): Option<{ readonly target: string; readonly aggregate: boolean }> => {
    const fitting = found.filter(({ keys }) => wanted.every((key) => keys.has(key)));
    const fewest = Math.min(...fitting.map(({ keys }) => keys.size));
    const [target, ...others] = [...new Set(fitting.filter(({ keys }) => keys.size === fewest).map((ran) => grouped(graph, ran.target)))];
    return target === undefined || others.length > 0 ? Option.none : Option.some({ target, aggregate: !found.some((ran) => ran.target === target) });
};

const forwarded = (program: string, words: readonly Word[], dropped: ReadonlySet<string>, subcommand: boolean) => {
    const listed = items(program, words);
    const first = listed.findIndex(({ names }) => names.length === 0);
    return listed.filter(({ names }, at) => (names.length === 0 ? !subcommand || at !== first : !names.every((name) => dropped.has(name))));
};

const checker = async (call: Call, { cwd, fed }: Command, invocation: Invocation, words: readonly Word[]): Promise<Option<{ readonly run: string; readonly passed: readonly string[] }>> => {
    const [program] = invocation;
    const selectors = declared(program).targets;
    const { home } = call.system;
    const [root] = call.system.root.toArray();
    const [folder] = cwd.toArray();
    const { options, inputs } = operands(invocation);
    if (selectors === undefined || root === undefined || folder === undefined || !within(folder, root) || fed || informational(program, options) || words.some(({ text }) => text === '--')) {
        return Option.none;
    }
    const [project] = (await owner(call, selectors, invocation, folder, root)).toArray();
    if (project === undefined) {
        return Option.none;
    }
    const { name, targets } = project;
    const [subcommand] = inputs;
    const found = await runs(call, targets, program, subcommand);
    const known = new Set(found.flatMap(({ keys }) => [...keys].flatMap((key) => key.split('=').slice(0, 1))));
    const wanted = [...keyed(invocation)].filter((key) =>
        key
            .split('=')
            .slice(0, 1)
            .some((flag) => known.has(flag)),
    );
    const target = chosen(targets, found, wanted);
    const kept = forwarded(program, words.slice(1), new Set([...known, ...selectors]), subcommand !== undefined);
    const switched = kept.some(({ names }) => names.length > 0);
    const moved = selectors.length === 0 && folder !== root;
    const passed = kept.flatMap((item) => item.words).map((word) => (moved ? rebased(word, folder, root, home) : Option.some(word.raw)));
    const [chose] = target.toArray();
    if (chose === undefined || (chose.aggregate && (wanted.length > 0 || switched)) || (moved && switched) || passed.some((argument) => argument.toArray().length === 0)) {
        return Option.none;
    }
    return Option.some({ run: `${name}:${chose.target}`, passed: [...passed.flatMap((argument) => argument.toArray()), ...(moved && passed.length === 0 ? [quote(folder.slice(root.length + 1))] : [])] });
};

// --- [DECISION]

const unrun = (detail: string): string => `function-hooks policy did not run. ${detail}`;

const routed = (command: Command, route: Route, tail: Word): readonly Verdict[] => {
    if (route.kind === 'refuse') {
        return [refuse(route.reason)];
    }
    return command.origin === 'call' ? [edit(route.start.span.start, tail.span.end, route.text, Option.some(`${route.program} became ${route.text}`))] : [refuse(`Nested ${route.program} cannot be rewritten. Run ${route.text}`)];
};

const rewritten = async (call: Call, command: Command): Promise<readonly Verdict[]> => {
    const last = command.invocations.at(-1);
    const words = command.parts.at(-1)?.words ?? [];
    const [head] = words;
    const tail = words.at(-1);
    const route = call.routes.get(command);
    if (last === undefined || head === undefined || tail === undefined) {
        return [];
    }
    if (route !== undefined) {
        return routed(command, route, tail);
    }
    const [found] = command.origin === 'call' ? (await checker(call, command, last, words)).toArray() : [];
    if (found === undefined) {
        return command.origin === 'call' ? finals(last, words) : [];
    }
    const { run, passed } = found;
    return [edit(head.span.start, tail.span.end, [`nx run ${run}`, ...(passed.length > 0 ? ['--', ...passed] : [])].join(' '), Option.some(`${last[0]} ran as nx run ${run}`))];
};

const decided = async (system: System, source: Uint8Array, script: Script, project: string, request: Request): Promise<Decision> => {
    const { commands } = script;
    const [apps, writers] = await Promise.all([running(system, commands), writing(system, request, commands)]);
    const routes = new Map((await Promise.all(commands.map(async (each) => (await launching(system, request, apps, each)).toArray().map((route) => [each, route] as const)))).flat());
    const call: Call = { system, source, script, project, request, apps, writers, routes };
    const verdicts = (await Promise.all([...[deletes, git, walks, probes, inlined, selfless, waiting, followed, detached, deadlines, ghidra, pipefail, repeated, drives, observes].map((policy) => policy(call)), ...commands.map((each) => rewritten(call, each))])).flat();
    const reasons = [...new Set(verdicts.flatMap((verdict) => (verdict.kind === 'refuse' ? [verdict.reason] : [])))];
    const edits = verdicts.flatMap((verdict) => (verdict.kind === 'edit' ? [verdict] : []));
    const backgrounds = verdicts.flatMap((verdict) => (verdict.kind === 'background' ? [verdict.notice] : []));
    const notices = [...new Set([...edits.flatMap(({ notice }) => notice.toArray()), ...backgrounds].filter((notice) => notice !== ''))];
    const timeouts = edits.flatMap(({ timeout }) => timeout.toArray());
    if (reasons.length > 0) {
        return { kind: 'deny', reason: reasons.join('. ') };
    }
    return edits.length === 0 && backgrounds.length === 0
        ? { kind: 'allow' }
        : {
              kind: 'rewrite',
              command: spliced(source, 0, edits),
              notice: notices.length === 0 ? Option.none : Option.some(notices.join('. ')),
              timeout:
                  timeouts.length === 0
                      ? Option.none
                      : Option.some(
                            Math.min(
                                CEILING,
                                timeouts.reduce((sum, value) => sum + value, 0),
                            ),
                        ),
              background: backgrounds.length > 0,
          };
};

const commandDecision = async (system: System, command: string, project: string, request: Request): Promise<Decision> => {
    const source = new TextEncoder().encode(command);
    return (await parse(system, command)).match<Decision | Promise<Decision>>(
        (script) => decided(system, source, script, project, request),
        (faults) => ({ kind: 'deny', reason: `Command not parsed. ${faults.map((value) => (value.kind === 'exited' ? [`${value.program} exited ${value.code}`, value.output].filter((part) => part !== '').join('. ') : `${value.program} failed. ${String(value.cause)}`)).join('. ')}` }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { commandDecision, type Request, type System, unrun, WATCHER, WORKTREE };

import { given, none, type Option, some, toArray } from '../composition.ts';
import { type Command, parse, printed, type Script, spliced, type Word } from './command.ts';
import { declared, firstOperand, type Invocation, informational, invocations, marked, operands, option, optional, placed, waits, within } from './invocation.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Decision = { readonly kind: 'deny'; readonly reason: string } | { readonly kind: 'allow' } | { readonly kind: 'rewrite'; readonly command: string; readonly notice: Option<string>; readonly timeout: Option<number>; readonly background: boolean };
type Request = { readonly kind: 'foreground'; readonly timeout: Option<number> } | { readonly kind: 'background' } | { readonly kind: 'monitor' };
type Verb = 'launch' | 'quit';
type Verdict = { readonly kind: 'refuse'; readonly reason: string } | { readonly kind: 'edit'; readonly start: number; readonly end: number; readonly text: string; readonly notice: Option<string>; readonly timeout: Option<number> } | { readonly kind: 'background'; readonly notice: string };

interface System {
    readonly run: (argv: Invocation, init: { readonly cwd: string; readonly stdin: string }) => Promise<{ readonly exitCode: number; readonly stdout: string; readonly stderr: string }>;
    readonly exists: (path: string) => Promise<boolean>;
    readonly read: (path: string) => Promise<string>;
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
    readonly request: Option<Request>;
}
interface Waited {
    readonly commands: readonly Command[];
    readonly command: Command;
    readonly invocation: Invocation;
    readonly at: number;
    readonly kind: Request['kind'];
}
interface Route {
    readonly program: string;
    readonly start: Word;
    readonly verb: Verb;
    readonly app: string;
    readonly flags: readonly string[];
    readonly foreign: readonly string[];
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
const CEILING = 600_000;
const BREAK = /(?<!\\)(?:\\\\)*\\n/u;
const EXPANDS = /[$`]/u;
const GLOB = /[*?[{]/u;
const NAMED = /[\p{L}\p{N}]/u;
const SAFE = /^[\w@%+=:,./-]+$/u;
const LETTER = /^\w/u;
const OWNED = /^(?<name>GHIDRA_(?:HEADLESS_)?MAXMEM)(?:=|$)/u;
const PROJECTS = /^"?\$\{?GHIDRA_PROJECT_DIR\}?"?$/u;
const DURATION = /^(?<value>\d+(?:\.\d+)?)(?<unit>[smh]?)$/u;
const BUNDLE = /^(?<bundle>\/.+?\.app)\/Contents\/MacOS\/(?<executable>[^/]+)$/u;
const TELL = /^tell (?:application|app) (?:id )?"(?<app>[^"]+)"(?: to (?<verb>\w+)(?<rest>.*))?$/u;
const AIMED = /^(?<verb>\w+) (?:application|app) (?:id )?"(?<app>[^"]+)"$/u;
const VERB = /^(?:activate|launch|run|reopen)$/u;
const SCRIPTED = /\b(?:Application|NSRunningApplication|NSWorkspace)\b/u;
const EVENTS = 'System Events';
const WATCHER = 'watch-apps';
const FINDERS: ReadonlySet<string> = new Set(['pgrep', 'pidof']);
const ARGUMENT = '--arguments';
const DOCUMENT = '--document';
const SPLASHES: readonly { readonly app: RegExp; readonly argument: string }[] = [{ app: /rhino/iu, argument: '-nosplash' }];
const CHECKER = /^(?:lint|format|typecheck):(?<tool>[^:]+)/u;
const CACHED = /^(?:check|typecheck(?::.+)?)$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [VERDICTS]

const refuse = (reason: string): Verdict => ({ kind: 'refuse', reason });

const edit = (start: number, end: number, text: string, notice: Option<string> = none, timeout: Option<number> = none): Verdict => ({ kind: 'edit', start, end, text, notice, timeout });

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

const positional = (program: string, words: readonly Word[]): Option<Word> => given(items(program, words).find(({ names }) => names.length === 0)?.words[0]);

const milliseconds = (amount: string): Option<number> => {
    const groups = DURATION.exec(amount)?.groups;
    const second = 1000;
    const value = Number(groups?.value) * second * 60 ** 'smh'.indexOf(String(groups?.unit));
    return value > 0 ? some(value) : none;
};

// --- [DELETE]

const deletes = ({ system: { root, home }, script: { commands }, project }: Call): readonly Verdict[] => {
    const removing = (invocation: Invocation): readonly Invocation[] => {
        if (invocation[0] !== 'git') {
            return ['rm', 'rimraf', 'trash', 'unlink', 'shred'].includes(invocation[0]) ? [invocation] : [];
        }
        const [key, ...args] = invocation.slice(firstOperand(invocation, 1));
        return key === 'rm' && !args.includes('--cached') ? [['rm', ...args]] : [];
    };
    const searched = (invocation: Invocation, cwd: Option<string>): { readonly roots: readonly string[]; readonly folder: Option<string>; readonly named: boolean } => {
        const [program, ...args] = invocation;
        const { roots, base, leading, values } = operands(invocation);
        const tests = ['-name', '-iname', '-path', '-ipath', '-wholename', '-iwholename', '-regex', '-iregex'];
        const patterns = program === 'find' || program === 'gfind' ? args.flatMap((word, at) => (tests.includes(word) ? args.slice(at + 1, at + 2) : [])) : [...leading, ...values.flatMap(([name, value]) => (name === '-e' || name === '--extension' ? [value] : []))];
        return { roots, folder: base.reduce((folder, word) => placed(word, folder, home), cwd), named: patterns.some((pattern) => NAMED.test(pattern)) };
    };
    const anchored = (word: string, unbounded: boolean, cwd: Option<string>): Option<string> => {
        const cut = unbounded ? word.search(GLOB) : -1;
        return placed(cut < 0 ? word : word.slice(0, word.lastIndexOf('/', cut) + 1), cwd, home);
    };
    return commands.flatMap(({ words, invocations: chain, cwd }) => {
        const last = chain.at(-1);
        if (last === undefined) {
            return [];
        }
        const [program] = last;
        const loose = new Set(words.slice(words.length - last.length).flatMap(({ text, unbounded }) => (unbounded ? [text] : [])));
        const judge =
            (name: string, covered: (word: string) => string, start: Option<string>, selective: boolean) =>
            (word: string): readonly Verdict[] => {
                if (loose.has(word) && EXPANDS.test(word)) {
                    return [refuse(`${name} ${word} expands at run time. Name literal path`)];
                }
                const place = anchored(word, loose.has(word), start);
                if (place.kind === 'none') {
                    return [refuse(`${name} ${UNPLACED}`)];
                }
                return root.kind === 'some' && !selective && within(root.value, place.value) ? [refuse(covered(word))] : [];
            };
        const { options, inner } = operands(last);
        const [remover] = removing(last);
        const [feeder] = chain.slice(0, -1).filter(([name]) => declared(name).stdin === 'always');
        if (remover !== undefined) {
            const name = program === 'git' ? 'git rm' : program;
            return feeder === undefined ? operands(remover).inputs.flatMap(judge(name, (word) => `${name} ${word} covers repository root. Run nx run ${project}:clean`, cwd, false)) : [refuse(`${name} reads paths from ${feeder[0]}. Use fd -X ${name}`)];
        }
        if (!(options.includes('-delete') || invocations(inner).some((launched) => removing(launched).length > 0))) {
            return [];
        }
        const { roots, folder, named } = searched(last, cwd);
        return roots.flatMap(judge(program, (word) => `${program} deletes under ${word} unfiltered. Add -name pattern`, folder, named));
    });
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
    if (root.kind === 'none') {
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
                return key === undefined || (moved.kind === 'some' && !within(moved.value, root.value)) ? [] : [{ values, key, args, moved }];
            }),
    );
    const probed = calls.flatMap(({ args, moved }) => args.filter((word) => !word.startsWith('-')).flatMap((word) => toArray(placed(word, moved, home))));
    const found = new Set((await Promise.all(probed.map(async (path) => ((await exists(path)) ? [path] : [])))).flat());
    return calls.flatMap(({ values, key, args, moved }) => {
        const aliased = values.flatMap(([name, value]) => (value.startsWith('alias.') ? [refuse(`git ${name} ${value} ${ALIAS}`)] : []));
        const rule = GIT.get(key);
        const judged =
            rule === undefined
                ? []
                : [
                      ...rule(args, (word) => {
                          const place = placed(word, moved, home);
                          return place.kind === 'some' && found.has(place.value);
                      }),
                      ...(moved.kind === 'none' ? rule(args, () => true) : []),
                  ];
        if (aliased.length > 0) {
            return aliased;
        }
        if (moved.kind === 'none') {
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
            const place = placed(word, folder, home);
            const wide = place.kind === 'some' && (place.value.lastIndexOf('/') === 0 || (home.kind === 'some' && within(home.value, place.value)) || (root.kind === 'some' && root.value !== place.value && within(root.value, place.value)));
            return wide ? [refuse(`${last[0]} root ${place.value} is too wide. Pass package folder`)] : [];
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
        const written = new Set(commands.slice(0, index).flatMap((writer) => writer.written.flatMap((path) => toArray(placed(path, writer.cwd, home)))));
        const ran = (word: string): boolean => toArray(placed(word, cwd, home)).some((path) => written.has(path));
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
    const [word] = ((program === 'pgrep' && options.includes('-f')) || piped) && command.origin !== 'file' ? toArray(positional(program, command.parts[at]?.words.slice(1) ?? [])) : [];
    const text = word === undefined || word.unbounded || !LETTER.test(word.text) ? undefined : quote(`[${word.text.slice(0, 1)}]${word.text.slice(1)}`);
    return word === undefined || text === undefined ? [] : [corrected(command, edit(word.span.start, word.span.end, text), `${program} ${word.raw} matches its own shell. Use ${text}`)];
};

const selfless = ({ script: { commands } }: Call): readonly Verdict[] => commands.flatMap((command, index) => command.invocations.flatMap((invocation, at) => searched(command, commands[index - 1], invocation, at)));

const blind = ({ command, invocation, at, kind }: Waited): readonly Verdict[] => {
    const { timer, endless } = declared(invocation[0]);
    const { options } = operands(invocation);
    const holding = at === command.invocations.length - 1 && (marked(timer, options) || marked(endless, options));
    return kind === 'foreground' && (holding || command.spinning) ? [refuse('Command waits blindly. Use Monitor or run_in_background')] : [];
};

const corrections = ({ commands, command, invocation, at }: Waited): readonly Verdict[] => {
    const [program] = invocation;
    const { options, values } = operands(invocation);
    const [first] = command.parts[at]?.words ?? [];
    const next = command.parts[at + 1]?.words[0];
    const [amount] = values.flatMap(([name, value]) => (name === '-t' ? [value] : []));
    const leaking = program === 'read' && marked(declared(program).timer, options) && command.fed && commands.some((other) => other.substituted && other !== command && other.extent.start >= command.extent.start && other.extent.end <= command.extent.end);
    return [
        ...(program === 'caffeinate' && first !== undefined && next !== undefined ? [corrected(command, edit(first.span.start, next.span.start, '', some(`caffeinate dropped, ${next.text} runs directly`)), `caffeinate wraps ${next.text}. Run ${next.text} directly`)] : []),
        ...(leaking && amount !== undefined ? [corrected(command, edit(command.extent.start, command.extent.end, `sleep ${amount}`, some(`read -t ${amount} became sleep ${amount}`)), `read -t leaks its input process. Use sleep ${amount}`)] : []),
    ];
};

const settled = (commands: readonly Command[], request: Option<Request>): readonly { readonly command: Command; readonly verdict: Verdict }[] => {
    const own = commands.filter(({ origin }) => origin === 'call').toSorted((left, right) => left.extent.start - right.extent.start);
    const driving = own.some((command) => !command.looped && command.invocations.some(([program]) => program === WATCHER));
    const foreground = toArray(request).some(({ kind }) => kind === 'foreground');
    return own.flatMap((command, at) => {
        const [only, ...more] = command.invocations;
        const [next, last] = [own[at + 1], own[at - 1]];
        if (!(driving && foreground && only?.[0] === 'sleep' && more.length === 0) || command.looped || command.fed || command.piping) {
            return [];
        }
        const [start, end] = next === undefined ? [last?.extent.end ?? command.extent.start, command.extent.end] : [command.extent.start, next.extent.start];
        return [{ command, verdict: edit(start, end, '', some(`sleep dropped, ${WATCHER} returns settled states`)) }];
    });
};

const waiting = ({ request, script: { commands } }: Call): readonly Verdict[] => {
    const dropped = settled(commands, request);
    const kept = commands.filter((command) => !dropped.some((drop) => drop.command === command));
    const sites = toArray(request).flatMap(({ kind }) => kept.flatMap((command) => command.invocations.map((invocation, at): Waited => ({ commands, command, invocation, at, kind }))));
    return [...dropped.map(({ verdict }) => verdict), ...sites.flatMap((site) => [...blind(site), ...corrections(site)])];
};

const detached = ({ request, source, script: { commands, jobs } }: Call): readonly Verdict[] => {
    const [job, ...more] = jobs;
    if (job === undefined || commands.some(({ invocations: chain }) => chain.some(([program]) => program === 'wait'))) {
        return [];
    }
    const trailing = more.length === 0 && job.origin === 'call' && new TextDecoder().decode(source.subarray(job.end)).trim() === '';
    if (!trailing || request.kind === 'none') {
        return [refuse('& job outlives the call. Set run_in_background')];
    }
    return [edit(job.start, job.end, ''), ...(request.value.kind === 'foreground' ? [backgrounded('&')] : [])];
};

// --- [GHIDRA]

const overridden = (name: string): string => `${name} overrides the analyzeHeadless heap. Drop it`;

const heaped = (command: Command, word: Word): readonly Verdict[] => {
    const [, name] = OWNED.exec(word.text) ?? [];
    const next = command.words.find(({ span }) => span.start > word.span.start) ?? word;
    return name === undefined ? [] : [corrected(command, edit(word.span.start, next.span.start, '', some(`${name} dropped, analyzeHeadless sets the heap`)), overridden(name))];
};

const headless = (root: Option<string>, { invocations: chain, parts, origin }: Command): readonly Verdict[] =>
    chain.flatMap((invocation, at) => {
        const words = invocation[0] === HEADLESS ? (parts[at]?.words.slice(1) ?? []) : [];
        const [location] = words;
        return [...(location === undefined || PROJECTS.test(location.raw) ? [] : [refuse(`${HEADLESS} project is not $GHIDRA_PROJECT_DIR. Follow use-ghidra`)]), ...(origin === 'call' && root.kind === 'some' && location !== undefined ? logged(root.value, words) : [])];
    });

const ghidra = ({ request, system: { root }, script: { commands, assignments } }: Call): readonly Verdict[] => {
    const runs = commands.filter(({ invocations: chain }) => chain.some((invocation) => invocation[0] === HEADLESS && !informational(HEADLESS, operands(invocation).options)));
    const prefixed = commands.flatMap((command) => command.parts.flatMap(({ assigned }) => assigned.map((word) => ({ command, word }))));
    return [
        ...prefixed.flatMap(({ command, word }) => heaped(command, word)),
        ...assignments.flatMap(({ name }) => (OWNED.test(name) ? [refuse(overridden(name))] : [])),
        ...runs.flatMap((run) => headless(root, run)),
        ...toArray(request)
            .filter(({ kind }) => kind === 'foreground' && runs.length > 0)
            .map(() => backgrounded(HEADLESS)),
    ];
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
        const notice = some(`${HEADLESS} logs to .artifacts/logs`);
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

const opened = (head: Word, args: readonly Word[]): Option<Route> => {
    const cut = args.findIndex(({ text }) => text === '--args');
    const listed = items('open', cut < 0 ? args : args.slice(0, cut));
    const valued = (flag: string): readonly Word[] => listed.flatMap(({ names, words: [, value] }) => (value !== undefined && names.includes(flag) ? [value] : []));
    const loose = listed.flatMap(({ names, words: [word] }) => (names.length === 0 && word !== undefined ? [word] : []));
    const [app] = [...valued('-a'), ...valued('-b'), ...loose.filter(({ text }) => text.endsWith('.app'))];
    const documents = loose.filter((word) => word !== app);
    const switches = listed.flatMap((item) => item.names);
    return app === undefined || documents.some(({ text }) => URL.canParse(text))
        ? none
        : some({
              program: 'open',
              start: head,
              verb: 'launch',
              app: app.raw,
              foreign: switches.filter((name) => !['-a', '-b', '-n', '-g', '-j', '-F', '--env'].includes(name)),
              flags: [...(switches.includes('-n') ? ['--new'] : []), ...documents.map(({ raw }) => `${DOCUMENT} ${raw}`), ...valued('--env').map(({ raw }) => `--environment=${raw}`), ...args.slice(cut < 0 ? args.length : cut + 1).map(({ raw }) => `${ARGUMENT}=${raw}`)],
          });
};

const acted = (word: string): Option<Verb> => {
    if (VERB.test(word)) {
        return some('launch');
    }
    return word === 'quit' ? some('quit') : none;
};

const spoken = (invocation: Invocation): { readonly apps: readonly string[]; readonly verbs: ReadonlySet<Verb>; readonly plain: boolean; readonly javascript: boolean; readonly files: readonly string[] } => {
    const { values, inputs } = operands(invocation);
    const lines = values.flatMap(([name, value]) => (name === '-e' ? value.split('\n') : [])).map((line) => line.trim());
    const read = lines.map((line) => {
        const groups = TELL.exec(line)?.groups ?? AIMED.exec(line)?.groups;
        const word = groups === undefined ? line : groups.verb;
        const verb = word === undefined ? none : acted(word);
        const bare = groups === undefined && (line === '' || line === 'end tell');
        return { app: groups?.app, verb, plain: bare || (word === undefined && groups !== undefined) || (verb.kind === 'some' && (groups?.rest ?? '') === '') };
    });
    const verbs = new Set(read.flatMap(({ verb }) => toArray(verb)));
    return {
        apps: [...new Set(read.flatMap(({ app }) => (app === undefined ? [] : [app])))],
        verbs,
        plain: verbs.size === 1 && read.every((line) => line.plain),
        javascript: values.some(([name, value]) => name === '-l' && value === 'JavaScript') && lines.some((line) => SCRIPTED.test(line)),
        files: inputs,
    };
};

const told = (head: Word, invocation: Invocation): Option<Route> => {
    const { apps, verbs, plain, files } = spoken(invocation);
    const [verb] = verbs;
    return files.length > 0 || apps.length === 0 || !plain || verb === undefined ? none : some({ program: 'osascript', start: head, verb, app: apps.map(quote).join(' '), flags: [], foreign: [] });
};

const drives = ({ script: { commands } }: Call): readonly Verdict[] => {
    const called = commands.flatMap(({ invocations: chain }) => chain);
    const ended = [...new Set(called.flatMap(([program]) => (program === 'killall' || program === 'pkill' ? [program] : [])))];
    const finding = (command: Command, index: number): boolean =>
        (command.fed && commands[index - 1]?.piping === true && commands[index - 1]?.invocations.slice(-1).some(([program]) => FINDERS.has(program)) === true) ||
        commands.some((other) => other !== command && other.extent.start >= command.extent.start && other.extent.end <= command.extent.end && other.invocations.some(([program]) => FINDERS.has(program)));
    const fed = commands.some((command, index) => command.invocations.some(([program]) => program === 'kill') && finding(command, index));
    return [
        ...called.flatMap((invocation) => {
            if (invocation[0] !== 'osascript') {
                return [];
            }
            const { apps, plain, javascript, files } = spoken(invocation);
            const driven = apps.filter((app) => app !== EVENTS);
            return [
                ...files.map((file) => refuse(`osascript ${file} is opaque. Pass -e lines`)),
                ...(apps.includes(EVENTS) ? [refuse(`${EVENTS} hits frontmost app. Use watch-apps perform`)] : []),
                ...(driven.length > 0 && !plain ? [refuse(`osascript drives ${driven.map(quote).join(' ')}. Use watch-apps perform or atelier`)] : []),
                ...(javascript ? [refuse('JavaScript osascript reaches apps. Use watch-apps or atelier')] : []),
            ];
        }),
        ...ended.map((program) => refuse(`${program} ends every match. Kill pids or watch-apps quit`)),
        ...(fed ? [refuse('pgrep feeds kill every match. Kill pids or watch-apps quit')] : []),
    ];
};

const observes = ({ request, script: { commands } }: Call): readonly Verdict[] => {
    const runs = commands.flatMap((command) => command.invocations.filter(([program]) => program === WATCHER).map((invocation) => ({ command, ...operands(invocation) })));
    const streaming = runs.some(({ inputs: [subcommand], options }) => subcommand === 'watch' && !informational(WATCHER, options));
    return [...(runs.some(({ command }) => command.looped) ? [refuse('Loop polls watch-apps. Run watch-apps watch in Monitor')] : []), ...(streaming && toArray(request).some(({ kind }) => kind !== 'monitor') ? [refuse('watch-apps watch never exits. Run it in Monitor')] : [])];
};

const bundled = async (system: System, invocation: Invocation, { assigned, words: [head, ...args] }: Command['parts'][number]): Promise<Option<Route>> => {
    const [program] = invocation;
    const groups = head === undefined ? undefined : BUNDLE.exec(head.text)?.groups;
    if (head === undefined || groups?.bundle === undefined || informational(program, operands(invocation).options) || declared(program).headless?.some((name) => invocation.includes(name)) === true) {
        return none;
    }
    const info = await printed<{ readonly CFBundleExecutable?: string; readonly CFBundleIdentifier?: string }>(system, ['plutil', '-convert', 'json', '-o', '-', `${groups.bundle}/Contents/Info.plist`], system.plugin);
    return info.kind === 'ok' && info.value.CFBundleIdentifier !== undefined && info.value.CFBundleExecutable === groups.executable
        ? some({ program, start: assigned[0] ?? head, verb: 'launch', app: info.value.CFBundleIdentifier, flags: [...args.map(({ raw }) => `${ARGUMENT}=${raw}`), ...assigned.map(({ raw }) => `--environment=${raw}`)], foreign: [] })
        : none;
};

const launching = async (system: System, { invocations: chain, parts }: Command): Promise<Option<Route>> => {
    const last = chain.at(-1);
    const part = parts.at(-1);
    const [head, ...args] = part?.words ?? [];
    if (last === undefined || part === undefined || head === undefined) {
        return none;
    }
    switch (last[0]) {
        case 'open':
            return opened(head, args);
        case 'osascript':
            return told(head, last);
        default:
            return await bundled(system, last, part);
    }
};

// --- [REWRITE]

const deadlines = async ({ request, system, script: { commands } }: Call): Promise<readonly Verdict[]> => {
    const applied = toArray(request).some((settings) => settings.kind === 'foreground' && settings.timeout.kind === 'none');
    const limited = commands.flatMap((command) =>
        command.invocations.flatMap((invocation, at) => {
            const [from] = command.parts[at]?.words ?? [];
            const [to] = [...(command.parts[at + 1]?.assigned ?? []), ...(command.parts[at + 1]?.words ?? [])];
            const [amount] = operands(invocation).leading;
            const limit = invocation[0] === 'timeout' && amount !== undefined ? milliseconds(amount) : none;
            return from === undefined || to === undefined || limit.kind === 'none' ? [] : [{ command, from, to, amount, limit: limit.value }];
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
              return corrected(command, edit(from.span.start, to.span.start, '', some(`timeout is not installed. Call timeout ${applied ? capped : 'stays'}`), applied ? some(Math.min(limit, CEILING)) : none), 'timeout is not installed. Drop it');
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
            return [edit(head.span.start - 1, (taken.at(-1) ?? head).span.end, '', some(`${program} dropped ${head.text}${reason}`))];
        }
        const { joined } = option(declared(program), head.text);
        const text = first.startsWith('--') ? [first, ...joined].join('=') : `-${kept.map((name) => name.slice(1)).join('')}${joined.join('')}`;
        return [edit(head.span.start, head.span.end, text, some(`${program} took ${text} for ${head.text}`))];
    });

const replaced = (invocation: Invocation, [head, ...rest]: readonly Word[]): readonly Verdict[] => {
    const { options, leading } = operands(invocation);
    const [find] = leading;
    const [word] = items('sd', rest).flatMap(({ names, words: [first] }) => (names.length === 0 && first?.text.startsWith('-') === true ? [first] : []));
    const breaks = find !== undefined && (find.includes('\n') || (!options.some((name) => name === '-F' || name === '--fixed-strings') && BREAK.test(find)));
    return [
        ...(head === undefined || !breaks || options.some((name) => name === '-A' || name === '--across') ? [] : [edit(head.span.end, head.span.end, ' -A')]),
        ...(word === undefined || rest.some(({ text }) => text === '--') ? [] : [edit(word.span.start, word.span.start, '-- ', some(`sd took -- before ${word.raw}`))]),
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
            return [edit(head.span.start, head.span.end, text, some(`refurb ran as ${text} over mypy cache`))];
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
    const [place] = word.unbounded || EXPANDS.test(word.raw) ? [] : toArray(placed(word.text, some(cwd), home)).filter((path) => within(path, root));
    return place === undefined ? none : some(quote(place.slice(root.length + 1) || '.'));
};

const owner = async ({ system, project }: Call, selectors: readonly string[], invocation: Invocation, cwd: string, root: string): Promise<Option<{ readonly name: string; readonly targets: Readonly<Record<string, Target>> }>> => {
    const shown = async (named: string) => {
        const graph = await printed<{ readonly targets: Readonly<Record<string, Target>> }>(system, ['mise', 'exec', '--', 'nx', 'show', 'project', named, '--json'], root);
        return graph.kind === 'ok' ? some({ name: named, targets: graph.value.targets }) : none;
    };
    const [value, ...more] = operands(invocation).values.flatMap(([flag, argument]) => (selectors.includes(flag) ? [argument] : []));
    const [place] = toArray(placed(value ?? '.', some(cwd), system.home)).filter((path) => within(path, root));
    const located = (path: string): Option<string> => {
        const leaf = path.slice(path.lastIndexOf('/') + 1);
        const folder = leaf === 'tsconfig.json' ? path.slice(0, path.lastIndexOf('/')) : path;
        return folder === path && leaf.lastIndexOf('.') > 0 ? none : some(folder.slice(root.length + 1) || '.');
    };
    if (selectors.length === 0) {
        return shown(project);
    }
    if (more.length > 0) {
        return none;
    }
    const pattern = place !== undefined && (await system.exists(place)) ? located(place) : given(value);
    if (pattern.kind === 'none') {
        return none;
    }
    const [found, guessed] = await Promise.all([printed<readonly string[]>(system, ['mise', 'exec', '--', 'nx', 'show', 'projects', '--json', '-p', pattern.value], root), shown(pattern.value)]);
    const [name, ...others] = found.kind === 'ok' ? found.value : [];
    if (name === undefined || others.length > 0) {
        return none;
    }
    return guessed.kind === 'some' && guessed.value.name === name ? guessed : shown(name);
};

const runs = async ({ system }: Call, graph: Readonly<Record<string, Target>>, program: string, subcommand: string | undefined) => {
    const listed = Object.entries(graph).flatMap(([target, { options }]) => {
        const tool = CHECKER.exec(target)?.groups?.tool;
        const texts = [options?.command, ...(options?.commands ?? []).map((entry) => (typeof entry === 'string' ? entry : entry.command))];
        return tool === program || tool === subcommand ? texts.flatMap((text) => (text === undefined ? [] : [{ target, text }])) : [];
    });
    const parsed = await Promise.all(listed.map(async ({ target, text }) => ({ target, script: await parse(system, text) })));
    return parsed.flatMap(({ target, script }) =>
        script.kind === 'ok'
            ? script.value.commands
                  .flatMap(({ invocations: chain }) => chain.slice(-1))
                  .filter((ran) => ran[0] === program && operands(ran).inputs[0] === subcommand)
                  .map((ran) => ({ target, keys: keyed(ran) }))
            : [],
    );
};

const chosen = (graph: Readonly<Record<string, Target>>, found: readonly { readonly target: string; readonly keys: ReadonlySet<string> }[], wanted: readonly string[]): Option<{ readonly target: string; readonly aggregate: boolean }> => {
    const fitting = found.filter(({ keys }) => wanted.every((key) => keys.has(key)));
    const fewest = Math.min(...fitting.map(({ keys }) => keys.size));
    const [target, ...others] = [...new Set(fitting.filter(({ keys }) => keys.size === fewest).map((ran) => grouped(graph, ran.target)))];
    return target === undefined || others.length > 0 ? none : some({ target, aggregate: !found.some((ran) => ran.target === target) });
};

const forwarded = (program: string, words: readonly Word[], dropped: ReadonlySet<string>, subcommand: boolean) => {
    const listed = items(program, words);
    const first = listed.findIndex(({ names }) => names.length === 0);
    return listed.filter(({ names }, at) => (names.length === 0 ? !subcommand || at !== first : !names.every((name) => dropped.has(name))));
};

const checker = async (call: Call, { cwd, fed }: Command, invocation: Invocation, words: readonly Word[]): Promise<Option<{ readonly run: string; readonly passed: readonly string[] }>> => {
    const [program] = invocation;
    const selectors = declared(program).targets;
    const { root, home } = call.system;
    const { options, inputs } = operands(invocation);
    if (selectors === undefined || root.kind === 'none' || cwd.kind === 'none' || !within(cwd.value, root.value) || fed || informational(program, options) || words.some(({ text }) => text === '--')) {
        return none;
    }
    const project = await owner(call, selectors, invocation, cwd.value, root.value);
    if (project.kind === 'none') {
        return none;
    }
    const { name, targets } = project.value;
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
    const moved = selectors.length === 0 && cwd.value !== root.value;
    const passed = kept.flatMap((item) => item.words).map((word) => (moved ? rebased(word, cwd.value, root.value, home) : some(word.raw)));
    if (target.kind === 'none' || (target.value.aggregate && (wanted.length > 0 || switched)) || (moved && switched) || passed.some(({ kind }) => kind === 'none')) {
        return none;
    }
    return some({ run: `${name}:${target.value.target}`, passed: [...passed.flatMap(toArray), ...(moved && passed.length === 0 ? [quote(cwd.value.slice(root.value.length + 1))] : [])] });
};

// --- [DECISION]

const unrun = (detail: string): string => `function-hooks policy did not run. ${detail}`;

const splashed = (apps: readonly string[], documented: boolean, passed: readonly string[]): readonly string[] => (documented ? [] : SPLASHES.flatMap(({ app, argument }) => (apps.some((name) => app.test(name)) && !passed.includes(argument) ? [`${ARGUMENT}=${argument}`] : [])));

const routed = (command: Command, { program, start, verb, app, flags, foreign }: Route, tail: Word): readonly Verdict[] => {
    const passed = flags.flatMap((flag) => (flag.startsWith(`${ARGUMENT}=`) ? [flag.slice(ARGUMENT.length + 1)] : []));
    const added =
        verb === 'launch'
            ? splashed(
                  [app],
                  flags.some((flag) => flag.startsWith(DOCUMENT)),
                  passed,
              )
            : [];
    return command.origin !== 'call' || foreign.length > 0
        ? [refuse(`${[program, ...foreign].join(' ')} ${verb === 'launch' ? 'launches' : 'quits'} ${app}. Use ${WATCHER} ${verb}`)]
        : [edit(start.span.start, tail.span.end, [WATCHER, verb, app, ...flags, ...added].join(' '), some(`${program} became ${WATCHER} ${verb} ${app}`))];
};

const relaunched = ({ origin }: Command, invocation: Invocation, tail: Word): readonly Verdict[] => {
    const {
        inputs: [subcommand, ...names],
        values,
    } = operands(invocation);
    const added =
        subcommand === 'launch' && origin === 'call'
            ? splashed(
                  names,
                  values.some(([name]) => name === DOCUMENT),
                  values.flatMap(([name, value]) => (name === ARGUMENT ? [value] : [])),
              )
            : [];
    return added.length === 0 ? [] : [edit(tail.span.end, tail.span.end, ` ${added.join(' ')}`, some(`${WATCHER} launch took ${added.join(' ')}`))];
};

const rewritten = async (call: Call, command: Command): Promise<readonly Verdict[]> => {
    const last = command.invocations.at(-1);
    const words = command.parts.at(-1)?.words ?? [];
    const [head] = words;
    const tail = words.at(-1);
    if (last === undefined || head === undefined || tail === undefined) {
        return [];
    }
    const launch = await launching(call.system, command);
    if (launch.kind === 'some') {
        return routed(command, launch.value, tail);
    }
    if (last[0] === WATCHER) {
        return relaunched(command, last, tail);
    }
    const found = command.origin === 'call' ? await checker(call, command, last, words) : none;
    if (found.kind === 'none') {
        return command.origin === 'call' ? finals(last, words) : [];
    }
    const { run, passed } = found.value;
    return [edit(head.span.start, tail.span.end, [`nx run ${run}`, ...(passed.length > 0 ? ['--', ...passed] : [])].join(' '), some(`${last[0]} ran as nx run ${run}`))];
};

const commandDecision = async (system: System, command: string, project: string, request: Option<Request> = none): Promise<Decision> => {
    const source = new TextEncoder().encode(command);
    const parsed = await parse(system, command);
    if (parsed.kind === 'fault') {
        return { kind: 'deny', reason: `Command not parsed. ${parsed.faults.map((value) => (value.kind === 'exited' ? [`${value.program} exited ${value.code}`, value.output].filter((part) => part !== '').join('. ') : `${value.program} failed. ${String(value.cause)}`)).join('. ')}` };
    }
    const call = { system, source, script: parsed.value, project, request };
    const verdicts = (await Promise.all([...[deletes, git, walks, probes, inlined, selfless, waiting, detached, deadlines, ghidra, pipefail, repeated, drives, observes].map((policy) => policy(call)), ...parsed.value.commands.map((each) => rewritten(call, each))])).flat();
    const reasons = [...new Set(verdicts.flatMap((verdict) => (verdict.kind === 'refuse' ? [verdict.reason] : [])))];
    const edits = verdicts.flatMap((verdict) => (verdict.kind === 'edit' ? [verdict] : []));
    const backgrounds = verdicts.flatMap((verdict) => (verdict.kind === 'background' ? [verdict.notice] : []));
    const notices = [...new Set([...edits.flatMap(({ notice }) => toArray(notice)), ...backgrounds].filter((notice) => notice !== ''))];
    const timeouts = edits.flatMap(({ timeout }) => toArray(timeout));
    if (reasons.length > 0) {
        return { kind: 'deny', reason: reasons.join('. ') };
    }
    return edits.length === 0 && backgrounds.length === 0
        ? { kind: 'allow' }
        : {
              kind: 'rewrite',
              command: spliced(source, 0, edits),
              notice: notices.length === 0 ? none : some(notices.join('. ')),
              timeout:
                  timeouts.length === 0
                      ? none
                      : some(
                            Math.min(
                                CEILING,
                                timeouts.reduce((sum, value) => sum + value, 0),
                            ),
                        ),
              background: backgrounds.length > 0,
          };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { commandDecision, type Request, type System, unrun, WORKTREE };

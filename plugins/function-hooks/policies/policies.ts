import { all, type Fault, map, none, type Option, ok, type Result, some, toArray } from '../composition.ts';
import { parse, printed, type Script } from './command.ts';
import { declared, firstOperand, type Invocation, informational, invocations, OPTION, operands, option, placed, waits, within } from './invocation.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Tool = 'Bash' | 'Monitor';
type Command = Script['commands'][number];
type Word = Command['words'][number];
type Decision =
    | { readonly kind: 'deny'; readonly reason: string }
    | { readonly kind: 'allow' }
    | { readonly kind: 'rewrite'; readonly command: string; readonly notice: Option<string>; readonly timeout: Option<number>; readonly background: boolean };
type Launch = { readonly kind: 'deny'; readonly reason: string } | { readonly kind: 'launch'; readonly names: readonly string[]; readonly flags: readonly string[]; readonly environment: boolean };
type Targets = Readonly<Record<string, { readonly options?: { readonly command?: string; readonly commands?: readonly (string | { readonly command: string })[] }; readonly dependsOn?: readonly (string | { readonly target: string })[] }>>;

interface System {
    readonly run: (argv: Invocation, init: { readonly cwd: string; readonly stdin: string }) => Promise<{ readonly exitCode: number; readonly stdout: string; readonly stderr: string }>;
    readonly exists: (path: string) => Promise<boolean>;
    readonly plugin: string;
    readonly root: Option<string>;
    readonly cwd: string;
    readonly home: Option<string>;
}
interface Request {
    readonly tool: Tool;
    readonly background: boolean;
    readonly timeout: Option<number>;
}
interface Call {
    readonly system: System;
    readonly script: Script;
    readonly project: string;
    readonly request: Request;
}
interface Change {
    readonly start: number;
    readonly end: number;
    readonly text: string;
    readonly notice: Option<string>;
    readonly timeout: Option<number>;
}
interface Segment {
    readonly invocation: Invocation;
    readonly words: readonly Word[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORKTREE = 'creates second checkout. Work in main checkout';
const BRANCH = 'creates branch. Work on current branch';
const UNPLACED = 'runs after unresolved directory change. Run cd to named directory first';
const ALIAS = 'defines alias that can hide refused subcommand';
const OVERWRITES = 'overwrites working-tree files. Edit files';
const INLINE = 'runs inline program. Edit with Edit or sd, run installed tools by name';
const BREAK = /(?<!\\)(?:\\\\)*\\n/u;
const EXPANDS = /[$`]/u;
const GLOB = /[*?[{]/u;
const NAMED = /[\p{L}\p{N}]/u;
const SAFE = /^[\w@%+=:,./-]+$/u;
const DURATION = /^(?<amount>\d+(?:\.\d+)?)(?<unit>[smhd]?)$/u;
const BUNDLE = /\/(?<name>[^/]+)\.app\/Contents\/MacOS\/[^/]+$/u;
const TELL = /^tell (?:application|app) (?:id )?"(?<app>[^"]+)"(?: to (?<verb>activate|launch|run|reopen))?$/u;
const VERB = /^(?:activate|launch|run|reopen)$/u;
const CHECKER = /^(?:lint|format|typecheck):(?<tool>[^:]+)/u;
const CACHED = /^(?:check|typecheck(?::.+)?)$/u;
const UNITS: Readonly<Record<string, number>> = { '': 1000, s: 1000, m: 60_000, h: 3_600_000, d: 86_400_000 };
const LOGS: Readonly<Record<string, string>> = { '-log': 'ghidra.log', '-scriptlog': 'ghidra-script.log' };
const EZA: Readonly<Record<string, string>> = { '-a': '', '--all': '', '--noreport': '', '--dirsfirst': '--group-directories-first' };
const UNCACHED = ['--skip-nx-cache', '--skipNxCache', '--disable-nx-cache', '--disableNxCache'];
const OPENS = ['-a', '-b', '-n', '-g', '-j', '-F', '--env'];
const FOREGROUND: Request = { tool: 'Bash', background: false, timeout: none };

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [WORDS]

const quote = (word: string): string => (SAFE.test(word) ? word : `'${word.replaceAll("'", String.raw`'\''`)}'`);

const assigned = ({ words, invocations: chain }: Command): readonly Word[] => words.slice(0, words.length - chain.reduce((sum, { length }) => sum + length, 0));

const segments = (command: Command): readonly Segment[] =>
    command.invocations.reduce<{ readonly at: number; readonly parts: readonly Segment[] }>(({ at, parts }, invocation) => ({ at: at + invocation.length, parts: [...parts, { invocation, words: command.words.slice(at, at + invocation.length) }] }), { at: assigned(command).length, parts: [] }).parts;

// --- [DELETE]

const deletes = ({ system: { root, home }, script: { commands }, project }: Call): readonly string[] => {
    const finder = 'find -exec or fd -X';
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
            (name: string, scope: (word: string) => string, start: Option<string>, selective: boolean) =>
            (word: string): readonly string[] => {
                if (loose.has(word) && EXPANDS.test(word)) {
                    return [`${name} deletes ${word} expanded at run time. Name path or use ${finder}`];
                }
                const place = anchored(word, loose.has(word), start);
                if (place.kind === 'none') {
                    return [`${name} ${UNPLACED}`];
                }
                return root.kind === 'some' && !selective && within(root.value, place.value) ? [`${name} deletes ${scope(word)} at or above repository root. Name directory beneath root or run nx run ${project}:clean`] : [];
            };
        const { options, inner } = operands(last);
        const [remover] = removing(last);
        const [feeder] = chain.slice(0, -1).filter(([name]) => declared(name).stdin === 'always');
        if (remover !== undefined) {
            const name = program === 'git' ? 'git rm' : program;
            return feeder === undefined ? operands(remover).inputs.flatMap(judge(name, String, cwd, false)) : [`${name} deletes paths ${feeder[0]} reads from standard input. Use ${finder}`];
        }
        if (!(options.includes('-delete') || invocations(inner).some((launched) => removing(launched).length > 0))) {
            return [];
        }
        const { roots, folder, named } = searched(last, cwd);
        return roots.flatMap(judge(program, (word) => `under ${word} with no name pattern`, folder, named));
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
            const deleting = flagged(args, 'deletes or force-moves branch', ['-d', '-D', '-M', '--delete'], ['--force']);
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
            return [...forced, ...creating, ...(args.includes('--detach') || !paths ? [] : [`${args.join(' ')} ${OVERWRITES}`])].slice(0, 1);
        },
        clean: () => ['deletes untracked files. Remove named files with rm'],
        config: (args) => args.flatMap((word, at) => (word.startsWith('alias.') && args.slice(at + 1).some((value) => !value.startsWith('-')) ? [`${word} ${ALIAS}`] : [])).slice(0, 1),
        push: (args) => flagged(args, 'rewrites or deletes remote history', ['-f', '-d', '--delete', '--mirror', '--prune'], ['--force', '+', ':']),
        rebase: () => ['rewrites commits other agents can hold'],
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
        stash: ([first]) => (first === 'list' || first === 'show' ? [] : ['hides uncommitted work other agents depend on. Commit to branch']),
        switch: (args) => {
            const forced = flagged(args, 'discards local changes', ['-f', '-C', '--discard-changes'], ['--force']);
            return [...forced, ...flagged(args, BRANCH, ['-c', '--create', '--orphan'], [])].slice(0, 1);
        },
        worktree: ([first]) => (first === 'list' ? [] : [WORKTREE]),
    }),
);

const git = async ({ system: { exists, root, home }, script: { commands } }: Call): Promise<readonly string[]> => {
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
        const aliased = values.flatMap(([name, value]) => (value.startsWith('alias.') ? [`git ${name} ${value} ${ALIAS}`] : []));
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
            return judged.length > 0 ? [`git ${key} ${UNPLACED}`] : [];
        }
        return judged.map((tail) => `git ${key} ${tail}`);
    });
};

// --- [WALK]

const walks = ({ system: { root, home }, script: { commands } }: Call): readonly string[] =>
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
            return wide ? [`${last[0]} root ${place.value} is wider than the work. Narrow root to repository or owning package folder`] : [];
        });
    });

// --- [PROBE]

const probes = ({ script: { commands } }: Call): readonly string[] =>
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
            const preview = `Search with rg or preview with ${program} ${previews[0]}`;
            if (find === '') {
                return [`${program} empty find matches between every character. ${preview}`];
            }
            const [line] = find.split('\n');
            return find === replacement && inputs.length > 0 && !options.some((name) => previews.includes(name)) ? [`${program} replaces ${quote(line ?? find)} with identical text in ${inputs.join(' ')}. ${preview}`] : [];
        }),
    );

// --- [BACKGROUND]

const detached = ({ script: { commands, background } }: Call): readonly string[] => (background && !commands.some(({ invocations: chain }) => chain.some(([program]) => program === 'wait')) ? ['Job started with & outlives call. Add wait or set run_in_background'] : []);

// --- [STDIN]

const stdin = ({ script: { commands } }: Call): readonly string[] => commands.flatMap(({ fed, words, invocations: chain }) => (!fed && chain.some(waits) ? [`${words.map(({ text }) => text).join(' ')} waits on unfed standard input. Pass file or pipe input`] : []));

// --- [INLINE]

const inline = ({ script: { commands } }: Call): readonly string[] =>
    commands.flatMap(({ fed, invocations: chain }) =>
        chain.flatMap((invocation) => {
            const [program] = invocation;
            const { code } = declared(program);
            return code !== undefined && (operands(invocation).options.some((name) => code.includes(name)) || (fed && waits(invocation))) ? [`${program} ${INLINE}`] : [];
        }),
    );

const scripts = ({ system: { home }, script: { commands } }: Call): readonly string[] =>
    commands.flatMap(({ invocations: chain, cwd }, index) => {
        const written = new Set(commands.slice(0, index).flatMap((writer) => writer.written.flatMap((path) => toArray(placed(path, writer.cwd, home)))));
        return chain.flatMap((invocation) => (declared(invocation[0]).code !== undefined && operands(invocation).inputs.some((word) => toArray(placed(word, cwd, home)).some((path) => written.has(path))) ? [`${invocation[0]} ${INLINE}`] : []));
    });

// --- [DECISION]

const unrun = (detail: string): string => `function-hooks policy did not run. ${detail}`;

const without = (program: string, words: Script['commands'][number]['words'], dropped: readonly string[]) => {
    const stop = words.findIndex(({ text }) => text === '--');
    return words.slice(0, stop < 0 ? words.length : stop).flatMap(({ text, span }) => {
        const { names, joined } = text.startsWith('-') ? option(declared(program), text) : { names: [], joined: [] };
        const kept = names.filter((name) => !dropped.includes(name));
        if (kept.length === names.length) {
            return [];
        }
        return kept.length === 0 ? [{ start: span.start - 1, end: span.end, text: '', notice: none }] : [{ start: span.start, end: span.end, text: `-${kept.map((name) => name.slice(1)).join('')}${joined.join('')}`, notice: none }];
    });
};

const commandDecision = async (system: System, command: string, project: string): Promise<Decision> => {
    const parsed = await parse(system, command);
    if (parsed.kind === 'fault') {
        return { kind: 'deny', reason: `Command not parsed. ${parsed.faults.map((value) => (value.kind === 'exited' ? [`${value.program} exited ${value.code}`, value.output].filter((part) => part !== '').join('. ') : `${value.program} failed. ${String(value.cause)}`)).join('. ')}` };
    }
    const call = { system, script: parsed.value, project };
    const reasons = new Set((await Promise.all([deletes, git, walks, probes, stdin, inline, detached].map((policy) => policy(call)))).flat());
    if (reasons.size > 0) {
        return { kind: 'deny', reason: [...reasons].join('. ') };
    }
    const changes = parsed.value.commands
        .filter(({ nested }) => !nested)
        .flatMap(({ words, invocations: chain }) => chain.slice(-1).flatMap((last) => words.slice(words.length - last.length, words.length - last.length + 1).map(({ span }) => ({ last, span, rest: words.slice(words.length - last.length + 1) }))))
        .flatMap(({ last, span: { start, end }, rest }) => {
            const [program] = last;
            const { options, leading } = operands(last);
            const given = (names: readonly string[]): boolean => informational(program, options) || names.some((name) => options.includes(name));
            switch (program) {
                case 'fd':
                    return given(['-H', '--hidden', '--no-hidden', '-u', '--unrestricted']) ? [] : [{ start: end, end, text: ' --hidden', notice: none }];
                case 'rg':
                    return given(['-.', '--hidden', '--no-hidden', '-u', '--unrestricted', '--no-config']) ? [] : [{ start: end, end, text: ' --hidden', notice: none }];
                case 'sd': {
                    const [find] = leading;
                    const breaks = find !== undefined && (find.includes('\n') || (!given(['-F', '--fixed-strings']) && BREAK.test(find)));
                    return breaks && !given(['-A', '--across']) ? [{ start: end, end, text: ' -A', notice: none }] : [];
                }
                case 'tree':
                    return without(program, rest, ['-a', '--all']);
                case 'refurb': {
                    const text = 'python -m tools.refurb';
                    return [{ start, end, text, notice: some(`refurb ran as ${text} over mypy cache`) }];
                }
                default:
                    return [];
            }
        });
    if (changes.length === 0) {
        return { kind: 'allow' };
    }
    const bytes = new TextEncoder().encode(command);
    const decoder = new TextDecoder();
    const sorted = changes.toSorted((left, right) => left.start - right.start);
    const notices = [...new Set(changes.flatMap(({ notice }) => (notice.kind === 'some' ? [notice.value] : [])))];
    return {
        kind: 'rewrite',
        command: [...sorted.map(({ start, text }, index) => `${decoder.decode(bytes.subarray(sorted[index - 1]?.end, start))}${text}`), decoder.decode(bytes.subarray(sorted.at(-1)?.end))].join(''),
        notice: notices.length === 0 ? none : some(notices.join('. ')),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { commandDecision, type System, unrun, WORKTREE };

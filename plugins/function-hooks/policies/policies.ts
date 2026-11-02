import { all, bind, type Fault, map, none, type Option, ok, type Result, some, toArray } from '../composition.ts';
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
const APP = /(?<name>[^/]+?)(?:\.app)?\/*$/u;
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

// --- [POLL]

const polls = ({ request: { tool }, script: { commands } }: Call): readonly string[] =>
    tool === 'Monitor'
        ? []
        : commands.flatMap(({ looped, invocations: chain }) => {
              const last = chain.at(-1);
              if (looped && chain.some(([program]) => program === 'sleep')) {
                  return ['Loop polls with sleep. Run until-loop in Monitor'];
              }
              return last?.[0] === 'caffeinate' && operands(last).options.includes('-t') ? ['caffeinate -t holds call open. Wait in Monitor or on task notification'] : [];
          });

// --- [LAUNCH]

const launched = (command: Command): Option<Launch> => {
    const last = command.invocations.at(-1);
    const [head] = segments(command).at(-1)?.words ?? [];
    if (last === undefined || head === undefined) {
        return none;
    }
    const [program, ...args] = last;
    const outside = (name: string, form: string): Launch => ({ kind: 'deny', reason: `${form} launches ${name} outside watch-apps. Run watch-apps launch ${quote(name)} --hides` });
    const windowless = (app: string, given: readonly string[]): boolean => informational(app, operands([app, ...given]).options) || declared(app).headless?.some((name) => given.includes(name)) === true;
    const bundle = BUNDLE.exec(head.text)?.groups?.name;
    if (bundle !== undefined) {
        return windowless(program, args) ? none : some({ kind: 'launch', names: [bundle], flags: args.map((arg) => `--arguments=${quote(arg)}`), environment: true });
    }
    switch (program) {
        case 'open': {
            const cut = args.indexOf('--args');
            const given = cut < 0 ? [] : args.slice(cut + 1);
            const { options, values, inputs } = operands(['open', ...(cut < 0 ? args : args.slice(0, cut))]);
            const [app, ...more] = values.flatMap(([name, value]) => (name === '-a' ? [APP.exec(value)?.groups?.name ?? value] : name === '-b' ? [value] : []));
            if (app === undefined || windowless(app, given)) {
                return none;
            }
            const foreign = options.filter((name) => !OPENS.includes(name));
            return some(
                foreign.length > 0 || more.length > 0
                    ? outside(app, ['open', ...foreign].join(' '))
                    : {
                          kind: 'launch',
                          names: [app],
                          flags: [...(options.includes('-n') ? ['--new'] : []), ...inputs.map((path) => `--document=${quote(path)}`), ...given.map((arg) => `--arguments=${quote(arg)}`), ...values.flatMap(([name, value]) => (name === '--env' ? [`--environment=${quote(value)}`] : []))],
                          environment: false,
                      },
            );
        }
        case 'osascript': {
            const { values, inputs } = operands(last);
            if (inputs.length > 0 || values.some(([name, value]) => name === '-l' && value !== 'AppleScript')) {
                return none;
            }
            const { launches, other } = values
                .flatMap(([name, value]) => (name === '-e' ? value.split('\n') : []))
                .map((line) => line.trim())
                .filter((line) => line !== '')
                .reduce<{ readonly blocks: readonly string[]; readonly launches: readonly string[]; readonly other: boolean }>(
                    (state, line) => {
                        const told = TELL.exec(line)?.groups;
                        const [block] = state.blocks;
                        if (told?.app !== undefined) {
                            return told.verb === undefined ? { ...state, blocks: [told.app, ...state.blocks] } : { ...state, launches: [...state.launches, told.app] };
                        }
                        if (line === 'end tell') {
                            return { ...state, blocks: state.blocks.slice(1) };
                        }
                        return VERB.test(line) && block !== undefined ? { ...state, launches: [...state.launches, block] } : { ...state, other: true };
                    },
                    { blocks: [], launches: [], other: false },
                );
            const names = [...new Set(launches)];
            const [first] = names;
            if (first === undefined || names.some((name) => EXPANDS.test(name))) {
                return none;
            }
            return some(other ? outside(first, program) : { kind: 'launch', names, flags: [], environment: false });
        }
        default:
            return none;
    }
};

const launches = ({ script: { commands } }: Call): readonly string[] =>
    commands.flatMap((command) =>
        toArray(launched(command)).flatMap((launch) => {
            if (launch.kind === 'deny') {
                return [launch.reason];
            }
            const [name = ''] = launch.names;
            return command.nested || [...launch.names, ...launch.flags].some((word) => EXPANDS.test(word)) ? [`${command.invocations.at(-1)?.[0]} launches ${name} outside watch-apps. Run watch-apps launch ${quote(name)} --hides`] : [];
        }),
    );

// --- [REWRITE]

const edit = (start: number, end: number, text: string, notice: Option<string> = none, timeout: Option<number> = none): Change => ({ start, end, text, notice, timeout });

const timed = (parts: readonly Segment[]): readonly Change[] =>
    parts.flatMap(({ invocation, words: [first] }, index) => {
        const next = parts[index + 1]?.words[0];
        const [amount = ''] = operands(invocation).leading;
        const groups = invocation[0] === 'timeout' ? DURATION.exec(amount)?.groups : undefined;
        const scale = UNITS[groups?.unit ?? ''];
        return first === undefined || next === undefined || groups?.amount === undefined || scale === undefined ? [] : [edit(first.span.start, next.span.start, '', some(`timeout ${amount} became call timeout`), some(Number(groups.amount) * scale))];
    });

const without = (program: string, words: readonly Word[], table: Readonly<Record<string, string>>): readonly Change[] => {
    const stop = words.findIndex(({ text }) => text === '--');
    return words.slice(0, stop < 0 ? words.length : stop).flatMap(({ text, span }) => {
        const { names, joined } = OPTION.test(text) ? option(declared(program), text) : { names: [], joined: [] };
        const kept = names.flatMap((name) => {
            const replacement = table[name] ?? name;
            return replacement === '' ? [] : [replacement];
        });
        const [first] = kept;
        if (kept.join(' ') === names.join(' ')) {
            return [];
        }
        if (first === undefined) {
            return [edit(span.start - 1, span.end, '', some(`${program} dropped ${text}`))];
        }
        const replaced = first.startsWith('--') ? `${first}${joined.map((value) => `=${value}`).join('')}` : `-${kept.map((name) => name.slice(1)).join('')}${joined.join('')}`;
        return [edit(span.start, span.end, replaced, some(`${program} took ${replaced} for ${text}`))];
    });
};

const finals = ({ system: { root, home } }: Call, { cwd }: Command, { invocation, words }: Segment): readonly Change[] => {
    const [program] = invocation;
    const [head, ...rest] = words;
    const end = words.at(-1)?.span.end;
    if (head === undefined || end === undefined) {
        return [];
    }
    const { options, leading, inputs, values } = operands(invocation);
    const given = (names: readonly string[]): boolean => informational(program, options) || names.some((name) => options.includes(name));
    switch (program) {
        case 'fd':
            return given(['-H', '--hidden', '--no-hidden', '-u', '--unrestricted']) ? [] : [edit(head.span.end, head.span.end, ' --hidden')];
        case 'rg':
            return given(['-.', '--hidden', '--no-hidden', '-u', '--unrestricted', '--no-config']) ? [] : [edit(head.span.end, head.span.end, ' --hidden')];
        case 'sd': {
            const [find] = leading;
            const breaks = find !== undefined && (find.includes('\n') || (!given(['-F', '--fixed-strings']) && BREAK.test(find)));
            const dashed = rest.some(({ text }) => text === '--') ? undefined : rest.find(({ text }) => text.startsWith('-') && leading.includes(text));
            return [
                ...(breaks && !given(['-A', '--across']) ? [edit(head.span.end, head.span.end, ' -A')] : []),
                ...(dashed === undefined ? [] : [edit(dashed.span.start, dashed.span.start, '-- ', some(`sd took -- before ${quote(dashed.text)}`))]),
            ];
        }
        case 'tree':
            return without(program, rest, EZA);
        case 'refurb': {
            const text = 'python -m tools.refurb';
            return [edit(head.span.start, head.span.end, text, some(`refurb ran as ${text} over mypy cache`))];
        }
        case 'watch-apps': {
            const subcommand = words[firstOperand(invocation, 1)];
            return subcommand?.text === 'launch' && !options.includes('--hides') ? [edit(subcommand.span.end, subcommand.span.end, ' --hides', some('watch-apps launch took --hides'))] : [];
        }
        case 'nx': {
            const stop = rest.findIndex(({ text }) => text === '--');
            const before = rest.slice(0, stop < 0 ? rest.length : stop);
            const [subcommand, first = ''] = inputs;
            const targets = subcommand === 'run' ? (first.includes(':') ? [first.slice(first.indexOf(':') + 1)] : []) : subcommand === 'run-many' || subcommand === 'affected' ? values.flatMap(([name, value]) => (['-t', '--targets', '--target'].includes(name) ? value.split(',') : [])) : toArray(subcommand === undefined ? none : some(subcommand));
            const dropped = before.filter(({ text }) => UNCACHED.includes(text.split('=')[0] ?? text));
            return targets.length > 0 && targets.every((target) => CACHED.test(target)) ? dropped.map(({ text, span }) => edit(span.start - 1, span.end, '', some(`nx dropped ${text}, ${targets.join(' ')} inputs hash every source`))) : [];
        }
        case 'analyzeHeadless':
            return root.kind === 'none'
                ? []
                : Object.entries(LOGS).flatMap(([flag, file]) => {
                      const log = `${root.value}/.artifacts/logs/${file}`;
                      const at = rest.findIndex(({ text }) => text === flag);
                      const value = rest[at + 1];
                      const notice = some('analyzeHeadless logs to .artifacts/logs');
                      if (at < 0) {
                          return [edit(end, end, ` ${flag} ${quote(log)}`, notice)];
                      }
                      return value === undefined || toArray(placed(value.text, cwd, home)).includes(log) ? [] : [edit(value.span.start, value.span.end, quote(log), notice)];
                  });
        default:
            return [];
    }
};

// --- [TARGET]

const forwarded = (program: string, words: readonly Word[], dropped: ReadonlySet<string>, subcommand: boolean): readonly { readonly word: Word; readonly operand: boolean }[] =>
    words.reduce<{ readonly skip: number; readonly carry: number; readonly subcommand: boolean; readonly kept: readonly { readonly word: Word; readonly operand: boolean }[] }>(
        (state, word) => {
            if (state.skip > 0) {
                return { ...state, skip: state.skip - 1 };
            }
            if (state.carry > 0) {
                return { ...state, carry: state.carry - 1, kept: [...state.kept, { word, operand: false }] };
            }
            if (OPTION.test(word.text)) {
                const { names, taken } = option(declared(program), word.text);
                return names.every((name) => dropped.has(name)) ? { ...state, skip: taken } : { ...state, carry: taken, kept: [...state.kept, { word, operand: false }] };
            }
            return state.subcommand ? { ...state, subcommand: false } : { ...state, kept: [...state.kept, { word, operand: true }] };
        },
        { skip: 0, carry: 0, subcommand, kept: [] },
    ).kept;

const checker = async ({ system }: Call, { cwd }: Command, { invocation, words }: Segment, raw: (span: Word['span']) => string): Promise<Result<Option<string>>> => {
    const [program] = invocation;
    const row = declared(program).targets;
    const { options, inputs, values } = operands(invocation);
    const { root, home } = system;
    if (row === undefined || root.kind === 'none' || cwd.kind === 'none' || !within(cwd.value, root.value) || informational(program, options) || words.some(({ text }) => text === '--')) {
        return ok(none);
    }
    const relative = (path: string): string => (path === root.value ? '.' : path.slice(root.value.length + 1));
    const owned = (): Option<string> => {
        const [value, ...more] = row.project === 'owner' ? values.flatMap(([name, given]) => (row.selects.includes(name) ? [given] : [])) : [];
        if (row.project === 'root') {
            return some('.');
        }
        if (more.length > 0 || (row.names === 'path' && inputs.length > 0)) {
            return none;
        }
        if (row.names === 'project') {
            return some(value ?? relative(cwd.value));
        }
        const [place] = toArray(placed(value ?? '.', cwd, home)).filter((path) => within(path, root.value));
        const leaf = place?.slice(place.lastIndexOf('/') + 1) ?? '';
        if (place === undefined || (leaf.includes('.') && leaf !== 'tsconfig.json')) {
            return none;
        }
        return some(relative(leaf.includes('.') ? place.slice(0, place.lastIndexOf('/')) : place));
    };
    const pattern = owned();
    if (pattern.kind === 'none') {
        return ok(none);
    }
    const nx = <A>(...args: readonly string[]) => printed<A>(system, ['mise', 'exec', '--', 'nx', ...args], root.value);
    return bind(await nx<readonly string[]>('show', 'projects', '--json', '-p', pattern.value), async ([name, ...others]) => {
        if (name === undefined || others.length > 0) {
            return ok(none);
        }
        return bind(await nx<{ readonly targets: Targets }>('show', 'project', name, '--json'), async ({ targets }) => {
            const [subcommand] = inputs;
            const listed = Object.entries(targets).flatMap(([target, { options: given }]) => {
                const tool = CHECKER.exec(target)?.groups?.tool;
                const texts = [given?.command, ...(given?.commands ?? []).map((entry) => (typeof entry === 'string' ? entry : entry.command))].filter((text) => text !== undefined);
                return tool === program || tool === subcommand ? texts.map((text) => ({ target, text })) : [];
            });
            const parsed = all(await Promise.all(listed.map(async ({ target, text }) => map(await parse(system, text), ({ commands }) => ({ target, commands })))));
            return map(parsed, (scripts) => {
                const runs = scripts.flatMap(({ target, commands }) =>
                    commands.flatMap(({ invocations: chain }) => chain.slice(-1).flatMap((ran) => (ran[0] === program && operands(ran).inputs[0] === subcommand ? [{ target, options: new Set(operands(ran).options) }] : []))),
                );
                const union = new Set(runs.flatMap(({ options: names }) => [...names]));
                const selecting = new Set(options.filter((name) => union.has(name)));
                const fitting = runs.filter(({ options: names }) => [...selecting].every((name) => names.has(name)));
                const fewest = Math.min(...fitting.map(({ options: names }) => names.size));
                const parent = (target: string): string => {
                    const grouped = target.split(':').slice(0, 2).join(':');
                    return targets[grouped]?.dependsOn?.some((entry) => {
                        const named = typeof entry === 'string' ? entry : entry.target;
                        return named === target || (named.endsWith(':*') && target.startsWith(named.slice(0, -1)));
                    }) === true
                        ? grouped
                        : target;
                };
                const [chosen, ...ambiguous] = [...new Set(fitting.filter(({ options: names }) => names.size === fewest).map(({ target }) => parent(target)))];
                if (chosen === undefined || ambiguous.length > 0) {
                    return none;
                }
                const kept = forwarded(program, words.slice(1), new Set([...selecting, ...(row.project === 'owner' ? row.selects : [])]), subcommand !== undefined);
                const moved = row.project === 'root' && cwd.value !== root.value;
                const rebased = kept.map(({ word, operand }) => (operand && moved ? toArray(placed(word.text, cwd, home)).filter((path) => within(path, root.value)).map((path) => quote(relative(path))) : [raw(word.span)]));
                if ((!runs.some(({ target }) => target === chosen) && kept.some(({ operand }) => !operand)) || rebased.some((texts) => texts.length === 0)) {
                    return none;
                }
                const passed = [...rebased.flat(), ...(moved && !kept.some(({ operand }) => operand) ? [quote(relative(cwd.value))] : [])];
                return some(`nx run ${name}:${chosen}${passed.length > 0 ? ` -- ${passed.join(' ')}` : ''}`);
            });
        });
    });
};

// --- [DECISION]

const unrun = (detail: string): string => `function-hooks policy did not run. ${detail}`;

const faulted = (lead: string, faults: readonly Fault[]): Decision => ({
    kind: 'deny',
    reason: `${lead}. ${faults.map((value) => (value.kind === 'exited' ? [`${value.program} exited ${value.code}`, value.output].filter((part) => part !== '').join('. ') : `${value.program} failed. ${String(value.cause)}`)).join('. ')}`,
});

const changes = async (call: Call, raw: (span: Word['span']) => string): Promise<Result<readonly Change[]>> =>
    map(
        all(
            await Promise.all(
                call.script.commands
                    .filter(({ nested }) => !nested)
                    .map(async (command): Promise<Result<readonly Change[]>> => {
                        const parts = segments(command);
                        const last = parts.at(-1);
                        const head = parts.find(({ invocation: [program] }) => program !== 'timeout');
                        const [start] = head?.words ?? [];
                        const end = last?.words.at(-1);
                        if (last === undefined || head === undefined || start === undefined || end === undefined) {
                            return ok([]);
                        }
                        const timing = timed(parts);
                        const launch = launched(command);
                        if (launch.kind === 'some' && launch.value.kind === 'launch') {
                            const { names, flags, environment } = launch.value;
                            const prefix = environment && parts[0] === head ? assigned(command) : [];
                            const text = ['watch-apps', 'launch', ...names.map(quote), '--hides', ...flags, ...prefix.map(({ text: pair }) => `--environment=${quote(pair)}`)].join(' ');
                            return ok([...timing, edit((prefix[0] ?? start).span.start, end.span.end, text, some(`${last.invocation[0]} became watch-apps launch ${names.join(' ')}`))]);
                        }
                        return map(await checker(call, command, last, raw), (found) => [
                            ...timing,
                            ...(found.kind === 'some' ? [edit(start.span.start, end.span.end, found.value, some(`${last.invocation[0]} ran as ${found.value.split(' -- ')[0]}`))] : finals(call, command, last)),
                        ]);
                    }),
            ),
        ),
        (lists) => lists.flat(),
    );

const commandDecision = async (system: System, command: string, project: string, request: Request = FOREGROUND): Promise<Decision> => {
    const parsed = await parse(system, command);
    if (parsed.kind === 'fault') {
        return faulted('Command not parsed', parsed.faults);
    }
    const call = { system, script: parsed.value, project, request };
    const reasons = new Set((await Promise.all([deletes, git, walks, probes, stdin, inline, scripts, polls, launches, detached].map((policy) => policy(call)))).flat());
    if (reasons.size > 0) {
        return { kind: 'deny', reason: [...reasons].join('. ') };
    }
    const bytes = new TextEncoder().encode(command);
    const decoder = new TextDecoder();
    const edited = await changes(call, ({ start, end }) => decoder.decode(bytes.subarray(start, end)));
    if (edited.kind === 'fault') {
        return faulted('Command not judged', edited.faults);
    }
    const background = request.tool === 'Bash' && !request.background && parsed.value.commands.some(({ invocations: chain }) => chain.some(([program]) => program === 'analyzeHeadless'));
    if (edited.value.length === 0 && !background) {
        return { kind: 'allow' };
    }
    const sorted = edited.value.toSorted((left, right) => left.start - right.start);
    const notices = [...new Set([...sorted.flatMap(({ notice }) => toArray(notice)), ...(background ? ['analyzeHeadless runs in background, completion notice follows'] : [])])];
    const timeouts = sorted.flatMap(({ timeout }) => toArray(timeout));
    return {
        kind: 'rewrite',
        command: [...sorted.map(({ start, text }, index) => `${decoder.decode(bytes.subarray(sorted[index - 1]?.end, start))}${text}`), decoder.decode(bytes.subarray(sorted.at(-1)?.end))].join(''),
        notice: notices.length === 0 ? none : some(notices.join('. ')),
        timeout: timeouts.length === 0 ? none : some(Math.max(...toArray(request.timeout), timeouts.reduce((sum, value) => sum + value, 0))),
        background,
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { commandDecision, type Request, type System, unrun, WORKTREE };

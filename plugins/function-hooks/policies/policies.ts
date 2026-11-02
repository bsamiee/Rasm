import { given, none, type Option, some, toArray } from '../composition.ts';
import { type Command, parse, printed, type Script, spliced, type Word } from './command.ts';
import { declared, firstOperand, type Invocation, informational, invocations, operands, option, optional, placed, waits, within } from './invocation.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Decision = { readonly kind: 'deny'; readonly reason: string } | { readonly kind: 'allow' } | { readonly kind: 'rewrite'; readonly command: string; readonly notice: Option<string>; readonly timeout: Option<number>; readonly background: boolean };

interface System {
    readonly run: (argv: Invocation, init: { readonly cwd: string; readonly stdin: string }) => Promise<{ readonly exitCode: number; readonly stdout: string; readonly stderr: string }>;
    readonly exists: (path: string) => Promise<boolean>;
    readonly plugin: string;
    readonly root: Option<string>;
    readonly cwd: string;
    readonly home: Option<string>;
}
interface Request {
    readonly background: boolean;
    readonly timeout: Option<number>;
}
interface Call {
    readonly system: System;
    readonly script: Script;
    readonly project: string;
    readonly request: Option<Request>;
}
interface Launch {
    readonly program: string;
    readonly start: Word;
    readonly app: string;
    readonly flags: readonly string[];
    readonly foreign: readonly string[];
}
interface Target {
    readonly options?: { readonly command?: string; readonly commands?: readonly (string | { readonly command: string })[] };
    readonly dependsOn?: readonly (string | { readonly target: string })[];
}
interface Change {
    readonly start: number;
    readonly end: number;
    readonly text: string;
    readonly notice: Option<string>;
    readonly timeout: Option<number>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORKTREE = 'creates second checkout. Work in main checkout';
const BRANCH = 'creates branch. Work on current branch';
const UNPLACED = 'runs after unresolved directory change. Run cd to named directory first';
const ALIAS = 'defines alias that can hide refused subcommand';
const OVERWRITES = 'overwrites working-tree files. Edit files';
const BREAK = /(?<!\\)(?:\\\\)*\\n/u;
const EXPANDS = /[$`]/u;
const GLOB = /[*?[{]/u;
const NAMED = /[\p{L}\p{N}]/u;
const SAFE = /^[\w@%+=:,./-]+$/u;
const DURATION = /^(?<value>\d+(?:\.\d+)?)(?<unit>[smh]?)$/u;
const BUNDLE = /^(?<bundle>\/Applications\/.+?\.app)\/Contents\/MacOS\/(?<executable>[^/]+)$/u;
const TELL = /^tell (?:application|app) (?:id )?"(?<app>[^"]+)"(?: to (?<verb>\w+))?$/u;
const VERB = /^(?:activate|launch|run|reopen)$/u;
const CHECKER = /^(?:lint|format|typecheck):(?<tool>[^:]+)/u;
const CACHED = /^(?:check|typecheck(?::.+)?)$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [WORDS]

const quote = (word: string): string => (SAFE.test(word) ? word : `'${word.replaceAll("'", String.raw`'\''`)}'`);

const items = (program: string, [head, ...rest]: readonly Word[]): readonly { readonly words: readonly Word[]; readonly names: readonly string[] }[] => {
    if (head === undefined || head.text === '--') {
        return [];
    }
    const { names, taken } = optional(declared(program), head.text) ? option(declared(program), head.text) : { names: [], taken: 0 };
    return [{ words: [head, ...rest.slice(0, taken)], names }, ...items(program, rest.slice(taken))];
};

const edit = (start: number, end: number, text: string, notice: Option<string> = none, timeout: Option<number> = none): Change => ({ start, end, text, notice, timeout });

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

const inline = ({ system: { home }, script: { commands } }: Call): readonly string[] =>
    commands.flatMap(({ fed, invocations: chain, cwd }, index) => {
        const written = new Set(commands.slice(0, index).flatMap((writer) => writer.written.flatMap((path) => toArray(placed(path, writer.cwd, home)))));
        const ran = (word: string): boolean => toArray(placed(word, cwd, home)).some((path) => written.has(path));
        return chain.flatMap((invocation) => {
            const [program] = invocation;
            const { code, shell } = declared(program);
            const { options, inputs } = operands(invocation);
            const inlined = code !== undefined && (options.some((name) => code.includes(name)) || (fed && waits(invocation)));
            return inlined || ((code ?? shell) !== undefined && inputs.slice(0, 1).some(ran)) ? [`${program} runs inline program. Edit with Edit or sd, run installed tools by name`] : [];
        });
    });

// --- [POLL]

const polls = ({ request, script: { commands, background } }: Call): readonly string[] =>
    toArray(request)
        .flatMap((settings) => (settings.background ? [] : commands))
        .flatMap(({ looped, invocations: chain }) =>
            chain.flatMap((invocation, at) => {
                const [program] = invocation;
                const { options, inputs } = operands(invocation);
                const holding = (program === 'caffeinate' && at === chain.length - 1) || (program === 'wait' && inputs.length > 0 && !background);
                const probing = looped && (program === 'sleep' || (program === 'kill' && options.includes('-0')) || (program === 'lsof' && options.includes('-p')));
                return [...(holding ? [`${program} holds call open. Wait on task notification or in Monitor until-loop`] : []), ...(probing ? [`Loop polls with ${program}. Run until-loop in Monitor, or run_in_background for own work`] : [])];
            }),
        );

// --- [CHECK]

const checks = ({ script: { commands } }: Call): readonly string[] =>
    commands.flatMap(({ invocations: chain }) =>
        chain.some(([program, ...rest]) =>
            ['ast-grep', 'sg', 'tsc', 'biome', 'swiftlint', 'swift-format', 'periphery', 'tombi', 'cargo-clippy', 'cargo-dylint'].includes(program ?? '') ||
            (program === 'cargo' && ['clippy', 'check', 'dylint', 'fmt', 'test', 'nextest'].includes(rest[0] ?? '')) ||
            rest.some((word, at) => ['run', 'run-many', 'affected'].includes(word) && rest.slice(at + 1).some((target) => /(^|:|-t=?|,)(check|lint|typecheck|format|test)\b/.test(target))),
        )
            ? ['Checks run once when the queue drains. Continue the work without running them']
            : [],
    );

// --- [LAUNCH]

const opened = (head: Word, args: readonly Word[]): Option<Launch> => {
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
              app: app.raw,
              foreign: switches.filter((name) => !['-a', '-b', '-n', '-g', '-j', '-F', '--env'].includes(name)),
              flags: [...(switches.includes('-n') ? ['--new'] : []), ...documents.map(({ raw }) => `--document ${raw}`), ...valued('--env').map(({ raw }) => `--environment=${raw}`), ...args.slice(cut < 0 ? args.length : cut + 1).map(({ raw }) => `--arguments=${raw}`)],
          });
};

const told = (head: Word, invocation: Invocation): Option<Launch> => {
    const { values, inputs } = operands(invocation);
    const lines = values.flatMap(([name, value]) => (name === '-e' ? value.split('\n') : [])).map((line) => line.trim());
    const groups = lines.map((line) => TELL.exec(line)?.groups);
    const apps = [...new Set(groups.flatMap((group) => (group?.app === undefined ? [] : [group.app])))];
    const plain = lines.every((line, at) => (groups[at] === undefined ? line === '' || line === 'end tell' || VERB.test(line) : groups[at]?.verb === undefined || VERB.test(groups[at]?.verb ?? '')));
    return inputs.length > 0 || apps.length === 0 || !plain || !lines.some((line, at) => VERB.test(groups[at]?.verb ?? line)) ? none : some({ program: 'osascript', start: head, app: apps.map(quote).join(' '), flags: [], foreign: [] });
};

const bundled = async (system: System, invocation: Invocation, { assigned, words: [head, ...args] }: Command['parts'][number]): Promise<Option<Launch>> => {
    const [program] = invocation;
    const groups = head === undefined ? undefined : BUNDLE.exec(head.text)?.groups;
    if (head === undefined || groups?.bundle === undefined || informational(program, operands(invocation).options) || declared(program).headless?.some((name) => invocation.includes(name)) === true) {
        return none;
    }
    const info = await printed<{ readonly CFBundleExecutable?: string; readonly CFBundleIdentifier?: string }>(system, ['plutil', '-convert', 'json', '-o', '-', `${groups.bundle}/Contents/Info.plist`], system.plugin);
    return info.kind === 'ok' && info.value.CFBundleIdentifier !== undefined && info.value.CFBundleExecutable === groups.executable
        ? some({ program, start: assigned[0] ?? head, app: info.value.CFBundleIdentifier, flags: [...args.map(({ raw }) => `--arguments=${raw}`), ...assigned.map(({ raw }) => `--environment=${raw}`)], foreign: [] })
        : none;
};

const launching = async ({ system }: Call, { invocations: chain, parts }: Command): Promise<Option<Launch>> => {
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

const launches = async (call: Call): Promise<readonly string[]> =>
    (await Promise.all(call.script.commands.map(async (command) => toArray(await launching(call, command)).flatMap(({ program, app, foreign }) => (command.nested || foreign.length > 0 ? [`${[program, ...foreign].join(' ')} launches ${app} outside watch-apps. Run watch-apps launch ${app}`] : []))))).flat();

// --- [REWRITE]

const timed = ({ request }: Call, { invocations: chain, parts }: Command): readonly Change[] => {
    const second = 1000;
    const base = 60;
    const applied = toArray(request).some(({ background, timeout }) => !background && timeout.kind === 'none');
    const milliseconds = (amount: string): Option<number> => {
        const groups = DURATION.exec(amount)?.groups;
        const value = Number(groups?.value) * second * base ** 'smh'.indexOf(String(groups?.unit));
        return value > 0 ? some(value) : none;
    };
    return chain.flatMap((invocation, at) => {
        const [from] = parts[at]?.words ?? [];
        const [to] = [...(parts[at + 1]?.assigned ?? []), ...(parts[at + 1]?.words ?? [])];
        const [amount] = operands(invocation).leading;
        const limit = invocation[0] === 'timeout' && amount !== undefined ? milliseconds(amount) : none;
        return from === undefined || to === undefined || limit.kind === 'none' ? [] : [edit(from.span.start, to.span.start, '', some(applied ? `timeout ${amount} became call timeout` : `timeout ${amount} removed, call keeps its timeout`), applied ? limit : none)];
    });
};

const respelled = (program: string, words: readonly Word[], table: Readonly<Record<string, string>>, reason = ''): readonly Change[] =>
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

const replaced = (invocation: Invocation, [head, ...rest]: readonly Word[]): readonly Change[] => {
    const { options, leading } = operands(invocation);
    const [find] = leading;
    const [word] = items('sd', rest).flatMap(({ names, words: [first] }) => (names.length === 0 && first?.text.startsWith('-') === true ? [first] : []));
    const breaks = find !== undefined && (find.includes('\n') || (!options.some((name) => name === '-F' || name === '--fixed-strings') && BREAK.test(find)));
    return [
        ...(head === undefined || !breaks || options.some((name) => name === '-A' || name === '--across') ? [] : [edit(head.span.end, head.span.end, ' -A')]),
        ...(word === undefined || rest.some(({ text }) => text === '--') ? [] : [edit(word.span.start, word.span.start, '-- ', some(`sd took -- before ${word.raw}`))]),
    ];
};

const uncached = (invocation: Invocation, words: readonly Word[]): readonly Change[] => {
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

const logged = (root: string, words: readonly Word[]): readonly Change[] =>
    (
        [
            ['-log', 'ghidra.log'],
            ['-scriptlog', 'ghidra-script.log'],
        ] as const
    ).flatMap(([flag, file]) => {
        const log = quote(`${root}/.artifacts/logs/${file}`);
        const at = words.findIndex(({ text }) => text === flag);
        const [value, tail] = [words[at + 1], words.at(-1)];
        const notice = some('analyzeHeadless logs to .artifacts/logs');
        if (at < 0) {
            return tail === undefined ? [] : [edit(tail.span.end, tail.span.end, ` ${flag} ${log}`, notice)];
        }
        return value === undefined || value.raw === log ? [] : [edit(value.span.start, value.span.end, log, notice)];
    });

const finals = ({ system: { root } }: Call, invocation: Invocation, words: readonly Word[]): readonly Change[] => {
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
        case 'analyzeHeadless':
            return root.kind === 'none' || informational(program, options) ? [] : logged(root.value, rest);
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

const owner = async ({ system, project }: Call, selectors: readonly string[], invocation: Invocation, cwd: string, root: string): Promise<Option<string>> => {
    const [value, ...more] = operands(invocation).values.flatMap(([flag, argument]) => (selectors.includes(flag) ? [argument] : []));
    const [place] = toArray(placed(value ?? '.', some(cwd), system.home)).filter((path) => within(path, root));
    const located = (path: string): Option<string> => {
        const leaf = path.slice(path.lastIndexOf('/') + 1);
        const folder = leaf === 'tsconfig.json' ? path.slice(0, path.lastIndexOf('/')) : path;
        return folder === path && leaf.lastIndexOf('.') > 0 ? none : some(folder.slice(root.length + 1) || '.');
    };
    if (selectors.length === 0) {
        return some(project);
    }
    if (more.length > 0) {
        return none;
    }
    const pattern = place !== undefined && (await system.exists(place)) ? located(place) : given(value);
    const found = pattern.kind === 'none' ? undefined : await printed<readonly string[]>(system, ['mise', 'exec', '--', 'nx', 'show', 'projects', '--json', '-p', pattern.value], root);
    const [name, ...others] = found?.kind === 'ok' ? found.value : [];
    return name === undefined || others.length > 0 ? none : some(name);
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
    const name = await owner(call, selectors, invocation, cwd.value, root.value);
    const project = name.kind === 'none' ? undefined : await printed<{ readonly targets: Readonly<Record<string, Target>> }>(call.system, ['mise', 'exec', '--', 'nx', 'show', 'project', name.value, '--json'], root.value);
    if (name.kind === 'none' || project?.kind !== 'ok') {
        return none;
    }
    const [subcommand] = inputs;
    const found = await runs(call, project.value.targets, program, subcommand);
    const known = new Set(found.flatMap(({ keys }) => [...keys].flatMap((key) => key.split('=').slice(0, 1))));
    const wanted = [...keyed(invocation)].filter((key) =>
        key
            .split('=')
            .slice(0, 1)
            .some((flag) => known.has(flag)),
    );
    const target = chosen(project.value.targets, found, wanted);
    const kept = forwarded(program, words.slice(1), new Set([...known, ...selectors]), subcommand !== undefined);
    const switched = kept.some(({ names }) => names.length > 0);
    const moved = selectors.length === 0 && cwd.value !== root.value;
    const passed = kept.flatMap((item) => item.words).map((word) => (moved ? rebased(word, cwd.value, root.value, home) : some(word.raw)));
    if (target.kind === 'none' || (target.value.aggregate && (wanted.length > 0 || switched)) || (moved && switched) || passed.some(({ kind }) => kind === 'none')) {
        return none;
    }
    return some({ run: `${name.value}:${target.value.target}`, passed: [...passed.flatMap(toArray), ...(moved && passed.length === 0 ? [quote(cwd.value.slice(root.value.length + 1))] : [])] });
};

// --- [DECISION]

const unrun = (detail: string): string => `function-hooks policy did not run. ${detail}`;

const rewrites = async (call: Call, command: Command): Promise<readonly Change[]> => {
    const last = command.invocations.at(-1);
    const words = command.parts.at(-1)?.words ?? [];
    const [head] = words;
    const tail = words.at(-1);
    if (command.nested || last === undefined || head === undefined || tail === undefined) {
        return [];
    }
    const timing = timed(call, command);
    const launch = await launching(call, command);
    if (launch.kind === 'some') {
        const { program, start, app, flags } = launch.value;
        return [...timing, edit(start.span.start, tail.span.end, ['watch-apps launch', app, ...flags].join(' '), some(`${program} became watch-apps launch ${app}`))];
    }
    const found = await checker(call, command, last, words);
    if (found.kind === 'some') {
        const { run, passed } = found.value;
        return [...timing, edit(head.span.start, tail.span.end, [`nx run ${run}`, ...(passed.length > 0 ? ['--', ...passed] : [])].join(' '), some(`${last[0]} ran as nx run ${run}`))];
    }
    return [...timing, ...finals(call, last, words)];
};

const commandDecision = async (system: System, command: string, project: string, request: Option<Request> = none): Promise<Decision> => {
    const parsed = await parse(system, command);
    if (parsed.kind === 'fault') {
        return { kind: 'deny', reason: `Command not parsed. ${parsed.faults.map((value) => (value.kind === 'exited' ? [`${value.program} exited ${value.code}`, value.output].filter((part) => part !== '').join('. ') : `${value.program} failed. ${String(value.cause)}`)).join('. ')}` };
    }
    const call = { system, script: parsed.value, project, request };
    const reasons = new Set((await Promise.all([deletes, git, walks, probes, stdin, inline, polls, checks, launches, detached].map((policy) => policy(call)))).flat());
    if (reasons.size > 0) {
        return { kind: 'deny', reason: [...reasons].join('. ') };
    }
    const changes = (await Promise.all(parsed.value.commands.map((each) => rewrites(call, each)))).flat();
    const background = toArray(request).some((settings) => !settings.background) && parsed.value.commands.some(({ nested, invocations: chain }) => !nested && chain.some((invocation) => invocation[0] === 'analyzeHeadless' && !informational(invocation[0], operands(invocation).options)));
    const notices = [...new Set([...changes.flatMap(({ notice }) => toArray(notice)), ...(background ? ['analyzeHeadless runs in background, completion notice follows'] : [])])];
    const timeouts = changes.flatMap(({ timeout }) => toArray(timeout));
    return changes.length === 0 && !background
        ? { kind: 'allow' }
        : {
              kind: 'rewrite',
              command: spliced(new TextEncoder().encode(command), 0, changes),
              notice: notices.length === 0 ? none : some(notices.join('. ')),
              timeout: timeouts.length === 0 ? none : some(timeouts.reduce((sum, value) => sum + value, 0)),
              background,
          };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { commandDecision, type System, unrun, WORKTREE };

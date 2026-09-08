// Shell rows over the argvs of a Bash command: refusals that name the correct form, span rewrites, and routing lines

// --- [IMPORTS] -------------------------------------------------------------------------

import { type Decision, deny, rewrite } from '../composition/decision.ts';
import { type Argv, hasGroup, INTERPRETER, parse, pastAssignments, type Span, strip, type Word } from '../text/argv.ts';
import { basename } from '../text/path.ts';
import { BINLOG_DENY, type OnceLine, OP_LINE } from './paths.ts';

// --- [TYPES] ---------------------------------------------------------------------------

// The input of a Bash call, the command with the tool's own bound in milliseconds and its background flag
interface Bash {
    readonly command: string;
    readonly timeout?: number;
    readonly ['run_in_background']?: boolean;
}

// The command a rewrite row produced with the line naming the change
interface Rewritten {
    readonly command: string;
    readonly context: string;
}

// The command after the rewrite rows with one line per applied rewrite
interface Applied {
    readonly command: string;
    readonly context: readonly string[];
}

// Rows read one argv and the whole command, head names the command words the row reads and an empty head reads every argv, and a move
// answers undefined or no line for an argv it leaves as written
interface ShellRow {
    readonly head: readonly string[];
    readonly deny?: (argv: Argv, command: string) => string | undefined;
    readonly rewrite?: (argv: Argv, command: string) => Rewritten | undefined;
    readonly lines?: (argv: Argv, command: string) => readonly OnceLine[];
}

interface Splice extends Span {
    readonly text: string;
}

// An argv headed by timeout with its duration in milliseconds and the start of the wrapped command, whole when the argv is the command
interface Prefix {
    readonly head: Word;
    readonly ms: number;
    readonly wrapped: number;
    readonly whole: boolean;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _MISE_EXEC: readonly string[] = ['x', 'exec'];
// The eval word with the opening of the substitution before the argv, and the closing with the joining operator after it
const _EVAL_OPEN = /\beval\s+"?\$\($/u;
const _EVAL_CLOSE = /^\)"?\s*(?:&&|;)?\s*/u;
const _PREVIEW: readonly string[] = ['--dry-run', '--dryRun', '--dryrun'];
// -n is a dry run for act and the guarded git subcommands alone, uv reads it as --no-cache and pytest as its worker count
const _SHORT_PREVIEW: readonly string[] = ['act', 'git'];
const _GIT_PREVIEW: readonly string[] = ['add', 'rm', 'mv', 'clean', 'push', 'fetch'];
const _REDIRECTS: readonly string[] = ['>', '>>', '<'];
const _OP_REFERENCE = 'op://';
// The gh subcommands the github MCP covers, gh keeps the local checkout forms CLAUDE.md names
const _GH_WRITES: readonly string[] = ['issue', 'run rerun', 'pr merge'];
const _UV_PIN = /^(?<name>[^=]+)==/u;
const _PNPM_PIN = /^(?<name>@?[^@]+)@(?<version>.+)$/u;
const _RECURSIVE_LS = /^-[A-Za-z]*R|^--recursive$/u;
const _BINLOG = /\.binlog$/u;
const _BINLOG_SWITCH = /^-bl/u;
// The skill a grep or rg over a file kind routes to, the tool that reads the kind by its semantics
const _GREP_SKILLS: readonly (readonly [RegExp, string, string])[] = [
    [/\.cs$/u, 'dotnet-roslyn-codelens', 'Roslyn reads C# by its semantics and rg reads text'],
    [/\.(?:csproj|props|targets)$/u, 'dotnet-msbuild-evaluation', 'MSBuild evaluation reads project files and rg reads text'],
    [/\.(?:ts|py)$/u, 'ast-grep', 'ast-grep reads code by its syntax tree and rg reads text'],
];
// The Bash timeout parameter's maximum, BuiltinToolInputs.Bash in claude-code.d.ts
const _TIMEOUT_MAX_MS = 600_000;
// The argvs that run past the default two minutes on a cold cache, a fresh restore, an uncached target, or a solution build
const _SLOW: Readonly<Partial<Record<string, (words: readonly string[]) => boolean>>> = {
    nx: (words): boolean => ['run', 'run-many', 'affected'].includes(words[1] ?? ''),
    dotnet: (words): boolean => ['build', 'test'].includes(words[1] ?? ''),
    'ast-grep': (words): boolean => words[1] === 'test',
    claude: (words): boolean => words.includes('-p') || words.includes('--print'),
    act: (): boolean => true,
    uv: (words): boolean => words[1] === 'sync',
    pnpm: (words): boolean => words[1] === 'install',
};
// The one form that runs past the ceiling, a workflow job under act
const _WORKFLOW = 'rasm:workflow';
const _MS_PER_SECOND = 1000;
const _TIMEOUT_WORDS: readonly string[] = ['timeout', 'gtimeout'];
// -s and -k take the next word as their value, the = and attached spellings are one word
const _TIMEOUT_VALUE_OPTIONS: readonly string[] = ['-s', '--signal', '-k', '--kill-after'];
// The GNU duration, a number with an optional unit suffix, and the seconds each unit holds
const _DURATION = /^(?<number>\d+(?:\.\d*)?|\.\d+)(?<unit>[smhd]?)$/u;
const _UNIT_SECONDS: Readonly<Record<string, number>> = { '': 1, s: 1, m: 60, h: 3600, d: 86_400 };
// The operator after an argv and the operator before a last argv, a pipe feeds a command and a sleep beside a pipe stays
const _JOIN_AFTER = /^[ \t]*(?:&&|\|\||;|&|\r?\n)\s*/u;
const _JOIN_BEFORE = /\s*(?:&&|\|\||;|&|\r?\n)\s*$/u;
// The reserved words at command position, an argv opening with one sits in a compound command
const _RESERVED: readonly string[] = [
    'if',
    'then',
    'else',
    'elif',
    'fi',
    'do',
    'done',
    'while',
    'until',
    'for',
    'case',
    'esac',
    '{',
    '}',
    '!',
    'time',
];
const _WAIT =
    'run the command it waits for with run_in_background: true and read its completion notification, or watch the condition with an until loop under the Monitor tool';
// Every NX_DAEMON value but false outranks nx.json useDaemonProcess: false and starts the daemon, whose outputs watcher logs every event
// under .cache/ into a log that never rotates
const _DAEMON = /^NX_DAEMON=(?!false$)/u;
const _MISE_LINE = 'the SessionStart hook wrote the mise environment to CLAUDE_ENV_FILE';

// --- [WORDS] ---------------------------------------------------------------------------

const _texts = (argv: Argv): readonly string[] => argv.map((word) => word.text);

const _word = (argv: Argv, index: number): string => argv[index]?.text ?? '';

// The argv past its env assignments and the reserved words of a compound command, the command a row reads
const _command = (argv: Argv): Argv => {
    const words = pastAssignments(argv);
    const index = words.findIndex((word) => !_RESERVED.includes(word.text));
    return index < 0 ? [] : words.slice(index);
};

const _head = (argv: Argv): string => basename(_word(_command(argv), 0));

const _has = (argv: Argv, pattern: RegExp): boolean => argv.slice(1).some((word) => pattern.test(word.text));

const _group = (match: RegExpMatchArray | null, name: string): string => match?.groups?.[name] ?? '';

// The whole argv's span, from its first word to its last
const _span = (argv: Argv): Span => ({ start: argv[0]?.start ?? 0, end: argv.at(-1)?.end ?? 0 });

const _text = (argv: Argv, command: string): string => command.slice(_span(argv).start, _span(argv).end);

// Splices apply from the last span to the first, every earlier offset survives
const _splice = (command: string, splices: readonly Splice[]): string =>
    splices
        .toSorted((left, right) => right.start - left.start)
        .reduce((text, splice) => text.slice(0, splice.start) + splice.text + text.slice(splice.end), command);

// Removed words take the space before them, and a leading word the space after it
const _removal = (word: Word): Splice =>
    word.start === 0 ? { start: 0, end: word.end + 1, text: '' } : { start: word.start - 1, end: word.end, text: '' };

const _replaced = (word: Word, text: string): Splice => ({ start: word.start, end: word.end, text });

const _dropped = (command: string, words: readonly Word[]): string => _splice(command, words.map(_removal));

// --- [MISE] ----------------------------------------------------------------------------

const _isMiseExec = (argv: Argv): boolean => _MISE_EXEC.includes(_word(argv, 1));

// The first word of the command mise exec runs, after -- when present, else the first word past the tool specs that holds no @
const _miseCommand = (argv: Argv): Word | undefined => {
    const dash = _texts(argv).indexOf('--');
    return dash >= 0 ? argv[dash + 1] : argv.slice(2).find((word) => !word.text.includes('@'));
};

// An option among the words before the command, -C or --command, changes what runs and has no direct form
const _miseOption = (argv: Argv): boolean => {
    const command = _miseCommand(argv);
    const end = command === undefined ? argv.length : argv.indexOf(command);
    return argv.slice(2, end).some((word) => word.text !== '--' && word.text.startsWith('-'));
};

const _miseRewrite = (argv: Argv, command: string): Rewritten => {
    const span = _span(argv);
    const end = _miseCommand(argv)?.start ?? span.end;
    return {
        command: _splice(command, [{ start: span.start, end, text: '' }]),
        context: `Ran ${command.slice(end, span.end)} in place of ${command.slice(span.start, end).trim()}, ${_MISE_LINE}, and a missing binary is a missing [tools] row in mise.toml followed by mise install`,
    };
};

// The command with the eval word through the closing quote and the joining operator removed, undefined when the substitution sits elsewhere
const _evalDropped = (argv: Argv, command: string): string | undefined => {
    const span = _span(argv);
    const open = command.slice(0, span.start).match(_EVAL_OPEN);
    const close = command.slice(span.end).match(_EVAL_CLOSE)?.[0].length ?? 0;
    return open === null ? undefined : _splice(command, [{ start: open.index ?? 0, end: span.end + close, text: '' }]);
};

const _isEvalEnv = (argv: Argv, command: string): boolean => _word(argv, 1) === 'env' && _evalDropped(argv, command) !== undefined;

// Every argv of the command is the eval word, parsed as the shell sentinel, or the mise env it evaluates, nothing else runs
const _allEval = (command: string): boolean =>
    _parse(command).every((argv) => _word(argv, 1) === INTERPRETER || (_word(argv, 0) === 'mise' && _word(argv, 1) === 'env'));

// The line of a mise argv that runs as written: an exec with no command or an option before it, or an eval of the environment alone
const _miseLine = (argv: Argv, command: string): readonly OnceLine[] => {
    if (_isMiseExec(argv) && (_miseCommand(argv) === undefined || _miseOption(argv))) {
        return [
            {
                key: 'mise',
                line: `mise ${_word(argv, 1)} names no command or holds an option before it, ${_MISE_LINE}, run the command itself with -C <dir> as cd <dir> && <command> and --command as the command`,
            },
        ];
    }
    return _isEvalEnv(argv, command) && _allEval(command) ? [{ key: 'mise', line: `eval "$(mise env)" changes nothing, ${_MISE_LINE}` }] : [];
};

// The mise argv rewritten to the command it runs: an exec past its tool specs, or an eval dropped before the command it joins
const _miseRewritten = (argv: Argv, command: string): Rewritten | undefined => {
    if (_isMiseExec(argv)) {
        return _miseCommand(argv) !== undefined && !_miseOption(argv) ? _miseRewrite(argv, command) : undefined;
    }
    const dropped = _isEvalEnv(argv, command) && !_allEval(command) ? _evalDropped(argv, command) : undefined;
    return dropped === undefined ? undefined : { command: dropped, context: `Dropped eval "$(${_texts(argv).join(' ')})", ${_MISE_LINE}` };
};

// --- [PACKAGES] ------------------------------------------------------------------------

const _previewFlag = (argv: Argv): Word | undefined =>
    argv.slice(1).find((word) => _PREVIEW.includes(word.text) || (_SHORT_PREVIEW.includes(_head(argv)) && word.text === '-n'));

const _previewsGit = (argv: Argv): boolean => _head(argv) !== 'git' || _GIT_PREVIEW.includes(_word(argv, 1));

const _adds = (argv: Argv): boolean => _word(argv, 1) === 'add';

// The words of a pnpm add that pin a version other than the catalog
const _pnpmPins = (argv: Argv): readonly Word[] =>
    argv.slice(2).filter((word) => {
        const version = _group(word.text.match(_PNPM_PIN), 'version');
        return version !== '' && version !== 'catalog:';
    });

// The --version switch of a dotnet package add with its value
const _dotnetVersion = (argv: Argv): readonly Word[] => {
    const words = _texts(argv);
    const index = words.indexOf('--version');
    const adds = (words[1] === 'add' && words[2] === 'package') || (words[1] === 'package' && words[2] === 'add');
    return adds && index > 0 ? argv.slice(index, index + 2) : [];
};

// --- [SLEEP] ---------------------------------------------------------------------------

const _isSleep = (argv: Argv): boolean => _head(strip(argv)) === 'sleep';

const _allSleep = (command: string): boolean => _parse(command).every(_isSleep);

// A sleep inside a loop, conditional, brace group, case body, or subshell polls or paces a step, and no span drop keeps the command whole
const _compound = (command: string): boolean =>
    hasGroup(command) || _parse(command).some((argv) => _RESERVED.includes(_word(pastAssignments(argv), 0)));

// The span of a top-level sleep argv with its joining operator, the one after it or the one before a last argv, none beside a pipe
const _sleepSplice = (argv: Argv, command: string): Splice | undefined => {
    const span = _span(argv);
    const after = command.slice(span.end).match(_JOIN_AFTER);
    if (after !== null) {
        return { start: span.start, end: span.end + after[0].length, text: '' };
    }
    const before = command.slice(span.end).trim() === '' ? command.slice(0, span.start).match(_JOIN_BEFORE) : null;
    return before === null ? undefined : { start: before.index ?? 0, end: span.end, text: '' };
};

// --- [TIMEOUT] -------------------------------------------------------------------------

// The argv spans the trimmed command, the argvs of a wrapped shell body sit inside it
const _whole = (argv: Argv, command: string): boolean =>
    argv[0]?.start === command.length - command.trimStart().length && argv.at(-1)?.end === command.trimEnd().length;

// The timeout prefix of an argv, undefined without one, or with a duration outside the GNU grammar or no command after it
const _prefix = (argv: Argv, command: string): Prefix | undefined => {
    const words = pastAssignments(argv);
    const [head] = words;
    if (head === undefined || !_TIMEOUT_WORDS.includes(basename(head.text))) {
        return undefined;
    }
    const tail = words.slice(1);
    const at = tail.findIndex((word, index) => !(word.text.startsWith('-') || _TIMEOUT_VALUE_OPTIONS.includes(_word(tail, index - 1))));
    const match = at < 0 ? null : _word(tail, at).match(_DURATION);
    const wrapped = tail[at + 1];
    if (match === null || wrapped === undefined) {
        return undefined;
    }
    const ms = Math.round(Number(_group(match, 'number')) * (_UNIT_SECONDS[_group(match, 'unit')] ?? 1) * _MS_PER_SECOND);
    return { head, ms, wrapped: wrapped.start, whole: _whole(argv, command) };
};

// --- [ROUTING] -------------------------------------------------------------------------

const _ghWrite = (argv: Argv): string | undefined => _GH_WRITES.find((sub) => _texts(argv).slice(1).join(' ').startsWith(sub));

const _grepLines = (argv: Argv): readonly OnceLine[] => [
    ...(_head(argv) === 'grep' ? [{ key: 'ast-grep', line: 'Load the ast-grep skill, rg is for literals and comments' }] : []),
    ..._GREP_SKILLS.flatMap(([pattern, skill, reason]) => (_has(argv, pattern) ? [{ key: skill, line: `Load the ${skill} skill, ${reason}` }] : [])),
];

// --- [ROWS] ----------------------------------------------------------------------------

const SHELL = [
    {
        head: [],
        rewrite: (argv: Argv, command: string): Rewritten | undefined => {
            const words = argv.filter((word) => _DAEMON.test(word.text));
            return words.length === 0
                ? undefined
                : {
                      command: _dropped(command, words),
                      context: `Dropped ${_texts(words).join(' ')}, nx.json useDaemonProcess: false keeps the Nx daemon off and its outputs watcher log never rotates`,
                  };
        },
    },
    { head: ['mise'], lines: _miseLine },
    { head: ['mise'], rewrite: _miseRewritten },
    {
        head: ['nx', 'pnpm', 'dotnet', 'uv', 'pulumi', 'act', 'git'],
        lines: (argv: Argv, command: string): readonly OnceLine[] => {
            const flag = _previewFlag(argv);
            return _previewsGit(argv) && flag !== undefined
                ? [{ key: 'preview', line: `A preview proves nothing, the proof is the target itself: ${_dropped(command, [flag])}` }]
                : [];
        },
    },
    {
        head: ['uv'],
        rewrite: (argv: Argv, command: string): Rewritten | undefined => {
            const pins = _adds(argv) ? argv.slice(2).filter((word) => _UV_PIN.test(word.text)) : [];
            return pins.length === 0
                ? undefined
                : {
                      command: _splice(
                          command,
                          pins.map((word) => _replaced(word, _group(word.text.match(_UV_PIN), 'name'))),
                      ),
                      context: 'Dropped the version pins, uv.lock alone pins versions',
                  };
        },
    },
    {
        head: ['pnpm'],
        rewrite: (argv: Argv, command: string): Rewritten | undefined => {
            const pins = _adds(argv) ? _pnpmPins(argv) : [];
            return pins.length === 0
                ? undefined
                : {
                      command: _splice(
                          command,
                          pins.map((word) => _replaced(word, `${_group(word.text.match(_PNPM_PIN), 'name')}@catalog:`)),
                      ),
                      context: 'Ran the packages at @catalog:, pnpm-workspace.yaml holds every version',
                  };
        },
    },
    {
        head: ['dotnet'],
        rewrite: (argv: Argv, command: string): Rewritten | undefined => {
            const words = _dotnetVersion(argv);
            return words.length === 2
                ? { command: _dropped(command, words), context: 'Dropped --version, Directory.Packages.props alone holds the version' }
                : undefined;
        },
    },
    {
        head: ['cat', 'head', 'strings', 'xxd', 'od', 'less', 'tail'],
        deny: (argv: Argv): string | undefined => (_has(argv, _BINLOG) ? BINLOG_DENY : undefined),
    },
    {
        head: [],
        lines: (argv: Argv, command: string): readonly OnceLine[] =>
            command.includes(_OP_REFERENCE) && (argv.some((word) => _REDIRECTS.includes(word.text)) || command.includes('<<'))
                ? [{ line: OP_LINE }]
                : [],
    },
    // A sleep alone or inside a loop, conditional, group, or case body runs as written, the line names the wait forms once
    {
        head: [],
        lines: (argv: Argv, command: string): readonly OnceLine[] =>
            _isSleep(argv) && (_allSleep(command) || _compound(command))
                ? [{ key: 'sleep', line: `${_text(argv, command)} holds the turn, ${_WAIT}` }]
                : [],
    },
    {
        head: [],
        rewrite: (argv: Argv, command: string): Rewritten | undefined => {
            const splice = _isSleep(argv) && !_allSleep(command) && !_compound(command) ? _sleepSplice(argv, command) : undefined;
            return splice === undefined ? undefined : { command: _splice(command, [splice]), context: `Dropped ${_text(argv, command)}, ${_WAIT}` };
        },
    },
    { head: ['grep', 'rg'], lines: _grepLines },
    {
        head: ['ls'],
        lines: (argv: Argv): readonly OnceLine[] =>
            _has(argv, _RECURSIVE_LS) ? [{ key: 'tree', line: 'Use tree <dir> to list every directory and file, -D for directories alone' }] : [],
    },
    { head: ['find'], lines: (): readonly OnceLine[] => [{ key: 'fd', line: 'Use fd for filesystem queries, fd <pattern> <dir>' }] },
    {
        head: ['wc'],
        lines: (argv: Argv): readonly OnceLine[] =>
            _texts(argv).includes('-l') ? [{ key: 'loc', line: 'Use loc <dir> for the line count with a complexity score per file' }] : [],
    },
    {
        head: ['gh'],
        lines: (argv: Argv): readonly OnceLine[] => {
            const sub = _ghWrite(argv);
            return sub === undefined
                ? []
                : [
                      {
                          key: 'github',
                          line: `Use the github MCP for gh ${sub}, gh serves the local checkout (pull requests from HEAD, checks, checkout, releases, secrets)`,
                      },
                  ];
        },
    },
    {
        head: ['dotnet'],
        lines: (argv: Argv): readonly OnceLine[] =>
            _word(argv, 1) === 'build' && !argv.some((word) => _BINLOG_SWITCH.test(word.text) || _BINLOG.test(word.text))
                ? [
                      {
                          key: 'dotnet-msbuild-diagnostics',
                          line: 'Add -bl to dotnet build and read the .binlog through the dotnet-msbuild-diagnostics skill',
                      },
                  ]
                : [],
    },
] as const satisfies readonly ShellRow[];

// --- [HITS] ----------------------------------------------------------------------------

// The heads the rows read, with npm for the package manager rule and sleep for the sleep rows, each an argv of its own inside interpreter code
const _parse = parse([...new Set(SHELL.flatMap((row) => row.head)), 'npm', 'sleep']);

const _applies = (row: ShellRow, argv: Argv): boolean =>
    row.head.length === 0 || row.head.includes(_head(argv)) || row.head.includes(_head(strip(argv)));

// The argvs of a command a row reads
const _argvs = (row: ShellRow, command: string): readonly Argv[] => _parse(command).filter((argv) => _applies(row, argv));

// One rewrite row over its argvs from the last to the first, every earlier span survives
const _rewriteRow = (row: ShellRow, state: Applied): Applied =>
    _argvs(row, state.command)
        .toReversed()
        .reduce((current, argv) => {
            const next = row.rewrite?.(argv, current.command);
            return next === undefined ? current : { command: next.command, context: [...current.context, next.context] };
        }, state);

// The rewrite rows in table order, each over the command the rows before it produced, the context one line per applied rewrite
const _rewritten = (command: string): Applied =>
    SHELL.reduce<Applied>((state, row: ShellRow) => (row.rewrite === undefined ? state : _rewriteRow(row, state)), { command, context: [] });

// The deny and line rows read one parse of the rewritten command, each over the argvs its head names
const _denial = (command: string): string | undefined => {
    const argvs = _parse(command);
    return SHELL.flatMap((row: ShellRow) =>
        row.deny === undefined ? [] : argvs.filter((argv) => _applies(row, argv)).flatMap((argv) => row.deny?.(argv, command) ?? []),
    )[0];
};

// The once lines the command raises, the adapter stamps their keys as injected once the call ran
const shellOnce = (command: string): readonly OnceLine[] => {
    const argvs = _parse(command);
    return SHELL.flatMap((row: ShellRow) =>
        row.lines === undefined ? [] : argvs.filter((argv) => _applies(row, argv)).flatMap((argv) => row.lines?.(argv, command) ?? []),
    );
};

// --- [RULES] ---------------------------------------------------------------------------

// The rewrites run first and the refusals read the command they produced, then the unseen once lines join the rewrite lines
const shellRule =
    (seen: ReadonlySet<string>): (<E extends Bash>(e: E) => Decision<E>) =>
    <E extends Bash>(e: E): Decision<E> => {
        const next = _rewritten(e.command);
        const reason = _denial(next.command);
        if (reason !== undefined) {
            return deny(reason);
        }
        const once = shellOnce(next.command).filter((line) => line.key === undefined || !seen.has(line.key));
        return rewrite({ ...e, command: next.command }, [...new Set([...next.context, ...once.map((line) => line.line)])]);
    };

// Every npm argv takes the configured package manager by span
const packageManager =
    (name: string): (<E extends Bash>(e: E) => Decision<E>) =>
    <E extends Bash>(e: E): Decision<E> => {
        const words = _parse(e.command).flatMap((argv) => {
            const [head] = pastAssignments(argv);
            return head !== undefined && basename(head.text) === 'npm' ? [head] : [];
        });
        return words.length > 0
            ? rewrite(
                  {
                      ...e,
                      command: _splice(
                          e.command,
                          words.map((word) => _replaced(word, name)),
                      ),
                  },
                  [`Ran ${name} in place of npm`],
              )
            : rewrite(e);
    };

// Every timeout prefix of the command moves its duration to the Bash parameter, one whole prefix keeps the smaller bound and partial ones the larger
const commandTimeout = <E extends Bash>(e: E): Decision<E> => {
    const prefixes = _parse(e.command).flatMap((argv) => _prefix(argv, e.command) ?? []);
    if (prefixes.length === 0) {
        return rewrite(e);
    }
    const largest = Math.min(_TIMEOUT_MAX_MS, Math.max(...prefixes.map((prefix) => prefix.ms)));
    const timeout = prefixes.every((prefix) => prefix.whole) ? Math.min(largest, e.timeout ?? largest) : Math.max(largest, e.timeout ?? 0);
    const command = _splice(
        e.command,
        prefixes.map((prefix): Splice => ({ start: prefix.head.start, end: prefix.wrapped, text: '' })),
    );
    return rewrite({ ...e, command, timeout }, [
        `Ran ${command} under the Bash timeout parameter at ${timeout} ms`,
        ...prefixes
            .filter((prefix) => prefix.ms > _TIMEOUT_MAX_MS)
            .map(
                (prefix) =>
                    `timeout ${prefix.ms} ms exceeds the Bash timeout maximum of ${_TIMEOUT_MAX_MS} ms and ran capped, run_in_background: true runs past it`,
            ),
    ]);
};

// An argv's tool from its raw words past the assignments or its stripped words, whichever the table names
const _isSlow = (argv: Argv): boolean =>
    [pastAssignments(argv), strip(argv)].some((words) => _SLOW[basename(_word(words, 0))]?.(_texts(words)) === true);

const _runsWorkflow = (argv: Argv): boolean => {
    const words = strip(argv);
    return basename(_word(words, 0)) === 'nx' && _word(words, 1) === 'run' && _word(words, 2) === _WORKFLOW;
};

// A background call passes, a workflow job moves to the background with its line, and a slow argv raises the bound to the ceiling silently
const commandCeiling = <E extends Bash>(e: E): Decision<E> => {
    if (e.run_in_background === true) {
        return rewrite(e);
    }
    const argvs = _parse(e.command);
    if (argvs.some(_runsWorkflow)) {
        return rewrite({ ...e, ['run_in_background']: true }, [
            `Ran with run_in_background: true, ${_WORKFLOW} runs a workflow job past the Bash timeout maximum of ${_TIMEOUT_MAX_MS} ms, and its completion notification carries the result`,
        ]);
    }
    return argvs.some(_isSlow) && (e.timeout ?? 0) < _TIMEOUT_MAX_MS ? rewrite({ ...e, timeout: _TIMEOUT_MAX_MS }) : rewrite(e);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Bash, Rewritten, ShellRow };
export { commandCeiling, commandTimeout, packageManager, shellOnce, shellRule };

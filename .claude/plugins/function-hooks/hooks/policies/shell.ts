// Shell rows over parsed leaves for refusals that name the correct form, span rewrites, and routing context

// --- [IMPORTS] -------------------------------------------------------------------------

import { type Decision, deny, rewrite } from '../composition/decision.ts';
import { flatMap, fromBoolean, fromNullable, getOrElse, liftPredicate, map, type Option, toArray } from '../composition/option.ts';
import { type Leaf, leaves, pastAssignments, strip, type Word } from '../text/argv.ts';
import { basename, extension, under } from '../text/path.ts';
import { BINLOG_DENY, OP_DENY } from './paths.ts';
import { TREE } from './scan.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Command {
    readonly command: string;
}

interface Rewritten {
    readonly command: string;
    readonly context: string;
}

// Rows read one raw leaf and the whole command, once names the skill or tool a context row injects once per session
interface ShellRow {
    readonly word: readonly string[];
    readonly when: (leaf: Leaf, command: string) => boolean;
    readonly deny?: (leaf: Leaf, command: string) => string;
    readonly rewrite?: (leaf: Leaf, command: string) => Rewritten;
    readonly context?: (leaf: Leaf) => string;
    readonly once?: string;
}

interface Hit {
    readonly row: ShellRow;
    readonly leaf: Leaf;
}

interface Splice {
    readonly start: number;
    readonly end: number;
    readonly text: string;
}

// The input of a Bash call, the command and the tool's own bound in milliseconds
interface Timed {
    readonly command: string;
    readonly timeout?: number;
}

// A leaf headed by timeout: the duration past its options, the first word of the wrapped command, and whether the leaf is the whole command
interface Prefix {
    readonly head: Word;
    readonly duration: Option<Word>;
    readonly wrapped: Option<Word>;
    readonly whole: boolean;
}

interface TimeoutRow {
    readonly when: (prefix: Prefix) => boolean;
    readonly deny: (prefix: Prefix, command: string) => string;
}

// One nx run <project>:<target> leaf carrying a skip-cache flag, the flag word is the span the rule removes
interface NxTarget {
    readonly project: string;
    readonly target: string;
    readonly flag: Word;
}

// The -r flag of a scan leaf with its file, the span from the flag word to the file word and the file it names
interface RuleFile {
    readonly start: number;
    readonly end: number;
    readonly file: string;
}

// The cache flag of each project:target the adapter read from nx show project, absent when the manifest states none or the read failed
type NxCaches = Readonly<Record<string, boolean>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const _MAX_REWRITES = 8;
const _MISE_EXEC: readonly string[] = ['x', 'exec'];
const _MISE_ENV = 'env';
// The eval word with the opening of the substitution before the leaf, and the closing with the joining operator after it
const _EVAL_OPEN = /\beval\s+"?\$\($/u;
const _EVAL_CLOSE = /^\)"?\s*(?:&&|;)?\s*/u;
const _PACKAGE_INDEX = 3;
const _PREVIEW: readonly string[] = ['--dry-run', '--dryRun', '--dryrun'];
// -n is a dry run for act and the guarded git subcommands alone, uv reads it as --no-cache and pytest as its worker count
const _SHORT_PREVIEW: readonly string[] = ['act', 'git'];
const _GIT_PREVIEW: readonly string[] = ['add', 'rm', 'mv', 'clean', 'push', 'fetch'];
const _REDIRECTS: readonly string[] = ['>', '>>', '<'];
const _GH_WRITES: readonly string[] = ['api', 'issue', 'release create', 'run rerun', 'pr create', 'pr merge'];
const _CATALOG = 'catalog:';
const _UV_PIN = /^(?<name>[^=]+)==/u;
const _PNPM_PIN = /^(?<name>@?[^@]+)@(?<version>.+)$/u;
const _RECURSIVE_LS = /^-[A-Za-z]*R|^--recursive$/u;
const _CSHARP = /\.cs$/u;
const _MSBUILD = /\.(?:csproj|props|targets)$/u;
const _SYNTAX = /\.(?:ts|py)$/u;
const _BINLOG = /\.binlog$/u;
const _BINLOG_SWITCH = /^-bl/u;
// The Bash timeout parameter's maximum, BuiltinToolInputs.Bash in claude-code.d.ts
const _TIMEOUT_MAX_MS = 600_000;
const _MS_PER_SECOND = 1000;
const _TIMEOUT_WORDS: readonly string[] = ['timeout', 'gtimeout'];
// -s and -k take the next word as their value, the = and attached spellings are one word
const _TIMEOUT_VALUE_OPTIONS: readonly string[] = ['-s', '--signal', '-k', '--kill-after'];
// The GNU duration, a number with an optional unit suffix, and the seconds each unit holds
const _DURATION = /^(?<number>\d+(?:\.\d*)?|\.\d+)(?<unit>[smhd]?)$/u;
const _UNIT_SECONDS: Readonly<Record<string, number>> = { '': 1, s: 1, m: 60, h: 3600, d: 86_400 };
const _UPDATE_ALL: readonly string[] = ['-U', '--update-all'];
const _FILTER: readonly string[] = ['--filter', '-f'];
// A scratch config or test directory is one agent's own tree, its snapshots are nobody else's
const _OWN_TREE: readonly string[] = ['-c', '--config', '-t', '--test-dir'];
const _TEST_UPDATE_DENY =
    "ast-grep test -U with no --filter rewrites every changed snapshot in the shared tree, run ast-grep test -U --filter '^<id>$' for the rule whose snapshot changed";
const _NX_TARGET = /^(?<project>[^:]+):(?<target>[^:]+)/u;
const _SKIP_CACHE: readonly string[] = ['--skip-nx-cache', '--skipNxCache'];
const _AST_GREP = 'ast-grep';
const _INCLUDE_OFF = '--include-off';
// The subcommands that take --json and -U, and test takes -U alone
const _SEARCHES: readonly string[] = ['run', 'scan'];
const _INTERACTIVE: readonly string[] = ['-i', '--interactive'];
const _JSON = /^--json(?:=.*)?$/u;
// The array styles, each one line under wc -l
const _JSON_ARRAY = /^--json(?:=(?:pretty|compact))?$/u;
const _JSON_STREAM = '--json=stream';
const _PIPE = /^\s*\|\s*$/u;
const _LANG: readonly string[] = ['-l', '--lang'];
const _LANG_TYPESCRIPT = /^(?:-l=?|--lang=)typescript$/u;
const _TYPESCRIPT = 'typescript';
const _TSX = 'tsx';
const _STDIN = '--stdin';
const _CONFIG: readonly string[] = ['-c', '--config'];
const _SCAN = 'scan';
const _RULE_FLAGS: readonly string[] = ['-r', '--rule'];
const _RULE_ATTACHED = /^--rule=(?<file>.+)$/u;
const _YAML = /\.ya?ml$/u;
// A rule id as the tree spells it, rule-checks.sh pairing fails a file stem that differs from the id
const _RULE_ID = /^[a-z][a-z0-9-]*$/u;
// The test options that take the next word as their value
const _TEST_VALUE_OPTIONS: readonly string[] = ['-t', '--test-dir', '--snapshot-dir', '-f', '--filter', '-c', '--config', '--color'];
const _LANG_ATTACHED = /^(?:-l=?|--lang=)\S+$/u;
// The operator after a leaf and the operator before a last leaf, a pipe feeds a command and a sleep beside a pipe stays
const _JOIN_AFTER = /^[ \t]*(?:&&|\|\||;|&|\r?\n)\s*/u;
const _JOIN_BEFORE = /\s*(?:&&|\|\||;|&|\r?\n)\s*$/u;
// The compound openers and closers by leaf head, a sleep inside a loop or an if body is the until form and stays
const _OPENERS: readonly string[] = ['if', 'do', '{', 'case'];
const _CLOSERS: readonly string[] = ['fi', 'done', '}', 'esac'];
const _WAIT =
    'run the command it waits for with run_in_background: true and read its completion notification, or watch the condition with an until loop under the Monitor tool';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _texts = (leaf: Leaf): readonly string[] => leaf.map((word) => word.text);

const _word = (leaf: Leaf, index: number): string => leaf[index]?.text ?? '';

const _head = (leaf: Leaf): string => basename(_word(leaf, 0));

const _has = (leaf: Leaf, pattern: RegExp): boolean => leaf.slice(1).some((word) => pattern.test(word.text));

const _first = (leaf: Leaf, pattern: RegExp): Option<RegExpMatchArray> =>
    fromNullable(leaf.slice(1).flatMap((word) => toArray(fromNullable(word.text.match(pattern))))[0]);

const _group = (match: RegExpMatchArray, name: string): string => match.groups?.[name] ?? '';

// Splices apply from the last span to the first, every earlier offset survives
const _splice = (command: string, splices: readonly Splice[]): string =>
    splices
        .toSorted((left, right) => right.start - left.start)
        .reduce((text, splice) => text.slice(0, splice.start) + splice.text + text.slice(splice.end), command);

// Removed words take the space before them
const _removal = (word: Word): Splice => ({ start: Math.max(word.start - 1, 0), end: word.end, text: '' });

const _without = (command: string, word: Word): string => _splice(command, [_removal(word)]);

const _sub = (leaf: Leaf): string => _GH_WRITES.find((sub) => _texts(leaf).slice(1).join(' ').startsWith(sub)) ?? '';

const _previewFlags = (leaf: Leaf): readonly string[] =>
    fromBoolean(_SHORT_PREVIEW.includes(_head(leaf))).match<readonly string[]>({
        some: () => [..._PREVIEW, '-n'],
        none: () => _PREVIEW,
    });

const _previewFlag = (leaf: Leaf): Option<Word> => fromNullable(leaf.slice(1).find((word) => _previewFlags(leaf).includes(word.text)));

const _previewWord = (leaf: Leaf): boolean => _head(leaf) !== 'git' || _GIT_PREVIEW.includes(_word(leaf, 1));

const _pinnedUv = (leaf: Leaf): boolean => _word(leaf, 1) === 'add' && _has(leaf, _UV_PIN);

const _pinnedPnpm = (leaf: Leaf): boolean =>
    _word(leaf, 1) === 'add' &&
    leaf
        .slice(2)
        .some((word) =>
            fromNullable(word.text.match(_PNPM_PIN)).match<boolean>({ some: (match) => _group(match, 'version') !== _CATALOG, none: () => false }),
        );

const _pinnedDotnet = (leaf: Leaf): boolean => {
    const [first, second] = [_word(leaf, 1), _word(leaf, 2)];
    return ((first === 'add' && second === 'package') || (first === 'package' && second === 'add')) && _texts(leaf).includes('--version');
};

const _packageName = (leaf: Leaf): string =>
    _texts(leaf)
        .slice(_PACKAGE_INDEX)
        .find((text) => !text.startsWith('-')) ?? '';

const _redirected = (leaf: Leaf, command: string): boolean =>
    command.includes('op://') && (leaf.some((word) => _REDIRECTS.includes(word.text)) || command.includes('<<'));

const _buildsWithoutBinlog = (leaf: Leaf): boolean =>
    _word(leaf, 1) === 'build' && !leaf.some((word) => _BINLOG_SWITCH.test(word.text) || _BINLOG.test(word.text));

// The words past the runner and its options, the view a row over a runner-wrapped command reads
const _stripped = (leaf: Leaf): readonly string[] => _texts(strip(leaf));

const _rewritesSharedSnapshots = (leaf: Leaf): boolean => {
    const words = _stripped(leaf);
    return (
        basename(_word(strip(leaf), 0)) === 'ast-grep' &&
        words[1] === 'test' &&
        words.some((word) => _UPDATE_ALL.includes(word)) &&
        _bareIds(strip(leaf)).length === 0 &&
        !words.some((word) => _FILTER.includes(word) || word.startsWith('--filter=') || _OWN_TREE.includes(word))
    );
};

// The whole leaf's span, from its first word to its last
const _leafSpan = (leaf: Leaf, text: string): Splice => ({
    start: getOrElse(() => 0)(map((word: Word) => word.start)(fromNullable(leaf[0]))),
    end: getOrElse(() => 0)(map((word: Word) => word.end)(fromNullable(leaf.at(-1)))),
    text,
});

const _isMiseExec = (leaf: Leaf): boolean => _MISE_EXEC.includes(_word(leaf, 1));

// The first word of the command mise exec runs, after -- when present, else the first word past the tool specs that holds no @
const _miseCommand = (leaf: Leaf): Option<Word> =>
    liftPredicate<number>((dash) => dash >= 0)(_texts(leaf).indexOf('--')).match<Option<Word>>({
        some: (dash) => fromNullable(leaf[dash + 1]),
        none: () => fromNullable(leaf.slice(2).find((word) => !word.text.includes('@'))),
    });

// An option among the words before and at the command, -C or --command, changes what runs and has no direct form
const _miseOption = (leaf: Leaf): boolean => {
    const end = getOrElse(() => leaf.length)(map((word: Word) => leaf.indexOf(word) + 1)(_miseCommand(leaf)));
    return leaf.slice(2, end).some((word) => word.text !== '--' && word.text.startsWith('-'));
};

const _miseRewrite = (leaf: Leaf, command: string): Rewritten => {
    const span = _leafSpan(leaf, '');
    const end = getOrElse(() => span.end)(map((word: Word) => word.start)(_miseCommand(leaf)));
    return {
        command: _splice(command, [{ start: span.start, end, text: '' }]),
        context: `Ran ${command.slice(end, span.end)} in place of ${command.slice(span.start, end).trim()}, the SessionStart hook wrote the mise environment to CLAUDE_ENV_FILE, and a missing binary is a missing [tools] row in mise.toml followed by mise install`,
    };
};

// The command with the eval word through the closing quote and the joining operator removed, none when the substitution sits elsewhere
const _evalDropped = (leaf: Leaf, command: string): Option<string> => {
    const span = _leafSpan(leaf, '');
    const close = getOrElse(() => 0)(map((match: RegExpMatchArray) => match[0].length)(fromNullable(command.slice(span.end).match(_EVAL_CLOSE))));
    return map((open: RegExpMatchArray) => _splice(command, [{ start: open.index ?? 0, end: span.end + close, text: '' }]))(
        fromNullable(command.slice(0, span.start).match(_EVAL_OPEN)),
    );
};

const _evalEnv = (leaf: Leaf, command: string, runs: boolean): boolean =>
    _word(leaf, 1) === _MISE_ENV && _evalDropped(leaf, command).match<boolean>({ some: (rest) => (rest.trim() !== '') === runs, none: () => false });

const _evalRewrite = (leaf: Leaf, command: string): Rewritten => ({
    command: getOrElse(() => command)(_evalDropped(leaf, command)),
    context: `Dropped eval "$(${_texts(leaf).join(' ')})", the SessionStart hook wrote the mise environment to CLAUDE_ENV_FILE`,
});

// The nx run <project>:<target> leaf with its skip-cache flag, none for another command or a run without the flag
const _nxTarget = (leaf: Leaf): Option<NxTarget> => {
    const stripped = strip(leaf);
    return flatMap((flag: Word) =>
        map((match: RegExpMatchArray): NxTarget => ({ project: _group(match, 'project'), target: _group(match, 'target'), flag }))(
            fromNullable(_word(stripped, 2).match(_NX_TARGET)),
        ),
    )(
        flatMap(() => fromNullable(stripped.find((word) => _SKIP_CACHE.includes(word.text))))(
            fromBoolean(basename(_word(stripped, 0)) === 'nx' && _word(stripped, 1) === 'run'),
        ),
    );
};

// --- [AST_GREP] ------------------------------------------------------------------------

// The words past the runner when the leaf runs an ast-grep subcommand the predicate admits, none for another command
const _astGrep = (leaf: Leaf, subcommand: (word: string) => boolean): Option<readonly Word[]> =>
    liftPredicate<readonly Word[]>((words) => basename(_word(words, 0)) === _AST_GREP && subcommand(_word(words, 1)))(strip(leaf));

const _astGrepWhen =
    (subcommand: (word: string) => boolean, holds: (words: readonly Word[], command: string) => boolean) =>
    (leaf: Leaf, command: string): boolean =>
        _astGrep(leaf, subcommand).match<boolean>({ some: (words) => holds(words, command), none: () => false });

const _isTest = (word: string): boolean => word === 'test';

const _isSearch = (word: string): boolean => _SEARCHES.includes(word);

// A first word that is an option runs the implicit run
const _isRun = (word: string): boolean => word === 'run' || word.startsWith('-');

const _isUpdater = (word: string): boolean => _isSearch(word) || _isTest(word);

const _hasWord = (words: readonly Word[], list: readonly string[]): boolean => words.some((word) => list.includes(word.text));

const _matching = (words: readonly Word[], pattern: RegExp): readonly Word[] => words.filter((word) => pattern.test(word.text));

const _removals = (words: readonly Word[], command: string): string => _splice(command, words.map(_removal));

// The flag inserted after the word with the space before it
const _after = (word: Word, text: string): Splice => ({ start: word.end, end: word.end, text: ` ${text}` });

const _includeOff = (leaf: Leaf, command: string): Rewritten => ({
    command: _splice(command, toArray(map((word: Word) => _after(word, _INCLUDE_OFF))(fromNullable(strip(leaf)[1])))),
    context: 'Ran ast-grep test --include-off, the severity: off rewrite rules under rewrites/ run only under that flag',
});

// The typescript value of -l or --lang, the next word or attached with = or nothing between
const _typescriptWords = (words: readonly Word[]): readonly Word[] =>
    words.filter((word, index) => _LANG_TYPESCRIPT.test(word.text) || (word.text === _TYPESCRIPT && _LANG.includes(_word(words, index - 1))));

// A config of its own or stdin parses .ts as typescript
const _parsesAlone = (words: readonly Word[]): boolean =>
    _texts(words).includes(_STDIN) || words.some((word) => _CONFIG.includes(word.text) || word.text.startsWith('--config='));

// A cd leaf leaves the working directory unread, the root sgconfig.yml then binds no run after it
const _changesDirectory = (command: string): boolean => _leaves(command).some((other) => _head(other) === 'cd');

const _parsesElsewhere = (words: readonly Word[], command: string): boolean => _parsesAlone(words) || _changesDirectory(command);

const _tsxRewrite = (leaf: Leaf, command: string): Rewritten => ({
    command: _splice(
        command,
        _typescriptWords(strip(leaf)).map(
            (word): Splice => ({ start: word.start, end: word.end, text: word.text.slice(0, -_TYPESCRIPT.length) + _TSX }),
        ),
    ),
    context: 'Ran with -l tsx, sgconfig.yml languageGlobs maps every .ts file to tsx and typescript finds nothing',
});

const _jsonDrop = (leaf: Leaf, command: string): Rewritten => ({
    command: _removals(_matching(strip(leaf), _JSON), command),
    context: 'Dropped --json beside -U, the two together write nothing',
});

const _interactiveDrop = (leaf: Leaf, command: string): Rewritten => ({
    command: _removals(
        strip(leaf).filter((word) => _INTERACTIVE.includes(word.text)),
        command,
    ),
    context: 'Dropped -i beside -U, -U accepts every diff and -i prompts on a terminal the Bash tool lacks',
});

// The leaf after this one when one pipe joins them, the words share their last word with the raw leaf
const _piped = (words: readonly Word[], command: string): Option<Leaf> => {
    const parsed = _leaves(command);
    return flatMap((index: number) =>
        flatMap((next: Leaf) => liftPredicate<Leaf>(() => _PIPE.test(command.slice(_leafSpan(words, '').end, _leafSpan(next, '').start)))(next))(
            fromNullable(parsed[index + 1]),
        ),
    )(liftPredicate<number>((index) => index >= 0)(parsed.findIndex((other) => _leafSpan(other, '').end === _leafSpan(words, '').end)));
};

const _countsLines = (words: readonly Word[], command: string): boolean =>
    _piped(words, command).match<boolean>({ some: (next) => _head(next) === 'wc' && _texts(next).includes('-l'), none: () => false });

const _streamRewrite = (leaf: Leaf, command: string): Rewritten => ({
    command: _splice(
        command,
        _matching(strip(leaf), _JSON_ARRAY).map((word): Splice => ({ start: word.start, end: word.end, text: _JSON_STREAM })),
    ),
    context: 'Ran --json=stream before wc -l, the array form counts one line',
});

const _isScan = (word: string): boolean => word === _SCAN;

const _hasConfig = (words: readonly Word[]): boolean => words.some((word) => _CONFIG.includes(word.text) || word.text.startsWith('--config='));

// The span from the flag word to the file word of -r <file>, --rule <file>, or --rule=<file>, and the file it names
const _ruleFile = (words: readonly Word[]): Option<RuleFile> =>
    fromNullable(
        words.flatMap((word, index): readonly RuleFile[] => [
            ...toArray(
                map((match: RegExpMatchArray): RuleFile => ({ start: word.start, end: word.end, file: _group(match, 'file') }))(
                    fromNullable(word.text.match(_RULE_ATTACHED)),
                ),
            ),
            ...toArray(
                map((file: Word): RuleFile => ({ start: word.start, end: file.end, file: file.text }))(
                    flatMap(() => fromNullable(words[index + 1]))(fromBoolean(_RULE_FLAGS.includes(word.text))),
                ),
            ),
        ])[0],
    );

const _isRewriteFile = (file: string): boolean => under(file, TREE.rewrites);

// A rule file of the tree under the root config, a scratch config registers its own set and a draft outside the tree has no id
const _treeRule = (words: readonly Word[]): Option<RuleFile> =>
    flatMap(liftPredicate<RuleFile>((found) => _YAML.test(found.file) && (under(found.file, TREE.rules) || _isRewriteFile(found.file))))(
        flatMap(() => _ruleFile(words))(fromBoolean(!_hasConfig(words))),
    );

const _ruleId = (file: string): string => basename(file).slice(0, -extension(file).length);

const _filterFlag = (id: string): string => `--filter '^${id}$'`;

// The filter for the file's id, with --error=<id> under rewrites/ where the severity is off
const _filterText = (file: string): string =>
    fromBoolean(_isRewriteFile(file)).match<string>({
        some: () => `${_filterFlag(_ruleId(file))} --error=${_ruleId(file)}`,
        none: () => _filterFlag(_ruleId(file)),
    });

const _ruleRewrite = (leaf: Leaf, command: string): Rewritten =>
    _treeRule(strip(leaf)).match<Rewritten>({
        some: (found) => ({
            command: _splice(command, [{ start: found.start, end: found.end, text: _filterText(found.file) }]),
            context: `Ran ${_filterText(found.file)} in place of ${command.slice(found.start, found.end)}, scan -r loads no utilDirs and the root sgconfig.yml registers the rule`,
        }),
        none: () => ({ command, context: '' }),
    });

// The words past the subcommand that are no option and no option value, the ids a bare test form names
const _bareIds = (words: readonly Word[]): readonly Word[] =>
    words.filter((word, index) => index > 1 && _RULE_ID.test(word.text) && !_TEST_VALUE_OPTIONS.includes(_word(words, index - 1)));

const _idsPattern = (ids: readonly Word[]): string =>
    fromBoolean(ids.length > 1).match<string>({ some: () => `(${_texts(ids).join('|')})`, none: () => _word(ids, 0) });

// The first bare id becomes the filter and the rest leave
const _filterRewrite = (leaf: Leaf, command: string): Rewritten => {
    const ids = _bareIds(strip(leaf));
    const filter = _filterFlag(_idsPattern(ids));
    return {
        command: _splice(
            command,
            ids.map(
                (word, index): Splice =>
                    fromBoolean(index === 0).match<Splice>({
                        some: () => ({ start: word.start, end: word.end, text: filter }),
                        none: () => _removal(word),
                    }),
            ),
        ),
        context: `Ran ast-grep test ${filter}, test takes no positional and --filter selects the cases by rule id`,
    };
};

// The -l or --lang words with their values, in the split, =, and attached spellings
const _langWords = (words: readonly Word[]): readonly Word[] =>
    words.filter((word, index) => _LANG.includes(word.text) || _LANG_ATTACHED.test(word.text) || _LANG.includes(_word(words, index - 1)));

const _langDrop = (leaf: Leaf, command: string): Rewritten => ({
    command: _removals(_langWords(strip(leaf)), command),
    context: `Dropped ${_texts(_langWords(strip(leaf))).join(' ')}, scan takes no language flag and each rule's language field parses its files`,
});

// --- [SLEEP] ---------------------------------------------------------------------------

const _isSleep = (leaf: Leaf): boolean => _head(leaf) === 'sleep';

const _leafText = (leaf: Leaf, command: string): string => command.slice(_leafSpan(leaf, '').start, _leafSpan(leaf, '').end);

// The compound depth at the leaf, the openers less the closers among the leaves before it
const _depth = (leaf: Leaf, command: string): number => {
    const { start } = _leafSpan(leaf, '');
    return _leaves(command)
        .filter((other) => _leafSpan(other, '').end <= start)
        .reduce((depth, other) => depth + Number(_OPENERS.includes(_head(other))) - Number(_CLOSERS.includes(_head(other))), 0);
};

// The span of a top-level sleep leaf with its joining operator, the one after it or the one before a last leaf, none beside a pipe
const _sleepSpan = (leaf: Leaf, command: string): Option<Splice> => {
    const span = _leafSpan(leaf, '');
    const rest = command.slice(span.end);
    return fromNullable(
        [
            ...toArray(
                map((match: RegExpMatchArray): Splice => ({ start: span.start, end: span.end + match[0].length, text: '' }))(
                    fromNullable(rest.match(_JOIN_AFTER)),
                ),
            ),
            ...toArray(
                map((match: RegExpMatchArray): Splice => ({ start: match.index ?? 0, end: span.end, text: '' }))(
                    flatMap(() => fromNullable(command.slice(0, span.start).match(_JOIN_BEFORE)))(fromBoolean(rest.trim() === '')),
                ),
            ),
        ][0],
    );
};

const _allSleep = (command: string): boolean => _leaves(command).every(_isSleep);

// One leaf leaves per pass, the re-read of the rewritten command drops the next
const _sleepRewrite = (leaf: Leaf, command: string): Rewritten => ({
    command: _splice(command, toArray(_sleepSpan(leaf, command))),
    context: `Dropped ${_leafText(leaf, command)}, ${_WAIT}`,
});

// --- [TIMEOUT] -------------------------------------------------------------------------

// The first word after the options that is no option and no value of -s or -k
const _durationIndex = (tail: readonly Word[]): number =>
    tail.findIndex((word, index) => !(word.text.startsWith('-') || _TIMEOUT_VALUE_OPTIONS.includes(_word(tail, index - 1))));

// The leaf spans the trimmed command, the leaves of a wrapped shell body sit inside it
const _whole = (leaf: Leaf, command: string): boolean =>
    leaf[0]?.start === command.length - command.trimStart().length && leaf.at(-1)?.end === command.trimEnd().length;

const _prefix = (leaf: Leaf, command: string): Option<Prefix> =>
    map((head: Word): Prefix => {
        const tail = pastAssignments(leaf).slice(1);
        const split = getOrElse(() => tail.length)(liftPredicate<number>((index) => index >= 0)(_durationIndex(tail)));
        return { head, duration: fromNullable(tail[split]), wrapped: fromNullable(tail[split + 1]), whole: _whole(leaf, command) };
    })(flatMap(liftPredicate<Word>((word) => _TIMEOUT_WORDS.includes(basename(word.text))))(fromNullable(pastAssignments(leaf)[0])));

const _durationText = (prefix: Prefix): string => _word(toArray(prefix.duration), 0);

// The duration in milliseconds under the GNU grammar, none for a spelling outside it
const _milliseconds = (prefix: Prefix): Option<number> =>
    map((match: RegExpMatchArray) => Math.round(Number(_group(match, 'number')) * _UNIT_SECONDS[_group(match, 'unit')] * _MS_PER_SECOND))(
        fromNullable(_durationText(prefix).match(_DURATION)),
    );

// The span from the timeout word to the wrapped command, none when nothing follows
const _dropped = (prefix: Prefix): Option<Splice> =>
    map((word: Word): Splice => ({ start: prefix.head.start, end: word.start, text: '' }))(prefix.wrapped);

// The command with every prefix removed by its span, the redirects and quoting around them stay byte for byte
const _unwrapped = (prefixes: readonly Prefix[], command: string): string =>
    _splice(
        command,
        prefixes.flatMap((prefix) => toArray(_dropped(prefix))),
    );

const _advice = (prefix: Prefix, command: string): string =>
    `run ${getOrElse(() => 'the command')(map(() => _unwrapped([prefix], command))(prefix.wrapped))} with the Bash timeout parameter in milliseconds (max ${_TIMEOUT_MAX_MS})`;

// The refusals in the order the first holding one decides, and a prefix past every row rewrites
const _TIMEOUT = [
    {
        when: (prefix: Prefix): boolean => toArray(prefix.duration).length === 0,
        deny: (prefix: Prefix, command: string): string => `timeout names no duration, ${_advice(prefix, command)}`,
    },
    {
        when: (prefix: Prefix): boolean => toArray(prefix.wrapped).length === 0,
        deny: (): string => `timeout names no command, run the command with the Bash timeout parameter in milliseconds (max ${_TIMEOUT_MAX_MS})`,
    },
    {
        when: (prefix: Prefix): boolean => toArray(_milliseconds(prefix)).length === 0,
        deny: (prefix: Prefix, command: string): string =>
            `The timeout duration ${_durationText(prefix)} is no number with an optional s, m, h, or d suffix, ${_advice(prefix, command)}`,
    },
] as const satisfies readonly TimeoutRow[];

// The largest duration under the maximum, one process under both bounds takes the smaller, leaves under their own bounds take the larger
const _bound = (e: Timed, prefixes: readonly Prefix[]): number => {
    const largest = Math.min(_TIMEOUT_MAX_MS, Math.max(...prefixes.flatMap((prefix) => toArray(_milliseconds(prefix)))));
    return fromBoolean(prefixes.every((prefix) => prefix.whole)).match<number>({
        some: () => Math.min(largest, getOrElse(() => largest)(fromNullable(e.timeout))),
        none: () => Math.max(largest, getOrElse(() => 0)(fromNullable(e.timeout))),
    });
};

// One line per duration past the maximum, the bound the cap keeps lifts under run_in_background
const _capped = (prefixes: readonly Prefix[]): readonly string[] =>
    prefixes
        .filter((prefix) => getOrElse(() => 0)(_milliseconds(prefix)) > _TIMEOUT_MAX_MS)
        .map(
            (prefix) =>
                `timeout ${_durationText(prefix)} exceeds the Bash timeout maximum of ${_TIMEOUT_MAX_MS} ms and ran capped, run_in_background: true runs past it`,
        );

const _exact = <E extends Timed>(e: E, prefixes: readonly Prefix[]): Decision<E, unknown, string> => {
    const bound = _bound(e, prefixes);
    const command = _unwrapped(prefixes, e.command);
    return rewrite({ ...e, command, timeout: bound }, [`Ran ${command} under the Bash timeout parameter at ${bound} ms`, ..._capped(prefixes)]);
};

// The first refusal over the prefixes in command order decides, else every prefix moves to the parameter
const _timed = <E extends Timed>(e: E, prefixes: readonly Prefix[]): Decision<E, unknown, string> =>
    fromNullable(
        prefixes.flatMap((prefix) =>
            toArray(map((row: TimeoutRow) => row.deny(prefix, e.command))(fromNullable(_TIMEOUT.find((row) => row.when(prefix))))),
        )[0],
    ).match<Decision<E, unknown, string>>({ some: deny, none: () => _exact(e, prefixes) });

// --- [ROWS] ----------------------------------------------------------------------------

const SHELL = [
    {
        word: ['mise'],
        when: (leaf: Leaf): boolean => _isMiseExec(leaf) && toArray(_miseCommand(leaf)).length === 0,
        deny: (leaf: Leaf): string =>
            `mise ${_word(leaf, 1)} names no command, the SessionStart hook wrote the mise environment to CLAUDE_ENV_FILE, run the command directly`,
    },
    {
        word: ['mise'],
        when: (leaf: Leaf): boolean => _isMiseExec(leaf) && _miseOption(leaf),
        deny: (leaf: Leaf): string =>
            `mise ${_word(leaf, 1)} holds an option before the command, the SessionStart hook wrote the mise environment to CLAUDE_ENV_FILE, run the command itself in place of mise ${_word(leaf, 1)} [<tool>@<version>...] -- <command>, with -C <dir> as cd <dir> && <command> and --command as the command`,
    },
    {
        word: ['mise'],
        when: (leaf: Leaf): boolean => _isMiseExec(leaf) && toArray(_miseCommand(leaf)).length > 0 && !_miseOption(leaf),
        rewrite: _miseRewrite,
    },
    {
        word: ['mise'],
        when: (leaf: Leaf, command: string): boolean => _evalEnv(leaf, command, false),
        deny: (): string =>
            'eval "$(mise env)" names no command after it, the SessionStart hook wrote the mise environment to CLAUDE_ENV_FILE, run the command directly',
    },
    {
        word: ['mise'],
        when: (leaf: Leaf, command: string): boolean => _evalEnv(leaf, command, true),
        rewrite: _evalRewrite,
    },
    {
        word: ['nx', 'pnpm', 'dotnet', 'uv', 'pulumi', 'act', 'git'],
        when: (leaf: Leaf): boolean => _previewWord(leaf) && leaf.slice(1).some((word) => _previewFlags(leaf).includes(word.text)),
        deny: (leaf: Leaf, command: string): string =>
            `Preview flags are refused, a proof runs the target: ${_previewFlag(leaf).match<string>({ some: (flag) => _without(command, flag), none: () => command })}`,
    },
    {
        word: ['uv'],
        when: _pinnedUv,
        deny: (leaf: Leaf): string =>
            `Unpinned: uv add ${_first(leaf, _UV_PIN).match<string>({ some: (match) => _group(match, 'name'), none: () => '' })}, uv.lock alone pins versions`,
    },
    {
        word: ['pnpm'],
        when: _pinnedPnpm,
        deny: (leaf: Leaf): string =>
            `Unpinned: pnpm add ${_first(leaf, _PNPM_PIN).match<string>({ some: (match) => _group(match, 'name'), none: () => '' })}@catalog:, pnpm-workspace.yaml holds the version`,
    },
    {
        word: ['dotnet'],
        when: _pinnedDotnet,
        deny: (leaf: Leaf): string => `Unpinned: dotnet package add ${_packageName(leaf)}, Directory.Packages.props alone holds the version`,
    },
    {
        word: ['yq'],
        when: (leaf: Leaf): boolean => _word(leaf, 1) === 'r' || _word(leaf, 1) === 'w',
        deny: (): string => "yq r and yq w are the yq v3 forms, use yq '.expr' file",
    },
    {
        word: ['cat', 'head', 'strings', 'xxd', 'od', 'less', 'tail'],
        when: (leaf: Leaf): boolean => _has(leaf, _BINLOG),
        deny: (): string => BINLOG_DENY,
    },
    {
        word: [],
        when: _redirected,
        deny: (): string => OP_DENY,
    },
    {
        word: ['sleep'],
        when: (_leaf: Leaf, command: string): boolean => _allSleep(command),
        deny: (leaf: Leaf, command: string): string => `${_leafText(leaf, command)} waits on nothing, ${_WAIT}`,
    },
    {
        word: ['sleep'],
        when: (leaf: Leaf, command: string): boolean =>
            !_allSleep(command) && _depth(leaf, command) === 0 && toArray(_sleepSpan(leaf, command)).length > 0,
        rewrite: _sleepRewrite,
    },
    {
        word: ['ast-grep', 'pnpm', 'npx'],
        when: _rewritesSharedSnapshots,
        deny: (): string => _TEST_UPDATE_DENY,
    },
    {
        word: ['ast-grep', 'pnpm', 'npx'],
        when: _astGrepWhen(_isTest, (words) => !_texts(words).includes(_INCLUDE_OFF)),
        rewrite: _includeOff,
    },
    {
        word: ['ast-grep', 'pnpm', 'npx'],
        when: _astGrepWhen(_isRun, (words, command) => _typescriptWords(words).length > 0 && !_parsesElsewhere(words, command)),
        rewrite: _tsxRewrite,
    },
    {
        word: ['ast-grep', 'pnpm', 'npx'],
        when: _astGrepWhen(_isSearch, (words) => _hasWord(words, _UPDATE_ALL) && _matching(words, _JSON).length > 0),
        rewrite: _jsonDrop,
    },
    {
        word: ['ast-grep', 'pnpm', 'npx'],
        when: _astGrepWhen(_isUpdater, (words) => _hasWord(words, _UPDATE_ALL) && _hasWord(words, _INTERACTIVE)),
        rewrite: _interactiveDrop,
    },
    {
        word: ['ast-grep', 'pnpm', 'npx'],
        when: _astGrepWhen(_isSearch, (words, command) => _matching(words, _JSON_ARRAY).length > 0 && _countsLines(words, command)),
        rewrite: _streamRewrite,
    },
    {
        word: ['ast-grep', 'pnpm', 'npx'],
        when: _astGrepWhen(_isScan, (words) => toArray(_treeRule(words)).length > 0),
        rewrite: _ruleRewrite,
    },
    {
        word: ['ast-grep', 'pnpm', 'npx'],
        when: _astGrepWhen(_isTest, (words) => _bareIds(words).length > 0),
        rewrite: _filterRewrite,
    },
    {
        word: ['ast-grep', 'pnpm', 'npx'],
        when: _astGrepWhen(_isScan, (words) => _langWords(words).length > 0),
        rewrite: _langDrop,
    },
    {
        word: ['grep'],
        when: (): boolean => true,
        context: (): string => 'Load the ast-grep skill, rg is for literals and comments',
        once: 'ast-grep',
    },
    {
        word: ['rg', 'grep'],
        when: (leaf: Leaf): boolean => _has(leaf, _CSHARP),
        context: (): string => 'Load the dotnet-roslyn-codelens skill, Roslyn reads C# by its semantics and rg reads text',
        once: 'dotnet-roslyn-codelens',
    },
    {
        word: ['rg', 'grep'],
        when: (leaf: Leaf): boolean => _has(leaf, _MSBUILD),
        context: (): string => 'Load the dotnet-msbuild-evaluation skill, MSBuild evaluation reads project files and rg reads text',
        once: 'dotnet-msbuild-evaluation',
    },
    {
        word: ['rg', 'grep'],
        when: (leaf: Leaf): boolean => _has(leaf, _SYNTAX),
        context: (): string => 'Load the ast-grep skill, ast-grep reads code by its syntax tree and rg reads text',
        once: 'ast-grep',
    },
    {
        word: ['ls'],
        when: (leaf: Leaf): boolean => _has(leaf, _RECURSIVE_LS),
        context: (): string => 'Use tree <dir> to list every directory and file, -D for directories alone',
        once: 'tree',
    },
    {
        word: ['find'],
        when: (): boolean => true,
        context: (): string => 'Use fd for filesystem queries, fd <pattern> <dir>',
        once: 'fd',
    },
    {
        word: ['wc'],
        when: (leaf: Leaf): boolean => _texts(leaf).includes('-l'),
        context: (): string => 'Use loc <dir> for the line count with a complexity score per file',
        once: 'loc',
    },
    {
        word: ['gh'],
        when: (leaf: Leaf): boolean => _sub(leaf) !== '',
        context: (leaf: Leaf): string =>
            `Use the github MCP for gh ${_sub(leaf)}, gh serves the local checkout (pull requests from HEAD, checks, checkout, releases, secrets)`,
        once: 'github',
    },
    {
        word: ['dotnet'],
        when: _buildsWithoutBinlog,
        context: (): string => 'Add -bl to dotnet build and read the .binlog through the dotnet-msbuild-diagnostics skill',
        once: 'dotnet-msbuild-diagnostics',
    },
] as const satisfies readonly ShellRow[];

// --- [HITS] ----------------------------------------------------------------------------

const _leaves = leaves([...new Set(SHELL.flatMap((row) => row.word)), 'npm']);

const _applies = (row: ShellRow, leaf: Leaf): boolean => row.word.length === 0 || row.word.includes(_head(leaf));

const shellHits = (command: string): readonly Hit[] => {
    const parsed = _leaves(command);
    return SHELL.flatMap((row) => parsed.filter((leaf) => _applies(row, leaf) && row.when(leaf, command)).map((leaf): Hit => ({ row, leaf })));
};

// The once keys the adapter stamps as injected, beside pathSkills and toolSkills
const shellSkills = (command: string): readonly string[] => shellHits(command).flatMap((hit) => toArray(fromNullable(hit.row.once)));

// The first hit with the member, the optional call answers undefined on a row without it
const _denial = (hits: readonly Hit[], command: string): Option<string> =>
    fromNullable(hits.map((hit) => hit.row.deny?.(hit.leaf, command)).find((reason) => reason !== undefined));

const _rewriting = (hits: readonly Hit[], command: string): Option<Rewritten> =>
    fromNullable(hits.map((hit) => hit.row.rewrite?.(hit.leaf, command)).find((next) => next !== undefined));

// The first hit of each once key stands, and a key the session already injected yields no line
const _contexts = (hits: readonly Hit[], seen: ReadonlySet<string>): readonly string[] => [
    ...new Set(
        hits
            .filter(
                (hit, index) =>
                    hit.row.once === undefined || (!seen.has(hit.row.once) && hits.findIndex((other) => other.row.once === hit.row.once) === index),
            )
            .flatMap((hit) => toArray(map((line: (leaf: Leaf) => string) => line(hit.leaf))(fromNullable(hit.row.context)))),
    ),
];

// Rewrites re-read the command they produced, a rewrite row never matches its own output, and the budget bounds the recursion
const _decide = <E extends Command>(seen: ReadonlySet<string>, e: E, context: readonly string[], budget: number): Decision<E, unknown, string> => {
    const hits = shellHits(e.command);
    const settle = (): Decision<E, unknown, string> => rewrite(e, [...context, ..._contexts(hits, seen)]);
    return _denial(hits, e.command).match<Decision<E, unknown, string>>({
        some: deny,
        none: () =>
            _rewriting(hits, e.command).match<Decision<E, unknown, string>>({
                some: (next) =>
                    fromBoolean(budget > 0).match<Decision<E, unknown, string>>({
                        some: () => _decide(seen, { ...e, command: next.command }, [...context, next.context], budget - 1),
                        none: settle,
                    }),
                none: settle,
            }),
    });
};

// --- [RULES] ---------------------------------------------------------------------------

const shellRule =
    (seen: ReadonlySet<string>): (<E extends Command>(e: E) => Decision<E, unknown, string>) =>
    <E extends Command>(e: E): Decision<E, unknown, string> =>
        _decide(seen, e, [], _MAX_REWRITES);

// Every npm leaf takes the configured package manager by span, a leaf after && rewrites too
const packageManager =
    (name: string): (<E extends Command>(e: E) => Decision<E, unknown, string>) =>
    <E extends Command>(e: E): Decision<E, unknown, string> => {
        const splices = _leaves(e.command).flatMap((leaf) =>
            leaf
                .slice(0, 1)
                .filter((word) => basename(word.text) === 'npm')
                .map((word): Splice => ({ start: word.start, end: word.end, text: name })),
        );
        return fromBoolean(splices.length > 0).match<Decision<E, unknown, string>>({
            some: () => rewrite({ ...e, command: _splice(e.command, splices) }, [`Ran ${name} in place of npm`]),
            none: () => rewrite(e, []),
        });
    };

// Every timeout prefix of the command moves its duration to the Bash parameter, a wrapped shell body keeps its prefix leaf for the read
const commandTimeout = <E extends Timed>(e: E): Decision<E, unknown, string> =>
    liftPredicate<readonly Prefix[]>((prefixes) => prefixes.length > 0)(
        _leaves(e.command).flatMap((leaf) => toArray(_prefix(leaf, e.command))),
    ).match<Decision<E, unknown, string>>({
        some: (prefixes) => _timed(e, prefixes),
        none: () => rewrite(e, []),
    });

// Every nx run leaf of the command that carries a skip-cache flag, the pairs the adapter reads nx show project for
const nxTargets = (command: string): readonly NxTarget[] => _leaves(command).flatMap((leaf) => toArray(_nxTarget(leaf)));

// A target whose manifest states cache: false never reads the cache, the flag leaves and the context names the fact, true and an unread flag pass
const skipNxCache =
    (caches: NxCaches): (<E extends Command>(e: E) => Decision<E, unknown, string>) =>
    <E extends Command>(e: E): Decision<E, unknown, string> => {
        const dropped = nxTargets(e.command).filter((named) => caches[`${named.project}:${named.target}`] === false);
        return fromBoolean(dropped.length > 0).match<Decision<E, unknown, string>>({
            some: () =>
                rewrite(
                    {
                        ...e,
                        command: _splice(
                            e.command,
                            dropped.map((named) => _removal(named.flag)),
                        ),
                    },
                    dropped.map(
                        (named) => `Dropped ${named.flag.text}, ${named.project}:${named.target} declares cache: false and never reads the cache`,
                    ),
                ),
            none: () => rewrite(e, []),
        });
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Hit, NxCaches, NxTarget, Rewritten, ShellRow, Timed };
export { commandTimeout, nxTargets, packageManager, SHELL, shellHits, shellRule, shellSkills, skipNxCache };

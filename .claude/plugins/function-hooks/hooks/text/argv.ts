// Argv leaves of a shell command with source spans and no verdict

// --- [IMPORTS] -------------------------------------------------------------------------

import {
    flatMap,
    fromBoolean,
    fromNullable,
    fromPredicate,
    getOrElse,
    liftPredicate,
    map,
    none,
    type Option,
    some,
    toArray,
} from '../composition/option.ts';
import { basename } from './path.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Word {
    readonly text: string;
    readonly start: number;
    readonly end: number;
}

type Leaf = readonly Word[];

interface Span {
    readonly start: number;
    readonly end: number;
}

interface Frame {
    readonly text: string;
    readonly depth: number;
    readonly offset: number;
    readonly guarded: readonly string[];
}

interface Scan {
    readonly quoted: boolean;
    readonly single: boolean;
    readonly escaped: boolean;
    readonly dollar: boolean;
    readonly level: number;
    readonly opened: number;
    readonly bodies: readonly Span[];
}

interface Building {
    readonly text: string;
    readonly start: number;
    readonly end: number;
}

interface Token {
    readonly separator: boolean;
    readonly word: Word;
}

interface Lexing {
    readonly tokens: readonly Token[];
    readonly current: Option<Building>;
}

interface Grouping {
    readonly leaves: readonly Leaf[];
    readonly argv: readonly Word[];
}

type ScanClass = 'body' | 'single' | 'escaped' | 'escape' | 'double' | 'quote' | 'open' | 'other';
type SegmentKind = 'space' | 'punctuation' | 'comment' | 'single' | 'singleOpen' | 'double' | 'doubleOpen' | 'escape' | 'plain';
type TokenKind = 'separator' | 'word';
type HeadClass = 'shell' | 'interpreter' | 'plain';

// --- [CONSTANTS] -----------------------------------------------------------------------

const INTERPRETER = '<inline-interpreter>';
const _MAX_DEPTH = 8;
// Escaped backticks are literals and open no substitution
const _BACKTICK = /(?<!\\)`(?<body>(?:[^`\\]|\\.)*)`/gu;
const _CONTINUE = /\\\n/gu;
// Quoted delimiters expand nothing in the body
const _HEREDOC = /<<-?\s*(?<quote>['"]?)(?<delimiter>\w+)\k<quote>\n.*?^\t*\k<delimiter>$/gmsu;
const _PROCESS_CALL = /subprocess|child_process|os\.(?:system|exec|spawn|popen)|\b(?:system|exec|spawn|popen|qx|Open3)\b|`|%x/u;
// List, call, and string punctuation around a process argv in interpreter code
const _LITERAL = /[[\](){}"',]/gu;
const _IFS = /\$\{IFS[^}]*\}|\$IFS/gu;
const _ENV_ASSIGN = /^[A-Za-z_][A-Za-z0-9_]*=/u;
const _DIGITS = /^\d+$/u;
const _SEPARATOR = /^(?:\(|\)|[;&|\n\r]+)$/u;
const _DOUBLE_ESCAPE = /\\(?<escaped>["\\])/gu;
// The segment classes of the shlex posix lexer
const _SEGMENT =
    /(?<space>[ \t\f\v]+)|(?<punctuation>[;&|<>()\n\r]+)|(?<comment>#[^\n]*\n?)|(?<single>'[^']*')|(?<singleOpen>'[^']*)|(?<double>"(?:[^"\\]|\\.)*")|(?<doubleOpen>"(?:[^"\\]|\\.)*)|(?<escape>\\(?:.|$))|(?<plain>[^ \t\f\v;&|<>()\n\r'"\\#]+)/gsu;
const _SEGMENT_KINDS: readonly SegmentKind[] = ['space', 'punctuation', 'comment', 'single', 'singleOpen', 'double', 'doubleOpen', 'escape', 'plain'];
const _SINGLE_KINDS: readonly SegmentKind[] = ['single', 'singleOpen'];
const _INLINE_FLAGS: readonly string[] = ['--eval', '--command', '--print', '--execute'];
const _INTERPRETERS: readonly string[] = ['python', 'node', 'ruby', 'perl'];
const _SHELLS: readonly string[] = ['sh', 'bash', 'zsh', 'dash', 'ksh', 'eval'];
const _SUBCOMMANDS: readonly string[] = ['run', 'exec', 'tool', 'x'];
const _VALUE_OPTS: readonly string[] = ['-u', '-I', '-n', '-g', '--user', '--replace'];
const _RUNNERS: readonly string[] = ['uv', 'npm', 'npx', 'pnpm', 'poetry', 'hatch'];
const _WRAPPERS: readonly string[] = [
    'sudo',
    'doas',
    'env',
    'command',
    'nice',
    'nohup',
    'stdbuf',
    'timeout',
    'time',
    'xargs',
    'caffeinate',
    'arch',
    'setsid',
    ..._RUNNERS,
];
const _DELTA: Readonly<Partial<Record<string, number>>> = { '(': 1, ')': -1 };
const _INITIAL_SCAN: Scan = { quoted: false, single: false, escaped: false, dollar: false, level: 0, opened: 0, bodies: [] };

// --- [OPERATIONS] ----------------------------------------------------------------------

// Every blanking keeps the text length, each index into the original command survives
const _blank = (text: string, pattern: RegExp): string => text.replace(pattern, (match: string): string => ' '.repeat(match.length));

const _blankSpan = (text: string, span: Span): string => text.slice(0, span.start) + ' '.repeat(span.end - span.start) + text.slice(span.end);

// --- [SUBSTITUTIONS] -------------------------------------------------------------------

const _scanClass = (scan: Scan, text: string, index: number): ScanClass => {
    const character = text[index];
    return (
        (scan.level > 0 && 'body') ||
        (scan.single && 'single') ||
        (scan.escaped && 'escaped') ||
        (character === '\\' && 'escape') ||
        (character === '"' && 'double') ||
        (character === "'" && !scan.quoted && text.indexOf("'", index + 1) >= 0 && 'quote') ||
        (character === '(' && scan.dollar && 'open') ||
        'other'
    );
};

const _closeBody = (scan: Scan, index: number): Scan =>
    fromBoolean(scan.level === 0).match<Scan>({
        some: () => ({ ...scan, bodies: [...scan.bodies, { start: scan.opened, end: index }] }),
        none: () => scan,
    });

// Scan for $( bodies, inside a body the parentheses count alone
const _SCAN: Readonly<Record<ScanClass, (scan: Scan, text: string, index: number) => Scan>> = {
    body: (scan, text, index): Scan => _closeBody({ ...scan, dollar: false, level: scan.level + (_DELTA[text[index]] ?? 0) }, index),
    single: (scan, text, index): Scan => ({ ...scan, dollar: false, single: text[index] !== "'" }),
    escaped: (scan): Scan => ({ ...scan, dollar: false, escaped: false }),
    escape: (scan): Scan => ({ ...scan, dollar: false, escaped: true }),
    double: (scan): Scan => ({ ...scan, dollar: false, quoted: !scan.quoted }),
    quote: (scan): Scan => ({ ...scan, dollar: false, single: true }),
    open: (scan, _text, index): Scan => ({ ...scan, dollar: false, level: 1, opened: index + 1 }),
    other: (scan, text, index): Scan => ({ ...scan, dollar: text[index] === '$' }),
};

// Unclosed bodies run to the end of the text
const _substitutions = (text: string): readonly Span[] => {
    const scan = text.split('').reduce<Scan>((state, _character, index) => _SCAN[_scanClass(state, text, index)](state, text, index), _INITIAL_SCAN);
    return fromBoolean(scan.level > 0).match<readonly Span[]>({
        some: () => [...scan.bodies, { start: scan.opened, end: text.length }],
        none: () => scan.bodies,
    });
};

// --- [LEXER] ---------------------------------------------------------------------------

const _segmentKind = (match: RegExpMatchArray): SegmentKind => _SEGMENT_KINDS.find((kind) => match.groups?.[kind] !== undefined) ?? 'plain';

// Single quotes expand nothing, a backtick inside them is text and the blanked runs keep every index
const _literal = (text: string): string =>
    [...text.matchAll(_SEGMENT)]
        .filter((match) => _SINGLE_KINDS.includes(_segmentKind(match)))
        .reduce((current, match) => _blankSpan(current, { start: match.index ?? 0, end: (match.index ?? 0) + match[0].length }), text);

const _flush = (lexing: Lexing): Lexing =>
    lexing.current.match<Lexing>({
        some: (building) => ({ tokens: [...lexing.tokens, { separator: false, word: building }], current: none() }),
        none: () => lexing,
    });

const _extend = (lexing: Lexing, text: string, start: number, end: number): Lexing => ({
    ...lexing,
    current: some(
        lexing.current.match<Building>({
            some: (building) => ({ text: building.text + text, start: building.start, end }),
            none: () => ({ text, start, end }),
        }),
    ),
});

const _punctuation = (lexing: Lexing, text: string, start: number, end: number): Lexing => ({
    tokens: [..._flush(lexing).tokens, { separator: _SEPARATOR.test(text), word: { text, start, end } }],
    current: none(),
});

// Inside double quotes a backslash escapes a quote or a backslash alone, as shlex reads it
const _LEX: Readonly<Record<SegmentKind, (lexing: Lexing, text: string, start: number, end: number) => Lexing>> = {
    space: _flush,
    comment: _flush,
    punctuation: _punctuation,
    single: (lexing, text, start, end): Lexing => _extend(lexing, text.slice(1, -1), start, end),
    singleOpen: (lexing, text, start, end): Lexing => _extend(lexing, text.slice(1), start, end),
    double: (lexing, text, start, end): Lexing => _extend(lexing, text.slice(1, -1).replace(_DOUBLE_ESCAPE, '$<escaped>'), start, end),
    doubleOpen: (lexing, text, start, end): Lexing => _extend(lexing, text.slice(1).replace(_DOUBLE_ESCAPE, '$<escaped>'), start, end),
    escape: (lexing, text, start, end): Lexing => _extend(lexing, text.slice(1), start, end),
    plain: (lexing, text, start, end): Lexing => _extend(lexing, text, start, end),
};

const _tokens = (text: string, offset: number): readonly Token[] =>
    _flush(
        [...text.matchAll(_SEGMENT)].reduce<Lexing>(
            (lexing, match) => {
                const start = offset + (match.index ?? 0);
                return _LEX[_segmentKind(match)](lexing, match[0], start, start + match[0].length);
            },
            { tokens: [], current: none() },
        ),
    ).tokens;

// --- [RESOLUTION] ----------------------------------------------------------------------

const _stripOptions = (argv: readonly Word[]): readonly Word[] =>
    fromNullable(argv[0]).match<readonly Word[]>({
        some: (head) =>
            fromBoolean(head.text.includes('=') || _DIGITS.test(head.text) || _SUBCOMMANDS.includes(head.text) || head.text.startsWith('-')).match<
                readonly Word[]
            >({
                some: () => _stripOptions(argv.slice((_VALUE_OPTS.includes(head.text) && 2) || 1)),
                none: () => argv,
            }),
        none: () => argv,
    });

// The words past the leading env assignments, the view a rule reads when a wrapper word is its subject
const pastAssignments = (argv: readonly Word[]): readonly Word[] =>
    argv.slice(getOrElse(() => argv.length)(liftPredicate<number>((index) => index >= 0)(argv.findIndex((word) => !_ENV_ASSIGN.test(word.text)))));

// Env assignments, wrappers, and runners leave with their options, the view the git guard reads
const strip = (argv: readonly Word[]): readonly Word[] =>
    fromNullable(argv[0]).match<readonly Word[]>({
        some: (head) =>
            fromBoolean(_ENV_ASSIGN.test(head.text) || _WRAPPERS.includes(head.text)).match<readonly Word[]>({
                some: () => strip(_stripOptions(argv.slice(1))),
                none: () => argv,
            }),
        none: () => argv,
    });

// The operand after a short, clustered, or long inline flag ending in the letter, a blanked substitution is spaces and no body
const _flagOperand = (argv: readonly Word[], letter: string): Option<Word> => {
    const hit = argv.findIndex(
        (word) => _INLINE_FLAGS.includes(word.text) || (word.text.startsWith('-') && !word.text.startsWith('--') && word.text.endsWith(letter)),
    );
    return flatMap(() =>
        fromPredicate((word: Word | undefined): word is Word => word !== undefined && word.text.trim() !== '')(
            argv.slice(hit + 1).find((word) => word.text !== '--'),
        ),
    )(fromBoolean(hit >= 0));
};

const _interpreterBody = (argv: readonly Word[]): Option<Word> =>
    _flagOperand(argv, 'c').match<Option<Word>>({ some, none: () => _flagOperand(argv, 'e') });

const _sentinel = (head: Word): Leaf => [
    { ...head, text: 'git' },
    { ...head, text: INTERPRETER },
];

// Body words that held escapes or quote runs are not verbatim in the text, their spans then start at the word
const _bodyFrame = (frame: Frame, body: Word): Frame => {
    const local = frame.text.slice(body.start - frame.offset, body.end - frame.offset).indexOf(body.text);
    return { ...frame, depth: frame.depth + 1, offset: (local >= 0 && body.start + local) || body.start };
};

const _slices =
    (guarded: readonly string[]) =>
    (leaf: Leaf): readonly Leaf[] =>
        leaf.flatMap((word, index) => toArray(liftPredicate<Leaf>(() => guarded.includes(basename(word.text)))(leaf.slice(index))));

// No script and no stdin runs nothing
const _shellFallback = (argv: readonly Word[], head: Word): readonly Leaf[] =>
    toArray(liftPredicate<Leaf>(() => argv.slice(1).some((word) => word.text === '-s' || !word.text.startsWith('-')))(_sentinel(head)));

const _shell = (argv: readonly Word[], head: Word, frame: Frame): readonly Leaf[] =>
    _flagOperand(argv, 'c').match<readonly Leaf[]>({
        some: (body) =>
            fromBoolean(frame.depth < _MAX_DEPTH).match<readonly Leaf[]>({
                some: () => _leaves(body.text, _bodyFrame(frame, body)),
                none: () => _shellFallback(argv, head),
            }),
        none: () => _shellFallback(argv, head),
    });

// Code with no process call runs no git, the word alone is text
const _interpreter = (argv: readonly Word[], head: Word, frame: Frame): readonly Leaf[] =>
    _interpreterBody(argv).match<readonly Leaf[]>({
        some: (body) =>
            fromBoolean(_PROCESS_CALL.test(body.text)).match<readonly Leaf[]>({
                some: () =>
                    fromBoolean(frame.depth < _MAX_DEPTH).match<readonly Leaf[]>({
                        some: () => _leaves(body.text.replace(_LITERAL, ' '), _bodyFrame(frame, body)).flatMap(_slices(frame.guarded)),
                        none: () => [_sentinel(head)],
                    }),
                none: () => [],
            }),
        none: () => [],
    });

// Shell words first, then interpreter words with an inline body, else a plain leaf
const _headClass = (argv: readonly Word[], head: Word): HeadClass => {
    const name = basename(head.text);
    return getOrElse((): HeadClass => 'plain')(
        fromNullable(
            [
                liftPredicate<HeadClass>(() => _SHELLS.includes(name))('shell'),
                flatMap(() => map((): HeadClass => 'interpreter')(_interpreterBody(argv)))(
                    fromBoolean(_INTERPRETERS.some((prefix) => name.startsWith(prefix))),
                ),
            ].flatMap(toArray)[0],
        ),
    );
};

// The raw leaf of a wrapped shell, interpreter, or nothing, the prefix a rule over the wrapper words reads after the body
const _wrapped = (argv: readonly Word[], raw: readonly Word[]): readonly Leaf[] => toArray(liftPredicate<Leaf>(() => argv.length < raw.length)(raw));

// Plain leaves keep their env assignments and wrappers, the git guard strips them and the shell rows read the runner word
const _HEAD: Readonly<Record<HeadClass, (argv: readonly Word[], head: Word, frame: Frame, raw: readonly Word[]) => readonly Leaf[]>> = {
    shell: (argv, head, frame, raw): readonly Leaf[] => [..._shell(argv, head, frame), ..._wrapped(argv, raw)],
    interpreter: (argv, head, frame, raw): readonly Leaf[] => [..._interpreter(argv, head, frame), ..._wrapped(argv, raw)],
    plain: (_argv, _head, _frame, raw): readonly Leaf[] => [raw],
};

const _resolve = (raw: readonly Word[], frame: Frame): readonly Leaf[] => {
    const argv = strip(raw);
    return fromNullable(argv[0]).match<readonly Leaf[]>({
        some: (head) => _HEAD[_headClass(argv, head)](argv, head, frame, raw),
        none: () => _wrapped(argv, raw),
    });
};

const _GROUP: Readonly<Record<TokenKind, (grouping: Grouping, token: Token, frame: Frame) => Grouping>> = {
    separator: (grouping, _token, frame): Grouping => ({ leaves: [...grouping.leaves, ..._resolve(grouping.argv, frame)], argv: [] }),
    word: (grouping, token): Grouping => ({ ...grouping, argv: [...grouping.argv, token.word] }),
};

const _grouped = (tokens: readonly Token[], frame: Frame): readonly Leaf[] => {
    const grouping = tokens.reduce<Grouping>((state, token) => _GROUP[(token.separator && 'separator') || 'word'](state, token, frame), {
        leaves: [],
        argv: [],
    });
    return [...grouping.leaves, ..._resolve(grouping.argv, frame)];
};

// Substitution bodies parse first and the rest splits quote-aware, unquoted heredoc bodies keep their substitutions for the scan
const _leaves = (text: string, frame: Omit<Frame, 'text'>): readonly Leaf[] => {
    const unquoted = text.replace(_HEREDOC, (match: string, quote: string): string =>
        fromBoolean(quote !== '').match<string>({ some: () => ' '.repeat(match.length), none: () => match }),
    );
    const continued = _blank(unquoted, _CONTINUE);
    const backticks = [..._literal(continued).matchAll(_BACKTICK)].map(
        (match): Span => ({ start: (match.index ?? 0) + 1, end: (match.index ?? 0) + match[0].length - 1 }),
    );
    const flat = backticks.reduce((current, span) => _blankSpan(current, { start: span.start - 1, end: span.end + 1 }), continued);
    const substitutions = _substitutions(flat);
    const lexed = _blank(
        _blank(
            substitutions.reduce(
                (current, span) => _blankSpan(current, { start: span.start - 2, end: Math.min(span.end + 1, current.length) }),
                flat,
            ),
            _HEREDOC,
        ),
        _IFS,
    );
    const bodies = [...backticks.map((span) => ({ span, source: continued })), ...substitutions.map((span) => ({ span, source: flat }))];
    const inner = fromBoolean(frame.depth < _MAX_DEPTH).match<readonly Leaf[]>({
        some: () =>
            bodies.flatMap(({ span, source }) =>
                _leaves(source.slice(span.start, span.end), { ...frame, depth: frame.depth + 1, offset: frame.offset + span.start }),
            ),
        none: () => [],
    });
    return [...inner, ..._grouped(_tokens(lexed, frame.offset), { ...frame, text: lexed })];
};

const leaves =
    (guarded: readonly string[]) =>
    (command: string): readonly Leaf[] =>
        _leaves(command, { depth: 0, offset: 0, guarded });

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Leaf, Word };
export { INTERPRETER, leaves, pastAssignments, strip };

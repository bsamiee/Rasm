// Words of every simple command in a shell command, each with its span in the source text

// --- [IMPORTS] -------------------------------------------------------------------------

import { basename } from './path.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Word {
    readonly text: string;
    readonly start: number;
    readonly end: number;
}

// One simple command as words, raw with its env assignments and wrappers
type Argv = readonly Word[];

interface Span {
    readonly start: number;
    readonly end: number;
}

// The text under parse at one nesting level, its offset into the source, and the words a slice inside interpreter code starts at
interface Frame {
    readonly text: string;
    readonly depth: number;
    readonly offset: number;
    readonly guarded: readonly string[];
}

interface Token {
    readonly separator: boolean;
    readonly word: Word;
}

// A substitution or backtick body with the text its span indexes
interface Body {
    readonly span: Span;
    readonly source: string;
}

// The text the lexer tokenizes, every body blanked to spaces, and the bodies that parse on their own
interface Prepared {
    readonly lexed: string;
    readonly bodies: readonly Body[];
}

type SegmentKind = 'space' | 'punctuation' | 'comment' | 'single' | 'singleOpen' | 'double' | 'doubleOpen' | 'escape' | 'plain';

// The state of the substitution scan: the paren level inside a $( body with its opening index, the quoting outside one, and a $ just read
interface Scan {
    readonly level: number;
    readonly opened: number;
    readonly single: boolean;
    readonly quoted: boolean;
    readonly escaped: boolean;
    readonly dollar: boolean;
}

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
// The text a quoted or escaped segment contributes to its word, inside double quotes a backslash escapes a quote or a backslash alone
const _UNQUOTE: Readonly<Record<SegmentKind, (raw: string) => string>> = {
    space: (raw): string => raw,
    punctuation: (raw): string => raw,
    comment: (raw): string => raw,
    single: (raw): string => raw.slice(1, -1),
    singleOpen: (raw): string => raw.slice(1),
    double: (raw): string => raw.slice(1, -1).replace(_DOUBLE_ESCAPE, '$<escaped>'),
    doubleOpen: (raw): string => raw.slice(1).replace(_DOUBLE_ESCAPE, '$<escaped>'),
    escape: (raw): string => raw.slice(1),
    plain: (raw): string => raw,
};
const _INLINE_FLAGS: readonly string[] = ['--eval', '--command', '--print', '--execute'];
const _INTERPRETERS: readonly string[] = ['python', 'node', 'ruby', 'perl'];
const _SHELLS: readonly string[] = ['sh', 'bash', 'zsh', 'dash', 'ksh', 'eval'];
const _SUBCOMMANDS: readonly string[] = ['run', 'exec', 'tool', 'x'];
const _VALUE_OPTS: readonly string[] = ['-u', '-I', '-n', '-g', '--user', '--replace'];
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
    'uv',
    'npm',
    'npx',
    'pnpm',
    'poetry',
    'hatch',
];
const _PAREN_DELTA: Readonly<Partial<Record<string, number>>> = { '(': 1, ')': -1 };
const _INITIAL_SCAN: Scan = { level: 0, opened: 0, single: false, quoted: false, escaped: false, dollar: false };

// --- [BLANKING] ------------------------------------------------------------------------

// Every blanking keeps the text length, each index into the original command survives
const _blank = (text: string, pattern: RegExp): string => text.replace(pattern, (match: string): string => ' '.repeat(match.length));

const _blankSpan = (text: string, span: Span): string => text.slice(0, span.start) + ' '.repeat(span.end - span.start) + text.slice(span.end);

// One character outside a substitution body: quotes and escapes toggle, and $( opens a body at level one
const _outside = (scan: Scan, text: string, index: number): Scan => {
    const character = text[index];
    if (scan.single) {
        return { ...scan, single: character !== "'", dollar: false };
    }
    if (scan.escaped || character === '\\') {
        return { ...scan, escaped: !scan.escaped, dollar: false };
    }
    if (character === '"') {
        return { ...scan, quoted: !scan.quoted, dollar: false };
    }
    if (character === "'" && !scan.quoted && text.indexOf("'", index + 1) >= 0) {
        return { ...scan, single: true, dollar: false };
    }
    if (character === '(' && scan.dollar) {
        return { ...scan, level: 1, opened: index + 1, dollar: false };
    }
    return { ...scan, dollar: character === '$' };
};

// The spans of $( bodies, inside a body the parentheses count alone, and an unclosed body runs to the end of the text
const _substitutions = (text: string): readonly Span[] => {
    const bodies: Span[] = [];
    let scan = _INITIAL_SCAN;
    for (let index = 0; index < text.length; index += 1) {
        if (scan.level === 0) {
            scan = _outside(scan, text, index);
        } else {
            scan = { ...scan, level: scan.level + (_PAREN_DELTA[text[index] ?? ''] ?? 0) };
            if (scan.level === 0) {
                bodies.push({ start: scan.opened, end: index });
            }
        }
    }
    return scan.level > 0 ? [...bodies, { start: scan.opened, end: text.length }] : bodies;
};

// --- [LEXER] ---------------------------------------------------------------------------

const _segmentKind = (match: RegExpMatchArray): SegmentKind => _SEGMENT_KINDS.find((kind) => match.groups?.[kind] !== undefined) ?? 'plain';

// Single quotes expand nothing, a backtick inside them is text and the blanked runs keep every index
const _literal = (text: string): string =>
    [...text.matchAll(_SEGMENT)]
        .filter((match) => ['single', 'singleOpen'].includes(_segmentKind(match)))
        .reduce((current, match) => _blankSpan(current, { start: match.index ?? 0, end: (match.index ?? 0) + match[0].length }), text);

const _tokens = (text: string, offset: number): readonly Token[] => {
    const tokens: Token[] = [];
    let current: Word | undefined;
    const flush = (): void => {
        if (current !== undefined) {
            tokens.push({ separator: false, word: current });
            current = undefined;
        }
    };
    for (const match of text.matchAll(_SEGMENT)) {
        const kind = _segmentKind(match);
        const start = offset + (match.index ?? 0);
        const end = start + match[0].length;
        if (kind === 'space' || kind === 'comment') {
            flush();
        } else if (kind === 'punctuation') {
            flush();
            tokens.push({ separator: _SEPARATOR.test(match[0]), word: { text: match[0], start, end } });
        } else {
            const piece = _UNQUOTE[kind](match[0]);
            current = current === undefined ? { text: piece, start, end } : { text: current.text + piece, start: current.start, end };
        }
    }
    flush();
    return tokens;
};

// --- [RESOLUTION] ----------------------------------------------------------------------

// The words past a wrapper's own options and subcommand, the operand of a value-taking option leaves with it
const _stripOptions = (argv: Argv): Argv => {
    const [head] = argv;
    if (
        head === undefined ||
        !(head.text.includes('=') || _DIGITS.test(head.text) || _SUBCOMMANDS.includes(head.text) || head.text.startsWith('-'))
    ) {
        return argv;
    }
    return _stripOptions(argv.slice(_VALUE_OPTS.includes(head.text) ? 2 : 1));
};

// The words past the leading env assignments
const pastAssignments = (argv: Argv): Argv => {
    const index = argv.findIndex((word) => !_ENV_ASSIGN.test(word.text));
    return index < 0 ? [] : argv.slice(index);
};

// Env assignments, wrappers, and runners leave with their options, the view the git guard reads
const strip = (argv: Argv): Argv => {
    const [head] = argv;
    return head !== undefined && (_ENV_ASSIGN.test(head.text) || _WRAPPERS.includes(head.text)) ? strip(_stripOptions(argv.slice(1))) : argv;
};

// The operand after a short, clustered, or long inline flag ending in the letter, a blanked substitution is spaces and no body
const _flagOperand = (argv: Argv, letter: string): Word | undefined => {
    const hit = argv.findIndex(
        (word) => _INLINE_FLAGS.includes(word.text) || (word.text.startsWith('-') && !word.text.startsWith('--') && word.text.endsWith(letter)),
    );
    const operand = hit < 0 ? undefined : argv.slice(hit + 1).find((word) => word.text !== '--');
    return operand !== undefined && operand.text.trim() !== '' ? operand : undefined;
};

const _interpreterBody = (argv: Argv): Word | undefined => _flagOperand(argv, 'c') ?? _flagOperand(argv, 'e');

// The argv of a shell or interpreter whose body the parse cannot read, the guard's row over the interpreter word refuses it
const _sentinel = (head: Word): Argv => [
    { ...head, text: 'git' },
    { ...head, text: INTERPRETER },
];

// Body words that held escapes or quote runs are not verbatim in the text, their spans then start at the word
const _bodyFrame = (frame: Frame, body: Word): Frame => {
    const local = frame.text.slice(body.start - frame.offset, body.end - frame.offset).indexOf(body.text);
    return { ...frame, depth: frame.depth + 1, offset: local >= 0 ? body.start + local : body.start };
};

// The argv from each guarded word onward, the process calls inside interpreter code
const _slices =
    (guarded: readonly string[]): ((argv: Argv) => readonly Argv[]) =>
    (argv: Argv): readonly Argv[] =>
        argv.flatMap((word, index) => (guarded.includes(basename(word.text)) ? [argv.slice(index)] : []));

// A shell parses its -c body, and one with a script or stdin runs code the parse cannot read
const _shell = (argv: Argv, head: Word, frame: Frame): readonly Argv[] => {
    const body = _flagOperand(argv, 'c');
    if (body !== undefined && frame.depth < _MAX_DEPTH) {
        return _parse(body.text, _bodyFrame(frame, body));
    }
    return argv.slice(1).some((word) => word.text === '-s' || !word.text.startsWith('-')) ? [_sentinel(head)] : [];
};

// Interpreter code with no process call runs no command, and code past the depth is the sentinel
const _interpreter = (body: Word, head: Word, frame: Frame): readonly Argv[] => {
    if (!_PROCESS_CALL.test(body.text)) {
        return [];
    }
    return frame.depth < _MAX_DEPTH
        ? _parse(body.text.replace(_LITERAL, ' '), _bodyFrame(frame, body)).flatMap(_slices(frame.guarded))
        : [_sentinel(head)];
};

// A plain argv keeps its assignments and wrappers, a wrapped shell or interpreter yields its body's argvs then its own raw words
const _resolve = (raw: Argv, frame: Frame): readonly Argv[] => {
    const argv = strip(raw);
    const [head] = argv;
    const wrapped = argv.length < raw.length ? [raw] : [];
    if (head === undefined) {
        return wrapped;
    }
    const name = basename(head.text);
    if (_SHELLS.includes(name)) {
        return [..._shell(argv, head, frame), ...wrapped];
    }
    const body = _interpreterBody(argv);
    if (body !== undefined && _INTERPRETERS.some((prefix) => name.startsWith(prefix))) {
        return [..._interpreter(body, head, frame), ...wrapped];
    }
    return [raw];
};

// The argvs between separator tokens, each resolved
const _grouped = (tokens: readonly Token[], frame: Frame): readonly Argv[] => {
    const argvs: Argv[] = [];
    let words: Word[] = [];
    for (const token of tokens) {
        if (token.separator) {
            argvs.push(..._resolve(words, frame));
            words = [];
        } else {
            words.push(token.word);
        }
    }
    return [...argvs, ..._resolve(words, frame)];
};

// Quoted heredoc bodies blank first, then backticks and $( bodies, and the lexed text keeps every index of the original
const _prepare = (text: string): Prepared => {
    const unquoted = text.replace(_HEREDOC, (match: string, quote: string): string => (quote === '' ? match : ' '.repeat(match.length)));
    const continued = _blank(unquoted, _CONTINUE);
    const backticks = [..._literal(continued).matchAll(_BACKTICK)].map(
        (match): Span => ({ start: (match.index ?? 0) + 1, end: (match.index ?? 0) + match[0].length - 1 }),
    );
    const flat = backticks.reduce((current, span) => _blankSpan(current, { start: span.start - 1, end: span.end + 1 }), continued);
    const substitutions = _substitutions(flat);
    const blanked = substitutions.reduce(
        (current, span) => _blankSpan(current, { start: span.start - 2, end: Math.min(span.end + 1, current.length) }),
        flat,
    );
    return {
        lexed: _blank(_blank(blanked, _HEREDOC), _IFS),
        bodies: [...backticks.map((span): Body => ({ span, source: continued })), ...substitutions.map((span): Body => ({ span, source: flat }))],
    };
};

// Substitution bodies parse first and the rest splits quote-aware, unquoted heredoc bodies keep their substitutions for the scan
const _parse = (text: string, frame: Omit<Frame, 'text'>): readonly Argv[] => {
    const { lexed, bodies } = _prepare(text);
    const inner =
        frame.depth < _MAX_DEPTH
            ? bodies.flatMap(({ span, source }) =>
                  _parse(source.slice(span.start, span.end), { ...frame, depth: frame.depth + 1, offset: frame.offset + span.start }),
              )
            : [];
    return [...inner, ..._grouped(_tokens(lexed, frame.offset), { ...frame, text: lexed })];
};

// Every simple command of a shell command, with the guarded words that start an argv of their own inside interpreter code
const parse =
    (guarded: readonly string[]): ((command: string) => readonly Argv[]) =>
    (command: string): readonly Argv[] =>
        _parse(command, { depth: 0, offset: 0, guarded });

// Whether the command's own text opens a subshell or a group, a paren outside quotes and substitutions
const hasGroup = (command: string): boolean => _tokens(_prepare(command).lexed, 0).some((token) => token.word.text === '(');

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Argv, Span, Word };
export { hasGroup, INTERPRETER, parse, pastAssignments, strip };

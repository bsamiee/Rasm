import refa from 'refa';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MAXIMUM = 0x10_ff_ff;
const ASCII_MAXIMUM = 0x7f;
const NEWLINE = 0x0a;
const CASELESS = { unicode: true, unicodeSets: false, ignoreCase: true };
const UNICODE = { unicode: true, unicodeSets: false, ignoreCase: false };
const ASCII = refa.CharSet.fromRange(MAXIMUM, { min: 0, max: ASCII_MAXIMUM });
const FLAGS = new Set(['i', 'm', 's', 'x', 'u', 'U', 'R']);
const CONTROLS = { a: 0x07, f: 0x0c, n: NEWLINE, r: 0x0d, t: 0x09, v: 0x0b };
const WIDTHS = { x: 2, u: 4, U: 8 };
const ASSERTIONS = new Set(['A', 'z', 'b', 'B', '<', '>']);
const QUANTIFIERS = { '*': [0, Number.POSITIVE_INFINITY], '+': [1, Number.POSITIVE_INFINITY], '?': [0, 1] };
const PERL = { d: ['\\p{Nd}', '[0-9]'], s: ['\\p{White_Space}', '[\\t\\n\\v\\f\\r ]'], w: ['[\\p{Alphabetic}\\p{M}\\p{Nd}\\p{Pc}\\p{Join_Control}]', '[0-9A-Za-z_]'] };
const POSIX = {
    alnum: '[0-9A-Za-z]',
    alpha: '[A-Za-z]',
    ascii: '[\\0-\\x7F]',
    blank: '[\\t ]',
    cntrl: '[\\0-\\x1F\\x7F]',
    digit: '[0-9]',
    graph: '[!-~]',
    lower: '[a-z]',
    print: '[ -~]',
    punct: '[!-\\/:-@\\[-`\\{-~]',
    space: '[\\t\\n\\v\\f\\r ]',
    upper: '[A-Z]',
    word: '[0-9A-Za-z_]',
    xdigit: '[0-9A-Fa-f]',
};
const OPERATORS = { '&&': (left, right) => left.intersect(right), '--': (left, right) => left.without(right), '~~': (left, right) => left.union(right).without(left.intersect(right)) };
const PROPERTY = /^(?<caret>\^?)(?<body>.*)$/su;
const ASSIGNMENT = /^(?<key>.*?)(?<inequality>!?)[=:](?<value>.*)$/su;
const WHITESPACE = /^\p{White_Space}$/u;
const HEXADECIMAL = /^[0-9A-Fa-f]{1,8}$/u;
const PUNCTUATION = /^[ !-/:-;=?-@[-`{-~]$/u;
const POSIX_CLASS = /^\[:(?<inverted>\^?)(?<name>[a-z]+):\]/u;
const REPETITION = /^\{\p{White_Space}*(?<low>[0-9]+)\p{White_Space}*(?<comma>,?)\p{White_Space}*(?<high>[0-9]*)\p{White_Space}*\}/u;
const MODIFIERS = /^\?(?<letters>[A-Za-z-]*)(?<closer>[:)])/u;
const NAMED = /^\?P?</u;
const ESCAPES = [
    ['perl', (letter) => letter.toLowerCase() in PERL],
    ['property', (letter) => letter.toLowerCase() === 'p'],
    ['hex', (letter) => letter in WIDTHS],
    ['control', (letter) => letter in CONTROLS],
    ['assertion', (letter, bracketed) => !bracketed && ASSERTIONS.has(letter)],
    ['punctuation', RegExp.prototype.test.bind(PUNCTUATION)],
];

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [CHARACTERS]

const fail = (message) => {
    throw new SyntaxError(message);
};
const memo = (derive) => {
    const cache = new Map();
    return (key) => (cache.has(key) ? cache.get(key) : cache.set(key, derive(key)).get(key));
};
const compiles = (source) => {
    try {
        return new RegExp(source, 'u') instanceof RegExp;
    } catch {
        return false;
    }
};
const scanned = memo((source) => {
    const matches = RegExp.prototype.test.bind(new RegExp(`^${source}$`, 'u'));
    return refa.CharSet.fromCharacters(
        MAXIMUM,
        Array.from({ length: MAXIMUM + 1 }, (_, code) => code).filter((code) => matches(String.fromCodePoint(code))),
    );
});
const single = (code) => refa.CharSet.fromCharacter(MAXIMUM, code);
const negated = (characters, positive) => (positive ? characters : characters.negate());
const property = (name) => {
    const { caret, body } = PROPERTY.exec(name.trim()).groups;
    const { key, inequality, value } = ASSIGNMENT.exec(body)?.groups ?? { key: body, inequality: '', value: undefined };
    const spelling = (value === undefined ? [`\\p{${key}}`, `\\p{Script=${key}}`] : [`\\p{${key.trim()}=${value.trim()}}`]).find(compiles);
    return negated(spelling === undefined ? fail(`Unknown Unicode property ${name}`) : scanned(spelling), (caret === '^') === (inequality === '!'));
};

// --- [REGEX]

const regex = (source, caseless = false) => {
    const text = [...source];
    const rest = (at) => text.slice(at).join('');
    const stop = (message, at) => fail(`${message} at ${at} in ${source}`);
    const close = (at, letter) => (text.indexOf(letter, at) < 0 ? stop(`Unclosed ${letter}`, at) : text.indexOf(letter, at));
    const between = (at) => [text.slice(at, close(at, '}')).join(''), close(at, '}') + 1];
    const folded = (characters, flags) => (flags.i ? refa.JS.createCharSet(characters.ranges, CASELESS) : characters);
    const node = (characters, start, end, perl = false) => ({ type: 'CharacterClass', characters, source: { start, end }, perl });
    const concatenation = (elements) => ({ type: 'Concatenation', elements });
    const union = (items) => ({ characters: refa.CharSet.empty(MAXIMUM).union(...items.map(({ characters }) => characters)), perl: items.some(({ perl }) => perl) });
    const literal = (code, onward) => [{ characters: single(code), code }, onward];
    const comment = (at) => (text.indexOf('\n', at) < 0 ? text.length : text.indexOf('\n', at) + 1);
    const trivia = (at, flags, bracketed = false) => {
        if (!flags.x) {
            return at;
        }
        if (text[at] !== undefined && WHITESPACE.test(text[at])) {
            return trivia(at + 1, flags, bracketed);
        }
        return !bracketed && text[at] === '#' ? trivia(comment(at), flags, bracketed) : at;
    };
    const hex = (at, letter) => {
        const [digits, onward] = text[at] === '{' ? between(at + 1) : [text.slice(at, at + WIDTHS[letter]).join(''), at + WIDTHS[letter]];
        return HEXADECIMAL.test(digits) ? literal(Number.parseInt(digits, 16), onward) : stop(`Invalid \\${letter} escape`, at);
    };
    const escapes = {
        perl: (at, letter, flags) => [{ characters: negated(scanned(PERL[letter.toLowerCase()][flags.u ? 0 : 1]), letter === letter.toLowerCase()), perl: true }, at + 1],
        property: (at, letter) => {
            const [name, onward] = text[at + 1] === '{' ? between(at + 2) : [text[at + 1], at + 2];
            return [{ characters: negated(property(name), letter === 'p') }, onward];
        },
        hex: (at, letter) => hex(at + 1, letter),
        control: (at, letter) => literal(CONTROLS[letter], at + 1),
        assertion: (at, letter) => [{ assertion: letter }, letter === 'b' && text[at + 1] === '{' ? close(at + 1, '}') + 1 : at + 1],
        punctuation: (at, letter) => literal(letter.codePointAt(0), at + 1),
    };
    const escape = (at, flags, bracketed) => {
        const letter = text[at] ?? stop('Trailing backslash', at);
        const [kind] = ESCAPES.find(([, matches]) => matches(letter, bracketed)) ?? stop(`Unknown escape \\${letter}`, at);
        return escapes[kind](at, letter, flags);
    };
    const character = (at, flags) => (text[at] === '\\' ? escape(at + 1, flags, true) : literal(text[at].codePointAt(0), at + 1));
    const span = (low, at, flags) => {
        const [high, end] = character(at, flags);
        return high.code === undefined || high.code < low.code ? stop('Invalid class range', at) : [{ characters: refa.CharSet.fromRange(MAXIMUM, { min: low.code, max: high.code }) }, end];
    };
    const member = (at, flags) => {
        const [low, onward] = character(at, flags);
        const ranged = low.code !== undefined && text[onward] === '-' && text[onward + 1] !== ']' && text[onward + 1] !== '-';
        return ranged ? span(low, onward + 1, flags) : [low, onward];
    };
    const item = (at, flags) => {
        const posix = POSIX_CLASS.exec(rest(at));
        if (posix) {
            return [{ characters: negated(scanned(POSIX[posix.groups.name] ?? stop(`Unknown POSIX class ${posix.groups.name}`, at)), posix.groups.inverted === '') }, at + posix[0].length];
        }
        return text[at] === '[' ? bracket(at + 1, flags, at) : member(at, flags);
    };
    const operator = (at) => Object.keys(OPERATORS).find((spelled) => rest(at).startsWith(spelled));
    const operand = (at, flags, items, leading) => {
        const start = trivia(at, flags, true);
        if (text[start] === undefined) {
            return stop('Unclosed class', start);
        }
        if ((text[start] === ']' && !(leading && items.length === 0)) || (items.length > 0 && operator(start))) {
            return [union(items), start];
        }
        const [next, onward] = item(start, flags);
        return operand(onward, flags, [...items, next], leading);
    };
    const combined = (left, at, flags) => {
        const spelled = operator(at);
        if (spelled === undefined) {
            return [left, at];
        }
        const [right, onward] = operand(at + spelled.length, flags, [], false);
        return combined({ characters: OPERATORS[spelled](left.characters, right.characters), perl: left.perl || right.perl }, onward, flags);
    };
    const bracket = (at, flags, start) => {
        const inverted = text[at] === '^';
        const [{ characters, perl }, end] = combined(...operand(inverted ? at + 1 : at, flags, [], true), flags);
        return text[end] === ']' ? [node(negated(folded(characters, flags), !inverted), start, end + 1, perl), end + 1] : stop('Unclosed class', end);
    };
    const counted = (at) => {
        const repetition = REPETITION.exec(rest(at)) ?? stop('Invalid repetition', at);
        const { low, comma, high } = repetition.groups;
        const maximum = comma && !high ? Number.POSITIVE_INFINITY : Number(high || low);
        return [[Number(low), maximum], at + repetition[0].length];
    };
    const quantified = (element, at, flags) => {
        const start = trivia(at, flags);
        if (!(text[start] in QUANTIFIERS || text[start] === '{')) {
            return [element, at];
        }
        const [[min, max], onward] = text[start] === '{' ? counted(start) : [QUANTIFIERS[text[start]], start + 1];
        const lazy = text[onward] === '?';
        return [{ type: 'Quantifier', min, max, lazy: lazy !== flags.U, alternatives: [concatenation([element])] }, lazy ? onward + 1 : onward];
    };
    const flag = (letter) => (FLAGS.has(letter) ? letter : stop(`Unknown flag ${letter}`, 0));
    const toggled = (flags, letters) => [...letters].reduce(({ on, scoped }, letter) => (letter === '-' ? { on: false, scoped } : { on, scoped: { ...scoped, [flag(letter)]: on } }), { on: true, scoped: flags }).scoped;
    const group = (at, flags) => {
        const named = NAMED.test(rest(at));
        const modifiers = named ? null : MODIFIERS.exec(rest(at));
        const scoped = modifiers ? toggled(flags, modifiers.groups.letters) : flags;
        if (modifiers?.groups.closer === ')') {
            return [undefined, at + modifiers[0].length, scoped];
        }
        const [alternatives, end] = sequence(named ? close(at, '>') + 1 : at + (modifiers?.[0].length ?? 0), scoped, [[]]);
        return text[end] === ')' ? [{ type: 'Alternation', alternatives }, end + 1, flags] : stop('Unclosed group', end);
    };
    const escaped = (at, flags) => {
        const [{ assertion, characters, perl }, onward] = escape(at + 1, flags, false);
        return [assertion ? { type: 'Unknown', id: assertion } : node(perl ? characters : folded(characters, flags), at, onward, perl), onward, flags];
    };
    const anchor = (at, flags) => [{ type: 'Unknown', id: text[at] }, at + 1, flags];
    const atoms = {
        '(': (at, flags) => group(at + 1, flags),
        '[': (at, flags) => [...bracket(at + 1, flags, at), flags],
        '.': (at, flags) => [node(flags.s ? refa.CharSet.all(MAXIMUM) : single(NEWLINE).negate(), at, at + 1), at + 1, flags],
        '^': anchor,
        $: anchor,
        '\\': escaped,
    };
    const plain = (at, flags) => [node(folded(single(text[at].codePointAt(0)), flags), at, at + 1), at + 1, flags];
    const step = (at, flags, alternatives) => {
        const [element, onward, scoped] = (atoms[text[at]] ?? plain)(at, flags);
        const [placed, end] = element === undefined ? [undefined, onward] : quantified(element, onward, scoped);
        return sequence(end, scoped, placed === undefined ? alternatives : [...alternatives.slice(0, -1), [...alternatives.at(-1), placed]]);
    };
    const sequence = (at, flags, alternatives) => {
        const start = trivia(at, flags);
        if (text[start] === undefined || text[start] === ')') {
            return [alternatives.map(concatenation), start];
        }
        return text[start] === '|' ? sequence(start + 1, flags, [...alternatives, []]) : step(start, flags, alternatives);
    };
    const [parsed, consumed] = sequence(0, { i: caseless, m: false, s: false, x: false, u: true, U: false, R: false }, [[]]);
    return consumed < text.length ? stop('Unbalanced group', consumed) : { type: 'Expression', alternatives: parsed };
};
const sets = (expression) => (expression.type === 'CharacterClass' ? [expression] : (expression.alternatives ?? expression.elements ?? []).flatMap(sets));

// --- [AUTOMATA]

const fragment = (rule) =>
    ({
        STRING: () => [{ type: 'Concatenation', elements: [...rule.value].map((letter) => ({ type: 'CharacterClass', characters: single(letter.codePointAt(0)) })) }],
        PATTERN: () => regex(rule.value, rule.flags?.includes('i')).alternatives,
        SEQ: () => [{ type: 'Concatenation', elements: rule.members.map((member) => ({ type: 'Alternation', alternatives: fragment(member) })) }],
        CHOICE: () => rule.members.flatMap(fragment),
        BLANK: () => [{ type: 'Concatenation', elements: [] }],
        REPEAT: () => [{ type: 'Concatenation', elements: [{ type: 'Quantifier', min: 0, max: Number.POSITIVE_INFINITY, lazy: false, alternatives: fragment(rule.content) }] }],
        REPEAT1: () => [{ type: 'Concatenation', elements: [{ type: 'Quantifier', min: 1, max: Number.POSITIVE_INFINITY, lazy: false, alternatives: fragment(rule.content) }] }],
    })[rule.type]?.() ?? (rule.content ? fragment(rule.content) : fail(`${rule.type} ${rule.name ?? ''} is not lexical`));
const automaton = (rule) => refa.NFA.fromRegex(fragment(rule), { maxCharacter: MAXIMUM });
const minimal = (fa) => {
    const dfa = refa.DFA.fromFA(fa);
    dfa.minimize();
    return dfa;
};
const pattern = (fa) => {
    const { source, flags } = refa.JS.toLiteral(fa.toRegex(), { flags: UNICODE, fastCharacters: true });
    return { type: 'PATTERN', value: source, flags };
};

// --- [CLASSES]

const fact = (characters) => ({ characters, unicode: !characters.isSubsetOf(ASCII) });
const reached = (dfa, nodes) => refa.CharSet.empty(MAXIMUM).union([...nodes].flatMap((node) => [...node.out].filter(([, target]) => dfa.finals.has(target)).map(([range]) => range)));
const classes = (grammar) => {
    const word = grammar.word ? minimal(automaton(grammar.rules[grammar.word])) : undefined;
    const extras = minimal(automaton({ type: 'CHOICE', members: grammar.extras.filter(({ type }) => type === 'PATTERN' || type === 'STRING') }));
    return { ...(word && { identifier: fact(reached(word, word.finals)) }), whitespace: fact(reached(extras, [extras.initial])) };
};

// --- [PREDICATES]

const token = (value) => (rule) => rule.type === 'STRING' && rule.value === value;
const symbol = (name) => (rule) => rule.type === 'SYMBOL' && rule.name === name;
const named = (field) => (rule) => rule.type === 'FIELD' && rule.name === field;
const choiceOf = (matches) => (rule) => rule.type === 'CHOICE' && rule.members.some(matches);
const optionalOf = (matches) => (rule) => rule.type === 'CHOICE' && rule.members.length === 2 && rule.members.some(({ type }) => type === 'BLANK') && rule.members.some(matches);
const any =
    (...predicates) =>
    (rule) =>
        predicates.some((matches) => matches(rule));

// --- [WALKERS]

const children = ({ members, content }) => members ?? (content === undefined ? [] : [content]);
const rebuild = (rule, next) => ({ ...rule, ...(rule.members && { members: next }), ...(rule.content && { content: next[0] }) });
const rewrite = (rule, apply) =>
    apply(
        rebuild(
            rule,
            children(rule).map((part) => rewrite(part, apply)),
        ),
    );
const reaches = (rule, matches) => matches(rule) || children(rule).some((part) => reaches(part, matches));
const hidden = (grammar, open) => (rule) => rule.type === 'SYMBOL' && rule.name.startsWith('_') && !open.has(rule.name) && !grammar.externals.some(symbol(rule.name));
const expand = (grammar, open) => (rule) => (hidden(grammar, open)(rule) ? expand(grammar, new Set([...open, rule.name]))(grammar.rules[rule.name]) : rebuild(rule, children(rule).map(expand(grammar, open))));
const resolve = (grammar, rule) => expand(grammar, new Set())(rule);

// --- [EDITS]

const edit = (matches, apply) => Object.assign(($, original) => apply($)(original), { matches });
const normalized = ($, extra) => seq(...extra.map((part) => (typeof part === 'function' ? part($) : part))).members;
const splice = (matches, place) => (rule) =>
    rule.type === 'SEQ'
        ? { ...rule, members: rule.members.flatMap((part) => (matches(part) ? place(part) : [part])) }
        : rebuild(
              rule,
              children(rule).map((part) => (matches(part) ? seq(...place(part)) : part)),
          );
const placed = (matches, place) => (rule) => (matches(rule) ? seq(...place(rule)) : rewrite(rule, splice(matches, place)));
const after = (matches, ...extra) => edit(matches, ($) => placed(matches, (part) => [part, ...normalized($, extra)]));
const before = (matches, ...extra) => edit(matches, ($) => placed(matches, (part) => [...normalized($, extra), part]));
const widened = (rule, extra) => (rule.type === 'FIELD' || rule.type.startsWith('PREC') ? { ...rule, content: widened(rule.content, extra) } : choice(rule, ...extra));
const widen = (matches, ...extra) => edit(matches, ($) => (rule) => rewrite(rule, (part) => (matches(part) ? widened(part, normalized($, extra)) : part)));
const pruned = (matches) => (rule) => (rule.members ? { ...rule, members: rule.members.filter((member) => !matches(member)) } : rule);
const pruning = (matches) => edit(matches, () => (rule) => rewrite(rule, pruned(matches)));
const drop = (...predicates) => pruning(any(...predicates));

// --- [GRAMMARS]

const applying = (transforms, rule) => (rule === undefined ? [] : transforms.filter(({ matches }) => reaches(rule, matches)));
const extend = (base, { transforms = [], rules = {}, ...options } = {}) =>
    grammar(base, {
        name: base.grammar.name,
        supertypes: (_, original) => original.filter(({ name }) => !base.grammar.inline.includes(name)),
        ...options,
        rules: Object.fromEntries(
            [...new Set([...Object.keys(rules), ...Object.keys(base.grammar.rules).filter((name) => applying(transforms, base.grammar.rules[name]).length > 0)])].map((name) => [
                name,
                ($, original) =>
                    (rules[name] ?? ((_, rule) => rule))(
                        $,
                        applying(transforms, original).reduce((rule, transform) => transform($, rule), original),
                    ),
            ]),
        ),
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { after, any, automaton, before, children, choiceOf, classes, drop, extend, named, optionalOf, pattern, reaches, rebuild, regex, resolve, rewrite, sets, symbol, token, widen };

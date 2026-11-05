import refa from 'refa';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MAXIMUM = 0x10_ff_ff;
const NON_ASCII = refa.CharSet.fromRange(MAXIMUM, { min: 0x80, max: MAXIMUM });
const LEXICAL = new Set(['STRING', 'PATTERN', 'TOKEN', 'IMMEDIATE_TOKEN']);
const HEXADECIMAL = 16;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [AUTOMATA]

const written = ({ value, flags }) => refa.JS.Parser.fromLiteral({ source: value, flags: `u${flags?.includes('i') ? 'i' : ''}` }).parse().expression.alternatives;
const fragment = (rule, parse) => {
    const within = (part) => fragment(part, parse);
    const repeated = (min) => [{ type: 'Concatenation', elements: [{ type: 'Quantifier', min, max: Number.POSITIVE_INFINITY, lazy: false, alternatives: within(rule.content) }] }];
    const forms = {
        STRING: () => [{ type: 'Concatenation', elements: [...rule.value].map((letter) => ({ type: 'CharacterClass', characters: refa.CharSet.fromCharacter(MAXIMUM, letter.codePointAt(0)) })) }],
        PATTERN: () => parse(rule),
        SEQ: () => [{ type: 'Concatenation', elements: rule.members.map((member) => ({ type: 'Alternation', alternatives: within(member) })) }],
        CHOICE: () => rule.members.flatMap(within),
        BLANK: () => [{ type: 'Concatenation', elements: [] }],
        REPEAT: () => repeated(0),
        REPEAT1: () => repeated(1),
    };
    if (forms[rule.type] === undefined && rule.content === undefined) {
        throw new SyntaxError(`${rule.type} ${rule.name ?? ''} is not lexical`);
    }
    return (forms[rule.type] ?? (() => within(rule.content)))();
};
const automaton = (rule, parse = written) => refa.NFA.fromRegex(fragment(rule, parse), { maxCharacter: MAXIMUM });
const minimal = (fa) => {
    const dfa = refa.DFA.fromFA(fa);
    dfa.minimize();
    return dfa;
};
const point = (code) => `\\u{${code.toString(HEXADECIMAL)}}`;
const span = ({ min, max }) => (min === max ? point(min) : `${point(min)}-${point(max)}`);
const element = ({ type, characters, alternatives, min, max }) => (type === 'CharacterClass' ? `[${characters.ranges.map(span).join('')}]` : `(?:${spelled(alternatives)})${type === 'Quantifier' ? `{${min},${max === Number.POSITIVE_INFINITY ? '' : max}}` : ''}`);
const spelled = (alternatives) => alternatives.map(({ elements }) => elements.map(element).join('')).join('|');
const pattern = (fa) => ({ type: 'PATTERN', value: spelled(fa.toRegex().alternatives) });
const without = (kept, removed) => {
    const excluded = refa.DFA.fromFA(removed);
    excluded.complement();
    return minimal(refa.DFA.fromIntersection(kept, excluded));
};
const unicode = (rule) =>
    minimal(
        refa.NFA.fromTransitionIterator(
            refa.FAIterators.mapOut(automaton(rule).transitionIterator(), (transitions) => new Map(Array.from(transitions, ([state, characters]) => [state, characters.union(NON_ASCII)]))),
            { maxCharacter: MAXIMUM },
        ),
    );

// --- [PREDICATES]

const string = (value) => (rule) => rule.type === 'STRING' && rule.value === value;
const symbol = (name) => (rule) => rule.type === 'SYMBOL' && rule.name === name;
const named = (field) => (rule) => rule.type === 'FIELD' && rule.name === field;
const choiceOf = (matches) => (rule) => rule.type === 'CHOICE' && rule.members.some(matches);
const optionalOf = (matches) => (rule) => rule.type === 'CHOICE' && rule.members.length === 2 && rule.members.some(({ type }) => type === 'BLANK') && rule.members.some(matches);
const any =
    (...predicates) =>
    (rule) =>
        predicates.some((matches) => matches(rule));
const lexical = ({ type }) => LEXICAL.has(type);

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
const located = (rule, matches) => (matches(rule) ? rule : children(rule).reduce((found, part) => found ?? located(part, matches), undefined));
const collected = (rule, matches) => [...(matches(rule) ? [rule] : []), ...children(rule).flatMap((part) => collected(part, matches))];
const closure = (grammar, name) => {
    const held = new Set(name in grammar.rules ? [name] : []);
    held.forEach((next) => collected(grammar.rules[next], ({ type }) => type === 'SYMBOL').forEach((part) => part.name in grammar.rules && held.add(part.name)));
    return held;
};
const expand = (grammar, open) => (rule) => (rule.type === 'SYMBOL' && rule.name.startsWith('_') && !open.has(rule.name) && !grammar.externals.some(symbol(rule.name)) ? expand(grammar, new Set([...open, rule.name]))(grammar.rules[rule.name]) : rebuild(rule, children(rule).map(expand(grammar, open))));
const mandatory = (rule) => {
    const optional = (part) => part.type === 'CHOICE' && part.members.some(({ type }) => type === 'BLANK');
    const found = (part) => optional(part) || children(part).some(found);
    if (optional(rule)) {
        const kept = rule.members.filter(({ type }) => type !== 'BLANK');
        return kept.length === 1 ? kept[0] : { ...rule, members: kept };
    }
    const index = children(rule).findIndex(found);
    return rebuild(
        rule,
        children(rule).map((part, at) => (at === index ? mandatory(part) : part)),
    );
};

// --- [STATES]

const expanded = (members) => {
    const [head, ...rest] = members;
    if (head === undefined) {
        return [];
    }
    if (head.type === 'SYMBOL' || lexical(head)) {
        return [members];
    }
    return (
        {
            BLANK: () => expanded(rest),
            SEQ: () => expanded([...head.members, ...rest]),
            CHOICE: () => head.members.flatMap((member) => expanded([member, ...rest])),
            REPEAT: () => [...expanded(rest), ...expanded([head.content, head, ...rest])],
            REPEAT1: () => expanded([head.content, { ...head, type: 'REPEAT' }, ...rest]),
        }[head.type] ?? (() => expanded([head.content, ...rest]))
    )();
};
const headOf = ([head]) => head.name ?? JSON.stringify(head);
const states = (grammar, start, closed) => {
    const productions = new Map();
    const closures = new Map();
    const successors = new Map();
    const partIds = new Map();
    const seen = new Set();
    const partId = (part) => partIds.getOrInsertComputed(part, () => partIds.size);
    const productionsOf = (name) => productions.getOrInsertComputed(name, () => Map.groupBy(closed(name) ? [] : expanded([grammar.rules[name]]), headOf));
    const closureOf = (name) =>
        closures.getOrInsertComputed(name, () => {
            const held = new Set();
            const visit = (next) => {
                if (!held.has(next)) {
                    held.add(next);
                    productionsOf(next).forEach((tails, head) => tails[0][0].type === 'SYMBOL' && visit(head));
                }
            };
            visit(name);
            return held;
        });
    const successorsOf = (tail) => successors.getOrInsertComputed(tail, () => expanded(tail.slice(1)));
    const fresh = (kernel) => {
        const key = [...new Set(kernel.map((tail) => tail.map(partId).join()))].toSorted((left, right) => (left < right ? -1 : 1)).join(';');
        return kernel.length > 0 && !seen.has(key) && seen.add(key);
    };
    const moved = (kernel) => {
        const open = new Set(kernel.filter(([head]) => head.type === 'SYMBOL').flatMap(([head]) => [...closureOf(head.name)]));
        const moves = Map.groupBy(kernel, headOf);
        open.forEach((name) => productionsOf(name).forEach((tails, head) => moves.set(head, [...(moves.get(head) ?? []), ...tails])));
        return { open, next: [...moves.values()].map((tails) => tails.flatMap(successorsOf)).filter(fresh) };
    };
    const explored = (frontier) => {
        const steps = frontier.map(moved);
        return frontier.length === 0 ? [] : [...steps.map(({ open }) => open), ...explored(steps.flatMap(({ next }) => next))];
    };
    return explored([expanded([grammar.rules[start]])]);
};

// --- [CLASSES]

const reached = (dfa, nodes) => refa.CharSet.empty(MAXIMUM).union([...nodes].flatMap((node) => [...node.out].filter(([, target]) => dfa.finals.has(target)).map(([range]) => range)));
const tokens = (rule) => {
    if (!reaches(rule, ({ type }) => type === 'SYMBOL')) {
        return [rule];
    }
    return rule.type === 'SEQ' ? [] : children(rule).flatMap(tokens);
};
const lexicalOf = (grammar, name) => tokens(expand(grammar, new Set())(grammar.rules[name]));
const symbols = (rule) => collected(rule, ({ type }) => type === 'SYMBOL');
const mostHeld = (grammar, held, admits) => [...Map.groupBy(held.filter(({ name }) => name in grammar.rules && admits(name)), ({ name }) => name)].reduce(([chosen, most], [name, rows]) => (rows.length > most ? [name, rows.length] : [chosen, most]), [undefined, 0])[0];
const spacing = (parse) => (rule) => {
    const dfa = minimal(automaton(rule, parse));
    const opening = reached(dfa, [dfa.initial]);
    return [...dfa.nodes()].every((node) => [...node.out].every(([range]) => opening.isSupersetOf(range))) ? opening : refa.CharSet.empty(MAXIMUM);
};
const lexemes = (grammar) => {
    const rules = Object.values(grammar.rules);
    const name = grammar.word ?? mostHeld(grammar, rules.flatMap((rule) => collected(rule, named('name'))).flatMap(({ content }) => symbols(content)), (held) => lexicalOf(grammar, held).length > 0);
    const stated = grammar.extras.map((extra) => (extra.type === 'SYMBOL' ? grammar.rules[extra.name] : extra)).filter((rule) => rule !== undefined && lexical(rule));
    const spaced = stated.length > 0 ? undefined : mostHeld(grammar, rules.flatMap(symbols), (held) => lexical(grammar.rules[held]));
    return {
        identifier: name === undefined ? [] : lexicalOf(grammar, name),
        whitespace: spaced === undefined ? stated : [grammar.rules[spaced]],
    };
};
const classes = ({ identifier, whitespace }, parse) => {
    const name = minimal(automaton({ type: 'CHOICE', members: identifier }, parse));
    return {
        ...(identifier.length > 0 && { identifier: reached(name, name.finals), 'identifier start': reached(name, [name.initial]) }),
        whitespace: refa.CharSet.empty(MAXIMUM).union(...whitespace.map(spacing(parse))),
    };
};
const outermost = (rule) => (lexical(rule) ? [rule] : children(rule).flatMap(outermost));
const folded = (characters) => [...new Set(characters.ranges.flatMap(({ min, max }) => Array.from({ length: max - min + 1 }, (_, at) => String.fromCodePoint(min + at).toLowerCase())))];
const spellings = (grammar) => {
    const name = minimal(automaton({ type: 'CHOICE', members: lexemes(grammar).identifier }));
    const leaves = [...new Map(Object.values(grammar.rules).flatMap(outermost).map((leaf) => [JSON.stringify(leaf), leaf])).values()];
    const finite = leaves.map((leaf) => minimal(automaton(leaf))).filter(({ isFinite }) => isFinite);
    const words = finite.flatMap((fa) => [...fa.wordSets()].flatMap((set) => set.reduce((held, characters) => held.flatMap((prefix) => folded(characters).map((letter) => prefix + letter)), [''])));
    return [...new Set(words)].filter((word) => word.length > 0 && name.test([...word].map((letter) => letter.codePointAt(0)))).toSorted();
};

// --- [EDITS]

const edit = (matches, apply) => Object.assign(($, original) => apply($)(original), { matches });
const normalized = ($, extra) => seq(...extra.map((part) => (typeof part === 'function' ? part($) : part))).members;
const placed = (matches, place) => {
    const held = (part) => (matches(part) ? seq(...place(part)) : part);
    const inserted = (part) => (matches(part) ? place(part) : [part]);
    return (rule) => (matches(rule) ? held(rule) : rewrite(rule, (part) => (part.type === 'SEQ' ? { ...part, members: part.members.flatMap(inserted) } : rebuild(part, children(part).map(held)))));
};
const after = (matches, ...extra) => edit(matches, ($) => placed(matches, (part) => [part, ...normalized($, extra)]));
const before = (matches, ...extra) => edit(matches, ($) => placed(matches, (part) => [...normalized($, extra), part]));
const widened = (rule, extra) => (rule.type === 'FIELD' || rule.type.startsWith('PREC') ? { ...rule, content: widened(rule.content, extra) } : choice(rule, ...extra));
const widen = (matches, ...extra) => edit(matches, ($) => (rule) => rewrite(rule, (part) => (matches(part) ? widened(part, normalized($, extra)) : part)));
const replace = (matches, replacement) => edit(matches, ($) => (original) => rewrite(original, (part) => (matches(part) ? replacement($, part) : part)));
const drop = (...predicates) => {
    const matches = any(...predicates);
    const pruned = (part) => (part.members ? { ...part, members: part.members.filter((member) => !matches(member)) } : part);
    return edit(matches, () => (rule) => rewrite(rule, pruned));
};
const chain =
    (...edits) =>
    ($, original) =>
        edits.reduce((rule, apply) => apply($, rule), original);

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

export { after, any, automaton, before, chain, children, choiceOf, classes, closure, collected, drop, extend, lexemes, lexical, located, MAXIMUM, mandatory, NON_ASCII, named, optionalOf, pattern, reaches, rebuild, replace, rewrite, spellings, states, string, symbol, unicode, widen, without };

import refa from 'refa';
import { fail, MAXIMUM, NON_ASCII, regex } from './regex.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const UNICODE = { unicode: true, unicodeSets: false, ignoreCase: false };
const LEXICAL = new Set(['STRING', 'PATTERN', 'TOKEN', 'IMMEDIATE_TOKEN']);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [AUTOMATA]

const fragment = (rule) =>
    ({
        STRING: () => [{ type: 'Concatenation', elements: [...rule.value].map((letter) => ({ type: 'CharacterClass', characters: refa.CharSet.fromCharacter(MAXIMUM, letter.codePointAt(0)) })) }],
        PATTERN: () => regex(rule.value, { caseless: rule.flags?.includes('i'), unicodePerl: false }).alternatives,
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
const external = (grammar, name) => grammar.externals.some(symbol(name));

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
const closure = (grammar, name, seen = new Set()) => (seen.has(name) || grammar.rules[name] === undefined ? seen : collected(grammar.rules[name], ({ type }) => type === 'SYMBOL').reduce((held, { name: next }) => closure(grammar, next, held), new Set([...seen, name])));
const expand = (grammar, open) => (rule) => (rule.type === 'SYMBOL' && rule.name.startsWith('_') && !open.has(rule.name) && !external(grammar, rule.name) ? expand(grammar, new Set([...open, rule.name]))(grammar.rules[rule.name]) : rebuild(rule, children(rule).map(expand(grammar, open))));
const resolve = (grammar, rule) => expand(grammar, new Set())(rule);
const extraTokens = (grammar) => grammar.extras.map((extra) => (extra.type === 'SYMBOL' ? grammar.rules[extra.name] : extra)).filter((rule) => rule !== undefined && lexical(rule));
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

// --- [CLASSES]

const reached = (dfa, nodes) => refa.CharSet.empty(MAXIMUM).union([...nodes].flatMap((node) => [...node.out].filter(([, target]) => dfa.finals.has(target)).map(([range]) => range)));
const tokens = (rule) => {
    if (!reaches(rule, ({ type }) => type === 'SYMBOL')) {
        return [rule];
    }
    return rule.type === 'SEQ' ? [] : children(rule).flatMap(tokens);
};
const lexicalOf = (grammar, name) => tokens(resolve(grammar, grammar.rules[name]));
const heldByName = (grammar) => {
    const held = Object.values(grammar.rules)
        .flatMap((rule) => collected(rule, named('name')))
        .flatMap(({ content }) => collected(content, ({ type }) => type === 'SYMBOL'))
        .filter(({ name }) => name in grammar.rules && lexicalOf(grammar, name).length > 0);
    return [...Map.groupBy(held, ({ name }) => name)].reduce(([chosen, most], [name, rows]) => (rows.length > most ? [name, rows.length] : [chosen, most]), [undefined, 0])[0];
};
const spacing = (rule) => {
    const dfa = minimal(automaton(rule));
    const opening = reached(dfa, [dfa.initial]);
    return [...dfa.nodes()].every((node) => [...node.out].every(([range]) => opening.isSupersetOf(range))) ? opening : refa.CharSet.empty(MAXIMUM);
};
const classes = (grammar) => {
    const name = grammar.word ?? heldByName(grammar);
    const identifier = name === undefined ? undefined : minimal(automaton({ type: 'CHOICE', members: lexicalOf(grammar, name) }));
    return { ...(identifier && { identifier: reached(identifier, identifier.finals) }), whitespace: refa.CharSet.empty(MAXIMUM).union(...extraTokens(grammar).map(spacing)) };
};

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

export { after, any, automaton, before, chain, children, choiceOf, classes, closure, collected, drop, extend, external, extraTokens, lexical, located, mandatory, minimal, named, optionalOf, pattern, reaches, rebuild, replace, resolve, rewrite, string, symbol, unicode, widen };

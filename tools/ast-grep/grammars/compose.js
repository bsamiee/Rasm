import refa from 'refa';
import { any, automaton, children, classes, named, pattern, reaches, rebuild, resolve, rewrite, symbol, widen } from './derive.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MAXIMUM = 0x10_ff_ff;
const HOST_COMMENT = 'host_comment';
const HOST_WHITESPACE = 'host_whitespace';
const LEXICAL = new Set(['STRING', 'PATTERN', 'TOKEN', 'IMMEDIATE_TOKEN']);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [NAMES]

const prefixer = (host) => (name) => (name.startsWith('_') ? `_${host}${name}` : `${host}_${name}`);
const external = (grammar, name) => grammar.externals.some(symbol(name));
const extraNames = (grammar) => new Set(grammar.extras.filter(({ type }) => type === 'SYMBOL').map(({ name }) => name));

// --- [AUTOMATA]

const minimal = (fa) => {
    const dfa = refa.DFA.fromFA(fa);
    dfa.minimize();
    return dfa;
};
const complement = (dfa) => {
    const negated = dfa.copy();
    negated.complement();
    return negated;
};
const meet = (left, right) => minimal(refa.DFA.fromIntersection(left, right));
const lexical = (rule) => LEXICAL.has(rule.type);
const leaves = (rule) => (lexical(rule) ? [rule] : children(rule).flatMap(leaves));
const union = (rules) => automaton({ type: 'CHOICE', members: rules.length > 0 ? rules : [{ type: 'BLANK' }] });
const codes = (text) => [...text].map((letter) => letter.codePointAt(0));

// --- [TRIVIA]

const leading = (grammar, rule) =>
    ({
        SEQ: () => leading(grammar, rule.members[0]),
        CHOICE: () => {
            const heads = rule.members.filter(({ type }) => type !== 'BLANK').map((member) => leading(grammar, member));
            return heads.includes(undefined) ? undefined : { type: 'CHOICE', members: heads };
        },
        SYMBOL: () => (external(grammar, rule.name) ? undefined : leading(grammar, grammar.rules[rule.name])),
    })[rule.type]?.() ?? (lexical(rule) ? rule : rule.content && leading(grammar, rule.content));
const vocabulary = (grammar) => {
    const trivia = extraNames(grammar);
    return Object.entries(grammar.rules)
        .filter(([name]) => !trivia.has(name))
        .flatMap(([, rule]) => leaves(rule));
};
const prefixes = (grammar) => {
    const nfa = union(vocabulary(grammar));
    nfa.prefixes();
    return nfa;
};
const collides = (grammar, rule, others) => {
    const head = leading(grammar, rule);
    return head !== undefined && others.some((other) => !refa.isDisjointWith(automaton(head), other));
};
const swallowing = (grammar) => {
    const trivia = union(grammar.extras.flatMap((extra) => (extra.type === 'SYMBOL' ? [grammar.rules[extra.name]].filter((rule) => rule !== undefined && lexical(rule)) : [extra])));
    return (value) => codes(value).some((_, index, all) => trivia.test(all.slice(0, index + 1)));
};

// --- [RENAMING]

const renaming = (rename, raised) => {
    const walk = (rule, inside) => {
        if (!inside && rule.type === 'STRING' && raised(rule.value)) {
            return token(prec(1, rule));
        }
        const nested = inside || rule.type === 'TOKEN' || rule.type === 'IMMEDIATE_TOKEN';
        const part = rebuild(
            rule,
            children(rule).map((child) => walk(child, nested)),
        );
        return {
            ...part,
            ...(part.type === 'SYMBOL' && { name: rename(part.name) }),
            ...(part.type === 'ALIAS' && part.named && { value: rename(part.value) }),
            ...(part.type.startsWith('PREC') && typeof part.value === 'string' && { value: rename(part.value) }),
            ...(part.type === 'RESERVED' && { context_name: rename(part.context_name) }),
        };
    };
    return (rule) => walk(rule, false);
};

// --- [IDENTIFIERS]

const powerset = (count) => (count === 0 ? [[]] : powerset(count - 1).flatMap((members) => [members, [...members, count - 1]]));
const subsets = (count) => powerset(count).filter((members) => members.length > 0);
const regions = (languages) => {
    const words = languages.map(({ word }) => minimal(word));
    const outside = words.map(complement);
    return subsets(languages.length)
        .map((members) => ({
            members,
            name: `_word_${members.map((index) => languages[index].name).join('_')}`,
            dfa: words.reduce((region, word, index) => meet(region, members.includes(index) ? word : outside[index]), refa.DFA.all({ maxCharacter: MAXIMUM })),
        }))
        .filter(({ dfa }) => !dfa.isEmpty);
};
const identifier = ($, name, context, owned, common) => {
    const visible = !name.startsWith('_');
    const wrapped = (rule) => (visible ? alias(rule, $[name]) : rule);
    return choice(wrapped(context === undefined ? $[common] : reserved(context, $[common])), ...owned.map((region) => wrapped($[region])));
};

// --- [FRAGMENTS]

const head = (rule) => {
    const [first] = children(rule);
    return rule.type === 'SYMBOL' || first === undefined ? rule : head(first);
};
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
const form = (dialect, fragments) => ($, fragment) => {
    const rule = dialect.rules[fragment];
    const led = fragments.some((other) => other !== fragment && symbol(other)(head(rule)));
    return led ? alias(mandatory(rule), $[fragment]) : $[fragment];
};
const closure = (grammar, name, seen = new Set()) => (seen.has(name) || grammar.rules[name] === undefined ? seen : [...collect(grammar.rules[name])].reduce((held, next) => closure(grammar, next, held), new Set([...seen, name])));
const collect = (rule) => new Set([...(rule.type === 'SYMBOL' ? [rule.name] : []), ...children(rule).flatMap((part) => [...collect(part)])]);
const labelled = (rule, targets, hole) => {
    const label = targets.find((target) => reaches(rule, named(target)));
    return label === undefined ? hole : field(label, hole);
};
const slotted = (grammar, slots, name, targets) => {
    const whole = Object.keys(slots).filter((other) => slots[other].length === 0);
    if (targets.length > 0) {
        const matches = any(...targets.flatMap((target) => [named(target), symbol(target)]));
        return ($, original) => rewrite(widen(matches, ($$) => prec(whole.length, $$.substitution))($, original), (part) => ((part.type === 'REPEAT' || part.type === 'REPEAT1') && reaches(part.content, matches) ? choice(part, labelled(part.content, targets, prec(whole.length, $.substitution))) : part));
    }
    const reached = closure(grammar, name);
    const level = whole.filter((other) => other !== name && reached.has(other)).length;
    return ($, original) => choice(original, prec(level, $.substitution));
};

// --- [HOSTS]

const renamedConflicts = ($, { host, rename }) => host.conflicts.map((names) => names.map((held) => $[rename(held)]));
const renamedPrecedences = ({ host, rename }) => host.precedences.map((level) => level.map((entry) => (entry.type === 'STRING' ? { ...entry, value: rename(entry.value) } : sym(rename(entry.name)))));
const entered = ($, dialect, fragments, { host, entry, hole, rename, walk }) => {
    const filled = choice(dialect.rules.source_file, ...fragments.map((fragment) => form(dialect, fragments)($, fragment)));
    return rewrite(walk(resolve(host, host.rules[entry])), (rule) => (rule.type === 'REPEAT' && reaches(rule.content, symbol(rename(hole))) ? filled : rule));
};

// --- [KEYWORDS]

const keywords = (base) => {
    const table = new Map(
        Object.values(base.grammar.rules)
            .flatMap(function found(rule) {
                return rule.type === 'ALIAS' && !rule.named && rule.content.type === 'PATTERN' ? [rule] : children(rule).flatMap(found);
            })
            .map((rule) => [rule.value, rule]),
    );
    return (word) => table.get(word.toUpperCase()) ?? alias(new RegExp([...word].map((letter) => `[${letter.toUpperCase()}${letter.toLowerCase()}]`).join(''), 'u'), word.toUpperCase());
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const compose = (base, { name, hosts, fragments, slots = {}, rules = {}, conflicts = () => [] }) => {
    const dialect = base.grammar;
    const raised = swallowing(dialect);
    const components = [dialect, ...hosts.map(({ grammar: host }) => host.grammar)];
    const vocabularies = components.map(prefixes);
    const parts = hosts.map(({ grammar: { grammar: host }, entry, hole }, index) => {
        const others = vocabularies.filter((_, at) => at !== index + 1);
        const survivors = [...extraNames(host)].filter((extra) => external(host, extra) || !collides(host, host.rules[extra], others));
        const trivia = survivors.filter((extra) => external(host, extra));
        const prefix = prefixer(host.name);
        const rename = (rule) => (trivia.includes(rule) ? HOST_COMMENT : prefix(rule));
        const context = Object.keys(host.reserved ?? {}).length > 0 ? rename(Object.keys(host.reserved)[0]) : host.name;
        return { host, entry, hole, rename, walk: renaming(rename, raised), context, comments: survivors.filter((extra) => !external(host, extra)) };
    });
    const languages = [dialect, ...parts.map(({ host }) => host)].map((grammar) => ({ name: grammar.name, word: automaton(grammar.rules[grammar.word]) }));
    const zones = regions(languages);
    const common = zones.find(({ members }) => members.length === languages.length).name;
    const owned = (index) => zones.filter(({ members }) => members.includes(index) && members.length < languages.length).map(({ name: zone }) => zone);
    const whitespace = parts.reduce((held, { host }) => held.union(classes(host).whitespace.characters), refa.CharSet.empty(MAXIMUM)).without(classes(dialect).whitespace.characters);
    const [dialectContext] = Object.keys(dialect.reserved ?? {});
    const comments = parts.flatMap(({ rename, comments: held }) => held.map((comment) => [rename, comment]));
    const overrides = Object.fromEntries(
        [...new Set([...Object.keys(rules), ...Object.keys(slots)])].map((rule) => [
            rule,
            ($, original) => {
                const defined = (rules[rule] ?? ((_, held) => held))($, original);
                return slots[rule] === undefined ? defined : slotted(dialect, slots, rule, slots[rule])($, defined);
            },
        ]),
    );
    return grammar(base, {
        name,
        externals: (_, original) => [...original, ...parts.flatMap(({ host, rename }) => host.externals.map((rule) => (rule.type === 'SYMBOL' ? sym(rename(rule.name)) : rule)))],
        extras: ($, original) => [...original, ...parts.flatMap(({ host, rename }) => [...extraNames(host)].filter((extra) => rename(extra) === HOST_COMMENT).map(() => $[HOST_COMMENT])), ...(comments.length > 0 ? [$._host_trivia] : []), ...(whitespace.isEmpty ? [] : [$[HOST_WHITESPACE]])],
        inline: ($, original) => [...original, $[dialect.word], ...parts.flatMap(({ host, rename }) => [...host.inline.map((rule) => $[rename(rule)]), $[rename(host.word)]])],
        supertypes: ($, original) => [...original, ...parts.flatMap(({ host, rename }) => host.supertypes.map((rule) => $[rename(rule)]))],
        conflicts: ($, original) => [...original, ...conflicts($), ...parts.flatMap((part) => renamedConflicts($, part))],
        precedences: (_, original) => [...original, ...parts.flatMap(renamedPrecedences)],
        reserved: Object.fromEntries(parts.flatMap(({ host, rename, walk, context }) => (Object.keys(host.reserved ?? {}).length > 0 ? Object.entries(host.reserved).map(([set, words]) => [rename(set), () => words.map(walk)]) : [[context, () => []]]))),
        word: ($) => $[common],
        rules: {
            ...Object.fromEntries(parts.flatMap(({ host, rename, walk }) => Object.entries(host.rules).map(([rule, body]) => [rename(rule), () => walk(body)]))),
            ...Object.fromEntries(zones.map(({ name: zone, dfa }) => [zone, () => pattern(dfa)])),
            [dialect.word]: ($) => identifier($, dialect.word, dialectContext, owned(0), common),
            ...Object.fromEntries(parts.map(({ host, rename, context }, index) => [rename(host.word), ($) => identifier($, rename(host.word), context, owned(index + 1), common)])),
            ...(whitespace.isEmpty ? {} : { [HOST_WHITESPACE]: () => pattern(refa.DFA.fromCharSet(whitespace)) }),
            ...(comments.length > 0 && { _host_trivia: ($) => choice(...comments.map(([rename, comment]) => alias($[rename(comment)], $[HOST_COMMENT]))) }),
            source_file: ($, original) => choice(original, ...parts.map((part) => entered($, dialect, fragments, part))),
            substitution: ($) => prec.right(repeat1(choice(...parts.map(({ hole, rename }) => $[rename(hole)])))),
            ...overrides,
        },
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { compose, keywords, mandatory };

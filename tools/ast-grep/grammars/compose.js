import { Array, Function, Record } from 'effect';
import refa from 'refa';
import { any, automaton, children, classes, closure, external, extraTokens, lexical, mandatory, minimal, named, pattern, reaches, rebuild, resolve, rewrite, symbol, widen } from './derive.js';
import { MAXIMUM } from './regex.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const HOST_COMMENT = 'host_comment';
const HOST_WHITESPACE = 'host_whitespace';
const DIALECT_WHITESPACE = '_dialect_whitespace';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [NAMES]

const extraNames = (grammar) => new Set(grammar.extras.filter(({ type }) => type === 'SYMBOL').map(({ name }) => name));
const wordSets = ({ name, reserved: sets = {} }, rename, walk) => (Object.keys(sets).length > 0 ? Record.collect(sets, (set, words) => [rename(set), () => words.map(walk)]) : [[name, () => []]]);

// --- [AUTOMATA]

const complement = (dfa) => {
    const negated = dfa.copy();
    negated.complement();
    return negated;
};
const leaves = (rule) => (lexical(rule) ? [rule] : children(rule).flatMap(leaves));
const union = (rules) => automaton({ type: 'CHOICE', members: rules.length > 0 ? rules : [{ type: 'BLANK' }] });

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
const keptComments = (host, others) =>
    [...extraNames(host)]
        .filter((extra) => !external(host, extra))
        .filter((extra) => {
            const start = leading(host, host.rules[extra]);
            return start === undefined || others.every((other) => refa.isDisjointWith(automaton(start), other));
        });
const prefixes = (grammar) => {
    const trivia = extraNames(grammar);
    const nfa = union(
        Object.entries(grammar.rules)
            .filter(([name]) => !trivia.has(name))
            .flatMap(([, rule]) => leaves(rule)),
    );
    nfa.prefixes();
    return nfa;
};
const swallowing = (grammar) => {
    const trivia = union(extraTokens(grammar));
    return (value) => [...value].map((letter) => letter.codePointAt(0)).some((_, index, all) => trivia.test(all.slice(0, index + 1)));
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
const regions = (languages) => {
    const words = languages.map(({ word }) => minimal(word));
    const outside = words.map(complement);
    return powerset(languages.length)
        .filter(Array.isReadonlyArrayNonEmpty)
        .map((members) => ({
            members,
            name: `_word_${members.map((index) => languages[index].name).join('_')}`,
            dfa: words.reduce((region, word, index) => minimal(refa.DFA.fromIntersection(region, members.includes(index) ? word : outside[index])), refa.DFA.all({ maxCharacter: MAXIMUM })),
        }))
        .filter(({ dfa }) => !dfa.isEmpty);
};
const identifier = ($, name, context, owned, common) => {
    const visible = !name.startsWith('_');
    const wrapped = (rule) => (visible ? alias(rule, $[name]) : rule);
    return choice(wrapped(reserved(context, $[common])), ...owned.map((region) => wrapped($[region])));
};

// --- [FRAGMENTS]

const head = (rule) => {
    const [first] = children(rule);
    return rule.type === 'SYMBOL' || first === undefined ? rule : head(first);
};
const form = (dialect, fragments) => ($, fragment) => {
    const rule = dialect.rules[fragment];
    const led = fragments.some((other) => other !== fragment && symbol(other)(head(rule)));
    return led ? alias(mandatory(rule), $[fragment]) : $[fragment];
};
const slotted = (grammar, slots, name, targets) => {
    const whole = Object.keys(slots).filter((other) => slots[other].length === 0);
    if (targets.length > 0) {
        const matches = any(...targets.flatMap((target) => [named(target), symbol(target)]));
        const list = (part) => (part.type === 'REPEAT' || part.type === 'REPEAT1') && !lexical(head(part.content)) && reaches(part.content, matches);
        const hole = ($, part) => {
            const label = targets.find((target) => reaches(part.content, named(target)));
            const filled = prec(whole.length, $.substitution);
            return label === undefined ? filled : field(label, filled);
        };
        return ($, original) => rewrite(widen(matches, ($$) => prec(whole.length, $$.substitution))($, original), (part) => (list(part) ? choice(part, hole($, part)) : part));
    }
    const reached = closure(grammar, name);
    const level = whole.filter((other) => other !== name && reached.has(other)).length;
    return ($, original) => choice(original, prec(level, $.substitution));
};

// --- [HOSTS]

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
        const rename = (rule) => (rule.startsWith('_') ? `_${host.name}${rule}` : `${host.name}_${rule}`);
        const walk = renaming(rename, raised);
        const sets = wordSets(host, rename, walk);
        return {
            host,
            entry,
            hole,
            rename,
            walk,
            sets,
            context: sets[0][0],
            comments: keptComments(
                host,
                vocabularies.filter((_, at) => at !== index + 1),
            ),
        };
    });
    const conflicted = parts.flatMap(({ host, rename }) => host.conflicts.map((names) => names.map(rename)));
    const ranked = parts.flatMap(({ host, rename }) => host.precedences.map((level) => [rename, level]));
    const dialectSets = wordSets(dialect, Function.identity, Function.identity);
    const languages = [dialect, ...parts.map(({ host }) => host)].map((grammar) => ({ name: grammar.name, word: automaton(grammar.rules[grammar.word]) }));
    const zones = regions(languages);
    const common = zones.find(({ members }) => members.length === languages.length).name;
    const owned = (index) => zones.filter(({ members }) => members.includes(index) && members.length < languages.length).map(({ name: zone }) => zone);
    const spaces = dialect.extras.filter(lexical);
    const whitespace = parts.reduce((held, { host }) => held.union(classes(host).whitespace), refa.CharSet.empty(MAXIMUM)).without(classes(dialect).whitespace);
    const comments = parts.flatMap(({ rename, comments: held }) => held.map((comment) => [rename, comment]));
    const overrides = Record.fromIterableWith(new Set([...Object.keys(rules), ...Object.keys(slots)]), (rule) => [
        rule,
        ($, original) => {
            const defined = (rules[rule] ?? ((_, held) => held))($, original);
            return slots[rule] === undefined ? defined : slotted(dialect, slots, rule, slots[rule])($, defined);
        },
    ]);
    return grammar(base, {
        name,
        externals: (_, original) => [...original, ...parts.flatMap(({ host, rename }) => host.externals.map((rule) => (rule.type === 'SYMBOL' ? sym(rename(rule.name)) : rule)))],
        extras: ($, original) => [...(spaces.length > 0 ? [$[DIALECT_WHITESPACE]] : []), ...original.filter((extra) => !lexical(extra)), ...(comments.length > 0 ? [$._host_trivia] : []), ...(whitespace.isEmpty ? [] : [$[HOST_WHITESPACE]])],
        inline: ($, original) => [...original, $[dialect.word], ...parts.flatMap(({ host, rename }) => [...host.inline.map((rule) => $[rename(rule)]), $[rename(host.word)]])],
        supertypes: ($, original) => [...original, ...parts.flatMap(({ host, rename }) => host.supertypes.map((rule) => $[rename(rule)]))],
        conflicts: ($, original) => [...original, [$.substitution], ...conflicts($), ...conflicted.map((names) => names.map((held) => $[held]))],
        precedences: (_, original) => [...original, ...ranked.map(([rename, level]) => level.map((entry) => (entry.type === 'STRING' ? { ...entry, value: rename(entry.value) } : sym(rename(entry.name)))))],
        reserved: Object.fromEntries([...dialectSets, ...parts.flatMap(({ sets }) => sets)]),
        word: ($) => $[common],
        rules: {
            ...Object.fromEntries(parts.flatMap(({ host, rename, walk }) => Record.collect(host.rules, (rule, body) => [rename(rule), () => walk(body)]))),
            ...Record.fromIterableWith(zones, ({ name: zone, dfa }) => [zone, () => pattern(dfa)]),
            [dialect.word]: ($) => identifier($, dialect.word, dialectSets[0][0], owned(0), common),
            ...Object.fromEntries(parts.map(({ host, rename, context }, index) => [rename(host.word), ($) => identifier($, rename(host.word), context, owned(index + 1), common)])),
            ...(spaces.length > 0 && { [DIALECT_WHITESPACE]: () => pattern(minimal(automaton({ type: 'REPEAT1', content: { type: 'CHOICE', members: spaces } }))) }),
            ...(whitespace.isEmpty ? {} : { [HOST_WHITESPACE]: () => pattern(refa.DFA.fromCharSet(whitespace)) }),
            ...(comments.length > 0 && { _host_trivia: ($) => choice(...comments.map(([rename, comment]) => alias($[rename(comment)], $[HOST_COMMENT]))) }),
            source_file: ($, original) => choice(original, ...parts.map((part) => entered($, dialect, fragments, part))),
            substitution: ($) => {
                const hole = choice(...parts.map(({ hole: held, rename }) => $[rename(held)]));
                return choice(hole, prec.dynamic(-1, seq(hole, repeat1(hole))));
            },
            ...overrides,
        },
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { compose, keywords };

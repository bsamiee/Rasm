import { Record } from 'effect';
import { any, children, classes, closure, lexemes, lexical, mandatory, named, reaches, rewrite, spans, spellings, symbol, widen } from './derive.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const HOSTS = {
    tsx: { documents: [{ opening: '`', closing: '`' }], hole: { opening: '${', closing: '}' } },
    csharp: {
        documents: [
            { opening: /\$@?"|@\$"/u, closing: '"' },
            { opening: /\$+"""+/u, closing: /"""+/u },
        ],
        hole: { opening: '{', closing: '}' },
    },
};

const LINE_COMMENT = /--(?:[^\r\n`]|`[^\r\n`]*`)*/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const head = (rule) => {
    const [first] = children(rule);
    return rule.type === 'SYMBOL' || first === undefined ? rule : head(first);
};
const level = (grammar, slots, name) => {
    const reached = closure(grammar, name);
    return Object.keys(slots).filter((other) => slots[other].length === 0 && other !== name && reached.has(other)).length;
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
    return ($, original) => choice(original, prec(level(grammar, slots, name), $.substitution));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const compose = (base, { name, fragments, slots, rules, conflicts }) => {
    const dialect = base.grammar;
    const hosts = Object.values(HOSTS);
    const documents = hosts.flatMap((host) => host.documents);
    const overrides = Record.fromIterableWith(new Set([...Object.keys(rules), ...Object.keys(slots)]), (rule) => [
        rule,
        ($, original) => {
            const defined = (rules[rule] ?? ((_, held) => held))($, original);
            return slots[rule] === undefined ? defined : slotted(dialect, slots, rule, slots[rule])($, defined);
        },
    ]);
    return grammar(base, {
        name,
        externals: ($, original) => [...original, $._hole, $._hole_end],
        conflicts: ($, original) => [...original, [$.substitution], ...conflicts($)],
        rules: {
            substitution: ($) => prec.dynamic(1, repeat1(seq($._hole, $._hole_end))),
            _hole_forms: () => seq(...hosts.flatMap(({ hole: { opening, closing } }) => [opening, closing])),
            _hole_tail: () => spans(classes(lexemes(dialect)).identifier),
            _hole_words: () => seq(...spellings(dialect)),
            source_file: ($, original) => {
                const led = (fragment) => fragments.some((other) => other !== fragment && symbol(other)(head(dialect.rules[fragment])));
                const filled = choice(dialect.rules.source_file, ...fragments.map((fragment) => (led(fragment) ? alias(mandatory(dialect.rules[fragment]), $[fragment]) : $[fragment])));
                return choice(original, seq(choice(...documents.map(({ opening }) => opening)), filled, choice(...documents.map(({ closing }) => closing))));
            },
            ...overrides,
        },
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { compose, LINE_COMMENT, level };

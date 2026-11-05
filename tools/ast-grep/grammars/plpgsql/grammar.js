import plpgsql from 'tree-sitter-postgres/plpgsql/grammar.js';
import { extend, pattern, rebuild, rewrite, unicode } from '../derive.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const RULES = plpgsql.grammar.rules;
const STATEMENT = 'proc_stmt';
const TERMINATOR = ';';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [TERMINATORS]

const terminated = (rule) =>
    ({
        SEQ: () => terminated(rule.members.at(-1)),
        CHOICE: () => rule.members.some(terminated),
        STRING: () => rule.value === TERMINATOR,
        SYMBOL: () => rule.name in RULES && terminated(RULES[rule.name]),
    })[rule.type]?.() ??
    (rule.content !== undefined && terminated(rule.content));
const closers = (rule) =>
    ({
        SEQ: () => closers(rule.members.at(-1)),
        CHOICE: () => rule.members.flatMap(closers),
        SYMBOL: () => (terminated(rule) ? [rule.name, ...closers(RULES[rule.name])] : []),
    })[rule.type]?.() ?? (rule.content === undefined ? [] : closers(rule.content));
const unterminated = (name) => `_${name}_unterminated`;
const opened = new Set(closers(RULES[STATEMENT]));
const open = ($, rule) =>
    ({
        SEQ: () => rebuild(rule, [...rule.members.slice(0, -1), open($, rule.members.at(-1))]),
        CHOICE: () =>
            rebuild(
                rule,
                rule.members.map((member) => open($, member)),
            ),
        STRING: () => (rule.value === TERMINATOR ? optional(rule) : rule),
        SYMBOL: () => (opened.has(rule.name) ? alias($[unterminated(rule.name)], $[rule.name]) : rule),
    })[rule.type]?.() ?? (rule.content === undefined ? rule : rebuild(rule, [open($, rule.content)]));

// --- [EXPORTS] -------------------------------------------------------------------------

export default extend(plpgsql, {
    externals: ($, original) => [...original, $._template_open],
    rules: {
        source_file: ($, original) => {
            const delimiter = '`';
            return choice(original, seq(alias($._template_open, delimiter), repeat($[STATEMENT]), alias($._block_statement, $[STATEMENT]), delimiter));
        },
        _block_statement: ($) => open($, RULES[STATEMENT]),
        ...Object.fromEntries([...opened].map((name) => [unterminated(name), ($) => open($, RULES[name])])),
        [plpgsql.grammar.word]: (_, original) => rewrite(original, (part) => (part.type === 'PATTERN' ? pattern(unicode(part)) : part)),
    },
});

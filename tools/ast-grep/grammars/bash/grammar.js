import bash from 'tree-sitter-bash/grammar.js';
import { children, drop, extend, reaches, rewrite, symbol, widen } from '../derive.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DESCRIPTOR = /\{[A-Za-z_][A-Za-z0-9_]*\}/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [QUOTED]

const hidden = (name) => name.startsWith('_') && name in bash.grammar.rules;
const references = (rule) => (rule.type === 'SYMBOL' ? [rule.name] : children(rule).flatMap(references));
const closure = (names) => {
    const next = [...new Set([...names, ...names.flatMap((name) => references(bash.grammar.rules[name]).filter(hidden))])];
    return next.length === names.length ? names : closure(next);
};
const quotes = (name) => closure([name]).some((reached) => reaches(bash.grammar.rules[reached], symbol('raw_string')));
const QUOTED = closure(['expansion']).filter(quotes);
const quoted = (name) => `_quoted${hidden(name) ? '' : '_'}${name}`;
const unquote = ($) => (rule) => {
    if (rule.type !== 'SYMBOL') {
        return rule;
    }
    if (rule.name === 'raw_string') {
        return alias("'", $.word);
    }
    if (!QUOTED.includes(rule.name)) {
        return rule;
    }
    return hidden(rule.name) ? $[quoted(rule.name)] : alias($[quoted(rule.name)], $[rule.name]);
};

// --- [STATEMENTS]

const statement = (...extra) => widen(symbol('command'), ...extra);
const commandStatement = statement(($) => $.coproc_command);
const RULES = {
    _statement_not_subshell: ($, original) => widen(symbol('pipeline'), $.time_command)($, commandStatement($, original)),
    _statement_not_pipeline: commandStatement,
    time_command: ($) => prec.left(-1, seq('time', optional('-p'), optional(field('body', choice($.pipeline, $._statement_not_pipeline))))),
    coproc_command: ($) => seq('coproc', choice(prec.dynamic(1, seq(optional(field('name', $.word)), field('body', choice($.compound_statement, $.subshell, $.if_statement, $.while_statement, $.for_statement, $.c_style_for_statement, $.case_statement, $.test_command)))), field('body', $.command))),
    command_substitution: ($, original) => choice(original, seq(field('operator', choice(...[' ', '\t', '\n', '|'].map((separator) => alias(token(seq('${', repeat('\\\n'), separator)), separator === '|' ? '${|' : '${')))), optional($._terminated_statement), '}')),
    _concatenation_in_expansion: drop(symbol('variable_name'), symbol('array')),
    string: ($, original) => rewrite(original, unquote($)),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export default extend(bash, {
    conflicts: ($, original) => [...original, [$.command_name, $.coproc_command]],
    inline: ($, original) => [...original, ...QUOTED.filter((name) => original.some(symbol(name))).map((name) => $[quoted(name)])],
    transforms: [widen(symbol('file_descriptor'), ($) => alias(token(DESCRIPTOR), $.file_descriptor))],
    rules: {
        ...RULES,
        ...Object.fromEntries(QUOTED.map((name) => [quoted(name), ($) => rewrite((RULES[name] ?? ((_, rule) => rule))($, bash.grammar.rules[name]), unquote($))])),
    },
});

import bash from 'tree-sitter-bash/grammar.js';
import { drop, extend, symbol, widen } from '../derive.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DESCRIPTOR = /\{[A-Za-z_][A-Za-z0-9_]*\}/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const coprocStatement = widen(symbol('command'), ($) => $.coproc_command);

// --- [EXPORTS] -------------------------------------------------------------------------

export default extend(bash, {
    externals: ($, original) => [...original, $._time_posix],
    conflicts: ($, original) => [...original, [$.command_name, $.coproc_command]],
    transforms: [widen(symbol('file_descriptor'), ($) => alias(token(DESCRIPTOR), $.file_descriptor))],
    rules: {
        _statement_not_subshell: ($, original) => widen(symbol('pipeline'), $.time_command)($, coprocStatement($, original)),
        _statement_not_pipeline: coprocStatement,
        time_command: ($) => prec.left(-1, seq('time', optional(alias($._time_posix, '-p')), optional(field('body', choice($.pipeline, $._statement_not_pipeline))))),
        coproc_command: ($) => seq('coproc', choice(prec.dynamic(1, seq(optional(field('name', $.word)), field('body', choice($.compound_statement, $.subshell, $.if_statement, $.while_statement, $.for_statement, $.c_style_for_statement, $.case_statement, $.test_command)))), field('body', $.command))),
        command_substitution: ($, original) => choice(original, seq(field('operator', choice(...[' ', '\t', '\n', '|'].map((separator) => alias(token(seq('${', repeat('\\\n'), separator)), separator === '|' ? '${|' : '${')))), optional($._terminated_statement), '}')),
        _concatenation_in_expansion: drop(symbol('variable_name'), symbol('array')),
    },
});

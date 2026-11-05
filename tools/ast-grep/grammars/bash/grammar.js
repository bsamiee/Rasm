import bash from 'tree-sitter-bash/grammar.js';
import { choiceOf, drop, extend, located, named, optionalOf, reaches, rewrite, string, symbol, widen } from '../derive.js';

// --- [OPERATIONS] ----------------------------------------------------------------------

const compound = ($) => [$.compound_statement, $.subshell, $.if_statement, $.while_statement, $.for_statement, $.c_style_for_statement, $.case_statement, $.test_command];
const redirected = ($, body) => seq(field('body', body), field('redirect', repeat1($._redirect)));
const coprocStatement = widen(symbol('command'), ($) => $.coproc_command);

// --- [EXPORTS] -------------------------------------------------------------------------

export default extend(bash, {
    externals: ($, original) => [...original, $._time_posix, $._time_end, $._closed, $._extglob_word, $._brace_open, $._backtick_end, $._heredoc_due],
    extras: ($, original) => [...original, $._heredoc_body],
    conflicts: ($, original) => [...original.filter((set) => !set.some(symbol('pipeline'))), [$.command_name, $.coproc_command], [$.herestring_redirect]],
    rules: {
        _statement_not_subshell: ($, original) => widen(symbol('pipeline'), $.time_command)($, coprocStatement($, original)),
        _statement_not_pipeline: ($, original) => coprocStatement($, drop(symbol('list'), symbol('negated_command'))($, original)),
        _pipeline_statement: ($) => choice($.pipeline, $._statement_not_pipeline, $.negated_command, $.time_command),
        _terminated_statement: widen(symbol('_terminator'), ($) => $._closed),
        redirected_statement: ($) =>
            prec.dynamic(
                -1,
                prec.right(
                    -1,
                    choice(
                        redirected($, choice(drop(symbol('pipeline'), symbol('list'), symbol('negated_command'), symbol('variable_assignment'))($, bash.grammar.rules._statement_not_subshell), $.subshell)),
                        prec.right(0, redirected($, $.variable_assignment)),
                        prec.right(0, seq(field('redirect', repeat1($._redirect)), field('body', $.variable_assignment))),
                        field('redirect', repeat1($._redirect)),
                    ),
                ),
            ),
        _redirect: widen(symbol('herestring_redirect'), ($) => $.heredoc_redirect),
        for_statement: ($) => seq(choice('for', 'select'), field('variable', $._simple_variable_name), choice(seq('in', optional(field('value', repeat1($._literal))), $._terminator), optional($._terminator)), field('body', choice($.do_group, $.compound_statement))),
        last_case_item: (_, original) => rewrite(original, (part) => (optionalOf((member) => reaches(member, string(';;')))(part) ? optional(located(bash.grammar.rules.case_item, choiceOf(named('termination')))) : part)),
        negated_command: ($) => prec.right(seq('!', optional($._pipeline_statement))),
        test_command: ($, original) => rewrite(original, (part) => (symbol('redirected_statement')(part) ? choice(alias($._test_redirected, $.redirected_statement), prec.dynamic(-1, alias($._test_operand, $.command))) : part)),
        _test_redirected: ($) => choice(redirected($, alias($._test_operand, $.command)), field('redirect', repeat1($._redirect))),
        _test_operand: ($) => seq(optional('!'), field('name', $.command_name), repeat(field('argument', $._literal))),
        command: drop((rule) => named('argument')(rule) && (reaches(rule, string('==')) || reaches(rule, symbol('_bare_dollar')))),
        command_name: widen(symbol('_literal'), ($) => alias($._brace_name, $.concatenation)),
        _brace_name: ($) => seq(alias($._brace_open, $.word), repeat1(seq($._concat, choice($._primary_expression, alias($._special_character, $.word), alias($._comment_word, $.word))))),
        file_redirect: widen(string('>|'), '<>'),
        herestring_redirect: ($) => prec.left(seq(field('descriptor', optional($.file_descriptor)), '<<<', $._literal, repeat(field('argument', $._literal)))),
        heredoc_redirect: ($) => prec.left(seq(field('descriptor', optional($.file_descriptor)), choice('<<', '<<-'), $.heredoc_start, repeat(field('argument', $._literal)))),
        _heredoc_body: ($) => seq($._heredoc_due, choice($.heredoc_body, alias($.simple_heredoc_body, $.heredoc_body)), $.heredoc_end),
        time_command: ($) => prec.left(-1, seq('time', optional(alias($._time_posix, '-p')), optional(alias($._time_end, '--')), optional(field('body', $._pipeline_statement)))),
        coproc_command: ($) => seq('coproc', choice(prec.dynamic(1, seq(optional(field('name', $.word)), field('body', choice(...compound($), alias($._redirected_compound, $.redirected_statement))))), field('body', choice($.command, alias($._redirected_command, $.redirected_statement))))),
        _redirected_compound: ($) => redirected($, choice(...compound($))),
        _redirected_command: ($) => redirected($, $.command),
        _literal: widen(symbol('concatenation'), ($) => alias($._bare_dollar, $.word)),
        _primary_expression: widen(symbol('word'), ($) => alias($._extglob_word, $.extglob_pattern)),
        command_substitution: ($, original) =>
            choice(
                rewrite(original, (part) => (part.type === 'SEQ' && string('`')(part.members.at(-1)) ? { ...part, members: part.members.toSpliced(-1, 0, optional($._backtick_end)) } : part)),
                seq(field('operator', choice(...[' ', '\t', '\n', '|'].map((separator) => alias(token(seq('${', repeat('\\\n'), separator)), separator === '|' ? '${|' : '${')))), optional($._terminated_statement), '}'),
            ),
        concatenation: drop(optionalOf((rule) => rule.type === 'SEQ' && reaches(rule, string('$')))),
        _expansion_expression: drop(symbol('array')),
        _expansion_regex_replacement: drop(symbol('array')),
        _concatenation_in_expansion: drop(symbol('variable_name'), symbol('array')),
    },
});

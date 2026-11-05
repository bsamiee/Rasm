import { Predicate } from 'effect';
import plpgsql from 'tree-sitter-postgres/plpgsql/grammar.js';
import postgres from 'tree-sitter-postgres/postgres/grammar.js';
import { extend, pattern, rebuild, rewrite, string, symbol, unicode, widen } from '../derive.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const RULES = plpgsql.grammar.rules;
const LEXICON = postgres.grammar.rules;
const STATEMENT = 'proc_stmt';
const TERMINATOR = ';';
const DELIMITER = '`';
const SEPARATOR = ',';
const KEYWORD = 'kw_';
const HOLE = 'substitution';
const WHITESPACE = /[\t\n\v\f\r ]+/u;
const UNICODE_PREFIX = /[uU]&/u;
const UNICODE_ESCAPE = /\s+[uU][eE][sS][cC][aA][pP][eE]\s+/u;
const [IDENTIFIER, ...QUOTED] = LEXICON._ident.members.map(({ name }) => name);
const [LITERAL, ...STRINGS] = LEXICON.Sconst.members.map(({ name }) => name);

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

// --- [LEXICON]

const escapedForm = (name) => `unicode_${name}`;
const symbols = (names) => names.map((name) => ($) => $[name]);
const lexical = [widen(symbol(IDENTIFIER), ...symbols([...QUOTED, ...QUOTED.map(escapedForm), HOLE])), widen(symbol(LITERAL), ...symbols([...STRINGS, escapedForm(LITERAL), HOLE]), ($) => alias($._format_string, $[LITERAL]))];
const escaped = (name) => [escapedForm(name), () => token(seq(UNICODE_PREFIX, LEXICON[name].content, optional(seq(UNICODE_ESCAPE, LEXICON[LITERAL].content))))];

// --- [FORMS]

const replacing = (rule, matches, apply) => rewrite(rule, (part) => (matches(part) ? apply(part) : part));
const cursorMove = ($, opener, ...rest) => seq(opener, optional($.fetch_direction), optional(choice($.kw_from, $.kw_in)), $._cursor_variable, ...rest);
const list = (item) => [item, repeat(seq(SEPARATOR, item))];
const decorated = (arm, tail) => seq(seq(...(arm.members.some(string('%')) ? arm.members : arm.members.slice(0, -1))), tail);
const forms = {
    ...Object.fromEntries(['into_target', 'for_variable', 'getdiag_list_item', '_cursor_variable', 'for_cursor'].map((name) => [name, ($) => replacing(RULES[name], symbol('any_identifier'), () => $.dotted_name)])),
    type_name: ($) =>
        replacing(
            choice(...RULES.type_name.members.map((arm) => decorated(arm, choice(RULES.type_name.members.at(-1).members.at(-1), seq($.kw_array, optional(seq('[', $.integer_literal, ']'))))))),
            (part) => part.type === 'SEQ' && string('(')(part.members[0]),
            () => seq('(', ...list($._sql_comma_rparen_expr), ')'),
        ),
    raise_option: () => replacing(RULES.raise_option, string('='), () => choice('=', ':=')),
    stmt_call: () => replacing(RULES.stmt_call, symbol('_sql_semicolon_expr'), (part) => field('call', part)),
    stmt_exit: () => replacing(RULES.stmt_exit, symbol('any_identifier'), (part) => field('label', part)),
    stmt_raise: () => prec(1, RULES.stmt_raise),
    stmt_dynexecute: ($) => seq($.kw_execute, $._sql_into_using_or_semicolon_expr, repeat(choice(seq($.kw_into, optional($.kw_strict), $.into_target), seq($.kw_using, ...list(alias($._sql_until_comma_into_or_semicolon, $.sql_expression))))), TERMINATOR),
    stmt_fetch: ($) => cursorMove($, $.kw_fetch, $.kw_into, $.into_target, TERMINATOR),
    stmt_move: ($) => cursorMove($, $.kw_move, TERMINATOR),
    fetch_direction: ($) => {
        const count = alias($._fetch_expression, $.sql_expression);
        const stepped = optional(choice(count, $.kw_all));
        return choice($.kw_next, $.kw_prior, $.kw_first, $.kw_last, $.kw_all, seq($.kw_absolute, count), seq($.kw_relative, count), seq($.kw_forward, stepped), seq($.kw_backward, stepped), alias($._fetch_count, $.sql_expression));
    },
};

// --- [ROLES]

const word = ({ type, name }) => type === 'SYMBOL' && name.startsWith(KEYWORD);
const silent = ({ type, members }) => type === 'CHOICE' && members.every((member) => word(member) || member.type === 'BLANK' || silent(member));
const spoken = ({ type, members }) => type === 'STRING' || (type === 'CHOICE' && members.every(spoken));
const infix = (members, at) => at > 0 && at < members.length - 1 && spoken(members[at]) && !spoken(members[at - 1]) && !spoken(members[at + 1]);
const trailing = ({ type, content }) => type === 'REPEAT' && content.type === 'SEQ' && string(SEPARATOR)(content.members[0]);
const role = (members, at) => {
    const member = members[at];
    const lead = members.slice(0, at).findLast(Predicate.not(silent));
    return lead !== undefined && word(lead) && !spoken(member) && !word(member) && !silent(member) ? lead.name.slice(KEYWORD.length) : undefined;
};
const placed = (members, at) => {
    const member = members[at];
    if (infix(members, at)) {
        return field('operator', member);
    }
    if (infix(members, at + 1)) {
        return field('left', member);
    }
    if (infix(members, at - 1)) {
        return field('right', member);
    }
    const name = role(members, trailing(member) ? at - 1 : at);
    if (name === undefined) {
        return member;
    }
    return trailing(member) ? repeat(seq(SEPARATOR, field(name, member.content.members[1]))) : field(name, member);
};
const shaped = ($, name) =>
    rewrite(
        lexical.reduce((held, edit) => edit($, held), forms[name]?.($) ?? RULES[name]),
        (part) =>
            part.type === 'SEQ'
                ? rebuild(
                      part,
                      part.members.map((_, at) => placed(part.members, at)),
                  )
                : part,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export default extend(plpgsql, {
    extras: ($) => [WHITESPACE, $.comment],
    externals: ($, original) => [...original, $._template_open, ...postgres.grammar.externals.map(({ name }) => $[name]), $.comment, $._sql_until_comma_into_or_semicolon, $._fetch_count, $._fetch_expression, $[HOLE], $._format_string],
    conflicts: ($, original) => [...original, [$.source_file, $._block_statement]],
    rules: {
        ...Object.fromEntries(Object.keys(RULES).map((name) => [name, ($) => shaped($, name)])),
        raise_level: ($) => prec.dynamic(1, choice(shaped($, 'raise_level'), $[HOLE])),
        source_file: ($) => {
            const statements = seq(repeat($[STATEMENT]), alias($._block_statement, $[STATEMENT]));
            return choice(RULES.source_file, seq(alias($._template_open, DELIMITER), statements, DELIMITER), statements);
        },
        _block_statement: ($) => open($, shaped($, STATEMENT)),
        ...Object.fromEntries([...opened].map((name) => [unterminated(name), ($) => open($, shaped($, name))])),
        ...Object.fromEntries([LITERAL, ...STRINGS, ...QUOTED].filter((name) => name in LEXICON).map((name) => [name, () => LEXICON[name]])),
        ...Object.fromEntries([...QUOTED, LITERAL].map(escaped)),
        [plpgsql.grammar.word]: () => rewrite(RULES[plpgsql.grammar.word], (part) => (part.type === 'PATTERN' ? pattern(unicode(part)) : part)),
    },
});

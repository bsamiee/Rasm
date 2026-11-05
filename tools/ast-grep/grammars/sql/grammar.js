import refa from 'refa';
import sqlite from 'tree-sitter-sqlite3/grammar.js';
import { compose, keywords } from '../compose.js';
import { hosted } from '../csharp/grammar.js';
import { any, automaton, chain, children, choiceOf, extend, mandatory, named, optionalOf, pattern, reaches, replace, rewrite, string, symbol, unicode, widen } from '../derive.js';
import tsx from '../typescript/grammar.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BYTE_ORDER_MARK = '\u{FEFF}';
const TRIVIA = ['\f', BYTE_ORDER_MARK];
const RULES = sqlite.grammar.rules;
const ASSIGNMENTS = RULES.update_statement.members.findIndex((member) => reaches(member, named('assignment')));
const CLAUSES = RULES._select_core.members.findIndex((member) => reaches(member, named('from')));
const VARIABLE_CHARACTER = '[0-9A-Za-z_$\\u{80}-\\u{10FFFF}]';
const VARIABLE_TAIL = `(?:${VARIABLE_CHARACTER}|::)*(?:\\([^\\t-\\r )]*\\))?`;
const VARIABLE = new RegExp(`(?:::)*${VARIABLE_CHARACTER}${VARIABLE_TAIL}`, 'u');
const REGISTERLESS = new RegExp(`(?:(?:::)+${VARIABLE_CHARACTER}|[A-Za-z_$\\u{80}-\\u{10FFFF}])${VARIABLE_TAIL}`, 'u');

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [IDENTIFIER]

const widened = unicode(RULES[sqlite.grammar.word]);
const marked = refa.DFA.fromFA(
    automaton({
        type: 'SEQ',
        members: [
            { type: 'STRING', value: BYTE_ORDER_MARK },
            { type: 'PATTERN', value: '(?s:.)*' },
        ],
    }),
);
marked.complement();
const identifier = refa.DFA.fromIntersection(widened, marked);
identifier.minimize();

// --- [DIALECT]

const hole = ($) => $.substitution;
const holed = ($, rule) => rewrite(rule, (part) => (symbol('_select_or_values')(part) ? choice(part, $.substitution) : part));
const keyworded = (value) => (rule) => rule.type === 'ALIAS' && rule.value === value;
const compound = (rule) => rule.type === 'ALIAS' && symbol('_select_compound')(rule.content);
const parenthesized = (rule) => rule.type === 'SEQ' && string('(')(rule.members[0]) && string(')')(rule.members.at(-1));
const enclosed = ($, { members }) => {
    const [opening, closing] = [members.findIndex(string('(')), members.findIndex(string(')')) + 1];
    return seq(...members.slice(0, opening), choice(seq(...members.slice(opening, closing)), $.substitution), ...members.slice(closing));
};
const conflicted = optionalOf((part) => reaches(part, named('conflict_resolution')));
const setting = replace(
    (part) => part.type === 'SEQ' && part.members.some(keyworded('SET')),
    ($, { members }) => {
        const at = members.findIndex(keyworded('SET'));
        return seq(...members.slice(0, at), choice($._set_clause, $.substitution), ...members.slice(at + 2));
    },
);
const tail = (name, members) => Object.fromEntries(Array.from({ length: members.length + 1 }, (_, at) => [`${name}${at}`, ($) => choice(prec.dynamic(1, seq($.substitution, optional($[`${name}${at}`]))), ...members.slice(at).map((member, offset) => seq(mandatory(member), optional($[`${name}${at + offset + 1}`]))))]));
const SELECT_TAIL = tail('_select_tail', RULES._select_core.members.slice(CLAUSES));
const keyword = keywords(sqlite);
const dialect = extend(sqlite, {
    extras: (_, original) => [...original, ...TRIVIA],
    rules: {
        [sqlite.grammar.word]: () => pattern(identifier),
        _result_columns: ($) => alias(seq($._result_column, repeat1(seq(',', $._result_column))), $.result_column_list),
        _column_alias: ($) => prec.dynamic(-2, $._id),
        expression_result_column: replace(named('alias'), ($) => field('alias', $._column_alias)),
        _typed_column: ($) => alias(mandatory(RULES.column_definition), $.column_definition),
        _set_clause: () => seq(...RULES.update_statement.members.slice(ASSIGNMENTS - 1, ASSIGNMENTS + 1)),
        _compounded: () => repeat1(RULES._select_compound.members.at(-1).content),
        _windowed: ($) => alias(choice(...RULES.window_definition.members.map(mandatory)), $.window_definition),
        bind_parameter: (
            _,
            {
                content: {
                    members: [
                        numbered,
                        {
                            members: [sigils],
                        },
                    ],
                },
            },
        ) => token(choice(numbered, seq(choice(...sigils.members.filter(({ value }) => value !== '#')), VARIABLE), seq('#', REGISTERLESS))),
        cast_expression: replace(named('type'), (_, part) => optional(part)),
        like_expression: widen(named('operator'), () => keyword('match')),
        binary_expression: (_, original) => ({ ...original, members: original.members.filter((member) => !reaches(member, keyworded('MATCH'))) }),
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export default compose(dialect, {
    name: 'sql',
    hosts: [
        { grammar: tsx, entry: 'template_string', hole: 'template_substitution' },
        { grammar: hosted, entry: 'interpolated_string_expression', hole: 'interpolation' },
    ],
    fragments: [
        '_expression',
        'order_term',
        'expression_result_column',
        'star_result_column',
        'qualified_star_result_column',
        '_result_columns',
        'from_clause',
        'join_step',
        'where_clause',
        'group_by_clause',
        'having_clause',
        'window_clause',
        'window_partition_clause',
        '_windowed',
        'order_by_clause',
        'limit_clause',
        'returning_clause',
        'with_clause',
        'default_values_clause',
        'upsert_clause',
        '_set_clause',
        '_compounded',
        'filter_clause',
        'over_clause',
        'column_constraint',
        'table_constraint',
        '_typed_column',
        'cte_definition',
        'named_window',
    ],
    slots: {
        _statement: [],
        _expression: [],
        _id: [],
        _function_name: [],
        upsert_clause: [],
        set_assignment: [],
        trigger_event: [],
        _typed_column: ['name'],
        insert_statement: ['with', 'source'],
        update_statement: ['with'],
        delete_statement: ['with'],
        begin_statement: ['mode'],
        create_table_statement: ['temporary'],
        create_view_statement: ['temporary', 'body'],
        create_index_statement: ['unique', 'where'],
        create_trigger_statement: ['temporary', 'timing', 'body'],
        normal_function_arguments: ['modifier', 'order_by_clause'],
        window_definition: ['window_partition_clause', 'order_by_clause', 'window_frame_clause'],
        with_clause: ['cte'],
        window_clause: ['named_window'],
        cte_definition: ['body'],
    },
    conflicts: ($) => [
        [$.insert_statement, $._id],
        [$._id, $._function_name],
        [$.null_literal, $.column_constraint],
        [$.column_reference, $.cte_definition, $.named_window],
        [$._select_core, $._id],
        [$.column_reference, $._typed_column],
        [$.column_constraint, $.table_constraint],
        [$.with_clause, $._id],
        [$.order_term],
        [$._select_core],
        ...Object.keys(SELECT_TAIL).map((name) => [$[name]]),
        [$._select_core, $.window_definition],
        [$._select_core, $.window_definition, $._id],
        [$.normal_function_arguments, $._function_name],
        [$.window_definition, $._id],
        [$._id, $._write_tail1],
        [$.update_statement, $._id],
    ],
    rules: {
        _compounded: holed,
        select_statement: ($, original) => choice(original, prec(-1, seq(field('with', $.substitution), original))),
        _select_compound: ($, original) => {
            const [opening, core, operands] = original.members;
            return choice(seq(opening, core, holed($, operands)), seq(opening, $.substitution, repeat1(holed($, operands.content))), seq(mandatory(opening), $.substitution));
        },
        _function_name: () => choice(...children(RULES._id).filter((member) => !reaches(member, symbol('string_literal')))),
        function_call: ($, original) =>
            widen(any(named('filter'), named('over')), hole)(
                $,
                rewrite(original, (rule) => (named('name')(rule) ? field('name', $._function_name) : rule)),
            ),
        in_list_expression: ($, original) => choice(original, prec.left(original.value, seq(...original.content.members.slice(0, original.content.members.findIndex(string('('))), field('values', $.substitution)))),
        order_term: widen(optionalOf(Boolean), hole),
        _select_core: ($, { members }) => choice(seq(...members.slice(0, CLAUSES), optional($._select_tail0)), seq($.substitution, choice(...members.slice(CLAUSES).map((member, index) => seq(mandatory(member), optional($[`_select_tail${index + 1}`])))))),
        ...SELECT_TAIL,
        ...tail('_write_tail', RULES.update_statement.members.slice(ASSIGNMENTS + 1)),
        values_clause: replace(parenthesized, ($, part) => choice(part, $.substitution)),
        insert_statement: widen(conflicted, hole),
        update_statement: ($, original) => {
            const { members } = setting($, original);
            return seq(...members.slice(0, ASSIGNMENTS), optional($._write_tail0));
        },
        delete_statement: ($, original) =>
            seq(
                ...original.members.slice(
                    0,
                    original.members.findIndex((member) => reaches(member, named('where'))),
                ),
                optional($._write_tail1),
            ),
        upsert_clause: chain(
            setting,
            widen(
                optionalOf((part) => reaches(part, named('target_column'))),
                hole,
            ),
            widen(choiceOf(keyworded('NOTHING')), hole),
        ),
        named_window: enclosed,
        exists_expression: chain(widen(compound, hole), enclosed),
        create_table_statement: widen(any(symbol('_create_table_columns'), compound), hole),
        _create_table_columns: replace(
            (part) => part.type === 'REPEAT1' && reaches(part, named('constraint')),
            ($, part) => seq(part.content, repeat(choice(part.content, $.substitution))),
        ),
        create_view_statement: widen(optionalOf(parenthesized), hole),
        create_index_statement: enclosed,
        create_trigger_statement: widen(
            optionalOf((part) => reaches(part, named('when'))),
            hole,
        ),
        table_options: ($, original) =>
            seq(
                widen(
                    choiceOf((part) => reaches(part, named('option'))),
                    hole,
                )($, original),
                optional($.substitution),
            ),
        reindex_statement: ($, original) => {
            const [opening, target] = original.members;
            return choice(original, seq(opening, keyword('expressions')), seq(opening, field(mandatory(target).name, alias(seq(field('schema', alias(keyword('expressions').content, $.identifier)), '.', field('name', $._id)), $.qualified_table_name))));
        },
        alter_table_statement: ($, original) =>
            choice(
                original,
                seq(
                    ...original.members.filter((_, index, members) => index <= members.findIndex(named('name'))),
                    choice(
                        $.substitution,
                        seq(keyword('drop'), keyword('constraint'), field('constraint', $._id)),
                        seq(keyword('alter'), optional(keyword('column')), field('altered_column', $._id), choice(seq(keyword('drop'), keyword('not'), keyword('null')), seq(keyword('set'), keyword('not'), keyword('null'), optional($._on_conflict_clause)))),
                        seq(keyword('add'), optional(seq(keyword('constraint'), field('constraint', $._id))), keyword('check'), '(', field('check', $._expression), ')', optional($._on_conflict_clause)),
                    ),
                ),
            ),
    },
});

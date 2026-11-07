import sqlite from 'tree-sitter-sqlite3/grammar.js';
import { compose, LINE_COMMENT } from '#/grammars/compose.js';
import { any, automaton, chain, children, choiceOf, collected, drop, extend, mandatory, named, optionalOf, pattern, reaches, replace, rewrite, string, symbol, unicode, widen, without } from '#/grammars/derive.js';

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
const NON_FALLBACK = ['deferrable', 'join', 'nothing', 'to', 'transaction'];
const INDEXED = 'indexed';
const RAISE_TYPES = ['rollback', 'abort', 'fail'];
const BARE_ARGUMENT_ARM = /\|\[\^,\(\)'"\]/u;
const QUOTED_ARGUMENT = '|`(?:[^`]|``)*`|\\[[^\\]]*\\]|[^,()\'"`\\[]';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [IDENTIFIER]

const identifier = without(unicode(RULES[sqlite.grammar.word]), automaton({ type: 'PATTERN', value: `${BYTE_ORDER_MARK}[^]*` }));

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
const KEYWORDS = new Map(
    Object.values(RULES)
        .flatMap((rule) => collected(rule, ({ type, named: visible, content }) => type === 'ALIAS' && !visible && content.type === 'PATTERN'))
        .map((rule) => [rule.value, rule]),
);
const keyword = (word) => KEYWORDS.get(word.toUpperCase()) ?? alias(token(prec(1, new RegExp([...word].map((letter) => `[${letter.toUpperCase()}${letter.toLowerCase()}]`).join(''), 'u'))), word.toUpperCase());

// --- [NAMES]

const spelledAs = (word) => (rule) => rule.type === 'PATTERN' && rule.value === keyword(word).content.value;
const unique = (words) => [...new Map(words.map((word) => [word.value, word])).values()];
const STRICT = [...sqlite.grammar.reserved.sql, ...NON_FALLBACK.map((word) => keyword(word).content)];
const JOIN_KINDS = collected(RULES._join_operator, ({ type, named: visible, content }) => type === 'ALIAS' && !visible && content.type === 'PATTERN').map(({ content }) => content);
const NAME = drop((part) => reaches(part, spelledAs('recursive')))(undefined, RULES._id);
const restricted = (set) => () => rewrite(NAME, (part) => (symbol('identifier')(part) ? reserved(set, part) : part));
const unaliased = (part) => part.type === 'SEQ' && part.members.length === 2 && optionalOf(keyworded('AS'))(part.members[0]) && named('alias')(part.members[1]);
const bare = replace(unaliased, ($, { members: [as, name] }) =>
    choice(
        seq(mandatory(as), name),
        rewrite(name, (part) => (symbol('_id')(part) ? $._ids : part)),
    ),
);
const unbounded = (direction) => (part) => part.type === 'SEQ' && part.members.some(keyworded('UNBOUNDED')) && part.members.some(keyworded(direction));
const dialect = extend(sqlite, {
    extras: (_, original) => [...original, ...TRIVIA],
    reserved: {
        sql: () => STRICT,
        ids: () => unique([...STRICT, keyword(INDEXED).content, ...JOIN_KINDS]),
        window: () => unique([...STRICT, keyword(INDEXED).content, keyword('filter').content]),
    },
    transforms: [bare],
    rules: {
        _id: () => NAME,
        _ids: restricted('ids'),
        _window_name: restricted('window'),
        _window_head: ($) => rewrite(RULES.named_window, (part) => (named('name')(part) ? field('name', $._window_name) : part)),
        window_clause: replace(
            (part) => part.type === 'SEQ' && symbol('named_window')(part.members[0]),
            ($, { members: [, ...rest] }) => seq(alias($._window_head, $.named_window), ...rest),
        ),
        over_clause: replace(named('window_name'), ($) => field('window_name', $._window_name)),
        window_frame_bound: drop(unbounded('FOLLOWING')),
        _window_frame_end: () => drop(unbounded('PRECEDING'))(undefined, RULES.window_frame_bound),
        window_frame_clause: replace(named('end'), ($) => field('end', alias($._window_frame_end, $.window_frame_bound))),
        _create_table_columns: replace(
            (part) => part.type === 'SEQ' && choiceOf(named('column'))(part.members[0]),
            (_, { members: [{ members: items }, { content: next }] }) => {
                const [comma] = next.members;
                const [column, constraints] = [items.find(named('column')), items.find((item) => reaches(item, named('constraint')))];
                return seq(column, repeat(seq(comma, column)), repeat(seq(comma, constraints)));
            },
        ),
        type_name: replace(symbol('_id'), ($) => $._ids),
        raise_expression: ($) => seq(keyword('raise'), '(', choice(keyword('ignore'), seq(field('type', choice(...RAISE_TYPES.map(keyword))), ',', field('message', $._expression))), ')'),
        _expression: ($, original) => choice(original, $.raise_expression),
        [sqlite.grammar.word]: () => pattern(identifier),
        unary_expression: (
            _,
            {
                value,
                content: {
                    members: [{ content: operators }, operand],
                },
            },
        ) => {
            const negated = keyworded('NOT');
            const level = RULES.binary_expression.members.find((member) => reaches(member, keyworded('AND'))).value + 1;
            const arm = (at, kept) => prec.right(at, seq(field('operator', kept), operand));
            return choice(arm(value, choice(...operators.members.filter((part) => !negated(part)))), arm(level, operators.members.find(negated)));
        },
        vtab_module_arg: (_, original) => rewrite(original, (part) => (part.type === 'PATTERN' ? { ...part, value: part.value.replace(BARE_ARGUMENT_ARM, QUOTED_ARGUMENT) } : part)),
        line_comment: () => token(LINE_COMMENT),
        _result_columns: ($) => alias(seq($._result_column, repeat1(seq(',', $._result_column))), $.result_column_list),
        expression_result_column: replace(named('alias'), (_, part) => field('alias', prec.dynamic(-2, part.content))),
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
        _ids: [],
        _window_name: [],
        _window_head: [],
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
        [$.expression_result_column, $.binary_expression],
        [$.binary_expression, $._ids],
        [$.where_clause, $.binary_expression],
        [$.limit_clause, $.binary_expression],
        [$.having_clause, $.binary_expression],
        [$.binary_expression],
        [$.order_term, $.binary_expression],
        [$.group_by_clause, $.binary_expression],
        [$._join_constraint, $.binary_expression],
        [$.window_partition_clause, $.binary_expression],
        [$.normal_function_arguments, $.binary_expression],
        [$.set_assignment, $.binary_expression],
        [$.insert_statement, $._ids],
        [$._id, $._function_name],
        [$.null_literal, $.column_constraint],
        [$.column_reference, $.cte_definition, $.named_window],
        [$._select_core, $._id],
        [$._select_core, $._ids],
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
        [$._ids, $._write_tail1],
        [$.update_statement, $._ids],
    ],
    rules: {
        _compounded: holed,
        select_statement: ($, original) => choice(original, prec(-1, seq(field('with', $.substitution), original))),
        _select_compound: ($, original) => {
            const [opening, core, operands] = original.members;
            return choice(seq(opening, core, holed($, operands)), seq(opening, $.substitution, repeat1(holed($, operands.content))), seq(mandatory(opening), $.substitution));
        },
        _function_name: () => choice(...children(NAME).filter((member) => !reaches(member, symbol('string_literal')))),
        binary_expression: ($, original) => choice(original, seq(field('left', $._expression), field('operator', $.substitution), field('right', $._expression))),
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
        _window_head: enclosed,
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

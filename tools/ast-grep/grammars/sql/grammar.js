import sqlite from 'tree-sitter-sqlite3/grammar.js';

// --- [OPERATIONS] ----------------------------------------------------------------------

const keyword = (word) => alias(new RegExp([...word].map((c) => `[${c.toUpperCase()}${c.toLowerCase()}]`).join(''), 'u'), word.toUpperCase());
const balanced = (depth) => (depth === 0 ? '[^{}]' : `(?:[^{}]|\\{${balanced(depth - 1)}*\\})`);

// --- [EXPORTS] -------------------------------------------------------------------------

export default grammar(sqlite, {
    name: sqlite.grammar.name,
    rules: {
        source_file: ($, original) => choice(original, seq('`', choice(original, $._expression, alias(seq($._expression, choice(keyword('asc'), keyword('desc'))), $.order_term), $.upsert_clause), '`')),
        upsert_clause: ($, original) => choice(original, $.template_substitution),
        template_substitution: () => {
            const nesting = 4;
            return token(new RegExp(`(?:\\$\\{${balanced(nesting)}*\\})+`, 'u'));
        },
        _statement: ($, original) => choice(original, $.template_substitution),
        _expression: ($, original) => choice(original, prec(1, $.template_substitution)),
        _id: ($, original) => choice(original, $.template_substitution),
        in_list_expression: ($, original) => choice(original, prec.left(original.value + 1, seq(field('operand', $._expression), optional(keyword('not')), keyword('in'), field('values', $.template_substitution)))),
        insert_statement: ($, original) =>
            choice(
                original,
                prec.dynamic(
                    1,
                    seq(
                        optional(field('with', $.with_clause)),
                        field('kind', choice(seq(keyword('insert'), optional(seq(keyword('or'), field('conflict_resolution', $._resolve_type)))), keyword('replace'))),
                        keyword('into'),
                        field('target', $.qualified_table_name),
                        field('source', $.template_substitution),
                        repeat(field('upsert', $.upsert_clause)),
                        optional(field('returning', $.returning_clause)),
                    ),
                ),
            ),
        values_clause: ($, original) => choice(original, seq(keyword('values'), $.template_substitution)),
        set_assignment: ($, original) => choice(original, $.template_substitution),
        table_options: ($, original) => choice(seq(original, optional($.template_substitution)), $.template_substitution),
        create_index_statement: ($, original) =>
            choice(
                original,
                seq(
                    keyword('create'),
                    field('unique', $.template_substitution),
                    keyword('index'),
                    optional($._if_not_exists),
                    field('name', $.qualified_table_name),
                    keyword('on'),
                    field('table', $._id),
                    '(',
                    field('column', $.indexed_column),
                    repeat(seq(',', field('column', $.indexed_column))),
                    ')',
                    optional(field('where', $.where_clause)),
                ),
            ),
        trigger_event: ($, original) => choice(original, $.template_substitution),
        create_trigger_statement: ($, original) =>
            choice(
                original,
                seq(
                    keyword('create'),
                    optional(field('temporary', choice(keyword('temp'), keyword('temporary')))),
                    keyword('trigger'),
                    optional($._if_not_exists),
                    field('name', $.qualified_table_name),
                    optional(field('timing', choice(keyword('before'), keyword('after'), seq(keyword('instead'), keyword('of'))))),
                    field('event', $.trigger_event),
                    keyword('on'),
                    field('table', $.qualified_table_name),
                    optional(seq(keyword('for'), keyword('each'), keyword('row'))),
                    optional(seq(keyword('when'), field('when', $._expression))),
                    keyword('begin'),
                    field('body', $.template_substitution),
                    keyword('end'),
                ),
            ),
        alter_table_statement: ($, original) =>
            choice(
                original,
                seq(
                    keyword('alter'),
                    keyword('table'),
                    field('name', $.qualified_table_name),
                    choice(
                        seq(keyword('drop'), keyword('constraint'), field('constraint', $._id)),
                        seq(keyword('alter'), optional(keyword('column')), field('altered_column', $._id), choice(seq(keyword('drop'), keyword('not'), keyword('null')), seq(keyword('set'), keyword('not'), keyword('null'), optional($._on_conflict_clause)))),
                        seq(keyword('add'), optional(seq(keyword('constraint'), field('constraint', $._id))), keyword('check'), '(', field('check', $._expression), ')', optional($._on_conflict_clause)),
                    ),
                ),
            ),
    },
});

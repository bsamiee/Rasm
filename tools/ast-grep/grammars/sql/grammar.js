import refa from 'refa';
import sqlite from 'tree-sitter-sqlite3/grammar.js';
import { compose, keywords, mandatory } from '../compose.js';
import csharp from '../csharp/grammar.js';
import { automaton, children, extend, named, pattern, reaches, rewrite, string, symbol, unicode } from '../derive.js';
import tsx from '../typescript/grammar.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BYTE_ORDER_MARK = '\u{FEFF}';
const TRIVIA = ['\f', BYTE_ORDER_MARK];

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [IDENTIFIER]

const widened = unicode(sqlite.grammar.rules[sqlite.grammar.word]);
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

const keyword = keywords(sqlite);
const dialect = extend(sqlite, {
    extras: (_, original) => [...original, ...TRIVIA],
    rules: { [sqlite.grammar.word]: () => pattern(identifier) },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export default compose(dialect, {
    name: 'sql',
    hosts: [
        { grammar: tsx, entry: 'template_string', hole: 'template_substitution' },
        { grammar: csharp, entry: 'interpolated_string_expression', hole: 'interpolation' },
    ],
    fragments: ['_expression', 'order_term', 'expression_result_column', 'upsert_clause', 'order_by_clause', 'limit_clause'],
    slots: {
        _statement: [],
        _expression: [],
        _id: [],
        _function_name: [],
        upsert_clause: [],
        set_assignment: [],
        trigger_event: [],
        insert_statement: ['source'],
        create_index_statement: ['unique'],
        create_trigger_statement: ['body'],
        cte_definition: ['body'],
    },
    conflicts: ($) => [[$.insert_statement, $._id]],
    rules: {
        _function_name: () => choice(...children(sqlite.grammar.rules._id).filter((member) => !reaches(member, symbol('string_literal')))),
        function_call: ($, original) => rewrite(original, (rule) => (named('name')(rule) ? field('name', $._function_name) : rule)),
        in_list_expression: ($, original) => choice(original, prec.left(original.value, seq(...original.content.members.slice(0, original.content.members.findIndex(string('('))), field('values', $.substitution)))),
        _select_core: ($, original) => {
            const trailing = original.members.slice(original.members.findIndex((member) => reaches(member, named('order_by'))));
            return choice(original, seq($.substitution, choice(...trailing.map((member, index) => seq(mandatory(member), ...trailing.slice(index + 1))))));
        },
        values_clause: ($, original) => choice(original, seq(original.members[0], $.substitution)),
        table_options: ($, original) => choice(seq(original, optional($.substitution)), $.substitution),
        reindex_statement: (_, original) => choice(original, seq(original.members[0], keyword('expressions'))),
        alter_table_statement: ($, original) =>
            choice(
                original,
                seq(
                    ...original.members.filter((_, index, members) => index <= members.findIndex(named('name'))),
                    choice(
                        seq(keyword('drop'), keyword('constraint'), field('constraint', $._id)),
                        seq(keyword('alter'), optional(keyword('column')), field('altered_column', $._id), choice(seq(keyword('drop'), keyword('not'), keyword('null')), seq(keyword('set'), keyword('not'), keyword('null'), optional($._on_conflict_clause)))),
                        seq(keyword('add'), optional(seq(keyword('constraint'), field('constraint', $._id))), keyword('check'), '(', field('check', $._expression), ')', optional($._on_conflict_clause)),
                    ),
                ),
            ),
    },
});

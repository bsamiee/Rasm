import refa from 'refa';
import sqlite from 'tree-sitter-sqlite3/grammar.js';
import { automaton, classes, pattern } from '../derive.js';
import tsx from '../typescript/grammar.js';

// --- [OPERATIONS] ----------------------------------------------------------------------

const keyword = (word) => alias(new RegExp([...word].map((c) => `[${c.toUpperCase()}${c.toLowerCase()}]`).join(''), 'u'), word.toUpperCase());
const hostName = (name) => (name.startsWith('_') ? `_host${name}` : `host_${name}`);
const hostExtras = Object.groupBy(tsx.grammar.extras, ({ type }) => type);
const hostSymbol = (rule) => (rule.type === 'SYMBOL' ? sym(hostName(rule.name)) : rule);
const mapChildren = (rule, map) => ({ ...('members' in rule ? { members: rule.members.map(map) } : {}), ...('content' in rule ? { content: map(rule.content) } : {}) });
const hostRule = (rule) =>
    rule.type === 'STRING' && rule.value === sqlite.grammar.rules.line_comment.content.members[0].value
        ? token(prec(1, rule))
        : {
              ...rule,
              ...mapChildren(rule, hostRule),
              ...(rule.type === 'SYMBOL' ? { name: hostName(rule.name) } : {}),
              ...((rule.type === 'ALIAS' && rule.named) || (rule.type.startsWith('PREC') && typeof rule.value === 'string') ? { value: hostName(rule.value) } : {}),
              ...(rule.type === 'RESERVED' ? { context_name: hostName(rule.context_name) } : {}),
          };
const byteOrderMark = '\uFEFF';
const sqlWhitespaceExtras = ['\f', byteOrderMark];
const sqlToken = automaton(sqlite.grammar.rules[sqlite.grammar.word]);
const nonAscii = refa.CharSet.fromRange(sqlToken.maxCharacter, { min: 0x80, max: sqlToken.maxCharacter });
const sqlIdentifier = refa.NFA.fromTransitionIterator(
    refa.FAIterators.mapOut(sqlToken.transitionIterator(), (transitions) => new Map(Array.from(transitions, ([state, characters]) => [state, characters.union(nonAscii)]))),
    { maxCharacter: sqlToken.maxCharacter },
);
const withoutInitialByteOrderMark = refa.DFA.fromFA(
    automaton({
        type: 'SEQ',
        members: [
            { type: 'STRING', value: byteOrderMark },
            { type: 'PATTERN', value: '(?s:.)*' },
        ],
    }),
);
withoutInitialByteOrderMark.complement();
const sqlWord = refa.DFA.fromIntersection(sqlIdentifier, withoutInitialByteOrderMark);
const hostWord = automaton(tsx.grammar.rules[tsx.grammar.word]);
const commonIdentifier = refa.DFA.fromIntersection(sqlWord, hostWord);
commonIdentifier.minimize();
const outsideCommon = commonIdentifier.copy();
outsideCommon.complement();
const exclusiveIdentifier = (fa) => {
    const exclusive = refa.DFA.fromIntersection(fa, outsideCommon);
    exclusive.minimize();
    return pattern(exclusive);
};
const sqlWhitespace = classes({ ...sqlite.grammar, extras: [...sqlite.grammar.extras, ...sqlWhitespaceExtras.map((value) => ({ type: 'STRING', value }))] }).whitespace;
const hostWhitespace = refa.DFA.fromCharSet(classes(tsx.grammar).whitespace.characters.without(sqlWhitespace.characters));

// --- [EXPORTS] -------------------------------------------------------------------------

export default grammar(sqlite, {
    name: sqlite.grammar.name,
    conflicts: ($, original) => [...original, ...tsx.grammar.conflicts.map((names) => names.map((name) => $[hostName(name)]))],
    externals: (_, original) => [...original, ...tsx.grammar.externals.map(hostSymbol)],
    extras: ($, original) => [...original, ...sqlWhitespaceExtras, ...hostExtras.SYMBOL.map(hostSymbol), $.host_whitespace],
    inline: ($, original) => [...original, ...tsx.grammar.inline.map((name) => $[hostName(name)]), $[sqlite.grammar.word], $[hostName(tsx.grammar.word)]],
    supertypes: ($, original) => [...original, ...tsx.grammar.supertypes.map((name) => $[hostName(name)])],
    precedences: (_, original) => [...original, ...tsx.grammar.precedences.map((rules) => rules.map((rule) => (rule.type === 'STRING' ? { ...rule, value: hostName(rule.value) } : hostSymbol(rule))))],
    reserved: { host: () => [] },
    word: ($) => $.common_identifier,
    rules: {
        ...Object.fromEntries(Object.keys(tsx.grammar.rules).map((name) => [hostName(name), () => hostRule(tsx.grammar.rules[name])])),
        common_identifier: () => pattern(commonIdentifier),
        _sql_identifier_token: () => exclusiveIdentifier(sqlWord),
        _host_identifier_token: () => exclusiveIdentifier(hostWord),
        [sqlite.grammar.word]: ($) => choice(alias(reserved('sql', $.common_identifier), $[sqlite.grammar.word]), alias($._sql_identifier_token, $[sqlite.grammar.word])),
        [hostName(tsx.grammar.word)]: ($) => choice(alias(reserved('host', $.common_identifier), $[hostName(tsx.grammar.word)]), alias($._host_identifier_token, $[hostName(tsx.grammar.word)])),
        host_whitespace: () => pattern(hostWhitespace),
        source_file: ($, original) =>
            choice(
                original,
                seq(
                    '`',
                    choice(
                        original,
                        $._expression,
                        alias(seq($._expression, choice(keyword('asc'), keyword('desc'))), $.order_term),
                        alias(seq(field('expression', $._expression), keyword('as'), field('alias', $._id)), $.expression_result_column),
                        seq($.template_substitution, choice(seq(field('order_by', $.order_by_clause), optional(field('limit', $.limit_clause))), field('limit', $.limit_clause))),
                        $.upsert_clause,
                    ),
                    '`',
                ),
            ),
        upsert_clause: ($, original) => choice(original, $.template_substitution),
        template_substitution: ($) => prec.right(repeat1($.host_template_substitution)),
        _statement: ($, original) => choice(original, $.template_substitution),
        _expression: ($, original) => choice(original, prec(1, $.template_substitution)),
        _id: ($, original) => choice(original, $.template_substitution),
        _function_name: ($) => choice(...sqlite.grammar.rules._id.members.filter((name) => name.type !== 'PREC' || name.content.name !== 'string_literal'), $.template_substitution),
        function_call: ($, original) => seq(...original.members.map((rule) => (rule.type === 'FIELD' && rule.name === 'name' ? field(rule.name, $._function_name) : rule))),
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

import { Array, Option, Predicate, Record, String } from 'effect';
import postgres from 'tree-sitter-postgres/postgres/grammar.js';
import { compose, LINE_COMMENT, level } from '../compose.js';
import { children, closure, collected, extend, lexical, mandatory, optionalOf, pattern, reaches, rebuild, rewrite, states, string, symbol, unicode } from '../derive.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LAYER = '_prec';
const QUANTIFIER = 'subquery_Op';
const QUANTIFIED = 'a_expr';
const OPERATOR = 'operator';
const SCANNED = `_${OPERATOR}`;
const NOT_EQUALS = ['<>', '!='];
const IDENT = '_ident';
const WIDE = ['unreserved_keyword', 'bare_label_keyword'];
const OPTIONAL = 'opt_';
const CASED = /\[(?<upper>.)(?<lower>.)\]/gu;
const UNICODE_ESCAPE = `(?:[\\t\\n\\v\\f\\r ]*[uU][eE][sS][cC][aA][pP][eE][\\t\\n\\v\\f\\r ]*'[^']')?`;
const ENTRY = 'toplevel_stmt';
const TAILED = ['a_expr', 'func_table'];
const COMMENT_TEXT = /[^*/-]+/u;
const COMMENT_MARK = /[-*/]/u;
const WHITESPACE = /[\t\n\v\f\r ]+/u;
const PLACEHOLDERS = [/%s/u, /%\([^)\n]+\)s/u];
const CLAUSES = [
    'from_clause',
    'where_clause',
    'group_clause',
    'having_clause',
    'window_clause',
    'sort_clause',
    'select_limit',
    'for_locking_clause',
    'returning_clause',
    'with_clause',
    'opt_on_conflict',
    'filter_clause',
    'over_clause',
    'join_qual',
    'when_clause',
    'merge_when_clause',
    'opt_partition_clause',
    'opt_frame_clause',
    'tablesample_clause',
    'ConstraintAttributeSpec',
    'key_actions',
    'opt_reloptions',
    'PartitionSpec',
    'TableLikeClause',
    'opt_if_not_exists',
];
const SLOTS = {
    [IDENT]: [],
    c_expr: [],
    qual_Op: [],
    table_ref: [],
    simple_select: [],
    comment_target: [],
    Sconst: [],
    common_table_expr: [],
    opt_unique: [],
    OptTemp: [],
    opt_or_replace: [],
    alter_table_cmd: [],
    ColConstraintElem: [],
    set_clause: [],
    join_qual: [],
    when_clause: [],
    opt_on_conflict: [],
    opt_conf_expr: [],
    merge_when_clause: [],
    PartitionBoundSpec: [],
    opt_concurrently: [],
    TableFuncElement: [],
    NumericOnly: [],
    Iconst: [],
    lock_type: [],
    opt_with_clause: [],
    utility_option_list: ['utility_option_elem'],
};
const RANK = Object.keys(SLOTS).length;
const FRAGMENTS = ['a_expr', 'target_list', 'table_ref', 'sortby_list', 'alter_table_cmd', 'ColConstraint', 'TableConstraint', 'comment_target', 'returning_list', 'alter_ts_configuration_cmd', 'columnDef', 'common_table_expr', 'window_definition', 'Typename', '_clauses'];

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SHAPES]

const deduped = (rows) => [...new Map(rows.map((row) => [JSON.stringify(row), row])).values()];
const bare = (rule) => (rule.type.startsWith('PREC') ? bare(rule.content) : rule);
const arms = (rule) => (bare(rule).type === 'CHOICE' ? bare(rule).members : [rule]);
const alternatives = (members) => (members.length === 1 ? members[0] : { type: 'CHOICE', members });
const within = (rule, apply) => (rule.type.startsWith('PREC') ? rebuild(rule, [within(rule.content, apply)]) : apply(rule));
const referenced = (rule) =>
    Option.map(
        Option.liftPredicate(bare(rule), ({ type }) => type === 'SYMBOL'),
        ({ name }) => name,
    );
const refers = (rule, name) => Option.exists(referenced(rule), (held) => held === name);

// --- [NORMAL_FORM]

const unlayered = (rules) => {
    const layer = (name) => `${name}${LAYER}`;
    const base = (name) => Object.keys(rules).find((held) => layer(held) === name) ?? name;
    const merged = (name, rule) => (layer(name) in rules ? alternatives([...arms(rules[layer(name)]), ...arms(rule).filter((arm) => !reaches(arm, symbol(layer(name))))]) : rule);
    const renamed = (rule) => rewrite(rule, (part) => (part.type === 'SYMBOL' ? { ...part, name: base(part.name) } : part));
    return Object.fromEntries(
        Object.entries(rules)
            .filter(([name]) => base(name) === name)
            .map(([name, rule]) => [name, renamed(merged(name, rule))]),
    );
};
const respelled = (rules) => Record.map(rules, (rule) => rewrite(rule, (part) => (string(NOT_EQUALS[0])(part) ? alternatives(NOT_EQUALS.map((value) => ({ type: 'STRING', value }))) : part)));
const leaves = (rules, rule) =>
    arms(rule).flatMap((arm) =>
        Option.match(
            Option.filter(referenced(arm), (name) => !lexical(rules[name])),
            { onNone: () => [arm], onSome: (name) => leaves(rules, rules[name]) },
        ),
    );
const quantified = (rules) => {
    const shapes = arms(rules[QUANTIFIED]).filter((arm) => reaches(arm, symbol(QUANTIFIER)));
    const groups = [...Map.groupBy(leaves(rules, rules[QUANTIFIER]), (leaf) => JSON.stringify(within(leaf, () => ({ type: 'BLANK' })))).values()];
    const operator = (at) => `_${QUANTIFIER}${at}`;
    const placed = (shape, at) => within(groups[at][0], () => rewrite(bare(shape), (part) => (symbol(QUANTIFIER)(part) ? { type: 'ALIAS', content: { type: 'SYMBOL', name: operator(at) }, named: true, value: QUANTIFIER } : part)));
    return {
        ...rules,
        [QUANTIFIED]: alternatives([...arms(rules[QUANTIFIED]).filter((arm) => !shapes.includes(arm)), ...groups.flatMap((_, at) => shapes.map((shape) => placed(shape, at)))]),
        ...Object.fromEntries(groups.map((group, at) => [operator(at), alternatives(group.map(bare))])),
    };
};
const RULES = quantified(respelled(unlayered(postgres.grammar.rules)));
const BASE = { ...postgres.grammar, rules: RULES };

// --- [FACTS]

const keyword = (name) => RULES[name]?.type === 'TOKEN' && RULES[name].content.type === 'PREC' && RULES[name].content.value === 1 && RULES[name].content.content.type === 'PATTERN';
const spelling = (name) => RULES[name].content.content.value.replaceAll(CASED, '$<upper>').toUpperCase();
const category = (name) => arms(RULES[name]).every((arm) => Option.exists(referenced(arm), keyword));
const positional = (name) => bare(RULES[name]).type === 'CHOICE' && arms(RULES[name]).some((arm) => refers(arm, IDENT));
const identifying = (name, arm) => positional(name) && Option.exists(referenced(arm), category);
const chained = (name) => name !== 'source_file' && name !== ENTRY && !keyword(name) && arms(RULES[name]).every((arm) => Option.exists(referenced(arm), Predicate.not(keyword)) && !identifying(name, arm));
const reachable = new Map(Object.keys(RULES).map((name) => [name, closure(BASE, name)]));
const chains = Object.keys(RULES).filter(chained);
const stripped = (rule) => (rule.type.startsWith('PREC') ? stripped(rule.content) : rebuild(rule, children(rule).map(stripped)));
const distinct = (name) => deduped(arms(RULES[name]).map(stripped)).length === arms(RULES[name]).length;
const inlined = Object.keys(RULES).filter((name) => name.startsWith(OPTIONAL) && distinct(name) && !chains.includes(name) && !FRAGMENTS.includes(name) && !CLAUSES.includes(name) && !reaches(RULES[name], symbol(name)) && !postgres.grammar.conflicts.flat().includes(name));
const hidden = new Set([IDENT, ENTRY, ...chains, ...inlined]);
const unprefixed = (text, lead) => (text.startsWith(lead) ? text.slice(lead.length) : text);
const snake = (name) => unprefixed(String.pascalToSnake(unprefixed(name, '_')).replaceAll('__', '_'), OPTIONAL);

// --- [PRECEDENCE]

const sequence = (arm) => (bare(arm).type === 'SEQ' ? bare(arm).members : [bare(arm)]);
const leads = (short, long) => short.length < long.length && short.every((member, at) => JSON.stringify(member) === JSON.stringify(long[at]));
const lengthened = (arm, others) => arm.type === 'PREC_LEFT' && others.some((other) => other.type === 'PREC_LEFT' && other.value === arm.value && leads(sequence(other), sequence(arm)));
const extensions = (rule) =>
    rule.type === 'CHOICE'
        ? rebuild(
              rule,
              rule.members.map((arm) => (lengthened(arm, rule.members) ? { ...arm, value: arm.value + 1 } : arm)),
          )
        : rule;
const associated = (rule) => rewrite(rule, (part) => (part.type === 'PREC_LEFT' && bare(part).type === 'SEQ' && optionalOf(Predicate.isTruthy)(bare(part).members.at(-1)) ? { ...part, type: 'PREC_RIGHT' } : part));

// --- [LEXEMES]

const spanned = (lead, name) => () => token(new RegExp(`${lead}${RULES[name].content.value}${UNICODE_ESCAPE}`, 'u'));
const commented = (part) => token.immediate(prec(1, part));
const block = ($, opening) => seq(opening, repeat(choice(commented(COMMENT_TEXT), commented(COMMENT_MARK), $._nested_comment)), commented('*/'));
const extended =
    (name, ...more) =>
    () =>
        token(choice(RULES[name].content, ...more));

// --- [KEYWORDS]

const spelled = (rule) => rewrite(rule, (part) => (part.type === 'SYMBOL' && keyword(part.name) ? alias(part, spelling(part.name)) : part));
const scanned = ($, rule) => rewrite(rule, (part) => (symbol(OPERATOR)(part) ? alias($[SCANNED], $[OPERATOR]) : part));
const keywordsOf = (rule) => Array.getSomes(arms(rule).map((arm) => Option.filter(referenced(arm), keyword)));
const categoriesOf = (name) => Array.getSomes(arms(RULES[name]).map((arm) => Option.filter(referenced(arm), category)));
const identifiers = Object.keys(RULES).filter((name) => arms(RULES[name]).some((arm) => identifying(name, arm)));
const accepted = new Map(identifiers.map((name) => [name, new Set(categoriesOf(name).flatMap((held) => keywordsOf(RULES[held])))]));
const closed = (name) => RULES[name] === undefined || keyword(name) || identifiers.includes(name) || category(name);
const positions = states(BASE, 'source_file', closed);
const narrowest = (name) => !identifiers.some((other) => other !== name && accepted.get(name).isSupersetOf(accepted.get(other)));
const whole = (name) => (narrowest(name) ? Array.difference(categoriesOf(name), WIDE).flatMap((held) => keywordsOf(RULES[held])) : []);
const projected = deduped(positions.map((open) => ({ names: identifiers.filter((name) => open.has(name)), words: [...open].filter(keyword) })).filter(({ names, words }) => names.length > 0 && words.length > 0));
const settled = (held) => {
    const grown = ({ names, words }) => {
        const valid = new Set([...words, ...names.flatMap((name) => [...held.get(name)])]);
        return names.map((name) => [name, accepted.get(name).intersection(valid)]);
    };
    const next = projected.flatMap(grown).reduce((growing, [name, words]) => growing.set(name, growing.get(name).union(words)), new Map(held));
    return identifiers.every((name) => next.get(name).size === held.get(name).size) ? held : settled(next);
};
const contested = settled(new Map(identifiers.map((name) => [name, new Set(whole(name))])));
const contestedRule = (name) => `_${name}_keyword`;
const identified = ($, name, rule) => (identifiers.includes(name) ? choice(...arms(rule).filter((arm) => !identifying(name, arm)), ...(contested.get(name).size > 0 ? [alias($[contestedRule(name)], $.identifier)] : [])) : rule);

// --- [ROLES]

const spoken = (part) => part.type === 'STRING' || (part.type === 'ALIAS' && !part.named) || (part.type === 'CHOICE' && part.members.every(spoken));
const operator = (operand) => (part) => spoken(part) || Option.exists(referenced(part), (held) => !reachable.get(held).has(operand));
const infix = (name, members) => {
    const middle = members.slice(1, -1);
    return middle.length > 0 && Option.exists(referenced(members[0]), (operand) => refers(members.at(-1), operand) && middle.every(operator(operand)) && (operand === name || middle.some(spoken)));
};
const prefix = (name, members) => members.length >= 2 && refers(members.at(-1), name) && members.slice(0, -1).every(operator(name));
const role = (name, part) => {
    if (part.type !== 'SEQ') {
        return part;
    }
    const middle = (members) => members.map((member) => field('operator', member));
    if (infix(name, part.members)) {
        return seq(field('left', part.members[0]), ...middle(part.members.slice(1, -1)), field('right', part.members.at(-1)));
    }
    return prefix(name, part.members) ? seq(...middle(part.members.slice(0, -1)), field('operand', part.members.at(-1))) : part;
};

// --- [FIELDS]

const label = (counts) => (part) => {
    const target = part.type === 'SYMBOL' ? part.name : undefined;
    return target !== undefined && hidden.has(target) && counts.get(target) === 1 ? field(snake(target), part) : part;
};
const descend = (part, place) =>
    ({
        FIELD: () => part,
        ALIAS: () => part,
        SYMBOL: () => place(part),
    })[part.type]?.() ??
    rebuild(
        part,
        children(part).map((child) => descend(child, place)),
    );
const sequenced = (part, place) =>
    part.type === 'SEQ'
        ? rebuild(
              part,
              part.members.map((member) => descend(member, place)),
          )
        : rebuild(
              part,
              children(part).map((child) => sequenced(child, place)),
          );
const counted = (arm) => collected(arm, ({ type }) => type === 'SYMBOL').reduce((held, { name }) => held.set(name, (held.get(name) ?? 0) + 1), new Map());
const labelled = (rule) => {
    const body = bare(rule);
    const placed = (arm) => sequenced(arm, label(counted(arm)));
    return body.type === 'CHOICE' ? rewrite(rule, (part) => (part === body ? rebuild(part, part.members.map(placed)) : part)) : placed(rule);
};

// --- [SPLITS]

const shaped = ($, name) => labelled(rewrite(scanned($, spelled(identified($, name, TAILED.includes(name) ? associated(extensions(RULES[name])) : RULES[name]))), (part) => role(name, part)));
const shared = (lists) => {
    const differing = lists[0].findIndex((member, at) => lists.some((list) => JSON.stringify(list[at]) !== JSON.stringify(member)));
    return differing < 0 ? lists[0].findIndex(Predicate.not(spoken)) : differing;
};
const split = (name, part) => {
    const bounds = ($) => {
        const lists = arms(shaped($, name)).map((arm) => bare(arm).members);
        return { first: lists[0], head: shared(lists), tail: shared(lists.map((list) => list.toReversed())) };
    };
    return {
        [name]: ($) => {
            const { first, head, tail } = bounds($);
            return seq(...first.slice(0, head), $[part], ...first.slice(first.length - tail));
        },
        [part]: ($) => {
            const { head, tail } = bounds($);
            return choice(...arms(shaped($, name)).map((arm) => within(arm, (body) => seq(...body.members.slice(head, body.members.length - tail)))));
        },
    };
};

// --- [FORMS]

const headed = ($, name, lead) => arms(shaped($, name)).filter((arm) => bare(arm).type === 'SEQ' && reaches(bare(arm).members[0], symbol(lead)));
const tail = ($, name, lead) => alias(choice(...headed($, name, lead).map((arm) => within(arm, (body) => seq(...body.members.slice(1))))), $[name]);

// --- [HOLES]

const located = (body, name) => body.members.findIndex((member) => reaches(member, symbol(name)));
const holding = (rule, name) => arms(rule).filter((arm) => bare(arm).type === 'SEQ' && located(bare(arm), name) >= 0);
const replaced = (rule, name, apply) => choice(rule, ...deduped(holding(rule, name).map((arm) => within(arm, (body) => seq(...apply(body.members, located(body, name)))))));
const following = ($, rule, name) => replaced(rule, name, (members, at) => [...members.slice(0, at + 1), $.substitution]);
const closing = ($, rule, name, ...rest) => replaced(rule, name, (members, at) => [...members.slice(0, at), $.substitution, ...rest]);
const spanning = ($, rule, from, to) =>
    choice(
        rule,
        ...holding(rule, from).map((arm) =>
            prec(
                RANK,
                within(arm, (body) => seq(...body.members.slice(0, located(body, from)), $.substitution, ...body.members.slice(located(body, to) + 1))),
            ),
        ),
    );

// --- [DIALECT]

const dialect = extend(postgres, {
    extras: ($) => [WHITESPACE, $.comment],
    externals: ($, original) => [...original, $[SCANNED]],
    conflicts: (_, original) => original.filter((rows) => !rows.some(({ name }) => category(name) || name === 'target_el')),
    supertypes: ($, original) => [...original, ...chains.map((name) => $[name])],
    inline: ($, original) => [...original, $[ENTRY], ...inlined.map((name) => $[name])],
    rules: {
        ...Object.fromEntries(identifiers.filter((name) => contested.get(name).size > 0).map((name) => [contestedRule(name), ($) => choice(...[...contested.get(name)].map((held) => alias($[held], spelling(held))))])),
        ...Object.fromEntries(Object.keys(RULES).map((name) => [name, ($) => shaped($, name)])),
        ...split('CommentStmt', 'comment_target'),
        ...split('returning_clause', 'returning_list'),
        ...split('AlterTSConfigurationStmt', 'alter_ts_configuration_cmd'),
        _joined: ($) => tail($, 'joined_table', 'table_ref'),
        _compounded: ($) => tail($, 'simple_select', 'select_clause'),
        _assigned: ($) => seq(spelled($.kw_set), $.set_clause_list),
        _clauses: ($) => repeat1(choice(...CLAUSES.map((name) => $[name]), $._joined, $._compounded, $._assigned)),
        integer_literal: extended('integer_literal', /0[xX](?:_?[0-9a-fA-F])+/u, /0[oO](?:_?[0-7])+/u, /0[bB](?:_?[01])+/u),
        float_literal: extended('float_literal', /[0-9](?:_?[0-9])*\.(?:[eE][+-]?[0-9](?:_?[0-9])*)?/u),
        string_literal: spanned('(?:[nN]|[uU]&)?', 'string_literal'),
        quoted_identifier: spanned('(?:[uU]&)?', 'quoted_identifier'),
        comment: ($) => choice(LINE_COMMENT, block($, '/*')),
        _nested_comment: ($) => block($, commented('/*')),
        param: (_, original) => token(choice(original, ...PLACEHOLDERS)),
        [postgres.grammar.word]: () => pattern(unicode(RULES[postgres.grammar.word])),
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export default compose(dialect, {
    name: 'pgsql',
    fragments: FRAGMENTS,
    slots: SLOTS,
    conflicts: ($) => [
        [$.ColConstraintElem, $.ConstraintElem],
        [$.columnref, $.func_name],
        [$.Sconst, $._ident],
        [$.columnref, $.qualified_name, $.func_name],
        [$.ConstraintElem, $._ident],
        [$.ColId, $.type_function_name],
        [$.columnref, $.qualified_name],
        [$.func_expr, $.func_expr_windowless],
        [$.SelectStmt, $.table_ref, $.c_expr],
        [$.ColConstraintElem, $.AexprConst],
        [$.ColConstraintElem, $.a_expr],
        [$.object_type_any_name, $.simple_select],
        [$.TableConstraint, $._ident],
        [$.ConstraintAttr, $.ConstraintAttributeElem],
        [$.GenericType, $.func_name],
        [$.RepackStmt],
        [$.VacuumStmt],
        [$.AnalyzeStmt],
        [$.source_file, $.sortby, $.target_el],
        [$.sortby, $.target_el],
        [$.source_file, $.returning_list],
        [$.columnDef, $.columnref, $.qualified_name],
        [$.window_definition, $.columnref, $.qualified_name, $.name],
        [$.columnDef, $.qualified_name],
        [$.DoStmt, $.dostmt_opt_list],
        [$.Iconst, $._ident],
        [$.ReindexStmt],
        [$.set_rest_more],
        [$.fetch_args, $.first_or_next],
        [$.AlterDatabaseStmt],
        [$.AlterRoleStmt],
        [$.AlterTableStmt, $._ident],
        [$.CreatedbStmt],
        [$.CreateGroupStmt],
        [$.CreateRoleStmt],
        [$.CreateSchemaStmt],
        [$.CreateUserStmt],
        [$.createdb_opt_list, $.createdb_opt_items],
        [$.AlterOptRoleList, $.AlterRoleStmt],
        [$.SeqOptElem],
        [$.CreateDomainStmt],
        [$.OptRoleList, $.CreateGroupStmt],
        [$.CreateRoleStmt, $.OptRoleList],
        [$.CreateSchemaStmt, $.OptSchemaEltList],
        [$.OptRoleList, $.CreateUserStmt],
        [$.InsertStmt],
        [$.ColQualList, $.CreateDomainStmt],
        [$.a_expr],
        [$.columnDef],
        [$.columnDef, $.ColQualList],
        [$.alter_identity_column_option],
        [$.opt_on_conflict],
        [$.Iconst, $.Sconst],
        [$.Iconst, $.Sconst, $._ident],
        [$.SelectStmt, $.c_expr],
        [$.key_actions],
        [$.source_file, $.ConstraintAttributeSpec],
        [$.source_file, $.select_no_parens],
        [$.select_limit],
        [$.for_locking_clause, $.for_locking_items],
        [$.opt_on_conflict, $.merge_when_clause, $.join_qual, $.c_expr, $.when_clause, $.Iconst, $.Sconst, $._ident],
        [$.opt_on_conflict, $.merge_when_clause, $.simple_select, $.join_qual, $.when_clause],
        [$.opt_on_conflict, $.merge_when_clause, $.join_qual, $.when_clause],
        [$.CreateStmt, $.opt_on_conflict, $.merge_when_clause, $.join_qual, $.when_clause],
        [$.simple_select, $.c_expr, $.Iconst, $.Sconst, $._ident],
        [$.c_expr, $.when_clause, $.Iconst, $.Sconst, $._ident],
        [$.func_expr],
        [$.alter_table_cmd, $.ConstraintAttributeElem],
        [$.select_fetch_first_value, $.a_expr],
        [$.var_name, $.set_target],
        [$.IndexStmt, $._ident],
        [$.ReindexStmt, $._ident],
        [$.I_or_F_const, $.AexprConst],
        [$.sortby],
        [$.ColId, $.ColLabel],
        [$.RefreshMatViewStmt, $._ident],
        [$.MergeStmt, $.merge_when_list],
        [$.columnDef, $.ConstraintElem, $.opt_on_conflict, $.merge_when_clause, $.join_qual, $.c_expr, $.qual_Op, $.when_clause, $.Iconst, $.Sconst, $._ident],
        [$.columnDef, $.c_expr, $.qual_Op, $.Iconst, $.Sconst, $._ident],
        [$.columnDef, $.opt_on_conflict, $.merge_when_clause, $.join_qual, $.qual_Op, $.when_clause, $._ident],
        [$.columnDef, $.ConstraintElem, $.opt_on_conflict, $.merge_when_clause, $.join_qual, $.when_clause],
        [$.opt_on_conflict, $.merge_when_clause, $.join_qual, $.qual_Op, $.when_clause, $._ident],
        [$.Numeric, $._ColId_keyword],
        [$.BitWithoutLength, $._ColId_keyword],
        [$._ColId_keyword, $._type_function_name_keyword],
        [$.character, $._ColId_keyword],
        [$.object_type_any_name, $._type_function_name_keyword],
        [$.func_expr_common_subexpr, $._type_function_name_keyword],
        [$.object_type_name, $._ColId_keyword],
        [$.c_expr, $._ColId_keyword],
        [$.JsonType, $._ColId_keyword],
        [$.drop_type_name, $._ColId_keyword],
        [$.PrepareStmt, $._ColId_keyword],
        [$.TransactionStmt, $.PrepareStmt, $._ColId_keyword],
        [$.TransactionStmt, $._ColId_keyword],
        [$.object_type_any_name, $._ColId_keyword],
        [$.ConstDatetime, $._ColId_keyword],
        [$.values_clause, $._ColId_keyword],
        [$.DeallocateStmt, $._ColId_keyword],
        [$.fetch_args, $._ColId_keyword],
        [$.RenameStmt, $.AlterOwnerStmt, $._ColId_keyword],
        [$.fetch_args, $.first_or_next, $._ColId_keyword],
        [$.utility_option_name, $._NonReservedWord_keyword],
        [$.OptTempTableName, $._ColId_keyword],
        [$.alter_generic_option_elem, $._ColLabel_keyword],
        [$.OptConsTableSpace, $._ColId_keyword],
        [$.copy_generic_opt_elem, $._ColLabel_keyword],
        [$.columnDef, $.opt_on_conflict, $.merge_when_clause, $.table_ref, $.join_qual, $.qual_Op, $.when_clause, $._ident],
        [$.columnDef, $.opt_on_conflict, $.merge_when_clause, $.join_qual, $.c_expr, $.qual_Op, $.when_clause, $.Iconst, $.Sconst, $._ident],
        [$.object_type_name, $._ColId_keyword, $._type_function_name_keyword],
        [$.ConstInterval, $._ColId_keyword],
        [$.drop_type_name, $._ColId_keyword, $._type_function_name_keyword],
        [$.PrepareStmt, $._ColId_keyword, $._type_function_name_keyword],
        [$.TransactionStmt, $._ColId_keyword, $._type_function_name_keyword],
        [$.object_type_any_name, $._ColId_keyword, $._type_function_name_keyword],
        [$.columnDef, $.columnref],
        [$.values_clause, $._NonReservedWord_keyword],
        [$.set_clause, $.a_expr],
    ],
    rules: {
        [ENTRY]: ($, original) => choice(original, seq($.stmt, $.substitution)),
        func_name: ($, original) => choice(original, prec(RANK + 1, $.substitution)),
        ConstraintElem: ($, original) => choice(original, seq($.substitution, optional($.ConstraintAttributeSpec))),
        TableConstraint: ($, original) => closing($, original, 'name', optional($.ConstraintAttributeSpec)),
        AlterTableStmt: ($, original) => closing($, original, 'relation_expr'),
        AlterTSConfigurationStmt: ($, original) => closing($, original, 'any_name'),
        CreateStmt: ($, original) =>
            choice(
                replaced(original, 'OnCommitOption', (members, at) => [$.substitution, mandatory(members[at]), ...members.slice(at + 1)]),
                following($, original, 'qualified_name'),
            ),
        UpdateStmt: ($, original) => spanning($, original, 'kw_set', 'set_clause_list'),
        opt_on_conflict: ($, original) => spanning($, original, 'kw_set', 'set_clause_list'),
        window_clause: ($, original) => following($, original, 'kw_window'),
        insert_rest: ($, original) => choice(original, ...holding(original, 'insert_column_list').map((arm) => within(arm, (body) => seq(...body.members.slice(0, located(body, 'insert_column_list')), $.substitution, ...body.members.slice(located(body, 'insert_column_list') + 1, -1), optional(body.members.at(-1)))))),
        simple_select: ($, original) => replaced(original, 'from_clause', (members, at) => [...members.slice(0, at), prec(RANK, $.substitution), ...members.slice(at + 1)]),
        values_clause: ($, original) => choice(original, ...holding(original, 'values_clause').map((arm) => prec(-1, seq(...bare(arm).members.map((member) => (symbol('values_clause')(member) ? $.substitution : member))))), seq(spelled($.kw_values), $.substitution), seq($.values_clause, ',', $.substitution)),
        columnDef: ($, original) =>
            choice(
                spanning($, original, 'ColId', 'Typename'),
                ...holding(original, 'ColId').map((arm) =>
                    prec(
                        level(dialect.grammar, SLOTS, 'c_expr'),
                        within(arm, (body) => seq($.substitution, ...body.members.slice(1))),
                    ),
                ),
            ),
        CreateTrigStmt: ($, original) => spanning($, original, 'TriggerActionTime', 'TriggerEvents'),
        select_no_parens: ($, original) =>
            choice(
                original,
                ...headed($, 'select_no_parens', 'with_clause').map((arm) =>
                    prec(
                        RANK,
                        within(arm, (body) => seq($.substitution, $.simple_select, ...body.members.slice(2))),
                    ),
                ),
            ),
        a_expr: ($, original) =>
            choice(
                original,
                ...arms(original)
                    .filter((arm) => reaches(arm, string('::')))
                    .map((arm) => within(arm, (body) => seq(body.members[0], prec(-1, $.substitution)))),
            ),
    },
});

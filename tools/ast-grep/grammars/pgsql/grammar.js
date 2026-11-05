import { Option, Predicate } from 'effect';
import refa from 'refa';
import postgres from 'tree-sitter-postgres/postgres/grammar.js';
import { compose } from '../compose.js';
import { automaton, children, closure, collected, extend, mandatory, minimal, optionalOf, pattern, reaches, rebuild, rewrite, string, symbol, unicode } from '../derive.js';
import tsx from '../typescript/grammar.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const RULES = postgres.grammar.rules;
const IDENT = '_ident';
const OPTIONAL = 'opt_';
const CASED = /\[(?<upper>.)(?<lower>.)\]/gu;
const CAMEL = /(?<tail>[a-z0-9])(?<head>[A-Z])/gu;
const ACRONYM = /(?<last>[A-Z])(?<head>[A-Z][a-z])/gu;
const UNICODE_ESCAPE = `(?:[\\t\\n\\v\\f\\r ]*[uU][eE][sS][cC][aA][pP][eE][\\t\\n\\v\\f\\r ]*'[^']')?`;
const ENTRY = 'toplevel_stmt';
const TAILED = ['a_expr', 'func_table'];
const NESTING = 3;
const LINE_COMMENT = /--(?:[^\r\n`]|`[^\r\n`]*`)*/u;
const WHITESPACE = /[\t\n\v\f\r ]+/u;
const SPLIT_OPERATOR = String.raw`//.*|.*(?:--|/\*).*`;
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
};
const RANK = Object.keys(SLOTS).length;
const FRAGMENTS = ['a_expr', 'target_list', 'table_ref', 'sortby_list', 'alter_table_cmd', 'ColConstraint', 'TableConstraint', 'comment_target', 'returning_list', 'alter_ts_configuration_cmd', 'columnDef', 'common_table_expr', 'window_definition', 'Typename', '_clauses'];

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [FACTS]

const bare = (rule) => (rule.type.startsWith('PREC') ? bare(rule.content) : rule);
const arms = (rule) => (bare(rule).type === 'CHOICE' ? bare(rule).members : [rule]);
const referenced = (rule) =>
    Option.map(
        Option.liftPredicate(bare(rule), ({ type }) => type === 'SYMBOL'),
        ({ name }) => name,
    );
const refers = (rule, name) => Option.exists(referenced(rule), (held) => held === name);
const keyword = (name) => RULES[name]?.type === 'TOKEN' && RULES[name].content.type === 'PREC' && RULES[name].content.value === 1 && RULES[name].content.content.type === 'PATTERN';
const spelling = (name) => RULES[name].content.content.value.replaceAll(CASED, '$<upper>').toUpperCase();
const category = (name) => arms(RULES[name]).every((arm) => Option.exists(referenced(arm), keyword));
const positional = (name) => bare(RULES[name]).type === 'CHOICE' && arms(RULES[name]).some((arm) => refers(arm, IDENT));
const identifying = (name, arm) => positional(name) && Option.exists(referenced(arm), category);
const chained = (name) => name !== 'source_file' && name !== ENTRY && !keyword(name) && arms(RULES[name]).every((arm) => Option.exists(referenced(arm), Predicate.not(keyword)) && !identifying(name, arm));
const reachable = new Map(Object.keys(RULES).map((name) => [name, closure(postgres.grammar, name)]));
const chains = Object.keys(RULES).filter(chained);
const stripped = (rule) => (rule.type.startsWith('PREC') ? stripped(rule.content) : rebuild(rule, children(rule).map(stripped)));
const distinct = (name) => new Set(arms(RULES[name]).map((arm) => JSON.stringify(stripped(arm)))).size === arms(RULES[name]).length;
const inlined = Object.keys(RULES).filter((name) => name.startsWith(OPTIONAL) && distinct(name) && !chains.includes(name) && !FRAGMENTS.includes(name) && !CLAUSES.includes(name) && !reaches(RULES[name], symbol(name)) && !postgres.grammar.conflicts.flat().includes(name));
const hidden = new Set([IDENT, ENTRY, ...chains, ...inlined]);
const level = (name) => Object.keys(SLOTS).filter((other) => other !== name && reachable.get(name).has(other)).length;
const unprefixed = (text, lead) => (text.startsWith(lead) ? text.slice(lead.length) : text);
const snake = (name) => unprefixed(unprefixed(name, '_').replaceAll(CAMEL, '$<tail>_$<head>').replaceAll(ACRONYM, '$<last>_$<head>').toLowerCase(), OPTIONAL);

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

const spanned = (lead, name) => () => token(new RegExp(`${lead}${RULES[name].content.value}${UNICODE_ESCAPE}`));
const opened = (depth) => `\\*(?:[^*/]|\\*+[^*/]|/+[^*/]${depth > 0 ? `|/+${opened(depth - 1)}` : ''})*\\*+/`;
const extended = (name, ...more) => () => token(choice(RULES[name].content, ...more));
const unsplit = () => {
    const split = minimal(automaton({ type: 'PATTERN', value: SPLIT_OPERATOR }));
    split.complement();
    return minimal(refa.DFA.fromIntersection(minimal(automaton(RULES.operator)), split));
};

// --- [KEYWORDS]

const spelled = (rule) => rewrite(rule, (part) => (part.type === 'SYMBOL' && keyword(part.name) ? alias(part, spelling(part.name)) : part));
const identified = ($, name, rule) =>
    rewrite(rule, (part) =>
        part.type === 'CHOICE'
            ? rebuild(
                  part,
                  part.members.map((arm) => (identifying(name, arm) ? alias(arm, $.identifier) : arm)),
              )
            : part,
    );
const keywords = Object.keys(RULES)
    .filter(keyword)
    .map((name) => spelling(name).toLowerCase());
const stranded = (word) => keywords.some((held) => held !== word && word.startsWith(held) && !keywords.some((other) => other.startsWith(word.slice(0, held.length + 1))));
const outranked = (host) => ({
    grammar: {
        ...host.grammar,
        rules: Object.fromEntries(Object.entries(host.grammar.rules).map(([name, rule]) => [name, rewrite(rule, (part) => (part.type === 'STRING' && stranded(part.value) ? token(prec(1, part)) : part))])),
    },
});

// --- [ROLES]

const spoken = (part) => part.type === 'STRING' || (part.type === 'ALIAS' && !part.named);
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
const roles = (name, rule) => rewrite(rule, (part) => role(name, part));

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

const within = (rule, apply) => (rule.type.startsWith('PREC') ? rebuild(rule, [within(rule.content, apply)]) : apply(rule));
const shaped = ($, name) => labelled(roles(name, spelled(identified($, name, TAILED.includes(name) ? associated(extensions(RULES[name])) : RULES[name]))));
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
const quantified = ($) =>
    arms(RULES.a_expr)
        .filter((arm) => reaches(arm, symbol('subquery_Op')))
        .map((arm) => within(arm, (body) => seq(field('left', $.a_expr_prec), alias($.MathOp, $.subquery_Op), ...body.members.slice(2))));

// --- [HOLES]

const distinctly = (rules) => [...new Map(rules.map((rule) => [JSON.stringify(rule), rule])).values()];
const located = (body, name) => body.members.findIndex((member) => reaches(member, symbol(name)));
const holding = (rule, name) => arms(rule).filter((arm) => bare(arm).type === 'SEQ' && located(bare(arm), name) >= 0);
const replaced = (rule, name, apply) => choice(rule, ...distinctly(holding(rule, name).map((arm) => within(arm, (body) => seq(...apply(body.members, located(body, name)))))));
const closing = ($, rule, name, ...rest) => replaced(rule, name, (members, at) => [...members.slice(0, at), $.substitution, ...rest]);
const opening = ($, rule, name) => replaced(rule, name, (members, at) => [$.substitution, mandatory(members[at]), ...members.slice(at + 1)]);
const spanning = ($, rule, from, to) => choice(rule, ...holding(rule, from).map((arm) => prec(RANK, within(arm, (body) => seq(...body.members.slice(0, located(body, from)), $.substitution, ...body.members.slice(located(body, to) + 1))))));
const trailing = ($, rule, text) =>
    choice(
        rule,
        ...arms(rule)
            .filter((arm) => reaches(arm, string(text)))
            .map((arm) => within(arm, (body) => seq(body.members[0], $.substitution))),
    );
const listed = ($, rule, name) => choice(rule, ...holding(rule, name).map((arm) => within(arm, (body) => seq(...body.members.slice(0, located(body, name)), $.substitution, ...body.members.slice(located(body, name) + 1, -1), optional(body.members.at(-1))))));

// --- [DIALECT]

const dialect = extend(postgres, {
    extras: ($) => [WHITESPACE, $.comment],
    supertypes: ($, original) => [...original, ...chains.map((name) => $[name])],
    inline: ($, original) => [...original, $[ENTRY], ...inlined.map((name) => $[name])],
    rules: {
        ...Object.fromEntries(Object.keys(RULES).map((name) => [name, ($) => shaped($, name)])),
        ...split('CommentStmt', 'comment_target'),
        ...split('returning_clause', 'returning_list'),
        ...split('AlterTSConfigurationStmt', 'alter_ts_configuration_cmd'),
        a_expr_prec: ($) => choice(shaped($, 'a_expr_prec'), ...quantified($)),
        _joined: ($) => tail($, 'joined_table', 'table_ref'),
        _compounded: ($) => tail($, 'simple_select', 'select_clause'),
        _assigned: ($) => seq(spelled($.kw_set), $.set_clause_list),
        _clauses: ($) => repeat1(choice(...CLAUSES.map((name) => $[name]), $._joined, $._compounded, $._assigned)),
        integer_literal: extended('integer_literal', /0[xX](_?[0-9a-fA-F])+/, /0[oO](_?[0-7])+/, /0[bB](_?[01])+/),
        float_literal: extended('float_literal', /[0-9](_?[0-9])*\.([eE][+-]?[0-9](_?[0-9])*)?/),
        string_literal: spanned('(?:[nN]|[uU]&)?', 'string_literal'),
        quoted_identifier: spanned('(?:[uU]&)?', 'quoted_identifier'),
        comment: () => token(choice(LINE_COMMENT, new RegExp(`/${opened(NESTING)}`))),
        operator: () => pattern(unsplit()),
        [postgres.grammar.word]: () => pattern(unicode(RULES[postgres.grammar.word])),
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export default compose(dialect, {
    name: 'pgsql',
    hosts: [{ grammar: outranked(tsx), entry: 'template_string', hole: 'template_substitution' }],
    fragments: FRAGMENTS,
    slots: SLOTS,
    conflicts: ($) => [
        [$.OptConsTableSpace, $.unreserved_keyword],
        [$.ColConstraintElem, $.ConstraintElem],
        [$.alter_generic_option_elem, $.unreserved_keyword],
        [$.columnref, $.func_name],
        [$.Sconst, $._ident],
        [$.columnref, $.qualified_name, $.func_name],
        [$.DropTableSpaceStmt, $.unreserved_keyword],
        [$.DropSubscriptionStmt, $.unreserved_keyword],
        [$.DropRoleStmt, $.unreserved_keyword],
        [$.object_type_name_on_any_name, $.unreserved_keyword],
        [$.RemoveFuncStmt, $.unreserved_keyword],
        [$.drop_type_name, $.unreserved_keyword],
        [$.object_type_any_name, $.unreserved_keyword],
        [$.RemoveAggrStmt, $.unreserved_keyword],
        [$.RenameStmt, $.AlterObjectDependsStmt, $.unreserved_keyword],
        [$.AlterSystemStmt, $.unreserved_keyword],
        [$.AlterForeignServerStmt, $.RenameStmt, $.AlterOwnerStmt, $.unreserved_keyword],
        [$.RenameStmt, $.unreserved_keyword],
        [$.AlterRoleStmt, $.AlterRoleSetStmt, $.RenameStmt, $.unreserved_keyword],
        [$.AlterPolicyStmt, $.RenameStmt, $.unreserved_keyword],
        [$.RenameStmt, $.AlterOwnerStmt, $.unreserved_keyword],
        [$.AlterTableStmt, $.RenameStmt, $.AlterObjectDependsStmt, $.unreserved_keyword],
        [$.ConstraintElem, $._ident],
        [$.ColId, $.type_function_name],
        [$.columnref, $.qualified_name],
        [$.func_expr, $.func_expr_windowless],
        [$.SelectStmt, $.table_ref, $.c_expr],
        [$.VacuumStmt, $.unreserved_keyword],
        [$.UnlistenStmt, $.unreserved_keyword],
        [$.TruncateStmt, $.unreserved_keyword],
        [$.VariableShowStmt, $.unreserved_keyword],
        [$.RevokeStmt, $.unreserved_keyword],
        [$.RevokeStmt, $.RevokeRoleStmt, $.unreserved_keyword],
        [$.VariableResetStmt, $.unreserved_keyword],
        [$.ReindexStmt, $.unreserved_keyword],
        [$.RefreshMatViewStmt, $.unreserved_keyword],
        [$.ReassignOwnedStmt, $.unreserved_keyword],
        [$.TransactionStmt, $.PrepareStmt, $.unreserved_keyword],
        [$.PrepareStmt, $.unreserved_keyword],
        [$.ColConstraintElem, $.AexprConst],
        [$.NotifyStmt, $.unreserved_keyword],
        [$.FetchStmt, $.unreserved_keyword],
        [$.LockStmt, $.unreserved_keyword],
        [$.LoadStmt, $.unreserved_keyword],
        [$.ListenStmt, $.unreserved_keyword],
        [$.JsonType, $.col_name_keyword],
        [$.ConstInterval, $.col_name_keyword],
        [$.ImportForeignSchemaStmt, $.unreserved_keyword],
        [$.ExplainStmt, $.unreserved_keyword],
        [$.ExecuteStmt, $.unreserved_keyword],
        [$.DropRoleStmt, $.DropUserMappingStmt, $.unreserved_keyword],
        [$.alter_table_cmd, $.DropTransformStmt, $.unreserved_keyword],
        [$.alter_table_cmd, $.DropTableSpaceStmt, $.unreserved_keyword],
        [$.alter_table_cmd, $.DropSubscriptionStmt, $.unreserved_keyword],
        [$.DropRoleStmt, $.alter_table_cmd, $.unreserved_keyword],
        [$.alter_table_cmd, $.DropOwnedStmt, $.unreserved_keyword],
        [$.alter_table_cmd, $.RemoveFuncStmt, $.unreserved_keyword],
        [$.DropStmt, $.unreserved_keyword],
        [$.alter_table_cmd, $.RemoveAggrStmt, $.unreserved_keyword],
        [$.alter_table_cmd, $.DropStmt, $.unreserved_keyword],
        [$.Numeric, $.unreserved_keyword],
        [$.DiscardStmt, $.unreserved_keyword],
        [$.ColConstraintElem, $.a_expr],
        [$.DeclareCursorStmt, $.unreserved_keyword],
        [$.DeallocateStmt, $.unreserved_keyword],
        [$.func_expr_common_subexpr, $.type_func_name_keyword],
        [$.CopyStmt, $.unreserved_keyword],
        [$.RepackStmt, $.unreserved_keyword],
        [$.ClosePortalStmt, $.unreserved_keyword],
        [$.CheckPointStmt, $.unreserved_keyword],
        [$.character, $.col_name_keyword],
        [$.CallStmt, $.unreserved_keyword],
        [$.BitWithoutLength, $.col_name_keyword],
        [$.Numeric, $.col_name_keyword],
        [$.TransactionStmtLegacy, $.unreserved_keyword],
        [$.AlterRoleStmt, $.AlterRoleSetStmt, $.AlterUserMappingStmt, $.RenameStmt, $.unreserved_keyword],
        [$.AlterDefaultPrivilegesStmt, $.unreserved_keyword],
        [$.alter_table_cmd, $.unreserved_keyword],
        [$.TransactionStmt, $.unreserved_keyword],
        [$.alter_table_cmd, $.unreserved_keyword, $.alter_ts_configuration_cmd],
        [$.object_type_any_name, $.type_func_name_keyword],
        [$.object_type_name, $.unreserved_keyword],
        [$.object_type_any_name, $.simple_select],
        [$.columnDef, $.ConstraintElem],
        [$.columnDef, $._ident],
        [$.columnDef, $.ConstraintElem, $._ident],
        [$.TableConstraint, $._ident],
        [$.ConstraintAttr, $.ConstraintAttributeElem],
        [$.GenericType, $.func_name],
        [$.RepackStmt],
        [$.VacuumStmt],
        [$.AnalyzeStmt],
        [$.c_expr, $.col_name_keyword],
        [$.over_clause, $.unreserved_keyword],
        [$.source_file, $.sortby, $.target_el],
        [$.sortby, $.target_el],
        [$.a_expr_prec],
        [$.source_file, $.returning_list],
        [$.columnDef, $.columnref, $.qualified_name],
        [$.window_definition, $.columnref, $.qualified_name, $.name],
        [$.columnDef, $.columnref],
        [$.columnDef, $.qualified_name],
        [$.DoStmt, $.dostmt_opt_list],
        [$.fetch_args, $.unreserved_keyword],
        [$.Iconst, $._ident],
        [$.ReindexStmt],
        [$.set_rest_more],
        [$.fetch_args, $.first_or_next],
        [$.fetch_args, $.first_or_next, $.unreserved_keyword],
        [$.sortby, $.bare_label_keyword],
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
        [$.ConstraintAttr, $.ConstraintAttributeElem, $.unreserved_keyword],
        [$.ConstraintAttributeElem, $.unreserved_keyword],
        [$.alter_table_cmd, $.ConstraintAttributeElem, $.unreserved_keyword],
        [$.values_clause, $.col_name_keyword],
        [$.key_actions],
        [$.source_file, $.ConstraintAttributeSpec],
        [$.source_file, $.select_no_parens],
        [$.select_limit],
        [$.for_locking_clause, $.for_locking_items],
        [$.opt_on_conflict, $.merge_when_clause, $.table_ref, $.join_qual, $.c_expr, $.qual_Op, $.when_clause, $.Iconst, $.Sconst, $._ident],
        [$.opt_on_conflict, $.merge_when_clause, $.join_qual, $.c_expr, $.when_clause, $.Iconst, $.Sconst, $._ident],
        [$.opt_on_conflict, $.merge_when_clause, $.join_qual, $.c_expr, $.qual_Op, $.when_clause, $.Iconst, $.Sconst, $._ident],
        [$.opt_on_conflict, $.merge_when_clause, $.simple_select, $.join_qual, $.when_clause],
        [$.opt_on_conflict, $.merge_when_clause, $.join_qual, $.qual_Op, $.when_clause, $._ident],
        [$.opt_on_conflict, $.merge_when_clause, $.join_qual, $.when_clause],
        [$.CreateStmt, $.opt_on_conflict, $.merge_when_clause, $.join_qual, $.when_clause],
        [$.opt_on_conflict, $.merge_when_clause, $.simple_select, $.join_qual, $.c_expr, $.qual_Op, $.when_clause, $.Iconst, $.Sconst, $._ident],
        [$.simple_select, $.c_expr, $.Iconst, $.Sconst, $._ident],
        [$.c_expr, $.when_clause, $.Iconst, $.Sconst, $._ident],
        [$.c_expr, $.qual_Op, $.when_clause, $.Iconst, $.Sconst, $._ident],
        [$.func_expr],
        [$.alter_table_cmd, $.ConstraintAttributeElem],
        [$.select_fetch_first_value, $.a_expr_prec],
        [$.key_delete, $.unreserved_keyword],
        [$.key_update, $.unreserved_keyword],
        [$.var_name, $.set_target],
        [$.merge_when_tgt_matched, $.unreserved_keyword],
        [$.IndexStmt, $._ident],
        [$.ReindexStmt, $._ident],
        [$.simple_select, $.c_expr, $.qual_Op, $.Iconst, $.Sconst, $._ident],
        [$.I_or_F_const, $.AexprConst],
        [$.sortby],
        [$.ColId, $.ColLabel],
        [$.RefreshMatViewStmt, $._ident],
        [$.MergeStmt, $.merge_when_list],
        [$.columnDef, $.ConstraintElem, $.opt_on_conflict, $.merge_when_clause, $.join_qual, $.c_expr, $.when_clause, $.Iconst, $.Sconst, $._ident],
        [$.opt_with_clause, $.c_expr, $.qual_Op, $.Iconst, $.Sconst, $._ident],
        [$.columnDef, $.ConstraintElem, $.opt_on_conflict, $.merge_when_clause, $.join_qual, $.c_expr, $.qual_Op, $.when_clause, $.Iconst, $.Sconst, $._ident],
        [$.columnDef, $.opt_on_conflict, $.merge_when_clause, $.select_no_parens, $.simple_select, $.join_qual, $.c_expr, $.qual_Op, $.when_clause, $.Iconst, $.Sconst, $._ident],
        [$.select_no_parens, $._ident],
        [$.columnDef, $.c_expr, $.qual_Op, $.Iconst, $.Sconst, $._ident],
        [$.columnDef, $.opt_on_conflict, $.merge_when_clause, $.table_ref, $.join_qual, $.c_expr, $.qual_Op, $.when_clause, $.Iconst, $.Sconst, $._ident],
        [$.columnDef, $.opt_with_clause, $.c_expr, $.qual_Op, $.Iconst, $.Sconst, $._ident],
        [$.columnDef, $.opt_on_conflict, $.merge_when_clause, $.join_qual, $.qual_Op, $.when_clause, $._ident],
        [$.columnDef, $.opt_on_conflict, $.merge_when_clause, $.join_qual, $.c_expr, $.qual_Op, $.when_clause, $.Iconst, $.Sconst, $._ident],
        [$.column_compression, $.unreserved_keyword],
        [$.ConstraintAttr, $.unreserved_keyword],
        [$.column_storage, $.unreserved_keyword],
    ],
    rules: {
        [ENTRY]: ($, original) => choice(original, seq($.stmt, $.substitution)),
        ConstraintElem: ($, original) => choice(original, seq($.substitution, optional($.ConstraintAttributeSpec))),
        TableConstraint: ($, original) => closing($, original, 'name', optional($.ConstraintAttributeSpec)),
        AlterTableStmt: ($, original) => closing($, original, 'relation_expr'),
        AlterTSConfigurationStmt: ($, original) => closing($, original, 'any_name'),
        CreateStmt: ($, original) => opening($, original, 'OnCommitOption'),
        insert_rest: ($, original) => listed($, original, 'insert_column_list'),
        values_clause: ($, original) => choice(original, seq(spelled($.kw_values), $.substitution), seq($.values_clause, ',', $.substitution)),
        columnDef: ($, original) => choice(spanning($, original, 'ColId', 'Typename'), ...holding(original, 'ColId').map((arm) => prec(level('c_expr'), within(arm, (body) => seq($.substitution, ...body.members.slice(1)))))),
        CreateTrigStmt: ($, original) => spanning($, original, 'TriggerActionTime', 'TriggerEvents'),
        select_no_parens: ($, original) => choice(original, ...headed($, 'select_no_parens', 'with_clause').map((arm) => prec(RANK, within(arm, (body) => seq($.substitution, $.simple_select, ...body.members.slice(2)))))),
        a_expr_prec: ($, original) => trailing($, original, '::'),
    },
});

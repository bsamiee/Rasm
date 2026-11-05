import { Option, Predicate } from 'effect';
import refa from 'refa';
import postgres from 'tree-sitter-postgres/postgres/grammar.js';
import { compose, mandatory } from '../compose.js';
import { automaton, children, extend, pattern, reaches, rebuild, rewrite, symbol, token } from '../derive.js';
import tsx from '../typescript/grammar.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const RULES = postgres.grammar.rules;
const IDENT = '_ident';
const OPTIONAL = 'opt_';
const CASED = /\[(?<upper>.)(?<lower>.)\]/gu;
const CAMEL = /(?<tail>[a-z0-9])(?<head>[A-Z])/gu;
const ACRONYM = /(?<last>[A-Z])(?<head>[A-Z][a-z])/gu;
const FRAGMENTS = ['a_expr', 'target_el', 'table_ref', 'sortby', 'opt_on_conflict', 'alter_table_cmd', 'ColConstraintElem', 'TableConstraint', 'comment_target', 'returning_list', 'alter_ts_configuration_cmd'];

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
const symbols = (rule) => [...(rule.type === 'SYMBOL' ? [rule.name] : []), ...children(rule).flatMap(symbols)];
const keyword = (name) => RULES[name]?.type === 'TOKEN' && RULES[name].content.type === 'PREC' && RULES[name].content.value === 1 && RULES[name].content.content.type === 'PATTERN';
const spelling = (name) => RULES[name].content.content.value.replaceAll(CASED, '$<upper>').toUpperCase();
const category = (name) => arms(RULES[name]).every((arm) => Option.exists(referenced(arm), keyword));
const positional = (name) => bare(RULES[name]).type === 'CHOICE' && arms(RULES[name]).some((arm) => refers(arm, IDENT));
const identifying = (name, arm) => positional(name) && Option.exists(referenced(arm), category);
const chained = (name) => name !== 'source_file' && !keyword(name) && arms(RULES[name]).every((arm) => Option.exists(referenced(arm), Predicate.not(keyword)) && !identifying(name, arm));
const closure = (name, seen = new Set()) => (seen.has(name) || RULES[name] === undefined ? seen : symbols(RULES[name]).reduce((held, next) => closure(next, held), new Set([...seen, name])));
const reachable = new Map(Object.keys(RULES).map((name) => [name, closure(name)]));
const chains = Object.keys(RULES).filter(chained);
const stripped = (rule) => (rule.type.startsWith('PREC') ? stripped(rule.content) : rebuild(rule, children(rule).map(stripped)));
const distinct = (name) => new Set(arms(RULES[name]).map((arm) => JSON.stringify(stripped(arm)))).size === arms(RULES[name]).length;
const inlined = Object.keys(RULES).filter((name) => name.startsWith(OPTIONAL) && distinct(name) && !chains.includes(name) && !FRAGMENTS.includes(name) && !symbols(RULES[name]).includes(name) && !postgres.grammar.conflicts.flat().includes(name));
const hidden = new Set([IDENT, ...chains, ...inlined]);
const unprefixed = (text, lead) => (text.startsWith(lead) ? text.slice(lead.length) : text);
const snake = (name) => unprefixed(unprefixed(name, '_').replaceAll(CAMEL, '$<tail>_$<head>').replaceAll(ACRONYM, '$<last>_$<head>').toLowerCase(), OPTIONAL);

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
const counted = (arm) => symbols(arm).reduce((held, name) => held.set(name, (held.get(name) ?? 0) + 1), new Map());
const labelled = (rule) => {
    const body = bare(rule);
    const placed = (arm) => sequenced(arm, label(counted(arm)));
    return body.type === 'CHOICE' ? rewrite(rule, (part) => (part === body ? rebuild(part, part.members.map(placed)) : part)) : placed(rule);
};

// --- [IDENTIFIER]

const widened = () => {
    const word = automaton(RULES[postgres.grammar.word]);
    const above = refa.CharSet.fromRange(word.maxCharacter, { min: 0x80, max: word.maxCharacter });
    const dfa = refa.DFA.fromFA(
        refa.NFA.fromTransitionIterator(
            refa.FAIterators.mapOut(word.transitionIterator(), (transitions) => new Map(Array.from(transitions, ([state, characters]) => [state, characters.union(above)]))),
            { maxCharacter: word.maxCharacter },
        ),
    );
    dfa.minimize();
    return pattern(dfa);
};

// --- [SPLITS]

const within = (rule, apply) => (rule.type.startsWith('PREC') ? rebuild(rule, [within(rule.content, apply)]) : apply(rule));
const shaped = ($, name) => labelled(roles(name, spelled(identified($, name, RULES[name]))));
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

// --- [HOLES]

const distinctly = (rules) => [...new Map(rules.map((rule) => [JSON.stringify(rule), rule])).values()];
const located = (body, name) => body.members.findIndex((member) => reaches(member, symbol(name)));
const holding = (rule, name) => arms(rule).filter((arm) => bare(arm).type === 'SEQ' && located(bare(arm), name) >= 0);
const replaced = (rule, name, apply) => choice(rule, ...distinctly(holding(rule, name).map((arm) => within(arm, (body) => seq(...apply(body.members, located(body, name)))))));
const closing = ($, rule, name, ...rest) => replaced(rule, name, (members, at) => [...members.slice(0, at), $.substitution, ...rest]);
const opening = ($, rule, name) => replaced(rule, name, (members, at) => [$.substitution, mandatory(members[at]), ...members.slice(at + 1)]);
const spanning = ($, rule, from, to) => replaced(rule, from, (members, at) => [...members.slice(0, at), $.substitution, ...members.slice(located({ members }, to) + 1)]);
const trailing = ($, rule, text) =>
    choice(
        rule,
        ...arms(rule)
            .filter((arm) => reaches(arm, token(text)))
            .map((arm) => within(arm, (body) => seq(body.members[0], $.substitution))),
    );

// --- [DIALECT]

const dialect = extend(postgres, {
    supertypes: ($, original) => [...original, ...chains.map((name) => $[name])],
    inline: ($, original) => [...original, ...inlined.map((name) => $[name])],
    rules: {
        ...Object.fromEntries(Object.keys(RULES).map((name) => [name, ($) => shaped($, name)])),
        ...split('CommentStmt', 'comment_target'),
        ...split('returning_clause', 'returning_list'),
        ...split('AlterTSConfigurationStmt', 'alter_ts_configuration_cmd'),
        [postgres.grammar.word]: widened,
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export default compose(dialect, {
    name: 'pgsql',
    hosts: [{ grammar: tsx, entry: 'template_string', hole: 'template_substitution' }],
    fragments: FRAGMENTS,
    slots: {
        [IDENT]: [],
        c_expr: [],
        qual_Op: [],
        table_ref: [],
        select_clause: [],
        PreparableStmt: [],
        comment_target: [],
        Sconst: [],
        alter_table_cmd: [],
        ColConstraintElem: [],
        insert_rest: [],
        set_clause: [],
    },
    conflicts: ($) => [
        [$.OptConsTableSpace, $.unreserved_keyword],
        [$.ColConstraintElem, $.ConstraintElem],
        [$.alter_generic_option_elem, $.unreserved_keyword],
        [$.columnref, $.func_name],
        [$.Sconst, $._ident],
        [$.columnref, $.qualified_name, $.func_name],
        [$.source_file, $.bare_label_keyword],
        [$.source_file],
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
        [$.source_file, $.target_el],
        [$.table_ref, $.c_expr, $.qual_Op, $.Sconst, $._ident],
        [$.columnDef, $.ConstraintElem],
        [$.columnDef, $._ident],
        [$.columnDef, $.ConstraintElem, $._ident],
        [$.TableConstraint, $._ident],
        [$.ConstraintAttr, $.ConstraintAttributeElem],
        [$.GenericType, $.func_name],
    ],
    rules: {
        ConstraintElem: ($, original) => choice(original, seq($.substitution, optional($.ConstraintAttributeSpec))),
        TableConstraint: ($, original) => closing($, original, 'name', optional($.ConstraintAttributeSpec)),
        AlterTableStmt: ($, original) => closing($, original, 'relation_expr'),
        AlterTSConfigurationStmt: ($, original) => closing($, original, 'any_name'),
        CreateStmt: ($, original) => opening($, original, 'OnCommitOption'),
        columnDef: ($, original) => spanning($, original, 'ColId', 'Typename'),
        a_expr_prec: ($, original) => trailing($, original, '::'),
    },
});

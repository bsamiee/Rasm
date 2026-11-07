import toml from '@tree-sitter-grammars/tree-sitter-toml/grammar.js';
import { after, any, chain, drop, extend, located, replace, rewrite, string } from '#/tools/ast-grep/grammars/derive.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SECONDS = ':([0-5][0-9]|60)([.][0-9]+)?';
const ESCAPES = '[btnfr"\\\\]';
const NEWLINE = located(toml.grammar.rules.array, ({ type }) => type === 'PATTERN');
const SPACE = toml.grammar.extras.find(({ type }) => type === 'PATTERN');

// --- [OPERATIONS] ----------------------------------------------------------------------

const immediate = ({ type }) => type === 'IMMEDIATE_TOKEN';
const respelled = (from, to) => (_, original) => rewrite(original, (part) => (part.type === 'PATTERN' && part.value.includes(from) ? { ...part, value: part.value.replace(from, to) } : part));
const contented = replace(
    (rule) => immediate(rule) && rule.content.type === 'REPEAT1',
    ($, part) => alias(part, $.string_content),
);
const bodied = (name) =>
    replace(
        ({ type }) => type === 'REPEAT',
        ($) => optional(alias($[`${name}_body`], $.string_content)),
    );
const body = (name) => () => repeat1(located(drop(immediate)(undefined, toml.grammar.rules[name]), ({ type }) => type === 'REPEAT').content);
const timed = respelled(SECONDS, `(${SECONDS})?`);

// --- [EXPORTS] -------------------------------------------------------------------------

export default extend(toml, {
    rules: {
        _basic_string: contented,
        _literal_string: contented,
        _multiline_basic_string: bodied('_multiline_basic_string'),
        _multiline_basic_string_body: body('_multiline_basic_string'),
        _multiline_literal_string: bodied('_multiline_literal_string'),
        _multiline_literal_string_body: body('_multiline_literal_string'),
        escape_sequence: respelled(ESCAPES, `[btnfre"\\\\]|x[0-9a-fA-F]{2}`),
        _escape_line_ending: after(({ type, value }) => type === 'PATTERN' && value === '\\\\', repeat(SPACE)),
        local_time: timed,
        local_date_time: timed,
        offset_date_time: timed,
        inline_table: chain(
            after(({ type }) => type === 'REPEAT', optional(',')),
            after(
                any(string('{'), string(','), ({ type }) => type === 'ALIAS'),
                repeat(NEWLINE),
            ),
        ),
    },
});

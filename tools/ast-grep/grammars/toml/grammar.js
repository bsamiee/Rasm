import toml from '@tree-sitter-grammars/tree-sitter-toml/grammar.js';
import { after, any, chain, drop, extend, replace, rewrite, string, symbol } from '../derive.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const NEWLINE = /\r?\n/u;
const SECONDS = ':([0-5][0-9]|60)([.][0-9]+)?';
const ESCAPE = /\\(?:[btnfre"\\]|x[0-9a-fA-F]{2}|u[0-9a-fA-F]{4}|U[0-9a-fA-F]{8})/u;
const SPACE = toml.grammar.extras.find(({ type }) => type === 'PATTERN');

// --- [OPERATIONS] ----------------------------------------------------------------------

const immediate = ({ type }) => type === 'IMMEDIATE_TOKEN';
const contented = replace(any((rule) => immediate(rule) && rule.content.type === 'REPEAT1', symbol('_multiline_basic_string_content'), symbol('_multiline_literal_string_content')), ($, part) => alias(part, $.string_content));
const scanned = chain(drop(immediate), contented);
const timed = (_, original) => rewrite(original, (part) => (part.type === 'PATTERN' && part.value.includes(SECONDS) ? { ...part, value: part.value.replace(SECONDS, `(${SECONDS})?`) } : part));

// --- [EXPORTS] -------------------------------------------------------------------------

export default extend(toml, {
    rules: {
        _basic_string: contented,
        _literal_string: contented,
        _multiline_basic_string: scanned,
        _multiline_literal_string: scanned,
        escape_sequence: () => token.immediate(ESCAPE),
        _escape_line_ending: after(({ type, value }) => type === 'PATTERN' && value === '\\\\', repeat(SPACE)),
        local_time: timed,
        local_date_time: timed,
        offset_date_time: timed,
        inline_table: chain(after(({ type }) => type === 'REPEAT', optional(',')), after(any(string('{'), string(','), ({ type }) => type === 'ALIAS'), repeat(NEWLINE))),
    },
});

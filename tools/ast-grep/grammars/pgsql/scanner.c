#include <stdbool.h>
#include <stdint.h>

#pragma push_macro("tree_sitter_postgres_external_scanner_scan")
#undef tree_sitter_postgres_external_scanner_scan
#define tree_sitter_postgres_external_scanner_scan postgres_upstream_scan
#include "tree-sitter-postgres/postgres/src/scanner.c"
#undef tree_sitter_postgres_external_scanner_scan
#pragma pop_macro("tree_sitter_postgres_external_scanner_scan")

// --- [TYPES] ---------------------------------------------------------------------------

enum { OPERATOR = DOLLAR_QUOTED_STRING + 1 };

// --- [OPERATIONS] ----------------------------------------------------------------------

static bool spacing(int32_t character) { return character == ' ' || (character >= '\t' && character <= '\r'); }

static bool sign(int32_t character) { return character == '+' || character == '-'; }

static bool qualifying(int32_t character) {
    switch (character) {
        case '~':
        case '!':
        case '@':
        case '#':
        case '^':
        case '&':
        case '|':
        case '?':
        case '%':
            return true;
        default:
            return false;
    }
}

static bool operating(int32_t character) {
    switch (character) {
        case '+':
        case '-':
        case '*':
        case '/':
        case '<':
        case '>':
        case '=':
            return true;
        default:
            return qualifying(character);
    }
}

static bool opens_comment(int32_t character, int32_t next) { return (character == '-' && next == '-') || (character == '/' && next == '*'); }

static bool self_lexed(int32_t first, int32_t second, uint32_t length) {
    if (length == 1) {
        return first == '%' || first == '^' || (operating(first) && !qualifying(first));
    }
    return length == 2 && ((second == '=' && (first == '<' || first == '>' || first == '!')) || (first == '<' && second == '>') || (first == '=' && second == '>'));
}

static bool scan_operator(TSLexer *lexer) {
    int32_t head[2] = {0, 0};
    uint32_t length = 0;
    uint32_t marked = 0;
    bool qualified = false;
    while (operating(lexer->lookahead) && !lexer->eof(lexer)) {
        int32_t character = lexer->lookahead;
        lexer->advance(lexer, false);
        if (opens_comment(character, lexer->lookahead)) {
            break;
        }
        if (length < 2) {
            head[length] = character;
        }
        length++;
        qualified = qualified || qualifying(character);
        if (qualified || !sign(character) || length == 1) {
            lexer->mark_end(lexer);
            marked = length;
        }
    }
    lexer->result_symbol = OPERATOR;
    return marked > 0 && !self_lexed(head[0], head[1], marked);
}

// --- [COMPOSITION] ---------------------------------------------------------------------

bool tree_sitter_postgres_external_scanner_scan(void *payload, TSLexer *lexer, const bool *valid_symbols) {
    while (spacing(lexer->lookahead)) {
        lexer->advance(lexer, true);
    }
    if (lexer->lookahead == '$') {
        return postgres_upstream_scan(payload, lexer, valid_symbols);
    }
    return valid_symbols[OPERATOR] && scan_operator(lexer);
}

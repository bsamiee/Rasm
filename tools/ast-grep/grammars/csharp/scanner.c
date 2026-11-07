#include <stdbool.h>
#include <stdint.h>
#include <wctype.h>

#include "grammar.h"

#define tree_sitter_c_sharp_external_scanner_scan(...) c_sharp_upstream_scan(__VA_ARGS__)

static inline bool is_separator(int32_t character);

#define iswspace is_separator

#include "tree-sitter-c-sharp/src/scanner.c"

// --- [TYPES] ---------------------------------------------------------------------------

enum { CONDITIONAL_ACCESS_QUESTION = LAMBDA_PAREN_OPEN + 1 };

typedef struct {
    int32_t min;
    int32_t max;
} Span;

// --- [CONSTANTS] -----------------------------------------------------------------------

static const Span SEPARATORS[] = {SPACE};

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [CHARACTERS]

static inline bool is_separator(int32_t character) {
    for (size_t index = 0; index < sizeof(SEPARATORS) / sizeof(*SEPARATORS); index++) {
        if (character >= SEPARATORS[index].min && character <= SEPARATORS[index].max) {
            return true;
        }
    }
    return false;
}

static bool is_decimal_digit(int32_t character) { return character >= '0' && character <= '9'; }

static bool skip_comment(TSLexer *lexer) {
    advance(lexer);
    if (lexer->lookahead == '/') {
        while (lexer->lookahead != 0 && lexer->lookahead != '\n' && lexer->lookahead != '\r') {
            advance(lexer);
        }
        return true;
    }
    if (lexer->lookahead != '*') {
        return false;
    }
    advance(lexer);
    for (int32_t previous = 0; !(previous == '*' && lexer->lookahead == '/');) {
        if (lexer->lookahead == 0) {
            return false;
        }
        previous = lexer->lookahead;
        advance(lexer);
    }
    advance(lexer);
    return true;
}

static bool skip_trivia(TSLexer *lexer) {
    while (is_separator(lexer->lookahead)) {
        advance(lexer);
    }
    return lexer->lookahead != '/' || (skip_comment(lexer) && skip_trivia(lexer));
}

// --- [TOKENS]

static bool recovering(const bool *valid_symbols) {
    for (int symbol = OPT_SEMI; symbol < CONDITIONAL_ACCESS_QUESTION; symbol++) {
        if (!valid_symbols[symbol]) {
            return false;
        }
    }
    return true;
}

static bool scan_conditional_access_question(TSLexer *lexer) {
    advance(lexer);
    lexer->mark_end(lexer);
    lexer->result_symbol = CONDITIONAL_ACCESS_QUESTION;
    if (!skip_trivia(lexer) || lexer->lookahead != '.') {
        return false;
    }
    advance(lexer);
    return !is_decimal_digit(lexer->lookahead);
}

static bool closes_verbatim(Scanner *scanner, TSLexer *lexer, const bool *valid_symbols) {
    if (!valid_symbols[INTERPOLATION_END_QUOTE] || !valid_symbols[INTERPOLATION_STRING_CONTENT] || scanner->interpolation_stack.size == 0 || lexer->lookahead != '"') {
        return false;
    }
    Interpolation *interpolation = array_back(&scanner->interpolation_stack);
    return is_verbatim(interpolation) && !is_raw(interpolation);
}

static bool scan_verbatim_quotes(Scanner *scanner, TSLexer *lexer) {
    unsigned count = 0;
    for (; lexer->lookahead == '"'; count++) {
        advance(lexer);
        if (count % 2 == 1) {
            lexer->mark_end(lexer);
        }
    }
    if (count > 1) {
        lexer->result_symbol = INTERPOLATION_STRING_CONTENT;
        return true;
    }
    lexer->mark_end(lexer);
    lexer->result_symbol = INTERPOLATION_END_QUOTE;
    array_pop(&scanner->interpolation_stack);
    return true;
}

// --- [COMPOSITION] ---------------------------------------------------------------------

bool(tree_sitter_c_sharp_external_scanner_scan)(void *payload, TSLexer *lexer, const bool *valid_symbols) {
    bool recovery = recovering(valid_symbols);
    if (!recovery && closes_verbatim(payload, lexer, valid_symbols)) {
        return scan_verbatim_quotes(payload, lexer);
    }
    bool question = valid_symbols[CONDITIONAL_ACCESS_QUESTION] && !recovery;
    while (question && is_separator(lexer->lookahead)) {
        skip(lexer);
    }
    return question && lexer->lookahead == '?' ? scan_conditional_access_question(lexer) : c_sharp_upstream_scan(payload, lexer, valid_symbols);
}

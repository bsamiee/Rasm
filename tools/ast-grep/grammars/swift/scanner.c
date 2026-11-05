#define tree_sitter_swift_external_scanner_scan(...) swift_upstream_scan(__VA_ARGS__)

#include "tree-sitter-swift/src/scanner.c"

// --- [CONSTANTS] -----------------------------------------------------------------------

#define POSTFIX_OPERATOR TOKEN_COUNT
#define RAW_STRING_OPEN (TOKEN_COUNT + 1)
#define RAW_STRING_CLOSE (TOKEN_COUNT + 2)
#define FUNCTION_SIGNATURE_END (TOKEN_COUNT + 3)
#define TRAILING_CONSTRAINT_COMMA (TOKEN_COUNT + 4)
#define TRIPLE_QUOTED 0x80000000U
static const char CLOSURE_LABEL_EXCLUSION[] = "default";

// --- [OPERATIONS] ----------------------------------------------------------------------

static bool holds(const char *set, int32_t character) { return character > 0 && character < 0x80 && strchr(set, character) != NULL; }

static bool right_unbound(int32_t character) { return character == 0 || should_treat_as_wspace(character) || holds(")]},:.", character); }

static bool opens_generic_parameter(int32_t character) { return character >= 0x80 || character == '_' || (character >= 'a' && character <= 'z') || (character >= 'A' && character <= 'Z'); }

// --- [RAW_STRINGS]

static uint32_t counted(TSLexer *lexer, int32_t character, uint32_t limit) {
    uint32_t count = 0;
    while (lexer->lookahead == character && count < limit) {
        advance(lexer);
        count++;
    }
    return count;
}

static bool closes_on_line(TSLexer *lexer, uint32_t hashes) {
    while (!lexer->eof(lexer) && lexer->lookahead != '\n' && lexer->lookahead != '\r') {
        bool quote = lexer->lookahead == '"';
        advance(lexer);
        if (quote && counted(lexer, '#', hashes) == hashes) {
            return true;
        }
    }
    return false;
}

static bool scan_raw_open(struct ScannerState *state, TSLexer *lexer) {
    uint32_t hashes = counted(lexer, '#', ~TRIPLE_QUOTED);
    if (lexer->lookahead != '"') {
        if (hashes != 1) {
            return false;
        }
        lexer->mark_end(lexer);
        lexer->result_symbol = find_possible_compiler_directive(lexer);
        return true;
    }
    advance(lexer);
    lexer->mark_end(lexer);
    bool triple = counted(lexer, '"', 2) == 2;
    if (triple) {
        if (lexer->lookahead == '#' && counted(lexer, '#', hashes) == hashes) {
            triple = false;
        } else {
            lexer->mark_end(lexer);
            triple = !closes_on_line(lexer, hashes);
        }
    }
    state->ongoing_raw_str_hash_count = hashes | (triple ? TRIPLE_QUOTED : 0);
    lexer->result_symbol = RAW_STRING_OPEN;
    return true;
}

static bool scan_raw_body(struct ScannerState *state, TSLexer *lexer) {
    uint32_t hashes = state->ongoing_raw_str_hash_count & ~TRIPLE_QUOTED;
    uint32_t quotes = state->ongoing_raw_str_hash_count & TRIPLE_QUOTED ? 3 : 1;
    bool held = false;
    while (!lexer->eof(lexer)) {
        lexer->mark_end(lexer);
        if (lexer->lookahead == '\\') {
            advance(lexer);
            if (counted(lexer, '#', hashes) == hashes && lexer->lookahead == '(') {
                lexer->result_symbol = RAW_STR_PART;
                return held;
            }
            held = true;
        } else if (lexer->lookahead == '"') {
            if (counted(lexer, '"', quotes) == quotes && counted(lexer, '#', hashes) == hashes) {
                if (held) {
                    lexer->result_symbol = RAW_STR_END_PART;
                    return true;
                }
                lexer->mark_end(lexer);
                state->ongoing_raw_str_hash_count = 0;
                lexer->result_symbol = RAW_STRING_CLOSE;
                return true;
            }
            held = true;
        } else {
            advance(lexer);
            held = true;
        }
    }
    return false;
}

// --- [SEPARATORS]

static bool labels_trailing_closure(TSLexer *lexer) {
    uint32_t matched = 0;
    if (!opens_generic_parameter(lexer->lookahead)) {
        return false;
    }
    while (opens_generic_parameter(lexer->lookahead) || (lexer->lookahead >= '0' && lexer->lookahead <= '9')) {
        matched = matched < sizeof(CLOSURE_LABEL_EXCLUSION) - 1 && lexer->lookahead == CLOSURE_LABEL_EXCLUSION[matched] && matched != UINT32_MAX ? matched + 1 : UINT32_MAX;
        advance(lexer);
    }
    bool excluded = matched == sizeof(CLOSURE_LABEL_EXCLUSION) - 1;
    while (should_treat_as_wspace(lexer->lookahead) && lexer->lookahead != ';') {
        advance(lexer);
    }
    if (excluded || lexer->lookahead != ':') {
        return false;
    }
    advance(lexer);
    while (should_treat_as_wspace(lexer->lookahead) && lexer->lookahead != ';') {
        advance(lexer);
    }
    return lexer->lookahead == '{';
}

// --- [CONSTRAINTS]

static bool scan_trailing_constraint_comma(TSLexer *lexer) {
    advance(lexer);
    lexer->mark_end(lexer);
    lexer->result_symbol = TRAILING_CONSTRAINT_COMMA;
    while (should_treat_as_wspace(lexer->lookahead) && lexer->lookahead != ';') {
        advance(lexer);
    }
    return holds("{>", lexer->lookahead);
}

// --- [COMPOSITION] ---------------------------------------------------------------------

bool(tree_sitter_swift_external_scanner_scan)(void *payload, TSLexer *lexer, const bool *valid_symbols) {
    struct ScannerState *state = payload;
    if (valid_symbols[RAW_STRING_CLOSE] && !valid_symbols[RAW_STRING_OPEN]) {
        return scan_raw_body(state, lexer);
    }
    int32_t first = lexer->lookahead;
    if (first == ',' && valid_symbols[TRAILING_CONSTRAINT_COMMA] && !valid_symbols[RAW_STRING_CLOSE]) {
        return scan_trailing_constraint_comma(lexer);
    }
    bool left_bound =valid_symbols[POSTFIX_OPERATOR] && !should_treat_as_wspace(first);
    bool found = swift_upstream_scan(payload, lexer, valid_symbols);
    if (!found && valid_symbols[RAW_STRING_OPEN] && !valid_symbols[RAW_STRING_CLOSE] && lexer->lookahead == '#') {
        return scan_raw_open(state, lexer);
    }
    if (found && lexer->result_symbol == IMPLICIT_SEMI && ((valid_symbols[FUNCTION_SIGNATURE_END] && lexer->lookahead == '{') || labels_trailing_closure(lexer))) {
        return false;
    }
    if (found && lexer->result_symbol == EXPLICIT_SEMI) {
        lexer->mark_end(lexer);
    }
    if (found && lexer->result_symbol == CUSTOM_OPERATOR && !opens_generic_parameter(lexer->lookahead)) {
        lexer->mark_end(lexer);
    }
    if (left_bound && right_unbound(lexer->lookahead)) {
        if (found && lexer->result_symbol == CUSTOM_OPERATOR) {
            lexer->result_symbol = POSTFIX_OPERATOR;
        }
        if (!found && holds("%^~*", first)) {
            lexer->mark_end(lexer);
            lexer->result_symbol = POSTFIX_OPERATOR;
            return true;
        }
    }
    return found;
}

#define tree_sitter_swift_external_scanner_create(...) swift_upstream_create(__VA_ARGS__)
#define tree_sitter_swift_external_scanner_destroy(...) swift_upstream_destroy(__VA_ARGS__)
#define tree_sitter_swift_external_scanner_serialize(...) swift_upstream_serialize(__VA_ARGS__)
#define tree_sitter_swift_external_scanner_deserialize(...) swift_upstream_deserialize(__VA_ARGS__)
#define tree_sitter_swift_external_scanner_scan(...) swift_upstream_scan(__VA_ARGS__)
#ifndef SCANNER
#define SCANNER(fn) tree_sitter_swift_external_scanner_##fn
#endif

#include "tree-sitter-swift/src/scanner.c"

// --- [CONSTANTS] -----------------------------------------------------------------------

#define POSTFIX_OPERATOR TOKEN_COUNT

// --- [OPERATIONS] ----------------------------------------------------------------------

static bool holds(const char *set, int32_t character) { return character > 0 && character < 0x80 && strchr(set, character) != NULL; }

static bool right_unbound(int32_t character) { return character == 0 || should_treat_as_wspace(character) || holds(")]},:.", character); }

// --- [COMPOSITION] ---------------------------------------------------------------------

void *(SCANNER(create))(void) { return swift_upstream_create(); }

void(SCANNER(destroy))(void *payload) { swift_upstream_destroy(payload); }

unsigned(SCANNER(serialize))(void *payload, char *buffer) { return swift_upstream_serialize(payload, buffer); }

void(SCANNER(deserialize))(void *payload, const char *buffer, unsigned length) { swift_upstream_deserialize(payload, buffer, length); }

bool(SCANNER(scan))(void *payload, TSLexer *lexer, const bool *valid_symbols) {
    int32_t first = lexer->lookahead;
    bool left_bound = valid_symbols[POSTFIX_OPERATOR] && !should_treat_as_wspace(first);
    bool found = swift_upstream_scan(payload, lexer, valid_symbols);
    if (found && lexer->result_symbol == EXPLICIT_SEMI) {
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

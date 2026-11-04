#define tree_sitter_rust_external_scanner_create(...) rust_upstream_create(__VA_ARGS__)
#define tree_sitter_rust_external_scanner_destroy(...) rust_upstream_destroy(__VA_ARGS__)
#define tree_sitter_rust_external_scanner_serialize(...) rust_upstream_serialize(__VA_ARGS__)
#define tree_sitter_rust_external_scanner_deserialize(...) rust_upstream_deserialize(__VA_ARGS__)
#define tree_sitter_rust_external_scanner_scan(...) rust_upstream_scan(__VA_ARGS__)
#ifndef SCANNER
#define SCANNER(fn) tree_sitter_rust_external_scanner_##fn
#endif

#include "tree-sitter-rust/src/scanner.c"

// --- [CONSTANTS] -----------------------------------------------------------------------

enum { FIELD_LETTER = 'a' };

// --- [MODELS] --------------------------------------------------------------------------

typedef struct {
    TSLexer lexer;
    TSLexer *inner;
} FieldLexer;

// --- [OPERATIONS] ----------------------------------------------------------------------

static void field_advance(TSLexer *lexer, bool skipped) {
    FieldLexer *self = (FieldLexer *)lexer;
    bool dot = self->lexer.lookahead == '.';
    self->inner->advance(self->inner, skipped);
    self->lexer.lookahead = dot && self->inner->lookahead == '_' ? FIELD_LETTER : self->inner->lookahead;
}

static void field_mark_end(TSLexer *lexer) {
    FieldLexer *self = (FieldLexer *)lexer;
    self->inner->mark_end(self->inner);
}

static uint32_t field_get_column(TSLexer *lexer) {
    FieldLexer *self = (FieldLexer *)lexer;
    return self->inner->get_column(self->inner);
}

static bool field_is_at_included_range_start(const TSLexer *lexer) {
    const FieldLexer *self = (const FieldLexer *)lexer;
    return self->inner->is_at_included_range_start(self->inner);
}

static bool field_eof(const TSLexer *lexer) {
    const FieldLexer *self = (const FieldLexer *)lexer;
    return self->inner->eof(self->inner);
}

// --- [COMPOSITION] ---------------------------------------------------------------------

void *(SCANNER(create))(void) { return rust_upstream_create(); }

void(SCANNER(destroy))(void *payload) { rust_upstream_destroy(payload); }

unsigned(SCANNER(serialize))(void *payload, char *buffer) { return rust_upstream_serialize(payload, buffer); }

void(SCANNER(deserialize))(void *payload, const char *buffer, unsigned length) { rust_upstream_deserialize(payload, buffer, length); }

bool(SCANNER(scan))(void *payload, TSLexer *lexer, const bool *valid_symbols) {
    if (!valid_symbols[FLOAT_LITERAL]) {
        return rust_upstream_scan(payload, lexer, valid_symbols);
    }
    FieldLexer field = {
        .lexer = {
            .lookahead = lexer->lookahead,
            .result_symbol = lexer->result_symbol,
            .advance = field_advance,
            .mark_end = field_mark_end,
            .get_column = field_get_column,
            .is_at_included_range_start = field_is_at_included_range_start,
            .eof = field_eof,
        },
        .inner = lexer,
    };
    bool found = rust_upstream_scan(payload, &field.lexer, valid_symbols);
    lexer->result_symbol = field.lexer.result_symbol;
    return found;
}

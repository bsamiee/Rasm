#pragma push_macro("tree_sitter_plpgsql_external_scanner_create")
#pragma push_macro("tree_sitter_plpgsql_external_scanner_destroy")
#pragma push_macro("tree_sitter_plpgsql_external_scanner_serialize")
#pragma push_macro("tree_sitter_plpgsql_external_scanner_deserialize")
#pragma push_macro("tree_sitter_plpgsql_external_scanner_scan")
#define tree_sitter_plpgsql_external_scanner_create plpgsql_upstream_create
#define tree_sitter_plpgsql_external_scanner_destroy plpgsql_upstream_destroy
#define tree_sitter_plpgsql_external_scanner_serialize plpgsql_upstream_serialize
#define tree_sitter_plpgsql_external_scanner_deserialize plpgsql_upstream_deserialize
#define tree_sitter_plpgsql_external_scanner_scan plpgsql_upstream_scan

#include "tree-sitter-postgres/plpgsql/src/scanner.c"

#pragma pop_macro("tree_sitter_plpgsql_external_scanner_create")
#pragma pop_macro("tree_sitter_plpgsql_external_scanner_destroy")
#pragma pop_macro("tree_sitter_plpgsql_external_scanner_serialize")
#pragma pop_macro("tree_sitter_plpgsql_external_scanner_deserialize")
#pragma pop_macro("tree_sitter_plpgsql_external_scanner_scan")

#ifndef SCANNER
#define SCANNER(fn) tree_sitter_plpgsql_external_scanner_##fn
#endif

#include "tree_sitter/alloc.h"

// --- [TYPES] ---------------------------------------------------------------------------

enum { TEMPLATE_OPEN = SQL_UNTIL_SEMICOLON_GUARDED + 1 };

// --- [CONSTANTS] -----------------------------------------------------------------------

enum { DELIMITER = '`', SUBSTITUTION = '$', BRACE_OPEN = '{', BRACE_CLOSE = '}' };

// --- [MODELS] --------------------------------------------------------------------------

typedef struct {
    bool templated;
} Document;

typedef struct {
    TSLexer lexer;
    TSLexer *inner;
    uint32_t depth;
    bool substitution;
} TemplateLexer;

// --- [OPERATIONS] ----------------------------------------------------------------------

static bool template_closes(const TemplateLexer *self) { return self->depth == 0 && self->inner->lookahead == DELIMITER; }

static void template_advance(TSLexer *lexer, bool skipped) {
    TemplateLexer *self = (TemplateLexer *)lexer;
    int32_t passed = self->inner->lookahead;
    self->depth += (passed == BRACE_OPEN && (self->substitution || self->depth > 0)) - (passed == BRACE_CLOSE && self->depth > 0);
    self->substitution = passed == SUBSTITUTION;
    self->inner->advance(self->inner, skipped);
    self->lexer.lookahead = template_closes(self) ? 0 : self->inner->lookahead;
}

static void template_mark_end(TSLexer *lexer) {
    TemplateLexer *self = (TemplateLexer *)lexer;
    self->inner->mark_end(self->inner);
}

static uint32_t template_get_column(TSLexer *lexer) {
    TemplateLexer *self = (TemplateLexer *)lexer;
    return self->inner->get_column(self->inner);
}

static bool template_is_at_included_range_start(const TSLexer *lexer) {
    const TemplateLexer *self = (const TemplateLexer *)lexer;
    return self->inner->is_at_included_range_start(self->inner);
}

static bool template_eof(const TSLexer *lexer) {
    const TemplateLexer *self = (const TemplateLexer *)lexer;
    return template_closes(self) || self->inner->eof(self->inner);
}

// --- [COMPOSITION] ---------------------------------------------------------------------

void *(SCANNER(create))(void) { return ts_calloc(1, sizeof(Document)); }

void(SCANNER(destroy))(void *payload) { ts_free(payload); }

unsigned(SCANNER(serialize))(void *payload, char *buffer) {
    buffer[0] = (char)((Document *)payload)->templated;
    return 1;
}

void(SCANNER(deserialize))(void *payload, const char *buffer, unsigned length) { ((Document *)payload)->templated = length > 0 && buffer[0] != 0; }

bool(SCANNER(scan))(void *payload, TSLexer *lexer, const bool *valid_symbols) {
    Document *document = (Document *)payload;
    if (valid_symbols[TEMPLATE_OPEN] && !valid_symbols[SQL_STATEMENT] && lexer->lookahead == DELIMITER) {
        lexer->advance(lexer, false);
        lexer->result_symbol = TEMPLATE_OPEN;
        document->templated = true;
        return true;
    }
    if (!document->templated) {
        return plpgsql_upstream_scan(NULL, lexer, valid_symbols);
    }
    TemplateLexer template = {
        .lexer = {
            .lookahead = lexer->lookahead == DELIMITER ? 0 : lexer->lookahead,
            .result_symbol = lexer->result_symbol,
            .advance = template_advance,
            .mark_end = template_mark_end,
            .get_column = template_get_column,
            .is_at_included_range_start = template_is_at_included_range_start,
            .eof = template_eof,
        },
        .inner = lexer,
    };
    bool found = plpgsql_upstream_scan(NULL, &template.lexer, valid_symbols);
    lexer->result_symbol = template.lexer.result_symbol;
    return found;
}

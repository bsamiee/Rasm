#include <algorithm>
#include <ctype.h>
#include <stdio.h>
#include <wctype.h>
#include "tree_sitter/parser.h"
#include "scanner-offset.h"

// --- [COMPOSITION] ---------------------------------------------------------------------

namespace sqlite {
#include <tree-sitter-sqlite3/src/scanner.c>
}

namespace tsx {
#include <tree-sitter-typescript/tsx/src/scanner.c>
}

extern "C" {
void *tree_sitter_sqlite3_external_scanner_create() { return nullptr; }
void tree_sitter_sqlite3_external_scanner_destroy(void *) {}
unsigned tree_sitter_sqlite3_external_scanner_serialize(void *, char *) { return 0; }
void tree_sitter_sqlite3_external_scanner_deserialize(void *, const char *, unsigned) {}

bool tree_sitter_sqlite3_external_scanner_scan(void *payload, TSLexer *lexer, const bool *valid_symbols) {
    const auto *host = valid_symbols + SQL_EXTERNAL_COUNT;
    if (std::find(valid_symbols, host, true) != host)
        return sqlite::tree_sitter_sqlite3_external_scanner_scan(payload, lexer, valid_symbols);
    const auto found = tsx::tree_sitter_tsx_external_scanner_scan(payload, lexer, host);
    if (found)
        lexer->result_symbol += SQL_EXTERNAL_COUNT;
    return found;
}
}

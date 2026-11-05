#ifndef SCANNER
#define SCANNER(fn) tree_sitter_bash_external_scanner_##fn
#endif
#define tree_sitter_bash_external_scanner_create SCANNER(create)
#define tree_sitter_bash_external_scanner_destroy SCANNER(destroy)
#define tree_sitter_bash_external_scanner_serialize SCANNER(serialize)
#include <string.h>
#pragma push_macro("tree_sitter_bash_external_scanner_scan")
#pragma push_macro("tree_sitter_bash_external_scanner_deserialize")
#define tree_sitter_bash_external_scanner_scan bash_upstream_scan
#define tree_sitter_bash_external_scanner_deserialize bash_upstream_deserialize
#define strcmp(left, right) (strcmp(left, right) != 0 || !(lexer->eof(lexer) || lexer->lookahead == '\n'))
#include "tree-sitter-bash/src/scanner.c"
#undef strcmp
#pragma pop_macro("tree_sitter_bash_external_scanner_deserialize")
#pragma pop_macro("tree_sitter_bash_external_scanner_scan")

// --- [TYPES] ---------------------------------------------------------------------------

enum { TIME_POSIX = ERROR_RECOVERY + 1, TIME_END, CLOSED, EXTGLOB_WORD, BRACE_OPEN, BACKTICK_END, HEREDOC_DUE };

// --- [CONSTANTS] -----------------------------------------------------------------------

static const char METACHARACTERS[] = " \t\n;&|()<>";
static const char *const CLOSERS[] = {"}", "do", "done", "elif", "else", "fi", "then"};

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [CHARACTERS]

static inline bool ascii(int32_t character) {
    return character > 0 && character < 0x80;
}

static inline bool among(const char *set, int32_t character) {
    return ascii(character) && strchr(set, character) != NULL;
}

static inline bool digit(int32_t character) {
    return ascii(character) && isdigit(character);
}

static inline bool named(int32_t character, bool leading) {
    return character == '_' || (ascii(character) && (isalpha(character) || (!leading && isdigit(character))));
}

static inline bool ends(TSLexer *lexer) {
    return lexer->eof(lexer) || iswspace(lexer->lookahead);
}

static inline bool bounds(TSLexer *lexer) {
    return lexer->eof(lexer) || among(METACHARACTERS, lexer->lookahead);
}

static inline bool blank(TSLexer *lexer) {
    return lexer->lookahead == ' ' || lexer->lookahead == '\t';
}

static inline bool plain(const bool *valid_symbols) {
    return !valid_symbols[EXPANSION_WORD] && !valid_symbols[REGEX] && !valid_symbols[REGEX_NO_SLASH] && !valid_symbols[REGEX_NO_SPACE];
}

// --- [HEREDOCS]

static uint32_t waiting(const Scanner *scanner) {
    uint32_t index = scanner->heredocs.size;
    while (index > 0 && !scanner->heredocs.contents[index - 1].started) {
        index--;
    }
    while (index < scanner->heredocs.size && scanner->heredocs.contents[index].delimiter.size == 0) {
        index++;
    }
    return index;
}

static bool delimiter(Heredoc *heredoc, TSLexer *lexer) {
    lexer->result_symbol = HEREDOC_START;
    while (blank(lexer)) {
        skip(lexer);
    }
    int32_t quote = 0;
    bool found = false;
    while (!lexer->eof(lexer) && (quote != 0 || (lexer->lookahead != '`' && !among(METACHARACTERS, lexer->lookahead)))) {
        const int32_t character = lexer->lookahead;
        if (character >= 0x80) {
            return false;
        }
        advance(lexer);
        if (character == quote) {
            quote = 0;
        } else if (quote == 0 && (character == '\'' || character == '"')) {
            quote = character;
            heredoc->is_raw = true;
        } else if (character == '\\' && ascii(lexer->lookahead) && (quote == 0 || (quote == '"' && among("$`\"\\\n", lexer->lookahead)))) {
            heredoc->is_raw = true;
            array_push(&heredoc->delimiter, (char)lexer->lookahead);
            advance(lexer);
            found = true;
        } else {
            array_push(&heredoc->delimiter, (char)character);
            found = true;
        }
    }
    array_push(&heredoc->delimiter, '\0');
    if (!found) {
        reset_string(&heredoc->delimiter);
    }
    return found;
}

static bool due(Scanner *scanner, TSLexer *lexer, uint32_t index) {
    Heredoc *contents = scanner->heredocs.contents;
    const uint32_t last = scanner->heredocs.size - 1;
    const Heredoc due = contents[index];
    memmove(&contents[index], &contents[index + 1], (last - index) * sizeof(Heredoc));
    contents[last] = due;
    lexer->result_symbol = HEREDOC_DUE;
    lexer->mark_end(lexer);
    return true;
}

static void truncated(Scanner *scanner, uint32_t size) {
    while (scanner->heredocs.size > size) {
        Heredoc heredoc = array_pop(&scanner->heredocs);
        array_delete(&heredoc.delimiter);
        array_delete(&heredoc.current_leading_word);
    }
}

// --- [TOKENS]

static bool literal(TSLexer *lexer) {
    lexer->result_symbol = BARE_DOLLAR;
    advance(lexer);
    lexer->mark_end(lexer);
    return ends(lexer) || among("\")|&;<>", lexer->lookahead);
}

static bool flag(TSLexer *lexer, const bool *valid_symbols) {
    advance(lexer);
    if (lexer->lookahead == 'p' && valid_symbols[TIME_POSIX]) {
        lexer->result_symbol = TIME_POSIX;
    } else if (lexer->lookahead == '-' && valid_symbols[TIME_END]) {
        lexer->result_symbol = TIME_END;
    } else {
        return false;
    }
    advance(lexer);
    lexer->mark_end(lexer);
    return bounds(lexer);
}

static bool closed(TSLexer *lexer) {
    lexer->result_symbol = CLOSED;
    lexer->mark_end(lexer);
    char word[sizeof "then"] = {0};
    for (size_t length = 0; length < sizeof word - 1 && ascii(lexer->lookahead) && !bounds(lexer); length++) {
        word[length] = (char)lexer->lookahead;
        advance(lexer);
    }
    if (!bounds(lexer)) {
        return false;
    }
    for (size_t index = 0; index < sizeof CLOSERS / sizeof *CLOSERS; index++) {
        if (strcmp(word, CLOSERS[index]) == 0) {
            return true;
        }
    }
    return false;
}

static bool concat(TSLexer *lexer) {
    lexer->result_symbol = CONCAT;
    lexer->mark_end(lexer);
    if (lexer->lookahead == ')') {
        return true;
    }
    advance(lexer);
    return !ends(lexer);
}

static bool range(TSLexer *lexer) {
    lexer->result_symbol = BRACE_START;
    while (digit(lexer->lookahead)) {
        advance(lexer);
    }
    for (int dot = 0; dot < 2; dot++) {
        if (lexer->lookahead != '.') {
            return false;
        }
        advance(lexer);
    }
    while (digit(lexer->lookahead)) {
        advance(lexer);
    }
    return lexer->lookahead == '}';
}

static bool descriptor(TSLexer *lexer, const bool *valid_symbols) {
    advance(lexer);
    lexer->mark_end(lexer);
    const bool word = valid_symbols[BRACE_OPEN] && !bounds(lexer);
    lexer->result_symbol = BRACE_OPEN;
    if (!named(lexer->lookahead, true)) {
        return word || (valid_symbols[BRACE_START] && range(lexer));
    }
    while (named(lexer->lookahead, false)) {
        advance(lexer);
    }
    if (lexer->lookahead != '}') {
        return word;
    }
    advance(lexer);
    if (lexer->lookahead != '<' && lexer->lookahead != '>') {
        return word;
    }
    lexer->result_symbol = FILE_DESCRIPTOR;
    lexer->mark_end(lexer);
    return true;
}

static bool group(TSLexer *lexer) {
    uint32_t depth = 0;
    do {
        const int32_t character = lexer->lookahead;
        if (lexer->eof(lexer) || character == '\n' || among("$`<>", character)) {
            return false;
        }
        advance(lexer);
        if (character == '\\') {
            advance(lexer);
        } else if (character == '\'' || character == '"') {
            while (!lexer->eof(lexer) && lexer->lookahead != character) {
                if (character == '"' && among("$`", lexer->lookahead)) {
                    return false;
                }
                advance(lexer);
            }
            advance(lexer);
        } else if (character == '(') {
            depth++;
        } else if (character == ')') {
            depth--;
        }
    } while (depth > 0);
    return true;
}

static bool extglob(TSLexer *lexer) {
    lexer->result_symbol = EXTGLOB_WORD;
    bool found = false;
    while (!bounds(lexer)) {
        const int32_t character = lexer->lookahead;
        if (among("$`'\"{}[]", character) || !ascii(character)) {
            break;
        }
        advance(lexer);
        if (character == '\\') {
            advance(lexer);
        } else if (among("?*+@!", character) && lexer->lookahead == '(') {
            if (!group(lexer)) {
                return false;
            }
            found = true;
        }
    }
    lexer->mark_end(lexer);
    return found;
}

// --- [COMPOSITION] ---------------------------------------------------------------------

void SCANNER(deserialize)(void *payload, const char *buffer, unsigned length) {
    Scanner *scanner = payload;
    bash_upstream_deserialize(payload, buffer, length);
    if (length > 0) {
        truncated(scanner, (unsigned char)buffer[3]);
    }
}

bool SCANNER(scan)(void *payload, TSLexer *lexer, const bool *valid_symbols) {
    Scanner *scanner = payload;
    if (!in_error_recovery(valid_symbols) && valid_symbols[HEREDOC_END] && lexer->eof(lexer) && scanner->heredocs.size > 0) {
        truncated(scanner, scanner->heredocs.size - 1);
        lexer->result_symbol = HEREDOC_END;
        lexer->mark_end(lexer);
        return true;
    }
    if (in_error_recovery(valid_symbols)) {
        return bash_upstream_scan(payload, lexer, valid_symbols);
    }
    if (valid_symbols[HEREDOC_BODY_BEGINNING] || valid_symbols[SIMPLE_HEREDOC_BODY] || valid_symbols[HEREDOC_CONTENT] || valid_symbols[HEREDOC_END]) {
        const bool scanned = bash_upstream_scan(payload, lexer, valid_symbols);
        if (scanned && lexer->eof(lexer) && scanner->heredocs.size > 0 && array_back(&scanner->heredocs)->delimiter.size == 0) {
            lexer->mark_end(lexer);
        }
        return scanned;
    }
    if (valid_symbols[HEREDOC_START] && scanner->heredocs.size > 0) {
        return delimiter(array_back(&scanner->heredocs), lexer);
    }
    const uint32_t queue = waiting(scanner);
    const bool queued = queue < scanner->heredocs.size;
    const bool word = valid_symbols[EXTGLOB_WORD] && plain(valid_symbols) && !valid_symbols[VARIABLE_NAME] && !valid_symbols[EXTGLOB_PATTERN];
    const bool closes = valid_symbols[CLOSED] && !valid_symbols[CONCAT];
    const bool head = valid_symbols[FILE_DESCRIPTOR] && !valid_symbols[EXPANSION_WORD] && !valid_symbols[CLOSING_BRACE];
    const bool timed = valid_symbols[TIME_POSIX] || valid_symbols[TIME_END];
    const bool dollar = valid_symbols[BARE_DOLLAR] && plain(valid_symbols);
    bool separated[HEREDOC_DUE + 1];
    memcpy(separated, valid_symbols, sizeof separated);
    if (blank(lexer) && !valid_symbols[EMPTY_VALUE] && (queued || head || closes || timed || ((word || dollar) && !valid_symbols[CLOSING_BRACE]))) {
        separated[CONCAT] = false;
        while (blank(lexer)) {
            skip(lexer);
        }
    }
    if (queued && lexer->lookahead == '\n') {
        return due(scanner, lexer, queue);
    }
    if (queued && valid_symbols[BACKTICK_END] && lexer->lookahead == '`') {
        truncated(scanner, queue);
        lexer->result_symbol = BACKTICK_END;
        lexer->mark_end(lexer);
        return true;
    }
    if (timed && lexer->lookahead == '-') {
        return flag(lexer, valid_symbols);
    }
    if (closes && among("}deft", lexer->lookahead)) {
        return closed(lexer);
    }
    if (head && lexer->lookahead == '{' && !separated[CONCAT]) {
        return descriptor(lexer, valid_symbols);
    }
    if (head && lexer->lookahead == '0' && !separated[CONCAT]) {
        advance(lexer);
        lexer->result_symbol = FILE_DESCRIPTOR;
        lexer->mark_end(lexer);
        return lexer->lookahead == '<' || lexer->lookahead == '>';
    }
    if (!separated[CONCAT] && lexer->lookahead == '$' && dollar) {
        return literal(lexer);
    }
    if (!separated[CONCAT] && word && (named(lexer->lookahead, true) || among("./~?*+@!%^,:=", lexer->lookahead))) {
        return extglob(lexer);
    }
    if (valid_symbols[EXPANSION_WORD] && lexer->lookahead == '(') {
        advance(lexer);
        if (lexer->lookahead == '$') {
            lexer->result_symbol = EXPANSION_WORD;
            lexer->mark_end(lexer);
            return true;
        }
    }
    return separated[CONCAT] && (lexer->lookahead == '\\' || (lexer->lookahead == ')' && valid_symbols[CLOSING_BRACE])) ? concat(lexer) : bash_upstream_scan(payload, lexer, separated);
}

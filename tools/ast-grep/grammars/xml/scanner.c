#include <stdbool.h>
#include <stdint.h>
#include <wctype.h>

#include "word.h"

#define tree_sitter_xml_external_scanner_scan(...) xml_upstream_scan(__VA_ARGS__)

static inline bool name_start(uint32_t code);
static inline bool name_character(uint32_t code);

#define iswalpha name_start
#define iswalnum name_character

#include "@tree-sitter-grammars/tree-sitter-xml/xml/src/scanner.c"

// --- [TYPES] ---------------------------------------------------------------------------

typedef struct {
    uint32_t min;
    uint32_t max;
} Span;

// --- [CONSTANTS] -----------------------------------------------------------------------

static const Span NAME_START[] = {WORD_START};
static const Span NAME_CHARACTER[] = {WORD_CHARACTER};

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [NAMES]

static inline bool spanned(const Span *spans, uint32_t count, uint32_t code) {
    for (uint32_t index = 0; index < count; index++) {
        if (code >= spans[index].min && code <= spans[index].max) {
            return true;
        }
    }
    return false;
}

static inline bool name_start(uint32_t code) {
    return spanned(NAME_START, sizeof(NAME_START) / sizeof(*NAME_START), code);
}

static inline bool name_character(uint32_t code) {
    return spanned(NAME_CHARACTER, sizeof(NAME_CHARACTER) / sizeof(*NAME_CHARACTER), code);
}

// --- [INSTRUCTIONS]

static bool scan_target(TSLexer *lexer, const bool *valid_symbols) {
    char read[sizeof("xml-stylesheet") + 1] = {0};
    uint32_t length = 0;
    if (!is_valid_name_start_char(lexer->lookahead)) {
        return false;
    }
    while (is_valid_name_char(lexer->lookahead)) {
        if (length < sizeof(read) - 1) {
            read[length] = lexer->lookahead < 0x80 ? (char)lexer->lookahead : '?';
        }
        length++;
        advance(lexer);
    }
    lexer->mark_end(lexer);
    lexer->result_symbol = PI_TARGET;
    bool reserved = length == 3 && (read[0] | 0x20) == 'x' && (read[1] | 0x20) == 'm' && (read[2] | 0x20) == 'l';
    return !reserved && !(valid_symbols[XML_MODEL] && strcmp(read, "xml-model") == 0) && !(valid_symbols[XML_STYLESHEET] && strcmp(read, "xml-stylesheet") == 0);
}

static bool scan_instruction(TSLexer *lexer) {
    bool held = false;
    while (!lexer->eof(lexer)) {
        lexer->mark_end(lexer);
        if (lexer->lookahead == '?') {
            advance(lexer);
            if (lexer->lookahead == '>') {
                lexer->result_symbol = PI_CONTENT;
                return held;
            }
        } else {
            advance(lexer);
        }
        held = true;
    }
    return false;
}

// --- [TEXT]

static bool scan_text(TSLexer *lexer) {
    uint32_t brackets = 0;
    while (in_char_data(lexer)) {
        if (brackets >= 2 && lexer->lookahead == '>') {
            return false;
        }
        brackets = lexer->lookahead == ']' ? brackets + 1 : 0;
        advance(lexer);
    }
    lexer->mark_end(lexer);
    lexer->result_symbol = CHAR_DATA;
    return true;
}

static bool scan_section(TSLexer *lexer) {
    lexer->result_symbol = CDATA;
    for (bool held = false; !lexer->eof(lexer); held = true) {
        if (held) {
            lexer->mark_end(lexer);
        }
        bool bracket = lexer->lookahead == ']';
        advance(lexer);
        if (!held) {
            lexer->mark_end(lexer);
        }
        if (bracket && lexer->lookahead == ']') {
            advance(lexer);
            if (lexer->lookahead == '>') {
                return held;
            }
            if (lexer->lookahead == ']') {
                return true;
            }
        }
    }
    return false;
}

// --- [TAGS]

static String scan_name(TSLexer *lexer) {
    String name = array_new();
    if (!is_valid_name_start_char(lexer->lookahead)) {
        return name;
    }
    while (is_valid_name_char(lexer->lookahead)) {
        uint32_t code = (uint32_t)lexer->lookahead;
        if (code < 0x80) {
            array_push(&name, (char)code);
        } else {
            uint32_t tail = code < 0x800 ? 1 : code < 0x10000 ? 2 : 3;
            array_push(&name, (char)((0xF00u >> (tail + 1)) | (code >> (6 * tail))));
            while (tail-- > 0) {
                array_push(&name, (char)(0x80u | ((code >> (6 * tail)) & 0x3Fu)));
            }
        }
        advance(lexer);
    }
    return name;
}

static bool scan_start_name(Vector *tags, TSLexer *lexer) {
    String name = scan_name(lexer);
    if (name.size == 0) {
        array_delete(&name);
        return false;
    }
    array_push(tags, name);
    lexer->result_symbol = START_TAG_NAME;
    return true;
}

static bool scan_end_name(Vector *tags, TSLexer *lexer) {
    String name = scan_name(lexer);
    bool closes = name.size > 0 && tags->size > 0;
    if (closes) {
        String *open = array_back(tags);
        closes = open->size == 0 || (open->size == UINT8_MAX ? name.size >= UINT8_MAX && memcmp(open->contents, name.contents, UINT8_MAX) == 0 : string_eq(open, &name));
    }
    array_delete(&name);
    if (closes) {
        String last = array_pop(tags);
        array_delete(&last);
        lexer->result_symbol = END_TAG_NAME;
    }
    return closes;
}

// --- [COMPOSITION] ---------------------------------------------------------------------

bool(tree_sitter_xml_external_scanner_scan)(void *payload, TSLexer *lexer, const bool *valid_symbols) {
    Vector *tags = (Vector *)payload;
    if (in_error_recovery(valid_symbols)) {
        return false;
    }
    if (valid_symbols[PI_TARGET]) {
        return scan_target(lexer, valid_symbols);
    }
    if (valid_symbols[PI_CONTENT]) {
        return scan_instruction(lexer);
    }
    if (valid_symbols[CDATA]) {
        return scan_section(lexer);
    }
    if (valid_symbols[CHAR_DATA] && in_char_data(lexer)) {
        return scan_text(lexer);
    }
    if (valid_symbols[START_TAG_NAME]) {
        return scan_start_name(tags, lexer);
    }
    if (valid_symbols[END_TAG_NAME]) {
        return scan_end_name(tags, lexer);
    }
    return xml_upstream_scan(payload, lexer, valid_symbols);
}

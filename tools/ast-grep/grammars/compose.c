#include <stdbool.h>
#include <stddef.h>
#include <stdint.h>

#include "tree_sitter/alloc.h"
#include "tree_sitter/parser.h"

#include "components.h"

// --- [CONSTANTS] -----------------------------------------------------------------------

#define EXTERNAL_SCANNER(composite, fn) tree_sitter_##composite##_external_scanner_##fn
#define EXPANDED_SCANNER(composite, fn) EXTERNAL_SCANNER(composite, fn)
#define ABI(fn) EXPANDED_SCANNER(COMPOSITE, fn)

// --- [MODELS] --------------------------------------------------------------------------

typedef struct {
    void *(*create)(void);
    void (*destroy)(void *);
    unsigned (*serialize)(void *, char *);
    void (*deserialize)(void *, const char *, unsigned);
    bool (*scan)(void *, TSLexer *, const bool *);
    uint16_t offset;
    uint16_t count;
} Component;

typedef struct {
    const Component *stateful;
    void *payload;
} Composite;

// --- [SERVICES] ------------------------------------------------------------------------

#define COMPONENT(name, offset, count)                                                     \
    void *name##_component_create(void);                                                   \
    void name##_component_destroy(void *);                                                 \
    unsigned name##_component_serialize(void *, char *);                                   \
    void name##_component_deserialize(void *, const char *, unsigned);                     \
    bool name##_component_scan(void *, TSLexer *, const bool *);
COMPONENTS
#undef COMPONENT

#define COMPONENT(name, offset, count)                                                     \
    {name##_component_create, name##_component_destroy, name##_component_serialize,        \
     name##_component_deserialize, name##_component_scan, offset, count},
static const Component components[] = {COMPONENTS};
#undef COMPONENT

static const size_t component_count = sizeof components / sizeof *components;

// --- [OPERATIONS] ----------------------------------------------------------------------

static bool holds(const Component *component, const bool *valid, bool extra) {
    for (uint16_t index = component->offset; index < component->offset + component->count; index++) {
        if (valid[index] && EXTRA[index] == extra) {
            return true;
        }
    }
    return false;
}

static const Component *owner(const bool *valid) {
    const Component *found = NULL;
    for (size_t index = 0; index < component_count; index++) {
        if (holds(&components[index], valid, false)) {
            if (found != NULL) {
                return &components[0];
            }
            found = &components[index];
        }
    }
    for (size_t index = 0; found == NULL && index < component_count; index++) {
        if (holds(&components[index], valid, true)) {
            found = &components[index];
        }
    }
    return found == NULL ? &components[0] : found;
}

static void *payload_of(const Composite *composite, const Component *component) {
    return composite->stateful == component ? composite->payload : NULL;
}

// --- [COMPOSITION] ---------------------------------------------------------------------

void *ABI(create)(void) {
    Composite *composite = ts_calloc(1, sizeof *composite);
    for (size_t index = 0; index < component_count; index++) {
        void *payload = components[index].create();
        if (payload != NULL) {
            composite->stateful = &components[index];
            composite->payload = payload;
        }
    }
    return composite;
}

void ABI(destroy)(void *payload) {
    Composite *composite = payload;
    for (size_t index = 0; index < component_count; index++) {
        components[index].destroy(payload_of(composite, &components[index]));
    }
    ts_free(composite);
}

unsigned ABI(serialize)(void *payload, char *buffer) {
    Composite *composite = payload;
    return composite->stateful == NULL ? 0 : composite->stateful->serialize(composite->payload, buffer);
}

void ABI(deserialize)(void *payload, const char *buffer, unsigned length) {
    Composite *composite = payload;
    if (composite->stateful != NULL) {
        composite->stateful->deserialize(composite->payload, buffer, length);
    }
}

bool ABI(scan)(void *payload, TSLexer *lexer, const bool *valid_symbols) {
    Composite *composite = payload;
    const Component *component = owner(valid_symbols);
    bool scanned = component->scan(payload_of(composite, component), lexer, valid_symbols + component->offset);
    lexer->result_symbol += scanned ? component->offset : 0;
    return scanned;
}

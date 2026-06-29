# pyphen

## Owned Surface

| Capability | API | Artifact Owner |
|---|---|---|
| Hyphenation dictionary | `pyphen.Pyphen` | `.planning/typography/layout.md` |
| Hyphenation positions | `Pyphen.positions` | `.planning/typography/layout.md` |
| Hyphenation alternatives | `Pyphen.iterate` | `.planning/typography/layout.md` |
| Inserted hyphen view | `Pyphen.inserted` | `.planning/typography/layout.md` |
| Width-constrained wrap | `Pyphen.wrap` | `.planning/typography/layout.md` |
| Language fallback and dictionary index | `pyphen.language_fallback`, `pyphen.LANGUAGES` | `.planning/typography/layout.md` |

## Integration Rules

- `typography/layout` owns `pyphen`; document and composition receive local line boxes and penalties, never a provider dictionary.
- Hyphenation points become local penalty rows in the paragraph layout algorithm.
- Language fallback is resolved once at the layout boundary and stored as local layout evidence.

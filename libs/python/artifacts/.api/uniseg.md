# uniseg

## Owned Surface

| Capability | API | Artifact Owner |
|---|---|---|
| Unicode line-break units | `uniseg.linebreak.line_break_units` | `.planning/typography/layout.md` |
| Unicode line-break boundaries | `uniseg.linebreak.line_break_boundaries` | `.planning/typography/layout.md` |
| Unicode line-break opportunities | `uniseg.linebreak.line_break_breakables` | `.planning/typography/layout.md` |
| Unicode grapheme clusters | `uniseg.graphemecluster.grapheme_clusters`, `grapheme_cluster_boundaries` | `.planning/typography/layout.md` |
| Unicode word boundaries | `uniseg.wordbreak.word_boundaries` | `.planning/typography/layout.md` |

## Integration Rules

- `typography/layout` owns Unicode segmentation; shaping consumes positioned glyph runs and never recomputes text breaks.
- Segmentation functions normalize into local `Item`, `BreakClass`, and `LineBrokenRun` values before Knuth-Plass layout runs.
- Provider enum values such as `LineBreak` and `WordBreak` remain boundary data and do not enter document or composition owners.

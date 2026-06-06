Question: Which non-language code-fence intent labels are viable local conventions without breaking CommonMark, GFM, GitHub highlighting, or common Markdown tooling?
Type: standards-research
Lane: track-external-research
Merge key: docs/standards/information-structure.md :: code fence info-string intent labels :: clarify local grammar
Target owner: `docs/standards/information-structure.md`
Source basis: active standards and synthesis reads; CommonMark/GFM/GitHub Docs/Linguist/markdown-it/mdast/highlight.js/Prism sources accessed 2026-06-06
Promotion target: `docs/standards/information-structure.md`, with notation ripple to `docs/standards/formatting.md`
Outcome: PROMOTE

## [FINDINGS]

Finding 1
    Active owner/section: `docs/standards/information-structure.md` `[11][CODE_BLOCKS]`.
    Evidence source URLs: https://github.github.com/gfm/#fenced-code-blocks; https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/creating-and-highlighting-code-blocks; https://raw.githubusercontent.com/markdown-it/markdown-it/master/lib/renderer.mjs; https://raw.githubusercontent.com/syntax-tree/mdast/main/readme.md.
    Finding: Multi-word info strings are legal Markdown source, but the first word is the portable language/highlighter token. CommonMark and GFM leave renderer treatment open while describing first-word language convention. GitHub Docs says the optional language identifier enables highlighting and GitHub uses Linguist. markdown-it passes the first word as `langName` and keeps the rest as attributes. mdast splits the first token into `lang` and the rest into `meta`.
    Weakness/inconsistency: Rasm's current `language intent` rule is locally useful, but trailing intent words are not GitHub renderer semantics and should not be described as highlighting behavior.
    Recommendation: CHANGE. State the grammar as `<renderer-language> <rasm-intent>` for ordinary fences. The first token must be a real language, renderer tag, or neutral text token; the second token is Rasm metadata for agents and future validators. Renderer-local tags such as `mermaid` remain exact and carry intent in nearby prose.
    Active owner: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/formatting.md`; `docs/standards/proof.md` if renderer-proof packets mention fence labels; type standards with fence examples.
    Decision: PROMOTE.
    User-choice point: none for first-token language rule; it is already supported by sources and current local wording.

Finding 2
    Active owner/section: `docs/standards/information-structure.md` `[11][CODE_BLOCKS]`.
    Evidence source URLs: https://raw.githubusercontent.com/github-linguist/linguist/main/lib/linguist/languages.yml; https://highlightjs.readthedocs.io/en/latest/supported-languages.html; https://prismjs.com/.
    Finding: GitHub Linguist exposes aliases for fenced code blocks. Current Linguist data recognizes `json`, `markdown`/`md`, `text` through aliases such as `plain text`, and `yaml`/`yml`. highlight.js recognizes `json`, `markdown`, `yaml`, and `plaintext`/`text`. Prism documents `language-xxxx` or `lang-xxxx` classes with language aliases including `markdown`/`md`.
    Weakness/inconsistency: Intent labels such as `template`, `conceptual`, `rejected`, `generated`, `copy-safe`, `test-only`, `deprecated`, and `output-only` are not language identifiers. If placed first, they can disable intended highlighting or route the block to an unrelated highlighter if a renderer later adopts that name.
    Recommendation: KEEP with guard. Use intent labels only after the language token: `yaml template`, `markdown rejected`, `text output-only`, `diff output-only`, `bash copy-safe`. Do not write `template yaml`, `rejected markdown`, or `output-only text`.
    Active owner: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/formatting.md`; existing standards examples with fenced blocks.
    Decision: PROMOTE.
    User-choice point: none unless Rasm wants a validator to reject currently unlisted language-intent pairs.

Finding 3
    Active owner/section: `docs/standards/information-structure.md` `[11][CODE_BLOCKS]`; `docs/standards/formatting.md` `[5][LIST_WHITESPACE_DISCIPLINE]` for compact contrast labels.
    Evidence source URLs: https://github.github.com/gfm/#fenced-code-blocks; https://raw.githubusercontent.com/syntax-tree/mdast/main/readme.md.
    Finding: The existing intent vocabulary is coherent when each label answers one reader-safety question: run/paste, fill in, study only, fixture only, generated output, sample output, retained but not adopted, or counter-example. These are source-level labels, not syntax languages.
    Weakness/inconsistency: The current allowed-pairs table lists only nine pairs while nearby prose names broader reusable intent labels including `test-only`, `generated`, and `deprecated`. A future edit can either over-widen pairs ad hoc or block legitimate language-specific examples.
    Recommendation: CHANGE. Keep the intent vocabulary closed, but split the rule into two layers: allowed intent labels and allowed language-intent pairs already used by the corpus. New pairs are allowed only when the language token is real and the intent comes from the closed set.
    Active owner: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/formatting.md`; `docs/standards/reference/code-documentation.md`; type standards with generated or test-only examples.
    Decision: PROMOTE.
    User-choice point: choose whether future validators enforce only declared pairs or allow any real language plus closed intent.

Finding 4
    Active owner/section: `docs/standards/information-structure.md` `[11][CODE_BLOCKS]`.
    Evidence source URLs: https://raw.githubusercontent.com/github-linguist/linguist/main/lib/linguist/languages.yml; https://highlightjs.readthedocs.io/en/latest/supported-languages.html.
    Finding: `yaml template` is viable. `yaml` is a real language token for GitHub/Linguist and highlight.js, and `template` is already a Rasm intent meaning copy the structure and replace placeholders.
    Weakness/inconsistency: None in current concept; validation would only need to check that placeholders are actually present or the surrounding prose says the block is structural.
    Recommendation: ADD. Permit `yaml template` when the block is a fillable YAML structure, not a current config that can be pasted unchanged. Use `yaml copy-safe` only when the block is byte-equivalent to a named source-of-truth YAML file.
    Active owner: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/formatting.md`; generated or config documentation that shows YAML.
    Decision: PROMOTE.
    User-choice point: none.

Finding 5
    Active owner/section: `docs/standards/information-structure.md` `[11][CODE_BLOCKS]`.
    Evidence source URLs: https://raw.githubusercontent.com/github-linguist/linguist/main/lib/linguist/languages.yml; https://highlightjs.readthedocs.io/en/latest/supported-languages.html.
    Finding: `json schema` is syntactically viable Markdown because `json` stays first, but `schema` is not one of the current Rasm intent labels and GitHub Linguist does not expose `json schema` as a separate language in the checked languages file. The source class is still JSON.
    Weakness/inconsistency: `schema` names a content subtype, not reader safety. Promoting it as an intent would blur Rasm's one-intent rule and compete with `template`, `generated`, `copy-safe`, and `output-only`.
    Recommendation: HOLD. Prefer `json template` for a fillable schema-shaped example, `json generated` for generated schema output, `json copy-safe` for a byte-equivalent source file, or visible prose such as "This JSON Schema template..." before the fence. Do not add `schema` to the intent vocabulary unless Rasm chooses a third-token subtype grammar.
    Active owner: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/reference/api.md`; `docs/standards/reference/reference.md`; generated contract docs.
    Decision: HOLD.
    User-choice point: decide whether Rasm wants two-token grammar only (`language intent`) or a three-token grammar (`language intent subtype`) for cases like schema, transcript, fixture, or patch.

Finding 6
    Active owner/section: `docs/standards/information-structure.md` `[11][CODE_BLOCKS]`.
    Evidence source URLs: https://raw.githubusercontent.com/github-linguist/linguist/main/lib/linguist/languages.yml; https://highlightjs.readthedocs.io/en/latest/supported-languages.html.
    Finding: `text transcript` is syntactically viable if `text` stays first, but `transcript` is not a current Rasm intent. It also conflicts with existing report policy that rejects session transcripts and with the code-block rule that separates runnable commands from output.
    Weakness/inconsistency: A transcript can contain prompts, command lines, terminal output, and local paths, so one fence label cannot tell the reader what is safe to copy or what proof the block represents.
    Recommendation: HOLD. Use `text output-only` for bounded expected signals, `text conceptual` for illustrative terminal-shaped text, or separate `bash copy-safe` command fences from `text output-only` output fences. If a literal transcript must be retained inside `_reports/**`, describe it in visible prose rather than adding `transcript` as a standards-wide intent.
    Active owner: `docs/standards/information-structure.md`; `_reports/AGENTS.md` for report-transcript rejection boundaries.
    Ripple files: `docs/standards/_reports/AGENTS.md`; runbook/how-to standards only if transcript examples are later allowed.
    Decision: HOLD.
    User-choice point: decide whether `_reports/**` may have a report-local `text transcript` exception; do not promote it into active standards without that boundary.

Finding 7
    Active owner/section: `docs/standards/information-structure.md` `[11][CODE_BLOCKS]`.
    Evidence source URLs: https://raw.githubusercontent.com/github-linguist/linguist/main/lib/linguist/languages.yml; https://highlightjs.readthedocs.io/en/latest/supported-languages.html; https://prismjs.com/.
    Finding: `markdown output-only` is syntactically viable because `markdown` is a recognized language token, but it is semantically narrow. `output-only` means sample output, not input to run; Markdown output is usually rendered structure, generated documentation, or expected source text.
    Weakness/inconsistency: The current corpus uses `markdown template`, `markdown conceptual`, and `markdown rejected`; generated Markdown examples may be better labeled `markdown generated` if the generator owns them, while expected rendered/source output may be better introduced in prose.
    Recommendation: HOLD. Permit only after a concrete owner needs sample Markdown output that is not a template, concept, generated artifact, or rejected form. Prefer `markdown generated` for generator-owned Markdown and `text output-only` for terminal output that happens to contain Markdown-looking text.
    Active owner: `docs/standards/information-structure.md`.
    Ripple files: generated documentation standards; `docs/standards/reference/api.md`; `docs/standards/reference/code-documentation.md`.
    Decision: HOLD.
    User-choice point: decide whether `generated` should cover generated Markdown fully, making `markdown output-only` unnecessary.

Finding 8
    Active owner/section: `docs/standards/information-structure.md` `[11][CODE_BLOCKS]`; `docs/standards/proof.md` if a validator is later claimed.
    Evidence source URLs: https://markdown-it.github.io/markdown-it/; https://raw.githubusercontent.com/markdown-it/markdown-it/master/lib/renderer.mjs; https://raw.githubusercontent.com/syntax-tree/mdast/main/readme.md.
    Finding: Common tooling preserves or exposes trailing info-string metadata, but not uniformly as renderer behavior. markdown-it exposes `langAttrs` to the highlighter callback; mdast exposes `meta`; GitHub rendering does not promise Rasm-specific trailing token behavior.
    Weakness/inconsistency: Rasm can standardize trailing intent labels for source review and future validators, but must not imply every renderer, editor, or highlighter will display or enforce them.
    Recommendation: ADD. Add proof wording when promoted: "Intent labels are local source metadata. They are useful to agents and validators; syntax highlighting depends on the first token and the renderer's language registry."
    Active owner: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/proof.md`; local Markdown validator docs if added.
    Decision: PROMOTE.
    User-choice point: none for wording; only validator scope is a later choice.

## [RECOMMENDATIONS]

[PROMOTE]:
- Clarify that ordinary fences use `<language> <intent>`, where `<language>` is the renderer/highlighter token and `<intent>` is Rasm source metadata.
- Keep `template`, `conceptual`, `rejected`, `output-only`, `generated`, `copy-safe`, `test-only`, and `deprecated` as closed intent labels, but only after a real language token.
- Add or allow `yaml template`; treat it as the model for language-plus-intent labels.
- State that trailing intent labels do not create GitHub syntax-highlighting behavior.

[HOLD]:
- Hold `json schema`, `text transcript`, and `markdown output-only` as general conventions. They are syntactically possible but need a user choice or concrete owner route because they introduce subtype or provenance labels beyond the current one-intent grammar.

[DROP]:
- Do not allow non-language intent labels as first info-string tokens.
- Do not add `schema` or `transcript` to the global intent vocabulary by analogy alone.

## [CANDIDATE_WORDING]

```text template
Ordinary code fences use `<language> <intent>`. The first token is the renderer or highlighter language identifier; the second token is a Rasm intent label for source readers and validators. Use one intent label from the closed set: `copy-safe`, `template`, `conceptual`, `test-only`, `generated`, `output-only`, `deprecated`, or `rejected`. Do not put intent labels first.
```

```text template
Use subtype words such as `schema`, `transcript`, `fixture`, or `contract` in visible prose, source filenames, or proof fields unless this standard declares a three-token grammar. Prefer `json template`, `json generated`, or `json copy-safe` over `json schema`; prefer `text output-only` over `text transcript`.
```

## [EVIDENCE]

[PRIMARY_SOURCE_SET_ACCESSED_2026_06_06]:
- CommonMark/GFM fenced code block behavior: https://github.github.com/gfm/#fenced-code-blocks.
- GitHub code-block highlighting and Linguist route: https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/creating-and-highlighting-code-blocks.
- GitHub Linguist language and alias source: https://raw.githubusercontent.com/github-linguist/linguist/main/lib/linguist/languages.yml.
- markdown-it API and renderer source: https://markdown-it.github.io/markdown-it/ and https://raw.githubusercontent.com/markdown-it/markdown-it/master/lib/renderer.mjs.
- mdast code-node `lang` and `meta` model: https://raw.githubusercontent.com/syntax-tree/mdast/main/readme.md.
- highlight.js supported language aliases: https://highlightjs.readthedocs.io/en/latest/supported-languages.html.
- Prism supported language aliases: https://prismjs.com/.

[LOCAL_SOURCE_SET_READ]:
- `CLAUDE.md`
- `AGENTS.md`
- `docs/standards/README.md`
- `docs/standards/AGENTS.md`
- `docs/standards/_reports/AGENTS.md`
- `docs/standards/_reports/standards-structure-notation-060626/README.md`
- `docs/standards/information-structure.md`
- `docs/standards/formatting.md`
- `docs/standards/reference/readme.md`
- `docs/standards/explanation/adr.md`
- `docs/standards/explanation/architecture.md`
- `docs/standards/explanation/design-doc.md`
- `docs/standards/explanation/roadmap.md`
- `docs/standards/explanation/test-strategy.md`
- `docs/standards/_reports/standards-structure-notation-060626/track-synthesis/00-collective-task-list.md`
- `docs/standards/_reports/standards-structure-notation-060626/track-external-research/01-gfm-github-markdown-capabilities.md`

## [PROOF_GAPS]

- No active standards were edited.
- No local Markdown fence validator was found or run; `git diff --check` is the only requested validation gate for this report.
- GitHub render proof was not run because this report relies on official GitHub/GFM/CommonMark/tooling sources and does not claim exact rendered output.

# [INPAGE_DEPTH_PROMPT_2]

Agent tool, model fable, subagent_type general-purpose, description "InPage depth rerun over the landed libraries". Prompt, verbatim:

```text
Run the InPage depth pass again over the TypeScript the InPage work reaches in /Users/bardiasamiee/Developer/Rasm: @rasm/typography, @rasm/search, @rasm/automation, @rasm/indesign, @rasm/uxp and @rasm/extendscript where they serve it, and the Atelier consumers (apps/creative-cloud/atelier, its InDesign plugin). Earlier passes landed and reviewed this code (3c293ca22, f9bfccb21, 2dbd02b56, 233e6a109, 2a7c018e2 and the TypeScript sweep since); act on what stands and never relitigate a recorded decision. The pass delivers the full InPage capability in the strongest form with real, measured line and surface reduction; growth stands only where a stated capability needs it.

Read first, whole, and work by them: CLAUDE.md, README.md, TODO.md [01] and [02] (the bar and the method), .claude/workflows/TYPESCRIPT.md, and Skill(typescript-coding), Skill(typescript-coding-effect), Skill(uxp-coding) and Skill(clean-prose). The distilled products are evidence of the logic they hold, never scripts to reproduce: the libraries state that capability as general logic any future app, tool or plugin composes, with no MCP or Atelier assumption, each capability once at its level.

Inputs that are facts to act on:
- .artifacts/workflows/typescript-extension/inpage/PLAN.md whole, including [16] (pushbacks) and [17]-[DESKTOP_MANUAL] (19 rows from the InPage manual audit): land every row, in its strongest form.
- docs/research/applications/indesign/distilled/typesetting/inpage-typesetting-tool/ in full, and its product folder.
- docs/research/typescript-extension: typography.md, perso-arabic.md, composition.md, grids.md, packages.md and studies/inpage-typesetting-tool.md; every value there is a relation, parameter or font-stated property, and code follows the same rule (no hardcoded sizes, steps, limits or names).
- The decisions already landed: fontkit 2.0.4 with HarfBuzz extents at the instance; kashida as a degree and a style bound in nuqta; features as tag to value; letter space only at joining-run boundaries.

Work:
- Capability: every capability or piece of logic the distilled source and the plan rows hold that the code lacks or narrows lands, as capability, not as copied procedure.
- Higher-order logic: lead 4 to 5 Opus agents (Agent tool, model opus) that ideate and argue on principle; land what survives as the one relation, codec or parameterized operation that makes existing functions uses of it, placed in an existing folder, a sub-folder or a new package by where it belongs; no folder holding one file, no coined names in files, members or messages.
- Packages and Effect 4: read each package (fontkit, harfbuzzjs, icu and the rest of the catalog the code reaches) and Effect from source at the pinned version; hand logic they state is deleted, and the capability they make cheap is added where it belongs.
- Reduction: delete poor logic and indirection and remove files, never shuffle; collapse struct, type, interface, schema, constant and string-literal spam into fewer, richer objects whose operations live with them; no `{ Model, model }` splits, wrappers, aliases or needless exports; flat control flow; algebraic, derived and parameterized forms. Every output message is agent-facing, built from its values, and clean-prose.
- Adversaries: any new or rebuilt surface gets its own adversarial Opus agents, and each adversary delegates Opus agents that mine the distilled InPage source against the result for anything still missing or narrowed; adversaries fix in place, removing or rebuilding what fails the bar.
- Push back only on guidance that is naive or grows surface without capability, one line each, and still land the capability it pointed at in the stronger form.
- Atelier consumers move to every new form in the same pass.
- Host proofs, only where a fact cannot be read from the code, the distilled source or docs/research/applications/indesign/facts: through tools/harnesses (`node tools/harnesses/main.ts indesign prove|compare <cases>`), watched with watch-apps; closing a World-Ready-composed document from a UXP script deadlocks InDesign 2027 Beta (facts file), so cases avoid it.

Measure lines, files and exports per package before and after with `loc` and `ast-grep outline`. Keep the A/B behavior oracle intact (`node proof/ab.ts` in the distilled folder). Another agent edits tools/harnesses; edit nothing there.

When done, run once: `nx run <project>:check` for each changed project, `nx run rasm:lint:biome -- <files>`, `nx run rasm:lint:ast-grep -- <files>` and `node proof/ab.ts`; fix each finding at its cause once, then run them once more. Add no tests and do not commit. Return at most 20 lines: capability added, plan rows landed, higher-order logic landed and where, code removed, lines, files and exports before and after per package, pushbacks, and the check results.
```

# Rhino-Bridge Design Corpus — Index and Cold-Start Charter

Design corpus for the ground-up bridge rebuild, COMPLETE and finalized 2026-06-11 (verified against tree HEAD caf5f75b2). Evidence base: `../rhino-bridge/` (12-doc review corpus, red-team grade A-). Everything is settled and cited; relitigation requires NEW evidence filed as a 10 §2 amendment with both-side citations. Graded docs (the review corpus and design 01-06) are IMMUTABLE — corrections never back-edit, they register in 10 §2.

## [1]-[COLD_START] — a new session reads in this order

1. `10-distill.md` — THE charter. Settled register (§1), amendments register (§2, 17 entries — the only home of corpus corrections), Phase-0 probe gate (§3), build order (§4), cutover done-criteria (§5), deferred closeout (§7). An implementation session codes from this file and follows its citations outward.
2. `07-code-blueprints.md` — the complete signature surface: 13-file layout (§1), Contract amendments (§2), shell addenda (§3), Cargo/Supervisor/SDK definitions (§4-§6), the 16-class agent diagnostic surface (§7), capture pipeline (§8), MCP seam (§9), 13 gated proofs with fallbacks (§10).
3. `08-assay-impact.md` — §0 the authoritative old-flow/fragility/circularity/rebuild-order account (why bridge-first, assay-last); §1-§6 line-verified python deltas; §7 parity checklist; §8 finalization decisions.
4. `09-design-redteam.md` — what was attacked and what held: R1-R15 sweep, 23 attacks (13 dispatched into 07/08, 8 refuted, 2 = the existing Phase-0 gates), reconciliations ratified.
5. `01`-`06` — graded inputs, consulted by topic: 01 doctrine charter (R1-R15) · 02 MCP verdict (COMPLEMENT; substrate REJECTED) · 03 feature set + 26-item cut list · 04 version tolerance M1-M4 · 05 package plan · 06 object model (Contract/Shell half; truncation closed by 07 §4-§6 per 09 S1).
6. `11-agent-first-docs.md` — authorship laws L1-L6 binding every string the rebuilt tool emits; §4 exact-checkout line-cite register; §5 MCP-matrix currency; §6 the ENSHRINED tri-tool standards (agent-first/only, monorepo-first, future-facing, doctrine-bound, hardened — each made concrete for this tool; binding home `tools/assay/BACKLOG.md`).

GATE: the session's first action is the four Phase-0 live probes (10 §3 — cargo-ALC unload, GH2 headless solve off-UI-thread, EventPipe, spawn benchmark; run via the OLD tool, 0b split-phase) plus operator sign-off on G5 effort sizing; per the 10 §4 sequencing law only 0a and 0b BLOCK Phases 1-2 (0c/0d may complete in parallel — their verdicts size the EventPipe admission and the process-model policy rows). REBUILD ORDER: bridge-first, assay-last — old tool stays the regression oracle until the five 10 §5 criteria are green; cutover and rollback are the same one Tool row (08 §1). DEFERRED ITEMS: zero open — every question across both corpora is closed, gated-with-fallback, or not-built-with-named-revival (10 §7).

## [2]-[DOC_TABLE] — status and amendment exposure

| [DOC] | [STATUS] | [CONTENT] | [AMENDED BY 10 §2] |
| ----- | :------: | --------- | ------------------ |
| 01-doctrine-charter.md | graded, immutable | repo C# doctrine in-host; R1-R15 binding 06+ | #2 (client LOC split) |
| 02-mcp-verdict.md | graded, immutable | COMPLEMENT two-lane verdict; coexistence mandatory | — (re-corroborated 11 §5, tightened) |
| 03-feature-set.md | graded, immutable | capability set, stories, 26-item cut list | — (OQ1-OQ5 all closed, 10 §7) |
| 04-version-tolerance.md | graded, immutable | no-pinning: probes, choke points, additive wire | — (hooks 1-4 → 07 §10 proofs) |
| 05-packages.md | graded, immutable | validated package plan + rejected list | #1 (graph-proof cmd), #14 (MCP SDK), #15 (OQ4), #17 (pin line-cite) |
| 06-object-model.md | graded, immutable | Contract/Shell half; wire declared once | #3-#7, #9, #10, #16; S1/S2 per 09 §1 |
| 07-code-blueprints.md | finalized | full signatures, 13 files; diagnostics; proofs | carries #3-#5, #9, #10, #13, #15, #16 |
| 08-assay-impact.md | finalized | old-flow account (§0); exact deltas; parity | carries #1, #7, #12 |
| 09-design-redteam.md | finalized | 23-attack audit; all dispositions named | #11 (own header arithmetic) |
| 10-distill.md | finalized | the charter; amendments; gates; closeout | owner of the register |
| 11-agent-first-docs.md | finalized | L1-L6; line-cite register; tri-tool standards | carries #7, #8 |

Citation key corpus-wide: bare numbers = review corpus (`../rhino-bridge/`); named keys = design wave (charter=01, mcp=02, features=03, tolerance=04, packages=05, model=06).

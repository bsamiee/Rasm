# [PROPERTIES_SEED_DEPTH_RESIDUALS]

The `properties-seed-depth` run (`wf_e1568768-c65`) rebuilt the 2 Properties pages to full seed depth and ran a project-wide reconcile, then its tail (Critique-2 / Redteam-2 / Reconcile-B) was cut short by a server rate-limit (a concurrent agent bursting sub-agents). The 2 Properties pages landed Rebuild + Critique-1 + Redteam-1 (refined). What it FIXED and what it DEFERRED:

## [01]-[FIXED]
- **Seq1 deprecation sweep — COMPLETE.** `Prelude.Seq1<A>` is `[Obsolete]` in LanguageExt.Core 5.0.0-beta-77 (use `Seq(x)`). The project-wide reconcile replaced every `Seq1(...)` call-site across the C# planning corpus; `grep -rE 'Seq1\(' libs/csharp` is now 0. (39 libs/csharp design pages touched: the 2 Properties rebuilds + the Seq1 sweep + Classification fixes across Materials/Element/AppUi/Compute/Persistence/Geometry/Bim/Fabrication.) Committed with the Properties leg.
- **Classification mislabel** in `properties.md` (was labeling Classification as a `MaterialPropertySet` case; Classification egress is the generic `Classification` value-object riding the `MaterialBinding` to the `Object` node) — fixed.

## [02]-[OPEN] (deferred by the rate-limit; address in `seam-reconcile` or a quick follow-up)
- **ROSTER GAP — aluminium `6082-T6`.** EN AW-6082-T6 is the most common European structural extrusion alloy (EN 1999-1-1 names it first), yet both rosters carry only 6061-T6 / 6063-T5 / 5083. Add in LOCKSTEP to both pages (preserve the dual-key 91=91 parity → 92=92):
  - `properties.md` engineering row (EN 1999-1-1 Table 3.2b, 5<t<=25mm): `rho 2710`, `E 70000 MPa`, `f0 260`, `fu 310`, `nu 0.33`, `alpha 23.1e-6`, `lambda ~170`, `c ~897`, fire `B-s1,d0`.
  - `sustainability.md` EPD row: shares the EU-Al-Profile extrusion eco-profile (A1A3 ~5.73, like 6061-T6, per the page's "GWP tracks mass not alloy" doctrine), `Uniclass Pr_20_85_08_02`.
  - Update both pages' `[FULL_ROSTER]`/`[CATALOGUE_DOMAIN_COVERAGE]` prose listing aluminium as "6061-T6/6063-T5/5083". Secondary candidate: `7020` (EN 1999 names it, "less frequently").
- **PROSE-VS-DATA DRIFT in `properties.md`.** The rosters are now exact 91=91 parity (every engineering id resolves a lifecycle row), which retired the old "required superset" asymmetry — but `properties.md` prose still describes the asymmetry: line ~315 claims `insulation.phenolic` is omitted from the lifecycle roster (FALSE — `sustainability.md` carries Kingspan-Kooltherm-Phenolic), and line ~384 claims the rebar generic aliases / ASTM-CSA grades are EPD-roster-optional (FALSE — `sustainability.md` carries gr40/60/75/80/60w/80w/400w/500w/metal.steel/metal.iron). Fix the prose to state EXACT parity; `sustainability.md` `[FULL_ROSTER_DEPTH]` is the correct side.

## [03]-[LOW_VALUE] (flagged, not forced without grounded numbers)
- Hardwood ladder: EN 338:2016 is D18..D80 (13 grades); both rosters carry 8 (omit D45/D55/D65/D75/D80). D75/D80 are exotic dense tropical hardwoods; the page's stated "D18..D70" range is internally honest.

## [04]-[VERIFY]
- Confirm no other Materials/Element page repeats the Classification-as-property-case error, and that the `MeasureValue`/`PropertyValue`/`PropertyName` "composes-the-seam" phantom (listing them as composed members where a folder only passes raw doubles through seam smart-constructors) is absent in sibling catalogue pages.

## [05]-[INCOMPLETE_ADVERSARIAL_TAIL]
- Critique-2 / Redteam-2 / Reconcile-B did not run (rate-limited). The 2 Properties pages have one adversarial round (Critique-1 + Redteam-1), not two. A second hardening round is available if depth warrants; otherwise the pages are at the seed-depth bar.

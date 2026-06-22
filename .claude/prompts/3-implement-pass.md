# [IMPLEMENT_PASS]

`<TARGET>` =

Realize every open `IDEAS` and `TASKLOG` card under `<TARGET>` into deep design-page code FENCES. `<TARGET>` is a folder (`libs/python/geometry`), a package (`libs/csharp/Rasm.Bim`), a language root (`libs/python`), `libs` for all three, or several such paths.

List the folders and files under `<TARGET>` so its `.planning/`, `.api/`, root index docs, and central manifests resolve, then `Workflow({ name: 'implement', args: '<TARGET>' })`. Pre-grant `tools/assay`, `rhino-mcp`, and shell permissions first; the nested scout's serial probe (Map phase) and package-prep call out to them.

The workflow auto-pulls every ripple-reached folder into scope to closure and realizes ONLY the ripple-counterpart cards there, so a narrow `<TARGET>` may still touch sibling and cross-language folders. On return, read `pulled_in`, `closed`, `admitted`, `weak`, `realize_failed`, `blocked_unresolved`, `ripple_out_of_scope`, and `hard_residual`. `pulled_in` names the out-of-scope folders the closure realized (ripple cards only). `weak` and `realize_failed` are cards left open (under-realized, or whose folder realize agent died) — re-run `<TARGET>` to finish them. A non-empty `ripple_out_of_scope` is the genuinely-unrealizable remainder — ripple targets with no design-page surface (a language-level/central scope or an unrecognized package), informational only, not a re-run trigger. A non-empty `hard_residual` is for the human-in-the-loop `resolve-residuals` workflow: paste those items into its `DEFAULT_RESIDUALS` (or pass `args: { residuals }`) and run it.

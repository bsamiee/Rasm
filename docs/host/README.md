# [H1][HOST]
>**Dictum:** *Host SDK boundaries own native geometry and data-access semantics; composition-root packages live separately.*

<br>

Scope: in-process host environments (CAD, visual programming, desktop shells). Universal MathNet/LanguageExt API truth remains under `docs/external-libs/`. Cross-stack owner order: `docs/usage.md` §1 and §5.

Runtime fork: in-process plugin paths use `Eff.runtime<RT>()` and explicit factories — no Scrutor. Generic host bootstrap uses `IServiceCollection` packages documented in [`host-libraries.md`](../host-libraries.md) §1.

| [INDEX] | [FILE]                                    | [HOST]                                                       |
| :-----: | ----------------------------------------- | ------------------------------------------------------------ |
|   [1]   | [rhino.md](rhino.md)                      | RhinoCommon geometry ownership vs MathNet kernels            |
|   [2]   | [gh2.md](gh2.md)                          | GH2 data access, trees, diagnostics vs MathNet numerics      |
|   [3]   | [host-libraries.md](../host-libraries.md) | Composition-root and first-consumer package candidate policy |

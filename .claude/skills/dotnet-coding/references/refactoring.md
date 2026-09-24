# [REFACTORING]

Covers C# corrections no ast-grep rule enforces, from the host boundary to naming.

Use `dotnet-coding-mapperly` for member copies between host and domain types.

## [01]-[HOST_BOUNDARY]

Host answers map by each member's documented contract, read from decompiled source or a live probe:

| [INDEX] | [REJECTED]                                          | [REQUIRED]                                         | [REASON]                      |
| :-----: | :-------------------------------------------------- | :------------------------------------------------- | :---------------------------- |
|  [01]   | Host answer discarded, emptied, folded, or misread  | Case per documented answer, no-op as success value | Caller acts on wrong outcome  |
|  [02]   | Domain pre-check copying a host rule                | Host answer or exception mapped to typed case      | Copy drifts from host rule    |
|  [03]   | Guard on value host or constructor guarantees       | Guard deleted, own producer typed `IterableNE<A>`  | Dead branch fabricates error  |
|  [04]   | Type mirroring host type, presets, or argument list | Host type itself, or function over host object     | Mirror lags host members      |
|  [05]   | Host unset value kept inside domain value           | `Option` from documented unset value at call       | Sentinel reads as real value  |
|  [06]   | Bulk host call with one answer for partial success  | Per-element call traversed, error per element      | Success hides partial failure |
|  [07]   | Host callback throwing or reading null              | Adapter runs `IO<A>` body, `Optional` arguments    | Throw or null reaches host    |
|  [08]   | Displayed text as `string` where host localizes     | Host's localized text type, named-token templates  | Text skips localization       |
|  [09]   | Host output passed on without its validity flag     | Governing flag tested, typed failure on false      | Invalid output reads as value |
|  [10]   | One fallback for absent and for failed callback     | Absent answers base, failed answers host's failure | Host answers each differently |
|  [11]   | `_` arm raising one error over host-closed types    | Named arm per reachable case, unreachable throws   | Error hides which case failed |

Pre-checks stay where the member stores invalid input or throws one exception for distinct causes.

## [02]-[TYPES]

Types hold only values producers create and consumers read:

| [INDEX] | [REJECTED]                                           | [REQUIRED]                                     | [REASON]                        |
| :-----: | :--------------------------------------------------- | :--------------------------------------------- | :------------------------------ |
|  [01]   | Member, case, or argument nothing reads              | Deleted with every reference                   | Unread shape widens every match |
|  [02]   | Joint or derivable fact stored as separate fields    | Fact once, `Option<(A, B)>` for joint facts    | Copies disagree                 |
|  [03]   | `int` code or joined `string` as field               | Enum, tuple, or `Seq` converted in one member  | Primitive hides cases and parts |
|  [04]   | Siblings or constant families differing in one value | One shape, value as field or parameter         | Variants repeat shared fields   |
|  [05]   | Enum, type tag, or type name mirroring union cases   | Union case itself, `Switch` or abstract member | Mirror drifts from cases        |
|  [06]   | Hand-written downcast recovering an erased type      | Owner generic in element, `.As()` on `K<F, A>` | Cast fails at run time          |
|  [07]   | Local seeded with placeholder, overwritten later     | Value per branch, callback answer as `Option`  | Placeholder is reachable fake   |

## [03]-[ERRORS]

Error records hold the failed call and its facts:

| [INDEX] | [REJECTED]                                    | [REQUIRED]                                 | [REASON]                    |
| :-----: | :-------------------------------------------- | :----------------------------------------- | :-------------------------- |
|  [01]   | Shared helper raising one fixed member name   | Helper taking member its caller invoked    | Error names wrong call      |
|  [02]   | Error field holding another fact or host code | Case per documented failure with its facts | Consumer cannot match cause |

## [04]-[EFFECTS]

Evaluation time and body form follow the type:

| [INDEX] | [REJECTED]                                         | [REQUIRED]                                       | [REASON]                         |
| :-----: | :------------------------------------------------- | :----------------------------------------------- | :------------------------------- |
|  [01]   | `Func<A>` run at once, state read at `IO` build    | Plain `A`, or read inside lift thunk             | Value taken at wrong time        |
|  [02]   | `Unit` arithmetic or discard ternary as statements | Block body of host statements                    | Statement spelled as value       |
|  [03]   | Block calling host member, then `return unit;`     | Expression body returning `IO.lift` of call      | Effect escapes return type       |
|  [04]   | Write target derived from name that can collide    | Duplicate targets refused before any write       | Later write erases earlier       |
|  [05]   | Queued marshal to UI thread around each host call  | `Post()` on UI-bound `IO`, UI context in `EnvIO` | Value and error never return     |
|  [06]   | `Finally` or `Bracket` over a received `IO` value  | Release after a `Bind` that enters the body      | Received `IO.fail` skips release |

## [05]-[OWNERSHIP]

Each concept and value has one owner, and callers reach it directly:

| [INDEX] | [REJECTED]                                         | [REQUIRED]                                       | [REASON]                      |
| :-----: | :------------------------------------------------- | :----------------------------------------------- | :---------------------------- |
|  [01]   | Repeated literal or rule, library gap fixed in app | One parameterized owner in lowest shared package | Copies diverge in meaning     |
|  [02]   | Owner signature callers work around                | Owner signature fixed, every workaround deleted  | Workaround repeats per caller |
|  [03]   | Named member lifting one host call, nothing added  | Caller lifting host member where it composes     | Layer adds no fact            |
|  [04]   | Reverse, rescale, clamp, or filter of own output   | Inputs derived so construction yields result     | Correction hides wrong build  |
|  [05]   | Literal or heuristic copied from ported source     | Derived from construction, or one stated rule    | Accident becomes rule         |
|  [06]   | Positional pairing, index lookup, zip then unzip   | Rows carrying values, arrays paired once proven  | Positions drift apart         |
|  [07]   | Argument restating value callee or host derives    | Dropped, `None` picks overload or skips setter   | Value stated twice            |

## [06]-[NAMING]

Names follow the member they mirror or the value they return:

| [INDEX] | [REJECTED]                                       | [REQUIRED]                        | [REASON]                     |
| :-----: | :----------------------------------------------- | :-------------------------------- | :--------------------------- |
|  [01]   | Declaration renaming host member it mirrors      | Host member's exact name          | Mirror hides its host member |
|  [02]   | Name outliving or misstating what its body runs  | Name of what body runs or returns | Callers act on stale name    |
|  [03]   | One role spelled with distinct suffixes or words | One suffix per role               | Readers miss siblings        |

# Parse-Issue Tree Algebra

[NODE_TAXONOMY]:
- The eight-arm union `Type | Missing | Unexpected | Forbidden | Pointer | Refinement | Transformation | Composite` is not eight peer faults but three structural roles in one tree: `Composite` is the only branch (its `issues: SingleOrNonEmpty<ParseIssue>` holds the children), `Pointer` is the only path edge (`{ path: Path; actual; issue }`, exactly one child plus the `PropertyKey` step), `Refinement`/`Transformation` are single-child kind-tagged wrappers (`{ ast; actual; kind; issue }`), and `Type`/`Missing`/`Unexpected`/`Forbidden` are the leaves that terminate every walk — so a consumer folding the tree dispatches on three shapes (branch / edge / wrapper / leaf), never eight, and the `_tag` is the fold's discriminant.
- Only `Pointer` carries `path`; every other node is path-free, so a flat per-field issue list is reconstructed by accumulating the `Pointer.path` steps along the root-to-leaf spine — the `path: ReadonlyArray<PropertyKey>` a flat formatter reports is the concatenation of every `Pointer` between root and leaf, never a field on the leaf itself, which is why a leaf relocated under a different key needs no change: its address is the edges above it.
- `Path = SingleOrNonEmpty<PropertyKey>` admits a non-empty array in one `Pointer`, so a transform that re-keys across several levels at once collapses its address into a single edge rather than a `Pointer` chain — the edge is the path segment, of arbitrary length, not one key per node.
- The leaf set is closed and each leaf's `actual` is the value that failed: `Type.actual` is the mistyped value, `Unexpected.actual` is the surplus value, `Missing.actual` is fixed `undefined` (the constructor takes no `actual` — absence has no value), and `Missing.ast: AST.Type` is the property signature whose key never arrived — so the missing-key diagnostic recovers the expected field shape from the leaf while every present-but-wrong diagnostic recovers the offending value.

[FAN_WIDTH_POLICY]:
- One flag, `errors: "all"` versus the default `"first"`, switches the tree between two topologies at every struct node, and the switch is the same `if (allErrors)` arm repeated at every accumulation point: on a child Left it either pushes `[stepKey, new Pointer(key, input, issue)]` onto a local `es` array and `continue`s, or it returns `Either.left(new Composite(ast, input, e, output))` immediately — `"all"` fans the node wide into one `Composite` whose `issues` is the full `es` list, `"first"` short-circuits at the first failing field into a `Composite` carrying that single `Pointer`.
- The fan-width is per-owner policy when baked as `parseOptions: { errors: "all" }` on the class header, not a per-call override threaded through every decode site — the annotation rides `Self.ast`, `goMemo` merges it into the parser via `mergeInternalOptions(options, parseOptionsAnnotation.value)`, so an owner that must surface every field fault declares it once and every `Schema.decodeUnknown(Self)`, every nested decode of `Self` as a member, and the `new`/`make` validator inherit the wide fan with no argument at the seam.
- `Composite` always carries `output` — the partially-decoded record of the fields that DID pass — so even a failing wide decode under `"all"` retains every well-formed field beside the issue list; the partial result is recoverable from the issue node, the basis of a decode that reports faults yet salvages the valid remainder.

```typescript
import { Schema } from 'effect';

// the fan-width is baked policy: every decode of this owner fans wide, none threads `errors` at the seam
class Manifest extends Schema.Class<Manifest>('Manifest')(
    {
        head: Schema.NonEmptyTrimmedString,
        size: Schema.Number.pipe(Schema.positive()),
        kind: Schema.Literal('a', 'b'),
    },
    { parseOptions: { errors: 'all', onExcessProperty: 'error' } },
) {}

// one decode surfaces head + size + kind faults as one wide Composite — never first-fault-only
const admit = Schema.decodeUnknown(Manifest);

// reject: a sibling `validateAllFields(raw)` that re-decodes field-by-field and concatenates errors
// re-derives, by hand, the `es`-accumulation the `errors: "all"` policy already performs in one pass.
```

[CONVERGENCE_NODE]:
- Three fault origins that look unrelated converge on the identical `Composite`/`Pointer` topology, which is why one `ParseError` handler walks all of them uniformly: a nested decode failure builds `Composite(structAst, input, [Pointer(key, …)])` at the struct node; a cross-field `Schema.filter` invariant returning a path-targeted issue builds `Composite(refinementAst, input, [issue, Pointer(['upper'], …)])`; and a refinement returning a structured `ParseIssue` (not a `FilterIssue`) injects an arbitrary subtree at the refinement node — all three terminate at the same leaf algebra, so the consumer never branches on fault origin.
- The cross-field invariant and the field-level decode failure are the same node KIND, not merely similar: the array-returning `Schema.filter` predicate's `{ path: ['upper'], message }` entries are converted into `Pointer`-addressed children under the refinement's `Composite`, so a width-violation on `upper` lands at the exact `upper` address a missing-or-mistyped `upper` would — the form binding to `['upper']` renders both without distinguishing the cross-field gate from the field codec.
- `Refinement.kind: "From" | "Predicate"` splits the two ways a refinement fails: `"From"` means the underlying schema rejected before the predicate ran (the issue is the inner decode fault), `"Predicate"` means the value decoded but the predicate said no — so a refined struct's failure tree distinguishes "the fields were malformed" from "the fields were valid but the cross-field invariant failed", the kind being the discriminant a handler uses to route a structural error apart from a business-rule error.
- Under `errors: "all"`, a stable-filter refinement whose `From` side fails as a `Composite` emits BOTH children at once: the parser re-runs `ast.filter(i, options, ast)` and, on a `Some`, builds `Composite(ast, i, [new Refinement(ast, i, "From", ef), new Refinement(ast, i, "Predicate", ep)])` — so a malformed-field fault and the cross-field predicate fault that the malformation also trips are reported in one tree, the refinement not masking the predicate behind the From failure.

[FORBIDDEN_SYNC_BOUNDARY]:
- `Forbidden` is the one leaf that is not a shape mismatch but an execution-mode mismatch: it is emitted by `handleForbidden`, the wrapper every node's parser passes through, when a parser that produced an `Effect` (not an `Either`) is run on a synchronous path — the function forks the effect on a `SyncScheduler`, flushes, polls the fiber, and if `unsafePoll()` returns nothing (the effect performed async work) returns `Either.left(new Forbidden(ast, actual, "cannot be be resolved synchronously…"))`.
- The same `handleForbidden` arm catches a synchronous DEFECT distinctly from a `ParseIssue`: when the polled exit is a failure whose cause `Cause.isFailType` holds, it returns the underlying `cause.error` (a real `ParseIssue` propagates unchanged), but a die — a missing dependency, a thrown non-issue — becomes `new Forbidden(ast, actual, Cause.pretty(cause))`, so an effectful field schema whose requirement was never provided surfaces as a `Forbidden` leaf carrying the rendered cause, not a silent crash.
- `options.isEffectAllowed === true` is the single gate that disarms the boundary: under it `handleForbidden` returns the original effect untouched, which is the mode the async runner sets — so the SAME owner whose field pulls a `Context.Tag` decodes cleanly through `Schema.decodeUnknown` (effect-allowed) yet produces a `Forbidden` leaf through `decodeUnknownSync`/`decodeUnknownEither`, the sync entrypoints being the seam where an async or context-dependent owner is forbidden, the diagnostic naming exactly that.
- An owner with a `filterEffect` invariant or a `Context.Tag`-bearing field is therefore the owner that must NOT be admitted through a sync decode boundary; the `Forbidden` leaf is the compile-time-invisible, runtime-loud signal that a context-dependent admission was routed to a context-free seam, the requirement surfacing on the issue tree rather than the type when the entrypoint erased `R`.

[TITLE_NODE_POLICY]:
- `parseIssueTitle: (issue: ParseIssue) => string | undefined` is read off the AST of the offending node by `getParseIssueTitle(issue) = getParseIssueTitleAnnotation(issue) ?? String(issue.ast)`, so the owner reshapes how its node names itself in the tree: the annotation is the title when it returns a string, the AST's stringification the fallback when it returns `undefined` — the function receiving the issue (not the instance) is what lets one title annotation branch on `issue._tag` to title a `Composite` differently from a `Type`.
- The title rides every node whose `ast` bears the annotation, and `getAnnotated(issue)` is `"ast" in issue ? Some(issue.ast) : None` — so `Unexpected` (the lone leaf without an `ast` field) can NEVER carry a `parseIssueTitle`, its title fixed to the formatter's `"is unexpected"`; the title annotation reshapes only the AST-bearing nodes, a boundary that decides where a per-node rename is even possible.
- Because the title is keyed on the node's own AST, the same owner reshapes the title of its REFINEMENT node independently of its TYPE node: a cross-field invariant returns its own AST under the refinement, so `parseIssueTitle: (issue) => issue._tag === 'Composite' ? '<owner-invalid>' : undefined` titles the whole-record invariant failure while leaving each field's `Type` leaf to its default — the policy discriminates by node kind, one annotation governing the family of nodes the owner generates.

[MESSAGE_UPWALK_POLICY]:
- `message: (issue) => string | Effect<string> | { message; override: boolean }` is resolved by `getMessage`, which is the tree's most subtle policy: it does not simply read the message at the failing node, it WALKS toward the inner issue governed by an `override` flag and a `useInnerMessage` predicate — `useInnerMessage = !override && (isComposite(issue) || (isRefinement && kind === "From") || (isTransformation && kind !== "Transformation"))` — so an owner's message on a wrapper or composite node defers to the child's message unless it explicitly claims precedence.
- The `override` flag is the precedence lever between a parent owner's message and a nested field's message: a field schema deep in the tree with its own `message` annotation wins by default at a wrapping `Composite`/`From`-Refinement/non-`Transformation`-Transformation, because the parent's non-overriding message yields to `getMessage(issue.issue)`; setting `{ message, override: true }` on the parent reverses it, the parent's message replacing the entire subtree's — so a leaf-precise field message survives bubbling up through a class owner unless the owner deliberately overrides, the conflict resolved by data on the annotation, never call-site order.
- The up-walk terminates at a leaf or a `Transformation` of `kind: "Transformation"`: the `kind !== "Transformation"` condition means an `Encoded`/`Type`-side transform failure defers inward to the representation fault, but the transform PROCESS failure (the `decode`/`encode` function itself threw a `ParseIssue`) keeps its own message — so a `transformOrFail` whose conversion logic fails reports the transform's message, while a transform that failed only because its input was malformed reports the input's message, the two transform failure modes rendering distinctly without the consumer inspecting `kind`.
- `message` returning an `Effect<string>` makes the human-facing message itself a fallible, context-readable computation resolved inside the parse — the resolution threads through the same `flatMap`/`Effect.map` the parser runs, so a message that must consult a localization service or compute from injected context is a definition-time aspect of the owner, the rendering deferred to parse time yet declared once at the header.

[MISSING_LEAF_POLICY]:
- The `Missing` leaf is the one fault whose message rides a SEPARATE annotation: `missingMessage: () => string | Effect<string>` is a nullary thunk read off the property signature's AST by `getMissingMessageAnnotation(issue.ast)`, distinct from `message` because a missing key has no `actual` value for a `(issue) => …` message to inspect — the thunk takes nothing, since absence carries no value to describe.
- `missingMessage` lives on the FIELD's annotations, not the class header's `ClassAnnotations`, because it titles a specific property signature's absence — so an owner with several required fields carries one `missingMessage` per field whose omission must read distinctly, each riding its own field declaration, never a single header annotation that cannot tell which key vanished.
- The `Missing` leaf only forms under `exact` or on a genuinely absent key, never on an explicit `undefined` for a field admitting it: the struct parser builds `new Missing(ps)` exactly when `!hasKey` (and the field is non-optional, or `isExact`), so a field that accepts `undefined` and receives it produces a `Type` fault from its own schema, not a `Missing` — the leaf distinguishes "the key never arrived" from "the key arrived holding a value the schema rejects", a distinction the owner's `exact: true` parse policy sharpens.

[FALLBACK_RECOVERY_POLICY]:
- `decodingFallback: (issue: ParseIssue) => Effect<A, ParseIssue>` is wired at `goMemo` as `handleForbidden(orElse(parser(i, options), decodingFallback.value), ast, i, options)`, so the fallback is an `orElse` arm baked onto the node's own parser: when the node's decode produces a Left, the fallback effect runs with that issue and either recovers to a value of the node's own type or re-raises a refined `ParseIssue` — the recovery is a property of the owner's AST node, fired automatically at every decode of that owner, never a `catch` at each consumer.
- The fallback runs ONLY on the decode path (`isDecoding && Option.isSome(decodingFallbackAnnotation)`), never on encode — so an owner recovers a malformed wire value to a default instance while its encode path stays total, the asymmetry matching the directionality of admission: trust is granted inbound at the boundary, never fabricated outbound.
- Wrapping the fallback's `orElse` in `handleForbidden` means the fallback EFFECT is itself subject to the sync boundary: a `decodingFallback` that performs async work or pulls an unprovided dependency turns a recoverable decode failure into a `Forbidden` leaf at a sync seam — so the recovery aspect inherits the same execution-mode contract as the field schemas, and a context-dependent fallback is admissible only through an effect-allowed entrypoint, the recovery's requirement surfacing exactly where the owner's does.

```typescript
import { ParseResult, Schema } from 'effect';

// the owner bakes title, up-walking message, and decode-recovery as one declaration — the tree IS the policy
class Quantity extends Schema.Class<Quantity>('Quantity')(
    {
        amount: Schema.Number.pipe(
            Schema.positive(),
            // a leaf-precise field message survives the bubble up through the class owner
            Schema.annotations({ message: () => '<amount-must-exceed-zero>' }),
        ),
        unit: Schema.NonEmptyTrimmedString,
    },
    {
        parseOptions: { errors: 'all' },
        // titles the whole-record node distinctly from each field leaf, keyed on issue._tag
        parseIssueTitle: (issue) => (issue._tag === 'Composite' ? '<quantity-invalid>' : undefined),
        // recovers a malformed wire value to a canonical instance; runs only on decode, never encode
        decodingFallback: (issue) =>
            ParseResult.isComposite(issue) ? ParseResult.succeed(new Quantity({ amount: 1, unit: '<each>' })) : ParseResult.fail(issue),
    },
) {}

const admit = Schema.decodeUnknown(Quantity);

// reject: a consumer `try { decode(raw) } catch { return DEFAULT }` plus a sibling `titleFor(issue)`
// switch and a `messageFor(issue)` map re-implement, at every call site, the three node policies the
// owner already bakes onto its own AST — the recovery, the per-node title, and the message up-walk.
```

[CONSUMER_FOLD_COLLAPSE]:
- Because the tree is closed and the leaf address is the `Pointer` spine, one recursive fold over `ParseIssue` reaches every fault uniformly, and `ParseResult.isComposite` plus the `_tag` discriminant are the only branch the fold needs — a consumer that hand-writes per-`_tag` extraction, a flat-list builder, and a per-field message lookup is re-deriving a descent the owner's baked title/message/fallback already settle per node, the collapse being to read those node policies and let one fold descend.
- `ParseError` is the `YieldableError` wrapper holding `{ issue: ParseIssue }`, so the entire tree travels in the typed `E` channel as one value: a boundary `Effect.catchTag('ParseError', e => …)` receives the root issue and folds it once, the per-node owner policies already baked into the rendering — the cross-field invariant's message, the field's message, the recovery — so the consumer's fold never reconstructs what the owner declared, it walks a tree whose every node already carries its own policy.
- The `Composite.output` partial-result field makes the fold non-destructive: a consumer can both render the issue subtree AND read the salvaged valid fields from the same node, so a decode that fans wide under `errors: "all"` yields one value carrying every fault for rendering and every well-formed field for partial recovery — the loose spelling of a second "best-effort" decoder beside the strict one is the exact surface this dual-carrying node deletes.

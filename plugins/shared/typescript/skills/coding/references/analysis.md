# [ANALYSIS]

## [01]-[SYNTAX_TREES]

Foreign syntax trees index once into a value every consumer reads, and each later question about a node is a lookup. The host's walker reports every node with its parent, property, and position:

```ts
const index = (source: string, syntax: File): Tree => {
    const visits: Link[] = [];
    walk(syntax, (node, parent, prop, at) => {
        visits.push({ node: Equal.byReferenceUnsafe(node), parent, prop, index: at });
    });
    return new Tree({ syntax, located: createSourceIndex(source), links: new Map(visits.map((link) => [link.node, link])), children: Map.groupBy(visits, ({ parent }) => parent) });
};
```

`walk` and `createSourceIndex` are `sh-syntax` exports, `Link` holds a node with its parent, property, and index, and `Tree` is the `Data.Class` an editor, an analysis, and template construction read in place of a walk each:
- Facts a node derives from its children (a visible end) fill in one `reduceRight` over the visits, as a walker visits a parent first
- Parent paths unfold from the link map through `Array.unfold`, a node with no link ending the path at the root
- Nodes of one kind select through iterator helpers over the index (`tree.links.keys().filter(isBraced).toArray()`)
- Analyses read on demand are a field holding `Effect.cached(Effect.suspend(() => interpret(script)))`, run once for every reader
- Lexers fold one named-group pattern's `matchAll` through `Array.mapAccum` over a `{ depth, call }` state, each token keeping its offsets
- Lexers whose next offset depends on the token read step through `Array.unfold(offset, step)`, `Option.none()` ending the text

## [02]-[EVALUATORS]

Evaluators take the state they read and return the state they leave beside their value, so an assignment inside an expression reaches the next expression in order and no environment mutates:

```ts
const combine = (node: Arithm, scope: Scope): readonly [Scope, Integer] => {
    const [after, left] = evaluate(node.x, scope);
    const [end, right] = evaluate(node.y, after);
    return [end, lift([left, right], operators[node.op])];
};
```

`evaluate` dispatches on an arithmetic node's kind, `Integer` is a three-valued arithmetic domain, `lift` applies an operator over it, and `operators` holds one function per operator. `y` evaluates against the scope `x` left, so `$(( (i = 2) + i ))` reads `i` as 2:
- Parts of a word evaluate through `Array.mapAccum(parts, scope, (from, part) => [to, found])`, returning the last scope beside every result
- Branches on an unknown condition evaluate each arm from one scope and join the scopes the arms leave
- Name lookups carry the names they visited (`HashSet.add(visited, name)`), a name reached twice refusing where a depth cap guesses
- Expansions over an environment remove the binding they expand (`expanded(named, Record.remove(environment, name))`), ending a self-reference
- Constructs an evaluator cannot decide (a command substitution, a function call) evaluate to the domain's unknown value

## [03]-[ABSTRACT_DOMAINS]

Values an analysis cannot always compute are a tagged enum of what it knows: a value, an unknown value, or a refusal of a program the host rejects. Operators lift over the domain once:

```ts
type Integer = Data.TaggedEnum<{ Known: { readonly value: bigint }; Unknown: object; Refused: object }>;
const Integer = Data.taggedEnum<Integer>();

const lift = (values: readonly Integer[], apply: (...operands: readonly bigint[]) => Integer): Integer =>
    Option.getOrElse(Array.findFirst(values, Integer.$is('Refused')), () =>
        Option.match(Option.liftPredicate(values, Array.every(Integer.$is('Known'))), {
            onNone: () => Integer.Unknown(),
            onSome: (known) => apply(...known.map(({ value }) => value)),
        }),
    );
```

`Integer` is a shell arithmetic value, `Refused` a division by zero or a literal the dialect rejects. A refused operand refuses the result, an unknown one leaves it unknown, and known operands compute it:
- Refusals hold the `Origin` naming their construct, and unknown values the set of origins they came from, so a verdict names its cause
- Maps over open names are one `Total<A>` of `listed` values and an `unlisted` default joining pointwise, a pair of fields per kind restating it
- Joins derive from reducers: `Option.makeReducer(Combiner.make((self, that) => self.join(that)))` makes an unreached path the identity
- Flows hold one such `Option` per exit channel, and `Struct.makeReducer` over the channel reducers joins whole flows through `combineAll`
- Loop states widen each round against the literals the loop holds (`state.widen(next, thresholds)`), any other new value becoming unknown
- Variables holding more than one known value split evaluation into one scope per value through `Array.cartesianWith`, the results joining after

## [04]-[FIXED_POINTS]

Analyses repeat a round until its state stops changing. `Stream.paginate` runs the rounds with no stack depth, and `Equal.equals` against the held state ends them:

```ts
const iterate = (entry: State, thresholds: ReadonlySet<string>, round: (head: State) => Effect.Effect<Flow, ShellError>) =>
    Stream.paginate(entry, (head) =>
        Effect.map(round(head), (flow) => {
            const wider = head.widen(Option.match(flow.continues, { onNone: () => head, onSome: (again) => head.join(again) }), thresholds);
            return [[flow], Option.liftPredicate(wider, () => !Equal.equals(wider, head))] as const;
        }),
    ).pipe(Stream.runFold(() => Flow.none, (seen, flow) => flow.prepend(seen.found)));
```

`round` walks a loop body once from `head`, and `Flow` holds the state reaching each exit, `continues` among them, with the commands found. Each round joins the state reaching the next iteration into the head and widens it, and the round that changes nothing ends the loop:
- Worklists page one frontier per round, ending once `Set.difference` against the seen set leaves nothing
- Rewrite rounds carry a trail of earlier results, a result the trail holds ending them as a cycle and a round bound as divergence
- `Stream.runLast` reads the state a fixed point converged to, and `Stream.runFold` gathers what every round found
- Reachability is `Graph.values(Graph.dfs(Graph.fromSnapshot({ type: 'directed', nodes, edges }), { start, direction }))`
- `Graph.isAcyclic` refuses a cycle once, and `Graph.topo` throws on one
- Orders over cyclic references sort without edges inside one `Graph.stronglyConnectedComponents` set, applying in-set edges as a later step

## [05]-[REWRITING]

Edits are values: each edit is a `Result` of non-empty changes or a refusal naming its offset, every edit validates before any applies, and one pass splices the source:

```ts
type Edit = Result.Result<Array.NonEmptyReadonlyArray<Change>, Rejected>;

const apply = (source: string, edits: readonly Edit[]) =>
    Result.gen(function* () {
        const changes = (yield* Result.all(edits)).flat().toSorted((left, right) => left.start - right.start || left.end - right.end);
        const overlap = Array.findFirst(Array.window(changes, 2), ([left, right]) => right.start < left.end);
        yield* Option.match(overlap, { onNone: () => Result.void, onSome: ([, right]) => Result.fail<Rejected>({ kind: 'Overlap', offset: right.start }) });
        const [rest, pieces] = Array.mapAccum(changes, 0, (at, { start, end, text }) => [end, `${source.slice(at, start)}${text}`]);
        return `${pieces.join('')}${source.slice(rest)}`;
    });
```

`Change` holds a span of `source` and its replacement text, and `Rejected` the refusal's kind and offset. A refused edit or overlapping changes fail the whole application, and the splice copies each unchanged run once, where a string rebuilt per edit copies the text per edit:
- Template holes mark their position with a marker the template text lacks, parse once, and quote each hole for the context its tree position names
- Secret text keeps its redacted spans through concatenation, slicing, and replacement, a `Reducer.make` concatenation shifting the right side's spans
- Text holding a secret renders through `Formatter.format` of each `Redacted`, and implements `Redactable.symbolRedactable` for loggers
- Documents edit through their format library's concrete syntax tree, keeping comments and layout, a pattern rewrite restating it
- Source no TypeScript library parses (Swift) edits through the `ast-grep` CLI under the project's `sgconfig.yml`

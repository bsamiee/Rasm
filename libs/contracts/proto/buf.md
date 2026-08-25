# [RASM_CONTRACTS]

`rasm.contracts` is the Rasm estate's cross-language wire corpus: one Protobuf release unit whose packages define every value crossing a process or publisher boundary between the C#, Python, and TypeScript branches.

## [01]-[DEPENDING]

```yaml
version: v2
deps:
  - buf.build/rasm/contracts
```

- `buf dep update` resolves the reference to one immutable commit and locks it beside a `b5:` digest every later build re-verifies.
- Bare references resolve the default label `main`, which carries the released stream; naming a commit pins until an explicit update moves it.
- Resolution pins `bufbuild/protovalidate` and `googleapis/googleapis` transitively — field rules and `google.rpc` details ride the corpus.

## [02]-[SHAPE]

- Packages spell `rasm.contracts.<family>` under proto3, and managed mode derives every language option, so no source carries a file option.
- Field constraints are Protovalidate rules on the message; consumers evaluate them at their decode boundary, never re-spelling a scalar rule.
- Faults cross as one numeric case under a producing domain inside `FaultDetail`, carried by `google.rpc.Status` that keeps the transport code.
- Publisher schemata stay out: CloudEvents and gRPC health vendor as unnamed sibling modules and never enter this release unit.

## [03]-[EVOLUTION]

- Families reshape in place — one live shape per package, every estate consumer updated same-change, and `main` carries the released stream.
- Generation belongs at the consumer against its locked commit; that lock, never this module's label, is what dates every guarantee above.
- Tightened Protovalidate rules narrow admission at the decode door — a locked consumer holds while one re-resolving `main` meets the narrowed rule.
- Auto-served BSR SDKs carry the module whole with `opt` fixed at the plugin, so their surface and runtime pin match no branch emission here.

# [RASM_CONTRACTS]

`rasm.contracts` is the Rasm estate's cross-language wire corpus: one Protobuf release unit whose packages define every value crossing a process or publisher boundary between the C#, Python, and TypeScript branches.

## [01]-[DEPENDING]

```yaml copy-safe
version: v2
deps:
  - buf.build/rasm/contracts
```

- `buf dep update` resolves the reference to one immutable commit and locks it beside a `b5:` digest every later build re-verifies.
- Bare references resolve the default label `main`, which carries the released stream; naming a commit pins until an explicit update moves it.
- Resolution pins `bufbuild/protovalidate` and `googleapis/googleapis` transitively — field rules and `google.rpc` details ride the corpus.

## [02]-[SHAPE]

- Packages spell `rasm.contracts.<family>.v1` under proto3, and managed mode derives every language option, so no source carries a file option.
- Field constraints are Protovalidate rules on the message; consumers evaluate them at their decode boundary, never re-spelling a scalar rule.
- Faults cross as one numeric case under a producing domain inside `FaultDetail`, carried by `google.rpc.Status` that keeps the transport code.
- Publisher schemata stay out: CloudEvents and gRPC health vendor as unnamed sibling modules and never enter this release unit.

## [03]-[COMPATIBILITY]

- `breaking: use: [FILE]` grades every commit, so a file path, message, field, enum value, RPC, and service each survive once published.
- Retirement is `deprecated`, never deletion — `reserved` rescues no removal under FILE.
- Families that must change shape land `rasm.contracts.<family>.v2` beside the `v1` they leave intact.
- Tightened Protovalidate rules clear every breaking category, so a consumer reads the rule off the message and never trusts the gate for it.

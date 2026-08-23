# [PY_CONTRACTS_ARCHITECTURE]

`contracts` seats generated wire vocabulary at the Python estate floor. Separate estate and publisher roots preserve generator-relative imports without shadowing standard or installed modules.

## [01]-[DOMAIN_MAP]

```text codemap
contracts/
├── .api/rasm-contracts.md
├── pyproject.toml
└── src/rasm/contracts/
    ├── __init__.py
    ├── admission.py
    ├── artifact.py
    ├── py.typed
    ├── gen/
    │   ├── buf/validate/validate_pb.py
    │   ├── google/{rpc,type}/<source>_pb.py
    │   └── rasm/contracts/<family>/v1/
    │       ├── <source>_pb.py
    │       └── <source>_connect.py               # Service protocols, ASGI applications, and clients
    └── vendor/<publisher package>/
        ├── <source>_pb.py
        ├── <source>_connect.py                   # Publisher service stubs when consumed
        └── <source>.avsc                         # Exact publisher resource when consumed
```

## [02]-[STRATA]

`rasm` remains the shared PEP 420 namespace; `rasm.contracts` is the typed distribution boundary above the clean-swept `gen` and `vendor` roots. Consumers resolve the workspace package during development and the same wheel outside the repository.

## [03]-[SEAMS]

| [INDEX] | [KIND]        | [SOURCE]                    | [LANDING]                       |
| :-----: | :------------ | :-------------------------- | :------------------------------ |
|  [01]   | `[CONTRACT]`  | corpus estate descriptor    | `rasm.contracts.gen`            |
|  [02]   | `[BOUNDARY]`  | publisher descriptor        | `rasm.contracts.vendor`         |
|  [03]   | `[ASSET]`     | frozen publisher resource   | `rasm.contracts.vendor`         |
|  [04]   | `[PORT]`      | generated Connect interface | consumer-owned server or client |
|  [05]   | `[ADMISSION]` | generated body element      | descriptor rule evaluation      |
|  [06]   | `[ARTIFACT]`  | generated artifact service  | verified spool and transfer     |

## [04]-[INTERNAL]

Estate generation emits selected corpus packages with their reachable support closure under `gen`, publisher generation emits selected foreign packages and manifest-projected exact resources under `vendor`, and Connect generation follows each service-bearing root without minting a parallel message model.

`BodyAdmission` composes the official Connect interceptor protocols with Protovalidate's descriptor evaluator, owning message-body traversal and status projection rather than field rules or domain faults.

`ArtifactTransfer` composes generated messages and clients without mirroring them, owning temporary custody, framing, identity and extent proof, and publication confirmation while applications own storage and domain faults. It reads every bound from the generated `buf.validate` descriptors, rails refusals as `Result` values over one closed record family, and reconstructs a raise only where a generated stream demands one.

## [05]-[ROUTING]

| [INDEX] | [CHANGE]           | [OWNER_SURFACE]          | [EDIT]                       |
| :-----: | :----------------- | :----------------------- | :--------------------------- |
|  [01]   | corpus declaration | `tests/contracts/proto`  | edit source and regenerate   |
|  [02]   | publisher binding  | root Buf configuration   | change package filter        |
|  [03]   | publisher asset    | `tests/contracts/vendor` | replace bytes and regenerate |
|  [04]   | generator pair     | root Python manifest     | align source and regenerate  |
|  [05]   | consumer family    | root generation template | add or remove package token  |
|  [06]   | runtime closure    | package build manifest   | align install metadata       |

## [06]-[BOUNDARIES]

- `contracts` owns package identity, generated messages and services, Connect admission, validation closure, transfer proof, and publisher resources.
- Consumer packages own domain admission, service composition, and fault projection.
- Corpus sources and frozen publisher assets own wire shape.

---
paths:
  - "**/*.sh"
---

Shell scripts run under bash 5.3:
- An `exit` inside a `${ cmd; }` substitution ends the script, and a `&` job reads stdin from `/dev/null` without job control
- `SHELLOPTS=xtrace ./script` traces a script

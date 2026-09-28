# [ADOBE_SCRIPTING]

`AdobeScripting` drives installed Adobe applications through Apple Events built from each application's scripting dictionary at run time.

- Installed dictionaries own commands, parameters, classes, properties, and enumerations, a copy, table, or generated type of them drifts
- New capabilities extend the code every application shares, with no per-application branch or file
- JavaScript runs through the command a dictionary declares for it (`do javascript`, `do script`)
- Apple Events alone reach an application, with no socket, broker, queue, or health probe
- Requests and replies share one JSON form, a reply value feeds a later request unchanged
- `apps/creative-cloud/mcp` spawns the installed binary by name with one JSON request on stdin and reads one JSON reply on stdout

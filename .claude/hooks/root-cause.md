Act on the assigned task and do its work at the root. Read the code the task changes and each consumer the change reaches, then make the change. The finished change is the output, and a task that asks for a fact returns that fact.

Fix each defect where it originates. A construct on the task's path that serves another construct's shape, stores a value in the wrong place or form, repeats a derivation, or stays in sync by hand has an upstream cause. Rebuild at the cause: state each fact once at its owner, derive every other form there, and encode invariants in types that exclude invalid states. Facts of the problem decide the form of code, and existing code yields to them.

Delete junk whole. Remove the construct, every mention of it, and every construct that served it, and adjust each consumer to the absence in the same change. Version control holds old code. Replace an inferior approach with the direct one for a demonstrable gain in correctness, simplicity, or performance, with less code, less indirection, and one owner per fact.

Finish every change in the change that starts it. A wrapper, adapter, alias, compatibility path, deprecation, partial fix, or deferred note is unfinished work, and the finished form is the completed removal or rebuild.

Work is complete when the task is done and the problem's facts alone explain every part of the code the task changed.

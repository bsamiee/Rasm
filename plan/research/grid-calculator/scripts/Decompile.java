// Decompile functions of the program into one file, each under a `//==== FUNC @ <request> -> <entry>` line:
//   -postScript Decompile.java <out>.c all
//   -postScript Decompile.java <out>.c at 1101a0,110ffc,12335c
//   -postScript Decompile.java <out>.c string 'Gutter too large|minimum value'
// A listed address inside no function is disassembled and given a function first, and every function
// gets the one-hour timeout and the 1000 MB payload the largest observers need.
import ghidra.app.decompiler.DecompInterface;
import ghidra.app.decompiler.DecompileOptions;
import ghidra.app.decompiler.DecompileResults;
import ghidra.app.script.GhidraScript;
import ghidra.program.model.listing.Function;
import java.io.IOException;
import java.io.PrintWriter;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Arrays;
import java.util.List;
import java.util.Objects;
import java.util.Optional;
import util.CollectionUtils;

public class Decompile extends GhidraScript {
    private static final int TIMEOUT_SECONDS = 3600;
    private static final int PAYLOAD_MBYTES = 1000;

    record Target(String request, Optional<Function> function) {
        static Target of(Function function) {
            return new Target(function.getEntryPoint().toString(), Optional.of(function));
        }
    }

    @Override
    public void run() throws IOException {
        var args = getScriptArgs();
        var targets = switch (args[1]) {
            case "all" -> CollectionUtils.asStream(currentProgram.getFunctionManager().getFunctions(true)).map(Target::of);
            case "at" -> Arrays.stream(args[2].split(",")).map(this::at);
            case "string" -> DefinedString.containing(currentProgram, List.of(args[2].split("\\|")))
                .flatMap(string -> string.referrers(currentProgram))
                .map(this::getFunctionContaining)
                .filter(Objects::nonNull)
                .distinct()
                .map(Target::of);
            default -> throw new IllegalArgumentException("selector " + args[1] + " is none of all, at, string");
        };
        var options = new DecompileOptions();
        options.setMaxPayloadMBytes(PAYLOAD_MBYTES);
        var decompiler = new DecompInterface();
        decompiler.setOptions(options);
        decompiler.toggleSyntaxTree(false);
        if (!decompiler.openProgram(currentProgram)) {
            throw new IllegalStateException(decompiler.getLastMessage());
        }
        try (var out = new PrintWriter(Files.newBufferedWriter(Path.of(args[0])), true)) {
            targets.forEach(target -> out.println(dump(decompiler, target)));
        } finally {
            decompiler.dispose();
        }
    }

    private Target at(String hex) {
        var address = toAddr(Long.parseLong(hex, 16));
        return new Target(hex, Optional.ofNullable(getFunctionContaining(address)).or(() -> {
            disassemble(address);
            return Optional.ofNullable(createFunction(address, null));
        }));
    }

    private String dump(DecompInterface decompiler, Target target) {
        println("decompiling " + target.request());
        var entry = target.function().map(function -> function.getEntryPoint().toString()).orElse("none");
        var body = target.function().map(function -> "\n" + code(decompiler.decompileFunction(function, TIMEOUT_SECONDS, monitor))).orElse("");
        return "//==== FUNC @ " + target.request() + " -> " + entry + body;
    }

    private static String code(DecompileResults results) {
        return results.decompileCompleted() ? results.getDecompiledFunction().getC() : "// failed: " + results.getErrorMessage();
    }
}

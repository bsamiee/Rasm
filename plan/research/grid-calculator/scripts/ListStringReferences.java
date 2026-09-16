// List every defined string holding one of the needles, then the address and function of each reference to it:
//   -postScript ListStringReferences.java <out>.txt 'Fit Leading|kGCLockKey'
import ghidra.app.script.GhidraScript;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.List;
import java.util.Optional;
import java.util.stream.Stream;

public class ListStringReferences extends GhidraScript {
    @Override
    public void run() throws IOException {
        var args = getScriptArgs();
        Files.write(Path.of(args[0]), DefinedString.containing(currentProgram, List.of(args[1].split("\\|"))).flatMap(this::lines).toList());
    }

    private Stream<String> lines(DefinedString string) {
        var references = string.referrers(currentProgram).map(from -> "  ref " + from + " in "
            + Optional.ofNullable(getFunctionContaining(from)).map(function -> function.getEntryPoint().toString()).orElse("none"));
        return Stream.concat(Stream.of("STR " + string.address() + " " + string.text().replace("\n", "\\n")), references);
    }
}

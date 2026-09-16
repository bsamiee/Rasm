// List every function as `<entry> <body bytes> <name>`, the map `disassembly.py callers` reads:
//   -postScript ListFunctions.java funcs.txt
import ghidra.app.script.GhidraScript;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import util.CollectionUtils;

public class ListFunctions extends GhidraScript {
    @Override
    public void run() throws IOException {
        Files.write(Path.of(getScriptArgs()[0]), CollectionUtils.asStream(currentProgram.getFunctionManager().getFunctions(true))
            .map(function -> function.getEntryPoint() + " " + function.getBody().getNumAddresses() + " " + function.getName())
            .toList());
    }
}

// A defined string of the program with its text, the selection Decompile and ListStringReferences share.
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Program;
import ghidra.program.model.symbol.Reference;
import java.util.List;
import java.util.stream.Stream;
import util.CollectionUtils;

record DefinedString(Address address, String text) {
    static Stream<DefinedString> containing(Program program, List<String> needles) {
        return CollectionUtils.asStream(program.getListing().getDefinedData(true))
            .flatMap(data -> data.getValue() instanceof String text && needles.stream().anyMatch(text::contains)
                ? Stream.of(new DefinedString(data.getAddress(), text))
                : Stream.empty());
    }

    Stream<Address> referrers(Program program) {
        return CollectionUtils.asStream(program.getReferenceManager().getReferencesTo(address)).map(Reference::getFromAddress);
    }
}

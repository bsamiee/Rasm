# [PROPERTIES]

## [01]-[ROUND_TRIPS]

Properties draw their inputs from an engine seeded by a literal and capture each draw:

```cpp
TEST_CASE("extent text formatted from two sides parses back to them", "[raster][extent]") {
    std::mt19937 engine{20'261'112};
    std::uniform_int_distribution<std::uint32_t> side{1, std::numeric_limits<std::uint32_t>::max()};

    for (auto _ : std::views::iota(0, 2'000)) {
        const auto width = side(engine);
        const auto height = side(engine);
        CAPTURE(width, height);

        const auto parsed = raster::extent_of(std::format("{}x{}", width, height), std::numeric_limits<std::uint64_t>::max());

        REQUIRE(parsed.has_value());
        REQUIRE(parsed->width() == width);
        REQUIRE(parsed->height() == height);
    }
}
```

## [02]-[REFERENCE_PATHS]

Optimized paths agree with a plain reference written in the test over the same inputs:

```cpp
TEST_CASE("level totals over columns equal totals over records", "[raster][tiles]") {
    struct Record {
        std::uint64_t bytes;
        std::uint8_t level;
    };

    std::mt19937 engine{20'261'112};
    std::uniform_int_distribution<std::uint64_t> size{0, 1ULL << 40};
    std::uniform_int_distribution<int> depth{0, 3};
    const auto records =
        std::views::iota(0, 4'096) | std::views::transform([&](int) { return Record{.bytes = size(engine), .level = static_cast<std::uint8_t>(depth(engine))}; }) | std::ranges::to<std::vector>();
    const raster::Tiles tiles{
        .bytes = records | std::views::transform(&Record::bytes) | std::ranges::to<std::vector>(),
        .column = std::vector<std::uint32_t>(records.size()),
        .level = records | std::views::transform(&Record::level) | std::ranges::to<std::vector>(),
    };

    for (const auto level : {std::uint8_t{0}, std::uint8_t{3}, std::uint8_t{4}}) {
        CAPTURE(int{level});
        std::uint64_t expected{0};
        for (const auto& record : records) {
            if (record.level == level) {
                expected += record.bytes;
            }
        }

        CHECK(tiles.at_level(level) == expected);
    }
}
```

`Tiles` stores `bytes`, `column`, and `level` as one array each. Level `4` holds no tile, and its total is `0` on both paths.

## [03]-[FUZZ_TARGETS]

Fuzz targets enter at the function taking untrusted bytes and end the process when an oracle fails:

```cpp
extern "C" auto LLVMFuzzerTestOneInput(const std::uint8_t* data, std::size_t size) -> int {
    constexpr std::uint64_t limit{1'000'000};
    const std::string text(data, std::next(data, static_cast<std::ptrdiff_t>(size)));

    const auto parsed = raster::extent_of(text, limit);

    if (parsed && (parsed->cells() == 0 || parsed->cells() > limit || raster::extent_of(std::format("{}x{}", parsed->width(), parsed->height()), limit) != parsed)) {
        std::abort();
    }
    return 0;
}
```

Accepted extents are nonzero, within `limit`, and equal to the parse of their own formatted text. Inputs that end the process join the corpus before their fix.

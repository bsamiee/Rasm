# [SEAMS]

## [01]-[COUNTED_ALLOCATIONS]

Allocation counts are asserted through a memory resource the test passes:

```cpp
class Counted final : public std::pmr::memory_resource {
public:
    explicit Counted(std::pmr::memory_resource& upstream) noexcept
        : upstream_{&upstream} {}

    [[nodiscard]] auto allocations() const noexcept -> std::size_t { return allocations_; }

private:
    auto do_allocate(std::size_t bytes, std::size_t alignment) -> void* override {
        allocations_ += 1;
        return upstream_->allocate(bytes, alignment);
    }

    auto do_deallocate(void* block, std::size_t bytes, std::size_t alignment) -> void override { upstream_->deallocate(block, bytes, alignment); }

    [[nodiscard]] auto do_is_equal(const std::pmr::memory_resource& other) const noexcept -> bool override { return this == &other; }

    std::pmr::memory_resource* upstream_;
    std::size_t allocations_{0};
};

TEST_CASE("row median allocates once from its caller's resource", "[raster][median]") {
    const std::array<raster::Rgb, 5> row{
        {{.red = 9, .green = 0, .blue = 0}, {.red = 1, .green = 0, .blue = 0}, {.red = 5, .green = 0, .blue = 0}, {.red = 3, .green = 0, .blue = 0}, {.red = 7, .green = 0, .blue = 0}}};
    std::array<std::byte, 64> storage{};
    std::pmr::monotonic_buffer_resource arena{storage.data(), storage.size(), std::pmr::null_memory_resource()};
    Counted counted{arena};

    CHECK(raster::median(row, counted) == 5U);
    CHECK(counted.allocations() == 1);
}
```

`median` takes its scratch storage from a `std::pmr::memory_resource` parameter. `std::pmr::null_memory_resource()` upstream makes an allocation past `storage` throw in place of reaching the heap.

## [02]-[WAITS]

Concurrent cases end on a stop request and a join, then assert state their own thread owns:

```cpp
TEST_CASE("inbox hands each pushed item to its reader in push order", "[raster][inbox]") {
    raster::Inbox<int> inbox;
    std::vector<int> received;
    {
        std::jthread reader{[&](const std::stop_token& stop) {
            while (const auto item = inbox.pop(stop)) {
                received.push_back(*item);
            }
        }};
        std::ranges::for_each(std::views::iota(0, 100), [&](int item) { inbox.push(item); });
        reader.request_stop();
    }

    CHECK_THAT(received, Catch::Matchers::RangeEquals(std::views::iota(0, 100)));
}
```

`Inbox::pop` returns each queued item before it reports a stop, and `reader` joins at its closing brace.

## [03]-[BENCHMARKS]

Benchmarks assert a result before timing it:

```cpp
TEST_CASE("level total over a full tile set", "[raster][tiles][!benchmark]") {
    const raster::Tiles tiles{.bytes = std::vector<std::uint64_t>(1 << 16, 1), .column = std::vector<std::uint32_t>(1 << 16), .level = std::vector<std::uint8_t>(1 << 16, 2)};

    REQUIRE(tiles.at_level(2) == 1ULL << 16);
    BENCHMARK("at_level over 65536 tiles") { return tiles.at_level(2); };
}
```

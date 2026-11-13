# [SEAMS]

## [01]-[COUNTED_STORAGE]

Storage a contract routes through a passed resource is counted by a resource the test passes:

```cpp
class Counted final : public std::pmr::memory_resource {
public:
    [[nodiscard]] auto allocations() const noexcept -> std::size_t { return allocations_; }

private:
    auto do_allocate(std::size_t bytes, std::size_t alignment) -> void* override {
        allocations_ += 1;
        return std::pmr::new_delete_resource()->allocate(bytes, alignment);
    }

    auto do_deallocate(void* block, std::size_t bytes, std::size_t alignment) -> void override { std::pmr::new_delete_resource()->deallocate(block, bytes, alignment); }

    [[nodiscard]] auto do_is_equal(const std::pmr::memory_resource& other) const noexcept -> bool override { return this == &other; }

    std::size_t allocations_{0};
};

TEST_CASE("row median takes its temporary storage from its caller's resource", "[raster][median]") {
    const std::array<raster::Rgb, 5> row{
        {{.red = 9, .green = 0, .blue = 0}, {.red = 1, .green = 0, .blue = 0}, {.red = 5, .green = 0, .blue = 0}, {.red = 3, .green = 0, .blue = 0}, {.red = 7, .green = 0, .blue = 0}}};
    Counted scratch;

    const auto middle = raster::median(row, scratch);

    REQUIRE(middle.has_value());
    CHECK(*middle == 5U);
    CHECK(scratch.allocations() > 0);
}
```

`median` documents that its temporary storage comes from `scratch`.

## [02]-[ORDER_ON_ONE_THREAD]

Contracts about order run on one thread:

```cpp
TEST_CASE("inbox hands out each queued item before it reports a stop", "[raster][inbox]") {
    raster::Inbox<int> inbox;
    std::stop_source source;
    for (const auto item : {0, 1, 2}) {
        inbox.push(item);
    }
    source.request_stop();

    for (const auto expected : {0, 1, 2}) {
        const auto item = inbox.pop(source.get_token());
        REQUIRE(item.has_value());
        CHECK(*item == expected);
    }
    CHECK_FALSE(inbox.pop(source.get_token()).has_value());
}
```

`Inbox::pop` documents that it returns each queued item before it reports a stop.

## [03]-[WAITS]

Blocking contracts take a second thread, which counts down a latch before it waits:

```cpp
TEST_CASE("inbox reader blocked on an empty inbox receives a later push", "[raster][inbox]") {
    raster::Inbox<int> inbox;
    std::latch waiting{1};
    std::optional<int> received;
    {
        const std::jthread reader{[&](const std::stop_token& stop) {
            waiting.count_down();
            received = inbox.pop(stop);
        }};
        waiting.wait();
        inbox.push(7);
    }

    REQUIRE(received.has_value());
    CHECK(*received == 7);
}
```

`push` follows `waiting.wait()`, so `reader` is in `pop` or about to enter it. `reader` joins at its closing brace, and `received` is read after that join. A `pop` returning without a wait fails most runs of this case, and a ThreadSanitizer run covers what scheduling leaves.

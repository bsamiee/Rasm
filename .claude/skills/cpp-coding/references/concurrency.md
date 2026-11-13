# [CONCURRENCY]

## [01]-[SCOPED_WORKERS]

Threads belong to a scope that joins them before the state they borrow ends:

```cpp
template <std::invocable<std::size_t> Work>
auto each_tile(std::size_t tiles, std::uint32_t workers, const std::stop_token& stop, Work work) -> void {
    std::atomic<std::size_t> next{0};
    const auto claim = [&] {
        for (auto index = next.fetch_add(1, std::memory_order_relaxed); index < tiles && !stop.stop_requested(); index = next.fetch_add(1, std::memory_order_relaxed)) {
            work(index);
        }
    };
    const auto crew = std::views::iota(0U, workers) | std::views::transform([&](std::uint32_t) { return std::jthread{claim}; }) | std::ranges::to<std::vector>();
}
```

`crew` is declared last and joins first, while `next`, `claim`, and `work` are alive. `next` hands out indices and publishes no other data, so its order is `std::memory_order_relaxed`. Workers write disjoint elements and take no lock.

## [02]-[GUARDED_STATE]

Shared state and its mutex are one type, read and written inside a call holding the lock:

```cpp
template <typename Value>
class Guarded {
public:
    explicit Guarded(Value value)
        : value_{std::move(value)} {}

    template <std::invocable<Value&> Step>
        requires(!std::is_reference_v<std::invoke_result_t<Step, Value&>>)
    auto with(Step&& step) -> std::invoke_result_t<Step, Value&> {
        const std::scoped_lock lock{mutex_};
        return std::invoke(std::forward<Step>(step), value_);
    }

private:
    std::mutex mutex_;
    Value value_;
};
```

`with` refuses a step returning a reference. Steps return a copy or a moved value, a view or pointer into the guarded value outlives the lock.

## [03]-[WAITS]

Waits end on a predicate or a stop request:

```cpp
template <typename Item>
class Inbox {
public:
    auto push(Item item) -> void {
        {
            const std::scoped_lock lock{mutex_};
            items_.push_back(std::move(item));
        }
        ready_.notify_one();
    }

    [[nodiscard]] auto pop(const std::stop_token& stop) -> std::optional<Item> {
        std::unique_lock lock{mutex_};
        if (!ready_.wait(lock, stop, [&] { return !items_.empty(); })) {
            return std::nullopt;
        }
        auto item = std::move(items_.front());
        items_.pop_front();
        return item;
    }

private:
    std::mutex mutex_;
    std::condition_variable_any ready_;
    std::deque<Item> items_;
};
```

`pop` returns each queued item before it reports a stop, and a reader loops on `while (const auto item = inbox.pop(stop))`.

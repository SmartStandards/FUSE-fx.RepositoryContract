# CachedRepository for FUSE-fx IRepository

## 1. Introduction

### Abstract

The `CachedRepository<TEntity, TKey>` is a high-performance, thread-safe caching wrapper for the FUSE-fx `IRepository<TEntity, TKey>` contract.  
It introduces **query-scoped caching**, **key-based entity caching**, and **stale-while-revalidate semantics**, while remaining fully transparent to consuming code.

Unlike many competing repository cache implementations, this design:

- Preserves **strong consistency guarantees** under concurrent access
- Avoids data loss by preferring **patching over invalidation**
- Supports **query-level caching** with automatic enablement heuristics
- Operates without `async/await`, enabling use in legacy or constrained runtimes
- Exposes **cache state observability** for diagnostics and UI integration

The wrapper is designed as a drop-in replacement for any existing `IRepository` implementation.

---

## 2. Motivation & Differentiation

### Problems Addressed

Typical repository caching solutions fail in one or more of these areas:

| Problem | Typical Implementations |
|------|--------------------------|
| Query result caching | Missing or unsafe |
| Thread safety | Lock-heavy or race-prone |
| Cache invalidation | Over-aggressive (flush-all) |
| Update propagation | Often ignored |
| Observability | Rarely available |
| Paging correctness | Frequently broken |

### Key Differentiators

| Feature | CachedRepository |
|------|------------------|
| Query-scoped caching | ✔ Opt-in / auto-enabled |
| Stale-While-Revalidate | ✔ |
| Single-flight refresh | ✔ |
| Patch-based updates | ✔ |
| Delete patching | ✔ |
| Cache state exposure | ✔ |
| Non-async runtime | ✔ |

---

## 3. Conceptual Architecture (Top-Down)

```
Consumer
   │
   ▼
CachedRepository<TEntity,TKey>
   │
   ├── Key Cache (Entities / Refs / Fields)
   │
   ├── Query Result Cache
   │     ├── ExpressionTree
   │     ├── SearchExpression
   │     ├── Sorting / Paging
   │
   └── Inner IRepository<TEntity,TKey>
```

Two cache layers coexist:

1. **Key Cache** – optimized for direct key-based access  
2. **Query Cache** – optimized for filtered/search-based queries

---

## 4. Getting Started (Minimal Example)

```csharp
IRepository<Customer, int> baseRepo = new SqlCustomerRepository(connection);

CachedRepositoryOptions<Customer, int> options =
  new CachedRepositoryOptions<Customer, int>();

options.KeySelector = (c) => c.Id;
options.EnableQueryCacheOptIn = true;

IRepository<Customer, int> cached =
  new CachedRepository<Customer, int>(baseRepo, options);
```

No further changes are required in consuming code.

---

## 5. Query-Scoped Caching

### What is Cached?

Each unique query is cached independently, including:

- ExpressionTree structure
- SearchExpression
- Included fields
- Sorting
- Paging (limit / skip)
- Repository origin identity

### Cache Key Stability

Expression trees are normalized into a **stable string representation**:

- Predicates are sorted deterministically
- Operators are preserved as strings
- Tree structure is recursively serialized

This guarantees identical semantic queries map to the same cache entry.

---

## 6. Cache Lifetime Model

### Stepped Expiration

Cache lifetime is extended per access using configurable steps:

```csharp
options.AccessExtensionSeconds = new int[] { 5, 15, 60 };
```

| Access Count | TTL Extension |
|-------------|---------------|
| 1 | +5s |
| 2 | +15s |
| 3+ | +60s |

### Absolute Max Lifetime (Optional)

```csharp
options.UseAbsoluteMaxLifetime = true;
options.AbsoluteMaxLifetime = TimeSpan.FromMinutes(30);
```

Prevents infinite cache survival under heavy access.

---

## 7. Stale-While-Revalidate

When a cache entry expires:

- **Stale value is returned immediately**
- A **single background refresh** is started
- Concurrent callers join the same refresh task

```csharp
options.ReadMode = CacheReadMode.AllowStaleWhileRefresh;
```

This avoids latency spikes and thundering-herd effects.

---

## 8. Auto Prefetch

Prefetching can be triggered automatically:

```csharp
options.PrefetchTrigger =
  PrefetchTrigger.OnStart | PrefetchTrigger.OnInvalidate;
```

Prefetch behavior:
- Never blocks callers
- Uses double-buffering
- Does not discard valid data on failure

---

## 9. Change Processing Semantics

### Modes

```csharp
options.ChangeProcessing = ChangeProcessing.Patch;
```

| Mode | Behavior |
|----|---------|
| Decoupled | Forward only, no cache changes |
| Patch | Patch key-cache + query-cache |
| Invalidate | Clear cache before forwarding |

### Delete Handling

Deletes are handled via **patch semantics**:

- Entity removed from key cache
- Entity removed from cached query results
- Paging-safe best-effort behavior

---

## 10. Update & Patch Strategy

### Entity Updates

- Key cache updated immediately
- Query results scanned and patched by key
- If patch safety cannot be guaranteed → fallback to invalidation

### Mass Updates

- Key cache entries removed
- Query cache invalidated (to avoid incorrect paging)

---

## 11. Automatic Query Cache Enablement

Query caching can be:

- Explicitly enabled via option
- Automatically enabled when `CountAll() > threshold`

```csharp
options.AutoEnableQueryCacheAboveEntityCount = 200;
```

This prevents unnecessary memory usage for small datasets.

---

## 12. Cache State Observability

Consumers can inspect runtime cache state:

```csharp
RepositoryCacheState state =
  ((CachedRepository<Customer,int>)cached).CacheState;
```

Available metrics:

| Metric |
|------|
| Last refresh attempt |
| Last successful refresh |
| Hit / Miss counters |
| Active refresh count |
| Cached entity count |
| Cached query count |

---

## 13. Error Handling & Safety

- No silent failures
- Expected exceptions are logged via `DevLogger.LogError`
- Cache is never corrupted by failed refreshes
- Inconsistent patch scenarios degrade safely to invalidation

---

## 14. Performance Characteristics

| Aspect | Strategy |
|-----|---------|
| Concurrency | Fine-grained locks per entry |
| Refresh | Single-flight |
| Memory | Bounded query cache |
| Allocation | Arrays preferred over lists |
| Reflection | Cached / limited to key extraction |

---

## 15. Bottom-Up: Key Artifacts

### CachedRepository<TEntity,TKey>

Main wrapper implementing `IRepository<TEntity,TKey>`.

### CachedRepositoryOptions<TEntity,TKey>

Defines all cache behavior:
- Lifetime
- Prefetch
- Change semantics
- Query cache enablement

### _CacheEntry (internal)

Encapsulates:
- Cached value
- Expiration
- Refresh task
- Generation tracking

### RepositoryCacheState

Immutable snapshot of runtime cache metrics.

---

## 16. Typical Use Cases

### UI Grid with Filters

- Query cache avoids repeated filter evaluation
- Stale-while-revalidate keeps UI responsive

### High-Latency Backends

- Prefetch + stale reads hide backend latency
- Single-flight prevents overload

### Mixed Read/Write Workloads

- Patch-based updates preserve cache usefulness
- Minimal invalidation scope

---

## 17. Summary

The `CachedRepository<TEntity,TKey>` provides:

- Safe, observable, and configurable caching
- Full compatibility with the FUSE-fx IRepository contract
- Production-grade concurrency behavior
- A balance between correctness and performance

It is intended as a **foundational building block** for higher-level access contexts, UI layers, and aggregation repositories.

---

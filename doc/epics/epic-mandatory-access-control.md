# Epic: Mandatory Access Control (MAC)

Status: **idea / under discussion** (not an active requirement yet, see [requirements](../requirements.md)).
This epic is intended to become a dedicated AI skill once the concept is stable.

## 1. Motivation

Multi-tenant and confidentiality-aware applications must guarantee that a caller can only see and modify data
inside the scopes he is cleared for (e.g. `Tenant:4711`, `ConfLevel:3`). Today this is usually implemented by
hand in every query, which is error-prone: one forgotten `Where` leaks foreign data.

MAC moves this responsibility into the repository layer: **mandatory filters** are applied to every read and
**scope violations** are rejected on every write - independent of whether the repository is backed by EF, by a
file store or by a web service client (UJMW).

## 2. Origin: the PoC `KornSW.EntityFrameworkCore.DataAccessFilter`

Repository: `https://github.com/KornSW/EntityFrameworkCore.DataAccessFilter` (local: `C:\GIT-TK\EntityFrameworkCore.DataAccessFilter`),
package `KornSW.EntityFrameworkCore.DataAccessFilter` 2.1.1, namespace `System.Data.AccessControl`.

### 2.1 What the PoC does

- **Dimension**: a string such as `Tenant` or `ConfLevel`.
- **Classification**: an entity property bound to a dimension, either via
	`[AccessControlClassification("Tenant")]` or via
	`EntityAccessControl.RegisterPropertyAsAccessControlClassification((ArchiveEntity e) => e.TenantName, "Tenant")`.
	Supported property types: `string`, `byte`, `Int16`, `Int32`, `Int64`, `bool`, `DateTime`, `double`, `decimal`, `Guid`.
- **Clearances**: a static delegate `EntityAccessControl.ClearanceGetter(dimensionName) : string[]`.
	Syntax per value: `x` grants, `*` grants all, `!x` denies, `!*` denies all, empty array denies all (fail-safe).
- **Combination**: AND across dimensions (and across classified properties), OR within the granted values of one dimension,
	`granted AND NOT denied`.
- **Relational (join) classification**: the filter is propagated recursively over all navigation properties
	annotated as `[Principal]` or `[Lookup]` (via `FUSE-fx.EntityAnnotations` `GetNavigations()`), so
	`Document` is filtered by `Document.Archive.TenantName`.
- **Usage**: `db.Documents.AccessScopeFiltered().Where(...)` - a pure LINQ `Expression<Func<T,bool>>` appended to any
	`IQueryable<T>`.

**Key insight:** the PoC is **not EF-specific at all**. It only depends on `FUSE-fx.EntityAnnotations` and builds plain
LINQ expressions. EF merely translates the navigation member accesses into SQL JOINs. The unit test runs against
LINQ-to-Objects.

### 2.2 Weaknesses of the PoC (must be fixed in the real feature)

| # | Finding | Impact |
|---|---|---|
| W1 | For deny values the constant is built **before** the `!` prefix is stripped. | Denies on string dimensions never match; numeric dimensions throw a parse exception. **Bug.** |
| W2 | Filter expressions are cached in static per-type builder instances; cache validity is checked via a clearance hash, but rebuild/use are not atomic across callers. | **Security risk**: concurrent requests with different clearances may receive a foreign filter. |
| W3 | The cache is bound to the last navigation path (`entitySourceExpression`). | Constant cache thrashing when a type is reachable via multiple paths. |
| W4 | Clearance values are embedded as **constants**. | Every clearance set produces different SQL; EF query plan caching and DB plan caching do not work. **Main performance issue.** |
| W5 | Implicit traversal of **all** principals and lookups, no cycle detection, no depth limit. | Possible endless recursion and unnecessary JOINs. |
| W6 | No defined semantics for `null` classifications and optional navigations. | NullReferenceException in LINQ-to-Objects, silent row loss in SQL. |
| W7 | Read filtering only; no single-entity violation signalling; no insert/update validation; static global clearance source. | Not a complete MAC; not request-scoped. |

## 3. Goals

- **G1** Mandatory filtering of every multi-result read (`GetEntities*`, `GetEntityRefs*`, `GetEntityFields*`, `Count*`).
- **G2** Keyed single-entity access (`*ByKey`) to entities outside the caller's clearance raises a dedicated violation exception
	(configurable: throw vs. treat as not found).
- **G3** Insert and update are validated against the clearances for the **new** state and (update) the **old** state, so data cannot
	be moved out of or into a foreign scope.
- **G4** Mass update and delete only affect entities inside the caller's clearance.
- **G5** Existence probing must not leak information (`ContainsKey`, duplicate-key errors).
- **G6** Classification may be **local** (property on the entity) or **relational** (property on a related principal entity, reached via
	navigation path or foreign key).
- **G7** **Hybrid execution**: push the filter down to `IQueryable` (single SQL statement with JOINs) when the data lives in the same
	database, and still support **composite** setups where related data lives in another repository (e.g. EF + UJMW web service client).
- **G8** Performance: parameterized filters (plan-cache friendly), no per-request expression rebuild, bounded JOIN depth.
- **G9** Thread safety: clearances are resolved per request/ambient context, never via mutable static state shared across callers.
- **G10** **Maximum configurability** (developer decision): every policy below is configurable per dimension and/or per entity type,
	with secure defaults.

## 4. Non-goals

- Authentication and token validation (clearances are an input, e.g. derived from token scopes like `Tenant:x`).
- Discretionary/role-based permissions on operations (e.g. "may delete") - MAC is about **data scopes**.
- Row-level security inside the database engine (may be an optional additional push-down target later).

## 5. Terminology

| Term | Meaning |
|---|---|
| Dimension | Named classification axis, e.g. `Tenant`, `ConfLevel`, `Region`. |
| Classification | Value of an entity within a dimension, resolved locally or via a relation path. |
| Clearance | Set of values a caller is allowed (or explicitly denied) within a dimension. |
| Scope | Pair `Dimension:Value`, e.g. `Tenant:4711`; token scopes are a typical clearance source. |
| Violation | Attempt to read a single entity or write an entity whose classification is outside the clearance. |
| Push-down | Translating the mandatory filter into the native query of the underlying store (`IQueryable`, SQL). |

## 6. Decided so far

- **D1 Placement**: the EF-independent core lives in this repository as a sub-namespace of the core contracts
	(proposed `System.Data.Fuse.MandatoryAccessControl` in shared project `RepositoryContract`); the EF push-down lives in `EfSupport`.
- **D2 Configurability**: maximum configurability with secure defaults (see section 8).

## 7. Proposed design

### 7.1 Core abstractions (EF-free)

- `IClearanceProvider` - request-scoped source of clearances per dimension (replaces the static `ClearanceGetter`).
	Ships with an adapter from token scopes (`Dimension:Value`).
- `MacPolicy` / registry - explicit registration of classifications per entity type:
	- local: `Classify<Archive>((a) => a.TenantName, "Tenant")`
	- relational by navigation path: `Classify<Document>("Archive.TenantName", "Tenant")`
	- relational by foreign key + principal repository: `Classify<Document>("ArchiveId", typeof(Archive), "Tenant")`
	- attribute-based registration remains available as convenience.
	Relational paths are **explicit**; there is no implicit traversal of all navigations (fixes W5).
- Clearance model compiled once per request into an immutable `ClearanceSet` (grant values, deny values, all/none flags) - fixes W1.

### 7.2 Enforcement point: `MacRepository<TEntity, TKey> : IRepository<TEntity, TKey>` (decorator)

- Multi-result reads: `filter AND mandatoryFilter`.
- `*ByKey` reads: load, check, then throw `MacViolationException` (or return `null`, configurable).
- `Count*`, `ContainsKey`: filtered.
- `TryAddEntity`, `AddOrUpdate*`, `TryUpdate*`: validate new state; for updates also validate the persisted old state.
- `Massupdate*`, `TryDeleteEntities`, `TryUpdateKey`: restricted to entities inside the clearance; classification fields must not be
	changed to foreign values.
- `GetCapabilities()` is passed through; `GetOriginIdentity()` stays unchanged.
- Can be applied by `RepositoryCollection` / data stores so that consumers cannot bypass it accidentally.

Alternative considered: **hooks inside each repository implementation**. Rejected as primary mechanism because every implementation
would have to re-implement enforcement; hooks are still useful as an **optional push-down extension** (see 7.3).

### 7.3 Hybrid execution

1. **Push-down (same database)**: the inner repository may implement an optional extension, e.g.
	 `IMandatoryFilterSupport<TEntity>`, accepting a LINQ predicate. `EfRepository` applies it on a single query root
	 (requires refactoring the ~30 direct `dbContext.Set<TEntity>()` usages into one `GetQueryRoot()` method).
	 Clearances are passed as captured variables / `Contains` lists so that SQL stays parameterized (fixes W4).
2. **Fallback (composite repositories)**: the decorator expresses the mandatory filter as `ExpressionTree`/`FieldPredicate`.
	 For relational classifications whose principal lives in another repository it performs a **semi-join**:
	 first resolve the permitted principal keys via the (MAC-protected) principal repository, then filter the dependent by
	 `ForeignKey in [...]`. Results are cached per request and clearance set.
3. **Denormalization option**: a redundant classification column (e.g. `TenantId` on `Document`) avoids JOINs entirely;
	 MAC then validates on write that it is consistent with the principal.

### 7.4 Write validation for relational classifications

A dependent may only reference a principal the caller is cleared to see. This is checked via the MAC-protected principal repository
and therefore also works across repository boundaries.

## 8. Configuration matrix (G10)

| Setting | Scope | Options | Secure default |
|---|---|---|---|
| Empty clearance | dimension | deny all / allow all | deny all |
| `null` classification | dimension, entity | deny / allow / treat as value | deny |
| Missing optional navigation | relation | deny / allow | deny |
| Keyed read violation | entity | throw / return null | throw |
| Write violation | entity | throw / skip | throw |
| Lookups as classification source | relation | enabled / disabled | open question Q1 |
| Execution strategy | relation | push-down / semi-join / denormalized | push-down if supported, else semi-join |
| Max relation depth | global | number | 3 |

## 9. Open questions

- **Q1** Should lookups (e.g. a classified `Country`) restrict entities referencing them, or only principals? (undecided)
- **Q2** How are multiple relation paths to the same dimension combined (AND vs. OR)?
- **Q3** Exception type and transport via UJMW (fault contract / error code).
- **Q4** Should EF Core global query filters (`HasQueryFilter`) be offered as an additional push-down target?
- **Q5** Migration path for existing consumers of `KornSW.EntityFrameworkCore.DataAccessFilter`.

## 10. Acceptance criteria (draft)

- Deny values work for all supported types (regression test for W1).
- Parallel requests with different clearances never receive foreign data (concurrency test for W2).
- Generated SQL is identical for different clearance sets of equal size (parameterization test for W4).
- Cyclic models do not cause endless recursion (W5).
- Every `IRepository` method is covered by MAC tests against `InMemoryRepository` and `EfRepository`.
- A composite setup (EF dependent + in-memory/web-client principal) enforces relational classification via semi-join.

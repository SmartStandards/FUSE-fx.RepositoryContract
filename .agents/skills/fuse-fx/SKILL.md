---
name: fuse-fx
description: Desired-state standard for implementing and consuming FUSE-fx repositories (IRepository<TEntity, TKey>) and typed Stores/DataStores. Use when building data access over databases, EF Core, in-memory, files, foreign APIs or SDKs, when generating non-.NET consumers, or when accessing repositories remotely via UJMW.
---

# FUSE-fx Repository & Store Skill

## Desired-State Standard for AI Agents

**Version:** 1.0\
**Status:** Initial normative baseline\
**Primary scope:** `IRepository<TEntity, TKey>` and typed
Store/DataStore access\
**Secondary scope:** implementation guidance, dynamic stores, foreign
APIs/SDKs, cross-language consumption, and UJMW-based remote access

------------------------------------------------------------------------

## 1. Purpose and Scope

This document defines the desired state for implementing and consuming
FUSE-fx repositories.

It is intended to be sufficiently explicit for an AI agent to:

1.  implement a concrete and complete `IRepository<TEntity, TKey>` for a
    given data source;
2.  generate idiomatic consumer code for FUSE-fx repositories;
3.  generate helper models and access layers for non-.NET consumers
    where native FUSE-fx libraries are not available;
4.  use a remotely exposed repository through UJMW when the repository
    schema, operation contract, and endpoint URL are known and the agent
    has HTTP access.

The central architectural goal is not merely to abstract databases.
FUSE-fx provides a standardized, flat, entity-oriented data-access
facade over heterogeneous sources. Those sources may include relational
databases, Entity Framework Core models, in-memory collections, JSON
files, foreign Web APIs, REST-like services, or vendor SDKs.

A consumer should be able to work with the same repository semantics
without needing to understand the implementation technology behind the
repository.

### 1.1 Normative core

The preferred and normative architecture is:

-   `IRepository<TEntity, TKey>` as the strongly typed repository
    contract;
-   a typed Store/DataStore as the preferred single entry point for
    multiple entity types belonging to one model.

`IRepository<TEntity, TKey>` is the **gold standard**. New repository
implementations and new consumer code should prefer this contract unless
a concrete use case makes that impossible.

### 1.2 Non-preferred experimental artifacts

FUSE-fx also contains experimental or historically evolved abstractions
such as:

-   Universal Repositories;
-   dictionary/untyped repositories;
-   untyping facades;
-   combinations of universal and dictionary-based access.

These artifacts originated from legitimate dynamic WebUI and
remote-access use cases, but they contain design debt and are **not**
the default desired state.

An AI agent MUST NOT infer that an abstraction is recommended merely
because it exists in the FUSE-fx source tree.

Such artifacts may be:

-   recognized;
-   explained;
-   maintained when an existing system depends on them;
-   used when their specific dynamic scenario genuinely requires them.

They MUST NOT be recommended as the default architecture for new code
without an explicit reason.

------------------------------------------------------------------------

## 2. Normative Sources

Before implementing a repository, an agent MUST read the current
versions of the following source files. The source files are
authoritative; this skill document explains intent and invariants but
does not replace the live contract.

### 2.1 Repository contract

-   `https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs`
-   `https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/Logic/ExpressionTree.cs`
-   `https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/Logic/FieldOperators.cs`
-   `https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/Logic/FieldPredicate.cs`
-   `https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/RepositoryCapabilities.cs`

The current `IRepository<TEntity, TKey>` contract MUST be implemented
completely. An agent MUST NOT silently omit inconvenient operations,
replace the contract with a smaller locally invented interface, or infer
old signatures from examples.

### 2.2 Dynamic schema support

For dynamic stores, the agent MUST additionally read:

-   `https://raw.githubusercontent.com/SmartStandards/FUSE-fx.ModelReader/refs/heads/master/dotnet/src/ModelReader/ModelReader.cs`
-   `https://raw.githubusercontent.com/SmartStandards/FUSE-fx.ModelDescription/refs/heads/master/dotnet/src/ModelDescription/SchemaRoot.cs`
-   `https://raw.githubusercontent.com/SmartStandards/FUSE-fx.ModelDescription/refs/heads/master/dotnet/src/ModelDescription/EntitySchema.cs`
-   `https://raw.githubusercontent.com/SmartStandards/FUSE-fx.ModelDescription/refs/heads/master/dotnet/src/ModelDescription/RelationSchema.cs`
-   `https://raw.githubusercontent.com/SmartStandards/FUSE-fx.ModelDescription/refs/heads/master/dotnet/src/ModelDescription/IndexSchema.cs`
-   `https://raw.githubusercontent.com/SmartStandards/FUSE-fx.ModelDescription/refs/heads/master/dotnet/src/ModelDescription/FieldSchema.cs`

The `ModelReader` may be used internally and on demand to derive an
`EntitySchema` for a supplied entity type.

------------------------------------------------------------------------

## 3. Architectural Intent

### 3.1 Repository as a standardization facade

A FUSE-fx repository is a standardized facade over a set of entities.

The underlying source may be:

-   Entity Framework Core;
-   ADO.NET / SQL;
-   in-memory objects;
-   JSON or another file-backed representation;
-   a foreign REST-like Web API;
-   a remote RPC API;
-   a vendor SDK;
-   another persistence or service technology.

The facade should hide unnecessary backend-specific details while
preserving the source's meaningful semantics.

This is particularly important when wrapping foreign platforms. A vendor
may expose deeply nested resources, unusual identifiers, proprietary SDK
objects, or operation-specific endpoints. The FUSE-fx adapter may map
those concepts to a clean local entity model and expose a flat
repository surface.

The consumer should not need to know whether `Customer` originates from
SQL, an EF Core model, an HTTP endpoint, or a vendor SDK.

### 3.2 Why flat entity access matters

The standardized flat model enables generic consumers.

A generic WebUI can, for example:

-   display an entity set as a table;
-   page through records;
-   sort by selected fields;
-   apply structured filters;
-   provide a global search field;
-   load only selected columns;
-   populate lookup controls;
-   edit entities through standardized mutation operations.

The repository contract is therefore not merely a CRUD abstraction. Its
query shapes deliberately support generic data-driven applications.

------------------------------------------------------------------------

## 4. Preferred Architecture

The preferred conceptual architecture is:

``` text
Application / Generic UI / Agent
             |
             v
        Typed Store
             |
             v
 IRepository<TEntity, TKey>
             |
             v
       Backend Adapter
             |
   +---------+---------+---------+---------+
   |         |         |         |         |
 EF Core   ADO.NET   InMemory   JSON    API / SDK
```

A typed repository remains the semantic center.

### 4.1 Typed Store

A Store provides one typed entry point to the repositories belonging to
a model. Its role is comparable to the discoverability and aggregation
role of an EF `DbContext`, but this analogy MUST NOT be extended to
transaction lifetime or unit-of-work semantics.

A Store:

-   groups access to multiple entity repositories;
-   exposes those repositories in a strongly typed way;
-   may know the managed model;
-   may hold or receive technical connection/provider configuration;
-   does not represent the lifetime of a transaction.

The preferred public surface should remain explicit and strongly typed.

### 4.2 AccessContext generation

When generating an application-specific access layer for a statically
known model, generate a higher-level `...AccessContext` class with one
concretely typed repository property per entity.

Example:

``` csharp
public class SalesAccessContext {

  private readonly IRepository<Customer, long> _Customers;
  private readonly IRepository<Order, long> _Orders;

  public SalesAccessContext(string connectionString) {
    _Customers = new CustomerRepository(connectionString);
    _Orders = new OrderRepository(connectionString);
  }

  public IRepository<Customer, long> Customers {
    get {
      return _Customers;
    }
  }

  public IRepository<Order, long> Orders {
    get {
      return _Orders;
    }
  }

}
```

The exact construction strategy depends on the backend and MUST be
clarified before generation.

### 4.3 Dynamic Store

If the set of entities is supplied by the consumer at runtime, do not
generate one property per entity.

Use a generic entry point conceptually equivalent to:

``` csharp
public IRepository<TEntity, TKey> Repo<TEntity, TKey>()
  where TEntity : class {
  // Resolve schema and return the generic repository implementation.
}
```

The internal repository implementation should exist only once in
generic/reflection-based form.

The ModelReader may resolve metadata on demand. Reflection metadata and
derived schema information SHOULD be cached where safe because repeated
reflection in hot query or materialization paths is unnecessarily
expensive.

Dynamic entity selection does **not** automatically imply an untyped
dictionary model. If `TEntity` and `TKey` are available, retain the
strongly typed repository contract.

------------------------------------------------------------------------

## 5. Repository Contract Semantics

### 5.1 Completeness is mandatory

A concrete implementation targets the complete current
`IRepository<TEntity, TKey>` interface.

This includes all contract areas, including:

-   origin identity;
-   capabilities;
-   entity references;
-   entity loading;
-   key-based loading;
-   field projection;
-   counts;
-   key existence;
-   add/update operations;
-   partial field updates;
-   mass updates;
-   deletion;
-   key updates;
-   structured filtering;
-   string-based search where supported;
-   sorting;
-   paging.

A repository is not considered complete merely because common CRUD
operations work.

If an operation cannot be implemented correctly, the method still exists
because it is part of the interface. The corresponding capability must
describe the actual support and the operation should throw
`NotSupportedException` where appropriate.

However, lack of a native backend primitive is not sufficient reason to
declare an operation unsupported. Correct emulation should be considered
first.

### 5.2 Semantic fidelity before convenience

The adapter MUST preserve the observable semantics of the repository
contract.

Do not simplify an operation merely because a backend exposes a
superficially similar primitive.

Examples:

-   do not apply paging before a required local post-filter;
-   do not lose requested ordering when resolving multiple keys;
-   do not silently omit missing-key positions when the contract
    requires positional correspondence;
-   do not convert a partial update into a destructive full replacement;
-   do not overwrite database-generated or protected values
    accidentally.

------------------------------------------------------------------------

## 6. Query Semantics

### 6.1 ExpressionTree

`ExpressionTree`, `FieldPredicate`, and `FieldOperators` form the
structured filtering language.

The current source definitions MUST be read before implementation.

An implementation must account for:

-   predicates;
-   nested/sub trees;
-   `MatchAll`;
-   `Negate`;
-   field operators and their type-dependent meaning.

A particularly important semantic rule is that multiple predicates
concerning the same field can require grouping behavior that is not
equivalent to naïvely AND-ing every predicate in sequence. An agent MUST
preserve the exact current `ExpressionTree` semantics rather than
translating the structure mechanically.

### 6.2 FieldOperators

`FieldOperators` is string-based and MUST NOT be replaced by a locally
invented enum unless a private internal translation layer preserves the
exact public contract.

Some textual operator representations may have meaning that depends on
the field type. The current source is authoritative.

### 6.3 Sorting

Sorting is applied before paging.

The current contract convention for descending sort markers MUST be
preserved.

An implementation should translate sorting to the backend whenever
possible.

### 6.4 Paging

`limit` and `skip` are intentional parts of the repository contract.

They are especially important for generic UIs, remote sources, and large
datasets.

The preferred order is conceptually:

``` text
filter/search
    -> sort
    -> page
    -> materialize/project result
```

If part of the filter must be evaluated locally, the implementation must
ensure that paging still applies to the semantically correct result set.

Incorrect:

``` text
backend page -> local filter
```

when the local filter can remove records and therefore alter page
boundaries.

Correctness takes precedence over avoiding additional work.

### 6.5 String-based search expressions

The `...BySearchExpression` operations represent a standardized
free-form search entry point.

A primary use case is the **global search field of a generic table or
WebUI**.

The exact interpretation may depend on the backend implementation. The
capability must truthfully indicate support.

When wrapping a foreign API, its native global search facility may be an
excellent implementation target. If no native search exists, a correct
emulation may be considered.

------------------------------------------------------------------------

## 7. EntityRef: A First-Class Generic-UI Primitive

`EntityRef<TKey>` is not merely a convenience DTO.

It exists to retrieve a lightweight representation of an entity suitable
for identifying and displaying it without loading the full entity.

Conceptually it represents:

``` text
Key + human-readable Label
```

This is particularly valuable for:

-   comboboxes;
-   dropdowns;
-   autocomplete controls;
-   lookup dialogs;
-   relation editors;
-   foreign-key selectors;
-   generic navigation UIs.

Consider a `Customer` entity containing many columns, large text fields,
audit information, or expensive mapped data. A UI that merely needs:

``` text
4711 -> "Acme GmbH"
```

should not have to materialize the entire customer object.

`GetEntityRefs*` therefore has architectural and performance
significance.

Repository implementations SHOULD implement EntityRef retrieval as
efficiently as their backend permits. Loading complete entities and
discarding almost every field may be functionally correct, but it
defeats much of the purpose and should be treated as an optimization
target rather than an ideal implementation.

------------------------------------------------------------------------

## 8. Field Projection

`GetEntityFields*` supports lightweight access to a requested subset of
fields.

This is another important primitive for generic UIs and remote data
access.

A repository should push projection into the backend when practical:

-   SQL should select only required columns;
-   EF Core should project only required properties where safely
    possible;
-   a foreign API should request field subsets when the API supports it;
-   an in-memory repository may project after filtering because the data
    is already local.

Do not assume that returning full entities and converting them
afterwards is an equally desirable implementation merely because the
observable values are correct.

------------------------------------------------------------------------

## 9. Key Semantics

### 9.1 Single keys

For a single-field key, `TKey` should normally be the natural type of
that key.

Example:

``` csharp
IRepository<Customer, long>
```

### 9.2 Composite keys

An implementation MUST NOT assume that `TKey` is primitive.

Composite keys are a supported requirement.

Current transitional FUSE-fx helper types include generic composite-key
representations such as `CompositeKey2<...>` through
`CompositeKey5<...>`. Tuples can also technically represent multiple key
values.

These mechanisms are transitional and should not define the long-term
desired model.

### 9.3 Desired-state composite key model

The preferred future model is a semantically named key class containing
the original key properties. The entity derives from that key class.

Example:

``` csharp
public class OrderLineKey {

  public long OrderId { get; set; }

  public int LineNumber { get; set; }

}

public class OrderLine : OrderLineKey {

  public string ArticleNumber { get; set; }

  public decimal Quantity { get; set; }

}
```

The repository can then be expressed as:

``` csharp
IRepository<OrderLine, OrderLineKey>
```

This has several advantages:

-   the key has meaningful property names;
-   `TKey` is strongly typed;
-   the entity itself still directly exposes the original key properties
    through inheritance;
-   EF Core can map those properties normally;
-   generic repository code can treat the key as a structured value
    without losing domain meaning.

Until this desired state is universally available, existing FUSE
composite-key helpers may be used where required.

### 9.4 Schema-driven key discovery

For dynamic implementations, derive key information from FUSE model
metadata.

Do not guess keys from conventions such as:

-   `Id`;
-   `<EntityName>Id`;
-   the first property;
-   the first numeric field.

Composite primary keys and explicit schema metadata make such heuristics
unsafe.

------------------------------------------------------------------------

## 10. Mutation Semantics and Data Integrity

Data integrity has higher priority than convenience and higher priority
than maximizing apparent capability support.

### 10.1 Returned values matter

Mutation results MUST be evaluated and propagated correctly.

The returned state can differ from the submitted state because:

-   a value was rejected or not updated;
-   a value was normalized;
-   the backend generated a key;
-   a timestamp changed;
-   a row-version/concurrency field changed;
-   another backend-generated field changed.

Consumer code MUST NOT blindly assume that the submitted object is the
final persisted state.

### 10.2 Partial updates

Dictionary/field-based update operations represent a deliberate
partial-update concept.

Only intended fields should be changed.

Key fields, generated identities, auto values, and non-settable fields
require special care according to schema and repository semantics.

### 10.3 Add versus update

The current contract and capabilities determine how externally supplied
keys and generated keys are handled.

An implementation MUST inspect and preserve those semantics rather than
imposing generic ORM assumptions.

### 10.4 Mass updates

Mass updates should preferably execute at the backend.

For EF Core and SQL in particular, avoid materializing every affected
entity merely to update it if the current platform offers a safe
server-side update mechanism.

If server-side execution is unavailable, a correct iterative fallback
may be acceptable, but the performance implications should be
understood.

### 10.5 Key updates

Changing a primary key can be dangerous and backend-dependent.

Implement it only when semantics are correct and data integrity can be
preserved. Otherwise report the capability accurately and throw
`NotSupportedException`.

Do not emulate a key update through delete-and-reinsert unless all
relevant semantics, relations, generated values, and integrity effects
are understood and preserved.

------------------------------------------------------------------------

## 11. Capability Strategy

`RepositoryCapabilities` describes what a repository can actually
provide.

Capabilities MUST be derived from the finished behavior of the adapter,
not from an aspirational feature list.

The preferred decision sequence is:

1.  use a native backend feature;
2.  if unavailable, push as much work as safely possible to the backend;
3.  complete the operation locally while preserving exact repository
    semantics;
4.  accept a performance cost when required for robust correctness;
5.  use `NotSupportedException` only when a correct implementation is
    genuinely unavailable or unreasonable.

A capability can therefore be functionally supported even when the
backend does not provide a one-to-one native primitive.

Do not claim support when an emulation changes observable semantics.

------------------------------------------------------------------------

## 12. Backend Implementation Profiles

The repository contract is backend-agnostic. The following profiles
provide implementation priorities rather than alternative contracts.

### 12.1 Entity Framework Core

For C#/.NET persistence, EF Core is the preferred state-of-the-art
implementation profile unless a use case requires lower-level SQL
access.

Important principles:

-   retain `IRepository<TEntity, TKey>` as the public contract;
-   use `IQueryable<TEntity>` to push filters, sorting, paging, counts,
    and projection to the database;
-   use `AsNoTracking()` for read-only materialization where
    appropriate;
-   avoid client-side enumeration before filtering/paging;
-   use EF Core's current server-side bulk capabilities where
    appropriate;
-   preserve database-generated identities and auto-generated values;
-   do not let repository lifetime imply DbContext transaction lifetime;
-   make DbContext acquisition/lifetime explicit through an appropriate
    provider/factory strategy.

Small conceptual example:

``` csharp
IQueryable<TEntity> query = dbContext.Set<TEntity>().AsNoTracking();

if (filter != null) {
  Expression<Func<TEntity, bool>> predicate =
    ExpressionTreeMapper.BuildLinqExpressionFromTree<TEntity>(
      filter,
      false
    );

  query = query.Where(predicate);
}

query = query.ApplySorting(sortedBy);
query = query.ApplyPaging(limit, skip);

return query.ToArray();
```

This is illustrative only. The current FUSE helper APIs and contract
sources remain authoritative.

### 12.2 ADO.NET / SQL

ADO.NET is the preferred direct-SQL profile for C# when an ORM is not
appropriate.

A robust implementation should:

-   parameterize all values;
-   translate supported predicates to SQL;
-   quote identifiers correctly for the target database;
-   push counts, sorting, paging, and projection to SQL;
-   map database values explicitly;
-   handle generated keys correctly;
-   use deterministic ordering when paging requires it;
-   avoid SQL string concatenation of user-controlled values.

Conceptually:

``` csharp
using (IDbCommand command = connection.CreateCommand()) {
  command.CommandText = sql;

  IDbDataParameter parameter = command.CreateParameter();
  parameter.ParameterName = "@CustomerId";
  parameter.Value = customerId;
  command.Parameters.Add(parameter);

  // Execute and map explicitly.
}
```

The exact SQL dialect and connection lifecycle MUST be clarified before
implementation.

### 12.3 In-memory

An in-memory implementation is valuable for:

-   tests;
-   small local datasets;
-   deterministic development scenarios;
-   reference behavior.

Because data is already local, LINQ-based filtering and projection are
natural.

String comparison semantics MUST be explicit. In-memory `.NET` string
behavior may differ from a typical case-insensitive SQL collation. An
adapter must not accidentally change observable search/filter behavior
without documenting or configuring the difference.

### 12.4 JSON files

JSON-backed repositories are a planned/relevant profile but this version
does not prescribe a complete file-store architecture.

Before implementing such a repository, clarify at least:

-   one file per entity set versus one file per entity;
-   file locking;
-   atomic writes;
-   crash safety;
-   concurrent writers;
-   indexing;
-   key generation;
-   maximum expected dataset size;
-   backup/recovery behavior;
-   encoding;
-   schema evolution.

Data-loss prevention is more important than implementing every operation
cheaply.

### 12.5 Foreign Web APIs and SDKs

Wrapping foreign platforms is a first-class FUSE-fx use case.

The repository may map between an external representation and a clean
local entity.

Example:

``` csharp
public class Customer {

  public long CustomerId { get; set; }

  public string DisplayName { get; set; }

}

internal class ExternalCustomerDto {

  public string ExternalId { get; set; }

  public string LegalName { get; set; }

}
```

The adapter may translate:

``` text
ExternalId <-> CustomerId
LegalName  <-> DisplayName
```

provided the mapping is stable, reversible where required, and does not
hide data-loss risks.

The external API's native:

-   filtering;
-   search;
-   pagination;
-   field selection;
-   update semantics;
-   rate limits;
-   continuation tokens;
-   concurrency tokens

should be understood before implementing the facade.

A vendor SDK is not fundamentally different: it is another backend
access mechanism behind the FUSE contract.

------------------------------------------------------------------------

## 13. Dynamic and Untyped Artifacts

### 13.1 Universal Repository

Existing FUSE-fx Universal Repository implementations route operations
to an entity-specific inner repository by accepting an additional entity
identifier/name.

Conceptually:

``` text
GetEntities(entityName, filter, ...)
```

instead of:

``` text
repositoryForCustomer.GetEntities(filter, ...)
```

This enables a single endpoint to route to arbitrary entity sets.

It can be useful for highly dynamic remote clients, but it weakens the
strongly typed contract surface and introduces routing, resolution,
caching, and runtime validation concerns.

It is therefore a **specialized compatibility/dynamic pattern**, not the
preferred architecture.

### 13.2 Dictionary repositories

Dictionary repositories expose entities as property bags such as:

``` csharp
Dictionary<string, object>
```

They can support clients that do not possess compile-time entity types.

Existing adapters can convert between dictionary models and strongly
typed repositories.

This can be useful for schema-driven WebUIs but sacrifices compile-time
type safety and creates additional conversion/error-handling
requirements.

Prefer typed entities whenever the entity type is available.

### 13.3 Combined permutations

Universal routing and dictionary entities can be combined.

Such combinations solve real dynamic scenarios but compound the
disadvantages:

-   less compile-time validation;
-   more reflection/conversion;
-   more runtime failure modes;
-   weaker discoverability;
-   harder refactoring;
-   more complicated key handling.

An agent should maintain these forms when required by an existing
architecture, but should not introduce them casually into new designs.

------------------------------------------------------------------------

## 14. Store and Lifetime Semantics

The Store is the preferred aggregation concept above repositories.

It is essential not to confuse:

-   Store lifetime;
-   repository object lifetime;
-   EF DbContext lifetime;
-   SQL connection lifetime;
-   transaction lifetime.

These are separate concerns.

Existing FUSE-fx code contains short-living, long-living, and
delegate-based instance-provider experiments for EF contexts and SQL
connections. They provide useful implementation experience, but their
existence does not define the desired transaction model.

### 14.1 Transactions

Transaction management is intentionally kept small in this version.

The intended architectural direction is **ambient transaction
management**.

Therefore:

-   do not design the Store as a transaction object;
-   do not introduce `SaveChanges()`-style unit-of-work semantics into
    the Store merely because EF uses them internally;
-   preserve backend transaction behavior where necessary;
-   do not invent a broad FUSE transaction API from incomplete
    historical artifacts.

This area is expected to be refined incrementally in later versions.

------------------------------------------------------------------------

## 15. Remote Exposure and UJMW

FUSE-fx repository semantics and UJMW transport semantics are separate
concerns.

A repository may be local or remote.

When a FUSE repository is exposed through UJMW:

-   FUSE defines the repository operations, argument structures,
    entities, keys, filters, and result semantics;
-   UJMW defines how the typed operation is represented as an HTTP/RPC
    call.

This document intentionally does not duplicate the UJMW wire protocol.

Agents MUST use the separate **UJMW skill/specification** when
generating or executing UJMW transport calls.

Do not reinterpret FUSE operations as REST CRUD merely because HTTP is
used.

------------------------------------------------------------------------

## 16. Agent Capability A: Implement a Repository

When asked to implement a repository, the agent MUST first determine the
properties of the concrete backend.

Do not start generating the final implementation while semantically
relevant backend questions remain unanswered.

The investigation should cover, as applicable:

  Area                  Required understanding
  --------------------- -------------------------------------------------------
  Backend               EF Core, SQL, memory, file, HTTP API, SDK, other
  Entity set            Static model or runtime-supplied entity types
  Schema                Fields, types, required values, setability
  Keys                  Single/composite, generated/external, updateable
  Unique constraints    Alternate/unique key sets
  Reads                 Native filters, key lookup, projection
  Search                Native global/free-form search semantics
  Sorting               Supported fields and ordering rules
  Paging                Offset/limit, page number, cursor, continuation token
  Counts                Native count support and filtering
  Writes                Create, full update, partial update/upsert behavior
  Generated values      Identities, timestamps, normalized values
  Mass update           Native bulk operation or fallback
  Delete                Semantics, soft delete, restrictions
  Key update            Supported and safe or not
  Concurrency           RowVersion, ETag, optimistic concurrency, other
  Connection/lifetime   Factory, pooling, ownership, disposal
  Error behavior        Backend exceptions and expected failure modes
  Performance           Expected volume, remote latency, rate limits
  Mapping               External DTO/entity transformations
  Capabilities          Derived only after behavior is understood

The agent should continue asking targeted questions until these points
are sufficiently clear for the requested backend.

### 16.1 Implementation priority

For each repository operation:

``` text
native backend implementation
        |
        v
safe backend-side reduction
        |
        v
correct local completion
        |
        v
NotSupportedException as last resort
```

Correctness and data integrity outrank performance, but unnecessary
performance loss should be avoided.

------------------------------------------------------------------------

## 17. Agent Capability B: Generate Repository Consumer Code

An agent must also be able to generate code that consumes an existing
repository.

### 17.1 .NET

In .NET, prefer the actual FUSE-fx libraries and strongly typed
contracts.

Example:

``` csharp
IRepository<Customer, long> customers = accessContext.Customers;

Customer[] result = customers.GetEntities(
  filter,
  new string[] { "DisplayName" },
  100,
  0
);
```

The exact current method signatures must come from the live contract.

### 17.2 Non-.NET languages

For another programming language, an agent may generate:

-   FUSE data models;
-   `ExpressionTree` helpers/builders;
-   `FieldPredicate` models;
-   operator constants;
-   key models;
-   EntityRef models;
-   repository client abstractions;
-   serialization/deserialization helpers;
-   HTTP transport glue.

The generated helpers MUST preserve FUSE semantics rather than replacing
them with idioms that change the contract.

If the remote transport is UJMW, consult the separate UJMW skill for the
HTTP representation.

------------------------------------------------------------------------

## 18. Agent Capability C: Directly Access a Remote Repository

If an agent itself has HTTP access, it may directly use a remotely
exposed FUSE repository when all required information is available.

Required inputs include:

-   the UJMW base URL;
-   the exact repository operation to call;
-   the entity schema;
-   the key schema;
-   the current `IRepository` contract;
-   the separate UJMW transport specification.

The reasoning sequence is:

``` text
Understand FUSE entity/schema
        |
        v
Choose IRepository operation
        |
        v
Construct FUSE arguments
        |
        v
Apply UJMW transport specification
        |
        v
HTTP POST
        |
        v
Interpret FUSE result
```

The agent MUST NOT guess unknown schema fields, key shapes, operation
names, or transport details.

------------------------------------------------------------------------

## 19. Coding Standard for Generated C# Code

The following conventions are normative for code generated under this
skill.

### 19.1 Language and communication

-   Prompt/user communication is German by default.
-   Source code is English.
-   Code comments are English.
-   XML documentation is English.

### 19.2 Explicitness

Prefer clean, explicit, compiler-friendly C#.

Avoid unnecessary syntactic sugar.

Specifically:

-   do not use `var`;
-   avoid nullable annotations/operators when they are not necessary;
-   avoid the null-coalescing operator when an explicit construct is
    clearer or required by the surrounding style;
-   do not use inline conditional (`?:`) expressions;
-   do not use the `async`/`await` pattern;
-   properties should be fully implemented rather than
    expression-bodied;
-   explicitly use `this.` when accessing instance members other than
    fields;
-   lambdas always use parenthesized parameters;
-   one-line control blocks still use braces.

### 19.3 Naming

Constants and fields use PascalCase with `_` prefix.

Example:

``` csharp
private readonly string _ConnectionString;
private const int _DefaultLimit = 500;
```

### 19.4 Braces and continuation keywords

Opening braces remain on the same line:

``` csharp
if (condition) {
  DoSomething();
} else {
  DoSomethingElse();
}
```

`else`, `else if`, `catch`, and `finally` follow the closing brace
without an intervening line break.

### 19.5 Collections in signatures

When collections cross method/API signatures, prefer arrays over
`List<T>` unless the contract or a concrete technical requirement
dictates otherwise.

### 19.6 Documentation

Methods should have `<summary>` XML documentation.

Comments should explain non-obvious intent, invariants, backend
compromises, and safety considerations rather than restating syntax.

### 19.7 JSON

Use `Newtonsoft.Json` instead of `System.Text.Json` in newly generated
code unless an external contract or existing codebase explicitly
requires otherwise.

### 19.8 Logging and exceptions

Use targeted error handling.

Expected exceptions that are deliberately caught should be logged with:

``` csharp
DevLogger.LogError(ex);
```

Do not generate pro-forma `catch` blocks around arbitrary code.

Do not silently swallow exceptions.

Trace output uses:

``` csharp
DevLogger.LogTrace(0, 99999, "Trace message");
```

The numeric values may remain `0` and `99999`.

### 19.9 Platform compatibility

Avoid platform-specific Interop and `DllImport`.

If a Windows-only interop solution appears unavoidable, obtain explicit
user approval before generating it.

### 19.10 Compatibility and maintainability

Prefer:

-   strong static typing;
-   explicit behavior;
-   predictable control flow;
-   compiler-verifiable constructs;
-   long-term maintainability;
-   broad runtime/platform compatibility.

Do not introduce newer C# syntax merely because it is shorter.

------------------------------------------------------------------------

## 20. Common Failure Modes

An agent implementing or consuming FUSE-fx MUST actively avoid the
following mistakes.

### 20.1 Contract failures

-   implementing only CRUD;
-   omitting less obvious interface methods;
-   using stale remembered signatures instead of reading the current
    contract;
-   inventing a substitute repository interface;
-   misreporting capabilities.

### 20.2 Query failures

-   naïvely translating `ExpressionTree`;
-   treating `FieldOperators` as an enum;
-   ignoring type-dependent operator semantics;
-   applying paging before required filtering;
-   losing sort semantics;
-   materializing large datasets unnecessarily.

### 20.3 Key failures

-   assuming every key is `Id`;
-   assuming every key is scalar;
-   ignoring composite keys;
-   reordering key-based result arrays when positional semantics matter;
-   dropping missing-key placeholders where the contract requires them.

### 20.4 Mutation failures

-   overwriting database-generated fields;
-   treating partial updates as full replacements;
-   ignoring normalized/generated returned values;
-   mutating key fields through ordinary mass-update operations;
-   implementing unsafe delete/reinsert key changes;
-   swallowing write exceptions.

### 20.5 Architecture failures

-   treating EF Core as the definition of FUSE;
-   treating a Store as a transaction lifetime;
-   exposing backend-specific SDK concepts unnecessarily;
-   selecting dictionary repositories merely because the UI is dynamic;
-   selecting Universal Repository merely because multiple entities
    exist;
-   treating experimental FUSE artifacts as normative architecture;
-   mixing UJMW transport semantics into the FUSE repository contract.

### 20.6 Performance failures

-   loading full entities for an EntityRef query without necessity;
-   loading full entities for field projection;
-   performing `Count` by materializing records;
-   performing `ContainsKey` by loading complete entities;
-   repeating reflection/property discovery in hot loops;
-   issuing one remote request per entity when batching is available;
-   implementing EF bulk updates as iterative tracked updates when safe
    server-side operations are available.

------------------------------------------------------------------------

## 21. Desired-State Examples

### 21.1 Generic table

A generic table needs:

``` text
Entity schema
+ GetEntityFields(...)
+ sorting
+ limit/skip
+ Count(...)
```

It can render a paged dataset without knowing the backend.

### 21.2 Global search

A generic UI search box can use:

``` text
GetEntityFieldsBySearchExpression(...)
CountBySearchExpression(...)
```

where the repository advertises string-based search support.

The backend may implement this using:

-   database text search;
-   SQL predicates;
-   an external API's search endpoint;
-   SDK search facilities;
-   a correct local fallback.

### 21.3 Lookup control

A relation editor should prefer:

``` text
GetEntityRefs(...)
```

rather than loading full entities.

The UI needs only:

``` text
Key + Label
```

for each selectable record.

### 21.4 Foreign platform facade

A vendor SDK might expose:

``` text
SdkClient.Accounts.Query(...)
```

with vendor-specific DTOs.

The FUSE adapter can expose:

``` text
IRepository<Customer, long>
```

and isolate:

-   SDK calls;
-   pagination tokens;
-   DTO conversion;
-   external identifiers;
-   vendor exceptions;
-   search translation.

Generic application code remains FUSE-oriented.

------------------------------------------------------------------------

## 22. Testing Guidance

After generating a repository implementation, the agent MUST actively
ask whether unit tests should also be generated.

Tests should focus on contract semantics rather than only implementation
details.

High-value test areas include:

-   every `IRepository` method;
-   empty results;
-   missing keys;
-   ordering of key-based results;
-   paging boundaries;
-   sorting before paging;
-   nested ExpressionTrees;
-   same-field predicate grouping;
-   negation;
-   every supported FieldOperator;
-   case sensitivity behavior;
-   field projection;
-   EntityRef labels and keys;
-   generated keys;
-   partial updates;
-   normalized/generated returned values;
-   mass updates;
-   delete behavior;
-   key update behavior;
-   composite keys;
-   capability consistency;
-   external API mapping;
-   backend failures.

For adapters with a native query engine, include tests that detect
accidental client-side semantic changes.

------------------------------------------------------------------------

## 23. Optional Implementation Documentation

After generating a concrete repository implementation, the agent MUST
also actively ask whether a Markdown implementation document should be
generated.

If requested, produce one large continuous English Markdown file
containing:

1.  introduction / abstract / motivation;
2.  positive differentiation from competing or backend-native access
    approaches;
3.  top-down entry examples;
4.  advanced examples showing full complexity and explaining the use
    case;
5.  a tabular specification of all underlying requirements;
6.  bottom-up documentation of the important artifacts.

This implementation-specific documentation is separate from this skill
document.

------------------------------------------------------------------------

## 24. Migration and Adoption Guidance

The desired state is primary. Migration is secondary.

When adapting an existing DAO, repository, SDK wrapper, or service
client to FUSE-fx:

1.  inventory the existing operations and semantics;
2.  identify the actual entity model and keys;
3.  read the current FUSE-fx contract;
4.  map existing capabilities to FUSE operations;
5.  identify semantic gaps;
6.  implement native translations where possible;
7.  add safe emulation where required;
8.  preserve data integrity;
9.  derive truthful `RepositoryCapabilities`;
10. add the typed Store/AccessContext surface;
11. test the complete contract;
12. only then remove or deprecate redundant legacy access paths if
    requested.

Do not mechanically wrap method names. Normalize the data-access
semantics.

For existing Universal/Dictionary-based FUSE code, do not migrate merely
for aesthetic reasons. First determine why the dynamic form exists. If
compile-time entity types are available, consider moving new consumer
code toward typed repositories/Stores while retaining compatibility
where required.

------------------------------------------------------------------------

## 25. Decision Hierarchy for AI Agents

When uncertain, apply the following order:

1.  **Current FUSE-fx source contract**
2.  **Data integrity and exact observable semantics**
3.  **Strongly typed `IRepository<TEntity, TKey>`**
4.  **Typed Store for multi-entity model access**
5.  **Native backend execution**
6.  **Performance-conscious correct emulation**
7.  **Dynamic/untyped compatibility mechanisms only when justified**
8.  **`NotSupportedException` when correct support is genuinely
    unavailable**

Never reverse this hierarchy merely to produce shorter code.

------------------------------------------------------------------------

## 26. Versioning and Maintenance of This Skill

This document is maintained incrementally under version control.

The structure established in Version 1 should remain stable unless a
structural change is genuinely necessary.

Future revisions should:

-   modify the smallest relevant section;
-   preserve section numbering where practical;
-   avoid rewriting unaffected prose;
-   add new knowledge without regenerating the entire document;
-   distinguish desired state from legacy/experimental observations;
-   update normative source references when contracts move;
-   avoid copying large portions of source contracts into this document;
-   keep examples small and canonical.

The goal is a useful Git diff between versions.

### 26.1 Evolution rule

A newly discovered implementation in the FUSE-fx codebase is
**evidence**, not automatically a standard.

Before promoting a pattern to desired state, evaluate:

-   whether it preserves strong typing;
-   whether it reduces or adds semantic ambiguity;
-   whether it improves generic consumption;
-   whether it is backend-independent;
-   whether it introduces lifecycle coupling;
-   whether it carries historical design debt;
-   whether a simpler Store + typed Repository architecture already
    solves the problem.

------------------------------------------------------------------------

## 27. Summary of Normative Position

FUSE-fx should be approached as a standardized entity-data access model.

The strongly typed:

``` csharp
IRepository<TEntity, TKey>
```

is the gold-standard repository abstraction.

A typed Store is the preferred aggregation point for accessing multiple
repositories belonging to a model, without representing transaction
lifetime.

The repository contract intentionally contains query forms such as
paging, global search, projections, and EntityRefs because FUSE-fx is
intended to support generic consumers, including generic WebUIs,
efficiently and consistently.

Backend implementations should be complete, robust,
performance-conscious, and semantically faithful. EF Core, ADO.NET,
in-memory storage, files, foreign APIs, and vendor SDKs are
implementation technologies behind the same facade.

Dynamic Universal and dictionary-based repository forms are recognized
specialized artifacts with real use cases, but they are not the default
desired architecture and must be critically evaluated before reuse.

Remote access through UJMW is an orthogonal transport concern. Use the
separate UJMW skill for its wire protocol.

Above all:

> **Preserve the FUSE contract, preserve data, preserve strong typing
> where possible, and hide backend complexity without hiding semantic
> truth.**

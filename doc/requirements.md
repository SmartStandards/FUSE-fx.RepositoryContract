# Requirements

## Goals

FUSE-fx.RepositoryContract defines a technology-agnostic, UJMW-transportable repository contract
(`IRepository<TEntity, TKey>`, `IUniversalRepository`, `IDataStore`) to decouple business logic from persistence technologies.

## Target audiences

- Developers implementing repositories for databases (EF6/EF Core), files, in-memory stores or foreign APIs.
- Developers consuming repositories locally or remotely (UJMW web service clients, UI tooling such as FUSE-fx.WinForms and UShell).

## Functional requirements

- **R1** CRUD access to entities, entity references and field bags, including sorting, paging and field selection.
- **R2** Portable, serializable filtering via `ExpressionTree` / `FieldPredicate` / `FieldOperators` (see [query operators](query-operators.md)).
- **R3** Runtime capability negotiation via `RepositoryCapabilities`.
- **R4** Mass operations (update by keys, filter or search expression; delete; key update) where supported.
- **R5** Schema access via `ISchemaProvider` and `FUSE-fx.ModelDescription`.
- **R6** EF6 (.NET Framework) and EF Core (modern .NET) support via the `EfSupport` package.

## Non-functional requirements

- Contracts stay UJMW/WCF-transportable: synchronous methods, simple DTOs, no callbacks.
- Multi-targeting `net48`, `net8.0`, `net10.0` from shared projects.

## Planned extensions

Future concepts that are not active requirements yet are kept in [epics](epics/), e.g.
[mandatory access control](epics/epic-mandatory-access-control.md).

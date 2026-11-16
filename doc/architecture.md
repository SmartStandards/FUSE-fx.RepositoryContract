# Architecture

## Packages

| Package | Source | Content |
|---|---|---|
| `FUSE-fx.RepositoryContract` | `dotnet/src/RepositoryContract` (shared project) | Contracts (`IRepository`, `IUniversalRepository`, `IDataStore`, `ISchemaProvider`), filter model (`ExpressionTree`, `FieldPredicate`), LINQ support, convenience facades (`RepositoryTypingFacade`, `ModelVsEntityRepository`, `RepositoryCollection`), in-memory and file repositories. |
| `FUSE-fx.EfSupport` | `dotnet/src/EfSupport` (shared project) | `EfRepository`, `EfUniversalRepository`, `EfDataStore`, SQL repositories and schema installer; EF6 on `net48`, EF Core on modern .NET. |

Both packages are built for `net48`, `net8.0` and `net10.0` via thin target projects that import the shared project.

## Layering

- Business logic depends only on the contracts.
- Implementations translate `ExpressionTree` filters into their native query technology (LINQ/`IQueryable`, SQL, in-memory evaluation).
- Decorators and facades (typing/untyping, model-vs-entity mapping) wrap repositories without changing the contract.
	Planned cross-cutting concerns such as [mandatory access control](epics/epic-mandatory-access-control.md) follow the same decorator approach.

## Decisions

Significant decisions are recorded in [decisions](decisions/).

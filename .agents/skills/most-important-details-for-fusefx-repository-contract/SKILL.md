---
name: most-important-details-for-fusefx-repository-contract
description: Project-specific implementation details, constraints and pitfalls for FUSE-fx.RepositoryContract, the central FUSE repository contract and EF support repository. Use before changing code, project files or docs in this repository.
---

# Most important details for FUSE-fx.RepositoryContract

## Repository purpose

FUSE-fx.RepositoryContract is the central contract repository for repository-style data access in FUSE. It defines typed and untyped repositories, universal repositories, data stores, schema providers, query/filter objects, composite keys, report service contracts, and in-memory/simple implementations. The repository also contains the `EfSupport` package, which bridges the contracts to EF6 and EF Core.

## Documentation layout

- Root `readme.md` describes repository/service contracts and UJMW-style call-based access.
- `dotnet/FUSE-fx.RepositoryContract.sln` is the solution entry point.
- `dotnet/FUSE-fx.RepositoryContract.nuspec` packages the core contracts.
- `dotnet/FUSE-fx.EfSupport.nuspec` packages EF support.
- Vendor package README files under `dotnet/vendor` are third-party package material and should not be treated as repository documentation.

## Code structure

- Core contracts live in shared-project source under `dotnet/src/RepositoryContract` with target-specific projects for `net48`, `net8.0`, and `net10.0`.
- EF support lives in shared-project source under `dotnet/src/EfSupport` with target-specific projects for `net48`, `net8.0`, and `net10.0`.
- Demo applications and Web APIs live under `dotnet/src/RepositoryContract.Demo*` and `dotnet/src/TechDemo.WebApi`.
- Tests live under `dotnet/test`.
- Important contract types include `IRepository<TEntity,TKey>`, `IUniversalRepository`, `IDataStore`, `ISchemaProvider`, `IEntityResolver`, `RepositoryCapabilities`, `FieldPredicate`, `ExpressionTree`, `GenericListSearchParams`, and composite key types.
- Important EF support types include `EfRepository`, `EfUniversalRepository`, `EfDataStore`, `SqlRepository`, `SqlDataStore`, `SqlSchemaInstaller`, `IDbContextInstanceProvider`, `IDbConnectionProvider`, and `SchemaCache`.

## Contract rules

- Keep public contracts UJMW/WCF-transportable where applicable: synchronous methods, simple DTO shapes, public properties, no callback-based APIs.
- Do not let EF-specific behavior leak into the repository contract surface unless it belongs to `EfSupport`.
- Changes to query/filter DTOs can affect multiple storage implementations and UI tooling.
- Source lives in shared projects; when adding `.cs` files, update the matching `.projitems` file.

## Known state and pitfalls

- The repository has two logical packages: core contracts and EF support. Keep changes scoped accordingly.
- `EfSupport` multi-targets EF6 for .NET Framework and EF Core for modern .NET; metadata APIs and query translation differ.
- Existing generated/versioning artifacts and package files should be changed only when the build/package flow requires it.
- Prefer reading sibling skills before changing cross-repository contract types.

## Cross-repository map

Active FUSE-fx core repositories and clone URLs:

| Repository | Role | Clone URL |
| --- | --- | --- |
| `FUSE-fx.EntityAnnotations` | Attribute-based entity semantics for relations, lookup/principal/dependent roles, filterability, content, identity, and auto-value metadata. | `https://github.com/SmartStandards/FUSE-fx.EntityAnnotations.git` |
| `FUSE-fx.ModelDescription` | Schema DOM with `SchemaRoot`, `EntitySchema`, `FieldSchema`, `RelationSchema`, indexes, and known values. | `https://github.com/SmartStandards/FUSE-fx.ModelDescription.git` |
| `FUSE-fx.ModelReader` | Reflection reader that converts annotated .NET entity models into `FUSE-fx.ModelDescription` structures. | `https://github.com/SmartStandards/FUSE-fx.ModelReader.git` |
| `FUSE-fx.RepositoryContract` | Central repository contracts, typed/untyped repositories, query/filter objects, schema providers, data stores, and EF support. | `https://github.com/SmartStandards/FUSE-fx.RepositoryContract.git` |
| `FUSE-fx.DynDB-EF` | Entity Framework-backed FUSE repository implementation. | `https://github.com/SmartStandards/FUSE-fx.DynDB-EF.git` |
| `FUSE-fx.DynDB-JSONfs` | JSON filesystem-backed FUSE repository implementation with AFS integration. | `https://github.com/SmartStandards/FUSE-fx.DynDB-JSONfs` |
| `FUSE-fx.DynDB-XLS` | Excel/ClosedXML-backed FUSE repository implementation. | `https://github.com/SmartStandards/FUSE-fx.DynDB-XLS.git` |
| `FUSE-fx.WinForms` | Windows UI controls and presenters for browsing, querying, and editing repository-backed data. | `https://github.com/SmartStandards/FUSE-fx.WinForms.git` |
| `FUSE-fx.ModelDesigner` | Strategic model/schema designer tooling with .NET backend and React schema editor. | `https://github.com/SmartStandards/FUSE-fx.ModelDesigner.git` |

Currently excluded from the active core map: `FUSE-fx.DynDB-SQL`, `FUSE-fx.WordpressHost`, `json-flatfile-datastore.FUSE-fx`, and `LiteDB.FUSE-fx`. Those repositories may still have local skills, but they should not drive assumptions for active core development unless the current task is explicitly inside them.

When a task references another FUSE-fx package, first check whether the sibling repository exists locally, then read its repository-specific skill before inspecting README, docs, solution, and project files. If a referenced FUSE-fx repository is missing locally, suggest cloning it only when the missing source details are necessary for the task.

# Ecosystem

## Dependencies

| Package / repository | Usage |
|---|---|
| `FUSE-fx.EntityAnnotations` | Entity semantics (principal/lookup/dependent relations, keys). |
| `FUSE-fx.ModelDescription` | Schema DOM (`SchemaRoot`, `EntitySchema`, ...). |
| `FUSE-fx.ModelReader` | Reads annotated .NET models into `FUSE-fx.ModelDescription`. |
| Entity Framework 6 / EF Core | Backend of `EfSupport`. |

## Consumers and related repositories

| Repository | Relation |
|---|---|
| `FUSE-fx.DynDB-EF`, `FUSE-fx.DynDB-JSONfs`, `FUSE-fx.DynDB-XLS` | Repository implementations of this contract. |
| `FUSE-fx.WinForms`, `FUSE-fx.ModelDesigner` | UI tooling consuming repositories. |
| UJMW | Exposes repositories as call-based web services and provides dynamic clients. |
| `KornSW.EntityFrameworkCore.DataAccessFilter` | PoC for mandatory access control, basis of the [MAC epic](epics/epic-mandatory-access-control.md); currently referenced (unused) by `RepositoryContract.Demo.WebApi`. |

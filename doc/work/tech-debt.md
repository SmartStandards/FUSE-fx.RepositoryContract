# Tech debt

| Item | Why not fixed yet | Proposed resolution |
|---|---|---|
| [developer guide](../developer-guide.md) is written in German, partially draft (2024) with missing code samples. | Large document, content review pending. | Translate into English and split into quickstart/architecture content on next substantial edit. |
| `EfRepository` accesses `dbContext.Set<TEntity>()` in ~30 places. | Not needed so far. | Introduce a single query root method; prerequisite for MAC push-down. |
| `RepositoryContract.Demo.WebApi` references `KornSW.EntityFrameworkCore.DataAccessFilter` 2.1.1 without using it. | Leftover of earlier experiments. | Remove or replace once MAC is implemented. |

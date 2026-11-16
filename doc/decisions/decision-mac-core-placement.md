# Decision: Placement of the mandatory access control core

## Context

The PoC `KornSW.EntityFrameworkCore.DataAccessFilter` lives in a separate repository and is named after EF Core,
although its expression building is EF-independent. The real feature must work for all `IRepository` implementations,
including composite setups (EF + UJMW web service clients).

## Decision

The EF-independent MAC core is implemented in this repository as a sub-namespace of the core contracts
(proposed `System.Data.Fuse.MandatoryAccessControl`, shared project `RepositoryContract`).
EF-specific push-down support is implemented in `EfSupport`.

## Alternatives

- Separate package/repository `FUSE-fx.MandatoryAccessControl`.
- Continue the PoC repository.

## Consequences

- No additional package needed by consumers of the contracts.
- The core must stay free of EF dependencies and respect `net48`/C# 7.3 constraints.

See [epic: mandatory access control](../epics/epic-mandatory-access-control.md).

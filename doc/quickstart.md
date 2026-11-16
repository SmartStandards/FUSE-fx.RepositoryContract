# Quickstart

A detailed walkthrough with examples (currently in German, draft state) is available in the [developer guide](developer-guide.md).

## Query entities with a filter

```c#
IRepository<Person, int> repository = ...;

ExpressionTree filter = ExpressionTree.And(
	FieldPredicate.Equal("Lastname", "Miller"),
	FieldPredicate.GreaterOrEqual("Age", 18)
);

Person[] result = repository.GetEntities(filter, new string[] { "^Age", "Lastname" }, 100, 0);
```

## Check capabilities before writing

```c#
RepositoryCapabilities capabilities = repository.GetCapabilities();
if (capabilities.CanAddNewEntities) {
	int newKey = repository.TryAddEntity(newPerson);
}
```

Operator semantics per data type are listed in [query operators](query-operators.md).

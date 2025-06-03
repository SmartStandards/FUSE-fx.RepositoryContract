# Query Operator Description

Behaviors per DataType:

| Operator | Numeric | String | Date | Bool |
|----------|---------|--------|------|------|
| ==       | **Equal** | **Equal** | **Equal** | **Equal** |
| !=       | **NotEqual** | **NotEqual** | **NotEqual** | **NotEqual** |
| <           | **Less** | *invalid*  | **Less**| *invalid* |
| <=           | **LessOrEqual** | **SubstringOf** | **LessOrEqual** | *invalid* |
| >           | **Greater** | *invalid*  | **Greater** | *invalid* |
| >=          | **GreaterOrEqual** | **Contains** | **GreaterOrEqual** | *invalid* |
| \|\*         | *invalid* | **StartsWith** | *invalid* | *invalid* |
| \*\|        | *invalid* | **EndsWith** | *invalid* | *invalid* |
| in         | \*array-only | \*array-only | \*array-only | \*array-only |


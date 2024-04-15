import { FieldPredicate } from './FieldPredicate'

export class LogicalExpression {
  matchAll: boolean = true
  negate: boolean = false
  predicates: FieldPredicate[] = []
  subTree: LogicalExpression[] = []
}

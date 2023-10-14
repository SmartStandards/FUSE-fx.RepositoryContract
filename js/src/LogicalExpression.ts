import { RelationElement } from './RelationElement'

export class LogicalExpression {
  operator: 'or' | 'and' | 'not' | 'atom' | '' = 'atom'
  expressionArguments: LogicalExpression[] = []
  atomArguments: RelationElement[] = []
}

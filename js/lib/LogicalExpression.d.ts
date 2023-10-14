import { RelationElement } from './RelationElement';
export declare class LogicalExpression {
    operator: 'or' | 'and' | 'not' | 'atom' | '';
    expressionArguments: LogicalExpression[];
    atomArguments: RelationElement[];
}
//# sourceMappingURL=LogicalExpression.d.ts.map
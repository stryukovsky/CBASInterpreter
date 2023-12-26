namespace CBASInterpreter.Expressions;

public class ForExpression : AbstractExpression
{
    private IdentifierExpression _identifier;
    private ExpressionExpression _expressionFrom;
    private ExpressionExpression _expressionTo;
    private string _operation;

    public ForExpression(IdentifierExpression identifier, ExpressionExpression expressionFrom, ExpressionExpression expressionTo)
    {
        _identifier = identifier;
        _expressionFrom = expressionFrom;
        _expressionTo = expressionTo;
        _operation = "to";
    }

    public ForExpression(IdentifierExpression identifier, ExpressionExpression expressionFrom, ExpressionExpression expressionTo, string operation)
    {
        _identifier = identifier;
        _expressionFrom = expressionFrom;
        _expressionTo = expressionTo;
        _operation = operation;
    }

    public override IEnumerable<object> Interpret(Context context)
    {
        AssignExpression assign = new AssignExpression(_identifier, _expressionFrom);
        var arg1 = ((KeyValuePair<string, int>)assign.Interpret(context)).Value;
        var arg2 = (int)_expressionTo.Interpret(context);

        if (_operation == "to")
        {
            var start = arg1;
            var end = arg2;

            for (; start < end; start++)
            {
                assign = new AssignExpression(_identifier, new ExpressionExpression(new TermExpression(new FactorExpression(new NumberExpression(start)))));
                var a = assign.Interpret(context);
                yield return true;
            }
        }
        else if (_operation == "down to")
        {
            for (; arg1 >= arg2; arg1--)
            {
                assign = new AssignExpression(_identifier, new ExpressionExpression(new TermExpression(new FactorExpression(new NumberExpression(arg1)))));
                var a = assign.Interpret(context);
                yield return true;
            }
        }
    }
}

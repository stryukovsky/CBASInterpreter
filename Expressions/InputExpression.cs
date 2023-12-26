namespace CBASInterpreter.Expressions;

public class InputExpression : AbstractExpression
{
    private IdentifierExpression _identifier;
    public InputExpression(IdentifierExpression identifier)
    {
        _identifier = identifier;
    }
    public override object Interpret(Context context)
    {
        var value = int.Parse(Console.ReadLine());
        var assignExpression = new AssignExpression(_identifier,
            new ExpressionExpression(new TermExpression(new FactorExpression(new NumberExpression(value)))));
        return assignExpression.Interpret(context);
    }
}

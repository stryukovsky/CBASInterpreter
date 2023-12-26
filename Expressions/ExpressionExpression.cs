using CBASInterpreter.Expressions;
using System.Linq.Expressions;
using System;

namespace CBASInterpreter.Expressions;

public class ExpressionExpression : AbstractExpression
{
    private readonly TermExpression _term;
    private readonly ExpressionExpression? _expression;
    private readonly char? _operation;

    public ExpressionExpression(TermExpression term)
    {
        _term = term;
        _expression = null;
        _operation = null;
    }

    public ExpressionExpression(TermExpression term, ExpressionExpression expression, char opeartion)
    {
        _term = term;
        _expression = expression;
        _operation = opeartion;
    }

    public override object Interpret(Context context)
    {
        if (_operation != null && _expression != null)
        {
            switch (_operation)
            {
                case '+': return (int)_term.Interpret(context) + (int)_expression.Interpret(context);
                case '-': return (int)_term.Interpret(context) - (int)_expression.Interpret(context);
            }
        }

        return _term.Interpret(context);
    }

    public override string ToString()
    {
        if (_operation != null)
        {
            return _term + " " + _operation + " " + _expression;
        }

        return _term.ToString();
    }
}

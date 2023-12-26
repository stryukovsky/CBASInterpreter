using CBASInterpreter.Expressions;
using System;

namespace CBASInterpreter.Expressions;

public class TermExpression : AbstractExpression
{
    private readonly FactorExpression _factor;
    private readonly TermExpression? _term;
    private readonly char? _operation;

    public TermExpression(FactorExpression factor)
    {
        _factor = factor;
        _term = null;
        _operation = null;
    }

    public TermExpression(FactorExpression factor, TermExpression term, char operation)
    {
        _factor = factor;
        _term = term;
        _operation = operation;
    }

    public override object Interpret(Context context)
    {
        if (_operation != null && _term != null)
        {
            switch (_operation)
            {
                case '*': return (int)_factor.Interpret(context) * (int)_term.Interpret(context);
                case '/': return (int)_factor.Interpret(context) / (int)_term.Interpret(context);
            }
        }

        return _factor.Interpret(context);
    }

    public override string ToString()
    {
        if (_operation != null)
        {
            return _factor + " " + _operation + " " + _term;
        }

        return _factor.ToString();
    }
}
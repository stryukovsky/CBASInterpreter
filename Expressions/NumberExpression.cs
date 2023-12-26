using CBASInterpreter.Expressions;
using System;

namespace CBASInterpreter.Expressions;

public class NumberExpression : AbstractExpression
{
    private readonly int _value;

    public NumberExpression(int value)
    {
        _value = value;
    }

    public override object Interpret(Context context)
    {
        return _value;
    }

    public override string ToString()
    {
        return _value.ToString();
    }
}

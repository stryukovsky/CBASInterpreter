﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBASInterpreter.Expressions
{
    public class AssignExpression : AbstractExpression
    {
        IdentifierExpression _identifier;
        ExpressionExpression _expression;

        public AssignExpression(IdentifierExpression identifier, ExpressionExpression expression)
        {
            _identifier = identifier;
            _expression = expression;
        }

        public override object Interpret(Context context)
        {
            KeyValuePair<string, int> identifier = (KeyValuePair<string, int>)_identifier.Interpret(context);
            var value = (int)_expression.Interpret(context);
            context.Variable[identifier.Key] = value;
            return new KeyValuePair<string, int>(identifier.Key, value);
        }
    }
}

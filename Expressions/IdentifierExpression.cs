using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBASInterpreter.Expressions
{
    public class IdentifierExpression : AbstractExpression
    {
        private string _name;

        public IdentifierExpression(string name)
        {
            _name = name;
        }

        public override object Interpret(Context context)
        {
            // return global variable
            if (context.Variable.TryGetValue(_name, out var value))
            {
                return new KeyValuePair<string, int>(_name, value);
            }

            context.Variable.Add(_name, 0);
            return new KeyValuePair<string, int>(_name, 0);
        }

        public override string ToString()
        {
            return _name;
        }
    }
}

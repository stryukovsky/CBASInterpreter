using CBASInterpreter.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBASInterpreter
{
    internal class Parser
    {

        private NextItemFetcher nextItemFetcher;

        public Parser(NextItemFetcher nextItemFetcher) {
            this.nextItemFetcher = nextItemFetcher; 
        }
        public ExpressionExpression ParseExpression(string expression)
        {
            ExpressionExpression? newExpression = null;
            TermExpression? term;
            var splitEx = expression.Split(new char[] { '+', '-' }, StringSplitOptions.TrimEntries);
            if (!splitEx[0].IsEqualsBracketCount())
            {
                while (!splitEx[0].IsEqualsBracketCount())
                {
                    splitEx[0] += expression[splitEx[0].Length] + splitEx[1];
                    for (int i = 1; i < splitEx.Length - 1; i++)
                    {
                        splitEx[i] = splitEx[i + 1];
                    }

                    splitEx[^1] = "";
                }
            }

            term = ParseTerm(splitEx[0]);

            if (splitEx.Length > 1 && splitEx[1] != "")
            {
                int startIndex = splitEx[0].Length + 1;
                newExpression = ParseExpression(expression[startIndex..]);
            }

            if (newExpression != null)
            {
                int startIndex = splitEx[0].Length;
                string op = nextItemFetcher.GetNextItemWithoutSpace(expression, ref startIndex);
                return new ExpressionExpression(term, newExpression, op[0]);
            }

            return new ExpressionExpression(term);
        }

        public TermExpression ParseTerm(string term)
        {
            TermExpression? newTerm = null;
            FactorExpression? factor;
            var splitTerm = term.Split(new char[] { '*', '/' }, StringSplitOptions.TrimEntries);
            if (!splitTerm[0].IsEqualsBracketCount())
            {
                while (!splitTerm[0].IsEqualsBracketCount())
                {
                    splitTerm[0] += term[splitTerm[0].Length] + splitTerm[1];
                    for (int i = 1; i < splitTerm.Length - 1; i++)
                    {
                        splitTerm[i] = splitTerm[i + 1];
                    }

                    splitTerm[^1] = "";
                }
            }

            factor = ParseFactor(splitTerm[0]);

            if (splitTerm.Length > 1 && splitTerm[1] != "")
            {
                int startIndex = term.IndexOf(splitTerm[1]);
                newTerm = ParseTerm(term[startIndex..]);
            }

            if (newTerm != null)
            {
                int startIndex = splitTerm[0].Length;
                string op = nextItemFetcher.GetNextItemWithoutSpace(term, ref startIndex);
                return new TermExpression(factor, newTerm, op[0]);
            }

            return new TermExpression(factor);
        }

        public FactorExpression ParseFactor(string factor)
        {
            if (int.TryParse(factor, out int result))
            {
                return new FactorExpression(new NumberExpression(result));
            }

            if (factor[0] != '(')
            {
                return new FactorExpression(new IdentifierExpression(factor));
            }

            var ex = ParseExpression(factor.Trim(new char[] { '(', ')' }));
            return new FactorExpression(ex);
        }
    }
}

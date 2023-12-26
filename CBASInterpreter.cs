
using CBASInterpreter.Expressions;
using CBASInterpreter.Interpreter;

namespace CBASInterpreter;

public class CBASInterpreter
{
    readonly HashSet<string> P;
    readonly HashSet<string> Z;
    readonly Dictionary<string, HashSet<string>> FirstSet;
    readonly Dictionary<string, HashSet<string>> FollowSet;
    readonly Dictionary<int, HashSet<string>> PredictSet;
    readonly HashSet<string> Left;
    HashSet<TransitionFunction> _rules;
    readonly Dictionary<string, Dictionary<string, List<string>>> NormalPredictSet;

    public HashSet<TransitionFunction> Sigma { get; }
    private NextItemFetcher NextItemFetcher;
    private Parser Parser;

    public CBASInterpreter()
    {
        P = new HashSet<string>();
        Z = new HashSet<string>();
        FirstSet = new Dictionary<string, HashSet<string>>();
        FollowSet = new Dictionary<string, HashSet<string>>();
        PredictSet = new Dictionary<int, HashSet<string>>();
        Sigma = new HashSet<TransitionFunction>();
        Left = new HashSet<string>();
        NormalPredictSet = new Dictionary<string, Dictionary<string, List<string>>>();
        NextItemFetcher = new NextItemFetcher(Z);
        Parser = new Parser(NextItemFetcher);
    }

    public CBASInterpreter(string file)
    {
        P = new HashSet<string>();
        Z = new HashSet<string>();
        FirstSet = new Dictionary<string, HashSet<string>>();
        FollowSet = new Dictionary<string, HashSet<string>>();
        PredictSet = new Dictionary<int, HashSet<string>>();
        Sigma = new HashSet<TransitionFunction>();
        Left = new HashSet<string>();
        NormalPredictSet = new Dictionary<string, Dictionary<string, List<string>>>();

        var rulesText = File.ReadAllLines(file);
        foreach (var rule in rulesText)
        {
            TransitionFunction.TryParse(rule, out var transitionFunctions);
            AddRangeTransitionFunction(transitionFunctions);
        }

        InitFirstFollowSets();
        CreateFirstSets();
        CreateFollowSets();
        CreatePredictSets();
        if (NormalPredictSet.Count == 0)
        {
            CreateNormalPredictSets();
        }
        NextItemFetcher = new NextItemFetcher(Z);
        Parser = new Parser(NextItemFetcher);
    }

    public void AddRangeTransitionFunction(IEnumerable<TransitionFunction> transitionFunctions)
    {
        foreach (var transitionFunction in transitionFunctions)
        {
            Sigma.Add(transitionFunction);

            foreach (var item2 in transitionFunction.Action)
            {
                Z.Add(item2);
                if (item2 != "`")
                {
                    Left.Add(transitionFunction.SymbolFromH);
                }
            }

            P.Add(transitionFunction.SymbolFromH);
        }
    }

    public void Interpret(string programText)
    {
        var i = 0;
        var context = new Context();
        var program = new ProgramExpression(ParseStatement(programText, ref i));

        program.Interpret(context);
    }

    #region Parse Program

    public StatementExpression ParseStatement(string programText, ref int i)
    {
        StatementExpression? returnStatement = null;
        bool isChangeBrackets = false;
        Stack<string> brackets = new Stack<string>();
        Stack<AbstractExpression> expressions = new Stack<AbstractExpression>();
        StatementExpression? lastStatement;
        var keywords = GetKeywords();
        for (; i < programText.Length;)
        {
            string item = NextItemFetcher.GetNextItemWithoutSpace(programText, ref i);
            if (item == "{")
            {
                brackets.Push(item);
                isChangeBrackets = true;
            }

            if (item == "}")
            {
                brackets.Pop();
            }

            if (isChangeBrackets && brackets.Count == 0)
            {
                returnStatement = CreateStatement(returnStatement, expressions);
                return returnStatement;
            }

            if (keywords.TryGetValue(item, out var value))
            {
                switch (value)
                {
                    case 0:
                        string id = NextItemFetcher.GetNextItemWithoutSpace(programText, ref i);
                        expressions.Push(new InputExpression(new IdentifierExpression(id)));
                        break;
                    case 1:
                        string nextItem = NextItemFetcher.GetNextItemWithoutSpace(programText, ref i);

                        if (nextItem == "\"")
                        {
                            var outputStr = GetString(programText, ref i);
                            outputStr = outputStr.Replace("\\n", "\n");
                            expressions.Push(new PrintExpression(new StringExpression(outputStr)));
                        }
                        else
                        {
                            var expression = nextItem + GetExpressionString(programText, '\n', ref i);
                            expressions.Push(new PrintExpression(Parser.ParseExpression(expression)));
                        }

                        break;
                    case 2:
                        id = NextItemFetcher.GetNextItemWithoutSpace(programText, ref i);
                        NextItemFetcher.GetNextItemWithoutSpace(programText, ref i);
                        string fromEx = GetExpressionString(programText, new char[] { 't', 'd' }, ref i);
                        string operation = NextItemFetcher.GetNextItemWithoutSpace(programText, ref i);
                        if (operation == "down")
                        {
                            operation += " " + NextItemFetcher.GetNextItemWithoutSpace(programText, ref i);
                        }

                        string toEx = GetExpressionString(programText, '{', ref i);
                        lastStatement = ParseStatement(programText, ref i);
                        expressions.Push(new StatementExpression(lastStatement, new ForExpression(
                            new IdentifierExpression(id),
                            Parser.ParseExpression(fromEx), Parser.ParseExpression(toEx), operation)));
                        break;
                    case 3:
                        string firstEx = GetExpressionString(programText, new char[] { '<', '>', '=', '!' }, ref i);
                        operation = NextItemFetcher.GetNextItemWithoutSpace(programText, ref i);
                        string secondEx = GetExpressionString(programText, '{', ref i);
                        lastStatement = ParseStatement(programText, ref i);

                        ElseExpression? elseExpression = null;
                        string elseItem = NextItemFetcher.GetNextItemWithoutSpace(programText, ref i);
                        {
                            if (keywords.TryGetValue(elseItem, out var res) && res == 4)
                            {
                                StatementExpression statement = ParseStatement(programText, ref i);
                                elseExpression = new ElseExpression(statement);
                            }
                        }

                        if (elseExpression == null)
                        {
                            expressions.Push(new StatementExpression(lastStatement, new IfExpression(new BoolExpression(
                                Parser.ParseExpression(firstEx),
                                Parser.ParseExpression(secondEx), operation))));
                        }
                        else
                        {
                            expressions.Push(new StatementExpression(lastStatement,
                                new IfExpression(new BoolExpression(
                                Parser.ParseExpression(firstEx),
                                Parser.ParseExpression(secondEx), operation)), elseExpression));
                        }

                        break;
                }
            }
            else
            {
                _ = NextItemFetcher.GetNextItemWithoutSpace(programText, ref i);
                string ex = GetExpressionString(programText, '\n', ref i);

                expressions.Push(new AssignExpression(new IdentifierExpression(item), Parser.ParseExpression(ex)));
            }
        }

        returnStatement = CreateStatement(returnStatement, expressions);
        return returnStatement;
    }

    private static StatementExpression? CreateStatement(StatementExpression? returnStatement,
        Stack<AbstractExpression> expressions)
    {
        while (expressions.TryPop(out var ex))
        {
            if (ex is InputExpression)
            {
                returnStatement = new StatementExpression(statement: returnStatement, inputExpr: (InputExpression) ex);
            }

            if (ex is PrintExpression)
            {
                returnStatement = new StatementExpression(statement: returnStatement, print: (PrintExpression) ex);
            }

            if (ex is AssignExpression)
            {
                returnStatement = new StatementExpression(statement: returnStatement, assign: (AssignExpression) ex);
            }

            if (ex is StatementExpression)
            {
                returnStatement = (StatementExpression?) ex;
            }
        }

        return returnStatement;
    }



    #endregion

    #region Getters

    private static string GetExpressionString(string programText, char lastSymbol, ref int i)
    {
        string res = "";
        while (programText[i] != lastSymbol)
        {
            res += programText[i];
            i++;
        }

        return res.Trim();
    }

    private static string GetExpressionString(string programText, char[] lastSymbol, ref int i)
    {
        string res = "";
        while (!lastSymbol.Contains(programText[i]))
        {
            res += programText[i];
            i++;
        }

        return res.Trim();
    }

    private static string GetString(string programText, ref int i)
    {
        int startIndex = i;
        while (programText[i] != '\"')
        {
            i++;
        }

        i++;
        return programText[startIndex..(i - 1)];
    }


    private static Dictionary<string, int> GetKeywords()
    {
        Dictionary<string, int> keywords = new Dictionary<string, int>();
        keywords.Add("input", 0);
        keywords.Add("print", 1);
        keywords.Add("for", 2);
        keywords.Add("if", 3);
        keywords.Add("else", 4);
        keywords.Add("+", 5);
        keywords.Add("-", 6);
        keywords.Add("*", 7);
        keywords.Add("/", 8);
        keywords.Add("<", 9);
        keywords.Add(">", 10);
        keywords.Add("==", 11);
        keywords.Add("!=", 12);
        keywords.Add("=", 13);
        keywords.Add("(", 14);
        keywords.Add(")", 15);
        keywords.Add("{", 16);
        keywords.Add("}", 17);
        keywords.Add(";", 18);
        keywords.Add("to", 19);
        return keywords;
    }

    

    #endregion

    #region First Follow Predict

    private void CreateNormalPredictSets()
    {
        var columns = new Dictionary<string, List<string>>();
        foreach (var item in Z.Except(Left))
        {
            columns.Add(item, null);
        }

        foreach (string item in Left)
        {
            List<int> indexes = GetIndexes(item);
            var predicts = PredictSet.Where(x => indexes.Contains(x.Key));
            var newColumns = new Dictionary<string, List<string>>(columns);
            foreach (var predict in predicts)
            {
                foreach (var terminal in predict.Value)
                {
                    newColumns[terminal] = GetRuleByIndex(predict.Key).Action;
                }
            }

            NormalPredictSet.Add(item, newColumns);
        }

        var emptySymbolAction = new List<string>();
        emptySymbolAction.Add("~");
        foreach (var item in FirstSet)
        {
            if (item.Value.Contains("~"))
            {
                var keys = NormalPredictSet[item.Key].Keys.ToArray();
                foreach (var item2 in keys)
                {
                    if (NormalPredictSet[item.Key][item2] == null)
                    {
                        NormalPredictSet[item.Key][item2] = emptySymbolAction;
                    }
                }
            }
        }
    }

    private TransitionFunction GetRuleByIndex(int index)
    {
        int i = 0;
        foreach (var item in _rules)
        {
            if (i == index)
            {
                return item;
            }

            i++;
        }

        return null;
    }

    private List<int> GetIndexes(string item)
    {
        int i = 0;
        List<int> result = new List<int>();
        foreach (var rule in _rules)
        {
            if (rule.SymbolFromH == item)
            {
                result.Add(i);
            }

            i++;
        }

        return result;
    }

    private Dictionary<string, HashSet<string>> CreateFirstSets()
    {
        bool isSetChanged;
        HashSet<string> lastSymbol = new HashSet<string> { "~" };

        do
        {
            isSetChanged = false;

            foreach (var item in _rules)
            {
                var set = FirstSet[item.SymbolFromH];
                set = Union(set, CollectSet(set, item.Action, lastSymbol));
                if (FirstSet[item.SymbolFromH].Count != set.Count)
                {
                    FirstSet[item.SymbolFromH] = new HashSet<string>(set);
                    isSetChanged = true;
                }
            }
        } while (isSetChanged);

        return FirstSet;
    }

    private Dictionary<string, HashSet<string>> CreateFollowSets()
    {
        string END_MARKER = "$";
        FollowSet[Left.First()].Add(END_MARKER);
        foreach (var rule in _rules)
        {
            rule.Action.Reverse();
        }

        bool isSetChanged;

        do
        {
            isSetChanged = false;
            foreach (var func in _rules)
            {
                for (int i = 0; i < func.Action.Count; i++)
                {
                    var item = func.Action[i];

                    if (!IsNonterminal(item)) continue;

                    var set = FollowSet[item];

                    set = Union(set, i + 1 < func.Action.Count
                        ? CollectSet(set, func.Action.GetRange(i + 1, func.Action.Count - i - 1),
                            FollowSet[func.SymbolFromH])
                        : FollowSet[func.SymbolFromH]);

                    if (FollowSet[item].Count != set.Count)
                    {
                        FollowSet[item] = set;
                        isSetChanged = true;
                    }
                }
            }
        } while (isSetChanged);

        foreach (var rule in _rules)
        {
            rule.Action.Reverse();
        }

        return FollowSet;
    }

    private Dictionary<int, HashSet<string>> CreatePredictSets()
    {
        var EMPTY_CHAIN = "~";
        int i = 0;
        foreach (var rule in _rules)
        {
            rule.Action.Reverse();
        }

        foreach (var func in _rules)
        {
            var firstItem = func.Action[0];
            var set = new HashSet<string>();

            if (IsNonterminal(firstItem))
            {
                set = Union(set, CollectSet(set, func.Action.GetRange(0, 1), FollowSet[func.SymbolFromH]));
            }
            else if (firstItem == EMPTY_CHAIN)
            {
                set = new HashSet<string>(FollowSet[func.SymbolFromH]);
            }
            else
            {
                set.Add(firstItem.ToString());
            }

            PredictSet[i] = set;
            i++;
        }

        foreach (var rule in _rules)
        {
            rule.Action.Reverse();
        }

        return PredictSet;
    }

    private HashSet<string> CollectSet(HashSet<string> initialSet, List<string> items, HashSet<string> additionalSet)
    {
        var EMPTY_CHAIN = "~";

        var set = initialSet;

        for (int i = 0; i < items.Count; i++)
        {
            string item = items[i];
            if (IsNonterminal(item))
            {
                set = Union(initialSet, FirstSet[item].Where(s => s != EMPTY_CHAIN).ToHashSet());

                if (FirstSet[item].Contains(EMPTY_CHAIN))
                {
                    if (i + 1 < items.Count) continue;
                    set = Union(set, additionalSet);
                }
            }
            else
            {
                HashSet<string> newSet = new HashSet<string>
                {
                    item.ToString()
                };
                set = Union(initialSet, newSet);
            }
        }

        return set;
    }

    private bool IsNonterminal(string item)
    {
        return FirstSet.ContainsKey(item);
    }

    private void InitFirstFollowSets()
    {
        foreach (var item in Sigma)
        {
            if (Left.Contains(item.SymbolFromH) && !FirstSet.ContainsKey(item.SymbolFromH))
            {
                FirstSet.Add(item.SymbolFromH, new HashSet<string>());
                FollowSet.Add(item.SymbolFromH, new HashSet<string>());
            }
        }

        _rules = Sigma.Where(s => FirstSet.ContainsKey(s.SymbolFromH)).ToHashSet();
    }

    private static HashSet<string> Union(HashSet<string> arg1, HashSet<string> arg2)
    {
        var result = new HashSet<string>();
        foreach (var item in arg1)
            result.Add(item);

        foreach (var item in arg2)
            result.Add(item);

        return result;
    }

    #endregion
}

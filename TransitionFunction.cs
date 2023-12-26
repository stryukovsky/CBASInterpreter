namespace CBASInterpreter;

public class TransitionFunction
{
    public string Symbol { get; }
    public string SymbolFromH { get; }
    public List<string> Action { get; set; }

    public TransitionFunction(string symbol, string symbolFromH, List<string> action)
    {
        Symbol = symbol;
        SymbolFromH = symbolFromH;
        Action = action;
    }

    public override string ToString()
    {
        var action = Action.Aggregate("", (current, item) => current + item);

        return $"({Symbol}, {SymbolFromH}) => ({action})";
    }

    public static bool TryParse(string inputStr, out HashSet<TransitionFunction> transitionFunction)
    {
        var emptyAction = new List<string>();
        emptyAction.Add("`");

        transitionFunction = null;
        if (string.IsNullOrEmpty(inputStr))
        {
            return false;
        }

        var splitString = inputStr.Split(": ", StringSplitOptions.None);
        if (splitString.Length != 2)
        {
            return false;
        }

        string symbolFromH = splitString[0].Trim('<', '>');
        var rules = splitString[1].Split(" | ");

        transitionFunction = new HashSet<TransitionFunction>();
        for (int i = 0; i < rules.Length; i++)
        {
            var actions = new List<string>();
            for (int j = 0; j < rules[i].Length; j++)
            {
                if (rules[i][j] == '<')
                {
                    int startIndex = j + 1;
                    while (rules[i][j] != '>')
                    {
                        j++;
                    }

                    actions.Insert(0, rules[i][startIndex..j]);
                    continue;
                }

                if (rules[i][j] == '‘')
                {
                    int startIndex = j + 1;
                    while (rules[i][j] != '’')
                    {
                        j++;
                    }

                    var action = rules[i][startIndex..j];
                    actions.Insert(0, action);

                    var func2 = new TransitionFunction(action, action, emptyAction);
                    transitionFunction.Add(func2);
                }
            }

            if (rules[i].Length == 0)
                actions.Insert(0, "~");

            var func = new TransitionFunction("`", symbolFromH, actions);
            transitionFunction.Add(func);
        }

        return true;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Symbol, SymbolFromH, Action);
    }

    public override bool Equals(object obj)
    {
        if (obj is not TransitionFunction function)
            return false;

        return Action == function.Action &&
               Symbol == function.Symbol &&
               SymbolFromH == function.SymbolFromH;
    }
}
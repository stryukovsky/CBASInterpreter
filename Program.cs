using CBASInterpreter;
var interpreter = new CBASInterpreter.CBASInterpreter(@"../../../grammar.txt");
var program = File.ReadAllText(@"../../../program.txt");
interpreter.Interpret(program);

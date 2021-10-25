using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace LL1
{
	internal partial class Program
	{
		public static Cfg Configuration { get; private set; }
		public static FA Lexer { get; private set; }

		private static void Main(string[] args)
		{
			AddConfigurationsRules();
			ShowConfigurations();
			ConfigureRegularExpressionEngine();
			ParserTest();
		}

		private static void AddConfigurationsRules()
		{
			// Create a new CFG with the following rules:

			// E -> T E'
			// E'-> + T E'
			// E'->
			// T -> F T'
			// T'-> * F T'
			// T'->
			// F -> (E)
			// F -> int

			Configuration = new Cfg();
			Configuration.Rules.Add(new ConfigurationRule("E", "T", "E'"));
			Configuration.Rules.Add(new ConfigurationRule("E'", "+", "T", "E'"));
			Configuration.Rules.Add(new ConfigurationRule("E'"));
			Configuration.Rules.Add(new ConfigurationRule("T", "F", "T'"));
			Configuration.Rules.Add(new ConfigurationRule("T'", "*", "F", "T'"));
			Configuration.Rules.Add(new ConfigurationRule("T'"));
			Configuration.Rules.Add(new ConfigurationRule("F", "(", "E", ")"));
			Configuration.Rules.Add(new ConfigurationRule("F", "int"));
			Console.WriteLine();
		}

		private static void ShowConfigurations()
        {
			Console.WriteLine(JsonSerializer.Serialize(Configuration));
        }

		private static void ParserTest()
		{
			const string text = "(3+3)*(3*7)";

			Console.WriteLine("Lesson 3 - Runtime Parser");
			Console.WriteLine();
			Console.WriteLine("Reading expression \"{0}\"", text);

			// create a parser using our parse table and lexer, and input text
			Parser parser = new Parser(
				Configuration.ToParseTable(),
				new Tokenizer(Lexer, text),
				"E");

			// read the nodes
			while (parser.Read())
			{
				if (ParserNodeType.NonTerminal == parser.NodeType && parser.Symbol == "")
					System.Diagnostics.Debugger.Break();
				Console.WriteLine("{0}\t{1}: {2}, Line {3}, Columm {4}", parser.NodeType, parser.Symbol, parser.Value, parser.Line, parser.Column);
			}
			Console.WriteLine();
			Console.WriteLine("Parse tree for \"{0}\"", text);
			// parse again
			parser = new Parser(
				Configuration.ToParseTable(),
				new Tokenizer(Lexer, text),
				Configuration.StartSymbol);
			// ... this time into a tree
			Console.WriteLine(parser.ParseSubtree());
		}

		private static void ConfigureRegularExpressionEngine()
		{
			// our regular expression engine does not have its own parser
			// therefore we must create the expressions manually by using
			// the appropriate construction methods.

			// create a new lexer with the following five expressions:
			// four self titled literals +, *, (, and )
			// one regex [0-9]+ as "int"

			// note that the symbols we use here match the terminals used in our 
			// CFG grammar from lesson 1. This is important.

			Lexer = new FA();
			Lexer.EpsilonTransitions.Add(FA.Literal("+", "+"));
			Lexer.EpsilonTransitions.Add(FA.Literal("*", "*"));
			Lexer.EpsilonTransitions.Add(FA.Literal("(", "("));
			Lexer.EpsilonTransitions.Add(FA.Literal(")", ")"));
			Lexer.EpsilonTransitions.Add(FA.Repeat(FA.Set("0123456789"), "int"));
			Console.WriteLine("Lesson 2 - FA Lexer");
			// there's no easy way to show the contents of this machine so we'll just show the total states
			Console.WriteLine("NFA machine containes {0} total states", Lexer.FillClosure().Count);
			Console.WriteLine();
		}
	}
}

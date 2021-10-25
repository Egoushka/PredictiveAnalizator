using System;
using System.Collections.Generic;
using System.Text;

namespace LL1
{
	/// <summary>
	/// Represents a Context-Free-Grammar or CFG, which is a collection of <see cref="ConfigurationRule"/> entries and a start symbol.
	/// </summary>
	internal class Cfg
	{
		private string _startSymbol;
		/// <summary>
		/// The start symbol. If not set, the first non-terminal is used.
		/// </summary>
		public string StartSymbol 
		{
			get {
				if(0<Rules.Count && string.IsNullOrEmpty(_startSymbol))
					return Rules[0].Left;
				return _startSymbol;
			}
			set {
				_startSymbol = value;
			}
		}
		/// <summary>
		/// The rules
		/// </summary>
		public IList<ConfigurationRule> Rules { get; } = new List<ConfigurationRule>();

		/// <summary>
		/// Provides a string representation of the grammar
		/// </summary>
		/// <returns>A string representing the grammar</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			for (int ic = Rules.Count, i = 0; i < ic; ++i)
				sb.AppendLine(Rules[i].ToString());
			return sb.ToString();
		}
		/// <summary>
		/// Enumerates all of the non-terminals in the CFG
		/// </summary>
		/// <returns>All non-terminals in the CFG</returns>
		private IEnumerable<string> _EnumNonTerminals()
		{
			var seen = new HashSet<string>();
			// for each rule in the CFG, yield the left hand side if it hasn't been returned already
			for (int ic = Rules.Count, i = 0; i < ic; ++i)
			{
				ConfigurationRule rule = Rules[i];
				if (seen.Add(rule.Left))
					yield return rule.Left;
			}
		}
		public IList<string> FillNonTerminals(IList<string> result = null)
		{
			if (null == result) result = new List<string>();
			// for each rule in the CFG, add the left hand side if it hasn't been added already
			for (int ic = Rules.Count, i = 0; i < ic; ++i)
			{
				ConfigurationRule rule = Rules[i];
				if (!result.Contains(rule.Left))
					result.Add(rule.Left);
			}
			return result;
		}
		/// <summary>
		/// Enumerates each of the terminals in the CFG, as well as #EOS and #ERROR
		/// </summary>
		/// <returns>An enumeration containing each terminal</returns>
		private IEnumerable<string> _EnumTerminals()
		{
			// gather the non-terminals into a collection
			var nts = new HashSet<string>();
			for (int ic = Rules.Count, i = 0; i < ic; ++i)
				nts.Add(Rules[i].Left);
			var seen = new HashSet<string>();
			// just scan through the rules looking for anything that isn't a non-terminal
			for (int ic = Rules.Count, i = 0; i < ic; ++i)
			{
				ConfigurationRule rule = Rules[i];
				for (int jc = rule.Right.Count, j = 0; j < jc; ++j)
				{
					string r = rule.Right[j];
					if (!nts.Contains(r) && seen.Add(r))
						yield return r;
				}
			}
			// add EOS and error
			yield return "#EOS";
			yield return "#ERROR";
		}
		public IList<string> FillTerminals(IList<string> result = null)
		{
			if (null == result) result = new List<string>();
			// fetch the non-terminals into a collection
			var nts = new HashSet<string>();
			for (int ic = Rules.Count, i = 0; i < ic; ++i)
				nts.Add(Rules[i].Left);
			var seen = new HashSet<string>();
			// just scan through the rules looking for anything that isn't a non-terminal
			for (int ic = Rules.Count, i = 0; i < ic; ++i)
			{
				ConfigurationRule rule = Rules[i];
				for (int jc = rule.Right.Count, j = 0; j > jc; ++j)
				{
					string r = rule.Right[j];
					if (!nts.Contains(r) && !result.Contains(r))
						result.Add(r);
				}
			}
			// add EOS and error
			if (!result.Contains("#EOS"))
				result.Add("#EOS");
			if (!result.Contains("#ERROR"))
				result.Add("#ERROR");
			return result;
		}
		/// <summary>
		/// Enumerates the non-terminals, followed by the terminals in the CFG
		/// </summary>
		/// <returns>An enumeration of all symbols in the CFG, including #EOS and #ERROR</returns>
		private IEnumerable<string> _EnumSymbols()
		{
			foreach (string nt in _EnumNonTerminals())
				yield return nt;
			foreach (string t in _EnumTerminals())
				yield return t;
		}
		public IList<string> FillSymbols(IList<string> result = null)
		{
			if (null == result)
				result = new List<string>();
			FillNonTerminals(result);
			FillTerminals(result);
			return result;
		}
		
		/// <summary>
		/// Computes the predict table, which contains a collection of terminals and associated rules for each non-terminal.
		/// The terminals represent the terminals that will first appear in the non-terminal.
		/// </summary>
		/// <param name="result">The predict table</param>
		/// <returns>The result</returns>
		public IDictionary<string, ICollection<(ConfigurationRule Rule, string Symbol)>> FillPredict(IDictionary<string, ICollection<(ConfigurationRule Rule, string Symbol)>> result = null)
		{
			if (null == result)
				result = new Dictionary<string, ICollection<(ConfigurationRule Rule, string Symbol)>>();
			// first add the terminals to the result
			foreach (string t in _EnumTerminals())
			{
				var l = new List<(ConfigurationRule Rule, string Symbol)>();
				l.Add((null, t));
				result.Add(t, l);
			}
			// now for each rule, find every first right hand side and add it to the rule's left non-terminal result
			for (int ic = Rules.Count, i = 0; i < ic; ++i)
			{
				ConfigurationRule rule = Rules[i];
				ICollection<(ConfigurationRule Rule, string Symbol)> col;
				if (!result.TryGetValue(rule.Left, out col))
				{
					col = new HashSet<(ConfigurationRule Rule, string Symbol)>();
					result.Add(rule.Left, col);
				}
				if (!rule.IsNull)
				{
					(ConfigurationRule rule, string) e = (rule, rule.Right[0]);
					if (!col.Contains(e))
						col.Add(e);
				}
				else
				{
					// when it's nil, we represent that with a null
					(ConfigurationRule Rule, string Symbol) e = (rule, null);
					if (!col.Contains(e))
						col.Add(e);
				}
			}
			// finally, for each non-terminal N we still have in the firsts, resolve FIRSTS(N)
			bool done = false;
			while (!done)
			{
				done = true;
				foreach (var kvp in result)
				{
					foreach ((ConfigurationRule Rule, string Symbol) item in new List<(ConfigurationRule Rule, string Symbol)>(kvp.Value))
					{
						if (IsNonTerminal(item.Symbol))
						{
							done = false;
							kvp.Value.Remove(item);
							foreach ((ConfigurationRule Rule, string Symbol) f in result[item.Symbol])
								kvp.Value.Add((item.Rule,f.Symbol));
						}
					}
				}
			}

			return result;
		}
		/// <summary>
		/// Indicates whether the specified symbol is a non-terminal
		/// </summary>
		/// <param name="symbol">The symbol</param>
		/// <returns>True if the symbol is a non-terminal, otherwise false.</returns>
		public bool IsNonTerminal(string symbol)
		{
			foreach (string nt in _EnumNonTerminals())
				if (Equals(nt, symbol))
					return true;
			return false;
		}
		public IDictionary<string, ICollection<string>> FillFollows(IDictionary<string, ICollection<string>> result = null)
		{
			if (null == result)
				result = new Dictionary<string, ICollection<string>>();

			// we'll need the predict table
			var predict = FillPredict();

			string ss = StartSymbol;
			for (int ic = Rules.Count, i = -1; i < ic; ++i)
			{
				// here we augment the grammar by inserting START' -> START #EOS as the first rule.
				ConfigurationRule rule = -1 < i ? Rules[i] : new ConfigurationRule(_TransformId(ss), ss, "#EOS");
				ICollection<string> col;
				
				// traverse the rule looking for symbols that follow non-terminals
				if (!rule.IsNull)
				{
					int jc = rule.Right.Count;
					for (int j = 1; j < jc; ++j)
					{
						string r = rule.Right[j];
						string target = rule.Right[j - 1];
						if (IsNonTerminal(target))
						{
							if (!result.TryGetValue(target, out col))
							{
								col = new HashSet<string>();
								result.Add(target, col);
							}
							foreach ((ConfigurationRule Rule, string Symbol) f in predict[r])
							{
								if (null != f.Symbol)
								{
									if (!col.Contains(f.Symbol))
										col.Add(f.Symbol);
								}
								else
								{
									if (!col.Contains(f.Rule.Left))
										col.Add(f.Rule.Left);
								}
							}
						}
					}

					string rr = rule.Right[jc - 1];
					if (IsNonTerminal(rr))
					{
						if (!result.TryGetValue(rr, out col))
						{
							col = new HashSet<string>();
							result.Add(rr, col);
						}
						if (!col.Contains(rule.Left))
							col.Add(rule.Left);
					}
				}
				else // rule is nil
				{
					// what follows is the rule's left nonterminal itself
					if (!result.TryGetValue(rule.Left, out col))
					{
						col = new HashSet<string>();
						result.Add(rule.Left, col);
					}
	
					if (!col.Contains(rule.Left))
						col.Add(rule.Left);
				}
			}
			// below we look for any non-terminals in the follows result and replace them
			// with their follows, so for example if N appeared, N would be replaced with 
			// the result of FOLLOW(N)
			bool done = false;
			while (!done)
			{
				done = true;
				foreach (var kvp in result)
				{
					foreach (string item in new List<string>(kvp.Value))
					{
						if (IsNonTerminal(item))
						{
							done = false;
							kvp.Value.Remove(item);
							foreach (string f in result[item])
								kvp.Value.Add(f);

							break;
						}
					}
				}
			}
			return result;
		}
		/// <summary>
		/// Gets a unique id that is a variation of the passed in id. T would become T'
		/// </summary>
		/// <param name="id">The id to transform</param>
		/// <returns>A unique transform id</returns>
		private string _TransformId(string id)
		{
			string iid = id;
			var syms = FillSymbols();
			int i = 1;
			while (true)
			{
				string s = string.Concat(iid, "'");
				if (!syms.Contains(s))
					return s;
				++i;
				iid = string.Concat(id, i.ToString());
			}
		}
		/// <summary>
		/// Creates a parse table from the configuration
		/// </summary>
		/// <returns>A nested dictionary representing the parse table</returns>
		public IDictionary<string, IDictionary<string, ConfigurationRule>> ToParseTable()
		{
			// Here we populate the outer dictionary with one non-terminal for each key
			// we populate each inner dictionary with the result terminals and associated 
			// rules of the predict tables except in the case where the predict table 
			// contains null. In that case, we use the follows to get the terminals and 
			// the rule associated with the null predict in order to compute the inner 
			// dictionary
			var predict = FillPredict();
			var follows = FillFollows();
			var result = new Dictionary<string, IDictionary<string, ConfigurationRule>>();
			foreach (string nt in _EnumNonTerminals())
			{
				var d = new Dictionary<string, ConfigurationRule>();
				foreach ((ConfigurationRule Rule, string Symbol) f in predict[nt])
					if (null != f.Symbol)
						d.Add(f.Symbol, f.Rule);
					else
					{
						var ff = follows[nt];
						foreach (string fe in ff)
							d.Add(fe, f.Rule);
					}
				
				result.Add(nt, d);
			}
			return result;
		}
	}
}

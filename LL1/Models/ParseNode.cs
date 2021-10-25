using System;
using System.Collections.Generic;
using System.Text;

namespace LL1
{
	/// <summary>
	/// Represents a node of a parse tree
	/// </summary>
	internal class ParseNode
	{
		private int _line;
		private int _column;
		private long _position;

		/// <summary>
		/// Gets every descendent of this node and itself
		/// </summary>
		/// <param name="result">The collection to fill</param>
		/// <returns>The <paramref name="result"/> or a new collection, filled with the results</returns>
		public IList<ParseNode> FillDescendantsAndSelf(IList<ParseNode> result = null)
		{
			if (null == result) result = new List<ParseNode>();
			result.Add(this);
			int ic = Children.Count;
			for (int i = 0; i < ic; ++i)
				Children[i].FillDescendantsAndSelf(result);
			return result;
		}
		internal void SetLocationInfo(int line, int column, long position)
		{
			_line = line;
			_column = column;
			_position = position;
		}
		public int Line {
			get
			{
				if (null == Value)
				{
					if (0 < Children.Count)
						return Children[0].Line;
					return 0;
				}

				return _line;
			}
		}
		public int Column {
			get
			{
				if (null == Value)
				{
					if (0 < Children.Count)
						return Children[0].Column;
					return 0;
				}

				return _column;
			}
		}
		public long Position {
			get
			{
				if (null == Value)
				{
					if (0 < Children.Count)
						return Children[0].Position;
					return 0;
				}

				return _position;
			}
		}

		public int Length {
			get
			{
				if (null == Value)
				{
					if (0 < Children.Count)
					{
						int c = Children.Count - 1;
						long p = Children[c].Position;
						int l = Children[c].Length;
						return (int)(p - Position) + l;
					}
					return 0;
				}

				return Value.Length;
			}
		}

		public string Symbol { get; set; }
		public string Value { get; set; }
		
		public IList<ParseNode> Children { get; } = new List<ParseNode>();

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			_AppendTreeTo(sb, this);
			return sb.ToString();
		}

		private static void _AppendTreeTo(StringBuilder result, ParseNode node)
		{
			// adapted from https://stackoverflow.com/questions/1649027/how-do-i-print-out-a-tree-structure
			List<ParseNode> firstStack = new List<ParseNode>();
			firstStack.Add(node);

			List<List<ParseNode>> childListStack = new List<List<ParseNode>>();
			childListStack.Add(firstStack);

			while (childListStack.Count > 0)
			{
				List<ParseNode> childStack = childListStack[childListStack.Count - 1];

				if (childStack.Count == 0)
				{
					childListStack.RemoveAt(childListStack.Count - 1);
				}
				else
				{
					node = childStack[0];
					childStack.RemoveAt(0);

					string indent = "";
					for (int i = 0; i < childListStack.Count - 1; i++)
					{
						indent += childListStack[i].Count > 0 ? "|  " : "   ";
					}
					string s = node.Symbol;
					result.Append(string.Concat(indent, "+- ", s, " ", node.Value ?? "").TrimEnd());
					result.AppendLine();// string.Concat(" at line ", node.Line, ", column ", node.Column, ", position ", node.Position, ", length of ", node.Length));
					if (node.Children.Count > 0)
					{
						childListStack.Add(new List<ParseNode>(node.Children));
					}
				}
			}
		}
	}
}

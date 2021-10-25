using System;
using System.Collections.Generic;
using System.Text;

namespace LL1
{
	/// <summary>
	/// Represents a rule in a CFG
	/// A rule takes the form of Left -> Right1 Right2 ... RightN
	/// </summary>
	/// <remarks>This class implements value semantics</remarks>
	internal class ConfigurationRule : IEquatable<ConfigurationRule>
	{
		public string Left { get; } = null;
		public IList<string> Right { get; } = new List<string>();
		public ConfigurationRule(string left,IEnumerable<string> right) { Left = left; if(null!=Right) Right = new List<string>(right); }
		public ConfigurationRule(string left, params string[] right) : this(left,(IEnumerable<string>)right) { }

		public ConfigurationRule() { }
		
		public bool IsNull => Right != null && 0==Right.Count;

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(Left ?? "");
			sb.Append(" ->");
			int ic = Right.Count;
			for(int i = 0;i < ic;++i)
			{
				sb.Append(" ");
				sb.Append(Right[i]);
			}
			return sb.ToString();
		}
		public bool Equals(ConfigurationRule rhs)
		{
			if (ReferenceEquals(this, rhs)) return true;
			if (ReferenceEquals(null,rhs)) return false;
			if (!Equals(Left, rhs.Left)) return false;
			if (Right.Count != rhs.Right.Count) return false;
			
			for(int ic = Right.Count,i=0;i<ic;++i)
				if (!Equals(Right[i], rhs.Right[i]))
					return false;
			return true;
		}
		public override bool Equals(object obj)
			=> Equals(obj as ConfigurationRule);
		public override int GetHashCode()
		{
			int result = 0;
			if (null != Left)
				result ^= Left.GetHashCode();
			for (int ic = Right.Count, i = 0; i < ic; ++i) {
				string r = Right[i];
				if (null != r) result ^= r.GetHashCode();
			}
			return result;
		}
		public static bool operator==(ConfigurationRule lhs,ConfigurationRule rhs)
		{
			if (ReferenceEquals(lhs, rhs))
				return true;
			if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
				return false;
			return lhs.Equals(rhs);
		}
		public static bool operator !=(ConfigurationRule lhs, ConfigurationRule rhs)
		{
			if (ReferenceEquals(lhs, rhs))
				return false;
			if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
				return true;
			return !lhs.Equals(rhs);
		}
	}
}

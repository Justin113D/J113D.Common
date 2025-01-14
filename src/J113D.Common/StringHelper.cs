using System;
using System.Text;

namespace J113D.Common
{
	/// <summary>
	/// Various string helper methods
	/// </summary>
	public static class StringHelper
	{
		/// <summary>
		/// Escape a string
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string Escape(this string value)
		{
			if(string.IsNullOrEmpty(value))
			{
				return value;
			}

			StringBuilder sb = new(value.Length);

			foreach(char ch in value)
			{
				switch(ch)
				{
					case '\\':
						sb.Append(@"\\");
						break;
					case '\t':
						sb.Append(@"\t");
						break;
					case '\n':
						sb.Append(@"\n");
						break;
					case '\r':
						sb.Append(@"\r");
						break;
					default:
						sb.Append(ch);
						break;
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Unescape a string
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <exception cref="InvalidCastException"></exception>
		public static string Unescape(this string value)
		{
			if(string.IsNullOrEmpty(value))
			{
				return value;
			}

			StringBuilder sb = new(value.Length);

			bool startEscape = false;

			foreach(char ch in value)
			{
				if(ch == '\\' && !startEscape)
				{
					startEscape = true;
				}
				else if(startEscape)
				{
					switch(ch)
					{
						case '\\':
							sb.Append('\\');
							break;
						case 't':
							sb.Append('\t');
							break;
						case 'n':
							sb.Append('\n');
							break;
						case 'r':
							sb.Append('\r');
							break;
						default:
							throw new InvalidCastException("\\" + ch + " is not a valid escape character!");
					}

					startEscape = false;
				}
				else
				{
					sb.Append(ch);
				}
			}

			return sb.ToString();
		}
	}
}
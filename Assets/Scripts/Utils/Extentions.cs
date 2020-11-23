using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amatsugu.Phos
{
	public static class Extentions
	{
		public static StringBuilder ToNumberString(this float value, string positiveColor = "#00dd00", string negativeColor = "#dd0000")
		{
			var sb = new StringBuilder();
			if(value > 0)
			{
				sb.Append("<color=");
				sb.Append(positiveColor);
				sb.Append(">");
				sb.Append("+");
				sb.Append(value);
				sb.Append("</color>");
			}else
			{
				sb.Append("<color=");
				sb.Append(positiveColor);
				sb.Append(">");
				sb.Append(value);
				sb.Append("</color>");
			}
			return sb;
		}
		public static StringBuilder ToNumberString(this int value, string positiveColor = "#00dd00", string negativeColor = "#dd0000")
		{
			var sb = new StringBuilder();
			if (value > 0)
			{
				sb.Append("<color=");
				sb.Append(positiveColor);
				sb.Append(">");
				sb.Append("+");
				sb.Append(value);
				sb.Append("</color>");
			}
			else
			{
				sb.Append("<color=");
				sb.Append(positiveColor);
				sb.Append(">");
				sb.Append(value);
				sb.Append("</color>");
			}
			return sb;
		}
	}
}

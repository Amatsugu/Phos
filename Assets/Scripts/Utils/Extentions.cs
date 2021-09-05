using System.Collections.Generic;
using System.Text;

using Unity.Entities;

namespace Amatsugu.Phos
{
	public static class Extentions
	{
		public static StringBuilder ToNumberString(this float value, string positiveColor = "#00dd00", string negativeColor = "#dd0000", float pivot = 0, bool invert = false)
		{
			var sb = new StringBuilder();
			if (invert ? value < pivot : value > pivot)
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
				sb.Append(negativeColor);
				sb.Append(">");
				sb.Append(value);
				sb.Append("</color>");
			}
			return sb;
		}

		public static StringBuilder ToNumberString(this int value, string positiveColor = "#00dd00", string negativeColor = "#dd0000", int pivot = 0, bool invert = false)
		{
			var sb = new StringBuilder();
			if (invert ? value < pivot : value > pivot)
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
				sb.Append(negativeColor);
				sb.Append(">");
				sb.Append(value);
				sb.Append("</color>");
			}
			return sb;
		}

		public static void AppendResource(this Dictionary<int, ResourceProduction> dict, ResourceIndentifier resource)
		{
			if (dict.ContainsKey(resource.id))
			{
				var rate = dict[resource.id];
				rate.rate += resource.ammount;
				dict[resource.id] = rate;
			}
			else
				dict.Add(resource.id, new(resource.id, resource.ammount));
		}

		public static void AppendResource(this Dictionary<int, ResourceConsumption> dict, ResourceIndentifier resource)
		{
			if (dict.ContainsKey(resource.id))
			{
				var rate = dict[resource.id];
				rate.rate += resource.ammount;
				dict[resource.id] = rate;
			}
			else
				dict.Add(resource.id, new(resource.id, resource.ammount));
		}

		public static ResourceIndentifier[] ToIdentifiers(this DynamicBuffer<ResourceConsumption> resources)
		{
			var result = new ResourceIndentifier[resources.Length];
			for (int i = 0; i < resources.Length; i++)
				result[i] = new(resources[i].resourceId, resources[i].rate);
			return result;
		}

		public static ResourceIndentifier[] ToIdentifiers(this DynamicBuffer<ResourceProduction> resources)
		{
			var result = new ResourceIndentifier[resources.Length];
			for (int i = 0; i < resources.Length; i++)
				result[i] = new(resources[i].resourceId, resources[i].rate);
			return result;
		}
	}
}
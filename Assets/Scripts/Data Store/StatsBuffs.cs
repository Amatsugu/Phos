using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Amatsugu.Phos;

using Unity.Entities.UniversalDelegates;

using UnityEngine;

namespace Amatsugu.Phos.DataStore
{
	[Serializable]
	public struct StatsBuffs
	{
		public float structureRange;
		public float unitRange;
		public float structureAttack;
		public float unitAttack;
		public float structureHealth;
		public float unitHealth;
		public float buildSpeedMulti;
		public float buildCostMulti;
		public float consumptionMulti;
		public float productionMulti;

		public static StatsBuffs Default => new()
		{
			consumptionMulti = 1,
			productionMulti = 1,
			buildCostMulti = 1,
			buildSpeedMulti = 1
		};

		public static StatsBuffs Empty => new();

		public static StatsBuffs operator +(StatsBuffs left, StatsBuffs right)
		{
			return new StatsBuffs
			{
				structureHealth = left.structureHealth + right.structureHealth,
				structureRange = left.structureRange + right.structureRange,
				structureAttack = left.structureAttack + right.structureAttack,


				unitHealth = left.unitHealth + right.unitHealth,
				unitRange = left.unitRange + right.unitRange,
				unitAttack = left.unitAttack + right.unitAttack,

				buildCostMulti = left.buildCostMulti + right.buildCostMulti,
				buildSpeedMulti = left.buildSpeedMulti + right.buildSpeedMulti,
				consumptionMulti = left.consumptionMulti + right.consumptionMulti,
				productionMulti = left.productionMulti + right.productionMulti,
			};
		}

		public static StatsBuffs operator -(StatsBuffs left, StatsBuffs right)
		{
			return new StatsBuffs
			{
				structureHealth = left.structureHealth - right.structureHealth,
				structureAttack = left.structureAttack - right.structureAttack,
				structureRange = left.structureRange - right.structureRange,

				unitHealth = left.unitHealth - right.unitHealth,
				unitAttack = left.unitAttack - right.unitAttack,
				unitRange = left.unitRange - right.unitRange,

				buildCostMulti = left.buildCostMulti - right.buildCostMulti,
				buildSpeedMulti = left.buildSpeedMulti - right.buildSpeedMulti,
				consumptionMulti = left.consumptionMulti - right.consumptionMulti,
				productionMulti = left.productionMulti - right.productionMulti,
			};
		}

		public override string ToString()
		{
			return ToString(1);
		}

		public string ToString(float multiPivot = 1, bool additive = false)
		{
			var sb = new StringBuilder();

			if (structureHealth != default)
				sb.AppendLine($"Structure Health: {((additive && structureHealth >= 0) ? "+" :"")}{structureHealth.ToNumberString()}");
			if (structureRange != default)
				sb.AppendLine($"Structure Range: {((additive && structureRange >= 0) ? "+" : "")}{structureRange.ToNumberString()}");
			if (structureAttack != default)
				sb.AppendLine($"Structure Attack: {((additive && structureAttack >= 0) ? "+" : "")}{structureAttack.ToNumberString()}");

			if (unitHealth != default)
				sb.AppendLine($"Unit Health: {((additive && unitHealth >= 0) ? "+" : "")}{unitHealth.ToNumberString()}");
			if (unitRange != default)
				sb.AppendLine($"Unit Range: {((additive && unitRange >= 0) ? "+" : "")}{unitRange.ToNumberString()}");
			if (unitAttack != default)
				sb.AppendLine($"Unit Attack: {((additive && unitAttack >= 0) ? "+" : "")}{unitAttack.ToNumberString()}");

			if (buildCostMulti != multiPivot)
				sb.AppendLine($"Build Cost: {((additive && buildCostMulti >= 0) ? "+" : "")}{buildCostMulti.ToNumberString(pivot: multiPivot, invert: true)}");
			if (buildSpeedMulti != multiPivot)
				sb.AppendLine($"Build Speed: {((additive && buildSpeedMulti >= 0) ? "+" : "")}{buildSpeedMulti.ToNumberString(pivot: multiPivot)}");
			if (consumptionMulti != multiPivot)
				sb.AppendLine($"Resource Consumtion: {((additive && consumptionMulti >= 0) ? "+" : "")}{consumptionMulti.ToNumberString(pivot: multiPivot)}");
			if (productionMulti != multiPivot)
				sb.AppendLine($"Resource Production: {((additive && productionMulti >= 0) ? "+" : "")}{productionMulti.ToNumberString(pivot: multiPivot)}");

			return sb.ToString();
		}

	}
}

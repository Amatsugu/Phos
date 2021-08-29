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

		public string ToString(float multiPivot = 1)
		{
			var sb = new StringBuilder();

			if (structureHealth != default)
				sb.AppendLine($"Structure Health: {structureHealth.ToNumberString()}");
			if (structureRange != default)
				sb.AppendLine($"Structure Range: {structureRange.ToNumberString()}");
			if (structureAttack != default)
				sb.AppendLine($"Structure Attack: {structureAttack.ToNumberString()}");

			if (unitHealth != default)
				sb.AppendLine($"Unit Health: {unitHealth.ToNumberString()}");
			if (unitRange != default)
				sb.AppendLine($"Unit Range: {unitRange.ToNumberString()}");
			if (unitAttack != default)
				sb.AppendLine($"Unit Attack: {unitAttack.ToNumberString()}");

			if (buildCostMulti != multiPivot)
				sb.AppendLine($"Build Cost: {buildCostMulti.ToNumberString(pivot: 1)}x");
			if (buildSpeedMulti != multiPivot)
				sb.AppendLine($"Build Speed: {buildSpeedMulti.ToNumberString(pivot: 1)}x");
			if (consumptionMulti != multiPivot)
				sb.AppendLine($"Resource Consumtion: {consumptionMulti.ToNumberString(pivot: 1)}x");
			if (productionMulti != multiPivot)
				sb.AppendLine($"Resource Production: {productionMulti.ToNumberString(pivot: 1)}x");

			return sb.ToString();
		}

	}
}

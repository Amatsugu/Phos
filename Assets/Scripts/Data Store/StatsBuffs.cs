using System;
using System.Collections;
using System.Collections.Generic;

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


		public static StatsBuffs operator +(StatsBuffs left, StatsBuffs right)
		{
			return new StatsBuffs
			{
				structureHealth = left.structureHealth + right.structureHealth,
				unitHealth = left.unitHealth + right.unitHealth,
				structureRange = left.structureRange + right.structureRange,
				unitRange = left.unitRange + right.unitRange,
				buildCostMulti = left.buildCostMulti + right.buildCostMulti,
				buildSpeedMulti = left.buildSpeedMulti + right.buildSpeedMulti,
				consumptionMulti = left.consumptionMulti + right.consumptionMulti,
				productionMulti = left.productionMulti + right.productionMulti,
				structureAttack = left.structureAttack + right.structureAttack,
				unitAttack = left.unitAttack + right.unitAttack
			};
		}

		public static StatsBuffs operator -(StatsBuffs left, StatsBuffs right)
		{
			return new StatsBuffs
			{
				structureHealth = left.structureHealth - right.structureHealth,
				unitHealth = left.unitHealth - right.unitHealth,
				structureRange = left.structureRange - right.structureRange,
				unitRange = left.unitRange - right.unitRange,
				buildCostMulti = left.buildCostMulti - right.buildCostMulti,
				buildSpeedMulti = left.buildSpeedMulti - right.buildSpeedMulti,
				consumptionMulti = left.consumptionMulti - right.consumptionMulti,
				productionMulti = left.productionMulti - right.productionMulti,
				structureAttack = left.structureAttack - right.structureAttack,
				unitAttack = left.unitAttack - right.unitAttack
			};
		}
	}
}

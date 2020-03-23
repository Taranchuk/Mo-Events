using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace MoreIncidents
{
	public class Pawn_theThing : Pawn
	{
		public Pawn_theThing()
		{

        }

		public override void ExposeData()
		{
			base.ExposeData();
		}

		public override void Tick()
		{
			bool flag = Find.TickManager.TicksGame % 250 == 0;
			bool flag2 = flag;
			if (flag2)
			{
				this.TickRare();
			}
			this.mindState.mentalStateHandler.neverFleeIndividual = true;
			bool flag3 = !this.stances.FullBodyBusy;
			bool flag4 = flag3;
			if (flag4)
			{
				try
				{
					this.pather.PatherTick();
				}
				catch (Exception)
				{
				}
			}
			base.Drawer.DrawTrackerTick();
			this.health.HealthTick();
			this.stances.StanceTrackerTick();
			bool flag5 = this.equipment != null;
			bool flag6 = flag5;
			if (flag6)
			{
				this.equipment.EquipmentTrackerTick();
			}
			bool flag7 = this.apparel != null;
			bool flag8 = flag7;
			if (flag8)
			{
				this.apparel.ApparelTrackerTick();
			}
			bool flag9 = this.jobs != null;
			bool flag10 = flag9;
			if (flag10)
			{
				this.jobs.JobTrackerTick();
			}
			bool flag11 = this.health.hediffSet.HasTendableInjury();
			bool flag12 = flag11;
			if (flag12)
			{
				this.TickHeal(true);
			}
			bool flag13 = !base.Dead || !base.Downed;
			bool flag14 = flag13;
			if (flag14)
			{
				int num = this.ticksSinceStatusUpdate;
				this.ticksSinceStatusUpdate = num + 1;
				bool flag15 = this.ticksSinceStatusUpdate >= this.updateStatusEveryXTicks;
				bool flag16 = flag15;
				if (flag16)
				{
					IntVec3 intVec = CellFinder.RandomClosewalkCellNear(base.Position, base.Map, 1);
					ThingDef named = DefDatabase<ThingDef>.GetNamed("Filth_Blood", true);
					FilthMaker.TryMakeFilth(intVec, base.Map, named, 1);
					this.ticksSinceStatusUpdate = 0;
				}
			}
			bool downed = base.Downed;
			bool flag17 = downed;
			if (flag17)
			{
				IntVec3 intVec2 = CellFinder.RandomClosewalkCellNear(base.Position, base.Map, 2);
				ThingDef named2 = DefDatabase<ThingDef>.GetNamed("Filth_Blood", true);
				FilthMaker.TryMakeFilth(intVec2, base.Map, named2, 3);
                HealthUtility.DamageUntilDead(this);
			}
		}

		private void TickHeal(bool allowMiracles)
		{
			int num = this.ticksToNextRepair;
			this.ticksToNextRepair = num - 1;
			bool flag = num > 0;
			bool flag2 = !flag;
			if (flag2)
			{
				this.ticksToNextRepair = 15;
				bool flag3 = allowMiracles && this.def.repairEffect != null;
				bool flag4 = flag3;
				if (flag4)
				{
					this.def.repairEffect.Spawn();
				}
				Hediff[] hediffs = this.GetHediffs(allowMiracles);
				bool flag5 = hediffs.Length == 0;
				bool flag6 = !flag5;
				if (flag6)
				{
					Hediff hediff = GenCollection.RandomElement<Hediff>(hediffs);
					float severity = hediff.Severity;
					bool tendableNow = hediff.TendableNow();
					bool flag7 = tendableNow;
					if (flag7)
					{
						Mo_Utility.HealHediff(this, hediff, 1);
						bool flag8 = allowMiracles && Rand.Value < 0.05f;
						bool flag9 = flag8;
						if (flag9)
						{
							this.health.RemoveHediff(hediff);
						}
						else
						{
							Mo_Utility.HealHediff(this, hediff, 1);
						}
					}
				}
			}
		}

		private Hediff[] GetHediffs(bool allowMiracles)
		{
			bool flag = !allowMiracles;
			bool flag2 = flag;
			Hediff[] result;
			if (flag2)
			{
				IEnumerable<Hediff> hediffs = this.health.hediffSet.hediffs;
				Func<Hediff, bool> predicate;
				bool flag3 = (predicate = Pawn_theThing.thing.isInjury) == null;
				if (flag3)
				{
					predicate = (Pawn_theThing.thing.isInjury = new Func<Hediff, bool>(Pawn_theThing.thing.IncidentThing.getHediffsInjury));
				}
				result = hediffs.Where(predicate).ToArray<Hediff>();
			}
			else
			{
				IEnumerable<Hediff> hediffs2 = this.health.hediffSet.hediffs;
				Func<Hediff, bool> predicate2;
				bool flag4 = (predicate2 = Pawn_theThing.thing.isHeal) == null;
				if (flag4)
				{
					predicate2 = (Pawn_theThing.thing.isHeal = new Func<Hediff, bool>(Pawn_theThing.thing.IncidentThing.getHediffsHeal));
				}
				result = hediffs2.Where(predicate2).ToArray<Hediff>();
			}
			return result;
		}

		private int updateStatusEveryXTicks = 45;

		private int ticksSinceStatusUpdate;

		private int ticksToNextRepair;

		private const int TicksBetweenRepairs = 15;

		private sealed class thing
		{
			public bool getHediffsInjury(Hediff h)
			{
				return h is Hediff_Injury;
			}

            public bool getHediffsHeal(Hediff h)
			{
				return !(h is Hediff_AddedPart);
			}

			public static readonly Pawn_theThing.thing IncidentThing = new Pawn_theThing.thing();

			public static Func<Hediff, bool> isInjury;

			public static Func<Hediff, bool> isHeal;
		}
	}
}


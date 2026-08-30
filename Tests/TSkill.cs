using System.Collections.Generic;

using NUnit.Framework;

using SpaceAge;



namespace UnitTests

{

	[TestFixture]

	public class TSkill : TTest

	{

		[SetUp]

		public void setupSkill()

		{

			this.LoadDefaultGame();

		}



		[TearDown]

		public void teardownSkill()

		{

			this.ClearGame();

		}



		[Test]

		public void SkillType_LoadsUsableInAndProduce()

		{

			SkillType frgplt = SkillType.All["frgplt"];

			SkillType arpldr = SkillType.All["arpldr"];

			SkillType hmedic = SkillType.All["hmedic"];

			SkillType snsroff = SkillType.All["snsroff"];



			Assert.That(frgplt.UsableIn.Count, Is.GreaterThanOrEqualTo(1));

			Assert.That(frgplt.UsableIn[0].ModuleGroup, Is.EqualTo(EModuleTypesGroup.frigate));

			Assert.That(frgplt.UsableIn[0].ModuleStackSize, Is.EqualTo(1));

			Assert.That(arpldr.ProduceEffect, Is.EqualTo("effective attack"));

			Assert.That(arpldr.ProduceTarget, Is.EqualTo("units"));

			Assert.That(arpldr.ProduceFormula.IsNonZero, Is.True);

			Assert.That(hmedic.CureChanceFormula.IsNonZero, Is.True);

			Assert.That(snsroff.ProduceEffect, Is.EqualTo("research output"));

			Assert.That(snsroff.ProduceFormula.IsNonZero, Is.True);

			Assert.That(snsroff.UsableIn.Count, Is.GreaterThanOrEqualTo(1));

			Assert.That(SkillType.All["ntrplt"].UsableIn.Count, Is.GreaterThanOrEqualTo(2));

			Assert.That(SkillType.All.Count, Is.EqualTo(16));

		}



		[Test]

		public void SkillFormula_LegacyIntegerStillParses()

		{

			SkillBonusFormula formula = SkillBonusFormula.Parse("5");

			Assert.That(formula.Evaluate(null, null), Is.EqualTo(5));

		}



		[Test]

		public void CombatBonus_FrgpltOnFrigate_Applies()

		{

			ModuleStack frigate = ModuleStack.All["100011"];

			ModuleStack bridge = ModuleStack.All["100012"];

			Person pilot = Person.All["200002"];



			Assert.That(bridge.People.CombatDefense(frigate.RootModuleStack), Is.EqualTo(5));

			Assert.That(bridge.People.CombatInitiative(frigate.RootModuleStack), Is.EqualTo(5));

			Assert.That(pilot.CombatDefense(frigate.RootModuleStack), Is.EqualTo(5));

			Assert.That(pilot.CombatInitiative(frigate.RootModuleStack), Is.EqualTo(5));

		}



		[Test]

		public void CombatBonus_FrgpltOnInfantry_DoesNotApply()

		{

			Region region = Region.All["R00002"];

			Faction owner = Faction.All["2"];

			ModuleStack infantry = new ModuleStack(region, owner, ModuleType.All["inftry"], "skinf1");

			Person officer = new Person(infantry, owner, Race.All["terran"], "skoff1");

			officer.Skills.Add(SkillType.All["frgplt"]);



			Assert.That(officer.CombatDefense(infantry.RootModuleStack), Is.EqualTo(0));

			Assert.That(officer.CombatInitiative(infantry.RootModuleStack), Is.EqualTo(0));

			Assert.That(infantry.People.CombatDefense(infantry.RootModuleStack), Is.EqualTo(0));

		}



		[Test]

		public void CombatBonus_ArpldrOnCorphq_DoesNotApply()

		{

			ModuleStack headquarters = ModuleStack.All["000112"];

			Person ceo = Person.All["000101"];

			ceo.Skills.Add(SkillType.All["arpldr"]);



			Assert.That(ceo.CombatAttack(headquarters.RootModuleStack), Is.EqualTo(0));

			Assert.That(ceo.CombatInitiative(headquarters.RootModuleStack), Is.EqualTo(0));

			Assert.That(headquarters.Attack, Is.EqualTo(0));

		}



		[Test]

		public void CombatBonus_ArpldrOnVehicle_AddsFlatAndProducePerUnit()

		{

			Region region = Region.All["R00002"];

			Faction owner = Faction.All["2"];

			ModuleStack tanks = new ModuleStack(region, owner, ModuleType.All["tanks"], "skveh1");

			tanks.AddModules(3);

			Person officer = new Person(tanks, owner, Race.All["terran"], "skoff2");

			officer.Skills.Add(SkillType.All["arpldr"]);



			Assert.That(officer.CombatAttack(tanks.RootModuleStack), Is.EqualTo(8));

		}



		[Test]

		public void CombatBonus_InbtcmOnInfantry_AddsProduceDefencePerUnit()

		{

			Region region = Region.All["R00002"];

			Faction owner = Faction.All["2"];

			ModuleStack infantry = new ModuleStack(region, owner, ModuleType.All["inftry"], "skinf2");

			infantry.AddModules(4);

			Person commander = new Person(infantry, owner, Race.All["terran"], "skoff3");

			commander.Skills.Add(SkillType.All["inbtcm"]);



			Assert.That(commander.CombatAttack(infantry.RootModuleStack), Is.EqualTo(5));

			Assert.That(commander.CombatDefense(infantry.RootModuleStack), Is.EqualTo(4));

		}



		[Test]

		public void CombatBonus_StackAttackFormula_ScalesWithFormation()

		{

			Region region = Region.All["R00002"];

			Faction owner = Faction.All["2"];

			ModuleStack tanks = new ModuleStack(region, owner, ModuleType.All["tanks"], "skveh2");

			tanks.AddModules(10);

			Person officer = new Person(tanks, owner, Race.All["terran"], "skoff5");

			officer.Skills.Add(SkillType.All["arpldr"]);



			Assert.That(SkillFormationStats.FormationBaseAttack(tanks), Is.EqualTo(40));

			Assert.That(officer.CombatAttack(tanks.RootModuleStack), Is.EqualTo(22));

		}



		[Test]

		public void CombatBonus_AstrogOnCommandBridge_AppliesViaHost()

		{

			ModuleStack frigate = ModuleStack.All["100011"];

			ModuleStack bridge = ModuleStack.All["100012"];

			Person navigator = new Person(bridge, Faction.All["2"], Race.All["terran"], "sknav1");

			navigator.Skills.Add(SkillType.All["astrog"]);



			Assert.That(navigator.CombatInitiative(frigate.RootModuleStack), Is.GreaterThan(0));

		}



		[Test]

		public void CombatBonus_GunnryOnMilitaryModule_AppliesViaHost()

		{

			Region region = Region.All["R00002"];

			Faction owner = Faction.All["2"];

			ModuleStack tanks = new ModuleStack(region, owner, ModuleType.All["tanks"], "skveh3");

			tanks.AddModule();

			ModuleStack weapon = new ModuleStack(tanks, owner, ModuleType.All["gunplc"], "skgun1");

			weapon.AddModule();

			Person gunner = new Person(weapon, owner, Race.All["terran"], "skgun2");

			gunner.Skills.Add(SkillType.All["gunnry"]);



			Assert.That(gunner.CombatAttack(tanks.RootModuleStack), Is.GreaterThan(0));

		}



		[Test]

		public void ResearchBonus_SnsroffOnResearchLab_AddsOutputPerOfficer()

		{

			Region region = Region.All["R00002"];

			Faction owner = Faction.All["2"];

			ModuleStack lab = new ModuleStack(region, owner, ModuleType.All["cmplib"], "sklab1");

			lab.AddModule();

			Person officer = new Person(lab, owner, Race.All["terran"], "skoff4");

			officer.Skills.Add(SkillType.All["snsroff"]);



			Assert.That(officer.ResearchOutputBonus(lab), Is.EqualTo(1));

			Assert.That(lab.ResearchSkillOutputBonus, Is.EqualTo(1));

			Assert.That(Research.WeeklyOutput(lab), Is.EqualTo(2));

		}



		[Test]

		public void ResearchBonus_XenbioOnResearchLab_AddsOutput()

		{

			Region region = Region.All["R00002"];

			Faction owner = Faction.All["2"];

			ModuleStack lab = new ModuleStack(region, owner, ModuleType.All["cmplib"], "sklab2");

			lab.AddModule();

			Person biologist = new Person(lab, owner, Race.All["terran"], "skbio1");

			biologist.Skills.Add(SkillType.All["xenbio"]);



			Assert.That(biologist.ResearchOutputBonus(lab), Is.EqualTo(1));

		}



		[Test]

		public void CureChance_HmedicIsFlatTwentyFive()
		{
			ModuleStack frigate = ModuleStack.All["100011"];
			Person medic = new Person(frigate, Faction.All["2"], Race.All["terran"], "skmed2");
			medic.Skills.Add(SkillType.All["hmedic"]);

			Assert.That(medic.CureChance(frigate.RootModuleStack), Is.EqualTo(25));
		}



		[Test]

		public void CureChance_RadmedAndHmedic_UsesMax()

		{

			Region region = Region.All["R00002"];

			Faction owner = Faction.All["2"];

			ModuleStack ward = new ModuleStack(region, owner, ModuleType.All["medfac"], "skward");

			ward.AddModule();

			Person radDoc = new Person(ward, owner, Race.All["terran"], "skrad1");

			radDoc.Skills.Add(SkillType.All["radmed"]);

			Person medic = new Person(ward, owner, Race.All["terran"], "skmed3");

			medic.Skills.Add(SkillType.All["hmedic"]);



			int max = System.Math.Max(

				radDoc.CureChance(ward.RootModuleStack),

				medic.CureChance(ward.RootModuleStack));

			Assert.That(ward.MedicalCureChance, Is.EqualTo(25));

		}

		[Test]
		public void SkillPercentProduce_AstrogReducesMoveDuration()
		{
			ModuleStack frigate = ModuleStack.All["100011"];
			ModuleStack bridge = ModuleStack.All["100012"];
			Person navigator = new Person(bridge, Faction.All["2"], Race.All["terran"], "sknav2");
			navigator.Skills.Add(SkillType.All["astrog"]);

			Assert.That(SkillEffects.MoveDurationPercent(frigate), Is.EqualTo(90));
			Assert.That(SkillEffects.ApplyDurationPercent(10, 90), Is.EqualTo(9));
			Assert.That(SkillEffects.ApplyDurationPercent(3, 90), Is.EqualTo(3));
		}

		[Test]
		public void SkillPercentProduce_ChfengReducesProductionDuration()
		{
			Region region = Region.All["R00002"];
			Faction owner = Faction.All["2"];
			ModuleStack factory = new ModuleStack(region, owner, ModuleType.All["factry"], "skfact");
			factory.AddModule();
			Person engineer = new Person(factory, owner, Race.All["terran"], "skeng1");
			engineer.Skills.Add(SkillType.All["chfeng"]);

			Assert.That(SkillEffects.ProductionDurationPercent(factory), Is.EqualTo(75));
			Assert.That(SkillEffects.ApplyDurationPercent(4, 75), Is.EqualTo(3));
		}

		[Test]
		public void SkillPercentProduce_LogoffIncreasesCapacity()
		{
			Region region = Region.All["R00002"];
			Faction owner = Faction.All["2"];
			ModuleStack warehouse = new ModuleStack(region, owner, ModuleType.All["cargob"], "skstor");
			warehouse.AddModule();
			Person quartermaster = new Person(warehouse, owner, Race.All["terran"], "sklog1");
			quartermaster.Skills.Add(SkillType.All["logoff"]);

			Assert.That(SkillEffects.CapacityPercent(warehouse), Is.EqualTo(110));
			Assert.That(warehouse.Capacity, Is.EqualTo(1980));
		}

		[Test]
		public void SkillPercentProduce_ExoagrIncreasesFarmOutput()
		{
			Region region = Region.All["R00002"];
			Faction owner = Faction.All["2"];
			ModuleStack farm = new ModuleStack(region, owner, ModuleType.All["farms"], "skfarm");
			farm.AddModule();
			Person agronomist = new Person(farm, owner, Race.All["terran"], "skagr1");
			agronomist.Skills.Add(SkillType.All["exoagr"]);

			Assert.That(SkillEffects.ItemOutputPercent(farm), Is.EqualTo(125));
			ItemStacks stacks = new ItemStacks();
			stacks.Add(new ItemStack(ItemType.All["food"], 10));
			SkillEffects.ApplyOutputPercent(stacks, 125);
			Assert.That(stacks[ItemType.All["food"]].Quantity, Is.EqualTo(12));
		}

		[Test]
		public void SkillPercentProduce_ExcoffIncreasesDrillOutput()
		{
			Region region = Region.All["R00002"];
			Faction owner = Faction.All["2"];
			ModuleStack drill = new ModuleStack(region, owner, ModuleType.All["cdrill"], "skdril");
			drill.AddModule();
			Person miner = new Person(drill, owner, Race.All["terran"], "skexc1");
			miner.Skills.Add(SkillType.All["excoff"]);

			Assert.That(SkillEffects.ItemOutputPercent(drill), Is.EqualTo(125));
		}

	}

}



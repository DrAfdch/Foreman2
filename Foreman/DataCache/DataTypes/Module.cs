﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Foreman
{
	public interface Module : DataObjectBase
	{
		IReadOnlyCollection<Recipe> Recipes { get; }
		IReadOnlyCollection<Assembler> Assemblers { get; }
		IReadOnlyCollection<Beacon> Beacons { get; }
		IReadOnlyCollection<Recipe> AvailableRecipes { get; }

		Item AssociatedItem { get; }

		double SpeedBonus { get; }
		double ProductivityBonus { get; }
		double ConsumptionBonus { get; }
		double PollutionBonus { get; }
		double QualityBonus { get; }

		string Category { get; }

		int Tier { get; }

		bool IsMissing { get; }
	}

	public class ModulePrototype : DataObjectBasePrototype, Module
	{
		public IReadOnlyCollection<Recipe> Recipes { get { return recipes; } }
		public IReadOnlyCollection<Assembler> Assemblers { get { return assemblers; } }
		public IReadOnlyCollection<Beacon> Beacons { get { return beacons; } }
		public IReadOnlyCollection<Recipe> AvailableRecipes { get; private set; }
		public Item AssociatedItem { get { return Owner.Items[Name]; } }

		public double SpeedBonus { get; internal set; }
		public double ProductivityBonus { get; internal set; }
		public double ConsumptionBonus { get; internal set; }
		public double PollutionBonus { get; internal set; }
		public double QualityBonus { get; internal set; }

		public string Category { get; internal set; }

		public int Tier { get; set; }

		public bool IsMissing { get; private set; }
		public override bool Available { get { return AssociatedItem.Available; } set { } }

		internal HashSet<RecipePrototype> recipes { get; private set; }
		internal HashSet<AssemblerPrototype> assemblers { get; private set; }
		internal HashSet<BeaconPrototype> beacons { get; private set; }

		public ModulePrototype(DataCache dCache, string name, string friendlyName, bool isMissing = false) : base(dCache, name, friendlyName, "-")
		{
			Enabled = true;
			IsMissing = isMissing;

			SpeedBonus = 0;
			ProductivityBonus = 0;
			ConsumptionBonus = 0;
			PollutionBonus = 0;
			QualityBonus = 0;

			Category = "";

			recipes = new HashSet<RecipePrototype>();
			assemblers = new HashSet<AssemblerPrototype>();
			beacons = new HashSet<BeaconPrototype>();
		}

		internal void UpdateAvailabilities()
		{
			AvailableRecipes = new HashSet<Recipe>(recipes.Where(r => r.Enabled));
		}

		public override string ToString() { return string.Format("Module: {0}", Name); }
	}
}

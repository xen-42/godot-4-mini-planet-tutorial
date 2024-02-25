using Godot;
using System;
using System.Linq;

public partial class SolarSystem : Node3D
{
	public static SolarSystem Instance { get; private set; }

	[Export] public Planet[] planets;

	public override void _Ready()
	{
		Instance = this;

		var orderedPlanets = planets.OrderBy(planet => planet.HowManyParents());
		foreach (var planet in orderedPlanets)
		{
			planet.Init();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}

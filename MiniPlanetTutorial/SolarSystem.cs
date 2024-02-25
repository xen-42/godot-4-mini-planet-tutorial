using Godot;
using System;
using System.Linq;

public partial class SolarSystem : Node3D
{
	[Export] public Planet[] planets;

	public override void _Ready()
	{
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

using Godot;
using System.Linq;

public partial class SolarSystem : Node3D
{
	public static SolarSystem Instance { get; private set; }

	public Planet[] Planets { get; private set; }

	public override void _Ready()
	{
		Instance = this;

		Planets = this.GetChildren().Where(x => x is Planet).Cast<Planet>().ToArray();

		var orderedPlanets = Planets.OrderBy(planet => planet.HowManyParents());
		foreach (var planet in orderedPlanets)
		{
			planet.Init();
		}
	}
}

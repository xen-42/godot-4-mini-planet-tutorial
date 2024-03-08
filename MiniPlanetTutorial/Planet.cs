using Godot;
using System;

public partial class Planet : StaticBody3D
{
	/// <summary>
	/// Our planet will orbit around this planet
	/// </summary>
	[ExportCategory("Orbit")]
	[Export] private Planet _orbitalParent;

	/// <summary>
	/// Our planet will orbit the _orbitalParent at this distance
	/// </summary>
	[Export] private float _orbitalRadius;

	/// <summary>
	/// Our planet will start at this angle within the orbital plane
	/// </summary>
	[Export] private float _orbitalAngle;

	/// <summary>
	/// Walkable surface, used to calculate gravity
	/// </summary>
	[ExportCategory("Gravity")]
	[Export] public float surfaceRadius;

	/// <summary>
	/// Gravity felt at _surfaceRadius in m/s^2
	/// </summary>
	[Export] private float _surfaceGravity;

	/// <summary>
	/// How long to complete on revolution around its axis
	/// </summary>
	[ExportCategory("Rotation")]
	[Export] private float _dayLength;

	/// <summary>
	/// Fix the day length to equal the orbital period
	/// </summary>
	[Export] private bool _tidallyLocked;

	/// <summary>
	/// Equal to GM
	/// </summary>
	public float StandardGravitationalParameter { get; private set; }

	public Vector3 AccelerationByGravity { get; private set; }

	public override void _Ready()
	{
		// Fg = (G m1 m2)/d^2
		// Fg = m1 a
		// m1 a = (G m1 m2)/d^2
		// a = G m2 / d^2
		// _surfaceGravity = G m2 / _surfaceRadius^2
		// G m2 = _surfaceGravity * _surfaceRadius^2
		StandardGravitationalParameter = _surfaceGravity * surfaceRadius * surfaceRadius;
	}

	/// <summary>
	/// Called after all planets are Ready
	/// Init must be called on the orbitalParent of a planet first
	/// </summary>
	public void Init()
	{
		if (_orbitalParent != null )
		{
			GlobalPosition = _orbitalParent.GlobalPosition + (Vector3.Forward * _orbitalRadius).Rotated(Vector3.Up, _orbitalAngle);
			ConstantLinearVelocity = _orbitalParent.ConstantLinearVelocity + _orbitalParent.GetOrbitalVelocity(GlobalPosition);
		}

		if (_tidallyLocked)
		{
			_dayLength = GetOrbitalPeriod();
		}

		ConstantAngularVelocity = -(_dayLength == 0 ? 0 : 2f * Mathf.Pi / _dayLength) * Vector3.Up;

		GD.Print($"Initialized {Name}");
	}

	/// <summary>
	/// Gets the acceleration caused by the force that this planet exerts at the point given
	/// </summary>
	/// <param name="globalPosition"></param>
	/// <returns></returns>
	public Vector3 GetAccelerationAtPosition(Vector3 globalPosition)
	{
		var d = globalPosition - this.GlobalPosition;
		return -d.Normalized() * StandardGravitationalParameter / d.LengthSquared();
	}

	/// <summary>
	/// Gives the velocity required to orbit this planet at the point given
	/// </summary>
	/// <param name="globalPosition"></param>
	/// <returns></returns>
	public Vector3 GetOrbitalVelocity(Vector3 globalPosition)
	{
		var d = globalPosition - this.GlobalPosition;
		var speed = Mathf.Sqrt(StandardGravitationalParameter / d.Length());
		var direction = d.Normalized().Cross(Vector3.Up);
		
		return direction * speed;
	}

	/// <summary>
	/// Gives the orbital period of this planet
	/// </summary>
	/// <returns></returns>
	public float GetOrbitalPeriod()
	{
		return 2f * Mathf.Pi * Mathf.Sqrt(Mathf.Pow(_orbitalRadius, 3) / _orbitalParent.StandardGravitationalParameter);
	}

	/// <summary>
	/// Gives how quickly a body is moving relative to the surface of this planet at a given position and with a given velocity
	/// </summary>
	/// <param name="globalPosition"></param>
	/// <param name="linearVelocity"></param>
	/// <returns></returns>
	public Vector3 GetRelativeVelocityToSurface(Vector3 globalPosition, Vector3 linearVelocity)
	{
		var planetSpeed = ConstantLinearVelocity;
		var rotationalSpeed = ConstantAngularVelocity.Cross(globalPosition - GlobalPosition);
		var relativeSpeed = linearVelocity - (planetSpeed + rotationalSpeed);

		return relativeSpeed;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_orbitalParent != null)
		{
			AccelerationByGravity = _orbitalParent.GetAccelerationAtPosition(GlobalPosition) + _orbitalParent.AccelerationByGravity;
			var acc = AccelerationByGravity;
			ConstantLinearVelocity += acc * (float)delta;
			GlobalPosition += ConstantLinearVelocity * (float)delta;
		}

		GlobalRotation += ConstantAngularVelocity * (float)delta;
	}

	/// <summary>
	/// Used to order planets depending on where they are in the hierarchy.
	/// Ex)
	/// Sun   - 0 parents
	/// Earth - 1 parent  (Sun)
	/// Mars  - 1 parent  (Sun)
	/// Moon  - 2 parents (Earth -> Sun)
	/// </summary>
	/// <returns></returns>
	public int HowManyParents()
	{
		if (_orbitalParent == null)
		{
			return 0;
		}
		else
		{
			return _orbitalParent.HowManyParents() + 1;
		}
	}
}

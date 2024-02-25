using Godot;
using System;

public partial class Player : RigidBody3D
{
	[ExportCategory("Nodes")]
	[Export] private Camera3D _camera;
	[Export] private Node3D _cameraPivot;


	[ExportCategory("Settings")]
	[Export] private float _mouseSensitivity = 0.3f;
	private Vector2 _mouseDelta;
	private float _cameraXRotation;

	private bool _inMap;

	public override void _Ready()
	{
		base._Ready();

		ShowMap(false);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (Input.IsActionJustReleased("Map"))
		{
			ShowMap(!_inMap);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		foreach (var planet in SolarSystem.Instance.planets)
		{
			var force = planet.GetForceAtPosition(GlobalPosition);
			ApplyCentralForce(force);
		}

		if (!_inMap)
		{
			ProcessMovementInputs(delta);
			ProcessLookInputs(delta);
		}

		_mouseDelta = Vector2.Zero;
	}

	private void ProcessMovementInputs(double delta)
	{

	}

	private void ProcessLookInputs(double delta)
	{
		// Look - mouse y is the rotation around the x axis and vice versa
		var deltaX = -_mouseDelta.Y * _mouseSensitivity;
		var deltaY = -_mouseDelta.X * _mouseSensitivity;

		if (Input.IsActionPressed("Rotate"))
		{
			Rotate(_cameraPivot.GlobalTransform.Basis.Column2, Mathf.DegToRad(deltaY));
		}
		else
		{
			RotateObjectLocal(Vector3.Up, Mathf.DegToRad(deltaY));
			if (_cameraXRotation + deltaX > -90 && _cameraXRotation + deltaX < 90)
			{
				_cameraPivot.RotateX(Mathf.DegToRad(deltaX));
				_cameraXRotation += deltaX;
			}
		}
	}

	public override void _Input(InputEvent e)
	{
		base._Input(e);

		if (e is InputEventMouseMotion)
		{
			// Add up mouse motions in the input method and then apply them in physics process
			var mouseMotion = e as InputEventMouseMotion;
			_mouseDelta += mouseMotion.Relative;
		}
	}

	private void ShowMap(bool inMap)
	{
		_inMap = inMap;

		if (_inMap)
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
			_camera.Current = false;
		}
		else
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
			_camera.Current = true;
		}
	}
}

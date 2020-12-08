using Godot;
using System;

public class Taskbar : Control {
	[Export]
	public NodePath cashNode, clockNode;
	public Label cash, clock;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		clock = (Label)GetNode(clockNode);
		cash = (Label)GetNode(cashNode);
	}

	public override void _Process(float delta) {
		clock.Text = $"{GameController.CurrentTime.GetAsShortDayOfWeek()}   {GameController.CurrentTime.Hours.ToString("D2")}:{GameController.CurrentTime.Minutes.ToString("D2")}";
	}
}

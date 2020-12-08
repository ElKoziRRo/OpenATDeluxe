using Godot;
using System;

public class MouseArea : Area2D, IInteractionLayer {
	[Export]
	public bool isExitToAirport = false;

	[Export]
	public bool ignoreInteractionLock = false;

	public Action onClick;
	public Node2D area;

	public virtual int Layer => (int)BaseLayer.MouseArea;

	public override void _Ready() {
		if (GetChildCount() != 0)
			area = (Node2D)GetChild(0);

		if (area == null) {
			area = new CollisionShape2D();
			(area as CollisionShape2D).Shape = new RectangleShape2D();
			AddChild(area);
		}

		Connect("mouse_entered", this, nameof(MouseEntered));
		Connect("mouse_exited", this, nameof(MouseExited));
	}

	public void MouseEntered() {
		MouseCursor.instance?.MouseEnter(this);
	}
	public void MouseExited() {
		MouseCursor.instance?.MouseLeave(this);
	}


	public virtual void OnClick() {
		if (GameController.canPlayerInteract || ignoreInteractionLock)
			onClick?.Invoke();
	}
}
using Godot;
using System;
using System.Collections.Generic;
using Environment = Godot.Environment;

public class AnimationList : List<SmkAnimation> {
	public static Action OnCancelTrigger;

	public Vector2 basePosition;

	public SmkAnimation current;
	public MouseArea mouseArea;

	public MouseArea CreateMouseArea(Node parent) {
		mouseArea = new MouseArea();
		CollisionShape2D area = new CollisionShape2D();
		area.Shape = new RectangleShape2D();
		area.ZIndex = 1;
		mouseArea.AddChild(area);
		parent.AddChild(mouseArea);

		return mouseArea;
	}

	public void Play(int index) {
		StopAll();

		current = this[index];

		if (mouseArea != null) {
			int width = current.GetWidth(), height = current.GetHeight();
			mouseArea.area.Scale = new Vector2(width / 2, height / 2) / 10f;
			mouseArea.area.Position = new Vector2(width / 2, height / 2) + basePosition;
		}
		current.Play();
	}

	public new void Add(SmkAnimation item) {
		base.Add(item);

		item.Position += basePosition;

		if (item.goal.finish != null || item.goal.finishID != -1)
			item.OnAnimationFinish += (item.goal.finish != null ? (Action)(() => Play(item.goal.finish)) : (() => Play(item.goal.finishID)));

		if (item.goal.onFinish != null) {
			item.OnAnimationFinish += item.goal.onFinish;
		}
	}

	public void Play(SmkAnimation animation) {
		StopAll();

		current = Find((toFind) => toFind == animation);

		if (mouseArea != null) {
			int width = current.GetWidth(), height = current.GetHeight();
			mouseArea.area.Scale = new Vector2(width / 2, height / 2) / 10f;
			mouseArea.area.Position = new Vector2(width / 2, height / 2) + basePosition;
		}

		current.Play();
	}

	public void StopAll() {
		foreach (SmkAnimation p in this) {
			p.Hide();
		}
	}

	public void Stop(int index) {
		foreach (SmkAnimation p in this) {
			p.Hide();
		}

		current = this[index];
		current.Visible = true;
		current.isPlaying = false;
	}

	public void Cancel() {
		SmkAnimation anim = current?.goal?.cancel;
		if (current.goal.cancelID != -1)
			Play(current.goal.cancelID);

		if (anim != null)
			Play(anim);
	}

	public void Trigger() {
		SmkAnimation anim = current.goal.trigger;
		if (current.goal.triggerID != -1)
			Play(current.goal.triggerID);

		if (anim != null)
			Play(anim);
	}

	public void ProcessTrigger() {
		if (current?.goal?.onTrigger?.Invoke(current.goal) == true)
			Trigger();
	}
}


public class AnimationGoal {
	/// <summary>
	/// When a animation is done, go to this animation:
	/// When null -> loop until other criterea are set.      
	/// </summary>
	public SmkAnimation finish;
	public int finishID;
	public Action onFinish;

	/// <summary>
	/// When a cancel message is recieved, go to this animation
	/// </summary>
	public SmkAnimation cancel;
	public int cancelID;
	/// <summary>
	/// When the current animation gets triggered by a call to smkPlayer.Trigger(), go to this animation
	/// </summary>
	public SmkAnimation trigger;
	public int triggerID;

	public Func<AnimationGoal, bool> onTrigger;

	public AnimationGoal(SmkAnimation finish = null, SmkAnimation cancel = null, SmkAnimation trigger = null, Func<AnimationGoal, bool> onTrigger = null) {
		this.finish = finish;
		this.cancel = cancel;
		this.trigger = trigger;
		finishID = -1;
		cancelID = -1;
		triggerID = -1;

		this.onTrigger = onTrigger;
	}
	public AnimationGoal(int finish = -1, int cancel = -1, int trigger = -1, Func<AnimationGoal, bool> onTrigger = null) {
		this.finishID = finish;
		this.cancelID = cancel;
		this.triggerID = trigger;
		this.finish = null;
		this.cancel = null;
		this.trigger = null;

		this.onTrigger = onTrigger;
	}
	public AnimationGoal(Func<AnimationGoal, bool> onTrigger) {
		finishID = -1;
		cancelID = -1;
		triggerID = -1;
		this.finish = null;
		this.cancel = null;
		this.trigger = null;

		this.onTrigger = onTrigger;
	}

	public AnimationGoal() {
		finishID = -1;
		cancelID = -1;
		triggerID = -1;
		this.finish = null;
		this.cancel = null;
		this.trigger = null;
	}
}

public class AnimationGoalIdle : AnimationGoal {
	/// <summary>
	/// AnimationGoal for a idle animation. Will switch between some random animations, this animation and to a dialogue animation when a dialogue starts
	/// </summary>
	/// <param name="actor">The actor of this animation in any dialogue</param>
	/// <param name="dialogueStartId">The animation to play when the player starts the conversation</param>
	/// <param name="idleAnimationsIds">A list of animations which get played at random intervals</param>
	public AnimationGoalIdle(string actor, int dialogueStartId, params int[] idleAnimationsIds) : base() {
		onTrigger += (g) => IdleAnimHelper(g, actor, dialogueStartId, idleAnimationsIds);
	}

	private int randomInterval;
	private int maxTimeBetweenIntervals = 10; //in seconds
	private int minTimeBetweenIntervals = 2; //in seconds

	private bool IdleAnimHelper(AnimationGoal goal, string actor, int dialogueStartId, int[] idleAnimationsIds) {
		if (DialogueSystem.IsDialogueActive && (DialogueSystem.actorA == actor || DialogueSystem.actorB == actor)) {
			goal.triggerID = dialogueStartId;
			return true;
		}

		if (idleAnimationsIds?.Length != 0 && randomInterval <= OS.GetTicksMsec()) {
			goal.triggerID = idleAnimationsIds[GameController.r.Next(0, idleAnimationsIds.Length)];

			randomInterval = (int)OS.GetTicksMsec() + 1000 * (int)(GameController.r.NextDouble() * maxTimeBetweenIntervals) + minTimeBetweenIntervals;

			return true;
		}

		return false;
	}
}

public class AnimationGoalTalking : AnimationGoal {
	/// <summary>
	/// AnimationGoal for a talking animation. Will switch between a listening animation, this animation and back to a normal animation when the dialogue stopped
	/// </summary>
	/// <param name="currentDialogue">The dialogue to which this animation belongs to</param>
	/// <param name="actor">The actor of this animation in the dialogue</param>
	/// <param name="listeningID">The animation to play when the actor stops talking and listens instead</param>
	/// <param name="dialogueStoppedID">The animation to play when the player stops the conversation</param>
	public AnimationGoalTalking(Dialogue currentDialogue, string actor, int listeningID, int dialogueStoppedID) : base() {
		onTrigger += (g) => WhileTalkingAnimHelper(g, currentDialogue, actor, listeningID, dialogueStoppedID);
	}
	private static bool WhileTalkingAnimHelper(AnimationGoal goal, Dialogue creditDialogue, string actor, int noTalkingID, int dialogueStoppedID) {
		if (DialogueSystem.currentlyTalking != actor) {
			goal.triggerID = noTalkingID;
			return true;
		}
		if (DialogueSystem.currentSound?.IsTalking() == false) {
			goal.triggerID = noTalkingID;
			GD.Print("Change to NO TALKING");
			return true;
		}
		if (DialogueSystem.IsDialogueActive == false) {
			goal.triggerID = dialogueStoppedID;
			return true;
		}

		return false;
	}
}

public class AnimationGoalListening : AnimationGoal {
	/// <summary>
	/// AnimationGoal for a listening animation. Will switch between a talking animation, this animation and back to a normal animation when the dialogue stopped
	/// </summary>
	/// <param name="currentDialogue">The dialogue to which this animation belongs to</param>
	/// <param name="actor">The actor of this animation in the dialogue</param>
	/// <param name="startTalkingID">The animation to play when the actor starts talking</param>
	/// <param name="dialogueStoppedID">The animation to play when the player stops the conversation</param>
	public AnimationGoalListening(Dialogue currentDialogue, string actor, int startTalkingID, int dialogueStoppedID) : base() {
		onTrigger += (g) => WhileListeningAnimHelper(g, currentDialogue, actor, startTalkingID, dialogueStoppedID);
	}

	private static bool WhileListeningAnimHelper(AnimationGoal goal, Dialogue listeningDialogue, string actor, int startTalkingID, int dialogueStoppedID) {
		if (DialogueSystem.IsDialogueActive == false) {
			goal.triggerID = dialogueStoppedID;
			GD.Print("Change to TALKING");
			return true;
		}

		if (DialogueSystem.currentSound?.IsTalking() == true && DialogueSystem.currentlyTalking == actor) {
			goal.triggerID = startTalkingID;

			GD.Print("Change to TALKING");
			return true;
		}

		return false;
	}
}

public class AnimationGoalDialogueStart : AnimationGoal {
	private bool startTelephone;

	private void PrepareStartForDialogue() {
		void Start() {
			if (DialogueSystem.isTelephoneCall && startTelephone) {
				DialogueSystem.StartPreparedTelephoneCall();
				return;
			} else if (DialogueSystem.isTelephoneCall && startTelephone == false) {
				return;
			}

			DialogueSystem.StartCurrentDialogue();

			//throw new NullReferenceException("Trying to use a dialogue start Animation goal, without properly preparing the dialogue system! Please use DialogueSystem.PrepareDialogue(...) to prepare!");
		}

		onFinish = Start;
	}

	public AnimationGoalDialogueStart() {
		PrepareStartForDialogue();
	}

	public AnimationGoalDialogueStart(Func<AnimationGoal, bool> onTrigger) : base(onTrigger) {
		PrepareStartForDialogue();

	}

	public AnimationGoalDialogueStart(SmkAnimation finish = null, SmkAnimation cancel = null, SmkAnimation trigger = null, Func<AnimationGoal, bool> onTrigger = null) : base(finish, cancel, trigger, onTrigger) {
		PrepareStartForDialogue();
	}

	public AnimationGoalDialogueStart(bool startTelephone = false, int finish = -1, int cancel = -1, int trigger = -1, Func<AnimationGoal, bool> onTrigger = null) : base(finish, cancel, trigger, onTrigger) {
		this.startTelephone = startTelephone;
		PrepareStartForDialogue();
	}
}
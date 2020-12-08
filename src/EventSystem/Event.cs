using System;

public class Event {
	public string name;

	bool _hasStarted;

	public Action onQueue, onEventStart, onEventTick, onEventFinish;
	public Func<bool> onValidation;

	public bool IsValid() {
		return onValidation == null ? true : onValidation.Invoke();
	}

	public void Execute() {
		if (_hasStarted) {
			onEventTick?.Invoke();
		} else {
			onEventStart?.Invoke();
			_hasStarted = true;
		}
	}

	ATTime startTime;
	int duration; //in ticks

	public int Duration { get => duration; set => duration = value; }
	public ATTime StartTime { get => startTime; set => startTime = value; }
}

using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Executes the event callbacks
/// </summary>
public static class EventManager {
	static List<Event> events = new List<Event>();
	static List<Event> finishedEvents = new List<Event>();

	public static void ScheduleEvent(Event e, ATTime time) {
		e.StartTime = time;
		events.Add(e);
		e.onQueue?.Invoke();
	}

	/// <summary>
	/// Schedules an event in "time" from the current time 
	/// </summary>
	/// <param name="e">the event to schedule</param>
	/// <param name="time">time from now</param>
	public static void ScheduleEventFromNow(Event e, ATTime time) {
		ScheduleEvent(e, GameController.CurrentTime + time);
	}

	public static List<Event> GetEvents() {
		return events;
	}
	public static List<Event> GetFinishedEvents() {
		return finishedEvents;
	}

	public static void HandleTick() {
		List<Event> eventsMarkedForDeletion = new List<Event>();

		foreach (Event e in events) {
			if (e.IsValid() == false) {
				eventsMarkedForDeletion.Add(e);
				continue;
			}

			if (e.StartTime == GameController.CurrentTime) {
				//markedEvents.Add(e.e);
				e?.onEventStart();
				continue;
			}

			if (e.StartTime.IsInRange(GameController.CurrentTime, e.Duration)) {
				e.onEventTick?.Invoke();
				continue;
			}

			eventsMarkedForDeletion.Add(e);
		}

		foreach (Event e in eventsMarkedForDeletion) {
			if (e.Duration != 0)
				e?.onEventFinish();

			events.Remove(e);
		}
		finishedEvents.AddRange(eventsMarkedForDeletion);
	}
}

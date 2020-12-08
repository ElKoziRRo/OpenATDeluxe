using Godot;
using System;

public class ATTime {
	public const int TicksInAMinute = 30;

	public float ToTicks() {
		return (((Weeks * 7 + Days) * 24 + Hours) * 60 + Minutes) * 30 + Ticks;
	}

	static readonly string[] daysInWeek = {
		"Schd>3010",//Mon
        "Schd>3011",//Tue
        "Schd>3012",//Wed
        "Schd>3013",//Thu
        "Schd>3014",//Fri
        "Schd>3015",//Sat
        "Schd>3016",//Sun
    };

	static readonly string[] daysInWeekShort = {
		"Schd>3000",//Mon
        "Schd>3001",//Tue
        "Schd>3002",//Wed
        "Schd>3003",//Thu
        "Schd>3004",//Fri
        "Schd>3005",//Sat
        "Schd>3006",//Sun
    };

	public ATTime(int weeks, int days, int hours, int minutes, float ticks) {
		this.weeks = weeks;
		this.days = days;
		this.hours = hours;
		this.minutes = minutes;
		this.ticks = ticks;

		HandleOverflow();
	}

	public ATTime(int weeks, int days, int hours, int minutes) : this(weeks, days, hours, minutes, 0) {
	}

	public string GetAsDayOfWeek(bool translationApplied = true) {
		return translationApplied ? TranslationServer.Translate(daysInWeek[days]) : daysInWeek[days];
	}

	public string GetAsShortDayOfWeek(bool translationApplied = true) {
		return translationApplied ? TranslationServer.Translate(daysInWeekShort[days]) : daysInWeekShort[days];
	}

	int weeks, days, hours, minutes;
	float ticks;

	public int Weeks { get => weeks; }
	public int Days { get => days; }
	public int Hours { get => hours; }
	public int Minutes { get => minutes; }

	public float Ticks { get => ticks; }
	public float TotalTicks { get => ticks + Minutes * TicksInAMinute + Hours * 60 * TicksInAMinute + Days * 24 * 60 * TicksInAMinute + Weeks * 7 * 24 * 60 * TicksInAMinute; }

	/// <summary>
	/// Moves time forward by x ticks
	/// </summary>
	/// <param name="ticks">adds x ticks. Overflow will be transferred, but it will not add more than 1 minute, per 'Tick()'</param>
	public void Tick(float ticks = 1) {
		this.ticks += ticks;

		HandleOverflow();
	}

	private void HandleOverflow() {
		while (ticks >= TicksInAMinute) {
			ticks = Mathf.Max(0, ticks - TicksInAMinute);
			minutes++;
		}

		while (minutes >= 60) {
			minutes = Math.Max(0, minutes - 60);
			hours++;
		}

		while (hours >= 24) {
			hours = Math.Max(0, hours - 24); ;
			days++;
		}

		while (days >= 7) {
			days = Math.Max(0, days - 7); ;
			weeks++;
		}
	}

	public void AddMinute() {
		minutes++;

		HandleOverflow();
	}

	public void AddHour() {
		minutes++;

		HandleOverflow();
	}

	public void AddDay() {
		days++;

		HandleOverflow();
	}

	public void AddWeek() {
		weeks++;

		HandleOverflow();
	}

	public static ATTime operator +(ATTime l, ATTime r) {
		return new ATTime(l.weeks + r.weeks, l.days + r.days, l.hours + r.hours, l.minutes + r.minutes, l.ticks + r.ticks);
	}
	public static bool operator ==(ATTime l, ATTime r) {
		return l.weeks == r.weeks && l.days == r.days && l.hours == r.hours && l.minutes == r.minutes && l.ticks == r.ticks;
	}
	public static bool operator !=(ATTime l, ATTime r) {
		return !(l.weeks == r.weeks && l.days == r.days && l.hours == r.hours && l.minutes == r.minutes && l.ticks == r.ticks);
	}

	public bool IsInRange(ATTime currentTime, ATTime duration) {
		return currentTime.TotalTicks < (this.TotalTicks + duration.TotalTicks);
	}

	public bool IsInRange(ATTime currentTime, int tickDuration) {
		return IsInRange(currentTime, new ATTime(0, 0, 0, 0, tickDuration));
	}

	public static ATTime FromCurrentTimeForward(int weeks, int days, int hours, int minutes, int ticks) {
		return GameController.CurrentTime + new ATTime(weeks, days, hours, minutes, ticks);
	}

	public override string ToString() {
		return $"W{Weeks}D{Days} {Hours.ToString("D2")}:{Minutes.ToString("D2")}T{Ticks}";
	}
}

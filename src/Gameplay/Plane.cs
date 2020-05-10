using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class Plane {
	public int id;

	#region infos
	public string producer;
	public string name;
	public string description;
	public int yearOfDevelopment;
	public string engines;
	public int thrust; //lb
	#endregion

	#region gameplay relevant
	int unlockingMission, unlockingDay;

	public int passengerCapacity;

	public int speed; //kph
	public int range; //km

	public int maxFuel; //L
	public int fuelConsumption; //L/h

	public int pilots, service;

	public int newPrice;
	public float serviceFactor;
	#endregion

	static List<Plane> planeTypes;

	public static void LoadPlanes() {
		CSVFileDecoder pFile = new CSVFileDecoder("planetyp.csv");
		var pl = pFile.GetCSVDictionaryLineSeperated();

		planeTypes = new List<Plane>();

		foreach (var plane in pl) {
			List<string> data = (from p in plane select p.Value).ToList();
			try {
				//Sanitize data[22], because some idiot found it funny to allow "," and "." as starting char, instead of using a number... eg. ".8"
				data[22] = "0" + data[22];

				planeTypes.Add(new Plane {
					id = data[0].ToInt(),
					producer = data[1],
					name = data[2],
					//Currently not in use from us:
					//photo = data[3],
					//photoCount = data[4],
					unlockingMission = data[5].ToInt(),
					unlockingDay = data[6].ToInt(),
					yearOfDevelopment = data[7].ToInt(),
					//Currently not in use from us:
					//width = data[8],
					//length = data[9],
					//height = data[10],
					//maxStartingWeight = data[11],
					passengerCapacity = data[12].ToInt(),
					speed = data[13].ToInt(),
					range = data[14].ToInt(),
					engines = data[15],
					thrust = data[16].ToInt(),
					pilots = data[17].ToInt(),
					service = data[18].ToInt(),
					maxFuel = data[19].ToInt(),
					fuelConsumption = data[20].ToInt(),
					newPrice = data[21].ToInt(),
					serviceFactor = float.Parse(data[22]),
					description = data[23],
				});
			} catch (Exception e) {
				GD.Print(e.Message);
				GD.Print(e.StackTrace);

				string brokenEntry = "";
				foreach (var d in data) {
					brokenEntry += "," + d;
				}

				GD.Print(brokenEntry);
			}
		}

	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DataMunging
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 0 || !int.TryParse(args[0], out int n))
			{
				Console.WriteLine("Please supply an argument for the number of days.");
				return;
			}

			var days = GetDays(@"../../weather.txt");

			var bestVacations = BestVacation(days, n);
			foreach (var vacation in bestVacations)
				Console.WriteLine($"{vacation.StartDay} the {vacation.StartDate}th day of the month is the best day for a {(n == 1 ? "picnic" : "vacation")}.");

			Console.WriteLine($"\nThe maximum possible number of nice days on any vacation is {GetMaxN(days)}");
		}
		
		private static int GetMaxN(List<Day> days)
		{
			var lengths = new List<int>();
			for (int i = 0; i < days.Count; ++i)
			{
				if (days[i].Warm && days[i].Dry)
				{
					var vacation = new List<Day> { days[i] };

					// Crawl along to find contiguous nice days.
					int j = i + 1;
					for (; j < days.Count && (days[j].Warm && days[j].Dry); ++j)
						vacation.Add(days[j]);

					// Add the length of the vacation and reset.
					lengths.Add(vacation.Count);
					i = j;
				}
			}
			return lengths.Max();
		}
		
		private static List<Day> GetDays(string fileLocation)
		{
			// Get the data.
			List<string> lines = new List<string>();
			using (StreamReader reader = new StreamReader(fileLocation))
			{
				Regex whitespace = new Regex(@"\s+");
				while (!reader.EndOfStream)
					lines.Add(whitespace.Replace(reader.ReadLine(), ","));
			}

			var validLines = lines
				.Select(x => x.Split(','))
				.Where(x => int.TryParse(x[0], out _));

			var daysArray = GetDaysArray(validLines);

			return validLines
				.Select(x => new Day(decimal.Parse(x[3]), decimal.Parse(x[2]), int.Parse(x[4]), int.Parse(x[0]), daysArray))
				.ToList();
		}
		
		private static List<string> GetDaysArray(IEnumerable<string[]> lines)
		{
			var days = new List<string>();
			for (int i = 0; i < 7; ++i)
			{
				var today = lines.ElementAt(i)[1];
				var tomorrow = lines.ElementAt(i + 1)[1];

				switch (today)
				{
					case "m":
						days.Add("Monday");
						break;
					case "t":
						if (tomorrow == "w")
							days.Add("Tuesday");
						else
							days.Add("Thursday");
						break;
					case "w":
						days.Add("Wednesday");
						break;
					case "f":
						days.Add("Friday");
						break;
					case "s":
						if (tomorrow == "s")
							days.Add("Saturday");
						else
							days.Add("Sunday");
						break;
				}
			}

			return days;
		}
		
		private static List<Vacation> BestVacation(List<Day> days, int n)
		{
			var vacations = new List<Vacation>();
			for (int i = 0; i < days.Count; ++i)
			{
				if (days[i].Warm && days[i].Dry)
				{
					var vacation = new List<Day> { days[i] };

					// Crawl along to find contiguous nice days.
					int j = i + 1;
					for (; (j < days.Count && days[j].Date < days[i].Date + n) && (days[j].Warm && days[j].Dry); ++j)
						vacation.Add(days[j]);

					// Add the length of the vacation and reset.
					vacations.Add(new Vacation
					{
						Length = n,
						NiceDays = vacation.Count,
						StartDate = days[i].Date,
						StartDay = days[i].Weekday
					});

					i = j;
				}
			}

			// Find the maximum nice days.
			var maxNiceDays = vacations.Max(x => x.NiceDays);

			// Return the best vacations
			return vacations
				.Where(x => x.NiceDays == maxNiceDays)
				.ToList();
		}
    }

	public class Vacation
	{
		public int NiceDays { get; set; }
		public int StartDate { get; set; }
		public int Length { get; set; }
		public string StartDay { get; set; }
	}

	public class Day
	{
		private static int _dryThreshold = 10;
		private static decimal _warmLower = 70;
		private static decimal _warmUpper = 85;
		
		public bool Dry { get; set; }
		public bool Warm { get; set; }
		public int Date { get; set; }
		public string Weekday { get; set; }

		public Day(decimal low, decimal high, int rainChance, int date, List<string> days)
		{
			Date = date;
			Dry = rainChance <= _dryThreshold;
			Warm = IsWarm(ToFahrenheit(low), ToFahrenheit(high));
			Weekday = days[(date - 1) % 7];
		}
		
		public decimal ToFahrenheit(decimal celcius)
			=> celcius * 1.8M + 32;
		
		public bool IsWarm(decimal low, decimal high) 
			=> low >= _warmLower && high <= _warmUpper;
	}
}

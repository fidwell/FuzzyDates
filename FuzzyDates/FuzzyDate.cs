﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using FuzzyDates.Rules;

[assembly: InternalsVisibleTo("FuzzyDates.Tests")]

namespace FuzzyDates
{
	public struct FuzzyDate : IComparable<FuzzyDate>, ISerializable
	{
		/// <summary>
		/// Gets the year component of the date represented by this instance.
		/// </summary>
		public int? Year { get; }

		/// <summary>
		/// Gets the month component of the date represented by this instance.
		/// </summary>
		public int? Month { get; }

		/// <summary>
		/// Gets the day component of the date represented by this instance.
		/// </summary>
		public int? Day { get; }

		/// <summary>
		/// Initializes a new instance of the FuzzyDate class from the specified DateTime value.
		/// </summary>
		/// <param name="dateTime">The date.</param>
		public FuzzyDate(DateTime dateTime) : this(dateTime.Year, dateTime.Month, dateTime.Day)
		{
		}

		/// <summary>
		/// Initializes a new instance of the FuzzyDate class to the specified year, month, and day.
		/// </summary>
		/// <param name="year">The year. Can be any signed int value.</param>
		/// <param name="month">The month (1 through 12).</param>
		/// <param name="day">The day (1 through the number of days in month).</param>
		public FuzzyDate(int year, int month, int day) : this((int?)year, month, day)
		{
		}

		/// <summary>
		/// Initializes a new instance of the FuzzyDate class to the specified year and month.
		/// </summary>
		/// <param name="year">The year. Can be any signed int value.</param>
		/// <param name="month">The month (1 through 12).</param>
		public FuzzyDate(int year, int month) : this(year, month, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the FuzzyDate class to the specified year.
		/// </summary>
		/// <param name="year">The year. Can be any signed int value.</param>
		public FuzzyDate(int year) : this(year, null, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the FuzzyDate class with no value.
		/// </summary>
		public static FuzzyDate Unknown
		{
			get
			{
				return new FuzzyDate();
			}
		}

		/// <summary>
		/// Initializes a new instance of the FuzzyDate class set to today's date.
		/// </summary>
		public static FuzzyDate Today
		{
			get
			{
				var now = DateTime.Today;
				return new FuzzyDate(now.Year, now.Month, now.Day);
			}
		}

		private FuzzyDate(int? year, int? month, int? day)
		{
			Year = year;
			Month = month;
			Day = day;

			RulesRunner.RunRules(this);
		}

		/// <summary>
		/// Parses a date from format "YYYY", "YYYY/MM", or "YYYY/MM/DD"
		/// </summary>
		/// <param name="value"></param>
		public static FuzzyDate Parse(string value)
		{
			int? year = null;
			int? month = null;
			int? day = null;

			// Could probably be made more efficient with a regex or something
			if (value.Length >= 4)
			{
				year = int.Parse(value.Substring(0, 4));

				if (value.Length >= 7)
				{
					month = int.Parse(value.Substring(5, 2));

					if (value.Length == 10)
					{
						day = int.Parse(value.Substring(8, 2));
					}
				}
			}

			return new FuzzyDate(year, month, day);
		}

		/// <summary>
		/// Compares the value of this instance to a specified FuzzyDate value and returns an integer
		/// that indicates whether this instance is earlier than, the same as, or later than the
		/// specified FuzzyDate value.
		/// </summary>
		/// <param name="value">The object to compare to the current instance.</param>
		/// <returns>A signed number indicating the relative values of this instance and the value parameter.</returns>
		public int CompareTo(FuzzyDate value)
		{
			// If either date's year is empty, it should go first
			if (!Year.HasValue && !value.Year.HasValue)
				return 0;
			if (Year.HasValue && !value.Year.HasValue)
				return 1;
			if (!Year.HasValue && value.Year.HasValue)
				return -1;

			// If the years are not null and different, compare those
			if (Year.HasValue && value.Year.HasValue && Year.Value != value.Year.Value)
				return Year.Value.CompareTo(value.Year.Value);

			// If either date's month is empty, it should go first
			if (!Month.HasValue && !value.Month.HasValue)
				return 0;
			if (Month.HasValue && !value.Month.HasValue)
				return 1;
			if (!Month.HasValue && value.Month.HasValue)
				return -1;

			// If the months are not null and different, compare those
			if (Month.HasValue && value.Month.HasValue && Month.Value != value.Month.Value)
				return Month.Value.CompareTo(value.Month.Value);

			// If either date's day is empty, it should go first
			if (!Day.HasValue && !value.Day.HasValue)
				return 0;
			if (Day.HasValue && !value.Day.HasValue)
				return 1;
			if (!Day.HasValue && value.Day.HasValue)
				return -1;

			// Finally, compare days
			return Day.Value.CompareTo(value.Day.Value);
		}

		/// <summary>
		/// For testing purposes only. You should be using
		/// some sort of display adapter to control how
		/// dates are displayed to a consumer.
		/// </summary>
		/// <returns>A human-readable string representation of the date.</returns>
		public override string ToString()
		{
			if (!Year.HasValue)
			{
				return "unknown date";
			}

			if (!Month.HasValue)
			{
				return Year.ToString();
			}

			if (!Day.HasValue)
			{
				return new DateTime(Year.Value, Month.Value, 1).ToString("Y");
			}

			return new DateTime(Year.Value, Month.Value, Day.Value).ToString("D");
		}

		/// <summary>
		/// Converts a FuzzyDate to a .NET DateTime object. Uses "1" for unknown values.
		/// </summary>
		/// <returns>A DateTime with a close-as-possible-equivalent representation of the value.</returns>
		public DateTime ToDateTime()
		{
			// DateTime years must be between 1 and 9999
			var year = Year.HasValue
				? Math.Max(Year.Value, 1)
				: 1;

			if (year > 9999)
			{
				year = DateTime.Now.Year;
			}

			return new DateTime(year, Month ?? 1, Day ?? 1);
		}

		/// <summary>
		/// Populates a SerializationInfo object with the data needed to serialize the current FuzzyDate object.
		/// </summary>
		/// <param name="info">The object to populate with data.</param>
		/// <param name="context">The destination for this serialization. (This parameter is not used; specify null.)</param>
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			info.AddValue("Year", Year);
			info.AddValue("Month", Day);
			info.AddValue("Day", Day);
		}

		/// <summary>
		/// Returns a new FuzzyDate that adds the specified number of years to the value of this instance.
		/// </summary>
		/// <param name="value">A number of years. The value parameter can be negative or positive.</param>
		/// <returns>A FuzzyDate whose value is the sum of the date represented by this instance and the number of years represented by value.</returns>
		public FuzzyDate AddYears(int value)
		{
			var newYear = Year.HasValue
				? Year.Value + value
				: (int?)null;
			return new FuzzyDate(newYear, Month, Day);
		}

		/// <summary>
		/// Returns a new FuzzyDate that adds the specified number of months to the value of this instance.
		/// </summary>
		/// <param name="value">A number of months. The value parameter can be negative or positive.</param>
		/// <returns>A FuzzyDate whose value is the sum of the date represented by this instance and the number of years represented by value.</returns>
		public FuzzyDate AddMonths(int value)
		{
			if (Month.HasValue)
			{
				var asDateTime = ToDateTime();
				var newDateTime = asDateTime.AddMonths(value);
				return new FuzzyDate(newDateTime);
			}

			return new FuzzyDate(Year, Month, Day);
		}

		/// <summary>
		/// Returns a new FuzzyDate that adds the specified number of days to the value of this instance.
		/// </summary>
		/// <param name="value">A number of days. The value parameter can be negative or positive.</param>
		/// <returns>A FuzzyDate whose value is the sum of the date represented by this instance and the number of years represented by value.</returns>
		public FuzzyDate AddDays(int value)
		{
			if (Day.HasValue)
			{
				var asDateTime = ToDateTime();
				var newDateTime = asDateTime.AddDays(value);
				return new FuzzyDate(newDateTime);
			}

			return new FuzzyDate(Year, Month, Day);
		}

		/// <summary>
		/// Returns an indication whether the specified year is a leap year.
		/// </summary>
		/// <returns>true if Year is defined and is a leap year; otherwise, false.</returns>
		public bool IsLeapYear()
		{
			if (!Year.HasValue)
			{
				return false;
			}

			return DateTime.IsLeapYear(Year.Value);
		}
	}
}
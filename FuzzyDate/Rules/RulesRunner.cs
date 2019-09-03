﻿using System.Collections.Generic;

namespace FuzzyDate.Rules
{
	internal static class RulesRunner
	{
		private static IEnumerable<IRule> rules = new List<IRule>
		{
			new DayMustBeInRangeRule(),
			new MonthMustBeInRangeRule(),
			new YearMustBeNonZeroRule()
		};

		internal static void RunRules(FuzzyDate date)
		{
			foreach(var rule in rules)
			{
				rule.Verify(date);
			}
		}
	}
}
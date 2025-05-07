using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GasStation.Helpers
{
	public static class Helper
	{
        public static string FormatSimpleProperCase(string input)
        {
            input = input.Trim();
            if (string.IsNullOrEmpty(input))
                return input;

            if (input.Length == 1)
                return input.ToUpper();

            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }
    }
}
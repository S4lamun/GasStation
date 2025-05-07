using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GasStation.Services
{
	public class BusinessLogicException:Exception
	{
		string message;
		public BusinessLogicException(string message) : base(message)
        {
            this.message = message;
        }
    }
}
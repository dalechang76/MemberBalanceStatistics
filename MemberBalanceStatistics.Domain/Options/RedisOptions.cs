using System;
using System.Collections.Generic;
using System.Text;

namespace MemberBalanceStatistics.Domain.Options
{
	public class RedisOptions
	{
		public string Address { get; set; }
		public int Port { get; set; }
		public string Auth { get; set; }
		public int DbIndex { get; set; }
		public string AffixKey { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace MemberBalanceStatistics.Domain.Dto
{
    public class ExtremeResultDto
    {
        public string MemberID { get; set; }
        public long ResultMax { get; set; }
        public long ResultMin { get; set; }
    }
}

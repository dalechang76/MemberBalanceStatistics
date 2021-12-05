using System;
using System.Collections.Generic;
using System.Text;
using MemberBalanceStatistics.Domain.Dto;

namespace MemberBalanceStatistics.Domain.Provider
{
    public interface IMemberBalanceProvider
    {
        IEnumerable<TotalResultDto> GetToalResult(int categoryID);

        IEnumerable<ExtremeResultDto> GetExtremeResult(int categoryID, IEnumerable<string> memberIDs);

        bool UpdateExtremeResult(int categoryID, IEnumerable<ExtremeResultDto> memberIDs);

        bool UpdateMemberBalance(int categoryID, IEnumerable<MemberBalanceDto> memberBalances);

        DateTime? GetLastExcuteTime();

        bool SetLastExcuteTime();

        bool ClearExtremeResult();

        bool CleareMemberBalance();
    }
}

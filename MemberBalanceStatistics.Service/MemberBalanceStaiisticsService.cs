using System;
using System.Collections.Generic;
using System.Linq;
using MemberBalanceStatistics.Domain.Dto;
using MemberBalanceStatistics.Domain.Provider;
using MemberBalanceStatistics.Domain.Services;

namespace MemberBalanceStatistics.Service
{
    public class MemberBalanceStaiisticsService : IMemberBalanceStaiisticsService
    {
        private readonly IMemberBalanceProvider _memberBalanceProvider;

        public MemberBalanceStaiisticsService(IMemberBalanceProvider memberBalanceProvider)
        {
            _memberBalanceProvider = memberBalanceProvider;
        }

        public void Excute()
        {
            //FIXME
            var categoryID = 10;

            // 1.取出SR線上人數各個玩家當前的TotalResult
            // (疑問:MaxResult、MinResult邏輯?是否為特定時間最贏跟最輸，若是就不用再算，直接取這兩個值就好，省略2、3)
            var totalResults = _memberBalanceProvider.GetToalResult(categoryID);
            var memberIDs = totalResults.Select(x => x.MemberID);

            // 判斷是否跨日
            if (IsCrossDay())
            {
                // 清除極端結果、小紅綠人
                _memberBalanceProvider.ClearExtremeResult();
                _memberBalanceProvider.CleareMemberBalance();

                // 初始ExtremeResult
                var initExtremeResults = totalResults.Select(x => new ExtremeResultDto
                {
                    MemberID = x.MemberID,
                    ResultMax = x.TotalResult,
                    ResultMin = x.TotalResult
                });
                _memberBalanceProvider.UpdateExtremeResult(categoryID, initExtremeResults);

                // 初始小紅綠人
                var initMemberBalanceDtos = memberIDs.Select(x => new MemberBalanceDto
                {
                    MemberID = x,
                    Balance = -1
                });
                _memberBalanceProvider.UpdateMemberBalance(categoryID, initMemberBalanceDtos);

                // 存最後執行時間
                _memberBalanceProvider.SetLastExcuteTime();
                return;
            }

            // 2.取對應玩家的Max、Min TotalResult
            var extremeResults = _memberBalanceProvider.GetExtremeResult(categoryID, memberIDs);

            // 3.將當前玩家TotalResult與歷史Max、Min TotalResult比較是否要更新
            foreach (var extremeResult in extremeResults)
            {
                var totalResult = totalResults
                    .Where(x => x.MemberID == extremeResult.MemberID)
                    .Select(x => x.TotalResult)
                    .FirstOrDefault();

                extremeResult.ResultMax = totalResult > extremeResult.ResultMax ? totalResult : extremeResult.ResultMax;
                extremeResult.ResultMin = totalResult < extremeResult.ResultMin ? totalResult : extremeResult.ResultMin;
            }

            _memberBalanceProvider.UpdateExtremeResult(categoryID, extremeResults);


            // 4.計算玩家的小紅綠人
            var memberBalanceDtos = CalculateMemberBalance(extremeResults);

            // 5.更新玩家小紅綠人
            _memberBalanceProvider.UpdateMemberBalance(categoryID, memberBalanceDtos);

            // 6.紀錄最後執行時間
            _memberBalanceProvider.SetLastExcuteTime();
        }

        private IEnumerable<MemberBalanceDto> CalculateMemberBalance(IEnumerable<ExtremeResultDto> dtos)
        {
            // if 最贏>0 && 最輸<0
            //   if 最輸+最贏>=0
            //      綠人
            //   else 
            //      紅人
            // else
            //  橘人

            var result = new List<MemberBalanceDto>();

            foreach (var dto in dtos)
            {
                var memberBalanceDto = new MemberBalanceDto();
                memberBalanceDto.MemberID = dto.MemberID;
                if (dto.ResultMax > 0 && dto.ResultMin < 0)
                {
                    if ((dto.ResultMax+ dto.ResultMin) >= 0)
                    {
                        memberBalanceDto.Balance = 1;
                    }
                    else
                    {
                        memberBalanceDto.Balance = 0;
                    }
                }
                else
                {
                    memberBalanceDto.Balance = -1;
                }

                result.Add(memberBalanceDto);
            }

            return result;
        }

        private bool IsCrossDay()
        {
            var lastTime = _memberBalanceProvider.GetLastExcuteTime();
            if (lastTime == null)
            {
                return false;
            }

            var now = DateTime.UtcNow.AddHours(8);
            return lastTime?.Day != now.Day;
        }
    }
}

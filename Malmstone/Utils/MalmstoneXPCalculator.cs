using System;
using System.Collections.Generic;

namespace Malmstone.Utils
{
    public static class MalmstoneXPCalculator
    {
        private static readonly int[] PvpLevels = {
            0, 2000, 2000, 2000, 2000, 3000, 3000, 3000, 3000, 3000, 4000, 4000, 4000, 4000, 4000, 5500, 5500, 5500, 5500, 5500,
            7500, 7500, 7500, 7500, 7500, 10000, 10000, 10000, 10000, 10000, 20000, 20000
        };

        private const int InfinityLevelExp = 20000;
        private const int FrontlineWinExp = 1500;
        private const int FrontlineLose2Exp = 1250;
        private const int FrontlineLoseExp = 1000;
        private const int FrontlineDailyWinExp = 3000;
        private const int FrontlineDailyLose2Exp = 2750;
        private const int FrontlineDailyLoseExp = 2500;
        private const int CrystallineWinExp = 900;
        private const int CrystallineLoseExp = 700;
        private const int RivalWingsWinExp = 1250;
        private const int RivalWingsLoseExp = 750;

        public class XpCalculationResult
        {
            public int RemainingXp { get; set; }
            public int TargetLevel { get; set; }
            public Dictionary<string, int> ActivityCounts { get; set; } = new();
        }

        public static XpCalculationResult CalculateXp(int currentLevel, int goalLevel, int currentProgress)
        {
            if (currentLevel < 1 || goalLevel < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(currentLevel), "Levels must be greater than 0.");
            }

            int remainingXp = 0;

            if (currentLevel <= PvpLevels.Length && goalLevel <= PvpLevels.Length)
            {
                remainingXp = CalculateRemainingXp(currentLevel, goalLevel, currentProgress);
            }
            else
            {
                remainingXp = CalculateRemainingXpBeyondChart(currentLevel, goalLevel, currentProgress);
            }

            if (remainingXp <= 0)
            {
                return new XpCalculationResult { RemainingXp = 0, TargetLevel = goalLevel };
            }

            var result = new XpCalculationResult
            {
                RemainingXp = remainingXp,
                TargetLevel = goalLevel,
            };

            result.ActivityCounts["Crystalline Conflict Win"] = CalculateActivityCount(remainingXp, CrystallineWinExp);
            result.ActivityCounts["Crystalline Conflict Lose"] = CalculateActivityCount(remainingXp, CrystallineLoseExp);
            result.ActivityCounts["Frontline Win"] = CalculateActivityCount(remainingXp, FrontlineWinExp);
            result.ActivityCounts["Frontline Lose 2nd"] = CalculateActivityCount(remainingXp, FrontlineLose2Exp);
            result.ActivityCounts["Frontline Lose 3rd"] = CalculateActivityCount(remainingXp, FrontlineLoseExp);
            result.ActivityCounts["Frontline Daily Win"] = CalculateActivityCount(remainingXp, FrontlineDailyWinExp);
            result.ActivityCounts["Frontline Daily Lose 2nd"] = CalculateActivityCount(remainingXp, FrontlineDailyLose2Exp);
            result.ActivityCounts["Frontline Daily Lose 3rd"] = CalculateActivityCount(remainingXp, FrontlineDailyLoseExp);
            result.ActivityCounts["Rival Wings Win"] = CalculateActivityCount(remainingXp, RivalWingsWinExp);
            result.ActivityCounts["Rival Wings Lose"] = CalculateActivityCount(remainingXp, RivalWingsLoseExp);

            return result;
        }

        private static int CalculateRemainingXp(int currentLevel, int goalLevel, int currentProgress)
        {
            int remainingXp = 0;

            for (int level = currentLevel; level < goalLevel; level++)
            {
                remainingXp += PvpLevels[level];
            }

            return remainingXp - currentProgress;
        }

        private static int CalculateRemainingXpBeyondChart(int currentLevel, int goalLevel, int currentProgress)
        {
            int remainingXp = 0;

            if (currentLevel <= PvpLevels.Length)
            {
                remainingXp = CalculateRemainingXp(currentLevel, PvpLevels.Length, currentProgress);
                currentLevel = PvpLevels.Length;
            }

            remainingXp += (goalLevel - currentLevel) * InfinityLevelExp;

            return remainingXp;
        }

        private static int CalculateActivityCount(int remainingXp, int activityXp)
        {
            return (int)Math.Ceiling((double)remainingXp / activityXp);
        }
    }
}

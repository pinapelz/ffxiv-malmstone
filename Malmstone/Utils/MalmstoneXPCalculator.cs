using System;
using System.Collections.Generic;

namespace Malmstone.Utils
{
    public class MalmstoneXPCalculator
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

        public struct XpCalculationResult
        {
            public int RemainingXp;
            public int TargetLevel;
            public int CrystallineConflictWin;
            public int CrystallineConflictLose;
            public int FrontlineWin;
            public int FrontlineLose2nd;
            public int FrontlineLose3rd;
            public int FrontlineDailyWin;
            public int FrontlineDailyLose2nd;
            public int FrontlineDailyLose3rd;
            public int RivalWingsWin;
            public int RivalWingsLose;
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
                CrystallineConflictWin = CalculateActivityCount(remainingXp, CrystallineWinExp),
                CrystallineConflictLose = CalculateActivityCount(remainingXp, CrystallineLoseExp),
                FrontlineWin = CalculateActivityCount(remainingXp, FrontlineWinExp),
                FrontlineLose2nd = CalculateActivityCount(remainingXp, FrontlineLose2Exp),
                FrontlineLose3rd = CalculateActivityCount(remainingXp, FrontlineLoseExp),
                FrontlineDailyWin = CalculateActivityCount(remainingXp, FrontlineDailyWinExp),
                FrontlineDailyLose2nd = CalculateActivityCount(remainingXp, FrontlineDailyLose2Exp),
                FrontlineDailyLose3rd = CalculateActivityCount(remainingXp, FrontlineDailyLoseExp),
                RivalWingsWin = CalculateActivityCount(remainingXp, RivalWingsWinExp),
                RivalWingsLose = CalculateActivityCount(remainingXp, RivalWingsLoseExp)
            };

            return result;
        }

        public static XpCalculationResult CalculateCrystallineConflictMatches(int currentLevel, int goalLevel, int currentProgress)
        {
            var baseResult = CalculateXp(currentLevel, goalLevel, currentProgress);
            var result = new XpCalculationResult
            {
                RemainingXp = baseResult.RemainingXp,
                TargetLevel = baseResult.TargetLevel,
                CrystallineConflictWin = CalculateActivityCount(baseResult.RemainingXp, CrystallineWinExp),
                CrystallineConflictLose = CalculateActivityCount(baseResult.RemainingXp, CrystallineLoseExp)
            };

            return result;
        }

        public static XpCalculationResult CalculateFrontlineMatches(int currentLevel, int goalLevel, int currentProgress)
        {
            var baseResult = CalculateXp(currentLevel, goalLevel, currentProgress);
            var result = new XpCalculationResult
            {
                RemainingXp = baseResult.RemainingXp,
                TargetLevel = baseResult.TargetLevel,
                FrontlineWin = CalculateActivityCount(baseResult.RemainingXp, FrontlineWinExp),
                FrontlineLose2nd = CalculateActivityCount(baseResult.RemainingXp, FrontlineLose2Exp),
                FrontlineLose3rd = CalculateActivityCount(baseResult.RemainingXp, FrontlineLoseExp),
                FrontlineDailyWin = CalculateActivityCount(baseResult.RemainingXp, FrontlineDailyWinExp),
                FrontlineDailyLose2nd = CalculateActivityCount(baseResult.RemainingXp, FrontlineDailyLose2Exp),
                FrontlineDailyLose3rd = CalculateActivityCount(baseResult.RemainingXp, FrontlineDailyLoseExp)
            };

            return result;
        }

        public static XpCalculationResult CalculateRivalWingsMatches(int currentLevel, int goalLevel, int currentProgress)
        {
            var baseResult = CalculateXp(currentLevel, goalLevel, currentProgress);
            var result = new XpCalculationResult
            {
                RemainingXp = baseResult.RemainingXp,
                TargetLevel = baseResult.TargetLevel,
                RivalWingsWin = CalculateActivityCount(baseResult.RemainingXp, RivalWingsWinExp),
                RivalWingsLose = CalculateActivityCount(baseResult.RemainingXp, RivalWingsLoseExp)
            };

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
            // Should always be greater than 0
            return Math.Max(1, (int)Math.Ceiling((double)remainingXp / activityXp));
        }
    }
}

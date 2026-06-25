using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PushPelmesh.Zong
{
    public readonly struct ZongRollScore
    {
        public ZongRollScore(int score, int usedDice)
        {
            Score = score;
            UsedDice = usedDice;
        }

        public int Score { get; }
        public int UsedDice { get; }
    }

    public static class ZongScoring
    {
        public static List<int> RollDice(int count)
        {
            List<int> dice = new List<int>();

            for (int i = 0; i < count; i++)
                dice.Add(Random.Range(1, 7));

            return dice;
        }

        public static ZongRollScore CalculateScore(List<int> dice)
        {
            if (dice == null || dice.Count == 0)
                return new ZongRollScore(0, 0);

            int score = 0;
            int used = 0;
            Dictionary<int, int> counts = Enumerable.Range(1, 6)
                .ToDictionary(face => face, face => dice.Count(value => value == face));

            if (dice.Count == 6 && counts.All(pair => pair.Value == 1))
                return new ZongRollScore(1500, 6);

            if (dice.Count == 6 && counts.Count(pair => pair.Value == 2) == 3)
                return new ZongRollScore(1500, 6);

            for (int face = 1; face <= 6; face++)
            {
                int count = counts[face];

                if (count < 3)
                    continue;

                int baseScore = face == 1 ? 1000 : face * 100;
                int multiplier = count == 3 ? 1 : count == 4 ? 2 : count == 5 ? 4 : 8;
                score += baseScore * multiplier;
                used += count;
                counts[face] = 0;
            }

            if (counts[1] > 0)
            {
                score += counts[1] * 100;
                used += counts[1];
            }

            if (counts[5] > 0)
            {
                score += counts[5] * 50;
                used += counts[5];
            }

            return new ZongRollScore(score, used);
        }
    }
}

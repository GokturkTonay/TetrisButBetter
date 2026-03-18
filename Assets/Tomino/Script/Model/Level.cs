using System;

namespace Tomino.Model
{
    public class Level
    {
        public int Number => Lines / 10 + 1;
        public float FallDelay => Math.Max(0.05f, 1.0f - (Number - 1) * 0.1f);
        public int Lines { get; private set; }
        // Değiştirilebilir hale getirdik
        public int TargetScore { get; set; } = 100;
        public int CurrentMultiplier { get; private set; }

        public void RowsCleared(int count)
        {
            Lines += count;
            CurrentMultiplier = count;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dsci
{
    public class Progress
    {
        private class ProgressCore
        {
            public double Value;
            public int IntValue;

            public Action<int> Updated;
        }

        private readonly ProgressCore Core;

        private readonly double Step;

        private readonly object UpdateLock;

        public Progress(int min, int max, Action<int> updated)
        {
            Core = new ProgressCore();
            Core.Updated = updated;
            Core.Value = Core.IntValue = min;
            Step = max - min;
        }

        private Progress(ProgressCore core, double step)
        {
            Core = core;
            Step = step;
        }

        public Progress Divide(int divider)
        {
            return new Progress(Core, Step / divider);
        }

        public void Advance(int n = 1)
        {
            int new_int_value;
            lock (Core)
            {
                new_int_value = (int)Math.Round(Core.Value += Step);
                if (new_int_value <= Core.IntValue) return;
                Core.IntValue = new_int_value;
            }
            lock (UpdateLock)
            {
                lock (Core) if (new_int_value < Core.IntValue) return;
                Core.Updated(new_int_value);
            }
        }
    }
}

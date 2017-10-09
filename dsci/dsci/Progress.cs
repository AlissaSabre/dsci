using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dsci
{
    public class Progress
    {
        private float Value;

        private readonly float Step;

        private readonly Action<float> Updated;

        public Progress(float min, float max, Action<float> updated)
        {
            Value = min;
            Step = max - min;
            Updated = updated;
        }

        private Progress(int divider, float value, float step, Action<float> updated)
        {
            Value = value;
            Step = step / divider;
            Updated = updated;
        }

        public Progress Divide(int divider)
        {
            return new Progress(divider, Value, Step, Updated);
        }

        public void Advance()
        {
            Updated?.Invoke(Value += Step);
        }

        public void Advance(int n)
        {
            Updated?.Invoke(Value += Step * n);
        }
    }
}

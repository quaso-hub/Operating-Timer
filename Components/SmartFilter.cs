using System;
using System.Collections.Generic;
using System.Linq;

namespace Operating_Timer.Components
{
    public class SmartFilter
    {
        private readonly int _bufferSize;
        private readonly List<float> _tempBuffer;
        private readonly List<float> _rhBuffer;

        private float? _lastValidTemp;
        private float? _lastValidRh;
        private int _invalidCount = 0;
        private const int MaxInvalidsBeforeReset = 10;

        // Kalibrasi opsional
        public float TempOffset { get; set; } = 0f;
        public float RhOffset { get; set; } = 0f;

        public SmartFilter(int bufferSize = 10)
        {
            _bufferSize = bufferSize;
            _tempBuffer = new List<float>();
            _rhBuffer = new List<float>();
        }

        public void Feed(float rawTemp, float rawRh)
        {
            float temp = rawTemp + TempOffset;
            float rh = rawRh + RhOffset;

            if (!IsPhysicallyPossible(temp, rh))
                return;

            AddToBuffer(_tempBuffer, temp);
            AddToBuffer(_rhBuffer, rh);

            float medianTemp = GetMedian(_tempBuffer);
            float medianRh = GetMedian(_rhBuffer);

            if (Math.Abs(temp - medianTemp) > 5 || Math.Abs(rh - medianRh) > 10)
            {
                _invalidCount++;
                if (_invalidCount >= MaxInvalidsBeforeReset)
                    Reset();
            }
            else
            {
                _lastValidTemp = temp;
                _lastValidRh = rh;
                _invalidCount = 0;
            }
        }

        public bool IsValid() => _lastValidTemp.HasValue && _lastValidRh.HasValue;

        //public float GetLastValidTemp() => _lastValidTemp ?? -1f;
        //public float GetLastValidRh() => _lastValidRh ?? -1f;

        public float GetAverageTemp() => _tempBuffer.Count > 0 ? _tempBuffer.Average() : -1f;
        public float GetAverageRh() => _rhBuffer.Count > 0 ? _rhBuffer.Average() : -1f;


        public void Reset()
        {
            _tempBuffer.Clear();
            _rhBuffer.Clear();
            _lastValidTemp = null;
            _lastValidRh = null;
            _invalidCount = 0;
        }

        private void AddToBuffer(List<float> buffer, float value)
        {
            if (buffer.Count >= _bufferSize)
                buffer.RemoveAt(0);

            buffer.Add(value);
        }

        private float GetMedian(List<float> list)
        {
            var sorted = list.OrderBy(x => x).ToList();
            int mid = sorted.Count / 2;
            return sorted.Count % 2 == 0
                ? (sorted[mid - 1] + sorted[mid]) / 2f
                : sorted[mid];
        }

        private bool IsPhysicallyPossible(float temp, float rh)
        {
            return temp >= 0 && temp <= 80 && rh >= 0 && rh <= 100;
        }
    }
}

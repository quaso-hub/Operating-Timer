using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Operating_Timer.Components
{
    public class ClockManager
    {
        public event Action<string> OnTimeUpdate;
        public event Action<string> OnDateUpdate;

        private Timer _timer;

        public ClockManager()
        {
            _timer = new Timer { Interval = 1000 };
            _timer.Tick += (s, e) =>
            {
                OnTimeUpdate?.Invoke(DateTime.Now.ToString("HH:mm:ss"));
                OnDateUpdate?.Invoke(DateTime.Now.ToString("dd MMM yyyy"));
            };
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pr32_34
{
    public class ActivityMonitor
    {
        private static ActivityMonitor _instance;
        public static ActivityMonitor Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ActivityMonitor();
                }
                return _instance;
            }
        }

        private Timer _idleTimer;
        private DateTime _lastActivityTime;
        private int _timeoutSeconds = 30;

        public event Action OnTimeout;

        private ActivityMonitor()
        {
            _idleTimer = new Timer();
            _idleTimer.Interval = 1000;
            _idleTimer.Tick += (s, e) => CheckIdleTime();
            _idleTimer.Start();
            _lastActivityTime = DateTime.Now;
        }

        public void Reset() => _lastActivityTime = DateTime.Now;

        private void CheckIdleTime()
        {
            if ((DateTime.Now - _lastActivityTime).TotalSeconds >= _timeoutSeconds)
            {
                _idleTimer.Stop();
                OnTimeout?.Invoke(); // Сигнал: "Пора на вход!"
            }
        }
    }

    // Этот класс будет "слушать" все нажатия кнопок и движения мыши во всем приложении
    public class GlobalInputFilter : IMessageFilter
    {
        public bool PreFilterMessage(ref Message m)
        {
            const int WM_MOUSEMOVE = 0x0200;
            const int WM_KEYDOWN = 0x0100;
            const int WM_LBUTTONDOWN = 0x0201;

            if (m.Msg == WM_MOUSEMOVE || m.Msg == WM_KEYDOWN || m.Msg == WM_LBUTTONDOWN)
            {
                ActivityMonitor.Instance.Reset(); // Сброс таймера при любом действии
            }
            return false;
        }
    }
}

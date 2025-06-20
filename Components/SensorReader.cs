using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Operating_Timer.Components
{
    public class SensorReader : IDisposable
    {
        private RawModbusReader _reader;
        private Timer _pollingTimer;
        private Timer _retryTimer;
        private string _currentPort;
        private bool _connected = false;
        private bool _disposed = false;
        private bool _isConnecting = false;

        private ManagementEventWatcher _usbInsertWatcher;
        private ManagementEventWatcher _usbRemoveWatcher;

        private readonly SmartFilter _filter = new SmartFilter
        {
            TempOffset = 0f,
            RhOffset = 0f
        };

        public event Action<string, string> OnSensorUpdate;
        public event Action<bool> OnConnectionStatusChanged;

        public SensorReader()
        {
            StartUsbWatcher();
            StartAutoConnect();
        }

        private void StartAutoConnect()
        {
            if (_isConnecting) return;
            _isConnecting = true;

            Timer initialConnectTimer = new Timer { Interval = 500 };
            initialConnectTimer.Tick += (s, e) =>
            {
                initialConnectTimer.Stop();
                initialConnectTimer.Dispose();
                AutoConnect();
            };
            initialConnectTimer.Start();
        }

        private void AutoConnect()
        {
            _connected = false;
            _reader?.Dispose();
            _reader = null;

            string[] ports = SerialPort.GetPortNames();
            Debug.WriteLine("[SensorReader] Scanning ports: " + string.Join(", ", ports));

            foreach (string port in ports)
            {
                try
                {
                    Debug.WriteLine($"[SensorReader] Trying port: {port}");

                    // PRE-CHECK: Test open/close to ensure port is ready
                    using (var testPort = new SerialPort(port))
                    {
                        try
                        {
                            testPort.Open();
                            testPort.Close();
                        }
                        catch
                        {
                            Debug.WriteLine($"[SensorReader] Port {port} not yet ready. Skipping.");
                            continue;
                        }
                    }

                    var raw = new RawModbusReader(port);
                    if (!raw.Connect()) continue;

                    var result = raw.ReadSensor();
                    if (result.Temp == null || result.Humidity == null)
                        continue;

                    _reader = raw;
                    _currentPort = port;
                    _connected = true;
                    _filter.Reset();

                    InitPolling();
                    OnConnectionStatusChanged?.Invoke(true);
                    Debug.WriteLine($"[SensorReader] Connected to {port}");
                    _isConnecting = false;
                    return;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[SensorReader] Error on port {port}: {ex.Message}");
                }
            }

            Debug.WriteLine("[SensorReader] All ports failed. Will retry...");
            RetryConnect();
        }

        private void RetryConnect()
        {
            _retryTimer?.Stop();
            _retryTimer?.Dispose();

            _retryTimer = new Timer { Interval = 3000 };
            _retryTimer.Tick += (s, e) =>
            {
                _retryTimer.Stop();
                _retryTimer.Dispose();

                Thread.Sleep(200); // Give time for port enumeration
                AutoConnect();
            };
            _retryTimer.Start();
        }

        private void InitPolling()
        {
            _pollingTimer?.Stop();
            _pollingTimer = new Timer { Interval = 1000 };
            _pollingTimer.Tick += PollSensor;
            _pollingTimer.Start();

            Debug.WriteLine("[SensorReader] Polling started.");

            var data = _reader.ReadSensor();
            if (data.Temp.HasValue && data.Humidity.HasValue)
            {
                _filter.Feed(data.Temp.Value, data.Humidity.Value);
                OnSensorUpdate?.Invoke(
                    data.Temp.Value.ToString("F1"),
                    data.Humidity.Value.ToString("F1"));
            }
        }

        private void PollSensor(object sender, EventArgs e)
        {
            try
            {
                if (_reader == null || !_connected) return;

                var (temp, rh) = _reader.ReadSensor();
                if (temp == null || rh == null) return;

                _filter.Feed(temp.Value, rh.Value);
                var avgTemp = _filter.GetAverageTemp();
                var avgHum = _filter.GetAverageRh();

                if (avgTemp >= 0 && avgHum >= 0)
                {
                    OnSensorUpdate?.Invoke(avgTemp.ToString("F1"), avgHum.ToString("F1"));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SensorReader] Polling error: {ex.Message}");
                _connected = false;
                OnConnectionStatusChanged?.Invoke(false);
                Reconnect();
            }
        }

        private void Reconnect()
        {
            _pollingTimer?.Stop();
            _pollingTimer?.Dispose();
            _pollingTimer = null;

            _reader?.Dispose();
            _reader = null;

            _connected = false;
            _currentPort = null;

            _filter.Reset();
            _isConnecting = false;

            RetryConnect();
        }

        private void StartUsbWatcher()
        {
            try
            {
                var insertQuery = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");
                _usbInsertWatcher = new ManagementEventWatcher(insertQuery);
                _usbInsertWatcher.EventArrived += (s, e) =>
                {
                    Debug.WriteLine("[USB Watcher] Device inserted.");
                    Reconnect();
                };
                _usbInsertWatcher.Start();

                var removeQuery = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3");
                _usbRemoveWatcher = new ManagementEventWatcher(removeQuery);
                _usbRemoveWatcher.EventArrived += (s, e) =>
                {
                    Debug.WriteLine("[USB Watcher] Device removed.");
                    Reconnect();
                };
                _usbRemoveWatcher.Start();

                Debug.WriteLine("[USB Watcher] Started.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[USB Watcher] Init failed: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _pollingTimer?.Stop();
            _pollingTimer?.Dispose();
            _reader?.Dispose();
            _retryTimer?.Stop();
            _retryTimer?.Dispose();

            _usbInsertWatcher?.Stop();
            _usbInsertWatcher?.Dispose();
            _usbRemoveWatcher?.Stop();
            _usbRemoveWatcher?.Dispose();

            Debug.WriteLine("[SensorReader] Fully disposed.");
        }
    }
}

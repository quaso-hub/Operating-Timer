using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace Operating_Timer.Components
{
    public class RawModbusReader : IDisposable
    {
        private SerialPort _serial;
        private readonly string _portName;
        private readonly object _lock = new object();
        private bool _isConnected = false;

        public RawModbusReader(string portName)
        {
            _portName = portName;
        }

        public bool Connect()
        {
            try
            {
                if (_serial != null && _serial.IsOpen)
                {
                    Debug.WriteLine($"[RawModbusReader] Already connected to {_portName}");
                    return true;
                }

                if (!SerialPort.GetPortNames().Contains(_portName))
                {
                    Debug.WriteLine($"[RawModbusReader] Port {_portName} not available.");
                    return false;
                }

                _serial = new SerialPort(_portName, 9600, Parity.None, 8, StopBits.One)
                {
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };

                _serial.Open();
                _isConnected = true;
                Debug.WriteLine($"[RawModbusReader] Connected to {_portName}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RawModbusReader] Failed to open port {_portName}: {ex.Message}");
                _isConnected = false;
                return false;
            }
        }

        public (float? Temp, float? Humidity) ReadSensor()
        {
            if (!_isConnected || _serial == null || !_serial.IsOpen)
                return (null, null);

            for (int attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    lock (_lock)
                    {
                        byte[] request = new byte[] { 0x01, 0x04, 0x00, 0x01, 0x00, 0x02 };
                        byte[] crc = CRC16(request);
                        byte[] frame = request.Concat(crc).ToArray();

                        _serial.DiscardInBuffer();
                        _serial.Write(frame, 0, frame.Length);
                        Thread.Sleep(100);

                        byte[] buffer = new byte[9];
                        int read = _serial.Read(buffer, 0, buffer.Length);

                        if (read != 9 || buffer[2] != 0x04)
                        {
                            Debug.WriteLine("[RawModbusReader] Invalid response format");
                            continue;
                        }

                        if (!IsValidCRC(buffer))
                        {
                            Debug.WriteLine("[RawModbusReader] CRC check failed");
                            continue;
                        }

                        float val1 = ((ushort)((buffer[3] << 8) | buffer[4])) * 0.1f;
                        float val2 = ((ushort)((buffer[5] << 8) | buffer[6])) * 0.1f;

                        float temp = (val1 >= 5 && val1 <= 60) ? val1 : val2;
                        float rh = (temp == val1) ? val2 : val1;

                        if (temp < 0 || rh < 0)
                            return (null, null);

                        return (temp, rh);
                    }
                }
                catch (TimeoutException)
                {
                    Debug.WriteLine($"[RawModbusReader] Timeout on attempt {attempt + 1}");
                    Thread.Sleep(50);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[RawModbusReader] Exception on attempt {attempt + 1}: {ex.Message}");
                    Thread.Sleep(50);
                }
            }

            return (null, null);
        }

        public void Dispose()
        {
            try
            {
                if (_serial != null)
                {
                    if (_serial.IsOpen)
                    {
                        _serial.Close();
                        Debug.WriteLine("[RawModbusReader] Port closed");
                    }

                    _serial.Dispose();
                    _serial = null;
                }

                _isConnected = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RawModbusReader] Dispose error: {ex.Message}");
            }
        }

        private byte[] CRC16(byte[] data)
        {
            ushort crc = 0xFFFF;
            foreach (byte b in data)
            {
                crc ^= b;
                for (int i = 0; i < 8; i++)
                {
                    bool lsb = (crc & 0x0001) != 0;
                    crc >>= 1;
                    if (lsb) crc ^= 0xA001;
                }
            }

            return new byte[] { (byte)(crc & 0xFF), (byte)(crc >> 8) };
        }

        private bool IsValidCRC(byte[] response)
        {
            if (response.Length < 3)
                return false;

            byte[] data = response.Take(response.Length - 2).ToArray();
            byte[] crc = CRC16(data);

            return crc[0] == response[response.Length - 2] &&
                   crc[1] == response[response.Length - 1];
        }
    }
}

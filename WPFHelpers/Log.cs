using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WPFHelpers.Async;
using WPFHelpers.MessengerNSpace;

namespace WPFHelpers
{
    public class Log : IDisposable
    {
        public Log()
        {
            Instance = this;
        }

        private ReaderWriterLock _RWLock = new ReaderWriterLock();
        private ReaderWriterSemaphoreClass _RWSemaphore = new ReaderWriterSemaphoreClass();
        private readonly object _LockObject = new object();
        private StreamWriter _Stream;
        private Timer _Timer;
        private ConcurrentQueue<string> _Messages = new ConcurrentQueue<string>();
        private ConcurrentQueue<string> _Second = new ConcurrentQueue<string>();
        private bool _PrivateSlowMode;
        private static bool _SlowMode;
        public float _SlowModeSeconds = 0.05f;
        public int _MaxMessagesToWriteInOneGo = 500;

        public static bool SlowMode
        {
            get { return _SlowMode; }
            set
            {
                if (_SlowMode != value)
                {
                    _SlowMode = value;

                    if (value)
                    {
                        Instance.ActivateSlowMode();
                    }
                    else
                    {
                        Task.Run(() => Instance.DeactivateSlowModeAsync());
                    }
                }
            }
        }
        public static Log Instance { get; private set; }

        public void WriteLine(string value)
        {
            var msg = $"{Environment.NewLine}{DateTime.Now.ToString(@"dd / MM / yyyy - hh:mm: ss")} ---- - {value}";

            //if (_RWLock.IsWriterLockHeld && _PrivateSlowMode)
            //{
                Task.Run(() => AddMessagesToWaitSlowMode(msg));
                return;
            //}

            //_RWLock.AcquireWriterLock(2000);
            ////var msgArray = new string[] { Environment.NewLine + DateTime.Now.ToString(@"dd/MM/yyyy - hh:mm:ss") + "----- " + value };
            ////var msg = string.Join(Environment.NewLine, msgArray);

            //try
            //{
            //    using (var writer = File.AppendText($"Log{DateTime.Today.ToString("dd.MM.yyyy")}.txt"))
            //    {
            //        Task.Run(() => writer.WriteAsync(msg));
            //    }
            //}
            //catch (Exception e)
            //{
            //    e.ShowException();
            //}

            ////File.AppendAllLines(
            ////    $"Log{DateTime.Today.ToString("dd.MM.yyyy")}.txt",
            ////    msgArray);
            ////    byte[] encodedText = Encoding.Unicode.GetBytes(msg);
            ////Task.Run(() => _Stream.WriteAsync(encodedText, 0, encodedText.Length));

            ////if (WriteToLogTextbox)
            //Messenger.TrySendGuidedMessage(Properties.WPFHelperSettings.Default.Message_LogMsg, msg);
            //_RWLock.ReleaseWriterLock();
        }
        public async Task WriteAsync(string value)
        {
            var msg = $@"{Environment.NewLine}{DateTime.Now.ToString(@"dd / MM / yyyy - hh:mm: ss.fff")} ---- - {value}";

            //if (_RWSemaphore.WritersMustWait() && _PrivateSlowMode)
            //{
                var forget = AddMessagesToWaitSlowMode(msg).ConfigureAwait(false);
                return;
            //}

            //await _RWSemaphore.WaitWriterAsync(2000).ConfigureAwait(false);

            //try
            //{
            //    await _Stream.WriteAsync(msg).ConfigureAwait(false);
            //}
            //catch (Exception e)
            //{
            //    e.ShowException();
            //}

            //Messenger.TrySendGuidedMessage(Properties.WPFHelperSettings.Default.Message_LogMsg, msg);

            //_RWSemaphore.WriterRelease();
        }

        private async Task AddMessagesToWaitSlowMode(string msg)
        {
            if (_Messages.Count > _MaxMessagesToWriteInOneGo)
                _Second.Enqueue(msg);
            else
                _Messages.Enqueue(msg);
        }
        private async void MyTimerCallback(object state)
        {
            if (!_SlowMode)
                return;

            string msg = null;

            if (_Messages.Count > 0 || _Second.Count > 0)
            {
                lock (_LockObject)
                {
                    if (_Messages.Count > 0 || _Second.Count > 0)
                    {
                        if (!SlowMode)
                        {
                            msg = string.Join("", _Messages.Union(_Second));
                            _Messages = new ConcurrentQueue<string>();
                            _Second = new ConcurrentQueue<string>();
                        }
                        else
                        {
                            if (_Messages.Count <= _MaxMessagesToWriteInOneGo)
                            {
                                msg = string.Join("", _Messages);
                                if (_Second.Count > 0)
                                {
                                    _Messages = new ConcurrentQueue<string>(_Second);
                                    _Second = new ConcurrentQueue<string>();
                                }
                                else
                                    _Messages = new ConcurrentQueue<string>();
                            }
                            else
                            {
                                var sb = new StringBuilder();
                                if (_Second.Count == 0)
                                {
                                    for (int i = 0; i < _MaxMessagesToWriteInOneGo; i++)
                                    {
                                        string partial;
                                        _Messages.TryDequeue(out partial);
                                        sb.Append(partial);
                                    }

                                    msg = sb.ToString();
                                }
                                else
                                {
                                    for (int i = 0; i < _MaxMessagesToWriteInOneGo; i++)
                                    {
                                        string partial;
                                        _Messages.TryDequeue(out partial);
                                        sb.Append(partial);

                                        if(_Second.Count > 0)
                                        {
                                            _Second.TryDequeue(out partial);
                                            _Messages.Enqueue(partial);
                                        }
                                    }

                                    msg = sb.ToString();
                                }
                            }
                        }
                    }
                }

                try
                {
                    await _RWSemaphore.WaitWriterAsync();
                    using (var writer = File.AppendText($"Log{DateTime.Today.ToString("dd.MM.yyyy")}.txt"))
                        await writer.WriteAsync(msg).ConfigureAwait(false);
                    _RWSemaphore.WriterRelease();
                }
                catch (Exception e)
                {
                    e.ShowException();
                }

                Messenger.TrySendGuidedMessage(Properties.WPFHelperSettings.Default.Message_LogMsg, msg);
            }
        }
        private void ActivateSlowMode()
        {
            _PrivateSlowMode = true;
            int miliSec = (int)(_SlowModeSeconds * 1000);
            _Timer = new Timer(new TimerCallback(MyTimerCallback), null, miliSec, miliSec);
            //Task.Run(() => _RWSemaphore.WaitWriterAsync());
            //_RWLock.AcquireWriterLock(miliSec);
        }
        private async Task DeactivateSlowModeAsync()
        {
            _Timer.Change(Timeout.Infinite, Timeout.Infinite);
            string msg = null;

            if (_Messages.Count > 0 || _Second.Count > 0)
            {
                lock (_LockObject)
                {
                    if (_Second.Count > 0)
                    {
                        _Messages = new ConcurrentQueue<string>(_Second);
                        _Second = new ConcurrentQueue<string>();
                    }
                    if (_Messages.Count > 0)
                    {
                        msg = string.Join("", _Messages.Union(_Second));
                        _Messages = new ConcurrentQueue<string>();
                        _Second = new ConcurrentQueue<string>();
                    }
                }

                try
                {
                    await _Stream.WriteAsync(msg).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    e.ShowException();
                }

                Messenger.TrySendGuidedMessage(Properties.WPFHelperSettings.Default.Message_LogMsg, msg);
            }

            _RWLock.ReleaseWriterLock();
            _RWSemaphore.WriterRelease();
            _PrivateSlowMode = false;
        }

        public void Dispose()
        {
            _RWSemaphore.Dispose();
            _Stream.Dispose();
            _Timer.Dispose();
        }
    }
}

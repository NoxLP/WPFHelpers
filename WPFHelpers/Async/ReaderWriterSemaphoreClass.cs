using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WPFHelpers.Async
{
    public sealed class ReaderWriterSemaphoreClass : IDisposable
    {
        public ReaderWriterSemaphoreClass(int readerMaxCount = 0)
        {
            _WriterSemaphore = new SemaphoreSlim(1);
            _MaxReaders = readerMaxCount;
            _CurrentReaders = 0;
        }

        private object _Lock = new object();
        private SemaphoreSlim _WriterSemaphore;
        private int _CurrentReaders;
        private readonly int _MaxReaders;
        private int _WriterWaiting;

        public bool WritersMustWait()
        {
            return _WriterSemaphore.CurrentCount == 0 || _WriterWaiting > 0;
        }
        public bool ReadersMustWait()
        {
            return _WriterSemaphore.CurrentCount == 0 || _WriterWaiting > 0 || (_MaxReaders != 0 && _CurrentReaders > _MaxReaders);
        }

        public async Task WaitWriterAsync()
        {
            _WriterWaiting++;
            while (_CurrentReaders > 0)
            {
                await Task.Delay(50);
            }
            _WriterWaiting--;

            await _WriterSemaphore.WaitAsync();
        }
        public async Task WaitWriterAsync(CancellationToken token)
        {
            _WriterWaiting++;
            while (_CurrentReaders > 0)
            {
                if (token.IsCancellationRequested)
                    return;

                await Task.Delay(50);
            }
            _WriterWaiting--;

            await _WriterSemaphore.WaitAsync(token);
        }
        public async Task WaitWriterAsync(int milisecondsTimeout)
        {
            _WriterWaiting++;
            int ms = 0;
            while (_CurrentReaders > 0 && ms < milisecondsTimeout)
            {
                await Task.Delay(50);
                ms += 50;
            }
            _WriterWaiting--;

            await _WriterSemaphore.WaitAsync();
        }
        public async Task WaitReaderAsync()
        {
            while (ReadersMustWait())
            {
                await Task.Delay(50);
            }

            _CurrentReaders++;
        }
        public async Task WaitReaderAsync(CancellationToken token)
        {
            while (ReadersMustWait())
            {
                if (token.IsCancellationRequested)
                    return;

                await Task.Delay(50);
            }

            _CurrentReaders++;
        }

        public int WriterRelease()
        {
            return _WriterSemaphore.Release();
        }
        public int ReaderRelease()
        {
            int ret = _CurrentReaders;
            _CurrentReaders--;
            return ret;
        }
        public int ReaderRelease(int releaseCount)
        {
            int ret = _CurrentReaders;
            _CurrentReaders -= releaseCount;
            return ret;
        }

        public void Dispose()
        {
            _WriterSemaphore.Dispose();
        }
    }
}

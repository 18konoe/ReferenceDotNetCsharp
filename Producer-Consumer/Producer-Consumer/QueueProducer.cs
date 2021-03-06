﻿using System;

namespace Producer_Consumer
{
    public class QueueProducer : IDisposable
    {
        private QueueConsumer _consumer;

        public bool IsConsumerRunning => _consumer.IsRunning;

        public void AddExample()
        {
            _consumer?.Add(new QueueItems.Example());
        }

        public void StartConsumer()
        {
            _consumer.Start();
        }

        public void StopConsumer()
        {
            _consumer.Stop();
        }

        public QueueProducer()
        {
            _consumer = new QueueConsumer();
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_consumer != null)
                    {
                        _consumer.Dispose();
                    }
                }

                _consumer = null;

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
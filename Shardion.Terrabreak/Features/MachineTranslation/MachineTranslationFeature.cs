using System;
using System.Threading.Tasks;
using GTranslate.Translators;

namespace Shardion.Terrabreak.Features.MachineTranslation
{
    public class MachineTranslationFeature : ITerrabreakFeature, IDisposable
    {
        private bool _disposed;

        public AggregateTranslator Translator { get; }

        public MachineTranslationFeature()
        {
            Translator = new AggregateTranslator([new GoogleTranslator(), new GoogleTranslator2()]);
        }

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposingManagedObjects)
        {
            if (_disposed)
            {
                return;
            }

            if (disposingManagedObjects)
            {
                Translator.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(disposingManagedObjects: true);
            GC.SuppressFinalize(this);
        }
    }
}

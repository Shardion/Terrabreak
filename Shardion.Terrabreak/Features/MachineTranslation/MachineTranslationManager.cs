using System;
using System.Threading.Tasks;
using GTranslate.Translators;

namespace Shardion.Terrabreak.Features.MachineTranslation;

public class MachineTranslationManager : ITerrabreakFeature, IDisposable
{
    private bool _disposed;

    public AggregateTranslator Translator { get; } = new([new GoogleTranslator(), new GoogleTranslator2()]);

    public Task StartAsync()
    {
        return Task.CompletedTask;
    }

    protected virtual void Dispose(bool disposingManagedObjects)
    {
        if (_disposed) return;

        if (disposingManagedObjects) Translator.Dispose();

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

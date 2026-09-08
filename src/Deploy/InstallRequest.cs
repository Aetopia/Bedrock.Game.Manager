using System;
using System.Threading.Tasks;
using Bedrock.Game.Manager.Store;
using Windows.ApplicationModel.Store.Preview.InstallControl;
using static System.Threading.Tasks.TaskContinuationOptions;
using static Windows.ApplicationModel.Store.Preview.InstallControl.AppInstallState;

namespace Bedrock.Game.Manager.Deploy;

sealed class InstallRequest<T> where T : IProgress<(string, int)>
{
    readonly T _progress;
    readonly TaskCompletionSource<bool> _tcs;

    readonly ProductInfo _info;
    readonly AppInstallItem _item;

    public Task<bool> Task => _tcs.Task;

    public void Cancel()
    {
        try
        {
            if (_item.IsInstalled)
                _item.Pause();
            else
                _item.Cancel();
        }
        catch { }
    }

    internal InstallRequest(ProductInfo info, AppInstallItem item, T progress)
    {
        _item = item;
        _info = info;

        _tcs = new();
        _progress = progress;

        _tcs.Task.ContinueWith(OnFaulted, OnlyOnFaulted | ExecuteSynchronously);

        _item.Completed += OnCompleted;
        _item.StatusChanged += OnStatusChanged;
    }

    void OnFaulted(Task task)
    {
        try { _item.Cancel(); }
        catch { }
    }

    void OnCompleted(AppInstallItem sender, object args)
    {
        switch (sender.GetCurrentStatus().InstallState)
        {
            case Completed:
                _tcs.TrySetResult(true);
                break;

            case Canceled:
                if (!_tcs.Task.IsFaulted)
                    _tcs.TrySetResult(false);
                break;
        }
    }

    void OnStatusChanged(AppInstallItem sender, object args)
    {
        var status = sender.GetCurrentStatus();
        switch (status.InstallState)
        {
            case Paused:
                _tcs.TrySetResult(false);
                break;

            case Error:
                _tcs.TrySetException(status.ErrorCode);
                break;

            case Pending or Downloading or Installing:
                _progress.Report((_info.Title, (int)status.PercentComplete));
                break;
        }
    }
}
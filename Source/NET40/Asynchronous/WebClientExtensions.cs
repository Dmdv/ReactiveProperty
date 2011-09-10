﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
#if WINDOWS_PHONE
using Microsoft.Phone.Reactive;
#else
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using Codeplex.Reactive.Notifier;
using System.Diagnostics.Contracts;
#endif

namespace Codeplex.Reactive.Asynchronous
{
    // TODO:other methods

    public static class WebClientExtensions
    {
        static Func<IDisposable> RegisterDownloadProgress(WebClient client, IProgress<DownloadProgressChangedEventArgs> progress)
        {
            if (progress != null)
            {
                return () =>
                    Observable.FromEvent<DownloadProgressChangedEventHandler, DownloadProgressChangedEventArgs>(
                            h => (sender, e) => h(e), h => client.DownloadProgressChanged += h, h => client.DownloadProgressChanged -= h)
                        .Subscribe(e => progress.Report(e));
            }
            else
            {
                return () => Disposable.Empty;
            }
        }

        static Func<IDisposable> RegisterUploadProgress(WebClient client, IProgress<UploadProgressChangedEventArgs> progress)
        {
            if (progress != null)
            {
                return () =>
                    Observable.FromEvent<UploadProgressChangedEventHandler, UploadProgressChangedEventArgs>(
                            h => (sender, e) => h(e), h => client.UploadProgressChanged += h, h => client.UploadProgressChanged -= h)
                        .Subscribe(e => progress.Report(e));
            }
            else
            {
                return () => Disposable.Empty;
            }
        }

        static IObservable<TEventArgs> RegisterAsyncEvent<TDelegate, TEventArgs>(WebClient client,
            Func<Action<TEventArgs>, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler,
            Func<IDisposable> progressSubscribe, Action startAsync)
        {
            var result = Observable.Create<TEventArgs>(observer =>
            {
                var progress = progressSubscribe();
                var subscription = Observable.FromEvent<TDelegate, TEventArgs>(conversion, addHandler, removeHandler)
                    .Take(1)
                    .Do(_ => progress.Dispose())
                    .Subscribe(observer);
                startAsync();
                return () =>
                {
                    client.CancelAsync();
                    subscription.Dispose();
                    progress.Dispose();
                };
            });

            Contract.Assume(result != null);
            return result;
        }

        public static IObservable<DownloadDataCompletedEventArgs> DownloadDataObservableAsync(this WebClient client, string address)
        {
            Contract.Requires<ArgumentNullException>(client != null);
            Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(address));
            Contract.Ensures(Contract.Result<IObservable<DownloadDataCompletedEventArgs>>() != null);

            return DownloadDataObservableAsyncCore(client, new Uri(address), null);
        }

        public static IObservable<DownloadDataCompletedEventArgs> DownloadDataObservableAsync(this WebClient client, Uri address)
        {
            Contract.Requires<ArgumentNullException>(client != null);
            Contract.Requires<ArgumentNullException>(address != null);
            Contract.Ensures(Contract.Result<IObservable<DownloadDataCompletedEventArgs>>() != null);

            return DownloadDataObservableAsyncCore(client, address, null);
        }

        public static IObservable<DownloadDataCompletedEventArgs> DownloadDataObservableAsync(this WebClient client, Uri address, IProgress<DownloadProgressChangedEventArgs> progress)
        {
            Contract.Requires<ArgumentNullException>(client != null);
            Contract.Requires<ArgumentNullException>(address != null);
            Contract.Requires<ArgumentNullException>(progress != null);
            Contract.Ensures(Contract.Result<IObservable<DownloadDataCompletedEventArgs>>() != null);

            return DownloadDataObservableAsyncCore(client, address, progress);
        }

        static IObservable<DownloadDataCompletedEventArgs> DownloadDataObservableAsyncCore(this WebClient client, Uri address, IProgress<DownloadProgressChangedEventArgs> progress)
        {
            Contract.Requires<ArgumentNullException>(client != null);
            Contract.Requires<ArgumentNullException>(address != null);
            Contract.Ensures(Contract.Result<IObservable<DownloadDataCompletedEventArgs>>() != null);

            return RegisterAsyncEvent<DownloadDataCompletedEventHandler, DownloadDataCompletedEventArgs>(
                client,
                h => (sender, e) => h(e),
                h => client.DownloadDataCompleted += h,
                h => client.DownloadDataCompleted -= h,
                RegisterDownloadProgress(client, progress),
                () => client.DownloadDataAsync(address));
        }

        public static IObservable<WebResponse> DownloadFileObservableAsync(this WebClient client)
        {
            return null;
        }

        public static IObservable<WebResponse> DownloadStringObservableAsync(this WebClient client)
        {
            return null;
        }

        public static IObservable<WebResponse> OpenReadObservableAsync(this WebClient client)
        {
            return null;
        }

        public static IObservable<WebResponse> OpenWriteObservableAsync(this WebClient client)
        {
            return null;
        }

        public static IObservable<WebResponse> UploadDataObservableAsync(this WebClient client)
        {
            return null;
        }

        public static IObservable<WebResponse> UploadFileObservableAsync(this WebClient client)
        {
            return null;
        }
    }
}
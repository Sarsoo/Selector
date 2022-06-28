using System;
using System.Threading.Tasks;

namespace Selector
{
    public interface IConsumer
    {
        public void Subscribe(IWatcher watch = null);
        public void Unsubscribe(IWatcher watch = null);
    }

    public interface IConsumer<T>: IConsumer
    {
        public void Callback(object sender, T e);
    }

    public interface IPlayerConsumer: IConsumer<ListeningChangeEventArgs>
    { }

    public interface IPlaylistConsumer : IConsumer<PlaylistChangeEventArgs>
    { }
}

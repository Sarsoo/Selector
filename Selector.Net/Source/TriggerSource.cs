namespace Selector.Net;

public class TriggerSource<T> : BaseSource<T>
{
    public void Trigger(T obj)
    {
        Emit(obj);
    }
}


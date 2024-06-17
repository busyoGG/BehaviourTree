
public class Singleton<T> where T : new()
{
    private static T _instance;
    // ����һ����ʶȷ���߳�ͬ��
    private static readonly object locker = new object();

    public static T Ins()
    {
        lock (locker)
        {
            if (_instance == null)
            {
                _instance = new T();
            }
            return _instance;
        }
    }

    protected Singleton() { }
}

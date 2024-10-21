
public class DDOL : Singleton<DDOL>
{
    protected override void Awake_()
    {
        DontDestroyOnLoad(this);
    }

    protected override void OnDestroy_()
    {

    }
}
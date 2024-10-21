namespace Boom.Mono
{
    using UnityEngine;
    using UnityEngine.Events;

    public class SimpleOnLoginStateChange : MonoBehaviour
    {
        //Events
        [SerializeField] UnityEvent onFetchingUserData;
        [SerializeField] UnityEvent onLoggedIn;
        [SerializeField] UnityEvent onLoggedOut;


        //First log out happens right at the start of the game, it initialize BoomManager.
        //  Wee don't care about this first log out, there fore we use this field to ignore it.
        bool hasIgnoredFirstLoggedout = false;

        private void Start()
        {
            UserUtil.AddListenerMainDataChange<MainDataTypes.LoginData>(EnableButtonHandler, new() { invokeOnRegistration = true });
        }

        //Unregister from events
        private void OnDestroy()
        {
            UserUtil.RemoveListenerMainDataChange<MainDataTypes.LoginData>(EnableButtonHandler);
        }
        //Handle whether or not the button must be disabled
        private void EnableButtonHandler(MainDataTypes.LoginData data)
        {

            if(data.state == MainDataTypes.LoginData.State.FetchingUserData)
            {
                onFetchingUserData.Invoke();
            }
            else if (data.state == MainDataTypes.LoginData.State.LoggedIn)
            {
                onLoggedIn.Invoke();
            }
            else if (data.state == MainDataTypes.LoginData.State.Logedout)
            {
                if(hasIgnoredFirstLoggedout == false)
                {
                    hasIgnoredFirstLoggedout = true;
                    return;
                }

                onLoggedOut.Invoke();
            }
        }
    }
}
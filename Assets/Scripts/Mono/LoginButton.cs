namespace Boom.UI
{
    using Boom.Patterns.Broadcasts;
    using Boom;
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Button))]
    public class LoginButton : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField, ShowOnly] bool isEmbeddedAgent;
        [SerializeField, ShowOnly] bool noneInteractable;
        [SerializeField, ShowOnly] MainDataTypes.LoginData.State loginState;

        //Register to events
        private void Awake()
        {
            button.onClick.AddListener(Handler);

            BroadcastState.Register<WaitingForResponse>(AllowButtonInteractionHandler, new() { invokeOnRegistration = true });

            UserUtil.AddListenerMainDataChange<MainDataTypes.LoginData>(EnableButtonHandler, new() { invokeOnRegistration = true });
        }

        //Unregister from events
        private void OnDestroy()
        {
            button.onClick.RemoveListener(Handler);

            BroadcastState.Unregister<WaitingForResponse>(AllowButtonInteractionHandler);

            UserUtil.RemoveListenerMainDataChange<MainDataTypes.LoginData>(EnableButtonHandler);
        }

        //Handle whether or not the button must be interactable
        private void AllowButtonInteractionHandler(WaitingForResponse response)
        {
            noneInteractable = response.value;
            button.gameObject.SetActive(!isEmbeddedAgent);
            button.interactable = !noneInteractable && loginState == MainDataTypes.LoginData.State.Logedout;
        }
        //Handle whether or not the button must be disabled
        private void EnableButtonHandler(MainDataTypes.LoginData data)
        {
            loginState = data.state;
            isEmbeddedAgent = data.isEmbeddedAgent;
            button.gameObject.SetActive(!isEmbeddedAgent);
            button.interactable = !noneInteractable && loginState == MainDataTypes.LoginData.State.Logedout;
        }

        //Execute Login Request
        public void Handler()
        {
            Broadcast.Invoke<UserLoginRequest>();
        }
    }
}

namespace Boom.Mono
{
    using Boom.Values;
    using UnityEngine;
    using UnityEngine.Events;

    public class OnLoginStateChange : MonoBehaviour
    {
        //Set it to true if you want to check  the login state right on the Start
        [SerializeField] bool checkOnStart = false;

        //Se to false if you want the condition to be true as long as the login state is different to the expected state
        [SerializeField] bool equalTo = true;
        [SerializeField] MainDataTypes.LoginData.State expectedState;

        //This will hold the value of whether or not the condition is met
        InitValue<bool> conditionMet;

        //Events
        [SerializeField] UnityEvent onConditionMetTrue;
        [SerializeField] UnityEvent onConditionMetFalse;
        [SerializeField] UnityEvent<bool> onConditionMetToggle;

        private void Start()
        {
            UserUtil.AddListenerMainDataChange<MainDataTypes.LoginData>(EnableButtonHandler, new() { invokeOnRegistration = checkOnStart });
        }

        //Unregister from events
        private void OnDestroy()
        {
            UserUtil.RemoveListenerMainDataChange<MainDataTypes.LoginData>(EnableButtonHandler);
        }
        //Handle whether or not the button must be disabled
        private void EnableButtonHandler(MainDataTypes.LoginData data)
        {
            bool _conditionMet = equalTo ? data.state == expectedState : data.state != expectedState;

            if (_conditionMet && !conditionMet.Value)
            {
                onConditionMetTrue.Invoke();
                onConditionMetToggle.Invoke(true);
            }
            else if (!_conditionMet && (conditionMet.Value || !conditionMet.IsInit))
            {
                onConditionMetFalse.Invoke();
                onConditionMetToggle.Invoke(false);
            }

            conditionMet.Value = _conditionMet;
        }
    }

}

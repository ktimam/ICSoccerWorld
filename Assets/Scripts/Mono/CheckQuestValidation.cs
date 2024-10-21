namespace Boom.Mono
{
    using Boom;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Events;

    public class CheckQuestValidation : MonoBehaviour
    {
        public enum Mode { OnStart, OnEnable }
        public enum QuestState { None, Valid, Invalid, Checking, Error }
        [SerializeField, ShowOnly] QuestState questState = QuestState.None;

        [SerializeField] string actionQuestId;
        [SerializeField] Mode checkMode;

        [SerializeField] UnityEvent onIsValid;
        [SerializeField] UnityEvent onIsInvalid;

        private void Start()
        {
            if (checkMode == Mode.OnStart)
            {
                Check().Forget();
            }
        }

        private void OnEnable()
        {
            if (checkMode == Mode.OnEnable)
            {
                Check().Forget();
            }
        }

        private async UniTaskVoid Check()
        {
            questState = QuestState.Checking;

            var result = await ActionUtil.CanClaimGuildQuest(actionQuestId);

            if (result.IsErr)
            {
                questState = QuestState.Error;

                Debug.LogError(result.AsErr());
                return;
            }

            if (result.AsOk())
            {
                questState = QuestState.Valid;
                onIsValid.Invoke();
            }
            else
            {
                questState = QuestState.Invalid;
                onIsInvalid.Invoke();
            }
        }
    }

}
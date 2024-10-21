namespace Boom.Tutorials
{
    using Boom.Utility;
    using Boom.Values;
    using Boom;
    using Cysharp.Threading.Tasks;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using System;

    public class TutorialActionTimeIntervalConstraint : MonoBehaviour
    {
        #region FIELDS

        //This is the text that display the user's  action tries left.
        [SerializeField] TMP_Text triesLeftText;
        //This is the text that display the action's return value (Outcomes or an error).
        [SerializeField] TMP_Text actionLogText;
        //This is the button that triggers the action "match_outcome_won"
        [SerializeField] Button actionButton;

        //This is the a coroutine cache from displaying the logs. It is used to stop it when required.
        private Coroutine logCoroutine;

        //The action ID
        string actionId = "match_outcome_won";

        #endregion


        #region MONO

        private void Awake()
        {
            //This is to clear out any unwanted listener
            actionButton.onClick.RemoveAllListeners();

            //Register to action button click
            actionButton.onClick.AddListener(ActionButtonClickHandler);

            //We register LoginDataChangeHandler to MainDataTypes.LoginData change event to initialize userNameInputField with the user's username
            UserUtil.AddListenerMainDataChange<MainDataTypes.LoginData>(LoginDataChangeHandler);

            //Register to the user's actions state change event to be able to update the triesLeftText
            UserUtil.AddListenerDataChangeSelf<DataTypes.ActionState>(DataTypeActionStateChangeHandler);
        }
        private void OnEnable()
        {
            actionLogText.text = "...";
        }

        private void OnDestroy()
        {
            //Unregister to action button click
            actionButton.onClick.RemoveListener(ActionButtonClickHandler);

            //We unregister from MainDataTypes.LoginData change event
            UserUtil.RemoveListenerMainDataChange<MainDataTypes.LoginData>(LoginDataChangeHandler);

            UserUtil.RemoveListenerDataChangeSelf<DataTypes.ActionState>(DataTypeActionStateChangeHandler);
        }

        #endregion


        #region ACTION 

        //This function is just a wrapper so that we can register "ExecuteAction" function on the Action Button's onClick event
        public void ActionButtonClickHandler()
        {
            //Forget() is included as we dont care awaiting for the result
            ExecuteAction().Forget();
        }

        public async UniTaskVoid ExecuteAction()
        {
            if (logCoroutine != null) StopCoroutine(logCoroutine);

            //SECTION A: We check if there is any try left to execute the action is handled
            //automatically by "ActionUtil.ProcessAction". If the action make use of an TimeIntervalConstraint
            //"ActionUtil.ProcessAction" will check if you have tries left to execute the action, otherwise,
            //it will result on a error returned by "ActionUtil.ProcessAction".


            //SECTION B: Action execution

            //Here we execute the action by passing the actionId we wantto execute.
            actionLogText.text = $"Processing Action of id: {actionId}";
            var actionResult = await ActionUtil.ProcessAction(actionId);

            //SECTION C: Error handling

            //Here we handle the errors
            bool isError = actionResult.IsErr;

            if (isError)
            {
                string errorMessage = actionResult.AsErr().content;

                Debug.LogError(errorMessage);
                logCoroutine = StartCoroutine(DisplayTempLog(errorMessage));

                return;
            }

            //SECTION D: Cast action result to the OK return type (action outcomes)

            //This is the result if the execution of the action was successful
            var expectedResult = actionResult.AsOk();

            //SECTION E: Break down the caller action outcome and console log them

            //We get the outcomes of the user who executed the action
            var callerOutcomes = expectedResult.callerOutcomes;

            //We get the outcomes of the user who executed the action
            var entityOutcomes = callerOutcomes.entityOutcomes;

            string message = "";
            List<KeyValue<string, double>> outcomesToDisplay = new();

            //We loop through all the entity outcomes
            foreach (var keyValue in entityOutcomes)
            {
                var entityEdit = keyValue.Value;

                string entityId = entityEdit.eid;

                //Try get the entity's config field's value
                bool configEntityNameFound = entityEdit.TryGetConfigFieldAs<string>
                    //World Id
                    (BoomManager.Instance.WORLD_CANISTER_ID,
                    //Config's field name
                    "name",
                    //result
                    out string entityName,
                    //default value of the result
                    "None");

                //If config doesn't exist for the entity we just skip it
                if (configEntityNameFound == false)
                {
                    $"Could not find the config field name of entityId: {entityId}".Warning();
                    continue;
                }

                string editedFieldName = "amount";

                bool fieldAmountFound = entityEdit.TryGetOutcomeFieldAsDouble(editedFieldName, out var amount);

                //If the amount field is not found on the entity we just display an error
                if (fieldAmountFound == false)
                {
                    message = $"Could not find the edited entity field's name : {editedFieldName},  of entityId: {entityId}";
                    message.Warning();
                    break;
                }

                if (amount.NumericType_ == EntityFieldEdit.Numeric.NumericType.Increment)
                {
                    outcomesToDisplay.Add(new(entityName, amount.Value));
                }
            }

            if (string.IsNullOrEmpty(message)) message = $"Rewards:\n\n{outcomesToDisplay.Reduce(e => $"> +{e.value} {e.key}", "\n")}";

            logCoroutine = StartCoroutine(DisplayTempLog(message));
        }

        #endregion


        #region LOG

        //This is to temporarily display the outcomes or errors
        IEnumerator DisplayTempLog(string message, float duration = 5f)
        {
            actionLogText.text = message;
            yield return new WaitForSeconds(duration);
            actionLogText.text = "...";
        }

        #endregion


        #region HANDLERS


        private void LoginDataChangeHandler(MainDataTypes.LoginData data)
        {
            if (data.state != MainDataTypes.LoginData.State.LoggedIn) return;

            UpdateTriesLeftText();
        }

        //We update the triesLeftText whenever the user's action state has changed
        private void DataTypeActionStateChangeHandler(Data<DataTypes.ActionState> data)
        {
            UpdateTriesLeftText();
        }
        public void UpdateTriesLeftText()
        {
            if (ActionUtil.TryGetTriesDetails(actionId, out var triesDetails) == false)
            {
                "Could not fetch tries left".Warning();
            }

            triesLeftText.text = $"{triesDetails.maxTries}/{triesDetails.triesLeft}";
        }
        #endregion
    }
}
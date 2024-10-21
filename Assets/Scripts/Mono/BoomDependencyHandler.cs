namespace Boom.Mono
{
    using System;
    using Boom;
    using Boom.Patterns.Broadcasts;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    public class BoomDependencyHandler : MonoBehaviour
    {
        [SerializeField, Header("Dependensies")] string[] actionsIds;

        [SerializeField] bool userIdEntityData;
        [SerializeField] bool userIdTokenData;
        [SerializeField] bool userIdNftData;

        [SerializeField] bool worldIdEntityData;
        [SerializeField] bool worldIdTokenData;
        [SerializeField] bool worldIdNftData;

        [SerializeField] UnityEvent<bool> onDependenciesValid = new();
        [SerializeField] UnityEvent<bool> onDependenciesInvalid = new();
        private void Start()
        {

            if (actionsIds.Length > 0)
            {
                foreach (var item in actionsIds)
                {
                    BroadcastState.Register<ActionExecutionState>(_Update, default, item);
                }
            }

            //We register LoginDataChangeHandler to MainDataTypes.LoginData change event to initialize userNameInputField with the user's username
            UserUtil.AddListenerMainDataChange<MainDataTypes.LoginData>(LoginDataChangeHandler);

            UserUtil.AddListenerDataChangeSelf<DataTypes.ActionState>(DataTypeStateChange, default);

            if (userIdEntityData) UserUtil.AddListenerDataTypeLoadingStateChangeSelf<DataTypes.Entity>(DataTypeLoadingStateChange, default);
            if (userIdTokenData) UserUtil.AddListenerDataTypeLoadingStateChangeSelf<DataTypes.Token>(DataTypeLoadingStateChange, default);
            if (userIdNftData) UserUtil.AddListenerDataTypeLoadingStateChangeSelf<DataTypes.NftCollection>(DataTypeLoadingStateChange, default);

            if (worldIdEntityData) UserUtil.AddListenerDataTypeLoadingStateChange<DataTypes.Entity>(DataTypeLoadingStateChange, default, BoomManager.Instance.WORLD_CANISTER_ID);
            if (worldIdTokenData) UserUtil.AddListenerDataTypeLoadingStateChange<DataTypes.Token>(DataTypeLoadingStateChange, default, BoomManager.Instance.WORLD_CANISTER_ID);
            if (worldIdNftData) UserUtil.AddListenerDataTypeLoadingStateChange<DataTypes.NftCollection>(DataTypeLoadingStateChange, default, BoomManager.Instance.WORLD_CANISTER_ID);

            _UpdateButton();
        }


        private void LoginDataChangeHandler(MainDataTypes.LoginData data)
        {
            if (data.state != MainDataTypes.LoginData.State.LoggedIn) return;
            _UpdateButton();
        }

        private void OnDestroy()
        {
            if (actionsIds.Length > 0)
            {
                foreach (var item in actionsIds)
                {
                    BroadcastState.Unregister<ActionExecutionState>(_Update, item);
                }
            }

            UserUtil.RemoveListenerMainDataChange<MainDataTypes.LoginData>(LoginDataChangeHandler);

            UserUtil.RemoveListenerDataChangeSelf<DataTypes.ActionState>(DataTypeStateChange);

            if (userIdEntityData) UserUtil.RemoveListenerDataTypeLoadingStateChangeSelf<DataTypes.Entity>(DataTypeLoadingStateChange);
            if (userIdTokenData) UserUtil.RemoveListenerDataTypeLoadingStateChangeSelf<DataTypes.Token>(DataTypeLoadingStateChange);
            if (userIdNftData) UserUtil.RemoveListenerDataTypeLoadingStateChangeSelf<DataTypes.NftCollection>(DataTypeLoadingStateChange);

            if (worldIdEntityData) UserUtil.RemoveListenerDataTypeLoadingStateChange<DataTypes.Entity>(DataTypeLoadingStateChange, BoomManager.Instance.WORLD_CANISTER_ID);
            if (worldIdTokenData) UserUtil.RemoveListenerDataTypeLoadingStateChange<DataTypes.Token>(DataTypeLoadingStateChange, BoomManager.Instance.WORLD_CANISTER_ID);
            if (worldIdNftData) UserUtil.RemoveListenerDataTypeLoadingStateChange<DataTypes.NftCollection>(DataTypeLoadingStateChange, BoomManager.Instance.WORLD_CANISTER_ID);
        }

        private void DataTypeStateChange(Data<DataTypes.ActionState> data)
        {
            _UpdateButton();
        }

        private void DataTypeLoadingStateChange(DataLoadingState<DataTypes.NftCollection> state)
        {
            _UpdateButton();
        }

        private void DataTypeLoadingStateChange(DataLoadingState<DataTypes.Token> state)
        {
            _UpdateButton();
        }

        private void DataTypeLoadingStateChange(DataLoadingState<DataTypes.Entity> state)
        {
            _UpdateButton();
        }

        private void _Update(ActionExecutionState change)
        {
            _UpdateButton();
        }

        private void _UpdateButton()
        {
            if (userIdEntityData)
            {
                if (UserUtil.IsDataValidSelf<DataTypes.Entity>() == false || UserUtil.IsDataLoadingSelf<DataTypes.Entity>())
                {
                    onDependenciesValid.Invoke(false);
                    onDependenciesInvalid.Invoke(true);
                    return;
                }
            }
            if (userIdTokenData)
            {
                if (UserUtil.IsDataValidSelf<DataTypes.Token>() == false || UserUtil.IsDataLoadingSelf<DataTypes.Token>())
                {
                    onDependenciesValid.Invoke(false);
                    onDependenciesInvalid.Invoke(true);
                    return;
                }
            }
            if (userIdNftData)
            {
                if (UserUtil.IsDataValidSelf<DataTypes.NftCollection>() == false || UserUtil.IsDataLoadingSelf<DataTypes.NftCollection>())
                {
                    onDependenciesValid.Invoke(false);
                    onDependenciesInvalid.Invoke(true);
                    return;
                }
            }

            string worldId = BoomManager.Instance.WORLD_CANISTER_ID;

            if (worldIdEntityData)
            {
                if (UserUtil.IsDataValid<DataTypes.Entity>(worldId) == false || UserUtil.IsDataLoading<DataTypes.Entity>(worldId))
                {
                    onDependenciesValid.Invoke(false);
                    onDependenciesInvalid.Invoke(true);
                    return;
                }
            }
            if (worldIdTokenData)
            {
                if (UserUtil.IsDataValid<DataTypes.Token>(worldId) == false || UserUtil.IsDataLoading<DataTypes.Token>(worldId))
                {
                    onDependenciesValid.Invoke(false);
                    onDependenciesInvalid.Invoke(true);
                    return;
                }
            }
            if (worldIdNftData)
            {
                if (UserUtil.IsDataValid<DataTypes.NftCollection>(worldId) == false || UserUtil.IsDataLoading<DataTypes.NftCollection>(worldId))
                {
                    onDependenciesValid.Invoke(false);
                    onDependenciesInvalid.Invoke(true);
                    return;
                }
            }


            if (actionsIds.Length > 0)
            {
                if (ActionUtil.ActionsInProcess(actionsIds))
                {
                    onDependenciesValid.Invoke(false);
                    onDependenciesInvalid.Invoke(true);
                    return;
                }
            }

            foreach (var actionId in actionsIds)
            {
                if (ActionUtil.ValidateConstraint(actionId) == false)
                {
                    onDependenciesValid.Invoke(false);
                    onDependenciesInvalid.Invoke(true);
                    return;
                }
            }


            onDependenciesValid.Invoke(true);
            onDependenciesInvalid.Invoke(false);
        }
    }
}
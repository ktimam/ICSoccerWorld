namespace Boom
{
    // Ignore Spelling: Util

    using Boom;
    using EdjCase.ICP.Agent.Agents;
    using Boom.Patterns.Broadcasts;
    using Boom.Utility;
    using Boom.Values;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Newtonsoft.Json;

    public static class UserUtil
    {
        #region Login
        public enum LoginType { User, Anon, None }

        public static void SetAsLoginIn()
        {
            UserUtil.UpdateMainData(new MainDataTypes.LoginData() { state = MainDataTypes.LoginData.State.LoginRequested });
        }

        public static bool IsLoginRequestedPending()
        {
            var loginDataResult = GetMainData<MainDataTypes.LoginData>();

            if (loginDataResult.IsErr) return false;

            var loginDataAsOk = loginDataResult.AsOk();


            return loginDataAsOk.state == MainDataTypes.LoginData.State.LoginRequested;
        }

        /// <summary>
        /// If LoginData is ever initialized this function will return a result as an Ok, being this the User/Anon LoginData
        /// Otherwise it will return a result as an Err, being this an error message
        /// </summary>
        /// <returns>Could be either the Data State Object of LoginData or an error message</returns>
        public static UResult<MainDataTypes.LoginData, string> GetLogInData()
        {
            var loginDataResult = GetMainData<MainDataTypes.LoginData>();


            if (loginDataResult.IsErr)
            {
                return new(loginDataResult.AsErr());
            }


            return new(loginDataResult.AsOk());
        }
        /// <summary>
        /// If LoginData is ever initialized this function will return a result as an Ok, being this the User/Anon Principal
        /// Otherwise it will return a result as an Err, being this an error message
        /// </summary>
        /// <returns>Could be either the user Principal or an error message</returns>
        public static UResult<string> GetPrincipal()
        {
            var result = GetLogInData();
            if (result.Tag == UResultTag.Err)
            {
                return new(new UResult<string>.ERR<string>(result.AsErr()));
            }

            return new(new UResult<string>.OK<string>(result.AsOk().principal));
        }
        /// <summary>
        /// If LoginData is ever initialized this function will return a result as an Ok, being this the User/Anon AccountIdentifier
        /// Otherwise it will return a result as an Err, being this an error message
        /// </summary>
        /// <returns>Could be either the user AccountIdentifier or an error message</returns>
        public static UResult<string> GetAccountIdentifier()
        {
            var result = GetLogInData();
            if (result.Tag == UResultTag.Err)
            {
                return new(new UResult<string>.ERR<string>(result.AsErr()));
            }

            return new(new UResult<string>.OK<string>(result.AsOk().accountIdentifier));
        }
        /// <summary>
        /// If LoginData is ever initialized this function will return a result as an Ok, being this the User/Anon Agent
        /// Otherwise it will return a result as an Err, being this an error message
        /// </summary>
        /// <returns>Could be either the user Agent or an error message</returns>
        public static UResult<IAgent, string> GetAgent()
        {
            var result = GetLogInData();
            if (result.Tag == UResultTag.Err)
            {
                return new(result.AsErr());
            }

            return new(result.AsOk().agent);
        }

        public static bool IsDataManipulable(out MainDataTypes.LoginData loginData)
        {
            loginData = default;

            var getLogInDataResult = UserUtil.GetLogInData();

            if (getLogInDataResult.Tag == UResultTag.Err)
            {
                return false;
            }

            var asOk = getLogInDataResult.AsOk();

            if (asOk.state == MainDataTypes.LoginData.State.LoggedIn || asOk.state == MainDataTypes.LoginData.State.FetchingUserData)
            {
                loginData = getLogInDataResult.AsOk();
                return true;
            }

            return false;
        }

        public static bool IsLoggedIn(out MainDataTypes.LoginData loginData)
        {
            loginData = default;

            var getLogInDataResult = UserUtil.GetLogInData();

            if (getLogInDataResult.Tag == UResultTag.Err)
            {
                return false;
            }

            var asOk = getLogInDataResult.AsOk();

            if (asOk.state == MainDataTypes.LoginData.State.LoggedIn)
            {
                loginData = getLogInDataResult.AsOk();
                return true;
            }

            return false;
        }

        public static bool IsLoggedIn()
        {
            return IsLoggedIn(out var data) && data.state == MainDataTypes.LoginData.State.LoggedIn;
        }
        #endregion

        #region DataTypes

        static readonly Dictionary<string, HashSet<string>> loadingData = new();// worldId/userId -> loading data type

        //SUBSCRIPTIONS
        public static void AddListenerRequestData<T>(this System.Action<FetchDataReq<T>> action) where T : DataTypeRequestArgs.Base
        {
            Broadcast.Register<FetchDataReq<T>>(action);
        }
        public static void RemoveListenerRequestData<T>(this System.Action<FetchDataReq<T>> action) where T : DataTypeRequestArgs.Base
        {
            Broadcast.Unregister<FetchDataReq<T>>(action);
        }

        public static void AddListenerDataChange<T>(this System.Action<Data<T>> action, BroadcastState.BroadcastSetting broadcastSetting = default, params string[] uids) where T : DataTypes.Base
        {
            if (IsLoggedIn(out var loginData))
            {
                foreach (var uid in uids)
                {
                    string _uid = uid == loginData.principal ? "self" : uid;

                    BroadcastState.Register<Data<T>>(action, broadcastSetting, _uid);
                }

                return;
            }

            foreach (var uid in uids)
                BroadcastState.Register<Data<T>>(action, broadcastSetting, uid);
        }
        public static void RemoveListenerDataChange<T>(this System.Action<Data<T>> action, params string[] uids) where T : DataTypes.Base
        {
            if (IsLoggedIn(out var loginData))
            {
                foreach (var uid in uids)
                {
                    string _uid = uid == loginData.principal ? "self" : uid;

                    BroadcastState.Unregister<Data<T>>(action, _uid);
                }

                return;
            }

            foreach (var uid in uids)
                BroadcastState.Unregister<Data<T>>(action, uid);
        }

        public static void AddListenerDataChangeSelf<T>(this System.Action<Data<T>> action, BroadcastState.BroadcastSetting broadcastSetting = default) where T : DataTypes.Base
        {
            AddListenerDataChange<T>(action, broadcastSetting, "self");
        }
        public static void RemoveListenerDataChangeSelf<T>(this System.Action<Data<T>> action) where T : DataTypes.Base
        {
            RemoveListenerDataChange<T>(action, "self");
        }


        public static void AddListenerDataTypeLoadingStateChange<T>(this System.Action<DataLoadingState<T>> action, BroadcastState.BroadcastSetting broadcastSetting = default, params string[] uids) where T : DataTypes.Base
        {
            if (IsLoggedIn(out var loginData))
            {
                foreach (var uid in uids)
                {
                    string _uid = uid == loginData.principal ? "self" : uid;

                    BroadcastState.Register<DataLoadingState<T>>(action, broadcastSetting, _uid);
                }

                return;
            }

            foreach (var uid in uids)
                BroadcastState.Register<DataLoadingState<T>>(action, broadcastSetting, uid);
        }
        public static void RemoveListenerDataTypeLoadingStateChange<T>(this System.Action<DataLoadingState<T>> action, params string[] uids) where T : DataTypes.Base
        {
            if (IsLoggedIn(out var loginData))
            {
                foreach (var uid in uids)
                {
                    string _uid = uid == loginData.principal ? "self" : uid;

                    BroadcastState.Unregister<DataLoadingState<T>>(action, _uid);
                }

                return;
            }

            foreach (var uid in uids)
                BroadcastState.Unregister<DataLoadingState<T>>(action, uid);
        }

        public static void AddListenerDataTypeLoadingStateChangeSelf<T>(this System.Action<DataLoadingState<T>> action, BroadcastState.BroadcastSetting broadcastSetting = default) where T : DataTypes.Base
        {
            AddListenerDataTypeLoadingStateChange<T>(action, broadcastSetting, "self");
        }
        public static void RemoveListenerDataTypeLoadingStateChangeSelf<T>(this System.Action<DataLoadingState<T>> action) where T : DataTypes.Base
        {
            RemoveListenerDataTypeLoadingStateChange<T>(action, "self");
        }

        //CLEAR

        public abstract class ClearMode
        {
            public abstract class Base { };
            public class All : Base { }
            public class AllBut : Base
            {
                public string[] blacklistIds;

                public AllBut(params string[] blacklistIds)
                {
                    this.blacklistIds = blacklistIds;
                }
            }
            public class Targets : Base
            {
                public string[] targetIds;

                public Targets(params string[] targetIds)
                {
                    this.targetIds = targetIds;
                }
            }
        }
        /// <summary>
        /// Clean up user data handled by type
        /// </summary>
        //
        public static void ClearData<T>(ClearMode.Base cleanUpType = default) where T : DataTypes.Base
        {
            switch (cleanUpType)
            {
                case ClearMode.AllBut e:
                    BroadcastState.ClearAll<Data<T>>(e.blacklistIds);
                    break;
                case ClearMode.Targets e:

                    e.targetIds.Iterate(k =>
                    {
                        BroadcastState.ForceInvoke<Data<T>>(j =>
                        {
                            j.Clear();
                            return j;
                        }, k);
                    });
                    break;
                default:
                    BroadcastState.ClearAll<Data<T>>();
                    break;

            }
        }

        // REQUEST
        /// <summary>
        /// Request Data from Canisters,
        /// This will trigger listeners from "CandidApiManager" which handle fetching the data
        /// After Fetching the data "UpdateData<T>" will be called
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="optional">you can set it to any argument as required, for example DataTypes.Token requires an canisterId as argument so that it knows what token canister to fetch data from</param>
        /// <param name="loadingMessage">This is the message you want to display when waiting for the data to be fetched</param>
        public static void RequestData(DataTypeRequestArgs.Base arg)
        {
            var loginData = UserUtil.GetLogInData();

            if (loginData.IsErr)
            {
                loginData.AsErr().Error(nameof(UserUtil.RequestData));

                return;
            }
            var loginDataAsOk = loginData.AsOk();

            if ((loginDataAsOk.state == MainDataTypes.LoginData.State.FetchingUserData || loginDataAsOk.state == MainDataTypes.LoginData.State.LoggedIn) == false)
            {
                $"You can only fetch data if your login state is {MainDataTypes.LoginData.State.FetchingUserData} or {MainDataTypes.LoginData.State.LoggedIn}, Current state: {loginDataAsOk.state}".Error(nameof(UserUtil.RequestData));

                return;
            }

            if (arg.uids.Length == 0)
            {
                arg.uids = new string[1] { loginDataAsOk.principal };
            }

            switch (arg)
            {
                case DataTypeRequestArgs.Entity e:

                    Broadcast.Invoke<FetchDataReq<DataTypeRequestArgs.Entity>>(new FetchDataReq<DataTypeRequestArgs.Entity>(e));

                    foreach (var uid in arg.uids)
                    {
                        string _uid = uid != loginDataAsOk.principal ? uid : "self";
                        BroadcastState.Invoke(new DataLoadingState<DataTypes.Entity>(true), false, $"{_uid}");
                    }
                    break;

                case DataTypeRequestArgs.ActionState e:

                    Broadcast.Invoke<FetchDataReq<DataTypeRequestArgs.ActionState>>(new FetchDataReq<DataTypeRequestArgs.ActionState>(e));

                    foreach (var uid in arg.uids)
                    {
                        string _uid = uid != loginDataAsOk.principal ? uid : "self";
                        BroadcastState.Invoke(new DataLoadingState<DataTypes.ActionState>(true), false, $"{_uid}");
                    }
                    break;

                case DataTypeRequestArgs.Token e:

                    Broadcast.Invoke<FetchDataReq<DataTypeRequestArgs.Token>>(new FetchDataReq<DataTypeRequestArgs.Token>(e));

                    foreach (var uid in arg.uids)
                    {
                        string _uid = uid != loginDataAsOk.principal ? uid : "self";
                        BroadcastState.Invoke(new DataLoadingState<DataTypes.Token>(true), false, $"{_uid}");
                    }
                    break;

                case DataTypeRequestArgs.NftCollection e:

                    Broadcast.Invoke<FetchDataReq<DataTypeRequestArgs.NftCollection>>(new FetchDataReq<DataTypeRequestArgs.NftCollection>(e));

                    foreach (var uid in arg.uids)
                    {
                        string _uid = uid != loginDataAsOk.principal ? uid : "self";
                        BroadcastState.Invoke(new DataLoadingState<DataTypes.NftCollection>(true), false, $"{_uid}");
                    }
                    break;

                case DataTypeRequestArgs.StakedNftCollections e:

                    Broadcast.Invoke<FetchDataReq<DataTypeRequestArgs.StakedNftCollections>>(new FetchDataReq<DataTypeRequestArgs.StakedNftCollections>(e));

                    foreach (var uid in arg.uids)
                    {
                        string _uid = uid != loginDataAsOk.principal ? uid : "self";
                        BroadcastState.Invoke(new DataLoadingState<DataTypes.StakedNftCollections>(true), false, $"{_uid}");
                    }
                    break;
            }
        }

        public static void RequestDataSelf<T>() where T : DataTypeRequestArgs.Base, new()
        {
            T arg = new()
            {
                uids = new string[0]
            };

            RequestData(arg);
        }

        //UPDATE

        /// <summary>
        /// Use it to update the data of a given DataType, this will add or override entries, but will not remove them,
        /// the entries are only removed by calling CleanUpUserData()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newVals"></param>

        public static void UpdateData<T>(string uid, params T[] newVals) where T : DataTypes.Base
        {
            //#if UNITY_EDITOR
            if (newVals != null) $">>> DATA of type {typeof(T).Name}, key: {uid}, has been edited.\nKeys to store:\n{newVals.Reduce(e => $"* {e.GetKey()}, value: {JsonConvert.SerializeObject(e)}", ",\n")}".Log(nameof(UserUtil));
            //#endif

            var loginData = UserUtil.GetLogInData();

            if (loginData.IsErr)
            {
                loginData.AsErr().Error(nameof(UserUtil.UpdateData));

                return;
            }
            var loginDataAsOk = loginData.AsOk();

            if ((loginDataAsOk.state == MainDataTypes.LoginData.State.FetchingUserData || loginDataAsOk.state == MainDataTypes.LoginData.State.LoggedIn) == false)
            {
                $"You can only update data if your login state is {MainDataTypes.LoginData.State.FetchingUserData} or {MainDataTypes.LoginData.State.LoggedIn}, Current state: {loginDataAsOk.state}".Error(nameof(UserUtil.UpdateData));

                return;
            }



            if (uid == loginDataAsOk.principal) uid = "self";

            //

            BroadcastState.Invoke(new DataLoadingState<T>(false), false, $"{uid}");

            //

            if (BroadcastState.GetUpdateCount<Data<T>>(uid) == 0)
            {
                BroadcastState.Set<Data<T>>(new(uid, new(), e => e.GetKey(), newVals), uid);
                return;
            }

            BroadcastState.ForceInvoke<Data<T>>(e =>
            {
                ////#if UNITY_EDITOR
                //            $"Before Update of: {typeof(T).Name}\nCurrent Keys:\n{e.data.elements.Reduce(k => $"* {k.Key}, value: {JsonConvert.SerializeObject(k.Value)}", ",\n")}".Log(nameof(UserUtil));
                ////#endif
                ///

                return new(uid, e, k => k.GetKey(), newVals);

            }, uid);
        }
        public static void UpdateDataSelf<T>(params T[] newVals) where T : DataTypes.Base
        {
            UpdateData("self", newVals);
        }

        //GET


        public static bool IsDataValid<T>(params string[] uids) where T : DataTypes.Base
        {
            if (IsDataManipulable(out var loginData) == false)
            {
                //"Issue getting loginData!".Error();

                return false;
            }

            for (int i = 0; i < uids.Length; i++)
            {
                var uid = uids[i];
                if (uids[i] == loginData.principal) uid = "self";

                if (BroadcastState.GetUpdateCount<Data<T>>(uid) == 0)
                {
                    //$"Could not find data of type {typeof(T)} of id: {uid.SimplifyAddress()}".Error();
                    return false;
                }
            }

            return true;
        }
        public static bool IsDataValidSelf<T>() where T : DataTypes.Base
        {
            return IsDataValid<T>("self");
        }

        public static bool IsDataLoading<T>(params string[] uids) where T : DataTypes.Base
        {
            string selfPrincipal = GetPrincipal().AsOkorDefault().Value;

            for (int i = 0; i < uids.Length; i++)
            {
                var uid = uids[i];
                if (uid == selfPrincipal) uid = "self";

                if (BroadcastState.TryRead<DataLoadingState<T>>(out var loadingState, $"{uid}"))
                {
                    if (loadingState.isLoading) return true;
                }
            }

            return false;
        }

        public static bool IsDataLoadingSelf<T>() where T : DataTypes.Base
        {
            return IsDataLoading<T>("self");
        }

        //

        public static UResult<Data<T>, string> GetData<T>(string uid) where T : DataTypes.Base
        {

            if (IsLoggedIn(out var loginData))
            {
                if (uid == loginData.principal) uid = "self";

                if (BroadcastState.TryRead<Data<T>>(out var val, uid) == false)
                {
                    return new($"Data could not be found for DataType: {typeof(T).Name}");
                }

                return new(val);
            }
            else
            {
                if(loginData.state != MainDataTypes.LoginData.State.FetchingUserData) return new("You cannot get shared data as an anon user!");

                if (loginData.principal == uid) uid = "self";
                if (string.IsNullOrEmpty(uid)) uid = "self";


                if (BroadcastState.TryRead<Data<T>>(out var val, uid) == false)
                {
                    uid = "self";

                    if (BroadcastState.TryRead<Data<T>>(out val, uid) == false)
                    {
                        return new($"Data could not be found for DataType: {typeof(T).Name}");
                    }
                }

                return new(val);
            }
        }

        public static UResult<Data<T>, string> GetDataSelf<T>() where T : DataTypes.Base
        {
            return GetData<T>("self");
        }


        public static UResult<(Data<DataTypes.Entity> entityData, Data<DataTypes.Token> tokenData, Data<DataTypes.NftCollection> nftData, Data<DataTypes.ActionState> actionStateData), string> GetAllData(string uid)
        {
            var entityDataResult = GetData<DataTypes.Entity>(uid);

            if (entityDataResult.IsErr) return new($"{entityDataResult.AsErr()}");

            var entityDataAsOk = entityDataResult.AsOk();

            //

            var tokenDataResult = GetData<DataTypes.Token>(uid);

            if (tokenDataResult.IsErr) return new($"{tokenDataResult.AsErr()}");

            var tokenDataAsOk = tokenDataResult.AsOk();

            //

            var nftDataResult = GetData<DataTypes.NftCollection>(uid);

            if (nftDataResult.IsErr) return new($"{nftDataResult.AsErr()}");

            var nftDataAsOk = nftDataResult.AsOk();

            //

            var sctionStateDataResult = GetData<DataTypes.ActionState>(uid);

            if (sctionStateDataResult.IsErr) return new($"{sctionStateDataResult.AsErr()}");

            var actionStateDataAsOk = sctionStateDataResult.AsOk();

            return new((entityDataAsOk, tokenDataAsOk, nftDataAsOk, actionStateDataAsOk));
        }

        public static UResult<(Data<DataTypes.Entity> entityData, Data<DataTypes.Token> tokenData, Data<DataTypes.NftCollection> nftData, Data<DataTypes.ActionState> actionStateData), string> GetAllDataSelf()
        {
            return GetAllData("self");
        }

        //

        /// <summary>
        /// Try get an element from collection of "T" Type that derive from Datatypes.Base
        /// </summary>
        /// <typeparam name="T">Datatypes.Base</typeparam>
        /// <returns></returns>
        public static UResult<T, string> GetElementOfType<T>(string uid, string elementId) where T : DataTypes.Base
        {
            var result = GetData<T>(uid);
            if (result.IsErr)
            {
                return new(result.AsErr());
            }

            var asOk = result.AsOk();

            if (asOk.elements.TryGetValue(elementId, out var element) == false) return new($"Data Type: {typeof(T).Name} does not contain element of id: {elementId}.\n\nValid Ids:\n\n{asOk.elements.Reduce(e => $"-{e.Key}", "\n\n")}\nend.");
            return new(element);
        }

        public static UResult<T, string> GetElementOfTypeSelf<T>(string elementId) where T : DataTypes.Base
        {
            return GetElementOfType<T>("self", elementId);
        }

        /// <summary>
        /// Get a collection of "T" Type that derive from Datatypes.Base
        /// </summary>
        /// <typeparam name="T">Datatypes.Base</typeparam>
        /// <returns></returns>
        public static UResult<LinkedList<T>, string> GetElementsOfType<T>(string uid) where T : DataTypes.Base
        {
            var result = GetData<T>(uid);
            if (result.IsErr)
            {
                return new(result.AsErr());
            }

            LinkedList<T> elements = new();
            result.AsOk().elements.Iterate(e =>
            {
                elements.AddLast(e.Value);
            });

            return new(elements);
        }
        public static UResult<LinkedList<T>, string> GetElementsOfTypeSelf<T>() where T : DataTypes.Base
        {
            return GetElementsOfType<T>("self");
        }

        /// <summary>
        /// Try get a property value from an element from collection of "T" Type that derive from Datatypes.Base
        /// </summary>
        /// <typeparam name="T">Datatypes.Base</typeparam>
        /// <returns></returns>
        public static UResult<PropertyType, string> GetPropertyFromType<T, PropertyType>(string uid, string elementId, Func<T, PropertyType> getter) where T : DataTypes.Base
        {
            var restult = GetElementOfType<T>(uid, elementId);

            if (restult.Tag == UResultTag.Err)
            {
                return new(restult.AsErr());
            }

            try
            {
                PropertyType propertyType = getter(restult.AsOk());

                return new(propertyType);
            }
            catch (Exception e)
            {
                return new(e.Message);
            }
        }
        public static UResult<PropertyType, string> GetPropertyFromTypeSelf<T, PropertyType>(string elementId, Func<T, PropertyType> getter) where T : DataTypes.Base
        {
            return GetPropertyFromType<T, PropertyType>("self", elementId, getter);
        }

        public static PropertyType GetPropertyFromType<T, PropertyType>(string uid, string elementId, Func<T, PropertyType> getter, PropertyType defaultVal = default) where T : DataTypes.Base
        {
            var restult = GetElementOfType<T>(uid, elementId);

            if (restult.Tag == UResultTag.Err)
            {
#if UNITY_EDITOR
                Debug.LogWarning("Failure to find property of element: " + elementId);
#endif

                return defaultVal;
            }

            PropertyType propertyType = getter(restult.AsOk());

            return propertyType;
        }
        public static PropertyType GetPropertyFromTypeSelf<T, PropertyType>(string elementId, Func<T, PropertyType> getter, PropertyType defaultVal = default) where T : DataTypes.Base
        {
            return GetPropertyFromType<T, PropertyType>("self", elementId, getter, defaultVal);
        }

        #endregion

        #region DataTypes Main

        public static void AddListenerMainDataChange<T>(this System.Action<T> action, BroadcastState.BroadcastSetting broadcastSetting = default) where T : MainDataTypes.Base, new()
        {
            BroadcastState.Register<T>(action, broadcastSetting);
        }
        public static void RemoveListenerMainDataChange<T>(this System.Action<T> action) where T : MainDataTypes.Base, new()
        {
            BroadcastState.Unregister<T>(action);
        }


        //CLEAR

        /// <summary>
        /// Clean up user data handled by type
        /// </summary>
        //
        public static void ClearMainData<T>() where T : MainDataTypes.Base, new()
        {
            BroadcastState.TryDispose<T>(out var disposedValue);
        }

        //UPDATE

        public static void UpdateMainData<T>(T newVal) where T : MainDataTypes.Base, new()
        {
            //#if UNITY_EDITOR
            if (newVal != null) $"DATA of type {typeof(T).Name}, has been edited, value: {JsonConvert.SerializeObject(newVal)}".Log(nameof(UserUtil));
            //#endif

            BroadcastState.ForceInvoke<T>(newVal);
        }

        //GET
        public static bool IsMainDataValid<T>() where T : MainDataTypes.Base, new()
        {
            if (BroadcastState.TryRead<T>(out var val) == false)
            {
                return false;
            }

            return true;
        }

        public static UResult<T, string> GetMainData<T>() where T : MainDataTypes.Base, new()
        {
            if (BroadcastState.TryRead<T>(out var val) == false)
            {
                return new($"Data could not be found for DataType: {typeof(T).Name}");
            }

            return new(val);
        }

        #endregion

    }
}
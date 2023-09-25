using Candid;
using Boom.Patterns.Broadcasts;
using Boom.UI;
using Boom.Values;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Boom.Utility;
using SoccerSim;
using Cysharp.Threading.Tasks;
using EdjCase.ICP.Agent.Agents;
using EdjCase.ICP.Candid.Models;
//using UnityEditor.PackageManager.UI;
using SoccerSim.SoccerSimClient;
using EdjCase.ICP.Agent.Agents.Http;
using System.Net.Http;
using UnityEngine.SceneManagement;

public class LoginWindow : Window
{
    public class WindowData
    {
    }
    public bool live;
    public Button incrementBtn;
    public Button doubleBtn;
    public Button getBtn;


    public Button logInBtn;
    public Button logOutBtn;
    public Button playMatchBtn;
    public TextMeshProUGUI logInStateTxt;
    public TextMeshProUGUI loadingTxt;
    public TextMeshProUGUI principalTxt;
    public GameObject pageControl;

    readonly List<Type> typesToLoad = new();

    bool? initialized;

    public override bool RequireUnlockCursor()
    {
        return true;
    }
    public override void Setup(object data)
    {
        UserUtil.RegisterToLoginDataChange(UpdateWindow, true);
        UserUtil.RegisterToDataChange<DataTypes.Entity>(UpdateWindow);
        UserUtil.RegisterToDataChange<DataTypes.Action>(UpdateWindow);
        UserUtil.RegisterToDataChange<DataTypes.Stake>(UpdateWindow);
        UserUtil.RegisterToDataChange<DataTypes.NftCollection>(UpdateWindow);

        logInBtn.onClick.AddListener(LogIn);
        logOutBtn.onClick.AddListener(LogoutUser);
        playMatchBtn.onClick.AddListener(PlayMatch);

        loadingTxt.text = "Loading...";
        principalTxt.text = "";

        typesToLoad.Add(typeof(DataTypes.Entity));
        typesToLoad.Add(typeof(DataTypes.Action));
        typesToLoad.Add(typeof(DataTypes.Stake));
        typesToLoad.Add(typeof(DataTypes.NftCollection));
    }

    private void OnDestroy()
    {
        LoginManager.Instance.CancelLogin();

        logInBtn.onClick.RemoveListener(LogIn);
        logOutBtn.onClick.RemoveListener(LogoutUser);

        UserUtil.UnregisterToLoginDataChange(UpdateWindow);
        UserUtil.UnregisterToDataChange<DataTypes.Entity>(UpdateWindow);
        UserUtil.UnregisterToDataChange<DataTypes.Action>(UpdateWindow);
        UserUtil.UnregisterToDataChange<DataTypes.Stake>(UpdateWindow);
        UserUtil.UnregisterToDataChange<DataTypes.NftCollection>(UpdateWindow);
    }

    private void UpdateWindow(DataState<Data<DataTypes.Entity>> state)
    {
        if (state.IsReady() && typesToLoad.Count > 0)
        {
            if (typesToLoad.Contains(typeof(DataTypes.Entity)))
            {
                typesToLoad.Remove(typeof(DataTypes.Entity));
            }
        }

        var loginDataStateResult = UserUtil.GetLogInDataState();
        if (loginDataStateResult.IsOk) UpdateWindow(loginDataStateResult.AsOk());
    }
    private void UpdateWindow(DataState<Data<DataTypes.Action>> state)
    {
        if (state.IsReady() && typesToLoad.Count > 0)
        {
            if (typesToLoad.Contains(typeof(DataTypes.Action)))
            {
                typesToLoad.Remove(typeof(DataTypes.Action));
            }
        }

        var loginDataStateResult = UserUtil.GetLogInDataState();
        if (loginDataStateResult.IsOk) UpdateWindow(loginDataStateResult.AsOk());
    }
    private void UpdateWindow(DataState<Data<DataTypes.Stake>> state)
    {
        if (state.IsReady() && typesToLoad.Count > 0)
        {
            if (typesToLoad.Contains(typeof(DataTypes.Stake)))
            {
                typesToLoad.Remove(typeof(DataTypes.Stake));
            }
        }

        var loginDataStateResult = UserUtil.GetLogInDataState();
        if (loginDataStateResult.IsOk) UpdateWindow(loginDataStateResult.AsOk());
    }
    private void UpdateWindow(DataState<Data<DataTypes.NftCollection>> state)
    {
        if (UserUtil.IsDataValid<DataTypes.NftCollection>(Env.Nfts.BOOM_COLLECTION_CANISTER_ID) && typesToLoad.Count > 0)
        {
            if (typesToLoad.Contains(typeof(DataTypes.NftCollection)))
            {
                typesToLoad.Remove(typeof(DataTypes.NftCollection));
            }
        }

        var loginDataStateResult = UserUtil.GetLogInDataState();
        if (loginDataStateResult.IsOk) UpdateWindow(loginDataStateResult.AsOk());
    }

    private void UpdateWindow(DataState<LoginData> state)
    {
        bool isLoading = state.IsLoading();
        var getIsLoginResult = UserUtil.GetLogInType();

        //logInBtn.interactable = state.IsReady();
        //logOutBtn.interactable = state.IsReady();

        if (getIsLoginResult.Tag == UResultTag.Ok)
        {
            if(getIsLoginResult.AsOk() == UserUtil.LoginType.User)
            {
                var isUserDataLoaded =
                    UserUtil.IsDataValid<DataTypes.Entity>() &&
                    UserUtil.IsDataValid<DataTypes.Action>() &&
                    UserUtil.IsDataValid<DataTypes.Stake>() &&
                    UserUtil.IsDataValid<DataTypes.NftCollection>(Env.Nfts.BOOM_COLLECTION_CANISTER_ID);

                logInBtn.gameObject.SetActive(false);

                if (isUserDataLoaded || (initialized.HasValue && initialized.Value))
                {
                    initialized = true;

                    logInStateTxt.text = "Logged In";
                    principalTxt.text = $"Principal: <b>\"{state.data.principal}\"</b>\nAccountId: <b>\"{state.data.accountIdentifier}\"</b>";
                    pageControl.SetActive(true);
                    logOutBtn.gameObject.SetActive(true);
                    loadingTxt.text = "";
                }
                else
                {
                    loadingTxt.text = $"{typesToLoad.Reduce(e=> $"Loading {e.Name} Data...","\n")}";
                }
            }
            else//Logged In As Anon
            {
                if (initialized.HasValue && initialized.Value)
                {
                    typesToLoad.Add(typeof(DataTypes.Entity));
                    typesToLoad.Add(typeof(DataTypes.Action));
                    typesToLoad.Add(typeof(DataTypes.Stake));
                    typesToLoad.Add(typeof(DataTypes.NftCollection));
                }
                initialized = false;


                logInStateTxt.text = "";//"Logged in as Anon";
                principalTxt.text = ""; //$"Principal: <b>\"{state.data.principal}\"</b>\nAccountId: <b>\"{state.data.accountIdentifier}\"</b>";
                pageControl.SetActive(false);
                logInBtn.gameObject.SetActive(true);
                logOutBtn.gameObject.SetActive(false);
                loadingTxt.text = "Please login";
            }
        }
        else
        {
            if (isLoading) loadingTxt.text = state.LoadingMsg;
            else loadingTxt.text = "Loading...";
            logInStateTxt.text = "";
            principalTxt.text = $"";
            pageControl.SetActive(false);
            logInBtn.gameObject.SetActive(false);
            logOutBtn.gameObject.SetActive(false);
        }
    }

    //

    public void CancelWalletIntegration()
    {
        Close();
    }

    private async void PlayMatch()
    {
        var agent = new HttpAgent();
        //ic network
        Principal canisterId = Principal.FromText("pcxn6-vyaaa-aaaal-ac2xq-cai");
        if(!live)
        {
            var httpClient = new DefaultHttpClient(new HttpClient
            {
                BaseAddress = (new Uri("http://localhost:40435"))
            });
            agent = new HttpAgent(httpClient);
            //Local network
            canisterId = Principal.FromText("bkyz2-fmaaa-aaaaa-qaaaq-cai");
        }
        var client = new SoccerSimClientApiClient(agent, canisterId);
        var result = await client.PlayMatch((ulong) DateTime.Now.Millisecond);// 110174);
        PlayerPrefs.SetString("snapshot", result);
        SceneManager.LoadScene("MatchField", LoadSceneMode.Single);
    }

    private void LogoutUser()
    {
        PlayerPrefs.SetString("authTokenId", string.Empty);
        Broadcast.Invoke<UserLogout>();
    }

    //Login
    public void LogIn()
    {
        if (BroadcastState.TryRead<DataState<LoginData>>(out var dataState))
        {
            if (dataState.IsLoading()) return;
        }

        PlayerPrefs.SetString("walletType", "II");
        UserUtil.StartLogin("Logging In...");
    }
}
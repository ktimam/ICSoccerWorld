namespace Boom
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Candid.Extv2Boom;
    using Candid.World;
    using Candid.WorldHub;
    using Cysharp.Threading.Tasks;
    using EdjCase.ICP.Agent.Agents;
    using EdjCase.ICP.Agent.Identities;
    using EdjCase.ICP.Candid.Models;
    using Boom.Patterns.Broadcasts;
    using Boom.Utility;
    using Boom.Values;
    using UnityEngine;
    using Candid.IcrcLedger;
    using EdjCase.ICP.BLS;
    using Newtonsoft.Json;
    using Candid;

    public class BoomManager : Singleton<BoomManager>
    {
        [SerializeField] bool enableBoomLogs = true;

        [field: SerializeField] public BoomSettings BoomSettings { private set; get; }
        public string WORLD_HUB_CANISTER_ID { get => BoomSettings.WorldHubId; }
        public string WORLD_CANISTER_ID { get => BoomSettings.WorldId; }
        public string GAMING_GUILDS_CANISTER_ID { get => BoomSettings.GamingGuildsId; }

        [field: SerializeField] public string WORLD_COLLECTION_CANISTER_ID { private set; get; } = "6uvic-diaaa-aaaap-abgca-cai";

        public enum GameType { SinglePlayer, Multiplayer, WebsocketMultiplayer }

        //Cache
        [field: SerializeField] public GameType BoomDaoGameType { private set; get; } = GameType.SinglePlayer;
        [ShowOnly, SerializeField] InitValue<IAgent> cachedAnonAgent;
        [ShowOnly, SerializeField] InitValue<string> cachedUserAddress;

        // Canister APIs
        public WorldApiClient WorldApiClient { get; private set; }
        public WorldApiClient GuildApiClient { get; private set; }
        public WorldHubApiClient WorldHub { get; private set; }


        [SerializeField, ShowOnly] string principal;
        [SerializeField, ShowOnly] bool isLoginIn;

        [SerializeField, ShowOnly] float nextUpdateIn;
        [SerializeField] float secondsToUpdateClient = 2;
        [SerializeField, ShowOnly] private long lastClientUpdate;


        [SerializeField, ShowOnly] bool inRoom;
        [SerializeField, ShowOnly] string currentRoomId;
        [SerializeField, ShowOnly] string[] usersInRoom;

        [SerializeField] bool multPlayerActionStateFetch;
        [SerializeField] bool multPlayerTokenFetch;
        [SerializeField] bool multPlayerCollectionFetch;

        bool configsRequested;

        [SerializeField, ShowOnly] string principalId;
        [SerializeField, ShowOnly] MainDataTypes.LoginData.State loginState;
        [SerializeField, ShowOnly] bool loginCompleted;

        protected override void Awake_()
        {
            Broadcast.Register<LoginManager.IndetityJson>(OnLoginCompleted);

            BroadcastState.Invoke(new WaitingForResponse(true));

            IAgent CreateAgentWithRandomIdentity(bool useLocalHost = false)
            {
                IAgent randomAgent = null;

                var httpClient = new UnityHttpClient();
#if UNITY_WEBGL && !UNITY_EDITOR
                var bls = new BypassedBlsCryptography ();
#else
                var bls = new WasmBlsCryptography();
#endif

                try
                {
                    if (useLocalHost)
                        randomAgent = new HttpAgent(Ed25519Identity.Generate(), new Uri("http://localhost:4943"), bls);
                    else
                        randomAgent = new HttpAgent(httpClient, Ed25519Identity.Generate(), bls);
                }
                catch (Exception e)
                {
                    e.ToString().Error();
                }

                return randomAgent;
            }

            instance = this;

            Broadcast.Register<UserLoginRequest>(FetchHandler);

            Broadcast.Register<UserLogout>(UserLogoutHandler);

            Broadcast.Register<FetchListings>(FetchHandler);

            UserUtil.AddListenerMainDataChange<MainDataTypes.LoginData>(LoginDataChangeHandler, new() { invokeOnRegistration = true });

            UserUtil.AddListenerRequestData<DataTypeRequestArgs.Entity>(FetchHandler);
            UserUtil.AddListenerRequestData<DataTypeRequestArgs.ActionState>(FetchHandler);
            UserUtil.AddListenerRequestData<DataTypeRequestArgs.Token>(FetchHandler);
            UserUtil.AddListenerRequestData<DataTypeRequestArgs.NftCollection>(FetchHandler);
            UserUtil.AddListenerRequestData<DataTypeRequestArgs.StakedNftCollections>(FetchHandler);

            UserUtil.AddListenerDataChange<DataTypes.Entity>(SelfDataChangeHandler, new() { invokeOnSet = true }, WORLD_CANISTER_ID);
            UserUtil.AddListenerDataChangeSelf<DataTypes.Entity>(SelfDataChangeHandler, new() { invokeOnSet = true });
            UserUtil.AddListenerDataChangeSelf<DataTypes.ActionState>(SelfDataChangeHandler, new() { invokeOnSet = true });
            UserUtil.AddListenerDataChangeSelf<DataTypes.Token>(SelfDataChangeHandler, new() { invokeOnSet = true });
            UserUtil.AddListenerDataChangeSelf<DataTypes.NftCollection>(SelfDataChangeHandler, new() { invokeOnSet = true });

            InitializeCandidApis(CreateAgentWithRandomIdentity(), true).Forget();
        }

        private void LoginDataChangeHandler(MainDataTypes.LoginData data)
        {
            principalId = data.principal;
            loginState = data.state;
        }

        private void SelfDataChangeHandler(Data<DataTypes.Entity> data)
        {
            HandleLoginCompletion();
        }
        private void SelfDataChangeHandler(Data<DataTypes.ActionState> data)
        {
            HandleLoginCompletion();
        }

        private void SelfDataChangeHandler(Data<DataTypes.Token> data)
        {
            HandleLoginCompletion();
        }

        private void SelfDataChangeHandler(Data<DataTypes.NftCollection> data)
        {
            HandleLoginCompletion();
        }

        private void HandleLoginCompletion()
        {
            if (loginCompleted) return;

            loginCompleted =
                UserUtil.IsDataValid<DataTypes.Entity>(WORLD_CANISTER_ID) &&

                UserUtil.IsDataValidSelf<DataTypes.Entity>() &&
                UserUtil.IsDataValidSelf<DataTypes.ActionState>() &&
                UserUtil.IsDataValidSelf<DataTypes.Token>() &&
                UserUtil.IsDataValidSelf<DataTypes.NftCollection>();

            if (loginCompleted)
            {
                var loginDataResult = UserUtil.GetLogInData();

                if (loginDataResult.IsErr)
                {
                    loginDataResult.AsErr().Error();
                    return;
                }

                var loginDataOk = loginDataResult.AsOk();
                UserUtil.UpdateMainData(new MainDataTypes.LoginData(loginDataOk, MainDataTypes.LoginData.State.LoggedIn, LoginManager.Instance.IsEmbeddedAgent));
                BroadcastState.Invoke(new WaitingForResponse(false));
            }
        }



        protected override void OnDestroy_()
        {
            Broadcast.Unregister<LoginManager.IndetityJson>(OnLoginCompleted);

            Broadcast.Unregister<UserLoginRequest>(FetchHandler);

            Broadcast.Unregister<UserLogout>(UserLogoutHandler);

            Broadcast.Unregister<FetchListings>(FetchHandler);

            UserUtil.RemoveListenerMainDataChange<MainDataTypes.LoginData>(LoginDataChangeHandler);

            UserUtil.RemoveListenerRequestData<DataTypeRequestArgs.Entity>(FetchHandler);
            UserUtil.RemoveListenerRequestData<DataTypeRequestArgs.ActionState>(FetchHandler);

            UserUtil.RemoveListenerRequestData<DataTypeRequestArgs.Token>(FetchHandler);
            UserUtil.RemoveListenerRequestData<DataTypeRequestArgs.NftCollection>(FetchHandler);
            UserUtil.RemoveListenerRequestData<DataTypeRequestArgs.StakedNftCollections>(FetchHandler);

            UserUtil.RemoveListenerDataChange<DataTypes.Entity>(SelfDataChangeHandler, WORLD_CANISTER_ID);
            UserUtil.RemoveListenerDataChangeSelf<DataTypes.Entity>(SelfDataChangeHandler);
            UserUtil.RemoveListenerDataChangeSelf<DataTypes.ActionState>(SelfDataChangeHandler);
            UserUtil.RemoveListenerDataChangeSelf<DataTypes.Token>(SelfDataChangeHandler);
            UserUtil.RemoveListenerDataChangeSelf<DataTypes.NftCollection>(SelfDataChangeHandler);

            //WEBSOCKET
            if (BoomDaoGameType == BoomManager.GameType.WebsocketMultiplayer)
            {
                //TODO: DISCONNECT WEBSOCKET
            }
        }

        int frameCount;
        private void Update()
        {
            DebugUtil.enableBoomLogs = enableBoomLogs;

            ++frameCount;
            if (BoomDaoGameType == GameType.Multiplayer)
            {
                nextUpdateIn = (lastClientUpdate - MainUtil.Now()) / 1000f;
                if (lastClientUpdate <= MainUtil.Now())
                {
                    lastClientUpdate = MainUtil.Now() + (long)(secondsToUpdateClient * 1000);

                    if (frameCount > 0) FetchRoomData();
                }

            }
            else if (BoomDaoGameType == GameType.SinglePlayer)
            {
            }
        }
        //


        void OnLoginCompleted(LoginManager.IndetityJson json)
        {
            var isLoggedIn = UserUtil.IsLoggedIn();

            if (isLoggedIn == false)
            {
                CreateAgentUsingIdentityJson(json.data, false).Forget();
                return;
            }

            "You already have an Agent created".Log();
        }
        public async UniTaskVoid CreateAgentUsingIdentityJson(string json, bool useLocalHost = false)
        {
            await UniTask.SwitchToMainThread();

            try
            {
                var identity = Identity.DeserializeJsonToIdentity(json);

                var httpClient = new UnityHttpClient();

#if UNITY_WEBGL && !UNITY_EDITOR
                var bls = new BypassedBlsCryptography ();
#else
                var bls = new WasmBlsCryptography();
#endif
                if (useLocalHost) await InitializeCandidApis(new HttpAgent(identity, new Uri("http://localhost:4943"), bls));
                else await InitializeCandidApis(new HttpAgent(httpClient, identity, bls));

                "You have logged in".Log();
            }
            catch (Exception e)
            {
                e.Message.Error();
            }
        }

        private void UserLogoutHandler(UserLogout obj)
        {
            var loginData = UserUtil.GetLogInData();

            if (loginData.IsErr)
            {
                loginData.AsErr().Error(GetType().Name);
                return;
            }

            if (loginData.AsOk().state != MainDataTypes.LoginData.State.LoggedIn)
            {
                $"To log out you must be logged in. Current state: {loginData.AsOk().state}".Error(GetType().Name);
                return;
            }

            LoginManager.Instance.IsEmbeddedAgent = false;

            loginCompleted = false;

            UserUtil.ClearData<DataTypes.Entity>();
            UserUtil.ClearData<DataTypes.ActionState>();
            UserUtil.ClearData<DataTypes.Token>();
            UserUtil.ClearData<DataTypes.NftCollection>();

            PlayerPrefs.SetString("authTokenId", string.Empty);
            InitializeCandidApis(cachedAnonAgent.Value, true).Forget();

            configsRequested = false;

            "You have logged out".Log(GetType().Name);

            //WEBSOCKET
            if (BoomDaoGameType == BoomManager.GameType.WebsocketMultiplayer)
            {
                //TODO: DISCONNECT WEBSOCKET
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actiontType">If the type is "Update" then must use the return value once at a time to record the update call</param>
        /// <returns></returns>
        private async UniTask InitializeCandidApis(IAgent agent, bool asAnon = false)
        {
            var userPrincipal = agent.Identity.GetPublicKey().ToPrincipal().ToText();
            string userAccountIdentity;
            //Check if anon setup is required
            if (asAnon)
            {
                //If anon Agent is cached then set it up
                if (cachedAnonAgent.IsInit)
                {
                    userAccountIdentity = cachedUserAddress.Value;
                    agent = cachedAnonAgent.Value;

                    //Build Interfaces
                    WorldHub = new WorldHubApiClient(agent, Principal.FromText(WORLD_HUB_CANISTER_ID));
                    WorldApiClient = new WorldApiClient(agent, Principal.FromText(WORLD_CANISTER_ID));
                    GuildApiClient = new WorldApiClient(agent, Principal.FromText(GAMING_GUILDS_CANISTER_ID));
                }
                //Else fetch required dependencies and catch it
                else
                {
                    //Build Interfaces
                    WorldHub = new WorldHubApiClient(agent, Principal.FromText(WORLD_HUB_CANISTER_ID));
                    WorldApiClient = new WorldApiClient(agent, Principal.FromText(WORLD_CANISTER_ID));
                    GuildApiClient = new WorldApiClient(agent, Principal.FromText(GAMING_GUILDS_CANISTER_ID));

                    cachedAnonAgent.Value = agent;

                    userAccountIdentity = await WorldHub.GetAccountIdentifier(userPrincipal);
                    cachedUserAddress.Value = userAccountIdentity;
                }


                //Set Login Data
                UserUtil.UpdateMainData(new MainDataTypes.LoginData(agent, userPrincipal, userAccountIdentity, MainDataTypes.LoginData.State.Logedout, LoginManager.Instance.IsEmbeddedAgent, "Null"));
            }
            else
            {
                //var loginData = UserUtil.GetLogInData();

                //UserUtil.UpdateMainData(new MainDataTypes.LoginData(loginData.AsOk(), LoginManager.Instance.IsEmbeddedAgent));


                //Build Interfaces
                WorldHub = new WorldHubApiClient(agent, Principal.FromText(WORLD_HUB_CANISTER_ID));
                WorldApiClient = new WorldApiClient(agent, Principal.FromText(WORLD_CANISTER_ID));
                GuildApiClient = new WorldApiClient(agent, Principal.FromText(GAMING_GUILDS_CANISTER_ID));

                userAccountIdentity = await WorldHub.GetAccountIdentifier(userPrincipal);

                "Try Fetch User Data".Log(GetType().Name);

                //HERE: YOU CAN REQUEST FOR THE FIRST TIME ON THE GAME THE USER DATA

                //Set Login Data

                //TODO: REMOVE THIS CODE AND UNCOMMENT THE ONE BELOW TO ENABLE FETCHING THE BOOM STAKING TIER
                string tier = "Null";
                if (GAMING_GUILDS_CANISTER_ID == Env.CanisterIds.GAMING_GUILDS.STAGING)
                {
                    var tierResult = await GuildApiClient.GetUserBoomStakeTier(userPrincipal);

                    tier = tierResult.Tag == Candid.World.Models.Result2Tag.Ok ? tierResult.AsOk() : tierResult.AsErr();
                }
                
                //var tierResult = await GuildApiClient.GetUserBoomStakeTier(userPrincipal);

                //string tier = tierResult.Tag == Candid.World.Models.Result2Tag.Ok? tierResult.AsOk() : tierResult.AsErr();

                UserUtil.UpdateMainData(new MainDataTypes.LoginData(agent, userPrincipal, userAccountIdentity, MainDataTypes.LoginData.State.FetchingUserData, LoginManager.Instance.IsEmbeddedAgent, tier));

                //USER DATA
                UserUtil.RequestData(new DataTypeRequestArgs.Entity(userPrincipal, WORLD_CANISTER_ID));

                UserUtil.RequestDataSelf<DataTypeRequestArgs.ActionState>();

                await UniTask.WaitUntil(() => UserUtil.IsMainDataValid<MainDataTypes.AllTokenConfigs>());

                //WE REQUEST USER TOKENS

                var allTokensConfigResult = ConfigUtil.GetAllTokenConfigs();

                if (allTokensConfigResult.IsErr)
                {
                    allTokensConfigResult.AsErr().Warning();
                    return;
                }

                var tokensToFetch = allTokensConfigResult.AsOk();

                var tokensToFetchIds = tokensToFetch.Map(e =>
                {
                    return e.canisterId;
                });
                UserUtil.RequestData(new DataTypeRequestArgs.Token(tokensToFetchIds.ToArray()));

                await UniTask.WaitUntil(() => UserUtil.IsMainDataValid<MainDataTypes.AllNftCollectionConfig>());

                //WE REQUEST USER NFTs

                var nftsToFetchResult = ConfigUtil.GetAllNftConfigs();

                if (nftsToFetchResult.IsErr)
                {
                    nftsToFetchResult.AsErr().Warning();
                    return;
                }

                var nftsToFetch = nftsToFetchResult.AsOk();

                var nftsToFetchIds = nftsToFetch.Map(e =>
                {
                    return e.canisterId;
                });

                UserUtil.RequestData(new DataTypeRequestArgs.NftCollection(nftsToFetchIds.ToArray()));

                //WEBSOCKET
                if (BoomDaoGameType == BoomManager.GameType.WebsocketMultiplayer)
                {
                    //TODO: CONNECT WEBSOCKET
                }

                UserUtil.RequestData(new DataTypeRequestArgs.StakedNftCollections(userPrincipal));
            }

            isLoginIn = false;

            //INIT CONFIGS
            if (!configsRequested)
            {
                configsRequested = true;
                await FetchConfigs();
            }
        }


        #region Fetch

        private async UniTask FetchConfigs()
        {
            //HERE: You can specify all World's Ids you want to fetch entity configs from
            string[] worlds = new string[] { WORLD_CANISTER_ID };


            //Set Configs
            var configsResult =
                await FetchUtil.ProcessWorldCall<Dictionary<string, MainDataTypes.AllConfigs.Config>>(
                    async (worldInterface, wid) =>
                    {
                        var stableConfigs = await worldInterface.GetAllConfigs();

                        return stableConfigs.Map(e => new MainDataTypes.AllConfigs.Config(e)).ToDictionary(e => e.cid);
                    },
                    worlds
                );

            if (configsResult.IsOk)
            {
                var asOk = configsResult.AsOk();

                UserUtil.UpdateMainData(new MainDataTypes.AllConfigs(asOk));
            }
            else
            {
                throw new(configsResult.AsErr());
            }

            //Set Tokens & Nft Configs
            FetchTokenConfig().Forget();
            FetchNftConfig().Forget();

            //Set Actions
            var actionsResult =
                await FetchUtil.ProcessWorldCall<Dictionary<string, MainDataTypes.AllAction.Action>>(
                    async (worldInterface, wid) =>
                    {
                        var stableConfigs = await worldInterface.GetAllActions();

                        return stableConfigs.Map(e => new MainDataTypes.AllAction.Action(e)).ToDictionary(e => e.aid);
                    },
                    worlds
                );

            if (actionsResult.IsOk)
            {
                var asOk = actionsResult.AsOk();

                UserUtil.UpdateMainData(new MainDataTypes.AllAction(asOk));
            }
            else
            {
                actionsResult.AsErr().Error(GetType().Name);
            }

            BroadcastState.Invoke(new WaitingForResponse(false));
        }

        //Entities And ActionsState

        private async UniTask FetchEntities(DataTypeRequestArgs.Entity arg)
        {
            await UniTask.SwitchToMainThread();

            var uids = arg.uids;

            var worldResult = await FetchUtil.GetAllEntities(WORLD_CANISTER_ID, uids);
            var guildsResult = await FetchUtil.GetAllEntities(GAMING_GUILDS_CANISTER_ID, uids);

            if (worldResult.IsOk)
            {
                var asOk = worldResult.AsOk();

                asOk.Iterate(user =>
                {
                    UserUtil.UpdateData(user.Key, user.Value.ToArray());
                });

                //NEW
                if (asOk.ContainsKey(principal)) HandleLoginCompletion();
            }
            else
            {
                $"DATA of type {nameof(DataTypes.Entity)} from World: {WORLD_CANISTER_ID} failed to load. Message: {worldResult.AsErr()}".Error(nameof(BoomManager));
            }

            if (guildsResult.IsOk)
            {
                var asOk = guildsResult.AsOk();

                asOk.Iterate(user =>
                {
                    UserUtil.UpdateData(user.Key, user.Value.Map(e=> new DataTypes.Badge(e.wid, e.eid, e.fields)).ToArray());
                });

                //NEW
                if (asOk.ContainsKey(principal)) HandleLoginCompletion();
            }
            else
            {
                $"DATA of type {nameof(DataTypes.Badge)} from BGG: {GAMING_GUILDS_CANISTER_ID} failed to load. Message: {guildsResult.AsErr()}".Error(nameof(BoomManager));
            }
        }
        private async UniTask FetchActionStates(DataTypeRequestArgs.ActionState arg)
        {
            await UniTask.SwitchToMainThread();

            var uids = arg.uids;

            var result = await FetchUtil.GetAllActionState(WORLD_CANISTER_ID, uids);

            if (result.IsOk)
            {
                var asOk = result.AsOk();

                asOk.Iterate(user =>
                {
                    UserUtil.UpdateData(user.Key, user.Value.ToArray());
                });

                //NEW
                if (asOk.ContainsKey(principal)) HandleLoginCompletion();
            }
            else
            {
                $"DATA of type {nameof(DataTypes.ActionState)} failed to load. Message: {result.AsErr()}".Warning(nameof(BoomManager));
            }
        }

        //Tokens
        private async UniTask FetchToken(DataTypeRequestArgs.Token arg)
        {
            await UniTask.SwitchToMainThread();

            var uids = arg.uids;

            //
            var result = await FetchUtil.GetAllTokens(arg.canisterIds, uids);

            if (result.IsOk)
            {
                var asOk = result.AsOk();

                asOk.Iterate(user =>
                {
                    UserUtil.UpdateData(user.Key, user.Value.Map(token => new DataTypes.Token(token.Key, token.Value)).ToArray());
                });

                //NEW
                if (asOk.ContainsKey(principal)) HandleLoginCompletion();
            }
            else
            {
                $"DATA of type {nameof(DataTypes.Token)} failed to load. Message: {result.AsErr()}".Warning(nameof(BoomManager));
            }
        }

        //Nfts
        private async UniTask FetchNfts(DataTypeRequestArgs.NftCollection arg)
        {
            await UniTask.SwitchToMainThread();

            var uids = arg.uids;

            //
            var result = await FetchUtil.GetAllNfts(arg.canisterIds, uids);

            if (result.IsOk)
            {
                var asOk = result.AsOk();

                asOk.Iterate(user =>
                {
                    UserUtil.UpdateData(user.Key, user.Value.ToArray());
                });

                //NEW
                if (asOk.ContainsKey(principal)) HandleLoginCompletion();
            }
            else
            {
                $"DATA of type{nameof(DataTypes.ActionState)} failed to load. Message: {result.AsErr()}".Warning(nameof(BoomManager));
            }
        }

        //Configs
        private async UniTaskVoid FetchTokenConfig()
        {
            await UniTask.SwitchToMainThread();

            try
            {
                List<MainDataTypes.AllTokenConfigs.TokenConfig> tokens = new();

                var icpTokenInterface = new IcrcLedgerApiClient(cachedAnonAgent.Value, Principal.FromText(Env.CanisterIds.ICP_LEDGER));

                var icpDecimals = await icpTokenInterface.Icrc1Decimals();
                var icpName = await icpTokenInterface.Icrc1Name();
                var icpSymbol = await icpTokenInterface.Icrc1Symbol();
                var icpFee = await icpTokenInterface.Icrc1Fee();
                icpFee.TryToUInt64(out ulong _icpFee);

                tokens.Add(new MainDataTypes.AllTokenConfigs.TokenConfig(Env.CanisterIds.ICP_LEDGER, icpName, icpSymbol, icpDecimals, _icpFee, "This is the base Internet Computer Token", "https://cryptologos.cc/logos/internet-computer-icp-logo.png?v=026"));

                if (ConfigUtil.QueryConfigsByTag(WORLD_CANISTER_ID, "token", out var tokensMetadata))
                {
                    foreach (var tokenMetadata in tokensMetadata)
                    {
                        if (tokenMetadata.TryGetConfigFieldAs<string>("canister", out var canisterId))
                        {
                            var tokenCanisterId = canisterId;

                            if (string.IsNullOrEmpty(tokenCanisterId)) tokenCanisterId = Env.CanisterIds.ICP_LEDGER;

                            var tokenInterface = new IcrcLedgerApiClient(cachedAnonAgent.Value, Principal.FromText(tokenCanisterId));

                            var decimals = await tokenInterface.Icrc1Decimals();
                            var name = await tokenInterface.Icrc1Name();
                            var symbol = await tokenInterface.Icrc1Symbol();
                            var fee = await tokenInterface.Icrc1Fee();

                            fee.TryToUInt64(out ulong _fee);

                            if (ConfigUtil.TryGetConfig(WORLD_CANISTER_ID, e =>
                            {
                                e.TryGetConfigFieldAs<string>("canister", out var _canister, "");

                                return _canister == tokenCanisterId;
                            }, out var tokenConfig))
                            {
                                tokenConfig.TryGetConfigFieldAs("description", out var description, "");

                                tokenConfig.TryGetConfigFieldAs("url_logo", out var urlLogo, "");

                                tokens.Add(new MainDataTypes.AllTokenConfigs.TokenConfig(tokenCanisterId, name, symbol, decimals, _fee, description, urlLogo));
                            }
                        }
                        else
                        {
                            $"config of tag \"token\" doesn't have field \"canister\"".Warning();
                        }
                    }
                }
                else "No Token Config found in World Config".Warning(nameof(BoomManager));

                if (tokens.Count > 0) UserUtil.UpdateMainData(new MainDataTypes.AllTokenConfigs(tokens.ToArray().ToDictionary(e => e.canisterId)));
                else UserUtil.UpdateMainData(new MainDataTypes.AllTokenConfigs());

            }
            catch (Exception e)
            {
                e.Message.Error();
            }
        }

        private async UniTaskVoid FetchNftConfig()
        {
            await UniTask.SwitchToMainThread();

            try
            {
                List<MainDataTypes.AllNftCollectionConfig.NftConfig> collections = new();

                if (ConfigUtil.QueryConfigsByTag(WORLD_CANISTER_ID, "nft", out var nftConfigs))
                {
                    $"Collections Config fetched {JsonConvert.SerializeObject(nftConfigs)}".Log();
                    nftConfigs.Iterate(e =>
                    {
                        if (!e.TryGetConfigFieldAs<string>("name", out var collectionName))
                        {
                            $"config of tag \"nft\" doesn't have field \"collectionName\"".Warning();
                        }
                        if (!e.TryGetConfigFieldAs<string>("description", out var description))
                        {
                            $"config of tag \"nft\" doesn't have field \"description\"".Warning();
                        }
                        if (!e.TryGetConfigFieldAs<string>("url_logo", out var urlLogo))
                        {
                            $"config of tag \"nft\" doesn't have field \"url_logo\"".Warning();
                        }

                        if (!e.TryGetConfigFieldAs<bool>("is_standard", out var isStandard))
                        {
                            $"config of tag \"nft\" doesn't have field \"is_standard\"".Error();

                            return;
                        }

                        if (!e.TryGetConfigFieldAs<string>("canister", out var canisterId))
                        {
                            $"config of tag \"nft\" doesn't have field \"canister\"".Error();

                            return;
                        }

                        collections.Add(new MainDataTypes.AllNftCollectionConfig.NftConfig(canisterId, isStandard == false, collectionName, description, urlLogo));
                    });
                }
                else "No Nft Config found in World Config".Warning(nameof(BoomManager));

                if (collections.Count > 0)
                {
                    UserUtil.UpdateMainData(new MainDataTypes.AllNftCollectionConfig(collections.ToArray().ToDictionary(e => e.canisterId)));
                }
                else UserUtil.UpdateMainData(new MainDataTypes.AllNftCollectionConfig());
            }
            catch (Exception e)
            {
                e.Message.Error();
            }
        }
        //Listings
        private async UniTask FetchListings()
        {
            await UniTask.SwitchToMainThread();

            var getAgentResult = UserUtil.GetAgent();

            if (getAgentResult.Tag == UResultTag.Err)
            {
                getAgentResult.AsErr().Error();
                return;
            }

            Extv2BoomApiClient collectionInterface = new(getAgentResult.AsOk(), Principal.FromText(WORLD_COLLECTION_CANISTER_ID));

            var listingResult = await collectionInterface.Listings();

            List<MainDataTypes.AllListings.Listing> listing = new();

            foreach (var item in listingResult)
            {
                var tokenIdentifier = await BoomManager.Instance.WorldHub.GetTokenIdentifier(WORLD_COLLECTION_CANISTER_ID, item.F0);
                listing.Add(new(tokenIdentifier, item));
            }

            UserUtil.UpdateMainData(new MainDataTypes.AllListings(listing.ToArray().ToDictionary(e => e.index)));
        }

        //Rooms
        private void FetchRoomData()
        {
            if (UserUtil.IsLoggedIn(out var loginData) == false)
            {
                return;
            }


            if (EntityUtil.TryQueryAllEntitiesFeild<DataTypes.Entity>(EntityUtil.Queries.rooms, out var rooms, e => e))
            {
                $"Try Fetch Room Data Success, data: {JsonConvert.SerializeObject(rooms)}".Log();

                var allRoomsData = new MainDataTypes.AllRoomData(rooms);
                UserUtil.UpdateMainData(allRoomsData);

                UserUtil.RequestData(new DataTypeRequestArgs.Entity(WORLD_CANISTER_ID));

                var usersInCurrentRoom = allRoomsData.GetAllUsersInCurrentRoom();
                usersInCurrentRoom ??= new string[0];

                inRoom = allRoomsData.inRoom;
                currentRoomId = allRoomsData.currentRoomId;
                usersInRoom = usersInCurrentRoom;

                if (inRoom)
                {
                    UserUtil.RequestData(new DataTypeRequestArgs.Entity(usersInCurrentRoom));

                    if (multPlayerActionStateFetch) UserUtil.RequestData(new DataTypeRequestArgs.ActionState(usersInCurrentRoom));
                    if (multPlayerTokenFetch)
                    {
                        var tokenConfigsResult = ConfigUtil.GetAllTokenConfigs();
                        if (tokenConfigsResult.IsErr)
                        {
                            tokenConfigsResult.AsErr().Error();
                            return;
                        }
                        var tokenConfigs = tokenConfigsResult.AsOk();

                        UserUtil.RequestData(new DataTypeRequestArgs.Token(tokenConfigs.Map(e => e.canisterId).ToArray(), usersInCurrentRoom));
                    }
                    if (multPlayerCollectionFetch)
                    {
                        var nftConfigsResult = ConfigUtil.GetAllNftConfigs();

                        if (nftConfigsResult.IsErr)
                        {
                            nftConfigsResult.AsErr().Error();
                            return;
                        }
                        var nftConfigs = nftConfigsResult.AsOk();

                        UserUtil.RequestData(new DataTypeRequestArgs.NftCollection(nftConfigs.Map(e => e.canisterId).ToArray(), usersInCurrentRoom));
                    }
                }
            }
            else
            {
                "Try Fetch Room Data Failure".Log();
            }
        }

        //

        private async UniTask FetchStakedNfts(DataTypeRequestArgs.StakedNftCollections arg)
        {
            await UniTask.SwitchToMainThread();

            var getAgentResult = UserUtil.GetAgent();

            if (getAgentResult.Tag == UResultTag.Err)
            {
                getAgentResult.AsErr().Error();
                return;
            }

            var uids = arg.uids;

            //TODO: REMOVE THIS CODE TO ENABLE FETCHING STAKED NFT DATA IN PRODUCTION
            if(GAMING_GUILDS_CANISTER_ID != Env.CanisterIds.GAMING_GUILDS.STAGING)
            {
                UserUtil.UpdateData(uids[0], new DataTypes.StakedNftCollections[0]);
                return;
            }

            var result = await FetchUtil.GetAllStakedNFTs(GAMING_GUILDS_CANISTER_ID, uids[0]);

            if (result.IsErr)
            {
                Debug.LogError(result.AsErr());
                return;
            }

            result.AsOk().Iterate(e =>
            {
                UserUtil.UpdateData(e.Key, e.Value.ToArray());
            });
        }

        #endregion

        #region FetchHandlers
        private void FetchHandler(FetchListings arg)
        {
            FetchListings().Forget();
        }
        private void FetchHandler(UserLoginRequest arg)
        {
            if (UserUtil.IsLoginRequestedPending() || UserUtil.IsLoggedIn()) return;

            UserUtil.SetAsLoginIn();
            BroadcastState.Invoke(new WaitingForResponse(true));


            PlayerPrefs.SetString("walletType", "II");

#if UNITY_WEBGL && !UNITY_EDITOR
            LoginManager.Instance.StartLoginFlowWebGl();
            return;
#endif

            isLoginIn = true;
            LoginManager.Instance.StartLoginFlow();
        }

        private void FetchHandler(FetchDataReq<DataTypeRequestArgs.Entity> req)
        {
            //$"DATA of type {nameof(DataTypeRequestArgs.Entity)} was requested, args: {JsonConvert.SerializeObject(req.arg.uids)}".Log(nameof(CandidApiManager));


            FetchEntities(req.arg).Forget();
        }
        private void FetchHandler(FetchDataReq<DataTypeRequestArgs.ActionState> req)
        {
            //$"DATA of type {nameof(DataTypeRequestArgs.ActionState)} was requested, args: {JsonConvert.SerializeObject(req.arg.uids)}".Log(nameof(CandidApiManager));

            FetchActionStates(req.arg).Forget();
        }
        private void FetchHandler(FetchDataReq<DataTypeRequestArgs.Token> req)
        {
            //$"DATA of type {nameof(DataTypeRequestArgs.Token)} was requested, args: {JsonConvert.SerializeObject(req.arg.uids)}".Log(nameof(CandidApiManager));

            FetchToken(req.arg).Forget();
        }
        private void FetchHandler(FetchDataReq<DataTypeRequestArgs.NftCollection> req)
        {
            FetchNfts(req.arg).Forget();
        }

        private void FetchHandler(FetchDataReq<DataTypeRequestArgs.StakedNftCollections> req)
        {
            FetchStakedNfts(req.arg).Forget();
        }
        #endregion
    }
}
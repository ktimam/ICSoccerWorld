namespace Boom.Tutorials
{
    using Boom;
    using Cysharp.Threading.Tasks;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using Candid.World.Models;
    using Candid.WorldDeployer;
    using Candid.World;
    using EdjCase.ICP.Candid.Models;
    using Boom.Utility;
    using System.Collections.Generic;
    using System.Collections;

    public class WorldDeployer : MonoBehaviour
    {
        #region FIELDS

        [SerializeField] BoomSettings boomSettings;
        [Header("Specify a WorldId where you want to copy and paste configs and actions from.")]
        [SerializeField] string optionalReferenceWorldId;

        [SerializeField, ShowOnly] bool deployingWorld;
        [SerializeField, ShowOnly] bool worldJustDeployed;
        [SerializeField, ShowOnly] bool isWorldDeployed;
        [SerializeField, ShowOnly] bool questsExist = false;

        //Pages

        [SerializeField, Space(30), Header ("PAGES")] GameObject mainPage;
        [SerializeField] GameObject testPage;

        [SerializeField] GameObject createQuestSection;
        [SerializeField] GameObject openTestQuestSection;


        //CREATE WORLD FIELDS

        [SerializeField, Space(30), Header("WORLD SECTION")] TMP_Text createWorldTitleText;
        [SerializeField] TMP_Text createWorldButtonText;

        [SerializeField, Space(10)] TMP_InputField worldNameInputField;
        [SerializeField] TMP_InputField worldCoverImageInputField;

        [SerializeField, Space(10)] Button createWorldButton;
        [SerializeField] Button candidUIButton;

        //CREATE GAMING GUILDS FIELDS

        [SerializeField, Space(30), Header("GAMING GUILDS SECTION")] TMP_Text createQuestsTitleText;
        [SerializeField] TMP_Text createQuestsButtonText;

        [SerializeField, Space(10)] TMP_InputField questNameInputField_1;
        [SerializeField] TMP_InputField questDescriptionInputField_1;
        [SerializeField] TMP_InputField questCoverImageInputField_1;

        [SerializeField, Space(10)] TMP_InputField questNameInputField_2;
        [SerializeField] TMP_InputField questDescriptionInputField_2;
        [SerializeField] TMP_InputField questCoverImageInputField_2;

        [SerializeField, Space(10)] TMP_InputField questGameUrlInputField;

        [SerializeField, Space(10)] Button createGuildQuestsButton;

        [SerializeField, Space(10)] GameObject createQuestsCover;

        //TEST FIELDS

        [SerializeField, Space(30), Header("TEST SECTION")] Button testButton;

        [SerializeField, Space(10)] GameObject testQuestsCover;


        //QUESTS FIELDS
        [SerializeField, Space(30), Header("QUESTS SECTION")] Button winButton;
        [SerializeField] Button loseButton;
        [SerializeField] Button guildButton;

        [SerializeField, Space(10)] TMP_Text questLog;



        //RESET GUILD QUESTS FIELDS

        [SerializeField, Space(30), Header("RESET SECTION")] Button resetGuildQuestsButton;


        //BACK FIELDS

        [SerializeField, Space(30), Header("BACK")] Button backButton;

        //LOGS FIELDS

        [SerializeField, Space(10)] TMP_Text logText;



        private WorldDeployerApiClient worldDeployer;
        private WorldApiClient gamingGuildWorld;

        private WorldApiClient deployedWorld;

        [SerializeField, ShowOnly] private string questId_1 = "";
        [SerializeField, ShowOnly] private string questId_2 = "";

        List<StableConfig> gamingGuildsConfigs;
        List<StableEntity> userEntities;
        #endregion


        #region MONO
        private void Awake()
        {
            //CREATE WORLD SET UP

            createWorldButton.onClick.AddListener(CreateWorldClickHandler);

            candidUIButton.onClick.AddListener(OpenCandidUI);

            //CREATE GAMING GUILDS SET UP

            createGuildQuestsButton.onClick.AddListener(TryCreateQuests);

            //TEST SET UP

            testButton.onClick.AddListener(OpenTestPageHandler);

            guildButton.onClick.AddListener(OpenGuildsWebPage);

            //PLAYTEST SET UP

            winButton.onClick.AddListener(WinMatch);
            loseButton.onClick.AddListener(LoseMatch);

            //RESET GUILD QUESTS SET UP

            resetGuildQuestsButton.onClick.AddListener(ResetQuests);



            //BACK SET UP

            backButton.onClick.AddListener(OpenMainPageHandler); 

            UserUtil.AddListenerMainDataChange<MainDataTypes.LoginData>(LoginDataChangeHandler);
        }

        private void OpenTestPageHandler()
        {
            mainPage.SetActive(false);
            testPage.SetActive(true);
        }

        private void OpenMainPageHandler()
        {
            mainPage.SetActive(true);
            testPage.SetActive(false);
        }

        private void Start()
        {

            gamingGuildsConfigs = new();
            userEntities = new();

            logText.text = string.IsNullOrEmpty(boomSettings.WorldId) ? "Your TemplateBoomSettings has not yet been set up!" : $"You world id is: {boomSettings.WorldId}";
            Debug.Log($"You world id is: {boomSettings.WorldId}");

            candidUIButton.enabled = string.IsNullOrEmpty(boomSettings.WorldId) == false;

            if (!boomSettings)
            {
                Debug.LogError("Boom Settings cannot be null");
            }

            createQuestsCover.SetActive(true);
            testQuestsCover.SetActive(true);
            guildButton.gameObject.SetActive(false);

            UpdateQuestLogs(false, false, 0, 0, 0);
        }


        private void OnDestroy()
        {
            //CREATE WORLD 

            createWorldButton.onClick.RemoveListener(CreateWorldClickHandler);

            candidUIButton.onClick.RemoveListener(OpenCandidUI);

            //CREATE GAMING GUILDS

            createGuildQuestsButton.onClick.RemoveListener(TryCreateQuests);


            //TEST 

            testButton.onClick.RemoveListener(OpenTestPageHandler);

            guildButton.onClick.RemoveListener(OpenGuildsWebPage);

            //PLAYTEST SET UP

            winButton.onClick.RemoveListener(WinMatch);
            loseButton.onClick.RemoveListener(LoseMatch);

            //RESET GUILD QUEST

            resetGuildQuestsButton.onClick.RemoveListener(ResetQuests);



            //BACK

            backButton.onClick.RemoveListener(OpenMainPageHandler);

            UserUtil.RemoveListenerMainDataChange<MainDataTypes.LoginData>(LoginDataChangeHandler);

        }
        #endregion

        #region Log
        Coroutine logRoutine;
        private void Log(string message, float seconds = 3)
        {
            if(logRoutine != null) StopCoroutine(logRoutine);

            Debug.Log(message);
            logRoutine = StartCoroutine(LogRoutine(message, seconds));
        }
        private void LogError(string message, float seconds = 3)
        {
            if (logRoutine != null) StopCoroutine(logRoutine);

            Debug.LogError(message);
            logRoutine = StartCoroutine(LogRoutine(message, seconds));
        }
        IEnumerator LogRoutine(string message, float seconds)
        {
            logText.text = message;
            yield return new WaitForSeconds(seconds);
            logText.text = "...";
        }

        private void UpdateQuestLogs(bool isQuestCompleted_1, bool isQuestCompleted_2, int count, int wonCount, int lostCount)
        {
            questLog.text = "<b>Quests Status.</b>\n" +
                            $"#1 - Play Once: {(isQuestCompleted_1? "Completed": "Incomplete")}.\n" +
                            $"#2 - Win Once: {(isQuestCompleted_2 ? "Completed" : "Incomplete")}.\n\n" +
                            "<b>Match Stats.</b>\n" +
                            $"- Count: {count}\n" +
                            $"- Won Count: {wonCount}\n" +
                            $"- Lost Count: {lostCount}";
        }

        #endregion

        #region SET UP

        private void LoginDataChangeHandler(MainDataTypes.LoginData data)
        {
            if (!boomSettings) return;

            if (data.state == MainDataTypes.LoginData.State.LoggedIn)
            {
                LoginDataChangeHandlerAsync(data).Forget();
            }
            else if (data.state == MainDataTypes.LoginData.State.Logedout)
            {
                deployingWorld = false;
                worldJustDeployed = false;
                isWorldDeployed = false;
                questsExist = false;
            }
        }

        private async UniTaskVoid LoginDataChangeHandlerAsync(MainDataTypes.LoginData data)
        {
            worldDeployer = new(data.agent, Principal.FromText(boomSettings.DeploymentEnv_ == BoomSettings.DeploymentEnv.Production ? Env.CanisterIds.WORLD_DEPLOYER.PRODUCTION : Env.CanisterIds.WORLD_DEPLOYER.STAGING));
            gamingGuildWorld = new(data.agent, Principal.FromText(boomSettings.DeploymentEnv_ == BoomSettings.DeploymentEnv.Production ? Env.CanisterIds.GAMING_GUILDS.DEVELOPMENT : Env.CanisterIds.GAMING_GUILDS.STAGING));

            TrySetupDeployedWorld(data);

            await TrySetupCreateWorldFields();

            await SetupGamingGuildsConfigs();

            TryLoadUserData(data).Forget();
        }

        private void TrySetupDeployedWorld(MainDataTypes.LoginData data)
        {
            if (string.IsNullOrEmpty(boomSettings.WorldId))
            {
                return;
            }
            else
            {
                if (boomSettings.WorldEnv_ == BoomSettings.WorldEnv.None)
                {
                    Debug.LogError($"System doesn't know what enviroment the world id {boomSettings.WorldId.SimplifyAddress()} was deployed from");
                    return;
                }
            }

            deployedWorld = new(data.agent, Principal.FromText(boomSettings.WorldId));
            isWorldDeployed = true;

            createWorldTitleText.text = "Update World";
            createWorldButtonText.text = "Update";
            createQuestsCover.SetActive(false);
        }

        private async UniTask TryLoadUserData(MainDataTypes.LoginData data)
        {
            Debug.Log("Load user data");

            if (worldJustDeployed)
            {
                if (!questsExist) return;
            }
            else
            {
                if (!questsExist || !isWorldDeployed) return;
            }

            Log("Loading user's entities");

            var userEntitiesResult = await deployedWorld.GetAllUserEntities(new WorldApiClient.GetAllUserEntitiesArg0(new(), data.principal));

            if (userEntitiesResult.Tag == Result5Tag.Err)
            {
                LogError(userEntitiesResult.AsErr());
                return;
            }
            Log("User's entities has been loaded");

            userEntities = userEntitiesResult.AsOk();

            int matchCount = 0;
            int matchCountWon = 0;
            int matchCountLost = 0;

            foreach (var entity in userEntities)
            {
                if(entity.Eid == "match_count")
                {

                    foreach (var field in entity.Fields)
                    {
                        if(field.FieldName == "quantity")
                        {
                            if (float.TryParse(field.FieldValue, out var outValue) == false)
                            {
                                LogError($"Failure to pare field of name {field.FieldName}, current value \"{field.FieldValue}\"");
                            }
                            matchCount = (int) outValue;
                        }
                    }
                }
                else if (entity.Eid == "match_count_won")
                {
                    foreach (var field in entity.Fields)
                    {
                        if (field.FieldName == "quantity")
                        {
                            if (float.TryParse(field.FieldValue, out var outValue) == false)
                            {
                                LogError($"Failure to pare field of name {field.FieldName}, current value \"{field.FieldValue}\"");
                            }
                            matchCountWon = (int) outValue;
                        }
                    }
                }
                else if (entity.Eid == "match_count_lost")
                {
                    foreach (var field in entity.Fields)
                    {
                        if (field.FieldName == "quantity")
                        {
                            if (float.TryParse(field.FieldValue, out var outValue) == false)
                            {
                                LogError($"Failure to pare field of name {field.FieldName}, current value \"{field.FieldValue}\"");
                            }
                            matchCountLost = (int) outValue;
                        }
                    }
                }
            }

            //
            Log($"Loading user's quest states for quest: {questId_1} and {questId_2}");

            var isQuestCompleted_1 = await gamingGuildWorld.GetActionStatusComposite(new Candid.World.WorldApiClient.GetActionStatusCompositeArg0(questId_1, data.principal));
            var isQuestCompleted_2 = await gamingGuildWorld.GetActionStatusComposite(new Candid.World.WorldApiClient.GetActionStatusCompositeArg0(questId_2, data.principal));

            Log("User's quest states has been loaded");

            if (isQuestCompleted_1.Tag == Result8Tag.Err)
            {
                LogError(isQuestCompleted_1.AsErr());
                return;
            }
            if (isQuestCompleted_2.Tag == Result8Tag.Err)
            {
                LogError(isQuestCompleted_2.AsErr());
                return;
            }

            UpdateQuestLogs(isQuestCompleted_1.AsOk().IsValid, isQuestCompleted_2.AsOk().IsValid, matchCount, matchCountWon, matchCountLost);
        }
        private async UniTask TrySetupCreateWorldFields()
        {
            if (deployedWorld == null) return;

            var worldDetailsResult = await worldDeployer.GetWorldDetails(boomSettings.WorldId);

            if (worldDetailsResult.HasValue == false) return;

            var worldDetails = worldDetailsResult.ValueOrDefault;

            worldNameInputField.SetTextWithoutNotify(worldDetails.Name);
            worldCoverImageInputField.SetTextWithoutNotify(worldDetails.Cover);

            SetupQuestIds();
        }

        private void SetupQuestIds()
        {
            string worldName = worldNameInputField.text.ToLower().Trim().Replace(' ', '_');

            questId_1 = $"quest_{worldName}_01";
            questId_2 = $"quest_{worldName}_02";
        }

        private async UniTask SetupGamingGuildsConfigs()
        {
            gamingGuildsConfigs = await gamingGuildWorld.GetAllConfigs();

            testQuestsCover.SetActive(true);
            guildButton.gameObject.SetActive(false);

            if (!string.IsNullOrEmpty(questId_1) && !string.IsNullOrEmpty(questId_2))
            {
                foreach (var config in gamingGuildsConfigs)
                {
                    if (config.Cid == questId_1)
                    {
                        questsExist = true;

                        foreach (var configField in config.Fields)
                        {
                            if (configField.FieldName == "name")
                            {
                                questNameInputField_1.SetTextWithoutNotify(configField.FieldValue);
                            }
                            else if (configField.FieldName == "description")
                            {
                                questDescriptionInputField_1.SetTextWithoutNotify(configField.FieldValue);
                            }
                            else if (configField.FieldName == "image_url")
                            {
                                questCoverImageInputField_1.SetTextWithoutNotify(configField.FieldValue);
                            }
                            else if (configField.FieldName == "quest_url")
                            {
                                questGameUrlInputField.SetTextWithoutNotify(configField.FieldValue);
                            }
                        }
                    }
                    else if (config.Cid == questId_2)
                    {
                        questsExist = true;

                        foreach (var configField in config.Fields)
                        {
                            if (configField.FieldName == "name")
                            {
                                questNameInputField_2.SetTextWithoutNotify(configField.FieldValue);
                            }
                            else if (configField.FieldName == "description")
                            {
                                questDescriptionInputField_2.SetTextWithoutNotify(configField.FieldValue);
                            }
                            else if (configField.FieldName == "image_url")
                            {
                                questCoverImageInputField_2.SetTextWithoutNotify(configField.FieldValue);
                            }
                        }
                    }
                }
            }

            if (questsExist && !string.IsNullOrEmpty(boomSettings.WorldId))
            {
                testQuestsCover.SetActive(false);
                guildButton.gameObject.SetActive(true);
                createQuestsTitleText.text = "Update Guild Quests";
                createQuestsButtonText.text = "Update";
            }
            else
            {
                questNameInputField_1.text = "GameName";
                questDescriptionInputField_1.text = "Play a Match in GameName";
                questCoverImageInputField_1.text = "https://i.postimg.cc/x8p0gBgs/BoomDao.jpg";

                questNameInputField_2.text = "GameName";
                questDescriptionInputField_2.text = "Win a Match in GameName";
                questCoverImageInputField_2.text = "https://i.postimg.cc/x8p0gBgs/BoomDao.jpg";

                questGameUrlInputField.text = "https://qxl6y-7qaaa-aaaal-adwlq-cai.raw.icp0.io/";
            }
        }

        #endregion

        private void OpenCandidUI()
        {
            Application.OpenURL($"https://5pati-hyaaa-aaaal-qb3yq-cai.raw.icp0.io/?id={boomSettings.WorldId}");
        }

        private void OpenGuildsWebPage()
        {
            Application.OpenURL($"https://t2qzd-6qaaa-aaaak-qdbdq-cai.icp0.io/");
        }

        #region CREATE WORLD SECTION

        public void CreateWorldClickHandler() 
        {
            TryCreateWorld_().Forget();
        }
        public async UniTaskVoid TryCreateWorld_()
        {
            if (deployingWorld)
            {
                return;
            }

            if (string.IsNullOrEmpty(worldNameInputField.text))// || string.IsNullOrEmpty(worldCoverImageInputField.text))
            {
                Debug.LogError("Feilds on the Create World Section cannot be empty");
                return;
            }

            if (UserUtil.IsLoggedIn(out var loginData) == false)
            {
                Debug.LogError("You must log in!");
                return;
            }


            createWorldButton.interactable = false;
            createGuildQuestsButton.interactable = false;

            if (deployedWorld == null)
            {
                if (string.IsNullOrEmpty(boomSettings.WorldId) == false)
                {
                    createWorldButton.interactable = true;
                    LogError("Boom's Settings are already setup");
                    return;
                }

                await CreateWorld(loginData);
            }
            else
            {
                await UpdateWorld();

                testQuestsCover.SetActive(true);
            }

            createWorldButton.interactable = true;
            createGuildQuestsButton.interactable = true;
        }

        public async UniTask CreateWorld(MainDataTypes.LoginData loginData)
        {
            //SECTION A: Set up arguments
            deployingWorld = true;

            Log("Creaing a new world");

            var worldCanisterId = await worldDeployer.CreateWorldCanister(worldNameInputField.text, worldCoverImageInputField.text);
            isWorldDeployed = true;
            worldJustDeployed = true;

            SetupQuestIds();

            Log("Setting up world");

            WorldApiClient world = new WorldApiClient(loginData.agent, Principal.FromText(worldCanisterId));

            //world


            if (string.IsNullOrEmpty(optionalReferenceWorldId) == false)
            {
                Log($"Importing Actions from reference world of Id: {optionalReferenceWorldId}");

                var importActionsResult = await world.ImportAllActionsOfWorld(new WorldApiClient.ImportAllActionsOfWorldArg0(optionalReferenceWorldId));

                if (importActionsResult.Tag == Result2Tag.Err)
                {
                    LogError(importActionsResult.AsErr());
                }

                Log($"Importing Configs from reference world of Id: {optionalReferenceWorldId}");

                var importConfigsResult = await world.ImportAllConfigsOfWorld(new WorldApiClient.ImportAllConfigsOfWorldArg0(optionalReferenceWorldId));

                if (importConfigsResult.Tag == Result2Tag.Err)
                {
                   LogError(importConfigsResult.AsErr());
                }
            }

            boomSettings.WorldId = worldCanisterId;

            createWorldTitleText.text = "Update World";
            createWorldButtonText.text = "Update";
            createQuestsCover.SetActive(false);

            //boomSettings.WorldName = worldName;
            boomSettings.WorldEnv_ = boomSettings.DeploymentEnv_ == BoomSettings.DeploymentEnv.Production ? BoomSettings.WorldEnv.Production : BoomSettings.WorldEnv.Staging;

            TrySetupDeployedWorld(loginData);

            TryLoadUserData(loginData).Forget();

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(boomSettings);
#endif

            candidUIButton.enabled = string.IsNullOrEmpty(boomSettings.WorldId) == false;

            deployingWorld = false;
        }

        public async UniTask UpdateWorld()
        {
            await worldDeployer.UpdateWorldName(boomSettings.WorldId, worldNameInputField.text);
            await worldDeployer.UpdateWorldCover(boomSettings.WorldId, worldCoverImageInputField.text);
        }
        #endregion

        #region CREATE QUESTS
        private void TryCreateQuests()
        {
            CreateQuests().Forget();
        }

        private async UniTaskVoid CreateQuests()
        {
            Log(createQuestsCover.activeSelf ? $"Try create quests for your world of ID: {boomSettings.WorldId}" : $"Try update quests on your world of ID: {boomSettings.WorldId}");

            if (string.IsNullOrEmpty(questNameInputField_1.text) ||
                string.IsNullOrEmpty(questDescriptionInputField_1.text) ||
                string.IsNullOrEmpty(questCoverImageInputField_1.text) ||
                string.IsNullOrEmpty(questNameInputField_2.text) ||
                string.IsNullOrEmpty(questDescriptionInputField_2.text) ||
                string.IsNullOrEmpty(questCoverImageInputField_2.text) ||
                string.IsNullOrEmpty(questGameUrlInputField.text)
                )
            {
                LogError("Failure on the Create World Section cannot be empty");

                return;
            }

            var configs = new List<WorldApiClient.CreateTestQuestConfigsArg0Item>
            {
                new(questId_1, questDescriptionInputField_1.text, questCoverImageInputField_1.text, questNameInputField_1.text, questGameUrlInputField.text),
                new(questId_2, questDescriptionInputField_2.text, questCoverImageInputField_2.text, questNameInputField_2.text, questGameUrlInputField.text)
            };

            createWorldButton.interactable = false;
            createGuildQuestsButton.interactable = false;
            await gamingGuildWorld.CreateTestQuestConfigs(configs);


            var createQuestsResult =  await gamingGuildWorld.CreateTestQuestActions(new(questId_1, questId_2, boomSettings.WorldId));

            if(createQuestsResult.Tag == Result2Tag.Err)
            {
                LogError(createQuestsResult.AsErr());
                createGuildQuestsButton.interactable = true;
                return;
            }

            Log(createQuestsCover.activeSelf ? "Quests have been created" : "Quests have been updated");
            createGuildQuestsButton.interactable = true;
            createWorldButton.interactable = true;

            SetupGamingGuildsConfigs().Forget();

        }


        #endregion


        #region PLAYTEST

        private void WinMatch()
        {
            WinMatchAsync().Forget();
        }
        private async UniTaskVoid WinMatchAsync()
        {
            Log("Try win match");

            winButton.interactable = false;
            loseButton.interactable = false;
            resetGuildQuestsButton.interactable = false;

            if (UserUtil.IsLoggedIn(out var loginData) == false)
            {
                winButton.interactable = true;
                loseButton.interactable = true;
                resetGuildQuestsButton.interactable = true;
                LogError("You must log in!");
                return;
            }

            Log("Executing \"match_won\" action");
            var actionResult = await deployedWorld.ProcessAction(new ActionArg("match_won", new()));
            Log($"\"match_won\" action has been executed. Result: {(actionResult.Tag == Result4Tag.Ok ? "success" : "failure")}");

            if (actionResult.Tag == Result4Tag.Err)
            {
                winButton.interactable = true;
                loseButton.interactable = true;
                resetGuildQuestsButton.interactable = true;
                return;
            }

            await TryLoadUserData(loginData);

            winButton.interactable = true;
            loseButton.interactable = true;
            resetGuildQuestsButton.interactable = true;
        }


        private void LoseMatch()
        {
            LoseMatchAsync().Forget();
        }
        private async UniTaskVoid LoseMatchAsync()
        {
            Log("Try lose match");

            winButton.interactable = false;
            loseButton.interactable = false;
            resetGuildQuestsButton.interactable = false;

            if (UserUtil.IsLoggedIn(out var loginData) == false)
            {
                winButton.interactable = true;
                loseButton.interactable = true;
                resetGuildQuestsButton.interactable = true;
                LogError("You must log in!");
                return;
            }

            Log("Executing \"match_lost\" action");
            var actionResult = await deployedWorld.ProcessAction(new ActionArg("match_lost", new()));
            Log($"\"match_lost\" action has been executed. Result: {(actionResult.Tag == Result4Tag.Ok ? "success" : "failure")}");

            if (actionResult.Tag == Result4Tag.Err)
            {
                winButton.interactable = true;
                loseButton.interactable = true;
                resetGuildQuestsButton.interactable = true;
                return;
            }

            await TryLoadUserData(loginData);

            winButton.interactable = true;
            loseButton.interactable = true;
            resetGuildQuestsButton.interactable = true;
        }

        #endregion

        #region RESET QUESTS
        private void ResetQuests()
        {
            TryResetQuests().Forget();
        }
        private async UniTaskVoid TryResetQuests()
        {
            Log("Try reset quests");

            winButton.interactable = false;
            loseButton.interactable = false;
            resetGuildQuestsButton.interactable = false;

            if (UserUtil.IsLoggedIn(out var loginData) == false)
            {
                winButton.interactable = true;
                loseButton.interactable = true;
                resetGuildQuestsButton.interactable = true;
                LogError("You must log in!");
                return;
            }

            Log("Reseting quests state and history");
            await deployedWorld.DeleteActionHistoryForUser(new(loginData.principal));
            await gamingGuildWorld.DeleteTestQuestActionStateForUser(new(loginData.principal));
            Log("Quests have been reseted");

            await TryLoadUserData(loginData);

            winButton.interactable = true;
            loseButton.interactable = true;
            resetGuildQuestsButton.interactable = true;
        }
        #endregion
    }
}
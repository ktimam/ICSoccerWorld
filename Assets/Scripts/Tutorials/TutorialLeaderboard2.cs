namespace Boom
{
    using Boom.Utility;
    using Newtonsoft.Json;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    //This leaderboard script shows you two other ways of implementing leaderboard relying on the World's Data
    //The first implementation each user will use an entity in the world's data as an entry. Each entry will contain data such as username, score and timestamp
    //The second implementation each user will use a shared entity in the world's data to store their entry as a field in that shared entity. Each entry will contain data such as username, score and timestamp

    public class TutorialLeaderboard2 : MonoBehaviour
    {
        public class Entry
        {
            public string username;
            public double score;
            public long timestamp;
        }

        public enum LeaderboardMode
        {
            //This mode allows the user to store their leaderboard entry as an entity in the world
            EntityAsEntry,
            //This mode allows the user to store their leaderboard entry in a shared entity as a entiry's field in the world
            EntityFieldAsEntry
        }

        [SerializeField]
        LeaderboardMode leaderboardMode = LeaderboardMode.EntityAsEntry;
        string actionMode1 = "set_score_leaderboard_entry_1";
        string actionMode2 = "set_score_leaderboard_entry_2";

        [SerializeField] TMP_Text content;

        [SerializeField] TMP_InputField userNameInputField;

        [SerializeField] Button addButton;


        private void Awake()
        {
            UserUtil.AddListenerMainDataChange<MainDataTypes.LoginData>(LoginDataChangeHandler, new() { invokeOnRegistration = true });
            addButton.onClick.AddListener(SetLeaderboardEntry);
        }



        private void OnDestroy()
        {
            UserUtil.RemoveListenerMainDataChange<MainDataTypes.LoginData>(LoginDataChangeHandler);

        }

        private void LoginDataChangeHandler(MainDataTypes.LoginData data)
        {
            if (data.state == MainDataTypes.LoginData.State.LoggedIn)
            {
                UpdateLeaderboard();
            }
        }

        private void UpdateLeaderboard()
        {
            content.text = "";

            var loginData = UserUtil.GetLogInData().AsOk();
            var ownPrincipal = loginData.principal;

            if (leaderboardMode == LeaderboardMode.EntityAsEntry)
            {
                DisplayLeaderboardWithEntityAsEntries(ownPrincipal);
            }
            else
            {
                DisplayLeaderboardWithEntityFieldsAsEntries(ownPrincipal);
            }
        }
        private void DisplayLeaderboardWithEntityAsEntries(string ownPrincipal)
        {
            //I use try query all entities with a predefined filter that specifies that
            //I only want the entities from the world canister with a field tag of value "lb"
            EntityUtil.TryQueryEntities(EntityUtil.Queries.worldEntityFieldTagLb, out var lbEntries);

            //We initialize the entry list in case it is null
            if (lbEntries == null) lbEntries = new();

            bool userEntryExist = false;
            foreach (var entity in lbEntries)
            {
                if (entity.eid == ownPrincipal) userEntryExist = true;

                entity.TryGetFieldAsText("username", out var username);
                entity.TryGetFieldAsDouble("score", out var score);
                entity.TryGetFieldAsTimeStamp("timestamp", out var timestamp);

                content.text += $" -> Username: {username}, Score: {score},  Timestamp: {timestamp}\n";
            }
            if (userEntryExist == false) content.text += $" -> Username: {ownPrincipal.SimplifyAddress()}, Score: {0},  Timestamp: {0}\n";
        }
        private void DisplayLeaderboardWithEntityFieldsAsEntries(string ownPrincipal)
        {
            //Get all leaderboards in world
            EntityUtil.TryQueryEntities(BoomManager.Instance.WORLD_CANISTER_ID, e => e.eid.Contains("lb_"), out var leaderboards);

            //We initialize the entry list in case it is null
            if (leaderboards == null) leaderboards = new();

            DataTypes.Entity lb = null;

            foreach (var entity in leaderboards)
            {
                if (entity.eid == "lb_test")
                {
                    lb = entity;
                    break;
                }
            }

            bool userEntryExist = false;

            if (lb != null)
            {
                foreach (var lbEntry in lb.fields)
                {
                    if (lbEntry.Key == ownPrincipal) userEntryExist = true;

                    Entry entry = JsonConvert.DeserializeObject<Entry>(lbEntry.Value);

                    if (entry != null) content.text += $" -> Username: {entry.username}, Score: {entry.score},  Timestamp: {entry.timestamp}\n";
                }
            }

            if (userEntryExist == false) content.text += $" -> Username: {ownPrincipal.SimplifyAddress()}, Score: {0},  Timestamp: {0}\n";
        }


        public void SetLeaderboardEntry()
        {
            var loginData = UserUtil.GetLogInData().AsOk();


            if (leaderboardMode == LeaderboardMode.EntityAsEntry)
            {
                SetEntityAsEntry(loginData);

            }
            else
            {
                SetEntityFieldAsEntry(loginData);
            }
        }


        private void SetEntityAsEntry(MainDataTypes.LoginData loginData)
        {
            //I use try query all entities with a predefined filter that specifies that
            //I only want the entities from the world canister with a field tag of value "lb"
            EntityUtil.TryQueryEntities(EntityUtil.Queries.worldEntityFieldTagLb, out var lbEntries);

            //We initialize the entry list in case it is null
            if (lbEntries == null) lbEntries = new();

            DataTypes.Entity lbEntry = null;

            foreach (var entity in lbEntries)
            {
                if (entity.eid == loginData.principal)
                {
                    lbEntry = entity;
                    break;
                }
            }

            double currentScore = 0;
            if (lbEntry != null)
            {
                lbEntry.TryGetFieldAsDouble("score", out currentScore);
            }

            ActionUtil.ProcessAction(actionMode1, new()
        {
            new Candid.World.Models.Field() { FieldName = "username", FieldValue = string.IsNullOrEmpty(userNameInputField.text)? loginData.principal.SimplifyAddress() : userNameInputField.text },
            new Candid.World.Models.Field() { FieldName = "score", FieldValue = $"{currentScore + 1}" },
            new Candid.World.Models.Field() { FieldName = "timestamp", FieldValue = $"{MainUtil.Now()}" },
        });

            CoroutineManagerUtil.DelayAction(UpdateLeaderboard, 3f, transform);
        }

        private void SetEntityFieldAsEntry(MainDataTypes.LoginData loginData)
        {
            //Get all leaderboards in world
            EntityUtil.TryQueryEntities(BoomManager.Instance.WORLD_CANISTER_ID, e => e.eid.Contains("lb_"), out var leaderboards);

            //We initialize the entry list in case it is null
            if (leaderboards == null) leaderboards = new();

            DataTypes.Entity lb = null;

            foreach (var entity in leaderboards)
            {
                if (entity.eid == "lb_test")
                {
                    lb = entity;
                    break;
                }
            }

            double currentScore = 0;

            if (lb != null)
            {
                foreach (var lbEntry in lb.fields)
                {
                    if (lbEntry.Key == loginData.principal)
                    {
                        Entry entry = JsonConvert.DeserializeObject<Entry>(lbEntry.Value);

                        if (entry != null)
                        {
                            currentScore = entry.score;
                        }
                    }
                }
            }

            ActionUtil.ProcessAction(actionMode2, new()
            {
                new Candid.World.Models.Field() { FieldName = "json", FieldValue = JsonConvert.SerializeObject(new Entry()
                {
                    username = string.IsNullOrEmpty(userNameInputField.text)? loginData.principal.SimplifyAddress() : userNameInputField.text,
                    score = currentScore + 1,
                    timestamp = MainUtil.Now()
                })},
            }); ;

            CoroutineManagerUtil.DelayAction(UpdateLeaderboard, 3f, transform);
        }

    }
}
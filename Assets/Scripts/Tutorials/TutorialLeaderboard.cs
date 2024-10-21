using System.Collections.Generic;

namespace Boom.Tutorials
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Boom;
    using Boom.Utility;
    using Boom.Values;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class TutorialLeaderboard : MonoBehaviour
    {
        #region FIELDS

        [SerializeField, Range(3, 100)] int maxEntries = 10;

        //This is the text that display the user's rank.
        [SerializeField] TMP_Text myRankText;
        //This is the text that display the user's pricipal ID
        [SerializeField] TMP_Text myPrincipalIdText;
        //This is the text that display the user's gem value
        [SerializeField] TMP_Text myValueText;

        [SerializeField] GameObject entryTemplate;
        //This is the leaderboard content where all the entries will be created
        [SerializeField] Transform leaderboardContent;

        //This is the button that triggers the action "add_gem"
        [SerializeField] Button reloadButton;

        #endregion


        #region MONO
        private void Awake()
        {
            //This is to clear out any unwanted listener
            reloadButton.onClick.RemoveAllListeners();

            //Register to action button click
            reloadButton.onClick.AddListener(ReloadLeaderboardHandler);

            //We register LoginDataChangeHandler to MainDataTypes.LoginData change event to initialize userNameInputField with the user's username
            UserUtil.AddListenerMainDataChange<MainDataTypes.LoginData>(LoginDataChangeHandler);

            //We register EntiyDataChangeHandler to the world's DataTypes.Entity change event to update leaderboard with the world's entities
            //We specify to register to the world entity data change by passing the world's canister ID
            UserUtil.AddListenerDataChange<DataTypes.Entity>(EntiyDataChangeHandler, default, BoomManager.Instance.WORLD_CANISTER_ID);

            myRankText.text = "#";
            myPrincipalIdText.text = "xxx-xxx-xxx";
            myValueText.text = "xx";
        }

        private void OnDestroy()
        {
            //Unregister to action button click
            reloadButton.onClick.RemoveListener(ReloadLeaderboardHandler);

            //We unregister from MainDataTypes.LoginData change event
            UserUtil.RemoveListenerMainDataChange<MainDataTypes.LoginData>(LoginDataChangeHandler);

            //We unregister from DataTypes.Entity change event
            UserUtil.RemoveListenerDataChange<DataTypes.Entity>(EntiyDataChangeHandler, BoomManager.Instance.WORLD_CANISTER_ID);
        }
        #endregion


        #region Handlers

        private void ReloadLeaderboardHandler()
        {
            //If world data is already loading then return.
            if (UserUtil.IsDataLoading<DataTypes.Entity>(BoomManager.Instance.WORLD_CANISTER_ID) == true) return;

            //Request world data by passing the world canister ID
            UserUtil.RequestData(new DataTypeRequestArgs.Entity(BoomManager.Instance.WORLD_CANISTER_ID));
            "You have requested to re-fetch leaderboard".Log(typeof(TutorialLeaderboard).Name);
        }

        private void LoginDataChangeHandler(MainDataTypes.LoginData data)
        {
            //If user is not logged in, return
            if (data.state != MainDataTypes.LoginData.State.LoggedIn) return;


            //We update the inventory, we set the argument as default as it wont be used when updating the leaderboard 
            EntiyDataChangeHandler(default);
        }

        private void EntiyDataChangeHandler(Data<DataTypes.Entity> data)
        {
            //We destroy any old leaderboard GameObject entry
            foreach (Transform child in leaderboardContent.transform)
                Destroy(child.gameObject);

            //We temporarily catch the user's principal ID
            string userPrincipalId = UserUtil.GetPrincipal().AsOk().Value;

            //If entity of id "bitcoin_miners_leaderboard" is not found in
            //the world canister data then display a empty leaderboard and the user's texts with default values
            if (EntityUtil.TryGetEntities(BoomManager.Instance.WORLD_CANISTER_ID, out var entities) == false)
            {
                myRankText.text = $"#None";
                myPrincipalIdText.text = userPrincipalId.SimplifyAddress(); ;
                myValueText.text = $"{0}";
                return;
            }
         
            List<KeyValue<string, int>> topUsers = new();

            //Get Tops and my own value
            foreach (var entity in entities)
            {
                var principalId = entity.eid;

                if(entity.fields.TryGetValue("mined_bitcoin_count", out var score) == false)
                {
                    score = "0";
                }


                if(double.TryParse(score, out var value) == false)
                {
                    Debug.LogError($"Issue parsing leaderboard value : {score} to int. entry owner: {principalId}");
                    continue;
                }


                //Find lowest leaderboard entry
                var lowestLeaderboardEntryIndex = -1;
                var lowestLeaderboardEntryValue= int.MaxValue;

                for (int i = 0; i < topUsers.Count; i++)
                {
                    var topUser = topUsers[i];

                    if (topUser.value < lowestLeaderboardEntryValue)
                    {
                        lowestLeaderboardEntryValue = topUser.value;
                        lowestLeaderboardEntryIndex = i;
                    }
                }

                //If lowest topUser exist and number of top users is less than maxEntries...
                if(lowestLeaderboardEntryIndex > -1 && topUsers.Count == maxEntries)
                {
                    // if lowestLeaderboardEntryValue is lowest than "value" ...
                    if (value >= lowestLeaderboardEntryValue)
                    {
                        topUsers.RemoveAt(lowestLeaderboardEntryIndex);
                        topUsers.Add(new(principalId, (int)value));
                    }
                }
                //Otherwise just add the current entity field
                else topUsers.Add(new(principalId, (int)value));                
            }

            //Reorder topusers
            List<KeyValue<string, int>> orderedEntries = new();

            while(topUsers.Count > 0)
            {
                var highestLeaderboardEntryIndex = -1;
                var highestLeaderboardEntryValue = int.MinValue;

                for (int i = 0; i < topUsers.Count; i++)
                {
                    var topUser = topUsers[i];

                    if (topUser.value > highestLeaderboardEntryValue)
                    {
                        highestLeaderboardEntryValue = topUser.value;
                        highestLeaderboardEntryIndex = i;
                    }
                }

                if(highestLeaderboardEntryIndex != -1)
                {
                    orderedEntries.Add(topUsers[highestLeaderboardEntryIndex]);
                    topUsers.RemoveAt(highestLeaderboardEntryIndex);
                }
            }
            topUsers = topUsers.OrderBy(e => e.key).ToList();

            //Display
            orderedEntries.Iterate((e, i) =>
            {
                var newEntry = Instantiate(entryTemplate, leaderboardContent);


                var entryChildren = newEntry.GetComponentsInChildren<TMP_Text>();

                entryChildren.Iterate(k =>
                {
                    if (k.gameObject.name.Contains("Rank")) k.text = $"#{i + 1}";
                    else if (k.gameObject.name.Contains("Principal")) k.text = e.key.SimplifyAddress();
                    else if (k.gameObject.name.Contains("Value")) k.text = $"{e.value}";
                });

                //By default the template is disabled, thus, the instance is also disabled;
                //this is why we enable it
                newEntry.SetActive(true);
            });

            var myRank = orderedEntries.FindIndex(e => e.key == userPrincipalId);
            var myValue = myRank == -1 ? 0 : orderedEntries[myRank].value;

            myRankText.text = myRank == -1? "#None" : $"#{myRank + 1}";
            myPrincipalIdText.text = userPrincipalId.SimplifyAddress(); ;
            myValueText.text = $"{myValue}";
        }

        #endregion
    }
}
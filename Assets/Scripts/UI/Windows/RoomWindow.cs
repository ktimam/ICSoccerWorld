using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Boom;
using Boom.Patterns.Broadcasts;
using Boom.UI;
using Boom.Utility;
using Boom.Values;
using Candid;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class RoomWindow : Window
{
    public class WindowData
    {
    }

    [SerializeField] TextMeshProUGUI selfInfoText;
    [SerializeField] Transform content;
    private bool closeRequested;
    [SerializeField, ShowOnly] Window multiplayerActionsWindow;
    HashSet<string> listeningTo = new();


    public override bool RequireUnlockCursor()
    {
        return true;
    }

    public override void Setup(object data)
    {
        UserUtil.AddListenerMainDataChange<MainDataTypes.AllRoomData>(HandleListeningToUsers, new() { invokeOnRegistration = true });

        EntityUtil.TryGetFieldAsDouble("self", "item_b", "quantity", out var item_bQuantity);
        var testTokenAmount = TokenUtil.GetTokenAmountAsDecimal("self", "tvmv4-uqaaa-aaaap-abt5q-cai");

        
        if (UserUtil.IsLoggedIn(out var loginData))
        {
            //selfInfoText.text = $"> YOU: {loginData.principal.SimplifyAddress()}\n- Item B: {item_bQuantity}\n- Token Amount: {testTokenAmount}";
        }
    }

    private void HandleListeningToUsers(MainDataTypes.AllRoomData data)
    {
        if (data.inRoom)
        {
            var currentRoom = data.currentRoom;
            var newUsersValue = currentRoom.users;

            //Handle Resgistration

            LinkedList<string> usersLeftToStartListentTo = new(); 

            foreach (var user in newUsersValue)
            {
                if(!listeningTo.Contains(user))
                {
                    listeningTo.Add(user);
                    usersLeftToStartListentTo.AddLast(user);
                }
            }

            UserUtil.AddListenerDataChange<DataTypes.Entity>(UpdateWindow, default, usersLeftToStartListentTo.ToArray());
            UserUtil.AddListenerDataChange<DataTypes.Token>(UpdateWindow, default, usersLeftToStartListentTo.ToArray());

            //Handle Unregistration

            LinkedList<string> usersLeftToStopListentTo = new();

            foreach (var user in listeningTo)
            {
                if (!newUsersValue.Contains(user))
                {
                    listeningTo.Remove(user);
                    usersLeftToStopListentTo.AddLast(user);
                }
            }

            UserUtil.RemoveListenerDataChange<DataTypes.Entity>(UpdateWindow, usersLeftToStopListentTo.ToArray());
            UserUtil.RemoveListenerDataChange<DataTypes.Token>(UpdateWindow, usersLeftToStopListentTo.ToArray());

            //Update Window

            UpdateWindow(data);
        }
    }


    private void UpdateWindow(Data<DataTypes.Entity> data)
    {
        var allRoomData = UserUtil.GetMainData<MainDataTypes.AllRoomData>();

        if (allRoomData.IsErr)
        {
            Debug.LogError(allRoomData.AsErr());
            return;
        }

        UpdateWindow(allRoomData.AsOk());
    }
    private void UpdateWindow(Data<DataTypes.Token> data)
    {
        var allRoomData = UserUtil.GetMainData<MainDataTypes.AllRoomData>();

        if (allRoomData.IsErr)
        {
            Debug.LogError(allRoomData.AsErr());
            return;
        }

        UpdateWindow(allRoomData.AsOk());
    }

    private void UpdateWindow(MainDataTypes.AllRoomData data)
    {
        if (content == null) return;

        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }

        bool isDataValid = UserUtil.IsDataValid<DataTypes.Entity>(BoomManager.Instance.WORLD_CANISTER_ID);

        if (isDataValid)
        {
            if (data.inRoom)
            {
                var currentRoom = data.currentRoom;

                if (UserUtil.IsLoggedIn(out var loginData))
                {
                    if (currentRoom.users.Contains(loginData.principal))
                    {
                        foreach (var user in currentRoom.users)
                        {
                            if (user != loginData.principal)
                            {
                                EntityUtil.TryGetFieldAsDouble(user, "item_b", "quantity", out var item_bQuantity);
                                bool areUserEntityLoading = !UserUtil.IsDataValid<DataTypes.Entity>(user);
                                bool areUserTokenLoading = !UserUtil.IsDataValid<DataTypes.Token>(user);

                                WindowManager.Instance.AddWidgets<ActionWidgetTwo>(new ActionWidgetTwo.WindowData($"> USER: {user.SimplifyAddress()}\n- Item B: {(areUserEntityLoading ? "Loading..." : item_bQuantity)}\n- Token Amount: {(areUserTokenLoading ? "Loading..." : TokenUtil.GetTokenAmountAsDecimal(user, "tvmv4-uqaaa-aaaap-abt5q-cai"))}", "Actions", OpenMultiplayerActionsWindow, user), content);
                            }
                            else
                            {
                                EntityUtil.TryGetFieldAsDouble("self", "item_b", "quantity", out var item_bQuantity);

                                selfInfoText.text = $"> YOU: {user.SimplifyAddress()}\n- Item B: {item_bQuantity}\n- Token Amount: {TokenUtil.GetTokenAmountAsDecimal("self", "tvmv4-uqaaa-aaaap-abt5q-cai")}";
                            }

                        }
                    }
                }
                else
                {
                    Debug.LogError("Something went wrong fetching your login data");
                }
            }
        }
        else
        {
            Debug.Log("DATA ENTITY NO READY");
        }
    }


    private void OpenMultiplayerActionsWindow(object obj)
    {
        var selfPrincipal = obj != null? obj.ToString() : "";

        EntityUtil.TryGetFieldAsDouble(selfPrincipal, "item_b", "quantity", out var item_bQuantity);
        var testTokenAmount = TokenUtil.GetTokenAmountAsDecimal(selfPrincipal, "tvmv4-uqaaa-aaaap-abt5q-cai");

        multiplayerActionsWindow = WindowManager.Instance.OpenWindow<MultiplayerActionWindow>( new MultiplayerActionWindow.WindowData($"> USER: {selfPrincipal.SimplifyAddress()}", new ActionWidgetTwo.WindowData[]
        {
            new ActionWidgetTwo.WindowData($"- Test Token: {testTokenAmount}",  "Gift Token", GiftTokenToTargetUser, selfPrincipal),
            new ActionWidgetTwo.WindowData($"- Item B: {item_bQuantity}",  "Gift ItemB", GitItemB, selfPrincipal),

        }),3);
    }

    private async void GiftTokenToTargetUser(object obj)
    {
        var allRoomsResult = UserUtil.GetMainData<MainDataTypes.AllRoomData>();

        if (allRoomsResult.IsErr)
        {
            Debug.LogError(allRoomsResult.AsErr());
            return;
        }

        var allRoomsData = allRoomsResult.AsOk();

        if(allRoomsData.inRoom == false)
        {
            Debug.LogError("You are not in any room!");
            return;
        }

        if (multiplayerActionsWindow) multiplayerActionsWindow.Close();

        BroadcastState.Invoke(new WaitingForResponse(true));

        //Debug.Log($"Gift Token to: {obj}");
        var response = await ActionUtil.ProcessAction("TestMultiplayerAction_GiftToken", new List<Candid.World.Models.Field>() { new Candid.World.Models.Field("targetPrincipalId", obj.ToString()), new Candid.World.Models.Field("roomId", allRoomsData.currentRoomId) });

        BroadcastState.Invoke(new WaitingForResponse(false));


        if (response.IsErr)
        {
            Debug.LogError(response.AsErr());
            return;
        }

        DisplayActionResponse(response.AsOk());
    }
    private async void GitItemB(object obj)
    {
        var allRoomsResult = UserUtil.GetMainData<MainDataTypes.AllRoomData>();

        if (allRoomsResult.IsErr)
        {
            Debug.LogError(allRoomsResult.AsErr()); 
            return;
        }

        var allRoomsData = allRoomsResult.AsOk();

        if (allRoomsData.inRoom == false)
        {
            Debug.LogError("You are not in any room!");
            return;
        }

        if (multiplayerActionsWindow) multiplayerActionsWindow.Close();

        BroadcastState.Invoke(new WaitingForResponse(true));

        //Debug.Log($"Gift ItemB to: {obj}");
        var response = await ActionUtil.ProcessAction("TestMultiplayerAction_GiftItemB", new List<Candid.World.Models.Field>() { new Candid.World.Models.Field("targetPrincipalId", obj.ToString()), new Candid.World.Models.Field("roomId", allRoomsData.currentRoomId) });

        BroadcastState.Invoke(new WaitingForResponse(false));


        if (response.IsErr)
        {
            Debug.LogError(response.AsErr());
            return;
        }

        DisplayActionResponse(response.AsOk());
    }

    private void DisplayActionResponse(ProcessedActionResponse resonse)
    {
        List<string> inventoryElements = new();

        //NFTs
        Dictionary<string, int> collectionsToDisplay = new();

        if (resonse.targetOutcomes == null) return;


        resonse.targetOutcomes.nfts.Iterate(e =>
        {

            if (collectionsToDisplay.TryAdd(e.Canister, 1) == false) collectionsToDisplay[e.Canister] += 1;

        });

        collectionsToDisplay.Iterate(e =>
        {
            if (ConfigUtil.TryGetNftCollectionConfig(e.Key, out var collectionConfig) == false)
            {
                return;
            }

            inventoryElements.Add($"{(collectionConfig != null ? collectionConfig.name : "Name not Found")} x {e.Value}");
        });

        //Tokens
        resonse.targetOutcomes.tokens.Iterate(e =>
        {
            if (ConfigUtil.TryGetTokenConfig(e.Canister, out var tokenConfig) == false)
            {
                return;
            }

            inventoryElements.Add($"{(tokenConfig != null ? tokenConfig.name : "ICRC")} x {e.Quantity}");
        });


        //ENTITIES
        resonse.targetOutcomes.entityOutcomes.Iterate(e =>
        {
            //NEW EDIT
            //if (e.Value.fields.Has(k =>
            //{
            //    if (k.Value is EntityFieldEdit.Numeric numericOutcome) return numericOutcome.NumericType_ == EntityFieldEdit.Numeric.NumericType.Increment;
            //    return false;
            //}) == false) return;

            if (!e.Value.TryGetOutcomeFieldAsDouble("quantity", out var quantity)) return;

            //NEW EDIT
            string displayValue = "";
            if (quantity.NumericType_ == EntityFieldEdit.Numeric.NumericType.Set) displayValue = $"{quantity.Value}";
            else if (quantity.NumericType_ == EntityFieldEdit.Numeric.NumericType.Increment) displayValue = $"+ {quantity.Value}";
            else displayValue = $"- {quantity.Value}";

            if (!ConfigUtil.TryGetConfigFieldAs<string>(BoomManager.Instance.WORLD_CANISTER_ID, e.Value.eid, "name", out var configName)) return;

            if (e.Value.TryGetConfig(BoomManager.Instance.WORLD_CANISTER_ID, out var config)) inventoryElements.Add($"{configName} {displayValue}");
            else inventoryElements.Add($"{e.Value.GetKey()} {displayValue}");
        });

        WindowManager.Instance.OpenWindow<InventoryPopupWindow>(new InventoryPopupWindow.WindowData($"You have gifted to {resonse.targetOutcomes.uid.SimplifyAddress()}:", inventoryElements), 3);
    }

    public async override void Close()
    {
        BroadcastState.Invoke(new WaitingForResponse(true));//Set to false on destroy

        closeRequested = true;

        var allRoomDataResult = UserUtil.GetMainData<MainDataTypes.AllRoomData>();

        if (allRoomDataResult.IsErr)
        {
            Debug.LogError(allRoomDataResult.AsErr());
            return;
        }

        var allRoomData = allRoomDataResult.AsOk();


        var testJoinRoomArgs = new List<Candid.World.Models.Field>() { new Candid.World.Models.Field("roomId", allRoomData.currentRoomId) };
        var response = await ActionUtil.ProcessAction("TestLeaveRoom", testJoinRoomArgs);

        if (response.IsErr)
        {
            Debug.LogError(response.AsErr());
            return;
        }
        base.Close();
        Debug.Log("You have left the room");
    }

    private void OnDestroy()
    {
        BroadcastState.Invoke(new WaitingForResponse(false));

        UserUtil.RemoveListenerMainDataChange<MainDataTypes.AllRoomData>(HandleListeningToUsers);

        //    if (closeRequested) return;

        //    var allRoomDataResult = UserUtil.GetMainData<MainDataTypes.AllRoomData>();

        //    if (allRoomDataResult.IsErr)
        //    {
        //        Debug.LogError(allRoomDataResult.AsErr());
        //        return;
        //    }

        //    var allRoomData = allRoomDataResult.AsOk();

        //    var testJoinRoomArgs = new List<Candid.World.Models.Field>() { new Candid.World.Models.Field("roomId", allRoomData.currentRoomId) };

        //    ActionUtil.ProcessAction("TestLeaveRoom", testJoinRoomArgs);
    }
}

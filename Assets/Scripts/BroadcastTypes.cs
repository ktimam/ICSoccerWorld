namespace Boom
{
    using System;
    using System.Collections.Generic;
    using Boom.Patterns.Broadcasts;
    using Boom.Values;

    #region Login
    public struct UserLoginRequest : IBroadcast { }
    public struct UserLogout : IBroadcast { }

    #endregion
    
    public struct FetchDataReq<T> : IBroadcast where T : DataTypeRequestArgs.Base
    {
        public T arg;

        public FetchDataReq(T arg)
        {
            this.arg = arg;
        }
    }
    public struct DataLoadingState<T> : IBroadcastState where T : DataTypes.Base
    {
        public bool isLoading;

        public DataLoadingState(bool isLoading)
        {
            this.isLoading = isLoading;
        }

        public int MaxSavedStatesCount()
        {
            return 0;
        }
    }
    public struct FetchListings : IBroadcast { }

    public struct WaitingForResponse : IBroadcastState
    {
        public bool value;
        public string waitingMessage;

        public WaitingForResponse(bool disable, string waitingMessage = "")
        {
            this.value = disable;
            this.waitingMessage = waitingMessage;
        }

        public int MaxSavedStatesCount()
        {
            return 0;
        }
    }

    #region MarketPlace
    public struct ListingNftState : IBroadcastState
    {
        public bool isListing;

        public ListingNftState(bool isListing)
        {
            this.isListing = isListing;
        }

        public int MaxSavedStatesCount()
        {
            return 0;
        }
    }
    public struct PurchasingNftState : IBroadcastState
    {
        public bool isPurchasing;

        public PurchasingNftState(bool isPurchasing)
        {
            this.isPurchasing = isPurchasing;
        }

        public int MaxSavedStatesCount()
        {
            return 0;
        }
    }
    #endregion


    public struct ActionExecutionState : IBroadcastState
    {
        public string actionId;
        public bool inProcess;

        public ActionExecutionState(string actionId, bool inProcess)
        {
            this.actionId = actionId;
            this.inProcess = inProcess;
        }

        public int MaxSavedStatesCount()
        {
            return 0;
        }
    }

    public interface IDisposable
    {
        public void ScheduleDisposal();
        public bool CanDispose();
    }
    public class Data<T> : IBroadcastState where T : IDisposable
    {
        private string owner;
        public Dictionary<string, T> elements;

        public Data()
        {
            this.owner = "";
            this.elements = new();
        }

        public Data(string owner, Data<T> data, Func<T, string> getKey, params T[] tokensUpdate)
        {
            this.owner = owner;
            data.elements ??= new();
            elements = data.elements;

            if (tokensUpdate == null) return;

            foreach (var item in tokensUpdate)
            {
                string key = getKey(item);

                if (!item.CanDispose())
                {
                    if (elements.ContainsKey(key))
                    {
                        elements[key] = item;
                    }
                    else elements.Add(key, item);
                }
                else
                {
                    if (elements.ContainsKey(key))
                    {
                        elements.Remove(key);
                    }
                }
            }
        }

        public void Clear()
        {
            elements = new();
        }

        public int MaxSavedStatesCount()
        {
            return 1;
        }
    }
}
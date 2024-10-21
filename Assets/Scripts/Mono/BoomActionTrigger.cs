namespace Boom.Mono
{
    using Boom.Utility;
    using Candid.World.Models;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Events;

    public class BoomActionTrigger : MonoBehaviour
    {
        [System.Serializable]
        public class SerializableField
        {
            public string FieldName;
            public string FieldValue;

            public SerializableField(string fieldName, string fieldValue)
            {
                this.FieldName = fieldName;
                this.FieldValue = fieldValue;
            }

            public SerializableField()
            {
            }
        }

        [SerializeField] string actionName;

        [SerializeField] List<SerializableField> args;

        [field: SerializeField] public UnityEvent<ProcessedActionResponse> OnActionResponseSuccess = new();

        [field: SerializeField] public UnityEvent<ActionErrType.Base> OnActionResponseError = new();

        public string ActionName { get => actionName;}

        public void Trigger()
        {
            _Trigger();
        }
        private async void _Trigger()
        {
            //Map SerializableField List to Field List
            var _args = args.Map(e => new Field(e.FieldName, e.FieldValue)).ToList();

            //Process action
            var result = await ActionUtil.ProcessAction(ActionName, _args);


            //Handle events
            //If action was successful
            if (result.IsOk) OnActionResponseSuccess.Invoke(result.AsOk());
            //If action was not successful
            else OnActionResponseError.Invoke(result.AsErr());
        }
    }
}
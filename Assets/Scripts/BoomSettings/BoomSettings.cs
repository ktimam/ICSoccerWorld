namespace Boom
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "BoomSettings", menuName = "ScriptableObjects/BoomSettings", order = 1)]
    public class BoomSettings : ScriptableObject
    {
        public enum DeploymentEnv
        {
            Staging,
            Production
        }
        public enum WorldEnv
        {
            Staging,
            Production,
            None
        }
        [field: SerializeField] public DeploymentEnv DeploymentEnv_ { get; private set; } = DeploymentEnv.Staging;

        public string WorldId;
        [Header("Edit this only if your are manually setting this up with a WorldId")]
        public WorldEnv WorldEnv_ = WorldEnv.None;

        [SerializeField, Header("Production Only Settings")] bool proGuildsAsDevMode = true;

        public string WorldHubId
        {
            get
            {
                if (WorldEnv_ == WorldEnv.None) Debug.LogError("WorldSettings is not yet setup!");

                return WorldEnv_ ==
                    WorldEnv.Production ? Env.CanisterIds.WORLD_HUB.PRODUCTION : Env.CanisterIds.WORLD_HUB.STAGING;
            }
        }

        public string GamingGuildsId
        {
            get
            {
                if (WorldEnv_ == WorldEnv.None) Debug.LogError("WorldSettings is not yet setup!");

                return WorldEnv_ ==
                    WorldEnv.Production ? (proGuildsAsDevMode ? Env.CanisterIds.GAMING_GUILDS.DEVELOPMENT : Env.CanisterIds.GAMING_GUILDS.PRODUCTION) : Env.CanisterIds.GAMING_GUILDS.STAGING;
            }
        }

        #region EDITOR
#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= OnExitPlaymode;
            UnityEditor.EditorApplication.playModeStateChanged += OnExitPlaymode;
        }

        private void OnExitPlaymode(UnityEditor.PlayModeStateChange obj)
        {
            if (obj == UnityEditor.PlayModeStateChange.EnteredEditMode)
            {
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.AssetDatabase.SaveAssets();
                Debug.Log("BoomSettings has been saved!");
            }
        }
#endif
        #endregion
    }

}
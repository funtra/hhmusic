using System;
using UnityEngine;
using HHMusicLayer;

namespace Examples
{
    public class CustomSoundCoordinator : SoundCoordinator
    {

        #region Serailized Fields & Configuration

        [SerializeField]
        private SoundGroup powerupSoundGroup;

        [SerializeField]
        private SoundGroup firstSoundGroup;

        [SerializeField]
        private SoundGroup gameoverSoundGroup;

        [SerializeField]
        private WantedLevelSoundGroups[] wantedLevelSoundConfigurations;



        private int wantedLevel = 0;

        #endregion

        public override bool AutoPlay => true;
        public int CurrentWantedLevel => wantedLevel;

        protected override void Initialize()
        {
            base.Initialize();
            currentSoundGroup = firstSoundGroup;

            // Get your game play attributes and subscribe to events to have coordinator
            // respond to changes in game
            // {
            //     wantedLevel = GameplayAttributes.WantedLevelDifficulty.Value;
            //     GameplayAttributes.OnWantedLevelDifficultyChanged += TransitionToWantedLevel;
            // }
        }

        private void OnDestroy()
        {
            // if subscribed to something, here would be a good place to unsubscribe
            // {
            //     GameplayAttributes.OnWantedLevelDifficultyChanged -= TransitionToWantedLevel;
            // }
        }

        public void TransitionToWantedLevel(int nextWantedLevel)
        {
            nextWantedLevel = Mathf.Clamp(nextWantedLevel, 1, 5);
            if (nextWantedLevel == CurrentWantedLevel) return;

            int wantedIndex = nextWantedLevel - 1;
            var nextGroup = wantedLevelSoundConfigurations[wantedIndex].SoundGroup;
            wantedLevel = nextWantedLevel;
            TransitionToGroup(nextGroup);
        }

        public SoundGroup GetWantedLevelSoundGroup(int wantedLevel)
        {
            wantedLevel = Mathf.Clamp(wantedLevel, 1, 5);
            int wantedIndex = wantedLevel - 1;
            return wantedLevelSoundConfigurations[wantedIndex].SoundGroup;
        }

        protected override void OnPrecomputeSoundValues()
        {
            powerupSoundGroup = PrecomputeSoundGroup(powerupSoundGroup);
            firstSoundGroup = PrecomputeSoundGroup(firstSoundGroup);
            gameoverSoundGroup = PrecomputeSoundGroup(gameoverSoundGroup);

            for (var i = 0; i < wantedLevelSoundConfigurations.Length; i++)
            {
                var wantedLevelSoundGroups = wantedLevelSoundConfigurations[i];
                var computedSG = PrecomputeSoundGroup(wantedLevelSoundGroups.SoundGroup);
                wantedLevelSoundGroups.SoundGroup = computedSG;
                wantedLevelSoundConfigurations[i] = wantedLevelSoundGroups;
            }
        }


        [Serializable]
        public struct WantedLevelSoundGroups
        {
            public int WantedLevel; // this is so in the future we don't have to rely on array index position but by explicit definition. tbd.
            public SoundGroup SoundGroup;
        }

    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FullscreenEditor {
    /// <summary>Toggle fullscreen upon playmode change if <see cref="FullscreenPreferences.FullscreenOnPlayEnabled"/> is set to true.</summary>
    [InitializeOnLoad]
    internal static class FullscreenOnPlay {

        static FullscreenOnPlay() {

#if UNITY_2017_2_OR_NEWER
            EditorApplication.playModeStateChanged += state => {
                switch(state) {
                    case PlayModeStateChange.ExitingEditMode:
                        SetIsPlaying(true);
                        break;

                    case PlayModeStateChange.ExitingPlayMode:
                        SetIsPlaying(false);
                        break;

                    case PlayModeStateChange.EnteredPlayMode:
                        foreach(var fs in Fullscreen.GetAllFullscreen())
                            if(fs && fs is FullscreenWindow && (fs as FullscreenWindow).CreatedByFullscreenOnPlay) {
                                FixGameViewMouseInput.UpdateGameViewArea(fs);
                            }
                        break;
                }
            };

            EditorApplication.pauseStateChanged += state => SetIsPlaying(EditorApplication.isPlayingOrWillChangePlaymode && state == PauseState.Unpaused);
#else
            EditorApplication.playmodeStateChanged += () => SetIsPlaying(EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPaused);
#endif

        }

        private static void SetIsPlaying(bool playing) {

            var fullscreens = Fullscreen.GetAllFullscreen()
                .Select(fullscreen => fullscreen as FullscreenWindow)
                .Where(fullscreen => fullscreen);

            // We close all the game views created on play, even if the option was disabled in the middle of the play mode
            // This is done to best reproduce the default behaviour of the maximize on play
            if(!playing) {
                foreach(var fs in fullscreens)
                    if(fs && fs.CreatedByFullscreenOnPlay) // fs might have been destroyed
                        fs.Close();
                return;
            }

            if(!FullscreenPreferences.FullscreenOnPlayEnabled)
                return; // Nothing to do here

            if(FullscreenUtility
                .GetGameViews()
                .Any(gv => {
                    if(!gv) return false;

                    return PlaymodeBehaviourImplemented(gv) && GetEnterPlayModeBehavior(gv) == CustomEnterPlayModeBehavior.PlayFullscreen;
                })) {
                EditorUtility.DisplayDialog("Fullscreen on play conflict", "Seems like you have both Unity's built-in fullscreen on play and Fullscreen Editor's plugin enabled. Please, make sure you only have one of them enabled to prevent conflicts.", "Got it!");
                FullscreenPreferences.FullscreenOnPlayEnabled.Value = false;
                return;
            }

            var gameView = FullscreenOnPlayGameView();

            if(!gameView) // no gameview has the fullscreen on play option enabled
                return;

            foreach(var fs in fullscreens)
                if(fs && fs.Rect.Overlaps(gameView.position)) // fs might have been destroyed
                    return; // We have an open fullscreen where the new one would be, so let it there

            if(gameView && Fullscreen.GetFullscreenFromView(gameView))
                return; // The gameview is already in fullscreen

            var gvfs = Fullscreen.MakeFullscreen(Types.GameView, gameView);
            gvfs.CreatedByFullscreenOnPlay = true;
        }

        internal static EditorWindow FullscreenOnPlayGameView() {
            var mainGv = FullscreenUtility.GetMainGameView();

            if(mainGv) return mainGv;

            return FullscreenUtility
                .GetGameViews()
                .FirstOrDefault(gv => gv);
        }

        internal enum CustomEnterPlayModeBehavior {
            PlayFocused,
            PlayMaximized,
            PlayUnfocused,
            PlayFullscreen
        }

        internal static bool PlaymodeBehaviourImplemented(EditorWindow playmodeView) {
            return playmodeView.HasProperty("enterPlayModeBehavior");
        }

        internal static CustomEnterPlayModeBehavior GetEnterPlayModeBehavior(EditorWindow playmodeView) {
            return (CustomEnterPlayModeBehavior)playmodeView.GetPropertyValue<int>("enterPlayModeBehavior");
        }

        internal static void SetEnterPlayModeBehavior(EditorWindow playmodeView, CustomEnterPlayModeBehavior behaviour) {
            if(playmodeView.HasProperty("playModeBehaviorIdx"))
                playmodeView.SetPropertyValue("playModeBehaviorIdx", (int)behaviour); // unity forgot to update this prop when updating the play mode behaviour
            playmodeView.SetPropertyValue("enterPlayModeBehavior", (int)behaviour);
        }

    }
}

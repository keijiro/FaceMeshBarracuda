using UnityEditor;
using UnityEngine;
using FullscreenEditor;

[ExecuteInEditMode]
public class AutoRunner : MonoBehaviour
{
    EditorWindow gameView;

    private void Awake()
    {
        instance = this;
        arguments.ReadParameter();

#if UNITY_EDITOR
        if (arguments.autoRun && (!EditorApplication.isPlaying))
        {
            EditorApplication.isPlaying = true;
       
        }
#endif
    }

    private void Update()
    {
#if UNITY_EDITOR

        //Fullscreen設定
        if (arguments.autoRun && gameView == null)
        {

            //Find the gameView window
            var gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
            //get instance
            gameView = EditorWindow.GetWindow(gameViewType);

            //Set View Aspect Ratio
            gameViewType.GetMethod("SizeSelectionCallback")!.Invoke(gameView, new object[] { 0, null });

            // Make it fullscreen
            var fullscreen = Fullscreen.MakeFullscreen<EditorWindow>(gameView);
        }
#endif

    }

    public AutoRunParams arguments
    { get; private set; } = new AutoRunParams();
    public static AutoRunner instance
    { get; private set; } = null;
}
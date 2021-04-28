using System.Collections;
using FistVR;
using UnityEngine.SceneManagement;

namespace WurstModRefreshed
{
    public class MainMenuScenePatcher
    {
        public IEnumerator Run(Scene scene)
        {
            yield break;
        }
    }

    public class MainMenuCustomScenePointable : MainMenuScenePointable
    {
        public override void OnPoint(FVRViveHand hand)
        {
            // If we're loading a custom scene start our own code first
            if (hand.Input.TriggerDown && Def is MainMenuCustomSceneDef {CustomScene: { }} cDef)
                StartCoroutine(LoadSceneAsync(cDef.CustomScene));

            // Then let the game's code continue
            base.OnPoint(hand);
        }

        private IEnumerator LoadSceneAsync(CustomScene scene)
        {
            // First disable the screen stuff so the user can't do anything until the scene is loaded
            Screen.LoadSceneButton.SetActive(false);
            Screen.Label_Title.text = scene.Meta.Name + " - Loading...";
            Screen.Label_Description.text = scene.Meta.Description;
            Screen.Label_Type.text = scene.Meta.Author;

            // The image should already be loaded but just in case...
            if (scene.ThumbnailSprite is null) yield return scene.LoadThumbnailAsync();
            Screen.Image_Preview.sprite = scene.ThumbnailSprite;
            
            // If the asset bundle isn't loaded yet, do that now.
            if (scene.AssetBundle is null) yield return scene.LoadAssetBundleAsync();
            
            // Re-enable the button and fill in the info.
            Screen.LoadSceneButton.SetActive(true);
            Screen.Label_Title.text = scene.Meta.Name;
            
        }
    }

    public class MainMenuCustomSceneDef : MainMenuSceneDef
    {
        public CustomScene? CustomScene;
    }
}
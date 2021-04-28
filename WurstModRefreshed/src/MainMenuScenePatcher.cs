using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FistVR;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace WurstModRefreshed
{
    public class MainMenuScenePatcher
    {
        private readonly Vector3[] _screenPositions;
        private readonly List<CustomScene> _scenes;

        // There's some stuff we only need to do once, like calculating the screen positions
        public MainMenuScenePatcher(List<CustomScene> scenes)
        {
            _scenes = scenes;
            _screenPositions = EnumerateScreenPositions()
                .OrderByDescending(x => -x.z)
                .ThenBy(x => Mathf.Abs(x.y - 4.15f))
                .ToArray();
        }

        private static IEnumerable<Vector3> EnumerateScreenPositions()
        {
            // Define functions for a circle
            static float CircleX(float x) => 14.13f * Mathf.Cos(Mathf.Deg2Rad * x) - 0.39f;
            static float CircleZ(float z) => 14.13f * Mathf.Sin(Mathf.Deg2Rad * z) - 2.98f;

            // Loop over the entire circle
            for (int r = 0; r <= 360; r += 13)
            {
                // Make it 4 columns tall
                for (int h = 0; h < 4; h++)
                {
                    float x = CircleX(r), z = CircleZ(r);
                    if (z < -7f) yield return new Vector3(x, h * 2 + .5f, z);
                }
            }
        }

        public IEnumerator Run(Scene scene)
        {
            // First we want to find some objects we'll need to copy
            GameObject sceneScreenBase = GameObject.Find("SceneScreen_GDC");
            GameObject labelBase = GameObject.Find("Label_Description_1_Title (5)");
            
            // "Modded Scenes" header above scenes
            GameObject moddedScenesLabel = Object.Instantiate(labelBase, labelBase.transform.parent);
            moddedScenesLabel.transform.position = new Vector3(0f, 8.3f, -17.1f);
            moddedScenesLabel.transform.localEulerAngles = new Vector3(-180f, 0f, 180f);
            moddedScenesLabel.GetComponent<Text>().text = "Modded Scenes";
            
            // Create the screens we need
            CustomScene[] sandboxLevels = _scenes.Where(x => x.Meta.Gamemode != "take_and_hold").ToArray();
            for (int i = 0; i < sandboxLevels.Length; i++)
            {
                // The two references
                CustomScene customScene = _scenes[i];
                Vector3 position = _screenPositions[i];
                
                // Copy and set the position
                GameObject screen = Object.Instantiate(sceneScreenBase, sceneScreenBase.transform.parent);
                screen.transform.position = position;
                screen.transform.localEulerAngles = new Vector3(0, 180 - Mathf.Rad2Deg * Mathf.Atan(-position.x / position.z), 0);
                Vector3 localScale = screen.transform.localScale * .5f;
                screen.transform.localScale = localScale;
                
                // Replace the scene pointable with our custom one
                MainMenuScenePointable scenePointable = screen.GetComponent<MainMenuScenePointable>();
                MainMenuCustomScenePointable customScenePointable = screen.AddComponent<MainMenuCustomScenePointable>();
                customScenePointable.MaxPointingRange = scenePointable.MaxPointingRange;
                customScenePointable.Screen = scenePointable.Screen;
                
                // Make sure our thumbnail is loaded
                if (customScene.Thumbnail is null) yield return customScene.LoadThumbnailAsync();
                
                // Make our custom scene def and apply it
                MainMenuCustomSceneDef def = ScriptableObject.CreateInstance<MainMenuCustomSceneDef>();
                def.CustomScene = customScene;
                def.Desciption = customScene.Meta.Description;
                def.Image = customScene.ThumbnailSprite;
                def.Name = customScene.Meta.Name;
                def.Type = customScene.Meta.Author;
                def.SceneName = customScene.Meta.SceneName;
                customScenePointable.Def = def;
                
                // Now destroy the original
                Object.Destroy(scenePointable);
            }
        }
    }
}
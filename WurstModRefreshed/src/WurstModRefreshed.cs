using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Deli;
using Deli.Runtime;
using Deli.Setup;
using Deli.VFS;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WurstModRefreshed
{
	public class WurstModRefreshed : DeliBehaviour
	{
		private readonly Dictionary<string, List<Func<Scene, IEnumerator>>> _scenePatchers = new();
		private readonly List<CustomScene> _customScenes = new();
		private readonly Harmony _harmony;
		
		public WurstModRefreshed()
		{
			// Execute any patches
			_harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), Source.Info.Guid);
			
			// Register our scene patchers
			SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;
			_scenePatchers["MainMenu3"].Add(new MainMenuScenePatcher(_customScenes).Run);
			
			// Register our stages
			Stages.Setup += OnSetup;
			Stages.Runtime += OnRuntime;
		}

		private void OnSetup(SetupStage stage)
		{
			CustomScene.SceneMetaReader = stage.RegisterJson<CustomSceneMeta>();
			stage.SetupAssetLoaders[Source, "level"] += LevelLoader;
		}

		private void OnRuntime(RuntimeStage stage)
		{
			CustomScene.AssetBundleReader = stage.GetReader<AssetBundle>();
			CustomScene.Texture2DReader = stage.GetReader<Texture2D>();
		}

		private void LevelLoader(SetupStage stage, Mod mod, IHandle handle)
		{
			// Make sure we're given a directory and then add it to the list
			if (handle is not IDirectoryHandle dir)
				throw new NotSupportedException(
					$"Level handles must be to a directory! {handle.Path} is not a directory!");
			_customScenes.Add(new CustomScene(dir));
		}
		
		// When a scene is loaded, let our scene patchers run
		private void SceneManagerOnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			// Run any scene patchers
			if (_scenePatchers.ContainsKey(scene.name))
			{
				foreach (var patcher in _scenePatchers[scene.name])
					StartCoroutine(patcher(scene));
			}
		}
		
		// Just in case.
		public void OnDisable() => _harmony.UnpatchSelf();
	}
}
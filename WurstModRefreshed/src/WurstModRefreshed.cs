using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Deli.Runtime;
using Deli.Setup;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WurstModRefreshed
{
	public class WurstModRefreshed : DeliBehaviour
	{
		private readonly Dictionary<string, List<Func<Scene, IEnumerator>>> _scenePatchers = new();
		private readonly Harmony _harmony;
		
		public WurstModRefreshed()
		{
			// Execute any patches
			_harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), Source.Info.Guid);
			
			// Register our scene patchers
			SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;
			_scenePatchers["MainMenu3"].Add(new MainMenuScenePatcher().Run);
			
			Stages.Setup += OnSetup;
			Stages.Runtime += OnRuntime;
		}

		private void OnSetup(SetupStage stage)
		{
			CustomScene.SceneMetaReader = stage.RegisterJson<CustomSceneMeta>();
		}

		private void OnRuntime(RuntimeStage stage)
		{
			CustomScene.AssetBundleReader = stage.GetReader<AssetBundle>();
			CustomScene.Texture2DReader = stage.GetReader<Texture2D>();
		}

		public void OnDisable()
		{
			_harmony.UnpatchSelf();
		}

		private void SceneManagerOnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			// Run any scene patchers
			if (_scenePatchers.ContainsKey(scene.name))
			{
				foreach (var patcher in _scenePatchers[scene.name])
					StartCoroutine(patcher(scene));
			}
		}
	}
}
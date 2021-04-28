using System;
using System.Collections;
using System.IO;
using Deli.Immediate;
using Deli.Runtime;
using Deli.VFS;
using UnityEngine;

namespace WurstModRefreshed
{
    public class CustomScene
    {
        // Deli VFS stuff
        internal static DelayedReader<AssetBundle>? AssetBundleReader { get; set; }
        internal static DelayedReader<Texture2D>? Texture2DReader { get; set; }
        internal static ImmediateReader<CustomSceneMeta>? SceneMetaReader { get; set; }
        private readonly IFileHandle _assetBundleHandle;
        private readonly IFileHandle _thumbnailHandle;
        
        // References the rest of the code wants
        public AssetBundle? AssetBundle { get; private set; }
        public Texture2D? Thumbnail { get; private set; }
        public Sprite? ThumbnailSprite { get; private set; }
        public CustomSceneMeta Meta { get; }

        public CustomScene(IDirectoryHandle directory)
        {
            _assetBundleHandle = directory.GetFile("leveldata") ?? throw new FileNotFoundException("Leveldata file was missing!");
            _thumbnailHandle = directory.GetFile("thumb.png") ?? throw new FileNotFoundException("Thumbnail file was missing!");
            
            IFileHandle sceneMetaHandle = directory.GetFile("info.json") ?? throw new FileNotFoundException("Scene meta file was missing!");
            Meta = SceneMetaReader?.Invoke(sceneMetaHandle) ?? throw new InvalidOperationException("Scene meta reader was null??");
        }

        public IEnumerator LoadAssetBundleAsync()
        {
            if (AssetBundle is not null)
            {
                throw new InvalidOperationException("Asset bundle is already loaded!");
            }
            
            if (AssetBundleReader is null)
                throw new InvalidOperationException(
                    "Somehow we're trying to load a level before WurstMod had it's runtime stage!");
            
            // Use the reader to load the file.
            var op = AssetBundleReader(_assetBundleHandle);
            yield return op;
            AssetBundle = op.Result;
        }

        public IEnumerator LoadThumbnailAsync()
        {
            if (Thumbnail is not null)
                throw new InvalidOperationException("Thumbnail is already loaded!");

            if (Texture2DReader is null)
                throw new InvalidOperationException("Texture2D reader is null??");

            var op = Texture2DReader(_thumbnailHandle);
            yield return op;
            Thumbnail = op.Result;
            ThumbnailSprite = Thumbnail.ToSprite();
        }
    }
    
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CustomSceneMeta
    {
#pragma warning disable 8618
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Gamemode { get; set; }
        public string SceneName { get; set; }
        
        // ReSharper restore UnusedAutoPropertyAccessor.Global
#pragma warning restore 8618
    }
}
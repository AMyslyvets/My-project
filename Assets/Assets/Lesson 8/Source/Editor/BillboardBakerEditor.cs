using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Lesson8.Editor
{
    [CustomEditor(typeof(BillboardBaker))]
    public class BillboardBakerEditor : UnityEditor.Editor
    {
        private struct ShaderData
        {
            public Renderer renderer;
            public Shader[] shaders;
            public Material[] materials;
        }
        
        private Texture2D[] _textures;
        private Texture2D _atlas;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Bake Color"))
            {
                BakeColor();
            }
            if (GUILayout.Button("Bake Normal"))
            {
                BakeNormal();
            }
            if (_textures == null)
                return;
            /*for (int i = 0; i < _textures.Length; ++i)
            {
                DrawTexture(_textures[i]);
            }*/
            if (_atlas != null)
            {
                DrawTexture(_atlas);
                if (GUILayout.Button("Save Atlas:"))
                {
                    Save();
                }
            }
        }

        private void BakeColor()
        {
            if (target is not BillboardBaker billboardBaker)
                return;
            List<ShaderData> shaderCache = CacheShader(billboardBaker, billboardBaker.ColorShader);
            Bake();
            RestoreShader(shaderCache);
        }

        private void BakeNormal()
        {
            if (target is not BillboardBaker billboardBaker)
                return;
            List<ShaderData> shaderCache = CacheShader(billboardBaker, billboardBaker.NormalShader);
            Bake();
            RestoreShader(shaderCache);
        }

        private List<ShaderData> CacheShader(BillboardBaker billboardBaker, Shader shader)
        {
            Renderer[] renderers = billboardBaker.GetComponentsInChildren<Renderer>();
            
            List<ShaderData> shaderCache = new List<ShaderData>(renderers.Length * 2);
            
            for (int i = 0; i < renderers.Length; ++i)
            {
                ShaderData shaderData = new ShaderData();
                Renderer renderer = renderers[i];
                shaderData.renderer = renderer;
                shaderData.materials = renderer.sharedMaterials;
                shaderData.shaders = new Shader[shaderData.materials.Length];
                for (int j = 0; j < shaderData.materials.Length; ++j)
                {
                    shaderData.shaders[j] = shaderData.materials[j].shader;
                    shaderData.materials[j].shader = shader;
                }
                shaderCache.Add(shaderData);
            }
            
            return shaderCache;
        }

        private void RestoreShader(List<ShaderData> shaderCache)
        {
            for (int i = 0; i < shaderCache.Count; ++i)
            {
                ShaderData shaderData = shaderCache[i];
                for (int j = 0; j < shaderData.materials.Length; ++j)
                {
                    shaderData.materials[j].shader = shaderData.shaders[j];
                }
            }
        }
        
        private void DrawTexture(Texture2D texture)
        {
            Rect controlRect =
                EditorGUILayout.GetControlRect(false, texture.height, GUILayout.Width(texture.width));
            GUI.DrawTexture(controlRect,
                texture, ScaleMode.StretchToFill, true);
        }

        private void Save()
        {
            string path = EditorUtility.SaveFilePanel("Save Atlas", Application.dataPath, "Atlas", "png");
            File.WriteAllBytes(path, _atlas.EncodeToPNG());
            AssetDatabase.Refresh();
        }
        
        private void Bake()
        {
            if (_textures != null)
            {
                for (int i = 0; i < _textures.Length; ++i)
                {
                    DestroyImmediate(_textures[i]);
                }
            }
            
            BillboardBaker baker = target as BillboardBaker;
            
            if (baker == null)
                return;
            
            _textures = new Texture2D[baker.Angles.Length];
            
            int width = baker.RenderTexture.width;
            int height = baker.RenderTexture.height;
            
            for (int i = 0; i < baker.Angles.Length; ++i)
            {
                float angle = baker.Angles[i];
                baker.CameraRig.rotation = Quaternion.Euler(0, angle, 0);
                baker.Camera.Render();
                RenderTexture.active = baker.RenderTexture;
                Texture2D texture = new Texture2D(width, height);
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                _textures[i] = texture;
            }
            
            int size = Mathf.CeilToInt(Mathf.Sqrt(_textures.Length));

            if (_atlas != null)
            {
                DestroyImmediate(_atlas);
            }
            
            _atlas = new Texture2D(width * size, height * size);
            for (int i = 0; i < _textures.Length; ++i)
            {
                Texture2D texture = _textures[i];
                int x = i % size;
                int y = i / size;
                _atlas.SetPixels(x * width, y * height, width, height, texture.GetPixels());
            }
            _atlas.Apply();
        }

        private void OnDisable()
        {
            if (_textures != null)
            {
                for (int i = 0; i < _textures.Length; ++i)
                {
                    DestroyImmediate(_textures[i]);
                }
            }
            if (_atlas != null)
                DestroyImmediate(_atlas);
        }
    }
}
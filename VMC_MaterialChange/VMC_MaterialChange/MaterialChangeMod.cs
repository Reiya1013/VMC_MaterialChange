using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

using VMCMod;

[VMCPlugin(
    Name: "MaterialChange",
    Version: "0.0.1",
    Author: "Reiya",
    Description: "アバターのMaterialを変更することで、MToon以外のshaderを使えるようにします",
    AuthorURL: "https://twitter.com/Reiya__",
    PluginURL: null)]
public class MaterialChangeMod : MonoBehaviour
{
    //Shader Asset
    UnityEngine.Object[] Assets;
    private Dictionary<string, Shader> Shaders = new Dictionary<string, Shader>();

    //外部Material
    UnityEngine.Object[] AssetsMaterial;
    private Dictionary<string, Material> otherMaterials = new Dictionary<string, Material>();
    private List<string> MaterialsName = new List<string>();

    //モデル情報の保持
    private Dictionary<Renderer, Material[]> DefaultMaterials = new Dictionary<Renderer, Material[]>();
    private GameObject Model;

    private void Awake()
    {
        VMCEvents.OnModelLoaded += OnModelLoaded;
        //VMCEvents.OnCameraChanged += OnCameraChanged;
    }

    void Start()
    {
        Debug.Log("MaterialChange Mod started.");

        ShaderLoad();
    }

    void Update()
    {

    }

    [OnSetting]
    public void OnSetting()
    {
        var filename = WindowsDialogs.OpenFileDialog("Select MaterialChange File", ".mc");
        if (filename != null)
        {
            OtherMaterialLoad(filename);
        }
    }


    /// <summary>
    /// AssetBundleからBS_Saderを読み込む
    /// </summary>
    private void ShaderLoad()
    {
        //assetシェーダーを読み込む
        AssetBundle assetBundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("VMCAvatarMaterialChange.bs_shaders"));
        Assets = assetBundle.LoadAllAssets();
        foreach (var asset in Assets)
        {
            if (asset is Shader shader)
            {
                Shaders[shader.name] = shader;
            }
        }
    }

    /// <summary>
    /// ModelLoadedで標準Material情報を取得しておく
    /// </summary>
    /// <param name="currentModel"></param>
    private void OnModelLoaded(GameObject currentModel)
    {
        if (currentModel == null) return;
        Model = currentModel;
        foreach (Renderer renderer in Model.GetComponentsInChildren<Renderer>())
        {
            DefaultMaterials.Add(renderer, renderer.sharedMaterials);
        }
    }

    private void OnCameraChanged(Camera currentCamera)
    {
        //カメラ切り替え時に現在のカメラを取得できます
    }


    /// <summary>
    /// マテリアルチェンジを行う(.mcファイル読み込み後変更処理実行)
    /// </summary>
    public void OtherMaterialLoad(string Address)
    {

        AssetBundle assetBundle = AssetBundle.LoadFromFile(Address);
        AssetsMaterial = assetBundle.LoadAllAssets();
        assetBundle.Unload(false);
        foreach (var asset in AssetsMaterial)
        {
            if (asset is Material material)
            {
                otherMaterials[material.name] = material;
            }
        }

        SetOtherMaterial();
    }

    /// <summary>
    /// マテリアルチェンジ実行(.mcファイルパッチ処理)
    /// </summary>
    private void SetOtherMaterial()
    {
        if (otherMaterials == null) return;
        if (Model == null) return;
        foreach (Renderer renderer in Model.GetComponentsInChildren<Renderer>(true))
        {
            Material[] newMaterials = new Material[renderer.sharedMaterials.Length];
            for (int index = 0; index < renderer.sharedMaterials.Length; index++)
            {
                if (otherMaterials.ContainsKey(renderer.sharedMaterials[index].name))
                {
                    newMaterials[index] = otherMaterials[renderer.sharedMaterials[index].name];
                    //同名shader(BeatSaber用シェーダーが来た場合、α値有りのシェーダーに置き換えなおす)
                    if (Shaders.ContainsKey(newMaterials[index].shader.name))
                        newMaterials[index].shader = Shaders[newMaterials[index].shader.name];

                }
                else
                    newMaterials[index] = renderer.sharedMaterials[index];
            }
            renderer.sharedMaterials = newMaterials;
        }

    }
}
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using VMCMod;

using Application = UnityEngine.Application;
using System;

namespace VMC_MaterialChange
{

    [VMCPlugin(
    Name: "MaterialChange",
    Version: "0.0.4",
    Author: "Reiya",
    Description: "アバターのMaterialを変更することで、MToon以外のshaderを使えるようにします",
    AuthorURL: "https://twitter.com/Reiya__",
    PluginURL: "https://github.com/Reiya1013/VMC_MaterialChange")]
    public class MaterialChangeMod : MonoBehaviour
    {
        //Shader Asset
        UnityEngine.Object[] Assets;
        private Dictionary<string, Shader> Shaders = new Dictionary<string, Shader>();

        //外部Material
        UnityEngine.Object[] AssetsMaterial;
        private Dictionary<string, Material> otherMaterials = new Dictionary<string, Material>();
        private Dictionary<string, Material> otherMaterials2 = new Dictionary<string, Material>();
        private Dictionary<string, Material> otherMaterials3 = new Dictionary<string, Material>();
        private List<string> MaterialsName = new List<string>();

        //モデル情報の保持
        private Dictionary<Renderer, Material[]> DefaultMaterials = new Dictionary<Renderer, Material[]>();
        private GameObject Model;
        private string VRMMetaKey;
        private void Awake()
        {
            VMCEvents.OnModelLoaded += OnModelLoaded;
            //VMCEvents.OnCameraChanged += OnCameraChanged;
        }

        void Start()
        {
            Debug.Log("MaterialChange Mod started.");

            KeyboardAction.KeyDownEvent += KeyboardAction_KeyDown;
            KeyboardAction.KeyUpEvent += KeyboardAction_KeyUp;
        
            ShaderLoad();
            OtherMaterialChangeSetting.Instance.LoadConfiguration();
        }


        bool isShift = false;
        bool isKey = false;
        private void KeyboardAction_KeyDown(object sender, KeyboardEventArgs e)
        {
            //throw new Exception($"KeyDown{DateTime.Now} {e.KeyCode} {(int)Keys.RShiftKey} {(int)KeyCode.Alpha1}");
        
            if (e.KeyCode == (int)Keys.RShiftKey || e.KeyCode == (int)Keys.LShiftKey)
                isShift = true;
            if (e.KeyCode == (int)Keys.D1 ||
                e.KeyCode == (int)Keys.D2 ||
                e.KeyCode == (int)Keys.D3 )
                isKey = true;

            if (isShift && isKey)
            {
                OldInput = true;
                if (e.KeyCode == (int)Keys.D1) SetOtherMaterial(otherMaterials);
                else if (e.KeyCode == (int)Keys.D2) SetOtherMaterial(otherMaterials2);
                else if (e.KeyCode == (int)Keys.D3) SetOtherMaterial(otherMaterials3);
            }
        }

        private void KeyboardAction_KeyUp(object sender, KeyboardEventArgs e)
        {
            if (e.KeyCode == (int)Keys.RShiftKey || e.KeyCode == (int)Keys.LShiftKey)
                isShift = false;

            if (e.KeyCode == (int)Keys.D1 ||
                e.KeyCode == (int)Keys.D2 ||
                e.KeyCode == (int)Keys.D3)
                isKey = false;
        }




        bool OldInput = false;
        void Update()
        {
        }
        private void OnEnable()
        {
            Application.logMessageReceived += OnLogMessageReceived;
        }

        private void OnDisable()
        {
            KeyboardAction.KeyDownEvent -= KeyboardAction_KeyDown;
            KeyboardAction.KeyUpEvent -= KeyboardAction_KeyUp;
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
        {
        }

        [OnSetting]
        public void OnSetting()
        {
            otherMaterials.Clear();
            otherMaterials2.Clear();
            otherMaterials3.Clear();

            string filename1 = "";
            string filename2 = "";
            string filename3 = "";

            if (!FileLoad(otherMaterials, ref filename1, "(1st ChangeFile)")) return;
            if (FileLoad(otherMaterials2, ref filename2, "(2nd ChangeFile)"))
                FileLoad(otherMaterials3, ref filename3, "(3rd ChangeFile)");

            SetOtherMaterial(otherMaterials);

            OtherSettingSet(filename1, filename2, filename3);

        }

        /// <summary>
        /// パラメータ保存
        /// </summary>
        /// <param name="selectRow"></param>
        /// <param name="selectRow2"></param>
        /// <param name="selectRow3"></param>
        public void OtherSettingSet(string filename1, string filename2, string filename3)
        {
            //パラメータセット
            if (OtherMaterialChangeSetting.Instance.OtherParameter.List.ContainsKey(VRMMetaKey))
            {
                OtherMaterialChangeSetting.Instance.OtherParameter.List[VRMMetaKey].FileAddress1 = filename1;
                OtherMaterialChangeSetting.Instance.OtherParameter.List[VRMMetaKey].FileAddress2 = filename2;
                OtherMaterialChangeSetting.Instance.OtherParameter.List[VRMMetaKey].FileAddress3 = filename3;
            }
            else
            {
                //新規だった場合は追記
                SettingOtherList otherList = new SettingOtherList();
                otherList.Meta = VRMMetaKey;
                otherList.FileAddress1 = filename1;
                otherList.FileAddress2 = filename2;
                otherList.FileAddress3 = filename3;
                OtherMaterialChangeSetting.Instance.OtherParameter.List[VRMMetaKey] = otherList;
            }
            //JSON Save
            OtherMaterialChangeSetting.Instance.SaveConfiguration();
        }

        /// <summary>
        /// .mcファイル読み込み
        /// </summary>
        /// <param name="set"></param>
        /// <param name="AddMessage"></param>
        /// <returns></returns>
        private bool FileLoad(Dictionary<string, Material> set,ref string filename,string AddMessage="")
        {
            filename = WindowsDialogs.OpenFileDialog($"Select MaterialChange File{AddMessage}", ".mc");
            if (filename != null)
            {
                OtherMaterialLoad(filename, set);
                return true;
            }
            else
                filename = "";
                return false;
        }


        /// <summary>
        /// AssetBundleからBS_Saderを読み込む
        /// </summary>
        private void ShaderLoad()
        {
            //assetシェーダーを読み込む
            AssetBundle assetBundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("VMC_MaterialChange.bs_shaders"));
            Assets = assetBundle.LoadAllAssets();
            assetBundle.Unload(false);
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

            //自動MaterialChangeONの場合ファイル読み込みを行って設定する
            var meta = Model.GetComponent<VRM.VRMMeta>();
            VRMMetaKey = $"{meta.Meta.Title}_{meta.Meta.Version}_{meta.Meta.Author}";
            if (OtherMaterialChangeSetting.Instance.OtherParameter.AutoMaterialChange & VRMMetaKey != "")
            {
                if (OtherMaterialChangeSetting.Instance.OtherParameter.List != null)
                    if (OtherMaterialChangeSetting.Instance.OtherParameter.List.ContainsKey(VRMMetaKey))
                    {
                        OtherMaterialLoad(OtherMaterialChangeSetting.Instance.OtherParameter.List[VRMMetaKey].FileAddress1, otherMaterials);
                        OtherMaterialLoad(OtherMaterialChangeSetting.Instance.OtherParameter.List[VRMMetaKey].FileAddress2, otherMaterials2);
                        OtherMaterialLoad(OtherMaterialChangeSetting.Instance.OtherParameter.List[VRMMetaKey].FileAddress3, otherMaterials3);
                        if (otherMaterials.Count != 0)
                            SetOtherMaterial(otherMaterials);
                    }
            }

        }

        private void OnCameraChanged(Camera currentCamera)
        {
            //カメラ切り替え時に現在のカメラを取得できます
        }


        /// <summary>
        /// マテリアルチェンジを行う(.mcファイル読み込み後変更処理実行)
        /// </summary>
        public void OtherMaterialLoad(string filename, Dictionary<string, Material> set)
        {
            set.Clear();
            if (filename == "") return;
            AssetBundle assetBundle = AssetBundle.LoadFromFile(filename);
            AssetsMaterial = assetBundle.LoadAllAssets();
            assetBundle.Unload(false);
            foreach (var asset in AssetsMaterial)
            {
                if (asset is Material material)
                {
                    set[material.name] = material;
                }
            }
        }

        /// <summary>
        /// マテリアルチェンジ実行(.mcファイルパッチ処理)
        /// </summary>
        private void SetOtherMaterial(Dictionary<string, Material> set)
        {
            if (set == null) return;
            if (Model == null) return;
            foreach (Renderer renderer in Model.GetComponentsInChildren<Renderer>(true))
            {
                Material[] newMaterials = new Material[renderer.sharedMaterials.Length];
                for (int index = 0; index < renderer.sharedMaterials.Length; index++)
                {
                    if (set.ContainsKey(renderer.sharedMaterials[index].name))
                    {
                        newMaterials[index] = set[renderer.sharedMaterials[index].name];
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
}


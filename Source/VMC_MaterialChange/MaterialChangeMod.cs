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
    Version: "0.0.7",
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
        //設定アニメーション
        RuntimeAnimatorController ReturnAnimatorController = null; //ロード取得用
        private RuntimeAnimatorController otherAnimation1;
        private RuntimeAnimatorController otherAnimation2;
        private RuntimeAnimatorController otherAnimation3;

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

            SteamVR2Input.Instance.KeyDownEvent += ControllerAction_KeyDown;
            SteamVR2Input.Instance.KeyUpEvent += ControllerAction_KeyUp;

        }


        bool IsLeftTrigerClick = false;
        bool IsRightTigerClick = false;
        bool oldIsRightTigerClick = false;


        Int32 RightTriggerDownCount;
        float RightTriggerDownTime;

        private void ControllerAction_KeyUp(object sender, OVRKeyEventArgs e)
        {
            if (e.IsTouch) return;

            if (e.IsLeft && e.Name.Contains("Trigger"))
                IsLeftTrigerClick = false;
            else if (!e.IsLeft && e.Name.Contains("Trigger"))
                IsRightTigerClick = false;


            //左手トリガー握りっぱなしで右トリガー3連続で入力されたらチェンジアニメーション
            if (!IsLeftTrigerClick)
            {
                RightTriggerDownCount = 0;
                RightTriggerDownTime = 0;
            }


            oldIsRightTigerClick = IsRightTigerClick;
        }

        private void ControllerAction_KeyDown(object sender, OVRKeyEventArgs e)
        {
            if (e.IsTouch) return;
            if (e.Name.Contains("Touch")) return;

            Debug.Log($" e.Name { e.Name}");
            if (e.IsLeft && e.Name.Contains("Trigger"))
                IsLeftTrigerClick = true;
            else if (!e.IsLeft && e.Name.Contains("Trigger"))
                IsRightTigerClick = true;

            //左手トリガー握りっぱなしで右トリガー3連続で入力されたらチェンジアニメーション
            if (!IsLeftTrigerClick)
            {
                RightTriggerDownCount = 0;
                RightTriggerDownTime = 0;
            }

            //トリガークリックされたときに経過時間到達していた場合キャンセル
            if (RightTriggerDownTime > 0.5f)
            {
                RightTriggerDownCount = 0;
                RightTriggerDownTime = 0;
            }


            Debug.Log($"ControllerAction {IsLeftTrigerClick} {IsRightTigerClick} {RightTriggerDownTime}");
            if (IsLeftTrigerClick && IsRightTigerClick)
            {
                RightTriggerDownCount += 1;
                RightTriggerDownTime = 0;   //最後に入力があってから１秒経過しても３回目入力しなかった場合のみクリアするようにする

                if (RightTriggerDownCount >= 3)
                {
                    RightTriggerDownCount = 0;

                    ToggleAnimation();
                }
            }




            oldIsRightTigerClick = IsRightTigerClick;

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
                if (e.KeyCode == (int)Keys.D1) SetOtherMaterial(otherMaterials);
                else if (e.KeyCode == (int)Keys.D2) SetOtherMaterial(otherMaterials2);
                else if (e.KeyCode == (int)Keys.D3) SetOtherMaterial(otherMaterials3);
            }

            if (e.KeyCode == (int)Keys.T)
            {
                ToggleAnimation();
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

        private void Update()
        {
            if (RightTriggerDownCount != 0)
            {
                RightTriggerDownTime += Time.deltaTime;

                //トリガークリックされたときに経過時間到達していた場合キャンセル
                if (RightTriggerDownTime > 0.5f)
                {
                    RightTriggerDownCount = 0;
                    RightTriggerDownTime = 0;
                }
            }
        }


        private void OnEnable()
        {
        }

        private void OnDisable()
        {
            KeyboardAction.KeyDownEvent -= KeyboardAction_KeyDown;
            KeyboardAction.KeyUpEvent -= KeyboardAction_KeyUp;
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

            Debug.Log("OnSetting OtherMaterials Load Start");

            if (!FileLoad(otherMaterials, ref filename1, "(1st ChangeFile)")) return;
            otherAnimation1 = ReturnAnimatorController;
            if (FileLoad(otherMaterials2, ref filename2, "(2nd ChangeFile)"))
            {
                otherAnimation2 = ReturnAnimatorController;
                FileLoad(otherMaterials3, ref filename3, "(3rd ChangeFile)");
                otherAnimation3 = ReturnAnimatorController;
            }

            Debug.Log("OnSetting OtherMaterials Load End");
            SetOtherMaterial(otherMaterials);
            SetAnimation(otherAnimation1);
            Debug.Log("OnSetting SetOtherMaterials Load End");

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
            //使う予定だけどまだ使ってない
            //foreach (Renderer renderer in Model.GetComponentsInChildren<Renderer>())
            //{
            //    DefaultMaterials.Add(renderer, renderer.sharedMaterials);
            //}

            //自動MaterialChangeONの場合ファイル読み込みを行って設定する
            var meta = Model.GetComponent<VRM.VRMMeta>();
            VRMMetaKey = $"{meta.Meta.Title}_{meta.Meta.Version}_{meta.Meta.Author}";
            if (OtherMaterialChangeSetting.Instance.OtherParameter.AutoMaterialChange & VRMMetaKey != "")
            {
                if (OtherMaterialChangeSetting.Instance.OtherParameter.List != null)
                    if (OtherMaterialChangeSetting.Instance.OtherParameter.List.ContainsKey(VRMMetaKey))
                    {
                        OtherMaterialLoad(OtherMaterialChangeSetting.Instance.OtherParameter.List[VRMMetaKey].FileAddress1, otherMaterials);
                        otherAnimation1 = ReturnAnimatorController;
                        OtherMaterialLoad(OtherMaterialChangeSetting.Instance.OtherParameter.List[VRMMetaKey].FileAddress2, otherMaterials2);
                        otherAnimation2 = ReturnAnimatorController;
                        OtherMaterialLoad(OtherMaterialChangeSetting.Instance.OtherParameter.List[VRMMetaKey].FileAddress3, otherMaterials3);
                        otherAnimation3 = ReturnAnimatorController;
                        if (otherMaterials.Count != 0)
                        {
                            SetOtherMaterial(otherMaterials);
                            SetAnimation(otherAnimation1);
                        }
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
            if (string.IsNullOrEmpty(filename)) return;
            AssetBundle assetBundle = AssetBundle.LoadFromFile(filename);
            AssetsMaterial = assetBundle.LoadAllAssets();
            assetBundle.Unload(false);
            ReturnAnimatorController = null;
            foreach (var asset in AssetsMaterial)
            {
                if (asset is Material material)
                {
                    set[material.name] = material;
                }
                else //if (asset is RuntimeAnimatorController Anime)
                {
                    ReturnAnimatorController = (RuntimeAnimatorController)asset;
                    Debug.Log($"OtherMaterialLoad {ReturnAnimatorController.name}");
                }
            }
        }

        /// <summary>
        /// マテリアルチェンジ実行(.mcファイルパッチ処理)
        /// </summary>
        private void SetOtherMaterial(Dictionary<string, Material> set)
        {
            try { 

                if (set == null) return;
                if (Model == null) return;

                foreach (Renderer renderer in Model.GetComponentsInChildren<Renderer>(true))
                {
                    Debug.Log($"{renderer.name}");
                    Material[] newMaterials = new Material[renderer.sharedMaterials.Length];
                    for (int index = 0; index < renderer.sharedMaterials.Length; index++)
                    {
                        if (set.ContainsKey(renderer.sharedMaterials[index].name))
                        {
                            Debug.Log($"SetOtherMaterial MaterialChange MaterialName:{renderer.sharedMaterials[index].name}");
                            newMaterials[index] = set[renderer.sharedMaterials[index].name];
                            //同名shader(BeatSaber用シェーダーが来た場合、α値有りのシェーダーに置き換えなおす)
                            if (Shaders.ContainsKey(newMaterials[index].shader.name))
                                {
                                    Debug.Log($"SetOtherMaterial BeatSaberShader Change ShaderName:{newMaterials[index].shader.name}");
                                    newMaterials[index].shader = Shaders[newMaterials[index].shader.name];
                                }

                            }
                        else
                            newMaterials[index] = renderer.sharedMaterials[index];
                    }
                    renderer.sharedMaterials = newMaterials;
                }

            }
            catch (Exception)
            { Debug.Log($"OtherMaterialChange失敗"); }
        }

        #region Animator設定
        Animator VRMAnimator;
        Int32 LayerNo;
        private void SetAnimation(RuntimeAnimatorController animation)
        {
            Debug.Log($"SetAnimation Start");
            if (animation == null) return;

            //Animatorがなければ作成
             Debug.Log($"SetAnimation Get");
            var animator = Model.GetComponent<Animator>();
            if (animator == null)
                Model.AddComponent<Animator>();

            VRMAnimator = animator;

            //Animationが登録済みの場合でレイヤーMaterialChangeがある場合は削除する
             Debug.Log($"SetAnimation Set");
            VRMAnimator.runtimeAnimatorController = animation;
            LayerNo = VRMAnimator.GetLayerIndex("MaterialChange");
             Debug.Log($"SetAnimation End");
        }

        /// <summary>
        /// 次のアニメーションへ遷移
        /// </summary>
        public void ToggleAnimation()
        {
            //アニメーターがなかったり、遷移中は実行しない
            if (VRMAnimator == null) return;
            if (VRMAnimator.IsInTransition(LayerNo)) return;

            var max = VRMAnimator.GetInteger("MaxCount");
            var now = VRMAnimator.GetInteger("SetPoint");
            var set = (int)((now + 1) % (max + 1));
            set = set == 0 ? 1 : set;
            VRMAnimator.SetInteger("SetPoint", set);
             Debug.Log($"ToggleAnimation SetPoint:{set}");
        }

        #endregion

    }
}


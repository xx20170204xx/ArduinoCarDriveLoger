/*
 ////////////////////////////////////////////////////////////////////////////////////////////////////////
//FileName         : SoundEffectCheck.cs
//FileType         : C# Source file
//FileEncoding     : UTF-8(LF)
//Author           : fay
//Created On       : 2018/10/14
//Last Modified On : 2018/10/14
//Copy Rights      : (C) 2018 fay.
//Description      : Unity Editor Sound Effect Check Tool
//                 : Usage
//                     1.Select menu [Window] -> [Sound Effect Check]
//                     2.SE Folder is Drag & Drop to "* D&D Area *"
//                     3.Play button is push
////////////////////////////////////////////////////////////////////////////////////////////////////////
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace fay.SoundEffectCheck {
#if UNITY_EDITOR

    public class SoundEffectCheck : EditorWindow
    {

        string dataPath;
        List<string> searchPaths = null;
        List<AudioClip> clipList = new List<AudioClip>();
        Vector2 clipListPos = new Vector2();
        float updateProg = 0.0f;
        float addPer = 0.0f;
        AudioClip lastPlayClip = null;
        float playTime = 0;

        [MenuItem("Window/Sound Effect Check")]
        static void Open()
        {
            var _window = CreateInstance<SoundEffectCheck>();
            _window.Show();
        } /* Open */

        void Awake()
        {
            titleContent = new GUIContent("Sound Effect Check");
            dataPath = Application.dataPath.ToUpper().Replace("\\", "/");
            // Debug.Log("dataPath:" + dataPath);
        } /* Awake */

        void Update()
        {
            bool _repaint = false;
            var _source = GetAudioSource();
            if (searchPaths != null && searchPaths.Count != 0)
            {
                string _assetPath = searchPaths[0];
                searchPaths.RemoveAt(0);
                // Debug.Log(_assetPath);
                if (searchPaths.Count == 0)
                {
                    searchPaths = null;
                }

                updateProg += addPer;
                try
                {
                    var _asset = AssetDatabase.LoadAssetAtPath<AudioClip>(_assetPath);
                    if (_asset)
                    {
                        clipList.Add(_asset);
                    }
                    _repaint = true;
                }
                catch( System.Exception _e )
                {
                    Debug.LogError( _e.Message );
                }
            }
            if (_source != null && _source.isPlaying == true)
            {
                playTime = _source.time;
                _repaint = true;
            }

            if (_repaint == true)
            {
                Repaint();
            }


        } /* Update */

        void OnGUI()
        {

            EditorGUILayout.BeginVertical();
            Rect _rect = EditorGUILayout.GetControlRect();
            // _rect.height *= 1.5f;
            if (searchPaths == null || searchPaths.Count == 0)
            {
                List<string> _dropPathList = CreateDnDGUIPath(_rect);
                if (_dropPathList.Count != 0)
                {
                    lastPlayClip = null;
                    searchPaths = UpdateList(_dropPathList);
                    addPer = 1.0f / (float)searchPaths.Count;
                    updateProg = 0.0f;
                }
            }
            else {
                EditorGUI.ProgressBar(_rect, updateProg, "");
            }
            EditorGUILayout.EndVertical();

#if false
            /* 波形表示 */
            EditorGUILayout.Separator();
            if(lastPlayClip != null && lastPlayClip.loadType == AudioClipLoadType.DecompressOnLoad)
            {
                try
                {
                    float[] allSamples = new float[lastPlayClip.samples * lastPlayClip.channels];
                    lastPlayClip.GetData(allSamples, 0);
                    var _bkColor = GUI.backgroundColor;
                    GUI.backgroundColor = Color.gray;

                    for (int cnum = 0; cnum < lastPlayClip.channels; cnum++)
                    {
                        var _r = GUILayoutUtility.GetRect(1, 10000, 75, 75);
                        GUI.Box(_r, "");
                        GUI.BeginGroup(_r);
                        //GUI.BeginClip(_r);
                        /* TODO:取り出す位置は適当 */
                        AudioCurveRendering.DrawCurve(new Rect(0, 0, _r.width, _r.height), (t) => allSamples[(int)(t * (allSamples.Length - 1))], new Color(1.0f, 0.64f, 0.0f));
                        //GUI.EndClip();
                        GUI.EndGroup();
                    }
                    GUI.backgroundColor = _bkColor;
                }
                catch(System.Exception _e) {
                    Debug.LogError( _e.Message );
                }
            }
#endif

            EditorGUILayout.BeginHorizontal();
            if (lastPlayClip != null){
                GUILayout.Label(playTime.ToString("#0.000") + " / " + lastPlayClip.length.ToString("#0.000"));
            }
            if (true == GUILayout.Button("Stop"))
            {
                Stop();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            EditorGUILayout.BeginVertical();
            clipListPos = EditorGUILayout.BeginScrollView( clipListPos);
            for (int _num = 0; _num < clipList.Count; _num++)
            {
                Color _bkColor = GUI.backgroundColor;
                if (lastPlayClip == clipList[_num])
                {
                    GUI.backgroundColor = Color.cyan;
                }

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button( "Play"))
                {
                    Play(_num);
                }
                clipList[_num] = (AudioClip)EditorGUILayout.ObjectField( clipList[_num], typeof(AudioClip), false);
                EditorGUILayout.EndHorizontal();

                GUI.backgroundColor = _bkColor;
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

        } /* OnGUI */


        static AudioSource GetAudioSource()
        {
            var _gameObjectName = "HideAudioSourceObject";
            var _gameObject = GameObject.Find(_gameObjectName);
            if (_gameObject == null)
            {
                //HideAndDontSave フラグを立てて非表示・保存しないようにする
                _gameObject = EditorUtility.CreateGameObjectWithHideFlags(_gameObjectName,
                HideFlags.HideAndDontSave, typeof(AudioSource));
            }
            var _hideAudioSource = _gameObject.GetComponent<AudioSource>();
            // _hideAudioSource.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
            return _hideAudioSource;
        } /* GetAudioSource */

    #region action
        private void Play( int _num = 0 )
        {
            if (clipList.Count == 0)
            {
                return;
            }

            if (clipList[_num] == null)
            {
                return;
            }
            // Debug.Log( "Play" );
            var _source = GetAudioSource();
            if (_source.isPlaying == true)
            {
                _source.Stop();
            }
            lastPlayClip = clipList[_num];
            _source.clip = lastPlayClip;
            _source.Play();
        } /* Play */

        private void Stop()
        {
            // Debug.Log("Stop");
            GetAudioSource().Stop();
        } /* Stop */



        private List<string> UpdateList(List<string> _dropPathList)
        {
            // Debug.Log("UpdateList start");
            List<string> _filepaths = new List<string>();

            clipList.Clear();

            foreach (string _path in _dropPathList)
            {
                System.IO.DirectoryInfo _info = new System.IO.DirectoryInfo(_path);

                System.IO.FileInfo[] _finfos = _info.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
                foreach (var _finfo in _finfos)
                {
                    if (dataPath.Length >= _finfo.FullName.Length)
                    {
                        continue;
                    }
                    string _c = _finfo.FullName.Substring(0, dataPath.Length).ToUpper().Replace("\\","/");
                    if (_c.Contains(dataPath) == false)
                    {
                        continue;
                    }
                    string _assetDirName = System.IO.Path.GetFileName(Application.dataPath);
                    string _assetPath = _assetDirName + "/" + _finfo.FullName.Substring(dataPath.Length + 1, _finfo.FullName.Length - dataPath.Length - 1);
                    // Debug.Log(_assetDirName);
                    // Debug.Log(_assetPath);
                    _filepaths.Add( _assetPath );
                }
                //
            }
            // Debug.Log("UpdateList end");
            return _filepaths;
        } /* UpdateList */
    #endregion

    #region DnD
        private List<Object> CreateDnDGUIObj(Rect _rect)
        {
            List<Object> _list = new List<Object>();
            GUI.Box(_rect, "* D&D Area *");
            if (!_rect.Contains(Event.current.mousePosition))
            {
                return _list;
            }

            EventType _eventType = Event.current.type;

            if (_eventType == EventType.DragUpdated || _eventType == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (_eventType == EventType.DragPerform)
                {
                    _list = new List<Object>(DragAndDrop.objectReferences);
                    DragAndDrop.AcceptDrag();
                }
                Event.current.Use();
            }

            return _list;
        } /* CreateDnDGUIObj */

        private List<string> CreateDnDGUIPath(Rect _rect)
        {
            List<string> _list = new List<string>();
            GUI.Box(_rect, "* D&D Area *");
            if (!_rect.Contains(Event.current.mousePosition))
            {
                return _list;
            }

            EventType _eventType = Event.current.type;

            if (_eventType == EventType.DragUpdated || _eventType == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (_eventType == EventType.DragPerform)
                {
                    _list = new List<string>(DragAndDrop.paths);
                    DragAndDrop.AcceptDrag();
                }
                Event.current.Use();
            }

            return _list;
        } /* CreateDnDGUIPath */
    #endregion // DnD


    } // class end
#endif

} // namespace end
/* EOF */
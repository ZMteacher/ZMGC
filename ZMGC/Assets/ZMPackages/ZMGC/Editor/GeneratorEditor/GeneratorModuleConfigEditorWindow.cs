
  using System.IO;
  using UnityEditor;
  using UnityEngine;
  
  public class GeneratorModuleConfigEditorWindow : EditorWindow
  {
      public const string ConfigAssetPath = "Assets/ZMPackages/ZMGC/Editor/GeneratorEditor/GeneratorModuleConfig.asset";
  
      private GeneratorModuleConfig config;
      private SerializedObject serializedConfig;
  
      private GUIStyle _titleStyle;
      private GUIStyle _subtitleStyle;
      private GUIStyle _sectionHeaderStyle;
  
      // 脚本生成：模块名称输入
      private string _moduleName = string.Empty;
  
      // 新增：游戏世界名称输入
      private string _worldName = string.Empty;
  
      [MenuItem("ZM/Generator MVC", priority = 1)]
      public static void ShowWindow()
      {
          var window = GetWindow<GeneratorModuleConfigEditorWindow>();
          window.titleContent = new GUIContent("Generator Config", EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow").image);
          window.minSize = new Vector2(420, 400);
      }
  
      private void OnEnable()
      {
         
          AutoLoadConfig();
      }
  
      private void AutoLoadConfig()
      {
          config = AssetDatabase.LoadAssetAtPath<GeneratorModuleConfig>(ConfigAssetPath);
          if (config != null)
          {
              serializedConfig = new SerializedObject(config);
          }
      }
  
      private void OnGUI()
      {
          // 懒加载样式，保证在 GUI 环境下再访问 EditorStyles
          if (_titleStyle == null || _subtitleStyle == null || _sectionHeaderStyle == null)
          {
              InitStyles();
          }

          DrawHeader();

          EditorGUILayout.Space(6);

          DrawConfigPicker();

          if (config == null)
              return;

          if (serializedConfig == null || serializedConfig.targetObject != config)
              serializedConfig = new SerializedObject(config);

          serializedConfig.Update();

          EditorGUILayout.Space(4);

          EditorGUILayout.BeginVertical(EditorStyles.helpBox);
          DrawSavePathSection();
          EditorGUILayout.Space(6);
          DrawModulesSection();
          EditorGUILayout.Space(6);
          DrawScriptGenerateSection();
          EditorGUILayout.Space(6);
          DrawWorldBuildSection();
          EditorGUILayout.EndVertical();

          serializedConfig.ApplyModifiedProperties();
          EditorUtility.SetDirty(config);
      }

      private void InitStyles()
      {
          _titleStyle = new GUIStyle(EditorStyles.label)
          {
              fontSize = 16,
              fontStyle = FontStyle.Bold
          };

          _subtitleStyle = new GUIStyle(EditorStyles.label)
          {
              fontSize = 11,
              fontStyle = FontStyle.Italic,
              normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
          };

          _sectionHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
          {
              fontSize = 12
          };
      }
  
      private void DrawHeader()
      {
          EditorGUILayout.BeginVertical(EditorStyles.helpBox);
          EditorGUILayout.BeginHorizontal();
  
          var icon = EditorGUIUtility.IconContent("d_SettingsIcon").image;
          GUILayout.Label(icon, GUILayout.Width(32), GUILayout.Height(32));
  
          EditorGUILayout.BeginVertical();
          GUILayout.Label("DMVC 自动生成配置", _titleStyle);
          GUILayout.Label("配置代码生成模块及保存路径", _subtitleStyle);
          EditorGUILayout.EndVertical();
  
          EditorGUILayout.EndHorizontal();
          EditorGUILayout.EndVertical();
      }
  
      private void DrawConfigPicker()
      {
          EditorGUILayout.BeginVertical(EditorStyles.helpBox);
          EditorGUILayout.LabelField(new GUIContent("配置资源", EditorGUIUtility.IconContent("d_Project").image), _sectionHeaderStyle);
  
          EditorGUI.indentLevel++;
  
          using (new EditorGUI.DisabledScope(true))
          {
              EditorGUILayout.ObjectField("Config Asset", config, typeof(GeneratorModuleConfig), false);
          }
  
          if (config == null)
          {
              config = (GeneratorModuleConfig)EditorGUILayout.ObjectField("Config Asset (手动选择)", config, typeof(GeneratorModuleConfig), false);
              if (config != null)
                  serializedConfig = new SerializedObject(config);
          }
  
          EditorGUI.indentLevel--;
  
          if (config == null)
          {
              EditorGUILayout.Space(2);
              EditorGUILayout.HelpBox("未在路径 `Assets/ZMPackages/ZMGC/Editor/GeneratorEditor/GeneratorModuleConfig.asset` 找到 GeneratorModuleConfig 资源，请检查路径或手动选择。", MessageType.Info);
          }
  
          EditorGUILayout.EndVertical();
      }
  
      private void DrawSavePathSection()
      {
          var savePathProp = serializedConfig.FindProperty("savePath");
  
          EditorGUILayout.BeginHorizontal();
          var folderIcon = EditorGUIUtility.IconContent("d_Folder Icon").image;
          GUILayout.Label(folderIcon, GUILayout.Width(18), GUILayout.Height(18));
          GUILayout.Label("脚本保存路径", _sectionHeaderStyle);
          EditorGUILayout.EndHorizontal();
  
          EditorGUI.indentLevel++;
          EditorGUILayout.PropertyField(savePathProp, GUIContent.none);
          EditorGUI.indentLevel--;
      }
  
      private void DrawModulesSection()
      {
          var modulesProp = serializedConfig.FindProperty("modules");
  
          EditorGUILayout.BeginHorizontal();
          var listIcon = EditorGUIUtility.IconContent("d_FilterByType").image;
          GUILayout.Label(listIcon, GUILayout.Width(18), GUILayout.Height(18));
          GUILayout.Label("模块列表", _sectionHeaderStyle);
          EditorGUILayout.EndHorizontal();
  
          EditorGUI.indentLevel++;
          EditorGUILayout.PropertyField(modulesProp, GUIContent.none, true);
          EditorGUI.indentLevel--;
      }
  
      // 已有：脚本生成区域
      private void DrawScriptGenerateSection()
      {
          EditorGUILayout.BeginVertical(EditorStyles.helpBox);
  
          EditorGUILayout.BeginHorizontal();
          var scriptIcon = EditorGUIUtility.IconContent("d_cs Script Icon").image;
          GUILayout.Label(scriptIcon, GUILayout.Width(18), GUILayout.Height(18));
          GUILayout.Label("脚本生成", _sectionHeaderStyle);
          EditorGUILayout.EndHorizontal();
  
          EditorGUILayout.Space(4);
  
          EditorGUI.indentLevel++;
          _moduleName = EditorGUILayout.TextField("模块名称", _moduleName);
          EditorGUI.indentLevel--;
  
          EditorGUILayout.Space(4);
  
          bool disableButtons = string.IsNullOrWhiteSpace(_moduleName);
  
          using (new EditorGUI.DisabledScope(disableButtons))
          {
              EditorGUILayout.BeginHorizontal();
  
              if (GUILayout.Button("消息层", GUILayout.Height(24)))
              {
                  Debug.Log($"生成消息层脚本：{_moduleName}");
       
                  GeneratorMsgCtrl.AutoGeneratorMsgScripts("",_moduleName+"MsgMgr");
              }
  
              if (GUILayout.Button("逻辑层", GUILayout.Height(24)))
              {
                  Debug.Log($"生成逻辑层脚本：{_moduleName}");
                  
                  GeneratorLogicCtrl.AutoGeneratorLogicScripts("",_moduleName+"LogicCtrl");
              }
  
              if (GUILayout.Button("数据层", GUILayout.Height(24)))
              {
                  Debug.Log($"生成数据层脚本：{_moduleName}");
                  GeneratorDataCtrl.AutoGeneratorDataScripts("",_moduleName+"DataMgr");
              }
  
              
  
              EditorGUILayout.EndHorizontal();
          }
  
          EditorGUILayout.EndVertical();
      }
  
       // 新增：构建游戏世界区域
      private void DrawWorldBuildSection()
      {
          EditorGUILayout.BeginVertical(EditorStyles.helpBox);

          // 标题行（配一个场景/世界相关图标）
          EditorGUILayout.BeginHorizontal();
          var sceneIcon = EditorGUIUtility.IconContent("d_SceneAsset Icon").image;
          GUILayout.Label(sceneIcon, GUILayout.Width(18), GUILayout.Height(18));
          GUILayout.Label("构建游戏世界", _sectionHeaderStyle);
          EditorGUILayout.EndHorizontal();

          EditorGUILayout.Space(4);

          // 游戏世界名称输入
          EditorGUI.indentLevel++;
          _worldName = EditorGUILayout.TextField("游戏世界名称", _worldName);
          EditorGUI.indentLevel--;

          EditorGUILayout.Space(4);

          // 按输入内容切换按钮颜色：有内容为绿色，无内容为灰色，并控制可点状态
          bool hasWorldName = !string.IsNullOrWhiteSpace(_worldName);

          Color oldColor = GUI.backgroundColor;
          GUI.backgroundColor = hasWorldName ? Color.green : Color.gray;

          using (new EditorGUI.DisabledScope(!hasWorldName))
          {
              if (GUILayout.Button("构建游戏世界", GUILayout.Height(26)))
              {
                  // 这里写实际的构建逻辑
                  Debug.Log($"开始构建游戏世界：{_worldName}");
                  
                  config.GetNameSpaceByWorldName(_worldName, out string nameSpace);
                  if (string.IsNullOrEmpty(nameSpace))
                  {
 
                      EditorUtility.DisplayDialog("错误", $"未找到与游戏世界 '{_worldName}' 对应的命名空间，请检查配置。", "确定");
                      EditorGUILayout.EndVertical();
                      return;
                  }

                  Debug.Log($"获取到的命名空间：{nameSpace}");
                  string scriptName = _worldName.Replace("World", "");
                  GeneratorDataCtrl.AutoGeneratorDataScripts(_worldName,scriptName+"DataMgr",nameSpace,true);
                  GeneratorLogicCtrl.AutoGeneratorLogicScripts(_worldName,scriptName+"LogicCtrl",nameSpace,true);
                  GeneratorMsgCtrl.AutoGeneratorMsgScripts(_worldName,scriptName+"MsgMgr",nameSpace,true);
                  // 新增：生成世界脚本
                  CreateWorldScript(_worldName, nameSpace);
              }
          }

          GUI.backgroundColor = oldColor;

          EditorGUILayout.EndVertical();
      }
      /// <summary>
      /// 创建世界脚本：命名空间使用传入的 nameSpace，类名和文件名使用 worldName。
      /// </summary>
      private void CreateWorldScript(string worldName, string nameSpace)
      {
          if (serializedConfig == null)
          {
              Debug.LogError("Generator config 未初始化，无法生成世界脚本。");
              return;
          }

          // 从配置中读取保存路径
          var savePathProp = serializedConfig.FindProperty("savePath");
          string saveFolder = $"{Application.dataPath}/{savePathProp.stringValue}/{worldName}";
         
          if (string.IsNullOrEmpty(saveFolder))
          {
              saveFolder = "Assets";
          }

          // 确保目录存在
          if (!Directory.Exists(saveFolder))
          {
              Directory.CreateDirectory(saveFolder);
          }

          // 目标文件完整路径（带 .cs）
          string fileName = worldName + ".cs";
          string assetPath = Path.Combine(saveFolder, fileName).Replace("\\", "/");
          assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

          string className = worldName;

          // 生成脚本内容（基于你提供的模板）
          string scriptContent =
              $@"using UnityEngine;

namespace {nameSpace}
{{
    public class {className} : World
    {{
        public override void OnCreate()
        {{
            base.OnCreate();
        }}

        public override void OnDestroy()
        {{
            base.OnDestroy();
        }}

        public override void OnUpdate()
        {{
            base.OnUpdate();
        }}

        public override void OnDestroyPostProcess(object args)
        {{
            base.OnDestroyPostProcess(args);
        }}
    }}
}}";

        File.WriteAllText(assetPath, scriptContent);
        AssetDatabase.ImportAsset(assetPath);
        AssetDatabase.Refresh();

        Debug.Log($@"世界脚本已生成：{assetPath}");
    }
}
    
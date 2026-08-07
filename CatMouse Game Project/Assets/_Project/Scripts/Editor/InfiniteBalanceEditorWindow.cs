using System;
using System.Collections.Generic;
using System.IO;
using CatMouse.Game.Enemy;
using CatMouse.Game.Run;
using UnityEditor;
using UnityEngine;

namespace CatMouse.Game.Editor
{
    public sealed class InfiniteBalanceEditorWindow : EditorWindow
    {
        private const string WindowMenuPath = "CatMouse/Balance/무한 난이도 밸런스";
        private const string ProfileDirectoryName = "BalanceLocal";
        private const string ProfileFileName = "InfiniteBalanceProfile.json";
        private const int MinimumDifficultyLevel = 1;
        private const int ChartGridLineCount = 4;
        private const float ChartHeight = 190f;

        private LocalBalanceProfile _profile;
        private Vector2 _scrollPosition;
        private int _previewEnemyIndex;
        private int _previewStartLevel = MinimumDifficultyLevel;
        private int _previewLevelCount = 8;
        private int _chartMaximumLevel = 20;
        private string _message;

        [MenuItem(WindowMenuPath)]
        private static void Open()
        {
            GetWindow<InfiniteBalanceEditorWindow>("무한 난이도 밸런스");
        }

        private void OnEnable()
        {
            LoadOrCreateProfile();
        }

        private void OnGUI()
        {
            if (_profile == null)
            {
                LoadOrCreateProfile();
            }

            DrawToolbar();

            if (!string.IsNullOrEmpty(_message))
            {
                EditorGUILayout.HelpBox(_message, MessageType.Info);
            }

            EditorGUILayout.LabelField("로컬 JSON 경로", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(ProfilePath, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            EditorGUILayout.HelpBox(
                "이 JSON은 로컬 작업 원본이며 Git에 포함되지 않습니다. 검토가 끝난 값만 '게임 데이터에 적용'으로 ScriptableObject에 반영합니다.",
                MessageType.None);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            DrawPowerCurve();
            DrawSchedule();
            DrawWaves();
            DrawEnemies();
            DrawPreview();
            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("게임 데이터에서 불러오기", EditorStyles.toolbarButton))
            {
                ReadGameData();
                SaveProfile();
                _message = "현재 ScriptableObject 값을 로컬 JSON에 저장했습니다.";
            }

            if (GUILayout.Button("JSON 다시 불러오기", EditorStyles.toolbarButton))
            {
                LoadProfile();
            }

            if (GUILayout.Button("JSON 저장", EditorStyles.toolbarButton))
            {
                SaveProfile();
                _message = "로컬 JSON을 저장했습니다.";
            }

            if (GUILayout.Button("게임 데이터에 적용", EditorStyles.toolbarButton))
            {
                ApplyProfileToGameData();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawPowerCurve()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("예상 플레이어 단일 대상 DPS", EditorStyles.boldLabel);
            DrawAssetLabel<ExpectedPlayerPowerCurveDefinition>("성장 곡선 에셋", _profile.PowerCurve.AssetPath);

            _profile.PowerCurve.DpsIncreasePerLevelAfterLastMilestone = Mathf.Max(
                0f,
                EditorGUILayout.FloatField(
                    "마지막 구간 이후 레벨당 DPS 증가",
                    _profile.PowerCurve.DpsIncreasePerLevelAfterLastMilestone));

            for (int index = 0; index < _profile.PowerCurve.Milestones.Count; index++)
            {
                LocalPowerMilestone milestone = _profile.PowerCurve.Milestones[index];
                EditorGUILayout.BeginHorizontal();
                milestone.DifficultyLevel = Mathf.Max(
                    MinimumDifficultyLevel,
                    EditorGUILayout.IntField("난이도 레벨", milestone.DifficultyLevel));
                milestone.ExpectedDps = Mathf.Max(0.01f, EditorGUILayout.FloatField("예상 DPS", milestone.ExpectedDps));

                if (GUILayout.Button("삭제", GUILayout.Width(48f)))
                {
                    _profile.PowerCurve.Milestones.RemoveAt(index);
                    EditorGUILayout.EndHorizontal();
                    break;
                }

                EditorGUILayout.EndHorizontal();
                _profile.PowerCurve.Milestones[index] = milestone;
            }

            if (GUILayout.Button("마일스톤 추가"))
            {
                int nextLevel = GetNextMilestoneLevel();
                _profile.PowerCurve.Milestones.Add(new LocalPowerMilestone
                {
                    DifficultyLevel = nextLevel,
                    ExpectedDps = EvaluateExpectedDps(nextLevel),
                });
            }

            _chartMaximumLevel = Mathf.Max(
                MinimumDifficultyLevel + 1,
                EditorGUILayout.IntField("곡선 표시 최대 레벨", _chartMaximumLevel));
            DrawLevelCurveChart(
                "플레이어 예상 DPS 곡선",
                "초록색: 레벨별 예상 단일 대상 DPS",
                new Color(0.28f, 0.86f, 0.5f),
                EvaluateExpectedDps,
                true);

            EditorGUILayout.EndVertical();
        }

        private void DrawSchedule()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("무한 난이도 증가 규칙", EditorStyles.boldLabel);
            DrawAssetLabel<InfiniteSpawnScheduleDefinition>("스폰 일정 에셋", _profile.Schedule.AssetPath);

            _profile.Schedule.MetersPerDifficultyLevel = Mathf.Max(
                1f,
                EditorGUILayout.FloatField("난이도 레벨당 거리(m)", _profile.Schedule.MetersPerDifficultyLevel));
            _profile.Schedule.MinimumCooldownDistanceMeters = Mathf.Max(
                1f,
                EditorGUILayout.FloatField("최소 스폰 간격(m)", _profile.Schedule.MinimumCooldownDistanceMeters));
            _profile.Schedule.CooldownReductionMeters = Mathf.Max(
                0f,
                EditorGUILayout.FloatField("레벨당 간격 감소(m)", _profile.Schedule.CooldownReductionMeters));
            _profile.Schedule.MaximumEnemySpeedBonus = Mathf.Max(
                0f,
                EditorGUILayout.FloatField("최대 적 속도 보정", _profile.Schedule.MaximumEnemySpeedBonus));
            _profile.Schedule.EnemySpeedIncrease = Mathf.Max(
                0f,
                EditorGUILayout.FloatField("레벨당 적 속도 보정", _profile.Schedule.EnemySpeedIncrease));
            _profile.Schedule.MaximumConcurrentEnemyBonus = Mathf.Max(
                0,
                EditorGUILayout.IntField("최대 동시 적 추가 수", _profile.Schedule.MaximumConcurrentEnemyBonus));
            _profile.Schedule.ConcurrentEnemyBonus = Mathf.Max(
                0,
                EditorGUILayout.IntField("레벨당 동시 적 추가 수", _profile.Schedule.ConcurrentEnemyBonus));

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("플레이어 체력 감소율", EditorStyles.miniBoldLabel);
            _profile.Schedule.BaseHealthLossMultiplier = Mathf.Max(
                0.01f,
                EditorGUILayout.FloatField("초기 체력 감소 배율", _profile.Schedule.BaseHealthLossMultiplier));
            _profile.Schedule.HealthLossMultiplierIncreasePerLevel = Mathf.Max(
                0f,
                EditorGUILayout.FloatField("레벨당 감소 배율 증가", _profile.Schedule.HealthLossMultiplierIncreasePerLevel));
            _profile.Schedule.MaximumHealthLossMultiplier = Mathf.Max(
                _profile.Schedule.BaseHealthLossMultiplier,
                EditorGUILayout.FloatField("최대 체력 감소 배율", _profile.Schedule.MaximumHealthLossMultiplier));
            EditorGUILayout.EndVertical();
        }

        private void DrawWaves()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("웨이브별 등장 규칙", EditorStyles.boldLabel);

            for (int index = 0; index < _profile.Waves.Count; index++)
            {
                LocalWave wave = _profile.Waves[index];
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField(GetAssetName(wave.AssetPath), EditorStyles.boldLabel);
                wave.MinimumDifficultyLevel = Mathf.Max(
                    MinimumDifficultyLevel,
                    EditorGUILayout.IntField("등장 시작 레벨", wave.MinimumDifficultyLevel));
                wave.CooldownDistanceMeters = Mathf.Max(
                    1f,
                    EditorGUILayout.FloatField("기본 스폰 간격(m)", wave.CooldownDistanceMeters));
                wave.MaximumConcurrentEnemies = Mathf.Max(
                    1,
                    EditorGUILayout.IntField("기본 동시 적 수", wave.MaximumConcurrentEnemies));
                wave.MinimumEnemyCount = Mathf.Max(
                    1,
                    EditorGUILayout.IntField("최소 군집 수", wave.MinimumEnemyCount));
                wave.MaximumEnemyCount = Mathf.Max(
                    wave.MinimumEnemyCount,
                    EditorGUILayout.IntField("최대 군집 수", wave.MaximumEnemyCount));
                wave.EnemySpeed = Mathf.Max(0f, EditorGUILayout.FloatField("기본 접근 속도", wave.EnemySpeed));

                EditorGUILayout.LabelField("사용 적 원형", EditorStyles.miniBoldLabel);
                for (int enemyIndex = 0; enemyIndex < wave.EnemyArchetypePaths.Count; enemyIndex++)
                {
                    EnemyArchetypeDefinition current = LoadAsset<EnemyArchetypeDefinition>(
                        wave.EnemyArchetypePaths[enemyIndex]);
                    EnemyArchetypeDefinition next = (EnemyArchetypeDefinition)EditorGUILayout.ObjectField(
                        current,
                        typeof(EnemyArchetypeDefinition),
                        false);
                    wave.EnemyArchetypePaths[enemyIndex] = next == null
                        ? string.Empty
                        : AssetDatabase.GetAssetPath(next);
                }

                if (GUILayout.Button("사용 적 원형 추가"))
                {
                    wave.EnemyArchetypePaths.Add(string.Empty);
                }

                EditorGUILayout.EndVertical();
                _profile.Waves[index] = wave;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawEnemies()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("적 원형 기본 능력치", EditorStyles.boldLabel);

            for (int index = 0; index < _profile.Enemies.Count; index++)
            {
                LocalEnemy enemy = _profile.Enemies[index];
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField(GetAssetName(enemy.AssetPath), EditorStyles.boldLabel);
                enemy.MaximumHealth = Mathf.Max(1, EditorGUILayout.IntField("기본 체력", enemy.MaximumHealth));
                enemy.ContactDamage = Mathf.Max(1, EditorGUILayout.IntField("접촉 피해", enemy.ContactDamage));
                enemy.SpeedMultiplier = Mathf.Max(0.1f, EditorGUILayout.FloatField("이동 속도 배율", enemy.SpeedMultiplier));
                EditorGUILayout.EndVertical();
                _profile.Enemies[index] = enemy;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawPreview()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("레벨 미리보기", EditorStyles.boldLabel);

            if (_profile.Enemies.Count == 0)
            {
                EditorGUILayout.HelpBox("미리볼 적 원형이 없습니다.", MessageType.Warning);
                EditorGUILayout.EndVertical();
                return;
            }

            string[] enemyNames = new string[_profile.Enemies.Count];
            for (int index = 0; index < _profile.Enemies.Count; index++)
            {
                enemyNames[index] = GetAssetName(_profile.Enemies[index].AssetPath);
            }

            _previewEnemyIndex = Mathf.Clamp(_previewEnemyIndex, 0, _profile.Enemies.Count - 1);
            _previewEnemyIndex = EditorGUILayout.Popup("미리볼 적", _previewEnemyIndex, enemyNames);
            _previewStartLevel = Mathf.Max(
                MinimumDifficultyLevel,
                EditorGUILayout.IntField("시작 레벨", _previewStartLevel));
            _previewLevelCount = Mathf.Clamp(EditorGUILayout.IntField("표시 레벨 수", _previewLevelCount), 1, 20);

            LocalEnemy enemy = _profile.Enemies[_previewEnemyIndex];
            DrawLevelCurveChart(
                $"{GetAssetName(enemy.AssetPath)} 체력 곡선",
                "주황색: 플레이어 성장에 맞춰 계산되는 적 체력",
                new Color(1f, 0.62f, 0.2f),
                level => GetExpectedEnemyHealth(enemy, level),
                false);

            DrawLevelCurveChart(
                "플레이어 체력 감소율",
                "보라색: 자연 감소와 적 피해에 공통 적용되는 배율",
                new Color(0.62f, 0.45f, 0.88f),
                EvaluateHealthLossMultiplier,
                false);

            for (int offset = 0; offset < _previewLevelCount; offset++)
            {
                int level = _previewStartLevel + offset;
                float dps = EvaluateExpectedDps(level);
                int health = GetExpectedEnemyHealth(enemy, level);
                float timeToKill = health / dps;
                int availableWaves = GetAvailableWaveCount(level);

                EditorGUILayout.LabelField(
                    $"Lv.{level} 체력 감소 x{EvaluateHealthLossMultiplier(level):0.00}",
                    EditorStyles.miniLabel);

                EditorGUILayout.LabelField(
                    $"Lv.{level}  DPS {dps:0.00}  체력 {health}  처치 {timeToKill:0.00}초  사용 가능 웨이브 {availableWaves}개",
                    EditorStyles.miniLabel);
            }

            EditorGUILayout.EndVertical();
        }

        private void LoadOrCreateProfile()
        {
            if (File.Exists(ProfilePath))
            {
                LoadProfile();
                return;
            }

            ReadGameData();
            SaveProfile();
            _message = "현재 게임 데이터를 기준으로 로컬 JSON을 만들었습니다.";
        }

        private void LoadProfile()
        {
            try
            {
                _profile = JsonUtility.FromJson<LocalBalanceProfile>(File.ReadAllText(ProfilePath));
                EnsureProfileLists();
                _message = "로컬 JSON을 불러왔습니다.";
            }
            catch (Exception exception)
            {
                ReadGameData();
                _message = $"JSON을 읽지 못해 현재 게임 데이터를 불러왔습니다. {exception.Message}";
            }
        }

        private void SaveProfile()
        {
            EnsureProfileLists();
            Directory.CreateDirectory(Path.GetDirectoryName(ProfilePath));
            File.WriteAllText(ProfilePath, JsonUtility.ToJson(_profile, true));
        }

        private void ReadGameData()
        {
            _profile = new LocalBalanceProfile();

            ExpectedPlayerPowerCurveDefinition curve = FindFirstAsset<ExpectedPlayerPowerCurveDefinition>();
            InfiniteSpawnScheduleDefinition schedule = FindFirstAsset<InfiniteSpawnScheduleDefinition>();

            if (curve != null)
            {
                ReadPowerCurve(curve);
            }

            if (schedule != null)
            {
                ReadSchedule(schedule);
            }

            string[] enemyGuids = AssetDatabase.FindAssets($"t:{nameof(EnemyArchetypeDefinition)}");
            for (int index = 0; index < enemyGuids.Length; index++)
            {
                EnemyArchetypeDefinition enemy = AssetDatabase.LoadAssetAtPath<EnemyArchetypeDefinition>(
                    AssetDatabase.GUIDToAssetPath(enemyGuids[index]));
                if (enemy != null)
                {
                    ReadEnemy(enemy);
                }
            }

            EnsureProfileLists();
        }

        private void ReadPowerCurve(ExpectedPlayerPowerCurveDefinition curve)
        {
            _profile.PowerCurve.AssetPath = AssetDatabase.GetAssetPath(curve);
            SerializedObject serializedCurve = new SerializedObject(curve);
            SerializedProperty milestones = serializedCurve.FindProperty("_milestones");
            _profile.PowerCurve.DpsIncreasePerLevelAfterLastMilestone = serializedCurve
                .FindProperty("_dpsIncreasePerLevelAfterLastMilestone")
                .floatValue;

            for (int index = 0; index < milestones.arraySize; index++)
            {
                SerializedProperty milestone = milestones.GetArrayElementAtIndex(index);
                _profile.PowerCurve.Milestones.Add(new LocalPowerMilestone
                {
                    DifficultyLevel = milestone.FindPropertyRelative("_difficultyLevel").intValue,
                    ExpectedDps = milestone.FindPropertyRelative("_expectedSingleTargetDps").floatValue,
                });
            }
        }

        private void ReadSchedule(InfiniteSpawnScheduleDefinition schedule)
        {
            _profile.Schedule.AssetPath = AssetDatabase.GetAssetPath(schedule);
            SerializedObject serializedSchedule = new SerializedObject(schedule);
            _profile.Schedule.MetersPerDifficultyLevel = serializedSchedule
                .FindProperty("_metersPerDifficultyLevel")
                .floatValue;
            _profile.Schedule.MinimumCooldownDistanceMeters = serializedSchedule
                .FindProperty("_minimumCooldownDistanceMeters")
                .floatValue;
            _profile.Schedule.CooldownReductionMeters = serializedSchedule
                .FindProperty("_cooldownReductionMeters")
                .floatValue;
            _profile.Schedule.MaximumEnemySpeedBonus = serializedSchedule
                .FindProperty("_maximumEnemySpeedBonus")
                .floatValue;
            _profile.Schedule.EnemySpeedIncrease = serializedSchedule
                .FindProperty("_enemySpeedIncrease")
                .floatValue;
            _profile.Schedule.MaximumConcurrentEnemyBonus = serializedSchedule
                .FindProperty("_maximumConcurrentEnemyBonus")
                .intValue;
            _profile.Schedule.ConcurrentEnemyBonus = serializedSchedule
                .FindProperty("_concurrentEnemyBonus")
                .intValue;
            _profile.Schedule.BaseHealthLossMultiplier = serializedSchedule
                .FindProperty("_baseHealthLossMultiplier")
                .floatValue;
            _profile.Schedule.HealthLossMultiplierIncreasePerLevel = serializedSchedule
                .FindProperty("_healthLossMultiplierIncreasePerLevel")
                .floatValue;
            _profile.Schedule.MaximumHealthLossMultiplier = serializedSchedule
                .FindProperty("_maximumHealthLossMultiplier")
                .floatValue;

            SerializedProperty patterns = serializedSchedule.FindProperty("_patterns");
            for (int index = 0; index < patterns.arraySize; index++)
            {
                SpawnPatternDefinition pattern = patterns.GetArrayElementAtIndex(index)
                    .objectReferenceValue as SpawnPatternDefinition;
                if (pattern != null)
                {
                    ReadWave(pattern);
                }
            }
        }

        private void ReadWave(SpawnPatternDefinition waveAsset)
        {
            SerializedObject serializedWave = new SerializedObject(waveAsset);
            LocalWave wave = new LocalWave
            {
                AssetPath = AssetDatabase.GetAssetPath(waveAsset),
                MinimumDifficultyLevel = serializedWave.FindProperty("_minimumDifficultyLevel").intValue,
                CooldownDistanceMeters = serializedWave.FindProperty("_cooldownDistanceMeters").floatValue,
                MaximumConcurrentEnemies = serializedWave.FindProperty("_maximumConcurrentEnemies").intValue,
                MinimumEnemyCount = serializedWave.FindProperty("_minimumEnemyCount").intValue,
                MaximumEnemyCount = serializedWave.FindProperty("_maximumEnemyCount").intValue,
                EnemySpeed = serializedWave.FindProperty("_enemySpeed").floatValue,
                EnemyArchetypePaths = new List<string>(),
            };

            SerializedProperty archetypes = serializedWave.FindProperty("_enemyArchetypes");
            for (int index = 0; index < archetypes.arraySize; index++)
            {
                UnityEngine.Object archetype = archetypes.GetArrayElementAtIndex(index).objectReferenceValue;
                if (archetype != null)
                {
                    wave.EnemyArchetypePaths.Add(AssetDatabase.GetAssetPath(archetype));
                }
            }

            _profile.Waves.Add(wave);
        }

        private void ReadEnemy(EnemyArchetypeDefinition enemyAsset)
        {
            SerializedObject serializedEnemy = new SerializedObject(enemyAsset);
            _profile.Enemies.Add(new LocalEnemy
            {
                AssetPath = AssetDatabase.GetAssetPath(enemyAsset),
                MaximumHealth = serializedEnemy.FindProperty("_maximumHealth").intValue,
                ContactDamage = serializedEnemy.FindProperty("_contactDamage").intValue,
                SpeedMultiplier = serializedEnemy.FindProperty("_speedMultiplier").floatValue,
            });
        }

        private void ApplyProfileToGameData()
        {
            if (!TryGetAssets(out ExpectedPlayerPowerCurveDefinition curve, out InfiniteSpawnScheduleDefinition schedule))
            {
                return;
            }

            if (!TryValidateReferencedAssets(out string validationMessage))
            {
                _message = validationMessage;
                return;
            }

            WritePowerCurve(curve);
            WriteSchedule(schedule, curve);

            for (int index = 0; index < _profile.Waves.Count; index++)
            {
                WriteWave(_profile.Waves[index]);
            }

            for (int index = 0; index < _profile.Enemies.Count; index++)
            {
                WriteEnemy(_profile.Enemies[index]);
            }

            AssetDatabase.SaveAssets();
            SaveProfile();
            _message = "검토한 JSON 값을 게임용 ScriptableObject에 적용했습니다.";
        }

        private bool TryGetAssets(
            out ExpectedPlayerPowerCurveDefinition curve,
            out InfiniteSpawnScheduleDefinition schedule)
        {
            curve = LoadAsset<ExpectedPlayerPowerCurveDefinition>(_profile.PowerCurve.AssetPath);
            schedule = LoadAsset<InfiniteSpawnScheduleDefinition>(_profile.Schedule.AssetPath);
            if (curve != null && schedule != null)
            {
                return true;
            }

            _message = "성장 곡선 또는 스폰 일정 에셋을 찾지 못했습니다. '게임 데이터에서 불러오기'를 실행해 경로를 갱신해 주세요.";
            return false;
        }

        private bool TryValidateReferencedAssets(out string validationMessage)
        {
            for (int waveIndex = 0; waveIndex < _profile.Waves.Count; waveIndex++)
            {
                LocalWave wave = _profile.Waves[waveIndex];
                if (LoadAsset<SpawnPatternDefinition>(wave.AssetPath) == null)
                {
                    validationMessage = $"웨이브 에셋을 찾지 못했습니다: {wave.AssetPath}";
                    return false;
                }

                if (wave.EnemyArchetypePaths.Count == 0)
                {
                    validationMessage = $"{GetAssetName(wave.AssetPath)}에 사용할 적 원형을 하나 이상 지정해 주세요.";
                    return false;
                }

                for (int enemyIndex = 0; enemyIndex < wave.EnemyArchetypePaths.Count; enemyIndex++)
                {
                    if (LoadAsset<EnemyArchetypeDefinition>(wave.EnemyArchetypePaths[enemyIndex]) == null)
                    {
                        validationMessage = $"{GetAssetName(wave.AssetPath)}의 적 원형 참조가 비어 있거나 유효하지 않습니다.";
                        return false;
                    }
                }
            }

            for (int enemyIndex = 0; enemyIndex < _profile.Enemies.Count; enemyIndex++)
            {
                if (LoadAsset<EnemyArchetypeDefinition>(_profile.Enemies[enemyIndex].AssetPath) == null)
                {
                    validationMessage = $"적 원형 에셋을 찾지 못했습니다: {_profile.Enemies[enemyIndex].AssetPath}";
                    return false;
                }
            }

            validationMessage = string.Empty;
            return true;
        }

        private void WritePowerCurve(ExpectedPlayerPowerCurveDefinition curve)
        {
            Undo.RecordObject(curve, "Apply Local Infinite Balance");
            SerializedObject serializedCurve = new SerializedObject(curve);
            SerializedProperty milestones = serializedCurve.FindProperty("_milestones");
            _profile.PowerCurve.Milestones.Sort((left, right) => left.DifficultyLevel.CompareTo(right.DifficultyLevel));
            milestones.arraySize = _profile.PowerCurve.Milestones.Count;

            for (int index = 0; index < _profile.PowerCurve.Milestones.Count; index++)
            {
                LocalPowerMilestone source = _profile.PowerCurve.Milestones[index];
                SerializedProperty target = milestones.GetArrayElementAtIndex(index);
                target.FindPropertyRelative("_difficultyLevel").intValue = Mathf.Max(
                    MinimumDifficultyLevel,
                    source.DifficultyLevel);
                target.FindPropertyRelative("_expectedSingleTargetDps").floatValue = Mathf.Max(0.01f, source.ExpectedDps);
            }

            serializedCurve.FindProperty("_dpsIncreasePerLevelAfterLastMilestone").floatValue = Mathf.Max(
                0f,
                _profile.PowerCurve.DpsIncreasePerLevelAfterLastMilestone);
            serializedCurve.ApplyModifiedProperties();
            EditorUtility.SetDirty(curve);
        }

        private void WriteSchedule(
            InfiniteSpawnScheduleDefinition schedule,
            ExpectedPlayerPowerCurveDefinition curve)
        {
            Undo.RecordObject(schedule, "Apply Local Infinite Balance");
            SerializedObject serializedSchedule = new SerializedObject(schedule);
            serializedSchedule.FindProperty("_metersPerDifficultyLevel").floatValue = Mathf.Max(
                1f,
                _profile.Schedule.MetersPerDifficultyLevel);
            serializedSchedule.FindProperty("_minimumCooldownDistanceMeters").floatValue = Mathf.Max(
                1f,
                _profile.Schedule.MinimumCooldownDistanceMeters);
            serializedSchedule.FindProperty("_cooldownReductionMeters").floatValue = Mathf.Max(
                0f,
                _profile.Schedule.CooldownReductionMeters);
            serializedSchedule.FindProperty("_maximumEnemySpeedBonus").floatValue = Mathf.Max(
                0f,
                _profile.Schedule.MaximumEnemySpeedBonus);
            serializedSchedule.FindProperty("_enemySpeedIncrease").floatValue = Mathf.Max(
                0f,
                _profile.Schedule.EnemySpeedIncrease);
            serializedSchedule.FindProperty("_maximumConcurrentEnemyBonus").intValue = Mathf.Max(
                0,
                _profile.Schedule.MaximumConcurrentEnemyBonus);
            serializedSchedule.FindProperty("_concurrentEnemyBonus").intValue = Mathf.Max(
                0,
                _profile.Schedule.ConcurrentEnemyBonus);
            serializedSchedule.FindProperty("_baseHealthLossMultiplier").floatValue = Mathf.Max(
                0.01f,
                _profile.Schedule.BaseHealthLossMultiplier);
            serializedSchedule.FindProperty("_healthLossMultiplierIncreasePerLevel").floatValue = Mathf.Max(
                0f,
                _profile.Schedule.HealthLossMultiplierIncreasePerLevel);
            serializedSchedule.FindProperty("_maximumHealthLossMultiplier").floatValue = Mathf.Max(
                0.01f,
                _profile.Schedule.BaseHealthLossMultiplier,
                _profile.Schedule.MaximumHealthLossMultiplier);
            serializedSchedule.FindProperty("_expectedPlayerPowerCurve").objectReferenceValue = curve;
            serializedSchedule.ApplyModifiedProperties();
            EditorUtility.SetDirty(schedule);
        }

        private void WriteWave(LocalWave localWave)
        {
            SpawnPatternDefinition wave = LoadAsset<SpawnPatternDefinition>(localWave.AssetPath);
            Undo.RecordObject(wave, "Apply Local Infinite Balance");
            SerializedObject serializedWave = new SerializedObject(wave);
            serializedWave.FindProperty("_minimumDifficultyLevel").intValue = Mathf.Max(
                MinimumDifficultyLevel,
                localWave.MinimumDifficultyLevel);
            serializedWave.FindProperty("_cooldownDistanceMeters").floatValue = Mathf.Max(
                1f,
                localWave.CooldownDistanceMeters);
            serializedWave.FindProperty("_maximumConcurrentEnemies").intValue = Mathf.Max(
                1,
                localWave.MaximumConcurrentEnemies);
            serializedWave.FindProperty("_minimumEnemyCount").intValue = Mathf.Max(1, localWave.MinimumEnemyCount);
            serializedWave.FindProperty("_maximumEnemyCount").intValue = Mathf.Max(
                localWave.MinimumEnemyCount,
                localWave.MaximumEnemyCount);
            serializedWave.FindProperty("_enemySpeed").floatValue = Mathf.Max(0f, localWave.EnemySpeed);

            SerializedProperty archetypes = serializedWave.FindProperty("_enemyArchetypes");
            archetypes.arraySize = localWave.EnemyArchetypePaths.Count;
            for (int index = 0; index < localWave.EnemyArchetypePaths.Count; index++)
            {
                archetypes.GetArrayElementAtIndex(index).objectReferenceValue =
                    LoadAsset<EnemyArchetypeDefinition>(localWave.EnemyArchetypePaths[index]);
            }

            serializedWave.ApplyModifiedProperties();
            EditorUtility.SetDirty(wave);
        }

        private void WriteEnemy(LocalEnemy localEnemy)
        {
            EnemyArchetypeDefinition enemy = LoadAsset<EnemyArchetypeDefinition>(localEnemy.AssetPath);
            Undo.RecordObject(enemy, "Apply Local Infinite Balance");
            SerializedObject serializedEnemy = new SerializedObject(enemy);
            serializedEnemy.FindProperty("_maximumHealth").intValue = Mathf.Max(1, localEnemy.MaximumHealth);
            serializedEnemy.FindProperty("_contactDamage").intValue = Mathf.Max(1, localEnemy.ContactDamage);
            serializedEnemy.FindProperty("_speedMultiplier").floatValue = Mathf.Max(0.1f, localEnemy.SpeedMultiplier);
            serializedEnemy.ApplyModifiedProperties();
            EditorUtility.SetDirty(enemy);
        }

        private int GetNextMilestoneLevel()
        {
            int highestLevel = MinimumDifficultyLevel;
            for (int index = 0; index < _profile.PowerCurve.Milestones.Count; index++)
            {
                highestLevel = Mathf.Max(highestLevel, _profile.PowerCurve.Milestones[index].DifficultyLevel);
            }

            return highestLevel + 1;
        }

        private void DrawLevelCurveChart(
            string title,
            string legend,
            Color lineColor,
            Func<int, float> evaluate,
            bool drawMilestones)
        {
            Rect chartRect = GUILayoutUtility.GetRect(1f, ChartHeight, GUILayout.ExpandWidth(true));
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            int maximumLevel = Mathf.Max(MinimumDifficultyLevel + 1, _chartMaximumLevel);
            int sampleCount = Mathf.Min(128, maximumLevel);
            float maximumValue = 0f;
            for (int sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
            {
                int level = GetChartLevel(sampleIndex, sampleCount, maximumLevel);
                maximumValue = Mathf.Max(maximumValue, evaluate(level));
            }

            float verticalMaximum = GetGraphVerticalMaximum(maximumValue);
            Rect plotRect = new Rect(
                chartRect.x + 48f,
                chartRect.y + 24f,
                Mathf.Max(1f, chartRect.width - 62f),
                Mathf.Max(1f, chartRect.height - 50f));

            EditorGUI.DrawRect(chartRect, new Color(0.12f, 0.14f, 0.17f));
            EditorGUI.DrawRect(plotRect, new Color(0.08f, 0.1f, 0.12f));
            GUI.Label(new Rect(chartRect.x + 8f, chartRect.y + 4f, chartRect.width - 16f, 18f), title, EditorStyles.boldLabel);
            GUI.Label(new Rect(chartRect.x + 8f, chartRect.yMax - 18f, chartRect.width - 16f, 16f), legend, EditorStyles.miniLabel);

            Handles.BeginGUI();
            Color previousColor = Handles.color;
            Handles.color = new Color(1f, 1f, 1f, 0.14f);

            for (int gridIndex = 0; gridIndex <= ChartGridLineCount; gridIndex++)
            {
                float progress = gridIndex / (float)ChartGridLineCount;
                float y = Mathf.Lerp(plotRect.yMax, plotRect.y, progress);
                float x = Mathf.Lerp(plotRect.x, plotRect.xMax, progress);
                Handles.DrawLine(new Vector3(plotRect.x, y), new Vector3(plotRect.xMax, y));
                Handles.DrawLine(new Vector3(x, plotRect.y), new Vector3(x, plotRect.yMax));
            }

            Vector3[] points = new Vector3[sampleCount];
            for (int sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
            {
                int level = GetChartLevel(sampleIndex, sampleCount, maximumLevel);
                float xProgress = (level - MinimumDifficultyLevel) / (float)(maximumLevel - MinimumDifficultyLevel);
                float yProgress = Mathf.Clamp01(evaluate(level) / verticalMaximum);
                points[sampleIndex] = new Vector3(
                    Mathf.Lerp(plotRect.x, plotRect.xMax, xProgress),
                    Mathf.Lerp(plotRect.yMax, plotRect.y, yProgress));
            }

            Handles.color = lineColor;
            Handles.DrawAAPolyLine(2.5f, points);
            Handles.color = previousColor;
            Handles.EndGUI();

            for (int gridIndex = 0; gridIndex <= ChartGridLineCount; gridIndex++)
            {
                float progress = gridIndex / (float)ChartGridLineCount;
                float value = verticalMaximum * progress;
                float y = Mathf.Lerp(plotRect.yMax, plotRect.y, progress) - 8f;
                GUI.Label(new Rect(chartRect.x, y, 42f, 16f), FormatGraphValue(value), EditorStyles.miniLabel);

                int level = Mathf.RoundToInt(Mathf.Lerp(MinimumDifficultyLevel, maximumLevel, progress));
                float x = Mathf.Lerp(plotRect.x, plotRect.xMax, progress) - 12f;
                GUI.Label(new Rect(x, plotRect.yMax + 2f, 32f, 16f), level.ToString(), EditorStyles.miniLabel);
            }

            if (!drawMilestones)
            {
                return;
            }

            for (int index = 0; index < _profile.PowerCurve.Milestones.Count; index++)
            {
                LocalPowerMilestone milestone = _profile.PowerCurve.Milestones[index];
                if (milestone.DifficultyLevel > maximumLevel)
                {
                    continue;
                }

                float xProgress = (milestone.DifficultyLevel - MinimumDifficultyLevel)
                    / (float)(maximumLevel - MinimumDifficultyLevel);
                float yProgress = Mathf.Clamp01(milestone.ExpectedDps / verticalMaximum);
                float x = Mathf.Lerp(plotRect.x, plotRect.xMax, xProgress);
                float y = Mathf.Lerp(plotRect.yMax, plotRect.y, yProgress);
                EditorGUI.DrawRect(new Rect(x - 3f, y - 3f, 6f, 6f), Color.white);
            }
        }

        private static int GetChartLevel(int sampleIndex, int sampleCount, int maximumLevel)
        {
            if (sampleCount <= 1)
            {
                return MinimumDifficultyLevel;
            }

            float progress = sampleIndex / (float)(sampleCount - 1);
            return Mathf.RoundToInt(Mathf.Lerp(MinimumDifficultyLevel, maximumLevel, progress));
        }

        private int GetExpectedEnemyHealth(LocalEnemy enemy, int difficultyLevel)
        {
            float baseDps = Mathf.Max(0.01f, EvaluateExpectedDps(MinimumDifficultyLevel));
            float healthMultiplier = Mathf.Max(1f, EvaluateExpectedDps(difficultyLevel) / baseDps);
            return Mathf.CeilToInt(enemy.MaximumHealth * healthMultiplier);
        }

        private static float GetGraphVerticalMaximum(float maximumValue)
        {
            if (maximumValue <= Mathf.Epsilon)
            {
                return 1f;
            }

            float magnitude = Mathf.Pow(10f, Mathf.Floor(Mathf.Log10(maximumValue)));
            return Mathf.Ceil(maximumValue / magnitude) * magnitude;
        }

        private static string FormatGraphValue(float value)
        {
            return value >= 10f ? value.ToString("0") : value.ToString("0.0");
        }

        private float EvaluateExpectedDps(int difficultyLevel)
        {
            if (_profile.PowerCurve.Milestones.Count == 0)
            {
                return 1f;
            }

            int targetLevel = Mathf.Max(MinimumDifficultyLevel, difficultyLevel);
            LocalPowerMilestone lower = new LocalPowerMilestone();
            LocalPowerMilestone upper = new LocalPowerMilestone();
            bool hasLower = false;
            bool hasUpper = false;

            for (int index = 0; index < _profile.PowerCurve.Milestones.Count; index++)
            {
                LocalPowerMilestone milestone = _profile.PowerCurve.Milestones[index];
                if (milestone.DifficultyLevel <= targetLevel
                    && (!hasLower || milestone.DifficultyLevel > lower.DifficultyLevel))
                {
                    lower = milestone;
                    hasLower = true;
                }

                if (milestone.DifficultyLevel > targetLevel
                    && (!hasUpper || milestone.DifficultyLevel < upper.DifficultyLevel))
                {
                    upper = milestone;
                    hasUpper = true;
                }
            }

            if (!hasLower)
            {
                return upper.ExpectedDps;
            }

            if (!hasUpper)
            {
                return lower.ExpectedDps +
                    ((targetLevel - lower.DifficultyLevel) * _profile.PowerCurve.DpsIncreasePerLevelAfterLastMilestone);
            }

            float progress = Mathf.InverseLerp(lower.DifficultyLevel, upper.DifficultyLevel, targetLevel);
            return Mathf.Lerp(lower.ExpectedDps, upper.ExpectedDps, progress);
        }

        private float EvaluateHealthLossMultiplier(int difficultyLevel)
        {
            int levelStep = Mathf.Max(0, difficultyLevel - MinimumDifficultyLevel);
            return Mathf.Min(
                _profile.Schedule.MaximumHealthLossMultiplier,
                _profile.Schedule.BaseHealthLossMultiplier +
                (_profile.Schedule.HealthLossMultiplierIncreasePerLevel * levelStep));
        }

        private int GetAvailableWaveCount(int difficultyLevel)
        {
            int count = 0;
            for (int index = 0; index < _profile.Waves.Count; index++)
            {
                if (_profile.Waves[index].MinimumDifficultyLevel <= difficultyLevel)
                {
                    count++;
                }
            }

            return count;
        }

        private void EnsureProfileLists()
        {
            _profile ??= new LocalBalanceProfile();
            _profile.PowerCurve ??= new LocalPowerCurve();
            _profile.Schedule ??= new LocalSchedule();
            _profile.PowerCurve.Milestones ??= new List<LocalPowerMilestone>();
            _profile.Waves ??= new List<LocalWave>();
            _profile.Enemies ??= new List<LocalEnemy>();

            if (_profile.Schedule.BaseHealthLossMultiplier <= 0f)
            {
                _profile.Schedule.BaseHealthLossMultiplier = 0.3f;
            }

            if (_profile.Schedule.MaximumHealthLossMultiplier <= 0f)
            {
                _profile.Schedule.MaximumHealthLossMultiplier = 0.75f;
            }

            for (int index = 0; index < _profile.Waves.Count; index++)
            {
                LocalWave wave = _profile.Waves[index];
                wave.EnemyArchetypePaths ??= new List<string>();
                _profile.Waves[index] = wave;
            }
        }

        private static void DrawAssetLabel<T>(string label, string assetPath) where T : UnityEngine.Object
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(label, LoadAsset<T>(assetPath), typeof(T), false);
            EditorGUI.EndDisabledGroup();
        }

        private static T FindFirstAsset<T>() where T : UnityEngine.Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids.Length == 0)
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static T LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            return string.IsNullOrEmpty(assetPath)
                ? null
                : AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        private static string GetAssetName(string assetPath)
        {
            return string.IsNullOrEmpty(assetPath)
                ? "참조 없음"
                : Path.GetFileNameWithoutExtension(assetPath);
        }

        private static string ProfilePath => Path.GetFullPath(Path.Combine(
            Application.dataPath,
            "..",
            ProfileDirectoryName,
            ProfileFileName));

        [Serializable]
        private sealed class LocalBalanceProfile
        {
            public LocalPowerCurve PowerCurve = new LocalPowerCurve();
            public LocalSchedule Schedule = new LocalSchedule();
            public List<LocalWave> Waves = new List<LocalWave>();
            public List<LocalEnemy> Enemies = new List<LocalEnemy>();
        }

        [Serializable]
        private sealed class LocalPowerCurve
        {
            public string AssetPath;
            public float DpsIncreasePerLevelAfterLastMilestone;
            public List<LocalPowerMilestone> Milestones = new List<LocalPowerMilestone>();
        }

        [Serializable]
        private struct LocalPowerMilestone
        {
            public int DifficultyLevel;
            public float ExpectedDps;
        }

        [Serializable]
        private sealed class LocalSchedule
        {
            public string AssetPath;
            public float MetersPerDifficultyLevel;
            public float MinimumCooldownDistanceMeters;
            public float CooldownReductionMeters;
            public float MaximumEnemySpeedBonus;
            public float EnemySpeedIncrease;
            public int MaximumConcurrentEnemyBonus;
            public int ConcurrentEnemyBonus;
            public float BaseHealthLossMultiplier;
            public float HealthLossMultiplierIncreasePerLevel;
            public float MaximumHealthLossMultiplier;
        }

        [Serializable]
        private struct LocalWave
        {
            public string AssetPath;
            public int MinimumDifficultyLevel;
            public float CooldownDistanceMeters;
            public int MaximumConcurrentEnemies;
            public int MinimumEnemyCount;
            public int MaximumEnemyCount;
            public float EnemySpeed;
            public List<string> EnemyArchetypePaths;
        }

        [Serializable]
        private struct LocalEnemy
        {
            public string AssetPath;
            public int MaximumHealth;
            public int ContactDamage;
            public float SpeedMultiplier;
        }
    }
}

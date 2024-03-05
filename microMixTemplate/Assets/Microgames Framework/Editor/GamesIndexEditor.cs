using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.IO;

[CustomEditor(typeof(GamesIndex))]
public class GamesIndexEditor : Editor {

    static readonly string IMPORT_UNDO_NAME = "Import MicroGames";

    static readonly Regex _teamFolderMatch = new Regex(@"Team[^\d\\/]*(\d+)[^\d\\/]*$", RegexOptions.Compiled);
    static string _importInfo;

    #pragma warning disable CS0414 // Turn off warning in Monday version.
    static EditorCoroutine _importInProgress;
    #pragma warning restore CS0414
    public override void OnInspectorGUI()
    {
        var index = (GamesIndex)target;

        if (GUILayout.Button("Refresh Local Games"))
            RefreshLocal(index);

        /*
        if (_importInProgress == null) {
            if (GUILayout.Button("Import Games")) {
                _importInProgress = EditorCoroutineUtility.StartCoroutine(ImportCoroutine(index), this);
            }
        } else {
            if (GUILayout.Button($"Importing {_importInfo} (Abort)")) {
                EditorCoroutineUtility.StopCoroutine(_importInProgress);
                _importInProgress = null;
            }
        }  
        */      

        DrawDefaultInspector();
    }

    void RefreshLocal(GamesIndex index) {
        var infos = DataHelper.GetAllAssetsOfType<GameInfo>();
        DataHelper.MarkChangesForSaving(index, "Refresh Local Games");

        index.ReplaceGames(infos);

        var buildScenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(MicrogamesManager.FrameworkScenePath, true)
        };

        var scenes = new HashSet<SceneAsset>();
        foreach(var info in infos) {
            if (!info.includeInRandomizer) continue;
            if (info.singlePlayerScene != null && !scenes.Contains(info.singlePlayerScene)) {
                scenes.Add(info.singlePlayerScene);
                buildScenes.Add(new EditorBuildSettingsScene(info.singlePlayerScenePath, true));
            }
            if (info.twoPlayerScene != null && !scenes.Contains(info.twoPlayerScene)) {
                scenes.Add(info.twoPlayerScene);
                buildScenes.Add(new EditorBuildSettingsScene(info.twoPlayerScenePath, true));
            }
        }

        EditorBuildSettings.scenes = buildScenes.ToArray();
    }

    IEnumerator ImportCoroutine(GamesIndex index) {
        if (!IsSetupValid(index, true)) yield break;

        
        if (CloseImportedScenes(index.destinationPath))
            yield return null;

        var buildScenes = EraseOldVersions(index);
        int frameworkSceneCount = buildScenes.Count;

        // Copy data from all matching project folders into destination path.
        Dictionary<int, string> importMap = new();
        var projects = Directory.GetDirectories(index.sourcePath);
        
        int copied = 0;
        foreach(var projectPath in projects) {   
            if (ValidateTeamFolder(projectPath, out int teamNumber)) {
                yield return null;
                if (TryCopy(projectPath, index.destinationPath, teamNumber, importMap)) {
                    copied++;
                    _importInfo = $"{copied}/{projects.Length}";        
                }    
            }      
        }
        _importInfo = "refresh";
        yield return null;

        AssetDatabase.Refresh();

        _importInfo = "validate";
        yield return null;
        
        DataHelper.MarkChangesForSaving(index, IMPORT_UNDO_NAME);      
        var gameInfos = new List<GameInfo>();  
        int successfulTeamCount = ExtractTeamGames(importMap, gameInfos, buildScenes);
        EditorBuildSettings.scenes = buildScenes.ToArray();
        index.ReplaceGames(gameInfos);        

        Debug.Log($"Successfully imported {successfulTeamCount} / {importMap.Count} projects: {gameInfos.Count} MicroGames in {buildScenes.Count - frameworkSceneCount} scenes.");
        _importInProgress = null;
    }

    bool IsSetupValid(GamesIndex index, bool readingSource) {       

        if (string.IsNullOrEmpty(index.destinationPath)) {
            Debug.LogError("No import destination path specified.");
            return false;
        }

        if (!Directory.Exists($"{Application.dataPath}/{index.destinationPath}")) {
            Debug.LogError("The specified import destination path does not exist.");
            return false;
        }

        if (!readingSource) return true;

         if (string.IsNullOrEmpty(index.sourcePath)) {
            Debug.LogError("No import source path specified.");
            return false;
        }

        if (!Directory.Exists(index.sourcePath)) {
            Debug.LogError("The specified import source path does not exist.");
            return false;
        }
        
        return true;
    }

    bool CloseImportedScenes(string importedPath) {
        _importInfo = "close";

        bool changedScenes = false;
        int sceneCount = SceneManager.sceneCount;

        importedPath = $"Assets/{importedPath}";
        
        for(int i = 0; i < sceneCount; i++) {
            var scene = SceneManager.GetSceneAt(i);
            if (!scene.path.StartsWith(importedPath)) continue;

            if (sceneCount == 1) {
                EditorSceneManager.OpenScene(MicrogamesManager.FrameworkScenePath, OpenSceneMode.Single);
                return true;
            } else {
                EditorSceneManager.CloseScene(scene, true);
                i--;
                changedScenes = true;
                sceneCount--;
            }            
        }
        return changedScenes;
    }

    List<EditorBuildSettingsScene> EraseOldVersions(GamesIndex index) {    
        _importInfo = "erase";

        string destPath = $"{Application.dataPath}/{index.destinationPath}";
        Debug.Log($"Clearing old versions from {destPath}");

        var toKeep = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(MicrogamesManager.FrameworkScenePath, true)
        };

        EditorBuildSettings.scenes = toKeep.ToArray();

        DataHelper.MarkChangesForSaving(index, "Clear Previous Teams");       
        index.ReplaceGames(null);   
        
        DirectoryInfo di = new DirectoryInfo(destPath);
        foreach (FileInfo file in di.GetFiles()) {
            file.Delete(); 
        }
        foreach (DirectoryInfo dir in di.GetDirectories()) {
            dir.Delete(true); 
        }

        AssetDatabase.Refresh();
        return toKeep;
    }

    bool ValidateTeamFolder(string projectPath, out int teamNumber) {
        // Validate that project folder matches team naming convention, 
        // and extract team number.
        var match = _teamFolderMatch.Match(projectPath);
        teamNumber = int.MinValue;
        if (match.Success && int.TryParse(match.Groups[1].Value, out teamNumber)) {
            return true;
        }
        Debug.LogWarning($"Project folder does not match Team## naming convention: {projectPath} - {match.Success}");
        return false;
    }


    bool TryCopy(string projectPath, string destinationPath, int teamNumber, Dictionary<int, string> importMap) {
        if(importMap.ContainsKey(teamNumber)) {
            Debug.LogError($"Multiple project folders found that map to the same team number {teamNumber} - only the first will be imported.");
            return false;
        }

        string sourcePath = Path.Combine(projectPath, "Assets", "MicroGames");
        if (!Directory.Exists(sourcePath)){
            Debug.LogError($"Team {teamNumber}'s project does not contain an Assets/MicroGames folder.");
            return false;
        }

        try {
            string suffix =  $"{destinationPath}/Team{teamNumber}";
            string destPath = $"{Application.dataPath}/{suffix}";
            var created = CopyRecursively(sourcePath, destPath);
            importMap.Add(teamNumber, $"Assets/{suffix}");
            return true;                     
        } catch (System.Exception e) {
            Debug.LogError($"Error while copying files from {projectPath}\n{e.Message}");
        }

        return false;
    }

    static DirectoryInfo CopyRecursively(string sourceDir, string destinationDir)
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Cache directories before we start copying
        var subdirectories = dir.GetDirectories();

        // Create the destination directory
        var created = Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        foreach (DirectoryInfo subDir in subdirectories)
        {
            string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyRecursively(subDir.FullName, newDestinationDir);
        }

        return created;
    }

    int ExtractTeamGames(Dictionary<int, string> importMap, List<GameInfo> gameInfos, List<EditorBuildSettingsScene> buildSettingsScenes) {
        
        var scenesToBuild = new List<SceneAsset>();
        int successfulTeams = 0;
        foreach(var pair in importMap) {
            
            int teamNumber = pair.Key;
            string expectedPath = pair.Value;

            int importedForTeam = 0;

            var teamGames = DataHelper.GetAllAssetsOfType<GameInfo>(expectedPath);

            foreach(var game in teamGames) {
                DataHelper.MarkChangesForSaving(game, IMPORT_UNDO_NAME);
                game.teamNumber = teamNumber;

                if (!game.includeInRandomizer) continue;
                if (ValidateMicroGame(game, scenesToBuild)) {
                    gameInfos.Add(game);
                    importedForTeam++;
                }
            }
            
            if (importedForTeam == 0) {
                Debug.LogError($"No valid MicroGames found for Team {teamNumber}");
            } else {
                successfulTeams++;
                Debug.Log($"Imported {importedForTeam}/{teamGames.Count} MicroGames for Team {teamNumber}");
            }
        }

        foreach(var scene in scenesToBuild) {
            buildSettingsScenes.Add(new EditorBuildSettingsScene(
                AssetDatabase.GetAssetPath(scene), true)
            );
        }

        return successfulTeams;
    }

    bool ValidateMicroGame(GameInfo game, List<SceneAsset> scenesToBuild) {
        // TODO: Validate display name length, allowable characters, bad words?
        if (string.IsNullOrWhiteSpace(game.gameTitle)) {
            Debug.LogError($"A MicroGame by Team {game.teamNumber} is missing a valid display name.");
            return false;
        }
        int scenesFound = 0;
        if (game.singlePlayerScene != null) {
            if (scenesToBuild.Contains(game.singlePlayerScene)) {
                Debug.LogError($"Multiple MicroGames by Team {game.teamNumber} share a reference to scene {game.singlePlayerScene.name}. Only the first will be used.");
                return false;
            }
            scenesToBuild.Add(game.singlePlayerScene);
            scenesFound = 1;
        }
        if (game.twoPlayerScene != null && game.twoPlayerScene != game.singlePlayerScene) {
            if (scenesToBuild.Contains(game.twoPlayerScene)) {
                Debug.LogError($"Multiple MicroGames by Team {game.teamNumber} share a reference to scene {game.twoPlayerScene.name}. Only the first will be used.");
                return false;
            }
            scenesToBuild.Add(game.twoPlayerScene);
            scenesFound++;
        }

        if (scenesFound == 0) {
            Debug.LogError($"MicroGame {game.gameTitle} by Team {game.teamNumber} does not have any valid scenes assigned.");
            return false;
        }
        
        // TODO: Check other elements like attract mode image.
        
        return true;
    }

}

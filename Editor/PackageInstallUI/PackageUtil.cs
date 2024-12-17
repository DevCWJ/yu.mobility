#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace CWJ.Editor
{
    using static PackageDefine;
    public static class PackageUtil
    {
        [SerializeField]
        private static string _PackageRelativePath;

        /// <summary>
        /// Returns the fully qualified path of the package.
        /// </summary>
        public static string PackageFullPath
        {
            get
            {
                if (string.IsNullOrEmpty(_PackageFullPath_))
                    _PackageFullPath_ = GetPackageFullPath();

                return _PackageFullPath_;
            }
        }

        /// <summary>
        /// Returns the relative path of the package.
        /// </summary>
        public static string PackageRelativePath
        {
            get
            {
                if (string.IsNullOrEmpty(_PackageRelativePath))
                    _PackageRelativePath = GetPackageRelativePath();

                return _PackageRelativePath;
            }
        }

        [SerializeField]
        private static string _PackageFullPath_;

        private static string GetPackageFullPath()
        {
            //UPM 패키지 확인
            string packagePath = Path.GetFullPath($"Packages/{MyPackageName}");
            if (Directory.Exists(packagePath))
            {
                return packagePath;
            }

            packagePath = Path.GetFullPath("Assets/..");
            if (Directory.Exists(packagePath))
            {
                // 패키지의 기본 위치 검색
                if (Directory.Exists(packagePath + $"/Assets/{MyPackageInAssetName}/{UnityPackageFolderName}"))
                {
                    return packagePath + $"/Assets/{MyPackageInAssetName}";
                }

                // 사용자 프로젝트에서 대체 위치 검색
                string[] matchingPaths = Directory.GetDirectories(packagePath, MyPackageInAssetName, SearchOption.AllDirectories);
                string path = ValidateLocation(matchingPaths, packagePath);
                if (path != null) return packagePath + path;
            }

            return null;
        }

        private static string GetPackageRelativePath()
        {
            //UPM 패키지 확인
            string packagePath = Path.GetFullPath($"Packages/{MyPackageName}");
            if (Directory.Exists(packagePath))
            {
                return $"Packages/{MyPackageName}";
            }

            packagePath = Path.GetFullPath("Assets/..");
            if (Directory.Exists(packagePath))
            {
                // 패키지의 기본 위치 검색
                if (Directory.Exists(packagePath + $"/Assets/{MyPackageInAssetName}/{UnityPackageFolderName}"))
                {
                    return $"Assets/{MyPackageInAssetName}";
                }

                //사용자 프로젝트에서 대체 위치 검색
                string[] matchingPaths = Directory.GetDirectories(packagePath, MyPackageInAssetName, SearchOption.AllDirectories);
                packagePath = ValidateLocation(matchingPaths, packagePath);
                if (packagePath != null) return packagePath;
            }

            return null;
        }



        /// <summary>
        /// Method to validate the location of the asset folder by making sure the GUISkins folder exists.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        private static string ValidateLocation(string[] paths, string projectPath)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                // Check if any of the matching directories contain a GUISkins directory.
                if (Directory.Exists(paths[i] + $"/{UnityPackageFolderName}"))
                {
                    string folderPath = paths[i].Replace(projectPath, "");
                    folderPath = folderPath.TrimStart('\\', '/');
                    return folderPath;
                }
            }

            return null;
        }


        public static void ImportSamplesToPath(PackageInfo curPackageInfo, string downloadFolderPath)
        {
            string packagePath = curPackageInfo.GetPackagePath();
            if (string.IsNullOrEmpty(packagePath))
            {
                Debug.LogError("패키지 경로를 찾을 수 없습니다.");
                return;
            }

            string samplesPath = Path.Combine(packagePath, "Samples~");
            if (!Directory.Exists(samplesPath))
            {
                Debug.LogError("Samples~ 폴더를 찾을 수 없습니다.");
                return;
            }

            if (!Directory.Exists(downloadFolderPath))
            {
                Directory.CreateDirectory(downloadFolderPath);
            }

            foreach (string dirPath in Directory.GetDirectories(samplesPath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(samplesPath, downloadFolderPath));
            }

            foreach (string newPath in Directory.GetFiles(samplesPath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(samplesPath, downloadFolderPath), true);
            }

            AssetDatabase.Refresh();
            Debug.Log("Samples~ 폴더의 샘플이 프로젝트로 복사되었습니다: " + downloadFolderPath);
        }

        public static string GetPackagePath(this PackageInfo packageInfo)
        {
            var request = UnityEditor.PackageManager.Client.List(true, false);
            while (!request.IsCompleted) { }


            packageInfo ??= request.Result.FirstOrDefault(p => p.name == MyPackageName);
            return packageInfo?.resolvedPath;
        }

        public static bool TrySelectDownloadPath(out string folderPath, string lastFilePath = null)
        {
            string defaultPath = string.IsNullOrEmpty(lastFilePath) ? Directory.GetParent(Application.dataPath)?.FullName : null;

            folderPath = EditorUtility.OpenFolderPanel("unitypackage 다운로드 받을 폴더 선택", defaultPath, "");

            return !string.IsNullOrEmpty(folderPath);
        }

        public static bool CheckNeedUpdateByLastUpdate(this PackageInfo packageInfo, out string latestVersion)
        {
            latestVersion = GetLatestGitTag();

            if (string.IsNullOrEmpty(latestVersion))
            {
                return true;
            }

            latestVersion = latestVersion.VersionNormalized()[1..];
            string currentVersion = packageInfo.version.VersionNormalized();
            return !string.IsNullOrEmpty(latestVersion) && currentVersion != latestVersion;
        }

        private static string VersionNormalized(this string input)
        {
            return input
                   .Trim()
                   .Replace("\n", string.Empty) // 줄바꿈 제거
                   .Replace("\r", string.Empty) // 캐리지 리턴 제거
                   .Replace("\t", string.Empty); // 탭 제거
        }
        private static string GetLatestGitTag()
        {
            string latestTag = null;
            try
            {
                System.Diagnostics.ProcessStartInfo startInfo = new()
                                                                {
                                                                    FileName = "git",
                                                                    Arguments = $"ls-remote --tags {MyGitRepoUrl}",
                                                                    RedirectStandardOutput = true,
                                                                    UseShellExecute = false,
                                                                    CreateNoWindow = true
                                                                }; // Git 명령 실행

                using var process = System.Diagnostics.Process.Start(startInfo);
                if (process != null)
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    // Git 태그 목록에서 최신 태그 추출
                    string[] lines = output.Split('\n');
                    latestTag = lines
                                .Where(line => line.Contains("refs/tags/"))
                                .Select(line => line.Split('/').Last())
                                .OrderByDescending(tag => tag)
                                .FirstOrDefault();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Git 태그를 가져오는 중 오류 발생: " + ex.Message);
                latestTag = null;
            }

            return latestTag;
        }

        public static void ExportPackageFromLibrary(string srcFolderRootInPackagePath = RuntimeIgnoreFolderName)
        {
            if (srcFolderRootInPackagePath == null) srcFolderRootInPackagePath = RuntimeIgnoreFolderName;

            // Library/com.cwj.yu.mobility/Runtime~
            string downloadSrcFolderRoot = Path.Combine(PackageRelativePath, srcFolderRootInPackagePath);
            if (!Directory.Exists(downloadSrcFolderRoot))
            {
                Debug.LogError($"Source folder not found: {downloadSrcFolderRoot}");
                return;
            }

            string[] subDirectories = Directory.GetDirectories(downloadSrcFolderRoot);
            if (subDirectories.Length == 0)
            {
                Debug.LogError("No subdirectories found in the Runtime~ folder.");
                return;
            }

            string targetFolder = null;
            try
            {
                EditorApplication.LockReloadAssemblies();

                string sourceFolder = subDirectories[0]; // 복사 대상 폴더 (첫 번째 폴더)

                // Assets/CWJ.UnityPackages/temp_랜덤숫자 폴더 생성
                string randomFolderName = $"temp_{UnityEngine.Random.Range(1000, 9999)}";
                targetFolder = Path.Combine(RuntimeDownloadAssetsPath, randomFolderName);

                if (!Directory.Exists(RuntimeDownloadAssetsPath))
                    Directory.CreateDirectory(RuntimeDownloadAssetsPath);

                CopyDirectory(sourceFolder, targetFolder);
                AssetDatabase.Refresh();

                // .unitypackage 파일 경로 설정
                string folderName = Path.GetFileName(sourceFolder); // 폴더 이름 추출
                string unitypackageFilePath = Path.Combine(RuntimeDownloadAssetsPath, $"{MyPackageInAssetName}.{folderName}.unitypackage");
                Debug.Log($"Exporting: {sourceFolder} -> {unitypackageFilePath}");

                // 패키지 내보내기
                AssetDatabase.ExportPackage(targetFolder, unitypackageFilePath, ExportPackageOptions.Recurse);
                Debug.Log($"Exported folder to UnityPackage: {unitypackageFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error exporting folder: {ex}");
            }
            finally
            {
                // 리로드 잠금 해제
                // 정리: 임시 폴더 삭제
                if (targetFolder != null && Directory.Exists(targetFolder))
                {
                    Directory.Delete(targetFolder, true);
                    File.Delete(targetFolder + ".meta");
                }

                EditorApplication.UnlockReloadAssemblies();
                AssetDatabase.Refresh();
            }
        }

        private static void CopyDirectory(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            // 하위 파일 복사
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            // 하위 폴더 복사 (재귀)
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(targetDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }

        // private static void ExportSamplesPath(PackageInfo packageInfo, string exportTargetPath, string moveToPath)
        // {
        //     string packagePath = GetPackagePath(packageInfo);
        //     if (string.IsNullOrEmpty(packagePath))
        //     {
        //         Debug.LogError("패키지 경로를 찾을 수 없습니다.");
        //         return;
        //     }
        //
        //     string samplesPath = Path.Combine(packagePath, "Samples~");
        //     if (!Directory.Exists(samplesPath))
        //     {
        //         Debug.LogError("Samples~ 폴더를 찾을 수 없습니다.");
        //         return;
        //     }
        //
        //     if (!Directory.Exists(exportTargetPath))
        //     {
        //         return;
        //     }
        //     //
        //
        //     string[] samples = Directory.GetDirectories(exportTargetPath);
        //     foreach (string samplePath in samples)
        //     {
        //         string sampleName = Path.GetFileName(samplePath);
        //         string packagePath = Path.Combine(moveToPath, $"{sampleName}.unitypackage");
        //
        //         Debug.Log($"Exporting {sampleName} to {packagePath}");
        //
        //         AssetDatabase.ExportPackage(samplePath, packagePath, ExportPackageOptions.Default);
        //     }
        // }

    }
}

#endif

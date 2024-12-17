#if UNITY_EDITOR
namespace CWJ.Editor
{
	public static class PackageDefine
	{
		public const string MyPackageName = "com.cwj.yu.mobility";
		public const string MyPackageInAssetName = "CWJ.YU.Mobility";
		public const string MyGitRepoUrl = "https://github.com/DevCWJ/cwj.yu.mobility.git";
		public const string TitleStr = "[ RIS 영남대 스마트 모빌리티 패키지 ]";

		public const string UnityPackageFolderName = "UnityPackages~";
		public const string ThirdPartyPackageFileName = "ThirdPartyPackage.unitypackage";
		public const string TmpEssentialPackageFileName = "TMP Essential Resources.unitypackage";
		public const string RuntimeIgnoreFolderName = "Runtime~";
		public const string RuntimeDownloadAssetsPath = "Assets/CWJ.UnityPackages";
	}
}

#endif

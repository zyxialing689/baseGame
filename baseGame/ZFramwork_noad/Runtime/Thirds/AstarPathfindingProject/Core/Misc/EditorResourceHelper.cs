namespace Pathfinding {
#if UNITY_EDITOR
	using UnityEditor;
	using UnityEngine;
	using System.Collections.Generic;

	/// <summary>Internal utility class for looking up editor resources</summary>
	public static class EditorResourceHelper {
		/// <summary>
		/// Path to the editor assets folder for the A* Pathfinding Project. If this path turns out to be incorrect, the script will try to find the correct path
		/// See: LoadStyles
		/// </summary>
		public static string editorAssets;

		static EditorResourceHelper () {
			// Look up editor assets directory when first accessed
			LocateEditorAssets();
		}

		static Material surfaceMat, lineMat;
		static Texture2D handlesAALineTex;
		public static Material GizmoSurfaceMaterial {
			get {
				if (!surfaceMat) surfaceMat = UnityEditor.AssetDatabase.LoadAssetAtPath(EditorResourceHelper.editorAssets + "/Materials/Navmesh.mat", typeof(Material)) as Material;
				return surfaceMat;
			}
		}

		public static Material GizmoLineMaterial {
			get {
				if (!lineMat) lineMat = UnityEditor.AssetDatabase.LoadAssetAtPath(EditorResourceHelper.editorAssets + "/Materials/NavmeshOutline.mat", typeof(Material)) as Material;
				return lineMat;
			}
		}

		public static Texture2D HandlesAALineTexture {
			get {
				if (!handlesAALineTex) handlesAALineTex = Resources.Load<Texture2D>("handles_aaline");
				return handlesAALineTex;
			}
		}

		/// <summary>Locates the editor assets folder in case the user has moved it</summary>
		public static bool LocateEditorAssets () {
			var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(EditorResourceHelper).Assembly);
			if (package != null) {
				editorAssets = package.assetPath + "/Editor/AStart/EditorAssets";
				if (System.IO.File.Exists(package.resolvedPath + "/Editor/AStart/EditorAssets/AstarEditorSkinLight.guiskin")) {
					return true;
				} else {
					Debug.LogError("Could not find editor assets folder in package at " + editorAssets + ". Is the package corrupt?");
					return false;
				}
			}

			editorAssets = "Assets/AstarPathfindingProject/Editor/AStart/EditorAssets";
			if (!System.IO.File.Exists(editorAssets + "/AstarEditorSkinLight.guiskin") && !System.IO.File.Exists(editorAssets + "/AstarEditorSkin.guiskin")) {
				//Initiate search

				var sdir = new System.IO.DirectoryInfo(Application.dataPath);

				var dirQueue = new Queue<System.IO.DirectoryInfo>();
				dirQueue.Enqueue(sdir);

				bool found = false;
				while (dirQueue.Count > 0) {
					System.IO.DirectoryInfo dir = dirQueue.Dequeue();
					if (System.IO.File.Exists(dir.FullName + "/AstarEditorSkinLight.guiskin") || System.IO.File.Exists(dir.FullName + "/AstarEditorSkin.guiskin")) {
						// Handle windows file paths
						string path = dir.FullName.Replace('\\', '/');
						found = true;
						// Remove data path from string to make it relative
			

						if (path.StartsWith("/")) {
							path = path.Remove(0, 1);
						}

						editorAssets = path;
						return true;
					}
					var dirs = dir.GetDirectories();
					for (int i = 0; i < dirs.Length; i++) {
						dirQueue.Enqueue(dirs[i]);
					}
				}

				if (!found) {
					Debug.LogWarning("Could not locate editor assets folder. Make sure you have imported the package correctly.\nA* Pathfinding Project");
					return false;
				}
			}
			return true;
		}
	}
#endif
}

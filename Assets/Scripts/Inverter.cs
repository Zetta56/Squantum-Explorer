using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used to invert an object's mesh such that it's visible from the inside
public class Inverter : MonoBehaviour
{
	void Start()
	{
		// Continue if the current object has a mesh
		MeshFilter filter = GetComponent<MeshFilter>();
		if (filter != null) {
			Mesh mesh = filter.mesh;
			// Reverse all submeshes' directions (normals) so that they face inward
			Vector3[] normals = mesh.normals;
			for(int i = 0; i < normals.Length; i++) {
				normals[i] = -normals[i];
			}
			mesh.normals = normals;
			// Reflects mesh triangles to preserve orientation after reversing normals
			for(int submesh = 0; submesh < mesh.subMeshCount; submesh++) {
				int[] triangles = mesh.GetTriangles(submesh);
				for(int j = 0; j < triangles.Length; j += 3) {
					int temp = triangles[j];
					triangles[j] = triangles[j + 1];
					triangles[j + 1] = temp;
				}
				mesh.SetTriangles(triangles, submesh);
			}
			Debug.Log("done");
		}	
	}
}

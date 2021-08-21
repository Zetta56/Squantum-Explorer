using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceFieldController : MonoBehaviour
{
    private MeshRenderer mesh;
    private Shader forceFieldShader;
    private Shader frozenFieldShader;


    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        forceFieldShader = Shader.Find("Shader Graphs/ForceField");
        frozenFieldShader = Shader.Find("Shader Graphs/FrozenField");
    }

    public void ToggleFrozen(bool frozen) {
        if(frozen) {
            mesh.material.shader = frozenFieldShader;
        } else {
            mesh.material.shader = forceFieldShader;
        }
    }
}

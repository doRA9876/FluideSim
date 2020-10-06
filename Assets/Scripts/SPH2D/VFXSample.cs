using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[ExecuteInEditMode]
[RequireComponent(typeof(VisualEffect))]
public class VFXSample : MonoBehaviour
{
  struct ThreadSize
  {
    public int x;
    public int y;
    public int z;
    public ThreadSize(uint x, uint y, uint z)
    {
      this.x = (int)x;
      this.y = (int)y;
      this.z = (int)z;
    }
  }

  [SerializeField] ComputeShader cs;
  [SerializeField] int width = 128;
  [SerializeField] int height = 128;

  VisualEffect vfx;
  RenderTexture positionMap;
  int kernel;
  ThreadSize threadSize;

  Vector3 initialPosition;
  Vector3 initialVector;

  void Start()
  {
    if (!SystemInfo.supportsComputeShaders)
    {
      return;
    }
    vfx = GetComponent<VisualEffect>();
    SetupAttributeMaps();
  }

  // Update is called once per frame
  void Update()
  {
    if (!SystemInfo.supportsComputeShaders)
    {
      return;
    }

    if (width != positionMap.width || height != positionMap.height)
    {
      SetupAttributeMaps();
    }
    UpdateAttributeMaps();
  }

  void SetupAttributeMaps()
  {
    positionMap = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
    positionMap.enableRandomWrite = true;
    positionMap.Create();

    initialPosition = 5.0f * Random.insideUnitSphere;
    initialVector = new Vector3(0.1f, 0.1f, 0.1f);

    kernel = cs.FindKernel("SampleCS");
    uint threadSizeX, threadSizeY, threadSizeZ;
    cs.GetKernelThreadGroupSizes(kernel, out threadSizeX, out threadSizeY, out threadSizeZ);
    threadSize = new ThreadSize(threadSizeX, threadSizeY, threadSizeZ);

    cs.SetVector("_Position", initialPosition);
    cs.SetVector("_Vector", initialVector);
    cs.SetTexture(kernel, "_PositionMap", positionMap);
    vfx.SetTexture("Position Map", positionMap);
  }

  void UpdateAttributeMaps()
  {
    cs.SetFloat("_Time", Time.time);
    cs.Dispatch(kernel, Mathf.CeilToInt(width / threadSize.x), Mathf.CeilToInt(width / threadSize.y), 1);
  }
}

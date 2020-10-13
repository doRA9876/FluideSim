using System.Runtime.InteropServices;
using System.IO;
using System.Xml.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class RenderVTU : MonoBehaviour
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

  ThreadSize threadSize;
  [SerializeField] ComputeShader cs_shader;
  [SerializeField] int FrameRate = 50;

  //VTU File
  string dataDirectory;
  int vtuNum = 0;

  // Simulation
  bool simulation = true;
  int particleNum = 0;
  Vector3[] positionArray;


  // Power Computing
  int kernel;
  private ComputeBuffer positionArrayBuffer;

  // Visual Effect Graphics
  VisualEffect vfx;
  RenderTexture positionMap;
  int mapWidth, mapHeight;


  void Awake()
  {
    Application.targetFrameRate = FrameRate;
  }

  void Start()
  {
    if (!SystemInfo.supportsComputeShaders)
    {
      return;
    }

    dataDirectory = Application.dataPath + "/../Data/particle-2D/";

    string firstFilePath = dataDirectory + "particle_" + string.Format("{0:0000}", vtuNum) + ".vtu";
    if (!File.Exists(firstFilePath))
    {
      simulation = false;
      return;
    }

    positionArray = LoadVTU(firstFilePath);
    particleNum = positionArray.Length;

    vfx = GetComponent<VisualEffect>();
    InitBuffers();
    SetupAttributeMaps();

    // Show particle number and position map size
    Debug.Log("particle number : " + particleNum);
    Debug.Log(string.Format("width : {0}, height : {1}\n", mapWidth, mapHeight));
  }

  void Update()
  {
    if (simulation)
    {
      string path = dataDirectory + "particle_" + string.Format("{0:0000}", vtuNum) + ".vtu";
      if (File.Exists(path))
      {
        vtuNum++;
        positionArray = LoadVTU(path);
        simulation = true;
      }
      else
      {
        simulation = false;
      }
      UpdateBuffers();
      UpdateAttributeMaps();
    }
    // Debug.Log(xmlNum);
  }

  void OnDestroy()
  {
    DeleteBuffer(positionArrayBuffer);
  }

  void SetupAttributeMaps()
  {
    // Set up kernel
    kernel = cs_shader.FindKernel("CSMain");
    uint threadSizeX, threadSizeY, threadSizeZ;
    cs_shader.GetKernelThreadGroupSizes(kernel, out threadSizeX, out threadSizeY, out threadSizeZ);
    threadSize = new ThreadSize(threadSizeX, threadSizeY, threadSizeZ);

    // Calculate position map size
    mapWidth = threadSize.x;
    mapHeight = particleNum / threadSize.x + 1;

    // Set up and Create position map
    positionMap = new RenderTexture(mapWidth, mapHeight, 0, RenderTextureFormat.ARGBFloat);
    positionMap.enableRandomWrite = true;
    positionMap.Create();

    // Set positon map to Compute shader and VFX
    cs_shader.SetTexture(kernel, "_PositionMap", positionMap);
    vfx.SetTexture("Position Map", positionMap);
  }

  void UpdateAttributeMaps()
  {
    cs_shader.SetBuffer(kernel, "_PositionArray", positionArrayBuffer);
    cs_shader.Dispatch(kernel, 1, 1, 1);
  }

  void InitBuffers()
  {
    positionArrayBuffer = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(Vector3)));
  }

<<<<<<< HEAD:Assets/Scripts/SPH2D/VFXdrawVTU.cs
  void UpdateBuffers()
  {
    positionArrayBuffer.SetData(positionArray);
  }

=======
>>>>>>> develop:Assets/Scripts/Rendering/RenderVTU.cs
  private Vector3[] LoadVTU(string filePath)
  {
    var vtu = XDocument.Load(filePath);

    XNamespace ns = "VTK";

    XElement vtk = vtu.Element(ns + "VTKFile");
    XElement grid = vtk.Element(ns + "UnstructuredGrid");
    XElement piece = grid.Element(ns + "Piece");
    XElement points = piece.Element(ns + "Points");
    XElement data = points.Element(ns + "DataArray");

    int pointsNum = int.Parse(piece.Attribute("NumberOfPoints").Value);
    int dimension = int.Parse(data.Attribute("NumberOfComponents").Value);

    Vector3[] pointsArray = new Vector3[pointsNum];
    string str = data.Value;
    string[] strArray = str.Split('\n');
    for (int i = 1; i < pointsNum + 1; i++)
    {
      string[] p = strArray[i].Split(' ');
      float x = float.Parse(p[0]);
      float y = float.Parse(p[1]);
      float z = float.Parse(p[2]);
      pointsArray[i - 1] = new Vector3(x, y, z);
    }
    return pointsArray;
  }

  void InitBuffers()
  {
    positionArrayBuffer = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(Vector3)));
  }

  void UpdateBuffers()
  {
    positionArrayBuffer.SetData(positionArray);
  }

  void DeleteBuffer(ComputeBuffer buffer)
  {
    if (buffer != null)
    {
      buffer.Release();
      buffer = null;
    }
  }

  void InitBuffers()
  {
    positionArrayBuffer = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(Vector3)));
  }

  void UpdateBuffers()
  {
    positionArrayBuffer.SetData(positionArray);
  }

  void DeleteBuffer(ComputeBuffer buffer)
  {
    if(buffer != null)
    {
      buffer.Release();
      buffer = null;
    }
  }
}

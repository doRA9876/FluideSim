using System.Runtime.InteropServices;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class VFXdrawVTU : MonoBehaviour
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

  [SerializeField] ComputeShader cs_shader;
  int width = 1;
  int height = 1;

  string dataDirectory;

  int particleNum = 0;
  int xmlNum = 0;
  bool simulation = true;

  VisualEffect vfx;
  RenderTexture positionMap;
  int kernel;
  ThreadSize threadSize;

  Vector3[] positionArray;

  private ComputeBuffer positionArrayBuffer;

  void Awake()
  {
    Application.targetFrameRate = 50;
  }

  void Start()
  {
    dataDirectory = Application.dataPath + "/../Data/particle-2D/";
    simulation = GetPositionArray();
    particleNum = positionArray.Length;
    /*
    width = Mathf.CeilToInt(Mathf.Sqrt(particleNum));
    height = Mathf.CeilToInt((float)particleNum / width);
    */
    width = 32;
    height = 32;
    if (!SystemInfo.supportsComputeShaders)
    {
      return;
    }
    vfx = GetComponent<VisualEffect>();
    InitBuffers();
    SetupAttributeMaps();

    Debug.Log("particle number : " + particleNum);
    Debug.Log(string.Format("width : {0}, height : {1}\n", width, height));
  }

  void Update()
  {
    if (simulation)
    {
      UpdateBuffers();
      UpdateAttributeMaps();
      simulation = GetPositionArray();
    }
    // Debug.Log(xmlNum);
  }

  void SetupAttributeMaps()
  {
    positionMap = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
    positionMap.enableRandomWrite = true;
    positionMap.Create();

    kernel = cs_shader.FindKernel("CSMain");
    uint threadSizeX, threadSizeY, threadSizeZ;
    cs_shader.GetKernelThreadGroupSizes(kernel, out threadSizeX, out threadSizeY, out threadSizeZ);
    threadSize = new ThreadSize(threadSizeX, threadSizeY, threadSizeZ);

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

  void UpdateBuffers()
  {
    positionArrayBuffer.SetData(positionArray);
  }

  private Vector3[] LoadVTU(string filePath)
  {
    //ディレクトリ指定してファイルを読み込み
    var xml = XDocument.Load(filePath);

    XNamespace ns = "VTK";

    XElement vtk = xml.Element(ns + "VTKFile");
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

  bool GetPositionArray()
  {
    string path = dataDirectory + "particle_" + string.Format("{0:0000}", xmlNum) + ".vtu";
    xmlNum++;
    if (!File.Exists(path))
    {
      return false;
    }
    positionArray = LoadVTU(path);
    return true;
  }
}

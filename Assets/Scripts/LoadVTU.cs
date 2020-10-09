using UnityEngine;
using System.Xml.Linq;  //重要
using System.Collections.Generic;

public class LoadVTU : MonoBehaviour
{
  void Start()
  {
    string path = Application.dataPath + "/../Data/particle_0000.vtu";
    LoadData(path);
  }

  private void LoadData(string filePath)
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

    foreach (var item in pointsArray)
    {
      Debug.Log(item.x + " " + item.y + " " + item.z + "\n");
    }
  }
}


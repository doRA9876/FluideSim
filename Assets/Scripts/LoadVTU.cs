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
    string str = data.Value;
    Debug.Log(str);
  }
}


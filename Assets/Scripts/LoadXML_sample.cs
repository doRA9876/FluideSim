using UnityEngine;
using System.Xml.Linq;  //重要

public class LoadXML_sample : MonoBehaviour
{
  void Start()
  {
    LoadData();
  }

  private void LoadData()
  {
    string path = Application.dataPath + "/../Data/sample.xml";
    //ディレクトリ指定してファイルを読み込み
    XDocument xml = XDocument.Load(path);

    //テーブルを読み込む
    XElement table = xml.Element("list");

    //データの中身すべてを取得
    var rows = table.Elements("data");

    //取り出し
    foreach (XElement row in rows)
    {
      XElement item = row.Element("name");
      Debug.Log(item.Value);
    }
  }
}


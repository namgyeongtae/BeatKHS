using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class DataManager : Manager
{
    public List<MusicData> MusicList { get; private set; } = new List<MusicData>();

    public override void Init()
    {
        MusicList = JsonConvert.DeserializeObject<List<MusicData>>(Resources.Load<TextAsset>("Json/Musics").text);
    }
}

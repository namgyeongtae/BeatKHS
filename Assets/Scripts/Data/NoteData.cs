using System.Collections.Generic;

[System.Serializable]
public class NoteData
{
    public float time; // 판정 바에 맞춰져야 하는 시간(ms)
    public string type; // 노트 타입 (단타노트, 롱노트 등)
    public float duration; // 노트 지속 시간(ms) -> 롱노트 적용
    public int lane; // 노트가 생성된 레인 번호
}

[System.Serializable]
public class NoteDataContainer
{
    public List<NoteData> notes = new List<NoteData>();
}
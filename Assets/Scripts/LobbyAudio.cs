using UnityEngine;
using FMODUnity; // FMOD Unity 네임스페이스 추가

public class LobbyAudio : MonoBehaviour
{
    public static LobbyAudio Instance;

    [SerializeField] private EventReference lobbyBGM; // Inspector에서 FMOD 이벤트 설정
    private FMOD.Studio.EventInstance bgmInstance;

    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()

    {
        // BGM 이벤트 인스턴스 생성
        bgmInstance = RuntimeManager.CreateInstance(lobbyBGM);
        // BGM 재생 시작
        bgmInstance.start();
    }

    void OnDestroy()
    {
        // 씬이 종료될 때 BGM 정리
        bgmInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        bgmInstance.release();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public EventReference GetEventReference()
    {
        return lobbyBGM;
    }
}


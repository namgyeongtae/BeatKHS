using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InputKeys : MonoBehaviour
{
    private List<Key> _keys = new List<Key>();
    [SerializeField] private Sprite _keyInputs;
    [SerializeField] private Sprite _keyInputsUp;
    
    // Start is called before the first frame update
    void Start()
    {
        _keys = GetComponentsInChildren<Key>().ToList();

        BindKeyInputDown();
        BindKeyInputUp();
    }

    void BindKeyInputDown()
    {
        KeyInputManager.Instance.OnKeyInputDown[0] += () => _keys[0].GetComponent<Image>().sprite = _keyInputs;
        KeyInputManager.Instance.OnKeyInputDown[1] += () => _keys[1].GetComponent<Image>().sprite = _keyInputs;
        KeyInputManager.Instance.OnKeyInputDown[2] += () => _keys[2].GetComponent<Image>().sprite = _keyInputs;
        KeyInputManager.Instance.OnKeyInputDown[3] += () => _keys[3].GetComponent<Image>().sprite = _keyInputs;
        
        KeyInputManager.Instance.OnKeyInputDown[0] += () => _keys[0].transform.GetChild(0).gameObject.SetActive(true);
        KeyInputManager.Instance.OnKeyInputDown[1] += () => _keys[1].transform.GetChild(0).gameObject.SetActive(true);
        KeyInputManager.Instance.OnKeyInputDown[2] += () => _keys[2].transform.GetChild(0).gameObject.SetActive(true);
        KeyInputManager.Instance.OnKeyInputDown[3] += () => _keys[3].transform.GetChild(0).gameObject.SetActive(true);

        KeyInputManager.Instance.OnKeyInputDown[0] += () => JudgeManager.Instance.Judge(0);
        KeyInputManager.Instance.OnKeyInputDown[1] += () => JudgeManager.Instance.Judge(1);
        KeyInputManager.Instance.OnKeyInputDown[2] += () => JudgeManager.Instance.Judge(2);
        KeyInputManager.Instance.OnKeyInputDown[3] += () => JudgeManager.Instance.Judge(3);
    }

    void BindKeyInputUp()
    {
        KeyInputManager.Instance.OnKeyInputUp[0] += () => _keys[0].transform.GetChild(0).gameObject.SetActive(false);
        KeyInputManager.Instance.OnKeyInputUp[1] += () => _keys[1].transform.GetChild(0).gameObject.SetActive(false);
        KeyInputManager.Instance.OnKeyInputUp[2] += () => _keys[2].transform.GetChild(0).gameObject.SetActive(false);
        KeyInputManager.Instance.OnKeyInputUp[3] += () => _keys[3].transform.GetChild(0).gameObject.SetActive(false);

        KeyInputManager.Instance.OnKeyInputUp[0] += () => _keys[0].GetComponent<Image>().sprite = _keyInputsUp;
        KeyInputManager.Instance.OnKeyInputUp[1] += () => _keys[1].GetComponent<Image>().sprite = _keyInputsUp;
        KeyInputManager.Instance.OnKeyInputUp[2] += () => _keys[2].GetComponent<Image>().sprite = _keyInputsUp;
        KeyInputManager.Instance.OnKeyInputUp[3] += () => _keys[3].GetComponent<Image>().sprite = _keyInputsUp;
    }
}

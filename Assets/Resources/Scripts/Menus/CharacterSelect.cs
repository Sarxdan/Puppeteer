using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CharacterSelect : MonoBehaviour
{
    public GameObject[] SelectableCharacters;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void CharacterSelected(int index, string name)
    {
        Debug.Log("Hjehej");
        SelectableCharacter characterScript = SelectableCharacters[index].GetComponent<SelectableCharacter>();
        characterScript.LightEnabled(true);
        characterScript.Selected = true;
        characterScript.ChangeNameTag(name);
        
    }
}

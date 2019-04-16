using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CharacterSelect : MonoBehaviour
{
    public GameObject[] SelectableCharacters;
    public int NumberOfSelectedCharacters;

    // Start is called before the first frame update
    void Start()
    {
        NumberOfSelectedCharacters = 0;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void CharacterSelected(int index, string name, int playerIndex)
    {
        Debug.Log("Yoooooo");
        foreach (GameObject Character in SelectableCharacters)
        {
            SelectableCharacter script = Character.GetComponent<SelectableCharacter>();
            if (script.PlayerIndex == playerIndex)
            {
                Debug.Log("BigOOF");
                script.PlayerIndex = -1;
                script.Selected = false;
                script.LightEnabled(false);
                script.ChangeNameTag("");
                NumberOfSelectedCharacters -= 1;
                break;
            }
        }


        SelectableCharacter characterScript = SelectableCharacters[index].GetComponent<SelectableCharacter>();
        characterScript.LightEnabled(true);
        characterScript.Selected = true;
        characterScript.ChangeNameTag(name);
        characterScript.PlayerIndex = playerIndex;
        NumberOfSelectedCharacters += 1;
        
    }
}

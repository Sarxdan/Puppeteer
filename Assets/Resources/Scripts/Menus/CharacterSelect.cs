using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
* AUTHOR:
* Filip Renman, Kristoffer Lundgren
*
* DESCRIPTION:
* This class takes care of locking characters and unlocking characters when a player selects a character
*
* CODE REVIEWED BY:
* Anton Jonsson 17/04-2019
*
* CONTRIBUTORS:
*
* CLEANED
*/

public class CharacterSelect : MonoBehaviour
{
    public GameObject[] SelectableCharacters;
    public int NumberOfSelectedCharacters;

    // Start is called before the first frame update
    void Start()
    {
        NumberOfSelectedCharacters = 0;
    }

    /* 
     * If the player wanting to select a character already have a character selected, unlock that character and then lock the newly selected character.
     * Otherwise, only lock the newly selected character
     */

    public void CharacterSelected(int index, string name, int playerIndex)
    {
        foreach (GameObject character in SelectableCharacters)
        {
            SelectableCharacter script = character.GetComponent<SelectableCharacter>();
            if (script.PlayerIndex == playerIndex)
            {
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

using I2.Loc;
using UnityEngine;
using UnityTutorial.PlayerControl;
using Zenject;

[System.Serializable]
public class PlayerDataSaver : MonoBehaviour
{
    [Inject] GameScenario _gameScenario;
    [Inject] WorldSaveGameManager _worldSaveGameManager;
    [Inject] PlayerController _playerController;
    
    public void SaveCharacterData()
    {
        if (_worldSaveGameManager.currentCharacterSaveData == null)
        {
            Debug.LogError("WorldSaveGameManager.currentCharacterSaveData is null");
            return;
        }

        CharacterSaveData characterSaveData = _worldSaveGameManager.currentCharacterSaveData;

        characterSaveData.playerPosition = _playerController.transform.position;
        characterSaveData.playerRotation = _playerController.transform.rotation.eulerAngles;

        characterSaveData.language = LocalizationManager.CurrentLanguage;
    }

    public void LoadCharacterData()
    {
        CharacterSaveData characterSaveData = _worldSaveGameManager.currentCharacterSaveData;
        // = currentCharacterSaveData.dayNumber;

        _playerController.transform.position = characterSaveData.playerPosition;
        _playerController.transform.rotation = Quaternion.Euler(characterSaveData.playerRotation);

        LocalizationManager.CurrentLanguage = characterSaveData.language;
    }
}
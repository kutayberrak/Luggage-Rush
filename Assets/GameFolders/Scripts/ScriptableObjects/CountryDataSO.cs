using System.Collections.Generic;
using UnityEngine;

namespace GameFolders.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CountryDataSO", menuName = "ScriptableObjects/CountryDataSO")]
    public class CountryDataSO : ScriptableObject
    {
        [SerializeField] private string countryName;
        [SerializeField] private List<LevelDataSO> levelDataList;
        
        public string CountryName => countryName;
        public List<LevelDataSO> LevelDataList => levelDataList;
    }
}

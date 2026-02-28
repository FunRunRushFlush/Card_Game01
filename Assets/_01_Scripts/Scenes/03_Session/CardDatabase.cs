using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : MonoBehaviour
{
    [SerializeField] private CardDatabaseSO assetDb;
    public IReadOnlyList<CardData> AllCards => assetDb.AllCards;


    public CardData GetById(string id) => assetDb != null ? assetDb.GetById(id) : null;

    public CardDatabaseSO AssetDb => assetDb;
}
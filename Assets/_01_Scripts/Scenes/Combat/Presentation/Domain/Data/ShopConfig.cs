using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Run/Shop Config")]
public class ShopConfig : ScriptableObject
{
    public int rerollCost = 10;
    public int itemCount = 5;
    //public List<ShopItemDefinition> itemPool;
}

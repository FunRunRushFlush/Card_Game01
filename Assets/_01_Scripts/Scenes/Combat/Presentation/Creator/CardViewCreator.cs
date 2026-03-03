using DG.Tweening;
using Game.Logging;
using UnityEngine;

public class CardViewCreator : Singleton<CardViewCreator>
{
    [Header("World")]
    [SerializeField] private CardView cardViewPrefab;

    [Header("UI")]
    [SerializeField] private CardViewUI cardViewUiPrefab;

    public CardView CreateCardView(Card card, Vector3 position, Quaternion rotation)
    => CreateCardView(card, position, rotation, null);
    public CardView CreateCardView(Card card, Vector3 position, Quaternion rotation, Transform parent)
    {
        CardView cardView = Instantiate(cardViewPrefab, position, rotation, parent);
        cardView.Setup(card);

        Log.Debug(LogArea.Combat, () => $"CreateCardView activeScene={UnityEngine.SceneManagement.SceneManager.GetActiveScene().name} " +
             $"spawnedScene={cardView.gameObject.scene.name}");

        return cardView;
    }

    public CardViewUI CreateCardViewUI(Card card, RectTransform parent)
    {
        var view = Instantiate(cardViewUiPrefab, parent, worldPositionStays: false);
        view.Setup(card);
        return view;
    }
}

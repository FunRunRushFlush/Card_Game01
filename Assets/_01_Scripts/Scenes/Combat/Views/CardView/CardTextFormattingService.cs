using UnityEngine;

public class CardTextFormattingService : MonoBehaviour
{
    [SerializeField] private KeywordDatabase keywordDatabase;
    private CardTextFormatter formatter;

    public static CardTextFormattingService Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        formatter = new CardTextFormatter(keywordDatabase, emitLinks: true);
    }

    public string Format(string raw) => formatter.Format(raw);
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text;

public class Credits : MonoBehaviour
{
    public float scrollSeconds = 10f;
    
    /* // Uncomment to test credits without playing n games first.
    public GameInfo[] testGames;

    // Use for testing.
    void Start() {
       Populate(testGames);
       StartScroll();
    }
    */

    static StringBuilder _builder = new();
    [SerializeField] CreditsHeader _header;
    [SerializeField] CreditsRow _row;
    [SerializeField] RectTransform _assetsHeader;
    [SerializeField] TextMeshProUGUI _assetsBody;

    [SerializeField] RectTransform _spacer;

    [SerializeField] float _maxSpeedBoost = 4f;

    public void Clear() {
        // Remove all previous credits, keeping the header/footer.
        for (int index = transform.childCount - 2; index > 1; index--) {
            Destroy(transform.GetChild(index).gameObject);    
        }        
        transform.parent.gameObject.SetActive(false);
    }


    public void Populate(IList<GameInfo> games) {
        // Show credits screen.
        transform.parent.gameObject.SetActive(true);

        // Start credits scroll halfway up the screen.
        var rt = (RectTransform)transform;
        var pos = rt.anchoredPosition;
        pos.y = 0;
        rt.anchoredPosition = pos;

        var sources = new List<Source>();

        foreach(var info in games) {
            var header = Instantiate(_header, transform);

            header.image.sprite = info.bannerImage;
            header.title.text = info.gameTitle;

            var devs = info.developerCredits;
            Array.Sort(devs);
            for (int developer = 0; developer < devs.Length; developer++) {
                var row = Instantiate(_row, transform);
                row.left.text = devs[developer++];
                row.right.text = developer < devs.Length ? devs[developer] : string.Empty;
            }
            
            sources.Clear();
            foreach(var entry in info.licensedWorksCredits) {
                // Skip blank entries.
                if (string.IsNullOrWhiteSpace(entry.creator) && string.IsNullOrWhiteSpace(entry.source)) 
                    continue;

                // Group entries by source.
                string normalized = entry.source.ToLowerInvariant();
                var source = sources.Find(s => s.name.ToLowerInvariant() == normalized);
                if (source == null) {
                    source = new Source() { name = entry.source };
                    sources.Add(source);
                }

                if (string.IsNullOrWhiteSpace(entry.creator)) continue;

                // Prune duplicate entries.
                if (!source.creators.Contains(entry.creator))
                    source.creators.Add(entry.creator);
            }

            // Print sources in a standardized order.
            sources.Sort((a, b) => a.name.CompareTo(b.name));
            _builder.Length = 0;
            for (int index = 0; index < sources.Count; index++) {
                if (index > 0) _builder.Append(" / ");

                var source = sources[index];
                source.creators.Sort();
                for (int creator = 0; creator < source.creators.Count; creator++) {
                    if (creator > 0) _builder.Append(", ");
                    _builder.Append(source.creators[creator]);
                }
                if (!string.IsNullOrWhiteSpace(source.name)) {
                    if (source.creators.Count > 0)
                        _builder.Append(" via ");
                    _builder.Append(source.name);
                }
            }
            
            if (_builder.Length > 0) {
                Instantiate(_assetsHeader, transform);
                var body = Instantiate(_assetsBody, transform);
                body.text = _builder.ToString(); 
            }

            Instantiate(_spacer, transform);     
        } 
        
        // Swap the footer to the end.
        transform.GetChild(2).SetSiblingIndex(transform.childCount - 1);
    }

    public Coroutine StartScroll() {
        return StartCoroutine(Scroll());
    }


    IEnumerator Scroll() {
        yield return null;   
        var rt = (RectTransform)transform;
        var pos = rt.anchoredPosition;    
        
        var targetY = rt.rect.height + ((RectTransform)rt.parent).rect.height * 0.5f;
        float speed = 1f/scrollSeconds;        

        float speedMultiplier = 1f;

        var left = MicrogamesManager.Instance.leftPlayer;
        var right = MicrogamesManager.Instance.rightPlayer;        

        for (float t = 0; t < 1; t += speed * speedMultiplier * Time.deltaTime) {
            if (left.SecondsSinceInput < Time.deltaTime || right.SecondsSinceInput < Time.deltaTime) {
                speedMultiplier = Mathf.Min(speedMultiplier + 0.1f, _maxSpeedBoost);                
            }

            pos.y = t * targetY;
            rt.anchoredPosition = pos;
            yield return null;
        }        
    }

    class Source {
        public string name;
        public List<String> creators = new();
    }
}
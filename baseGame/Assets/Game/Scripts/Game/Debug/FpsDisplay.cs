using UnityEngine;

public class FpsDisplay : MonoBehaviour
{
    public bool show = true;
    public int fontSize = 28;
    public Color textColor = Color.white;
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.45f);
    public Vector2 position = new Vector2(12f, 12f);
    public Vector2 size = new Vector2(260f, 92f);
    public float refreshInterval = 0.25f;

    private float timeLeft;
    private int frames;
    private float fps;
    private float frameMs;
    private GUIStyle textStyle;
    private Texture2D backgroundTexture;

    private void Awake()
    {
        timeLeft = refreshInterval;
    }

    private void Update()
    {
        frames++;
        timeLeft -= Time.unscaledDeltaTime;

        if (timeLeft <= 0f)
        {
            float elapsed = refreshInterval - timeLeft;
            fps = frames / elapsed;
            frameMs = fps > 0f ? 1000f / fps : 0f;
            frames = 0;
            timeLeft = refreshInterval;
        }
    }

    private void OnGUI()
    {
        if (!show)
        {
            return;
        }

        EnsureStyle();

        Rect rect = new Rect(position.x, position.y, size.x, size.y);
        GUI.DrawTexture(rect, backgroundTexture);
        GUI.Label(rect, $"FPS: {fps:F1}\nFrame: {frameMs:F2} ms", textStyle);
    }

    private void EnsureStyle()
    {
        if (textStyle == null)
        {
            textStyle = new GUIStyle(GUI.skin.label);
            textStyle.alignment = TextAnchor.MiddleLeft;
            textStyle.padding = new RectOffset(14, 8, 8, 8);
        }

        textStyle.fontSize = fontSize;
        textStyle.normal.textColor = textColor;

        if (backgroundTexture == null)
        {
            backgroundTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            backgroundTexture.hideFlags = HideFlags.HideAndDontSave;
        }

        backgroundTexture.SetPixel(0, 0, backgroundColor);
        backgroundTexture.Apply();
    }
}

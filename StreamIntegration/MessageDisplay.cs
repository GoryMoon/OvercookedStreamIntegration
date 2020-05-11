using System;
using System.Collections.Concurrent;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace StreamIntegration
{
    public class MessageDisplay: MonoBehaviour
    {
        public static readonly ConcurrentQueue<Message> Messages = new ConcurrentQueue<Message>();
        private const int MaxMessages = 5;
        private float _nextActionTime;
        private Texture2D _boxTexture;

        private void Start()
        {
            var c = Color.black;
            c.a = 0.3f;
            _boxTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            var pixels = _boxTexture.GetPixels();
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = c;
            }
            _boxTexture.SetPixels(pixels);
        }

        private void Update()
        {
            if (Time.time > _nextActionTime)
            {
                _nextActionTime += 0.5f;
                while (Messages.Count > MaxMessages)
                {
                    Message m;
                    Messages.TryDequeue(out m);
                }
            }
        }

        private void OnGUI()
        {
            if (T17InGameFlow.Instance != null)
            {
                if (!T17InGameFlow.Instance.IsPauseMenuOpen() && Messages.Any(message => !message.ShouldRemove()))
                {
                    var guiRect = new GUIRect(new Rect(0.003f, 0.01f, 0.3f, 0.02f * MaxMessages), GUIAnchor.TopRight, GUICoordSystem.Normalised);
                    var m = string.Join("\n",
                        Messages.Reverse().Where(message => !message.ShouldRemove()).Take(MaxMessages).Select(message => message.Text).ToArray());
                    
                    var boxStyle = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperRight, fontSize = 15, normal = { background = _boxTexture ,textColor = Color.white}};
                    GUI.Box(guiRect.GetInPixels(Camera.main), m, boxStyle);

                    /*
                    var index = 0;
                    foreach (var m in Messages.Reverse().Where(message => !message.ShouldRemove()).Take(MaxMessages))
                    {
                        guiRect = new GUIRect(new Rect(0.005f, 0.01f + 0.02f * index + 0.007f * index, 0.3f, 0.02f),
                            GUIAnchor.TopRight, GUICoordSystem.Normalised);
                        DrawString(guiRect.GetInPixels(Camera.main), m.Text);
                        index++;
                    }*/
                }
            }
        }

        private static void DrawString(Rect rect, string message)
        {
            var textStyle = AccessUtils.GetTextStyle(rect, TextAnchor.MiddleRight, FontData.defaultFontData.font, message);
            var color = GUI.color;
            GUI.color = Color.white;
            GUI.Label(rect, message, textStyle);
            GUI.color = color;
        }

        internal static void Load()
        {
            try
            {
                var gameObject = new GameObject(typeof (MessageDisplay).FullName, typeof (MessageDisplay));
            }
            catch (Exception ex)
            {
                Main.Logger.LogException(ex);
            }
        }
    }

    public class Message
    {
        public readonly string Text;
        private readonly float _remove;

        private Message(string text, float remove)
        {
            Text = text;
            _remove = remove;
        }

        public bool ShouldRemove()
        {
            return Time.time > _remove;
        }

        public static Message Create(string t)
        {
            return new Message(t, Time.time + 10f);
        }
    }
}
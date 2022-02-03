using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Drawing.VR {
    public class WhiteBoard : MonoBehaviour {
        private int textureSize = 2048;
        private Texture2D texture;
        private Color[] brush;
        private Color[] deleteColor;
        private new Renderer renderer;
        private bool touchingLastFrame;
        private float lastX, lastY;
        private bool everyOthrFrame;

        // Start is called before the first frame update
        void Start() {
            renderer = GetComponent<Renderer>();
            texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Trilinear;
            texture.anisoLevel = 3;
            renderer.material.mainTexture = texture;
            Color fillColor = Color.white;
            var fillColorArray = texture.GetPixels();
            for (var i = 0; i < fillColorArray.Length; ++i) {
                fillColorArray[i] = fillColor;
            }
            texture.SetPixels(fillColorArray);
            texture.Apply();
            deleteColor = Enumerable.Repeat(Color.white, textureSize * textureSize).ToArray();
        }

        public void DrawAtPosition(float[] pos, int _pensize, float[] _color) {
            int penSize = _pensize;
            int penSizeHalf = _pensize / 2;

            brush = Enumerable.Repeat((new Color(_color[0], _color[1], _color[2])), penSize * penSize).ToArray();

            int x = (int)(pos[0] * textureSize - penSizeHalf);
            int y = (int)(pos[1] * textureSize - penSizeHalf);

            //If last frame was not touching a marker, we don't need to lerp from last pixel coordinate to new, so we set the last coordinates to the new.
            if (!touchingLastFrame) {
                lastX = (float)x;
                lastY = (float)y;
                touchingLastFrame = true;
            }

            if (touchingLastFrame) {
                texture.SetPixels(x, y, penSize, penSize, brush);

                //Lerp last pixel to new pixel, so we draw a continuous line.
                for (float t = 0.01f; t < 1.00f; t += 0.1f) {
                    int lerpX = (int)Mathf.Lerp(lastX, (float)x, t);
                    int lerpY = (int)Mathf.Lerp(lastY, (float)y, t);
                    texture.SetPixels(lerpX, lerpY, penSize, penSize, brush);
                }
                //We apply the texture every other frame, so we improve performance.
                if (!everyOthrFrame) {
                    everyOthrFrame = true;
                } else if (everyOthrFrame) {
                    texture.Apply();
                    everyOthrFrame = false;
                }
            }
            lastX = (float)x;
            lastY = (float)y;
        }

        public void ResetTouch() {
            touchingLastFrame = false;
        }

        //To clear the whiteboard.
        public void ClearWhiteboard() {
            texture.SetPixels(deleteColor);
            texture.Apply();
        }
    }
}

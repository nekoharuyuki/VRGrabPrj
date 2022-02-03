using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drawing.VR {
    //This class sends a Raycast from the marker and detect if it's hitting the whiteboard (tag: Finish)
    [RequireComponent(typeof(OVRGrabbable))]
    public class Pen : MonoBehaviour {
        private WhiteBoard whiteboard;
        [SerializeField] private Transform drawingPoint;
        [SerializeField] private Renderer penTop;
        private RaycastHit touch;
        private bool touching;
        private float drawingDistance = 0.015f;
        private Quaternion lastAngle;
        private OVRGrabbable grabbable;
        [SerializeField] private int penSize = 8;
        [SerializeField] private Color color = Color.red;

        private void Awake() {
            grabbable = GetComponent<OVRGrabbable>();
        }

        // Start is called before the first frame update
        void Start() {
            var block = new MaterialPropertyBlock();

            // You can look up the property by ID instead of the string to be more efficient.
            block.SetColor("_BaseColor", color);

            // You can cache a reference to the renderer to avoid searching for it.
            penTop.SetPropertyBlock(block);
        }

        // Update is called once per frame
        void Update() {
            //if the marker is not in possesion of the user, or is not grabbed, we don't run update.
            if (!grabbable.isGrabbed) return;

            //Cast a raycast to detect whiteboard.
            if (Physics.Raycast(drawingPoint.position, drawingPoint.up, out touch, drawingDistance)) {
                //The whiteboard has the tag "Finish".
                if (touch.collider.CompareTag("Finish")) {
                    if (!touching) {
                        touching = true;
                        //store angle so while drawing, marker doesn't rotate
                        lastAngle = transform.rotation;
                        whiteboard = touch.collider.GetComponent<WhiteBoard>();
                    }
                    if (whiteboard == null) return;
                    //Send the rpc with the coordinates, pen size and color of marker in RGB.
                    whiteboard.DrawAtPosition(new float[] { touch.textureCoord.x, touch.textureCoord.y }, penSize, new float[] { color.r, color.g, color.b });
                }
            } else if (whiteboard != null) {
                touching = false;
                whiteboard.ResetTouch();
                whiteboard = null;
            }
        }

        private void LateUpdate() {
            //lock rotation of marker when touching whiteboard.
            if (touching) {
                transform.rotation = lastAngle;
            }
        }
    }
}

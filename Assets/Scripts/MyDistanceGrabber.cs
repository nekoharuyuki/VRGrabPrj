using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class MyDistanceGrabber : MonoBehaviour {
    public enum HandType {
        LEFT,
        RIGHT
    }

    [SerializeField] private HandType _handType = HandType.LEFT;
    [SerializeField] private float _castCircleRadius = 0.2f;
    [SerializeField] private float _maxCastDistance = 3f;
    [SerializeField, Range(0f, 1f)] private float _powerAdjust = 0.2f;
    [SerializeField] private float _minReleasePower = 1f;
    [SerializeField] private float _maxReleasePower = 100f;

    private MyGrabbable _targetGrabbable;
    private MyGrabbable _grabbedGrabbable;

    void Start() {
        this.FixedUpdateAsObservable().Subscribe(_ => {
            // 掴めるオブジェクトの判定
            DetectGrabbable();
        }).AddTo(this);

        // 右か左かによってボタン、コントローラーの切り分け
        OVRInput.RawButton buttonType = OVRInput.RawButton.LIndexTrigger;
        OVRInput.Controller controller = OVRInput.Controller.LTouch;
        switch (_handType) {
            case HandType.LEFT:
                buttonType = OVRInput.RawButton.LIndexTrigger;
                controller = OVRInput.Controller.LTouch;
                break;
            case HandType.RIGHT:
                buttonType = OVRInput.RawButton.RIndexTrigger;
                controller = OVRInput.Controller.RTouch;
                break;
        }

        // ボタンが押されたとき
        this.UpdateAsObservable()
            .Where(_ => OVRInput.GetDown(buttonType))
            .Subscribe(_ => {
                if (_targetGrabbable != null) {
                    _targetGrabbable.Grab(transform);
                    _grabbedGrabbable = _targetGrabbable;
                }
            }).AddTo(this);

        // ボタンが離されたとき
        this.UpdateAsObservable()
            .Where(_ => OVRInput.GetUp(buttonType))
            .Subscribe(_ => {
                if (_grabbedGrabbable != null) {
                    Vector3 dir = transform.forward;
                    float mag = OVRInput.GetLocalControllerAcceleration(controller).magnitude;
                    if (mag < _minReleasePower) {
                        mag = 0f;
                    }

                    if (mag > _maxReleasePower) {
                        mag = _maxReleasePower;
                    }
                    Vector3 acc = dir.normalized * (mag * _powerAdjust);
                    _grabbedGrabbable.Release(acc);
                    _grabbedGrabbable = null;
                }
            }).AddTo(this);
    }

    /// <summary>
    /// 掴めるオブジェクトを検知する
    /// </summary>
    void DetectGrabbable() {
        RaycastHit hit;
        Vector3 p1 = transform.position;
        int layerMask = LayerMask.GetMask(new string[] { "Grabbable" });
        if (Physics.SphereCast(p1, _castCircleRadius, transform.forward, out hit, _maxCastDistance, layerMask)) {
            var grabbable = hit.collider.GetComponent<MyGrabbable>();
            if (grabbable) {
                if (_targetGrabbable != null) {
                    _targetGrabbable.OutLineEnabled = false;
                }
                grabbable.OutLineEnabled = true;
                _targetGrabbable = grabbable;
            }
        } else {
            if (_targetGrabbable != null) {
                _targetGrabbable.OutLineEnabled = false;
                _targetGrabbable = null;
            }
        }
    }
}

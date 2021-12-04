using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

[RequireComponent(typeof(Outline))]
public class MyGrabbable : MonoBehaviour {
    public bool IsGrabbed {
        get => _isGrabbed;
    }

    private Rigidbody _rigidbody;
    private Transform _defaultParent;
    private Outline _outline;
    private bool _isGrabbed = false;

    public bool OutLineEnabled {
        get { return _outline.enabled; }
        set { _outline.enabled = value; }
    }

    void Awake() {
        _rigidbody = this.GetComponent<Rigidbody>();
        _outline = this.GetComponent<Outline>();
        _outline.enabled = false;
    }

    void Start() {
        _defaultParent = transform.parent;
    }

    private IDisposable _grabDisposable;
    public void Grab(Transform handTrans) {
        if (_isGrabbed) {
            return;
        }

        if (_rigidbody != null) {
            _rigidbody.isKinematic = true;
        }
        _isGrabbed = true;
        _grabDisposable?.Dispose();
        _grabDisposable = this.UpdateAsObservable()
            // 手まで限りなく近くなるまで自分を近づける
            .TakeWhile(_ => (transform.position - handTrans.position).sqrMagnitude > 0.1f * 0.1f).Subscribe(
                _ => {
                    // 手までオブジェクトを近づける
                    Vector3 dir = (handTrans.position - transform.position).normalized;
                    transform.position += dir * (10f * Time.deltaTime);
                }, () => {
                    // 近づける事が完了した時
                    // 手まで限りなく近くなった場合、親を手にする
                    transform.parent = handTrans;
                    transform.position = handTrans.position;
                }).AddTo(this);
    }

    public void Release(Vector3 controllerAcc) {
        if (!_isGrabbed) {
            return;
        }

        if (_rigidbody != null) {
            _rigidbody.isKinematic = false;
            _rigidbody.AddForce(controllerAcc, ForceMode.Impulse);
        }
        _grabDisposable?.Dispose();
        _isGrabbed = false;
        transform.parent = _defaultParent;
    }
}
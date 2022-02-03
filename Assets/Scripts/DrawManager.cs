using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawManager : MonoBehaviour {
    //変数を用意
    //SerializeFieldをつけるとInspectorウィンドウからゲームオブジェクトやPrefabを指定できます。
    [SerializeField] GameObject LineObjectPrefab;
    [SerializeField] Transform HandAnchor;//positionを取得するコントローラーの位置情報

    //現在描画中のLineObject;
    private GameObject CurrentLineObject = null;

    private Transform Pointer {
        get {
            return HandAnchor;
        }
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        var pointer = Pointer;
        if (pointer == null) {
            Debug.Log("pointer not defiend");
            return;
        }

        //Oculus Touchの人差し指のトリガーが引かれている間
        if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger)) {
            if (CurrentLineObject == null) {
                //PrefabからLineObjectを生成
                CurrentLineObject = Instantiate(LineObjectPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            }
            //ゲームオブジェクトからLineRendererコンポーネントを取得
            LineRenderer render = CurrentLineObject.GetComponent<LineRenderer>();

            //LineRendererからPositionsのサイズを取得
            int NextPositionIndex = render.positionCount;

            //LineRendererのPositionsのサイズを増やす
            render.positionCount = NextPositionIndex + 1;

            //LineRendererのPositionsに現在のコントローラーの位置情報を追加
            render.SetPosition(NextPositionIndex, pointer.position);
        } else if (OVRInput.GetUp(OVRInput.RawButton.RIndexTrigger)) {
            //人差し指のトリガーを離したとき
            if (CurrentLineObject != null) {
                //現在描画中の線があったらnullにして次の線を描けるようにする。
                CurrentLineObject = null;
            }
        }
    }
}

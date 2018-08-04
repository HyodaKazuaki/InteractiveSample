using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour {

    public KinectWrapper.NuiSkeletonPositionIndex TrackedJoint = KinectWrapper.NuiSkeletonPositionIndex.HandRight;
    public GameObject HandPointObject; // 手の位置を示すためのオブジェクト
    public float smoothFactor = 5f;

    private float distanceToCamera = 10f;

	// Use this for initialization
	void Start () {
        if (HandPointObject)
        {
            // カメラとの距離を取得
            distanceToCamera = (HandPointObject.transform.position - Camera.main.transform.position).magnitude;
        }
    }

    // Update is called once per frame
    void Update () {
        KinectManager manager = KinectManager.Instance;

        if(manager && manager.IsInitialized())
        {
            // Kinectが利用可能
            int iJointIndex = (int)TrackedJoint;

            if (manager.IsUserDetected())
            {
                // 人物を認識
                // 人物を一人に固定
                uint userId = manager.GetPlayer1ID();

                if(manager.IsJointTracked(userId, iJointIndex))
                {
                    // 特定のユーザーのジョイント(手)を認識

                    Vector3 posJoint = manager.GetRawSkeletonJointPos(userId, iJointIndex);
                    if(posJoint != Vector3.zero)
                    {
                        // ジョイントの位置が0でない
                        // 3次元座標を深度に変換
                        Vector2 posDepth = manager.GetDepthMapPosForJointPos(posJoint);
                        // 3次元座標を色情報に変換
                        Vector2 posColor = manager.GetColorMapPosForDepthPos(posJoint);

                        // 座標変換(Y座標については仕様上反転している)
                        float scaleX = (float)posColor.x / KinectWrapper.Constants.ColorImageWidth;
                        float scaleY = 1.0f - (float)posColor.y / KinectWrapper.Constants.ColorImageHeight;

                        if (HandPointObject)
                        {
                            // 座標系を変換しなめらかに移動
                            Vector3 vPosHandPoint = Camera.main.ViewportToWorldPoint(new Vector3(scaleX, scaleY, distanceToCamera));
                            HandPointObject.transform.position = Vector3.Lerp(HandPointObject.transform.position, vPosHandPoint, smoothFactor * Time.deltaTime);
                        }
                    }
                }
                else
                {
                    Debug.Log("Hand lost");
                }
            }
            else
            {
                Debug.Log("Human lost");
            }
        }
        else
        {
            Debug.Log("Kinect is unavailavle");
        }
    }
}

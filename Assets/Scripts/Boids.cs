using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 参考
// http://neareal.net/index.php?ComputerGraphics%2FUnity%2FTips%2FBoids%20Model

public class Boids : MonoBehaviour {
    public int Fish = 50; // 魚の数
    public GameObject FishPrefab; // 魚のプレファブ
    public GameObject TargetObject; // 追従する対象
    public GameObject[] FishChildren;
    public float Turbulence = 0.95f; // 中心移動の重み
    public float Distance = 4f; // 個体間距離
    private float[] yPosition;

	// Use this for initialization
	void Start () {
        // 群れの生成
        var parent = transform;
        FishChildren = new GameObject[Fish];
        yPosition = new float[Fish];
        for (int i = 0; i < Fish; i++)
        {
            FishChildren[i] = Instantiate(FishPrefab, parent);
            FishChildren[i].transform.position = 
                new Vector3(Random.Range(-50f, 50f),
                FishPrefab.transform.position.y,
                Random.Range(-50f, 50f)
                );
            yPosition[i] = FishChildren[i].transform.position.y;
        }
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 center = Vector3.zero;

        /** ここにKinectで取得した手の位置をcenterに代入する **/

        Vector3 vecMouse = Input.mousePosition;
        vecMouse.z += 200f;
        Vector3 screenPos = Camera.main.ScreenToWorldPoint(vecMouse);
        TargetObject.transform.position = screenPos;

        // 群れの中心を求める
        foreach (var Child in FishChildren)
        {
            center += Child.transform.position;
        }
        center /= FishChildren.Length - 1;
        center += new Vector3(TargetObject.transform.position.x, FishPrefab.transform.position.y, TargetObject.transform.position.z);
        center /= 2;
        center.y = -2f;
        center = new Vector3(TargetObject.transform.position.x, -2f, TargetObject.transform.position.z);
        Debug.Log(center);

        // 群れの中心へ移動
        Vector3 aveVelocity = Vector3.zero;
        foreach(var Child in FishChildren)
        {
            Rigidbody rigidbody = Child.GetComponent<Rigidbody>();
            Vector3 ToCenter = (center - Child.transform.position).normalized;
            Vector3 direction = (rigidbody.velocity.normalized * Turbulence + ToCenter * (1 - Turbulence)).normalized;

            direction *= Random.Range(20f, 30f); // 速度
            rigidbody.velocity = direction;

            // 各個体間の間隔
            foreach (var ChildB in FishChildren)
            {
                if (Child == ChildB)
                    continue;

                Vector3 diff = Child.transform.position - ChildB.transform.position;

                if (diff.magnitude < Random.Range(2, Distance))
                    rigidbody.velocity = diff.normalized * rigidbody.velocity.magnitude;
            }
            // 速度平均
            aveVelocity += rigidbody.velocity;
        }

        aveVelocity /= FishChildren.Length;

        int i = 0;
        foreach (var Child in FishChildren)
        {
            // 平均移動ベクトルの追従
            Rigidbody rigidbody = Child.GetComponent<Rigidbody>();
            rigidbody.velocity = rigidbody.velocity * Turbulence + aveVelocity * (1f - Turbulence);
            var vcity = rigidbody.velocity;
            vcity.y = 0f;
            rigidbody.velocity = vcity;
            Child.transform.position = new Vector3(Child.transform.position.x, yPosition[i], Child.transform.position.z);
            // 常にcenterを向き続ける
            Child.transform.LookAt(center);
            i++;
        }
    }
}

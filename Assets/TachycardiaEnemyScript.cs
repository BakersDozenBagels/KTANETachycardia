using UnityEngine;

public class TachycardiaEnemyScript : MonoBehaviour
{
    [SerializeField]
    private Transform _transform;

    public void SetMotion(float v)
    {
        float h = v + 0.07f * (-Mathf.Abs(v - 0.5f) + 0.5f) * Mathf.Sin(2f * Mathf.PI * v);
        //float h = v;
        float x = h + (-Mathf.Abs(h - 0.5f) + 0.5f) * Mathf.Sin(20f * Mathf.PI * h + Mathf.PI) / 16f / Mathf.PI;
        float y = .6f * (-Mathf.Abs(x - 0.5f) + 0.5f) * Mathf.Sin(10f * Mathf.PI * x);
        _transform.localPosition = new Vector3(x, 0, y);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotentialTargetIndicator : MonoBehaviour
{
    private Vector3 originalScale;
    private float scaleRange = 0.1f; 
    private float scaleSpeed = 0.2f;

    private void Start()
    {
        originalScale = transform.localScale;
    }
    void FixedUpdate()
    {
        float scale = Mathf.PingPong(Time.fixedTime * scaleSpeed, scaleRange);

        float targetScale = 0.95f + scale * 0.5f;
        transform.localScale = originalScale * targetScale;
    }
}

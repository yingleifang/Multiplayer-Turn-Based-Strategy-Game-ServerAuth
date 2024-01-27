using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillBehavior : WeaponBehavior
{

    public float RotationSpeed = 15;
    public float spellPositionScaler = 0.6f;
    public float radius = 10;

    [SerializeField] GameObject projectile;
    float deltaRange = 1f;
    float speed = 200;
    float arrowPositionScaller = 0.2f;
    //private void Start()
    //{
    //    projectile = transform.GetChild(0).GetChild(0).gameObject;
    //    if (!projectile.CompareTag(projectileTag))
    //    {
    //        Debug.LogError("The projectile for this weapon is not set at the correct position");
    //    }
    //}
    public override IEnumerator AttackBehavior(UnitFeature target, UnitFeature attacker)
    {
        GameObject projectileInstance = Instantiate(projectile, attacker.transform.position + target.meshHeight * Vector3.up / arrowPositionScaller, Quaternion.identity);
        Debug.Log("Projectile instance: " + projectileInstance);
        projectileInstance.transform.localScale = attacker.transform.localScale;
        projectile.SetActive(false);
        Vector3 position = target.transform.position + target.meshHeight * transform.localScale.y * Vector3.up / arrowPositionScaller;
        while (Mathf.Abs(projectileInstance.transform.position.z - position.z) > deltaRange || Mathf.Abs(projectileInstance.transform.position.x - position.x) > deltaRange)
        {
            projectileInstance.transform.LookAt(position);
            projectileInstance.transform.Translate(speed * Time.deltaTime * Vector3.forward);
            yield return null;
        }
        Destroy(projectileInstance);
        projectile.SetActive(true);
    }
}
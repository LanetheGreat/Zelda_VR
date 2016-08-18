﻿using UnityEngine;

[RequireComponent(typeof(Animator))]

public class EnemyAnimation : MonoBehaviour
{
    Animator _animator;
    public Animator AnimatorComponent { get { return _animator ?? (_animator = GetComponent<Animator>()); } }

    Enemy _enemy;
    
    
    public bool IsSpawning { get { return AnimatorComponent ? AnimatorComponent.GetCurrentAnimatorStateInfo(0).IsTag("Spawn") : false; } }
    public bool IsDying { get { return AnimatorComponent ? AnimatorComponent.GetCurrentAnimatorStateInfo(0).IsTag("Die") : false; } }


    void Awake()
    {
        AnimatorComponent.logWarnings = false;

        _enemy = transform.parent.GetComponent<Enemy>();
    }


    public void PlayDeathAnimation()
    {
        GameObject deathAnimPrefab = CommonObjects.Instance.enemyDeathAnimation;
        Instantiate(deathAnimPrefab, transform.position, Quaternion.identity);
    }

    float _storedSpeed;
    public void Pause()
    {
        _storedSpeed = AnimatorComponent.speed;
        AnimatorComponent.speed = 0;
    }
    public void Resume()
    {
        AnimatorComponent.speed = _storedSpeed;
    }


    void Update()
    {
        if (AnimatorComponent != null)
        {
            //if (AnimatorComponent.ContainsParam("Jumping"))
            {
                AnimatorComponent.SetBool("Jumping", _enemy.IsJumping);
            }
        }

        float angle = GetFacingAngleRelativeToCamera();
        SetDirectionForAngle(angle);
    }


    float GetFacingAngleRelativeToCamera()
    {
        Transform cam = WorldInfo.Instance.GetPrimaryCamera().transform;

        Vector3 myForward = transform.parent.forward;
        Vector3 toCam = cam.position - transform.parent.position;

        Vector3 referenceRight = Vector3.Cross(Vector3.up, toCam);

        float angle = Vector3.Angle(myForward, toCam);
        float sign = (Vector3.Dot(myForward, referenceRight) > 0.0f) ? 1.0f : -1.0f;
        float finalAngle = sign * angle;

        return finalAngle;
    }

    void SetDirectionForAngle(float angle)
    {
        int direction;
        if ((angle >= -45 && angle < 45) || (angle >= 315))
        {
            direction = 0;
        }
        else if ((angle >= -315 && angle < -225) || (angle >= 45 && angle < 135))
        {
            direction = 1;
        }
        else if ((angle >= -225 && angle < -135) || (angle >= 135 && angle < 225))
        {
            direction = 2;
        }
        else
        {
            direction = 3;
        }

        if (AnimatorComponent != null)
        {
            AnimatorComponent.SetInteger("Direction", direction);
        }
    }


    public void ActivateFlash(bool activate = true)
    {
        if (activate)
        {
            gameObject.AddComponent<FlashColorsSimple>();
        }
        else
        {
            Destroy(GetComponent<FlashColorsSimple>());
        }
    }
}
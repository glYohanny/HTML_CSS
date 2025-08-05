using UnityEngine;
using System.Collections.Generic;

public class UnitAnimatorSetup : MonoBehaviour
{
    public Animator animator;
    public AnimatorOverrideController overrideController;

    // Clips básicos
    public AnimationClip walkClip;
    public AnimationClip attackClip;
    public AnimationClip dieClip;
    public AnimationClip idleClip;

    // Clips específicos de recolector
    public AnimationClip collectClip;
    public AnimationClip buildClip;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (animator == null || overrideController == null) return;

        AnimatorOverrideController instanceOverride = new AnimatorOverrideController(overrideController);

        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        instanceOverride.GetOverrides(overrides);

        for (int i = 0; i < overrides.Count; i++)
        {
            string keyName = overrides[i].Key.name;
            if (keyName == "Walk")
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, walkClip);
            else if (keyName == "Attack")
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, attackClip);
            else if (keyName == "Die")
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, dieClip);
            else if (keyName == "Idle")
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, idleClip);
            else if (keyName == "Collect")
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, collectClip);
            else if (keyName == "Build")
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, buildClip);
        }

        instanceOverride.ApplyOverrides(overrides);
        animator.runtimeAnimatorController = instanceOverride;
    }
} 
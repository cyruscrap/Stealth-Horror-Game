using UnityEngine;

public class close_mouth : MonoBehaviour
{
    private Animator animator;
    private Vector3 eulerzangle;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        eulerzangle = animator.GetBoneTransform(HumanBodyBones.Jaw).transform.localEulerAngles;

        //euler = Quaternion.Euler(eulerzangle);
        Debug.Log(eulerzangle);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        animator.GetBoneTransform(HumanBodyBones.Jaw).localRotation = Quaternion.Euler(eulerzangle);
    }
}
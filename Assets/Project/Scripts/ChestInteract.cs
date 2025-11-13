using UnityEngine;

public class ChestOpen : MonoBehaviour
{
    public Transform lid; // Assign the actual lid (or LidPivot)
    public float openAngle = -100f;
    public float speed = 2f;
    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        closedRotation = lid.localRotation;
        openRotation = Quaternion.Euler(openAngle, 0, 0) * closedRotation;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            StopAllCoroutines();
            StartCoroutine(OpenClose());
        }
    }

    System.Collections.IEnumerator OpenClose()
    {
        float t = 0;
        Quaternion start = lid.localRotation;
        Quaternion end = isOpen ? closedRotation : openRotation;

        while (t < 1)
        {
            t += Time.deltaTime * speed;
            lid.localRotation = Quaternion.Slerp(start, end, t);
            yield return null;
        }

        isOpen = !isOpen;
    }
}

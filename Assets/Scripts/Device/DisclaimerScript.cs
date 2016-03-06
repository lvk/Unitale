using UnityEngine;

/// <summary>
/// Attached to the disclaimer screen so you can skip it.
/// </summary>
public class DisclaimerScript : MonoBehaviour
{
    /// <summary>
    /// Checks if you pressed one of the things the disclaimer tells you to. It's pretty straightforward.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            Application.LoadLevel("ModSelect");
    }
}
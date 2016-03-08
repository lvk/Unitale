﻿using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller for all the lifebars in the game. To be used with the HPBar prefab.
/// </summary>
public class LifeBarController : MonoBehaviour
{
    public Color fillColor;
    public Color backgroundColor;
    public Image fill;
    public Image background;

    private float currentFill = 1.0f;
    private float oldFill = 1.0f;
    private float desiredFill = 1.0f;
    private float fillLinearTime = 1.0f; // how many seconds does it take to go from current healthbar position to new healthbar position
    private float fillTimer = 0.0f;

    /// <summary>
    /// Change the colours of the healthbar's images accordingly.
    /// </summary>
    private void Start()
    {
        background.color = backgroundColor;
        fill.color = fillColor;
        // ensure proper layering because tinkering with the prefab screws it up
        background.transform.SetAsLastSibling(); 
        fill.transform.SetAsLastSibling();
        transform.localScale=Vector3.one;
    }

    /// <summary>
    /// Immediately set the healthbar's fill to this value.
    /// </summary>
    /// <param name="fillvalue">Healthbar fill in range of [0.0, 1.0].</param>
    public void setInstant(float fillvalue)
    {
        currentFill = fillvalue;
        desiredFill = fillvalue;
        fill.fillAmount = fillvalue;
    }

    /// <summary>
    /// Start a linear-time transition from current fill to this value.
    /// </summary>
    /// <param name="fillvalue">Healthbar fill in range of [0.0, 1.0].</param>
    public void setLerp(float fillvalue)
    {
        oldFill = currentFill;
        desiredFill = fillvalue;
        fillTimer = 0.0f;
    }

    /// <summary>
    /// Start a linear-time transition from first value to second value.
    /// </summary>
    /// <param name="originalValue">Value to start the healthbar at, in range of [0.0, 1.0].</param>
    /// <param name="fillValue">Value the healthbar should be at when finished, in range of [0.0, 1.0].</param>
    public void setLerp(float originalValue, float fillValue)
    {
        setInstant(originalValue);
        setLerp(fillValue);
    }

    /// <summary>
    /// Set the fill color of this healthbar.
    /// </summary>
    /// <param name="c">Color for present health.</param>
    public void setFillColor(Color c)
    {
        fillColor = c;
        fill.color = c;
    }

    /// <summary>
    /// Set the background color of this healthbar.
    /// </summary>
    /// <param name="c">Color for missing health.</param>
    public void setBackgroundColor(Color c)
    {
        backgroundColor = c;
        background.color = c;
    }

    /// <summary>
    /// Sets visibility for the image components of the healthbar.
    /// </summary>
    /// <param name="visible">True for visible, false for hidden.</param>
    public void setVisible(bool visible)
    {
        foreach (Image img in GetComponentsInChildren<Image>())
        {
            img.enabled = visible;
        }
    }

    /// <summary>
    /// Takes care of moving the healthbar to its intended position.
    /// </summary>
    private void Update()
    {
        if (currentFill == desiredFill)
            return;

        currentFill = Mathf.Lerp(oldFill, desiredFill, fillTimer / fillLinearTime);
        fill.fillAmount = currentFill;
        fillTimer += Time.deltaTime;
    }
}
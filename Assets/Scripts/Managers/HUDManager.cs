using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private TMP_Text toolTip;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
		toolTip.gameObject.SetActive(false);
	}

	public void SetTooltip(string text)
	{
		toolTip.gameObject.SetActive(true);
		toolTip.text = text;
	}

    public void HideToolTip()
    {
        toolTip.gameObject.SetActive(false);
	}
}

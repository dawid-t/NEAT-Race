using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class NodesConnectionUIListener : MonoBehaviour, IPointerEnterHandler
{
	private static GameObject lastShowedNodesConnection, lastShowedNodesConnectionInfo;


	public void OnPointerEnter(PointerEventData eventData)
	{
		DeactivateLastShowedInfo();

		GameObject go = eventData.pointerCurrentRaycast.gameObject;
		go.transform.SetSiblingIndex(go.transform.parent.childCount);
		go.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 2);

		TextMeshProUGUI text = go.GetComponentInChildren<TextMeshProUGUI>(true);
		text.gameObject.SetActive(true);
		text.transform.parent = GenomeUI.Instance.GenomeDataPanelNodesConnectionInfo;

		lastShowedNodesConnection = go;
		lastShowedNodesConnectionInfo = text.gameObject;
	}

	public static void DeactivateLastShowedInfo()
	{
		if(lastShowedNodesConnection != null)
		{
			lastShowedNodesConnectionInfo.transform.parent = lastShowedNodesConnection.transform;
			lastShowedNodesConnectionInfo.SetActive(false);

			lastShowedNodesConnection.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1);
		}
	}
}
